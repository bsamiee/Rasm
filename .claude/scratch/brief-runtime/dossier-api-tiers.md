# [DOSSIER] runtime — lane: api-tiers

Scope covered: BOTH tiers complete. `libs/python/runtime/.api/*.md` (22 stubs, all read full), `libs/python/.api/*.md` (branch substrate tier, ownership-tagging surveyed), all 13 owning `.planning/` pages, `ARCHITECTURE.md`, `README.md`, `IDEAS.md`, `TASKLOG.md`, root `pyproject.toml` package block, `libs/.planning/architecture.md` strata, `RASM-COMPONENT-PARADIGM-DECISION.md` [AMENDMENTS] wire law.

Stance: hostile. A `.api` catalog is DATA feeding a page generator; a catalog with no consuming fence is a dead carrier or an illusory-capability gap. A capability asserted in `.api` prose but absent from every `.planning` fence is ILLUSORY.

---

## [00] FOLDER SHAPE (verified on disk)

- ARCHITECTURE `[01]-[DOMAIN_MAP]` = 6 sub-domains / 13 pages: `observability/{receipts,metrics,telemetry}`, `reliability/{faults,resilience}`, `transport/{roots,serve,wire}`, `execution/{admission,lanes}`, `evidence/{identity,evidence}`, `clock/{clock}`. NO `recipe` node.
- runtime README `[01]-[ROUTER]` = the same 13 pages. NO recipe route.
- runtime `.api` tier (22): `apscheduler asyncssh cyclopts fsspec gcsfs grpcio httpx keyring lbt-recipes lz4 msgspec obstore opentelemetry-api opentelemetry-instrumentation-grpc pollination-handlers protobuf queenbee s3fs tree-sitter tree-sitter-python tree-sitter-typescript watchfiles`.
- branch `.api` tier (26): the substrate registry (README `[03]`). Runtime pages consume from it: `expression anyio structlog opentelemetry-{api,sdk,exporter-otlp-proto-http} psutil xxhash stamina pydantic pydantic-settings beartype protobuf grpcio grpcio-tools msgspec` + cross-folder `adbc-driver-manager daft` (resilience only).
- Manifest owner tags (`pyproject.toml`): `s3fs`/`gcsfs`/`obstore`/`universal-pathlib`/`adbc-driver-manager`/`daft`/`deltalake`/`pyiceberg`=[DATA]; `compas`=[GEOMETRY]; `lz4`=[ARTIFACTS]; `queenbee`/`lbt-recipes`/`pollination-handlers`/`apscheduler`/`cyclopts`/`keyring`/`opentelemetry-instrumentation-grpc`/`protobuf`/`grpcio`=[RUNTIME]. **The manifest already disputes the runtime README's `[STORAGE_ROOTS]` claim over s3fs/gcsfs/obstore/universal-pathlib.**

---

## [01] PER-PAGE VERDICTS (api-consumption lens)

Verdict = how completely the page mines its declared package surface and how truthfully page prose aligns with its `.api` anchors. Not a full logic re-grade (that is the sibling lanes).

### transport/wire.md — 9/10
- Mines `msgspec` (Decoder/msgpack.Decoder/structs.asdict/convert/to_builtins/Raw/ValidationError), `protobuf` (`google.protobuf.proto` façade + json_format), `opentelemetry-api` (SpanKind CONSUMER/PRODUCER), `expression.Map`, resilience `guarded`, faults `boundary`. Deep, correct, `proto.serialize(deterministic=True)`/`json_format` round-trip is exactly the protobuf.md `WIRE_TOPOLOGY` binary-law.
- Defect (illusory-ish): `wire.md:7` + `:183` route the CRDT op-log decompress through an injected `DecompressFn` and explicitly DEFER lz4 ("LZ4 is worker and the compressed-envelope decode is deferred"). `runtime/.api/lz4.md` is a full 82-LOC catalog with zero live consumer. Legitimate (`IDEAS [CRDT_OPLOG_LZ4_DECODE]` = UPSTREAM-BLOCKED) but the catalog is authored far ahead and lz4 is manifest-tagged [ARTIFACTS], not [RUNTIME].
- Charter as-should-be: the codec owner is correct; the lz4 catalog should carry a one-line "blocked, see CRDT_OPLOG_LZ4_DECODE" and live in its owning tier, not a full frame+block surface in runtime.

