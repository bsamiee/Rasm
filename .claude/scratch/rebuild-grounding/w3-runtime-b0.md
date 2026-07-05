# W3 RUNTIME BATCH 0 — GROUNDING DOSSIER

Verified primary extracts for `transport/shapes.md` [new], `transport/roots.md` [improve], `transport/wire.md` [rebuild], `execution/lanes.md` [rebuild]. Every cited member confirmed by import against the installed distribution or quoted from its member-verified `.api` catalog with a `file:line` anchor. No paraphrase of doctrine law.

## REAL LS INVENTORIES

### doctrine root — `docs/stacks/python/`
```
algorithms.md boundaries.md concurrency.md iteration.md language.md
rails-and-effects.md README.md runtime.md shapes.md surfaces-and-dispatch.md system-apis.md
```
(README `[01]-[ATLAS]` routes 10 pages; `iteration.md` present on disk AND in atlas row [04].)

### shared/universal .api tier — `libs/python/.api/`
```
adbc-driver-manager.md anyio.md arro3-core.md beartype.md daft.md expression.md
grpcio-tools.md grpcio.md msgspec.md networkx.md numcodecs.md numpy.md
opentelemetry-api.md opentelemetry-exporter-otlp-proto-http.md opentelemetry-sdk.md
protobuf.md psutil.md pydantic-settings.md pydantic.md stamina.md structlog.md trio.md
universal-pathlib.md xarray.md xxhash.md zlib-ng.md
```

### folder domain .api tier — `libs/python/runtime/.api/`
```
apscheduler.md asyncssh.md cyclopts.md fsspec.md google-cloud-secret-manager.md
grpcio-health-checking.md grpcio.md httpx.md keyring.md lbt-recipes.md lz4.md
msgspec.md obstore.md opentelemetry-instrumentation-grpc.md pollination-handlers.md
protobuf.md queenbee.md tree-sitter-python.md tree-sitter-typescript.md tree-sitter.md watchfiles.md
```
NOTE: `grpcio-tools.md` lives ONLY in the SHARED tier (`libs/python/.api/grpcio-tools.md`); no runtime-tier copy. `transport/shapes.md` [new] does NOT yet exist on disk (confirmed absent); it is the leg-3 FIRST page.

### transport + execution planning folders — `libs/python/runtime/.planning/`
```
transport/  : roots.md serve.md wire.md            (shapes.md ABSENT — the new page)
execution/  : admission.md lanes.md recipe.md      (all present; recipe.md already imports lanes.Modality)
```

## MEMBER VERIFICATIONS (import-confirmed 2026-07-05)

- `google.protobuf.proto.serialize` = True, `.parse` = True — `libs/python/.api/protobuf.md:89` `proto.serialize(message, deterministic=None) -> bytes`; `:90` `proto.parse(message_class, payload) -> message`.
- `google.protobuf.descriptor_pool.Default` = True; `DescriptorPool.AddSerializedFile` = True; `.FindMessageTypeByName` — `protobuf.md:116` `descriptor_pool.Default() -> DescriptorPool`; `:117` `FindMessageTypeByName(full_name) -> Descriptor`; `:119` `AddSerializedFile(serialized_pb) -> FileDescriptor`.
- `google.protobuf.json_format.ParseDict` = True, `.MessageToDict` = True — `protobuf.md:105` / `:103` (`preserving_proto_field_name`).
- `proto.serialize_length_prefixed` / `parse_length_prefixed` — `protobuf.md:91`/`:92` (varint-length-prefixed streaming frames).
- `message_factory.GetMessageClass(descriptor)` — `protobuf.md:122` (dynamic-message path for runtime `Descriptor`).
- `grpc_tools.protoc.main` = True — `libs/python/.api/grpcio-tools.md:41` `protoc.main(command_arguments) -> int`; `:43` `command.build_package_protos(package_root, strict_mode=False)`; `:55` the `sys.meta_path` dynamic-stub hooks (REJECTED per brief).
- `msgspec.structs.asdict` = True; `msgspec.Raw` importable — `libs/python/.api/msgspec.md:126` `structs.asdict`; `:85` `inspect.RawType`; `:136` `inspect.type_info(type)`; `:137` `inspect.multi_type_info(types)`; `:74` `inspect.StructType`; `:75` `inspect.Field`; `:98` `msgspec.convert(..., strict, ...)`; `:117` `msgpack.Decoder(type, *, strict, dec_hook, ext_hook)`.
- `obstore.sign_async` = True, `.list` = True, `.get_async` = True — `libs/python/runtime/.api/obstore.md:100` `sign(store, method, paths, expires_in: timedelta)` (+ `sign_async` per `:88`); `:107` `list(..., chunk_size=50, return_arrow=False) -> ListStream`; `:99` `get_ranges(store, path, *, starts, ends, lengths) -> list[Bytes]`; `:94` `head(store, path) -> ObjectMeta`; `:116` `open_reader(store, path, *, buffer_size, size) -> ReadableFile`; `:52` `GetResult.stream(min_chunk_size)`.
- `apscheduler.triggers.combining.AndTrigger`/`OrTrigger`, `calendarinterval.CalendarIntervalTrigger` import OK — `libs/python/runtime/.api/apscheduler.md:37` `CalendarIntervalTrigger`; `:38` `AndTrigger`; `:39` `OrTrigger`; `:118` constructors; `:114` `CronTrigger.from_crontab`; `:65` `JobExecutionEvent(...)`; `:144` `EVENT_JOB_EXECUTED/_ERROR/_MISSED`.
- `watchfiles.Change.raw_str()` = True (`Change.added.raw_str()` -> `'added'`) — `libs/python/runtime/.api/watchfiles.md:22` `Change.raw_str()` lowercase member name; `:53` `awatch(*paths, watch_filter, debounce=1600, step=50, ...)`; `:37` `PythonFilter`; `:58` `awatch` async generator.
- `httpx.Auth` = True, `.BasicAuth` = True, `.Timeout` = True, `.Limits` = True — (folder `httpx.md` roots-slice; `BasicAuth` is the forward `CREDENTIAL`-shape row per brief:188; `http2=` client flag is a native `httpx.AsyncClient` kwarg).
- `asyncssh.SSHClientConnectionOptions` = True, `.read_known_hosts` = True, `.SSHKnownHosts` = True — `libs/python/runtime/.api/asyncssh.md` connection-options slice.
- `anyio.to_process.run_sync` = True, `.to_thread.run_sync` = True, `.to_interpreter.run_sync` = True — `libs/python/.api/anyio.md:219` `to_process.run_sync(func, *args, cancellable=False, limiter=None)`; `:214` `to_thread.run_sync(func, *args, abandon_on_cancel=False, limiter=None)`; `:221` `to_interpreter.run_sync(func, *args, limiter=None)`; `:40` `CapacityLimiter`; `:165` `move_on_after(delay, shield=False)`; `:104` `BrokenWorkerProcess`; `:105` `BrokenWorkerInterpreter`.
- `expression.collections.Block.partition` = True, `.choose` = True — used by `lanes.md:122-123` drain split-fold.

