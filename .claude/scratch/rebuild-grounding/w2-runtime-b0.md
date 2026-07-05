# W2 RUNTIME B0 — GROUNDING DOSSIER (verified primary extracts)

Batch: `observability/metrics.md` [rebuild], `reliability/resilience.md` [rebuild], `execution/admission.md` [rebuild], `observability/telemetry.md` [rebuild]. Read-only discovery. Every anchor below is `file:line` against on-disk state; downstream stages spot-verify.

## [00]-[INVENTORIES]

### doctrine root — `ls docs/stacks/python/`
```
algorithms.md boundaries.md concurrency.md iteration.md language.md
rails-and-effects.md README.md runtime.md shapes.md surfaces-and-dispatch.md system-apis.md
```
(README [01]-[ATLAS] routes 10 decisions; iteration.md is the algorithmic-body owner — atlas row [04].)

### shared .api tier — `ls libs/python/.api/`
```
adbc-driver-manager anyio arro3-core beartype daft expression grpcio grpcio-tools
msgspec networkx numcodecs numpy opentelemetry-api opentelemetry-exporter-otlp-proto-http
opentelemetry-sdk protobuf psutil pydantic pydantic-settings stamina structlog trio
universal-pathlib xarray xxhash zlib-ng
```

### folder .api tier — `ls libs/python/runtime/.api/`
```
apscheduler asyncssh cyclopts fsspec google-cloud-secret-manager grpcio-health-checking
grpcio httpx keyring lbt-recipes lz4 msgspec obstore opentelemetry-instrumentation-grpc
pollination-handlers protobuf queenbee tree-sitter-python tree-sitter-typescript
tree-sitter watchfiles
```

### runtime .planning module set (flat `rasm.runtime.<leaf>`)
```
clock evidence identity reproduction execution/{admission,lanes,recipe}
observability/{metrics,receipts,telemetry} reliability/{faults,resilience}
transport/{roots,serve,wire}  (+ NEW shapes per brief V2)
```

## [01]-[ASSAY_VERIFICATION] (members these maps cite)

- `opentelemetry-api` restored `1.43.0` (`assay api resolve opentelemetry-api` -> `"restore":"restored","status":"ok"`, 51/51 paths).
- `Meter.create_histogram` params on disk: `['self','name','unit','description','explicit_bucket_boundaries_advisory']` — advisory kwarg REAL (the branch catalog `libs/python/.api/opentelemetry-api.md:111` elides it as `create_histogram(name, ...)`; page member is verified, the catalog under-documents the kwarg).
- `Counter.add` params: `['self','amount','attributes','context']`; `Histogram.record` params: `['self','amount','attributes','context']` — the `context=` exemplar hand-off is real.
- `opentelemetry.sdk.metrics.MeterProvider.__init__` params: `['self','metric_readers','resource','exemplar_filter','shutdown_on_exit','views','_meter_configurator']` — `exemplar_filter` AND `views` are real constructor params.
- `opentelemetry.sdk.metrics.TraceBasedExemplarFilter` — importable.

## [02]-[SHARED_TIER API ANCHORS]

### `libs/python/.api/opentelemetry-api.md`
- `:46` `| [06] | Meter.create_gauge(...) | factory | synchronous last-value gauge` (Gauge ABC not top-level).
- `:50` `| [10] | Observation | value | (value, attributes) for async`
- `:51` `| [11] | CallbackOptions | value | timeout hint for async callbacks`
- `:110` `| [05] | Meter.create_up_down_counter(name, ...) | instrument`
- `:113` `| [08] | Meter.create_observable_counter(name, callbacks) | instrument`
- `:114` `| [09] | Meter.create_observable_gauge(name, callbacks) | instrument`
- `:115` `| [10] | Meter.create_observable_up_down_counter(name, callbacks) | instrument`
- `:116` `| [11] | Counter.add(amount, attributes, context) | record`
- `:117` `| [12] | UpDownCounter.add(amount, attributes, context) | record` (NOT used by metrics — observable up_down instead; correct)
- `:118` `| [13] | Histogram.record(amount, attributes, context) | record`
- `:111` `| [06] | Meter.create_histogram(name, ...) | instrument` (advisory kwarg not spelled in catalog; real on disk)

