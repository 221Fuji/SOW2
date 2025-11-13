using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : DamageField<ViolaCloud>
{
    [SerializeField] private float _consumptionValue;
    protected override void FightingUpdate()
    {
        base.FightingUpdate();
        //ÉäÉ\Å[ÉXå∏è≠
        _self.ConsumptionFogResource(_consumptionValue);
    }
}
