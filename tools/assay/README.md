# [H1][ASSAY_OPERATOR]
>**Dictum:** *One claim owns one proof; one Envelope owns stdout; one exit code owns the verdict.*

<br>

[IMPORTANT] `tools.assay` is an **agent-first** CLI. Humans may run it, but the contract targets automated agents parsing JSON. Read `claim`, `verb`, `status`, `exit_code`, `run_id`, and `report.artifacts[].path` from the Envelope — never scrape stderr for outcomes. It supersedes `tools/quality` (D31, §12) and the `package.json` quality scripts (`check:py*`, `check:ts`, `test:cs`, `verify:rhino`), routing C#, Python, TypeScript, Bash, and SQL through one Engine.

```bash
uv run python -m tools.assay <claim> <verb> [args]      # one Envelope on stdout
uv run python -m tools.assay watch <paths> --on <action> # NDJSON stream (automation arm)
```

Use the Python module entrypoint only; do not add package-manager aliases. Rails stay orthogonal (`static` never runs tests, `test` never opens Rhino, `bridge` never replaces compile proof, `api` never launches Rhino). Polyglot `static`/`test`/`docs` fold catalog rows across five languages; C#-only `bridge`/`package`/`api` own live or bespoke evidence (D31, §13).

---
## [1][PURPOSE]
>**Dictum:** *Replace shape sprawl with one Engine and one wire.*

<br>

`assay` collapses the predecessor's three model systems, ~25 free `Literal` aliases, and per-binary argument builders into one polymorphic surface: programs are `Tool` rows in `composition/catalog.py` (not modules), routed by `Language`, run through `core/engine.run_check`/`fan_out`, folded into one `Report`, emitted as one `Envelope`. The design optimizes **dozens of concurrent agents**: disjoint `run_id` artifact trees, bounded read-only polyglot fan-out, `psutil`-verified fail-fast leases instead of blocking `flock` (D34, §7). Adopted deps `psutil`/`watchfiles`/`aiocron`/`fsspec` are **declared, not gated** (§10, D39); `automation/` is a first-class arm, not a claim (D37, §9). Agent docs: `AGENTS.md` (edit routing), `ARCHITECTURE.md` (shapes, ledger, parity); the §11 **DECISION LEDGER** (D1–D39) is binding — cite it, never re-litigate.

---
## [2][CANONICAL_SHAPES]
>**Dictum:** *The wire is the Envelope; everything else nests under `report`.*

<br>

[CRITICAL] **Stdout contract:** every quality verb writes exactly **one** JSON `Envelope` line to stdout. `_emit` is the sole stdout writer (Invariant 1). Proof nests under `report` — never a parallel `data`/`evidence` ladder (D10). Process bytes, `dotnet` streams, and `structlog` diagnostics go to **stderr only**.

| [FIELD] | [TYPE] | [SEMANTICS] |
| ------- | ------ | ----------- |
| `claim` | `Claim` | `static\|test\|bridge\|package\|api\|docs` (D9 — `claim`, never `rail`). |
| `verb` | `str` | The selected verb within the claim. |
| `status` | `RailStatus` | `ok\|empty\|skip\|unsupported\|busy\|timeout\|failed\|faulted` (§8). |
| `exit_code` | `int` | `== status.exit_code` **always**, the single source (D29, Invariant 2). |
| `run_id` | `str` | `%Y-%m-%dT%H-%M-%S.%f-{pid}`; `ASSAY_RUN_ID` overrides for CI correlation. |
| `duration_ms` | `int` | Wall time of the invocation. |
| `report` | `Report \| None` | `{claim, verb, status, counts, artifacts, results, notes, detail}`; `None` on a `Fault`. |
| `error` | `Fault \| None` | `{argv, status=FAULTED, message}` — **no** `returncode`, **no** `detail`, **no** `stderr` (D28). |
| `truncated` | `bool` | `True` when `core/model.fold` caps `report.results` (>1000) or `report.artifacts` (>100); the full output stays in the rail's artifact dir and a stderr note names the path (D51). |
| `notes` | `tuple[str,...]` | Free-form operator notes (owners, closure, plan argv). |
| `schema_version` | `int` | Always emitted (`omit_defaults=False` on `Envelope` only). |

`report.detail` is a `msgspec` tagged union keyed by `kind` with explicit short tags `verify\|test\|package\|api` (D13) and `forbid_unknown_fields` (Invariant 5). `Counts(ok, failed, total)` is frozen and derived in the fold (D16). `Artifact{id, kind, path, bytes, lines}` and `Match{id, kind, text, line, score}` reuse `ArtifactKind` for `kind` (D18). Non-zero process exit folds to `Completed`(`FAILED`) on the success channel — **never** a `Fault` (D12, Invariant 3); `Fault` is reserved for spawn/timeout/lease-miss/validation/`--strict` promotion/missing host API.

---
## [3][VALIDATED_SNIPPET]
>**Dictum:** *The Envelope reports the exit code; Cyclopts reads it via the protocol method (D30).*

