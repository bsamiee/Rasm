# [COMPUTE_WIRE]

Rasm.Compute owns the suite wire CONTRACT — what the wire SAYS, while `Runtime/channels` owns the channel MECHANICS (transport rows, channel tuning, call policy, artifact framing) and therefore how it MOVES. Proto files compile GrpcServices=Client in this package and GrpcServices=Server at app roots, and one descriptor-diff evolution law gates every drift class behind a canonical XxHash128 projection-checksum.

One compact `FaultDetail` carries numeric identity, presentation, correlation, clock, tenancy, and recovery across the edge. The client-edge `WireFault` rail remains total over transport status, while a valid remote detail lands as opaque `RemoteFault` evidence rather than rehydrating or mirroring the source family. `DocumentService`↔`DocumentTransaction` parity lands wire-complete here: an in-process transaction and a channel-crossing one return the identical typed receipt and decode the same in-band conflict.

`GaussianSplatScan` is MINTED here — SPZ v4 and SOG v2 are stable versioned published law, so C# owns the reality-capture wire vocabulary — and the browser TS posture projects the whole suite wire as type-only contracts. Mapperly `[Mapper]`/`[MapDerivedType]` per-case transcription and Generator.Equals `[Equatable]` structural equality are the admitted proto↔domain boundary generators. Package spine: Google.Protobuf, Grpc.Tools, Grpc.Net.Client, NodaTime.Serialization.Protobuf, Riok.Mapperly, Generator.Equals, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [01]-[INDEX]

- [02]-[PROTO_VOCABULARY]: mints the contract-file header and the `GaussianSplatScan` frame, rosters the wire services, and carries canonical geometry and support-bundle messages, the `DocumentService`↔`DocumentTransaction` parity seam, and the polymorphic field-mask and `Any` message envelopes.
- [03]-[CONTRACT_EVOLUTION]: descriptor-diff drift law over field/rpc/oneof/enum/packed/nested surface behind one canonical projection-checksum gate, with parse hardening and the branch-interior stage-crossing slot mirror.
- [04]-[FAULT_PROJECTION]: one compact `FaultDetail`, the total `StatusCode`→`WireFault` transport rail, numeric `FaultWire` packing beside typed recovery, and the in-band conflict decode arm.
- [05]-[TS_PROJECTION]: browser wire posture — fault and frame contracts, method shapes, the transaction-parity shape, field-mask read.

## [02]-[PROTO_VOCABULARY]

- Owner: the service contracts and the canonical geometry and support-bundle message families declared in the remote-lane owner folder; `WireServices` — the channel-scoped generated-client capsule whose `Of(CallInvoker, GrpcChannel)` is the ONE mint binding every declared service to one intercepted invoker, carrying one polymorphic `Mask` projection and the `Unpack` typed-fault projection; `WireDocument` — the flagship `DocumentService`↔`DocumentTransaction` parity surface folding budget, `Bounded` pre-check, `Classify`, and receipt emission into the canonical operation set field-for-field across in-process and cross-process; `TransactionDraft`/`WireDocumentMapper` — the domain draft and its GENERATED transcription onto `TransactionRequest` under `RequiredMappingStrategy.Both` (NodaTime/protobuf statics registered whole, the get-only `RepeatedField` Ops filled through one existing-target `[UserMapping]`, no whole-source reader ever).
- Cases: ComputeService, DocumentService, ControlService, DiagnosticService, ArtifactSyncService — the five services `compute.proto` declares and the whole of the `[02]` roster. `grpc.health.v1.Health` and `google.rpc.Status` are UPSTREAM standards this corpus never mints: their generated types ship in Grpc.HealthCheck and Grpc.AspNetCore.HealthChecks, the server binds them through `MapGrpcHealthChecksService` at `Rasm.AppHost/Observability/health#WIRE_HEALTH`, and `WireServices.Health` holds the package-shipped client. Declaring either here forks a contract its own publisher versions, and importing `grpc/health/v1/health.proto` seats a non-`rasm` package in the neutral corpus for a file no branch build reads from it — so the roster carries no Health row and the two names appear only as bound client surface.
- Law: `rasm/compute/v1/compute.proto` — the corpus-homed suite header — is the ONE mint of every fully-qualified suite wire name, and it is SINGLE-WRITER — `ContractGuard.Surface` keys every descriptor row on the package-qualified `FullName`, so `package rasm.compute.v1` is the token peers project their service spellings from (`python:geometry/mesh/serve` dials `rasm.compute.v1.ComputeService` and `rasm.compute.v1.ArtifactSyncService`, connect-es resolves the same names off the emitted descriptor set) and a peer transcribes the header rather than re-spelling it. One package declares one source and the directory spells the package: the sibling `rasm/channels/v1/channels.proto` mints the appearance family under `package rasm.channels.v1`, so the two carry two `FileDescriptorSet` snapshots under one gate law and neither claims the other's names, and a third family seats at `rasm/<family>/v1/` without re-deciding the layout. `option csharp_namespace` DERIVES from the package exactly as the directory does — `rasm.<family>.v1` stamps `Rasm.<Family>` — so this source stamps `Rasm.Compute`, the appearance source `Rasm.Channels` that `Rasm.Materials/Raster/set` binds decoding `Rasm.Channels.AssetSetManifest`, and a later family stamps its own without re-deciding. Inheriting a sibling's namespace forces globally-unique message names across every family and pushes the family name back into the messages; a wire type colliding with a domain type resolves at the MESSAGE name instead, the one coordinate python and typescript also read. `FaultDetail.code` is the sole transported fault identity; owner and case remain local derivations, and every peer keeps the remote code opaque instead of mirroring another branch's band ledger.
- Law: a NON-RPC payload message keeps the plain concept name — `FaultDetail`, `ArtifactFrame`, `GeometryPayload` — and mints a `Wire` suffix only to break a collision with a co-resident domain type, which is why the element family suffixes every message and this one suffixes none; a consumer registry transcribes whichever name the descriptor declares and re-spells nothing, so a suffix added for symmetry alone binds a schema no source mints.
- Law: a wire message spells `<Verb>Request` inbound and `<Verb>Response`/`<Verb>Reply`/`<Verb>Receipt` outbound, while the domain shape it transcribes keeps its own name — `TransactionDraft`→`TransactionRequest`, `Runtime/tiles#TWO_HOP_TESSELLATION` `CompanionRequest.Tessellate`→`TessellationRequest`, `Runtime/payload#RESIDENCY` `SplatScan`→`GaussianSplatScan`. Simple names recurring at a package this suite never references — `Rasm.Bim` `Exchange/tessellation#TESSELLATION_BRIDGE` carries its own `TessellationRequest` domain record — scope-qualify at the citation, since the strata forbid a reference either way.
- Law: `WireDocument` is the `DocumentService`↔`DocumentTransaction` parity owner — `ExecuteTransaction` carries the in-process `DocumentTransaction` verb set field-for-field through one budget-bounded, fault-classified, receipt-emitting forwarder, so the same canonical operation set, the same `TransactionReceipt`, and the same wire choreography produce the identical typed receipt whether the transaction runs through the in-process handler or across the channel; the dedup window equals the `DeadlineClass.HopTotal` allotment so the one retry owner's horizon gates the idempotency edge on both legs, the response mirrors the typed receipt through `WireDocument.Receipt`, and a non-exceptional in-band conflict decodes through `WireDocument.Conflict` reading the `TransactionReceipt.conflict=5` slot as `RemoteFault` evidence with no fabricated transport cause, parallel response DTO, or hand-rolled per-consumer projection.
- Law: `GaussianSplatScan` is GROUNDED here — the SPZ v4 and SOG v2 binary specifications are stable, versioned, MIT-published law, so C# owns the wire vocabulary without waiting on a consumer. It rides the ArtifactSyncService `ArtifactFrame` byte seam as a STANDALONE artifact (NEVER a `GeometryPayload` oneof case — the oneof carries point_cloud/mesh/voxel only), reassembles via `FrameEdge.Reassemble<GaussianSplatScan>` under the whole-artifact `XxHash128` identity gate, and admits to the Compute `Rasm.Compute/Runtime/payload#RESIDENCY` `SplatScan`, whose member set (`FormatKey`/`Positions`/`Scales`/`Rotations`/`Harmonics`/`HarmonicDegree`/`SplatCount`/`Alphas` — the harmonic band leading with its DC triple, the alphas column the sigmoid-activated opacity appended additive-only) byte-mirrors these fields through the GENERATED `SplatMapper` both ways — a column appended on one shape alone fails that build, the drift class the alphas roster once shipped; Python `realitycapture` SPZ/SOG decode, the `xxhash` cp315 wheel, and the render-side consumer leaf `Rasm.AppUi/Render/reality#SPLAT_SOURCE` stay named sibling-branch facts.
- Auto: Grpc.Tools compiles GrpcServices=Client at build with `PrivateAssets=all`, `Access=Internal` for package-internal generated types and `Access=Public` only where the contract crosses the package boundary; app roots compile the same files GrpcServices=Server and emit the descriptor set that feeds connect-es codegen and the manifest checksum. Producer descriptors regenerate `rasm.runtime._pb2.rasm.channels.v1.channels_pb2`; `SupportBundleRequest`, `SupportBundleReply`, and `DiagnosticService/CaptureBundle` then bind the Python `PROTO_VOCABULARY` request and response rows without a hand-maintained twin.
- Packages: Google.Protobuf, Grpc.Tools, Grpc.Core.Api (`CallInvoker`, `CallOptions`, `AsyncUnaryCall<T>`), Grpc.Net.Client, NodaTime, NodaTime.Serialization.Protobuf, Riok.Mapperly (`[Mapper]`, `[UseStaticMapper]`, `[UserMapping]`, `[NamedMapping]`, `[MapProperty(Use = …)]`), LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: one rpc row on an existing service or one numbered message field absorbs a new wire fact; the browser collaboration decomposition (server-stream down, unary chunked up) is designed-only growth of one rpc row per direction; zero new surface. Control verbs are that row at full size, landing in FIVE places in one motion — the `[02]` service-roster row, its one `[NN]-[VERB]` law line, its request message row where the verb carries a payload, one `rpc` line under `service ControlService` in `rasm/compute/v1/compute.proto`, and one `ControlServiceShape` method-shape entry — while the handler side stays `Rasm.AppHost/Wire/companion#CONTROL_SERVICE` folding onto an owner that already exists, so a verb costs this mint a row and the fold nothing.
- Boundary: temporal values cross as Timestamp and protobuf Duration through `ToTimestamp`/`ToProtobufDuration` outward and `ToInstant`/`ToNodaDuration` inward — BCL DateTime never sits between wire and rail; calendar-bearing capture and schedule facts cross as `Google.Type` commons through `ToDate`/`ToTimeOfDay`/`ToProtobufDayOfWeek` outward and `ToLocalDate`/`ToLocalTime`/`ToIsoDayOfWeek` inward, so a serialized date string never sits between wire and rail; FieldMask carries the read projection and the partial-update write leg through ONE `WireServices.Mask(params ReadOnlySpan<FieldRef>)` entrypoint whose `FieldRef` `[Union<int,string>]` ad-hoc union absorbs numbers, caller paths, and mixed literals in one spread — a `Number` resolves to its field-NAME path through `FieldMask.FromFieldNumbers<QueryResponse>` (never a free string path), a `Path` admits through the non-throwing `FieldMask.FromString` re-guarded by the load-bearing `FieldMask.IsValid(QueryResponse.Descriptor, mask)` gate, both halves `Union` and `Normalize` to canonical sorted-deduplicated form on `Fin<FieldMask>`, and the empty spread faults typed — so an unknown path or number faults at the edge rather than silently dropping or throwing past it — the same partial-read mask the web-fed Query feed consumes, never a per-tile request DTO or a second mask carrier; Any with TypeRegistry carries polymorphic artifact message envelopes through `WireServices.Unpack` over `Any.TryUnpack<T>` keyed by `Any.Is(descriptor)` projecting the typed fault, while outbound packing rides `Any.Pack` directly at the one staging site (`FrameEdge.Transaction`) — a rename-forward `Pack` wrapper is the deleted form; Empty carries signals; `JsonFormatter` and `JsonParser` with the same TypeRegistry are the dashboard edge over the identical generated messages — a parallel web DTO family is the deleted form; `ExecuteTransaction` defends its idempotency edge by `Clone` on the dedup-window receipt rather than mutating the cached message in place — a shared-mutable cached message is the deleted form; `OriginalNameAttribute` reconciles a proto field name to its diverged C# name at the descriptor surface so the contract-evolution key reads the proto name, never the generated identifier; the proto geometry family is the single binary wire geometry, with NetTopologySuite as the store boundary projection, GeoJSON as the JSON projection, and RhinoCommon as the host projection; ArtifactSyncService carries the wire leg only — sync state, diffing, and transfer manifests are store mechanics; the gaussian-splat scan crosses this ArtifactSyncService wire as a STANDALONE `GaussianSplatScan` artifact riding the generic `ArtifactFrame` bytes, never a `GeometryPayload` oneof case (that oneof carries point_cloud/mesh/voxel only) and admitted to the `Rasm.Compute/Runtime/payload#RESIDENCY` `SplatScan`; the `Solve`/`Generate` rpcs carry the numeric-lane decomposition and generative-run legs field-for-field with no second request shape, and the `GraphDiff`/`SubtreeFetch` rpcs carry the content-key delta wire shape only — the set-difference computation is `Rasm.Persistence/Version/ledger#CHANGEFEED` (the `TransferSet` closure-minus-held fold over the `Closure` descendant content-key manifest), so Compute owns the wire frame and Persistence's ledger owns the diff algebra.

