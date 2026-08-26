# [APPHOST_RUNTIME_PORTS]

Rasm.AppHost exposes six typed port records as its only INWARD cross-package boundary; siblings adapt to them and no sibling assembly enters the AppHost graph. Outward vocabularies cross the same boundary as settled row payloads, never reversing an interior dependency. Owned axes are the port-record family under its cardinality invariant, the boot tenancy mint, and the host wire edge — the one ProtoJSON registry, the one fault wire, and the strict local serializer merge. Drain bands, deadline rows, phase vocabulary, classification, and degradation arrive settled as port payloads.

Settled composition: `CorrelationId` arrives from the kernel frame capsule `Rasm/Domain/frame#SOURCE`, `TenantId`/`TenantContext` and `TenantMirror` from `#TENANCY`, and `TelemetryContributorPort` from `Rasm/Domain/telemetry#CONTRIBUTE`.

This page seats the kernel contribution record inside the cardinality invariant and mints the boot tenancy value from its tenant-feed configuration; `Observability/telemetry#SIGNAL_GOVERNANCE` registers the OTel `Baggage.Current` store as the composition `TenantMirror` row, so a kernel caller spells `Stamp()` bare and threads no mirror per call site.

## [01]-[INDEX]

- [02]-[PORT_RECORDS]: Six inward port records, five declared here and one at the kernel capsule.
- [03]-[WIRE_LAW]: `WireJson` the one ProtoJSON edge, `FaultWire` the one fault wire, and the strict local serializer merge.

## [02]-[PORT_RECORDS]

- Owner: `TelemetryContributorPort`, `DrainParticipantPort`, `HostAttachPort`, `UiSchedulerPort`, `SupportContributorPort`, `HealthContributorPort` — the six sealed records of delegates and policy values, five declared here and one at the kernel signal capsule so every stratum mints one without an upward reference; zero interfaces, zero inheritance contracts, zero provider-branded vocabulary. `TenantContext` and `TenantId` are kernel tenancy primitives minted here at boot and consumed by every sibling as settled vocabulary, never ports.
- Cases: the capability axis is `PortCardinality` — four DRIVEN ports the host calls outward into the package interior (`TelemetryContributorPort`, `DrainParticipantPort`, `SupportContributorPort`, `HealthContributorPort`) and two DRIVING host-affine ports the host implements at the boundary (`HostAttachPort` injects phase transitions and surfaces the document, `UiSchedulerPort` marshals onto the host UI loop and carries no sample feed — a `ProfileSample` consumer subscribes through a `HookTap<AppHostPoint, AppHostFact, TelemetrySource>` scoped to `AppHostPoint.ProfileSample` at `Observability/hooks#HOOK_ROSTER`, so the published sample reaches its reader on the one hook dispatcher rather than a second port column).
- Entry: every registration enters as a `TryAddEnumerable` ordered `ServiceDescriptor` row admitted through `PortCardinality.Of(port)` — the `Runtime/modules#SCAN_AND_DECORATE` slot pass is that admission's one execution site — `DescriptorSlot.Contributor.Admits` is the column that carries it and the `Seated` leg folds that column over the module's contributor rows, so each contributor descriptor's service-type name crosses `Of` before it joins the ordered set — and every subscribing port returns disposable detachers composed LIFO, so a port is registered, never resolved by lookup; `TenantContext.Root` is the single-tenant ambient default (`TenantId` zero, slug `root`) and a multi-tenant host mints one row per admitted tenant at boot from its tenant-feed configuration.
- Packages: Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new cross-package adapter lands as one registration row on an existing port, zero new surface; a new tenant is one ambient `TenantContext` value minted at boot, never a second tenancy owner.
- Boundary: `PortCardinality` holds every admitted port name; a new aggregate store, compute, companion, or outbound-client adapter lands as a row on the owning port. Content carriers and constructor-injected spine owners remain values rather than delegate-bearing inward ports. Contributor ports carry settled row vocabularies, and `TenantContext` remains the one boot-minted tenancy primitive every store, cache, and bounded metric dimension reads.

