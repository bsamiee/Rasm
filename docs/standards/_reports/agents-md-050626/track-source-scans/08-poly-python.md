# [POLY_03_PYTHON]

## [TRANSCRIPT]

- [SCOPE] context / Advanced Polymorphism Python agent. I researched modern Python 3.15 typing and dense Python design for `AGENTS.md` wording, with `tools/assay` as the local proof surface. I did not edit active repo files.
- [SKILL_ROUTE] Loaded `coding-python` because Python typing, Result/Option rails, Protocol-driven services, PEP 695 generics, msgspec/Pydantic, and `pyproject.toml` policy are in scope. The skill frames Python code as polymorphic, functional, strongly typed, programmatic, algorithmic, and AOP-driven.
- [REPO_ROUTE] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/README.md`, `docs/standards/agents-md.md`, `tools/assay/AGENTS.md`, `pyproject.toml`, and the local Python skill references for type and transform policy.
- [LOCAL_CONTEXT] Inspected `tools/assay/core/model.py`, `tools/assay/composition/catalog.py`, `tools/assay/composition/registry.py`, `tools/assay/core/aspect.py`, and `tools/assay/core/engine.py`. The strongest local pattern is not "more classes"; it is one message model, behavior-bearing enum axes, tuple catalog rows, typed `Result` rails, typed aspect layers, and folds into one `Report` and `Envelope`.
- [CURRENT_SOURCES] Checked Python 3.15.0b2 docs, Python typing specification pages, PEP 728, PEP 742, PEP 747, Python `dataclasses`, `enum`, `functools.singledispatch`, `annotationlib`, msgspec, Pydantic, attrs, and expression docs.
- [CONSTRAINT] `pyproject.toml` currently sets `requires-python = ">=3.14"`, `ty` `python-version = "3.14"`, and mypy `python_version = "3.14"`. Python 3.15 features should be stated as the target standard or research posture, not enforced in current repo gates until the manifest and toolchain move.

## [MODERN_CAPABILITIES]

[PYTHON_3_15_TYPING]
- `TypeForm` is the main Python 3.15 typing capability relevant here. It types APIs that accept type expressions, not only runtime classes, so validation/introspection APIs can relate a passed type form to the narrowed or returned value. Use it for boundary validators, type-driven decoders, and analyzer-like tools after tool support proves it. Source: PEP 747, accepted for Python 3.15, last modified 2026-04-21: https://peps.python.org/pep-0747/
- `TypedDict` gained more precise shape vocabulary through PEP 728 typed extra items and closed dictionaries. That is useful for boundary message shapes, but in this repo it should not replace the owning msgspec wire model when `Envelope`, `Report`, `Detail`, and catalog rows already own runtime shape. Source: https://peps.python.org/pep-0728/ and typing spec `TypedDict`: https://typing.python.org/en/latest/spec/typeddict.html
- `TypeIs` remains the correct advanced narrowing tool for complement-safe predicates. It is especially useful at deserialization, FFI, plugin, and `object` boundary checks where a runtime predicate proves a more precise static type. Source: https://peps.python.org/pep-0742/
- PEP 695/696 syntax remains the default generic surface: `type Alias[T]`, `class Box[T]`, `def f[T](...)`, inline `**P` ParamSpec, inline `*Ts`, defaulted type parameters, and inferred variance. The typing spec explicitly covers inline ParamSpec and variance inference for newer generic syntax. Source: https://typing.python.org/en/latest/spec/generics.html
- Python 3.14+ lazy annotations and `annotationlib` matter for tools that inspect annotations. Do not read `__annotations__` directly in analyzer or schema-generation code; use `annotationlib.get_annotations()` or the tool-approved equivalent so forward references and lazy evaluation are handled deliberately. Source: https://docs.python.org/3.15/library/annotationlib.html

[POLYMORPHISM_MEANING]
- Protocols define structural ports. In this repo, use them for adapter boundaries and dependency surfaces, not for duplicating concrete model families. The typing spec defines protocol assignability structurally; explicit inheritance is not required when the shape matches. Source: https://typing.python.org/en/latest/spec/protocol.html
- Generic functions and classes encode relationships. They are strongest when the same type parameter, ParamSpec, or type-form parameter connects input, output, rail error, decorator, or decoder shape.
- Pattern matching is closed-domain projection. Use `match` over union variants, msgspec tagged detail types, enum axes, `Result` tags, and state tuples when the domain is closed. Use `assert_never` on unreachable arms where type checkers can prove exhaustiveness. Python 3.15 reference: https://docs.python.org/3.15/reference/compound_stmts.html
- `functools.singledispatch` is open extension. Use it when external types may register behavior without editing one closed union. It dispatches on the first argument type. Source: https://docs.python.org/3.15/library/functools.html#functools.singledispatch
- Dataclasses are appropriate for internal immutable value objects, local exception payloads, and params surfaces when runtime message decoding is not the owner. Prefer `frozen=True`, `slots=True`, and `kw_only=True` where positional swaps or mutation would weaken invariants. Source: https://docs.python.org/3.15/library/dataclasses.html
- msgspec is appropriate for wire and persisted message models. Its `Struct` supports fast structured data, generated `__match_args__`, frozen instances, tagged unions, and runtime validation while decoding. Local `tools/assay` already uses this as the message-model owner. Source: https://jcristharif.com/msgspec/structs.html
- Pydantic is appropriate at ingress/egress validation, settings, external API parsing, and TypeAdapter boundaries. It should not multiply internal domain models or shadow msgspec wire structs. TypeAdapter validates arbitrary Pydantic-compatible types without forcing a new `BaseModel`. Source: https://pydantic.dev/docs/validation/latest/concepts/type_adapter/
- attrs is a powerful class-building toolkit and can beat dataclasses for validators, converters, slotted behavior, and developer experience. It is not declared in this repo manifest, so it should be research wording only unless the dependency is intentionally added. Source: https://www.attrs.org/en/stable/why.html
- expression `Result` and `Option` turn failure and absence into typed values. Use `Result[T, E]` for fallible operations and `Option[T]` for absence, with `map`, `bind`, `filter`, `or_else_with`, and conversion between Option and Result instead of nullable/exception control flow. Sources: https://expression.readthedocs.io/en/latest/reference/result.html and https://expression.readthedocs.io/en/latest/reference/option.html
- Enum axes with behavior payloads are first-class Python polymorphism. A `StrEnum` or `IntEnum` member can carry CLI flags, route strategy, stream/write flags, suffix sets, prefixes, or slot ordering, so vocabulary, wire token, dispatch key, and behavior remain one object.
- Typed dispatch tables are dense design when rows carry behavior and data together: a tuple of `Tool` rows, `Bind` rows, layer rows, parser decoders, or route policies is better than parallel constants, parser maps, and branch cascades.

## [REPO_TRANSLATION]

- Root policy already says to treat the monorepo as polymorphic, identify canonical object shapes, avoid weak types, avoid imperative branch sprawl, avoid exception-style control flow in domain logic, avoid parallel schemas/models for one concept, and collapse related variants before adding entrypoints. Local refs: `CLAUDE.md:3-10`, `CLAUDE.md:60-87`, `CLAUDE.md:142-146`.
- Root `AGENTS.md` already translates that into instruction-file posture: newest viable standards replace current drift; repeated case families collapse into operation algebras, smart enums, unions, folds, projection carriers, typed receipts, or source-owned tables; nullable/bool/exception/runtime-failure channels convert into typed rails. Local refs: `AGENTS.md:35-49`.
- Current Python manifest reality is `>=3.14`, `ty` all-error, strict mypy, no explicit or unimported `Any`, Pydantic mypy plugin, `expression`, `msgspec`, `pydantic`, `pydantic-settings`, `beartype`, `anyio`, `stamina`, `structlog`, OpenTelemetry, and related tool dependencies. Local refs: `pyproject.toml:1-30`, `pyproject.toml:68-119`.
- `tools/assay` is the concrete local model for advanced Python polymorphism. `Runner`, `Input`, `Language`, and `Mode` carry behavior payloads such as prefixes, flags, route strategy, suffixes, stream flags, and write flags. Local refs: `tools/assay/core/model.py:18-97`.
- `tools/assay` keeps one wire hierarchy: `Base`, `Detail`, tagged detail variants, `AnyDetail`, `Report`, `Envelope`, `Bind`, `Completed`, `Fault`, `Counts`, `Artifact`, and `Match`. That is the "no schema sprawl" proof. Local refs: `tools/assay/core/model.py:166-170`, `tools/assay/core/model.py:275-380`.
- `tools/assay` validates tagged details by round-tripping through one msgspec union decoder, then folds completed receipts into one report. Local refs: `tools/assay/core/model.py:457-463`, `tools/assay/core/model.py:466-470`.
- `BaseParams.bound()` is a local example of polymorphic override instead of registry-specific helper sprawl: package/bridge params can override `_arity`, while the base method owns surplus-token fault construction. Local refs: `tools/assay/core/model.py:400-433`.
- `core/aspect.py` is the decorator/ParamSpec proof surface. `Bind[**P]`, `Hom[**P, T]`, `Layer[**P, T]`, `SpawnLayer[**P, T]`, `Slot`, `_AssayLogger` Protocol, `assemble`, and `compose` express AOP as typed rows and folds. Local refs: `tools/assay/core/aspect.py:34-59`, `tools/assay/core/aspect.py:143-170`.
- `tools/assay/AGENTS.md` already owns the best folder-local wording: one engine over data rows, programs are catalog rows, one envelope crosses every invocation, cross-cutting behavior attaches through ordered aspect seams, axis enums carry payloads, and registration stays data-driven. Local refs: `tools/assay/AGENTS.md:13-34`.

## [AGENTS_MD_WORDING]

[UNIVERSAL_AGENTS_MD]
- `AGENTS.md` wording should remain local behavior, not a Python tutorial. The standards file says a useful overlay carries scope, read behavior, future-standard posture, owner contract, route-away rules, rejections, and local stop behavior. Local refs: `docs/standards/agents-md.md:1-41`.
- Use local extension grammar, not slogans. The accepted form is: when adding a family, name the owner algebra/table/value, require extension of that owner, and reject helpers or parallel models. Local refs: `docs/standards/agents-md.md:81-91`.
- Do not add generic validation ladders or command catalogs to Python folder overlays; route command syntax to README/tool docs and state only folder-specific stop or proof hazards. Local refs: `docs/standards/agents-md.md:60-66`, `docs/standards/agents-md.md:93-110`.
- Reject fragile exact facts unless they are local owner identifiers, route targets, forbidden tokens, or invariants with an owner and refresh trigger. Local refs: `docs/standards/agents-md.md:112-125`.

[COPY_READY_PYTHON_TOOL_OVERLAY]
- Scope: `<path>/` only. Root policy owns universal Python posture; `coding-python` owns language mechanics; the local README owns command syntax and operator workflow.
- When editing a Python module, read the target module fully first, then read the owner module for the model, rail, registry, codec, or effect surface being extended.
- When adding a tool, rail, verb, parser, detail, or stdout shape, extend the existing axis enum, catalog row, registry bind, tagged detail variant, message model, or fold before adding a new object family.
- Keep Python design Protocol-driven at adapter boundaries, generic over caller/callee shape, and `Result`/`Option` based for fallible or absent values.
- Use `match` for closed unions and state tuples; use `singledispatch` only for open external type extension.
- Prefer frozen dataclasses for internal values, msgspec structs for wire/persisted messages, Pydantic TypeAdapter/settings for ingress and config, and attrs only when it is an approved dependency and its validators/converters materially replace local boilerplate.
- Axis enums carry behavior payloads. Do not split vocabulary, CLI token, wire value, route key, and behavior into parallel literals, constants, or maps.
- One message model owns each shape. Do not create Pydantic models, TypedDicts, schemas, helper structs, or wrapper objects for wire shapes already owned by msgspec or by the canonical message model.
- Registration stays data-driven: rows and folds own behavior; helper files, parser protocols, command wrapper classes, and one-use adapters are rejected.
- Stop when a Python behavior change cannot be exercised through a runtime smoke path; report the proof gap instead of claiming operator readiness.

[PYTHON_3_15_FORWARD_WORDING]
- Target Python 3.15 typing where the manifest and type checkers support it: `TypeForm` for APIs that accept type expressions, closed/extra-item `TypedDict` only for boundary dictionaries that do not already have a message-model owner, and `annotationlib` for runtime annotation inspection.
- Until `pyproject.toml` moves from Python 3.14 gates, do not require Python 3.15-only syntax or diagnostics in `AGENTS.md` close rules.

## [ANTI_PATTERNS]

- Parallel Pydantic/BaseModel/schema files for an existing msgspec `Envelope`, `Report`, `Detail`, or catalog shape.
- Free `Literal` aliases that restate a `StrEnum` vocabulary already carrying behavior payloads.
- Helper modules, parser protocol files, command wrapper classes, or one-use functions added because a dispatch row felt too dense.
- Branch cascades over rail, language, mode, or runner when an enum payload, table row, fold, or tagged union can carry the behavior.
- `Optional[T]`, `None`, bool flags, return-code integers, or exceptions used as domain failure signals where `Result[T, E]`, `Option[T]`, `RailStatus`, `Fault`, or tagged detail variants already exist.
- `TYPE_CHECKING`-only annotations for runtime-validated shapes when beartype, msgspec, Pydantic, or Cyclopts needs the real annotation at call time.
- New attrs dependency as style preference. Use attrs only after manifest approval and a concrete validators/converters/slotted-class reason.
- `AGENTS.md` prose that teaches Python docs instead of naming the local owner to extend and the rejected substitute.
- Generic "no helpers" wording without a replacement owner. The replacement must name the row, algebra, enum, fold, tagged case, message model, Protocol, or boundary.

## [CONFIDENCE]

- [HIGH] Local repo translation. The current `tools/assay` files directly demonstrate behavior-bearing enum axes, msgspec wire ownership, `Result` rails, ParamSpec decorators, Protocol ports, and data-row registration.
- [HIGH] `AGENTS.md` wording constraints. `docs/standards/agents-md.md` clearly bans tutorial prose, command catalogs, metadata spam, current-baseline caveats, and route-free slogans.
- [HIGH] Current Python 3.15 source basis for `TypeForm`, `TypedDict` extra/closed shapes, lazy annotations, Protocols, generics, dataclasses, enum payloads, `singledispatch`, msgspec, Pydantic TypeAdapter, attrs positioning, and expression Result/Option.
- [MEDIUM] Python 3.15 enforcement timing. Official docs are current to Python 3.15.0b2, but this repo still gates Python as 3.14. The safe recommendation is target wording plus no enforcement until manifest/tool support changes.
- [MEDIUM] attrs. The library is current and relevant, but it is absent from this repo's dependencies. The report therefore treats attrs as a possible future class-building tool, not current repo policy.

