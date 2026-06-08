# Model Materialization Pipeline

# Critical Signals

- Seven ordered stages each with one owner surface — ingress carriers (`bytes`, `dict`, staging views) never survive past validation exit; inter-stage algebra composes as boundary `capture`/`async_capture` with `expression` `Result`.
- Module-level `TypeAdapter`/`Encoder`/`Decoder` singletons — never per-request construction; single-pass decode (`validate_json`, tagged `Decoder.decode`) replaces `json.loads` then validate.
- Pydantic owns untrusted ingress; msgspec owns internal wire and cache rows — three projection families (`ingress`, `domain`, `wire`) with explicit boundary projection, not parallel DTO/model/struct triples.
- Materialization exit is the only type domain modules accept without adapter re-entry — enrichment uses immutable replacement (`model_copy`, `structs.replace`, smart constructors returning `Result`).
- Round-trip proof (`decoder.decode(encoder.encode(value))`) required before persist on polymorphic slots — proof is egress guard at root `_validated`, not interior domain logic.
- Composition root owns REGISTRY, codec singletons, envelope emit chain, and one-write stdout invariant — leaf domain modules import canonical owners only.
- Unknown `schema_version` fails pass-one envelope decode — migration folds at read boundary via `msgspec.convert`, one hop per read; deterministic `order="deterministic"` when bytes become cache keys.
- Stage-first failure attribution — validation faults at ingress adapter, decode faults at wire owner, proof faults at root guard, handler faults after materialization exit.

# Seven-Stage Pipeline

- Ordered stages: ingress payload → validation → normalization → construction → enrichment → immutable materialization → outbound serialization.
- Each stage has exactly one owner surface; crossing stages without an explicit adapter is a boundary violation.
- Fallible stages return `Result[T, E]` via boundary `capture`/`async_capture`; inter-stage algebra composes as `pipe(ingress, validate, bind(domain_transform), map(project), encode)` using `expression` `Result`.
- `object`, `dict[str, object]`, and `bytes` are ingress carriers only — they must not survive past the validation owner.
- A stage may collapse two logical transforms only when the owning codec documents single-pass semantics (`validate_json`, tagged `Decoder.decode`). Splitting parse and validate across stage boundaries without an adapter is a contract break.
- Downstream stages treat upstream exit artifacts as trusted for shape legality, not for business truth; trust posture is per-stage.
- Ingress payload stage owner: transport adapter; exit artifact: typed carrier or bytes.
- Validation stage owner: `TypeAdapter` or `msgspec.Decoder`; exit artifact: frozen model or `Struct`.
- Normalization stage owner: Pydantic `BeforeValidator` / `model_validator(before)` / msgspec hooks; exit artifact: wire-normalized instance of same runtime type.
- Construction stage owner: `BaseModel` / `Struct` / smart constructor; exit artifact: first immutable instance.
- Enrichment stage owner: domain factory / `structs.replace` / `model_copy`; exit artifact: enriched immutable instance.
- Immutable materialization stage owner: rich owner or frozen model; exit artifact: canonical domain object.
- Outbound serialization stage owner: msgspec `Encoder` (default) or `TypeAdapter.dump_*`; exit artifact: `bytes` wire.

# Ingress Carriers And Admission

- Carriers: `bytes` (wire), `str` (text wire), `dict[str, object]` (parsed host), `Mapping[str, object]` (ORM views), `TypedDict` instances (typed dict views).
- Raw ingress stays at the adapter until validation produces a typed intermediate or model.
- `bytes`/`str` route to `TypeAdapter.validate_json` (Pydantic) or `msgspec.json.decode`/`msgspec.msgpack.decode` (Struct targets) — never `json.loads` then second-pass validate.
- `dict` ingress routes to `TypeAdapter.validate_python` or `msgspec.convert`.
- `TypedDict` ingress is valid only inside adapter signatures and pre-validation staging; it must not cross into domain modules as the durable shape.
- `Unpack[TypedDict]` (PEP 692) types keyword ingress at call boundaries; unpack into validation immediately inside the adapter body.
- `context=` on Pydantic validators carries boundary metadata (source id, transport id, actor) — not domain state.

# Validation Owners

- Pydantic owns ingress validation for external/untrusted shapes: `BaseModel(frozen=True)`, discriminated unions, `RootModel`, settings, and non-Struct targets.
- msgspec owns validation when the target is already a `Struct` and the bytes path is trusted-internal or performance-critical.
- Module-level `TypeAdapter[T]` singleton — never per-request instantiation — is the validation owner for unions, `TypedDict`, `Annotated` scalars, and containers when `T` is not a `BaseModel` subclass.
- Core methods: `validate_json`, `validate_python`, `validate_strings`, `dump_json`, `dump_python`, `json_schema`, `.validator`.
- `validate_json(data: str | bytes | bytearray)` parses and validates in one pass through pydantic-core — replaces `json.loads` + `validate_python`.
- `validate_python(data: object)` validates already-materialized Python objects — use for dict/ORM ingress, not for wire bytes.
- `ConfigDict(strict=True)` rejects silent coercion at the trust boundary.
- msgspec validates on `decode`/`convert` when `type=` is supplied; direct `Struct(...)` construction does not re-check annotations.
- `msgspec.ValidationError`/`DecodeError` and `pydantic.ValidationError` map inside `capture` — never propagate raw across domain.
- `forbid_unknown_fields=True` rejects unknown keys at decode and on direct construction (extra kwargs raise `TypeError`).
- `beartype.beartype` decorates boundary callables after validation materializes typed values — not on raw ingress carriers.
- `pydantic-settings` `BaseSettings(frozen=True)` materializes config once at bootstrap via `capture("settings")` — replaces `os.getenv`/`os.environ` reads.
- `validate_call` decorates boundary callables accepting already-validated domain types at the function edge.
- `mypy` plugin `pydantic.mypy` with `init_typed`, `init_forbid_extra`, `warn_required_dynamic_aliases`, `warn_untyped_fields` — TypeAdapter targets must remain fully typed.
- `experimental_allow_partial` on `validate_json`/`validate_python`/`validate_strings` only — not on `BaseModel` constructors; values: `False`/`'off'`, `True`/`'on'`, `'trailing-strings'` for stream/LLM ingress.
- Experimental Pipeline API (Pydantic ≥2.8) composes `Annotated` validation transforms with explicit ordering — treat as unstable; prefer `Annotated` + `BeforeValidator`/`AfterValidator` for production ingress.

# Normalization And Wire Reshape

