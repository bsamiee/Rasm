# W3 RUNTIME B1 — GROUNDING DOSSIER

Batch: `execution/recipe.md` [improve] + `transport/serve.md` [rebuild]. Verified primary extracts only; every claim carries a `file:line` or a live-introspection anchor. Downstream stages spot-verify these anchors.

## [A] REAL INVENTORIES (ls)

### Doctrine root — `docs/stacks/python/` (README atlas order)
```
README.md language.md shapes.md surfaces-and-dispatch.md iteration.md
rails-and-effects.md concurrency.md boundaries.md algorithms.md system-apis.md runtime.md
```
Atlas `[01]` order (`docs/stacks/python/README.md:11-22`): language, shapes, surfaces-and-dispatch, iteration, rails-and-effects, concurrency, boundaries, algorithms, system-apis, runtime. All eleven read in full. `iteration.md` present on disk AND in atlas row `[04]`.

### Shared tier — `libs/python/.api/` (27 catalogs)
```
adbc-driver-manager anyio arro3-core beartype daft expression grpcio-tools grpcio
msgspec networkx numcodecs numpy opentelemetry-api opentelemetry-exporter-otlp-proto-http
opentelemetry-sdk protobuf psutil pydantic-settings pydantic stamina structlog trio
universal-pathlib xarray xxhash zlib-ng
```

### Folder tier — `libs/python/runtime/.api/` (21 catalogs)
```
apscheduler asyncssh cyclopts fsspec google-cloud-secret-manager grpcio-health-checking
grpcio httpx keyring lbt-recipes lz4 msgspec obstore opentelemetry-instrumentation-grpc
pollination-handlers protobuf queenbee tree-sitter-python tree-sitter-typescript
tree-sitter watchfiles
```
Folder tier `msgspec.md`/`grpcio.md`/`protobuf.md` are runtime-delta OVERLAYS over the shared tier (brief `[V13]`, line 121).

### Runtime planning tree — the two targets and their siblings
```
clock/clock.md
evidence/{evidence,identity,reproduction}.md
execution/{admission,lanes,recipe}.md        # recipe = TARGET (improve)
observability/{metrics,receipts,telemetry}.md
reliability/{faults,resilience}.md
transport/{roots,serve,wire}.md              # serve = TARGET (rebuild)
```
`runtime/{ARCHITECTURE,README,IDEAS,TASKLOG}.md` at folder root. Brief at repo root `RASM-PY-RUNTIME-BRIEF.md` (217 lines).

## [B] EDITORCONFIG (`.editorconfig:16,24`)
`[*.py]` block present; `dotnet_style_namespace_match_folder = true:error` is a `.cs` rule, not applied to `.py` — no error-level analyzer contradicts these Python fences.

## [C] LIVE MEMBER VERIFICATION (`uv run --frozen python` introspection; all packages installed)
Installed: cyclopts 4.19.0, lbt_recipes, queenbee, pollination_handlers, google.cloud.secretmanager 2.29.0, grpc(io) 1.81.1, msgspec 0.21.1, expression 5.6.0.

### SERVE — grpc.aio surface (all TRUE)
- `grpc.local_server_credentials`, `grpc.LocalConnectionType` = `['UDS','LOCAL_TCP']`, `grpc.Compression.Gzip`.
- `grpc.aio.server(migration_thread_pool, handlers, interceptors, options, maximum_concurrent_rpcs, compression)` — interceptors + compression kwargs real.
- `grpc.aio.insecure_channel`, `grpc.aio.AioRpcError` (`.code()`/`.details()`/`.trailing_metadata()` all TRUE).
- `grpc.aio.Server`: `add_secure_port`, `add_insecure_port`, `start`, `wait_for_termination`, `stop`, **`add_generic_rpc_handlers`**, **`add_registered_method_handlers`** — all TRUE.
- `grpc.aio.ServicerContext`: `invocation_metadata`, `abort(code, details='', trailing_metadata=()) -> NoReturn`, **`set_trailing_metadata`**, **`time_remaining`** — all TRUE.
- `grpc.aio.Channel`: `unary_unary`, `close` — TRUE.
- `grpc.unary_unary_rpc_method_handler`, `grpc.method_handlers_generic_handler` — TRUE.
- **`abort` returns `NoReturn`** — it raises to terminate; `serve.md:138 return b""` after `await servicer_context.abort(...)` is dead code and the `-> bytes` on the Error arm is a fiction.
- `grpc_health.v1.health.HealthServicer` (+ `health.aio`) — TRUE (grpcio-health-checking installed).

