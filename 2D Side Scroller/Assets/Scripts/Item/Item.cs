using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemType item;

    [Header("Coin")]
    [SerializeField] int coinMultiplier;
    [SerializeField] float moveDistance;
    [SerializeField] float duration;
    [SerializeField] AudioClip collectCoinAudioClip;

    [Header("Fuel")]
    [SerializeField] int fuelMultiplier;

    private SpriteRenderer itemSprite;

    private void Start()
    {
        itemSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                if(collision.TryGetComponent<CarController>(out CarController player))
                {
                    switch (item)
                    {
                        case ItemType.None: return;

                        case ItemType.Coin:
                            GainCoin(player); break;

                        case ItemType.Fuel:
                            GainFuel(player); break;
                    }
                }
            }
        }
    }

    private void GainFuel(CarController player)
    {
        player.GainFuel(fuelMultiplier);
        gameObject.SetActive(false);
    }

    private void GainCoin(CarController player)
    {
        GameManager.instance.highScore += coinMultiplier;
        WorldUIManager.instance.UpdateHighScore();
        player.PlayCoinCollectionSound(collectCoinAudioClip);
        MoveUpwardsAndDisable();
    }

    private void MoveUpwardsAndDisable()
    {
        if (itemSprite == null) return;


        LeanTween.moveY(gameObject, transform.position.y + moveDistance, duration).setEase(LeanTweenType.easeOutQuad);
        LeanTween.value(gameObject, 1f, 0f, duration).setOnUpdate((float alpha) =>
        {
            Color color = itemSprite.color;
            color.a = alpha;
            itemSprite.color = color;
        }).setOnComplete(() =>
        {
            gameObject.SetActive(false); // Disable the sprite after animation
        });
    }

}

public enum ItemType
{
    None,
    Fuel,
    Coin
}
