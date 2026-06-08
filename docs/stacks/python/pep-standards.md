# [PYTHON_PEP_STANDARDS]

This page is the compact PEP decision index for Python implementation work. It canonizes the structural, systematic requirements that PEP-backed capabilities impose on Python code: where evidence lives, which surface owns a contract, how variants collapse into one polymorphic owner, and which older local patterns no longer earn space.

The table is an index, not a PEP manual. Keep each row atomic: name the capability family, state the implementation action, and identify the obsolete spelling, wrapper, or local pattern it replaces. Cards expand only when a capability family needs a durable system rule: declaration-site evidence, signature preservation, exact payload shape, primitive-owned invariants, structured runtime material, or another canonical approach that future Python guidance must follow.

## [1]-[PEP_INDEX]

| [INDEX] | [PEP]   | [CATEGORY]            | [USE]                                                   | [REPLACE]                                      |
| :-----: | :------ | :-------------------- | :------------------------------------------------------ | :--------------------------------------------- |
|   [1]   | PEP 747 | Type expressions      | Use `TypeForm` for type-expression values               | `object`, broad `type[Any]`, ad hoc type names |
|   [2]   | PEP 749 | Annotation inspection | Use `annotationlib`                                     | Raw `__annotations__` reads                    |
|   [3]   | PEP 649 | Annotation deferral   | Use unquoted annotations                                | Quoted annotation strings                      |
|   [4]   | PEP 750 | Template strings      | Use `t`-strings for structured interpolation            | Parsing formatted strings                      |
|   [5]   | PEP 810 | Lazy imports          | Declare cold imports with `lazy`                        | Local-import startup hacks                     |
|   [6]   | PEP 829 | Package startup       | Declare startup entrypoints in `.start`                 | Executable `.pth` import lines                 |
|   [7]   | PEP 661 | Sentinel values       | Create named sentinels with `sentinel()`                | `object()` markers and magic strings           |
|   [8]   | PEP 814 | Immutable mappings    | Use built-in `frozendict` for immutable mappings        | Tuple-pair encodings and thin wrappers         |
|   [9]   | PEP 686 | UTF-8 default         | Treat default text encoding as UTF-8                    | Locale-dependent implicit I/O                  |
|  [10]   | PEP 728 | TypedDict extras      | Declare `closed=` or `extra_items=` explicitly          | Implicit structural extra keys                 |
|  [11]   | PEP 742 | Type narrowing        | Use `TypeIs` for bidirectional narrowing                | `bool` plus `cast`                             |
|  [12]   | PEP 800 | Disjoint bases        | Mark bases with `@disjoint_base`                        | Prose-only disjointness                        |
|  [13]   | PEP 695 | Type parameters       | Declare generics inline and aliases with `type`         | TypeVar farms and assignment aliases           |
|  [14]   | PEP 696 | Type defaults         | Default type parameters at the declaration              | Overload-only defaults                         |
|  [15]   | PEP 705 | TypedDict read-only   | Mark read-only TypedDict items with `ReadOnly`          | Comments that items are read-only              |
|  [16]   | PEP 692 | Typed `**kwargs`      | Use `Unpack[TypedDict]` for keyword payloads            | Homogeneous `**kwargs`                         |
|  [17]   | PEP 655 | TypedDict keys        | Mark per-key requiredness explicitly                    | Split total/non-total mirror shapes            |
|  [18]   | PEP 589 | Typed mappings        | Use `TypedDict` shapes                                  | `dict[str, Any]` payloads                      |
|  [19]   | PEP 779 | Free-threaded target  | Treat free-threaded Python as supported                 | Experimental no-GIL caveats                    |
|  [20]   | PEP 703 | Shared mutation       | Synchronize shared mutation                             | Implicit GIL serialization                     |
|  [21]   | PEP 734 | Interpreter isolation | Use `concurrent.interpreters`                           | Process-only isolation wrappers                |
|  [22]   | PEP 684 | Per-interpreter GIL   | Create own-GIL interpreters                             | Process-only CPU isolation                     |
|  [23]   | PEP 567 | Context variables     | Use `ContextVar` context                                | Thread-local async state                       |
|  [24]   | PEP 831 | Native observability  | Preserve frame-pointer build flags                      | Frame-pointer-stripped native builds           |
|  [25]   | PEP 799 | Profiling namespace   | Use `profiling.tracing` or `profiling.sampling`         | Legacy `profile` module                        |
|  [26]   | PEP 768 | Debug attach          | Attach through safe execution points                    | Debugger injection hooks                       |
|  [27]   | PEP 669 | Execution monitoring  | Use `sys.monitoring` event APIs                         | `settrace()` event scrapers                    |
|  [28]   | PEP 578 | Runtime auditing      | Use audit hooks                                         | Monkeypatch security probes                    |
|  [29]   | PEP 765 | Finally control flow  | Keep `return`, `break`, and `continue` out of finally   | Finally control-flow exits                     |
|  [30]   | PEP 654 | Grouped exceptions    | Handle grouped failures with `except*`                  | Single-error collapse                          |
|  [31]   | PEP 678 | Exception notes       | Attach context with `add_note()`                        | Message concatenation                          |
|  [32]   | PEP 758 | Exception syntax      | Omit exception parentheses without `as`                 | Tuple-wrapped handlers                         |
|  [33]   | PEP 706 | Tar extraction        | Use tar extraction filters                              | Trusting archive member paths                  |
|  [34]   | PEP 721 | Sdist extraction      | Extract sdists with `data_filter`                       | Unfiltered source archive extraction           |
|  [35]   | PEP 735 | Dependency groups     | Declare `[dependency-groups]`                           | Requirements-file group sprawl                 |
|  [36]   | PEP 808 | Additive metadata     | Keep additive list/table metadata in `[project]`        | Backend shadow metadata fields                 |
|  [37]   | PEP 784 | Zstandard payloads    | Use `compression.zstd`                                  | Bespoke zstd adapters                          |
|  [38]   | PEP 791 | Integer math          | Use `math.integer` for integer math                     | Float-path integer helpers                     |
|  [39]   | PEP 682 | Signed zero           | Use `z` formatting for signed zero                      | Post-format negative-zero cleanup              |
|  [40]   | PEP 798 | Flattening            | Unpack directly in comprehensions                       | Nested comprehensions and chain adapters       |
|  [41]   | PEP 698 | Override markers      | Mark subclass overrides with `@override`                | Unmarked override contracts                    |
|  [42]   | PEP 673 | Self types            | Return `Self` from subclass-preserving APIs             | Bound `TypeVar` self patterns                  |
|  [43]   | PEP 675 | Literal strings       | Type literal-only sinks with `LiteralString`            | Untyped sensitive `str`                        |
|  [44]   | PEP 612 | Callable parameters   | Preserve decorator shape with `**P` and `Concatenate`   | `Callable[..., Any]`                           |
|  [45]   | PEP 646 | Variadic generics     | Model variadic shape with `*Ts`                         | Rank-specific generic classes                  |
|  [46]   | PEP 647 | One-way guards        | Use `TypeGuard` for non-subtype narrowing               | `bool` plus `cast`                             |
|  [47]   | PEP 681 | Dataclass transforms  | Mark generated models with `@dataclass_transform()`     | Checker-invisible decorators                   |
|  [48]   | PEP 544 | Protocol shapes       | Use `Protocol` contracts                                | Nominal ABC shells                             |
|  [49]   | PEP 586 | Literal values        | Type value-sensitive APIs                               | Checker plugins for flags                      |
|  [50]   | PEP 593 | Annotation metadata   | Use `Annotated` metadata                                | Parallel metadata maps                         |
|  [51]   | PEP 585 | Generic spelling      | Use built-in collection generics                        | `typing.List`, `typing.Dict`, legacy aliases   |
|  [52]   | PEP 604 | Union spelling        | Use `A \| B` and `T \| None`                            | `Union[...]`, `Optional[...]`                  |
|  [53]   | PEP 634 | Structural matching   | Use `match` for closed branch law                       | `if` / `elif` decision ladders                 |
|  [54]   | PEP 701 | F-string grammar      | Use normal expression grammar inside f-strings          | Quote and backslash workarounds                |
|  [55]   | PEP 618 | Invariant arity       | Use `zip(strict=True)`                                  | Post-truncation asserts                        |
|  [56]   | PEP 667 | Locals semantics      | Use `locals()` snapshots and `frame.f_locals` proxies   | Assumed `locals()` write-through               |
|  [57]   | PEP 688 | Buffer protocol       | Accept `Buffer` and `__buffer__` contracts              | `bytes` / `ByteString` buffer aliases          |
|  [58]   | PEP 594 | Removed batteries     | Replace dead standard-library modules                   | Dead-battery imports                           |
|  [59]   | PEP 632 | Packaging removal     | Remove `distutils`                                      | `distutils` builds                             |
|  [60]   | PEP 680 | TOML parsing          | Parse TOML with `tomllib`                               | `tomli` and parser shims                       |
|  [61]   | PEP 597 | Encoding warnings     | Use explicit text encodings                             | Implicit locale I/O                            |
|  [62]   | PEP 615 | Time zones            | Use `zoneinfo.ZoneInfo`                                 | `pytz`-style adapters                          |
|  [63]   | PEP 626 | Debug line tables     | Use `co_lines()`                                        | `co_lnotab` decoding                           |
|  [64]   | PEP 657 | Traceback locations   | Preserve fine-grained code positions                    | Line-only diagnostics                          |
|  [65]   | PEP 709 | Comprehension runtime | Keep comprehensions direct                              | Loop rewrites for comprehension speed          |
|  [66]   | PEP 616 | Affix removal         | Use affix removers                                      | Slice/strip affix hacks                        |
|  [67]   | PEP 584 | Dict merge            | Use dict union operators                                | Copy/update merge ladders                      |
|  [68]   | PEP 572 | Conditional binding   | Bind condition-local values with `:=`                   | Duplicated condition expressions               |
|  [69]   | PEP 570 | Positional API        | Use `/` parameters                                      | `*args` parsing                                |
|  [70]   | PEP 591 | Final contracts       | Mark final names and classes                            | Prose-only finality                            |
|  [71]   | PEP 574 | Pickle buffers        | Use protocol 5 out-of-band buffers                      | Copy-heavy pickle blobs                        |
|  [72]   | PEP 821 | Keyword callables     | Type keyword callables with `Callable[[Unpack[TD]], R]` | Callback `Protocol` shells                     |

