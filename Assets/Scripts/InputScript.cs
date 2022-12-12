using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    public GameObject missionsMenu; // Assign in inspector
    public GameObject mapMenu; // Assign in inspector
    public GameObject mapCamera;
    private bool isShowingMissions;
    private bool isShowingMap;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isShowingMissions)
            {
                closeCurrentMenu();
            }
            else {
                closeCurrentMenu();
                Time.timeScale = 0;
                isShowingMissions = true;
                missionsMenu.SetActive(isShowingMissions);
            }
        }

        /* if (Input.GetKeyDown(KeyCode.T))
        {
            missionsMenu.GetComponentInChildren<MissionsManager>().advanceMissionObjective();
        } */

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isShowingMap)
            {
                closeCurrentMenu();
            }
            else
            {
                closeCurrentMenu();
                mapCamera.SetActive(true);
                Time.timeScale = 0;
                isShowingMap = true;
                mapMenu.SetActive(isShowingMap);
            }
        }
    }

    public void closeCurrentMenu() {

        isShowingMap = false;
        isShowingMissions = false;
        missionsMenu.SetActive(false);
        mapMenu.SetActive(false);
        mapCamera.SetActive(false);
        Time.timeScale = 1;

    }

}
