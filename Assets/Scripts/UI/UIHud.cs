using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHud : MonoBehaviour
{
    public TextMeshProUGUI tmpClock;
    public TextMeshProUGUI tmpCoins;
    private int hours, minutes, seconds;
    private string clockDots = ":";

    private void Start()
    {
        GameplayManager.Instance.OnAddCoins += AddCoins;
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
}
