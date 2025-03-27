using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UICSReadyTxt : UIPersonalAct
{
    [SerializeField] GameObject ReadyFlame;
    public async UniTask ReadyDirection(CancellationToken token)
    {
        await ReadyFlame.GetComponent<TextMeshProUGUI>().DOFade(1,0.05f).SetEase(Ease.InCubic).ToUniTask(cancellationToken: token);

        GetComponent<TextMeshProUGUI>().DOFade(1,0.05f).SetEase(Ease.InCubic).ToUniTask(cancellationToken: token);

    }
}
