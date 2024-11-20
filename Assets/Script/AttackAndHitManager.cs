using UnityEngine;

/// <summary>
/// �U���̏����Ǘ�����\����
/// </summary>
[System.Serializable]
public struct AttackInfo
{
    public int Damage; // �_���[�W
    public int StartupFrame; // �����t���[��
    public int ActiveFrame; // �����t���[��
    public int RecoveryFrame; // �d���t���[��
    public int HitFrame; // �󂯂�����̍d���t���[��
    public float MeterGain; // �K�E�Q�[�W�����
    public Vector2 HitBackDirection; // �q�b�g�o�b�N�̃x�N�g��

    public AttackInfo(int damge, int startupFrame, int activeFrame, int recoveryFrame,
        int hitFrame, float meterGain, Vector2 hitBackDirection)
    {
        Damage = damge;
        StartupFrame = startupFrame;
        ActiveFrame = activeFrame;
        RecoveryFrame = recoveryFrame;
        HitFrame = hitFrame;
        MeterGain = meterGain;
        HitBackDirection = hitBackDirection;
    }
}
