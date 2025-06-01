using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string sceneName;
    public float posX, posY;
    public int hp;
    public int mp;
    public int coins;

    // 보스 처치 여부를 저장할 리스트
    public List<string> defeatedBosses = new List<string>();

    // 벽 파괴 여부를 저장할 리스트
    public List<string> destroyedWalls = new List<string>();

    // 쇼핑 아이템 구매 여부
    public bool boughtAttackPower = false;
    public bool boughtAttackRange = false;
    public bool boughtAttackSpeed = false;
}

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    string savePath;
    PlayerData data;

    public int SavedCoins => data.coins;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        data = new PlayerData();

        //에디터에선 테스트용으로 삭제하지않는다
#if UNITY_EDITOR
        if (File.Exists(savePath))
            File.Delete(savePath);
#endif

        if (File.Exists(savePath))
        {
            // 기존 파일이 있으면 _삭제하지 않고_ 로드만 한다
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<PlayerData>(json);

            // 데이터 중 리스트가 null로 내려올 경우 대비
            if (data.defeatedBosses == null) data.defeatedBosses = new List<string>();
            if (data.destroyedWalls == null) data.destroyedWalls = new List<string>();

        }
        else
        {
            // 세이브 파일이 없을 때만 초기값 세팅
            InitializeDefaultSave();
        }

    }

    void InitializeDefaultSave()
    {
        // 현재 씬과 플레이어의 시작 위치, 최대 체력 등 원하는 기본값 세팅
        data.sceneName = SceneManager.GetActiveScene().name;
        data.posX = -44.84f;          // 예시 시작 위치 X
        data.posY = -1f;          // 예시 시작 위치 Y
        data.hp = 100;         // 기본 HP
        data.mp = 5;          // 기본 MP
        data.coins = 0;           // 기본 동전

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"[GameManager] Save initialized: {savePath}");
    }

    public void SaveGame(Vector2 pos, int hp, int mp, int coins)
    {
        data.sceneName = SceneManager.GetActiveScene().name;
        data.posX = pos.x;
        data.posY = pos.y;
        data.hp = hp;
        data.mp = mp;
        data.coins = coins;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved → " + savePath + " (Scene: " + data.sceneName + ")");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file.");
            return;
        }

        string json = File.ReadAllText(savePath);
        data = JsonUtility.FromJson<PlayerData>(json);
        Debug.Log("Game Loaded. Loading Scene: " + data.sceneName);

        // 씬을 비동기로 로드하고, 로드 완료 후에 위치·상태 복원
        StartCoroutine(LoadAndRestore());
    }

    IEnumerator LoadAndRestore()
    {
        // 씬 로드
        var op = SceneManager.LoadSceneAsync(data.sceneName);
        while (!op.isDone)
            yield return null;

        // 플레이어 오브젝트 찾기
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player not found after load!");
            yield break;
        }

        // 상태 복원
        Vector3 pos = new Vector3(data.posX, data.posY, 0f);
        int hp = data.hp;
        int mp = data.mp;
        int coins = data.coins;


        var inv = playerGO.GetComponent<PlayerInventory>();
        if (inv != null) inv.SpendCoins(inv.CoinCount); // 0으로 초기화
        if (inv != null) inv.AddCoins(data.coins);

        Debug.Log("Player state restored: HP=" + data.hp +
                  " MP=" + data.mp + " Coins=" + data.coins);

        var ph = playerGO.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.Respawn(pos, hp, mp);
        }
        else
        {
            // fallback: 직접 복원
            playerGO.transform.position = pos;
            var rb = playerGO.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
        }

        yield return null;
    }
    // 보스가 이미 처치되었는지를 확인
    public bool IsBossDefeated(string bossID)
    {
        return data.defeatedBosses.Contains(bossID);
    }

    // 보스를 처치했음을 기록하고 저장까지 수행
    public void SetBossDefeated(string bossID)
    {
        if (!data.defeatedBosses.Contains(bossID))
        {
            data.defeatedBosses.Add(bossID);
            // 변화된 상태를 파일에 바로 저장
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[GameManager] Boss defeated recorded: {bossID}");
        }
    }

    // 벽이 이미 파괴되었는지를 확인
    public bool IsWallDestroyed(string wallID)
    {
        return data.destroyedWalls.Contains(wallID);
    }

    // 벽 파괴를 기록하고 저장까지 수행
    public void SetWallDestroyed(string wallID)
    {
        if (!data.destroyedWalls.Contains(wallID))
        {
            data.destroyedWalls.Add(wallID);
            // 변화된 상태를 파일에 바로 저장
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[GameManager] Wall destroyed recorded: {wallID}");
        }
    }

    // 상점 아이템 상태
    public bool IsBoughtAttackPower() { return data.boughtAttackPower; }
    public bool IsBoughtAttackRange() { return data.boughtAttackRange; }
    public bool IsBoughtAttackSpeed() { return data.boughtAttackSpeed; }

    public void SetBoughtAttackPower()
    {
        if (!data.boughtAttackPower)
        {
            data.boughtAttackPower = true;
            SaveJSON();
            Debug.Log("[GameManager] Bought AttackPower");
        }
    }

    public void SetBoughtAttackRange()
    {
        if (!data.boughtAttackRange)
        {
            data.boughtAttackRange = true;
            SaveJSON();
            Debug.Log("[GameManager] Bought AttackRange");
        }
    }

    public void SetBoughtAttackSpeed()
    {
        if (!data.boughtAttackSpeed)
        {
            data.boughtAttackSpeed = true;
            SaveJSON();
            Debug.Log("[GameManager] Bought AttackSpeed");
        }
    }
    public void SetCoins(int newCoinCount)
    {
        data.coins = newCoinCount;
        SaveJSON();
        Debug.Log($"[GameManager] Coins updated to: {newCoinCount}");
    }

    // JSON 갱신만 담당하는 내부 함수
    void SaveJSON()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }
}
