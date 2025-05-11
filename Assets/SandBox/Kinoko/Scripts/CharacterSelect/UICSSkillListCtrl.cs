using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICSSkillListCtrl : UIMovingCtrl
{
    [SerializeField] private CharacterDataBase _database;
    [SerializeField] private GameObject _skillboxPrefab;
    [SerializeField] private GameObject _skillboxBack;
    [SerializeField] private TextMeshProUGUI _maxPages;
    [SerializeField] private TextMeshProUGUI _nowPages;
    public GameObject SkillboxBack { get { return _skillboxBack; } }

    protected override void Awake()
    {
        //FirstCreating(0);
    }

    public void FirstCreating(int characterIndex)
    {
        List<UICSSkillBox> sboxs = new List<UICSSkillBox>();
        foreach(CmdListBox clb in _database.CharacterDataList[characterIndex].CmdListBoxes)
        {
            sboxs.Add(CreateSkillBox(clb));
        }

        for (int i = sboxs.Count - 1; i >= 0; i--)
        {
            UICSSkillBox sbox = sboxs[i];
            if (_outMap.Count <= 0)
            {
                Making mk = new Making();
                mk.SetData(sbox);
                OutMap.Add(mk);
            }
            else
            {
                Making making = _outMap[0];
                making.SetData(sbox);
            }
        }
        try 
        {

        }
        catch
        {
            Debug.Log("");
        }
        _maxPages.text = (ReturnArrayLength() - 1).ToString();
        DesignatedForcus(new Vector2(0,sboxs.Count - 1));
    }

    public UICSSkillBox CreateSkillBox(CmdListBox source)
    {
        GameObject sBoxObj = Instantiate(_skillboxPrefab, new Vector2(10, -60), Quaternion.identity);
        sBoxObj.transform.SetParent(_skillboxBack.transform, false);
        UICSSkillBox sBox = sBoxObj.GetComponent<UICSSkillBox>();
        sBox.SetData(source);
        sBoxObj.SetActive(false);
        return sBox;
    }

    public override void DesignatedForcus(Vector2 arrayPos)
    {
        base.DesignatedForcus(arrayPos);
        _nowPages.text = Forcus.y.ToString();
    }
    public override void ForcusUp()
    {
        base.ForcusUp();
        _nowPages.text = Forcus.y.ToString();
    }
    public override void ForcusDown()
    {
        base.ForcusDown();
        _nowPages.text = Forcus.y.ToString();
    }
}
