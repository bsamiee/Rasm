# [H1][CRITIQUE_SHAPES_WAVE2]
>**Dictum:** *Wave 1b locked vocabulary on paper; Wave 2 proves the shape catalog still forks across leaves, shards, and snippets — not implementation-ready.*

Scope: post–Wave 1b `model.md`, `status.md`, `TYPE_SYSTEM.md`, reconciled leaves, `research-holistic-shapes.md`, `AUDIT.md` §4 backlog. Bar: coding-python **5/10** (expression style, one model per concept, no weak bags, Protocol only at real seams).

---
## [1][VERDICT]

| [GATE] | [STATUS] |
| --- | --- |
| Shape catalog frozen | **NO** — `Completed` receipt, detail tags, `Fault` channel, and `Mode` semantics still disagree across authoritative files. |
| Single paradigm | **NO** — consolidated `TYPE_SYSTEM.md` vs stale `research-msgspec.md` §6 and `research-holistic-shapes.md` §1 still preach opposite `Mode`/`parser` shapes. |
| Registry parity | **NO** — 8 `Bind` rows vs quality surface (`static report/full`, `test coverage`, `package *`, `api query/show/doctor`, `self-test`, bridge lifecycle). |
| Snippet implementability | **NO** — `rails.md`/`registry.md`/`engine.md` snippets reference symbols and fields that do not exist on canonical structs. |

**Implementation-ready:** **false.** Wave 2 may start `status.py` + wire laws; **block** `engine.py`, `rails/*`, and full `REGISTRY` until P0–P1 close.

---
## [2][SNIPPET_QUALITY_VS_5/10_BAR]

Ratings judge copy-paste fidelity to coding-python (typed folds, one vocabulary instance, no phantom fields, `Result` channel discipline).

| [DOC / CLUSTER] | [RATING] | [WHY] |
| --- | :---: | --- |
| `status.md` | **8/10** | StrEnum payload + `join` monoid is dense and wire-verified; open fold-exit policy is honest. |
| `model.md` | **6/10** | Canonical structs clear; `Completed` omits fold inputs; `run_check(..., routed=)` vs engine `_argv` disagree; explicit `tag=` vs ARCH `tag=str.lower`. |
| `catalog.md` | **7/10** | Row-as-data holds; duplicate `name` twins and `Claim.PACKAGE` rows with no registry handler. |
| `routing.md` | **6/10** | `place()` uses free `settings`; `RouteScope = Literal[...]` survives despite enum policy; §5 still names deleted `Strategy`. |
| `registry.md` | **4/10** | `Handler(..., Scope, ...)` vs `ArtifactScope`; §5 `tool.bind` / `Claim.LINT` phantom; `_emit` OK but handler contract wrong. |
| `rails.md` | **3/10** | `Completed.status/artifacts/matches/note`, `Engine.run_all`, `bind_check` — none on `model.md` `Completed`; fold counts from wrong type. |
| `engine.md` | **4/10** | Non-zero exit → `Fault` contradicts fold; `_argv` shadows `scope`; `Engine.run` naming vs `run_check`/`fan_out`. |
| `aspect.md` | **7/10** | Slot algebra solid; structlog keys still `rail` not `claim` (name fork with `Envelope.claim`). |
| `TYPE_SYSTEM.md` | **7/10** | Good consolidation; does not retire contradicting shards; `Bind` listed but absent from `model.md`. |
| `research-holistic-shapes.md` | **4/10** | **Stale:** §1 still marks unified `Mode` "COLLAPSE", omits `Claim.PACKAGE`, worked path uses `tag=str.lower` → `verifysummary`. |
| `research-msgspec.md` §6 | **2/10** | **Active anti-canonical:** argues capture-only `Mode`, `parser: str` key, opposite of Wave 1b `model.md`/`catalog.md`. |
| `ARCHITECTURE.md` §4–§5 | **5/10** | "mutation flag" on `Tool`; `tag=str.lower`; `Engine.run` Protocol drift vs IMPLEMENTATION §4 fix. |

**Cluster mean:** **~5.6/10** — at bar on isolated leaves, **below** bar on cross-doc executable paths (rail → engine → fold → envelope).

---
## [3][PRIORITIZED_FIX_LIST]

