# [BIM_WIRE]

`IfcWire` is the cross-runtime IFC interchange wire: one content-keyed artifact carrying an `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` re-authored to the IFC serializations GeometryGym emits through the Bim-internal `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, stamped with the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` so EVERY IFC serialization of one model shares one identity, and re-admitted through `IfcWire.Admit` — the `Exchange/import#IMPORT_RAIL` `BimIo.ImportIfc` schema-sniffed decode, `SemanticProjector.Project`, then `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` under the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` constraint. `Rasm.Bim` owns GeometryGym alone, so the IFC bytes ARE the BIM wire: the `python:geometry/ifc-companion` ifcopenshell peer and the TypeScript web peer decode the same serialization Bim emits, never re-minting a parallel BIM shape.

Seam-graph interchange stays out of Bim: the `ElementGraph`/`GraphDelta` snapshot (`json-stj`/`cbor`/`messagepack` with the op-log change stream) is `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s and Version owner, and the gRPC service descriptor is an APP-PLATFORM transport concern — this page owns ONLY the IFC interchange wire. A generic-model STJ serializer and a gRPC descriptor inside an AEC-domain package are strata leaks, so the retired `BimModel`/`BimElement` snapshot wire, its `[SmartEnum]` `BimWireFace`, and the `BimWireContext` source-generated `JsonSerializerContext` are GONE; "one model, many faces" survives DISTRIBUTED by stratum, never consolidated in Bim. `IfcWire` is HOST-FREE — no RhinoCommon type, no host-bound geometry, only IFC bytes and the content-key the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store and the seam graph share — and deserialization is admission, faulting at the boundary so a malformed wire payload never mints a half-built graph.

## [01]-[INDEX]

- [02]-[WIRE_PROJECTION]: `IfcWire`, the content-keyed IFC interchange artifact (`Seal` egress, `Admit` ingress, `Negotiate` serialization selection) composing `SemanticProjector.Emit`/`Project` and `ProjectionAssembly.Assemble`, with the `WireParity` cross-runtime golden-corpus leg.

## [02]-[WIRE_PROJECTION]

