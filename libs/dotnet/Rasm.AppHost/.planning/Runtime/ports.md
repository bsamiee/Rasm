# [APPHOST_RUNTIME_PORTS]

Rasm.AppHost exposes exactly seven typed port records as its only INWARD cross-package seam; siblings adapt to them and no sibling assembly enters the AppHost graph. Outward vocabularies cross the same boundary as settled row payloads, never reversing an interior dependency. Owned axes are the port-record family under its cardinality invariant, the boot tenancy mint, and the host wire edge — the one ProtoJSON registry, the one fault wire, and the envelope projection onto the generated host family. Drain bands, deadline rows, phase vocabulary, classification, and degradation arrive settled as port payloads.

Settled composition: `CorrelationId` arrives from the kernel frame capsule `Rasm/Domain/frame#SOURCE`, `TenantId`/`TenantContext` and `TenantMirror` from `#TENANCY`, `ReceiptEnvelope` and `ReceiptSinkPort` with its one HLC mint from `#RECEIPT_PORT`, and `TelemetryContributorPort` from `Rasm/Domain/telemetry#CONTRIBUTE`.

This page seats both kernel records inside the cardinality invariant, mints the boot tenancy value from its tenant-feed configuration, and projects the stamped frame header onto the generated `Receipt.ReceiptHeaderWire`; `Observability/telemetry#SIGNAL_GOVERNANCE` registers the OTel `Baggage.Current` store as the composition `TenantMirror` row, so a kernel caller spells `Stamp()` bare and threads no mirror per call site.

## [01]-[INDEX]

- [02]-[PORT_RECORDS]: Seven inward port records, five declared here and two at the kernel capsule.
- [03]-[WIRE_LAW]: `WireJson` the one ProtoJSON edge, `FaultWire` the one fault wire, the envelope mapper, and the STJ merge's two surviving surfaces.
- [04]-[TS_PROJECTION]: Generated header family the TS dashboard decodes, and the packed-stamp law it retires.

## [02]-[PORT_RECORDS]

- Owner: `ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `HostAttachPort`, `UiSchedulerPort`, `SupportContributorPort`, `HealthContributorPort` — the seven sealed records of delegates and policy values, five declared here and two at the kernel signal capsule so every stratum mints one without an upward reference; zero interfaces, zero inheritance contracts, zero provider-branded vocabulary. `ReceiptEnvelope` is the receipt value the sink port emits, not a port; `TenantContext` and `TenantId` are the kernel tenancy primitives stamped on that value, minted here at boot and consumed by every sibling as settled vocabulary, never ports.
- Cases: the capability axis is `PortCardinality` — five DRIVEN ports the host calls outward into the package interior (`ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `SupportContributorPort`, `HealthContributorPort`) and two DRIVING host-affine ports the host implements at the boundary (`HostAttachPort` injects phase transitions and surfaces the document, `UiSchedulerPort` marshals onto the host UI loop and carries no sample feed — a `ProfileSample` consumer subscribes through a `HookTap<AppHostPoint, AppHostFact, TelemetrySource>` scoped to `AppHostPoint.ProfileSample` at `Observability/hooks#HOOK_ROSTER`, so the published sample reaches its reader on the one hook rail rather than a second port column); `ReceiptSinkPort` is the identity port whose HLC two-half stamp is the sole cross-process correlation, with `TenantContext` partitioning each stamped value.
- Entry: every registration enters as a `TryAddEnumerable` ordered `ServiceDescriptor` row admitted through `PortCardinality.Of(port)` — the `Runtime/modules#SCAN_AND_DECORATE` slot pass is that admission's one execution site — `DescriptorSlot.Contributor.Admits` is the column that carries it and the `Seated` leg folds that column over the module's contributor rows, so each contributor descriptor's service-type name crosses `Of` before it joins the ordered set — and every subscribing port returns disposable detachers composed LIFO, so a port is registered, never resolved by lookup; `TenantContext.Root` is the single-tenant ambient default (`TenantId` zero, slug `root`) and a multi-tenant host mints one row per admitted tenant at boot from its tenant-feed configuration.
- Receipt: `ReceiptEnvelope` carries the one causal frame — the kernel HLC two-half stamp orders evidence and the `Tenant` field partitions it, so every receipt and every content key composes the identical `(tenant, physical, logical)` frame; receipts, support bundles, and degradation stay process-local and correlate across processes solely through that stamp.
- Packages: Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new cross-package seam lands as one registration row on an existing port, zero new surface; a new tenant is one ambient `TenantContext` value minted at boot, never a second tenancy owner.
- Boundary: `PortCardinality` is the conserved invariant — its two direction rows hold every admitted port name and an eighth port record is the named defect: a new aggregate store, compute, companion, or outbound-client port is the rejected form, that content decomposing into rows on these seven, and a content carrier (the `ReceiptEnvelope` value, a `TenantContext` tenancy primitive, a `SecretLease` row, a `FencingToken` value object) is never promoted to a port. Spine owners constructor-injected as settled vocabulary are never ports: `ClockPolicy` (the clock pair), `SchedulePort` (a static fold over `ScheduleEntry` rows despite the `-Port` suffix), `CancelScope` (the cancellation provenance tree), the determinism RNG, the `HopPolicy` outbound rows, and the `CacheLane` L2 cache surface — each is a record or static surface threaded through composition, not a delegate-bearing inward seam. Contributor ports carry the settled row vocabularies — instrument rows, artifact rows, probe rows — never re-spelled fields, and the semconv schema coordinate is the kernel const `TelemetryIdentity.SchemaUrl`, stamped as `MeterOptions.TelemetrySchemaUrl` inside `TelemetryIdentity.Metered` at every contributor mint, so the port carries no schema slot; no sibling assembly enters the AppHost graph. `TenantContext` is a cross-package primitive beside the HLC stamp, the content-address `Hash`, and the boot-minted `CorrelationId`, and this platform is its one minting site: AppHost mints and threads it, the Persistence tenancy owner `dotnet:Rasm.Persistence/Element/identity` stores the canonical `TenantId.Text` render in a `text` column and compares it against `current_setting('rasm.tenant', true)` bare — a `::uuid` provider cast is the deleted form that forks one identity into two alphabets — `dotnet:Rasm.Persistence/Query/cache#L2_CONTRIBUTION` partitions the content-address cache key by `TenantId`, and `TenantSlot` (`rasm.tenant`) is the single GUC and meter-tag spelling every consumer reads; `TenantContext.Tags` rides the one per-instrument view projection at Observability/telemetry#SIGNAL_GOVERNANCE `Views`, which admits `TenantSlot` beside each row's declared dimensions under the governed series ceiling so the per-tenant meter dimension survives the tag projection and never fans unbounded, and the root row contributes no dimension at all so an absent `rasm.tenant` reads single-tenant everywhere; `TenantId` crosses the wire as a `UInt128`-keyed Thinktecture value object whose one `Text` render the RLS predicate, the cache-key partition, and the meter tag all compare byte-identically, never a string parse beside it.

