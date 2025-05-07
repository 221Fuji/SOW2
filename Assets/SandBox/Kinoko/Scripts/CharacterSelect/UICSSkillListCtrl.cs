using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UICSSkillListCtrl : UIMovingCtrl
{
    private int _nowPos = -1;
    [SerializeField] private CharacterDataBase _database;
    [SerializeField] private GameObject _skillboxPrefab;
    [SerializeField] private GameObject _skillboxBack;
    protected override void Awake()
    {
        FirstCreating(0);
    }

    public void FirstCreating(int characterIndex)
    {
        foreach(CmdListBox clb in _database.CharacterDataList[characterIndex].CmdListBoxs)
        {
            CreateSkillBox(clb);
        }
    }

    public void CreateSkillBox(CmdListBox source)
    {
        GameObject sBoxObj = Instantiate(_skillboxPrefab, new Vector2(9, -55), Quaternion.identity);
        sBoxObj.transform.SetParent(_skillboxBack.transform, false);
        UICSSkillBox sBox = sBoxObj.GetComponent<UICSSkillBox>();
        sBox.SetData(source);
        sBoxObj.SetActive(false);
    }

    
}