- Owner: `IfcWire` the content-keyed cross-runtime IFC interchange artifact — the IFC serialization bytes, the seam `ContentAddress` graph identity, and the `Rasm.Element/Graph/element#NODE_MODEL` `ReleaseVersion` schema stamp (the `docs/stacks/csharp/domain/data-interchange#ARTIFACT_IDENTITY` "payload + descriptor stamp + content hash" law applied to IFC); `WireParity` the IFC-wire leg of the cross-runtime golden corpus (`tests/contracts/MANIFEST.md` `IFC_WIRE`).
- Entry: `IfcWire.Seal(SemanticProjector projector, ElementGraph graph, InterchangeFormat format, Option<EmitContext> context, Instant at, Op key)` is the producer egress (the profiles resolver is the projector's ctor-held capability, never a `Seal` parameter — the `EmitContext` carrier rides through whole, so a diff-prior, a scoped trade-package slice, or a declared unit regime wires with zero `Seal` edits) and `IfcWire.Admit(ProjectionContext ctx, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` the consumer ingress — `Fin<T>` aborts on a non-IFC `format` (`Model/faults#FAULT_BAND` `BimFault.ModelRejected` `wire-encode`) or a `SemanticProjector.Emit` egress-gate fault on the egress — the `Projection/egress#IFC_EGRESS` per-token gate rails `BimFault.UnmappedClass` on a `class-out-of-schema` row, a `predefined-out-of-schema`/`predefined-reject` token [PREDEFINED_TOKEN_RULING], and an `Instantiable: false` abstract class (`abstract-class-at-egress`), and the frozen `ReleaseMap` rails `BimFault.CodecReject` on an unmapped schema member, the silent `IFC4X3_ADD2` fallback deleted — and on a malformed-bytes decode or an `IfcLegality`-rejected projection (`BimFault.ModelRejected` `wire-decode`, the wire-admission arm; the legality gate carries the two vocabulary arms `vocabulary-class-miss`/`vocabulary-token-reject` beside the relationship law) on the ingress — each typed `BimFault` case (band 2600, `Expected`-derived) lifting BARE onto the `Fin<T>` rail with no `.ToError()` hop; `IfcWire.Negotiate(Seq<string> accepted, Op key)` resolves the highest-fidelity IFC serialization a peer admits. Artifact identity is the SEMANTIC graph address (`ContentAddress.OfGraph`), never a positional DTO and never the byte hash.
- Auto: `Seal` re-authors the graph through `SemanticProjector.Emit` at the `Projection/egress#IFC_EGRESS` `IfcWireForm` the row resolves — the form's own seal writing the container and handing back BYTES this seam stores whole — stamps the wire-form-INDEPENDENT `ContentAddress.OfGraph(graph)` and the `graph.Header.Schema`, so a STEP and an ifcJSON of one model carry one `Content` and a peer joins them; `Admit` decodes through the ONE GeometryGym decode owner — `Exchange/import#IMPORT_RAIL` `BimIo.ImportIfc`, the schema sniffed off the bytes BEFORE construction — hands a fresh `SemanticProjector` to `ProjectionAssembly.Assemble` over an `ElementGraph.Genesis(ctx.Header)` seed (the projector's own `GraphDelta.Reheader` overriding the seed header), and runs the `IfcLegality` IFC-semantic legality (the relationship law with the vocabulary arms) so an illegal or out-of-roster projection never freezes a graph; `Negotiate` folds the IFC `InterchangeFormat` rows by the `IfcWireForm.FidelityRank` column so a peer that reads only ifcJSON receives ifcJSON without a call-site branch.
- Receipt: `IfcWire` is the one cross-runtime IFC contract — the ifcopenshell companion and the web peer decode the same bytes the C# branch emits; the `Content` joins the artifact to the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store (the `RepresentationContentHash` body keys inside the graph are cross-runtime stable) and the seam graph the `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec` persists; `WireParity` carries the cross-runtime contract as the seam `Content` (`Agrees` — a peer that decodes the same bytes and projects its OWN graph computes the same `ContentAddress`) and the C#-host re-seal byte golden as `Reproduces` (host-local emit determinism), so a cross-runtime peer is checked by `Agrees` and never by a byte compare (the GeometryGym/ifcopenshell/web serializers emit divergent byte layouts for one graph).
- Packages: GeometryGymIFC_Core, Rasm.Element, LanguageExt.Core, NodaTime, Rasm
- Growth: a new IFC serialization GeometryGym emits is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `GeometryGym` codec — the DERIVED `Serializations` and `Negotiate` admit it with NO wire edit, the row-promotion discipline applied to the wire; a new wire form — a genuinely new serialization KIND beyond STEP/XML/JSON, or a new CONTAINER over a landed one — is one `Projection/egress#IFC_EGRESS` `IfcWireForm` row named as that format row's `Serialization` value and carrying its own `FidelityRank` column, a container form ranking with the serialization it repeats, so the negotiation fold here takes it with zero edit and only its `Exchange/import#IMPORT_RAIL` decode arm lands (the wire carries no decode fence); a `CataloguePending` row (the `ifc5` row until an IFC5/IFCX toolkit lands) is excluded by the codec filter rather than advertised as sealable; a new peer is one decoder aligning to the IFC bytes (never a new wire owner); the artifact identity is the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph`, so a new content-stable rule is one clause on the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter`, never a second wire hasher; the seam-graph snapshot/delta wire grows in `Rasm.Persistence` and the gRPC descriptor in the APP-PLATFORM transport owner, never here.
- Boundary: this page carries NO decode fence — the bytes→`DatabaseIfc` admission IS `Exchange/import#IMPORT_RAIL` `BimIo.ImportIfc` composed under the `wire-decode` admission context, so a second decode beside the import rail is the deleted form and a hand-constructed non-IFC `IfcWire` faults at `Admit` through that owner's own codec gate. Rooted `NodeId` stays LOCAL — a fresh `Guid`-v7 per ingest, the compressed IFC `GlobalId` riding `Node.Object.ExternalId` for re-ingest correlation — so a re-admitted wire re-mints rooted ids, a "rooted address round-trips across runtimes" claim is the deleted form, and cross-runtime parity runs over that correlation and the content-keyed non-rooted `Material`/`PropertySet`/representation nodes. `ExportArtifact`, the geometry-bearing GLB byte-keyed emit, is `Exchange/export#EXPORT_RAIL`'s and distinct: byte identity, never graph identity.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Bim.Model;                                       // BimFault + the Detail roster the wire raises through
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;   // both imported namespaces declare the name; the seam one is the wire stamp

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// IfcWire is the cross-runtime IFC interchange artifact: IFC serialization bytes + the seam graph content-address
// identity + the schema stamp (the data-interchange "payload + descriptor stamp + content hash" law). One
// ElementGraph emits to every GeometryGym IFC serialization (STEP/ifcXML/ifcJSON) ALL sharing one Content, because
// identity is the SEMANTIC graph address (ContentAddress.OfGraph), never the byte hash — so a STEP and an ifcJSON
// of one model join on one key. HOST-FREE: only IFC bytes + the content-key the Compute geometry-blob store and the
// seam graph share, never a RhinoCommon type.
public sealed record IfcWire(
    InterchangeFormat Format,
    ReadOnlyMemory<byte> Bytes,
    ReleaseVersion Schema,
    ContentAddress Content,
    Instant At) {

    public long ByteCount => Bytes.Length;

    // Producer egress: re-author the graph to IFC bytes through the Bim-internal SemanticProjector.Emit, stamp the
    // wire-form-INDEPENDENT seam graph content-address and the Header schema. Emit's emittable gate IS the
    // format#FORMAT_AXIS Serialization column — an Option<IfcWireForm> carrying serialization AND container on one row,
    // Some exactly on the GeometryGym rows — filtered by RoundTrippable, because a sealed wire must re-admit
    // through the SAME row, so a None column or a one-way row faults
    // ModelRejected (wire-encode). Emit already returns BYTES (the row's own seal writes the container), so this seam
    // stores the memory whole and no re-encode hop exists; an Emit gate fault rails through: the per-token
    // UnmappedClass span gate [PREDEFINED_TOKEN_RULING], the abstract-class-at-egress check, the ReleaseMap
    // CodecReject on an unmapped schema member. Emit builds its own target database and ignores the projector's captured ingress db (the profiles
    // resolver rides the projector ctor, never a Seal parameter), so the same projector that imported a model (Project)
    // re-emits it (Emit) and a from-scratch authoring path supplies any instance.
    public static Fin<IfcWire> Seal(
        SemanticProjector projector, ElementGraph graph, InterchangeFormat format,
        Option<EmitContext> context, Instant at, Op key) =>
        format.Serialization.Filter(_ => format.RoundTrippable).Match(
            Some: form => projector.Emit(graph, form, key, context).Map(bytes =>
                new IfcWire(format, bytes, graph.Header.Schema, ContentAddress.OfGraph(graph), at)),
            None: () => Fin.Fail<IfcWire>(Detail.WireEncode.At(key, format.Key)));

    // Consumer admission: IFC bytes decode through the ONE GeometryGym decode owner — the import rail's
    // BimIo.ImportIfc, schema sniffed before construction — re-wrapped with the wire-decode admission context
    // (codec gate and parse funnel both land here), then a fresh SemanticProjector projects and Assemble runs
    // IfcLegality (the relationship law with the two vocabulary arms), so a malformed, non-IFC, or IFC-illegal payload
    // faults at admission (BimFault.ModelRejected, the wire-admission arm) rather than minting a half graph. Rooted
    // NodeId re-mints (a fresh Guid-v7), the GlobalId riding the node ExternalId for re-ingest correlation; the
    // projector's own delta Header overrides the Genesis seed.
    public Fin<ElementGraph> Admit(ProjectionContext ctx, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        BimIo.ImportIfc(Format, Bytes, ctx.Key)
            .MapFail(error => (Error)Detail.WireDecode.At(ctx.Key, error.Message))
            .Bind(db => ProjectionAssembly.Assemble(
                ProjectionSuite.Of(
                    Seq<IElementProjection>(new SemanticProjector(db, reconciler, profiles)),
                    Seq(ConstraintRegistration.Of(new IfcLegality()))),
                ElementGraph.Genesis(ctx.Header), ctx).Map(static r => r.Graph));

    // Content negotiation across the IFC serializations a peer admits (STEP > ifcXML > ifcJSON by interop breadth) —
    // data-interchange "fidelity routes the format" law, the IFC analog of the Persistence SnapshotCodec.Negotiate; an
    // empty intersection faults rather than silently defaulting to STEP, and Negotiate offers ONLY sealable rows (the
    // Serialization-column set below), so a negotiated format always Seals.
    public static Fin<InterchangeFormat> Negotiate(Seq<string> accepted, Op key) =>
        Serializations.Find(f => accepted.Contains(f.Key) || accepted.Contains(f.MediaType))
            .ToFin(Detail.WireNoMutual.At(key, string.Join(',', accepted)));

    // GeometryGym-emittable IFC wire forms, highest interop fidelity first (STEP > ifcXML > ifcJSON) — DERIVED
    // from the format#FORMAT_AXIS Serialization column (Some exactly on the GeometryGym rows) + export capability, so
    // a future IFC5/IFCX codec admission (one format row flipping InterchangeCodec.Ifc5Pending to a real toolkit)
    // joins the wire with NO edit here; IFC5 is ABSENT until then because GeometryGym reads/writes IFC2x3-IFC4.x only —
    // enumerating the CataloguePending ifc5 row would advertise a wire form Seal cannot produce (the deleted
    // phantom form). FidelityRank is a COLUMN on the Projection/egress#IFC_EGRESS IfcWireForm row that already owns
    // serialization and container, so this fold READS the owning row's rank rather than re-switching on
    // FormatIfcSerialization: the switch it replaces carried a `_` tail that silently seated beside ifcJSON every
    // serialization kind that vocabulary grows next. A container form ranks with the serialization it repeats, so
    // plain and zipped STEP tie and the stable sort holds InterchangeFormat.Items roster order, bare form first.
    static readonly Seq<InterchangeFormat> Serializations =
        toSeq(InterchangeFormat.Items
            .Where(static f => f.RoundTrippable && f.Serialization.IsSome)
            .OrderBy(static f => f.Serialization.Map(static form => form.FidelityRank).IfNone(int.MaxValue)));
}

// IFC-wire leg of the cross-runtime golden corpus (tests/contracts/MANIFEST.md IFC_WIRE): the seam graph content-key (the
// cross-runtime CONTRACT) with the C#-host IFC-bytes golden. Agrees is the cross-runtime parity — a peer (the
// ifcopenshell companion, the web peer) that decodes the same bytes and projects its OWN graph computes the same
// ContentAddress (over the content-keyed non-rooted nodes — the float-bearing IfcMaterialLayer golden vector the
// Projection/address corpus anchors — and the GlobalId correlation, NOT the rooted NodeId, a LOCAL fresh Guid-v7
// per ingest). Reproduces is host-local re-seal byte determinism under the canonical authoring order. A
// cross-runtime byte-equality claim is the deleted form: GeometryGym, ifcopenshell, and web serializers emit
// divergent byte layouts for one graph, so the byte golden NEVER crosses runtimes — only the GraphKey does.
public sealed record WireParity(string Corpus, ContentAddress GraphKey, UInt128 GoldenBytes, long ByteCount) {
    // The byte golden mints through the ONE kernel seed-zero `ContentHash` the semantic key already rides — the
    // two are different QUESTIONS (semantic parity across runtimes, host-local byte determinism) over one hasher,
    // so a second digest scheme beside it forks the content space this package's ruling seals to one.
    public static WireParity Of(string corpus, IfcWire wire) =>
        new(corpus, wire.Content, ContentHash.Of(wire.Bytes.Span), wire.ByteCount);

    // Cross-runtime semantic parity — the contract the corpus exists for: a Python/TypeScript peer reproduces the
    // seam ContentAddress from its OWN projection of the same bytes, so agreement is Content equality, never bytes.
    public bool Agrees(IfcWire wire) => wire.Content == GraphKey;

    // C#-host re-seal byte determinism (host-local, NOT cross-runtime): a re-Seal of one graph under the canonical
    // authoring order reproduces the byte golden, catching a GeometryGym-output regression; a peer satisfies Agrees.
    public bool Reproduces(IfcWire wire) =>
        wire.ByteCount == ByteCount && ContentHash.Of(wire.Bytes.Span) == GoldenBytes;
}
```

## [03]-[RESEARCH]

(none)
