# [STACKS_PYTHON]

This folder is the Python stack decision atlas. It routes language, shape, surface, rail, boundary, runtime, algorithm, platform, and proof decisions to the concept page that owns the coding choice. The atlas builds one declaration-first paradigm: raw ingress becomes typed payload, payload materializes into one canonical owner, owner operations flow through `Option` or `Result`, and projections leave through explicit boundary or egress surfaces.

## [1]-[CHOOSE]

This table is a lookup by reader decision.

| [INDEX] | [DECISION]               | [READ]                                                      | [STATE]  |
| :-----: | :----------------------- | :---------------------------------------------------------- | :------- |
|   [1]   | language syntax          | [language](language.md)                                     | active   |
|   [2]   | PEP-backed action        | [PEP standards](pep-standards.md)                           | active   |
|   [3]   | data shape               | [data shapes](data-shapes.md)                               | active   |
|   [4]   | surface and dispatch     | future `surfaces-and-dispatch.md`                           | next     |
|   [5]   | result and effect flow   | future `rails-and-effects.md`                               | target   |
|   [6]   | boundary and codec       | future `boundaries.md`                                      | target   |
|   [7]   | runtime and concurrency  | future `runtime.md`                                         | target   |
|   [8]   | algorithm and data       | future `algorithms.md`                                      | target   |
|   [9]   | package or platform fact | future `platform/build-and-packages.md`                     | target   |
|  [10]   | proof rail               | future `testing/README.md`                                  | target   |

## [2]-[OWNER_RULE]

Concept pages own coding decisions. Package versions, tool versions, optional dependency groups, and static-analysis configuration live in `pyproject.toml`; package-backed decisions live in the concept page that owns the capability.

Approved libraries are normal Python implementation material. Do not create package-named pages or folders; write the domain rule in the page that owns the decision and mention the package only when it changes implementation choice.

When the stronger package is not admitted yet, keep the target capability in the owning concept and record the adoption gap in the current planning or platform route.

## [3]-[PARADIGM_DECISIONS]

Use these decisions as the manifest for future concept pages. A construct earns space only when it proves lifecycle position, ownership depth, and boundary role.

[PAGE_CRAFT]:
- Decision: each page is an implicit, skill-style source of truth written as one holistic body — dense tables and tight snippets that teach by structure, not prose; it states its own law directly, with no meta-commentary, no cross-file references or ownership narration, and no project, tool, or skill context.
- Law: one page owns one layer and never re-demonstrates another's pattern — `language.md` owns primitives, `pep-standards.md` owns PEP-structural typing, `data-shapes.md` owns shapes and owners, each later page owns its architecture; a sibling concern is neither re-shown nor pointed to, because the reader holds the whole atlas as one body.
- Use: external libraries as first-class native surfaces over a single FP + ROP + `expression` spine — unified rails, ADTs, AOP aspects, and parameterized polymorphism; collapse 2-4 parallel surfaces into one richer one.
- Reject: duplicated atomic examples across pages, cross-file pointers, hardcoded or flat logic, model/wrapper/alias spam, and any snippet whose density does not beat ordinary Python ~3-4x.

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
- Use: `anyio` for structured concurrency, `httpx` for transport boundaries, `cyclopts` for CLI command dispatch, `pydantic-settings` for configuration and `pydantic.validate_call` for boundary admission, `stamina` for retry, `beartype` for runtime type contracts, `expression` for `Option` and `Result`, `structlog` and OpenTelemetry for observability, and standard primitives such as `Path`, `sentinel`, `frozendict`, `StrEnum`, `TypeIs`, and `TypeForm` when they are the exact invariant.
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

## [5]-[SURFACES_DISPATCH_TARGET]

`data-shapes.md` is complete. `surfaces-and-dispatch.md` is the active target. It owns dispatch and aspect ARCHITECTURE — how one polymorphic surface, its dispatch form, its runtime contract, its registry seam, and its aspects compose. It assumes the primitives and shapes the prior pages own and never re-teaches `match`, `TypeIs`, `**P`, `@dataclass_transform`, closed-family shape, or the `frozendict`-table shape.

