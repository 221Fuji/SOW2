using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Necrom : CharacterActions
{
    [Header("�ʏ�U��")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private Bullet _nmBulletPrefab;
    [SerializeField] private float _nmBulletVelocity;
    [Header("�W�����v�U��")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("�K�E�Z�P")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Bullet _trapPrefab;
    [Header("�K�E�Z2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [Header("���K�E�Z")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private HitBoxManager _ultimateHitBox;

    private int _jumpMoveCount = 0; //�P��̃W�����v�ōs�����W�����v�U���̉�

    private Bullet _nmBullet;
    private List<Bullet> _sm1Traps = new List<Bullet>();
    private List<Bullet> _sm2Traps = new List<Bullet>();

    //�e�s����CancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _ultCTS;

    //�s�������̐ݒ�
    protected override bool CanEveryAction
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

    public override void InitializeCA(int playerNum, CharacterActions enemyCA)
    {
        base.InitializeCA(playerNum, enemyCA);
        _sm1Traps.Clear();
        _sm2Traps.Clear();
        _nmBullet = null;
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
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, gameObject);
        _ultimateHitBox.InitializeHitBox(_ultimateInfo, gameObject);
    }

    /// <summary>
    /// �ʏ�U��
    /// </summary>
    public async UniTask NormalMove()
    {
        //�U�����̏ꍇ
        if (!CanEveryAction) return;

        //��J���
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //�W�����v���Ȃ�W�����v�U���̏������s��
        if (!OnGround)
        {
            JumpMove().Forget();
            return;
        }

        if (_nmBullet != null) return;

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
            CreateNmBullet(token);
            await RecoveryFrame(_normalMoveInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            Debug.Log("�ʏ�U�����L�����Z��");
            NmBulletHit(_nmBullet);
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

    private async void CreateNmBullet(CancellationToken token)
    {
        //�e�̍��W�Ƒ��x�ݒ�
        Vector2 bulletVelocity = new Vector2(_nmBulletVelocity, 0);
        Vector2 bulletPosOffset = new Vector2(1, 1);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_nmBulletPrefab, bulletPos, rotation);
        bullet.Velocity = bulletVelocity;

        //�e�̓����蔻��ݒ�
        bullet.HitBox.InitializeHitBox(_normalMoveInfo, gameObject);
        bullet.HitBox.HitBullet = NmBulletHit;
        bullet.HitBox.GuardBullet = NmBulletHit;
        bullet.DestroyBullet = NmBulletHit;

        try
        {
            await WaitForActiveFrame(bullet.HitBox, _normalMoveInfo.ActiveFrame, token);
        }
        finally
        {
            NmBulletHit(bullet);
        }
    }

    private async void NmBulletHit(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.Velocity = Vector2.zero;
        bullet.GetComponent<Animator>().SetTrigger("NmHitTrigger");

        await FightingPhysics.DelayFrameWithTimeScale(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            _nmBullet = null;
        }
    }

    /// <summary>
    /// �W�����v�U��
    /// </summary>
    public async UniTask JumpMove()
    {
        //��J���
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //�W�����v�U���͋󒆂ň��̂�
        if (_jumpMoveCount != 0) return;

        //�W�����v�U�������̉񐔂��L�^
        _jumpMoveCount++;

        // �V����CTS�𐶐�
        _jumpMoveCTS = new CancellationTokenSource();
        CancellationToken token = _jumpMoveCTS.Token;

        // �A�j���[�V��������
        _animator.SetTrigger("JumpMoveTrigger");

        //SP����
        _characterState.SetCurrentSP(-_jumpMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_jumpMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_jumpMoveInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_jumpMoveHitBox, _jumpMoveInfo.ActiveFrame, token); // ������҂�
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
        if (!CanEveryAction) return;

        //��J���
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //�󒆕s��
        if (!OnGround) return;

        // �V����CTS�𐶐�
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // ������҂�
            CreateSm1Trap();
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // �d����҂�
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

    private async void CreateSm1Trap()
    {
        //�S�Ă�sm1Trap���폜
        while(_sm1Traps.Count >= 1)
        {
            SM1TrapLost(_sm1Traps[0]);
        }

        //�e�̍��W�Ƒ��x�ݒ�
        Vector2 trapPosOffset = new Vector2(3f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            trapPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        float trapPosX = transform.position.x + trapPosOffset.x;

        //��ʒ[�𒴂��Ȃ�
        trapPosX = Mathf.Clamp(trapPosX,
            StageParameter.CurrentLeftWallPosX + 1.5f,
            StageParameter.CurrentRightWallPosX - 1.5f);

        Vector2 trapPos = new Vector2(trapPosX, StageParameter.GroundPosY);
        Bullet sm1trap = Instantiate(_trapPrefab, trapPos, rotation);
        _sm1Traps.Add(sm1trap);

        //�e�̓����蔻��ݒ�
        sm1trap.HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        sm1trap.HitBox.HitBullet = SM1TrapBoot;
        sm1trap.HitBox.GuardBullet = SM1TrapBoot;
        sm1trap.DestroyBullet = SM1TrapLost;

        //�A�j���[�V����
        Animator trapAnimator = sm1trap.GetComponent<Animator>();
        AnimatorByLayerName.SetLayerWeightByName(trapAnimator, "Trap1Layer", 1);
        AnimatorByLayerName.SetLayerWeightByName(trapAnimator, "Trap2Layer", 0);
        trapAnimator.SetTrigger("SetTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(10);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        sm1trap.HitBox.SetIsActive(true);
    }

    private async void SM1TrapBoot(Bullet trap)
    {
        if (trap == null) return;

        trap.GetComponent<Animator>().SetTrigger("BootTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(30);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        SM1TrapLost(trap);
    }

    private async void SM1TrapLost(Bullet trap)
    {
        if(_sm1Traps.Contains(trap))
        {
            _sm1Traps.Remove(trap);
        }
        trap.GetComponent<Animator>().SetTrigger("LostTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(30);
        }
        catch
        {
            return;
        }

        if(trap != null)
        {
            Destroy(trap.gameObject);
        }
    }

    /// <summary>
    /// �K�E�Z�Q
    /// </summary>
    public async UniTask SpecialMove2()
    {
        //��J���
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //�󒆕s��
        if (!OnGround) return;

        if (!CanEveryAction) return;

        // �V����CTS�𐶐�
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_specialMove2Info.ConsumptionSP);

        //UP���
        UPgain(_specialMove2Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove2Info.StartupFrame, token); // ������҂�
            CreateSm2Trap();
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // �d����҂�
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _specialMove2CTS.Dispose();
            _specialMove2CTS = null;

            AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
        }
    }

    private async void CreateSm2Trap()
    {
        //�S�Ă�sm2Trap���폜
        while (_sm2Traps.Count >= 1)
        {
            SM2TrapLost(_sm2Traps[0]);
        }

        //�e�̍��W�Ƒ��x�ݒ�
        Vector2 trapPosOffset = new Vector2(4.5f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            trapPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        float trapPosX = transform.position.x + trapPosOffset.x;

        //��ʒ[�𒴂��Ȃ�
        trapPosX = Mathf.Clamp(trapPosX, 
            StageParameter.CurrentLeftWallPosX + 1.5f,
            StageParameter.CurrentRightWallPosX - 1.5f);

        Vector2 trapPos = new Vector2(trapPosX, StageParameter.GroundPosY);
        Bullet sm2trap = Instantiate(_trapPrefab, trapPos, rotation);
        _sm2Traps.Add(sm2trap);

        //�e�̓����蔻��ݒ�
        sm2trap.HitBox.InitializeHitBox(_specialMove2Info, gameObject);
        sm2trap.HitBox.HitBullet = SM2TrapBoot;
        sm2trap.HitBox.GuardBullet = SM2TrapBoot;
        sm2trap.DestroyBullet = SM2TrapLost;

        //�j��\�ɂ���
        HurtBoxManager trapHurtBox = sm2trap.GetComponentInChildren<HurtBoxManager>();
        trapHurtBox.OnHurtWithTransform = SM2Trap2Break;
        trapHurtBox.SetPlayerNum(PlayerNum);
        trapHurtBox.SetActive(true);

        //�A�j���[�V����
        Animator trapAnimator = sm2trap.GetComponent<Animator>();
        AnimatorByLayerName.SetLayerWeightByName(trapAnimator, "Trap2Layer", 1);
        AnimatorByLayerName.SetLayerWeightByName(trapAnimator, "Trap1Layer", 0);
        trapAnimator.SetTrigger("SetTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(10);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        sm2trap.HitBox.SetIsActive(true);
    }

    private async void SM2TrapBoot(Bullet trap)
    {
        if (trap == null) return;

        trap.GetComponentInChildren<HurtBoxManager>().SetActive(false);
        trap.GetComponent<Animator>().SetTrigger("BootTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(30);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        SM2TrapLost(trap);
    }

    private async void SM2TrapLost(Bullet trap)
    {
        if(trap == null) return;

        if (_sm2Traps.Contains(trap))
        {
            _sm2Traps.Remove(trap);
        }
        trap.GetComponentInChildren<HurtBoxManager>().SetActive(false);
        trap.GetComponent<Animator>().SetTrigger("LostTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(30);
        }
        catch
        {
            return;
        }

        if (trap != null)
        {
            Destroy(trap.gameObject);
        }
    }

    private async void SM2Trap2Break(Transform trapTF)
    {
        if(trapTF == null) return;

        Bullet trap = trapTF.GetComponent<Bullet>();
        if (_sm2Traps.Contains(trap))
        {
            _sm2Traps.Remove(trap);
        }
        trap.GetComponentInChildren<HurtBoxManager>().SetActive(false);
        trap.GetComponent<Animator>().SetTrigger("BreakTrigger");

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(30);
        }
        catch
        {
            return;
        }

        if (trap != null)
        {
            Destroy(trap.gameObject);
        }
    }

    /// <summary>
    /// ���K�E�Z
    /// </summary>
    public async UniTask Ultimate()
    {
        if (!CanEveryAction || _characterState.CurrentUP < 100) return;

        //�󒆕s��
        if (!OnGround) return;

        //UP����
        _characterState.SetCurrentUP(-100);

        // �V����CTS�𐶐�
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltTrigger");
        _animator.SetFloat("WalkFloat", 0);
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        //��������
        Velocity = Vector2.zero;

        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);


        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(1, token);

            _animator.updateMode = AnimatorUpdateMode.Normal;

            _hurtBox.SetActive(false);
            await StartUpMove(_ultimateInfo.StartupFrame, token);

            float speed = 25;
            if (!_characterState.IsLeftSide)
            {
                speed *= -1;
            }
            Velocity = new Vector2(speed, Velocity.y);

            await WaitForActiveFrame(_ultimateHitBox, _ultimateInfo.ActiveFrame, token);
            _hurtBox.SetActive(true);
            await RecoveryFrame(_ultimateInfo.RecoveryFrame, token);
        }
        catch(OperationCanceledException)
        {
            _animator.updateMode = AnimatorUpdateMode.Normal;
            _hurtBox.SetActive(true);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _ultCTS.Dispose();
            _ultCTS = null;
        }

        //layer�����ɖ߂�
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    //���n���ɃW�����v�U�����L�����Z��
    protected override void Land()
    {
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;
    }

    public override void CancelActionByHit()
    {
        _normalMoveCTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _jumpMoveCTS?.Cancel();
        NmBulletHit(_nmBullet);
    }
}
