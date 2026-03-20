using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

public class Imeru : CharacterActions
{
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private HitBoxManager _normalMoveHitBox;
    [SerializeField] private AttackInfo _normalMoveBulletInfo;
    [SerializeField] private Bullet _nmBulletPrefab;
    [SerializeField] private float _nmBulletVelocity;
    [SerializeField] private Bullet _targetPrefab;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private HitBoxManager _specialMove1HitBox;
    [SerializeField] private AttackInfo _sm1BulletInfo;
    [SerializeField] private Bullet _sm1BulletPrefab;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private HitBoxManager _specialMove2HitBox;
    [SerializeField] private List<Vector2> _direction8;
    [SerializeField] private float _sm2Power;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private AttackInfo _ultBulletInfo;
    [SerializeField] private Bullet _ultBulletPrefab;
    [SerializeField] private int _ult1MPerformanceFrame;
    [Header("回避")]
    [SerializeField] private AttackInfo _dodgeInfo;
    [SerializeField] private float _dodgePower;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数
    private float _idlingSpeed = 17.5f;

    private Bullet _target;

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _ultCTS;
    private CancellationTokenSource _ultBulletCTS;
    private CancellationTokenSource _dodgeCTS;

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
                && _dodgeCTS == null;
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
            if (_target == null) return false;
            if (_target.transform.position.x < StageParameter.CurrentLeftWallPosX) return false;
            if (_target.transform.position.x > StageParameter.CurrentRightWallPosX) return false;

