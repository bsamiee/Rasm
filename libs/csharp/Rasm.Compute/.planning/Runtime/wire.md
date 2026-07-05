# [COMPUTE_WIRE]

Rasm.Compute owns the suite wire CONTRACT: the proto services compiled GrpcServices=Client in this package and GrpcServices=Server at app roots, one descriptor-diff contract-evolution law detecting field-shape, rpc-shape, oneof-membership, enum-surface, reserved-number, packed-flip, and nested-type drift behind one canonical XxHash128 projection-checksum gate, and one FaultDetail family carrying every typed fault across the wire — the client-edge `WireFault` rail total over the seventeen-code `StatusCode` taxonomy with an in-band conflict decode arm, and the BAND-COMPLETE `FaultWire` projection whose `Bands` registry mirrors the `Runtime/admission#DISPATCH_SPINE` custody map so every `ComputeFault` case (core 2200–2212, symbolic 2213–2216, analysis 2217–2219, scheduling 2220) provably crosses as a FaultDetail row. The flagship `DocumentService`↔`DocumentTransaction` parity seam is wire-complete here: the remote-lane `WireDocument` surface folds the deadline budget, the `Bounded` payload pre-check, the `Classify` fault rail, and receipt emission into the same canonical operation set field-for-field across in-process and cross-process, so a transaction executed locally and one executed over the channel return the identical typed receipt and decode the same in-band conflict. The `GaussianSplatScan` scan-buffer frame is MINTED on this page — SPZ v4 and SOG v2 are stable versioned published law, so C# owns the reality-capture wire vocabulary — and the browser TS posture projects the whole suite wire as type-only contracts. The channel MECHANICS — transport rows, channel tuning, call policy, artifact framing — live on `Runtime/transport`; this page owns what the wire SAYS, that page owns how it MOVES. The Mapperly `[Mapper]`/`[MapDerivedType]` per-case transcription and the Generator.Equals `[Equatable]` structural-equality aspects are the admitted proto↔domain boundary generators. The package spine is Google.Protobuf, Grpc.Tools, Grpc.Net.Client, NodaTime.Serialization.Protobuf, Riok.Mapperly, Generator.Equals, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [01]-[INDEX]

- [02]-[PROTO_VOCABULARY]: five wire services, canonical geometry messages, the `DocumentService`↔`DocumentTransaction` parity seam, one polymorphic field-mask projection, `Any` polymorphic envelope, JSON edge, generated-client capsule, the MINTED `GaussianSplatScan` reality-capture scan-buffer frame.
- [03]-[CONTRACT_EVOLUTION]: descriptor-diff drift law over field/rpc/oneof/enum/reserved/packed/nested surface behind one canonical projection-checksum gate, parse hardening, reserved-number policy, field-mask read guard.
- [04]-[FAULT_PROJECTION]: one `FaultDetail` family plus a `StatusCode`→`WireFault` rail total over the seventeen-code taxonomy, the band-complete `FaultWire` `ComputeFault`→`FaultDetail` projection mirroring the admission custody map, an in-band transaction-conflict decode arm, and a conflict receipt pack.
- [05]-[TS_PROJECTION]: browser wire posture, fault and frame contracts, method shapes, transaction-parity shape, field-mask read.

## [02]-[PROTO_VOCABULARY]

