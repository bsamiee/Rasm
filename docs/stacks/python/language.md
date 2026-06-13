# [PYTHON_LANGUAGE]

Python `>=3.15` is the active language surface. This page is the version-feature law for choosing syntax, type-expression, annotation, import/export, template, and standard-library primitive forms before adding a local abstraction.

`pyproject.toml` owns the interpreter floor, `uv`, and tool configuration facts. This page names those facts only when they change the language form a Python file may assume.

## [1]-[ACTIVE_SURFACE]

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

## [2]-[CANONICAL_CHOOSER]

Use the active Python surface directly. Replace older spellings and local machinery when syntax, typing, or the standard library owns the behavior.

| [INDEX] | [CONCERN]                | [USE]                                                        | [REPLACE]                           |
| :-----: | :----------------------- | :----------------------------------------------------------- | :---------------------------------- |
|   [1]   | runtime annotations      | `annotationlib.get_annotations()`                            | direct `__annotations__` reads      |
|   [2]   | conditional binding      | assignment expressions (`:=`)                                | precondition temporary variables    |
|   [3]   | callable shape           | parameter-preserving decorators                              | `Callable[..., T]` erasure          |
|   [4]   | generic shape            | inline type parameters and `type` aliases                    | `TypeVar`, `ParamSpec`, `TypeAlias` |
|   [5]   | type predicates          | `TypeIs`                                                     | one-way `TypeGuard` predicates      |
|   [6]   | type expressions         | `TypeForm`                                                   | `type[T]` or `object` forms         |
|   [7]   | kwargs payload           | `Unpack[TypedDict]`                                          | homogeneous `**kwargs`              |
|   [8]   | typed dict closure       | `closed=` and `extra_items=`                                 | open payload prose                  |
|   [9]   | immutable keys           | `ReadOnly[T]` in `TypedDict`                                 | prose-only immutable key contracts  |
|  [10]   | required keys            | `Required[]` and `NotRequired[]`                             | split `TypedDict` inheritance       |
|  [11]   | method override          | `@typing.override`                                           | unmarked subclass overrides         |
|  [12]   | self type                | `typing.Self`                                                | bound `TypeVar` self boilerplate    |
|  [13]   | generic defaults         | type parameter defaults and `NoDefault`                      | overload families for defaults      |
|  [14]   | typed disjointness       | `@typing.disjoint_base`                                      | prose-only disjointness             |
|  [15]   | variadic generic options | `TypeVarTuple` bound, variance, and infer_variance args      | asymmetric variadic-generic shims   |
|  [16]   | closed dispatch          | `match` with pattern narrowing                               | tag `if` chains                     |
|  [17]   | exhaustiveness proof     | `Never` and `assert_never()`                                 | default-arm raises                  |
|  [18]   | sentinel value           | `sentinel()`                                                 | `object()` or string sentinels      |
|  [19]   | immutable update         | `copy.replace()` or `__replace__()`                          | mutate-then-freeze copies           |
|  [20]   | immutable map            | `frozendict`                                                 | tuple-pair pseudo-maps              |
|  [21]   | invariant arity          | `zip(strict=True)`                                           | post-truncation asserts             |
|  [22]   | mapped arity             | `map(strict=True)`                                           | post-truncation asserts             |
|  [23]   | string enum              | `enum.StrEnum`                                               | `str, Enum` mixins                  |
|  [24]   | enum invariant           | `enum.verify()` and `EnumCheck`                              | local enum validation loops         |
|  [25]   | literal text boundary    | `LiteralString`                                              | untyped sensitive `str`             |
|  [26]   | safe templates           | t-strings and processors                                     | f-string pre-parsing                |
|  [27]   | template AST             | `ast.TemplateStr`                                            | f-string AST rewrites               |
|  [28]   | f-string expression      | full-expression f-strings                                    | quote juggling temporaries          |
|  [29]   | signature annotations    | `inspect.signature(annotation_format=...)`                   | annotation string post-processing   |
|  [30]   | static reflection        | `inspect.getmembers_static()`                                | descriptor-triggering scans         |
|  [31]   | union introspection      | `typing.get_origin()` and `get_args()`                       | private union implementation checks |
|  [32]   | protocol introspection   | `typing.get_protocol_members()` and `typing.is_protocol()`   | private protocol probes             |
|  [33]   | generic bases            | `types.get_original_bases()`                                 | direct `__orig_bases__` reads       |
|  [34]   | execution locals         | explicit `locals=` and `frame.f_locals`                      | mutating `locals()` snapshots       |
|  [35]   | frame locals type        | `types.FrameLocalsProxyType`                                 | private frame-locals proxy checks   |
|  [36]   | AST field schema         | `ast.AST._field_types`                                       | hand-maintained ASDL maps           |
|  [37]   | AST equality             | `ast.compare()`                                              | `ast.dump()` string comparison      |
|  [38]   | optimized AST            | `ast.parse(optimize=...)`                                    | local AST pruning                   |
|  [39]   | source module name       | `module=` compile APIs                                       | filename-only warning filters       |
|  [40]   | cold import              | tool-admitted module-scope `lazy import` or `lazy from`      | local-import startup hacks          |
|  [41]   | startup hook             | `.start` entries                                             | executable `.pth` import lines      |
|  [42]   | UTF-8 default            | UTF-8 default; `encoding="locale"`                           | locale-dependent implicit text I/O  |
|  [43]   | I/O protocol             | `io.Reader` and `io.Writer`                                  | `typing.IO` pseudo-protocols        |
|  [44]   | buffer protocol          | `collections.abc.Buffer`                                     | `ByteString` or bytes prose         |
|  [45]   | generic slice            | `slice[T]`                                                   | unparameterized slice contracts     |
|  [46]   | buffer flags             | `inspect.BufferFlags`                                        | magic integer buffer flags          |
|  [47]   | resource materialization | `importlib.resources.as_file()`                              | `__file__` extraction loops         |
|  [48]   | TOML parse               | `tomllib` TOML 1.1.0                                         | `tomli` or parser shims             |
|  [49]   | file traversal           | `Path.walk()`                                                | stringly `os.walk()` flow           |
|  [50]   | file tree transfer       | `Path.copy()` and `Path.move()`                              | `shutil` transfer wrappers          |
|  [51]   | file type cache          | `Path.info`                                                  | repeated `stat()` probes            |
|  [52]   | path glob match          | `PurePath.full_match()`                                      | ad hoc recursive glob predicates    |
|  [53]   | symlink traversal        | `recurse_symlinks=` and `follow_symlinks=`                   | manual symlink branch logic         |
|  [54]   | missing canonical path   | `os.path.realpath(strict=os.path.ALLOW_MISSING)`             | symlink-prefix resolution loops     |
|  [55]   | path root split          | `os.path.splitroot()`                                        | drive/root string slicing           |
|  [56]   | path subclassing         | `PurePath.with_segments()`                                   | path wrapper propagation            |
|  [57]   | file URI path            | `Path.from_uri()`                                            | hand-parsed `file:` URLs            |
|  [58]   | file MIME type           | `mimetypes.guess_file_type()`                                | path use of `guess_type()`          |
|  [59]   | URL component presence   | `missing_as_none=` and `keep_empty=`                         | empty-string sentinel logic         |
|  [60]   | temporary file reopen    | `NamedTemporaryFile(delete_on_close=...)`                    | `mkstemp()` unlink ladders          |
|  [61]   | tree removal errors      | `shutil.rmtree(onexc=...)`                                   | `onerror` tuple handlers            |
|  [62]   | Windows reserved path    | `os.path.isreserved()`                                       | reserved-name tables                |
|  [63]   | context variable scope   | `ContextVar.set()` token context manager                     | `reset(token)` `finally` blocks     |
|  [64]   | queue lifecycle          | `queue.Queue.shutdown()`                                     | sentinel queue items                |
|  [65]   | worker sizing            | `os.process_cpu_count()`                                     | `os.cpu_count()` worker counts      |
|  [66]   | interpreter isolation    | `concurrent.interpreters`                                    | process-only isolation wrappers     |
|  [67]   | subinterpreter pool      | `concurrent.futures.InterpreterPoolExecutor`                 | process-only CPU pools              |
|  [68]   | process interrupt        | `multiprocessing.Process.interrupt()`                        | cleanup-hostile `terminate()`       |
|  [69]   | iterator sharing         | `threading.serialize_iterator()` or `.concurrent_tee()`      | generator lock wrappers             |
|  [70]   | execution monitoring     | `sys.monitoring`                                             | `settrace()` event scrapers         |
|  [71]   | sampling profiler        | `profiling.sampling`                                         | handwritten timers or `profile`     |
|  [72]   | C-stack dump             | `faulthandler.dump_c_stack()`                                | external native stack probes        |
|  [73]   | live debug attach        | `sys.remote_exec()`                                          | debugger injection hooks            |
|  [74]   | ABI reflection           | `sys.abi_info`                                               | parsed SOABI strings                |
|  [75]   | iterable batching        | `itertools.batched()`                                        | local chunk helper loops            |
|  [76]   | partial holes            | `functools.Placeholder`                                      | lambda wrappers for partial gaps    |
|  [77]   | none predicate           | `operator.is_none()`                                         | one-off `lambda x: x is None`       |
|  [78]   | max heap                 | max-heap `heapq` APIs                                        | negated-priority heap wrappers      |
|  [79]   | dot product              | `math.sumprod()`                                             | `zip()` product folds               |
|  [80]   | multiset XOR             | `Counter` XOR                                                | manual count-difference folds       |
|  [81]   | ordered identifiers      | `uuid.uuid7()`                                               | timestamp-prefixed UUID wrappers    |
|  [82]   | UUID boundaries          | `uuid.NIL` and `uuid.MAX`                                    | magic UUID boundary literals        |
|  [83]   | active exception         | `sys.exception()`                                            | `sys.exc_info()[1]`                 |
|  [84]   | exception context        | `BaseException.add_note()`                                   | message concatenation               |
|  [85]   | regex prefix             | `re.prefixmatch()`                                           | ambiguous `re.match()`              |
|  [86]   | regex parse error        | `re.PatternError`                                            | generic `re.error` catches          |
|  [87]   | target date parsing      | `datetime.date.strptime()` and `datetime.time.strptime()`    | `datetime.strptime()` slicing       |
|  [88]   | file digest              | `hashlib.file_digest()`                                      | manual chunked hash loops           |
|  [89]   | Zstandard payload        | `compression.zstd`                                           | subprocess or bespoke zstd adapters |
|  [90]   | Z85 payload              | `base64.z85encode()` and `z85decode()`                       | local Z85 codecs                    |
|  [91]   | base-N canonical         | `canonical=True` decoders                                    | padding-bit postchecks              |
|  [92]   | base-N format control    | `padded=`, `wrapcol=`, `ignorechars=`                        | pre/post encode formatting          |
|  [93]   | checksum combine         | `zlib.adler32_combine()` and `zlib.crc32_combine()`          | recompress-to-checksum loops        |
|  [94]   | fd buffer read           | `os.readinto()`                                              | `os.read()` copy slices             |
|  [95]   | byte buffer drain        | `bytearray.take_bytes()`                                     | `bytes(buffer)` plus `clear()`      |
|  [96]   | FFI memory view          | `ctypes.memoryview_at()`                                     | `string_at()` copy scaffolds        |
|  [97]   | binary numeric views     | `array` and `memoryview` complex codes                       | struct-packed numeric buffers       |
|  [98]   | integer math             | `math.integer`                                               | float math for integers             |
|  [99]   | fused multiply-add       | `math.fma()`                                                 | rounded multiply-add                |
|  [100]  | float extrema            | `math.fmax()` and `math.fmin()`                              | NaN-aware min/max wrappers          |
|  [101]  | float classification     | `math.isnormal()`, `math.issubnormal()`, `math.signbit()`    | bit-level float probes              |
|  [102]  | fraction conversion      | `Fraction.from_number()`                                     | constructor branch ladders          |
|  [103]  | Unicode identifier       | `unicodedata.isxidstart()` and `unicodedata.isxidcontinue()` | regex identifier tables             |
|  [104]  | grapheme iteration       | `unicodedata.iter_graphemes()`                               | codepoint loops                     |
|  [105]  | Unicode block            | `unicodedata.block()`                                        | block range tables                  |
|  [106]  | XML text validity        | `xml.is_valid_name()` and `xml.is_valid_text()`              | XML character tables                |

