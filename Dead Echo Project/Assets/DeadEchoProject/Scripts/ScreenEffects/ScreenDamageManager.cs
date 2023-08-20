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

    [SerializeField] private AudioClip  _heartBeat          = null;
    [SerializeField] private float      _soundFadeValue = 0.5f;
    private bool _fadeSound = false;
    private float _currentVolume = 1;

    // Properties
    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }
    public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }

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

        if (_fadeSound)
        {
            _currentVolume -= _soundFadeValue * Time.deltaTime;
            if (_currentVolume <= 0)
            {
                AudioSystem.Instance.GetEffectsSource().Stop();
                _fadeSound = false;
            }
        }
        AudioSystem.Instance.GetEffectsSource().volume = _currentVolume;
    }

    public void SetCriticalHealth(bool isCritical)
    {
        if (isCritical)
        {
            if (_heartBeat != null)
            {
                if (AudioSystem.Instance.GetEffectsSource().isPlaying) return;
                AudioSystem.Instance.PlayEffectSound(_heartBeat, Vector2.zero, Vector2.zero);
                _currentVolume = 1f;
            }
        }
        else _fadeSound = true;
    }
}