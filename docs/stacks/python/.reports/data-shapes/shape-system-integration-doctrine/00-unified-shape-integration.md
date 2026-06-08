# Shape System Integration Doctrine — Unified Synthesis

# Canonical Shape Stack

- Python `>=3.15` admits one vertical shape stack per bounded context; each layer owns a distinct invariant class; lower layers never re-validate what an upper layer already proved.
- **Vocabulary** — `Literal`, `StrEnum`, `NewType`, and `Annotated` scalar constraints encode closed value sets and opaque atoms.
- **Absence** — `Option[T]`, `sentinel()`, and rail collapse at boundaries encode missing, withheld, or inapplicable states without overloading `None`.
- **Domain record** — `dataclass(frozen=True, slots=True)` or `BaseModel(frozen=True)` holds the canonical concept with smart constructors returning `Result[T, E]`.
- **Variant family** — `@tagged_union`, `Annotated[Union, Discriminator]`, or msgspec `tag_field` encodes closed multimodal cases under one owner.
- **Collection algebra** — `tuple`, `frozenset`, `frozendict`, `expression.Block`, and `expression.Map` carry immutable aggregates; no mutable `list`/`dict` in model fields.
- **Structural port** — `Protocol` slices capabilities for dependency injection; ports never own domain invariants.
- **Rail interior** — `Result[T, E]` and `@effect.result` generators thread typed failure through transforms without parallel exception paths.
- **Wire egress** — `msgspec.Struct(frozen=True, gc=False)` projects domain to transport; field rename and omit policy live here only.
- **Wire ingress** — Pydantic `TypeAdapter` and `BaseModel(strict=True)` decode foreign bytes; violations exit as `CodecFault` or boundary `Result`, never as domain exceptions.
- **Settings ingress** — `pydantic-settings` frozen models own environment and file configuration with the same strictness as wire ingress.
- Admission rule: a concept occupies exactly one canonical layer; projections derive sibling views; parallel types for the same concept are forbidden.

# Invariant Ownership

- Every invariant has one owner shape; other forms may carry the field but must not re-prove the rule.
- **Cardinality and presence** — owned by the canonical domain record or variant discriminant; ingress mirrors field optionality but does not invent new optional semantics.
- **Cross-field rules** — owned by the domain smart constructor or ingress `model_validator(mode="after")` when wire-visible; never duplicated in decorators, dispatch tables, or protocol methods.
- **Vocabulary closure** — owned by `Literal`, `StrEnum`, or `@tagged_union`; `match`/`case` with `assert_never` is the sole closed dispatch proof.
- **Structural capability** — owned by `Protocol` conformance checked at the injection boundary; domain code assumes the port contract without `isinstance` ladders.
- **Transport shape** — owned by msgspec struct policy (`forbid_unknown_fields`, `omit_defaults`, `rename`, `tag_field`); domain models do not encode wire aliases.
- **Configuration shape** — owned by `pydantic-settings` models; runtime domain records do not read environment variables directly.
- **Absence semantics** — owned by `Option`, sentinel union members, or explicit variant cases; `None` is not a generic sentinel when the domain distinguishes missing from empty or unknown.
- Duplicate-validation test: if removing a validator from form B leaves form A unable to enforce the invariant, B was wrongly owning it; move the rule to A and make B a projection; run per concept at proof time and when drift signals fire.

# Shape Graduation Ladder

- Shapes graduate upward when invariant density increases; they graduate outward when the concern crosses a boundary; never graduate by wrapper nesting.
- **Atom to vocabulary** — repeated primitive validation graduates to `NewType` or `Annotated[..., BeforeValidator, Field(...)]`.
- **Vocabulary to enum** — string literals referenced in three or more sites graduate to `StrEnum` with payload fields when wire tokens carry metadata.
- **Record to rich class** — a frozen record gains a smart constructor when construction requires multi-field proof or returns `Result`.
- **Record to variant family** — mutually exclusive shapes over the same concept graduate to `@tagged_union` or discriminated Pydantic union instead of parallel optional fields.
- **Domain to wire** — stable persisted or networked concepts graduate an egress msgspec struct; ingress keeps a separate Pydantic model when foreign schema differs.
- **Open extension to dispatch** — closed domains stay on `match`; open extension graduates to `singledispatch` or typed registry rows at the surface owner, not inside the domain record.
- **Mutable transition to replacement** — state change graduates to `copy.replace`, `model_copy(update=...)`, or `msgspec.structs.replace`; in-place mutation is not a domain transition.
- Reject graduation when the new form only renames the old form without adding an invariant class.
- Reject chained nesting where type feeds constant feeds wrapper class feeds another wrapper.

# Single Validation Gate

- Validation runs once per boundary crossing; interior pipelines trust canonical shapes.
- **Ingress gate** — Pydantic `TypeAdapter.validate_json` / `validate_python` at the boundary adapter; extract violations into `CodecFault` or tagged boundary errors.
- **Construction gate** — smart constructors on domain records return `Result[T, E]` instead of raising.
- **Egress gate** — msgspec `convert` / `encode` assumes domain already valid; encoding failures are transport defects, not re-validation.
- **Runtime gate** — `beartype` and static `ty`/`mypy` enforce shape at call sites; they do not replace domain constructors.
- **Decorator gate** — validation decorators admit only boundary-normalized values already on a `Result` rail; they do not parse raw dicts or strings.
- Pipeline law: `bytes → ingress model → transform(Result) → domain → project → egress struct → bytes`.
- No second Pydantic pass on domain objects.
- No msgspec decode of domain interiors except at persistence boundaries.

# Composition Topology