- Normalization reshapes wire keys, units, and envelopes without asserting domain invariants.
- Pydantic `model_validator(mode="before")` owns wire-shape normalization: alias collapse, key rename, envelope unwrap, null-sentinel removal.
- `BeforeValidator`/`AfterValidator` on `Annotated` fields own scalar normalization: strip, case-fold, path anchor, timezone attach, enum wire-token → literal.
- `AliasChoices`/`AliasPath`/`validation_alias`/`serialization_alias` own field-name mapping — not manual dict key pops.
- `model_validator(mode="after")` owns cross-field ingress invariants at the boundary — raise `ValueError` per Pydantic contract.
- msgspec `dec_hook`/`enc_hook` dispatch tables own wire↔Python type normalization — raise `NotImplementedError` for unknown types.
- Normalization never imports domain services, never reads mutable global state, never performs I/O.

# Construction And Tagged Unions

- Construction produces the first durable typed instance from normalized ingress.
- Pydantic `BaseModel` construction completes at validator pipeline end; msgspec `Struct` construction completes at `decode`/`convert` completion; both use `frozen=True`.
- `Discriminator("field")` resolves union members when every variant exposes the same `Literal` discriminant — callable discriminators and `Tag()` only when discriminant requires computation.
- Single-inheritance chains for Pydantic bases sharing `model_config` — `ConfigDict` merges via MRO without warning on multi-base conflicts.
- Tagged unions on wire use msgspec `tag_field` + per-subclass `tag=` — not parallel stringly routing tables.
- Smart constructors on rich class owners return `Result[T, E]` — invalid combinations never raise inside domain.
- Enum token admission on materialized fields closes at construction with closed vocabulary sentinel encoding.
- Variant discriminant on owner resolves through tagged union arm at construction → enrichment `match`.

# Enrichment And Derived Slots

- Enrichment adds derived slots, evidence, correlation, and projections ingress did not carry.
- Enrichment runs after construction on immutable inputs — output is a new immutable instance via `model_copy(update=...)`, `msgspec.structs.replace`, or rich-owner factory.
- `computed_field` and owner-local derived properties belong on the shape that owns the projection — not on ingress DTOs.
- `msgspec.structs.replace(struct, **changes)` is the Struct transition primitive; `__post_init__` runs on replace (msgspec 0.21+).
- Enrichment needing external reads stays in boundary adapters or `@effect.async_result` orchestration — not inside validators.
- Domain enrichment folds use `match/case` on discriminant — zero imperative branching in transform bodies.
- Owner successor transitions without re-validation use immutable replacement primitives (`model_copy`, `structs.replace`, `copy.replace`).

# Immutable Materialization

- Materialization terminus: frozen, hashable (when needed), typed owner ready for domain logic and serialization.
- Pydantic uses immutable collections on frozen models; msgspec uses `Struct(frozen=True, gc=False)` for wire-hot shapes; `omit_defaults=True` + `repr_omit_defaults=True` for compact deterministic wire.
- Rich class owners (dataclass `frozen=True, slots=True` or dense owner class) materialize when behavior, invariants, and multi-step lifecycle exceed declarative field lists.
- One canonical materialized type per concept — projections derive via `model_copy`, `structs.replace`, `msgspec.convert`, or owner factories.
- `validate_detail`-style round-trip guard: encode → decode through module-level codec proves the value still satisfies the wire union before persistence.

# Outbound Serialization And Wire Codec

- msgspec owns outbound wire encoding: `msgspec.json.encode`, `msgspec.msgpack.encode`, module-level `Encoder`/`Decoder`.
- `order="deterministic"` when byte-stable output is required. `msgspec.msgpack` for internal service-to-service egress; JSON for external human/API consumers unless contract specifies otherwise.
- Pydantic `TypeAdapter.dump_*` for egress only when the outbound contract is Pydantic-owned (OpenAPI export, settings snapshot) — not the default hot path.
- `model_serializer(mode="wrap")` adds envelope fields (version, trace) at Pydantic egress boundary.
- `msgspec.Struct` is the admitted zero-copy wire and cache shape for internal deterministic JSON/msgpack.
- Field constraints via `Annotated[T, msgspec.Meta(...)]`; `cache_hash=True` for hot-path dict keys; `gc=False` for high-volume wire rows; `forbid_unknown_fields` matches validation-owner policy on wire structs.
- `WireEnvelope` + `msgspec.Raw` pattern: parse envelope metadata first, decode payload with version-selected Struct type second.
- `msgspec.convert(source_struct, target_struct)` for version/schema coercion — no hand-rolled field copy loops.
- Module-level `msgspec.json.Decoder[T]` for hot decode paths.
- `__post_init__` runs after `decode`, `convert`, and `structs.replace`; faults become `msgspec.ValidationError` on decode/convert paths.
- `msgspec.inspect` types (`StructType`, etc.) support schema introspection without Pydantic dependency.
- Domain modules do not import msgspec encoders or Pydantic dump APIs — serialization adapters live at package boundary. Egress from domain to wire is lossy only at declared boundaries.
- `wire_safe` scrubs lone surrogates (PEP 383 `surrogateescape` from invalid-UTF-8 argv) to U+FFFD before encode — scrubbing is an egress adapter obligation, not a domain string cleanup helper.
- Residual `UnicodeEncodeError` after scrubbing folds to a minimal fault envelope with scrubbed `notes` — `_encode` never leaks partial bytes or truncated UTF-8 sequences on stdout.
- Ingress text that required scrubbing does not re-enter validation — scrubbed strings are wire-safe artifacts only; re-ingress of scrubbed values uses the normal validation owner if business logic requires re-admission.

# TypedDict Structural Contracts

- `TypedDict` is a static structural contract for dict-shaped ingress and `**kwargs` bundles — not a runtime validation or behavior owner.
- PEP 728 (Python 3.15): declare openness explicitly with `closed=True` or `extra_items=Type`; runtime error if `closed=True` and `extra_items=` appear in the same definition.
- `Required`/`NotRequired` (PEP 655) and `ReadOnly` (PEP 705) mark per-key semantics — no mirror total/non-total pairs.
- Payloads remain payloads through validation; `TypeAdapter[TypedDict]` promotes them to validated views, then construction promotes to model/Struct.
- Do not add methods, validators, or computed fields to `TypedDict` — escalate to `BaseModel` or `Struct`.

# Rich Class Owner Escalation

- Escalate from declarative model when the shape carries dispatch tables, folds, evidence receipts, enum behavior, or multi-port lifecycle.
- Rich owners use `frozen=True` dataclass with `slots=True` or a single dense class block — behavior co-located with fields, no helper module extraction.
- Ingress never constructs rich owners directly from raw dicts — adapter validates, then owner smart constructor returns `Result[Owner, E]`.
- Rich owners never import wire codecs; they expose canonical immutable snapshots for boundary projection (`to_wire()` or adapter-owned `msgspec.convert`).
- `Protocol` ports define what enrichers and services need — rich owner implements structural contract without `ABC`.

