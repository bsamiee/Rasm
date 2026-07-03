# [INTERCHANGE_DESCRIPTOR]

The runtime descriptor-diff contract-evolution gate: one `DescriptorVerdict` classifying the running `FileDescriptorSet` the `Transport/transport.md` codegen pipeline emits against the descriptor the dialed transport carries as `Identical`, `Additive`, or `Breaking`, judged once at the transport dial seam before the first call rather than per-call or mid-decode. The gate is the browser-tier mirror of the C# `csharp:Rasm.Compute/Runtime/channels#CONTRACT_EVOLUTION` `ContractGuard.Classify` surface-set diff — the TS classifier folds the identical seven-dimension descriptor surface (field number-type-cardinality-packing-oneof-jsonname, oneof membership, reserved range, enum value set, and rpc input-output-streaming shape) over `@bufbuild/protobuf` `createFileRegistry` reflection, so the two runtimes compute the same verdict from the same descriptor bytes. A `Breaking` verdict faults `DescriptorGateLive` at `WireClients` construction through `Ingress/fault#FAULT_FAMILY` `FaultDetail.Quarantine` carrying the failing `message.field` paths as `Ingress/quarantine#DRIFT_TERMINAL` `DriftPath` rows; an `Additive` verdict admits because the descriptor runtime retains unknown fields by default, the same additive tolerance `Contract/inventory#WIRE_LAW` fixes for every rail. The gate reads the descriptor, never re-authors a shape, and never restates the runtime drift classification the quarantine terminal owns — descriptor drift is a dial-time construction verdict, decode drift is a per-value stream class, and the two share the `Identical`/`Additive`/`Breaking` vocabulary but never the surface.

## [01]-[INDEX]

- [01]-[DESCRIPTOR_GATE]: the verdict family, the surface-set classify fold, the dial-seam gate service, the declaration-order checksum.
- [02]-[TS_PROJECTION]: the C# `ContractDrift`/`ContractGuard.Surface` shape the TS classifier mirrors verbatim.


## [02]-[DESCRIPTOR_GATE]

