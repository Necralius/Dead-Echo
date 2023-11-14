using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    #region - Singleton Pattern -
    public static PersistentCanvas Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    #endregion
}