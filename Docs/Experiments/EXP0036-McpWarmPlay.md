# EXP-0036 — original-project warm MCP Play workflow

Status: gameplay cases passed, but a controlled post-reconnect Play cycle reproduced stale readiness. Current BLOCKED_MCP; installed-code diagnosis/proposed repair is documented below. Executed after EXP-0035 closeout, with no new project or CLI fallback.

Question: can the designated existing 2D project complete a useful Play check through MCP using its current Editor/cache, without a replacement project or CLI fallback?

Target: `K:/Game/Unity/Dave/UnityTest2DProject`, Unity `6000.0.31f1`, MCP `10.3.0`, branch `codex/scene-entry-safety`, HEAD `96f4520d672394e57bcee5532dca7cbed29f6258` plus preserved dirty state. Capture the actual state again before execution; these values are not a substitute for that check.

Minimum scope:

1. MCP project/info and fresh editor/state must identify the target and show no competing test or Play run. Preserve the starting scene and source changes.
2. Inspect the existing `SceneUiIntegrationTests.NewGameLoadsClinicAndPresentsInkDialogue` case and its setup/teardown before selecting it. It must use the existing isolated SaveService seam, protecting player saves. No new gameplay assets or test scripts are planned.
3. Run only the selected existing PlayMode case through MCP Test Runner. Retain its single job ID, real executed count, result and elapsed time. Do not treat a zero-test job as a pass.
4. Verify the underlying run has ended and the Editor is no longer playing; restore the starting scene if the test changed it, without saving or overwriting existing assets. Record Console errors only when needed.

Report project path, Git HEAD/dirty state, Unity version, MCP instance, exact case, result/job, timings and restoration. This is a baseline for the original-project MCP workflow, not retrospective acceptance of EXP-0015–0035 or a new product QA requirement.

If MCP becomes unavailable, report BLOCKED_MCP and diagnose. Do not start a CLI substitute, another Editor, a new project or cache deletion without separate user approval. Original-project branch/source integration and formal policy changes are outside this experiment.

## Execution evidence

| Field | Result |
| --- | --- |
| Project/commit | Exact target above; HEAD96f4520d672394e57bcee5532dca7cbed29f6258, branch unchanged |
| Local source state |26 default Git status rows before/after, identical lists; no deliberate source edits, reset or cleanup. Full byte identity of pre-existing dirty files not measured |
| MCP case/job | `SceneUiIntegrationTests.NewGameLoadsClinicAndPresentsInkDialogue`; `31a3c491912e4108ad91881373796fd8` |
| Actual execution |1 passed,0 failed,0 skipped; succeeded job, no zero-test inference |
| Timing |Case7.82s; job wall19.655s, including runner overhead; excludes separate preflight investigation |
| Console |0 error entries before/after; a non-fatal Floor 2D SRP Batcher material warning appeared |
| Player-save isolation |Existing setup uses a new Guid-based temp directory and SaveService override; pre/post override null. Teardown includes settings/PlayerPrefs restoration and removal of that run's temp directory |
| Editor restoration |Initial post-run state reported non-Play/non-compile and no running test; direct MCP code read confirmed original04_WoodsPursuit active and clean, override null. No manual scene save/load required |
| Final health |`editor/state` stayed stale (age93s, ready_for_tools=false) after one instances refresh, instance reselection and telemetry ping. Other direct MCP reads succeeded; overall fresh readiness not verified |
| Evidence files |`K:/Game/Unity/Dave/Arena/Logs/EXP0036/summary.md` and `job-31a3c491912e4108ad91881373796fd8.json` |

Two same-prefix temp directories were seen after the test, with no pre-run directory count/ownership record. They were preserved; no claim is made that they belong to this run, or that every temporary artifact was removed. The test passed through its fixture teardown; no broad cleanup was attempted.

## Conclusion and next action

