# Pydantic v2 Shape Ownership

# Critical Signals

- Altitude placement and deferred-annotation admission certificate bind at composition-root warm-up — each owner occupies exactly one altitude (`composition-root`, `ingress-projection`, `canonical`, `boundary-pipeline`); `annotationlib.Format.VALUE` field-key parity against `model_fields` after `model_rebuild()` is the compile-time admission certificate that must replay before worker and PEP 734 subinterpreter publish; altitude violations and certificate drift block merge at root import smoke, not domain retry.
- Three proof surfaces are orthogonal — runtime admission (`model_validate` / `TypeAdapter`), compiled core-schema oracle (`__pydantic_core_schema__` walk), and dual-mode JSON Schema projection — passing one does not discharge the others; field-snapshot equality without core-graph replay is false-green.
- Cross-process and subinterpreter handoffs re-materialize through root-pinned `TypeAdapter` bytes — compiled `SchemaValidator` identity and generic parametrization (`Box[int]` vs `Box[str]`) do not cross spawn seams without vocabulary and schema-version parity at receiving root boot.

# Shape Engine Doctrine

- Pydantic v2 is the compiled shape engine for boundary admission, settings, discriminated unions, computed wire fields, and JSON Schema-rich contracts; validation is one projection of shape ownership, not the product definition.
- The durable runtime artifact is `__pydantic_core_schema__` and its JSON Schema mirror — static checkers read annotations, but shape law lives in the compiled core graph warmed once per process.
- Pydantic owns field-set closure, alias mapping, discriminator routing, coercion and strict policy, computed wire projections, JSON Schema emission, ingress `ValidationError` surfaces, and frozen immutability of declared slots at boundaries.
- Rich class owners own effectful operations, `PrivateAttr` caching, algorithmic transforms on validated data, persistence mapping when ORM shape differs, and expression-rail error handling beyond `ValidationError`.
- Workspace engine split: Pydantic for settings, boundary validation, discriminated unions, computed wire fields, and JSON Schema-rich ingress; msgspec for hot encode/decode, frozen struct egress, and volume lanes — evaluate `msgspec.Struct` at boundary volume, promote cross-family shapes once at the projection edge, and duplicate shapes only with explicit adapter rows.
- If the concept needs JSON Schema, discriminated unions, computed wire fields, or env-backed settings — Pydantic owns the shape; if the concept needs zero-copy encode at volume with a fixed struct layout — evaluate `msgspec.Struct` and adapt at the boundary.
- Pydantic does not own high-throughput JSON serialization lanes, structural capability contracts (`Protocol` ports), or decorator-dispatch registries — those are adjacent axes.
- Each `BaseModel` subclass materializes a `pydantic-core` schema at class body execution; hot validation and serialization delegate to Rust-backed nodes (`str_schema`, `model_schema`, `union_schema`, `chain_schema`, `discriminator`, `computed-field`, etc.) on the `>=2.13.3` release train with vendored core.
- Custom types participate via `__get_pydantic_core_schema__(source_type, handler)` — `handler.generate_schema(T)` must be preferred over `handler(T)` to avoid leaking `Annotated` context into unrelated parameter schemas.
- `GetCoreSchemaHandler` exposes `field_name` for metadata that must vary per field; generic owners use `get_args(source_type)` on `source_type` (not `cls`) to recover type parameters at compile time.
- Hypothesis strategy walkers traverse `__pydantic_core_schema__` to honor `pattern`, `multiple_of`, and nested item constraints that naive `from_type` generators miss.
- Treat models as immutable shape carriers at domain interiors: `ConfigDict(frozen=True)` plus `extra="forbid"` closes the instance surface and rejects silent field drift — the repo pins this posture for settings and artifact-backend models.
- With `extra='forbid'`, `__pydantic_extra__` is absent; `extra='allow'` populates it and breaks closed-shape doctrine; `extra='ignore'` is for forward-compatible ingress only at true versioned boundaries.
- After `model_validate` succeeds, downstream treats the instance as a trusted immutable record — effects, `PrivateAttr` caching, and cross-aggregate orchestration belong on the rich class owner after admission, never inside validators.
- Distinguish boundary models (wire/config ingress) from interior rich owners: Pydantic owns exact field sets, coercion policy, and serialization projections at boundaries; domain behavior that survives past materialization belongs on the rich class owner, not in validator side effects.

# Core Schema Compilation

- Schema compilation executes at class body completion for `BaseModel`, `RootModel`, and module-level `TypeAdapter`; `GenerateSchema` produces a frozen `CoreSchema` stored on `__pydantic_core_schema__`.
- Decorators register metadata during construction but invoke only at validation or serialization — compilation captures hook references, not hook side effects.
- Compilation is single-pass per class unless `model_rebuild()` or generic parametrization invalidates the cache — imported models are ready for validation at import time; lazy first-touch compilation is not a supported optimization path.
- `SchemaValidator` instances behind models and adapters are immutable after compilation — thread-safe reuse across workers without per-call rebuild.
- `model_config = ConfigDict(...)` merges along the MRO when multiple bases declare config — restrict Pydantic inheritance to single-inheritance chains; diamond or multi-base `ConfigDict` merges silently and can produce ambiguous strictness or alias policy.
- Forward references and recursive models defer nodes until `model_rebuild()` resolves names in the defining namespace; `model_rebuild(_types_namespace=...)` accepts an explicit namespace when models are defined inside factories or `exec` contexts where `globals()` is insufficient.
- Rebuild resolves type names only — not field-set mutations; field changes require a new class definition and re-import, not repeated rebuild in production paths.
- First `model_validate` before rebuild raises `PydanticUserError` with an unresolved forward-ref message; `RootModel` forward refs on the root type parameter follow the same contract.
- Generic parametrization (`Box[int]`) produces distinct compiled schemas and validators per concrete argument tuple — parametrized subclasses of generic bases inherit rebuilt parent schemas.
- Ruff `runtime-evaluated-base-classes` and `runtime-evaluated-decorators` require annotations resolve to real objects at class definition time — forward references require quoted annotations plus `model_rebuild()`, not deferred string-only stubs without rebuild.
- Module-level `Adapter: Final[TypeAdapter[T]] = TypeAdapter(T)` pays schema compilation once per process — constructing `TypeAdapter` inside hot loops or per-request handlers recompiles `GenerateSchema` work and is rejected.
- Compiled `__pydantic_core_schema__` on imported models is ready at import time — warm adapters and models at composition-root startup when cold-start latency matters.
- `model_rebuild()` invalidates forward-ref-deferred schemas only — it does not refresh runtime validator caches after field-set changes; field mutations require a new class definition and re-import, not repeated rebuild in production paths.

# Discriminated Union Families

