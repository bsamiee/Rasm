# [PYTHON_PEP_STANDARDS]

This page is the compact PEP decision index for Python implementation work. Use it to translate a PEP-backed language capability into the expected action and the older practice it removes.

The table is an index, not a PEP manual. Keep each row atomic: name the capability family, state the implementation action, and identify the obsolete spelling, wrapper, or local pattern that no longer earns space.

## [1]-[PEP_INDEX]

| [INDEX] | [PEP] | [CATEGORY] | [ACTION] | [SUPERSEDES] |
| :-----: | :---- | :--------- | :------- | :----------- |
| [1] | PEP 747 | Type expressions | Use `TypeForm` for type-expression values | `Any`, `type[Any]`, string type names |
| [2] | PEP 749 | Annotation inspection | Use `annotationlib` | Raw `__annotations__` reads |
| [3] | PEP 649 | Annotation deferral | Use unquoted annotations | Quoted annotation strings |
| [4] | PEP 750 | Template strings | Use `t`-strings for structured interpolation | Parsing formatted strings |
| [5] | PEP 810 | Lazy imports | Declare cold imports with `lazy` | Local-import startup hacks |
| [6] | PEP 829 | Package starts | Declare startup entrypoints in `.start` | Executable `.pth` import lines |
| [7] | PEP 661 | Sentinel values | Create named sentinels with `sentinel()` | `object()` markers and magic strings |
| [8] | PEP 814 | Frozen maps | Use built-in `frozendict` for immutable mappings | Tuple-pair encodings and thin wrappers |
| [9] | PEP 686 | Text encoding | Treat default text encoding as UTF-8 | Locale-dependent implicit I/O |
| [10] | PEP 728 | TypedDict extras | Declare `closed=` or `extra_items=` explicitly | Implicit structural extra keys |
| [11] | PEP 742 | Type narrowing | Use `TypeIs` for exact narrowing | `bool` plus `cast` |
| [12] | PEP 800 | Disjoint bases | Mark nominal disjoint bases | Prose-only disjointness |
| [13] | PEP 695 | Type parameters | Declare generics inline and aliases with `type` | TypeVar farms and assignment aliases |
| [14] | PEP 696 | Type defaults | Default type parameters at the declaration | Overload-only defaults |
| [15] | PEP 705 | Read-only keys | Mark immutable dictionary fields with `ReadOnly` | Comments that keys are immutable |
| [16] | PEP 692 | Typed `**kwargs` | Use `Unpack[TypedDict]` for keyword payloads | Homogeneous `**kwargs` |
| [17] | PEP 655 | TypedDict keys | Mark per-key requiredness explicitly | Split total/non-total mirror shapes |
| [18] | PEP 589 | Typed mappings | Use `TypedDict` shapes | `dict[str, Any]` payloads |
| [19] | PEP 803 | Free-threaded ABI | Target `abi3t` for free-threaded extensions | GIL-only stable-ABI wheels |
| [20] | PEP 779 | Free-threading support | Treat free-threaded Python as supported | Experimental no-GIL caveats |
| [21] | PEP 703 | Free-threaded runtime | Synchronize shared mutation | Implicit GIL serialization |
| [22] | PEP 788 | C finalization | Attach with guarded thread-state APIs | `PyGILState_*` foreign-thread attach |
| [23] | PEP 734 | Interpreter isolation | Use `concurrent.interpreters` | Process-only isolation wrappers |
| [24] | PEP 684 | Per-interpreter GIL | Create own-GIL interpreters | Process-only CPU isolation |
| [25] | PEP 567 | Context variables | Use `ContextVar` context | Thread-local async state |
| [26] | PEP 831 | Native observability | Preserve frame-pointer build flags | Frame-pointer-stripped native builds |
| [27] | PEP 799 | Profiling namespace | Use `profiling.tracing` or `profiling.sampling` | Legacy `profile` module |
| [28] | PEP 768 | Debug attach | Attach through safe execution points | Debugger injection hooks |
| [29] | PEP 669 | Execution monitoring | Use monitoring event APIs | `settrace()` event scrapers |
| [30] | PEP 578 | Runtime auditing | Use audit hooks | Monkeypatch security probes |
| [31] | PEP 765 | Finally exits | Keep exits out of `finally` blocks | Swallowed-exception control flow |
| [32] | PEP 654 | Grouped exceptions | Handle grouped failures with `except*` | Single-error collapse |
| [33] | PEP 678 | Exception notes | Attach context with `add_note()` | Message concatenation |
| [34] | PEP 758 | Exception syntax | Omit exception parentheses without `as` | Tuple-wrapped handlers |
| [35] | PEP 706 | Tar extraction | Filter tar extraction explicitly | Trusting archive member paths |
| [36] | PEP 721 | Sdist extraction | Extract sdists with `data_filter` | Unfiltered source archive extraction |
| [37] | PEP 735 | Dependency groups | Declare `[dependency-groups]` | Requirements-file group sprawl |
| [38] | PEP 751 | Reproducible locks | Consume `pylock.toml` | Tool-specific lock exports |
| [39] | PEP 739 | Build metadata | Read `build-details.json` | Interpreter-probing scripts |
| [40] | PEP 741 | Embedding config | Configure with `PyInitConfig` | Split preinit/init structs |
| [41] | PEP 784 | Zstandard payloads | Use `compression.zstd` | Bespoke zstd adapters |
| [42] | PEP 791 | Integer math | Use `math.integer` for integer math | Float-path integer helpers |
| [43] | PEP 682 | Signed zero | Use `z` formatting for signed zero | Post-format negative-zero cleanup |
| [44] | PEP 798 | Comprehension unpacking | Unpack directly in comprehensions | Manual flattening loops |
| [45] | PEP 782 | C bytes builder | Build bytes with `PyBytesWriter` | Mutable bytes allocation APIs |
| [46] | PEP 793 | C module export | Export C modules with `PyModExport_*` | `PyInit_*` extension hooks |
| [47] | PEP 820 | C API slots | Define extensions with `PySlot` | Cast slot tables |
| [48] | PEP 757 | Integer C API | Use native integer import/export APIs | Private `PyLong` limb access |
| [49] | PEP 730 | iOS platform | Treat iOS as a supported target | Unsupported-iOS assumptions |
| [50] | PEP 738 | Android platform | Treat Android as a supported target | Unsupported-Android assumptions |
| [51] | PEP 702 | Deprecation markers | Mark deprecated APIs with `@deprecated` | Docstring-only deprecation notices |
| [52] | PEP 698 | Override markers | Mark subclass overrides with `@override` | Unmarked override contracts |
| [53] | PEP 673 | Self types | Return `Self` from subclass-preserving APIs | Bound `TypeVar` self patterns |
| [54] | PEP 675 | Literal strings | Type literal-only sinks with `LiteralString` | Untyped sensitive `str` |
| [55] | PEP 612 | Callable parameters | Preserve decorator shape with `**P` and `Concatenate` | `Callable[..., Any]` |
| [56] | PEP 646 | Variadic generics | Model variadic shape with `TypeVarTuple` | Rank-specific generic classes |
| [57] | PEP 647 | One-way guards | Use `TypeGuard` for non-subtype narrowing | `bool` plus `cast` |
| [58] | PEP 681 | Dataclass transforms | Mark generated models with `dataclass_transform` | Checker-invisible decorators |
| [59] | PEP 544 | Protocol shapes | Use `Protocol` contracts | Nominal ABC shells |
| [60] | PEP 586 | Literal values | Type value-sensitive APIs | Checker plugins for flags |
| [61] | PEP 593 | Annotation metadata | Use `Annotated` metadata | Parallel metadata maps |
| [62] | PEP 585 | Generic spelling | Use built-in collection generics | `typing.List`, `typing.Dict`, legacy aliases |
| [63] | PEP 604 | Union spelling | Use `A \| B` and `T \| None` | `Union[...]`, `Optional[...]` |
| [64] | PEP 634 | Structural matching | Use `match` for closed branch law | `if` / `elif` decision ladders |
| [65] | PEP 701 | F-string grammar | Use normal expression grammar inside f-strings | Quote and backslash workarounds |
| [66] | PEP 618 | Invariant arity | Use `zip(strict=True)` | Post-truncation asserts |
| [67] | PEP 667 | Locals semantics | Use defined locals snapshots and proxies | Mutating `locals()` snapshots |
| [68] | PEP 688 | Buffer protocol | Accept `Buffer` and `__buffer__` contracts | Bytes-only local wrappers |
| [69] | PEP 689 | Unstable C API | Use `PyUnstable_*` for unstable access | Private `_Py*` dependencies |
| [70] | PEP 697 | C object layout | Extend opaque types through extra state | Base-struct layout coupling |
| [71] | PEP 699 | Dict versioning | Treat `ma_version_tag` as internal | Private dict version access |
| [72] | PEP 687 | Module isolation | Store state on modules | Shared stdlib state |
| [73] | PEP 683 | Immortal objects | Avoid refcount-value contracts | Singleton refcount probes |
| [74] | PEP 737 | C type names | Format type names with public C APIs | Private qualified-name assembly |
| [75] | PEP 670 | C API macros | Prefer typed C API functions | Macro side-effect workarounds |
| [76] | PEP 652 | Stable ABI | Maintain the stable ABI manifest | Ad hoc ABI lists |
| [77] | PEP 489 | C module init | Use multi-phase initialization | Single-phase module globals |
| [78] | PEP 573 | C module state | Fetch defining-class module state | `PyState_FindModule` lookups |
| [79] | PEP 590 | C call protocol | Implement vectorcall | Tuple/dict call packing |
| [80] | PEP 623 | C Unicode layout | Remove `wstr` Unicode APIs | Legacy Unicode access |
| [81] | PEP 624 | C Unicode encoding | Use object-based Unicode encoders | `Py_UNICODE` encoder APIs |
| [82] | PEP 594 | Removed batteries | Replace dead standard-library modules | Dead-battery imports |
| [83] | PEP 632 | Packaging removal | Remove `distutils` | `distutils` builds |
| [84] | PEP 680 | TOML parsing | Parse TOML with `tomllib` | `tomli` and parser shims |
| [85] | PEP 597 | Text encoding warnings | Use explicit text encodings | Implicit locale I/O |
| [86] | PEP 615 | Time zones | Use `zoneinfo.ZoneInfo` | `pytz`-style adapters |
| [87] | PEP 626 | Debug line tables | Use `co_lines()` | `co_lnotab` decoding |
| [88] | PEP 657 | Traceback locations | Preserve fine-grained code positions | Line-only diagnostics |
| [89] | PEP 709 | Comprehension execution | Keep comprehensions direct | Loop rewrites for comprehension speed |
| [90] | PEP 616 | Affix removal | Use affix removers | Slice/strip affix hacks |
| [91] | PEP 584 | Dict merge | Use dict union operators | Copy/update merge ladders |
| [92] | PEP 572 | Conditional binding | Use assignment expressions | Precondition temporaries |
| [93] | PEP 570 | Positional API | Use `/` parameters | `*args` parsing |
| [94] | PEP 591 | Final contracts | Mark final names and classes | Prose-only finality |
| [95] | PEP 574 | Pickle buffers | Use protocol 5 buffers | Copy-heavy pickle blobs |
