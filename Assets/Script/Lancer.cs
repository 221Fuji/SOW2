using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Lancer : CharacterActions
{
    [Header("�ʏ�U��")]
    [SerializeField] private AttackInfo _nomalMoveInfo;
    [SerializeField] private HitBoxManager _nomalMoveHitBox;
    [Header("�W�����v�U��")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("�K�E�Z�P")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Vector2 _sm1Direction;
    [SerializeField] private HitBoxManager _specialMove1HitBox;

    private int _jumpMoveCount = 0; //�P��̃W�����v�ōs�����W�����v�U���̉�
    

    //�e�s����CancellationTokenSource(CTS)
    private CancellationTokenSource _nomalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;

    //�s�������̐ݒ�
    private bool CanEveryAction
    {
        get
        {
            return !_characterState.IsRecoveringHit
                && _nomalMoveCTS == null
                && _jumpMoveCTS == null
                && _specialMove1CTS == null;
        }
    }
    protected override bool CanWalk { get { return CanEveryAction; } }
    protected override bool CanJump { get { return CanEveryAction; } }

    protected override void SetActionDelegate()
    {
        base.SetActionDelegate();
        _inputReciever.NomalMove = NomoalMove;
    }

    protected override void SetHitBox()
    {
        _nomalMoveHitBox.InitializeHitBox(_nomalMoveInfo, _enemyObject);
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, _enemyObject);
    }

    public async UniTask NomoalMove()
    {
        //�U�����̏ꍇ
        if(!CanEveryAction) return;

        //�W�����v���Ȃ�W�����v�U���̏������s��
        if(!_characterState.OnGround)
        {
            JumpMove().Forget();
            return;
        }

        // �V����CTS�𐶐�
        _nomalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _nomalMoveCTS.Token;

        //�A�j���[�V��������
        SetLayerWeightByName("MoveLayer", 1);
        _animator.SetTrigger("NomalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //��������
        _rb.velocity = new Vector2(0, _rb.velocity.y);

        try
        {
            await StartUpNomalMove(_nomalMoveInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_nomalMoveHitBox ,_nomalMoveInfo.ActiveFrame, token); // ������҂�
            await RecoveryFrame(_nomalMoveInfo.RecoveryFrame, token); // �d����҂�
        }
        catch (OperationCanceledException)
        {
            Debug.Log("�ʏ�U�����L�����Z��");
            _nomalMoveHitBox.SetIsActive(false);
        }
        finally
        {
            // �U������������������A�g�[�N�������
            _nomalMoveCTS.Dispose();
            _nomalMoveCTS = null;

            //layer�����ɖ߂�
            SetLayerWeightByName("MoveLayer", 0);
        }
    }

    public async UniTask JumpMove()
    {
        //�W�����v�U���͋󒆂ň��̂�
        if (_jumpMoveCount != 0) return;

        //�W�����v�U�������̉񐔂��L�^
        _jumpMoveCount++;

        // �V����CTS�𐶐�
        _jumpMoveCTS = new CancellationTokenSource();
        CancellationToken token = _jumpMoveCTS.Token;

        // �A�j���[�V��������
        _animator.SetTrigger("JumpMoveTrigger");

        try
        {
            await StartUpNomalMove(_jumpMoveInfo.StartupFrame, token); // ������҂�
            await WaitForActiveFrame(_jumpMoveHitBox ,_nomalMoveInfo.ActiveFrame, token); // ������҂�
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
    /// <returns></returns>
    public async UniTask SpecialMove1()
    {
        if (!CanEveryAction) return;

        // �V����CTS�𐶐�
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // �A�j���[�V��������
        SetLayerWeightByName("SpecialMove1Layer", 0);
        _animator.SetTrigger("SpecialMove1Trigger");

        //��������
        _rb.velocity = new Vector2(0, _rb.velocity.y);

        try
        {
            await StartUpNomalMove(_specialMove1Info.StartupFrame, token); // ������҂�

            //��������
            float sm1DirectionX = _sm1Direction.x * (_characterState.IsLeftSide ? 1 : -1);
            _rb.AddForce(new Vector2(sm1DirectionX, _sm1Direction.x), ForceMode2D.Impulse);

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
            _jumpMoveCTS.Dispose();
            _jumpMoveCTS = null;
        }

        //layer�����ɖ߂�
        SetLayerWeightByName("SpecialMove1Layer", 0);
    }

    //���n���ɃW�����v�U�����L�����Z��
    protected override void Land()
    {
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;
    }

    private async UniTask StartUpNomalMove(int startUpFrame, CancellationToken token)
    {
        await UniTask.DelayFrame(startUpFrame, cancellationToken: token);
    }

    private async UniTask WaitForActiveFrame(HitBoxManager hitBox, int activeFrame, CancellationToken token)
    {
        hitBox?.SetIsActive(true);
        await UniTask.DelayFrame(activeFrame, cancellationToken: token);
        hitBox?.SetIsActive(false);
    }

    private async UniTask RecoveryFrame(int recoveryFrame, CancellationToken token)
    {
        await UniTask.DelayFrame(recoveryFrame, cancellationToken: token);
    }
}