- Closed variant families are the primary Pydantic shape pattern: a parent field holds a `Union` of sibling `BaseModel` cases routed by a discriminator so static types, runtime validation, and JSON Schema `oneOf` stay aligned.
- Field discriminators require a `Literal`-typed discriminant on each case; Pydantic 2.13 extends this to `RootModel` roots whose discriminator is itself a `Literal`.
- Callable `Discriminator(fn)` plus `Tag('case')` maps non-uniform keys when a single field name cannot own routing — nested unions wrap inner `Annotated[..., Field(discriminator=inner)]` before outer routing; failure at any level is a typed `ValidationError` on the discriminated path.
- Alias the closed union (`type Pet = Annotated[Union[Cat, Dog], Field(discriminator='pet_type')]`) and reuse across parents, `TypeAdapter` surfaces, and schema generation to prevent union duplication drift.
- Model family inheritance preserves validators, `model_config`, and custom methods on parametrized subclasses the same way as ordinary `BaseModel` inheritance — variant cases are siblings under a shared base, not ad hoc dict branches.
- Published OpenAPI for discriminated unions derives from Pydantic model declarations — `Literal` discriminant fields on each case drive JSON Schema `oneOf` plus `discriminator.mapping`; hand-authored parallel discriminator tables diverge from runtime routing and are rejected.
- Callable `Discriminator` plus `Tag('case')` arms document computed routing through member `title` and tag metadata — OpenAPI consumers see the same tag vocabulary as `TypeAdapter` routing at runtime.
- Nested discriminated unions export nested `oneOf` under outer discriminator properties — each level is a separate routing table; contract tests must sample failures at every level, not only the outer discriminant.
- Every published arm appears in validation-mode `oneOf` with distinct discriminant mapping; exhaustiveness proofs use `assert_never` on interior `match` over materialized domain types after Pydantic admission — schema exhaustiveness and domain exhaustiveness are separate obligations; both must pass.
- Extension or catch-all arms (`extra='allow'`, untagged fallback unions) require explicit policy rows when published — undocumented open unions break client codegen and discriminator sync.
- Sub-owner decomposition (nested discriminated families as product fields) publishes independent schema fragments per sub-owner — top-level OpenAPI composes `$ref` to sub-schemas; each sub-owner maintains its own proof matrix.
- Closed union families in public API export a single aliased type — duplicate union spellings across parents drift discriminator metadata in generated schema.
- Adding an arm without updating OpenAPI and conformance samples is a partial contract publish.
- `RootModel` discriminators on `Literal` roots (2.13+) emit root-type schema directly — OpenAPI must not wrap an object envelope around root-form wire shapes.
- Callable discriminators on root-adjacent ingress models are not yet unified with cyclopts choice metadata in one generative schema export — dual maintenance persists until root publishes a single OpenAPI source for CLI and HTTP.

# RootModel, TypeAdapter, And Module Singletons

- `RootModel[T]` owns shapes whose wire form is the root value itself — `root` is the sole validated slot and serializes without a wrapper key; JSON Schema emits the root type directly.
- Use for newtype-style containers (`RootModel[list[str]]`, `RootModel[dict[str, str]]`) where the domain concept is "a validated X" without named fields — not as a shortcut around named `BaseModel` families.
- `RootModel.model_validate`, `model_dump`, and `model_dump_json` delegate to the root schema; shallow-copy semantics copy the `root` value (2.13 fix) so mutating nested mutable roots remains a domain concern outside Pydantic's frozen shell.
- `TypeAdapter(T)` compiles any type form into the same core machinery without a wrapper class — the polymorphic validator for unions, `Annotated` aliases, and bare primitives reused across functions, CLI parsers, and test fixtures.
- `validate_python` / `validate_json` / `dump_python` / `dump_json` mirror model methods; prefer over hand-rolled `isinstance` chains for union routing.
- `TypeAdapter` is the right owner for discriminated unions not embedded in a parent model — keeps routing logic in the schema, not in caller branches.
- Cache adapters at module level as `Final` singletons — constructing `TypeAdapter` inside hot loops or per-request handler bodies recompiles `GenerateSchema` work and is rejected.
- `TypeAdapter.json_schema()` and `TypeAdapter.core_schema` / `TypeAdapter.validator` expose compiled graph and underlying `SchemaValidator` for contract generation, micro-benchmarking, and oracle parity.

# Validators And Annotated Vocabulary

- `@field_validator` modes partition responsibility: `before` transforms raw input, `after` refines core-validated values, `wrap` sandwiches core logic, `plain` replaces core validation — cross-slot law belongs in `@model_validator(mode='after')`, not scattered `before` hooks.
- `@model_validator(mode='before')` reshapes the raw input dict before per-field work; `mode='after'` enforces cross-field invariants on a fully constructed instance.
- Validators are shape contract, not domain algorithms: normalization, bounded invariant checks, and wire-safety scrubbing only — no I/O or registry mutation inside hooks.
- `json_schema_input_type` on `field_validator` (modes `before`, `plain`, `wrap`) documents the wire type JSON Schema should advertise when Python annotations differ from accepted input — use for stringly-typed env values coerced to ints.
- `Annotated` functional validators (`BeforeValidator`, `AfterValidator`, `WrapValidator`, `PlainValidator`) compose right-to-left for reusable type-level normalizers.
- `Field(ge=, le=, min_length=, pattern=, description=)` and `annotated-types` constraints (`Gt`, `Len`, `Predicate`) compile into inspectable core nodes; custom validator bodies are opaque to schema walkers and require explicit `register_type_strategy` when semantic law exceeds core nodes.
- `Annotated[T, ...]` is the canonical stackable shape layer: `Field` constraints, functional validators, serializers, and `WithJsonSchema` ride on one type parameter without subclass proliferation.
- `PlainValidator` bypasses inner validation for the annotated type — reserve for trusted transforms where core type checking would reject intentional representations.

# Serializers And Wire Projection

- `@field_serializer` and `@model_serializer` own egress shape separately from ingress validation — validation types and wire types may diverge by design.
- Serializer modes mirror validators: `plain` replaces default output; `wrap` delegates to `handler(self)` then post-processes — use `wrap` when default model dump is the base and only projection layers differ.
- `Annotated` serializers (`PlainSerializer`, `WrapSerializer`) define reusable wire encodings on type aliases.
- `when_used='json'` limits transformation to JSON mode while preserving Python runtime types.
- `SerializeAsAny[T]` widens serialization to runtime subclasses while validation still treats the field as `T` — cross-engine round-trip proof for polymorphic detail slots requires encode → decode → re-validate before cache or cross-process handoff.
- Pydantic 2.13 `polymorphic_serialization` in `ConfigDict` addresses `serialize_as_any` edge cases in nested unions — prefer explicit config over ad hoc pre-dump dict hacking.
- `@computed_field` on `@property` declares derived slots participating in serialization and JSON Schema (`readOnly: true`) without constructor inputs — pure functions of declared fields only; excluded from `model_construct` inputs and ingress validation.
- `@computed_field` serializers are not supported — computed values serialize via their property return type; customize via wrapping `model_serializer` if wire form must differ.
- `@property` without `@computed_field` is invisible to wire and schema — use for pure domain views that must not leak to serializers.
- JSON Schema `mode='serialization'` includes computed fields in `required` when they are always emitted — document this for consumers that treat schema `required` as input requirements.
- Do not split one concept into parallel DTO plus domain class when a single frozen `BaseModel` can own both — DTO duplication is only justified when wire shape and domain shape intentionally diverge.

