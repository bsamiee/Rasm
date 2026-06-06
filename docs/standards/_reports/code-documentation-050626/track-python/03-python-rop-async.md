# [PYTHON_03_ROP_ASYNC_RESEARCH]

This is research input for `docs/standards/reference/code-documentation.md`; it is not an active standard. The useful change is to keep Python public docstrings focused on caller-visible rail, async, cancellation, generator, and native-exception boundaries that annotations cannot express, while preserving Rasm's current repo truth that Python tooling targets 3.14 and approved rail APIs are `expression.Result` and `expression.Option`.

## [1][SCOPE]

Read scope:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/information-structure.md`
- `docs/standards/proof.md`
- `pyproject.toml`

Local truth:
- `pyproject.toml` requires Python `>=3.14`.
- `tool.ty.environment.python-version`, `tool.mypy.python_version`, and `tool.ruff.target-version` are configured for Python 3.14 / `py314`.
- `pyproject.toml` approves `expression>=5.6.0` and `anyio>=4.13.0`.
- `pyproject.toml` bans `returns`, `returns.maybe`, and `returns.result` with the local message to use `expression.Result` / `expression.Option`.
- `docs/standards/reference/code-documentation.md` is already modified in the worktree; this report leaves it untouched.

## [2][SOURCE_BASE]

Current primary sources used for this report:

| [INDEX] | [SOURCE]                                                                                                           | [CURRENT_SIGNAL]                                         | [USE]                                                                                                                |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [Python 3.14 typing docs](https://docs.python.org/3.14/library/typing.html)                                        | Python 3.14.5 docs, accessed 2026-06-05                  | Current Rasm baseline for `TypeIs`, deferred annotation inspection, `TypedDict`, `ReadOnly`, and `Optional` meaning. |
|   [2]   | [Python 3.14 expression reference](https://docs.python.org/3.14/reference/expressions.html#yield-expressions)      | Python 3.14.5 docs, accessed 2026-06-05                  | Generator and async-generator yield, finalization, `aclose()`, and `StopAsyncIteration` behavior.                    |
|   [3]   | [Python 3.14 exceptions docs](https://docs.python.org/3.14/library/exceptions.html#exception-groups)               | Python 3.14.5 docs, accessed 2026-06-05                  | `ExceptionGroup`, `BaseExceptionGroup`, and `except*` boundary behavior.                                             |
|   [4]   | [Python 3.14 annotationlib docs](https://docs.python.org/3.14/library/annotationlib.html)                          | Python 3.14.5 docs, accessed 2026-06-05                  | Deferred annotation introspection hazards for generated references.                                                  |
|   [5]   | [Python 3.15 typing docs](https://docs.python.org/3.15/library/typing.html)                                        | Python 3.15.0b2 docs, accessed 2026-06-05                | Future-facing source for `TypeForm` and `@disjoint_base` already named by the active standard.                       |
|   [6]   | [Python 3.15 builtins docs](https://docs.python.org/3.15/library/functions.html#frozendict)                        | Python 3.15.0b2 docs, accessed 2026-06-05                | Future-facing source for `frozendict` already named by the active standard.                                          |
|   [7]   | [AnyIO 4.13 cancellation docs](https://anyio.readthedocs.io/en/stable/cancellation.html)                           | AnyIO 4.13.0 docs, accessed 2026-06-05                   | Cancel scopes, level cancellation, timeouts, shielding, finalization, re-raise, and LIFO risk.                       |
|   [8]   | [AnyIO 4.13 task docs](https://anyio.readthedocs.io/en/stable/tasks.html#handling-multiple-errors-in-a-task-group) | AnyIO 4.13.0 docs, accessed 2026-06-05                   | Task-group `ExceptionGroup` behavior.                                                                                |
|   [9]   | [Expression Result reference](https://expression.readthedocs.io/en/stable/reference/result.html)                   | Crawled 2026-05-30 by search, accessed 2026-06-05        | Rasm-approved `Result` / ROP source.                                                                                 |
|  [10]   | [Expression Option reference](https://expression.readthedocs.io/en/stable/reference/option.html)                   | Accessed 2026-06-05                                      | Rasm-approved `Option` source.                                                                                       |
|  [11]   | [PEP 257](https://peps.python.org/pep-0257/)                                                                       | Active informational PEP, last modified 2024-04-17       | Python docstring substrate.                                                                                          |
|  [12]   | [Google Python Style Guide docstrings](https://google.github.io/styleguide/pyguide.html#383-functions-and-methods) | Maintained style source, accessed 2026-06-05             | Google docstring field semantics.                                                                                    |
|  [13]   | [mkdocstrings-python docstring rendering](https://mkdocstrings.github.io/python/usage/configuration/docstrings/)   | Page shows 2025-11-10 update marker, accessed 2026-06-05 | Generated-reference behavior for `Receives:`, `Yields:`, `Returns:`, `Warns:`, and type parameters.                  |
|  [14]   | [PEP 810](https://peps.python.org/pep-0810/)                                                                       | Accepted for Python 3.15, accessed 2026-06-05            | Future-facing lazy import behavior and deferred import-error timing.                                                 |
|  [15]   | [PEP 789](https://peps.python.org/pep-0789/)                                                                       | Draft for Python 3.14, accessed 2026-06-05               | Background only for cancel-scope/yield hazards; AnyIO docs control active guidance.                                  |

## [3][FINDINGS]

### [3.1][BASELINE]

Finding: The active Python capsule names Python 3.15, but the repository Python configuration is still 3.14. Python 3.15 docs are current beta docs and primary sources exist for `TypeForm`, `@disjoint_base`, and `frozendict`, but Rasm's actual checkers and formatter are configured against Python 3.14.

Implication: The active standard can keep a future-facing Python 3.15 capsule only if it avoids implying the repo currently validates Python 3.15 syntax or runtime behavior. For Rasm-local docstring guidance, call the 3.15 items "feature-gated type truth" until `pyproject.toml`, `ty`, `mypy`, and `ruff` move.

Candidate delta: Split the Python capsule into current Rasm baseline and future-facing type-truth bullets, or add a local qualifier: "Use Python 3.15 features only when the repo target and toolchain have moved; until then, Python 3.14 repo settings control executable proof."

### [3.2][RESULT]

Finding: Rasm's approved Python rail is `expression.Result[T, E]`, not the `returns` package. Expression documents `Result[TSource, TError]` as composable monadic error handling and ROP, with `Ok`, `Error`, `bind`, `map`, `map_error`, `of_option`, and `to_option` operations.

Implication: A public function returning `Result[T, E]` needs `Returns:` text that names the `Ok` payload and each meaningful `Error` variant or domain fault class. It should not use `Raises:` for failures returned as `Error`.

Candidate delta: Change generic `Result[T, E]` wording to "Rasm Python uses `expression.Result[T, E]`: document `Ok` payload, each `Error` variant, and the boundary where native exceptions are converted into the rail."

### [3.3][OPTION]

Finding: Expression `Option[T]` exposes `Some` and singleton `Nothing`, conversions to and from `Result`, and `to_optional()`. Its `.value` property raises `ValueError` for `Nothing`, which means unchecked extraction reintroduces a throwing boundary.

Implication: A public `Option[T]` return should document absence only when absence has domain meaning beyond the name and annotation. A public API should not document `.value` failure unless the API intentionally exposes unchecked extraction; better docstrings describe how callers stay on the `Option` rail or convert to `Result`.

Candidate delta: Keep the existing "absence semantics only when not obvious" rule, and add "unchecked `Option.value` extraction is a throwing boundary; document it only if the public surface intentionally exposes it."

### [3.4][EXCEPTIONS_TO_RAILS]

Finding: Python native exceptions are still the runtime substrate: all exceptions derive from `BaseException`, and `ExceptionGroup` / `BaseExceptionGroup` carry grouped exceptions. Rasm policy and local banned APIs prefer typed result rails and marked boundary adapters over exception-style control flow.

Implication: The Python capsule should distinguish three cases:
- Domain rail: native exceptions are caught at the adapter and converted to `Error` variants; docstrings put the conversion policy in `Returns:`.
- Throwing surface: the public contract intentionally exposes native exceptions; docstrings use `Raises:` with concrete types and causes.
- Terminal boundary: CLI, process, async runner, test harness, bridge, or plugin boundary maps exceptions or rail failures to exit status, JSON envelope, log, rejection, or task failure; docstrings name that mapping.

Candidate delta: Add a native conversion bullet under `[RAILS_RESOURCES]`: "Exception conversion belongs at the boundary owner; `Raises:` appears only for intentionally exposed native exceptions, while converted exceptions are named as typed `Error` variants or terminal-boundary outcomes."

### [3.5][GENERATORS]

Finding: Google docstrings say `Yields:` documents the object returned by `next()`, not the generator object. mkdocstrings-python can render `Receives:` for `send()` input and `Yields:` for yielded values. Python's expression reference makes generator and async-generator finalization observable through `close()` / `aclose()`, `GeneratorExit`, `StopIteration`, and `StopAsyncIteration`.

Implication: Public generator docstrings need semantic fields only when the public contract uses the generator protocol beyond plain iteration:
- `Yields:` for item meaning, ordering, and end condition.
- `Receives:` for `send()` input.
- `Returns:` only when the generator return value is part of the public contract through `StopIteration.value`; otherwise omit.
- Extended summary for close/finalization, resource ownership, and side effects when observable.

Candidate delta: Expand the Python capsule's `Yields:` / `Receives:` line into a generator-specific rail rule covering item semantics, ordering, `send()` input, completion value only when observable, close/finalization ownership, and resource release.

### [3.6][ASYNC_GENERATORS]

Finding: Python docs state that async generators that exit early by `break`, task cancellation, or other exceptions may run cleanup in unexpected contexts unless the scheduler calls and runs `aclose()`. The same docs state `yield from` is a syntax error inside async generator functions and that async generator completion raises `StopAsyncIteration`.

Implication: A public async-generator docstring should not merely say "yields items." It must name cancellation and finalization duties when the generator holds resources, uses context variables, owns child work, or relies on a scheduler/event loop for cleanup.

Candidate delta: Add an async-generator bullet: "`AsyncGenerator` comments document item semantics, ordering, cancellation behavior, `aclose()` owner, cleanup context, and resource release when any of those are caller-visible."

### [3.7][ANYIO_CANCELLATION]

Finding: AnyIO 4.13 uses cancel scopes and level cancellation. A task inside an effectively cancelled scope receives a cancellation exception at yield points. AnyIO docs require re-raising caught cancellation exceptions, recommend shielding cleanup when awaiting during finalization, and warn that cancel scopes must enter and exit in LIFO order. They explicitly list yielding in an async generator while enclosed in a cancel scope as a cancel-scope stack-corruption risk.

Implication: Rasm Python docstrings should document anyio cancellation only where the public surface changes caller obligations:
- timeout owner and whether `move_on_after()` silently exits or `fail_after()` raises `TimeoutError`;
- cleanup shielding and whether finalization awaits inside a shielded scope;
- cancellation re-raise behavior;
- whether child tasks, subprocesses, network calls, file operations, or resource finalizers receive cancellation;
- prohibition or explicit design route for yielding from inside cancel scopes/task groups.

Candidate delta: Replace broad "anyio cancellation" wording with a finite checklist: "deadline owner, timeout outcome, shielded cleanup, re-raise requirement, propagated cancellation target, and no `yield` inside cancel scopes/task groups unless the public shape is a context manager that yields an iterable rather than suspending the cancel scope."

### [3.8][EXCEPTION_GROUP]

Finding: AnyIO task groups can raise `ExceptionGroup` or `BaseExceptionGroup` when more than one task raises, including when cleanup after cancellation raises. Python docs specify that `except*` matches subgroups by contained exception type, and `ExceptionGroup` is generic over contained exception types.

Implication: `ExceptionGroup` belongs in Python source documentation only when a public async/task-group surface exposes grouped native exceptions. If the surface converts task failures into `Result` errors, the docstring should name the grouped conversion policy in `Returns:` and omit `Raises:`.

Candidate delta: Add a sentence to the existing `ExceptionGroup` bullet: "Document grouped exceptions as a throwing surface only when callers catch them with `except*`; otherwise document the conversion into typed rail variants."

### [3.9][TERMINAL_BOUNDARIES]

Finding: Rasm local policy bans direct `asyncio`, raw `subprocess`, direct sockets, direct stdlib logging, scattered env access, and `contextlib.suppress`, routing those concerns through anyio, httpx adapters, pydantic-settings, structlog/OpenTelemetry, and `Result` rails. The active code-documentation standard already has a terminal-boundary concept, but the Python capsule does not make the Python-specific terminal shapes explicit.

Implication: Python docstrings for command functions, async runners, process adapters, network adapters, and generated-reference entrypoints should name:
- when the effect starts;
- who owns the anyio runtime or cancel scope;
- how native exceptions become `Error`, `ExceptionGroup`, timeout, exit status, JSON envelope, rejected promise-equivalent, or log event;
- which resources are opened and closed by the call;
- whether cancellation reaches the external process, socket, stream, task group, or finalizer.

Candidate delta: Add "terminal boundary" as a first-class Python rail/resource rule, parallel to the C# `Eff.Run` and TypeScript `Effect` terminal-runner language.

## [4][DOCSTRING_SHAPE]

Recommended Python public-surface review record for Rasm rail and async surfaces:

```text template
Surface: `<module.class.function or method>`
Profile: `<rail, throwing, generator, async-generator, terminal, or pure>`
Machine-shape source: `<signature, annotation, Protocol, TypedDict, pydantic/msgspec model, or generated reference>`
Returns or yields: `<Ok payload, Some payload, yielded item, send input, completion value, or omit>`
Failure carrier: `<expression.Error variant, Nothing absence, ExceptionGroup, concrete exception, timeout, exit status, or omit>`
Native conversion: `<exception-to-Error conversion, grouped conversion, terminal mapping, or omit>`
Cancellation/resource: `<deadline owner, shielded cleanup, re-raise, aclose owner, resource close, task/subprocess propagation, or omit>`
Generated reference: `<Griffe/mkdocstrings anchor or omit>`
Route-away: `<README, API reference, runbook, how-to, or omit>`
```

This record should remain an authoring/review aid, not a source-comment template.

## [5][CANDIDATE_STANDARD_DELTAS]

[HIGH_VALUE]:
- Name `expression.Result` and `expression.Option` as the Rasm-approved Python rails when the standard is repo-specific.
- Add native-exception conversion as a boundary rule: converted exceptions belong in `Returns:` as typed errors; intentionally exposed native exceptions belong in `Raises:`.
- Add generator and async-generator rules for `Yields:`, `Receives:`, completion values, `aclose()` ownership, and cleanup/resource semantics.
- Add an anyio cancellation checklist: deadline owner, timeout outcome, shielding, re-raise, propagation target, and cancel-scope/yield prohibition.
- Add `ExceptionGroup` guidance: throwing surface only when callers catch grouped exceptions; rail surfaces document grouped failure conversion.
- Add terminal-boundary guidance for anyio process/network/task runner adapters.

[VERSION_GATING]:
- Do not claim Rasm Python 3.15 executable proof until `pyproject.toml` and local tool settings move off Python 3.14.
- Keep Python 3.15 features (`TypeForm`, `@disjoint_base`, `frozendict`, accepted lazy imports) as current upstream/future-facing type truth, not current Rasm validation truth.
- Treat PEP 789 as useful background only while it remains draft; AnyIO 4.13 docs already provide enough active guidance against yielding inside cancel scopes.

[REJECT]:
- Do not add `Raises:` for `expression.Result.Error` variants.
- Do not document `Option` absence when the name and type already carry it.
- Do not document every generator as a resource surface; only public generator contracts with ordering, `send()`, cancellation, finalization, or resource semantics need expanded fields.
- Do not import TypeScript `Effect<A, E, R>` or C# `Eff<RT, T>` vocabulary into Python unless a Python source symbol or approved dependency owns that shape.
- Do not cite generated Python API docs, Griffe, or mkdocstrings as configured Rasm gates unless the repository adds those tools and a generation command.

## [6][BOUNDARIES]

This report is source research for one active standards page. It should not be copied wholesale into `code-documentation.md`; promote only durable rules that change future agent behavior. Active standards still own final phrasing, route-away language, validation checklists, and whether Python 3.15 feature bullets remain in the general capsule.

## [7][VALIDATION]

- [x] The report cites current primary sources close to each claim group.
- [x] The report distinguishes Rasm local repo truth from upstream Python 3.15 beta truth.
- [x] The report leaves active standards untouched.
- [x] The report does not claim generated Python API tooling is configured in Rasm.
