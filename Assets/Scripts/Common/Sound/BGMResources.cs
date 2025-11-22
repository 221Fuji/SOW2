using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SoundResources/BGMResources")]
public class BGMResources : ScriptableObject
{
    [SerializeField] private SoundResource[] _bgmList;
    
    public SoundResource[] BGMList => _bgmList;
}

[System.Serializable]
public class SoundResource
{
    [SerializeField]
    private EventReference _clip;
    public EventReference Clip => _clip;

    [SerializeField]
    private float _volume = 1.0f;

    public float Volume => _volume;
}
