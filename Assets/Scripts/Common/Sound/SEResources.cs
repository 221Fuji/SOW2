using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SoundResources/SEResources")]
public class SEResources : ScriptableObject
{
    [SerializeField] private SoundResource[] _seList;

    public SoundResource[] SEList => _seList;
}
