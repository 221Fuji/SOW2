using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSYazirusi : MonoBehaviour
{
    public void InstanceYazirusi(GameObject _ob)
    {
        this.transform.position = new Vector2(_ob.transform.position.x + 211,_ob.transform.position.y+3);
    }
}
