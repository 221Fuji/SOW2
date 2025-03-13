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
        float elapsedTime = 0f; // �o�ߎ���
        int elapsedFrames = 0; // �o�߃t���[��

        while (elapsedFrames < frames)
        {
            float timeSpeed = FightingFrameRate * FightingTimeScale; // ���݂̃t���[�����[�g�ƃ^�C���X�P�[�����擾
            if (timeSpeed > 0)
            {
                float frameTime = 1f / timeSpeed; // 1�t���[���̎���
                elapsedTime += Time.unscaledDeltaTime; // �o�ߎ��Ԃ����Z

                while (elapsedTime >= frameTime) // �o�ߎ��Ԃ�1�t���[�����ȏ�ɂȂ�����J�E���g
                {
                    elapsedTime -= frameTime;
                    elapsedFrames++;
                    if (elapsedFrames >= frames) break;
                }
            }
            await UniTask.Yield(cancellationToken); // 1�t���[���ҋ@
        }
    }
}
