# EXP-0036 — focused-idle snapshot freshness diagnosis

Status: read-only diagnosis complete enough for a repair proposal; **no installed plugin/server files modified**, no permanent fix verified. Current BLOCKED_MCP. This does not change formal design policy.

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
