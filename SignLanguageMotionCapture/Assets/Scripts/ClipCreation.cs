using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class ClipCreation : MonoBehaviour
{
    Animator Anim;
    AnimatorController ac;
    public AnimationClip[] animationClip;
    public AnimatorState[] emptyState;
    AnimatorStateMachine stateMachine;

    public string stateName = "";
    public string clipName = "";
    void Start()
    {
        emptyState = new AnimatorState[animationClip.Length];
        Anim = GetComponent<Animator>();
        AddEmptyStates();
        SetupTransitions();
    }


    private void AddEmptyStates()
    {
        ac = Anim.runtimeAnimatorController as AnimatorController;
        stateMachine = ac.layers[0].stateMachine;

        string[] clipNames; //= { "Armature_Extracted motion_Armature", "HipHopDancing", "Idle"};
        clipNames = new string[animationClip.Length];

        for(int  i = 0; i < animationClip.Length; i++)
        {
            clipNames[i] = animationClip[i].name;
        }

        AnimationClip[] animationClips = new AnimationClip[clipNames.Length];

        for (int i = 0; i < clipNames.Length; i++)
        {
            animationClips[i] = Resources.Load(clipNames[i]) as AnimationClip;

            emptyState[i] = stateMachine.AddState("NewEmptyState" + i);
            emptyState[i].motion = animationClips[i];
        }
    }

    void SetupTransitions()
    {
        for (int i = 0; i < emptyState.Length - 1; i++)
        {
            AnimatorStateTransition transition = emptyState[i].AddTransition(emptyState[i + 1]);
            transition.hasExitTime = true;
        }
    }

    private void OnApplicationQuit()
    {
        RemoveEmptyState();
    }

    private void RemoveEmptyState()
    {
        foreach (AnimatorState state in emptyState)
        {
            stateMachine.RemoveState(state);
        }
    }
}
