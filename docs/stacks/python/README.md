# [STACKS_PYTHON]

This folder is the Python stack decision atlas. It routes language, shape, surface, rail, boundary, runtime, algorithm, platform, and proof decisions to the concept page that owns the coding choice. The atlas builds one declaration-first paradigm: raw ingress becomes typed payload, payload materializes into one canonical owner, owner operations flow through `Option` or `Result`, and projections leave through explicit boundary or egress surfaces.

## [1]-[CHOOSE]

This table is a lookup by reader decision.

| [INDEX] | [DECISION]               | [READ]                                                      | [STATE]  |
| :-----: | :----------------------- | :---------------------------------------------------------- | :------- |
|   [1]   | language syntax          | [language](language.md)                                     | active   |
|   [2]   | PEP-backed action        | [PEP standards](pep-standards.md)                           | active   |
|   [3]   | data shape               | future `data-shapes.md`                                     | target   |
|   [4]   | surface and dispatch     | future `surfaces-and-dispatch.md`                           | target   |
|   [5]   | result and effect flow   | future `rails-and-effects.md`                               | target   |
|   [6]   | boundary and codec       | future `boundaries.md`                                      | target   |
|   [7]   | runtime and concurrency  | future `runtime.md`                                         | target   |
|   [8]   | algorithm and data       | future `algorithms.md`                                      | target   |
|   [9]   | package or platform fact | future `platform/build-and-packages.md`                     | target   |
|  [10]   | proof rail               | future `testing/README.md`                                  | target   |
|  [11]   | declaration-first design | [AOT/decorator spec](.planning/SPEC.aot-first-decorator.md) | planning |

## [2]-[OWNER_RULE]

Concept pages own coding decisions. Package versions, tool versions, optional dependency groups, and static-analysis configuration live in `pyproject.toml`; package-backed decisions live in the concept page that owns the capability.

Approved libraries are normal Python implementation material. Do not create package-named pages or folders; write the domain rule in the page that owns the decision and mention the package only when it changes implementation choice.

When the stronger package is not admitted yet, keep the target capability in the owning concept and record the adoption gap in the current planning or platform route.

## [3]-[PARADIGM_DECISIONS]

Use these decisions as the manifest for future concept pages. A construct earns space only when it proves lifecycle position, ownership depth, and boundary role.

[SHAPE_LIFECYCLE]:
- Decision: `Raw -> Payload -> Canonical owner -> Rail/effect -> Projection -> Egress`.
- Law: raw external material never becomes domain truth directly.
- Use: `TypedDict` and decoder inputs before materialization; canonical owners inside domain logic; projections at egress.
- Reject: provider-shaped domain objects, `dict[str, object]` payload drift, wire aliases in interiors, and local shape choice without lifecycle position.

[POLYMORPHISM]:
- Decision: closed internally, semi-closed at versioned boundaries, open only for foreign extension.
- Law: an in-repo concept is closed unless external code can add members without editing the owner.
- Use: total folds, pattern matches, owner-local registry rows, or rich owner methods for closed families; typed extension slots at ingress or egress; `Protocol`, registries, or `singledispatch` at plugin seams.
- Reject: string dispatch, open protocols for one implementation, optional-field variant bags, and `singledispatch` for owned closed vocabularies.

[DECORATOR_ADMISSION]:
- Decision: decorators are admitted only when definition-time composition owns behavior.
- Law: an aspect must preserve signature shape and materialize policy, registration, validation, capability selection, or observability.
- Use: inline `**P`, exact return types, `Concatenate` only for real leading context, and `functools.wraps`.
- Reject: marker-only decorators, erased wrappers, post-definition mutation, hidden provider switches, and decorators that raise inside domain flow.

[RAILS]:
- Decision: `Option` carries non-failing absence; `Result` carries fallibility; exceptions convert at boundaries.
- Law: domain logic returns typed rails instead of using exceptions as control flow.
- Use: sentinels for omitted or inherited parameter state, `Option.none` for valid no-value outcomes, `Result[T, E]` for fallible operations, and typed aggregate failures for grouped or concurrent failures.
- Reject: `None` for failure, `Option` that hides errors, message-concatenated exceptions, and catch/raise ladders inside domain operations.

[MATERIALIZATION]:
- Decision: boundary adapters validate raw material; canonical owners enforce domain invariants; egress validates projection compatibility.
- Law: payload validation, domain invariant enforcement, and wire serialization are lifecycle positions, not automatic separate model families.
- Use: `pydantic` for rich ingress, settings, schema, and semantic validation; `msgspec` for fast wire structs and codecs; frozen dataclasses or rich classes for compact in-process owners when they carry behavior better than schema owners.
- Reject: call-site validation, parallel validation/domain/wire classes with identical semantics, and egress-only validation for domain correctness.