<br>

`RailStatus` self-describes exit and severity (§8); `Envelope.__cyclopts_returncode__` hands Cyclopts the code; `meta` collapses to `resolve_returncode(app(...))` (`resolve_returncode(None) == 0`). `match` is **statement** form only (D27); `Fault` carries `{argv, status, message}` only (D28).

```python
from enum import StrEnum

import msgspec
from cyclopts import App, resolve_returncode

app = App(name="assay")


class RailStatus(StrEnum):
    SKIP = ("skip", 0, 0); EMPTY = ("empty", 0, 1); OK = ("ok", 0, 2)
    UNSUPPORTED = ("unsupported", 3, 3); BUSY = ("busy", 5, 4); TIMEOUT = ("timeout", 5, 5)
    FAILED = ("failed", 1, 6); FAULTED = ("faulted", 2, 7)

    exit_code: int
    severity: int

    def __new__(cls, value: str, exit_code: int, severity: int) -> "RailStatus":
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        return member


def from_returncode(rc: int) -> RailStatus:
    match rc:
        case 0: return RailStatus.EMPTY
        case 5: return RailStatus.BUSY
        case 124: return RailStatus.TIMEOUT
        case _: return RailStatus.FAILED


class Fault(msgspec.Struct, frozen=True, omit_defaults=True):
    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAULTED
    message: str = ""


class Envelope(msgspec.Struct, frozen=True, omit_defaults=False):
    claim: str
    verb: str
    status: RailStatus
    exit_code: int
    run_id: str
    schema_version: int = 1
    error: Fault | None = None

    def __cyclopts_returncode__(self) -> int:
        return self.exit_code


def meta(tokens: list[str]) -> int:
    return resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))
```

---
## [4][SEAMS]
>**Dictum:** *Name the neighbors; the Engine and the wire are the only shared seams.*

<br>

| [SEAM] | [NEIGHBOR] | [CONTRACT] |
| ------ | ---------- | ---------- |
| argv → claim×verb | `composition/registry.py` | `Bind(claim, verb, handler, params, help)` rows; `build_app` is a `groupby(claim)` fold (D31, §13). |
| claim → row selection | `composition/catalog.py` | `select(claim, language)` returns `Tool` rows; `place(routed, tool, *, settings)` is the sole argv-tail projector (D23, Invariant 6). |
| language → inputs | `core/routing.py` | git/fd/fsspec `Source` provides the change set; the only language-specific code (Invariant 6). |
| Check → outcome → wire | `core/engine.py` + `_emit` | `run_check`/`fan_out` fold a `Check` into `Result[Completed, Fault]` (one `anyio.run`, no nested loops, D14/D33); `_emit` is the sole stdout writer, folding `Result` via `match` (D26) under the `checked ▷ logged ▷ traced` rail seam. |
| automation fires | `automation/{model,engine}.py` | `Trigger` (`Watch\|Schedule\|Manual`) → `Action` (`Rail\|Program\|Sequence`); shares Engine, leases, settings, `_emit` (D37, §9). |

[CRITICAL] **Exit algebra (§8, D29).** One status type owns one exit meaning; orchestrators read exit codes, never stderr.

| [STATUS] | [EXIT] | [SEV] | [CHANNEL] | [MEANING] |
| -------- | :----: | :---: | --------- | --------- |
| `skip` | 0 | 0 | Completed | Per-check opt-out. |
| `empty` | 0 | 1 | Completed | Ran clean / nothing relevant changed (fold seed). |
| `ok` | 0 | 2 | Completed | Evidence affirmed (parser set it). |
| `unsupported` | 3 | 3 | Completed | Valid precondition, no applicable path (e.g. `verify` matched no scenario). |
| `busy` | 5 | 4 | Fault | Exclusive resource held — retry elsewhere, never wait. |
| `timeout` | 5 | 5 | Fault | Deadline exceeded (`anyio.fail_after` / rc 124). |
| `failed` | 1 | 6 | Completed | A check ran and found defects (build/lint/test failure). |
| `faulted` | 2 | 7 | Fault | Operational failure — assay could not run the check (spawn/lease-miss/validation/`--strict`/missing host API). |

Fold = `reduce(join, statuses, EMPTY)`, `join` = max-by-severity (D15); `faulted`/`busy`/`timeout` ride the `Result` Error channel and never enter the success-channel fold. `--strict` (on `api`/`docs`) promotes `empty`/`skip` to a `faulted` Fault — a flag, not a status member.

### [4.1][RAIL_MAP_AND_PARITY]

Full verb parity (§12, §13). Every `tools/quality` verb and every `package.json` script step maps to one assay owner; nothing is dropped.

