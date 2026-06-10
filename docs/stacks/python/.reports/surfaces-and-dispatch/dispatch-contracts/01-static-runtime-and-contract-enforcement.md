# [STATIC_RUNTIME_DISPATCH_CONTRACTS]

[STATIC_RUNTIME_LINE]:
- Static dispatch (`match`/`@overload`/`TypeIs`/`@disjoint_base`) is resolved by the checker before execution: zero runtime cost and exhaustiveness-proven, where a missing arm is a type error. Runtime dispatch (`singledispatch`/value table/`Protocol` isinstance) decides on runtime identity at call time, cannot be exhaustiveness-proven, and therefore requires a runtime contract at the boundary. [verified, 2026-06-09]
- The layering: static narrows inside the body (checker-proven, free); runtime resolves the concrete handler at ingress; a runtime contract closes the gap between what the checker proved and what actually arrives.

[BEARTYPE_CONTRACT]:
- `@beartype` generates an O(1) check wrapper at decoration time (default `BeartypeStrategy.O1` samples one random container item per call) and enforces what the annotations already say with no coercion. [beartype 0.23.0 (git pin) _conf/confmain.py, installed, 2026-06-09]
- `BeartypeConf(violation_type=DomainError)` redirects all violations to a chosen exception type, or per-site via `violation_param_type` / `violation_return_type` / `violation_door_type`; `strategy`, `violation_verbosity`, and `is_debug` are the other levers. The hierarchy is `BeartypeCallHintViolation -> BeartypeHintViolation -> BeartypeException -> Exception`.
- `@beartype` must be outermost to guard the whole invocation; the violation is caught at the boundary and converted to `Error(...)`, never propagated into domain flow. `beartype.door.is_bearable(obj, hint)` is the non-raising predicate; `die_if_unbearable` is the raising form for validation stages.
- `beartype.claw.beartype_this_package()` applies `@beartype` to every annotated callable at import via an import hook — the package-wide contract posture, distinct from per-function `@beartype` (a boundary-specific contract).

[VALIDATE_CALL_VS_BEARTYPE]:
- `pydantic.validate_call(config=, validate_return=False)` COERCES at the boundary (e.g. `"42" -> 42`) and raises `pydantic_core.ValidationError` (a subclass of `ValueError`); `ConfigDict(strict=True)` disables coercion and `validate_return=True` checks the return. [pydantic 2.13.4, 2026-06-09]
- Discriminator: `@validate_call` is a coercing ingress gate with rich field errors (HTTP, CLI, env, where pydantic owns the schema); `@beartype` is a pure type contract with no coercion for internal module boundaries. Do not stack both on one function — the outer governs.

[FREE_THREADING]:
- Python 3.15.0b1's default build keeps the GIL (`Py_GIL_DISABLED=0`); PEP 703 is the no-GIL build and PEP 779 formalizes the supported free-threaded tier. [sysconfig, 2026-06-09]
- `singledispatch` has no internal lock — `registry` is a plain dict and `dispatch_cache` a `WeakKeyDictionary` — so all `.register()` must complete at import time; concurrent register plus dispatch under no-GIL is a data race (`cache_token` TOCTOU). A `frozendict` value table is immutable and unconditionally read-safe. `ContextVar` per-call aspect state is safe under all threading models via the per-thread and per-task copy.