- Shapes compose through projection and rail bind, not through inheritance grab bags or mixin stacks.
- **Nesting** — domain records embed other canonical records and immutable collections; depth follows semantic ownership, not file convenience.
- **Variant composition** — variant members embed records flatly; avoid inheritance diamonds between ingress and domain types.
- **Tagged union composition** — algorithm-specific evidence details extend one union base; fold projections derive counts and summaries from the fact stream.
- **Map composition** — `frozendict` rows compose policy tables; shallow immutability requires nested values to be immutable or owned records.
- **Phantom staging** — stage parameters on generic records encode lifecycle without parallel model definitions.
- **Rail composition** — `pipe`, `block.fold`, `result.bind`, and `@effect.result` compose transforms over canonical shapes; error types stay in the error channel.
- **Decorator composition** — outer stack order `trace > authorize > validate > cache > govern > retry` preserves PEP 695 `**P` and threads `Result`; decorators wrap surfaces, not model internals.
- One bounded context exports one canonical shape per concept; chooser projections are named by transport or admission role, not by parallel entity identity.

# Protocol And Capability Admission

- Protocols integrate shapes at the injection seam; they do not store domain state or validate payloads.
- **Capability slicing** — minimal method sets express `Readable`, `Writable`, and merged `Dual`; authorization uses `Literal` capability vocabularies and frozenset subset checks.
- **Structural admission** — `@structural(Protocol)` or `Ok(obj).filter_with(narrows, evidence)` lifts untyped boundary objects onto `Result` rails before domain entry.
- **Variance** — PEP 695 infers protocol variance from method positions; adding an input type parameter flips to invariant and is a breaking contract change.
- **Runtime check** — `@runtime_checkable` only at boundaries where foreign objects arrive; interior domain code uses static protocol typing without `isinstance`.
- **DI threading** — `@effect.result` generators `yield from` port operations; ports return `Result` or `Option`, never raise for expected failure.
- Protocol rule: if a method needs domain invariants true before call, those invariants belong to the argument record, not to a pre-call validator on the protocol.

# Rail And Effect Integration

- Rails carry shapes through failure without erasing type evidence; the error type is part of the shape contract; error type changes require rail type changes.
- **Channel selection** — `Result[T, E]` for typed failure, `Option[T]` for absence, `@effect.async_result` for async fallible work; no `Optional[T]` fallible returns.
- **Error ownership** — file-internal errors stay as `@tagged_union` variants inside the owning context; package-level domain error types cross context seams only; defects accumulate in `Block[Defect]` for parallel validation.
- **Accumulator modes** — sequential `bind` short-circuits on first `Error` without mutating prior canonical identities; parallel fold concatenates defect blocks without losing earlier evidence.
- **Boundary collapse** — `to_option().map(...).default_value(...)`, `swap().default_value(...)`, and sentinel projection happen once at the adapter or seam exit; domain interiors never collapse rails.
- **Boundary adapter mapping** — `ValidationError`, `msgspec.ValidationError`, and seam remap failures map into `CodecFault` or tagged boundary errors inside `capture`/`async_capture`; sole permitted exception sites.
- **Recovery placement** — `.or_else_with` at composition boundaries and seam exits only; never inside `@effect.result` generators threading canonical transforms.
- Adding exception branches to projected views is a binding defect.

# Dispatch And Registry Coupling

- Dispatch surfaces consume canonical shapes; they do not define parallel ones; dispatch owns routing keys and registration order; domain shapes own case payloads.
- **Closed dispatch** — `match`/`case` on canonical unions with `assert_never`; not inside domain record methods.
- **Open dispatch** — `singledispatch` `project` tables key on canonical domain types or verified `StrEnum` members at definition time, not string keys at runtime; handlers are `Callable[[Canonical], Result[...]]` over materialized shapes.
- **Catalog rows** — `Runner`, `Language`, `Mode` supply wire token, flags, and routing metadata as enum payload fields; registry iteration for diagnostics uses `for member in Token`.
- **Fold-derived projections** — counts, status summaries, and receipts fold from struct streams with slot/kind metadata; do not maintain parallel counter models.
- **Foreign catalog import** — foreign contexts import catalog enums through vocabulary export, not handler registries.
- Dispatch never parses raw strings into domain objects; boundary adapters deliver canonical values before registry lookup.

# Decorator Shape Admission

- Decorators admit callables and registry entries onto surfaces; they are not alternate model definers.
- Reject marker-only decorators that signal intent without changing types or rails.
- Reject decorators that embed provider strings, dynamic import paths, or codec choice.
- **Outer stack order** — at admitted surfaces: `trace > authorize > validate > cache > govern > retry`; inner decorators preserve PEP 695 `**P` and thread `Result`.
- **Parameter preservation** — PEP 695 `**P`, `Concatenate`, and `@wraps` keep callable shape visible to `ty` and `mypy`.
- **Validation admission** — validation decorators admit boundary-normalized values on `Result` rails — not raw dicts, strings, or foreign projections.
- **Registration admission** — registration decorators require handler types parameterized on canonical domain types before registry insertion.
- **Idempotency and frozen config** — `__wrapped__` or marker attributes reject double decoration; decorator factories accept frozen `BaseModel` config records, not loose kwargs bags.
- **Context discharge** — `ContextVar` resolution belongs in `absorb`-style wrappers at the stack edge; model constructors and smart constructors do not read ambient context.

# Boundary And Codec Placement

- Boundaries are the only sites where foreign representations enter or leave the canonical stack.
- **Ingress** — Pydantic strict models, `BeforeValidator` on `Annotated` fields, `Discriminator` for unions, `model_validator(mode="before")` only for wire-shape renaming.
- **Egress and persistence** — msgspec structs with transport policy; read path decodes to ingress or wire struct then transforms to domain; write path projects domain to wire struct then encodes.
- **Process and settings** — cucumber/sk or explicit boundary adapters for unpicklable resources; `pydantic-settings` loads configuration into frozen models at process start.
- **Exception containment** — `capture` / `async_capture` combinator at boundary adapters; sole permitted `try`/`except` sites.
- Domain folders do not import `httpx`, `fsspec`, or filesystem paths as field types; boundaries return canonical shapes or `Result` thereof.

