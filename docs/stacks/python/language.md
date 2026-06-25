# [PYTHON_LANGUAGE]

Python `>=3.15` is the active language surface. This page is the version-feature and type-LEVEL evidence law: every static, caller-facing type-form contract that carries no runtime value — declaration evidence, predicate evidence, type-expression values, generated-owner decorators, callable-signature shape, and the language-form placement sites — is fixed here before a local abstraction is added. The value lifecycle is shed by kind: payload materialization, owner selection, and the `Result` rail ride `shapes.md`; standard-library API replacement rides `system-apis.md`; concurrency, isolation, and diagnostics primitives ride `runtime.md`.

`pyproject.toml` owns the interpreter floor, `uv`, and tool configuration facts. This page names those facts only when they change the language form a Python file may assume.

## [01]-[ACTIVE_SURFACE]

[ACTIVE_SURFACE]:
- Interpreter floor: `>=3.15`
- Tool baseline: `uv` owns Python environment, lockfile, dependency, and tool execution entrypoints
- Type gates: strict `ty` and strict `mypy`
- Formatter and lint gate: `Ruff` preview policy
- Encoding baseline: UTF-8 default with explicit persisted-I/O contracts
- Import baseline: module-scope named imports; lazy imports only after tool configuration admits the syntax
- Export baseline: explicit end-of-file `__all__`; no wildcard imports, barrel files, facade exports, or empty `__init__.py` package markers
- Annotation baseline: deferred annotations inspected through annotation APIs

Treat source files as modern Python, not compatibility layers. Remove old imports, shims, typing spellings, package markers, and tool bypasses when the active surface carries the concept directly.

## [02]-[CANONICAL_CHOOSER]

Use the active Python surface directly. This chooser owns language syntax, type-expression, annotation, import/export, template, and typing-protocol forms — the stable language-form law. Standard-library API replacement (paths, files, regex, datetime, numeric primitives, binary codecs, hashing, iteration) is the high-churn surface owned by `system-apis.md`; concurrency, interpreter isolation, and diagnostics primitives are owned by `runtime.md`. Replace an older spelling or local machinery when the active language surface owns the behavior. The chooser groups by form kind, and each group routes to its `[03]` placement card for the rule the table row cannot state.

[TYPE_DECLARATION_FORMS]: which declaration, generated-owner, callable, or generic form carries the type evidence.

| [INDEX] | [CONCERN]                | [USE]                                                     | [REPLACE]                           |
| :-----: | :----------------------- | :-------------------------------------------------------- | :---------------------------------- |
|  [01]   | generic shape            | inline type parameters and `type` aliases                 | `TypeVar`, `ParamSpec`, `TypeAlias` |
|  [02]   | generic defaults         | type parameter defaults and `NoDefault`                   | overload families for defaults      |
|  [03]   | variadic generic options | `TypeVarTuple` bound, variance, and `infer_variance` args | asymmetric variadic-generic shims   |
|  [04]   | callable shape           | inline `**P` parameter-preserving decorators              | `Callable[..., T]` erasure          |
|  [05]   | leading-context callable | `Concatenate[Context, P]`                                 | `*args` context threading           |
|  [06]   | generated owner          | `@dataclass_transform()` field-specifier decorator        | checker-invisible model decorators  |
|  [07]   | self type                | `typing.Self`                                             | bound `TypeVar` self boilerplate    |
|  [08]   | method override          | `@typing.override`                                        | unmarked subclass overrides         |
|  [09]   | finality                 | `@typing.final` and `Final`                               | prose-only finality                 |
|  [10]   | typed disjointness       | `@typing.disjoint_base`                                   | prose-only disjointness             |
|  [11]   | generic slice            | `slice[T]`                                                | unparameterized slice contracts     |
|  [12]   | I/O protocol             | `io.Reader` and `io.Writer`                               | `typing.IO` pseudo-protocols        |
|  [13]   | buffer protocol          | `collections.abc.Buffer`                                  | `ByteString` or bytes prose         |

