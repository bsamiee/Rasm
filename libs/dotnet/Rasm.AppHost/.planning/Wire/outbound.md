# [APPHOST_OUTBOUND_RESILIENCE]

Nine `OutboundHop` cases bind to frozen `HopPolicy` rows on the runtime spine, each deriving its strategy posture from one `HopAllotment` and stating what it can do as one `CapabilitySet<HopCapability>`. Every hop holds exactly one retry surface — the standard or hedging HTTP handler on `SocketsHttpHandler`-borne rows, one keyed result-typed Polly pipeline per non-HTTP row. Admission folds the degradation gate, the profile exclusion, and the retry-owner claim onto one `Fin` rail, and every dispatch exits as one native `HopReceipt`.

Settled composition: `CapabilitySet<TCapability>`, `ICapability<TSelf>`, and `CapabilityLaw<TCapability>` arrive from `Rasm/Domain/validation#CAPABILITY`; `Retriability`, `RedrivePolicy`, and `Redrive` from `Rasm/Domain/rails#REDRIVE`; `Transition<TState>` and `Cell.Claim` from `Rasm/Domain/rails#TRANSITION`; `FaultBand.Hop` from `Rasm/Domain/rails#FAULT_BAND`; `MonotonicTimeline` and `GaugedSpan<TLane>` from `Rasm/Parametric/projections#TIMELINE`; `ReceiptSinkPort`, `ReceiptEnvelope`, `TelemetrySource`, and `TenantContext` from `Rasm/Domain/frame`.

In-folder composition: `Faculty` and `DegradationLevel` from `Observability/health#DEGRADATION_RAIL`; `ReceiptKind` from `Observability/instruments#RECEIPT_PROJECTION`; `AppHostPoint`/`AppHostFact` and `HookRail` from `Observability/hooks#HOOK_ROSTER`; `ClockPolicy` and `DeadlineClass` from `Runtime/time`; `DedupeWindow` from `Runtime/resources#DEDUPE_WINDOW`.

Owned surfaces: the hop axis, both pipeline registries, the ownership law over the Polly, Http.Resilience, and Grpc.Net.Client spine, the transport chaos posture `Runtime/determinism#ADVERSARIAL_PROBE` records as `ChaosDecision` entries, and the discovery manifest, UDS attach, contract-generation gate, and companion-spawn lifecycle seating the `LocalIpc` hop case.

Boundary is the process seam rather than the dial — two store-rail cases hand their bytes to a provider SDK or a cluster driver, and cross exactly the seam the dialed cases cross.

## [01]-[INDEX]

- [02]-[HOP_AXIS]: Nine hop cases bound to frozen policy rows deriving one allotment, with total dispatch.
- [03]-[HTTP_PIPELINES]: Standard and hedging handlers for `SocketsHttpHandler`-borne rows.
- [04]-[KEYED_PIPELINES]: One keyed result-typed Polly registry, its strategy roster, and channel policy for non-HTTP hops.
- [05]-[OWNERSHIP_LAW]: One retry owner per hop claimed at boot, with the outcome fold, conflict evidence, and receipts.
- [06]-[DISCOVERY_ATTACH]: Manifest law, UDS attach, contract-generation gate, and companion child lifecycle.
- [07]-[DELIVERY_FANOUT]: Multi-channel notification fan-out, delivery receipts, and dedupe.
- [08]-[RECEIPT_FAMILY]: Native hop and delivery evidence consumed inside the process.

## [02]-[HOP_AXIS]