            return true;
        }
    }
    public bool CanSpecialMove2
    {
        get
        {
            if (!CanEveryAction) return false;
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return false;

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
        //Velocity = Velocity / 1.5f;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveInfo.MeterGain);

        //斬撃飛ばし
        CreateNmBullet(token);

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


    private async void CreateNmBullet(CancellationToken token)
    {
        try
        {
            await StartUpMove(_normalMoveBulletInfo.StartupFrame, token); // 発生を待つ
        }
        catch { return; }   

        //弾の座標と速度設定
        Vector2 bulletVelocity = new Vector2(_nmBulletVelocity, 0);
        Vector2 bulletPosOffset = new Vector2(2, 0);
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
        bullet.HitBox.InitializeHitBox(_normalMoveBulletInfo, gameObject);
        bullet.DestroyBullet = DestroyNmBullet;

        try
        {
            await WaitForActiveFrame(bullet.HitBox, _normalMoveBulletInfo.ActiveFrame, token);
        }
        finally
        {
            DestroyNmBullet(bullet);
        }
    }

    private async void DestroyNmBullet(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.Velocity = Vector2.zero;
        bullet.GetComponent<Animator>().SetTrigger("NmHitTrigger");

        CreateTarget(bullet.transform.position);

        await FrameManager.DeleyFightingFrame(30);

        if (bullet != null)
        {
            Destroy(bullet.gameObject);
        }
    }

    /// <summary>
    /// ターゲットの生成
    /// </summary>
    private void CreateTarget(Vector2 targetPos)
    {
        if (_target != null) DestroyTarget(_target);

        _target = Instantiate(_targetPrefab);
        _target.GetComponent<Bullet>().DestroyBullet = DestroyTarget;
        _target.transform.position = targetPos;
        Quaternion rotation = Quaternion.identity;
        if(!_characterState.IsLeftSide)
        {
            rotation = new Quaternion(0, 180, 0, 0);
        }
        _target.transform.rotation = rotation;
    }

    /// <summary>
    /// ターゲットの削除
    /// </summary>
    private async void DestroyTarget(Bullet target)
    {
        if (_target == null) return;
        target.GetComponent<Animator>().SetTrigger("TargetVanishTrigger");

        await FrameManager.DeleyFightingFrame(30);

        if (target != null)
        {
            Destroy(target.gameObject);
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
        if (!CanSpecialMove1) return;

        // 新しいCTSを生成
        _specialMove1CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove1CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 1);
        _animator.SetTrigger("SpecialMove1Trigger");

        //物理挙動
        //Velocity = Vector2.zero;

        //方向転換
        if(_target.transform.position.x >= GetPushBackBox().center.x && !_characterState.IsLeftSide)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        if(_target.transform.position.x <= GetPushBackBox().center.x && _characterState.IsLeftSide)
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
        }

        //SP消費
        _characterState.SetCurrentSP(-_specialMove1Info.ConsumptionSP);

        //UP回収
        UPgain(_specialMove1Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove1Info.StartupFrame, token); // 発生を待つ
            Vector2 prePos = GetPushBackBox().center;
            Warp(_target.transform.position);
            CreateSm1Bullet(prePos, GetPushBackBox().center, token);
            DestroyTarget(_target);
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

    /// <summary>
    /// 瞬間移動
    /// </summary>
    private void Warp(Vector2 targetPos)
    {
        Vector2 resultPos = new Vector2(targetPos.x, transform.position.y);
        Vector2 idlingSpeed = new Vector2(_idlingSpeed, 0);
        if(transform.position.x >= targetPos.x)
        {
            idlingSpeed *= Vector2.left;
        }
        transform.position = resultPos;
        Velocity = idlingSpeed;
    }

    private async void CreateSm1Bullet(Vector2 prePos, Vector2 currentPos, CancellationToken token)
    {
        //弾の座標と速度設定
        float bulletPosX = (prePos.x + currentPos.x) / 2;
        float bulletPosY = (_sm1BulletPrefab.transform.localScale.y / 2) + StageParameter.GroundPosY;
        Vector2 bulletPos = new Vector2(bulletPosX, bulletPosY);
        Bullet bullet = Instantiate(_sm1BulletPrefab, bulletPos, Quaternion.identity);
        bullet.transform.localScale = new Vector2(Mathf.Abs(prePos.x - currentPos.x), bullet.transform.localScale.y);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_sm1BulletInfo, gameObject);
        bullet.DestroyBullet = DestroySm1Bullet;

        //持続
        try
        {
            await WaitForActiveFrame(bullet.HitBox, _sm1BulletInfo.ActiveFrame, token);
        }
        finally
        {
            DestroySm1Bullet(bullet);
        }
    }

    private void DestroySm1Bullet(Bullet bullet)
    {
        if (bullet != null)
        {
            Destroy(bullet.gameObject);
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

        // アニメーション処理
        _animator.SetInteger("Sm2Int", 10);
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        

        //物理挙動
        Velocity = Vector2.zero;
        SetGravityScale(0);

        //SP消費
        _characterState.SetCurrentSP(-_specialMove2Info.ConsumptionSP);

        //UP回収
        UPgain(_specialMove2Info.MeterGain);

        try
        {
            await StartUpMove(_specialMove2Info.StartupFrame, token); // 発生を待つ

            //物理挙動
            Vector2 sm2Direction = GetSm2Direction(_inputReciever.InputDirection).normalized * _sm2Power;
            Velocity = sm2Direction;

            //アニメーション
            _animator.SetInteger("Sm2Int", GetSm2AnimationNum(sm2Direction, out bool isReverse));
            _animator.SetTrigger("SpecialMove2Trigger");

            await WaitForActiveFrame(_specialMove2HitBox, _specialMove2Info.ActiveFrame, token); // 持続を待つ
            SetGravityScale(1);
            Velocity = Velocity / 2;


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
            SetGravityScale(1);
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
    }

    /// <summary>
    /// 入力されている方向に一番近い8方向を返す
    /// </summary>
    /// <param name="inputDirection"></param>
    /// <returns></returns>
    private Vector2 GetSm2Direction(Vector2 inputDirection)
    {
        //方向が入力されていないとき前に進む
        if(inputDirection == Vector2.zero)
        {
            if (_characterState.IsLeftSide)
            {
                return _direction8[0];
            }
            else
            {
                return _direction8[4];
            }
        }

        //入力に一番近い8方向を探す
        Vector2 result = _direction8[0];
        float minAngle = 180;
        foreach (Vector2 direction in _direction8) 
        {
            float angle = Vector2.Angle(inputDirection, direction.normalized);
            if(minAngle > angle)
            {
                result = direction;
                minAngle = angle;
            }
        }

        return result;
    }

    private int GetSm2AnimationNum(Vector2 direction, out bool isReverse)
    {
        int directionNum = 0;

        //アニメーションを反転させるかどうか
        isReverse = false;
        if (_characterState.IsLeftSide)
        {
            if (direction.x < 0)
            {
                isReverse = true;
                transform.rotation = new Quaternion(0, 180, 0, 0);
            }
            else
            {
                transform.rotation = new Quaternion(0, 0, 0, 0);
            }
        }
        else
        {
            if (direction.x > 0)
            {
                isReverse = true;
                transform.rotation = new Quaternion(0, 0, 0, 0);
            }
            else
            {
                transform.rotation = new Quaternion(0, 180, 0, 0);
            }
        }

        //アニメーション番号を設定
        if(direction.y != 0)
        {
            if (direction.x != 0)
            {
                directionNum = 1;
            }
            else
            {
                directionNum = 2;
            }
        }
        if (direction.y < 0)
        {
            directionNum *= -1;
        }

        return directionNum;
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
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, _ult1MPerformanceFrame);
        _characterState.SetIsUltPerformance();
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        try
        {
            //演出解除
            await FrameManager.DeleyFightingFrame(1, token);
            _animator.updateMode = AnimatorUpdateMode.Normal;
            await StartUpMove(_ultimateInfo.StartupFrame, token); // 発生を待つ
            CreateUltBullet();
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

    private async void CreateUltBullet()
    {
        //弾の座標と速度設定
        Vector2 bulletPos = new Vector2(EnemyCA.GetPushBackBox().center.x, StageParameter.GroundPosY);
        Quaternion rotation = Quaternion.Euler(0, 0, 90);
        if (!_characterState.IsLeftSide)
        {
            rotation *= Quaternion.Euler(180, 0, 0);
        }
        Bullet bullet = Instantiate(_ultBulletPrefab, bulletPos, rotation);

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_ultBulletInfo, gameObject);
        bullet.HitBox.SetIsActive(true);

        try
        {
            await FrameManager.DeleyFightingFrame(_ultBulletInfo.ActiveFrame);
        }
        finally
        {
            if (bullet)
            {
                Destroy(bullet.gameObject);
            }
        }
    }

    //回避
    protected override async void GuardStance(bool isGuarding)
    {
        if (!CanGuard || !isGuarding) return;

        // 新しいCTSを生成
        _dodgeCTS = new CancellationTokenSource();
        CancellationToken token = _dodgeCTS.Token;

        //SP消費
        _characterState.SetCurrentSP(-_dodgeInfo.ConsumptionSP);

        //物理挙動
        Velocity = Vector2.zero;

        //アニメーション
        AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 1);
        _animator.SetTrigger("DodgeTrigger");

        try
        {
            await FrameManager.DeleyFightingFrame(_dodgeInfo.StartupFrame, token);
            float dodgeDirection = _dodgePower;
            if(_characterState.IsLeftSide)
            {
                dodgeDirection *= -1;
            }
            AddForce(new Vector2(dodgeDirection, 0));
            await FrameManager.DeleyFightingFrame(_dodgeInfo.RecoveryFrame, token);
        }
        finally
        {
            _dodgeCTS.Dispose();
            _dodgeCTS=null;
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 0);
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
        _ultCTS?.Cancel();
        _ultBulletCTS?.Cancel();
        _dodgeCTS?.Cancel();
        //Ult発動中の固定化解除
        SetIsFixed(false);
    }
}
