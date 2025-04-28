using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class LancerCPU : CPUAgent
{
    private Lancer _lancer;

    public override void SetCAandCS()
    {
        base.SetCAandCS();
        if(SelfCA is Lancer lancer)
        {
            _lancer = lancer;
        }
        else
        {
            Debug.LogError("ƒLƒƒƒ‰‚ªˆê’v‚µ‚Ü‚¹‚ñ");
        }
    }

    public override void DiscreteUniqueActionMask(IDiscreteActionMask actionMask)
    {
        if(_lancer.OnGround)
        {
            if (_lancer.CanNormalMove) actionMask.SetActionEnabled(0, 4, true);
        }
        else
        {
            if(_lancer.CanJumpMove) actionMask.SetActionEnabled(0, 4, true);
        }
        if (_lancer.CanSpecialMove1) actionMask.SetActionEnabled(0, 5, true);
        if (_lancer.CanSpecialMove2) actionMask.SetActionEnabled(0, 6, true);
        if (_lancer.CanUltimate) actionMask.SetActionEnabled(0, 7, true);
    }
}
