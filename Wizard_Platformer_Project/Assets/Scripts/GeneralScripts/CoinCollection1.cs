using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("coin"))
        {
            GameManager.Instance?.AddCoin(1);
            Destroy(other.gameObject);
        }
    }
}