- Owner: `OutboundHop` `[Union]` nine sealed hop cases; `HopKey` `[ValueObject<string>]` the resilience-registry identity whose namespace head is unspellable outside it; `HopPolicy` per-case row record; `HopCapability` `[SmartEnum<string>]` realizing kernel `ICapability<HopCapability>` — the combinable posture axis with its unconditional corner law; `HopAllotment` the derivation every strategy knob reads; `HopRows` frozen row set with its roster, its composition-time corner proof, and the total dispatches; `HopIdempotency` keyless vocabulary; `HopTransport` keyless byte-mover vocabulary; `HopRateLimit` keyless admission-shape vocabulary; `HopDelivery` keyless delivery-honesty vocabulary — every row STATES its guarantee, never assumes one; `HopFault` `[Union]` fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Hop`) and its retriability through the kernel discriminant; `ReleaseIdentity` vehicle-free update identity.
- Cases: HttpApi, Grpc, ServerStream, CompanionSpawn, LocalIpc, WebhookPost, UpdateCheck, ObjectStore, WideColumn — the stream case is gRPC server-stream; UpdateCheck carries `ReleaseIdentity`; the two store-rail cases name their object by its RENDERED key rather than a typed address, because `ContentAddress` seats at the element seam this spine does not reference. `HopCapability` rows — `Replayable`, `Redialing`, `SelfLaunching`.
- Entry: `HopKey.Of<TCase>()` is the ONE mint of a registry key, taking the hop case as its type argument so free text cannot name one, and `HopKey.Named(string?)` the admission arm resolving a Polly-reported pipeline name back onto the vocabulary; `HopRows.Admitted` proves every authored row's conditional corner at composition; `HopPolicy Policy`, `Fin<Uri> Authority`, and `string IdempotencyKey` are extension properties over the case — total state-free `Switch` from case to frozen row, dial target, and dedup identity.
- Packages: Rasm (kernel `CapabilitySet`/`CapabilityLaw`/`Retriability`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one case and one `HopPolicy` row absorb a new outbound boundary; a new posture is one `HopCapability` row and its membership on the rows that hold it; the admission shape lands as one `HopRateLimit` key beside one `Admission` column value, never a second limiter rail; a new resilience law lands as one `HopPolicy` column read by one `HopStrategy` arm; zero new surface.
- Boundary: every strategy knob DERIVES on `HopAllotment` from the axis deadline pair and the row's `Trip`/`Floor` pair, so a numeric literal inside `AddRetry`, `AddCircuitBreaker`, or an admission row reconstructs what the column already carries and is the deleted form; the deadline pair is `DeadlineClass.HopAttempt`/`HopTotal` on the AXIS rather than two columns holding one value nine times — a hop needing its own class earns the column then and not before; a store whose callee owns transactional semantics is excluded from the hop law and retries on its own execution strategy — `[LAYER_SPLIT]` row `[01]` — while a remote object-store or wide-column call crosses a process seam and lands here on row `[02]`, so the two store-rail rows are the seam-crossing half of one split rather than a second store policy; the `Transport` column routes the row to its lane through `Piped`, so `HttpLane.Wire`, `KeyedLane.Register`, `KeyedLane.Proven`, and `OutboundSurface.Enforce` read ONE residence predicate and `StoreRail` joins the keyed lane through that same predicate with no arm added; the `Admission` column names the pipeline-head limiter shape on every row — `Concurrency` for the bounded-permit default, `SlidingWindow` for the webhook segment cap, `TokenBucket` for the redial-paced peer hop — read by `HopStrategy.Admission`, never a second limiter selector; `Held` names the ONE combinable posture column and the two bool columns beside it are the deleted form, because the corners are not independent: `CapabilityLaw` states the unconditional one (a row holding both `Replayable` and `Redialing` names two lanes at once, since hedging arms only on the HTTP lane and the fallback redial only on the keyed one) while the CONDITIONAL one refuses at `HopRows.Admitted` after the discriminating `Idempotency` column is set — `Replayable` fans a body concurrently and duplicates its effect unless the row is semantically repeat-safe, which no flat corner list can read; `Hedges` therefore reads ONE capability, because admission already proved the idempotence half — `ObjectStore` is the sharpest instance: `Idempotent` earns it the retry while withholding `Replayable` withholds the hedge, since a hedged object PUT uploads the body twice; `SelfLaunching` is the exclusion axis — a row acting on the package's own process image cannot run where the host owns that process, so `Admit` reads one `ConsumptionProfile` axis value against one capability rather than carrying a predicate delegate no row can compare; the two store rows price against the classes they match rather than a fresh curve — a bulk, transient-tolerant object transfer at moderate concurrency takes the `WebhookPost` pair (`Trip: 0.3d`, `Floor: 32`), and a keyed, interactive wide-column read or upsert takes the `HttpApi` pair (`Trip: 0.2d`, `Floor: 64`); `Retries` folds `SingleShot` to a pipeline with no retry strategy at all, because `MaxRetryAttempts` refuses a zero and a spawn re-offered under a schedule forks a second child; the `Delivery` column is the honesty axis every row STATES — `BestEffort` (local-ipc, spawn, update-check: a lost frame is acceptable evidence loss), `AtLeastOnce` (webhook, http, server-stream: the retry schedule redelivers so consumers dedupe by delivery key), `ExactlyOnceEffective` (the wire-native gRPC hop the outbox drain rides: at-least-once transport + consumer dedupe by the Persistence CloudEvent operation id, the Persistence egress pump composing this exact column) — a hop whose guarantee is unstated and a claim stronger than the transport carries are both deleted forms; the row's `Needs` faculty is the degradation gate and the breaker group; retriability is the FAULT's, never the policy's — each `HopFault` case overrides the kernel `Retriability` virtual, so the retry predicate, the delay generator, and the in-process re-drive all read one discriminant and a per-policy classifier delegate is the deleted form; `IdempotencyKey` mints ABOVE the pipeline on the hop case itself — the webhook `DeliveryKey`, the gRPC `ContentKey`, the peer's pid-and-start pair — because a key minted inside the retried callback changes per attempt and defeats itself.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutboundHop {
    private OutboundHop() { }

    public sealed record HttpApi(Uri Authority) : OutboundHop;
    public sealed record Grpc(Uri Address, string ContentKey) : OutboundHop;
    public sealed record ServerStream(Uri Address) : OutboundHop;
    public sealed record CompanionSpawn(ProcessStartInfo Spec) : OutboundHop;
    public sealed record LocalIpc(DiscoveryManifest Peer) : OutboundHop;
    public sealed record WebhookPost(Uri Target, string DeliveryKey) : OutboundHop;
    public sealed record UpdateCheck(ReleaseIdentity Installed) : OutboundHop;
    // Store-rail objects cross as RENDERED keys: `ContentAddress` and every Persistence element type seat below
    // this spine's one reference, so bucket, region, and endpoint resolve inside the driver's own client.
    public sealed record ObjectStore(string Provider, string ObjectKey) : OutboundHop;
    public sealed record WideColumn(string Keyspace, string PartitionKey) : OutboundHop;
}

// Registry identity is a VALUE OBJECT over the closed case set, taking the hop case as its type argument, so
// registration, closure proof, pooled context, and Polly-reported refusal all read one spelling, and a
// `$"hop:{…}"` interpolation is unspellable rather than merely discouraged.
[ValueObject<string>(KeyMemberName = "Value")]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HopKey {
    private const string Head = "hop:";

    public static HopKey Of<TCase>() where TCase : OutboundHop => Create(Head + typeof(TCase).Name);

    // Polly reports a pipeline NAME as free text on its refusal exceptions; this is that text's one admission.
    public static Option<HopKey> Named(string? reported) =>
        Validate(reported, out HopKey? key) is null ? Optional(key) : None;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (!(value ?? string.Empty).StartsWith(Head, StringComparison.Ordinal)) {
            validationError = new ValidationError($"a '{Head}' registry key");
        }
    }
}

[SmartEnum]
public sealed partial class HopIdempotency {
    public static readonly HopIdempotency Idempotent = new();
    public static readonly HopIdempotency MethodDerived = new();
    public static readonly HopIdempotency Keyed = new();
    public static readonly HopIdempotency SingleShot = new();
}

[SmartEnum]
public sealed partial class HopTransport {
    public static readonly HopTransport SocketsHttpHandler = new();
    public static readonly HopTransport GrpcChannel = new();
    public static readonly HopTransport ProcessSpawn = new();
    public static readonly HopTransport StoreRail = new();
}

[SmartEnum]
public sealed partial class HopRateLimit {
    public static readonly HopRateLimit Concurrency = new();
    public static readonly HopRateLimit SlidingWindow = new();
    public static readonly HopRateLimit TokenBucket = new();
}

// ExactlyOnceEffective = at-least-once transport + consumer dedupe by the envelope operation id, never magic.
[SmartEnum]
public sealed partial class HopDelivery {
    public static readonly HopDelivery BestEffort = new();
    public static readonly HopDelivery AtLeastOnce = new();
    public static readonly HopDelivery ExactlyOnceEffective = new();
}

// Three postures, one combinable column. `Replayable` admits a CONCURRENT second body, which is a strictly
// stronger claim than the repeat-safety `HopIdempotency` states; `Redialing` substitutes a fresh dial after
// every attempt fails; `SelfLaunching` acts on this package's own process image.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HopCapability : ICapability<HopCapability> {
    public static readonly HopCapability Replayable = new("replayable", rank: 1);
    public static readonly HopCapability Redialing = new("redialing", rank: 2);
    public static readonly HopCapability SelfLaunching = new("self-launching", rank: 3);

    public int Rank { get; }
    static IReadOnlyList<HopCapability> ICapability<HopCapability>.Items => Items;

    // Hedging arms on the HTTP lane and the fallback redial on the keyed lane, so a row holding both names two
    // residences at once — an UNCONDITIONAL corner, which is the only kind a flat law states.
    public static readonly CapabilityLaw<HopCapability> Law =
        CapabilityLaw<HopCapability>.Forbidden(Seq(CapabilitySet<HopCapability>.Of(Replayable, Redialing)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ReleaseIdentity(string Product, string Channel, string Installed, Uri Feed);

public sealed record HopPolicy(
    HopKey Key,
    HopTransport Transport,
    Faculty Needs,
    HopIdempotency Idempotency,
    CapabilitySet<HopCapability> Held,
    HopRateLimit Admission,
    HopDelivery Delivery,
    double Trip,
    int Floor);

// Every strategy knob projects HERE off the axis deadline pair and the row's trip-and-floor pair, so a posture
// edit is one column and an incoherent knob pair is unconstructible. Each projection clamps against its own
// validator range — `MaxRetryAttempts` refuses below one, `MinimumThroughput` below two, `SamplingDuration`
// below 500 ms — because an eager `Add*` validation turns an out-of-range derivation into an unconstructible
// pipeline rather than a first-request fault.
public readonly record struct HopAllotment(TimeSpan Total, TimeSpan Attempt, double Trip, int Floor) {
    public const int ThroughputFloor = 2;
    public const int HedgeCeiling = 2;
    public const int WindowSegments = 6;
    public const int BackoffDivisor = 20;

    public int Attempts => int.Max((int)(Total / Attempt) - 1, 1);
    public TimeSpan Sampling => 2 * Attempt;
    public TimeSpan Break => Sampling / 2;
    public TimeSpan Backoff => Attempt / BackoffDivisor;
    public TimeSpan HedgeDelay => Attempt / 4;
    public int Permits => Floor * 2;
    public int Throughput => int.Max(Floor / 2, ThroughputFloor);
    public int Hedges => int.Min(Attempts, HedgeCeiling);
    // Half-open dwell grows with consecutive failed probes, the ONE per-execution datum this seat receives, so
    // a dependency that keeps failing its probe backs off in multiples of the sampling window it earned.
    public TimeSpan Dwell(int probes) => Break * int.Max(probes, 1);
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Retriability stays the kernel discriminant each arm declares.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HopFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Hop;
    private HopFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    public static HopFault Of(Error error) => error as HopFault ?? new Foreign(error);

    [FaultCase(0)]
    public sealed partial record Excluded : HopFault { public Excluded(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record Fenced : HopFault { public Fenced(string detail) : base(detail) { } }
    // Conflict evidence carries BOTH declaration symbols, because the losing claim degrades to the incumbent
    // and this arm is the only place that loser survives for repair comparison.
    [FaultCase(2)]
    public sealed partial record OwnerConflict : HopFault {
        public OwnerConflict(HopKey key, RetryOwner incumbent, RetryOwner loser)
            : base($"<owner-conflict:{key.Value}>") =>
            (Key, Incumbent, Loser) = (key, incumbent, loser);
        public HopKey Key { get; }
        public RetryOwner Incumbent { get; }
        public RetryOwner Loser { get; }
    }
    // Children race their own manifest publish, so this case names the one discovery refusal a bounded
    // in-process re-drive corrects.
    [FaultCase(3)]
    public sealed partial record StaleManifest : HopFault {
        public StaleManifest(string detail) : base(detail) { }
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(4)]
    public sealed partial record ContractBroken : HopFault { public ContractBroken(string detail) : base(detail) { } }
    [FaultCase(5)]
    public sealed partial record SpawnRejected : HopFault { public SpawnRejected(string detail) : base(detail) { } }

    // Rejection arms close the pipeline's own termination verbs, each binding its evidence as a typed field so
    // escalation matches the case and reads the field rather than re-parsing the wire detail the base carries.
    // `Foreign` passes the open tail through and `Empty` marks the no-exception-no-result sentinel.
    [FaultCase(6)]
    public sealed partial record CallerLeft : HopFault, ICausedFault {
        public CallerLeft(Error cause) : base("<caller-left>") => Cause = cause;
        public Error Cause { get; }
    }

    [FaultCase(7)]
    public sealed partial record Deadline : HopFault, ICausedFault {
        public Deadline(TimeSpan span, Error cause) : base($"<deadline:{span}>") => (Span, Cause) = (Duration.FromTimeSpan(span), cause);
        public Duration Span { get; }
        public Error Cause { get; }
        public override Retriability Retriability => Retriability.Transient;
    }

    [FaultCase(8)]
    public sealed partial record ForcedDark : HopFault, ICausedFault {
        public ForcedDark(Option<HopKey> pipeline, Error cause) : base($"<forced-dark:{pipeline}>") => (Pipeline, Cause) = (pipeline, cause);
        public Option<HopKey> Pipeline { get; }
        public Error Cause { get; }
    }

    // Both advised arms answer `Throttled` where the server named a window and `Transient` where it did not, so
    // one discriminant drives the retry delay generator and the in-process re-drive alike.
    [FaultCase(9)]
    public sealed partial record Open : HopFault, ICausedFault {
        public Open(TimeSpan? retryAfter, Error cause) : base($"<open:{retryAfter}>") =>
            (RetryAfter, Cause) = (Optional(retryAfter).Map(Duration.FromTimeSpan), cause);
        public Option<Duration> RetryAfter { get; }
        public Error Cause { get; }
        public override Retriability Retriability =>
            RetryAfter.Match(Some: Retriability.Throttled, None: static () => Retriability.Transient);
    }

    [FaultCase(10)]
    public sealed partial record Shed : HopFault, ICausedFault {
        public Shed(TimeSpan? retryAfter, Error cause) : base($"<shed:{retryAfter}>") =>
            (RetryAfter, Cause) = (Optional(retryAfter).Map(Duration.FromTimeSpan), cause);
        public Option<Duration> RetryAfter { get; }
        public Error Cause { get; }
        public override Retriability Retriability =>
            RetryAfter.Match(Some: Retriability.Throttled, None: static () => Retriability.Transient);
    }

    // Envelope refusal: the wire crossing itself was unsound — a malformed trailer, an unadmitted detail, a
    // contract violation — carrying the refusing boundary and the typed violation the boundary read.
    [FaultCase(11)]
    public sealed partial record Malformed : HopFault {
        public Malformed(WireBoundary boundary, WireViolation violation)
            : base($"<malformed:{boundary.Value}>") => (Boundary, Violation) = (boundary, violation);
        public WireBoundary Boundary { get; }
        public WireViolation Violation { get; }
    }

    [FaultCase(12)]
    public sealed partial record Foreign : HopFault, ICausedFault {
        public Foreign(Error cause) : base(cause.Message) => Cause = cause;
        public Error Cause { get; }
    }

    [FaultCase(13)]
    public sealed partial record Empty : HopFault { public Empty() : base("<empty-outcome>") { } }
}

// --- [POLICIES] -----------------------------------------------------------------------------
public static class HopRows {
    public static readonly HopPolicy HttpApi = new(HopKey.Of<OutboundHop.HttpApi>(), HopTransport.SocketsHttpHandler, Faculty.RemoteCompute, HopIdempotency.MethodDerived, CapabilitySet<HopCapability>.None, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Trip: 0.2d, Floor: 64);
    public static readonly HopPolicy Grpc = new(HopKey.Of<OutboundHop.Grpc>(), HopTransport.GrpcChannel, Faculty.RemoteCompute, HopIdempotency.Keyed, CapabilitySet<HopCapability>.None, HopRateLimit.Concurrency, HopDelivery.ExactlyOnceEffective, Trip: 0.2d, Floor: 64);
    public static readonly HopPolicy ServerStream = new(HopKey.Of<OutboundHop.ServerStream>(), HopTransport.GrpcChannel, Faculty.RemoteCompute, HopIdempotency.Idempotent, CapabilitySet<HopCapability>.None, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Trip: 0.5d, Floor: 16);
    public static readonly HopPolicy CompanionSpawn = new(HopKey.Of<OutboundHop.CompanionSpawn>(), HopTransport.ProcessSpawn, Faculty.LocalCompute, HopIdempotency.SingleShot, CapabilitySet<HopCapability>.None, HopRateLimit.Concurrency, HopDelivery.BestEffort, Trip: 0.5d, Floor: 4);
    public static readonly HopPolicy LocalIpc = new(HopKey.Of<OutboundHop.LocalIpc>(), HopTransport.GrpcChannel, Faculty.LocalCompute, HopIdempotency.Keyed, CapabilitySet<HopCapability>.Of(HopCapability.Redialing), HopRateLimit.TokenBucket, HopDelivery.BestEffort, Trip: 0.5d, Floor: 8);
    public static readonly HopPolicy WebhookPost = new(HopKey.Of<OutboundHop.WebhookPost>(), HopTransport.SocketsHttpHandler, Faculty.RemoteCompute, HopIdempotency.Keyed, CapabilitySet<HopCapability>.None, HopRateLimit.SlidingWindow, HopDelivery.AtLeastOnce, Trip: 0.3d, Floor: 32);
    public static readonly HopPolicy UpdateCheck = new(HopKey.Of<OutboundHop.UpdateCheck>(), HopTransport.SocketsHttpHandler, Faculty.RemoteCompute, HopIdempotency.Idempotent, CapabilitySet<HopCapability>.Of(HopCapability.Replayable, HopCapability.SelfLaunching), HopRateLimit.Concurrency, HopDelivery.BestEffort, Trip: 0.5d, Floor: 4);
    public static readonly HopPolicy ObjectStore = new(HopKey.Of<OutboundHop.ObjectStore>(), HopTransport.StoreRail, Faculty.RemoteCompute, HopIdempotency.Idempotent, CapabilitySet<HopCapability>.None, HopRateLimit.Concurrency, HopDelivery.ExactlyOnceEffective, Trip: 0.3d, Floor: 32);
    public static readonly HopPolicy WideColumn = new(HopKey.Of<OutboundHop.WideColumn>(), HopTransport.StoreRail, Faculty.RemoteCompute, HopIdempotency.Keyed, CapabilitySet<HopCapability>.Of(), HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Trip: 0.2d, Floor: 64);

    // Roster IS the registration set: every registration, proof, claim, and enforcement fold reads it, so a
    // caller-supplied subset is a knob that silently defeats the closure proof it feeds.
    public static readonly Seq<HopPolicy> Items = Seq(
        HttpApi, Grpc, ServerStream, CompanionSpawn, LocalIpc, WebhookPost, UpdateCheck, ObjectStore, WideColumn);

    // Corner proof at composition, accumulating: an authored row that never dials still refuses at boot with
    // its key named, and every offending row lands in one refusal rather than the first one alone.
    public static Fin<Seq<HopPolicy>> Admitted =>
        Items.Traverse(static row => Corner(row).Match(
                Succ: Validation<Error, HopPolicy>.Success,
                Fail: Validation<Error, HopPolicy>.Fail))
            .As()
            .ToFin();

    static Fin<HopPolicy> Corner(HopPolicy row) =>
        from held in HopCapability.Law.Admit(row.Held)
        from _replay in guard(
            !held.Admits(HopCapability.Replayable) || row.Idempotency == HopIdempotency.Idempotent,
            new HopFault.Excluded($"<replayable-without-idempotence:{row.Key.Value}>"))
        select row;

    extension(HopPolicy row) {
        public HopAllotment Allot(Func<DeadlineClass, TimeSpan> allotted) =>
            new(allotted(DeadlineClass.HopTotal), allotted(DeadlineClass.HopAttempt), row.Trip, row.Floor);

        // Residence, retry admission, and hedging admission each read ONE predicate, so the four registration
        // and enforcement seats cannot disagree about which lane a row lives on or which strategies it earns.
        public bool Piped => row.Transport != HopTransport.SocketsHttpHandler;

        public bool Retries => row.Idempotency.Switch(
            idempotent: static () => true,
            methodDerived: static () => true,
            keyed: static () => true,
            singleShot: static () => false);

        public bool Hedges => row.Held.Admits(HopCapability.Replayable);
    }

    // --- [OPERATIONS] -------------------------------------------------------------------------
    extension(OutboundHop hop) {
        public HopPolicy Policy => hop.Switch(
            httpApi: static _ => HttpApi,
            grpc: static _ => Grpc,
            serverStream: static _ => ServerStream,
            companionSpawn: static _ => CompanionSpawn,
            localIpc: static _ => LocalIpc,
            webhookPost: static _ => WebhookPost,
            updateCheck: static _ => UpdateCheck,
            objectStore: static _ => ObjectStore,
            wideColumn: static _ => WideColumn);

        // Dial target reads off the CASE, never a caller-supplied argument beside it naming a host that case
        // contradicts. Cases whose bytes move under someone else's resolver rail typed — a spawn has no
        // authority at all, and a provider SDK or cluster driver resolves endpoint, region, and contact points
        // off its own configured client — so an HTTP registration cannot be handed one.
        public Fin<Uri> Authority => hop.Switch(
            httpApi: static hit => Fin.Succ(hit.Authority),
            grpc: static hit => Fin.Succ(hit.Address),
            serverStream: static hit => Fin.Succ(hit.Address),
            companionSpawn: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.CompanionSpawn))),
            localIpc: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.LocalIpc))),
            webhookPost: static hit => Fin.Succ(hit.Target),
            updateCheck: static hit => Fin.Succ(hit.Installed.Feed),
            objectStore: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.ObjectStore))),
            wideColumn: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.WideColumn))));

        // Dedup identity becomes the lease `OperationKey`, so it reaches every attempt and lands as
        // `operation.key` on every resilience event. Keyless rows answer their registry key.
        public string IdempotencyKey => hop.Switch(
            httpApi: static _ => HttpApi.Key.Value,
            grpc: static hit => hit.ContentKey,
            serverStream: static _ => ServerStream.Key.Value,
            companionSpawn: static _ => CompanionSpawn.Key.Value,
            localIpc: static hit => $"{hit.Peer.Pid}:{hit.Peer.StartInstant}",
            webhookPost: static hit => hit.DeliveryKey,
            updateCheck: static _ => UpdateCheck.Key.Value,
            objectStore: static hit => hit.ObjectKey,
            wideColumn: static hit => hit.PartitionKey);
    }
}
```

## [03]-[HTTP_PIPELINES]

