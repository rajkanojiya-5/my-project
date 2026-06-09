
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
 [SerializeField] private  float maxhealth = 100f;
   private float _CurrentHealth = 0f;

    public float CurrentHealth => _CurrentHealth;
    public bool IsDead => _CurrentHealth <= 0f;
    // Start is called before the first frame update
    void Start()
    {
        _CurrentHealth = maxhealth;
    }

    // Update is called once per frame
    void Update()
    {
        //  if(Input.GetKeyDown(KeyCode.Space))
        //  {
        //_CurrentHealth += 10;
        // Debug.Log($"Current Health = {_CurrentHealth}");
        //}
       // TakeDamage(20);
    }
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        _CurrentHealth -= amount;
        _CurrentHealth = Mathf.Max(_CurrentHealth, 0f);
        Debug.Log($"After Taking action damage is : { _CurrentHealth }");

        if (IsDead) {Die(); }
    }
    public void Heal(float amount)
    { if (IsDead) return;
    _CurrentHealth += amount;
        _CurrentHealth = Mathf.Max(_CurrentHealth, maxhealth);
        Debug.Log("");
    }
    private void Die() { Debug.Log(gameObject.name + "has Died!");
     GetComponent<BaseEntity>()?.OnDeath();
    }
}
