using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LifeModel
{
    public event Action<float> OnLifeChanged;
    public event Action<int> OnMaxLifeChanged;

    private float life;
    public float Life 
    { 
        get { return life; }
        set
        {
            life = Mathf.Clamp(value, 0, MaxLife);
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

    public LifeModel() : this(10000) { }
    public LifeModel(int _mL)
    {
        maxLife = _mL;
        life = maxLife;
    }

    public void Reset()
    {
        Life = MaxLife;
    }

    public float GetDecreaseRate(float time)
    {
        return 2f;
    }

    public void ProcessJudge(bool IsCorrect)
    {
        if(IsCorrect)
        {
            Life += 10;
        }
        else
        {
            Life -= 10;
        }
    }
}
