using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIMSText : UIPersonalAct
{
    public virtual void StringToText(string str)
    {
        GetComponent<TextMeshProUGUI>().text = str;
    }
}
