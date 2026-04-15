using TMPro;
using static GameDefinitions;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIDialogueBox : MonoBehaviour, IInteractable
{
    public static UIDialogueBox Instance { get; private set; }

    public bool Consumable => true;
    public string InteractionName => "Dialogue Box Next";

    public TextMeshProUGUI textComponentHeader;
    public TextMeshProUGUI textComponentBody;
    public PlayerInput uiInput;
    public Image marker;
    public Sprite[] markerSprites;
    public AudioSource dialogueSound;

    private string goalBody = "";
    private string goalHeader = "";
    protected string currentText = "";
    protected float currentTime = 0f;
    protected int currentCharacterAmount = 0; 
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
        //uiInput.GetButton("ButtonSouth").onPressedDown += PressNext;
        textComponentHeader.text = "";
        textComponentBody.text = "";
        marker.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        //uiInput.GetButton("ButtonSouth").onPressedDown -= PressNext;
    }
    public virtual void Show(params Dialogue[] texts)
    {
        steps = new();
        texts.ForEach(t => { steps.Add(() => Show(t.header, t.body, t.aligment)); });
        steps.Add(() => Hide());
        Next();
    }
    public virtual void Show (params (string header, string body)[] texts)
    {
        Dialogue[] dialogues = new Dialogue[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            dialogues[i].body = texts[i].body;
            dialogues[i].header = texts[i].header;
            dialogues[i].aligment = TextAlignmentOptions.TopJustified;
        }

        Show(dialogues);
    }
    public virtual void Show (string header, string body, TextAlignmentOptions aligment)
    {
        goalBody = body;
        goalHeader = header;
        currentText = "";
        currentTime = 0f;
        textComponentBody.alignment = aligment;
        textComponentHeader.alignment = aligment;

        Show();
    }
    public void Show()
    {
        gameObject.SetActive(true);

        if (Player.Instance != null)
            Player.Instance.PlayerInControl = false;

        revealIndices = new();
        for (int i = 0; i < goalBody.Length; i++)
        {
            if (goalBody[i] != ' ')
                revealIndices.Add(i);
        }

        textComponentHeader.gameObject.SetActive(goalHeader.Length > 0);
        textComponentHeader.text = goalHeader;
    }
    public void Hide ()
    {
        gameObject.SetActive(false);

        if (Player.Instance != null)
            Player.Instance.PlayerInControl = true;
    }
    void Next ()
    {
        if (steps.Count == 0) return;

        steps[0]();
        steps.RemoveAt(0);

        marker.sprite = steps.Count > 1 ? markerSprites[0] : markerSprites[1];

        if (steps.Count > 0)
            Player.Instance.AddInteraction(this);
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

    public void Interact()
    {
        if (Writing)
            return;

        Next();
    }
}
