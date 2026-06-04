# [H1][RESEARCH_HOLISTIC_SHAPES]
>**Dictum:** *One vocabulary stack, one shape stack, one config stack; every struct/enum earns a distinct role or it collapses. Suspected merges that hold (Completed≠Detail, Artifact≠Match, Fault≠Detail) are kept by channel/role proof; the real collapses are a conflated `Mode`, a stray `Strategy`, dict bags, and a `claim`/`rail` name fork.*

Cross-checks every `.design/*.md` against `ARCHITECTURE.md`/`IMPLEMENTATION.md` and the live lint manifest (`pyproject.toml`: `TID251` bans `typing.NamedTuple`, `os.environ`, `json.*`, `returns`; `runtime-evaluated-base-classes` already lists `msgspec.Struct`/`pydantic_settings.BaseSettings`). Verified facts reused: msgspec encodes `StrEnum` by `_value_` and decodes value+`_add_value_alias_` to the singleton; tagged union (`tag_field="kind"`, `tag=str.lower`, `forbid_unknown_fields`) decodes in one pass; `Base` config inherits to subclasses; PEP 649/749 on 3.14 lets unquoted self-referential unions resolve at first `Encoder` build; expression `Result.map/bind`; Cyclopts derives an enum choice-set from members; pydantic-settings emits scalars/`Path` only.

---
## [1][UNIFIED_SHAPE_CATALOG]

> **[SUPERSEDED BY TYPE_SYSTEM.md]** Wave 4 canonical catalog: unified `Mode` (no CAPTURE/STREAM split), `Claim.PACKAGE`, explicit `Detail` tags (`verify`, not `tag=str.lower`), `Scope` on `Routed`, `Counts` struct. Rows marked **COLLAPSE** / **MERGE** below reflect pre–Wave 1b analysis — verify against `TYPE_SYSTEM.md` §4 and `snippets/model-status.py.md` before coding.

>**Dictum:** *The final minimal set: 9 enums, 9 structs + 1 detail base + 4 variants, 1 config. Each row proves a distinct field-set or role, or carries a collapse verdict.*

**Vocabularies (`StrEnum`/`IntEnum`).** A member is the CLI token, the wire value, and the match key at once — that triple is the existence proof.

| [ENUM] | [AXIS / DISTINCT PAYLOAD] | [HOME] | [VERDICT] |
| --- | --- | --- | --- |
| `RailStatus` | status: `value`+`exit_code`+severity+alias; sole exit-code source | `status.py` | KEEP — only status type |
| `Runner` | launch: argv-prefix tuple via `__new__` | `model.py` | KEEP |
| `Input` | placement: `flag` tuple + `scoped` bool; drives `place()` | `model.py` | KEEP |
| `Language` | language: `suffixes` + route discriminant | `model.py` | KEEP |
| `Claim` | rail: key = subcommand = `Report.claim` wire = dispatch | `model.py` | KEEP — thin vocabulary, not a free `Literal` |
| `Mode` | operation kind (CHECK/WRITE/RESTORE/BUILD/RUN/LIST/MUTATION/CLIENT/VERIFY/STAGE/DEPLOY/PUBLISH/QUERY) | `model.py` | **COLLAPSE** — absorb the duplicate capture-`Mode` (CAPTURE/STREAM) as a `stream: bool` payload; absorb `Tool.mutates` as a `writes: bool` payload |
| `ArtifactKind` | path + lease namespace; `artifact(kind,*parts)` | `settings.py` | KEEP — one vocabulary for paths and locks |
| `Slot` | aspect compose order (IntEnum) | `aspect.py` | KEEP — internal, never wire |
| `Strategy` | route: CLOSURE vs GLOB | `routing.py` | **MERGE** — it is `Language`'s `route` payload; declare once, not as a second free enum (see §4) |

**Shapes (`msgspec.Struct`).** `Base(frozen, gc=False, omit_defaults)` is the shared-kwargs root; only `Envelope` overrides (`omit_defaults=False`, `kw_only`), only `Detail` adds `forbid_unknown_fields`.

