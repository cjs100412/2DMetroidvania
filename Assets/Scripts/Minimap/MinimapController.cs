using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("References")]
    [Tooltip("→ 씬에서 태그가 'Player'인 오브젝트의 Transform을 드래그하세요.")]
    public Transform player;

    [Tooltip("→ 미니맵용 RawImage를 드래그하세요.")]
    public RawImage minimapDisplay;

    [Header("Rendering Layers")]
    [Tooltip("→ 'Ground' 레이어가 체크된 LayerMask")]
    public LayerMask groundLayer;

    [Header("Map Bounds (World Units)")]
    [Tooltip("→ 전체 지형 너비(월드 유닛)")]
    public float mapWidth = 50f;
    [Tooltip("→ 전체 지형 높이(월드 유닛)")]
    public float mapHeight = 30f;

    private Camera minimapCam;
    private float mapW, mapH;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        minimapCam = GetComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.cullingMask = groundLayer;

        mapW = mapWidth;
        mapH = mapHeight;
    }

    void Start()
    {
        // 만약 Inspector에 player가 비어 있으면 태그로 찾아본다
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }

        // 렌더 텍스처 생성 (해상도는 예시: mapWidth×10 px, mapHeight×10 px)
        int resX = Mathf.CeilToInt(mapWidth * 10f);
        int resY = Mathf.CeilToInt(mapHeight * 10f);

        RenderTexture rt = new RenderTexture(resX, resY, 16);
        minimapCam.targetTexture = rt;

        if (minimapDisplay != null)
        {
            minimapDisplay.texture = rt;
        }
    }

    void LateUpdate()
    {
        // player가 아직 null이면 계속 찾아본다
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null)
                player = go.transform;
            else
                return;
        }

        // 플레이어를 따라 카메라를 이동 (z는 고정)
        Vector3 p = player.position;
        minimapCam.transform.position = new Vector3(p.x, p.y, minimapCam.transform.position.z);
    }
}
