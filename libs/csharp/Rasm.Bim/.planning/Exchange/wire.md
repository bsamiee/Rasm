# [BIM_WIRE]

`IfcWire` is the cross-runtime IFC interchange wire: one raw artifact carrying the format key, IFC bytes, schema key, semantic `ContentAddress.OfGraph`, and mint instant. `SemanticProjector.Emit` re-authors an `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` to a GeometryGym serialization, and every serialization of one model shares the semantic address before re-admission through `IfcWire.Admit`.

`Rasm.Bim` owns GeometryGym alone, so the IFC bytes ARE the BIM wire: the `python:geometry/ifc-companion` ifcopenshell peer and the TypeScript web peer decode the same serialization Bim emits, never re-minting a parallel BIM shape.

Seam-graph interchange stays out of Bim: the `ElementGraph`/`GraphDelta` snapshot (`json-stj`/`cbor`/`messagepack` with the op-log change stream) is `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s and Version owner, and the gRPC service descriptor is an APP-PLATFORM transport concern — this page owns ONLY the IFC interchange wire. Generic-model STJ serializers and gRPC descriptors inside an AEC-domain package are strata leaks; "one model, many faces" survives DISTRIBUTED by stratum, never consolidated in Bim.

`IfcWire` is HOST-FREE — no RhinoCommon type, no host-bound geometry, only IFC bytes and the content-key the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store and the seam graph share — and deserialization is admission, faulting at the boundary so a malformed wire payload never mints a half-built graph.

## [01]-[INDEX]

- [02]-[WIRE_PROJECTION]: `IfcWire`, the content-keyed IFC interchange artifact (`Seal` egress, `Admit` ingress, `Negotiate` serialization selection) composing `SemanticProjector.Emit`/`Project` and `ProjectionAssembly.Assemble`, with the `WireParity` cross-runtime golden-corpus leg.

## [02]-[WIRE_PROJECTION]

- Owner: `IfcWire` the content-keyed cross-runtime IFC interchange artifact, minted through its ONE `Of` admission and structurally equatable over its byte column through the `WireBytes` comparer `[CustomEquality]` seats — the IFC serialization bytes, the seam `ContentAddress` graph identity, and the `Rasm.Element/Graph/element#NODE_MODEL` `ReleaseVersion` schema stamp (the `docs/stacks/csharp/domain/data-interchange#ARTIFACT_IDENTITY` "payload + descriptor stamp + content hash" law applied to IFC); `WireParity` the IFC-wire leg of the cross-runtime golden corpus (`tests/contracts/MANIFEST.md` `IFC_WIRE`).
- Entry: `IfcWire.Of(format, bytes, schema, content, at, key)` is the ONE construction — the record's ctor is private, so every wire in the estate resolved its format row and carried a payload BY CONSTRUCTION and no interior member re-tests a column; `IfcWire.Seal(SemanticProjector projector, ElementGraph graph, InterchangeFormat format, Option<EmitContext> context, Instant at, Op key)` is the producer egress (the profiles resolver is the projector's ctor-held capability, never a `Seal` parameter — the `EmitContext` carrier rides through whole, so a diff-prior, a scoped trade-package slice, or a declared unit regime wires with zero `Seal` edits) and `IfcWire.Admit(ProjectionContext ctx, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` the consumer ingress; `IfcWire.Negotiate(Seq<string> accepted, Op key)` resolves the highest-fidelity IFC serialization a peer admits. `Fin<T>` aborts on a non-round-trippable `format` row (`Model/faults#FAULT_BAND` `wire-encode`), on the `Projection/egress#IFC_EGRESS` gate faults `SemanticProjector.Emit` raises, and on a malformed-bytes decode or an `IfcLegality`-rejected projection (`wire-decode`) — each typed `BimFault` case lifting BARE onto the rail with no `.ToError()` hop, the gate vocabulary itself owned by the egress and legality pages rather than restated here. Artifact identity is the SEMANTIC graph address, never a positional DTO and never the byte hash.
- Auto: `Seal` re-authors the graph through `SemanticProjector.Emit` at the `Projection/wireform#IFC_WIRE_FORM` `IfcWireForm` the row resolves — the form's own seal writing the container and handing back BYTES this seam stores whole — stamps the wire-form-INDEPENDENT `ContentAddress.OfGraph(graph)` and the `graph.Header.Schema`, so a STEP and an ifcJSON of one model carry one `Content` and a peer joins them; `Admit` decodes through the ONE GeometryGym decode owner — `Exchange/import#IMPORT_RAIL` `BimIo.ImportIfc`, the schema sniffed off the bytes BEFORE construction — hands a fresh `SemanticProjector` to `ProjectionAssembly.Assemble` over an `ElementGraph.Genesis(ctx.Header)` seed (the projector's own `GraphDelta.Reheader` overriding the seed header), and runs the `IfcLegality` IFC-semantic legality (the relationship law with the vocabulary arms) so an illegal or out-of-roster projection never freezes a graph; `Negotiate` folds the IFC `InterchangeFormat` rows by the `IfcWireForm.FidelityRank` column so a peer that reads only ifcJSON receives ifcJSON without a call-site branch.
- Receipt: `IfcWire` is the one cross-runtime IFC contract — the ifcopenshell companion and the web peer decode the same bytes the C# branch emits; the `Content` joins the artifact to the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store (the `RepresentationContentHash` body keys inside the graph are cross-runtime stable) and the seam graph the `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec` persists; `WireParity` carries the cross-runtime contract as the seam `Content` (`Agrees` — a peer that decodes the same bytes and projects its OWN graph computes the same `ContentAddress`) and the C#-host re-seal byte golden as `Reproduces` (host-local emit determinism), so a cross-runtime peer is checked by `Agrees` and never by a byte compare (the GeometryGym/ifcopenshell/web serializers emit divergent byte layouts for one graph).
- Packages: GeometryGymIFC_Core, Generator.Equals, Rasm.Element, LanguageExt.Core, NodaTime, Rasm
- Growth: a new admitted column on the artifact is one `Validation` clause on `Of`, so a caller handing two bad columns reads both refusals; a new IFC serialization GeometryGym emits is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `GeometryGym` codec — the DERIVED `Serializations` and `Negotiate` admit it with NO wire edit, the row-promotion discipline applied to the wire; a new wire form — a genuinely new serialization KIND beyond STEP/XML/JSON, or a new CONTAINER over a landed one — is one `Projection/wireform#IFC_WIRE_FORM` row on the owning axis, the serialization carrying the `FidelityRank` a container form inherits by repeating it, so the negotiation fold here takes it with zero edit and only its `Exchange/import#IMPORT_RAIL` decode arm lands (the wire carries no decode fence); a `CataloguePending` row (the `ifc5` row until an IFC5/IFCX toolkit lands) is excluded by the codec filter rather than advertised as sealable; a new peer is one decoder aligning to the IFC bytes (never a new wire owner); the artifact identity is the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph`, so a new content-stable rule is one clause on the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`, never a second wire hasher; the seam-graph snapshot/delta wire grows in `Rasm.Persistence` and the gRPC descriptor in the APP-PLATFORM transport owner, never here.
- Boundary: this page carries NO decode fence — the bytes→`DatabaseIfc` admission IS `Exchange/import#IMPORT_RAIL` `BimIo.ImportIfc` composed under the `wire-decode` admission context, so a second decode beside the import rail is the deleted form and a hand-constructed non-IFC `IfcWire` faults at `Admit` through that owner's own codec gate. Rooted `NodeId` stays LOCAL — a fresh `Guid`-v7 per ingest, the compressed IFC `GlobalId` riding `Node.Object.ExternalId` for re-ingest correlation — so a re-admitted wire re-mints rooted ids, a "rooted address round-trips across runtimes" claim is the deleted form, and cross-runtime parity runs over that correlation and the content-keyed non-rooted `Material`/`PropertySet`/representation nodes. `ExportArtifact`, the geometry-bearing GLB byte-keyed emit, is `Exchange/export#EXPORT_RAIL`'s and distinct: byte identity, never graph identity.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using GeneratorEquals;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Bim.Model;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;   // both imported namespaces declare the name; the seam one is the wire stamp

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// ReadOnlyMemory<byte> under synthesized record equality compares the HANDLE — object reference, offset, length —
// so two byte-identical wires of one graph read UNEQUAL and every memo, parity probe, and re-seal guard keyed on
// that record misses. WireBytes moves the column onto a span compare and hashes through the ONE kernel
// seed-zero digest the parity row already mints, so the cheap pre-test and the exact answer share a hasher.
public sealed class WireBytes : IEqualityComparer<ReadOnlyMemory<byte>> {
    public static readonly WireBytes Default = new();

