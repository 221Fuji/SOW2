using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class ViolaCloud : CharacterActions
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
    [SerializeField] private Fog _fogPrefab;
    [SerializeField] private Animator _collectEffectAnimator;
    [SerializeField] private float _gainValue;
    private List<Fog> _fogList = new List<Fog>();
    public float FogMaxResource { get; } = 100;
    public float CurrentFogResource { get; private set; }
    [Header("�K�E�Z2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private GameObject _sm2BulletPrefab;
    [Header("���K�E�Z")]
    [SerializeField] private AttackInfo _ultOutSideInfo;
    [SerializeField] private Bullet _ultBullet;
    [SerializeField] private AttackInfo _ultInSideInfo;
    [SerializeField] private float _healValue;

    private int _jumpMoveCount = 0; //�P��̃W�����v�ōs�����W�����v�U���̉�

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
        CurrentFogResource = FogMaxResource;
    }

    protected override void SetActionDelegate()
    {
        _inputReciever.JumpDelegate = Jump;
        _inputReciever.GuardDelegate = GuardStance;
        _inputReciever.NormalMove = NormalMove;
        _inputReciever.SpecialMove1 = SpecialMove1;
        _inputReciever.SpecialMove2 = SpecialMove2;
        _inputReciever.Ultimate = UltimateOutSide;
    }

    protected override void SetHitBox()
    {
        _normalMoveHitBox.InitializeHitBox(_normalMoveInfo, gameObject);
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, gameObject);
    }

    protected override void FightingUpdate()
    {
        base.FightingUpdate();

        //�_�n���Ȃ��Ƃ��Q�[�W��
        if(_fogList.Count == 0)
        {
            if(CurrentFogResource < 100)
            {
                CurrentFogResource += _gainValue;
            }
            else
            {
                CurrentFogResource = 100;
            }         
        }
        else
        {
            //�Q�[�W�͊���
            if(CurrentFogResource <= 0)
            {
                DestoryAllFogInList(_fogList);
            }
        }
    }

    /// <summary>
    /// ���Ɏ��g�����鉌���̃��X�g
    /// </summary>
    private List<Fog> FogInSelfList()
    {
        List<Fog> fogInSelfList = new List<Fog>();

        if (_fogList.Count > 0)
        {
            foreach (var fog in _fogList)
            {
                if (fog.IsInSelf())
                {
                    fogInSelfList.Add(fog);
                }
            }
        }
        return fogInSelfList;
    }

    /// <summary>
    /// Fog��List�̗v�f��S�Ĕj�󂷂�
    /// </summary>
    private void DestoryAllFogInList(List<Fog> fogList)
    {
        var fogsToDestroy = new List<Fog>(fogList);

        foreach (var fog in fogsToDestroy)
        {
            fog.DestroyFog().Forget();
            _fogList.Remove(fog);
            fogList.Remove(fog);
        }
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

        List<Fog> fogIsSelfList = FogInSelfList();

        if (fogIsSelfList.Count > 0)
        {
            EnhancedNormalMove(fogIsSelfList).Forget();
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
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveInfo.StartupFrame, token); // ������҂�
            float speed = 14;
            if (!_characterState.IsLeftSide)
            {
                speed *= -1;
            }
            Velocity = new Vector2(speed, Velocity.y);
            await WaitForActiveFrame(_normalMoveHitBox, _normalMoveInfo.ActiveFrame, token); // ������҂�
            Velocity = Vector2.zero;
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
    /// �����ʏ�U��
    /// </summary>
    public async UniTask EnhancedNormalMove(List<Fog> fogList)
    {
        // �V����CTS�𐶐�
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        //�A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("EnhancedNormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP���
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveInfo.StartupFrame, token); // ������҂�
            float speed = 18;
            if (!_characterState.IsLeftSide)
            {
                speed *= -1;
            }
            DestoryAllFogInList(fogList);
            _collectEffectAnimator.SetTrigger("CollectFogTrigger");
            Velocity = new Vector2(speed, Velocity.y);
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
        if (!CanEveryAction) return;

        //��J���
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //�󒆕s��
        if (!OnGround) return;

        if (CurrentFogResource < 25) return;

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
            Fog fog = Instantiate(_fogPrefab);
            _fogList.Add(fog);
            fog.transform.position = transform.position;
            fog.InitializeFog(_characterState.IsLeftSide, gameObject);
            fog.SetIsActive(true);
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {

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

    /// <summary>
    /// Fog�N���X����Ă΂��
    /// </summary>
    public void ConsumptionFogResource(float value) 
    {
        CurrentFogResource -= value;
        if(CurrentFogResource < 0)
        {
            CurrentFogResource = 0;
        }
    }

    /// <summary>
    /// �K�E�Z�Q
    /// </summary>
    public async UniTask SpecialMove2()
    {
        if (!CanEveryAction) return;

        //��J���
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //�󒆕s��
        if (!OnGround) return;

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
            CreateSm2Bullet(token); //�e�̐���
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            //�e�͔����ۏ؂Ȃ�
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

    private async void CreateSm2Bullet(CancellationToken token)
    {
        //�e�̍��W�Ƒ��x�ݒ�
        Vector2 bulletPosOffset = new Vector2(1.5f, 1);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1.5f, 1);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        GameObject bullet = Instantiate(_sm2BulletPrefab, bulletPos, Quaternion.identity);

        //���������ǂ���
        List<Fog> fogIsSelfList = FogInSelfList();
        if (fogIsSelfList.Count > 0)
        {
            DestoryAllFogInList(fogIsSelfList);
            _collectEffectAnimator.SetTrigger("CollectFogTrigger");

            //�A�j���[�V����
            bullet.GetComponent<Animator>().SetTrigger("InSideTrigger");
        }
        else
        {
            //�A�j���[�V����
            bullet.GetComponent<Animator>().SetTrigger("OutSideTrigger");
        }

        //�e�̓����蔻��ݒ�
        HitBoxManager hbm = bullet.GetComponentInChildren<HitBoxManager>();
        hbm?.InitializeHitBox(_specialMove2Info, gameObject);

        await WaitForActiveFrame(hbm, _specialMove2Info.ActiveFrame, token);

        await FightingPhysics.DelayFrameWithTimeScale(60);
        if(bullet != null)
        {
            Destroy(bullet);
        }
    }

    /// <summary>
    /// ���K�E�Z
    /// </summary>
    public async UniTask UltimateOutSide()
    {
        List<Fog> fogList = FogInSelfList();
        if(fogList.Count > 0)
        {
            UltimateInSide();
            return;
        }

        if (!CanEveryAction || _characterState.CurrentUP < 100) return;

        if(!OnGround) return;

        //UP����
        _characterState.SetCurrentUP(-100);

        // �V����CTS�𐶐�
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltOutSideTrigger");

        //��������
        Velocity = Vector2.zero;

        //���o
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);

        try
        {
            await StartUpMove(_ultOutSideInfo.StartupFrame, token); // ������҂�

            for(int i = 0; i < 3 ;i++)
            {
                CreateUltBullet();
                await FightingPhysics.DelayFrameWithTimeScale(15, token);
            }

            await RecoveryFrame(_ultOutSideInfo.RecoveryFrame, token); // �d����҂�
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
        Quaternion bulletQuaternion = new Quaternion(0, 0, 0, 0);

        //���͕����ɂ���Đݒ�
        if (!_characterState.IsLeftSide)
        {
            generatePosOffsetX *= -1;
            bulletQuaternion = new Quaternion(0, 180, 0, 0);
        }
        float generatePosX = transform.position.x + generatePosOffsetX;
        Vector2 generatePos = new Vector2(generatePosX, transform.position.y);

        //UltBullet�̐���
        Bullet bullet = Instantiate(_ultBullet, generatePos, bulletQuaternion);
        bullet.HitBox.InitializeHitBox(_ultOutSideInfo, gameObject);
        bullet.DestroyBullet = DestroyUltBullet;

        bullet.Velocity = new Vector2(generatePosOffsetX * 4f, 0);
    }

    private void DestroyUltBullet(Bullet bullet)
    {
        if(bullet != null)
        {
            Destroy(bullet);
        }
    }


    private async void UltimateInSide()
    {
        if (!CanEveryAction || _characterState.CurrentUP < 100) return;

        if (!OnGround) return;

        //UP����
        _characterState.SetCurrentUP(-100);

        // �V����CTS�𐶐�
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltInSideTrigger");

        //���o
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);

        try
        {
            await StartUpMove(_ultInSideInfo.StartupFrame, token); // ������҂�
                                                                   //��������
            AddForce(new Vector2(0, 20));
            DestoryAllFogInList(_fogList);
            CurrentFogResource = 100;
            _characterState.TakeDamage(-_healValue);

            await WaitForActiveFrame(null, _ultInSideInfo.ActiveFrame, token);

            //��������
            Velocity = Vector2.zero;
            SetIsFixed(true);

            await RecoveryFrame(_ultInSideInfo.RecoveryFrame, token); // �d����҂�
        }
        finally
        {
            //��������
            Velocity = Vector2.zero;
            SetIsFixed(false);

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

    private async UniTask StartUpMove(int startUpFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(startUpFrame, cancellationToken: token);
    }

    private async UniTask WaitForActiveFrame(HitBoxManager hitBox, int activeFrame, CancellationToken token)
    {
        hitBox?.SetIsActive(true);
        await FightingPhysics.DelayFrameWithTimeScale(activeFrame, cancellationToken: token);
        hitBox?.SetIsActive(false);
    }

    private async UniTask RecoveryFrame(int recoveryFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(recoveryFrame, cancellationToken: token);
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