- Owner: the five service contracts and the canonical geometry message family declared in the remote-lane owner folder; `WireServices` — the channel-scoped generated-client capsule carrying one polymorphic `Mask` projection and the `Unpack` typed-fault projection; `WireDocument` — the flagship `DocumentService`↔`DocumentTransaction` parity surface folding budget, `Bounded` pre-check, `Classify`, and receipt emission into the canonical operation set field-for-field across in-process and cross-process.
- Cases: ComputeService, DocumentService, ControlService, ArtifactSync, grpc.health.v1.Health — google/rpc/status.proto and grpc.health.v1 compile verbatim beside the owned files.
- Auto: Grpc.Tools compiles GrpcServices=Client at build with `PrivateAssets=all`, `Access=Internal` for package-internal generated types and `Access=Public` only where the contract crosses the package boundary; app roots compile the same files GrpcServices=Server and emit the descriptor set that feeds connect-es codegen and the manifest checksum.
- Packages: Google.Protobuf, Grpc.Tools, Grpc.Net.Client, NodaTime.Serialization.Protobuf
- Flagship: `WireDocument` is the `DocumentService`↔`DocumentTransaction` parity owner — `ExecuteTransaction` carries the in-process `DocumentTransaction` verb set field-for-field through one budget-bounded, fault-classified, receipt-emitting forwarder, so the same canonical operation set, the same `TransactionReceipt`, and the same wire choreography produce the identical typed receipt whether the transaction runs through the in-process handler or across the channel; the dedup window equals the `DeadlineClass.HopTotal` allotment so the one retry owner's horizon gates the idempotency edge on both legs, the response mirrors the typed receipt through `WireDocument.Receipt`, and a non-exceptional in-band conflict decodes through `WireDocument.Conflict` reading the `TransactionReceipt.conflict=5` slot onto the typed `WireFault` rail with no parallel response DTO and no hand-rolled per-consumer projection.
- Growth: one rpc row on an existing service or one numbered message field absorbs a new wire fact; the browser collaboration decomposition (server-stream down, unary chunked up) is designed-only growth of one rpc row per direction; zero new surface.
- Boundary: temporal values cross as Timestamp and protobuf Duration through `ToTimestamp`/`ToProtobufDuration` outward and `ToInstant`/`ToNodaDuration` inward — BCL DateTime never sits between wire and rail; calendar-bearing capture and schedule facts cross as `Google.Type` commons through `ToDate`/`ToTimeOfDay`/`ToProtobufDayOfWeek` outward and `ToLocalDate`/`ToLocalTime`/`ToIsoDayOfWeek` inward, so a serialized date string never sits between wire and rail; FieldMask carries the read projection and the partial-update write leg through one polymorphic `WireServices.Mask` entrypoint that discriminates on input shape — an `int`-span admits a field-number-validated NAME-path mask through `FieldMask.FromFieldNumbers<QueryResponse>` (the typed overload resolving each number to its field NAME via `FindFieldByNumber` against the generated descriptor, never a free string path) and a `string`-span admits a caller-path mask through the non-throwing `FieldMask.FromString` re-guarded by one load-bearing `FieldMask.IsValid(QueryResponse.Descriptor, mask)` gate against the generated descriptor, both `Normalize`d to canonical sorted-deduplicated form and unified on `Fin<FieldMask>` so an unknown path or number faults at the edge rather than silently dropping or throwing past it — the same partial-read mask the web-fed Query feed consumes, never a per-tile request DTO or a second mask carrier; Any with TypeRegistry carries polymorphic artifact envelopes through `WireServices.Unpack` over `Any.TryUnpack<T>` keyed by `Any.Is(descriptor)` projecting the typed fault, while outbound packing rides `Any.Pack` directly at the one staging site (`FrameEdge.Transaction`) — a rename-forward `Pack` wrapper is the deleted form; Empty carries signals; `JsonFormatter` and `JsonParser` with the same TypeRegistry are the dashboard edge over the identical generated messages — a parallel web DTO family is the deleted form; `ExecuteTransaction` defends its idempotency edge by `Clone` on the dedup-window receipt rather than mutating the cached message in place — a shared-mutable cached message is the deleted form; `OriginalNameAttribute` reconciles a proto field name to its diverged C# name at the descriptor surface so the contract-evolution key reads the proto name, never the generated identifier; the proto geometry family is the single binary wire geometry, with NetTopologySuite as the store boundary projection, GeoJSON as the JSON projection, and RhinoCommon as the host projection; ArtifactSync carries the wire leg only — sync state, diffing, and transfer manifests are store mechanics; the gaussian-splat reality-capture scan crosses this ArtifactSync wire as the `GaussianSplatScan` artifact message — the Python `realitycapture` companion publishes the already-decoded scan-buffer layout (packed position/scale/rotation/spherical-harmonic float buffers + the harmonic-degree band + the per-splat count), framed by the `[ARTIFACT_FRAMES]` `FrameEdge` and reassembled through `FrameEdge.Reassemble<GaussianSplatScan>` under the whole-artifact `XxHash128` identity gate, then admitted (projected) to the Compute `Rasm.Compute/Runtime/payload#RESIDENCY` `SplatScan` the gaussian-splat residency encode consumes; it is a STANDALONE artifact riding the generic `ArtifactFrame` bytes, NEVER a `GeometryPayload` oneof case (the oneof carries point_cloud/mesh/voxel only), so no splat oneof arm is admitted and the frame is MINTED live wire vocabulary now — SPZ v4 / SOG v2 are stable versioned published law, the standalone artifact admits to `payload#RESIDENCY` `SplatScan`, and the Python `realitycapture` companion SPZ/SOG decode plus the `xxhash` cp315 wheel stay named sibling-branch facts, never a mint gate; the `Solve`/`Generate` rpcs carry the numeric-lane decomposition and generative-run legs field-for-field with no second request shape, and the `GraphDiff`/`SubtreeFetch` rpcs carry the content-key delta wire shape only — the set-difference computation is `Rasm.Persistence/Version/ledger#CHANGEFEED` (the `TransferSet` closure-minus-held fold over the `Closure` descendant content-key manifest), so Compute owns the wire frame and Persistence's ledger owns the diff algebra.

```csharp signature
public sealed record WireServices(
    GrpcChannel Channel,
    ComputeService.ComputeServiceClient Compute,
    DocumentService.DocumentServiceClient Document,
    ControlService.ControlServiceClient Control,
    ArtifactSync.ArtifactSyncClient Artifacts,
    Health.HealthClient Health) : IDisposable {
    public static Fin<FieldMask> Mask(params ReadOnlySpan<int> fieldNumbers) {
        var numbers = fieldNumbers.ToArray();
        return Try.lift(() => FieldMask.FromFieldNumbers<QueryResponse>(numbers).Normalize()).Run()
            .MapFail(static error => new ComputeFault.PayloadOverBounds($"unknown query field-number: {error.Message}"));
    }

    public static Fin<FieldMask> Mask(params ReadOnlySpan<string> paths) =>
        FieldMask.FromString(string.Join(',', paths.ToArray())) is var mask && FieldMask.IsValid(QueryResponse.Descriptor, mask)
            ? Fin.Succ(mask.Normalize())
            : Fin.Fail<FieldMask>(new ComputeFault.PayloadOverBounds($"unknown query path in [{string.Join(',', paths.ToArray())}]"));

    public static Fin<T> Unpack<T>(Any envelope) where T : class, IMessage<T>, new() =>
        envelope.TryUnpack<T>(out var artifact)
            ? Fin.Succ(artifact)
            : Fin.Fail<T>(new ComputeFault.PayloadOverBounds($"any-envelope {Any.GetTypeName(envelope.TypeUrl)} not {new T().Descriptor.FullName}"));

    public void Dispose() => Channel.Dispose();
}

public static class WireDocument {
    public static Task<Fin<TransactionReceipt>> ExecuteTransaction(WireServices services, TransactionRequest request, Instant deadline, CancellationToken token) =>
        CallSpine.Bounded(request).Match(
            Succ: bounded => CallSpine.Awaited(services.Document.ExecuteTransactionAsync(bounded, CallSpine.Options(deadline, token)).ResponseAsync),
            Fail: static error => Task.FromResult(Fin.Fail<TransactionReceipt>(error)));

    public static Task<Fin<QueryResponse>> Query(WireServices services, QueryRequest request, FieldMask projection, Instant deadline, CancellationToken token) =>
        CallSpine.Awaited(services.Document.QueryAsync(request with { Mask = projection }, CallSpine.Options(deadline, token)).ResponseAsync);

    public static IAsyncEnumerable<DocumentEvent> Watch(WireServices services, WatchRequest request, Instant deadline, CancellationToken token) =>
        services.Document.DocumentEvents(request, CallSpine.Options(deadline, token)).ResponseStream.ReadAllAsync(token);

    public static TransactionReceipt Receipt(TransactionReceipt wire, ByteString idempotencyKey) =>
        wire.IdempotencyKey == idempotencyKey ? wire : wire.Clone();

    public static Option<WireFault> Conflict(TransactionReceipt receipt) =>
        receipt.Committed || receipt.Conflict is null
            ? None
            : Some(WireFault.Decode(receipt.Conflict));
}
```

