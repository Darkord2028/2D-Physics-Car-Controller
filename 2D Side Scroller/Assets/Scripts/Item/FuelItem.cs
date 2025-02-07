using UnityEngine;

public class FuelItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] float fuelGain;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                if(collision.TryGetComponent<CarController>(out CarController player))
                {
                    player.GainFuel(fuelGain);
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
