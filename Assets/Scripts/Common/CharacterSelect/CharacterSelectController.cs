using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�����ɂ�����̂͊��R�s�ł���̑�փN���X�����
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