| [INDEX] | [SERVICE]       | [RPC]              | [SHAPE]       | [MESSAGES]                               | [LAW]                                                                                                                                                                                                                                                                                                                                                             |
| :-----: | --------------- | ------------------ | ------------- | ---------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | ComputeService  | Infer              | unary         | InferRequest → InferResponse             | payload caps pre-checked at the call edge; faults ride FaultDetail                                                                                                                                                                                                                                                                                                |
|  [02]   | ComputeService  | Progress           | server-stream | ProgressRequest → ProgressUpdate         | phase enum mirrors the nine phase keys 1:1, keyed by correlation                                                                                                                                                                                                                                                                                                  |
|  [03]   | ComputeService  | Capabilities       | unary         | Empty → ComputeCapabilities              | substrate rows, EP rows, model inventory, payload caps, contract metadata — Compute capability rows only                                                                                                                                                                                                                                                          |
|  [04]   | DocumentService | Capabilities       | unary         | Empty → DocumentCapabilities             | verb inventory and document scope                                                                                                                                                                                                                                                                                                                                 |
|  [05]   | DocumentService | DocumentEvents     | server-stream | WatchRequest → DocumentEvent             | watch-fact stream feeding the live-data spine                                                                                                                                                                                                                                                                                                                     |
|  [06]   | DocumentService | ExecuteTransaction | unary         | TransactionRequest → TransactionReceipt  | flagship parity: idempotency key; server dedup window equals the DeadlineClass.HopTotal allotment — the one retry owner's horizon; the forwarder folds Bounded+budget+Classify+receipt; the receipt mirrors the DocumentTransaction typed receipt field-for-field through `WireDocument.Receipt` and the in-band conflict decodes through `WireDocument.Conflict` |
|  [07]   | DocumentService | Query              | unary         | QueryRequest → QueryResponse             | read verb with FieldMask projection via `WireServices.Mask`                                                                                                                                                                                                                                                                                                       |
|  [08]   | DocumentService | CaptureEvents      | client-stream | CaptureFrame → CaptureSummary            | per-frame HLC idempotency keys; Http2 and UnixDomainSocket rows only                                                                                                                                                                                                                                                                                              |
|  [09]   | ControlService  | CaptureSupport     | unary         | Empty → CaptureSupportReply              | projects the SupportManifest receipt                                                                                                                                                                                                                                                                                                                              |
|  [10]   | ControlService  | SetDegradation     | unary         | SetDegradationRequest → DegradationReply | level key lands on the one override rail — the OperatorOverride consequence                                                                                                                                                                                                                                                                                       |
|  [11]   | ControlService  | ReloadOptions      | unary         | Empty → ReloadReply                      | projects the ReloadReceipt                                                                                                                                                                                                                                                                                                                                        |
|  [12]   | ArtifactSync    | Sync               | bidi          | ArtifactFrame → ArtifactFrame            | frame law below; FieldMask partials; Any artifact envelopes                                                                                                                                                                                                                                                                                                       |
|  [13]   | Health          | Check              | unary         | HealthCheckRequest → HealthCheckResponse | maps from the HealthChecks registry via the WireHealthRow tag predicate; substrate predicate and node selection read it                                                                                                                                                                                                                                           |
|  [14]   | Health          | Watch              | server-stream | HealthCheckRequest → HealthCheckResponse | compiled verbatim from the well-known proto                                                                                                                                                                                                                                                                                                                       |
|  [15]   | ComputeService  | Solve              | unary         | SolveRequest → SolveResponse             | carries the numeric-lane dense or sparse decomposition field-for-field; faults ride FaultDetail; the row-block shard sub-solve dials this rpc                                                                                                                                                                                                                     |
|  [16]   | ComputeService  | Generate           | server-stream | GenerateRequest → TokenChunk             | the remote token-streaming leg riding the Progress-class server-stream pattern, keyed by correlation; faults ride FaultDetail                                                                                                                                                                                                                                     |
|  [17]   | ComputeService  | GraphDiff          | unary         | GraphDiffRequest → GraphDiffResponse     | content-key delta over two Closure hashes; the set-difference algebra is `Rasm.Persistence/Version/ledger#CHANGEFEED` (`TransferSet`/`Closure`), this carries the wire shape only                                                                                                                                                                                                             |
|  [18]   | ComputeService  | SubtreeFetch       | server-stream | SubtreeFetchRequest → GraphChunk         | partial-graph checkout streaming the content-addressed subtree the GraphDiff added-set names                                                                                                                                                                                                                                                                      |

