# [PYTHON_02_PEPS]

This research note checks the Python type-documentation PEPs named by the current code-documentation standard against primary sources on 2026-06-05. It is a `.reports/` input only: active standards should cite the canonical typing spec and CPython documentation for current behavior, while PEP pages supply status, feature history, and design rationale.

## [1][BOTTOM_LINE]

The Python capsule is directionally correct, but it should stop treating the PEP list as the current specification. Most typing PEP pages now carry an explicit historical-document notice and route current static semantics to the typing spec and current runtime behavior to CPython docs; `docs/standards/reference/code-documentation.md` should preserve that source split if it later absorbs this research.

[KEEP_STRONG]:
- `PEP 695`, `PEP 696`, `PEP 742`, `PEP 747`, `PEP 661`, `PEP 814`, and `PEP 702` are `Final`.
- `PEP 728` and `PEP 810` are `Accepted` for Python 3.15, so cite them with current CPython 3.15 docs and avoid implying `Final`.
- `PEP 800` is now `Final`, but direct use is mostly for stubs or standard-library and extension-module class modeling; it should not receive the same caller-doc emphasis as `TypeIs`, `TypeForm`, or `TypedDict`.

[ADJUST]:
- Add `PEP 649` and `PEP 749` to the Python capsule because `annotationlib`, deferred annotation inspection, and generated-reference annotation behavior are directly relevant to code documentation.
- Add or cross-reference `PEP 705` beside `ReadOnly`, because the current text names `ReadOnly` but does not name its PEP source.
- Treat `PEP 655` and `PEP 692` as dependency context for `TypedDict`, `Required`, `NotRequired`, and `Unpack`, not as headline new doctrine.
- Treat `PEP 727`, `PEP 724`, and `PEP 821` as noisy for this standard today: `PEP 727` and `PEP 724` are withdrawn, and `PEP 821` remains draft.

## [2][SOURCE_RULE]

Use this evidence order for future standard edits:
1. Current CPython documentation for runtime surfaces such as `typing`, `annotationlib`, `sentinel`, `frozendict`, and lazy imports.
2. Current typing spec pages for static typing semantics such as type parameter lists, defaults, `TypeIs`, `TypeForm`, `TypedDict`, `ReadOnly`, and `@disjoint_base`.
3. PEP pages for status, Python version, acceptance history, and rationale.

Evidence: CPython 3.15 `typing` says the canonical, up-to-date Python type-system specification lives at the typing specification site. Several typing PEP pages repeat that they are historical documents and point to typing-spec pages plus CPython docs for current behavior.
Last verified: 2026-06-05
Review trigger: Python 3.15 final release, typing-spec feature-page changes, or `docs/standards/reference/code-documentation.md` Python capsule edits.

## [3][NAMED_PEPS]