## [2]-[PEP_FAMILY_CONTRACTS]

[TYPE_STRENGTH]:
- PEPs: PEP 747, PEP 742, PEP 800, PEP 695, PEP 696, PEP 646, PEP 647, PEP 698, PEP 673, PEP 681, PEP 544, PEP 586, PEP 585, PEP 604, PEP 634, PEP 591.
- Use when: the PEP row moves static proof into type expressions, predicates, aliases, generic owners, protocols, overrides, or closed branch law.
- Accept: `TypeForm`, `TypeIs`, `TypeGuard` only for non-subtype narrowing, `@disjoint_base`, inline type parameters, type defaults, `*Ts`, `Protocol`, `Literal`, `Self`, `@override`, `@dataclass_transform()`, built-in generic and union spellings, `match`, and `final`.
- Reject: `object` type-form carriers, broad `type[Any]`, bool predicates followed by `cast`, prose-only disjointness, TypeVar farms, rank-specific generic classes, fake nominal ABC shells, checker plugins for flags, legacy `typing.List` or `typing.Dict`, `Union`, `Optional`, and unmarked override or finality contracts.
- Law: static evidence belongs at the declaration, predicate, or branch owner; `TypeIs` proves exact membership, not filtered validity.
- Boundary: `TypeForm` carries type-expression values in APIs; `Protocol` is for real structural ports with independent implementers, not callback shells or nominal repair.

