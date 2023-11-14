using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImpactAudioSystem : MonoBehaviour
{
    #region - Dependencies -
    private SphereCollider col => GetComponent<SphereCollider>();
    #endregion
    public float timeToDeactive         = 3f;

    #region - Data -
    [Header("Audio Data")]
    [SerializeField] private List<AudioClip> clips;
    [SerializeField] private UnityEvent onCollision;
    #endregion

    // ----------------------------------------------------------------------
    // Name: OnCollisionEnter
    // Desc: Detect an object collision to execute the audio impact system,
    //       also, the system enables and disables the object trigger, to
    //       manage correcly the AI audio trigger system.
    // ----------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        col.enabled = true;
        onCollision.Invoke();
        AudioManager.Instance.PlayOneShotSound("Effects", clips[Random.Range(0, clips.Count)], transform.position, 0.8f, 1, 128);
    }
}