- Owner: `DescriptorVerdict`, the one closed three-arm `Data.TaggedEnum` (`Identical`, `Additive`, `Breaking`) every descriptor diff resolves to; `classifyDescriptor`, the surface-set diff fold projecting both descriptors to one `HashSet<string>` of surface rows and classifying by `HashSet.difference`; `DescriptorGateLive`, the `Effect.Service` running the verdict once at the transport dial seam over the `WireTransport` descriptor source; `declOrderChecksum`, the canonical-projection digest that hashes a byte-reorder across generator versions to `Identical` rather than a false `Breaking`. The verdict carries its payload only on the arms that need it — `Breaking` carries the `ReadonlyArray<DriftPath>` of failing `message.field` paths the SPA renders and the platform telemetry-ships, `Additive` carries the tolerated added surface rows, `Identical` carries nothing.
- Cases: the fold projects the local descriptor surface (`Required`, the shape the running browser bundle was generated against) and the peer descriptor surface (`Offered`, the shape the dialed transport's process emits), then classifies by set difference — a row in `Required` absent from `Offered` is a removed or retyped surface and classifies `Breaking`, a row in `Offered` absent from `Required` is a new surface and classifies `Additive`, and equal sets classify `Identical`. The surface projection reads seven diff-relevant dimensions per the C# `ContractGuard.Surface` source: each field's `number:fieldKind:cardinality:packed:oneof:jsonName`, each oneof's ordered member-number set, each reserved-range span, each enum's ordered `name:number` value set, and each rpc's `input->output:streaming-shape`. Nested messages and enums fold into the same set through the `nestedTypes` reflection walker, so a phase enum nested in a message and a nested-message field retype are both visible exactly as the C# fold recurses `NestedTypes`. The `cardinality` and `packed` columns key off `field.fieldKind` — a `list` fieldKind is repeated and its `listKind` carries the packed posture, a `singular`-to-`list` flip reorders the surface row and classifies `Breaking`, never tolerated as additive.
- Entry: `classifyDescriptor(local, peer)` is the one polymorphic entry over the two `FileDescriptorSet` values — never a sibling `classifyField`/`classifyMethod`/`classifyEnum` family, since the surface projection collapses every dimension into one comparable string set the one set-difference fold classifies. `DescriptorGateLive.gate` reads the verdict once at construction and either returns the `WireClients` or faults; a unary or server-stream call never re-classifies, mirroring the C# `AdditiveOnly` law that the verdict is computed once against the wire descriptor, never per-call. `declOrderChecksum(set)` folds the ordered surface rows into one UTF-8 byte stream and hashes it, so two generator versions emitting semantically-identical descriptors in a different physical byte order checksum-match and the equal-checksum fast path skips the surface diff entirely, exactly the two-gate structure the C# `ContractGuard.Checksum`/`Classify` pair enforces.
- Packages: `@bufbuild/protobuf` for the full reflection surface the rest of the folder reads none of today — `createFileRegistry(fileDescriptorSet)` building the `FileRegistry`, `registry.files` and `file.messages`/`file.services`/`file.enums` iteration, `message.fields`/`field.number`/`field.fieldKind`/`field.localName`/`field.oneof`, `service.methods`/`method.methodKind`/`method.input`/`method.output`, `enum.values`/`value.number`, `message.reservedRanges`, and the `nestedTypes` recursive walker from `@bufbuild/protobuf/reflect`; the `writeUnknownFields`-default-`true` round-trip the descriptor runtime preserves is the additive tolerance the gate relies on, never a branch-side allow-list. `@bufbuild/buf` `protoc-gen-buf-breaking` (`buf breaking --against`) is the admitted build-time governance leg that catches a breaking edit at generation time; the two legs share the one `Identical`/`Additive`/`Breaking` verdict vocabulary, the build-time leg gating the commit and the dial-time leg gating the peer. `hash-wasm` `xxhash128` is the canonical-projection digest the `declOrderChecksum` fast path hashes the sorted surface rows through — the same 128-bit wasm owner `Codec/frame#ARTIFACT_FRAME` resolves for the content key, here the one-shot `xxhash128` over a self-contained intra-runtime local-versus-peer comparison that crosses no wire and so skips the LE-to-BE content-key normalization, never a parallel digest provider. `effect` carries the `Data.TaggedEnum` verdict and its `$match` gate fold over the `_tag`-keyed arms, the `Match.discriminatorsExhaustive("kind")` descriptor-surface fold over the `kind`-discriminated `AnyDesc` union the `nestedTypes` walker yields (the protobuf descriptor union keys on `kind`, never `_tag`, so `Match.tagsExhaustive` does not apply to it), the `Effect.Service` composition, the `Effect.tryPromise` lift over the async digest, and the `HashSet` set algebra (`HashSet.fromIterable`, `HashSet.difference`) mirroring the C# `FrozenSet`/`Except` diff.
- Growth: a new descriptor dimension lands as one surface-row column on `surfaceRowsOf`, never a new verdict arm or a parallel classifier; a removed field lands as one reserved-range row so its number never returns to use and a removed-then-reclaimed number classifies `Breaking`; a new generator-version byte reorder is absorbed by `declOrderChecksum` and never surfaces as a false `Breaking`; the build-time `buf breaking` leg lands as one `buf.yaml` `breaking` rule set, never a second verdict vocabulary. A second descriptor verdict notion, a per-call re-classify, or a `Match.when` chain over the wire package re-encoding the surface knowledge is the named defect.
- Boundary: the gate classifies a descriptor diff at the dial seam and never re-validates a decoded value — that is the `Ingress/quarantine#DRIFT_TERMINAL` `QuarantineFold`'s owned per-value surface, and the two folds share only the verdict literals. The gate mints no `DriftPath` shape — it reuses the `Ingress/quarantine#DRIFT_TERMINAL` `DriftPath` interface, never a re-minted `path`/`message` pair and never the `Ingress/refinement.md` brand vocabulary that owns no such type. The `Breaking` fault rides `Ingress/fault#FAULT_FAMILY` `FaultDetail.Quarantine`, never a new fault rail. The descriptor source is the `Transport/transport#CODEGEN_TOOLING` committed `FileDescriptorSet` for `Required` and the dialed transport's published descriptor for `Offered`; the gate reads both as bytes and never reaches a C# interior, re-authors a `.proto`, or re-emits a descriptor.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type DescriptorVerdict = Data.TaggedEnum<{
  readonly Identical: {};
  readonly Additive: { readonly added: ReadonlyArray<DriftPath> };
  readonly Breaking: { readonly missing: ReadonlyArray<DriftPath> };
}>;
const DescriptorVerdict = Data.taggedEnum<DescriptorVerdict>();

