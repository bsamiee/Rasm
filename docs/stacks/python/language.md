# [PYTHON_LANGUAGE]

Python `>=3.15` is the active language surface. `pyproject.toml` owns the interpreter floor and tool configuration; this page owns syntax, type-expression, annotation, import, template, and standard-library primitive selection.

Concept pages own modeling, dispatch architecture, rails, boundary validation, runtime concurrency, algorithms, package-backed capability, and proof rails. Use this page to choose the language form before adding a local abstraction.

## [1]-[ACTIVE_SURFACE]

[ACTIVE_SURFACE]:
- Interpreter floor: `>=3.15`
- Type gates: strict `ty` and strict `mypy`
- Formatter and lint gate: Ruff preview policy
- Encoding baseline: UTF-8 default with explicit persisted-I/O contracts
- Import baseline: module-scope imports, with explicit lazy imports for cold dependencies
- Annotation baseline: deferred annotations inspected through annotation APIs

Treat source files as modern Python, not compatibility layers. Remove old imports, shims, and typing spellings when the language now carries the concept directly.

## [2]-[CANONICAL_CHOOSER]

Use this table to choose the language surface before adding a local abstraction.

| [INDEX] | [CONCERN]        | [USE]                                     | [REPLACE]                                  |
| :-----: | :--------------- | :---------------------------------------- | :----------------------------------------- |
|   [1]   | closed dispatch  | `match` and exhaustive pattern narrowing  | tag `if` chains                            |
|   [2]   | generic shape    | inline type parameters and `type` aliases | `TypeVar`, `ParamSpec`, `TypeAlias` blocks |
|   [3]   | callable shape   | parameter-preserving decorators           | `Callable[..., T]` erasure                 |
|   [4]   | type predicates  | `TypeIs`                                  | broad `TypeGuard`                          |
|   [5]   | type expressions | `TypeForm`                                | `type[T]` or `object` for type forms       |
|   [6]   | kwargs payload   | `Unpack[TypedDict]`                       | homogeneous `**kwargs`                     |
|   [7]   | immutable map    | `frozendict`                              | tuple-pair pseudo-maps                     |
|   [8]   | sentinel value   | `sentinel()`                              | `object()` or string sentinel values       |
|   [9]   | cold import      | module-scope `lazy import`                | local-import startup hacks                 |
|  [10]   | safe templates   | t-strings and processors                  | f-string pre-parsing                       |
|  [11]   | immutable update | `copy.replace()` or `__replace__()`       | mutate-then-freeze copies                  |
|  [12]   | file traversal   | `Path.walk()`                             | stringly `os.walk()` flow                  |
|  [13]   | invariant arity  | `zip(strict=True)`                        | silent truncation plus later asserts       |

## [3]-[TYPE_SYSTEM]

[GENERICS]:
- Use inline type parameters on functions, classes, and type aliases.
- Use defaults for type parameters when the default is the true domain default.
- Use `type Name[T] = ...` for aliases and keep aliases close to the owner that consumes them.
- Reject module-level `TypeVar`, `ParamSpec`, `TypeVarTuple`, and `TypeAlias` boilerplate.

[CALLABLES]:
- Preserve decorator and forwarding signatures with parameter specifications and `Concatenate` where the wrapper changes the leading argument.
- Use `Self` for fluent APIs, context managers, and alternative constructors.
- Mark real overrides with `@typing.override`.
- Reject `Callable[..., T]`, `Any`, and wrapper signatures that erase argument evidence.

[NARROWING]:
- Use `TypeIs` for predicates where both branches must narrow safely.
- Reserve non-subtype `TypeGuard` only for the rare boundary where complement narrowing is intentionally impossible.
- Use `LiteralString` for SQL, shell, template, format-string, and command boundaries that require literal text.
- Reject `cast`, stringly guard flags, and plain `str` at injection-sensitive APIs.

[TYPED_DICT]:
- Use closed `TypedDict` payloads for exact object shapes.
- Use typed extra items for extension payloads where additional keys are real capability.
- Use `ReadOnly` for immutable keys and `Unpack[TypedDict]` for heterogeneous `**kwargs`.
- Reject `dict[str, object]`, untyped extras, and prose-only keyword contracts.

## [4]-[DISPATCH_AND_EXPRESSIONS]

Pattern matching is the ordinary language surface for closed value dispatch. Architecture-level dispatch choices belong to `surfaces-and-dispatch.md`; this page only chooses syntax when the owner is already closed.

[DISPATCH_RULES]:
- Use `match` for total value dispatch and keep the fallback arm unreachable by construction.
- Use structural patterns to unpack admitted shapes at the decision point.
- Use `ExceptionGroup` and `except*` only at concurrent or multi-cause failure boundaries.
- Add diagnostic context with exception notes at boundaries; domain flow still uses typed rails.

[EXPRESSION_RULES]:
- Use unpacking comprehensions for direct flattening or merge projections.
- Use `copy.replace()` or owner-defined `__replace__()` for immutable updates.
- Use `zip(strict=True)` when equal length is an invariant.
- Reject mutable accumulator flow, silent `zip` truncation, and local helper extraction for one projection.

