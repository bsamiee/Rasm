# [RASM_APPHOST_RULINGS]

`Rasm.AppHost` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `System.Threading.Tasks.Dataflow` resolves from the `net10.0` shared framework — a manifest row re-admits what the SDK ref pack already ships.

## [02]-[SHAPE]

- Thermal and power state grade on the ONE `Pressure` contributor row — a thermal-only `DegradationLevel` forks the axis into two graders.
- Process utilization homes at `PressureSource`, never `ResourceMonitoring` — that package returns on darwin and grades a permanent zero there.
- `PowerAuthority` rows REFUSE an unlanded read rather than synthesize one — a plugged-nominal triple reads as measured at `FidelityScale.Grade`.
- `HostInstruments` declares the AppHost `BoardPack`, not `Observability/health` — `PanelSpec.Admit` resolves against `InstrumentSet`, not a reading.
- Durable OTLP egress owns the TRANSPORT alone — the exporter's persistence handler is `internal`, so arming both double-owns a batch.
- Durable OTLP handlers override BOTH `Send` and `SendAsync` — the export client calls the sync leg, so an async-only handler drops out unseen.
- `PersistentOtlpHandler` stores the BODY alone and replays headers off the live request — no credential reaches disk and rotation applies tail-wide.
- OTLP queue storage reads the deploy-declared volume, never `LocalStore` — that capability answers document storage, false where export runs.
- Propagated tenant baggage is trust-graded through `TenantAdoption`; authenticated webhook authority comes from the roster-resolved `Principal`.
- `ThrowOnUnregisteredNames` and `KeyedLane.Proven` close their rosters at composition — an unregistered name drops writes on a positionless token.
- `GovernanceRuntime` carries `IServiceProvider`, not `IServiceCollection` — request-time capability, so a self-registering pipeline forks it.
- Progress rides one `Option<SubscriptionPolicy>` column `CommandAlgebra` seats verbatim — an `IProgress<T>` thread manufactures an unchosen posture.
- Classification federation rides `TelemetryContributorPort.Classifications` alone — a roster beside the ports proves pairs no port declared.
- Committed op-log rows mint envelopes through Persistence; `RelayEntry` retains each exact envelope as the in-process dispatch carrier.
- Outbox settlement status is store-local text projected once from `RelayState` — no peer enum publishes internal cursor state.
- `InHost` capsules open no keyed `HybridCache`, framing entries `CacheLane.Capsuled` — an L2 row outlives the serializer its own unload disposed.
- Dispatch seats name a `WorkLane` and take the class `LaneClass` derives from its `Rank` — a supplied `DeadlineClass` contradicts its work lane.
- `SupportManifest.Entry` keys the FINAL written bytes and clears to absent on an empty arm — an earlier key names bytes no reader can extract.
- Support evidence is process-local — `SupportManifest` owns archive storage; no support-evidence protobuf or peer locator exists.
- Every external edge is one `TransportRow` over `ExternalTransport` — a per-protocol adapter forks read shape, hop, and echo class into rosters.
- Closed consumption axes seat once and pages compose them — `DeploymentTopology` and `Isolation` are those seats, and re-spelling either forks it.
- Open-axis rows answer `Fits`/`Tenancy`/`Lifetime` with `admit` on the family lead — a row answering elsewhere forks the cross-family coordinate.
- Consumption-descriptor forfeits DERIVE as the held set's complement — a `Degrade` column beside `CapabilitySet` gives one fact two owners.
- Host-integration capability rides `CapabilitySet<HostCapability>` — `Faculty` grades per reading, `HostCapability` fixes at the integration.
- Energy grades ONCE at `FidelityScale.Grade`, and `EnergyCell` publishes no thermal accessor — a rank read beside it is the second grader.
- `ResilienceSeries` is the ONE seat for the Polly meter's tag keys — `ResilienceTelemetryTags` is internal, so a second spelling partitions nothing.
- Served gRPC planes arrive as `ServedPlane` rows the root binds — naming a store type here is the S1-to-S2 edge the strata forbid.
- In-process transport evidence is one native family riding `SuiteContracts.Host` — no protobuf or peer claim mirrors a same-process fact.
- Delivery loss accounts by CONSERVATION — the topic gap and drain residual update the producer's loss tally, never an interceptor.
- Fan legs running no hop carry NO verdict — `DeliverySettled.Outcome` is `Option<HopVerdict>`, `None` on suppression and `Some(Refused)` on unbound.
- Delivery dedupe seats ABOVE the fan and admits once per message — a per-channel seat re-admits one message on every leg it fans to.
- Scope ceilings GRANT an opening balance at `GrantBroker.Open` and cross no debit — a unit the ceiling omits holds zero and refuses its first draw.
- Spend derives `Ceiling - Remaining` and stores nowhere — `grant.spend` projects off `CommandResult.Charged`, and a second stored vector drifts.
- Inherited allotments fold their `min` at `ControlInbound` alone — the root spends the budget as given, a second fold invisible at both seats.
- Unreachable policy cells read the value their reachable overrides select — a third answer no path can pick reads as live policy to every operator.
- Foreign-measurement quality rides the kernel `Quality` union at every decode — a `bool Good` beside a `0d` fill forges the not-good reading.
- Peer modality rides `CapabilitySet<ModalityCapability>` under its declared legal corners — adjacent bools spell peers nothing can serve.
- Keyed registries mint through a `[ValueObject<string>]` owning its head — bare-string keys lose the namespace proof and collide registries.
- Paired fields measuring one dial ride ONE `Option` — a zero attempt count beside a zero elapsed span forges a measurement no attempt produced.
- Eviction takes ONE seat at `QuotaControl.Evict` with `EvictionCause` naming the trigger — a second kill path duplicates the producer's result.
- Keyed limiter sets ride `PartitionedRateLimiter.Create`, not a dictionary — one heartbeat evicts idle partitions, and Polly disposes none.
- Conditional capability corners refuse at the owner's `Fin` mint AFTER the discriminant is set — `CapabilityLaw` states unconditional corners.
- Bim design-regime election seats ONCE at `Runtime/modules#MODULE_LEDGER` — a second election beside `SafEmit.Export` forks the national annex.
- Schedule-port registration is ONE composition-owned keyed arrow at `Runtime/modules` — idempotent by key, first writer arming, a later replacing.
- Consumer runtimes carry the `Func<ScheduleEntry, IO<Unit>>` column — a constructed-but-unregistered entry never escapes its acquiring fold.
- `FaultWire.Observe` is the sole AppHost fault projection — one projection keeps every boundary value on the kernel-derived bounded observation.
- `WireAdmission` owns one warmed contract validator over its descriptor roots; binary and ProtoJSON codecs compose it after parsing.
- Per-validation evidence is a CLOSED two-case family — refusal carries elapsed, correlation, and fault codes alone; sentinel principal slots delete.
- Sink tenancy is never a validation-result column — tenancy resolves at the sink, so a mirrored column states a second answer to one question.
- Capability discovery is generated `capability` from C# to Python and bound to the SDK pin — hand DTOs and TS generation are refused.
- Webhook delivery consumes `PolicyDescriptor.WebhookDelivery` — an endpoint-local scope check forks the identity policy vocabulary.
- Authorization policies consume canonical claims rebuilt from `Principal.Scopes` — provider scope names and shapes terminate at issuer projection.
- Webhook verification and format decode consume one bounded immutable body capture — stream rereads can verify bytes different from those decoded.
- Absence encodes ONCE at the `SuiteContracts` merge — every optional slot omits, so a `| null` peer face binds a token no producer emits.
- Wire value projections carry the identity they measured — an envelope routing coordinate keys no board fold and strands the decoded value.
- Capability argument schema rides its descriptor; native rows resolve generated metadata and federated tools retain the SDK document verbatim.
- Redaction covers the log and support-export path alone — a wire identifier a peer SELECTS on crosses intact; a redacted key matches nothing.

