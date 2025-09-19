using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

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

    public delegate void FightingUpdateCallBack();
    /// <summary>
    /// 対戦中毎フレーム呼ばれるコールバック
    /// </summary>
    public static FightingUpdateCallBack OnFightingUpdate { get; set; }
    public static CancellationTokenSource FightingUpdateCTS { get; private set; }

    /// <summary>
    /// FightingUpdate の呼び出し回数と同期する DelayFrame
    /// 1フレーム = 1/60秒 を FightingTimeScale 倍して待機
    /// </summary>
    public static async UniTask DelayFrameWithTimeScale(int frames, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1フレームの秒数（60fps基準）
            float baseFrameTime = 1f / FightingFrameRate;

            for (int i = 0; i < frames; i++)
            {
                // FightingTimeScale を考慮した実際の時間
                float scaledFrameTime = baseFrameTime * FightingTimeScale;

                // UniTask.Delay はミリ秒指定なので変換
                int delayMs = Mathf.Max(1, Mathf.RoundToInt(scaledFrameTime * 1000f));

                await UniTask.Delay(delayMs, cancellationToken: cancellationToken);
            }
        }
        catch
        {
            // キャンセル時やエラー時は無視して終了
        }
    }

    private static float _lastTime;
    /// <summary>
    /// 対戦中毎フレーム更新される
    /// </summary>
    public static async UniTask FightingUpdate()
    {
        FightingUpdateCTS?.Cancel();
        FightingUpdateCTS = new CancellationTokenSource();
        CancellationToken token = FightingUpdateCTS.Token;
        _lastTime = Time.realtimeSinceStartup;

        while (!token.IsCancellationRequested)
        {

            float now = Time.realtimeSinceStartup;
            float elapsed = now - _lastTime;
            _lastTime = now;

            Debug.Log($"疑似フレーム経過時間: {elapsed * 1000f:F2} ms");
            OnFightingUpdate?.Invoke();
            try
            {
                await DelayFrameWithTimeScale(1, token);
            }
            catch
            {
                break;
            }
        }
    }

    /// <summary>
    /// FightingUpdateを中断する
    /// </summary>
    public static void CancelUpdate()
    {
        FightingUpdateCTS?.Cancel();
        FightingUpdateCTS?.Dispose();
        FightingUpdateCTS = null;
    }


}
