using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Rendering;
using System;

public class CharacterSelectManager : ModeManager
{
    [SerializeField] private UICSMovingCtrl _csMovingCtrl1P;
    [SerializeField] private UICSMovingCtrl _csMovingCtrl2P;

    [SerializeField] private CharacterDataBase _characterDataBase;

    private CancellationTokenSource _goFightingCTS;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);

        if(GameManager.Player2Device != null)
        {
            InstantiatePlayer2Input(GameManager.Player2Device);
            SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _csMovingCtrl2P);
        }

        SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _csMovingCtrl1P);

        InputSystem.onEvent += OnInput2P;

        GoFighting();
    }

    //2P側のデバイス検知
    private void OnInput2P(InputEventPtr eventPtr, InputDevice device)
    {
        // キーボードとパッドだけ
        if (!(device is Keyboard) && !(device is Gamepad) && !(device is Joystick)) return;

        if (GameManager.Player1Device == device || _player2Input != null) return;

        InstantiatePlayer2Input(device);
        Debug.Log("2P側のデバイスを登録" + _player2Input.devices);
        _csMovingCtrl2P.ChangedIcon(GameManager.Player2Device);
        SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _csMovingCtrl2P);
    }

    //PlayerInputのデリゲート設定
    private void SetDelegate(OtherInputReceiver oir, UICSMovingCtrl csMovingCtrl)
    {
        //コントローラー側のデリゲート = (書き換えて良いほうの)開始待機状態にするメソッド(ここにある(bool)Selectedを監視)
        oir.Accept = csMovingCtrl.OnClick;
        oir.Cancel = csMovingCtrl.Cancell;
        oir.Up = csMovingCtrl.ForcusUp;
        oir.Down = csMovingCtrl.ForcusDown;
        oir.Left = csMovingCtrl.ForcusLeft;
        oir.Right = csMovingCtrl.ForcusRight;
        UICSReturnBack goBack = csMovingCtrl.OutMap[0].ReturnList()[0] as UICSReturnBack;
        goBack.ClickedActionEvent += GoTitle;
    }

    private async void GoFighting()
    {
        _goFightingCTS = new CancellationTokenSource();
        CancellationToken token = _goFightingCTS.Token;

        try
        {

            await UniTask.WaitUntil(() =>
            {
                return _csMovingCtrl1P.Selected && _csMovingCtrl2P.Selected;
            }, cancellationToken: token);

            CharacterData chara1P = _csMovingCtrl1P.CharacterData;
            CharacterData chara2P = _csMovingCtrl2P.CharacterData;

            await UniTask.WaitForSeconds(0.8f, cancellationToken: token);

            //FightingSceneに移行
            var vm = await GameManager.LoadAsync<VersusManager>("VersusScene");
            vm.VersusPerformance(chara1P, chara2P);
        }
        catch { }
    }

    private async void GoTitle()
    {
        _goFightingCTS?.Cancel();
        var modeSelectManager = await GameManager.LoadAsync<ModeSelectManager>("ModeSelectScene");
        modeSelectManager.Initialize(GameManager.Player1Device);
        //デバイスの管理追加処理書く
        GameManager.Player2Device = null;
    }

    private void OnDestroy()
    {
        InputSystem.onEvent -= OnInput2P;
        _goFightingCTS?.Cancel();
    }
}
