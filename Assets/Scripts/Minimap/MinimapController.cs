using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("References")]
    [Tooltip("�� ������ �±װ� 'Player'�� ������Ʈ�� Transform�� �巡���ϼ���.")]
    public Transform player;

    [Tooltip("�� �̴ϸʿ� RawImage�� �巡���ϼ���.")]
    public RawImage minimapDisplay;

    [Header("Rendering Layers")]
    [Tooltip("�� 'Ground' ���̾ üũ�� LayerMask")]
    public LayerMask groundLayer;

    [Header("Map Bounds (World Units)")]
    [Tooltip("�� ��ü ���� �ʺ�(���� ����)")]
    public float mapWidth = 50f;
    [Tooltip("�� ��ü ���� ����(���� ����)")]
    public float mapHeight = 30f;

    private Camera minimapCam;
    private float mapW, mapH;

    void Awake()
    {
        // �̱��� ����
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
        // ���� Inspector�� player�� ��� ������ �±׷� ã�ƺ���
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }

        // ���� �ؽ�ó ���� (�ػ󵵴� ����: mapWidth��10 px, mapHeight��10 px)
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
        // player�� ���� null�̸� ��� ã�ƺ���
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null)
                player = go.transform;
            else
                return;
        }

        // �÷��̾ ���� ī�޶� �̵� (z�� ����)
        Vector3 p = player.position;
        minimapCam.transform.position = new Vector3(p.x, p.y, minimapCam.transform.position.z);
    }
}
