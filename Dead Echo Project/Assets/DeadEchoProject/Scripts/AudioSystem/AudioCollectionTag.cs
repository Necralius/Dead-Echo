using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Audio Collection By Tag", menuName = "Dead Echo/Audio/Create New Audio Collection By Tag")]
public class AudioCollectionTag : AudioCollection
{
    public List<AudioTag> Clips = new List<AudioTag>();
    
    public AudioClip this[string tag]
    {
        get
        {
            if (tag == null || tag == string.Empty) return null;            
            foreach(var clipCollection in Clips)
            {
                if (clipCollection.Tag == tag) 
                    return clipCollection.clip;
            }
            return null;
        }
    }
}

[Serializable]
public struct AudioTag
{
    public string Tag;
    public List<AudioClip> Clip;

    public AudioClip clip
    {        
        get => Clip[Random.Range(0, Clip.Count)];
    }
}