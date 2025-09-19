using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

public class Nue : CharacterActions
{
    [Header("通常攻撃1")]
    [SerializeField] private AttackInfo _normalMove1Info;
    [SerializeField] private HitBoxManager _normalMove1HitBox;
    [SerializeField] private int _cancelFrame1;
    [Header("通常攻撃2")]
    [SerializeField] private AttackInfo _normalMove2Info;
    [SerializeField] private HitBoxManager _normalMove2HitBox;
    [SerializeField] private int _cancelFrame2;
    [Header("通常攻撃3")]
    [SerializeField] private AttackInfo _normalMove3Info;
    [SerializeField] private HitBoxManager _normalMove3HitBox;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Vector2 _sm1Direction;
    [SerializeField] private HitBoxManager _specialMove1HitBox;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private Bullet _sm2BulletPrefab;
    [SerializeField] private float _sm2BulletVelocity;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private int _ultPerformanceFrame;
    [SerializeField] private GameObject _rageFire;
    [Header("黒煙纏い")]
    [SerializeField] private float _blackFireDamage;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    private int _normalMoveCount = 0; //nmの段数
    private Bullet _sm2Bullet;
    private bool _isRage; //怒り状態か

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMove1CTS;
    private CancellationTokenSource _normalMove2CTS;
    private CancellationTokenSource _normalMove3CTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _ultCTS;
    private List<CancellationTokenSource> _cancelAcceptCTSList = new ();

