using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Necrom : CharacterActions
{
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private Bullet _nmBulletPrefab;
    [SerializeField] private float _nmBulletVelocity;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Bullet _trapPrefab;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private HitBoxManager _ultimateHitBox;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数

    private Bullet _nmBullet;
    private List<Bullet> _sm1Traps = new List<Bullet>();
    private List<Bullet> _sm2Traps = new List<Bullet>();

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _ultCTS;

    //行動制限の設定
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
    /// 通常攻撃
    /// </summary>
    public async UniTask NormalMove()
    {
        //攻撃中の場合
        if (!CanEveryAction) return;

        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //ジャンプ中ならジャンプ攻撃の処理を行う
        if (!OnGround)
        {
            JumpMove().Forget();
            return;
        }

        if (_nmBullet != null) return;

        // 新しいCTSを生成
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = new Vector2(0, Velocity.y);

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveInfo.StartupFrame, token); // 発生を待つ
            CreateNmBullet(token);
            await RecoveryFrame(_normalMoveInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃をキャンセル");
            NmBulletHit(_nmBullet);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _normalMoveCTS.Dispose();
            _normalMoveCTS = null;

            //layerを元に戻す
            AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
        }
    }

    private async void CreateNmBullet(CancellationToken token)
    {
        //弾の座標と速度設定
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

        //弾の当たり判定設定
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
    /// ジャンプ攻撃
    /// </summary>
    public async UniTask JumpMove()
    {
        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //ジャンプ攻撃は空中で一回のみ
        if (_jumpMoveCount != 0) return;

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
        UPgain(_jumpMoveInfo.MeterGain);

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
        if (!CanEveryAction) return;

        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //空中不可
        if (!OnGround) return;

        // 新しいCTSを生成
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // 発生を待つ
            CreateSm1Trap();
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // 硬直を待つ
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

    private async void CreateSm1Trap()
    {
        //全てのsm1Trapを削除
        while(_sm1Traps.Count >= 1)
        {
            SM1TrapLost(_sm1Traps[0]);
        }

        //弾の座標と速度設定
        Vector2 trapPosOffset = new Vector2(3f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            trapPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        float trapPosX = transform.position.x + trapPosOffset.x;

        //画面端を超えない
        trapPosX = Mathf.Clamp(trapPosX,
            StageParameter.CurrentLeftWallPosX + 1.5f,
            StageParameter.CurrentRightWallPosX - 1.5f);

        Vector2 trapPos = new Vector2(trapPosX, StageParameter.GroundPosY);
        Bullet sm1trap = Instantiate(_trapPrefab, trapPos, rotation);
        _sm1Traps.Add(sm1trap);

        //弾の当たり判定設定
        sm1trap.HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        sm1trap.HitBox.HitBullet = SM1TrapBoot;
        sm1trap.HitBox.GuardBullet = SM1TrapBoot;
        sm1trap.DestroyBullet = SM1TrapLost;

        //アニメーション
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
    /// 必殺技２
    /// </summary>
    public async UniTask SpecialMove2()
    {
        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //空中不可
        if (!OnGround) return;

        if (!CanEveryAction) return;

        // 新しいCTSを生成
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_specialMove2Info.ConsumptionSP);

        //UP回収
        UPgain(_specialMove2Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove2Info.StartupFrame, token); // 発生を待つ
            CreateSm2Trap();
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // 硬直を待つ
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _specialMove2CTS.Dispose();
            _specialMove2CTS = null;

            AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
        }
    }

    private async void CreateSm2Trap()
    {
        //全てのsm2Trapを削除
        while (_sm2Traps.Count >= 1)
        {
            SM2TrapLost(_sm2Traps[0]);
        }

        //弾の座標と速度設定
        Vector2 trapPosOffset = new Vector2(4.5f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            trapPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        float trapPosX = transform.position.x + trapPosOffset.x;

        //画面端を超えない
        trapPosX = Mathf.Clamp(trapPosX, 
            StageParameter.CurrentLeftWallPosX + 1.5f,
            StageParameter.CurrentRightWallPosX - 1.5f);

        Vector2 trapPos = new Vector2(trapPosX, StageParameter.GroundPosY);
        Bullet sm2trap = Instantiate(_trapPrefab, trapPos, rotation);
        _sm2Traps.Add(sm2trap);

        //弾の当たり判定設定
        sm2trap.HitBox.InitializeHitBox(_specialMove2Info, gameObject);
        sm2trap.HitBox.HitBullet = SM2TrapBoot;
        sm2trap.HitBox.GuardBullet = SM2TrapBoot;
        sm2trap.DestroyBullet = SM2TrapLost;

        //破壊可能にする
        HurtBoxManager trapHurtBox = sm2trap.GetComponentInChildren<HurtBoxManager>();
        trapHurtBox.OnHurtWithTransform = SM2Trap2Break;
        trapHurtBox.SetPlayerNum(PlayerNum);
        trapHurtBox.SetActive(true);

        //アニメーション
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
    /// 超必殺技
    /// </summary>
    public async UniTask Ultimate()
    {
        if (!CanEveryAction || _characterState.CurrentUP < 100) return;

        //空中不可
        if (!OnGround) return;

        //UP消費
        _characterState.SetCurrentUP(-100);

        // 新しいCTSを生成
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltTrigger");
        _animator.SetFloat("WalkFloat", 0);
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        //物理挙動
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
            // 攻撃処理が完了した後、トークンを解放
            _ultCTS.Dispose();
            _ultCTS = null;
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    //着地時にジャンプ攻撃をキャンセル
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
