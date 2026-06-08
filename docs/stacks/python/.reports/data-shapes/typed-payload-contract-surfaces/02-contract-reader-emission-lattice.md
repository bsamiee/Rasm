# Contract Reader Emission Lattice

# Emission Mandate

- Payload law lives in `TypedDict` declarations; reader emission is the sole programmatic field authority that turns declarations into boundary tables, OpenAPI fragments, hypothesis registries, and adapter field maps — prose field lists and parallel JSON schemas are rejected.
- Authoring truth and operational authority split deliberately: developers declare keys, requiredness, openness, and `ReadOnly` evidence on the owner; readers emit frozen rows that every downstream consumer must read — re-deriving field sets from comments, `model_dump` keys, or fixture prose is a duplicate-contract defect.
- One payload owner module emits one contract table row per concrete specialization; emission runs at build or test collection time, not on hot ingress paths.
- Readers consume type-only payload modules without importing domain owners, composition roots, or adapter implementations — emission is an orthogonal static layer bound at the composition root, not a substitute for promotion gates.
- Divergence between reader-emitted rows, `TypeAdapter(Payload).json_schema()`, and adapter correspondence tables is a duplicate-contract defect — merge blocks at the emission lattice before integration proof chains execute.

# Field Authority Charter

- Field authority names which surface owns declared keys, requiredness frozensets, read-only flags, openness posture, `extra_items` bounds, and discriminant vocabulary for a payload concept — only reader-emitted rows hold that authority; `TypedDict` annotations are the source graph readers fold, not a runtime registry adapters may re-parse ad hoc.
- Authority flows one direction: declaration → emission fold → consumer read — adapters, codegen, hypothesis strategies, provider matrices, and metamorphic proof chains are consumers; none may publish parallel field lists that precede or override emitted rows.
- `TypeAdapter(Payload).json_schema()` is the wire-schema oracle that must agree with emitted rows — adapter schema leads when bytes and OpenAPI publish; reader rows lead when strategies, correspondence maps, and drift negatives generate — disagreement attributes to emission fold bugs before adapter or gate defects.
- Interior domain modules never import emitted rows as parameter types or runtime validators — owners past the promotion gate own durable invariants; row authority terminates at pre-materialization evidence.
- Partial fan-out — adapter map updated without row, row updated without hypothesis registry, schema snapshot updated without oracle rerun — is a field-authority fracture; Stage 5 emits all consumers in one unit or merge blocks.

# Read Grade Lattice

- PEP 649 defers annotation evaluation through `__annotate__` thunks; PEP 749 makes `annotationlib.get_annotations` the canonical read path — raw `__annotations__` may be stale, misses deferred thunks, strips `Annotated` stacks, and produces false field sets on generic payload owners.
- Read through `annotationlib.Format` constants only — not legacy `inspect` format globals; supported grades are `VALUE`, `FORWARDREF`, and `STRING`.
- **STRING** (`Format.STRING`) preserves source-level annotation text for documentation diff and law-matrix collection that must not import unresolved peers — emission snapshots for renderers only; not validation or schema authority.
- **FORWARDREF** (`Format.FORWARDREF`) resolves defined names to values and substitutes `annotationlib.ForwardRef` for undefined peers — generic payload owners read body slots and `extra_items` type parameters here before concrete specialization rows emit; never throws on unresolved forward names.
- **VALUE** (`Format.VALUE`, default) materializes full annotations for requiredness folds, `ReadOnly` detection, and `TypeAdapter` parity oracles — may raise `NameError` on unresolved forward refs; hot decode paths never call VALUE reads; reader emission and adapter warm-up own VALUE at first-touch.
- `include_extras=True` is mandatory when `Annotated` layers carry admission metadata on payload-adjacent ingress models — default reads collapse to bare `T` and silently drop constraint nodes reader tables must not claim.
- Read grade selection is not caller convenience — wrong grade at a lattice slot produces false-green parity: STRING snapshots beside VALUE-driven `json_schema()` diff tests are merge blockers.

# Introspection Fold Algebra

