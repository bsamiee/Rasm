# [02] Python Integration Map — The Linkage Question

Scope: how `tools/rhino-bridge` is consumed from Python (`tools/quality` legacy rail, `tools/assay` successor rail), the full package-update → quit → relaunch choreography with per-step failure surfacing, and an evidence-based answer to "is the linkage backwards?". Sources: repo code as of 2026-06-10 (paths cited inline); no live-Rhino interaction.

## [1]-[CONSUMER_TOPOLOGY]

Two Python stacks consume one C# client today. Both spawn `dotnet run --no-build --project tools/rhino-bridge/client/... -- <verb>` per command — there is no persistent client process, no library binding, no socket from Python.

| [LAYER] | [SURFACE] | [VERBS] | [NOTES] |
| ------- | --------- | ------- | ------- |
| quality CLI (legacy) | `tools/quality/__main__.py` | `bridge doctor/launch/quit/check/clean/verify/build-bridge/package/deploy/publish` | being archived to `.archive/quality` |
| assay CLI (successor) | `tools/assay/composition/registry.py:751-757` | `bridge verify/doctor/launch/quit/check/clean/build` + `package stage/deploy/publish/list/plan` | mirrors quality near 1:1 |
| C# client | `tools/rhino-bridge/client/ClientVerb.cs` | `doctor`, `launch`, `check <target> [scenario] [--result]`, `clean <target>`, `quit` | exit 2 + usage on parse failure |
| C# plugin (in Rhino) | `tools/rhino-bridge/plugin/Rhino/Transport.cs` | wire: `hello`, `doctor`, `execute`, `quit` | named pipe, 4 instances, `SemaphoreSlim(1,1)` execution gate |

The seam is wide: 4 verb vocabularies (assay verb → quality-style rail fn → client CLI verb → wire command), each with its own parse, status enum mirror, and failure taxonomy.

## [2]-[CAPABILITY_OWNERSHIP_MATRIX]

Who actually owns each piece of intelligence today:

| [CAPABILITY] | [PLUGIN] | [CLIENT] | [QUALITY RAIL] | [ASSAY RAIL] |
| ------------ | -------- | -------- | -------------- | ------------ |
| Pipe server, UI-thread idle dispatch, RhinoCode compile/run | X | | | |
| Diagnostics capture (command window, `OnExceptionReport`, GH solution state) | X | | | |
| Endpoint record (`~/.rasm/rhino-bridge.json`: pipe, PID, start time) | writes | reads + validates + retires | | |
| Launch (`open -nosplash`), connect polling, liveness probe | | X (`Program.cs:314-391`) | | |
| Quit escalation: wire quit → JXA terminate → SIGKILL | | X (`Program.cs:221-259`) | | |
| Crash-marker reconcile (`.rhl`, autosave `.3dm`, `Rhinoceros-*.ips`) | | X (`Program.cs:260-288`) | | |
| Rhino bundle discovery (newest `Rhino*.app` via `CFBundleVersion`) | | | X (`settings.py:138-156`) | [!] dropped — bridge launch relies on ambient `RHINO_WIP_APP_PATH` |
| Per-scenario `dotnet restore/build/msbuild ResolveReferences` of the TARGET project | | X (`Program.cs:399-431`) | | |
| NU1004/NU1403 lock-drift classification | | X (`Program.cs:433-440`) | | |
| Scenario assembly (preamble hoist, base usings, LanguageExt bootstrap, `SCENARIO_NAME`/`CAPTURE_PATH`) | | X (`Program.cs:503-530`) | | |
| Reference shadow-copy with content fingerprint | | X (`Program.cs:585-623`) | | |
| Source-owner resolution (git ls-files + MSBuild per csproj) | | X (`Build.cs:97-126`) | duplicate by path convention (`_verify_resolve`) | duplicate by path convention (`_resolve_project`) |
| Bridge-closure build (protocol+plugin+client+testkit) | | | X | X |
| Process-global bridge lease (flock, busy→exit 5, stale-steal) | impl. by exec gate only | none | X (`with_bridge_lease`) | X (`bridge_lease`) |
| Scenario discovery (glob/`fd`, `**/<stem>` expansion) | | | X | X |
| `BridgeResult` JSON decode (msgspec mirror structs) | n/a (writer) | writer | mirror #1 | mirror #2 |
| Facts/captures decode (regex over execute-phase stdout markers) | emits markers | passes through as truncated text | regex #1 (`bridge.py:162-163`) | regex #2 (`bridge.py:55-56`) |
| Verify summary fold, first-failure triage, report TTL expiry | | | X | X |
| Yak staging (msbuild meta, atomic dir rotation, host-assembly excludes) | | | X (`package.py`) | X (`package.py`) |
| Quit→install→refresh choreography policy | | | X (`_PACKAGE_POLICY`) | X (`_STEP_POLICY`) |

