# [H1][ASSAY_OPERATOR]

`tools.assay` is the Rasm polyglot quality operator — an **agent-first** keychain that routes every C#, Python, TypeScript, Bash, and SQL quality program through one Engine and emits **exactly one JSON `Envelope` to stdout** (structlog rides stderr). It supersedes `tools/quality` (C#-only) and the `package.json` quality scripts.

```bash
uv run python -m tools.assay <claim> <verb> [flags]        # one Envelope on stdout
uv run python -m tools.assay self-test                     # composition + catalog census
uv run python -m tools.assay watch <paths> --on <action>   # NDJSON stream (automation arm)
```

The **executor** (launch/capture/scope/lease/fold) is separate from the **tool** (which program, which args, which language): a program is a `Tool` data row, not a module; a language is one `Language` member + one routing arm, not a folder.

## [1][ARCHITECTURE]

```mermaid
config: { layout: elk, look: neo, theme: base }
flowchart LR
  accTitle: Assay orchestration boundary
  accDescr: CLI routes claim and verb to a rail handler; the engine fans Tool rows into Completed outcomes; one Envelope is emitted to stdout.

  agent["Agent / CI"] -->|"argv"| cli["assay CLI\n__main__.py"]
  trig["Trigger\nwatch · schedule · manual"] -->|"Action"| cli
  cli -->|"claim × verb"| registry["REGISTRY\nclaim×verb → handler"]
  registry -->|"claim, language"| catalog["catalog\nTool rows"]
  registry -->|"language, paths"| routing["routing\ngit / fd / glob / walk"]
  routing -->|"routed inputs"| engine["Engine\nrun_check · fan_out\nexclusive_lease"]
  catalog -->|"Tool rows"| engine
  engine -->|"Completed | Fault"| fold["fold\nReport + Counts"]
  fold -->|"Result[Report, Fault]"| rail["rail\nchecked ▷ logged ▷ traced"]
  rail -->|"one Envelope"| stdout["stdout\n(structlog → stderr)"]

  subgraph aspects ["aspect stack (slot-ordered, two seams)"]
    direction TB
    a0["0: checked (beartype)"]
    a1["1: logged (structlog) — rail only"]
    a2["2: traced (otel) — both seams"]
    a3["3: retried (stamina) — engine spawn only"]
    a0 --> a1 --> a2 --> a3
  end
```

**Abstraction model — four concepts, one composition direction:**

| Concept | Kind | Responsibility |
| ------- | ---- | -------------- |
| `Tool` | Data row | A program: `runner`, `command`, `input` placement, `language`, `claim`, `Mode`, optional `parser`/`thunk`/`timeout`. |
| Engine | One executor | `run_check`/`fan_out` fold any `Check` → `Result[Completed, Fault]`; owns launch, capture, stream, in-process exec, timeout, leases. |
| Rail | A fold | `select`s rows for one claim, `route`s inputs, runs the Engine, folds outcomes into one `Report`. |
| Trigger | Automation | `Watch \| Schedule \| Manual` → an `Action` (rail / program / sequence) under one `anyio` loop. |

**Core invariants (testable, rail/language/trigger-agnostic):**
1. Every quality verb writes exactly one `Envelope` to stdout; automation one per fire (NDJSON); `_emit` is the sole stdout writer; all engine bytes/diagnostics ride stderr.
2. `RailStatus` is the only status type; `Envelope.exit_code == status.exit_code` always.
3. Non-zero process exit → `Completed(FAILED)`, never `Fault`; `Fault` rides `Envelope.error` only (never carries `FAILED`).
4. Adding a program is one `Tool` row; adding a language is one `Language` member + one routing arm + rows — no rail signature changes, no new module. An in-process tool rides `Runner.INPROC` + a `Tool.thunk`, folded through the same `Completed`→`fold` rail as a subprocess.
5. `report.detail` decodes with explicit short tags + `forbid_unknown_fields`; a drifting emitter fails loud.
6. `routing.place` is the sole argv-tail projector; routing is the only language-specific code.
7. Read-only checks fan out under `CapacityLimiter`; exclusive resources fail fast on a held, liveness-verified lease (never block, never nest `anyio.run`).
8. Cross-cutting behavior exists only as aspect layers at two seams — no inline structlog/otel/stamina, no `Engine`/`Parser` `Protocol`.
9. Output is never silently truncated: a capped inline list sets `truncated=true` AND writes the full set to an on-disk artifact.