[SECTIONS]:
- Polymorphic-surface law: one entrypoint owns all modalities; the discriminant is input shape, runtime type, tag, arity, value pattern, or predicate; ≥3 siblings sharing a prefix and return rail collapse into one `@tagged_union` request ADT plus one total fold.
- Dispatch-form chooser: a decision tree of discriminating questions over `match` (owned closed variants) / `singledispatch`(method) (open-by-type, with the ABC-ambiguity pitfall) / `frozendict[K, Callable]` value table (bounded vocabulary key) / `Protocol`+`TypeIs` (structural capability) / `@overload` (static-only variance, with the `get_overloads`+`annotationlib` runtime bridge) / `plum` (N×M positional type matrix) / `cyclopts App` (CLI subcommand token to typed binding).
- Dispatch contracts: static dispatch (`match`/`@overload`/`TypeIs`) is checker-proven and zero-cost; runtime dispatch (`singledispatch`/value table/`Protocol`) decides at call time and needs a contract — `beartype` (violation lifted to `Result` at the boundary) or `pydantic.validate_call` for coercing ingress; free-threaded builds require import-time registration completion.
- Registry and seam: registry taxonomy (`frozendict` closed table / `TypeForm[T]`-keyed open registry / `singledispatch` type-hierarchy / `entry_points` install-time); rows built at definition time returning the original handler; `match` owns closed in-repo domains while a registry or `singledispatch` owns the open boundary; lookup misses return `Option`.
- Aspects: bind each concern to its admitted lib — retry to `stamina`, observability to `structlog` + OpenTelemetry, runtime contract to `beartype`, boundary validation to `pydantic.validate_call`, memoization to `functools.cache`, timeout and scope to `anyio`; aspects materialize policy at definition time, preserve signature, return `Result` instead of raising, stack in deterministic order (cache outside the rail, never caching `Error`), and collapse 2-4 wrappers into one parameterized factory.
- Composition: `pipe`/`compose`/`curry_flip`, Kleisli `result.pipeline`, and `@effect.result` do-notation (`yield from`); config-driven pipelines fold via `compose(*steps)`; the seam where a dispatch surface returns a rail and downstream `.map`/`.bind` compose.
- Async dispatch: the async surface is `async def -> Result[T, E]` (`@effect.result` is sync-only); timeout lifts to `Result` via `anyio.move_on_after`; `stamina` async retry raises at the boundary and converts to `Error`; fan-out runs through an `anyio.TaskGroup` with `except*` into one aggregate rail; `anyio.to_thread`/`to_process` offload sync handlers.
- Collapse tests: a pressure-point scan proving N sibling entrypoints, ad-hoc wrappers, or string-dispatch chains collapse into one polymorphic surface, one parameterized aspect, or one typed table.

[EXTERNAL_LIBS]:
- `expression` (first-class): `pipe`/`compose`/`curry_flip`, `@tagged_union` for closed dispatch, `@effect.result`/`result.pipeline` for ROP composition, `catch` for boundary lift. `effect.result` is a class; `flow` and a standalone `Pipe` do not exist; `result.pipeline` lives in `expression.extra.result`.
- `cyclopts` (first-class CLI dispatch): `App` is the command table, `@app.command` registers arms, `@app.meta.default` is the token-level interceptor, `Parameter`/`Group` own typed binding and validation, `result_action="return_value"` embeds the surface programmatically.
- `stamina` retry, `structlog` + OpenTelemetry observability, `beartype` runtime contracts, `pydantic.validate_call` boundary validation, `anyio` scope/timeout/fan-out/offload, stdlib `functools` (`singledispatch`(method), `cache`, `wraps`, `Placeholder`) + `annotationlib`. Reject `wrapt`.
- `plum-dispatch` admit ONLY for genuine N×M positional type-matrix dispatch (keyword args bypass it); not yet in `pyproject.toml`, so add it there before use.

[ACCEPTANCE]:
- Every dispatch surface is one polymorphic entrypoint returning a rail; no sibling-entrypoint proliferation, string dispatch, or `isinstance` ladder survives.
- Each form is chosen by its discriminating contract; `match` owns closed in-repo domains, registries and `singledispatch` own open boundaries, `cyclopts` owns the CLI boundary, `plum` owns only true N×M.
- Runtime dispatch carries a contract; violations convert to `Result` at the boundary and never raise inside domain flow.
- Aspects bind to the admitted libs, materialize at definition time, preserve signature, and return `Result`; 2-4 wrappers collapse into one parameterized aspect with a stated stack order.
- Async surfaces return `Result` and fan-out collapses to one aggregate rail; sync handlers offload through `anyio`.

## [6]-[CURRENT_CONFLICT_RULE]

When active docs and repository tool configuration disagree, the tool configuration controls executable code and the docs keep the target rule scoped.

[SIGNATURES]:
- Current code uses inline `**P` decorator parameters, exact return types, `Concatenate` when a leading context is real, and `functools.wraps`.
- Imported `ParamSpec` is a historical spelling unless a checker or dependency boundary proves no inline form can express the signature.

[IMPORTS]:
- Current code uses module-scope named imports and no function-local import shims.
- `lazy import` remains a target language form only after the formatter, linter, type checker, and runtime route admit it in tool configuration.
