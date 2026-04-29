using System.Collections;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public SingletonGameObject singletonComponent;
    public int playerCurrency;
    public int playerMaxLife;
    public int playerLife;
    public float runTime;

    public event System.Action<int> OnAddCoins;
    public event System.Action<int> OnPlayerHurt;
    public event System.Action OnPlayerDie;
    private void Awake()
    {
        if (singletonComponent.queuedToBeDestroyed) return;

        playerLife = playerMaxLife;

        Instance = this;
    }
    public int GetRuntime()
    {
        return Mathf.FloorToInt(runTime);
    }
    public void PlayerHurt(int damage)
    {
        playerLife = playerLife - damage;
        OnPlayerHurt?.Invoke(damage);
    }
    public void PlayerDie ()
    {
        playerLife = 0;
        OnPlayerDie?.Invoke();

        StartCoroutine(Sequence());

        IEnumerator Sequence ()
        {
            if (EnemyHand.Instance != null && EnemyHand.Instance.Grabbing)
            {
                yield break;
            }

            yield return new WaitForSeconds(3f);

            GameOver();
        }
    }
    public void GameOver ()
    {
        SceneTransitioner.LoadScene(GameDefinitions.SCENE_GAMEOVER, GameDefinitions.PickColor(0), 1, 1, 1);
    }
    public void VoidOut ()
    {
        SceneTransitioner.ReloadScene(GameDefinitions.PickColor(0), 1, 1, 1);
    }
    public void HandEndGrab()
    {
        if (playerLife > 0)
        {
            VoidOut();
        }
        else
        {
            GameOver();
        }
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