# Package Boundaries And Anti-Patterns

- Package admission follows invariant class, not library branding.
- **Pydantic** — ingress, settings, and boundary `TypeAdapter` only; not interior domain transforms or dispatch tables.
- **msgspec** — wire structs, JSON/msgpack encode decode, and persistence envelopes; not rich behavior or smart constructors.
- **expression** — `Result`, `Option`, `Block`, `Map`, `@tagged_union`, `@effect.result`, and `pipe` compose domain logic; not boundary exception catching.
- **beartype** — call-site runtime shape checks; not a second validation pipeline duplicating Pydantic.
- **cyclopts** — CLI surface projects already-defined canonical types via `Parameter` metadata; CLI does not define parallel config shapes.
- **hypothesis / msgspec in tests** — law fixtures may use `msgspec.Struct` mirrors; test shapes do not become production owners without promotion through the graduation ladder.
- Forbidden patterns: DTO soup, dual validation for one rule, god modules mixing registry + codec + domain folds, package-named internal types, mutable model fields, ABC/`abstractmethod` ports, and stringly dispatch keys.

# Projection Lattice Roles

- One canonical domain owner per concept anchors a finite lattice of chooser projections; projections are named by transport or admission role, never by parallel entity identity.
- **Canonical** — frozen domain record or `@tagged_union` family; sole owner of invariants, smart constructors, and interior transforms.
- **Ingress** — Pydantic `BaseModel(frozen=True, strict=True)` or `TypeAdapter` target; admits foreign bytes and dict carriers; field optionality mirrors wire, not domain absence semantics unless the wire contract is the source of truth for that axis.
- **Wire egress** — msgspec `Struct(frozen=True)` with transport policy (`rename`, `omit_defaults`, `forbid_unknown_fields`, `tag_field`); never carries behavior or construction logic.
- **Settings slice** — `pydantic-settings` frozen model; environment and file keys map through `Field(validation_alias=...)` only here.
- **CLI slice** — cyclopts `Parameter` metadata projects fields from an already-defined canonical or settings type; CLI modules do not declare independent field sets.
- **Persistence envelope** — wire struct or tagged struct stream row when storage schema matches transport; otherwise a dedicated persistence projection with version slot and payload struct.
- **Receipt or evidence slice** — fold-derived struct or dataclass row materialized from fact streams; read-only; never a second domain owner for the same concept.
- Lattice law: every projection derives from canonical through an explicit adapter function or codec owner; no projection imports another projection as its definition source.
- Ingress and wire may diverge on aliases and omitted fields; they must not diverge on discriminant vocabulary or scalar closure without an adapter-owned mapping table.

# Derivation Direction And Sync

- Derivation flows outward from canonical; foreign shapes flow inward through ingress only; bidirectional sync is forbidden — each edge is a one-shot transform with a typed owner.
- **Canonical to wire** — boundary module exposes `project_<concept>_wire(domain: Canonical) -> WireStruct` using `msgspec.convert` or field-explicit construction; domain modules never import msgspec.
- **Canonical to ingress (reverse rarely)** — only when emitting schema samples, OpenAPI fixtures, or test factories; uses `model_validate` on a dict built from canonical fields, not a persisted ingress type as intermediate state.
- **Ingress to canonical** — adapter-owned `materialize_<concept>(ingress: IngressModel) -> Result[Canonical, BoundaryError]`; normalization completes before smart constructor or direct field mapping.
- **Wire to canonical** — persistence read path: `decode -> WireStruct -> materialize`; never `WireStruct -> IngressModel -> Canonical` unless ingress and wire are intentionally identical shapes under policy.
- **Settings to boot config** — settings model validates at process start; domain boot receives an immutable settings instance or a canonical config record projected once from settings via composition-root `project_*_config`.
- **Vocabulary sync** — `StrEnum`, `Literal`, and msgspec `tag_field` strings are declared once on the canonical owner or shared vocabulary module; ingress `Discriminator` and wire `tag` map from that row via import, not re-declaration.
- Field correspondence lives in the boundary adapter module as typed mapping or explicit constructor kwargs — not as scattered `model_dump` key pops.
- When ingress field count differs from canonical, the adapter documents each omission as wire-optional, domain-default, or computed-on-materialize.

# Phantom Stage Threading

- Lifecycle stages attach as phantom type parameters on generic canonical owners so one implementation serves raw, normalized, validated, and enriched states without parallel class hierarchies.
- **Stage parameter** — `Signal[P: Stage = "raw"]` or `Record[Stage: Literal["draft", "published"]]` encodes lifecycle; stage is not a runtime string field unless persistence requires it.
- **Stage transition** — methods return `Self` narrowed to the next stage literal or `Result` when transition proof fails; transitions use `copy.replace` or owner methods, not attribute mutation.
- **Projection stage lock** — ingress materializes at `"raw"` or `"wire"` stage; domain smart constructors promote to `"canonical"`; wire egress accepts only `"canonical"` or `"published"` inputs; mismatched stage at a projection edge is a static error when `ty`/`mypy` enforces stage bounds.
- **Cross-projection stage map** — boundary adapters declare which stage each projection accepts and emits; a wire struct never encodes a stage the domain has not yet proven.
- **Open extension stages** — when stage set is not closed, stage vocabulary graduates to `StrEnum` with registration rows; phantom literals remain for closed pipelines.
- Reject parallel `{Concept}Raw`, `{Concept}Normalized`, `{Concept}Final` classes when one generic owner with stage parameter and replace transitions expresses the same invariant ladder.

# Context Export Surfaces

