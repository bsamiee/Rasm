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
- Import baseline: module-scope named imports, with explicit lazy imports for cold dependencies
- Export baseline: explicit end-of-file `__all__`; no wildcard imports, barrel files, facade exports, or empty `__init__.py` package markers
- Annotation baseline: deferred annotations inspected through annotation APIs

Treat source files as modern Python, not compatibility layers. Remove old imports, shims, typing spellings, package markers, and tool bypasses when the active surface carries the concept directly.

## [2]-[CANONICAL_CHOOSER]

Use the active Python surface directly. Replace older spellings and local machinery when syntax, typing, or the standard library owns the behavior.

| [INDEX] | [CONCERN]                  | [USE]                                                          | [REPLACE]                           |
| :-----: | :------------------------- | :------------------------------------------------------------- | :---------------------------------- |
|   [1]   | runtime annotations        | `annotationlib.get_annotations()`                              | direct `__annotations__` reads      |
|   [2]   | callable shape             | parameter-preserving decorators                                | `Callable[..., T]` erasure          |
|   [3]   | generic shape              | inline type parameters and `type` aliases                      | `TypeVar`, `ParamSpec`, `TypeAlias` |
|   [4]   | type expressions           | `TypeForm`                                                     | `type[T]` or `object` forms         |
|   [5]   | type predicates            | `TypeIs`                                                       | one-way `TypeGuard` predicates      |
|   [6]   | signature annotations      | `inspect.signature(annotation_format=...)`                     | annotation string post-processing   |
|   [7]   | method override            | `@typing.override`                                             | unmarked subclass overrides         |
|   [8]   | self type                  | `typing.Self`                                                  | bound `TypeVar` self boilerplate    |
|   [9]   | generic defaults           | type parameter defaults and `NoDefault`                        | overload families for defaults      |
|  [10]   | typed disjointness         | `@typing.disjoint_base`                                        | prose-only disjointness             |
|  [11]   | variadic generic options   | `TypeVarTuple` variance args                                   | asymmetric variadic-generic shims   |
|  [12]   | kwargs payload             | `Unpack[TypedDict]`                                            | homogeneous `**kwargs`              |
|  [13]   | typed dict closure         | `closed=` and `extra_items=`                                   | open payload prose                  |
|  [14]   | required keys              | `Required[]` and `NotRequired[]`                               | split `TypedDict` inheritance       |
|  [15]   | immutable keys             | `ReadOnly[T]` in `TypedDict`                                   | prose-only immutable key contracts  |
|  [16]   | literal text boundary      | `LiteralString`                                                | untyped sensitive `str`             |
|  [17]   | safe templates             | t-strings and processors                                       | f-string pre-parsing                |
|  [18]   | template AST               | `ast.TemplateStr`                                              | f-string AST rewrites               |
|  [19]   | closed dispatch            | `match` with pattern narrowing                                 | tag `if` chains                     |
|  [20]   | exhaustiveness proof       | `Never` and `assert_never()`                                   | default-arm raises                  |
|  [21]   | sentinel value             | `sentinel()`                                                   | `object()` or string sentinels      |
|  [22]   | string enum                | `enum.StrEnum`                                                 | `str, Enum` mixins                  |
|  [23]   | enum invariant             | `enum.verify()` and `EnumCheck`                                | local enum validation loops         |
|  [24]   | immutable update           | `copy.replace()` or `__replace__()`                            | mutate-then-freeze copies           |
|  [25]   | immutable map              | `frozendict`                                                   | tuple-pair pseudo-maps              |
|  [26]   | invariant arity            | `zip(strict=True)`                                             | post-truncation asserts             |
|  [27]   | mapped arity               | `map(strict=True)`                                             | post-truncation asserts             |
|  [28]   | cold import                | module-scope `lazy import` or `lazy from`                      | local-import startup hacks          |
|  [29]   | startup hook               | `.start` entries                                               | executable `.pth` import lines      |
|  [30]   | UTF-8 default              | UTF-8 default; `encoding="locale"`                             | locale-dependent implicit text I/O  |
|  [31]   | static reflection          | `inspect.getmembers_static()`                                  | descriptor-triggering scans         |
|  [32]   | union introspection        | `typing.get_origin()` and `get_args()`                         | private union implementation checks |
|  [33]   | protocol introspection     | `typing.get_protocol_members()` and `typing.is_protocol()`     | private protocol probes             |
|  [34]   | generic bases              | `types.get_original_bases()`                                   | direct `__orig_bases__` reads       |
|  [35]   | execution locals           | explicit `locals=` and `frame.f_locals`                        | mutating `locals()` snapshots       |
|  [36]   | frame locals type          | `types.FrameLocalsProxyType`                                   | private frame-locals proxy checks   |
|  [37]   | I/O protocol               | `io.Reader` and `io.Writer`                                    | `typing.IO` pseudo-protocols        |
|  [38]   | buffer protocol            | `collections.abc.Buffer`                                       | `ByteString` or bytes prose         |
|  [39]   | buffer flags               | `inspect.BufferFlags`                                          | magic integer buffer flags          |
|  [40]   | generic slice              | `slice[T]`                                                     | unparameterized slice contracts     |
|  [41]   | f-string expression        | full-expression f-strings                                      | quote juggling temporaries          |
|  [42]   | AST field schema           | `ast.AST._field_types`                                         | hand-maintained ASDL maps           |
|  [43]   | AST equality               | `ast.compare()`                                                | `ast.dump()` string comparison      |
|  [44]   | optimized AST              | `ast.parse(optimize=...)`                                      | local AST pruning                   |
|  [45]   | source module name         | `module=` compile APIs                                         | filename-only warning filters       |
|  [46]   | warnings filter            | `/regex/` warning filters                                      | literal-only warning fields         |
|  [47]   | resource materialization   | `importlib.resources.as_file()`                                | `__file__` extraction loops         |
|  [48]   | TOML parse                 | `tomllib` TOML 1.1.0                                           | `tomli` or parser shims             |
|  [49]   | file traversal             | `Path.walk()`                                                  | stringly `os.walk()` flow           |
|  [50]   | file tree transfer         | `Path.copy()` and `Path.move()`                                | `shutil` transfer wrappers          |
|  [51]   | file type cache            | `Path.info`                                                    | repeated `stat()` probes            |
|  [52]   | path glob match            | `PurePath.full_match()`                                        | ad hoc recursive glob predicates    |
|  [53]   | symlink traversal          | `recurse_symlinks=` and `follow_symlinks=`                     | manual symlink branch logic         |
|  [54]   | missing canonical path     | `os.path.realpath(strict=os.path.ALLOW_MISSING)`               | symlink-prefix resolution loops     |
|  [55]   | path root split            | `os.path.splitroot()`                                          | drive/root string slicing           |
|  [56]   | path subclassing           | `PurePath.with_segments()`                                     | path wrapper propagation            |
|  [57]   | file URI path              | `Path.from_uri()`                                              | hand-parsed `file:` URLs            |
|  [58]   | file MIME type             | `mimetypes.guess_file_type()`                                  | path use of `guess_type()`          |
|  [59]   | URL component presence     | `missing_as_none=` and `keep_empty=`                           | empty-string sentinel logic         |
|  [60]   | temporary file reopen      | `NamedTemporaryFile(delete_on_close=...)`                      | `mkstemp()` unlink ladders          |
|  [61]   | tree removal errors        | `shutil.rmtree(onexc=...)`                                     | `onerror` tuple handlers            |
|  [62]   | Windows reserved path      | `os.path.isreserved()`                                         | reserved-name tables                |
|  [63]   | queue lifecycle            | `queue.Queue.shutdown()`                                       | sentinel queue items                |
|  [64]   | async queue lifecycle      | `asyncio.Queue.shutdown()`                                     | sentinel async-queue items          |
|  [65]   | async task group           | `asyncio.TaskGroup`                                            | `gather()` task sets                |
|  [66]   | task-group stop            | `asyncio.TaskGroup.cancel()`                                   | raiser-task sentinels               |
|  [67]   | async deadline             | `asyncio.timeout()` or `asyncio.timeout_at()`                  | `wait_for()` wrapper ladders        |
|  [68]   | completed-task correlation | `async for` over `asyncio.as_completed()`                      | task-result side maps               |
|  [69]   | event-loop lifetime        | `asyncio.Runner`                                               | manual loop lifecycle stacks        |
|  [70]   | context variable scope     | `ContextVar.set()` token context manager                       | `reset(token)` `finally` blocks     |
|  [71]   | async stream delimiter     | `asyncio.StreamReader.readuntil((...))`                        | manual separator scans              |
|  [72]   | server client shutdown     | `close_clients()` and `abort_clients()`                        | transport registries                |
|  [73]   | async eager execution      | `asyncio.eager_task_factory()`                                 | cache-hit scheduling wrappers       |
|  [74]   | async task CLI inspection  | `python -m asyncio ps` and `pstree`                            | private task graph scraping         |
|  [75]   | async call graph           | `asyncio.capture_call_graph()` or `asyncio.print_call_graph()` | private task graph scraping         |
|  [76]   | bounded map                | `Executor.map(buffersize=...)`                                 | submission throttling wrappers      |
|  [77]   | worker sizing              | `os.process_cpu_count()`                                       | `os.cpu_count()` worker counts      |
|  [78]   | interpreter isolation      | `concurrent.interpreters`                                      | process-only isolation wrappers     |
|  [79]   | subinterpreter pool        | `concurrent.futures.InterpreterPoolExecutor`                   | process-only CPU pools              |
|  [80]   | process-pool stop          | `ProcessPoolExecutor.terminate_workers()` or `.kill_workers()` | private worker traversal            |
|  [81]   | process interrupt          | `multiprocessing.Process.interrupt()`                          | cleanup-hostile `terminate()`       |
|  [82]   | iterator sharing           | `threading.serialize_iterator()` or `.concurrent_tee()`        | generator lock wrappers             |
|  [83]   | execution monitoring       | `sys.monitoring`                                               | `settrace()` event scrapers         |
|  [84]   | sampling profiler          | `profiling.sampling`                                           | handwritten timers or `profile`     |
|  [85]   | native profiling           | default frame pointers                                         | opaque native-extension stacks      |
|  [86]   | C-stack dump               | `faulthandler.dump_c_stack()`                                  | external native stack probes        |
|  [87]   | live debug attach          | `sys.remote_exec()`                                            | debugger injection hooks            |
|  [88]   | ABI reflection             | `sys.abi_info`                                                 | parsed SOABI strings                |
|  [89]   | JSON arrays                | `json.load(array_hook=...)` or `json.loads(array_hook=...)`    | post-decode list walks              |
|  [90]   | Zstandard payload          | `compression.zstd`                                             | subprocess or bespoke zstd adapters |
|  [91]   | Z85 payload                | `base64.z85encode()` and `z85decode()`                         | local Z85 codecs                    |
|  [92]   | base-N canonical           | `canonical=True` decoders                                      | padding-bit postchecks              |
|  [93]   | base-N format control      | `padded=`, `wrapcol=`, `ignorechars=`                          | pre/post encode formatting          |
|  [94]   | file digest                | `hashlib.file_digest()`                                        | manual chunked hash loops           |
|  [95]   | checksum combine           | `zlib.adler32_combine()` and `zlib.crc32_combine()`            | recompress-to-checksum loops        |
|  [96]   | ordered identifiers        | `uuid.uuid7()`                                                 | timestamp-prefixed UUID wrappers    |
|  [97]   | UUID boundaries            | `uuid.NIL` and `uuid.MAX`                                      | magic UUID boundary literals        |
|  [98]   | regex backtracking         | atomic groups and possessive quantifiers                       | scanner split workarounds           |
|  [99]   | regex prefix               | `re.prefixmatch()`                                             | ambiguous `re.match()`              |
|  [100]  | regex parse error          | `re.PatternError`                                              | generic `re.error` catches          |
|  [101]  | iterable batching          | `itertools.batched()`                                          | local chunk helper loops            |
|  [102]  | max heap                   | max-heap `heapq` APIs                                          | negated-priority heap wrappers      |
|  [103]  | dot product                | `math.sumprod()`                                               | `zip()` product folds               |
|  [104]  | partial holes              | `functools.Placeholder`                                        | lambda wrappers for partial gaps    |
|  [105]  | none predicate             | `operator.is_none()`                                           | one-off `lambda x: x is None`       |
|  [106]  | flattening                 | unpacking comprehensions                                       | nested flattening loops             |
|  [107]  | conditional binding        | assignment expressions (`:=`)                                  | precondition temporary variables    |
|  [108]  | multiset XOR               | `Counter` XOR                                                  | manual count-difference folds       |
|  [109]  | multi-exception            | unparenthesized `except` and `except*`                         | tuple wrapper noise without `as`    |
|  [110]  | active exception           | `sys.exception()`                                              | `sys.exc_info()[1]`                 |
|  [111]  | exception context          | `BaseException.add_note()`                                     | message concatenation               |
|  [112]  | deprecation marker         | `@warnings.deprecated()`                                       | docstring-only deprecation notices  |
|  [113]  | target date parsing        | `datetime.date.strptime()` and `datetime.time.strptime()`      | `datetime.strptime()` slicing       |
|  [114]  | environment reload         | `os.reload_environ()`                                          | manual environment cache repair     |
|  [115]  | fd buffer read             | `os.readinto()`                                                | `os.read()` copy slices             |
|  [116]  | byte buffer drain          | `bytearray.take_bytes()`                                       | `bytes(buffer)` plus `clear()`      |
|  [117]  | FFI memory view            | `ctypes.memoryview_at()`                                       | `string_at()` copy scaffolds        |
|  [118]  | binary numeric views       | `array` and `memoryview` complex codes                         | struct-packed numeric buffers       |
|  [119]  | integer math               | `math.integer`                                                 | float math for integers             |
|  [120]  | fused multiply-add         | `math.fma()`                                                   | rounded multiply-add                |
|  [121]  | float extrema              | `math.fmax()` and `math.fmin()`                                | NaN-aware min/max wrappers          |
|  [122]  | float classification       | `math.isnormal()`, `math.issubnormal()`, `math.signbit()`      | bit-level float probes              |
|  [123]  | fraction conversion        | `Fraction.from_number()`                                       | constructor branch ladders          |
|  [124]  | statistical density        | `statistics.kde()`                                             | local kernel-density estimators     |
|  [125]  | real timeout value         | real-number timeout arguments                                  | int/float-only gates                |
|  [126]  | Unicode identifier         | `unicodedata.isxidstart()` and `unicodedata.isxidcontinue()`   | regex identifier tables             |
|  [127]  | grapheme iteration         | `unicodedata.iter_graphemes()`                                 | codepoint loops                     |
|  [128]  | Unicode block              | `unicodedata.block()`                                          | block range tables                  |
|  [129]  | XML text validity          | `xml.is_valid_name()` and `xml.is_valid_text()`                | XML character tables                |

