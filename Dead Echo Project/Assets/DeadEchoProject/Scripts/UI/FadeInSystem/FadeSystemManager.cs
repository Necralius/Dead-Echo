using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FadeSystemManager : MonoBehaviour
{
    #region - Singleton Pattern -
    public static FadeSystemManager Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    //Dependencies
    private Animator    _anim;

    //Private Data
    private int         _fadeInHash = Animator.StringToHash("Fade_In");

    [Header("Event Actions")]
    public UnityEvent   _onTransitionEnd;

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// This method starts an fade action deactivating and activating especific GameObjects.
    /// </summary>
    /// <param name="objectsToDeactivate"> List of GameObjects to be deactivated.</param>
    /// <param name="objectsToActivate"> List of GameObjects to be activated.</param>
    public void CallFadeAction(List<GameObject> objectsToDeactivate, List<GameObject> objectsToActivate)
    {
        foreach(var obj in objectsToDeactivate) obj.SetActive(false);
        foreach(var obj in objectsToActivate)   obj.SetActive(true);

        _anim.SetTrigger(_fadeInHash);
    }

    /// <summary>
    /// This method starts the simple fade action.
    /// </summary>
    public void CallFadeAction()    => _anim.SetTrigger(_fadeInHash);

    /// <summary>
    /// This method starts an fade complete action.
    /// </summary>
    /// <param name="executeOnEnd">Actions to be executed on the fade in event end.</param>
    public void CallFadeAction(UnityEvent executeOnEnd)
    {
        _onTransitionEnd = executeOnEnd;
        _anim.SetTrigger(_fadeInHash);
    }

    /// <summary>
    /// This method is a animation event, and is called mainly when the FadeIn action phase is completed.
    /// </summary>
    public void OnFadeEnd()         => _onTransitionEnd.Invoke();
}