using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionsManager : MonoBehaviour
{

    public List<Mission> missions;
    public GameObject MissionUiComponent;
    public GameObject ObjectiveUiComponent;
    [HideInInspector]
    public List<GameObject> MissionUIs;
    [HideInInspector]
    public List<GameObject> ObjectiveUIs;

    [HideInInspector]
    public SingleMissionManager SelectedMission;
    //Panel missionsPanel;

    private SingleMissionManager currentMission;
    private int currentMissionIndex;

    // Start is called before the first frame update
    void Start()
    {
        //missionsPanel = gameObject.GetComponent<missionsPanel>;

        MissionUIs = new List<GameObject>();
        ObjectiveUIs = new List<GameObject>();

        foreach (Mission mission in missions) { // UI
            GameObject missionUI = Instantiate(MissionUiComponent, new Vector3(0, 0, 0), Quaternion.identity);
            missionUI.transform.SetParent(gameObject.transform);
            if (mission.missionStatus == Mission.MissionStatus.finished)
            {
                
                missionUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>" + mission.title + "</s>";

            }
            else if (mission.missionStatus == Mission.MissionStatus.started)
            {

                missionUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = mission.title;

            }
            else if (mission.missionStatus == Mission.MissionStatus.unstarted)
            {

                missionUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = mission.title;
                missionUI.SetActive(false);

            }

            SingleMissionManager missionManager = missionUI.GetComponent<SingleMissionManager>();
            missionManager.setMission(mission);

            RectTransform rectTransform = missionUI.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector3(-450, 200 - 100 * (MissionUIs.Count), 0);

            MissionUIs.Add(missionUI);
        }

        //Management
        currentMission = MissionUIs[0].GetComponent<SingleMissionManager>();
        currentMissionIndex = 0;

    }


    public void instantiateObjectives(Mission mission)
    {
        foreach (GameObject objectiveGameObject in ObjectiveUIs) {
            Destroy(objectiveGameObject);
        }
        ObjectiveUIs.Clear();

        foreach (Objective objective in mission.objectives)
        {
            if (objective.objectiveStatus == Objective.ObjectiveStatus.hidden) {
                continue;
            }
            GameObject objectiveUI = Instantiate(ObjectiveUiComponent, new Vector3(0, 0, 0), Quaternion.identity);
            TextMeshProUGUI textMesh = objectiveUI.GetComponent<TextMeshProUGUI>();
            textMesh.text = objective.title;
            if (objective.objectiveType == Objective.ObjectiveType.number) {
                textMesh.text = textMesh.text + string.Format(" ({0}/{1})", objective.amount, objective.goalAmount);
            }
            if (objective.optional)
            {
                textMesh.text = textMesh.text + " (optional)";
            } else {
                textMesh.text = textMesh.text;
            }
            if (objective.objectiveStatus == Objective.ObjectiveStatus.finished) {
                textMesh.text = "<s>" + textMesh.text + "</s>";

            } else {
                textMesh.text = textMesh.text;
            }
            objectiveUI.transform.SetParent(gameObject.transform);
            RectTransform rectTransform = objectiveUI.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector3(100, 200 - 100 * (ObjectiveUIs.Count), 0);
            ObjectiveUIs.Add(objectiveUI);
        }

    }

    public void advanceMissionObjective() {
        currentMission.advanceObjective();
    }

    public void nextMission()
    {

        currentMissionIndex++;
        MissionUIs[currentMissionIndex].SetActive(true);
        currentMission = MissionUIs[currentMissionIndex].GetComponent<SingleMissionManager>();

    }

}