[TYPE_PREDICATE_AND_EXPRESSION_FORMS]: how membership is proved and a type-expression value is carried.

| [INDEX] | [CONCERN]             | [USE]       | [REPLACE]                           |
| :-----: | :-------------------- | :---------- | :---------------------------------- |
|  [01]   | type predicates       | `TypeIs`    | one-way `TypeGuard` predicates      |
|  [02]   | non-subtype narrowing | `TypeGuard` | `bool` helper plus `cast`           |
|  [03]   | type expressions      | `TypeForm`  | `object` or broad `type[Any]` forms |

[TYPED_DICT_PAYLOAD_FORMS]: how a keyword or dictionary payload states its static key law.

| [INDEX] | [CONCERN]          | [USE]                            | [REPLACE]                          |
| :-----: | :----------------- | :------------------------------- | :--------------------------------- |
|  [01]   | kwargs payload     | `Unpack[TypedDict]`              | homogeneous `**kwargs`             |
|  [02]   | typed dict closure | `closed=` and `extra_items=`     | open payload prose                 |
|  [03]   | required keys      | `Required[]` and `NotRequired[]` | split `TypedDict` inheritance      |
|  [04]   | immutable keys     | `ReadOnly[T]` in `TypedDict`     | prose-only immutable key contracts |

[CLOSED_DISPATCH_AND_VALUE_FORMS]: how a closed domain dispatches, proves exhaustiveness, and carries an immutable value.

| [INDEX] | [CONCERN]            | [USE]                               | [REPLACE]                        |
| :-----: | :------------------- | :---------------------------------- | :------------------------------- |
|  [01]   | conditional binding  | assignment expressions (`:=`)       | precondition temporary variables |
|  [02]   | closed dispatch      | `match` with pattern narrowing      | tag `if` chains                  |
|  [03]   | exhaustiveness proof | `Never` and `assert_never()`        | default-arm raises               |
|  [04]   | string enum          | `enum.StrEnum`                      | `str, Enum` mixins               |
|  [05]   | enum invariant       | `enum.verify()` and `EnumCheck`     | local enum validation loops      |
|  [06]   | sentinel value       | `sentinel()`                        | `object()` or string sentinels   |
|  [07]   | immutable update     | `copy.replace()` or `__replace__()` | mutate-then-freeze copies        |
|  [08]   | immutable map        | `frozendict`                        | tuple-pair pseudo-maps           |
|  [09]   | invariant arity      | `zip(strict=True)`                  | post-truncation asserts          |
|  [10]   | mapped arity         | `map(strict=True)`                  | post-truncation asserts          |

[TEXT_AND_TEMPLATE_FORMS]: how structured text and templates stay typed and parseable.

| [INDEX] | [CONCERN]             | [USE]                     | [REPLACE]                  |
| :-----: | :-------------------- | :------------------------ | :------------------------- |
|  [01]   | literal text boundary | `LiteralString`           | untyped sensitive `str`    |
|  [02]   | safe templates        | t-strings and processors  | f-string pre-parsing       |
|  [03]   | f-string expression   | full-expression f-strings | quote juggling temporaries |
|  [04]   | template AST          | `ast.TemplateStr`         | f-string AST rewrites      |

[REFLECTION_AND_AST_FORMS]: how annotations, members, unions, frames, and AST are read structurally.

