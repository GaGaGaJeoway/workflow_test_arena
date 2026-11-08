# EXP-0038 — skip a short arrival without losing control

Status: BLOCKED_MCP; implemented, but all 3 runtime checks remain unrun (0 passed / 0 failed / 3 unrun). Follow-up to EXP37, with a different question: skipping an in-progress animation safely. This experiment is not phase-complete.

## Scope and decision

Reuse the existing EXP0037_Arrival scene, controller and clip in the designated daily project `K:/Game/Unity/Dave/UnityTest2DProject`, Unity6000.0.31f1, Git branch codex/scene-entry-safety / HEAD96f4520 plus preserved dirty state and experimental assets. MCP10.3.0 plus the recorded local heartbeat patch. No new project, worktree, Library or package. No focus/unfocus test.

Only EXP0037ArrivalControl.cs changes: public SkipArrival, Escape and a prototype skip button. Skip samples the authored clip endpoint, then uses the same completion handler as natural completion. Calling skip outside an active arrival is a no-op. Reuse the original scene/class name to avoid duplicating assets or renaming their GUIDs.

## Minimum acceptance

1. Skip during the actual in-progress clip: Visitor reaches X=0, IsPlaying becomes false and CompletedCount advances once. A held movement input can move Player afterward.
2. Repeated skip while idle and subsequent frames do not advance completion again or relock movement.
3. A replay after skipping can complete naturally, incrementing completion once more; the shared completion refactor must preserve this direct behavior.

Use one MCP Play session and a bounded in-memory observer / temporary synthetic keyboard. Exercise the actual Escape input path for skip if feasible. Direct BeginArrival is acceptable setup; physical keyboard/UI click and broad manual visual acceptance remain separate. Release/remove only the observer's device, unregister callback, stop Play and restore original clean04_WoodsPursuit scene. Do not touch player saves or replace InputSettings.

## Environment and rework

Preflight initially reported the correct fresh-ready project, but the first read-only execute_code returned success=false with no details and a subsequent state query reported `Unity session not ready ... (ping not answered)`. Marked BLOCKED_MCP; one reselect of the same instance plus telemetry ping restored fresh-ready and successful script read. Editor.log contained a WebSocket close without completing the close handshake. Cause/correlation with this request remains unconfirmed; no focus hypothesis or repeated focus testing. No CLI fallback, reinstall or cache reset.

After editing, reflection still found the old loaded type without SkipArrival. The edit tool had reported scheduledRefresh, but no compile had happened; requested one scripts-only compilation. A later fresh resource reported compile start1794115562009 / finish1794115585618 (about23.609s), domain reload after1794115602066. These are local Editor event timestamps, not proof the new method was loaded.

During test preflight, the first instance selection returned No Unity instances connected while one instances query listed the exact target. One reselect/ping restored a fresh resource. The host then removed the Unity MCP tool namespace before the required compilation probe and post-refresh method reflection could execute. The call was aborted; no Play was entered, no observer/callback was installed and no synthetic keyboard was created. Last snapshot: sequence49, age212ms, ready=true, idle/non-Play/non-compile/non-update/tests=false; original04_WoodsPursuit clean, external asset changes clean, Console errors0. This is the last observed state, not a claim of live readiness after tool removal. No player saves were touched by this attempt.

The [local preflight report](EXP0038-LocalResult.json) records these facts. A matching source timestamp/new DLL timestamp was observed through a read-only filesystem check, but does not replace loaded-method/probe verification. The heartbeat patch does not establish transport/tool availability; these connection symptoms are not a new focus experiment or a proven recurrence of the old state-freshness root cause.

## Source delivery and next action

One changed script was published to private [commit c0053106](https://github.com/GaGaGaJeoway/UnityTest2DProject/commit/c00531067221d8ec5d945e5a9e098de03da7413c), parent3b67b575, branch codex/arena-exp0038. The original branch, HEAD and original index bytes were checked unchanged. No checkout/worktree was created and working source was not rewritten during publication. An initial publication helper name collided with PowerShell's case-insensitive git resolution; that helper was stopped before creating an index/ref, then corrected to invoke git.exe explicitly.

[Manifest](EXP0038-SourceManifest.json) traces the script (working/publication SHA-256 b78c63a3d1d85488ac9cfeef1e6b79f125f5858f73bd192b5b62a7c058238809). The [public script snapshot](RuntimeSnapshots/EXP0038ArrivalControl.cs.txt) exposes the full current prototype for reviewers; the previous EXP37 snapshot/manifest remain historical. The unchanged scene, clip, controller and dependencies are inherited from the private parent, not copied into this public repository. This is source preservation, not accepted Runtime delivery or CI.

Restore access to the existing unityMCP server/tools in Codex while keeping the same original Editor/project. Then check one fresh state, the existing compilation probe plus loaded SkipArrival, and resume the already authorized single Play check. Do not reinstall/upgrade, clear caches, create a project or substitute CLI. A Unity panel reconnect alone may not restore a tool namespace removed from the Codex session; the exact reason for removal is not yet established.

Fixed source review3b67b575...c0053106: Standards0 actionable findings; Spec0 findings. Both were read-only reviews, not runtime verification. The three checks above remain pending.

## Follow-up: distinguish server availability from Codex client startup

On the next continuation (2026-11-08 Taiwan), Unity tools were still absent. Read-only Codex configuration inspection (`codex mcp get unityMCP --json`, not a Unity CLI test) showed enabled=true, Streamable HTTP URL http://127.0.0.1:8080/mcp, no disabled reason or tool filters. The original project was now open in Editor PID12688, Unity6000.0.31f1; the old PID is no longer the current Editor. An elevated read-only process/port query confirmed this exact project path and one listener on8080 (PID16244). An initial sandboxed port query was not authoritative because process enumeration was denied; the permitted query supersedes it.

Unity's current Editor.log reports Server ready and Session connected. Nevertheless, the native MCP resource client failed before reading editor/state: `MCP startup failed: handshaking with MCP server failed ... HTTP request failed ... error sending request for url (http://127.0.0.1:8080/mcp) ... when send initialize request`. Thus neither a live Editor state nor compile/method readiness was established in this continuation. A listening socket/log message does not prove the Codex client handshake works, and does not establish which Unity instance MCP would route to.

The failure is now narrowed to the Codex-client initialization/transport boundary; its underlying network/client cause is not proven. No configuration, Plugin, Unity source, scene or cache was changed in this follow-up; no gameplay test ran. Recommended next action: reconnect/restart the existing unityMCP connection from Codex's MCP settings, then retry native resource access. Preserve the original Editor and project. [Official MCP documentation](https://learn.chatgpt.com/docs/extend/mcp?surface=cli) explains the shared config and client-side server management; it does not diagnose this local failure. Still BLOCKED_MCP, 0 passed / 0 failed / 3 unrun.