```csharp

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PortCardinality {
    public static readonly PortCardinality Driven = new("driven", Names(
        nameof(TelemetryContributorPort), nameof(DrainParticipantPort),
        nameof(SupportContributorPort), nameof(HealthContributorPort)));
    public static readonly PortCardinality Driving = new("driving", Names(
        nameof(HostAttachPort), nameof(UiSchedulerPort)));

    public FrozenSet<string> Ports { get; }

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
    Func<RuntimePhase, Fin<PhaseCommit>> Inject,
    Func<Option<string>> HostDocument,
    Func<Action, IDisposable> DocumentChanged);

public sealed record UiSchedulerPort(
    Func<Action<PhaseCommit>, IDisposable> Phases,
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

- Owner: `WireAdmission` owns generated-message discovery and validation; `WireJson` owns ProtoJSON; `FaultWire` owns fault projection; `HostWire` owns correlation and HLC correspondences; `ProtoJsonConverterFactory` owns generated messages embedded in local domain facts; `SuiteContracts` owns the strict app-root serializer merge.
- Entry: `WireAdmission.Warm()` compiles every reachable generated-message rule before readiness; `WireJson` carries every protobuf JSON door; `FaultWire.Raise` and `Decode` own the transport fault edge; `SuiteContracts.Wire(contexts)` creates one strict merge per app root.
- Law: `WireAdmission.Files` is the ONE generated file-root roster. `TypeRegistry.FromFiles` folds its dependencies transitively for `Any`; `Validator` compiles and evaluates constraints from the same roots, so codec reachability and rule reachability cannot drift. `Warm` validates one parsed default for every admitted non-map descriptor while the host is still mutable, forcing Celly's lazy CEL compilation before readiness. ProtoJSON intake tolerates unknown fields (`WithIgnoreUnknownFields(true)`), since proto3 files a retired field to the unknown set, and bounds recursion at the parser's configured depth; a local validator, `JsonFormatter.Default`, and `JsonParser.Default` are deleted forms.
- Law: generated-message admission is two-tiered without dual policy. Parse, unrostered-type, compile, and evaluator failures ride the outer `Fin`; authored rule refusals accumulate inside `Validation<Seq<BadRequest.FieldViolation>,T>`. `Read<T>` collapses that verdict only after parse, while the gRPC interceptor reads the same verdict directly for request `InvalidArgument` and response `Internal` projection. One field-path projector preserves scalar fields, repeated indices, and bool, signed, unsigned, or JSON-quoted string map keys; no mapper restates a rule or calls validation itself.
- Law: `FaultWire.Status` is the ONE producer `Error` → `StatusCode` table, a closed ladder: every `KernelFault` case through its generated total `Switch`, every other `Fault` by the posture its `Retriability` already carries (`Throttled` → `ResourceExhausted`, `Transient` → `Unavailable`, `Terminal` → `FailedPrecondition`), the result type's two reserved termination codes (`Errors.Cancelled`, `Errors.TimedOut`) onto their gRPC twins, and every foreign `Error` onto `Internal`. No handler spells a status; a `new RpcException(new Status(...))` anywhere on this branch is the deleted form. The client inverse (`StatusCode` → local transport fault) stays the dialing branch's own fold — `dotnet:Rasm.Compute/Runtime/wire#FAULT_PROJECTION` `StatusFold` — so the producer fold and the client fold never merge into one dictionary read both ways.
- Law: `FaultDetail.domain` names the PRODUCING FAMILY and `case` is that family's closed ordinal, never a transport status and never the absolute code. The family's one stable identity the wire can carry is its `FaultBand` row, so `domain` derives ONCE at static init as the band owner's lowercased key beside the band base (`rasm.apphost.4500`) and `case` is `fault.Code - band.Key`; `FaultBand` has no lowercase family column, and the owner key alone collapses every AppHost band onto one domain, which strips `(domain, case)` of its identity (thirty AppHost rows share `TelemetrySource.AppHost`). The pair stays OPAQUE at every peer — a remote `domain` never drives topology or failover, and the admission consults this process's own ledger only to refuse a same-solution ordinal past its band's span, which is version-skew evidence rather than taxonomy rehydration.
- Law: `FaultObservation.(domain, case)` carries the SAME family identity `FaultDetail.(domain, case)` carries, so one field pair has one meaning across both generated messages. `Observe` derives the pair together from the kernel observation's optional numeric code; a foreign `Error` crosses with both absent, and the generated paired-presence rule refuses either field alone. The kernel `FaultId.Case` token NEVER crosses, so a leaf rename stays one compilation.
- Law: `FaultRecovery` is the ONE recovery crossing and its arm set is the kernel `Retriability` union's — `Recovery(Retriability)` assigns exactly one arm through the generated total `Switch`, and `Recovery(FaultRecovery)` dispatches on `KindCase` so an unset message, a `None` kind, an absent `retry_delay`, and a negative delay each refuse as their own typed `WireViolation`. The throttled arm IS a `google.rpc.RetryInfo`, so `Raise` packs THAT instance a second time as a top-level `Status` detail and a foreign gRPC client reads standard back-off without decoding a rasm type; one message occupies both seats and a second construction is the deleted form that lets the two disagree. `RetryInfo` states a delay alone, which is why the solution keeps the oneof: `terminal` and `transient` are unspellable inside it.
- Law: `Raise` packs exactly one `FaultDetail` per solution fault except a kernel contract admission, which packs one standard `BadRequest` instead of duplicating its violations into two details. Request refusal stays `InvalidArgument`, invalid server response stays `Internal`, and both preserve the same typed field coordinates without refused values. Other kernel and domain faults retain their solution detail. A captured foreign exception crosses as `google.rpc.DebugInfo` through `ToRpcDebugInfo`, and an exception-free foreign error crosses with its message alone, so a fabricated case, domain, or recovery never reaches a peer. `Decode` reads `RpcException.GetRpcStatus()` under `Op.Catch`: absent answers `None`, a malformed trailer refuses typed on `WireBoundary.RemoteStatus`, exactly one recognized `FaultDetail` admits as opaque `RemoteFault`, zero recognized details answer `None` so the caller's transport classification decides, and several refuse on `WireBoundary.DetailMultiplicity` — never a silent fallback. `Message` comes from the enclosing `google.rpc.Status`, never re-rendered.
- Law: every cross-process family crosses as its generated semantic-package message; local domain facts use the strict source-generated merge and `ProtoJsonConverterFactory` preserves ProtoJSON for embedded messages.
- Law: the `Hlc` stamp crosses as `Clock.Hlc{physical sfixed64 Unix ticks, logical uint64}`; `HostWire.Stamp` owns both directions and the packed `UInt128` remains an in-process content-key lane.
- Growth: a new generated file is one reflection root on `WireAdmission.Files`; a new fault family lands its band row at the kernel; a local fact requiring strict serialization adds one `[JsonSerializable]` row.
- Boundary: `WireJson` owns the statement-shaped stream doors, `FaultWire.Raise` owns the transport-required throw, and every temporal generated member crosses through protobuf well-known types. `SuiteContracts` remains local and exports no schema.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
// Contracts are retired from this logic.
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using Thinktecture.Text.Json.Serialization;
// Contracts are retired from this logic.
using static LanguageExt.Prelude;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] ---------------------------------------------------------------------------
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireViolation {
    private WireViolation() { }

    public sealed record Domain(string Value) : WireViolation;
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
    public sealed record Multiplicity(int Count) : WireViolation;
    public sealed record Captured(Error Cause) : WireViolation;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FaultContext(
    CorrelationId Correlation,
    Clock.Hlc Stamp,
    Option<TenantId> Tenant,
    Seq<BadRequest.Types.FieldViolation> Violations) {
    public static FaultContext Of(CorrelationId correlation, (Instant Physical, ulong Logical) stamp, TenantContext tenant,
        Seq<BadRequest.Types.FieldViolation> violations = default) =>
        new(correlation, HostWire.Stamp(stamp), tenant.Partitions ? Some(tenant.TenantId) : None, violations);
}

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

// --- [OPERATIONS] ----------------------------------------------------------------------
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

public static class WireJson {
    public static readonly JsonFormatter Formatter = new(
        JsonFormatter.Settings.Default.WithTypeRegistry(WireAdmission.Registry));

    public static readonly JsonParser Parser = new(JsonParser.Settings.Default
        .WithIgnoreUnknownFields(true)
        .WithRecursionLimit(100)
        .WithTypeRegistry(WireAdmission.Registry));

    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

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

    public static JsonElement Element(IMessage message) {
        using JsonDocument parsed = JsonDocument.Parse(Formatter.Format(message));
        return parsed.RootElement.Clone();
    }

    public static Fin<T> Read<T>(JsonElement payload, Op key) where T : IMessage<T>, new() =>
        key.Catch(() => Fin.Succ(Parser.Parse<T>(payload.GetRawText())))
            .Bind(message => WireAdmission.Admit(message, WireBoundary.InboundPayload, key));
}

public static class HostWire {
    public static ByteString Correlation(CorrelationId correlation) =>
        ByteString.CopyFrom(((Guid)correlation).ToByteArray(bigEndian: true));

    public static Fin<CorrelationId> Correlation(ByteString wire, Op key) =>
        wire.Length == 16
            ? Fin.Succ(CorrelationId.Create(new Guid(wire.Span, bigEndian: true)))
            : Fin.Fail<CorrelationId>(key.InvalidInput(nameof(CorrelationId)));

    public static Clock.Hlc Stamp((Instant Physical, ulong Logical) hlc) =>
        new() { Physical = hlc.Physical.ToUnixTimeTicks(), Logical = hlc.Logical };

    public static Fin<(Instant Physical, ulong Logical)> Stamp(Clock.Hlc wire, Op key) =>
        wire.Physical >= 0L
            ? Fin.Succ((Instant.FromUnixTimeTicks(wire.Physical), wire.Logical))
            : Fin.Fail<(Instant, ulong)>(key.InvalidInput(nameof(Clock.Hlc)));

    public static T Arm<T>(this T message, Action<T> arm) where T : class, IMessage<T> {
        arm(message);
        return message;
    }
}

public static partial class FaultWire {
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
    public static Fault.FaultRecovery Recovery(Retriability recovery) => recovery.Switch(
        terminalCase: static _ => new Fault.FaultRecovery { Terminal = new Empty() },
        transientCase: static _ => new Fault.FaultRecovery { Transient = new Empty() },
        throttledCase: static row => new Fault.FaultRecovery {
            RetryAfter = new RetryInfo { RetryDelay = row.RetryAfter.ToProtobufDuration() },
        });

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
    public static RpcException Raise(Error error, FaultContext context) =>
        new Google.Rpc.Status {
            Code = (int)Status(error),
            Message = error.Message,
            Details = { Details(error, context) },
        }.ToRpcException();

    static Seq<Any> Details(Error error, FaultContext context) =>
        ((error, context.Violations) switch {
            (KernelFault, { IsEmpty: false }) => Seq<Any>(),
            (Domain.Fault fault, _) => Pack(fault, context).Match(
                Succ: static detail => Seq(Any.Pack(detail))
                    + Optional(detail.Recovery.RetryAfter).Map(static advice => Any.Pack(advice)).ToSeq(),
                Fail: static _ => Seq<Any>()),
            _ => error.Exception.Map(static raised => Any.Pack(raised.ToRpcDebugInfo())).ToSeq(),
        }) + (error is KernelFault && !context.Violations.IsEmpty
            ? Seq(Any.Pack(new BadRequest { FieldViolations = { context.Violations } }))
            : Seq<Any>());

    // --- [DECODE]
    public static Fin<Option<RemoteFault>> Decode(RpcException raised) =>
        Op.Of().Catch(() => Fin.Succ(Optional(raised.GetRpcStatus())))
            .MapFail(captured => (Error)new HopFault.Malformed(
                WireBoundary.RemoteStatus, new WireViolation.Captured(captured)))
            .Bind(status => status.Match(
                None: static () => Fin.Succ(Option<RemoteFault>.None),
                Some: held => Recognized(held)));

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

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PhaseCommit))]
[JsonSerializable(typeof(BootMarker))]
[JsonSerializable(typeof(FaultSource))]
[JsonSerializable(typeof(ReplayOutcome))]
[JsonSerializable(typeof(StepStateRow))]
[JsonSerializable(typeof(HealthSnapshot))]
[JsonSerializable(typeof(DegradationState))]
[JsonSerializable(typeof(SupportManifest))]
[JsonSerializable(typeof(DumpTriage))]
[JsonSerializable(typeof(DiscoveryManifest))]
[JsonSerializable(typeof(TenantContext))]
[JsonSerializable(typeof(CommandArguments))]
[JsonSerializable(typeof(CommandResult))]
[JsonSerializable(typeof(Alert))]
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(Delivery))]
[JsonSerializable(typeof(DeliverySettled))]
public partial class AppHostWireContext : JsonSerializerContext;