| [INDEX] | [PEP]                                          | [STATUS] | [PYTHON] | [DISPOSITION]                                                                                                                                               |
| :-----: | :--------------------------------------------- | :------- | :------: | :---------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [`PEP 695`](https://peps.python.org/pep-0695/) | Final    |   3.12   | Keep. Type parameter lists and `type` aliases belong to signatures; docstrings add semantic generic roles only.                                             |
|   [2]   | [`PEP 696`](https://peps.python.org/pep-0696/) | Final    |   3.13   | Keep. Defaults belong to type parameters and runtime typing attributes; comments add caller-visible default semantics only when the declaration omits them. |
|   [3]   | [`PEP 742`](https://peps.python.org/pep-0742/) | Final    |   3.13   | Keep. `TypeIs` matters when a public predicate's narrowing contract changes caller behavior in both branches.                                               |
|   [4]   | [`PEP 728`](https://peps.python.org/pep-0728/) | Accepted |   3.15   | Keep with status precision. `closed` and `extra_items` carry payload shape; comments add domain meaning only.                                               |
|   [5]   | [`PEP 747`](https://peps.python.org/pep-0747/) | Final    |   3.15   | Keep and update any provisional language. `TypeForm` is relevant for public APIs that accept or return type expressions as values.                          |
|   [6]   | [`PEP 800`](https://peps.python.org/pep-0800/) | Final    |   3.15   | Keep but demote. `@disjoint_base` mainly documents nominal impossibility for type checkers, especially in stubs, not ordinary caller docs.                  |
|   [7]   | [`PEP 810`](https://peps.python.org/pep-0810/) | Accepted |   3.15   | Keep outside type-shape doctrine. Lazy imports affect import-error timing, side effects, and resource/lifecycle comments.                                   |
|   [8]   | [`PEP 661`](https://peps.python.org/pep-0661/) | Final    |   3.15   | Keep. `sentinel` carries public absence, identity, pickling, and type-narrowing semantics.                                                                  |
|   [9]   | [`PEP 814`](https://peps.python.org/pep-0814/) | Final    |   3.15   | Keep narrowly. `frozendict` affects immutable mapping, hashability, snapshot, and default-value contracts.                                                  |
|  [10]   | [`PEP 702`](https://peps.python.org/pep-0702/) | Final    |   3.13   | Keep only under lifecycle. `@warnings.deprecated` is generator-visible deprecation, not a general source-comment replacement.                               |

Evidence: named PEP headers on `peps.python.org`; CPython 3.15 `typing` docs for type parameters, `TypedDict`, `ReadOnly`, `TypeIs`, `TypeForm`, and `@disjoint_base`; CPython 3.15 language and builtins docs for lazy imports and `frozendict`; CPython 3.14 `annotationlib` docs for deferred annotations.
Last verified: 2026-06-05
Review trigger: any PEP status change, Python 3.15 documentation update, or typing-spec feature update.

## [4][MISSING_PEPS]

`PEP 649` and `PEP 749`
    Status: Final, Python 3.14.
    Usefulness: high. They explain deferred annotation evaluation, `annotationlib`, annotation formats, and generated-reference annotation inspection.
    Standard disposition: add beside the existing `annotationlib` sentence. The current capsule already says `annotationlib` governs generated-reference annotation inspection; these PEPs are the missing status/rationale anchors.

`PEP 705`
    Status: Final, Python 3.13.
    Usefulness: high. It introduces `ReadOnly` for `TypedDict` items, which the current capsule already names.
    Standard disposition: add as a dependency anchor for `ReadOnly`, especially near `PEP 728` because `extra_items` interacts with read-only items.

`PEP 692`
    Status: Final, Python 3.12.
    Usefulness: medium. It governs `Unpack[TypedDict]` for `**kwargs`, which the current capsule names through `Unpack`.
    Standard disposition: mention only if the Python capsule keeps a dependency-list sentence for `TypedDict` payload shape.

`PEP 655`
    Status: Final, Python 3.11.
    Usefulness: medium. It governs `Required` and `NotRequired`, both named in the current capsule.
    Standard disposition: use as supporting context rather than headline current doctrine.

`PEP 593`
    Status: Final, Python 3.9.
    Usefulness: medium. `Annotated` owns machine-readable metadata for schema and runtime consumers, and this helps explain why docstrings should not duplicate metadata that Pydantic, msgspec, or beartype already consumes.
    Standard disposition: add only if the capsule needs a concise `Annotated` source-truth sentence; avoid reopening `PEP 727`'s withdrawn `typing.Doc` direction.

## [5][NOISY_PEPS]

`PEP 727`
    Status: Withdrawn.
    Why noisy: it proposed `typing.Doc` in `Annotated` metadata, but the withdrawal notes negative reception around verbosity and readability.
    Standard disposition: do not cite as active doctrine. It is useful only as rationale for rejecting documentation-in-typing-metadata churn.

`PEP 724`
    Status: Withdrawn.
    Why noisy: `PEP 742` replaces the practical need with `TypeIs`.
    Standard disposition: do not mention unless explaining why `TypeGuard` lore should not drive current narrowing documentation.

`PEP 821`
    Status: Draft, Python 3.15 target.
    Why noisy: it may become relevant to `Callable[[Unpack[TypedDict]], R]`, but it is not accepted.
    Standard disposition: watch only. Do not add to active standards unless it is accepted or CPython and the typing spec publish current support.

`PEP 729`
    Status: Active process PEP.
    Why noisy: it explains typing governance and why PEPs are not living specifications, but it is not a type-documentation feature.
    Standard disposition: use as source-routing rationale only if the standards need to justify citing typing-spec pages over PEP bodies.

## [6][FUTURE_DELTA]

If the active Python capsule is updated later, the safest rewrite is a small source-truth correction rather than a larger PEP catalog. Replace the existing PEP-heavy sentence with a rule shaped like this:

```markdown conceptual
[TYPE_TRUTH]:
- CPython docs and the typing spec govern current annotation, typing, and runtime behavior; PEPs identify feature status and rationale only.
- `annotationlib`, `PEP 649`, and `PEP 749` govern deferred annotation inspection for generated references.
- `PEP 695` and `PEP 696` place type parameters and defaults in declarations; docstrings add semantic generic relationships only.
- `PEP 742` and `PEP 747` document public narrowing and type-expression value contracts only when callers depend on them.
- `PEP 728`, `PEP 705`, `PEP 655`, and `PEP 692` let `TypedDict`, `ReadOnly`, `Required`, `NotRequired`, and `Unpack` carry payload shape; comments add domain semantics only.
- `PEP 800` `@disjoint_base` belongs mostly to stubs or type-checker-facing nominal-disjointness surfaces.
- `PEP 810`, `PEP 661`, and `PEP 814` document lazy import timing, sentinel identity, and immutable mapping contracts only when caller-visible.
- `PEP 702` lifecycle markers belong to external support contracts only.
```

Do not add the conceptual block directly without reconciling it against the full `docs/standards/reference/code-documentation.md` capsule and the shared standards. The active document should stay a code-documentation standard, not a Python PEP index.

## [7][SOURCES]

- [`PEP 695`](https://peps.python.org/pep-0695/), [`PEP 696`](https://peps.python.org/pep-0696/), [`PEP 742`](https://peps.python.org/pep-0742/), [`PEP 728`](https://peps.python.org/pep-0728/), [`PEP 747`](https://peps.python.org/pep-0747/), [`PEP 800`](https://peps.python.org/pep-0800/), [`PEP 810`](https://peps.python.org/pep-0810/), [`PEP 661`](https://peps.python.org/pep-0661/), [`PEP 814`](https://peps.python.org/pep-0814/), and [`PEP 702`](https://peps.python.org/pep-0702/).
- [`PEP 649`](https://peps.python.org/pep-0649/), [`PEP 749`](https://peps.python.org/pep-0749/), [`PEP 705`](https://peps.python.org/pep-0705/), [`PEP 692`](https://peps.python.org/pep-0692/), [`PEP 655`](https://peps.python.org/pep-0655/), [`PEP 593`](https://peps.python.org/pep-0593/), [`PEP 727`](https://peps.python.org/pep-0727/), [`PEP 724`](https://peps.python.org/pep-0724/), [`PEP 821`](https://peps.python.org/pep-0821/), and [`PEP 729`](https://peps.python.org/pep-0729/).
- [CPython 3.15 `typing` docs](https://docs.python.org/3.15/library/typing.html), [CPython 3.14 `annotationlib` docs](https://docs.python.org/3.14/library/annotationlib.html), [CPython 3.15 lazy import statement docs](https://docs.python.org/3.15/reference/simple_stmts.html#the-lazy-import-statement), and [CPython 3.15 builtins docs](https://docs.python.org/3.15/library/functions.html).

## [8][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, the four shared standards, and `docs/standards/reference/code-documentation.md`.
- [x] Used current primary sources for PEP status and canonical runtime or typing behavior.
- [x] Edited only `.reports/code-documentation-050626/track-python/02-python-peps.md`.
- [x] Run `git diff --check -- .reports/code-documentation-050626/track-python/02-python-peps.md`.
