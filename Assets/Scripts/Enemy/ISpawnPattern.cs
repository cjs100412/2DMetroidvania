using UnityEngine;

public interface ISpawnPattern : IBossPattern
{
    void SetSpawnPoint(Transform spawnPoint);
}
