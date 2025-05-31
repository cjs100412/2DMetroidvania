using UnityEngine;

public class BossBarrier : MonoBehaviour
{
    [Tooltip("연결된 보스의 bossID와 동일한 접두어로 설정 (예: Level1_BossA_Wall)")]
    public string wallID = "Level1_BossA_Wall";

    void Start()
    {
        // 씬이 로드될 때, 이미 파괴된 벽이라면 오브젝트를 파괴
        if (GameManager.I.IsWallDestroyed(wallID))
        {
            Destroy(this.gameObject);
            return;
        }
        // 아니면 평소처럼 지상에 남아 있게 함
    }
}