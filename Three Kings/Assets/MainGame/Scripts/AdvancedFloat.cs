using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

[Serializable]
public class AdvancedFloat
{
    public float BaseValue;

    [SerializeField] protected float _value;
    public virtual float Value
    {
        get
        {
            _value = CalculateFinalValue();
            return _value;
        }
    }

    protected readonly List<FloatModifier> floatModifiers;
    public readonly ReadOnlyCollection<FloatModifier> FloatModifiers;

    public AdvancedFloat()
    {
        floatModifiers = new List<FloatModifier>();
        FloatModifiers = floatModifiers.AsReadOnly();
    }

    public AdvancedFloat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    protected virtual void AddModifier(FloatModifier mod)
    {
        floatModifiers.Add(mod);
        mod.associatedFloat = this;
        if (mod.isTimed)
        {
            mod.additionTime = Time.time;
        }
    }
    public virtual void AddSingleModifier(FloatModifier mod)
    {
        AddModifier(mod);
        floatModifiers.Sort(CompareModifierOrder);
    }
    public virtual void AddMultipleModifiers(List<FloatModifier> mods)
    {
        foreach (FloatModifier mod in mods)
        {
            AddModifier(mod);
        }
        floatModifiers.Sort(CompareModifierOrder);
    }

    protected virtual bool RemoveModifier(FloatModifier mod)
    {
        if (floatModifiers.Remove(mod))
        {
            mod.associatedFloat = null;
            return true;
        }
        return false;
    }
    public virtual bool RemoveSingleModifier(FloatModifier mod)
    {
        bool result = RemoveModifier(mod);
        floatModifiers.Sort(CompareModifierOrder);
        return result;
    }
    public virtual bool RemoveMultipleModifiers(List<FloatModifier> mods)
    {
        bool result = false;
        foreach (FloatModifier mod in mods)
        {
            if (RemoveModifier(mod) && !result)
            {
                result = true;
            }
        }

        floatModifiers.Sort(CompareModifierOrder);

        return result;
    }

    public virtual bool RemoveAllModifiersFromSource(object source)
    {
        List<FloatModifier> found = floatModifiers.FindAll(mod => mod.source == source);
        RemoveMultipleModifiers(found);

        if (found.Count > 0)
        {
            return true;
        }
        return false;
    }

    public virtual void PauseAllModifiers()
    {
        foreach(FloatModifier mod in floatModifiers)
        {
            if (mod.isTimed)
            {
                mod.IsPaused = true;
            }
        }
    }
    public virtual void UnPauseAllModifiers()
    {
        foreach (FloatModifier mod in floatModifiers)
        {
            if (mod.isTimed)
            {
                mod.IsPaused = false;
            }
        }
    }

    public virtual void RemoveAllModifiers()
    {
        List<FloatModifier> mods = floatModifiers.ToList();
        RemoveMultipleModifiers(mods);
    }

    protected virtual int CompareModifierOrder(FloatModifier a, FloatModifier b)
    {
        if (a.order < b.order)
            return -1;
        else if (a.order > b.order)
            return 1;
        return 0; //if (a.Order == b.Order)
    }

    protected virtual float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        //floatModifiers.Sort(CompareModifierOrder);
        for(int i = floatModifiers.Count - 1; i >= 0; i--)
        {
            if (floatModifiers[i].TimeCheck())
            {
                RemoveSingleModifier(floatModifiers[i]);
            }
        }

        for (int i = 0; i < floatModifiers.Count; i++)
        {
            FloatModifier mod = floatModifiers[i];

            if (mod.type == FloatModifier.FloatModType.Flat)
            {
                finalValue += mod.ModifierValue;
            }
            else if (mod.type == FloatModifier.FloatModType.PercentAdd)
            {
                sumPercentAdd += mod.ModifierValue;

                if (i + 1 >= floatModifiers.Count || floatModifiers[i + 1].type != FloatModifier.FloatModType.PercentAdd)
                {
                    finalValue *= sumPercentAdd; //If you want to do increases, it needs to start at 1.
                    sumPercentAdd = 0;
                }
            }
            else if (mod.type == FloatModifier.FloatModType.PercentMult)
            {
                finalValue *= mod.ModifierValue; //If you want to do increases, it needs to start at 1.
            }
        }

        return finalValue;

        // Workaround for float calculation errors, like displaying 12.00001 instead of 12
        //return (float)Math.Round(finalValue, 4);
    }


}
