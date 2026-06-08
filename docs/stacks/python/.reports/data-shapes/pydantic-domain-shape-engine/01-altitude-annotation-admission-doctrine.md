# Altitude And Annotation Admission Doctrine

# Altitude Triage Matrix

- Every Pydantic owner occupies exactly one altitude: composition-root boot, ingress-projection, canonical interior, or boundary-pipeline adapter — altitude is a shape decision, not a folder convention.
- Composition-root altitude owns `BaseSettings`, pinned `TypeAdapter` constants, settings fan-out slices, adapter warm-up, and `@validate_call` on root ingress handlers — schema compilation pays once per process at this altitude only.
- Ingress-projection altitude owns wire-visible admission: discriminated ingress models, env-backed partial shapes, CLI argument models, and versioned envelope parsers — validators here enforce law foreign carriers must satisfy before `materialize_*`.
- Canonical altitude owns durable concepts after validation exit: frozen `BaseModel` subclasses with `@computed_field`, cross-field `model_validator(mode='after')`, and rich-owner methods — interior modules consume these as trusted shape artifacts without re-ingress.
- Boundary-pipeline altitude owns `model_validate` / `model_validate_json` on foreign carriers, `ValidationError` capture, and Pydantic-to-msgspec `project_wire` — executes in adapter modules the root imports, never inside rail interior folds.
- Promote a shape upward when invariant density, computed wire fields, JSON Schema publication, or env-backed configuration require Pydantic richness at the durable owner — demote to ingress-only when the concept is foreign-carrier normalization with no interior lifecycle.
- Demote to boundary-only when the concept never survives past `materialize_*`, never publishes OpenAPI, and never holds `PrivateAttr` or effectful methods — ingress model plus canonical non-Pydantic owner is the default demotion outcome.
- Altitude violations — `@field_validator` on canonical owners duplicating ingress wire law, `BaseSettings` in domain modules, `TypeAdapter` inside interior transforms, ingress models imported transitively through domain packages — are seam defects.

# Boundary-Only Versus Canonical Ownership

- Pydantic owns the full shape when the concept needs any of: published JSON Schema, discriminated union routing, `@computed_field` on the durable owner, `schema_version` on persisted envelopes, env-backed `BaseSettings` fields, or cross-field admission invariants that must replay on store re-read.
- Pydantic owns boundary-only when the durable owner is msgspec, dataclass, or rich non-`BaseModel` class and Pydantic's job ends at foreign-carrier admission — one ingress model per concept, one `materialize_*` smart constructor, zero parallel canonical `BaseModel`.
- Boundary-only ingress models carry wire law exclusively: alias routing, coercion, discriminant literals, and `model_validator(mode='before')` envelope unwrap — they must not host `@computed_field`, `PrivateAttr`, or domain algorithms that survive past materialization.
- Canonical Pydantic owners carry law that must survive interior transitions and optional egress projection — wire-only normalizers belong on ingress models or `Annotated` stacks, not duplicated on canonical owners.
- Ingress-as-canonical is permitted only when the bounded context documents the ingress model as the durable owner for that family — default posture rejects ingress-as-canonical to prevent domain packages importing boundary definitions and reintroducing import cycles.
- When wire shape and domain shape intentionally diverge, boundary-only ingress plus canonical msgspec or dataclass owner is mandatory — dual Pydantic models for the same concept at different altitudes require an explicit adapter row naming promotion direction.
- `RootModel` and module-level `TypeAdapter` at ingress altitude substitute for named boundary models when the wire form is a single root value or a reused union alias — they do not graduate to canonical altitude without field growth that demands a named owner family.
- Custom types with `__get_pydantic_core_schema__` on non-`BaseModel` classes keep Pydantic at the slot boundary while canonical behavior stays on the rich class — Pydantic does not become the canonical owner merely because a field embeds a cooperatively typed class.

# Python 3.15 Deferred Annotation Compilation

