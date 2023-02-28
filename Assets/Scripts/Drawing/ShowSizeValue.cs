using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRDrawable;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class ShowSizeValue : MonoBehaviour
{

    const int MIN_SIZE = 1;

    int fontSize;

    [SerializeField]
    private Drawable _drawable;

    [SerializeField]
    private int maxSize = 15;

    [SerializeField]
    private TextMeshPro textDisplay;

    // Start is called before the first frame update
    void Start()
    {

        fontSize = 3;
        SetText(fontSize);

    }

    void UpdateValue(SliderEventData eventData)
    {

        float val = eventData.NewValue * maxSize;

        fontSize = (int)val;

        SetText(fontSize);
        SetFont(fontSize);

    }


    void SetText(int value)
    {

        if(textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshPro>();
        }

        if(textDisplay != null)
        {
            textDisplay.text = $"{value}";
        }

    }

    
    void SetFont(int value)
    {

        if(_drawable != null)
        {

            _drawable.SetFontSize(value);

        }

    }

}
