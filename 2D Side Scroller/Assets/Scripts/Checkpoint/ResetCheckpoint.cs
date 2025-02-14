using UnityEngine;

public class ResetCheckpoint : MonoBehaviour
{
    private Collider2D rootCollider;

    private void Start()
    {
        rootCollider = GetComponentInParent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        rootCollider.isTrigger = true;
    }

}
