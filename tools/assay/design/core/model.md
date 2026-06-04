# [H1][MODEL]
>**Dictum:** *Five axis enums and one `Base` policy generate every wire shape; algorithm evidence is one tagged `detail`, never a new report struct.*

## [1][PURPOSE]

`core/model.py` (stage 2) owns the axis vocabulary and every non-config `msgspec` struct that crosses a rail. It imports `RailStatus` and the status `fold` (as `rail_fold`) from `core/status.py` (D9) and adds nothing status-shaped. It collapses `tools/quality`'s ~25 `Literal` aliases and 14 report structs (§4) into behavior-carrying `StrEnum`s plus one `Base` and one bounded `Detail` union. Per D4/D6 there is no standalone `Strategy`, `CAPTURE`, `STREAM`, or `Tool.mutates`: a single `Mode` carries `stream`+`writes`, and `Language.strategy: str` carries route discrimination. Per D22/D25 `Parser = Callable[[Completed], AnyDetail | None]` is attached by reference; no `Engine`/`Parser` `Protocol`. The `defstruct` escape hatch (§4) keeps irregular evidence as catalog-generated data. No `from __future__ import annotations` (PEP 649/749 deferred eval is native at 3.14); the class definition order (`Base` → `Detail` → variants → `AnyDetail` → cached codec) is load-bearing for forward-ref resolution (D53).

## [2][CANONICAL_SHAPES]

Axis enums each carry behavior through `__new__` so one member is simultaneously the Cyclopts token, the `msgspec` wire value, and the `match` key (§5 of `ARCHITECTURE.md`).

| [AXIS] | [ENUM] | [PAYLOAD] | [LEDGER] |
| ------ | ------ | --------- | -------- |
| Launch | `Runner` | `prefix: tuple[str,...]` (`MODULE=("uv","run","python","-m")`) | D7 |
| Input | `Input` | `flag: tuple[str,...]`, `scoped: bool` | D23 |
| Language | `Language` | `strategy: str` (`closure`/`glob`), `suffixes: frozenset[str]`; `+BASH(.sh,.bash)`, `+SQL(.sql)` | D6, D36 |
| Operation | `Mode` | `stream: bool`, `writes: bool`; 13 members; default `CHECK` | D4, D5 |
| Proof | `Claim` | value only | D9 |
| api source | `SourceKind` | value only (`ASSEMBLY`/`NUGET`/`TOOL`) — typed `ApiSurface.source_kind` | D54 |
| api shape | `SymbolShape` | value only (`INDEX`/`NAMESPACE`/`TYPE`/`MEMBER`/`SEARCH`) — typed `ApiSurface.shape` | D54 |
| — | `ArtifactKind` | value only (path-lease namespace); `Match.kind` reuses a subset | D18 |

`type Parser = Callable[[Completed], AnyDetail | None]` is the by-reference row parser alias (D22); the alias resolves after `AnyDetail` is declared.

