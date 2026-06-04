# [H1][AOT_OTEL]
>**Dictum:** *Tracing is one env-gated provider installed at import and one `@traced` Layer; the rail tree renders as the trace, and absence of a collector costs a `NonRecordingSpan` and nothing else.*

Verified against `opentelemetry-sdk` 1.42.1 (repo pins `>=1.39`; APIs stable since 1.39): `trace.get_tracer`, `Tracer.start_as_current_span`, `Span.set_attribute`/`set_status`/`record_exception`, `Status`/`StatusCode`, `Resource.create`, `TracerProvider`, `BatchSpanProcessor`, `ParentBased`/`ALWAYS_ON`, and `opentelemetry.exporter.otlp.proto.http.trace_exporter.OTLPSpanExporter`. With no provider set, `trace.get_tracer_provider()` returns the lazy `ProxyTracerProvider` → `NoOpTracer` → `NonRecordingSpan` (no allocation of the SDK span pipeline). `anyio.run` does `contextvars.copy_context()`; AnyIO task groups copy context at `start_soon`/`start` call time — OTel's contextvar-backed current span propagates across both boundaries automatically.

---
## [1][PROVIDER_INSTALL_GATED_AT_IMPORT]
>**Dictum:** *Install the SDK provider once, only when an endpoint is configured; otherwise leave the proxy so every span is a no-op.*

`core/aspect.py` installs at import (the module is itself the imperative shell for the CLI process — there is no other startup hook before the registry builds), but the install is a function gated on settings so the default path sets **no** provider.

```python
_GATE: Final[bool] = bool(os.environ.get("OTEL_EXPORTER_OTLP_ENDPOINT")) and os.environ.get("OTEL_SDK_DISABLED", "").lower() != "true"

def _install() -> None:                                  # idempotent; runs once at import
    match (_GATE, isinstance(trace.get_tracer_provider(), TracerProvider)):
        case (True, False):
            resource = Resource.create({"service.name": "assay", "service.version": _VERSION})
            provider = TracerProvider(resource=resource, sampler=ParentBased(ALWAYS_ON))
            provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))  # endpoint/headers from OTEL_* env
            trace.set_tracer_provider(provider)
            atexit.register(provider.shutdown)           # flush BatchSpanProcessor on CLI exit
        case _:
            ...                                          # gate off OR already installed -> ProxyTracerProvider stays

_install()
_TRACER: Final[trace.Tracer] = trace.get_tracer("assay")  # ProxyTracer upgrades lazily if a provider is set
```

`OTEL_EXPORTER_OTLP_ENDPOINT` *is* the gate (no bespoke flag): unset ⇒ no SDK, `start_as_current_span` returns a `NonRecordingSpan` whose `set_attribute`/`set_status` are empty bodies. `AssaySettings` may expose `otel_endpoint`/`otel_sampler` as typed mirrors of the env for validation, but the env presence is authoritative so a child `dotnet`/`pytest` process inherits the same decision. `set_tracer_provider` is write-once; the `isinstance` arm makes re-import inert. Because a short CLI flushes on exit, `BatchSpanProcessor` + `atexit` shutdown is sufficient (no `SimpleSpanProcessor` needed for the default path).

---
## [2][THE_TRACED_LAYER]
>**Dictum:** *`@traced` is a `Layer` factory: one span per call, attributes projected from `P`, status projected from the `Result`, faults recorded but never raised.*

```python
def traced[**P, T](*, span: str, attrs: Callable[P, Attrs]) -> Layer[P, T]:          # Slot.traced
    def layer(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with _TRACER.start_as_current_span(span) as s:
                for key, val in attrs(*a, **k).items(): s.set_attribute(key, val)      # language, claim, runner, argv head
                ctx = s.get_span_context()
                with bound_contextvars(trace_id=format(ctx.trace_id, "032x"), span_id=format(ctx.span_id, "016x")):
                    res = fn(*a, **k)                                                  # retries happen INSIDE this one span
                match res:
                    case Ok(done):  s.set_attribute("assay.returncode", done.returncode); s.set_status(Status(StatusCode.OK))
                    case Error(f):  s.set_attributes({"assay.status": f.status, "assay.returncode": f.returncode,
                                                      "assay.fault.detail": f.detail[:_ATTR_CAP]}); s.set_status(Status(StatusCode.ERROR, f.status))
                return res                                                             # Error -> ERROR status, returned unchanged
        return woven
    return (Slot.traced, layer)
```

- **Span name** is the caller's literal: rail seam `assay.rail.<claim>.<verb>`, engine seam `assay.check.<tool>` (or `check.id`). It is data passed by the seam, never computed from `fn.__qualname__`, so the trace reads in domain terms.
- **Attributes** come from a `Callable[P, Attrs]` projector the seam supplies (`_rail_attrs`, `_check_attrs`), keyed `assay.rail/verb/run_id/tool/runner/language/claim`. Projecting from `P` keeps `@traced` polymorphic over both seams.
- **Status from `Result`, no raise**: `Ok` ⇒ `StatusCode.OK`; `Error(Fault)` ⇒ fault attributes + `StatusCode.ERROR`, then the **same** `Result` is returned. A domain `Fault` is a value, not an exception, so it is *recorded*, not thrown. A genuine exception escaping `fn` (only the retry-exhaustion re-raise) bubbles through `start_as_current_span`, which auto-calls `record_exception` + sets `ERROR` before re-raising — caught once by the engine's boundary adapter into `Error(Fault)`.
- **Stack position** (`Slot.traced`, outer of `retried`): the `with` span encloses the entire retry loop, so retries are attributes/events on one span, never sibling spans.

