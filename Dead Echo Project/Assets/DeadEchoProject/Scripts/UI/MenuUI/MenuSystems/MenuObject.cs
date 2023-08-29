using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MenuManager;

public class MenuObject : MonoBehaviour
{
    //Private Data
    Animator anim => GetComponent<Animator>();

    private int onEntryHash = Animator.StringToHash("OnEntry");
    private int onExitHash = Animator.StringToHash("OnExit");
    public MenuType type;

    //Public Data
    public bool isActive = false;
    public string menuName = "Menu_";

    public void Activate()
    {
        isActive = true; 
        gameObject.SetActive(true);

        if (anim == null) return;
        anim.SetTrigger(onEntryHash);
    }
    public void Deactivate()
    {
        isActive = false; 
        gameObject.SetActive(false);

        if (anim == null) return;
        anim.SetTrigger(onExitHash);
    }
}