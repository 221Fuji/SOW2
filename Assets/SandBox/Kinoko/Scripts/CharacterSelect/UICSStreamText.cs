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
    private bool _isDestroy;
    private void OnDestroy()
    {
        _isDestroy = true;
    }

    public async UniTask StreamingText(CancellationToken token)
    {
        Vector2 firstPos = GetComponent<RectTransform>().anchoredPosition;
        try
        {
            await GetComponent<RectTransform>().DOAnchorPosX(_endPosition, _takeTime).SetEase(Ease.Linear).ToUniTask(cancellationToken: token);
        }
        catch { }

        if (_isDestroy) return;
        GameObject obj = Instantiate(gameObject,firstPos,Quaternion.identity);
        obj.transform.SetParent(transform.parent,false);
        try
        {
            obj.GetComponent<UICSStreamText>().StreamingText(token).Forget();
        }
        catch{}
        Destroy(gameObject);
    }
}
