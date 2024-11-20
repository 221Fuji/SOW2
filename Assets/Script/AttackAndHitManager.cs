using UnityEngine;

/// <summary>
/// 攻撃の情報を管理する構造体
/// </summary>
[System.Serializable]
public struct AttackInfo
{
    public int Damage; // ダメージ
    public int StartupFrame; // 発生フレーム
    public int ActiveFrame; // 持続フレーム
    public int RecoveryFrame; // 硬直フレーム
    public int HitFrame; // 受けた相手の硬直フレーム
    public float MeterGain; // 必殺ゲージ回収量
    public Vector2 HitBackDirection; // ヒットバックのベクトル

    public AttackInfo(int damge, int startupFrame, int activeFrame, int recoveryFrame,
        int hitFrame, float meterGain, Vector2 hitBackDirection)
    {
        Damage = damge;
        StartupFrame = startupFrame;
        ActiveFrame = activeFrame;
        RecoveryFrame = recoveryFrame;
        HitFrame = hitFrame;
        MeterGain = meterGain;
        HitBackDirection = hitBackDirection;
    }
}