- Bounded contexts integrate through explicit export surfaces, not through deep imports of foreign domain interiors.
- **Public export** — one or two symbols per module via `__all__`; canonical types and boundary adapters are the only stable cross-context imports.
- **Interior opacity** — `_`-prefixed constructors, registry rows, fold helpers, and file-local error unions stay inside the owning context; foreign modules import adapters, not registries.
- **Vocabulary export** — shared `StrEnum` and `NewType` atoms export from a vocabulary owner when two contexts must agree on tokens; domain records do not export across contexts — adapters translate.
- **Error export** — package-level domain error types cross context seams; file-internal `@tagged_union` errors collapse to boundary errors at the adapter and do not propagate as import dependencies.
- **Protocol export** — structural ports export as `Protocol` definitions; implementations stay in the providing context; consuming context depends on protocol plus adapter, not concrete classes.
- **Receipt export** — typed algorithm receipts and fold summaries may cross seams as immutable structs when they carry evidence, not when they duplicate canonical state.
- Import law: domain folders do not import sibling domain folders directly; integration routes through boundary packages, composition roots, or shared vocabulary modules.

# Anti-Corruption At Seams

- When foreign context shapes differ in field naming, cardinality, or discriminant encoding, an anti-corruption adapter owns all translation — the canonical owner never absorbs foreign layout.
- **Seam adapter** — single module function or small adapter class per foreign context pair: `foreign_ingress -> Result[Canonical, SeamError]` and optionally `Canonical -> foreign_wire`.
- **Vocabulary remap** — foreign string tokens map through explicit tables keyed by foreign token, valued by canonical enum member; no `.lower()` or `.strip()` normalization hidden in domain constructors.
- **Cardinality remap** — foreign optional fields that mean domain absence map to `Option` or sentinel variants at the adapter; foreign required empties map to validation failure at ingress, not silent domain defaults.
- **Union remap** — foreign discriminant values that collide or split differently than canonical variants map through adapter `match` arms; unmapped foreign cases return tagged seam errors, not partial canonical instances.
- **Version seam** — adapter selects materialization path by envelope `schema_version`; domain owner stays version-agnostic.
- **Idempotent replay** — seam adapters tolerate re-application when foreign payloads are identical; canonical materialization remains pure given normalized ingress.
- Anti-corruption modules do not call foreign domain services, registries, or smart constructors; they deliver canonical values or `Result` failure to the owning context entrypoint.

# Composition Roots And Cross-Context Wiring

- Integration assembles contexts at composition roots — CLI entry, job runner, test harness, or application factory — not inside domain transforms.
- **Root imports** — composition roots import boundary adapters, settings slices, protocol implementations, and seam modules — never foreign domain interiors or file-local registries.
- **Handoff graph** — typed per context: `foreign_ingress -> Result[Canonical, E]`, then `Canonical -> interior_transform(Result) -> Canonical`, then `Canonical -> foreign_wire` or seam export; context A completes before seam adapter produces input for context B materialization; no shared mutable store between contexts.
- **Settings fan-out** — one frozen settings model projects into per-context boot configs through root `project_*_config` before any domain transform; contexts do not read each other's settings fields or environment directly.
- **Protocol scope** — `Map[type[Port], object]` or explicit service records inject at root; domain operations `yield from` port resolution inside `@effect.result` generators.
- **Fact stream append** — `Block` or tagged struct stream append at mutation boundaries inside the owning context; cross-context consumers receive immutable stream snapshots or fold-derived receipts — not live mutable buffers or re-parsed domain interiors.
- **Test seam** — law fixtures build canonical shapes directly or through ingress adapters; cross-context integration tests compose roots with in-memory protocol fakes, not foreign domain imports.
- **Import-cycle resolution** — belongs at the root; acyclic domain modules within a context must not import sibling context domain folders.

# Stack-To-Projection Binding

- Each stack layer admits at most one active projection role per concept; chooser views are lattice siblings, not alternate stack layers; stack layers never re-validate lattice projections, and projections never own domain invariants the canonical layer already proved.
- **Vocabulary and absence** — canonical only; ingress mirrors closure via vocabulary-owner import; wire and settings never declare parallel token or sentinel types.
- **Domain record and variant family** — canonical projection and sole `materialize_*` target; ingress and wire never substitute as durable domain owners.
- **Collection algebra** — canonical fields only; ingress may admit mutable carriers at the adapter edge; materialization promotes to immutable aggregates before domain transforms run.
- **Structural port** — exports at context boundary with root binding; implementations stay in providing context; ports do not appear as ingress or wire field types.
- **Rail interior** — spans canonical transforms only; boundary adapters collapse `Result`/`Option` once at ingress exit and seam exit; projected views do not re-enter rails without rematerialization.
- **Wire egress, ingress, and settings** — boundary projections exclusively; domain modules import neither msgspec nor Pydantic except through adapter signatures in boundary packages; settings fans out via composition-root `project_*_config` only.

# Materialization Pipeline Alignment

- **Ingress payload stage** — accepts only foreign carriers (`bytes`, `str`, `Mapping`); output is never a canonical domain record.
- **Validation stage** — produces ingress projection or `TypeAdapter`-validated intermediate; enforces `strict=True` and unknown-key policy aligned with lattice ingress role.
- **Normalization stage** — reshapes wire layout on the ingress projection; does not assert cross-field domain rules owned by the canonical smart constructor.
- **Construction stage** — `materialize_*` adapter emits canonical record or returns `Result` from smart constructor; first stage where stack domain layer receives ownership.
- **Enrichment stage** — `copy.replace`, `model_copy`, or owner methods on canonical shapes only; phantom stage parameters narrow here when lifecycle applies.
- **Materialization exit** — canonical owner or terminal frozen model; interior modules accept only this artifact class without adapter re-entry.
- **Serialization stage** — `project_*_wire` from canonical to msgspec struct, then encode; no domain import of codec modules past this edge.
- Stage-skipping requires composition-root exemption documentation — not convenience imports that bypass validation or materialization owners.

# Seam And Stack Altitude Binding

