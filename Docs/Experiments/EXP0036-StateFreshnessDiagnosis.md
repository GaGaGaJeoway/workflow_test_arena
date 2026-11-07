# EXP-0036 — focused-idle snapshot freshness diagnosis

Status: the reviewed single-file temporary plugin repair has now been applied after user continuation of the specific repair request; verification is in progress. No upgrade/reinstall, server change or permanent fix is claimed. The latest passive observation is fresh while Unity is unfocused; the focused-idle defect remains unrepaired. This does not change formal design policy.

## Reproduction and evidence

Original project `K:/Game/Unity/Dave/UnityTest2DProject`, Unity6000.0.31f1, MCP10.3.0, HEAD96f4520d672394e57bcee5532dca7cbed29f6258 plus preserved26-row dirty state. The same safe existing case is a single Play stimulus, not repeated product QA.

The original MCP job passed1/1 but final state went stale. After same-project reconnection, three snapshots across109.6s were fresh. One further controlled Play cycle (job3b59af3fcd5e47f1bfd59c665fc7ceb6) passed1/1, then resource readiness failed again. Before the cycle focused=false/age306ms; after it focused=true/age21.244s. Reselect/ping did not move observed timestamp1794065825581 or sequence15; age grew to50.891s. Direct MCP readings confirmed a responsive idle Editor and clean original scene, Console errors0; no current workload justified stale readiness. No CLI replacement was used.

## Ranked explanations and installed-code findings

1. **Snapshot observation versus content-change time** — strongly supported. `Editor/Services/EditorStateCache.cs` OnUpdate lines319–405 throttles at1s but returns early when tracked state is unchanged, without refreshing observed_at_unix_ms. GetSnapshot lines535–560 restamps only when InternalEditorUtility.isApplicationActive is false. A focused idle Editor can therefore return an old observation even while responding normally. The observed focus transition matches this path. It may also affect focused idle without Play; that scenario is not yet separately tested.
2. **Server/session cache routing** — not supported as the primary explanation by the inspected path. Installed server `services/resources/editor_state.py` lines224–241 sends get_editor_state to the selected Unity instance on each request; it recalculates age on each reply. The same instance remained selected and direct tools worked. Other transport defects have not been exhaustively excluded.
3. **Clock skew** — not supported by this run: pre-run age was306ms, while the later age grew with elapsed time against an unchanged observation. No clock changes were made; a complete clock audit was not performed.

The server's `_enrich_advice_and_staleness` at lines178–215 treats age>2000ms as stale and adds stale_status to readiness blockers. This conflicts with using the plugin's last state-change time as the last observation time. This is a source-supported causal explanation, not a claim that a repair was tested in Unity.

Installed source: `K:/Game/Unity/Dave/UnityTest2DProject/Library/PackageCache/com.coplaydev.unity-mcp/Editor/Services/EditorStateCache.cs`. SHA256135e5cdced56fe66abffa0a7f926f346bc39c40a6469c511e6ff725752e7932d; package lock81542a7e86292b5c4c21b1782c5af23b567f45ce, version10.3.0. Source pin is saved locally alongside the follow-up evidence. The live source remains unchanged.

## Reviewable repair proposal — not applied

[EXP0036-idle-heartbeat.patch](Patches/EXP0036-idle-heartbeat.patch) changes only the idle no-change branch to advance the existing1s throttle and call existing ForceUpdate("heartbeat"). It obtains a complete new snapshot on the main thread rather than merely rewriting a timestamp on potentially old metadata. It does not relax the server's2s threshold or invent a ready flag. No new dependency, project, process or cache purge is needed.

Tradeoff: this restores periodic BuildSnapshot allocation while idle, reversing part of the plugin's GC optimization. The source comments cite28ms GC spikes historically; this workstation cost is **not measured**. Do not call the patch performance-safe or fixed before testing. A later lower-allocation design is not part of this proposal.

This would be a temporary local edit to an installed PackageCache file, potentially overwritten by future package resolution; it is not a formal upgrade, pinned dependency change or durable upstream fix. If approved, first verify the exact source hash, save a byte-exact backup in Arena/Logs, apply only this hunk, and let the existing Editor compile/reconnect. Roll back only this edited file if it introduces compile/tool failures; preserve all unrelated dirty files. Never reset the project or clear Library.

## Options requiring user choice