```python conceptual
from dataclasses import dataclass
from typing import Literal, Self, TypeIs, assert_never, final


type Kind = Literal["<value-a>", "<value-b>"]


@final
@dataclass(frozen=True, slots=True)
class Shape[T: Kind = Literal["<value-a>"]]:
    kind: T
    key: str
    value: str

    def renamed(self, key: str, /) -> Self:
        return type(self)(kind=self.kind, key=key, value=self.value)


type Member = Shape[Literal["<value-a>"]] | Shape[Literal["<value-b>"]]


def primary(shape: Member, /) -> TypeIs[Shape[Literal["<value-a>"]]]:
    return shape.kind == "<value-a>"


def projected(shape: Member, /) -> str:
    match shape:
        case value if primary(value):
            selected = value.renamed("<field-a>")
            return f"<result-a>:{selected.key}:{selected.value}"
        case Shape(kind="<value-b>", key=key, value=value):
            return f"<result-b>:{key}:{value}"
        case _ as unreachable:
            assert_never(unreachable)
```

[TYPED_PAYLOADS]:
- PEPs: PEP 728, PEP 705, PEP 655, PEP 589.
- Use when: dictionary payload shape is part of the static callable, boundary, or data contract.
- Accept: `TypedDict`, `closed=`, `extra_items=`, `Required`, `NotRequired`, and `ReadOnly` items.
- Reject: `dict[str, Any]`, implicit extra keys, split mirror shapes for requiredness, and comments that claim item immutability.
- Law: payload exactness, requiredness, read-only evidence, and deliberate extension slots belong in the payload shape, not in prose or runtime repair.
- Boundary: `ReadOnly` is static payload evidence; runtime immutability belongs to the materialized domain owner.

