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
        await GetComponent<RectTransform>().DOAnchorPosX(_endPosition,_takeTime).SetEase(Ease.Linear).ToUniTask(cancellationToken: token);
        GameObject obj = Instantiate(gameObject,firstPos,Quaternion.identity);
        obj.transform.SetParent(transform.parent,false);
        obj.GetComponent<UICSStreamText>().StreamingText(token);
        Destroy(gameObject);
    }
}
