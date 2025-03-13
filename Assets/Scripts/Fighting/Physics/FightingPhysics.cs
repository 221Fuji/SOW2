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
        float elapsedTime = 0f; // 経過時間
        int elapsedFrames = 0; // 経過フレーム

        while (elapsedFrames < frames)
        {
            float timeSpeed = FightingFrameRate * FightingTimeScale; // 現在のフレームレートとタイムスケールを取得
            if (timeSpeed > 0)
            {
                float frameTime = 1f / timeSpeed; // 1フレームの時間
                elapsedTime += Time.unscaledDeltaTime; // 経過時間を加算

                while (elapsedTime >= frameTime) // 経過時間が1フレーム分以上になったらカウント
                {
                    elapsedTime -= frameTime;
                    elapsedFrames++;
                    if (elapsedFrames >= frames) break;
                }
            }
            await UniTask.Yield(cancellationToken); // 1フレーム待機
        }
    }
}