## [2][ENVELOPE_CONTRACT]

Every invocation writes one JSON `Envelope` line to `stdout.buffer`. `Envelope` inherits `Base`'s `omit_defaults=True`, so the **success wire is terse** — only non-default fields emit. `schema_version` is required (no default), so the wire version tag always appears; `error`/`error_context`/`status`/`exit_code`/`notes`/`truncated`/`command_path` are omitted on a clean OK run and present (non-default) on a fault.

| [FIELD] | [TYPE] | [SEMANTICS] |
| ------- | ------ | ----------- |
| `schema_version` | `int` | Always `1`; always present. |
| `claim` | `Claim` | `static\|code\|test\|bridge\|package\|api\|docs`. |
| `verb` | `str` | The selected verb within the claim. |
| `status` | `RailStatus` | Wire token; see [3]. Omitted on the wire when `OK` (the default). |
| `exit_code` | `int` | `== status.exit_code` always. Omitted when `0`. |
| `run_id` | `str` | `%Y-%m-%dT%H-%M-%S.%f-{pid}`; `ASSAY_RUN_ID` overrides for CI correlation. |
| `duration_ms` | `float` | Wall time of the invocation. |
| `report` | `Report \| None` | `{claim, verb, status, counts, artifacts, results, notes, detail}`; `None` on a Fault. |
| `error` | `Fault \| None` | `{argv, status, message}` — no `returncode`, no `detail`, no `stderr`. Present only on the Error channel. |
| `error_context` | `Diagnostic \| None` | Auto-diagnosis on a Fault: `{failing_step, recent_events, elapsed_ms, hint, dispatched}`; absent on success. `dispatched` is `False` only on a parse fault whose `claim`/`verb` are placeholders (bare unknown root token). |
| `truncated` | `bool` | `True` when `report.results` ≥ 1000 or `report.artifacts` ≥ 100; the full output stays in the run dir. |
| `notes` | `tuple[str, ...]` | Operator notes (owners, closure plan, applied-changes, argv). |

`report.counts` = `Counts(ok, failed, total)`, derived in the fold only. `report.detail` is a `msgspec` tagged union keyed by `kind` (`verify`/`test`/`package`/`api`/`resolution`/`diagnostic`) with `forbid_unknown_fields`. `report.artifacts` = `tuple[Artifact{id, kind, path, bytes, lines}]`. `report.results` = `tuple[Match{id, kind, text, line, score, severity, confidence}]`. `Envelope.__cyclopts_returncode__` returns `exit_code` — the single exit-code source.

## [3][RAILSTATUS_ALGEBRA]

`RailStatus` is the only status type. Fold = `reduce(join, statuses, EMPTY)`, `join` = max-by-severity. `faulted`/`busy`/`timeout` ride the `Result` Error channel and never enter the success fold.

| [STATUS] | [EXIT] | [SEV] | [CHANNEL] | [MEANING] |
| -------- | :----: | :---: | --------- | --------- |
| `skip` | 0 | 0 | Completed | Per-check opt-out (alias `"skipped"`). |
| `empty` | 0 | 1 | Completed | Ran clean / nothing relevant changed; fold seed. |
| `ok` | 0 | 2 | Completed | Evidence affirmed — only a parser/rail sets this explicitly. |
| `unsupported` | 3 | 3 | Completed | Valid precondition, no applicable path (e.g. `bridge verify` matched no scenario; an `api` key miss → richer `ApiResolution` candidates). |
| `busy` | 5 | 4 | Fault | Exclusive resource held — retry elsewhere, never wait. |
| `timeout` | 5 | 5 | Fault | Deadline exceeded (`anyio.fail_after` / rc 124). |
| `failed` | 1 | 6 | Completed | A check ran and found defects. |
| `faulted` | 2 | 7 | Fault | Operational failure — assay could not run the check. |