Struct policy (D10, D13, D16, D17, D56): `Base(frozen, gc=False, omit_defaults, repr_omit_defaults)` declared once and inherited; `forbid_unknown_fields` lives on `Detail` only (external bridge/C# JSON carries extras), with `tag_field="kind"` and explicit short tags (`verify`/`test`/`package`/`api`) — **never** `str.lower(classname)`. `Tool`/`Check` add `cache_hash=True` so they are usable as set/dict keys; `Match`/`Artifact` reject `array_like` (the agent/bridge contract reads JSON by key). `Envelope` overrides `omit_defaults=False` so `schema_version` always emits. `Completed` is the success receipt `{argv,returncode,stdout,stderr,duration_ms,status,notes}` (D11); `Fault` is the error receipt `{argv,status=FAULTED,message}` with **no** `returncode`/`detail`/`stderr` (D28); `message` is `Annotated[str, Meta(max_length=1024)]` to bound the wire (D50). `ResourceBusyError(Exception)` is the single exception this module owns (D40): `exclusive_lease` (`core/engine.py`) raises it on lease contention, the rail seam maps it to `RailStatus.BUSY` (exit 5), and `aspect._transient` returns `False` for it so `@retried` never re-attempts a held lease. `Counts(ok,failed,total)` lives on `Report` only, derived in `fold` (D16). `ApiSurface`/`VerifySummary`/`TestRun`/`PackageRun`/`ApiResolution`/`Diagnostic` are the six explicit-tag `Detail` variants (D13, D17); `ApiSurface.source_kind: SourceKind`/`shape: SymbolShape` are enums, never `str`. `VerifySummary` adds `first_fault_phase: str = ""` (the lifecycle phase — `launch`/`execute`/`check`/`cleanup` — where `first_failure` stopped) and `first_fault_output: Annotated[str, Meta(max_length=256)] = ""` (the bounded earliest diagnostic of that phase, capped like `Fault.message` per D50) so an agent retriages a bridge failure off the summary without opening the report dir. `ApiResolution(tag="resolution")` is a sanctioned extension instance: ranked `candidates: tuple[tuple[str,int],...]` (name, score) plus a bounded `reason: str` (`unknown`/`ambiguous`/`partial`) for agent disambiguation — one new tag joining `AnyDetail`, never a parallel type. `Diagnostic(tag="diagnostic")` is the auto-observability extension: `failing_step: str`, a bounded ring `recent_events: tuple[str,...]`, `elapsed_ms: Annotated[float, Meta(ge=0)]`, and a bounded `hint: Annotated[str, Meta(max_length=256)]` (capped like `Fault.message` per D50) so the `Envelope` self-diagnoses on failure — an agent retriages a faulted run off the wire without re-running. It rides `Envelope.error_context: Diagnostic | None` (added by `envelope(...)`'s `error_context` param on the **Fault branch only**; `omit_defaults` keeps it OFF the success wire), preserving D28 — `Fault` stays `{argv,status,message}` with no `context` field. Detail variants carry algorithm-specific evidence only; they **never** hold `ok`/`failed`/`total` or any fold-derived field — counts live on `Report` and derive in `fold` (D16/D46), so `TestRun.killed`/`survived`/`selected` are raw mutation telemetry, not pass/fail rollups. `Match` adds two defaulted agent-prioritization fields (`severity: str | None = None`, `confidence: int = 100` bounded `[0,100]`) — no new shape, the ranked row stays one struct. `validate_detail(detail)→AnyDetail|None` is the thin pure round-trip primitive that re-encodes a parser `Detail` through the cached `_ENCODER`/`_DETAIL_DECODER` tagged-union codec so a malformed parser `Detail` fails loud (`msgspec.ValidationError`) at fold rather than silently riding the wire.

## [3][VALIDATED_SNIPPET]

The canonical core pattern: one axis enum (`__new__`-payloaded), the `Base`/`Detail` policy pair, one typed `Detail` variant (enums, never `str`), the self-contained `fold`/`_count`, and the statement-match `envelope`. The status `fold` imports as `rail_fold` to free the module-level `fold` name; `RailStatus.from_returncode` projects `0→EMPTY`/`5→BUSY`/`124→TIMEOUT`/`else FAILED` (never `OK`/`UNSUPPORTED`). `_count` is the **sole** count-derivation site (D46 — no `Counts.derive`), embedded with `fold` so no phantom helper. `match` is statement form only (D27). Class order (`Base` → `Detail` → variants → `AnyDetail` → cached codec) is forward-ref-load-bearing (D53). Full members: `Mode` is 13 (`CHECK`…`PUBLISH`), `Language` 6 (+BASH/+SQL), `Input` 6, `Runner` 5.

```python
from collections.abc import Callable
from enum import StrEnum
from typing import Annotated, Self
import msgspec
from tools.assay.core.status import fold as rail_fold, RailStatus  # D9: status.py owns RailStatus + fold

# --- [TYPES] ---------------------------------------------------------------------------

class Runner(StrEnum):  # one axis exemplar; Input/Language/Mode follow the same __new__ shape
    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    MODULE = "module", ("uv", "run", "python", "-m")
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "exec")

    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self:
        m = str.__new__(cls, value)
        m._value_, m.prefix = value, prefix
        return m

class Claim(StrEnum):
    STATIC, TEST, BRIDGE, PACKAGE, API, DOCS = "static", "test", "bridge", "package", "api", "docs"

class ArtifactKind(StrEnum):
    LOCKS, PROCESS, TEST, MUTATION, RHINO, SCOPE = "locks", "process", "test", "mutation", "rhino", "scope"

class SourceKind(StrEnum):
    ASSEMBLY, NUGET, TOOL = "assembly", "nuget", "tool"

class SymbolShape(StrEnum):
    INDEX, NAMESPACE, TYPE, MEMBER, SEARCH = "index", "namespace", "type", "member", "search"

type Parser = Callable[[Completed], AnyDetail | None]  # by-reference row parser (D22); resolves after AnyDetail

# --- [ERRORS] --------------------------------------------------------------------------

class ResourceBusyError(Exception):  # D40: exclusive_lease raises on contention → BUSY; _transient → False (never retried)
    ...

# --- [MODELS] --------------------------------------------------------------------------

class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True): ...
class Detail(Base, frozen=True, forbid_unknown_fields=True, tag_field="kind"): ...

class Tool(Base, frozen=True, cache_hash=True):  # cache_hash → usable as a dict/set key
    name: str
    runner: Runner
    command: tuple[str, ...]
    input: Input
    language: Language
    claim: Claim
    mode: Mode = Mode.CHECK
    timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None
    parser: Parser | None = None

class Completed(Base, frozen=True):
    argv: tuple[str, ...]
    returncode: int
    stdout: bytes = b""
    stderr: bytes = b""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    status: RailStatus = RailStatus.EMPTY
    notes: tuple[str, ...] = ()

class Fault(Base, frozen=True):  # D28: no returncode/detail/stderr; D50: message length-bounded
    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAULTED
    message: Annotated[str, msgspec.Meta(max_length=1024)] = ""

class Counts(Base, frozen=True):
    ok: int = 0
    failed: int = 0
    total: int = 0

class ApiSurface(Detail, frozen=True, tag="api"):  # source_kind/shape are enums (D54), never str
    source_kind: SourceKind = SourceKind.TOOL
    source_id: str = ""
    version: str = ""
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""

# VerifySummary(tag="verify") / TestRun(tag="test") / PackageRun(tag="package") follow the same Detail shape.
# VerifySummary adds first_fault_phase: str = "" and first_fault_output: Annotated[str, Meta(max_length=256)] = ""
# — the failing phase + bounded earliest-diagnostic snippet of first_failure, so an agent retriages off the summary.

class ApiResolution(Detail, frozen=True, tag="resolution"):  # sanctioned extension: one new tag joins AnyDetail
    candidates: tuple[tuple[str, int], ...] = ()             # (name, score) ranked disambiguation
    reason: str = ""                                         # unknown | ambiguous | partial

class Diagnostic(Detail, frozen=True, tag="diagnostic"):    # auto-observability: rides Envelope.error_context on the Fault branch
    failing_step: str = ""
    recent_events: tuple[str, ...] = ()
    elapsed_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    hint: Annotated[str, msgspec.Meta(max_length=256)] = ""  # bounded remediation hint (D50)

type AnyDetail = ApiSurface | VerifySummary | TestRun | PackageRun | ApiResolution | Diagnostic

class Report(Base, frozen=True):
    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    artifacts: tuple[Artifact, ...] = ()
    results: tuple[Match, ...] = ()
    notes: tuple[str, ...] = ()
    detail: AnyDetail | None = None

class Envelope(Base, frozen=True, omit_defaults=False, kw_only=True):
    schema_version: int = 1
    claim: Claim
    verb: str
    command_path: tuple[str, ...] = ()
    status: RailStatus = RailStatus.OK
    exit_code: int = 0
    run_id: str = ""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    report: Report | None = None
    error: Fault | None = None
    error_context: Diagnostic | None = None  # auto-observability: set on the Fault branch only; omit_defaults keeps it off the success wire
    truncated: bool = False
    notes: tuple[str, ...] = ()

    def __cyclopts_returncode__(self) -> int:  # D30: Cyclopts reads exit from the value
        return self.exit_code

# --- [OPERATIONS] ----------------------------------------------------------------------

def receipt(argv: tuple[str, ...], rc: int, *, stdout: bytes = b"", stderr: bytes = b"",
            duration_ms: float = 0.0, status: RailStatus | None = None,
            notes: tuple[str, ...] = ()) -> Completed:
    return Completed(argv, rc, stdout, stderr, duration_ms,
                     status or RailStatus.from_returncode(rc), notes)

def detail_type(tool: Tool, fields: tuple[tuple[str, type, object], ...]) -> type[Detail]:
    generated = msgspec.defstruct(f"{tool.name}_detail", fields, bases=(Detail,), tag=tool.name)
    assert issubclass(generated, Detail)  # narrows the lib-stubbed type[Struct] to type[Detail]
    return generated

def validate_detail(detail: AnyDetail | None) -> AnyDetail | None:  # round-trip through cached codec; malformity fails loud at fold
    return _DETAIL_DECODER.decode(_ENCODER.encode(detail))

def _count(done: Completed) -> tuple[int, int]:  # the sole count-derivation site (D46)
    match done.status:
        case RailStatus.OK | RailStatus.EMPTY | RailStatus.SKIP:
            return 1, 0
        case RailStatus.FAILED:
            return 0, 1
        case _:
            return 0, 0

def fold(claim: Claim, verb: str, outcomes: tuple[Completed, ...], *,
         detail: AnyDetail | None = None) -> Report:
    pairs = tuple(map(_count, outcomes))
    ok_n, fail_n = (sum(a) for a in zip(*pairs, strict=False)) if pairs else (0, 0)
    return Report(claim, verb, rail_fold(*(o.status for o in outcomes)),
                  Counts(ok_n, fail_n, ok_n + fail_n),
                  notes=tuple(n for o in outcomes for n in o.notes), detail=detail)

def envelope(payload: Report | Fault, *, claim: Claim, verb: str,
             error_context: Diagnostic | None = None) -> Envelope:
    match payload:
        case Report() as r:
            return Envelope(claim=claim, verb=verb, status=r.status,
                            exit_code=r.status.exit_code, report=r)
        case Fault() as f:
            return Envelope(claim=claim, verb=verb, status=f.status,
                            exit_code=f.status.exit_code, error=f, error_context=error_context)
```

## [4][SEAMS]

| [NEIGHBOR] | [CONSUMES] | [CONTRACT] |
| ---------- | ---------- | ---------- |
| `core/status.py` | imports `RailStatus`, `fold as rail_fold` | sole status type; `Envelope.exit_code == status.exit_code` always (D29); module `fold` calls `rail_fold` to join statuses |
| `core/engine.py` | `Tool`/`Check`, `receipt`, `Fault`, `ResourceBusyError` | `run_check(check,*,settings,scope,routed,deadline=None)→Result[Completed,Fault]`; process non-zero → `receipt`, spawn/lease/timeout → `Fault` (D12). The engine seam (`checked ▷ traced ▷ retried`, **no** `logged`, D3) is where `traced` binds `run_id` + `tool.name` for retry/span correlation — never `logged` |
| `core/routing.py` | `Language.strategy`/`suffixes`, `Input` | sole language-specific code; `place(routed,tool,*,settings)→tuple[tuple[str,...],...]` projects the argv tail via a six-arm `Input` match + `assert_never` (D23) |
| `composition/catalog.py` | `Tool` rows, `Parser`, `detail_type` | **39** data rows in `TOOLS`; parsers referenced not inlined (`parse_findings`/`parse_build`/`parse_tests`/`parse_verify`/`parse_surface`/`parse_shellcheck`); irregular evidence via `defstruct` |
| `composition/registry.py` | `Bind`, `fold`, `envelope`, `receipt`, `_ENCODER` | `rail(bind)` weaves `checked ▷ logged ▷ traced` (`_RAIL_LAYERS`, **no** `retried`, D2) once over `_narrow(bind.handler)`; `_emit` is the sole stdout writer (`_ENCODER.encode(envelope)`); `Envelope.__cyclopts_returncode__` feeds `resolve_returncode` (D30) |
| `automation/engine.py` | `Check`, `Fault`, `Report`, `RailStatus` | `_watch` (native `awatch` stop) + `_schedule` (`aiocron.crontab(start=False)` waiting on the shared stop) host one `Action` per fire under one task group sharing **one** `anyio.Event` stop (D37) |
| rails (`static`/`test`/`bridge`/`package`/`api`/`docs`) | `Report`, `Counts`, `BaseParams`, six `Detail` variants, `validate_detail` | one `Report` crosses every rail; algorithm evidence rides `detail` only (`ApiResolution` carries api disambiguation candidates; `Diagnostic` rides `Envelope.error_context` on the Fault branch); `validate_detail` fails a malformed parser `Detail` loud at fold; `BaseParams` (D58) is the shared params leaf imported here |

## [5][EXTENSIBILITY]

A sixth language is one `Language` member + one routing arm + catalog rows; a new program is one `Tool` row; a new evidence shape is one tagged `Detail` variant or one `detail_type` call — no rail signature changes, no new module (D31, invariant 4).

## [6][CONSIDERATIONS]

- `Match.kind` is typed `ArtifactKind` (D18), so a ranked row and a produced file share one bounded namespace; a future match-only kind expands `ArtifactKind`, never a free string.
- `_DECODER = Decoder(Report)` resolves the `AnyDetail` union by `tag_field="kind"` in one pass; the alias must be declared **after** all six variants (no forward refs per msgspec gotcha), and `tag_field` must collide with no field name — none do.
- `Fault` riding `FAULTED` by default (D28/D29) means the `--strict` promotion (registry `_strict`, applied to api/docs/static-family per D43, not just api/docs) constructs a `Fault` directly rather than mutating a `Completed`; it is a flag-driven Fault construction at the rail boundary, never a new `RailStatus` member.
- `TestRun.selected` is an `int` count of selected tests, **not** a `(killed, survived)` or any tuple (D41); `killed`/`survived`/`selected` are flat mutation-telemetry integers that `test.md` mirrors verbatim, so a downstream consumer reads three scalars, never a packed pair.
- `Bind.params` is the per-verb frozen `@dataclass` (D19) Cyclopts consumes via `Annotated`: `Parameter(name="*")` **field-flattens** the dataclass fields into flat CLI tokens, while `Parameter(parse=False)` injects a **non-CLI** dependency (e.g. `settings`/`scope`) as-is with no token parsing (keyword-only params only). The two compose — `Annotated[T, Parameter(name="*", parse=False)]` flattens a struct *and* withholds it from the CLI surface — which is how a handler receives flattened verb params alongside injected runtime context in one signature.