- Requiredness folds from `__required_keys__` and `__optional_keys__` frozensets — `__total__` alone is insufficient when `Required`/`NotRequired` or mixed inheritance merges bases.
- Multiple inheritance merges required and optional key sets as set unions with per-key conflict resolution: a key required in any base remains required unless the merging child explicitly marks `NotRequired` and inheritance law permits the override.
- Openness folds from `__closed__` and `extra_items` annotation on the owner — missing `__closed__` records default-open assignability posture at introspection; emission must still echo the declared posture, not infer permissive authoring from absence.
- `ReadOnly[T]` wrappers strip for codec dispatch value types while preserving read-only flags in the emitted row — static signature emission keeps wrappers; runtime validation strips to `T` for `TypeAdapter` field compatibility.
- `extra_items` type folds into a dedicated row column when present; `closed=True` and `extra_items` mutual exclusivity is validated at emission — combining them is a runtime owner defect caught before table fan-out.
- Discriminant keys fold as required `Literal` or closed vocabulary enum members with a linked vocabulary module import path — bare string tags without literal evidence fail emission before matrix modules compile.

# Emitted Row Shape

- Each row binds `{owner_id, specialization_args?, required_keys, optional_keys, read_only_keys, closed, extra_items_type?, discriminant_key?, vocabulary_ref}` — adapters, factories, and OpenAPI fragments consume identical column semantics.
- `required_keys` and `optional_keys` are disjoint frozensets; `read_only_keys` is a subset of the union — a key may be required and read-only simultaneously.
- Generic owners emit one row per concrete argument tuple registered at the composition root — unbound generic rows are invalid integration sources.
- Wire-visible payloads include a `schema_version` column when boundary envelopes carry version literals — egress and ingress rows for the same concept share `vocabulary_ref` but may diverge on required-key frozensets when stage roles differ.
- Stage role tags (`boundary`, `staging`, `patch`, `event`, `egress`, `keyword`) attach to rows without forking owner modules — one owner per concept per stage, multiple rows only across specializations, not across duplicate prose tables.

# Emission Pipeline Topology

- Stage 1 — locate owner module and concrete specialization registry at composition-root bind time; triage outcome for the concept-stage pair indexes which owner modules participate.
- Stage 2 — `annotationlib.get_annotations(owner, format=Format.FORWARDREF)` resolves generic parameters and forward names; `model_rebuild()` on embedded ingress models runs before Stage 3 when Pydantic fields wrap payload slots.
- Stage 3 — `Format.VALUE` read plus introspection frozenset folds produce the canonical row; inheritance walks merge base rows only when the child does not redeclare conflicting requiredness.
- Stage 4 — parity oracle compares row frozensets to `TypeAdapter(specialized_payload).json_schema()` property keys and required arrays — mismatch attributes to reader fold bug before adapter bug.
- Stage 5 — fan-out writes adapter field maps, hypothesis registry keys, OpenAPI fragments, and matrix module expected frozensets in one emission unit — partial fan-out is a merge blocker.
- Emission order is static layer before contract tables before hypothesis strategies before root round-trip — harness layer inversion fails CI at emission, not at integration smoke.

# Generic Specialization Binding

- Type parameters on `class SlotPayload[T](TypedDict)` resolve at reader and adapter sites — emission never maintains parallel non-generic siblings to dodge forward-ref proof.
- `extra_items=T` specialization requires paired rows: `T` bound, `ReadOnly` wrapper on `T`, and extension-band hypothesis registry slice — unbounded `object` erosion in generic clients fails emission lint.
- PEP 696 default type parameters stabilize rows for fixed extension atoms — default specialization emits first; additional rows register only when composition root admits distinct concrete bands.
- When specialization changes, reader frozenset emission, `TypeAdapter` concrete argument rows, and matrix repro modules update in one version unit — partial specialization retarget is a merge blocker.

# TypeAdapter Parity Oracle

