using UnityEngine;

public abstract class AbstractEnemyFactory : MonoBehaviour
{

    public abstract EnemyBase CreateEnemy(Vector3 position, Quaternion rotation);
}
