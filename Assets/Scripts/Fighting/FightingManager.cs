using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;


public class FightingManager : ModeManager
{
    [SerializeField] private int _timeLimit;

    [SerializeField] private float _startPosX1P;
    [SerializeField] private float _startPosX2P;

    [SerializeField] private GameObject _camera;
    [SerializeField] private FightingUI _fightingUI;
    [SerializeField] private FightingEffectManager _effectManager;
    [Space]

    private PlayerData _playerData1P;
    private PlayerData _playerData2P;

    private RoundData _currentRoundData;
    private CancellationTokenSource _timeLimitCTS;

    public RoundData CurrentRoundData { get => _currentRoundData; }

    private void Awake()
    {
        Application.targetFrameRate = 60; // デバッグ用
    }

    public void InitializeFM(CharacterData characterData1P, CharacterData characterData2P)
    {
        PlayerInput player1 = PlayerInput.Instantiate(
            prefab: characterData1P.CharacterPrefab.gameObject,
            playerIndex: 1,
            pairWithDevice: GameManager.Player1Device
            );
        PlayerInput player2 = PlayerInput.Instantiate(
            prefab: characterData2P.CharacterPrefab.gameObject,
            playerIndex: 2,
            pairWithDevice: GameManager.Player2Device
            );

        //ラウンド１開始
        RoundData firstRound = new RoundData(2, 2, 1);
        PlayerData playerData1P = new PlayerData(
            characterData1P, 
            player1.GetComponent<CharacterActions>(),
            player1.GetComponent<CharacterState>(),
            1
            );
        PlayerData playerData2P = new PlayerData(
            characterData2P,
            player2.GetComponent<CharacterActions>(),
            player2.GetComponent<CharacterState>(),
            2
            );
        StartRound(firstRound, playerData1P, playerData2P);
    }

    private void StartRound(RoundData roundData, PlayerData playerData1P, PlayerData playerData2P)
    {
        Debug.Log("ラウンド開始！");
        _currentRoundData = roundData;
        _playerData1P = playerData1P;
        _playerData2P = playerData2P;

        CharacterActions ca1P = _playerData1P.CharacterActions;
        CharacterActions ca2P = _playerData2P.CharacterActions;

        CharacterState cs1P = _playerData1P.CharacterState;
        CharacterState cs2P = _playerData2P.CharacterState;

        //敵設定
        ca1P.InitializeCA(1, ca2P);
        ca2P.InitializeCA(2, ca1P);
        //死亡デリゲート
        ca1P.OnDie = KO;
        ca2P.OnDie = KO;
        //カメラ設定
        _camera.GetComponent<FightingCameraManager>().InitializeCamera(ca1P.transform,  ca2P.transform);
        //UI設定
        _fightingUI.SetPlayer(playerData1P, playerData2P);
        _fightingUI.HeartLost(_currentRoundData);
        _fightingUI.SetTimeLimitText(_timeLimit.ToString("D2"));
        _effectManager.InitializeFEM(ca1P, ca2P);
        //ラウンドコール
        cs1P.SetAcceptOperations(false);
        cs2P.SetAcceptOperations(false);
        RoundCall();

        //座標リセット
        ca1P.transform.position = new Vector2(_startPosX1P, StageParameter.GroundPosY);
        ca1P.transform.position += new Vector3(ca1P.PushBackBoxOffset.x, 0);
        ca2P.transform.position = new Vector2(_startPosX2P, StageParameter.GroundPosY);
        ca2P.transform.position -= new Vector3(ca2P.PushBackBoxOffset.x, 0);
    }

    private async void RoundCall()
    {
        await _fightingUI.RoundCall(CurrentRoundData.RoundNum);
        _playerData1P.CharacterState.SetAcceptOperations(true);
        if(!TitleManager.SoloPlayDebug)
        {
            _playerData2P.CharacterState.SetAcceptOperations(true);
        }
        CountdownAsync().Forget();
    }

