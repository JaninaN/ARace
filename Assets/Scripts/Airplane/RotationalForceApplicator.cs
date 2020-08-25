using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalForceApplicator : ForceApplicator {

    public float PercentageOfMaxRPM
    {
        get
        {
            return m_maxAngularVelocity / m_rigidbody.maxAngularVelocity;
        }
    }

    private float m_maxAngularVelocity;

    public RotationalForceApplicator(Rigidbody new_rigidbody, Vector3 new_forceAxis, float new_MaxForce, float maxRPM) : base(new_rigidbody, new_forceAxis, new_MaxForce)
    {
        // This converts RPM to degrees/second, and then to rads/second.
        m_maxAngularVelocity = ((maxRPM / 60F) * 360F) / 57.29577951308F;
    }

    public override void ApplyForcePercentage(float percentage)
    {
        m_rigidbody.AddRelativeTorque(CalculateForce(percentage));
    }

}
