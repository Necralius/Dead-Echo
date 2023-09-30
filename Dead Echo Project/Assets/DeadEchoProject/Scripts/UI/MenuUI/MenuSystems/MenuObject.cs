using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MenuManager;

public class MenuObject : MonoBehaviour
{
    //Private Data
    Animator anim => GetComponent<Animator>();

    private int onEntryHash     = Animator.StringToHash("OnEntry");
    private int onExitHash      = Animator.StringToHash("OnExit");

    public MenuType type;

    //Public Data
    public bool isActive = false;
    public string menuName = "Menu_";

    public void OpenMenu()
    {
        isActive = true; 
        gameObject.SetActive(true);

        if (anim != null) anim.SetTrigger(onEntryHash);
    }
    public void CloseMenu()
    {
        isActive = false; 
        gameObject.SetActive(false);
    }
    public void CloseMenu_Animated()
    {
        if (anim != null) anim.SetTrigger(onExitHash);
    }
    public void DeactiveMenu()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}