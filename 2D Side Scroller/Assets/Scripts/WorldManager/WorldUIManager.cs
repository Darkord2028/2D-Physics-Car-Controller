using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldUIManager : MonoBehaviour
{
    public static WorldUIManager instance;

    [Header("Car Fuel")]
    [SerializeField] Slider fuelSlider;
    [SerializeField] Image fuelIcon;

    [Header("Retry")]
    [SerializeField] GameObject retryGameobject;

    [Header("Other UI")]
    [SerializeField] TextMeshProUGUI stuntText;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    private void Start()
    {
        
    }

    private void SetInitialUI()
    {
        retryGameobject.SetActive(false);
        stuntText.text = string.Empty;
    }

    #region Set Function

    public void Retry()
    {
        retryGameobject.SetActive(true);
    }

    public void SetInitialFuel(int maxFuel)
    {
        fuelSlider.maxValue = maxFuel;
        fuelSlider.value = maxFuel;
    }

    public void UpdateFuelSlider(int currentFuel)
    {
        fuelSlider.value = currentFuel;
    }

    public void ShowStuntMessage(string stuntMessage)
    {
        stuntText.text = stuntMessage;
    }

    #endregion

}
