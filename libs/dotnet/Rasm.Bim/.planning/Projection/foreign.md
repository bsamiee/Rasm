# [BIM_FOREIGN_PROJECTION]

`ForeignProjector` is the shared arm for foreign OBJECT GRAPHS — a dotbim `File` or a Speckle `Base` tree already deserialized in memory — lowering each host object onto a `Rasm.Element/Graph/delta#GRAPH_DELTA` `GraphDelta` exactly as `semantic#SEMANTIC_PROJECTOR` `SemanticProjector` lowers a live `DatabaseIfc`. `Reingest` is the projector-polymorphic re-ingest beside it: it re-projects a revised source through ANY `IElementProjection`, reconciles the fresh graph to a prior `ElementGraph` by `ExternalId`, and reads back the type candidates the first ingest left unreconciled. Both read a PROJECTED graph, never foreign bytes — byte-and-graph decode is `Exchange/import#IMPORT_PIPELINE`'s and stays one stratum up.

This page composes the shared `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` floor whole: `IElementProjection`, `ProjectionContext`, and the contract-declared `TypeCandidate` row `Rasm.Materials` admits from its own end. Node identity mints through `Rasm.Element/Graph/element#NODE_MODEL` `NodeId.Of(NodeSeed)`, edge re-identification through the contract's own `Relationship.Remap` and `Node.Relabel`, and content keys through `Rasm/Domain/identity#CONTENT_KEY`. Display geometry never crosses here — a dotbim mesh pool and a Speckle display mesh decode on the `Exchange/import#IMPORT_PIPELINE` fold, so a semantic node references no representation.

## [01]-[INDEX]

- [02]-[FOREIGN_PROJECTION]: `ForeignProjector` the ONE foreign-object-graph shared arm over its closed `ForeignSource` discriminant, the `ForeignHost` admission row both sources land on, and the path-forest containment fold.
- [03]-[REINGEST]: `Reingest.Advance` the projector-polymorphic incremental re-ingest with its `ExternalId` reconcile, and `Reingest.ExportTypeCandidates` the reverse type-candidate export off the unreconciled ingested types.

## [02]-[FOREIGN_PROJECTION]

