using UnityEngine;
using UnityEngine.InputSystem;

public class ModeSelectManager : MonoBehaviour
{
    private PlayerInput _playerInput = null;

    public void InitializeMSM(PlayerInput playerInput)
    {
        _playerInput = playerInput;

        //inputReciever‚ÌƒfƒŠƒQ[ƒg‚Ìİ’è
        OtherInputReceiver oir = _playerInput.gameObject.GetComponent<OtherInputReceiver>();

        //‚ ‚Æ‚ÅC³
        oir.Accept += GoCharacterSelect;
    }

    private void OnDestroy()
    {
        if (_playerInput == null) return;

        //inputReciever‚ÌƒfƒŠƒQ[ƒg‚Ìİ’è
        OtherInputReceiver oir = _playerInput.gameObject.GetComponent<OtherInputReceiver>();

        //‚ ‚Æ‚ÅC³
        oir.RemoveDelegate();
    }

    private async void GoCharacterSelect()
    {
        var characterSelectManager =
            await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
        characterSelectManager.InitializeCSM(_playerInput);
    }
}
