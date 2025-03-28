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
        oir.Accept += DoNotaAcceptOperations;
        oir.Cancel += GoTitle;
        oir.Cancel += DoNotaAcceptOperations; 
        oir.Up = _uimsMovigCtrl.ForcusUp;
        oir.Down = _uimsMovigCtrl.ForcusDown;

        //セレクトモードボタンの設定
        UIMSReturnBack goBack = _uimsMovigCtrl.OutMap[0].ReturnList()[0] as UIMSReturnBack;
        goBack.ClickedActionEvent += GoTitle;
        goBack.ClickedActionEvent += DoNotaAcceptOperations;
        UIMSButton offline = _uimsMovigCtrl.OutMap[0].ReturnList()[1] as UIMSButton;
        offline.ClickedActionEvent = GoCharacterSelect;
    }

    private void DoNotaAcceptOperations()
    {
        _player1Input.gameObject.GetComponent<OtherInputReceiver>().SetAcceptOpelation(false);
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

    private async void GoCharacterSelect()
    {
        try
        {
            await WaitForFade(new Color(1, 1, 1, 0), 1);

            GameManager.Player2Device = null;
            var characterSelectManager =
                await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);
        }
        catch
        {
            return;
        }
    }

    private async void GoTitle()
    {
        try
        {
            await WaitForFade(new Color(1, 1, 1, 0), 1);
            await GameManager.LoadAsync<TitleManager>("TitleScene");
        }
        catch
        {
            return;
        }
    }
}
