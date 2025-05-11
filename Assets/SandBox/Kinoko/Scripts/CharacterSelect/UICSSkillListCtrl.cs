using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UICSSkillListCtrl : UIMovingCtrl
{
    [SerializeField] private int _playerNum;
    [SerializeField] private CharacterDataBase _database;
    [SerializeField] private GameObject _skillboxPrefab;
    [SerializeField] private GameObject _skillboxBack;
    [SerializeField] private TextMeshProUGUI _maxPages;
    [SerializeField] private TextMeshProUGUI _nowPages;
    [SerializeField] private UICSMovingCtrl _movingCtrl;
    public GameObject SkillboxBack { get { return _skillboxBack; } }

    public UnityAction<UIMovingCtrl,int> SwitchDelegate { get; set; }

    private List<CmdListBox> _cmdListBoxs;
    protected override void Awake()
    {
        _movingCtrl.SwitchAdmin += FirstCreating;
        foreach (UIField i in _uiFields)
        {
            UiChanging += i.ChangedIcon;
        }
    }

    public void FirstCreating(int characterIndex)
    {
        _skillboxBack.SetActive(true);

        DeleteSkillBox();
        List<UICSSkillBox> sboxs = new List<UICSSkillBox>();
        Debug.Log(_database.CharacterDataList[characterIndex].name + ">>>" + characterIndex);
        foreach(CmdListBox clb in _database.CharacterDataList[characterIndex].CmdListBoxes)
        {
            Debug.Log("create");
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
        _maxPages.text = ReturnArrayLength().ToString();
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

    public void DeleteSkillBox()
    {
        foreach (CmdListBox box in _cmdListBoxs)
        {
            Destroy(box);
        }
        _cmdListBoxs.Clear();
    }

    public void SwitchtoOtherCtrler()
    {

        SwitchDelegate.Invoke(_movingCtrl, _playerNum);
    }

    public override void DesignatedForcus(Vector2 arrayPos)
    {
        base.DesignatedForcus(arrayPos);
        _nowPages.text = (ReturnArrayLength() - Forcus.y).ToString();
    }
    public override void ForcusUp()
    {
        base.ForcusUp();
        _nowPages.text = (ReturnArrayLength() - Forcus.y).ToString();
    }
    public override void ForcusDown()
    {
        base.ForcusDown();
        _nowPages.text = (ReturnArrayLength() - Forcus.y).ToString();
    }
}