- Context seams sit at the same architectural altitude as wire and settings boundaries — outside the single validation gate interior, never between domain transform steps.
- Seam modules align with materialization pipeline stage owners: validation and normalization stay on ingress side of the seam; enrichment stays inside the owning context after canonical materialization.
- Bind ingress and wire stack layers to seam adapters only — vocabulary through settings layers do not execute across context seams without canonical materialization in the exporting context first.
- Seam crossing consumes one anti-corruption `materialize_*` per foreign pair; interior stack layers from collection algebra through rail interior never span the seam as foreign projection types.
- Post-seam entry in the receiving context restarts at canonical or boot-config handoff — not at ingress payload carriers, even when foreign and local wire schemas match byte-for-byte.
- Decorator binding at seams: `trace` and `authorize` on adapters; `validate` only when the seam re-admits a foreign carrier — not when handing off an already-canonical value.
- **Pipeline seam placement** — before domain: one ingress or anti-corruption materialization; after domain: one egress or handoff projection with no smart-constructor re-entry on projected views; between contexts: seam adapters with `capture`/`async_capture` only — no domain folds or registry dispatch.
- Parallel contexts do not share canonical types unless vocabulary module explicitly unifies tokens; shared concepts get one owning context and adapter exports for consumers.
- Observability — trace decorators wrap seam adapters and composition root handoffs; domain interiors assume traced canonical values already admitted.

# Fact Stream And Fold Bus

- Mutation receipts append to one immutable `Block[ReceiptRow]` or tagged struct stream per bounded context; slot and kind fields use closed `StrEnum` vocabularies imported from catalog or dispatch owners — not ad hoc strings.
- Fold projections derive counts, status summaries, and evidence slices from the stream; receipt or evidence projections are read-only lattice siblings materialized by fold — never parallel counter models.
- Algorithm-specific typed receipts remain when fields carry route, status, sampling, solver, spectral, mesh, or extraction evidence; generic `IReceipt` abstractions do not replace typed rows.
- Cross-context handoff exports fold-derived receipts or stream snapshots when evidence crosses seams; canonical state does not duplicate inside receipt payloads.
- Stream version envelopes follow version-envelope law; fold paths select by version at the adapter.

# Trust Posture Routing

- **Untrusted foreign ingress** — full Pydantic strict validation at ingress projection; no `model_construct` or trusted convert on raw bytes.
- **Semi-trusted service payloads** — validate once at ingress; `forbid_unknown_fields` or `extra="forbid"` remains on for the session.
- **Trusted-internal same-process** — msgspec `Decoder` permitted when bytes were encoded by the owning codec module in-session; decode faults still map at boundary.
- **Trusted-replay** — `model_construct` or trusted `convert` only when composition root pins store key, schema version, and encoder identity.
- Trust label does not propagate: re-entry of `json.loads` output, ORM dict views, or partial buffers always restarts at validation regardless of upstream label.

# Evolution, Versioning, And Simultaneous Update

- Persistence rows, wire structs, and cross-process handoffs carry `schema_version: Literal[...]` on envelope or payload structs — canonical domain owners stay version-agnostic when migration folds absorb historical layouts at read boundaries.
- Version literals graduate through a closed ladder declared beside the vocabulary owner — `"v1"`, `"v2"` as `StrEnum` or `Literal` union members, not free strings or integer counters without documented semantics.
- Adding a version arm requires simultaneous updates to migration fold, ingress discriminator map when wire-visible, egress `tag_field` when persisted, and contract proof samples — partial version promotion is a merge blocker.
- Deprecation retires version arms only after migration fold proves zero read-path dependency in production stores and test fixtures — removed arms stay in migration modules with `assert_never` witnesses, not in active domain `match`.
- Canonical smart constructors never branch on `schema_version` — version selection completes in boundary adapters before `materialize_*` emits the domain owner.
- Forward-compatible ingress may accept unknown version keys at validation exit only when composition root documents tier policy — unknown versions fail at adapter materialization with `MigrationFault`, not silent default to current canonical.
- Read-path migration lives in boundary adapter modules as total folds: `StoredEnvelope -> Result[WireStruct | Canonical, MigrationFault]` — one fold owner per concept per version jump, not scattered `if version` in domain transforms.
- Write-path emits only current-version wire structs — adapters do not dual-write obsolete layouts unless composition root documents transitional dual-emit with sunset date.
- Cross-context migration at seams maps foreign version envelopes through anti-corruption adapters before local version folds run — foreign and local version ladders do not merge into one undifferentiated handler.
- Nested sub-owner migrations compose inside-out — inner family version resolves before outer envelope materialization; outer migration fold receives already-current inner payloads or typed `MigrationFault`.
- Migration folds return `Result` with tagged fault variants — unknown stored tags, impossible field combinations, and partial historical rows fail closed; default arm selection on unknown tags is forbidden unless `Extension` policy explicitly captures.
- Replay and cache rehydration pin encoder identity, store key, and schema version at composition root — trusted-replay does not bypass migration when stored bytes predate current version.
- Vocabulary changes propagate in one promotion unit: canonical enum or `Literal` owner, ingress `Discriminator` mapping, wire `tag_field` strings, OpenAPI enum arms, hypothesis registry rows, and seam remap tables — CI contract suite fails on any orphan token.
- Stack graduation changes bind lattice edges in the same unit — graduating record to variant family updates materialization adapter, wire egress projection, and dispatch registry keys together; interior domain modules change only after boundary adapters land.
- Phantom stage literal additions require stage-map updates on every projection edge that accepts or emits staged canonical owners — static `ty`/`mypy` stage bounds and adapter rejection tests update in the same change.
- Fact-stream receipt kinds gain new `StrEnum` members only with fold arm updates — adding a kind without fold totality proof is a binding defect caught by exhaustive fold tests.
- Settings slice field renames use `validation_alias` exclusively on the settings model; domain boot configs project through root `project_*_config` in the same change; domain constructors never absorb renamed env keys.

