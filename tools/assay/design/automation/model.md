# [H1][AUTOMATION_MODEL]
>**Dictum:** *A trigger is data and an action is data; a third of either is one tagged row, never a new module.*

## [1][PURPOSE]

Owns the two `msgspec`-tagged unions that close the automation arm (§9, D37): `Trigger = Watch | Schedule | Manual` (what re-fires) and `Action = Rail | Program | Sequence | Debounce` (what each fire runs). Automation is a first-class arm, **not** a `Claim` — these shapes live under `automation/`, share the Engine, leases, settings, and the sole `_emit` writer, but sit outside the six quality claims and the `Detail` tagged base. They are inert data decoded once at the loop boundary; `automation/engine.py` (the neighbor) interprets them via a `match` dispatch under one `anyio` task group, emitting one `Envelope` per fire as NDJSON — the documented exception to the one-Envelope invariant (§17.1).

## [2][CANONICAL_SHAPES]

Both unions reuse the canonical `Base` config (`frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True`, declared once in `core/model.py` per §5) and add the `Detail`-style discriminator (`tag_field="kind"`, explicit short tag, `forbid_unknown_fields=True`) so a one-pass `Decoder` recovers the case and a drifting emitter fails loudly (§17.5, D13). Tags are **explicit short strings**, never `str.lower(classname)`. `cron` reuses an `aiocron`/`cronsim`-parseable spec string; `paths`/`argv`/`actions` are tuples (immutable, hashable under `frozen`). `Action` nests recursively in two ways: `Sequence.actions: tuple[Action, ...]` is a fan-out row and `Debounce.action: Action` is a single-wrap row, so a fold over actions is one `match`, not a hierarchy. Two module-scope `frozenset[str]` constants — `_TRIGGER_TAGS = {"watch","schedule","manual"}` and `_ACTION_TAGS = {"rail","program","sequence","debounce"}` — pin each union's full short-tag vocabulary in lockstep with the alias, so a drifting case is a static set/union mismatch rather than a silent decode gap.

| [UNION] | [CASE] | [TAG] | [FIELDS] | [LIBRARY SEAM] |
| ------- | ------ | :---: | -------- | -------------- |
| `Trigger` | `Watch` | `watch` | `paths: tuple[str, ...]`, `filter: str = "default"`, `ignore_patterns: tuple[str, ...] = ()`, `debounce: int = 1600`, `cpu_threshold: float \| None = None` | `_watch`: `awatch(*paths, watch_filter=_FILTERS.get(filter, DefaultFilter()), debounce=, stop_event=stop)` |
| `Trigger` | `Schedule` | `schedule` | `cron: str`, `cpu_threshold: float \| None = None` | `_schedule`: `aiocron.crontab(cron, func=fire, start=False)` + shared-`stop` waiter → `cron.stop()` |
| `Trigger` | `Manual` | `manual` | *(none)* | immediate single `await fire()` |
| `Action` | `Rail` | `rail` | `claim: Claim`, `verb: str`, `params: Raw = msgspec.Raw()` | `composition/registry.rail(bind)` |
| `Action` | `Program` | `program` | `argv: tuple[str, ...]` | `core/engine.run_check` (DIRECT row, `Input.NONE`) |
| `Action` | `Sequence` | `sequence` | `actions: tuple["Action", ...]` | recursive `_sequence` `join`-fold |
| `Action` | `Debounce` | `debounce` | `action: "Action"`, `window_ms: int = 500`, `collapse: bool = True` | `_emit_leaf` unwrap → engine quiescence-timer over the wrapped `action` |

`Rail.params` is `msgspec.Raw` (zero-copy passthrough) so an action carries an opaque per-verb `Params` payload without `automation/model.py` importing every rail's frozen `@dataclass`; the registry decodes it at dispatch against the `Bind.params` type. `Watch.filter` is a vocabulary tag (`"default"` → `DefaultFilter()`, `"python"` → `PythonFilter()`) resolved in `engine`, keeping the wire shape a string and the `BaseFilter` choice in the interpreter — `model.py` declares no `watchfiles`/`aiocron` import (catalog-row discipline; the spine never imports a trigger backend per the watchfiles seam note). `Watch.ignore_patterns: tuple[str, ...] = ()` extends that base tag with domain-specific rejection globs (vendor dirs, build artifacts): the interpreter folds them into the resolved `BaseFilter` (e.g. composing the base `__call__` with a glob reject), so the wire stays inert data and no `watchfiles` subclass leaks onto it. `Debounce(action, window_ms=500, collapse=True)` WRAPS an existing `Action` — it owns no execution engine of its own: `engine` schedules a per-action quiescence timer that resets on each trigger and fires the inner `action`'s existing seam (`_emit_leaf` unwraps the wrapper, recursing through nested wraps) only after `window_ms` elapses with no new trigger. `collapse=True` coalesces a storm to a single trailing fire; `collapse=False` keeps the leading fire and suppresses only the trailing tail. `cpu_threshold: float | None = None` (D49) is the optional fleet-governor gate carried as inert data on both `Watch` and `Schedule`: when set, `engine._governed` reads `psutil.cpu_percent(0.1)` and trips when it is `>= cpu_threshold*100` (so a fractional ceiling `0.85` gates at 85% measured utilization), emitting a `Completed{status=SKIP}` and eliding the fire; `None` disables the gate (`Manual` is always ungoverned). The threshold is a bare float on the wire — `model.py` imports no `psutil`, just as it imports no trigger backend.

