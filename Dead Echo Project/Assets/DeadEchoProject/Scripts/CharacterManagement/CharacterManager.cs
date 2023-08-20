using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    //Inspector assinged
    [Header("Dependencies")]
    [SerializeField] private CapsuleCollider            _meleeTrigger   = null;
    [SerializeField] private Camera                     _fpsCamera      = null;
    [SerializeField] public ScreenDamageManager         _damageManager  = null;

    [Header("Health System")]
    [Range(0f, 300)] public float      _currentHealth  = 100f;
    public float                       _maxHealth      = 100f;

    //Private
    private Collider                _collider               = null;
    private FPS_Controller          _fpsController          = null;
    private CharacterController     _characterController    = null;
    private GameSceneManager        _gameSceneManager       = null;

    #region - BuiltIn Methods -
    private void Start()
    {
        _collider               = GetComponent<Collider>();
        _fpsController          = GetComponent<FPS_Controller>();
        _characterController    = GetComponent<CharacterController>();
        _gameSceneManager       = GameSceneManager.instance;

        if (_gameSceneManager != null)
        {
            PlayerInfo playerInfo = new PlayerInfo();

            playerInfo.camera               = _fpsCamera;
            playerInfo.characterManager     = this;
            playerInfo.collider             = _collider;
            playerInfo.meleeTrigger         = _meleeTrigger;

            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), playerInfo);
        }
    }
    #endregion

    #region - Damage System -
    public void TakeDamage(float value)
    {
        _currentHealth = Mathf.Max(_currentHealth - value, 0f);

        if (_damageManager != null)
        {
            _damageManager.minBloodAmount = (1.0f - (_currentHealth / 100f));
            _damageManager.bloodAmount = Mathf.Min(_damageManager.minBloodAmount + 0.3f, 1f);
        }
        if (_currentHealth <= (_maxHealth / 4)) _damageManager.SetCriticalHealth(true);
        else _damageManager.SetCriticalHealth(false);
        UI_Manager.Instance.UpdatePlayerState(_fpsController, this);
    }
    #endregion
}