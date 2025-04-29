using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class UIField : MonoBehaviour
{
    [SerializeField] private Func _funcType;
    [Header("何Pの情報を表示するか")]
    [SerializeField] private PlayerNum _playerNum;
    private GameObject _uiObj = null;
    private string _deviceStr = "";

    private enum Func : byte
    {
        Accept, Cancell
    }
    private enum PlayerNum
    {
        OneP,
        TwoP,
    }

    private Dictionary<string, GameObject> _uIIconPacks = new Dictionary<string, GameObject> 
        { {"buttonSouth",null}, {"buttonEast",null}, {"Keyboardj",null} };

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

        foreach (var actionMap in input.actions.actionMaps)
        {
            //なんのストリームか
            if (actionMap.name != "Other") continue;
            foreach (var action in actionMap.actions)
            {
                //なんのアクションか
                if (action.name != _funcType.ToString()) continue;

                foreach (var binding in action.bindings)
                {
                    //Debug.Log($"    Binding: {binding.path} ({binding.effectivePath})");
                    if (!(binding.path).Contains(_deviceStr)) continue;
                    //pathの整形,strs[1]になんのボタンかが入る
                    string[] strs = binding.path.Split("/");
                    Debug.Log(strs[1]);
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
        GameObject instantiated = Instantiate(_uiObj, new Vector2(0, 0), Quaternion.identity);
        instantiated.transform.SetParent(transform, false);
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
        return "";
    }
}