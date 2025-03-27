using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class UICSFigureBox : UIPersonalAct
{
    public async UniTask InstanceFigure(int playerNum ,GameObject figure,CancellationToken token)
    {
        foreach(Transform figureChild in GetComponentInChildren<Transform>())
        {
            Destroy(figureChild.gameObject);
        }

        Vector2 figureRect = figure.GetComponent<RectTransform>().anchoredPosition;
        if(playerNum == 2) figureRect.x = figureRect.x * -1;
        Vector2 thisRect = GetComponent<RectTransform>().anchoredPosition;
        GameObject ob = Instantiate(figure,new Vector2(thisRect.x,figureRect.y),Quaternion.identity);
        ob.transform.SetParent(transform,false);

        var sequence = DOTween.Sequence();
        await sequence.Append(ob.GetComponent<RectTransform>().DOAnchorPos(new Vector3(figureRect.x,figureRect.y,0),0.5f))
        .Join(ob.GetComponent<UnityEngine.UI.Image>().DOFade(endValue: 1, duration:0.5f)).ToUniTask(cancellationToken: token);
    }
}