```csharp signature
// ReceiptSinkPort and TelemetryContributorPort declare at the kernel signal capsule — both are string-scoped
// so an emitter outside this platform's reach mints one legally, and both hold their seats in the seven-port
// cardinality with their declarations homed at S0. The five records below are this page's own.

// Conserved seven ride as DATA: each row carries the port names its direction governs, so the cardinality
// invariant is a set membership a registration fold proves rather than a count a reader keeps by hand.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PortCardinality {
    public static readonly PortCardinality Driven = new("driven", Names(
        nameof(ReceiptSinkPort), nameof(TelemetryContributorPort), nameof(DrainParticipantPort),
        nameof(SupportContributorPort), nameof(HealthContributorPort)));
    public static readonly PortCardinality Driving = new("driving", Names(
        nameof(HostAttachPort), nameof(UiSchedulerPort)));

    public FrozenSet<string> Ports { get; }

    // Eighth port record reaches composition as a service type naming no row, refused at the module fold's
    // contributor leg while the collection is still editable rather than discovered later as a leaked inward
    // dependency. The refusal accumulates across the module's whole slot pass, so one module fold names every
    // foreign service type that module carries at once; the composition then aborts naming that module, which
    // is why the refusal is worth accumulating at all — the boot reads one complete verdict per module.
    public static Fin<PortCardinality> Of(string port) =>
        toSeq(Items).Find(row => row.Ports.Contains(port))
            .ToFin(new KernelFault.InvalidValue(Label: port, Requirement: "a declared inward port row"));

    static FrozenSet<string> Names(params ReadOnlySpan<string> ports) =>
        ports.ToArray().ToFrozenSet(StringComparer.Ordinal);
}

public sealed record DrainParticipantPort(
    string Name,
    DrainBand Band,
    int Rank,
    Func<CancellationToken, IO<Unit>> Drain);

public sealed record HostAttachPort(
    Func<RuntimePhase, Fin<PhaseReceipt>> Inject,
    Func<Option<string>> HostDocument,
    Func<Action, IDisposable> DocumentChanged);

public sealed record UiSchedulerPort(
    Func<Action<PhaseReceipt>, IDisposable> Phases,
    Func<Action<DegradationLevel>, IDisposable> Degradation,
    Func<Action, IO<Unit>> Marshal);

public sealed record SupportContributorPort(
    string Package,
    Seq<SupportArtifact> Rows);

public sealed record HealthContributorPort(
    string Package,
    Seq<HealthContributorRow> Rows);
```

## [03]-[WIRE_LAW]

