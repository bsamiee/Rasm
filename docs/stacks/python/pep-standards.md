# [PYTHON_PEP_STANDARDS]

This page is the compact PEP decision index for Python implementation work. Use it to translate a PEP-backed language capability into the expected action and the older practice it removes.

The table is an index, not a PEP manual. Keep each row atomic: name the capability family, state the implementation action, and identify the obsolete spelling, wrapper, or local pattern that no longer earns space.

## [1]-[PEP_INDEX]

| [INDEX] | [PEP]   | [CATEGORY]               | [ACTION]                                               | [SUPERSEDES]                                  |
| :-----: | :------ | :----------------------- | :----------------------------------------------------- | :-------------------------------------------- |
|   [1]   | PEP 585 | Generic spelling         | Use built-in collection generics                       | `typing.List`, `typing.Dict`, legacy aliases |
|   [2]   | PEP 604 | Union spelling           | Use `A \| B` and `T \| None`                           | `Union[...]`, `Optional[...]`                |
|   [3]   | PEP 612 | Callable parameters      | Preserve decorator shape with `**P` and `Concatenate`  | `Callable[..., Any]`                         |
|   [4]   | PEP 634 | Structural matching      | Use `match` for closed branch law                      | `if` / `elif` decision ladders               |
|   [5]   | PEP 649 | Annotation access        | Use deferred annotations and `annotationlib` formats   | Raw `__annotations__` evaluation             |
|   [6]   | PEP 655 | Required keys            | Mark partial dictionary contracts explicitly           | Split total/non-total mirror shapes          |
|   [7]   | PEP 661 | Sentinel values          | Use named built-in sentinels                           | `object()` markers and magic strings         |
|   [8]   | PEP 673 | Self types               | Return `Self` from fluent or subclass-preserving APIs  | Bound `TypeVar` self patterns                |
|   [9]   | PEP 675 | Literal strings          | Require `LiteralString` for literal-only sinks         | Comments or runtime claims of safe text      |
|  [10]   | PEP 681 | Dataclass transforms     | Type framework-generated models explicitly             | Untyped class decorators                     |
|  [11]   | PEP 688 | Buffer protocol          | Accept buffer contracts directly                       | Bytes-only local wrappers                    |
|  [12]   | PEP 692 | Typed `**kwargs`         | Use `Unpack[TypedDict]` for keyword payloads           | `**kwargs: Any`                              |
|  [13]   | PEP 695 | Type parameters          | Declare generics inline and aliases with `type`        | TypeVar farms and assignment aliases         |
|  [14]   | PEP 696 | Type defaults            | Default type parameters at the declaration             | Overload-only defaults                       |
|  [15]   | PEP 701 | F-string grammar         | Use normal expression grammar inside f-strings         | Precomputed formatting temporaries           |
|  [16]   | PEP 705 | Read-only keys           | Mark immutable dictionary fields with `ReadOnly`       | Comments that keys are immutable             |
|  [17]   | PEP 728 | Closed and extra keys    | Choose closed or typed extra `TypedDict`               | Open `dict[str, object]` payloads            |
|  [18]   | PEP 742 | Type narrowing           | Use `TypeIs` for two-branch narrowing                  | `TypeGuard` where negative precision matters |
|  [19]   | PEP 747 | Type expressions         | Use `TypeForm` for type-expression values              | `Any`, `type[Any]`, string type names        |
|  [20]   | PEP 750 | Template strings         | Use `t`-strings for structured interpolation           | Parsing formatted strings                    |
|  [21]   | PEP 798 | Unpacking comprehensions | Flatten expression pipelines directly                  | Nested loops used only to unpack             |
|  [22]   | PEP 810 | Lazy imports             | Declare cold imports without hiding the graph          | Local import shims                           |
|  [23]   | PEP 814 | Frozen maps              | Use built-in `frozendict` for immutable mappings       | Tuple-pair encodings and thin wrappers       |
|  [24]   | PEP 829 | Package starts           | Declare package startup with `.start`                  | Import side effects and boot shims           |
