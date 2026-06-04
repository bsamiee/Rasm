# [H1][BRIDGE_RAIL]
>**Dictum:** *One global live-Rhino lane; verify folds per-scenario JSON into a `VerifySummary` carrying only the bridge facts the fold cannot derive.*

## [1][PURPOSE]

`rails/bridge.py` owns the `bridge` claim — the sole C#-only rail that drives a *running* `RhinoWIP.app` through the in-process bridge client. Its seven verbs (`verify`, `doctor`, `launch`, `quit`, `check`, `clean`, `build`) share one exclusive `bridge.lock` lease (D34 liveness) so the fleet runs exactly one live-Rhino proof lane (§7); `build` alone is lease-free (closure compile, never touches the live host). Each verb folds `dotnet run --no-build` client invocations through the Engine into one `Report`; `verify` alone attaches the `VerifySummary` detail (`kind="verify"`, D13). The handler arity is the `Handler` contract `(settings, scope, params)` — the registry opens the `ArtifactScope` and threads it in, so `report_dir` is `scope.path / "verify"`. No `retried` on the rail seam (D2): the rail stack is `checked ▷ logged ▷ traced`, and the retry-correlation key is bound in `traced` (the engine seam), not `logged`, so a retried spawn logs under the same `run_id`.

## [2][CANONICAL_SHAPES]

`VerifySummary` is the lone `Detail` variant this rail owns. It carries only bridge-specific fields that the success-channel fold cannot reconstruct from `Completed` outcomes; the `ok/failed/total` triple lives on `Report.counts` (D16), never duplicated here.

```python
class VerifySummary(Detail, frozen=True, tag="verify"):          # forbid_unknown_fields, tag_field="kind" (§5)
    exceptions: int = 0                                          # summed in-Rhino exception reports across scenarios
    report_dir: str = ""                                         # .artifacts/assay/bridge/<run_id>/verify
    first_failure: str = ""                                      # scenario stem of the earliest non-zero exit, else "" (model.md canonical)
    first_fault_phase: str = ""                                  # phase where first_failure stopped: launch|execute|check|cleanup
    first_fault_output: Annotated[str, Meta(max_length=256)] = ""  # ≤256 chars of that phase's earliest diagnostic (model.md canonical)
```

| [FIELD] | [WHY NON-DERIVABLE] | [SOURCE] |
| ------- | ------------------- | -------- |
| `exceptions` | In-Rhino exception count is parsed from scenario JSON, absent from process exit | `_BridgeDiagnostics.exception_reports`, summed across `execute` phases |
| `report_dir` | Retention TTL consumers need the directory path, not the artifacts | `scope.path / "verify"` |
| `first_failure` | First-fail scenario identity is an ordering fact, not a count | first row with `status.exit_code != 0`, else `""` |
| `first_fault_phase` | The failing phase of `first_failure` lets an agent retriage without opening the report dir | `_first_fault`: first phase with `status.severity > OK` |
| `first_fault_output` | The earliest diagnostic snippet of that phase, bounded to 256 chars | `_phase_diagnostic`: stderr → stdout → exception-roster head |

The rail constructs two module-level `Tool` rows itself: `_CLIENT_TOOL` (`DOTNET / FILES / RUN / C# / BRIDGE`, `command=("run","--no-build","--project")`, the per-verb client driver) and `_BUILD_TOOL` (`DOTNET / PROJECT / BUILD / C# / BRIDGE`, lease-free closure compile). The catalog's `Claim.BRIDGE` slice carries one further row — `rasm-bridge` (`Mode.VERIFY`, `parser=parse_verify`) — held live by reference through `_bridge_tool_census` so a renamed catalog row is an import-time error. `UNSUPPORTED` (exit 3, D29) fires when discovery matches zero `*.verify.csx` scenarios — a valid precondition with no applicable path (§8). Discovery is the predecessor's three-stage fold: **direct** `*.verify.csx` path → **directory** containing csx files → worktree **glob** expansion (`**/{pattern}` when the token carries no path glyphs in `/*?[`).

## [3][VALIDATED_SNIPPET]

`verify` is a plain `(settings, scope, params)` `Handler`: it acquires `bridge.lock` via the **callback** form of `leased` (not a context manager) and delegates the work window to `_verify_locked`. The leased body sequences `TTL → build → launch` on the `Result` rail with `.bind`, then folds discovered scenarios in `_fold_scenarios`. The returned `Result` is folded in the registry's `_emit` via statement-form `match` (D26/D27 — never an `@effect.result` generator).

```python
def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    argv = ("bridge", "verify", params.pattern)
    return leased("bridge", lambda _held: _verify_locked(settings, scope, params, argv),    # D34 fail-fast busy
                  settings=settings, run_id=settings.run_id, project="bridge")

def _verify_locked(settings, scope, params, argv) -> Result[Report, Fault]:
    report_dir = Path(scope.path) / "verify"
    _expire_stale(report_dir, _VERIFY_TTL_S)                                                 # retention before launch
    prelude = (_ensure_dir(report_dir)
               .bind(lambda _: _build_closure(settings))                                     # protocol+plugin+client
               .bind(lambda _: _client_run(settings, "launch").map(lambda _run: None)))
    match prelude:
        case Result(tag="ok"):
            return _fold_scenarios(settings, report_dir, _discover(settings, params.pattern), argv)
        case Result(error=fault):
            return Error(fault)                                                              # Fault(status=FAULTED)

def _fold_scenarios(settings, report_dir, scenarios, argv) -> Result[Report, Fault]:
    match scenarios:
        case ():
            return Ok(fold(Claim.BRIDGE, "verify", (receipt(argv, 3, status=RailStatus.UNSUPPORTED),)))
        case _:
            rows = tuple(_run_scenario(settings, report_dir, s) for s in scenarios)
            first = next((r for r in rows if r.status.exit_code != 0), None)                  # ordering fact, eager
            summary = VerifySummary(
                exceptions=sum(r.exceptions for r in rows),
                report_dir=str(report_dir),
                first_failure=first.stem if first is not None else "",
                first_fault_phase=first.fault_phase if first is not None else "",             # phase + snippet lifted off one row
                first_fault_output=first.fault_output if first is not None else "",
            )
            return Ok(fold(Claim.BRIDGE, "verify", tuple(r.completed for r in rows), detail=summary))
```

