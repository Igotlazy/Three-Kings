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

    public Action SetUpMethod { get; private set; }
    public Action ControlMethod { get; private set; }
    public Action UpdateMethod { get; private set; }
    public Action FixedUpdateMethod { get; private set; }
    public Action CancelMethod { get; private set; }

    public StateSetter(object _source, Action _SetUpMethod, Action _ControlMethod, Action _UpdateMethod, Action _FixedUpdateMethod, Action _CancelMethod, SetStrength _setStrength = SetStrength.Weak)
    {
        source = _source;

        SetUpMethod = _SetUpMethod;
        ControlMethod = _ControlMethod;
        UpdateMethod = _UpdateMethod;
        FixedUpdateMethod = _FixedUpdateMethod;
        CancelMethod = _CancelMethod;

        setStrength = _setStrength;
    }

    public StateSetter(object _source, Action _SetUpMethod, Action _ControlMethod, Action _UpdateMethod, Action _FixedUpdateMethod, SetStrength _setStrength = SetStrength.Weak)
        : this(_source, _SetUpMethod, _ControlMethod, _UpdateMethod, _FixedUpdateMethod, null, _setStrength) { }

}
