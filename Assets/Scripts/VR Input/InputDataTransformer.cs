using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;

public class InputDataTransformer : MonoBehaviour
{


    //Type of input wanted
    public InputSourceType inputSource;

    //Hand(right or left) wanted if detecting a hand
    public Handedness handType;

    private PhotonView photonview;


    // Start is called before the first frame update
    void Start()
    {

       photonview = PhotonView.Get(this);

    }

    // Update is called once per frame
    void Update()
    {

        Ray getInput;

        if (photonview.IsMine)
        {
            if (InputRayUtils.TryGetRay(inputSource, handType, out getInput))
            {

                transform.localPosition = getInput.origin;
                transform.localRotation = Quaternion.LookRotation(getInput.direction, Vector3.up);

            }
        }

    }
}
