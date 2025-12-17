using Unity.Cinemachine;
using UnityEngine;

public class CarSelectionManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera _camera;
    [SerializeField] GameObject leaderboard;

    public void SelectCar(GameObject Car)
    {
        leaderboard.SetActive(false);
        GameObject instance = Instantiate(Car, transform.position, transform.rotation);
        _camera.Follow = instance.transform;
    }
}
