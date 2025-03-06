using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class FightingManager : ModeManager
{
    [SerializeField] private float _startPosX1P;
    [SerializeField] private float _startPosX2P;

    [SerializeField] private GameObject _camera;
    [SerializeField] private FightingUI _fightingUI;
    [SerializeField] private FightingEffectManager _effectManager;

    private CharacterActions _player1CA;
    private CharacterActions _player2CA;
    private RoundData _currentRoundData;

    public RoundData CurrentRoundData { get => _currentRoundData; }

    private void Awake()
    {
        Application.targetFrameRate = 60; // デバッグ用
    }

    public void InitializeFM(InputDevice inputDevice1P, CharacterData characterData1P,
        InputDevice inputDevice2P, CharacterData characterData2P)
    {
        PlayerInput player1 = PlayerInput.Instantiate(
            prefab: characterData1P.CharacterPrefab.gameObject,
            playerIndex: 1,
            pairWithDevice: inputDevice1P
            );
        PlayerInput player2 = PlayerInput.Instantiate(
            prefab: characterData2P.CharacterPrefab.gameObject,
            playerIndex: 2,
            pairWithDevice: inputDevice2P
            );

        //ラウンド１開始
        RoundData firstRound = new RoundData(2, 2, 1);
        CharacterActions player1CA = player1.GetComponent<CharacterActions>();
        CharacterActions player2CA = player2.GetComponent<CharacterActions>();
        StartRound(firstRound, player1CA, player2CA);
    }

    private void StartRound(RoundData roundData, CharacterActions player1CA, CharacterActions player2CA)
    {
        Debug.Log("ラウンド開始！");
        _currentRoundData = roundData;
        _player1CA = player1CA;
        _player2CA = player2CA;

        //敵設定
        _player1CA.InitializeCA(1, _player2CA);
        _player2CA.InitializeCA(2, _player1CA);
        //死亡デリゲート
        _player1CA.OnDie = GoNextRound;
        _player2CA.OnDie = GoNextRound;
        //カメラ設定
        _camera.GetComponent<FightingCameraManager>().InitializeCamera(_player1CA.transform, _player2CA.transform);
        //UI設定
        _fightingUI.SetPlayer(_player1CA.GetComponent<CharacterState>(), _player2CA.GetComponent<CharacterState>());
        _fightingUI.HeartLost(_currentRoundData);
        _effectManager.InitializeFEM(_player1CA, _player2CA);

        //座標リセット
        _player1CA.transform.position = new Vector2(_startPosX1P, StageParameter.GroundPosY);
        _player1CA.transform.position += new Vector3(_player1CA.PushBackBoxOffset.x, 0);
        _player2CA.transform.position = new Vector2(_startPosX2P, StageParameter.GroundPosY);
        _player2CA.transform.position -= new Vector3(_player2CA.PushBackBoxOffset.x, 0);
    }


    //ここを呼ぶ機構を作る
    private async void GoNextRound(int loserNum)
    {
        _currentRoundData.RoundNum++;

        if(loserNum == 2)
        {
            _currentRoundData.Heart2P--;
            if(_currentRoundData.Heart2P <= 0)
            {
                GameSet(1);
                return;
            }
        }
        else 
        {
            _currentRoundData.Heart1P--;
            if (_currentRoundData.Heart1P <= 0)
            {
                GameSet(2);
                return;
            }
        }

        var nextRoundFM = await GameManager.LoadAsync<FightingManager>("FightingScene");
        nextRoundFM.StartRound(_currentRoundData, _player1CA, _player2CA);
    }

    private void GameSet(int winnerNum)
    {
        _fightingUI.HeartLost(_currentRoundData);
        Debug.Log($"Player{winnerNum}の勝ち！");
    }
}

public struct RoundData
{
    public int Heart1P;
    public int Heart2P;
    public int RoundNum;

    public RoundData(int heart1P, int heart2P, int roundNum)
    {
        Heart1P = heart1P;
        Heart2P = heart2P;
        RoundNum = roundNum;
    }
}
