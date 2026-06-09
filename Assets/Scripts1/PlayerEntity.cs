using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerentity : BaseEntity
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            health.TakeDamage(15f);
        }
        if (Input.GetKeyDown(KeyCode.H))
            health.Heal(20f);
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
    public override void OnDeath()
    {
        Debug.Log(entityname + "Died!");

    }
    
}