`from_returncode`: `0→empty`, `5→busy`, `124→timeout`, else `failed`. `--strict` on `api`/`docs` promotes `empty`/`skip` to a `faulted` Fault — a flag, not a status member. On a Fault, `error_context.failing_step` names the stage structurally from `(status, argv, message-prefix)`: `timeout` / `lease_busy` / `strict` / `validation` / `spawn`, plus `parse` for a CLI parse fault — an unknown command/option folded through `parse_fault` at the `meta` boundary, OR a surplus positional rejected by the verb's `BaseParams.bound` arity contract at `rail.run` (a no-positional verb like `api doctor`/`package list` takes zero; `api query`/`show` cap at 1, `resolve` at 2; the variadic `static`/`code`/`test`/`docs` path rail takes any). Both raise sites fold ONE `Diagnostic` shape via `_distill`: `recent_events=[dispatch=<claim|none>, <full command line>]` (`recent_events[1]` is the whole reconstructed `<claim> <verb> <tokens>` line at BOTH sites — a surplus reads the same shape as an unknown verb), `elapsed_ms`, a `parse: … after …ms` hint whose trailing `after …ms` framing survives any clip, and `truncated=true` whenever a byte was dropped. A `failing_step="parse"` envelope's `claim`/`verb` is a genuine dispatch fact only when a rail resolved; read the TYPED `error_context.dispatched` boolean (`False` on a bare unknown root token, `True` on a surplus or an unknown verb/option under a resolved sub-app) — never substring-scrape `recent_events[0]`.

## [4][CLAIM_VERB_MAP]

`Bind(claim, verb, handler, params, help)`; `build_app` groups by `claim` into nested Cyclopts apps. Polyglot `static`/`code`/`test`/`docs` handlers `select(claim, language)` rows and fold; C#-only `bridge`/`package`/`api` emit a bespoke `detail`. Verb names encode cost/mutability — `fix`/`rewrite --apply` mutate, `report`/`search`/`query` never do, `full` is the expensive parity rail.

| [CLAIM] | [VERB] | [HELP] | [DETAIL] |
| ------- | ------ | ------ | -------- |
| `static` | `fix`, `report`, `build`, `full`, `plan` | Format/style/analyzer (fix mutates), non-mutating diagnostics, closure-leased build, `.slnx` parity, routing plan. | none |
| `code` | `search`, `rewrite`, `query` | Structural pattern search (ast-grep); rewrite preview, `--apply` writes under lease; AST query (tree-sitter, in-process). | none (reuses `Match`/`Artifact`) |
| `test` | `run`, `list`, `coverage` | Unit + coverage + mutation fold; bounded discovery JSON; coverlet json + cobertura. | `TestRun` |
| `bridge` | `verify`, `doctor`, `launch`, `quit`, `check`, `clean`, `build` | Live RhinoWIP scenario fold + lifecycle; `build` is the lease-free compile. | `VerifySummary` |
| `package` | `stage`, `deploy`, `publish`, `list`, `plan` | Yak stage (leased), install, push, manifests, stage plan. | `PackageRun` |
| `api` | `doctor`, `resolve`, `query`, `show` | Host/NuGet/tool health (`--strict`→faulted), asset resolution, polymorphic ilspy surface (fingerprint cache), artifact preview. | `ApiSurface` (+ `ApiResolution` on a fuzzy miss) |
| `docs` | `check` | Markdown + Mermaid render-as-validation. | none |
| (root) | `self-test` | Composition + catalog census; failure → faulted (exit 2). | none |

Polyglot reach: `static`/`test`/`docs` span C#/Python/TypeScript/Bash/SQL; `code` is grammar-backed (Python + TypeScript today, data-driven off the catalog); `bridge`/`package`/`api` are C#-only bespoke folds. The thin-rail families share one `thin_rail(settings, scope, params, *, claim, verb, mode)`; per-verb behavior is `Mode` + `Params`, not separate functions. Per-verb `Params` inherit `BaseParams{paths, language}` (flattened onto the CLI via an inherited `@Parameter(name="*")`); `api`/`docs` add `strict`.

