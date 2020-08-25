using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearForceApplicator : ForceApplicator {
    
    public LinearForceApplicator(Rigidbody new_rigidbody, Vector3 new_forceAxis, float new_MaxForce) : base(new_rigidbody, new_forceAxis, new_MaxForce)
    {

    }

    public override void ApplyForcePercentage(float percentage)
    {
        m_rigidbody.AddRelativeForce(CalculateForce(percentage));
    }

}