| [STRUCT] | [DISTINCT FIELDS / ROLE] | [VERDICT] |
| --- | --- | --- |
| `Tool` | data row: runner/command/input/language/claim/mode/parser/timeout | KEEP |
| `Check` | `Tool` + routed subset + `scope` arg; engine-internal, never encoded | KEEP |
| `Completed` | raw receipt: argv/rc/`bytes` stdout+stderr/duration | KEEP — pre-fold, zero-copy, never serialized |
| `Fault` | wire error: argv/status/rc/message/stderr | KEEP |
| `Report` | one rail payload: claim/verb/status/counts/artifacts/results/notes/detail | KEEP |
| `Artifact` | produced file: id/kind/path/`bytes`/`lines` | KEEP |
| `Match` | ranked row: id/kind/text/`line`/`score` | KEEP |
| `Envelope` | versioned stdout wrapper | KEEP |
| `Detail`(+`ApiSurface`/`VerifySummary`/`TestRun`/`PackageRun`) | tagged typed evidence; one-pass `kind` decode | KEEP — `ApiSurface.source: dict` → typed fields (§4) |
| `Routed` | routing output: files/projects/groups/triggers; one shape all langs | KEEP |
| `Bind` | registry row: claim/verb/handler/params/help | KEEP |
| `Counts` | fold counts: ok/failed/total (invariant `total=ok+failed`) | **ADOPT** — retire `Report.counts: dict[str,int]` weak bag |
| `Params`×N | per-rail CLI args | KEEP **as frozen `@dataclass`** — `NamedTuple` is lint-banned (`TID251`) |
| `AssaySettings` | env scalars/`Path`; sole pydantic surface | KEEP |

**Suspected merges that DO NOT hold (the rigorous verdict):**

| [PAIR] | [SHARED] | [WHY DISTINCT — KEEP] |
| --- | --- | --- |
| `Completed` vs `Report.detail` | both carry program evidence | `Completed` is raw pre-fold `bytes` the engine returns and never serializes; `detail` is post-fold *typed* algorithm proof on the wire. Merging re-decodes bytes into the success payload — the exact ladder `tools/quality` retired. |
| `Artifact` vs `Match` | `id`, `kind` | `Artifact`=filesystem pointer (`path`/`bytes`/`lines`); `Match`=in-payload ranked content (`text`/`score`). A merge yields a 7-field struct ~5 fields null on each side — weak typing, the prohibition itself. (`Artifact.lines`=count, `Match.line`=position; not the same field.) |
| `Fault` vs an error `Detail` variant | both describe a failure | Opposite `Result` channels: `Fault` rides `Envelope.error` (`Error`); `Detail` rides `Report` (`Ok`). A `Detail` is success-path typed evidence; an error is never a `Report` field. |

---
## [2][HOLISTIC_STACK_ONE_WORKED_PATH]
>**Dictum:** *One `Claim` instance is the CLI token, the wire value, the match key; pydantic hands primitives to msgspec; the tagged `detail` decodes in one pass; the whole rail is one `Result[Report, Fault]`.*

```python
class Claim(StrEnum):                                   # vocabulary: one instance, three subsystems
    STATIC = "static"; TEST = "test"; BRIDGE = "bridge"; API = "api"; DOCS = "docs"

class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True): ...
class Detail(Base, forbid_unknown_fields=True, tag=str.lower, tag_field="kind"): ...
class VerifySummary(Detail): ok: int = 0; failed: int = 0; total: int = 0; report_dir: str = ""

class Report(Base):
    claim: Claim                                        # the SAME member Cyclopts yielded and msgspec encodes
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    detail: ApiSurface | VerifySummary | TestRun | PackageRun | None = None

def verify(params: BridgeParams) -> Result[Report, Fault]:        # the whole rail is one Result rail
    settings = AssaySettings()                                    # pydantic validates env -> scalars/Path only
    return (
        route(params.language, params.paths)                      # Result[Routed, Fault]
        .bind(lambda routed: select(params.claim, params.language)
              .traverse(lambda tool: bind_check(tool, routed, settings.root)))   # primitives -> Check, no shared shape
        .bind(lambda checks: fan_out(checks, settings))           # Result[tuple[...], Fault]; faults never raise
        .map(lambda outcomes: fold(params.claim, params.verb, outcomes))         # one Report
    )

def exit_code(report: Report) -> int:                             # match dispatch on the SAME member
    match report.status:
        case RailStatus.SKIP | RailStatus.EMPTY | RailStatus.OK: return 0
        case RailStatus.UNSUPPORTED | RailStatus.BUSY | RailStatus.TIMEOUT | RailStatus.FAILED: return report.status.exit_code
        case _ as never: assert_never(never)

r = Report(claim=Claim.BRIDGE, verb="verify", detail=VerifySummary(ok=3, failed=1, total=4))
raw = msgspec.json.encode(r)                            # detail -> {"kind":"verifysummary","ok":3,"failed":1,"total":4}
back = msgspec.json.decode(raw, type=Report)            # ONE pass: tag "verifysummary" selects the variant
assert back == r and isinstance(back.detail, VerifySummary)
msgspec.json.decode(b'{"claim":"bridge","verb":"v","detail":{"kind":"verifysummary","bogus":1}}', type=Report)
# -> ValidationError: unknown field `bogus`  (forbid_unknown_fields, drift fails loud)

def command(params: Annotated[BridgeParams, Parameter(name="*")]) -> int:   # Cyclopts choice-set = Claim members
    return rail(params)                                 # same enum instance flows CLI -> handler -> wire -> match
```

