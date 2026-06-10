# [STACKS_PYTHON]

This folder is the Python stack decision atlas. It routes language, type, shape, surface, rail, runtime, and proof decisions to the concept page that owns the coding choice. The atlas builds one declaration-first paradigm: raw ingress becomes typed payload, payload materializes into one canonical owner, owner operations flow through `Option` or `Result`, and projections leave through explicit boundary or egress surfaces.

## [1]-[ATLAS]

This table is a lookup by reader decision.

| [INDEX] | [DECISION]                       | [READ]                        | [STATE] |
| :-----: | :------------------------------- | :---------------------------- | :------ |
|   [1]   | language syntax and standards    | [language](language.md)       | active  |
|   [2]   | type evidence and payloads       | [type system](type-system.md) | partial |
|   [3]   | data shape                       | [shapes](shapes.md)           | active  |
|   [4]   | surface and dispatch             | `surfaces-and-dispatch.md`    | planned |
|   [5]   | rail and effect flow             | `rails-and-effects.md`        | planned |
|   [6]   | concurrency, binary, diagnostics | [runtime](runtime.md)         | partial |
|   [7]   | proof rail                       | `testing/README.md`           | planned |

## [2]-[DOCTRINE]

Thirteen laws in five groups govern every Python decision in this stack. Concept pages instantiate them; no page restates them.

[FLOW]:
- `EXPRESSION_SPINE` — all domain logic is expression-shaped and composes monadically; statements survive only inside measured kernels and platform-forced boundaries, and any page that shows one names the exemption. Composition runs through `pipe`, `compose`, `@effect.result`, comprehensions, and `match` used as an expression of record.
- `BOUNDARY_ADMISSION` — raw material is admitted exactly once into an evidence-carrying owner; interior code never re-validates and never sees `None`-as-failure, sentinels, or provider shapes. The lifecycle is `Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`: `Option` carries non-failing absence, `Result` carries fallibility, exceptions convert at the owning boundary.

[SHAPE]:
- `SHAPE_BUDGET` — a concept owns exactly one type; variants are cases inside one closed family, never sibling types. Families are closed internally, semi-closed at versioned boundaries, and open only where foreign code must extend without editing the owner. Three or more parallel shapes, sibling factories, repeated dispatch arms, or single-call helpers is the collapse trigger, not a style preference.
- `DEEP_SURFACES` — prefer one rich polymorphic surface over many shallow ones; each module exposes one entrypoint family and keeps internals private. One deep owner that holds a full concern beats four fragments that scatter it.
- `MODAL_ARITY` — one entrypoint owns all call modalities; singular, plural, batch, and stream discriminate on the shape of the input value — type, tag, pattern, or arity — never on name suffixes or boolean knobs. A parallel parameter that describes the input is a knob smuggled back in; the discriminant must be recoverable from the value itself.

[DERIVATION]:
- `POLICY_VALUES` — configuration enters as one domain value that carries its own behavior — a vocabulary member, tagged variant, or frozen policy table — never as flag sets whose combinations the implementation must re-derive. Behavior rows live with the vocabulary that selects them.
- `DERIVED_LOGIC` — when cases share generative structure, the logic is derived — a `frozendict` table, fold, or comprehension — never enumerated arms. One primary correspondence is declared and every secondary map derives from it.
- `DERIVED_TYPES` — types are computed where the language allows so one declaration yields the family: inline type parameters with defaults, `TypeForm`, variadic generics, and `@dataclass_transform` owners replace rank-specific or per-provider type copies.
- `SYMBOLIC_REFERENCE` — names, paths, discriminants, and correspondences travel as symbols and derived values — enum members, `Path` algebra, vocabulary tables — never as string literals that restate something the program already knows.

[MATERIAL]:
- `LIBRARY_DEPTH` — admitted packages are the standard library: `expression` for rails and composition, `pydantic` for admission and settings, `msgspec` for wire codecs, `anyio` for structured concurrency, `cyclopts` for CLI binding, `stamina` for retry, `beartype` for runtime contracts, `structlog` and OpenTelemetry for observability. Use the deepest primitive the package itself reaches for; wrappers, rename adapters, and stdlib-first reflexes are rejected. Language primitives remain owners only when they carry the invariant directly.
- `DEFINITION_TIME_ASPECTS` — cross-cutting capability — retry, observability, contracts, validation, memoization, registration — attaches at definition time as a signature-preserving, rail-preserving decorator with inline `**P` and `functools.wraps`. Aspects materialize policy, stack in deterministic order, and never raise inside domain flow; two to four recurring wrappers collapse into one parameterized aspect factory.

[INTEGRATION]:
- `ROOT_REBUILD` — new capability is woven into the owning shape as if it had always existed; reshape the owner rather than appending beside it. No shims, compat aliases, deprecation layers, or migration surfaces — break the API when the collapse improves the system.
- `ONE_HOP_RESOLUTION` — a name resolves to its semantics in one hop: no alias-to-constant-to-enum-to-class chains, no re-export or barrel files, no helper or util shells, no convenience wrappers. A value that takes two jumps to trace marks a layer to delete.

## [3]-[COLLAPSE_SCAN]

Run this scan on every edit. Any signal triggers the move; three or more instances make it mandatory.

