using TMPro;
using UnityEngine;

[RequireComponent(typeof(ObjectSounds))]
public class UIOptionBox : MonoBehaviour
{
    public PlayerInput uiInput;
    [Space]
    public TextMeshProUGUI header;
    public TextMeshProUGUI optionA;
    public TextMeshProUGUI optionB;
    public RectTransform marker;
    [HideInInspector]
    public ObjectSounds sound;

    protected int currentSelected;
    protected System.Action onOptionA;
    protected System.Action onOptionB;

    private void Awake()
    {
        sound = GetComponent<ObjectSounds>();

        uiInput.GetButton("ButtonSouth").onPressedDown += PressButton;
        uiInput.GetButton("ButtonWest").onPressedDown += PressButton;
        uiInput.GetButton("Move").onPressedDown += PressButton;
    }
    private void OnEnable()
    {
        SetCurrentSelected(0);
    }
    public void Show(string header, string optionA, string optionB, 
        System.Action onOptionA, System.Action onOptionB)
    {
        gameObject.SetActive(true);

        SetContent(header, optionA, optionB);

        this.onOptionA = onOptionA;
        this.onOptionB = onOptionB;
    }
    public void PressButton()
    {
        if (currentSelected == 0)
        {
            onOptionA();
        }
        else
        {
            onOptionB();
        }
    }
    public void PressAxis ()
    {
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
