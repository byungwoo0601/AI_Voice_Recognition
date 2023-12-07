using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMenu : MonoBehaviour
{
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SlideStudy()
    {
        animator.SetTrigger("M");
        animator.SetTrigger("SS");
    }

    public void SlideTranslate()
    {
        animator.SetTrigger("M");
        animator.SetTrigger("ST");
    }

    public void SlideStudyToMain() 
    {
        animator.SetTrigger("M");
        animator.SetTrigger("STM");
    }

    public void SlideTranslateToMain()
    {
        animator.SetTrigger("M");
        animator.SetTrigger("TTM");
    }

}
