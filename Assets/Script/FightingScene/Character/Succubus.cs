using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;

public class Succubus : CharacterActions
{
    [Header("�ʏ�U��")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private HitBoxManager _normalMoveHitBox;
    [Header("���ʏ�U��")]
    [SerializeField] private AttackInfo _normalMoveBehindInfo;
    [SerializeField] private GameObject _portalPrefab;
    [SerializeField] private GameObject _chainPrefab;
    private HitBoxManager _normalMoveBehindHitBox;
    [Header("�W�����v�U��")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [SerializeField] private Vector2 _jmDirection;
    [Header("�K�E�Z�P")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private GameObject _sm1BulletPrefab;
    [SerializeField] private float _sm1BulletVelocity;
    private HitBoxManager _specialMove1HitBox;
    [Header("�K�E�Z2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private HitBoxManager _specialMove2HitBox;
    [SerializeField] private AttackInfo _sm2DerivationInfo;
    [SerializeField] private HitBoxManager _sm2DerivationHitBox;
    [Header("���K�E�Z")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private GameObject _ultBulletPrefab;
    [SerializeField] private Vector2 _ultBulletVelocity;
    private HitBoxManager _ultHitBox;

    private int _jumpMoveCount = 0; //�P��̃W�����v�ōs�����W�����v�U���̉�

    //���ʏ�U���̕ϐ�
    private GameObject _startPortal;
    private GameObject _endPortal;
    private GameObject _chain;

    //�K�E�Z�P�̕ϐ�
    private GameObject _sm1Bullet;

    //���K�E�Z�̕ϐ�
    private GameObject _ultBullet;

    //�e�s����CancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _normalMoveBehindCTS;
    private CancellationTokenSource _pullChainCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _sm2DerivationCTS;
    private CancellationTokenSource _ultCTS;

    //�s�������̐ݒ�
    protected override bool CanEveryAction
    {
        get
        {
            return base.CanEveryAction
                && _normalMoveCTS == null
                && _normalMoveBehindCTS == null
                && _pullChainCTS == null
                && _jumpMoveCTS == null
                && _specialMove1CTS == null
                && _specialMove2CTS == null
                && _sm2DerivationCTS == null
                && _ultCTS == null;
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
        _specialMove2HitBox.InitializeHitBox(_specialMove2Info, gameObject);
        _sm2DerivationHitBox.InitializeHitBox(_sm2DerivationInfo, gameObject);
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

        //�����͂ł����
        if(WalkDirection() < 0)
        {
            NormalMoveBehind().Forget();
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

            //��������
            float jmDirectionX = _jmDirection.x * (_characterState.IsLeftSide ? 1 : -1);
            Velocity = Vector2.zero;

            AddForce(new Vector2(jmDirectionX, _jmDirection.y));

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
    /// ���ʏ�U��
    /// </summary>
    public async UniTask NormalMoveBehind()
    {
        // �V����CTS�𐶐�
        _normalMoveBehindCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveBehindCTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveBehindTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_normalMoveBehindInfo.ConsumptionSP);

        //UP���
        UPgain(_normalMoveBehindInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveBehindInfo.StartupFrame, token); // ������҂�
            await CreatePortal(token); //�|�[�^���̐���
            await RecoveryFrame(_normalMoveBehindInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            _normalMoveBehindHitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _normalMoveBehindCTS.Dispose();
            _normalMoveBehindCTS = null;

            //layer�����ɖ߂�
            if (_pullChainCTS == null)
            {
                AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
                _animator.SetTrigger("ChainCancelTrigger");
                DestoryPortal().Forget();
            }
        }
    }

    public async UniTask CreatePortal(CancellationToken token)
    {
        //�|�[�^���̐���
        Vector2 startPortalPos = new Vector2(transform.position.x, -4);
        Vector2 endPortalPos;
        if (_characterState.IsLeftSide)
        {
            startPortalPos -= new Vector2(5, 0);
            _startPortal = Instantiate(_portalPrefab, startPortalPos, Quaternion.identity);

            endPortalPos = new Vector2(StageParameter.CurrentRightWallPosX - 1, -4);
            _endPortal = Instantiate(_portalPrefab, endPortalPos, new Quaternion(0, 180, 0, 0));
        }
        else
        {
            startPortalPos += new Vector2(5f, 0);
            _startPortal = Instantiate(_portalPrefab, startPortalPos, new Quaternion(0, 180, 0, 0));

            endPortalPos = new Vector2(StageParameter.CurrentLeftWallPosX + 1, -4);
            _endPortal = Instantiate(_portalPrefab, endPortalPos, Quaternion.identity);
        }

        await FightingPhysics.DelayFrameWithTimeScale(5, token);
        await CreateChain(endPortalPos, token);
    }

    public async UniTask CreateChain(Vector2 endPortalPos, CancellationToken token)
    {
        Vector2 chainPos;
        Vector2 chainPosOffset = new Vector2(0, 0);
        if(_characterState.IsLeftSide)
        {
            chainPos = endPortalPos - chainPosOffset;
            _chain = Instantiate(_chainPrefab, chainPos, new Quaternion(0, 180, 0, 0));
        }
        else
        {
            chainPos = endPortalPos + chainPosOffset;
            _chain = Instantiate(_chainPrefab, chainPos, Quaternion.identity);
        }

        _normalMoveBehindHitBox = _chain.GetComponentInChildren<HitBoxManager>();
        _normalMoveBehindHitBox.InitializeHitBox(_normalMoveBehindInfo, gameObject);
        _normalMoveBehindHitBox.Hit = HitChain;

        await WaitForActiveFrame(_normalMoveBehindHitBox, _normalMoveBehindInfo.ActiveFrame, token);
    }

    /// <summary>
    /// ���ʏ�U����������ƌĂ΂��
    /// </summary>
    public void HitChain()
    {
        _pullChainCTS = new CancellationTokenSource();
        CancellationToken token = _pullChainCTS.Token;
        PullChain(token).Forget();

        _normalMoveBehindCTS.Cancel();
    }

    public async UniTask PullChain(CancellationToken token)
    {
        //�o�C���h�t�^
        _enemyCA.GetComponent<CharacterState>().AnormalyStates.Add(AnormalyState.Bind);

        //�A�j���[�V��������
        Animator chainAnimator = _chain.GetComponent<Animator>();
        float pulltime = 0;
        if(!chainAnimator.GetCurrentAnimatorStateInfo(0).IsName("PullChain"))
        {
            pulltime = 1 - AnimatorByLayerName.GetCurrentAnimationProgress(chainAnimator, "Base Layer");
        }
        AnimatorByLayerName.PlayAnimationOnLayer(chainAnimator, "PullChain", "Base Layer", pulltime);
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("PullTrigger");

        //�|�[�^���܂ň�������
        bool completePull = false;
        while (!completePull)
        {
            if(_characterState.IsLeftSide)
            {
                completePull = _endPortal?.transform.position.x < _enemyCA.GetPushBackBox().xMax;
            }
            else
            {
                completePull = _endPortal?.transform.position.x > _enemyCA.GetPushBackBox().xMin;
            }

            //�G�̍��W�����̐�[�ɍX�V
            Vector3 offset = _enemyCA.transform.position - (Vector3)_enemyCA.GetPushBackBox().center;
            _enemyCA.transform.position = _normalMoveBehindHitBox.transform.position + offset;

            await FightingPhysics.DelayFrameWithTimeScale(1, token);

            //�L�����Z�����̋���
            if(token.IsCancellationRequested)
            {
                _enemyCA.Velocity = Vector2.zero;
                EndPullChain();
                return;
            }
        }

        FishingEnemy();

        //���������̍d��
        await RecoveryFrame(25, token);
        EndPullChain();
    }

    /// <summary>
    /// pullChainCTS���I������ƌĂ΂��
    /// </summary>
    private void EndPullChain()
    {
        _animator.SetTrigger("ChainCancelTrigger");
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
        DestoryPortal().Forget();
        _pullChainCTS?.Dispose();
        _pullChainCTS = null;
    }

    public void FishingEnemy()
    {
        AnimatorByLayerName.PlayAnimationOnLayer(_animator, "Pull", "NormalMoveLayer", 0.5f);

        _enemyCA.GetComponent<CharacterState>().RecoverAnormalyState(AnormalyState.Bind);

        //��������
        _enemyCA.transform.position = _startPortal.transform.position;
        Vector2 fishingPower = new Vector2(5, 25);
        if(!_characterState.IsLeftSide)
        {
            fishingPower *= new Vector2(-1, 1);
        }
        _enemyCA.Velocity = Vector2.zero;
        _enemyCA.AddForce(fishingPower);
    }

    private async UniTask DestoryPortal()
    {
        //���̍폜
        if (_chain != null)
        {
            Destroy(_chain);
            _chain = null;
        }

        if (_startPortal == null || _endPortal == null) return;

        Animator startAnimator = _startPortal.GetComponent<Animator>();
        Animator endAnimator = _endPortal.GetComponent<Animator>();

        //�|�[�^������U�J���܂ő҂�
        await UniTask.WaitUntil(() =>
        {
            var startStateInfo = startAnimator.GetCurrentAnimatorStateInfo(0);
            return !startStateInfo.IsName("Open");
        });

        startAnimator.SetTrigger("CloseTrigger");
        endAnimator.SetTrigger("CloseTrigger");

        //�|�[�^��������܂ő҂�
        await UniTask.WaitUntil(() =>
        {
            var startStateInfo = startAnimator.GetCurrentAnimatorStateInfo(0);
            return startStateInfo.IsName("Close") && startStateInfo.normalizedTime >= 1f;
        });

        Destroy(_startPortal);
        _startPortal = null;
        Destroy(_endPortal);
        _endPortal = null;
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
            CreateSm1Bullet(token);
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            _specialMove1HitBox.SetIsActive(false);
            Sm1BulletHit();
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

    private void CreateSm1Bullet(CancellationToken token)
    {
        //�e�̍��W�Ƒ��x�ݒ�
        Vector2 bulletVelocity = new Vector2(_sm1BulletVelocity, 0);
        Vector2 bulletPosOffset = new Vector2(1, 1);
        if(!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        _sm1Bullet = Instantiate(_sm1BulletPrefab, bulletPos, Quaternion.identity);
        _sm1Bullet.GetComponent<FightingRigidBody>().Velocity = bulletVelocity;

        //�A�j���[�V����
        _sm1Bullet.GetComponent<Animator>().SetTrigger("Sm1BulletTrigger");

        //�e�̓����蔻��ݒ�
        _specialMove1HitBox = _sm1Bullet.GetComponentInChildren<HitBoxManager>();
        _specialMove1HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        _specialMove1HitBox.Hit = Sm1BulletHit;
        _specialMove1HitBox.Guard = Sm1BulletHit;
        WaitForActiveFrame(_specialMove1HitBox, 0, token).Forget();
    }

    private async void Sm1BulletHit()
    {
        if (_sm1Bullet == null) return;
        _sm1Bullet.GetComponent<FightingRigidBody>().Velocity = Vector2.zero;
        _sm1Bullet.GetComponent<Animator>().SetTrigger("Sm1HitTrigger");
        await FightingPhysics.DelayFrameWithTimeScale(30);
        if (_sm1Bullet != null)
        {
            Destroy(_sm1Bullet);
            _sm1Bullet = null;
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

        //2�i�ڔh��
        if (_specialMove2CTS != null)
        {
            Sm2Derivation().Forget();
            _specialMove2CTS.Cancel();
            return;
        }

        if (!CanEveryAction) return;

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
            if(_sm2DerivationCTS == null)
            {
                //layer�����ɖ߂�
                Debug.Log("hasei ");
                AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
            }
        }

    }

    private async UniTask Sm2Derivation()
    {
        // �V����CTS�𐶐�
        _sm2DerivationCTS = new CancellationTokenSource();
        CancellationToken token = _sm2DerivationCTS.Token;

        // �A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("Sm2DerivationTrigger");

        //��������
        Velocity = Vector2.zero;

        //SP����
        _characterState.SetCurrentSP(-_sm2DerivationInfo.ConsumptionSP);

        //UP���
        UPgain(_sm2DerivationInfo.MeterGain);

        try
        {
            await StartUpMove(_sm2DerivationInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_sm2DerivationHitBox, _sm2DerivationInfo.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_sm2DerivationInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException) 
        {
            _sm2DerivationHitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _sm2DerivationCTS.Dispose();
            _sm2DerivationCTS = null;
        }

        //layer�����ɖ߂�
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
    }

    /// <summary>
    /// ���K�E�Z
    /// </summary>
    public async UniTask Ultimate()
    {

        if (!CanEveryAction && _characterState.CurrentUP >= 100) return;

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

        //��������
        Velocity = Vector2.zero;

        try
        {
            await CreateUltBullet(token); // �����ۏ؂���
        }
        catch(OperationCanceledException) 
        {
            UltBulletHit();
        }

        await RecoveryFrame(_ultimateInfo.RecoveryFrame, token); // �d����҂�

        // �U������������������A�g�[�N�������
        _ultCTS.Dispose();
        _ultCTS = null;

        //layer�����ɖ߂�
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    private async UniTask CreateUltBullet(CancellationToken token)
    {
        //�e�̐���
        Vector2 bulletVelocity = _ultBulletVelocity;
        Vector2 bulletPosOffset = new Vector2(0, 1.5f);

        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        _ultBullet = Instantiate(_ultBulletPrefab, bulletPos, Quaternion.identity);
        BulletPhysics ultFRB = _ultBullet.GetComponent<BulletPhysics>();
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            ultFRB.SetBoundVelocity(ultFRB.BoundVelocity * new Vector2(-1, 1));
        }
        ultFRB.SetIsFixed(true);

        //�A�j���[�V����
        _ultBullet.GetComponent<Animator>().SetTrigger("UltBulletTrigger");

        // ������҂�
        await StartUpMove(_ultimateInfo.StartupFrame, token);

        //���x�ݒ�
        ultFRB.SetIsFixed(false);
        _ultBullet.GetComponent<FightingRigidBody>().Velocity = bulletVelocity;


        //�e�̓����蔻��ݒ�
        _ultHitBox = _ultBullet.GetComponentInChildren<HitBoxManager>();
        _ultHitBox.InitializeHitBox(_ultimateInfo, gameObject);
        _ultHitBox.Hit = UltBulletHit;
        _ultHitBox.Guard = UltBulletHit;
        WaitForActiveFrame(_ultHitBox, 0, token).Forget();
    }

    private async void UltBulletHit()
    {
        if (!_ultBullet.TryGetComponent(out Transform t)) return;
        FightingRigidBody ultFRG = _ultBullet.GetComponent<FightingRigidBody>();
        ultFRG.Velocity = Vector2.zero;
        ultFRG.SetIsFixed(true);
        _ultBullet.GetComponent<Animator>().SetTrigger("UltHitTrigger");
        await FightingPhysics.DelayFrameWithTimeScale(30);

        Destroy(_ultBullet);
        _ultBullet = null;
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
        if (activeFrame <= 0) return;
        await FightingPhysics.DelayFrameWithTimeScale(activeFrame, cancellationToken: token);
        hitBox?.SetIsActive(false);
    }

    private async UniTask RecoveryFrame(int recoveryFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(recoveryFrame, cancellationToken: token);
    }

    protected override void CancelActionByHit()
    {
        _normalMoveCTS?.Cancel();
        _normalMoveBehindCTS?.Cancel();
        _pullChainCTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _sm2DerivationCTS?.Cancel();
        _jumpMoveCTS?.Cancel();
    }
}
