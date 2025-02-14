using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int highScore = 0;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
