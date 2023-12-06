using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class ClipCreation : MonoBehaviour
{
    Animator Anim;
    AnimatorController ac;
    public AnimationClip[] animationClip;
    public AnimatorState[] emptyState;
    AnimatorStateMachine stateMachine;

    public Text resultText;

    public string stateName = "";
    public string clipName = "";
    public void Btn()
    {
        animationClip = new AnimationClip[SentenceToWords.words.Count];
        emptyState = new AnimatorState[SentenceToWords.words.Count];

        Anim = GetComponent<Animator>();
        AddEmptyStates();
        SetupTransitions();
    }

    private void AddEmptyStates()
    {
        ac = Anim.runtimeAnimatorController as AnimatorController;
        stateMachine = ac.layers[0].stateMachine;

        List<string> words = SentenceToWords.words;

        AnimationClip[] animationClips = new AnimationClip[words.Count];

        for (int i = 0; i < words.Count; i++)
        {
            animationClips[i] = Resources.Load(words[i]) as AnimationClip;

            emptyState[i] = stateMachine.AddState(words[i]);
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
