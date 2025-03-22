using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModeSelectManager : ModeManager
{
    [SerializeField] private UIMSMovingCtrl _uimsMovigCtrl;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);

        OtherInputReceiver oir = _player1Input.gameObject.GetComponent<OtherInputReceiver>();
        SetDelegate(oir);
    }

    private void SetDelegate(OtherInputReceiver oir)
    {
        //入力の設定
        oir.Accept = _uimsMovigCtrl.OnClick;
        oir.Cancel = GoTitle;
        oir.Up = _uimsMovigCtrl.ForcusUp;
        oir.Down = _uimsMovigCtrl.ForcusDown;

        //セレクトモードボタンの設定
        UIMSButton offline = _uimsMovigCtrl.OutMap[0].ReturnList()[1] as UIMSButton;
        offline.ClickedActionEvent = GoCharacterSelect;
    }

    private async void GoTitle()
    {
        await GameManager.LoadAsync<TitleManager>("TitleScene");
    }

    private async void GoCharacterSelect(GameObject ob)
    {
        ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);
        var kettei = movingCtrlClass?.ReturnKettei();
        kettei.StartAnim();

        try
        {
            var characterSelectManager =
                await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);
        }
        catch
        {
            return;
        }
    }
}
