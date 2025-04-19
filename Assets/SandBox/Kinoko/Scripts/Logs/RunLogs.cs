using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RunLogs : MonoBehaviour
{
    [SerializeField] private TextAsset _txtFileP1 = null;
    [SerializeField] private TextAsset _txtFileP2 = null;
    
    private bool _isRunning = false;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void FixedUpdate()
    {
        if (!_isRunning) return;
        
    }

    public void SwitchFlag()
    {
        if(!(_txtFileP1 is null) && !(_txtFileP2 is null)) return;
        if (_isRunning) _isRunning = false;
        else _isRunning = true;
    }
}
