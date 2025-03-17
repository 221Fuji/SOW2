using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UIMSFloorUnder : UIMSFloor
{
    public Sequence Moving()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(GetComponent<RectTransform>().DOAnchorPosY(GetComponent<RectTransform>().anchoredPosition.y+5,_moveSpeed).SetLoops(-1,LoopType.Yoyo))
        .Join(transform.DOScale(new Vector3(transform.localScale.x * 0.95f,transform.localScale.y * 0.95f,transform.localScale.z * 0.95f), _moveSpeed).SetLoops(-1,LoopType.Yoyo));
        return sequence;
    }

    public override void InstanceObject(GameObject ob)
    {
        GameObject parent = ob.transform.parent.gameObject;
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(parent.GetComponent<RectTransform>().anchoredPosition.x,parent.GetComponent<RectTransform>().anchoredPosition.y - 55.5f);
    }


}
