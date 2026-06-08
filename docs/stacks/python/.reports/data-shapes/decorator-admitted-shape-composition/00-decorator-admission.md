# Decorator-Admitted Shape Composition

# Admission Doctrine

- Cross-cutting shape behavior—field synthesis, immutability, validation, derived members, runtime type checking, and model enrichment—is declared at definition time through stacked, type-visible decorators that build one coherent shape owner.
- Admission means the decorator carries executable semantics and static evidence; the decorated class or callable remains the canonical owner.
- Stacked decorators compose outward-in at application time and must leave one inspectable surface: preserved signatures, propagated `__wrapped__`, declared effect order, and checker-visible transforms.
- Callable admission uses PEP 695 inline `**P` parameter preservation; legacy `ParamSpec` imports are rejected by repo lint policy.
- Class admission uses `@typing.dataclass_transform()` when a library decorator synthesizes dataclass-like `__init__`, ordering, frozen, or field semantics invisible to the checker without the marker.

# Effect Surface Taxonomy

- Every shape-admitting decorator exposes a finite effect surface: the class of runtime mutations, static evidence, and introspection keys it leaves on the owner.
- **Synthesis decorators** (`@dataclass`, `@record`, Pydantic model metaclass, msgspec struct layout) rewrite `__init__`, slot tables, comparison hooks, and field registries; admission evidence is `__dataclass_fields__`, `__pydantic_fields__`, or msgspec `inspect.type_info`.
- **Validation decorators** (`@field_validator`, `@model_validator`, `Annotated[..., BeforeValidator]`) append compiled validator nodes to the pydantic-core schema without altering Python field order; admission evidence is the validator method registry and the compiled `function-after` / `function-before` chain inside `__pydantic_core_schema__`.
- **Projection decorators** (`@computed_field`, `@field_serializer`, `@model_serializer`) admit derived or egress-only slots; they never accept constructor input and must not share names with stored fields.
- **Runtime enforcement decorators** (`@beartype`, `@validate_call`) wrap callables and preserve PEP 695 signatures; admission evidence is `__wrapped__`, `__type_params__`, and real annotation objects at decoration time.
- **Catalog decorators** (`@register`, `@spec`, `singledispatch.register`) perform definition-time side effects; they must return the same callable or class object and attach no shadow type.
- **Marker decorators** (`@typing.final`, `@typing.override`, `@dataclass_transform`) emit static-only evidence; they are rejected unless paired with a synthesis, validation, or wrap decorator that owns executable semantics.

# Family Admission Surfaces

- PEP 681 `dataclass_transform` is the static admission contract for third-party class factories that behave like `dataclasses.dataclass`.
- Apply `dataclass_transform` to the transforming decorator function, base class, or metaclass—not every leaf model—using `field_specifiers=(Field, field, …)` so checker-visible field metadata matches the library specifier vocabulary.
- Honor transform parameters the checker must read: `eq_default`, `order_default`, `kw_only_default`, `frozen_default`, and the `field_specifiers` tuple; obsolete `field_descriptors` spelling is invalid.
- At runtime the decorator only sets `__dataclass_transform__` as a parameter dict for introspection; all synthesis remains in the library implementation.
- Competing `dataclass_transform` markers on one inheritance chain enter undefined checker behavior and are rejected in repo shape code.
- Repo canonical lightweight record pattern: `@dataclass_transform(frozen_default=True, slots_default=True, kw_only_default=True)` factory wrapping `dataclass(frozen=True, slots=True, kw_only=True)`.
- Pydantic v2 is the approved rich-domain shape engine; `BaseModel` and `pydantic_settings.BaseSettings` are runtime-evaluated base classes in Ruff type-checking config.
- `model_config = ConfigDict(frozen=True, extra="forbid")` is primary immutability and closure admission; decorators must not contradict frozen semantics with post-hoc mutation.
- `@field_validator` admits per-field ingress rules at class body sites; use `mode="before"` for wire normalization, `mode="after"` for cross-field invariants on the constructed model; `@classmethod` binding is required by Pydantic v2.
- `@model_validator` admits whole-model gates; `mode="after"` receives `self` and is the owner for backend-scoped invariants (non-file roots, traversal bans, mutual exclusion) that cannot live on a single field.
- `@computed_field` over `@property` admits derived shape members into the model schema, JSON schema, and static field lists; stack `@computed_field` above `@property` and register `computed_field` in Ruff `property-decorators`.
- `Annotated[T, BeforeValidator(fn), Field(...)]` admits inline field transforms without an extra method—prefer when the rule is a pure projection on one scalar.
- `@validate_call` admits function-boundary shape checking with the same core schema machinery as models; preserve the wrapped signature for `ty`/`mypy` and expose `.raw_function` only at trusted fast paths.
- `pydantic.computed_field`, `pydantic.field_validator`, `pydantic.model_validator`, and `pydantic.validate_call` are listed as `runtime-evaluated-decorators` in `pyproject.toml`.
- Reject `pydantic.validator` (v1), mutable models, `extra="allow"` on closed domain payloads, and domain `raise`/`if` ladders where a validator decorator can own the rule.
- msgspec `Struct` admits wire-fast, frozen, slot-backed records where Pydantic richness is unnecessary; declare with `frozen=True` and `gc=False` when instances are high-volume or long-lived.
- `msgspec.field(default_factory=...)` is the field specifier; `msgspec.Meta` and `msgspec.field` are immutable-call extend entries in Ruff bugbear config.
- msgspec does not use `dataclass_transform`; checkers treat `Struct` subclasses as known structural types—do not wrap Structs in marker-only decorators.
- Prefer msgspec for internal manifests, seam payloads, and decode oracles; prefer Pydantic when settings, computed fields, or validator graphs are load-bearing.
- beartype is an approved runtime admission layer pinned from git in `pyproject.toml` (`beartype>=0.22.2`).
- `@beartype` on functions and methods enforces call-time type contracts derived from annotations; it complements static `ty`/`mypy` gates rather than replacing them.
- `beartype.beartype` is a `runtime-evaluated-decorator`—parameters and return annotations must be real objects at definition time.
- `BeartypeConf` is an immutable-call anchor composed at module constants; `beartype.vale` attaches through `Annotated[..., Is[...]]` as runtime-only enrichment unless a plugin is adopted.
- Synthesizing decorators run before `@beartype` on callables; apply beartype to operational entrypoints where static proof ends, not to repair erased `Any` signatures.
- `PrivateAttr` admits mutable implementation state excluded from the public shape; default factory admission for caches and handles stays off the frozen field surface.
- Capability markers (`typing.override` on settings hooks, `typing.final` on sealed families) are admission evidence, not runtime behavior—pair them with real decorator semantics.

# Callable Signature Preservation

- Every callable-admitting decorator must preserve the decorated signature for checkers, introspection, and test collection.
- Type as `Callable[[Callable[P, R]], Callable[P, R]]` or widened codomain `Callable[[Callable[P, R]], Callable[P, R | E]]`—never `Callable[..., Any]`.
- Inner wrappers use `functools.wraps` so `__wrapped__`, `__name__`, `__qualname__`, `__module__`, and `__type_params__` propagate for `inspect.signature` and `inspect.unwrap`.
- PEP 695 inline `**P` is mandatory; legacy `ParamSpec` imports are rejected.
- `Concatenate[Ctx, P]` admits leading context injection discharged through parameters or `ContextVar` with `Token.reset()`, never bare `set(old)` under concurrency.
- `Result[T, E]` codomain widening preserves exception transparency on the `Ok` path only; error-channel values are return values, not raised exceptions.
- Double-decoration guards key on `getattr(obj, "__wrapped__", obj)`, decorator identity tuples (`id(dec)` + `frozenset` on the wrapper), or frozen marker attributes set once; repeated factory application must return the existing admitted owner unchanged.

