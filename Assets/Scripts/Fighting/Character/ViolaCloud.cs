using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class ViolaCloud : CharacterActions
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
    [SerializeField] private Fog _fogPrefab;
    [SerializeField] private Animator _collectEffectAnimator;
    [SerializeField] private float _gainValue;
    private List<Fog> _fogList = new List<Fog>();
    public float FogMaxResource { get; } = 100;
    public float CurrentFogResource { get; private set; }
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private GameObject _sm2BulletPrefab;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultOutSideInfo;
    [SerializeField] private Bullet _ultBullet;
    [SerializeField] private AttackInfo _ultInSideInfo;
    [SerializeField] private float _healValue;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数

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
        CurrentFogResource = FogMaxResource;
    }

    protected override void SetActionDelegate()
    {
        _inputReciever.JumpDelegate = Jump;
        _inputReciever.GuardDelegate = GuardStance;
        _inputReciever.NormalMove = NormalMove;
        _inputReciever.SpecialMove1 = SpecialMove1;
        _inputReciever.SpecialMove2 = SpecialMove2;
        _inputReciever.Ultimate = UltimateOutSide;
    }

    protected override void SetHitBox()
    {
        _normalMoveHitBox.InitializeHitBox(_normalMoveInfo, gameObject);
        _jumpMoveHitBox.InitializeHitBox(_jumpMoveInfo, gameObject);
    }

    protected override void FightingUpdate()
    {
        base.FightingUpdate();

        //雲刃がないときゲージ回復
        if(_fogList.Count == 0)
        {
            if(CurrentFogResource < 100)
            {
                CurrentFogResource += _gainValue;
            }
            else
            {
                CurrentFogResource = 100;
            }         
        }
        else
        {
            //ゲージ枯渇時
            if(CurrentFogResource <= 0)
            {
                DestoryAllFogInList(_fogList);
            }
        }
    }

    /// <summary>
    /// 中に自身がいる煙霧のリスト
    /// </summary>
    private List<Fog> FogInSelfList()
    {
        List<Fog> fogInSelfList = new List<Fog>();

        if (_fogList.Count > 0)
        {
            foreach (var fog in _fogList)
            {
                if (fog.IsInSelf())
                {
                    fogInSelfList.Add(fog);
                }
            }
        }
        return fogInSelfList;
    }

    /// <summary>
    /// FogのListの要素を全て破壊する
    /// </summary>
    private void DestoryAllFogInList(List<Fog> fogList)
    {
        var fogsToDestroy = new List<Fog>(fogList);

        foreach (var fog in fogsToDestroy)
        {
            fog.DestroyFog().Forget();
            _fogList.Remove(fog);
            fogList.Remove(fog);
        }
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

        List<Fog> fogIsSelfList = FogInSelfList();

        if (fogIsSelfList.Count > 0)
        {
            EnhancedNormalMove(fogIsSelfList).Forget();
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
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveInfo.StartupFrame, token); // 発生を待つ
            float speed = 14;
            if (!_characterState.IsLeftSide)
            {
                speed *= -1;
            }
            Velocity = new Vector2(speed, Velocity.y);
            await WaitForActiveFrame(_normalMoveHitBox, _normalMoveInfo.ActiveFrame, token); // 持続を待つ
            Velocity = Vector2.zero;
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
    /// 強化通常攻撃
    /// </summary>
    public async UniTask EnhancedNormalMove(List<Fog> fogList)
    {
        // 新しいCTSを生成
        _normalMoveCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveCTS.Token;

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("EnhancedNormalMoveTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveInfo.StartupFrame, token); // 発生を待つ
            float speed = 18;
            if (!_characterState.IsLeftSide)
            {
                speed *= -1;
            }
            DestoryAllFogInList(fogList);
            _collectEffectAnimator.SetTrigger("CollectFogTrigger");
            Velocity = new Vector2(speed, Velocity.y);
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
        if (!CanEveryAction) return;

        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //空中不可
        if (!OnGround) return;

        if (CurrentFogResource < 25) return;

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
            Fog fog = Instantiate(_fogPrefab);
            _fogList.Add(fog);
            fog.transform.position = transform.position;
            fog.InitializeFog(_characterState.IsLeftSide, gameObject);
            fog.SetIsActive(true);
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
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove1Layer", 0);
    }

    /// <summary>
    /// Fogクラスから呼ばれる
    /// </summary>
    public void ConsumptionFogResource(float value) 
    {
        CurrentFogResource -= value;
        if(CurrentFogResource < 0)
        {
            CurrentFogResource = 0;
        }
    }

    /// <summary>
    /// 必殺技２
    /// </summary>
    public async UniTask SpecialMove2()
    {
        if (!CanEveryAction) return;

        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //空中不可
        if (!OnGround) return;

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
            CreateSm2Bullet(token); //弾の生成
            await RecoveryFrame(_specialMove2Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            //弾は発生保証なし
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
        Vector2 bulletPosOffset = new Vector2(1.5f, 1);
        if (!_characterState.IsLeftSide)
        {
            bulletPosOffset *= new Vector2(-1.5f, 1);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        GameObject bullet = Instantiate(_sm2BulletPrefab, bulletPos, Quaternion.identity);

        //煙霧内かどうか
        List<Fog> fogIsSelfList = FogInSelfList();
        if (fogIsSelfList.Count > 0)
        {
            DestoryAllFogInList(fogIsSelfList);
            _collectEffectAnimator.SetTrigger("CollectFogTrigger");

            //アニメーション
            bullet.GetComponent<Animator>().SetTrigger("InSideTrigger");
        }
        else
        {
            //アニメーション
            bullet.GetComponent<Animator>().SetTrigger("OutSideTrigger");
        }

        //弾の当たり判定設定
        HitBoxManager hbm = bullet.GetComponentInChildren<HitBoxManager>();
        hbm?.InitializeHitBox(_specialMove2Info, gameObject);

        await WaitForActiveFrame(hbm, _specialMove2Info.ActiveFrame, token);

        await FightingPhysics.DelayFrameWithTimeScale(60);
        if(bullet != null)
        {
            Destroy(bullet);
        }
    }

    /// <summary>
    /// 超必殺技
    /// </summary>
    public async UniTask UltimateOutSide()
    {
        List<Fog> fogList = FogInSelfList();
        if(fogList.Count > 0)
        {
            UltimateInSide();
            return;
        }

        if (!CanEveryAction || _characterState.CurrentUP < 100) return;

        if(!OnGround) return;

        //UP消費
        _characterState.SetCurrentUP(-100);

        // 新しいCTSを生成
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltOutSideTrigger");

        //物理挙動
        Velocity = Vector2.zero;

        //演出
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);

        try
        {
            await StartUpMove(_ultOutSideInfo.StartupFrame, token); // 発生を待つ

            for(int i = 0; i < 3 ;i++)
            {
                CreateUltBullet();
                await FightingPhysics.DelayFrameWithTimeScale(15, token);
            }

            await RecoveryFrame(_ultOutSideInfo.RecoveryFrame, token); // 硬直を待つ
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
        Quaternion bulletQuaternion = new Quaternion(0, 0, 0, 0);

        //入力方向によって設定
        if (!_characterState.IsLeftSide)
        {
            generatePosOffsetX *= -1;
            bulletQuaternion = new Quaternion(0, 180, 0, 0);
        }
        float generatePosX = transform.position.x + generatePosOffsetX;
        Vector2 generatePos = new Vector2(generatePosX, transform.position.y);

        //UltBulletの生成
        Bullet bullet = Instantiate(_ultBullet, generatePos, bulletQuaternion);
        bullet.HitBox.InitializeHitBox(_ultOutSideInfo, gameObject);
        bullet.DestroyBullet = DestroyUltBullet;

        bullet.Velocity = new Vector2(generatePosOffsetX * 4f, 0);
    }

    private void DestroyUltBullet(Bullet bullet)
    {
        if(bullet != null)
        {
            Destroy(bullet);
        }
    }


    private async void UltimateInSide()
    {
        if (!CanEveryAction || _characterState.CurrentUP < 100) return;

        if (!OnGround) return;

        //UP消費
        _characterState.SetCurrentUP(-100);

        // 新しいCTSを生成
        _ultCTS = new CancellationTokenSource();
        CancellationToken token = _ultCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 1);
        _animator.SetTrigger("UltInSideTrigger");

        //演出
        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);

        try
        {
            await StartUpMove(_ultInSideInfo.StartupFrame, token); // 発生を待つ
                                                                   //物理挙動
            AddForce(new Vector2(0, 20));
            DestoryAllFogInList(_fogList);
            CurrentFogResource = 100;
            _characterState.TakeDamage(-_healValue);

            await WaitForActiveFrame(null, _ultInSideInfo.ActiveFrame, token);

            //物理挙動
            Velocity = Vector2.zero;
            SetIsFixed(true);

            await RecoveryFrame(_ultInSideInfo.RecoveryFrame, token); // 硬直を待つ
        }
        finally
        {
            //物理挙動
            Velocity = Vector2.zero;
            SetIsFixed(false);

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

    private async UniTask StartUpMove(int startUpFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(startUpFrame, cancellationToken: token);
    }

    private async UniTask WaitForActiveFrame(HitBoxManager hitBox, int activeFrame, CancellationToken token)
    {
        hitBox?.SetIsActive(true);
        await FightingPhysics.DelayFrameWithTimeScale(activeFrame, cancellationToken: token);
        hitBox?.SetIsActive(false);
    }

    private async UniTask RecoveryFrame(int recoveryFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(recoveryFrame, cancellationToken: token);
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
