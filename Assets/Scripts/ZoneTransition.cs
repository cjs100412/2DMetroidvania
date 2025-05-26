using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneTransition : MonoBehaviour
{
    [Tooltip("�� ������ Ÿ�� �̵��� Zone �� �̸�")]
    public string targetZoneScene;
    [Tooltip("�� Zone �� ���ο��� �÷��̾ ��Ÿ�� SpawnPoint ������Ʈ �̸�")]
    public string spawnPointName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // ������ �ε��� ��/������ġ ����
        SceneLoader.NextZone = targetZoneScene;
        SceneLoader.NextSpawnPoint = spawnPointName;
        // Bootstrap ���� Single ���� �ε� -> ���� ��/Bootstrap ��� ��ε�
        SceneManager.LoadScene("Bootstrap", LoadSceneMode.Additive);
    }
}