using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ModeSelectManager : ModeManager
{
    [SerializeField] private UIMSMovingCtrl _uimsMovigCtrl;
    [SerializeField] private Image _panel;

    private CancellationTokenSource _fadeCTS;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);

        OtherInputReceiver oir = _player1Input.gameObject.GetComponent<OtherInputReceiver>();
        SetDelegate(oir);
        WaitForFade(Color.white, 0).Forget();
    }

    private void SetDelegate(OtherInputReceiver oir)
    {
        //入力の設定
        oir.Accept += _uimsMovigCtrl.OnClick;
        oir.Cancel += GoTitle;
        oir.Cancel += DoNotAcceptOperations; 
        oir.Up = _uimsMovigCtrl.ForcusUp;
        oir.Down = _uimsMovigCtrl.ForcusDown;

        //セレクトモードボタンの設定
        UIMSReturnBack goBack = _uimsMovigCtrl.OutMap[0].ReturnList()[0] as UIMSReturnBack;
        goBack.ClickedActionEvent += GoTitle;
        goBack.ClickedActionEvent += DoNotAcceptOperations;
        UIMSButton cpuMatch = _uimsMovigCtrl.OutMap[0].ReturnList()[1] as UIMSButton;
        UIMSButton localMatch = _uimsMovigCtrl.OutMap[0].ReturnList()[2] as UIMSButton;
        //
        cpuMatch.ClickedActionEvent = GoCPUMatchCS;
        localMatch.ClickedActionEvent = GoLocalMatchCS;
    }

    private void DoNotAcceptOperations()
    {
        _player1Input.GetComponent<OtherInputReceiver>().SetAcceptOpelation(false);
    }

    private async UniTask WaitForFade(Color startPanelColor, float endValue)
    {
        _fadeCTS = new CancellationTokenSource();
        _panel.color = startPanelColor;

        try
        {
            await _panel.DOFade(endValue, 0.5f).ToUniTask(cancellationToken: _fadeCTS.Token);
        }
        catch(OperationCanceledException)
        {
            return;
        }
    }

    private async void GoLocalMatchCS()
    {
        DoNotAcceptOperations();
        try
        {
            await WaitForFade(new Color(1, 1, 1, 0), 1);

            GameManager.Player2Device = null;

            var characterSelectManager =
                await GameManager.LoadAsync<LocalMatchCSM>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);

        }
        catch
        {
            if (_player1Input)
            {
                _player1Input.GetComponent<OtherInputReceiver>().SetAcceptOpelation(true);
            }
            return;
        }
    }

    private async void GoCPUMatchCS()
    {
        DoNotAcceptOperations();
        try
        {
            await WaitForFade(new Color(1, 1, 1, 0), 1);

            GameManager.Player2Device = null;

            var characterSelectManager =
                await GameManager.LoadAsync<CPUMatchCSM>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);

        }
        catch
        {
            if(_player1Input)
            {
                _player1Input.GetComponent<OtherInputReceiver>().SetAcceptOpelation(true);
            }
            return;
        }
    }

    private async void GoTitle()
    {
        DoNotAcceptOperations();
        try
        {
            await WaitForFade(new Color(1, 1, 1, 0), 1);
            await GameManager.LoadAsync<TitleManager>("TitleScene");
        }
        catch
        {
            if (_player1Input)
            {
                _player1Input.GetComponent<OtherInputReceiver>().SetAcceptOpelation(true);
            }
            return;
        }
    }
}
