using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceController : MonoBehaviour {

    private float m_Distance = 0f;
    private GameObject m_DistanceDisplay;
    private GameObject m_Player;
    private Vector3 m_LastPlayerPosition = Vector3.zero;

    private void Update()
    {
        if(m_DistanceDisplay == null || m_Player == null)
        {
            m_DistanceDisplay = GameObject.FindGameObjectWithTag("Distance");
            m_Player = GameObject.FindGameObjectWithTag("Player");
            if(m_Player != null)
            {
                m_LastPlayerPosition = m_Player.transform.position;
            }
            
        }
        else
        {
            CalculateDistance();
            m_DistanceDisplay.GetComponent<Text>().text = ConvertDistance();
        }

        

    }

    //Calculates the moved distance of the Player since last Updat and add it to the total distance.
    private void CalculateDistance()
    {
        try
        {
            float currentStep = Vector3.Distance(m_LastPlayerPosition, m_Player.transform.position);
            m_LastPlayerPosition = m_Player.transform.position;
            m_Distance += currentStep;
        }
        catch 
        {
            
        }
        
    }

    //Converts the distance to string
    private string ConvertDistance()
    {
        string distanceText;

        //Switch to Km if distance is high enough
        if(m_Distance >= 1000)
        {
            distanceText = (Mathf.RoundToInt(m_Distance / 10)/100).ToString() + " Km";
        }
        else
        {
            distanceText = (Mathf.RoundToInt((int)m_Distance)).ToString() + " m";
        }

        return distanceText;
    }

}
