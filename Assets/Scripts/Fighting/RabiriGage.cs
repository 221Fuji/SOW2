using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RabiriGage : UniqueResourceUI
{
    private const float _threePixelValue = 1.219123f;
    private Rabiri _rabiri;
    private GameObject _rabiriGage;
    private List<Rabiri.EStackMove> _stackMoves;
    private List<GameObject> _lampList;

    public RabiriGage(Rabiri rabiri, GameObject gage)
    {
        _rabiri = rabiri;
        _rabiriGage = gage;
        _stackMoves = new List<Rabiri.EStackMove>();
        _lampList = new List<GameObject>();
        _rabiri.RabiriGage = _rabiriGage.GetComponent<Animator>();
    }

    public override void UpdateUniueResourceUI()
    {
        // ì‡óeÇ…ïœâªÇ™Ç†ÇÍÇŒ
        if (_stackMoves.SequenceEqual(_rabiri.StackMoves)) return;
        _stackMoves = new List<Rabiri.EStackMove>(_rabiri.StackMoves);
        Debug.Log("ÇøÇÒÇ€"+ _stackMoves.Count());

        // Ç∑Ç◊ÇƒçÌèú
        if (_lampList.Count() > 0)
        {
            for (int i = _lampList.Count() - 1; i >= 0; i--)
            {
                _rabiri.DestroyLamp(_lampList[i]);
                _lampList[i] = null;
                _lampList.RemoveAt(i);
            }
        }

        // ëSÇƒê∂ê¨
        for (int i = 0; i < _stackMoves.Count(); i++)
        {
            _lampList.Add(InstantiateLamp(i, _stackMoves[i]));
        }
    }

    private GameObject InstantiateLamp(int index, Rabiri.EStackMove move)
    {
        int lampPrefabIndex = (int)move;

        Vector2 lampPos = new Vector2(index * _threePixelValue, 0);
        RectTransform lamp = _rabiri.InstantiateLamp(lampPrefabIndex);
        lamp.SetParent(_rabiriGage.transform, false);
        if(_rabiri.PlayerNum == 1)
        {
            lamp.anchoredPosition += lampPos;
        }
        else
        {
            lamp.anchoredPosition -= lampPos;
        }       

        return lamp.gameObject;
    }
}
