using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

/// <summary>
/// �ΐ풆�̓Ǝ��̕�������
/// </summary>
public static class FightingPhysics
{

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
    /// �t���[�����[�g
    /// </summary>
    public static int FightingFrameRate { get; private set; } = 60;

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
    public static async UniTask DelayFrameWithTimeScale(int frames, CancellationToken cancellationToken = default)
    {
        if (frames <= 0 || FightingFrameRate <= 0) return;

        float frameTime = 1f / FightingFrameRate; // 1�t���[���̎���
        float waitTime = frames * frameTime; // �ҋ@���鑍����

        float startTime = Time.realtimeSinceStartup; // �J�n���ԁi���A���^�C���j
        while (Time.realtimeSinceStartup - startTime < waitTime)
        {
            await UniTask.Yield(cancellationToken); // 1�t���[�����ƂɃ`�F�b�N
        }
    }
}
