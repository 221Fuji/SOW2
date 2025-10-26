using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UramiMeter : UniqueResourceUI
{
    private Teddy _teddy;
    private Slider _uramiMeter;
    public UramiMeter(Teddy teddy, Slider uramiMeter)
    {
        _teddy = teddy;
        _uramiMeter = uramiMeter;
    }

    public override void UpdateUniueResourceUI()
    {
        _uramiMeter.value = _teddy.CurrentUramiResource / _teddy.UramiMaxResource;
    }
}