# ConfigDict And Coercion Policy

- `frozen=True` makes instances immutable after validation and enables hashing when `__hash__` is not overridden — all fields must be hashable; unhashable field types disable hashing unless excluded via custom `__hash__`.
- Default equality is field-wise on `BaseModel` — two validated instances with identical fields compare equal.
- `populate_by_name=True` admits both alias and field name for settings ingress; lax mode (default) coerces JSON-friendly scalars at wire boundaries; interior trusted paths prefer `strict=True` per-call or class-level with selective `Field(strict=False)` exceptions for stringly env keys.
- `model_validate(data, strict=True)` applies strict mode without mutating class config — strict mode is a shape contract decision, not a performance knob; pair with `extra="forbid"` for closed payloads.
- `validate_assignment=True` re-validates on attribute set (non-frozen models) — rarely needed when `frozen=True` is the default posture.
- `arbitrary_types_allowed=True` is an escape hatch — prefer `__get_pydantic_core_schema__` on rich types.
- Wire encodings for datetime, bytes, timedelta, and uuid belong in `model_config` (`ser_json_timedelta`, `ser_json_bytes`, etc.), not per-call dump flags.
- `from_attributes=True` ORM construction is a boundary adapter lane — persistence row objects enter at validation once; interior domain code receives the validated model, not live ORM instances.

# Generic Models And Parametric Families

- Parametric model families inherit `BaseModel` then `Generic[T]` (that order), declare type parameters via PEP 695 `class Box[T](BaseModel)` or `TypeVar`, and use parameters in field annotations — Pydantic 2.11+ fully supports PEP 695 syntax and defaults.
- `model_rebuild()` replaces v1 `update_forward_refs()` and must run only after all referenced types are defined — recursive generic models need explicit rebuild before first validation.
- Parametrized subclasses inherit `model_config`, validators, and methods from the generic definition — `Box[int]` and `Box[str]` share one validator body, distinct compiled schemas.
- Subclasses of generic models must also inherit `Generic` to preserve parametrization through the hierarchy — otherwise type parameters erase at intermediate bases.
- `SerializeAsAny` and `SerializeAsAny[T | U]` control serialization width for unparametrized type variables — validation uses the bound; serialization can widen to runtime instances.
- Pydantic 2.13 allows `Field(default_factory=...)` on generic models to receive validated model data as a factory argument — use for defaults that depend on sibling fields after partial validation.
- `__get_pydantic_core_schema__` on generic domain classes (non-`BaseModel`) enables field embedding without `arbitrary_types_allowed=True` — the class cooperates with schema compilation while retaining its own runtime identity.
- Generic parametrization version arms (`Box[int]` vs `Box[str]`) compile distinct schemas — persistence keys must include concrete type arguments, not erasure-friendly generic names alone.

# Materialization And Trust Lanes

- Admission path is chosen by trust posture, not convenience: untrusted carriers enter through `Model(**data)`, `model_validate`, `model_validate_json`, or `TypeAdapter.validate_python` / `validate_json` — never through `model_construct`, `.raw_function`, or `SkipValidation` on boundary-facing keys.
- `model_validate_json` parses JSON then validates — prefer over `json.loads` + `model_validate` for single-pass schema enforcement.
- `model_construct` skips validation for trusted-replay only — probe stores, in-session validated graphs, and trusted-replay caches qualify when upstream proved shape legality via validation, pinned encoder identity, and schema version matching the write path; foreign wire, process boundaries without version gates, and pickle/`copy.deepcopy` paths re-admit through `model_validate`.
- Prefer `model_dump` + `model_validate` when schema version tags must be enforced at re-admission.
- `SkipValidation` on `Annotated` marks interior slots upstream already closed on trusted same-process artifacts — never on boundary-facing keys.
- `validate_call` compiles function signatures into admission schemas — positional, keyword, `*args`, and `**kwargs` participate when annotated; supports `config=ConfigDict(strict=True)` per function for stricter CLI entrypoints.
- `.validate_call` re-runs validation without invoking the body — proves admission independent of effects.
- `.raw_function` mirrors `model_construct` as interior fast path — public API surfaces call the validated wrapper only; prefer `validate_call` over duplicating a `BaseModel` wrapper whose sole job is admitting function arguments.
- `model_dump` / `model_dump_json` project validated state; `model_copy(update={...})` and `copy.replace` are trusted same-owner transitions assuming the base instance passed full validation at materialization — wire-sourced or external deltas re-enter at Tier V through `model_validate` regardless of field type match; document the tier in the owning transition method when callers cannot infer trust from ingress versus domain context.
- Tier V (re-validated): wire-sourced, computed, or cross-boundary deltas always re-enter through `model_validate` / `TypeAdapter.validate_python` even when field annotations match the incoming value — shallow `model_copy(update=...)` cannot replay `field_validator` or cross-field `model_validator` law.
- Trusted same-owner field swaps use `model_copy` / `copy.replace` without re-validation — document the tier in the owning transition method when callers cannot infer trust from ingress versus domain context.
- Nested mutable containers inside frozen models remain mutable unless field types are immutable collections — re-validation Tier V applies when replacing nested content from foreign sources, not when mutating trusted in-session subgraphs already isolated by deep copy.
- Child model re-admission before parent replace: foreign nested dicts validate through child schema (`Child.model_validate(nested)`) before `model_copy(update={'child': validated})` — never assign raw nested dicts into frozen parents.
- Per-call `strict=True` on `model_validate` / `TypeAdapter.validate_python` overrides class `ConfigDict` for a single boundary crossing without mutating the compiled schema.
- `model_copy(deep=True)` on frozen models clones nested models and containers for isolation transitions; shallow copy shares nested mutables; frozen parent does not imply deep immutability.
- Validation context (`context={...}`) threads through functional validators — use for admission metadata (tenant, correlation) without storing it on the model.
- Pickle and `copy.deepcopy` traverse validated models but bypass explicit version gates — not a substitute for dump+validate when foreign processes write the store.
- Settings models validate once at process boot — hold the frozen instance on the composition root; domain code receives the validated settings object, not live `os.environ` reads or repeated `BaseSettings()` construction.

# Alias Routing And Field Surface

- Three alias surfaces partition key law: `validation_alias` (ingress), `serialization_alias` (egress), and bare `alias` (both when others unset); `AliasChoices` and `AliasPath` admit versioned wire envelopes without `model_validator` dict surgery — first matching path wins.
- `AliasPath("parent", "child")` validates nested dict paths as flat field slots — the shape engine walks the path; manual nested `.get()` chains in adapters duplicate routing already owned by `Field`.
- `serialization_alias` without `validation_alias` emits one wire key while accepting only the field name on ingress — use when outbound contracts rename keys but inbound sources are canonical.
- `model_dump(by_alias=True)` applies serialization aliases; `model_copy` and `copy.replace` accept Python field names only — normalize aliased ingress before replacement transitions.
- Discriminant fields need explicit `validation_alias` when wire keys differ from field names; callable discriminators bypass field-name alias lookup.
- `Field(exclude=True)` removes slots from dumps while keeping validation and JSON Schema unless combined with generator overrides.
- `Field(repr=False)` hides fields from `repr()` without affecting serialization — distinct from `PrivateAttr`, which excludes validation and schema entirely.
- `Field(init=False)` omits constructor population — pair with `model_validator(mode='after')` or `default_factory` for derived init-only slots; `copy.replace` does not carry `init=False` fields unless explicitly restated.
- `Field(validate_default=True)` runs validators on default values at class definition — use when defaults must satisfy the same constraints as ingress.
- `Field(frozen=True)` on a mutable container field prevents reassignment of the field attribute while container contents may still mutate — not a substitute for `tuple`/`frozenset` immutable collection types in domain interiors.

