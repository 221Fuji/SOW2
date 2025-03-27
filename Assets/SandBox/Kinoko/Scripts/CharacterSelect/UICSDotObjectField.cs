using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICSDotObjectField : UIPersonalAct
{
    [SerializeField] private int playerNum;
    public void InstanceDot(GameObject obj)
    {
        foreach(Transform child in GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }

        Vector2 rectObj = obj.GetComponent<RectTransform>().anchoredPosition;
        if(playerNum == 2) rectObj.x = rectObj.x * -1;
        GameObject instanced = Instantiate(obj,new Vector2(rectObj.x,0),Quaternion.identity);
        instanced.transform.SetParent(transform,false);
        if(playerNum == 2)
        {
            instanced.transform.localRotation = Quaternion.Euler(0,180,0);
        }
    }
}
