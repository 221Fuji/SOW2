using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class SEPlayer 
{
    private SEResources _seResources;

    private EventInstance[] _instList;

    public SEPlayer(SEResources seResources)
    {
        _seResources = seResources;
        _instList = new EventInstance[_seResources.SEList.Length];
        for(int i = 0; i < _instList.Length; i++)
        {
            _instList[i] = RuntimeManager.CreateInstance(_seResources.SEList[i].Clip);
            _instList[i].setVolume(_seResources.SEList[i].Volume);
        }
    }

    public void PlaySE(int index)
    {
        if (IsPlaying(_instList[index]))
        {
            _instList[index].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        _instList[index].getVolume(out float volume);
        _instList[index].setVolume(volume * SoundManager.I.SEVolume);
        _instList[index].start();
    }

    /// <summary>
    /// çƒê∂íÜÇ»ÇÁtrue
    /// </summary>
    /// <param name="inst"></param>
    /// <returns></returns>
    public bool IsPlaying(EventInstance inst)
    {
        inst.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.PLAYING
            || state == PLAYBACK_STATE.STARTING
            || state == PLAYBACK_STATE.SUSTAINING;
    }
}
