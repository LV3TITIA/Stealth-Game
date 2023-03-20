using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;

public class IkBehaviour : MonoBehaviour
{
    public Animator _anim;

    //possible d'avoir un V3 aussi
    public Transform HeadDirection;
    public Transform RightHandPosition;

    public Cinemachine.CinemachineFreeLook fl;

    private void Awake()
    {
        TryGetComponent<Animator>(out _anim);
    }

    private void OnAnimatorIK(int layerIndex)
    {


        _anim.SetLookAtWeight(1); //diminuer le weight si la target est trop derrière
        _anim.SetLookAtPosition(HeadDirection.position);



        //_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        //_anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandPosition.position);
        //raycast depuis la position de la main
        //normale du point de collision

        //fl.m_YAxisRecentering = new AxisState.Recentering(false, 1, 2); ;
    }
}
