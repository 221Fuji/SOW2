using UnityEngine;
using UnityEngine.InputSystem;

public class ModeSelectManager : MonoBehaviour
{
    private PlayerInput _playerInput = null;

    public void InitializeMSM(PlayerInput playerInput)
    {
        _playerInput = playerInput;

        //inputReciever�̃f���Q�[�g�̐ݒ�
        OtherInputReceiver oir = _playerInput.gameObject.GetComponent<OtherInputReceiver>();

        //���ƂŏC��
        oir.Accept += GoCharacterSelect;
    }

    private void OnDestroy()
    {
        if (_playerInput == null) return;

        //inputReciever�̃f���Q�[�g�̐ݒ�
        OtherInputReceiver oir = _playerInput.gameObject.GetComponent<OtherInputReceiver>();

        //���ƂŏC��
        oir.RemoveDelegate();
    }

    private async void GoCharacterSelect()
    {
        var characterSelectManager =
            await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
        characterSelectManager.InitializeCSM(_playerInput);
    }
}
