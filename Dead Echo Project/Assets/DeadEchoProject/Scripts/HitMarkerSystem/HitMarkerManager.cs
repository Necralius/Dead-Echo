using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitMarkerManager : MonoBehaviour
{
    #region - Singleton Pattern -
    public static HitMarkerManager Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;
    }
    #endregion

    [Header("Audio")]
    [SerializeField] private AudioCollection    collection  = null;

    [Header("Visual")]
    private RectTransform      hitMarker   = null;

    [SerializeField] private float _timeOnScreen = 0.3f;

    [SerializeField] private float _changeSpeed     = 5f;
    [SerializeField] private float _maxSize         = 2f;
    [SerializeField] private float _targetSize      = 0f;

    private float _defaultSize     = 1f;
    private float _currentSize     = 0f;

    private void Start()
    {
        hitMarker = GetComponent<RectTransform>();

        _defaultSize = hitMarker.sizeDelta.x;
        _currentSize = _defaultSize;
    }

    public void OnHit(HitInfo hitInfo)
    {
        hitInfo.OnHit(collection);

        _targetSize = _maxSize;
        hitMarker.gameObject.SetActive(true);

        if (hitInfo.type == HitInfo.HitType.Headshot) ChangeColor(Color.red);
        else ChangeColor(Color.white);
    }
    public void ChangeColor(Color color)
    {
        Image[] components = hitMarker.GetComponentsInChildren<Image>();
        foreach(var c in components) c.color = color;
    }

    private void Update()
    {
        if (hitMarker.gameObject.activeInHierarchy)
        {
            _currentSize = Mathf.Lerp(_currentSize, (_targetSize * 10), _changeSpeed * Time.deltaTime);
            hitMarker.sizeDelta = new Vector2(_currentSize, _currentSize);
            StartCoroutine(DeactiveHitMarker());
        }

    }
    IEnumerator DeactiveHitMarker()
    {
        yield return new WaitForSeconds(_timeOnScreen);
        _targetSize = _defaultSize;
        hitMarker.gameObject.SetActive(false);
    }
}
public class HitInfo
{
    public enum HitType { Default = 0 , Headshot = 1 }

    public HitType type         = HitType.Default;
    public Vector3 hitPoint     = Vector3.zero;

    public HitInfo(HitType type, Vector3 hitPoint)
    {
        this.type       = type; 
        this.hitPoint   = hitPoint;
    }

    public void OnHit(AudioCollection hitSounds)
    {
        if (hitSounds == null) return;
        AudioManager.Instance.PlayOneShotSound(hitSounds[(int)type], hitPoint, hitSounds);
    }
}