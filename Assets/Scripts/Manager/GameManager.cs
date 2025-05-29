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
}
