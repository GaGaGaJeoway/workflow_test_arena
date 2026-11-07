# EXP-0037 — a short arrival animation and player control handoff

Status: scoped runtime feasibility checks passed (3/3, 0 failed). This is a new gameplay question; no focus/unfocus experiment was repeated.

## Question and minimum scope

Can a short2D arrival animation temporarily stop normal player movement, finish, return movement control, and replay without leaving the player locked?

The original project and existing Editor/cache are reused: `K:/Game/Unity/Dave/UnityTest2DProject`, Unity6000.0.31f1, MCP10.3.0 plus the recorded local heartbeat patch. Starting GitHEAD96f4520d672394e57bcee5532dca7cbed29f6258 has26 pre-existing status rows; those are protected. New experimental files are additive and reported separately. No new project, worktree or package is created.

## Implementation boundary

- New scene: `Assets/Scenes/Experiments/EXP0037/EXP0037_Arrival.unity`, not added to the existing MVP scene catalog or Build Settings.
- New Runtime component: `Assets/Experiments/EXP0037/EXP0037ArrivalControl.cs`, attached to a kinematic Rigidbody2D Player.
- New authored Animator controller/clip: Arrival.controller and Arrival.anim in the same experiment folder. Visitor moves from x3 to0 over1.5s.
- Both actors reuse existing RubyIdleDown1 from `Assets/UnityTechnologies/StarterAsset/Art/Ruby/Characters/PlayerCharacterSheet.png`, with different tint/scale. Existing URP Sprite-Unlit-Default material; no new art generation/import or package installation.
- Space and the prototype button call the same BeginArrival handler. During animation FixedUpdate applies zero movement; after normalizedTime>=1, control returns and the final pose is retained. Another activation restarts the clip.

No existing gameplay source, story, save path or formal policy is intentionally edited. The new scene is outside the MVP bootstrap catalog. Original04_WoodsPursuit scene was clean before authoring and was restored clean after the check without saving it. Existing Git status rows were preserved; this is not a byte-for-byte audit of pre-existing dirty assets. Authoring called AssetDatabase.SaveAssets once, so it could also persist assets already dirty in memory; future authoring should save only owned assets.

## Method and results

On 2026-11-07 UTC (2026-11-08 Taiwan), MCP entered Play once in the original Editor. A bounded in-memory EditorApplication.update observer avoided remote-call latency missing the short animation window. A temporary synthetic keyboard held D and the observer invoked the shared BeginArrival handler directly. This was neither Unity Test Runner nor CLI, and does not establish physical Space/button acceptance.

| Check | Observed result |
| --- | --- |
| Movement locked during actual animation | PASS: at about 0.352 editor seconds, IsPlaying=true, Player X delta=0, Visitor X moved from 3 to 1.718. |
| Control returns on completion | PASS: CompletedCount=1, Visitor X=0; the same held D moved Player from -2 to -1.85. |
| Replay resets and completes | PASS: Visitor immediately reset to X=3, then reached X=0 with CompletedCount=2 and IsPlaying=false. |

Observer interval was about 2.745 editor seconds. This does not measure exact real-time pacing of two 1.5-second clips. The first observer draft failed dynamic compilation because of missing Input System namespaces and an incorrect callback delegate type; it executed no gameplay. The corrected observer executed once. No new project, Library, package, TestRunner job, global InputSettings replacement or cache deletion was used.

Cleanup released/removed the synthetic keyboard, restored the prior current keyboard when available, and unregistered the observer. Play stopped normally; original scene04_WoodsPursuit was restored clean. Final MCP state was fresh-ready, idle, non-Play/non-compile with no active observer and zero Console errors. No MVPGameController was present in the experimental scene. No screenshot or manual visual acceptance was performed.

## Source delivery and evidence boundary

The tested environment was HEAD96f4520 plus its pre-existing dirty state and these new, then-untracked assets. The 12 new experiment files were subsequently published as private [commit 3b67b575](https://github.com/GaGaGaJeoway/UnityTest2DProject/commit/3b67b575b1bb38b5b49cae31bb0ebe9a6f4d238a), branch codex/arena-exp0037, parent96f4520. A separate publication index preserved the original branch, HEAD and staged entries; no worktree or checkout was created. The original 26 status rows remain, plus four status groups for the new experiment folders/meta.

[Source manifest](EXP0037-SourceManifest.json) records tested working-file SHA-256 values, published Git blobs and publication SHA-256 values. Publication normalized line endings/trailing whitespace only; original working files were untouched. The [public C# snapshot](RuntimeSnapshots/EXP0037ArrivalControl.cs.txt) matches the published script. Existing Ruby sprite, URP material and project package dependencies remain in the private project; this public report is not a portable EXP37 Runtime delivery.

GitHub can verify the source commit, script snapshot and manifest. The [reported local result JSON](EXP0037-LocalResult.json) preserves the numeric observations and cleanup report; publishing this report does not independently verify execution. Runtime observations are local MCP reports, not GitHub CI or a clean-checkout rebuild. Local raw evidence is in K:/Game/Unity/Dave/Arena/Logs/EXP0037 (result.md/json, observer.cs.txt, publication.json).

Not checked: physical Space/button input, standalone Player/build, clean-checkout import, integration into the main MVP flow/save system, interruption/skip or broad QA. The result supports only arrival-animation movement lock, completion handoff and replay in this dedicated scene.

Fixed source review (96f4520...3b67b575): Standards 0 actionable findings; Spec 0 findings. Reviewers did not operate Unity or rerun tests. Phase Complete / Sufficient for Feasibility; future work should ask a different gameplay question rather than repeat these three checks.