- **Read-only focus check first:** make the same original Unity lose focus (switch to Codex), then allow one resource check. The predicted fresh timestamp would distinguish the focus branch without a plugin edit or new Play run. Estimated30–60s and a small JSON record; no new Library.
- **Approve the single-file temporary patch and MCP verification:** estimated2–5min with existing Editor/cache; reserve up to0.2GiB incremental compile/log use (unmeasured estimate). No reinstall/upgrade or branch switch. Then verify a focused idle Editor beyond2s and one exact-case MCP Play cycle followed by fresh-state checks; retain the same stop-on-stale policy. No full suite or CLI fallback. The negative main-thread-stall scenario is not to be forced on the user's Editor.
- **Retain diagnosis without a plugin change:** leave BLOCKED_MCP and preserve all evidence. Do not repeatedly request reconnection as if it were a permanent repair.

Issue23 retains approval for changes to the designated environment. No option has been silently selected, and no patch has been applied. This proposal and result are local evidence, not GitHub CI or an upstream acceptance.

## Passive focus comparison (no installed changes)

A later read-only continuation first returned the same focused snapshot(sequence15, observed1794065825581), now stale by2,880,596ms. Original HEAD/status rows and installed plugin source hash still matched their pins.

Computer Use was attempted only to select an existing Unity Hub window and make the Editor lose focus. App approval timed out; no successful controlled activation is claimed, and no retry bypass was attempted. Subsequent MCP reads nevertheless reported focused=false and fresh readiness: sequence16, observed1794069101249/1794070431422, ages295/301ms. The original04_WoodsPursuit scene was directly read as clean. No Play, explicit reconnect, refresh, code edit or cache cleanup was requested in this continuation.

The reported instance/session ID stayed the same, but instances.connected_at advanced; full connection-event history and the actual cause of focus change were not captured. The domain-reload timestamp did not change. Treat this as a **passive association**, not a controlled A/B or proof that no transport reconnection occurred.

This is consistent with the installed GetSnapshot background branch, which can restamp an old clone on read. Freshness while unfocused is therefore not evidence of a permanent fix or of every cached field being resampled. external_changes_dirty remained true, and no refresh was requested. Full-snapshot/readiness correctness and the patch's GC cost remain unverified.

Local evidence: `K:/Game/Unity/Dave/Arena/Logs/EXP0036/focus-observation.json`. Current sampled readiness is READY while unfocused; do not treat the focused-idle problem as repaired. Installed-plugin repair still requires an explicit user choice under Issue23; generic continuation has not been treated as patch approval.

## Temporary repair application — verification pending

Following the concrete single-file repair request, the user replied to continue. The work proceeded only within that stated scope. The original source hash matched its pin; a byte-exact backup was saved before modifying the single no-change branch. Server thresholds, package manifest/lock, gameplay sources and Editor instance were not changed.

- Original SHA256:135e5cdced56fe66abffa0a7f926f346bc39c40a6469c511e6ff725752e7932d.
- Patched SHA256:f06af60c7bed9a2273f6f27fe48cb173ad3c6845860623addd16f88a766a692f.
- Backup: `K:/Game/Unity/Dave/Arena/Logs/EXP0036/repair-1/EditorStateCache.cs.original`.
- Application metadata and pre-change Git status: same repair-1 directory.
- Environment label: **MCP10.3.0 + temporary local heartbeat patch**, not unmodified10.3.0. PackageCache resolution may overwrite the edit.

The MCP verification will confirm actual compiled code, no C# errors, a single existing save-isolated PlayMode case, and post-Play freshness checkpoints. The earlier unapplied-proposal/passive-observation sections describe their historical checkpoints; they are not the current installation status. Source application alone is not a test pass. If the change introduces compile/tool failures, rollback is limited to this source file's verified backup; no Library purge or unrelated file reset.

### Repair verification checkpoint (still in progress)

Source hash alone initially matched the patch but the live OnUpdate IL lacked heartbeat; the old DLL was still loaded. One MCP refresh/compile request produced a new compilation interval1794072084633–1794072103898, and the compilation probe returned ready with no C# errors. Reflection then found heartbeat in the actual loaded method. Idle snapshots advanced sequence81→94 and observed time1794072205485→1794072219221; they were fresh/ready, while unfocused.

The single integration job `82745f3250dd46ab94b74ce81c9de18b` is still under observation. At its latest checkpoint the job had0/null completed/total, while direct TestRunnerApi.IsRunActive and Play were true in an InitTestScene. The heartbeat state was fresh and correctly reported running_tests, not idle. This is not a test pass or evidence that the whole workflow is repaired. No duplicate job or cache purge was launched; the same run is being diagnosed and will be normally cancelled if it cannot complete within its bound.
