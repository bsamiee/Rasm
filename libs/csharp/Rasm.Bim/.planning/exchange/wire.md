# [BIM_WIRE]

The cross-runtime IFC interchange wire: one `IfcWire` content-keyed artifact carrying an `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` re-authored to IFC bytes (STEP / ifcXML / ifcJSON, the three serializations GeometryGym emits) through the Bim-internal `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit`, stamped with the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` so EVERY IFC serialization of one model shares one identity, and re-admitted through `IfcWire.Admit` (schema-sniffed decode → `SemanticProjector.Project` → `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` under the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` constraint). `Rasm.Bim` is the SOLE GeometryGym owner, so the IFC bytes ARE the BIM wire — the `python:geometry/ifc-companion` ifcopenshell peer and the TypeScript web peer decode the same IFC serialization Bim emits, never re-minting a parallel BIM shape.

The seam-graph cross-runtime wire — the `ElementGraph`/`GraphDelta` snapshot as `json-stj`/`cbor`/`messagepack` plus the op-log change stream — is `Rasm.Persistence`'s `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec` and Version owner, and the gRPC service descriptor is an APP-PLATFORM transport concern; NEITHER is re-minted here. This page owns ONLY the IFC interchange wire. The retired `BimModel`/`BimElement` snapshot wire, its `[SmartEnum]` `BimWireFace` (`Snapshot`/`OpLog`/`Grpc`), and the `BimWireContext` source-generated `JsonSerializerContext` are GONE — a generic-model STJ serializer and a gRPC descriptor inside an AEC-domain package were strata leaks; "one model, many faces" survives DISTRIBUTED by stratum, not consolidated in Bim. The `IfcWire` is HOST-FREE: it carries no RhinoCommon type and no host-bound geometry, only IFC bytes and the content-key the `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store and the seam graph share; deserialization is admission and faults at the boundary so a malformed wire payload never mints a half-built graph.

## [01]-[INDEX]

- [01]-[WIRE_PROJECTION]: the `IfcWire` content-keyed IFC interchange artifact (`Seal` egress, `Admit` ingress, `Negotiate` serialization selection) composing `SemanticProjector.Emit`/`Project` + `ProjectionAssembly.Assemble`, and the `WireParity` cross-runtime golden-corpus leg.

## [02]-[WIRE_PROJECTION]

