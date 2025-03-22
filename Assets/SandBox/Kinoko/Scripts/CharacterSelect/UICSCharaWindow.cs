using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class UICSCharaWindow : UIPersonalAct
{
    [SerializeField] private int _windowTurn = -1;
    [SerializeField] private GameObject _backPanel;
    public CharacterData Characterdata {get; private set;}
    private bool _movingExceptionFlag;
    private int _selectedPlayer = 0;
    private GameObject _instancedLine = null;
    //以下このウィンドウに何が割り当てられるのかを指定して表示する用の要素を準備するメソッド

    public void SetCharacterData(GameObject ob)
    {
        ob.TryGetComponent<UICSMovingCtrl>(out var movingCtrlClass);
        List<CharacterData> characterDatas = movingCtrlClass?.DataBase.CharacterDataList;

        if(_movingExceptionFlag || characterDatas.Count < _windowTurn)
        {
            _movingExceptionFlag = true;
            return;
        }
        Characterdata = characterDatas[_windowTurn - 1];

        GameObject windowFace = Characterdata.WindowFace;
        if(windowFace == null) return;
        Vector2 backPos = _backPanel.GetComponent<RectTransform>().anchoredPosition;
        GameObject InstantiatedFase = Instantiate(windowFace, 
                                                new Vector2(backPos.x + windowFace.GetComponent<RectTransform>().anchoredPosition.x
                                                            ,backPos.y + windowFace.GetComponent<RectTransform>().anchoredPosition.y),Quaternion.identity);
        InstantiatedFase.transform.SetParent(_backPanel.transform,false);
        
    }

    public override bool MovingException(GameObject ob)
    {
        return _movingExceptionFlag;
    }

    public override void FocusedAction(GameObject ob)
    {
        ob.TryGetComponent<UICSMovingCtrl>(out var movingctrlClass);
        //1が1Pで2が2P
        int playernum = 0;
        try
        {
            playernum = (int)movingctrlClass?.PlayerNum;
        }
        catch(Exception e)
        {
            Debug.Log("playerが識別できません>>" + e);
            return;
        }
        CharacterDataBase characterDatabase = movingctrlClass.DataBase;
        TextMeshProUGUI jpTxt = movingctrlClass?.CharacterNameJField;
        TextMeshProUGUI enTxt = movingctrlClass?.CharacterNameEField;
        


        if(characterDatabase.CharacterDataList.Count < _windowTurn)
        {
            _movingExceptionFlag = true;
            return;
        }

        jpTxt.text = characterDatabase.CharacterDataList[_windowTurn - 1].CharacterNameJ;
        enTxt.text = characterDatabase.CharacterDataList[_windowTurn - 1].CharacterNameE;

        _selectedPlayer += playernum;
        if(_selectedPlayer == 0 || _selectedPlayer > 3)
        {
            Debug.Log("_selectedPlayerがオーバーしています>>" + _selectedPlayer);
            return;
        }
        
        if(_instancedLine)
        {
            Destroy(_instancedLine);
            _instancedLine = null;
        }

        GameObject instanceLine = null;
        //windowが下列にある場合に非対応、時が来たら書き足す
        switch(_selectedPlayer)
        {
            case 1:
                instanceLine = movingctrlClass.ReturnCharacterOutFrames()._1PFrameTop;
                break;
            case 2:
                instanceLine = movingctrlClass.ReturnCharacterOutFrames()._2PFrameTop;
                break;
            case 3:
                instanceLine = movingctrlClass.ReturnCharacterOutFrames()._MixFrameTop;
                break;
        }
        //Debug.Log("selectedPlayer>>" + _selectedPlayer + "このオブジェクト"+ _windowTurn);

        Vector2 instanceLineRect = instanceLine.GetComponent<RectTransform>().anchoredPosition;
        _instancedLine = Instantiate(instanceLine,new Vector2(instanceLineRect.x
                                                            ,instanceLineRect.y),Quaternion.identity);
        _instancedLine.transform.SetParent(transform,false);
    }

    public override void SeparateAction(GameObject ob)
    {
        ob.TryGetComponent<UICSMovingCtrl>(out var movingCtrlClass);
        int playerNum = 0;
        try
        {
            playerNum = (int)movingCtrlClass?.PlayerNum;
        }
        catch(Exception e)
        {
            Debug.Log("playerが識別できません>>" + e);
            return;
        }

        _selectedPlayer -= playerNum;
        if(_selectedPlayer < 0 || _selectedPlayer >= 3)
        {
            Debug.Log("_selectedPlayerがオーバーしています>>" + _selectedPlayer);
            return;
        }
        Destroy(_instancedLine);
        _instancedLine = null;
        
        GameObject instanceLine = null;
        switch(_selectedPlayer)
        {
            case 0:
                break;
            case 1:
                instanceLine = movingCtrlClass.ReturnCharacterOutFrames()._1PFrameTop;
                break;
            case 2:
                instanceLine = movingCtrlClass.ReturnCharacterOutFrames()._2PFrameTop;
                break;
        }
        if(instanceLine)
        {
            Vector2 instanceLineRect = instanceLine.GetComponent<RectTransform>().anchoredPosition;
            _instancedLine = Instantiate(instanceLine,new Vector2(instanceLineRect.x
                                                                ,instanceLineRect.y),Quaternion.identity);
            _instancedLine.transform.SetParent(transform,false);
        }


    }
}