# Stacked Composition Order

- Stack order is semantically load-bearing even when static types appear unchanged; document stacks through decorator order in source, not comments alone.
- Callable policy stack order outermost-to-innermost: `trace > authorize > validate > cache > govern > retry > operation`.
- Class shape stacks place immutability and `ConfigDict`/`model_config` in the class body first, then field annotations with `Annotated` validators, then `@field_validator`, then `@model_validator`, then `@computed_field` properties.
- Synthesizing decorators (`@dataclass`, metaclass, Pydantic model construction) execute before `@beartype` on the resulting callable surface.
- Field validators run before model validators in the Pydantic pipeline; admission order in source should mirror that dependency—normalize scalars first, then enforce cross-field law.
- `mode="before"` validators admit wire-to-domain coercion at the boundary edge of the model; keep coercion out of domain methods when a validator can own it.
- Registry and catalog decorators append typed rows during module import, return the wrapped callable or class without shadow types, and propagate `register`, `dispatch`, and `registry` through wrappers because `@wraps` alone omits them.

# Annotation And Field Registry Introspection

- Python 3.15 defers annotation evaluation through `__annotate__`; decorator factories must read through `annotationlib.get_annotations`, not legacy `__annotations__` mutation.
- `annotationlib.get_annotations(obj, *, format=annotationlib.Format.VALUE)` is the canonical read for decorator factories that must evaluate constraints, `Field` metadata, or `Annotated` stacks at definition time.
- `annotationlib.Format.STRING` preserves unevaluated forward references for signature renderers and test harnesses that must not import unresolved symbols during collection.
- `annotationlib.Format.FORWARDREF` materializes lazy references without forcing full evaluation of the referent module graph.
- `include_extras=True` is mandatory when `Annotated` metadata is load-bearing; default reads strip `BeforeValidator`, `Field`, and `msgspec.Meta` layers and collapse the admitted shape to bare `T`.
- Decorators must not assign to `cls.__annotations__`; admission that adds synthetic fields routes through the library field specifier (`Field`, `dataclass.field`, `msgspec.field`) so checkers and `get_annotations` stay aligned.
- Repo lint and ast-grep reject `inspect.get_annotations` without `annotationlib` routing on modules that own shape decorators.
- Stacked admission leaves one inspectable field registry per family; downstream codegen, Hypothesis registration, and catalog rows walk that chain.
- Dataclass synthesis exposes `dataclasses.fields(cls)` and `__dataclass_fields__`; decorator stacks that run after `@dataclass` read the synthesized registry, not the pre-synthesis class dict.
- Pydantic exposes `cls.model_fields` (public contract) and `cls.__pydantic_fields__` (internal `FieldInfo`); validator and serializer decorators key off `FieldInfo.alias`, `validation_alias`, and `serialization_alias` already frozen at class creation.
- msgspec exposes `msgspec.inspect.type_info(cls)` with per-field `encode_name`, constraints from `Meta`, and layout flags (`frozen`, `gc`, `tag_field`); structural decorators must not wrap structs solely to add parallel registries.
- Generic owners carry parametrized registries: `Box[int].model_fields` differs from `Box[str].model_fields` after `model_rebuild()`; decorator propagation on subclasses must trigger rebuild when new type arguments bind.
- Registry walks for test strategies prefer compiled pydantic-core inner schemas over re-deriving constraints from bare annotations when `@field_validator` bodies are opaque.

# Inheritance, Generics, And MRO

- Decorator semantics propagate through inheritance only along declared library contracts; ad hoc subclass decoration re-executes admission and may duplicate or override parent evidence.
- `dataclass_transform` on a base class or metaclass propagates to subclasses that do not declare a competing transform; a subclass that applies a second transform decorator is rejected.
- Pydantic validators and serializers declared on a base class run for subclasses unless `@override` marks an intentional replacement; subclass validators append to the compiled pipeline rather than replacing parent nodes silently.
- `model_config` and `ConfigDict` merge down the MRO with subclass wins on conflicting keys; decorators must not contradict parent `frozen=True` or `extra="forbid"` through looser child config.
- msgspec struct subclasses inherit layout and field order; adding fields on a subclass recompiles layout and requires all ancestors remain struct types—no pydantic base mixed into msgspec inheritance chains.
- `@beartype` on an overridden method does not inherit from the base implementation; each override site is an independent admission point and must carry complete annotations.
- `@typing.final` on a sealed family blocks subclass decoration that would widen the shape surface; new variants belong in the closed union owner, not in `final` subclasses.
- Re-applying `@record` or `@dataclass` on a subclass that already inherited a synthesized base is rejected—the synthesis decorator runs once on the leaf concrete owner.
- Subclass `model_config` updates use class-body assignment, not `model_config.update()` after class creation; post-hoc config mutation hides policy from checkers.
- Callable decorators on classmethods and staticmethods must stack with `classmethod`/`staticmethod` below the shape decorator so `__wrapped__` chains preserve binding descriptors.
- PEP 695 type parameters on decorated owners bind at class definition and rebind on parametrized subclasses; decorator stacks must preserve parameter visibility for checkers and `inspect.signature`.
- Declare `class Owner[T](BaseModel)` or `class Row[T](msgspec.Struct)` with parameters before decorator application; synthesizing decorators read `type_params` from the class object after the body executes.
- Parametrized subclasses (`Owner[int]`) trigger pydantic `model_rebuild()` when referenced types become concrete; decorator authors that cache schemas at import time must key caches on `(cls, get_args(cls))`.
- Bounds and defaults on type parameters (`T: str = str`) are admission evidence for `TypeAdapter` and field defaults; do not erase parameters to `Any` at decorator boundaries.
- `Generic` inheritance order is `class Box[T](BaseModel, Generic[T])` or PEP 695 compact form; intermediate bases that drop `Generic` erase parameters and break validator bodies that close over `T`.
- Callable decorators on generic methods must preserve `__type_params__` via `@wraps`; test collectors and `ty` read parameter lists from the outermost wrapper that still carries `__wrapped__`.
- Metaclass `__new__` that synthesizes `__init__` must apply `dataclass_transform` to the metaclass or factory function so pyright and mypy infer generated constructors.
- `__init_subclass__` hooks that register validators or fields must run before the class object is published to importers; late mutation after module export is rejected.
- Metaclass admission must not compete with Pydantic's own metaclass on `BaseModel` subclasses—domain models inherit Pydantic's metaclass chain, not a parallel custom metaclass.
- `__init_subclass__` keyword parameters (`**kwargs` on the hook) are not field specifiers; closed shape families reject open subclass kwargs that smuggle undeclared fields.
- When one base carries `dataclass_transform` and another carries a conflicting synthesizer, the leaf class must declare one canonical transform owner—no diamond repair through manual `__init__` assignment.
- Pydantic mixin bases that only contribute validators must inherit `BaseModel` through a single concrete base; validator MRO follows Python MRO and appends hooks in C3 order.
- Mixing msgspec `Struct` with pydantic `BaseModel` in one leaf class is rejected; projection between families happens through `model_validate` / `msgspec.convert` at boundaries, not through multiple synthesis decorators on one type.
- Protocol bases do not participate in synthesis MRO; a record that inherits `Protocol` for structural typing still has exactly one synthesis decorator on the concrete base.