- Owner: `IfcWire` the content-keyed cross-runtime IFC interchange artifact — the IFC serialization bytes, the seam `ContentAddress` graph identity, and the `Rasm.Element/Graph/element#NODE_MODEL` `ReleaseVersion` schema stamp (the `csharp:data-interchange#ARTIFACT_IDENTITY` "payload + descriptor stamp + content hash" law applied to IFC); `WireParity` the IFC-wire leg of the cross-runtime golden corpus (`lib:ONE_WIRE_FIXTURE_CORPUS`).
- Entry: `IfcWire.Seal(SemanticProjector projector, ElementGraph graph, InterchangeFormat format, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles, Instant at, Op key)` is the producer egress and `IfcWire.Admit(ProjectionContext ctx)` the consumer ingress — `Fin<T>` aborts on a non-IFC `format` (`Model/faults#FAULT_BAND` `BimFault.ModelRejected` `wire-encode`) or a `SemanticProjector.Emit` egress-gate fault (`Projection/semantic#IFC_EGRESS` `BimFault.UnmappedClass` on an out-of-schema `PredefinedType` [C6]) on the egress, and on a malformed-bytes decode or an `IfcLegality`-rejected projection (`BimFault.ModelRejected` `wire-decode`, the wire-admission arm) on the ingress — each typed `BimFault` case (band 2600, `Expected`-derived) lifting BARE onto the `Fin<T>` rail with no `.ToError()` hop; `IfcWire.Negotiate(Seq<string> accepted, Op key)` resolves the highest-fidelity IFC serialization a peer admits. The artifact identity is the SEMANTIC graph address (`ContentAddress.OfGraph`), never a positional DTO and never the byte hash.
- Auto: `Seal` re-authors the graph through `SemanticProjector.Emit` at the `FormatIfcSerialization` the row resolves, stamps the serialization-INDEPENDENT `ContentAddress.OfGraph(graph)` and the `graph.Header.Schema`, so a STEP and an ifcJSON of one model carry one `Content` and a peer joins them; `Admit` sniffs the schema off the bytes BEFORE constructing the `DatabaseIfc` [H8], hands a fresh `SemanticProjector` to `ProjectionAssembly.Assemble` over an `ElementGraph.Genesis(ctx.Header)` seed (the projector's own `GraphDelta.Reheader` overriding the seed header), and runs the `IfcLegality` IFC-semantic legality so an illegal projection never freezes a graph; `Negotiate` folds the IFC `InterchangeFormat` rows by interop breadth so a peer that reads only ifcJSON receives ifcJSON without a call-site branch.
- Receipt: the `IfcWire` is the one cross-runtime IFC contract — the ifcopenshell companion and the web peer decode the same bytes the C# branch emits; the `Content` joins the artifact to the `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store (the `RepresentationContentHash` body keys inside the graph are cross-runtime stable) and the seam graph the `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec` persists; `WireParity` carries the cross-runtime contract as the seam `Content` (`Agrees` — a peer that decodes the same bytes and projects its OWN graph computes the same `ContentAddress`) and the C#-host re-seal byte golden as `Reproduces` (host-local emit determinism), so a cross-runtime peer is checked by `Agrees` and never by a byte compare (the GeometryGym/ifcopenshell/web serializers emit divergent byte layouts for one graph).
- Packages: GeometryGymIFC_Core, Rasm.Element, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm
- Growth: a new IFC serialization GeometryGym emits is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `GeometryGym` codec — the DERIVED `Serializations` and `Negotiate` admit it with NO wire edit, the row-promotion discipline applied to the wire; only a genuinely new serialization KIND beyond STEP/XML/JSON is one `SerializationOf`/`Decode` arm, and a `CataloguePending` row (the `ifc5` row until an IFC5/IFCX toolkit lands) is excluded by the codec filter rather than advertised as sealable; a new peer is one decoder aligning to the IFC bytes (never a new wire owner); the artifact identity is the seam `ContentAddress.OfGraph`, so a new content-stable rule is one clause on the seam `Rasm.Element/Projection/address#CANONICAL_WRITER`, never a second wire hasher; the seam-graph snapshot/delta wire grows in `csharp:Rasm.Persistence` and the gRPC descriptor in the APP-PLATFORM transport owner, never here.
- Boundary: this page owns ONLY the IFC interchange wire — the seam-graph cross-runtime wire (`ElementGraph`/`GraphDelta` as `json-stj`/`cbor`/`messagepack` + the op-log change stream) is `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s and a `BimWire`/`BimWireContext` STJ serializer over the seam graph re-minted in Bim is the deleted form (the seam-graph STJ wire is Persistence's `ElementJson`, peers depending on a peer is the named strata violation); the gRPC service descriptor and the op-log change-stream FACE are APP-PLATFORM transport concerns, and the retired `BimWireDescriptor`/`OpLogWire`/`BimWireFace` consolidation inside an AEC-domain package was the strata leak this rebuild deletes; the artifact identity is the SEMANTIC `ContentAddress.OfGraph` (order-independent, the one `XxHash128` seed-zero hasher) so the same graph emitted to STEP and to ifcJSON shares one key and a byte-hash identity is the deleted form; the rooted `NodeId` is LOCAL — a fresh `Guid`-v7 per ingest [H6], the compressed IFC `GlobalId` riding the `Node.Object.ExternalId` for re-ingest correlation — so a re-admitted wire re-mints rooted ids and a "rooted address round-trips across runtimes" claim is the deleted form, the cross-runtime parity being over the content-keyed (non-rooted) `Material`/`PropertySet`/representation nodes + the `GlobalId` correlation; the geometry-bearing `ExportArtifact` (the GLB byte-keyed emit) is `Exchange/export#EXPORT_RAIL`'s and distinct (byte identity, not graph identity); `SemanticProjector.Emit` builds its OWN target `DatabaseIfc` and ignores the captured ingress db, so the same projector that imported a model re-emits it and a from-scratch path supplies any instance; the bytes→`DatabaseIfc` admission decode is the schema-sniffed sibling of `Exchange/import#IMPORT_RAIL`'s `Database`, sharing the one GeometryGym decode but correcting its hardcoded `IFC4X3_ADD2` to the sniffed schema [H8].

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The cross-runtime IFC interchange artifact: the IFC serialization bytes + the seam graph content-address
// identity + the schema stamp (the data-interchange "payload + descriptor stamp + content hash" law). One
// ElementGraph emits to the N GeometryGym IFC serializations (STEP/ifcXML/ifcJSON) ALL sharing one Content, because the
// identity is the SEMANTIC graph address (ContentAddress.OfGraph), never the byte hash — so a STEP and an
// ifcJSON of one model join on one key. HOST-FREE: only IFC bytes + the content-key the Compute geometry-blob
// store and the seam graph share, never a RhinoCommon type.
public sealed record IfcWire(
    InterchangeFormat Format,
    ReadOnlyMemory<byte> Bytes,
    Rasm.Element.ReleaseVersion Schema,
    ContentAddress Content,
    long ByteCount,
    Instant At) {

    // Producer egress: re-author the graph to IFC bytes through the Bim-internal SemanticProjector.Emit, stamp
    // the serialization-INDEPENDENT seam graph content-address and the Header schema. A non-emittable format (a
    // non-GeometryGym codec, or a read-only IFC row) faults ModelRejected (wire-encode); an Emit gate fault
    // (UnmappedClass on an out-of-schema PredefinedType [C6]) rails through. Emit
    // builds its own target database and ignores the projector's captured ingress db, so the same projector that
    // imported a model (Project) re-emits it (Emit) and a from-scratch authoring path supplies any instance.
    public static Fin<IfcWire> Seal(
        SemanticProjector projector, ElementGraph graph, InterchangeFormat format,
        Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles, Instant at, Op key) =>
        format.Codec == InterchangeCodec.GeometryGym && format.CanExport
            ? projector.Emit(graph, SerializationOf(format), prior, profiles).Map(text => {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                return new IfcWire(format, bytes, graph.Header.Schema, ContentAddress.OfGraph(graph), bytes.LongLength, at);
            })
            : Fin.Fail<IfcWire>(new BimFault.ModelRejected(key, $"wire-encode:{format.Key}"));

    // Consumer admission: decode the IFC bytes to a schema-sniffed DatabaseIfc, project to a GraphDelta through a
    // fresh SemanticProjector, and assemble onto a Genesis seed under the IfcLegality constraint — so a malformed
    // or IFC-illegal payload faults at admission (BimFault.ModelRejected, the fault band's wire-admission arm)
    // rather than minting a half graph. The rooted NodeId re-mints (a fresh Guid-v7), the GlobalId riding the node
    // ExternalId for re-ingest correlation [H6]; the projector's own delta Header overrides the Genesis seed.
    public Fin<ElementGraph> Admit(ProjectionContext ctx) =>
        Database(Format, Bytes, ctx.Key).Bind(db => ProjectionAssembly.Assemble(
            Seq1<IElementProjection>(new SemanticProjector(db)),
            Seq1<IGraphConstraint>(new IfcLegality()),
            ElementGraph.Genesis(ctx.Header), ctx).Map(static r => r.Graph));

    // Content negotiation across the IFC serializations a peer admits (STEP > ifcXML > ifcJSON by interop breadth) —
    // the data-interchange "fidelity routes the format" law, the IFC analog of the Persistence SnapshotCodec.Negotiate;
    // an empty intersection faults rather than silently defaulting to STEP, and Negotiate offers ONLY sealable rows
    // (the GeometryGym codec set below), so a negotiated format always Seals.
    public static Fin<InterchangeFormat> Negotiate(Seq<string> accepted, Op key) =>
        Serializations.Find(f => accepted.Contains(f.Key) || accepted.Contains(f.MediaType))
            .ToFin(new BimFault.CodecReject(key, $"wire-no-mutual:{string.Join(',', accepted)}"));

    // The GeometryGym-emittable IFC serializations, highest interop fidelity first (STEP > ifcXML > ifcJSON) —
    // DERIVED from the format#FORMAT_AXIS table on the GeometryGym codec + export capability, so a future IFC5/IFCX
    // codec admission (one format row flipping InterchangeCodec.Ifc5Pending to a real toolkit) joins the wire with NO
    // edit here; IFC5 is ABSENT until then because GeometryGym reads/writes IFC2x3-IFC4.x only — enumerating the
    // CataloguePending ifc5 row would advertise a serialization Seal cannot produce (the deleted phantom form).
    static readonly Seq<InterchangeFormat> Serializations =
        InterchangeFormat.Items
            .Where(static f => f.Codec == InterchangeCodec.GeometryGym && f.CanExport)
            .OrderBy(static f => SerializationOf(f) switch {
                FormatIfcSerialization.STEP => 0,
                FormatIfcSerialization.XML  => 1,
                _                           => 2,
            })
            .ToSeq();

    static FormatIfcSerialization SerializationOf(InterchangeFormat format) =>
        format == InterchangeFormat.IfcXml ? FormatIfcSerialization.XML
        : format == InterchangeFormat.IfcJson ? FormatIfcSerialization.JSON
        : FormatIfcSerialization.STEP;

    // The wire's IFC-bytes -> DatabaseIfc admission decode, the schema sniffed off the bytes BEFORE construction
    // [H8] so a 2x3 wire admits as 2x3 (STEP auto-resolves FILE_SCHEMA; ifcJSON/ifcXML construct at the sniffed
    // schema, the corrected form of the geometry-import rail's hardcoded IFC4X3_ADD2). GeometryGym parse faults
    // funnel to BimFault.ModelRejected so a malformed payload never crosses a domain signature.
    static Fin<DatabaseIfc> Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Op key) =>
        Try.lift(() => Decode(format, bytes)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"wire-decode:{error.Message}"));

    static DatabaseIfc Decode(InterchangeFormat format, ReadOnlyMemory<byte> bytes) {
        GeometryGym.Ifc.ReleaseVersion schema = SemanticProjector.Sniff(bytes, format);
        return format == InterchangeFormat.IfcJson ? JsonDatabase(bytes, schema)
            : format == InterchangeFormat.IfcXml ? XmlDatabase(bytes, schema)
            : DatabaseIfc.ParseString(Encoding.UTF8.GetString(bytes.Span));
    }

    static DatabaseIfc JsonDatabase(ReadOnlyMemory<byte> bytes, GeometryGym.Ifc.ReleaseVersion schema) {
        var db = new DatabaseIfc(false, schema);
        db.ReadJSON((JsonObject)JsonNode.Parse(bytes.Span)!);
        return db;
    }

    static DatabaseIfc XmlDatabase(ReadOnlyMemory<byte> bytes, GeometryGym.Ifc.ReleaseVersion schema) {
        var doc = new XmlDocument();
        doc.LoadXml(Encoding.UTF8.GetString(bytes.Span));
        var db = new DatabaseIfc(false, schema);
        db.ReadXMLDoc(doc);
        return db;
    }
}