    //行動制限の設定
    public override bool CanEveryAction
    {
        get
        {
            return base.CanEveryAction
                && _normalMove1CTS == null
                && _normalMove2CTS == null
                && _normalMove3CTS == null
                && _jumpMoveCTS == null
                && _specialMove1CTS == null
                && _specialMove2CTS == null;
        }
    }
    public bool CanNormalMove1
    {
        get
        {
            if(_normalMoveCount == 0)
            {
                if (!CanEveryAction) return false;
            }
            else
            {
                if(_cancelAcceptCTSList.Count == 0) return false;
            }

            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;

            return true;
        }
    }
    public bool CanJumpMove
    {
        get
        {
            if (!CanNormalMove1) return false;
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
            if (!OnGround) return false;
            if (_sm2Bullet) return false;

            return true;
        }
    }
    public bool CanSpecialMove2
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;
            if (!OnGround) return false;

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
        _inputReciever.NormalMove = NormalMove1;
        _inputReciever.SpecialMove1 = SpecialMove1;
        _inputReciever.SpecialMove2 = SpecialMove2;
        _inputReciever.Ultimate = Ultimate;
    }

    protected override void SetHitBox()
    {
        _normalMove1HitBox.InitializeHitBox(_normalMove1Info, gameObject);
        _normalMove2HitBox.InitializeHitBox(_normalMove2Info, gameObject);
        _normalMove3HitBox.InitializeHitBox(_normalMove3Info, gameObject);
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, gameObject);
        _specialMove1HitBox.InitializeHitBox(_specialMove1Info, gameObject);
    }

    public override void UPgain(float value)
    {
        if (_isRage)
        {
            value = -100f / _ultimateInfo.ActiveFrame;
            _characterState.SetCurrentUP(value);
            Debug.Log("koushi"+_ultimateInfo.ActiveFrame);
        }
        else
        {
            _characterState.SetCurrentUP(value);
        }
    }

    /// <summary>
    /// 通常攻撃1
    /// </summary>
    public async UniTask NormalMove1()
    {
        if (!CanNormalMove1) return;

        //段数によって派生
        if(_normalMoveCount == 1)
        {
            NormalMove2().Forget();
            return;
        }
        if(_normalMoveCount == 2)
        {
            NormalMove3().Forget();
            return;
        }

        //ジャンプ中ならジャンプ攻撃の処理を行う
        if (!OnGround)
        {
            JumpMove().Forget();
            return;
        }

        // 新しいCTSを生成
        _normalMove1CTS = new CancellationTokenSource();
        CancellationToken token = _normalMove1CTS.Token;

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMove1Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;
        if(_characterState.IsLeftSide)
        {
            AddForce(new Vector2(15, 0));
        }
        else
        {
            AddForce(new Vector2(-15, 0));
        }
        

        //SP消費
        _characterState.SetCurrentSP(-_normalMove1Info.ConsumptionSP);

        //UP回収
        if(!_isRage) UPgain(_normalMove1Info.MeterGain);

        //キャンセル猶予時間
        WaitCancelAcceptFrame(_cancelFrame1).Forget();

        //段数カウント
        _normalMoveCount = 1;

        AttackInfo attackInfo = RageModeAttackInfo(_normalMove1Info, _normalMove1HitBox);

        try
        {
            await StartUpMove(attackInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_normalMove1HitBox, attackInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(attackInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃1をキャンセル");
            _normalMove2HitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _normalMove1CTS.Dispose();
            _normalMove1CTS = null;

            //layerを元に戻す
            if(_normalMove2CTS == null && _normalMove3CTS == null)
            {
                AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);

                //カウントを元に戻す
                _normalMoveCount = 0;
            }
        }
    }

    /// <summary>
    /// 通常攻撃2
    /// </summary>
    public async UniTask NormalMove2()
    {
        // 新しいCTSを生成
        _normalMove2CTS = new CancellationTokenSource();
        CancellationToken token = _normalMove2CTS.Token;

        //nm1をキャンセル
        _normalMove1CTS?.Cancel();

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMove2Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //SP消費
        _characterState.SetCurrentSP(-_normalMove2Info.ConsumptionSP);

        //UP回収
        if (!_isRage) UPgain(_normalMove2Info.MeterGain);

        //キャンセル猶予時間
        WaitCancelAcceptFrame(_cancelFrame2).Forget();

        //段数カウント
        _normalMoveCount = 2;

        AttackInfo attackInfo = RageModeAttackInfo(_normalMove2Info, _normalMove2HitBox);

        try
        {
            await StartUpMove(attackInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_normalMove2HitBox, attackInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(attackInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃2をキャンセル");
            _normalMove2HitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _normalMove2CTS.Dispose();
            _normalMove2CTS = null;

            //layerを元に戻す
            if (_normalMove3CTS == null)
            {
                AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);

                //カウントを元に戻す
                _normalMoveCount = 0;
            }
        }
    }

    /// <summary>
    /// 通常攻撃2
    /// </summary>
    public async UniTask NormalMove3()
    {
        // 新しいCTSを生成
        _normalMove3CTS = new CancellationTokenSource();
        CancellationToken token = _normalMove3CTS.Token;

        //nm2をキャンセル
        _normalMove2CTS?.Cancel();

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMove3Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        if (_characterState.IsLeftSide)
        {
            AddForce(new Vector2(10, 0));
        }
        else
        {
            AddForce(new Vector2(-10, 0));
        }

        //SP消費
        _characterState.SetCurrentSP(-_normalMove3Info.ConsumptionSP);

        //UP回収
        if (!_isRage) UPgain(_normalMove3Info.MeterGain);

        //段数リセット（派生先なし）
        _normalMoveCount = 0;

        AttackInfo attackInfo = RageModeAttackInfo(_normalMove3Info, _normalMove3HitBox);
        try
        {
            await StartUpMove(attackInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_normalMove3HitBox, attackInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(attackInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃3をキャンセル");
            _normalMove3HitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _normalMove3CTS.Dispose();
            _normalMove3CTS = null;

            //layerを元に戻す
            AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
        }
    }

    /// <summary>
    /// ジャンプ攻撃
    /// </summary>
    public async UniTask JumpMove()
    {
        //ジャンプ攻撃は空中で一回のみ
        if (!CanJumpMove) return;

        //ジャンプ攻撃したの回数を記録
        _jumpMoveCount++;

        // 新しいCTSを生成
        _jumpMoveCTS = new CancellationTokenSource();
        CancellationToken token = _jumpMoveCTS.Token;

        // アニメーション処理
        _animator.SetTrigger("JumpMoveTrigger");

        //SP消費
        _characterState.SetCurrentSP(-_jumpMoveInfo.ConsumptionSP);

        //UP回収
        if (!_isRage) UPgain(_jumpMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_jumpMoveInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_jumpMoveHitBox, _jumpMoveInfo.ActiveFrame, token); // 持続を待つ
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
    public async UniTask SpecialMove1()
    {
        if (!CanSpecialMove1) return;

        // 新しいCTSを生成
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");

        //SP消費
        _characterState.SetCurrentSP(-_specialMove1Info.ConsumptionSP);

        //UP回収
        if (!_isRage) UPgain(_specialMove1Info.MeterGain);

        AttackInfo attackInfo = RageModeAttackInfo(_specialMove1Info, _specialMove1HitBox);
        try
        {
            //アーマー付与
            _characterState.TakeAnormalyState(AnormalyState.SuperArmor);
            await StartUpMove(attackInfo.StartupFrame, token); // 発生を待つ
            //アーマー解除
            _characterState.RecoverAnormalyState(AnormalyState.SuperArmor);
            //物理挙動
            float sm1DirectionX = _sm1Direction.x * (_characterState.IsLeftSide ? 1 : -1);
            Velocity = Vector2.zero;
            AddForce(new Vector2(sm1DirectionX, _sm1Direction.y));

            await WaitForActiveFrame(_specialMove1HitBox, attackInfo.ActiveFrame, token); // 持続を待つ
            Velocity = Vector2.zero;

            await RecoveryFrame(attackInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _specialMove1HitBox.SetIsActive(false);
            //アーマー解除
            _characterState.RecoverAnormalyState(AnormalyState.SuperArmor);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _specialMove1CTS.Dispose();
            _specialMove1CTS = null;
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
    }

    /// <summary>
    /// 必殺技２
    /// </summary>
    public async UniTask SpecialMove2()
    {
        if (!CanSpecialMove2) return;

        // 新しいCTSを生成
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("SpecialMove2Trigger");

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_specialMove2Info.ConsumptionSP);

        //UP回収
        if (!_isRage) UPgain(_specialMove2Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove2Info.StartupFrame, token); // 発生を待つ
            CreateSm2Bullet(token);
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Sm2BulletHit(_sm2Bullet);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _specialMove2CTS.Dispose();
            _specialMove2CTS = null;
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
    }

    private async void CreateSm2Bullet(CancellationToken token)
    {
        //弾の座標と速度設定
        Vector2 bulletVelocity = new Vector2(_sm2BulletVelocity, 0);
        Vector2 bulletPosOffset = new Vector2(3f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_sm2BulletPrefab, bulletPos, rotation);
        bullet.Velocity = bulletVelocity;

        //弾の当たり判定設定
        AttackInfo attackInfo = RageModeAttackInfo(_specialMove2Info, null);
        bullet.HitBox.InitializeHitBox(attackInfo, gameObject);
        bullet.HitBox.HitBullet = Sm2BulletHit;
        bullet.HitBox.GuardBullet = Sm2BulletHit;
        bullet.DestroyBullet = Sm2BulletHit;

        try
        {
            await WaitForActiveFrame(bullet.HitBox, _specialMove2Info.ActiveFrame, token);
        }
        finally
        {
            Sm2BulletHit(bullet);
        }
    }

    private async void Sm2BulletHit(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.Velocity = Vector2.zero;
        bullet.GetComponent<Animator>().SetTrigger("Sm2HitTrigger");

        await FightingPhysics.DelayFrameWithTimeScale(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
            _sm2Bullet = null;
        }
    }

    /// <summary>
    /// 超必殺技
    /// </summary>
    public async UniTask Ultimate()
    {

        if (!CanUltimate) return;

        //UP消費
        _characterState.SetCurrentUP(-1);

        // 新しいCTSを生成
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltTrigger");

        //物理挙動
        Velocity = Vector2.zero;

        //演出
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, _ultPerformanceFrame);
        _characterState.SetIsUltPerformance();

        //演出解除
        await FightingPhysics.DelayFrameWithTimeScale(1, token);
        _animator.updateMode = AnimatorUpdateMode.Normal;
        RageMode(_ultimateInfo.ActiveFrame, token).Forget();

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    private AttackInfo RageModeAttackInfo(AttackInfo attackInfo ,HitBoxManager hitbox)
    {
        AttackInfo result = attackInfo;
        if(_isRage)
        {
            result.Damage = attackInfo.Damage * 1.5f;
            result.DrainSP = attackInfo.DrainSP * 1.5f;
        }
        hitbox?.InitializeHitBox(result, gameObject);
        return result;
    }

    private async UniTask RageMode(int rageModeFrame, CancellationToken token)
    {
        GameObject rageFire = Instantiate(_rageFire, transform);
        rageFire.transform.localScale = Vector3.one;
        try
        {
            _isRage = true;
            await FightingPhysics.DelayFrameWithTimeScale(rageModeFrame, token);
        }
        finally
        {
            _isRage = false;
            if(rageFire)
            {
                rageFire.GetComponent<Animator>().SetTrigger("DestoryRageFireTrigger");
                await FightingPhysics.DelayFrameWithTimeScale(30);
                Destroy(rageFire);
            }
            _ultCTS.Dispose();
            _ultCTS = null;
        }
    }

    //着地時にジャンプ攻撃をキャンセル
    protected override void Land()
    {
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;
    }

    private async UniTask WaitCancelAcceptFrame(int cancelFrame)
    {
        if(_cancelAcceptCTSList.Count != 0)
        {
            _cancelAcceptCTSList[0]?.Cancel();
        }
        var lastCts = new CancellationTokenSource();
        _cancelAcceptCTSList.Add(lastCts);
        Debug.Log(_cancelAcceptCTSList.Count);
        CancellationToken token = lastCts.Token;

        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(cancelFrame, cancellationToken: token);
        }
        finally
        {
            _cancelAcceptCTSList[0]?.Dispose();
            _cancelAcceptCTSList[0] = null;
            _cancelAcceptCTSList.RemoveAt(0);
        }
    }

    public async override UniTask Guard(AttackInfo attackInfo)
    {
        if(!attackInfo.IsBullet)
        {
            CharacterState enemyCS = EnemyCA.GetComponent<CharacterState>();

            if (enemyCS == null) return;

            if (enemyCS.CurrentHP - _blackFireDamage > 1)
            {
                enemyCS.TakeDamage(_blackFireDamage);
            }
            else if (enemyCS.CurrentHP - _blackFireDamage > 0)
            {
                enemyCS.TakeDamage(enemyCS.CurrentHP - 2);
            }
        }

        await base.Guard(attackInfo);
    }

    protected override void Die()
    {
        base.Die();
        _ultCTS?.Cancel();
    }

    public override void CancelActionByHit()
    {
        _normalMove1CTS?.Cancel();
        _normalMove2CTS?.Cancel();
        _normalMove3CTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _jumpMoveCTS?.Cancel();
    }
}
