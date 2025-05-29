using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;
    public PlayerInventory playerInventory;
    public Slider hp_Slider;
    public Slider mp_Slider;
    public Text coinText;
    private void Awake()
    {
            mp_Slider.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerInventory.OnCoinChanged += UpdateCoinText;
    }
    void Update()
    {
        float t = Mathf.InverseLerp(1f, 100f, (float)playerHealth.currentHp);
        hp_Slider.value = Mathf.Clamp01(t);

        float m = Mathf.InverseLerp(0f, 5f, (float)playerHealth.currentMp);
        mp_Slider.value = Mathf.Clamp01(m);
    }
    void UpdateCoinText(int newCount)
    {
        coinText.text = newCount.ToString();
    }
    public void ShowMpSlider()
    {
        mp_Slider.gameObject.SetActive(true);
    }
}
