using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class FontBallRespawnHandler : Solver
{

    public Rigidbody rigidbody;
    private Vector3 ReferencePosition => SolverHandler.TransformTarget != null ? SolverHandler.TransformTarget.position : Vector3.zero;
    private Quaternion ReferenceRotation => SolverHandler.TransformTarget != null ? SolverHandler.TransformTarget.rotation : Quaternion.identity;

    // Update is called once per frame
    public override void SolverUpdate()
    {

        float dist = Vector3.Distance(transform.position, ReferencePosition);

        if(dist >= 20f)
        {
            ResetBallHandMenu();
            
        }

    }

    public void ResetBallHandMenu()
    {

        Vector3 currentPosition = ReferencePosition;
        Vector3 refForward = ReferenceRotation * Vector3.forward;

        Vector3 wantedPosition = currentPosition + refForward;

        transform.position = wantedPosition;
        rigidbody.velocity = new Vector3(0, 0, 0);
    }



}
