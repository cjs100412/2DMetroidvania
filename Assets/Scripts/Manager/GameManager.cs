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

    // ���� óġ ���θ� ������ ����Ʈ
    public List<string> defeatedBosses = new List<string>();

    // �� �ı� ���θ� ������ ����Ʈ
    public List<string> destroyedWalls = new List<string>();

    // ���� ������ ���� ����
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

        //�����Ϳ��� �׽�Ʈ������ ���������ʴ´�
#if UNITY_EDITOR
        if (File.Exists(savePath))
            File.Delete(savePath);
#endif

        if (File.Exists(savePath))
        {
            // ���� ������ ������ _�������� �ʰ�_ �ε常 �Ѵ�
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<PlayerData>(json);

            // ������ �� ����Ʈ�� null�� ������ ��� ���
            if (data.defeatedBosses == null) data.defeatedBosses = new List<string>();
            if (data.destroyedWalls == null) data.destroyedWalls = new List<string>();

        }
        else
        {
            // ���̺� ������ ���� ���� �ʱⰪ ����
            InitializeDefaultSave();
        }

    }

    void InitializeDefaultSave()
    {
        // ���� ���� �÷��̾��� ���� ��ġ, �ִ� ü�� �� ���ϴ� �⺻�� ����
        data.sceneName = SceneManager.GetActiveScene().name;
        data.posX = -44.84f;          // ���� ���� ��ġ X
        data.posY = -1f;          // ���� ���� ��ġ Y
        data.hp = 100;         // �⺻ HP
        data.mp = 5;          // �⺻ MP
        data.coins = 0;           // �⺻ ����

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
        Debug.Log("Game Saved �� " + savePath + " (Scene: " + data.sceneName + ")");
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

        // ���� �񵿱�� �ε��ϰ�, �ε� �Ϸ� �Ŀ� ��ġ������ ����
        StartCoroutine(LoadAndRestore());
    }

    IEnumerator LoadAndRestore()
    {
        // �� �ε�
        var op = SceneManager.LoadSceneAsync(data.sceneName);
        while (!op.isDone)
            yield return null;

        // �÷��̾� ������Ʈ ã��
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player not found after load!");
            yield break;
        }

        // ���� ����
        Vector3 pos = new Vector3(data.posX, data.posY, 0f);
        int hp = data.hp;
        int mp = data.mp;
        int coins = data.coins;


        var inv = playerGO.GetComponent<PlayerInventory>();
        if (inv != null) inv.SpendCoins(inv.CoinCount); // 0���� �ʱ�ȭ
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
            // fallback: ���� ����
            playerGO.transform.position = pos;
            var rb = playerGO.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
        }

        yield return null;
    }
    // ������ �̹� óġ�Ǿ������� Ȯ��
    public bool IsBossDefeated(string bossID)
    {
        return data.defeatedBosses.Contains(bossID);
    }

    // ������ óġ������ ����ϰ� ������� ����
    public void SetBossDefeated(string bossID)
    {
        if (!data.defeatedBosses.Contains(bossID))
        {
            data.defeatedBosses.Add(bossID);
            // ��ȭ�� ���¸� ���Ͽ� �ٷ� ����
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[GameManager] Boss defeated recorded: {bossID}");
        }
    }

    // ���� �̹� �ı��Ǿ������� Ȯ��
    public bool IsWallDestroyed(string wallID)
    {
        return data.destroyedWalls.Contains(wallID);
    }

    // �� �ı��� ����ϰ� ������� ����
    public void SetWallDestroyed(string wallID)
    {
        if (!data.destroyedWalls.Contains(wallID))
        {
            data.destroyedWalls.Add(wallID);
            // ��ȭ�� ���¸� ���Ͽ� �ٷ� ����
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[GameManager] Wall destroyed recorded: {wallID}");
        }
    }

    // ���� ������ ����
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

    // JSON ���Ÿ� ����ϴ� ���� �Լ�
    void SaveJSON()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }
}
