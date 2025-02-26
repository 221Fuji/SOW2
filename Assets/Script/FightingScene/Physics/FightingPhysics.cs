using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 対戦中の独自の物理挙動
/// </summary>
public static class FightingPhysics
{
    private static CancellationTokenSource _slowCTS;

    /// <summary>
    /// 重力加速度
    /// </summary>
    public static float GravityAcceleration { get; private set; } = 55f;

    /// <summary>
    /// 摩擦係数
    /// 1フレームで減少する速度
    /// </summary>
    public static float FrictionCoefficient { get; private set; } = 0.75f;

    /// <summary>
    /// 処理速度
    /// </summary>
    public static float FightingTimeScale { get; private set; } = 1;

    /// <summary>
    /// 重力加速度の変更する
    /// </summary>
    public static void SetGravityAcceleration(float value)
    {
        GravityAcceleration = value;
    }

    /// <summary>
    /// FightingPhysics準拠の処理速度を変更する
    /// </summary>
    public static void SetFightTimeScale(float value)
    {
        FightingTimeScale = value;
    }

    /// <summary>
    /// TimeScaleの影響を受けるUniTaskのDelayFrame
    /// </summary>
    public static async UniTask DelayFrameWithTimeScale(int targetFrameCount, CancellationToken cancellationToken = default)
    {
        int frameCount = 0;
        while (frameCount < targetFrameCount)
        {
            if (Time.timeScale > 0)
            {
                frameCount++;
            }

            // キャンセルが要求された場合、処理を中断する
            cancellationToken.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
}
