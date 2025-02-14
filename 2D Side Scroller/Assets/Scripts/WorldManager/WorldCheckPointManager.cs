using UnityEngine;

public class WorldCheckPointManager : MonoBehaviour
{
    public static WorldCheckPointManager instance;
    public Transform currentCheckPoint { get; private set; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCheckPoint(Transform checkPointTransform)
    {
        currentCheckPoint = checkPointTransform;
    }

    public Transform GetCheckPoint()
    {
        return currentCheckPoint;
    }

    public void TranslateToLastCheckpoint(GameObject currentCar)
    {
        currentCar.transform.position = GetCheckPoint().position;
        currentCar.transform.rotation = Quaternion.identity;
        currentCar.gameObject.SetActive(false);
        currentCar.gameObject.SetActive(true);
    }

}
