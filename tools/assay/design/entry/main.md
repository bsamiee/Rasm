# [H1][ASSAY_MAIN]
>**Dictum:** *The command tree is a projection of REGISTRY; one expression maps the matched return to an exit code; the runner already spoke the Envelope.*

## [1][PURPOSE]

`__main__.py` is the entrypoint (stage 12, `ARCHITECTURE.md` §14). It binds `app = build_app(REGISTRY)` (from `composition/registry.py`), attaches the `@app.meta.default` wrapper, attaches the two root-level commands outside the claim tree — `self-test` (D38) and the `auto` group (D37) — and exposes `main(argv)`. It owns **zero** verb, flag, or sub-app declarations: every quality command is a `Bind` row folded by `build_app` (D31), and the entrypoint never builds an `Envelope`, never reads a status string, never performs exit arithmetic. The exit code originates solely from `RailStatus.exit_code` via `Envelope.__cyclopts_returncode__` (D29, D30). `structlog` configuration, the agent-correlation bind (`{run.id, agent.task.id}` into the process-global ContextVar + the span `Resource`, from `ASSAY_RUN_ID`/`ASSAY_AGENT_TASK_ID`), and the optional `beartype` claw live in `__init__.py` (§6, `init.md`), not here; importing `registry` transitively triggers that gate, so by the time `meta` dispatches every log/span/`Envelope` already correlates to its driving agent task with zero CLI flags.

## [2][CANONICAL_SHAPES]

This doc owns the module-level `app` binding, the `meta` collapse, and `main`. `build_app`/`REGISTRY` originate in `composition/registry.py`; `resolve_returncode` is the verified Cyclopts protocol reader imported from the concrete `cyclopts._result_action` (the package re-export is untyped — hence the `# noqa: PLC2701`); `self_test`/automation handlers are referenced, never re-declared here.

```python
app: Final = build_app(REGISTRY)                          # claim tree + root self-test/auto attached inside build_app
```

D30 protocol — `meta` is one expression, not a `match`. The help/version path is **Envelope-free by construction**: under `result_action="return_value"` Cyclopts handles `--help`/`--version` itself — it writes the rendered help/version text to stdout *before* `meta` ever runs and returns `None` (`lib-cyclopts.md:88`, verified `_result_action.py:31-56`), so `meta` receives `None`, `resolve_returncode(None) == 0`, and the process exits `0` having emitted **no** `Envelope`. Only a matched verb returns an `Envelope`. An agent therefore reads the help text directly off stdout and must not parse it as a wire receipt; the one-Envelope invariant (Invariant 1) covers matched rails, never the help/version flags. Because `resolve_returncode(None) == 0` already absorbs this, a defensive `case None` arm is dead ceremony (registry.md §6). The collapse is verbatim D27/D30 — statement form, no `return match`:

```python
@app.meta.default
def meta(*tokens: str) -> int:
    return resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))
```

| [FIELD] | [VALUE] | [LEDGER] |
| ------- | ------- | -------- |
| `result_action` | `"return_value"` | bypasses `sys.exit`; a matched verb returns its `Envelope`, while `--help`/`--version` return `None` (Cyclopts already wrote help/version text to stdout — no `Envelope`) (D30). |
| `backend` | `"asyncio"` | one event loop at the entrypoint; rails own `anyio.run` once inside `run_check` (D33; never nest). |
| meta return | `resolve_returncode(...)` | reads `Envelope.__cyclopts_returncode__()` for verbs; `resolve_returncode(None) == 0` for the help/version path (D30). |

## [3][VALIDATED_SNIPPET]

`meta` collapses to one `resolve_returncode(app(...))` expression — a `return match ... case None: 0; case env: ...` is **invalid Python** and redundant (`resolve_returncode(None) == 0`). Pre-rail parse errors precede `meta` and skip the `Envelope` by construction (§4). `main` wraps dispatch in a `try/finally` so the OTel `BatchSpanProcessor` queue drains before the process returns — `force_flush(5000)` runs in the `finally` (5 s bound, matching the default `schedule_delay`; `force_flush(timeout_millis: int) -> bool`, `sdk/trace/__init__.py:1448`). The API `ProxyTracerProvider` exposes **no** `force_flush` (verified opentelemetry 1.41.1), so the bound method is read off the provider with `getattr(..., lambda _timeout_millis: True)`: when §6's endpoint gate installed the SDK provider the flush drains, otherwise the identity fallback returns immediately — the drain is unconditional and zero-cost when tracing is gated off. `argv is None` forwards `sys.argv[1:]` (so `python -m` dispatches a verb); an explicit `[]` is splayed as-is for root help.

