# [H1][STATUS]
>**Dictum:** *One `StrEnum` is the whole status algebra; every exit code originates in a member; the rail return code is always `status.exit_code`.*

## [1][PURPOSE]

`core/status.py` defines `RailStatus(StrEnum)` and two module-level operations (`join`, `fold`) — the sole status type (ARCHITECTURE §17 Inv 2). Three payloads ride each member via `__new__`: the wire `value` (the `StrEnum` token, also the `msgspec` encode value and the `match` key), an `exit_code`, and a `severity` fold-rank. No status-string projector, `ApiStatus`, or `StaticOutcome` may exist (§4 retire row 1, D15). One member instance is reused unchanged across three subsystems — Cyclopts token, `msgspec` wire value, `match` discriminant — so `__main__` returns `report.status.exit_code` and nothing else computes an exit code. This module is Stage 1, depending on nothing (§14).

## [2][CANONICAL_SHAPES]

The full 8-member table is law (§8, D29). `severity` is a total order forming a bounded join-semilattice; `exit_code` is the §8 algebra (D29). `FAULTED=("faulted",2)` severity 7 is the absorbing top.

| [MEMBER] | [value] | [exit] | [sev] | [alias] | [channel] | [MEANING] |
| -------- | ------- | :----: | :---: | ------- | --------- | --------- |
| `SKIP` | `"skip"` | 0 | 0 | `"skipped"` | Completed | Per-check opt-out; vacuous, never gates CI. |
| `EMPTY` | `"empty"` | 0 | 1 | — | Completed | Ran clean / nothing in scope. Fold seed (D15). |
| `OK` | `"ok"` | 0 | 2 | — | Completed | Evidence affirmed; a parser set it explicitly. |
| `UNSUPPORTED` | `"unsupported"` | 3 | 3 | — | Completed | Valid precondition, no applicable path/tool. |
| `BUSY` | `"busy"` | 5 | 4 | — | Fault | Exclusive lease held; retry elsewhere, never wait. |
| `TIMEOUT` | `"timeout"` | 5 | 5 | — | Fault | Deadline exceeded (`fail_after` / rc 124). |
| `FAILED` | `"failed"` | 1 | 6 | — | Completed | A check ran and found defects. |
| `FAULTED` | `"faulted"` | 2 | 7 | — | Fault | Operational failure — assay could not run the check. Fold top. |

`from_returncode` (D12, classmethod): `0→EMPTY`, `5→BUSY`, `124→TIMEOUT`, else `FAILED` — **never** `OK` (a parser affirms `OK`), **never** `UNSUPPORTED` (a routing decision). `Fault.status` defaults to `FAULTED` (D28). `join` and `fold` are **module-level functions**, not methods: `join` is binary max-by-severity (D15 — **not** `worst`), `fold` is `reduce(join, members, EMPTY)` seeding `EMPTY` so a rail that genuinely ran but had no checks reports `EMPTY`, not `SKIP`. Public surface is exactly `__all__ = ["RailStatus", "fold", "join"]`.

## [3][VALIDATED_SNIPPET]

The 3-payload `__new__` (value, exit_code, severity, *aliases) is canonical; `join`/`fold` are lifted to module scope (`str.join` already owns the `join` name with an LSP-incompatible signature, so a method would shadow it). No `from __future__ import annotations` — the lone forward-ref (`from_returncode -> RailStatus`) resolves lazily as a method annotation.

```python
from enum import StrEnum
from functools import reduce
from typing import Self

# --- [TYPES] ----------------------------------------------------------------------------

class RailStatus(StrEnum):
    exit_code: int                       # annotation-only; metaclass skips it as a member
    severity: int
    SKIP        = "skip", 0, 0, "skipped"
    EMPTY       = "empty", 0, 1
    OK          = "ok", 0, 2
    UNSUPPORTED = "unsupported", 3, 3
    BUSY        = "busy", 5, 4
    TIMEOUT     = "timeout", 5, 5
    FAILED      = "failed", 1, 6
    FAULTED     = "faulted", 2, 7

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        member = str.__new__(cls, value)          # member IS its wire string (StrEnum contract)
        member._value_, member.exit_code, member.severity = value, exit_code, severity
        for alias in aliases:
            member._add_value_alias_(alias)        # py3.13+: alias→member in _value2member_map_
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        match rc:
            case 0:   return cls.EMPTY
            case 5:   return cls.BUSY
            case 124: return cls.TIMEOUT
            case _:   return cls.FAILED

# --- [OPERATIONS] -----------------------------------------------------------------------

def join(left: RailStatus, right: RailStatus) -> RailStatus:
    return left if left.severity >= right.severity else right

def fold(*members: RailStatus) -> RailStatus:
    return reduce(join, members, RailStatus.EMPTY)
```