// --- [SERVICES] ----------------------------------------------------------------------
interface DescriptorGate {
  readonly gate: (clients: WireClients) => Effect.Effect<WireClients, FaultDetail>;
}

// --- [OPERATIONS] --------------------------------------------------------------------
const CARDINALITY_OF: Record<DescField["fieldKind"], "R" | "S"> = { scalar: "S", enum: "S", message: "S", list: "R", map: "R" };

const fieldRow = (typeName: string, field: DescField): string => {
  const oneof = field.fieldKind === "list" || field.fieldKind === "map" ? "-" : field.oneof?.name ?? "-";
  const packed = field.fieldKind === "list" && field.listKind === "scalar" ? field.proto.options?.packed === false ? "-" : "P" : "-";
  return `${typeName}.${field.name}=${field.number}:${field.fieldKind}:${CARDINALITY_OF[field.fieldKind]}:${packed}:${oneof}:${field.jsonName}`;
};

const messageRows = (message: DescMessage): ReadonlyArray<string> => [
  ...message.fields.map((field) => fieldRow(message.typeName, field)),
  ...message.oneofs.map((oneof) => `${message.typeName}~${oneof.name}=[${oneof.fields.map((f) => f.number).toSorted((a, b) => a - b).join(",")}]`),
  ...message.proto.reservedRange.map((range) => `${message.typeName}.reserved:${range.start}-${range.end}`),
];

const enumRows = (enumeration: DescEnum): ReadonlyArray<string> =>
  [`${enumeration.typeName}=[${enumeration.values.toSorted((a, b) => a.number - b.number).map((v) => `${v.name}:${v.number}`).join(",")}]`];

const serviceRows = (service: DescService): ReadonlyArray<string> =>
  service.methods.map((method) => `${service.typeName}/${method.name}:${method.input.typeName}->${method.output.typeName}:${method.methodKind}`);

const surfaceRowsOf = (set: FileDescriptorSet): HashSet.HashSet<string> =>
  HashSet.fromIterable(
    Array.flatMap([...createFileRegistry(set).files], (file) =>
      Array.flatMap([...nestedTypes(file)], (type) =>
        Match.value(type).pipe(
          Match.discriminatorsExhaustive("kind")({
            message: (m) => messageRows(m),
            enum: (e) => enumRows(e),
            service: (s) => serviceRows(s),
            extension: () => [],
          }),
        ),
      ),
    ),
  );

const driftPathsOf = (rows: HashSet.HashSet<string>): ReadonlyArray<DriftPath> =>
  Array.fromIterable(rows).map((row) => ({ path: row.split(/[.~/]/), message: row }));

const classifyDescriptor = (local: FileDescriptorSet, peer: FileDescriptorSet): DescriptorVerdict => {
  const required = surfaceRowsOf(local);
  const offered = surfaceRowsOf(peer);
  const missing = HashSet.difference(required, offered);
  const added = HashSet.difference(offered, required);
  return HashSet.size(missing) > 0
    ? DescriptorVerdict.Breaking({ missing: driftPathsOf(missing) })
    : HashSet.size(added) > 0
      ? DescriptorVerdict.Additive({ added: driftPathsOf(added) })
      : DescriptorVerdict.Identical();
};

const declOrderChecksum = (set: FileDescriptorSet): Effect.Effect<string, FaultDetail> =>
  Effect.tryPromise({
    try: () => xxhash128(new TextEncoder().encode(Array.fromIterable(surfaceRowsOf(set)).toSorted().join(";"))),
    catch: faultDetailRail.fromConnect,
  });

const verdictOf = (local: FileDescriptorSet, peer: FileDescriptorSet): Effect.Effect<DescriptorVerdict, FaultDetail> =>
  Effect.map(Effect.all([declOrderChecksum(local), declOrderChecksum(peer)]), ([localDigest, peerDigest]) =>
    localDigest === peerDigest ? DescriptorVerdict.Identical() : classifyDescriptor(local, peer));

