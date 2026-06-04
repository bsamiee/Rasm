# [H1][ASSAY_STATUS]
>**Dictum:** *One StrEnum is the whole status algebra; every non-zero exit originates in a member.*

`core/status.py` defines `RailStatus(StrEnum)` and nothing else. It is the sole status type (Invariant 2): the CLI return code is always `report.status.exit_code`, so no status-string projector, `ApiStatus`, or `StaticOutcome` can exist. Three orthogonal payloads ride each member: the wire `value`, an `exit_code`, and a fold-severity rank.

**Canonical:** [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) §2 · [`snippets/model-status.py.md`](snippets/model-status.py.md) §1 · [`CRITIQUE-SHAPES.md`](CRITIQUE-SHAPES.md) §3 P0.2–P0.3.

---
## [1][MEMBER_SET]

| [MEMBER]      | [value]         | [exit] | [sev] | [alias]    | [RATIONALE]                                                                 |
| ------------- | --------------- | :----: | :---: | ---------- | --------------------------------------------------------------------------- |
| `SKIP`        | `"skip"`        |   0    |   0   | `"skipped"`| Check opted out of scope; vacuous, must not gate CI. Fold identity (bottom).|
| `EMPTY`       | `"empty"`       |   0    |   1   | —          | Ran, zero work in scope (no changed files / no findings). Clean pass.       |
| `OK`          | `"ok"`          |   0    |   2   | —          | Affirmed positive success with findings cleared. Distinct notes from EMPTY. |
| `UNSUPPORTED` | `"unsupported"` |   3    |   3   | —          | No tool for this claim×language. Usage/routing error, not a defect.         |
| `BUSY`        | `"busy"`        |   5    |   4   | —          | Exclusive lease held; could not start. Transient, orchestrator may retry.   |
| `TIMEOUT`     | `"timeout"`     |   5    |   5   | —          | Deadline exceeded; result indeterminate. Operational, not a verdict.        |
| `FAILED`      | `"failed"`      |   1    |   6   | —          | Definitive quality defect — the verdict the gate exists to emit. Fold top.  |

**Exit-code rationale.** `0` (SKIP/EMPTY/OK) — the gate passed; CI proceeds; vacuous success never reds the pipeline. `1` (FAILED) — conventional generic failure, the one outcome a quality gate is built to surface. `3` (UNSUPPORTED) — held distinct from `1` so CI distinguishes "no tool routed" from a real defect, and distinct from `2` (Cyclopts/argparse usage error) to avoid collision. `5` (BUSY/TIMEOUT) — a single "could-not-complete" class matching the predecessor's lease/`fail_after` contract (`exclusive_lease`→busy, returncode `124`→timeout), distinct from `1`/`3` so a transient `5` is retryable without being read as a defect.

**Alias discipline.** Only `SKIP→"skipped"` exists, because the bridge boundary actually emits that spelling. No alias is added preemptively (spam ban); a new alias lands only when a concrete external emitter uses a divergent token (e.g. a future `"timed_out"`).

---
## [2][MEMBER_MECHANICS_AND_WIRE]

```python
class RailStatus(StrEnum):
    exit_code: int                       # annotation-only: NOT a member, becomes a member attr
    SKIP = "skip", 0, "skipped"
    EMPTY = "empty", 0
    OK = "ok", 0
    UNSUPPORTED = "unsupported", 3
    BUSY = "busy", 5
    TIMEOUT = "timeout", 5
    FAILED = "failed", 1

    def __new__(cls, value: str, exit_code: int, *aliases: str) -> Self:
        member = str.__new__(cls, value)     # member IS its wire string (StrEnum contract)
        member._value_ = value
        member.exit_code = exit_code
        for alias in aliases:
            member._add_value_alias_(alias)  # py3.13+: registers alias in _value2member_map_
        return member
```

`exit_code: int` is an annotation-only name, which the enum metaclass skips as a member; each assignment is a `tuple` that `__new__` unpacks into `value`, `exit_code`, and variadic `aliases`. `str.__new__(cls, value)` makes the member a `str` subclass equal to its wire token; `_add_value_alias_` adds alias→member entries to `_value2member_map_` **without** creating new names (so aliases are absent from `__members__` but present in the value map).

