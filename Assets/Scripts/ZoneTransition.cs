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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.Portal);
        }
        // ������ �ε��� ��/������ġ ����
        SceneLoader.NextZone = targetZoneScene;
        SceneLoader.NextSpawnPoint = spawnPointName;
        // Bootstrap ���� Single ���� �ε� -> ���� ��/Bootstrap ��� ��ε�
        SceneLoader.IsRespawn = false;
        SceneManager.LoadScene("Bootstrap", LoadSceneMode.Additive);
    }
}