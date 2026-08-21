# [BIM_ELEMENT_QUERY]

`Rasm.Bim` instantiates the seam `Rasm.Element/Query/predicate#ELEMENT_PREDICATE` closure over `BimLeaf` — one `[Union]` wrapping the seam `ElementLeaf` in a single `Element(...)` arm beside five leaves whose payloads are IFC-schema vocabularies the seam refuses to carry. `ElementQuery` folds a term graph-bound over the seam `ElementGraph`, `PredicateCodec` carries the CONTRACTED wire face, and `StoreLowering` lowers onto the persisted flat projection. One boundary holds this page: no second selection surface exists.

Any refinement this algebra cannot express lands as one new arm at ITS owner, never an untyped escape hatch, and the store phase answers a SUPERSET its in-process residue narrows so live graph and durable estate resolve one question identically.

Selection composes settled owners. `Model/spatial#SPATIAL_STRUCTURE` owns the containment walk whole; seam `ValueBag<V>.Merge` owns the effective property stream under its stamped `InheritanceMode`; `Model/elements#IFC_CLASS` owns the entity-class token; `Relations/relation#EDGE_ALGEBRA` owns the incidence beside its `Generic` long tail [NEUTRAL_EDGE_RULING].

`Rasm.Element` owns the ALGEBRA (`Predicate<TLeaf>`, `ValueMatch`, `RangeBound`, `NodeMatch<TLeaf>`, `MatchVerdict`, `Selection<TKey>`, `WalkDepth`, `PredicateKey`) and this page owns the EVALUATION, because evaluation authority stays with the folder that owns the data it runs over. Rails stay typed — `Combine` gates cross-graph composition, `PredicateCodec.Admit` re-runs every gate, faults lift `Model/faults#FAULT_BAND` BARE.

## [01]-[INDEX]

- [02]-[ELEMENT_SET]: `BimLeaf` the Bim leaf family over the seam closure — the `Element(ElementLeaf)` wrapping arm beside the IFC-schema leaves — the `SpatialReach` containment-reach discriminant, the `ObjectAttribute`/`ValueSource` value vocabularies, the `SetOperation` combinator rows, and `ElementQuery` the graph-bound fold carrying `Selection<NodeId>` with the verdict evidence its walk raised.
- [03]-[PREDICATE_WIRE]: `PredicateWire` the CONTRACT-FROZEN JSON face — `[JsonDerivedType]`-discriminated wire records with primitive payloads — and the `PredicateCodec` one-owner correspondence (`Seal` the accumulating lowering, `Admit` the accumulating re-admission through the standing gates), so a UI-authored filter, a saved view, and a coordination rule travel as data and evaluate in C#.
- [04]-[PREDICATE_PUSHDOWN]: `StorePlan` the term-to-SQL lowering over the persisted BimOpenSchema flat projection — the store-expressible subset lowered to one parameterized DuckDB statement over the suffixed fact tables, the residue folded in-process over the returned candidates — so the SAME selection language spans the live graph and the durable estate under the two-phase broad/narrow law.

## [02]-[ELEMENT_SET]