- `TypeAdapter(Payload).json_schema()` is wire-schema authority for payload names — reader-emitted OpenAPI fragments diff against adapter output, not against hand-maintained fixture dicts.
- Parity oracle runs per concrete specialization after `model_rebuild()` ordering documented at root first-touch — pydantic-wrapped payload fields defer emission until rebuild completes.
- Property key sets, `required` arrays, `additionalProperties` posture, and `readOnly` hints must agree — assignability-open payloads may admit `additionalProperties` while construction remains closed; oracle documents both postures explicitly.
- Mypy and ty deferred-thunk disagreements on generic payloads pin `from __future__ import annotations` posture in matrix modules — oracle compares frozenset outputs across backends until parity is proven; one green backend does not relax row columns.

# Field Authority Consumers

- **Adapter field maps** — wire-key correspondence, alias routing, and anti-corruption tables derive canonical key lists from row columns; handwritten maps beside emitted rows are collapse defects.
- **OpenAPI fragments** — published field names, required arrays, enum discriminants, and `additionalProperties` posture copy from rows after oracle green — not from parallel JSON contract files.
- **Hypothesis registries** — `st.fixed_dictionaries` and `st.builds` key off `required_keys`, `optional_keys`, `closed`, and `extra_items_type`; `st.from_type(Payload)` on closed owners bypasses field authority and is rejected.
- **Drift negative generators** — forbidden keys and wrong discriminant literals sample from row `closed` and `discriminant_key` metadata — hand-authored prose lists are rejected.
- **Metamorphic proof chains** — lawful payload dict → validate → promote → project egress → validate assignability reads row columns as field authority; chain failures attribute to adapter binding when row columns are current.
- **Provider compatibility matrices** — correspondence-ready key lists come from boundary rows; provider rename tables live in adapters, not duplicated in codegen.
- Consumer modules import rows or pre-emitted tables — they do not re-run reader factories at hot paths or maintain shadow field lists.

# Codegen And Fixture Consumption

- OpenAPI fragment generators import emitted rows — field names, requiredness, and enum discriminants never duplicate in JSON contract files.
- Hypothesis `st.builds` and `st.fixed_dictionaries` strategies key off `required_keys` and `optional_keys` columns — `st.from_type(Payload)` on closed owners is rejected.
- Test factories for lawful closed literals and drift negatives draw forbidden keys from rows marked `closed=True` — extension-band strategies read `extra_items_type` for value bounds disjoint from declared key sets.
- Provider compatibility matrices consume correspondence-ready key lists from boundary rows — adapter maps wire keys to row canonical keys once; codegen does not embed provider rename tables.

# Cross-Checker Emission Matrix

- Matrix modules pin one minimal owner per proof edge: `closed=True`, `extra_items`, mixed `Required`/`NotRequired`, generic `TypedDict[T]`, `ReadOnly` variance, and inheritance narrowing under `extra_items=ReadOnly[T]`.
- Each module asserts reader row frozensets match checker-visible requiredness — static proof runs pyright, mypy, and ty before runtime `TypeAdapter` samples execute.
- Assignability versus construction splits own distinct matrix modules — reader rows document both postures; checker errors at the wrong site indicate mis-tagged proof modules, not row drift.
- Callable seam modules are out of scope for reader emission — `Unpack` preservation proves separately; reader lattice covers dict contract owners only.
- PEP 728 inheritance edges mypy tracks as open — subclass under `extra_items=ReadOnly[T]`, narrowing `extra_items`, closed parent sibling variants — each edge owns a minimal repro module until tracking closes.
- One passing backend does not relax row columns required by others — suppressions and `cast` at reader fold sites fail lint before matrix execution.

# Inheritance Row Merge Law

- Child payload owners emit independent rows — emission does not inherit merged frozensets implicitly from bases; the fold algebra walks MRO and applies PEP 728 inheritance rules explicitly.
- Subclass under `closed=True` parent cannot widen keys — emission rejects child declarations that add keys to a closed parent; sibling closed body payloads per variant emit separate rows linked by shared `vocabulary_ref`.
- Narrowing `extra_items` under `extra_items=ReadOnly[T]` parent is permitted — child row records narrowed `extra_items_type`; widening `T` or removing `closed=True` from a closed parent fails emission before matrix compile.
- `total=False` base with `Required` overrides in subclass merges optional defaults with per-key required promotions — row `optional_keys` drops keys promoted to required unless inheritance law blocks the promotion.

