# [COMPUTE_WIRE]

Rasm.Compute owns its generated-contract consumption and channel admission while the corpus owns what the wire says and `Runtime/channels` owns how it moves. Bindings arrive by project reference to `Rasm.Contracts` — one committed emission serving client and server alike — and one corpus emission owns compatibility: a wire reshapes in place, the contract generation (`ContractGeneration.Subject`) compares at attach, unknown fields tolerate otherwise.

One compact `rasm.contracts.fault.FaultDetail` — `domain`, `case`, `correlation`, `stamp`, `tenant`, `recovery`, `violations` — carries the producing family's numeric identity, correlation, clock, tenancy, recovery, and rule refusals across the edge while the message rides the enclosing `google.rpc.Status`. Client-edge `WireFault` stays total over transport status, while a valid remote detail lands as opaque `RemoteFault` evidence rather than rehydrating or mirroring the source family.

`GaussianSplatScan` is minted and artifact-published by the Python `ScanIngestion.run` producer from the corpus-owned `scan` schema and consumed here through `SplatMapper.Read`; C# does not re-mint the vocabulary. `Read` fetches the exact SHA-256 `ArtifactRef`, preserves the frame edge's allocation evidence, performs the bounded generated parse, and then calls the Mapperly projection. Mapperly `[Mapper]` per-row transcription and Generator.Equals `[Equatable]` structural equality are the admitted proto-to-domain boundary generators. Package spine: Rasm.Contracts, Google.Protobuf, Grpc.Net.Client, Grpc.StatusProto, Google.Api.CommonProtos, Celly.Protovalidate, NodaTime.Serialization.Protobuf, Riok.Mapperly, Generator.Equals, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [01]-[INDEX]

- [02]-[PROTO_VOCABULARY]: consumes the corpus-owned compute, progress, stage, control, artifact, fault, clock, scan, and event families, rosters the surviving generated services, seats the bounded `ParseGuard` beside `WireServices`, and holds the one enum-to-key lowering every interior roster reads.
- [03]-[FAULT_PROJECTION]: the total `StatusCode`→`WireFault` client rail and the `RemoteFault` admission composed off AppHost `FaultWire`.
- [04]-[TS_PROJECTION]: the browser consumes the generated `@rasm\/contracts` schemas and service descriptors; this page mints no TS shape.

## [02]-[PROTO_VOCABULARY]

