using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour {
    
    [SerializeField] private Material modifiableColorMaterial;

    private List<Color> m_colors = new List<Color>()
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.white,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        new Color(0,0.5f,0), //dark green
        new Color(1,0.5f,0), //orange
        new Color(0.5f,0,0), //dark red
        new Color(0.5f,0,1)  //purple
    };

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "CarBottom")
        {
            // Dye Parts of the Car in random Color
            GameObject[] carParts = GameObject.FindGameObjectsWithTag("CarColor");
            modifiableColorMaterial.SetColor("_Color", getRandomColor());
            foreach(GameObject part in carParts)
            {
                MeshRenderer mr = part.GetComponent<MeshRenderer>();
                mr.material = modifiableColorMaterial;
            }
            
        }
    }

    private Color getRandomColor()
    {
        return m_colors[Random.Range(0, m_colors.Count)];
    }
}