**Round-trip.** msgspec encodes any enum by `_value_` (PR #211, "encode by value"), so `encode(SKIP) == b'"skip"'` — never the alias, never the name. Decoding str-enums, msgspec builds its lookup from `_value2member_map_` (the map `_add_value_alias_` populates), so both the canonical value and every alias resolve to the singleton member. Proven empirically by the predecessor `wire_laws` (`tests/tools/quality/test_quality.py:154`): `decode(b'{"status":"skipped"}').status is RailStatus.SKIP` (identity, no `_missing_` hook). Thus: `decode(encode(m)) is m` for all members, and alias tokens decode to the canonical member while encode normalizes back to `value`.

---
## [3][FROM_RETURNCODE_AND_FOLD]

`from_returncode` maps a raw process code into the algebra — the only place a `subprocess` integer becomes a status:

| [returncode] | [→ member] | [WHY]                                              |
| :----------: | ---------- | ------------------------------------------------- |
|      `0`      | `EMPTY`    | Clean exit, nothing affirmed beyond "ran clean".  |
|      `5`      | `BUSY`     | Lease-contention convention.                      |
|     `124`     | `TIMEOUT`  | `anyio.fail_after` / coreutils timeout convention.|
|      `_`      | `FAILED`   | Any other non-zero is a defect.                   |

`from_returncode` never yields `OK` (a parser affirms `OK` explicitly) and never yields `UNSUPPORTED` (a routing decision, not a process code).

**Fold (monoid).** Status forms a bounded join-semilattice on the total severity order `SKIP < EMPTY < OK < UNSUPPORTED < BUSY < TIMEOUT < FAILED`. `join(a,b) = max-by-severity`; it is associative, commutative, idempotent. Identity (bottom) is `SKIP`; `FAILED` is the absorbing top. A rail folds many outcomes with `reduce(join, statuses, EMPTY)` — seeding `EMPTY` (not `SKIP`) so a rail that genuinely ran but had no checks, or whose checks all skipped, reports `EMPTY` rather than `SKIP` (SKIP is per-check opt-out; EMPTY is rail-level no-op). The winner's `exit_code` becomes the process code: a fold containing both `FAILED` and `TIMEOUT` reports `FAILED`→exit `1`, because a definitive defect dominates an indeterminate sibling.

---
## [4][STACKING]

One member instance serves three subsystems with zero translation and no parallel `Literal`:

| [SUBSYSTEM] | [USE]                                                                                             |
| ----------- | ------------------------------------------------------------------------------------------------- |
| msgspec     | `Report.status: RailStatus` field — encodes `value`, decodes value+aliases to the member (§2).    |
| `match`     | `match status: case RailStatus.FAILED: ...` with `assert_never` for exhaustiveness; reads `.exit_code` directly — no projector. |
| Cyclopts    | Enum-annotated parameter derives its choice set from the members; the StrEnum token *is* the value, so the instance Cyclopts yields is the same one msgspec encodes and `match` dispatches. |

The payoff: `__main__` returns `report.status.exit_code` and nothing else computes an exit code (Invariant 2). Because members are `str` subclasses, `RailStatus.OK == "ok"` holds for wire convenience, but canonical code dispatches on members.

---
## [5][EDGE_CASES_AND_OPEN_DECISIONS]

- **Unknown returncode.** Any non-zero ∉ `{5,124}` → `FAILED`. A tool that itself exits `3` collapses to `FAILED`, not `UNSUPPORTED` (UNSUPPORTED is never a process code). *Open:* honor a tool's literal `3`/`5`, or keep routing-only semantics (default: routing-only).
- **Unknown wire string.** No `_missing_` hook, so an unmapped token (e.g. `"weird"`) raises `msgspec.ValidationError` — fail-loud, matching the architecture's "drift fails loudly." *Open:* add `_missing_` coercing unknowns→`FAILED`, or keep strict (default: strict).
- **Alias collision.** `_add_value_alias_` raises `ValueError` if an alias equals an existing value or alias, so an alias can never shadow a canonical token or be shared across members — a load-time guardrail.
- **`str`-equality footgun.** Members compare equal to bare strings; restrict comparison to members in domain code to avoid silent mismatches against the wrong literal.
- **Fold exit code.** Default reports the severity-winner's `exit_code` (FAILED+TIMEOUT→`1`). *Open:* whether a co-present `TIMEOUT`/`BUSY` should force exit `5` (max-exit) instead of yielding to `FAILED`.
- **OK vs EMPTY.** Both exit `0`; they differ only in `Report.notes`. *Open:* keep the pair (affirmed-clean vs nothing-in-scope) or collapse to one — default: keep, since `from_returncode(0)` needs `EMPTY` while parsers need `OK`.
- **Cyclopts token form (build-time verify).** Confirm Cyclopts matches the StrEnum by `value` vs `name` for any status-typed CLI parameter before relying on token spelling.
