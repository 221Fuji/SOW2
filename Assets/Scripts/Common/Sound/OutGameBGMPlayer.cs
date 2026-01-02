using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutGameBGMPlayer : BGMPlayer
{
    [SerializeField] private EOutGameBGMState _outgameState;

    public EOutGameBGMState OutGameBGMState 
    {   
        get 
        { 
            return _outgameState; 
        }
        
        set 
        {
            _outgameState = value;
            ChangeStateAllBGM(_outgameState);
        }
    }

    public void ChangeStateAllBGM(EOutGameBGMState state)
    {
        foreach (var inst in _instList)
        {
            if (IsPlaying(inst))
            {
                inst.setParameterByNameWithLabel("OutGameState", state.ToString());
            }
        }
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
        SoundManager.I.OutGameBGMPlayer.OutGameBGMState = EOutGameBGMState.Default;
    }

    public enum EOutGameBGMState
    {
        Default,
        ChangeScene
    }
}
