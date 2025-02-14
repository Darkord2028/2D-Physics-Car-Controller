using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class WorldUIManager : MonoBehaviour
{
    public static WorldUIManager instance;

    #region UI Data

    [Header("Car Fuel")]
    [SerializeField] Slider fuelSlider;
    [SerializeField] Image fuelIcon;

    [Header("Retry")]
    [SerializeField] GameObject retryGameobject;

    [Header("Car Stunt UI")]
    [SerializeField] TextMeshProUGUI stuntText;
    [SerializeField] Transform textParent;

    #endregion

    #region Object Pool Variable

    //Object Pooling
    private ObjectPool<TextMeshProUGUI> textPool;

    #endregion

    #region Unity Callback Functions

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

        textPool = new ObjectPool<TextMeshProUGUI>(CreateText);
    }

    private void Start()
    {
        SetInitialUI();
    }

    #endregion

    #region Object Pool Functions

    private TextMeshProUGUI CreateText()
    {
        TextMeshProUGUI text = Instantiate(stuntText);
        text.transform.SetParent(textParent);
        return text;
    }

    #endregion

    #region Set Function

    private void SetInitialUI()
    {
        retryGameobject.SetActive(false);
        stuntText.text = string.Empty;
    }

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

    public void ShowStuntMessage(string message)
    {
        TextMeshProUGUI text = textPool.Get();
        text.gameObject.SetActive(true);
        text.text = message;
        StartCoroutine(popUpText(text, 1.5f));
    }

    private IEnumerator popUpText(TextMeshProUGUI text, float textWaitTime)
    {
        yield return new WaitForSeconds(textWaitTime);
        text.gameObject.SetActive(false);
        textPool.Release(text);
    }

    #endregion

}