- Owner: `BimLeaf` is the Bim leaf family the seam `Predicate<TLeaf>` closes over — `Element(ElementLeaf)` the ONE wrapping arm carrying every seam-owned query dimension (kind, classification branch, attribute, property, material, composition, assignment, connection, void, assessment, generic wire) beside the five IFC-SCHEMA leaves the seam declines: `ByClass`/`ByDomain`/`ByPredefinedType` over the `Model/elements#IFC_CLASS` roster, `ByClassificationSystem` the system-membership existential, and `BySpatialContainer` the ceded containment reach. `SpatialReach`, `ObjectAttribute`, `ValueSource`, and `SetOperation` are the policy vocabularies its evaluation and its consumers compose. `ElementQuery` owns graph-bound selection, refinement, set algebra, effective-value reads, baking, and measured aggregation over the seam `Selection<NodeId>`; no wrapper duplicates the term identity.
- Entry: `ElementQuery.Query(ElementGraph graph, BimTerm term)` folds the term over `graph.ObjectNodes`; the seam `And`/`Or`/`AndNot` flatten the boolean closure and `Predicate<BimLeaf>.Open` is the named vacuous conjunction; `BimLeaf.InZone`/`OfType`/`Classified` mint the derived terms whose seam spelling is a composition rather than one arm; `Combine(other, operation, key)` applies one `SetOperation` row after proving both queries share the same graph; `Where` refines the current selection; `Bake(key)` traverses the selected objects through the seam derivation rail; `BimLeaf.Key(term)` streams any term through the seam `PredicateKey` into the content key a memo and a replayable selection share.
- Auto: evaluation is ONE parameterized fold — `Holds<TLeaf>` supplies the seam `Predicate<TLeaf>.Holds` its leaf verdict and its closure verdict, and the two leaf vocabularies (`BimLeaf` at the top, `ElementLeaf` inside every nested `NodeMatch`) instantiate it rather than forking a second walker. Both leaf dispatches are the Thinktecture generated total `Switch`, so a missing query dimension is a build error at every site rather than a silent fallthrough. Classification arms decide over the primary `Classification` AND the co-applied `Classifications` set, so a secondary standard-system code never escapes a branch facet; `ByProperty` decides set-name, property-name, and value through three independent `ValueMatch` restrictions over BOTH bag kinds on occurrence and type, so a patterned `Pset_.*` facet lowers whole without a heavy `Bake`; the incidence arms read the O(degree) `EdgesAt` index and decide every related endpoint through `MatchesNode`, whose `Where` case recurses the same fold on the resolved `Node.Object` so a non-`Object` target fails the nested probe structurally; `BySpatialContainer` reads its `SpatialReach` row's own chain projection, so the reach vocabulary owns the walk and the arm owns none of it.
- Auto: the seam `Closure` arm answers with a GENUINE bounded transitive walk — one `BreadthFirstSearchAlgorithm` over the memoized `graph.View(EdgeFilter.Composition, EdgeOrientation.Ascending)` ascent, `TreeEdge` folding the per-vertex level and `GrayTarget` classifying a non-tree edge onto a queued vertex as the cyclic-`Compose` evidence `Bake` rails on — never an opaque-leaf pass-through, which is the seam's binding evaluator law. `WalkDepth` bounds the level and `WalkDepth.Whole` walks to fixpoint.
- Law: verdicts carry FAULTS. `MatchVerdict` accumulates from both sides of every combinator and `Negate` flips only a CLEAN verdict, so a cyclic ascent or an unresolvable nested target keeps `Holds` false through any surrounding `Not` while its cause rides out on `ElementQuery.Faults` — where the deleted `bool` fold silently delivered or silently dropped a malformed arm and reported a healthy empty selection. Selection stays TOTAL regardless: evidence rides beside the answer, never a rail refusing a whole query for one bad node.
- Law: `Selection<NodeId>.Keys` is ORDER-BEARING in `graph.ObjectNodes` order, so two runs over one snapshot answer byte-identically and `BimLeaf.Key` keys a replayable selection; the deleted `LanguageExt.HashSet<NodeId>` carrier made membership cheap and order arbitrary, so a keyed replay proved nothing about re-deriving the same set. `ElementQuery.Holds` answers O(1) membership off the query's own built-once member set.
- Growth: a new SEAM-owned query dimension is one `ElementLeaf` arm at the seam with one arm on this page's `ElementVerdict` (the generated `Switch` breaks loudly until it lands); a new IFC-SCHEMA dimension is one `BimLeaf` arm with its `CanonicalBytes` ordinal; a new value restriction is one seam `ValueMatch` arm; a new rostered `Generic` family is ZERO query edits — `ByGeneric` parameterizes the wire-name; a new incidence flavor is a `SubKind` value the existing arm already parameterizes; a new cross-page value read composes `ElementQuery.ValuesOf` over the `ValueSource` axis and a cross-page set AGGREGATE composes `ElementQuery.SumOf` (the zone rollup and the system demand accumulation are its two standing consumers), never a re-derived bag merge or a manual `double` fold; a new queryable object attribute is one `ObjectAttribute` row (only when the seam `Node.Object` gains the column); a new set combinator is one `SetOperation` row the derived trio and the gated `Combine` share; a new containment reach is one `SpatialReach` row carrying its chain delegate; the chainage axis is ZERO arms — a station band is `ByProperty` with a `Range` restriction over the station row the `Model/spatial#LINEAR_POSITIONING` reader and the `Semantics/feature#GEO_FEATURE` corridor stamp each land dimensioned on their own bag, an offset band the same `Range` over the offset row, and the alignment identity an `Exact` restriction beside them, so "every element between station 2+400 and 3+100" and "everything within 8 m of centreline" compose the standing arms and an `AlongAlignment`-shaped arm is the second selection surface both owning pages already rule out; never a `Get<Dimension>` operation family and never a parallel selection surface.
- Boundary: `BimLeaf` holds the leaf vocabulary and `ElementQuery` mints only through `Query`, `Where`, or a graph-identity-gated `Combine`; a one-field query wrapper, public arbitrary-set constructor, arity family, or cross-graph `NodeId` merge is invalid. Spatial up-chain law stays elsewhere: `SpatialReach.Ancestry` composes the `Model/spatial#SPATIAL_STRUCTURE` ancestor law and `SpatialReach.Direct` that same owner's `ContainerOf` read, so a local `Contain`/`Aggregate` walk here is the third copy of one traversal and the deleted form. `Range` decides ONLY over a `PropertyValue.Measure` sharing the bound's `Dimension`, so reachability by a magnitude facet is the STAMPING owner's obligation — a projector landing a station, an offset, or any other magnitude as a bare `Number` mints a row no range restriction can ever reach — and widening the restriction to swallow an undimensioned candidate is the deleted form that admits a length against a pressure bound. `Rasm.Element` owns the numeric-equality tolerance whole (`ValueMatch` decides at the IDS relative tolerance in SI value space), so no epsilon is declared here.
- Boundary: every nested `NodeMatch` carries the SEAM leaf vocabulary (`NodeMatch<ElementLeaf>`), because the seam's own incidence arms type their target that way and one nesting vocabulary corpus-wide beats two. NAMED LOSS: a nested pattern reaches the three IFC-schema leaves only through the `PredicateCodec.Elemental` lowering — `ByClass` and `ByDomain` lower onto the seam `ByClassification` branch closure, while `ByPredefinedType`, `ByClassificationSystem`, and `BySpatialContainer` have no nested form and refuse typed at admission. WITNESS: `Review/validation#IDS_FACETS` lowers its `PartOf` container facet to `NodeMatch<ElementLeaf>.Where` over an entity/classification/property pattern, which the lowering carries whole.
- Packages: Rasm.Element (`Query/predicate#ELEMENT_PREDICATE` the whole algebra — `Predicate<TLeaf>`/`ValueMatch`/`RangeBound`/`NodeMatch<TLeaf>`/`MatchVerdict`/`Selection<TKey>`/`WalkDepth`/`ElementLeaf`/`PredicateKey`; `Graph/element#ELEMENT_GRAPH` `ElementGraph`/`View`/`EdgeFilter`/`EdgeOrientation`/`TypedEdge`/`Bake`; `Projection/address#CONTENT_ADDRESS`), QuikGraph (`BreadthFirstSearchAlgorithm` over the memoized view, its `TreeEdge`/`GrayTarget`/`DiscoverVertex` event fan sharing ONE walk), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Error`), Rasm (`Op`).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;         // closed-generic aliases: the bare name
using ElementTerm = Rasm.Element.Query.Predicate<Rasm.Element.Query.ElementLeaf>;   // collides with global-using System.Predicate<T>

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Each reach row carries its OWN chain projection as delegate data, so the BySpatialContainer arm selects a reach
// and never spells a walk. Reach is an EDGE-KIND scope no WalkDepth absorbs: Direct is Contain-only at one level
// where Ancestry is Contain-then-Aggregate to fixpoint.
[SmartEnum<string>]
public sealed partial class SpatialReach {
    public static readonly SpatialReach Direct   = new("direct",   static (graph, node) => SpatialStructure.ContainerOf(graph, node).ToSeq());
    public static readonly SpatialReach Ancestry = new("ancestry", static (graph, node) => SpatialStructure.Ancestry(graph, node));

    [UseDelegateFromConstructor]
    public partial Seq<NodeId> Chain(ElementGraph graph, NodeId node);
}

// ObjectAttribute closes the queryable direct-attribute vocabulary the seam ByAttribute arm reads off a Node.Object;
// its Read projection lifts the attribute to the typed PropertyValue a ValueMatch decides. Name/Tag/GlobalId/ObjectType
// span the COMPLETE seam direct string-column surface — a row lands only when the seam node gains the column.
[SmartEnum<string>]
public sealed partial class ObjectAttribute {
    public static readonly ObjectAttribute Name     = new("Name",     static o => o.Name is { Length: > 0 } n ? Some<PropertyValue>(new PropertyValue.Text(n)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute Tag      = new("Tag",      static o => o.Tag is { Length: > 0 } t ? Some<PropertyValue>(new PropertyValue.Text(t)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute GlobalId = new("GlobalId", static o => o.ExternalId.Map(static e => (PropertyValue)new PropertyValue.Text(e)));
    // ObjectType carries the USERDEFINED designation the Projection/semantic UserLabel ingress lands and
    // Projection/egress StampPredefined re-stamps — queryable because a federation filter selects on the user-defined label a
    // PredefinedType of USERDEFINED leaves otherwise opaque.
    public static readonly ObjectAttribute ObjectType = new("ObjectType", static o => o.ObjectType.Map(static t => (PropertyValue)new PropertyValue.Text(t)));

    [UseDelegateFromConstructor]
    public partial Option<PropertyValue> Read(Node.Object value);
}

// SetOperation closes the combinator vocabulary over the seam Selection<NodeId>: each row carries its fold as data, so
// derived same-graph trio and the graph-identity-gated Combine share ONE combination law and a new combinator is
// one row — never a fourth sibling method body.
[SmartEnum<string>]
public sealed partial class SetOperation {
    public static readonly SetOperation Union     = new("union",     static (left, right) => left.Union(right));
    public static readonly SetOperation Intersect = new("intersect", static (left, right) => left.Intersect(right));
    public static readonly SetOperation Except    = new("except",    static (left, right) => left.Except(right));

    [UseDelegateFromConstructor]
    public partial Selection<NodeId> Apply(Selection<NodeId> left, Selection<NodeId> right);
}

// ValueSource names the axis a cross-page consumer reads an element value by — the direct ObjectAttribute row or the
// effective (type→occurrence-merged) Pset/Qto property. Review/coordination#COORDINATION Unique carries it; the
// effective-value merge stays THIS page's one owner, exposed through ElementQuery.ValuesOf, never re-derived
// at a consumer (the named seam-bag-merge drift).
[Union]
public abstract partial record ValueSource {
    private ValueSource() { }

    public sealed record Attribute(ObjectAttribute Key) : ValueSource;
    public sealed record Property(string Set, string Name) : ValueSource;
}

// BimLeaf instantiates the seam closure over Bim's vocabulary: ONE Element arm wraps the whole seam leaf family so
// a mixed expression stays one value, and an arm survives beside it only where its payload is an IFC-SCHEMA
// vocabulary the seam declares out of scope (Classification/classification rules the IfcClass roster and the
// PredefinedType valid-set Bim's) or a capability the seam ceded (E-E9 spatial ancestry).
[Union]
public abstract partial record BimLeaf {
    private BimLeaf() { }

    public sealed record Element(ElementLeaf Leaf) : BimLeaf;
    public sealed record ByClass(IfcClass Class) : BimLeaf;                               // exact IFC entity class — a roster ROW, not a code string
    public sealed record ByDomain(IfcDomain Domain) : BimLeaf;                            // the IfcClass.Domain discipline partition
    public sealed record ByPredefinedType(IfcClass Class, PredefinedType Type) : BimLeaf; // entity class + the typed predefined token
    // System-only membership — classified in the system at ANY code (the IDS no-identification facet). The seam
    // arm carries a RESOLVED branch closure, so this existential has no closure to hand in and stays here.
    public sealed record ByClassificationSystem(string System) : BimLeaf;
    public sealed record BySpatialContainer(NodeMatch<ElementLeaf> Container, SpatialReach Reach) : BimLeaf;

    // Derived terms whose seam spelling is a COMPOSITION rather than one arm mint once here, so no consumer
    // re-derives the correspondence: zone membership is the Assign{Group} logical modality OR the Compose{Reference}
    // spatial one (the zones MembersOf pair the seam splits across two arms), a type bind is the TypeDefinition
    // assignment, and a classification branch is the bSDD-resolved closure the seam takes as payload.
    public static BimTerm InZone(NodeMatch<ElementLeaf> group) => new BimTerm.Any(Seq<BimTerm>(
        Of(new ElementLeaf.ByAssigned(AssignKind.Group, group)),
        Of(new ElementLeaf.ByComposed(ComposeKind.Reference, group))));

    public static BimTerm OfType(NodeMatch<ElementLeaf> type) => Of(new ElementLeaf.ByAssigned(AssignKind.TypeDefinition, type));

    public static BimTerm Classified(Seq<Classification> branch) => Of(new ElementLeaf.ByClassification(branch));

    public static BimTerm Of(ElementLeaf leaf) => new BimTerm.Leaf(new Element(leaf));

    // CanonicalBytes is the Bim-vocabulary leaf writer the seam PredicateKey composes: its Element arm delegates to the
    // seam's own writer, so one predicate keys through one projection and ordinals stay frozen per family.
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

// --- [MODELS] -----------------------------------------------------------------------------
// ElementQuery binds one selection to its graph: the seam Selection<NodeId> carries the answer, the ElementGraph the
// scope Bake and every incidence read need, and Faults the DISTINCT evidence the verdict fold raised. That binding is the named
// loss the wrapper preserves — a bare Selection<NodeId> could not rail ElementFault on a cyclic Compose at Bake.
public sealed record ElementQuery {
    // IfcSystem names the entity-class token the Projection/semantic#SEMANTIC_PROJECTOR Objects fold stamps onto every
    // Node.Object as Classification("ifc", IfcClass.Key); the seam admission lower-cases the system, so
    // ByClass/ByDomain/ByPredefinedType match the lower-case token, the IfcClass roster staying the projector's.
    internal const string IfcSystem = "ifc";

    static readonly Op Gate = Op.Of(name: nameof(ElementQuery));

    private ElementQuery(ElementGraph graph, Selection<NodeId> selection, Seq<Error> faults) {
        (Graph, Selection, Faults) = (graph, selection, faults);
        members = toHashSet(selection.Keys);   // built ONCE per query — Objects and every consumer membership test read it
    }

    readonly LanguageExt.HashSet<NodeId> members;

    public ElementGraph Graph { get; }
    public Selection<NodeId> Selection { get; }
    public Seq<Error> Faults { get; }

    public static ElementQuery Query(ElementGraph graph, BimTerm term) => Fold(graph, graph.ObjectNodes, term);

    // One Where, one modality: refine the current selection by another term — re-folds ONLY the current members,
    // not the whole graph — so the closed algebra stays the single selection surface; a raw Func<Node.Object, bool>
    // escape hatch is the deleted form (a refinement the algebra cannot express is one new arm at its owner).
    // Evidence is CUMULATIVE: a fault the first fold raised survives the refinement that never re-visited its node.
    public ElementQuery Where(BimTerm term) {
        ElementQuery refined = Fold(Graph, Objects, term);
        return new(Graph, refined.Selection, (Faults + refined.Faults).Distinct().Strict());
    }

    public int Count => Selection.Count;
    public Seq<NodeId> Ids => Selection.Keys;
    public Seq<Node.Object> Objects => Graph.ObjectNodes.Filter(o => members.Contains(o.Id));
    public Seq<string> GlobalIds => Objects.Choose(static o => o.ExternalId);
    public bool Holds(NodeId id) => members.Contains(id);

    // Bake is the one Fin-railed step: it derives every selected object into the seam Element ("has it all"),
    // railing ElementFault on a cyclic Compose or an absent root the selection never reaches in a healthy graph.
    public Fin<Seq<Element>> Bake(Op key) => Objects.TraverseM(o => Graph.Bake(o.Id, key)).As();

    // Union/Intersect/Except name one-hop conveniences over the SetOperation rows for the refinement
    // partitions a policy delegate composes (both operands minted from THIS graph — the IDS cardinality and
    // coordination Require/Prohibit partition rows); two independently-held queries meet through Combine.
    public ElementQuery Union(ElementQuery other) => Derive(SetOperation.Union, other);
    public ElementQuery Intersect(ElementQuery other) => Derive(SetOperation.Intersect, other);
    public ElementQuery Except(ElementQuery other) => Derive(SetOperation.Except, other);

    // Combine is the one cross-query meet: two independently-minted selections prove they share ONE graph before any
    // id algebra, because a cross-graph merge mints ids no downstream Objects read could tell from an honest empty
    // refinement. CONTENT identity decides (ContentAddress.OfGraph), since the reloaded snapshot this meet serves is
    // a different instance of one graph; reference identity stays a fast path, never the verdict.
    public Fin<ElementQuery> Combine(ElementQuery other, SetOperation operation, Op key) =>
        ReferenceEquals(Graph, other.Graph) || ContentAddress.OfGraph(Graph) == ContentAddress.OfGraph(other.Graph)
            ? Fin.Succ(Derive(operation, other))
            : Fin.Fail<ElementQuery>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "set-cross-graph", operation.Key })));

    ElementQuery Derive(SetOperation operation, ElementQuery other) =>
        new(Graph, operation.Apply(Selection, other.Selection), (Faults + other.Faults).Distinct().Strict());

    // Fold mints every selection: a member is selected on a HOLDING verdict, and every fault any verdict raised
    // rides out beside the answer deduplicated — total, so one malformed sub-term never refuses the query.
    static ElementQuery Fold(ElementGraph graph, Seq<Node.Object> candidates, BimTerm term) {
        Seq<(NodeId Id, MatchVerdict Verdict)> verdicts = candidates.Map(o => (o.Id, Verdict(graph, o, term))).Strict();
        return new(graph,
            new Selection<NodeId>(verdicts.Filter(static v => v.Verdict.Holds).Map(static v => v.Id).Strict(), Option<UInt128>.None),
            verdicts.Bind(static v => v.Verdict.Faults).Distinct().Strict());
    }

    // --- [PREDICATE_FOLD]
    // ONE parameterized fold serves BOTH leaf vocabularies: BimLeaf at the top and ElementLeaf inside every nested
    // NodeMatch. The leaf verdict is the caller's row, the closure verdict is Reach, and the seam Holds owns the
    // boolean structure — so the All/Any/Not recursion is never re-spelled here.
    // PUBLIC: the ONE per-node verdict a consumer holding its own candidate reaches (the Projection/semantic
    // IfcLegality endpoint gate is the standing one — it decides a single resolved node against a rule term and
    // needs the evidence, not a selection). The graph argument serves the incidence and closure arms alone, so a
    // node the graph does not yet carry still answers correctly for the payload arms that read it directly.
    public static MatchVerdict Verdict(ElementGraph graph, Node.Object obj, BimTerm term) => Holds(graph, obj, term, BimVerdict);

    static MatchVerdict Holds<TLeaf>(ElementGraph graph, Node.Object obj, Predicate<TLeaf> term, Func<ElementGraph, Node.Object, TLeaf, MatchVerdict> leaf)
        where TLeaf : notnull =>
        term.Holds(l => leaf(graph, obj, l), walk => Reach(graph, obj, walk, leaf));

    // Reach answers the seam Closure arm with a GENUINE bounded transitive walk (the Persistence evaluator law binds
    // every consumer): one BFS over the memoized ascending composition view, TreeEdge folding each vertex's level and
    // GrayTarget classifying a non-tree edge onto a QUEUED vertex — the cyclic Compose the Bake fold also rails.
    // Every vertex within the bound tests the seed, the candidate itself at level 0 included.
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

    // BimVerdict decides the Bim leaf: its Element arm delegates the whole seam vocabulary to ElementVerdict and the
    // five IFC-schema arms decide here, so no dimension is spelled twice.
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

    // ElementVerdict decides the seam leaf across eleven arms, every incidence read through the O(degree) EdgesAt
    // index. ByClassification carries the RESOLVED branch the bSDD resolver hands in, so membership is set
    // containment over (System, Code, Edition) identity and no ancestry derives here.
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

    // ONE incidence decision every edge arm shares: the arm supplies the edge-to-related-endpoint projection and
    // this fold walks the index once, so the five arms that differed only in which endpoint they read collapse.
    static MatchVerdict Incident(ElementGraph graph, NodeId self, NodeMatch<ElementLeaf> target, Func<Relationship, Option<NodeId>> related) =>
        toSeq(graph.EdgesAt(self)).Choose(related)
            .Fold(MatchVerdict.Of(false), (acc, candidate) => acc.Or(MatchesNode(graph, target, candidate)));

    // MatchesNode decides an incidence target: an Exact id equality, or a Where nested pattern resolved on the related
    // Object node and recursed through the SAME fold over the SEAM leaf vocabulary — a non-Object target fails the nested
    // probe structurally, so Where never lies about a bag/material/assessment node.
    static MatchVerdict MatchesNode(ElementGraph graph, NodeMatch<ElementLeaf> target, NodeId candidate) => target.Switch(
        state: (graph, candidate),
        exact: static (s, t) => MatchVerdict.Of(t.Id == s.candidate),
        where: static (s, t) => s.graph.Find<Node.Object>(s.candidate)
                                    .Map(o => Holds(s.graph, o, t.Pattern, ElementVerdict))
                                    .IfNone(MatchVerdict.Of(false)));

    // --- [VALUE_READS]
    // ValuesOf is the PUBLIC effective-value read over one node — the ONE exposure a cross-page consumer composes
    // instead of re-deriving the seam bag merge.
    public static Seq<PropertyValue> ValuesOf(ElementGraph graph, Node.Object obj, ValueSource source) => source.Switch(
        attribute: a => a.Key.Read(obj).ToSeq(),
        property:  p => EffectiveValues(graph, obj.Id,
                            new ValueMatch.Exact(new PropertyValue.Text(p.Set)),
                            new ValueMatch.Exact(new PropertyValue.Text(p.Name))));

    // SumOf rails the SET-aggregate read over the one ValuesOf exposure — the zone rollup and the system demand
    // accumulation COMPOSE this fold. None means no value exists; a present non-measure is typed failure, never
    // silently discarded into a partial sum.
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

    // EffectiveValues streams candidate set names from BOTH bag kinds on occurrence AND type under its Set
    // restriction; each surviving set resolves through the seam ValueBag.Merge under its stamped InheritanceMode, and
    // a Qto_* quantity wraps as PropertyValue.Measure — so a patterned (SetName, Name) facet reads without a Bake.
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

    // ONE edge walk gathers BOTH bag kinds a node's own Assign{PropertyDefinition} edges attach — the (Props, Qty)
    // shape the seam Bake.TypeBagsOf reads — so property and quantity resolution share one walk; a caller reaches a
    // type object's bags by walking from its TypeIdOf id, under the SAME edge conventions the seam Bake reads.
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

    // One named bag resolution for BOTH aliases (PropertyBag/QuantityBag are ValueBag<V> global-using aliases): the
    // occurrence bag matching SetName merges with its type counterpart via the ONE seam ValueBag<V>.Merge (the
    // occurrence carrying the stamped InheritanceMode), a type-only bag inheriting as-is — never a per-alias pair.
    static Option<ValueBag<V>> Resolve<V>(Seq<ValueBag<V>> occurrence, Seq<ValueBag<V>> type, string setName) =>
        occurrence.Find(b => b.SetName == setName).Match(
            Some: occ => Some(type.Find(b => b.SetName == setName).Match(Some: typ => ValueBag<V>.Merge(typ, occ), None: () => occ)),
            None: () => type.Find(b => b.SetName == setName));
}
```