## [3]-[LANGUAGE_FORM_CONTRACTS]

Use these contracts when the chooser names the primitive but code still needs a placement rule.

[DECLARATION_SITE]:
- Use when: the owner declaration can state a type, callable, keyword, or annotation shape directly.
- Accept: inline type parameters, `type` aliases, exact callable forwarding, `TypeIs`, `TypeForm`, `Unpack[TypedDict]`, `ReadOnly`, `io.Reader`, and `io.Writer`.
- Reject: remote alias repair, erased callables, downstream casts, broad `object` recovery, and prose keyword contracts.
- Boundary: object-family policy, payload policy, and decorator architecture belong to the owning concept page.

[TYPE_PREDICATE_SITE]:
- Use when: a reusable predicate proves exact type membership that inline narrowing cannot express.
- Accept: `TypeIs[T]` over the concrete target or owned structural target, where `T` is compatible with the input type and the predicate is true exactly for `T`.
- Reject: subset predicates, incompatible target narrowing, `TypeGuard` for subtype-compatible checks, bool helpers followed by `cast`, and runtime-checkable protocols created only to satisfy `isinstance`.
- Soundness: `TypeIs[T]` is a biconditional type-membership proof, not a validation rail for valid, non-empty, active, normalized, positive, or otherwise filtered `T` values.
- Consumption: keep predicate use at ingress, dispatch, or projection boundaries; fold the narrowed value through one expression or the owning result rail instead of spreading branch bodies.
- Boundary: untyped package gaps enter as `object` at the boundary; typed object-shape ownership, validation, and provider adoption belong to the owning concept page.

