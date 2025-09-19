using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class FrameManager : MonoBehaviour
{
    public static FrameManager Instance { get; private set; }
    private float _accumulator = 0f;

    // フレーム更新のコールバック
    public delegate void FightingUpdateEvent();
    /// <summary>
    /// フレーム更新のコールバック
    /// </summary>
    public FightingUpdateEvent OnfightingUpdate { get; private set; }

    // フレームのカウント
    private static int _stepCounter = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        _accumulator += Time.deltaTime;
        float frameTime = 1f / (FightingPhysics.FightingFrameRate * FightingPhysics.FightingTimeScale);
        while (_accumulator >= frameTime)
        {
            OnfightingUpdate?.Invoke();
            _stepCounter++;
            _accumulator -= frameTime;
        }
    }

    /// <summary>
    /// 任意のフレーム待機
    /// </summary>
    public static async UniTask DeleyFightingFrame(int frames, CancellationToken token = default)
    {
        int target = _stepCounter + frames;
        try
        {
            await UniTask.WaitUntil(() => _stepCounter >= target, cancellationToken: token);
        }
        catch
        { }
    }
}
