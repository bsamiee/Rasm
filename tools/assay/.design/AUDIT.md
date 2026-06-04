# [H1][ASSAY_DESIGN_AUDIT]
>**Dictum:** *Wave 0 verdict: the spine is sound; vocabulary forks and dual paradigms block implementation.*

**[WAVE_1B_APPLIED]** 2026-06-03: §3 conflict matrix and §4 collapse backlog items 1–7 applied to `model.md`, `status.md`, `settings.md`, `routing.md`, `aspect.md`, `engine.md`, `registry.md`, `main.md`, `catalog.md`, `rails.md`, `ARCHITECTURE.md`, `IMPLEMENTATION.md`; consolidated `TYPE_SYSTEM.md` and `AOT.md` added. Shard files retained until Wave 5 doc retirement.

Scope: all `tools/assay/**` (ARCHITECTURE, IMPLEMENTATION, 22 `.design/*.md`, 15 stub `.py`), `tools/quality/README.md`, root `pyproject.toml`. Bar: coding-python 5/10; assay target 12/10 — dense, verified, single paradigm.

---
## [1][PROBLEM_STATEMENT]
**Score: 4/5**

| [CAPTURED] | [EVIDENCE] |
| --- | --- |
| C#-only reach | `ARCHITECTURE.md` §1; quality rail map is dotnet-centric |
| Shape sprawl (~25 Literals, 14 reports, 3 model systems) | §1 table; `research-holistic-shapes.md` |
| Executor/tool fusion | §1; per-binary modules rejected |
| Projector ceremony (`rail()` eight callables + payload ladder) | §1; `registry.md` cites `quality/__main__.py` |
| Single Envelope / no stdout duality | Aligns with `quality/README.md` §CRITICAL |

**Gaps (why not 5/5):**
- Multi-agent concurrency pain is implied (`settings.md` parallel `ASSAY_*`, disjoint `run_id`) but never stated as a first-class defect of `tools/quality` (shared `.artifacts/quality`, no polyglot fan-out, lease semantics only documented for quality paths).
- Py/TS gate fragmentation (`package.json` `check:py`/`check:ts`) is named as retirement target, not diagnosed (no routing, no Envelope, no artifact isolation for those proofs).
- Agent-only contract (stderr vs stdout, JSON-only consumption, help-without-Envelope seam) lives in `main.md` / quality README, not in `ARCHITECTURE.md` §1 problem table.
- Quality successors (`self-test`, `api` polymorphic engine, mutation opt-in) are end-state rails, not listed as *current* quality capabilities assay must preserve or supersede.

---
## [2][END_STATE]
**Score: 4/5**

| [CAPTURED] | [EVIDENCE] |
| --- | --- |
| Polyglot static/test/docs + C# bridge/package/api | `ARCHITECTURE.md` §2–§6, `catalog.md` row set |
| One Engine, Tool rows, one Envelope/Report | §3–§4, invariants §11 |
| AOT aspects (beartype, structlog, otel, stamina) | §6, `aot-*.md`, `aspect.md` |
| ADT dispatch (`StrEnum`, tagged `detail`, `match`+`assert_never`) | §5, `research-enum-typing.md` |
| Extension = row + optional `detail` tag | §6, `rails.md` §5 |

**Gaps:**
- “Dozens of agents concurrent” is operational (leases, `run_id`, read-only fan-out) but lacks a throughput model: max parallel closures, bridge exclusivity, watch/stream contract, CI vs local agent pools.
- `Envelope` field contract drifts: quality uses `rail`+`data`+`evidence`; assay pins `claim`+`report` (`model.md`) while `registry.md` still emits `rail=`/`data=`.
- `Engine`/`Parser` as `Protocol` in `ARCHITECTURE.md` §5 vs research verdict (module + `Callable`) — end-state doc over-promises ceremony.
- No pinned migration matrix (quality verb → assay `Bind` row), no `self-test`/`watch` registry rows, no Python/TS test coverage ownership in `pyproject` (`coverage.run` still `tools/quality` only).
- Future deps (`watchfiles`, `psutil`, `fsspec`) listed as capabilities without “future vs v1” gate in ARCHITECTURE.

