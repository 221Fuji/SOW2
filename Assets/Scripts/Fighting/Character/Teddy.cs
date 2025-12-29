using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Teddy : CharacterActions
{
    [SerializeField] private Transform _kage;
    [SerializeField] private Animator _airKage;
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private HitBoxManager _normalMoveHitBox;
    [SerializeField] private float _gainUramiMeter;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private Bullet _jmBulletPrefab;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private HitBoxManager _specialMove1HitBox;
    [SerializeField] private GroundFire _groundFire;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private Bullet _sm2BulletPrefab;
    [SerializeField] private Vector2 _sm2BulletDirection;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private Bullet _ultBulletPrefab;
    [SerializeField] private int _ultPerformanceFrame;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    private bool _isKageInGround = false;
    private Bullet _jmBullet;
    private Bullet _sm2Bullet = null;
    public float UramiMaxResource { get; } = 50;
    public float CurrentUramiResource { get; private set; }

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _ultCTS;
    private CancellationTokenSource _ultBulletCTS;

    //行動制限の設定
    public override bool CanEveryAction
    {
        get
        {
            return base.CanEveryAction
                && _normalMoveCTS == null
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
            if (_isKageInGround) return false;

            return true;
        }
    }
    public bool CanJumpMove
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;
            if (_jumpMoveCount != 0) return false;
            if (_jmBullet != null) return false;

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
            if (_sm2Bullet) return false;

            return true;
        }
    }
    public bool CanUltimate
    {
        get
        {
            if (!CanEveryAction || _characterState.CurrentUP < 100) return false;
            if (!OnGround) return false;

            return true;
        }
    }

    public override bool CanJump
    {
        get
        {
            if (!base.CanJump) return false;
            if (_isKageInGround) return false;

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
        _normalMoveHitBox.Hit = NormalMoveHit;
        _specialMove1HitBox.InitializeHitBox(_specialMove1Info, gameObject);
    }

    public override void InitializeCA(int playerNum, CharacterActions enemyCA)
    {
        base.InitializeCA(playerNum, enemyCA);
        _sm2Bullet = null;
        AddUramiResource(-UramiMaxResource);
    }

    protected override void Update()
    {
        base.Update();

        if(_kage)
        {
            _airKage.transform.position = new Vector2(transform.position.x, StageParameter.GroundPosY);
        }
    }

    protected override void PlayHurtAnimation()
    {
        if (!_isKageInGround)
        {
            base.PlayHurtAnimation();
        }
        else
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "HurtLayer", 1);
            _animator.SetTrigger("HurtAirTrigger");
        }  
    }

    private void AddUramiResource(float value)
    {
        CurrentUramiResource += value;
        CurrentUramiResource = Mathf.Clamp(CurrentUramiResource, 0, UramiMaxResource);

        if(CurrentUramiResource > 0)
        {
            _characterState.TakeAnormalyState(AnormalyState.SuperArmor);
        }
        if(CurrentUramiResource <= 0)
        {
            _characterState.RecoverAnormalyState(AnormalyState.SuperArmor);
        }
    }

    public override async UniTask TakeAttack(AttackInfo attackInfo)
    {
        await base.TakeAttack(attackInfo);
        AddUramiResource(-attackInfo.Damage);
    }

    /// <summary>
    /// 通常攻撃
    /// </summary>
    public async UniTask NormalMove()
    {
        //ジャンプ中ならジャンプ攻撃の処理を行う
        if (!OnGround)
        {
            JumpMove().Forget();
            return;
        }

        if (!CanNormalMove) return;

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
            await WaitForActiveFrame(_normalMoveHitBox, _normalMoveInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_normalMoveInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃をキャンセル");
            _normalMoveHitBox.SetIsActive(false);
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

    private void NormalMoveHit()
    {
        AddUramiResource(_gainUramiMeter);
    }

    /// <summary>
    /// ジャンプ攻撃
    /// </summary>
    public async UniTask JumpMove()
    {
        if (!CanJumpMove) return;

        //ジャンプ攻撃したの回数を記録
        _jumpMoveCount++;

        //SP消費
        _characterState.SetCurrentSP(-_jumpMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_jumpMoveInfo.MeterGain);

        await CreateJmBullet();
    }

    private async UniTask CreateJmBullet()
    {
        //弾の座標
        Vector2 bulletPos = new Vector2(transform.position.x, StageParameter.GroundPosY);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            rotation = new Quaternion(0, 180, 0, 0);
        }
        _jmBullet = Instantiate(_jmBulletPrefab, bulletPos, rotation);
        _jmBullet.Velocity = Vector2.zero;

        //弾の当たり判定設定
        _jmBullet.HitBox.InitializeHitBox(_jumpMoveInfo, gameObject);

        try
        {
            await FrameManager.DeleyFightingFrame(_jumpMoveInfo.StartupFrame);
            _jmBullet.HitBox.SetIsActive(true);
            await FrameManager.DeleyFightingFrame(_jumpMoveInfo.ActiveFrame);
            _jmBullet.HitBox.SetIsActive(false);
            await FrameManager.DeleyFightingFrame(_jumpMoveInfo.ActiveFrame);
        }
        catch 
        {
            return;
        }

        if (_jmBullet != null)
        {
            Destroy(_jmBullet.gameObject);
            _jmBullet = null;
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

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_specialMove1Info.ConsumptionSP);

        //UP回収
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_specialMove1HitBox, _specialMove1Info.ActiveFrame, token); // 持続を待つ
            CreateGroundFire();
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _specialMove1HitBox.SetIsActive(false);
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

    private void CreateGroundFire()
    {
        GroundFire groundFire = Instantiate(_groundFire);
        groundFire.Initialize(_characterState.IsLeftSide, this);
        groundFire.SetIsActive(true);
        Vector2 posOffset = new Vector2(4.75f, StageParameter.GroundPosY);
        if (!_characterState.IsLeftSide)
        {
            posOffset *= new Vector2(-1, 1);
        }
        groundFire.transform.position = transform.position + (Vector3)posOffset;
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
        UPgain(_specialMove2Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove2Info.StartupFrame, token); // 発生を待つ
            CreateSm2Bullet(token);
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            //追加
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
        Vector2 bulletVelocity = _sm2BulletDirection;
        Vector2 bulletPosOffset = new Vector2(3.5f, 0.75f);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        _sm2Bullet = Instantiate(_sm2BulletPrefab, bulletPos, rotation);
        _sm2Bullet.Velocity = bulletVelocity;

        //弾の当たり判定設定
        _sm2Bullet.HitBox.InitializeHitBox(_specialMove2Info, gameObject);
        _sm2Bullet.HitBox.HitBullet = Sm2BulletHit;
        _sm2Bullet.HitBox.GuardBullet = Sm2BulletHit;
        _sm2Bullet.DestroyBullet = Sm2BulletHit;

        try
        {
            await WaitForActiveFrame(_sm2Bullet.HitBox, _specialMove2Info.ActiveFrame, token);
        }
        finally
        {
            Sm2BulletHit(_sm2Bullet);
        }
    }

    private async void Sm2BulletHit(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.Velocity = Vector2.zero;
        bullet.SetGravityScale(0);
        bullet.GetComponent<Animator>().SetTrigger("HitTrigger");

        await FrameManager.DeleyFightingFrame(30);

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
        _characterState.SetCurrentUP(-100);

        // 新しいCTSを生成
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltTrigger");

        //物理挙動
        Velocity = Vector2.zero;

        //演出
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, _ultPerformanceFrame);
        _characterState.SetIsUltPerformance();

        AddUramiResource(UramiMaxResource);

        try
        {
            await StartUpMove(_ultimateInfo.StartupFrame, token); // 発生を待つ
            await CreateUltBullet(token);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _ultCTS.Dispose();
            _ultCTS = null;
            _hurtBox.SetActive(true);
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    private async UniTask CreateUltBullet(CancellationToken token)
    {
        //弾の座標
        Vector2 bulletPosOffset = new Vector2(6f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_ultBulletPrefab, bulletPos, rotation);
        bullet.Velocity = Vector2.zero;

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_ultimateInfo, gameObject);

        try
        {
            await WaitForActiveFrame(bullet.HitBox, _ultimateInfo.ActiveFrame, token);
            await RecoveryFrame(_ultimateInfo.RecoveryFrame, token);
        }
        finally
        {
            if(bullet)
            {
                Destroy(bullet.gameObject);
            }
        }
    }

    protected async override void Land()
    {
        _jumpMoveCount = 0;
        _airKage.SetTrigger("OutGroundTrigger");

        try
        {
            await UniTask.WaitUntil(() =>
            {
                if (_airKage == null) return true;
                var startStateInfo = _airKage.GetCurrentAnimatorStateInfo(0);
                return startStateInfo.IsName("OutGround") && startStateInfo.normalizedTime >= 0.9f;
            });
        }
        catch
        {
            return;
        }
        
        _kage?.gameObject?.SetActive(true);
        _airKage?.gameObject?.SetActive(false);
        _isKageInGround = false;
    }

    protected override void LeaveGround()
    {
        _airKage.gameObject.SetActive(true);
        _kage.gameObject.SetActive(false);
        _airKage.SetTrigger("InGroundTrigger");
        _isKageInGround = true;
    }

    public override void CancelActionByHit()
    {
        _normalMoveCTS?.Cancel();
        if(_jmBullet != null)
        {
            _jmBullet.HitBox.SetIsActive(false);
        }
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _ultBulletCTS?.Cancel();
        //Ult発動中の固定化解除
        SetIsFixed(false);
    }
}
