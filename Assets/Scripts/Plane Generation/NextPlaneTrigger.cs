using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextPlaneTrigger : MonoBehaviour {

    /// <summary>
    /// A simple GameObject wich represents the position for a new Modular Plane.
    /// </summary>
    public GameObject anchor;

    private bool active = true;

	// Use this for initialization
	void Start () {
		
	}

    /// <summary>
    /// Disable Trigger on this GameObject
    /// </summary>
    public void DisableTrigger()
    {
        try
        {
            this.GetComponent<MeshCollider>().isTrigger = false;
            this.GetComponent<MeshCollider>().convex = false;
            active = false;
            GetComponent<MeshCollider>().enabled = false;
        }
        catch
        {

        }
        try
        {
            active = false;
            GetComponent<BoxCollider>().enabled = false;
            gameObject.SetActive(false);
        }
        catch
        {

        }
        
    }
    
    private void OnTriggerEnter(Collider other)
    {

        //Add new Modular Plane if the Car enter this Trigger.
        if (other.tag == "CarBottom" || other.tag == "AirplaneBody")
        {
            AddNewPlane();
        }
    }

    /// <summary>
    /// Add a new Modular Plane at the Anchor´s position
    /// </summary>
    public void AddNewPlane()
    {
        this.DisableTrigger();
        GameObject controller = GameObject.Find("ARaceController");
        GameObject newPlane = controller.GetComponent<PlaneManager>().GetRandomPlane();
        newPlane.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
        newPlane.transform.parent = GameObject.FindGameObjectWithTag("StartPlane").transform;
    }

    public bool IsActive()
    {
        return active;
    }
}
