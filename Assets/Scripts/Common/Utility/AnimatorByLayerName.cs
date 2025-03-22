using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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

    /// <summary>
    /// TimeScale�̉e�����󂯂Ȃ��A�j���[�V�������Đ�����
    /// </summary>
    public static void PlayUnscaledAnimation(Animator animator, AnimationClip animationClip)
    {
        PlayableGraph playableGraph;

        // PlayableGraph���쐬���A���O�� "UnscaledAnimation" �ɐݒ�
        playableGraph = PlayableGraph.Create("UnscaledAnimation");

        // PlayableGraph�̍X�V���[�h�� UnscaledGameTime �ɐݒ� (Time.timeScale �̉e�����󂯂Ȃ�)
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);

        // PlayableGraph �� AnimationPlayableOutput ���쐬
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

        // AnimationClipPlayable ���쐬���A�Đ������� AnimationClip ��ݒ�
        var animationPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

        // PlayableOutput �̏o�͐�� animationPlayable ��ݒ�
        playableOutput.SetSourcePlayable(animationPlayable);

        // PlayableGraph ���Đ�
        playableGraph.Play();
    }
}
