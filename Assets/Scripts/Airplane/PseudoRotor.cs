using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoRotor{

    private Transform m_transform;
    private Vector3 rotationAxis;
    private float maxDegreesPerSecond;

    public PseudoRotor(Transform new_transform, Vector3 new_rotationAxis, float maxRPM)
    {
        m_transform = new_transform;
        rotationAxis = new_rotationAxis;
        maxDegreesPerSecond = (maxRPM / 60f) * 360f;
    }

    public void Rotate(float percentageOfMaxSpeed)
    {
        m_transform.Rotate(rotationAxis * ((maxDegreesPerSecond * percentageOfMaxSpeed) * Time.deltaTime));
    }

}
