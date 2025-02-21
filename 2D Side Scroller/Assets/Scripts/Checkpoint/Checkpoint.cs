using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Collider2D triggerCollider;
    public Transform checkPointTransform;

    private void Start()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WorldCheckPointManager.instance.SetCheckPoint(checkPointTransform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            triggerCollider.isTrigger = false;

        }
    }

}
