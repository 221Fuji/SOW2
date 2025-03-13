using UnityEngine;

/// <summary>
/// �U���̏����Ǘ�����\����
/// </summary>
[System.Serializable]
public struct AttackInfo
{
    public string Name; //�Z�� 
    public int Damage; // �_���[�W
    public int StartupFrame; // �����t���[��
    public int ActiveFrame; // �����t���[��
    public int RecoveryFrame; // �d���t���[��
    public int HitFrame; // �󂯂�����̍d���t���[��
    public int GuardFrame; // �K�[�h�����Ƃ��̍d���t���[��
    public int HitStopFrame; //�q�b�g�X�g�b�v�̃t���[��
    public float MeterGain; // �K�E�Q�[�W�����
    public float ConsumptionSP; // �X�^�~�i�����
    public float DrainSP; //�X�^�~�i����
    public bool IsHeavy; //�U���̏d��
    public Vector2 HitBackDirection; // �q�b�g�o�b�N�̃x�N�g��
    public Vector2 GuardBackDirection; // �J�[�h�o�b�N�̃x�N�g��
    public Vector2 GuardedBackDirection; //�K�[�h���ꂽ�Ƃ��̌�ރx�N�g��

    public AttackInfo(string name, int damge, int startupFrame, int activeFrame, int recoveryFrame,
        int hitFrame, int guardFram, int hitStopFrame, float meterGain, float consumptionSP, 
        float drainSP, bool isHeavy, Vector2 hitBackDirection, 
        Vector2 guardBackDirection, Vector2 guardedBackDirection)
    {
        Name = name;
        Damage = damge;
        StartupFrame = startupFrame;
        ActiveFrame = activeFrame;
        RecoveryFrame = recoveryFrame;
        HitFrame = hitFrame;
        GuardFrame = guardFram;
        HitStopFrame = hitStopFrame;
        MeterGain = meterGain;
        ConsumptionSP = consumptionSP;
        DrainSP = drainSP;
        IsHeavy = isHeavy;
        HitBackDirection = hitBackDirection;
        GuardBackDirection = guardBackDirection;
        GuardedBackDirection = guardedBackDirection;
    }
}
