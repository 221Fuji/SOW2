using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResultManager : ModeManager
{
    [SerializeField] protected UIRMovingCtrl _uirMovingCtrl1P;
    [SerializeField] protected ResultPerformance _resultPerformance;
    protected PlayerData _playerData1P;
    protected PlayerData _PlayerData2P;

    public virtual async void InitializeRM(int winnerNum, PlayerData pd1, PlayerData pd2)
    {
        _playerData1P = pd1;
        _PlayerData2P = pd2;
        _resultPerformance.WinPerformance(winnerNum == 1 ? pd1 : pd2);

        await UniTask.WaitUntil(() => { return _resultPerformance.IsCompletedPerformance; });

        Initialize(GameManager.Player1Device);
        SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _uirMovingCtrl1P);

        /*
        try
        {
            Debug.Log("read");
            int num = new StatisticsLogs().ArrangeResult(winnerNum == 1 ? pd1.CharacterData : pd2.CharacterData, winnerNum == 2 ? pd1.CharacterData : pd2.CharacterData);
            new StatisticsLogs().WritingTxt(num);
        }
        catch(Exception e)
        {
            Debug.Log("error");
            Debug.Log(e);
        }
        */
    }

    //PlayerInputÇÃÉfÉäÉQÅ[Égê›íË
    protected void SetDelegate(OtherInputReceiver oir, UIRMovingCtrl rMovingCtrl)
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

    protected abstract void GoCharaSelect();

    protected abstract void GoFighting();
}
