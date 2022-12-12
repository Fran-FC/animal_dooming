using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class SingleMissionManager : MonoBehaviour
{

    private Mission mission;
    private Objective objective;
    GameObject text;
    bool selected = false;

    MissionsManager missionsManager;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.transform.GetChild(0).gameObject;
        missionsManager = gameObject.transform.parent.GetComponent<MissionsManager>();
    }

    public void setMission(Mission newMission) {

        mission = newMission;
        objective = mission.objectives[0];
    
    }

    public void onPointerEnterTitle()
    {
        // Debug.Log("Enter Trigger");
        text.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
    }

    public void onPointerExitTitle()
    {
        // Debug.Log("Exit Trigger");
        if (!selected)
        {
            text.GetComponent<TextMeshProUGUI>().color = new Color32(164, 164, 164, 255);
        }
    }

    public void onMissionClick() 
    {
        if (missionsManager.SelectedMission != null)
        {
            missionsManager.SelectedMission.deselect();
        }
        missionsManager.SelectedMission = this;
        missionsManager.instantiateObjectives(mission);
        selected = true;
    }


    public void advanceObjective() {

        if (objective.objectiveType == Objective.ObjectiveType.number)
        {
            objective.amount++;
            if (objective.amount == objective.goalAmount)
            {
                objective.objectiveStatus = Objective.ObjectiveStatus.finished;
                if (objective.Equals(mission.objectives[mission.objectives.Count - 1])) {
                    mission.missionStatus = Mission.MissionStatus.finished;
                    missionsManager.nextMission();
                }
            }
        }else {
            objective.objectiveStatus = Objective.ObjectiveStatus.finished;
            if (objective.Equals(mission.objectives[mission.objectives.Count - 1]))
            {
                mission.missionStatus = Mission.MissionStatus.finished;
                missionsManager.nextMission();
            }
        }

    }

    public void deselect()
    {
        selected = false;
        text.GetComponent<TextMeshProUGUI>().color = new Color32(164, 164, 164, 255);
    }


}