---
## [3][CONFLICT_MATRIX]
Canonical resolution = right column; fix all cited files before Wave 1 code.

| [CONFLICT] | [CANONICAL] | [FIX] |
| --- | --- | --- |
| Aspect stack order | `checked ▷ logged ▷ traced ▷ retried ▷ op` (`aspect.md`, `aot-architecture.md` §3.i) | `engine.md` §4, `registry.md` §2/§4 |
| `@retried` on rail runner | **No** — rail is not `Spawn` | `registry.md` §2 (`compose(..., retried, ...)`) |
| `Engine.run` missing `@logged` | Engine: `checked, traced, retried` only; rail owns bind | `engine.md` §4 vs `aspect.md` §3 seam table |
| `Mode`: capture (`CAPTURE`/`STREAM`) vs operation (`CHECK`/`BUILD`/…) | One `Mode` with `stream`+`writes` payloads (`aot-architecture.md` §3.iii, holistic §6 #1) | `model.md` §1, `catalog.md`, `engine.md`, `rails.md` |
| `Tool.mode` default `CAPTURE` vs catalog `CHECK` | Align default to `CHECK` after Mode unify | `model.md`, `catalog.md` |
| `Language.route`: `Literal["graph","glob"]` vs `Strategy` `closure`/`glob` | `Language.strategy: Strategy` only; delete standalone `Strategy` enum | `model.md`, `routing.md` |
| `Runner.MODULE` prefix | `("uv","run","python","-m")` (repo invocations) | `model.md` vs `catalog.md` §2 |
| `Check` shape | Inline `paths/owner/solution/glob`; `scope` param to `run_check` | `engine.md` (`check.routed`, `check.scope` fields) |
| `Report.claim` vs `rail=` / `Report.rail` | `claim` everywhere (enum name) | `registry.md`, `rails.md` fold, `main.md` context keys optional `claim` |
| `Envelope`: `report` vs `data`; quality `evidence` | `report: Report \| None`, `error: Fault`; drop `data`/`evidence` ladder | `registry.md` `_emit`, `research-cyclopts-projects.md` snippet |
| `RailStatus` members | No `STRICT_FAILED` unless added to `status.md` with exit payload | `registry.md` §3 (`STRICT_FAILED->2`) |
| `Report.counts: dict` vs `Counts` struct | Frozen `Counts(ok,failed,total)` | `model.md`, `rails.md` §1 |
| `ApiSurface.source: dict` | Typed fields on variant | `model.md` §4 |
| `Artifact.kind: str` vs `ArtifactKind` | Field type `ArtifactKind` or drop sharing claim | `model.md`, `settings.md` §5 |
| CLI params `NamedTuple` | Frozen `@dataclass` (`TID251` bans `typing.NamedTuple`) | `main.md` §4, `registry.md` §6 |
| `CliSettingsSource` in settings precedence | Cyclopts-only argv; `(init, env)` only | `settings.md` §4 vs §6 open #2 |
| `Configuration = Literal[...]` | `Configuration(StrEnum)` | `settings.md`, `research-pydantic-core.md` §5 |
| `Tool.parser: str` registry key | `Parser = Callable[[Completed], Detail]` on row | `model.md`, `catalog.md` |
| `Input.bind` on enum | `place(routed, tool)` free function | `catalog.md` §2 vs `routing.md` |
| `catalog` `Mode.WRITE`/`RESTORE`/… | Must exist on unified `Mode` before rows compile | `catalog.md` vs `model.md` |
| `RailStatus.worst` vs `join`/`from_returncode` | One fold API name in `status.md` | `rails.md` §1 (`worst`) vs `status.md` (`join`) |
| Detail wire tags | Explicit `tag="verify"`… not only `str.lower` class names | `model.md` open vs `rails.md` §3 / `aot-architecture.md` §3.iii |
| `REGISTRY` incomplete vs quality surface | Full verb parity table required before implementation | `registry.md` (6 binds) vs quality README §2 (static report/full, test coverage, bridge lifecycle, api query/show, self-test) |
| `compose(traced(bind), logged(bind), …)` | Layers are constants: `logged(event="rail")`, not per-bind factories | `registry.md` §2 |

---
## [4][SHAPE_COLLAPSE_BACKLOG]
**Block implementation until resolved (order = dependency):**

1. **Unify `Mode`** (+ retire separate capture enum and `Tool.mutates` → `writes` payload) — `model.py`, `catalog.py`, `engine.py`.
2. **Delete `Strategy`; fix `Language` route payload** (`closure` not `graph`) — `model.py`, `routing.py`.
3. **`Counts` struct; typed `ApiSurface`; `claim` field names** — `model.py`, all `rails/*.md` folds.
4. **`Params` → frozen `@dataclass`** — `main.md`, `registry.md`, per-rail params modules.
5. **Canonical `Runner` prefixes; `place()` not `Input.bind`** — `model.py`, `catalog.md`, `routing.md`.
6. **`Configuration` StrEnum** — `settings.py`.
7. **Drop `Engine`/`Parser` Protocols** — `ARCHITECTURE.md` §5, `IMPLEMENTATION.md` §4.
8. **Pin `Detail` tags + closed union** before bespoke rails — `model.py` stage 2 gate.
9. **`Envelope`/`Report` wire schema frozen** (incl. quality parity: `truncated`, `notes`, command_path) — single `msgspec` encode path.
10. **Full `REGISTRY` + `TOOLS` tables** — parity with quality commands; no hand-wired gaps.

---
## [5][MISSING_ARTIFACTS]

| [ARTIFACT] | [STATUS] | [NOTE] |
| --- | --- | --- |
| `tools/assay/README.md` | absent | Agent entrypoint, Envelope contract, quality migration |
| `tools/assay/AGENTS.md` | absent | Delta-only load order for assay edits |
| `research-cyclopts-api.md` | absent | API-level Cyclopts 4.16 truth (only `research-cyclopts-projects.md`) |
| `research-cyclopts-snippet.md` | absent | Copy-paste spine for `__main__`/`registry` |
| Spine `.py` implementations | stubs/docstrings only | No types, no `REGISTRY`, no tests |
| `tests/tools/assay/` | absent | No wire laws, fold laws, routing fixtures |
| `pyproject.toml` integration | partial | Runtime deps present (cyclopts, msgspec, OTel, stamina); **not declared:** `watchfiles`, `psutil`, `fsspec` (future per ARCH §6); **not wired:** `coverage`/`mutmut`/`ruff` per-file ignores for `tools/assay`; assay not in `testpaths` scope |
| Code snippets in ARCH/IMPLEMENTATION | narrative only | Need one verified minimal round-trip module cited by IMPLEMENTATION §5 |

---
## [6][DOC_CONSOLIDATION]

| [MERGE] | [INTO] | [KEEP SEPARATE] |
| --- | --- | --- |
| `aot-beartype.md`, `aot-structlog.md`, `aot-otel.md`, `aot-stamina.md` | **`AOT.md`** (one aspect/seam doc) | `aot-architecture.md` → fold into `AOT.md` §doctrine, then delete |
| `research-msgspec.md`, `research-enum-typing.md`, `research-pydantic-core.md`, `research-pydantic-settings.md` | **`TYPE_SYSTEM.md`** (boundary + enums + msgspec policy) | `research-holistic-shapes.md` → summary table only in TYPE_SYSTEM §catalog; archive or trim to §collapse list |
| `research-cyclopts-projects.md` + new `research-cyclopts-api.md` + new `research-cyclopts-snippet.md` | **`CLI.md`** | — |
| `main.md` + `registry.md` (after conflict fix) | **`CLI.md`** §entry + §registry | — |
| `engine.md`, `aspect.md`, `routing.md` | stay **`ENGINE.md`**, **`ASPECT.md`**, **`ROUTING.md`** | — |
| `model.md`, `status.md`, `settings.md`, `catalog.md` | stay; cross-link TYPE_SYSTEM | — |
| `rails.md` | stay (fold recipe); shrink once `rails/*.py` exist | — |

**Recommended agent doc set (build order):** `ARCHITECTURE.md` → `IMPLEMENTATION.md` → `TYPE_SYSTEM.md` → `CLI.md` → `ENGINE.md` / `ASPECT.md` / `ROUTING.md` → leaf (`model`, `status`, `settings`, `catalog`, `rails`) → `AUDIT.md` (this file, retired after Wave 5).

---
## [7][WAVE_EXECUTION_ORDER]

| [WAVE] | [GOAL] | [DELIVERABLES] |
| --- | --- | --- |
| **1 — Vocabulary lock** | Zero forked enums/fields | Patched `model.md`/`status.md`/`settings.md`/`routing.md`/`aspect.md`/`engine.md`/`registry.md`/`main.md`; publish `TYPE_SYSTEM.md`; collapse §4 items 1–7 |
| **2 — Wire + CLI spine** | One Envelope encode, Cyclopts tree | `core/status.py`, `core/model.py`, `composition/settings.py`, `CLI.md` + cyclopts research; `__main__.py` + `registry.py` skeleton; wire round-trip tests |
| **3 — Engine + routing** | Runnable `Check` | `core/engine.py`, `core/routing.py`, `core/aspect.py`, `AOT.md`; port leases from `quality/process.py`; routing golden tests |
| **4 — Data plane** | Full catalog + thin rails | `catalog.py`, `rails/static|test|docs.py`, then `bridge|package|api.py`; complete `REGISTRY`; README + AGENTS.md |
| **5 — Parity + retirement** | Supersede quality entrypoints | Parity matrix vs `tools/quality/README.md`; `pyproject` coverage/mutmut/ruff paths; optional deps gated; deprecate `check:py`/`check:ts` when static/test parity proven |

**Rule:** No `rails/*.py` until Wave 1 docs + Wave 2 `Report`/`Envelope` types are frozen (prevents `detail`/field rewrites).

---
## [8][EXECUTIVE_SUMMARY]
The assay design credibly diagnoses `tools/quality` shape sprawl and projector ceremony and proposes the right collapse (Tool rows, one Engine, one `Report`/`Envelope`, aspect seams), but several authoritative files still disagree on stack order, `Mode` meaning, and `claim` vs `rail` naming — implementation before Wave 1 doc reconciliation will hard-fork the codebase. The largest structural debt is an overloaded `Mode` enum and lint-banned `NamedTuple` CLI params, both guaranteed CI failure under current `pyproject.toml`. The registry and catalog are illustrative, not complete versus the quality command surface, so parity risk is high for bridge/package/api/self-test verbs. Runtime integration is ahead on declared libs but behind on tests, coverage scope, and future-only deps (`watchfiles`, `psutil`, `fsspec`). Consolidate five `aot-*` and four `research-*` shards into `AOT.md` and `TYPE_SYSTEM.md` before agents implement, or parallel waves will reintroduce dual paradigms (pydantic CLI + msgspec wire, `Protocol` + module functions).

---
## [FURTHER_CONSIDERATION]
- **`__cyclopts_returncode__` vs explicit exit:** Cyclopts 4.16 + `result_action="return_value"` may not auto-read the protocol; pin one pattern in `CLI.md` with a test (`research-cyclopts-projects.md` §6).
- **Fold monoid:** fuse `RailStatus.join` and `Counts` derivation in one `reduce` over outcomes (`research-holistic-shapes.md` esoteric) — halves rail fold cost.
- **`msgspec.Raw` on `VerifySummary`:** prefer one opaque bridge field over `defstruct` proliferation for C# JSON (`model.md` open #2).