Reading of the matrix: the C# client is the single smartest layer (lifecycle, reconcile, build, scenario assembly). Python contributes lease + discovery + decode + fold + yak — and contributes it TWICE, in two near-duplicate stacks (~795 LOC quality, ~1,188 LOC assay for the same two rails). The same wire shape (`BridgeResult`/`BridgePhase`/`BridgeFault`) is declared four times across the system (protocol C#, client C#, quality msgspec, assay msgspec), and the same two fact/capture regexes are declared twice.

## [3]-[THE_LEASE_SEAM]

- Python owns mutual exclusion: `leased()` flock on `.artifacts/<...>/locks/bridge.lock`; busy → typed exit-5 (`tools/quality/process.py:295-305`, `tools/assay/core/engine.py:998-1135` with stale-owner steal).
- The C# client has NO locking. Any direct client invocation (or a second workflow) bypasses the lease and races quit/launch/endpoint retirement. The plugin's `SemaphoreSlim(1,1)` serializes `execute` only — it cannot protect against a concurrent `quit` SIGKILLing the host mid-scenario.
- Consequence: the safety property "one RhinoWIP, serialized access" is enforced in the wrapper, not at the resource. Capability and its guard live in different languages and different processes.

## [4]-[PACKAGE_UPDATE_CHOREOGRAPHY]

Why the relaunch cycle exists at all: the `.rhp` plugin (+ its `Protocol` dependency) is loaded into Rhino's default ALC at startup and cannot hot-reload; yak packages are scanned at startup only. So any plugin/protocol change requires quit → `yak install` → cold relaunch. Production-library changes do NOT require it — scenarios shadow-copy the target assembly per execute (content-fingerprinted refs dir), so a rebuilt `Rasm.*.dll` is picked up by the next `verify` against a still-running host.

End-to-end (`deploy`/`publish` with slug `rasm-bridge`; quality rail shown, assay equivalent noted):

