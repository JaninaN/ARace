using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointsController : MonoBehaviour {

    public int points { get; private set; }

	// Use this for initialization
	void Start () {
        points = 0;
	}
	
	public void AddPoints(int amount)
    {
        points += amount;
        GetComponent<Text>().text = points.ToString();
    }
}