### `libs/python/.api/opentelemetry-sdk.md`
- `:64` `| [06] | sdk.metrics.TraceBasedExemplarFilter | filter | include exemplar when span is sampled`
- `:87` `| [12] | sdk.metrics.view.View | config | instrument-to-aggregation mapping`
- `:92` `| [17] | sdk.metrics.view.ExplicitBucketHistogramAggregation | aggregation | fixed-bucket histogram`
- `:147` `View(instrument_type=..., instrument_name=..., ..., aggregation=None, exemplar_reservoir_factory=None)`
- `:148` `ExplicitBucketHistogramAggregation(boundaries=None, record_min_max=True)`
- `:182` metric output shape set by View + Aggregation.

### `libs/python/.api/psutil.md`
- `:3` `Process.oneshot()` batches syscalls — one collection per block.
- `:57` `| [01] | Process.oneshot() | context manager | batch internal syscalls`
- `:70` `| [01] | Process.memory_info() -> pmem | RSS/VMS`
- `:71` `| [02] | Process.memory_full_info() -> pfullmem | adds uss`
- `:73` `| [04] | Process.cpu_percent(interval=None) | None = since-last-call delta`
- `:75` `| [06] | Process.num_threads()`
- `:77` `| [08] | Process.num_fds() (POSIX)`
(All mined by metrics `ProcessReading.sample` — metrics.md:96-107.)

### `libs/python/.api/stamina.md`
- `:39` `| [01] | ExcOrBackoffHook | type[Exception] | tuple[...] | BackoffHook`
- `:40` `| [02] | BackoffHook | Callable[[Exception], bool | float | timedelta]`
- `:48` `| [01] | instrumentation.RetryHook | Protocol | (RetryDetails) -> None | AbstractContextManager[None]`
- `:49` `| [02] | instrumentation.RetryHookFactory | frozen dataclass | wraps hook_factory: Callable[[], RetryHook]`
- `:50` `| [03] | instrumentation.RetryDetails | frozen dataclass | per-retry payload`
- `:51` `| [04] | instrumentation.StructlogOnRetryHook | RetryHookFactory`
- `:53`/`:94` `PrometheusOnRetryHook` / `get_on_retry_hooks()` (unmined — structlog is branch logger; correct)
- `:88`/`:92` `set_on_retry_hooks(hooks)` — `()` deactivates, process-global.
- `:84` `is_testing`/`set_testing(bool, *, attempts=1)`.
- `:110` `RetryDetails` maps field-for-field onto a receipt fact stream (`caused_by`/`retry_num`/`wait_for`/`waited_so_far`).

### `libs/python/.api/pydantic-settings.md`
- `:36` `| [04] | SecretsSettingsSource | secrets-directory layer (flat files)`
- `:37` `| [05] | NestedSecretsSettingsSource | subdir-per-model`
- `:45` `| [13] | GoogleSecretManagerSettingsSource | GCP Secret Manager layer (gcp-secret-manager extra)`
- `:74` `settings_customise_sources(cls, settings_cls, init_settings, env_settings, dotenv_settings, file_secret_settings)`
- `:104` default order `init_settings > env_settings > dotenv_settings > file_secret_settings`; reorder by permuted tuple.

## [03]-[FOLDER_TIER API ANCHORS]

### `libs/python/runtime/.api/google-cloud-secret-manager.md` (ADMITTED 2.29.0, Apache-2.0)
- `:3` charter: "the client backing the branch-catalogued `GoogleSecretManagerSettingsSource` ... which the `execution/admission#SETTINGS` `SecretTier.cloud` arm graduates from the deferred `Ok(Nothing)` placeholder to a real `SECRET_LADDER` cloud-tier row."
- PUBLIC_TYPES `[01]` `SecretManagerServiceClient` — "the shape `GoogleSecretManagerSettingsSource` accepts as `secret_client=`"; `[02]` `SecretManagerServiceAsyncClient`.
- payload `[02]` `AccessSecretVersionResponse` (`name` + `payload`); `[03]` `SecretPayload` (`data: bytes` + `data_crc32c: int`).
- ENTRYPOINTS `[01]` `SecretManagerServiceClient(credentials=None, transport=None, client_options=None)`; `[02]` `.from_service_account_file(path)`; `[03]` `.secret_version_path(project, secret, version)`.
- read `[01]` `client.access_secret_version(name=)` -> response, `.payload.data` bytes; `[03]` async twin.

