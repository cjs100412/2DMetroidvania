using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using static UnityEditor.PlayerSettings;

[System.Serializable]
public class PlayerData
{
    public string sceneName;
    public float posX, posY;
    public int hp;
    public int mp;
    public int coins;
}

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    string savePath;
    PlayerData data;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        data = new PlayerData();
        if (File.Exists(savePath))
            File.Delete(savePath);
        InitializeDefaultSave();
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
}
