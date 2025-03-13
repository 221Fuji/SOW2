using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    public bool Selected { get; private set; }

    //����{�^��
    public void Accept()
    {
        Selected = true;
    }

    public void Cancel()
    {
        Selected = false;
    }
}