# Settings And Secrets Shape

- `BaseSettings` compiles the same core schema as `BaseModel` then layers `PydanticBaseSettingsSource` providers — env, dotenv, secrets dir, and init kwargs merge into one frozen instance; domain code receives the validated settings object, not live `os.environ` reads.
- `SettingsConfigDict(env_prefix=..., env_nested_delimiter='__', case_sensitive=...)` maps nested models — nested `BaseSettings` fields validate sub-shapes independently; submodel `extra='forbid'` catches env namespace leakage.
- `settings_customise_sources` returns an ordered tuple of sources — leftmost wins on key collision unless a source opts out; default order is init → env → dotenv → file secrets; drop dotenv in production paths that must be env-only; ordering is part of the deployment contract.
- `Field(validation_alias=AliasChoices('LEGACY_KEY', 'CANONICAL_KEY'))` on settings fields admits multiple env names — normalization belongs in `field_validator(mode='before')`, not in source classes.
- `SecretStr` / `SecretBytes` redact `repr()` and default serialization — wire dumps must use explicit `model_dump(mode='json')` policy when secrets cross logging boundaries; prefer never serializing settings models; published OpenAPI for settings models omits secret values; examples use placeholders.
- Settings JSON Schema is internal or operator-facing only — do not publish settings shapes in public client OpenAPI unless every field is client-safe and secret-free.
- Custom `PydanticBaseSettingsSource` subclasses own exotic ingress — return them from `settings_customise_sources`; implement `get_field_value` and `prepare_field_value`; keep I/O inside the source, not in field validators; prove `get_field_value` / `prepare_field_value` behavior with fixture files.
- Nested settings submodels inherit `extra='forbid'` — env namespace leakage surfaces at construction with mapped violations, not lazy domain raises.
- `settings_customise_sources` ordering is part of the deployment contract — document collision precedence beside the settings owner; conformance tests matrix env key collisions.

# Custom Type Schema Hooks

- `__get_pydantic_core_schema__(cls, source_type, handler)` is the single custom-type admission contract — return a `CoreSchema` node or call `handler.generate_schema(inner)` to delegate; prefer `handler.generate_schema(T)` over `handler(T)` to avoid leaking `Annotated` context — `handler(T)` preserves `Annotated` context for the parameter.
- `handler.field_name` branches metadata per embedding site — stricter bounds or alternate coercion on the same Python type at different field sites.
- `__get_pydantic_json_schema__(core_schema, handler)` adapts compiled nodes to JSON Schema — keep JSON overrides there, not post-processed `model_json_schema()` output; core schema stays transport-agnostic.
- `__get_pydantic_core_schema__` on non-`BaseModel` domain classes embeds rich types without `arbitrary_types_allowed=True` — the class keeps behavior; Pydantic owns slot validation and dump hooks.
- `PlainValidator` / `WrapValidator` on `Annotated` are preferred over subclassing when the transform is a function, not a type identity.
- `pydantic_core.core_schema` builders (`no_info_plain_validator_function`, `with_info_wrap_validator_function`, `union_schema`, `chain_schema`) serve multi-step core logic when `Annotated` validators are insufficient.

# Rich Class Owner Integration

- The rich class owner pattern: Pydantic materializes and freezes the shape; the same class (or a composed owner) carries methods that consume validated fields — validators enforce wire safety, methods enforce domain law.
- Keep `@model_validator(mode='after')` for invariants that are part of admission (non-empty roots, traversal-free paths, mutual field bounds); move operational logic (path resolution, subprocess env projection) to methods on the validated instance.
- `PrivateAttr` holds runtime state excluded from schema, validation, and serialization — caches, handles, and session tokens never appear in `model_dump` or JSON Schema.
- When a domain class is not a `BaseModel` but must appear as a field type, implement `__get_pydantic_core_schema__` so Pydantic owns validation/serialization of its slots while the class owns behavior — avoid duplicating the shape as a parallel DTO.
- Hand off after materialization: once `model_validate` succeeds, downstream code treats the instance as a trusted immutable record — no re-validation on read paths.

# Boundary Admission And Dual-Engine Handoff

- Pydantic owns the validation gate exit artifact — frozen instance with coercion and `extra` policy applied; downstream normalization and construction stages must not re-run Pydantic on the same carrier or promote raw `json.loads` dicts past validation — stage-skip regardless of apparent field shape requires explicit documented trust posture; convenience `model_construct` on untrusted carriers is never an exemption.
- Ingress stage entry accepts `bytes`, `str`, or adapter-scoped mapping views; Pydantic exit is the first durable typed instance.
- Normalization may reshape alias-resolved field storage but must not widen the admitted field set — `model_validator(mode='before')` belongs at validation exit, not as a second validation pass in domain modules.
- Construction and enrichment consume validation exit types as trusted-for-shape, not trusted-for-business-truth — domain smart constructors and `model_validator(mode='after')` invariants still run where business law exceeds wire law.
- Stage-skipping exemptions (ingress dict → domain owner without validation owner) require explicit composition-root documentation — convenience `model_construct` in leaf modules is never an exemption.
- Cross-family promotion (Pydantic ingress model → msgspec wire struct) occurs once at the boundary projection edge — route through `msgspec.convert` with explicit policy or adapter-owned field tables; domain interiors never hold both engines' types for the same concept.
- Pydantic-to-msgspec projection prefers `msgspec.convert(instance, StructType)` or adapter `project_wire(domain)` over `model_dump` → dict surgery → `Struct(...)` — dump keys, alias policy, and serializer transforms must not be re-encoded manually at the seam.
- `model_dump(mode='python')` feeds `convert` when Python runtime types match struct field types; `mode='json'` when wire scalars differ (Decimal→float, datetime ISO strings) — pick mode from struct field annotations, not from caller habit.
- Ingress from msgspec decode into Pydantic re-enters at validation: `decoder.decode(bytes)` → `model_validate(decoded)` when the ingress model adds coercion, alias, or cross-field law beyond struct closure — never assume struct decode substitutes for Pydantic admission.
- `CodecFault` and boundary `Result` rails own both engines' failures — map Pydantic `ValidationError` and msgspec `ValidationError` to the same violation tuple shape at capture; neither propagates past the adapter.
- Each cross-engine seam declares an explicit conformance row: source owner, projection expression (`project_wire`, `msgspec.convert`, field-explicit struct construction), target struct, dump mode (`python` vs `json`), and alias/rename policy.
- Field-level conformance maps every serialized key through one of: direct name match, `serialization_alias` → struct field rename, serializer transform with documented target type, or computed-field exclusion from wire struct — unmapped dump keys fail conformance review.
- Tagged union conformance requires per-arm samples in proof gates — polymorphic detail slots and `SerializeAsAny` fields inherit the same encode-decode-revalidate loop as closed unions.
- `Decimal`, `datetime`, `UUID`, and bytes fields declare mode explicitly: `model_dump(mode='json')` when struct fields expect wire scalars; `mode='python'` when struct fields accept Python runtime types — pick mode from struct annotations, not caller habit.
- Nested model fields project through child conformance rows before parent struct assembly — foreign nested dicts validate through child Pydantic schema (`Child.model_validate`) before parent wire projection, matching re-admission tier law.
- Conformance failures map to boundary `Fault` or `Result` at the capture site — never coerce to default variants, silent key drops, or `None` substitution unless the row explicitly declares optional polymorphism.
- Round-trip proof form for adapter-owned unions: `TypeAdapter.validate_python(decoder.decode(encoder.encode(materialized)))` with equality on the re-validated instance — catches serializer width mismatches validation alone misses.
- Round-trip proof gates apply when Pydantic ingress models feed msgspec egress structs with tagged unions, `SerializeAsAny` detail slots, or polymorphic serialization config — encode through the module-level wire codec and decode back before cache write or cross-process handoff.
- Settings and env-backed models skip wire round-trip proof unless they also project to msgspec egress — their proof is single-pass `BaseSettings()` construction with frozen `extra='forbid'`.
- Discriminated union round-trip must preserve discriminant wire keys under alias policy — proof failures often indicate `serialization_alias` / `validation_alias` drift between ingress model and egress struct `rename` policy.
- Alias policy on discriminant fields must appear consistently across ingress schema, egress struct `rename` policy, and round-trip proof samples.

