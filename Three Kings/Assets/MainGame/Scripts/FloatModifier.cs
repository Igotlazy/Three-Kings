using UnityEngine;

[System.Serializable]
public class FloatModifier
{
    [SerializeField] private float modifierValue;
    public float ModifierValue
    {
        get
        {
            return modifierValue * multiplier;
        }
        set
        {
            modifierValue = value;
        }
    }

    public float multiplier = 1;

    public readonly FloatModType type;
    public readonly int order;
    public readonly object source;

    public float duration;
    public readonly bool isTimed;
    public bool ignoreRemove;

    private bool isPaused;
    public bool IsPaused
    {
        get
        {
            return isPaused;
        }
        set
        {
            if (value && !isPaused)
            {
                lastPauseTime = Time.time;
            }
            if(!value && isPaused)
            {
                totalPauseTime += Time.time - lastPauseTime;
            }

            isPaused = value;
        }
    }
    private float totalPauseTime;
    private float lastPauseTime;
    public float timeLeft;

    public AdvancedFloat associatedFloat;

    public float timeofAddition;

    public enum FloatModType
    {
        Flat = 100,
        PercentAdd = 200,
        PercentMult = 300,
    }

    public FloatModifier(float value, FloatModType type, int order, object source, float duration)
    {
        this.ModifierValue = value;
        this.type = type;
        this.order = order;
        this.source = source;
        this.duration = duration;
        timeLeft = duration;

        if (duration > 0)
        {
            isTimed = true;
        }
    }

    public FloatModifier(float value, FloatModType type) : this(value, type, (int)type, null, 0) { }
    public FloatModifier(float value, FloatModType type, int order) : this(value, type, order, null, 0) { }
    public FloatModifier(float value, FloatModType type, object source) : this(value, type, (int)type, source, 0) { }
    public FloatModifier(float value, FloatModType type, float duration) : this(value, type, (int)type, null, duration) { }
    public FloatModifier(float value, FloatModType type, int order, float duration) : this(value, type, order, null, duration) { }
    public FloatModifier(float value, FloatModType type, object source, float duration) : this(value, type, (int)type, source, duration) { }


    public bool UpdateTime()
    {
        if (isTimed && !isPaused)
        {
            
            if(associatedFloat != null && associatedFloat.ApplyTimeClamp)
            {
                timeLeft -= associatedFloat.TimeClampStep;
            }
            else
            {
                timeLeft = duration - (Time.time - timeofAddition - totalPauseTime);
            }
            

            if (timeLeft < 0)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public float ModifierValueWithClamp(float step)
    {
        if (isTimed && type == FloatModType.Flat)
        {
            float result = TimeClamp(duration - timeLeft, duration, step) * ModifierValue;
            Debug.Log(result);
            return result;
        }
        else
        {
            return ModifierValue;
        }
    }

    public bool RemoveFromFloat()
    {
        if (associatedFloat != null)
        {
            return associatedFloat.RemoveSingleModifier(this, true);
        }
        return false;
    }

    public static float TimeClamp(float current, float target, float step)
    {
        if (current >= target)
        {
            return 0;
        }

        float normalAdd = current + step;

        if (normalAdd <= target)
        {
            return 1;
        }
        else
        {
            float over = normalAdd - target;
            return (step - over) / step;
        }
    }
}