| [INDEX] | [MESSAGE]           | [FIELDS]                                                                                                                                                                         | [ALIGNS]                                                                                                                                                                               |
| :-----: | ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | GeometryPayload     | oneof kind: point_cloud=1, mesh=2, voxel=3; symbolic_dims=4 repeated                                                                                                             | envelope for Infer payloads and artifacts                                                                                                                                              |
|  [02]   | PointCloudTensor    | count=1 int64; channels=2 int32; dtype=3 string; data=4 bytes                                                                                                                    | point-cloud N×C encoding row                                                                                                                                                           |
|  [03]   | MeshTensor          | vertex_count=1 int64; vertices=2 bytes; face_count=3 int64; faces=4 bytes                                                                                                        | mesh vertex N×3 and face F×3 rows                                                                                                                                                      |
|  [04]   | VoxelTensor         | dims=1 repeated int64; dtype=2 string; data=3 bytes                                                                                                                              | voxel NCHW row                                                                                                                                                                         |
|  [05]   | SymbolicDim         | name=1 string; bound=2 int64                                                                                                                                                     | symbolic-dim binding row                                                                                                                                                               |
|  [06]   | TransactionRequest  | idempotency_key=1 bytes; ops=2 repeated Any; expected_epoch=3 uint64; hlc_physical=4 google.protobuf.Timestamp; hlc_logical=5 uint64; correlation=6 string                       | flagship: the in-process `DocumentTransaction` verb set field-for-field, ops as polymorphic Any envelopes                                                                              |
|  [07]   | TransactionReceipt  | idempotency_key=1 bytes; committed=2 bool; new_epoch=3 uint64; applied=4 repeated string; conflict=5 FaultDetail; hlc_physical=6 google.protobuf.Timestamp; hlc_logical=7 uint64 | flagship: mirrors the `DocumentTransaction` typed receipt field-for-field; the `conflict` slot carries the FaultDetail the retry owner decodes through `WireDocument.Conflict` in band |
|  [08]   | QueryRequest        | scope=1 string; predicate=2 Struct; cursor=3 string; mask=4 google.protobuf.FieldMask                                                                                            | read verb carrying the field-mask projection                                                                                                                                           |
|  [09]   | QueryResponse       | rows=1 repeated Struct; cursor=2 string; total=3 int64                                                                                                                           | masked read result; the mask names the projected columns                                                                                                                               |
|  [10]   | SolveRequest        | matrix=1 bytes; rhs=2 bytes; factorization_kind=3 string; sparse_format=4 string; shard_tile=5 int32                                                                             | numeric-lane decomposition request: matrix + rhs are column-major float64 bytes (a dense solver operand is NOT a point_cloud/mesh/voxel GeometryPayload — no geometry envelope), server reshapes from byte length + shard_tile |
|  [11]   | SolveResponse       | solution=1 bytes; provider=2 string; decomposition=3 string; rows=4 int64; cols=5 int64; nnz=6 int64                                                                             | numeric-lane solve result + Factorization-receipt evidence                                                                                                                             |
|  [12]   | GenerateRequest     | model_checksum=1 string; prompt=2 string; max_length=3 double; guidance_kind=4 string; guidance_data=5 string; tools=6 string                                                    | generative-run request mirroring GenerationPolicy                                                                                                                                      |
|  [13]   | TokenChunk          | piece=1 string; token_index=2 int64; done=3 bool                                                                                                                                 | one decoded token piece per server-stream frame                                                                                                                                        |
|  [14]   | GraphDiffRequest    | base_hash=1 string; target_hash=2 string                                                                                                                                         | content-key delta over two Closure hashes                                                                                                                                              |
|  [15]   | GraphDiffResponse   | added=1 repeated string; removed=2 repeated string                                                                                                                               | added/removed content-key set                                                                                                                                                          |
|  [16]   | SubtreeFetchRequest | content_keys=1 repeated string                                                                                                                                                   | partial-graph checkout request                                                                                                                                                         |
|  [17]   | GraphChunk          | content_key=1 string; payload=2 bytes; ordinal=3 int64                                                                                                                           | one content-addressed subtree node per frame                                                                                                                                           |

[GaussianSplatScan]:
- Fields: format_key=1 string; positions=2 bytes; scales=3 bytes; rotations=4 bytes; harmonics=5 bytes; harmonic_degree=6 int32; splat_count=7 int64
- MINTED: the reality-capture splat scan-buffer frame is GROUNDED here — the SPZ v4 and SOG v2 binary specifications are stable, versioned, MIT-published law, so C# owns the wire vocabulary without waiting on a consumer: the packed float buffers carry the spec's position/scale/rotation/spherical-harmonic accessor order, `harmonic_degree` the SH band (0–3), `splat_count` the per-buffer element count, and `format_key` the source-format discriminant (`spz-v4`/`sog-v2`). It rides the ArtifactSync `ArtifactFrame` byte seam as a STANDALONE artifact (NEVER a `GeometryPayload` oneof case — the oneof carries point_cloud/mesh/voxel only), reassembled via `FrameEdge.Reassemble<GaussianSplatScan>` under the whole-artifact `XxHash128` identity gate and admitted to the Compute `Rasm.Compute/Runtime/payload#RESIDENCY` `SplatScan`, whose member set (`FormatKey`/`Positions`/`Scales`/`Rotations`/`Harmonics`/`HarmonicDegree`/`SplatCount`) byte-mirrors these fields. The Python `realitycapture` companion's SPZ/SOG DECODE and the `xxhash` cp315 wheel stay named sibling-branch facts — the frame is live wire vocabulary either way.

## [03]-[CONTRACT_EVOLUTION]

