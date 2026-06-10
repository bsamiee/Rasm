# [PYTHON_LANGUAGE]

Python `>=3.15` is the active language surface. This page is the language and standards law for choosing syntax, expression, value, text, template, module, import, filesystem, and project-surface primitive forms before adding a local abstraction. Each section owns one concern family: the chooser table names the form and the spelling it replaces, the family card states the placement law and names the PEPs that canonize its rows once, and the snippet proves the rule.

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

## [2]-[EXPRESSIONS_AND_VALUES]

Invariants live where the primitive executes; values carry their own semantics without prose.

| [INDEX] | [CONCERN]           | [USE]                                   | [REPLACE]                         |
| :-----: | :------------------ | :-------------------------------------- | :-------------------------------- |
|   [1]   | closed dispatch     | `match` with pattern narrowing          | tag `if` chains                   |
|   [2]   | exhaustiveness      | `Never` and `assert_never()`            | default-arm raises                |
|   [3]   | conditional binding | assignment expressions (`:=`)           | precondition temporary variables  |
|   [4]   | sentinel value      | `sentinel()`                            | `object()` or string sentinels    |
|   [5]   | immutable map       | `frozendict`                            | tuple-pair pseudo-maps            |
|   [6]   | dict merge          | dict union operators                    | copy/update merge ladders         |
|   [7]   | immutable update    | `copy.replace()` or `__replace__()`     | mutate-then-freeze copies         |
|   [8]   | string enum         | `enum.StrEnum`                          | `str, Enum` mixins                |
|   [9]   | enum invariant      | `enum.verify()` and `EnumCheck`         | local enum validation loops       |
|  [10]   | invariant arity     | `zip(strict=True)`                      | post-truncation asserts           |
|  [11]   | mapped arity        | `map(strict=True)`                      | post-truncation asserts           |
|  [12]   | flattening          | unpacking comprehensions                | nested flattening loops           |
|  [13]   | comprehension speed | direct comprehensions                   | loop rewrites for speed           |
|  [14]   | affix removal       | `removeprefix()` and `removesuffix()`   | slice/strip affix hacks           |
|  [15]   | iterable batching   | `itertools.batched()`                   | local chunk helper loops          |
|  [16]   | max heap            | max-heap `heapq` APIs                   | negated-priority heap wrappers    |
|  [17]   | dot product         | `math.sumprod()`                        | `zip()` product folds             |
|  [18]   | partial holes       | `functools.Placeholder`                 | lambda wrappers for partial gaps  |
|  [19]   | none predicate      | `operator.is_none()`                    | one-off `lambda x: x is None`     |
|  [20]   | multiset XOR        | `Counter` XOR                           | manual count-difference folds     |
|  [21]   | signed zero         | `z` format specifier                    | post-format negative-zero cleanup |
|  [22]   | target date parsing | `date.strptime()` and `time.strptime()` | `datetime.strptime()` slicing     |

[CLOSED_MATCH]:
- PEPs: 634.
- Use when: a closed domain must project every case through one operation-local `match` statement.
- Accept: pattern narrowing over the closed variant owner, one operation-local projection, and `case _ as unreachable: assert_never(unreachable)` when the static checker needs an exhaustiveness witness.
- Reject: tag `if` chains, guarded matches that leave cases unproved, catch-all raises, default arms that hide a missing case, and pre-match normalization that erases the discriminant.
- Boundary: variant ownership, error rail shape, and cross-module dispatch architecture belong to the owning shape or surface page.

```python conceptual
from dataclasses import dataclass
from typing import assert_never, disjoint_base


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
```

[SENTINEL_DEFAULT]:
- PEPs: 661.
- Use when: omission or inherited selection must be distinct from every valid domain value, including `None`.
- Accept: module-global `NAME = sentinel("NAME")` values whose variable name matches the sentinel name, reused by `is`, and carried directly in the union type.
- Reject: repeated `sentinel("<name>")` calls for one semantic value, `object()` defaults, `None` defaults when `None` is domain-valid, string tokens, omitted-vs-supplied overloads with no return-shape change, and bool flags that split one option shape.
- Boundary: queue or task lifecycle, wire payload tags, and protocol tokens use the owning lifecycle API or explicit domain value instead of sentinel payloads.

