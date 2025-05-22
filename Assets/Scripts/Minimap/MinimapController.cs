using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RawImage minimapDisplay;   // UI RawImage to show the minimap camera RenderTexture
    public RawImage fogDisplay;       // UI RawImage for Fog of War overlay

    [Header("Map Bounds (World Units)")]
    public float mapWidth = 50f;
    public float mapHeight = 30f;

    [Header("Rendering Layers")]
    public LayerMask groundLayer;     // Ground layer to render on minimap

    [Header("Fog of War")]
    public int fogResolutionX = 100;
    public int fogResolutionY = 60;
    public Color unseenColor = new Color(0, 0, 0, 1f);   // 완전 불투명
    public Color visitedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 반투명 회색
    public Color visibleColor = new Color(0, 0, 0, 0f);   // 완전 투명
    public int viewRadiusTiles = 5;

    private Camera minimapCam;
    private Texture2D fogTexture;
    private bool[,] visited;

    void Start()
    {
        // Setup minimap camera
        minimapCam = GetComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.cullingMask = groundLayer; // Only render ground layer

        RenderTexture rt = new RenderTexture(fogResolutionX, fogResolutionY, 16);
        minimapCam.targetTexture = rt;
        minimapDisplay.texture = rt;

        // Setup fog texture and overlay
        fogTexture = new Texture2D(fogResolutionX, fogResolutionY, TextureFormat.RGBA32, false);
        fogDisplay.texture = fogTexture;
        Material fogMat = new Material(Shader.Find("Unlit/Transparent"));
        fogDisplay.material = fogMat;

        visited = new bool[fogResolutionX, fogResolutionY];

        // Sync fog overlay RectTransform with minimap display
        fogDisplay.rectTransform.anchorMin = minimapDisplay.rectTransform.anchorMin;
        fogDisplay.rectTransform.anchorMax = minimapDisplay.rectTransform.anchorMax;
        fogDisplay.rectTransform.anchoredPosition = minimapDisplay.rectTransform.anchoredPosition;
        fogDisplay.rectTransform.sizeDelta = minimapDisplay.rectTransform.sizeDelta;
    }

    void LateUpdate()
    {
        // Center minimap camera on player
        Vector3 p = player.position;
        minimapCam.transform.position = new Vector3(p.x, p.y, minimapCam.transform.position.z);

        UpdateFogOfWar();
    }

    void UpdateFogOfWar()
    {
        float scaleX = fogResolutionX / mapWidth;
        float scaleY = fogResolutionY / mapHeight;

        int px = Mathf.FloorToInt((player.position.x + mapWidth * 0.5f) * scaleX);
        int py = Mathf.FloorToInt((player.position.y + mapHeight * 0.5f) * scaleY);

        // Mark visited tiles within view radius
        for (int dx = -viewRadiusTiles; dx <= viewRadiusTiles; dx++)
        {
            for (int dy = -viewRadiusTiles; dy <= viewRadiusTiles; dy++)
            {
                int x = px + dx;
                int y = py + dy;
                if (x >= 0 && x < fogResolutionX && y >= 0 && y < fogResolutionY)
                    visited[x, y] = true;
            }
        }

        // Redraw fog texture
        for (int x = 0; x < fogResolutionX; x++)
        {
            for (int y = 0; y < fogResolutionY; y++)
            {
                Color c;
                if (!visited[x, y])
                    c = unseenColor;
                else if (Mathf.Abs(x - px) <= viewRadiusTiles && Mathf.Abs(y - py) <= viewRadiusTiles)
                    c = visibleColor;
                else
                    c = visitedColor;

                fogTexture.SetPixel(x, y, c);
            }
        }
        fogTexture.Apply();
    }
}