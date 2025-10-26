using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogMeter : UniqueResourceUI
{
    private ViolaCloud _cloud;
    private Slider _fogMeter;

    public FogMeter(ViolaCloud cloud, Slider fogMeter)
    {
        _cloud = cloud;
        _fogMeter = fogMeter;
    }

    public override void UpdateUniueResourceUI()
    {
        _fogMeter.value = _cloud.CurrentFogResource / _cloud.FogMaxResource;
    }
}