### transport/serve.md — 8/10
- Mines `grpcio` (`grpc.aio` server/channel/ServicerContext/AioRpcError/local_server_credentials), `opentelemetry-instrumentation-grpc` (aio_server_interceptor/aio_client_interceptors/filters.negate/health_check/request_hook/response_hook — well-mined), `cyclopts` (App/Parameter/NonNegativeFloat), protobuf/msgspec/expression.
- Defect (prose-vs-catalog drift, `serve.md:200`): cites "branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [04]/[11], the trace-span members the resident `.api/opentelemetry-api.md` metrics/context scope does not carry" — but `runtime/.api/opentelemetry-api.md` DOES carry the full trace surface (PUBLIC_TYPES trace [03]/[04], ENTRYPOINTS [03]/[09], span lifecycle §93-108). The cross-ref is stale; the resident is NOT a metrics/context overlay.
- Defect (unmined by design, not a defect to fix): `Credential` decodes the 5-row C# `CredentialPolicy` but serves only `insecure_loopback`; the `tls`/`mtls`/`bearer`/`composed` rows are decoded-then-`Error`-rejected (`serve.md:82-89`). So `grpcio.md`'s `ssl_channel_credentials`/`ssl_server_credentials`/`composite_channel_credentials`/`metadata_call_credentials` surface (grpcio.md ENTRYPOINTS credential [01]-[07]) is entirely unmined — correct for a UDS-loopback companion, but the whole grpcio credential family is catalogued for one loopback call.

### transport/roots.md — 9/10
- Mines `obstore` (get_async/list/sign_async/store.from_url/Bytes/ObjectStore/GetResult.bytes+stream/RetryConfig), `httpx` (AsyncClient/Auth/Timeout/Limits/stream/event_hooks), `asyncssh` (connect/SSHClientConnectionOptions/SSHKnownHosts/start_sftp_client/SFTP), `universal-pathlib` (UPath), `anyio` (to_thread), `pydantic.SecretStr`. Dense, `functools.cache` client/store pooling + `drain` teardown is exactly httpx.md `drain law` + obstore.md `STORAGE_TOPOLOGY`.
- Defect (dead carrier): `_store` (`roots.md:203-204`) routes `OBJECT_STORE_SCHEMES = {s3,gs,az,abfs}` to `obstore` and everything else to `UPath`. `s3fs`/`gcsfs` are NEVER reached — obstore's Rust core owns s3/gs/az, and obstore.md `fsspec-bridge law` + s3fs.md/gcsfs.md `obstore boundary` note ("pick one transport per root") make s3fs/gcsfs redundant. The two catalogs (s3fs 85 LOC, gcsfs 76 LOC) have no runtime consumer.
- Defect (unmined by split): obstore write side (put/copy/rename/delete/open_writer/PutMode/UpdateVersion, obstore.md ENTRYPOINTS [02]/[05]/[06] + streaming IO) is deferred to `data:tabular/egress#EGRESS` (`roots.md:35` Boundary). Correct split, but the full obstore write/conditional surface is catalogued in a runtime-tier stub whose runtime consumer reads only.

### observability/receipts.md — 9/10
- Mines `structlog` (configure/processors/contextvars/make_filtering_bound_logger/BytesLoggerFactory/testing), `msgspec` (Struct/json.Encoder), `psutil` (Process/memory_info), `expression` (Option/Map/tagged_union), `opentelemetry-api` (context/propagate/trace), stdlib hashlib. `structlog` chain ordering + `enc_hook=repr`/`order=deterministic` is exactly structlog.md STACKS_WITH msgspec row. No runtime-tier structlog catalog exists (branch-only) — consistent with the substrate-canonical-at-branch rule.

