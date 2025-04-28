using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocalMatchManager : FightingManager
{
    [SerializeField] private RunLogs _runLogs;

    private BattleInfoLog _battleInfoLog;
    protected async override void GoFighting()
    {
        LocalMatchManager localMatchManager =
            await GameManager.LoadAsync<LocalMatchManager>("FightingScene");
        localMatchManager.StartRound(CurrentRoundData, _playerData1P, _playerData2P);
    }

    protected override void StartRound(RoundData roundData, PlayerData playerData1P, PlayerData playerData2P)
    {
        base.StartRound(roundData, playerData1P, playerData2P);
        KinokoLogger();
    }

    protected override void GameSet(int winnerNum)
    {
        base.GameSet(winnerNum);
        KinokoLoggerEnd();
    }

    /// <summary>
    /// きのこの行動ログ用メソッド
    /// </summary>
    public void KinokoLogger()
    {
        MovingLog _moving1 = _playerData1P.CharacterActions.gameObject.GetComponent<MovingLog>();
        MovingLog _moving2 = _playerData2P.CharacterActions.gameObject.GetComponent<MovingLog>();
        BattleInfoLog infoLog = new BattleInfoLog();
        _battleInfoLog = infoLog;
        _moving1?.SetBattleInfoLog(infoLog);
        _moving2?.SetBattleInfoLog(infoLog);
    }

    public void KinokoLoggerEnd()
    {
        MovingLog _moving1 = _playerData1P.CharacterActions.gameObject.GetComponent<MovingLog>();
        MovingLog _moving2 = _playerData2P.CharacterActions.gameObject.GetComponent<MovingLog>();
        _moving1.RegisterLogs();
        _moving2.RegisterLogs();
        _battleInfoLog.ArrangeForFile();
    }
}