# ValidationError Boundary Projection

- `ValidationError.errors()` returns dicts with `type`, `loc`, `msg`, `input`, `ctx`, and optional `url` — `loc` is a tuple of str/int/step indices tracing the failure path through models, lists, dicts, and union branches.
- Boundary adapters wrap admission in `capture` and project into `CodecFault.violations: tuple[str, ...]` via `'.'.join(str(p) for p in e['loc'])}: {e['msg']}` — never forward raw `ValidationError` into domain modules or `Result` interiors without mapping.
- Union failures concatenate branch errors with distinct `loc` prefixes — consumers must not assume a single-root `loc`; batch ingress surfaces every violation; operator logs may truncate after mapping.
- `error_count()` and `errors(include_url=..., include_context=...)` filter payload size at boundary extraction.
- Default validation collects all field violations in one pass.
- `ValidationError.json()` is the machine-readable 422 body — prefer over `str(exc)` when clients need machine-readable violation paths.
- `from_exception_data(title, line_errors, input_type=...)` constructs errors for custom boundary adapters that still want Pydantic-shaped diagnostics without running full validation.
- `PydanticCustomError(type, message_template, context_dict)` raised inside validators produces typed `type` codes (`path_traversal`, `empty_root`, etc.) mapped through the same projection — use for domain-coded admission failures that must survive boundary mapping.
- `validate_call` failures prefix `loc` with parameter names — apply identical mapping rules as model validation inside the same boundary module.
- Settings construction failures (`BaseSettings` init) are boundary defects, not runtime domain exceptions — fail at settings load with mapped violations instead of lazy raises inside build rails past the `Result` boundary.

# JSON Schema And OpenAPI Contract

- `model_json_schema()` and `TypeAdapter.json_schema()` accept `mode='validation'` (default) or `mode='serialization'` — validation mode documents accepted ingress shapes; serialization mode documents emitted egress shapes including computed fields.
- Store validation-mode and serialization-mode JSON Schema snapshots as paired artifacts per owner family — CI diffs both on `pydantic` upgrades; deterministic set ordering (2.13+) stabilizes diffs but does not eliminate review obligation.
- Validation-mode schema omits computed fields from input requirements; serialization-mode schema may list computed fields under `required` when always emitted — document this split for OpenAPI consumers that treat `required` as write requirements.
- Validation-mode snapshots document accepted ingress shapes: field requirements, `pattern`/`ge`/`le`, discriminant literals, and `json_schema_input_type` overrides when wire types differ from Python annotations.
- Serialization-mode snapshots document emitted egress shapes: computed fields under `required`, serializer-transformed scalar formats, and `SerializeAsAny` widened properties — OpenAPI consumers must not treat serialization `required` as write requirements without explicit documentation.
- `schema_generator=GenerateJsonSchema` subclass hooks customize ref naming, union representation, and constraint emission at compile time — override per family rather than post-processing dict output.
- `$ref` deduplication and deterministic set ordering (Pydantic 2.13) stabilize schema diffs across releases — pin regeneration when upgrading `pydantic`; moved definitions appear as ref churn, not semantic churn — review ref renames separately from constraint changes.
- `WithJsonSchema({...})` on `Annotated` types and `Field(json_schema_extra=...)` attach vendor extensions without touching Python types — keep overrides on the type alias, not duplicated per embedding field; overrides appear in both modes unless mode-specific generator hooks exclude them.
- `json_schema_input_type` on `field_validator` and `WrapValidator`/`PlainValidator` metadata overrides the wire type shown in validation-mode schema when Python annotations differ from accepted input.
- `ConfigDict(json_schema_extra=...)` applies model-level OpenAPI metadata — version stamps, deprecation notices, and composite examples at the owner level.
- JSON Schema from `model_json_schema()` is a projection, not a round-trip spec — do not generate ingress fixtures from schema alone when `BeforeValidator` or `model_validator(mode='before')` widens accepted shapes beyond annotations.
- When ingress Pydantic shapes intentionally differ from msgspec egress tags, OpenAPI reflects ingress discriminators only — wire-only tags belong in adapter documentation rows, not duplicated in public schema as conflicting vocabularies.

# Proof Surfaces And Property Oracles