| [CLAIM] | [VERBS] | [LANGUAGES] | [DETAIL] | [PREDECESSOR] |
| ------- | ------- | ----------- | -------- | ------------- |
| `static` | `fix`, `report`, `build`, `full`, `plan` | C#, Python, TS, **Bash**, **SQL** | none | `quality static *`; `check:py`/`check:ts` lint+type+format rows. |
| `test` | `run`, `list`, `coverage` | C#, Python, TS | `TestRun` | `quality test *`; `test:cs`; `check:py(:coverage/:mutation/:benchmark/:fixtures)`; `vitest`. |
| `bridge` | `verify`, `doctor`, `launch`, `quit`, `check`, `clean`, `build` | C# | `VerifySummary` | `quality bridge *`; `verify:rhino`. |
| `package` | `stage`, `deploy`, `publish`, `list`, `plan` | C# | `PackageRun` | `quality bridge package *`. |
| `api` | `doctor`, `resolve`, `query`, `show` | C# | `ApiSurface` | `quality api *`. |
| `docs` | `check` | Mermaid (`mmdc`) | none | (new). |
| (root) | `self-test [--rhino]` | — | none | `quality self-test`; failure → `faulted`/exit 2 (D38). |

New Bash/SQL coverage has no quality predecessor (D36): `static report`/`fix` Bash rows (`shellcheck -f json1`, `shfmt -d`) and SQL rows (`sqlfluff lint --dialect postgres`, `squawk` migration safety). Known gap (logged, not silently dropped): TS has no mutation runner (immutable-AST architectural limit). Thin-rail verbs share one `thin_rail(settings, scope, params, *, claim, verb, mode)`; `mutation`/`benchmark`/`coverage`/`target` on `test` are `Params` fields, not verbs (D19, §13).

```bash
uv run python -m tools.assay static fix libs/csharp/Rasm        # Mode.WRITE twins (dotnet format + ruff format + biome)
uv run python -m tools.assay static build tools/assay           # closure-leased compile + analyzers
uv run python -m tools.assay static report packages/core        # TS/py/bash/sql diagnostics, no mutation
uv run python -m tools.assay test run --target tests/csharp/libs/Rasm/Rasm.Tests.csproj --mutation changed
uv run python -m tools.assay bridge verify "tests/**/Vectors/scenarios/*.verify.csx"
uv run python -m tools.assay api query LanguageExt.Core "Prelude.Some"
```

---
## [5][EXTENSIBILITY]
>**Dictum:** *A program is a row; a language is a member plus a routing arm; an automation is a tagged case.*

<br>

Adding a program is one `Tool` row in `catalog.py` (Invariant 4); adding a language is one `Language` member + one `routing.py` arm + rows; adding a trigger or action is one tagged case on the `automation` unions (D37). No rail signature changes, no new module — `<2000` LOC hard, `<1250` dream (§16). Concurrency is isolation-by-path with `psutil`-verified leases (D34, §7): `build-<closure>.lock` (one winner per `sha256(sorted-projects)[:16]`, distinct closures concurrent), global-exclusive `mutation.lock`/`bridge.lock`, per-dir `package-stage.lock`, lock-free read-only `fan_out` under `CapacityLimiter(max_checks)`. A dead/recycled holder (`not is_running()` or `create_time` mismatch >1s) is **stale** and stolen, never stranded. Artifacts: `.artifacts/assay/<claim>/<run_id>/` (scope + isolated `DOTNET_CLI_HOME`), `.artifacts/assay/build/<closure>/` (warm MSBuild, D32), fingerprinted API surface cache. The `automation/` arm is the documented exception to the one-Envelope invariant: it streams **one valid Envelope per fire** as NDJSON via the same `_emit`; `@retried` never retries `busy` inside a fire (D37, §9, Invariant 7).

---
## [6][CONSIDERATIONS]

- **Busy is a capacity signal, not a failure.** Orchestrators receiving `status == busy` (exit 5) must reroute — another closure, another claim, another agent slot — never poll or sleep. Hundreds of agents on read-only `static report`/`api query`/`docs check` scale with disk and bounded fan-out; hundreds on the **same** `static build` closure return `busy`, so shard by project or queue closures. The lease holder text records `(pid, create_time)`, so `psutil` distinguishes a live holder from a crashed one and steals the latter (D34).
- **The exit code is the only machine contract; the Envelope is the only payload contract.** Because `exit_code == status.exit_code` always (D29) and `__cyclopts_returncode__` returns it (D30), a wrapper can branch purely on exit code without decoding JSON. A `faulted` (exit 2) means assay could not run the check — distinct from `failed` (exit 1, the check ran and found defects); never conflate the two when gating CI, since a `faulted` is an infrastructure fault, not a quality regression.
- **The automation arm reuses the quality rails verbatim; do not fork a watcher.** `watch <paths> --on "static build <project>"` re-drives the identical Engine + leases + `_emit`, so a fired action is byte-identical to the manual invocation, differing only in NDJSON framing. A `Schedule(cron)` action under `aiocron` and a `Watch` under `watchfiles` co-host one `anyio` task group with a shared `stop_event` — a third trigger is a tagged case, not a parallel loop.