- Owner: `HttpLane` — one registration fold for the `SocketsHttpHandler`-borne rows (HttpApi, WebhookPost, UpdateCheck).
- Entry: `Wire(IServiceCollection services, IConfiguration configuration, OutboundHop hop, Func<DeadlineClass, TimeSpan> allotted, params ReadOnlySpan<WeightedUriEndpoint> routes)` returns `Fin<IServiceCollection>` — rails on the hop's own `Authority`, then the `Hedges` capability read selects hedging over standard.
- Auto: `AddStandardResilienceHandler` and `AddStandardHedgingHandler` each call `EnableReloads` on their own options name inside the registration, so a section edit re-materializes the pipeline with in-flight executions finishing under the old generation and no reload member on this fence; package-generated validators prove attempt ≤ total, sampling ≥ 2× attempt, and the cumulative hedging plan at startup with the misconfigured client named; the handler sets `HttpClient.Timeout` infinite so the pipeline owns the seam's only deadline; registering any resilience handler installs the HTTP metrics enricher stamping `error.type` with the status code.
- Packages: Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Http.Diagnostics, Microsoft.Extensions.Telemetry.Abstractions, LanguageExt.Core, BCL inbox
- Growth: one options row per pipeline key under the `Outbound` section root; a new HTTP-borne hop is one `Wire` call over its row; a multi-region target is one weighted-endpoint span, never a second pipeline — zero new surface.
- Boundary: `AddStandardResilienceHandler` binds rate limiter, total timeout, retry, breaker, and attempt timeout as one options record, and every slot the row decides is set FROM the row's `HopAllotment` before the `Outbound:{key}` section binds over it, so an operator edits a posture the derivation already made coherent; hedging admits the `Replayable` capability alone, since `HopRows.Admitted` already refused that capability on any row whose idempotency is weaker than `Idempotent` — a hedged attempt replays the body concurrently and the request snapshot refuses a stream body at construction; routes ride the call as `WeightedUriEndpoint` values with `WeightedGroupSelectionMode.EveryAttempt` above one group and `InitialAttempt` at one, so load-spread against primary-with-failover is a route count, never a config toggle; the standard handler owns the closed transient set (408, 429, status ≥ 500, `HttpRequestException`, `TimeoutRejectedException`), so this lane declares no predicate of its own and `HttpClientResiliencePredicates.IsTransient` stays the reuse point a custom pipeline takes; `DisableForUnsafeHttpMethods` is the method-derived guard and already covers DELETE, POST, PUT, CONNECT, and PATCH, so a `DisableFor(HttpMethod.Patch)` beside it decorates a filter the set holds — its declaring `HttpRetryStrategyOptionsExtensions` carries `EXTEXP0001`, so the guard rides one centrally pinned acknowledgment and never a call-site pragma; `SelectPipelineByAuthority` mints one breaker, limiter, and deadline state per scheme-host-port from ONE declaration, so a flapping webhook target darkens only itself while `pipeline.instance` cardinality tracks the live target set, and the authority provider demands an absolute request URI so a webhook send crosses absolute; a custom `DelayGenerator` on the retry slot silently replaces the `Retry-After` header generator, so the header parse composes INSIDE any generator this lane later takes; `Configure(IConfigurationSection)` binds with unknown-key errors and refuses an empty section as a wiring defect, so the bind rides an existence gate; the hedging family binds under the client name with no suffix and carries a different options shape, so a posture flip rewrites the section rather than moving it; one `IDownstreamDependencyMetadata` row (`DependencyName`, `UniqueHostNameSuffixes`, `RequestMetadata` route set) declares each dependency route once through `HttpDiagnosticsServiceCollectionExtensions.AddDownstreamDependencyMetadata`, so spans carry the route with no per-call literal and the ambient `IOutgoingRequestContext.SetRequestMetadata` slot stays unused — a hop's route is its row, never per-call variance; the named-client `AddHttpClient` arrives through the Microsoft.Extensions.Http transitive closure, never a direct pin; a finite `client.Timeout` beside the `BaseAddress` assignment is the deleted form that re-mints a second untyped deadline surfacing as bare cancellation.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HttpLane {
    public const string SectionRoot = "Outbound";

    // Authority rails FIRST, so a process-borne row handed to this lane refuses by type rather than
    // registering a client whose base address nothing can fill.
    public static Fin<IServiceCollection> Wire(
        IServiceCollection services, IConfiguration configuration, OutboundHop hop,
        Func<DeadlineClass, TimeSpan> allotted, params ReadOnlySpan<WeightedUriEndpoint> routes) =>
        hop.Authority.Map(authority => Bind(
            services, configuration, hop.Policy, hop.Policy.Allot(allotted), authority, toSeq(routes.ToArray())));

    static IServiceCollection Bind(
        IServiceCollection services, IConfiguration configuration, HopPolicy row, HopAllotment allot,
        Uri authority, Seq<WeightedUriEndpoint> routes) =>
        row.Hedges
            ? (Hedged(services, configuration, row, allot, authority, routes), services).Item2
            : (Standard(services, configuration, row, allot, authority), services).Item2;

    // Slot edits land BEFORE the section bind, so config overrides a coherent derivation rather than the
    // package defaults; the order reverses and an operator's partial section reverts every unnamed slot.
    static IHttpStandardResiliencePipelineBuilder Standard(
        IServiceCollection services, IConfiguration configuration, HopPolicy row, HopAllotment allot, Uri authority) =>
        Sectioned(
            services.AddHttpClient(row.Key.Value, client => client.BaseAddress = authority)
                .AddStandardResilienceHandler(options => {
                    options.TotalRequestTimeout.Timeout = allot.Total;
                    options.AttemptTimeout.Timeout = allot.Attempt;
                    options.Retry.MaxRetryAttempts = allot.Attempts;
                    options.Retry.Delay = allot.Backoff;
                    options.Retry.MaxDelay = allot.Attempt;
                    options.CircuitBreaker.FailureRatio = allot.Trip;
                    options.CircuitBreaker.MinimumThroughput = allot.Throughput;
                    options.CircuitBreaker.SamplingDuration = allot.Sampling;
                    options.CircuitBreaker.BreakDurationGenerator =
                        args => new ValueTask<TimeSpan>(allot.Dwell(args.HalfOpenAttempts));
                    options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = allot.Permits;
                    options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = allot.Floor;
                    options.Retry.DisableForUnsafeHttpMethods();
                }),
            configuration, row)
            .SelectPipelineByAuthority();

    static IStandardHedgingHandlerBuilder Hedged(
        IServiceCollection services, IConfiguration configuration, HopPolicy row, HopAllotment allot,
        Uri authority, Seq<WeightedUriEndpoint> routes) =>
        Sectioned(
            services.AddHttpClient(row.Key.Value, client => client.BaseAddress = authority)
                .AddStandardHedgingHandler(route => Route(route, routes))
                .Configure(options => {
                    options.TotalRequestTimeout.Timeout = allot.Total;
                    options.Hedging.MaxHedgedAttempts = allot.Hedges;
                    options.Hedging.Delay = allot.HedgeDelay;
                    options.Endpoint.Timeout.Timeout = allot.Attempt;
                    options.Endpoint.CircuitBreaker.FailureRatio = allot.Trip;
                    options.Endpoint.CircuitBreaker.MinimumThroughput = allot.Throughput;
                    options.Endpoint.CircuitBreaker.SamplingDuration = allot.Sampling;
                    options.Endpoint.CircuitBreaker.BreakDurationGenerator =
                        args => new ValueTask<TimeSpan>(allot.Dwell(args.HalfOpenAttempts));
                }),
            configuration, row)
            .SelectPipelineByAuthority();

    // Section bind refuses an empty section by construction, so an unconfigured hop keeps its derived
    // posture instead of failing composition on a section an operator never had to write.
    static IHttpStandardResiliencePipelineBuilder Sectioned(
        IHttpStandardResiliencePipelineBuilder builder, IConfiguration configuration, HopPolicy row) =>
        Section(configuration, row).Match(Some: builder.Configure, None: () => builder);

    static IStandardHedgingHandlerBuilder Sectioned(
        IStandardHedgingHandlerBuilder builder, IConfiguration configuration, HopPolicy row) =>
        Section(configuration, row).Match(Some: builder.Configure, None: () => builder);

    static Option<IConfigurationSection> Section(IConfiguration configuration, HopPolicy row) =>
        configuration.GetSection($"{SectionRoot}:{row.Key.Value}") is var section && section.GetChildren().Any()
            ? Some(section)
            : None;

    // Route exhaustion stops hedging, so a single-group row draws its primary first and walks the rest in
    // declaration order while a multi-group row spreads by weight on every attempt.
    static void Route(IRoutingStrategyBuilder route, Seq<WeightedUriEndpoint> routes) =>
        ignore(route.ConfigureWeightedGroups(groups => {
            groups.SelectionMode = routes.Count > 1
                ? WeightedGroupSelectionMode.EveryAttempt
                : WeightedGroupSelectionMode.InitialAttempt;
            routes.Iter(endpoint => groups.Groups.Add(new WeightedUriEndpointGroup {
                Weight = endpoint.Weight,
                Endpoints = { endpoint },
            }));
        }));
}
```

## [04]-[KEYED_PIPELINES]

- Owner: `KeyedLane` — the one keyed registry registration fold for every non-HTTP hop, with its `Composition` seat record, its transport chaos catalogue, and the closure proof; `HopEvidence` the per-composition breaker cell set; `HopContext` the typed side-channel keys every strategy and the dispatch seat share; `HopStrategy` `[SmartEnum<string>]` the pipeline-row vocabulary whose declaration order IS the strategy order and whose delegate column IS each strategy's arm; `HopSeat` the per-row arm argument; `HopEnricher` the per-row measurement dimensions; the channel-policy record is the canonical `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` `GrpcChannelPolicy` consumed by reference — this page reads `GrpcChannelPolicy.Canonical` and never re-declares the record.
- Cases: 10 `HopStrategy` rows in canonical order — admission, budget, redial, retry, breaker, deadline, then the four chaos planes; the order is the derivation, not a convention.
- Entry: `Register(IServiceCollection services, Composition composition)` folds one `AddResiliencePipeline<string, HopOutcome>` entry per piped row of `HopRows.Items`; `Proven(ResiliencePipelineProvider<string> pipelines)` returns `Fin<Unit>` — the built-provider half of the closure proof, probing every piped row through `TryGetPipeline<HopOutcome>`.
- Auto: `ConfigureTelemetry(TelemetryOptions)` inserts the telemetry strategy at pipeline head and carries the estate's grammar to the meter — `MeteringEnrichers` append the transport, delivery, faculty, and tenant dimensions, `ResultFormatter` projects the outcome to its wire key so the result dimension reads, and `SeverityProvider` resolves the emitting strategy back to its own roster row; every strategy row NAMES itself, so `(pipeline.name, strategy.name)` deduplicates the budget and attempt timeouts instead of merging them into one unattributed bucket; each pipeline binds the composition's own `CircuitBreakerStateProvider` so the breaker state reads from Polly's own observation surface, never a parallel state delegate; the retry row writes its attempt ordinal onto the execution's `ResilienceContext` through `HopContext.Attempts`, so the dispatch seat reads the count off the context Polly threads rather than a closure a hedged or forked execution shares; the pipeline-head limiter is `AutoReplenishment` true on every replenishing shape so the sliding-window and token-bucket rows self-refill without a parallel timer; the container `TimeProvider` flows into every registry pipeline, so one injection drives every delay, deadline, and sampling window.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, System.Threading.RateLimiting, Rasm (kernel `Retriability`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new resilience dimension is one `HopStrategy` row carrying its arm and its severity, seated where its declaration order places it; one `HopRateLimit` key and one `Admission` switch arm absorb a new admission shape; a new measurement dimension is one `HopEnricher` tag beside its `HopTags` const; a new injected fault, substitution, or behavior is one weighted `ChaosRow` on its band beside one `HopChaos` key, and all four verbs — `AddChaosLatency`, `AddChaosFault`, `AddChaosOutcome`, `AddChaosBehavior` — arm here over all four planes; zero new surface.
- Boundary: strategy order is the derivation every reorder breaks with a named failure mode — admission counts logical calls so ONE limiter sits outermost and its single lease spans every attempt (a limiter inside retry converts a retry storm into permit starvation), the budget bounds the whole loop so the total deadline sits outside retry (inside, it re-arms per attempt and unbounds the loop), the redial sits ABOVE retry so it substitutes after every attempt fails (below, the retry predicate reads the substituted outcome and silently stops looping), health statistics count attempts so the breaker sits inside retry (outside, it reaches its throughput floor N times slower), each attempt earns a fresh deadline so the attempt timeout is innermost, and chaos sits below everything it tests since injection above a breaker proves nothing; a second `AddConcurrencyLimiter` beside the admission row is the deleted form that stacks two queues on one seam and makes the effective permit the smaller of two columns; retry and breaker BOTH bind the one `Transient` `PredicateBuilder<HopOutcome>` row, whose result half reads the kernel `Retriability` the fault itself carries rather than a second classification of the same fact — the package default handles every exception except `OperationCanceledException` and therefore never sees a `HopOutcome.Faulted` RESULT, so a hop stating its refusal as a result retries nothing and breaks no circuit — while the exception half must NAME the transports in use, since gRPC states every remote fault as a throw and an `RpcException` matching no exception row leaves the three `GrpcChannel` rows retrying nothing for the same reason from the opposite direction; `Unavailable` is the one status the row admits and `SocketException` covers the Unix-domain dial; `BrokenCircuitException` stays out of that row, since spending budget re-dialing a dead endpoint is the exact waste the breaker exists to stop, and the redial's own `Rejected` row handles the rejection family instead; the retry delay generator reads that SAME discriminant — a `Throttled` fault answers the server's own window and every other retriable one answers `null` and rides the computed curve, so `MaxDelay` caps the curve alone and the advised window escapes it by construction; an exception-shaped advice arm here is the deleted form, because `RateLimiterRejectedException` is raised outside the retry entirely and `BrokenCircuitException` is refused by the retry's own predicate, so neither ever reaches a generator that runs only after `ShouldHandle` admits; gRPC-native retry is rejected — the channel `ServiceConfig` retry and hedging fork a second retry owner; a SEPARATE store-rail pipeline family is rejected on the same ground and one rung lower — it duplicates the claim registry, the allotment derivation, the telemetry enricher, and the ownership law, while `[HOP_TOPOLOGY]` `[ONE_OWNER]` holds one registry per key type per container, so a store rail enters as two cases on the settled union at Tier-0 `[12]-[ADMISSION]` rung `[01]`; admission is one `RateLimiterStrategyOptions` row whose `RateLimiter` lease-producer delegate is the `Leased` projection over the row's `HopRateLimit Admission` column — `Concurrency` keeps the `DefaultRateLimiterOptions` typed `ConcurrencyLimiterOptions` path, `SlidingWindow` leases from a `SlidingWindowRateLimiter` and `TokenBucket` from a `TokenBucketRateLimiter`, every window, segment, permit, and replenishment value projecting off `HopAllotment`; a limiter handed through `AddRateLimiter` is NEVER disposed by the pipeline, so each minted limiter registers its release on `OnPipelineDisposed` — an unreleased replenishing limiter keeps a live refill timer for the process lifetime; the `LocalIpc` redial is one `AddFallback` strategy on the `Redialing` capability whose `FallbackStrategyOptions<HopOutcome>.FallbackAction` re-reads the peer manifest and reconnects — typed `Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>`, the `readonly struct FallbackActionArguments<HopOutcome>` carrier exposing `ResilienceContext Context` and the inbound `Outcome<HopOutcome> Outcome` — and a row without the capability takes no fallback row at all, never a redial the composition must synthesize; the breaker cells live on the COMPOSITION rather than a process static, so a second composition mints its own state and cannot inherit the first one's breaker readings — `CircuitBreakerStateProvider` is SINGLE-ATTACH so one provider seats per key and a second pipeline reusing it throws at build, while `CircuitBreakerManualControl` seats per `Faculty` so isolate and close act on a faculty's whole breaker set as one verb and a pipeline materializing lazily under a pinned control comes up dark; the breaker's dwell takes a GENERATOR over the one per-execution datum this seat receives — consecutive failed half-open probes — while every other knob stays a value, because a generator over a datum the context never carries returns its own column verbatim; `AddHedging<TResult>` is reachable on this generic builder and stays UNARMED, because hedging admits only the `Replayable` capability and no piped row holds it — the server-stream row is semantically repeat-safe yet cannot replay its body concurrently, and the object-store row is repeat-safe yet withholds `Replayable` because a hedged PUT uploads the body twice — so a hedge row here is a strategy nothing arms and hedging as a failure remedy is the rejected form; the four chaos planes compose `Runtime/determinism#ADVERSARIAL_PROBE` whole — `ChaosArming` writes the gate, the address, the seeded weighted pick, and the chain record, so this page declares its bands and its `HopChaos` row resolvers alone and mints no posture cell, no options body, and no decision record; the package's own `FaultGenerator` and `OutcomeGenerator<T>` catalogue constructions stay refused, since each builds its selection draw from an internal helper no options member substitutes and picks a different row every run beneath a gate that reads deterministic; an unarmed composition or an undeclared band appends no row at all, so a production pipeline carries zero chaos strategies rather than four disabled ones; per-pipeline options reload is declined — every knob derives from `HopAllotment`, so the reload unit is the deadline table and a per-pipeline options record forks the derivation it reloads; the COMPOSED chain is proved off the built pipeline in the suite through `Polly.Testing` `pipeline.GetPipelineDescriptor().Strategies` — an ordered roster whose `Options` type and `Name` are the assertable identity — because resolution alone admits a hop whose arms silently dropped a strategy, and that inspection dependency belongs on the test plane; the canonical channel record carries keepalive 60s/30s, infinite pooled-connection idle, multiplexed HTTP/2, and 4 MiB caps in both directions.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
// GrpcChannelPolicy is OWNED by Rasm.Compute/Runtime/channels#TRANSPORT_AXIS (the canonical 9-field
// record: PooledConnectionIdle, KeepAlivePingDelay, KeepAlivePingTimeout, EnableMultipleHttp2Connections,
// MaxSendBytes, MaxReceiveBytes, InitialReconnectBackoff, MaxReconnectBackoff, HttpVersionPosture Version).
// This page consumes GrpcChannelPolicy.Canonical by reference and never re-declares the record.

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record HopSeat(
    HopPolicy Row,
    HopAllotment Allot,
    KeyedLane.Composition Composition,
    Func<Action, Unit> Reclaim);

