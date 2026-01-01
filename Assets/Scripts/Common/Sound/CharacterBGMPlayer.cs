using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine.SceneManagement;

public class CharacterBGMPlayer : BGMPlayer
{
    protected CharacterActions _ca;

    protected override void Awake()
    {
        base.Awake();
        PlayBGM(0);
    }

    public void Initialize(CharacterActions ca)
    {
        _ca = ca;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ResultScene")
        {
            DestoryBGMPlayer();
        }
    }
}