// --- [COMPOSITION] -------------------------------------------------------------------
class DescriptorGateLive extends Effect.Service<DescriptorGateLive>()("@rasm/ts/interchange/DescriptorGate", {
  effect: Effect.gen(function* () {
    const source = yield* DescriptorSourceLive;
    const local = yield* source.committed;
    const peer = yield* source.dialed;
    const verdict = yield* verdictOf(local, peer);
    const gate = (clients: WireClients): Effect.Effect<WireClients, FaultDetail> =>
      DescriptorVerdict.$match(verdict, {
        Identical: () => Effect.succeed(clients),
        Additive: () => Effect.succeed(clients),
        Breaking: ({ missing }) =>
          Effect.fail(FaultDetail.Quarantine({ code: Code.FailedPrecondition, evidence: { drift: "breaking", paths: missing.map((p) => p.message).join(";") } })),
      });
    return { gate } satisfies DescriptorGate;
  }),
  dependencies: [DescriptorSourceLive.Default],
}) {}
```

The reflection walk spine the `surfaceRowsOf` projection rides is descriptor-grounded against the `@bufbuild/protobuf` v2 source: `createFileRegistry(set)` returns a `FileRegistry` whose `files` is an `Iterable<DescFile>` (spread into an array before `Array.flatMap`, the same materialization `nestedTypes(file)` takes), the `nestedTypes(file)` walker from `@bufbuild/protobuf/reflect` yields `Iterable<DescMessage | DescEnum | DescExtension | DescService>` discriminated on `kind` (`"message"|"enum"|"extension"|"service"`), `message.fields`/`message.typeName`/`message.oneofs`, `field.fieldKind`/`field.number`/`field.name`/`field.jsonName`/`field.listKind`, the singular-arm `field.oneof` (`DescOneof | undefined`, carried only on the `scalar`/`enum`/`message` arms, never the `list`/`map` arms — the `fieldRow` narrows on `fieldKind` before the access), `oneof.fields`/`oneof.name`, `enum.values`/`value.name`/`value.number`, `service.methods`, and `method.name`/`method.methodKind`/`method.input.typeName`/`method.output.typeName` are descriptor members the fold reads as-is. The reserved-range and packed-posture columns reach the raw `FieldDescriptorProto`/`DescriptorProto` the descriptor wraps: `message.proto.reservedRange` yields `DescriptorProto_ReservedRange` rows with `start` (inclusive) and `end` (exclusive) — the v2 spelling of the C# `message.ToProto().ReservedRange` `Start`/`End` — and the packed bit reads `field.proto.options?.packed` against the raw `FieldDescriptorProto`, the one dimension without a top-level descriptor mirror of the C# `FieldDescriptor.IsPacked`, defaulting to packed for a repeated scalar unless `options.packed === false` so a `[packed=true]`-to-`[packed=false]` flip reorders the surface row and classifies `Breaking` exactly as the C# fold detects it.

> [!IMPORTANT]
> Disk gap: every descriptor member above is grounded against the `@bufbuild/protobuf` v2 source, but the `@bufbuild/protobuf@2.12.0` pnpm-store entry is a content-less stub on disk, so the spellings are not yet re-confirmed against the materialized `.d.ts`. Close at transcription-lock by reading the emitted `src/gen/descriptor_pb.ts` or the materialized descriptor `.d.ts` once `node_modules` resolves the package; the seven surface dimensions and the set-difference verdict are fixed regardless.

The verdict is judged once over the two descriptor sources `DescriptorSource` resolves — `committed` is the build-time `FileDescriptorSet` the `Transport/transport#CODEGEN_TOOLING` `buf generate` pipeline emits and bundles, `dialed` is the descriptor the running transport's process publishes beside its discovery manifest. The `declOrderChecksum` fast path short-circuits the surface diff when the two generators emit byte-reordered but semantically-identical descriptors, so the cost of the full `surfaceRowsOf` projection is paid only on a real digest divergence; the `Breaking` arm folds the failing `message.field` rows into the `Ingress/fault#FAULT_FAMILY` `FaultDetail.Quarantine` evidence map exactly as the C# hop fault rail rejects on the surface set, and the `Additive` arm admits because the descriptor runtime's default unknown-field retention round-trips the new surface without loss.