- Owner: `ContractDrift` `[Union]` three-way drift classification; `ContractGuard` — descriptor surface fold over field/rpc/oneof/enum/reserved/packed shape recursing nested message and enum types, classifier delegate, descriptor publication path, proto-name reconciliation, canonical projection checksum, field-mask read guard; `ParseGuard` — inbound parse-hardening policy record carrying the size-gated buffer decode, the proto2 `ExtensionRegistry`, and the dynamic open-envelope admission.
- Cases: Identical, Additive (tolerated), Breaking (typed rejection carrying the missing or retyped surface rows).
- Entry: `AdditiveOnly(Seq<ByteString> local, Func<string, Fin<Seq<ByteString>>> peerSetOf)` — the delegate `Discovery.Compatible` consumes; checksum equality or additive drift admits, breaking drift rejects on the hop fault rail.
- Packages: Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, Rasm.AppHost (project), BCL inbox
- Growth: a removed field becomes one reserved row carrying its number range — `message.ToProto().ReservedRange` projects each `Start`-`End` span into the surface set so numbers never return to use and a removed-then-reclaimed number classifies Breaking; one surface-projection row absorbs a new descriptor dimension — packed-encoding flip, nested-type retype, oneof-membership change; the host↔companion capability negotiation and per-node EP-option bag ride the `Struct`/`Value`/`ListValue` open-envelope column under the same additive-only contract — open within an additive-only contract, never a drift escape hatch; zero new surface.
- Boundary: contract identity is the serialized descriptor set built through `FileDescriptor.BuildFromByteStrings` at startup and published beside the discovery manifest at `DescriptorPath`; the descriptor key reads the proto field name reconciled through `OriginalNameAttribute` so a diverged C# identifier never enters the surface set; the manifest checksum is the canonical projection digest — `ContractGuard.Checksum` builds the descriptors, folds the ordered `Surface(...)` `FrozenSet<string>` the classifier already computes through `InDeclarationOrder()` into one UTF-8 byte stream, and `XxHash128.Hash`es that, so two generators emitting semantically-identical descriptors checksum-match while the doctrine-rejected raw `SerializedData` hash (non-canonical across generator versions) never enters; the `AdditiveOnly` gate admits on checksum equality before any descriptor parse and only descends into `Build`+`Classify` when the digests diverge, so an equal-descriptor peer never pays the surface-set diff and a checksum mismatch never admits on its own — descriptor-diff is the second gate behind the checksum gate, never a replacement for it; the surface fold reads seven diff-relevant dimensions, recursing nested types so a phase enum nested in a message and a nested-message field retype are both visible — field number-type-cardinality-packing-oneof-jsonname through `MessageDescriptor.Fields.InDeclarationOrder()` with `FieldDescriptor.IsPacked`, the reserved-range set through `MessageDescriptor.ToProto().ReservedRange`, oneof membership through `Oneofs`, the enum value set through `EnumTypes`/`NestedTypes` recursion with `EnumValueDescriptor.Number`, and rpc input-output-streaming shape through `ServiceDescriptor.Methods` with `MethodDescriptor.InputType`/`OutputType`/`IsClientStreaming`/`IsServerStreaming` — so a duplex→unary flip, a request-message retyping, a oneof-membership change, an enum-value removal (top-level or nested), a `[packed=true]`→`[packed=false]` flip on a repeated scalar, and a removed-then-reclaimed field number each detect as breaking, never only a removed message; `UnknownFieldSet` retention stays at the generated-parser default so forward-decoded payloads re-serialize with unknown fields intact — a discard-configured parser is the rejected form; `ParseGuard.Read` gates the untrusted payload by an O(1) `ReadOnlySequence<byte>.Length` check against `SizeLimitBytes` BEFORE any decode — an over-bound payload faults on the `PayloadOverBounds` rail without ever materializing, symmetric with the send-side pre-check — then decodes on the buffer fast path through `MessageParser<T>.ParseFrom(ReadOnlySequence<byte>)` so a fragmented pooled payload parses with no contiguous copy (the `Runtime/transport#ARTIFACT_FRAMES` `FrameEdge.Drain` decode-mirror), the buffer parse context upholding recursion at the protobuf default the `RecursionLimit` pins; the proto2 `ExtensionRegistry` resolves declared extensions at that same boundary through `parser.WithExtensionRegistry(Extensions)`, and the `Struct` open envelope admits a forward-compatible option bag without a proto regen per option — an eager `payload.ToArray()` that copies the whole sequence before the size gate and a per-call `CodedInputStream`/`MemoryStream` construction are the deleted forms.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContractDrift {
    private ContractDrift() { }

    public sealed record Identical : ContractDrift;
    public sealed record Additive(Seq<string> Added) : ContractDrift;
    public sealed record Breaking(Seq<string> Missing) : ContractDrift;
}

public sealed record ParseGuard(int SizeLimitBytes, int RecursionLimit, ExtensionRegistry Extensions) {
    public static readonly ParseGuard Canonical = new(
        SizeLimitBytes: GrpcChannelPolicy.Canonical.MaxReceiveBytes,
        RecursionLimit: 100,
        Extensions: new ExtensionRegistry());

    public Fin<T> Read<T>(MessageParser<T> parser, ReadOnlySequence<byte> payload) where T : IBufferMessage, IMessage<T> =>
        payload.Length > SizeLimitBytes
            ? new ComputeFault.PayloadOverBounds($"inbound payload {payload.Length}B over {SizeLimitBytes}B receive bound")
            : Try.lift(() => parser.WithExtensionRegistry(Extensions).ParseFrom(payload)).Run()
                .MapFail(static error => new ComputeFault.PayloadOverBounds(error.Message));

    public static Struct Envelope(HashMap<string, Value> options) =>
        options.Fold(new Struct(), static (envelope, entry) => { envelope.Fields[entry.Key] = entry.Value; return envelope; });
}

public static class ContractGuard {
    public static string DescriptorPath(ProfileRoots roots, int pid) =>
        Path.Join(roots.AppRoot, "discovery", $"rasm-{pid}.pb");

    public static Fin<string> Checksum(Seq<ByteString> serialized) =>
        Build(serialized).Map(static files => Convert.ToHexStringLower(
            XxHash128.Hash(Encoding.UTF8.GetBytes(string.Join(';', Surface(files).OrderBy(static row => row, StringComparer.Ordinal))))));

    public static Fin<Seq<FileDescriptor>> Build(Seq<ByteString> serialized) =>
        Try.lift(() => FileDescriptor.BuildFromByteStrings(serialized).ToSeq())
            .Run()
            .MapFail(static error => new HopFault.ChecksumBreaking(error.Message));

    public static ContractDrift Classify(Seq<FileDescriptor> local, Seq<FileDescriptor> peer) =>
        (Required: Surface(local), Offered: Surface(peer)) switch {
            var sets when sets.Required.Except(sets.Offered).ToSeq() is { IsEmpty: false } missing => new ContractDrift.Breaking(missing),
            var sets when sets.Offered.Except(sets.Required).ToSeq() is { IsEmpty: false } added => new ContractDrift.Additive(added),
            _ => new ContractDrift.Identical(),
        };

    public static Func<string, string, bool> AdditiveOnly(Seq<ByteString> local, Func<string, Fin<Seq<ByteString>>> peerSetOf) =>
        (localChecksum, peerChecksum) =>
            (Checksum(local).Map(digest => digest == localChecksum && peerChecksum == localChecksum).IfFail(false)) ||
            (from peerBytes in peerSetOf(peerChecksum) from peerFiles in Build(peerBytes) from localFiles in Build(local) select Classify(localFiles, peerFiles))
                .Map(static drift => drift is not ContractDrift.Breaking)
                .IfFail(false);

    static FrozenSet<string> Surface(Seq<FileDescriptor> files) =>
        files.Bind(static file => file.MessageTypes.ToSeq().Bind(MessageSurface)
                .Concat(file.EnumTypes.ToSeq().Map(EnumSurface))
                .Concat(RpcSurface(file)))
            .ToFrozenSet(StringComparer.Ordinal);