    private async UniTaskVoid CountdownAsync()
    {
        _timeLimitCTS = new CancellationTokenSource();
        CancellationToken token = _timeLimitCTS.Token;

        int time = _timeLimit;

        try
        {
            while (time >= 0)
            {
                _fightingUI.SetTimeLimitText(time.ToString("D2")); // 2桁表示

                await FightingPhysics.DelayFrameWithTimeScale(
                    FightingPhysics.FightingFrameRate,
                    cancellationToken: token
                    );

                time--;
            }

            await RoundSetPerformance(_fightingUI.TimeOver);

            if (_playerData1P.CharacterState.CurrentHP > _playerData2P.CharacterState.CurrentHP)
            {
                GoNextRound(2);
            }
            else if (_playerData1P.CharacterState.CurrentHP < _playerData2P.CharacterState.CurrentHP)
            {
                GoNextRound(1);
            }
            else
            {
                GoNextRound(0);
            }
        }
        catch(OperationCanceledException)
        {
            //カウントダウン停止
        }
    }

    private async UniTask RoundSetPerformance(Func<UniTask> performance)
    {
        FightingPhysics.SetFightTimeScale(0.5f);
        Time.timeScale = 0.5f;
        _playerData1P.CharacterState.SetAcceptOperations(false);
        _playerData2P.CharacterState.SetAcceptOperations(false);

        await performance.Invoke();

        FightingPhysics.SetFightTimeScale(1);
        Time.timeScale = 1;
        _playerData1P.CharacterState.SetAcceptOperations(true);
        _playerData2P.CharacterState.SetAcceptOperations(true);
    }

    private async void KO(int loserNum)
    {
        _timeLimitCTS?.Cancel();
        await RoundSetPerformance(_fightingUI.KO);
        GoNextRound(loserNum);
    }

    private async void GoNextRound(int loserNum)
    {
        if(loserNum != 0)
        {
            _currentRoundData.RoundNum++;
        }    

        if(loserNum == 2)
        {
            _currentRoundData.Heart2P--;
            if(_currentRoundData.Heart2P <= 0)
            {
                GameSet(1);
                return;
            }
        }
        else if(loserNum == 1)
        {
            _currentRoundData.Heart1P--;
            if (_currentRoundData.Heart1P <= 0)
            {
                GameSet(2);
                return;
            }
        }

        FightingManager fightingManager =
            await GameManager.LoadAsync<FightingManager>("FightingScene");
        fightingManager.StartRound(CurrentRoundData, _playerData1P, _playerData2P);
    }

    private async void GameSet(int winnerNum)
    {
        _fightingUI.HeartLost(_currentRoundData);

        _playerData1P.CharacterState.SetAcceptOperations(false);
        _playerData2P.CharacterState.SetAcceptOperations(false);

        await _fightingUI.KO();

        if(_playerData1P.CharacterActions.gameObject != null)
        {
            Destroy(_playerData1P.CharacterActions.gameObject);
        }
        if (_playerData2P.CharacterActions.gameObject != null)
        {
            Destroy(_playerData2P.CharacterActions.gameObject);
        }

        //ResultSelectSceneに移動
        ResultManager resultManager =
            await GameManager.LoadAsync<ResultManager>("ResultScene");
        resultManager.InitializeRM(winnerNum, _playerData1P, _playerData2P);
    }

    private void OnDestroy()
    {
        if(_timeLimitCTS != null)
        {
            _timeLimitCTS.Cancel();
        }
    }
}

public struct PlayerData
{
    public CharacterData CharacterData { get; private set; }
    public CharacterActions CharacterActions { get; private set; }
    public CharacterState CharacterState { get; private set; }
    public int PlayerNum { get; private set; }

    public PlayerData(CharacterData cd, CharacterActions ca, CharacterState cs, int pn)
    {
        CharacterData = cd;
        CharacterActions = ca;
        CharacterState = cs;
        PlayerNum = pn;
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