| [INDEX] | [CONCERN]              | [USE]                                                      | [REPLACE]                           |
| :-----: | :--------------------- | :--------------------------------------------------------- | :---------------------------------- |
|  [01]   | runtime annotations    | `annotationlib.get_annotations()`                          | direct `__annotations__` reads      |
|  [02]   | signature annotations  | `inspect.signature(annotation_format=...)`                 | annotation string post-processing   |
|  [03]   | static reflection      | `inspect.getmembers_static()`                              | descriptor-triggering scans         |
|  [04]   | union introspection    | `typing.get_origin()` and `get_args()`                     | private union implementation checks |
|  [05]   | protocol introspection | `typing.get_protocol_members()` and `typing.is_protocol()` | private protocol probes             |
|  [06]   | generic bases          | `types.get_original_bases()`                               | direct `__orig_bases__` reads       |
|  [07]   | execution locals       | explicit `locals=` and `frame.f_locals`                    | mutating `locals()` snapshots       |
|  [08]   | frame locals type      | `types.FrameLocalsProxyType`                               | private frame-locals proxy checks   |
|  [09]   | AST field schema       | per-node `ast.<Node>._field_types`                         | hand-maintained ASDL maps           |
|  [10]   | AST equality           | `ast.compare()`                                            | `ast.dump()` string comparison      |
|  [11]   | optimized AST          | `ast.parse(optimize=...)`                                  | local AST pruning                   |
|  [12]   | source module name     | `module=` compile APIs                                     | filename-only warning filters       |

[MODULE_AND_IO_FORMS]: how imports, startup hooks, and text encoding state their boundary.

| [INDEX] | [CONCERN]     | [USE]                                                   | [REPLACE]                          |
| :-----: | :------------ | :------------------------------------------------------ | :--------------------------------- |
|  [01]   | cold import   | tool-admitted module-scope `lazy import` or `lazy from` | local-import startup hacks         |
|  [02]   | startup hook  | `.start` entries                                        | executable `.pth` import lines     |
|  [03]   | UTF-8 default | UTF-8 default; `encoding="locale"`                      | locale-dependent implicit text I/O |

## [03]-[LANGUAGE_FORM_CONTRACTS]

Use these contracts when the chooser names the primitive but code still needs a placement rule.

[TYPE_DECLARATION_SITE]:
- Use when: the defining declaration, the decorator that builds a family of owners, or the callable signature can carry type evidence that callers would otherwise repair downstream.
- Accept: inline type parameters, `type` aliases, `TypeForm[T]` for type-expression values, type parameter defaults, `NoDefault`, `slice[T]`, `io.Reader`/`io.Writer`, `@typing.override`, `typing.Self`, `@typing.disjoint_base`, `@typing.final` and `Final`, `@dataclass_transform()` with `field_specifiers` for a generated-owner decorator, inline `**P` with `Concatenate` for parameter-preserving callable signatures, and `TypeVarTuple` `bound`, `covariant`, `contravariant`, and `infer_variance` arguments.
- Reject: erased `Callable[..., T]`, imported `ParamSpec` or `TypeVar`/`Generic` where inline `**P` and `class C[T]` express the shape, `from typing_extensions import` for any member the active `typing` namespace exports, `slots_default`/non-existent decorator keywords, remote alias repair, broad `type[T]` or `object` placeholders for type-form values, unmarked overrides, prose-only disjointness or finality, bound-self boilerplate, checker-invisible model decorators, and protocol shells created only to type an existing object.
- Boundary: `TypeForm`, disjointness, finality, generated-owner, and signature evidence are static typing contracts that carry no runtime value; runtime validation, object-family policy, aspect architecture, protocol ports, and payload materialization belong to the owning concept page.

[TYPE_PREDICATE_SITE]:
- Use when: a reusable predicate proves exact type membership that inline narrowing cannot express.
- Accept: `TypeIs[T]` over the concrete or owned structural target where the predicate is true exactly for `T`; `TypeGuard[T]` only for genuine non-subtype narrowing; `beartype.door.is_bearable(value, hint)` as the `TypeIs`-returning guard for a parametrized hint, and `beartype.door.is_subhint` for decidable subtype proof over a type-expression registry.
- Reject: subtype-compatible predicates written as `TypeGuard`, subset predicates disguised as membership proofs, bool helpers followed by `cast`, hand-rolled `isinstance` trees over parametrized hints, and runtime-checkable protocols created only to satisfy `isinstance`.
- Soundness: `TypeIs[T]` is a biconditional type-membership proof, not a validation rail for valid, non-empty, active, normalized, positive, or otherwise filtered `T` values; that refinement is `beartype.vale.Is[...]` on a shared `Annotated` alias or the owning result rail.
- Consumption: keep predicate use at ingress, dispatch, or projection boundaries; the narrowed value folds through one total `match` or the owning rail, never a branch body re-spread across the function.