## [5][AXES_AND_SHAPES]

Variance rides five behavior-carrying `StrEnum` axes (payload via `__new__`), never per-program code:

| Axis | Enum | Payload | Members |
| ---- | ---- | ------- | ------- |
| Launch | `Runner` | argv `prefix` tuple | `DIRECT()`, `MODULE(uv run python -m)`, `UV(uv run)`, `DOTNET(dotnet)`, `PNPM(pnpm exec)`, `INPROC()` |
| Input | `Input` | `flag` tuple + `scoped` bool | `FILES`, `INCLUDE`, `PROJECT`, `SOLUTION`, `NONE` |
| Language | `Language` | `strategy` (`Literal["closure","glob"]`) + `suffixes` | `CSHARP`, `PYTHON`, `TYPESCRIPT`, `BASH`, `SQL`, `DOCS` |
| Operation | `Mode` | `stream` + `writes` bools | `CHECK`, `WRITE`, `RESTORE`, `BUILD`, `RUN`, `LIST`, `MUTATION`, `CLIENT`, `VERIFY`, `QUERY`, `STAGE`, `DEPLOY`, `PUBLISH` |
| Proof | `Claim` | value only | `STATIC`, `CODE`, `TEST`, `BRIDGE`, `PACKAGE`, `API`, `DOCS` |

The load-bearing reuse: one `StrEnum` instance is simultaneously the Cyclopts token, the `msgspec` wire value, and the `match` key. Fixed shapes: `Tool`, `Check` (a `Tool` bound to routed scalars — `scope`/`routed`/`deadline` are `run_check` parameters, never fields), `Report`, `Artifact`, `Match`, `Completed`, `Fault`, `Envelope`, `Counts`, `Bind`, `Routed`, the `Detail` tagged base. Algorithm evidence lives only in `report.detail`. A tool that walks the tree itself (`ast-grep run`, `biome ci`) rides `Input.NONE` and splices its target paths into `tool.command` — there is no glob-pattern projection (ast-grep rejects `**/*.py` as a path).

**Type-system stack (no overlap):** `msgspec` owns wire/evidence structs; `pydantic-settings` owns `AssaySettings` scalars; frozen `@dataclass` owns per-verb `Params`; `ty` + `beartype` own static proof + the two runtime seam boundaries. `match` + `assert_never` is the exhaustive-dispatch form; PEP 695 generics throughout; the sole `Protocol` is `Source` (the git/fd/fsspec change-set provider).

## [6][ASPECT_WEAVE]

Cross-cutting behavior attaches only as slot-ordered decorators at **two seams** — never inline. `compose` sorts by `Slot` (`IntEnum`, `checked=0` outermost), raises `TypeError` on inversion at decoration time, and is idempotent via an `id(dec)` guard.

| Slot | Factory | Library | Effect |
| :--: | ------- | ------- | ------ |
| 0 `checked` | `checked(conf=)` | `beartype` (O1) | Shape boundary; `@wraps`-preserved. |
| 1 `logged` | `logged(event, keys)` | `structlog` | `bound_contextvars` + terminal level by status; stderr only. |
| 2 `traced` | `traced(span, attrs)` | `opentelemetry` | One span per call; status from `Result`; endpoint-gated. |
| 3 `retried` | `retried(on, attempts, timeout)` | `stamina` | Engine `Spawn` only; transient spawn faults; never `BUSY`/`TIMEOUT`. |

The rail seam (`registry.rail`) weaves `checked ▷ logged ▷ traced` (a rail is a `Hom`); the engine seam (`engine._guarded`) weaves `checked ▷ traced ▷ retried` (a per-`Check` `Spawn`, retry on spawn only). `structlog.configure` runs once in `__init__.py` bound to `sys.stderr`; the OTel provider installs once, endpoint-gated; beartype attaches at seams only unless `ASSAY_CLAW` is set. Every Fault site auto-records `span.record_exception` + a `fault.resource_snapshot` (psutil oneshot) and distills a `Diagnostic` into `error_context` from an invocation-scoped ring — the Envelope self-diagnoses on failure, no separate query.

