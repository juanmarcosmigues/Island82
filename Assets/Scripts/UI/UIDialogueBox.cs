using TMPro;
using static GameDefinitions;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIDialogueBox : MonoBehaviour
{
    public static UIDialogueBox Instance { get; private set; }

    public TextMeshProUGUI textComponentHeader;
    public TextMeshProUGUI textComponentBody;
    public PlayerInput uiInput;
    public Image marker;
    public Sprite[] markerSprites;
    public AudioSource dialogueSound;

    private string goalBody;
    private string goalHeader;
    protected string currentText;
    protected float currentTime;
    protected int currentCharacterAmount; 
    protected List<int> revealIndices;
    protected List<System.Action> steps;
    public bool Writing => currentText.Length < goalBody.Length;

    private void Awake()
    {
        SingletonGameObject parent = GetComponentInParent<SingletonGameObject>();
        if (parent != null )
        {
            if (parent.queuedToBeDestroyed)
                return;
        }

        Instance = this;

        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        uiInput.GetButton("ButtonSouth").onPressedDown += PressNext;
        textComponentHeader.text = "";
        textComponentBody.text = "";
        marker.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        uiInput.GetButton("ButtonSouth").onPressedDown -= PressNext;
    }
    public virtual void Show (params (string header, string body)[] texts)
    {
        steps = new();
        texts.ForEach(t => { steps.Add(() => Show(t.header, t.body)); });
        steps.Add(() => Hide());
        Next();
    }
    public virtual void Show (string header, string body)
    {
        goalBody = body;
        goalHeader = header;
        currentText = "";
        currentTime = 0f;

        Show();
    }
    public void Show()
    {
        gameObject.SetActive(true);

        if (Player.Instance != null) 
            Player.Instance.input.inputEnabled = false;

        revealIndices = new();
        for (int i = 0; i < goalBody.Length; i++)
        {
            if (goalBody[i] != ' ')
                revealIndices.Add(i);
        }

        textComponentHeader.text = goalHeader;
    }
    public void Hide ()
    {
        gameObject.SetActive(false);

        if (Player.Instance != null)
            Player.Instance.input.inputEnabled = true;
    }
    public void PressNext ()
    {
        if (Writing)
            return;

        Next();
    }
    void Next ()
    {
        if (steps.Count == 0) return;

        steps[0]();
        steps.RemoveAt(0);

        marker.sprite = steps.Count > 1 ? markerSprites[0] : markerSprites[1];
    }
    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentText.Length < goalBody.Length)
        {
            if (marker.gameObject.activeSelf) 
                marker.gameObject.SetActive(false);

            var lastCharacterAmount = currentCharacterAmount;
            currentCharacterAmount = Mathf.Min(Mathf.CeilToInt(currentTime * UI_CHARACTERS_PER_SECOND), revealIndices.Count - 1);
            currentText = goalBody.Substring(0, revealIndices[currentCharacterAmount]+1);
            textComponentBody.text = currentText;

            if (lastCharacterAmount != currentCharacterAmount)
                dialogueSound.Play();
        }
        else
        {
            if (!marker.gameObject.activeSelf)
                marker.gameObject.SetActive(true);
        }
    }
}
