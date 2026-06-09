
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private int _score = 0;
    // Start is called before the first frame update
   private void Awake()
    {
        instance = this;
        
    }

    // Update is called once per frame
   public void AddScore(int amount)
    {
        _score += amount;
        Debug.Log("Score : " +  _score);
    }
}
