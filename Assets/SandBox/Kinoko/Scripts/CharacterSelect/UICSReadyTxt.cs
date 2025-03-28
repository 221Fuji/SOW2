using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class UICSReadyTxt : UIPersonalAct
{
    private Color _colorBody;
    private Color _colorFlame;
    private RectTransform _sizeBody;
    private RectTransform _sizeFlame;
    [SerializeField] GameObject ReadyFlame;
    public void ReadyDirection(CancellationToken token)
    {
        _colorBody = GetComponent<TextMeshProUGUI>().color;
        _colorFlame = ReadyFlame.GetComponent<TextMeshProUGUI>().color;
        _sizeBody = GetComponent<RectTransform>();
        _sizeFlame = ReadyFlame.GetComponent<RectTransform>();

        var sequence = DOTween.Sequence();
        sequence.Append(ReadyFlame.GetComponent<TextMeshProUGUI>().DOFade(1, 0.05f).SetEase(Ease.InCubic))
                    .Append(GetComponent<TextMeshProUGUI>().DOFade(1, 0.05f).SetEase(Ease.InCubic))
                    .ToUniTask(cancellationToken: token).Forget();

        GetComponent<RectTransform>().DOScale(new Vector2(0.9f,0.9f),0.1f).SetEase(Ease.Linear)
            .ToUniTask(cancellationToken: token).Forget();

        
        var sequence2 = DOTween.Sequence();
        sequence2.Append(GetComponent<RectTransform>().DOScale(new Vector2(1.1f, 1.1f), 0.1f))
                .Append(ReadyFlame.GetComponent<RectTransform>().DOScale(new Vector2(1.2f, 1.2f), 0.1f))
                .Join(ReadyFlame.GetComponent<TextMeshProUGUI>().DOFade(0, 0.1f))
                .ToUniTask(cancellationToken: token).Forget();
    }

    public void ResetUI()
    {
        GetComponent<RectTransform>().localScale = _sizeBody.localScale;
        ReadyFlame.GetComponent<RectTransform>().localScale = _sizeFlame.localScale;
        GetComponent<TextMeshProUGUI>().color = _colorBody;
        ReadyFlame.GetComponent<TextMeshProUGUI>().color = _colorFlame;
    }
}
