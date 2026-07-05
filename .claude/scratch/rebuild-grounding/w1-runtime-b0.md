# W1 RUNTIME B0 — GROUNDING DOSSIER (verified primary extracts only)

Batch pages: `reliability/faults.md` [rebuild], `clock/clock.md` [rebuild], `evidence/identity.md` [rebuild], `observability/receipts.md` [rebuild].
Every anchor below is a real `file:line`. Downstream spot-verifies; no paraphrase of doctrine law, no removal framing.

## [00]-[INVENTORIES]

### Doctrine root (`ls docs/stacks/python/`)
```
algorithms.md boundaries.md concurrency.md iteration.md language.md
rails-and-effects.md README.md runtime.md shapes.md surfaces-and-dispatch.md system-apis.md
```
Atlas order (`docs/stacks/python/README.md:11-22`): language, shapes, surfaces-and-dispatch, iteration, rails-and-effects, concurrency, boundaries, algorithms, system-apis, runtime. `iteration.md` is on disk AND in the atlas (row [04]).

### Shared .api tier (`ls libs/python/.api/`)
```
adbc-driver-manager.md anyio.md arro3-core.md beartype.md daft.md expression.md
grpcio-tools.md grpcio.md msgspec.md networkx.md numcodecs.md numpy.md
opentelemetry-api.md opentelemetry-exporter-otlp-proto-http.md opentelemetry-sdk.md
protobuf.md psutil.md pydantic-settings.md pydantic.md stamina.md structlog.md
trio.md universal-pathlib.md xarray.md xxhash.md zlib-ng.md
```

### Folder .api tier (`ls libs/python/runtime/.api/`)
```
apscheduler.md asyncssh.md cyclopts.md fsspec.md google-cloud-secret-manager.md
grpcio-health-checking.md grpcio.md httpx.md keyring.md lbt-recipes.md lz4.md
msgspec.md obstore.md opentelemetry-instrumentation-grpc.md pollination-handlers.md
protobuf.md queenbee.md tree-sitter-python.md tree-sitter-typescript.md tree-sitter.md watchfiles.md
```
Note: `opentelemetry-api.md` is a SHARED-tier catalog only (deleted from runtime tier per brief V13); the four pages cite the branch tier `libs/python/.api/opentelemetry-api.md`.

## [01]-[MEMBER VERIFICATION] (installed source, py3.15, `.venv/lib/python3.15/site-packages`)

### expression 5.6.0 (assay: 47/47 paths present, restore=restored)
- `Block.append` block.py:91 · `Block.choose` :95 · `Block.fold` :157 · `Block.sum_by` :273 · `Block.of_seq` :299 · `Block.partition` :307 · `Block.reduce` :349 · `Block.singleton` :372 · `Block.try_head` :457 · module `map2` :784
- `Option.default_value` option.py:64 · `Option.default_with` :76 · `Option.is_some` :196 · `Option.is_none` :204 · `Option.of_optional` :218
- `Result.map_error` result.py:124 · `Result.bind` :136 · `Result.swap` :204 · `Result.merge` :222 · `Result.to_option` :229
- `effect.result` = `ResultBuilder` re-export: `expression/effect/__init__.py:4` `from .result import ResultBuilder as result`
- `.api` anchors: expression.md:63 `effect.result` builder; :113 `Option.default_with`; :141 `Block.try_head`; :142 `Block.choose`/`Block.partition`; :145 `Map.of_seq`/`Map.empty`; :147 `map[key]`; :148 `Map.try_find`; :168 `@effect.result` generator-coroutine `yield from` protocol.