`_run_scenario(settings, report_dir, scenario)` issues `_client_run(settings, "check", str(scenario), "--result", str(path), timeout=_SCENARIO_TIMEOUT_S)` — `dotnet run --no-build --project <client> --configuration <conf> -- check …` — then decodes the per-scenario JSON (result file first, captured `stdout or stderr` fallback), projecting an `_Scenario` row carrying `status`, `stem`, `exceptions`, `fault_phase`/`fault_output` (from `_first_fault`), and the `Completed` receipt the fold reduces. A rail-level spawn/timeout `Fault` projects to a `FAILED` row tagged `fault_phase="launch"` with the bounded `Fault.message` as its snippet, never short-circuiting — the comprehension stays total.

## [4][SEAMS]

| [NEIGHBOR] | [SEAM] | [DIRECTION] |
| ---------- | ------ | ----------- |
| `core/engine.py` | `run_check` spawns the `dotnet run` client (`scope=None`, optional `deadline`); `leased("bridge", cb, …)` callback form + `psutil` liveness (D34) | bridge → engine |
| `composition/catalog.py` | `select(Claim.BRIDGE, Language.CSHARP)` yields the `rasm-bridge` verify row (`parser=parse_verify`) out of 39 catalog rows; held live by `_bridge_tool_census` (D22) | bridge ← catalog |
| `composition/settings.py` | `settings.root` (anchors the rail's own `_CLIENT_PROJECT`/`_PLUGIN_PROJECT`/`_PROTOCOL_PROJECT`), `configuration`, `run_id`; the `ArtifactScope` is opened by the registry and passed in — bridge does not call `ArtifactScope.open` itself | bridge ← settings |
| `core/model.py` | `fold(claim, verb, outcomes, *, detail)`/`receipt(argv, rc, *, status)`/`Completed`/`Report`; `VerifySummary` extends `Detail`; `BaseParams` (D58); `RailStatus.join` from `core/status.py` | bridge ← model/status |
| `composition/registry.py` | `Bind(Claim.BRIDGE, verb, bridge_rail.*, BridgeParams, …)`; the rail handler is `compose(checked ▷ logged ▷ traced)(_narrow(bind.handler))` (D2); retry-correlation `run_id` is bound in `traced`, not `logged` (D3) | registry → bridge |
| `rails/package.py` | Shares `bridge.lock` for `rasm-bridge` deploy/publish lifecycle (quit → install → refresh) | sibling |

Lifecycle verbs (`doctor`/`launch`/`quit`/`check`/`clean`) collapse to one `_lifecycle(settings, verb, *args)` fold over `_client_run` under the same lease, each discriminating on `BridgeParams` (which they ignore); `build` is the lease-free closure compile of protocol + plugin + client (`DOTNET / PROJECT / BUILD`). Non-zero client exit → `Completed(FAILED)` (D12); only spawn/lease-miss/timeout produces a `Fault`. The `quit` verb shadows the builtin name (registry verb token) by design.

## [5][EXTENSIBILITY]

A new lifecycle verb is one `Bind` row plus a `Mode`/`BridgeParams` discriminant — never a new module or a new lock; a richer scenario evidence field is one `VerifySummary` attribute (or a `defstruct` detail variant), not a parallel report struct.

## [6][CONSIDERATIONS]

- **TTL expiry must precede launch, never follow it.** Expiring `report_dir` after a populated run would race the freshly-written per-scenario JSON; `_expire_stale` runs the `st_mtime <= cutoff` sweep on the *parent* before the client writes any artifact. `_is_stale` bounds each candidate to `is_relative_to(parent)` so a symlinked report path cannot `rmtree` outside the scope.
- **Decode the per-scenario `--result` JSON first, captured stdout/stderr only as fallback.** The client streams structlog to stderr (logged seam), so trusting stdout alone conflates diagnostics with evidence; `_decode_result` reads the result file when present (`_read_result` → `Result`), falls back to `run.stdout or run.stderr`, and lets a decode miss degrade to a `FAILED` `_BridgeResult` rather than a rail `Fault` — a malformed scenario is a defect, not an operational failure (D12).
- **`first_failure` is an ordering fact the fold discards.** `model.fold` derives counts + status by max-severity `RailStatus.join` (order-insensitive, D15), so the *identity* of the earliest failing scenario cannot be recovered downstream — `_fold_scenarios` lifts the first non-zero-exit row eagerly (`next((r for r in rows if r.status.exit_code != 0), None)`) and projects three facts off it: `first_failure` (stem), `first_fault_phase` (the failing lifecycle phase), and `first_fault_output` (≤256-char snippet). `_first_fault` reads the phase ordering off the wire (`launch`→`execute`→`check`→`cleanup`) and takes the first phase whose `status.severity > OK`; `_phase_diagnostic` prefers a captured stderr stream, falls back to stdout, then to the head exception-report message of an `execute` phase that failed without a stream — so an agent retriages the failing phase + diagnostic straight off the summary, never opening the report dir. `exceptions` is summed independently of `counts.failed` (parsed from `execute`-phase `data.diagnostics`): a scenario can pass its assertion yet still surface in-Rhino exception reports, so the two never collapse.
