using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingGeneralSEPlayer : SEPlayer
{
    public FightingGeneralSEPlayer(SEResources seResources) : base(seResources)
    {
        
    }

    public void PlayHurtSE(float damage)
    {
        float intensity = Mathf.Clamp01(damage / 40);

        _instList[0].setParameterByName("Intensity", intensity);
        PlaySE(0);
    }

    public void PlayGuardSE(bool isHeavy)
    {
        int paraValue = isHeavy ? 1 : 0;
        _instList[1].setParameterByName("IsHeavy", paraValue);
        PlaySE(1);
    }

    public void PlayComboSE(int combo)
    {
        float comboValue = Mathf.Clamp01((float)(combo - 1)/ 4);
        _instList[2].setParameterByName("Combo", comboValue);
        PlaySE(2);
    }
}
