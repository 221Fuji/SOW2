using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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

    /// <summary>
    /// TimeScaleの影響を受けないアニメーションを再生する
    /// </summary>
    public static void PlayUnscaledAnimation(Animator animator, AnimationClip animationClip)
    {
        PlayableGraph playableGraph;

        // PlayableGraphを作成し、名前を "UnscaledAnimation" に設定
        playableGraph = PlayableGraph.Create("UnscaledAnimation");

        // PlayableGraphの更新モードを UnscaledGameTime に設定 (Time.timeScale の影響を受けない)
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);

        // PlayableGraph に AnimationPlayableOutput を作成
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

        // AnimationClipPlayable を作成し、再生したい AnimationClip を設定
        var animationPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

        // PlayableOutput の出力先に animationPlayable を設定
        playableOutput.SetSourcePlayable(animationPlayable);

        // PlayableGraph を再生
        playableGraph.Play();
    }
}