A real MCP PlayMode test can run in the designated existing project using its warm Editor/cache. This supports that limited execution path, not retrospective MCP acceptance of the delivered Arena scenes (the selected case exercises the original project's existing clinic/Ink flow). GitHub can inspect this record; execution remains local reported evidence, not GitHub CI or independent reproduction.

The post-Play stale resource was a concrete workflow blocker. Under the implementation guide, no further Unity commands were sent after the one allowed instance/ping recovery attempt failed to produce a fresh state, until the user confirmed reconnecting the original project bridge. Reconnect the bridge in the same original Editor, then verify project identity plus a fresh ready snapshot before another experiment. Do not reinstall/upgrade the plugin, restart into a different project, clear caches or switch to CLI to hide the stale state.

The first attempt to read broad reflection metadata through execute_code returned failure without diagnostics; simpler fixed-type/script reads succeeded via the same MCP connection. A single tool failure was not treated as evidence that all MCP operations were unavailable.

This narrow baseline has mixed results: reconnection temporarily restored readiness, but the later Play follow-up reproduced the stale-state blocker. No full-game/manual UI acceptance, player-save migration, new gameplay feature or formal policy change is claimed.

## Same-project reconnection verification — 2026-11-07

After the user reported Disconnect → Connect in the original Editor, read-only MCP checks confirmed the same exact project/Unity/instance and clean04_WoodsPursuit scene. Three state snapshots had advancing observation timestamps over approximately109.6s, with stale=false and ready_for_tools=true (ages310/102/300ms). The latest state was idle, non-Play, not compiling/reloading/updating and with no running test. An initial external_changes_dirty flag cleared in later snapshots; no refresh or compilation was requested by this verification. A domain reload occurred between the first two snapshots, so the sequence reset19→4 is not used as a monotonic freshness claim.

Git HEAD remained96f4520d672394e57bcee5532dca7cbed29f6258 and the26 default status rows still matched the audit. Evidence: `K:/Game/Unity/Dave/Arena/Logs/EXP0036/reconnect-check.json`.

Operational label at that reconnection check: **MCP_CONNECTED_READY**; see the later follow-up for the current blocked state. No tests were rerun, scenes modified or caches cleared. This verifies recovery now, not a permanent root-cause repair or stability after another Play cycle.

## Follow-up 1 — one Play cycle after reconnection

Status: one case passed; freshness failure reproduced and BLOCKED_MCP reinstated. This is a targeted check of the unresolved post-Play freshness concern, using the same existing case as a stimulus rather than adding another gameplay feature or repeating the whole suite.

Predeclared observations: fresh target/idle state before the run; one exact-case MCP job; editor/state immediately after completion and near30s/90s; direct post-run Play/scene/save-override check; original Git status comparison. Record observed timestamps, stale/ready/age, sequence and any domain reload. If a checkpoint is stale, preserve it before the single allowed instances/reselect/ping recovery; if still stale, stop with BLOCKED_MCP rather than forcing more samples or switching to CLI.

Current preflight confirmed the original project/version, fresh readiness, external_changes_dirty=false and the preserved26-row source state. No new project, Editor process or Library is planned. One successful cycle would support that cycle only, not prove the underlying stale-resource defect permanently repaired.

### Follow-up 1 result

Job `3b59af3fcd5e47f1bfd59c665fc7ceb6`:1 passed,0 failed/skipped; case3.923s, job wall19.003s. Pre-run state was fresh (age306ms, focused=false). Immediately after terminal result the cached state was focused=true, sequence15, observed1794065825581, age21.244s, stale=true/ready=false. The one permitted instances/reselect/ping recovery retained the same observation timestamp and sequence, with age50.891s. This reproduces the post-Play false-readiness/staleness concern; direct post-run MCP readings still found original scene clean and no Play/compile/import/override, with Console errors0 and matching26 Git-status rows.

The30s/90s checkpoints were skipped under the stop-on-stale policy; no further Unity calls, CLI fallback or new Editor followed. A pre-run set of two existing test-temp directories was recorded and preserved; no post-run cleanup conclusion is inferred. Source fixture SHA256:591f2ae73a5ef6834cc4dfe13023d1bdefd318e8b9b4450a92c50a7e00af8a46.

Evidence: `K:/Game/Unity/Dave/Arena/Logs/EXP0036/followup-1/summary.md` and `snapshots.json`. Read-only installed-code diagnosis and an unapplied patch proposal are in [EXP0036-StateFreshnessDiagnosis.md](EXP0036-StateFreshnessDiagnosis.md). Current operational status is **BLOCKED_MCP**; the earlier reconnect recovery is historical, not a permanent repair.
