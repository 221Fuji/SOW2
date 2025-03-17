using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class UIMSYazirusi : UIPersonalAct
{
    public override void InstanceObject(GameObject _ob)
    {
        GameObject parent = _ob.transform.parent.gameObject;
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(parent.GetComponent<RectTransform>().anchoredPosition.x + 211,parent.GetComponent<RectTransform>().anchoredPosition.y);
    }
}