- Owner: `ForeignProjector : IElementProjection` the ONE foreign-object-graph arm — `ForeignSource` its closed discriminant (`DotBim` a deserialized `dotbim.File`, `Speckle` a received `Base` tree), `ForeignHost` the admitted host row every source lands on before any graph node mints, `ForeignMap` the `[Mapper]` contract owning both admissions. Two sibling projectors stood here: they shared the identity regime (a foreign external id), the admission path (an already-deserialized graph captured internally), the payload timing (the whole graph in memory), and the consumer (the app root's `Seq<IElementProjection>`) — so the source type IS the discriminant and it is recoverable from the value. NAMED LOSS: a caller can no longer name a per-format projector type; the contract already forbids that (`projection#PROJECTION_CONTRACT` holds the concrete internal behind its owning package's minting factory), so the loss is the floor's own law and the two `Of` overloads discriminate on the source they take.
- Cases: `ForeignSource.DotBim` carries the whole-object `Color` and a `MeshId` tag, so its arm alone mints a content-keyed `Node.Appearance` and its `Associate` edge; `ForeignSource.Speckle` carries namespace PATHS, so its arm alone folds the containment forest. Everything between — external id, classification, property bag, the `Assign.PropertyDefinition` edge — is the shared `Seat` fold, which is why neither arm re-spells a node construction.
- Law: projectors publish values and typed refusals, never recovery posture. A thrown foreign failure remains the exact captured `Error`; only a documented provider refusal can become a cause-bearing owner fault.
- Entry: `ForeignProjector.Of(dotbim.File)` and `ForeignProjector.Of(Base)` are the two mints — argument type discriminates, so no source-name suffix rides an entrypoint — each returning the shared floor; `Project(ProjectionContext ctx)` lowers the captured graph to one `GraphDelta` seeded `GraphDelta.Empty.Reheader(ctx.Header)`. `Fin<T>` carries the contract admissions this fold crosses (`Classification.Of`'s accumulated blank gates, `AppearanceSummary.Of`'s channel scan); `ProjectionAssembly.Assemble` captures a thrown foreign error exactly and never remints it through projector policy.
- Auto: each arm admits its hosts ONCE through the `ForeignMap` `[Mapper]` — the foreign nullable string and the foreign untyped property cell cross their per-type `[UserMapping]` converters there and nowhere else — then folds `Seat` over the admitted rows: one rooted `Node.Object` carrying the neutral `Classification(system, typeToken)` and the foreign external id, one content-keyed `Node.PropertySet` bag, one `Assign.PropertyDefinition` edge. Beyond that fold the dotbim arm lowers each element's display-referred `Color` through the `Semantics/appearance#APPEARANCE_PROJECTION` transfer pair into a content-keyed `Node.Appearance` bound by an `Associate` edge; the Speckle arm descends the `TraverseWithPath` path segments through ONE forest, seating each host at its own chain tip and reading its enclosing owner on the SAME descent.
- Output: the `GraphDelta` IS the result — `NodeCount`/`EdgeCount` the change magnitude and the distinct classification codes the source vocabulary; the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `AssembledModel` carries it up, so this arm mints no second evidence value.
- Packages: Speckle.Sdk, Speckle.Objects, dotbim, Riok.Mapperly, Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new foreign object-graph source is one `ForeignSource` case, one `ForeignMap` admission arm, and one `Of` overload — the total `Switch` breaks the `Project` body at compile time until its arm lands, and `Seat` is untouched; a new admitted column is one `ForeignHost` column the mapper fills; a new host-object discriminant is one classification code, never a parallel row family; a new containment reading is one rule on the forest the path descent already builds. Managed geometry decode never grows here: display meshes ride `Exchange/import#IMPORT_PIPELINE`.
- Boundary: `Speckle.Sdk`/`Speckle.Objects` are the OUTSIDE-RHINO concern — this arm composes them only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC — and no foreign type crosses `Project`: `dotbim.*` and `Speckle.*` die at the `ForeignMap` admission, which is why the fold below reads no nullable and no untyped cell. Property cells cross TYPED — a Speckle `object?` lands `PropertyValue.Boolean`/`Integer`/`Number`/`Text` by its own runtime shape, because collapsing every cell to `Text` erased exactly the discriminants the shared `PropertyValue` union exists to keep. Colour rides ONE carrier: the appearance node the `export#EXPORT_PIPELINE` counterpart writes from, decoded through the appearance projector's declared inverse, so a Rasm-authored `.bim` keeps its own colour on re-ingest — re-reading it into a hex `PropertyValue.Text` row beside a summary-sourced export is the deleted asymmetry. Alpha never takes the transfer curve because coverage is linear by definition, and the three unauthored PBR channels take the matte-dielectric reading the format's vocabulary implies rather than a guessed metalness. Every graph node here is a PRIMARY identity mint (`NodeId.Of(new NodeSeed.Placement())`), because a foreign host object is an authored element, and every non-rooted bag or appearance re-labels to its own content seed so identical payloads dedup to one node. Speckle containment owns no QuikGraph arm: every `AlgorithmExtensions` entry consumes the parent relation as INPUT and deriving that relation is precisely this fold's work.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using LanguageExt;
using Riok.Mapperly.Abstractions;
using Speckle.Sdk.Models;
using Rasm;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Node = Rasm.Element.Graph.Node;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ForeignSource {
    private ForeignSource() { }

    public sealed record DotBim(dotbim.File Value) : ForeignSource;
    public sealed record Speckle(Base Root) : ForeignSource;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ForeignHost(
    string System, string TypeToken, Option<string> ExternalId, string Name, string Tag,
    string SetName, Map<PropertyName, PropertyValue> Rows);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ForeignProjector : IElementProjection {
    private readonly ForeignSource source;

    private ForeignProjector(ForeignSource source) => this.source = source;

    public static IElementProjection Of(dotbim.File file) => new ForeignProjector(new ForeignSource.DotBim(file));
    public static IElementProjection Of(Base root) => new ForeignProjector(new ForeignSource.Speckle(root));

    public Fin<GraphDelta> Project(ProjectionContext ctx) => source.Switch(
        state: ctx,
        dotBim: static (context, arm) => Lower(arm.Value, context),
        speckle: static (context, arm) => Lower(arm.Root, context));

    // --- [OPERATIONS]
    static Fin<GraphDelta> Lower(dotbim.File file, ProjectionContext ctx) =>
        toSeq(file.Elements)
            .Traverse(element => Appearance(element, ctx.Key)
                .Bind(appearance => Seat(ForeignMap.ToHost(element), ctx)
                    .Map(seated => seated.Delta
                        .Put(appearance)
                        .Link(new Relationship.Associate(seated.Id, appearance.Id, new MaterialUsage.Unbound())))))
            .As()
            .Map(static parts => parts.Fold(GraphDelta.Empty, static (all, part) => all.Merge(part)))
            .Map(delta => delta.Reheader(ctx.Header));

    static Fin<GraphDelta> Lower(Base root, ProjectionContext ctx) =>
        toSeq(toSeq(root.TraverseWithPath(static _ => false))
            .Choose(static step => step.Item2 is DataObject data ? Some((Path: step.Item1, Data: data)) : None)
            .OrderBy(static step => step.Path.Length))
            .Traverse(step => Seat(ForeignMap.ToHost(step.Data), ctx).Map(seated => (step.Path, seated.Id, seated.Delta)))
            .As()
            .Map(static seats => Contained(seats))
            .Map(delta => delta.Reheader(ctx.Header));

    static Fin<(GraphDelta Delta, NodeId Id)> Seat(ForeignHost host, ProjectionContext ctx) =>
        Classification.Of(host.System, host.TypeToken, ctx.Key).Map(classification => {
            NodeId id = NodeId.Of(new NodeSeed.Placement());
            Node.PropertySet bag = Bag(host, ctx.Header.Tolerance);
            return (Delta: GraphDelta.Empty
                .Put(new Node.Object(
                    Id:              id,
                    Kind:            ObjectKind.Occurrence,
                    ExternalId:      host.ExternalId,
                    Classification:  classification,
                    PredefinedType:  PredefinedType.NotDefined,
                    ObjectType:      Option<string>.None,
                    Name:            host.Name,
                    Tag:             host.Tag,
                    Representations: RepresentationContentHash.Empty,
                    History:         Option<OwnerHistory>.None,
                    Span:            SchemaSpan.From(ctx.Header.Schema)))
                .Put(bag)
                .Link(new Relationship.Assign(id, bag.Id, AssignKind.PropertyDefinition)),
                Id: id);
        });

    static Node.PropertySet Bag(ForeignHost host, double tolerance) {
        Node.PropertySet seed = new(NodeId.Of(new NodeSeed.Placement()),
            new PropertyBag(host.SetName, host.Rows, InheritanceMode.OccurrenceWins, EvidenceGrade.Import));
        return (Node.PropertySet)seed.Relabel(NodeId.Of(new NodeSeed.Content(seed, tolerance)));
    }

    static Fin<Node.Appearance> Appearance(dotbim.Element element) =>
        AppearanceSummary.Of(
            AppearanceVector.Create(
                baseColorR:   AppearanceProjection.Linearize(element.Color.R / 255.0),
                baseColorG:   AppearanceProjection.Linearize(element.Color.G / 255.0),
                baseColorB:   AppearanceProjection.Linearize(element.Color.B / 255.0),
                metallic:     0.0,
                roughness:    1.0,
                opacity:      element.Color.A / 255.0,
                transmissive: false))
        .Map(static summary => {
            Node.Appearance draft = new(NodeId.Of(new NodeSeed.Placement()), summary);
            return (Node.Appearance)draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, 0.0)));
        });

    static GraphDelta Contained(Seq<(string[] Path, NodeId Id, GraphDelta Delta)> seats) {
        var nodes = new Dictionary<(int Parent, string Segment), int>();
        var owners = new Dictionary<int, NodeId>();
        return seats.Fold(GraphDelta.Empty, (all, seat) => {
            (int tip, Option<NodeId> enclosing) = Descend(seat.Path);
            owners[tip] = seat.Id;
            return enclosing.Match(
                Some: parent => all.Merge(seat.Delta).Link(new Relationship.Compose(parent, seat.Id, ComposeKind.Contain)),
                None: () => all.Merge(seat.Delta));
        });

        (int Tip, Option<NodeId> Enclosing) Descend(string[] path) {
            (int cursor, Option<NodeId> held) = (0, Option<NodeId>.None);
            foreach (string segment in path) {
                held = owners.TryGetValue(cursor, out NodeId owner) ? Some(owner) : held;
                var step = (cursor, segment);
                cursor = nodes.TryGetValue(step, out int seated) ? seated : nodes[step] = nodes.Count + 1;
            }
            return (cursor, held);
        }
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ForeignMap {
    [FormatProvider(Default = true)]
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    [MapperIgnoreSource(nameof(dotbim.Element.Vector))]
    [MapperIgnoreSource(nameof(dotbim.Element.Rotation))]
    [MapperIgnoreSource(nameof(dotbim.Element.Color))]
    [MapperIgnoreSource(nameof(dotbim.Element.Guid))]
    [MapValue(nameof(ForeignHost.System), "dotbim")]
    [MapValue(nameof(ForeignHost.SetName), "Pset_Dotbim")]
    [MapProperty(nameof(dotbim.Element.Type), nameof(ForeignHost.TypeToken))]
    [MapProperty(nameof(dotbim.Element.Type), nameof(ForeignHost.Name))]
    [MapProperty(nameof(dotbim.Element.MeshId), nameof(ForeignHost.Tag), StringFormat = "D")]
    [MapProperty(nameof(dotbim.Element.Info), nameof(ForeignHost.Rows), Use = nameof(Text))]
    [MapPropertyFromSource(nameof(ForeignHost.ExternalId), Use = nameof(External))]
    public static partial ForeignHost ToHost(dotbim.Element element);

    [MapValue(nameof(ForeignHost.System), "speckle")]
    [MapValue(nameof(ForeignHost.Tag), "")]
    [MapProperty(nameof(DataObject.speckle_type), nameof(ForeignHost.TypeToken))]
    [MapProperty(nameof(DataObject.speckle_type), nameof(ForeignHost.SetName))]
    [MapProperty(nameof(DataObject.applicationId), nameof(ForeignHost.ExternalId), Use = nameof(Admit))]
    [MapProperty(nameof(DataObject.name), nameof(ForeignHost.Name), Use = nameof(Blank))]
    [MapProperty(nameof(DataObject.properties), nameof(ForeignHost.Rows), Use = nameof(Cells))]
    public static partial ForeignHost ToHost(DataObject data);

    [UserMapping] private static Option<string> Admit(string? value) => Optional(value).Filter(static v => v.Length > 0);

    [UserMapping] private static string Blank(string? value) => Optional(value).IfNone("");

    [UserMapping]
    private static Option<string> External(dotbim.Element element) =>
        Optional(element.Info).Bind(info => info.TryGetValue("globalId", out string? held) ? Optional(held) : None)
            .Filter(static held => held.Length > 0)
            .IfNone(() => Some(element.Guid));

    [UserMapping]
    private static Map<PropertyName, PropertyValue> Text(Dictionary<string, string>? info) =>
        toMap(Optional(info).Map(static rows => toSeq(rows).Map(static pair =>
                (PropertyName.Create(pair.Key), (PropertyValue)new PropertyValue.Text(pair.Value))))
            .IfNone([]));

    [UserMapping]
    private static Map<PropertyName, PropertyValue> Cells(Dictionary<string, object?> properties) =>
        toMap(toSeq(properties).Map(static pair => (PropertyName.Create(pair.Key), Cell(pair.Value))));

    private static PropertyValue Cell(object? value) => value switch {
        null => new PropertyValue.Logical(None),
        bool flag => new PropertyValue.Boolean(flag),
        sbyte or byte or short or ushort or int or uint or long or ulong =>
            new PropertyValue.Integer(BigInteger.Parse(((IConvertible)value).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)),
        float or double or decimal =>
            new PropertyValue.Number(((IConvertible)value).ToDouble(CultureInfo.InvariantCulture)),
        string text => new PropertyValue.Text(text),
        var other => new PropertyValue.Text(other.ToString() ?? ""),
    };
}
```

## [03]-[REINGEST]

- Owner: `Reingest` the projector-polymorphic incremental re-ingest — `Advance` re-projects a revised source through ANY `IElementProjection` and reconciles it to a prior `ElementGraph` snapshot by `ExternalId` so a large model's minor revision costs the delta, not the whole graph; `ReingestResult` pairing the patched snapshot with the forward delta; `Reconcile` the `ExternalId`-keyed structural diff; `Candidates` the reverse type-minting export — one shared `TypeCandidate` per ingested `IfcTypeObject` the `semantic#SEMANTIC_PROJECTOR` `AdmitType` reconciler left UNresolved, read off that type node's `TypeSignatureSet` bookkeeping bag and its own property bags. Both members read a PROJECTED snapshot rather than foreign bytes, which is why they share this owner and why neither seats on the interchange codec.
- Entry: `Reingest.Advance(IElementProjection projector, ElementGraph prior, ProjectionContext ctx)` — the caller decodes the revised source ONCE into its projector (`Exchange/import#IMPORT_PIPELINE` `BimIo.ImportIfc` then the IFC `SemanticProjector`, or a received `Base` then `ForeignProjector.Of`), so reingest never re-decodes a format and stays one polymorphic owner; `key.Catch` preserves a thrown foreign error exactly, while a documented terminal contract refusal can arrive already typed as `ElementFault.ProjectorFaulted`, and a corrupt reconcile delta naming an absent endpoint returns `ElementFault.NodeAbsent` at `Apply`. `Reingest.ExportTypeCandidates(ElementGraph graph)` projects the unreconciled types out of ANY projected snapshot — candidacy IS the `TypeSignatureSet` bag's PRESENCE, because a resolver hit lands `CanonicalTypeSeed` with no source bag at all, so the export reads the reconciliation verdict itself and never a second trust column.
- Auto: `Advance` runs the projector once onto a `Genesis(ctx.Header)` seed, then `Reconcile` remaps each revised rooted `Object` to its prior identity by `ExternalId` — a re-projection mints FRESH neutral Guid-v7 ids, so identity matches on the stable external id — and rewrites every revised node and edge through the contract's own `Node.Relabel` and `Relationship.Remap`. Partitioning then runs ONCE: the remapped nodes split on prior presence, the present half yields the revised pairs whose `Generator.Equals` comparer reports a divergent member, and the prior keys absent from the revised set are the removals. Edges diff by structural equality over hashed membership.
- Output: `ReingestResult` carries the patched `ElementGraph` and the forward `GraphDelta` the `Rasm.Persistence` event log stores — the delta IS the change set, its `Address(tolerance)` content key deduping a re-applied delta; the `Review/diff#MODEL_DIFF` `ElementChange` federation change-set is the SEPARATE review surface, not minted here.
- Packages: Rasm.Element, Generator.Equals, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new re-ingestable source is one more `IElementProjection` the caller hands `Advance` — the reconcile is projector-agnostic and keyed only on `ExternalId`, so no second reingest entrypoint; a finer change granularity is one more `MemberPath` the SAME `Inequalities` substrate already yields; a new candidate axis is one `TypeCandidate` column read off the type node or its signature bag, the Materials admission fold widening on that same column. Never a parallel delta store and never a re-tessellation of a content-key-matched representation.
- Boundary: change detection is the ONE `Generator.Equals` `Inequalities` engine `egress#IFC_EGRESS`, `Exchange/export#ROUNDTRIP`, and `Review/diff#MODEL_DIFF` already share — a whole-node canonical-byte compare is the deleted second engine, because it re-quantized every measure through the document tolerance to answer a membership question and names no changed member. Identity re-writing is the CONTRACT's: `Node.Relabel` sets a rooted node's own id and `Relationship.Remap` rewrites both endpoints, a `Connect`'s realizing intermediary, AND every `PropertyValue.Reference` buried in a `Generic` attribute map with its `Participants` roster — a local endpoints-only rewrite dangled exactly those references and went stale on every shared case addition. Duplicate `ExternalId`s are a REAL malformed-source long tail (colliding IFC GlobalIds ship in the wild), so the reconcile is TOTAL: the first revised claimant keeps the prior identity and later duplicates keep their fresh ids — an add and a remove, never a wrong merge. Content-key-matched representations are never re-tessellated because `RepresentationContentHash` is identical; heavy display geometry therefore never enters the delta. `TypeCandidate` is the contract-declared record — `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` owns the one declaration and both this export and the `Rasm.Materials` `Component/component#CATALOGUE` `AdmitImported` fold compose it, so neither package references the other and a locally re-spelled twin is the drift the contract homing forecloses; `IIfcTypeReconciler` stays the Bim-declared port both ends compose by contract. Candidate rows are an IN-MEMORY projection minting NO store provenance column: `Rasm.Persistence/Version/provenance#CAUSAL_DAG` `ProvKind.Import` already attributes an imported entity off the changefeed. Candidate identity reads the NODE, never a re-spelled bag row — `ExternalId` the 1:1 `GlobalId` projection, `Classification.Code` the IFC entity name (the `Model/elements#IFC_CLASS` row key IS that name, so a roster hit and a roster miss spell it identically), and `ObjectType`-over-`PredefinedType` the EFFECTIVE token the signature folded a `USERDEFINED` label into — so only the material and profile axes, which have no node column, read the signature bag by the row names `semantic#SEMANTIC_PROJECTOR` `ImportedSource` authors. Mapperly owns no arm on this cluster: a `TypeCandidate` column reading `node.ExternalId` through a typed refusal and two columns reading a bag by row NAME are admissions, not member correspondences, and a mapper forced over them carries more ignore rows than mappings.

```csharp
public sealed record ReingestResult(ElementGraph Patched, GraphDelta Delta);

public static class Reingest {
    public static Fin<ReingestResult> Advance(IElementProjection projector, ElementGraph prior, ProjectionContext ctx) =>
        Try.lift(() => projector.Project(ctx)).Run().Bind(static inner => inner)
            .Map(fresh => fresh.ReplayOnto(ElementGraph.Genesis(ctx.Header)))
            .Map(revised => Reconcile(prior, revised))
            .Bind(delta => prior.Apply(delta).Map(patched => new ReingestResult(patched, delta)));

    static GraphDelta Reconcile(ElementGraph prior, ElementGraph revised) {
        var priorByExternal = prior.ObjectNodes
            .Choose(static o => o.ExternalId.Map(x => (External: x, o.Id)))
            .Fold(Map<string, NodeId>(), static (held, p) => held.TryAdd(p.External, p.Id));
        var remap = revised.ObjectNodes
            .Choose(o => o.ExternalId.Bind(x => priorByExternal.Find(x)).Map(priorId => (o.Id, Prior: priorId)))
            .Fold((Claimed: HashSet<NodeId>(), Held: Map<NodeId, NodeId>()), static (acc, p) =>
                acc.Claimed.Contains(p.Prior) ? acc : (acc.Claimed.Add(p.Prior), acc.Held.Add(p.Id, p.Prior)))
            .Held;
        NodeId Reidentify(NodeId id) => remap.Find(id).IfNone(id);
        var revisedNodes = toSeq(revised.Nodes.Values).Map(n => n is Node.Object o ? o.Relabel(Reidentify(o.Id)) : n);
        var revisedEdges = toSeq(revised.Edges).Map(e => e.Remap(Reidentify));
        var revisedIds = toHashSet(revisedNodes.Map(static n => n.Id));
        var (held, added) = revisedNodes.Partition(n => prior.Find(n.Id).IsSome);
        var revisedPairs = held.Choose(n => prior.Find(n.Id)
            .Filter(p => !EqualityComparer<Node>.Default.Equals(p, n))
            .Map(p => (Before: p, After: n)));
        var removed = toSeq(prior.Nodes.Keys).Filter(id => !revisedIds.Contains(id));
        var priorEdges = toHashSet(prior.Edges);
        var revisedEdgeSet = toHashSet(revisedEdges);
        return new GraphDelta(
            toSeq(added), removed, revisedPairs,
            revisedEdges.Filter(e => !priorEdges.Contains(e)),
            toSeq(prior.Edges).Filter(e => !revisedEdgeSet.Contains(e)),
            Some(revised.Header));
    }

    public static Fin<Seq<TypeCandidate>> ExportTypeCandidates(ElementGraph graph) =>
        from types in graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Type)
            .Traverse(node => graph.Bake(node.Id).Map(baked => (Node: node, Bags: baked.Properties))).As()
        let library = Library(graph.Header.Step)
        from candidates in types
            .Choose(static type => type.Bags
                .Filter(static bag => bag.SetName == SemanticProjector.TypeSignatureSet).Head
                .Map(signature => (type.Node, type.Bags, Signature: signature)))
            .Traverse(pair => Candidate(library, pair.Node, pair.Bags, pair.Signature)).As()
        select candidates;

    static Fin<TypeCandidate> Candidate(string library, Node.Object node, Seq<PropertyBag> bags, PropertyBag signature) =>
        node.ExternalId
            .ToFin(new BimFault.Refused(BimScope.Projection, BimReason.Rejected,
                string.Join(':', new object?[] { "type-candidate-identity-missing", node.Id.ToValue() })))
            .Map(globalId => new TypeCandidate(
                SourceLibrary:      library,
                GlobalId:           globalId,
                IfcEntity:          node.Classification.Code,
                PredefinedToken:    node.ObjectType.IfNone(node.PredefinedType.ToValue()),
                Name:               node.Name,
                Properties:         Rows(bags),
                MaterialName:       Text(signature, SemanticProjector.SignatureRows.MaterialName),
                ProfileDesignation: Text(signature, SemanticProjector.SignatureRows.ProfileDesignation),
                ProfileStandard:    Text(signature, SemanticProjector.SignatureRows.ProfileStandard)));

    static Map<PropertyName, PropertyValue> Rows(Seq<PropertyBag> bags) =>
        bags.Filter(static bag => bag.SetName != SemanticProjector.TypeSignatureSet)
            .Fold(Map<PropertyName, PropertyValue>(), static (all, bag) =>
                bag.Values.AsIterable().Fold(all, static (held, row) => held.AddOrUpdate(row.Key, row.Value)));

    static Option<string> Text(PropertyBag signature, PropertyName row) =>
        signature.Values.Find(row).Bind(static value => value is PropertyValue.Text text ? Some(text.Value) : None);

    static string Library(StepHeader step) =>
        string.IsNullOrWhiteSpace(step.OriginatingSystem) ? step.Name : step.OriginatingSystem;
}
```

## [04]-[RESEARCH]

(none)
