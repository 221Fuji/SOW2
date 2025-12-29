using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Cysharp.Threading.Tasks.Triggers;

public class CharacterBGMPlayer : BGMPlayer
{
    protected CharacterActions _ca;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(CharacterActions ca)
    {
        _ca = ca;
    }
}