The single load-bearing move: `Claim`/`RailStatus`/`Language` are never re-spelled. Cyclopts yields the member, msgspec encodes its `_value_`, `match` dispatches the member, and `pydantic` contributes only `settings.root`/timeouts — primitives that *feed* the msgspec structs and are never modelled twice. No translation layer, no projector, no `JsonValue` ladder.

---
## [3][PROTOCOL_USAGE]
>**Dictum:** *A `Protocol` only when ≥2 implementers or a genuinely injected boundary exist; assay has exactly ONE.*

| [CANDIDATE] | [IMPLEMENTERS] | [REAL BOUNDARY?] | [VERDICT] |
| --- | --- | --- | --- |
| `Source` | `LOCAL` now, `FsspecSource` planned (`routing.md` §5) | yes — local git/`fd` vs remote/in-memory FS, injected into `route(...)` | **USE Protocol** — the one justified contract |
| `Engine` | one (`run_check`/`fan_out` module fns) | no — single in-process executor | **DROP** — module functions; a Protocol is ceremony (`ARCHITECTURE.md` §5 over-promises it) |
| `Parser` | 5 (`parse_findings`/`parse_build`/`parse_tests`/`parse_verify`/`parse_surface`) | no — single-method | **DROP Protocol; use `type Parser = Callable[[Completed], Detail]`** — a one-method Protocol *is* a `Callable` |

Definitive rule for assay: **Source is the only Protocol.** Engine is a module; Parser is a `Callable` alias carried by reference on the `Tool` row. This removes two structural surfaces that `ARCHITECTURE.md` §5 and `IMPLEMENTATION.md` §4 currently name.

---
## [4][CONSTANT_AND_ALIAS_TRIPWIRES]
>**Dictum:** *Every spam risk has exactly one canonical home; drift between notes is the early symptom.*

| [RISK] | [DRIFT FOUND] | [CANONICAL HOME] |
| --- | --- | --- |
| Status strings | only `SKIP→"skipped"` legitimate (real bridge emitter) | `RailStatus` member + alias in `status.py`; no preemptive aliases |
| Path constants | `_QUALITY`/`_ARTIFACTS`/`_DOTNET_CLI`/`_MARKER` | `AssaySettings.artifact(kind,*parts)` + `ArtifactKind`; leaf tokens are call args |
| Lease names | `mutation.lock`/`bridge.lock`/`build-<c>.lock` | `artifact(ArtifactKind.LOCKS, name)` — locks are just a `kind` |
| Runner prefixes | **value fork**: `MODULE=("python","-m")` (`model.md`) vs `("uv","run","python","-m")` (`catalog.md`) | one `Runner.__new__` payload in `model.py`; pick the `uv run python -m` form |
| Route discriminant | **value fork**: `route: Literal["graph","glob"]` (`model.md`) vs `Strategy{CLOSURE,GLOB}` "closure" (`routing.md`) | one `Language.route` discriminant; "closure"/"glob"; delete the second declaration |
| Rail/claim field | **name fork**: `Report.claim`/`Envelope.claim` (`model.md`) vs `rail=` (`rails.md`,`registry.md`,`main.md`) | `claim` everywhere — matches the enum; rename the runner/fold sites |
| Counts | `Counts(ok,failed,total)` (`rails.md`) vs `counts: dict[str,int]` (`model.md`) | frozen `Counts` struct; retire the dict (weak-type ban) |
| Detail dict bags | `ApiSurface.source: dict[str,str]` | typed fields (`origin`/`version`/`assembly`/`xml`) on the variant |
| Verb/CLI params | `NamedTuple` (`main.md`) — **lint-banned `TID251`** | frozen `@dataclass` Params (Cyclopts-introspectable, runtime-validated) |
| Env reads | `os.environ.get(OTEL_*/ASSAY_CLAW)` (`aot-otel.md`,`aot-beartype.md`) — **banned** | `AssaySettings` typed mirror, or one marked boundary exemption at `__init__.py` import seam only |
| `PackageStepKind` | quit/install/refresh/push (`rails.md`) | demote to `PackageRun` ordered data unless a member carries behavior (lease re-entry); not a standing top-level enum |