### `libs/python/runtime/.api/stamina.md` — none (stamina is shared tier only; no folder overlay).

## [04]-[TARGET_PAGE ANCHORS]

### `observability/metrics.md` (rebuild — V8 instrument collapse, V5 domain generalization, V8 scope/Map rail)
- `:71` `METER_NAME: Final = "rasm.runtime"` — local literal; faults `SCOPES[Scope.METER]` = `"rasm.runtime"` (faults.md:174) is the branch owner.
- `:80` `FAULT_OUTCOME: Final[dict[FaultTag, DrainOutcome]] = {"deadline": "cancelled"}` — plain `dict`, must join corpus `expression.Map` rail (V8).
- `:84` `ZERO_DRAIN: Final[DrainReceipt[object]] = DrainReceipt(accepted=0, completed=0, cancelled=0, rejected=0)` — omits `hit=`; `DRAIN_COLUMNS` includes `"hit"` (receipts.md:82) — V8 wants explicit `hit=0` column parity.
- `:130-135` `class SyncInstruments` fields `duration`/`retries`/`artifact_bytes`/`artifact_ratio` — declaration site 1.
- `:139-143` `_HIST_SLOT` name->field map — declaration site 2.
- `:168-174` `_seed_sync(meter)` mints 4 recorders with name/unit literals — declaration site 3.
- `:179-193` `INSTRUMENTS` `Block[InstrumentSpec]` rows (4 synchronous + observable) — declaration site 4. V8: one `INSTRUMENTS` fold must DERIVE the `SyncInstruments` carrier; `_seed_sync` deletes, `_HIST_SLOT` derives from row metadata.
- `:244-252` `record_artifact(cls, kind, byte_volume, ratio)` — artifacts-DOMAIN method baked into base spine; V5: generalize to a table-keyed domain-histogram recorder over `INSTRUMENTS` rows (a new domain distribution is one row, never a classmethod). `artifacts` is the named demanding consumer (brief V5).
- `:262-274` `measured[**P, T]` aspect — sound (FAULT_OUTCOME projection); `:280-282` `_callback` polymorphic pull — sound.
- No tracer minted here (metrics reads meter only); latched imported from faults (`:39`) — V1 landed.

### `reliability/resilience.md` (rebuild — V5 base purity, V14 wire predicate, V8 scope)
- `:31-37` module-top imports of `adbc_driver_manager`/`adbc_driver_manager.dbapi`/`daft.exceptions.DaftTransientError`/`obstore.exceptions.BaseError` — DATA-tier gated packages imported in a BASE-tier module (V5 base-purity violation; E8).
- `:109` `_TRACER: Final = trace.get_tracer("rasm.runtime.resilience")` — raw literal; faults `SCOPES[Scope.RESILIENCE]` = `"rasm.runtime.resilience"` (faults.md:175) is the owner (V8).
- `:126-132` `_named(*qualnames)` `BackoffHook` — the import-free by-`__qualname__` predicate already built to dodge gated imports.
- `:135-153` `_adbc_transient` reads `AdbcStatusCode.TIMEOUT`/`.IO` via the enum import — must convert to a name-based predicate (V5).
- `:242-298` `POLICY` `Map[str, Policy]` rows — sibling-domain rows (occt/rpc/lake-commit/remote-db/streaming) STAY (data, one branch table) but must be import-free.
- `:246` `("wire", Policy(attempts=5, timeout=15.0, target=(ConnectionError,)))` — `(ConnectionError,)` can NEVER match `grpc.aio.AioRpcError`; V14 wants an import-free status-code-name `BackoffHook` predicate (the `_named`/`_adbc_transient` shape) admitting transient gRPC codes.
- `:305-311` `RetryReceiptHook`/`RETRY_HOOKS` — module-load `Metrics.retry_hook()` call; resilience imports `Metrics` (`:43`), DAG-legal (metrics < resilience).
- `:43-44` imports `faults` + `metrics` + `receipts` downward — DAG order holds.