```python conceptual
from typing import assert_type, final, TypeIs


class Shape: ...


class RefinedShape(Shape): ...


@final
class OtherShape: ...


def is_shape(value: object) -> TypeIs[Shape]:
    return isinstance(value, Shape)


def projected(value: RefinedShape | OtherShape) -> RefinedShape | OtherShape:
    return assert_type(value, RefinedShape) if is_shape(value) else assert_type(value, OtherShape)
```

[INSPECTION_SITE]:
- Use when: runtime code consumes annotations, signatures, unions, or type forms.
- Accept: `annotationlib`, `inspect.signature(annotation_format=...)`, `typing.get_origin()`, and `typing.get_args()` at the consuming boundary.
- Reject: raw annotation dictionaries, string surgery, annotation `eval`, and private union implementation checks.
- Boundary: external text, dynamic execution, annotation cost, and import-time behavior belong to the owning concept page.

[MODULE_ENTRY_SITE]:
- Use when: import, startup, or template structure must stay visible before execution.
- Accept: module-scope lazy imports, `.start` entries, t-string processors, and template AST nodes.
- Reject: function-local import hiding, executable `.pth` lines, rendered-string reparsing, and scattered `importlib` laziness.
- Boundary: runtime startup policy belongs to the runtime owner; package graph and tool graph truth belong to the platform owner.

