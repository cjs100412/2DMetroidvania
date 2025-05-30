public static class SceneLoader
{
    // 현재 활성화된 Zone 씬 이름
    public static string CurrentZone;
    // 다음에 로드할 Zone 씬 이름
    public static string NextZone;
    // 다음 Zone에서 플레이어를 스폰할 위치(SpawnPoint 오브젝트) 이름
    public static string NextSpawnPoint;

    public static string LastCheckpointZone;
    public static string LastCheckpointSpawn;
}