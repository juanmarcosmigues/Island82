using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHud : MonoBehaviour
{
    public static UIHud Instance { get; private set; }

    public TextMeshProUGUI tmpClock;
    public TextMeshProUGUI tmpCoins;
    public GameObject[] lifePoints;
    private int hours, minutes, seconds;
    private string clockDots = ":";

    private void Awake()
    {
        SingletonGameObject parent = GetComponentInParent<SingletonGameObject>();
        if (parent != null)
        {
            if (parent.queuedToBeDestroyed)
                return;
        }

        Instance = this;
    }
    private void Start()
    {
        GameplayManager.Instance.OnAddCoins += AddCoins;
        Player.Instance.OnGetHurt += PlayerHurt;
    }
    private void OnDestroy()
    {
        GameplayManager.Instance.OnAddCoins -= AddCoins;
        Player.Instance.OnGetHurt -= PlayerHurt;
    }
    private void Update()
    {
        int s = GameplayManager.Instance.GetRuntime();
        if (seconds != s)
        {
            seconds = s;
            minutes = Mathf.FloorToInt(seconds / 60f);
            hours = Mathf.FloorToInt(minutes / 60f);
            clockDots = s % 2 == 0 ? ":" : " ";
        }

        tmpClock.text = $"{hours.ToString("00") + clockDots + minutes.ToString("00")}";
    }
    private void AddCoins (int amount)
    {
        tmpCoins.text = GameplayManager.Instance.playerCurrency.ToString("000");
    }
    private void PlayerHurt(int damage) 
    {
        for (int i = 0; i < lifePoints.Length; i++)
            lifePoints[i].SetActive(i < Player.Instance.CurrentLife);
    }
}