[SCHEMA_MODEL_CREATION]:
- Decision: `data-shapes.md` owns schema, model, class, enum, and payload creation rules.
- Law: no Pydantic model, `msgspec` struct, dataclass, enum, protocol, payload, or rich class appears without a lifecycle role and collapse test.
- Use: Pydantic models for validated ingress, settings, schemas, semantic domain admission, and error-rich materialization; `msgspec` structs for wire codecs and high-volume serialization; enums for stable external vocabularies; rich classes for behavior-dense canonical owners; protocols only for real ports with independent implementers.
- Route: protocol dispatch, registry behavior, and decorator admission move to `surfaces-and-dispatch.md`; rail carriers move to `rails-and-effects.md`; codecs and foreign payload translation move to `boundaries.md`.
- Reject: ad hoc `BaseModel` sprawl, package-named model layers, one-field wrapper classes, enum/string duplication, model-per-provider interiors, and protocol shells that repair weak ownership.

[PACKAGE_OWNERSHIP]:
- Decision: capability owner chooses package or standard-library primitive by invariant, not by habit.
- Law: approved packages are direct implementation material; language primitives remain owners when they carry the invariant directly.
- Use: `anyio` for structured concurrency, `httpx` for transport boundaries, `pydantic-settings` for configuration, `expression` for `Option` and `Result`, `structlog` and OpenTelemetry for observability, and standard primitives such as `Path`, `sentinel`, `frozendict`, `StrEnum`, `TypeIs`, and `TypeForm` when they are the exact invariant.
- Reject: stdlib-first reflexes, package-branded wrappers, thin facades, and public provider leakage.

## [4]-[BUILD_ORDER]

This table is a lookup by target page sequence.

| [INDEX] | [PAGE]                           | [MUST_DECIDE]                        |
| :-----: | :------------------------------- | :----------------------------------- |
|   [1]   | `data-shapes.md`                 | shape, schema, and model ownership   |
|   [2]   | `surfaces-and-dispatch.md`       | polymorphic dispatch and aspects     |
|   [3]   | `rails-and-effects.md`           | rail carriers and failure flow       |
|   [4]   | `boundaries.md`                  | external material and typed adapters |
|   [5]   | `runtime.md`                     | startup, import, and concurrency law |
|   [6]   | `algorithms.md`                  | algorithm families and data flow     |
|   [7]   | `platform/build-and-packages.md` | package admission and tool ownership |
|   [8]   | `testing/README.md`              | type, runtime, and boundary proof    |

## [5]-[DATA_SHAPES_TARGET]

`data-shapes.md` is the first target page. It should be the lifecycle and owner-choice law, not a catalog of Python shape tricks.

[ACCEPTANCE]:
- It classifies every value as raw ingress, typed payload, canonical owner, boundary adapter, rail carrier, projection, or egress.
- It defines how to create Pydantic models, `msgspec` structs, dataclasses, rich classes, enums, protocols, and typed payloads without ad hoc local patterns.
- It chooses closed, semi-closed, or open family posture before dispatch form.
- It separates absence as omitted key, valid `None`, sentinel default, enum/member state, `Option.none`, or typed failure.
- It chooses `TypedDict`, `pydantic`, `msgspec`, frozen dataclass, rich class, `StrEnum`, `Literal`, sentinel, `Option`, or `Result` by lifecycle role.
- It keeps wire tokens, provider names, schema aliases, serialization, command strings, and external payload forms at boundaries.
- It rejects new wrappers, aliases, tiny classes, constants, protocols, or helper functions when one deeper canonical owner can absorb the behavior.

Paths below are relative to `.reports/data-shapes/`.

[SOURCE_CANDIDATES]:
- `shape-system-integration-doctrine/00-unified-shape-integration.md`
- `model-materialization-pipeline/00-ingress-to-materialization.md`
- `typed-payload-contract-surfaces/00-contract-payload-doctrine.md`
- `vocabulary-absence-state-encoding/00-vocabulary-sentinel-doctrine.md`
- `class-family-variant-architecture/00-variant-family-architecture.md`
- `pydantic-domain-shape-engine/00-pydantic-v2-shape-ownership.md`
- `rich-class-owner-design/00-rich-class-owner-mechanics.md`
- `protocols-capabilities-structural-ports/00-structural-capability-doctrine.md`
- `immutable-persistent-replacement-shapes/00-immutable-replacement-doctrine.md`

Use the `00-*` reports as promotion candidates. Use later report files as edge-case evidence, proof stress material, or anti-pattern sources only after the target page has a compact owner law.

## [6]-[CURRENT_CONFLICT_RULE]

When active docs and repository tool configuration disagree, the tool configuration controls executable code and the docs keep the target rule scoped.

[SIGNATURES]:
- Current code uses inline `**P` decorator parameters, exact return types, `Concatenate` when a leading context is real, and `functools.wraps`.
- Imported `ParamSpec` is a historical spelling unless a checker or dependency boundary proves no inline form can express the signature.

[IMPORTS]:
- Current code uses module-scope named imports and no function-local import shims.
- `lazy import` remains a target language form only after the formatter, linter, type checker, and runtime route admit it in tool configuration.
