using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMenu : MonoBehaviour
{
    Animator animator;
    public GameObject character;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SlideStudy()
    {
        animator.SetTrigger("M");
        animator.SetTrigger("SS");

        Invoke("characterOn", 1.8f);
    }

    public void SlideTranslate()
    {
        animator.SetTrigger("M");
        animator.SetTrigger("ST");

        Invoke("characterOn", 1.8f);
    }

    public void SlideStudyToMain()
    {
        character.SetActive(false);

        animator.SetTrigger("M");
        animator.SetTrigger("STM");
    }

    public void SlideTranslateToMain()
    {
        character.SetActive(false);

        animator.SetTrigger("M");
        animator.SetTrigger("TTM");
    }
    public void characterOn()
    {
        character.SetActive(true);
    }
}
