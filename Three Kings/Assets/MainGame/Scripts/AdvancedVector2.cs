using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AdvancedVector2
{
    [SerializeField] private AdvancedFloat x;
    public AdvancedFloat X
    {
        get
        {
            return x;
        }
        private set
        {
            x = value;
        }
    }

    [SerializeField] private AdvancedFloat y;
    public AdvancedFloat Y
    {
        get
        {
            return y;
        }
        private set
        {
           y = value;
        }
    }

    public AdvancedVector2()
    {
        X = new AdvancedFloat();
        Y = new AdvancedFloat();
    }

    public AdvancedVector2(AdvancedFloat newX, AdvancedFloat newY)
    {
        X = newX;
        Y = newY;
    }

    public float XValue
    {
        get
        {
            return X.Value;
        }
    }

    public float XValueTimeClamp(float step)
    {
        return x.TimeClampedValue(step);
    }

    public float YValue
    {
        get
        {
            return Y.Value;
        }
    }

    public float YValueTimeClamp(float step)
    {
        return y.TimeClampedValue(step);
    }

    public Vector2 Vector2Value
    {
        get
        {
            return new Vector2(X.Value, Y.Value);
        }
    }

    public Vector2 Vector2ValueTimeClamp(float step)
    {
        return new Vector2(X.TimeClampedValue(step), Y.TimeClampedValue(step));
    }

    public void RemoveAllModifiers()
    {
        X.RemoveAllModifiers();
        Y.RemoveAllModifiers();
    }

}