> **Runtime invariant (load-bearing):** any type in a `@checked` signature must be imported **unconditionally** — a type stranded under `if TYPE_CHECKING:` silently disarms beartype's first-call forward-ref resolution and crashes every invocation while passing all static gates. Verify a new rail/arm with a real runtime invocation, not just the gate.

## [7][CONCURRENCY_LEASE_ISOLATION]

Isolation by path; exclusivity by liveness-checked fail-fast lease; fan-out bounded by `CapacityLimiter(max_checks)`. Each run opens `.artifacts/assay/<claim>/<run_id>/` with an isolated `DOTNET_CLI_HOME`; closure builds use a **stable** `.artifacts/assay/build/<closure>/` (closure = `sha256(sorted-projects)[:16]`) so the warm MSBuild/analyzer tree survives across runs. `run_check` calls `anyio.run` exactly once — `fan_out` never nests loops.

| [RESOURCE] | [LOCK PATH] | [PARALLELISM] |
| ---------- | ----------- | ------------- |
| MSBuild closure | `locks/build-<closure>.lock` | One winner per closure; distinct closures concurrent → `busy` (exit 5), never hang. |
| Stryker mutation | `locks/mutation.lock` | Global exclusive. |
| Live Rhino + verify | `locks/bridge.lock` | Global exclusive (one live-Rhino lane). |
| Yak stage commit | `locks/package-stage.lock` (per dir) | Per package dir. |
| `code rewrite --apply` | `locks/code.lock` | Exclusive while writing in place. |
| Read-only tools | none | `fan_out` + distinct `run_id`. |

Leases: `fcntl.flock(LOCK_EX\|LOCK_NB)`, owner block `{resource, run_id, pid, create_time, cwd, started_at, target}`, truncate-on-release, **never block**. A dead or recycled holder (`psutil` `is_running()` fails or `create_time` drift > tolerance) is stolen, not stranded. `@retried` never retries `busy`/`timeout`. `run_check`/`fan_out` take an optional absolute `deadline`; `fan_out` returns partial results via `move_on_after` (unfinished slot → `Fault(TIMEOUT)`).

**Environment knobs (pydantic-settings, `ASSAY_*`, read once):** `ASSAY_ROOT` (root; anchors to `Workspace.slnx`, falls back to `cwd` — never crashes on a missing solution); `ASSAY_EXEC_TARGET` (`""`=local, `ssh://[user@]host[:port]`=remote via asyncssh — every rail inherits remote-exec through the single `_run_process_backend` seam); `ASSAY_EXEC_KNOWN_HOSTS`; `ASSAY_RUN_ID`; `ASSAY_AGENT_TASK_ID` (fleet correlation onto every log/span/Envelope). `universal-pathlib` `UPath` is the path type throughout — point-and-go I/O on any fsspec backend; only process execution stays local-or-ssh.

## [8][AUTOMATION_ARM]

A first-class arm (not a `Claim`) sharing the Engine, leases, settings, and `_emit`. `drive(trigger, action, settings)` hosts watch + schedule under one `anyio` task group sharing one stop `Event`, emitting one Envelope per fire (NDJSON — the documented exception to the one-Envelope invariant).

| Concept | Shape | Backend |
| ------- | ----- | ------- |
| `Trigger` | `Watch(paths, filter, debounce, cpu_threshold, ignore_patterns) \| Schedule(cron, cpu_threshold) \| Manual` | `watchfiles.awatch` / `aiocron.crontab` / immediate |
| `Action` | `Rail(claim, verb, params: msgspec.Raw) \| Program(argv) \| Sequence(actions) \| Debounce(action, window_ms, collapse)` | Engine + registry |

A leased `Action` is gated by a per-drive `CapacityLimiter(1)` so a slow fire never re-enters into `busy`; `cpu_threshold` emits `Completed{SKIP}` when `psutil.cpu_percent ≥ threshold`. `Sequence` folds by `RailStatus.join`: a `FAILED` leaf halts (exit 1), any Fault leaf dominates, `SKIP`/`EMPTY`/`OK` continue.

## [9][AGENT_ROUTING] — which gate when