## [3]-[LANGUAGE_FORM_CONTRACTS]

Use these contracts when the chooser names the primitive but code still needs a placement rule.

[TYPE_DECLARATION_SITE]:
- Use when: the defining declaration can carry type evidence that callers would otherwise repair downstream.
- Accept: inline type parameters, `type` aliases, `TypeForm` for type-expression values, parameter-preserving callable signatures, type parameter defaults, `NoDefault`, `@typing.override`, `typing.Self`, `@typing.disjoint_base`, and `TypeVarTuple` `bound`, `covariant`, `contravariant`, and `infer_variance` arguments.
- Reject: erased `Callable[..., T]`, imported `ParamSpec` where inline `**P` can express the decorator, remote alias repair, broad `type[T]` or `object` placeholders for type-form values, unmarked overrides, prose-only disjointness, bound-self boilerplate, and protocol shells created only to type an existing object.
- Boundary: `TypeForm`, disjointness, and override evidence are static typing contracts; runtime validation, object-family policy, decorator architecture, protocol ports, and package-backed typing decisions belong to the owning concept page.

[TYPE_PREDICATE_SITE]:
- Use when: a reusable predicate proves exact type membership that inline narrowing cannot express.
- Accept: `TypeIs[T]` over the concrete target or owned structural target, where `T` is compatible with the input type and the predicate is true exactly for `T`.
- Reject: subtype-compatible predicates written as `TypeGuard`, subset predicates disguised as membership proofs, bool helpers followed by `cast`, and runtime-checkable protocols created only to satisfy `isinstance`.
- Soundness: `TypeIs[T]` is a biconditional type-membership proof, not a validation rail for valid, non-empty, active, normalized, positive, or otherwise filtered `T` values.
- Consumption: keep predicate use at ingress, dispatch, or projection boundaries; fold the narrowed value through one expression or the owning result rail instead of spreading branch bodies.
- Boundary: untyped package gaps enter as `object` at the boundary; typed object-shape ownership, validation, and provider adoption belong to the owning concept page.

