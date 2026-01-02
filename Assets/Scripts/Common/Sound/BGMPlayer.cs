using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] protected BGMResources _bgmResources;

    protected EventInstance[] _instList;

    public EventInstance[] InstList {  get { return _instList; } }

    protected virtual void Awake()
    {
        _instList = new EventInstance[_bgmResources.BGMList.Length];
        for (int i = 0; i < _instList.Length; i++)
        {
            _instList[i] = RuntimeManager.CreateInstance(_bgmResources.BGMList[i].Clip);
            _instList[i].setVolume(_bgmResources.BGMList[i].Volume);
        }
        DontDestroyOnLoad(gameObject);
    }

    public virtual void PlayBGM(int index)
    {
        if (IsPlaying(_instList[index]))
        {
            _instList[index].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        _instList[index].getVolume(out float volume);
        _instList[index].setVolume(volume * SoundManager.I.SEVolume);
        _instList[index].start();
    }

    public virtual void StopBGM(int index)
    {
        _instList[index].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public virtual void StopAll()
    {
        foreach (var inst in _instList)
        {
            if (IsPlaying(inst))
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    public virtual void DestoryBGMPlayer()
    {
        foreach(var inst in _instList)
        {
            if(IsPlaying(inst))
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }          
            inst.release();
        }
        Destroy(gameObject);
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