```python
# tools/assay/__main__.py
import sys
from typing import Final, TYPE_CHECKING
from cyclopts._result_action import resolve_returncode  # noqa: PLC2701  # concrete module is typed -> int
from opentelemetry.trace import get_tracer_provider
from tools.assay.composition.registry import build_app, REGISTRY  # noqa: PLC2701  # intra-package

if TYPE_CHECKING:
    from collections.abc import Callable

app: Final = build_app(REGISTRY)                          # self-test + auto attached on root inside build_app

@app.meta.default                                         # the meta app wraps EVERY command result before exit
def meta(*tokens: str) -> int:
    return resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))

def main(argv: list[str] | None = None) -> int:
    flush: Callable[[int], bool] = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)
    try:
        return meta(*(sys.argv[1:] if argv is None else argv))  # None -> sys.argv[1:]; [] -> splayed empty (root help)
    finally:
        flush(5000)                                       # drain the BatchSpanProcessor queue before the process returns

if __name__ == "__main__":
    raise SystemExit(main())
```

## [4][SEAMS]

| [SEAM] | [DIRECTION] | [CONTRACT] |
| ------ | ----------- | ---------- |
| `composition/registry.py` | imports | `REGISTRY: tuple[Bind, ...]`, `build_app(registry) -> App`. `build_app` folds `groupby(claim)` into sub-`App`s, then attaches `self_test` (`name="self-test"`, D38) and the `auto` sub-`App` (D37) to the root. `rail` weaves `checked ▷ logged ▷ traced` over `_narrow(bind.handler)` — the single validated adapter that narrows the erased `object` handler to the 3-arg `Handler` via a `FunctionType` match (no `cast`, banned by TID251), raising `TypeError` on a non-function bind. |
| `cyclopts` | imports | `resolve_returncode(result, default=0)` imported from `cyclopts._result_action` (concrete typed module); `App.meta.default` registers the metacommand wrapping all results; `app(tokens, result_action="return_value", backend="asyncio")` returns `Envelope \| None` (`None` on the `--help`/`--version` path, which Cyclopts renders to stdout itself before `meta` runs). |
| `opentelemetry.trace` | imports | `get_tracer_provider() -> TracerProvider`; `main` drains via `getattr(provider, "force_flush", identity)(5000)` in a `finally` (D50). The §6 install hook (gated on `(endpoint, provider)`) seats the SDK provider at `__init__.py` import; absent that, the accessor returns the API `ProxyTracerProvider` (no `force_flush`), so the identity fallback returns immediately and the drain stays unconditional. |
| `core/engine.py` | indirect | Retry correlation is bound at the engine seam, **not** `logged`: `_spawn` weaves `retried()` (`Spawn`-only) onto `_guarded` then lifts `traced` with `run_id` as a span attribute, so `_on_retry` logs every scheduled retry under the same `run_id` as its spawn. |
| `core/model.py` | indirect | `Envelope.__cyclopts_returncode__(self) -> int: return self.exit_code` (D30); `RailStatus.exit_code` is the single exit source (D29). The entrypoint reads neither directly — `resolve_returncode` does. |
| `__init__.py` | import-order | `structlog.configure(...)`, the `bind_contextvars(**agent_context)` correlation seed + the span-`Resource` agent enrichment, and the optional `ASSAY_CLAW` `beartype` claw run at package import (§6, `init.md`), so importing `registry`/`automation` here already armed the aspect rails *and* the agent correlation (`{run.id, agent.task.id}`) before `meta` dispatches. |
| `automation/engine.py` | indirect | The `auto` sub-`App` (`watch`/`schedule`/`run`) hosts `_watch` (`watchfiles.awatch`) and `_schedule` (`aiocron.crontab`) under **one** `anyio` task group over a shared `anyio.Event` stop constructed before the trigger match: `awatch` honors the stop natively, `_schedule` waits on it then `cron.stop()` + cancels the scope. NDJSON streaming is the documented one-Envelope exception (D37, §9). The entrypoint only mounts the group; it never loops. |