### SERVE — otel-instrumentation-grpc
- `aio_server_interceptor(tracer_provider=None, filter_=None)` — takes NO hook params (confirms server enrichment is `set_attributes`, not hooks).
- `aio_client_interceptors(tracer_provider=None, filter_=None, request_hook=None, response_hook=None)`.
- `filters.negate`, `filters.health_check` — TRUE.
- Client UNARY `response_hook(span, details)` payload IS the status-details string (`.venv/.../opentelemetry/instrumentation/grpc/_aio_client.py:38` `response_hook(span, details)` where `details = await call.details()`, `_aio_client.py:104`), `""` on OK. **serve.md:291-292 response_hook(span, details: str) claim is CORRECT — not a defect.** Stream path uses `_call_response_hook(span, response)` (`_aio_client.py:121`).

### SERVE — cyclopts / msgspec
- `cyclopts.App(... result_action=)`, `cyclopts.Parameter(env_var=)`, `cyclopts.types.NonNegativeFloat` — all TRUE.
- `msgspec.Raw`, `msgspec.to_builtins(..., str_keys=True)`, `msgspec.convert(..., strict=)`, `msgspec.json.Decoder` — all TRUE.

### RECIPE — lbt_recipes (AGPL, installed)
- `Recipe.__init__(self, recipe_name)`.
- `Recipe.run(self, settings=None, radiance_check=False, openstudio_check=False, energyplus_check=False, queenbee_path=None, silent=False, debug_folder=None)` — the three `*_check` kwargs exist but recipe.md deliberately front-loads them via `guarded_sync(RetryClass.ENGINE)` (recipe.md:254) and calls `run(settings=..., silent=True)` (recipe.md:272).
- `Recipe.write_inputs_json(self, project_folder=None, indent=4, cpu_count=None) -> str`.
- `Recipe.output_value_by_name(self, output_name, project_folder=None)`.
- `Recipe.luigi_execution_summary(self, project_folder=None)`, `Recipe.failure_message(self, project_folder=None)`, **`Recipe.error_summary`** (TRUE — admitted at recipe.md:17 Packages but composed by NO fence body).
- `Recipe.input_value_by_name`, attrs `simulation_id`/`name`/`tag`/`path` — all TRUE.
- `RecipeSettings(self, folder=None, workers=None, reload_old=False, report_out=False, debug_folder=None)` — `reload_old`/`report_out`/`debug_folder` columns exist; recipe.md uses only `workers`.
- `version.check_radiance_date` / `check_openstudio_version` / `check_energyplus_version` — all TRUE.

### RECIPE — queenbee (installed)
- `queenbee.recipe.recipe.BakedRecipe.from_folder`, `RecipeInterface.from_recipe` — TRUE.
- `queenbee.job.Job`, `queenbee.io.inputs.job.{JobArgument,JobPathArgument}` — TRUE.

## [D] .API CATALOG ANCHORS (folder tier, cited members)