- Owner: generated compute/progress/control services and every generated message family admitted at Compute ingress; `WireServices` holds the composition's raw shared channel/invoker and `WireCall` binds the generated CLIENT family once to one logical call's `CallSpine`; `ParseGuard` owns bounded parse plus one Celly validator over its closed descriptor set — compute, progress, stage, control, artifact, fault, clock, scan, and `event` for broker extensions, each family earning its seat through a consumer at this ingress; `RuleViolations` projects accumulated rule failures; `WireKeys` folds a generated enum member to the interior roster key that names it.
- Cases: dialed — `compute.ComputeService`, `artifact.ArtifactService`, `compute.ControlService`; served — `compute.ProgressService`. `grpc.health.v1.Health` and `google.rpc.Status` are upstream standards this corpus never mints: their generated types ship in Grpc.HealthCheck and Google.Api.CommonProtos, the server binds health through `MapGrpcHealthChecksService` at `Rasm.AppHost/Observability/health#WIRE_HEALTH`, and `WireCall.Health` holds the package-shipped client.
- Law: each semantic package under `libs/contracts/proto/rasm/contracts/<family>/` is the one mint of its fully-qualified names. `compute` owns tessellation, control, and progress, `stage` owns the photo-to-PBR crossing, `artifact` owns transfer, and `scan` owns Gaussian scan payloads; managed mode derives `Rasm.Contracts.<Family>` with no source `csharp_namespace`. `FaultDetail.domain` and `FaultDetail.case` together are the sole transported fault identity; every peer keeps the remote pair opaque instead of mirroring another branch's band ledger.
- Law: a non-RPC payload message keeps the plain concept name — `FaultDetail`, `ArtifactFrame`, `GaussianSplatScan` — and mints a `Wire` suffix only to break a collision with a co-resident domain type; a consumer registry transcribes whichever name the descriptor declares and re-spells nothing. Where a page names both, the generated side takes the alias suffix at the fence prelude (`using ProgressPhaseWire = Rasm.Contracts.Compute.ProgressPhase;`).
- Law: a wire message an rpc binds spells `<Verb>Request` inbound and `<Verb>Response` outbound, so the rpc and its two messages resolve from one verb and no envelope stands between the verb and the payload it carries; the domain shape it transcribes keeps its own name — the app root drives the `Rasm.Bim` `Exchange/tessellation#TESSELLATION_BRIDGE` projection onto generated `TessellateRequest`, and `Runtime/payload#RESIDENCY` transcribes `SplatScan` onto `GaussianSplatScan`. Compute carries no parallel tessellation request or policy mirror.
- Law: `rasm.contracts.stage` is a SAME-LANGUAGE process crossing seated at the corpus — Materials specifies the request, the app root relays the bytes, and `Model/stage#STAGE_WIRE` admits `StageRequestWire` through `ParseGuard.Read` and answers `StageResultWire`. It carries no service, so `WireCall` gains no client for it; the family earns its `ParseGuard` seat through that admission alone. No slot roster, positional checksum, or reciprocal mirror survives on either end — the descriptor IS the compatibility law, and a column landed at one end alone refuses at the corpus gate rather than at a digest two hand rosters computed.
- Law: the unknown-field posture and validation seat are ONE admission — `ParseGuard.Parser<T>` memoizes the generated parser under `WithDiscardUnknownFields(false)` per message type, so a retired peer field lands in the `UnknownFieldSet` and never raises, while `ParseGuard.Read` validates every parsed message through the ONE process-wide `Celly.Protovalidate.Validator` AFTER the parse and BEFORE the interior sees the value. `WarmRules` walks the closed non-map message-descriptor set and validates one default instance per descriptor before readiness, forcing Celly's lazy CEL compilation into bootstrap; `Validated` admits only those full names and projects every accumulated refusal onto `WireViolation.Rules(Seq<BadRequest.Types.FieldViolation>)`. JSON intake tolerates unknown fields the same way through AppHost `WireJson.Parser` (`WithIgnoreUnknownFields(true)`), so binary and JSON share one posture.
- Law: `WireKeys.Camel` is the ONE lowering from a generated enum member to a Compute-interior key string, and it is TOTAL over every roster this branch reads by key — the generated member name with its first character lowered, so `IntrinsicAppearance` answers `intrinsicAppearance` and `CoreMl` answers `coreMl`. A roster whose key needs any other fold refuses at that roster's own generated `TryGet` rather than growing a second lowering here, and a hand `(enum, key)` table anywhere on this branch is the deleted form the fold replaces. `defined_only` already refused an undefined ordinal at admission, so the fold's unnamed arm is unreachable through `ParseGuard`.
- Law: corpus-owned `scan.GaussianSplatScan` rides `artifact.ArtifactFrame` as a standalone artifact. Python `ScanIngestion.run` is the sole domain producer; `SplatMapper.Read` fetches the generated `ArtifactRef`, `FrameEdge` proves its fixed SHA-256 and extent, `ParseGuard.Read` performs the bounded descriptor admission, and `ToDomain` projects it once into `Runtime/payload#RESIDENCY` `SplatScan`. No semantic `ContentHash`, reverse C# wire minter, or geometry-envelope alias enters that artifact path.
- Auto: the bindings are ONE committed emission — `assay contracts generate` writes `Rasm.Contracts`, this package and every other consumer reference it by project, app roots derive generated bases from the same assembly, and connect-es reads service descriptors off the generated `_pb.ts`.
- Packages: Rasm.Contracts (project — generated compute, artifact, control, progress, stage, scan, event, and fault families), Google.Protobuf (`MessageParser<T>.WithDiscardUnknownFields`, `CodedInputStream.CreateWithLimits`, `MessageDescriptor.Parser`, `Struct`, `Value`, `UnknownFieldSet`), Celly.Protovalidate (`Validator`, accumulated `Buf.Validate.Violation`, `FieldPath`, `FieldPathElement`), Google.Api.CommonProtos (`BadRequest.Types.FieldViolation`), Grpc.Core.Api (`CallInvoker`, `CallOptions`, `AsyncUnaryCall<T>`, `InterceptorExtensions.Intercept`), Grpc.Net.Client, CommunityToolkit.HighPerformance (`ReadOnlySequenceExtensions.AsStream`), NodaTime, NodaTime.Serialization.Protobuf, Riok.Mapperly, LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.AppHost (project — `FaultWire`, `RemoteFault`, `WireViolation`, `WireBoundary`, `WireJson`, `ContractGeneration`)
- Growth: one rpc row on an existing service or one numbered message field absorbs a new wire fact; zero new surface. An rpc lands with its corpus row, generated service roster, server override, and real peer invocation in one motion; a service-only or client-only declaration is deleted rather than padded with an unused adapter.
- Boundary: temporal values cross as `Timestamp` and protobuf `Duration` through `ToTimestamp`/`ToProtobufDuration` outward and `ToInstant`/`ToNodaDuration` inward; ProtoJSON formatting and parsing of every generated message is AppHost `Runtime/ports#WIRE_LAW` `WireJson`. `ParseGuard.Read` gates a payload before bounded parse, while `Runtime/channels#ARTIFACT_FRAMES` streams Put and Fetch through the shared frame law under `WireLimits.Artifact`. Sync state, diffing, transfer manifests, and atomic storage strategy remain store mechanics and cannot alias onto either RPC.