## [3][VALIDATED_SNIPPET]

```python
from msgspec import json, Raw
from assay.core.model import Base, Claim

# --- [MODELS] -------------------------------------------------------------------------------
class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    claim: Claim
    verb: str
    params: Raw = Raw()  # zero-copy; registry decodes late against Bind.params

class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    argv: tuple[str, ...]

class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    actions: tuple["Action", ...]  # noqa: UP037 — load-bearing forward-ref string; Action is declared below, __future__ is forbidden

class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    action: "Action"  # noqa: UP037 — load-bearing forward-ref string, same codec-ordering rule as Sequence
    window_ms: int = 500
    collapse: bool = True

# --- [TYPES] --------------------------------------------------------------------------------
type Action = Rail | Program | Sequence | Debounce

# --- [CONSTANTS] ----------------------------------------------------------------------------
_DECODE_ACTION: json.Decoder[Action] = json.Decoder(Action)  # built AFTER the alias so no forward-ref string reaches msgspec early
_ENCODE = json.Encoder(order="deterministic")
_TRIGGER_TAGS: frozenset[str] = frozenset({"watch", "schedule", "manual"})  # exhaustiveness: every case tag in lockstep with its union
_ACTION_TAGS: frozenset[str] = frozenset({"rail", "program", "sequence", "debounce"})

# --- [OPERATIONS] ---------------------------------------------------------------------------
def describe(node: Trigger | Action) -> str:  # one polymorphic match, statement form
    match node:
        case Rail(claim=c, verb=v):
            return f"rail[{c.value}:{v}]"
        case Program(argv=a):
            return f"program[{a[0] if a else ''}]"
        case Sequence(actions=acts):
            return "seq[" + " > ".join(describe(a) for a in acts) + "]"
        case Debounce(action=inner, window_ms=w):
            return f"debounce[{describe(inner)} @ {w}ms]"
        # ... Watch | Schedule | Manual arms elided

# round-trip proof: explicit short tag recovers the case in one pass
_blob = _ENCODE.encode(Sequence(actions=(Rail(claim=Claim.STATIC, verb="report"),
                                         Program(argv=("ruff", "check")))))
# => b'{"kind":"sequence","actions":[{"kind":"rail",...},{"kind":"program",...}]}'
match _DECODE_ACTION.decode(_blob):
    case Sequence(actions=acts):
        assert describe(acts[0]) == "rail[static:report]"
```

