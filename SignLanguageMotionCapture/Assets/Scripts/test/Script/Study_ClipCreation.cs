using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Study_ClipCreation : MonoBehaviour
{
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    void CreateEmpty()
    {
        
    }
    public void test()
    {
        anim.SetTrigger("ActStart");
    }
}