```csharp
using Grpc.Core.Interceptors;
using ArtifactRef = Rasm.Contracts.Artifact.ArtifactRef;
using ArtifactService = Rasm.Contracts.Artifact.ArtifactService;
using GaussianSplatScan = Rasm.Contracts.Scan.GaussianSplatScan;
using SplatFormat = Rasm.Contracts.Scan.SplatFormat;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WireServices(GrpcChannel Channel, CallInvoker Invoker) : IDisposable {
    public static WireServices Of(CallInvoker invoker, GrpcChannel channel) {
        ParseGuard.WarmRules();
        return new(channel, invoker);
    }

    public WireCall Bind(CallSpine spine) => WireCall.Of(Invoker.Intercept(spine));

    public void Dispose() => Channel.Dispose();
}

public sealed record WireCall(
    ComputeService.ComputeServiceClient Compute,
    ControlService.ControlServiceClient Control,
    ArtifactService.ArtifactServiceClient Artifacts,
    Health.HealthClient Health) {
    public static WireCall Of(CallInvoker invoker) => new(
        new ComputeService.ComputeServiceClient(invoker),
        new ControlService.ControlServiceClient(invoker),
        new ArtifactService.ArtifactServiceClient(invoker),
        new Health.HealthClient(invoker));
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class ParseGuard {
    private static readonly FileDescriptor[] Files = [
        ComputeReflection.Descriptor, ControlReflection.Descriptor, ProgressReflection.Descriptor,
        global::Rasm.Contracts.Stage.StageReflection.Descriptor,
        global::Rasm.Contracts.Artifact.ArtifactReflection.Descriptor, FaultReflection.Descriptor,
        global::Rasm.Contracts.Clock.HlcReflection.Descriptor,
        global::Rasm.Contracts.Scan.GaussianReflection.Descriptor,
        global::Rasm.Contracts.Event.EventReflection.Descriptor,
    ];
    private static readonly FrozenDictionary<string, MessageDescriptor> Allowed = Messages(Files)
        .ToFrozenDictionary(static descriptor => descriptor.FullName, StringComparer.Ordinal);

    public static readonly Validator Rules = new(Files);

    public static Unit WarmRules() {
        Allowed.Values.Iter(static descriptor => ignore(Rules.Validate(descriptor.Parser.ParseFrom([]))));
        return unit;
    }

    public static MessageParser<T> Parser<T>(MessageParser<T> generated) where T : IMessage<T> => Configured<T>.Of(generated);

    public static Fin<T> Read<T>(MessageParser<T> generated, ReadOnlySequence<byte> payload, WireLimits limits) where T : IMessage<T> =>
        payload.Length > limits.SizeLimit
            ? Fin.Fail<T>(new ComputeFault.PayloadOverBounds($"<inbound-over-bound:{payload.Length}:{limits.SizeLimit}>"))
            : Op.Of().Catch(() => Fin.Succ(Parser(generated).ParseFrom(
                    CodedInputStream.CreateWithLimits(payload.AsStream(), limits.SizeLimit, limits.RecursionLimit))))
                .MapFail(static error => (Error)new WireFault.Internal(WireBoundary.InboundPayload, error))
                .Bind(Validated);

    public static Fin<T> Validated<T>(T message) where T : IMessage<T> =>
        !Allowed.ContainsKey(message.Descriptor.FullName)
            ? Fin.Fail<T>(new ComputeFault.WireDecodeRejected($"<validator-message-unrostered:{message.Descriptor.FullName}>"))
            : Rules.Validate(message) switch {
                [] => Fin.Succ(message),
                var violations => Fin.Fail<T>(new WireFault.InvalidRequest(
                    new WireViolation.Rules(toSeq(violations).Map(RuleViolations.Violation)))),
            };

    public static Struct Envelope(HashMap<string, Value> options) =>
        new() { Fields = { options.ToDictionary(static entry => entry.Key, static entry => entry.Value) } };

    private static class Configured<T> where T : IMessage<T> {
        private static readonly Atom<Option<MessageParser<T>>> held = Atom(Option<MessageParser<T>>.None);

        public static MessageParser<T> Of(MessageParser<T> generated) =>
            held.Swap(seated => seated | Some(generated.WithDiscardUnknownFields(false))).IfNone(generated);
    }

    private static IEnumerable<MessageDescriptor> Messages(IEnumerable<FileDescriptor> files) =>
        files.SelectMany(static file => Messages(file.MessageTypes));

    private static IEnumerable<MessageDescriptor> Messages(IEnumerable<MessageDescriptor> descriptors) {
        foreach (MessageDescriptor descriptor in descriptors) {
            if (descriptor.IsMapEntry) { continue; }
            yield return descriptor;
            foreach (MessageDescriptor nested in Messages(descriptor.NestedTypes)) { yield return nested; }
        }
    }
}

public static class RuleViolations {
    public static BadRequest.Types.FieldViolation Violation(Violation violation) => new() {
        Field = Path(violation.Field),
        Reason = violation.RuleId,
        Description = violation.Message,
    };

    private static string Path(FieldPath? field) =>
        field is null || field.Elements.Count == 0
            ? ""
            : string.Join('.', field.Elements.Select(Element));

    private static string Element(FieldPathElement element) =>
        (element.HasFieldName ? element.FieldName : $"#{element.FieldNumber.ToString(CultureInfo.InvariantCulture)}")
        + (element.SubscriptCase switch {
            FieldPathElement.SubscriptOneofCase.None => "",
            FieldPathElement.SubscriptOneofCase.Index => $"[{element.Index.ToString(CultureInfo.InvariantCulture)}]",
            FieldPathElement.SubscriptOneofCase.BoolKey => element.BoolKey ? "[true]" : "[false]",
            FieldPathElement.SubscriptOneofCase.IntKey => $"[{element.IntKey.ToString(CultureInfo.InvariantCulture)}]",
            FieldPathElement.SubscriptOneofCase.UintKey => $"[{element.UintKey.ToString(CultureInfo.InvariantCulture)}]",
            FieldPathElement.SubscriptOneofCase.StringKey => $"[{JsonSerializer.Serialize(element.StringKey)}]",
            _ => throw new UnreachableException($"Unknown field-path subscript {element.SubscriptCase}"),
        });
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(SplatCodec))]
public static partial class SplatMapper {
    [MapProperty(nameof(GaussianSplatScan.Format), nameof(SplatScan.FormatKey), Use = nameof(SplatCodec.Key))]
    public static partial SplatScan ToDomain(GaussianSplatScan wire);

    public static IO<Fin<(SplatScan Scan, AllocationEvidence Copy)>> Read(
        WireCall calls,
        CallSpine spine,
        StreamPool pool,
        ArtifactRef artifact,
        CancellationToken token) =>
        FrameEdge.Fetch(calls, spine, pool, artifact, token).Map(result => result.Bind(copy =>
            ParseGuard.Read(
                    GaussianSplatScan.Parser,
                    new System.Buffers.ReadOnlySequence<byte>(copy.Payload),
                    WireLimits.Artifact)
                .Map(wire => (ToDomain(wire), copy.Evidence))));
}

public static class SplatCodec {
    public static string Key(SplatFormat format) => WireKeys.Camel(format);

    public static ReadOnlyMemory<float> Planes(ByteString packed) =>
        MemoryMarshal.Cast<byte, float>(packed.Span).ToArray();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class WireKeys {
    public static string Camel<TEnum>(TEnum value) where TEnum : struct, Enum =>
        Enum.GetName(value) is { Length: > 0 } name
            ? string.Create(name.Length, name, static (span, source) => {
                source.CopyTo(span);
                span[0] = char.ToLowerInvariant(span[0]);
            })
            : value.ToString();
}
```