## BRIEF SEAM / RIDER ANCHORS (`RASM-PY-RUNTIME-BRIEF.md`)

- IMPORT_DAG_LAW `:38-45`: topological order `faults < clock < identity < receipts < metrics < resilience < admission < telemetry < {reproduction, evidence, roots, shapes} < wire < lanes < recipe < serve`; `_pb2` package imports nothing intra-package, sits immediately below `shapes`.
- V2 WIRE_VOCABULARY_OWNER `:73-75`: NEW `transport/shapes.md` owns all 16 `msgspec.Struct` shapes, `FaultDetail` 9 fields (`package/code/case/message/evidence/correlation/hlc_physical/hlc_logical/tenant`) promoted from `wire.md:102` prose, the 64-bit proto3-JSON decimal-string `Meta` contract, the `_pb2` codegen seam (C# `.proto` -> `channels_pb2` via `grpcio-tools`, `descriptor_pool`-backed drift gate), AND `_PROTO_VOCABULARY` (today `wire.md:164-183`) + drift gate — a wire-resident registry would force a `shapes -> wire` back-edge. Geometry `TessellationRequest`+response/receipt rows land as registry rows (geometry `mesh/serve` demanding consumer). `wire.md` NESTED framing (`:344-371`) demotes to a proven-need row, FLAT sole realized path, empty `[05]-[RESEARCH]` (`:392`) deleted.
- V5 BASE_PURITY `:87-89`: `WORKER_BAND` (`lanes.md:90`, declared/unreferenced, sibling-enumerating comment) ruled WIRED not deleted — `offload` gains isolation-modality axis (interpreter | process | thread) with `WORKER_BAND` the process-modality process-wide native-worker bound; charter rewritten parameterized (seven recited sibling interiors die); THREAD modality carries its own process-wide band; `offload` accepts optional `retry: RetryClass`. Named demanding consumers: geometry `mesh/daemon` (OCCT interpreter retry), geometry graph analytics (THREAD band collapses three `CapacityLimiter(4)`), compute (PROCESS modality for JAX x64 global flag), artifacts (THREAD/process band, three `CapacityLimiter(4)` collapse).
- V8 GENERATOR_LAW `:49`: `type Trigger` closes at 3 of apscheduler's 6 (`lanes.md:58` vs `apscheduler.md:37-39`) under Growth prose claiming "one Trigger row"; instrumentation-scope literals (`"rasm.wire"` `wire.md:39`) mint per page with no scope vocabulary (`[V8]` owner: `faults` `SCOPES`/`Scope`).
- V12 NAMING `:117`: `roots`' `_object_client` renames `_transport_client` off its false object-store affinity.
- V14 FAULT_ROUND_TRIP `:123-125`: `FaultDetail` produced/consumed by zero fences; `settle` egress projects `BoundaryFault -> FaultDetail`, encodes through `fault_detail` registry row, sets `grpc-status-details-bin`; the invoke ingress decodes the trailer — serve's obligation, `fault_detail` registry row is `shapes`/`wire`-resident vocabulary.
- V11 PAGE_GRAMMAR `:111-113`: `[RESEARCH]` appendix BANNED (`roots.md:336`, `wire.md:392` empty); load-bearing `.api` anchors fold into `Packages` blocks; invariant-once dedupe; index-vs-body numbering aligned (`wire.md:15-17` vs `[02]`-`[04]`).
- BUILD_LEGS leg 3 `:204`: TRANSPORT/EDGE + GOVERNANCE — NEW `transport/shapes.md` FIRST, then `roots.md` (cold), `wire.md`, `lanes.md`, NEW `execution/recipe.md`, `serve.md` LAST.
- PACKAGE_PRESSURE `:180-181,187-188`: apscheduler — complete trigger union (`CalendarIntervalTrigger`/`AndTrigger`/`OrTrigger`); watchfiles — `Change.raw_str()` receipt serialization, `debounce`/`step` batching as `watched`-case data not baked defaults; obstore — runtime read-slice (`get_async`/`list`/`sign_async`/streaming; write/conditional/`PutMode` stay data-folder); httpx — custom bearer `auth_flow` stays, `BasicAuth` forward row, drop `NetRCAuth`/`DigestAuth`.

## FOLDER-CONTEXT ANCHORS (planning fences)

- `wire.md:116-119` phantom import `from rasm.runtime.shapes import (16 Structs)`; `wire.md:164-185` `_PROTO_VOCABULARY`/`WIRE_REGISTRY` (MOVE to shapes); `wire.md:190-192` `codec(name)` resolver (stays in wire, imports registry); `wire.md:102` `FaultDetail` 9-field prose (promote to shapes Struct); `wire.md:342-390` `_Nested`/`Framing.NESTED`/`_unnest`/`_tagged`/`_stream` NESTED machinery (demote); `wire.md:224` `type WireU64 = Annotated[int, Meta(ge=0)]` decode-floor law; `wire.md:44-94` `Decode` railed/routed/acquired aspect (preserve — brief `:150` sound surface).
- `roots.md:254-260` `@cache _object_client(endpoint)` (rename `_transport_client`, `:219`/`:258` call sites); `roots.md:104-105` `TRANSPORT_TIMEOUT`/`TRANSPORT_LIMITS`; `roots.md:255-260` `httpx.AsyncClient(...)` construction (no `http2=`); `roots.md:263-272` `drain()` teardown; `roots.md:336` `[03]-[RESEARCH]` appendix (delete per V11).
- `lanes.md:58` `type Trigger = CronTrigger | IntervalTrigger | DateTrigger` (3-of-6); `lanes.md:82-88` `WORKER_BAND` declaration + sibling-enumerating comment (kill enumeration, wire as PROCESS band); `lanes.md:139-157` `offload` single-modality (`interpreter_run_sync` only) — MUST gain `Modality` enum + `to_process`/`to_thread` legs; `lanes.md:36` `from rasm.runtime.receipts import DrainReceipt, Receipt, Redaction, Signals` (taxonomy re-home LANDED); `lanes.md:71-74` `LaneSource` `scheduled`/`watched` cases (surface `debounce`/`step`, `Change.raw_str`); `lanes.md:295` `[03]-[RESEARCH]` appendix (delete).
- `recipe.md:36` `from rasm.runtime.lanes import Admit, LanePolicy, Modality` — PHANTOM from lanes' side: `Modality` must be exported by `lanes`; `recipe.md:196,219` `self.lane.offload(_staged/_execute, spec/staged, modality=Modality.THREAD)` — the named demanding consumer proving the THREAD modality axis.
- `serve.md:158-174` `dispatch` generic over the 16 shapes; `serve.md:129-140` `settle` (V14 egress site, serve leg); `serve.md:94-103` `_FAULT_STATUS` `Map[FaultTag, grpc.StatusCode]`.
- `admission.md:47` `from rasm.runtime.clock import CausalFrame`; `admission.md:212-218` `SecretBoundary.resolve` @overload consuming into roots `ssh`/`http` legs.
- ARCH seam ledger (`libs/python/runtime/ARCHITECTURE.md`): `transport/wire ⇄ Rasm.Compute/Runtime [WIRE]: WireProtoCodec PROTO_VOCABULARY transcode`; `transport/wire ⇄ Rasm.Persistence/Version [WIRE]: CrdtOp MessagePack union decode`; `execution/admission ← Rasm.AppHost/Runtime [WIRE]: CredentialPem` (V4 drift: re-files to `transport/serve`); `transport/roots ⇄ Rasm.AppHost/Runtime [TRANSPORT]: TransportResource HTTP/SSH`. Governance-tail owns ARCH corrections; leg-3 tail adds the `shapes` node.