# Integration Proof Architecture

- Integration correctness is provable through composed static, contract, metamorphic, runtime, and mutation obligations — not through duplicate manual review.
- Proof harness layers stack orthogonally; execution order is static checkers before contract tables before metamorphic properties before integration smoke before mutation — failures at the static layer block expensive generative runs; orthogonal checker matrices execute only after integration static layers pass.
- Proof failure targets the adapter or vocabulary owner first, not the canonical domain record; proof debt from checker gaps tracks on the language axis — source declarations stay spec-complete; harness suppressions at integration seams are rejected; binding round-trip failures implicate integration adapter binding before concept-axis engine or replacement owners.
- **Layer 1 — static** — `ty`/`mypy` exhaustiveness on canonical unions and stage literals; `assert_never` arms required; runs before orthogonal checker matrices on typed-payload, pydantic, and protocols axes; static failure blocks all generative suites.
- **Layer 2 — import architecture** — domain folders never import sibling domain folders; domain modules never import msgspec or Pydantic except through type-only boundary stubs when unavoidable; composition roots never import foreign registries or file-local fold helpers; wire modules never import ingress modules; runs before arch tests on concept-axis modules; import failure attributes to integration seam placement before concept-axis interior.
- **Layer 3 — contract tables** — vocabulary bijection, remap totality, and payload key tables as parametrized rows beside vocabulary or family owners; one table row drives production remap assertions and CI bijection proofs; adapters import the table, tests do not duplicate token lists; typed-payload contract rows share the same parametrized owner beside vocabulary or family tables.
- **Layer 4 — metamorphic and round-trip** — runs after contract tables; when bijection holds: generate lawful canonical value → `project_*_wire` → encode → decode → `materialize_*` → assert equality on canonical identity; subset bijection tests declare excluded fields — computed-on-materialize, wire-omitted, and settings-only — with documented rationale; cross-projection chain when ingress and wire diverge: canonical → wire → decode → materialize must succeed; canonical → ingress reverse fixture → materialize only on documented test-only paths; polymorphic families add per-arm samples in Cartesian coverage when cross-arm law requires; nested sub-owners inherit parent arm conditioning in generative strategies; seam metamorphic law: foreign fixture corpus → anti-corruption `materialize_*` → local canonical → local wire → decode → local `materialize_*` when foreign-local bijection is policy — otherwise one-way foreign-to-canonical proof only.
- **Layer 5 — runtime boundary** — runs after metamorphic suite; `beartype` on adapters post-materialization; integration smoke asserts diagnostic routing for injected ingress, wire, seam, and migration defects; interior domain beartype stays secondary.
- **Layer 6 — mutation and drift** — executes last; Stryker on adapter `materialize_*` and remap tables when seam logic is dense — concept-axis mutation surfaces are secondary; import-linter custom rules flag wire importing ingress, domain importing boundary adapter implementation, and receipt models duplicating fold output; mutation testing on discriminant routing requires kill ratio on `match` arms — mutants defaulting to catch-all must fail exhaustiveness type-check or contract tests; integration drift signals gate merge before concept-axis-only proof debt.
- **Role map coverage** — every concept maps to exactly one stack layer and at most one active lattice projection; checklist or arch rule fails on orphan ingress without canonical owner or canonical without egress path.
- **Stage-to-layer alignment** — each materialization stage name pairs with one stack layer artifact; mixed-type handoffs between construction and enrichment indicate binding drift.
- **Fold totality** — receipt-stream reducers are total per `StrEnum` kind or return explicit `Result` unhandled arms — `None` collapse is a binding defect, not a domain edge case.

# OpenAPI, Schema Drift, And Failure Archaeology

- Pydantic discriminated unions export JSON Schema `oneOf` plus `discriminator` from model declarations — hand-maintained OpenAPI discriminator tables parallel to `Annotated[..., Discriminator(...)]` are merge blockers.
- Schema diff tests compare generated OpenAPI or JSON Schema snapshots on ingress model changes — discriminator mapping drift fails CI before runtime discovery.
- Wire-only tags absent from public API contracts stay in adapter documentation — when ingress and wire are intentionally identical, published schema reflects ingress discriminators; dual publication of wire-only tags without ingress mirror is drift.
- Settings models excluded from public OpenAPI still generate internal schema snapshots when they fan out to boot configs — env key renames fail schema diff on settings slice.
- `RootModel` with `Literal` root in union members appears as const or enum schema arms — wrapper-only schema nodes hiding discriminant are static defects.
- Failure attribution follows owner-first routing: `ValidationError` at ingress projection → boundary adapter and ingress owner; `msgspec.ValidationError` at decode → wire projection owner; smart constructor `Result` error → canonical owner; `BeartypeCallHintViolation` → call-site surface or missing materialization before interior entry.
- Duplicate-validation failures surface when the same invariant message appears from Pydantic and smart constructor — harness runs duplicate-validation checklist per concept; attribution targets secondary owner for removal per duplicate-validation test.
- Stage mismatch failures at adapter rejection include stage expected and stage received in boundary fault payload — attribution targets projection stage map, not domain enrichment logic.
- Seam remap failures attribute to anti-corruption adapter and remap table — foreign token in fault payload maps to table row or explicit unmapped bucket; domain `match` on foreign strings indicates context leak.
- Migration failures attribute to migration fold owner and version literal — domain modules after successful materialization never emit `MigrationFault`.
- Handoff correlation inherits settings-declared run and context identifiers at composition root — seam adapter spans tag exporting and importing context names without embedding canonical field payloads in trace attributes.

# Operational Diagnostics Across Integration Handoffs