- Python `>=3.15` defaults to PEP 649 deferred evaluation through `__annotate__` thunks (PEP 749 `annotationlib` is the canonical read path) — raw `__annotations__` on metaclass owners may raise `NameError` for forward names or return stale eager caches; Pydantic `>=2.13.3` routes introspection through `annotationlib` on Python 3.14+ (`safe_get_annotations` uses `Format.FORWARDREF` before full resolution).
- `annotationlib.get_annotations(owner, format=annotationlib.Format.VALUE)` is the runtime admission read — `Format` members are `VALUE` (1), `VALUE_WITH_FAKE_GLOBALS` (2, internal), `FORWARDREF` (3), `STRING` (4); never pass legacy `inspect` format integers.
- Admission certificate at root warm-up: VALUE-read field keys must match `cls.model_fields` keys after class-body execution and any required `model_rebuild()` — mismatches signal deferred-evaluation drift, missing rebuild, or post-hoc `__annotations__` mutation outside `Field` specifiers; certificate failure blocks worker and subinterpreter publish, not domain retry.
- `Annotated` stack parity uses `typing.get_type_hints(cls, include_extras=True)` beside VALUE reads — `annotationlib.get_annotations` has no `include_extras` parameter; stripping `Annotated` metadata in parity proofs is a merge blocker on ingress and settings owners.
- `annotationlib.Format.FORWARDREF` admits schema-build and test-collection reads that must not evaluate unresolved peers — returns `annotationlib.ForwardRef` proxies for undefined names; forward-ref model families still require quoted annotations plus `model_rebuild()` before first production validation; FORWARDREF does not substitute for rebuild.
- `annotationlib.Format.STRING` preserves source-text annotations for signature renderers and static export tooling — runtime validation and `GenerateSchema` require VALUE at first-touch warm-up; STRING snapshots do not discharge compiled-graph proof.
- `annotationlib.call_annotate_function` and `get_annotate_from_class_namespace` are the metaclass-safe reads during class construction — Pydantic metaclass synthesis may consult annotate thunks before the class object is fully materialized; post-body surgery on `model_fields` without redefinition invalidates the certificate.
- PEP 695 `class Owner[T](BaseModel)` and `type Alias = Annotated[...]` bind type parameters at definition; generic parametrization (`Owner[int]`) produces distinct `model_fields` and `__pydantic_core_schema__` per argument tuple — decorator caches, `TypeAdapter` pins, and persistence keys must include `get_args(cls)` and `cls.__type_params__`, not erased generic names.
- PEP 747 `TypeForm[T]` types module-level `TypeAdapter` constants, settings field type expressions, and adapter registry keys — `TypeForm(x)` is identity at runtime; public root exports annotate concrete specializations or PEP 695 aliases until checker plugins align, never bare `object` or erased `Any`.
- Repo policy bans legacy `TypeVar`, `Optional`, and `from __future__ import annotations` on Pydantic owner modules — `X | Y` unions and PEP 695 syntax are the only generic and optional forms on shape-engine modules; `from __future__ import annotations` forces stringified semantics PEP 749 will eventually remove.
- Ruff `runtime-evaluated-base-classes` and `runtime-evaluated-decorators` require annotations resolve to real objects before `__pydantic_core_schema__` freezes — deferred evaluation shifts *when* names resolve, not *whether* VALUE reads must succeed at warm-up.
- PEP 814 `frozendict` is the canonical immutable mapping for settings tables, registry rows, and annotation-metadata folds at composition-root altitude — not a `dict` subclass; hashable when keys and values are hashable; comparison ignores insertion order; mutable `dict` fields on frozen models remain a nested-mutability concern.
- `pydantic` minor upgrades pin behind admission certificate replay: VALUE key parity, `include_extras` hint parity on representative owners, dual-mode JSON Schema diff, and root import-graph oracle — compilation success alone does not discharge merge.

# Decorator Stack Integration

