using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 演出のアニメーションから呼ばれる用
/// </summary>
public class TitleSoundCaller : MonoBehaviour
{
    public void PlayBreackGlass()
    {
        SoundManager.I.SystemSEPlayer.PlaySE(2);
    }

    public void PlayTitleSE()
    {
        SoundManager.I.SystemSEPlayer.PlaySE(1);
    }
}
