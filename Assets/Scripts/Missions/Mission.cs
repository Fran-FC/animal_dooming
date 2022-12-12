using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Mission
{
    public int missionId;
    public enum MissionStatus {
        unstarted,
        started,
        finished
    }

    public string title;
    // public string description;
    public MissionStatus missionStatus;
    public List<Objective> objectives;

}

[System.Serializable]
public class Objective
{

    public enum ObjectiveStatus
    {
        hidden,
        started,
        finished
    }

    public enum ObjectiveType {
        single,
        number,
        time,
    }

    public string title;
    // public string description;
    public ObjectiveStatus objectiveStatus;
    public bool optional;
    public ObjectiveType objectiveType;
    [HideInInspector]
    public int amount;
    public int goalAmount;
}