## [03]-[COLLAPSE]

- Metric views collapse to ONE `AddView` predicate over the roster — every match mints a stream, so a named row beside a wildcard double-exports.
- `AlertEngine` and the kernel SLO algebra evaluate disjoint evidence — the engine folds `DegradationReading`, burn compiles from an `Objective`.
- `Idempotency` and `HopIdempotency` stay two owners on KEY REGIME — the op roster carries a `KeyRegime` column the hop roster has no reader for.
- Store-side and port-side fault unions stay TWO-FORMED across the decode boundary — each keys its own band, and neither strata references the other.
- Ambient scope is the kernel `AmbientSlot` alone — a page-local `AsyncLocal` beside a hand restore scope forks the bound, restore, and refusal.
- One policy cut serving a count, a content key, and a payload is ONE projection — three `Min` spellings describe three slices, each reading right.
- Chaos arms through ONE `ChaosArming` gate as a runtime value — a build-time fork makes pipelines no operator can arm and seats the ambient draw.
- Acquire, renew, guard, and release are ONE algebra on a namespaced key — per-consumer runtime, holding, and status are three copies of one lease.
- Dependency capsules carry a `Func<>` column only where the provider is a PER-CALL effect — a pure projection is a member, a handle a typed port.
- Two capsules sharing over half their columns COLLAPSE, the survivor naming its discriminant — composition-time weave against per-call drive is one.
- Solver contracts ride `SolverKind` row columns, `Progress` and `Rank` included — a parallel contract record re-splits one roster's join.
- Hook points consume the producer's typed fact directly; a parallel generic result stream duplicates the fact stream.
- `AppHostPoint` is the folder's ONE hook roster, `AppHostFact` its closed fact union — a page folds its family onto one point via its signal union.

## [04]-[STRUCTURE]

- `AddApplicationLogEnricher` does NOT re-enter — its unqualified `service.name` and `deployment.environment` break record-to-series joins.
- MCP serves the stateless revision — every session-scoped capability is `[Obsolete]` at the installed SDK, so sampling and frame replay delete whole.
- Router rows stay noun-led fragments — the row-grammar test reads CAD-host boundary, which a gRPC mount and an ASP.NET pipeline never cross.

## [05]-[PROCESS]

- (none)