// Breaker cells mint ONCE per composition off the closed rosters, so a lookup is a total read rather than a
// find-or-add race, and a second composition cannot inherit the first one's breaker readings.
public sealed record HopEvidence(
    FrozenDictionary<HopKey, CircuitBreakerStateProvider> States,
    FrozenDictionary<Faculty, CircuitBreakerManualControl> Controls) {
    public static HopEvidence Of() => new(
        HopRows.Items.ToFrozenDictionary(static row => row.Key, static _ => new CircuitBreakerStateProvider()),
        Faculty.Items.ToFrozenDictionary(static row => row, static _ => new CircuitBreakerManualControl()));

    public Option<CircuitBreakerStateProvider> State(HopKey key) => States.Find(key);
    public CircuitBreakerManualControl Breaker(Faculty group) => Controls[group];
}

// --- [TYPES] --------------------------------------------------------------------------------
// Both halves of a hop read one typed side channel: `Window` fixes BEFORE the pipeline runs, `Attempts` is
// written by the retry row DURING it, and neither value is a closure a hedged or forked execution shares nor
// ambient state a second seam could reach.
public static class HopContext {
    public static readonly ResiliencePropertyKey<Duration> Window = new("rasm.hop.window");
    public static readonly ResiliencePropertyKey<int> Attempts = new("rasm.hop.attempts");
}

