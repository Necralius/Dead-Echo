using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MenuManager;

public class MenuObject : MonoBehaviour
{
    public MenuType type;

    //Public Data
    public bool isActive = false;
    public string menuName = "Menu_";

    public void OpenMenu()
    {
        isActive = true; 
        gameObject.SetActive(true);
    }
    public void CloseMenu()
    {
        isActive = false; 
        gameObject.SetActive(false);
    }
}