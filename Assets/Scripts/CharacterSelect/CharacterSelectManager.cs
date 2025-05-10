using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Rendering;
using System;

public abstract class CharacterSelectManager : ModeManager
{
    [SerializeField] protected UICSMovingCtrl _csMovingCtrl1P;
    [SerializeField] protected UICSMovingCtrl _csMovingCtrl2P;

    [SerializeField] protected CharacterDataBase _characterDataBase;

    protected OtherInputReceiver _oir1P;
    protected CancellationTokenSource _goFightingCTS;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);
        _oir1P = _player1Input.GetComponent<OtherInputReceiver>();
        SetDelegate(_oir1P, _csMovingCtrl1P);
        GoFighting();
    }

    //PlayerInput�̃f���Q�[�g�ݒ�
    protected void SetDelegate(OtherInputReceiver oir, UICSMovingCtrl csMovingCtrl)
    {
        //�R���g���[���[���̃f���Q�[�g = (���������ėǂ��ق���)�J�n�ҋ@��Ԃɂ��郁�\�b�h(�����ɂ���(bool)Selected���Ď�)
        oir.Accept += csMovingCtrl.OnClick;
        oir.Cancel += csMovingCtrl.Cancel;
        oir.Up += csMovingCtrl.ForcusUp;
        oir.Down += csMovingCtrl.ForcusDown;
        oir.Left += csMovingCtrl.ForcusLeft;
        oir.Right += csMovingCtrl.ForcusRight;
        UICSReturnBack goBack = csMovingCtrl.OutMap[0].ReturnList()[0] as UICSReturnBack;
        goBack.ClickedActionEvent += GoTitle;
    }

    protected abstract void GoFighting();

    private async void GoTitle()
    {
        _goFightingCTS?.Cancel();
        var modeSelectManager = await GameManager.LoadAsync<ModeSelectManager>("ModeSelectScene");
        modeSelectManager.Initialize(GameManager.Player1Device);
        //�f�o�C�X�̊Ǘ��ǉ���������
        GameManager.Player2Device = null;
    }

    protected virtual void OnDestroy()
    {
        _goFightingCTS?.Cancel();
    }
}
