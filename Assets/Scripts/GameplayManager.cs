using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public SingletonGameObject singletonComponent;
    public int playerCurrency;
    public float runTime;

    public event System.Action<int> OnAddCoins;
    public event System.Action<Player, int> OnPlayerHurt;

    private void Awake()
    {
        if (singletonComponent.queuedToBeDestroyed) return;

        Instance = this;
    }
    public int GetRuntime()
    {
        return Mathf.FloorToInt(runTime);
    }
    public void PlayerHurt(Player player, int damage)
    {
        OnPlayerHurt?.Invoke(player, damage);
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
