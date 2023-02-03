using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

public class ColorVisualizer : MonoBehaviour
{


    private MeshRenderer colorRenderer;

    private Color currColor = Color.gray;


    // Start is called before the first frame update
    void Start()
    {

        colorRenderer = GetComponent<MeshRenderer>();
        UpdateMeshColor();

    }

    // Update is called once per frame
    void UpdateMeshColor()
    {
        Debug.Log("Updating Mesh");
        colorRenderer.material.SetColor("_Color", currColor);

    }


    public void SetRedValue(SliderEventData eventData)
    {

        currColor.r = eventData.NewValue;
        UpdateMeshColor();
    }

    public void SetGreenValue(SliderEventData eventData)
    {

        currColor.g = eventData.NewValue;
        UpdateMeshColor();
    }


    public void SetBlueValue(SliderEventData eventData)
    {

        currColor.b = eventData.NewValue;
        UpdateMeshColor();

    }


}
