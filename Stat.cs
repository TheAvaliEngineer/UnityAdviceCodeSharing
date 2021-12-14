using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public abstract class Stat
{
    //   Vars
    // Events
    public event EventHandler<StatChangeArgs> StatChanged = delegate { };
    public event EventHandler<StatChangeArgs> StatZeroed = delegate { };

    // Identification
    public abstract string statName { get; set; }
    public string subName { get; set; }
    public ObjectGUID statGUID { get; }

    // Values
    public float value { get; set; }
    public float maxValue { get; set; }

    // Modifiers/effects
    public List<StatModifier> modifiers = new List<StatModifier>();
    public float multiplier { get; set; }

    // Activity
    public bool isActive { get; set; }



    //   Methods
    // initialization
    public void Init() { 
        StatChanged?.Invoke(this, new StatChangeArgs(0, this.value, this.maxValue));
    }
    public void Activate() { this.isActive = true; }


    // equality
    public bool Equals(Stat other) { 
        if(this.statName == null || other.statName == null) { return GUIDEquals(other); } 
        else { return this.statName.Equals(other.statName); }
    }
    public bool GUIDEquals(Stat other) {
        return this.statGUID.Equals(other.statGUID);
    }
    

    // string conversion
    public override string ToString() {
        return this.GetType() + "<value: " + this.value + ", maxValue: " + 
        this.maxValue + ", isActive: " + this.isActive + ">";
    }


    // event invocation
    protected virtual void OnStatChange(StatChangeArgs e) { StatChanged?.Invoke(this, e); }
    protected virtual void OnStatZero(StatChangeArgs e) { StatZeroed?.Invoke(this, e); }
    

    // value changing
    public abstract void ChangeValue(float changeValue);
    public abstract void ChangeMaxValue(float changeValue);

    // value accessing
    public abstract float GetModifiedValue();
    public abstract float GetModifiedMax();


    // per-tick update
    public virtual void TickUpdate() {
        this.multiplier = CalculateMultiplier();
    }
    

    // modifier addition/removal
    public void AddModifier(StatModifier modifier) {
        modifiers.Add(modifier);
    }
    public void RemoveModifier(StatModifier modifier) { 
        modifiers.Remove(modifier);
    }


    // multiplier value calculation
    public float CalculateMultiplier() {
        if(modifiers.Count == 0) return 1;
    
        float product = 1; float sum = 0;
        foreach (StatModifier modifier in modifiers) {
            product *= modifier.multiplyValue;
            sum += modifier.addValue;
        }

        return product + sum;
    }   
}





// Stats
class HealthStat : Stat {
    //   Vars
    // Name
    public override string statName { 
        get { return "Health"; } 
        set { this.subName = value; }
    }

    // Resistances
    public List<DamageModifier> baseModifiers = new List<DamageModifier>();



    //   Methods
    // Value changing
    public override void ChangeValue(float changeValue) { 
        if(isActive) {
            this.value = Mathf.Min(this.maxValue, this.value + changeValue); 
            OnStatChange(new StatChangeArgs(changeValue, this.value, this.maxValue));
        }
    }
    public override void ChangeMaxValue(float changeValue) { 
        this.maxValue += changeValue; 
    }

    // Value accessing
    public override float GetModifiedValue() {
        return(this.value);
    }
    public override float GetModifiedMax() {
        return(Mathf.Floor(this.maxValue * this.multiplier));
    }

    // Per-tick update
    public override void TickUpdate() {
        if(isActive && this.value < 0) { 
            OnStatZero(new StatChangeArgs(0, this.value, this.maxValue));
            this.isActive = false;
        }

        base.TickUpdate();
    }


    // Configure resistances

}

class TimeStat : Stat {
    //   Vars
    // Name
    public override string statName { 
        get { return "Time"; } 
        set { this.subName = value; } 
    }



    //   Methods
    // Value changing
    public override void ChangeValue(float changeValue) { 
        this.value += changeValue;
        this.maxValue = this.value;
        OnStatChange(new StatChangeArgs(changeValue, this.value, this.maxValue));
    }
    public override void ChangeMaxValue(float changeValue) {
        this.maxValue = changeValue;
    }

    // Value accessing
    public override float GetModifiedValue() {
        return(this.value);
    }
    public override float GetModifiedMax() {
        return(this.maxValue);
    }

    // Per-tick update
    public override void TickUpdate() {
        if(isActive) { 
            float incTime = Time.deltaTime * this.multiplier; 
            this.value -= incTime; 

            if(this.value < 0) { OnStatZero(new StatChangeArgs(0, this.value, this.maxValue)); } 
            else if(incTime != 0) { OnStatChange(new StatChangeArgs(incTime, this.value, this.maxValue)); }
        }

        base.TickUpdate();
    }
}

class AmmoStat : Stat {
    //   Vars
    // Name
    public override string statName { 
        get { return null; } 
        set { this.subName = value; } 
    }



    //   Methods 
    // Value changing
    public override void ChangeValue(float changeValue) { 
        this.value += changeValue; 
        OnStatChange(new StatChangeArgs(changeValue, this.value, this.maxValue));
    }
    public override void ChangeMaxValue(float changeValue) {
        this.maxValue += changeValue;
    }

    // Value accessing
    public override float GetModifiedValue() {
        return(this.value);
    }
    public override float GetModifiedMax() {
        return(Mathf.Floor(this.maxValue * this.multiplier));
    }
}