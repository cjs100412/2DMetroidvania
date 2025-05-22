using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [Header("References")]
    public Transform target;               // 플레이어 또는 몬스터 Transform
    public RectTransform iconsParent;      // IconsParent RectTransform under Canvas
    public MinimapController minimapController; // 참조하는 MinimapController
    public Color iconColor = Color.blue;   // 파란 점(플레이어) 또는 빨간 점(몬스터)
    public float iconSize = 5f;

    private RectTransform iconRect;
    private Image iconImage;

    void Start()
    {
        GameObject go = new GameObject(target.name + "_Icon");
        go.transform.SetParent(iconsParent, false);
        iconImage = go.AddComponent<Image>();
        iconImage.color = iconColor;
        iconRect = iconImage.rectTransform;
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
    }

    void LateUpdate()
    {
        float mapW = minimapController.mapWidth;
        float mapH = minimapController.mapHeight;

        RectTransform miniRT = minimapController.minimapDisplay.rectTransform;
        float w = miniRT.rect.width;
        float h = miniRT.rect.height;

        float normalizedX = (target.position.x + mapW * 0.5f) / mapW;
        float normalizedY = (target.position.y + mapH * 0.5f) / mapH;

        iconRect.anchoredPosition = new Vector2(normalizedX * w, normalizedY * h);
    }
}