The type-evidence spotlight: a `@dataclass_transform` decorator mints the closed owner family, `@disjoint_base` seals the nominal root over `@final` sealed members, `Self`/`override` carry declaration evidence, inline `**P`/`Concatenate` preserve a context-threading signature woven under `@beartype`, `TypeForm` carries the refinement-alias type-expression value that `door.is_bearable` decides, `is_primary` narrows at the projection boundary, and the total `match` over the sealed members proves exhaustiveness — all static evidence, no value materialized into an owner rail.

```python conceptual
import dataclasses
from collections.abc import Callable
from functools import wraps
from typing import Annotated, Concatenate, Literal, Self, TypeForm, TypeIs
from typing import assert_never, dataclass_transform, disjoint_base, final, override

from beartype import beartype
from beartype.door import is_bearable
from beartype.vale import Is


type Kind = Literal["<value-a>", "<value-b>"]
type Atom = Annotated[int, Is[lambda n: n >= 0]]
type Context = tuple[Kind, str]


@dataclass_transform(frozen_default=True, kw_only_default=True, field_specifiers=(dataclasses.field,))
def record[T: type](cls: T, /) -> T:
    return dataclasses.dataclass(frozen=True, slots=True, kw_only=True)(cls)


@disjoint_base
class Shape:
    def rendered(self, prefix: str, /) -> str:
        raise NotImplementedError


@final
@record
class Primary(Shape):
    key: str
    value: Atom

    def widened(self, value: Atom, /) -> Self:
        return dataclasses.replace(self, value=value)

    @override
    def rendered(self, prefix: str, /) -> str:
        return f"{prefix}:<value-a>:{self.key}:{self.value}"


@final
@record
class Secondary(Shape):
    key: str
    note: str = ""

    @override
    def rendered(self, prefix: str, /) -> str:
        return f"{prefix}:<value-b>:{self.key}:{self.note}"


type Member = Primary | Secondary
ATOM_FORM: TypeForm[Atom] = Atom
ATOM_ADMITS: bool = is_bearable(0, ATOM_FORM)


def is_primary(value: Member, /) -> TypeIs[Primary]:
    return isinstance(value, Primary)


def with_context[**P, T](context: Context, /) -> Callable[[Callable[Concatenate[Context, P], T]], Callable[P, T]]:
    def bind(operation: Callable[Concatenate[Context, P], T], /) -> Callable[P, T]:
        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> T:
            return operation(context, *args, **kwargs)

        return call

    return bind


@with_context(("<value-a>", "<field-a>"))
@beartype
def rendered(context: Context, value: Member, /) -> str:
    _, prefix = context
    projected = value.widened(0) if is_primary(value) else value
    match projected:
        case Primary() as primary:
            return primary.rendered(prefix)
        case Secondary() as secondary:
            return secondary.rendered(prefix)
        case _ as unreachable:
            assert_never(unreachable)
```

[TYPED_DICT_PAYLOAD_SITE]:
- Use when: keyword or dictionary payload shape is part of the static callable contract and only the type form is declared here.
- Accept: `Unpack[TypedDict]` as the keyword-payload type, `closed=` for static exact-key constraints, `extra_items=` for the typed extension slot, and `Required[]`, `NotRequired[]`, `ReadOnly[T]` as per-key static evidence.
- Reject: homogeneous `**kwargs`, open payload prose, split total/non-total `TypedDict` mirror shapes, mutable-key promises in comments, and `Mapping[str, object]` bags.
- Boundary: this site declares the payload type form only; `TypeAdapter` admission, `Unpack` at the root entrypoint, `frozendict` extension-band folding, and the `Result` materialization rail are the value lifecycle owned by the shape page.