### observability/metrics.md — 9/10
- Mines `opentelemetry-api` (get_meter/create_* full instrument set), `psutil` (oneshot/memory_full_info/cpu_percent/num_threads/num_fds — deep), `stamina` (RetryHook/RetryDetails), `expression` (Block/Result/Option), msgspec. `INSTRUMENTS` `Block[InstrumentSpec]` table folding both disciplines is a genuine generator, not enumerated `create_*` calls — APPROACH-correct.

### observability/telemetry.md — 9/10
- Mines `opentelemetry-sdk` (TracerProvider/MeterProvider/LoggerProvider/BatchSpanProcessor/BatchLogRecordProcessor/PeriodicExportingMetricReader/ParentBased/ALWAYS_ON/Resource/get_aggregated_resources/detectors/AggregationTemporality — deep), `opentelemetry-exporter-otlp-proto-http` (OTLPSpanExporter/OTLPLogExporter/OTLPMetricExporter/Compression), api set_*_provider/propagate/CompositePropagator. `SIGNAL_SPECS` `Block[SignalSpec]` fold + `_batched` kernel is APPROACH-correct. Both OTel SDK/exporter catalogs are branch-only (no runtime dup) — consistent.

### execution/admission.md — 9/10
- Mines `pydantic-settings` (BaseSettings/SettingsConfigDict/secrets_dir/file_secret_settings), `pydantic` (DirectoryPath/FilePath/AnyUrl/HttpUrl/SecretStr), `keyring` (get_credential/errors/credentials — well-mined incl. NoKeyringError miss seam), `asyncssh` (read_known_hosts/SSHKnownHosts), `anyio.to_thread`, `expression`, resilience `guarded`. `SecretTier`/`SECRET_LADDER` ADT+table is APPROACH-correct. Declares the cross-file `RetryClass.SECRET` addition on resilience (`admission.md:164`) — a real cross-page seam.
- Note: cloud `SecretTier.cloud` arm is a deferred placeholder (`google-cloud-secret-manager` unadmitted); `pydantic-settings.md` `GoogleSecretManagerSettingsSource` (source [13]) correctly unmined.

### execution/lanes.md — 9/10
- Mines `anyio` (create_task_group/CapacityLimiter/move_on_after/create_memory_object_stream/to_interpreter/WouldBlock/BrokenWorkerInterpreter), `apscheduler` (AsyncIOScheduler/CronTrigger/IntervalTrigger/DateTrigger/events), `watchfiles` (awatch/PythonFilter/BaseFilter/Change), `opentelemetry-api` (propagate/context stitch), `expression`, stdlib graphlib/functools/contextlib. `Admit`/`ADMIT_TABLE`/`LaneSource` ADTs are APPROACH-correct.
- Defect (COVERAGE): `type Trigger = CronTrigger | IntervalTrigger | DateTrigger` (`lanes.md:58`) omits `CalendarIntervalTrigger` + `AndTrigger`/`OrTrigger` — 3 of the 6 trigger types `apscheduler.md` PUBLIC_TYPES trigger [01]-[06] documents. Growth prose ("a new trigger modality is one apscheduler Trigger row") is contradicted by the closed 3-member alias.
- Defect (foreign scope): `WORKER_BAND` constant (`lanes.md:84-89`) documents a process-wide native-worker `CapacityLimiter` for `exchange/detect`, `graphic/raster/io`, `media`, `process` — folders that are NOT `libs/python/runtime` (they read as artifacts/data/geometry sub-domains). A runtime lane page hardcodes a shared limiter charter for sibling folders' interiors — a coupling/scope smell.

