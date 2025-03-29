using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UIRArawBlue : UIPersonalAct
{
    [SerializeField] private Vector2 _instantiatePos;
    public void InstantiateObj(GameObject obj, CancellationToken token)
    {
        GetComponent<RectTransform>().DOAnchorPos(new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x + _instantiatePos.x
                                                            , obj.GetComponent<RectTransform>().anchoredPosition.y + _instantiatePos.y), 0.2f)
                                                              .SetEase(Ease.OutBack).ToUniTask(cancellationToken: token);
    }
}
