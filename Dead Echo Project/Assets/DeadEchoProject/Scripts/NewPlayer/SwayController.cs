using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;
using static NekraByte.FPS_Utility.Core.Procedural;

public class SwayController : MonoBehaviour
{
    private ControllerManager _controllerManager;

    private GameObject _swayObject;

    [SerializeField] private SwayData _swayData;

    #region - Input Sway -
    private Vector3 inpt_initialPosition;
    private SpringInterpolator inpt_SpringLerp;
    #endregion

    private void Start()
    {
        _controllerManager = GetComponent<ControllerManager>();
        _swayObject = AnimationLayer.GetAnimationLayer("SwayLayer", _controllerManager._animLayers).layerObject;

        inpt_initialPosition = _swayObject.transform.localPosition;
        inpt_SpringLerp = new SpringInterpolator(inpt_initialPosition);
    }
    private void Update()
    {
        InputSwayHandler();
    }
    private void InputSwayHandler()
    {
        float xValue = 0;
        float yValue = 0;

        xValue = Mathf.Clamp(xValue, -_swayData.inpt_maxAmout, _swayData.inpt_maxAmout);
        yValue = Mathf.Clamp(yValue, -_swayData.inpt_maxAmout, _swayData.inpt_maxAmout);

        Vector3 targetPos = new Vector3(xValue, yValue, 0);

        _swayObject.transform.localPosition = 
            inpt_SpringLerp.SpringLerp(targetPos, 
            _swayData.inpt_damping, 
            _swayData.inpt_stiffness, 
            Time.deltaTime * _swayData.inpt_smoothAmount);
    }
}