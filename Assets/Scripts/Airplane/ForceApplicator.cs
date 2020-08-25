using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceApplicator{

    protected Rigidbody m_rigidbody;
    protected Vector3 m_forceAxis;

    public float MaxForce
    {
        get;
        protected set;
    }

    public ForceApplicator(Rigidbody new_rigidbody, Vector3 new_forceAxis, float new_MaxForce)
    {
        m_rigidbody = new_rigidbody;
        m_forceAxis = new_forceAxis;
        MaxForce = new_MaxForce;
    }

    public virtual void ApplyForcePercentage(float percentage)
    {

    }

    protected Vector3 CalculateForce(float percentageOfMaxForce)
    {
        return m_forceAxis * (MaxForce * percentageOfMaxForce);
    }

}