# Validator Modes, Serializers, And Field Specifiers

- Pydantic admits four validator modes and three serializer modes as distinct effect surfaces; field validators run before model validators and mode choice is not interchangeable even when signatures match.
- `mode="before"` admits wire-to-domain coercion; `mode="after"` admits cross-field invariants on constructed models.
- `mode="wrap"` validators receive a `handler` callable and sandwich pydantic-core validation; admission order places wrap nodes outside plain transforms and inside terminal after-nodes for the same field.
- `mode="plain"` validators replace core validation for the field entirely; use only when wire types intentionally diverge from Python types and `json_schema_input_type` documents the wire contract.
- `Annotated[..., WrapValidator(fn), BeforeValidator(g)]` composes right-to-left with explicit wrap phases; prefer `Annotated` stacks over method validators when the rule is type-level and reused across models.
- `@field_serializer` and `@model_serializer` admit egress-only transforms; `mode="wrap"` serializers delegate to default dump handlers—ingress validators must never be paired on the same method.
- `when_used='json'` on `PlainSerializer` limits admission to JSON egress; Python-mode dumps remain unmodified and checker-visible return types stay at the stored field type.
- Serializer decorators run after validation materialization; admitting serializers on the same class does not relax `frozen=True` or reopen constructor fields.
- `computed_field` admits derived data as first-class schema members; do not parallel the same projection as an undecorated `@property` outside the model contract.
- Field specifiers are decorator-adjacent admission points: they declare constructor participation, wire presence, and comparison hooks without separate wrapper types.
- `Field(init=False)` excludes a name from synthesized `__init__` while keeping it on the model surface; `PrivateAttr` excludes from schema and serialization—do not use both for the same implementation slot.
- `Field(exclude=True)` admits wire omission while retaining construction; `computed_field` admits derivation without construction—never mirror one logical slot across excluded field and computed field.
- `Field(validation_alias=AliasChoices(...))` admits multi-key ingress at the specifier layer; keep alias law on `Field`, not in post-decoration `setattr` patches.
- `kw_only=True` per field on dataclass owners forces named construction after non-kw-only fields; decorator stacks that synthesize `__init__` must honor per-field `kw_only` flags from `dataclass.field`.
- `json_schema_extra` and `Field(description=...)` admit OpenAPI evidence at definition time; do not maintain parallel schema dicts outside the specifier.

# Decorator Factory Configuration

- Parameterized decorator factories admit shape policy through frozen configuration anchors, not through closure mutation or untyped `**options` bags.
- `BeartypeConf`, `ConfigDict`, `SettingsConfigDict`, and `Field(...)` are immutable-call extend targets in Ruff config; factories close over frozen instances declared in `[CONSTANTS]`, not literals rebuilt per decoration.
- Factory return type must be the narrowest applicable decorator alias: `Callable[[type[T]], type[T]]` for class factories, `Callable[[Callable[P, R]], Callable[P, R]]` for callable wrappers.
- Factories that vary behavior by string dispatch (`mode="before"|"after"`) should expose distinct factory entrypoints or `Literal`-typed parameters—not runtime `if` on strings inside an erased factory.
- Import-cycle resolution and acyclic domain folder policy belong at composition root; decorator factories referenced from roots close over frozen configuration anchors declared once, not rebuilt per adapter call site.

# Static Checker Registration And Toolchain Integration

- Python `>=3.15` is the interpreter floor; decorator admission must satisfy concurrent static and lint gates declared in `pyproject.toml`.
- New shape-admitting decorators enter the toolchain through explicit registration before production adoption.
- Add decorator dotted paths to `[tool.ruff.lint.flake8-type-checking] runtime-evaluated-decorators` when the decorator forces runtime evaluation of parameter and return annotations.
- Add base classes to `runtime-evaluated-base-classes` when annotations on fields resolve only after metaclass execution (`BaseModel`, `BaseSettings`, `msgspec.Struct` are already listed).
- Register property-style decorators in `property-decorators` (`computed_field`) and classmethod-style validators in `classmethod-decorators` (`field_validator`, `model_validator`).
- Enable `pydantic.mypy` plugin entries when decorators admit pydantic models; `ty` strict mode follows the same annotation objects at decoration time.
- Extend `extend-immutable-calls` when a decorator factory accepts only frozen configuration calls (`Field`, `ConfigDict`, `BeartypeConf`).
- Ruff `pep8-naming` recognizes `field_validator` and `model_validator` as classmethod decorators; `pydocstyle` recognizes `computed_field` as a property decorator.

# Registry Rows And Definition-Time Catalogs

- Registration decorators append typed rows (`LawRecord`, handler tables, catalog entries) during module import—avoid scattered runtime `list.append` from call sites.
- Registration must not replace the shape: the decorated function's domain contract remains the law subject; the registry records coverage metadata only.
- `@register`, `@spec`, and `singledispatch.register` execute at module-import phase; seam adapters registered at root must reference handlers whose signatures already admit canonical domain types.
- Registry mutation is not a substitute for ingress validation; registration does not substitute for shape admission on the handler body.
- Dispatch registration on seam handlers propagates `register`, `dispatch`, and `registry` through `@wraps` chains when policy wrappers wrap dispatched functions; `@wraps` alone omits dispatch attributes.
- Test `@spec` decorators preserve law-subject signatures through hypothesis stacks; seam integration tests collect the same outer callable presented to pytest after `inspect.unwrap`.
- Catalog rows store decorator identity fingerprints (`qualname`, `module`, spec ids) sufficient to locate admitted objects without serializing closure state.
- Cross-process handoff uses canonical values and versioned envelopes, not catalog row payloads alone.

# Decoration Phases And Runtime Pipeline