[FROZENDICT_TABLE]:
- PEPs: 814.
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

[PRIMITIVE_INVARIANTS]:
- PEPs: 584, 616, 572, 798, 618, 709, 682.
- Use when: a language primitive carries an invariant for values, mappings, numeric formatting, iteration, or collection composition.
- Accept: assignment expressions, dict union, affix removers, unpacking comprehensions, `zip(strict=True)`, `map(strict=True)`, direct comprehensions, and `z` formatting at display sinks.
- Reject: precondition temporaries, copy/update ladders, slice/strip hacks, chain adapters, post-truncation asserts, loop rewrites for comprehension speed, and post-format negative-zero cleanup.
- Law: keep the invariant where the primitive executes; the expression form is the record of the invariant.

```python conceptual
from builtins import frozendict, sentinel
import math.integer as integer


MISSING = sentinel("MISSING")

type FrozenRow = frozendict[str, str]

BASE: FrozenRow = frozendict({"<field-a>": "<value-a>"})


def normalized(
    fields: tuple[str, ...],
    values: tuple[str, ...],
    weights: tuple[int, ...],
    /,
    *,
    fallback: str | MISSING = MISSING,
    overlay: FrozenRow = frozendict(),
) -> FrozenRow:
    names = tuple(
        *field.removeprefix("<prefix>").removesuffix("<suffix>").split(":")
        for field in map(str.strip, fields, strict=True)
        if field
    )
    measured = frozendict(
        (
            name,
            f"{float(integer.gcd(weight, len(value))):z.1f}:{value}",
        )
        for name, raw, weight in zip(names, map(str.strip, values, strict=True), weights, strict=True)
        if (value := raw.removeprefix("<value-a>:").removesuffix(":<value-b>"))
    )
    return (
        BASE
        | (frozendict({"<fallback>": fallback}) if fallback is not MISSING else frozendict())
        | overlay
        | measured
    )
```

## [3]-[TEXT_AND_TEMPLATES]

Policy consumes structured material, not reconstructed text or trusted transport names.

| [INDEX] | [CONCERN]             | [USE]                                            | [REPLACE]                          |
| :-----: | :-------------------- | :----------------------------------------------- | :--------------------------------- |
|   [1]   | safe templates        | t-strings and processors                         | f-string pre-parsing               |
|   [2]   | template AST          | `ast.TemplateStr`                                | f-string AST rewrites              |
|   [3]   | literal text boundary | `LiteralString`                                  | untyped sensitive `str`            |
|   [4]   | f-string expression   | full-expression f-strings                        | quote juggling temporaries         |
|   [5]   | UTF-8 default         | UTF-8 default; `encoding="locale"`               | locale-dependent implicit text I/O |
|   [6]   | persisted encodings   | explicit text encodings                          | implicit locale I/O                |
|   [7]   | regex backtracking    | atomic groups and possessive quantifiers         | scanner split workarounds          |
|   [8]   | regex prefix          | `re.prefixmatch()`                               | ambiguous `re.match()`             |
|   [9]   | regex parse error     | `re.PatternError`                                | generic `re.error` catches         |
|  [10]   | Unicode identifier    | `unicodedata.isxidstart()` and `isxidcontinue()` | regex identifier tables            |
|  [11]   | grapheme iteration    | `unicodedata.iter_graphemes()`                   | codepoint loops                    |
|  [12]   | Unicode block         | `unicodedata.block()`                            | block range tables                 |
|  [13]   | XML text validity     | `xml.is_valid_name()` and `xml.is_valid_text()`  | XML character tables               |
|  [14]   | TOML parse            | `tomllib` TOML 1.1.0                             | `tomli` or parser shims            |
|  [15]   | time zones            | `zoneinfo.ZoneInfo`                              | `pytz`-style adapters              |
|  [16]   | tar extraction        | tar extraction filters                           | trusting archive member paths      |
|  [17]   | sdist extraction      | `data_filter` extraction                         | unfiltered source archives         |