```proto
syntax = "proto3";

package rasm.contracts.compute;
```

Each row names one rpc the semantic corpus sources declare; the generated `<Svc>.<Svc>Client` and `<Svc>Base` carry exactly these:

| [INDEX] | [SERVICE]       | [RPC]          | [SHAPE]       | [MESSAGES]                                     |
| :-----: | :-------------- | :------------- | :------------ | :--------------------------------------------- |
|  [01]   | ComputeService  | Tessellate     | unary         | TessellateRequest → TessellateResponse         |
|  [02]   | ArtifactService | Fetch          | server-stream | FetchRequest → FetchResponse                   |
|  [03]   | ArtifactService | Put            | client-stream | PutRequest → PutResponse                       |
|  [04]   | ControlService  | SetDegradation | unary         | SetDegradationRequest → SetDegradationResponse |
|  [05]   | ControlService  | DrainRuntime   | unary         | DrainRuntimeRequest → DrainRuntimeResponse     |
|  [06]   | ProgressService | Watch          | server-stream | WatchRequest → stream WatchResponse            |

Each rpc carries one wire law:

- [01]-[TESSELLATE]: bridges tessellation to the companion over two hops — the app root transcribes the Bim bridge request, `Runtime/tiles#TWO_HOP_TESSELLATION` validates and invokes it, and the Python geometry service reads the same generated message
- [02]-[FETCH]: `Runtime/channels#ARTIFACT_FRAMES` validates `FetchRequest.sha256`, unwraps each `FetchResponse.frame`, and admits arrival-ordered frames under the whole-artifact SHA-256 gate
- [03]-[PUT]: the same owner validates each shared frame, wraps it in `PutRequest.frame`, and admits `PutResponse.artifact` against the submitted digest and extent; storage mode remains the provider's atomic implementation detail
- [04]-[SETDEGRADATION]: `SetDegradationRequest` lands its `DegradationLevel` on the one override rail as the `Rasm.AppHost/Runtime/config#KILL_SWITCH` `ForceLevel` arm, and the response answers the resolved level the caller re-admits; richer degradation evidence rides the health owner, so a column lands here when a caller reads it
- [05]-[DRAINRUNTIME]: commits the drain phase and folds onto `Rasm.AppHost/Runtime/lifecycle#DRAIN_CONDUCTOR`, re-implementing nothing it holds; the request carries the parent's REMAINING cooperative allotment and the handler takes `min(inherited, DeadlineClass.DrainCooperative)`, since allotments inherit through nested seams as the minimum
- [06]-[WATCH]: Compute SERVES this one — `Runtime/progress#OBSERVATION_SEAMS` `ProgressStream` overrides the generated base, admits the request through `ParseGuard.Validated`, subscribes the correlated `ProgressCell` at `SubscriptionPolicy.Wire`, ends the stream on the terminal mark, and the app root maps the service beside `Rasm.AppHost/Wire/companion#CONTROL_SERVICE` `ControlServiceImpl`; the peer client is `typescript:core/interchange/invoke#PROGRESS_WATCH`

