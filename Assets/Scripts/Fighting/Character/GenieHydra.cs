using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GenieHydra : CharacterActions
{
    [Header("歩き速度")]
    [SerializeField] private float _frontWalkSpeedM;
    [SerializeField] private float _backWalkSpeedM;
    [SerializeField] private float _frontWalkSpeedU;
    [SerializeField] private float _backWalkSpeedU;
    [Header("ジャンプの高さ")]
    [SerializeField] private float _jumpPowerM;
    [SerializeField] private float _jumpPowerU;
    [Header("通常攻撃_M")]
    [SerializeField] private AttackInfo _normalMoveMInfo;
    [SerializeField] private HitBoxManager _normalMoveMHitBox;
    [Header("通常攻撃_U")]
    [SerializeField] private AttackInfo _normalMoveUInfo;
    [SerializeField] private HitBoxManager _normalMoveUHitBox;
    [SerializeField] private Bullet _nmSpikePrefab;
    [SerializeField] private AttackInfo _nmSpikeInfo;
    [SerializeField] private Bullet _nmBigPrefab;
    [SerializeField] private AttackInfo _nmBigInfo;
    [Header("ジャンプ攻撃_M")]
    [SerializeField] private AttackInfo _jumpMoveMInfo;
    [SerializeField] private HitBoxManager _jumpMoveMHitBox;
    [Header("ジャンプ攻撃_U")]
    [SerializeField] private AttackInfo _jumpMoveUInfo;
    [SerializeField] private Bullet _jmUBulletPrefab;
    [SerializeField] private Vector2[] _jmUVelocity;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private HitBoxManager _specialMove1HitBox;
    [Header("必殺技2_M")]
    [SerializeField] private AttackInfo _specialMove2MInfo;
    [SerializeField] private HitBoxManager _specialMove2MHitBox;
    [Header("必殺技2_U")]
    [SerializeField] private AttackInfo _specialMove2UInfo;
    [SerializeField] private Bullet _iceWall;
    [Header("超必殺技1_M")]
    [SerializeField] private AttackInfo _ultimate1MInfo;
    [SerializeField] private Bullet _ult1FootPrefab;
    [SerializeField] private int _ult1MPerformanceFrame;
    [Header("超必殺技1_U")]
    [SerializeField] private AttackInfo _ultimate1UInfo;
    [SerializeField] private Bullet _ult1FistPrefab;
    [SerializeField] private int _ult1UPerformanceFrame;
    [Header("超必殺技2")]
    [SerializeField] private AttackInfo _ultimate2Info;
    [SerializeField] private HitBoxManager _ult2HitBox;
    [SerializeField] private Bullet _ult2BulletPrefab;
    [SerializeField] private int _ult2PerformanceFrame;
    [Header("超必殺技2(最終弾)")]
    [SerializeField] private AttackInfo _ultimate2LastInfo;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    private bool _isMander = true; //Manderかどうか

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
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
            if (!OnGround) return false; //空中不可

            return true;
        }
    }
    public bool CanUltimate
    {
        get
        {
            if (!CanEveryAction || _characterState.CurrentUP < 100) return false;
            if (!OnGround) return false; //空中不可

            return true;
        }
    }

    public override void InitializeCA(int playerNum, CharacterActions enemyCA)
    {
        base.InitializeCA(playerNum, enemyCA);
        _isMander = true;
        _animator.SetBool("ManderBool", true);
    }

    protected override void SetActionDelegate()
    {
        _inputReciever.JumpDelegate = Jump;
        _inputReciever.GuardDelegate = GuardStance;
        _inputReciever.NormalMove = NormalMoveM;
        _inputReciever.SpecialMove1 = SpecialMove1;
        _inputReciever.SpecialMove2 = SpecialMove2M;
        _inputReciever.Ultimate = Ultimate1;
    }

    protected override void SetHitBox()
    {
        _normalMoveMHitBox.InitializeHitBox(_normalMoveMInfo, gameObject);
        _normalMoveUHitBox.InitializeHitBox(_normalMoveUInfo, gameObject);
        _jumpMoveMHitBox.InitializeHitBox(_jumpMoveMInfo, gameObject);
        _specialMove1HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        _specialMove2MHitBox.InitializeHitBox(_specialMove2MInfo, gameObject);
        _ult2HitBox.InitializeHitBox(_ultimate2Info, gameObject);
        _ult2HitBox.Guard = GuardUlt2;
    }

    /// <summary>
    /// Undiなら被ダメ増加
    /// </summary>
    public override async UniTask TakeAttack(AttackInfo attackInfo)
    {
        if (!_isMander)
        {
            attackInfo.Damage *= 1.25f;
        }
        await base.TakeAttack(attackInfo);
    }

    /// <summary>
    /// 通常攻撃M
    /// </summary>
    public async UniTask NormalMoveM()
    {
        if (!CanNormalMove) return;

        if(!_isMander)
        {
            NormalMoveU().Forget();
            return;
        }

        //ジャンプ中ならジャンプ攻撃の処理を行う
        if (!OnGround)
        {
            JumpMoveM().Forget();
            return;
        }

        // 新しいCTSを生成
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveMInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveMInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveMInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_normalMoveMHitBox, _normalMoveMInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_normalMoveMInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃をキャンセル");
            _normalMoveMHitBox.SetIsActive(false);
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

    public async UniTask NormalMoveU()
    {
        //ジャンプ中ならジャンプ攻撃の処理を行う
        if (!OnGround)
        {
            JumpMoveU().Forget();
            return;
        }

        // 新しいCTSを生成
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveUInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveUInfo.MeterGain);

        try
        {

            NmBulletSpike(token);
            NmBulletBig(token);

            await StartUpMove(_normalMoveUInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_normalMoveUHitBox, _normalMoveUInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_normalMoveUInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃をキャンセル");
            _normalMoveMHitBox.SetIsActive(false);
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

    private async void NmBulletSpike(CancellationToken token)
    {
        //弾の座標
        Vector2 bulletPosOffset = new Vector2(3, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1, 0);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_nmSpikePrefab, bulletPos, rotation);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_normalMoveUInfo, gameObject);
        bullet.DestroyBullet = BulletExplode;
        bullet.HitBox.HitBullet = FalseActiveBullet;
        bullet.HitBox.GuardBullet = FalseActiveBullet;

        try
        {
            await StartUpMove(_nmSpikeInfo.StartupFrame, token);

            //アニメーション
            bullet.GetComponent<Animator>().SetTrigger("NmSpikeTrigger");

            await WaitForActiveFrame(bullet.HitBox, _nmSpikeInfo.ActiveFrame, token);
        }
        finally
        {
            BulletExplode(bullet);
        }
    }

    private async void NmBulletBig(CancellationToken token)
    {
        //弾の座標
        Vector2 bulletPosOffset = new Vector2(6.5f, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1, 0);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_nmBigPrefab, bulletPos, rotation);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_normalMoveUInfo, gameObject);
        bullet.DestroyBullet = BulletExplode;
        bullet.HitBox.HitBullet = FalseActiveBullet;
        bullet.HitBox.GuardBullet = FalseActiveBullet;

        try
        {
            await StartUpMove(_nmBigInfo.StartupFrame, token);

            //アニメーション
            bullet.GetComponent<Animator>().SetTrigger("NmBigTrigger");

            await WaitForActiveFrame(bullet.HitBox, _nmBigInfo.ActiveFrame, token);
        }
        finally
        {
            BulletExplode(bullet);
        }
    }

    private async void BulletExplode(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.Velocity = Vector2.zero;

        //アニメーション
        bullet.GetComponent<Animator>().SetTrigger("ExplodeTrigger");

        await FrameManager.DeleyFightingFrame(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
        }
    }

    /// <summary>
    /// ジャンプ攻撃
    /// </summary>
    public async UniTask JumpMoveM()
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
        _characterState.SetCurrentSP(-_jumpMoveMInfo.ConsumptionSP);

        //UP回収
        UPgain(_jumpMoveMInfo.MeterGain);

        try
        {
            await StartUpMove(_jumpMoveMInfo.StartupFrame, token); // 発生を待つ
            await RecoveryFrame(_jumpMoveMInfo.RecoveryFrame, token); // 硬直を待つ
            await WaitForActiveFrame(_jumpMoveMHitBox, _jumpMoveMInfo.ActiveFrame, token); // 持続を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("ジャンプ攻撃をキャンセル");
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _jumpMoveCTS.Dispose();
            _jumpMoveCTS = null;
        }
    }

    /// <summary>
    /// ジャンプ攻撃
    /// </summary>
    public async UniTask JumpMoveU()
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
        _characterState.SetCurrentSP(-_jumpMoveUInfo.ConsumptionSP);

        //UP回収
        UPgain(_jumpMoveUInfo.MeterGain);

        //物理挙動
        Velocity = Vector2.zero;
        SetIsFixed(true);

        try
        {
            await StartUpMove(_jumpMoveUInfo.StartupFrame, token); // 発生を待つ
            CreateJmUBullet(token);
            await RecoveryFrame(_jumpMoveUInfo.RecoveryFrame, token); // 硬直を待つ
            AddForce(new Vector2(0, -2.5f));
        }
        catch (OperationCanceledException)
        {
            Debug.Log("ジャンプ攻撃をキャンセル");
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _jumpMoveCTS.Dispose();
            _jumpMoveCTS = null;

            //物理挙動
            SetIsFixed(false);
        }
    }

    private async void CreateJmUBullet(CancellationToken token)
    {
        //弾の座標と速度設定
        Vector2 bulletVelocity = new Vector2(1, 1);
        Vector2 bulletPosOffset = new Vector2(2.5f, 3);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;

        for(int i = 0; i < _jmUVelocity.Length; i++)
        {
            Bullet bullet = Instantiate(_jmUBulletPrefab, bulletPos, rotation);
            bullet.Velocity = (_jmUVelocity[i] * bulletVelocity).normalized * 20;

            bullet.GetComponent<Animator>().SetInteger("BulletNumInt", i);

            //弾の当たり判定設定
            bullet.HitBox.InitializeHitBox(_jumpMoveUInfo, gameObject);
            bullet.HitBox.HitBullet = BulletJmUExplode;
            bullet.HitBox.GuardBullet = BulletJmUExplode;
            bullet.DestroyBullet = BulletJmUExplode;
            bullet.HitBox.SetIsActive(true);

            try
            {
                await FrameManager.DeleyFightingFrame(2, token);
            }
            catch
            {
                break;
            }
        }
    }

    private async void BulletJmUExplode(Bullet bullet)
    {
        if (bullet == null) return;

        //バグ補強
        if(bullet.HitBox.IsActive)
        {
            bullet.HitBox.SetIsActive(false);
        }

        bullet.Velocity = Vector2.zero;

        //アニメーション
        bullet.GetComponent<Animator>().SetTrigger("ExplodeTrigger");
        await FrameManager.DeleyFightingFrame(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
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

        //SP回復
        _characterState.SetCurrentSP(_specialMove1Info.ConsumptionSP);

        //UP回収
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // 発生を待つ

            //変身
            Metamorphosis(!_isMander);

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

    private void Metamorphosis(bool toMander)
    {
        _animator.SetBool("ManderBool", toMander);
        _isMander = toMander;

        if(toMander)
        {
            _characterState.SetWalkSpeed(_frontWalkSpeedM, _backWalkSpeedM);
            _characterState.SetJumpPower(_jumpPowerM);
        }
        else
        {
            _characterState.SetWalkSpeed(_frontWalkSpeedU, _backWalkSpeedU);
            _characterState.SetJumpPower(_jumpPowerU);
        }
        
    }

    /// <summary>
    /// 必殺技２
    /// </summary>
    public async UniTask SpecialMove2M()
    {
        if (!CanSpecialMove2) return;

        if (!_isMander)
        {
            SpecialMove2U().Forget();
            return;
        }

        // 新しいCTSを生成
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("SpecialMove2Trigger");

        //SP消費
        _characterState.SetCurrentSP(-_specialMove2MInfo.ConsumptionSP);

        //UP回収
        UPgain(_specialMove2MInfo.MeterGain);

        _characterState.TakeAnormalyState(AnormalyState.SuperArmor);

        try
        {
            await StartUpMove(_specialMove2MInfo.StartupFrame, token); // 発生を待つ

            _specialMove2MHitBox.SetIsActive(true);

            //物理挙動
            Vector2 chargeVector = new Vector2(15, 0);
            if(!_characterState.IsLeftSide)
            {
                chargeVector *= new Vector2(-1, 1);
            }
            for(int i = 0; i < _specialMove2MInfo.ActiveFrame; i++)
            {
                Velocity = chargeVector;
                await FrameManager.DeleyFightingFrame(1, token);
            }
            _specialMove2MHitBox.SetIsActive(false);
            if (_specialMove2MHitBox.IsActive)
            {
                OnMissAI?.Invoke();
            }
            _characterState.RecoverAnormalyState(AnormalyState.SuperArmor);

            await RecoveryFrame(_specialMove2MInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _specialMove2MHitBox.SetIsActive(false);
            Velocity = Vector2.zero;
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

    /// <summary>
    /// 必殺技２
    /// </summary>
    public async UniTask SpecialMove2U()
    {
        // 新しいCTSを生成
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("SpecialMove2Trigger");

        //SP消費
        _characterState.SetCurrentSP(-_specialMove2UInfo.ConsumptionSP);

        //UP回収
        UPgain(_specialMove2UInfo.MeterGain);

        try
        {
            await StartUpMove(_specialMove2UInfo.StartupFrame, token); // 発生を待つ
            CreateIceWall(token);
            await RecoveryFrame(_specialMove2UInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _specialMove2MHitBox.SetIsActive(false);
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

    private async void CreateIceWall(CancellationToken token)
    {
        //座標設定
        Vector2 wallPosOffset = new Vector2(EnemyCA.GetPushBackBox().xMax + 5,
                                StageParameter.GroundPosY);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            wallPosOffset = new Vector2(EnemyCA.GetPushBackBox().xMin -5, 
                            StageParameter.GroundPosY);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Bullet iceWall = Instantiate(_iceWall, wallPosOffset, rotation);
        iceWall.Velocity = Vector2.zero;

        iceWall.HitBox.InitializeHitBox(_specialMove2UInfo, gameObject);
        iceWall.HitBox.HitBullet = HitWall;
        iceWall.HitBox.GuardBullet = HitWall;

        try
        {
            iceWall.HitBox.SetIsActive(true);
            await FrameManager.DeleyFightingFrame(_specialMove2UInfo.ActiveFrame);
        }
        finally 
        {
            iceWall.HitBox.SetIsActive(false);
            BulletExplode(iceWall);
        }
    }

    private async void HitWall(Bullet bullet)
    {
        bullet.HitBox.SetIsActive(false);

        try
        {
            await FrameManager.DeleyFightingFrame(10);
        }
        finally 
        {
            bullet.HitBox.SetIsActive(true);
        }
    }

    /// <summary>
    /// 超必殺技
    /// </summary>
    public async UniTask Ultimate1()
    {

        if (!CanUltimate) return;

        if(_characterState.CurrentHP <= 30)
        {
            Ultimate2().Forget();
            return;
        }

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
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, _ult1MPerformanceFrame);
        _characterState.SetIsUltPerformance();
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        //発動保障
        try
        {
            //演出解除
            await FrameManager.DeleyFightingFrame(1, token);
            _animator.updateMode = AnimatorUpdateMode.Normal;

            await StartUpMove(_ultimate1MInfo.StartupFrame, token); // 発生を待つ
        }
        finally
        {
            if(_isMander)
            {
                CreateUlt1Foot();
            }
            else
            {
                CreateUlt1Fist();
            }          
        }

        try
        {
            await RecoveryFrame(_ultimate1MInfo.RecoveryFrame, token); // 硬直を待つ
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

    private async void CreateUlt1Foot()
    {
        //弾の座標と速度設定
        Vector2 footPos = new Vector2(EnemyCA.GetPushBackBox().center.x, StageParameter.GroundPosY);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Bullet bullet = Instantiate(_ult1FootPrefab, footPos, rotation);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_ultimate1MInfo, gameObject);
        bullet.HitBox.HitBullet = FalseActiveBullet;
        bullet.HitBox.GuardBullet = FalseActiveBullet;
        bullet.HitBox.SetIsActive(true);

        try
        {
            await FrameManager.DeleyFightingFrame(_ultimate1MInfo.ActiveFrame);
        }
        finally
        {
            if(bullet)
            {
                Destroy(bullet.gameObject);
            }
        }
    }

    private async void CreateUlt1Fist()
    {
        //弾の座標と速度設定
        Vector2 fistPos = new Vector2(EnemyCA.GetPushBackBox().center.x, StageParameter.GroundPosY);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Bullet bullet = Instantiate(_ult1FistPrefab, fistPos, rotation);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_ultimate1UInfo, gameObject);
        bullet.HitBox.HitBullet = FalseActiveBullet;
        bullet.HitBox.GuardBullet = FalseActiveBullet;
        bullet.HitBox.SetIsActive(true);

        try
        {
            await FrameManager.DeleyFightingFrame(_ultimate1UInfo.ActiveFrame);
        }
        finally
        {
            if (bullet)
            {
                Destroy(bullet.gameObject);
            }
        }
    }
    private void FalseActiveBullet(Bullet bullet)
    {
        if (!bullet) return;
        bullet.HitBox.SetIsActive(false);
    }

    private async UniTask Ultimate2()
    {
        //UP消費
        _characterState.SetCurrentUP(-100);

        // 新しいCTSを生成
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("Ult2Trigger");

        //物理挙動
        Velocity = Vector2.zero;

        //演出
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, _ult2PerformanceFrame);
        _characterState.SetIsUltPerformance();
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;


        try
        {
            //演出解除
            await FrameManager.DeleyFightingFrame(1, token);
            _animator.updateMode = AnimatorUpdateMode.Normal;

            //最終弾以外の設定
            _ult2HitBox.InitializeHitBox(_ultimate2Info, gameObject);
            _ult2HitBox.Hit = HitUlt2;

            await StartUpMove(_ultimate2Info.StartupFrame, token); // 発生を待つ

            _ult2HitBox.SetIsActive(true);
            await FrameManager.DeleyFightingFrame(_ultimate2Info.ActiveFrame, token);
            _ult2HitBox.SetIsActive(true);

            //最終弾の設定
            _ult2HitBox.InitializeHitBox(_ultimate2LastInfo, gameObject);
            _ult2HitBox.Hit = HitUlt2Last;

            await RecoveryFrame(_ultimate2Info.RecoveryFrame, token); // 硬直を待つ
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

    //Ult2のAnimationからイベントとして呼ばれる
    private async void CreateUlt2Bullet()
    {
        //弾の座標と速度設定
        float offset = (176f / 256f) //PixelPerUnitから動かしたPixel数を計算
                        * 15f;       //scale分を掛ける
        Vector2 bullet2PosOffset = new Vector2(offset, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            rotation = new Quaternion(0, 180, 0, 0);
            bullet2PosOffset *= new Vector2(-1, 0);
        }
        Bullet bullet1 = Instantiate(_ult2BulletPrefab, transform.position, rotation);
        Bullet bullet2 = Instantiate(_ult2BulletPrefab, transform.position + (Vector3)bullet2PosOffset, rotation);

        // 新しいCTSを生成
        _ultBulletCTS = new CancellationTokenSource();
        CancellationToken token = _ultBulletCTS.Token;

        try
        {
            await UniTask.WaitUntil(() =>
            {
                return _ultCTS == null;
            }, cancellationToken :token);
        }
        finally
        {
            if(bullet1)
            {
                Destroy(bullet1.gameObject);
            }

            if(bullet2)
            {
                Destroy(bullet2.gameObject);
            }
        }
    }

    private void HitUlt2()
    {
        EnemyCA.Velocity = Vector2.zero;

        if(_characterState.IsLeftSide)
        {
            EnemyCA.transform.position += new Vector3(0.1f, 0, 0);
        }
        else
        {
            EnemyCA.transform.position += new Vector3(-0.1f, 0, 0);
        }

        GuardUlt2();
    }

    private async void GuardUlt2()
    {
        if (_ult2HitBox.IsActive)
        {
            _ult2HitBox.SetIsActive(false);
        }

        try
        {
            await FrameManager.DeleyFightingFrame(2);
        }
        finally
        {
            if (!_ult2HitBox.IsActive)
            {
                _ult2HitBox.SetIsActive(true);
            }
        }
    }

    private void HitUlt2Last()
    {
        EnemyCA.SetGravityScale(1);

        if (_ult2HitBox.IsActive)
        {
            _ult2HitBox.SetIsActive(false);
        }
    }

    //着地時にジャンプ攻撃をキャンセル
    protected override void Land()
    {
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;

        SetIsFixed(false);
    }

    public override void CancelActionByHit()
    {
        _normalMoveCTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _jumpMoveCTS?.Cancel();
        _ultCTS?.Cancel();
        _ultBulletCTS?.Cancel();
    }
}
