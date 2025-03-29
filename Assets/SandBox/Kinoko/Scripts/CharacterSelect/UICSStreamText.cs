using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UICSStreamText : MonoBehaviour
{
    [SerializeField] private float _endPosition;
    [SerializeField] private float _takeTime;

    public async UniTask StreamingText(CancellationToken token)
    {
        Vector2 firstPos = GetComponent<RectTransform>().anchoredPosition;
        try
        {
            await GetComponent<RectTransform>().DOAnchorPosX(_endPosition, _takeTime).SetEase(Ease.Linear).ToUniTask(cancellationToken: token);
        }
        catch 
        {
            return;
        }
        GameObject obj = null;
        try
        {

            obj = Instantiate(gameObject,firstPos,Quaternion.identity);
        }
        catch 
        {
            return;
        }
        obj?.transform.SetParent(transform.parent,false);
        try
        {
            obj?.GetComponent<UICSStreamText>().StreamingText(token).Forget();
        }
        catch{}
        Destroy(gameObject);
    }
}
