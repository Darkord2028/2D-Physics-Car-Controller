using UnityEngine;

public class ResetCheckpoint : MonoBehaviour
{
    [SerializeField] Collider2D rootCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        rootCollider.isTrigger = true;
    }

}
