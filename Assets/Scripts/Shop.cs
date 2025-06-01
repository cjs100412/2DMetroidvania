using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Shop : MonoBehaviour
{
    [Header("– 상점 UI 전체 패널 –")]
    [Tooltip("상점 UI 루트 오브젝트 (인스펙터에서 할당)")]
    public GameObject shopUI;

    [Header("– 아이템별 설정 –")]
    [Tooltip("공격력 상승 아이템 패널")]
    public GameObject panelAttackPower;
    [Tooltip("공격범위 상승 아이템 패널")]
    public GameObject panelAttackRange;
    [Tooltip("공격속도 상승 아이템 패널")]
    public GameObject panelAttackSpeed;

    [Header("– 상호작용 설정 –")]
    [Tooltip("플레이어가 근처에 있으면 true")]
    bool playerInRange = false;

    [Tooltip("상호작용 키 (기본 F)")]
    public KeyCode interactKey = KeyCode.F;

    [Header("– 아이템 가격 –")]
    public int costAttackPower = 100;
    public int costAttackRange = 100;
    public int costAttackSpeed = 100;

    // 내부 참조
    private Button btnAttackPower;
    private Button btnAttackRange;
    private Button btnAttackSpeed;

    private Image overlayAttackPower;
    private Image overlayAttackRange;
    private Image overlayAttackSpeed;

    private Text costTextAttackPower;
    private Text costTextAttackRange;
    private Text costTextAttackSpeed;

    private Text descTextAttackPower;
    private Text descTextAttackRange;
    private Text descTextAttackSpeed;

    void Start()
    {
        // 상점 UI는 처음에 숨겨 둔다
        if (shopUI != null)
            shopUI.SetActive(false);

        // === 아이템별 버튼과 오버레이, 텍스트 찾아오기 ===
        if (panelAttackPower != null)
        {
            btnAttackPower = panelAttackPower.GetComponentInChildren<Button>();
            overlayAttackPower = panelAttackPower.transform.Find("Overlay")?.GetComponent<Image>();
            costTextAttackPower = panelAttackPower.transform.Find("CostText")?.GetComponent<Text>();
            descTextAttackPower = panelAttackPower.transform.Find("DescText")?.GetComponent<Text>();

            // 가격과 설명 설정
            if (costTextAttackPower != null) costTextAttackPower.text = costAttackPower + " 코인";
            if (descTextAttackPower != null) descTextAttackPower.text = "공격력 + 3";
        }

        if (panelAttackRange != null)
        {
            btnAttackRange = panelAttackRange.GetComponentInChildren<Button>();
            overlayAttackRange = panelAttackRange.transform.Find("Overlay")?.GetComponent<Image>();
            costTextAttackRange = panelAttackRange.transform.Find("CostText")?.GetComponent<Text>();
            descTextAttackRange = panelAttackRange.transform.Find("DescText")?.GetComponent<Text>();

            if (costTextAttackRange != null) costTextAttackRange.text = costAttackRange + " 코인";
            if (descTextAttackRange != null) descTextAttackRange.text = "공격 범위 +1.5";
        }

        if (panelAttackSpeed != null)
        {
            btnAttackSpeed = panelAttackSpeed.GetComponentInChildren<Button>();
            overlayAttackSpeed = panelAttackSpeed.transform.Find("Overlay")?.GetComponent<Image>();
            costTextAttackSpeed = panelAttackSpeed.transform.Find("CostText")?.GetComponent<Text>();
            descTextAttackSpeed = panelAttackSpeed.transform.Find("DescText")?.GetComponent<Text>();

            if (costTextAttackSpeed != null) costTextAttackSpeed.text = costAttackSpeed + " 코인";
            if (descTextAttackSpeed != null) descTextAttackSpeed.text = "공격 속도 0.2초 감소";
        }

        // 구매 버튼 이벤트 연결
        if (btnAttackPower != null) btnAttackPower.onClick.AddListener(OnBuyAttackPower);
        if (btnAttackRange != null) btnAttackRange.onClick.AddListener(OnBuyAttackRange);
        if (btnAttackSpeed != null) btnAttackSpeed.onClick.AddListener(OnBuyAttackSpeed);

        // 이미 구매된 아이템이 있으면 UI를 회색처리
        UpdateUIBoughtStates();
    }

    void Update()
    {
        if (!playerInRange) return;

        // F키를 누르면 UI 토글
        if (Input.GetKeyDown(interactKey))
        {
            if (shopUI != null)
                shopUI.SetActive(!shopUI.activeSelf);

            // UI를 열 때마다, 혹시 게임머니가 바뀌었으면 버튼 상태 갱신
            if (shopUI.activeSelf)
                UpdateButtonInteractability();
        }
    }

    // 플레이어가 충돌 콜라이더에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    // 플레이어가 콜라이더를 벗어났을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUI != null)
                shopUI.SetActive(false);
        }
    }

    // 상점 UI를 열 때마다(또는 구매 직후) 버튼 활성/비활성 갱신
    void UpdateButtonInteractability()
    {
        int coins = GameManager.I.SavedCoins;

        if (!GameManager.I.IsBoughtAttackPower())
            btnAttackPower.interactable = (coins >= costAttackPower);
        else
            btnAttackPower.interactable = false;

        if (!GameManager.I.IsBoughtAttackRange())
            btnAttackRange.interactable = (coins >= costAttackRange);
        else
            btnAttackRange.interactable = false;

        if (!GameManager.I.IsBoughtAttackSpeed())
            btnAttackSpeed.interactable = (coins >= costAttackSpeed);
        else
            btnAttackSpeed.interactable = false;
    }

    // 이미 구매된 아이템은 오버레이 띄우고 버튼 비활성
    void UpdateUIBoughtStates()
    {
        if (GameManager.I.IsBoughtAttackPower())
        {
            if (overlayAttackPower != null) overlayAttackPower.color = new Color(0, 0, 0, 0.5f);
            if (btnAttackPower != null) btnAttackPower.interactable = false;
        }
        if (GameManager.I.IsBoughtAttackRange())
        {
            if (overlayAttackRange != null) overlayAttackRange.color = new Color(0, 0, 0, 0.5f);
            if (btnAttackRange != null) btnAttackRange.interactable = false;
        }
        if (GameManager.I.IsBoughtAttackSpeed())
        {
            if (overlayAttackSpeed != null) overlayAttackSpeed.color = new Color(0, 0, 0, 0.5f);
            if (btnAttackSpeed != null) btnAttackSpeed.interactable = false;
        }
    }

    // “공격력 상승” 구매 시
    void OnBuyAttackPower()
    {
        int coins = GameManager.I.SavedCoins;
        if (coins < costAttackPower) return;

        // 1) 코인 차감
        var inv = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
        if (inv != null)
            inv.SpendCoins(costAttackPower);

        // 2) GameManager에 “공격력 구매 완료”로 기록
        GameManager.I.SetBoughtAttackPower();

        // 3) UI 업데이트 (버튼 비활성, 오버레이)
        UpdateUIBoughtStates();

        var pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        if (pm != null) pm.ApplyShopUpgrades();
    }

    // “공격 범위 상승” 구매 시
    void OnBuyAttackRange()
    {
        int coins = GameManager.I.SavedCoins;
        if (coins < costAttackRange) return;

        var inv = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
        if (inv != null)
            inv.SpendCoins(costAttackRange);

        GameManager.I.SetBoughtAttackRange();

        UpdateUIBoughtStates();

        var pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        if (pm != null) pm.ApplyShopUpgrades();
    }

    // “공격 속도 상승” 구매 시
    void OnBuyAttackSpeed()
    {
        int coins = GameManager.I.SavedCoins;
        if (coins < costAttackSpeed) return;

        var inv = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
        if (inv != null)
            inv.SpendCoins(costAttackSpeed);

        GameManager.I.SetBoughtAttackSpeed();

        UpdateUIBoughtStates();

        var pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        if (pm != null) pm.ApplyShopUpgrades();
    }
}
