using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// それぞれのシーンのManager
/// </summary>
public class ModeManager : MonoBehaviour
{
    [SerializeField] protected PlayerInput _playerInputPrefab;
    protected PlayerInput _player1Input = null;
    protected PlayerInput _player2Input = null;

    public virtual void Initialize(InputDevice device)
    {
        string scheme = GameManager.GetControlSchemeFromDevice(_playerInputPrefab, device);

        _player1Input = PlayerInput.Instantiate(
            prefab: _playerInputPrefab.gameObject,
            playerIndex: 1,
            controlScheme: scheme,
            pairWithDevice: device
            );

        GameManager.Player1Device = device;
    }

    private void OnDestroy()
    {
        if (_player1Input == null) return;

        //inputRecieverのデリゲートの設定
        OtherInputReceiver oir = _player1Input.gameObject.GetComponent<OtherInputReceiver>();

        oir.RemoveDelegate();
    }
}
