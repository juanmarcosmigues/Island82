using System.Collections;
using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    public float force;
    public float radius;

    private Coin[] coins;
    private int aliveCoins;

    private void Awake()
    {
        coins = GetComponentsInChildren<Coin>();
    }

    private void OnEnable()
    {
        StartCoroutine(Drop());
    }
    IEnumerator Drop ()
    {
        yield return null;

        float angle = 360 / (coins.Length);
        Vector3 forward = Camera.main.RotateTowardsCamera(new Vector2(0, 1f));
        for (int i = 0; i < coins.Length; i++)
        {
            Vector3 f = Quaternion.AngleAxis(angle * i, Vector3.up) * forward;
            f = f.normalized;

            coins[i].transform.localPosition = Vector3.zero;
            coins[i].transform.SetParent(null);

            coins[i].transform.position += f * radius;
            coins[i].SetVelocity((f * 0.35f + Vector3.up).normalized * force);

            coins[i].OnPickUp += ReturnCoin;
        }
        aliveCoins = coins.Length;
    }
    private void ReturnCoin (Coin c)
    {
        c.OnPickUp -= ReturnCoin;
        c.transform.SetParent(transform);
        c.transform.localPosition = Vector3.zero;

        aliveCoins--;

        if (aliveCoins <= 0)
            gameObject.SetActive(false);
    }
}