### `execution/admission.md` (rebuild — V12 rename/mount, cloud tier real)
- `:266` `SECRETS_MOUNT: Final[str] = "/run/secrets"` — hardcoded infra literal; V12 parameterizes into `SettingsAdmission` with `/run/secrets` as default value.
- `:271-273` `class Credential(Struct, frozen=True): username; secret: SecretStr` — collides with serve's `Credential` union (V11/V12); admission's renames `BasicCredential`.
- `:250-259` `SecretTier` `@tagged_union` `keystore`/`file`/`cloud`; `cloud: str = case()` the deferred arm.
- `:363-367` `case SecretTier(tag="cloud"): ... return Ok(Nothing)` — the STUB; cloud arm graduates to a real `SecretManagerServiceClient.access_secret_version` read gated by `Feature.SECRET_MANAGER`, offloaded through `anyio.to_thread.run_sync` under `guarded(RetryClass.SECRET)`.
- `:382-385` `SECRET_LADDER` `Block[TierRow]` — keystore over file; deferred cloud row `TierRow(SecretTier(cloud="rasm"), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` graduates.
- `:276-285` `SettingsAdmission(BaseSettings)` fields — a `gcp_project_id` field admits the cloud tier; `secrets_dir=SECRETS_MOUNT` (`:280`) reads the mount const.
- `:388-396` `## [04]-[RESEARCH]` tail — BANNED (V11), deletes.
- `:97-138` `PROFILE_POLICY`/`FeatureGate` — `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER` are the forward gate the cloud row resolves against (admission.md:57-73).

### `observability/telemetry.md` (rebuild — V9 per-signal subject, grammar; latched landed)
- `:178-187` `_meter_provider(endpoint, headers, profile)` -> `MeterProvider(metric_readers=[reader], resource=RUNTIME_RESOURCE)` — NO `exemplar_filter=` and NO `views=`. metrics.md `:242`/`:257` claim `context=otel_context.get_current()` ties measurements to spans via "the SDK exemplar filter" — INERT without `exemplar_filter=TraceBasedExemplarFilter()` (SDK default filter drops exemplars). Confirmed constructor param exists (see [01]).
- `:190-194` `_drained(provider, signal)` raises `TimeoutError(signal.value)` on falsy `force_flush`.
- `:200-208` `InstalledProviders.flush` -> `boundary("resource", lambda kv=kv: _drained(kv[1], kv[0]))` — subject hardcoded `"resource"` for every signal (V9); the `TimeoutError(signal.value)` message is dropped by the CLASSIFY deadline row (faults.md:147), so a wedged exporter cannot be attributed traces-vs-logs-vs-metrics. Fix: `boundary(kv[0].value, ...)`.
- `:74` `from rasm.runtime.faults import Disposition, RuntimeRail, boundary, latched, traversed` — `latched` already re-homed to faults (V1 landed).
- `:259-264` `## [03]-[RESEARCH]` tail — BANNED (V11), deletes.
- `:160-175` `SIGNAL_SPECS` + `_batched` kernel — sound (preserve).

## [05]-[SIBLING/SEAM ANCHORS]

