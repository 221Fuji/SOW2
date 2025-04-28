using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIField : MonoBehaviour
{
    [SerializeField] private Func _funcType;
    [SerializeField] GameObject buttonSouth;
    [SerializeField] GameObject buttonEast;
    [SerializeField] GameObject KeyboardJ;
    private void Awake()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        foreach(var actionMap in input.actions.actionMaps)
        {
            //なんのストリームか
            if (actionMap.name != "Other") continue;
            Debug.Log(actionMap.name);
            foreach(var action in actionMap.actions)
            {
                //なんのアクションか
                if (action.name != _funcType.ToString()) continue;

                 foreach(var binding in action.bindings)
                {
                    Debug.Log($"    Binding: {binding.path} ({binding.effectivePath})");
                }
            }
        }

        Debug.Log(GameManager.Player1Device);
    }
}

public enum Func : byte
{
    Accept,Cancell
}
