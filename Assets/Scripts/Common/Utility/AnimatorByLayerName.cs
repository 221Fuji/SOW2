using UnityEngine;

public static class AnimatorByLayerName
{
    /// <summary>
    /// Animator��SetlayerWeight��layer�̖��O�ŌĂяo��
    /// </summary>
    public static void SetLayerWeightByName(Animator animator, string layerName, float weight)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex != -1)
        {
            animator.SetLayerWeight(layerIndex, weight);
        }
        else
        {
            Debug.LogWarning($"���C���[�� '{layerName}' ��������܂���ł���");
        }
    }

    /// <summary>
    /// Animation�����̈ʒu����Đ�����
    /// </summary>
    public static void PlayAnimationOnLayer(Animator animator, string animationName, string layerName, float normalizedTime)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex >= 0) // ���C���[�����݂���ꍇ�̂ݎ��s
        {
            animator.Play(animationName, layerIndex, normalizedTime);
        }
        else
        {
            Debug.LogWarning($"���C���[ '{layerName}' ��������܂���ł����B");
        }
    }

    /// <summary>
    /// ���݂̃A�j���[�V�����̍Đ��ʒu���擾
    /// </summary>
    public static float GetCurrentAnimationProgress(Animator animator, string layerName)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex >= 0)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.normalizedTime;
        }
        return 0f;
    }
}
