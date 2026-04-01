using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class Rabiri : CharacterActions
{
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private Bullet _nmBulletPrefab;
    [SerializeField] private Vector2 _nmBulletOffset;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [SerializeField] private Vector2 _warpOffset;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Bullet _sm1BulletPrefab;
    [SerializeField] private float _sm1BulleetSpeed;
    [SerializeField] private Bullet _seekBarPrefab;
    [SerializeField] private float _sm1BackSpeed;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private Bullet _sm2ControllerPrefab;
    [SerializeField] private AttackInfo _additionalSm2Info;
    [SerializeField] private int _additionalInputFrame;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private GameObject _ultFloat;
    [SerializeField] private int _ultPerformanceFrame;
    [SerializeField] private int _overFlowFrame;
    [SerializeField] private int _gageCoolFrame;
    [Header("UI")]
    [SerializeField] private List<GameObject> _stackLampPrefab;


    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    private Bullet _seekBar = null;
    private bool _isWaitingForAdditionalInput = false;
    private bool _isOverFlow = false;
    private bool _isOverClock = false;
    private int _offCountFrame = 0;
    private int _gcfCountFrame = 0;
    private int _ocCountFrame = 0;

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _nmBulletCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _sm1BulletCTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _sm2ControllerCTS;
    private CancellationTokenSource _ultCTS;

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
                && _ultCTS == null
                && _sm2ControllerCTS == null
                && !_isOverFlow;
        }
    }
    public bool CanNormalMove
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;
            if (_nmBulletCTS != null) return false;

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
            if (_sm1BulletCTS != null) return false;

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

    /// <summary>
    /// ゲージにスタックされる技の種類
    /// </summary>
    public enum EStackMove
    {
        NormalMove,
        SpecialMove1,
        SpecialMove2
    }
    private List<EStackMove> _stackMoves = new List<EStackMove>();
    public List<EStackMove> StackMoves
    {
        get { return _stackMoves; }
    }
    public List<GameObject> LampPrefabList
    {
        get { return _stackLampPrefab; }
    }
    public RectTransform InstantiateLamp(int moveIndex)
    {
        Debug.Log("ランプ");
        return Instantiate(LampPrefabList[moveIndex]).GetComponent<RectTransform>();
    }
    public void DestroyLamp(GameObject lamp)
    {
        Debug.Log("aaa");
        if (lamp == null) return;
        Destroy(lamp);
    }
    public Animator RabiriGage
    {
        get; set;
    } = null;

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
    }

    private void AddStack(EStackMove move)
    {
        _stackMoves.Add(move);

        // オーバーフロー処理
        if(_stackMoves.Count() > 12)
        {
            Debug.Log("オーバーフロー");
            _isOverFlow = true;
            _stackMoves = new List<EStackMove>();
            RabiriGage?.SetBool("OverFlowBool", true);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "OverFlowLayer", 1);
            _animator.SetTrigger("OverFlowTrigger");
        }
    }

    protected override void FightingUpdate()
    {
        base.FightingUpdate();

        //
        //ガード継続によるゲージ減少
        //
        if (_characterState.IsGuarding)
        {
            _gcfCountFrame++;
            if (_gcfCountFrame >= _gageCoolFrame)
            {
                _gcfCountFrame = 0;
                if (_stackMoves.Count() >= 1)
                {
                    _stackMoves.RemoveAt(_stackMoves.Count() - 1);
                }
            }
        }
        else
        {
            _gcfCountFrame = 0;
        }

        // オーバーフロー回復処理
        if (_isOverFlow)
        {
            if( _offCountFrame < _overFlowFrame)
            {
                _offCountFrame++;
            }
            else
            {
                _isOverFlow = false;
                _offCountFrame = 0;
                // アニメーション処理
                RabiriGage?.SetBool("OverFlowBool", false);
                AnimatorByLayerName.SetLayerWeightByName(_animator, "OverFlowLayer", 0);
            }
        }

        if (_isOverClock)
        {
            _ocCountFrame++;
            if (_ocCountFrame >= _ultimateInfo  .ActiveFrame)
            {
                _ocCountFrame = 0;
                _isOverClock = false;
                RabiriGage?.SetBool("OverClockBool", false);
                _ultFloat.SetActive(true);
            }
        }
    }

    private float CalcMoveSpeed(int stack)
    {
        return ((float)stack / 12.0f) + 1.0f;
    }

    private AttackInfo GetInfoWithStack(AttackInfo attackInfo, float moveSpeed)
    {
        attackInfo.StartupFrame =  (int)(attackInfo.StartupFrame / moveSpeed);
        attackInfo.ActiveFrame = (int)(attackInfo.ActiveFrame / moveSpeed);
        attackInfo.RecoveryFrame = (int)(attackInfo.RecoveryFrame / moveSpeed);
        Debug.Log(attackInfo.Name + attackInfo.RecoveryFrame);
        return attackInfo;
    }

    public override void InitializeCA(int playerNum, CharacterActions enemyCA)
    {
        RabiriGage = null;
        _gcfCountFrame = 0;
        _offCountFrame = 0;
        _ocCountFrame = 0;
        _isOverClock = false;
        _ultFloat.SetActive(false);

        base.InitializeCA(playerNum, enemyCA);
    }

    public override UniTask Guard(AttackInfo attackInfo)
    {
        //ガード成功によるゲージ減少
        if (_stackMoves.Count() >= 1)
        {
            _stackMoves.RemoveAt(_stackMoves.Count() - 1);
        }
        return base.Guard(attackInfo);
    }

    /// <summary>
    /// 通常攻撃
    /// </summary>
    public async UniTask NormalMove()
    {
        if (!CanNormalMove) return;

        //ジャンプ中ならジャンプ攻撃の処理を行う
        if (!OnGround)
        {
            JumpMove().Forget();
            return;
        }

        // 新しいCTSを生成
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        // スタック処理
        float moveSpeed = CalcMoveSpeed(StackMoves.Count(x => x == EStackMove.NormalMove));
        AttackInfo attackInfo = GetInfoWithStack(_normalMoveInfo, moveSpeed);
        _animator.SetFloat("NmSpeedFloat", moveSpeed);

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(attackInfo.StartupFrame, token); // 発生を待つ
            if (!_isOverClock)
            {
                AddStack(EStackMove.NormalMove);
            }
            CreateNmBullet(attackInfo);
            await RecoveryFrame(attackInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            Debug.Log("通常攻撃をキャンセル");
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

    private async void CreateNmBullet(AttackInfo attackInfo)
    {
        //弾の座標
        Vector2 bulletPosOffset = _nmBulletOffset;
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_nmBulletPrefab, bulletPos, rotation);
        bullet.Velocity = Vector2.zero;

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(attackInfo, gameObject);
        bullet.DestroyBullet = DestroyNmBullet;

        // 新しいCTSを生成
        _nmBulletCTS = new CancellationTokenSource();
        CancellationToken token = _nmBulletCTS.Token;

        try
        {
            await WaitForActiveFrame(bullet.HitBox, attackInfo.ActiveFrame, token);
        }
        finally
        {
            DestroyNmBullet(bullet);
            // 攻撃処理が完了した後、トークンを解放
            _nmBulletCTS.Dispose();
            _nmBulletCTS = null;
        }
    }

    private async void DestroyNmBullet(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.GetComponent<Animator>().SetTrigger("NmDestroyTrigger");

        await FrameManager.DeleyFightingFrame(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
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

        //物理挙動
        Velocity = Vector2.zero;
        SetIsFixed(true);

        //SP消費
        _characterState.SetCurrentSP(-_jumpMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_jumpMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_jumpMoveInfo.StartupFrame, token); // 発生を待つ
            if (!_isOverClock)
            {
                AddStack(EStackMove.NormalMove);
            }
            JumpMoveWarp(); // ワープ
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

            SetIsFixed(false);
        }
    }

    private void JumpMoveWarp()
    {
        Vector2 enemyPos = EnemyCA.GetPushBackBox().center;
        Vector2 offset = _warpOffset;
        if (!_characterState.IsLeftSide)
        {
            offset *= new Vector2(-1, 1);
        }
        Vector2 resultPos = enemyPos + offset;
        transform.position = resultPos;
        SetIsFixed(false);
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

        // スタック処理
        float moveSpeed = CalcMoveSpeed(StackMoves.Count(x => x == EStackMove.SpecialMove1));
        AttackInfo attackInfo = GetInfoWithStack(_specialMove1Info, moveSpeed);
        _animator.SetFloat("Sm1SpeedFloat", moveSpeed);

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
            if (!_isOverClock)
            {
                AddStack(EStackMove.SpecialMove1);
            }
            CreateSm1Bullet();
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _specialMove1CTS.Dispose();
            _specialMove1CTS = null;
            DestroySeekBar();
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
    }

    private void CreateSm1Bullet()
    {
        //弾の座標と速度設定
        Vector2 bulletVelocity = new Vector2(_sm1BulleetSpeed, 0);
        Vector2 bulletPosOffset = new Vector2(2, 0.5f);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_sm1BulletPrefab, bulletPos, rotation);
        bullet.Velocity = bulletVelocity;

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        bullet.DestroyBullet = DestroySm1Bullet;
        bullet.HitBox.SetIsActive(true);
    }

    private void DestroySm1Bullet(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.Velocity = Vector2.zero;

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
        }
    }

    /// <summary>
    /// Sm1アニメーションからイベントとして呼ばれる
    /// </summary>
    private void CreateSeekBar()
    {
        if (_seekBar != null)
        {
            DestroySeekBar();
        }

        SetGravityScale(0);
        transform.position += new Vector3(0, 0.1f, 0); 
        float sm1BackVelocity = _sm1BackSpeed;
        //seekBarの座標
        Vector2 seekBarPosOffset = new Vector2(2, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            seekBarPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
            sm1BackVelocity *= -1;
        }
        Vector2 bulletPos = (Vector2)transform.position + seekBarPosOffset;
        Bullet seekbar = Instantiate(_seekBarPrefab, bulletPos, rotation);
        seekbar.Velocity = Vector2.zero;
        _seekBar = seekbar;
        Velocity = new Vector2(sm1BackVelocity, 0);
    }

    /// <summary>
    /// SeekBarアニメーションからも呼ばれる
    /// </summary>
    private async void DestroySeekBar()
    {
        SetGravityScale(1);
        if (_seekBar == null) return;

        Velocity = Vector2.zero;

        _seekBar.GetComponent<Animator>().SetTrigger("SeekBarDestroyTrigger");
        await FrameManager.DeleyFightingFrame(30);
        if (!_seekBar.gameObject)
        {
            Destroy(_seekBar.gameObject);
            _seekBar = null;
        } 
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

        // スタック処理
        float moveSpeed = CalcMoveSpeed(StackMoves.Count(x => x == EStackMove.SpecialMove2));
        AttackInfo attackInfo = GetInfoWithStack(_specialMove2Info, moveSpeed);

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
            if (!_isOverClock)
            {
                AddStack(EStackMove.SpecialMove2);
            }           
            CreateSm2Bullet();
            await RecoveryFrame(attackInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            
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

    private async void CreateSm2Bullet()
    {
        //弾の座標設定
        Vector2 bulletPosOffset = new Vector2(0, 0);
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1, 1);
            rotation = new Quaternion(0, 180, 0, 0);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_sm2ControllerPrefab, bulletPos, rotation);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_specialMove2Info, gameObject);
        bullet.DestroyBullet = DestroySm1Bullet;
        bullet.HitBox.HitBullet = HitSm2;

        // 新しいCTSを生成
        _sm2ControllerCTS = new CancellationTokenSource();
        CancellationToken token = _sm2ControllerCTS.Token;

        try
        {
            _animator.SetInteger("InputDirInt", -1);
            await WaitForActiveFrame(bullet.HitBox, _specialMove2Info.ActiveFrame, token);
            await UniTask.WaitUntil(() => {
                return !_isWaitingForAdditionalInput;
            }, cancellationToken:token);
        }
        finally
        {
            DestroySm2Bullet(bullet);
            _sm2ControllerCTS.Dispose();
            _sm2ControllerCTS = null;
        }
    }

    private async void HitSm2(Bullet bullet)
    {
        if (_sm2ControllerCTS == null) return;
        try
        {
            _specialMove2CTS?.Cancel();
            _animator.SetInteger("InputDirInt", 0);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
            _isWaitingForAdditionalInput = true;
            await FrameManager.DeleyFightingFrame(_additionalInputFrame, _sm2ControllerCTS.Token); // 入力猶予
            await AdditionalSm2(bullet); // 追加入力

        }
        finally
        {
            _isWaitingForAdditionalInput = false;
            AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
            _animator.SetInteger("InputDirInt", -1);
        }
    }

    private async UniTask AdditionalSm2(Bullet bullet)
    {
        CancellationToken token = _sm2ControllerCTS.Token;
        Vector3 inputDir = _inputReciever.InputDirection;
        AttackInfo sm2AddInfo = _additionalSm2Info;
        int animIndex = 0;

        // 追加入力なし
        if ((Vector2)inputDir == Vector2.zero)
        {
            _animator.SetInteger("InputDirInt", -1);
            return;
        }

        // 一旦1P側だけ考える
        if (inputDir.y >= 0.5f)
        {
            sm2AddInfo.HitBackDirection = new Vector2(0, 15);
            animIndex = 1;
        }
        if (inputDir.x >= 0.5f)
        {
            sm2AddInfo.HitBackDirection = new Vector2(10, 10);
            animIndex = 2;
        }
        if (inputDir.y <= -0.5f)
        {
            sm2AddInfo.HitBackDirection = new Vector2(0, -15);
            animIndex = 3;
        }
        if (inputDir.x <= -0.5f)
        {
            sm2AddInfo.HitBackDirection = new Vector2(-10, 10);
            animIndex = 4;
        }

        // 2P側を考える
        if (!_characterState.IsLeftSide)
        {
            if (animIndex == 2) animIndex = 4;
            else if (animIndex == 4) animIndex = 2;
            sm2AddInfo.HitBackDirection *= new Vector2(-1, 1);
        }
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);

        _animator.SetInteger("InputDirInt", animIndex);
        try
        {
            await StartUpMove(sm2AddInfo.StartupFrame, token);
            // 追撃処理
            EnemyCA.TakeAttack(sm2AddInfo).Forget();
            bullet.GetComponent<Animator>().SetInteger("InputDirInt", animIndex);
            await RecoveryFrame(sm2AddInfo.RecoveryFrame, token);
        }
        catch { }
    }

    private async void DestroySm2Bullet(Bullet bullet) 
    {
        if (bullet == null) return;

        bullet.GetComponent<Animator>().SetTrigger("Sm2DestroyTrigger");

        await FrameManager.DeleyFightingFrame(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
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
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.25f, _ultPerformanceFrame);
        _characterState.SetIsUltPerformance();
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        try
        {
            //演出解除
            await FrameManager.DeleyFightingFrame(1, token);
            _animator.updateMode = AnimatorUpdateMode.Normal;

            // OverClockに入る
            RabiriGage?.SetBool("OverClockBool", true);
            _stackMoves = new List<EStackMove>();
            _ultFloat.SetActive(true);

            await StartUpMove(_ultimateInfo.StartupFrame, token); // 発生を待つ
            await RecoveryFrame(_ultimateInfo.RecoveryFrame, token); // 硬直を待つ
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

    /// <summary>
    /// ガード構えをする
    /// </summary>
    protected override void GuardStance(bool isGuarding)
    {
        if (!CanGuard) return;

        // Layerを変更
        if (isGuarding)
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 1);
            _animator.SetTrigger("GuardTrigger");
            //発動時SP消費
            if (!_characterState.IsGuarding)
            {
                _characterState.SetCurrentSP(-10);
            }
            _characterState.SetIsGuarding(true);
        }
        else
        {
            //ガード解除
            GuardRelease();
            _animator.SetTrigger("GuardCancelTrigger");
        }
    }

    //着地時にジャンプ攻撃をキャンセル
    protected override void Land()
    {
        _animator.SetBool("OnGroundBool", true);
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;
    }

    public override void CancelActionByHit()
    {
        ActionCancel();
        // オーバーフロー解除
        if (_characterState.AcceptOperations)
        {
            _isOverFlow = false;
            RabiriGage?.SetBool("OverFlowBool", false);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "OverFlowLayer", 0);
            _offCountFrame = 0;
        }
    }

    private void ActionCancel()
    {
        _normalMoveCTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _jumpMoveCTS?.Cancel();
        //弾を消す
        _nmBulletCTS?.Cancel();
        _sm1BulletCTS?.Cancel();
        _sm2ControllerCTS?.Cancel();
        DestroySeekBar();
        SetIsFixed(false);
    }
}
