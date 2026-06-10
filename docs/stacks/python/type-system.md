# [PYTHON_TYPE_SYSTEM]

Python `>=3.15` is the active typing surface. This page is the static-evidence law: declarations, predicates, payloads, and signatures carry type proof where they are declared, and callers never repair downstream what the declaration could have preserved. Each section owns one concern family: the chooser table names the form and the spelling it replaces, the family card states the placement law and names the PEPs that canonize its rows once, and the snippet proves the rule.

## [1]-[TYPE_EVIDENCE]

Static evidence belongs at the declaration, predicate, or branch owner.

| [INDEX] | [CONCERN]             | [USE]                                        | [REPLACE]                           |
| :-----: | :-------------------- | :------------------------------------------- | :---------------------------------- |
|   [1]   | generic shape         | inline type parameters and `type` aliases    | `TypeVar`, `ParamSpec`, `TypeAlias` |
|   [2]   | generic defaults      | type parameter defaults and `NoDefault`      | overload families for defaults      |
|   [3]   | variadic generics     | `*Ts` with bound, variance, `infer_variance` | rank-specific generic classes       |
|   [4]   | type expressions      | `TypeForm`                                   | `object` or broad `type[Any]` forms |
|   [5]   | type predicates       | `TypeIs`                                     | one-way predicates plus `cast`      |
|   [6]   | non-subtype narrowing | `TypeGuard`                                  | `bool` helpers plus `cast`          |
|   [7]   | typed disjointness    | `@typing.disjoint_base`                      | prose-only disjointness             |
|   [8]   | method override       | `@typing.override`                           | unmarked subclass overrides         |
|   [9]   | self type             | `typing.Self`                                | bound `TypeVar` self boilerplate    |
|  [10]   | structural ports      | `Protocol`                                   | nominal ABC shells                  |
|  [11]   | value-sensitive APIs  | `Literal`                                    | checker plugins for flags           |
|  [12]   | finality              | `@typing.final` and `Final`                  | prose-only finality                 |
|  [13]   | generated models      | `@dataclass_transform()`                     | checker-invisible decorators        |
|  [14]   | generic spelling      | built-in collection generics                 | `typing.List`, legacy aliases       |
|  [15]   | union spelling        | `A \| B` and `T \| None`                     | `Union[...]`, `Optional[...]`       |
|  [16]   | I/O protocol          | `io.Reader` and `io.Writer`                  | `typing.IO` pseudo-protocols        |
|  [17]   | generic slice         | `slice[T]`                                   | unparameterized slice contracts     |

[DECLARATION_EVIDENCE]:
- PEPs: 695, 696, 646, 747, 800, 698, 673, 681, 544, 586, 585, 604, 591.
- Use when: the defining declaration can carry type evidence that callers would otherwise repair downstream.
- Accept: inline type parameters, `type` aliases, `TypeForm` for type-expression values, type parameter defaults, `NoDefault`, `*Ts` arguments, `@typing.override`, `typing.Self`, `@typing.disjoint_base`, `Protocol`, `Literal`, `final`, and `@dataclass_transform()`.
- Reject: erased `Callable[..., T]`, remote alias repair, broad `type[T]` or `object` placeholders for type-form values, unmarked overrides, prose-only disjointness or finality, TypeVar farms, rank-specific generic classes, checker plugins for flags, and protocol shells created only to type an existing object.
- Boundary: `TypeForm` carries type-expression values in APIs; `Protocol` is for real structural ports with independent implementers, not callback shells or nominal repair; runtime validation and object-family policy belong to the owning concept page.

[PREDICATE_EVIDENCE]:
- PEPs: 742, 647.
- Use when: a reusable predicate proves exact type membership that inline narrowing cannot express.
- Accept: `TypeIs[T]` over the concrete or owned structural target where the predicate is true exactly for `T`; `TypeGuard` only for non-subtype narrowing; disjoint nominal owners or tagged generic owners as the narrowing seam.
- Reject: subtype-compatible predicates written as `TypeGuard`, subset predicates disguised as membership proofs, bool helpers followed by `cast`, and runtime-checkable protocols created only to satisfy `isinstance`.
- Law: `TypeIs[T]` is a biconditional type-membership proof, not a validation rail for filtered `T` values; predicate use stays at ingress, dispatch, or projection boundaries and folds through one expression or the owning rail.

```python conceptual
from dataclasses import dataclass
from typing import Literal, Protocol, Self, TypeForm, TypeGuard, TypeIs, assert_never
from typing import dataclass_transform, disjoint_base, final, override


type Kind = Literal["<value-a>", "<value-b>"]
type Atom = str | int
type Extension = tuple[str, Atom]
type Batch[*Ts] = tuple[*Ts]


@dataclass_transform(frozen_default=True, slots_default=True, kw_only_default=True)
def record[T: type[object]](cls: T, /) -> T:
    return dataclass(frozen=True, slots=True, kw_only=True)(cls)


class Renders(Protocol):
    def rendered(self, prefix: str, /) -> str: ...


@disjoint_base
class ShapeRoot:
    def rendered(self, prefix: str, /) -> str:
        raise NotImplementedError


@final
@record
class Shape[T: Kind = Literal["<value-a>"]](ShapeRoot):
    kind: T
    key: str
    value: Atom
    extensions: tuple[Extension, ...] = ()

    def renamed(self, key: str, /) -> Self:
        return type(self)(kind=self.kind, key=key, value=self.value, extensions=self.extensions)

    @override
    def rendered(self, prefix: str, /) -> str:
        return f"{prefix}:{self.kind}:{self.key}:{self.value}"


type Member = Shape[Literal["<value-a>"]] | Shape[Literal["<value-b>"]]
type MemberForm = TypeForm[Member]


def accepts(form: MemberForm, /) -> MemberForm:
    return form


def primary(value: Member, /) -> TypeIs[Shape[Literal["<value-a>"]]]:
    return value.kind == "<value-a>"


def extension(value: object, /) -> TypeGuard[Extension]:
    return isinstance(value, tuple) and len(value) == 2 and isinstance(value[0], str)


def packed[*Ts](*items: *Ts) -> Batch[*Ts]:
    return items


def projected(value: Member, prefix: str, /) -> str:
    match value:
        case selected if primary(selected):
            port: Renders = selected.renamed("<field-a>")
            return port.rendered(prefix)
        case Shape(kind="<value-b>") as selected:
            return selected.rendered(prefix)
        case unreachable:
            assert_never(unreachable)
```

