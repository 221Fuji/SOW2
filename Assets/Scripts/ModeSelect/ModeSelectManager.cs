using UnityEngine;
using UnityEngine.InputSystem;

public class ModeSelectManager : ModeManager
{
    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);
        //inputReciever‚ÌƒfƒŠƒQ[ƒg‚Ìİ’è
        OtherInputReceiver oir = _player1Input.gameObject.GetComponent<OtherInputReceiver>();

        oir.Accept += GoCharacterSelect;
    }

    private async void GoCharacterSelect()
    {
        var characterSelectManager =
            await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
        characterSelectManager.Initialize(GameManager.Player1Device);
    }
}
