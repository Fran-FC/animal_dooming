using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Texto : MonoBehaviour
{
    Color oc;

    TMPro.TextMeshProUGUI texto;

    [SerializeField]
    float angulo, velAng;

    // Start is called before the first frame update
    void Start()
    {
        texto = GetComponent<TMPro.TextMeshProUGUI>();    
        oc = texto.color;
        if(velAng < 0.0f)
            velAng = -velAng;
    }

    // Update is called once per frame
    void Update()
    {
        float seno = Mathf.Abs(Mathf.Sin(angulo));

        angulo += velAng * Time.deltaTime;
        if (angulo > 360.0f)
            angulo -= 360.0f;

        texto.color = oc * seno;
    }
}
