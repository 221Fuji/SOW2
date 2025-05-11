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
    [SerializeField] protected UICSSkillListCtrl _csSkillListCtrl1P;
    [SerializeField] protected UICSSkillListCtrl _csSkillListCtrl2P;

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
    protected void SetDelegate(OtherInputReceiver oir, UIMovingCtrl movingCtrl)
    {
        //�R���g���[���[���̃f���Q�[�g = (���������ėǂ��ق���)�J�n�ҋ@��Ԃɂ��郁�\�b�h(�����ɂ���(bool)Selected���Ď�)
        oir.Accept = movingCtrl.OnClick;
        oir.Up = movingCtrl.ForcusUp;
        oir.Down =  movingCtrl.ForcusDown;
        oir.Left = movingCtrl.ForcusLeft;
        oir.Right = movingCtrl.ForcusRight;
        oir.Interact = movingCtrl.SwitchtoOtherCtrler;

        if(movingCtrl is UICSMovingCtrl uiCSMoving)
        {
            oir.Cancel = uiCSMoving.Cancel;
            if(uiCSMoving.OutMap[0].ReturnList()[0] is UICSReturnBack goBack)
            {
                goBack.ClickedActionEvent += GoTitle;
            }
        }
    }

    /// <summary>
    /// �X�L���ڍחp
    /// </summary>
    protected abstract void SwitchDelegate(UIMovingCtrl movingCtrl,int playerNum);
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
