using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;

/// <summary>
/// ÉTÉEÉìÉhÇÃä«óù
/// </summary>
public class SoundManager : SingletonMonoBihaviour<SoundManager>
{
    [SerializeField] private float _masterVolume;
    [SerializeField] private float _bgmVolume;
    [SerializeField] private float _seVolume;
    [SerializeField] private SEResources _systemSE;
    [SerializeField] private SEResources _fightingGeneralSE;
    [SerializeField] private BGMPlayer _outGameBGMPrefab;
    private SEPlayer _systemSEPlayer;
    private FightingGeneralSEPlayer _fgSEPlayer;
    private BGMPlayer _outGameBGMPlayer;
    private BGMPlayer _fightingBGMPlayer;

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SEVolume => _seVolume;

    public SEPlayer SystemSEPlayer => _systemSEPlayer;
    public FightingGeneralSEPlayer FGSEPlayer => _fgSEPlayer;
    public BGMPlayer FightingBGMPlayer => _fightingBGMPlayer;
    public BGMPlayer OutGameBGMPlayer => _outGameBGMPlayer;

    protected override void Awake()
    {
        base.Awake();
        _systemSEPlayer = new SEPlayer(_systemSE);
        _fgSEPlayer = new FightingGeneralSEPlayer(_fightingGeneralSE);
        _outGameBGMPlayer = Instantiate(_outGameBGMPrefab);
    }

    public void CreateCharaBGMPlayer(CharacterData characterData)
    {
        _fightingBGMPlayer?.DestoryBGMPlayer();
        _fightingBGMPlayer = Instantiate(characterData.CharacterBGMPlayer);
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