```python conceptual
from dataclasses import dataclass
from typing import Literal, NotRequired, ReadOnly, Required, TypedDict, assert_never


type ExtensionValue = str | int
type Extension = tuple[str, ExtensionValue]
type Kind = Literal["<value-a>", "<value-b>"]


@dataclass(frozen=True, slots=True)
class Shape[T: Kind = Literal["<value-a>"]]:
    kind: T
    key: str
    value: str
    extensions: tuple[Extension, ...] = ()


type Member = Shape[Literal["<value-a>"]] | Shape[Literal["<value-b>"]]


class RowPayload(TypedDict, total=False, extra_items=ReadOnly[ExtensionValue]):
    key: Required[ReadOnly[str]]
    value: Required[ReadOnly[str]]
    kind: NotRequired[ReadOnly[Kind]]


def materialized(row: RowPayload, /) -> Member:
    extensions = tuple(
        (field, value)
        for field, value in row.items()
        if field not in ("key", "kind", "value")
    )

    match row.get("kind", "<value-a>"):
        case "<value-a>":
            return Shape(
                kind="<value-a>",
                key=row["key"],
                value=row["value"],
                extensions=extensions,
            )
        case "<value-b>":
            return Shape(
                kind="<value-b>",
                key=row["key"],
                value=row["value"],
                extensions=extensions,
            )
        case _ as unreachable:
            assert_never(unreachable)
```

[CALLABLE_SIGNATURES]:
- PEPs: PEP 612, PEP 692, PEP 821, PEP 570.
- Use when: callable shape, decorator AOP, keyword payloads, or positional-only contracts must survive API boundaries.
- Accept: `ParamSpec`, `Concatenate`, `Callable[[Unpack[TypedDict]], R]`, `Unpack[TypedDict]`, and `/` positional-only parameters.
- Reject: `Callable[..., Any]`, homogeneous `**kwargs`, callback `Protocol` shells for keyword-callable aliases, wrapper signatures that erase parameters, and `*args` parsing for positional contracts.
- Law: signatures carry the call contract where the aspect, payload, or positional boundary is declared.
- Boundary: `Unpack[TypedDict]` types the implementation `**kwargs`; `Callable[[Unpack[TD]], R]` types the callback value.

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass
from functools import wraps
from typing import Concatenate, Literal, NotRequired, ReadOnly, Required, TypedDict, Unpack


type Kind = Literal["<value-a>", "<value-b>"]

@dataclass(frozen=True, slots=True)
class Context:
    kind: Kind
    prefix: str


class RowPayload(TypedDict, total=False, closed=True):
    key: Required[ReadOnly[str]]
    value: Required[ReadOnly[str]]
    kind: NotRequired[ReadOnly[Kind]]


type RowOperation[T] = Callable[[Unpack[RowPayload]], T]


def contextual[**P, T](
    context: Context,
    operation: Callable[Concatenate[Context, P], T],
    /,
) -> Callable[P, T]:
    @wraps(operation)
    def projected(*args: P.args, **kwargs: P.kwargs) -> T:
        return operation(context, *args, **kwargs)

    return projected


