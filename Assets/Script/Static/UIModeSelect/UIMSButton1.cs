using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIMSButton1 : UIPersonalAct
{
    [SerializeField] UIMSYazirusi YazirusiOb;
    public override void FocusedAction()
    {
        YazirusiOb.InstanceYazirusi(this.transform.gameObject);
    }
}
