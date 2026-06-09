
using UnityEngine;

public class EnemyEntity : BaseEntity
{
    [SerializeField] private int scorevalue = 10;
    private void OnMouseDown()
    {
        health.TakeDamage(34);
    }
    // Start is called before the first frame update
    public override void OnDeath()
    {
        Debug.Log(entityname + "Died!" + scorevalue + "points");
        GameManager.instance.AddScore( scorevalue);
        gameObject.SetActive(false);

    }
}