```csharp signature
[Union<int, string>(T1Name = "Number", T2Name = "Path")]
public readonly partial struct FieldRef;

public sealed record WireServices(
    GrpcChannel Channel,
    ComputeService.ComputeServiceClient Compute,
    DocumentService.DocumentServiceClient Document,
    ControlService.ControlServiceClient Control,
    DiagnosticService.DiagnosticServiceClient Diagnostic,
    ArtifactSyncService.ArtifactSyncServiceClient Artifacts,
    Health.HealthClient Health) : IDisposable {
    // The ONE mint: every generated client on this record binds to the SAME intercepted invoker, so a service the
    // contract adds breaks HERE — at the owner that declares it — rather than at a dialing capsule that would
    // otherwise fill the roster positionally and could silently open a channel never carrying the new service.
    // Interception and channel construction stay the dialer's; this owner only refuses to be built inconsistently.
    public static WireServices Of(CallInvoker invoker, GrpcChannel channel) =>
        new(channel,
            new ComputeService.ComputeServiceClient(invoker),
            new DocumentService.DocumentServiceClient(invoker),
            new ControlService.ControlServiceClient(invoker),
            new DiagnosticService.DiagnosticServiceClient(invoker),
            new ArtifactSyncService.ArtifactSyncServiceClient(invoker),
            new Health.HealthClient(invoker));

    // The two halves of a mixed spread refuse INDEPENDENTLY — an unknown number tells the caller nothing about an
    // unknown path — so they accumulate through the applicative and a caller reads every bad reference at once.
    // The empty spread needs no gate of its own: it resolves to neither half and lands the same refusal, so the
    // fabricated empty mask that arm once returned (readable as "project nothing") has no spelling left.
    public static Fin<FieldMask> Mask(params ReadOnlySpan<FieldRef> fields) {
        Seq<FieldRef> refs = toSeq(fields.ToArray());
        return (Numbered(refs.Choose(static field => field.IsNumber ? Some(field.AsNumber) : None)),
                Pathed(refs.Choose(static field => field.IsPath ? Some(field.AsPath) : None)))
            .Apply(static (numbered, pathed) => Joined(numbered, pathed)).As().ToFin()
            .Bind(static joined => joined.ToFin(new ComputeFault.PayloadOverBounds("<empty-mask>")));
    }

    private static K<Validation<Error>, Option<FieldMask>> Numbered(Seq<int> numbers) =>
        numbers.IsEmpty
            ? Fin.Succ(Option<FieldMask>.None).ToValidation()
            : Op.Of().Catch(() => Fin.Succ(Some(FieldMask.FromFieldNumbers<QueryResponse>(numbers))))
                .MapFail(static error => (Error)new WireFault.Internal(WireBoundary.QueryFieldNumber, error))
                .ToValidation();

    private static K<Validation<Error>, Option<FieldMask>> Pathed(Seq<string> paths) =>
        (paths.IsEmpty
            ? Fin.Succ(Option<FieldMask>.None)
            : FieldMask.FromString(string.Join(',', paths)) is FieldMask mask && FieldMask.IsValid(QueryResponse.Descriptor, mask)
                ? Fin.Succ(Some(mask))
                : Fin.Fail<Option<FieldMask>>(new ComputeFault.PayloadOverBounds($"<query-path-unknown:{string.Join(',', paths)}>")))
            .ToValidation();

    private static Option<FieldMask> Joined(Option<FieldMask> numbered, Option<FieldMask> pathed) =>
        numbered.Match(
            Some: resolved => pathed.Match(
                Some: guarded => Some(resolved.Union(guarded).Normalize()),
                None: () => Some(resolved.Normalize())),
            None: () => pathed.Map(static guarded => guarded.Normalize()));

    public static Fin<T> Unpack<T>(Any envelope) where T : class, IMessage<T>, new() =>
        envelope.TryUnpack<T>(out T artifact)
            ? Fin.Succ(artifact)
            : Fin.Fail<T>(new ComputeFault.PayloadOverBounds($"<any-envelope-mismatch:{Any.GetTypeName(envelope.TypeUrl)}:{new T().Descriptor.FullName}>"));

    public void Dispose() => Channel.Dispose();
}

public static class WireDocument {
    public static IO<Fin<TransactionReceipt>> ExecuteTransaction(WireServices services, CallSpine spine, AdmittedIntent intent, TransactionRequest request, CancellationToken token) =>
        Dialed(spine, intent, request, services.Document.ExecuteTransactionAsync, token)
            .Map(result => result.Bind(receipt => Receipt(receipt, request.IdempotencyKey)));

    public static IO<Fin<QueryResponse>> Query(WireServices services, CallSpine spine, AdmittedIntent intent, QueryRequest request, FieldMask projection, CancellationToken token) =>
        Dialed(spine, intent, Projected(request, projection), services.Document.QueryAsync, token);

    // ONE dial rail for every unary document verb — bound, call, await, lift — where the two verbs each spelled the
    // whole bound-and-lift shape and a third would spell it again. The RESPONSE TYPE is the verb's discriminant and
    // stays a type parameter: a `[Union]` over the verbs would collapse both answers onto `IMessage` and erase the
    // field-for-field parity this seam exists to prove.
    private static IO<Fin<TResponse>> Dialed<TRequest, TResponse>(
        CallSpine spine,
        AdmittedIntent intent,
        TRequest request,
        Func<TRequest, CallOptions, AsyncUnaryCall<TResponse>> dial,
        CancellationToken token)
        where TRequest : IMessage<TRequest>
        where TResponse : IMessage<TResponse> =>
        CallSpine.Bounded(request).Match(
            Succ: bounded => CallSpine.Awaited(
                () => dial(bounded, spine.Options(intent, token)).ResponseAsync,
                token),
            Fail: static error => IO.pure(Fin.Fail<TResponse>(error)));

    // Exemption: statement body — a generated message is mutable-by-construction and the protobuf runtime publishes
    // no with-expression, so the projection lands by assignment on a defensive clone rather than on the caller's.
    private static QueryRequest Projected(QueryRequest request, FieldMask projection) {
        QueryRequest projected = request.Clone();
        projected.Mask = projection;
        return projected;
    }

    public static Fin<TransactionReceipt> Receipt(TransactionReceipt wire, ByteString idempotencyKey) =>
        wire.IdempotencyKey == idempotencyKey
            ? Fin.Succ(wire.Clone())
            : Fin.Fail<TransactionReceipt>(new WireFault.Conflict(idempotencyKey, wire.IdempotencyKey));

    public static Fin<Option<RemoteFault>> Conflict(TransactionReceipt receipt) =>
        (receipt.Committed, receipt.Conflict) switch {
            (true, null) => Fin.Succ(Option<RemoteFault>.None),
            (false, { } detail) => WireFault.DecodeConflict(detail).Map(static evidence => Some(evidence)),
            _ => Fin.Fail<Option<RemoteFault>>(new WireFault.InvalidRequest(
                new WireViolation.ReceiptDisposition(receipt.Committed, receipt.Conflict is not null))),
        };
}

public sealed record TransactionDraft(
    ByteString IdempotencyKey, Seq<Any> Ops, ulong ExpectedEpoch, Instant HlcPhysical, ulong HlcLogical, CorrelationId Correlation);

// Parity-seam boundary transcription is GENERATED under RequiredMappingStrategy.Both, so a request column added
// at either end FAILS THE BUILD instead of crossing as a silent default. NO whole-source
// [MapPropertyFromSource] reader ever lands here — one reader suppresses RMG020 for every source member of its
// mapping. Temporal members cross through the registered NodaTime/protobuf static family; the get-only
// RepeatedField Ops fills through the one existing-target [UserMapping]; verb-case DISPATCH stays the domain
// union's own generated Switch — [MapDerivedType] serves only a shared-base oneof envelope, never a
// Thinktecture [Union] re-spelled.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(NodaExtensions))]
[UseStaticMapper(typeof(ProtobufExtensions))]
public static partial class WireDocumentMapper {
    [MapProperty(nameof(TransactionDraft.Correlation), nameof(TransactionRequest.Correlation), Use = nameof(CorrelationText))]
    public static partial TransactionRequest ToWire(TransactionDraft draft);

    // Get-only RepeatedField fill: AddRange copies the already-packed envelopes — the one sanctioned shape for
    // a member the generated constructor path cannot assign.
    [UserMapping]
    private static void Ops(Seq<Any> source, RepeatedField<Any> target) => target.AddRange(source);

    [NamedMapping("correlation-text")]
    private static string CorrelationText(CorrelationId correlation) => correlation.ToString();
}

// This mapper GENERATES the splat frame's byte-mirror crossing both ways under Both, so a column appended on one shape
// alone — exactly the alphas drift the roster once shipped — fails THIS build instead of surviving to the
// SplatShapeValid admission gate; the codec pair owns the packed float↔ByteString lift, each direction copying
// exactly once into an owned buffer (`UnsafeWrap` then avoids the second copy ByteString.CopyFrom would take).
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(SplatCodec))]
public static partial class SplatMapper {
    public static partial SplatScan ToDomain(GaussianSplatScan wire);

    public static partial GaussianSplatScan ToWire(SplatScan scan);
}

public static class SplatCodec {
    public static ReadOnlyMemory<float> Planes(ByteString packed) =>
        MemoryMarshal.Cast<byte, float>(packed.Span).ToArray();

    public static ByteString Packed(ReadOnlyMemory<float> planes) =>
        UnsafeByteOperations.UnsafeWrap(MemoryMarshal.AsBytes(planes.Span).ToArray());
}
```

