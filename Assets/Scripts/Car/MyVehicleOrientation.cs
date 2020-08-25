using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyVehicleOrientation : MonoBehaviour {

    private Camera firstPersonCamera;
    private float cooldown = 3f;
    private float searchRadius = 250f;

    private void Start()
    {
        
        Camera[] cams = new Camera[2];
        Camera.GetAllCameras(cams);
        firstPersonCamera = cams[0];

        GameObject resetBtn = GameObject.FindGameObjectWithTag("ResetVehicleButton");
        Button btn = resetBtn.GetComponent<Button>();
        btn.onClick.AddListener(BringBackVehicle);
    }
    
    // Update is called once per frame
    void Update () {

        //Fix fliped over Vehicle 
        float vehicleAngle = Vector3.Angle(Vector3.up, transform.up);

        if(Mathf.Abs(vehicleAngle) > 45f)
        {

            cooldown -= Time.deltaTime;
            if(cooldown < 0)
            {

                //Repositioning Vehicle above Ground and in correct Orientation
                Vector3 currentAngle = transform.rotation.eulerAngles;
                Vector3 invertedXZangle = new Vector3(-1f * currentAngle.x, currentAngle.y, -1f * currentAngle.z);

                transform.position += Vector3.up * 0.05f;
                transform.Rotate(invertedXZangle, Space.World);

                cooldown = 3f;

            }

        }
        else
        {

            cooldown = 3f;

        }

	}

    /// <summary>
    /// Repositioning the Vehicle in front of the Player
    /// </summary>
    public void BringBackVehicle()
    {

        //First check if AR Core is tracking the Anchor of the Vehicle, Vehicle will be inactive if not
        if (!gameObject.activeSelf)
        {
            return;
        }

        //Check if there is a Plane to set the Vehicle in front of the Player
        Vector3 newVehiclePos = firstPersonCamera.transform.position + firstPersonCamera.transform.forward;
        newVehiclePos = new Vector3(newVehiclePos.x, 0.05f, newVehiclePos.z);
        GameObject testSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        testSphere.transform.position = newVehiclePos;
        Collider[] collisions = Physics.OverlapSphere(testSphere.transform.position, testSphere.transform.localScale.x / 2f);
        
        foreach (Collider col in collisions)
        {
            if (col.gameObject.tag == "ModularPlane" || col.gameObject.tag == "StartPlane")
            {
                transform.position = newVehiclePos;
                Destroy(testSphere);
                return;
            }
        }

        //Search for nearby Planes by increasing the Sphere scale 
        while (true)
        {
            
            testSphere.transform.localScale = new Vector3(testSphere.transform.localScale.x + 20f, testSphere.transform.localScale.y + 20f, testSphere.transform.localScale.z + 20f);
            collisions = Physics.OverlapSphere(testSphere.transform.position, testSphere.transform.localScale.x / 2f);

            foreach (Collider col in collisions)
            {
                if (col.gameObject.tag == "ModularPlane" || col.gameObject.tag == "StartPlane")
                {

                    GameObject nearestPlane = col.gameObject;

                    //Activate triggers on the Way from the nearest Plane to the Player
                    Vector3 way = (new Vector3(firstPersonCamera.transform.position.x, nearestPlane.transform.position.y, firstPersonCamera.transform.position.z)) - nearestPlane.transform.position;
                    RaycastHit[] hits = Physics.RaycastAll(nearestPlane.transform.position, way, way.magnitude);
                    NextPlaneTrigger npt;

                    for (int i = 0; i < hits.Length; i++)
                    {
                        npt = hits[i].collider.gameObject.GetComponent<NextPlaneTrigger>();
                        if(npt != null && npt.IsActive())
                        {
                            npt.AddNewPlane();
                            hits = Physics.RaycastAll(nearestPlane.transform.position, way, way.magnitude);
                        }
                    }

                    transform.position = newVehiclePos;
                    Destroy(testSphere);
                    goto VehicleIsBack;
                }
            }

            //Abort searching if radius is too big 
            //Delete all Planes and start with a new one
            if(testSphere.transform.localScale.magnitude > searchRadius)
            {
                GameObject oldStartPlane = GameObject.FindGameObjectWithTag("StartPlane");
                Transform anchor = oldStartPlane.transform.parent;
                Destroy(oldStartPlane);
                GameObject newStartPlane = GameObject.Find("ARaceController").GetComponent<SceneController>().StartPlanePrefab;
                newStartPlane = Instantiate(newStartPlane);
                newStartPlane.transform.SetPositionAndRotation(new Vector3(firstPersonCamera.transform.position.x, 0f, firstPersonCamera.transform.position.z), firstPersonCamera.transform.rotation);
                newStartPlane.transform.parent = anchor;
                transform.position = newVehiclePos;
                Destroy(testSphere);
                break;
            }

        }

    VehicleIsBack:;

    }
    
}