- Shape-admitting decorators execute across four phases—module-import, class-body, first-touch, and call-time—and phase determines which evidence is durable, which failures are import-time, and which introspection APIs are valid at admission.
- **Module-import phase** — catalog decorators (`@register`, `@spec`, `singledispatch.register`) append rows and return the wrapped object; failures abort module load and must surface as typed configuration errors, not deferred `ValidationError`.
- **Class-body phase** — synthesis, validation, projection, and marker decorators on class statements compile owner registries before the class object binds to its module global; pydantic metaclass and msgspec struct layout complete in this phase.
- **First-touch phase** — pydantic `model_rebuild()`, `TypeAdapter` schema compile, and beartype wrapper materialization may defer until first reference; decorator authors that cache compiled artifacts must treat first-touch as part of the public admission contract.
- **Call-time phase** — `@beartype`, `@validate_call`, and operational policy wrappers execute per invocation; synthesis and field-validator registration do not re-run unless the class object is redefined.
- Phase boundaries are not reorderable: import-time catalog mutation must not depend on call-time validation results; call-time wrappers must not mutate class registries.
- Admission order in source fixes compiled node positions on pydantic owners; runtime execution walks the pydantic-core graph, not Python decorator application order on methods.
- Per-field chain executes `before` → core type validation → `after` → `wrap` terminal handlers for that field; cross-field `model_validator(mode="after")` runs only after all fields materialize.
- `mode="wrap"` handlers receive a `handler` that already includes inner plain and before nodes; outer wrap decorators in source compile to outer runtime sandwich layers.
- `Annotated[..., BeforeValidator(f), WrapValidator(g)]` composes at compile time with explicit phase tags; runtime order follows `Annotated` right-to-left composition, not method definition order elsewhere in the module.
- `@field_serializer` and `@model_serializer` execute only on egress paths (`model_dump`, `model_dump_json`, custom serializers); they never interleave with ingress `model_validate` unless a single owner method explicitly combines both concerns—which is rejected.
- msgspec decode runs layout validation and `Meta` constraints in one native pass; there is no pydantic-style after-validator replay—cross-field invariants on struct owners belong in `__post_init__` or boundary adapters, not stacked method validators.
- Dataclass `__post_init__` runs after synthesized `__init__` field assignment and before the instance is returned to callers; validators admitted through Pydantic never run on dataclass-only owners.
- msgspec struct layout is fixed at class creation; generic struct parametrization is not pydantic-style rebuild—parametric owners choose pydantic when rebuild semantics are load-bearing.
- beartype decoration is eager on the callable object at decoration time; changing annotations after `@beartype` without re-decoration is undefined and rejected in repo code.

# Failure Surface And Attribution

- Decorator stacks route failures to the admitting owner and phase without erasing error context or mixing rails.
- Pydantic `ValidationError` carries `loc` tuples keyed to field aliases and validator tags; decorator factories must not catch and rethrow as undifferentiated `ValueError` when `loc` evidence is load-bearing for boundary mapping.
- `model_validator` and `field_validator` bodies return corrected values or raise `ValueError`/`AssertionError` absorbed by pydantic-core; domain typed errors belong on `Result` rails at operational entrypoints, not inside validator bodies unless the validator is the boundary owner.
- beartype violations raise `BeartypeCallHintViolation` at the outermost `@beartype` wrapper; policy stacks that wrap beartype must not swallow violation types into generic `Exception` handlers.
- msgspec decode failures surface as `msgspec.ValidationError` with structured `msg` and index paths; post-init `TypeError`/`ValueError` inside struct `__post_init__` is wrapped into the same class on decode paths but propagates raw on `copy.replace`—callers must attribute by entrypoint, not by exception type alone.
- Import-time decorator failures (duplicate registration, double-decoration guards, conflicting `dataclass_transform`) must fail fast with repository-owned error types or `TypeError` at definition site; they must not defer to first consumer call.
- Callable decorators widening codomain to `Result[T, E]` preserve exception transparency on the `Ok` path only; error-channel values are return values, not raised exceptions, and must not trigger retry wrappers unless the rail owner documents retryable `E` cases.

# Stack Verification And Diagnostic Introspection

- Proof that an admitted owner matches its declared decorator stack uses introspection chains, not convention or comment tables.
- Walk callable stacks with `inspect.unwrap` until `__wrapped__` exhaustion; the outermost object presented to pytest collection must still expose `__type_params__` and a signature compatible with PEP 695 expectations when the inner target is generic.
- For pydantic owners, compare `cls.model_fields` public keys against `annotationlib.get_annotations(cls, include_extras=True)`; mismatches signal deferred-evaluation drift or post-hoc `model_rebuild` omission.
- Read compiled evidence through `cls.__pydantic_core_schema__` or `TypeAdapter(cls).core_schema` after first-touch; Hypothesis strategy registration and catalog rows that derive constraints must prefer compiled inner nodes over re-parsing validator source when `mode="wrap"` obscures bodies.
- Dataclass stacks verify through `dataclasses.fields(cls)` ordering against `__init__` parameter order from `inspect.signature(cls.__init__)`; kw-only and `init=False` specifiers must agree with `Field`/`field` metadata.
- msgspec stacks verify through `msgspec.inspect.type_info(cls)` field order, `encode_name`, and layout flags; do not maintain parallel field lists in test helpers.
- Registry decorators on tests verify by reading pytest marks on the collected item (`spec_id`, `law_id`, hypothesis `settings`) while ensuring the collected function signature still matches the law subject parameters.
- Operational diagnostics read admitted shape through stable public introspection APIs, not private dunder mutation.
- Prefer `annotationlib.get_annotations(obj, format=annotationlib.Format.VALUE, include_extras=True)` for decorator audit scripts and law-matrix codegen.
- Prefer `cls.model_json_schema()` for OpenAPI evidence after all `@computed_field` and `Field(description=...)` admissions complete.
- Schema drift checks diff `cls.model_json_schema()` snapshots across dependency upgrades when `@computed_field` or `Field(json_schema_extra=...)` admissions are load-bearing for OpenAPI consumers.
- For runtime enforcement audit, `inspect.signature(inspect.unwrap(fn))` compared against `Result` codomain widening documents the effective callable contract.

# Lazy Compile And Rebuild Triggers

- Deferred compilation phases must be explicit so import-order hazards and generic parametrization do not silently serve stale schemas.
- Pydantic generic subclasses trigger `model_rebuild()` when concrete type arguments first bind; decorator stacks that cache `TypeAdapter` or core schemas must include `get_args(cls)` in the cache key.
- Forward references in deferred annotation modules resolve at rebuild; owners that reference not-yet-defined peers must use string annotations or `from __future__ import annotations` consistently with `annotationlib.Format.FORWARDREF` reads during factory execution.
- First `model_validate`, `model_validate_json`, or `TypeAdapter.validate_python` on a rebuilt model is the proof gate for compile-time decorator admission; CI tests must exercise first-touch on generic parametrizations, not only on monomorphic import fixtures.
- msgspec struct layout is fixed at class creation; generic struct parametrization is not pydantic-style rebuild—parametric owners choose pydantic when rebuild semantics are load-bearing.
- beartype decoration is eager on the callable object at decoration time; changing annotations after `@beartype` without re-decoration is undefined and rejected in repo code.

# Test Harness Decoration Algebra

- Test admission decorators compose with shape decorators under stricter signature rules than production callables.
- `@spec` and law-matrix decorators apply hypothesis marks in fixed inner order (`@given` before `@settings` before pytest marks); the outer collected callable must remain the law subject, not an anonymous inner closure.
- Hypothesis `@given` that consumes a strategy parameter removes that parameter from the test signature presented to pytest; do not re-stamp the inner function with `functools.wraps` that resurrects the strategy parameter on the collected item.
- Double-decoration guards on tests key on `__wrapped__` and decorator identity tuples; a test function re-wrapped with an identical spec decorator must return the existing admitted item unchanged.
- `pytest.mark.parametrize` outermost placement stacks above hypothesis when both apply; parametrize indices must align with law records, not with ad hoc string ids in decorator closures.
- Registry decorators on tests verify by reading pytest marks on the collected item (`spec_id`, `law_id`, hypothesis `settings`) while ensuring the collected function signature still matches the law subject parameters.
- Production `@beartype` on test helpers is permitted; `@beartype` on the collected test function itself is discouraged when hypothesis or pytest plugins rewrite calling conventions—prefer beartype on extracted operational helpers under test.

# Call-Time Boundary Admission