- Pydantic shape decorators are class-body admission, not call-time policy — `@field_validator`, `@model_validator`, `@computed_field`, and `model_config` execute during metaclass synthesis; `@beartype` and `@validate_call` execute at call-time on operational surfaces derived from admitted owners.
- Class-body stack order is load-bearing: `model_config` / `ConfigDict` first, field annotations with `Annotated` validators second, `@field_validator` third, `@model_validator` fourth, `@computed_field` properties last — normalization before cross-field law before derived wire slots.
- Callable policy stack outer-to-inner remains `trace > authorize > validate > cache > govern > retry > operation` — `@validate_call` sits in the validate band; `@beartype` sits after synthesis on handlers that receive materialized canonical types, not raw ingress carriers.
- `@validate_call` on root handlers admits function-boundary shape when arguments are already typed domain owners or annotated scalars — prefer over duplicating a `BaseModel` wrapper whose sole job is admitting function arguments; `.raw_function` and `.validate_call` are trusted fast paths only beside documented tier rows.
- `@beartype` on rich-owner methods enforces call-time contracts on materialized instances — it does not replace Pydantic admission on foreign dicts; stacking `@beartype` on `@field_validator` bodies is rejected because validators are classmethods on the synthesis surface.
- `PrivateAttr` admits mutable implementation state excluded from schema, validation, and serialization — caches, handles, and session tokens never pair with `@computed_field` or `Field` on the same logical slot.
- `dataclass_transform` on custom factories does not apply to `BaseModel` subclasses — Pydantic metaclass is the synthesis owner; competing transforms on one inheritance chain enter undefined checker behavior and are rejected.
- Catalog decorators (`@register`, `@spec`, `singledispatch.register`) append rows at module-import phase and return the same class or callable — registry mutation is not ingress validation; handler bodies must already admit canonical types at decoration time.
- Decoration phases are not reorderable: import-time catalog rows must not depend on call-time validation; call-time wrappers must not mutate `model_fields` or `__pydantic_core_schema__` — schema changes require redeploy and class redefinition.
- Double-decoration guards key on `__wrapped__` and decorator identity — repeated `@validate_call` or `@beartype` application on the same symbol must return the existing admitted wrapper unchanged.

# Settings Shape At Boot Altitude

- `BaseSettings` is composition-root altitude exclusively — domain modules receive the frozen validated instance injected from root scope; constructing `BaseSettings()` in leaf modules hides `settings_customise_sources` order defects and is a root boot defect.
- Settings shapes own configuration law, not domain algorithms — nested `BaseSettings` submodels validate at parent construction with `extra='forbid'`; fan-out `project_*_config` slices read canonical field values from the validated parent, never pre-validation env maps.
- `settings_customise_sources` ordering is a deployment contract pinned at root boot — init → env → dotenv → file secrets is default; production env-only paths drop dotenv explicitly; collision precedence requires fixture matrices, not single-key happy paths.
- `Field(validation_alias=AliasChoices(...))` on settings fields admits legacy env names at construction — normalization belongs in `field_validator(mode='before')`, not in custom source classes that bypass compiled field law.
- `SecretStr` / `SecretBytes` never cross logging, public OpenAPI, or root diagnostic dumps — settings JSON Schema is operator-facing only; client-published OpenAPI omits settings owners unless every field is secret-free by policy review.
- Nested `Field(default_factory=SubSettings)` with `nested_model_default_partial_update=True` merges env deltas at construction only — not on materialized `model_copy`, parent transition methods, or domain enrichment folds.

# Ingress-To-Canonical Handoff

- Handoff is a typed step, not a cast: ingress model validates foreign carriers; `materialize_*` at root or boundary adapter constructs the canonical owner from the validation exit instance — raw dict promotion past validation exit is rejected regardless of field-name overlap.
- Ingress validators must not encode business truth that canonical owners re-prove differently — if removing an ingress validator leaves canonical construction unable to enforce wire safety, the rule was canonical law wrongly placed at ingress altitude.
- Canonical `model_validator(mode='after')` invariants assume fully constructed instances — they must not call context-bound projections requiring parent settings not yet bound at ingress altitude.
- Cross-field ingress invariants on partial env submodels (`ArtifactBackend()` in isolation) belong in sub-owner `mode='after'` validators provable from stored fields alone — parent-dependent laws belong on canonical methods with explicit typed context parameters.
- `materialize_*` return types are canonical owners — ingress model types must not appear in domain module signatures except at adapter exports named in the root seam map.

# Dual-Engine Altitude At The Seam

- Pydantic at ingress altitude and msgspec at egress altitude is the default dual-engine posture — duplicate shape owners in both engines without a root conformance row is rejected.
- Boundary-only ingress plus msgspec canonical is the preferred demotion when volume egress dominates and JSON Schema publishes struct tags, not Pydantic models — OpenAPI reflects ingress discriminators; wire-only tags live in adapter documentation rows.
- Canonical Pydantic plus msgspec wire projection applies when interior law requires Pydantic richness and volume lanes require struct layout — projection occurs once at `project_wire` with declared `model_dump` mode; domain interiors never hold both types for the same concept.
- Ingress from msgspec decode into Pydantic re-enters when ingress adds coercion, alias, or cross-field law beyond struct closure — struct decode never substitutes for Pydantic admission on those rows.