### evidence/identity.md — 9/10
- Mines `xxhash` (xxh3_128_intdigest/xxh3_64_intdigest/streaming xxh3_128), `msgspec` (Struct gc=False/Meta/msgpack.Encoder deterministic/EncodeError), `expression` (tagged_union/Block), `opentelemetry-api` (span+total Status), faults `boundary`. `IdentitySource` 4-case ADT owning lift+fold is APPROACH-correct. xxhash is branch-only (owner=runtime tagged) — consistent.

### evidence/evidence.md — 9/10
- Mines `tree-sitter` (Language/Parser.parse/Query/QueryCursor match_limit/captures/set_byte_range/set_max_start_depth/did_exceed_match_limit/Point/Node.*/descendant_count/progress_callback), `tree-sitter-python`/`tree-sitter-typescript` (language/TAGS_QUERY/HIGHLIGHTS_QUERY), stdlib importlib.metadata, faults `@trapped`/`traversed`, receipts.
- Defect (unmined optimization): `tree-sitter.md` LOCAL_ADMISSION + `traversal law` prescribe resolving capture-name/node-kind strings to integer ids ONCE at registry build (`Language.id_for_node_kind`/`field_id_for_name`) and matching integers in the hot loop. `evidence.md` matches capture names as strings per node (`fact.capture == "name"`, `:241`) — the id-resolution surface (tree-sitter.md ENTRYPOINTS grammar introspection [01]-[09]) is unmined.
- Note: tree-sitter incremental `edit`/`changed_ranges`/`TreeCursor` correctly unmined (one-shot scans).

### reliability/faults.md — 10/10
- Mines `expression` (Result/Option/effect.result/Block folds), `beartype` (BeartypeConf/BeartypeCallHintViolation), `msgspec` (ValidationError/DecodeError), `anyio` (Broken* + to_process), `opentelemetry-api` (record_exception on active span, never mints one). `CLASSIFY` ordered table + `Disposition` overloads are APPROACH-perfect. No cross-folder import — the one reliability page that stays clean.

### reliability/resilience.md — 6/10 (structural coupling; see VC3)
- Mines `stamina` deeply (AsyncRetryingCaller/RetryingCaller/BoundAsyncRetryingCaller/retry_context/set_on_retry_hooks/instrumentation/set_testing), `expression.Map`, faults, receipts, metrics.
- Defect (strata coupling): module-top imports `from adbc_driver_manager import AdbcStatusCode`, `from adbc_driver_manager.dbapi import OperationalError`, `from daft.exceptions import DaftTransientError`, `from obstore.exceptions import BaseError` (`resilience.md:31-37`). All manifest-tagged [DATA]. 5 of 11 `RetryClass` rows (OCCT/RPC/LAKE_COMMIT/REMOTE_DB/STREAMING) + `guarded_sync`/`guard_sync` serve ONLY data/geometry consumers. Runtime cannot import `rasm.runtime.resilience` without data-tier packages installed — violates campaign-method `[03]` "packages complete in isolation."
- Defect (inconsistency): `_named(qualname...)` (`resilience.md:128`) exists precisely to dodge importing gated packages (used for deltalake/pyiceberg CommitFailed*, compas RPCServerError). Yet daft `DaftTransientError` and obstore `BaseError` are bare-tuple `target`s that COULD be `_named`-dodged identically — they are imported instead. Only `AdbcStatusCode` (a status enum read in `_adbc_transient`) is a genuinely unavoidable import.

---

## [02] PER-STUB MINED-vs-UNMINED (runtime tier)