- `reliability/faults.md:68-76` `class Scope(StrEnum)` members incl. `METER`/`RESILIENCE`; `:172-180` `SCOPES: Final[Map[Scope, str]]` rows `(Scope.METER,"rasm.runtime")`, `(Scope.RESILIENCE,"rasm.runtime.resilience")` — the V8 scope-vocabulary owner metrics/resilience must read.
- `reliability/faults.md:146-162` `CLASSIFY` — `TimeoutError` deadline row defaults budget `0.0` and reads `str(cause) or type(cause).__name__` into cause (so telemetry's per-signal `TimeoutError(signal.value)` message survives IF telemetry passes the signal subject; V9 depends on it).
- `reliability/faults.md:265-282` `latched[R, **P]` — the re-homed one-shot latch metrics/telemetry compose.
- `observability/receipts.md:66` `type DrainOutcome = Literal["accepted","completed","cancelled","rejected","hit"]`; `:82` `DRAIN_COLUMNS = get_args(DrainOutcome.__value__)`; `:115-148` `DrainReceipt[T]` + `.of` — the drain taxonomy metrics/lanes import downward (V1 landed).
- `execution/lanes.md:214-215` `drained` aspect calls `Metrics.observe(receipt, in_flight=receipt.cancelled)` + `Signals.emit(Receipt.of(owner, receipt), redaction)` — the metrics `observe`/`in_flight` consumer.
- `execution/lanes.md:250-254` `ADMIT_TABLE` `retried` row binds `guard(unit.retried[0])` — the resilience `guard` bare-caller consumer.

## [06]-[BRIEF SEAM/RIDER ANCHORS]

- `RASM-PY-RUNTIME-BRIEF.md:49` GENERATOR_LAW: sync-instrument definition spread FOUR sites (metrics.md:134-139/171-179/143-147/183-196); `record_artifact` bakes artifacts domain into base spine; per-page scope literals mint with no vocabulary (owner: faults); `FAULT_OUTCOME`/`SLOTS` plain-dict beside Map corpus.
- `:89` V5 BASE_PURITY: resilience.md:31-37 adbc/daft/obstore imports die via `_named`; rows STAY import-free; `metrics` `record_artifact`+two `artifact.*` rows generalize to table-keyed domain-histogram recorder (`artifacts` named demanding consumer).
- `:99-101` V8: metrics derives sync carrier from INSTRUMENTS (`_seed_sync` deletes, `_HIST_SLOT` derives); `ZERO_DRAIN` spells `hit=0`; `FAULT_OUTCOME`/`SLOTS` join Map rail; faults gains `latched` + scope vocabulary (one table owning every tracer/meter/service literal); CLASSIFY deadline row keeps cause.
- `:104-105` V9: telemetry `flush` hardcodes `"resource"` (telemetry.md:206) vs `_drained` `TimeoutError(signal.value)` (:193) dropped — thread `subject=kv[0].value`; meter-last order + `_batched` stay.
- `:117` V12: admission's `Credential` -> `BasicCredential`; serve's union -> `CredentialPolicy`; `SECRETS_MOUNT` literal parameterizes into `SettingsAdmission` with `/run/secrets` default.
- `:125` V14: `wire` POLICY row `target=(ConnectionError,)` (resilience.md:245) can never match `AioRpcError` -> import-free status-code-name `BackoffHook` predicate admitting transient gRPC codes.
- `:183` PACKAGE_PRESSURE: `google-cloud-secret-manager` ADMITTED, graduates `SecretTier.cloud` placeholder to a `SECRET_LADDER` row through `GoogleSecretManagerSettingsSource`.
- `:167-168` ESCALATION: metrics 7->9.5 (one-table derivation; domain-histogram generalization; latched->faults; Map rail); telemetry 8->9.5 (latched leaves; per-signal subject; grammar); resilience 6->9.5 (import-free rows; hooks proven; wire predicate; scope vocabulary; grammar); admission 7->9.5 (SECRET seam held; BasicCredential; parameterized mount; cloud tier REAL; grammar).
- `:202-203` BUILD_LEGS: metrics/resilience/admission/telemetry are all LEG 2 (MECHANISM); import only same-or-earlier-leg pages.

## [07]-[CONFIRMED CROSS-PAGE INTEGRATION GAP]
metrics.md's exemplar-correlation claim (`:242`,`:257`,`:287-290` "the SDK exemplar filter ties each measurement to its span") is INERT because telemetry's `_meter_provider` (telemetry.md:178-187) omits `exemplar_filter=TraceBasedExemplarFilter()`. The constructor param and the filter class are both verified real on disk (assay [01]). ROUTED to `observability/telemetry.md` as a buildout: wire `TraceBasedExemplarFilter` (and optionally a `View`+`ExplicitBucketHistogramAggregation` honoring the metrics `DURATION_BUCKETS_MS` advisory) into `MeterProvider(..., exemplar_filter=..., views=...)`.
