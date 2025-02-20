using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WorldCheckPointManager.instance.TranslateToLastCheckpoint(collision.gameObject);
        }
    }
}