# Boundary Adapters And Pipeline Algebra

- Adapters are the sole `try`/`except` sites: `capture`/`async_capture` wrap validation, decode, and transport faults.
- Adapter stack order (outer → inner): trace → authorize → validate → cache → govern → retry → operation.
- One polymorphic ingress adapter per transport — parameterized on target `TypeAdapter` or `Decoder`, not per-entity copies.
- `ingest(adapter, transform, project, encoder)` pattern: `validate_json` → `result.bind(domain_transform)` → `result.map(project)` → `encoder.encode`.
- Boundary pipelines compose as `pipe(ingress, validate, bind(domain_transform), map(project), encode)` — no nested `try` beyond `capture`/`async_capture` shells.
- `result.bind` chains fallible transitions; `result.map` chains total projections on success values.
- `sequence`/`traverse` over ingress row collections materialize per-row at the boundary, then fold — domain folds receive tuples of canonical owners, not parallel dict lists.
- Layer stacks wrap the orchestration root; validation layer owns `TypeAdapter`/`Decoder`; operation layer receives materialized owners only.
- Async materialization uses `async_capture` at the same boundary sites as sync `capture`; domain transforms between await points remain synchronous on immutable inputs.
- Boundary exemption metadata (`NOESIS_BOUNDARY_EXEMPTION`) only where statement-form control is unavoidable inside adapter shells.
- Ingress pydantic hook stacks preserve decorated adapter callable signatures at the validation outer shell.

# Payload Versus Model Escalation

- Stay payload: ephemeral dict/TypedDict/blob not referenced beyond adapter exit; logging context; pre-validation staging; passthrough proxy views.
- Must become model: repeated field bundle; shared between modules; validation constraints; public API signatures; artifact/cache storage.
- Model must become richer owner: ≥3 variant dispatch arms; algorithm-specific evidence slots; fold-derived status/count semantics; registry/catalog row behavior; dispatch tables, folds, enum behavior, or multi-port lifecycle.
- De-escalation is forbidden — collapse parallel types into polymorphic owner instead.
- `NamedTuple`, `collections.namedtuple`, stdlib `json`, and bare `dict[str, Any]` at boundaries are banned replacements — use `Struct` or `BaseModel(frozen=True)`.
- Boundary payload promotion before construction gate routes through typed payload admission at the adapter.

# Stage Handoff Artifact Law

- Each stage emits exactly one handoff artifact type; the next stage accepts only that artifact or an explicit adapter product — never the original ingress carrier.
- Transition contracts are named in type signatures — `bytes` → validated `Struct`, validated `Struct` → enriched `Owner`, enriched `Owner` → deterministic `bytes`. Erased `object` handoffs between stages are rejected.
- Stage-skipping (ingress dict → domain owner without validation owner, or domain owner → wire bytes without projection owner) requires a documented exemption at the composition root — not ad hoc convenience in leaf modules.
- Ingress exit: `bytes`, `str`, or adapter-scoped `Mapping`/`TypedDict` view.
- Validation exit: frozen `BaseModel`, `Struct`, or `TypeAdapter`-validated scalar/container.
- Normalization exit: same runtime type with wire keys and envelopes collapsed to canonical field storage.
- Construction exit: first immutable instance with full field closure — smart constructors may wrap as `Result[Owner, E]`.
- Enrichment exit: successor immutable instance with derived slots filled.
- Materialization exit: canonical domain owner or terminal frozen model — the only type domain modules accept without adapter re-entry.
- Serialization exit: `bytes` or adapter-owned text.
- Cross-family handoff (Pydantic ingress model → msgspec wire struct) routes through one projection expression: `msgspec.convert`, owner `to_wire()`, or adapter-owned field table — never `model_dump` → dict surgery → `Struct(...)`.

# Trust Posture Per Stage

- Untrusted: all external wire, CLI argv, queue messages, uploaded files, and cache bytes without integrity proof — full validation and strict coercion policy required.
- Semi-trusted: internal service payloads with transport auth but without content-hash proof — validate once at ingress; `forbid_unknown_fields` or `extra="forbid"` stays on.
- Trusted-internal: same-process struct graphs already validated in-session, or bytes read from a store immediately after `wire_encode` by the same codec module — may use module-level `Decoder` without Pydantic; still map decode faults at the boundary.
- Trusted-replay: rehydration from probe stores, artifact caches, or snapshot files written by the owning encoder — `model_construct` or trusted `convert` permitted only when store key, schema version, and encoder identity are pinned in the composition root.
- Never trusted: `json.loads` output, ORM `__dict__` views, `MappingProxyType` over live backing maps, partial LLM token buffers without `experimental_allow_partial` admission — each re-enters at validation regardless of upstream label.

# Stage-Tagged Fault Surfaces

- Validation faults carry engine identity: `pydantic.ValidationError` → admission fault with field paths; `msgspec.ValidationError`/`DecodeError` → wire fault with type context.
- Normalization faults are `ValueError` inside Pydantic validators — caught at validation stage, not re-tagged in domain.
- Construction faults from smart constructors are typed `E` in `Result[Owner, E]` — never bare exceptions past the constructor gate.
- Enrichment faults return `Result` or typed family errors — no validator re-entry to express domain law.
- Serialization faults (`UnicodeEncodeError`, surrogate rejection) fold to scrubbed minimal wire envelopes at the egress adapter — holding single-envelope contracts without leaking partial bytes.

# Envelope Decode And Materialization

- Versioned wire envelopes decode in two passes when body type depends on envelope metadata: pass one materializes closed envelope struct (`schema_version`, `claim`, `verb`, status fields); pass two selects body `Decoder` from version + discriminant.
- `schema_version` is a closed literal — unknown versions fail at pass one without attempting body decode.
- `msgspec.Raw` holds unparsed body bytes between passes when the envelope parser must not eagerly decode polymorphic payload.
- Success and fault bodies share one envelope type with nullable slots (`report`, `error`) — mutual exclusivity enforced in `model_validator(mode="after")` or `__post_init__`, not runtime `if` ladders in domain.
- Egress re-wraps domain outcomes into the same envelope constructor — `envelope(report=..., fault=...)` — so pass-one shape is identical for success and failure lines on the wire.
- Pipeline exit requires a closed envelope struct before serialization-stage encode.
- `envelope(payload, claim=..., verb=..., run_id=...)` is the single polymorphic constructor — `match` on `Report | Fault` selects success versus error slots without parallel envelope types.
- Root `envelope(...)` is the only constructor that may populate both `claim` and `verb` from bind metadata — rehydrated envelopes decode into the same struct the root emitted.
- Wire caps derive from struct field metadata via `msgspec.inspect` (`field_cap`) — `_HINT_CAP`, `_MESSAGE_CAP`, `_RESULT_CAP`, `_ARTIFACT_CAP` read from `Meta(max_length=...)` on the owning struct, not magic constants duplicated in adapters.
- Hint truncation reserves framing space for parse diagnostics — surplus-token clips subtract a fixed framing budget from `_HINT_CAP` so positional overflow does not sever the diagnostic suffix on the wire.
- When any cap applies, the root sets `truncated=True` on the envelope via `structs.replace` — consumers must treat truncated lines as summaries; full payloads live in history artifacts or full-report stores keyed by `run_id`.
- Defect text tails, artifact lists, and result rows cap before encode — overflow routes to persisted artifacts named in `Report.artifacts`, not to elongated stdout lines.
- Cap application is lossy only at declared fields — computed slots and envelope metadata outside capped tuples remain intact; do not cap by `model_dump` truncation or string slicing on encoded JSON.

