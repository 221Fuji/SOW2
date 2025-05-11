using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMatchRM : ResultManager
{
    [SerializeField] protected UIRMovingCtrl _uirMovingCtrl2P;
    public override void InitializeRM(int winnerNum, PlayerData pd1, PlayerData pd2)
    {
        _uirMovingCtrl1P.ActivateThis();
        _uirMovingCtrl2P.ActivateThis();
        base.InitializeRM(winnerNum, pd1, pd2);
        InstantiatePlayer2Input(GameManager.Player2Device);
        SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _uirMovingCtrl2P);
    }

    protected override async void GoCharaSelect()
    {
        try
        {
            var characterSelectManager =
                await GameManager.LoadAsync<LocalMatchCSM>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);
        }
        catch
        {
            return;
        }
    }

    protected override async void GoFighting()
    {
        try
        {
            var fightingManager =
                await GameManager.LoadAsync<LocalMatchFM>("FightingScene");
            fightingManager.InitializeFM(_playerData1P.CharacterData, _PlayerData2P.CharacterData);
        }
        catch
        {
            return;
        }
    }
}
