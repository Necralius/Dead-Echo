using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{
    // Inspector Assigned
    [SerializeField] private float _decayRate = 1f;

    // Internal
    private SphereCollider  _collider             = null;
    private float           _srcRadius            = 0f;
    private float           _tgtRadius            = 0f;
    private float           _interpolator         = 0f;
    private float           _interpolatorSpeed    = 0f;

    // ----------------------------------------------------------------------
    // Name: Awake
    // Desc: Is the first method called on an frame rendering action.
    // ----------------------------------------------------------------------
    void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        if (!_collider) return;

        _srcRadius      = _tgtRadius = _collider.radius;
        _interpolator   = 0f;

        if (_decayRate > 0.02f) _interpolatorSpeed  = 1f / _decayRate;
        else                    _interpolatorSpeed  = 0f;
    }

    // ----------------------------------------------------------------------
    // Name : FixedUpdate
    // Desc : This method is called fixed times in an frame, mainly the
    //        method is managing the collider radius, lerping between the
    //        sizes.
    // ----------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (!_collider)
        {
            Debug.LogWarning("No collider founded on Object: " + gameObject.name);
            return;
        }

        _interpolator       = Mathf.Clamp01(_interpolator + Time.deltaTime * _interpolatorSpeed);

        _collider.radius    = Mathf.Lerp(_srcRadius, _tgtRadius, _interpolator);

        if (_collider.radius < Mathf.Epsilon)   _collider.enabled = false;
        else                                    _collider.enabled = true;
    }

    // ----------------------------------------------------------------------
    // Name: SetRadius
    // Desc: This method set an new collider size, using the resize
    //       interpolation system.
    // ----------------------------------------------------------------------
    public void SetRadius(float newRadius, bool instantResize = false)
    {
        if (!_collider) return;

        _srcRadius      = instantResize ? newRadius : _collider.radius;
        _tgtRadius      = newRadius;
        _interpolator   = 0f;

        // TODO -> To represent an blood smell instinct on the zombies the radius seted on this method, will have some modification:
        // flot newRadius = Mathf.Max(_RadiusToSet, (MaxHealthValue - CurrentHealthValue) / 6f);
    }

    // ----------------------------------------------------------------------
    // Name: SetRadius (Method Overcharge)
    // Desc: This method is an overcharge of the defaul SetRadius method,
    //       the difference between the method are that, this method is only
    //       for interactions where the radius will shrink over the time.
    // ----------------------------------------------------------------------
    public void SetRadius(float newRadius)
    {
        if (!_collider) return;

        _srcRadius      = _collider.radius;
        _tgtRadius      = newRadius;
        _interpolator   = 0f;
    }
}