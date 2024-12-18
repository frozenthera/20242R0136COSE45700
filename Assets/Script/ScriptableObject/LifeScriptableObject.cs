using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LifeSO", menuName = "ScriptableObject/LifeModelInfo", order = 1)]
public class LifeScriptableObject : ScriptableObject
{
    public int MaxLife;
    public List<CurveEntry> LifeDecreaseRateCurveList;
    public int OnFailDecrease;
    public int OnSuccessGain;

    public LifeScriptableObject Default()
    {
        MaxLife = 100;
        LifeDecreaseRateCurveList = new()
        {
            new CurveEntry()
            {
                Curve = new AnimationCurve(new Keyframe[2]{new Keyframe(1f, 1f), new Keyframe(1f, 1f)}),
                Duration = 999,
                StartPointValue = 0,
                EndPointValue = 2, 
            }
        };
        return this;
    }
}

[Serializable]
public class CurveEntry
{
    public AnimationCurve Curve;
    public int Duration;
    public int StartPointValue;
    public int EndPointValue;
}
