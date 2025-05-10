using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.HID;
using Unity.VisualScripting;
using UnityEngine.InputSystem.XInput;

public class UIField : MonoBehaviour
{
    [SerializeField] private UIActionList _funcType;
    [Header("何Pの情報を表示するか")]
    [SerializeField] private PlayerNum _playerNum;
    private GameObject _uiObj = null;
    private string _deviceStr = "";
    private string[] _otherTypes = new string[3]{ "Accept", "Cancell", "Detail" };
    private enum PlayerNum
    {
        OneP,
        TwoP,
    }

    private Dictionary<string, GameObject> _uIIconPacks = new Dictionary<string, GameObject> 
        { {"buttonSouth",null}, {"buttonEast",null},{"buttonNorth",null},{"buttonWest",null},{"Keyboardj",null},{"Keyboardf",null},{"Keyboardk",null},{ "Keyboardu",null},{"Keyboardl",null} };

    private void Awake()
    {
        ChangedIcon();
    }

    public void ChangedIcon()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        //デバイスの情報
        InputDevice device = null;
        if ((int)_playerNum + 1 == 1) device = GameManager.Player1Device;
        else if ((int)_playerNum + 1 == 2) device = GameManager.Player2Device;
        if (device is null) return;
        _deviceStr = DeviceToString(device);

        _uiObj = null;
        foreach (var actionMap in input.actions.actionMaps)
        {
            //なんのストリームか
            if (_otherTypes.Contains(_funcType.ToString()))
            {
                if (actionMap.name != "Other") continue;
            }
            else
            {
                if (actionMap.name != "Fighting") continue;
            }
            foreach (var action in actionMap.actions)
            {
                //なんのアクションか
                if (action.name != _funcType.ToString())
                {
                    continue;
                }

                foreach (var binding in action.bindings)
                {
                    if (!(binding.path).Contains(_deviceStr))
                    {
                        continue;
                    }

                    //pathの整形,strs[1]になんのボタンかが入る
                    string[] strs = binding.path.Split("/");
                    GameObject obj = AccessIconPack(strs[1]);

                    if (obj == null)
                    {
                        Debug.Log("一致するボタンのUIがありません");
                        break;
                    }
                    _uiObj = obj;
                    break;
                }
            }
        }
        if (_uiObj == null) return;
        //フィールド内にあるボタンガイドを全削除する
        ClearField();
        GameObject instantiated = Instantiate(_uiObj, new Vector2(0, 0), Quaternion.identity);
        instantiated.transform.SetParent(transform, false);
    }

    public void ChangeActionType(UIActionList action)
    {
        _funcType = action;
    }

    private GameObject AccessIconPack(string str)
    {
        string str2 = "";
        foreach (var pair in _uIIconPacks)
        {
            if(pair.Key == str || pair.Key.Contains(str))
            {
                str2 = pair.Key;
            } 
        }

        if(str2 == "") return null;
        GameObject obj = _uIIconPacks[str2];
        if (obj == null) obj = (GameObject)Resources.Load("Prefab/" + "UIICONs/" + str2);
        return obj;
    }

    private string DeviceToString(InputDevice device)
    {
        if (device is Keyboard) return "Keyboard";
        else if (device is Gamepad) return "Gamepad";
        else if (device is XInputControllerWindows) return "XInputControllerWindows";
        return "";
    }

    private void ClearField()
    {
        Transform children = GetComponentInChildren<Transform>();
        if (children.childCount == 0)
        {
            return;
        }
        foreach(Transform ob in children)
        {
            Destroy(ob.gameObject);
        }
    }
}

public enum UIActionList : byte
{
    Accept, Cancell, Detail, NormalMove, SpecialMove1, SpecialMove2, Ultimate,
}