using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactAudioSystem : MonoBehaviour
{
    #region - Dependencies -
    private AudioSource source => GetComponent<AudioSource>();
    private SphereCollider col => GetComponent<SphereCollider>();
    #endregion
    public float timeToDeactive         = 3f;

    #region - Data -
    [Header("Audio Data")]
    [SerializeField] private List<AudioClip> clips;
    private float timeActive            = 1f;
    private float timer                 = 0f;
    private float destructionTimer      = 0f;
    #endregion

    private void OnValidate()
    {
        col.radius = source.maxDistance * 27.4f;
    }
    // ----------------------------------------------------------------------
    // Name: OnCollisionEnter
    // Desc: Detect an object collision to execute the audio impact system,
    //       also, the system enables and disables the object trigger, to
    //       manage correcly the AI audio trigger system.
    // ----------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        col.enabled = true;
        timer = 0;
        destructionTimer = 0;
        AudioSystem.Instance.PlayEffectSound(clips[Random.Range(0, clips.Count)], new Vector2(0.60f, 0.70f), new Vector2(0.95f, 1f), source);
    }
    // ----------------------------------------------------------------------
    // Name: Update
    // Desc: Its called every frame, its used to manage an timer to deactive the object trigger when necessary.
    // ----------------------------------------------------------------------
    private void Update()
    {
        destructionTimer += Time.deltaTime;
        if (destructionTimer >= timeToDeactive) gameObject.SetActive(false);

        if (timer >= timeActive)
        {
            col.enabled = false;
            //col.radius = defaultColliderSize;
            return;
        }
        timer += Time.deltaTime;
    }
}