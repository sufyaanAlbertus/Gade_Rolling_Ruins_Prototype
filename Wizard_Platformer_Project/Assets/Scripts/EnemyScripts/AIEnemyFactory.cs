using UnityEngine;

public class AIEnemyFactory : AbstractEnemyFactory
{
    [Header("Enemy Prefabs")]
    [Tooltip("Prefab with PatrolEnemy script.")]
    public GameObject patrolEnemyPrefab;

    [Tooltip("Prefab with StationaryEnemy script.")]
    public GameObject stationaryEnemyPrefab;

    [Tooltip("Prefab with BossEnemy script — larger and faster.")]
    public GameObject bossEnemyPrefab;

    [Header("Default Stats per Type")]
    public float patrolSpeed     = 3.0f;
    public float stationarySpeed = 0f;
    public float bossSpeed       = 5.5f;

    public float patrolSize      = 1.0f;
    public float stationarySize  = 1.2f;
    public float bossSize        = 1.8f;

    public Color patrolColor     = Color.red;
    public Color stationaryColor = Color.yellow;
    public Color bossColor       = Color.magenta;

    // -----------------------------------------------------------------------
    // Factory methods — each returns a specific enemy type
    // -----------------------------------------------------------------------

 
    public override EnemyBase CreateEnemy(Vector3 position, Quaternion rotation)
    {
        return CreatePatrolEnemy(position, rotation);
    }

    public EnemyBase CreatePatrolEnemy(Vector3 position, Quaternion rotation)
    {
        if (patrolEnemyPrefab == null)
        {
            Debug.LogError("[AIEnemyFactory] patrolEnemyPrefab not assigned!");
            return null;
        }

        GameObject obj = Instantiate(patrolEnemyPrefab, position, rotation);
        PatrolEnemy enemy = obj.GetComponent<PatrolEnemy>();
        if (enemy != null)
        {
            enemy.speed      = patrolSpeed;
            enemy.size       = patrolSize;
            enemy.enemyColor = patrolColor;
        }
        Debug.Log("[AIEnemyFactory] Patrol enemy created.");
        return enemy;
    }

    public EnemyBase CreateStationaryEnemy(Vector3 position, Quaternion rotation)
    {
        if (stationaryEnemyPrefab == null)
        {
            Debug.LogError("[AIEnemyFactory] stationaryEnemyPrefab not assigned!");
            return null;
        }

        GameObject obj = Instantiate(stationaryEnemyPrefab, position, rotation);
        StationaryEnemy enemy = obj.GetComponent<StationaryEnemy>();
        if (enemy != null)
        {
            enemy.speed      = stationarySpeed;
            enemy.size       = stationarySize;
            enemy.enemyColor = stationaryColor;
        }
        Debug.Log("[AIEnemyFactory] Stationary enemy created.");
        return enemy;
    }

    public EnemyBase CreateBossEnemy(Vector3 position, Quaternion rotation)
    {
        if (bossEnemyPrefab == null)
        {
            Debug.LogError("[AIEnemyFactory] bossEnemyPrefab not assigned!");
            return null;
        }

        GameObject obj = Instantiate(bossEnemyPrefab, position, rotation);
        BossEnemy enemy = obj.GetComponent<BossEnemy>();
        if (enemy != null)
        {
            enemy.speed      = bossSpeed;
            enemy.size       = bossSize;
            enemy.enemyColor = bossColor;
        }
        Debug.Log("[AIEnemyFactory] Boss enemy created.");
        return enemy;
    }
}
