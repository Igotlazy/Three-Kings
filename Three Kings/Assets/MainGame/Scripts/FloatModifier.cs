using UnityEngine;

[System.Serializable]
public class FloatModifier
{
    private float modifierValue;
    public float ModifierValue
    {
        get
        {
            return modifierValue * multiplier;
        }
        set
        {
            this.modifierValue = value;
        }
    }

    public float multiplier = 1;

    public readonly FloatModType type;
    public readonly int order;
    public readonly object source;

    public float duration;
    public readonly bool isTimed;

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

    public float additionTime;

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


    public bool TimeCheck()
    {
        if (isTimed && !isPaused)
        {
            timeLeft = duration - (Time.time - additionTime - totalPauseTime);

            if(timeLeft < 0)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public bool RemoveFromFloat()
    {
        if (associatedFloat != null)
        {
            return associatedFloat.RemoveSingleModifier(this);
        }
        return false;
    }
}