# Envelope-Body Row Linking

- Semi-closed variant families emit two linked rows: closed envelope with read-only identity fields and body payload per discriminant arm — `vocabulary_ref` and `discriminant_key` columns join rows; `match` exhaustiveness proves at promotion gates, not in reader output alone.
- Body rows carry arm-specific `required_keys` frozensets — envelope row never absorbs body keys; emission failure when a single row mixes envelope identity with variant body fields.
- Open extension arms record `extra_items_type` on ingress or staging rows only — promotion fold targets appear as `extension_fold_target` metadata column (`frozendict`, tuple) for hypothesis snapshot proofs.
- Illegal FSM transition fault payloads emit separate fault rows with enum-member discriminant vocabulary — out-of-vocabulary wire tokens are drift negatives keyed on envelope row `discriminant_key`.

# Dual-Surface And Keyword Row Binding

- CLI cyclopts `Parameter` omission lists and settings `validation_alias` tables do not emit separate payload owners — staging rows at the root bind `NotRequired` columns to adapter-documented omission semantics; one row per staging concept.
- Sentinel-parameter defaults on keyword entrypoints pair with `*` boundaries — row metadata flags `sentinel_boundary` when `NotRequired` keys align with cyclopts omission; proof verifies positional callers cannot skip into sentinel arms.
- `Unpack[Payload]` keyword surfaces do not emit distinct row shapes — the underlying `TypedDict` owner row is authoritative; callable seam proof consumes the same `required_keys` frozenset via `inspect.signature` after `inspect.unwrap`.
- Dual CLI and HTTP ingress for one concept share one boundary or staging row — root wiring differs by carrier; row `vocabulary_ref` is singular.

# Trusted Replay Row Pinning

- Trusted-replay paths pin the same row `owner_id` and specialization as live ingress beside `schema_version` at the composition root — replay emission never shortcuts to a different boundary artifact class than triage bound.
- Migration modules emit superseded row snapshots with version literals — active promotion `match` arms reference current rows only; retired discriminants remain in migration row history for `assert_never` witnesses.
- Store-key and adapter-module identity columns on replay rows prevent cross-context row reuse — identical frozensets in two bounded contexts still require distinct `owner_id` unless shared vocabulary module import documents federation.

# Integration Chain Row Consumption

- Root metamorphic chains read emitted rows as field authority: lawful payload dict → `TypeAdapter.validate_python` → promote → project egress → validate assignability — chain failures attribute to adapter binding when row columns are current.
- Drift negative samples beside owner modules reference row `closed` and `discriminant_key` columns — forbidden keys and wrong literals are generated from row metadata, not hand-authored prose lists.
- Egress assignability proof builds dict literals from canonical projection, asserts compatibility against egress row `required_keys`, then validates through `TypeAdapter` when wire compliance is closed.
- Extension capture tests read `extension_fold_target` metadata — promotion must fold `**extensions` into immutable snapshots named by the column value.
- Chain catalog rows bind `emitted_row_ref` to `{owner_id, specialization_args?, schema_version?}` — chains without row refs are incomplete integration surfaces.

# Collapse Signals At Emission

- JSON contract file maintained beside emitted row — collapse to adapter-derived `json_schema()` and reader fan-out only.
- Hypothesis registry keys not derived from row columns — collapse strategies to `st.fixed_dictionaries` keyed on frozensets.
- Adapter field map handwritten while row exists — collapse map generation to Stage 5 fan-out.
- Two rows for same `owner_id` and specialization with divergent frozensets — collapse duplicate emission passes to single fold algebra bind.
- Domain module importing emitted row as parameter type — collapse to owner promotion at the composition-root gate.
- Interior module re-parsing `TypedDict` annotations for field lists when a row exists — collapse to row consumer import.

# Failure Attribution