### `runtime/.api/lbt-recipes.md`
- `:75` — `Recipe.luigi_execution_summary(...)` / `Recipe.error_summary(...)` / `Recipe.failure_message(...)` "read log" row (error_summary is catalogued but page-unused).
- `:86` — `RecipeSettings(folder=None, workers=None, reload_old=False, report_out=False, debug_folder=None)`.
- `:88` — `RecipeSettings.workers/.folder/.reload_old/.report_out/.debug_folder` accessors.
- `:89` — `version.check_radiance_date()/check_openstudio_version()/check_energyplus_version()`.
- `:72` — `write_inputs_json(project_folder=None, indent=4, cpu_count=None) -> str`; `:74` — `output_value_by_name`; `:96` — result law (product is the typed output, never the raw project folder).

### `runtime/.api/queenbee.md`
- `:50-51` — `BakedRecipe` (flow owner, dependencies inlined) / `RecipeInterface` "lean metadata/source/inputs/**outputs** projection".
- `:144` — `BakedRecipe.from_folder(folder_path, refresh_deps=True, config=Config())`; `:146` — `RecipeInterface.from_recipe(recipe, source=None)`.
- `:90` — `JobArgument/JobPathArgument`; `:98` — `Job` submission shape.

### `runtime/.api/grpcio.md`
- `:15` — serve leg drives `grpc.aio.server(interceptors=[...])` through `add_secure_port/start/stop(grace)` (does NOT document the handler-registration factories — catalog gap parallel to the page gap).
- `:18` — `ServicerContext.abort(code, details)`/`set_code`/`set_details` compose directly.
- `:21` — `local_server_credentials(grpc.LocalConnectionType.UDS)` in-host UDS socket-locality auth.
- `:24` — `AioRpcError.code()` (a `grpc.StatusCode`) / `.details()` / `.trailing_metadata()` lift at the channel boundary (the V14 client-ingress source).

### `runtime/.api/opentelemetry-instrumentation-grpc.md`
- `:70` — `aio_client_interceptors(tracer_provider=None, filter_=None, request_hook=None, response_hook=None)`.
- `:71` — `aio_server_interceptor(tracer_provider=None, filter_=None)` (no hook params).
- `:84` — `filters.health_check() -> Condition`; `:87` — `filters.negate(condition)`.
- `:96` — hook law: `request_hook`/`response_hook` enrich CLIENT spans only, non-blocking, inside the active span scope.

### `runtime/.api/cyclopts.md`
- `:3` — backs runtime's `Entrypoint` owner, cites `serve.md:328`.
- `:24` — `App` result_action owner; `:25` — `Parameter` binds `env_var`; `:27` — `types.NonNegativeFloat` the grace bound; `:46` — `App(name, *, help, result_action)`.

## [E] BRIEF SEAM/RIDER ANCHORS (`RASM-PY-RUNTIME-BRIEF.md`)

### RECIPE (target)
- `[V3]` line 79-81 — RULED default AUTHOR; recipe stands on disk. Line 81 is the load-bearing rider: **"The page consumes two ruled-but-unbuilt spellings later legs bind or sweep: the `[V8]` faults scope table as `from rasm.runtime.faults import SCOPES, Scope` (`SCOPES[Scope.RECIPE]`) and the `[V5]` offload isolation axis as `lane.offload(..., modality=Modality.THREAD)` with `Modality` exported from `lanes`."** → recipe's `Modality`/`modality=Modality.THREAD` (recipe.md:36,170,196,219) is a brief-sanctioned FORWARD REFERENCE, resolved by the V5 lanes buildout, not a defect to strip.
- SCOPES half is LANDED: `faults.md:68-76` `Scope` StrEnum includes `RECIPE = "recipe"`; `faults.md:172-180` `SCOPES` includes `(Scope.RECIPE, "rasm.runtime.recipe")`. recipe.md:218 `SCOPES[Scope.RECIPE]` resolves.
- Modality half is OPEN: `lanes.md:139` `offload[T](self, kernel, *args, retry: RetryClass | None = None)` has NO `modality` param and hardcodes `interpreter_run_sync` (`lanes.md:37,144`); `lanes.md` exports NO `Modality`. Recipe needs THREAD offload (subprocess wait + handler-tree file copies are blocking I/O, not isolate-CPU).
- `[V5]` line 89 — "`offload` gains the isolation-modality axis (interpreter | process | thread) with `WORKER_BAND` as the process-modality's one process-wide native-worker bound"; `WORKER_BAND` at `lanes.md:88` today single-occurrence, unused.
- `[E3]`/`[03]` line 165 — recipe target 9.5: "NEW owner composing lanes/admission/resilience/roots/receipts … typed `output_value_by_name` readback + parsed-log `RecipeReceipt`; engine-gate rows; handler catalog as data." `RetryClass.ENGINE` row LANDED (`resilience.md:275`).