## [03]-[PREDICATE_WIRE]

- Owner: `PredicateWire` is the CONTRACT-FROZEN JSON face of the selection language — one `[JsonDerivedType]`-discriminated sealed family whose case names ARE the discriminators and whose payloads are primitives, so the wire is authorable in a browser filter builder, storable as a saved view, and carried on a `Review/coordination#COORDINATION` rule as data; `PredicateCodec` the ONE correspondence owner carrying both directions — `Seal` the domain→wire lowering, `Admit` the wire→domain re-admission — never direction-named sibling owners. `tests/contracts/MANIFEST.md` freezes this face under `Model/query#PREDICATE_WIRE` with the TypeScript `interchange/codec#LANDING_WIRE` duplex mirror live, so the record roster, the discriminator tokens, and the field names are IMMUTABLE and only the interior mapping moves.
- Entry: `PredicateCodec.Seal(BimTerm term)` lowers each arm onto its wire record; `PredicateCodec.Admit(PredicateWire wire, Op key)` re-admits through the STANDING gates — `ValueMatch.Pattern.Of` re-compiles a pattern facet, `IfcClass.Resolve` re-admits a class key, `Classification.Of` re-admits a branch root, the sub-kind vocabularies re-admit through their generated `Validate` — so a hostile or stale wire never mints an unevaluable term. Both directions return `Validation<Error, T>` and a consumer needing one value lowers the accumulated errors through the standard rail.
- Auto: both directions ACCUMULATE. `All`/`Any` operands are independent, so a filter builder saving a nine-term view learns every unsealable term in one apply and a saved view loading against a moved vocabulary learns every stale token at once — where the deleted first-fail `Fin` chain reported one defect and hid the rest. `ValueMatchWire`/`NodeMatchWire`/`RangeBoundWire` sub-families mirror their vocabularies; measure-valued payloads travel as `(double Si, string Type, int[7] Dimension)` triples re-admitted through `MeasureValue.OfSi` over `Dimension.Create`, so the wire never carries an `IfcValue` or a locale-rendered number.
- Law: the frozen face expresses a SUBSET of the interior algebra and `Seal` REFUSES the remainder by name rather than misfiling it. Four terms have no discriminator: the seam `Closure` walk, a `ByAssigned` on any kind outside `{TypeDefinition, Group}`, a `ByClassification` whose branch is not exactly one edition-blank classification, and the seam's `ValueMatch.Prefix` facet. Each refusal is a `BimFault.Refused` with `BimReason.Capability` raise naming the arm, so an author learns which term cannot become a shared view instead of receiving a view that means something else. Widening the face is a MANIFEST change with a TypeScript mirror row, never a codec edit.
- Law: `ByClassificationWire` carries the branch ROOT, never a resolved closure. Freezing a dictionary expansion onto a wire goes stale the instant bSDD re-editions the standard, so `Admit` lands the one-element branch and a consumer expands it through `Semantics/classification#BSDD_RESOLUTION` before querying — which is why `Seal` refuses a multi-element branch rather than writing its head.
- Law: `AdmitNode` lowers a nested wire term onto the SEAM leaf vocabulary through `Elemental` — `ByClass` and `ByDomain` become `ByClassification` branch closures (an entity class IS the `"ifc"`-system classification the projector stamps, and a domain is that roster's partition), while `ByPredefinedType`, `ByClassificationSystem`, and `BySpatialContainer` have no nested form and raise `BimFault.Refused` with `BimReason.Capability`. `Seal` never needs the lowering, because a nested `NodeMatch<ElementLeaf>` is already seam-shaped.
- Receipt: the sealed wire is the shareable-selection artifact — a saved view replayed against a reloaded snapshot re-admits and re-queries, a UI-authored coordination rule carries its applicability/requirement pair as two wires, and the store plan lowers an ADMITTED term, so a UI-authored query executes at store scale with zero second selection surface. Byte round-trip witness: `Admit(Seal(t))` reproduces `t` for every face-expressible `t`, and `Seal(Admit(w))` reproduces `w` byte-for-byte for every well-formed `w`, because each wire arm admits into exactly the interior shape its `Seal` arm inverts.
- Packages: System.Text.Json (`[JsonDerivedType]`/`[JsonPolymorphic]` — the closed-family polymorphic contract), Rasm.Element (the seam algebra and its `Pattern.Of`/`MeasureValue.OfSi`/`Classification.Of` admissions), `Projection/relations#RELATION_ALGEBRA` (`IfcRelKind.Admit` — the decode-edge roster proof that page names as its open obligation), `Model/elements#IFC_CLASS` (`IfcClass.Resolve`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Validation` applicative, `Traverse`), Rasm.
- Growth: a new interior arm needs NO wire edit — it falls outside the face and refuses by name until the contract widens; a face widening is one wire record, one `Seal` arm, one `Admit` arm, one MANIFEST row, and one TypeScript mirror member, landed together. Every widening stays ADDITIVE (a new derived type never re-keys an existing discriminator), the versionable property every package wire holds.
- Boundary: the wire family is protocol-shaped at the edge and the interior union carries NO codec attributes — `PredicateWire` is the DTO family, `BimLeaf` never serializes directly; re-admission is ADMISSION (every gate re-runs) so a wire minted by an older vocabulary faults typed instead of resurrecting a retired arm; the codec is one owner with both directions and a `ToWire`/`FromWire` sibling pair or a per-arm converter family is the deleted form. Direction splits asymmetrically BY OWNERSHIP: `Seal` dispatches the closed interior unions through their generated `Switch` and refuses only where the FACE is narrower, while `Admit` pattern-matches the genuinely OPEN wire families and faults typed on an unregistered record — a match-all arm on the closed side is the deleted form that lowers an unrostered restriction to a term selecting everything.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using Rasm.Bim.Projection;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Rasm.Element.Relations;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;
using ElementTerm = Rasm.Element.Query.Predicate<Rasm.Element.Query.ElementLeaf>;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// PredicateWire closes the CONTRACT-FROZEN face: one [JsonDerivedType] row per expressible term, primitive payloads
// only, closed by the [JsonPolymorphic] contract. Every record name, discriminator token, and field name below is
// frozen by tests/contracts/MANIFEST.md and mirrored in the TypeScript duplex schema.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "arm")]
[JsonDerivedType(typeof(ByClassWire), "class")]
[JsonDerivedType(typeof(ByDomainWire), "domain")]
[JsonDerivedType(typeof(ByPredefinedTypeWire), "predefined")]
[JsonDerivedType(typeof(ByClassificationWire), "classification")]
[JsonDerivedType(typeof(ByClassificationSystemWire), "classificationSystem")]
[JsonDerivedType(typeof(ByKindWire), "kind")]
[JsonDerivedType(typeof(ByAttributeWire), "attribute")]
[JsonDerivedType(typeof(ByPropertyWire), "property")]
[JsonDerivedType(typeof(ByMaterialWire), "material")]
[JsonDerivedType(typeof(BySpatialContainerWire), "spatialContainer")]
[JsonDerivedType(typeof(ByComposedWire), "composed")]
[JsonDerivedType(typeof(ByTypeWire), "type")]
[JsonDerivedType(typeof(ByZoneWire), "zone")]
[JsonDerivedType(typeof(ByConnectedWire), "connected")]
[JsonDerivedType(typeof(ByVoidedWire), "voided")]
[JsonDerivedType(typeof(ByAssessmentWire), "assessment")]
[JsonDerivedType(typeof(ByGenericWire), "generic")]
[JsonDerivedType(typeof(AllWire), "all")]
[JsonDerivedType(typeof(AnyWire), "any")]
[JsonDerivedType(typeof(NotWire), "not")]
public abstract record PredicateWire;

public sealed record ByClassWire(string Class) : PredicateWire;
public sealed record ByDomainWire(string Domain) : PredicateWire;
public sealed record ByPredefinedTypeWire(string Class, string Token) : PredicateWire;
public sealed record ByClassificationWire(string System, string Code) : PredicateWire;
public sealed record ByClassificationSystemWire(string System) : PredicateWire;
public sealed record ByKindWire(string Kind) : PredicateWire;
public sealed record ByAttributeWire(ValueMatchWire Attribute, ValueMatchWire Restriction) : PredicateWire;
public sealed record ByPropertyWire(ValueMatchWire Set, ValueMatchWire Name, ValueMatchWire Restriction) : PredicateWire;
public sealed record ByMaterialWire(ValueMatchWire Restriction) : PredicateWire;
public sealed record BySpatialContainerWire(NodeMatchWire Container, string Reach) : PredicateWire;
public sealed record ByComposedWire(string SubKind, NodeMatchWire Whole) : PredicateWire;
public sealed record ByTypeWire(NodeMatchWire Type) : PredicateWire;
public sealed record ByZoneWire(NodeMatchWire Group) : PredicateWire;
public sealed record ByConnectedWire(NodeMatchWire Other, string? Kind) : PredicateWire;
public sealed record ByVoidedWire(string SubKind, NodeMatchWire Other) : PredicateWire;
public sealed record ByAssessmentWire(string Discipline, string? Outcome) : PredicateWire;
public sealed record ByGenericWire(string WireName, NodeMatchWire Other) : PredicateWire;
public sealed record AllWire(PredicateWire[] Operands) : PredicateWire;
public sealed record AnyWire(PredicateWire[] Operands) : PredicateWire;
public sealed record NotWire(PredicateWire Operand) : PredicateWire;

// Sub-family mirrors carry the value restriction, the incidence target, and the measure triple — each re-admitted
// through its standing gate on Admit. BoundWire.Inclusive is the WIRE column the seam RangeBound arm pair projects
// onto; the domain keeps the arms whose dimension-gated AllowsLower/AllowsUpper a bool could never carry.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "match")]
[JsonDerivedType(typeof(PresentWire), "present")]
[JsonDerivedType(typeof(ExactTextWire), "exact")]
[JsonDerivedType(typeof(ExactMeasureWire), "exactMeasure")]
[JsonDerivedType(typeof(PatternWire), "pattern")]
[JsonDerivedType(typeof(RangeWire), "range")]
[JsonDerivedType(typeof(OneOfWire), "oneOf")]
[JsonDerivedType(typeof(LengthWire), "length")]
[JsonDerivedType(typeof(DigitsWire), "digits")]
public abstract record ValueMatchWire;

public sealed record PresentWire : ValueMatchWire;
public sealed record ExactTextWire(string Value) : ValueMatchWire;
public sealed record ExactMeasureWire(MeasureWire Value) : ValueMatchWire;
public sealed record PatternWire(string Expression) : ValueMatchWire;
public sealed record RangeWire(BoundWire? Lower, BoundWire? Upper) : ValueMatchWire;
public sealed record OneOfWire(string[] Allowed) : ValueMatchWire;
public sealed record LengthWire(int? Min, int? Max) : ValueMatchWire;
public sealed record DigitsWire(int? Total, int? Fraction) : ValueMatchWire;

public sealed record MeasureWire(double Si, string Type, int[] Dimension);
public sealed record BoundWire(MeasureWire Value, bool Inclusive);
public sealed record NodeMatchWire(string? Exact, PredicateWire? Matching);

// --- [OPERATIONS] -------------------------------------------------------------------------
// PredicateCodec owns the one term↔wire correspondence: Seal the lowering refusing what the frozen face cannot carry,
// Admit the
// re-admission that re-runs every gate. Both accumulate, so one apply reports every defective term.
public static class PredicateCodec {
    public static Validation<Error, PredicateWire> Seal(BimTerm term) => term.Switch(
        state: unit,
        leaf:    static (_, t) => SealLeaf(t.Value),
        all:     static (_, t) => t.Operands.Traverse(Seal).As().Map(static PredicateWire (ops) => new AllWire([.. ops])),
        any:     static (_, t) => t.Operands.Traverse(Seal).As().Map(static PredicateWire (ops) => new AnyWire([.. ops])),
        not:     static (_, t) => Seal(t.Operand).Map(static PredicateWire (op) => new NotWire(op)),
        // Closure has no discriminator on the frozen face, so it refuses by name rather than lowering
        // onto its seed and silently dropping the bound — a saved view that queried one level deep.
        closure: static (_, t) => Unsealable<PredicateWire>("closure"));

    // SealLeaf covers the Bim half: three IFC-schema arms and the spatial reach write their own records, and its
    // Element arm delegates to the seam-vocabulary half. Generated-total on both unions, so a new arm breaks here loudly.
    static Validation<Error, PredicateWire> SealLeaf(BimLeaf leaf) => leaf.Switch(
        state: unit,
        element:                static (_, l) => SealElement(l.Leaf),
        byClass:                static (_, l) => Success<Error, PredicateWire>(new ByClassWire(l.Class.Key)),
        byDomain:               static (_, l) => Success<Error, PredicateWire>(new ByDomainWire(l.Domain.Key)),
        byPredefinedType:       static (_, l) => Success<Error, PredicateWire>(new ByPredefinedTypeWire(l.Class.Key, l.Type.Token)),
        byClassificationSystem: static (_, l) => Success<Error, PredicateWire>(new ByClassificationSystemWire(l.System)),
        bySpatialContainer:     static (_, l) => SealNode(l.Container).Map(PredicateWire (n) => new BySpatialContainerWire(n, l.Reach.Key)));

    static Validation<Error, PredicateWire> SealElement(ElementLeaf leaf) => leaf.Switch(
        state: unit,
        byKind:           static (_, l) => Success<Error, PredicateWire>(new ByKindWire(l.Kind.Key)),
        // Only the branch ROOT crosses: a resolved multi-code closure and an edition-scoped code both exceed those
        // two frozen columns, and writing the head would ship a narrower query wearing the same discriminator.
        byClassification: static (_, l) => l.Branch.Count == 1 && l.Branch[0] is { Edition: "" } root
                              ? Success<Error, PredicateWire>(new ByClassificationWire(root.System, root.Code))
                              : Unsealable<PredicateWire>("classification-branch"),
        byAttribute:      static (_, l) => (SealMatch(l.Name), SealMatch(l.Restriction))
                              .Apply(static PredicateWire (n, r) => new ByAttributeWire(n, r)).As(),
        byProperty:       static (_, l) => (SealMatch(l.Set), SealMatch(l.Name), SealMatch(l.Restriction))
                              .Apply(static PredicateWire (s, n, r) => new ByPropertyWire(s, n, r)).As(),
        byMaterial:       static (_, l) => SealMatch(l.Restriction).Map(static PredicateWire (r) => new ByMaterialWire(r)),
        byComposed:       static (_, l) => SealNode(l.Whole).Map(PredicateWire (n) => new ByComposedWire(l.SubKind.Key, n)),
        byConnected:      static (_, l) => SealNode(l.Other).Map(PredicateWire (n) => new ByConnectedWire(n, l.Kind.Map(static k => k.Key).ValueUnsafe())),
        byVoided:         static (_, l) => SealNode(l.Other).Map(PredicateWire (n) => new ByVoidedWire(l.SubKind.Key, n)),
        byGeneric:        static (_, l) => SealNode(l.Other).Map(PredicateWire (n) => new ByGenericWire(l.Wire.Value, n)),
        byAssessment:     static (_, l) => Success<Error, PredicateWire>(new ByAssessmentWire(l.Discipline.Key, l.Outcome.Map(static o => o.Key).ValueUnsafe())),
        // Only TWO assign kinds have discriminators; the seam parameterizes the whole AssignKind vocabulary, so a
        // property-definition, assessment, or observation incidence refuses rather than misfiling as a zone.
        byAssigned:       static (_, l) => l.Kind == AssignKind.TypeDefinition ? SealNode(l.Other).Map(static PredicateWire (n) => new ByTypeWire(n))
                              : l.Kind == AssignKind.Group ? SealNode(l.Other).Map(static PredicateWire (n) => new ByZoneWire(n))
                              : Unsealable<PredicateWire>($"assigned:{l.Kind.Key}"));

    public static Validation<Error, BimTerm> Admit(PredicateWire wire, Op key) => wire switch {
        ByClassWire w => Rail(IfcClass.Resolve(w.Class, key)).Map(static BimTerm (c) => new BimTerm.Leaf(new BimLeaf.ByClass(c))),
        ByDomainWire w => Vocab(IfcDomain.TryGet(w.Domain, out IfcDomain? d) ? d : null, w.Domain, key).Map(static BimTerm (v) => new BimTerm.Leaf(new BimLeaf.ByDomain(v))),
        ByPredefinedTypeWire w => Rail(IfcClass.Resolve(w.Class, key)).Map(BimTerm (c) => new BimTerm.Leaf(new BimLeaf.ByPredefinedType(c, PredefinedType.Create(w.Token)))),
        ByClassificationWire w => Rail(Classification.Of(w.System, w.Code, key)).Map(static BimTerm (c) => BimLeaf.Classified(Seq(c))),
        ByClassificationSystemWire w => Success<Error, BimTerm>(new BimTerm.Leaf(new BimLeaf.ByClassificationSystem(w.System))),
        ByKindWire w => Vocab(ObjectKind.TryGet(w.Kind, out ObjectKind? k) ? k : null, w.Kind, key).Map(static BimTerm (v) => BimLeaf.Of(new ElementLeaf.ByKind(v))),
        ByAttributeWire w => (AdmitMatch(w.Attribute, key), AdmitMatch(w.Restriction, key)).Apply(static BimTerm (a, r) => BimLeaf.Of(new ElementLeaf.ByAttribute(a, r))).As(),
        ByPropertyWire w => (AdmitMatch(w.Set, key), AdmitMatch(w.Name, key), AdmitMatch(w.Restriction, key)).Apply(static BimTerm (s, n, r) => BimLeaf.Of(new ElementLeaf.ByProperty(s, n, r))).As(),
        ByMaterialWire w => AdmitMatch(w.Restriction, key).Map(static BimTerm (r) => BimLeaf.Of(new ElementLeaf.ByMaterial(r))),
        BySpatialContainerWire w => (AdmitNode(w.Container, key), Vocab(SpatialReach.TryGet(w.Reach, out SpatialReach? sr) ? sr : null, w.Reach, key))
            .Apply(static BimTerm (n, r) => new BimTerm.Leaf(new BimLeaf.BySpatialContainer(n, r))).As(),
        ByComposedWire w => (Vocab(ComposeKind.TryGet(w.SubKind, out ComposeKind? ck) ? ck : null, w.SubKind, key), AdmitNode(w.Whole, key))
            .Apply(static BimTerm (k, n) => BimLeaf.Of(new ElementLeaf.ByComposed(k, n))).As(),
        ByTypeWire w => AdmitNode(w.Type, key).Map(BimLeaf.OfType),
        ByZoneWire w => AdmitNode(w.Group, key).Map(static BimTerm (n) => BimLeaf.Of(new ElementLeaf.ByAssigned(AssignKind.Group, n))),
        ByConnectedWire w => (AdmitNode(w.Other, key), Optional(w.Kind).Traverse(k => Vocab(ConnectKind.TryGet(k, out ConnectKind? c) ? c : null, k, key)).As())
            .Apply(static BimTerm (n, k) => BimLeaf.Of(new ElementLeaf.ByConnected(n, k))).As(),
        ByVoidedWire w => (Vocab(VoidKind.TryGet(w.SubKind, out VoidKind? vk) ? vk : null, w.SubKind, key), AdmitNode(w.Other, key))
            .Apply(static BimTerm (k, n) => BimLeaf.Of(new ElementLeaf.ByVoided(k, n))).As(),
        ByAssessmentWire w => (Vocab(Discipline.TryGet(w.Discipline, out Discipline? di) ? di : null, w.Discipline, key),
                               Optional(w.Outcome).Traverse(o => Vocab(AssessmentOutcome.TryGet(o, out AssessmentOutcome? ao) ? ao : null, o, key)).As())
            .Apply(static BimTerm (d, o) => BimLeaf.Of(new ElementLeaf.ByAssessment(d, o))).As(),
        ByGenericWire w => (AdmitWireName(w.WireName, key), AdmitNode(w.Other, key))
            .Apply(static BimTerm (name, other) => BimLeaf.Of(new ElementLeaf.ByGeneric(name, other))).As(),
        AllWire w => toSeq(w.Operands).Traverse(o => Admit(o, key)).As().Map(static BimTerm (ops) => new BimTerm.All(ops)),
        AnyWire w => toSeq(w.Operands).Traverse(o => Admit(o, key)).As().Map(static BimTerm (ops) => new BimTerm.Any(ops)),
        NotWire w => Admit(w.Operand, key).Map(static BimTerm (op) => new BimTerm.Not(op)),
        _ => Fail<Error, BimTerm>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "wire-case-unknown", "predicate", wire.GetType().Name }))),
    };

    // Elemental down-projects onto the seam leaf vocabulary a nested NodeMatch<ElementLeaf> types: an entity class IS
    // its "ifc"-system classification the projector stamps, so ByClass lowers onto a one-code branch and ByDomain onto
    // that roster's partition; the three arms with no seam twin refuse by name. The boolean
    // closure recurses so a nested All/Any/Not lowers whole.
    static Validation<Error, ElementTerm> Elemental(BimTerm term, Op key) => term.Switch(
        state: key,
        leaf:    static (k, t) => ElementalLeaf(t.Value, k),
        all:     static (k, t) => t.Operands.Traverse(op => Elemental(op, k)).As().Map(static ElementTerm (ops) => new ElementTerm.All(ops)),
        any:     static (k, t) => t.Operands.Traverse(op => Elemental(op, k)).As().Map(static ElementTerm (ops) => new ElementTerm.Any(ops)),
        not:     static (k, t) => Elemental(t.Operand, k).Map(static ElementTerm (op) => new ElementTerm.Not(op)),
        closure: static (k, t) => Elemental(t.Seed, k).Map(ElementTerm (seed) => new ElementTerm.Closure(seed, t.Depth)));

    static Validation<Error, ElementTerm> ElementalLeaf(BimLeaf leaf, Op key) => leaf.Switch(
        state: key,
        element:                static (_, l) => Success<Error, ElementTerm>(new ElementTerm.Leaf(l.Leaf)),
        byClass:                static (k, l) => Rail(Classification.Of(ElementQuery.IfcSystem, l.Class.Key, k))
                                    .Map(static ElementTerm (c) => new ElementTerm.Leaf(new ElementLeaf.ByClassification(Seq(c)))),
        byDomain:               static (k, l) => toSeq(IfcClass.Items).Filter(row => row.Domain == l.Domain)
                                    .Traverse(row => Rail(Classification.Of(ElementQuery.IfcSystem, row.Key, k))).As()
                                    .Map(static ElementTerm (branch) => new ElementTerm.Leaf(new ElementLeaf.ByClassification(branch))),
        byPredefinedType:       static (k, _) => Unreachable<ElementTerm>("predefined", k),
        byClassificationSystem: static (k, _) => Unreachable<ElementTerm>("classificationSystem", k),
        bySpatialContainer:     static (k, _) => Unreachable<ElementTerm>("spatialContainer", k));

    // TOTAL over the closed ValueMatch union through its generated Switch, never a type-pattern ladder: a match-all
    // fallthrough silently widened an unrostered restriction to Present, so a round-trip returned a term selecting
    // that whole graph. Prefix is the seam's AppUi-proved facet and the frozen face carries no discriminator for it.
    static Validation<Error, ValueMatchWire> SealMatch(ValueMatch match) => match.Switch(
        state:   unit,
        present: static (_, _) => Success<Error, ValueMatchWire>(new PresentWire()),
        exact:   static (_, m) => Success<Error, ValueMatchWire>(m.Value is PropertyValue.Measure measure
                                      ? new ExactMeasureWire(SealMeasure(measure.Value))
                                      : new ExactTextWire(m.Value.Render())),
        prefix:  static (_, _) => Unsealable<ValueMatchWire>("value-prefix"),
        pattern: static (_, m) => Success<Error, ValueMatchWire>(new PatternWire(m.Expression)),
        range:   static (_, m) => Success<Error, ValueMatchWire>(new RangeWire(m.Lower.Map(SealBound).ValueUnsafe(), m.Upper.Map(SealBound).ValueUnsafe())),
        oneOf:   static (_, m) => Success<Error, ValueMatchWire>(new OneOfWire([.. m.Allowed])),
        length:  static (_, m) => Success<Error, ValueMatchWire>(new LengthWire(m.Min.ToNullable(), m.Max.ToNullable())),
        digits:  static (_, m) => Success<Error, ValueMatchWire>(new DigitsWire(m.Total.ToNullable(), m.Fraction.ToNullable())));

    static Validation<Error, ValueMatch> AdmitMatch(ValueMatchWire wire, Op key) => wire switch {
        PresentWire => Success<Error, ValueMatch>(new ValueMatch.Present()),
        ExactTextWire w => Success<Error, ValueMatch>(new ValueMatch.Exact(new PropertyValue.Text(w.Value))),
        ExactMeasureWire w => AdmitMeasure(w.Value, key).Map(static ValueMatch (m) => new ValueMatch.Exact(new PropertyValue.Measure(m))),
        PatternWire w => Rail(ValueMatch.Pattern.Of(w.Expression, key)),
        RangeWire w => (Optional(w.Lower).Traverse(b => AdmitBound(b, key)).As(), Optional(w.Upper).Traverse(b => AdmitBound(b, key)).As())
            .Apply(static ValueMatch (lo, hi) => new ValueMatch.Range(lo, hi)).As(),
        OneOfWire w => Success<Error, ValueMatch>(new ValueMatch.OneOf(toSeq(w.Allowed))),
        LengthWire w => Success<Error, ValueMatch>(new ValueMatch.Length(Optional(w.Min), Optional(w.Max))),
        DigitsWire w => Success<Error, ValueMatch>(new ValueMatch.Digits(Optional(w.Total), Optional(w.Fraction))),
        _ => Fail<Error, ValueMatch>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "wire-case-unknown", "value-match", wire.GetType().Name }))),
    };

    static Validation<Error, NodeMatchWire> SealNode(NodeMatch<ElementLeaf> node) => node.Switch(
        state: unit,
        exact: static (_, n) => Success<Error, NodeMatchWire>(new NodeMatchWire(n.Id.Value, null)),
        where: static (_, n) => SealElementTerm(n.Pattern).Map(static NodeMatchWire (p) => new NodeMatchWire(null, p)));

    // SealElementTerm seals a nested seam-vocabulary pattern through the SAME arm set by riding the Element arm — one
    // lowering, so the nested and top-level encodings of one question are byte-identical.
    static Validation<Error, PredicateWire> SealElementTerm(ElementTerm term) => term.Switch(
        state: unit,
        leaf:    static (_, t) => SealElement(t.Value),
        all:     static (_, t) => t.Operands.Traverse(SealElementTerm).As().Map(static PredicateWire (ops) => new AllWire([.. ops])),
        any:     static (_, t) => t.Operands.Traverse(SealElementTerm).As().Map(static PredicateWire (ops) => new AnyWire([.. ops])),
        not:     static (_, t) => SealElementTerm(t.Operand).Map(static PredicateWire (op) => new NotWire(op)),
        closure: static (_, _) => Unsealable<PredicateWire>("closure"));

    // Exactly one leg populated — the both-and-neither shapes refuse here, which is the refusal the TypeScript
    // mirror re-states as its own node-match-exclusive filter.
    static Validation<Error, NodeMatch<ElementLeaf>> AdmitNode(NodeMatchWire wire, Op key) => (wire.Exact, wire.Matching) switch {
        ({ } raw, null) => NodeId.Validate(raw, null, out NodeId? id) is { } fault
            ? Fail<Error, NodeMatch<ElementLeaf>>(fault)
            : Success<Error, NodeMatch<ElementLeaf>>(new NodeMatch<ElementLeaf>.Exact(id!)),
        (null, { } pattern) => Admit(pattern, key).Bind(t => Elemental(t, key)).Map(static NodeMatch<ElementLeaf> (p) => new NodeMatch<ElementLeaf>.Where(p)),
        _ => Fail<Error, NodeMatch<ElementLeaf>>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "node-match-wire-ambiguous" }))),
    };

    static MeasureWire SealMeasure(MeasureValue measure) => new(measure.Si, measure.Type.Value,
        [measure.Dimension.Length, measure.Dimension.Mass, measure.Dimension.Time, measure.Dimension.Current,
         measure.Dimension.Temperature, measure.Dimension.Amount, measure.Dimension.LuminousIntensity]);

    static BoundWire SealBound(RangeBound bound) => bound.Switch(
        inclusive: static b => new BoundWire(SealMeasure(b.Value), Inclusive: true),
        exclusive: static b => new BoundWire(SealMeasure(b.Value), Inclusive: false));

    static Validation<Error, MeasureValue> AdmitMeasure(MeasureWire wire, Op key) =>
        wire.Dimension is [var l, var m, var t, var i, var th, var n, var j]
            ? Rail(MeasureValue.OfSi(QuantityType.Create(wire.Type), Dimension.Create(l, m, t, i, th, n, j), wire.Si, key: key)
                .MapFail(_ => (Error)new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "measure-wire-reject", wire.Type }))))
            : Fail<Error, MeasureValue>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "measure-wire-dimension", wire.Type })));

    static Validation<Error, RangeBound> AdmitBound(BoundWire wire, Op key) =>
        AdmitMeasure(wire.Value, key).Map(m => wire.Inclusive ? (RangeBound)new RangeBound.Inclusive(m) : new RangeBound.Exclusive(m));

    // IfcRelKind IS the vocabulary (Projection/relations#RELATION_ALGEBRA): a crossed name admits as a WireName and
    // then proves against IfcRelKind, so a name no row declares never mints an unevaluable incidence term.
    static Validation<Error, WireName> AdmitWireName(string raw, Op key) =>
        WireName.Validate(raw, null, out WireName? name) is { } fault
            ? Fail<Error, WireName>(fault)
            : Rail(IfcRelKind.Admit(name!, key)).Map(_ => name!);

    // Rail spells the ONE Fin→Validation crossing: every gate below this page rails, every direction here accumulates,
    // and one spelling keeps any arm from re-deciding which algebra it reports on.
    static Validation<Error, T> Rail<T>(Fin<T> gate) => gate.Match(Succ: Success<Error, T>, Fail: Fail<Error, T>);

    static Validation<Error, T> Vocab<T>(T? row, string token, Op key) where T : class =>
        row is null ? Fail<Error, T>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "predicate-wire-token", typeof(T).Name, token }))) : Success<Error, T>(row);

    static readonly Op Face = Op.Of(name: nameof(PredicateCodec));
    static Validation<Error, T> Unsealable<T>(string arm) => Fail<Error, T>(new BimFault.Refused(Face, BimScope.Model, BimReason.Capability, string.Join(':', new object?[] { "predicate-wire-unsealable", arm })));
    static Validation<Error, T> Unreachable<T>(string arm, Op key) => Fail<Error, T>(new BimFault.Refused(key, BimScope.Model, BimReason.Capability, string.Join(':', new object?[] { "predicate-nested-unreachable", arm })));
}
```

## [04]-[PREDICATE_PUSHDOWN]

- Owner: `StorePlan` the store-side evaluation artifact — ONE parameterized SQL statement over the persisted BimOpenSchema flat fact tables and the in-process `Residue` term — and `StoreLowering.Lower` the two-phase split: the store-expressible subset lowers to SQL, the residue folds in-process over the returned candidates, and the split is SOUND by construction (the SQL phase selects a SUPERSET — a conjunction narrows with its expressible conjuncts and parks the rest on the residue; a disjunction lowers only when EVERY operand lowers, else the whole branch is residue; a negation lowers only over a lowerable operand whose clause is TOTAL, because SQL's third value makes `NOT` over a nullable fact column exclude exactly the absent-column rows the negation selects) — the same broad/narrow law the geospatial H3 prefilter holds at bit parity.
- Entry: `StoreLowering.Lower(BimTerm term, Op key)` folds the term into a `StorePlan` — `Sql` the one `SELECT DISTINCT e.GlobalId` statement over the `FactTable.Entities` scan whose predicates join the remaining rows (`Strings` by `rowid` for the string-index columns, `StringParameters`/`DoubleParameters` through `Descriptors` for the property facts), `Parameters` the positional value list (every dynamic value a parameter — raw-string interpolation into engine SQL is the deleted form the Persistence trust gate names), `Residue` the remainder re-checked in-process; the executing lane is the `Rasm.Persistence/Query/columnar#COLUMNAR_LANE` analytical session, and the returned GlobalId candidates re-enter the algebra as `ByAttribute(GlobalId, OneOf(candidates))` conjoined with the residue over the materialized graph, so the store phase and the in-process phase agree bit-for-bit on the final set.
- Auto: the expressible leaves are the flat projection's own axes — `ByClass` compares the `Category` fact, `ByDomain` expands to the roster's class-key partition (`IfcClass.Items` filtered by `Domain`, one `IN` parameter list), the seam `ByAttribute` over `GlobalId`/`Name` compares the entity columns, and the seam `ByProperty` with exact set/name restrictions lowers `Exact`/`OneOf`/`Range`/`Present` onto the parameter tables (`Value` string equality through the `FactTable.Strings` join, numeric bounds on the `FactTable.DoubleParameters` `Value` column — SI magnitudes by the fact convention); every classification, incidence, zone, spatial, patterned, transitive-`Closure`, and nested-`Where` term is residue, because graph topology stays the graph's and the flat projection carries no classification table beside the single `Category` column. `IN` lists narrow only over a NON-EMPTY value set: an empty set lowers to the canonical FALSE predicate through the one `InFragment` mint, because emitting `IN ()` breaks the statement and dropping the fragment widens the superset into a scan the residue never narrows.
- Receipt: the `StorePlan` is the estate-scale query evidence — "every fire-rated door on any current model" runs WHERE the data rests, saved queries and federation-wide reporting execute the same closed algebra, and the plan's `Residue` names exactly what ran in-process so the split is auditable per query. Chainage pushes DOWN whole: an exact set/name `ByProperty` carrying a `Range` lowers onto the `FactTable.DoubleParameters` SI-magnitude column and the alignment identity beside it onto the string join, so a station band over a whole infrastructure estate is one statement with an empty residue.
- Packages: Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions (the `FactTable` `[SmartEnum<string>]` row table), Rasm; the fact-table vocabulary is the `Ara3D.BimOpenSchema` record surface (`Entity(LocalId, GlobalId, Document, Name, Category)`, `ParameterString`/`ParameterDouble(Entity, Descriptor, Value)`, `ParameterDescriptor(Name, Units, Group, Type)`, `EntityRelation(EntityA, EntityB, RelationType)` — decompile-verified; the `<Name>_<n>` projection-ordinal identifiers and the single-column `Strings` adapter are the `libs/csharp/Rasm.Persistence/.api/api-ara3d-bimopenschema.md` `[IMPLEMENTATION_LAW]` law, that catalogue owning the package at the Persistence tier).
- Growth: a new expressible leaf is one `Fragment` case in the lowering fold (the SQL text, its parameter rows, and its totality verdict), zero executor edits; a new fact column is the flat projection's row and one comparison fragment; a re-ordered serializer projection is one `FactTable` `Ordinal` edit and zero fragment edits; never a second selection language and never a store-side term vocabulary beside the algebra.
- Boundary: the lowering emits SQL TEXT + parameters and never opens a connection — execution is the Persistence analytical lane's (the `ColumnarSession` refcounted anchor, the `Query/lane#READ_ROUTING` staleness gate), so the plan crosses the seam as data on the standing `BimOpenSchema` projection edge; the FACT CONVENTION is Bim's half of that seam — `GlobalId` = the node `ExternalId`, `Category` = the `"ifc"` classification code, a parameter descriptor `Name` = the `{Set}.{Name}` dot-path with `ParameterDouble.Value` the SI magnitude, and every parameter fact the EFFECTIVE value with its type→occurrence merge already resolved under the stamped `InheritanceMode` — the BIM-typed projection `columnar.md` rules Bim-implemented; that materialization is what makes the SQL phase provably a SUPERSET, because an occurrence-only projection puts a `ByProperty` lowering UNDER the in-process answer by dropping every type-inherited value, and a residue narrows but never widens; the table IDENTIFIER is the other half — the `<Stem>_<Ordinal>` name is a serializer emit-order fact the Persistence catalogue owns, so every fragment derives it from a `FactTable` row and a transcribed suffixed literal is the deleted form that survives a re-ordered projection as a name still resolving against the wrong table; the residue split is a correctness law, not an optimization: a lowering that narrows the superset silently drops rows the residue can never recover and is the deleted form — an `Any` lowered as its expressible operands alone, and a `NOT` lowered over a non-total clause, are its two standing instances, the second being why `Fragment` carries a totality verdict rather than a `NOT` wrapper trusting SQL comparison to be two-valued.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Each row pairs a joined table's stem with the serializer's fixed IDataSet.Tables emit ORDINAL the
// `<Stem>_<Ordinal>` DuckDB identifier carries, so a re-ordered projection moves ONE row here; a transcribed
// `Entities_4` literal survives that re-order as a name still resolving, silently skewing the plan.
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

// --- [MODELS] -----------------------------------------------------------------------------
// StorePlan carries the store-side evaluation artifact: one parameterized statement, its positional parameter values,
// and the in-process residue. Sql selects DISTINCT candidate GlobalIds — always a SUPERSET of the final set; the residue
// re-checks in-process, so store phase + residue == the in-process fold, bit-for-bit.
public sealed record StorePlan(string Sql, Seq<object> Parameters, Option<BimTerm> Residue);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class StoreLowering {
    // Total means the clause decides TRUE or FALSE for every row, never SQL's third value — the column that makes
    // negation soundly lowerable. A None fragment IS the residue verdict for that sub-tree.
    readonly record struct Fragment(string Where, Seq<object> Parameters, bool Total);

    public static StorePlan Lower(BimTerm term, Op key) {
        (Option<Fragment> store, Option<BimTerm> residue) = Split(term);
        return store.Match(
            Some: fragment => new StorePlan($"{EntityScan} WHERE {fragment.Where}", fragment.Parameters, residue),
            None: () => new StorePlan(EntityScan, Seq<object>(), residue));
    }

    // Split runs the sound two-phase division: All narrows with its expressible conjuncts and parks the rest as
    // residue; Any lowers only whole; Not lowers only over a lowerable AND total operand; a Closure walk and every
    // unexpressible leaf ride residue. A composite is total only where BOTH halves are — one UNKNOWN poisons it.
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
        // Negation parks a non-total operand WHOLE: a clause answering UNKNOWN drops every row whose fact column
        // is absent, and those rows are precisely the ones the negation selects.
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

    // Leaf lowers the expressible leaves over the verified fact columns; every other leaf answers None and rides the
    // residue. Entity-column comparisons are NON-total (a NULL Category, Name, or GlobalId answers UNKNOWN); every
    // parameter-fact leaf lowers through EXISTS and is total by construction.
    static Option<Fragment> Leaf(BimLeaf leaf) => leaf switch {
        BimLeaf.ByClass c => Some(new Fragment(CategoryEquals, Seq<object>(c.Class.Key), Total: false)),
        BimLeaf.ByDomain d => Some(InFragment(CategoryColumn,
            toSeq(IfcClass.Items).Filter(row => row.Domain == d.Domain).Map(static row => (object)row.Key))),
        // ONE row-keyed attribute leaf over the EntityColumns table: the two arms that differed only by which
        // column they compared collapse, and the key is the ObjectAttribute ROW rather than a "GlobalId"/"Name"
        // literal a roster rename leaves silently resolving against a column the vocabulary no longer names.
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

    // Empty value sets lower to the canonical FALSE predicate: `IN ()` fails the statement, and dropping the
    // fragment WIDENS the superset into a scan the residue never narrows. That constant IS total; a populated `IN`
    // over a nullable entity column is not.
    static Fragment InFragment(string column, Seq<object> values) =>
        values.IsEmpty
            ? new Fragment(FalsePredicate, Seq<object>(), Total: true)
            : new Fragment($"{column} IN ({string.Join(",", values.Map(static _ => "?"))})", values, Total: false);

    // Verified fact joins, every table identifier derived from its FactTable row: string-index columns resolve
    // through the single-column Strings adapter by rowid and parameters join their entity by the append-ordinal
    // rowid. Interpolated statics rather than consts — the identifier is a derived row read, never a literal.
    const string FalsePredicate = "1 = 0";
    static readonly string EntityScan = $"SELECT DISTINCT e.GlobalId FROM {FactTable.Entities.Identifier} e";
    static readonly string CategoryColumn = $"(SELECT s.Strings FROM {FactTable.Strings.Identifier} s WHERE s.rowid = e.Category)";
    static readonly string CategoryEquals = $"{CategoryColumn} = ?";
    static readonly string NameColumn = $"(SELECT s.Strings FROM {FactTable.Strings.Identifier} s WHERE s.rowid = e.Name)";

    // EntityColumns keys the store-expressible attribute rows by their ObjectAttribute row. Tag and ObjectType hold no
    // column on the flat projection, so they stay residue by ABSENCE from this table rather than by a match arm.
    static readonly Map<string, string> EntityColumns = toMap(Seq(
        (ObjectAttribute.GlobalId.Key, "e.GlobalId"),
        (ObjectAttribute.Name.Key, NameColumn)));
    static readonly string StringParameterEquals = $"EXISTS (SELECT 1 FROM {FactTable.StringParameters.Identifier} p JOIN {FactTable.Descriptors.Identifier} d ON p.Descriptor = d.rowid JOIN {FactTable.Strings.Identifier} dn ON d.Name = dn.rowid JOIN {FactTable.Strings.Identifier} sv ON p.Value = sv.rowid WHERE p.Entity = e.rowid AND dn.Strings = ? AND sv.Strings = ?)";
    static readonly string ParameterPresent = $"EXISTS (SELECT 1 FROM {FactTable.StringParameters.Identifier} p JOIN {FactTable.Descriptors.Identifier} d ON p.Descriptor = d.rowid JOIN {FactTable.Strings.Identifier} dn ON d.Name = dn.rowid WHERE p.Entity = e.rowid AND dn.Strings = ?) OR EXISTS (SELECT 1 FROM {FactTable.DoubleParameters.Identifier} q JOIN {FactTable.Descriptors.Identifier} qd ON q.Descriptor = qd.rowid JOIN {FactTable.Strings.Identifier} qn ON qd.Name = qn.rowid WHERE q.Entity = e.rowid AND qn.Strings = ?)";
}
```

## [05]-[RESEARCH]

(none)