[MODULE_BOUNDARY_SITE]:
- Use when: a module declares its public names, imports another module surface, or registers an auditable startup hook.
- Accept: named imports, end-of-file `__all__`, `__init__.py` only when it owns real package initialization or a public package contract, and `.start` entries in mandatory `pkg.mod:callable` form for zero-argument startup hooks.
- Reject: wildcard imports, barrel files, facade-only exports, empty `__init__.py` package markers, re-export files that hide the real owner, executable `.pth` import lines, implicit import side effects, and startup code hidden in package marker files.
- Boundary: package topology, generated API documentation, public package contracts, source-symbol documentation, site processing, and startup ordering belong to their owning platform, architecture, or code-documentation surface.

[LAZY_IMPORT_SITE]:
- Use when: a cold dependency should remain declared at the module boundary without paying import cost until first use.
- Accept: module-scope `lazy import` and `lazy from` statements for named modules or named imported members only when formatter, linter, type checker, and runtime configuration admit the syntax.
- Reject: function-local import hiding, `importlib` laziness scattered through call sites, `lazy` inside functions, classes, or `try` blocks, lazy star imports, lazy future imports, and `__lazy_modules__` in target-only code where direct `lazy` imports can state the boundary.
- Boundary: global lazy-import modes, startup policy, dependency graph costs, and tool graph truth belong to the runtime or platform owner.

[TEMPLATE_STRUCTURE_SITE]:
- Use when: dynamic text must preserve template structure for processing, policy, or AST analysis before rendering.
- Accept: t-string processors, `string.templatelib.Template` and `Interpolation`, `ast.TemplateStr`, and `ast.Interpolation` where code needs static segments, interpolation values, expression text, conversion, or format-spec structure.
- Reject: f-string pre-parsing, rendered-string reparsing, regex extraction from formatted text, hand-built interpolation tuples, and string concatenation used to hide template policy.
- Boundary: t-string processors decide whether to apply conversions and format specs; template security, localization, escaping, rendering, and AST rewrite policy belong to the owning boundary or text-processing concept page.

```python conceptual
from string.templatelib import Template
from typing import Literal


type Conversion = Literal["a", "r", "s"] | None
type InterpolationParts = tuple[object, str, Conversion, str]
type TemplateParts = tuple[tuple[str, ...], tuple[InterpolationParts, ...]]


def selected(template: Template) -> TemplateParts:
    return (template.strings, tuple((field.value, field.expression, field.conversion, field.format_spec) for field in template.interpolations))


VALUE = "<value-a>"
SELECTED_RESULT = selected(t"<field-a>{VALUE!r:<field-b>}")
```

[CLOSED_MATCH_SITE]:
- Use when: a closed domain must project every case through one operation-local `match` statement.
- Accept: structural pattern dispatch over the sealed member set, one operation-local projection, and `case _ as unreachable: assert_never(unreachable)` as the exhaustiveness witness; a `TypeIs` predicate narrows at the projection boundary before the `match`, never as a guarded arm the checker cannot prove total.
- Reject: tag `if` chains, a `case x if predicate(x):` guard arm carrying the only narrowing, catch-all raises, default arms that hide a missing case, and pre-match normalization that erases the discriminant.
- Boundary: the type-evidence spotlight above demonstrates the closed `match` over `@disjoint_base` members; variant ownership, error rail shape, and cross-module dispatch architecture belong to the owning domain or surface concept page.

[SENTINEL_DEFAULT_SITE]:
- Use when: omission or inherited selection must be distinct from every valid domain value, including `None`.
- Accept: module-global `NAME = sentinel("NAME")` values whose variable name matches the sentinel name, reused by `is`, and carried directly in the union type.
- Reject: repeated `sentinel("<name>")` calls for the same semantic value, `object()` defaults, `None` defaults when `None` is domain-valid, string tokens, omitted-vs-supplied overloads with no return-shape change, and bool flags that split one option shape.
- Boundary: queue or task lifecycle, wire payload tags, and protocol tokens use the owning lifecycle API or explicit domain value instead of sentinel payloads.

