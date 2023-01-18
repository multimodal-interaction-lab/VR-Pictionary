using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class MRTKManipulationToggler : MonoBehaviour
{

    public Behaviour objectManipulator;
    
    /*
    void Awake()
    {

        objectManipulator = gameObject.GetComponent<ObjectManipulator>();
        if(objectManipulator == null)
        {
            Debug.Log("Not working");
        }
        else
        {
            Debug.Log("Works!");
        }

    }
    */
    public void ToggleObjectManip()
    {

        if (objectManipulator.enabled)
        {
            objectManipulator.enabled = false;
            
        }
        else
        {
            objectManipulator.enabled = true;
            
        }
        
    }

}

    