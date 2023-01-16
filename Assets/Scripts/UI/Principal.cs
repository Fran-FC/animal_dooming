using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Principal : MonoBehaviour
{
    const float ImageWidth = 2000.0f,
                TimeOut = 9.0f;

    public enum SplashStates {Moving, Finish}

    public SplashStates State;
    public Vector3 Speed;

    float startTime;
    Image image;
    Color32 c;

    // Start is called before the first frame update
    void Start()
    {
        State = SplashStates.Moving;
        startTime = Time.time;
        image = GetComponent<Image>();
        c = new Color32(0,0,0,255);   
    }

    // Update is called once per frame
    void Update()
    {
        switch(State)
        {
            case SplashStates.Moving:
                if(c.r < 255)
                    c.r +=1;
                if(c.g <255)
                    c.g +=1;
                if(c.b <255)
                    c.b +=1;

                /*if(Time.time - startTime > TimeOut)
                    State = SplashStates.Finish;*/
                
                if(Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return) ||Input.GetKey(KeyCode.Space))
                    State = SplashStates.Finish;
                break;
            case SplashStates.Finish:
                SceneManager.LoadScene("Options");   
                break;
        }
    }
}
