# [BIM_ELEMENT_QUERY]

`Rasm.Bim` instantiates the shared `Rasm.Element/Query/predicate#ELEMENT_PREDICATE` closure over `BimLeaf` — one `[Union]` wrapping `ElementLeaf` beside five IFC-schema leaves. `ElementQuery` folds a native term over `ElementGraph`, and `StoreLowering` lowers that same algebra onto the persisted flat projection. No second selection surface or generated predicate DTO exists.

Any refinement this algebra cannot express lands as one new arm at ITS owner, never an untyped escape hatch, and the store phase answers a SUPERSET its in-process residue narrows so live graph and durable store resolve one question identically.

Selection composes settled owners. `Model/spatial#SPATIAL_STRUCTURE` owns the containment walk whole; shared `ValueBag<V>.Merge` owns the effective property stream under its stamped `InheritanceMode`; `Model/elements#IFC_CLASS` owns the entity-class token; `Relations/relation#EDGE_ALGEBRA` owns the incidence beside its `Generic` long tail [NEUTRAL_EDGE_RULING].

`Rasm.Element` owns the algebra and this page owns evaluation because evaluation authority stays with the folder that owns the data. Results stay typed: `Combine` gates cross-graph composition and faults lift `Model/faults#FAULT_BAND` bare.

## [01]-[INDEX]

- [02]-[ELEMENT_SET]: `BimLeaf` the Bim leaf family over the contract closure — the `Element(ElementLeaf)` wrapping arm beside the IFC-schema leaves — the `SpatialReach` containment-reach discriminant, the `ObjectAttribute`/`ValueSource` value vocabularies, the `SetOperation` combinator rows, and `ElementQuery` the graph-bound fold carrying `Selection<NodeId>` with the verdict evidence its walk raised.
- [03]-[PREDICATE_PUSHDOWN]: `StorePlan` lowers the store-expressible subset to one parameterized DuckDB statement and folds the residue in-process, so one native selection algebra spans live graph and durable store.

## [02]-[ELEMENT_SET]

