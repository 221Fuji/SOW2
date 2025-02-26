using System.Linq;
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

    //入力デリゲート
    public UnityAction Accept { get; set; }
    public UnityAction Cancel { get; set; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputUser = _playerInput.user;
        DontDestroyOnLoad(gameObject);

        // 入力イベントを監視
        InputSystem.onEvent += OnInputEvent;
    }

    public void RemoveDelegate()
    {
        InputSystem.onEvent -= OnInputEvent;
        Accept = null;
        Cancel = null;
    }

    private void OnDestroy()
    {

    }

    private void OnApplicationQuit()
    {
        Destroy(gameObject);
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // キャラセレと対戦中はデバイスの切り替え不可
        if(SceneManager.GetActiveScene().name == "CharacterSelectScene" ||
           SceneManager.GetActiveScene().name == "FightingScene")
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
    }

    public void OnAccept()
    {
        Debug.Log("Accept");
        Accept?.Invoke();
    }

    public void OnCancel()
    {
        Debug.Log("Cancel");
        Cancel?.Invoke();
    }
}