`Base` carries `frozen`/`gc`/`omit_defaults`/`repr_omit_defaults` from `core/model.py`; each case adds only the discriminator triple (`tag_field`/`tag`/`forbid_unknown_fields`). The recursive `Sequence.actions: tuple["Action", ...]` is a **string** forward-ref — the spine forbids `from __future__ import annotations`, so the string is load-bearing and the `Decoder` must instantiate **after** the `type` aliases, so no forward-ref string reaches `msgspec` (codec-ordering gotcha). `describe` is the one polymorphic surface and uses statement-form `match` throughout (no `return match`).

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| ---------- | ----------- | ---------- |
| `automation/engine.py` | consumes | `drive` is the single public surface: it `match`-dispatches `Trigger` → `_watch` (`awatch`) / `_schedule` (`aiocron.crontab`) / immediate `Manual` under one `anyio` task group sharing **one** `anyio.Event` stop constructed before the match. `awatch` honors `stop_event` natively; `_schedule` waits on the shared `stop` then `cron.stop()` + `cancel_scope.cancel()` (aiocron exposes no native `stop_event`). It `match`-folds `Action` → `run_check` (`Program`/`Sequence`) or `registry.rail` (`Rail`); `_emit`s one `Envelope` per fire (the registry re-emits its own line for a `Rail`). Owns the fixed `Sequence` short-circuit policy (`RailStatus.join` max-severity — `FAILED` halts exit 1, any `Fault` dominates+halts, `SKIP`/`EMPTY`/`OK` continue), the per-drive `CapacityLimiter(1)` (`async with` in `_emit_leaf`) so a slow fire never re-enters a leased `Action` into spurious `BUSY`, and the `cpu_threshold` governor against `psutil.cpu_percent(0.1) >= threshold*100`. The model carries `cpu_threshold` data only; limiter and governor are engine state, never wire fields. |
| `composition/registry.py` | consumes | `Rail(claim, verb, params)` resolves to a `Bind(claim, verb, handler, Params, doc)` row in the 39-verb `REGISTRY`; `params: Raw` decodes against `Bind.params` at dispatch. `rail(bind)` weaves `checked ▷ logged ▷ traced` once over `_narrow(bind.handler)` (no `@retried` — a rail is a `Hom`, not a `Spawn`) and reuses the sole `_emit` writer (no second stdout writer). An unbound verb or malformed `params` payload folds to a `FAULTED` `Fault` at the fire boundary. |
| `core/engine.py` | consumes | `Program.argv`/`Sequence` leaf programs bind to a DIRECT-runner `Check` and fold through `run_check`, which owns the `checked ▷ traced ▷ retried` seam; `@retried` never retries `BUSY` inside a fire. Retry correlation is bound in **`traced`** (the engine span seam), not `logged`: `run_id` correlates the parent rail span with each `run_check` child span via `traced(attrs=_correlate)`. |
| `core/model.py` | imports | `Base`, `Claim` (and, in `engine`, `Completed`/`Fault`/`RailStatus`/`fold`/`envelope`) — automation never redefines the status algebra or the `Detail` base. |
| `core/status.py` | imports (transitive) | each fire's outcome folds via `RailStatus.join`; `Envelope.exit_code == status.exit_code` holds per emitted line; `Fault{argv,status=FAULTED,message}` only on spawn/lease/timeout/unbound-verb. |

## [5][EXTENSIBILITY]

A fourth trigger (e.g. an inotify-debounced `Webhook`) or a fourth action (e.g. `Parallel(actions)`) is **one** `Base`-derived case with an explicit short `tag` plus one `match` arm in `engine`; no rail signature, no new module, no `Decoder` change beyond the union alias (D37).

## [6][CONSIDERATIONS]

- `Rail.params: Raw` defers per-verb validation to the registry's `Bind.params` type, so `automation/model.py` stays import-free of every rail's `Params` dataclass — but the `Raw` bytes are only validated at dispatch, not decode; an invalid params payload surfaces as a `FAULTED` `Fault` at the fire boundary, not at trigger-config decode. Document this asymmetry so config authors do not expect early rejection.
- `Sequence` is the natural fold seam but carries no short-circuit policy of its own: the policy is **fixed and owned by `automation/engine.py`** (D48), not the data. The engine folds leaves by `RailStatus.join` (max-severity): a `FAILED` leaf halts the sequence (exit 1); any `Fault` leaf (`BUSY`/`TIMEOUT`/`FAULTED`) dominates and halts; `SKIP`/`EMPTY` continue. The recursive `match` over `tuple[Action, ...]` is the fold vehicle; the data stays inert and the divergence on exit code is settled, not deferred.
- `Watch.filter` stays a base-vocabulary string tag (resolved to `BaseFilter` in `engine`); `Watch.ignore_patterns: tuple[str, ...]` carries the domain-specific rejection layer (vendor dirs, build artifacts) as inert globs the interpreter folds into that base filter. This keeps both the base choice and the reject set as wire data — neither leaks a `watchfiles` `BaseFilter` subclass onto the wire. If the base vocabulary itself outgrows `"default"`/`"python"`, promote `filter` to a small `StrEnum` rather than widening the string; the patterns extension already absorbs per-config rejection so the tag set can stay narrow.
- `Debounce` is the coalescing wrapper, deliberately distinct from `Watch.debounce` (the Rust-layer batching window inside one `awatch` batch): `Watch.debounce` collapses one filesystem batch before yield, while `Debounce.window_ms` collapses repeated *fires* of any `Action` (cron ticks, manual storms, watch batches) at the engine's per-action quiescence timer. The wrapper adds no parallel execution engine — it reuses `_emit_leaf`'s existing leaf seam — so a `Debounce(Sequence(...))` debounces the whole fold as one unit. `collapse` is the only policy knob; the timer cadence and single-flight `CapacityLimiter(1)` remain engine-owned (D49), never wire fields.
