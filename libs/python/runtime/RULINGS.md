# [PY_RUNTIME_RULINGS]

`python/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

Sub-modules admit or refuse on their module-scope side effects, never the distribution.

- Broker client legs ADMIT here — a branch carrying the message envelope owner dials it, and Speckle and OPC-UA terminate .NET-side on SDK reach alone.
- `confluent-kafka` is the one Kafka client — librdkafka is the engine the C# and TypeScript legs dial, so a pure-Python client forks that floor.
- `cloudevents.core` is the admitted family and `cloudevents.v1` refuses — `v1.http.CloudEvent` checks a required-NAME subset and mutates past it.
- `pika` KEEP against its transitive `asyncio` — the eager adapter roster DEFINES a class and creates no loop, the ban governing module scope here.
- `confluent_kafka.aio` refuses — dialing `get_running_loop` and answering `asyncio.Future` pins composition the sync client leaves free.
- `protobuf-py` generated messages and codecs own proto binary and ProtoJSON — a msgspec twin or local JSON codec forks the peer vocabulary.
- Capability discovery admits static semantics only from the pin document; generated live rows add availability and estimates before invocation.
- `rasm.contracts.BodyAdmission` evaluates descriptor constraints at the Connect body boundary; runtime `protovalidate` serves non-Connect seams.
- `connect-python` and `protoc-gen-connect-python` refuse — `connectrpc`'s renamed predecessors ship no ASGI application; one dist owns the seam.
- `hypercorn` KEEP by capability — the bidi `Sync` leg needs HTTP/2 on the ASGI host and h2c on plaintext, and `uvicorn` speaks HTTP/1.1 alone.
- `grpcio` is a transitive alone — the OTLP gRPC exporter and GCP secret client dial it; a direct row re-opens the retired `grpc.aio` serve rail.
- `pyqwest` is `connectrpc`'s transitive and the one dial transport — `HTTPTransport.aclose` releases the sockets, so the composition owns it.
- Client telemetry rides the `connectrpc` `MetadataInterceptor` — clients dial through `pyqwest`, so the httpx instrumentor reaches no Connect leg.
- `grpc.health.v1` serves from the vendored `rasm.contracts.vendor.grpc.health.v1` emission — `connectrpc` ships no health surface.
- MessagePack carries the uncompressed positional op-log envelope, `crdt` alone decoding generated `CrdtOpWire`; compression changes no contract.

## [02]-[SHAPE]

- `CanonicalWriter` owns seed-zero peer framing; `memory` is little-endian hash input and `wire_bytes` is big-endian generated output.
- `measured`'s free `scope` carries the branch telemetry grammar — a bare package-prefixed scope entering the exported weave forks one namespace.
- `runtime` mints every branch bound; a sibling minting its own capacity, band, isolation, or probe constant forks the roster.
- Object-store absence crosses as `StoreFault.missing` and streamed bodies stay behind `StoreStream.pull`; a consumer catch aliases it off the rail.
- `Hlc` samples this branch's clock and merges every inbound stamp before `tick` — the semilattice converges with no privileged physical authority.
- Above-`int64` wire ints carry `ge=0` alone and rail the ceiling at decode — msgspec spells no such bound, while in-band `I63` rides `Meta(le=…)`.
- Producer `optional` scalars spell `T | None` while no-presence scalars keep their zero — that keyword discriminates, never felt nullability.
- Telemetry install fails BEFORE publish — `_pipeline` completes construction before `_commit`, since every OTel global is set-once, never unset.
- OTLP is the one log egress door and the console stays armed — its `ProcessorFormatter` is the only seam a foreign stdlib record reaches the wire.
- Log posture FOLDS, never partitions — the stdlib root roster is process-global, so requests meet at the strictest floor and egress arms as a union.
- Payload bounds ride a CHAIN row and wire-bound values coerce to shapes both renders keep — console and wire read one narrowed value.
- Log egress arms by chain MEMBERSHIP, never per event — `shared_chain` rebuilds at configure, so an unarmed process allocates nothing per line.
- Terminal interpreter doors belong to the log egress and CHAIN their predecessor — a replacement deletes an embedding host's own hook.
- Foreign `extra` mappings stay untrusted for every chain-owned key — `trace_context` is the sole writer and sole eraser of the correlation roster.
- OTLP gRPC is refused on every forked floor — no channel survives `fork()`, so `GRPC_ELIGIBLE` admits the SIDECAR alone and install clamps the rest.
- Metric-stream shaping is data at the instrument owner and SDK objects at the install root — only that root names a `View` or an aggregation.
- Metric cardinality bounds at two tiers — the allow-list closes the KEY axis and a tenant budget the VALUE axis, folding at `otel.metric.overflow`.
- Temporality preferences key SDK FAMILY classes — monotonic sums and the histogram ride DELTA, non-monotonic sums CUMULATIVE; an API key raises.
- Tag-partitioned counters carry terminal partitions alone — the admitted count is a receipt line, and one more dimension value doubles every sum.
- Cross-cutting metric dimensions carry no domain segment — a discriminant meaningful everywhere names no capability subject, riding `rasm.<key>`.
- Measure shape decides the instrument class — a per-event delta records synchronously, a level observes from a probe the EXPORT CYCLE calls.
- Declared aggregation intent picks the census row's kind — percentile, mean, and maximum ride one histogram whose data point already carries them.
- Evidence never sheds and its derived projection always may — the journal AWAITS a bounded intake while line and series ride a droppable fence.
- `Ledger` is async WHOLE, proven at the bind emitter identity binds beside — one sync member stalls the loop and an unchecked bind defers its raise.
- Subject index rides the fact and its row — export and erasure key on one `(tenant, subject)` composite, so both rights compose by construction.
- Journal writers mint the `Hlc` stamp at admission over any caller slot — identity covers the stamped payload, so two producers never share one key.
- Erasure destroys the key and `open` is TOTAL — a destroyed key folds to `Nothing`, `InvalidTag` stays a fault, sealed envelopes bind `SubjectKey`.
- Every instrument row proves its `DOMAINS` segment at IMPORT over the whole table — filtering an unrostered row in the census defers the failure.
- Tables keyed on a closed family prove TOTAL at the boot gate — a `try_find` absence or a defaulted status defers an unrostered member to first use.
- Wire ABSENCE rides `Option` at the decode seam — an unstated case inside the interior vocabulary makes consumers match a state no value holds.
- Wire-form legality is a DECLARED (form, release) row on the producer's token — narrowing the slot refuses the frame an additive release carries.
- Optional dimensions spell absence by OMITTING the key at every arm — an empty-string value identifies a series a board groups on and nobody fills.
- Supervision probes answer typed columns, WHICH ceiling tripped, and UNMEASURED — fences bind per COLUMN, so an all-refused reading actuates none.
- One `CapacityLimiter` publishes ONE occupancy probe concurrent brackets REFCOUNT — `Metrics.occupied` sums live probes and retires by identity.
- Caller payload validates HOST-side onto a typed rail before the crossing — a defect reaching the far floor raises where no fault lift can name it.
- Journal intake is TRI-STATE — an armed scope sends, retired custody refuses `config`, and an uninstalled one folds `Ok(0)` as a declaration.
- `BoundaryFault.wire` constructs only with a real protocol or status code — a registry miss lands `config`, a decoded-material gap `boundary`.
- `ServerHost` implements generated `Health.Check` alone and keys its serving map on slashless `WireService.path`; `Watch` stays support closure.
- Connect metadata admission remains per-call; body validation implements all four native rpc shapes and checks every request and response element.
- Constraint refusals carry structured `buf.validate.Violations` — request failures map to `INVALID_ARGUMENT`, response failures to `INTERNAL`.
- `FaultRecovery`'s throttled arm IS `google.rpc.RetryInfo` — `RecoveryCell.of` is its one mint and `advice` seats THAT instance as standard detail.
- `FaultDetail.case` is the emitting leg's row ordinal off the fault census, 1-based in declaration order — never a Connect code; unseated is zero.
- `transport/shapes` `REGISTRY` is the ONE descriptor registry — `Any`, ProtoJSON `@type`, and `ErrorDetail.value` resolve there alone.
- Selected served rosters are generated subsets — the census refuses an ungenerated row without manufacturing actors for support-closure methods.
- Every mount and dial reads one `WirePolicy` row per profile — `read_max_bytes` and the zstd-then-gzip roster; an unbounded mount is refused.
- Outbound requests pre-encode under `SERVE_ENCODE` ahead of any retried call — the client maps an encode raise to UNAVAILABLE and re-drives it.
- `MessageEnvelope.extensions` is the generated `event.Extensions`; `EXTENSION_ROWS` derives per `local_name` — a hand roster is the mirror.
- `MessageEnvelope.payload` is the Rasm `Raw | protobuf.Message | None` seam over the generic SDK envelope — `None` is reference-only `dataref`.
- Generated extensions re-enter CloudEvents codecs before mint — `ce_integer` is signed 32-bit even when its corpus scalar is wider.
- Modality-polymorphic entrypoints whose return shape varies with input carry one `@overload` per shape — an unoverloaded arm type-checks nowhere.
- `HostRow` extends the descriptor with `lanes`/`recovery`/`scratch`/`document` and refuses `tenancy` — a host separates no tenant, the profile does.
- `ProviderRow` extends with `supplies`, refusing `tenancy` and `isolation` — `ConsumptionProfile.admit` carries that crossing as axis evidence.
- `paho` drives socket-first on `loop_read`/`loop_write`/`loop_misc` — `loop_start`'s daemon thread outlives every cancel scope and joins nowhere.
- Binding rows foreclose `retry` and name their resilience class — a row holding its own curve makes effective attempts the product of two schedules.
- Structured-format rows bind optional batch codecs: JSON/protobuf have batch forms, Avro does not; binary selects no suffix.
- Binding rows decline batch send until a bounded producer returns one custody result per event.
- One `BINDINGS` row owns carrier and execution facts; no adapter table or prefix literal shadows it.
- Media admission parses the complete MIME value once; malformed parameter tails never fall through as binary.
- Rasm structured rows declare payload arms: JSON/Avro carry opaque bytes; protobuf also carries generated `Message` through `Any`.
- Generic Avro codec preserves the publisher's recursive JSON-value union; profile admission narrows later and edits no codec.
- Generic `EventFormat.write`/`decode` re-mint through v1 URI admission; `encode`/`admit` add the Rasm profile.
- `CloudEventUri`/`CloudEventUriRef` retain unknown Protobuf attribute arms without replacing the SDK envelope.
- `Source` states domain/capability independently from `EventType`; Rasm composition proves domain agreement only, never capability/subject aliasing.
- Message-envelope `subject` carries `WireKey`; generated `dataref` carries the external residence URI-reference.
- Residence confines `dataref`, acquires through `ObjectStoreLane`, verifies `subject`, and compares dual data.
- Dual residence becomes reference-only only with admitted `datarefprojection=reference` negotiation evidence.
- One BROKER retry class serves every binding — Kafka rides its `retriable()` verdict, taxonomy-only families matching their narrowest transient arm.
- `Recovery` crosses every retry reader as a VALUE, lowering to `bool | float | timedelta` at stamina's `on=` alone — a bool reads `0.0` as refusal.
- `Recovery` states WHETHER a refusal re-offers and `Reoffer` states HOW — a terminal satisfiable NARROWED lands a value, never prose.
- Failure windows key the dependency INSTANCE and settle circuit and rate INSIDE the retried unit — only TRANSIENCE trips, a stated window re-seats.
- Classes declaring a `CIRCUIT` or `RATES` row REFUSE a dial naming no peer — one arc over a row's destinations sheds a peer that never fell.
- Peers name an ORIGIN through the `roots` `origin` fold — a raw href splits one arc per object, and a DSN publishes its password as the key.
- `RateGate` WAITS and never refuses — the caller's deadline is the one ceiling, and two refusals over one queue disagree the moment either moves.
- Every settlement joins the durable write it stands for — automatic commit acknowledges what a crash then loses, the loss invisible at both ends.
- Prefetch sizes WITH the broker lane's limiter, never above it — an unmatched window buys latency the broker lane then pays as unbounded memory.
- Event ingress takes tenancy from the authenticated `PrincipalScope` beside the provider delivery; adoption proves each arm against its grant set.
- Event classification is explicit at every crossing — missing or unknown `dataclassification` refuses before lowering, trust, routing, or settling.
- Unbound trust tables are CLOSED — a forgotten binding surfaces as a refusal an operator sees rather than an authorization hole nobody observes.
- Grades above their issuer's ceiling REFUSE and never downgrade — silently lowering publishes the fact onto every binding the lower grade admits.
- `OpLogEntry` tracks the producer record WHOLE — a short positional wire shifts every column past the truncation into a type-checking neighbour.
- Its payload is the decoded MessagePack bin value — `msgspec.Raw` also retains the bin header and therefore changes the payload digest.
- `OpLogCodec` owns exact envelope arity and all-lane content-key admission; `CrdtOpCodec` opens only the generated CRDT payload.
- Transfer includes the root content key and a unique, strictly sorted closure; descendants alone omit the payload the entry names.
- Frozen wire slots, ORDER, and numbers survive an interior-owner re-seat — the derived view re-points in ONE unit, a tear stranding every peer.
- Every convergence column publishes its own read on its owning shape — a write-only column converges a state no replica can project back out.
- CRDT materialization keeps `(OperationId, CrdtOpWire)` paired — identity-free replay loses redelivery, causal-write, and maintenance authority.
- Multi-value writes retain outer dots, observed-set tombstones key by element, and RGA compaction retains value-free routing tombstones.
- One CRDT field seats in one family; unseated or multiply seated maintenance refuses rather than guessing a family.
- Presence retains stamped live/left cells with its maintenance horizon; a pre-horizon replay never resurrects.
- PN sequence ties repeat both cumulative halves, and RGA identity reuse repeats predecessor and value identity or refuses.
- Log-record exception semantics stay the SDK's — the emit seam takes the RAISED OBJECT, so the chain resolves `exc_info` to it before the renderer.
- `BoundaryFault` grows by ingress class or case alone — a sibling fault type needing an adapter breaks the cross-tier `combine`/`aggregate` fold.
- Instrumentor rows carry the driver they wrap and gate on presence before reifying — a contrib instrumentor imports its driver at module scope.
- Conformance rows PROJECT off the deciding member and the key roster proves TOTAL at import — a dropped row emits as a branch legislating none.
- Withheld conformance seats carry the value beside the pin — a plane with no member for a ceiling it honors reads as a dropped row without one.
- Row shape and the `Leg` contract seat at the fault root while each folder mints its own roster — a subject derives from its leg, never a literal.
- `catch` is REQUIRED on every fault-lift shape — a defaulted `Exception` makes the banned bare funnel the cheapest form at every call site.
- Providers publishing no typed refusal absorb into ONE stated fold re-raising a NAMED set — widening `catch` to `Exception` re-opens that funnel.
- Raised sibling refusal tokens cross as `BoundaryFault.domain` WHOLE — `Exception.__str__` renders EMPTY for a kwarg-only union and fuses its cases.
- Fault egress and ingress are total TOGETHER — a peer's decoded conflict mints a typed token onto `domain`, never a concatenated subject string.
- Retriability resolves in ONE predicate — a rostered raise answers its own `FaultRow` posture, every other fault deriving from its `FaultTag`.
- Walk bounds ride ONE `Depth` at the fault root — walk-to-fixpoint is a NAMED case, and exhaustion rails a typed fault rather than truncating.
- Foreign-edge absence rides ONE `Posture` at the fault root — `defaulted` NAMES its source, so no sentinel fuses a read fact with a fabricated one.
- Per-SEAM re-drive verdicts bind their own `stamina` caller off its class `POLICY` row — `guard` stays per-CLASS, its `@cache` admits no seam hook.
- Hook subscription answers the DETACHER and a scope releases WHOLE — a count retires nothing and an undrained table outlives its own composition.
- Isolation emits behind its OWN fence and parks the verdict — an unfenced sink destroys what it isolates and a silent eviction reads as no loss.
- Rostered raises seat in ONE fault census — a per-module `DETAILS` fold builds a map `retriability` and `facts` never read.
- Receipts settle on ONE six-column spine bearing absence in key, provenance, band, and stamp — a required slot makes every producer forge one.
- Fault span and log attribute keys roster at the fault root — a per-page literal forks the vocabulary the C# and TypeScript ends both publish.
- Deadlines construct through ONE fold over a DECLARED unknown-budget floor — a per-site zero fuses an unmeasured bound with a real zero.
- Typed band tokens cross a worker as DATA and re-mint parent-side — a kwarg-only `@tagged_union` reconstructs through no pickler, raised or railed.

## [03]-[COLLAPSE]

- Content-key elision and RFC-9111 revalidation stay two owners — `lanes` cuts before transport, HTTP revalidates at it, acquisition reads key first.
- Survivor and ordering decisions read `evidence/clock#CLOCK`'s `compare` and fold its `Ordering` — two spellings drift when either bound flips.
- Trace spans partition by `SpanKind` at its four boundaries — a unified aspect mis-kinds them and re-parents the serve interceptor's spans.
- `loky` and `pebble` stay two executors — crash-respawning warm reuse against terminal wall-clock kill, and neither carries the other's guarantee.
- Stdlib-handler bridging collapses into the chain's terminal row — a handler reads a rendered record, carrying neither redaction nor caps.
- Shutdown closes the intake and awaits the drain, retiring custody — a drain-deadline parameter re-threads the cancellation the scope already owns.
- ONE row-driven `BrokerLane` owns every connection; one state agent serializes immutable custody and event-driven drain.
- Provider coordinates own frames; `(source, id)` alone owns deduplication and journal verdicts.
- Expired deliveries remain frame-bound, bypass durable recording, and settle under a distinct `MOOT` receipt.

