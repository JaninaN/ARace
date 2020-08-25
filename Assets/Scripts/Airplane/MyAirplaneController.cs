using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAirplaneController : MonoBehaviour{

    [Header("Components")]
    public Rigidbody mainRotor;
    public Rigidbody body;
    public Transform tailRotor;

    [Header("Values")]
    public float liftMaxForce = 1000062;
    public float mainRotorMaxForce = 3;
    public float mainRotorMaxRPM = 240;
    public float bodyMaxForce = 25000;
    public float bodyMaxRPM = 120;
    public float bodyCounterMaxForce = 50000;
    public float bodyCounterMaxRPM = 120;
    public float pitchMaxForce = 20000;
    public float pitchMaxRPM = 100;
    public float rollMaxForce = 15000;
    public float rollMaxRPM = 100;
    public float tailRotorMaxRPM = 500;
    public float maxTiltAngle = 45;

    private LinearForceApplicator lift;
    private RotationalForceApplicator mainRotorTorque;
    private RotationalForceApplicator bodyTorque;
    private RotationalForceApplicator bodyCounterTorque;
    private RotationalForceApplicator pitchTorque;
    private RotationalForceApplicator rollTorque;
    private PseudoRotor tailRotorRotater;

    // Use this for initialization
    void Start () {

        lift = new LinearForceApplicator(body, Vector3.up, liftMaxForce);
        mainRotorTorque = new RotationalForceApplicator(mainRotor, Vector3.down, mainRotorMaxForce, mainRotorMaxRPM);
        bodyTorque = new RotationalForceApplicator(body, Vector3.down, bodyMaxForce, bodyMaxRPM);
        bodyCounterTorque = new RotationalForceApplicator(body, Vector3.up, bodyCounterMaxForce, bodyCounterMaxRPM);
        pitchTorque = new RotationalForceApplicator(body, Vector3.right, pitchMaxForce, pitchMaxRPM);
        rollTorque = new RotationalForceApplicator(body, Vector3.back, rollMaxForce, rollMaxRPM);
        tailRotorRotater = new PseudoRotor(tailRotor, Vector3.right, tailRotorMaxRPM);
        
        // Convert RPM to rads/second an set as max RPM 
        mainRotor.maxAngularVelocity = body.maxAngularVelocity = ((mainRotorMaxRPM / 60F) * 360F) / 57.29577951308F;

        // This should account for any weird wobbling caused by misaligned pivots.
        body.centerOfMass = Vector3.zero;
    }
    

    public void Move(float collective_height, float pedals_rotation, float cyclic_forward, float cyclic_sideway, float throttle)
    {

        mainRotorTorque.ApplyForcePercentage(throttle);
        bodyTorque.ApplyForcePercentage(mainRotorTorque.PercentageOfMaxRPM);

        tailRotorRotater.Rotate(mainRotorTorque.PercentageOfMaxRPM);
        bodyCounterTorque.ApplyForcePercentage(bodyCounterTorque.PercentageOfMaxRPM * pedals_rotation);

        lift.ApplyForcePercentage(mainRotorTorque.PercentageOfMaxRPM * collective_height);

        // Check tild angle and stop adding force if it´s reached to prevent body from tipping over
        bool forwardForceDenied = cyclic_forward > 0 && Vector3.Angle(body.transform.forward, new Vector3(body.transform.forward.x, 0f, body.transform.forward.z)) >= maxTiltAngle && body.transform.forward.y < 0;
        bool backwardForceDenied = cyclic_forward < 0 && Vector3.Angle(body.transform.forward, new Vector3(body.transform.forward.x, 0f, body.transform.forward.z)) >= maxTiltAngle && body.transform.forward.y > 0;

        bool rightForceDenied = cyclic_sideway > 0 && Vector3.Angle(body.transform.right, new Vector3(body.transform.right.x, 0f, body.transform.right.z)) >= maxTiltAngle && body.transform.right.y < 0;
        bool leftForceDenied = cyclic_sideway < 0 && Vector3.Angle(body.transform.right, new Vector3(body.transform.right.x, 0f, body.transform.right.z)) >= maxTiltAngle && body.transform.right.y > 0;

        if (!forwardForceDenied && !backwardForceDenied)
        {
            pitchTorque.ApplyForcePercentage(pitchTorque.PercentageOfMaxRPM * cyclic_forward);
        }

        if(!rightForceDenied && !leftForceDenied)
        {
            rollTorque.ApplyForcePercentage(rollTorque.PercentageOfMaxRPM * cyclic_sideway);
        }
        
        // Rotate body back horizontal if there is no User input
        Vector3 Right = body.transform.right;
        Vector3 Forward = body.transform.forward;

        Vector3 ProjectedRightVector = Vector3.ProjectOnPlane(Right, Vector3.Cross(Vector3.forward, Vector3.right));
        Vector3 ProjectedForwardVector= Vector3.ProjectOnPlane(Forward, Vector3.Cross(Vector3.forward, Vector3.right));
        

        // Check if Airplane still rolls to the Side without any roll input and correct body angle
        if (cyclic_sideway == 0f && ProjectedRightVector.magnitude != 1f) 
        {
            
            Vector3 horizontalRight = new Vector3(Right.x, 0f, Right.z).normalized;
            float rollAngle = Vector3.Angle(Right, horizontalRight);
            float minRollBackAngle = (rollAngle / 5f > 3f ? rollAngle / 5f : rollAngle / 10f > 3f ? rollAngle / 10f : rollAngle / 50f > 3f ? rollAngle / 50f : 3f);
            Quaternion deltaRotation;

            // Calculate angle to rotate back, angle is bigger if the way back is longer: from rollAngle (biggest) over minRollBackAngle to exactly calculated rest angle (smallest)
            // Makes the rotation smooth
            if (Time.deltaTime * minRollBackAngle > rollAngle)
            {
                rollAngle /= Time.deltaTime;
            }
            else if (rollAngle < minRollBackAngle)
            {
                rollAngle = minRollBackAngle;
            }
            
            // Find rotation direction
            if (Right.y > horizontalRight.y)
            {
                deltaRotation = Quaternion.Euler(new Vector3(0f, 0f, -rollAngle) * Time.deltaTime);
            }
            else
            {
                deltaRotation = Quaternion.Euler(new Vector3(0f, 0f, rollAngle) * Time.deltaTime);
            }
            
            body.MoveRotation(body.rotation * deltaRotation);
        }

        // Check if Airplane still pitches forward/backward without any pitch input and correct body angle
        if (cyclic_forward == 0f && ProjectedForwardVector.magnitude != 1f)
        {
            
            Vector3 horizontalFwd = new Vector3(Forward.x, 0f, Forward.z).normalized;
            float pitchAngle = Vector3.Angle(Forward, horizontalFwd);
            float minPitchBackAngle = (pitchAngle / 5f > 3f ? pitchAngle / 5f : pitchAngle / 10f > 3f ? pitchAngle / 10f : pitchAngle / 50f > 3f ? pitchAngle / 50f : 3f);
            Quaternion deltaRotation;

            // Calculate angle to rotate back, angle is bigger if the way back is longer: from pitchAngle (biggest) over minPitchBackAngle to exactly calculated rest angle (smallest)
            // Makes the rotation smooth
            if (Time.deltaTime * minPitchBackAngle > pitchAngle)
            {
                pitchAngle /= Time.deltaTime;
            }
            else if (pitchAngle < minPitchBackAngle)
            {
                pitchAngle = minPitchBackAngle;
            }

            // Find rotation direction
            if (Forward.y < horizontalFwd.y)
            {
                deltaRotation = Quaternion.Euler(new Vector3(-pitchAngle, 0f, 0f) * Time.deltaTime);
            }
            else
            {
                deltaRotation = Quaternion.Euler(new Vector3(pitchAngle, 0f, 0f) * Time.deltaTime);
            }

            body.MoveRotation(body.rotation * deltaRotation);
        }

        // Stopping force in roll/pitch direction

        // Current velocity of the body represented in local Coordinate system
        Vector3 localBodyVelocity = body.transform.InverseTransformVector(body.velocity);

        if (cyclic_sideway == 0f && localBodyVelocity.x != 0f)
        {
            rollTorque.ApplyForcePercentage(mainRotorTorque.PercentageOfMaxRPM * (-localBodyVelocity.x / 500f));
        }
        if (cyclic_forward == 0f && localBodyVelocity.z != 0f)
        {
            pitchTorque.ApplyForcePercentage(mainRotorTorque.PercentageOfMaxRPM * (-localBodyVelocity.z / 500f));
        }
        

    }

}
