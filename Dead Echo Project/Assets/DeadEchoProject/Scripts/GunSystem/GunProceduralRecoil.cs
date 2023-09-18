using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes.GunDataConteiner;

public class GunProceduralRecoil : MonoBehaviour
{
    /* Code made by Victor Paulo Melo da Silva - Junior Unity Programmer - https://www.linkedin.com/in/victor-nekra-dev/
     * GunProceduralRecoil - Code Update Version 0.7 - (Refactored code).
     * Feel free to take all the code logic and apply in yours projects.
     * This project represents a work to improve my personal habilities, and has no intention of obtaining any financial return.
     */

    #region - Weapon Recoil Vectors -
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private Vector3 currentPosition;
    private Vector3 targetPosition;
    #endregion

    #region - Class References -
    private GameObject recoilObject     = null;
    #endregion

    #region - Recoil Values -
    private float _recoilX      = -3;
    private float _recoilY      = 3f;
    private float _recoilZ      = 2f;

    private float _snappiness       = 6f;
    private float _returnSpeed      = 20f;

    private float _zKickBack                = 0.4f;
    private float Z_Current_KickBack        = 0.4f;

    private float _recoilReduct                 = 1f;
    [HideInInspector] public bool _isAiming     = false;
    #endregion

    // ------------------------------------------ Methods ------------------------------------------ //

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name: Start
    // Desc: This method is called on the game start, and mainly the method
    //       get all the class dependencies objects.
    // ----------------------------------------------------------------------
    private void Start()
    {
        recoilObject = AnimationLayer.GetAnimationLayer("RecoilLayer", ControllerManager.Instance._animLayers).layerObject;
    }
    #endregion

    #region - Data Initialization -
    // ----------------------------------------------------------------------
    // Name: InitializeData
    // Desc: This method receive the initial recoil data by the current
    //       weapon.
    // ----------------------------------------------------------------------
    public void InitializeData(RecoilData data)
    {
        _recoilX        = data._recoilX;
        _recoilY        = data._recoilY;
        _recoilZ        = data._recoilZ;

        _zKickBack      = data._zKickback;

        _snappiness     = data._snappiness;
        _returnSpeed    = data._returnSpeed;

        _recoilReduct   = data._recoilReduction;
    }
    #endregion

    #region - Recoil Calculation -
    // ----------------------------------------------------------------------
    // Name: RecoilCalculation
    // Desc: This method calls the recoil movement calculation every frame
    //       update.
    // -----------------------------------------------------------------------
    private void Update() => RecoilCalculation();

    // ----------------------------------------------------------------------
    // Name: RecoilCalculation
    // Desc: This method calculates the recoil vectors, adding smoothig using
    //       lerps and slerps, considering the aim state and finally adding
    //       the movement to the recoil object.
    // ----------------------------------------------------------------------
    private void RecoilCalculation()
    {
        //This method use the Vector3.Lerp to interpolate between two vectors to apply the recoil, one vector represents the weapon position with the recoil values, the other vector literally applis the recoil to the current position and rotation
        if (_isAiming) Z_Current_KickBack = _zKickBack;
        else Z_Current_KickBack = _zKickBack;

        targetPosition      = Vector3.Lerp(targetPosition, Vector3.zero, _returnSpeed * Time.deltaTime);
        currentPosition     = Vector3.Lerp(currentPosition, targetPosition, _snappiness * Time.deltaTime);

        recoilObject.transform.localPosition = currentPosition;

        targetRotation      = Vector3.Lerp(targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
        currentRotation     = Vector3.Slerp(currentRotation, targetRotation, _snappiness * Time.fixedDeltaTime);

        recoilObject.transform.localRotation = Quaternion.Euler(currentRotation);
    }
    #endregion

    #region - Recoil Fire -
    // ----------------------------------------------------------------------
    // Name: RecoilFire
    // Desc: This method trigger the gun recoil action, seting the recoil to
    //       the target rotation that will be handle on the update section
    //       later.
    // ----------------------------------------------------------------------
    public void RecoilFire()
    {
        //This method fires the recoil, add the recoil variables values to the current targetRotation and position
        targetPosition -= _isAiming ? (Vector3.forward * Z_Current_KickBack) * _recoilReduct : Vector3.forward * Z_Current_KickBack;

        if (_isAiming) targetRotation += new Vector3(_recoilX, Random.Range(-_recoilY, _recoilY), Random.Range(-_recoilZ, _recoilZ)) * _recoilReduct;
        else targetRotation += new Vector3(_recoilX, Random.Range(-_recoilY, _recoilY), Random.Range(-_recoilZ, _recoilZ));
        
    }
    #endregion
}