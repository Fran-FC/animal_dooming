using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space))
        {
            OnClickPlay();
        }
        if(Input.GetKey(KeyCode.Escape))
        {
            OnClickExit();
        }
    }

    public void OnClickPlay()
    {
    //    SceneManager.LoadScene("Play");
    }

    public void OnClickLoad()
    {
        SceneManager.LoadScene("Load");
    }

    public void OnClickOptions(){
        Debug.Log("You have clicked the options button!");
	}

    public void OnClickExit()
    {
        Debug.Log("You have clicked the exit button!");
        Application.Quit();
        //UnityEditor.EditorApplication.isPlaying = false;
    }

}
