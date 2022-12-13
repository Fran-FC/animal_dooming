using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
public class Ammo : MonoBehaviour
{
    public float damage = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Health health = col.gameObject.GetComponent<Health>();
            health.TakeDamage(damage, this.gameObject);
        }
    }
}
