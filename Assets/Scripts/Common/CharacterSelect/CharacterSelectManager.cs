using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CharacterSelectManager : ModeManager
{

    [SerializeField] private CharacterSelectController _csc1P;
    [SerializeField] private CharacterSelectController _csc2P;

    private CancellationTokenSource _goFightingCTS;

    //デバッグ
    [SerializeField] private CharacterData _lancer;
    [SerializeField] private CharacterData _succubus;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);

        SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _csc1P);

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
        SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _csc2P);
    }

    //PlayerInputのデリゲート設定
    private void SetDelegate(OtherInputReceiver oir, CharacterSelectController csc)
    {
        oir.Accept = csc.Accept;
        oir.Cancel = csc.Cancel;
    }

    private async void GoFighting()
    {
        _goFightingCTS = new CancellationTokenSource();
        CancellationToken token = _goFightingCTS.Token;

        await UniTask.WaitUntil(() =>
        {
            return _csc1P.Selected && _csc2P.Selected;
        }, cancellationToken : token);

        //FightingSceneに移行
        var vm = await GameManager.LoadAsync<VersusManager>("VersusScene");
        vm.VersusPerformance(_lancer, _succubus);
    }
}