- Operational seams admit call-time shape checking through `@validate_call` and `TypeAdapter` without relocating domain validators into handlers.
- `@validate_call` and module-owned `TypeAdapter` admit call-time shape checking from complete annotations at decoration or first-touch compile; they are not substitutes for missing static shape on domain interiors.
- `@validate_call` on boundary handlers compiles a fresh callable schema from annotations at decoration time; keep handler annotations complete and avoid `Any` repair.
- `TypeAdapter(T)` at module constants for semi-trusted replay paths compiles once at first use; boundary modules own the adapter, not leaf domain methods.
- `validate_call` preserves `.raw_function` for trusted fast paths; only composition roots with documented trust posture may call `.raw_function`; default ingress routes through the validated wrapper.
- Decoration order on boundary callables: domain logic function first, then `@validate_call`, then policy wrappers (`trace`, `authorize`, `cache`) from outermost to innermost—`validate_call` sits inside observability wrappers so validation failures surface before trace spans complete when possible.
- `@beartype` and `@validate_call` on the same callable require beartype outside validate_call when both enforce overlapping contracts; prefer one admission owner unless beartype guards internal helpers validate_call invokes.
- Use `@validate_call` when the handler is the wire or CLI owner; use `TypeAdapter(T)` when the admitted shape is a model or payload type validated at the call site without wrapping domain logic.
- `TypeAdapter.validate_python` and `validate_json` on ingress projections enforce strict and unknown-key policy aligned with lattice ingress role—seam handlers do not parse raw dicts with manual key pops before adapter validation.
- Keyword-callable hooks typed with `Unpack[Payload]` at seams preserve PEP 695 `**P` through decorator stacks; `Concatenate[Context, P]` prepends aspect context without redeclaring payload keys on each wrapper layer.

# Stack Altitude And Decorator Placement

- Decorator-admitted shapes bind to the stack lattice at fixed altitudes; altitude determines which decorators may execute and which owner carries compiled evidence.
- **Boundary altitude** — ingress adapters, wire projectors, and seam `materialize_*` functions admit `@validate_call`, `@beartype`, and policy wrappers (`trace`, `authorize`); synthesis and validator decorators on domain records do not run here except through `TypeAdapter` or explicit `model_validate` on declared ingress types.
- **Ingress-projection altitude** — Pydantic discriminated ingress models carry `@field_validator`, `@model_validator`, and `Field` specifiers for wire-visible law; this altitude validates foreign layout before canonical construction and never substitutes as the durable domain owner.
- **Canonical altitude** — frozen domain records, variant `Member` arms, and rich class owners carry synthesis, replacement, and enrichment decorators; `@computed_field` and `@model_validator(mode="after")` on canonical owners enforce invariants that must survive interior transforms without re-ingress.
- **Composition-root altitude** — roots wire adapters, settings fan-out, protocol scope maps, and module-owned `TypeAdapter` constants; decorator factories close over frozen `ConfigDict` and `BeartypeConf` declared in root `[CONSTANTS]`, not in leaf domain modules.
- **Interior altitude** — rail transforms, folds, and `copy.replace` chains operate on already-admitted canonical values; call-time `@beartype` on interior helpers is permitted; `@validate_call` and ingress validators are rejected unless an explicit re-ingress boundary is declared.
- Altitude violations—domain modules importing boundary ingress models, serializer decorators on interior methods, or registry decorators that mutate catalogs from call-time wrappers—are seam defects, not style issues.

# Ingress Projection And Decorator Ownership

- Ingress projections are decorator-admitted Pydantic or payload surfaces that own wire-visible validation; canonical owners must not re-encode the same wire law.
- Ingress models admit `@field_validator(mode="before")` for wire normalization, alias resolution, and scalar coercion; canonical smart constructors admit cross-field domain law the ingress surface cannot see without leaking foreign keys inward.
- `Field(validation_alias=...)` and `AliasChoices` belong on ingress field specifiers; canonical field names stay internal—anti-corruption tables in adapter modules map external keys before validation, not through post-decoration `setattr` on canonical classes.
- Discriminated ingress unions admit `Literal` discriminants and `Tag()` metadata on each member; OpenAPI and runtime routing read the same discriminant vocabulary—hand-maintained parallel tag tables at seams are rejected.
- `@model_validator(mode="after")` on ingress models enforces wire-visible mutual exclusion and envelope shape; backend-scoped invariants (filesystem roots, traversal bans, capability gates) belong on canonical owners after `materialize_*`, not duplicated on ingress and canonical surfaces.
- msgspec wire structs at ingress admit `Meta` constraints and layout flags only when ingress and wire share one struct; when ingress is Pydantic and wire is msgspec, validators stay on the ingress owner and wire layout is a separate egress projection—not a second validator stack on the same concept.
- Ingress decorator stacks terminate at validation exit; enrichment decorators (`@computed_field` for derived egress-only slots, interior-only projections) do not belong on ingress models that never durably own the concept.

# Canonical Owner And Replacement Handoffs

- `materialize_*` adapters return canonical records or `Result[Canonical, E]` from smart constructors—ingress `BaseModel` instances are inputs, not durable owners; canonical classes carry synthesis decorators (`@record`, frozen `BaseModel`, msgspec `Struct`) appropriate to their stack role.
- `model_copy(update=...)` and `copy.replace` on canonical frozen owners replay post-init and validator evidence per family rules; seam handoffs export canonical values after replacement, not mid-replacement ingress aliases.
- `@computed_field` on canonical owners projects interior state for OpenAPI and operational diagnostics; seam consumers receive canonical instances and call owner projection methods—they do not re-run ingress validators on materialized values.
- Rich class owners that graduate from variant families inherit decorator admission on one exterior type; seam exports use the rich owner's egress method taking `Member` or canonical payload—per-arm undecorated dump helpers stay private to the owner module.
- Re-ingress at seams (foreign replay, store read, cross-context handoff) re-enters at ingress-projection altitude through declared adapters—interior modules do not call `model_validate` on dicts extracted from canonical `model_dump` without a boundary owner.

# Wire Egress And Serializer Seam Binding

- Egress decorators admit wire transforms only on outbound paths; they bind canonical owners to msgspec structs, JSON, or provider layouts at the serialization stage.
- `@field_serializer` and `@model_serializer` execute after canonical materialization and validation; composition roots call `model_dump` / custom serializers once per outbound event—interior folds hold canonical types, not wire structs flowing backward.
- `when_used='json'` and `PlainSerializer` confine admission to JSON egress; Python-mode dumps and interior transforms retain stored field types—serializer decorators do not relax `frozen=True` or reopen constructor fields.
- Wire projection functions (`project_*_wire`) live in boundary packages; they may wrap canonical owners with msgspec `Struct` encode paths without decorating canonical classes with msgspec-only concerns when pydantic richness is load-bearing on the canonical side.
- OpenAPI evidence after egress binding diff-checks `model_json_schema()` on ingress models against published contracts; `@computed_field` and `Field(description=...)` on canonical owners feed operational schema when ingress and canonical diverge by policy.
- Serializer wrap modes delegate to default dump handlers; ingress `@field_validator` bodies must never share method names with egress serializers—dual-purpose methods collapse phase attribution and break failure routing.

# Cross-Family Projection Without Dual Decoration

