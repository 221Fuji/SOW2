using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceIdentification : MonoBehaviour
{

    [SerializeField] private InputAction _inputAction = default;
    // PlayerInput���A�^�b�`����Ă���v���C���[�I�u�W�F�N�g
    //�f�o�b�O�p
    [SerializeField] private PlayerInput _playerPrefab = default;
    // �ő�Q���l��
    [SerializeField] private int _maxPlayerCount;

    private InputDevice[] _joinedDevices = new InputDevice[2];
    private int _currentPlayerCount = 0;
    private GameObject[] _characters = new GameObject[2];

    private void Awake()
    {
        _inputAction.Enable();
        _inputAction.performed += OnJoin;
    }

    private void OnJoin(InputAction.CallbackContext context)
    {
        // Join�v�����̃f�o�C�X�����ɎQ���ς݂̂Ƃ��A�������I��
        foreach (var device in _joinedDevices)
        {
            if (context.control.device == device)
            {
                return;
            }
        }

        // PlayerInput�������������z�̃v���C���[���C���X�^���X��
        // ��Join�v�����̃f�o�C�X����R�Â��ăC���X�^���X�𐶐�����
        PlayerInput character = PlayerInput.Instantiate(
            prefab: _playerPrefab.gameObject,
            playerIndex: _currentPlayerCount,
            pairWithDevice: context.control.device
            );

        //character�̏���ۑ�
        _characters[_currentPlayerCount] = character.gameObject;

        // Join�����f�o�C�X����ۑ�
        _joinedDevices[_currentPlayerCount] = context.control.device;

        _currentPlayerCount++;

        //�f�o�b�O�p
        if(_currentPlayerCount == _maxPlayerCount)
        {
            //�G�ݒ�
            _characters[0].GetComponent<CharacterActions>().SetEnemy(_characters[1]);
            _characters[1].GetComponent<CharacterActions>().SetEnemy(_characters[0]);
        }
    }
}