```proto signature
// Header of the corpus-homed suite contract file `rasm/compute/v1/compute.proto`.
syntax = "proto3";

package rasm.compute.v1;

option csharp_namespace = "Rasm.Compute";

import "google/protobuf/duration.proto";
import "google/protobuf/empty.proto";
import "google/protobuf/timestamp.proto";

message FaultRecovery {
  oneof kind {
    google.protobuf.Empty terminal = 1;
    google.protobuf.Empty transient = 2;
    google.protobuf.Duration retry_after = 3;
  }
}

message FaultDetail {
  int32 code = 1;
  string message = 2;
  string correlation = 3;
  google.protobuf.Timestamp hlc_physical = 4;
  uint64 hlc_logical = 5;
  string tenant = 6;
  FaultRecovery recovery = 7;
}
```

Each row reads `minted` when `compute.proto` declares the rpc and both its messages, and `unminted` when this roster has settled the verb but a message field set has no owner yet. `unminted` marks a DECLARED GAP, never a missing declaration: the proto follows this roster, so a reader sees a correct source beside the field set it still owes.

| [INDEX] | [SERVICE]           | [RPC]              | [SHAPE]       | [MESSAGES]                                | [STATE]  |
| :-----: | :------------------ | :----------------- | :------------ | :---------------------------------------- | :------- |
|  [01]   | ComputeService      | Infer              | unary         | InferRequest → InferResponse              | minted   |
|  [02]   | ComputeService      | Solve              | unary         | SolveRequest → SolveResponse              | minted   |
|  [03]   | ComputeService      | Generate           | server-stream | GenerateRequest → TokenChunk              | minted   |
|  [04]   | ComputeService      | GraphDiff          | unary         | GraphDiffRequest → GraphDiffResponse      | minted   |
|  [05]   | ComputeService      | SubtreeFetch       | server-stream | SubtreeFetchRequest → GraphChunk          | minted   |
|  [06]   | ComputeService      | Tessellate         | unary         | TessellationRequest → TessellationReceipt | minted   |
|  [07]   | DocumentService     | ExecuteTransaction | unary         | TransactionRequest → TransactionReceipt   | minted   |
|  [08]   | DocumentService     | Query              | unary         | QueryRequest → QueryResponse              | minted   |
|  [09]   | ControlService      | ReloadOptions      | unary         | Empty → ReloadReply                       | minted   |
|  [10]   | ControlService      | DispatchTool       | unary         | DispatchToolRequest → CommandReply        | minted   |
|  [11]   | ControlService      | DispatchPatch      | unary         | DispatchPatchRequest → ReloadReply        | minted   |
|  [12]   | ControlService      | SetDegradation     | unary         | SetDegradationRequest → DegradationReply  | minted   |
|  [13]   | ControlService      | DrainRuntime       | unary         | DrainRuntimeRequest → DrainReply          | minted   |
|  [14]   | DiagnosticService   | CaptureBundle      | unary         | SupportBundleRequest → SupportBundleReply | minted   |
|  [15]   | ArtifactSyncService | Sync               | bidi          | ArtifactFrame → ArtifactFrame             | minted   |
|  [16]   | ComputeService      | Progress           | server-stream | ProgressRequest → ProgressUpdate          | unminted |
|  [17]   | ComputeService      | Capabilities       | unary         | Empty → ComputeCapabilities               | unminted |
|  [18]   | DocumentService     | Capabilities       | unary         | Empty → DocumentCapabilities              | unminted |
|  [19]   | DocumentService     | DocumentEvents     | server-stream | WatchRequest → DocumentEvent              | unminted |
|  [20]   | DocumentService     | CaptureEvents      | client-stream | CaptureFrame → CaptureSummary             | unminted |
|  [21]   | ControlService      | CaptureSupport     | unary         | Empty → CaptureSupportReply               | unminted |

Each rpc carries one wire law; an unminted row names the field set that has no owner:

