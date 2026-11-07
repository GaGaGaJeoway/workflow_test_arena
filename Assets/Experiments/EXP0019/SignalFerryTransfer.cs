using Ink.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(RainyStopInk))]
public sealed class SignalFerryTransfer : MonoBehaviour
{
    private const string Destination = "Assets/Scenes/Experiments/EXP0017/EXP0017_NightFerry.unity";
    private RainyStopInk bridge;
    private bool departing;

    private void OnEnable()
    {
        bridge = GetComponent<RainyStopInk>();
        bridge.Advanced += Transfer;
    }

    private void OnDisable() => bridge.Advanced -= Transfer;

    private void Transfer(Story story)
    {
        if (departing || !story.currentTags.Contains("depart-for-ferry"))
            return;

        bool receivedSignal = story.variablesState["signal_sent"] is true;
        departing = true;
        var load = SceneManager.LoadSceneAsync(Destination, LoadSceneMode.Single);
        load.completed += _ =>
        {
            var arrival = FindFirstObjectByType<RainyStopInk>();
            if (SceneManager.GetActiveScene().path == Destination && arrival != null)
                arrival.ApplyFerryArrival(receivedSignal);
        };
    }
}