Root-command topology (D37, D38) — attached by `build_app` after the claim fold, beside the six claim sub-`App`s, not inside them:

| [ROOT COMMAND] | [SHAPE] | [LEDGER] |
| -------------- | ------- | -------- |
| `self-test [--rhino]` | leaf `self_test -> Envelope`; failure → `FAULTED`/exit 2 | D38; outside the claim tree (preflight, not a `Claim`). |
| `auto watch \| schedule \| run` | sub-`App` over `automation/`; `Trigger → Action` loop, NDJSON per fire | D37; a first-class arm, **not** a `Claim` — never folded into `REGISTRY`. |

## [5][EXTENSIBILITY]

A new quality verb is one `Bind` row in `REGISTRY` (registry.md §5) — `__main__` is never edited; a new automation trigger is one tagged `Trigger`/`Action` case plus its `auto` leaf, also leaving the entrypoint untouched (Invariant 4).

## [6][CONSIDERATIONS]

- **`meta` must not branch.** D30/registry.md §6 are explicit: `resolve_returncode(None) == 0` covers `--help`/`--version`, so any `match`/`if` arm in `meta` is dead ceremony and a snippet-truth regression. Keep it a single expression; `from_returncode` never yields `OK`, and `meta` never constructs a status — exit codes are read, never computed (D29).
- **`--help`/`--version` emit no Envelope.** Under `result_action="return_value"` Cyclopts intercepts both flags, writes the rendered text to stdout itself *before* `meta` is invoked, and returns `None`; `meta` then maps `None → 0`. The one-Envelope invariant (Invariant 1) is scoped to matched rails — help/version is a sanctioned Envelope-free stdout path beside malformed argv (§4). An agent must read help off stdout as text, never decode it as a wire receipt; only a matched verb yields a JSON `Envelope`.
- **Spans drain before exit.** `main` reads `flush = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)` and calls `flush(5000)` in the `finally` (D50/P1-31): the `traced` slot enqueues into a `BatchSpanProcessor` (`schedule_delay` default 5 s), so without an explicit flush a short-lived CLI invocation would exit before the queue exports and silently lose spans. The 5 s bound caps the drain. When §6's endpoint gate is off the accessor returns the API `ProxyTracerProvider`, which carries no `force_flush`, so the identity fallback makes the flush a zero-cost return rather than a conditional. The drain runs on every path — verb success, `FAILED`/`FAULTED` Envelope, and the Envelope-free help/version exit alike — because it sits in the `finally`, after `resolve_returncode` has already computed the code (exit arithmetic stays read-only, D29).
- **Agent correlation is armed at import, not at dispatch.** The `from … import build_app, REGISTRY` line transitively runs `__init__.py`, which binds `{run.id, agent.task.id}` (from `ASSAY_RUN_ID`/`ASSAY_AGENT_TASK_ID` via the sole `AssaySettings()`) into the process-global structlog ContextVar and the OTel `Resource` (init.md §1-3). `__main__` adds nothing to that path — it neither reads the env nor passes a flag; the `force_flush(5000)` drain in the `finally` simply egresses spans that are *already* resource-correlated. An agent driving a fleet sets the two env vars once and every Envelope, log line, and span across the run carries its task id with no per-command argument — the zero-flag contract is owned by the package marker, surfaced (never re-implemented) here.
- **Pre-rail parse errors are deliberately Envelope-free.** Cyclopts validates argv *before* `meta` dispatches, writing the `CycloptsError` to `error_console` (stderr) and exiting non-zero with no `Envelope`. The one-Envelope invariant (Invariant 1) covers matched rails and automation fires only; alongside the help/version flags, malformed argv is a sanctioned *error* exit without a wire receipt — do not add a `try`/`except` around `app(...)` to synthesize one (the `main` `try/finally` is span-drain plumbing only, never error capture; it has no `except`).
- **`self-test` parity is asymmetric to the registry test.** The contract `names == {b.claim.value for b in REGISTRY}` (cli.py.md §1) enumerates claim sub-`App`s only; `self-test` and `auto` live on the root **beside** them and are absent from `REGISTRY` (D38, D37). Any assertion over the full root command set must add `{"self-test", "auto"}` explicitly, or it falsely fails — they are root siblings, not claim members.
