using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public sealed class RainyStopInteraction : MonoBehaviour
{
    [SerializeField] private BoxCollider2D noticeBoard;
    [SerializeField] private BoxCollider2D ticketWindow;

    private Camera sceneCamera;

    public event Action<string> Interacted;

    private void Awake()
    {
        sceneCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        var ray = sceneCamera.ScreenPointToRay(mouse.position.ReadValue());
        var hit = Physics2D.GetRayIntersection(ray).collider;
        if (hit == null)
            return;

        if (hit == noticeBoard)
            Report("notice-board");
        else if (hit == ticketWindow)
            Report("ticket-window");
    }

    private void Report(string id)
    {
        Debug.Log($"EXP-0015 interaction: {id}", this);
        Interacted?.Invoke(id);
    }
}
