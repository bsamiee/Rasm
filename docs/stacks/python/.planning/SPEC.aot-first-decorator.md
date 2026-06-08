# [AOT_FIRST_DECORATOR_SPEC]

Status: DISCUSSION. Profile: Lightweight. Date: 2026-06-08.

This planning spec captures AOT-first and decorator-first Python design as source material for the Python stack. It is not active doctrine by itself; each rule becomes binding only after promotion to the owner named in the owner map.

## [1]-[PROBLEM]

Python guidance can drift into runtime improvisation: local import shims, marker decorators, string dispatch, handwritten registries, and post-hoc branch checks. That produces code that is hard for agents to reason about because shape, capability, and side effect are discovered late instead of declared at the surface.

The target style is declaration-first Python. Imports, type surfaces, decorator composition, registry rows, templates, and dynamic boundaries should be visible before normal execution paths start.

## [2]-[GOALS]

- Define AOT-first as declaration-first design, not as a native compilation promise.
- Treat decorators as behavior-bearing composition points that preserve callable shape.
- Replace hidden runtime discovery with typed registries, structured templates, and boundary-owned dynamic execution.
- Route each durable rule to the concept page that can own it without bloating `language.md`.

## [3]-[NON_GOALS]

- Do not require a Python JIT, free-threaded runtime, or packaging backend as language law.
- Do not make decorators mandatory when a typed function, class, registry row, or pattern match is the clearer owner.
- Do not preserve research notes, source lists, or agent process details as future instruction text.

## [4]-[DESIGN_LAW]

[AOT_FIRST]:
- [MEANS]: Declare the import graph, typed call surfaces, object shapes, registry rows, and template processors before ordinary runtime flow depends on them.
- [REJECTS]: Local import shims, stringly provider lookup, branch-built registries, import side effects, and dynamic evaluation outside boundary owners.
- [OWNER]: `language.md` for syntax primitives; `runtime.md` and `platform-and-proof.md` for execution proof.

[DECORATOR_FIRST]:
- [MEANS]: Use decorators when definition-time composition is the natural owner for cross-cutting behavior, registration, capability selection, tracing, validation, or adaptation.
- [REJECTS]: Marker-only decorators, erased wrappers, post-definition mutation, and decorators that hide provider or boundary behavior.
- [OWNER]: `surfaces-and-dispatch.md`.

[SHAPE_FIRST]:
- [MEANS]: Build the dense shape from the root: type parameters, records, enum/domain cases, immutable maps, sentinels, and replacement behavior belong in one coherent owner.
- [REJECTS]: Chained shape nesting where a type feeds a constant, the constant feeds a wrapper class, and the wrapper only delays the actual domain concept.
- [OWNER]: `data-shapes.md`.

[BOUNDARY_FIRST]:
- [MEANS]: Dynamic import, dynamic execution, external text, codecs, process calls, and template rendering are explicit boundary surfaces that return typed values or rails.
- [REJECTS]: Domain code carrying strings, provider names, unescaped templates, or runtime module paths.
- [OWNER]: `boundaries.md`.

## [5]-[DECORATOR_REQUIREMENTS]

| [CONDITION] | [PROMOTE] | [REJECT] | [OWNER] |
| :---------- | :-------- | :------- | :------ |
| A wrapper preserves call shape. | Inline `**P`, exact return type, and `functools.wraps`. | `Callable[..., Any]` and untyped wrappers. | `language.md` |
| A wrapper adds leading context. | `Concatenate[Context, P]` at the declaration. | Capturing global context with erased types. | `language.md` |
| A decorator registers a surface. | Typed registry row built at definition time. | Runtime list mutation from scattered modules. | `surfaces-and-dispatch.md` |
| A decorator validates boundary input. | Boundary-owned decoder returning a rail. | Domain decorator that raises or branches internally. | `boundaries.md` |
| A decorator selects provider behavior. | Typed dispatch table or capability object. | String provider switches inside the wrapper. | `surfaces-and-dispatch.md` |

```python
type Handler[T] = Callable[[T], Result[Receipt, HandlerError]]

def register[T: Event](shape: TypeForm[T]) -> Callable[[Handler[T]], Handler[T]]:
    def apply(handler: Handler[T]) -> Handler[T]:
        registry.add(shape, handler)
        return handler

    return apply
```

## [6]-[OWNER_MAP]

| [RULE] | [FUTURE_OWNER] | [PROMOTION_ACTION] |
| :----- | :------------- | :----------------- |
| Inline type parameters, `TypeForm`, decorator callable preservation, lazy import syntax, sentinels, `frozendict`, and `copy.replace`. | `language.md` | Already belongs in active language law. |
| Shape-first object families, closed/extra payloads, immutable data, absence markers, replacement policy, and chained shape nesting. | `data-shapes.md` | Promote as the first concept page. |
| Decorator-first architecture, registry construction, typed dispatch rows, `singledispatch` boundaries, and polymorphic surfaces. | `surfaces-and-dispatch.md` | Promote after shape taxonomy exists. |
| `Option` / `Result` rail conversion, effect composition, and error transport. | `rails-and-effects.md` | Promote with boundary return contracts. |
| Dynamic import, codecs, structured templates, external text, subprocess/sql/shell sinks, and validation branches. | `boundaries.md` | Promote before package/platform rules depend on it. |
| Deferred annotation cost, startup behavior, free-threading, JIT, concurrency, and import graph performance. | `runtime.md` | Promote as runtime policy, not language law. |
| Package `.start`, toolchain proof, filesystem proof, and platform-specific execution records. | `platform-and-proof.md` | Promote after runtime boundaries are stable. |

## [7]-[OPEN_QUESTIONS]

- Which decorator patterns should be mandatory composition points and which should remain ordinary functions or registry rows?
- Where should `singledispatch` be allowed for open extension without weakening closed `match` law?
- How much provider-specific proof belongs in `platform-and-proof.md` before it becomes package documentation instead of stack law?

## [8]-[VALIDATION]

- Keep this file as planning material until each rule is promoted to its named owner.
- Strip source-history, role labels, and research process narration when promoting.
- Do not add active doctrine here after the owning concept page exists.
