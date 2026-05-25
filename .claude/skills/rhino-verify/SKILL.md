---
name: rhino-verify
description: >-
  Verifies Rhino/Grasshopper C# behavior on macOS via the in-process bridge,
  producing JSON evidence + PNG viewport captures per scenario. Use when
  validating extrusion/intersection/projection/extraction behavior, plugin
  load-and-execute correctness, or any change that needs real RhinoCommon
  evidence rather than static analysis. Companion to `coding-csharp`.
---

# [H1][RHINO-VERIFY]
>**Dictum:** *Static analysis proves the code compiles; the bridge proves it behaves.*

---
## [1][WHY_THIS_EXISTS]

Rhino has no working headless mode on macOS as of Rhino 9 WIP 9.0.26132 (May 2026): `Rhino.Inside` does not host Rhino on macOS, `Rhino.Testing` is `rhino-8.x` Windows-only, `rhino.compute` is Windows/Linux, and the `runscript`/`-_exit` flags are Windows-only. The only path that works is in-process execution through a launched `RhinoWIP.app` via the named-pipe bridge in `tools/rhino-bridge/`. Build the test loop around that fact; do not propose Docker, VM, or Rhino.Inside paths.

[CRITICAL]: Sources older than 6 months on "headless Rhino on Mac" are stale. Re-verify before quoting.

---
## [2][TWO_RAILS]

| [RAIL] | [WHEN] | [COMMAND] | [FAILURE_MODE] |
| --- | --- | --- | --- |
| Static (xUnit) | Pure C# logic — algebras, folds, projections, validation that does not require Rhino runtime | `bash scripts/test.sh [<filter>]` | Compile or assertion failure; non-zero exit |
| Scenario (Rhino bridge) | Anything that touches `RhinoCommon`, `Grasshopper2`, plugin behavior, geometric correctness, or needs a visual oracle | `bash scripts/rhino.sh verify <path-or-glob>` | Phase JSON status != `ok`; non-zero exit; summary JSON written to `.artifacts/verify/summary.json` |

[ALWAYS] Static first. Push to the scenario rail only when the question requires Rhino's runtime.

---
## [3][STATIC_TESTS_LAYOUT]

Tests live under `tests/csharp/<project>/`, mirroring source layout. `Directory.Build.props` detects test projects by path (under `tests/csharp/`), so any csproj here gets `xunit.v3.mtp-off`, `Microsoft.NET.Test.Sdk`, and `xunit.runner.visualstudio` wired automatically.

- New project: `tests/csharp/<name>/<Name>.Tests.csproj` with a single `<ProjectReference Include="../../../<source-path>/<Source>.csproj" />`.
- Add the path to `Workspace.slnx` under `/tests/csharp/<name>/`.
- Run targeted: `bash scripts/test.sh "FullyQualifiedName~MyArea"`.

[NEVER] Run tests through `scripts/check-cs.sh`. That script is build/format/analyze only. Tests are a separate gate.

---
## [4][SCENARIO_CONVENTION]

A scenario is a single `*.verify.csx` file co-located with the code it exercises (typical path: `apps/grasshopper/<plugin>/Scenarios/<name>.verify.csx`). It is a RhinoCode C# script executed through the bridge inside the running `RhinoWIP.app`.

The runner prepends two `const string` declarations to every scenario before execution:

```csharp
const string SCENARIO_NAME = "<file-stem>";
const string CAPTURE_PATH  = "<absolute-png-path>";
```

[NEVER] Declare `SCENARIO_NAME` or `CAPTURE_PATH` yourself — the runner injects them. Use them; do not shadow them.

Scenario shape (in this order):