# Projection Families

- Ingress projection family: Pydantic frozen models, settings, and `TypeAdapter` targets — JSON Schema-rich, env-backed, discriminated union admission.
- Domain projection family: rich class owners, frozen dataclasses, and closed variant families — behavior, folds, and `Result` transitions live here.
- Wire projection family: msgspec `Struct` rows with `tag_field`, `gc=False`, deterministic encode — no domain methods on wire types.
- Egress projection from domain to wire is lossy only at declared boundaries: fields omitted via `omit_defaults`, computed slots stripped, or explicit adapter tables — not accidental `model_dump` field drift.
- One discriminant vocabulary spans families — literal tags, `StrEnum` values, and msgspec `tag` strings are declared once on the owner; adapters map external synonyms at ingress and egress only.
- Interior modules import one family per concern. Domain owners do not import wire struct modules for behavior; wire structs do not import pydantic for validation.

# Cross-Axis Seam Map

- Foreign bytes → validated struct: wire carrier; ingress adapter → `TypeAdapter` / `Decoder`; materialization axis owns edge.
- Validated struct → canonical owner: frozen model or `Struct`; smart constructor `Result` gate; rich class owner axis.
- Canonical owner → wire struct: domain snapshot; `msgspec.convert` / owner `to_wire()`; wire egress projection.
- Patch dict → successor owner: boundary payload; `model_validate(snapshot | delta)`; typed payload axis.
- Prior owner → enriched owner: immutable instance; `copy.replace` / `model_copy` / `structs.replace`; immutable replacement axis.
- Enum token on materialized field: `StrEnum` member; construction after vocabulary admission; vocabulary absence axis.
- Variant discriminant on owner: tagged union arm; construction → enrichment `match`; class family variant axis.
- Ingress pydantic hook stack: decorated adapter callable; validation stage outer shell; decorator admitted shape axis.
- Structural port at handler edge: materialized owner; post-`beartype` handler bind; protocols capabilities axis.
- Settings → frozen boot record: env slice; root `capture("settings")` bootstrap; composition root.
- Stored bytes → current wire struct: versioned egress row; `msgspec.convert` migration hop; evolution axis.
- Fault → envelope line: `Result` rail outcome; root `_emit` distillation; composition root.
- Route each edge through the stage or projection entry — interior domain folds receive materialized owners only; ingress carriers and staging dicts stop at named adapter or root guard symbols.
- Chained pipelines compose as typed stage transitions: validate → normalize (codec-internal) → construct → enrich → materialize → project wire → encode — no step widens to erased `object` between enumerated handoffs without a documented exemption at the composition root.
- Cross-family projection (Pydantic ingress model → msgspec wire struct) executes in one boundary expression per concept — seam map rows do not chain `model_dump` dict surgery through domain modules.

# Round-Trip Proof And Persistence Gates

- Round-trip proof encodes a materialized value through the module-level wire codec and decodes back to the same type before persistence, cache write, or cross-process handoff when the nested shape is a tagged union or polymorphic detail slot.
- Proof form: `decoder.decode(encoder.encode(value))` with equality on the decoded value — the `validate_detail` pattern for `AnyDetail | None` tagged unions.
- Proof is not a substitute for ingress validation — it witnesses that a once-validated interior value still satisfies the wire union before storage, catching drift from enrichment bugs, manual field assembly, or discriminator typos.
- Proof failure maps to boundary `Fault` or `Result` error — never silent coercion to a default variant or `None` unless the slot is explicitly optional and the proof contract says so.
- Deterministic encoders (`order="deterministic"`) make proof outputs stable across runs — required when proof bytes become cache keys or content hashes.
- Tagged union detail slots: proof required before artifact persist, registry emit, or subprocess wire handoff.
- Closed struct rows without interior unions: proof optional when type is monomorphic and constructed only through decode or single factory.
- Fault outcomes skip detail proof.

# Stage-Attributed Proof Layers

- Proof harness layers stack orthogonally on the materialization axis: static stage-map exhaustiveness, import architecture (domain never imports codecs), contract tables (handoff artifact types per transition row), metamorphic round-trip on polymorphic slots, and runtime `beartype` on adapters after validation exit.
- Harness execution order: static checkers and import rules before transition contract tables before metamorphic properties before root integration smoke — failures at static layer block expensive generative runs.
- Each transition row maps to at least one parametrized proof obligation — missing row coverage is a merge blocker when the transition is admitted in production adapters.
- Stage tags on boundary faults (`strict:`, `validation:`, `config:`, `parse:`) are harness attribution keys — smoke tests inject violations at each gate and assert faults surface with the expected prefix and stage owner, not undifferentiated messages.
- Proof debt from checker gaps tracks on the language axis — source declarations stay spec-complete; harness suppressions or `cast` escapes at materialization seams are rejected.

# Metamorphic Law Registry

- Round-trip laws bind to module-level codec symbols the composition root exports — `wire_encode`, `validate_detail`, `_ENVELOPE_DECODER`, and detail decoders alias the same singletons in production and conftest; shadow encoders in test helpers fail reference-identity checks.
- `@spec` and `register_law` associate metamorphic witnesses with root codec functions or materialization owners — laws register on the integration owner symbol at decoration time; conftest `register_law` hoists only laws that cannot self-register; proof tests import root symbols under test, not shadow encoders.
- Round-trip proof functions are metamorphic targets: for all lawful values in the generator domain, `proof(value) == value` or agrees with the reference encoder on bytes.
- Reference encoder identity: tests assert `wire_encode(value)` agrees with the conftest `WIRE_ENCODER.encode` — production and test encoders alias the same module-level singleton.
- Hypothesis strategies draw from closed variant families admitted by the root decoder — proof failures implicate enrichment or discriminator wiring at the root, not domain leaf modules.
- Proof skips on fault paths are explicit in guard code (`_validated` matches `Error` and returns unchanged) — tests cover success proof and fault passthrough independently.
- Full pipeline metamorphic law when bijection holds: materialized canonical → wire projection → encode → decode → rematerialize → assert equality on canonical identity — failures implicate adapter binding or enrichment wiring before vocabulary or interior folds.
- Subset bijection tests declare explicitly which fields round-trip — computed-on-materialize, wire-omitted, and cap-truncated slots exclude from equality assertion with documented rationale beside the law module.
- Polymorphic detail slots require decode-after-encode proof before persist paths — law draws from closed variant families admitted by `_DETAIL_DECODER`; fault-path laws assert proof is skipped explicitly on `Error` outcomes matching `_validated` guard behavior.
- Encoder determinism laws pin `order="deterministic"` on module-level `Encoder` singletons — metamorphic bytes equality and probe cache key laws use the production encoder instance, not per-test construction.
- Shrinking preserves discriminant legality — hypothesis composites rebuild valid materialized values after shrink; invalid tag mutations fail at construction gate, not as accepted counterexamples.

