using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public abstract class CPUAgent : Agent
{
    [SerializeField] protected int _numActions;

    public FightingInputReceiver InputReciever { get; private set; }
    public int SelfCharaNum { get; private set; }
    public int EnemyCharaNum { get; private set; }
    public CharacterActions SelfCA { get; private set; }
    public CharacterActions EnemyCA { get; private set; }
    public CharacterState SelfCS { get; private set; }
    public CharacterState EnemyCS { get; private set; }

    private int _preMoveNum = 0; // 前回の行動番号
    private int _leftFrameSinceHit = 0; // 攻撃を当てて（ガードさせて）からの時間
    private readonly int _assertiveness = 120; // 攻撃を当てようとする積極性(小さい方が高い)
    private int _leftFrameSinceChangeVector; // 歩く向きを変えてからの時間
    private readonly int _frequencyWalkChange = 30; // 歩く向きの切り替え頻度
    private int _leftFrameSinceGuard = 0; // ガード中のフレーム
    private readonly int _frequencyGuardChange = 30; // ガード切り替え頻度

    public virtual void SetCAandCS()
    {
        InputReciever = GetComponent<FightingInputReceiver>();
        SelfCA = GetComponent<CharacterActions>();
        SelfCS = GetComponent<CharacterState>();
        EnemyCA = SelfCA.EnemyCA;
        EnemyCS = EnemyCA.GetComponent<CharacterState>();

        OnEpisodeStart();
    }

    protected virtual void OnEpisodeStart()
    {
        // デリゲートの設定
        SelfCA.OnHurtAI += OnHurt;
        SelfCA.OnGuardAI += OnGuard;
        SelfCA.OnComboAI += OnCombo;
        SelfCA.OnDieAI += OnDie;
        SelfCA.OnBreakAI += OnBreak;
        SelfCA.OnMissAI += OnMiss;

        EnemyCA.OnHurtAI += OnHit;
        EnemyCA.OnGuardAI += OnGuarded;
        EnemyCA.OnDieAI += OnKill;
        EnemyCA.OnMissAI = OnAvert;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 自分の基本情報を観測
        sensor.AddObservation(SelfCA.HurtBox.transform.position);
        sensor.AddObservation(SelfCS.CurrentHP / SelfCS.MaxHP);
        sensor.AddObservation(SelfCS.CurrentSP / SelfCS.MaxSP);
        sensor.AddObservation(SelfCS.CurrentUP / SelfCS.MaxUP);
        bool isSelfBroken = SelfCS.AnormalyStates.Contains(AnormalyState.Fatigue);
        sensor.AddObservation(isSelfBroken ? 1 : 0);

        // 相手の基本情報を観測
        sensor.AddObservation(EnemyCharaNum);
        sensor.AddObservation(EnemyCA.HurtBox.transform.position);
        sensor.AddObservation(EnemyCS.CurrentHP / EnemyCS.MaxHP);
        sensor.AddObservation(EnemyCS.CurrentSP / EnemyCS.MaxSP);
        sensor.AddObservation(EnemyCS.CurrentUP / SelfCS.MaxUP);
        bool isEnemyBroken = EnemyCS.AnormalyStates.Contains(AnormalyState.Fatigue);
        sensor.AddObservation(isEnemyBroken ? 1 : 0);

        //相対情報
        Vector2 distance = SelfCA.GetPushBackBox().center - (Vector2)EnemyCA.HurtBox.transform.position;
        sensor.AddObservation(distance);
    }

    private void Update()
    {
        if (SelfCS == null) return;

        if(FightingPhysics.FightingTimeScale <= 0) return;

        if(SelfCS.AcceptOperations)
        {
            RequestDecision();

            //毎フレームカウント
            _leftFrameSinceHit++;
            _leftFrameSinceChangeVector++;
        }       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int move = actions.DiscreteActions[0];

        switch (move)
        {
            case 0: InputReciever.SetWalkDirectionFromAI(0); break;
            case 1: InputReciever.SetWalkDirectionFromAI(-1f); break;
            case 2: InputReciever.SetWalkDirectionFromAI(1f); break;
            case 3: InputReciever.OnJump(); break;
            case 4: InputReciever.OnNomalMove(); break;
            case 5: InputReciever.OnSpecialMove1(); break;
            case 6: InputReciever.OnSpecialMove2(); break;
            case 7: InputReciever.OnUltimate(); break;
            case 8: InputReciever.SetGuardFromAI(true); break;
            case 9: InputReciever.SetGuardFromAI(false); break;
        }

        AddRewardEveryFrame(move);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        for (int i = 3; i < _numActions; i++)
        {
            actionMask.SetActionEnabled(0, i, false);
        }

        if (SelfCA.CanJump)
        {
            actionMask.SetActionEnabled(0, 3, true);
        }

        if (SelfCA.CanGuard)
        {
            actionMask.SetActionEnabled(0, 8, true);
        }

        if (SelfCS.IsGuarding)
        {
            actionMask.SetActionEnabled(0, 9, true);
        }

        DiscreteUniqueActionMask(actionMask);
    }

    public abstract void DiscreteUniqueActionMask(IDiscreteActionMask actionMask);


    /// <summary>
    /// 毎フレーム報酬を与える項目
    /// </summary>
    private void AddRewardEveryFrame(int currentMoveNum)
    {
        //立ち止まるの減点
        if(currentMoveNum == 0)
        {
            AddReward(-0.0005f);
        }

        //カクカクしないようにするため
        bool changeFromLeft = _preMoveNum == 1 && (currentMoveNum == 0 || currentMoveNum == 2);
        bool changeFromRight = _preMoveNum == 2 && (currentMoveNum == 0 || currentMoveNum == 1);
        if(changeFromLeft || changeFromRight)
        {
            if (_leftFrameSinceChangeVector < _frequencyWalkChange)
            {
                AddReward(-0.01f);
            }
            _leftFrameSinceChangeVector = 0;
        }

        //ガード切り替え連打をしないようにするため
        if (currentMoveNum == 9)
        {
            if (_leftFrameSinceGuard < _frequencyGuardChange)
            {
                AddReward(-0.05f);
            }
        }
        if (SelfCS.IsGuarding)
        {
            _leftFrameSinceGuard++;
        }
        else
        {
            _leftFrameSinceGuard = 0;
        }

        //積極的に攻撃するように
        if(_leftFrameSinceHit < _assertiveness)
        {
            AddReward(-0.0005f);
        }

        _preMoveNum = currentMoveNum;
    }

    //攻撃を喰らったとき
    protected virtual void OnHurt(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDamage(-1f, attackInfo.Damage));
    }

    //攻撃がヒットしたとき
    protected virtual void OnHit(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDamage(1.25f, attackInfo.Damage));
        _leftFrameSinceHit = 0;
    }

    //攻撃をガードしたとき
    protected virtual void OnGuard(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDrainSP(0.1f, attackInfo.DrainSP));
        //攻撃ガード成功後すぐにガードを解除してもOK
        _leftFrameSinceGuard = _frequencyGuardChange;
    }

    //攻撃をガードさせたとき
    protected virtual void OnGuarded(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDrainSP(1, attackInfo.DrainSP));
        _leftFrameSinceHit = 0;
    }

    //攻撃を空振りしたとき
    protected virtual void OnMiss()
    {
        AddReward(-0.5f);
    }

    //攻撃を避けた時
    protected virtual void OnAvert()
    {
        AddReward(0.25f);
    }

    //コンボしたとき
    protected virtual void OnCombo()
    {
        AddReward(0.75f);
    }

    //自身がBreak状態になったとき
    protected virtual void OnBreak()
    {
        AddReward(-1f);
    }

    //倒したとき
    protected virtual void OnKill()
    {
        float reward = 2f;
        reward += (SelfCS.CurrentHP / SelfCS.MaxHP) * 1.5f;
        SetReward(reward);
        OnEpisodeEnd();
        EndEpisode();
    }

    //倒されたとき
    protected virtual void OnDie()
    {
        float reward = -2f;
        reward -= (EnemyCS.CurrentHP/EnemyCS.MaxHP) * 1.5f;
        SetReward(reward);
        OnEpisodeEnd();
        EndEpisode();
    }

    private void OnEpisodeEnd()
    {
        float totalReward = GetCumulativeReward();

        Debug.Log($"Player{SelfCA.PlayerNum}が報酬:{totalReward}を獲得");

        //デリゲートの初期化
        SelfCA.OnHurtAI = null;
        SelfCA.OnComboAI = null;
        SelfCA.OnDieAI = null;
        SelfCA.OnGuardAI = null;
        SelfCA.OnBreakAI = null;
        SelfCA.OnMissAI = null;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 開発用にテスト操作を記述
    }

    protected float CalRewardByDamage(float baseReward, float damage)
    {       
        return baseReward * (damage / 20);
    }

    protected float CalRewardByDrainSP(float baseReward, float drainSP)
    {
        return baseReward * (drainSP / 20);
    }
}