- Owner: `WireAdmission` the ONE generated-message descriptor and rule edge — one root set feeds the `TypeRegistry`, warmed evaluator, violation projection, `EventExtensions`, and every binary or JSON admission; `WireJson` the ONE ProtoJSON codec over that neutral admission; `FaultWire` the ONE fault wire — in-process `Observe`, the `Retriability` ⇄ `FaultRecovery` correspondence, `Pack` onto `FaultDetail`, the one `Error` → `StatusCode` producer table, `Raise` the one `RpcException` mint, and the client pair `Decode`/`Admit` onto `RemoteFault`; `FaultContext` the producer's call evidence; `WireViolation` `[Union]` and `WireBoundary` `[SmartEnum<string>]` the typed malformed-envelope evidence and the captured-codec-site vocabulary every branch reads; `HostWire` the byte and stamp correspondences the host families share — the 16-byte correlation, the `Hlc` stamp, the `Package` bridge, and the one-arm oneof assignment; `EnvelopeMap` the one `[Mapper]` projecting the kernel `ReceiptEnvelope` onto `Receipt.ReceiptHeaderWire`; `ProtoJsonConverterFactory` the one STJ crossing for a generated message embedded inside an in-process receipt; `SuiteContracts` the app-root STJ merge that survives for surfaces no corpus family carries.
- Entry: `WireAdmission.Warm()` compiles every reachable generated-message rule before readiness; `WireAdmission.EventExtensions` composes the kernel descriptor bridge over that evaluator; `WireAdmission.Validate<T>(T, Op)` preserves accumulated violations for request/response projection, while `WireAdmission.Admit<T>(T, WireBoundary, Op)` collapses the same verdict for a named generated-message edge; `WireJson.Write(IMessage, TextWriter|Stream)` and `WireJson.Read<T>(TextReader|Stream|JsonElement, Op)` are the ProtoJSON codec doors, with each read composing neutral admission after parse; `WireJson.Element(IMessage)` is the one door onto the kernel envelope's `JsonElement` payload; `FaultWire.Observe(Error)` projects the kernel `FaultObservation.Of` onto `Fault.FaultObservation`; `FaultWire.Raise(Error, FaultContext)` is every failing gRPC arm's one exit; `FaultWire.Decode(RpcException)` and `FaultWire.Admit(FaultDetail, string)` are the client read; `SuiteContracts.Wire(contexts)` creates one merge per app root for the discovery manifest and the in-process receipts no peer decodes.
- Packages: Rasm.Contracts (project — every emitted file reflection root and generated fault, host, compute, element, evidence, parity, and RPC message), Celly.Protovalidate (`Validator`, `ValidationCompileException`, `ValidationException`, generated `Buf.Validate.Violation`/`FieldPath`), Google.Protobuf (`TypeRegistry.FromFiles`, message descriptors and parsers, `JsonFormatter`, `JsonParser`, `Any.Pack`/`Is`/`Unpack<T>`, `ByteString`), Grpc.StatusProto (`Google.Rpc.Status.ToRpcException`, `RpcException.GetRpcStatus`, `Exception.ToRpcDebugInfo`), Google.Api.CommonProtos (`Google.Rpc.Status`, `RetryInfo`, `BadRequest.Types.FieldViolation`, `DebugInfo`), Grpc.Core.Api (`RpcException`, `StatusCode`), NodaTime.Serialization.Protobuf (`ToProtobufDuration`/`ToNodaDuration`), Rasm (kernel `FaultObservation`, `FaultBand`, `KernelFault`, `Retriability`, `ContentHash`), NodaTime.Serialization.SystemTextJson, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, BCL inbox
- Law: `WireAdmission.Files` is the ONE generated file-root roster. `TypeRegistry.FromFiles` folds its dependencies transitively for `Any`; `Validator` compiles and evaluates constraints from the same roots, so codec reachability and rule reachability cannot drift. `Warm` validates one parsed default for every admitted non-map descriptor while the host is still mutable, forcing Celly's lazy CEL compilation before readiness. ProtoJSON intake tolerates unknown fields (`WithIgnoreUnknownFields(true)`), since proto3 files a retired field to the unknown set, and bounds recursion at the parser's configured depth; a local validator, `JsonFormatter.Default`, and `JsonParser.Default` are deleted forms.
- Law: generated-message admission is two-railed without dual policy. Parse, unrostered-type, compile, and evaluator failures ride the outer `Fin`; authored rule refusals accumulate inside `Validation<Seq<BadRequest.FieldViolation>,T>`. `Read<T>` collapses that verdict only after parse, while the gRPC interceptor reads the same verdict directly for request `InvalidArgument` and response `Internal` projection. One field-path projector preserves scalar fields, repeated indices, and bool, signed, unsigned, or JSON-quoted string map keys; no mapper restates a rule or calls validation itself.
- Law: `FaultWire.Status` is the ONE producer `Error` → `StatusCode` table, a closed ladder: every `KernelFault` case through its generated total `Switch`, every other `Fault` by the posture its `Retriability` already carries (`Throttled` → `ResourceExhausted`, `Transient` → `Unavailable`, `Terminal` → `FailedPrecondition`), the rail's two reserved termination codes (`Errors.Cancelled`, `Errors.TimedOut`) onto their gRPC twins, and every foreign `Error` onto `Internal`. No handler spells a status; a `new RpcException(new Status(...))` anywhere on this branch is the deleted form. The client inverse (`StatusCode` → local transport fault) stays the dialing branch's own fold — `dotnet:Rasm.Compute/Runtime/wire#FAULT_PROJECTION` `StatusRail` — so the producer fold and the client fold never merge into one dictionary read both ways.
- Law: `FaultDetail.domain` names the PRODUCING FAMILY and `case` is that family's closed ordinal, never a transport status and never the absolute code. The family's one stable identity the wire can carry is its `FaultBand` row, so `domain` derives ONCE at static init as the band owner's lowercased key beside the band base (`rasm.apphost.4500`) and `case` is `fault.Code - band.Key`; `FaultBand` has no lowercase family column, and the owner key alone collapses every AppHost band onto one domain, which strips `(domain, case)` of its identity (thirty AppHost rows share `TelemetrySource.AppHost`). The pair stays OPAQUE at every peer — a remote `domain` never drives topology or failover, and the admission consults this process's own ledger only to refuse a same-estate ordinal past its band's span, which is version-skew evidence rather than taxonomy rehydration.
- Law: `FaultObservation.(domain, case)` carries the SAME family identity `FaultDetail.(domain, case)` carries, so one field pair has one meaning across both generated messages. `Observe` derives the pair together from the kernel observation's optional numeric code; a foreign `Error` crosses with both absent, and the generated paired-presence rule refuses either field alone. The kernel `FaultId.Case` token NEVER crosses, so a leaf rename stays one compilation.
- Law: `FaultRecovery` is the ONE recovery crossing and its arm set is the kernel `Retriability` union's — `Recovery(Retriability)` assigns exactly one arm through the generated total `Switch`, and `Recovery(FaultRecovery)` dispatches on `KindCase` so an unset message, a `None` kind, an absent `retry_delay`, and a negative delay each refuse as their own typed `WireViolation`. The throttled arm IS a `google.rpc.RetryInfo`, so `Raise` packs THAT instance a second time as a top-level `Status` detail and a foreign gRPC client reads standard back-off without decoding a rasm type; one message occupies both seats and a second construction is the deleted form that lets the two disagree. `RetryInfo` states a delay alone, which is why the estate keeps the oneof: `terminal` and `transient` are unspellable inside it.
- Law: `Raise` packs exactly one `FaultDetail` per estate fault except a kernel contract admission, which packs one standard `BadRequest` instead of duplicating its violations into two details. Request refusal stays `InvalidArgument`, invalid server response stays `Internal`, and both preserve the same typed field coordinates without refused values. Other kernel and domain faults retain their estate detail. A captured foreign exception crosses as `google.rpc.DebugInfo` through `ToRpcDebugInfo`, and an exception-free foreign error crosses with its message alone, so a fabricated case, domain, or recovery never reaches a peer. `Decode` reads `RpcException.GetRpcStatus()` under `Op.Catch`: absent answers `None`, a malformed trailer refuses typed on `WireBoundary.RemoteStatus`, exactly one recognized `FaultDetail` admits as opaque `RemoteFault`, zero recognized details answer `None` so the caller's transport classification decides, and several refuse on `WireBoundary.DetailMultiplicity` — never a silent fallback. `Message` comes from the enclosing `google.rpc.Status`, never re-rendered.
- Law: every cross-process family the corpus carries crosses as its generated semantic-package message — the owning page projects it once through Mapperly for structural twins or a direct producer for operational folds, then formats through `WireJson`, so a hand STJ record under a generated message's name is the deleted twin. The `SuiteContracts` STJ merge survives for the local attach manifest and in-process receipts no peer runtime decodes; the generated capability-discovery catalog is a distinct RPC reply. A generated message embedded in such a receipt crosses the merge through the one `ProtoJsonConverterFactory`, which writes `WireJson.Formatter` text and reads through `WireJson.Parser`, so the receipt fan and the dashboard read one ProtoJSON spelling of every fault observation.
- Law: kernel `ReceiptEnvelope` stays the in-process carrier and its `JsonElement` payload holds ProtoJSON text for every kind the corpus carries (`WireJson.Element`), while `EnvelopeMap.ToWire(envelope)` projects the HEADER alone — correlation, tenant, package, stamp, skew bound — so no payload and no kind key cross here. Producing packages pair that header with a oneof over their OWN closed receipt families, making the payload compiler-exhaustive at the composition site and the producer derivable from the arm; `Ui.EvidenceRowWire` is the corpus's worked example, and a package with no family yet emits a header its peer reads without a payload rather than an `Any` no registry resolves. `ReceiptKind.Admit` at `Observability/instruments#RECEIPT_PROJECTION` stays the one in-process decoder per kind the instrument write reads.
- Law: the `Hlc` stamp crosses as `Clock.Hlc{physical sfixed64 Unix ticks, logical uint64}` — the ONE wire stamp every host family, the fault detail, and the receipt envelope carry — so the packed `physical<<64 | logical` `UInt128` is an in-process content-key lane (`Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`) and never a wire layout; `HostWire.Stamp` is the one mint and the one admission of that message.
- Growth: a new generated file is one reflection root on `WireAdmission.Files`, which feeds registry, allowed-message index, and contract evaluator together; a new host family message is one owning producer and one `ReceiptKind` row; a new fault family lands its band row at the kernel and packs with no edit here; a new malformed-envelope shape is one `WireViolation` case at the refusing admission; a surface the corpus does not carry registers one `[JsonSerializable]` row on the merge and names its carve.
- Boundary: `WireJson` is a statement capsule at its two stream doors because `JsonFormatter.Format(IMessage, TextWriter)` and `JsonParser.Parse<T>(TextReader)` are the substrate's writer-shaped entries, and a string-returning `Format` is reached only where the consumer IS a string-shaped slot (the envelope's `JsonElement`, the raw STJ value the converter writes); a caller holding an `IBufferWriter<byte>` reaches the stream door through `CommunityToolkit.HighPerformance` `AsStream()` at its own tier, since this assembly admits no buffer-writer package row. `FaultWire.Raise` is the one `throw` on this page family and it sits INSIDE the gRPC verb edge the platform forces (`ControlServiceImpl` at `Wire/companion#CONTROL_SERVICE`): the typed receipt is sealed on the rail first and the exception is the transport's egress form, never control flow. `WireBoundary` keeps `QueryFieldNumber` and `InboundPayload` because `Rasm.Compute` `WireFault.Internal(WireBoundary, Error)` names them and one vocabulary serves both branches. ProtoJSON becomes a string in one place, the kernel envelope's `JsonElement` payload: `Element` parses the formatted text into a detached element and `Read<T>(JsonElement, Op)` re-parses its raw text, both stated as the string door rather than hidden. `HostFingerprintWire` and `BenchmarkClaimWire` ride `benchmark`; `DescriptorPinWire` rides `capability`; they bind at `Runtime/determinism`, `Observability/benchmarks`, and `Agent/capability`. NodaTime converters still bind at the merge through `ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)` for the receipts that stay STJ, and every temporal member of a generated message crosses as `Timestamp`/`Duration` through the `NodaTime.Serialization.Protobuf` static family at the owning mapper. NAMED LOSS of the former STJ fault-observation trio and its mapper: cause stamps crossed as structured rows (`code`, `exceptionType`, `hResult`) and now cross as the corpus's rendered strings, so a peer no longer reads a cause's numeric identity or CLR type name off the observation — witness `CommandTxn.Rejected(fault)` at `Agent/capability#COMMAND_ALGEBRA`, which now carries `FaultWire.Observe(fault)` and reaches the TS dashboard as the generated `FaultObservation` schema. NAMED LOSS of the former STJ schema export: the exporter and the `schema-derived TS` tool row delete — TS consumes the generated schema (`@rasm\/contracts/...`) and grades the capability catalog through `DescriptorPinWire` alone.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using Buf.Validate;
using Celly.Protovalidate;
using Google.Api;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using LanguageExt;
using NodaTime;
using NodaTime.Serialization.Protobuf;
using NodaTime.Serialization.SystemTextJson;
using Rasm.Contracts.Appearance;
using Rasm.Contracts.Compute;
using Rasm.Contracts.Declaration;
using Rasm.Contracts.Element;
using Rasm.Contracts.Event;
using Rasm.Contracts.Fabrication;
using Rasm.Contracts.Organization;
using Rasm.Contracts.Parity;
using Rasm.Contracts.Scene;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using Thinktecture.Text.Json.Serialization;
using Host = Rasm.Contracts.Receipt;
using Clock = Rasm.Contracts.Clock;
using Fault = Rasm.Contracts.Fault;
using static LanguageExt.Prelude;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireBoundary {
    public static readonly WireBoundary QueryFieldNumber = new("query-field-number");
    public static readonly WireBoundary ContractAdmission = new("contract-admission");
    public static readonly WireBoundary InboundPayload = new("inbound-payload");
    public static readonly WireBoundary OutboundPayload = new("outbound-payload");
    public static readonly WireBoundary RemoteStatus = new("remote-status");
    public static readonly WireBoundary RemoteDetail = new("remote-detail");
    public static readonly WireBoundary DetailAdmission = new("detail-admission");
    public static readonly WireBoundary DetailMultiplicity = new("detail-multiplicity");
}

