using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateSetter
{
    public enum SetStrength
    {
        Weak,
        Medium,
        Strong,
        Immovable
    }
    public SetStrength setStrength;
    public object source;


    public Action ControlMethod { get; private set; }
    public Action UpdateMethod { get; private set; }
    public Action FixedUpdateMethod { get; private set; }
    public Action CancelMethod { get; private set; }

    public bool nullsOverride = true;

    public StateSetter(object _source, Action _ControlMethod, Action _UpdateMethod, Action _FixedUpdateMethod, Action _CancelMethod, SetStrength _setStrength = SetStrength.Weak)
    {
        source = _source;
        ControlMethod = _ControlMethod;
        UpdateMethod = _UpdateMethod;
        FixedUpdateMethod = _FixedUpdateMethod;
        setStrength = _setStrength;
        CancelMethod = _CancelMethod;
    }

    public StateSetter(object _source, Action _ControlMethod, Action _UpdateMethod, Action _FixedUpdateMethod, SetStrength _setStrength = SetStrength.Weak)
        : this(_source, _ControlMethod, _UpdateMethod, _FixedUpdateMethod, null, _setStrength) { }

    public bool CancelState()
    {
        if(CancelMethod != null)
        {
            CancelMethod.Invoke();
            return true;
        }
        else
        {
            return false;
        }
    }
}