[MODULE_BOUNDARY_SITE]:
- Use when: a module declares its public names or imports another module surface.
- Accept: named imports, end-of-file `__all__`, and `__init__.py` only when it owns real package initialization or a public package contract.
- Reject: wildcard imports, barrel files, facade-only exports, empty `__init__.py` package markers, and re-export files that only hide the real owner.
- Boundary: package topology and generated API documentation belong to their owning platform, architecture, or code-documentation surface.

[TOOL_SURFACE]:
- Use when: executing Python code, resolving dependencies, locking the environment, or invoking Python tools.
- Accept: `uv`-managed entrypoints through the repository tool graph.
- Reject: ambient `python`, `pip`, `venv`, direct tool shims, and PATH-dependent execution that bypasses `uv`.
- Boundary: command catalogs, package versions, tool groups, and cache paths belong to tool, manifest, or platform owners.

[EXPRESSION_SITE]:
- Use when: the invariant is local to one expression or statement.
- Accept: `match`, assignment expressions, unpacking comprehensions, `copy.replace()`, `zip(strict=True)`, and direct multi-exception syntax.
- Reject: precondition temporaries, accumulator loops, after-the-fact asserts, one-use helpers, and mutate-then-freeze copies.
- Boundary: dispatch architecture, error transport, and recovery policy belong to the owning concept page.