- Owner: `BimLeaf` is the Bim leaf family the shared `Predicate<TLeaf>` closes over — `Element(ElementLeaf)` the ONE wrapping arm carrying every contract-owned query dimension (kind, classification branch, attribute, property, material, composition, assignment, connection, void, assessment, generic wire) beside the five IFC-SCHEMA leaves the contract declines: `ByClass`/`ByDomain`/`ByPredefinedType` over the `Model/elements#IFC_CLASS` roster, `ByClassificationSystem` the system-membership existential, and `BySpatialContainer` the ceded containment reach. `SpatialReach`, `ObjectAttribute`, `ValueSource`, and `SetOperation` are the policy vocabularies its evaluation and its consumers compose. `ElementQuery` owns graph-bound selection, refinement, set algebra, effective-value reads, baking, and measured aggregation over the shared `Selection<NodeId>`; no wrapper duplicates the term identity.
- Entry: `ElementQuery.Query(ElementGraph graph, BimTerm term)` folds the term over `graph.ObjectNodes`; the shared `And`/`Or`/`AndNot` flatten the boolean closure and `Predicate<BimLeaf>.Open` is the named vacuous conjunction; `BimLeaf.InZone`/`OfType`/`Classified` mint the derived terms whose shared spelling is a composition rather than one arm; `Combine(other, operation, key)` applies one `SetOperation` row after proving both queries share the same graph; `Where` refines the current selection; `Bake(key)` traverses the selected objects through the shared derivation path; `BimLeaf.Key(term)` streams any term through the shared `PredicateKey` into the content key a memo and a replayable selection share.
- Auto: evaluation is ONE parameterized fold — `Holds<TLeaf>` supplies the shared `Predicate<TLeaf>.Holds` its leaf verdict and its closure verdict, and the two leaf vocabularies (`BimLeaf` at the top, `ElementLeaf` inside every nested `NodeMatch`) instantiate it rather than forking a second walker. Both leaf dispatches are the Thinktecture generated total `Switch`, so a missing query dimension is a build error at every site rather than a silent fallthrough. Classification arms decide over the primary `Classification` AND the co-applied `Classifications` set, so a secondary standard-system code never escapes a branch facet; `ByProperty` decides set-name, property-name, and value through three independent `ValueMatch` restrictions over BOTH bag kinds on occurrence and type, so a patterned `Pset_.*` facet lowers whole without a heavy `Bake`; the incidence arms read the O(degree) `EdgesAt` index and decide every related endpoint through `MatchesNode`, whose `Where` case recurses the same fold on the resolved `Node.Object` so a non-`Object` target fails the nested probe structurally; `BySpatialContainer` reads its `SpatialReach` row's own chain projection, so the reach vocabulary owns the walk and the arm owns none of it.
- Auto: the shared `Closure` arm answers with a GENUINE bounded transitive walk — one `BreadthFirstSearchAlgorithm` over the memoized `graph.View(EdgeFilter.Composition, EdgeOrientation.Ascending)` ascent, `TreeEdge` folding the per-vertex level and `GrayTarget` classifying a non-tree edge onto a queued vertex as the cyclic-`Compose` evidence `Bake` faults on — never an opaque-leaf pass-through, which is the contract's binding evaluator law. `WalkDepth` bounds the level and `WalkDepth.Whole` walks to fixpoint.
- Law: verdicts carry FAULTS. `MatchVerdict` accumulates from both sides of every combinator and `Negate` flips only a CLEAN verdict, so a cyclic ascent or an unresolvable nested target keeps `Holds` false through any surrounding `Not` while its cause rides out on `ElementQuery.Faults` — where the deleted `bool` fold silently delivered or silently dropped a malformed arm and reported a healthy empty selection. Selection stays TOTAL regardless: evidence rides beside the answer, never a fault refusing a whole query for one bad node.
- Law: `Selection<NodeId>.Keys` is ORDER-BEARING in `graph.ObjectNodes` order, so two runs over one snapshot answer byte-identically and `BimLeaf.Key` keys a replayable selection; the deleted `LanguageExt.HashSet<NodeId>` carrier made membership cheap and order arbitrary, so a keyed replay proved nothing about re-deriving the same set. `ElementQuery.Holds` answers O(1) membership off the query's own built-once member set.
- Growth: a new CONTRACT-owned query dimension is one `ElementLeaf` arm at the contract with one arm on this page's `ElementVerdict` (the generated `Switch` breaks loudly until it lands); a new IFC-SCHEMA dimension is one `BimLeaf` arm with its `CanonicalBytes` ordinal; a new value restriction is one shared `ValueMatch` arm; a new rostered `Generic` family is ZERO query edits — `ByGeneric` parameterizes the wire-name; a new incidence flavor is a `SubKind` value the existing arm already parameterizes; a new cross-page value read composes `ElementQuery.ValuesOf` over the `ValueSource` axis and a cross-page set AGGREGATE composes `ElementQuery.SumOf` (the zone rollup and the system demand accumulation are its two standing consumers), never a re-derived bag merge or a manual `double` fold; a new queryable object attribute is one `ObjectAttribute` row (only when the shared `Node.Object` gains the column); a new set combinator is one `SetOperation` row the derived trio and the gated `Combine` share; a new containment reach is one `SpatialReach` row carrying its chain delegate; the chainage axis is ZERO arms — a station band is `ByProperty` with a `Range` restriction over the station row the `Model/spatial#LINEAR_POSITIONING` reader and the `Semantics/feature#GEO_FEATURE` corridor stamp each land dimensioned on their own bag, an offset band the same `Range` over the offset row, and the alignment identity an `Exact` restriction beside them, so "every element between station 2+400 and 3+100" and "everything within 8 m of centreline" compose the standing arms and an `AlongAlignment`-shaped arm is the second selection surface both owning pages already rule out; never a `Get<Dimension>` operation family and never a parallel selection surface.
- Boundary: `BimLeaf` holds the leaf vocabulary and `ElementQuery` mints only through `Query`, `Where`, or a graph-identity-gated `Combine`; a one-field query wrapper, public arbitrary-set constructor, arity family, or cross-graph `NodeId` merge is invalid. Spatial up-chain law stays elsewhere: `SpatialReach.Ancestry` composes the `Model/spatial#SPATIAL_STRUCTURE` ancestor law and `SpatialReach.Direct` that same owner's `ContainerOf` read, so a local `Contain`/`Aggregate` walk here is the third copy of one traversal and the deleted form. `Range` decides ONLY over a `PropertyValue.Measure` sharing the bound's `Dimension`, so reachability by a magnitude facet is the STAMPING owner's obligation — a projector landing a station, an offset, or any other magnitude as a bare `Number` mints a row no range restriction can ever reach — and widening the restriction to swallow an undimensioned candidate is the deleted form that admits a length against a pressure bound. `Rasm.Element` owns the numeric-equality tolerance whole (`ValueMatch` decides at the IDS relative tolerance in SI value space), so no epsilon is declared here.
- Boundary: every nested `NodeMatch` carries `NodeMatch<ElementLeaf>` because the shared incidence arms type their target that way. Bim-only leaves stay top-level; a nested pattern uses the native shared classification/property vocabulary directly. `Review/validation#IDS_FACETS` lowers its `PartOf` facet onto that native shape.
- Packages: Rasm.Element (`Query/predicate#ELEMENT_PREDICATE` the whole algebra — `Predicate<TLeaf>`/`ValueMatch`/`RangeBound`/`NodeMatch<TLeaf>`/`MatchVerdict`/`Selection<TKey>`/`WalkDepth`/`ElementLeaf`/`PredicateKey`; `Graph/element#ELEMENT_GRAPH` `ElementGraph`/`View`/`EdgeFilter`/`EdgeOrientation`/`TypedEdge`/`Bake`; `Projection/address#CONTENT_ADDRESS`), QuikGraph (`BreadthFirstSearchAlgorithm` over the memoized view, its `TreeEdge`/`GrayTarget`/`DiscoverVertex` event fan sharing ONE walk), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Error`), Rasm (`Op`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms.Search;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;
using ElementTerm = Rasm.Element.Query.Predicate<Rasm.Element.Query.ElementLeaf>;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SpatialReach {
    public static readonly SpatialReach Direct   = new("direct",   static (graph, node) => SpatialStructure.ContainerOf(graph, node).ToSeq());
    public static readonly SpatialReach Ancestry = new("ancestry", static (graph, node) => SpatialStructure.Ancestry(graph, node));

    [UseDelegateFromConstructor]
    public partial Seq<NodeId> Chain(ElementGraph graph, NodeId node);
}

[SmartEnum<string>]
public sealed partial class ObjectAttribute {
    public static readonly ObjectAttribute Name     = new("Name",     static o => o.Name is { Length: > 0 } n ? Some<PropertyValue>(new PropertyValue.Text(n)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute Tag      = new("Tag",      static o => o.Tag is { Length: > 0 } t ? Some<PropertyValue>(new PropertyValue.Text(t)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute GlobalId = new("GlobalId", static o => o.ExternalId.Map(static e => (PropertyValue)new PropertyValue.Text(e)));
    public static readonly ObjectAttribute ObjectType = new("ObjectType", static o => o.ObjectType.Map(static t => (PropertyValue)new PropertyValue.Text(t)));

    [UseDelegateFromConstructor]
    public partial Option<PropertyValue> Read(Node.Object value);
}

[SmartEnum<string>]
public sealed partial class SetOperation {
    public static readonly SetOperation Union     = new("union",     static (left, right) => left.Union(right));
    public static readonly SetOperation Intersect = new("intersect", static (left, right) => left.Intersect(right));
    public static readonly SetOperation Except    = new("except",    static (left, right) => left.Except(right));

    [UseDelegateFromConstructor]
    public partial Selection<NodeId> Apply(Selection<NodeId> left, Selection<NodeId> right);
}

[Union]
public abstract partial record ValueSource {
    private ValueSource() { }

    public sealed record Attribute(ObjectAttribute Key) : ValueSource;
    public sealed record Property(string Set, string Name) : ValueSource;
}

[Union]
public abstract partial record BimLeaf {
    private BimLeaf() { }

    public sealed record Element(ElementLeaf Leaf) : BimLeaf;
    public sealed record ByClass(IfcClass Class) : BimLeaf;
    public sealed record ByDomain(IfcDomain Domain) : BimLeaf;
    public sealed record ByPredefinedType(IfcClass Class, PredefinedType Type) : BimLeaf;
    public sealed record ByClassificationSystem(string System) : BimLeaf;
    public sealed record BySpatialContainer(NodeMatch<ElementLeaf> Container, SpatialReach Reach) : BimLeaf;

    public static BimTerm InZone(NodeMatch<ElementLeaf> group) => new BimTerm.Any(Seq<BimTerm>(
        Of(new ElementLeaf.ByAssigned(AssignKind.Group, group)),
        Of(new ElementLeaf.ByComposed(ComposeKind.Reference, group))));

    public static BimTerm OfType(NodeMatch<ElementLeaf> type) => Of(new ElementLeaf.ByAssigned(AssignKind.TypeDefinition, type));

    public static BimTerm Classified(Seq<Classification> branch) => Of(new ElementLeaf.ByClassification(branch));

    public static BimTerm Of(ElementLeaf leaf) => new BimTerm.Leaf(new Element(leaf));

    public void CanonicalBytes(CanonicalWriter w) => Switch(
        state: w,
        element:                static (wr, m) => { wr.Ordinal(0); m.Leaf.CanonicalBytes(wr); },
        byClass:                static (wr, m) => { wr.Ordinal(1).String(m.Class.Key); },
        byDomain:               static (wr, m) => { wr.Ordinal(2).String(m.Domain.Key); },
        byPredefinedType:       static (wr, m) => { wr.Ordinal(3).String(m.Class.Key).String(m.Type.Token); },
        byClassificationSystem: static (wr, m) => { wr.Ordinal(4).String(m.System); },
        bySpatialContainer:     static (wr, m) => { wr.Ordinal(5).String(m.Reach.Key); PredicateKey.Node(m.Container, wr); });

    public static ContentAddress Key(BimTerm term) => PredicateKey.Key(term, static (leaf, w) => leaf.CanonicalBytes(w));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ElementQuery {
    internal const string IfcSystem = "ifc";

    static readonly Op Gate = Op.Of(name: nameof(ElementQuery));

    private ElementQuery(ElementGraph graph, Selection<NodeId> selection, Seq<Error> faults) {
        (Graph, Selection, Faults) = (graph, selection, faults);
        members = toHashSet(selection.Keys);
    }

    readonly LanguageExt.HashSet<NodeId> members;

    public ElementGraph Graph { get; }
    public Selection<NodeId> Selection { get; }
    public Seq<Error> Faults { get; }

    public static ElementQuery Query(ElementGraph graph, BimTerm term) => Fold(graph, graph.ObjectNodes, term);

    public ElementQuery Where(BimTerm term) {
        ElementQuery refined = Fold(Graph, Objects, term);
        return new(Graph, refined.Selection, (Faults + refined.Faults).Distinct().Strict());
    }

    public int Count => Selection.Count;
    public Seq<NodeId> Ids => Selection.Keys;
    public Seq<Node.Object> Objects => Graph.ObjectNodes.Filter(o => members.Contains(o.Id));
    public Seq<string> GlobalIds => Objects.Choose(static o => o.ExternalId);
    public bool Holds(NodeId id) => members.Contains(id);

    public Fin<Seq<Element>> Bake(Op key) => Objects.TraverseM(o => Graph.Bake(o.Id, key)).As();

    public ElementQuery Union(ElementQuery other) => Derive(SetOperation.Union, other);
    public ElementQuery Intersect(ElementQuery other) => Derive(SetOperation.Intersect, other);
    public ElementQuery Except(ElementQuery other) => Derive(SetOperation.Except, other);

    public Fin<ElementQuery> Combine(ElementQuery other, SetOperation operation, Op key) =>
        ReferenceEquals(Graph, other.Graph) || ContentAddress.OfGraph(Graph) == ContentAddress.OfGraph(other.Graph)
            ? Fin.Succ(Derive(operation, other))
            : Fin.Fail<ElementQuery>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "set-cross-graph", operation.Key })));

    ElementQuery Derive(SetOperation operation, ElementQuery other) =>
        new(Graph, operation.Apply(Selection, other.Selection), (Faults + other.Faults).Distinct().Strict());

    static ElementQuery Fold(ElementGraph graph, Seq<Node.Object> candidates, BimTerm term) {
        Seq<(NodeId Id, MatchVerdict Verdict)> verdicts = candidates.Map(o => (o.Id, Verdict(graph, o, term))).Strict();
        return new(graph,
            new Selection<NodeId>(verdicts.Filter(static v => v.Verdict.Holds).Map(static v => v.Id).Strict(), Option<UInt128>.None),
            verdicts.Bind(static v => v.Verdict.Faults).Distinct().Strict());
    }

    // --- [PREDICATE_FOLD]
    public static MatchVerdict Verdict(ElementGraph graph, Node.Object obj, BimTerm term) => Holds(graph, obj, term, BimVerdict);

    static MatchVerdict Holds<TLeaf>(ElementGraph graph, Node.Object obj, Predicate<TLeaf> term, Func<ElementGraph, Node.Object, TLeaf, MatchVerdict> leaf)
        where TLeaf : notnull =>
        term.Holds(l => leaf(graph, obj, l), walk => Reach(graph, obj, walk, leaf));

    static MatchVerdict Reach<TLeaf>(ElementGraph graph, Node.Object obj, Predicate<TLeaf>.Closure walk, Func<ElementGraph, Node.Object, TLeaf, MatchVerdict> leaf)
        where TLeaf : notnull {
        BidirectionalGraph<NodeId, TypedEdge> ascent = graph.View(EdgeFilter.Composition, EdgeOrientation.Ascending);
        if (!ascent.ContainsVertex(obj.Id)) { return Holds(graph, obj, walk.Seed, leaf); }

        BreadthFirstSearchAlgorithm<NodeId, TypedEdge> search = new(ascent);
        Map<NodeId, int> level = Map((obj.Id, 0));
        Seq<NodeId> within = Seq(obj.Id);
        Seq<Error> cycles = Seq<Error>();
        search.TreeEdge += edge => {
            int depth = level.Find(edge.Source).IfNone(0) + 1;
            level = level.AddOrUpdate(edge.Target, depth);
            if (depth <= walk.Depth.Value) { within = within.Add(edge.Target); }
        };
        search.GrayTarget += edge => cycles = cycles.Add(new BimFault.Refused(Gate, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "query-closure-cyclic", edge.Target.Value })));
        search.Compute(obj.Id);

        MatchVerdict reached = within
            .Choose(id => graph.Find<Node.Object>(id))
            .Fold(MatchVerdict.Of(false), (acc, node) => acc.Or(Holds(graph, node, walk.Seed, leaf)));
        return cycles.Fold(reached, static (acc, cause) => acc.And(MatchVerdict.Fault(cause)));
    }

    static MatchVerdict BimVerdict(ElementGraph graph, Node.Object obj, BimLeaf leaf) => leaf.Switch(
        state: (graph, obj),
        element:                static (s, l) => ElementVerdict(s.graph, s.obj, l.Leaf),
        byClass:                static (s, l) => MatchVerdict.Of(s.obj.Classification.System == ElementQuery.IfcSystem
                                                     && string.Equals(s.obj.Classification.Code, l.Class.Key, StringComparison.OrdinalIgnoreCase)),
        byDomain:               static (s, l) => MatchVerdict.Of(s.obj.Classification.System == ElementQuery.IfcSystem
                                                     && IfcClass.TryGet(s.obj.Classification.Code).Exists(c => c.Domain == l.Domain)),
        byPredefinedType:       static (s, l) => MatchVerdict.Of(s.obj.Classification.System == ElementQuery.IfcSystem
                                                     && string.Equals(s.obj.Classification.Code, l.Class.Key, StringComparison.OrdinalIgnoreCase)
                                                     && s.obj.PredefinedType == l.Type),
        byClassificationSystem: static (s, l) => MatchVerdict.Of(string.Equals(s.obj.Classification.System, l.System, StringComparison.OrdinalIgnoreCase)
                                                     || s.obj.Classifications.Exists(c => string.Equals(c.System, l.System, StringComparison.OrdinalIgnoreCase))),
        bySpatialContainer:     static (s, l) => l.Reach.Chain(s.graph, s.obj.Id)
                                                     .Fold(MatchVerdict.Of(false), (acc, whole) => acc.Or(MatchesNode(s.graph, l.Container, whole))));

    static MatchVerdict ElementVerdict(ElementGraph graph, Node.Object obj, ElementLeaf leaf) => leaf.Switch(
        state: (graph, obj),
        byKind:           static (s, l) => MatchVerdict.Of(s.obj.Kind == l.Kind),
        byClassification: static (s, l) => MatchVerdict.Of(l.Branch.Exists(b => b == s.obj.Classification)
                                               || s.obj.Classifications.Exists(c => l.Branch.Exists(b => b == c))),
        byAttribute:      static (s, l) => MatchVerdict.Of(toSeq(ObjectAttribute.Items).Exists(row =>
                                               l.Name.Matches(new PropertyValue.Text(row.Key))
                                               && row.Read(s.obj).Exists(v => l.Restriction.Matches(v)))),
        byProperty:       static (s, l) => MatchVerdict.Of(EffectiveValues(s.graph, s.obj.Id, l.Set, l.Name).Exists(v => l.Restriction.Matches(v))),
        byMaterial:       static (s, l) => MatchVerdict.Of(s.graph.MaterialsOf(s.obj.Id)
                                               .Exists(m => m.Composition.Materials.Exists(id => l.Restriction.Matches(new PropertyValue.Text(id.Value))))),
        byComposed:       static (s, l) => Incident(s.graph, s.obj.Id, l.Whole, e =>
                                               e is Relationship.Compose c && c.SubKind == l.SubKind && c.Part == s.obj.Id ? Some(c.Whole) : None),
        byConnected:      static (s, l) => Incident(s.graph, s.obj.Id, l.Other, e =>
                                               e is Relationship.Connect c && c.Touches(s.obj.Id)
                                                   && l.Kind.Match(Some: k => c.SubKind == k, None: static () => true)
                                                   ? c.Members.Find(m => m != s.obj.Id) : None),
        byVoided:         static (s, l) => Incident(s.graph, s.obj.Id, l.Other, e =>
                                               e is Relationship.Void v && v.SubKind == l.SubKind
                                                   ? v.Host == s.obj.Id ? Some(v.Feature) : v.Feature == s.obj.Id ? Some(v.Host) : None
                                                   : None),
        byGeneric:        static (s, l) => Incident(s.graph, s.obj.Id, l.Other, e =>
                                               e is Relationship.Generic g && g.WireName == l.Wire
                                                   ? g.Source == s.obj.Id ? Some(g.Target) : g.Target == s.obj.Id ? Some(g.Source) : None
                                                   : None),
        byAssigned:       static (s, l) => Incident(s.graph, s.obj.Id, l.Other, e =>
                                               e is Relationship.Assign a && a.SubKind == l.Kind && a.Subject == s.obj.Id ? Some(a.Definition) : None),
        byAssessment:     static (s, l) => MatchVerdict.Of(toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e =>
                                               e is Relationship.Assign a && a.SubKind == AssignKind.Assessment && a.Subject == s.obj.Id
                                               && s.graph.Find<Node.Assessment>(a.Definition).Exists(asm =>
                                                   asm.Payload.Discipline == l.Discipline
                                                   && l.Outcome.Match(Some: o => asm.Payload.Outcome == o, None: static () => true)))));

    static MatchVerdict Incident(ElementGraph graph, NodeId self, NodeMatch<ElementLeaf> target, Func<Relationship, Option<NodeId>> related) =>
        toSeq(graph.EdgesAt(self)).Choose(related)
            .Fold(MatchVerdict.Of(false), (acc, candidate) => acc.Or(MatchesNode(graph, target, candidate)));

    static MatchVerdict MatchesNode(ElementGraph graph, NodeMatch<ElementLeaf> target, NodeId candidate) => target.Switch(
        state: (graph, candidate),
        exact: static (s, t) => MatchVerdict.Of(t.Id == s.candidate),
        where: static (s, t) => s.graph.Find<Node.Object>(s.candidate)
                                    .Map(o => Holds(s.graph, o, t.Pattern, ElementVerdict))
                                    .IfNone(MatchVerdict.Of(false)));

    // --- [VALUE_READS]
    public static Seq<PropertyValue> ValuesOf(ElementGraph graph, Node.Object obj, ValueSource source) => source.Switch(
        attribute: a => a.Key.Read(obj).ToSeq(),
        property:  p => EffectiveValues(graph, obj.Id,
                            new ValueMatch.Exact(new PropertyValue.Text(p.Set)),
                            new ValueMatch.Exact(new PropertyValue.Text(p.Name))));

    public static Fin<Option<MeasureValue>> SumOf(ElementGraph graph, Seq<NodeId> ids, ValueSource source, Op key) {
        Seq<PropertyValue> values = ids
            .Bind(id => graph.Find<Node.Object>(id).ToSeq())
            .Bind(o => ValuesOf(graph, o, source));
        return values.IsEmpty
            ? Fin.Succ(Option<MeasureValue>.None)
            : values.TraverseM(value => value is PropertyValue.Measure measure
                    ? Fin.Succ(measure.Value)
                    : Fin.Fail<MeasureValue>(ElementFault.ValueRejected(key, $"<aggregate-non-measure:{value.GetType().Name}>")))
                .As()
                .Bind(measures => MeasureValue.Sum(measures, key).Map(Some));
    }

    static Seq<PropertyValue> EffectiveValues(ElementGraph graph, NodeId obj, ValueMatch set, ValueMatch name) {
        (Seq<PropertyBag> occProps, Seq<QuantityBag> occQty) = BagsOf(graph, obj);
        (Seq<PropertyBag> typProps, Seq<QuantityBag> typQty) = TypeIdOf(graph, obj).Match(Some: t => BagsOf(graph, t), None: static () => (Seq<PropertyBag>(), Seq<QuantityBag>()));
        Seq<string> names = (occProps.Map(static b => b.SetName) + typProps.Map(static b => b.SetName)
                           + occQty.Map(static b => b.SetName) + typQty.Map(static b => b.SetName))
            .Distinct().Filter(n => set.Matches(new PropertyValue.Text(n)));
        return names.Bind(n =>
            Resolve(occProps, typProps, n).ToSeq().Bind(bag => Named(bag.Values, name, static v => v))
          + Resolve(occQty, typQty, n).ToSeq().Bind(bag => Named(bag.Values, name, static m => (PropertyValue)new PropertyValue.Measure(m))));
    }

    static Seq<PropertyValue> Named<V>(Map<PropertyName, V> values, ValueMatch name, Func<V, PropertyValue> lift) =>
        toSeq(values.AsIterable()).Choose(pair => name.Matches(new PropertyValue.Text(pair.Key.Value)) ? Some(lift(pair.Value)) : Option<PropertyValue>.None);

    static (Seq<PropertyBag> Props, Seq<QuantityBag> Qty) BagsOf(ElementGraph graph, NodeId id) =>
        toSeq(graph.EdgesAt(id)).Fold(
            (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>()),
            (acc, e) => e is Relationship.Assign { SubKind: var k, Subject: var subj, Definition: var def } && k == AssignKind.PropertyDefinition && subj == id && graph.Nodes.TryGetValue(def, out Node? n)
                ? n switch {
                    Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) },
                    Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) },
                    _                   => acc,
                }
                : acc);

    static Option<NodeId> TypeIdOf(ElementGraph graph, NodeId obj) =>
        toSeq(graph.EdgesAt(obj)).Choose(e =>
            e is Relationship.Assign { SubKind: var k, Subject: var subj, Definition: var def } && k == AssignKind.TypeDefinition && subj == obj
                ? Some(def) : Option<NodeId>.None).Head;

    static Option<ValueBag<V>> Resolve<V>(Seq<ValueBag<V>> occurrence, Seq<ValueBag<V>> type, string setName) =>
        occurrence.Find(b => b.SetName == setName).Match(
            Some: occ => Some(type.Find(b => b.SetName == setName).Match(Some: typ => ValueBag<V>.Merge(typ, occ), None: () => occ)),
            None: () => type.Find(b => b.SetName == setName));
}
```

