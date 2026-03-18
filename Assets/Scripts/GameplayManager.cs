using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public int playerCurrency;
    public float runTime;

    public event System.Action<int> OnAddCoins;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Player.Instance.OnPickUpCoin += AddCoins;
    }
    public int GetRuntime()
    {
        return Mathf.FloorToInt(runTime);
    }
    public void AddCoins (int amount)
    {
        playerCurrency += amount;
        OnAddCoins?.Invoke(amount);
    }
    void Update()
    {
        runTime += Time.deltaTime;
    }
}