- [01]-[INFER]: payload caps pre-checked at the call edge; faults ride FaultDetail
- [02]-[SOLVE]: carries the numeric-lane dense or sparse decomposition field-for-field; faults ride FaultDetail; the row-block shard sub-solve dials this rpc
- [03]-[GENERATE]: remote token streaming rides a correlation-keyed server stream; faults ride FaultDetail. DECLARED NARROWING: the frozen six-field `GenerateRequest` crosses the remote-generable subset of `GenerationPolicy` ALONE (checksum, prompt, max-length, the guidance pair, tools) — `InMemory`, `AdapterPaths`, and `Decoder` are structurally unwireable local handles, and the session-local columns (search rows, stop sequences, template, history, retrieved context, media reserve) stay the local lane's until a remote-chat contract mints WITH its `tests/contracts` row; the narrowing is this seam's declared refusal, never drift, and an additive field lands as one frozen-numbered proto field beside its policy column in one change
- [04]-[GRAPHDIFF]: content-key delta over two Closure hashes; the set-difference algebra is `Rasm.Persistence/Version/ledger#CHANGEFEED` (`TransferSet`/`Closure`), this carries the wire shape only
- [05]-[SUBTREEFETCH]: partial-graph checkout streaming the content-addressed subtree the GraphDiff added-set names
- [06]-[TESSELLATE]: bridges tessellation to the companion over two hops — `Runtime/tiles#TWO_HOP_TESSELLATION` owns the `CompanionRequest.Tessellate` build and the receipt admission, the python geometry serve end implements the rpc against the same two messages and projects its fully-qualified name from this mint, and faults ride FaultDetail
- [07]-[EXECUTETRANSACTION]: flagship parity: idempotency key; server dedup window equals the DeadlineClass.HopTotal allotment — the one retry owner's horizon; the forwarder folds Bounded+budget+Classify+receipt; the receipt mirrors the DocumentTransaction typed receipt field-for-field through `WireDocument.Receipt` and the in-band conflict decodes through `WireDocument.Conflict`
- [08]-[QUERY]: read verb with FieldMask projection via `WireServices.Mask`
- [09]-[RELOADOPTIONS]: projects the ReloadReceipt
- [10]-[DISPATCHTOOL]: routes an agent tool call onto the `Rasm.AppHost/Agent/runtime#COMMAND_DISPATCH` front door behind the redaction-and-audit seam, arguments crossing as `Struct` under the additive-only open message-envelope contract; `CommandReply` projects `Rasm.AppHost/Agent/capability#COMMAND_ALGEBRA` `CommandReceipt` field-for-field, the `CommandTxn` union expanded onto a case-key column beside its arm payloads, while the capability pin's `output` schema serves discovery and binds no carriage
- [11]-[DISPATCHPATCH]: carries an RFC-6902 section patch onto the one `ReloadOutcome` transition under `ReloadReceipt.PatchTrigger`; the reply projects `ReloadReceipt` field-for-field, the same message the reload-options verb answers
- [12]-[SETDEGRADATION]: `SetDegradationRequest` lands its level key on the one override rail as the `Rasm.AppHost/Runtime/config#KILL_SWITCH` `ForceLevel` arm, and the reply answers the resolved `DegradationLevel` key the caller re-admits; richer degradation evidence rides its own JSON wire at the health owner, so a column lands here when a caller reads it
- [13]-[DRAINRUNTIME]: commits the drain phase and folds onto `Rasm.AppHost/Runtime/lifecycle#DRAIN_CONDUCTOR`, re-implementing nothing it holds; the request carries the parent's REMAINING cooperative allotment and the handler takes `min(inherited, DeadlineClass.DrainCooperative)`, since allotments inherit through nested seams as the minimum
- [14]-[CAPTUREBUNDLE]: requested collector keys select the bounded diagnostic capture; an empty set selects every admitted collector, and the reply carries the archive content key, bytes, collected keys, and skipped keys
- [15]-[SYNC]: frame law below; FieldMask partials; Any artifact message envelopes
- [16]-[PROGRESS]: UNMINTED — `ProgressUpdate` is grounded (`Runtime/progress#PROGRESS_CELL` `ProgressMark` transcribes phase key, rank, fraction, segments, instant, correlation under `RequiredMappingStrategy.Both`), but no owner declares what a REMOTE subscriber sends: `ProgressCell.Mint` reads its cadence off the admitted intent in process, so `ProgressRequest` has no field set to transcribe
- [17]-[CAPABILITIES]: UNMINTED — no owner enumerates `ComputeCapabilities`; the substrate rows at `Runtime/admission#DISPATCH_SPINE` and the EP rows at `Model/providers#EP_AXIS` are selection vocabularies, and the `tests/contracts/MANIFEST.md` `[02.12]` capability descriptor is the AppHost command axis, not a Compute inventory reply
- [18]-[CAPABILITIES]: UNMINTED — no owner enumerates `DocumentCapabilities`; the verb inventory and document scope exist as no declared shape
- [19]-[DOCUMENTEVENTS]: UNMINTED — no owner declares `WatchRequest` or `DocumentEvent`; the nearest live-data fact is the in-process `Rasm.AppUi/Editing/livedata#DATA_SOURCES` `HostDocumentFact`, which a host callback feeds rather than a stream, and the store changefeed carries `OpLogEntry` for a different purpose
- [20]-[CAPTUREEVENTS]: UNMINTED — no owner declares `CaptureFrame` or `CaptureSummary` for this seam; the two records carrying that spelling are a Grasshopper paint-audit frame and a reality-capture playback epoch, neither a wire shape and neither carrying the per-frame HLC idempotency key this verb states
- [21]-[CAPTURESUPPORT]: UNMINTED BY CARRIAGE — the field set IS grounded as `Rasm.AppHost/Observability/bundles#MANIFEST_RECEIPT` `SupportCaptureWire`, and `tests/contracts/MANIFEST.md` `[02.21]-[APPHOST_WIRE]` rules that family's carriage JSON and owes it no descriptor source, so the reply mints here only if that ruling moves; the proto-carried diagnostic capture is `[14]-[CAPTUREBUNDLE]`

Each message carries its proto field set and wire role:

- [01]-[GEOMETRYPAYLOAD]: `oneof kind: point_cloud=1, mesh=2, voxel=3; symbolic_dims=4 repeated` — message envelope for Infer payloads and artifacts
- [02]-[POINTCLOUDTENSOR]: `count=1 int64; channels=2 int32; dtype=3 string; data=4 bytes` — point-cloud N×C encoding row
- [03]-[MESHTENSOR]: `vertex_count=1 int64; vertices=2 bytes; face_count=3 int64; faces=4 bytes` — mesh vertex N×3 and face F×3 rows
- [04]-[VOXELTENSOR]: `dims=1 repeated int64; dtype=2 string; data=3 bytes` — voxel NCHW row
- [05]-[SYMBOLICDIM]: `name=1 string; bound=2 int64` — symbolic-dim binding row
- [06]-[TRANSACTIONREQUEST]: `idempotency_key=1 bytes; ops=2 repeated Any; expected_epoch=3 uint64; hlc_physical=4 google.protobuf.Timestamp; hlc_logical=5 uint64; correlation=6 string` — flagship: the in-process `DocumentTransaction` verb set field-for-field, ops as polymorphic Any message envelopes
- [07]-[TRANSACTIONRECEIPT]: `idempotency_key=1 bytes; committed=2 bool; new_epoch=3 uint64; applied=4 repeated string; conflict=5 FaultDetail; hlc_physical=6 google.protobuf.Timestamp; hlc_logical=7 uint64` — flagship: mirrors the `DocumentTransaction` typed receipt field-for-field; the `conflict` slot carries the FaultDetail the retry owner decodes through `WireDocument.Conflict` in band
- [08]-[QUERYREQUEST]: `scope=1 string; predicate=2 Struct; cursor=3 string; mask=4 google.protobuf.FieldMask` — read verb carrying the field-mask projection
- [09]-[QUERYRESPONSE]: `rows=1 repeated Struct; cursor=2 string; total=3 int64` — masked read result; the mask names the projected columns
- [10]-[SOLVEREQUEST]: `matrix=1 bytes; rhs=2 bytes; factorization_kind=3 string; sparse_format=4 string; shard_tile=5 int32` — numeric-lane decomposition request: matrix + rhs are column-major float64 bytes (a dense solver operand is NOT a point_cloud/mesh/voxel GeometryPayload — no geometry message envelope), server reshapes from byte length + shard_tile
- [11]-[SOLVERESPONSE]: `solution=1 bytes; provider=2 string; decomposition=3 string; rows=4 int64; cols=5 int64; nnz=6 int64` — numeric-lane solve result + Factorization-receipt evidence
- [12]-[GENERATEREQUEST]: `model_checksum=1 string; prompt=2 string; max_length=3 double; guidance_kind=4 string; guidance_data=5 string; tools=6 string` — generative-run request mirroring GenerationPolicy
- [13]-[TOKENCHUNK]: `piece=1 string; token_index=2 int64; done=3 bool` — one decoded token piece per server-stream frame
- [14]-[GRAPHDIFFREQUEST]: `base_hash=1 string; target_hash=2 string` — content-key delta over two Closure hashes
- [15]-[GRAPHDIFFRESPONSE]: `added=1 repeated string; removed=2 repeated string` — added/removed content-key set
- [16]-[SUBTREEFETCHREQUEST]: `content_keys=1 repeated string` — partial-graph checkout request
- [17]-[GRAPHCHUNK]: `content_key=1 string; payload=2 bytes; ordinal=3 int64` — one content-addressed subtree node per frame
- [18]-[SUPPORTBUNDLEREQUEST]: `collectors=1 repeated string` — requested diagnostic collector keys; empty selects every admitted collector
- [19]-[SUPPORTBUNDLEREPLY]: `content_key=1 string; archive=2 bytes; collected=3 repeated string; skipped=4 repeated string` — content-addressed archive and the realized collector partition
- [20]-[GAUSSIANSPLATSCAN]: `format_key=1 string; positions=2 bytes; scales=3 bytes; rotations=4 bytes; harmonics=5 bytes; harmonic_degree=6 int32; splat_count=7 int64; alphas=8 bytes` — reality-capture splat frame; packed buffers carry the spec accessor order, `harmonic_degree` the SH band 0–3, `splat_count` the per-buffer element count, `alphas` the sigmoid-activated opacity column the `SplatScan` mirror gates on (`SplatShapeValid`), `format_key` the `spz-v4`/`sog-v2` source discriminant
- [21]-[FAULTDETAIL]: `code=1 int32; message=2 string; correlation=3 string; hlc_physical=4 google.protobuf.Timestamp; hlc_logical=5 uint64; tenant=6 string; recovery=7 FaultRecovery` — numeric identity and typed recovery only; owner and case remain local derivations and no generic source-union envelope crosses.
- [22]-[TESSELLATIONREQUEST]: `source_modality=1 string; source=2 bytes; policy=3 map<string,string>` — the companion tessellation job on the wire: the modality key selecting the ifc or cad arm, the source bytes, and the deflection/tolerance policy the `TessellationPolicy` columns render as text
- [23]-[TESSELLATIONRECEIPT]: `content_key=1 string; element_count=2 int64; triangle_count=3 int64; semantic_header=4 google.protobuf.Struct; artifact_hash=5 string; replay_phase=6 string` — the companion's evidence floor; the GLB body itself rides the ArtifactSyncService `ArtifactFrame` byte seam, so this carries coordinates, counts, and keys alone
- [24]-[DISPATCHTOOLREQUEST]: `tool=1 string; arguments=2 google.protobuf.Struct` — the requested tool key beside the open message envelope its arguments cross, admitted within the additive-only contract
- [25]-[DISPATCHPATCHREQUEST]: `section=1 string; patch=2 google.protobuf.Struct` — the config section and its RFC-6902 patch document
- [26]-[RELOADREPLY]: `section=1 string; reload_class=2 string; trigger=3 string; outcome=4 string; at=5 google.protobuf.Timestamp; correlation=6 string` — projects the `ReloadReceipt` field-for-field, answering both the reload-options and dispatch-patch verbs so one transition carries one reply shape
- [27]-[DRAINRUNTIMEREQUEST]: `cooperative=1 google.protobuf.Duration; reason=2 string` — the caller's REMAINING cooperative allotment beside the reason it drains, since the handler takes the minimum of the two budgets rather than the one it declares
- [28]-[DRAINREPLY]: `steps=1 repeated DrainStepRow; final_phase=2 string; at=3 google.protobuf.Timestamp; elapsed=4 google.protobuf.Duration; correlation_id=5 string` — projects `DrainReceipt` field-for-field, so a straggling step is evidence rather than a timeout the caller infers
- [29]-[DRAINSTEPROW]: `name=1 string; band=2 int32; allotted=3 google.protobuf.Duration; consumed=4 google.protobuf.Duration; outcome=5 string` — projects `DrainStep`, the band riding its `[SmartEnum<int>]` value and the outcome its `flushed`/`escalated`/`straggled` token
- [30]-[DISPATCHRECEIPT]: `executor=1 string; selection=2 string; elapsed=3 google.protobuf.Duration` — projects `DispatchReceipt`, the dispatch evidence the command spine decodes at its seam, so no executing stratum's own receipt type reaches the message envelope
- [31]-[COMMANDREPLY]: `descriptor=1 string; txn=2 string; reason=3 string; refusal=4 FaultDetail; compensation=5 DispatchReceipt; charged=6 map<string,int64>; elapsed=7 google.protobuf.Duration; correlation=8 string; tenant=9 string; tenant_slug=10 string; at=11 google.protobuf.Timestamp; dispatch=12 DispatchReceipt` — projects `CommandReceipt` field-for-field, `charged` rendering the `MeterVector` map under its `CostUnit` keys and tenancy crossing as the `TenantId.Wire` text beside its slug
- [32]-[SETDEGRADATIONREQUEST]: `level=1 string; reason=2 string` — the `DegradationLevel` key text the admission validates beside the reason it is forced, landing as the `KillSwitchConfig` `ForceLevel` arm rather than a second override vocabulary
- [33]-[DEGRADATIONREPLY]: `level=1 string` — the resolved `DegradationLevel` key the caller re-admits, the one column a call site reads; a derived, forced, cascade, streak, or floor column lands when a caller reads it, never because the health owner's own JSON wire carries it