- Pydantic shape owners carry three independent proof surfaces: runtime admission (`model_validate` / `TypeAdapter.validate_python`), compiled core-schema oracle (`__pydantic_core_schema__` walk), and published contract projection (`model_json_schema` / `TypeAdapter.json_schema`) — each must pass; passing one does not discharge the others.
- Runtime proof samples `resolve(T).example()` through full validation in a loop — walker-generated instances must survive admission; failures signal validator opacity or schema drift that compilation-only checks miss.
- Core-schema oracle proof walks `definitions`, `tagged-union`, `chain`, and `definition-ref` nodes — property tests register `st.register_type_strategy(Model, strategy)` when validator bodies encode law beyond core nodes; absence of registration is an explicit waiver row in the owner module.
- Contract proof snapshots both JSON Schema modes: `mode='validation'` for ingress write contracts and `mode='serialization'` when computed fields or serializer transforms define the public egress contract — single-mode snapshots under-approximate published API truth.
- Cross-engine proof applies when Pydantic ingress models project to msgspec wire structs: `encoder.encode(project_wire(domain))` → `decoder.decode` → `model_validate` or struct equality per arm — validation width and wire width may diverge by design; proof must cover the declared seam policy row.
- Settings and secrets models prove single-pass construction at composition-root boot — `BaseSettings()` with frozen `extra='forbid'`; they do not participate in wire round-trip proof unless they also egress through msgspec lanes.
- `st.from_type(Model)` under-generates: native resolution drops `pattern`, `multiple_of`, nested item constraints, and discriminator routing that `__pydantic_core_schema__` exposes — walk via `_pyd_node`, honor `tagged-union` choices, unwrap `function-after`/`function-before`/`function-wrap` to inner constraint nodes, and resolve `definition-ref` via `st.deferred` for recursive models.
- The `_pyd_node` walker over `__pydantic_core_schema__` is the canonical oracle: recurse `definitions`, honor `tagged-union` choices, unwrap `function-after`/`function-before`/`function-wrap` to inner constraint nodes, and resolve `definition-ref` via `st.deferred` for recursive models.
- Validator bodies (`@field_validator`, `@model_validator`) are opaque to schema walkers — when semantic law exceeds core nodes (acyclicity, traversal-free paths, regex-plus-context rules), register `st.register_type_strategy(Model, strategy)` before `resolve(T)` so generated instances survive admission.
- `resolve(T)` idempotently registers engine-appropriate strategies: Pydantic models via `_pyd_node`, msgspec structs via `msgspec.inspect` `_node`, then returns `st.from_type(T)` — one entrypoint for polymorphic test fixtures.
- JSON Schema snapshots and schema-walk oracles are complementary, not interchangeable — schema documents ingress contract; core-schema walk documents runtime admission.
- Inline snapshots of `model_json_schema()` catch accidental contract drift in review — pair with `mode='serialization'` snapshots when computed fields are part of the public wire contract; pair with core-schema oracle loops when `BeforeValidator` or `model_validator(mode='before')` widens accepted shapes beyond annotation-inferred schema.
- `validate_call` surfaces prove admission independent of body effects via `.validate_call` on decorated functions.
- CI oracle gates sample `resolve(Model).example()` through `model_validate` in a loop — walker-generated instances must pass admission before merge; failures signal schema/validator drift that compilation-only snapshots miss.
- Hypothesis shrink on constrained models must stay within oracle-generated admissible set — shrinking toward annotation-only values fails validators opaque to core nodes.

# Version Arms And Evolution

- Long-lived persisted owners carry `schema_version: Literal[...]` on owner or envelope — version is part of the admitted shape, not metadata stored only outside validation.
- Read-path migration folds run once at the boundary: `StoredVn` → `model_validate` → migration expression → current owner — domain modules after migration see only the current shape; obsolete arms remain in migration folds with explicit exhaustiveness, not in active domain logic.
- Migration folds prefer `model_validate` over `model_construct` unless store key, encoder identity, and trusted-replay pin prove legality — version skew and silent default changes require full validator replay.
- Breaking field removals, discriminator vocabulary changes, and coercion-policy tightening are version bumps — document each bump in the conformance row and regenerate dual-mode schema snapshots.
- Breaking discriminant vocabulary change — new `Literal` value or renamed tag: bump version arm; migration fold maps legacy tags; update OpenAPI discriminator mapping and msgspec wire tags in the same change.
- Breaking coercion policy change — `strict=True` adoption or removal of `BeforeValidator` widen: ingress fixtures that passed under lax policy may fail; treat as breaking for external consumers even when Python annotations are unchanged.
- Breaking alias routing change — `validation_alias`, `serialization_alias`, or `AliasChoices` mutation: breaking for any consumer keyed on wire paths; run round-trip proof under old and new alias samples.
- Breaking serializer transform change — `@field_serializer`, `PlainSerializer`, or `when_used='json'` policy: breaking for egress consumers; serialization-mode schema snapshot must diff.
- Breaking computed field contract change — adding or removing `@computed_field`: breaking for serialization-mode schema and OpenAPI read models; not breaking for validation-mode ingress unless consumers incorrectly treated computed keys as writable.
- Breaking default or `default_factory` change — `Field(default=...)`, generic `default_factory` receiving model data (2.13): breaking for dump+validate re-admission and cross-process replay.
- Breaking settings source order change — `settings_customise_sources` reorder: breaking for deployment environments relying on collision precedence.
- `validate_call` signatures are schema sources — parameter annotations drive compiled admission; erased or overly wide annotations (`Any`, bare `dict`) fail static-runtime alignment for public API surfaces.
- `Field(exclude=True)`, `PrivateAttr`, and `@property` without `@computed_field` partition visibility: static checkers read annotations; JSON Schema generators read schema metadata — alignment rows in the owner module document what appears in each projection.
- Annotations on model fields must match runtime schema intent — mypy plugin catches drift on optional defaults, constrained aliases, and validator-decorated fields; runtime-only fixes without annotation updates fail static proof.
- `json_schema_input_type` on validators documents wire types for static OpenAPI generators and runtime schema — Python annotation, validator input type, and accepted ingress sample must agree or document intentional divergence in the owner module.

# Anti-Patterns

- Validator I/O or registry mutation inside hooks; `model_construct` on untrusted input for perceived performance — throughput belongs in schema design, module-level adapter caching, and trusted stores, not admission bypass; `model_dump` → manual dict rewrites → `model_validate` as a light transform — alias and serializer law already live in the compiled schema; duplicate `BaseModel` wrappers when `validate_call` suffices; double validation on the same boundary crossing — single validation gate per crossing, interior trusts validation exit artifacts; propagating `ValidationError` into `Result` interiors without mapping.

# Platform Integration

- Workspace targets `requires-python >= 3.15` with `pydantic>=2.13.3` and `pydantic-settings>=2.14.0` — PEP 695 type parameters and `X | Y` unions are the only generic syntax; legacy `TypeVar`/`Optional` imports are policy-banned.
- Pydantic 2.13 baseline adds Python 3.14+ alignment fixes, `polymorphic_serialization`, recursive generic model fixes, deterministic JSON Schema set ordering, and merged `pydantic-core`.
- `pydantic.mypy` plugin aligns static checking with runtime schema; Hypothesis `resolve(T)` walks the engine that owns the shape under test.
- Ruff classifies `BaseModel`, `RootModel`, `BaseSettings` as runtime-evaluated base classes and `field_validator`, `model_validator`, `computed_field`, `validate_call` as runtime-evaluated decorators — imports and annotations resolve at runtime for schema compilation.

# Root Pydantic Assembly

