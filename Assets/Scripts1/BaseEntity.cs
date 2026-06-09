
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
   [SerializeField] protected string entityname = " ";
    protected HealthComponent health; 
    // Start is called before the first frame update
   protected virtual void Start()
    {
      health = GetComponent<HealthComponent>();
        if(health == null)
        {
            Debug.LogWarning(entityname + "is missing a healthComponent!");
        }
    }

    public virtual void OnDeath()
    { Debug.Log(entityname + " base OnDeath Called"); }
    // Update is called once per frame
   
}
