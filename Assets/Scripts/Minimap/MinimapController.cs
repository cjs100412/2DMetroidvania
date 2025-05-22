using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RawImage minimapDisplay;   // UI RawImage to show RenderTexture

    [Header("Map Bounds (World Units)")]
    public float mapWidth = 50f;
    public float mapHeight = 30f;

    [Header("Fog of War")]
    public int fogResolutionX = 100;
    public int fogResolutionY = 60;
    public Color unseenColor = Color.black;
    public Color visitedColor = Color.gray;
    public Color visibleColor = new Color(0.6f, 0.4f, 0.2f, 1f);
    public int viewRadiusTiles = 5;

    private Camera minimapCam;
    private Texture2D fogTexture;
    private bool[,] visited;

    void Start()
    {
        minimapCam = GetComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.targetTexture = new RenderTexture(fogResolutionX, fogResolutionY, 16);
        minimapDisplay.texture = minimapCam.targetTexture;

        fogTexture = new Texture2D(fogResolutionX, fogResolutionY);
        visited = new bool[fogResolutionX, fogResolutionY];
        minimapDisplay.material = new Material(Shader.Find("Unlit/Transparent"));
        minimapDisplay.material.mainTexture = fogTexture;
    }

    void LateUpdate()
    {
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

        for (int dx = -viewRadiusTiles; dx <= viewRadiusTiles; dx++)
            for (int dy = -viewRadiusTiles; dy <= viewRadiusTiles; dy++)
            {
                int x = px + dx;
                int y = py + dy;
                if (x >= 0 && x < fogResolutionX && y >= 0 && y < fogResolutionY)
                    visited[x, y] = true;
            }

        for (int x = 0; x < fogResolutionX; x++)
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
        fogTexture.Apply();
    }
}