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
        SelfCA.OnHurtAI = OnHurt;
        SelfCA.OnComboAI = OnCombo;
        SelfCA.OnDieAI = OnDie;
        SelfCA.OnGuardAI = OnGuard;
        SelfCA.OnBreakAI = OnBreak;
        EnemyCA.OnHurtAI = OnHit;
        EnemyCA.OnGuardAI = OnGuarded;
        EnemyCA.OnBreakAI = OnBroken;
        EnemyCA.OnDieAI = OnKill;
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
    }

    //攻撃を喰らったとき
    private void OnHurt()
    {
        AddReward(-1.5f);
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
}
