# [BIM_WIRE]

`IfcWire` is the cross-runtime IFC interchange wire: one raw artifact carrying the format key, IFC bytes, schema key, semantic `ContentAddress.OfGraph`, and mint instant. `SemanticProjector.Emit` re-authors an `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` to a GeometryGym serialization, and every serialization of one model shares the semantic address before re-admission through `IfcWire.Admit`.

`Rasm.Bim` owns GeometryGym alone, so the IFC bytes ARE the BIM wire: the `python:geometry/ifc-companion` ifcopenshell peer and the TypeScript web peer decode the same serialization Bim emits, never re-minting a parallel BIM shape.

Element-graph interchange stays out of Bim: the `ElementGraph`/`GraphDelta` snapshot (`json-stj`/`cbor`/`messagepack` with the op-log change stream) is `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s and Version owner, and the gRPC service descriptor is an APP-PLATFORM transport concern — this page owns ONLY the IFC interchange wire. Generic-model STJ serializers and gRPC descriptors inside an AEC-domain package are strata leaks; "one model, many faces" survives DISTRIBUTED by stratum, never consolidated in Bim.

`IfcWire` is HOST-FREE — no RhinoCommon type, no host-bound geometry, only IFC bytes and the content-key the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store and the element graph share — and deserialization is admission, faulting at the boundary so a malformed wire payload never mints a half-built graph.

## [01]-[INDEX]

- [02]-[WIRE_PROJECTION]: `IfcWire`, the content-keyed IFC interchange artifact (`Seal` egress, `Admit` ingress, `Negotiate` serialization selection) composing `SemanticProjector.Emit`/`Project` and `ProjectionAssembly.Assemble`, with the `WireParity` cross-runtime agreement leg.

## [02]-[WIRE_PROJECTION]

- Entry: `IfcWire.Of(format, bytes, schema, content, at, key)` is the ONE construction — the record's ctor is private, so every wire in the solution resolved its format row and carried a payload BY CONSTRUCTION and no interior member re-tests a column; `IfcWire.Seal(SemanticProjector projector, ElementGraph graph, InterchangeFormat format, Option<EmitContext> context, Instant at)` is the producer egress (the profiles resolver is the projector's ctor-held capability, never a `Seal` parameter — the `EmitContext` carrier rides through whole, so a diff-prior, a scoped trade-package slice, or a declared unit regime wires with zero `Seal` edits) and `IfcWire.Admit(ProjectionContext ctx, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` the consumer ingress; `IfcWire.Negotiate(Seq<string> accepted)` resolves the highest-fidelity IFC serialization a peer admits. `Fin<T>` aborts on a non-round-trippable `format` row (`Model/faults#FAULT_BAND` `wire-encode`), on the `Projection/egress#IFC_EGRESS` gate faults `SemanticProjector.Emit` raises, and on a malformed-bytes decode or an `IfcLegality`-rejected projection (`wire-decode`) — each typed `BimFault` case lifting BARE onto the result with no `.ToError()` hop, the gate vocabulary itself owned by the egress and legality pages rather than restated here. Artifact identity is the SEMANTIC graph address, never a positional DTO and never the byte hash.
- Auto: `Seal` re-authors the graph through `SemanticProjector.Emit` at the `Projection/wireform#IFC_WIRE_FORM` `IfcWireForm` the row resolves — the form's own seal writing the container and handing back BYTES this shared stores whole — stamps the wire-form-INDEPENDENT `ContentAddress.OfGraph(graph)` and the `graph.Header.Schema`, so a STEP and an ifcJSON of one model carry one `Content` and a peer joins them; `Admit` decodes through the ONE GeometryGym decode owner — `Exchange/import#IMPORT_PIPELINE` `BimIo.ImportIfc`, the schema sniffed off the bytes BEFORE construction — hands a fresh `SemanticProjector` to `ProjectionAssembly.Assemble` over an `ElementGraph.Genesis(ctx.Header)` seed (the projector's own `GraphDelta.Reheader` overriding the seed header), and runs the `IfcLegality` IFC-semantic legality (the relationship law with the vocabulary arms) so an illegal or out-of-roster projection never freezes a graph; `Negotiate` folds the IFC `InterchangeFormat` rows by the `IfcWireForm.FidelityRank` column so a peer that reads only ifcJSON receives ifcJSON without a call-site branch.
- Output: `IfcWire` is the one cross-runtime IFC contract — the ifcopenshell companion and the web peer decode the same bytes the .NET branch emits; the `Content` joins the artifact to the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` geometry-blob store (the `RepresentationContentHash` body keys inside the graph are cross-runtime stable) and the element graph the `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec` persists; `WireParity` carries the cross-runtime contract as the shared `Content` (`Agrees` — a peer that decodes the same bytes and projects its OWN graph computes the same `ContentAddress`) and the C#-host re-seal as `Reproduces` (host-local emit determinism), so a cross-runtime peer is checked by `Agrees` and never by a byte compare (the GeometryGym/ifcopenshell/web serializers emit divergent byte layouts for one graph).
- Packages: GeometryGymIFC_Core, Generator.Equals, Rasm.Element, LanguageExt.Core, NodaTime, Rasm
- Growth: a new admitted column on the artifact is one `Validation` clause on `Of`, so a caller handing two bad columns reads both refusals; a new IFC serialization GeometryGym emits is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `GeometryGym` codec — the DERIVED `Serializations` and `Negotiate` admit it with NO wire edit, the row-promotion discipline applied to the wire; a new wire form — a genuinely new serialization KIND beyond STEP/XML/JSON, or a new CONTAINER over a landed one — is one `Projection/wireform#IFC_WIRE_FORM` row on the owning axis, the serialization carrying the `FidelityRank` a container form inherits by repeating it, so the negotiation fold here takes it with zero edit and only its `Exchange/import#IMPORT_PIPELINE` decode arm lands (the wire carries no decode fence); a `CataloguePending` row (the `ifc5` row until an IFC5/IFCX toolkit lands) is excluded by the codec filter rather than advertised as sealable; a new peer is one decoder aligning to the IFC bytes (never a new wire owner); the artifact identity is the shared `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph`, so a new content-stable rule is one clause on the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`, never a second wire hasher; the element-graph snapshot/delta wire grows in `Rasm.Persistence` and the gRPC descriptor in the APP-PLATFORM transport owner, never here.
- Boundary: this page carries NO decode fence — the bytes→`DatabaseIfc` admission IS `Exchange/import#IMPORT_PIPELINE` `BimIo.ImportIfc` composed under the `wire-decode` admission context, so a second decode beside the import path is the deleted form and a hand-constructed non-IFC `IfcWire` faults at `Admit` through that owner's own codec gate. Rooted `NodeId` stays LOCAL — a fresh `Guid`-v7 per ingest, the compressed IFC `GlobalId` riding `Node.Object.ExternalId` for re-ingest correlation — so a re-admitted wire re-mints rooted ids, a "rooted address round-trips across runtimes" claim is the deleted form, and cross-runtime parity runs over that correlation and the content-keyed non-rooted `Material`/`PropertySet`/representation nodes. `ExportArtifact`, the geometry-bearing GLB byte-keyed emit, is `Exchange/export#EXPORT_PIPELINE`'s and distinct: byte identity, never graph identity.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

