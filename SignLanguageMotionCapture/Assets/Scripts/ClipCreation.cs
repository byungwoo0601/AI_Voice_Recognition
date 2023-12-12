using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class ClipCreation : MonoBehaviour
{
    Animator anim;
    AnimatorStateMachine stateMachine;
    AnimatorState[] emptyState;
    Text resultText;
    int num = 0;

    public AnimationClip defaultClip;
    public GameObject Content_text;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Btn()
    {
        InitializeEmptyStates();
        ApplyText();
        AddEmptyStates();
        SetupTransitions();
    }

    private void InitializeEmptyStates()
    {
        emptyState = new AnimatorState[SentenceToWords.words.Count + 1];
    }

    private void AddEmptyStates()
    {
        var ac = anim.runtimeAnimatorController as AnimatorController;

        if (ac != null)
        {
            stateMachine = ac.layers[0].stateMachine;

            List<string> words = SentenceToWords.words;

            for (int i = 0; i < words.Count + 1; i++)
            {
                AnimationClip clip = (i != words.Count) ? Resources.Load(words[i]) as AnimationClip : defaultClip;
                emptyState[i] = stateMachine.AddState((i != words.Count) ? words[i] : defaultClip.name);
                emptyState[i].motion = clip;
            }
        }
    }

    private void SetupTransitions()
    {
        for (int i = 0; i < emptyState.Length - 1; i++)
        {
            AnimatorStateTransition transition = emptyState[i].AddTransition(emptyState[i + 1]);
            transition.hasExitTime = true;
        }
    }

    private void ApplyText()
    {
        resultText = Content_text.transform.GetChild(num).GetChild(0).GetComponent<Text>();
        num++;
    }


    private void RemoveEmptyState()
    {
        foreach (AnimatorState state in emptyState)
        {
            stateMachine.RemoveState(state);
        }
    }

    private void OnApplicationQuit()
    {
        RemoveEmptyState();
    }
}
