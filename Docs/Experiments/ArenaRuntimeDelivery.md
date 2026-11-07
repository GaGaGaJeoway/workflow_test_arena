# EXP-0035 — minimal Arena Runtime delivery

Research scope: one short flow, imported from private `UnityTest2DProject` commit `62668d6c649ab143260370f700e25e32b2e539fb`, without its historical gameplay, tests, Library or UserSettings. Design backlog: [candidate 16](https://github.com/GaGaGaJeoway/Workflow_instruction_for_2D_game/issues/21).

## Rebuild and play

1. Clone this PR branch from GitHub. Open the repository root with **Unity 6000.6.2f1** and allow Package Manager/import to finish. A licensed Editor, internet/package access and installed platform support are prerequisites.
2. Open `Assets/Scenes/Experiments/EXP0019/EXP0019_SignalDeparture.unity`; enter Play. Click the signal lever twice. In the ferry room click the ticket window, then choose boarding. The ending offers replay; replay clears the arrival signal.
3. For the automated smoke, close this project's Editor first. In PowerShell, set `$env:ARENA_SMOKE_REVISION = git rev-parse HEAD`, then invoke your Unity executable with `-projectPath <absolute-clone-path> -executeMethod ArenaCrossSceneSmoke.Run -logFile <absolute-log-path>`. The probe exits the Editor itself. Use a bounded outer timeout (first import can be slow).
4. Require process exit 0 **and** `ARENA_SMOKE COMPLETE`; the 3 PASS groups cover signal transfer, signalled boarding/replay and direct-entry boarding/replay. Synthetic mouse events call the real input Update/raycast; choice selection invokes the UI's Choose handler. This is not a physical mouse/UI layout acceptance test.

## Dependency boundary

| Part | Delivered dependency |
| --- | --- |
| Scenes | SignalDeparture + NightFerry; exactly these two enabled in Build Settings |
| Runtime | RainyStopInteraction, RainyStopInk, SignalFerryTransfer; no new runtime logic |
| Content | Two `.ink` source files + compiled `.json`, original GUID `.meta`, one 149-byte block sprite, one Noto Sans TC regular font |
| Ink | Vendored existing Ink 1.1.1 libraries/compiler + Unity Editor integration; demos, extras and documentation omitted; MIT licence retained in `Assets/Ink/LICENCE.md` |
| Packages/settings | Public main's existing locked packages, URP settings and Input System; Editor version now explicit. No private manifest copied, no MCP dependency added |
| Verification | Existing ArenaCrossSceneSmoke plus missing-component assertions in both loaded scenes |

`ArenaRuntimeSourceManifest.json` lists original Git blob SHA-1 identifiers at the private source commit. Two copied sources change for this delivery: `ArenaCrossSceneSmoke.cs` (two missing-component checks) and `InkEditorUtils.cs` (Unity 6000.6 asset-creation callback API compatibility); existing public `Assets/Scenes.meta` is preserved. A Git-tree comparison confirms 318 unchanged blobs and two deliberately modified Editor scripts. This is provenance, not a runtime manifest. Dependencies resolved by Unity packages include URP `UniversalAdditionalLightData` and `Sprite-Unlit-Default` material; existing public graphics settings supply the pipeline.

The slice directly uses Input System, URP, Physics2D and IMGUI plus the vendored Ink runtime/compiler. Unused packages already present in the public template are retained to keep this PR focused; this is a minimum added content set, not a claim that the entire template package list is irreducible.

Noto Sans TC is redistributed under SIL OFL: `../NotoSansTC-OFL.txt`, retrieved from `google/fonts` commit `86aced91de05cd4f96a06838124cca45be337b2b` (`ofl/notosanstc/OFL.txt`). No other private project art is included.

## Evidence status

Before this PR, public main contained the Unity template and experiment reports, not this playable slice. GitHub can inspect the scenes, scripts, stories, GUIDs, package lock and provenance. Local cold import and gameplay evidence, followed by the authorized warm repair check below, supports the minimum feasibility of reconstructing and executing this slice. **No complete CLI acceptance under the exit-0-plus-COMPLETE rule is claimed**, because the final OS exit code was not captured. No GitHub CI, new Player build, save/reload or full product QA is claimed. Existing local experiments remain untouched.

## Import failure and repair

The first fresh GitHub checkout (`8b7bb56`, no Library, default shared UPM cache) failed during initial C# compilation, before either Ink JSON regenerated or any smoke ran. Unique failure: `CS0619`, Ink editor integration inherited obsolete `EndNameEditAction`. Unity 6000.6 requires `AssetCreationEndAction` and `EntityId` instead. The repair changes the callback base/signature and passes `EntityId.None` to its caller; no runtime gameplay changes. Reference: [Unity AssetCreationEndAction.Action API](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/projectwindowcallback/assetcreationendaction/action), also confirmed in installed 6000.6.2f1 `UnityEditor.CoreModule.xml`.

Initial compile took 110.98 seconds according to AssetDatabase; launch to diagnosis was about 7m19s. The Editor stayed open after compile failure and was stopped only after verifying its exact project command line. Log and failed clone are preserved locally. Earlier licensing/signature and Burst named-pipe messages did not explain this concrete compiler error. Deleting compiled JSON before first import also caused Unity to delete the orphan `.json.meta` files. This is a test-method problem: regeneration could then change GUIDs and invalidate serialized scene references. The new clone instead keeps both JSON assets/metas and replaces only the JSON contents with an unmistakable sentinel. The probe must explicitly compile both `.ink` sources, remove the sentinel and run the story, while preserving the original JSON GUIDs. No files from the failed clone are reused.

For the new clone, Package Manager caches are isolated with process-only `UPM_CACHE_ROOT` / `UPM_NPM_CACHE_PATH` / `UPM_GIT_LFS_CACHE_PATH` under its ignored Logs directory. Existing user cache/configuration is preserved. This follows [Unity's cache customization documentation](https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/package-manager-caches/upm-config-cache). The installed Editor/licence and operating system are shared; this is not an independent computer or offline-install test. The cold clone regenerated both JSON files with their original GUIDs and reported no C# errors (82.62s compilation). It logged four gameplay PASS markers and COMPLETE, then FAIL during RestoreSettings: ArgumentNullException at InputSystem.settings. No reliable process exit code was collected, so this run is not accepted. Logs and caches are retained; the failure is recorded in Design Issue #23.

## Closeout repair and authorized warm check

Input System 1.20's InputManager.settings setter destroys the previous settings object when its hideFlags equal HideAndDontSave. The smoke kept that transient object as its restore target; replacement destroyed it. The repair snapshots only transient settings before replacement, makes teardown non-reentrant, and writes COMPLETE only after successful restoration. No Runtime gameplay logic changes.

The user explicitly approved one warm CLI exception under Design Issue #23. The existing clone was fast-forwarded to `233569201104bd1c53b904972f7ebe2cb4e28439`; existing generated scene/settings changes, Library and UPM cache were retained, with before/after records. No new clone, cache deletion or original-project operation occurred.

| Field | Observed result |
| --- | --- |
| Project | `K:/Game/Unity/Dave/Arena/arena-exp0035-cold` |
| Method/version | Hidden Editor CLI/Probe, Unity6000.6.2f1; MCP absent for this delivery check |
| Gameplay checks | 4 PASS markers, 0 FAIL; COMPLETE emitted after successful RestoreSettings |
| Compilation | C#47.489s; no C# error; both Ink files compiled, JSON GUIDs unchanged |
| Time | About201s launch to final log write, versus planning estimate1–3min |
| OS exit status | **Unknown**; the launch shell did not retain its Process handle for WaitForExit/ExitCode. No inference of exit0 |
| Shutdown | PID no longer running; log contains clean shutdown/Mono cleanup |
| Other diagnostics | Burst named-pipe errors occurred; they did not prevent these smoke checks from completing |
| Disk | Existing cache reused;0.2GiB was an estimated incremental budget, not measured consumption |
| Log | `Logs/EXP0035-warm-final.log` in the clone; patch/status snapshots alongside it |

Phase closeout: minimum source/dependency reconstruction and gameplay feasibility have local evidence. The transient-settings fix completed its regression path. Reliable OS-exit capture remains an unverified harness criterion, so the whole CLI run is not labelled PASS and the PR remains Draft for user review. Do not launch another CLI attempt without separate approval. Future daily experiments return to the designated existing project/MCP workflow. This warm check is not a fresh checkout of the final commit and does not replace MCP acceptance.