Route by **proof claim**, not habit. One claim per concern: lint never type-checks, type-check never tests, test never opens a runtime host.

| [USE] | [WHEN] |
| ----- | ------ |
| `static fix` | After editing source you want auto-formatted/auto-fixed (mutates). |
| `static report` | Non-mutating lint/format/type diagnostics on the change-set. |
| `static build` | Compile/analyzer proof of the changed closure. |
| `static full` | `.slnx` parity (Debug+Release) — only on trigger-file changes. |
| `code search` / `query` | Find a structural pattern (ast-grep) or AST shape (tree-sitter) across the tree — read-only. |
| `code rewrite [--apply]` | Preview, then apply, a structural codemod under the `code` lease. |
| `test run` / `coverage` | Unit laws / coverage; `--mutation` is opt-in. |
| `bridge verify` | Live-Rhino runtime proof of a `.verify.csx` scenario. |
| `api query\|resolve\|show` | Resolve host/package API + source truth before relying on a signature. |

| [AVOID] | [WHY] |
| ------- | ----- |
| Waiting on a `busy` lock | Exclusive resources fail fast (exit 5) — retry elsewhere, never spin; never delete a lock file. |
| Running `--mutation`/`full` implicitly | Expensive; opt in deliberately. |
| Probing `--help` to discover behavior | The claim→verb map is here; a verb self-describes on failure via `error_context`. |
| Treating `empty` as an error | `empty` (exit 0) = ran clean / nothing matched; distinct from `failed` (exit 1) and `unsupported` (exit 3). |
| Parsing stderr for results | Results are on stdout's single `Envelope`; stderr is diagnostics only. |

## [10][CODEMAP_AND_DEPS]

```
tools/assay/
├── __init__.py            # package marker: structlog→stderr config + endpoint-gated OTel + optional ASSAY_CLAW
├── __main__.py            # Cyclopts tree from REGISTRY; one Envelope; resolve_returncode
├── core/        status.py model.py engine.py routing.py aspect.py
├── composition/ settings.py catalog.py registry.py
├── rails/       static.py code.py test.py docs.py bridge.py package.py api.py
└── automation/  model.py engine.py        # Trigger/Action unions + one anyio loop
```

Load-bearing deps (`requires-python >=3.14`): `msgspec` (wire), `pydantic-settings` (config), `expression` (`Result`/`Block.fold`/`@effect.result`), `anyio` (single loop + `to_thread` for INPROC), `cyclopts` (CLI), `beartype`/`structlog`/`opentelemetry-*`/`stamina` (the four aspect slots), `psutil` (lease liveness + CPU governor), `watchfiles`+`aiocron` (automation), `fsspec`+`universal-pathlib` (artifacts/UPath), `asyncssh` (remote exec), `tree-sitter`+`tree-sitter-python`+`tree-sitter-typescript` (`code query`). Orchestrated programs are catalog rows, not deps: `dotnet`/`dotnet-stryker`/`ilspycmd`/`yak`, `ruff`/`ty`/`mypy`/`pytest`/`coverage`/`mutmut`/`ast-grep`, `tsc`/`biome`/`knip`/`sherif`/`vitest`, `shellcheck`/`shfmt`, `sqlfluff`/`squawk`, `mmdc`.

## [11][MAINTENANCE]

Gate every change from the **repo root** (a wrong cwd reports phantom errors), with a **cleared ruff cache** for an authoritative result:

```bash
uv run ruff clean
uv run ruff check tools/assay && uv run ruff format --check tools/assay
uv run ty check --python-platform all tools/assay
uv run mypy --strict --explicit-package-bases tools/assay
uv run python -c "import tools.assay.__main__" && uv run python -m tools.assay self-test
```

Density is concept count, not byte count. Adding a program = one `Tool` row; adding a language = one `Language` member + one routing arm + rows; adding a verb = one `Bind` row + a handler. No new shape for an existing concept — extend a tagged-union variant, an enum member, or a data row. Functionality is never deleted to hit a number. The pre-promotion design corpus + decision ledger live (read-only) at `.archive/assay-_TMP/` (`design/` + `ARCHITECTURE.md`).