# Transition Contract Proof Tables

- Handoff artifact types live beside root codec modules as parametrized contract rows — upstream type, downstream type, re-validate policy, round-trip proof required flag; adapters and tests read the same table; prose transition lists are rejected.
- Ingress → validation rows assert single-pass semantics — `validate_json` and tagged `Decoder.decode` samples; negative fixtures with `json.loads` intermediate are harness failures, not documentation-only anti-patterns.
- Trusted-replay rows pin store key, schema version, and encoder module identity — rehydration tests decode bytes written by production `wire_encode` through root decoders without alternate dump paths.
- Cross-family projection rows prove one-expression convert — Pydantic validated instance → msgspec wire struct via `msgspec.convert` or adapter-owned projection; chained dump-and-reconstruct fixtures are negative cases.
- Partial stream rows isolate `experimental_allow_partial` admission at validation exit only — downstream construction laws use partial fixtures that document intentional incomplete closure; mixing partial validation with full domain invariant laws is a negative module.

# Rehydration And Cache Admission

- Cache read path: `bytes` from store → module-level `Decoder` → materialized struct — no `json.loads` intermediate.
- Cache write path: materialized struct → round-trip proof (when union slot) → `wire_encode` → store. Write and read share one encoder/decoder pair pinned at module level.
- Probe and fingerprint stores key on deterministic encode output — encoder `order="deterministic"` is part of the cache contract.
- Rehydration of pydantic models from trusted snapshots uses `model_validate` when snapshot may include computed or aliased keys; `model_construct` only when snapshot bytes were written by `model_dump(mode="python")` from the same model in the same process session.
- Version migration on read: `msgspec.convert(old_struct, new_struct)` at the adapter — one conversion per version step, not chained hand-rolled copies.
- Merged cache rows (`{**retained, **fresh}`) materialize as new dict only inside boundary persistence helpers — immediately encoded to bytes; domain never operates on the merged dict.
- `store.load_history(run_id)` returns bytes decoded by `_ENVELOPE_DECODER` — no `json.loads` intermediate.
- `RunSnapshot` materializes from decoded history envelopes — comparison fields are immutable struct scalars (`id`, `status`, `counts`), not dict projections of wire JSON.
- `RunDelta` detail reports `before`/`after` snapshots plus `added`/`removed` key counts — missing history sides fold to `EMPTY` with explicit notes; consumers surface absence in notes rather than silently zeroing counts.
- Delta consumers never merge snapshot dicts — key sets derive from struct fields on materialized snapshots; merged staging dicts exist only inside boundary persistence helpers immediately before re-encode.

# Async And Streaming Ingress

- `experimental_allow_partial` on `TypeAdapter.validate_json`/`validate_python`/`validate_strings` admits incomplete documents at streaming and LLM ingress — values: off, on, `trailing-strings` for incremental token buffers.
- Partial admission stops at validation exit — downstream construction assumes the admitted partial shape is intentional; never mix partial validation with frozen domain invariants that require full closure.
- Streaming byte feeds buffer to `bytes` completion at the adapter before decode unless the codec documents incremental decode — msgspec decoders are not incremental; complete frames first.
- `async_capture` wraps awaitable transport reads that return raw carriers; validation and decode remain sync on completed buffers inside the async boundary shell.

# Composition Root Integration

