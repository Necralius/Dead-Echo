using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RespawnAction : MonoBehaviour
{
    [SerializeField] private bool callFade;

    [SerializeField] private UnityEvent onFadeEnd; // -> Actions to be executed on the fade end.

    private void Start()
    {
        onFadeEnd.AddListener(delegate { LoadFromLastSave(); });
    }

    private void LoadFromLastSave()
    {
        GameStateManager.Instance.LoadGame(GameStateManager.Instance.saveIndexer);
    }

    public void RespawnPlayer() 
    {
        if (callFade) FadeSystemManager.Instance.CallFadeAction(onFadeEnd); //Call Fade Action
        else LoadFromLastSave();
    }
}