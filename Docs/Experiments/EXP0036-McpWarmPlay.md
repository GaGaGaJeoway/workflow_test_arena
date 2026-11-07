# EXP-0036 — original-project warm MCP Play workflow

Status: planned; gameplay has not executed. Start after EXP-0035 closeout.

Question: can the designated existing 2D project complete a useful Play check through MCP using its current Editor/cache, without a replacement project or CLI fallback?

Target: `K:/Game/Unity/Dave/UnityTest2DProject`, Unity `6000.0.31f1`, MCP `10.3.0`, branch `codex/scene-entry-safety`, HEAD `96f4520d672394e57bcee5532dca7cbed29f6258` plus preserved dirty state. Capture the actual state again before execution; these values are not a substitute for that check.

Minimum scope:

1. MCP project/info and fresh editor/state must identify the target and show no competing test or Play run. Preserve the starting scene and source changes.
2. Inspect the existing `SceneUiIntegrationTests.NewGameLoadsClinicAndPresentsInkDialogue` case and its setup/teardown before selecting it. It must use the existing isolated SaveService seam, protecting player saves. No new gameplay assets or test scripts are planned.
3. Run only the selected existing PlayMode case through MCP Test Runner. Retain its single job ID, real executed count, result and elapsed time. Do not treat a zero-test job as a pass.
4. Verify the underlying run has ended and the Editor is no longer playing; restore the starting scene if the test changed it, without saving or overwriting existing assets. Record Console errors only when needed.

Report project path, Git HEAD/dirty state, Unity version, MCP instance, exact case, result/job, timings and restoration. This is a baseline for the original-project MCP workflow, not retrospective acceptance of EXP-0015–0035 or a new product QA requirement.

If MCP becomes unavailable, report BLOCKED_MCP and diagnose. Do not start a CLI substitute, another Editor, a new project or cache deletion without separate user approval. Original-project branch/source integration and formal policy changes are outside this experiment.
