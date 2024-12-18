using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LifeModel
{
    public event Action<float> OnLifeChanged;
    public event Action OnLifeExhausted;
    public event Action<int> OnMaxLifeChanged;

    private LifeScriptableObject lifeSO;

    private float life;
    public float Life 
    { 
        get { return life; }
        set
        {
            life = Mathf.Clamp(value, 0, MaxLife);
            if (life <= 0)
            {
                OnLifeExhausted?.Invoke();
            }
            OnLifeChanged?.Invoke(life);
        }
    }
    private int maxLife;
    public int MaxLife 
    {    
        get { return maxLife; }
        set
        {
            maxLife = value;
            Life = Mathf.Min(Life, MaxLife);
            OnMaxLifeChanged?.Invoke(MaxLife);
        }
    }

    public LifeModel() : this(ScriptableObject.CreateInstance<LifeScriptableObject>().Default()) { }
    public LifeModel(LifeScriptableObject _lifeSO)
    {
        lifeSO = _lifeSO;
        maxLife = _lifeSO.MaxLife;
        life = maxLife;
    }

    public void Reset()
    {
        OnLifeChanged = null;
        OnLifeExhausted = null;
        OnMaxLifeChanged = null;
    }
    public float GetDecreaseRate(float elapsed)
    {
        int cumulativeTime = 0;
        for (int i = 0; i < lifeSO.LifeDecreaseRateCurveList.Count; i++)
        {
            var curEntry = lifeSO.LifeDecreaseRateCurveList[i];
            if (elapsed < cumulativeTime + curEntry.Duration)
            {
                return  (curEntry.EndPointValue - curEntry.StartPointValue) * curEntry.Curve.Evaluate((elapsed - cumulativeTime) / curEntry.Duration) + curEntry.StartPointValue;
            }
            cumulativeTime += curEntry.Duration;
        }
        return lifeSO.LifeDecreaseRateCurveList[lifeSO.LifeDecreaseRateCurveList.Count - 1].EndPointValue;
    }

    public void ProcessJudge(bool IsCorrect)
    {
        if(IsCorrect)
        {
            Life += lifeSO.OnSuccessGain;
        }
        else
        {
            Life -= lifeSO.OnFailDecrease;
        }
    }
}