- One composition-root module per bounded context owns REGISTRY rows, module-level `Encoder`/`Decoder` pairs, egress guards, and history persistence — leaf domain modules import canonical owners only.
- Codec singletons (`_ENCODER`, `_DETAIL_DECODER`, `_ENVELOPE_DECODER`) live in the root or its declared codec submodule; hot paths never allocate per-request codecs.
- Synthetic fault prefixes (`strict:`, `validation:`, `config:`, `parse:`) are formatted exclusively inside root guards — domain `Fault` and `Result` errors carry structured payloads, not transport-prefixed strings.
- Invocation-scoped context (`ContextVar` rings, write counters, lazy resource snapshots) resets in a `finally` block around every root entry — reused runners must not leak prior invocation evidence.
- Root exports stabilize at `__all__` boundary symbols: envelope constructors, wire encode/decode, proof helpers, and registry lookup — not interior guard names unless test seams require them.
- `REGISTRY` is a frozen tuple of `Bind` rows: each row pins `claim`, `verb`, `handler`, `params_type`, and help text — one polymorphic ingress surface per transport family, not per entity clone.
- `rail(bind)` closes over settings, layer stack, and scope opener; returned callable is the sole executable entry for that verb.
- `_bound(params, claim, verb)` promotes raw CLI/host params through `BaseParams.bound(verb)` — surplus tokens and arity faults seed the dispatch ring and return `Error(Fault)` before validation runs.
- Params that already satisfy the bound type pass through `_bound` as `Ok(params)` — re-validation at the root is limited to beartype on the handler edge and codec proof on egress.
- `parse_fault` is the alternate ingress path for pre-validation failures — it materializes the same `Envelope` shape as operational faults without entering the handler pipeline.
- Report layers compose outer-to-inner via `compose(*_RAIL_LAYERS)(_narrow(handler))` — checked, logged, traced slots wrap the handler before `_guard` sees the thunk.
- `_narrow` rejects non-`FunctionType` handlers at registry build time — erased callables do not reach the materialization pipeline.
- Layer correlation attrs (`run_id`, `strict`, agent context) project from `AssaySettings` and bound params — trace spans inherit the same keys as log events without duplicating domain fields.
- Spawn/retry/govern layers belong on effectful transports only — in-process report rails omit retry layers when the handler is pure on immutable owners.
- Canonical success path: `_guard(lambda: _bound(...).bind(lambda p: _validated(_strict(handler(..., p), p))))` — each guard is a total function on `Result`; no nested `try` inside `bind` bodies.
- `_strict` promotes policy faults when `--strict` admits only non-empty success folds — empty/skip `Report` statuses become `strict:` faults without re-entering validation.
- `_validated` runs round-trip proof on success `Report.detail` only — `Error` outcomes skip proof because fault rails already exclude invalid success interior shapes.
- `_emit` folds `Result[Report, Fault]` into one `Envelope`, applies cap truncation via `structs.replace`, and routes `FAILED` reports through the same `Diagnostic` distillation as hard faults.
- Per-invocation one-write guard: a second `Envelope` emission during the same `rail()` call is suppressed and replaced with a `FAULTED` invariant fault on stderr — wire contract forbids duplicate stdout lines; tests and CI parsers assert the invariant independently of handler success.
- Success and fault lines share one struct closure — cap and truncation fields apply identically on success and fault paths before `_encode`.
- History retention and full-report artifact writes are best-effort `OSError` paths logged at warning — persistence failure does not suppress the stdout wire line already emitted.
- `delta` and `load_history` rehydrate through `_ENVELOPE_DECODER` — trusted-replay posture: bytes written by the root encoder in a prior session, keyed by `run_id`.
- `fold(claim, verb, outcomes, detail=...)` is the terminal materialization for multi-receipt rails — status, counts, defects, and artifacts reduce from `Completed` rows without mutable accumulation.
- Interior `detail` on folded `Report` is optional — when present and polymorphic, it undergoes `_validated` proof before envelope emission.
- Truncation on fold output applies at envelope construction — defect text tails and artifact lists cap before wire encode, with overflow routed to full-report artifacts.
- `AssaySettings` (or context-equivalent `BaseSettings(frozen=True)`) materializes once per invocation via `capture("settings")` at the root — `parse_fault` falls back to `model_construct()` only when settings validation itself fails, tagging `config:` faults.
- Settings feed correlation attrs and store backends — domain handlers receive the frozen settings instance, not raw environment reads.
- `artifact_retention`, `run_id`, and store roots are settings-owned — composition root does not read `os.environ` for paths bypassing settings.
- Stage seams at the root boundary: validation exit supplies params types consumed by bind promotion; handler execution runs on materialization-exit owners between enrichment closure and serialization projection; round-trip proof applies to polymorphic detail slots after materialization and before wire encode; serialization exit is the only artifact history and stdout consumers read.
- Leaf domain modules import canonical owners and return structured outcomes — they do not import root guard symbols, module-level codecs, or envelope emit helpers.
- Params bind (`_bound`): consumes CLI/Cyclopts staging; emits bound params or parse `Fault`.
- Handler exec (composed layers + handler): consumes materialized params + scope; emits `Result[Report, Fault]`.
- Strict policy (`_strict`): consumes success `Report`; emits promoted or passthrough `Result`.
- Detail proof (`_validated`): consumes ok `Report` with detail slot; emits unchanged `Result` or proof fault.
- Fault capture (`_guard`): consumes thunk; emits uniform `Result`.
- Envelope fold (`_emit`): consumes `Result`; emits `Envelope` with caps applied.
- Wire encode (`_encode` / `wire_encode`): consumes `Envelope` or wire struct; emits deterministic `bytes`.
- Stdout emit (`_emit_envelope`): consumes `Envelope`; emits persisted + printed bytes.
- History read (`_ENVELOPE_DECODER`): consumes stored `bytes`; emits `Envelope` for delta/replay.
- Registry dispatch (`rail(bind)`): consumes raw `params`; emits terminal `Envelope`.
- One bounded context pins `_ENCODER`, envelope decoders, and detail decoders at root or declared codec submodule — import-linter rules flag handler-local `Encoder()` or `TypeAdapter()` construction.
- Root `__all__` exports stabilize wire encode/decode, proof helpers, and envelope constructors — interior modules import canonical owners; they do not re-export codec singletons under alternate names.
- History persist and stdout emit share `_encode` — proof harness asserts `store.write_history` bytes equal live emit bytes for the same envelope value; dual-encoding fixtures fail CI.
- Merged cache rows prove immediate re-encode — boundary persistence helpers that merge dict staging encode to bytes in the same function; domain modules never appear in merge proof modules.
- Worker and subprocess replay import root codec module path pinned at composition root — renaming codec submodules breaks replay even when struct layouts are unchanged; harness includes module identity smoke beside layout proofs.

# Failure Archaeology Routing

- Failure attribution follows stage-first routing: `pydantic.ValidationError` → validation stage owner and ingress adapter; `msgspec.ValidationError` / `DecodeError` → wire decode owner; smart constructor `Result` error → construction gate owner; proof failure in `_validated` → enrichment or discriminator wiring at root; `BeartypeCallHintViolation` → handler edge after materialization exit.
- Duplicate-validation failures surface when the same invariant message appears from Pydantic and smart constructor — harness runs duplicate-validation checklist per concept; attribution targets secondary owner for removal.
- Stage-skipping failures attribute to composition root exemption registry — ingress dict reaching domain owner without validation owner is a root wiring defect, not a leaf handler bug.
- Seam remap failures attribute to anti-corruption adapter — foreign layout in fault payload maps to migration or ingress remap table row; domain `match` on foreign wire keys indicates context leak past materialization exit.
- Migration failures attribute to read-boundary convert fold — domain modules after successful materialization never emit version or layout faults.

# CI Enforcement And Drift Gates

- Adding a pipeline stage owner without updating stage-map proof table and import architecture rules fails merge — orphan adapters bypassing named stage owners block CI.
- Adding a polymorphic wire slot without round-trip law, `_DETAIL_DECODER` arm, and exhaustive `match` update fails static and metamorphic gates together.
- Ruff banned-api and custom analyzer rules flag `json.loads`, per-request `TypeAdapter`, domain imports of `msgspec`/`pydantic`, and `TypeGuard`/`cast` on materialization modules — lint runs before behavioral suites.
- OpenAPI and ingress schema diff tests run on pydantic-owned ingress changes — wire-only tag changes require egress struct and migration row updates in the same promotion unit.
- Registry rebuild at import proves handler map coverage — CI jobs importing composition root fail when new ingress type lacks adapter route or proof row.
- Mutation testing targets adapter `materialize_*`, projection folds, and remap tables when seam logic is dense — interior domain folds are secondary mutation surface.

# Integration Smoke And Consumer Alignment

- Root integration smoke asserts one-write invariant, deterministic stdout line shape, and envelope decode parity with `_ENVELOPE_DECODER` — consumer contract tests reference the same smoke fixtures, not duplicated parser logic.
- Injected ingress violations surface at validation owner — injected wire decode failures at decode owner — injected proof failures at `_validated` guard — conflated attribution fails the smoke module.
- Subprocess parent fold smoke decodes child stdout through root envelope decoder when child is an admitted rail — foreign tool bytes stay opaque unless envelope contract is adopted.
- Cap truncation smoke asserts `truncated=True` implies artifact spill paths exist — full evidence resolves through history keyed by `run_id`, not elongated wire tuples.

