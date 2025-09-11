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
    public static void SetFightingTimeScale(float value)
    {
        FightingTimeScale = value;
    }

    /// <summary>
    /// FightingUpdate の呼び出し回数と完全同期する DelayFrame
    /// </summary>
    public static async UniTask DelayFrameWithTimeScale(int frames, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < frames; i++)
        {
            // FightingTimeScale を考慮
            int step = Mathf.RoundToInt(FightingTimeScale);
            i += (step - 1); // まとめて進める

            await UniTask.Yield(cancellationToken);
        }
    }
}