// The IFC-wire leg of the cross-runtime golden corpus (lib:ONE_WIRE_FIXTURE_CORPUS): the seam graph content-key
// (the cross-runtime CONTRACT) plus the C#-host IFC-bytes golden. Agrees is the cross-runtime parity — a peer (the
// ifcopenshell companion, the web peer) that decodes the same bytes and projects its OWN graph computes the same
// ContentAddress (over the content-keyed non-rooted nodes — the float-bearing IfcMaterialLayer golden vector the
// Projection/address corpus anchors — and the GlobalId correlation, NOT the rooted NodeId, a LOCAL fresh Guid-v7 per
// ingest). Reproduces is host-local re-seal byte determinism under the canonical authoring order. A cross-runtime
// byte-equality claim is the deleted form: the GeometryGym, ifcopenshell, and web serializers emit divergent byte
// layouts for one graph, so the byte golden NEVER crosses runtimes — only the GraphKey does.
public sealed record WireParity(string Corpus, ContentAddress GraphKey, UInt128 GoldenBytes, long ByteCount) {
    public static WireParity Of(string corpus, IfcWire wire) =>
        new(corpus, wire.Content, XxHash128.HashToUInt128(wire.Bytes.Span), wire.ByteCount);

    // Cross-runtime semantic parity — the contract the corpus exists for: a Python/TypeScript peer reproduces the
    // seam ContentAddress from its OWN projection of the same bytes, so agreement is Content equality, never bytes.
    public bool Agrees(IfcWire wire) => wire.Content == GraphKey;