    public bool Equals(ReadOnlyMemory<byte> left, ReadOnlyMemory<byte> right) => left.Span.SequenceEqual(right.Span);

    public int GetHashCode(ReadOnlyMemory<byte> value) => ContentHash.Of(value.Span).GetHashCode();
}

// IfcWire stores InterchangeFormat.Key because the rich format row carries host-local codec and capability state.
// One ElementGraph emits to every GeometryGym IFC serialization ALL sharing one Content, because identity is the
// SEMANTIC graph address, never the byte hash — so a STEP and an ifcJSON of one model join on one key.
[Equatable]
public sealed partial record IfcWire {
    private IfcWire(string format, ReadOnlyMemory<byte> bytes, ReleaseVersion schema, ContentAddress content, Instant at) =>
        (Format, Bytes, Schema, Content, At) = (format, bytes, schema, content, at);

    public string Format { get; }

    [CustomEquality(typeof(WireBytes))]
    public ReadOnlyMemory<byte> Bytes { get; }

    public ReleaseVersion Schema { get; }
    public ContentAddress Content { get; }
    public Instant At { get; }

    public long ByteCount => Bytes.Length;

    // ONE admission for every column a caller supplies, ACCUMULATING across the two that can refuse: the format key
    // must resolve to a rostered row under ordinal identity, and the payload must carry bytes. Schema, Content, and
    // At are typed non-nullable columns with nothing left to prove, which is precisely why the two null-shaped
    // guards this replaces could never fire — they tested positional columns the type system already closed, and
    // their interior readers treated absence as reachable.
    public static Fin<IfcWire> Of(
        string format, ReadOnlyMemory<byte> bytes, ReleaseVersion schema, ContentAddress content, Instant at, Op key) =>
        (Rostered(format, key), Payload(bytes, format, key))
            .Apply((row, payload) => new IfcWire(row.Key, payload, schema, content, at)).As().ToFin();