### P0 — Block implementation (shape lies)

| [#] | [DEFECT] | [FIX] | [FILES] |
| --- | --- | --- | --- |
| 1 | **`Completed` under-specified** — fold expects `status`, `artifacts`, `matches`, optional `note`; model has bytes only. | Extend `Completed` **or** move projection to `run_check` return and rewrite fold to consume `Result[Completed, Fault]` only. Pin one receipt algebra. | `model.md`, `engine.md`, `rails.md` |
| 2 | **`Fault` vs `Completed` channel** — engine maps analyzer non-zero → `Fault`; status algebra says `FAILED` is a **status**, not `Envelope.error`. | Non-zero process exit → `Completed` + parser/`from_returncode`; reserve `Fault` for spawn/timeout/lease/no-process. | `engine.md`, `research-msgspec.md` §6, `TYPE_SYSTEM.md` §4 |
| 3 | **Detail wire tags fork** — `model.md` pins `tag="verify"`; ARCH/IMPLEMENTATION/holistic use `tag=str.lower` (`verifysummary`). | One policy in `TYPE_SYSTEM.md`; update ARCH §5, IMPLEMENTATION §4, holistic §2, delete lowercased-class-name examples. | `model.md`, `TYPE_SYSTEM.md`, `ARCHITECTURE.md`, `IMPLEMENTATION.md`, `research-holistic-shapes.md` |
| 4 | **Executor naming/API spam** — `run_check`, `fan_out`, `Engine.run`, `Engine.run_all`, `Check.run` used interchangeably. | One module surface: `run_check`, `fan_out`, `run_all` (or single `run` polymorph); grep-replace all leaves. | `engine.md`, `rails.md`, `aspect.md`, `ARCHITECTURE.md`, `aot-*.md` |
| 5 | **Shard contradicts canonical** — `research-msgspec.md` §6 rejects unified `Mode` and `Callable` parser. | Mark §6 superseded by Wave 1b or rewrite to match `model.md`; same for holistic §1 Mode verdict + missing `PACKAGE`. | `research-msgspec.md`, `research-holistic-shapes.md` |

### P1 — Collapse missed in Wave 1b

| [#] | [DEFECT] | [FIX] | [FILES] |
| --- | --- | --- | --- |
| 6 | **`RouteScope` Literal** — bounded vocabulary on wire struct. | `Scope(StrEnum){CHANGED,FULL}` on `Routed.scope` (`research-enum-typing.md`). | `routing.md`, `model.md` if `Routed` moves |
| 7 | **`Match.kind: str`** vs `Artifact.kind: ArtifactKind` — same axis, weak vs strong. | `MatchKind` StrEnum or reuse `ArtifactKind` subset; ban free string. | `model.md`, parsers in `catalog.md` §4 |
| 8 | **Metric double-ownership** — `Counts` on `Report` **and** `ok/failed/total` on `VerifySummary`/`TestRun`. | Derive fold counts from outcomes only; detail variants hold **non-derivable** evidence (`report_dir`, `coverage`, `mutation`). | `model.md`, `rails.md` §3 |
| 9 | **`PackageRun` shape drift** — model fields vs rails prose (`slug`, `PackageStepKind`, `PackageMeta`). | One typed variant; demote step policy to ordered `tuple[str,...]` on variant, not top-level enum. | `model.md`, `rails.md` §3 |
| 10 | **`Bind` orphan** — in TYPE_SYSTEM catalog, not in `model.md`; handler type uses wrong `Scope`. | Add `Bind` to `model.md` or drop from catalog; align `Handler` with `ArtifactScope`. | `model.md`, `registry.md`, `TYPE_SYSTEM.md` |
| 11 | **`claim` vs `rail` context keys** — aspect/aot docs bind `rail=`; wire uses `claim`. | Rename structlog/otel attrs to `claim` everywhere. | `aspect.md`, `aot-otel.md`, `aot-architecture.md` |

### P2 — Registry / catalog completeness

| [#] | [DEFECT] | [FIX] | [FILES] |
| --- | --- | --- | --- |
| 12 | **Missing binds** — no `static report/full`, `test coverage`, `package *`, `api query/show/doctor`, `self-test`. | Parity matrix vs `tools/quality/README.md` §2; expand `REGISTRY` before Wave 4. | `registry.md`, `catalog.md`, `AUDIT.md` §3 |
| 13 | **`Claim.PACKAGE` catalog rows, zero handlers** — yak tools orphaned. | `rails/package.py` + `Bind` rows or remove rows until handler exists. | `catalog.md`, `registry.md` |
| 14 | **`settings.log_format: Literal`** — third micro-vocabulary. | `LogFormat(StrEnum)` or derive-only (no env field). | `settings.md`, `composition/settings.py` |

### P3 — Doc hygiene (Wave 5 or now if coding)

| [#] | [DEFECT] | [FIX] | [FILES] |
| --- | --- | --- | --- |
| 15 | ARCH §4 "mutation flag" | → `Mode.writes` / unified `Mode` | `ARCHITECTURE.md` |
| 16 | `place()` snippet references undefined `settings` | Pass `settings` into `place` or close over `AssaySettings` | `routing.md` |
| 17 | Registry §5 `Claim.LINT` example | Replace with real `Claim.STATIC` or mark hypothetical | `registry.md` |
| 18 | Retire duplicate shard guidance | Fold contradicting § into `TYPE_SYSTEM.md`; trim holistic to pointer | `AUDIT.md` §6 |

---
## [4][DUAL_PARADIGMS_STILL_ALIVE]

| [FORK] | [CANONICAL (Wave 1b)] | [STILL COMPETING] |
| --- | --- | --- |
| `Mode` | 13 members + `stream`/`writes` payloads (`model.md`) | `research-msgspec.md` capture-only `Mode`; holistic §1 "COLLAPSE" wording |
| `Tool.parser` | `Callable[[Completed], Detail \| None] \| None` on row | `research-msgspec.md` string registry key |
| Detail tags | Explicit short tags (`verify`, …) | `tag=str.lower` in ARCH, IMPLEMENTATION, holistic snippet |
| Process failure | Status on `Report` via `Completed` | Non-zero exit as `Fault` in `engine.md` |
| Handler scope type | `ArtifactScope` (`main.md`) | `Scope` (`registry.md`) |

---
## [5][COLLAPSE_OPPORTUNITIES_MISSED]

1. **Fold monoid fusion** (AUDIT esoteric) — `RailStatus.join` + `Counts` in one `reduce` over outcomes; `rails.md` still double-scans and uses phantom `Completed.status`.
2. **`Mode` as lease/parallel carrier** (holistic §FC) — not adopted; lease table duplicated in `rails.md` §4 instead of enum payload.
3. **`VerifySummary` + `msgspec.Raw`** for C# facts — open in `model.md`; rails §3 still prose `facts`/`captures` without typed/Raw field.
4. **`defstruct` vs closed union** — TYPE_SYSTEM allows both; no rule for when `Report.detail` union widens vs per-rail decode (backdoor to 14-report sprawl).
5. **Duplicate tool `name` keys** — `ruff-format`/`ruff`/`dotnet-test`/`rasm-bridge`/`yak` twins need `(name, mode)` identity or fold breaks row lookup.

---
## [6][WAVE2_ENTRY_CRITERIA]

Proceed to code only when:

1. P0 items 1–3 resolved in **one** amended `model.md` + matching `engine.md`/`rails.md` snippets.
2. Shards marked superseded or rewritten (`research-msgspec.md` §6, `research-holistic-shapes.md` §1/§2).
3. Wire law tests scoped: `RailStatus` alias round-trip, explicit `detail` tags, `Envelope` encode — **before** `run_check` lands.

---
## [FURTHER_CONSIDERATION]

- **`Completed` never on wire** is the strongest argument for a richer internal receipt — do not shrink the fold to match an anemic struct; enrich `Completed` and keep `Detail` post-fold only.
- **`Source` Protocol** remains the sole justified Protocol; any second Protocol for parsers or handlers reopens Wave 1b item 7.
- **Esoteric:** cached module-level `msgspec.json.Encoder`/`Decoder(Report)` (`research-msgspec.md` §5) is omitted from IMPLEMENTATION verification — add to Wave 2 wire spine or accept per-call codec cost.
