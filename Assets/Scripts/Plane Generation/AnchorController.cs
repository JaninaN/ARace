using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorController : MonoBehaviour {
    

    private void OnTriggerEnter(Collider other)
    {

        //Disable next Plane Trigger if a Modular Plane alredy exists on this side
        if (other.tag == "ModularPlane")
        {
            NextPlaneTrigger npt = transform.parent.gameObject.GetComponent<NextPlaneTrigger>();
            npt.DisableTrigger();
        }
    }
}