Disjoint nominal owner: use when runtime class membership owns the variant split and the predicate replaces protocol or cast repair.

```python conceptual
from typing import TypeIs, assert_type, final
from typing_extensions import disjoint_base


class Shape:
    @disjoint_base
    class _Seal: ...


    class RefinedShape(_Seal): ...


    @final
    class OtherShape(_Seal): ...


type Member = Shape.RefinedShape | Shape.OtherShape


def is_refined(value: Member) -> TypeIs[Shape.RefinedShape]:
    return isinstance(value, Shape.RefinedShape)


def projected(value: Member) -> Member:
    return (
        assert_type(value, Shape.RefinedShape)
        if is_refined(value)
        else assert_type(value, Shape.OtherShape)
    )
```

Tagged generic owner: use when one parameterized model owns the family and literal state proves the narrower member.

```python conceptual
from dataclasses import dataclass
from typing import Literal, TypeIs, assert_type


type RefinedTag = Literal["<refined>"]
type OtherTag = Literal["<other>"]
type Tag = RefinedTag | OtherTag


@dataclass(frozen=True, slots=True)
class Shape[T: Tag]:
    tag: T


type Member = Shape[RefinedTag] | Shape[OtherTag]


def is_refined(value: Member) -> TypeIs[Shape[RefinedTag]]:
    return value.tag == "<refined>"


def projected(value: Member) -> Member:
    return (
        assert_type(value, Shape[RefinedTag])
        if is_refined(value)
        else assert_type(value, Shape[OtherTag])
    )
```

