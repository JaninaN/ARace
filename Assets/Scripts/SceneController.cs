using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;
using System.Linq;

#if UNITY_EDITOR
// NOTE:
// - InstantPreviewInput does not support `deltaPosition`.
// - InstantPreviewInput does not support input from
//   multiple simultaneous screen touches.
// - InstantPreviewInput might miss frames. A steady stream
//   of touch events across frames while holding your finger
//   on the screen is not guaranteed.
// - InstantPreviewInput does not generate Unity UI event system
//   events from device touches. Use mouse/keyboard in the editor
//   instead.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class SceneController : MonoBehaviour {

    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
    /// </summary>
    public Camera FirstPersonCamera;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject DetectedPlanePrefab;

    /// <summary>
    /// A model to place if the User touch the AR tracking Planes.
    /// Representing a Vehicle
    /// </summary>
    public GameObject VehiclePrefab;

    /// <summary>
    /// A Plane to place if the User touch the AR tracking Planes.
    /// </summary>
    public GameObject StartPlanePrefab;

    /// <summary>
    /// A game object parenting UI for displaying the "searching for planes" Text.
    /// </summary>
    public GameObject SearchingForPlaneUI;

    /// <summary>
    /// A game object parenting UI for displaying User controller on the left.
    /// </summary>
    public GameObject LeftControllerUI;

    /// <summary>
    /// A game object parenting UI for displaying User controller on the right.
    /// </summary>
    public GameObject RightControllerUI;

    /// <summary>
    /// Whether or not the Plane schould be updated occasionally to the current Floor in the real World.
    /// </summary>
    public bool UpdateFloor;

    /// <summary>
    /// The minimum Distance to the last anchored Floor to start rechecking the Position of the real World´s Floor.
    /// Possibly correcting Plane positioning.
    /// </summary>
    public float recheckFloorDistance;

    /// <summary>
    /// The rotation in degrees need to apply to model when the model is placed.
    /// </summary>
    private const float k_ModelRotation = 0.0f;

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    /// <summary>
    /// True if the Vehicle has been placed in the World, otherwise false.
    /// </summary>
    private bool m_IsVehiclePlaced = false;

    /// <summary>
    /// The UI shows up an Information text for the Player as long as the Vehicle is not placed.
    /// </summary>
    private bool m_showInformationUI = true;

    /// <summary>
    /// A List to hold all created Anchors. Is used to manage and destroy them.
    /// </summary>
    private List<Anchor> planeAnchors = new List<Anchor>();

    // Update is called once per frame
    void Update () {
        
        _UpdateApplicationLifecycle();

        // Hide SearchingText in UI when currently tracking at least one plane.
        Session.GetTrackables<DetectedPlane>(m_AllPlanes);
        
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                SearchingForPlaneUI.GetComponent<Text>().text = "Tippe auf das Gitter um \bdein Fahrzeug zu platzieren.";
                break;
            }
        }

        SearchingForPlaneUI.SetActive(m_showInformationUI);

        // If the player has not touched the screen and the Vehicle is not placed, we are done with this update.
        if ((Input.touchCount < 1 || (Input.GetTouch(0)).phase != TouchPhase.Began) && !m_IsVehiclePlaced)
        {
            return;
        }

        if (!m_IsVehiclePlaced)
        {
            SetupVehicle();
        }

        //Check if ARCore is still tracking, restart from plane searching if not
        if(planeAnchors.Count > 0 && planeAnchors.Last().TrackingState == TrackingState.Stopped && GameObject.FindGameObjectWithTag("Player") == null)
        {
            ReplaceVehicle();
            return;
        }

        //Try to clip the Floor to a new Plane and Anchor if the last Anchor ist too far away
        bool isAnchorTooFarAway = (FirstPersonCamera.transform.position - planeAnchors.Last().gameObject.transform.position).magnitude > recheckFloorDistance;
        if(isAnchorTooFarAway && UpdateFloor)
        {
            ClipNewFloor();
        }

    }

    /// <summary>
    /// Creates the Startplane and the Vehicle 
    /// </summary>
    private void SetupVehicle()
    {
        Touch currentTouch;
        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;
        currentTouch = Input.GetTouch(0);

        if (Frame.Raycast(currentTouch.position.x, currentTouch.position.y, raycastFilter, out hit))
        {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                // Instantiate Vehicle model and Startplane at the hit pose.
                var PlaneObject = Instantiate(StartPlanePrefab, hit.Pose.position, hit.Pose.rotation);
                var VehicleObject = Instantiate(VehiclePrefab, hit.Pose.position + VehiclePrefab.transform.position, hit.Pose.rotation);


                // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                VehicleObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);
                PlaneObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // Make Car Model and StartPlane a child of the anchor.
                PlaneObject.transform.parent = anchor.transform;
                VehicleObject.transform.parent = anchor.transform;

                planeAnchors.Add(anchor);

                m_IsVehiclePlaced = true;
                m_showInformationUI = false;

                LeftControllerUI.SetActive(true);
                RightControllerUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Clip Plane and Content to new Anchor
    /// </summary>
    private void ClipNewFloor()
    {
        UpdateFloor = false;

        for (int i = m_AllPlanes.Count - 1; i > 0; i--)
        {

            DetectedPlane currentPlane = m_AllPlanes[i];
            List<Vector3> polygonPoints = new List<Vector3>();
            currentPlane.GetBoundaryPolygon(polygonPoints);

            if (currentPlane.TrackingState == TrackingState.Tracking && currentPlane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing && polygonPoints.Count > 5)
            {
                // Add a new Anchor
                var anchor = currentPlane.CreateAnchor(currentPlane.CenterPose);
                planeAnchors.Add(anchor);

                // Take all GameObjects from the previous Anchor to the new one
                Transform[] trans = planeAnchors[planeAnchors.Count - 2].GetComponentsInChildren<Transform>();
                foreach (Transform t in trans.Skip(1))
                {
                    // Make sure the Transform t is a direct child of the previous Anchor
                    if(t.parent.Equals(planeAnchors[planeAnchors.Count - 2].transform))
                    {
                        t.parent = anchor.transform;
                        Vector3 pos = new Vector3(t.position.x, currentPlane.CenterPose.position.y, t.position.z);
                        t.position = pos;

                    }
                }

                // Destroy old Anchor 
                if(planeAnchors[0].GetComponentsInChildren<Transform>().Length < 2)
                {
                    planeAnchors[0].gameObject.transform.parent = null;
                    Destroy(planeAnchors[0].gameObject);
                    planeAnchors.RemoveAt(0);
                }
                
                break;

            }
        }

        UpdateFloor = true;
    }

    /// <summary>
    /// Replace Vehicle if AR Core tracking got lost, starting with plane searching 
    /// </summary>
    public void ReplaceVehicle()
    {
        // Only used if there is no Vehicle in the Scene
        if(GameObject.FindGameObjectWithTag("Player") == null || !GameObject.FindGameObjectWithTag("Player").activeSelf)
        {

            foreach (Anchor anchor in planeAnchors)
            {
                try
                {
                    anchor.gameObject.transform.parent = null;
                    Destroy(anchor.gameObject);
                    Debug.Log("anchor destroyed: " + anchor);
                    
                }
                catch
                {

                }
            }
            planeAnchors.Clear();

            // Reset values to restart from plane searching
            SearchingForPlaneUI.GetComponent<Text>().text = "▲\b◄   Schau dich weiter am Boden um.   ►\b▼";

            m_IsVehiclePlaced = false;
            m_showInformationUI = true;

            LeftControllerUI.SetActive(false);
            RightControllerUI.SetActive(false);
            
        }

    }


    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
