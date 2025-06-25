using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetRootState : AIState
{
    public override void Awake()
    {

    }

    public override AIStateData TryNextCond()
    {
        return null;
    }

    public override bool TryRestCond()
    {
        return true;
    }
}
