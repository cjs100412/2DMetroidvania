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

    // �� ���� óġ ���θ� ������ ���ڿ� ����Ʈ
    public List<string> defeatedBosses = new List<string>();

    // �� �� �ı� ���θ� ������ ���ڿ� ����Ʈ
    public List<string> destroyedWalls = new List<string>();
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
    /// <summary>
    /// ������ �̹� óġ�Ǿ������� Ȯ��
    /// </summary>
    /// <param name="bossID">��: "Level1_BossA"</param>
    /// <returns>óġ�Ǿ����� true, �ƴϸ� false</returns>
    public bool IsBossDefeated(string bossID)
    {
        return data.defeatedBosses.Contains(bossID);
    }

    /// <summary>
    /// ������ óġ������ ����ϰ� ������� ����
    /// </summary>
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

    /// <summary>
    /// ���� �̹� �ı��Ǿ������� Ȯ��
    /// </summary>
    public bool IsWallDestroyed(string wallID)
    {
        return data.destroyedWalls.Contains(wallID);
    }

    /// <summary>
    /// �� �ı��� ����ϰ� ������� ����
    /// </summary>
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
}
