using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UIRAraw : UIPersonalAct
{
    [SerializeField] private Vector2 _instantiatePos;
    public void InstantiateObj(GameObject obj,CancellationToken token)
    {
        GetComponent<RectTransform>().DOAnchorPos(new Vector2(GetComponent<RectTransform>().anchoredPosition.x + _instantiatePos.x
                                                            , obj.GetComponent<RectTransform>().anchoredPosition.y + _instantiatePos.y), 0.2f)
                                                              .SetEase(Ease.OutBack).ToUniTask(cancellationToken: token);
    }
}
