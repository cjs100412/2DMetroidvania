//// MinimapIcon.cs
//using UnityEngine;
//using UnityEngine.UI;

//public class MinimapIcon : MonoBehaviour
//{
//    [Header("Icon Settings")]
//    public Color iconColor = Color.red;    // �� �������� ����
//    public float iconSize = 6f;           // �� ������ ũ��

//    private Transform target;             // Enemy �ڽ��� Transform
//    private RectTransform iconsParent;    // MinimapController.iconsParent ����
//    private MinimapController minimapController;

//    private RectTransform iconRect;
//    private Image iconImage;

//    void Start()
//    {
//        // 1) �ڱ� �ڽ�(Enemy)�� Transform�� target���� ����
//        target = transform;

//        // 2) MinimapController �ν��Ͻ��� ���ٸ� ���� �α�
//        if (MinimapController.Instance == null)
//        {
//            Debug.LogError("[MinimapIcon] MinimapController�� ���� �����ϴ�!");
//            enabled = false;
//            return;
//        }

//        minimapController = MinimapController.Instance;

//        // 3) ������ �θ�(iconsParent)�� MinimapController �Ӽ����� ��������
//        iconsParent = minimapController.iconsParent;
//        if (iconsParent == null)
//        {
//            Debug.LogError("[MinimapIcon] MinimapController.iconsParent�� �Ҵ���� �ʾҽ��ϴ�!");
//            enabled = false;
//            return;
//        }

//        // 4) ȭ�鿡 ǥ���� ������ ����
//        GameObject go = new GameObject(this.name + "_MinimapIcon");
//        go.transform.SetParent(iconsParent, false);
//        iconImage = go.AddComponent<Image>();
//        iconImage.color = iconColor;
//        iconRect = iconImage.rectTransform;
//        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
//    }

//    void LateUpdate()
//    {
//        if (minimapController == null) return;

//        // World ��ǥ �� 0~1 normalized ��ǥ (�� ���� ���)
//        float mapW = minimapController.mapW;
//        float mapH = minimapController.mapH;

//        float nx = (target.position.x + mapW * 0.5f) / mapW;
//        float ny = (target.position.y + mapH * 0.5f) / mapH;

//        nx = Mathf.Clamp01(nx);
//        ny = Mathf.Clamp01(ny);

//        // ���̾ƿ� ��� (iconsParent�� ���� �� ��ġ)
//        RectTransform miniRT = minimapController.minimapDisplay.rectTransform;
//        float w = miniRT.rect.width;
//        float h = miniRT.rect.height;

//        float localX = (nx - 0.5f) * w;
//        float localY = (ny - 0.5f) * h;

//        iconRect.localPosition = new Vector3(localX, localY, 0f);
//    }

//    private void OnDestroy()
//    {
//        // Enemy�� �ı��� ��, �̴ϸ� �����ܵ� �Բ� ����
//        if (iconRect != null)
//            Destroy(iconRect.gameObject);
//    }
//}