| [#] | [STEP] | [CODE] | [FAILURE MODE] | [SURFACES AS] |
| --- | ------ | ------ | -------------- | ------------- |
| 1 | package lease (`package-stage-<dir>` / `package-<slug>`) | `package.py:222` / assay `:421,558` | held by live process | typed BUSY, exit 5 |
| 2 | slug → csproj resolution (scan `apps/`+`tools/`) | `_resolve_project` | 0 or duplicate slugs | typed fault |
| 3 | `dotnet msbuild -getProperty:` meta + validation (slug match, `.rhp`, target-dir containment, yak executable) | `_evaluate_meta`, `PackageMeta.validate` | missing props / validation | typed fault |
| 4 | `rmtree(target_dir)` | `package.py:226` | OSError | [SWALLOWED] `ignore_errors=True` |
| 5 | versioned `dotnet build` (serial) | `stage_locked` | compile error | typed fault |
| 6 | manifest + primary `.rhp` existence | `filter_with` | missing | typed fault |
| 7 | copy artifacts → temp stage (host assemblies excluded) | `package.py:271-281` | OSError | [!] quality: UNCAUGHT — raw traceback, stage dir leaks; assay catches (`_copy_tree`) |
| 8 | `yak build` in stage | check=True | non-zero | typed fault (quality); FAILED report row (assay — non-zero yak stays on `Completed`) |
| 9 | atomic commit: `package_dir` → `.previous.<pid>` → stage promote | `commit()` / `_commit` | OSError | typed fault + previous-dir restore |
| 10 | exactly-one `.yak` glob | `_finish` / `_resolve_package_file` | 0 or >1 | typed fault |
| 11 | bridge lease | `with_bridge_lease` / `bridge_lease` | busy | typed BUSY, exit 5 |
| 12 | build client (quality prelude only) | `build_client` | compile error | typed fault |
| 13a | QUIT — graceful wire quit (plugin marks docs clean so terminate skips save prompts) | `TryPrepareQuitAsync` | host unreachable | [SWALLOWED] recorded as `prepared:"unreachable(...)"`, phase still Ok (best-effort by design) |
| 13b | QUIT — JXA `NSRunningApplication.terminate` | `ForceCloseAsync` | osascript non-zero | [SWALLOWED] exit code discarded (`_ =`) |
| 13c | QUIT — wait 30s → SIGKILL → wait 30s | `ForceCloseAsync`/`ForceKill` | still alive after both | [SWALLOWED] `closed:false` in phase data but phase status Ok → `client_quit` sees OK → rail proceeds |
| 13d | QUIT — retire endpoint file | `RetireEndpoint` | any | [SWALLOWED] NonFatal catch |
| 13e | QUIT — clear `.rhl` + `Unsaved RhinoWIP Document.3dm` + `Rhinoceros-*.ips` | `ClearRecoveryMarkers` | per-file IO/permission | [SWALLOWED] per-file best effort |
| 14 | `yak install <pkg>` | `_yak_argv` | non-zero | typed fault (quality); FAILED row (assay) |
| 15a | REFRESH — `launch`: Hello (3s) → expect fail → reconcile again (marker backstop) → `open <app> --args -nosplash` (30s) | `LaunchPhaseAsync` | `RHINO_WIP_APP_PATH` unset; `open` fails | phase Failed → exit ≠0 → typed fault |
| 15b | REFRESH — `doctor`: Hello/connect poll then doctor request | `DoctorAsync` | plugin never answers | connect phase Failed after 35s (Transport) / launch verb 90s (Connect) → typed fault |
| 16 | report fold | `_merge_stage` etc. | — | JSON envelope |

Timeouts: `TimeoutPolicy` (`protocol/BridgeWire.cs:262-280`) — Hello 3s, Connect 90s, Transport 35s, QuitWait 30s, IdleDispatch 300s, LivenessSettle 4s; one global `RASM_BRIDGE_TIMEOUT_SCALE` knob.

### [4.1]-[CRASH_MARKER_MECHANICS]

- Markers: `~/Library/Autosave Information/Unsaved RhinoWIP Document.3dm{,.rhl}` + `~/Library/Logs/DiagnosticReports/Rhinoceros-*.ips`. Present at cold open → Rhino's recovery dialog blocks startup (and the plugin's pipe server, which starts on first `RhinoApp.Idle`).
- Clear points: (a) inside every quit reconcile; (b) backstop inside `LaunchPhaseAsync` before cold open — covers `.ips`/autosave written asynchronously AFTER the quit-time clear (post-SIGKILL `ReportCrash` writes are racy; the comment at `Program.cs:283` acknowledges this).
- Residual window: a marker landing between the launch-time clear and Rhino's own startup scan is unmitigated. The failure then presents as a modal dialog the bridge cannot see: Hello never answers → 90s connect timeout → fault text says "did not answer", not "a recovery dialog is blocking startup". This is precisely the operator's "forced-quit / crash-recovery dialogs" pain: the failure is real but the diagnosis is absent. No phase ever asserts "markers were present at launch" or "process is alive but pipe silent for Ns ⇒ probable modal".
- Stale-host hazard: if 13c truly failed (`closed:false`), the endpoint file is already retired, so refresh Hello fails, `open` merely ACTIVATES the still-running old Rhino (macOS `open` semantics), the old plugin never rewrites the endpoint file → 90s connect timeout. Surfaces eventually, but as an opaque timeout two steps away from the cause.