## [2]-[PAYLOADS_AND_SIGNATURES]

Payload exactness and call contracts live in the shape and signature, not in prose or runtime repair.

| [INDEX] | [CONCERN]            | [USE]                            | [REPLACE]                           |
| :-----: | :------------------- | :------------------------------- | :---------------------------------- |
|   [1]   | typed mappings       | `TypedDict` shapes               | `dict[str, Any]` payloads           |
|   [2]   | required keys        | `Required[]` and `NotRequired[]` | split total/non-total mirror shapes |
|   [3]   | read-only keys       | `ReadOnly[T]`                    | prose-only immutable key contracts  |
|   [4]   | payload closure      | `closed=` and `extra_items=`     | implicit structural extra keys      |
|   [5]   | kwargs payload       | `Unpack[TypedDict]`              | homogeneous `**kwargs`              |
|   [6]   | callable shape       | inline `**P` and `Concatenate`   | `Callable[..., Any]` erasure        |
|   [7]   | keyword callables    | `Callable[[Unpack[TD]], R]`      | callback `Protocol` shells          |
|   [8]   | positional API       | `/` parameters                   | `*args` parsing                     |
|   [9]   | decorator signatures | parameter-preserving decorators  | erased wrapper signatures           |

[PAYLOAD_SHAPE]:
- PEPs: 589, 655, 705, 728, 692.
- Use when: keyword or dictionary payload shape is part of the static callable, boundary, or data contract.
- Accept: `Unpack[TypedDict]` for keyword payloads, `closed=` for exact-key constraints, `extra_items=` for typed extension slots, and `Required[]`, `NotRequired[]`, `ReadOnly[T]` on individual keys.
- Reject: homogeneous `**kwargs`, open payload prose, split `TypedDict` inheritance for requiredness bookkeeping, `Mapping[str, object]` bags, mutable-key promises in comments, and runtime validation used to repair erased static payload shape.
- Boundary: `ReadOnly` is static payload evidence — runtime immutability belongs to the materialized owner; rich domain objects, ingress validation, and serialization policy belong to the owning shape page.

```python conceptual
from typing import NotRequired, ReadOnly, Required, TypedDict, assert_never


class RowPayload(TypedDict, total=False, extra_items=ReadOnly[Atom]):
    kind: NotRequired[ReadOnly[Kind]]
    key: Required[ReadOnly[str]]
    value: Required[ReadOnly[Atom]]


def materialized(
    row: RowPayload,
    /,
    *,
    default: Kind = "<value-a>",
) -> Member:
    match {"kind": default} | row:
        case {"kind": "<value-a>" | "<value-b>" as kind, "key": key, "value": value, **extensions}:
            return Shape(
                kind=kind,
                key=key,
                value=value,
                extensions=tuple(extensions.items()),
            )
        case unreachable:
            assert_never(unreachable)
```

[SIGNATURE_SHAPE]:
- PEPs: 612, 821, 570.
- Use when: callable shape, decorator aspects, keyword payloads, or positional-only contracts must survive API boundaries.
- Accept: inline `**P`, `Concatenate` only for real leading context, `Callable[[Unpack[TypedDict]], R]` for keyword-callable values, `/` positional-only parameters, and `functools.wraps`.
- Reject: imported `ParamSpec` where inline `**P` can express the decorator, `Callable[..., Any]`, callback `Protocol` shells for keyword-callable aliases, wrapper signatures that erase parameters, and `*args` parsing for positional contracts.
- Law: signatures carry the call contract where the aspect, payload, or positional boundary is declared; `Unpack[TypedDict]` types the implementation `**kwargs` while `Callable[[Unpack[TD]], R]` types the callback value.

```python conceptual
from collections.abc import Callable
from functools import wraps
from typing import Concatenate, Unpack


type Context = tuple[Kind, str]
type RowOperation[T] = Callable[[Unpack[RowPayload]], T]


def with_context[**P, T](
    context: Context,
    /,
) -> Callable[[Callable[Concatenate[Context, P], T]], Callable[P, T]]:
    def bind(operation: Callable[Concatenate[Context, P], T], /) -> Callable[P, T]:
        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> T:
            return operation(context, *args, **kwargs)

        return call

    return bind


@with_context(("<value-a>", "<field-a>"))
def render(context: Context, /, **row: Unpack[RowPayload]) -> str:
    default, prefix = context
    return projected(materialized(row, default=default), prefix)


SELECTED: RowOperation[str] = render
```
