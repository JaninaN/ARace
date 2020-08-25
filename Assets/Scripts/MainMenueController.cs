using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Vehicle
{
    Car,
    Airplane
}

public class MainMenueController : MonoBehaviour {

    private Vehicle vehicle;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(GameObject.Find("EventSystem"));
        vehicle = Vehicle.Car;
	}
	
    public void StartGame()
    {
        switch (vehicle)
        {
            case Vehicle.Car:
                SceneManager.LoadScene(1);
                break;
            case Vehicle.Airplane:
                SceneManager.LoadScene(2);
                break;
        }
        
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void CarChosen(bool value)
    {
        vehicle = Vehicle.Car;
    }

    public void AirplaneChosen(bool value)
    {
        vehicle = Vehicle.Airplane;
    }
}
