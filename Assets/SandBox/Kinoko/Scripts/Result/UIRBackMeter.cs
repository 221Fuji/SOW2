using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UIRBackMeter : UIPersonalAct
{
    private CancellationTokenSource _cts = new CancellationTokenSource();
    [SerializeField]private float _outXPos;
    [SerializeField]private float _insideXPos;
    public async UniTask InstantiateObj(GameObject obj)
    {
        Debug.Log("Instance");
        GetComponent<RectTransform>().anchoredPosition = new Vector2(_outXPos, obj.GetComponent<RectTransform>().anchoredPosition.y);
        transform.SetParent(obj.transform, false);

        try
        {
            await GetComponent<RectTransform>().DOAnchorPosX(_insideXPos, 0.25f).SetEase(Ease.OutBounce).ToUniTask(cancellationToken: _cts.Token);
        }
        catch { }
    }
    
    public async UniTask ReturnObj()
    {
        try 
        {
            Debug.Log("Cancell");
            await GetComponent<RectTransform>().DOAnchorPosX(_outXPos, 0.1f).ToUniTask(cancellationToken: _cts.Token);
        }
        catch {
            
        }
    }

    private void OnDestroy()
    {
        _cts.Cancel();
    }
}
