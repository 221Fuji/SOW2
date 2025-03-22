using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using NUnit.Framework;
using System.Collections.Generic;

public class Succubus : CharacterActions
{
    [Header("通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveInfo;
    [SerializeField] private HitBoxManager _normalMoveHitBox;
    [Header("後ろ通常攻撃")]
    [SerializeField] private AttackInfo _normalMoveBehindInfo;
    [SerializeField] private GameObject _portalPrefab;
    [SerializeField] private GameObject _chainPrefab;
    private HitBoxManager _normalMoveBehindHitBox;
    [Header("ジャンプ攻撃")]
    [SerializeField] private AttackInfo _jumpMoveInfo;
    [SerializeField] private HitBoxManager _jumpMoveHitBox;
    [SerializeField] private Vector2 _jmDirection;
    [Header("必殺技１")]
    [SerializeField] private AttackInfo _specialMove1Info;
    [SerializeField] private Bullet _sm1BulletPrefab;
    [SerializeField] private float _sm1BulletVelocity;
    [Header("必殺技2")]
    [SerializeField] private AttackInfo _specialMove2Info;
    [SerializeField] private HitBoxManager _specialMove2HitBox;
    [SerializeField] private AttackInfo _sm2DerivationInfo;
    [SerializeField] private HitBoxManager _sm2DerivationHitBox;
    [Header("超必殺技")]
    [SerializeField] private AttackInfo _ultimateInfo;
    [SerializeField] private Bullet _ultBulletPrefab;
    [SerializeField] private Vector2 _ultBulletVelocity;

    private int _jumpMoveCount = 0; //１回のジャンプで行ったジャンプ攻撃の回数

    //後ろ通常攻撃の変数
    private GameObject _startPortal;
    private GameObject _endPortal;
    private GameObject _chain;

    //必殺技１の変数
    private List<Bullet> _sm1Bullet = new List<Bullet>();

    //超必殺技の変数
    private List<Bullet> _ultBullet = new List<Bullet>();

    //各行動のCancellationTokenSource(CTS)
    private CancellationTokenSource _normalMoveCTS;
    private CancellationTokenSource _normalMoveBehindCTS;
    private CancellationTokenSource _pullChainCTS;
    private CancellationTokenSource _destroyPortalCTS;
    private CancellationTokenSource _jumpMoveCTS;
    private CancellationTokenSource _specialMove1CTS;
    private CancellationTokenSource _specialMove2CTS;
    private CancellationTokenSource _sm2DerivationCTS;
    private CancellationTokenSource _ultCTS;

    //行動制限の設定
    protected override bool CanEveryAction
    {
        get
        {
            return base.CanEveryAction
                && _normalMoveCTS == null
                && _normalMoveBehindCTS == null
                && _pullChainCTS == null
                && _jumpMoveCTS == null
                && _specialMove1CTS == null
                && _specialMove2CTS == null
                && _sm2DerivationCTS == null
                && _ultCTS == null;
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
        _specialMove2HitBox.InitializeHitBox(_specialMove2Info, gameObject);
        _sm2DerivationHitBox.InitializeHitBox(_sm2DerivationInfo, gameObject);
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

        //後ろ入力であれば
        if(WalkDirection() < 0)
        {
            NormalMoveBehind().Forget();
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

            //物理挙動
            float jmDirectionX = _jmDirection.x * (_characterState.IsLeftSide ? 1 : -1);
            Velocity = Vector2.zero;

            AddForce(new Vector2(jmDirectionX, _jmDirection.y));

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
    /// 後ろ通常攻撃
    /// </summary>
    public async UniTask NormalMoveBehind()
    {
        // 新しいCTSを生成
        _normalMoveBehindCTS = new CancellationTokenSource();
        CancellationToken token = _normalMoveBehindCTS.Token;

        // アニメーション処理
        _animator.ResetTrigger("ChainCancelTrigger");
        _animator.ResetTrigger("PullTrigger");
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("NormalMoveBehindTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_normalMoveBehindInfo.ConsumptionSP);

        //UP回収
        UPgain(_normalMoveBehindInfo.MeterGain);

        try
        {
            await StartUpMove(_normalMoveBehindInfo.StartupFrame, token); // 発生を待つ
            await CreatePortal(token); //ポータルの生成
            await RecoveryFrame(_normalMoveBehindInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            _normalMoveBehindHitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _normalMoveBehindCTS.Dispose();
            _normalMoveBehindCTS = null;

            //layerを元に戻す
            if (_pullChainCTS == null)
            {
                AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
                _animator.SetTrigger("ChainCancelTrigger");
                DestoryPortal();
            }
        }
    }

    public async UniTask CreatePortal(CancellationToken token)
    {
        //ポータルの生成
        Vector2 startPortalPos = new Vector2(transform.position.x, -4);
        Vector2 endPortalPos;
        if (_characterState.IsLeftSide)
        {
            startPortalPos -= new Vector2(5, 0);
            _startPortal = Instantiate(_portalPrefab, startPortalPos, Quaternion.identity);

            endPortalPos = new Vector2(StageParameter.CurrentRightWallPosX - 1, -4);
            _endPortal = Instantiate(_portalPrefab, endPortalPos, new Quaternion(0, 180, 0, 0));
        }
        else
        {
            startPortalPos += new Vector2(5f, 0);
            _startPortal = Instantiate(_portalPrefab, startPortalPos, new Quaternion(0, 180, 0, 0));

            endPortalPos = new Vector2(StageParameter.CurrentLeftWallPosX + 1, -4);
            _endPortal = Instantiate(_portalPrefab, endPortalPos, Quaternion.identity);
        }

        await FightingPhysics.DelayFrameWithTimeScale(5, token);
        await CreateChain(endPortalPos, token);
    }

    public async UniTask CreateChain(Vector2 endPortalPos, CancellationToken token)
    {
        Vector2 chainPos;
        Vector2 chainPosOffset = new Vector2(0, 0);
        if(_characterState.IsLeftSide)
        {
            chainPos = endPortalPos - chainPosOffset;
            _chain = Instantiate(_chainPrefab, chainPos, new Quaternion(0, 180, 0, 0));
        }
        else
        {
            chainPos = endPortalPos + chainPosOffset;
            _chain = Instantiate(_chainPrefab, chainPos, Quaternion.identity);
        }

        _normalMoveBehindHitBox = _chain.GetComponentInChildren<HitBoxManager>();
        _normalMoveBehindHitBox.InitializeHitBox(_normalMoveBehindInfo, gameObject);
        _normalMoveBehindHitBox.Hit = HitChain;

        await WaitForActiveFrame(_normalMoveBehindHitBox, _normalMoveBehindInfo.ActiveFrame, token);
    }

    /// <summary>
    /// 後ろ通常攻撃が当たると呼ばれる
    /// </summary>
    public void HitChain()
    {
        _pullChainCTS = new CancellationTokenSource();
        CancellationToken token = _pullChainCTS.Token;
        PullChain(token).Forget();

        _normalMoveBehindCTS.Cancel();
    }

    public async UniTask PullChain(CancellationToken token)
    {
        //バインド付与
        _enemyCA.GetComponent<CharacterState>().AnormalyStates.Add(AnormalyState.Bind);

        //アニメーション処理
        Animator chainAnimator = _chain.GetComponent<Animator>();
        float pulltime = 0;
        if(!chainAnimator.GetCurrentAnimatorStateInfo(0).IsName("PullChain"))
        {
            pulltime = 1 - AnimatorByLayerName.GetCurrentAnimationProgress(chainAnimator, "Base Layer");
        }
        AnimatorByLayerName.PlayAnimationOnLayer(chainAnimator, "PullChain", "Base Layer", pulltime);
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 1);
        _animator.SetTrigger("PullTrigger");

        //ポータルまで引っ張る
        bool completePull = false;
        while (!completePull)
        {
            if(_characterState.IsLeftSide)
            {
                completePull = _endPortal?.transform.position.x < _enemyCA.GetPushBackBox().xMax;
            }
            else
            {
                completePull = _endPortal?.transform.position.x > _enemyCA.GetPushBackBox().xMin;
            }

            //敵の座標を鎖の先端に更新
            Vector3 offset = _enemyCA.transform.position - (Vector3)_enemyCA.GetPushBackBox().center;
            _enemyCA.transform.position = _normalMoveBehindHitBox.transform.position + offset;

            await FightingPhysics.DelayFrameWithTimeScale(1, token);

            //キャンセル時の挙動
            if(token.IsCancellationRequested)
            {
                _enemyCA.Velocity = Vector2.zero;
                EndPullChain();
                return;
            }
        }

        FishingEnemy();

        //引っ張り後の硬直
        await RecoveryFrame(25, token);
        EndPullChain();
    }

    /// <summary>
    /// pullChainCTSが終了すると呼ばれる
    /// </summary>
    private void EndPullChain()
    {
        if (_animator == null) return;
        _animator.SetTrigger("ChainCancelTrigger");
        AnimatorByLayerName.SetLayerWeightByName(_animator, "NormalMoveLayer", 0);
        DestoryPortal();
        _pullChainCTS?.Cancel();
        _pullChainCTS?.Dispose();
        _pullChainCTS = null;
    }

    public void FishingEnemy()
    {
        AnimatorByLayerName.PlayAnimationOnLayer(_animator, "Pull", "NormalMoveLayer", 0.5f);

        _enemyCA.GetComponent<CharacterState>().RecoverAnormalyState(AnormalyState.Bind);

        //物理挙動
        _enemyCA.transform.position = _startPortal.transform.position;
        Vector2 fishingPower = new Vector2(5, 25);
        if(!_characterState.IsLeftSide)
        {
            fishingPower *= new Vector2(-1, 1);
        }
        _enemyCA.Velocity = Vector2.zero;
        _enemyCA.AddForce(fishingPower);
    }

    private async void DestoryPortal()
    {
        //鎖の削除
        if (_chain != null)
        {
            Destroy(_chain);
            _chain = null;
        }

        if (_startPortal == null || _endPortal == null) return;

        Animator startAnimator = _startPortal.GetComponent<Animator>();
        Animator endAnimator = _endPortal.GetComponent<Animator>();

        _destroyPortalCTS = new CancellationTokenSource();

        if (startAnimator != null && endAnimator != null)
        {
            //ポータルが一旦開くまで待つ
            await UniTask.WaitUntil(() =>
            {
                if (startAnimator == null) return true;
                var startStateInfo = startAnimator.GetCurrentAnimatorStateInfo(0);
                return !startStateInfo.IsName("Open");
            }, cancellationToken: _destroyPortalCTS.Token);

            startAnimator.SetTrigger("CloseTrigger");
            endAnimator.SetTrigger("CloseTrigger");

            //ポータルが閉じるまで待つ
            await UniTask.WaitUntil(() =>
            {
                if (startAnimator == null) return true;
                var startStateInfo = startAnimator.GetCurrentAnimatorStateInfo(0);
                return startStateInfo.IsName("Close") && startStateInfo.normalizedTime >= 1f;
            }, cancellationToken: _destroyPortalCTS.Token);
        }

        Destroy(_startPortal);
        _startPortal = null;
        Destroy(_endPortal);
        _endPortal = null;
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
            CreateSm1Bullet(token);
            await RecoveryFrame(_specialMove1Info.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException)
        {
            //全ての弾の削除
            for (int i = _sm1Bullet.Count - 1; i >= 0; i--)
            {
                _sm1Bullet[i].HitBox.SetIsActive(false);
                Sm1BulletHit(_sm1Bullet[i]);
            }
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

    private void CreateSm1Bullet(CancellationToken token)
    {
        //弾の座標と速度設定
        Vector2 bulletVelocity = new Vector2(_sm1BulletVelocity, 0);
        Vector2 bulletPosOffset = new Vector2(1, 1);
        if(!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            bulletPosOffset *= new Vector2(-1, 1);
        }
        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet bullet = Instantiate(_sm1BulletPrefab, bulletPos, Quaternion.identity);
        _sm1Bullet.Add(bullet);
        bullet.Velocity = bulletVelocity;

        //アニメーション
        bullet.GetComponent<Animator>().SetTrigger("Sm1BulletTrigger");

        //弾の当たり判定設定
        bullet.HitBox.InitializeHitBox(_specialMove1Info, gameObject);
        bullet.HitBox.HitBullet = Sm1BulletHit;
        bullet.HitBox.GuardBullet = Sm1BulletHit;
        WaitForActiveFrame(bullet.HitBox, 0, token).Forget();
    }

    private async void Sm1BulletHit(Bullet bullet)
    {
        if (!_sm1Bullet.Contains(bullet)) return;

        bullet.Velocity = Vector2.zero;
        bullet.GetComponent<Animator>().SetTrigger("Sm1HitTrigger");

        await FightingPhysics.DelayFrameWithTimeScale(30);

        _sm1Bullet.Remove(bullet);
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
        //疲労状態
        if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)) return;

        //空中不可
        if (!OnGround) return;

        //2段目派生
        if (_specialMove2CTS != null)
        {
            Sm2Derivation().Forget();
            _specialMove2CTS.Cancel();
            return;
        }

        if (!CanEveryAction) return;

        // 新しいCTSを生成
        _specialMove2CTS = new CancellationTokenSource();
        CancellationToken token = _specialMove2CTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("SpecialMove2Trigger");
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
            if(_sm2DerivationCTS == null)
            {
                AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
            }
        }

    }

    private async UniTask Sm2Derivation()
    {
        // 新しいCTSを生成
        _sm2DerivationCTS = new CancellationTokenSource();
        CancellationToken token = _sm2DerivationCTS.Token;

        // アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 1);
        _animator.SetTrigger("Sm2DerivationTrigger");
        _animator.SetFloat("WalkFloat", 0);

        //物理挙動
        Velocity = Vector2.zero;

        //SP消費
        _characterState.SetCurrentSP(-_sm2DerivationInfo.ConsumptionSP);

        //UP回収
        UPgain(_sm2DerivationInfo.MeterGain);

        try
        {
            await StartUpMove(_sm2DerivationInfo.StartupFrame, token); // 発生を待つ
            await WaitForActiveFrame(_sm2DerivationHitBox, _sm2DerivationInfo.ActiveFrame, token); // 持続を待つ
            await RecoveryFrame(_sm2DerivationInfo.RecoveryFrame, token); // 硬直を待つ
        }
        catch (OperationCanceledException) 
        {
            _sm2DerivationHitBox.SetIsActive(false);
        }
        finally
        {
            // 攻撃処理が完了した後、トークンを解放
            _sm2DerivationCTS.Dispose();
            _sm2DerivationCTS = null;
        }

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "SpecialMove2Layer", 0);
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

        //物理挙動
        Velocity = Vector2.zero;

        PerformUltimate?.Invoke(GetPushBackBox().center, 3.5f, 30);

        try
        {
            await CreateUltBullet(token); // 発生保証あり
        }
        catch(OperationCanceledException) 
        {
            //全ての弾の削除
            for (int i = _ultBullet.Count - 1; i >= 0; i--)
            {
                _ultBullet[i].HitBox.SetIsActive(false);
                Sm1BulletHit(_ultBullet[i]);
            }
        }

        await RecoveryFrame(_ultimateInfo.RecoveryFrame, token); // 硬直を待つ

        // 攻撃処理が完了した後、トークンを解放
        _ultCTS.Dispose();
        _ultCTS = null;

        //layerを元に戻す
        AnimatorByLayerName.SetLayerWeightByName(_animator, "UltLayer", 0);
    }

    private async UniTask CreateUltBullet(CancellationToken token)
    {
        //弾の生成
        Vector2 bulletVelocity = _ultBulletVelocity;
        Vector2 bulletPosOffset = new Vector2(0, 1.5f);

        Vector2 bulletPos = (Vector2)transform.position + bulletPosOffset;
        Bullet ultBullet = Instantiate(_ultBulletPrefab, bulletPos, Quaternion.identity);
        _ultBullet.Add(ultBullet);
        if (!_characterState.IsLeftSide)
        {
            bulletVelocity *= new Vector2(-1, 1);
            ultBullet.SetBoundVelocity(ultBullet.BoundVelocity * new Vector2(-1, 1));
        }
        ultBullet.SetIsFixed(true);

        //アニメーション
        ultBullet.GetComponent<Animator>().SetTrigger("UltBulletTrigger");

        // 発生を待つ
        await StartUpMove(_ultimateInfo.StartupFrame, token);

        //速度設定
        ultBullet.SetIsFixed(false);
        ultBullet.Velocity = bulletVelocity;


        //弾の当たり判定設定
        ultBullet.HitBox.InitializeHitBox(_ultimateInfo, gameObject);
        ultBullet.HitBox.HitBullet = UltBulletHit;
        ultBullet.HitBox.GuardBullet = UltBulletHit;
        WaitForActiveFrame(ultBullet.HitBox, 0, token).Forget();
    }

    private async void UltBulletHit(Bullet bullet)
    {
        if (!_ultBullet.Contains(bullet)) return;
        bullet.Velocity = Vector2.zero;
        bullet.SetIsFixed(true);
        bullet.GetComponent<Animator>().SetTrigger("UltHitTrigger");

        await FightingPhysics.DelayFrameWithTimeScale(30);

        _ultBullet.Remove(bullet);

        if(bullet != null)
        {
            Destroy(bullet.gameObject);
        }     
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
        if (activeFrame <= 0) return;
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
        _normalMoveBehindCTS?.Cancel();
        EndPullChain();
        _specialMove1CTS?.Cancel();
        _specialMove2CTS?.Cancel();
        _sm2DerivationCTS?.Cancel();
        _jumpMoveCTS?.Cancel();
    }
}