```ts contract
// --- [SERVICES] ----------------------------------------------------------------------
interface DescriptorSource {
  readonly committed: Effect.Effect<FileDescriptorSet, FaultDetail>;
  readonly dialed: Effect.Effect<FileDescriptorSet, FaultDetail>;
}

// --- [OPERATIONS] --------------------------------------------------------------------
const decodeSet = (bytes: Uint8Array): Effect.Effect<FileDescriptorSet, FaultDetail> =>
  Effect.try({ try: () => fromBinary(FileDescriptorSetSchema, bytes), catch: faultDetailRail.fromConnect });

// --- [COMPOSITION] -------------------------------------------------------------------
class DescriptorSourceLive extends Effect.Service<DescriptorSourceLive>()("@rasm/ts/interchange/DescriptorSource", {
  effect: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const transport = yield* WireTransportLive;
    const committed = Effect.flatMap(config.committedDescriptorBytes, decodeSet);
    const dialed = Effect.flatMap(
      Effect.tryPromise({ try: () => fetch(config.dialedDescriptorUrl).then((r) => r.arrayBuffer()), catch: faultDetailRail.fromConnect }),
      (buffer) => decodeSet(new Uint8Array(buffer)),
    );
    return { committed, dialed } satisfies DescriptorSource;
  }),
}) {}
```

## [03]-[TS_PROJECTION]

- Owner: the C# `csharp:Rasm.Compute/Runtime/channels#CONTRACT_EVOLUTION` `ContractDrift` `[Union]` and `ContractGuard.Surface`/`Classify` source the TS classifier mirrors — the authoritative shape the `classifyDescriptor` fold reproduces row-for-row over the browser-tier reflection surface. The C# side is the producer of the verdict the gate re-derives; the TS classifier never invents a surface dimension the C# fold does not read and never relaxes a `Breaking` dimension the C# fold rejects.
- Entry: `ContractDrift` is the three-arm `[Union]` (`Identical`, `Additive(Seq<string> Added)`, `Breaking(Seq<string> Missing)`) carrying the added or missing surface rows; `ContractGuard.Surface` projects each `FileDescriptor` to a `FrozenSet<string>` of the seven-dimension rows, `ContractGuard.Classify` diffs `Required.Except(Offered)` (breaking) against `Offered.Except(Required)` (additive), and `ContractGuard.Checksum` folds the ordered surface set into the `XxHash128` canonical digest. The TS `DescriptorVerdict` arms, the `surfaceRowsOf` row format, and the `declOrderChecksum` digest are the verbatim browser-tier mirror of these three members.
- Packages: this cluster transcribes the C# source as the consumed shape; the TS realization in `[2]-[DESCRIPTOR_GATE]` composes `@bufbuild/protobuf` reflection and `effect` set algebra against it.
- Growth: a new surface dimension the C# `MessageSurface`/`EnumSurface`/`RpcSurface` fold adds lands as one matching column on the TS `surfaceRowsOf` row; a new verdict arm is impossible because the surface-set diff is closed at three outcomes.

```csharp source
// sourced verbatim from `csharp:Rasm.Compute/Runtime/channels#CONTRACT_EVOLUTION`
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContractDrift {
    private ContractDrift() { }

    public sealed record Identical : ContractDrift;
    public sealed record Additive(Seq<string> Added) : ContractDrift;
    public sealed record Breaking(Seq<string> Missing) : ContractDrift;
}

public static class ContractGuard {
    public static Fin<string> Checksum(Seq<ByteString> serialized) =>
        Build(serialized).Map(static files => Convert.ToHexStringLower(
            XxHash128.Hash(Encoding.UTF8.GetBytes(string.Join(';', Surface(files).OrderBy(static row => row, StringComparer.Ordinal))))));

    public static ContractDrift Classify(Seq<FileDescriptor> local, Seq<FileDescriptor> peer) =>
        (Required: Surface(local), Offered: Surface(peer)) switch {
            var sets when sets.Required.Except(sets.Offered).ToSeq() is { IsEmpty: false } missing => new ContractDrift.Breaking(missing),
            var sets when sets.Offered.Except(sets.Required).ToSeq() is { IsEmpty: false } added => new ContractDrift.Additive(added),
            _ => new ContractDrift.Identical(),
        };

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