- Python `>=3.15` binds Pydantic shape owners at composition roots so settings boot, module-level adapters, ingress models, and proof harnesses share one compiled schema graph per process — roots warm validators once, fan validated settings inward, and route cross-axis handoffs through named adapter entries, not leaf-module `BaseSettings()` or per-request `TypeAdapter` construction.
- Composition roots declare module-level `TypeAdapter[T]` and `BaseSettings` subclasses as `Final` singletons — schema compilation pays at import or explicit root warm-up, not inside handler bodies, request loops, or property-test inner loops.
- Root import graph loads every `BaseModel` family that participates in ingress, settings, or public API contracts — forward-ref models call `model_rebuild()` from root `__init__` or conftest session hooks before first production validation; unresolved refs at handler bind are root wiring defects.
- Ingress model modules stay boundary-adjacent — domain interior folders import canonical owners and root-projected boot records, not sibling ingress `BaseModel` definitions, unless the bounded context explicitly documents ingress-as-canonical for that family; ingress model modules imported transitively through domain packages reintroduce cycles — keep ingress definitions boundary-adjacent and bind through root adapter imports named in `__all__`.
- Plugin hosts that register new Pydantic model classes after root import lack a published rebuild and adapter warm-up protocol — dynamic class bodies do not refresh root `TypeAdapter` caches without explicit registration hooks.
- `BaseSettings()` constructs exactly once per process at root boot — domain modules receive the frozen validated instance from root scope; repeated settings construction in leaf modules hides `settings_customise_sources` order defects and env collision precedence bugs.
- Generic parametrized adapters (`TypeAdapter(Box[int])`) cache per concrete argument tuple at root `[CONSTANTS]` — hot paths import the pinned adapter symbol, not `TypeAdapter(Box)(int)` at call sites.
- `pydantic.mypy` plugin alignment runs in CI on the same module set the root imports — static proof on model fields and validator signatures is a root-graph obligation; models not reachable from root import smoke are orphan shape owners.

# Stack Altitude And Owner Placement

- Composition-root altitude — `BaseSettings`, pinned `TypeAdapter` constants, settings `project_*_config` fan-out, adapter warm-up, and `validate_call` on root ingress handlers; `ConfigDict` anchors declared once beside root constants.
- Ingress-projection altitude — discriminated Pydantic ingress models, env-backed partial shapes, and CLI argument admission models; validators here own wire-visible law only; durable domain behavior graduates to canonical owners after `materialize_*`.
- Canonical altitude — frozen `BaseModel` subclasses that durably own a concept carry `@computed_field`, cross-field `model_validator(mode='after')`, and rich-owner methods; interior modules consume these instances as validation exit artifacts without re-ingress.
- Boundary pipeline altitude — `model_validate` / `model_validate_json` on foreign carriers, `ValidationError` capture and violation mapping, Pydantic-to-msgspec `project_wire` — executes in adapter modules the root imports, not inside rail interior folds.
- Altitude violations — `@field_validator` on canonical owners duplicating ingress wire law, `BaseSettings` in domain modules, or `TypeAdapter` construction inside interior transforms — are seam defects routed to root collapse tests.

# Cross-Axis Seam Routing

- Process boot → domain start — frozen `BaseSettings` instance; root scope inject; no `os.environ` in domain.
- Foreign bytes → ingress model — raw JSON/dict/env map; `model_validate_json` / `BaseSettings()` / `TypeAdapter.validate_python`.
- Ingress model → canonical owner — validated Pydantic instance; root `materialize_*` smart constructor.
- Canonical interior transition — prior frozen owner; `model_copy` / `copy.replace` per tier law.
- Canonical → msgspec wire — validated domain instance; `project_wire` / `msgspec.convert` with declared dump mode.
- msgspec decode → Pydantic re-ingress — decoded struct or dict; `model_validate` when Pydantic adds law beyond struct.
- Validation failure → envelope — `ValidationError` at boundary; root `capture` → `CodecFault.violations`.
- Store replay → domain owner — trusted bytes or dict; `model_validate` or documented `model_construct` row.
- Public API call → handler args — foreign positional/kwargs; `@validate_call` wrapper on root handler.
- Schema version read → current owner — stored envelope with version literal; boundary migration fold → `model_validate`.
- CI fixture → admission sample — walker-generated instance; `resolve(T).example()` → `model_validate`.
- Route each edge through its admission or projection entry — interior domain folds receive materialized canonical owners; raw dicts and unmapped `ValidationError` objects stop at root guards and boundary adapters named in the root module graph.
- Chained root pipelines compose as typed steps: settings validate → scope bind → foreign ingress validate → `materialize_*` → interior fold → `project_wire` → encode — no step skips the single validation gate per boundary crossing.

# Settings Fan-Out And Boot Projection

- Root `project_*_config` functions slice validated settings into per-bounded-context boot records — boot models are frozen Pydantic or msgspec owners admitting the same enum and path law live ingress uses; slices do not leak raw env key strings into smart constructors.
- `Field(validation_alias=AliasChoices(...))` on settings fields resolves at settings construction — fan-out reads canonical field values from the validated settings instance, not pre-validation env maps.
- `settings_customise_sources` ordering is pinned at root boot — collision precedence is a deployment contract owned at composition root; breaking source-order changes require regenerated snapshots and fixture matrices.
- `SecretStr` / `SecretBytes` never cross logging or public OpenAPI surfaces from root emit paths — settings dumps for diagnostics use redacted representations; root guards reject accidental `model_dump` of full settings on stdout.
- Nested settings submodels validate independently at construction — parent root boot fails closed when submodel `extra='forbid'` catches namespace leakage; domain code does not construct nested settings piecemeal.

# Dual-Engine Root Wiring

- Root declares one conformance row per Pydantic→msgspec seam the bounded context exports — source owner, `project_wire` expression, target struct, `model_dump` mode, and alias/rename policy live beside the root adapter import; ad hoc dump→dict→struct at handlers is a root wiring defect.
- Module-level msgspec `Encoder` / `Decoder` pairs and Pydantic `TypeAdapter` constants share root `[CONSTANTS]` — cross-engine round-trip proof in CI uses the same singletons production hot paths use.
- Ingress stays Pydantic where settings, discriminated unions, computed wire fields, or JSON Schema-rich contracts apply; egress stays msgspec for volume lanes — root does not register duplicate shape owners in both engines without a named row in the seam map.
- Polymorphic and `SerializeAsAny` seams prove at root before cache write — `encode → decode → model_validate` or struct equality uses root codecs; failures block merge when serializer width diverges from validation width.
- Pinned decoder replay requires root codec module path stability — renaming root submodules breaks trusted-replay rows even when `__pydantic_core_schema__` bytes are unchanged.

# Validation Failure Projection At Root

- Root boundary modules own `capture` around Pydantic admission — violation projection follows `loc`+`msg` law at the outermost adapter export; domain interiors never import or catch `ValidationError`.
- Union and discriminated-union failures may carry multiple `loc` branches — root HTTP and automation surfaces choose batch (all violations) or truncated operator views; policy is per root export, not per leaf adapter.
- `validate_call` failures prefix `loc` with parameter names — root uses the same projection function as model validation so envelope and CLI consumers see one violation vocabulary.
- `PydanticCustomError` types from validators map through the same projection — domain-coded admission failures (`path_traversal`, `empty_root`) survive root distillation without special cases.
- Settings construction failures at root boot map to startup faults with violation tuples — lazy settings raises inside domain rails past the `Result` boundary are rejected; fail at root before handlers bind.