// Typed malformed-envelope evidence: each case carries the VALUE that refused, never a rendered message.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireViolation {
    private WireViolation() { }

    public sealed record Domain(string Value) : WireViolation;
    // Same-estate ordinal past its band's span: the peer runs this ledger under another generation.
    public sealed record Case(string Domain, uint Value, int Span) : WireViolation;
    public sealed record Correlation(int Length) : WireViolation;
    public sealed record Tenant(string Value) : WireViolation;
    public sealed record MissingRecovery : WireViolation;
    public sealed record MissingRetryDelay : WireViolation;
    public sealed record MissingStamp : WireViolation;
    public sealed record RecoveryKind(Fault.FaultRecovery.KindOneofCase Value) : WireViolation;
    public sealed record RetryDelay(NodaTime.Duration Value) : WireViolation;
    public sealed record Stamp(long Physical) : WireViolation;
    public sealed record Kind(string Value) : WireViolation;
    public sealed record Contract(string Type, Seq<BadRequest.Types.FieldViolation> Violations) : WireViolation;
    public sealed record UnrosteredMessage(string Type) : WireViolation;
    public sealed record UnrosteredOp(string TypeUrl) : WireViolation;
    public sealed record ReceiptDisposition(bool Committed, bool HasConflict) : WireViolation;
    // More than one recognized detail on a trailer: the count is the refusing value.
    public sealed record Multiplicity(int Count) : WireViolation;
    // A decode step that RAISED rather than refused: the captured `Error` is the evidence, so a malformed
    // trailer and a throwing unpack ride the same typed envelope-refusal family instead of a bare `Error`.
    public sealed record Captured(Error Cause) : WireViolation;
}

// --- [MODELS] -------------------------------------------------------------------------------
// Producer call evidence the detail carries: correlation off the call, the stamp the sink's one HLC mint issued,
// the tenant `PeerAdmission` admitted, and the protovalidate refusals the verb accumulated.
public sealed record FaultContext(
    CorrelationId Correlation,
    Clock.Hlc Stamp,
    Option<TenantId> Tenant,
    Seq<BadRequest.Types.FieldViolation> Violations) {
    public static FaultContext Of(CorrelationId correlation, (Instant Physical, ulong Logical) stamp, TenantContext tenant,
        Seq<BadRequest.Types.FieldViolation> violations = default) =>
        new(correlation, HostWire.Stamp(stamp), tenant.Partitions ? Some(tenant.TenantId) : None, violations);
}

// Remote evidence stays OPAQUE: `Domain` and `Case` are the peer's pair, compared and logged, never rehydrated
// into a local taxonomy; `Recovery` is the one column a re-drive reads.
public sealed record RemoteFault(
    string Domain,
    uint Case,
    string Message,
    CorrelationId Correlation,
    Instant HlcPhysical,
    ulong HlcLogical,
    Option<TenantId> Tenant,
    Retriability Recovery,
    Seq<BadRequest.Types.FieldViolation> Violations);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The closed file-root set feeds codec lookup and contract evaluation together; neither can drift from the
// generated estate the other admits. Every generated-message codec composes this neutral verdict after parsing.
public static class WireAdmission {
    private static readonly FileDescriptor[] Files = [
        AppearanceReflection.Descriptor,
        Rasm.Contracts.Capability.DescriptorReflection.Descriptor,
        Rasm.Contracts.Capability.DiscoveryReflection.Descriptor,
        ComputeReflection.Descriptor,
        ControlReflection.Descriptor,
        DeclarationReflection.Descriptor,
        Rasm.Contracts.Element.EvidenceReflection.Descriptor,
        GraphReflection.Descriptor,
        SubstanceReflection.Descriptor,
        ValueReflection.Descriptor,
        EventReflection.Descriptor,
        FabricationReflection.Descriptor,
        Fault.FaultReflection.Descriptor,
        Clock.HlcReflection.Descriptor,
        Host.EnvelopeReflection.Descriptor,
        Rasm.Contracts.Ui.CommandsReflection.Descriptor,
        Rasm.Contracts.Ui.ControlsReflection.Descriptor,
        Rasm.Contracts.Ui.EvidenceReflection.Descriptor,
        Rasm.Contracts.Ui.LayoutReflection.Descriptor,
        Rasm.Contracts.Ui.SurfaceReflection.Descriptor,
        Rasm.Contracts.Render.ResidencyReflection.Descriptor,
        Rasm.Contracts.Bim.DiffReflection.Descriptor,
        OrganizationReflection.Descriptor,
        ParityReflection.Descriptor,
        SceneReflection.Descriptor,
    ];

    private static readonly FrozenDictionary<string, MessageDescriptor> Messages = Files
        .SelectMany(static file => file.MessageTypes.SelectMany(Descendants))
        .ToFrozenDictionary(static message => message.FullName, StringComparer.Ordinal);

    public static readonly TypeRegistry Registry = TypeRegistry.FromFiles(Files);

    private static readonly Validator Rules = Compile(new Validator(Files));

    public static readonly EventExtensionContract<Extensions> EventExtensions = new(
        Extensions.Parser,
        Extensions.Descriptor,
        Rules);

    // Composition calls this while the host is still mutable; reading `Rules` forces static initialization, so
    // every reachable CEL program has compiled when the method returns.
    public static Unit Warm() {
        _ = Rules.GetType();
        return unit;
    }

    public static Fin<Validation<Seq<BadRequest.Types.FieldViolation>, T>> Validate<T>(T message, Op key)
        where T : IMessage =>
        Messages.ContainsKey(message.Descriptor.FullName)
            ? key.Catch(() => Fin.Succ(Admission(message)))
            : Fin.Fail<Validation<Seq<BadRequest.Types.FieldViolation>, T>>(new HopFault.Malformed(
                WireBoundary.ContractAdmission,
                new WireViolation.UnrosteredMessage(message.Descriptor.FullName)));

    private static IEnumerable<MessageDescriptor> Descendants(MessageDescriptor message) =>
        message.IsMapEntry
            ? []
            : [message, .. message.NestedTypes.SelectMany(Descendants)];

    private static Validator Compile(Validator rules) {
        foreach (MessageDescriptor message in Messages.Values) {
            _ = rules.Validate(message.Parser.ParseFrom([]));
        }

        return rules;
    }

    private static Validation<Seq<BadRequest.Types.FieldViolation>, T> Admission<T>(T message)
        where T : IMessage {
        Seq<BadRequest.Types.FieldViolation> violations = toSeq(Rules.Validate(message).Select(Project));
        return violations.IsEmpty
            ? new Validation<Seq<BadRequest.Types.FieldViolation>, T>.Success(message)
            : new Validation<Seq<BadRequest.Types.FieldViolation>, T>.Fail(violations);
    }