| stub | LOC | owning page | mined? | note |
|---|---|---|---|---|
| `wire`→`msgspec` (runtime dup) | 108 | wire/serve/lanes/clock/identity | MINED | but full re-catalog of branch tier — see VC4 |
| `protobuf` (runtime dup) | 131 | wire/serve | PARTIAL | wire uses `proto.serialize/parse`+`json_format`; WKT wrappers (Any/Timestamp/Duration/Struct), descriptor_pool, text_format, message_factory UNMINED |
| `grpcio` (runtime dup) | 118 | serve | PARTIAL | UDS-loopback only; TLS/mTLS/call-cred family + streaming calls UNMINED by design |
| `opentelemetry-api` (runtime dup) | 144 | 9 pages | MINED | full trace+metrics+context+propagation; serve.md prose calls it "metrics/context scope" — STALE |
| `opentelemetry-instrumentation-grpc` | 89 | serve | MINED | aio_server/client interceptors + filters fully used |
| `obstore` | 138 | roots | PARTIAL | read side mined; write/conditional/streaming-writer/auth-providers deferred to data |
| `httpx` | 123 | roots | PARTIAL | AsyncClient+Auth+stream mined; BasicAuth/DigestAuth/NetRCAuth, mounts, transports, ASGI/WSGI/Mock UNMINED (bearer is custom auth_flow) |
| `asyncssh` | 147 | roots + admission | THIN | SFTP-read + read_known_hosts only; server/forward/exec/scp/keygen/agent UNMINED; EPL/GPL-2.0 copyleft flagged |
| `apscheduler` | 140 | lanes | PARTIAL | AsyncIOScheduler+cron/interval/date+events mined; CalendarInterval/And/Or triggers, persistent jobstores, executors, export/import UNMINED |
| `watchfiles` | 71 | lanes | PARTIAL | awatch+PythonFilter+Change mined; run_process/arun_process reload drivers, RustNotify, sync watch UNMINED |
| `cyclopts` | 135 | serve (Entrypoint) | THIN | 1 private `serve` command; config/validators/types/meta/completion/shell reserved for Assay CLI — over-scoped for runtime |
| `tree-sitter` | 126 | evidence | PARTIAL | query surface mined; Language-id-resolution, incremental edit, TreeCursor UNMINED |
| `tree-sitter-python` | 39 | evidence | MINED | language/TAGS_QUERY/HIGHLIGHTS_QUERY used |
| `tree-sitter-typescript` | 42 | evidence | MINED | language_typescript/language_tsx/queries used |
| `keyring` | 98 | admission | MINED | get_credential+errors+credentials fully mined |
| `lz4` | 82 | wire (deferred) | UNMINED | UPSTREAM-BLOCKED; DecompressFn injected; manifest [ARTIFACTS] |
| `fsspec` | 107 | roots (via UPath) | INDIRECT | consumed only through universal-pathlib UPath; direct fsspec surface unused |
| `s3fs` | 85 | — | DEAD | no page reaches; obstore owns s3 + fsspec bridge subsumes |
| `gcsfs` | 76 | — | DEAD | no page reaches; obstore owns gs + fsspec bridge subsumes |
| `queenbee` | 156 | — | ILLUSORY | asserts a "runtime recipe owner"; no fence exists |
| `lbt-recipes` | 88 | — | ILLUSORY | asserts Recipe+RecipeSettings+Recipe.run composed under runtime rails; no fence |
| `pollination-handlers` | 65 | — | ILLUSORY | asserts handler resolution "under its process-resource lane"; no fence |

Branch-tier substrate the runtime consumes (all branch-only, no runtime dup, mined via pages): `expression anyio structlog opentelemetry-sdk opentelemetry-exporter-otlp-proto-http psutil xxhash stamina pydantic pydantic-settings beartype grpcio-tools`. **The four packages that DO carry a runtime-tier dup (msgspec/protobuf/grpcio/opentelemetry-api) are exactly the heaviest-integration ones — the duplication is ad hoc, not a uniform overlay policy.**

---

## [03] CROSS-CUTTING FINDINGS