Each message carries its generated field set and wire role; enum vocabularies carry their `_UNSPECIFIED = 0` arm refused by `defined_only` + `not_in: [0]` rules:

- [01]-[COMPUTE]: `Spill`, `GeomSetting`, `Dimensionality`, IFC scope, request, semantic, and response; every unspecified enum arm is refused at admission
- [02]-[SCAN]: `scan.GaussianSplatScan` carries the format and five exact float32 byte columns; message CEL proves every byte width from `splat_count` and harmonic degree
- [03]-[TESSELLATEREQUEST]: `policy=2; tolerance_m=4; geom_settings=5; dimensionality=6; scope=7; source_artifact=8 ArtifactRef`, with `1`/`source` and `3`/`angle_tolerance_rad` reserved — every IfcOpenShell output setting crosses typed and required materials/GUID capabilities are CEL-proved; IFC is the sole source this request admits, so STEP and IGES tessellate through `cad.CadService.Tessellate`
- [04]-[TESSELLATIONSCOPE]: required `oneof kind { whole_model=1 Empty; elements=2 ElementScope; entities=3 EntityScope; exclude_entities=4 EntityScope }`; token lists retain validated carriers
- [05]-[SEMANTIC]: `schema=1 string; project=2 string` — source-declared labels
- [06]-[TESSELLATERESPONSE]: `content_key=1 bytes(16); element_count=2 uint64; triangle_count=3 uint64; semantic=4 Semantic; spill=6 Spill; artifact=7 ArtifactRef`
- [07]-[ARTIFACTIDENTITY]: `FetchRequest.sha256=1 bytes(32)`; `ArtifactRef.sha256=1 bytes(32); artifact_bytes=2 uint64` — SHA-256 over the exact raw artifact octets beside their extent
- [08]-[ARTIFACTFRAME]: `payload=3 bytes(1..65536); artifact=4 ArtifactRef` — the shared ordered payload law inside distinct Fetch/Put direction envelopes
- [09]-[PATCHOP]: required RFC 6902 operation oneof in `patch`; Compute does not own a JSON-patch vocabulary
- [10]-[SETDEGRADATIONREQUEST]: `level=1 DegradationLevel; reason=2 string` — the level beside the reason it is forced
- [11]-[SETDEGRADATIONRESPONSE]: `level=1 DegradationLevel` — the resolved level
- [12]-[DRAINRUNTIMEREQUEST]: `cooperative=1 Duration; reason=2 string` — the caller's remaining cooperative allotment
- [13]-[DRAINSTEP]: `name=1; band=2; allotted=3; consumed=4; outcome=5` — projects one typed drain step
- [14]-[DRAINRUNTIMERESPONSE]: `steps=1; final_phase=2; at=3; elapsed=4; correlation=5` — projects `DrainReceipt` field-for-field
- [15]-[WATCHREQUEST]: `correlation=1 bytes(16)` — the RFC 4122 form of the watched intent's own key, and the whole request
- [16]-[WATCHRESPONSE]: `phase=1 ProgressPhase; fraction=2 optional double; segments=3 optional uint64; at=4 Timestamp; correlation=5 bytes(16)` — the two measurements cross OPTIONAL so an unmeasured phase publishes absence rather than a zero a chart reads as a stall
- [17]-[STAGE]: `rasm.contracts.stage.StageRequestWire`/`StageResultWire` with their nested product, input, output, score, and bucket rows — admitted here and executed at `Model/stage#STAGE_WIRE`