# Evolution Promotion Units

- `pydantic` minor upgrades pin behind a single promotion unit: dependency bump, dual-mode JSON Schema snapshot diff, core-schema oracle sample loop, and mypy plugin pass on the root import graph — compilation success alone does not discharge merge.
- Schema version arm additions on persisted Pydantic owners update simultaneously: `schema_version` literal on owner or envelope, boundary migration fold, dual-mode snapshots, msgspec conformance row, and OpenAPI discriminator mapping when wire-visible — partial promotion is a merge blocker.
- Breaking discriminant vocabulary, alias routing, coercion policy, serializer transforms, computed-field contracts, defaults, and settings source order each require regenerated snapshots and fixture matrices, not isolated field edits.
- Obsolete version arms remain in root migration modules with exhaustive folds — active domain and root ingress models expose only the current arm; `assert_never` witnesses retired layouts.
- Forward-compatible `extra='ignore'` at versioned boundaries requires root documentation rows — closed domain interiors and canonical owners stay `extra='forbid'` unless the promotion unit explicitly widens ingress.
- Root `--strict` CLI flags and per-call `strict=True` are orthogonal to class-level `ConfigDict(strict=True)` — promotion units must declare which strict surfaces move together.
- Fixes applied only to runtime validators without annotation updates fail static proof — promotion units include both mypy plugin and runtime schema rails.

# Composition Root Proof Harness

- Harness layers stack orthogonally at root: static (`mypy` + `pydantic.mypy`), import architecture (domain avoids direct Pydantic except through types), contract snapshots (dual-mode JSON Schema per owner family), core-schema oracle (`resolve` → `model_validate` loop), cross-engine metamorphic (Pydantic → wire → decode → re-validate), and runtime (`beartype` on adapters post-materialization).
- Discriminated union families sample at least one generated instance per arm through root CI — `resolve(Member).example()` → `model_validate` per arm; missing arm coverage is an exhaustiveness defect.
- `TypeAdapter` and settings singletons warm in test session fixtures mirroring production boot — cold-start compilation failures surface in CI, not first request.
- JSON Schema snapshots are not ingress fixture generators when validators widen shapes beyond annotations — oracle loops and explicit admission fixtures own generative truth; schema snapshots own published contract truth.
- Proof debt from checker gaps tracks on the language axis — suppressions at root adapter seams are rejected; source annotations stay spec-complete.
- Mypy `pydantic.mypy` plugin alignment is a static proof rail — model field types, validator signatures, and settings sources must typecheck under strict config alongside runtime schema truth.

# Diagnostic And Envelope Binding

- Operational admission failures at root distill into envelope fault slots with `validation:` or `config:` stage prefixes on `failing_step` — raw `ValidationError` strings do not cross into domain `Result` interiors.
- Distillation runs at root `_emit` / envelope assembly — interior folds return typed faults; root owns step names, violation tuple formatting, and cap truncation on diagnostic hints.
- HTTP 422 and automation consumers receive `ValidationError.json()`-shaped bodies from root adapters when machine-readable paths are required — domain modules consume mapped violations only.
- Cap truncation on fault hints preserves `loc` prefixes — truncation drops verbose context, not discriminant identity fields required for replay and dispatch.

# Worker Boot And Cross-Process Shape Parity

- Worker entrypoints re-import the root module graph or a documented subset that includes all Pydantic owners used on the worker path — compiled schemas are process-local; parent-process validator identity does not cross spawn seams.
- Cross-process handoff prefers `model_dump` + `model_validate` at the worker boundary when schema version enforcement matters — pickle and `copy.deepcopy` bypass explicit version gates unless trusted-replay rows document encoder and version pins.
- `model_rebuild()` for forward-ref families runs in worker boot when models are defined in shared packages imported after partial initialization — worker-first validation without rebuild is a root boot defect.
- Free-threaded importers treat compiled `SchemaValidator` instances as immutable after root warm-up — post-import field mutation on model classes races; schema changes require redeploy and re-import, not runtime class surgery.
- Cross-process schema parity for forked workers does not yet prove generic parametrization closure (`Box[int]` vs `Box[str]`) in one CI gate — persistence keys and adapter pins must align per argument tuple.
- `BaseSettings` construction must complete before worker threads read the frozen instance — lazy settings accessors on hot paths race under parallel importers.

# CI Enforcement And Drift Gates

- Root import smoke imports every ingress model, settings owner, and pinned `TypeAdapter` — failures from unresolved forward refs or missing `model_rebuild()` block merge before expensive generative suites run.
- Dual-mode schema snapshot tests diff on every ingress family the root exports to OpenAPI — validation mode and serialization mode pairs per owner family; single-mode diffs under-approximate contract review.
- Pin `pydantic` minor upgrades behind dual-mode schema snapshot diffs plus core-schema oracle sample loops — compilation success alone does not discharge contract review.
- OpenAPI discriminator tables must derive from model declarations — hand-maintained parallel mappings beside `Field(discriminator=...)` fail CI when root schema export is enabled.
- Root CI env matrices exercise source-order collisions — not single-key boot happy paths alone.
- Registry rebuild jobs that import the root graph fail when new discriminant literals lack conformance samples, migration rows, or handler map entries tied to Pydantic ingress families.
- Settings models construct once in test fixtures mirroring production boot — repeated `BaseSettings()` in tests that mutate env between cases hide source-order defects.
- CI imports all model modules to prove rebuild completeness — forward refs need quoted annotations plus `model_rebuild()` before first validation.

# Collapse Tests

- Leaf `BaseSettings()` — domain or handler modules construct settings at call time; collapse to root singleton validated at boot and injected through scope.
- Per-request `TypeAdapter` — adapter instantiated inside handlers or tests without module constant; collapse to root `Final[TypeAdapter[T]]` warm-up.
- Ingress model in domain import — interior module imports boundary ingress `BaseModel` for typing or validation; collapse to canonical owner plus root `materialize_*` return type.
- Raw `ValidationError` in `Result` — domain fold catches or forwards pydantic exceptions; collapse to root `capture` and violation tuple mapping.
- Dump-surgery projection — `model_dump` → manual key rewrites → `Struct(**d)` without conformance row; collapse to `project_wire` / `msgspec.convert` with documented mode and alias policy.
- Schema-only fixtures — generative tests built from `model_json_schema()` alone when `BeforeValidator` widens ingress; collapse to core-schema oracle plus explicit admission fixtures.
- Version guess replay — store read selects layout by partial key presence; collapse to `schema_version` literal on envelope plus root migration fold.
- Duplicate engine owners — same concept validated in Pydantic and msgspec without seam row; collapse to single ingress owner plus named cross-engine projection at root.
- Done when every Pydantic owner used in production is reachable from root import smoke, every cross-axis handoff in the seam map routes through a named root or adapter entry, settings and adapters are singletons at root, validation failures project through root capture before domain rails, and evolution bumps land as single promotion units with dual-mode snapshot and oracle proof.
