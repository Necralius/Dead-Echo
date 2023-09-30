using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class SliderFloatField : MonoBehaviour
{
    Slider          _sld     = null;
    TMP_InputField  _inpt    = null;

    private void OnEnable()
    {
        _sld    = GetComponent<Slider>();
        _inpt   = GetComponentInChildren<TMP_InputField>();

        _sld.onValueChanged.AddListener(delegate    { OnSliderChange(); });
        _inpt.onValueChanged.AddListener(delegate   { OnInputChange();  });

        OnSliderChange();
        OnInputChange();
    }

    private void OnSliderChange()
    {
        if (_inpt == null || _sld == null) 
            return;

        _inpt.text = _sld.value.ToString("F1");
    }
    private void OnInputChange()
    {
        if (_inpt == null || _sld == null) 
            return;

        float value = 0; 
        float.TryParse(_inpt.text, out value);

        if (value > _sld.maxValue)
        {
            value = _sld.maxValue;
            _inpt.text = value.ToString("F1");
        }
        _sld.value = value;
    }

    public float GetValue() => _sld.value;
    public void OverrideValue(float value)
    {
        if (_sld == null || _inpt == null) 
            return;

        _sld.value = value;
        _inpt.text = value.ToString("F1");
    }
}