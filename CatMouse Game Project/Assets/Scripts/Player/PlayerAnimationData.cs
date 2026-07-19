using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAnimationData
{
    [SerializeField] private string MoveParameterName = "Move"; 
    [SerializeField] private string RunParameterName = "Run";

    [SerializeField] private string BaseAttackParameterName = "BaseAttack"; 
    [SerializeField] private string FindingParameterName = "Finding";
    [SerializeField] private string HitParameterName = "Hit"; 

    public int MoveParameterHash { get; private set; }
    public int RunParameterHash { get; private set; }
    public int BaseAttackParameterHash { get; private set;}
    public int FindingParameterHash {get; private set;}
    public int HitParameterHash { get; private set; }

    public void Initialize()
    {
        MoveParameterHash = Animator.StringToHash(MoveParameterName);
        RunParameterHash = Animator.StringToHash(RunParameterName);
        BaseAttackParameterHash = Animator.StringToHash(BaseAttackParameterName);   
        FindingParameterHash = Animator.StringToHash(FindingParameterName);
        HitParameterHash = Animator.StringToHash(HitParameterName);
    }

}