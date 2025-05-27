using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider hp_Slider;

    void Update()
    {
        float t = Mathf.InverseLerp(1f, 100f, (float)playerHealth.currentHp);
        hp_Slider.value = Mathf.Clamp01(t);
    }
}