- Diagnostic evidence tags stack layer, projection role, and seam identity on every boundary fault — `ingress`, `wire`, `seam`, `migration`, or `materialize` discriminator in structured fault payloads, not undifferentiated validation messages.
- Trace decorators on seam adapters emit span attributes for exporting context, importing context, schema version, and materialization stage at handoff — interior domain spans omit foreign codec identifiers.
- Cross-context handoff logs record canonical type name and adapter function at exit and entry — receipt stream append events carry slot/kind enum members, not raw handler strings.
- Integration smoke tests assert diagnostic routing: injected ingress violations surface at ingress owner, injected wire decode failures at wire owner, injected remap gaps at seam owner — conflated attribution fails the smoke module.
- Observability vocabulary aligns with catalog and dispatch `StrEnum` rows — diagnostic tags reuse token owners; ad hoc log strings for concepts already enum-owned are drift signals.

# Property Harness And CI Gates

- Hypothesis strategies draw from closed registry rows keyed by `StrEnum` or materialized exemplars — `st.sampled_from` over lawful canonical instances per arm, not `st.from_type` on closed unions without registered exemplars or unfiltered text filtered only by runtime validation.
- Composite strategies mirror nested `match` structure — outer discriminant conditions inner arm generation; independent random tags across hierarchy levels are rejected.
- Law registration associates integration proofs with port symbols, vocabulary owners, or adapter modules via `register_law` — witnesses bind to the integration owner, not pytest file paths.
- `@spec` and law-matrix decorators on integration tests preserve collected signatures — hypothesis `@given` removes strategy parameters from pytest-visible signatures per decorator-admitted-shape rules.
- Shrinking preserves discriminant legality — custom composite builders rebuild valid canonical values after shrink; invalid tag mutations fail at construction gate, not as accepted counterexamples.
- Fold totality laws assert receipt-stream reducers handle every `StrEnum` kind or return explicit `Result` unhandled arms — `None` collapse from fold is harness failure, not domain edge case.
- Adding a canonical field without ingress mirror, wire projection, or documented adapter omission fails role-map coverage checklist — orphan canonical fields without egress path block merge.
- Adding a variant arm without vocabulary table, migration fold, OpenAPI mapping, hypothesis registry row, and exhaustive `match` update fails static and contract gates together.
- Ruff policy bans `TypeGuard` and `cast` on family and integration modules — tag-check escapes fail lint before behavioral tests.
- Encoder determinism for cache and fingerprint keys pins `order="deterministic"` at module-level encoder singleton — metamorphic proof uses the same encoder instance as production hot paths.

# Lattice Drift, Collapse Tests, And Anti-Patterns

- Drift is detectable when projections silently diverge from canonical semantics; treat these signals as integration defects, not local fixes.
- **Parallel field names** — ingress and canonical share semantics but use different unmapped names without adapter documentation.
- **Double validation** — same rule appears in Pydantic field validator and smart constructor.
- **Tag mismatch** — wire `tag_field` string differs from Pydantic discriminator literal for the same variant without seam remap table.
- **Optional inversion** — field optional on ingress, required on canonical, with default injected in domain constructor instead of adapter.
- **Enum sprawl** — same token set declared in ingress model, wire struct comments, and domain enum; collapse to vocabulary owner.
- **Projection import cycle** — wire module imports ingress module or domain imports boundary adapter implementation details.
- **Layer projection inversion** — domain module imports msgspec or defines ingress `BaseModel`; move codec to boundary adapter and restore canonical-only interior.
- **Stage type leak** — enrichment transform accepts ingress model or wire struct; insert materialization between validation and enrichment.
- **Rail on projection** — `Result` bind chains operate on `model_dump` dicts or wire structs; rematerialize to canonical before interior bind.
- **Registry before materialize** — handler invoked on raw string or foreign dict before `materialize_*` completes.
- **Counter parallel to fold** — mutable or frozen counter model mirrors stream fold output; delete counter, keep fold-derived receipt projection.
- **Decorator inside model** — validation or trace decorator on smart constructor or `model_validator` interior; move to surface or boundary adapter per stack order.
- **Root domain import** — composition root imports foreign `Registry` or fold helper from another context's interior; import adapter and protocol only.
- **Context leak** — foreign `BaseModel` or msgspec struct appears in domain transform signatures.
- Collapse tests verify remediation:
- **Dual owner per concept** — ingress model and canonical record both enforce the same cross-field rule; collapse rule to canonical, thin ingress to mirror optionality only.
- **Wire as domain** — domain transform accepts msgspec struct; collapse to `project_*_wire` inverse at boundary, canonical-only interior.
- **Settings in constructor** — smart constructor reads `os.environ`; collapse to settings slice plus root `project_*_config` handoff.
- **Seam skip** — context B imports context A domain record directly; collapse to seam adapter plus canonical handoff at root.
- **Receipt as state** — receipt struct duplicates canonical fields for query convenience; collapse to fold projection from fact stream over canonical snapshots.
- **Stringly seam** — foreign token normalized in domain `match`; collapse to adapter `frozendict` remap plus enum construction.
- Evolution anti-patterns to reject:
- Domain smart constructor branching on `schema_version` or foreign wire layout — collapse to adapter migration fold before materialization.
- Hand-maintained OpenAPI discriminator tables diverging from Pydantic declarations — collapse to generated schema with diff gate.
- Migration mapping unknown stored tags to default variant arm — fail closed with `MigrationFault` or typed `Extension` capture.
- Skipping metamorphic proof for polymorphic cache slots because same-process — enrichment and manual assembly still drift discriminators.
- Version bump updating wire struct without migration fold or contract samples — partial promotion leaves stores unreadable.
- Generating hypothesis samples via unfiltered text filtered by runtime validation — wastes cycles and misses lawful shrink paths.
- Harness suppressions or cast escapes at integration seams — proof debt belongs on language axis; fix owner typing instead.
- Cross-axis `StrEnum` declared in ingress model, wire struct, and domain module without vocabulary owner — collapse to vocabulary axis first, then bind edges.
- Concept axis redefining stack layer order or inventing a projection role outside the lattice — reject before merge; integration doctrine owns taxonomy.
- Integration doctrine restating pydantic-core, replacement tier, or capability intersection mechanics — bind edge only on concept axis.
- Promoting concept-axis proof law into integration binding block — mechanics stay on concept owners.
- Harness or CI gate validating concept-axis interior law before integration role-map coverage — reorder proof composition per integration layer ordering.
- Standard section naming package brands as shape owners — package boundaries name admission sites, not canonical concepts.
- Remediation order: consolidate vocabulary, move rule to canonical owner, thin secondary projection, add or fix seam adapter — never add a third parallel type.

