using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CharacterSelectManager : MonoBehaviour
{

    [SerializeField] private CharacterSelectController _csc1P;
    [SerializeField] private CharacterSelectController _csc2P;

    [SerializeField] private GameObject _playerPrefab;
    private PlayerInput _player1Input;
    private PlayerInput _player2Input;

    private CancellationTokenSource _goFightingCTS;

    //デバッグ
    [SerializeField] private CharacterData _lancer;
    [SerializeField] private CharacterData _succubus;

    public void InitializeCSM(PlayerInput playerInput)
    {
        //1Pの入力設定
        _player1Input = playerInput;
        SetDelegate(playerInput.GetComponent<OtherInputReceiver>(), _csc1P);

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

        //2Pのデバイス登録
        _player2Input = PlayerInput.Instantiate(
            prefab: _playerPrefab,
            playerIndex: 1, 
            pairWithDevice: device
            );
        Debug.Log("2P側のデバイスを登録" + _player2Input.devices);
        SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _csc2P);
    }

    //PlayerInputのデリゲート設定
    private void SetDelegate(OtherInputReceiver oir, CharacterSelectController csc)
    {
        oir.Accept = csc.Accept;
        oir.Cancel = csc.Cancel;
    }

    private void OnDestroy()
    {
        _player1Input.GetComponent<OtherInputReceiver>().RemoveDelegate();
        _player2Input.GetComponent<OtherInputReceiver>().RemoveDelegate();
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
        var fm = await GameManager.LoadAsync<FightingManager>("FightingScene");
        fm.InitializeFM(_player1Input, _lancer, _player2Input, _succubus);
    }
}
