# EXP-0036 — MCP freshness repair verification

**Current result:** the temporary local heartbeat patch is compiled and its targeted freshness path is verified in the original project. One warm PlayMode case passed and subsequent idle snapshots stayed fresh. The first post-compile job timed out during initialization and required normal cancellation; that failure is retained, not replaced by the warm success. This is a scoped local repair, not a permanent package release or complete MCP reliability claim.

## Environment and traceability

- Project: `K:/Game/Unity/Dave/UnityTest2DProject`, Unity6000.0.31f1, instance `UnityTest2DProject@745a8f02fda8e2d8`.
- Branch/HEAD: `codex/scene-entry-safety` / `96f4520d672394e57bcee5532dca7cbed29f6258` plus26 existing default Git status rows. Row lists matched before/after; byte identity of every pre-existing dirty file was not measured.
- Environment label: **MCP10.3.0 + temporary local heartbeat patch**. Server, manifest/lock and gameplay sources were not changed. No new project, Editor process or Library was created; no cache purge, branch switch or important PR merge occurred.
- Installed file: `Library/PackageCache/com.coplaydev.unity-mcp/Editor/Services/EditorStateCache.cs` under the original project.
- Original SHA256:135e5cdced56fe66abffa0a7f926f346bc39c40a6469c511e6ff725752e7932d.
- Patched SHA256:f06af60c7bed9a2273f6f27fe48cb173ad3c6845860623addd16f88a766a692f.
- Package lock81542a7e86292b5c4c21b1782c5af23b567f45ce remains unchanged. A package version string alone does not describe this modified installation.
- Byte-exact backup: `K:/Game/Unity/Dave/Arena/Logs/EXP0036/repair-1/EditorStateCache.cs.original`; application.json records hashes, scope and pre-change status. Application followed the user's continuation of the specific single-file repair request; scope was not expanded to reinstall/upgrade.

## Failure mechanism and minimal repair

The installed C# cache throttles OnUpdate at1s but originally returned immediately when tracked state did not change, leaving observed_at_unix_ms at the previous content-change time. GetSnapshot restamps only when InternalEditorUtility.isApplicationActive is false. The server's `services/resources/editor_state.py` sends get_editor_state to Unity and declares age>2000ms stale. Consequently a responsive, focused idle Editor can be falsely blocked while an unfocused clone can look fresh merely from timestamp restamping.

This explanation is supported by repeated local observations: a safe PlayMode case passed but the focused idle snapshot aged21.244s then50.891s without moving its sequence; direct MCP reads found the Editor responsive, idle and scene clean. Pre-run focus=false snapshots were fresh. A later passive focus comparison also changed from stale/focused to fresh/unfocused, but app approval timed out during the attempted Computer Use activation and connected_at changed; that observation was not a controlled A/B or proof excluding transport reconnection. The domain-reload timestamp did not change in that comparison.

[The applied patch](Patches/EXP0036-idle-heartbeat.patch) changes only the unchanged-state branch: advance the existing1s throttle and call existing ForceUpdate("heartbeat"). It samples a complete snapshot on the main thread, rather than rewriting timestamps on old metadata. It does not relax server thresholds or fabricate ready flags. It reintroduces periodic snapshot allocations; cost is bounded only by the measurements below, not assumed free.

## Compile gate: source change was not enough

Initially the disk hash matched the patch but the loaded OnUpdate IL lacked heartbeat. One MCP refresh/compile request then produced compile interval1794072084633–1794072103898 (19.265s), later than source application. CompilationProbe returned ready, Console C# errors were0, and reflection found heartbeat in the actual loaded method. No repeated refresh or Library deletion was used.

## Execution results

All rows use the existing save-isolated `SceneUiIntegrationTests.NewGameLoadsClinicAndPresentsInkDialogue` case in the designated original project. Each result remains local reported evidence, not GitHub CI. These cases test the original clinic/Ink flow, not the delivered EXP35 Arena scenes.

