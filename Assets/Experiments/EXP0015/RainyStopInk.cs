using System;
using Ink.Runtime;
using UnityEngine;

[RequireComponent(typeof(RainyStopInteraction))]
public sealed class RainyStopInk : MonoBehaviour
{
    [SerializeField] private TextAsset storyJson;
    [SerializeField] private Font dialogueFont;

    private RainyStopInteraction interaction;
    private Story story;
    private string dialogue;
    private GUIStyle textStyle;
    private GUIStyle choiceStyle;

    public event Action<Story> Advanced;

    private void Awake()
    {
        interaction = GetComponent<RainyStopInteraction>();
        if (storyJson == null || dialogueFont == null)
        {
            Debug.LogError("EXP-0015 requires the compiled Ink story and a dialogue font.", this);
            enabled = false;
            return;
        }

        story = new Story(storyJson.text);
        Advance();
    }

    private void OnEnable() => interaction.Interacted += OnInteraction;

    private void OnDisable() => interaction.Interacted -= OnInteraction;

    private void OnInteraction(string id)
    {
        if (story.currentTags.Contains("choice-only") ||
            (story.currentChoices.Count == 0 && !story.currentTags.Contains("await-world-click")))
            return;

        switch (id)
        {
            case "notice-board":
                story.ChoosePathString("notice_board");
                break;
            case "ticket-window":
                story.ChoosePathString("ticket_window");
                break;
            default:
                return;
        }

        Advance();
    }

    private void Choose(int index)
    {
        story.ChooseChoiceIndex(index);
        Advance();
    }

    public void ApplyFerryArrival(bool receivedSignal)
    {
        story.variablesState["arrived_with_signal"] = receivedSignal;
        story.ChoosePathString("waiting_room");
        Advance();
    }

    public void ContinueAtKnot(string knot)
    {
        story.ChoosePathString(knot);
        Advance();
    }

    private void Advance()
    {
        dialogue = story.ContinueMaximally().Trim();
        Advanced?.Invoke(story);
    }

    private void OnGUI()
    {
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label) { font = dialogueFont, wordWrap = true };
            choiceStyle = new GUIStyle(GUI.skin.button) { font = dialogueFont, wordWrap = true };
        }

        int fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 34f), 12, 24);
        textStyle.fontSize = choiceStyle.fontSize = fontSize;
        var panel = new Rect(12, Screen.height * 0.67f, Screen.width - 24, Screen.height * 0.33f - 8);
        GUILayout.BeginArea(panel, GUI.skin.box);
        GUILayout.Label(dialogue, textStyle);
        GUILayout.BeginHorizontal();
        int selected = -1;
        foreach (var choice in story.currentChoices)
            if (GUILayout.Button(choice.text, choiceStyle, GUILayout.MinHeight(fontSize * 2)))
                selected = choice.index;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (selected >= 0)
            Choose(selected);
    }
}