[STANDARD_PRIMITIVE_SITE]:
- Use when: a standard-library primitive exactly owns the operation shape.
- Accept: the named primitive for value, path, compression, heap, UUID, regex, queue, interpreter, or profiling concerns.
- Reject: local wrappers, magic literals, pseudo-protocols, subprocess adapters, sentinel payloads, and private probes.
- Boundary: data-structure strategy, runtime scheduling, and proof policy belong to algorithm, runtime, or testing owners.

[SENTINEL_DEFAULT_SITE]:
- Use when: omission or inherited selection must be distinct from every valid domain value, including `None`.
- Accept: module-global `NAME = sentinel("NAME")` values whose variable name matches the sentinel name, reused by `is`, and carried directly in the union type.
- Reject: `object()` defaults, `None` defaults when `None` is domain-valid, string tokens, omitted-vs-supplied overloads with no return-shape change, and bool flags that split one option shape.
- Boundary: queue or task lifecycle, wire payload tags, and protocol tokens use the owning lifecycle API or explicit domain value instead of sentinel payloads.

```python conceptual
from builtins import sentinel
from enum import StrEnum


INHERIT = sentinel("INHERIT")


class Variant(StrEnum):
    PRIMARY = "<variant-a>"
    SECONDARY = "<variant-b>"


type VariantSelection = Variant | INHERIT


def selected(base: Variant, selection: VariantSelection = INHERIT) -> Variant:
    match selection:
        case Variant() as variant:
            return variant
        case _ if selection is INHERIT:
            return base
```