    public static Fin<T> Admit<T>(T message, WireBoundary boundary, Op key) where T : IMessage =>
        Validate(message, key).Bind(admission => admission.Match(
            Fail: violations => Fin.Fail<T>(new HopFault.Malformed(
                boundary,
                new WireViolation.Contract(message.Descriptor.FullName, violations))),
            Succ: static admitted => Fin.Succ(admitted)));

    private static BadRequest.Types.FieldViolation Project(Buf.Validate.Violation violation) => new() {
        Field = Path(violation.Field),
        Description = violation.Message,
    };

    private static string Path(FieldPath? path) =>
        path is null ? string.Empty : string.Join('.', path.Elements.Select(Element));

    private static string Element(FieldPathElement element) => string.Concat(
        element.HasFieldName
            ? element.FieldName
            : element.HasFieldNumber
                ? element.FieldNumber.ToString(CultureInfo.InvariantCulture)
                : string.Empty,
        element.SubscriptCase switch {
            FieldPathElement.SubscriptOneofCase.None => string.Empty,
            FieldPathElement.SubscriptOneofCase.Index => $"[{element.Index.ToString(CultureInfo.InvariantCulture)}]",
            FieldPathElement.SubscriptOneofCase.BoolKey => element.BoolKey ? "[true]" : "[false]",
            FieldPathElement.SubscriptOneofCase.IntKey => $"[{element.IntKey.ToString(CultureInfo.InvariantCulture)}]",
            FieldPathElement.SubscriptOneofCase.UintKey => $"[{element.UintKey.ToString(CultureInfo.InvariantCulture)}]",
            FieldPathElement.SubscriptOneofCase.StringKey => $"[{JsonSerializer.Serialize(element.StringKey)}]",
            _ => throw new InvalidOperationException($"Unknown field-path subscript {element.SubscriptCase}."),
        });
}

// ProtoJSON owns formatting and parsing alone. Registry reach comes from WireAdmission, and every parsed message
// crosses the same neutral rule graph binary consumers compose.
public static class WireJson {
    // Defaults elide (proto3 JSON canon); the diff/patch lane that wanted them is binary `FieldMask` + `Merge`.
    public static readonly JsonFormatter Formatter = new(
        JsonFormatter.Settings.Default.WithTypeRegistry(WireAdmission.Registry));

    public static readonly JsonParser Parser = new(JsonParser.Settings.Default
        .WithIgnoreUnknownFields(true)
        .WithRecursionLimit(100)
        .WithTypeRegistry(WireAdmission.Registry));

    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

    // Exemption: the two stream doors are the platform-forced statement seam — the substrate's writer-shaped
    // entries take a `TextWriter`/`TextReader` the bracket owns, and no string is minted between them.
    public static Unit Write(IMessage message, TextWriter sink) {
        Formatter.Format(message, sink);
        return unit;
    }

    public static Unit Write(IMessage message, Stream sink) {
        using StreamWriter writer = new(sink, Utf8, leaveOpen: true);
        return Write(message, writer);
    }

    public static Fin<T> Read<T>(TextReader source, Op key) where T : IMessage<T>, new() =>
        key.Catch(() => Fin.Succ(Parser.Parse<T>(source)))
            .Bind(message => WireAdmission.Admit(message, WireBoundary.InboundPayload, key));

    public static Fin<T> Read<T>(Stream source, Op key) where T : IMessage<T>, new() {
        using StreamReader reader = new(source, Utf8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        return Read<T>(reader, key);
    }

    // The ONE string door, stated: the kernel `ReceiptEnvelope` carries a `JsonElement`, so a generated payload
    // crosses the sink as ProtoJSON text parsed into a DETACHED element; the inverse re-parses its raw text.
    public static JsonElement Element(IMessage message) {
        using JsonDocument parsed = JsonDocument.Parse(Formatter.Format(message));
        return parsed.RootElement.Clone();
    }

    public static Fin<T> Read<T>(JsonElement payload, Op key) where T : IMessage<T>, new() =>
        key.Catch(() => Fin.Succ(Parser.Parse<T>(payload.GetRawText())))
            .Bind(message => WireAdmission.Admit(message, WireBoundary.InboundPayload, key));
}

// Byte and stamp correspondences every host family shares, seated once so no mapper spells a width, an
// endianness, or a tick unit of its own.
public static class HostWire {
    // RFC 4122 byte order: the one 16-byte form a `Guid` publishes that every peer reads identically.
    public static ByteString Correlation(CorrelationId correlation) =>
        ByteString.CopyFrom(((Guid)correlation).ToByteArray(bigEndian: true));

    public static Fin<CorrelationId> Correlation(ByteString wire, Op key) =>
        wire.Length == 16
            ? Fin.Succ(CorrelationId.Create(new Guid(wire.Span, bigEndian: true)))
            : Fin.Fail<CorrelationId>(key.InvalidInput(nameof(CorrelationId)));

    // ONE wire stamp: the NodaTime Unix tick count is the physical half the kernel HLC mint already carries.
    public static Clock.Hlc Stamp((Instant Physical, ulong Logical) hlc) =>
        new() { Physical = hlc.Physical.ToUnixTimeTicks(), Logical = hlc.Logical };

    public static Fin<(Instant Physical, ulong Logical)> Stamp(Clock.Hlc wire, Op key) =>
        wire.Physical >= 0L
            ? Fin.Succ((Instant.FromUnixTimeTicks(wire.Physical), wire.Logical))
            : Fin.Fail<(Instant, ulong)>(key.InvalidInput(nameof(Clock.Hlc)));

    // The kernel package roster and the wire enum are one correspondence: the generated `Map` is total, so a
    // package minted at the kernel breaks this projection until its enum value lands at the corpus.
    public static Host.Package Package(TelemetrySource source) => source.Map(
        kernel: Host.Package.Kernel, element: Host.Package.Element, appHost: Host.Package.AppHost,
        materials: Host.Package.Materials, bim: Host.Package.Bim, fabrication: Host.Package.Fabrication,
        persistence: Host.Package.Persistence, compute: Host.Package.Compute, generation: Host.Package.Generation,
        appUi: Host.Package.AppUi, rhino: Host.Package.Rhino, grasshopper: Host.Package.Grasshopper);

    public static Fin<TelemetrySource> Package(Host.Package wire, Op key) =>
        key.Row<Host.Package, string, TelemetrySource>(wire, Package);

