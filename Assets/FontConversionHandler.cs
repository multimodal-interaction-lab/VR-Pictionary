using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using FreeDraw;

public class FontConversionHandler : MonoBehaviour
{

    public Drawable drawable;

    public void SetPenFontFromBall()
    {

        Vector3 currentScale = transform.localScale;
        Debug.Log("Untransformed size: " + currentScale.x);
        int newPenSize = (int)(currentScale.x * 30);
        Debug.Log("New size: " + newPenSize);
        drawable.SetFontSize(newPenSize);

    }
    

}