// Vocabulary IS the pipeline. Declaration order is the canonical strategy order the resilience law derives,
// and each row carries the arm that appends it beside the severity its events map to.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HopStrategy {
    public static readonly HopStrategy Admission = new("admission", ResilienceEventSeverity.Warning, ArmAdmission);
    public static readonly HopStrategy Budget = new("budget", ResilienceEventSeverity.Error, ArmBudget);
    public static readonly HopStrategy Redial = new("redial", ResilienceEventSeverity.Warning, ArmRedial);
    public static readonly HopStrategy Retry = new("retry", ResilienceEventSeverity.Information, ArmRetry);
    public static readonly HopStrategy Breaker = new("breaker", ResilienceEventSeverity.Error, ArmBreaker);
    public static readonly HopStrategy Deadline = new("deadline", ResilienceEventSeverity.Warning, ArmDeadline);
    public static readonly HopStrategy Slow = new("chaos-latency", ResilienceEventSeverity.Information, ArmSlow);
    public static readonly HopStrategy Injection = new("chaos-fault", ResilienceEventSeverity.Information, ArmInjection);
    public static readonly HopStrategy Substitute = new("chaos-outcome", ResilienceEventSeverity.Information, ArmSubstitute);
    public static readonly HopStrategy Perturb = new("chaos-behavior", ResilienceEventSeverity.Information, ArmPerturb);

    public ResilienceEventSeverity Severity { get; }

    [UseDelegateFromConstructor]
    public partial ResiliencePipelineBuilder<HopOutcome> Arm(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    // Both halves are load-bearing. RESULT reads the kernel discriminant the fault already carries, so retry,
    // breaker, and in-process re-drive classify one fact once. EXCEPTION names the transports in use, since
    // three rows ride `HopTransport.GrpcChannel` and gRPC states every remote fault as a
    // THROW, so `RpcException` never reaches `HandleResult`. `Unavailable` alone earns the retry, since only
    // that code names a transient condition, while `DeadlineExceeded` belongs to the budget the pipeline owns.
    // Both `StoreRail` rows need no exception row: a driver refusal converts to `HopOutcome.Faulted` at the body.
    static readonly PredicateBuilder<HopOutcome> Transient = new PredicateBuilder<HopOutcome>()
        .Handle<TimeoutRejectedException>()
        .Handle<HttpRequestException>()
        .Handle<SocketException>()
        .Handle<RpcException>(static refused => refused.StatusCode == StatusCode.Unavailable)
        .HandleResult(static outcome =>
            outcome is HopOutcome.Faulted { Reason: Fault { Retriability: not Retriability.TerminalCase } });

    static readonly PredicateBuilder<HopOutcome> Rejected = new PredicateBuilder<HopOutcome>()
        .Handle<ExecutionRejectedException>();

    static ResiliencePipelineBuilder<HopOutcome> ArmAdmission(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        KeyedLane.Leased(seat) is { } limiter
            ? (seat.Reclaim(limiter.Dispose), builder.AddRateLimiter(new RateLimiterStrategyOptions {
                Name = Admission.Key,
                RateLimiter = args => limiter.AcquireAsync(permitCount: 1, args.Context.CancellationToken),
            })).Item2
            : builder.AddRateLimiter(new RateLimiterStrategyOptions {
                Name = Admission.Key,
                DefaultRateLimiterOptions = new ConcurrencyLimiterOptions {
                    PermitLimit = seat.Allot.Permits,
                    QueueLimit = seat.Allot.Floor,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                },
            });

    static ResiliencePipelineBuilder<HopOutcome> ArmBudget(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        builder.AddTimeout(new TimeoutStrategyOptions { Name = Budget.Key, Timeout = seat.Allot.Total });

    static ResiliencePipelineBuilder<HopOutcome> ArmRedial(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        seat.Row.Held.Admits(HopCapability.Redialing)
            ? builder.AddFallback(new FallbackStrategyOptions<HopOutcome> {
                Name = Redial.Key,
                ShouldHandle = Rejected,
                FallbackAction = seat.Composition.Redial(seat.Row),
            })
            : builder;

    // `MaxRetryAttempts` refuses a zero, so a single-shot row takes NO retry row rather than a retry bounded
    // at nothing; a re-offered spawn forks a second child the manifest read then races.
    static ResiliencePipelineBuilder<HopOutcome> ArmRetry(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        seat.Row.Retries
            ? builder.AddRetry(new RetryStrategyOptions<HopOutcome> {
                Name = Retry.Key,
                MaxRetryAttempts = seat.Allot.Attempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = seat.Allot.Backoff,
                MaxDelay = seat.Allot.Attempt,
                ShouldHandle = Transient,
                DelayGenerator = static args => new ValueTask<TimeSpan?>(Advised(args.Outcome)),
                OnRetry = static args => {
                    args.Context.Properties.Set(HopContext.Attempts, args.AttemptNumber + 2);
                    return default;
                },
            })
            : builder;

    // Server-advised windows honor verbatim and escape `MaxDelay` by construction; every other retriable fault
    // answers null and rides the computed curve. Advice reads the fault's own `Throttled` case, so a store-rail
    // refusal stated as a result carries its window exactly as a thrown one would.
    static TimeSpan? Advised(Outcome<HopOutcome> outcome) =>
        outcome.Result is HopOutcome.Faulted { Reason: Fault { Retriability: Retriability.ThrottledCase throttled } }
            ? throttled.RetryAfter.ToTimeSpan()
            : null;

    static ResiliencePipelineBuilder<HopOutcome> ArmBreaker(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HopOutcome> {
            Name = Breaker.Key,
            FailureRatio = seat.Allot.Trip,
            MinimumThroughput = seat.Allot.Throughput,
            SamplingDuration = seat.Allot.Sampling,
            BreakDurationGenerator = args => new ValueTask<TimeSpan>(seat.Allot.Dwell(args.HalfOpenAttempts)),
            ShouldHandle = Transient,
            ManualControl = seat.Composition.Evidence.Breaker(seat.Row.Needs),
            StateProvider = seat.Composition.Evidence.State(seat.Row.Key).IfNone(new CircuitBreakerStateProvider()),
        });

    static ResiliencePipelineBuilder<HopOutcome> ArmDeadline(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        builder.AddTimeout(new TimeoutStrategyOptions { Name = Deadline.Key, Timeout = seat.Allot.Attempt });

    // Four chaos planes seat BELOW every strategy under test, each declaring its band and its row resolver and
    // handing the bound options straight to its builder verb; an unarmed composition answers `None`.
    static ResiliencePipelineBuilder<HopOutcome> ArmSlow(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Latency).Match(
            Some: seated => builder.AddChaosLatency(seated.Arming.Latency(seated.Band)),
            None: () => builder);

    static ResiliencePipelineBuilder<HopOutcome> ArmInjection(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Fault).Match(
            Some: seated => builder.AddChaosFault(seated.Arming.Fault(seated.Band, KeyedLane.HopChaos.Thrown)),
            None: () => builder);

    // Result substitution needs the result type, so this plane is reachable on the branch's ONE generic keyed
    // pipeline and nowhere else — the transport rail is where a refusal is faked without a throw.
    static ResiliencePipelineBuilder<HopOutcome> ArmSubstitute(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Outcome).Match(
            Some: seated => builder.AddChaosOutcome(seated.Arming.Substitution<HopOutcome>(seated.Band, KeyedLane.HopChaos.Substituted)),
            None: () => builder);

    static ResiliencePipelineBuilder<HopOutcome> ArmPerturb(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Behavior).Match(
            Some: seated => builder.AddChaosBehavior(seated.Arming.Behavior(seated.Band,
                row => KeyedLane.HopChaos.Perturbed(seat.Composition.Evidence, row, seat.Row.Needs))),
            None: () => builder);

    static Option<(ChaosArming Arming, ChaosBand Band)> Banded(HopSeat seat, ChaosKind kind) =>
        from arming in seat.Composition.Chaos
        from band in arming.BandOf(seat.Row.Key.Value, kind)
        select (arming, band);
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class KeyedLane {
    public static class HopTags {
        public const string Transport = "rasm.hop.transport";
        public const string Delivery = "rasm.hop.delivery";
        public const string Faculty = "rasm.hop.faculty";
    }

    public sealed record Composition(
        ILoggerFactory Telemetry,
        Func<DeadlineClass, TimeSpan> Allotted,
        Func<HopPolicy, Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>> Redial,
        HopEvidence Evidence,
        Option<ChaosArming> Chaos);

    // Transport-plane catalogue ROWS this page resolves — weights and rates stay the band's declaration. Each
    // key maps to a real transport perturbation: a connect failure, a stalled attempt, a stated refusal, and
    // operator darkness over the row's own faculty group.
    public static class HopChaos {
        public const string Connect = "connect";
        public const string Stall = "stall";
        public const string Refuse = "refuse";
        public const string Isolate = "isolate";

        public static Exception Thrown(string row) => row switch {
            Connect => new HttpRequestException("<injected-connect>"),
            _ => new TimeoutRejectedException("<injected-stall>"),
        };

        public static Outcome<HopOutcome> Substituted(string row) =>
            Outcome.FromResult<HopOutcome>(new HopOutcome.Faulted(new HopFault.ContractBroken($"<injected:{row}>")));

        public static ValueTask Perturbed(HopEvidence evidence, string row, Faculty group) =>
            row == Isolate ? new ValueTask(evidence.Breaker(group).IsolateAsync()) : ValueTask.CompletedTask;
    }

    // Hop dimensions on every resilience measurement, so a per-transport or per-delivery-class resilience
    // series is real. Tenancy SPREADS the kernel pair's own projection, empty for the root row.
    public sealed class HopEnricher(HopPolicy row) : MeteringEnricher {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context) {
            context.Tags.Add(new(HopTags.Transport, row.Transport.ToString()));
            context.Tags.Add(new(HopTags.Delivery, row.Delivery.ToString()));
            context.Tags.Add(new(HopTags.Faculty, row.Needs.Key));
            foreach (KeyValuePair<string, object?> tenancy in TenantContext.Current.Tags) {
                context.Tags.Add(tenancy);
            }
        }
    }

    // Every window, segment, permit, and replenishment value projects off the allotment; a null answer takes
    // Polly's own concurrency limiter, the one shape it disposes on the caller's behalf.
    public static RateLimiter? Leased(HopSeat seat) =>
        seat.Row.Admission.Switch<RateLimiter?>(
            concurrency: static () => null,
            slidingWindow: () => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                Window = seat.Allot.Total,
                SegmentsPerWindow = HopAllotment.WindowSegments,
                AutoReplenishment = true,
                PermitLimit = seat.Allot.Permits,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = seat.Allot.Floor,
            }),
            tokenBucket: () => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions {
                ReplenishmentPeriod = seat.Allot.Backoff,
                TokensPerPeriod = seat.Allot.Throughput,
                AutoReplenishment = true,
                TokenLimit = seat.Allot.Permits,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = seat.Allot.Floor,
            }));

    // Registration takes the two-arg configure overload for ONE reason: `OnPipelineDisposed` is the reclaim
    // hook a supplied limiter needs, since a limiter handed through `AddRateLimiter` never disposes with the
    // pipeline that leases from it. Statement bodies here are the platform-forced registry seam.
    public static IServiceCollection Register(IServiceCollection services, Composition composition) =>
        HopRows.Items.Filter(static row => row.Piped).Fold(services, (graph, row) =>
            graph.AddResiliencePipeline<string, HopOutcome>(row.Key.Value, (builder, context) => {
                HopSeat seat = new(row, row.Allot(composition.Allotted), composition, release => {
                    context.OnPipelineDisposed(release);
                    return unit;
                });
                ignore(toSeq(HopStrategy.Items).Fold(
                    builder.ConfigureTelemetry(new TelemetryOptions {
                        LoggerFactory = composition.Telemetry,
                        MeteringEnrichers = { new HopEnricher(row) },
                        ResultFormatter = static (_, result) => result is HopOutcome outcome ? HopVerdict.Of(outcome).Key : result,
                        SeverityProvider = static args =>
                            HopStrategy.TryGet(args.Source.StrategyName ?? string.Empty, out HopStrategy? strategy)
                                ? strategy!.Severity
                                : ResilienceEventSeverity.Information,
                    }),
                    (chain, strategy) => strategy.Arm(chain, seat)));
            }));

    // Closure proof completes on the BUILT provider, because a pipeline exists only after the provider builds
    // and `TryGetPipeline` probes without throwing where `GetPipeline` raises.
    public static Fin<Unit> Proven(ResiliencePipelineProvider<string> pipelines) =>
        HopRows.Items.Filter(static row => row.Piped)
            .Traverse(row => pipelines.TryGetPipeline<HopOutcome>(row.Key.Value, out _)
                ? Validation<Error, Unit>.Success(unit)
                : new KernelFault.InvalidValue(Label: row.Key.Value, Requirement: "<a built hop pipeline>"))
            .As()
            .Map(static _ => unit)
            .ToFin();
}
```

## [05]-[OWNERSHIP_LAW]

- Owner: `OutboundSurface` — admission, dispatch, conflict evidence, in-process re-drive, and enforcement over one runtime record; `RetryOwner` `[SmartEnum<string>]` the two-row claim vocabulary; `OutboundRuntime` capability record; `HopClaim` the retry-owner cell value; `HopOutcome` `[Union]`; `HopDial` the measured half of a hop; `HopReceipt` the stable native evidence; `HopSettled<T>` the receipt beside the typed carried rail.
- Cases: Delivered, Refused, Faulted — Refused carries pre-flight admission faults, Faulted carries in-flight pipeline rejection; two `RetryOwner` rows — `Pipeline`, `Schedule`.
- Entry: `Seat(OutboundRuntime runtime)` returns `Fin<Unit>` — the boot gate proving every row's conditional corner, claiming every row for the pipeline owner, and proving every piped row materialized; `Dispatch<T>(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<(HopOutcome, T)>> send, Option<ILatencyContext> latency = default)` is the ONE hop run, answering `IO<HopSettled<T>>` that never fails — the value rides its typed `Fin<T>` beside the native receipt that timed it; `Run(...)` is its `Unit` projection returning `IO<HopReceipt>`, the shape a write-only hop takes; `Redriven<T>(OutboundRuntime runtime, HopPolicy row, RedrivePolicy policy, IO<T> work)` claims the schedule owner and re-drives in process where the pipeline provably holds no retry row. Every exit is a receipt, never a throw.
- Receipt: native `HopReceipt` — typed registry key and verdict, one optional attempt/duration measurement, optional breaker state, and optional kernel `FaultObservation`; `HopDial` stays an execution-local measured pair only long enough to mint the stable receipt. Every hop exit fans that receipt under `ReceiptKind.Hop` through `SuiteContracts.Host`; no protobuf or parallel STJ carrier exists for this same-process evidence.
- Packages: Polly.Core, Rasm (kernel `Redrive`/`Cell`/`GaugedSpan`/`FaultObservation`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one outcome case per new terminal kind; one `HopFault` rejection arm per new pipeline termination verb, seated ahead of `Foreign`; zero new surface.
- Boundary: a hop body that produces a value carries it out on the settlement the same run timed, so the reported value and the receipt describe ONE transport call — a hop run for its outcome followed by a second raw call for its value is the deleted form, since that second frame rides no pipeline, no retry, and no breaker while the receipt attributes its timing to the first; the value rides the execution's own `ResilienceContext` rather than a cell captured beside the run, so a retried attempt cannot leave a stale value behind an outcome the next attempt settled, and `HopSettled` makes the delivered-with-no-value corner one `Fin` refusal at the owner rather than a fall-through arm at every caller; exactly one retry owner per hop, claimed at COMPOSITION through `Seat` and read by `Admit` — a per-dispatch claim swap re-decides a boot fact on every frame and leaves an unregistered key admitting until its first dial throws; the claim is a `Cell.Claim` first-writer transition, so the seater and the finder are distinguishable and the loser's own policy reference survives on the conflict; domain rails retry through `RedrivePolicy`, transport hops retry through the keyed or HTTP pipeline, never both on one seam, since a schedule of m over a pipeline of n multiplies attempts invisibly and inflates the idempotency window by m — `Redriven` is the ONE lawful in-process re-drive and it is lawful exactly because `Retries` is false for the row it serves: a `SingleShot` row takes no retry strategy at all, so the schedule claim cannot stack on a pipeline loop that does not exist, and a claim conflict degrades to a single pass carrying its conflict receipt rather than stacking one; the remote object store and the wide-column store seat on THIS owner because `[LAYER_SPLIT]` row `[02]` reads the process seam alone and a `HopOutcome` is transport-neutral by construction, while the embedded and coordination bands answer rows `[03]` and `[01]` at the Persistence side, so a store call has exactly one retry owner and which one is a seam fact rather than a package preference; `Dispatch` runs the hop through `ExecuteOutcomeAsync` over a pooled `ResilienceContext` acquired and released by `IO.Bracket`, so a pipeline rejection surfaces as a captured `Outcome<HopOutcome>.Exception` with no exception-as-control-flow round-trip and the release arm runs on the raise path the hand `finally` covered; the lease fixes `OperationKey` to the hop's own `IdempotencyKey`, so the dedup identity reaches every attempt and lands as `operation.key` on every resilience event, and `HopContext.Window` carries the allotment span through `context.Properties` BEFORE the pipeline runs while `HopContext.Attempts` carries the ordinal the retry row writes during it — never ambient state, and never a key minted inside the retried callback that changes per attempt; the total-outcome fold happens exactly ONCE at this seam over a taxonomy ordered child-before-parent, `IsolatedCircuitException` ahead of its `BrokenCircuitException` base so operator-forced darkness never masquerades as a dependency open, and a caller re-folding the rail re-opens the vocabulary the seam retired; `Enforce` sweeps the per-faculty manual breakers from the effective degradation level and runs once at boot against the resolved level, so a pipeline materializing lazily under a pinned isolation comes up dark rather than serving one undegraded call; `BreakerState` answers `None` for a row whose breaker lives inside the HTTP handler's per-authority instances, because a receipt claiming `Closed` from an unattached provider reads as a measurement to every consumer; an admission refusal answers `Measure: None` for the same reason — a zero attempt count beside a zero span is a forged measurement of a dial that never happened, and absence is the honest carrier; the hop's latency checkpoint records through `LatencySpine.Mark(ILatencyContext, CheckpointToken)` in the bracket's release arm, so a faulted or rejected hop is measured exactly like a delivered one and the span on the receipt is never the recorder's only source; `HopReceipt.Of` classifies the execution once and projects errors through the kernel `FaultObservation`, while `SuiteContracts.Host` owns the sole local JSON crossing; the runtime record and its issued `CheckpointToken` are constructed once at the composition root.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HopOutcome {
    private HopOutcome() { }

    public sealed record Delivered : HopOutcome;
    public sealed record Refused(Error Reason) : HopOutcome;
    public sealed record Faulted(Error Reason) : HopOutcome;
}

// Two owners, two claims: the pipeline claims at boot and holds every row, and a schedule claims one row only
// where that row's pipeline carries no retry strategy at all.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetryOwner {
    public static readonly RetryOwner Pipeline = new("transport-pipeline");
    public static readonly RetryOwner Schedule = new("schedule");
}

// --- [MODELS] -------------------------------------------------------------------------------
// Dial carries the MEASURED half of a hop, present exactly where a dial happened, and its elapsed rides the
// kernel gauged span against the hop-total bound, so `Breached` and `Overrun` derive and no column stores them.
public readonly record struct HopDial(int Attempts, GaugedSpan<DeadlineClass> Span);

// Breaker state is an OPTION because only a keyed row seats an observable provider; `Dial` is an OPTION because
// an admission refusal never dialed, and a zero attempt count beside a zero span is a measurement nobody took.
// The execution union retains its Error; the stable receipt carries the closed verdict plus one bounded kernel
// observation, so serializing a local receipt never walks an exception graph.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HopVerdict {
    public static readonly HopVerdict Delivered = new("delivered");
    public static readonly HopVerdict Refused = new("refused");
    public static readonly HopVerdict Faulted = new("faulted");

    public static HopVerdict Of(HopOutcome outcome) => outcome.Switch(
        delivered: static _ => Delivered,
        refused: static _ => Refused,
        faulted: static _ => Faulted);
}

public readonly record struct HopMeasure(int Attempts, Duration Elapsed);

public sealed record HopReceipt(
    HopKey Hop,
    HopVerdict Outcome,
    Option<HopMeasure> Measure,
    Option<CircuitState> Breaker,
    Option<FaultObservation> Fault) {
    public static HopReceipt Of(
        HopKey hop, HopOutcome outcome, Option<HopDial> dial, Option<CircuitState> breaker) => new(
            hop,
            HopVerdict.Of(outcome),
            dial.Map(static measured => new HopMeasure(measured.Attempts, measured.Span.Elapsed)),
            breaker,
            outcome.Switch(
                delivered: static _ => Option<FaultObservation>.None,
                refused: static row => Some(FaultObservation.Of(row.Reason)),
                faulted: static row => Some(FaultObservation.Of(row.Reason))));
}

public readonly record struct HopSettled<T>(HopReceipt Receipt, Fin<T> Carried);

public sealed record HopClaim(RetryOwner Owner, HopKey Row);

// One spelling per wire key: the projection below and every reliability objective partitioning the hop and
// delivery counters on their outcome dimension read the same const.
// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record OutboundRuntime(
    ResiliencePipelineProvider<string> Pipelines,
    ConsumptionProfile Profile,
    ClockPolicy Clocks,
    Func<DeadlineClass, TimeSpan> Allotted,
    Func<DegradationLevel> Level,
    HopEvidence Evidence,
    Atom<HashMap<HopKey, HopClaim>> RetryOwners,
    CheckpointToken Checkpoint,
    ReceiptSinkPort Sink) {
    // Checkpoint is the hop phase's token, issued ONCE at composition through
    // ILatencyContextTokenIssuer.GetCheckpointToken against the folded LatencyCheckpoint roster — a name
    // resolved per call against an unfolded roster answers a positionless token whose writes drop unseen.
    public Option<CircuitState> BreakerState(HopPolicy row) =>
        row.Piped ? Evidence.State(row.Key).Map(static state => state.CircuitState) : None;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class OutboundSurface {
    // Claim identity is the HOP, never the policy or the instance, so per-authority pipeline instances stay
    // one owner with N isolated states. `Cell.Claim` distinguishes the seater from the finder, so a losing
    // claim reads the incumbent off the transition rather than re-reading a cell that already moved.
    public static Fin<HopClaim> Claim(OutboundRuntime runtime, HopPolicy row, RetryOwner owner) =>
        Cell.Claim(runtime.RetryOwners, row.Key, () => new HopClaim(owner, row.Key)).Switch(
            committed: seated => Held(seated.State, row.Key, owner),
            ceded: ceded => Held(ceded.State, row.Key, owner),
            refused: refused => Held(refused.State, row.Key, owner),
            contended: contended => Held(contended.State, row.Key, owner));

    // Incumbency rides the transition's own post-state, so the loser reads WHO holds the key rather than
    // re-reading a cell that may already have moved again.
    static Fin<HopClaim> Held(HashMap<HopKey, HopClaim> seated, HopKey key, RetryOwner owner) =>
        seated.Find(key) is var held && held.Exists(claim => claim.Owner == owner)
            ? held.ToFin(new HopFault.OwnerConflict(key, owner, owner))
            : Fin.Fail<HopClaim>(new HopFault.OwnerConflict(
                key, held.Map(static claim => claim.Owner).IfNone(RetryOwner.Pipeline), owner));

    // Boot gate: the roster's conditional corners prove, every row claims the pipeline owner, and every piped
    // row proves materialized BEFORE a dispatch reaches the registry, so a missing registration names its key
    // at composition rather than throwing inside whichever frame dials it first.
    public static Fin<Unit> Seat(OutboundRuntime runtime) =>
        from rows in HopRows.Admitted
        from _claimed in rows
            .Traverse(row => Claim(runtime, row, RetryOwner.Pipeline).Match(
                Succ: static _ => Validation<Error, Unit>.Success(unit),
                Fail: static conflict => Validation<Error, Unit>.Fail(conflict)))
            .As()
            .Map(static _ => unit)
            .ToFin()
        from _proven in KeyedLane.Proven(runtime.Pipelines)
        select unit;

    // Exclusion reads ONE profile axis against ONE capability: a row acting on this package's own process image
    // cannot run where the host owns that process, and no row carries a predicate the axis cannot compare.
    public static Fin<HopPolicy> Admit(OutboundRuntime runtime, OutboundHop hop) =>
        from row in Fin.Succ(hop.Policy)
        from _excluded in guardnot(
            row.Held.Admits(HopCapability.SelfLaunching) && runtime.Profile.Attach == HostAttach.Foreign,
            new HopFault.Excluded(row.Key.Value))
        from _fenced in guard(runtime.Level().Retains.Admits(row.Needs), new HopFault.Fenced(row.Key.Value))
        from _owner in runtime.RetryOwners.Value.Find(row.Key)
            .Filter(static held => held.Owner == RetryOwner.Pipeline)
            .ToFin(new HopFault.OwnerConflict(row.Key, RetryOwner.Schedule, RetryOwner.Pipeline))
        select row;

    // THE hop run. The body states its own HopOutcome because a protocol-level refusal (a 404, a false
    // confirmed-request ack) is a hop fact the rail cannot infer from a non-throwing return, and it carries its
    // produced value out on the same settlement — two frames on the wire, one timed and one not, is the
    // deleted form the settlement pair forecloses.
    public static IO<HopSettled<T>> Dispatch<T>(
        OutboundRuntime runtime, OutboundHop hop,
        Func<CancellationToken, Task<(HopOutcome Outcome, T Value)>> send, Option<ILatencyContext> latency = default) =>
        Admit(runtime, hop).Match(
                Succ: row => Execute(runtime, row, hop, send, latency),
                Fail: error => IO.pure(new HopSettled<T>(
                    Conflicted(runtime, hop.Policy, error), Fin.Fail<T>(error))))
            .Bind(settled => Fan(runtime, settled.Receipt).Map(receipt => settled with { Receipt = receipt }));

    // Latency context is the P14 threading half: the composition root's factory hands it in and this boundary
    // records ONE checkpoint through LatencySpine.Mark. Callers with no telemetry composition pass none, so
    // that hop runs unrecorded rather than minting a second recorder.
    public static IO<HopReceipt> Run(
        OutboundRuntime runtime, OutboundHop hop,
        Func<CancellationToken, Task<HopOutcome>> send, Option<ILatencyContext> latency = default) =>
        Dispatch<Unit>(runtime, hop, async token => (await send(token).ConfigureAwait(false), unit), latency)
            .Map(static settled => settled.Receipt);

    // Redriven is the ONE in-process re-drive over a hop, lawful only where the row's pipeline holds no retry
    // strategy: `Retries` reads false for a `SingleShot` row, so this claim cannot stack on a loop that never
    // armed, and a conflict degrades to a single pass carrying its conflict receipt.
    public static IO<T> Redriven<T>(OutboundRuntime runtime, HopPolicy row, RedrivePolicy policy, IO<T> work) =>
        Claim(runtime, row, RetryOwner.Schedule).Match(
            Succ: _ => Redrive.Run(policy, work),
            Fail: conflict => Fan(runtime, Conflicted(runtime, row, conflict)).Bind(_ => work));

    // Enforcement folds the FACULTY set, never the row set: one control governs a faculty's whole breaker set
    // as one verb, so a hop registered after the sweep inherits the pinned isolation. Both verbs are idempotent.
    public static IO<Unit> Enforce(OutboundRuntime runtime, DegradationLevel effective) =>
        HopRows.Items.Map(static row => row.Needs).Distinct()
            .TraverseM(group => IO.liftAsync(async () => {
                await (effective.Retains.Admits(group)
                    ? runtime.Evidence.Breaker(group).CloseAsync()
                    : runtime.Evidence.Breaker(group).IsolateAsync());
                return unit;
            }))
            .As()
            .Map(static _ => unit);

    static IO<HopReceipt> Fan(OutboundRuntime runtime, HopReceipt receipt) =>
        runtime.Sink.Send(Correlation.Mint(), TenantContext.Current, TelemetrySource.AppHost, ReceiptKind.Hop.Key,
            JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host)).Map(_ => receipt);

    // ONE recording seat for every entry. The pooled context is the bracket's resource, so the release arm runs
    // on the raise path a hand `finally` also covered and on the rejection path it did not; the attempt ordinal
    // and the carried value both ride that context, so nothing survives the execution that produced them.
    static IO<HopSettled<T>> Execute<T>(
        OutboundRuntime runtime, HopPolicy row, OutboundHop hop,
        Func<CancellationToken, Task<(HopOutcome Outcome, T Value)>> send, Option<ILatencyContext> latency) =>
        from start in IO.lift(() => runtime.Clocks.Line.Capture(Op.Of()))
        from settled in IO.liftAsync(envIO => ValueTask.FromResult(Leased(runtime, row, hop, envIO.Token))).Bracket(
            Use: context => Dialed(runtime, row, context, send).Bind(fold => Sealed(runtime, row, start, context, fold)),
            Catch: error => IO.pure(new HopSettled<T>(
                Conflicted(runtime, row, HopFault.Of(error)), Fin.Fail<T>(error))),
            Fin: context => IO.lift(() => {
                ResilienceContextPool.Shared.Return(context);
                latency.Iter(held => ignore(LatencySpine.Mark(held, runtime.Checkpoint)));
                return unit;
            }))
        select settled;

    static ResilienceContext Leased(OutboundRuntime runtime, HopPolicy row, OutboundHop hop, CancellationToken token) {
        ResilienceContext context = ResilienceContextPool.Shared.Get(hop.IdempotencyKey, token);
        context.Properties.Set(HopContext.Window, Duration.FromTimeSpan(runtime.Allotted(DeadlineClass.HopTotal)));
        context.Properties.Set(HopContext.Attempts, 1);
        return context;
    }

    // `ExecuteOutcomeAsync` carries the one statement body on this owner, the same platform-forced shape the
    // registry fold takes. `TryGetPipeline` keeps an unregistered key a typed refusal rather than a throw in
    // any request path, and each produced value lands on the EXECUTION's own context inside the callback, so
    // a delivered outcome answers the value its own delivering attempt set.
    static IO<(HopOutcome Outcome, Option<T> Value)> Dialed<T>(
        OutboundRuntime runtime, HopPolicy row, ResilienceContext context,
        Func<CancellationToken, Task<(HopOutcome Outcome, T Value)>> send) =>
        IO.liftAsync(async envIO => {
            if (!runtime.Pipelines.TryGetPipeline<HopOutcome>(row.Key.Value, out ResiliencePipeline<HopOutcome>? pipeline)) {
                return (new HopOutcome.Refused(new HopFault.ContractBroken($"<unregistered-hop:{row.Key.Value}>")), Option<T>.None);
            }
            Outcome<HopOutcome> captured = await pipeline!.ExecuteOutcomeAsync(
                static async (ctx, state) => {
                    (HopOutcome outcome, T value) = await state(ctx.CancellationToken).ConfigureAwait(false);
                    ctx.Properties.Set(HopCarry<T>.Slot, Some(value));
                    return Outcome.FromResult(outcome);
                },
                context, send);
            return (Fold(captured, envIO.Token), context.Properties.GetValue(HopCarry<T>.Slot, Option<T>.None));
        });

    static IO<HopSettled<T>> Sealed<T>(
        OutboundRuntime runtime, HopPolicy row, Fin<MonotonicStamp> start, ResilienceContext context,
        (HopOutcome Outcome, Option<T> Value) fold) =>
        from end in IO.lift(() => runtime.Clocks.Line.Capture(Op.Of()))
        from span in IO.lift(() =>
            from opened in start
            from closed in end
            from elapsed in runtime.Clocks.Line.Elapsed(opened, closed, Op.Of())
            select new GaugedSpan<DeadlineClass>(
                DeadlineClass.HopTotal, Op.Of(), elapsed, runtime.Allotted(DeadlineClass.HopTotal)))
        select new HopSettled<T>(
            HopReceipt.Of(
                row.Key, fold.Outcome,
                span.ToOption().Map(measured => new HopDial(context.Properties.GetValue(HopContext.Attempts, 1), measured)),
                runtime.BreakerState(row)),
            fold.Outcome.Switch(
                delivered: _ => fold.Value.ToFin(new HopFault.Empty()),
                refused: static refused => Fin.Fail<T>(refused.Reason),
                faulted: static faulted => Fin.Fail<T>(faulted.Reason)));

    // One total fold, ordered child-before-parent so operator-forced darkness never reads as a dependency
    // open, and cancelled-versus-rejected stays structural: the caller's token decides which of the two this
    // cancellation was. Every arm binds its own typed evidence, so escalation matches the case.
    static HopOutcome Fold(Outcome<HopOutcome> captured, CancellationToken caller) => captured switch {
        { Exception: null, Result: { } outcome } => outcome,
        { Exception: OperationCanceledException cancelled } when caller.IsCancellationRequested => new HopOutcome.Faulted(
            new HopFault.CallerLeft(Captured(cancelled))),
        { Exception: TimeoutRejectedException slow } => new HopOutcome.Faulted(
            new HopFault.Deadline(slow.Timeout, Captured(slow))),
        { Exception: IsolatedCircuitException dark } => new HopOutcome.Faulted(new HopFault.ForcedDark(
            HopKey.Named(dark.TelemetrySource?.PipelineName), Captured(dark))),
        { Exception: BrokenCircuitException open } => new HopOutcome.Faulted(
            new HopFault.Open(open.RetryAfter, Captured(open))),
        { Exception: RateLimiterRejectedException shed } => new HopOutcome.Faulted(
            new HopFault.Shed(shed.RetryAfter, Captured(shed))),
        { Exception: { } foreign } => new HopOutcome.Faulted(new HopFault.Foreign(Captured(foreign))),
        _ => new HopOutcome.Faulted(new HopFault.Empty()),
    };

    static Error Captured(Exception raised) => Error.New(raised.Message, raised);

    static HopReceipt Conflicted(OutboundRuntime runtime, HopPolicy row, Error reason) =>
        HopReceipt.Of(row.Key, new HopOutcome.Refused(reason), None, runtime.BreakerState(row));
}

// Slot keys per payload TYPE, so one static serves every T without a generic member minting a fresh key per
// call, and each slot lives on the EXECUTION's own context and dies with it.
public static class HopCarry<T> {
    public static readonly ResiliencePropertyKey<Option<T>> Slot = new($"rasm.hop.carried:{typeof(T).FullName}");
}
```

## [06]-[DISCOVERY_ATTACH]

- Owner: `DiscoveryManifest` attach record; `ContractGeneration` the contract coordinate a peer advertises — the generated descriptor's whole package, with `Compute` derived once from the generated `ComputeReflection.Descriptor` package and never hand-spelled or re-read at a caller; `CompanionChild` spawn capsule; `Discovery` static surface — path law, atomic publish, staleness probe, contract gate, UDS connect, spawn, the named drain-verb arrow, and the drain-fan producer.
- Cases: `Compatible` admits the peer manifest on package EQUALITY and refuses everything else as `HopFault.ContractBroken` — no additive arm exists, because a widening lands a retired field in the proto3 unknown set at the consumer while a peer built off a foreign emission answers shapes this consumer never admits.
- Entry: `Read(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract)` — `Fin` aborts on missing, empty, or dead-pid manifests; `Compatible(DiscoveryManifest peer, ContractGeneration local)` returns `Fin<DiscoveryManifest>`; `FanOf(OutboundRuntime runtime, ILatencyContext latency, FactSink<CompanionSignal> fan, Duration cooperative, string reason)` seats the drain-fan producer and its existing AppHost evidence sink, taking the parent's remaining cooperative allotment as the budget the child inherits; `DrainVerb(Duration cooperative, string reason)` invokes `ControlService.DrainRuntime` and returns its generated `DrainRuntimeResponse` for settlement.
- Packages: Grpc.Net.Client (`GrpcChannel.ForAddress`; a `ConnectCallback` handler resolves `HttpHandlerType.Custom`, which forfeits the balancer surface WHOLE yet reports only half that forfeit — `ConnectAsync`, `State`, and `WaitForStateChangedAsync` raise `InvalidOperationException` loudly, while `CallOptions.WithWaitForReady(true)` accepts and does nothing, because `Custom` takes the passive subchannel transport whose connect reports Ready without connecting and leaves the one `PickResultType.Fail` arm that reads the flag unreachable), Grpc.Core.Api (`RpcException.StatusCode`/`Status.Detail`, `Metadata` — every remote fault leaves as a throw), Rasm.Contracts (project — `ControlService.ControlServiceClient`, `DrainRuntimeRequest`, `DrainRuntimeResponse`, and `ComputeReflection.Descriptor`), Google.Protobuf (`FileDescriptor.Package`), NodaTime.Serialization.Protobuf (`ToProtobufDuration` carries the inherited remainder onto the drain request), Rasm (kernel `Redrive`), LanguageExt.Core, NodaTime, BCL inbox
- Growth: the connect dispatch is the single Unix-domain-socket route; socket-file mode and the accept-side peer-credential read are the access boundary, never a transport-level ACL; a second family a peer must advertise is one more `ContractGeneration` column on the manifest beside one more derived static off its `<F>Reflection.Descriptor`; zero new surface.
- Boundary: `Publish` and `Connect` are the named boundary capsules carrying statement bodies — atomic temp-write-then-move publication and the UDS connect callback; the socket lives at the temp-root `rasm-{pid}.sock` path under the 104-byte `sun_path` cap; the manifest directory is created 0700 and directory mode is the credential boundary; `Compatible` compares the descriptor package whole — the one corpus emission every branch regenerates from is the one compatibility authority, a retired field lands in the peer's unknown set, and serialized-descriptor byte equality is refused by construction because buf's image and protoc's `FileDescriptorProto` bytes diverge on `json_name` and option encoding, so `SerializedData` equality across a C# host and a Connect peer is a falsehood; the spawn's post-spawn manifest read is the ONE in-process re-drive on this spine — `CompanionSpawn` is `SingleShot`, so its pipeline arms no retry row at all and a manifest a child has not yet published is a `StaleManifest` the kernel discriminant marks transient, which is exactly the row `OutboundSurface.Redriven` re-drives on the caller's own `RedrivePolicy`; `DrainFan` is the parent-to-child drain-fan producer — it dials the peer over the `LocalIpc` hop case through `OutboundSurface.Run` and invokes `DrainVerb`, the ONE named `ControlService.DrainRuntime` arrow, returning the `IO<Unit>` that `Spawn` threads into `CompanionChild.FanDrain` as the `drainFan` arg, so the drain conductor fans onto a named verb rather than a delegate any caller fills; the client arrow admits the generated `DrainRuntimeResponse`, derives the worst validated `DeadlineOutcome`, and fans the whole generated message as canonical ProtoJSON through `CompanionSignal.Drain` before settling the hop — no response is ignored, no local reply mirror exists, and `Met`, `Escalated`, and `Forced` are all delivered evidence; only a schema-valid reply whose `final_phase` is not `UNLOADED` is `HopFault.ContractBroken`, because that response refuses the drain method's completion postcondition rather than describing a legal straggler policy; liveness over this transport is an actual CALL, since the channel forfeits every connectivity member — a connect-probe drain raises `InvalidOperationException` on every fan and proves nothing about the peer, and wait-for-ready is the silent face of that one forfeit rather than a second remedy beside it; the redial and the retry cover DISJOINT fault classes and never stack, so nothing here is missing a third wait — `HopStrategy.ArmRedial` handles `ExecutionRejectedException` alone (a shed limiter, an open circuit) while a dead-peer connect arrives as `HttpRequestException`/`SocketException` on the retry row's `Transient` predicate, and wait-for-ready contributes its queueing only where the handler type admits the flag at all, which this channel never does; the accepted-socket peer-credential read moves to `Wire/companion#PEER_ADMISSION` (the serving side reads the connecting peer's uid and pid once at accept and never trusts the manifest) — a seam-split, not an owner here.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// Generated descriptors are the ONE spelling: every estate package reads `rasm.contracts.<family>`, so the family
// splits off a fixed prefix and a descriptor outside that namespace is a foreign emission this attach never admits.
public sealed record ContractGeneration(string Family) {
    private const string Estate = "rasm.contracts.";

    // Compute's local generation derives once off the committed emission this assembly references; a caller
    // re-deriving it from a second descriptor read or a literal is the knob this member deletes.
    public static readonly Fin<ContractGeneration> Compute = Of(ComputeReflection.Descriptor);

    public string Coordinate => $"{Estate}{Family}";

    public static Fin<ContractGeneration> Of(FileDescriptor file) =>
        file.Package.StartsWith(Estate, StringComparison.Ordinal) && file.Package.Length > Estate.Length
            ? Fin.Succ(new ContractGeneration(file.Package[Estate.Length..]))
            : Fin.Fail<ContractGeneration>(new HopFault.ContractBroken($"<foreign-package:{file.Package}>"));
}

public sealed record DiscoveryManifest(
    int Pid,
    string SocketPath,
    Instant StartInstant,
    ContractGeneration Contract,
    long StoreEpoch);

public sealed record CompanionChild(
    Process Child,
    DiscoveryManifest Manifest,
    Func<CancellationToken, IO<Unit>> FanDrain);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Discovery {
    public const int SunPathMax = 104;
    public const UnixFileMode SocketDirMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    public static string ManifestPath(ProfileRoots roots, int pid) =>
        Path.Join(roots.AppRoot, "discovery", $"rasm-{pid}.json");

    public static Fin<string> SocketPath(int pid) =>
        Path.Join(Path.GetTempPath(), $"rasm-{pid}.sock") is var path && Encoding.UTF8.GetByteCount(path) <= SunPathMax
            ? Fin.Succ(path)
            : Fin.Fail<string>(new HopFault.ContractBroken($"sun_path over {SunPathMax} bytes: {path}"));

    public static IO<DiscoveryManifest> Publish(DiscoveryManifest manifest, ProfileRoots roots, JsonTypeInfo<DiscoveryManifest> contract) =>
        IO.lift(() => {
            var path = ManifestPath(roots, manifest.Pid);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!, SocketDirMode);
            File.WriteAllBytes($"{path}.tmp", JsonSerializer.SerializeToUtf8Bytes(manifest, contract));
            File.Move($"{path}.tmp", path, overwrite: true);
            return manifest;
        });

    public static Fin<DiscoveryManifest> Read(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract) =>
        Op.Of().Catch(() => Fin.Succ(Optional(JsonSerializer.Deserialize(File.ReadAllBytes(ManifestPath(roots, pid)), contract))))
            .MapFail(static error => HopFault.Of(error))
            .Bind(manifest => manifest.ToFin(new HopFault.StaleManifest($"empty manifest: {pid}")))
            .Bind(static manifest => Alive(manifest));

    // Equality on the package IS the handshake: unknown fields are tolerated by the parser, one corpus emission generates
    // both peers, and no descriptor byte crosses — buf's image and protoc's bytes never agree on one source.
    public static Fin<DiscoveryManifest> Compatible(DiscoveryManifest peer, ContractGeneration local) =>
        peer.Contract == local
            ? Fin.Succ(peer)
            : Fin.Fail<DiscoveryManifest>(new HopFault.ContractBroken($"{peer.Contract.Coordinate}!={local.Coordinate}"));

    public static GrpcChannel Connect(DiscoveryManifest peer, GrpcChannelPolicy policy) =>
        GrpcChannel.ForAddress(new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri, new GrpcChannelOptions {
            MaxSendMessageSize = policy.MaxSendBytes,
            MaxReceiveMessageSize = policy.MaxReceiveBytes,
            HttpHandler = new SocketsHttpHandler {
                PooledConnectionIdleTimeout = policy.PooledConnectionIdle,
                KeepAlivePingDelay = policy.KeepAlivePingDelay,
                KeepAlivePingTimeout = policy.KeepAlivePingTimeout,
                EnableMultipleHttp2Connections = policy.EnableMultipleHttp2Connections,
                ConnectCallback = async (_, cancel) => {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    try {
                        await socket.ConnectAsync(new UnixDomainSocketEndPoint(peer.SocketPath), cancel);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch {
                        socket.Dispose();
                        throw;
                    }
                },
            },
        });

    // Children publish their manifest AFTER booting, so this read races the spawn by construction. `HopRows`
    // marks the spawn row `SingleShot` and arms no retry strategy, which is what makes the schedule owner's
    // claim lawful here — the re-drive rides `StaleManifest`'s transient discriminant and nothing else.
    public static IO<CompanionChild> Spawn(
        OutboundRuntime runtime, ProcessStartInfo spec, RedrivePolicy attach,
        Func<int, Fin<DiscoveryManifest>> manifestOf, Func<DiscoveryManifest, CancellationToken, IO<Unit>> drainFan) =>
        from child in Started(spec)
        from manifest in OutboundSurface.Redriven(
            runtime, HopRows.CompanionSpawn, attach,
            IO.lift(() => manifestOf(child.Id))
                .Bind(static read => read.Match(Succ: IO.pure, Fail: IO.fail<DiscoveryManifest>)))
        select new CompanionChild(child, manifest, cancel => drainFan(manifest, cancel));

    static IO<Process> Started(ProcessStartInfo spec) =>
        IO.lift(() => Op.Of().Catch(() => Fin.Succ(Optional(Process.Start(spec))))
                .MapFail(static error => HopFault.Of(error))
                .Bind(child => child.ToFin(new HopFault.SpawnRejected(spec.FileName))))
            .Bind(static started => started.Match(Succ: IO.pure, Fail: IO.fail<Process>));

    public static Func<DiscoveryManifest, CancellationToken, IO<Unit>> FanOf(
        OutboundRuntime runtime, ILatencyContext latency, FactSink<CompanionSignal> fan,
        Duration cooperative, string reason) =>
        (peer, token) => DrainFan(peer, runtime, latency, fan, DrainVerb(cooperative, reason), token);

    // Naming the verb is what closes the arrow: `Wire/companion#CONTROL_SERVICE` owns the serving half, and
    // `ControlService` reaches this assembly through the `Rasm.Contracts` project reference — the one committed
    // emission every consumer shares, never a per-consumer compile. Cooperative REMAINDER rides the request
    // rather than a fresh child budget: the child folds the minimum of its own drain class and what arrives, so
    // a child re-arming its full allotment cannot outlive the parent drain that asked for it.
    public static Func<GrpcChannel, CancellationToken, Task<DrainRuntimeResponse>> DrainVerb(Duration cooperative, string reason) =>
        async (channel, token) => await new ControlService.ControlServiceClient(channel).DrainRuntimeAsync(
            new DrainRuntimeRequest { Reason = reason, Cooperative = cooperative.ToProtobufDuration() },
            TraceContext.Inject(new Metadata()), cancellationToken: token);

    // Unix-domain channels forfeit the connectivity surface entirely, so a connect probe here is an
    // unconditional throw the hop reports as a foreign fault on every fan. Liveness is an actual CALL, which is
    // why the fan rides `DrainVerb` while the parameter stays a delegate: one seam names the verb and this fold
    // still folds against a stub peer. Converting `RpcException` HERE keeps the fan a receipt rather than a
    // raise, since the retry row admits only `Unavailable`.
    public static IO<Unit> DrainFan(DiscoveryManifest peer, OutboundRuntime runtime, ILatencyContext latency,
        FactSink<CompanionSignal> fan,
        Func<GrpcChannel, CancellationToken, Task<DrainRuntimeResponse>> control, CancellationToken token) =>
        OutboundSurface.Run(runtime, new OutboundHop.LocalIpc(peer), inner =>
            Drain(peer, Connect(peer, GrpcChannelPolicy.Canonical), fan, control,
                CancellationTokenSource.CreateLinkedTokenSource(token, inner).Token), latency)
            .Map(static _ => unit);

    static async Task<HopOutcome> Drain(
        DiscoveryManifest peer, GrpcChannel channel, FactSink<CompanionSignal> fan,
        Func<GrpcChannel, CancellationToken, Task<DrainRuntimeResponse>> control, CancellationToken token) {
        await using (channel) {
            try {
                DrainRuntimeResponse received = await control(channel, token);
                Op key = Op.Of(ControlVerb.DrainRuntime.Key);
                return await WireAdmission.Admit(received, WireBoundary.InboundPayload, key).Match(
                    Succ: reply => Fanned(peer, fan, reply, key).RunAsync().AsTask(),
                    Fail: error => Task.FromResult<HopOutcome>(new HopOutcome.Faulted(HopFault.Of(error))));
            }
            catch (RpcException refused) when (refused.StatusCode != StatusCode.Unavailable) {
                return new HopOutcome.Faulted(new HopFault.Foreign(Error.New(refused.Message, (Exception)refused)));
            }
        }
    }

    // The generated enum is the severity vocabulary. Proto admission above has already refused unspecified
    // or foreign values, so this fold only ranks the three authored arms and carries the reply whole.
    static global::Rasm.Contracts.Compute.DeadlineOutcome Outcome(DrainRuntimeResponse reply) =>
        toSeq(reply.Steps).Fold(global::Rasm.Contracts.Compute.DeadlineOutcome.Met, static (held, step) =>
            step.Outcome switch {
                global::Rasm.Contracts.Compute.DeadlineOutcome.Met => held,
                global::Rasm.Contracts.Compute.DeadlineOutcome.Escalated =>
                    held == global::Rasm.Contracts.Compute.DeadlineOutcome.Forced ? held : step.Outcome,
                global::Rasm.Contracts.Compute.DeadlineOutcome.Forced => step.Outcome,
                _ => held,
            });

    static IO<HopOutcome> Fanned(
        DiscoveryManifest peer, FactSink<CompanionSignal> fan, DrainRuntimeResponse reply, Op key) =>
        HostWire.Correlation(reply.Correlation, key).Match(
            Succ: correlation => fan.Fan(
                    correlation,
                    ControlVerb.DrainRuntime.Key,
                    WireJson.Element(reply),
                    new CompanionSignal.Drain(peer.Pid, Outcome(reply), reply))
                .Map(_ => reply.FinalPhase == global::Rasm.Contracts.Compute.RuntimePhase.Unloaded
                    ? (HopOutcome)new HopOutcome.Delivered()
                    : new HopOutcome.Faulted(new HopFault.ContractBroken(
                        $"{DrainRuntimeResponse.Descriptor.FullName}.final_phase:{reply.FinalPhase}"))),
            Fail: error => IO.pure<HopOutcome>(new HopOutcome.Faulted(HopFault.Of(error))));

    static Fin<DiscoveryManifest> Alive(DiscoveryManifest manifest) =>
        Op.Of().Catch(() => Fin.Succ(Process.GetProcessById(manifest.Pid)))
            .MapFail(HopFault.Of)
            .Map(_ => manifest);
}
```

## [07]-[DELIVERY_FANOUT]

- Owner: `DeliveryTarget` `[Union]` the channel-target carrier (endpoint `Uri` versus discovered peer manifest); `DeliveryChannel` `[SmartEnum<string>]` the outbound notification-channel axis under the hop key policy; `DeliveryMessage` the channel-agnostic notification payload; native `DeliveryDisposition` and `DeliveryReceipt` the per-channel account and evidence; `DeliveryFanout` the static multi-channel fan surface.
- Cases: 2 target cases — `Endpoint(Uri)` for the network-borne channels, `Peer(DiscoveryManifest)` for the in-app companion channel; 4 channel rows — push, webhook, email, in-app — each binding the `OutboundHop` its bytes ride through a target-discriminating `Hop(DeliveryTarget, string)` returning `Fin<OutboundHop>`: push and webhook on `WebhookPost` over an `Endpoint`, email on `HttpApi` over an `Endpoint` (the transactional-mail API), in-app on `LocalIpc` over a `Peer` manifest; a channel fed the wrong target shape returns `HopFault.Excluded` so the in-app channel can never forge a null manifest and a network channel can never dial a peer. The generated `DeliveryDisposition` enum owns dialed, suppressed, and unbound.
- Entry: `Fan(DeliveryRuntime runtime, DeliveryMessage message, params ReadOnlySpan<DeliveryChannel> channels)` returns `IO<Seq<DeliveryReceipt>>` — deduplicates the message by its idempotency key, then fans it to each channel through the channel's `OutboundHop` under a bounded fork width, so one notification reaches every configured channel under one dedupe guard.
- Auto: every channel rides its `OutboundHop` so delivery inherits the hop's retry, breaker, rate-limit, and deadline — a flapping webhook endpoint breaks on the existing circuit breaker and a rate-capped push channel admits through the existing sliding-window limiter, never a per-channel retry loop; the dedupe verdict is one `DedupeWindow.Admit(key, now)` call against the `Runtime/resources#DEDUPE_WINDOW` bounded seen-key window — a `true` is the first admission and a `false` is a key still holding an unexpired deadline — so the expiry prune, the capacity ceiling, and the admit-record race are the primitive's while the instant stays this composition's own `ClockPolicy` read, and this page carries no cell and no window column; each channel mints one native receipt carrying the channel, disposition, and hop verdict so a partial fan records every channel independently; the fan SCHEDULES each window's legs through `IO.Fork` before it awaits them, so channels inside a window genuinely overlap while the width stays the composition's own budget; the evidence leg stays sequential after the join because the receipt stream is the one ordered record of what the fan did; the evidence leg fires the `Observability/hooks#HOOK_ROSTER` `Delivery` row through `rail.Fire(AppHostPoint.Delivery, new AppHostFact.Delivery(receipt), key)`, so a per-channel subscriber reads the typed receipt while the envelope carries the same local evidence.
- Receipt: native `DeliveryReceipt` — channel, optional typed verdict (`None` on a suppressed leg, which ran no hop and earned none) beside the disposition, optional measured attempts/duration, optional fenced watermark, and optional kernel fault observation. The idempotency key governs dedupe and the envelope correlation governs joining; neither is copied into the receipt.
- Packages: Rasm (kernel `HookRail`/`FaultObservation`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one channel row absorbs a new delivery medium — a new SMS or chat channel is one `DeliveryChannel` row binding its `OutboundHop` over the matching `DeliveryTarget` case, never a parallel sender; a new target shape is one `DeliveryTarget` case breaking every channel's `Hop` switch; a new account of a leg lands on the corpus enum and producer switch; zero new surface.
- Boundary: the delivery fan-out is the only multi-channel notification owner — a per-channel sender, a notification service wrapper, and a parallel delivery queue are the deleted forms, so all channels ride one fan and one dedupe; delivery never owns its own resilience — each channel composes its `OutboundHop` so the retry-owner, breaker, and rate-limit are the existing hop policy, and the delivery fan is purely the fan-and-dedupe layer above the hops; the dedupe is bounded and NOT owned here — `DedupeWindow` at `Runtime/resources#DEDUPE_WINDOW` is the one TTL-and-capacity seen-key window, composed by this fan and by `Wire/topics#SUBSCRIPTION_FABRIC` alike, so a long-lived process accumulates no unbounded dedup state under either bound and a local idempotency-key map beside it is the twin that primitive deleted; the disposition is the fan's honesty axis — a suppressed leg and an unbound leg each ran NO hop, so an attempt count and an elapsed span are measurements nobody took and a receipt reporting them as zeros is indistinguishable from a dial that returned instantly; the unbound arm carries the binding `Error` beside that absence, so a fan that fails to resolve a target names why on the receipt rather than on a log line; fork width is the composition's own worker budget rather than the channel count, because `IO.Fork` spins one dedicated long-running thread per leg with no pool ceiling, so an unwindowed fan is an unbounded thread count and a bare traverse over the deliveries is the opposite deleted form that sequences them and makes the partial-fan claim prose over a serial loop; the fan is the scheduled-delivery consumer — a `ScheduleEntry` row fires the fan on its cadence so scheduled multi-channel delivery is one schedule row and one fan call, never a second scheduler; the in-app channel rides the `LocalIpc` hop over a `DeliveryTarget.Peer` carrying the attached companion's `DiscoveryManifest` so an in-app notification reaches the companion over the control hop with a real peer manifest, never a `default!` placeholder and never a separate transport; the message's idempotency key threads INTO the hop case, so the fan's dedupe key, the pipeline's `OperationKey`, and the receiver's dedup key are one value rather than three mints of one intent.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryTarget {
    private DeliveryTarget() { }

    public sealed record Endpoint(Uri Authority) : DeliveryTarget;
    public sealed record Peer(DiscoveryManifest Manifest) : DeliveryTarget;
}

// Delivery identity threads the MESSAGE's own idempotency key into the hop case, so the key the fan dedupes
// on, the key the pipeline leases as its `OperationKey`, and the key the receiver dedupes on are ONE value.
[SmartEnum<string>]
public sealed partial class DeliveryChannel {
    public static readonly DeliveryChannel Push = new("push", static (target, key) => target.Switch(
        state: key,
        endpoint: static (e, k) => Fin<OutboundHop>.Succ(new OutboundHop.WebhookPost(e.Authority, k)),
        peer: static (_, _) => Fin<OutboundHop>.Fail(new HopFault.Excluded("push:requires-endpoint"))));
    public static readonly DeliveryChannel Webhook = new("webhook", static (target, key) => target.Switch(
        state: key,
        endpoint: static (e, k) => Fin<OutboundHop>.Succ(new OutboundHop.WebhookPost(e.Authority, k)),
        peer: static (_, _) => Fin<OutboundHop>.Fail(new HopFault.Excluded("webhook:requires-endpoint"))));
    public static readonly DeliveryChannel Email = new("email", static (target, _) => target.Switch(
        endpoint: static e => Fin<OutboundHop>.Succ(new OutboundHop.HttpApi(e.Authority)),
        peer: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("email:requires-endpoint"))));
    public static readonly DeliveryChannel InApp = new("in-app", static (target, _) => target.Switch(
        endpoint: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("in-app:requires-peer")),
        peer: static p => Fin<OutboundHop>.Succ(new OutboundHop.LocalIpc(p.Manifest))));

    [UseDelegateFromConstructor]
    public partial Fin<OutboundHop> Hop(DeliveryTarget target, string idempotencyKey);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeliveryDisposition {
    public static readonly DeliveryDisposition Dialed = new("dialed");
    public static readonly DeliveryDisposition Suppressed = new("suppressed");
    public static readonly DeliveryDisposition Unbound = new("unbound");
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DeliveryMessage(
    string IdempotencyKey,
    string Subject,
    JsonElement Body,
    DataClassification Classification,
    HashMap<DeliveryChannel, DeliveryTarget> Targets);

// `Outcome` is `Option<HopVerdict>` because a suppressed leg ran NO hop: `None` is the honest account of a
// verdict nobody produced, where the `Delivered` stamp made a fan whose every leg was suppressed read as a
// healthy delivery on the availability objective. An unbound leg DID refuse — its binding failed — so it
// carries `Some(Refused)` beside the binding fault.
public sealed record DeliveryReceipt(
    string Channel,
    Option<HopVerdict> Outcome,
    DeliveryDisposition Disposition,
    Option<HopMeasure> Measure,
    Option<ulong> Watermark,
    Option<FaultObservation> Fault) {
    public static DeliveryReceipt Dialed(string channel, HopSettled<Unit> settled, Option<ulong> watermark) => new(
        channel,
        Some(settled.Receipt.Outcome),
        settled.Receipt.Measure.IsSome ? DeliveryDisposition.Dialed : DeliveryDisposition.Unbound,
        settled.Receipt.Measure,
        watermark,
        settled.Receipt.Fault);

    public static DeliveryReceipt Suppressed(string channel) => new(
        channel, None, DeliveryDisposition.Suppressed, None, None, None);

    public static DeliveryReceipt Unbound(string channel, Error error) => new(
        channel, Some(HopVerdict.Refused), DeliveryDisposition.Unbound, None, None, Some(FaultObservation.Of(error)));
}

// Watermark is the raw `ulong` on PURPOSE: `OutboxOrdinal` is `Wire/outbox#OUTBOX_FABRIC`'s value object and
// that page composes this one, so naming it here would invert the page direction into a cycle. The relay hands
// down the admitted ordinal's value and re-enters the alphabet at its own owner; the WIRE narrowing to ordinal
// text lands at this page's mapper, so the carrier stays direction-safe without forking the wire spelling.
// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record DeliveryRuntime(
    OutboundRuntime Outbound,
    Func<OutboundHop, DeliveryMessage, Func<CancellationToken, Task<HopOutcome>>> Send,
    DedupeWindow Dedupe,
    // Fork width is the composition's worker budget: `IO.Fork` takes a dedicated long-running thread per leg
    // and the pool imposes no ceiling, so the fan's overlap is a declared number rather than the roster size.
    int FanWidth,
    ILatencyContext Latency,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    HookRail<AppHostPoint, AppHostFact, TelemetrySource> Rail);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class DeliveryFanout {
    public static IO<Seq<DeliveryReceipt>> Fan(
        DeliveryRuntime runtime, DeliveryMessage message, params ReadOnlySpan<DeliveryChannel> channels) =>
        // ONE admission against the shared bounded window, ABOVE the fan: TRUE is first sight, FALSE is a
        // replay still holding an unexpired deadline. The verdict is per MESSAGE, so every channel of one
        // admission delivers — the per-channel admit let the first leg record the key and every later leg of
        // the same fan read its own message back as a replay. The prune, the ceiling, and the admit-record
        // race are the primitive's; the instant is this composition's read, which is what lets a spec expire
        // rows against a fake clock.
        IO.lift(() => (
                Correlation: Correlation.Mint(),
                Admitted: runtime.Dedupe.Admit(message.IdempotencyKey, runtime.Clocks.Now))).Bind(frame =>
            Windowed(
                toSeq(channels.ToArray()).Map(channel => frame.Admitted
                    ? Deliver(runtime, channel, message, frame.Correlation)
                    : IO.pure(DeliveryReceipt.Suppressed(channel.Key))),
                runtime.FanWidth)
                .Bind(receipts => receipts.TraverseM(receipt => Evidence(runtime, frame.Correlation, receipt)).As()));

    // Bounded fork width: one window forks and joins whole before the next opens, so legs inside a window
    // overlap while the live thread count never exceeds the budget the composition declared.
    static IO<Seq<DeliveryReceipt>> Windowed(Seq<IO<DeliveryReceipt>> legs, int width) =>
        legs.IsEmpty
            ? IO.pure(Seq<DeliveryReceipt>())
            : legs.Take(int.Max(width, 1)).TraverseM(static leg => leg.Fork()).As()
                .Bind(handles => handles.TraverseM(static handle => handle.Await).As())
                .Bind(head => Windowed(legs.Skip(int.Max(width, 1)), width).Map(tail => head + tail));

    // Every leg — dialed, suppressed, unbound — fans its evidence envelope; the arm counts outcomes. The typed
    // Delivery fact fires beside that envelope because a subscriber reading channel, outcome, and disposition
    // off the serialized wire re-derives what the receipt already carries.
    static IO<DeliveryReceipt> Evidence(
        DeliveryRuntime runtime, CorrelationId correlation, DeliveryReceipt receipt) =>
        runtime.Rail.Fire(at: AppHostPoint.Delivery, fact: new AppHostFact.Delivery(receipt), key: Op.Of()).Bind(_ =>
            runtime.Sink.Send(correlation, TenantContext.Current, TelemetrySource.AppHost, ReceiptKind.Delivery.Key,
                JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host))).Map(_ => receipt);

    static IO<DeliveryReceipt> Deliver(
        DeliveryRuntime runtime, DeliveryChannel channel, DeliveryMessage message, CorrelationId correlation) =>
        (from target in message.Targets.Find(channel).ToFin(new HopFault.ContractBroken($"no-target:{channel.Key}"))
         from hop in channel.Hop(target, message.IdempotencyKey)
         select hop).Match(
            Succ: bound => OutboundSurface.Dispatch<Unit>(runtime.Outbound, bound,
                    async token => (await runtime.Send(bound, message)(token).ConfigureAwait(false), unit), runtime.Latency)
                .Map(settled => DeliveryReceipt.Dialed(channel.Key, settled, watermark: None)),
            Fail: error => IO.pure(DeliveryReceipt.Unbound(channel.Key, error)));
}
```

## [08]-[RECEIPT_FAMILY]

- Owner: `HopVerdict`, `HopMeasure`, `HopReceipt`, `DeliveryDisposition`, and `DeliveryReceipt` are the one native transport-evidence family; the execution-only `HopOutcome` is projected once at the receipt mint.
- Packages: Rasm (kernel `FaultObservation`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new terminal hop result extends `HopOutcome` and breaks `HopVerdict.Of`; a new delivery account is one `DeliveryDisposition` row and one mint arm.
- Boundary: these receipts never cross a process boundary. The app-root `SuiteContracts.Host` serializer carries them only through the local receipt sink; no protobuf, TypeScript face, generated enum, or second projection repeats them.

Outbound carries no open research. Accepted-socket peer-credential projection moves to the serving owner at `Wire/companion#PEER_ADMISSION`, where the P/Invoke `getsockopt` route and the `ucred`/`xucred` blittable layout seat the admission fence. Transactional-mail channel target API resolves at app-root creation behind the same app-root pin the OTLP exporter rides; the channel rows bind the `OutboundHop` only, never the provider client, so a mail provider is one channel target Uri, never a delivery-page client.

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