## [03]-[FAULT_PROJECTION]

- Owner: `WireFault` is the local client-edge transport rail; `StatusRail` the CLIENT fold `StatusCode → WireFault`, keyed by numeric `StatusCode`; `Classify` the residual-status projection; `Decode` the detail admission composed off AppHost `FaultWire.Decode`.
- Cases: every local transport arm derives its numeric identity from `[FaultCase]` against `FaultBand.Wire`, ordinals compacted with the unshipped plane; `Remote` carries the foreign `RemoteFault` opaquely and never reconstructs its source family.
- Entry: `Decode(RpcException error, Error cause)` returns a cause-bearing `Remote` when AppHost admits exactly one detail, `None` when none was present; `Classify` projects residual transport status.
- Law: `FaultWire` — `Observe`, `Recovery`, `Pack`, `Status`, `Raise`, `Decode`, `Admit` — together with `RemoteFault`, `WireViolation`, `WireBoundary`, and `FaultContext` live at `Rasm.AppHost/Runtime/ports#WIRE_LAW`; this page declares none of them and composes them by name. `FaultWire.Status` is the producer fold (`Error → StatusCode`); `StatusRail` here is its client inverse and never packs. `Decode` reads `error.GetRpcStatus()` through `Grpc.StatusProto` inside AppHost.
- Law: recovery crosses through the generated `FaultRecovery` oneof because numeric identity alone cannot determine it; the detail's `domain` + `case` pair is the producing family's ordinal under its owner key and is NEVER a gRPC status code; the message rides `google.rpc.Status`.
- Packages: Grpc.Core.Api (`RpcException`, `StatusCode`), Rasm.AppHost (project — `FaultWire`, `RemoteFault`, `WireViolation`, `WireBoundary`), Rasm.Contracts (project — `Fault.FaultDetail`), LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (project)
- Growth: a new local transport arm is one `[FaultCase]` leaf and one `StatusRail` entry when a status gains distinct semantics; a new malformed-envelope condition is one `WireViolation` case at the AppHost owner.
- Boundary: the server raises through AppHost `FaultWire.Raise` with one `FaultDetail` in `google.rpc.Status.details`; the client admits exactly one recognized detail as opaque `RemoteFault` on a cause-bearing `WireFault.Remote`. Zero recognized details use transport classification; malformed or multiple recognized details retain the caught RPC error on AppHost's typed `WireBoundary` evidence. In-band conflict slots admit the same compact envelope as response evidence without fabricating a transport cause. Status lookup is keyed by numeric `StatusCode`, never ordinal position. This family is the CLIENT edge alone — the served `ProgressService` leaves its refusals through `FaultWire.Raise` at `Runtime/progress#OBSERVATION_SEAMS`, so no arm here is ever packed onto a trailer.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Wire;
    private WireFault(string message) => Message = message;

    public sealed override string Message { get; }

    [FaultCase(0)] public sealed partial record Cancelled(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(1)] public sealed partial record DeadlineExpired(string Detail, Error Cause) : WireFault(Detail), ICausedFault;

    [FaultCase(2)] public sealed partial record Unreachable(StatusCode Status, string Detail, Error Cause) : WireFault($"{Status}:{Detail}"), ICausedFault {
        public override Retriability Retriability => Status is StatusCode.Unavailable ? Retriability.Transient : Retriability.Terminal;
    }

    [FaultCase(3)] public sealed partial record InvalidRequest(WireViolation Violation) : WireFault("invalid wire value");
    [FaultCase(4)] public sealed partial record NotFound(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(5)] public sealed partial record PermissionDenied(string Detail, Error Cause) : WireFault(Detail), ICausedFault;

    [FaultCase(6)] public sealed partial record Exhausted(string Detail, Error Cause) : WireFault(Detail), ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }

    [FaultCase(7)] public sealed partial record Unauthenticated(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(8)] public sealed partial record Internal(WireBoundary Boundary, Error Cause)
        : WireFault($"wire boundary failed: {Boundary.Key}"), ICausedFault;
    [FaultCase(9)] public sealed partial record OutOfRange(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(10)] public sealed partial record DataLoss(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(11)] public sealed partial record Unimplemented(string Detail, Error Cause) : WireFault(Detail), ICausedFault;

    [FaultCase(12)] public sealed partial record Remote(RemoteFault Evidence, Error Cause) : WireFault(Evidence.Message), ICausedFault {
        public override Retriability Retriability => Evidence.Recovery;
    }

    public static Fin<Option<WireFault>> Decode(RpcException error, Error cause) =>
        FaultWire.Decode(error).Map(admitted => admitted.Map(remote => (WireFault)new Remote(remote, cause)));

    private static readonly FrozenDictionary<StatusCode, Func<string, Error, WireFault>> StatusRail =
        new Dictionary<StatusCode, Func<string, Error, WireFault>> {
            [StatusCode.Cancelled] = static (detail, cause) => new Cancelled(detail, cause),
            [StatusCode.DeadlineExceeded] = static (detail, cause) => new DeadlineExpired(detail, cause),
            [StatusCode.InvalidArgument] = static (detail, cause) => new Unreachable(StatusCode.InvalidArgument, detail, cause),
            [StatusCode.NotFound] = static (detail, cause) => new NotFound(detail, cause),
            [StatusCode.AlreadyExists] = static (detail, cause) => new Unreachable(StatusCode.AlreadyExists, detail, cause),
            [StatusCode.Aborted] = static (detail, cause) => new Unreachable(StatusCode.Aborted, detail, cause),
            [StatusCode.FailedPrecondition] = static (detail, cause) => new Unreachable(StatusCode.FailedPrecondition, detail, cause),
            [StatusCode.PermissionDenied] = static (detail, cause) => new PermissionDenied(detail, cause),
            [StatusCode.Unauthenticated] = static (detail, cause) => new Unauthenticated(detail, cause),
            [StatusCode.ResourceExhausted] = static (detail, cause) => new Exhausted(detail, cause),
            [StatusCode.Internal] = static (detail, cause) => new Unreachable(StatusCode.Internal, detail, cause),
            [StatusCode.OutOfRange] = static (detail, cause) => new OutOfRange(detail, cause),
            [StatusCode.DataLoss] = static (detail, cause) => new DataLoss(detail, cause),
            [StatusCode.Unimplemented] = static (detail, cause) => new Unimplemented(detail, cause),
            [StatusCode.Unavailable] = static (detail, cause) => new Unreachable(StatusCode.Unavailable, detail, cause),
        }.ToFrozenDictionary();

    public static WireFault Classify(RpcException error, Error cause) =>
        StatusRail.TryGetValue(error.StatusCode, out Func<string, Error, WireFault>? make)
            ? make(error.Status.Detail, cause)
            : new Unreachable(error.StatusCode, error.Status.Detail, cause);
}
```

## [04]-[TS_PROJECTION]

- Law: the browser consumes only its selected generated semantic-package schemas and this page mints no TS interface, alias, or method-shape roster. `artifact.ArtifactService.Fetch` is server-streaming and `Put` client-streaming where an app needs artifact transfer; `scan.GaussianSplatScan` rides framed bytes with no browser decode forced on every app; the `stage` family is a same-language crossing no browser reads, so it carries no TS consumer at all. `progress.ProgressService.Watch` is the browser's ONE dial into a Compute-served endpoint, bound at `typescript:core/interchange/invoke#PROGRESS_WATCH` over the generated `WatchResponseSchema`.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