    static Seq<string> MessageSurface(MessageDescriptor message) =>
        message.Fields.InDeclarationOrder().ToSeq()
            .Map(field => $"{message.FullName}.{field.Name}={field.FieldNumber}:{field.FieldType}:{(field.IsRepeated ? "R" : "S")}:{(field.IsPacked ? "P" : "-")}:{field.ContainingOneof?.Name ?? "-"}:{field.JsonName}")
            .Concat(message.Oneofs.ToSeq().Map(oneof => $"{message.FullName}~{oneof.Name}=[{string.Join(',', oneof.Fields.OrderBy(static f => f.FieldNumber).Select(static f => f.FieldNumber))}]"))
            .Concat(message.ToProto().ReservedRange.ToSeq().Map(range => $"{message.FullName}.reserved:{range.Start}-{range.End}"))
            .Concat(message.NestedTypes.ToSeq().Bind(MessageSurface))
            .Concat(message.EnumTypes.ToSeq().Map(EnumSurface));

    static string EnumSurface(EnumDescriptor enumeration) =>
        $"{enumeration.FullName}=[{string.Join(',', enumeration.Values.OrderBy(static v => v.Number).Select(static v => $"{v.Name}:{v.Number}"))}]";

    static Seq<string> RpcSurface(FileDescriptor file) =>
        file.Services.ToSeq().Bind(static service => service.Methods.ToSeq()
            .Map(method => $"{service.FullName}/{method.Name}:{method.InputType.FullName}->{method.OutputType.FullName}:{(method.IsClientStreaming ? "C" : "U")}{(method.IsServerStreaming ? "S" : "U")}"));
}
```

## [04]-[FAULT_PROJECTION]

- Owner: `WireFault` `[Union]` — the client-edge typed rail that the one FaultDetail message family decodes onto from both the trailer path and the in-band receipt slot; the server edge packs FaultDetail at app roots; `WireFault.PackConflict` mints the FaultDetail the flagship transaction-conflict receipt carries and `WireFault.Decode(FaultDetail)` is the inverse arm reading it back onto the typed rail; `FaultWire` — the BAND-COMPLETE `ComputeFault` → `FaultDetail` projection: ONE uniform pack over the `Expected` base (code, case name, message), total over EVERY 2200-band case BY CONSTRUCTION (a new case inherits the projection with zero wire edit), with the `Bands` registry mirroring the `Runtime/admission#DISPATCH_SPINE` custody map (core 2200–2212, symbolic 2213–2216, analysis 2217–2219 — the `AnalysisFailed` case in its 2219 slot — and scheduling 2220 `GraphCyclic` all cross the wire) so a fault code outside every band row fails the composition `Probe` — the count-vs-band drift class (a "seventeen-code taxonomy" prose over a fifteen-row rail) cannot recur because the rail derives from the band, never from a hand-counted list.
- Cases: `Cancelled`, `DeadlineExpired`, `Unreachable` (carrying the residual `StatusCode`), `InvalidRequest`, `NotFound`, `Conflict`, `PermissionDenied`, `Exhausted`, `Unauthenticated`, `Internal`, `OutOfRange`, `DataLoss`, `Unimplemented` — thirteen union arms deriving `Expected` so the typed rail lifts into `Fin`/`Eff` with no bridge, each carrying its own code in the wire-fault sub-band 4520-4532 (distinct from the HopFault 4500 hop band); every typed wire-fault family — ComputeFault (band 2200), HopFault (band 4500), store faults at their app roots — packs into the same FaultDetail rows and the client decodes them back, while `Classify` lands the residual `StatusCode` taxonomy on these arms with no fallthrough but the structurally-non-fault `OK`/`Unknown` codes.
- Entry: `Decode(RpcException error)` — `Option<FaultDetail>` from the status-details trailer; `Decode(FaultDetail detail)` — the in-band arm projecting a receipt-carried FaultDetail onto the typed `Conflict` arm (the transaction-conflict slot is definitionally a conflict, the band code, package, and case preserved in the detail string); `Classify` converts the residual StatusCode taxonomy into the typed rail through the `StatusRail` `FrozenDictionary<StatusCode, Func<string, WireFault>>` fold keyed by the non-sequential numeric code, never 17 hand arms.
- Packages: Google.Protobuf, Grpc.Net.Client, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new `ComputeFault` case is one band row + one wire crossing in the SAME declaration motion — the uniform `FaultWire.Pack` already carries it and the band registry row (mirrored in the `admission.md` custody map) is the single edit, mirroring the receipts `[JsonDerivedType]` registry discipline; one evidence map row per new fault family; one `StatusRail` entry per residual code reclassification; the in-band `Decode(FaultDetail)` arm reads any new FaultDetail-bearing receipt slot onto the typed rail; zero new surface.
- Boundary: a gRPC status code plus string is never the terminal error shape — the server edge packs FaultDetail into `google.rpc.Status` details, the client edge unpacks back onto the typed rail from the trailer, and TS reconstructs the identical literal-discriminated union; the Conflict receipt is the retry-owner complement of this law and the flagship transaction-conflict path is its consumer — `WireFault.PackConflict` builds the FaultDetail the `TransactionReceipt.conflict` slot carries through the message body (not a trailer) and `WireFault.Decode(FaultDetail)` reads it back onto the typed `Conflict` arm so the retry owner reads the typed conflict off the receipt rather than re-deriving it from a status string or hand-rolling a per-consumer projection — the non-exceptional in-band conflict and the exceptional trailer fault both terminate on the one typed rail; the `StatusCode` taxonomy is non-sequential by value (`OK=0`..`Unauthenticated=16`, `Aborted=10`, `Unavailable=14`, `OutOfRange=11`, `DataLoss=15`) so the fold keys by the numeric value through a `FrozenDictionary`, never by ordinal position — an ordinal-indexed table is the deleted form; fourteen of the seventeen codes resolve to a typed non-`Unreachable` arm — `AlreadyExists`/`Aborted`/`FailedPrecondition` collapse onto the one `Conflict` arm, so twelve distinct arms cover the fourteen — while the explicitly-mapped `Unavailable` plus the two unmapped success/indeterminate codes (`OK`, `Unknown`) resolve to `Unreachable` carrying the spelled code, so the rail is total over the seventeen-code taxonomy without enumerating `OK` as a typed fault; the `WireFault` 4520–4532 sub-band is Compute's SECOND custody — distinct from the `ComputeFault` 2200 band and from the AppHost `HopFault` 4500 hop band — RECORDED beside the `Runtime/admission#DISPATCH_SPINE` custody map and PINNED as a reciprocal mirror row in the AppHost/AppUi/Persistence registries (AppHost re-bands `CoordinationFault` to 4540 around it), so cross-package band disjointness is checkable from BOTH ends and a foreign band change is a row edit on both ends, never prose; the Mapperly `[Mapper]`/`[MapDerivedType]` per-case transcription owns any `oneof`-envelope boundary where a case carries fields the protobuf runtime does not transcribe (the union's generated total `Switch` dispatches, Mapperly transcribes), and `[Equatable]` owns structural equality wherever a class-root wire shape surrenders the record-root compare — hand-written transcription or `Equals`/`GetHashCode` where the generator reaches is the deleted form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireFault : Expected, IValidationError<WireFault> {
    private WireFault(string detail, int code) : base(detail, code, None) { }

    public static WireFault Create(string message) => new Internal(message);

    public sealed record Cancelled(string Detail) : WireFault(Detail, 4520);
    public sealed record DeadlineExpired(string Detail) : WireFault(Detail, 4521);
    public sealed record Unreachable(StatusCode Code, string Detail) : WireFault($"{Code}:{Detail}", 4522);
    public sealed record InvalidRequest(string Detail) : WireFault(Detail, 4523);
    public sealed record NotFound(string Detail) : WireFault(Detail, 4524);
    public sealed record Conflict(string Detail) : WireFault(Detail, 4525);
    public sealed record PermissionDenied(string Detail) : WireFault(Detail, 4526);
    public sealed record Exhausted(string Detail) : WireFault(Detail, 4527);
    public sealed record Unauthenticated(string Detail) : WireFault(Detail, 4528);
    public sealed record Internal(string Detail) : WireFault(Detail, 4529);
    public sealed record OutOfRange(string Detail) : WireFault(Detail, 4530);
    public sealed record DataLoss(string Detail) : WireFault(Detail, 4531);
    public sealed record Unimplemented(string Detail) : WireFault(Detail, 4532);

    public const string DetailsTrailer = "grpc-status-details-bin";

    public static Option<FaultDetail> Decode(RpcException error) =>
        Optional(error.Trailers.GetValueBytes(DetailsTrailer))
            .Map(static bytes => Google.Rpc.Status.Parser.ParseFrom(bytes))
            .Bind(static rich => rich.Details.ToSeq()
                .Filter(static any => any.Is(FaultDetail.Descriptor)).Head
                .Map(static any => any.Unpack<FaultDetail>()));

    public static WireFault Decode(FaultDetail detail) =>
        new Conflict($"{detail.Package}#{detail.Case}({detail.Code}): {detail.Message}");

    public static FaultDetail PackConflict(string package, int code, string @case, string message, CorrelationId correlation, Instant hlc) =>
        new() {
            Package = package, Code = code, Case = @case, Message = message,
            Correlation = correlation.ToString(), HlcPhysical = hlc.ToTimestamp(), HlcLogical = 0,
        };

    static readonly FrozenDictionary<StatusCode, Func<string, WireFault>> StatusRail =
        new Dictionary<StatusCode, Func<string, WireFault>> {
            [StatusCode.Cancelled] = static detail => new Cancelled(detail),
            [StatusCode.DeadlineExceeded] = static detail => new DeadlineExpired(detail),
            [StatusCode.InvalidArgument] = static detail => new InvalidRequest(detail),
            [StatusCode.NotFound] = static detail => new NotFound(detail),
            [StatusCode.AlreadyExists] = static detail => new Conflict(detail),
            [StatusCode.Aborted] = static detail => new Conflict(detail),
            [StatusCode.FailedPrecondition] = static detail => new Conflict(detail),
            [StatusCode.PermissionDenied] = static detail => new PermissionDenied(detail),
            [StatusCode.Unauthenticated] = static detail => new Unauthenticated(detail),
            [StatusCode.ResourceExhausted] = static detail => new Exhausted(detail),
            [StatusCode.Internal] = static detail => new Internal(detail),
            [StatusCode.OutOfRange] = static detail => new OutOfRange(detail),
            [StatusCode.DataLoss] = static detail => new DataLoss(detail),
            [StatusCode.Unimplemented] = static detail => new Unimplemented(detail),
            [StatusCode.Unavailable] = static detail => new Unreachable(StatusCode.Unavailable, detail),
        }.ToFrozenDictionary();

    public static WireFault Classify(RpcException error) =>
        StatusRail.TryGetValue(error.StatusCode, out var make)
            ? make(error.Status.Detail)
            : new Unreachable(error.StatusCode, error.Status.Detail);
}

