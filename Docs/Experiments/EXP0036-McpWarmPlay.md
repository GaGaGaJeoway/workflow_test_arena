# EXP-0036 — original-project warm MCP Play workflow

Status: scoped MCP case passed; final editor/state freshness is BLOCKED_MCP. Executed after EXP-0035 closeout, with no new project or CLI fallback.

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

The post-Play stale resource is a concrete workflow blocker. Under the implementation guide, no further Unity commands were sent after the one allowed instance/ping recovery attempt failed to produce a fresh state. Reconnect the bridge in the same original Editor, then verify project identity plus a fresh ready snapshot before another experiment. Do not reinstall/upgrade the plugin, restart into a different project, clear caches or switch to CLI to hide the stale state.

The first attempt to read broad reflection metadata through execute_code returned failure without diagnostics; simpler fixed-type/script reads succeeded via the same MCP connection. A single tool failure was not treated as evidence that all MCP operations were unavailable.

This experiment is complete as a narrow baseline measurement with an explicit remaining freshness blocker. No full-game/manual UI acceptance, player-save migration, new gameplay feature or formal policy change is claimed.
