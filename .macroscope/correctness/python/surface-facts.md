---
include:
  - "libs/python/**"
  - "**/*.py"
---

# [PYTHON_SURFACE_FACTS]

Corpus floor is py3.15 — its surface is never flagged as invalid syntax, a missing import, or a hallucinated API. Every entry below is a verified truth a generic reviewer misfires on; flagging one is a false positive.

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