## [5]-[VERIFY_RAIL_DATA_FLOW]

Per scenario: Python resolves owner project (path convention) → spawns client `check <csproj> <scenario.verify.csx> --result <json>` → client re-runs `dotnet restore --locked-mode` + `build` + `msbuild ResolveReferences` for the TARGET project (3 process spawns), assembles the script, executes over the pipe, probes liveness, writes result JSON → Python re-decodes the JSON with mirror structs and regex-scans the execute-phase stdout text for `rasm.rhino-bridge.evidence=facts=...` / `...capture=...` lines.

Defects inherent to this seam:

- Facts/captures are typed `FactBag` data in the plugin, flattened to stdout text, capped by `OutputLimit` 32768 per stream. Quality at least carries `stdout_truncated` onto `VerifyScenario`; assay's `_with_facts` silently loses any fact past the truncation point. Evidence is smuggled through a log channel instead of the wire.
- Each scenario pays full client spawn + restore/build/msbuild + Hello handshake — the 3-8s-per-file cost that forces scenario authors to pack unrelated checks into one file (memory: bridge handshake amortization). The cost is an artifact of the process-per-scenario linkage, not of Rhino.
- Quality's verify prelude swallows launch failure: `client_run(..., "launch", check=False)` result discarded (`tools/quality/rails/bridge.py:364`); a non-launchable host then fails N scenarios serially, each burning its own launch-retry + connect window. Assay fixed this (`_faulted(client_run(settings, "launch"))` promotes to Fault and aborts) — proof the two stacks have already drifted behaviorally.

## [6]-[CONTRACT_DRIFT_DEFECTS]

Concrete bugs found at the Python↔C# seam (all verified against current source, none executed):