### Duplication
- **4 substrate packages double-catalogued** (`[PY_RUNTIME_API_*]` + `[PY_BRANCH_API_*]`): msgspec (108/141), protobuf (131/153), grpcio (118/159), opentelemetry-api (144/158). Full surface re-catalogs, not lean runtime-integration deltas. README `[03]` says substrate API evidence is canonical at the branch tier. The other ~12 runtime-consumed substrate packages have NO runtime dup — the pattern is inconsistent.
- **Three-way object-store overlap**: obstore (Rust core, owns s3/gs/az + fsspec bridge) vs s3fs vs gcsfs. roots.md picks obstore; s3fs/gcsfs unreachable.

### Concern mixing / strata
- `resilience.md` (runtime) hard-imports data-tier `adbc_driver_manager`/`daft`/`obstore` exception types (VC3). Branch-wide RetryClass table living in runtime couples runtime's import graph downward.
- `lanes.md` `WORKER_BAND` (`:84-89`) names a shared native-worker limiter for `exchange/detect`/`graphic`/`media`/`process` sibling folders — runtime page reaching into other folders' interior scheduling.

### Hardcoding-vs-generator
- `.api` catalogs are correctly DATA (roster tables) feeding page generators. Pages use generators over rows (INSTRUMENTS/SIGNAL_SPECS/CLASSIFY/POLICY/ADMIT_TABLE/SLOTS/PROBE_SOURCES/_CORPUS). No enumerated-instances-where-a-generator-belongs defect in the fences. The one alias-thinness is `lanes.Trigger` (3 of 6 apscheduler triggers).

### Dead carriers
- `s3fs.md`, `gcsfs.md` — admitted (README `[STORAGE_ROOTS]`, manifest [DATA]) but zero runtime consumer; obstore + its fsspec bridge subsume both. Prune from runtime tier or relocate to data.
- `lz4.md` — UPSTREAM-BLOCKED, DecompressFn-injected, manifest [ARTIFACTS]. Deferred-but-full-authored; acceptable as documented deferral, wrong owner tier.

### Unwired declared seams (largest gap)
- **The entire RECIPE rail**: README `[02] [RECIPE]` group + three 65-156 LOC `.api` catalogs (`queenbee`/`lbt-recipes`/`pollination-handlers`) describe "the runtime recipe owner" / "the package owner composes the handler catalog into the recipe IO-coercion seam" / "runs it through the process-resource lane, a deadline scope, a stamina engine-precheck, and a structlog/OpenTelemetry span" — and NO `.planning` page owns any of it. The seam is declared in three catalogs and the README and wired to nothing. 309 LOC of catalog prose asserting an owner that exists in zero code fences.

### Unmined capability (catalog anchors)
- obstore: `open_reader`/`open_writer` (obstore.md streaming IO), `PutMode`/`UpdateVersion` conditional writes (types [07]/[08]), `obstore.auth.*` credential providers ([132-142]) — deferred to data.
- httpx: `BasicAuth`/`DigestAuth`/`NetRCAuth` (types auth [02]-[04]), `mounts`/`MockTransport`/`ASGITransport` — bearer uses custom auth_flow; the built-in Auth family unused.
- asyncssh: `run`/`create_process`/forwarding family/`scp()`/`generate_private_key`/`SSHAgentClient` — narrow SFTP-read consume of a heavy copyleft dep.
- apscheduler: `CalendarIntervalTrigger`/`AndTrigger`/`OrTrigger`, `SQLAlchemyJobStore`/`RedisJobStore`/`MongoDBJobStore` (persistent — correctly unmined; session-local), executors, `export_jobs`/`import_jobs`.
- cyclopts: `config.*`/`validators.*`/the full `cyclopts.types` library/`App.meta`/`interactive_shell`/completion — reserved for Assay, over-scoped in runtime tier.
- tree-sitter: `Language` id-resolution surface (id_for_node_kind/field_id_for_name), incremental `edit`/`changed_ranges`, `TreeCursor`.
- protobuf: well-known wrappers (Any/Timestamp/Duration/Struct/Value), descriptor_pool, message_factory, text_format — CrdtOp path uses only proto.serialize/parse + json_format.
- grpcio: TLS/mTLS/call-credential minting (ssl_*/composite_*/metadata_*), streaming multicallables — UDS-loopback serve mines one credential row.

