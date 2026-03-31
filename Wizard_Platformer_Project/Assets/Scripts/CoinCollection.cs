using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    private int Coin = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "coin")
        {
            Coin++;
            Debug.Log(Coin);
            Destroy(other.gameObject);
        }
    }
}
