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

    private CharacterActions _player1CA;
    private CharacterActions _player2CA;
    private CharacterState _player1CS;
    private CharacterState _player2CS;
    private RoundData _currentRoundData;
    private CancellationTokenSource _timeLimitCTS;

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
        _player1CS = player1CA.GetComponent<CharacterState>();
        _player2CS = player2CA.GetComponent<CharacterState>();

        //敵設定
        _player1CA.InitializeCA(1, _player2CA);
        _player2CA.InitializeCA(2, _player1CA);
        //死亡デリゲート
        _player1CA.OnDie = KO;
        _player2CA.OnDie = KO;
        //カメラ設定
        _camera.GetComponent<FightingCameraManager>().InitializeCamera(_player1CA.transform, _player2CA.transform);
        //UI設定
        _fightingUI.SetPlayer(_player1CS, _player2CS);
        _fightingUI.HeartLost(_currentRoundData);
        _fightingUI.SetTimeLimitText(_timeLimit.ToString("D2"));
        _effectManager.InitializeFEM(_player1CA, _player2CA);
        //ラウンドコール
        _player1CS.SetAcceptOperations(false);
        _player2CS.SetAcceptOperations(false);
        RoundCall();

        //座標リセット
        _player1CA.transform.position = new Vector2(_startPosX1P, StageParameter.GroundPosY);
        _player1CA.transform.position += new Vector3(_player1CA.PushBackBoxOffset.x, 0);
        _player2CA.transform.position = new Vector2(_startPosX2P, StageParameter.GroundPosY);
        _player2CA.transform.position -= new Vector3(_player2CA.PushBackBoxOffset.x, 0);
    }

    private async void RoundCall()
    {
        await _fightingUI.RoundCall(CurrentRoundData.RoundNum);
        _player1CS.SetAcceptOperations(true);
        if(!TitleManager.SoloPlayDebug)
        {
            _player2CS.SetAcceptOperations(true);
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

            if (_player1CS.CurrentHP > _player2CS.CurrentHP)
            {
                GoNextRound(2);
            }
            else if (_player1CS.CurrentHP < _player2CS.CurrentHP)
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
        _player1CS.SetAcceptOperations(false);
        _player2CS.SetAcceptOperations(false);

        await performance.Invoke();

        FightingPhysics.SetFightTimeScale(1);
        Time.timeScale = 1;
        _player1CS.SetAcceptOperations(true);
        _player2CS.SetAcceptOperations(true);
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
        fightingManager.StartRound(CurrentRoundData, _player1CA, _player2CA);
    }

    private async void GameSet(int winnerNum)
    {
        _fightingUI.HeartLost(_currentRoundData);

        _player1CS.SetAcceptOperations(false);
        _player2CS.SetAcceptOperations(false);

        await _fightingUI.KO();

        _player1CS.SetAcceptOperations(true);
        _player2CS.SetAcceptOperations(true);

        Debug.Log($"Player{winnerNum}の勝ち！");

        //ModeSelectSceneに移動
        ResultManager resultManager =
            await GameManager.LoadAsync<ResultManager>("ResultScene");
        resultManager.InitializeRM(winnerNum);
    }

    private void OnDestroy()
    {
        if(_timeLimitCTS != null)
        {
            _timeLimitCTS.Cancel();
        }
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