namespace Rasm.Bim;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class WireBytes : IEqualityComparer<ReadOnlyMemory<byte>> {
    public static readonly WireBytes Default = new();

    public bool Equals(ReadOnlyMemory<byte> left, ReadOnlyMemory<byte> right) => left.Span.SequenceEqual(right.Span);

    public int GetHashCode(ReadOnlyMemory<byte> value) => ContentHash.Of(value.Span).GetHashCode();
}

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

    public static Fin<IfcWire> Of(
        string format, ReadOnlyMemory<byte> bytes, ReleaseVersion schema, ContentAddress content, Instant at) =>
        (Rostered(format, key), Payload(bytes, format, key))
            .Apply((row, payload) => new IfcWire(row.Key, payload, schema, content, at)).As().ToFin();

    static Validation<Error, InterchangeFormat> Rostered(string value) =>
        toSeq(InterchangeFormat.Items).Find(row => StringComparer.Ordinal.Equals(value, row.Key))
            .ToValidation<Error>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "interchange-format-miss", value })));

    static Validation<Error, ReadOnlyMemory<byte>> Payload(ReadOnlyMemory<byte> bytes, string format) =>
        bytes.IsEmpty
            ? Validation<Error, ReadOnlyMemory<byte>>.Fail(new BimFault.Refused(BimScope.Wire, BimReason.Rejected, string.Join(':', new object?[] { "wire-encode", format })))
            : Validation<Error, ReadOnlyMemory<byte>>.Success(bytes);

    public static Fin<IfcWire> Seal(
        SemanticProjector projector, ElementGraph graph, InterchangeFormat format,
        Option<EmitContext> context, Instant at) =>
        format.Serialization.Filter(_ => format.RoundTrippable).Match(
            Some: form => projector.Emit(graph, form, key, context).Bind(bytes =>
                Of(bytes, graph.Header.Schema, ContentAddress.OfGraph(graph), at)),
            None: () => Fin.Fail<IfcWire>(new BimFault.Refused(key, BimScope.Wire, BimReason.Rejected, string.Join(':', new object?[] { "wire-encode", format.Key }))));

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

    public static Fin<InterchangeFormat> Negotiate(Seq<string> accepted) =>
        Mutual(toHashSet(accepted)).ToFin(new BimFault.Refused(BimScope.Wire, BimReason.Codec, string.Join(':', new object?[] { "wire-no-mutual", string.Join(',', accepted) })));

    static Option<InterchangeFormat> Mutual(HashSet<string> offered) =>
        Serializations.Value.Find(f => offered.Contains(f.Key) || offered.Contains(f.MediaType));

    static readonly Lazy<Seq<InterchangeFormat>> Serializations = new(static () =>
        toSeq(InterchangeFormat.Items
            .Choose(static f => f.Serialization.Filter(_ => f.RoundTrippable).Map(form => (Row: f, form.FidelityRank)))
            .OrderBy(static pair => pair.FidelityRank)
            .Select(static pair => pair.Row)),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

public sealed record WireParity(string Corpus, ContentAddress GraphKey, UInt128 SealedBytes, long ByteCount) {
    public static WireParity Of(string corpus, IfcWire wire) =>
        new(corpus, wire.Content, ContentHash.Of(wire.Bytes.Span), wire.ByteCount);

    public bool Agrees(IfcWire wire) => wire.Content == GraphKey;

    public bool Reproduces(IfcWire wire) =>
        wire.ByteCount == ByteCount && ContentHash.Of(wire.Bytes.Span) == SealedBytes;
}
```

## [03]-[RESEARCH]

(none)