### structlog (installed)
- `make_filtering_bound_logger` _native.py:83 · async mirrors `adebug/ainfo/awarning/aerror` documented _native.py:126, alias `awarn`/`amsg` :256/:258
- `get_logger` _config.py:114 · `configure` :202
- processors: `JSONRenderer` processors.py:315 · `TimeStamper` :462 · `CallsiteParameterAdder` :819 · `EventRenamer` :949 · `dict_tracebacks = ExceptionRenderer(ExceptionDictTransformer())` :444 (declared __all__ :63) · `add_log_level` present (hasattr True)
- `contextvars.merge_contextvars` contextvars.py:82 · `testing.capture_logs` testing.py:67 / `LogCapture` :37 · `BytesLoggerFactory` present (hasattr True) · `typing.FilteringBoundLogger` typing.py:149
- `.api` anchors: structlog.md:27 `BytesLoggerFactory`; :39 `FilteringBoundLogger`; :57 `await log.ainfo` async-mirror; :78 `EventRenamer(to=)`; :84 `dict_tracebacks`; :131 `make_filtering_bound_logger(min_level)`; :142 `merge_contextvars` first-in-chain; :157 `adebug/ainfo/awarning/aerror` offload; :158 `testing.capture_logs()`; :169 `JSONRenderer(serializer=)` + `BytesLoggerFactory`; :174 msgspec `json.Encoder().encode` serializer row; :180 chain-order law.

### msgspec (installed, __init__.pyi / structs.pyi)
- `Struct` __init__.pyi:89 · `Meta` :162 · `convert` overloads :206/:217 · `structs.replace` structs.pyi:7 · runtime hasattr: `convert`/`Meta`/`UNSET` True, `msgspec.msgpack.Encoder` True, `msgspec.json.Encoder` True, `structs.replace` True

### xxhash (installed)
- `xxh3_128_intdigest`, `xxh3_64_intdigest`, `xxh3_128` all present (hasattr True)

### opentelemetry (installed)
- `trace.get_current_span` True · `trace.format_trace_id` True · `trace.format_span_id` True · `Span.record_exception`/`set_status`/`is_recording` True
- `propagate.extract` True · `propagate.get_global_textmap` True · `context.attach`/`context.detach` True

### psutil (installed)
- `Process.memory_info` True · `NoSuchProcess`/`ZombieProcess`/`AccessDenied` True

### beartype (installed)
- `BeartypeConf` importable, instance has `violation_type` attr True · `beartype.roar.BeartypeCallHintViolation` importable

### anyio (installed)
- `BrokenWorkerProcess`, `BrokenWorkerInterpreter`, `BrokenResourceError`, `ClosedResourceError`, `ConnectionFailed` all present

## [02]-[BRIEF SEAM / RIDER ANCHORS] (`RASM-PY-RUNTIME-BRIEF.md`)