- Row missing declared key present in source owner — fold algebra bug or wrong `annotationlib.Format` grade.
- `json_schema()` property superset versus row key set — adapter specialization mismatch or missing `model_rebuild()` before emission.
- Hypothesis strategy accepts drift keys on closed payload — registry not keyed on emitted `closed` column.
- Domain module imports reader emission tables as runtime authority — category error; domain imports owners; adapters import rows and payload types.
- Interior module constructs `TypeAdapter` from reader output — boundary leak; root owns adapter warm-up bound to emitted specialization registry.
- Parallel JSON Schema file diverges from oracle diff — collapse to adapter-derived schema and reader fan-out only.
- Consumer publishes field list not present in any emitted row column — duplicate-contract surface; regenerate from Stage 3 fold or fix oracle skew.

# Evolution Binding

- Payload key addition, discriminant arm extension, or openness flag change updates owner declaration, emitted row, adapter field map, `json_schema()` snapshot, and hypothesis registry in one promotion unit — emission lattice blocks partial evolution.
- Closed key removal or requiredness tightening emits migration row metadata when stored bytes carry retired keys — reader documents retired keys in migration modules, not in active row columns.
- Deprecation retires row specializations only after migration proves zero ingress dependency — retired rows remain in migration snapshots with `assert_never` witnesses on promotion arms.
- Provider remap table changes bind ingress adapter correspondence, boundary row canonical key set, and anti-corruption field map — external key renames never reach row columns without adapter-owned mapping updates.
- Callable payload seam changes do not fork row shapes — root handler `Unpack` preservation proves separately; row `required_keys` remains the keyword authority.

# Composition Root Emission Ownership

- The composition root registers concrete specialization tuples, pins `schema_version` literals for replay rows, and triggers first-touch emission warm-up — interior modules import payload types and consume pre-emitted tables only.
- Root binds Stage 5 fan-out destinations per ingress route — HTTP, CLI, settings, and trusted-replay carriers share row authority; wiring differs by carrier, not by duplicate field lists.
- Arch import rules extend to emission consumers: domain modules must not import reader factories or emitted tables; adapters and test harnesses import rows; codegen imports rows without importing composition roots.
- Multiple ingress routes for one concept register one `owner_id` per stage — root triage outcome row is the emission registry index.

# Property Testing Row Binding

- Discriminant-conditioned strategies mirror envelope-body row links — outer kind sampled from `vocabulary_ref`, body fields conditioned on arm-specific `required_keys`; independent random tags filtered only at runtime validation are rejected as strategy design.
- Shrinking rebuilds lawful dicts from row columns after shrink steps — invalid extra keys on `closed=True` rows and illegal read-only mutations reject at construction gate, not as shrink endpoints.
- Stryker mutation on promotion `match` arms requires kill ratio keyed on row `discriminant_key` vocabulary — mutants dropping arms must fail exhaustiveness type-check or contract table parity before behavioral suites run.
- Adding a row column or metadata flag without updating hypothesis registry slice and drift negative generator is a merge blocker — static emission and generative suites encode identical closed membership.

# Pre-Materialization Boundaries

- Reader emission encodes dict contract field law only — decorator wrap depth, ParamSpec preservation stacks, and callable altitude ordering prove on keyword surfaces separately; `Unpack` rows type the underlying payload, not wrapper layers.
- Materialized owner schemas past the promotion gate are out of scope — reader output stops at pre-materialization evidence; canonical `model_fields` parity oracles belong on owner proof modules, not payload row emission.
- Patch-stage rows carry `NotRequired`-only update columns and `read_only_keys` on identity fields — row metadata tags `stage=patch`; replace proof consumes columns at the composition root, not inside interior domain folds.

# Rejection Shortcuts

- Raw `__annotations__` reads in reader factories — use `annotationlib.get_annotations` with explicit `Format` grade.
- Hand-maintained field lists beside emitted rows — duplicate contract surfaces bypassing field authority.
- `st.from_type(Payload)` or table-less builders on closed payloads — strategies must read row columns.
- Emission at hot ingress — readers run at collection or build; adapters consume pre-emitted tables.
- Non-generic sibling payloads dodging generic forward-ref proof — specialize at reader sites.
- STRING-grade snapshots as schema authority — VALUE grade plus `TypeAdapter` oracle owns wire truth.
- Consumer-generated OpenAPI or fixture dict that disagrees with emitted row after oracle green — field-authority fracture; fix consumer or re-emit, never maintain parallel truth.
