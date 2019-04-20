using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AdvancedVector2
{
    public AdvancedFloat X { get; private set; }
    public AdvancedFloat Y { get; private set; }

    public AdvancedVector2()
    {
        X = new AdvancedFloat();
        Y = new AdvancedFloat();
    }

    public AdvancedVector2(AdvancedFloat x, AdvancedFloat y)
    {
        X = x;
        Y = y;
    }

    public float XValue
    {
        get
        {
            return X.Value;
        }
    }

    public float YValue
    {
        get
        {
            return Y.Value;
        }
    }

    public Vector2 Vector2Value
    {
        get
        {
            return new Vector2(X.Value, Y.Value);
        }
    }

}