- Pydantic and msgspec families each admit decorators on one owner per concept; cross-family shape movement happens at boundary adapters, not through stacked synthesis on one class.
- Canonical pydantic owners project to msgspec wire structs through explicit adapter functions at the serialization stage—do not wrap pydantic `BaseModel` subclasses in msgspec layout decorators or vice versa on the same leaf type.
- `model_validate` / `TypeAdapter` at the ingress seam and `msgspec.convert` / struct encode at the wire seam form a typed handoff pair; projection tables document field correspondence once in the adapter module.
- msgspec `Struct` owners used for internal manifests and seam payloads carry `frozen=True` and `Meta` at definition time; promotion from pydantic ingress to msgspec canonical happens in `materialize_*`, not by decorating an ingress model with struct synthesis.
- `TypeAdapter(T)` for semi-trusted replay compiles pydantic or payload types at module constants in boundary or composition-root modules—domain interiors import neither adapter instances nor foreign family decorators.
- Family choice at admission time is load-bearing: pydantic when settings, computed fields, validator graphs, or rebuild semantics are required; msgspec when wire-fast frozen layout suffices—family switching mid-stack requires a documented boundary adapter, not a decorator stack on one owner.

# Composition Root Decorator Assembly

- Composition roots assemble decorator-admitted surfaces into one typed handoff graph; roots own adapters and module constants, not domain validator bodies.
- Root wiring declares `foreign_ingress -> Result[Canonical, E]` per context using `@validate_call` or `TypeAdapter`-backed handlers on adapter functions—handler annotations must be complete; roots do not use call-time validation to repair erased domain signatures.
- Policy stack on root adapters follows canonical order outermost-to-innermost: `trace > authorize > validate > cache > govern > retry`; `@validate_call` sits inside observability wrappers so validation failures surface with attributable `loc` before trace spans complete when possible.
- Settings admission isolates to process-start: `BaseSettings` with `@field_validator` and `settings_customise_sources` lives in root or boot modules; domain constructors receive projected boot records, not raw environment reads.
- Module-owned `TypeAdapter` and pinned `Decoder` constants compile at first touch in root `[CONSTANTS]` or adjacent boundary modules; cache keys include `get_args(cls)` for generic parametrizations.
- Protocol scope injection and registry catalogs bind at root import; catalog decorators on handlers require handler types parameterized on canonical domain types before registry insertion.
- Import-cycle resolution and acyclic domain folder policy belong at root; decorator factories referenced from roots close over frozen configuration anchors declared once, not rebuilt per adapter call site.
- Call-time boundary admission routes through `@validate_call` and `TypeAdapter` with explicit trust posture; seam routing must not duplicate pydantic-core pipelines already owned by ingress models.
- `@beartype` and `@validate_call` on the same seam handler prefer beartype outside validate_call when both enforce overlapping contracts; otherwise designate one admission owner to avoid duplicate failure surfaces with incompatible exception types.

# Settings And Process-Start Admission Isolation

- `BaseSettings` subclasses admit `@field_validator`, `Field`, and `@computed_field` for boot-time normalization; `@override` on `settings_customise_sources` documents intentional source ordering—settings hooks are not domain validators.
- Settings fan-out at composition root projects one frozen settings model into per-context boot configs through `project_*_config` functions before domain transforms execute—domain modules read boot records, not `os.environ`.
- `SettingsConfigDict(frozen=True, extra="forbid")` matches domain closure posture; settings validators normalize paths, encodings, and wire-safe strings at boot—the same rules are not re-applied as `@field_validator` on canonical domain records unless the domain field mirrors a live setting.
- Generic settings parametrization follows pydantic rebuild law; first-touch proof in CI exercises settings models with PEP 695 parameters, not only monomorphic boot fixtures.
- cyclopts plus pydantic dual-surface owners remain staging until a single decorator-admitted owner can project CLI keyword seams and settings without lattice collapse—until then CLI payloads stay on dedicated staging contracts at the boundary.

# Seam Proof Contracts For Decorated Owners

- Integration proof verifies decorator stacks at seams through compiled evidence and round-trip gates, not source decorator order alone.
- **Ingress proof** — lawful wire samples pass `TypeAdapter.validate_json` on ingress models; drift samples fail with attributable `ValidationError.loc`; removing duplicate validators on canonical owners must not weaken admission.
- **Materialization proof** — `materialize_*` samples from validated ingress produce canonical instances or expected `Result` errors; construction rejection tests prove smart constructors enforce domain law ingress cannot express.
- **Egress proof** — canonical values serialize through admitted serializers to wire structs or bytes; round-trip `decoder.decode(encoder.encode(value))` per polymorphic arm before cache write or cross-process handoff.
- **Decorator stack proof at seams** — `inspect.unwrap` on adapter handlers preserves `__type_params__`; `model_fields` agrees with `annotationlib.get_annotations(..., include_extras=True)` on ingress and canonical owners after first-touch rebuild.
- **Altitude proof** — static import graphs show domain modules do not import boundary ingress types; ast-grep or arch tests flag domain folders referencing Pydantic ingress models or msgspec wire structs directly.
- **Failure attribution proof** — seam tests assert exception types and `loc` paths match the admitting altitude; ingress failures never masquerade as canonical `BeartypeCallHintViolation` and vice versa.

# Trust Posture And Raw Function Gates

- Trusted fast paths bypass validated wrappers only at documented composition-root gates; trust posture is explicit, not inferred from call site convenience.
- `.raw_function` on `@validate_call` wrappers is restricted to roots that document replay trust (pinned decoder identity, schema version, store key) beside the adapter—default foreign ingress always uses the validated wrapper.
- Semi-trusted replay through module-owned `TypeAdapter` or pinned `Decoder` compiles once; trust rows declare which fields may skip re-validation and under which storage envelope—interior modules do not construct trust bypasses locally.
- `@beartype` on operational entrypoints complements static proof where annotations are complete; trust posture does not use beartype to repair erased handler signatures that should be fixed at definition time.
- Seam adapters that widen codomain to `Result[T, E]` preserve exception transparency on the `Ok` path; error-channel values are returns, not raised exceptions—retry and govern wrappers consult rail owners for retryable `E`, not validator `ValueError` absorption.

# Evolution And Proof Harness

- Decorator-admitted shape owners evolve under dependency upgrades, validator-graph changes, and toolchain registration pressure; evolution binds decorator stacks, compiled evidence, schema snapshots, and seam proof rows in one promotion unit; harnesses attribute defects to the admitting decorator and phase before domain logic is suspect.

# Decorator Stack Evolution Ladder

- Admitted decorator stacks are versioned by owner fingerprint, not by comment tables; fingerprint rows capture outermost wrapper `qualname`, synthesis owner family (`pydantic`, `msgspec`, `dataclass`), frozen `ConfigDict` hash keys, and registered `runtime-evaluated-decorators` paths active at class creation.
- Adding a decorator to an existing owner is a stack version event when the new decorator alters synthesis, validation phase, projection schema, or callable codomain; append one harness row and regenerate compiled-evidence snapshots; marker-only additions without executable semantics do not bump stack version.
- Removing a validator or serializer decorator is a breaking stack event; downstream ingress projections, canonical cross-field law, OpenAPI snapshots, and Hypothesis strategy registrations must update in the same promotion unit; silent removal that leaves duplicate law on a sibling owner is a merge blocker.
- Reordering decorators on callables changes observability and failure semantics even when static types appear unchanged; policy stack reorder (`trace`, `authorize`, `validate_call`, `beartype`) requires seam proof re-run and documented rationale in the adapter owner module.
- Dependency minor bumps (`pydantic`, `beartype`, `msgspec`) may recompile inner validator nodes without source edits; treat pin upgrades as stack evolution triggers: rerun first-touch rebuild proof, dual-mode schema diffs, and `__pydantic_core_schema__` oracle walks before merge.
- Obsolete decorator spellings (`pydantic.validator` v1, `field_descriptors` on `dataclass_transform`, raw `__annotations__` introspection in factories) retire through explicit rejection rows in lint policy; removed patterns stay in migration notes beside owner modules, not in active admission stacks.