def build(context: Context, /, **row: Unpack[RowPayload]) -> str:
    kind = row.get("kind", context.kind)
    return f"{context.prefix}:{kind}:{row['key']}:{row['value']}"


SELECTED: RowOperation[str] = contextual(
    Context(kind="<value-a>", prefix="<field-a>"),
    build,
)
```

[COLLECTION_INVARIANTS]:
- PEPs: PEP 661, PEP 814, PEP 791, PEP 682, PEP 798, PEP 618, PEP 709, PEP 616, PEP 584, PEP 572.
- Use when: a language primitive carries an invariant for values, mappings, numeric formatting, iteration, or collection composition.
- Accept: `sentinel()`, `frozendict`, `math.integer`, signed-zero formatting, unpacking comprehensions, `zip(strict=True)`, `map(strict=True)`, direct comprehensions, affix removers, dict union, and assignment expressions.
- Reject: `object()` sentinels, magic strings, tuple-pair pseudo-maps, float-path integer helpers, post-format negative-zero cleanup, chain adapters, post-truncation asserts, loop rewrites for comprehension speed, slice/strip hacks, copy/update ladders, and duplicated condition expressions.
- Law: keep the invariant where the primitive executes; `math.integer` belongs on integer algorithms, and `z` formatting belongs at display sinks.

```python conceptual
from builtins import frozendict, sentinel


MISSING = sentinel("MISSING")

type Row = frozendict[str, str]

POLICY: Row = frozendict({
    "<field-a>": "<value-a>",
    "<field-b>": "<value-b>",
})


def selected(
    fields: tuple[str, ...],
    values: tuple[str, ...],
    /,
    *,
    fallback: str | MISSING = MISSING,
    overlay: Row = frozendict(),
) -> Row:
    fallback_row = (
        frozendict({"<field-b>": fallback})
        if fallback is not MISSING
        else frozendict()
    )
    row = frozendict(
        (field, normalized)
        for field, value in zip(fields, values, strict=True)
        if (normalized := value.removeprefix("<field-a>:").removesuffix(":<field-b>"))
    )
    return POLICY | fallback_row | overlay | row
```

[ANNOTATION_RUNTIME]:
- PEPs: PEP 749, PEP 649, PEP 593.
- Use when: annotations or annotation metadata must become runtime material without string evaluation.
- Accept: unquoted deferred annotations, `annotationlib`, and `Annotated` metadata.
- Reject: raw `__annotations__` reads, annotation-string evaluation, annotation regexes, and parallel metadata maps.
- Law: annotation consumers ask the annotation runtime for structured values; they do not reconstruct meaning from strings.

```python conceptual
import annotationlib
from builtins import frozendict
from collections.abc import Callable
from typing import Annotated, get_args


type Policy = frozendict[str, str]

POLICY: Policy = frozendict({"<field-a>": "<value-a>"})
type Field = Annotated[str, POLICY]


def materialized(operation: Callable[[Field], str], /) -> tuple[Callable[[Field], str], Policy]:
    annotations = annotationlib.get_annotations(
        operation,
        format=annotationlib.Format.VALUE,
    )
    _, policy = get_args(annotations["field"])
    return operation, policy


def selected(field: Field, /) -> str:
    return field