[TEMPLATE_STRUCTURE]:
- PEPs: 750.
- Use when: dynamic text must preserve template structure for processing, policy, or AST analysis before rendering.
- Accept: t-string processors, `string.templatelib.Template` and `Interpolation`, `ast.TemplateStr`, and `ast.Interpolation` where code needs static segments, interpolation values, expression text, conversion, or format-spec structure.
- Reject: f-string pre-parsing, rendered-string reparsing, regex extraction from formatted text, hand-built interpolation tuples, and string concatenation used to hide template policy.
- Boundary: t-string processors decide whether to apply conversions and format specs; template security, localization, escaping, rendering, and AST rewrite policy belong to the owning boundary or text-processing page.

[TEXT_POLICY]:
- PEPs: 675, 686, 597, 701, 706, 721, 680, 615.
- Use when: text, archives, project data, or time zones must be structured before policy runs.
- Accept: `LiteralString` at literal-only sinks, UTF-8 defaults with explicit persisted-text encodings, normal f-string grammar, tar and sdist filters, `tomllib`, and `zoneinfo.ZoneInfo`.
- Reject: parsing rendered strings, untyped sensitive `str`, locale-dependent persisted I/O, archive path trust, `tomli` shims, `pytz` adapters, and stringly timezone policy.

```python conceptual
from pathlib import Path
from string.templatelib import Template
from typing import LiteralString
from zoneinfo import ZoneInfo
import tarfile, tomllib


type PolicyMaterial = tuple[Template, dict[str, object], tuple[str, ...], ZoneInfo]


def materialized_policy(policy: LiteralString, root: Path, zone: LiteralString, /) -> PolicyMaterial:
    data = tomllib.loads((root / "<file>.toml").read_text(encoding="utf-8"))
    zone_info = ZoneInfo(zone)
    view = t"{policy=}:{data['<field>']=}:{zone_info.key=}"
    with tarfile.open(root / "<archive>.tar.gz") as archive:
        safe = tuple(item for member in archive if (item := tarfile.data_filter(member, root / "<folder>")))
        archive.extractall(root / "<folder>", members=safe, filter="data")
    return view, data, tuple(member.name for member in safe), zone_info
```

## [4]-[MODULES_AND_REFLECTION]

Evaluation, startup, and annotation boundaries are visible where the module declares them; annotation consumers ask the runtime for structured values.

| [INDEX] | [CONCERN]                | [USE]                                           | [REPLACE]                           |
| :-----: | :----------------------- | :---------------------------------------------- | :---------------------------------- |
|   [1]   | module exports           | end-of-file `__all__`                           | wildcard imports and barrel files   |
|   [2]   | package markers          | `__init__.py` only with a real package contract | empty package markers               |
|   [3]   | cold import              | tool-admitted module-scope `lazy import`        | local-import startup hacks          |
|   [4]   | startup hook             | `.start` entries                                | executable `.pth` import lines      |
|   [5]   | annotation deferral      | unquoted annotations                            | quoted annotation strings           |
|   [6]   | runtime annotations      | `annotationlib.get_annotations()`               | direct `__annotations__` reads      |
|   [7]   | annotation metadata      | `Annotated` metadata                            | parallel metadata maps              |
|   [8]   | signature annotations    | `inspect.signature(annotation_format=...)`      | annotation string post-processing   |
|   [9]   | static reflection        | `inspect.getmembers_static()`                   | descriptor-triggering scans         |
|  [10]   | union introspection      | `typing.get_origin()` and `get_args()`          | private union implementation checks |
|  [11]   | protocol introspection   | `get_protocol_members()` and `is_protocol()`    | private protocol probes             |
|  [12]   | generic bases            | `types.get_original_bases()`                    | direct `__orig_bases__` reads       |
|  [13]   | execution locals         | explicit `locals=` and `frame.f_locals`         | mutating `locals()` snapshots       |
|  [14]   | frame locals type        | `types.FrameLocalsProxyType`                    | private frame-locals proxy checks   |
|  [15]   | AST field schema         | `ast.AST._field_types`                          | hand-maintained ASDL maps           |
|  [16]   | AST equality             | `ast.compare()`                                 | `ast.dump()` string comparison      |
|  [17]   | optimized AST            | `ast.parse(optimize=...)`                       | local AST pruning                   |
|  [18]   | source module name       | `module=` compile APIs                          | filename-only warning filters       |
|  [19]   | resource materialization | `importlib.resources.as_file()`                 | `__file__` extraction loops         |