## [4][SEAMS]

| [CONSUMER] | [USE] |
| ---------- | ----- |
| `core/model.py` | `Completed.status` (default `EMPTY`) / `Fault.status` (default `FAULTED`, D28); `receipt(...)` defaults `status` to `from_returncode(rc)` (D11/D12); `model.fold` imports module `fold as rail_fold` and calls `rail_fold(*(o.status for o in outcomes))` over the success-channel `Completed.status` only; `envelope`/`Envelope.exit_code == status.exit_code` (D29). |
| `core/engine.py` | `run_check` projects a process `returncode` via `receipt → from_returncode`; `fail_after`/rc-124 → `Fault(TIMEOUT)`; `OSError` spawn failure → `Fault(FAULTED)`; `leased`/`exclusive_lease` live contention → `Fault(BUSY)` (exit 5, D34/D40), `OSError` at the lock fd → `Fault(FAULTED)`. |
| rails (`static`/`test`/`bridge`/`package`/`api`/`docs`) | Each rail handler returns `Result[Report, Fault]`; many `Completed.status` fold into one `Report.status` via `model.fold`. |
| `composition/registry.py` | `rail` weaves `compose(*_RAIL_LAYERS)(_narrow(bind.handler))`; `_strict` (D52) sits between the handler and `_emit`, promoting a folded `EMPTY`/`SKIP` to a `FAULTED` `Fault` under `--strict` (a flag, not a member); `_guard` catches the docs `FaultedPromotion` → `Fault(FAULTED)`; `_emit` reads `Envelope.exit_code` off `status.exit_code` (D29) and `__cyclopts_returncode__` returns it (D30); `match status` dispatch closes with `assert_never`. |
| `msgspec` / Cyclopts | Encodes by `_value_` (encode-by-value); decodes `value` **and** aliases via `_value2member_map_`; Cyclopts derives choices from members; one instance serves all three. |

## [5][EXTENSIBILITY]

A new outcome is one member with `(value, exit_code, severity, *aliases)`; a new external token spelling is a `*aliases` entry on an existing member (e.g. a future `"timed_out"` on `TIMEOUT`) — never a parallel `Literal`, never a `_missing_` coercion (drift fails loud as `msgspec.ValidationError`, §17 Inv 5).

## [6][CONSIDERATIONS]

- **Exit ≠ severity ordering.** `join` ranks by `severity` (7-fold total order), not `exit_code`: a fold of `{FAILED, TIMEOUT}` wins on `FAILED` (sev 6 > 5) → exit `1`, so a definitive defect dominates an indeterminate sibling even though `TIMEOUT`'s exit `5` is numerically larger. The §8 channel split is orthogonal: `FAULTED`/`BUSY`/`TIMEOUT` ride the `Result` Error channel and **never enter the success-channel fold** (`model.fold` only sees `Completed.status`), so `join`'s top-`FAULTED` rank matters only for the rare manual fold over mixed members — the live rail fold tops out at `FAILED`.
- **`_add_value_alias_` is a load-time guard.** It raises `ValueError` if an alias equals an existing value or alias, so an alias can never shadow a canonical token or be shared across members — the spam ban is enforced by the metaclass, not by convention. Keep `SKIP→"skipped"` as the sole alias until a concrete emitter forces another.
- **`str`-equality footgun under `match`.** Members are `str` subclasses, so `RailStatus.OK == "ok"` holds and a bare-string `case "ok":` would match — restrict domain dispatch to `case RailStatus.OK:` with `assert_never(status)` so an added member breaks `ty` rather than silently falling through; this also pins the Cyclopts token form to `value` (verify at build time).
