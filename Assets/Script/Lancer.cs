using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Lancer : CharacterActions
{
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _nomalMoveInfo;
    [SerializeField] private HitBoxManager _nomalMoveHitBox;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Vector2 _sm1Direction;
    [SerializeField] private HitBoxManager _specialMove1HitBox;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _nomalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;

    //行動制限の設定
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
        //攻撃中の場合
        if(!CanEveryAction) return;

        //ジャンプ中ならジャンプ攻撃の処理を行う
        if(!_characterState.OnGround)
        {
            JumpMove().Forget();
            return;
        }

        // 新しいCTSを生成
        _nomalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _nomalMoveCTS.Token;

        //アニメーション処理
        SetLayerWeightByName("MoveLayer", 1);
        _animator.SetTrigger("NomalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        _rb.velocity = new Vector2(0, _rb.velocity.y);

        try
        {
            await StartUpNomalMove(_nomalMoveInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_nomalMoveHitBox ,_nomalMoveInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_nomalMoveInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃をキャンセル");
            _nomalMoveHitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _nomalMoveCTS.Dispose();
            _nomalMoveCTS = null;

            //layerを元に戻す
            SetLayerWeightByName("MoveLayer", 0);
        }
    }

    public async UniTask JumpMove()
    {
        //ジャンプ攻撃は空中で一回のみ
        if (_jumpMoveCount != 0) return;

        //ジャンプ攻撃したの回数を記録
        _jumpMoveCount++;

        // 新しいCTSを生成
        _jumpMoveCTS = new CancellationTokenSource();
        CancellationToken token = _jumpMoveCTS.Token;

        // アニメーション処理
        _animator.SetTrigger("JumpMoveTrigger");

        try
        {
            await StartUpNomalMove(_jumpMoveInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_jumpMoveHitBox ,_nomalMoveInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_jumpMoveInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("ジャンプ攻撃をキャンセル");
            _jumpMoveHitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _jumpMoveCTS.Dispose();
            _jumpMoveCTS = null;
        }

    }

    /// <summary>
    /// 必殺技１
    /// </summary>
    /// <returns></returns>
    public async UniTask SpecialMove1()
    {
        if (!CanEveryAction) return;

        // 新しいCTSを生成
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // アニメーション処理
        SetLayerWeightByName("SpecialMove1Layer", 0);
        _animator.SetTrigger("SpecialMove1Trigger");

        //物理挙動
        _rb.velocity = new Vector2(0, _rb.velocity.y);

        try
        {
            await StartUpNomalMove(_specialMove1Info.StartupFrame, token); // 発生を待つ

            //物理挙動
            float sm1DirectionX = _sm1Direction.x * (_characterState.IsLeftSide ? 1 : -1);
            _rb.AddForce(new Vector2(sm1DirectionX, _sm1Direction.x), ForceMode2D.Impulse);

            await WaitForActiveFrame(_specialMove1HitBox, _specialMove1Info.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _specialMove1HitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _jumpMoveCTS.Dispose();
            _jumpMoveCTS = null;
        }

        //layerを元に戻す
        SetLayerWeightByName("SpecialMove1Layer", 0);
    }

    //着地時にジャンプ攻撃をキャンセル
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
