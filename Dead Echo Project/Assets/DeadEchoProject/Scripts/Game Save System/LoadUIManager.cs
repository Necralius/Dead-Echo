using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUIManager : MonoBehaviour
{
    public bool hasValidSave = false;
    [SerializeField] private int loadIndex = -1;

    MenuSystem menuSystem;

    private void Start()
    {
        menuSystem = GetComponentInParent<MenuSystem>();
    }

    public void SetUp()
    {
        if (hasValidSave) menuSystem.LoadGameSave(loadIndex);
    }
}