---
## [5][FUTURE_SHAPES_NO_NEW_PARADIGM]
>**Dictum:** *Each future capability attaches as a field, a bind key, or a projector — never a new model system.*

| [FUTURE] | [ATTACHES AS] | [WHY NO NEW PARADIGM] |
| --- | --- | --- |
| `watchfiles` events | one `Bind(Claim.WATCH,...)` row + `WatchParams`; handler re-invokes the target rail, emits one `Report`/cycle as a JSONL `Envelope` stream | reuses `Report`/`Envelope`; relaxes Invariant 1 to "one Envelope per rail cycle" (open decision, lean (a)) |
| `psutil` metrics | optional `resources: Resources \| None = None` on `Completed` (small frozen sub-struct: `rss`/`cpu_ms`) | `Completed` is the engine receipt; no signature change, no wire shape change (it stays engine-internal) |
| otel attributes | `Callable[P, Attrs]` projector (`_rail_attrs`/`_check_attrs`) returning a `Mapping[str, AttrValue]` from existing fields | attrs are projected from `Tool`/`Check`/`Report`; no struct; one `@traced` Layer per seam |
| RED metrics | one new `Layer` at a `Slot`, added to a `compose(...)` seam | aspect algebra already projects from the same `match res` outcome |

---
## [6][PRIORITIZED_COLLAPSE_LIST]
>**Dictum:** *Apply highest surface reduction first; each is a delete, not an addition.*

| [#] | [COLLAPSE] | [REMOVES] | [TOUCHES] |
| --- | --- | --- | --- |
| 1 | Unify `Mode`: fold capture-`Mode`(CAPTURE/STREAM) + `Tool.mutates` into ONE operation `Mode` with `stream`/`writes` `__new__` payloads | one enum, one struct field, the `CAPTURE`-vs-`CHECK` default disagreement | `model.py`, `catalog.py`, `engine.py` |
| 2 | Delete `Engine` and `Parser` Protocols; Engine=module fns, `type Parser=Callable[[Completed],Detail]` | two structural surfaces | `model.py`, `engine.py`, `ARCHITECTURE.md` §5, `IMPLEMENTATION.md` §4 |
| 3 | Adopt frozen `Counts`; retire `Report.counts: dict`; type `ApiSurface.source` | two dict bags (weak-type prohibition) | `model.py`, `rails/*` |
| 4 | One `Language.route` discriminant; delete standalone `Strategy`; fix "graph"→"closure" | one duplicate enum + a value fork | `model.py`, `routing.py` |
| 5 | `Params`: `NamedTuple` → frozen `@dataclass` (lint-mandated) | a guaranteed `TID251` failure | `main.py`, `registry.py`, `rails/*` |
| 6 | Canonicalize `claim` (drop `rail=` field name) | a silent dual name across 4 files | `rails/*`, `registry.py`, `main.py` |
| 7 | One canonical `Runner.MODULE` prefix tuple | a value fork between two notes | `model.py` |
| 8 | Demote `PackageStepKind` to `PackageRun` data | a candidate top-level enum | `rails/package.py`, `model.py` |

---
## [FURTHER_CONSIDERATION]
- **`Mode` as the central polymorphism carrier.** Once `Mode` carries `stream`/`writes`, it can also carry `parallel: bool` (read-only fan-out vs serialized exclusive) and `lease: ArtifactKind | None`, so `Engine.run_all`'s lease/concurrency decision reads row data instead of inspecting the `Tool` — pushing the §4 lease table (`rails.md`) into the enum payload and shrinking the engine.
- **`array_like=True` only for the never-wire `Completed`.** Since `Completed` is engine-internal and never crosses a schema boundary (§1 verdict), it is the *one* struct where positional `array_like` encoding is safe for a high-volume internal `Match` stream — the brittleness caveat (`model.md` open decision 1) does not apply precisely because it is not a wire shape.
- **Esoteric: `RailStatus` severity is a monoid the fold already needs — reuse it for `Counts`.** The join-semilattice (`SKIP<EMPTY<OK<…<FAILED`) that folds status can also classify each outcome into `Counts` in the *same* `reduce`, so `status` and `counts` are one fold pass over outcomes, not two — removing a second comprehension in every rail (`rails.md` §1 currently scans `completed` twice).
- **Esoteric: `msgspec.Raw` is the principled seam for the C# bridge, not a `defstruct` proliferation.** Bridge `facts`/`captures` arrive as opaque host JSON; holding them as `Raw` on one `VerifySummary` field defers decode until inspected and keeps a single variant, whereas a per-payload `defstruct` would quietly recreate the per-rail report sprawl the whole design retired.
