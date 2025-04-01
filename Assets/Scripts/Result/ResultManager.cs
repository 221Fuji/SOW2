using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultManager : ModeManager
{
    [SerializeField] private UIRMovingCtrl _uirMovingCtrl1P;
    [SerializeField] private UIRMovingCtrl _uirMovingCtrl2P;
    [SerializeField] private ResultPerformance _resultPerformance;
    private PlayerData _playerData1P;
    private PlayerData _PlayerData2P;

    public async void InitializeRM(int winnerNum, PlayerData pd1, PlayerData pd2)
    {
        _playerData1P = pd1;
        _PlayerData2P = pd2;
        _resultPerformance.WinPerformance(winnerNum == 1 ? pd1 : pd2);

        await UniTask.WaitUntil(() => { return _resultPerformance.IsCompletedPerformance; });

        Initialize(GameManager.Player1Device);
        if (GameManager.Player2Device == null) Debug.Log("こうしーランド開園");
        InstantiatePlayer2Input(GameManager.Player2Device);

        SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _uirMovingCtrl1P);
        SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _uirMovingCtrl2P);
    }

    //PlayerInputのデリゲート設定
    private void SetDelegate(OtherInputReceiver oir, UIRMovingCtrl rMovingCtrl)
    {
        oir.Accept = rMovingCtrl.OnClick;
        oir.Cancel = rMovingCtrl.Cancell;
        oir.Up = rMovingCtrl.ForcusUp;
        oir.Down = rMovingCtrl.ForcusDown;
        oir.Left = rMovingCtrl.ForcusLeft;
        oir.Right = rMovingCtrl.ForcusRight;

        UIRButton goTitle = rMovingCtrl.OutMap[0].ReturnList()[2] as UIRButton;
        goTitle.ClickedActionEvent = GoTitle;
        UIRButton goCharaSelect = rMovingCtrl.OutMap[0].ReturnList()[1] as UIRButton;
        goCharaSelect.ClickedActionEvent = GoCharaSelect;
        UIRButton goFighting = rMovingCtrl.OutMap[0].ReturnList()[0] as UIRButton;
        goFighting.ClickedActionEvent = GoFighting;
    }

    private async void GoTitle()
    {
        try
        {
            await GameManager.LoadAsync<TitleManager>("TitleScene");
        }
        catch
        {
            return;
        }
    }

    private async void GoCharaSelect()
    {
        try
        {
            var characterSelectManager =
                await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);
        }
        catch
        {
            return;
        }
    }

    private async void GoFighting()
    {
        try
        {
            var fightingManager =
                await GameManager.LoadAsync<FightingManager>("FightingScene");
            fightingManager.InitializeFM(_playerData1P.CharacterData, _PlayerData2P.CharacterData);
        }
        catch
        {
            return;
        }
    }
}
