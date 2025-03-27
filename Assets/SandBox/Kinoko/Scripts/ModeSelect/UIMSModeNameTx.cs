using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIMSModeNameTx : UIMSText
{
    [SerializeField] private GameObject _childObject1;
    [SerializeField] private GameObject _childObject2;
    [SerializeField] private GameObject _childObject3;

    void EmptyText(List<GameObject> obs)
    {
        foreach(GameObject ob in obs)
        {
            ob.GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    public void ResetPosition()
    {
        GameObject motherObject = transform.gameObject;
        motherObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(32.5f,70.9f,0);
        _childObject1.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);
        _childObject2.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);
        _childObject3.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);

        EmptyText(new List<GameObject>(){_childObject1,_childObject2,_childObject3});
        
    }

    public Sequence MovingDirection()
    {

        var sequence = DOTween.Sequence();
        sequence.Append(GetComponent<RectTransform>().DOAnchorPos(new Vector3(7.8f,12.5f,0),0.7f).SetEase(Ease.OutCubic));
        return sequence;
    }

    public Sequence MovingDirectionChild()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(_childObject1.GetComponent<RectTransform>().DOAnchorPos(new Vector3(0,98,0),0.5f).SetEase(Ease.OutCubic))
        .Append(_childObject2.GetComponent<RectTransform>().DOAnchorPosY(64,0.4f).SetEase(Ease.OutCubic))
        .Append(_childObject3.GetComponent<RectTransform>().DOAnchorPosY(64,0.4f).SetEase(Ease.OutCubic));
        return sequence;
    }

    public async UniTask FlashText(string str,CancellationToken token)
    {
        List<GameObject> objs = new List<GameObject>(){_childObject1,_childObject2,_childObject3};
        char[] chars = str.ToCharArray();
        for(int i = 1; i <= str.Length; i++)
        {
            try
            {
                await UniTask.WaitForSeconds(0.1f,cancellationToken: token);
                foreach(GameObject obj in objs) obj.GetComponent<TextMeshProUGUI>().text += chars[i - 1];
            }
            catch
            {
                break;
            }
        }
    }
}
