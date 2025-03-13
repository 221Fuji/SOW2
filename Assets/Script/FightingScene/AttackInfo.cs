using UnityEngine;

/// <summary>
/// 攻撃の情報を管理する構造体
/// </summary>
[System.Serializable]
public struct AttackInfo
{
    public string Name; //技名 
    public int Damage; // ダメージ
    public int StartupFrame; // 発生フレーム
    public int ActiveFrame; // 持続フレーム
    public int RecoveryFrame; // 硬直フレーム
    public int HitFrame; // 受けた相手の硬直フレーム
    public int GuardFrame; // ガードしたときの硬直フレーム
    public int HitStopFrame; //ヒットストップのフレーム
    public float MeterGain; // 必殺ゲージ回収量
    public float ConsumptionSP; // スタミナ消費量
    public float DrainSP; //スタミナ削り量
    public bool IsHeavy; //攻撃の重さ
    public Vector2 HitBackDirection; // ヒットバックのベクトル
    public Vector2 GuardBackDirection; // カードバックのベクトル
    public Vector2 GuardedBackDirection; //ガードされたときの後退ベクトル

    public AttackInfo(string name, int damge, int startupFrame, int activeFrame, int recoveryFrame,
        int hitFrame, int guardFram, int hitStopFrame, float meterGain, float consumptionSP, 
        float drainSP, bool isHeavy, Vector2 hitBackDirection, 
        Vector2 guardBackDirection, Vector2 guardedBackDirection)
    {
        Name = name;
        Damage = damge;
        StartupFrame = startupFrame;
        ActiveFrame = activeFrame;
        RecoveryFrame = recoveryFrame;
        HitFrame = hitFrame;
        GuardFrame = guardFram;
        HitStopFrame = hitStopFrame;
        MeterGain = meterGain;
        ConsumptionSP = consumptionSP;
        DrainSP = drainSP;
        IsHeavy = isHeavy;
        HitBackDirection = hitBackDirection;
        GuardBackDirection = guardBackDirection;
        GuardedBackDirection = guardedBackDirection;
    }
}
