using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static NekraByte.FPS_Utility.Core.DataTypes;

// --------------------------------------------------------------------------
// Name: AudioManager
// Desc:
// --------------------------------------------------------------------------
public class AudioManager : MonoBehaviour
{
    #region - Singleton Pattern -
    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
    }
    #endregion

    //Inspector assingned
    [SerializeField] private AudioMixer _mainMixer  = null;
    [SerializeField] private int        _maxSounds  = 256;

    //Private Data
    public Dictionary<string, TrackInfo>    _tracks         = new Dictionary<string, TrackInfo>();

    List<AudioPoolItem>                     _pool           = new List<AudioPoolItem>();
    Dictionary<ulong, AudioPoolItem>        _activePool     = new Dictionary<ulong, AudioPoolItem>();

    List<LayeredAudioSource>                _layeredAudio   = new List<LayeredAudioSource>(); 

    private     ulong                       _idGiver        = 0;
    private     Transform                   _listenerPos    = null;

    public      MusicManager                musicManager = null;

    // ------------------------------------------ Methods ------------------------------------------ //

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name: Start
    // Desc: This method sets the correct mixer groups and assing them
    //       TrackInfo for each one, also the method turn this object in an
    //       DontDestroyOnLoad object, so he can maintain himself in the
    //       scene even though.
    // ----------------------------------------------------------------------
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        musicManager.AudioManager = this;

        if (!_mainMixer) return;

        AudioMixerGroup[] groups = _mainMixer.FindMatchingGroups(string.Empty);

        foreach(var group in groups)
        {
            TrackInfo trackInfo     = new TrackInfo();

            trackInfo.Name          = group.name;
            trackInfo.Group         = group;
            trackInfo.TrackFader    = null;

            _tracks[group.name]     = trackInfo;
        }

        for (int i = 0; i < _maxSounds; i++)
        {
            GameObject      go          = new GameObject("Pool Item");
            AudioSource     audioSource = go.AddComponent<AudioSource>();
            go.transform.parent         = transform;

            AudioPoolItem poolItem  = new AudioPoolItem();

            poolItem.GameObject     = go;
            poolItem.AudioSource    = audioSource;
            poolItem.Transform      = go.transform;
            poolItem.Playing        = false;
            go.SetActive(false);
            _pool.Add(poolItem);
        }
    }

    // ----------------------------------------------------------------------
    // Name: OnEnable (Class)
    // Desc: This method is called when the object is enabled, mainly the
    //       method register an method in the sceneLoaded event.
    // ----------------------------------------------------------------------
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ----------------------------------------------------------------------
    // Name: OnDisable (Class)
    // Desc: This method is called when the object is disabled, mainly the
    //       method unregister an method in the sceneLoaded event.
    // ----------------------------------------------------------------------
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ----------------------------------------------------------------------
    // Name: OnSceneLoaded (Class)
    // Desc: This method represents an action that will happens in the scene
    //       load event, basically the method get the current updated audio
    //       listener on the scene.
    // ----------------------------------------------------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _listenerPos = FindFirstObjectByType<AudioListener>().transform;
    }

    // ----------------------------------------------------------------------
    // Name: Update (Method)
    // Desc: 
    // ----------------------------------------------------------------------
    private void Update()
    {
        foreach(var las in _layeredAudio) 
            if (las != null) las.Update();

        if (musicManager != null) 
            musicManager.OnUpdate();
    }
    #endregion

    #region - Audio Mixer Manipulation -
    // ----------------------------------------------------------------------
    // Name: GetTrackVolume
    // Desc: This method returns the volume of an AudioMixerGroup identified
    //       on an trackInfo passed as argument.
    // ----------------------------------------------------------------------
    public float GetTrackVolume(string track)
    {
        TrackInfo trackInfo;
        if (_tracks.TryGetValue(track, out trackInfo))
        {
            float volume;
            _mainMixer.GetFloat(track, out volume);
            return volume;
        }
        return float.MinValue;
    }

    // ----------------------------------------------------------------------
    // Name: GetAudioGroupFromTrackName (Method)
    // Desc: This method receive an name as an string, find Audio Mixer Group
    //       that have the name and returns it.
    // ----------------------------------------------------------------------
    public AudioMixerGroup GetAudioGroupFromTrackName(string name)
    {
        TrackInfo trackInfo;
        if (_tracks.TryGetValue(name, out trackInfo))
            return trackInfo.Group;
        return null;
    }

    // ----------------------------------------------------------------------
    // Name: SetTrackVolume (Method)
    // Desc: This method sets the volume of an AudioMixerGroup of an track
    //       passed as argument.
    //       If a fade time is given, a coroutine will be used to perform an
    //       volume fade.
    // ----------------------------------------------------------------------
    public void SetTrackVolume(string track, float volume, float fadeTime = 0f)
    {
        if (!_mainMixer) return;
        TrackInfo trackInfo;

        if (_tracks.TryGetValue(track, out trackInfo))
        {
            if (trackInfo.TrackFader != null) StopCoroutine(trackInfo.TrackFader);

            if (fadeTime == 0f) _mainMixer.SetFloat(track, volume);
            else
            {
                trackInfo.TrackFader = SetTrackVolumeInternal(track, volume, fadeTime);
                StartCoroutine(trackInfo.TrackFader);
            }
        }
    }

    // ----------------------------------------------------------------------
    // Name: SetTrackVolumeInternal (Coroutine)
    // Desc: This method is used by the SetTrackVolume to implement a fade
    //       between volumes of a track over time.
    // ----------------------------------------------------------------------
    protected IEnumerator SetTrackVolumeInternal(string track, float volume, float fadeTime)
    {
        float startVolume = 0f;
        float timer = 0f;
        _mainMixer.GetFloat(track, out startVolume);

        while(timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;

            _mainMixer.SetFloat(track, Mathf.Lerp(startVolume, volume, timer / fadeTime));
            yield return null;
        }
        _mainMixer.SetFloat(track, volume);
    }
    #endregion

    #region - Audio Pooling System -
    // ----------------------------------------------------------------------
    // Name: ConfigurePoolObject (Method)
    // Desc: This method register an audio pool instance.
    // ----------------------------------------------------------------------
    protected ulong ConfigurePoolObject(int poolIndex, string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float uniportance)
    {
        //Verify if the request is out of the list bounds
        if (poolIndex < 0 || poolIndex >= _pool.Count) return 0;

        //Get the neede pool item
        AudioPoolItem poolItem = _pool[poolIndex];

        //Generate an ID to the pool
        _idGiver++;

        //Configure the audio source settings
        AudioSource source              = poolItem.AudioSource;
        source.clip                     = clip;
        source.volume                   = volume;
        source.spatialBlend             = spatialBlend;

        //Assing the correct Audio Mixer Group to the source
        source.outputAudioMixerGroup    = _tracks[track].Group;

        source.transform.position       = position;
        poolItem.Playing                = true;
        poolItem.Uniportance            = uniportance;
        poolItem.ID                     = _idGiver;
        poolItem.GameObject.SetActive(true);
        source.Play();
        poolItem.Coroutine              = StopSoundDelayed(_idGiver, source.clip.length);

        StartCoroutine(poolItem.Coroutine);

        _activePool[_idGiver] = poolItem;

        return _idGiver;
    }

    protected IEnumerator StopSoundDelayed(ulong id, float duration)
    {
        //Waits for the sound duration
        yield return new WaitForSeconds(duration);
        AudioPoolItem activeSound;

        //Try get the pool item
        if (_activePool.TryGetValue(id, out activeSound))
        {
            //Stop the sound manually and imediatly
            activeSound.AudioSource.Stop();
            activeSound.AudioSource.clip = null;
            activeSound.GameObject.SetActive(false);
            //Remove from the active pool
            _activePool.Remove(id);

            //Makes the pool item available again
            activeSound.Playing = false;
        }
    }

    public void StopOneShotSound(ulong id)
    {
        AudioPoolItem activeSound;

        //Try get the pool item
        if (_activePool.TryGetValue(id, out activeSound))
        {
            //Stop the coroutine that gonna turn of this sound
            StopCoroutine(activeSound.Coroutine);

            //Stop the sound manually and imediatly
            activeSound.AudioSource.Stop();
            activeSound.AudioSource.clip = null;
            activeSound.GameObject.SetActive(false);

            //Remove from the active pool
            _activePool.Remove(id);

            //Makes the pool item available again
            activeSound.Playing = false;
        }
    }

    // ----------------------------------------------------------------------
    // Name: PlayOneShotSound (Method)
    // Desc: This method scores the sound priority an search for an unused
    //       pool item to use as the sound audio source. If don't exist an
    //       available audio pool item, an pool with an lower priority will
    //       be killed an reused.
    // ----------------------------------------------------------------------
    public ulong PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, int priority = 128)
    {
        //Prevent errors and unwanted behaviors
        if (!_tracks.ContainsKey(track) || clip == null || volume.Equals(0f)) return 0;

        float unimportance = (_listenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);

        int     leastImportantIndex     = -1;
        float   leastImportanceValue    = float.MaxValue;

        for (int i = 0; i < _pool.Count; i++)
        {
            AudioPoolItem poolItem = _pool[i];

            //Finded an avaliable pool item
            if (!poolItem.Playing) return ConfigurePoolObject(i, track, clip, position, volume, spatialBlend, unimportance);
            else if (poolItem.Uniportance > leastImportanceValue)//Finded an pool item that is less important that the new one tha the method gonna play
            {
                //Save the pool item that the loop has finde as an less important so far, for a candidate to be replaced with the new sound. 
                leastImportanceValue    = poolItem.Uniportance;
                leastImportantIndex     = i;
            }
        }

        //If all sound pools are being used, the least important is replaced by the new sound.
        if (leastImportanceValue > unimportance) return ConfigurePoolObject(leastImportantIndex, track, clip, position, volume, spatialBlend, unimportance);

        //If do not exist an pool less available or less important than the new sound, the sound is not played.
        return 0;
    }

    public void PlayOneShotSound(AudioClip clip, Vector3 position, AudioCollection audioCollection)
    {
        PlayOneShotSound(audioCollection.audioGroup, clip, position, audioCollection.volume, audioCollection.spatialBlend, audioCollection.priority);
    }

    // ----------------------------------------------------------------------
    // Name: PlayOneshotSound (Method)
    // Desc: Queue up an sound that will be played only after a few number of
    //       seconds.
    // ----------------------------------------------------------------------
    public IEnumerator PlayOneShotSoundDelayed(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, int priority = 128)
    {
        yield return new WaitForSeconds(duration);
        PlayOneShotSound(track, clip, position, volume, spatialBlend, priority);
    }
    #endregion

    #region - Layered Audio System -
    // -------------------------------------------------------------------------------
    // Name: RegisterLayeredAudioSource
    // Desc: This method register an LayeredAudioSource on the list.
    // -------------------------------------------------------------------------------
    public ILayeredAudioSource RegisterLayeredAudioSource(AudioSource source, int layers)
    {
        if (source != null && layers > 0)
        {
            // First check it doesn't exist already and if so just return the source
            for (int i = 0; i < _layeredAudio.Count; i++)
            {
                LayeredAudioSource item = _layeredAudio[i];
                if (item != null)
                    if (item.audioSource == source) return item;
            }

            // Create a new layered audio item and add it to the managed list
            LayeredAudioSource newLayeredAudio = new LayeredAudioSource(source, layers);
            _layeredAudio.Add(newLayeredAudio);

            return newLayeredAudio;
        }

        return null;
    }

    // -------------------------------------------------------------------------------
    // Name: UnregisterLayeredAudioSource (Overload)
    // Desc: This method Unregister an layered audio source, preventing the system to
    //       break after scenes transitions, using as reference an LayeredAudioSource.
    // -------------------------------------------------------------------------------
    public void UnregisterLayeredAudioSource(ILayeredAudioSource source)
    {
        _layeredAudio.Remove((LayeredAudioSource)source);
    }

    // -------------------------------------------------------------------------------
    // Name: UnregisterLayeredAudioSource (Overload)
    // Desc: This method unregister the layered audio source, preventing the system
    //       to break after scenes transitions, using as reference an AudioSource.
    // -------------------------------------------------------------------------------
    public void UnregisterLayeredAudioSource(AudioSource source)
    {
        for (int i = 0; i < _layeredAudio.Count; i++)
        {
            LayeredAudioSource item = _layeredAudio[i];
            if (item != null)
            {
                if (item.audioSource == source)
                {
                    _layeredAudio.Remove(item);
                    return;
                }
            }
        }
    }
    #endregion

    #region - Music Events -
    public void CallMusicEvent(MusicEvent eventType)
    {
        musicManager.SelectMusicEvent(eventType);
    }
    public void CallMusicEvent(int eventType)
    {
        musicManager.SelectMusicEvent((MusicEvent)eventType);
    }
    public void CallMusicEvent(string eventType)
    {
        musicManager.SelectMusicEvent(eventType);
    }
    #endregion
}