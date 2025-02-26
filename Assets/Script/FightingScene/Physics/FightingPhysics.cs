using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// �ΐ풆�̓Ǝ��̕�������
/// </summary>
public static class FightingPhysics
{
    private static CancellationTokenSource _slowCTS;

    /// <summary>
    /// �d�͉����x
    /// </summary>
    public static float GravityAcceleration { get; private set; } = 55f;

    /// <summary>
    /// ���C�W��
    /// 1�t���[���Ō������鑬�x
    /// </summary>
    public static float FrictionCoefficient { get; private set; } = 0.75f;

    /// <summary>
    /// �������x
    /// </summary>
    public static float FightingTimeScale { get; private set; } = 1;

    /// <summary>
    /// �d�͉����x�̕ύX����
    /// </summary>
    public static void SetGravityAcceleration(float value)
    {
        GravityAcceleration = value;
    }

    /// <summary>
    /// FightingPhysics�����̏������x��ύX����
    /// </summary>
    public static void SetFightTimeScale(float value)
    {
        FightingTimeScale = value;
    }

    /// <summary>
    /// TimeScale�̉e�����󂯂�UniTask��DelayFrame
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

            // �L�����Z�����v�����ꂽ�ꍇ�A�����𒆒f����
            cancellationToken.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
}
