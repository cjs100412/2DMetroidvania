//// MinimapIcon.cs
//using UnityEngine;
//using UnityEngine.UI;

//public class MinimapIcon : MonoBehaviour
//{
//    [Header("Icon Settings")]
//    public Color iconColor = Color.red;    // 적 아이콘은 빨강
//    public float iconSize = 6f;           // 적 아이콘 크기

//    private Transform target;             // Enemy 자신의 Transform
//    private RectTransform iconsParent;    // MinimapController.iconsParent 참조
//    private MinimapController minimapController;

//    private RectTransform iconRect;
//    private Image iconImage;

//    void Start()
//    {
//        // 1) 자기 자신(Enemy)의 Transform을 target으로 지정
//        target = transform;

//        // 2) MinimapController 인스턴스가 없다면 에러 로그
//        if (MinimapController.Instance == null)
//        {
//            Debug.LogError("[MinimapIcon] MinimapController가 씬에 없습니다!");
//            enabled = false;
//            return;
//        }

//        minimapController = MinimapController.Instance;

//        // 3) 아이콘 부모(iconsParent)도 MinimapController 속성에서 가져오기
//        iconsParent = minimapController.iconsParent;
//        if (iconsParent == null)
//        {
//            Debug.LogError("[MinimapIcon] MinimapController.iconsParent가 할당되지 않았습니다!");
//            enabled = false;
//            return;
//        }

//        // 4) 화면에 표시할 아이콘 생성
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

//        // World 좌표 → 0~1 normalized 좌표 (맵 범위 대비)
//        float mapW = minimapController.mapW;
//        float mapH = minimapController.mapH;

//        float nx = (target.position.x + mapW * 0.5f) / mapW;
//        float ny = (target.position.y + mapH * 0.5f) / mapH;

//        nx = Mathf.Clamp01(nx);
//        ny = Mathf.Clamp01(ny);

//        // 레이아웃 계산 (iconsParent의 영역 내 위치)
//        RectTransform miniRT = minimapController.minimapDisplay.rectTransform;
//        float w = miniRT.rect.width;
//        float h = miniRT.rect.height;

//        float localX = (nx - 0.5f) * w;
//        float localY = (ny - 0.5f) * h;

//        iconRect.localPosition = new Vector3(localX, localY, 0f);
//    }

//    private void OnDestroy()
//    {
//        // Enemy가 파괴될 때, 미니맵 아이콘도 함께 제거
//        if (iconRect != null)
//            Destroy(iconRect.gameObject);
//    }
//}
