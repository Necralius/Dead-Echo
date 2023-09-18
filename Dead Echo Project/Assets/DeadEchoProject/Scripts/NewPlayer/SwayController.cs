using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;
using static NekraByte.FPS_Utility.Core.Procedural;

public class SwayController : MonoBehaviour
{
    private ControllerManager _controllerManager;
    private InputManager _inptManager;

    private GameObject _swayObject;

    [SerializeField] private SwayData _swayData;

    #region - Input Sway -
    private Vector3 newWeaponRotation;
    private Vector3 newWeaponRotationVelocity;

    private Vector3 targetWeaponRotation;
    private Vector3 targetWeaponRotationVelocity;
    #endregion

    #region - Movement Sway -
    private Vector3 newWeaponMovementRotation;
    private Vector3 newWeaponMovementRotationVelocity;

    private Vector3 targetWeaponMovementRotation;
    private Vector3 targetWeaponMovementRotationVelocity;
    #endregion

    private Vector3 swayPosition;

    Vector2 look;
    Vector2 move;

    private void Start()
    {
        _controllerManager = GetComponent<ControllerManager>();
        _inptManager = InputManager.Instance;
        _swayObject = AnimationLayer.GetAnimationLayer("SwayLayer", _controllerManager._animLayers).layerObject;

        newWeaponRotation = _swayObject.transform.localRotation.eulerAngles;
    }
    private void Update()
    {
        look = _inptManager.Look;
        move = _inptManager.Move;

        SwayHandler();
    }
    private void SwayHandler()
    {
        if (_swayData._inputSway)
        {
            targetWeaponRotation.y += _swayData.inpt_amount * look.x * Time.deltaTime;
            targetWeaponRotation.x += _swayData.inpt_amount * look.y * Time.deltaTime;

            targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -_swayData.inpt_clampX, _swayData.inpt_clampX);
            targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -_swayData.inpt_clampY, _swayData.inpt_clampY);
            targetWeaponRotation.z = targetWeaponRotation.y;

            targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, _swayData.inpt_swayResetSmooth);
            newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, _swayData.inpt_smoothAmount);
        }

        if (_swayData._movementSway)
        {
            targetWeaponMovementRotation.z  = _swayData.move_SwayX * move.x;
            targetWeaponMovementRotation.x  = _swayData.move_SwayY * -move.y;

            targetWeaponMovementRotation    = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, _swayData.inpt_swayResetSmooth);
            newWeaponMovementRotation       = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, _swayData.move_SmoothAmount);
        }

        if (_swayData._idleSway)
        {
            var targetPosition = LissajousCurve(_swayData.swayTime, _swayData.swayAmountA, _swayData.swayAmountB) / _swayData.swayScale;

            swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * _swayData.swayLerpSpeed);
            _swayData.swayTime += Time.deltaTime;

            if (_swayData.swayTime > 6.3f) _swayData.swayTime = 0f;

            _swayObject.transform.localPosition = swayPosition;
        }

        _swayObject.transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private Vector3 LissajousCurve(float Time, float A, float B) => new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
}