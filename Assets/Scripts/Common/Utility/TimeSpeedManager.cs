using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 対戦時以外の時間速度を管理
/// </summary>
public static class TimeSpeedManager
{
    public static float timeScale { get; private set; } = 1f;

    /// <summary>
    /// 指定したFPS基準で、timeScaleの影響を受けるフレーム待機を行う。
    /// </summary>
    /// <param name="frames">待機したいフレーム数</param>
    /// <param name="fps">基準となるFPS（例：60）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    public static async UniTask DelayFrameScaled(int frames, float fps = 60f, CancellationToken cancellationToken = default)
    {
        float targetTime = frames / fps; // 例: 3フレーム @60FPS = 0.05秒
        float elapsed = 0f;

        while (elapsed < targetTime)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            elapsed += Time.unscaledDeltaTime * timeScale; ; // timeScaleの影響を受ける
        }
    }
}