[FROZENDICT_POLICY_TABLE]:
- Use when: immutable mapping rows or nested policy tables need mapping semantics, language-level immutability, and order-insensitive equality or hash identity.
- Accept: `frozendict[K, V]` rows with hashable keys; values must also be hashable when the row itself is hashed or used as an outer key.
- Reject: tuple-pair pseudo-maps, sorted-item key normalizers, module-level dictionaries used as immutable policy tables, `MappingProxyType` views over mutable storage, frozen shells around dictionaries, and mutate-then-freeze copy ladders.
- Boundary: `frozendict` preserves insertion order for iteration, but order is not equality or hash identity; use tuple pairs or an owning value object when order is semantic. `frozendict` is shallowly immutable; nested values must be immutable or owned by a model.

```python conceptual
from builtins import frozendict
from enum import StrEnum


class Field(StrEnum):
    KEY = "<field-a>"
    VALUE = "<field-b>"


type Row = frozendict[Field, str]


ROW_A: Row = frozendict({Field.KEY: "<key-a>", Field.VALUE: "<value-a>"})
ROW_B: Row = frozendict({Field.KEY: "<key-b>", Field.VALUE: "<value-b>"})
TABLE: frozendict[Row, str] = frozendict({ROW_A: "<result-a>", ROW_B: "<result-b>"})

SELECTED_RESULT: str = TABLE[ROW_A]
```

## [4]-[ABSTRACTION_COLLAPSE_TESTS]

Use these tests before keeping a local abstraction beside a language primitive.

[TYPE_REPAIR]:
- Smell: a cast, alias, helper, or erased callable exists only to recover evidence later.
- Collapse: move evidence into the owner declaration.
- Done when: callers no longer need repair code to recover the shape.

[PREDICATE_PROTOCOL]:
- Smell: a `Protocol` mirrors an imported object only to make `isinstance`, `cast`, or a bool helper acceptable to the type checker.
- Collapse: replace the protocol-as-predicate with a `TypeIs` predicate over the real target, or admit the object through a typed local owner when no real target type exists.
- Done when: both predicate branches narrow without wildcard protocols, `cast`, or post-check repair code.

[STRING_RECOVERY]:
- Smell: code parses rendered strings, paths, URLs, annotations, regex intent, or template fields by hand.
- Collapse: ask the language API for the structured form.
- Done when: the implementation consumes typed components instead of reconstructed text.

[CEREMONY_WRAPPER]:
- Smell: a helper only names one modern expression, iterator, update, predicate, or primitive operation.
- Collapse: inline the language form at the use site.
- Done when: the invariant is visible where it executes.

[SURFACE_FACADE]:
- Smell: a file exists only to re-export another module, mark a package, or collect names from several owners.
- Collapse: import the real owner by name and publish the module's own `__all__` only where the module owns the public surface.
- Done when: readers can find the implementation owner without traversing wildcard imports, barrels, or empty package files.

[MAGIC_VALUE]:
- Smell: absence, boundary, ordering, or identifier limits are represented by strings, objects, numeric sentinels, or literal UUIDs.
- Collapse: use the named sentinel, boundary value, or ordered identifier primitive.
- Done when: the value carries its own semantics without prose.

[PROOF_PROXY]:
- Smell: timing, async state, native stacks, or debug attach behavior is inferred through local probes.
- Collapse: use the built-in profiling, inspection, frame-pointer, or debug primitive.
- Done when: the proof hook is a language-owned surface and proof policy routes away.

## [5]-[REJECTIONS]

- No version-by-version feature lists, release-note summaries, copied provider manuals, source-link collections, or freshness caveats.
- No table cells with prose rationale, examples, links, source notes, caveats, or multiple unrelated primitives.
- No restating chooser rows as loose bullets below the table.
- No `typing.Optional`, `typing.Union`, `typing.TypeAlias`, module-level type-parameter boilerplate, `cast`, or `Any` leakage in durable examples.
- No ambient Python execution bypassing `uv`, wildcard imports, barrel files, facade exports, empty package-marker `__init__.py` files, direct `__annotations__` inspection, annotation-string eval, locals-mutation policy, helper modules, package facades, provider-branded surfaces, or external-library-specific docs topology.
- No runtime, validation, observability, testing, package graph, or algorithm policy in `language.md` when another concept page owns the decision.