## [03]-[PREDICATE_PUSHDOWN]

- Owner: `StorePlan` the store-side evaluation artifact — ONE parameterized SQL statement over the persisted BimOpenSchema flat fact tables and the in-process `Residue` term — and `StoreLowering.Lower` the two-phase split: the store-expressible subset lowers to SQL, the residue folds in-process over the returned candidates, and the split is SOUND by construction (the SQL phase selects a SUPERSET — a conjunction narrows with its expressible conjuncts and parks the rest on the residue; a disjunction lowers only when EVERY operand lowers, else the whole branch is residue; a negation lowers only over a lowerable operand whose clause is TOTAL, because SQL's third value makes `NOT` over a nullable fact column exclude exactly the absent-column rows the negation selects) — the same broad/narrow law the geospatial H3 prefilter holds at bit parity.
- Entry: `StoreLowering.Lower(BimTerm term, Op key)` folds the term into a `StorePlan` — `Sql` the one `SELECT DISTINCT e.GlobalId` statement over the `FactTable.Entities` scan whose predicates join the remaining rows (`Strings` by `rowid` for the string-index columns, `StringParameters`/`DoubleParameters` through `Descriptors` for the property facts), `Parameters` the positional value list (every dynamic value a parameter — raw-string interpolation into engine SQL is the deleted form the Persistence trust gate names), `Residue` the remainder re-checked in-process; the executing lane is the `Rasm.Persistence/Query/columnar#COLUMNAR_LANE` analytical session, and the returned GlobalId candidates re-enter the algebra as `ByAttribute(GlobalId, OneOf(candidates))` conjoined with the residue over the materialized graph, so the store phase and the in-process phase agree bit-for-bit on the final set.
- Auto: the expressible leaves are the flat projection's own axes — `ByClass` compares the `Category` fact, `ByDomain` expands to the roster's class-key partition (`IfcClass.Items` filtered by `Domain`, one `IN` parameter list), the shared `ByAttribute` over `GlobalId`/`Name` compares the entity columns, and the shared `ByProperty` with exact set/name restrictions lowers `Exact`/`OneOf`/`Range`/`Present` onto the parameter tables (`Value` string equality through the `FactTable.Strings` join, numeric bounds on the `FactTable.DoubleParameters` `Value` column — SI magnitudes by the fact convention); every classification, incidence, zone, spatial, patterned, transitive-`Closure`, and nested-`Where` term is residue, because graph topology stays the graph's and the flat projection carries no classification table beside the single `Category` column. `IN` lists narrow only over a NON-EMPTY value set: an empty set lowers to the canonical FALSE predicate through the one `InFragment` mint, because emitting `IN ()` breaks the statement and dropping the fragment widens the superset into a scan the residue never narrows.
- Output: the `StorePlan` is the dataset-scale query evidence — "every fire-rated door on any current model" runs WHERE the data rests, saved queries and federation-wide reporting execute the same closed algebra, and the plan's `Residue` names exactly what ran in-process so the split is auditable per query. Chainage pushes DOWN whole: an exact set/name `ByProperty` carrying a `Range` lowers onto the `FactTable.DoubleParameters` SI-magnitude column and the alignment identity beside it onto the string join, so a station band over a whole infrastructure model is one statement with an empty residue.
- Packages: Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions (the `FactTable` `[SmartEnum<string>]` row table), Rasm; the fact-table vocabulary is the `Ara3D.BimOpenSchema` record surface (`Entity(LocalId, GlobalId, Document, Name, Category)`, `ParameterString`/`ParameterDouble(Entity, Descriptor, Value)`, `ParameterDescriptor(Name, Units, Group, Type)`, `EntityRelation(EntityA, EntityB, RelationType)` — decompile-verified; the `<Name>_<n>` projection-ordinal identifiers and the single-column `Strings` adapter are the `libs/dotnet/Rasm.Persistence/.api/api-ara3d-bimopenschema.md` `[IMPLEMENTATION_LAW]` law, that catalogue owning the package at the Persistence tier).
- Growth: a new expressible leaf is one `Fragment` case in the lowering fold (the SQL text, its parameter rows, and its totality verdict), zero executor edits; a new fact column is the flat projection's row and one comparison fragment; a re-ordered serializer projection is one `FactTable` `Ordinal` edit and zero fragment edits; never a second selection language and never a store-side term vocabulary beside the algebra.
- Boundary: the lowering emits SQL TEXT + parameters and never opens a connection — execution is the Persistence analytical lane's (the `ColumnarSession` refcounted anchor, the `Query/lane#READ_ROUTING` staleness gate), so the plan crosses the contract as data on the standing `BimOpenSchema` projection edge; the FACT CONVENTION is Bim's half of that boundary — `GlobalId` = the node `ExternalId`, `Category` = the `"ifc"` classification code, a parameter descriptor `Name` = the `{Set}.{Name}` dot-path with `ParameterDouble.Value` the SI magnitude, and every parameter fact the EFFECTIVE value with its type→occurrence merge already resolved under the stamped `InheritanceMode` — the BIM-typed projection `columnar.md` rules Bim-implemented; that materialization is what makes the SQL phase provably a SUPERSET, because an occurrence-only projection puts a `ByProperty` lowering UNDER the in-process answer by dropping every type-inherited value, and a residue narrows but never widens; the table IDENTIFIER is the other half — the `<Stem>_<Ordinal>` name is a serializer emit-order fact the Persistence catalogue owns, so every fragment derives it from a `FactTable` row and a transcribed suffixed literal is the deleted form that survives a re-ordered projection as a name still resolving against the wrong table; the residue split is a correctness law, not an optimization: a lowering that narrows the superset silently drops rows the residue can never recover and is the deleted form — an `Any` lowered as its expressible operands alone, and a `NOT` lowered over a non-total clause, are its two standing instances, the second being why `Fragment` carries a totality verdict rather than a `NOT` wrapper trusting SQL comparison to be two-valued.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FactTable {
    public static readonly FactTable Strings          = new("Strings",          1);
    public static readonly FactTable Descriptors      = new("Descriptors",      2);
    public static readonly FactTable Entities         = new("Entities",         4);
    public static readonly FactTable DoubleParameters = new("DoubleParameters", 6);
    public static readonly FactTable StringParameters = new("StringParameters", 8);

    public int Ordinal { get; }
    public string Identifier { get; }

    private FactTable(string key, int ordinal) : this(key) => (Ordinal, Identifier) = (ordinal, $"{key}_{ordinal}");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StorePlan(string Sql, Seq<object> Parameters, Option<BimTerm> Residue);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StoreLowering {
    readonly record struct Fragment(string Where, Seq<object> Parameters, bool Total);

    public static StorePlan Lower(BimTerm term, Op key) {
        (Option<Fragment> store, Option<BimTerm> residue) = Split(term);
        return store.Match(
            Some: fragment => new StorePlan($"{EntityScan} WHERE {fragment.Where}", fragment.Parameters, residue),
            None: () => new StorePlan(EntityScan, Seq<object>(), residue));
    }

    static (Option<Fragment> Store, Option<BimTerm> Residue) Split(BimTerm term) => term switch {
        BimTerm.All all => all.Operands.Map(Split).Fold(
            (Store: Option<Fragment>.None, Residue: Option<BimTerm>.None),
            static (acc, part) => (
                Store: acc.Store.Match(
                    Some: held => part.Store.Map(next => new Fragment($"({held.Where}) AND ({next.Where})", held.Parameters + next.Parameters, held.Total && next.Total)).IfNone(held),
                    None: () => part.Store),
                Residue: acc.Residue.Match(
                    Some: held => part.Residue.Map(held.And).IfNone(held),
                    None: () => part.Residue))),
        BimTerm.Any any => any.Operands.Map(Split) is var parts
            && parts.ForAll(static p => p.Store.IsSome && p.Residue.IsNone)
                ? (parts.Choose(static p => p.Store).Fold(Option<Fragment>.None, static (acc, next) => acc.Match(
                    Some: held => Some(new Fragment($"({held.Where}) OR ({next.Where})", held.Parameters + next.Parameters, held.Total && next.Total)),
                    None: () => Some(next))), Option<BimTerm>.None)
                : (Option<Fragment>.None, Some(term)),
        BimTerm.Not not => Split(not.Operand) switch {
            ({ IsSome: true } inner, { IsNone: true }) when inner.Case is Fragment { Total: true } fragment =>
                (Some(new Fragment($"NOT ({fragment.Where})", fragment.Parameters, Total: true)), Option<BimTerm>.None),
            _ => (Option<Fragment>.None, Some(term)),
        },
        BimTerm.Leaf leaf => Leaf(leaf.Value).Match(
            Some: fragment => (Some(fragment), Option<BimTerm>.None),
            None: () => (Option<Fragment>.None, Some(term))),
        _ => (Option<Fragment>.None, Some(term)),
    };

    static Option<Fragment> Leaf(BimLeaf leaf) => leaf switch {
        BimLeaf.ByClass c => Some(new Fragment(CategoryEquals, Seq<object>(c.Class.Key), Total: false)),
        BimLeaf.ByDomain d => Some(InFragment(CategoryColumn,
            toSeq(IfcClass.Items).Filter(row => row.Domain == d.Domain).Map(static row => (object)row.Key))),
        BimLeaf.Element { Leaf: ElementLeaf.ByAttribute { Name: ValueMatch.Exact { Value: PropertyValue.Text key } } a }
            when EntityColumns.Find(key.Value) is { IsSome: true, Case: string column } => a.Restriction switch {
                ValueMatch.Exact { Value: PropertyValue.Text t } => Some(new Fragment($"{column} = ?", Seq<object>(t.Value), Total: false)),
                ValueMatch.OneOf o => Some(InFragment(column, o.Allowed.Map(static v => (object)v))),
                _ => None,
            },
        BimLeaf.Element { Leaf: ElementLeaf.ByProperty { Set: ValueMatch.Exact { Value: PropertyValue.Text set }, Name: ValueMatch.Exact { Value: PropertyValue.Text name } } p } => p.Restriction switch {
            ValueMatch.Exact { Value: PropertyValue.Text t } => Some(new Fragment(StringParameterEquals, Seq<object>($"{set.Value}.{name.Value}", t.Value), Total: true)),
            ValueMatch.Range r => RangeFragment($"{set.Value}.{name.Value}", r),
            ValueMatch.Present => Some(new Fragment(ParameterPresent, Seq<object>($"{set.Value}.{name.Value}", $"{set.Value}.{name.Value}"), Total: true)),
            _ => None,
        },
        _ => None,
    };

    static Option<Fragment> RangeFragment(string descriptor, ValueMatch.Range range) {
        Seq<(string Clause, object Value)> bounds =
            range.Lower.Map(b => b.Switch(
                inclusive: static i => (Clause: "p.Value >= ?", Value: (object)i.Value.Si),
                exclusive: static x => (Clause: "p.Value > ?", Value: (object)x.Value.Si))).ToSeq()
            + range.Upper.Map(b => b.Switch(
                inclusive: static i => (Clause: "p.Value <= ?", Value: (object)i.Value.Si),
                exclusive: static x => (Clause: "p.Value < ?", Value: (object)x.Value.Si))).ToSeq();
        return bounds.IsEmpty
            ? None
            : Some(new Fragment(
                $"EXISTS (SELECT 1 FROM {FactTable.DoubleParameters.Identifier} p JOIN {FactTable.Descriptors.Identifier} d ON p.Descriptor = d.rowid JOIN {FactTable.Strings.Identifier} dn ON d.Name = dn.rowid WHERE p.Entity = e.rowid AND dn.Strings = ? AND {string.Join(" AND ", bounds.Map(static b => b.Clause))})",
                ((object)descriptor).Cons(bounds.Map(static b => b.Value)),
                Total: true));
    }

    static Fragment InFragment(string column, Seq<object> values) =>
        values.IsEmpty
            ? new Fragment(FalsePredicate, Seq<object>(), Total: true)
            : new Fragment($"{column} IN ({string.Join(",", values.Map(static _ => "?"))})", values, Total: false);

    const string FalsePredicate = "1 = 0";
    static readonly string EntityScan = $"SELECT DISTINCT e.GlobalId FROM {FactTable.Entities.Identifier} e";
    static readonly string CategoryColumn = $"(SELECT s.Strings FROM {FactTable.Strings.Identifier} s WHERE s.rowid = e.Category)";
    static readonly string CategoryEquals = $"{CategoryColumn} = ?";
    static readonly string NameColumn = $"(SELECT s.Strings FROM {FactTable.Strings.Identifier} s WHERE s.rowid = e.Name)";

    static readonly Map<string, string> EntityColumns = toMap(Seq(
        (ObjectAttribute.GlobalId.Key, "e.GlobalId"),
        (ObjectAttribute.Name.Key, NameColumn)));
    static readonly string StringParameterEquals = $"EXISTS (SELECT 1 FROM {FactTable.StringParameters.Identifier} p JOIN {FactTable.Descriptors.Identifier} d ON p.Descriptor = d.rowid JOIN {FactTable.Strings.Identifier} dn ON d.Name = dn.rowid JOIN {FactTable.Strings.Identifier} sv ON p.Value = sv.rowid WHERE p.Entity = e.rowid AND dn.Strings = ? AND sv.Strings = ?)";
    static readonly string ParameterPresent = $"EXISTS (SELECT 1 FROM {FactTable.StringParameters.Identifier} p JOIN {FactTable.Descriptors.Identifier} d ON p.Descriptor = d.rowid JOIN {FactTable.Strings.Identifier} dn ON d.Name = dn.rowid WHERE p.Entity = e.rowid AND dn.Strings = ?) OR EXISTS (SELECT 1 FROM {FactTable.DoubleParameters.Identifier} q JOIN {FactTable.Descriptors.Identifier} qd ON q.Descriptor = qd.rowid JOIN {FactTable.Strings.Identifier} qn ON qd.Name = qn.rowid WHERE q.Entity = e.rowid AND qn.Strings = ?)";
}
```

## [04]-[RESEARCH]

(none)
