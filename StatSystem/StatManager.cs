using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class StatManager : MonoBehaviour
{
    //   Vars
    // Events
    public event EventHandler StatAdded = delegate { };
    public event EventHandler StatRemoved = delegate { };
    
    // Objects

    // Stats
    private List<Stat> stats = new List<Stat>();

    // Bools
    private bool DoTick = true;   



    //   Unity functions
    void Start() {
        Debug.Log("[StatManager] Start");
        foreach (Stat stat in stats) {
            stat.Init();
            stat.Activate();
        }
    }
    void Update() {
        if(DoTick) { TickStats(); }
    }



    //   Methods
    // Stat addition / removal
    public void AddStat(Stat stat) {
        stats.Add(stat);
        StatAdded(this, new EventArgs());
        
        //Debug.Log("[StatManager] Stat added, list is now " + String.Join(", ", stats));
    }

    public void RemoveStat(string name, ObjectGUID guid = null) { 
        if(guid != null) {
            stats.RemoveAll(s => (s.statGUID.Equals(guid)));
        } else {
            if(name != null) {
                stats.RemoveAll(s => (s.statName == name));
            } else { stats.Clear(); }
        }
        StatRemoved(this, new EventArgs());
        
        //Debug.Log("[StatManager] Stat removed, list is now " + String.Join(", ", stats));
    }

    // Stat access
    public List<Stat> GetStat(string name, ObjectGUID guid = null) {    
        if(guid != null) {
            return stats.FindAll(s => (s.statGUID.Equals(guid)));
        } else {
            List<Stat> entries;
            if(name != null) {
                entries = stats.FindAll(s => (s.statName == name));
            } else { entries = stats; }
            return entries;
        }
    }

    // Stat updating
    public void TickStats() {
        foreach (Stat stat in stats) {
            stat.TickUpdate();
        }
    }

    // Ticking control
    public void PauseTicking(float seconds) {
        StartCoroutine(PauseRoutine(seconds));
    }
    public void ToggleTicking() {
        this.DoTick = !this.DoTick;
    }
    public void SetTicking(bool value) {
        this.DoTick = value;
    }

    IEnumerator PauseRoutine(float time) {
        this.DoTick = false;
        yield return new WaitForSeconds(time);
        this.DoTick = true;        
    } 
}