[FROZENDICT_TABLE_SITE]:
- Use when: immutable mapping rows or nested policy tables need mapping semantics, language-level immutability, and order-insensitive equality or hashing of the `frozendict` value.
- Accept: `frozendict[K, V]` rows with hashable keys; values must also be hashable when the row itself is hashed or used as an outer key.
- Reject: tuple-pair pseudo-maps, sorted-item key normalizers, module-level dictionaries used as immutable policy tables, `MappingProxyType` views over mutable storage, frozen shells around dictionaries, and mutate-then-freeze copy ladders.
- Boundary: `frozendict` is not a `dict` subclass and preserves insertion order for iteration, but order is not equality or value-hash semantics; use tuple pairs or an owning value object when order is semantic. `frozendict` is shallowly immutable; nested values must be immutable or owned by a model.

```python conceptual
from builtins import frozendict
from enum import StrEnum


class Field(StrEnum):
    KEY = "<field-a>"
    VALUE = "<field-b>"


type Row = frozendict[Field, str]


TABLE: frozendict[Row, str] = frozendict({
    frozendict({Field.KEY: "<key-a>", Field.VALUE: "<value-a>"}): "<result-a>",
    frozendict({Field.KEY: "<key-b>", Field.VALUE: "<value-b>"}): "<result-b>",
})

SELECTED_RESULT: str = TABLE[frozendict({Field.VALUE: "<value-a>", Field.KEY: "<key-a>"})]
```

## [04]-[ABSTRACTION_COLLAPSE_TESTS]

Use these tests before keeping a local abstraction beside a language primitive.

[TYPE_EVIDENCE_REPAIR]:
- Smell: a cast, alias, erased callable, fake protocol, runtime-checkable shell, or bool predicate exists only to recover type evidence that the declaration could have preserved.
- Collapse: move evidence into the owner declaration, `TypeIs` predicate, `TypedDict` payload, `TypeForm`, parameter-preserving callable, or real structural owner.
- Done when: callers narrow, call, unpack, or inspect the value without `cast`, wildcard protocols, post-check repair code, or duplicated declaration aliases.

[STRING_RECOVERY]:
- Smell: code parses rendered strings, paths, URLs, annotations, regex intent, warning filters, template fields, AST structure, or source names by hand.
- Collapse: ask the language API for the structured form.
- Done when: the implementation consumes typed components instead of reconstructed text.

[CEREMONY_WRAPPER]:
- Smell: a helper only hides a modern expression, iterator, update, predicate, or primitive operation from the use site.
- Collapse: inline the language form where the invariant executes, or deepen the owning operation if the helper carries real policy.
- Done when: the call site shows the language-owned invariant directly and no one-use helper remains.

[SURFACE_FACADE]:
- Smell: a file exists only to re-export another module, mark a package, or collect names from several owners.
- Collapse: import the real owner by name and publish the module's own `__all__` only where the module owns the public surface.
- Done when: readers can find the implementation owner without traversing wildcard imports, barrels, or empty package files.

[MAGIC_VALUE]:
- Smell: absence, inherited selection, closed-domain membership, or ordering is carried by a bare string, an `object()` token, a numeric sentinel, or a `.value` literal the program already names.
- Collapse: replace the literal with the language form that carries the semantics — `sentinel()` for omission, a `StrEnum`/`Literal` member for closed membership, a `frozendict` row for a derived correspondence, `Never`/`assert_never` for the unreachable arm.
- Done when: the value carries its own semantics through a named language form, and no prose restates a meaning the declaration already holds; stdlib primitive policy (`uuid.NIL`/`uuid.MAX`, base-N decode flags, buffer flags) is the system-API owner's magic-value rule, named there.