[TYPED_DICT_PAYLOAD_SITE]:
- Use when: keyword or dictionary payload shape is part of the callable contract.
- Accept: `Unpack[TypedDict]` for keyword payloads, `closed=` for static exact-key constraints, `extra_items=` for typed extension slots, `Required[]`, `NotRequired[]`, and `ReadOnly[T]` on individual keys.
- Reject: homogeneous `**kwargs`, open payload prose, split `TypedDict` inheritance for required-key bookkeeping, mutable-key promises in comments, `Mapping[str, object]` bags, and runtime validation used to repair erased static payload shape.
- Boundary: `TypedDict` shapes static payload compatibility; rich domain objects, ingress validation, serialization policy, and provider payload mapping belong to the owning boundary or domain concept page.

```python conceptual
from enum import StrEnum
from typing import NotRequired, ReadOnly, Required, TypedDict, Unpack

from expression import Option


class Field(StrEnum):
    KEY = "<field-a>"
    VALUE = "<field-b>"


class Row(TypedDict, total=False, closed=True):
    key: Required[ReadOnly[str]]
    field: NotRequired[ReadOnly[Field]]


def selected(**row: Unpack[Row]) -> str:
    key = row["key"]
    return Option.of_optional(row.get("field")).map(
        lambda field: f"<result-a>:{key}:{field}"
    ).default_with(lambda: f"<result-b>:{key}")


SELECTED_RESULT = selected(key="<key-a>", field=Field.KEY)
```

