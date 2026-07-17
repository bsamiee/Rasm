---
include:
  - "libs/python/**"
  - "**/*.py"
---

# [PYTHON_SURFACE_FACTS]

Corpus floor is py3.15 — its surface is never flagged as invalid syntax, a missing import, or a hallucinated API, at any severity, because doctrine fences themselves spell these forms. Pinned-newest provider surfaces — anyio, loky, pebble among them — are corpus law, so a released-library semantics claim verifies against the owning doctrine page before flagging. Every entry below is a verified truth a generic reviewer misfires on; flagging one is a false positive.

## [01]-[LANGUAGE_SURFACE]

- `from builtins import frozendict` is the legislated spelling: `frozendict` is a builtin, not a `dict` subclass — insertion-ordered for iteration, order-free for equality and hash, shallowly immutable.
- `lazy import X` / `lazy from X import Y` are module-scope soft keywords; a `SyntaxError` on a `lazy` statement inside a function, class, or `try` is the intended grammar, not a bug.
- Paren-less chained `except A, B:` is the ruff-format canonical form; re-adding grouping parens fails the format gate.
- `tuple()` is a class pattern matching any tuple instance; only the literal `()` matches the empty tuple — a claim built on the opposite reading is wrong.
- t-strings (`t"..."`) with `string.templatelib.Template`/`Interpolation` are the required untrusted-markup form; `Interpolation.conversion` is an integer ordinal.
- Active-surface owners include `math.integer`, `math.fma`/`sumprod`, `uuid.uuid7`, `heapq` `*_max` family, `compression.zstd`, `os.readinto`, `os.process_cpu_count`, `Path.copy_into`/`move_into`/`info`, `functools.Placeholder`, `hmac.digest`, `concurrent.interpreters`, `InterpreterPoolExecutor`, `sys._is_gil_enabled`, `sys.set_lazy_imports_filter`, `sys.monitoring`, `annotationlib.get_annotations`.
- Typing rides inline `class C[T]`, `type` aliases, `TypeIs`, `TypeForm`, `Unpack[TypedDict]`, `TypedDict(closed=True)`, `Required`/`NotRequired`/`ReadOnly`; `from typing_extensions import X` for a member `typing` exports is the rejected form — `Sentinel` (PEP 661) is the one exception `typing` does not yet export.
- `Sentinel("NAME")` minted once as a module global, compared by `is`, carried as `T | NAME`, is a legitimate default form, not a magic value.
- `if TYPE_CHECKING:` imports are the narrower form for names that must never load at runtime; a `lazy from` used only in annotations costs nothing under deferred annotation evaluation.

## [02]-[STATEMENT_SEAMS]

Doctrine pages license named statement kernels where a `for`, `try`, or in-place mutation is platform-forced — never flag these as rail violations: the ordered-capture admission kernel (`except ValidationError` before `except ValueError`, since pydantic subclasses `ValueError`), the `TypeAdapter.validate_python` payload gate, the `EnumType(value)` `try/except ValueError` vocabulary crossing, the module `__getattr__` resolver, the aspect `try/except BeartypeCallHintViolation` rail-lift, the capsule `memoryview` release kernel, the async sequential fold under `move_on_after`, the shielded teardown `await`, recursion-frontier `while` kernels, and in-place heap operations. A single `if not isinstance(stream, (str, bytes))` guard on the `Iterable()` normalization arm is the named platform-forced seam, not a smuggled discriminant.

## [03]-[VERIFIED_TRAPS]

Truths that flip the polarity of a naive finding — the listed shape is the FIX, its inverse the bug:

- Any type in a `@checked`/beartype-wrapped signature imports unconditionally; moving it under `if TYPE_CHECKING:` crashes at first call while every static gate stays green — flag TYPE_CHECKING-stranded types on checked signatures, never demand the move.
- `structlog.BytesLoggerFactory()` with no `file=` writes to `sys.stdout.buffer`; each renderer branch carries its own explicit `file=sys.stderr.buffer` — a bare factory on a stdout-protocol tool is a real collision bug.
- A definition-time `@beartype` aspect never raises into domain flow: the weave catches `BeartypeCallHintViolation` and lifts it onto the fault rail; a bare `@beartype` throwing into the interior is the finding.
- `expression.MailboxProcessor` is forbidden on the anyio substrate (it reaches `asyncio.get_event_loop()`); the single-consumer anyio send/receive drain is the ruled replacement.
- Platform-gated type suppressions in the operator estate are irreducible pairs — removing one flips the rejection between `ty` and `mypy`; demanding their removal is the false positive.
- `tg.start_soon(...)` returns `TaskHandle[T]` and a child result reads `handle.return_value` — the pinned-anyio CHILD_CARRIER law `docs/stacks/python/concurrency.md` rules; a returns-None claim applies released-library semantics to the pinned surface.
- SHM release-in-finally is the legislated POSIX semantics: an attached mapping outlives the unlink, so the finally release is the fix and deferred cleanup leaks blocks for workers that never run; `Wire.SHARED_MEMORY` dispatches per argument shape — only bare ndarrays ride the span channel.
- Pool self-healing is structural: `reuse='auto'` replaces broken singletons and pool-DEAD is capsule absence, so a broken-probe `alive()` demand re-litigates the design; pebble ships no public pid roster — the complement over owned sets IS the pebble set under the custody law.
- A msgspec tagged-union fault carries no BaseException lineage: it is consumed as a value by `match` and never re-raised — an exception-subclass mint inverts the one-direction exception-to-fault law.
- Preview is the five-field contract with the trailing band per `core/receipt.md`; a DEGRADED roll double-buffers so capacity never gaps, and demanding NOT_SERVING before the roll advertises a phantom outage.
- An in-place delta bounds by shift-displaced occupancy over one shared region; a from-plus-to size sum contradicts in-place patching's defining property.
- `expression.extra.result.catch(exception=X)` is the substrate's single-exception trap minting `Result[T, X]` from a named raise; `effect.try_` composes `Try` values through `yield from` and never catches a provider raise — demanding `effect.try_` as the exception adapter is a false positive.
- `msgspec.UNSET` (a field typed `T | UnsetType = UNSET` round-trips absent under `omit_defaults`) and `pydantic.experimental.MISSING` are the codec's own presence sentinels collapsed at the seam; a domain absence axis that changes dispatch is a `@tagged_union` case, never the wire sentinel leaked inward.
- `expression.Map` is an ordered tree requiring a total order on every key — a second insert over an unordered or heterogeneous key set raises `TypeError` after every single-entry test passes — so an insertion-order-dependent derivation rides `frozendict`, never `Map`.
- An owned enum mirroring a provider vocabulary admits the provider's full member roster: the mirror construction `EnumType(foreign_value)` raises `ValueError` on every dropped member, so mirrored-vocabulary coverage is a totality obligation, not styling.