### SERVE (target, rebuild)
- `[V6]` lines 91-93 — the serve context/composition mandate: interceptor owns extraction + SERVER parenting; `attach(remote-parent)` dies; `run()`'s per-call `rasm.descriptor` threads through the client `request_hook`; **health servicer** (grpcio-health-checking) lands in `ServerHost` lifecycle or the filter claim dies; **`ServicerContext.time_remaining()` lifts into `RuntimeContext.Deadline`** (E16); the **`dispatch` registration surface (descriptor id + codec pair + railed handler)** is a sibling-composable export; the `Entrypoint` is the daemon COMPOSITION ROOT folding install→admit→serve→drain through `traversed(ACCUMULATE)` over five drainable owners.
- `[V14]`/`[E15]` lines 125,147 — FaultDetail round-trip: server egress `settle` Error arm projects `BoundaryFault -> FaultDetail` (off `facts()` + inbound frame hlc/tenant), encodes through the `fault_detail` registry row, sets `grpc-status-details-bin` via `ServicerContext.set_trailing_metadata` BEFORE `abort`; the current `":".join(k=v)` string (`serve.md:137`) demotes to human-readable `details`. Client ingress lifts `AioRpcError.trailing_metadata()` and decodes the `fault_detail` row. The `wire` transient-status predicate half is LANDED (`resilience.md:168-178` `_wire_transient`, `resilience.md:270` WIRE row `target=_wire_transient(ConnectionError)`).
- `[V12]` line 117 — serve's five-row `Credential` union (`serve.md:70`) renames `CredentialPolicy` (the C#-minted axis name it decodes, `serve.md:6,21`); admission's local struct already renamed `BasicCredential` (LANDED `admission.md:283`).
- `[E16]` line 148 — `ServicerContext.time_remaining()` read by zero fences; `inbound` (`serve.md:117-122`) admits no budget.
- `[E14]` line 146 — composition root absent; `companion_app` binds only serve/drain (`serve.md:344-352`); five drainable owners with no ordering owner (`serve.md:188-189,321-323`, `telemetry` flush, lanes `_events` finally, roots `drain`).
- `[03]` — serve not in the escalation table by name; it is the `[V6]` transport/edge leg. Sound-surface note line 150: `wire`'s transcode core + `CrdtArm` decode preserved (serve composes `WireProtoCodec` unchanged).

## [F] FOLDER-CONTEXT ANCHORS (sibling owners the targets compose)