## [04]-[STRUCTURE]

- `LogLimits` stays a page-owned policy shape — importing the SDK record-limit type reifies the cold `sdk._logs` tier in every composition root.
- One instrumentation-scope triple homes at `reliability/faults#FAULT` — `identity`, `clock`, and `wire` open spans below every observability owner.
- Scope handles mint once per module, never per call — the API caches none, and the pre-install proxy upgrades at install with no invalidation.
- `workers` stays one module over crossing, pool, and supervision — a split forces the closed `WorkerKind`/`KernelTrait` vocabulary across a seam.

## [05]-[PROCESS]

- Blocking native waits ride the library's OWN timeout in the thread — a cancel scope bounds the await while the abandoned thread keeps its token.
- Secret resolution stays ONE sync provider union crossed through `anyio.to_thread.run_sync` under its own probe band — `hvac` ships no `aio` twin.
- Rebalance callbacks record a delta and start NO work — library work on the client's thread under its lock is the deadlock the portal forecloses.
- `loky`'s tracker skew closes at `_use_simple_format` before pool construction — its child spawns across an exec seam no parent rebind crosses.
- Drains FLUSH and never cancel — a cancelled in-flight window loses exactly the facts acceptance already promised against.
- Every `Metrics.record` lands with its `INSTRUMENTS` row — `MEASURES` raises on an unregistered pair, so a dangling record kills its producer.
- Structure proves first at every boot — vocabulary and layout gates seat ahead of any install claiming process custody, and read no installed state.