1. **Setup** — `var doc = RhinoDoc.ActiveDoc ?? throw …;` then `doc.Objects.Clear();`.
2. **Deterministic input** — build geometry from constants (no I/O, no randomness, no model file dependency).
3. **Behavior** — call the production code under test (RhinoCommon API, the plugin's exposed surface, or `Rasm.Analysis.*` operations).
4. **Assertions** — `throw new InvalidOperationException(...)` on each failed predicate. The bridge captures the throw into the `execute` phase fault.
5. **Visual oracle (optional but recommended)** — `ZoomExtents` on the active view, capture to `CAPTURE_PATH`.
6. **Evidence** — `Console.WriteLine($"key=value")` lines for any quantity the assertion depends on. These show up in the result's `outputs[].text`.

[CRITICAL] macOS `ViewCapture` quirk (Rhino 9 WIP, regression open since March 2026): `CaptureToBitmap(view)` ignores the `view` parameter and captures the **active** viewport. Workaround: set `doc.Views.ActiveView` (or operate on whatever `ActiveView` returns) and `view.Redraw()` before capture. Off-screen / parallel-viewport captures are unreliable until McNeel fixes the bug.

Reference scenario: `apps/grasshopper/Radyab/Scenarios/extract-points.verify.csx`.

---
## [5][RUN_LOOP]

Pre-flight (once per session, when Rhino is not already running with the bridge):

```bash
bash scripts/rhino.sh bridge build      # builds protocol + plugin + client
bash scripts/rhino.sh bridge package $V # only when the plugin source changed
bash scripts/rhino.sh bridge install
bash scripts/rhino.sh bridge launch     # opens RhinoWIP, verifies hello
```

Per iteration:

```bash
bash scripts/rhino.sh verify apps/grasshopper/Radyab/Scenarios   # all
bash scripts/rhino.sh verify Radyab/Scenarios/extract-points    # one (regex)
bash scripts/rhino.sh verify path/to/single.verify.csx          # explicit
```

Reading results:

- Per-scenario evidence: `.artifacts/verify/<name>.json` — full bridge phase JSON, same shape as `bridge script`.
- Per-scenario capture: `.artifacts/verify/captures/<name>.png` — open with the multimodal Read tool.
- Aggregate: `.artifacts/verify/summary.json` — `{summary:{ok,failed,total}, scenarios:[…]}`.

A scenario passes when its top-level `status` is `ok`. The runner exits non-zero when any scenario fails.

---
## [6][AUTHORING_CHECKLIST]

Before committing a new scenario:

- [ ] File name ends in `.verify.csx`.
- [ ] Does not declare `SCENARIO_NAME` or `CAPTURE_PATH`.
- [ ] Calls `doc.Objects.Clear()` at the top (scenarios share one Rhino session).
- [ ] Inputs are deterministic constants — no `Random`, no file reads, no time-dependent values.
- [ ] Assertions are *behavioral*, not tautological (`bbox.Diagonal.Length == sqrt(300)` is real; `brep != null` after `ToBrep()` is weak).
- [ ] Throws `InvalidOperationException` with a message that names the failing quantity and observed value.
- [ ] If capturing, sets `ActiveView` and calls `view.Redraw()` before `CaptureToBitmap(view)`.
- [ ] Emits structured `key=value` lines so failure forensics do not require re-running.
- [ ] Runs green locally: `bash scripts/rhino.sh verify <path-to-new-scenario>`.

---
## [7][BLOCKERS_ENCOUNTERED]

[IMPORTANT] Append, never rewrite. Each entry: date, blocker, root cause, fix. Future invocations of this skill consult this section before guessing.

- **2026-05-20 — `Rhino.Inside`/`Rhino.Testing` paths explored and rejected for macOS.** Cause: McNeel ships Windows-only for both. Fix: route all runtime evidence through `scripts/rhino.sh bridge` against a launched `RhinoWIP.app`. Source: `https://github.com/mcneel/rhino.inside/blob/rhino-9.x/CHANGELOG.md`, `https://github.com/mcneel/Rhino.Testing`.
- **2026-05-20 — `ViewCapture.CaptureToBitmap(view)` captures the wrong viewport on macOS.** Cause: unfixed Rhino 9 WIP regression (forum thread `t/capture-specific-view-to-tif-file-on-macos/216528`). Fix: set `doc.Views.ActiveView` to the target before capture; do not rely on the `view` parameter.

<!-- New blockers below this line. -->