### DOMAIN GAPS (no admitted package owns a concern the folder needs; roster-addition candidates, named-not-researched)
- **Cloud secret manager**: `admission.md` `SecretTier.cloud` is a placeholder; `google-cloud-secret-manager` (or `azure-keyvault-secrets`/AWS SM) is the missing roster addition to graduate the cloud tier (IDEAS `STRUCTURED_SETTINGS_SCHEMA`).
- **Recipe execution rail is package-complete but owner-absent** (not a package gap — a page gap): queenbee/lbt-recipes/pollination-handlers ARE admitted; the concern has no owning page (VC1).
- No other genuine package gap surfaced: transport, concurrency, retry, telemetry, identity, parsing, secrets, scheduling all have strong admitted owners.

### FIT (boundary contracts, not coupling) — verified
- `evidence/identity.md` reproduces the C# `XxHash128` seed bit-identically over the `ONE_WIRE_FIXTURE_CORPUS`; the `[AMENDMENTS]` count-prefix wire law (`RASM-COMPONENT-PARADIGM-DECISION.md:63`, `w.Ordinal(bag.Values.Count)`) is RECORDED as the law the queued `PY_WIRE_ALIGNMENT` campaign builds against — `SeedReproduction` correctly carries the float-bearing `MATERIAL_LAYER_GOLDEN` as a DESIGN-PIN obligation, not fabricated bytes. Fit is clean: the folder decodes C#-minted shapes and re-mints nothing (single-mint invariant held across clock/wire/identity/serve/admission).
- The count-prefix amendment has NO runtime fence impact yet (Python canonical-writer mirror not authored) — correctly deferred; no live decoder forks.

---

## [04] VERDICT CANDIDATES (strongest, campaign-defining)

**VC1 — RECIPE RAIL: illusory owner / missing page (SEVERITY: HIGH).**
`runtime/.api/queenbee.md` (156), `lbt-recipes.md` (88), `pollination-handlers.md` (65) each assert a runtime owner composing `Recipe(recipe_name)`+`RecipeSettings`+`Recipe.run` under the process-resource lane / deadline scope / stamina engine-precheck / structlog+OTel span / typed receipt (queenbee.md:3, lbt-recipes.md:3, pollination-handlers.md:3). README `[02] [RECIPE]` lists all three. ARCHITECTURE `[01]` has 6 sub-domains, none recipe; README `[01]` has 13 routes, none recipe. Evidence: no `.planning` fence imports `queenbee`/`lbt_recipes`/`pollination_handlers`. RULING: author a `recipe` sub-domain page (owner e.g. `RecipeExecution`) composing lanes `offload`+`Transfer`, resilience `guarded`/engine-precheck, observability `@receipted`+span, queenbee schema→lbt-recipes executor→pollination-handlers coercion — OR relocate the three packages+catalogs to their real owner and strike them from runtime README. The catalogs already wire runtime rails, so intent = a runtime owner; the page is simply absent.

**VC2 — STORAGE-ROOTS: ownership mismatch + s3fs/gcsfs dead carriers (SEVERITY: HIGH).**
Manifest tags `s3fs`/`gcsfs`/`obstore`/`universal-pathlib` = [DATA] (`pyproject.toml:25-32`); runtime README `[02] [STORAGE_ROOTS]` claims all four as runtime domain. `roots.md:203` routes object-store schemes to `obstore` and file to `UPath`; s3fs/gcsfs are structurally unreachable, and obstore.md's fsspec bridge + s3fs/gcsfs's own "pick one per root" note make them redundant. RULING: strike s3fs/gcsfs from runtime README+`.api` (data owns them if needed); reconcile obstore/universal-pathlib/fsspec: runtime consumes an obstore READ slice + UPath, so the READMEs+manifest must agree on tier ownership (runtime read-slice vs data full I/O).