- IMPORT_DAG_LAW order `:39-40`: `faults < clock < identity < receipts < metrics < resilience < admission < telemetry < {reproduction, evidence, roots, shapes} < wire < lanes < recipe < serve`.
- V1 re-homings `:45,71`: `receipts` OWNS `DrainOutcome`/`DRAIN_COLUMNS`/`DrainReceipt` (was lanes); `faults` OWNS `latched` (was telemetry); `reproduction` is a declared module split from identity.
- V7 identity `:97`: `key`/`of` collapse onto one span-fold core (`identity.md:152` derived vs `:211-218` key inline dup); fenced/narrowed `key`; split `SeedReproduction`→`reproduction.md`; canonical name `content_identity`→`identity`; empty `[04]-[RESEARCH]` `identity.md:373` dies.
- V8 `:101`: `latched` lands beside `boundary`/`trapped`/`traversed`; instrumentation-scope vocabulary lands as faults seed data (one table owning `"rasm.wire"` wire.md:39, `METER_NAME` metrics.md:72, `"rasm-companion"` telemetry.md:108, resilience `resilience.md:108`/identity `identity.md:143` tracer strings), each consumer mints its handle from its row; CLASSIFY deadline row stops erasing evidence (`faults.md:130` drops cause message, defaults budget `0.0`) — carry the cause payload; `traversed(PARTITION)` never-`Error` rail keeps/tightens rationale; `FAULT_OUTCOME`+clock `SLOTS` join the `Map` rail.
- V9 `:105`: telemetry `flush` hardcodes subject `"resource"` (`telemetry.md:206`), `_drained` raises `TimeoutError(signal.value)` (`:193`) whose message the deadline classify row drops — thread `subject=kv[0].value`.
- V4 `:85`: identity module name unifies to `rasm.runtime.identity` (drift `content_identity` at `lanes.md:49`, `identity.md:239`).
- Escalation table `:158-166`: faults 9.5→10 (`latched` tenant; scope-vocabulary tenant; deadline row keeps cause evidence; PARTITION rationale; grammar); clock 9→9.5 (`SLOTS`→Map; invariant-once dedupe; RESEARCH purge); identity(+reproduction) 7→9.5 (one span-fold core; fenced/narrowed `key`; module split + canonical name; empty header dies); receipts 8→9.5 (drain vocabulary OWNED here; lanes import dies; grammar).
- E-register: E1 `:133` import SCC (`receipts.md:53` lanes import; taxonomy pivot `lanes.md:60,81,96-102`; `latched` pivot `telemetry.md:222-237`); E7 `:139` dup mechanism (`key` inline span fold `identity.md:211-218` vs `:152`); E9 `:141` hardcoding (per-page tracer/meter literals; drain subject `"resource"` `telemetry.md:206` message dropped `faults.md:130`); E11 `:143` name collision (`identity` vs `content_identity`); E12 `:144` coverage (`key`'s unfenced `Struct` path `identity.md:109-110,190`).
- GENERATOR_LAW `:49`: two plain-`dict` tables `FAULT_OUTCOME` (`metrics.md:81`) and `SLOTS` (`clock.md:163`) sit beside a corpus of `expression.Map` rails.
- Sound-surface preserve list `:150`: `faults.md` (9.5 — gains `latched`, scope vocab, deadline-evidence fidelity), `clock.md` (9 — `SLOTS` Map + grammar only), receipts' chain-placement/redaction machinery.

## [03]-[TARGET-PAGE PRIMARY ANCHORS]

### faults.md (`reliability/faults.md`)
- CLASSIFY deadline row `:130` `(TimeoutError, lambda subject, cause: BoundaryFault(deadline=(subject, 0.0)))` — defaults budget `0.0`, never reads `str(cause)`; `deadline` case payload `tuple[str, float]` `:59` carries `(subject, budget)` only (no cause slot).
- FAULT_CONF declared `:150` `Final[BeartypeConf] = BeartypeConf(violation_type=BeartypeCallHintViolation)` — no `@beartype(conf=FAULT_CONF)` application shown in-page (seam handoff to consumers).
- `railed = effect.result[Any, BoundaryFault]()` `:233` — fully composed.
- `traversed` overloads `:210-213`, PARTITION never-`Error` arm `:224-228`. No `latched`, no `SCOPES`/`Scope` present anywhere in the fence (coverage gap vs V8).
- imports `:28-41`: `inspect`, `functools.wraps`, no `msgspec.structs.replace` (latched reentrant needs it).

### clock.md (`clock/clock.md`)
- `SLOTS: Final[dict[Slot, tuple[str, str]]]` `:163-168` — plain dict (V8/GENERATOR_LAW → `expression.Map`).
- `[03]-[RESEARCH]` tail `:214-218` (ORDERING_VERDICT/GC_FALSE_LEAVES/ATTRIBUTE_PROJECTION) — grammar purge target.
- packed-key law restated across `:16`, `:35`, `:44`, `:50`, `:57-58`, `:162`, `:203` (invariant-once dedupe target).
- `msgspec.convert(mapping, CausalFrame, strict=False)` decode `:186-193`; `Ordering.fold`/`sign`/`reverse` `:93-113`; `Hlc.compare/tick/merge/packed/of_packed` `:125-147` — all complete.

### identity.md (`evidence/identity.md`)
- module name drift: `_TRACER = trace.get_tracer("rasm.runtime.content_identity")` `:143`; consumers import `from rasm.runtime.content_identity import ContentKey` (`lanes.md:49`); §3 prelude self-imports `from rasm.runtime.content_identity import ...` `:239`.
- `derived` span-fold core `:149-155` (`start_as_current_span("content.derive")` + `is_recording` + `set_attributes` + `boundary(...,catch=EncodeError)`).
- `key` re-implements the span fold INLINE `:211-218` (second `start_as_current_span` `:211`, `is_recording` `:212`, `set_attributes` `:213`, `032x` `:216`, `set_status(OK)` `:217`) — E7 duplicate.
- `IdentitySource.lift` `Struct` arm `:109-110` folds through `_ENCODER.encode` raising `EncodeError`; `key` signature narrows to `Buffer | Iterable[bytes] | tuple[ContentKey, ...]` `:199` (excludes Struct) but shares the raising `lift`.
- SeedReproduction owns `[03]-[SEED_REPRODUCTION]` `:221-371` + empty `[04]-[RESEARCH]` header `:373` — the module-split target (to reproduction.py, out of this batch).
- `xxh3_128_intdigest`/`xxh3_64_intdigest`/streaming `xxh3_128` `:124-135`; `msgspec.msgpack.Encoder(order="deterministic")` `:142`.

### receipts.md (`observability/receipts.md`)
- UPWARD import (back-edge) `:53` `from rasm.runtime.lanes import DRAIN_COLUMNS, DrainReceipt` — V1 flips: receipts OWNS the drain vocabulary, imports `ContentKey` downward from identity.
- `drained` case binds `DrainReceipt[object]` at class body `:110`; `project` drained arm reads `getattr(drain, column) for column in DRAIN_COLUMNS` `:131`.
- `Receipt` union + `of`/`project` `:105-133`; `Signals.emit`/`emit_async` over `LEVEL_METHOD` `(sync,async)` rows `:83-88,246-255`; redaction chain-resident `redact` after `merge_contextvars`/`trace_context` `:212-243`; `_ENCODE = Encoder(enc_hook=repr, order="deterministic").encode` `:94`; `_PROCESS = psutil.Process()` `:98`, `_rss` `:172-175`.
- `[03]-[RESEARCH]` tail `:294-299` — grammar purge target.

### drain-vocabulary SOURCE (lanes.md — receipts must absorb; out-of-batch ripple)
- `type DrainOutcome = Literal["accepted","completed","cancelled","rejected","hit"]` `lanes.md:60`; `DRAIN_COLUMNS = get_args(DrainOutcome.__value__)` `:80`; `DrainReceipt[T](Struct, frozen=True)` model + `.of` fold `:95-128` (`values: Block[T]`, `cache: Map[ContentKey, T]`, `faults: Block[BoundaryFault]`, 5 counts).

### latched SOURCE (telemetry.md — faults must absorb; out-of-batch ripple)
- `def latched[R, **P](read, write, reentrant)` aspect `telemetry.md:222-237`; consumed by `metrics.md:41,205` (`from rasm.runtime.telemetry import latched`) and telemetry `:248`. Reentrant closure uses `msgspec.structs.replace`.

## [04]-[FOLDER / GOVERNANCE ANCHORS]

- `ARCHITECTURE.md` codemap nested authoring org: `:27` `identity.py # ContentIdentity/ContentKey...`, `:30` `clock.py # Hlc ... CausalFrame.of`, `:12` `receipts.py`, `:16` `faults.py`.
- Seam ledger: `:36` `evidence/identity ⇄ csharp:Rasm/Spatial/reconciliation [CONTENT_KEY]`; `:37` `evidence/identity ⇄ csharp:Rasm.Element/Projection MATERIAL_LAYER_GOLDEN`; `:38` `clock/clock ← csharp:Rasm.AppHost/Runtime [PORT_RECORDS] Hlc single mint`; `:41` `execution/admission ⇄ ... [PORT] CausalFrame` (E4 double-attributed vs `:38`); `:42` `execution/admission ← ... [WIRE] CredentialPem` (E4 mis-filed → serve); `:43` `observability/receipts → csharp:Rasm.AppHost/Observability [WIRE] W3C trace-context inbound extraction` (V4 re-files to serve); `:53` `evidence/identity → python:data/tabular [CONTENT_KEY]`.
- Sub-domain refusals: `:66` observability, `:69` evidence (path-keyed identity / second hashing owner / cross-setting cache hit), `:70` clock (Python-minted HLC / second Hlc-ElementId-Tenant spelling).
