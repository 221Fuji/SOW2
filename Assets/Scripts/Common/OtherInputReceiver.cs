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

    //�f�o�C�X�؂�ւ��f���Q�[�g
    public UnityAction<InputDevice> SwitchDevice { get; set; }

    //���̓f���Q�[�g
    public UnityAction Accept { get; set; }
    public UnityAction Cancel { get; set; }
    public UnityAction Up { get; set; }
    public UnityAction Down { get; set; }
    public UnityAction Left { get; set; }
    public UnityAction Right { get; set; }

    //�������֘A
    private CancellationTokenSource _crossButtonCTS;
    private int _deleyFrameValue = 1; 

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputUser = _playerInput.user;

        // ���̓C�x���g���Ď�
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
        Debug.Log("�C�x���g������");
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
        // �L�����Z���Ƒΐ풆�̓f�o�C�X�̐؂�ւ��s��
        if (SceneManager.GetActiveScene().name == "CharacterSelectScene" ||
           SceneManager.GetActiveScene().name == "FightingScene" ||
           SceneManager.GetActiveScene().name == "VersusScene")
        {
            return;
        }

        // ���łɓo�^����Ă���f�o�C�X�Ȃ牽�����Ȃ�
        if (_inputUser.pairedDevices.Contains(device)) return;

        // �L�[�{�[�h�ƃp�b�h����
        if (!(device is Keyboard) && !(device is Gamepad)) return;

        Debug.Log($"�V�����f�o�C�X�̓��͌��m: {device.displayName}");

        // �����̃f�o�C�X������
        _inputUser.UnpairDevices();

        // �V�����f�o�C�X���y�A�����O
        _inputUser = InputUser.PerformPairingWithDevice(device, _inputUser);

        // PlayerInput �̐���X�L�[�����X�V
        _playerInput.SwitchCurrentControlScheme(device);

        Debug.Log($"�f�o�C�X�� {device.displayName} �ɐ؂�ւ��܂���");

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