# TypeAdapter And Union Altitude

- Module-level `TypeAdapter[T]` as `Final` singleton is root or ingress altitude — polymorphic validators for bare unions, `Annotated` aliases, `TypedDict` ingress, and primitives reused across handlers belong here, not on canonical owners unless the union is the canonical concept itself.
- Constructing `TypeAdapter` inside handlers, request loops, or property-test inner loops recompiles `GenerateSchema` work and is rejected at every altitude — hot paths import pinned adapter symbols from root `[CONSTANTS]`.
- Generic parametrized adapters (`TypeAdapter(Box[int])`) cache per concrete argument tuple at root — call sites import `ADAPTER_BOX_INT`, not `TypeAdapter(Box)(int)` expressions that defeat cache identity.
- Discriminated unions not embedded in a parent model own routing in the adapter schema — callable `Discriminator` plus `Tag('case')` at ingress altitude keeps union branches out of caller `match` ladders; canonical altitude embeds the same union as a field only when the concept is durably polymorphic interior-side.
- `TypeAdapter.json_schema()` publishes contract truth for adapter-owned unions — hand-maintained parallel discriminator tables beside adapter declarations are ingress-altitude defects when root schema export is enabled.
- `validate_python` versus `validate_json` choice is trust-lane, not altitude — bytes and str ingress use `validate_json`; dict and ORM views use `validate_python`; both exit through the same boundary capture projection.

# Canonical Projection Tiers

- Canonical Pydantic rich owners admit three projection tiers on one class: stored fields, `@computed_field` (schema + serialization + settings export), and stdlib `@property` (interior-only, invisible to wire and JSON Schema).
- Promote interior `@property` to `@computed_field` only when the fold uses stored fields alone and must appear in schema, settings export, or operator diagnostics — context-bound folds needing typed runtime parameters remain methods, not computed fields.
- Filesystem normalization, anchored path resolution, and IO-touching folds belong in validators or context-bound methods — never `@computed_field`, because schema export and static introspection must not execute IO or bake stale anchors.
- `exclude_computed_fields` on dumps is an egress knob at boundary altitude — interior transitions must not rely on dumps excluding computed evidence that validators assume on full snapshot re-validate.
- cyclopts and CLI param owners remain dataclass altitude until a decorator admits both CLI projection and Pydantic schema without tier collapse — settings export and CLI binding split owners persist as an explicit seam row.

# Version Arms And Migration Altitude

- `schema_version: Literal[...]` on canonical or envelope owners is canonical-altitude law — version is part of the admitted shape, not metadata stored only outside validation.
- Ingress altitude may carry forward-compatible `extra='ignore'` on versioned wire envelopes — closed canonical interiors and boot settings stay `extra='forbid'` unless a promotion unit explicitly widens ingress.
- Read-path migration folds run at boundary altitude once: stored layout → `model_validate` → migration expression → current canonical owner — obsolete arms live in root migration modules with exhaustive folds, not in active domain logic.
- Breaking discriminant, alias, coercion, serializer, computed-field, default, or settings source-order changes require version bumps and promotion-unit regeneration — altitude moves (ingress demotion or canonical promotion) trigger the same unit, not isolated field edits.

# JSON Schema Obligation By Altitude

- Ingress families that publish client contracts carry dual-mode JSON Schema snapshots at ingress altitude — validation mode for write requirements, serialization mode when computed fields or serializer transforms define public egress.
- Canonical families publish schema only when they are the published API owner — boundary-only ingress with msgspec canonical publishes ingress validation-mode schema and struct wire tags separately; conflicting vocabularies in one OpenAPI document are rejected.
- `WithJsonSchema` and `json_schema_input_type` overrides stay on `Annotated` type aliases at the owning altitude — post-processing `model_json_schema()` output duplicates law already compiled into `__get_pydantic_json_schema__` hooks.
- JSON Schema is a projection, not a generative oracle — when `BeforeValidator` or `model_validator(mode='before')` widens accepted shapes beyond annotations, admission fixtures and core-schema walks own generative truth; schema snapshots own published contract truth.

# Worker Boot, Subinterpreter, And Admission Certificate