# Doctrine Authority And Precedence

- **Integration wins on ownership** — when a concept axis proposes a new shape layer, projection role, validation gate, or seam altitude, the proposal must map to exactly one row in stack taxonomy or projection lattice; otherwise the proposal is rejected until integration doctrine assigns the binding.
- **Concept axis wins on mechanics** — pydantic-core compilation, replacement algebra, capability intersection joins, decorator phase taxonomy, payload `Unpack` preservation, and variant-family polymorphic persistence stay on their owning axis; integration references binding edges only.
- **Single invariant owner** — duplicate-validation test arbitrates cross-axis conflicts: when two axes enforce the same rule on different shapes, integration assigns ownership to the canonical layer; the secondary axis thins to projection mirror or deletes the validator.
- **Vocabulary unification** — token, sentinel, and discriminant closure defaults to vocabulary-absence axis; integration binds import-at-edge policy; concept axes do not declare parallel `StrEnum` rows without vocabulary-axis promotion unit.
- **Composition-root supremacy** — cross-context wiring, settings fan-out, protocol scope maps, and trusted-replay pins default to integration assembly rules; concept axes document context-local boot slices only.
- **Evolution coupling** — version envelope, simultaneous projection update, and metamorphic proof obligations gate all concept-axis promotions that touch ingress, wire, or seam edges; partial promotion without integration contract row is a merge blocker.

# Concept Axis Ownership

- **class-family-variant-architecture** — owns `@tagged_union`, discriminated union arms, sub-owner nesting, exhaustive `match` on variant payloads, and polymorphic persistence proof; integration owns dispatch registry keys on materialized canonical types and variant graduation from parallel optional fields.
- **decorator-admitted-shape-composition** — owns decorator phase taxonomy, `**P` preservation, and surface altitude tables; integration owns outer stack order `trace > authorize > validate > cache > govern > retry` and decorator gate on `Result` rails.
- **immutable-persistent-replacement-shapes** — owns `copy.replace`, `msgspec.structs.replace`, `model_copy`, tier routing, and hash-stability proof; integration owns mutable-transition graduation and enrichment stage placement after materialization exit.
- **model-materialization-pipeline** — owns stage carrier vocabulary, ingress-to-materialization sequencing detail, and consumer seam algebra for wire envelopes; integration owns materialization stage-to-stack-layer alignment and single validation gate.
- **protocols-capabilities-structural-ports** — owns capability lattice, mirror extraction, conformance witnesses, structural admission decorators, and capability intersection joins; integration owns protocol stack role, seam altitude binding, and interior trust without `isinstance` repair.
- **pydantic-domain-shape-engine** — owns pydantic-core schema compilation, `TypeAdapter` orchestration, and strict ingress engine mechanics; integration owns Pydantic package boundary, ingress projection role, trust-posture admission, and OpenAPI drift gates.
- **rich-class-owner-design** — owns smart constructors, post-init replay pressure, enrichment identity, boundary morphism on rich owners, and validated transition assembly; integration owns record-to-rich-class graduation, enrichment stage after construction, and phantom stage threading on projection edges.
- **typed-payload-contract-surfaces** — owns `TypedDict` closure, `Unpack`, `ReadOnly`, `NotRequired`, keyword-callable seam preservation, and composition-root payload wiring proof; integration owns payload surfaces as boundary ingress carriers only.
- **vocabulary-absence-state-encoding** — owns `Literal`, `Option`, `sentinel`, absence FSM mechanics, and seam remap table ownership on declared closure; integration owns vocabulary stack layer, absence semantics ownership, and vocabulary sync across projections.

# Integration Completeness

- Every concept in every bounded context maps to exactly one stack layer and at most one active lattice projection — role-map coverage passes with zero orphan ingress or canonical without egress path.
- Every invariant has exactly one enforcing owner — duplicate-validation checklist passes per concept; no Pydantic and smart-constructor duplication for the same rule.
- Every foreign or chooser view binds through an explicit adapter edge — no projection imports another projection as definition source; no domain module imports msgspec or Pydantic past boundary stubs.
- Every context seam has one anti-corruption `materialize_*` per foreign pair — vocabulary remap tables are total on declared closure; domain `match` on foreign strings is absent.
- Every composition root declares typed handoff graph, settings fan-out, and protocol scope map — no foreign domain interior or file-local registry imports at root.
- Every persistence and wire handoff carries closed `schema_version` with read-path migration fold — canonical owners do not branch on version; write-path emits current version only.
- Every vocabulary or variant promotion updates ingress discriminator, wire tag, contract table, and fold arm in one unit — partial promotion is a merge blocker.
- Every integration proof layer executes static → contract → metamorphic → runtime → mutation in composed order — failures at the static layer block expensive generative suites.
- Every boundary fault payload tags stack layer, projection role, or seam identity — diagnostic routing smoke passes for injected ingress, wire, seam, and migration defects.
- Collapse tests pass — no dual owner per concept, wire-as-domain, settings-in-constructor, seam skip, receipt-as-state, or stringly seam remains in the bounded context.
- Every concept axis retains mechanics inside integration-assigned stack role, lattice binding, and evolution gates — concept owners do not redefine taxonomy or projection roles.
