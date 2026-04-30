using System.Collections;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public SingletonGameObject singletonComponent;

    public int playerCurrency;
    public int playerLife;
    public int playerJumps;
    public float runTime;

    public SaveFile.Data lastSave;

    public event System.Action<int> OnAddCoins;
    public event System.Action<int> OnPlayerHurt;
    public event System.Action OnPlayerDie;
    private void Awake()
    {
        if (singletonComponent.queuedToBeDestroyed) return;

        if (lastSave == null || lastSave.IsEmpty())
            LoadGame();

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
    public void PlayerJump ()
    {
        playerJumps++;
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

    public void GoToMainMenu()
    {
        SceneTransitioner.LoadScene(GameDefinitions.SCENE_TITLESCREEN, 1, 1, 1);
    }
    public void SaveGame (string savePoint)
    {
        SaveFile.Data d = new SaveFile.Data(savePoint, playerJumps, playerLife, playerCurrency);
        SaveFile.Save(d);
    }
    public SaveFile.Data LoadGame ()
    {
        SaveFile.Data d = SaveFile.Load();

        if (d.IsEmpty())
        {
            d = NewGame();
        }

        lastSave = d;

        playerJumps = d.jumps;
        playerLife = d.life;
        playerCurrency = d.lums;

        return d;
    }
    public SaveFile.Data NewGame ()
    {
        SaveFile.Data d = new SaveFile.Data();

        d.life = GameDefinitions.PLAYER_MAX_LIFE;
        d.savePoint = "Null";
        d.jumps = 0;
        d.lums = 0;

        return d;
    }

}
