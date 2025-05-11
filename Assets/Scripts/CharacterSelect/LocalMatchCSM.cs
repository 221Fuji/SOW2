using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class LocalMatchCSM : CharacterSelectManager
{
    protected OtherInputReceiver _oir2P;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);
        _csMovingCtrl1P.SetCharaData(0);
        _csMovingCtrl1P.SwitchDelegate += SwitchDelegate;
        _csSkillListCtrl1P.SwitchDelegate += SwitchDelegate;

        if (GameManager.Player2Device != null)
        {
            InstantiatePlayer2Input(GameManager.Player2Device);
            _oir2P = _player2Input.GetComponent<OtherInputReceiver>();
            SetDelegate(_oir2P, _csMovingCtrl2P);
            _csMovingCtrl2P.SwitchDelegate += SwitchDelegate;
            _csSkillListCtrl2P.SwitchDelegate += SwitchDelegate;
        }
        InputSystem.onEvent += OnInput2P;
    }

    //2P側のデバイス検知
    private void OnInput2P(InputEventPtr eventPtr, InputDevice device)
    {
        // キーボードとパッドだけ
        if (!(device is Keyboard) && !(device is Gamepad) && !(device is Joystick)) return;

        if (GameManager.Player1Device == device || _player2Input != null) return;

        InstantiatePlayer2Input(device);
        Debug.Log("2P側のデバイスを登録" + _player2Input.devices);
        _csMovingCtrl2P.UiChanging();
        _oir2P = _player2Input.GetComponent<OtherInputReceiver>();
        SetDelegate(_oir2P, _csMovingCtrl2P);
        _csMovingCtrl2P.SwitchDelegate += SwitchDelegate;
        _csSkillListCtrl2P.SwitchDelegate += SwitchDelegate;
    }

    protected override void SwitchDelegate(UIMovingCtrl movingCtrl , int playerNum)
    {
        if(playerNum == 1)
        {
            _oir1P.RemoveDelegate();
            SetDelegate(_oir1P, movingCtrl);
        }
        else if (playerNum == 2)
        {
            _oir2P.RemoveDelegate();
            SetDelegate(_oir2P, movingCtrl);
        }
    }

    protected override async void GoFighting()
    {
        _goFightingCTS = new CancellationTokenSource();
        CancellationToken token = _goFightingCTS.Token;

        try
        {
            await UniTask.WaitUntil(() =>
            {
                return _csMovingCtrl1P.Selected && _csMovingCtrl2P.Selected;
            }, cancellationToken: token);

            CharacterData chara1P = _csMovingCtrl1P.CharacterData;
            CharacterData chara2P = _csMovingCtrl2P.CharacterData;

            await UniTask.WaitForSeconds(0.8f, cancellationToken: token);

            //FightingSceneに移行
            var vm = await GameManager.LoadAsync<VersusManager>("VersusScene");
            vm.InitializeLocalMode(chara1P, chara2P);
        }
        catch { }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InputSystem.onEvent -= OnInput2P;
    }
}