## [5]-[ANNOTATIONS_AND_RUNTIME_TYPES]

Deferred annotations are the normal model. Runtime annotation consumers must use the annotation APIs that state the needed format; ordinary code must not read raw `__annotations__`.

[ANNOTATION_RULES]:
- Use `annotationlib` for runtime annotation inspection.
- Use `TypeForm` when an API accepts a type expression rather than a runtime class.
- Use disjoint-base typing only where impossible multiple-inheritance overlap changes static reasoning.
- Reject quoted forward-reference policy, routine `from __future__ import annotations`, direct `__annotations__` reads, and `eval` of annotation strings.

[RUNTIME_TYPE_RULES]:
- Use `Protocol` for structural contracts.
- Use precise type parameters or `object` at validated unknown boundaries.
- Pass explicit namespaces to `exec()` and `eval()` when a boundary truly requires dynamic execution.
- Reject `abc.ABC` for ordinary ports, `typing.Any`, `typing.no_type_check`, and locals-mutation tricks.

## [6]-[IMPORTS_STARTUP_TEMPLATES]

Imports are declared at module scope. Use explicit lazy imports for cold, heavy, optional, or rarely touched modules; keep dynamic import APIs for truly dynamic module names only.

[IMPORT_RULES]:
- Use `lazy import` or `lazy from ... import ...` at module scope for cold dependencies.
- Keep eager imports for modules whose import-time failure should fail startup.
- Reject function-local import deferral, scattered `importlib` laziness, lazy star imports, and lazy future imports.

[STARTUP_RULES]:
- Use `.start` startup entries for auditable startup callables when package startup code is required.
- Reject executable `.pth` import lines and startup side effects hidden in path extension.

[TEMPLATE_RULES]:
- Use t-strings when processors need static and interpolated parts before rendering.
- Use ordinary f-strings only when the rendered string is the final value and no boundary processor must inspect interpolation structure.
- Reject hand-rolled placeholder parsing for SQL, shell, HTML, and log-template processors.

## [7]-[STDLIB_PRIMITIVES]

Use modern standard-library primitives where the language owns the exact concern. If a stronger manifest-backed library owns the domain, route the decision to the concept page instead of documenting a weaker standard-library fallback.

[VALUE_PRIMITIVES]:
- Use `frozendict` for immutable mapping snapshots, hashable dispatch keys, and frozen configuration records.
- Use `Mapping` or `collections.abc.Mapping` when accepting either mutable or immutable map input.
- Use `sentinel("NAME")` for public absence and default markers that need stable identity, representation, pickling, or type-expression support.
- Keep sentinel definitions at module or class scope under the matching name and compare them with `is`.

[IO_PRIMITIVES]:
- Treat UTF-8 as the default text baseline.
- Keep explicit `encoding=` for persisted files, generated artifacts, protocols, and cross-process contracts.
- Use `encoding="locale"` only when locale semantics are the required behavior.
- Use `tomllib` for TOML reads.
- Use `Path.walk()` for path-native tree traversal.
- Reject locale-dependent persisted I/O, `tomli` compatibility shims, and string path traversal when `Path` owns the flow.

## [8]-[REPLACEMENTS]

[TYPE_SYNTAX]:
- Accepted: `type Row[T = object] = tuple[str, T]`
- Rejected: `T = TypeVar("T"); Row: TypeAlias = tuple[str, T]`
- Reason: inline parameters and alias syntax keep the generic owner local to the declaration.

[TYPE_NARROWING]:
- Accepted: `TypeIs` for complement-safe predicates.
- Rejected: broad `TypeGuard` predicates and `cast` recovery.
- Reason: both branches need checker evidence at boundary decisions.

[LAZY_IMPORTS]:
- Accepted: module-scope `lazy import <module>` for cold dependencies.
- Rejected: local imports used only to hide startup cost.
- Reason: the import owner remains visible while load cost is deferred.

[SENTINELS]:
- Accepted: `MISSING = sentinel("MISSING")`.
- Rejected: `MISSING = object()` or `"missing"`.
- Reason: the built-in sentinel owns identity, representation, pickling, and type-expression behavior.

[IMMUTABLE_MAPS]:
- Accepted: `frozendict` for immutable value maps.
- Rejected: tuple-pair maps or `MappingProxyType` when callers need an immutable value.
- Reason: the language now owns immutable mapping value semantics.

## [9]-[REJECTIONS]

- No version-by-version feature lists in Python stack concept pages.
- No compatibility shims for removed stdlib modules or parser-era tools.
- No `typing.Optional`, `typing.Union`, `typing.TypeAlias`, module-level `TypeVar` families, `cast`, or `Any` leakage in durable examples.
- No direct `__annotations__` inspection, annotation-string eval, or locals-mutation policy.
- No helper modules, package-facade wrappers, provider-branded public surfaces, or external-library-specific docs topology.
- No runtime, validation, observability, testing, or algorithm policy in `language.md` when a concept page owns the decision.