SELECTED, SELECTED_POLICY = materialized(selected)
```

[TEXT_TEMPLATES]:
- PEPs: PEP 750, PEP 675, PEP 686, PEP 597, PEP 701, PEP 706, PEP 721, PEP 680, PEP 615.
- Use when: text, templates, archives, project data, or time zones must be structured before policy runs.
- Accept: t-string processors, `LiteralString`, UTF-8 defaults, explicit persisted-text encodings, normal f-string grammar, tar and sdist filters, `tomllib`, and `zoneinfo.ZoneInfo`.
- Reject: parsing rendered strings, untyped sensitive `str`, locale-dependent persisted I/O, archive path trust, `tomli` shims, `pytz` adapters, and stringly timezone policy.
- Law: policy consumes structured material, not reconstructed text or trusted transport names.

[IMPORT_STARTUP]:
- PEPs: PEP 810, PEP 829, PEP 667.
- Use when: imports, startup hooks, or locals views become runtime values.
- Accept: module-scope `lazy` imports, `.start` entries, `locals()` snapshots, and `frame.f_locals` proxies.
- Reject: local-import startup hacks, executable `.pth` import lines, hidden package-marker side effects, and assumed `locals()` write-through.
- Law: evaluation and startup boundaries must be visible where the module declares them.

[FREE_THREADING]:
- PEPs: PEP 779, PEP 703, PEP 567.
- Use when: shared mutation, context propagation, or supported-target claims depend on free-threaded execution.
- Accept: free-threaded Python as a supported target, explicit synchronization for shared mutation, and `ContextVar` for async or thread context.
- Reject: experimental no-GIL caveats, implicit GIL serialization, thread-local async state, mutable ambient globals, and import-time singleton mutation as coordination.
- Law: free-threaded code makes mutation ownership and context propagation explicit before relying on scheduling or cache behavior.

[INTERPRETERS]:
- PEPs: PEP 734, PEP 684.
- Use when: interpreter isolation, independent execution, or CPU separation owns the runtime boundary.
- Accept: `concurrent.interpreters` and own-GIL interpreters.
- Reject: process-only isolation wrappers where interpreter isolation is the owner and shared module state across interpreter boundaries.
- Law: interpreter boundaries own isolation directly; process wrappers survive only when the process boundary is the actual requirement.

[DIAGNOSTICS]:
- PEPs: PEP 831, PEP 799, PEP 768, PEP 669, PEP 578, PEP 626, PEP 657.
- Use when: runtime-owned evidence should explain execution behavior, performance, security observation, or failure location.
- Accept: frame-pointer-preserving builds, profiling namespaces, safe debug attach points, `sys.monitoring`, audit hooks, `co_lines()`, and fine-grained traceback locations.
- Reject: frame-pointer-stripped native builds, legacy `profile`, debugger injection hooks, `settrace()` event scrapers, monkeypatch security probes, `co_lnotab` decoding, and line-only diagnostics.
- Law: diagnostics use runtime-owned observation surfaces; private hooks and timing loops do not replace event, stack, or source-location evidence.

[EXCEPTION_FLOW]:
- PEPs: PEP 765, PEP 654, PEP 678, PEP 758.
- Use when: exception structure, grouped failure transport, or handler syntax changes control-flow semantics.
- Accept: `except*`, `BaseException.add_note()`, exits kept out of `finally`, and unparenthesized exception handlers without `as`.
- Reject: single-error collapse, message concatenation, `return`, `break`, or `continue` that exits `finally`, tuple-wrapper noise, and handler branches that erase grouped-failure identity.
- Law: exception flow preserves the failure set and causal context; syntax cleanup is allowed only when it keeps the handled shape visible.

[BINARY_TRANSPORT]:
- PEPs: PEP 688, PEP 574, PEP 784.
- Use when: binary payloads, buffers, compressed data, or serialized streams cross boundaries.
- Accept: `Buffer`, `__buffer__`, protocol 5 out-of-band buffers, and `compression.zstd`.
- Reject: `bytes` or `ByteString` buffer aliases, copy-heavy pickle blobs, bespoke zstd adapters, subprocess compression shells, and pre-decoded byte piles that discard buffer ownership.
- Law: binary boundaries carry buffer, compression, and serialization semantics directly.

[PROJECT_METADATA]:
- PEPs: PEP 735, PEP 808.
- Use when: project metadata shape changes how dependency groups or additive dynamic values are represented.
- Accept: `[dependency-groups]` and static list/table metadata in `[project]` when the backend only adds entries.
- Reject: requirements-file group sprawl, backend shadow metadata fields, and sidecar notes that make dependency or metadata values harder to inspect.
- Law: static project facts stay visible in the metadata surface that declares them; dynamic metadata is reserved for values that cannot be known at declaration time.

[REMOVALS]:
- PEPs: PEP 594, PEP 632.
- Use when: a removed standard-library or packaging surface still appears in code, docs, or build policy.
- Accept: maintained replacements for removed batteries and non-`distutils` build surfaces.
- Reject: dead-battery imports, `distutils` builds, compatibility aliases, and wrapper facades around removed modules.
- Law: removed surfaces are replacement targets, not compatibility baselines.