[MODULE_BOUNDARY]:
- Use when: a module declares its public names or imports another module surface.
- Accept: named imports, end-of-file `__all__`, and `__init__.py` only when it owns real package initialization or a public package contract.
- Reject: wildcard imports, barrel files, facade-only exports, empty `__init__.py` package markers, re-export files that hide the real owner, and package markers that exist only to make traversal or imports look tidy.
- Boundary: package topology, generated API documentation, and public package contracts belong to the owning platform or architecture surface.

[IMPORT_AND_STARTUP]:
- PEPs: 810, 829, 667.
- Use when: imports, startup hooks, or locals views become runtime values.
- Accept: module-scope `lazy import` and `lazy from` for named modules or members only when formatter, linter, type checker, and runtime configuration admit the syntax; `.start` entries in mandatory `pkg.mod:callable` form for zero-argument startup hooks; `locals()` snapshots and `frame.f_locals` proxies.
- Reject: function-local import hiding, `importlib` laziness scattered through call sites, `lazy` inside functions, classes, or `try` blocks, lazy star or future imports, executable `.pth` import lines, implicit import side effects, startup code hidden in package markers, and assumed `locals()` write-through.
- Boundary: global lazy-import modes, startup ordering, dependency graph costs, and site processing belong to the platform owner.

```python conceptual
# <distribution>.start: <module>:startup
from collections.abc import Callable, Mapping
from types import FrameType, MappingProxyType
lazy import cold_surface

STARTUP_FIELDS = ("<field-a>", "<field-b>")
DECLARED = MappingProxyType(dict(locals()))
type StartupSink[T] = Callable[[Mapping[str, object], Mapping[str, object]], T]

def startup[T](frame: FrameType, /, sink: StartupSink[T] = cold_surface.activate) -> T:
    frame_view = frame.f_locals
    captured = MappingProxyType({name: frame_view[name] for name in STARTUP_FIELDS if name in frame_view})
    return sink(DECLARED, captured)
```

[ANNOTATION_RUNTIME]:
- PEPs: 649, 749, 593.
- Use when: annotations or annotation metadata must become runtime material without string evaluation.
- Accept: unquoted deferred annotations, `annotationlib`, and `Annotated` metadata.
- Reject: raw `__annotations__` reads, annotation-string evaluation, annotation regexes, and parallel metadata maps.
- Law: annotation consumers ask the annotation runtime for structured values; they do not reconstruct meaning from strings.

```python conceptual
import annotationlib
from builtins import frozendict
from collections.abc import Callable
from typing import Annotated, get_args, get_origin


type Policy = frozendict[str, str]
type Tagged = Annotated[str, frozendict({"<prefix>": "<field-a>", "<suffix>": "<field-b>"})]
type AnnotationPolicies = frozendict[str, tuple[Policy, ...]]


def annotation_policies[**P, R](operation: Callable[P, R], /) -> AnnotationPolicies:
    return frozendict({
        name: tuple(
            policy
            for policy in get_args(annotation)[1:]
            if isinstance(policy, frozendict)
        )
        for name, annotation in annotationlib.get_annotations(
            operation,
            format=annotationlib.Format.VALUE,
        ).items()
        if get_origin(annotation) is Annotated
    })


def selected_policy(field: Tagged, /) -> str:
    return field.removeprefix("<field-a>:")


SELECTED_POLICY = annotation_policies(selected_policy)
```

