using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// �ΐ펞�ȊO�̎��ԑ��x���Ǘ�
/// </summary>
public static class TimeSpeedManager
{
    public static float timeScale { get; private set; } = 1f;

    /// <summary>
    /// �w�肵��FPS��ŁAtimeScale�̉e�����󂯂�t���[���ҋ@���s���B
    /// </summary>
    /// <param name="frames">�ҋ@�������t���[����</param>
    /// <param name="fps">��ƂȂ�FPS�i��F60�j</param>
    /// <param name="cancellationToken">�L�����Z���g�[�N��</param>
    public static async UniTask DelayFrameScaled(int frames, float fps = 60f, CancellationToken cancellationToken = default)
    {
        float targetTime = frames / fps; // ��: 3�t���[�� @60FPS = 0.05�b
        float elapsed = 0f;

        while (elapsed < targetTime)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            elapsed += Time.unscaledDeltaTime * timeScale; ; // timeScale�̉e�����󂯂�
        }
    }
}