    // C#-host re-seal byte determinism (host-local, NOT cross-runtime): a re-Seal of one graph under the canonical
    // authoring order reproduces the byte golden, catching a GeometryGym-output regression; a peer satisfies Agrees.
    public bool Reproduces(IfcWire wire) =>
        wire.ByteCount == ByteCount && XxHash128.HashToUInt128(wire.Bytes.Span) == GoldenBytes;
}
```

## [03]-[RESEARCH]

- [IFC_WIRE_AS_CONTRACT]: the IFC bytes ARE the cross-runtime BIM wire because `Rasm.Bim` is the SOLE GeometryGym owner — grounds against `ELEMENT-REBUILD-PLAN.md` §6 (the `libs/python` wire-alignment: "Align the IFC→IfcOpenShell→GLB companion + interchange decoders to the seam ... decode, never re-mint") and §4D (Bim `Exchange/` = "codecs over the seam graph"); the egress/ingress round-trip composes `SemanticProjector.Emit`/`Project` (`Projection/semantic#IFC_EGRESS`) over the verified GeometryGym surface — `FormatIfcSerialization { STEP, XML, JSON }`, `DatabaseIfc.ParseString(string)`, `DatabaseIfc.ReadJSON(JsonObject)`, `DatabaseIfc.ReadXMLDoc(XmlDocument)`, `DatabaseIfc.ToString(FormatIfcSerialization)`, `new DatabaseIfc(bool, ReleaseVersion)` — decompile-verified against `.api/api-geometrygym-ifc` and `assay api query GeometryGymIFC_Core` (25.7.30).
- [SEMANTIC_IDENTITY]: the artifact identity is the seam `ContentAddress.OfGraph` (order-independent over the node + edge sets, the ONE `XxHash128` seed-zero hasher) so STEP/ifcXML/ifcJSON of one graph share one key and a peer joins them — grounds against `Rasm.Element/Projection/address#CONTENT_ADDRESS` `[SINGLE_HASHER]` and `csharp:data-interchange#ARTIFACT_IDENTITY`; the rooted `NodeId` is a LOCAL fresh `Guid`-v7 per ingest [H6] (the compressed IFC `GlobalId` riding `Node.Object.ExternalId` for re-ingest correlation), so cross-runtime parity is the content-keyed non-rooted nodes (the float-bearing `IfcMaterialLayer`-shaped golden vector the `Projection/address` corpus anchors) plus the `GlobalId` correlation, never the rooted address; the `RepresentationContentHash` body keys inside the graph are the cross-runtime-stable join to the `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store. The `WireParity` cross-runtime leg verifies the seam `Content` (`Agrees` — the `ContentAddress` equality a Python/TypeScript peer reproduces from its own projection of the same bytes, the only cross-runtime-stable comparison) and composes `System.IO.Hashing` `XxHash128.HashToUInt128(ReadOnlySpan<byte>)` (`.api/api-hashing`) ONLY for the host-local re-seal byte determinism (`Reproduces`), never a cross-runtime byte compare.
- [STRATA_SPLIT]: the seam-graph cross-runtime wire (`ElementGraph`/`GraphDelta` snapshot as `json-stj`/`cbor`/`messagepack` + the op-log change stream) is `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec` (the `json-stj` row IS the inspector/web wire and the Marten event serializer) and Version owner, and the gRPC descriptor an APP-PLATFORM transport concern — neither re-minted here because `Rasm.Bim` is AEC-domain and peers never reference a peer (`csharp:Rasm.Persistence`); the retired `BimWire`/`BimWireContext`/`OpLogWire`/`BimWireDescriptor`/`BimWireFace` consolidation (a generic-model STJ serializer + a gRPC descriptor inside an AEC-domain package) was a strata leak this rebuild deletes, "one model, many faces" surviving DISTRIBUTED by stratum (IFC wire = Bim, seam-graph wire = Persistence, transport = AppHost) per `ELEMENT-REBUILD-PLAN.md` §4F and §6 (AppHost stays reference-light, owns transport primitives).
- [ADMISSION_GATE]: admission decodes the IFC bytes to a schema-sniffed `DatabaseIfc` [H8] and assembles under `IfcLegality : IGraphConstraint` [M3] so a malformed or IFC-illegal payload faults `BimFault.ModelRejected` (the `Model/faults#FAULT_BAND` wire-admission arm: "an IDS facet miss and a wire admission reject are `ModelRejected`") before minting a graph — grounds against `Rasm.Element/Projection/projection#GRAPH_CONSTRAINT` (net-new Rasm interfaces = 2) and `Projection/semantic#GRAPH_LEGALITY` plus the `SemanticProjector.Sniff` `FILE_SCHEMA`/`schema_identifier` read; the bytes→`DatabaseIfc` decode is the schema-sniffed sibling of `Exchange/import#IMPORT_RAIL`'s `Database` (which hardcodes `IFC4X3_ADD2` for ifcJSON/ifcXML — the H8 defect this wire corrects), a cross-file dedup candidate (one `IfcDatabase.Of(format, bytes)` owner both should compose).
