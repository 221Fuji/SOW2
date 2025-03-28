using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ここにあるものは完コピでこれの代替クラスを作る
public class CharacterSelectController : MonoBehaviour
{
    public bool Selected { get; private set; }

    //決定ボタン
    public void Accept()
    {
        Selected = true;
    }

    public void Cancel()
    {
        Selected = false;
    }
}