// The band-complete ComputeFault -> FaultDetail rail: ONE uniform pack over the Expected base, total over
// every 2200-band case by construction; the Bands registry mirrors the admission.md custody map so the
// analysis lane (2217-2219, AnalysisFailed in the 2219 slot) and the scheduling lane (2220 GraphCyclic)
// provably cross the wire, and a case code outside every band row fails the composition Probe.
public static class FaultWire {
    public const string Package = "rasm.compute";

    // Mirror of the admission custody map — a band change edits BOTH rows in one motion, never prose.
    public static readonly Seq<(int From, int To, string Lane)> Bands = Seq(
        (2200, 2212, "core"), (2213, 2216, "symbolic"), (2217, 2219, "analysis"), (2220, 2220, "scheduling"));

    public static FaultDetail Pack(ComputeFault fault, CorrelationId correlation, Instant hlc) =>
        new() {
            Package = Package, Code = fault.Code, Case = fault.GetType().Name, Message = fault.Message,
            Correlation = correlation.ToString(), HlcPhysical = hlc.ToTimestamp(), HlcLogical = 0,
        };

    public static Fin<Unit> Probe(int code) =>
        Bands.Exists(band => code >= band.From && code <= band.To)
            ? Fin.Succ(unit)
            : ComputeFault.Create($"<fault-wire-outside-band:{code}>");
}
```

[FaultDetail]:
- Fields: package=1 string; code=2 int32; case=3 string; message=4 string; evidence=5 map<string,string>; correlation=6 string; hlc_physical=7 google.protobuf.Timestamp; hlc_logical=8 uint64

## [05]-[TS_PROJECTION]

- Owner: `StreamKind`, `MethodShape`, `TransportCapabilityWire`, `TransportFramingWire`, `FaultDetailWire`, `ArtifactFrameWire`, `TransactionReceiptWire`, and the five service method-shape aliases — the TS posture for the whole suite wire including the flagship transaction-parity shape.
- Packages: BCL inbox
- Split law: the TS contract stays WHOLE on this page while the C# frame/channel mechanics (`FrameEdge`, `RemoteTransport`, `GrpcChannelPolicy`) live on `Runtime/transport#ARTIFACT_FRAMES`/`#TRANSPORT_AXIS` — `ArtifactFrameWire` and `TransportFramingWire` cite that frame law by PROSE ANCHOR, never a cross-split fence import.
- Growth: one method-shape row per new rpc and one field row per new evidence slot; zero new surface.
- Boundary: connect-es v2 `createClient` over `createGrpcWebTransport` consumes the app-root-emitted descriptor set through protoc-gen-es v2 single-plugin codegen — `binary` format with genuine binary server-streaming over Fetch, so the `application/grpc-web-text` base64 mode never enters; unary resolves as await and server-stream consumes as for-await; client-stream and bidi are structurally absent in the browser, so `ArtifactSyncShape.sync` (bidi) and `DocumentServiceShape.captureEvents` (clientStream) are wire shapes the browser transport cannot dial — the `TransportFramingWire.grpcWeb` tuple lists only the two shapes the binary frame carries; the flagship `executeTransaction` is a browser-dialable unary so the web tier commits a document transaction over the same parity contract the in-process and UDS legs use, and `TransactionReceiptWire` reconstructs the typed receipt field-for-field with the `conflict` slot as the literal-discriminated `FaultDetailWire` the in-band decode reads; the query read carries a `fieldMask` string array so a viewport tile requests only its rendered columns through the same `WireServices.Mask` paths; coalesced progress cadence is observer-side policy, never a wire knob; compression is a transport-level negotiation, never a per-method TS knob — the `grpc-internal-encoding-request` header is server-honored and the browser client reads the response `grpc-encoding` without a per-call selector; `FaultDetailWire` reconstructs the typed rail as a literal-discriminated union keyed by `case`.