- Worker entrypoints re-import the root module graph or a documented subset that warms every Pydantic owner on the worker path — compiled `SchemaValidator` identity is process-local; parent-process warm-up does not cross spawn seams.
- PEP 734 subinterpreters (`concurrent.interpreters`) treat each interpreter as an independent composition root — pinned `TypeAdapter` singletons, `BaseSettings` instances, and admission certificates from the parent interpreter are not portable object references; child interpreters rerun root warm-up and replay the annotationlib VALUE certificate before publishing handler entrypoints.
- Subinterpreter and multiprocessing handoff at boundary altitude transmits `model_dump` bytes plus schema-version metadata — never pickle compiled validators, `SchemaValidator` handles, or settings singletons across interpreter or process seams unless trusted-replay rows document encoder identity and version pins.
- Admission certificate replay in worker boot: for every owner on the worker path, VALUE field keys match `model_fields`, required forward-ref families call `model_rebuild()` before first validation, and pinned adapter symbols resolve without per-call `TypeAdapter()` construction — certificate failure is a root boot defect.
- `model_rebuild()` for forward-ref families runs in worker boot when shared packages import after partial initialization — worker-first validation without rebuild is a root wiring defect, not a domain retry concern.
- `BaseSettings` construction completes before worker threads read the frozen instance — lazy settings accessors on hot paths race under parallel importers and free-threaded root graphs.
- Free-threaded importers treat compiled validators as immutable after root warm-up — post-import class surgery races; altitude or field-set changes require redeploy, not runtime mutation.
- Cross-process handoff at boundary altitude prefers `model_dump` + `model_validate` when schema version enforcement matters — pickle and deepcopy bypass version gates unless trusted-replay rows document encoder and version pins.

# Settings Sources And Env Namespace Law

- Custom `PydanticBaseSettingsSource` subclasses own exotic ingress at root boot — implement `get_field_value` and `prepare_field_value`; keep I/O inside the source, not in field validators that would run without source context on non-env ingress.
- Nested settings submodels inherit independent validation — parent boot fails closed when child `extra='forbid'` catches env namespace leakage; piecemeal nested construction in domain modules bypasses namespace closure.
- `populate_by_name=True` on settings admits alias and field name on env ingress — canonical fan-out reads Python field names from the validated instance; alias law is fully consumed at construction.
- `case_sensitive` and `env_nested_delimiter` are shape decisions declared in `SettingsConfigDict` at root — changing them is a breaking deployment contract requiring regenerated collision matrices.

# Promotion And Demotion Signals

- Promote ingress to canonical when any interior module needs the type after `materialize_*`, when persisted stores key the layout, when `@computed_field` or `PrivateAttr` carry load-bearing law, or when OpenAPI names the owner as the durable read model — absence of all four signals keeps ingress boundary-only.
- Demote canonical to boundary-only when interior consumers need only msgspec or dataclass carriers, when no published schema names the Pydantic class, when validators encode wire-only alias or envelope law, and when `materialize_*` is the sole consumer — retain one ingress model and delete the canonical `BaseModel` duplicate in the same promotion unit.
- Split altitude when one class mixes env parsing, wire alias law, and post-materialization algorithms — settings fields and wire validators graduate to ingress or root boot slices; methods and `PrivateAttr` graduate to canonical or rich non-Pydantic owners in one collapse pass.
- `pydantic-settings>=2.14.0` treats settings as compiled `BaseModel` graphs with layered sources — altitude decision is unchanged: settings remain root boot only; version bumps do not relocate settings into domain modules.

# Anti-Patterns

- Canonical owner carrying wire alias law duplicated from its ingress sibling; ingress model with `@computed_field` or `PrivateAttr`; `BaseSettings` constructed outside root boot; `TypeAdapter` or `TypeAdapter(Box)(int)` at call sites; domain import of ingress `BaseModel` for typing; raw `__annotations__` reads or mutation instead of `Field` specifiers and `annotationlib` VALUE parity; `include_extras=False` in admission certificate proofs on `Annotated` ingress owners; catalog registration substituting for boundary validation; call-time decorator mutating `model_fields`; settings `model_dump` on stdout from root emit paths; pickle or parent-process validator reuse across worker or subinterpreter seams; promoting altitude without updating `materialize_*` return types and root adapter rows in the same promotion unit.
