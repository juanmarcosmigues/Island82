using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ObjectSounds))]
public class UIOptionBox : MonoBehaviour
{
    public enum OptionType { Neutral, Positive, Negative }  

    public PlayerInput uiInput;
    [Space]
    public TextMeshProUGUI header;
    public TextMeshProUGUI optionA;
    public TextMeshProUGUI optionB;
    public RectTransform marker;
    public float holdOnAnswerTime;

    [HideInInspector]
    public ObjectSounds sound;

    protected int currentSelected;
    protected System.Action onOptionA;
    protected System.Action onOptionB;
    protected OptionType optionTypeA;
    protected OptionType optionTypeB;
    protected bool inputEnabled;

    private void Awake()
    {
        sound = GetComponent<ObjectSounds>();

        uiInput.GetButton("ButtonSouth").onPressedDown += PressButton;
        uiInput.GetButton("ButtonWest").onPressedDown += PressButton;
        uiInput.GetButton("Move").onPressedDown += PressAxis;
    }
    private void OnEnable()
    {
        SetCurrentSelected(0);
    }
    private string GetSound (OptionType optionType)
    {
        return optionType switch
        {
            OptionType.Neutral => "NeutralOption",
            OptionType.Positive => "PositiveOption",
            OptionType.Negative => "NegativeOption",
            _ => "NeutralOption"
        };
    }
    public void Show(string header, string optionA, string optionB, 
        System.Action onOptionA, System.Action onOptionB, 
        OptionType optionTypeA = OptionType.Neutral,
        OptionType optionTypeB = OptionType.Neutral)
    {
        gameObject.SetActive(true);

        SetContent(header, optionA, optionB);

        this.onOptionA = onOptionA;
        this.onOptionB = onOptionB;
        this.optionTypeA = optionTypeA;
        this.optionTypeB = optionTypeB;

        marker.GetComponent<Blink>().enabled = false;
        marker.gameObject.SetActive(true);

        inputEnabled = true;
    }
    public void PressButton()
    {
        if (!inputEnabled) return;

        marker.GetComponent<Blink>().enabled = true;
        inputEnabled = false;

        sound.PlaySound(currentSelected == 0 ? GetSound(optionTypeA) : GetSound(optionTypeB));

        StartCoroutine(_());

        IEnumerator _ ()
        {
            yield return new WaitForSeconds(holdOnAnswerTime);

            if (currentSelected == 0)
            {
                onOptionA();
            }
            else
            {
                onOptionB();
            }
        }
    }
    public void PressAxis ()
    {
        if (!inputEnabled) return;

        var dir = (uiInput.GetButton("Move") as InputAxisAsButton).currentDirection;
        Move(dir);
    }
    public void Move(InputAxisAsButton.Direction direction)
    {
        int selected = currentSelected;

        if ((int)direction == 1 ||
            (int)direction == 2 ||
            (int)direction == 3)
        {
            selected--;
        }
        else if ((int)direction == 5 ||
            (int)direction == 6 ||
            (int)direction == 7)
        {
            selected++;
        }
        selected =
            selected >= 2 ? 0 :
            selected <= -1 ? 1 :
            selected;

        SetCurrentSelected(selected);

        sound.PlaySound("Move");
    }
    public void SetCurrentSelected (int selected)
    {
        marker.SetParent(selected == 0 ? optionA.rectTransform : optionB.rectTransform);
        marker.anchoredPosition = Vector3.zero;
        currentSelected = selected;
    }
    public void SetContent (string header, string optionA, string optionB)
    {
        this.header.text = header;
        this.optionA.text = optionA;
        this.optionB.text = optionB;
    }
    
}