    // Ordinal key identity keeps the wire canonical where Detect serves path, extension, and media-type ingress: a
    // wire column is the row's OWN key, never a case-folded or extension-shaped spelling of it.
    static Validation<Error, InterchangeFormat> Rostered(string value, Op key) =>
        InterchangeFormat.Items.Find(row => StringComparer.Ordinal.Equals(value, row.Key))
            .ToValidation<Error>(new BimFault.Refused(key, BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "interchange-format-miss", value })));

    static Validation<Error, ReadOnlyMemory<byte>> Payload(ReadOnlyMemory<byte> bytes, string format, Op key) =>
        bytes.IsEmpty
            ? Validation<Error, ReadOnlyMemory<byte>>.Fail(new BimFault.Refused(key, BimScope.Wire, BimReason.Rejected, string.Join(':', new object?[] { "wire-encode", format })))
            : Validation<Error, ReadOnlyMemory<byte>>.Success(bytes);

    // Producer egress: re-author the graph to IFC bytes through SemanticProjector.Emit, stamp the wire-form
    // INDEPENDENT seam graph content-address and the Header schema. Emit's emittable gate IS the
    // format#FORMAT_AXIS Serialization column filtered by RoundTrippable, because a sealed wire must re-admit
    // through the SAME row — so a None column or a one-way row faults wire-encode. Emit already returns BYTES (the
    // row's own seal writes the container), so this seam stores the memory whole and no re-encode hop exists.
    public static Fin<IfcWire> Seal(
        SemanticProjector projector, ElementGraph graph, InterchangeFormat format,
        Option<EmitContext> context, Instant at, Op key) =>
        format.Serialization.Filter(_ => format.RoundTrippable).Match(
            Some: form => projector.Emit(graph, form, key, context).Bind(bytes =>
                Of(format.Key, bytes, graph.Header.Schema, ContentAddress.OfGraph(graph), at, key)),
            None: () => Fin.Fail<IfcWire>(new BimFault.Refused(key, BimScope.Wire, BimReason.Rejected, string.Join(':', new object?[] { "wire-encode", format.Key }))));

    // Admission is a DEPENDENCE chain end to end — the decode needs the resolved row, the assembly needs the
    // database, the header check needs the assembled graph — so it sequences on Fin and the independent columns
    // accumulate at Of instead. The format read is total against that admission.
    public Fin<ElementGraph> Admit(ProjectionContext ctx, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        from format in Rostered(Format, ctx.Key).ToFin()
        from db in BimIo.ImportIfc(format, Bytes, ctx.Key)

        from assembled in ProjectionAssembly.Assemble(
            ProjectionSuite.Of(
                Seq<IElementProjection>(new SemanticProjector(db, reconciler, profiles)),
                Seq(ConstraintRegistration.Of(new IfcLegality()))),
            ElementGraph.Genesis(ctx.Header), ctx)
        let graph = assembled.Graph
        from _ in guard(
            graph.Header.Schema == Schema,
            (Error)new BimFault.Refused(ctx.Key, BimScope.Wire, BimReason.Rejected, string.Join(':', new object?[] { "wire-decode", $"schema:{Schema.Key}:{graph.Header.Schema.Key}" })))
        select graph;

    // Content negotiation across the IFC serializations a peer admits (STEP > ifcXML > ifcJSON by interop breadth)
    // — the data-interchange "fidelity routes the format" law. An empty intersection faults rather than silently
    // defaulting to STEP, and Negotiate offers ONLY sealable rows, so a negotiated format always Seals. The
    // accepted set is hashed ONCE at the boundary: the pair of linear Contains probes it replaces re-scanned the
    // caller's sequence twice per rostered row.
    public static Fin<InterchangeFormat> Negotiate(Seq<string> accepted, Op key) =>
        Mutual(toHashSet(accepted)).ToFin(new BimFault.Refused(key, BimScope.Wire, BimReason.Codec, string.Join(':', new object?[] { "wire-no-mutual", string.Join(',', accepted) })));

    static Option<InterchangeFormat> Mutual(HashSet<string> offered) =>
        Serializations.Value.Find(f => offered.Contains(f.Key) || offered.Contains(f.MediaType));

    // GeometryGym-emittable IFC wire forms, highest interop fidelity first — DERIVED from the format#FORMAT_AXIS
    // Serialization column plus export capability, so a future IFC5/IFCX codec admission joins the wire with NO
    // edit here. IFC5 is ABSENT until then because GeometryGym reads and writes IFC2x3-IFC4.x only, and
    // enumerating the pending ifc5 row would advertise a wire form Seal cannot produce. FidelityRank is a COLUMN
    // on the Projection/wireform#IFC_WIRE_FORM row that owns serialization and container, so this fold READS the
    // owning row's rank rather than re-switching on a serialization kind. Choose fuses the filter and the rank
    // read, which is what retires the unreachable int.MaxValue sort sentinel the two-step spelling carried behind
    // its own IsSome test. A container form ranks with the serialization it repeats, so plain and zipped STEP tie
    // and the stable sort holds InterchangeFormat.Items roster order, bare form first.
    static readonly Lazy<Seq<InterchangeFormat>> Serializations = new(static () =>
        toSeq(InterchangeFormat.Items
            .Choose(static f => f.Serialization.Filter(_ => f.RoundTrippable).Map(form => (Row: f, form.FidelityRank)))
            .OrderBy(static pair => pair.FidelityRank)
            .Select(static pair => pair.Row)),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

// IFC-wire leg of the cross-runtime golden corpus: the seam graph content-key (the cross-runtime CONTRACT) with the
// C#-host IFC-bytes golden. A cross-runtime BYTE-equality claim is the deleted form — GeometryGym, ifcopenshell,
// and web serializers emit divergent byte layouts for one graph, so the byte golden never crosses runtimes.
public sealed record WireParity(string Corpus, ContentAddress GraphKey, UInt128 GoldenBytes, long ByteCount) {
    // Byte goldens mint through the ONE kernel seed-zero ContentHash the semantic key already rides: the two are
    // different QUESTIONS over one hasher, so a second digest scheme beside it forks the content space this
    // package's own ruling seals to one.
    public static WireParity Of(string corpus, IfcWire wire) =>
        new(corpus, wire.Content, ContentHash.Of(wire.Bytes.Span), wire.ByteCount);

    public bool Agrees(IfcWire wire) => wire.Content == GraphKey;

    public bool Reproduces(IfcWire wire) =>
        wire.ByteCount == ByteCount && ContentHash.Of(wire.Bytes.Span) == GoldenBytes;
}
```

## [03]-[RESEARCH]

(none)
