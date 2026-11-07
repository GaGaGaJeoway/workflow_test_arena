using System;
using System.Linq;
using System.Reflection;
using Ink.UnityIntegration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ArenaCrossSceneSmoke
{
    private const string SetupKey = "Arena.CrossSceneSmoke.Setup";
    private const string CompileKey = "Arena.CrossSceneSmoke.Compile";
    private const string PlayKey = "Arena.CrossSceneSmoke.Play";
    private const string RestoreOptionsKey = "Arena.CrossSceneSmoke.RestoreOptions";
    private const string DeadlineKey = "Arena.CrossSceneSmoke.Deadline";
    private const string Source = "Assets/Scenes/Experiments/EXP0019/EXP0019_SignalDeparture.unity";
    private const string Destination = "Assets/Scenes/Experiments/EXP0017/EXP0017_NightFerry.unity";
    private const string SourceInk = "Assets/Experiments/EXP0019/Story/EXP0019_SignalDeparture.ink";
    private const string DestinationInk = "Assets/Experiments/EXP0017/Story/EXP0017_NightFerry.ink";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static bool inputSettingsChanged;
    private static InputSettings oldInputSettings;
    private static bool oldRunInBackground;

    static ArenaCrossSceneSmoke()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
        EditorApplication.quitting += RestoreSettings;
        if (SessionState.GetBool(SetupKey, false)) EditorApplication.update += Prepare;
    }

    public static void Run()
    {
        var revision = Environment.GetEnvironmentVariable("ARENA_SMOKE_REVISION");
        Require(!string.IsNullOrEmpty(revision), "Set ARENA_SMOKE_REVISION from git rev-parse HEAD.");
        Debug.Log($"ARENA_SMOKE source={revision} unity={Application.unityVersion}");
        SessionState.SetBool(RestoreOptionsKey, EditorSettings.enterPlayModeOptionsEnabled);
        if (EditorSettings.enterPlayModeOptionsEnabled) EditorSettings.enterPlayModeOptionsEnabled = false;
        SessionState.EraseBool(CompileKey);
        SessionState.EraseBool(PlayKey);
        SessionState.SetBool(SetupKey, true);
        SessionState.SetFloat(DeadlineKey, (float)EditorApplication.timeSinceStartup + 120);
        EditorApplication.update += Prepare;
    }

    private static void Prepare()
    {
        try
        {
            Require(EditorApplication.timeSinceStartup < SessionState.GetFloat(DeadlineKey, 0), "Timed out preparing Ink and scenes.");
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || InkCompiler.executingCompilationStack) return;
            var sourceInk = InkLibrary.GetInkFileWithPath(SourceInk);
            var destinationInk = InkLibrary.GetInkFileWithPath(DestinationInk);
            Require(sourceInk != null && destinationInk != null, "One or both Ink source files are missing from the Ink library.");
            if (SessionState.GetBool(CompileKey, false))
            {
                Require(!sourceInk.hasErrors && !destinationInk.hasErrors, "Ink compilation reported errors.");
                Require(sourceInk.jsonAsset != null && destinationInk.jsonAsset != null, "Ink compilation did not produce both JSON assets.");
                EditorApplication.update -= Prepare;
                SessionState.EraseBool(SetupKey);
                SessionState.SetBool(PlayKey, true);
                EditorSceneManager.OpenScene(Source, OpenSceneMode.Single);
                EditorApplication.EnterPlaymode();
                return;
            }
            SessionState.SetBool(CompileKey, true);
            InkCompiler.CompileInk(new[] { sourceInk, destinationInk }, immediate: true);
        }
        catch (Exception error) { Fail(error); }
    }

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PlayKey, false)) return;
        SessionState.EraseBool(PlayKey);
        var readyAt = EditorApplication.timeSinceStartup + 2;
        EditorApplication.update += VerifyAfterFrames;
        void VerifyAfterFrames()
        {
            if (EditorApplication.timeSinceStartup < readyAt || EditorApplication.isUpdating || EditorApplication.isCompiling) return;
            EditorApplication.update -= VerifyAfterFrames;
            try { VerifyTransfer(); }
            catch (Exception error) { Fail(error); }
        }
    }

    internal static void VerifyTransfer()
    {
        var source = SceneManager.GetSceneByPath(Source);
        Require(source.IsValid() && source.isLoaded && SceneManager.GetActiveScene() == source, "Departure scene is not the live active scene.");
        var sourceRoots = source.GetRootGameObjects();
        Require(sourceRoots.SelectMany(x => x.GetComponentsInChildren<Component>(true)).All(x => x != null), "Departure has a missing component/script.");
        var sourceCamera = sourceRoots.SelectMany(x => x.GetComponentsInChildren<Camera>(true)).Single();
        var interaction = sourceCamera.GetComponent<RainyStopInteraction>();
        var sourceBridge = sourceCamera.GetComponent<RainyStopInk>();
        Require(interaction != null && sourceBridge != null, "Departure camera is missing its interaction or Ink bridge.");
        oldInputSettings = InputSystem.settings;
        oldRunInBackground = Application.runInBackground;
        inputSettingsChanged = true;
        var settings = UnityEngine.Object.Instantiate(oldInputSettings);
        settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings = settings;
        Application.runInBackground = true;
        var mouse = InputSystem.AddDevice<Mouse>();
        mouse.MakeCurrent();
        Physics2D.SyncTransforms();
        var lever = GameObject.Find("SignalLever");
        Require(lever != null && lever.GetComponent<BoxCollider2D>() != null, "SignalLever collider is missing.");
        var point = sourceCamera.WorldToScreenPoint(lever.GetComponent<BoxCollider2D>().bounds.center);
        Click(mouse, interaction, point);
        var dialogue = typeof(RainyStopInk).GetField("dialogue", Private);
        var story = typeof(RainyStopInk).GetField("story", Private);
        Require(((string)dialogue.GetValue(sourceBridge)).Contains("訊號已送出") && SceneManager.GetActiveScene() == source, "First lever click should send a signal and stay in the booth.");
        Debug.Log("ARENA_SMOKE PASS signal sent");
        Click(mouse, interaction, point);
        var deadline = EditorApplication.timeSinceStartup + 30;
        EditorApplication.update += CheckArrival;

        void CheckArrival()
        {
            try
            {
                Require(EditorApplication.timeSinceStartup < deadline, "Ferry transition or dialogue timed out.");
                var arrivalScene = SceneManager.GetSceneByPath(Destination);
                if (!arrivalScene.IsValid() || !arrivalScene.isLoaded || SceneManager.GetActiveScene() != arrivalScene) return;
                var arrivalRoots = arrivalScene.GetRootGameObjects();
                Require(arrivalRoots.SelectMany(x => x.GetComponentsInChildren<Component>(true)).All(x => x != null), "Ferry has a missing component/script.");
                var arrivalCamera = arrivalRoots.SelectMany(x => x.GetComponentsInChildren<Camera>(true)).Single();
                var arrival = arrivalCamera.GetComponent<RainyStopInk>();
                Require(arrival != null, "Ferry scene is missing its Ink bridge.");
                var arrivedStory = (Ink.Runtime.Story)story.GetValue(arrival);
                var arrivedText = (string)dialogue.GetValue(arrival);
                Require(arrivedStory.variablesState["arrived_with_signal"] is true && arrivedText.Contains("訊號收到了"), "Arrival did not retain the departure signal.");
                Require(!source.isLoaded, "Departure scene is still loaded after single-scene transfer.");
                Debug.Log("ARENA_SMOKE PASS signal transfer");
                var cameraInteraction = arrivalCamera.GetComponent<RainyStopInteraction>();
                var choose = typeof(RainyStopInk).GetMethod("Choose", Private);
                var ticket = GameObject.Find("TicketWindow").GetComponent<BoxCollider2D>();
                Click(mouse, cameraInteraction, arrivalCamera.WorldToScreenPoint(ticket.bounds.center));
                Require(((string)dialogue.GetValue(arrival)).Contains("認出你在訊號亭") && arrivedStory.currentChoices.Count == 1, "Signal arrival should unlock ticketing.");
                choose.Invoke(arrival, new object[] { 0 });
                Require(((string)dialogue.GetValue(arrival)).Contains("你踏上浮橋") && arrivedStory.currentChoices.Count == 1, "Boarding should show one replay choice.");
                VerifyEnding(arrival, arrivalCamera, cameraInteraction, mouse, dialogue, story);
                Debug.Log("ARENA_SMOKE PASS signal ticket and boarding");
                SceneManager.LoadSceneAsync(Destination, LoadSceneMode.Single);
                var directDeadline = EditorApplication.timeSinceStartup + 30;
                EditorApplication.update -= CheckArrival;
                EditorApplication.update += CheckDirectEntry;
                void CheckDirectEntry()
                {
                    try
                    {
                        Require(EditorApplication.timeSinceStartup < directDeadline, "Direct ferry reload timed out.");
                        var direct = SceneManager.GetSceneByPath(Destination);
                        if (!direct.IsValid() || !direct.isLoaded || SceneManager.GetActiveScene() != direct || direct.handle == arrivalScene.handle) return;
                        var cam = direct.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Camera>(true)).Single();
                        var bridge = cam.GetComponent<RainyStopInk>();
                        var directStory = (Ink.Runtime.Story)story.GetValue(bridge);
                        Require(directStory.variablesState["arrived_with_signal"] is false && !((string)dialogue.GetValue(bridge)).Contains("訊號收到了"), "Direct entry inherited a prior transfer.");
                        var directInteraction = cam.GetComponent<RainyStopInteraction>();
                        var ticketPoint = cam.WorldToScreenPoint(GameObject.Find("TicketWindow").GetComponent<BoxCollider2D>().bounds.center);
                        Click(mouse, directInteraction, ticketPoint);
                        Require(((string)dialogue.GetValue(bridge)).Contains("先看公告") && directStory.currentChoices.Count == 1, "Direct entry should require the notice.");
                        typeof(RainyStopInk).GetMethod("Choose", Private).Invoke(bridge, new object[] { 0 });
                        Require(directStory.variablesState["saw_notice"] is true, "Notice choice should set saw_notice.");
                        Click(mouse, directInteraction, ticketPoint);
                        Require(((string)dialogue.GetValue(bridge)).Contains("你說出東側浮橋") && directStory.currentChoices.Count == 1, "Notice should unlock ticketing.");
                        typeof(RainyStopInk).GetMethod("Choose", Private).Invoke(bridge, new object[] { 0 });
                        Require(((string)dialogue.GetValue(bridge)).Contains("你踏上浮橋") && directStory.currentChoices.Count == 1, "Notice boarding should show one replay choice.");
                        VerifyEnding(bridge, cam, directInteraction, mouse, dialogue, story);
                        Debug.Log("ARENA_SMOKE PASS direct ticket and boarding");
                        Debug.Log("ARENA_SMOKE COMPLETE");
                        EditorApplication.update -= CheckDirectEntry;
                        RestoreSettings();
                        EditorApplication.Exit(0);
                    }
                    catch (Exception error) { EditorApplication.update -= CheckDirectEntry; Fail(error); }
                }
            }
            catch (Exception error) { EditorApplication.update -= CheckArrival; Fail(error); }
        }
    }

    private static void VerifyEnding(RainyStopInk bridge, Camera camera, RainyStopInteraction interaction, Mouse mouse, FieldInfo dialogue, FieldInfo storyField)
    {
        var story = (Ink.Runtime.Story)storyField.GetValue(bridge);
        var ending = (string)dialogue.GetValue(bridge);
        var notice = GameObject.Find("NoticeBoard").GetComponent<BoxCollider2D>();
        var ticket = GameObject.Find("TicketWindow").GetComponent<BoxCollider2D>();
        Click(mouse, interaction, camera.WorldToScreenPoint(notice.bounds.center));
        Click(mouse, interaction, camera.WorldToScreenPoint(ticket.bounds.center));
        Require((string)dialogue.GetValue(bridge) == ending && story.currentChoices.Count == 1 && SceneManager.GetActiveScene().path == Destination, "World clicks must not advance the choice-only ending.");
        typeof(RainyStopInk).GetMethod("Choose", Private).Invoke(bridge, new object[] { 0 });
        Require(story.variablesState["saw_notice"] is false && story.variablesState["arrived_with_signal"] is false && story.currentChoices.Count == 3, "Replay should clear both flags and return to the waiting room.");
        Click(mouse, interaction, camera.WorldToScreenPoint(ticket.bounds.center));
        Require(((string)dialogue.GetValue(bridge)).Contains("先看公告") && story.currentChoices.Count == 1, "Ticketing after replay should require the notice again.");
    }

    private static void Click(Mouse mouse, RainyStopInteraction interaction, Vector2 point)
    {
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
        InputSystem.Update();
        typeof(RainyStopInteraction).GetMethod("Update", Private).Invoke(interaction, null);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
        InputSystem.Update();
    }

    private static void Require(bool passed, string reason) { if (!passed) throw new InvalidOperationException(reason); }
    private static void Fail(Exception error)
    {
        SessionState.EraseBool(SetupKey);
        EditorApplication.update -= Prepare;
        Debug.LogError("ARENA_SMOKE FAIL");
        Debug.LogException(error);
        RestoreSettings();
        EditorApplication.Exit(1);
    }
    private static void RestoreSettings()
    {
        if (SessionState.GetBool(RestoreOptionsKey, false))
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            SessionState.EraseBool(RestoreOptionsKey);
        }
        if (!inputSettingsChanged) return;
        InputSystem.settings = oldInputSettings;
        Application.runInBackground = oldRunInBackground;
        inputSettingsChanged = false;
    }
}