# Simultaneous Decorator Update Contract

- New `runtime-evaluated-decorator` registration in `pyproject.toml` propagates in one promotion unit: Ruff flake8-type-checking row, optional `property-decorators` or `classmethod-decorators` row, `extend-immutable-calls` when the factory accepts frozen config only, and at least one reference owner exercising the decorator under `ty` strict mode.
- New synthesis or validation decorator on a canonical owner propagates simultaneously to ingress projection when wire law mirrors the rule, egress serializer when the rule affects dump shape, seam adapter `materialize_*` samples, and law-matrix `@spec` rows keyed to the owner; partial promotion leaves seams enforcing stale law.
- `@computed_field` additions require serialization-mode JSON Schema snapshot updates and OpenAPI diff review; validation-mode snapshots alone under-approximate public egress after computed admission.
- Generic PEP 695 owners that gain decorators triggering `model_rebuild()` require parametrized first-touch tests for every concrete argument tuple referenced in production or CI; monomorphic import fixtures do not discharge rebuild evolution.
- Catalog decorator changes (`@register`, `@spec`, dispatch `register`) update registry row schemas, dispatch key tables, and collected pytest signatures together; registry-only edits without handler signature proof are harness failures.
- `dataclass_transform` parameter changes (`frozen_default`, `field_specifiers`) affect every downstream subclass in the transform chain; promotion updates checker baselines and dataclass field-order verification rows for the whole family, not only the factory definition site.

# Decorator Proof Harness Architecture

- Proof layers stack orthogonally on admitted owners: static registration (`ty`, `mypy`, Ruff decorator rows), definition-time admission (import-phase guards, double-decoration rejection), compiled oracle (`model_fields` vs `annotationlib`, `__pydantic_core_schema__`, `msgspec.inspect.type_info`), call-time enforcement (`inspect.unwrap` codomain, `BeartypeCallHintViolation` routing), and seam altitude (domain modules do not import ingress decorator owners).
- Decorator audit scripts read `annotationlib.get_annotations(obj, format=annotationlib.Format.VALUE, include_extras=True)`; harnesses never diff raw source decorator lines without compiled backing evidence.
- Stack fingerprint proofs walk `inspect.unwrap` to exhaustion and compare outermost `__type_params__`, `__wrapped__` depth, and `inspect.signature` against declared PEP 695 contracts; fingerprint mismatch between production adapter and law-matrix collected item blocks merge.
- Phase-attributed failure injection tests exercise one defect per decoration phase: import-time duplicate registration, class-body conflicting `dataclass_transform`, first-touch stale generic schema, call-time beartype violation; conflated phase attribution fails the harness module.
- Harness execution order: static checker and Ruff registration rows before compiled oracle walks before call-time enforcement samples before seam altitude arch rules; static failures block expensive generative decorator proofs.
- Proof debt from erased decorator codomain or suppressed `ValidationError.loc` at seams is rejected; fix the admitting decorator or adapter, do not add harness waivers on integration owners.

# Dependency Upgrade Regression Gates

- Pin `pydantic` minor upgrades behind dual-mode JSON Schema snapshot diffs on every owner carrying `@computed_field`, `@field_serializer`, or `Field(json_schema_extra=...)`; compilation success alone does not discharge decorator contract review.
- Pin `beartype` upgrades behind call-time samples on operational entrypoints decorated at module scope; annotation object identity changes across versions can alter violation messages without source edits.
- Ruff upgrades that touch `flake8-type-checking` runtime-evaluated lists require grep proof that every listed decorator is imported on at least one exercised owner; orphan registration rows are drift signals.
- `msgspec` layout flag changes (`gc`, `tag_field`) require `type_info` ordering proofs and metamorphic encode-decode samples; struct layout is fixed at class creation; upgrades do not rebuild layouts silently.
- `annotationlib` behavior changes under Python micro bumps require deferred-evaluation modules to rerun `get_annotations` parity tests against `model_fields`; annotation read drift is a decorator propagation defect, not a domain logic defect.
- Toolchain promotion that adds a new shape-admitting library decorator must include negative tests rejecting marker-only shims and `Callable[..., Any]` factories before the decorator appears on production owners.

# Schema And Stack Drift Gates

- Store paired validation-mode and serialization-mode JSON Schema snapshots per pydantic owner family admitted through validator and projection decorators; CI diffs both on decorator or `Field` specifier edits and on `pydantic` pin changes.
- Decorator-induced schema churn separates ref renames from constraint changes; `$ref` deduplication moves definitions without semantic edits; reviewer checklist distinguishes ref churn from new validator nodes.
- `model_json_schema()` drift gates run after all `@computed_field` and `Field(description=...)` admissions complete; partial class definitions in snapshot tests under-approximate decorator stack evidence.
- Ingress and canonical owners under different decorator stacks diff independently; merging ingress wire law into canonical snapshots or vice versa masks altitude violations.
- Stack fingerprint rows stored beside owner modules enable diff on promotion; changed `__pydantic_core_schema__` tag sets without fingerprint row update fail the owner harness.
- OpenAPI publication derives from decorated ingress owners; hand-maintained parallel field tables diverging from `model_json_schema()` after decorator edits are merge blockers.

# Validator Graph Migration Folds

- Persisted or cached pydantic owners carrying `schema_version: Literal[...]` admit validator-graph evolution through boundary migration folds; domain interiors after migration see only the current decorator stack; obsolete validator nodes remain in migration modules with exhaustive witnesses.
- Read-path migration replays stored payloads through historical `TypeAdapter` or `model_validate` compiled at the stored version, then applies a total fold to current owner; validator decorator removals on the current owner do not retroactively rewrite stored bytes without a version bump.
- `mode="wrap"` to `mode="before"` validator refactors are version events when wire acceptance width changes; document each refactor in conformance rows and regenerate core-schema oracle samples.
- Adding `model_validator(mode="after")` cross-field law on canonical owners does not migrate ingress-only wire rules; ingress decorator stacks update independently unless public API and wire are intentionally unified.
- Generic parametrization validator graphs compile distinct schemas per `get_args(cls)`; migration folds and cache keys include concrete type arguments; erasure-friendly generic names alone are invalid persistence keys.
- Trusted replay through `.raw_function` or `model_construct` bypasses validator replay only when composition-root trust rows pin schema version and encoder identity; validator graph evolution invalidates trust rows without documented re-pin.

# Failure Archaeology Across Decoration Phases