---
## [3][PARENT_CHILD_AND_ASYNC_PROPAGATION]
>**Dictum:** *The rail span is the root; each `Check` span is a child; context copying makes the nesting automatic across the loop and the fan-out.*

The rail runner (`composition/registry.py`) composes `traced(span="assay.rail.<claim>.<verb>", attrs=_rail_attrs)` (no `@retried` — a rail is not a transient spawn). Its `start_as_current_span` attaches the rail span to the process contextvar **synchronously**, before the handler runs. The handler calls `Engine.run_check`/`fan_out`, which enter `anyio.run(...)`; that call does `contextvars.copy_context()`, so the rail span is the current span inside the new event loop. Each `Check`'s `traced(span="assay.check.<tool>", ...)` therefore opens **as a child of the rail span** with zero explicit propagation.

```python
async def _run() -> tuple[Result[Completed, Fault], ...]:
    async with anyio.create_task_group() as tg:
        for i, check in enumerate(checks):
            tg.start_soon(_into, slots, i, check, settings)   # context (incl. rail span) copied at THIS call
    return tuple(slots)
```

Fan-out correctness: AnyIO copies the **spawning task's** context at each `start_soon`, so every concurrent `_into` sees the rail span as parent and produces a sibling child span under the one rail — no crossed parents, no manual `context.attach`. Sequential and concurrent rails yield the identical parent→child shape; concurrency is invisible to the trace topology.

---
## [4][INTERACTION_WITH_RETRIED_AND_LOGGED]
>**Dictum:** *One span encloses all attempts; trace ids ride the structlog contextvars so every nested line correlates.*

- **`@retried` (Slot.retried, inner of traced):** `stamina.retry` re-executes only the spawn; because it sits inside the span `with`, all attempts share one span. The attempt count surfaces as `s.set_attribute("retry.attempts", n)` (a `stamina` hook increments a closure counter written before the status match) — N attempts, one span, never N spans. Stack regression (`retried` outside `traced`) is structurally impossible: `compose` sorts by `Slot`.
- **`@logged` (Slot.logged, outer of traced):** binds `rail/verb/run_id` via `bound_contextvars` *before* the span opens, so the span and every nested log inherit them. `@traced` then binds `trace_id`/`span_id` (§2) for the span's dynamic extent, so engine per-`Check` logs and child-span logs emit fully correlated through `merge_contextvars` (first in the structlog chain) — no per-event `get_current_span()` lookup required. The trade-off: the outer `@logged`'s terminal rail-completion event fires after the span closes and carries only `rail/verb/run_id` + the rail `trace_id` (bound at the rail span, still live in the outer scope), which is the correct correlation key for the whole trace; the authoritative record remains the single stdout `Envelope`.

---
## [5][NEW_OP_IS_A_TRACED_APPLICATION]
>**Dictum:** *Instrumenting a new operation is adding `traced(...)` to a seam's `compose`, never writing span code.*

A newly instrumented surface (e.g. a future `metered` exporter, a watch-cycle, or a new exclusive primitive) gains a span by appearing in one of the two `compose(...)` calls with a span name and an `attrs` projector — exactly as `static`/`test`/`bridge` rails do today. Inline `with tracer.start_as_current_span(...)` inside a rail body, handler, or `Tool` row is prohibited (`ARCHITECTURE.md` §9: aspects attach at two seams only). Adding a *child level* (e.g. spanning sub-steps within one `Check`) is a new `traced` Layer at a new compose site, not a hand-rolled span — the rail tree stays the trace tree by construction.

---
## [6][VERDICT_AND_OPEN_DECISIONS]
>**Dictum:** *Adopt decorator spans over an env-gated provider; settle sampling, endpoint config, and event-vs-attribute granularity.*

**Verdict — ADOPT.** Decorator-based spans (`@traced` as a `Slot.traced` `Layer`) over an env-gated `ProxyTracerProvider`-default provider is the correct AOT shape: the rail tree models as a trace with no inline span code, faults set `ERROR` without breaking the `Result` rail, retries collapse to one span, and the default no-collector path pays only `NonRecordingSpan` calls (no SDK, no exporter, no thread). It composes cleanly with `@logged`/`@retried` under the existing `compose` slot order and propagates correctly across `anyio.run` and task-group fan-out via context copying — verified, not assumed.

**Open decisions.** (1) **Sampling** — default `ParentBased(ALWAYS_ON)` (short CLI, complete traces); expose `AssaySettings.otel_sampler` to drop to `TraceIdRatioBased`/`OTEL_TRACES_SAMPLER` only if a CI fleet floods the collector. (2) **Exporter endpoint/protocol** — http OTLP via `OTEL_EXPORTER_OTLP_ENDPOINT`/`_HEADERS`; decide whether to also honor gRPC (`opentelemetry-exporter-otlp-proto-grpc`) or pin http-only to match the declared dependency, and whether per-process children should propagate `traceparent` so `dotnet`/`pytest` spans nest under the assay rail (W3C `TraceContextTextMapPropagator.inject` into the child env). (3) **Span event vs attribute granularity** — attributes for terminal facts (`returncode`, `status`, `retry.attempts`); decide whether per-attempt retries and stream-tee milestones become `add_event` timeline entries (richer waterfall, higher export volume) or stay folded attributes (default), and cap `assay.fault.detail` length (`_ATTR_CAP`) to bound payload size.