| Run | Result | Timing and boundary |
| --- | --- | --- |
| Before patch, job31a3c491912e4108ad91881373796fd8 |1 passed,0 failed/skipped; post-run state became stale |Case7.82s / job19.655s |
| Before patch follow-up, job3b59af3fcd5e47f1bfd59c665fc7ceb6 |1 passed,0 failed/skipped; stale readiness reproduced after reconnect |Case3.923s / job19.003s; focused idle ages21.244→50.891s |
| First post-compile job82745f3250dd46ab94b74ce81c9de18b |MCP initialization timeout;0 completed/total unknown; no current case result verified |Kept as failed initialization, not an assertion failure or pass |
| One warm retry, job1440452db7354ee2a73c0599debd343d |1 passed,0 failed/skipped |Case5.1135148s / job9.913s; new XML17:44:03Z matched exact case |

The first failed job still had an active native TestRunner after the MCP job timed out. Only its real Unity GUID `ec0815b0-1eae-4277-ba30-e35c04e1758a` was cancelled via public CancelTestRun; the MCP job ID was not used as a Unity GUID. Subsequent direct checks showed IsRunActive=false, zero runners, non-Play and the original scene clean before the single warm retry. An XML with15:37Z timestamps was explicitly excluded as an older run's result. No overlapping or third retry occurred.

Installed timeout handling clears the MCP running flag without proving the native run ended. Also, the existing is_changing field directly uses isPlayingOrWillChangePlaymode, which is true during ordinary Play as well. These unchanged metadata behaviors are not repaired by this patch; authoritative native-run checks and matching job/XML results remain necessary. The first initialization problem is still open and cannot yet be attributed to or ruled out as an interaction with the patch.

## Post-Play freshness and restoration

Natural resource observations after the successful warm retry:

| Point | Sequence | Observed Unix ms | Offset from first snapshot |
| --- | ---: | ---: | ---: |
| T0 |57|1794073486551|0s|
| Next sample |104|1794073535378|48.827s|
| Later sample |237|1794073668385|181.834s|
| Direct snapshot/getter check |294|1794073725669|239.118s|

Resource samples were fresh/ready/idle, non-Play/non-compile, with no active test/import and external changes cleared. The final same-call check read InternalEditorUtility.isApplicationActive=true and snapshot editor.is_focused=true, matching the plugin's getter. EditorApplication.isFocused returned a different value; it was not treated as a synonym or used to reinterpret the patch condition. Sampling was asynchronous: nominal5/30/90s waits were not claimed as exact intervals; actual observation timestamps are shown.

The original04_WoodsPursuit scene remained active and clean, SaveService override null, native TestRunner inactive with no runners, and Console C# errors0. Setup used a unique Guid-based test save directory; teardown restored its settings/PlayerPrefs. Unknown pre-existing temp directories were not broadly cleaned. The old unpatched snapshot's external_changes_dirty flag had been left alone; no extra refresh was used solely to hide it.

## Timing and allocation limits

A bounded10-call reflected BuildSnapshot("perfcheck") microcheck took1.4837ms total,0.14837ms mean, including reflection overhead. It was performed after natural heartbeat observation because it increments sequence/timestamps. This measures one method in a small local setup, not game FPS, worst-case pauses or a long-running GC workload.

The thread allocation counter returned0 for that microcheck, but also returned0 for a65536-byte positive allocation control retained with GC.KeepAlive. **Allocation cost is Unknown with this counter**; zero must not be reported as allocation-free. The source's historical28ms GC-spike comment is not a measurement on this workstation. Performance under larger projects remains unverified.

## Evidence and remaining decisions

Local raw records: `K:/Game/Unity/Dave/Arena/Logs/EXP0036/repair-1/verification.md`, verification-raw.json, application.json and gc-counter-positive-control.json. Earlier follow-up and passive focus JSON are retained in the parent evidence directory. Raw logs were not uploaded.

Current scoped operational status is **MCP_CONNECTED_READY with local patch**. The patch passed loaded-code, scoped Play and post-Play freshness checks, while retaining the first initialization failure. No whole-game/manual UI acceptance, deliberate main-thread-stall test or long-term memory/FPS validation is claimed.

PackageCache edits may be overwritten by package resolution. This is not a durable upstream fix or a dependency pin; formal packaging/upstream changes remain a separate user decision. The verified backup permits restoration of this single file without touching unrelated source or caches. Future reapplication must verify source hash instead of blindly applying to a different package revision.
