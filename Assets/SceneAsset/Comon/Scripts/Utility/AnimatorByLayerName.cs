using UnityEngine;

public static class AnimatorByLayerName
{
    /// <summary>
    /// AnimatorのSetlayerWeightをlayerの名前で呼び出す
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
            Debug.LogWarning($"レイヤー名 '{layerName}' が見つかりませんでした");
        }
    }

    /// <summary>
    /// Animationを特定の位置から再生する
    /// </summary>
    public static void PlayAnimationOnLayer(Animator animator, string animationName, string layerName, float normalizedTime)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex >= 0) // レイヤーが存在する場合のみ実行
        {
            animator.Play(animationName, layerIndex, normalizedTime);
        }
        else
        {
            Debug.LogWarning($"レイヤー '{layerName}' が見つかりませんでした。");
        }
    }

    /// <summary>
    /// 現在のアニメーションの再生位置を取得
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
