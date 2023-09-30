using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckboxActivator : MonoBehaviour
{
    private Toggle _tlg = null;
    [SerializeField] private bool       _invertedResponse   = false;
    [SerializeField] List<GameObject>   _ObjectsToActivate  = new List<GameObject>();

    private void OnEnable()
    {
        _tlg = GetComponent<Toggle>();
        _tlg.onValueChanged.AddListener(delegate { CheckState(); });

        CheckState();
    }

    private void CheckState()
    {
        foreach(var obj in _ObjectsToActivate) 
            obj.SetActive(_invertedResponse ? !_tlg.isOn : _tlg.isOn);
    }
}