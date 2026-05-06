using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public enum EnemyType { Patrol, Stationary, Boss }

    [System.Serializable]
    public class SpawnEntry
    {
        public Transform spawnPoint;
        public EnemyType enemyType;
    }

    [Header("Factory Reference")]
    public AIEnemyFactory factory;

    [Header("Spawn Entries")]
    public SpawnEntry[] entries;

    private void Start()
    {
        if (factory == null)
        {
            Debug.LogError("[EnemySpawner] No AIEnemyFactory assigned!");
            return;
        }

        SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (SpawnEntry entry in entries)
        {
            if (entry.spawnPoint == null) continue;

            Vector3    pos = entry.spawnPoint.position;
            Quaternion rot = entry.spawnPoint.rotation;

            switch (entry.enemyType)
            {
                case EnemyType.Patrol:
                    factory.CreatePatrolEnemy(pos, rot);
                    break;
                case EnemyType.Stationary:
                    factory.CreateStationaryEnemy(pos, rot);
                    break;
                case EnemyType.Boss:
                    factory.CreateBossEnemy(pos, rot);
                    break;
            }
        }
    }
}
