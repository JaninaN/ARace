using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCheckpointController : MonoBehaviour {

    /// <summary>
    /// The parent GameObject of the Checkpoint, used to destroy the whole Checkpoint
    /// </summary>
    public GameObject CheckpointRoot;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "AirplaneBody" || other.tag == "CarBottom")
        {
            GameObject pointsDisplay = GameObject.FindGameObjectWithTag("Points");
            pointsDisplay.GetComponent<PointsController>().AddPoints(1);
            Destroy(CheckpointRoot);
        }
        
    }
}
