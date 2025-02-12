using UnityEngine;

public class WorldLevelManager : MonoBehaviour
{
    public static WorldLevelManager Instance;

    [Header("Prefabs")]
    [SerializeField] GameObject[] platformPrefabs;

    [Header("Spawn Config")]
    [SerializeField] Vector2 firstSpawnLocation;
    [SerializeField] int initialSpawn;

    private int spawnMultiplier = 0;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        for (int i = 0; i < initialSpawn; i++)
        {
            CreateWorld();
        }
    }

    public void CreateWorld()
    {
        Shuffle(platformPrefabs);
        GameObject chunk = Instantiate(platformPrefabs[0]);
        chunk.transform.position = firstSpawnLocation * spawnMultiplier;
        chunk.transform.rotation = Quaternion.identity;
        spawnMultiplier += 1;
    }

    private void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]); // Swap
        }
    }

}
