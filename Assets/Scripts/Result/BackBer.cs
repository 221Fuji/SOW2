using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackBer : UIPersonalAct
{
    [SerializeField] private float _outX;
    [SerializeField] private float _insideX;
    public void InstantiateObj(GameObject obj)
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(obj.GetComponent<RectTransform>().anchoredPosition.x + _outX, obj.GetComponent<RectTransform>().anchoredPosition.y);
    }
}
