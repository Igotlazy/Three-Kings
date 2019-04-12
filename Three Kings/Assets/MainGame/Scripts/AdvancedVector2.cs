using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public float XValue()
    {
        return X.Value;
    }

    public float YValue()
    {
        return Y.Value;
    }

    public Vector2 Vector2Value()
    {
        return new Vector2(X.Value, Y.Value);
    }

}