| [#] | [DEFECT] | [EVIDENCE] |
| --- | -------- | ---------- |
| 1 | `assay bridge clean` can never succeed: rail sends bare `clean` (`rails/bridge.py:525-532`), client requires `clean <target>` exactly (`ClientVerb.cs:56`) → parse fail → usage, exit 2. Quality's `clean(target="")` default hits the same wall. | registry bind says "Clear crash markers + autosave" |
| 2 | `clean` semantics drift: README §3[5] and the assay registry describe crash/autosave cleanup; `CleanAsync` only deletes `.artifacts/rhino/bridge/check/<target>` report dirs. Marker cleanup is reachable solely via quit/launch reconcile. | `Program.cs:139-156` vs `README.md` |
| 3 | Quit phase is unconditionally Ok — `closed:false` (host survived SIGKILL window) and `prepared:"unreachable"` never fail the phase, so `client_quit`'s OK-filter (`quality/rails/bridge.py:306-319`) passes a not-actually-quit host downstream. | `QuitPhaseAsync`, `Program.cs:221-226` |
| 4 | Bundle discovery dropped in migration: quality auto-resolves newest `Rhino*.app` and injects `RHINO_WIP_APP_PATH` into the spawn env (`settings.py:152-156, 250-253`); assay only inherits ambient env (`core/engine.py:197`) and only whitelists the var for REMOTE propagation (`composition/settings.py:139`). Client's error message still says "launch resolves the bundle through the quality operator" — the operator being archived. | cold launch under assay fails on unset env |
| 5 | Owner resolution exists twice with different semantics: Python maps `tests/csharp/libs/<P>/**/scenarios/**` → `libs/csharp/<P>/<P>.csproj` by convention; the client's `check <file.cs>` path does full MSBuild `Compile`-item evaluation over every tracked csproj. Rails never use the latter; direct users never get the former. | `_verify_resolve` / `_resolve_project` vs `ResolveSourcePhaseAsync` |
| 6 | Four declarations of one wire shape; two copies of the fact/capture regexes; two step-policy tables for the same quit→install→refresh choreography. Any wire change is a 4-file, 2-language edit. | §2 matrix |

## [7]-[DIRECTIONAL_VERDICT]

The operator's framing — "intelligence lives in the Python wrapper while the C# bridge is a dumb pipe" — is NOT what the code shows. The client is the smartest single layer; the plugin is appropriately thin; the Python layer is mostly duplicated plumbing (mirror decode, regex scrape, spawn orchestration) plus three genuinely Python-shaped concerns. The real defect is not which side owns intelligence but that the SESSION concept is split across a process boundary with a lossy text seam, twice.

What is genuinely backwards (capability stranded in Python that belongs bridge-side):

- [X] Lease/single-instance ownership — the resource guard should live where the resource is (client or endpoint), not in each wrapper.
- [X] Bundle discovery + launch policy — the client already executes launch but cannot resolve the app; quality's discovery code is dying with the archive.
- [X] Verify-session orchestration — discovery globs aside, the loop "build once, launch once, run N scenarios over one connection" belongs in the client as a session verb; the process-per-scenario shape is the root of the handshake tax and the swallowed-launch class of bugs.
- [X] Facts/captures — typed in the plugin, typed in the wire DTOs, then degraded to regex-over-truncated-stdout for the actual consumer. Should be first-class `BridgePhase.data` fields end-to-end.
- [X] Quit/refresh choreography for self-redeploy — policy tables in two Python files drive client verbs one at a time; a single client `redeploy`/`refresh` verb would own the quit→install→relaunch transaction and could finally report "dialog suspected" with process-alive-but-pipe-silent evidence.

What legitimately stays Python-side (assay as thin typed consumer):

- [X] Repo routing conventions (scenario→project mapping, changed-file scoping) — workspace knowledge, not host knowledge.
- [X] The Assay envelope: uniform Report/Fault/artifact fold across ALL rails is assay's actual value; the bridge should feed it one typed JSON document per session.
- [X] Yak staging/list/plan for non-bridge slugs — pure MSBuild/file orchestration, no Rhino dependency.

Net recommendation for the rebuild: invert the seam, not the stack. One C# bridge CLI owning session lifecycle (discover-bundle, lease, launch, reconcile, N-scenario execution, typed evidence, redeploy transaction) emitting a single machine-decodable result; one assay rail of ~150 LOC that builds it, invokes it, decodes one struct, and folds the envelope. That deletes both regex scrapers, two of four wire-shape declarations, both step-policy tables, and the entire class of verb-arity/semantics drift in §6.

## [8]-[OPEN_QUESTIONS]

- Can the recovery-dialog state be detected positively (AX API / `NSRunningApplication` + window enumeration via JXA) so connect-timeout faults can name the dialog instead of guessing? Untested here.
- Does `yak install` while the old Rhino is still alive (defect §6.3 path) ever corrupt the package dir, or does yak write a fresh versioned dir making the stale-host window merely confusing rather than destructive? Needs a controlled experiment.
- Scenario-kit (`Rasm.TestKit.dll`/`Protocol.dll`) updates: client resolves them from canonical `bin/` per execute — confirm RhinoCode's isolated resolver actually reloads changed copies on a warm host, or whether stale-kit-on-warm-host is another silent staleness path.
- Whether the rebuilt bridge should keep `dotnet run` spawning or expose a long-lived daemon/socket for the client itself — the per-invocation `dotnet run` (~1-2s JIT/launch) is a measurable share of the handshake tax but was not benchmarked in this read-only pass.
