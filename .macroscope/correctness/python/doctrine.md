---
include:
  - "libs/python/**"
  - "**/*.py"
---

# [PYTHON_DOCTRINE]

`docs/stacks/python/` is the floor for every Python surface — fence code in planning pages and operator source alike, judged as production on the py3.15 floor. Rails, admission-once boundaries, closed dispatch, frozen payloads, and structured concurrency are the ruling paradigm; a conformant-but-weak form is a finding when a stronger form exists.

## [01]-[RAILS]

- A fallible step returns `Result[T, E]`; a non-failing absent step returns `Option[T]`; `def f() -> T | None` signaling failure, `raise DomainError` in interior flow, and `except Exception` silently railing are findings.
- Failure vocabulary is closed and owned — `Literal` set, `StrEnum`, or `@tagged_union` — never bare `str` for a multi-cause domain, never an `Exception` subclass in the carrier slot. A tagged-union fault is a value consumed by `match`; the exception-to-fault conversion is one-directional, so a fault never re-raises.
- A carrier is chosen once at admission and threaded unchanged; a `to_option().to_result(...)` round-trip mid-pipeline stamps over the original fault, and an `Option.value` read without a prior proof is a finding.
- Carrier decomposition uses tag patterns — `case Result(tag="ok", ok=v)` — never class patterns: `case Ok(value):` raises at runtime because `Ok`/`Error`/`Some` are constructor functions, not classes; as constructors they are legal.
- Independent operands compose applicatively (`map2`/`traverse`/`sequence`); a `bind` chain over independent operands reports only the first failure and silently discards the rest. Abort-vs-accumulate fixes once at the boundary as a policy value, never a `strict: bool` the body re-derives.
- Collapse to a bare value or raise happens only at the process, CLI, network, or persistence edge; egress logging is asymmetric — `Error` folds to one `structlog` event, `Ok` stays silent.

## [02]-[BOUNDARIES]

- Raw material admits exactly once into an evidence-carrying owner; interior code never re-validates — a second `TypeAdapter`/`model_validate` pass in a domain interior, or a re-check of a field the factory proved, is the defect.
- A payload `TypedDict` admits through one module-level `TypeAdapter` and appears only at the `Unpack[TypedDict]` root signature, never forwarded inward.
- A float bound closes over the non-finite domain: a bare `ge=0` on a float field with no finiteness predicate admits infinity.
- A foreign sentinel projects at the single first-read site; a nullable riding past the seam is a second admission site. Environment reads fuse into one `pydantic-settings` construction; raw `os.environ` in interior flow is a finding.
- `Is[...]` and `msgspec.Meta(...)` never share one `Annotated` on a `@beartype` factory hint — `[Is, Meta]` raises, `[Meta, Is]` silently drops the `Is`.
- Each validator enforces only its own metadata — `msgspec.Meta(...)` on the wire, a `pydantic` constrained field at ingress, `Is[...]` at the owner factory — and no validator reads a foreign one's marker; a bound expressed in a marker its validator silently ignores is a finding.
- A plugin set is read from distribution metadata (`importlib.metadata.entry_points`) without importing the plugin module and activated once through `EntryPoint.load()` traversed fallibly; an import-time side-effect registry — a decorator appending to a module global, a metaclass, `__init_subclass__` — silently emptied by `lazy import` is the rejected form.
- Digest equality uses `hmac.compare_digest`; key and nonce material comes from `secrets`, never `random`; a persisted fingerprint is a `hashlib` digest, never process-randomized `hash()`; byte-identical sub-trees ride `msgspec.Raw` signed from bytes captured before any parse.
- A recursive-node key uses structural path (`tuple[int, ...]` of child ordinals), never content alone; a boundary memo key joins the foreign axis via `id()` only for an axis content cannot recover.

## [03]-[DISPATCH_AND_SHAPES]

- A closed domain dispatches through one operation-local `match` with `case _ as unreachable: assert_never(unreachable)` as the exhaustiveness witness; tag `if`/`elif` chains and catch-all arms hiding a missing case are findings.
- A verb family is one `@tagged_union` request under one total `match`; sibling `create()`/`update()` functions and `dispatch(verb: str, **kwargs)` are findings. Singular/plural/batch collapse into one `T | Iterable[T]` parameter discriminated by shape — `process`/`process_many` siblings and `batch=True` flags are findings.
- `functools.singledispatch` is reserved for a genuinely open type set foreign code extends; a closed owned family dispatches via `match` or a `frozendict` table.
- Durable owners freeze after materialization (`@dataclass(frozen=True, slots=True, kw_only=True)`, `msgspec.Struct(frozen=True)`); state change is a transition returning `Self` or a closed successor union. Durable collections are `tuple`/`frozenset`/`frozendict`; `MappingProxyType` over mutable storage is not immutability.
- A `frozendict` table declares one primary correspondence as the single edit site; every secondary map derives by comprehension over the primary — a hand-maintained parallel inverse is a finding.
- Wire shapes stay at the edge (`msgspec.Struct` with `rename=`/`forbid_unknown_fields=True`); canonical owners carry no codec attributes. A numeric solve leaves as a frozen receipt carrying route, tolerance, and recomputed true relative residual — never a raw `ndarray` or factorization handle.
- Route selection rides one `Route` `StrEnum` plus `frozendict[Route, Policy]` tables; a `mode: str` or `sym: bool` knob beside the operand, or sibling per-route solve functions, are findings.

## [04]-[CONCURRENCY_AND_IMPORTS]

- Concurrent children run inside `anyio.create_task_group()`; `asyncio.gather`/`create_task` and closure-captured accumulation lists are findings. `start_soon` returns `TaskHandle[T]` and a child result reads `handle.return_value`. Deadlines are `CancelScope` (`fail_after`/`move_on_after`), never a `timeout=` parameter threaded inward.
- Cancellation re-raises `anyio.get_cancelled_exc_class()` after cleanup, never caught into `Result.Error`, never in a retry `on=` set. Retry is bounded and triggers only on a raised transient at the effect boundary; re-raising a railed `Error` to force a retry is a finding.
- Blocking, CPU, and native work offload through `to_thread`/`to_interpreter`/`to_process` under an explicit `CapacityLimiter`; a bare executor on the loop and unbounded fan-out are findings.
- Group faults convert once at the `except*` edge, one arm per vocabulary case; `return`/`break`/`continue` inside `except*` is illegal.
- A native handle rides a closed capsule with release registered via `weakref.finalize`, never `__del__`; a borrow window (`memoryview`) is strictly synchronous — an `await` between a view and its `bytes` copy is a finding; dynamic handle sets ride one `AsyncExitStack` with shielded cleanup.
- Every dependency is named once at module scope; cold, heavy, native, or cyclic ones defer via `lazy import`/`lazy from`. Function-local imports, `sys.modules` mutation, star and relative imports, and `from __future__ import annotations` are findings. A module-scope constant must not dereference a lazy-imported name, and a module whose import registers a side effect is never lazy-deferred.
- Exports are one explicit end-of-file `__all__`; barrel files and facade re-exports are findings.