# Collapse Tests

- Per-request codec: handler or adapter constructs fresh `TypeAdapter` or `Encoder`; collapse to module-level singletons pinned at composition root.
- Stage skip: domain module validates raw dict or calls `json.loads`; collapse to boundary adapter with named validation owner and single-pass decode.
- Dual dump persist: history stores `model_dump` JSON while stdout emits msgspec bytes; collapse to single `_encode` path for both surfaces.
- Proof in domain: interior fold calls `validate_detail` or decode-after-encode; collapse to root `_validated` egress guard only.
- Parallel concept schemas: `FooDTO` + `FooModel` + `FooStruct` for one entity; collapse to one canonical materialized owner with projection folds.
- Dict handoff between stages: enrichment passes `dict[str, object]` between modules; collapse to immutable owner transitions via `model_copy`, `structs.replace`, or smart constructor.
- Consumer json.loads: automation parses stdout with loose dict views; collapse to `_ENVELOPE_DECODER` and struct field access.
- Version default ingress: replay decodes unknown `schema_version` into current struct; collapse to pass-one literal failure and migration fold at read boundary.
- Done when every cross-axis handoff in the seam map routes through a named stage or projection entry, every polymorphic slot with persist obligation has a registered metamorphic law on root codec symbols, CI gates fail on stage skip and codec drift before handlers execute, and collapse tests eliminate parallel encodings and interior proof duplication.

# Schema Version Evolution

- Version evolution is egress-first — new fields and tags land on msgspec wire structs and migration folds; ingress Pydantic models update on independent schedule unless public API and wire are intentionally unified.
- `schema_version` is a closed `Literal` on the envelope struct — unknown versions fail at pass-one decode without attempting body materialization; never treat version as a free `int` with runtime defaulting.
- Each version bump adds one migration fold at the read boundary: `msgspec.convert(stored_v_n, current_struct)` — chain at most one conversion per read; multi-hop history replays step through a pinned migration table declared at the composition root, not ad hoc copies in leaf modules.
- Breaking wire changes require a new `schema_version` literal and a migration row — field renames, discriminant vocabulary changes, and envelope slot moves are version events, not silent `BeforeValidator` patches on live structs.
- Obsolete variant arms survive only inside migration folds with exhaustive `match` and `assert_never` on the current vocabulary — domain modules after migration see only the current owner; retired tags do not remain in active `match` arms.
- Ingress Pydantic models and egress msgspec structs may diverge by version arm — OpenAPI and public schema reflect ingress; storage and stdout wire reflect egress; migration folds bridge stored egress to current egress, not ingress replay.
- Settings snapshots that feed cache keys round-trip through `model_dump` + `TypeAdapter.validate_python` at bootstrap when settings schema evolves — settings version is independent of envelope `schema_version` unless the composition root explicitly couples them.
- Probe and fingerprint stores pin encoder identity beside `schema_version` — changing either invalidates prior keys without a documented re-key or migration pass; deterministic `order="deterministic"` is part of the version contract.

# Diagnostic Distillation And Consumer Contracts

- Operational faults (`Fault`) and `FAILED` report defect rows distill into one `Diagnostic` wire detail — same shape, same cap policy, same truncation spill rules; domain modules emit structured `Fault`/`Report`, not pre-formatted diagnostic strings.
- Distillation runs exclusively at composition-root egress (`_emit`, `_diagnostic`) — interior folds return `Result[Report, Fault]` with raw defect text; adapters own step names, event rings, and hint assembly.
- `failing_step` names the pipeline stage or handler gate that rejected the value — stage tags (`strict:`, `validation:`, `config:`, `parse:`) prefix adapter-owned fault text; distillation copies the prefix into `failing_step` or `hint` without re-parsing domain errors.
- `recent_events` is a bounded tuple ring seeded from dispatch, parse, and surplus-token paths — surplus positional tokens use the same ring shape as `parse_fault` so automation sees one diagnostic vocabulary for pre-validation and operational failures.
- `elapsed_ms` and `resource` snapshot attach at distillation time from invocation scope — resource facts batch through oneshot probes into the `Diagnostic` wire shape; domain handlers do not carry timing or psutil fields.
- `dispatched` records whether the handler pipeline ran — `False` for bind/parse/config faults that never entered the composed layer stack; consumers use it to distinguish admission failures from handler failures without inferring from empty `report`.
- `error_context` on the envelope carries an optional secondary `Diagnostic` on fault lines — mutual exclusivity of `report` and `error` holds; success lines populate `report` only; fault lines populate `error` and may attach `error_context` for nested evidence without duplicating the primary fault string.
- `claim` and `verb` on the envelope are the consumer routing keys — automation dispatches on the pair after `schema_version` closure; interior handler names and registry symbols are not repeated on the wire for consumer routing.
- Subprocess and automation consumers assume exactly one newline-terminated deterministic JSON line on stdout per root invocation — partial reads, multiple lines, or interleaved stderr JSON are contract violations, not parser edge cases.
- `Completed` receipts from spawned children store raw stdout/stderr bytes — parent folds decode child stdout through the envelope decoder when the child is an assay rail; foreign tools keep bytes opaque unless they adopt the same envelope contract.
- Consumers decode stdout through the same module-level `_ENVELOPE_DECODER` the root uses for emit — no `json.loads`, no per-consumer `Decoder` instances, no schema-loose dict views of the line.
- Pass-one consumer logic validates `schema_version`, `claim`, and `verb` before interpreting `report` or `error` slots — body polymorphism selects only after envelope closure is proven.
- Exit codes for CLI hosts project from materialized envelope fields (`__cyclopts_returncode__` or equivalent) — consumers must not re-derive exit status from stderr presence or ad hoc string matching on fault text.
- History readers (`load_history`, `delta`) consume bytes written by `_encode` — replay posture is trusted-internal with pinned encoder identity; consumers treat missing sides as explicit `EMPTY` status in delta notes, not as `None` coercion on snapshot fields.
- When `truncated=True`, consumers resolve full evidence through history artifact paths keyed by `run_id` — wire line is the index, not the archive; parsers must not treat truncated `results` or `artifacts` tuples as complete.
- `receipt()` materializes subprocess outcomes into `Completed` rows before parent fold — consumer-facing aggregation happens on `Completed` tuples at the boundary, not on raw exit codes alone.
- After materialization exit, consumers and interior dispatch narrow with `TypeIs` and exhaustive `match` on canonical owners — `TypeGuard` plus `cast` chains are rejected at consumer seams and handler interiors alike.
- Tagged `AnyDetail` arms narrow through discriminant tags shared with egress `tag_field` — consumer code matches on the same vocabulary the encoder emitted; foreign synonyms were already mapped at ingress and egress adapters only.
- Optional detail slots use explicit `None` policy — consumers do not default missing detail to a diagnostic or success variant unless the envelope `status` and slot policy declare optional polymorphism.
- Automation consumers may assume `report.detail` satisfied round-trip proof before emit when `status` is non-fault and detail is present — proof failure never reaches stdout; it becomes a root-level fault before `_emit_envelope`.
- Fault outcomes skip detail proof — consumers must not run `validate_detail` on error lines; `error` and `error_context` slots are authoritative for failure interpretation.
- Consumer contract tests decode lawful detail values through production `_ENCODER` and `_DETAIL_DECODER` singletons — not shadow encoders in test helpers.

