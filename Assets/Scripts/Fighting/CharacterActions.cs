using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using System.Linq;


public abstract class CharacterActions : FightingRigidBody
{
    [Space]
    [Header("キャラクターの設定")]
    [SerializeField] private GameObject _hurtBox;

    //基本情報
    private readonly float _spGainSpeed = 0.2f; // SPの自然回復量

    //コンポーネント
    protected FightingInputReceiver _inputReciever { get; private set; }
    protected CharacterState _characterState { get; private set; }
    protected Animator _animator { get; private set; }

    //その他プロパティ
    public CharacterActions EnemyCA { get; private set; }
    public int PlayerNum { get; private set; } = 0;

    //硬直等での行動制限プロパティ
    protected virtual bool CanEveryAction
    {
        get
        {
            return _characterState.AcceptOperations
                && !_characterState.IsRecoveringHit
                && !_characterState.IsRecoveringGuard
                && _IsCompleteLandStun
                && !_characterState.AnormalyStates.Contains(AnormalyState.Bind)
                && !_characterState.AnormalyStates.Contains(AnormalyState.Dead);
        }
    }
    protected virtual bool CanWalk
    {
        get
        {
            return CanEveryAction
                && !_characterState.IsGuarding;
        }
    }
    protected virtual bool CanJump
    {
        get
        {
            return CanEveryAction
                && !_characterState.IsGuarding;
        }
    }
    protected virtual bool CanHit
    {
        get
        {
            return !_characterState.AnormalyStates.Contains(AnormalyState.Dead)
                && _characterState.AcceptOperations;
        }
    }
    protected virtual bool CanGuard
    {
        get
        {
            return CanEveryAction
                && !_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)
                && OnGround;
        }
    }

    //ジャンプ硬直
    private bool _IsCompleteLandStun = true;

    //UI関連デリゲート
    public delegate UniTask ComboCountDelegate(int playerNum, int comboNum);
    public ComboCountDelegate ComboCount { get; set; }

    //エフェクト関連デリゲート
    public UnityAction<Vector2, FightingEffect> OnEffect { get; set; }

    //超必殺技演出
    public UnityAction<Vector2, float, int> PerformUltimate { get; set; }

    //死亡
    public UnityAction<int> OnDie { get; set; }

    //AI学習用デリゲート
    public UnityAction OnHurtAI { get; set; }
    public UnityAction OnDieAI { get; set; }
    public UnityAction OnGuardAI { get; set; } 
    public UnityAction OnBreakAI { get; set; }
    public UnityAction OnComboAI { get; set; }


    protected override void Awake()
    {
        tag = "Character";

        //csの初期化
        if (_characterState == null) _characterState = GetComponent<CharacterState>();
        _characterState.Break = Break;
        _characterState.RecoverBreak = RecoverBreak;

        if (_animator == null) _animator = GetComponent<Animator>();

        //入力デリゲートの設定
        if (_inputReciever = GetComponent<FightingInputReceiver>())
        {
            SetActionDelegate();
        }

        //当たり判定の設定
        SetHitBox();

        base.Awake();
    }

    /// <summary>
    /// InputReceiverのデリゲートを初期化する
    /// </summary>
    protected abstract void SetActionDelegate();

    /// <summary>
    /// 各攻撃等の当たり判定の初期化
    /// </summary>
    protected abstract void SetHitBox();

    /// <summary>
    /// 状態の初期化
    /// </summary>
    public virtual void InitializeCA(int playerNum, CharacterActions enemyCA)
    {
        PlayerNum = playerNum;
        EnemyCA = enemyCA;
        Debug.Log($"プレイヤー番号{PlayerNum}");

        Velocity = Vector2.zero;

        //パラメータ初期化
        if (_characterState == null) _characterState = GetComponent<CharacterState>();
        _characterState.ResetState();

        //色を元に戻す
        GetComponent<SpriteRenderer>().color = Color.white;

        //向かい合わせ
        if(playerNum == 1)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            _characterState.SetIsLeft(true);
        }
        else
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
            _characterState.SetIsLeft(false);
        }

        //全てのCTSをキャンセル
        _characterState.CancelGuardStun();
        _characterState.CancelHitStun();
        CancelActionByHit();

        //Animator初期化
        if (_animator == null) _animator = GetComponent<Animator>();
        RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
        _animator.runtimeAnimatorController = null;
        _animator.runtimeAnimatorController = controller;
    }

    protected override void FightingUpdate()
    {
        if (_characterState == null) return;

        //ガード中はSP回復なし
        if (!_characterState.IsGuarding)
        {
            float spGainValue = _spGainSpeed;
            //Break状態は二倍の速度で回復する
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue))
            {
                spGainValue *= 2;
            }
            _characterState.SetCurrentSP(spGainValue);
        }

        //UPゲージ自然増加
        UPgain(_characterState.UPgainSpeed);

        //Animatorパラメータを設定
        _animator.SetFloat("YspeedFloat", Velocity.y);

        //歩き
        Walk(_inputReciever.WalkValue);

        //空中にいるときアニメーションを切り替える
        ChangeAnimatorLayer();

        //ガード入力されているならガード
        GuardStance(_inputReciever.IsInputingGuard);

        //スタミナアウト時ガードが解ける
        if(_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)
            && _characterState.IsGuarding)
        {
            GuardRelease();
        }

        //ガード状態で空中にいるときはガードを解く
        if(!OnGround && _characterState.IsGuarding)
        {
            GuardRelease();
        }

        if (EnemyCA != null)
        {
            //キャラを向かい合わせる
            DirectionReversal();
        }
    }

    /// <summary>
    /// キャラ同士を向かい合わせる
    /// </summary>
    private void DirectionReversal()
    {
        // ガード中は振り向ける
        if(!_characterState.IsGuarding)
        {
            //接地中のみ反転
            if (!CanWalk || !OnGround) return;
        }

        if (GetPushBackBox().center.x > EnemyCA.GetPushBackBox().center.x)
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
            _characterState.SetIsLeft(false);
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            _characterState.SetIsLeft(true);
        }
    }

    /// <summary>
    /// AnmatorのLayerを切り替える
    /// </summary>
    private void ChangeAnimatorLayer()
    {
        // 空中ならAirLayerに切り替える
        if(!OnGround)
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "WalkLayer", 0);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "AirLayer", 1);
        }
        else
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "WalkLayer", 1);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "AirLayer", 0);
        }
    }

    /// <summary>
    /// 入力の方向によってキャラを移動させる
    /// </summary>
    /// <param name="inputValue">受け取った入力(1,-1,0のいずれか)</param>
    protected virtual void Walk(float inputValue)
    {
        if (!CanWalk || !OnGround || _characterState.IsRidenByEnemy)
        {
            _animator.SetFloat("WalkFloat", 0);
            return;
        }

        if(inputValue == 0)
        {
            Velocity = new Vector2(0, Velocity.y);
            _animator.SetFloat("WalkFloat", inputValue);
            return;
        }

        float speedPower = inputValue; //キャラクターに加える横方向の力

        //キャラの向きと入力に速度とアニメーションを一致させる
        if (_characterState.IsLeftSide)
        {
            if (inputValue > 0)
            {
                speedPower *= _characterState.CurrentFrontSpeed;
            }
            else
            {
                speedPower *= _characterState.CurrentBackSpeed;
            }
            _animator.SetFloat("WalkFloat", inputValue);
        }
        else
        {
            if (inputValue > 0)
            {
                speedPower *= _characterState.CurrentBackSpeed;
            }
            else
            {
                speedPower *= _characterState.CurrentFrontSpeed;
            }
            _animator.SetFloat("WalkFloat", -inputValue);
        }

        Velocity = new Vector2(speedPower, Velocity.y);
    }

    /// <summary>
    /// ジャンプさせる
    /// </summary>
    protected virtual void Jump()
    {
        if (!CanJump) return;

        //接地中のみ可能
        if (!OnGround) return;

        SetGround(false);

        //物理挙動
        Vector2 jumpVector = new Vector2(0, _characterState.CurrentJumpPower);

        AddForce(jumpVector);

        //アニメーション
        _animator.SetTrigger("JumpTrigger");
    }

    /// <summary>
    /// キャラクターとの接触をFightingRigidBodyから受け取る
    /// </summary>
    protected override void PushBackCharacter(FightingRigidBody other)
    {
        //重なっている部分の長さ
        float overlapLength;

        //押し出し速度
        float pushPower = 0.2f;

        //どちら側にいるか
        bool isleftSide = GetPushBackBox().center.x < other.GetPushBackBox().center.x;

        if(isleftSide)
        {
            float enemyLeftPos = other.GetPushBackBox().xMin;
            float thisRightPos = GetPushBackBox().xMax;
            overlapLength = enemyLeftPos - thisRightPos;

            if(overlapLength < 0)
            {
                if (-overlapLength < pushPower)
                {
                    pushPower = -overlapLength;
                }

                other.transform.position += new Vector3(pushPower, 0, 0);
            }
        }
        else 
        {
            float enemyRightPos = other.GetPushBackBox().xMax;
            float thisLeftPos = GetPushBackBox().xMin;
            overlapLength = thisLeftPos - enemyRightPos;

            if (overlapLength < 0)
            {
                if (-overlapLength < pushPower)
                {
                    pushPower = -overlapLength;
                }

                other.transform.position -= new Vector3(pushPower, 0, 0);
            }
        }
    }

    protected override async void OnLand()
    {
        Velocity = Vector2.zero;
        _IsCompleteLandStun = false;
        await FightingPhysics.DelayFrameWithTimeScale(1);
        _IsCompleteLandStun = true;
        Land();
    }

    /// <summary>
    /// 着地したときに呼ばれるメソッド
    /// ジャンプ攻撃をキャンセルしたりする用
    /// </summary>
    protected virtual void Land()
    {
        Debug.Log("着地");
    }

    /// <summary>
    /// 攻撃を受けた時の処理
    /// </summary>
    /// /// <param name="attackInfo">受けた攻撃の情報</param>
    public virtual async UniTask TakeAttack(AttackInfo attackInfo)
    {
        //各種行動キャンセル
        CancelActionByHit();

        //AI学習
        OnHurtAI?.Invoke();

        //コンボ処理
        if (_characterState.IsRecoveringHit)
        {
            _characterState.CancelHitStun();
            _characterState.SetComboCount(_characterState.ConboCount + 1);
            OnComboAI?.Invoke();
            Debug.Log($"{_characterState.ConboCount}コンボ");

            //演出反映
            if(ComboCount != null)
            {
                ComboCount(PlayerNum, _characterState.ConboCount).Forget();
            }          
        }
        else
        {
            _characterState.SetComboCount(1);
        }

        //エフェクト処理
        Vector2 hurtBoxPos = transform.TransformPoint(_hurtBox.GetComponent<BoxCollider2D>().offset);
        if (attackInfo.IsHeavy)
        {
            OnEffect?.Invoke(hurtBoxPos, FightingEffect.LargeHit);
        }
        else
        {
            OnEffect?.Invoke(hurtBoxPos, FightingEffect.SmallHit);
        }

        //ダメージ処理
        if (CanHit)
        {
            _characterState.TakeDamage(attackInfo.Damage);
            Debug.Log($"Player{PlayerNum}は{attackInfo.Damage}ダメージ受けた");
            if (_characterState.CurrentHP <= 0)
            {
                Die();
                return;
            }
        }

        //アニメーション処理
        AnimatorByLayerName.SetLayerWeightByName(_animator, "HurtLayer", 1); // AnimatorのLayerをHitLayerを最優先に変更
        _animator.SetTrigger("HurtTrigger"); // 喰らいアニメーション再生

        //コンボ技登録
        _characterState.AddAttackName(attackInfo);

        //ヒットストップ処理
        await HitStop(attackInfo.HitStopFrame);

        //ヒットバック処理(Bind状態ではヒットバックしない)
        if(!_characterState.AnormalyStates.Contains(AnormalyState.Bind))
        {
            Velocity = Vector2.zero;
            Vector2 hitBackVector = attackInfo.HitBackDirection;
            if (EnemyCA.GetPushBackBox().x > GetPushBackBox().center.x)
            {
                hitBackVector = attackInfo.HitBackDirection * new Vector2(-1, 1);
            }

            int sameAttackCount = _characterState.NameOfGivenAttack.Count(n => n == attackInfo.Name);

            //同技補正
            if (sameAttackCount != 0)
            {
                float correction = 1 / (float)sameAttackCount;

                //y方向に浮きにくくなっていく
                hitBackVector *= new Vector2(1, correction);
                Debug.Log(hitBackVector);
            }
            AddForce(hitBackVector);
        }

        //ヒット硬直処理
        await HitStun(attackInfo.HitFrame, _characterState.CreateHitCT());

        AnimatorByLayerName.SetLayerWeightByName(_animator, "HurtLayer", 0); // AnimatorのLayerをHitLayerを最優度を元に戻す
    }

    /// <summary>
    /// 攻撃を受けたことによる各アクションのキャンセル
    /// </summary>
    public abstract void CancelActionByHit();

    /// <summary>
    /// ヒット硬直の処理
    /// </summary>
    /// <param name="recoveryFrame">硬直時間(フレーム)</param>
    private async UniTask HitStun(int recoveryFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(recoveryFrame, cancellationToken: token);

        //バインド状態ではヒット硬直から回復しない
        await UniTask.WaitUntil(() => 
        !_characterState.AnormalyStates.Contains(AnormalyState.Bind), cancellationToken: token);

        //着地するまでヒット硬直が続く
        await UniTask.WaitUntil(() => OnGround, cancellationToken: token);


        if (token.IsCancellationRequested)
        {
            Debug.Log("ヒット硬直がキャンセルされた");
        }
        else
        {
            _characterState.ClearNameOfGivenAttack();
        }

        _characterState.CancelHitStun();
    }

    /// <summary>
    /// ヒットストップ
    /// </summary>
    private async UniTask HitStop(int stopFrame)
    {
        Time.timeScale = 0;
        FightingPhysics.SetFightTimeScale(0);
        await UniTask.DelayFrame(stopFrame - 1);
        Time.timeScale = 1;
        FightingPhysics.SetFightTimeScale(1);
        await UniTask.Yield(); // 1フレーム待機して速度適用を保証
    }

    /// <summary>
    /// ガード構えをする
    /// </summary>
    protected virtual void GuardStance(bool isGuarding)
    {
        if (!CanGuard) return;

        // Layerを変更
        if (isGuarding)
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 1);
            //発動時SP消費
            if(!_characterState.IsGuarding)
            {
                _characterState.SetCurrentSP(-10);
            }
            _characterState.SetIsGuarding(true);
        }
        else
        {
            //ガード解除
            GuardRelease();
        }
    }

    /// <summary>
    /// ガード解除
    /// </summary>
    private void GuardRelease()
    {
        AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 0);
        _characterState.SetIsGuarding(false);
    }

    /// <summary>
    /// 攻撃をガードした時の処理
    /// </summary>
    /// <param name="attackInfo">受けた攻撃の情報</param>
    public virtual async UniTask Guard(AttackInfo attackInfo)
    {
        //AI学習
        OnGuardAI?.Invoke();

        //ガードバック処理
        Velocity = Vector2.zero;
        Vector2 guardBackVector = attackInfo.GuardBackDirection;
        if (EnemyCA.GetPushBackBox().x > GetPushBackBox().center.x)
        {
            guardBackVector *= new Vector2(-1, 1);
        }
        AddForce(guardBackVector);

        //相手ののけぞり
        EnemyCA.GuardedBack(attackInfo);

        //エフェクト発生
        Vector2 hurtBoxPos = transform.TransformPoint(_hurtBox.GetComponent<BoxCollider2D>().offset);
        OnEffect?.Invoke(hurtBoxPos, FightingEffect.Guard);

        //SP削り
        _characterState.SetCurrentSP(-attackInfo.DrainSP);

        //連続ガード処理
        if(_characterState.IsRecoveringGuard)
        {
            //前回の硬直を処理
            _characterState.CancelGuardStun();
        }

        await GuardStun(attackInfo.GuardFrame, _characterState.CreateGuardCT());
    }

    private async UniTask GuardStun(int guardFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(guardFrame, cancellationToken: token);

        if (token.IsCancellationRequested)
        {
            Debug.Log("ガード硬直がキャンセルされた");
            _characterState.CancelGuardStun();
            return;
        }

        _characterState.CancelGuardStun();
    }

    /// <summary>
    /// 攻撃をガードされたときののけぞり
    /// </summary>
    private void GuardedBack(AttackInfo attackInfo)
    {
        Vector2 guardedBackVector = attackInfo.GuardedBackDirection;
        if (EnemyCA.GetPushBackBox().x > GetPushBackBox().center.x)
        {
            guardedBackVector *= new Vector2(-1, 1);
        }
        AddForce(guardedBackVector);
    }

    /// <summary>
    /// Break状態になる
    /// </summary>
    protected virtual void Break()
    {
        _characterState.TakeAnormalyState(AnormalyState.Fatigue);
        OnEffect?.Invoke(GetPushBackBox().center, FightingEffect.Break);
        GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0, 0);
        OnBreakAI?.Invoke();
    }

    /// <summary>
    /// Break状態から回復
    /// </summary>
    protected virtual void RecoverBreak()
    {
        _characterState.RecoverAnormalyState(AnormalyState.Fatigue);
        OnEffect?.Invoke(GetPushBackBox().center, FightingEffect.RecoverBreak);
        GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0, 0);
    }

    /// <summary>
    /// UP増加
    /// </summary>
    public virtual void UPgain(float value)
    {
        _characterState.SetCurrentUP(value);
    }

    /// <summary>
    /// 死亡処理
    /// HPが0以下になると呼ばれる
    /// </summary>
    protected virtual void Die() 
    {
        _characterState.TakeAnormalyState(AnormalyState.Dead);
        AnimatorByLayerName.SetLayerWeightByName(_animator, "DieLayer", 1);
        _animator.SetBool("DieBool", true);
        OnDie?.Invoke(PlayerNum);
        OnDieAI?.Invoke();
    }

    /// <summary>
    /// 入力の方向
    /// </summary>
    /// <returns>0:入力なし, 1:前, -1:後ろ</returns>
    protected int WalkDirection()
    {
        int direction;

        if(GetPushBackBox().center.x <= EnemyCA.GetPushBackBox().center.x)
        {
            direction = (int)_inputReciever.WalkValue;
        }
        else
        {
            direction = (int)-_inputReciever.WalkValue;
        }

        return direction;
    }
}