public static class SuiteContracts
{
    public static JsonSerializerOptions Host {
        get => field ?? throw new InvalidOperationException("SuiteContracts.Wire seats Host at the app-root mint.");
        private set;
    }

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
            Converters = {
                new ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true),
                new LanguageExtJsonConverterFactory(),
                new ProtoJsonConverterFactory(),
            },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        wire.MakeReadOnly();
        Host = wire;
        return wire;
    }
}
```

Codec placement is fixed per wire surface; producer and consumer cells name endpoints only, not alternate codecs.

| [INDEX] | [WIRE_SURFACE]             | [CODEC]                                      | [PRODUCER]            | [CONSUMER]                |
| :-----: | :------------------------- | :------------------------------------------- | :-------------------- | :------------------------ |
|  [01]   | semantic contract families | ProtoJSON via `WireJson`                     | owning producer       | generated peer schema     |
|  [02]   | discovery manifest         | STJ Strict atomic JSON                       | app-root boot         | attaching peer            |
|  [03]   | service verbs              | protobuf over gRPC                           | app roots             | connect-es clients        |
|  [04]   | wire faults                | `google.rpc.Status` details via `FaultWire`  | `FaultWire.Raise`     | `FaultWire.Decode`        |
|  [05]   | snapshot blobs             | MessagePack                                  | snapshot rows         | @msgpack/msgpack          |
|  [06]   | telemetry signals          | OTLP                                         | exporters             | OTLP collector            |
|  [07]   | in-process domain facts    | source-generated STJ with ProtoJSON members  | `SuiteContracts.Host` | local hook/event dispatch |
|  [08]   | semantic-time fields       | `Timestamp`/`Duration`; NodaTime STJ on [07] | owning mapper         | well-known types          |

Every family group riding row [01] has one generated-message producer. Mapperly owns structural correspondences; direct constructors own operational folds whose source is not a shape twin.

| [INDEX] | [FAMILY_GROUP]                  | [PRODUCER]            | [OWNER]                                   |
| :-----: | :------------------------------ | :-------------------- | :---------------------------------------- |
|  [01]   | binding status, coercion, write | `LiveWireContract`    | `Wire/livewire#TS_PROJECTION`             |
|  [02]   | command availability            | `AvailabilityMap`     | `Observability/health#DEGRADATION_LADDER` |
|  [03]   | credential material             | `CredentialPublicMap` | `Runtime/secrets#CREDENTIAL_PEM`          |
|  [04]   | flag verdict                    | `FeatureMap`          | `Runtime/features#VERDICT_PROJECTION`     |
|  [05]   | host fingerprint                | `HostFingerprintMap`  | `Runtime/determinism#DETERMINISM_KERNEL`  |
|  [06]   | descriptor pin                  | `DescriptorPin.Of`    | `Agent/capability#SDK_CODEGEN`            |
|  [07]   | control replies                 | `ControlReplyMap`     | `Wire/companion#CONTROL_SERVICE`          |

## [04]-[RESEARCH]

(none)
