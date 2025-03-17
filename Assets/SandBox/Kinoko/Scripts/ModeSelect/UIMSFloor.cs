using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIMSFloor : UIPersonalAct
{
    protected float _moveSpeed = 1.3f;
    public Sequence FadeIn()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(this.GetComponent<UnityEngine.UI.Image>().DOFade(endValue: 1, duration: 1.5f));

        return sequence;
    }
}
