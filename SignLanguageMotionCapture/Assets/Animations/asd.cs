using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
public class asd : MonoBehaviour
{
    Animator Anim;
    string stateName = "NewEmptyState";
    public AnimatorController ac;
    // Start is called before the first frame update
    private void AddEmptyState()
    {
        AnimatorController ac = Anim.runtimeAnimatorController as AnimatorController;
        AnimatorStateMachine stateMachine = ac.layers[0].stateMachine;
        AnimatorState emptyState = stateMachine.AddState(stateName);
        emptyState.motion = null; // Empty State이므로 모션은 null로 설정
    }

    void Start()
    {
        Anim = GetComponent<Animator>();
        Debug.Log(ac.animationClips[0].length);
        Debug.Log(ac.animationClips[1].length);
        AddEmptyState();
    }
}
