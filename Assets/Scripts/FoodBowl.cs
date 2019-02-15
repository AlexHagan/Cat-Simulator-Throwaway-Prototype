using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBowl : MonoBehaviour
{
	GameObject kibble;
	
    // Start is called before the first frame update
    void Start()
    {
        kibble = GameObject.Find("Cat Food");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnMouseDown()
	{
		kibble.SetActive(true);
	}
}
