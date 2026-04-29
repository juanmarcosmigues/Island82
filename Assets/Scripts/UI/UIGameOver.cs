using TMPro;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    public static UIGameOver Instance { get; private set; }

    public UIOptionBox optionBox;

    public static void SetInstance(UIGameOver instance) =>
    Instance = instance;

    public void Show ()
    {
        gameObject.SetActive(true);
        optionBox.Show("Continue?", "Yes", "No", Continue, GoToMainMenu, 
            UIOptionBox.OptionType.Positive, UIOptionBox.OptionType.Negative);
    }
    public void Continue ()
    {

    }
    public void GoToMainMenu ()
    {

    }
}
