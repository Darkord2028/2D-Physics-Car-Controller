using Unity.Cinemachine;
using UnityEngine;

public class CarSelectionManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera _camera;

    public void SelectCar(GameObject Car)
    {
        GameObject instance = Instantiate(Car, transform.position, transform.rotation);
        _camera.Follow = instance.transform;
    }
}
