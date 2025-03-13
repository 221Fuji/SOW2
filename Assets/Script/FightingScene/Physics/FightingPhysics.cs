using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

/// <summary>
/// 対戦中の独自の物理挙動
/// </summary>
public static class FightingPhysics
{

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
    /// フレームレート
    /// </summary>
    public static int FightingFrameRate { get; private set; } = 60;

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
    public static async UniTask DelayFrameWithTimeScale(int frames, CancellationToken cancellationToken = default)
    {
        if (frames <= 0 || FightingFrameRate <= 0) return;

        float frameTime = 1f / FightingFrameRate; // 1フレームの時間
        float waitTime = frames * frameTime; // 待機する総時間

        float startTime = Time.realtimeSinceStartup; // 開始時間（リアルタイム）
        while (Time.realtimeSinceStartup - startTime < waitTime)
        {
            await UniTask.Yield(cancellationToken); // 1フレームごとにチェック
        }
    }
}
