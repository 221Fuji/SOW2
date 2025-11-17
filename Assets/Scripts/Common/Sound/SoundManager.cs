using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// ÉTÉEÉìÉhÇÃä«óù
/// </summary>
public class SoundManager : SingletonMonoBihaviour<SoundManager>
{
    [SerializeField] private float _masterVolume;
    [SerializeField] private float _bgmVolume;
    [SerializeField] private float _seVolume;
    [SerializeField] private SEResources _systemSE;
    private SEPlayer _systemSEPlayer;

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SEVolume => _seVolume;

    public SEPlayer SystemSEPlayer => _systemSEPlayer;

    protected override void Awake()
    {
        base.Awake();
        _systemSEPlayer = new SEPlayer(_systemSE);
    }

    public void SetMaterVolume(float volume)
    {
        _masterVolume = volume;
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
    }

    public void SetSEVolume(float volume) 
    {
        _seVolume = volume;
    }
}
