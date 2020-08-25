using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class LoopinGravity : MonoBehaviour {

    public bool lowSpeedGravity = true;
    public float thrust;

    private bool looping;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        if (looping)
        {
            manipulateCar();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "CarBottom")
        {
            looping = true;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "CarBottom")
        {
            looping = false;
            Rigidbody rBody = other.gameObject.GetComponentInParent<Rigidbody>();
            rBody.useGravity = true;
        }
    }

    private void manipulateCar()
    {
        
        GameObject car = GameObject.FindGameObjectWithTag("Player");
        Rigidbody rBody = car.GetComponent<Rigidbody>();
        CarController carControl = car.GetComponent<CarController>();
        Vector3 gForce = -car.transform.up;
        float currSpeed = carControl.CurrentSpeed;
        rBody.useGravity = false;

        //let the Car drop if hasn´t enough speed 
        if(lowSpeedGravity && currSpeed < 1.5f)
        {
            gForce *= currSpeed;

            if(currSpeed < 0.8f)
            {
                rBody.useGravity = true;
            }
            else
            {
                gForce += -transform.parent.up * (400f * (1f - currSpeed));
            }
        }

        
        rBody.AddForce(gForce * thrust);
        Debug.Log("gForce: " + gForce);
    }
}