```ts contract
type StreamKind = "unary" | "serverStream" | "clientStream" | "bidi";

interface MethodShape<K extends StreamKind, I extends string, O extends string> { kind: K; request: I; response: O; }

interface TransportCapabilityWire { http2: ["unary", "serverStream", "clientStream", "bidi"]; grpcWeb: ["unary", "serverStream"]; }

interface TransportFramingWire { http2: { mode: "binary"; carries: ["unary", "serverStream", "clientStream", "bidi"] }; grpcWeb: { mode: "binary"; mediaType: "application/grpc-web"; carries: ["unary", "serverStream"] }; }

type ComputeServiceShape = { infer: MethodShape<"unary", "InferRequest", "InferResponse">; progress: MethodShape<"serverStream", "ProgressRequest", "ProgressUpdate">; capabilities: MethodShape<"unary", "Empty", "ComputeCapabilities">; solve: MethodShape<"unary", "SolveRequest", "SolveResponse">; generate: MethodShape<"serverStream", "GenerateRequest", "TokenChunk">; graphDiff: MethodShape<"unary", "GraphDiffRequest", "GraphDiffResponse">; subtreeFetch: MethodShape<"serverStream", "SubtreeFetchRequest", "GraphChunk">; };

type DocumentServiceShape = { capabilities: MethodShape<"unary", "Empty", "DocumentCapabilities">; documentEvents: MethodShape<"serverStream", "WatchRequest", "DocumentEvent">; executeTransaction: MethodShape<"unary", "TransactionRequest", "TransactionReceipt">; query: MethodShape<"unary", "QueryRequest", "QueryResponse">; captureEvents: MethodShape<"clientStream", "CaptureFrame", "CaptureSummary">; };

type ControlServiceShape = { captureSupport: MethodShape<"unary", "Empty", "CaptureSupportReply">; setDegradation: MethodShape<"unary", "SetDegradationRequest", "DegradationReply">; reloadOptions: MethodShape<"unary", "Empty", "ReloadReply">; };

type ArtifactSyncShape = { sync: MethodShape<"bidi", "ArtifactFrame", "ArtifactFrame">; };

type HealthShape = { check: MethodShape<"unary", "HealthCheckRequest", "HealthCheckResponse">; watch: MethodShape<"serverStream", "HealthCheckRequest", "HealthCheckResponse">; };

interface FaultDetailWire { package: RasmPackage; code: number; case: string; message: string; evidence: Record<string, string>; correlation: string; hlcPhysical: string; hlcLogical: number; }

interface TransactionRequestWire { idempotencyKey: Uint8Array; ops: { typeUrl: string; value: Uint8Array }[]; expectedEpoch: number; hlcPhysical: string; hlcLogical: number; correlation: string; }

interface TransactionReceiptWire { idempotencyKey: Uint8Array; committed: boolean; newEpoch: number; applied: string[]; conflict: FaultDetailWire | null; hlcPhysical: string; hlcLogical: number; }

interface QueryRequestWire { scope: string; predicate: Record<string, unknown>; cursor: string; fieldMask: string[]; }

interface ArtifactFrameWire { artifactId: string; artifactBytes: number; offset: number; frameCrc: number; payload: Uint8Array; }
```

## [06]-[RESEARCH]

- [SPLAT_SCAN_WIRE]: the `GaussianSplatScan` ArtifactSync artifact message is MINTED — the SPZ v4 / SOG v2 stable published specifications ground the packed position/scale/rotation/spherical-harmonic buffer order, the harmonic-degree band, and the per-splat count, so the wire vocabulary is live and `Rasm.Compute/Runtime/payload#RESIDENCY` `SplatScan` admits it whole; the Python `realitycapture` companion's SPZ/SOG decode and the `xxhash` cp315 wheel stay named sibling-branch facts (the geometry campaign and the Forge scientific lane), and the render-side consumer leaf rides `csharp:Rasm.AppUi/Render/reality#SPLAT_SOURCE`.
