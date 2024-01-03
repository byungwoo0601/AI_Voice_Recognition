using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Animations;

public class Study_ClipCreation : MonoBehaviour
{
    public static Study_ClipCreation instance;

    AnimatorController animatorController;
    AnimatorControllerLayer StateLayer;
    AnimatorState IdleState;
    public AnimatorState newState;

    public Animator anim;
    public AnimationClip IdleClip;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        anim = GetComponent<Animator>();
        animatorController = anim.runtimeAnimatorController as AnimatorController;
        StateLayer = animatorController.layers[0];
        newState = new AnimatorState();
        IdleState = new AnimatorState();
    }
    public void AnimStart()
    {
        Invoke("ActStart", 0.8f);
    }
    public void ActStart()
    {
        anim.SetTrigger("ActStart");
    }
    public void CreateState()
    {
        IdleState = StateLayer.stateMachine.AddState("Idle State");
        IdleState.motion = IdleClip;
        newState = StateLayer.stateMachine.AddState("New State");
        AnimatorStateTransition transition = StateLayer.stateMachine.AddAnyStateTransition(newState);
        transition.AddCondition(AnimatorConditionMode.If, 0, "ActStart");
        AnimatorStateTransition transition1 = newState.AddTransition(IdleState);
        transition1.hasExitTime = true;
    }
    public void RemoveState()
    {
        StateLayer.stateMachine.RemoveState(newState);
        StateLayer.stateMachine.RemoveState(IdleState);
    }
    private void OnApplicationQuit()
    {
        RemoveState();
    }
}