    // The one one-arm oneof assignment: a multi-arm object initializer clears the arm it just set, so every
    // union lowering assigns exactly ONE arm inside the domain union's own total `Switch`.
    public static T Arm<T>(this T message, Action<T> arm) where T : class, IMessage<T> {
        arm(message);
        return message;
    }
}

public static partial class FaultWire {
    // Domain derives ONCE per band: the owner's lowercased key beside the band base, so the thirty AppHost
    // rows stay thirty families on the wire and the pair `(domain, case)` keeps its identity.
    static readonly Lazy<FrozenDictionary<int, (string Domain, FaultBand Band)>> Seats = new(static () =>
        toSeq(FaultBand.Items).Filter(static band => band.Kind == BandKind.Fault)
            .ToFrozenDictionary(static band => band.Key, static band =>
                (string.Create(CultureInfo.InvariantCulture, $"{band.Owner.Key.ToLowerInvariant()}.{band.Key}"), band)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    static readonly Lazy<FrozenDictionary<string, FaultBand>> Ledger = new(static () =>
        Seats.Value.Values.ToFrozenDictionary(static seat => seat.Domain, static seat => seat.Band, StringComparer.Ordinal),
        LazyThreadSafetyMode.ExecutionAndPublication);

    [GeneratedRegex("^[a-z][a-z0-9.-]*$")]
    private static partial Regex DomainGrammar();

    static Option<(string Domain, uint Case)> Seat(int code) =>
        FaultBand.OwnerOf(BandKind.Fault, code).Map(band => (Seats.Value[band.Key].Domain, (uint)(code - band.Key)));

    // --- [OBSERVE]
    public static Fault.FaultObservation Observe(Error error) => Observe(FaultObservation.Of(error));

    // The code derives the generated family pair in one arm; the local case token never crosses.
    public static Fault.FaultObservation Observe(FaultObservation observation) =>
        new Fault.FaultObservation {
            Recovery = Recovery(observation.Recovery),
            Causes = { observation.Causes.Map(Rendered) },
            Truncated = observation.Truncated,
        }.Arm(wire => observation.Code.Bind(Seat).Iter(seat => {
            wire.Domain = seat.Domain;
            wire.Case = seat.Case;
        }));

    static string Rendered(FaultCauseStamp stamp) =>
        stamp.Identity.Map(static id => id.Code.ToString(CultureInfo.InvariantCulture))
            .Append(stamp.ExceptionType.Map(static type => type.FullName ?? type.Name))
            .Append(stamp.HResult.Map(static code => code.ToString(CultureInfo.InvariantCulture)))
            .Match(Some: static text => text, None: static () => string.Empty);

    // --- [RECOVERY]
    // Throttled mints the standard `RetryInfo` that the top-level `Status` detail packs, so this branch holds
    // ONE construction site for the window and its two seats cannot drift apart.
    public static Fault.FaultRecovery Recovery(Retriability recovery) => recovery.Switch(
        terminalCase: static _ => new Fault.FaultRecovery { Terminal = new Empty() },
        transientCase: static _ => new Fault.FaultRecovery { Transient = new Empty() },
        throttledCase: static row => new Fault.FaultRecovery {
            RetryAfter = new RetryInfo { RetryDelay = row.RetryAfter.ToProtobufDuration() },
        });

    // `retry_delay` is a message field the generated type spells nullable, and the authored CEL rule that forces
    // it present evaluates at `WireAdmission`, never here — so this admission collapses that optionality itself
    // and the interior only ever sees a non-negative window.
    public static Fin<Retriability> Recovery(Fault.FaultRecovery? wire) => wire switch {
        null => Fin.Fail<Retriability>(Violation(new WireViolation.MissingRecovery())),
        { KindCase: Fault.FaultRecovery.KindOneofCase.Terminal } => Fin.Succ(Retriability.Terminal),
        { KindCase: Fault.FaultRecovery.KindOneofCase.Transient } => Fin.Succ(Retriability.Transient),
        { KindCase: Fault.FaultRecovery.KindOneofCase.RetryAfter, RetryAfter.RetryDelay: { } stated } =>
            stated.ToNodaDuration() switch {
                var delay when delay >= NodaTime.Duration.Zero => Fin.Succ(Retriability.Throttled(delay)),
                var delay => Fin.Fail<Retriability>(Violation(new WireViolation.RetryDelay(delay))),
            },
        { KindCase: Fault.FaultRecovery.KindOneofCase.RetryAfter } =>
            Fin.Fail<Retriability>(Violation(new WireViolation.MissingRetryDelay())),
        { KindCase: var kind } => Fin.Fail<Retriability>(Violation(new WireViolation.RecoveryKind(kind))),
    };

    // --- [PACK]
    // Total over every generated family: a `Fault` minted through `FaultId` sits inside a band by construction,
    // so the `None` arm is the one shape `FaultId` forecloses, answered on the rail rather than thrown.
    public static Fin<Fault.FaultDetail> Pack(Domain.Fault fault, FaultContext context) =>
        Seat(fault.Code).Map(seat => new Fault.FaultDetail {
            Domain = seat.Domain,
            Case = seat.Case,
            Correlation = HostWire.Correlation(context.Correlation),
            Stamp = context.Stamp,
            Tenant = context.Tenant.Map(static tenant => tenant.Text).IfNone(string.Empty),
            Recovery = Recovery(fault.Retriability),
            Violations = { context.Violations },
        }).ToFin(Op.Of().InvalidResult($"<unseated-fault:{fault.Code}>"));

    // --- [STATUS]
    // The ONE producer table. Kernel cases dispatch through their generated total `Switch`, every other fault
    // by its own posture, the rail's two reserved termination codes by identity, and a foreign error stays
    // `Internal` — no arm reads rendered text.
    public static StatusCode Status(Error error) => error switch {
        KernelFault kernel => kernel.Switch(
            missingOperation: static _ => StatusCode.InvalidArgument,
            missingContext: static _ => StatusCode.FailedPrecondition,
            invalidContext: static _ => StatusCode.FailedPrecondition,
            invalidInput: static _ => StatusCode.InvalidArgument,
            unsupported: static _ => StatusCode.Unimplemented,
            invalidResult: static _ => StatusCode.Internal,
            cancelled: static _ => StatusCode.Cancelled,
            missingGeometry: static _ => StatusCode.InvalidArgument,
            invalidGeometry: static _ => StatusCode.InvalidArgument,
            invalidValue: static _ => StatusCode.InvalidArgument,
            outOfRange: static _ => StatusCode.OutOfRange,
            invalidUnitSystem: static _ => StatusCode.FailedPrecondition),
        Domain.Fault fault => fault.Retriability.Switch(
            terminalCase: static _ => StatusCode.FailedPrecondition,
            transientCase: static _ => StatusCode.Unavailable,
            throttledCase: static _ => StatusCode.ResourceExhausted),
        _ when error.Is(Errors.Cancelled) => StatusCode.Cancelled,
        _ when error.Is(Errors.TimedOut) => StatusCode.DeadlineExceeded,
        _ => StatusCode.Internal,
    };

    // --- [RAISE]
    // `Raise` is the ONE `RpcException` mint. Details: one `FaultDetail` per estate fault, plus this fault's own
    // throttled-arm `RetryInfo` seated a second time for generic middleware, plus standard `BadRequest` evidence
    // wherever a kernel admission fault carries contract refusals.
    public static RpcException Raise(Error error, FaultContext context) =>
        new Google.Rpc.Status {
            Code = (int)Status(error),
            Message = error.Message,
            Details = { Details(error, context) },
        }.ToRpcException();

    static Seq<Any> Details(Error error, FaultContext context) =>
        ((error, context.Violations) switch {
            (KernelFault, { IsEmpty: false }) => Seq<Any>(),
            // Advice READS the detail's own throttled arm — a generated oneof getter answers null off every other
            // arm, so this standard detail is that one instance packed twice and never a second mint.
            (Domain.Fault fault, _) => Pack(fault, context).Match(
                Succ: static detail => Seq(Any.Pack(detail))
                    + Optional(detail.Recovery.RetryAfter).Map(static advice => Any.Pack(advice)).ToSeq(),
                Fail: static _ => Seq<Any>()),
            _ => error.Exception.Map(static raised => Any.Pack(raised.ToRpcDebugInfo())).ToSeq(),
        }) + (error is KernelFault && !context.Violations.IsEmpty
            ? Seq(Any.Pack(new BadRequest { FieldViolations = { context.Violations } }))
            : Seq<Any>());

    // --- [DECODE]
    // Absent → None; malformed trailer → typed on `RemoteStatus`; exactly one recognized detail admits; several
    // refuse on `DetailMultiplicity`; zero recognized answer None so transport classification decides.
    public static Fin<Option<RemoteFault>> Decode(RpcException raised) =>
        Op.Of().Catch(() => Fin.Succ(Optional(raised.GetRpcStatus())))
            .MapFail(captured => (Error)new HopFault.Malformed(
                WireBoundary.RemoteStatus, new WireViolation.Captured(captured)))
            .Bind(status => status.Match(
                None: static () => Fin.Succ(Option<RemoteFault>.None),
                Some: held => Recognized(held)));

    // `Admit`'s fail side already accumulates `HopFault.Malformed` per refused column through `Violation`, so
    // the admission leg passes its rail through rather than wrapping typed refusals in a second `Malformed`.
    static Fin<Option<RemoteFault>> Recognized(Google.Rpc.Status status) =>
        toSeq(status.Details).Filter(static any => any.Is(Fault.FaultDetail.Descriptor)) switch {
            { IsEmpty: true } => Fin.Succ(Option<RemoteFault>.None),
            { Count: 1 } one => Op.Of().Catch(() => Fin.Succ(one.Head.Unpack<Fault.FaultDetail>()))
                .MapFail(captured => (Error)new HopFault.Malformed(
                    WireBoundary.RemoteDetail, new WireViolation.Captured(captured)))
                .Bind(detail => Admit(detail, status.Message))
                .Map(Some),
            var many => Fin.Fail<Option<RemoteFault>>(new HopFault.Malformed(
                WireBoundary.DetailMultiplicity, new WireViolation.Multiplicity(many.Count))),
        };

    // Every D6 field admits independently and the refusals ACCUMULATE, so one malformed detail names every
    // column it failed; `Case` refuses only a same-estate ordinal past its own band's span.
    public static Fin<RemoteFault> Admit(Fault.FaultDetail detail, string message) =>
        (Domain(detail), HostWire.Correlation(detail.Correlation, Op.Of()).ToValidation(), Stamp(detail.Stamp),
         Recovery(detail.Recovery).ToValidation(), Tenant(detail.Tenant))
            .Apply((domain, correlation, stamp, recovery, tenant) => new RemoteFault(
                domain, detail.Case, message, correlation, stamp.Physical, stamp.Logical, tenant, recovery,
                toSeq(detail.Violations)))
            .As()
            .ToFin();

    static Validation<Error, string> Domain(Fault.FaultDetail detail) =>
        !DomainGrammar().IsMatch(detail.Domain) ? Violation(new WireViolation.Domain(detail.Domain))
        : Ledger.Value.TryGetValue(detail.Domain, out FaultBand? band) && detail.Case >= (uint)band.Span
            ? Violation(new WireViolation.Case(detail.Domain, detail.Case, band.Span))
        : Validation<Error, string>.Success(detail.Domain);

    static Validation<Error, (Instant Physical, ulong Logical)> Stamp(Clock.Hlc? wire) => wire switch {
        null => Violation(new WireViolation.MissingStamp()),
        { Physical: < 0L } => Violation(new WireViolation.Stamp(wire.Physical)),
        _ => Validation<Error, (Instant, ulong)>.Success((Instant.FromUnixTimeTicks(wire.Physical), wire.Logical)),
    };

    static Validation<Error, Option<TenantId>> Tenant(string wire) =>
        wire.Length == 0 ? Validation<Error, Option<TenantId>>.Success(None)
        : TenantId.TryOf(wire).Match(
            Some: static tenant => Validation<Error, Option<TenantId>>.Success(Some(tenant)),
            None: () => Violation(new WireViolation.Tenant(wire)));

    static Error Violation(WireViolation violation) => new HopFault.Malformed(WireBoundary.DetailAdmission, violation);
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
// The header crossing. `ReceiptHeaderWire` carries what EVERY receipt has and nothing about what any one
// carries, so no payload slot and no kind key reach it: a producing package pairs this header with a oneof
// over its own closed families, which is what makes the payload exhaustive at the composition site and the
// producer derivable from the arm rather than from a string key. `Kind` and the kernel `JsonElement` payload
// stay in-process for the receipt fan and never cross.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
[UseStaticMapper(typeof(NodaExtensions))]
[UseStaticMapper(typeof(HostWire))]
internal static partial class EnvelopeMap {
    [MapperIgnoreSource(nameof(ReceiptEnvelope.Kind))]
    [MapperIgnoreSource(nameof(ReceiptEnvelope.Payload))]
    [MapperIgnoreSource(nameof(ReceiptEnvelope.Physical))]
    [MapperIgnoreSource(nameof(ReceiptEnvelope.Logical))]
    [MapProperty(nameof(ReceiptEnvelope.Correlation), nameof(Host.ReceiptHeaderWire.Correlation))]
    [MapProperty(nameof(ReceiptEnvelope.Package), nameof(Host.ReceiptHeaderWire.Package))]
    [MapProperty(nameof(ReceiptEnvelope.SkewBound), nameof(Host.ReceiptHeaderWire.SkewBound))]
    [MapPropertyFromSource(nameof(Host.ReceiptHeaderWire.Stamp), Use = nameof(Stamped))]
    public static partial Host.ReceiptHeaderWire ToWire(ReceiptEnvelope envelope);

    [MapProperty(nameof(TenantContext.TenantId), nameof(Host.TenantContextWire.Tenant), Use = nameof(Key))]
    public static partial Host.TenantContextWire ToWire(TenantContext tenant);

    [NamedMapping(nameof(Stamped))]
    private static Clock.Hlc Stamped(ReceiptEnvelope envelope) => HostWire.Stamp((envelope.Physical, envelope.Logical));

    [NamedMapping(nameof(Key))]
    private static ByteString Key(TenantId tenant) => ContentHash.Wire(tenant.Value);
}

// ONE STJ crossing for a generated message EMBEDDED in an in-process receipt: the factory writes the
// `WireJson` formatter's text and reads through its parser, so the receipt fan and a dashboard read one
// ProtoJSON spelling of every fault observation, never STJ's reflection over a generated class.
public sealed class ProtoJsonConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => typeof(IMessage).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(ProtoJsonConverter<>).MakeGenericType(typeToConvert))!;

    sealed class ProtoJsonConverter<T> : JsonConverter<T> where T : IMessage<T>, new() {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            WireJson.Parser.Parse<T>(JsonDocument.ParseValue(ref reader).RootElement.GetRawText());

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteRawValue(WireJson.Formatter.Format(value));
    }
}

// In-process receipts NO PEER decodes and the discovery manifest: the surfaces the corpus carries no family for.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PhaseReceipt))]
[JsonSerializable(typeof(BootMarker))]
[JsonSerializable(typeof(FaultSource))]
[JsonSerializable(typeof(DrainReceipt))]
[JsonSerializable(typeof(ReplayOutcome))]
[JsonSerializable(typeof(StepStateRow))]
[JsonSerializable(typeof(HealthSnapshot))]
[JsonSerializable(typeof(DegradationState))]
[JsonSerializable(typeof(SupportManifest))]
[JsonSerializable(typeof(SupportReceipt.Exported))]
[JsonSerializable(typeof(DumpTriage))]
[JsonSerializable(typeof(DiscoveryManifest))]
[JsonSerializable(typeof(ReceiptEnvelope))]
[JsonSerializable(typeof(TenantContext))]
[JsonSerializable(typeof(RosterReceipt))]
[JsonSerializable(typeof(FleetRollReceipt))]
[JsonSerializable(typeof(RollAnnotationWire))]
[JsonSerializable(typeof(CommandArguments))]
[JsonSerializable(typeof(CommandReceipt))]
[JsonSerializable(typeof(DescriptorReceipt))]
[JsonSerializable(typeof(SandboxReceipt))]
[JsonSerializable(typeof(EvictionCauseWire))]
[JsonSerializable(typeof(UpdateReceipt))]
[JsonSerializable(typeof(SupplyChainReceipt))]
[JsonSerializable(typeof(AlertReceipt))]
[JsonSerializable(typeof(BenchmarkReceipt))]
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(SecretReceipt))]
[JsonSerializable(typeof(ModalityReceipt))]
[JsonSerializable(typeof(ToolAudit))]
[JsonSerializable(typeof(VerbReceipt))]
[JsonSerializable(typeof(CascadeReceipt))]
[JsonSerializable(typeof(BindReceipt))]
[JsonSerializable(typeof(Delivery))]
[JsonSerializable(typeof(HopReceipt))]
[JsonSerializable(typeof(DeliveryReceipt))]
[JsonSerializable(typeof(DropReceipt))]
[JsonSerializable(typeof(OutboxSweep))]
[JsonSerializable(typeof(MembershipReceipt))]
// ONE fenced receipt covers election and lock alike: `Wire/coordination#ROLE_ELECTION` types both keys as the
// `LeaseKey` value object and recovers the discrimination through `LeaseKey.Namespace`.
[JsonSerializable(typeof(FenceReceipt<LeaseKey>))]
public partial class AppHostWireContext : JsonSerializerContext;