| [INDEX] | [SIGNAL]                                          | [MOVE]                                   |
| :-----: | :------------------------------------------------ | :--------------------------------------- |
|   [1]   | sibling names share a prefix or suffix            | one modality-polymorphic entrypoint      |
|   [2]   | same return rail, signatures differ only by arity | input-shape discrimination               |
|   [3]   | functions differ only by a literal                | parameterize; the literal becomes policy |
|   [4]   | boolean parameter selects between two bodies      | one derived body or one policy value     |
|   [5]   | function calls exactly one other function         | delete the hop                           |
|   [6]   | parallel dispatch arms repeat structure           | table or fold algebra                    |
|   [7]   | several types share fields for one concept        | one closed family                        |
|   [8]   | wrapper renames a package API                     | use the package surface directly         |
|   [9]   | the same 2-4 wrappers recur together              | one parameterized aspect                 |

## [4]-[PAGE_CRAFT]

How pages in this folder are authored. The corpus is one body; these laws keep it coherent.

- Atlas law: this README owns doctrine and routing; each concept page owns one disjoint layer; a sibling concern is neither re-shown nor pointed to. The README is the only file that links.
- Zero meta: concept pages carry no provenance, sourcing, version narration, process or planning state, project, tool, or skill context.
- Page grammar: narrow index table, then family cards (`Use / Accept / Reject / Law / Boundary`), then the snippet beside the rule it proves, then collapse tests, then a validation list. Structure is identical across stack folders; content never is.
- Snippet law: every snippet compiles under the active surface; identifiers are legal neutral names; placeholder strings such as `"<value-a>"` appear only inside literals; no project, host, or domain concept anchors a snippet.
- Snippet coverage: each snippet is doctrine-exemplary at full operator depth, roughly 3-4x denser than ordinary code, and exercises a surface region no other snippet in the corpus shows — variety within the doctrine, zero duplicated demonstrations.
- Reject columns are load-bearing: every `Use` names the spelling, wrapper, or local pattern it deletes.
- Tables enumerate, cards legislate: rows stay atomic and narrow — no prose cramming, no links inside cells; nuance moves to a card.
- Planning is quarantined: build order, target-page scopes, and conflict rules live only in the README roadmap tail.
- Manifest truth: package and tool versions live in `pyproject.toml`; no package-named pages or folders; a package is named only where it changes the implementation choice. When a stronger package is not yet admitted, the target capability stays in the owning concept and the adoption gap is recorded in the roadmap.

## [5]-[ROADMAP]

Planned pages in build order. Each entry states what the page must decide; the scope moves into the page when it is authored and leaves this tail.

[SURFACES_AND_DISPATCH]:
- Owns: dispatch and aspect architecture — how one polymorphic surface, its dispatch form, its runtime contract, its registry seam, and its aspects compose. Assumes primitives and shapes from prior pages; never re-teaches `match`, `TypeIs`, `**P`, `@dataclass_transform`, closed-family shape, or the `frozendict` table.
- Surface law: one entrypoint owns all modalities and the discriminant is recoverable from the input value — shape, runtime type, tag, arity, or pattern; collapsing arity must not smuggle the knob back in, so a parallel parameter that re-describes the input — a batch, many, or mode flag beside the value — is the rejected form, stated explicitly.
- Dispatch chooser: `match` for owned closed variants; `singledispatch` for open-by-type seams; `frozendict[K, Callable]` for bounded vocabulary keys; `Protocol` plus `TypeIs` for structural capability; `@overload` for static-only variance; `plum` only for genuine NxM positional type matrices; `cyclopts App` for the CLI token boundary.
- Dispatch contracts: static dispatch is checker-proven and zero-cost; runtime dispatch decides at call time and carries a contract — `beartype` violations lift to `Result` at the boundary, `pydantic.validate_call` coerces ingress; free-threaded builds require import-time registration completion.
- Registry taxonomy: `frozendict` closed table, `TypeForm[T]`-keyed open registry, `singledispatch` hierarchy, `entry_points` install-time; rows built at definition time returning the original handler; lookup misses return `Option`.
- Aspects: retry binds to `stamina`, observability to `structlog` and OpenTelemetry, runtime contracts to `beartype`, boundary validation to `pydantic.validate_call`, memoization to `functools.cache`, timeout and scope to `anyio`; cache sits outside the rail and never caches `Error`. Reject `wrapt`.
- Async dispatch: the async surface is `async def -> Result[T, E]`; timeout lifts through `anyio.move_on_after`; fan-out runs through an `anyio.TaskGroup` with `except*` into one aggregate rail; sync handlers offload through `anyio.to_thread` or `to_process`.
- Library facts: `effect.result` is a class; `flow` and a standalone `Pipe` do not exist; `result.pipeline` lives in `expression.extra.result`; `plum-dispatch` enters `pyproject.toml` before use.

[RAILS_AND_EFFECTS]:
- Owns: carrier law and failure flow — `Option` versus `Result` selection, Kleisli composition, `@effect.result` do-notation, aggregate fault carriers, structured-concurrency rail composition, retry, timeout, and cancellation policy as rail policy.

[TESTING]:
- Owns: type, runtime, and boundary proof rails for the Python stack, mirroring the shared testing page grammar.

[CONFLICT_RULES]:
- When active docs and repository tool configuration disagree, tool configuration controls executable code and the docs keep the target rule scoped.
- Signatures: current code uses inline `**P`, exact return types, `Concatenate` only for real leading context, and `functools.wraps`; imported `ParamSpec` is a historical spelling unless a checker or dependency boundary proves no inline form expresses the signature.
- Imports: current code uses module-scope named imports and no function-local import shims; `lazy import` remains a target form until formatter, linter, type checker, and runtime configuration admit the syntax.
- Typing floor: `@disjoint_base` is spelled from `typing` and payload extension bands use `extra_items=`; when an active checker rejects a floor spelling, tool configuration controls executable code and the page keeps the target spelling.