## [03]-[CONTRACT_EVOLUTION]

- Owner: `ContractDrift` COMPOSED from its `Rasm.AppHost/Wire/outbound#TS_PROJECTION` owner (the three-way verdict `Identical | Additive(Added) | Breaking(Removed)` this page's classifier was always DESIGNED to return — AppHost `outbound.md` takes `Func<string, string, Fin<ContractDrift>> classify` and `ContractGuard.Classify` is that classifier; the local re-declaration was the twin); `ContractGuard` — descriptor surface fold over field/rpc/oneof/enum/packed shape recursing nested message and enum types, classifier delegate, descriptor publication path, proto-name reconciliation, field-mask read guard, and `Digest` the ONE hex-`XxHash128` fold both this checksum and the stage roster's read; `ParseGuard` — inbound parse-hardening policy record carrying the size-gated buffer decode, the proto2 `ExtensionRegistry`, and the dynamic open message-envelope admission; `StageCrossing` — the Compute-side reciprocal mirror of the branch-interior photo-to-PBR slot roster, folding through the same canonical projection; `ContractSurface` the typed `[SetEquality]` surface carrier whose ONE generated Inequalities walk classifies drift (Added/Removed sentinels read through the factory `.Kind` projections, the checksum staying the XxHash128 fold); `StageRoster` the typed `[OrderedEquality]` roster carrier two generations diff by slot.
- Cases: Identical, Additive (tolerated), Breaking (typed rejection carrying the missing or retyped surface rows).
- Entry: `AdditiveOnly(Seq<ByteString> local, Func<string, Fin<Seq<ByteString>>> peerSetOf)` — the delegate `Discovery.Compatible` consumes; checksum equality or additive drift admits, breaking drift rejects on the hop fault rail. `StageCrossing.Probe()` proves both stage rosters against their own record arities at boot and `StageCrossing.Checksum(roster)` folds one roster to the digest the relaying root compares against the producing end's.
- Law: the photo-to-PBR stage crossing is NOT a Compute wire and mints no codec here — `Rasm.Materials` SPECIFIES the request, `Model/stage#STAGE_FOLD` EXECUTES it, the branch strata forbid a project reference either way, so the app root relays the bytes and transcribes them into the lowered-primitive `StageRequest`/`StageResult` records Compute declares independently. What Compute OWES is the reciprocal mirror of the frozen slot roster — the same both-ends-checkable discipline the `[04]-[FAULT_PROJECTION]` generated numeric identity takes — so an appended column at one end alone fails a boot probe rather than misdecoding at its slot, and a relaying root refuses to transcribe a byte across a checksum disagreement.
- Packages: Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, Generator.Equals (`[Equatable]`, `[SetEquality]`, `[OrderedEquality]`, `Inequalities` + the `MemberPathSegment.Added`/`Removed` sentinels), Rasm.AppHost (project), BCL inbox
- Growth: one surface-projection row absorbs a new descriptor dimension — packed-encoding flip, nested-type retype, oneof-membership change; the host↔companion capability negotiation and per-node EP-option bag ride the `Struct`/`Value`/`ListValue` open message-envelope column under the same additive-only contract — open within an additive-only contract, never a drift escape hatch; a new stage column is one `StageCrossing` slot row landing in the same change as its `Model/inference` record column, which the arity probe then forces; zero new surface.
- Boundary: contract identity is the serialized descriptor set built through `FileDescriptor.BuildFromByteStrings` at startup; an empty descriptor set faults before hashing. `StageCrossing` carries slot ordinals and wire field names ALONE in its digest — the half the producing end reproduces from its own `[Key(n)]` roster — while the Compute column each slot lands on stays a `nameof` binding this side proves and never transmits, so a rename here breaks a build and never a peer. `ContractGuard.Checksum` folds the ordered `Surface(...)` set into one UTF-8 stream and applies `XxHash128.Hash`, so semantically identical generators agree without hashing unstable raw `SerializedData`. `AdditiveOnly` admits checksum equality and otherwise classifies the descriptor diff; checksum mismatch never admits alone. `Surface` recurses messages and enums and projects message and service declarations, field number, type, cardinality, map shape, packing, oneof membership, JSON name, enum values, and RPC input, output, and streaming shape, so adding an empty declaration still changes identity. `UnknownFieldSet` retention stays at the generated-parser default. `ParseGuard.Read` checks `ReadOnlySequence<byte>.Length` before `MessageParser<T>.ParseFrom(ReadOnlySequence<byte>)`, and `ExtensionRegistry` resolves declared proto2 extensions at the same boundary.

```csharp signature
public sealed record ParseGuard(int SizeLimitBytes, ExtensionRegistry Extensions) {
    public static readonly ParseGuard Canonical = new(
        SizeLimitBytes: GrpcChannelPolicy.Canonical.MaxReceiveBytes,
        Extensions: new ExtensionRegistry());

    public Fin<T> Read<T>(MessageParser<T> parser, ReadOnlySequence<byte> payload) where T : IBufferMessage, IMessage<T> =>
        payload.Length > SizeLimitBytes
            ? new ComputeFault.PayloadOverBounds($"<inbound-over-receive-bound:{payload.Length}:{SizeLimitBytes}>")
            : Op.Of().Catch(() => Fin.Succ(parser.WithExtensionRegistry(Extensions).ParseFrom(payload)))
                .MapFail(static error => (Error)new WireFault.Internal(WireBoundary.InboundPayload, error));

    // Initializer, not a fold: the previous shape mutated one shared `Struct` inside a `Fold` and returned it, which
    // wears the fold's signature while holding none of its law.
    public static Struct Envelope(HashMap<string, Value> options) =>
        new() { Fields = { options.ToDictionary(static entry => entry.Key, static entry => entry.Value) } };
}

public static class ContractGuard {
    // ONE digest fold for every projection this package compares across a seam — descriptor surface here, stage
    // roster below — because two hand copies of `hex(XxHash128(utf8(join(';', ordered))))` are two chances for a
    // preimage to drift while both ends still call the result a checksum.
    public static string Digest(Seq<string> ordered) =>
        Convert.ToHexStringLower(XxHash128.Hash(Encoding.UTF8.GetBytes(string.Join(';', ordered))));

    public static Fin<string> Checksum(Seq<ByteString> serialized) =>
        Build(serialized).Map(static files => Digest(toSeq(Surface(files).OrderBy(static row => row, StringComparer.Ordinal))));

    public static Fin<Seq<FileDescriptor>> Build(Seq<ByteString> serialized) =>
        serialized.IsEmpty
            ? Fin.Fail<Seq<FileDescriptor>>(new WireFault.DescriptorRejected())
            : Op.Of().Catch(() => Fin.Succ(toSeq(FileDescriptor.BuildFromByteStrings(serialized))))
                .MapFail(static error => (Error)new WireFault.Internal(WireBoundary.DescriptorSet, error));

    public static ContractDrift Classify(Seq<FileDescriptor> local, Seq<FileDescriptor> peer) =>
        ContractSurface.Of(local).Classify(ContractSurface.Of(peer));

    public static Func<string, string, Fin<bool>> AdditiveOnly(Seq<ByteString> local, Func<string, Fin<Seq<ByteString>>> peerSetOf) =>
        (localChecksum, peerChecksum) =>
            Checksum(local).Bind(digest => digest == localChecksum && peerChecksum == localChecksum
                ? Fin.Succ(true)
                : from peerBytes in peerSetOf(peerChecksum)
                  from peerFiles in Build(peerBytes)
                  from localFiles in Build(local)
                  select Classify(localFiles, peerFiles) is not ContractDrift.Breaking);

    internal static FrozenSet<string> Surface(Seq<FileDescriptor> files) =>
        files.Bind(static file => toSeq(file.MessageTypes).Bind(MessageSurface)
                .Concat(toSeq(file.EnumTypes).Map(EnumSurface))
                .Concat(RpcSurface(file)))
            .ToFrozenSet(StringComparer.Ordinal);

    private static Seq<string> MessageSurface(MessageDescriptor message) =>
        Seq($"{message.FullName}:message")
            .Concat(toSeq(message.Fields.InDeclarationOrder())
            .Map(field => $"{message.FullName}.{field.Name}={field.FieldNumber}:{field.FieldType}:{(field.IsRepeated ? "R" : "S")}:{(field.IsMap ? "M" : "-")}:{(field.IsPacked ? "P" : "-")}:{field.ContainingOneof?.Name ?? "-"}:{field.JsonName}")
            .Concat(toSeq(message.Oneofs).Map(oneof => $"{message.FullName}~{oneof.Name}=[{string.Join(',', oneof.Fields.OrderBy(static f => f.FieldNumber).Select(static f => f.FieldNumber))}]"))
            .Concat(toSeq(message.NestedTypes).Bind(MessageSurface))
            .Concat(toSeq(message.EnumTypes).Map(EnumSurface)));

    private static string EnumSurface(EnumDescriptor enumeration) =>
        $"{enumeration.FullName}=[{string.Join(',', enumeration.Values.OrderBy(static v => v.Number).Select(static v => $"{v.Name}:{v.Number}"))}]";

    private static Seq<string> RpcSurface(FileDescriptor file) =>
        toSeq(file.Services).Bind(static service => Seq($"{service.FullName}:service")
            .Concat(toSeq(service.Methods).Map(method => $"{service.FullName}/{method.Name}:{method.InputType.FullName}->{method.OutputType.FullName}:{(method.IsClientStreaming ? "C" : "U")}{(method.IsServerStreaming ? "S" : "U")}")));
}

// Typed surface carrier: `[SetEquality]` over the canonical row set turns drift classification into ONE
// generated Inequalities walk — the Added/Removed membership sentinels partition the diff, read through the
// factory `.Kind` projections so a sentinel-case rename never diverges a transcription — replacing the
// stringified FrozenSet Except pair that walked the sets twice. The projection CHECKSUM stays the XxHash128
// fold over the ordered renders: content addressing never rides GetHashCode, whose set-member hash is a
// constant zero by the generator's own law.
[Equatable]
public sealed partial record ContractSurface([property: SetEquality] FrozenSet<string> Rows) {
    public static ContractSurface Of(Seq<FileDescriptor> files) => new(ContractGuard.Surface(files));

    // Locally-present, peer-absent rows are the BREAKING half and peer-only rows are additive, so one walk
    // partitions both halves and the verdict reads them in that order — a missing row outranks any gain beside it.
    // Each side matches its sentinel kind AND its string payload in one pattern, so the diff's `object?` columns
    // never need a null-forgiving cast to answer a question the pattern already asked.
    public ContractDrift Classify(ContractSurface peer) =>
        toSeq(EqualityComparer.Default.Inequalities(this, peer)) is { IsEmpty: false } diff
            ? Parted(diff, MemberPathSegment.Removed().Kind, static row => row.Left) is { IsEmpty: false } missing
                ? new ContractDrift.Breaking(missing)
                : new ContractDrift.Additive(Parted(diff, MemberPathSegment.Added().Kind, static row => row.Right))
            : new ContractDrift.Identical();

    private static Seq<string> Parted(Seq<Inequality> diff, MemberPathSegmentKind kind, Func<Inequality, object?> side) =>
        diff.Choose(row => row.Path.Segments[^1].Kind == kind && side(row) is string render ? Some(render) : None);
}

// Compute's end of the branch-interior photo-to-PBR crossing. No codec lands here: the specifying package owns the
// positional roster, the app root relays and transcribes the bytes, and Compute receives already-typed records
// whose every column is a lowered primitive. Two independently declared rosters with no correspondence is what
// this owner deletes — each row pins one frozen slot ordinal beside the `Model/stage#STAGE_WIRE` column it
// lands on through `nameof`, so a rename breaks a build, an appended column with no counterpart breaks `Probe`,
// and the relaying root compares one digest per direction before it moves a byte. Mirroring the peer's
// `[Key(n)]` annotations, its serializer, or its vocabularies here would re-mint the rosters that ruling forecloses.
// Typed roster carrier: `[Equatable]` with the ordered row column makes two roster GENERATIONS diff through the
// generated Inequalities — the moved slot is NAMED by its index — where a checksum mismatch alone localizes
// nothing; the digest below stays the wire identity.
[Equatable]
public sealed partial record StageRoster([property: OrderedEquality] Seq<(int Slot, string Wire, string Column)> Rows);

public static class StageCrossing {
    public const string RequestWire = "rasm.materials.stage-request.v1";
    public const string ResultWire = "rasm.materials.stage-result.v1";

    public static readonly StageRoster Request = new(Seq(
        (0, "stage", nameof(StageRequest.Stage)),
        (1, "modelCardId", nameof(StageRequest.ModelCardId)),
        (2, "licenseClass", nameof(StageRequest.License)),
        (3, "inputs", nameof(StageRequest.Inputs)),
        (4, "inputWidth", nameof(StageRequest.InputWidth)),
        (5, "inputHeight", nameof(StageRequest.InputHeight)),
        (6, "outputWidth", nameof(StageRequest.OutputWidth)),
        (7, "outputHeight", nameof(StageRequest.OutputHeight)),
        (8, "tileWidth", nameof(StageRequest.TileWidth)),
        (9, "tileHeight", nameof(StageRequest.TileHeight)),
        (10, "overlap", nameof(StageRequest.Overlap)),
        (11, "padMode", nameof(StageRequest.Pad)),
        (12, "bucket", nameof(StageRequest.Bucket)),
        (13, "provider", nameof(StageRequest.Provider)),
        (14, "precision", nameof(StageRequest.Precision)),
        (15, "seed", nameof(StageRequest.Seed)),
        (16, "op", nameof(StageRequest.Op)),
        (17, "artefact", nameof(StageRequest.Artefact)),
        (18, "layout", nameof(StageRequest.Layout))));

    public static readonly StageRoster Result = new(Seq(
        (0, "stage", nameof(StageResult.Stage)),
        (1, "modelCardId", nameof(StageResult.ModelCardId)),
        (2, "outputs", nameof(StageResult.Outputs)),
        (3, "providerUsed", nameof(StageResult.ProviderUsed)),
        (4, "partitionCount", nameof(StageResult.PartitionCount)),
        (5, "elapsedMs", nameof(StageResult.ElapsedMs)),
        (6, "goldenDelta", nameof(StageResult.GoldenDelta)),
        (7, "tilesEmitted", nameof(StageResult.TilesEmitted)),
        (8, "op", nameof(StageResult.Op)),
        (9, "artefact", nameof(StageResult.Artefact)),
        (10, "parityFresh", nameof(StageResult.ParityFresh)),
        (11, "coverage", nameof(StageResult.Coverage)),
        (12, "scores", nameof(StageResult.Scores))));

    // Slot ordinals and wire names ALONE fold, because that half the producing end reproduces from its own
    // roster; the Compute column stays a build-time binding. Ordering by slot makes the digest independent of
    // declaration order, so a re-sorted roster is not a contract change.
    public static string Checksum(StageRoster roster) =>
        ContractGuard.Digest(toSeq(roster.Rows.OrderBy(static row => row.Slot)).Map(static row => $"{row.Slot}:{row.Wire}"));

    // Record arity IS the proof: a wire column appended with no Compute column, a Compute column with no slot, or
    // a duplicated ordinal all fail here at boot, where a decoder trusting position fails at the texel.
    public static Fin<Unit> Probe() =>
        Arity<StageRequest>() == Some(Request.Rows.Count) && Arity<StageResult>() == Some(Result.Rows.Count)
        && Request.Rows.Map(static row => row.Slot).ToFrozenSet().Count == Request.Rows.Count
        && Result.Rows.Map(static row => row.Slot).ToFrozenSet().Count == Result.Rows.Count
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(Arity<StageRequest>() - Request.Rows.Count, Arity<StageResult>() - Result.Rows.Count, 0L))));

    // A second constructor is a drift the probe must REPORT, not throw on: the whole point of this member is a
    // typed refusal, and the single-element list pattern answers "exactly one primary constructor" without one.
    static Option<int> Arity<T>() =>
        typeof(T).GetConstructors() is [ConstructorInfo primary] ? Some(primary.GetParameters().Length) : None;
}
```

## [04]-[FAULT_PROJECTION]

- Owner: `WireFault` is the local client-edge transport rail; `WireViolation` carries typed malformed-envelope evidence; `WireBoundary` identifies captured codec/descriptor sites without a string grammar; `FaultWire` packs any generated `Fault` into compact `FaultDetail`; `RemoteFault` retains the admitted foreign numeric identity as opaque evidence; `FaultDetailMapper` owns the sole message construction.
- Cases: fourteen local transport and descriptor arms derive their numeric identities from `[FaultCase]`; `Remote` carries the foreign code opaquely and never reconstructs its source family.
- Entry: `Decode(RpcException error, Error cause)` returns a cause-bearing remote transport fault when one detail is admitted; `DecodeConflict(FaultDetail detail)` admits the in-band envelope as evidence; `Classify` projects residual transport status; `FaultWire.Pack` is total.
- Law: recovery crosses through the `FaultRecovery` oneof because a numeric code alone cannot determine it; TypeScript consumes recovery directly and never mirrors `FaultBand`.
- Law: `code` is the sole transported identity and is copied from the generated `Fault.Code`; owner, band, offset, and union case remain local derivations with no wire mirror.
- Packages: Google.Protobuf, Grpc.Net.Client, NodaTime, NodaTime.Serialization.Protobuf, Riok.Mapperly (`[Mapper]`, `[UseStaticMapper]`, the additional-parameter Map(TSource, TContext) shape — the one `FaultDetail` construction), LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (project)
- Growth: a new local transport arm is one `[FaultCase]` leaf and one `StatusRail` entry when a status gains distinct semantics; a new malformed-envelope condition is one `WireViolation` case; `FaultWire.Pack` already carries every generated fault family without a mirror row.
- Boundary: the server packs one `FaultDetail` into `google.rpc.Status`; the client admits exactly one recognized detail as opaque `RemoteFault` on a cause-bearing `WireFault.Remote`. Zero recognized details use transport classification; malformed or multiple recognized details retain the caught RPC error on `Internal`. Local admission failures carry typed `WireViolation` evidence, and codec/descriptor captures carry `WireBoundary` plus the exact cause. The in-band conflict slot admits the same compact envelope as response evidence without fabricating a transport cause. Status lookup is keyed by numeric `StatusCode`, never ordinal position.

```csharp signature
// The local transport family derives numeric identity directly from its generated cases; the remote arm retains
// a foreign code as evidence and never aliases it into this family's range.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Wire;
    private WireFault(string message) => Message = message;

    public sealed override string Message { get; }

    [FaultCase(0)] public sealed partial record Cancelled(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(1)] public sealed partial record DeadlineExpired(string Detail, Error Cause) : WireFault(Detail), ICausedFault;

    // The residual status is a COLUMN, not a fragment of the rendered message, because this arm absorbs every code
    // the rail does not map by name and a recovery reading `Unavailable` apart from `Unknown` cannot parse it back
    // out of prose. `Status` rather than `Code`: `Code` is the generated sealed integer derivation.
    [FaultCase(2)] public sealed partial record Unreachable(StatusCode Status, string Detail, Error Cause) : WireFault($"{Status}:{Detail}"), ICausedFault {
        public override Retriability Retriability => Status is StatusCode.Unavailable ? Retriability.Transient : Retriability.Terminal;
    }

    [FaultCase(3)] public sealed partial record InvalidRequest(WireViolation Violation) : WireFault("invalid wire value");
    [FaultCase(4)] public sealed partial record NotFound(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(5)] public sealed partial record Conflict(ByteString Expected, ByteString Actual)
        : WireFault("transaction idempotency mismatch");
    [FaultCase(6)] public sealed partial record PermissionDenied(string Detail, Error Cause) : WireFault(Detail), ICausedFault;

    // The two transport arms the kernel re-drive rail may re-attempt: a server out of capacity and an unreachable
    // endpoint both answer the same request on a later attempt, while every deterministic refusal below inherits
    // the kernel `Terminal` default by construction — re-attempting one buys the identical verdict at cost.
    [FaultCase(7)] public sealed partial record Exhausted(string Detail, Error Cause) : WireFault(Detail), ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }

    [FaultCase(8)] public sealed partial record Unauthenticated(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    // Captured local codec/descriptor failures land here with their exact error. A remote INTERNAL status is a
    // transport verdict and routes through Unreachable, so this leaf never needs an optional or fabricated cause.
    [FaultCase(9)] public sealed partial record Internal(WireBoundary Boundary, Error Cause)
        : WireFault($"wire boundary failed: {Boundary.Key}"), ICausedFault;
    [FaultCase(10)] public sealed partial record OutOfRange(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(11)] public sealed partial record DataLoss(string Detail, Error Cause) : WireFault(Detail), ICausedFault;
    [FaultCase(12)] public sealed partial record Unimplemented(string Detail, Error Cause) : WireFault(Detail), ICausedFault;

    // The contract gate's own refusal: an empty descriptor set and a set the runtime cannot build are wire-
    // ADMISSION failures, not the benchmark-equivalence claim `ComputeFault.EquivalenceMiss` answers, and one code
    // serving both made two unrelated recoveries indistinguishable — `FaultId` equality is by code alone.
    // Deterministic by construction: the same bytes fail the same way, so the arm inherits the kernel `Terminal`
    // default and states no re-drive posture of its own.
    [FaultCase(13)] public sealed partial record DescriptorRejected() : WireFault("descriptor set is empty");

    [FaultCase(14)] public sealed partial record Remote(RemoteFault Evidence, Error Cause) : WireFault(Evidence.Message), ICausedFault {
        public override Retriability Retriability => Evidence.Recovery;
    }

    public const string DetailsTrailer = "grpc-status-details-bin";

    public static Fin<Option<WireFault>> Decode(RpcException error, Error cause) =>
        Optional(error.Trailers.GetValueBytes(DetailsTrailer)).Match(
            None: static () => Fin.Succ(Option<WireFault>.None),
            Some: bytes => Op.Of().Catch(() => Fin.Succ(Google.Rpc.Status.Parser.ParseFrom(bytes)))
                .MapFail(fault => (Error)new Internal(WireBoundary.RemoteStatus, Error.Many([cause, fault])))
                .Bind(status => Detail(status, cause)));

    private static Fin<Option<WireFault>> Detail(Google.Rpc.Status status, Error cause) {
        Seq<Any> recognized = toSeq(status.Details).Filter(static any => any.Is(FaultDetail.Descriptor));
        return recognized.Count switch {
            0 => Fin.Succ(Option<WireFault>.None),
            1 => Op.Of().Catch(() => Fin.Succ(recognized.Head.Unpack<FaultDetail>()))
                .MapFail(fault => (Error)new Internal(WireBoundary.RemoteDetail, Error.Many([cause, fault])))
                .Bind(detail => FaultWire.Admit(detail)
                    .Map(evidence => Some<WireFault>(new Remote(evidence, cause)))
                    .MapFail(fault => (Error)new Internal(WireBoundary.DetailAdmission, Error.Many([cause, fault])))),
            _ => Fin.Fail<Option<WireFault>>(new Internal(WireBoundary.DetailMultiplicity, cause)),
        };
    }

    public static Fin<RemoteFault> DecodeConflict(FaultDetail detail) => FaultWire.Admit(detail);

    public static FaultDetail PackConflict(Fault fault, CorrelationId correlation, (Instant Physical, ulong Logical) stamp, Option<TenantId> tenant = default) =>
        FaultWire.Pack(fault, correlation, stamp, tenant);

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireBoundary {
    public static readonly WireBoundary QueryFieldNumber = new("query-field-number");
    public static readonly WireBoundary InboundPayload = new("inbound-payload");
    public static readonly WireBoundary DescriptorSet = new("descriptor-set");
    public static readonly WireBoundary RemoteStatus = new("remote-status");
    public static readonly WireBoundary RemoteDetail = new("remote-detail");
    public static readonly WireBoundary DetailAdmission = new("detail-admission");
    public static readonly WireBoundary DetailMultiplicity = new("detail-multiplicity");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireViolation {
    private WireViolation() { }

    public sealed record Code(int Value) : WireViolation;
    public sealed record Correlation(string Value) : WireViolation;
    public sealed record Tenant(string Value) : WireViolation;
    public sealed record MissingRecovery : WireViolation;
    public sealed record MissingTimestamp : WireViolation;
    public sealed record RecoveryKind(FaultRecovery.KindOneofCase Value) : WireViolation;
    public sealed record RetryDuration(long Seconds, int Nanos) : WireViolation;
    public sealed record Timestamp(long Seconds, int Nanos) : WireViolation;
    public sealed record RetryDelay(Duration Value) : WireViolation;
    public sealed record ReceiptDisposition(bool Committed, bool HasConflict) : WireViolation;
}

// Packing is total because every generated Fault already owns a positive numeric identity. Admission keeps the
// remote code opaque and validates only the context and recovery data this branch consumes.
public sealed record RemoteFault(
    int Code,
    string Message,
    CorrelationId Correlation,
    Instant HlcPhysical,
    ulong HlcLogical,
    Option<TenantId> Tenant,
    Retriability Recovery);

public static class FaultWire {
    public static FaultDetail Pack(Fault fault, CorrelationId correlation, (Instant Physical, ulong Logical) stamp, Option<TenantId> tenant = default) =>
        FaultDetailMapper.Detail(fault.Code, fault.Message, correlation.ToString(),
            stamp.Physical, stamp.Logical, tenant.Map(static id => id.Wire).IfNone(""), Recovery(fault.Retriability));

    public static Fin<RemoteFault> Admit(FaultDetail detail) =>
        from code in detail.Code > 0
            ? Fin.Succ(detail.Code)
            : Fin.Fail<int>(new InvalidRequest(new WireViolation.Code(detail.Code)))
        from correlation in CorrelationId.TryCreate(detail.Correlation)
            .ToFin(new InvalidRequest(new WireViolation.Correlation(detail.Correlation)))
        from recovery in Optional(detail.Recovery)
            .ToFin(new InvalidRequest(new WireViolation.MissingRecovery()))
            .Bind(Recovery)
        from physical in detail.HlcPhysical is { } timestamp
            ? Op.Of().Catch(() => Fin.Succ(timestamp.ToInstant()))
                .MapFail(_ => (Error)new InvalidRequest(new WireViolation.Timestamp(timestamp.Seconds, timestamp.Nanos)))
            : Fin.Fail<Instant>(new InvalidRequest(new WireViolation.MissingTimestamp()))
        from tenant in string.IsNullOrEmpty(detail.Tenant)
            ? Fin.Succ(Option<TenantId>.None)
            : TenantId.TryCreate(detail.Tenant)
                .Map(static id => Some(id))
                .ToFin(new InvalidRequest(new WireViolation.Tenant(detail.Tenant)))
        select new RemoteFault(code, detail.Message, correlation, physical, detail.HlcLogical, tenant, recovery);

    static FaultRecovery Recovery(Retriability recovery) => recovery.Switch(
        terminalCase: static _ => new FaultRecovery { Terminal = new Empty() },
        transientCase: static _ => new FaultRecovery { Transient = new Empty() },
        throttledCase: static held => new FaultRecovery { RetryAfter = held.RetryAfter.ToDuration() });

    static Fin<Retriability> Recovery(FaultRecovery recovery) => recovery.KindCase switch {
        FaultRecovery.KindOneofCase.Terminal => Fin.Succ(Retriability.Terminal),
        FaultRecovery.KindOneofCase.Transient => Fin.Succ(Retriability.Transient),
        FaultRecovery.KindOneofCase.RetryAfter when recovery.RetryAfter is null =>
            Fin.Fail<Retriability>(new InvalidRequest(new WireViolation.RetryDuration(0L, 0))),
        FaultRecovery.KindOneofCase.RetryAfter => Op.Of().Catch(() => Fin.Succ(recovery.RetryAfter.ToDuration()))
            .MapFail(_ => (Error)new InvalidRequest(new WireViolation.RetryDuration(
                recovery.RetryAfter.Seconds, recovery.RetryAfter.Nanos)))
            .Bind(static delay => delay >= Duration.Zero
                ? Fin.Succ(Retriability.Throttled(delay))
                : Fin.Fail<Retriability>(new InvalidRequest(new WireViolation.RetryDelay(delay)))),
        _ => Fin.Fail<Retriability>(new InvalidRequest(new WireViolation.RecoveryKind(recovery.KindCase))),
    };
}

// ONE FaultDetail construction: the trailer pack and the in-band conflict pack were the same initializer written
// twice, so a new FaultDetail column landed twice or silently once. Each parameter maps its same-named member.
[Mapper]
[UseStaticMapper(typeof(NodaExtensions))]
[UseStaticMapper(typeof(ProtobufExtensions))]
public static partial class FaultDetailMapper {
    public static partial FaultDetail Detail(
        int code, string message, string correlation, Instant hlcPhysical,
        ulong hlcLogical, string tenant, FaultRecovery recovery);
}
```

## [05]-[TS_PROJECTION]

- Owner: `StreamKind`, `MethodShape`, `TransportCapabilityWire`, `TransportFramingWire`, `ArtifactFrameWire`, `TransactionReceiptWire`, `SupportBundleRequestWire`, `SupportBundleReplyWire`, and the six service method-shape aliases — the TS posture for the whole suite wire including the flagship transaction-parity shape.
- Law: a method-shape row exists for a MINTED `[02]` row alone — connect-es generates from the emitted descriptor set, so a shape naming an unminted rpc types a client member codegen cannot produce; an unminted verb reaches these aliases in the same motion that mints its messages. `HealthShape` is the exception the upstream package earns: its client ships generated and `compute.proto` declares nothing for it.
- Law: the TS contract stays WHOLE on this page while the C# frame/channel mechanics (`FrameEdge`, `RemoteTransport`, `GrpcChannelPolicy`) live on `Runtime/channels#ARTIFACT_FRAMES`/`Runtime/channels#TRANSPORT_AXIS` — `ArtifactFrameWire` and `TransportFramingWire` cite that frame law by PROSE ANCHOR, never a cross-split fence import.
- Packages: BCL inbox
- Growth: one method-shape row per new rpc and one field row per new evidence slot; zero new surface.
- Boundary: connect-es v2 consumes the app-root-emitted descriptor set through protoc-gen-es v2 single-plugin codegen. The generated `FaultDetail` message is decoded directly by `interchange/codec`; TypeScript owns no package/category/case mirror. The flagship transaction conflict slot carries that same generated message. Client-stream and bidi remain absent in the browser, and `ArtifactSyncServiceShape.sync` is the one browser-undialable shape. Contract-diff carriers and `GaussianSplatScan` earn no hand-mirrored TS rows because their consumers read descriptor identity and framed bytes respectively.

```ts signature
type StreamKind = "unary" | "serverStream" | "clientStream" | "bidi";

interface MethodShape<K extends StreamKind, I extends string, O extends string> { kind: K; request: I; response: O; }

interface TransportCapabilityWire { http2: ["unary", "serverStream", "clientStream", "bidi"]; grpcWeb: ["unary", "serverStream"]; }

interface TransportFramingWire { http2: { mode: "binary"; carries: ["unary", "serverStream", "clientStream", "bidi"] }; grpcWeb: { mode: "binary"; mediaType: "application/grpc-web"; carries: ["unary", "serverStream"] }; }

type ComputeServiceShape = { infer: MethodShape<"unary", "InferRequest", "InferResponse">; solve: MethodShape<"unary", "SolveRequest", "SolveResponse">; generate: MethodShape<"serverStream", "GenerateRequest", "TokenChunk">; graphDiff: MethodShape<"unary", "GraphDiffRequest", "GraphDiffResponse">; subtreeFetch: MethodShape<"serverStream", "SubtreeFetchRequest", "GraphChunk">; tessellate: MethodShape<"unary", "TessellationRequest", "TessellationReceipt">; };

type DocumentServiceShape = { executeTransaction: MethodShape<"unary", "TransactionRequest", "TransactionReceipt">; query: MethodShape<"unary", "QueryRequest", "QueryResponse">; };

type ControlServiceShape = { reloadOptions: MethodShape<"unary", "Empty", "ReloadReply">; dispatchTool: MethodShape<"unary", "DispatchToolRequest", "CommandReply">; dispatchPatch: MethodShape<"unary", "DispatchPatchRequest", "ReloadReply">; setDegradation: MethodShape<"unary", "SetDegradationRequest", "DegradationReply">; drainRuntime: MethodShape<"unary", "DrainRuntimeRequest", "DrainReply">; };

type DiagnosticServiceShape = { captureBundle: MethodShape<"unary", "SupportBundleRequest", "SupportBundleReply">; };

type ArtifactSyncServiceShape = { sync: MethodShape<"bidi", "ArtifactFrame", "ArtifactFrame">; };

type HealthShape = { check: MethodShape<"unary", "HealthCheckRequest", "HealthCheckResponse">; watch: MethodShape<"serverStream", "HealthCheckRequest", "HealthCheckResponse">; };

interface TransactionRequestWire { idempotencyKey: Uint8Array; ops: { typeUrl: string; value: Uint8Array }[]; expectedEpoch: bigint; hlcPhysical: string; hlcLogical: bigint; correlation: string; }

type TransactionReceiptWire = TransactionReceipt;

interface QueryRequestWire { scope: string; predicate: Record<string, unknown>; cursor: string; mask: string[]; }

interface SupportBundleRequestWire { collectors: string[]; }

interface SupportBundleReplyWire { contentKey: string; archive: Uint8Array; collected: string[]; skipped: string[]; }

interface ArtifactFrameWire { artifactId: string; artifactBytes: bigint; offset: bigint; frameCrc: number; payload: Uint8Array; }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
