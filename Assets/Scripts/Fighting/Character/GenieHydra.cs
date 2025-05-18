using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GenieHydra : CharacterActions
{
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private HitBoxManager _normalMoveHitBox;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Vector2 _sm1Direction;
    [SerializeField] private HitBoxManager _specialMove1HitBox;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private HitBoxManager _specialMove2HitBox;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private AttackInfo _ultBulletInfo;
    [SerializeField] private GameObject _ultBullet;
    private HitBoxManager _ultHitBox;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    private int _sm1Count = 0; //sm1の回数

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
            if (!OnGround)
            {
                if (_sm1Count != 0) return false; //空中では１回のみ
            }

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
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, gameObject);
        _specialMove1HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        _specialMove1HitBox.Hit = HitSp1Move;
        _specialMove2HitBox.InitializeHitBox(_specialMove2Info, gameObject);
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
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_jumpMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_jumpMoveInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_jumpMoveHitBox, _normalMoveInfo.ActiveFrame, token); // 持続を待つ
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

        //空中では１回のみ
        if (!OnGround)
        {
            _sm1Count++;
        }

        // 新しいCTSを生成
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // 発生を待つ

            //物理挙動
            float sm1DirectionX = _sm1Direction.x * (_characterState.IsLeftSide ? 1 : -1);
            Velocity = Vector2.zero;

            AddForce(new Vector2(sm1DirectionX, _sm1Direction.y));

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
            _specialMove1CTS.Dispose();
            _specialMove1CTS = null;
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
    }

    //Sp1が当たったときに呼ばれる
    private void HitSp1Move()
    {
        AddForce(new Vector2(0, -10));
        Debug.Log("加速落下");
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
            await WaitForActiveFrame(_specialMove2HitBox, _specialMove2Info.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _specialMove2HitBox.SetIsActive(false);
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
        SetIsFixed(true);

        //演出
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);
        _characterState.SetIsUltPerformance();

        try
        {
            await StartUpMove(_ultimateInfo.StartupFrame, token); // 発生を待つ
            CreateUltBullet();
            await RecoveryFrame(_ultimateInfo.RecoveryFrame, token); // 硬直を待つ
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _ultCTS.Dispose();
            _ultCTS = null;

            //物理挙動
            SetIsFixed(false);
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    private void CreateUltBullet()
    {
        //UltBulletの生成座標
        float generatePosOffsetX = 2;
        float generatePosOffsetY = 1;
        //ヒットバック等のベクトル修正
        AttackInfo attackInfo = _ultBulletInfo;

        //入力方向によって設定
        if (WalkDirection() < 0)
        {
            generatePosOffsetX *= -1;
        }
        else
        {
            attackInfo.HitBackDirection *= new Vector2(-1, 1);
            attackInfo.GuardBackDirection *= new Vector2(-1, 1);
        }
        if (transform.position.x > EnemyCA.transform.position.x)
        {
            generatePosOffsetX *= -1;
        }
        float generatePosX = EnemyCA.transform.position.x + generatePosOffsetX;
        float generatePosY = EnemyCA.transform.position.y + generatePosOffsetY;
        Vector2 generatePos = new Vector2(generatePosX, generatePosY);

        //UltBulletの向き
        Quaternion bulletQuaternion
            = new Quaternion(0, generatePosOffsetX > 0 ? 180 : 0, 0, 0);

        //UltBulletの生成
        GameObject bullet = Instantiate(_ultBullet, generatePos, bulletQuaternion);
        _ultHitBox = bullet.GetComponentInChildren<HitBoxManager>();
        _ultHitBox?.InitializeHitBox(attackInfo, gameObject);

        // 新しいCTSを生成
        _ultBulletCTS = new CancellationTokenSource();
        CancellationToken token = _ultBulletCTS.Token;

        UltBullet(bullet, token).Forget();
    }

    private async UniTask UltBullet(GameObject bullet, CancellationToken token)
    {
        try
        {
            await StartUpMove(_ultBulletInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_ultHitBox, _ultBulletInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_ultBulletInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _ultHitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _ultBulletCTS.Dispose();
            _ultBulletCTS = null;

            //UltBullet削除
            if (bullet != null)
            {
                Destroy(bullet);
                _ultHitBox = null;
            }
        }
    }

    //着地時にジャンプ攻撃をキャンセル
    protected override void Land()
    {
        _jumpMoveCTS?.Cancel();
        _jumpMoveCount = 0;
        _sm1Count = 0;
    }

    public override void CancelActionByHit()
    {
        _normalMoveCTS?.Cancel();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _jumpMoveCTS?.Cancel();
        //Ult発動中の固定化解除
        SetIsFixed(false);
    }
}