### Seams composed by RECIPE (verified in the sibling fences)
- `lanes.md:110-137` `LanePolicy.drain(units: Block[Admit[T]], cache)` → `DrainReceipt[T]`; `lanes.md:62-67` `Admit[T]` (`bare`/`keyed`/`retried`); `lanes.md:139-157` `offload` (interpreter-only, no modality — the gap).
- `admission.md:163-169` `Deadline.seconds`; `admission.md:19` `LanePolicy.deadline` reads `Deadline.seconds`; recipe's `RecipeExecution.lane: LanePolicy` (recipe.md:163) carries the caller's budget projection.
- `resilience.md:239-241` `guarded_sync(cls, fn, *args, subject, **kwargs)`; `resilience.md:275` `RetryClass.ENGINE` row `Policy(attempts=2, timeout=10.0, target=(OSError, TimeoutError))`.
- `roots.md:181-188` `ResourceRoot.read(ref, modality) -> RuntimeRail[Acquired]`; `roots.md:85-88` `ReadModality.WHOLE/STREAM`; `roots.md:172-179` `ResourceRoot.child`.
- `identity.md:211-226` `ContentIdentity.key(fmt, source, ...)` bare infallible accessor (recipe.md:263 keys handled-inputs bytes — correct: buffer source, not Struct).
- `faults.md:205-217` `boundary`/`async_boundary`; `faults.md:82-91` `BoundaryFault` (recipe uses `resource` case, `serve` projects all tags); `faults.md:119-138` `BoundaryFault.facts()` slot map.
- `receipts.md:158-181` `Receipt.of`/`project`; `receipts.md:311-331` `receipted[R: ReceiptContributor](redaction)` aspect; `receipts.md:212` `OPEN`; `receipts.md:183-185` `ReceiptContributor` port (recipe's `RecipeReceipt.contribute`/`RecipeProduct.contribute` implement it).

### Seams composed by SERVE (verified in the sibling fences)
- `wire.md:138-159` `WireProtoCodec[S, M].encode/.decode`; `wire.md:183-185` `WIRE_REGISTRY`; `wire.md:190-191` `codec(name) -> RuntimeRail[WireProtoCodec]` (the per-descriptor resolution CapabilityInvoke does NOT use — it holds one fixed `self._encode`/`self._decode`, serve.md:278); `wire.md:180` `FaultDetail` registry row exists ("fault_detail", FaultDetail, channels_pb2.FaultDetail).
- `clock.md:166-180` `CausalFrame.decode(carrier) -> RuntimeRail[Self]` (serve.md:122 folds with `.map`); `clock.md:182-192` `attributes` dual-shape; `clock.md:75` `Tenant`; `clock.md:200-205` `SLOTS` (serve re-spells no header literal — verified serve.md:117-122 imports no `boundary`, defers to CausalFrame.decode).
- `admission.md:171-207` `RuntimeContext` (`admit`, `attribute`); serve.md:122 `RuntimeContext.admit(RuntimeProfile.SIDECAR, causal=causal)`; `admission.md:19` `Deadline` — the E16 target (no `time_remaining` feed today).
- `receipts.md:297-308` `Signals.continue_inbound`/`attach` (serve.md:121,170 the sole cross-module consumer — V6 narrows `attach(remote-parent)` away).
- `resilience.md:232-234` `guarded(cls, fn, *args, subject, **kwargs)` (CapabilityInvoke.connect dispatch, serve.md:306).
- `faults.md:56` `RuntimeRail[T] = Result[T, BoundaryFault]`; `faults.md:82-91` `BoundaryFault` with `FaultTag` (serve `_FAULT_STATUS` keys it, serve.md:94-103); `faults.md:119-138` `facts()` (serve.md:137 aborts with `facts()` string — V14 replaces with FaultDetail trailer).

### ARCHITECTURE seam ledger (target rows the rebuild corrects)
- `ARCHITECTURE.md:42` `execution/admission ← csharp:...AppHost/Runtime # CredentialPem` — MISFILED; the decoder is `serve.md:69-89` (brief V4/E4 re-files to transport/serve).
- `ARCHITECTURE.md:43` `transport/serve → ...AppHost/Observability # W3C trace-context inbound extraction`; `:45` `⇄ AppHost/Agent # DiscoveryResult capability invoke + CommandReceipt`; `:46` `⇄ AppHost/Runtime # HLC two-half stamp + Tenant`; `:20` codemap `serve.py # ServerHost grpc.aio lifecycle, decoded Credential axis, descriptor-driven CapabilityInvoke, and cyclopts Entrypoint`.
- `ARCHITECTURE.md:25` codemap `recipe.py # RecipeExecution: queenbee-schema recipe runs via lbt-recipes over the lane, engine-gated, content-keyed, luigi-evidence verdicts`.