## [5]-[FILESYSTEM_AND_OS]

Ask the platform API for the structured form; never reconstruct path, URL, or environment semantics from strings.

| [INDEX] | [CONCERN]              | [USE]                                            | [REPLACE]                        |
| :-----: | :--------------------- | :----------------------------------------------- | :------------------------------- |
|   [1]   | file traversal         | `Path.walk()`                                    | stringly `os.walk()` flow        |
|   [2]   | file tree transfer     | `Path.copy()` and `Path.move()`                  | `shutil` transfer wrappers       |
|   [3]   | file type cache        | `Path.info`                                      | repeated `stat()` probes         |
|   [4]   | path glob match        | `PurePath.full_match()`                          | ad hoc recursive glob predicates |
|   [5]   | symlink traversal      | `recurse_symlinks=` and `follow_symlinks=`       | manual symlink branch logic      |
|   [6]   | missing canonical path | `os.path.realpath(strict=os.path.ALLOW_MISSING)` | symlink-prefix resolution loops  |
|   [7]   | path root split        | `os.path.splitroot()`                            | drive/root string slicing        |
|   [8]   | path subclassing       | `PurePath.with_segments()`                       | path wrapper propagation         |
|   [9]   | file URI path          | `Path.from_uri()`                                | hand-parsed `file:` URLs         |
|  [10]   | file MIME type         | `mimetypes.guess_file_type()`                    | path use of `guess_type()`       |
|  [11]   | Windows reserved path  | `os.path.isreserved()`                           | reserved-name tables             |
|  [12]   | URL component presence | `missing_as_none=` and `keep_empty=`             | empty-string sentinel logic      |
|  [13]   | temporary file reopen  | `NamedTemporaryFile(delete_on_close=...)`        | `mkstemp()` unlink ladders       |
|  [14]   | tree removal errors    | `shutil.rmtree(onexc=...)`                       | `onerror` tuple handlers         |
|  [15]   | environment reload     | `os.reload_environ()`                            | manual environment cache repair  |

[STRUCTURED_PATHS]:
- Use when: filesystem, URL, or environment structure becomes program material.
- Accept: `Path` algebra for traversal, transfer, matching, and URI projection; structured component flags for URLs; lifecycle keywords for temporary files and tree removal.
- Reject: string slicing for roots and drives, hand-parsed `file:` URLs, manual symlink branches, reserved-name tables, and environment cache repair loops.
- Law: a path is a value with algebra, never a string with conventions.

## [6]-[PROJECT_SURFACE]

Static project facts stay visible in the metadata surface that declares them; removed surfaces are replacement targets, not compatibility baselines.

| [INDEX] | [CONCERN]         | [USE]                                     | [REPLACE]                      |
| :-----: | :---------------- | :---------------------------------------- | :----------------------------- |
|   [1]   | dependency groups | `[dependency-groups]`                     | requirements-file group sprawl |
|   [2]   | additive metadata | static list/table metadata in `[project]` | backend shadow metadata fields |
|   [3]   | removed batteries | maintained replacements                   | dead-battery imports           |
|   [4]   | packaging removal | non-`distutils` build surfaces            | `distutils` builds             |

[METADATA_AND_REMOVALS]:
- PEPs: 735, 808, 594, 632.
- Use when: project metadata shape changes how dependency groups or additive dynamic values are represented, or a removed standard-library or packaging surface still appears in code or build policy.
- Accept: `[dependency-groups]`, static list/table metadata in `[project]` when the backend only adds entries, and maintained replacements for removed surfaces.
- Reject: requirements-file group sprawl, backend shadow metadata fields, sidecar notes that hide dependency values, dead-battery imports, `distutils` builds, compatibility aliases, and wrapper facades around removed modules.
