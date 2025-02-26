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

    //���̓f���Q�[�g
    public UnityAction Accept { get; set; }
    public UnityAction Cancel { get; set; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputUser = _playerInput.user;
        DontDestroyOnLoad(gameObject);

        // ���̓C�x���g���Ď�
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
        // �L�����Z���Ƒΐ풆�̓f�o�C�X�̐؂�ւ��s��
        if(SceneManager.GetActiveScene().name == "CharacterSelectScene" ||
           SceneManager.GetActiveScene().name == "FightingScene")
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
