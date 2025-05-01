using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Reflection;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class CPUAgent : Agent
{
    public FightingInputReceiver InputReciever { get; private set; }
    public int SelfCharaNum { get; private set; }
    public int EnemyCharaNum { get; private set; }
    public CharacterActions SelfCA { get; private set; }
    public CharacterActions EnemyCA { get; private set; }
    public CharacterState SelfCS { get; private set; }
    public CharacterState EnemysCS { get; private set; }

<<<<<<< HEAD
    private int _preMoveNum = 0; // 前回の行動番号
    private int _leftFrameSinceHit = 0; // 攻撃を当てて（ガードさせて）からの時間
    private readonly int _assertiveness = 120; // 攻撃を当てようとする積極性(小さい方が高い)
    private int _leftFrameSinceChangeVector; // 歩く向きを変えてからの時間
    private readonly int _frequencyWalkChange = 30; // 歩く向きの切り替え頻度
    private int _leftFrameSinceGuard = 0; // ガード中のフレーム
    private readonly int _frequencyGuardChange = 30; // ガード切り替え頻度
=======
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04

    public void SetCAandCS()
    {
        InputReciever = GetComponent<FightingInputReceiver>();
        SelfCA = GetComponent<CharacterActions>();
        SelfCS = GetComponent<CharacterState>();
        EnemyCA = SelfCA.EnemyCA;
        EnemysCS = EnemyCA.GetComponent<CharacterState>();

        OnEpisodeStart();
    }

    private void OnEpisodeStart()
    {
        // デリゲートの設定
<<<<<<< HEAD
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
=======
        SelfCA.OnHurtAI = OnHurt;
        SelfCA.OnComboAI = OnCombo;
        SelfCA.OnDieAI = OnDie;
        SelfCA.OnGuardAI = OnGuard;
        SelfCA.OnBreakAI = OnBreak;
        EnemyCA.OnHurtAI = OnHit;
        EnemyCA.OnGuardAI = OnGuarded;
        EnemyCA.OnBreakAI = OnBroken;
        EnemyCA.OnDieAI = OnKill;
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 自分の基本情報を観測
        sensor.AddObservation(SelfCharaNum);
        sensor.AddObservation(SelfCA.GetPushBackBox().center);
        sensor.AddObservation(SelfCS.CurrentHP / SelfCS.MaxHP);
        sensor.AddObservation(SelfCS.CurrentSP / SelfCS.MaxSP);
        sensor.AddObservation(SelfCS.CurrentUP / SelfCS.MaxUP);

        // 相手の基本情報を観測
        sensor.AddObservation(EnemyCharaNum);
<<<<<<< HEAD
        sensor.AddObservation(EnemyCA.HurtBox.transform.position);
        sensor.AddObservation(EnemyCS.CurrentHP / EnemyCS.MaxHP);
        sensor.AddObservation(EnemyCS.CurrentSP / EnemyCS.MaxSP);
        sensor.AddObservation(EnemyCS.CurrentUP / SelfCS.MaxUP);
        bool isEnemyBroken = EnemyCS.AnormalyStates.Contains(AnormalyState.Fatigue);
        sensor.AddObservation(isEnemyBroken ? 1 : 0);

        //相対情報
        Vector2 distance = SelfCA.GetPushBackBox().center - (Vector2)EnemyCA.HurtBox.transform.position;
        sensor.AddObservation(distance);
=======
        sensor.AddObservation(EnemyCA.GetPushBackBox().center);
        sensor.AddObservation(EnemysCS.CurrentHP / EnemysCS.MaxHP);
        sensor.AddObservation(EnemysCS.CurrentSP / EnemysCS.MaxSP);

        /*
        //煙霧の情報
        foreach(FightingRigidBody frb in FightingRigidBody.FightingRigidBodies)
        {
            if(frb is Fog fog)
            {
                sensor.AddObservation(fog.PlayerNum);
                sensor.AddObservation(fog.transform.position);
                sensor.AddObservation(fog.transform.lossyScale);
            }
        }

        //攻撃の当たり判定
        foreach(HitBoxManager hitBox in HitBoxManager.HitBoxes)
        {
            sensor.AddObservation(hitBox.PlayerNum);
            sensor.AddObservation(hitBox.transform.position);
            sensor.AddObservation(hitBox.transform.lossyScale);
        }

        //固有リソースの観測
        if(SelfCA is ViolaCloud selfCloud)
        {
            sensor.AddObservation(selfCloud.CurrentFogResource / selfCloud.FogMaxResource);
        }
        if(EnemyCA is ViolaCloud enemysCloud)
        {
            sensor.AddObservation(enemysCloud.CurrentFogResource / enemysCloud.FogMaxResource);
        }
        */
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04
    }

    private void Update()
    {
        RequestDecision();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int move = actions.DiscreteActions[0];

        // 例えば 0 = Idle, 1 = Walk Left, 2 = Walk Right, 3 = Attack, 4 = Guard, ...
        switch (move)
        {
            case 0: InputReciever.SetWalkDirectionFromAI(0); break;
            case 1: InputReciever.SetWalkDirectionFromAI(-1f); break;
            case 2: InputReciever.SetWalkDirectionFromAI(1f); break;
            case 3: InputReciever.OnJump(); break;
            case 4: InputReciever.OnNomalMove(); break;
            case 5: InputReciever.OnSpecialMove1(); break;
            case 6: InputReciever.OnUltimate(); break;
            case 7: InputReciever.SetGuardFromAI(true); break;
            case 8: InputReciever.SetGuardFromAI(false); break;
        }

        //空中にいるのはあまりよくない
        if(SelfCA.OnGround)
        {
            AddReward(-0.005f);
        }
<<<<<<< HEAD

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
=======
    }

    //攻撃を喰らったとき
    private void OnHurt()
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04
    {
        AddReward(-1f);
    }

    //攻撃をヒットさせたとき
    private void OnHit()
    {
        AddReward(1f);
    }

    //攻撃をガードしたとき
    private void OnGuard()
    {
        AddReward(0.5f);
    }

    //攻撃をガードさせたとき
    private void OnGuarded()
    {
        AddReward(0.5f);
    }

    //コンボしたとき
    private void OnCombo()
    {
        AddReward(0.5f);
    }

    //自身がBreak状態になったとき
    private void OnBreak()
    {
        AddReward(-2f);
    }

    //相手をBreak状態にしたとき
    private void OnBroken() 
    {
        AddReward(0.5f);
    }

    //倒したとき
    private void OnKill()
    {
        SetReward(5f);
        EndEpisode();
    }

    //倒されたとき
    private void OnDie()
    {
        SetReward(-5f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 開発用にテスト操作を記述（今回は不要でもOK）
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