- Import-phase failures attribute to catalog decorators and duplicate-registration guards; fault payloads name the module global and decorator `qualname`; domain smart constructors are not suspect until class-body phase completes.
- Class-body failures attribute to synthesis and competing `dataclass_transform` markers; `TypeError` at definition site names the class and conflicting decorator; deferred `ValidationError` on first consumer call indicates misclassified import-phase defect.
- First-touch failures attribute to stale generic rebuild or forward-reference resolution; fault injection after `model_rebuild()` omission names the parametrized owner and type arguments; call-time beartype is not suspect.
- Call-time failures attribute to the outermost enforcing decorator on the invoked path; `ValidationError.loc` maps to pydantic admitting owners; `BeartypeCallHintViolation` maps to beartype wrappers; policy wrappers that swallow typed violations are seam defects.
- Seam failures crossing altitudes attribute by exception type and `loc` prefix; ingress `ValidationError` must not masquerade as canonical beartype violations after `materialize_*`; duplicate messages from ingress and canonical validators trigger duplicate-law checklist attribution.
- Test harness failures attribute to hypothesis stack signature drift; collected law-subject signature mismatch implicates `@spec` or `wraps` ordering, not production decorator stacks unless the test imports production stacks incorrectly.

# Property Harness And Law Registration

- Hypothesis strategies on decorator-admitted pydantic owners prefer compiled inner schema nodes when `@field_validator` bodies are opaque; register `st.register_type_strategy(Owner, strategy)` beside the owner when validator graphs exceed annotation-inferred constraints.
- Law-matrix `@spec` rows bind to decorator-admitted callables and classes by `qualname` and module; witnesses prove stack fingerprints and phase-attributed failures, not pytest file paths alone.
- Composite strategies respect decorator admission order dependencies; generate normalized scalars before cross-field tuples when `mode="before"` and `model_validator(mode="after")` both admit law on one owner.
- Shrinking preserves decorator-admitted invariants; invalid post-shrink values must fail at the admitting validator with attributable `loc`, not at interior domain logic unrelated to the decorator under test.
- Fold totality on decorator effect surfaces asserts every synthesis family in a promotion unit has oracle walk coverage; adding msgspec struct owners without `type_info` proofs alongside pydantic owners in the same family is an exhaustiveness defect.
- Metamorphic chains on decorated owners: lawful instance → egress serializers → decode → re-validate through ingress decorators → `materialize_*` → canonical equality; failures implicate projection decorators before validator body edits.

# CI Regression Gates For Decorator Drift

- Adding `@field_validator` or `@model_validator` without updating ingress mirror, canonical deduplication checklist, or documented exemption row fails decorator coverage gate; duplicate wire law on two altitudes blocks merge.
- Removing `@beartype` from operational entrypoints without static proof discharge row fails enforcement regression; call-time admission gaps require explicit waiver on the language axis, not silent removal.
- Mutation testing targets decorator factories and seam adapters wrapping `@validate_call`; mutants erasing `__wrapped__` or widening to `Any` must fail stack fingerprint or static checker gates.
- ast-grep rules flag raw `__annotations__` assignment in decorator bodies, post-class `model_config.update()`, and domain imports of ingress decorator owners; violations fail before behavioral suites.
- Arch tests enforce decorator altitude: domain folders do not import boundary ingress models admitted only for wire validation; composition roots own `TypeAdapter` constants and settings decorator owners.
- Stryker or equivalent prioritizes adapter modules where policy stacks wrap `@validate_call`; interior domain validator bodies are secondary when seam handlers concentrate decorator stacks.

# Evolution Anti-Patterns

- Bumping `pydantic` pin without dual-mode schema diff and core-schema oracle rerun on decorated owners; treat as silent validator graph drift.
- Adding marker-only decorators to satisfy lint without synthesis, validation, or signature-preserving wrap semantics; collapse to real admission or remove.
- Post-upgrade default arm widening in `model_validator` bodies to absorb dependency behavior changes; version bump and migration fold instead.
- Reordering policy stacks without seam failure-injection replay; observability and retry semantics change without proof.
- Using `beartype` or `validate_call` after dependency upgrade to repair annotations that should be fixed at definition time; enforcement decorators are not evolution sponges.
- Maintaining parallel field registries in test helpers instead of `model_fields`, `type_info`, or compiled schema walks; harness drift diverges from production decorator evidence.
- Generative tests sampling raw dicts filtered by runtime validation on decorated owners; wastes cycles and misses lawful shrink paths through admitted validator phases.
- Registry decorator edits without updating dispatch keys and collected signatures; catalog evolution without handler proof.

# Rejections

- Marker-only decorators that perform no validation, synthesis, registration, or signature-preserving wrap.
- `Callable[..., Any]` erasure and untyped decorator factories.
- Post-definition class mutation (`setattr` on instances or classes) to add fields or policies hidden from checkers.
- Chained shape nesting: type → constant → wrapper class → domain concept spread across four owners.
- God decorators that mix trace, validate, register, decode, and dispatch in one opaque wrapper.
- Domain `try`/`except` or `if`/`elif` ladders inside decorators where `match`, validators, or typed rails own the branch law.
- Using runtime validation to repair missing static shape (`TypedDict` erased to `dict`, optional fields repaired by `get` defaults).
- `typing.cast` or `Any` at decorator boundaries to silence checker failures.
- Assuming Python decorator application order equals pydantic-core runtime validator order.
- Using `beartype` or `validate_call` to compensate for erased handler signatures that should be fixed at definition time.
- Decorating one leaf class with both pydantic synthesis and msgspec struct layout for the same concept.
- Domain modules importing ingress Pydantic models, boundary payloads, or msgspec wire structs as durable owner types.
- `@field_validator` or `@model_validator` on canonical owners that duplicate wire law already enforced on ingress projections.
- Serializer or `@computed_field` admission that reopens constructor input for derived slots at seams.
- Calling `.raw_function` on `@validate_call` wrappers from default foreign ingress paths without root trust documentation.
- Re-validating materialized canonical values through ingress `TypeAdapter` at interior sites without declared re-ingress boundary.
- Registry or catalog decorators that register handlers with erased signatures or unparameterized domain types at composition root.
- Policy wrappers at seams that catch `ValidationError` and rethrow undifferentiated errors, erasing `loc` evidence for boundary mapping.
- Settings validators copied onto canonical domain records solely to re-read environment at transform time.
- Parallel OpenAPI or wire field tables diverging from `model_json_schema()` and adapter JSON Schema authority.
- Reading or mutating raw `__annotations__` instead of `annotationlib.get_annotations`; using `inspect.get_annotations` or `eval()` on string annotations when `annotationlib.Format.VALUE` suffices.
- Subclass decoration that re-synthesizes `__init__` on an already-admitted pydantic or dataclass owner; competing `dataclass_transform` markers on one inheritance chain or multiply-inherited synthesis bases.
- Generic parameter erasure at decorator boundaries (`TypeVar` replaced with `Any` inside wrapper `**kwargs`).
- Metaclass or `__init_subclass__` field injection after the defining module finishes import; decorator factories that rebuild `ConfigDict` or `BeartypeConf` per call site instead of closing over module constants.
- Catching `ValidationError` inside decorator factories or policy wrappers at domain interior layers.
- Verifying stacks solely by reading source decorators without `model_fields`, `type_info`, or `__pydantic_core_schema__` evidence.
- Rebuilding or mutating class registries during call-time policy wrappers.
- Test collection that presents a different signature than the law subject because of incorrect `wraps` on hypothesis stacks.
- Skipping first-touch rebuild tests for PEP 695 generic pydantic owners.
