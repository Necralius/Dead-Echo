using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenDamageManager : MonoBehaviour
{
    private CanvasGroup _canvasGroupd      = null;

    [SerializeField] private float      _bloodAmount        = 0.0f;
    [SerializeField] private float      _minBloodAmount     = 0.0f;
    [SerializeField] private bool       _autoFade           = true;
    [SerializeField] private float      _fadeSpeed          = 0.05f;

    [SerializeField] private AudioCollection  _heartBeat          = null;
    [SerializeField] private float           _soundFadeValue = 0.5f;
    private bool _fadeSound = false;
    private float _currentVolume = 1;

    // Properties
    public float    bloodAmount     { get   => _bloodAmount;        set => _bloodAmount     = value;  }
    public float    minBloodAmount  { get   => _minBloodAmount;     set => _minBloodAmount  = value;  }
    public float    fadeSpeed       { get   => _fadeSpeed;          set => _fadeSpeed       = value;  }
    public bool     autoFade        { get   => _autoFade;           set => _autoFade        = value;  }

    private void Start()
    {
        _canvasGroupd = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (_autoFade)
        {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
        _canvasGroupd.alpha = _bloodAmount;
    }

    public void SetCriticalHealth()
    {
        if (AudioManager.Instance != null) return;
        AudioManager.Instance.PlayOneShotSound(_heartBeat.audioClip, transform.position, _heartBeat);
    }
}