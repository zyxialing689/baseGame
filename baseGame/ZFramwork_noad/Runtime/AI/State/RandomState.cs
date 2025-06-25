using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomState : AIState
{
    private int runIndex;
    public override void Awake()
    {
        runIndex = Random.Range(0, stateData.condAIStateData.Count);
    }

    public override AIStateData TryNextCond()
    {
        if (stateData.condAIStateData.Count > 0)
        {
            if (tryCond.GetTryCondition(stateData.condAIStateData[runIndex].condTypes, stateData.condAIStateData[runIndex]))
            {
                return stateData.condAIStateData[runIndex];
            }
        }


        return null;
    }

}