[MODULE_BOUNDARY_SITE]:
- Use when: a module declares its public names or imports another module surface.
- Accept: named imports, end-of-file `__all__`, and `__init__.py` only when it owns real package initialization or a public package contract.
- Reject: wildcard imports, barrel files, facade-only exports, empty `__init__.py` package markers, re-export files that hide the real owner, and package markers that exist only to make traversal or imports look tidy.
- Boundary: package topology, generated API documentation, public package contracts, and source-symbol documentation belong to their owning platform, architecture, or code-documentation surface.

[LAZY_IMPORT_SITE]:
- Use when: a cold dependency should remain declared at the module boundary without paying import cost until first use.
- Accept: module-scope `lazy import` and `lazy from` statements for named modules or named imported members only when formatter, linter, type checker, and runtime configuration admit the syntax.
- Reject: function-local import hiding, `importlib` laziness scattered through call sites, `lazy` inside functions, classes, or `try` blocks, lazy star imports, lazy future imports, and `__lazy_modules__` in target-only code where direct `lazy` imports can state the boundary.
- Boundary: global lazy-import modes, startup policy, dependency graph costs, and tool graph truth belong to the runtime or platform owner.

[STARTUP_ENTRY_SITE]:
- Use when: interpreter startup code must be declared as an auditable startup entry point.
- Accept: `.start` entries in mandatory `pkg.mod:callable` form for zero-argument startup hooks.
- Reject: executable `.pth` import lines, implicit import side effects, wildcard startup modules, and startup code hidden in package marker files.
- Boundary: package installation layout, site processing, startup ordering, and command behavior belong to the runtime or platform owner.

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
    return (
        template.strings,
        tuple(
            (field.value, field.expression, field.conversion, field.format_spec)
            for field in template.interpolations
        ),
    )


VALUE = "<value-a>"
SELECTED_RESULT = selected(t"<field-a>{VALUE!r:<field-b>}")
```

[CLOSED_MATCH_SITE]:
- Use when: a closed domain must project every case through one operation-local `match` statement.
- Accept: pattern narrowing over the closed variant owner, one operation-local projection, and `case _ as unreachable: assert_never(unreachable)` when the static checker needs an exhaustiveness witness.
- Reject: tag `if` chains, guarded matches that leave cases unproved, catch-all raises, default arms that hide a missing case, and pre-match normalization that erases the discriminant.
- Boundary: variant ownership, error rail shape, and cross-module dispatch architecture belong to the owning domain or surface concept page.

```python conceptual
from dataclasses import dataclass
from typing import assert_never
from typing_extensions import disjoint_base


class Variant:
    @disjoint_base
    class _Seal: ...

    @dataclass(frozen=True, slots=True, kw_only=True)
    class A(_Seal):
        value: str

    @dataclass(frozen=True, slots=True, kw_only=True)
    class B(_Seal):
        value: int


def selected(variant: Variant.A | Variant.B) -> str:
    match variant:
        case Variant.A(value=value):
            return f"<result-a>:{value}"
        case Variant.B(value=value):
            return f"<result-b>:{value}"
        case _ as unreachable:
            assert_never(unreachable)


VARIANT_A = Variant.A(value="<value-a>")
SELECTED_RESULT = selected(VARIANT_A)
```

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

## [4]-[ABSTRACTION_COLLAPSE_TESTS]

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
- Smell: absence, boundary, ordering, identifier limits, UUID limits, base-encoding policy, or buffer flags are represented by strings, objects, numeric sentinels, or literal UUIDs.
- Collapse: use the named sentinel, boundary value, enum, flag, or standard primitive that carries the value semantics.
- Done when: the value carries its own semantics without prose.