**VC3 — resilience.md: branch-wide table hard-coupled into runtime (SEVERITY: HIGH).**
`resilience.md:31-37` module-top imports [DATA] `adbc_driver_manager`/`daft.exceptions`/`obstore.exceptions`; 5 of 11 RetryClass rows + `guarded_sync` serve only data/geometry. `_named` qualname-dodge (`:128`) exists to avoid gated imports yet daft/obstore are imported anyway (only `AdbcStatusCode` is unavoidable). Violates campaign-method `[03]` isolation. RULING: qualname-dodge daft (`DaftTransientError`) and obstore (`BaseError`) via `_named` exactly as deltalake/pyiceberg/compas are; accept the one `AdbcStatusCode` import or route it through a string-status hook; if the branch-wide table must import cross-folder types, hoist it to a higher shared tier rather than runtime/reliability.

**VC4 — FOUR substrate `.api` double-catalogued as full re-catalogs (SEVERITY: MEDIUM).**
msgspec/protobuf/grpcio/opentelemetry-api carry full runtime-tier AND branch-tier catalogs; README `[03]` makes branch canonical. Runtime copies re-document the whole surface, not a lean delta; the other 12 runtime-consumed substrate have no dup (ad hoc). Drift proven: `serve.md:200` calls the resident opentelemetry-api "metrics/context scope [that] does not carry" trace members it in fact carries. RULING: make the four runtime-tier substrate catalogs LEAN integration-overlays (LOCAL_ADMISSION/INTEGRATION_STACK + delta, pointer to branch canonical for PUBLIC_TYPES/ENTRYPOINTS) OR delete them and cite the branch tier; repair the serve.md/wire.md resident-vs-branch cross-refs.

**VC5 — cyclopts / asyncssh: full catalogs, thin runtime consume, wrong scope (SEVERITY: MEDIUM).**
`cyclopts.md` (135) documents a whole CLI framework (config/validators/types/meta/completion/shell); runtime uses one private `serve` command, and `serve.md:324` reserves public commands to Assay. `asyncssh.md` (147) documents SSH server/forwarding/exec/scp/keygen/agent under an EPL-2.0/GPL-2.0 copyleft flag; runtime consumes SFTP-read only. RULING: relocate the cyclopts full surface to the Assay CLI owner, leaving runtime a thin entry-grammar overlay; re-judge whether a copyleft asyncssh is warranted for SFTP-read-only, or whether the obstore/fsspec sftp:// path covers it (asyncssh.md:168 names the split).

**VC6 — lanes.Trigger COVERAGE thinness (SEVERITY: LOW).**
`lanes.md:58` `type Trigger = CronTrigger | IntervalTrigger | DateTrigger` omits `CalendarIntervalTrigger`/`AndTrigger`/`OrTrigger` (apscheduler.md trigger [04]-[06]) while Growth prose claims "a new trigger modality is one apscheduler Trigger row." RULING: widen the alias to the full 6-member trigger union the catalog documents, or state the 3-member restriction as a deliberate scope with rationale.

**VC7 — lz4 forward-catalogued in wrong tier (SEVERITY: LOW).**
`lz4.md` (82, full frame+block) is UPSTREAM-BLOCKED (IDEAS `CRDT_OPLOG_LZ4_DECODE`), DecompressFn-injected, manifest [ARTIFACTS]. RULING: reduce to a blocked-stub pointing at the card, or move to the artifacts tier that owns lz4; the wire seam keeps the injected DecompressFn port.

**VC8 — tree-sitter id-resolution unmined (SEVERITY: LOW).**
`tree-sitter.md` LOCAL_ADMISSION prescribes resolving capture/node-kind strings to integer ids once at registry build; `evidence.md:241` matches capture names as strings per node. RULING: mine `Language.id_for_node_kind`/`field_id_for_name` at `GRAMMARS`/`PROBES` build so the hot capture loop compares integers, per the catalog's own hot-loop law.
