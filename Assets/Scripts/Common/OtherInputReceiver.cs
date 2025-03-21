using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class OtherInputReceiver : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputUser _inputUser;

    //デバイス切り替えデリゲート
    public UnityAction<InputDevice> SwitchDevice { get; set; }

    //入力デリゲート
    public UnityAction Accept { get; set; }
    public UnityAction Cancel { get; set; }
    public UnityAction Up { get; set; }
    public UnityAction Down { get; set; }
    public UnityAction Left { get; set; }
    public UnityAction Right { get; set; }

    //長押し関連
    private CancellationTokenSource _crossButtonCTS;
    private int _deleyFrameValue = 1; 

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputUser = _playerInput.user;

        // 入力イベントを監視
        InputSystem.onEvent += OnInputEvent;

        _crossButtonCTS = new CancellationTokenSource();
        LookInputCrossButton(_crossButtonCTS.Token);
    }

    public void RemoveDelegate()
    {
        InputSystem.onEvent -= OnInputEvent;
        Accept = null;
        Cancel = null;
        Up = null;
        Down = null;
        Left = null;
        Right = null;
        Debug.Log("イベント消した");
    }

    private void OnDestroy()
    {
        if(_crossButtonCTS != null)
        {
            _crossButtonCTS.Cancel();
        }

        RemoveDelegate();
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // キャラセレと対戦中はデバイスの切り替え不可
        if (SceneManager.GetActiveScene().name == "CharacterSelectScene" ||
           SceneManager.GetActiveScene().name == "FightingScene" ||
           SceneManager.GetActiveScene().name == "VersusScene")
        {
            return;
        }

        // すでに登録されているデバイスなら何もしない
        if (_inputUser.pairedDevices.Contains(device)) return;

        // キーボードとパッドだけ
        if (!(device is Keyboard) && !(device is Gamepad)) return;

        Debug.Log($"新しいデバイスの入力検知: {device.displayName}");

        // 既存のデバイスを解除
        _inputUser.UnpairDevices();

        // 新しいデバイスをペアリング
        _inputUser = InputUser.PerformPairingWithDevice(device, _inputUser);

        // PlayerInput の制御スキームを更新
        _playerInput.SwitchCurrentControlScheme(device);

        Debug.Log($"デバイスを {device.displayName} に切り替えました");

        if(_playerInput.playerIndex == 1)
        {
            GameManager.Player1Device = device;
        }
        else
        {
            GameManager.Player2Device = device;
        }
    }

    public void OnAccept()
    {
        Accept?.Invoke();
    }

    public void OnCancel()
    {
        Cancel?.Invoke();
    }

    private async void LookInputCrossButton(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Vector2 inputValue = _playerInput.actions["FourDirections"].ReadValue<Vector2>();
            OnCrossButton(inputValue);
            if(_deleyFrameValue <= 0)
            {
                _deleyFrameValue = 1;
            }
            try
            {
                await UniTask.DelayFrame(_deleyFrameValue, cancellationToken: token);
            }
            catch
            {
                break;
            }
        }
    }

    private void OnCrossButton(Vector2 direction)
    {
        int newDeleyFrame = 10;

        if(direction == Vector2.zero)
        {
            _deleyFrameValue = 1;
            return;
        }

        if(direction == Vector2.up)
        {
            if(Up != null)
            {
                _deleyFrameValue = newDeleyFrame;
                Up.Invoke();
            }
        }
        if(direction == Vector2.down)
        {
            if (Down != null)
            {
                _deleyFrameValue = newDeleyFrame;
                Down.Invoke();
            }
        }
        if(direction == Vector2.left)
        {
            if (Left != null)
            {
                _deleyFrameValue = newDeleyFrame;
                Left.Invoke();
            }
        }
        if(direction == Vector2.right)
        {
            if (Right != null)
            {
                _deleyFrameValue = newDeleyFrame;
                Right.Invoke();
            }
        }
    }
}
