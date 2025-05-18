using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GenieHydra : CharacterActions
{
    [Header("�ʏ�U��")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private HitBoxManager _normalMoveHitBox;
    [Header("�W�����v�U��")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("�K�E�Z�P")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Vector2 _sm1Direction;
    [SerializeField] private HitBoxManager _specialMove1HitBox;
    [Header("�K�E�Z2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private HitBoxManager _specialMove2HitBox;
    [Header("���K�E�Z")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private AttackInfo _ultBulletInfo;
    [SerializeField] private GameObject _ultBullet;
    private HitBoxManager _ultHitBox;

    private int _jumpMoveCount = 0; //�P��̃W�����v�ōs�����W�����v�U���̉�
    private int _sm1Count = 0; //sm1�̉�

    //�e�s����CancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _ultCTS;
    private CancellationTokenSource _ultBulletCTS;

    //�s�������̐ݒ�
    public override bool CanEveryAction
    {
        get
        {
            return base.CanEveryAction
                && _normalMoveCTS == null
                && _jumpMoveCTS == null
                && _specialMove1CTS == null
                && _specialMove2CTS == null
                && _ultCTS == null;
        }
    }
    public bool CanNormalMove
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;

            return true;
        }
    }
    public bool CanJumpMove
    {
        get
        {
            if (!CanNormalMove) return false;
            if (_jumpMoveCount != 0) return false;
            return true;
        }
    }
    public bool CanSpecialMove1
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;
            if (!OnGround)
            {
                if (_sm1Count != 0) return false; //�󒆂ł͂P��̂�
            }

            return true;
        }
    }
    public bool CanSpecialMove2
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;
            if (!OnGround) return false; //�󒆕s��

            return true;
        }
    }
    public bool CanUltimate
    {
        get
        {
            if (!CanEveryAction || _characterState.CurrentUP < 100) return false;

            return true;
        }
    }

    protected override void SetActionDelegate()
    {
        _inputReciever.JumpDelegate = Jump;
        _inputReciever.GuardDelegate = GuardStance;
        _inputReciever.NormalMove = NormalMove;
        _inputReciever.SpecialMove1 = SpecialMove1;
        _inputReciever.SpecialMove2 = SpecialMove2;
        _inputReciever.Ultimate = Ultimate;
    }

    protected override void SetHitBox()
    {
        _normalMoveHitBox.InitializeHitBox(_normalMoveInfo, gameObject);
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, gameObject);
        _specialMove1HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        _specialMove1HitBox.Hit = HitSp1Move;
        _specialMove2HitBox.InitializeHitBox(_specialMove2Info, gameObject);
    }

    /// <summary>
    /// �ʏ�U��
    /// </summary>
    public async UniTask NormalMove()
    {
        if (!CanNormalMove) return;

        //�W�����v���Ȃ�W�����v�U���̏������s��
        if (!OnGround)
        {
            JumpMove().Forget();
            return;
        }

        // �V����CTS�𐶐�
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        //�A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //��������
        Velocity = new Vector2(0, Velocity.y);

        //SP����
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_normalMoveHitBox, _normalMoveInfo.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_normalMoveInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            Debug.Log("�ʏ�U�����L�����Z��");
            _normalMoveHitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _normalMoveCTS.Dispose();
            _normalMoveCTS = null;

            //layer�����ɖ߂�
            AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
        }
    }

    /// <summary>
    /// �W�����v�U��
    /// </summary>
    public async UniTask JumpMove()
    {
        //�W�����v�U���͋󒆂ň��̂�
        if (!CanJumpMove) return;

        //�W�����v�U�������̉񐔂��L�^
        _jumpMoveCount++;

        // �V����CTS�𐶐�
        _jumpMoveCTS = new CancellationTokenSource();
        CancellationToken token = _jumpMoveCTS.Token;

        // �A�j���[�V��������
        _animator.SetTrigger("JumpMoveTrigger");

        //SP����
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_jumpMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_jumpMoveInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_jumpMoveHitBox, _normalMoveInfo.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_jumpMoveInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            Debug.Log("�W�����v�U�����L�����Z��");
            _jumpMoveHitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _jumpMoveCTS.Dispose();
            _jumpMoveCTS = null;
        }
    }

    /// <summary>
    /// �K�E�Z�P
    /// </summary>
    public async UniTask SpecialMove1()
    {
        if (!CanSpecialMove1) return;

        //�󒆂ł͂P��̂�
        if (!OnGround)
        {
            _sm1Count++;
        }

        // �V����CTS�𐶐�
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // ������҂�

            //��������
            float sm1DirectionX = _sm1Direction.x * (_characterState.IsLeftSide ? 1 : -1);
            Velocity = Vector2.zero;

            AddForce(new Vector2(sm1DirectionX, _sm1Direction.y));

            await WaitForActiveFrame(_specialMove1HitBox, _specialMove1Info.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            _specialMove1HitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _specialMove1CTS.Dispose();
            _specialMove1CTS = null;
        }

        //layer�����ɖ߂�
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
    }

    //Sp1�����������Ƃ��ɌĂ΂��
    private void HitSp1Move()
    {
        AddForce(new Vector2(0, -10));
        Debug.Log("��������");
    }

    /// <summary>
    /// �K�E�Z�Q
    /// </summary>
    public async UniTask SpecialMove2()
    {
        if (!CanSpecialMove2) return;

        // �V����CTS�𐶐�
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("SpecialMove2Trigger");

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_specialMove2Info.ConsumptionSP);

        //UP���
        UPgain(_specialMove2Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove2Info.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_specialMove2HitBox, _specialMove2Info.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            _specialMove2HitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _specialMove2CTS.Dispose();
            _specialMove2CTS = null;
        }

        //layer�����ɖ߂�
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
    }

    /// <summary>
    /// ���K�E�Z
    /// </summary>
    public async UniTask Ultimate()
    {

        if (!CanUltimate) return;

        //UP����
        _characterState.SetCurrentUP(-100);

        // �V����CTS�𐶐�
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltTrigger");

        //��������
        Velocity = Vector2.zero;
        SetIsFixed(true);

        //���o
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);
        _characterState.SetIsUltPerformance();

        try
        {
            await StartUpMove(_ultimateInfo.StartupFrame, token); // ������҂�
            CreateUltBullet();
            await RecoveryFrame(_ultimateInfo.RecoveryFrame, token); // �d����҂�
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _ultCTS.Dispose();
            _ultCTS = null;

            //��������
            SetIsFixed(false);
        }

        //layer�����ɖ߂�
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    private void CreateUltBullet()
    {
        //UltBullet�̐������W
        float generatePosOffsetX = 2;
        float generatePosOffsetY = 1;
        //�q�b�g�o�b�N���̃x�N�g���C��
        AttackInfo attackInfo = _ultBulletInfo;

        //���͕����ɂ���Đݒ�
        if (WalkDirection() < 0)
        {
            generatePosOffsetX *= -1;
        }
        else
        {
            attackInfo.HitBackDirection *= new Vector2(-1, 1);
            attackInfo.GuardBackDirection *= new Vector2(-1, 1);
        }
        if (transform.position.x > EnemyCA.transform.position.x)
        {
            generatePosOffsetX *= -1;
        }
        float generatePosX = EnemyCA.transform.position.x + generatePosOffsetX;
        float generatePosY = EnemyCA.transform.position.y + generatePosOffsetY;
        Vector2 generatePos = new Vector2(generatePosX, generatePosY);

        //UltBullet�̌���
        Quaternion bulletQuaternion
            = new Quaternion(0, generatePosOffsetX > 0 ? 180 : 0, 0, 0);

        //UltBullet�̐���
        GameObject bullet = Instantiate(_ultBullet, generatePos, bulletQuaternion);
        _ultHitBox = bullet.GetComponentInChildren<HitBoxManager>();
        _ultHitBox?.InitializeHitBox(attackInfo, gameObject);

        // �V����CTS�𐶐�
        _ultBulletCTS = new CancellationTokenSource();
        CancellationToken token = _ultBulletCTS.Token;

        UltBullet(bullet, token).Forget();
    }

    private async UniTask UltBullet(GameObject bullet, CancellationToken token)
    {
        try
        {
            await StartUpMove(_ultBulletInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_ultHitBox, _ultBulletInfo.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_ultBulletInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            _ultHitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _ultBulletCTS.Dispose();
            _ultBulletCTS = null;

            //UltBullet�폜
            if (bullet != null)
            {
                Destroy(bullet);
                _ultHitBox = null;
            }
        }
    }

    //���n���ɃW�����v�U�����L�����Z��
    protected override void Land()
    {
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;
        _sm1Count = 0;
    }

    public override void CancelActionByHit()
    {
        _normalMoveCTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _jumpMoveCTS?.Cancel();
        //Ult�������̌Œ艻����
        SetIsFixed(false);
    }
}