public static class SuiteContracts
{
    // Composition-bound producer surface for the surfaces above: `Wire` seats it as the ONE merged options
    // identity. A `JsonTypeInfo` off `AppHostWireContext.Default` carries the context's own options (no
    // factory, no modifier) and a context INSTANCE over the merge rebinds the resolver and drops the modifier,
    // so `.Default` survives only as a merge argument and for type-init metadata roster reads.
    public static JsonSerializerOptions Host {
        get => field ?? throw new InvalidOperationException("SuiteContracts.Wire seats Host at the app-root mint.");
        private set;
    }

    // Absence OMITS. `WhenWritingNull` reaches reference and `Nullable<T>` slots alone and `Option<T>` is a
    // struct the condition never sees, so omission is a CONTRACT decision riding the resolver; `IOptional` is
    // the non-generic presence read every closed `Option<A>` carries. Paired law: an `Option<T>` constructor
    // parameter with no default reads WIRE-REQUIRED under `RespectRequiredConstructorParameters`.
    static void OmitAbsent(JsonTypeInfo contract) {
        foreach (JsonPropertyInfo property in contract.Properties) {
            if (property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(Option<>)) {
                property.ShouldSerialize = static (_, value) => value is IOptional { IsSome: true };
            }
        }
    }

    public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts) {
        JsonSerializerOptions wire = new JsonSerializerOptions(JsonSerializerOptions.Strict)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine([.. contexts]).WithAddedModifier(OmitAbsent),
            // Three factories over three DISJOINT type spaces — generated conversion metadata, the open-generic
            // kernel carrier rows (`Rasm/Domain/rails#CARRIER_CODEC`), and `IMessage` — so order decides nothing.
            Converters = {
                new ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true),
                new LanguageExtJsonConverterFactory(),
                new ProtoJsonConverterFactory(),
            },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        wire.MakeReadOnly();   // freeze at the mint; a post-mint converter or resolver edit throws instead of forking the suite
        Host = wire;   // the one producer surface; every write resolves through the merged chain
        return wire;
    }
}
```

Codec residence is fixed per wire surface; producer and consumer cells name endpoints only, not alternate codecs.

| [INDEX] | [WIRE_SURFACE]             | [CODEC]                                      | [PRODUCER]            | [CONSUMER]            |
| :-----: | :------------------------- | :------------------------------------------- | :-------------------- | :-------------------- |
|  [01]   | semantic contract families | ProtoJSON via `WireJson`                     | owning producer       | generated peer schema |
|  [02]   | discovery manifest         | STJ Strict atomic JSON                       | app-root boot         | attaching peer        |
|  [03]   | service verbs              | protobuf over gRPC                           | app roots             | connect-es clients    |
|  [04]   | wire faults                | `google.rpc.Status` details via `FaultWire`  | `FaultWire.Raise`     | `FaultWire.Decode`    |
|  [05]   | snapshot blobs             | MessagePack                                  | snapshot rows         | @msgpack/msgpack      |
|  [06]   | telemetry signals          | OTLP                                         | exporters             | OTLP collector        |
|  [07]   | in-process receipts        | STJ merge, `IMessage` members as ProtoJSON   | `SuiteContracts.Host` | receipt fan           |
|  [08]   | semantic-time fields       | `Timestamp`/`Duration`; NodaTime STJ on [07] | owning mapper         | well-known types      |

Every family group riding row [01] has one generated-message producer. Mapperly owns structural correspondences; direct constructors own operational folds whose source is not a shape twin. `LiveWireMap` stays outside this registry because it projects only host-local observations onto the STJ merge.

| [INDEX] | [FAMILY_GROUP]                  | [PRODUCER]            | [OWNER]                                  |
| :-----: | :------------------------------ | :-------------------- | :--------------------------------------- |
|  [01]   | receipt header, tenant          | `EnvelopeMap`         | this page                                |
|  [02]   | hop and delivery evidence       | native receipt family | `Wire/outbound#RECEIPT_FAMILY`           |
|  [03]   | bus loss account                | native `DropReceipt`  | `Wire/topics#LOSS_RECEIPT`               |
|  [04]   | binding status, coercion, write | `LiveWireContract`    | `Wire/livewire#TS_PROJECTION`            |
|  [05]   | command availability            | `AvailabilityMap`     | `Observability/health#DEGRADATION_RAIL`  |
|  [06]   | credential material             | `CredentialPublicMap` | `Runtime/secrets#CREDENTIAL_PEM`         |
|  [07]   | flag verdict                    | `FeatureMap`          | `Runtime/features#VERDICT_PROJECTION`    |
|  [08]   | host fingerprint                | `HostFingerprintMap`  | `Runtime/determinism#DETERMINISM_KERNEL` |
|  [09]   | descriptor pin                  | `DescriptorPin.Of`    | `Agent/capability#SDK_CODEGEN`           |
|  [10]   | control replies                 | `ControlReplyMap`     | `Wire/companion#CONTROL_SERVICE`         |