# Evolution Consumer Obligations

- CLI stdout parser: upstream one `bytes` line; decode via `_ENVELOPE_DECODER`; gate on `schema_version` literal; proof on success `detail`.
- Subprocess parent fold: upstream child stdout `bytes`; decode via same decoder; gate on version; proof when child ok.
- History replay: upstream stored `bytes`; decode via `_ENVELOPE_DECODER`; gate on version; proof optional.
- Delta compare: upstream two envelope structs; field access only; gate per side; no proof assumed.
- Probe cache key: upstream deterministic encode; pin encoder identity; gate with schema; no proof assumed.
- Invariant doubler: upstream second emit attempt; decode fault envelope on stderr; gate on version; no proof assumed.
- Settings bootstrap: upstream env → frozen settings; pydantic validate; gate on settings schema; round-trip when settings feed cache keys.
- Migration read: upstream stored struct vN; `msgspec.convert` → vCurrent; gate per hop; no proof assumed.

# Admitted Stack And Typing Gates

- Declared modeling dependencies: `pydantic>=2.13.3`, `pydantic-settings>=2.14.0`, `msgspec>=0.21.1`, `beartype>=0.22.2`, `expression>=5.6.0`. `requires-python >=3.15` — baseline for all materialization doctrine.
- Composition-root seams bind stage owners, codec singletons, round-trip proof gates, and cross-axis handoff rows to CI enforcement before domain logic is suspect.
- Verified runtime (workspace lock): Python 3.15.0b1, Pydantic 2.13.4, msgspec 0.21.1.
- PEP 728: explicit `TypedDict` openness. PEP 749/649: deferred annotations via `__annotate__` — no quoted forward refs. PEP 695: `type Payment = Annotated[...]` for union aliases. PEP 742: `TypeIs` for complement-safe narrowing. PEP 814: `frozendict` for immutable mapping materialization.
- Ruff `target-version = "py315"` — materialization code uses modern union/spelling throughout.
- Ruff `runtime-evaluated-base-classes` and `extend-immutable-calls` admit msgspec and pydantic constructors.
- Banned at boundary: `json.load`/`loads`/`dump`/`dumps`, `os.getenv`, `os.environ`, `typing.NamedTuple`, `typing.cast`, `typing.Any`, v1 `pydantic.validator`.
- Dual-path production proof: assay composition uses Pydantic `BaseSettings` + field/model validators for config ingress; assay core uses msgspec `Struct` + deterministic `wire_encode` for egress envelopes.

# Boundary Limits And Open Proofs

- Polymorphic nested `model_copy(update=...)` without typed nested validation lacks a one-expression seam — explicit nested `model_validate` assembly at patch handoff remains mandatory until replacement axis admits `replace_validated`.
- Cross-process worker boot does not yet prove codec singleton identity and enum closure parity across forked importers in one CI gate — replay depends on root module path stability and pinned decoder construction.

# Harness Composition And Runtime Coupling

- Stage-map vs lattice role map — materialization stage names and projection lattice roles align but are not interchangeable identifiers; harness tables must name both when a seam edge crosses family boundaries.
- Trust posture composition — semi-trusted internal payloads still decode through module-level decoders with fault mapping; trusted-replay does not bypass migration when stored version precedes current egress struct.
- Law collection vs isolated pytest — `@spec(lambda ...)` anonymous witnesses fail pytest policy; named symbols on root codec owners keep law matrices discoverable in full-suite mode.
- Hypothesis registry coupling — generative strategies for materialization laws must track vocabulary and variant registry rows in the same promotion unit — new arms without strategy updates produce false-green proof runs.
- Free-threaded codec publish — encoder and decoder singletons must finalize before parallel importers bind root symbols; post-import reassignment of module-level codecs races under parallel test workers.

# Anti-Patterns

- Double-parse (`json.loads` then `validate_python`), per-request `TypeAdapter`/`Encoder`/`Decoder`, domain-side validation with bare `if`/`try` instead of boundary `capture`, mutating materialized instances, parallel `FooDTO`/`FooModel`/`FooStruct` for one concept, importing `msgspec` or `pydantic` inside rich domain owner modules.
- `msgspec.Struct` for untrusted external ingress without pairing to explicit decode constraints and fault mapping.
- Pydantic `BaseModel` for high-volume internal cache/wire rows where msgspec `gc=False` Struct is admitted and sufficient.
- Defaulting unknown `schema_version` to the current struct and attempting body decode — masks version skew until interior union arms fail opaquely.
- Chained hand-rolled field copies across multiple version steps in one read — obscures migration provenance and breaks exhaustive arm checks.
- Persisting `model_dump` JSON while consumers parse msgspec deterministic bytes — desynchronizes history replay from live automation stdout.
- Parsing stdout with `json.loads` into untyped dicts — erases caps, `truncated`, and discriminant tags consumers need for correct routing.
- Running `validate_detail` inside consumer business logic on every line — proof is an egress guard obligation; consumers trust success-path materialization or handle explicit proof faults from the root.
- Emitting diagnostic strings from domain folds — bypasses cap spill, distillation rings, and uniform `Diagnostic` shape automation depends on.
- Declaring per-verb `TypeAdapter` or `Encoder` inside handler bodies — schema build cost and non-deterministic encoder identity break proof and cache contracts.
- Emitting wire bytes from domain folds or leaf rails — all stdout/stderr envelope lines route through root `_encode` / `_emit_envelope`.
- Calling `validate_detail` (or equivalent proof) inside domain transforms — proof is an egress guard obligation, not an interior invariant check.
- Skipping `_guard` because the handler "does not throw" — promotion faults (`FaultedPromotion`, beartype violations, `MsgspecError`) still require root capture.
- Second envelope write without the one-write invariant handler — automation and subprocess consumers assume exactly one parseable line per invocation.
