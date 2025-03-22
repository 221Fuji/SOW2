using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CharacterSelectManager : ModeManager
{
    [SerializeField] private UICSMovingCtrl _csMovingCtrl1P;
    [SerializeField] private UICSMovingCtrl _csMovingCtrl2P;

    [SerializeField] private CharacterDataBase _characterDataBase;

    private CancellationTokenSource _goFightingCTS;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);

        SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _csMovingCtrl1P);

        InputSystem.onEvent += OnInput2P;

        GoFighting();
    }

    //2P側のデバイス検知
    private void OnInput2P(InputEventPtr eventPtr, InputDevice device)
    {
        // キーボードとパッドだけ
        if (!(device is Keyboard) && !(device is Gamepad)) return;

        if (_player1Input.devices.Contains(device)||
            _player2Input != null) return;

        InstantiatePlayer2Input(device);
        Debug.Log("2P側のデバイスを登録" + _player2Input.devices);
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
    }

    private async void GoFighting()
    {
        _goFightingCTS = new CancellationTokenSource();
        CancellationToken token = _goFightingCTS.Token;

        await UniTask.WaitUntil(() =>
        {
            return _csMovingCtrl1P.Selected && _csMovingCtrl2P.Selected;
        }, cancellationToken : token);

        //
        //以下デバッグ用処理
        //

        CharacterData chara1P = _csMovingCtrl1P.CharacterData;
        CharacterData chara2P = _csMovingCtrl2P.CharacterData;

        //FightingSceneに移行
        var vm = await GameManager.LoadAsync<VersusManager>("VersusScene");
        vm.VersusPerformance(chara1P, chara2P);
    }

    private void OnDestroy()
    {
        InputSystem.onEvent -= OnInput2P;
    }
}