## [04]-[TS_PROJECTION]

- Owner: `ReceiptHeaderWire`, `TenantContextWire`, and `Package` — the generated `Receipt` header family the TS dashboard decodes from the generated schema; `EnvelopeMap` above is its one producer.
- Entry: every receipt crosses as its producing package's own composed message — this header beside a oneof over that package's closed families — so the peer switches on a generated arm rather than resolving an `Any` through a registry; a hand-mirrored TS interface of any header member is the deleted form.
- Packages: Rasm.Contracts (project)
- Growth: a new header column lands at `receipt/envelope.proto` and regenerates through the gate; this page gains no member.
- Boundary: `physical` and `logical` cross ONLY as `Clock.Hlc` — the ISO text half and the `physical_ticks<<64 | logical` packed `UInt128` are gone from the wire; the packed form survives as the in-process content-key lane `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Compose` seals, so a peer that hashes the frame reads the tick count off the stamp and never an ISO string; `tenant` crosses as the 16 big-endian bytes `ContentHash.Wire` publishes beside the slug, never a decimal string; `package` crosses as the `Receipt.Package` enum the kernel `TelemetrySource` roster maps onto through `HostWire.Package`, so a package minted at the kernel with no enum value breaks the projection at build; `correlation` crosses as the 16-byte RFC 4122 form `HostWire.Correlation` publishes. TS tooling collapses to the generated schema: `connect-es` over the service descriptors, `@rasm\/contracts/rasm/contracts/receipt/envelope_pb` over every host family, `@msgpack/msgpack` over snapshot blobs, OTLP over telemetry — the `schema-derived TS` tool row and its former STJ schema exporter are deleted.

## [05]-[RESEARCH]

(none)
