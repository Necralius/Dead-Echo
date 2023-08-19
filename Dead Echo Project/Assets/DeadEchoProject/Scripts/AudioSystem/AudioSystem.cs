using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSystem : MonoBehaviour
{
    #region - Singleton Pattern -
    public static AudioSystem Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    #endregion

    public AudioMixer mainMixer;
    public AudioSource gunsSource;
    public AudioSource effectsSource;
    public AudioSource zombiesSource;
    public AudioSource musicSource;
    
    public void PlayGunClip(AudioClip clip)
    {
        if (clip.Equals(null))
        {
            Debug.LogWarning("Null clip, unable to play anything!");
            return;
        }
        if (gunsSource.Equals(null))
        {
            Debug.LogWarning("Null audio source, unable to play anything!");
            return;
        }
        gunsSource.PlayOneShot(clip);
    }
    public void PlayGunClip(AudioClip clip, Vector2 volumeRange, Vector2 pitchRange)
    {
        if (clip.Equals(null))
        {
            Debug.LogWarning("Null clip, unable to play anything!");
            return;
        }
        if (gunsSource.Equals(null))
        {
            Debug.LogWarning("Null audio source, unable to play anything!");
            return;
        }

        if (volumeRange == Vector2.zero) volumeRange    = new Vector2(0.85f, 1f);
        if (pitchRange  == Vector2.zero) pitchRange     = new Vector2(0.90f, 1f);

        gunsSource.volume = Random.Range(volumeRange.x, volumeRange.y);
        gunsSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        gunsSource.PlayOneShot(clip);
    }
    public void PlayEffectSound(AudioClip clip, Vector2 volumeRange, Vector2 pitchRange)
    {
        if (clip.Equals(null))
        {
            Debug.LogWarning("Null clip, unable to play anything!");
            return;
        }
        if (gunsSource.Equals(null))
        {
            Debug.LogWarning("Null audio source, unable to play anything!");
            return;
        }

        if (volumeRange == Vector2.zero) volumeRange = new Vector2(0.85f, 1f);
        if (pitchRange == Vector2.zero) pitchRange = new Vector2(0.90f, 1f);

        effectsSource.volume = Random.Range(volumeRange.x, volumeRange.y);
        effectsSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        effectsSource.PlayOneShot(clip);
    }
    public void PlayEffectSound(AudioClip clip, Vector2 volumeRange, Vector2 pitchRange, AudioSource source)
    {
        if (clip.Equals(null))
        {
            Debug.LogWarning("Null clip, unable to play anything!");
            return;
        }
        if (source.Equals(null))
        {
            Debug.LogWarning("Null audio source, unable to play anything!");
            return;
        }

        if (volumeRange == Vector2.zero) volumeRange = new Vector2(0.85f, 1f);
        if (pitchRange == Vector2.zero) pitchRange = new Vector2(0.90f, 1f);

        source.volume = Random.Range(volumeRange.x, volumeRange.y);
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip);
    }
}