using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpInstructions : MonoBehaviour
{
    public GameObject instructionsButtonCanvas;

    // Update is called once per frame
    void Start()
    {
        Instantiate(instructionsButtonCanvas,instructionsButtonCanvas.transform.position, instructionsButtonCanvas.transform.rotation);

    }

    void OnClick()
    {
     
    }
}
