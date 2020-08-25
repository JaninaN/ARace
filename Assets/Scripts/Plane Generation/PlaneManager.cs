using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneManager : MonoBehaviour {

    private enum Vehicle {Car, Airplane}

    [SerializeField] private Vehicle m_Vehicle;
    private Object[] planes;

	// Use this for initialization
	void Start () {

        //Load Plane Resources depending on chosen Vehicle
        switch (m_Vehicle)
        {
            case Vehicle.Car:
                planes = Resources.LoadAll("ModularPlanes");
                break;
            case Vehicle.Airplane:
                planes = Resources.LoadAll("ModularAirPlanes");
                break;
            default:
                planes = Resources.LoadAll("");
                break;
        }
	}

    /// <summary>
    /// Returns a random Modular Plane from "Resources" Folder
    /// </summary>
    public GameObject GetRandomPlane()
    {

        int num = Random.Range(0, (planes.Length - 1));
        GameObject randomPlane = (GameObject)Instantiate(planes[num]);
        return randomPlane;
    }
}
