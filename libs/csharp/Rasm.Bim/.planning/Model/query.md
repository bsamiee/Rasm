# [BIM_ELEMENT_SET]

The set-algebraic element query is one `ElementSet.Query(ElementGraph, ElementPredicate)` fold over the seam `ElementGraph`. `ElementPredicate` owns boolean composition directly, `SetOperation` collapses union/intersection/difference behind one graph-identity-gated `Combine`, and `Where` refines the current set through the same predicate algebra. Incidence covers every neutral `Relationship` case plus the wire-name-parameterized `ByGeneric` long tail, and every related endpoint is a `NodeMatch` exact identity or nested predicate. `ValueMatch` decides the typed `PropertyValue` family, including typed members inside `Enumerated` and `List`; `Pattern` admits regex syntax before construction, `RangeBound` carries inclusive/exclusive bound identity, and measure comparisons require equal `Dimension`. The query is total; `Combine` rails cross-graph composition and `Bake` rails graph derivation.

## [01]-[INDEX]

- [01]-[ELEMENT_SET]: the `ElementPredicate` closed union over the full five-kind seam edge algebra, the `NodeMatch` incidence-target restriction, the `SpatialReach` containment-reach discriminant, the `ObjectAttribute`/`ValueMatch` value-restriction vocabulary, and the invariant-held `ElementSet` fold over the seam `ElementGraph`.

## [02]-[ELEMENT_SET]

- Owner: `ElementPredicate` is the one closed query algebra over classification, value, topology, composition, assignment, connection, void, assessment, and generic-wire incidence. `NodeMatch` carries either an exact `NodeId` or a recursive predicate. `SpatialReach`, `ObjectAttribute`, `ValueSource`, `RangeBound`, and `ValueMatch` are the policy vocabularies its arms compose. `ElementSet` owns graph-bound selection, refinement, set algebra, effective-value reads, baking, and measured aggregation; no wrapper duplicates the predicate identity.
- Entry: `ElementSet.Query(ElementGraph graph, ElementPredicate predicate)` folds the predicate over `graph.ObjectNodes`; `ElementPredicate.And`/`Or`/`AndNot` flatten the boolean closure; `ElementSet.Combine(ElementSet other, SetOperation operation, Op key)` applies one set-operation row after proving both sets share the same graph; `Where` refines the current set; `Bake(Op key)` traverses the selected objects through the seam derivation rail.
- Auto: `Match` dispatches the Thinktecture generated total `Switch` carrying the `(graph, obj)` state tuple into every arm — `ByClass`/`ByDomain`/`ByPredefinedType` read the `Node.Object`'s generic `Classification("ifc", code)` the projector stamps (resolving the `Model/elements#IFC_CLASS` `IfcClass` row for the `IfcDomain` partition), `ByClassification` the `Classification.Within` branch-containment over the primary pair and the co-applied `Classifications` set, `ByKind` the `ObjectKind` occurrence/type discriminant, `ByAttribute` the attribute rows whose key satisfies the name restriction lifted through `ObjectAttribute.Read`, `ByProperty` the effective `PropertyValue` stream (occurrence bags merged with type bags via the seam `PropertyBag.Merge`/`QuantityBag.Merge` under the stamped `InheritanceMode`, the set-name and property-name each decided by their own `ValueMatch` so a patterned `Pset_.*` facet lowers whole, over BOTH the `PropertySet` and `QuantitySet` bags), `ByMaterial` the `Associate`-bound `Material` node's composition material set, and the incidence arms the neutral edges through the O(degree) `EdgesAt` index with every related endpoint decided by `MatchesNode` — `Exact` an id equality, `Matching` a `Find<Node.Object>` resolve recursing the same `Match`, a non-`Object` target failing the nested probe structurally; `BySpatialContainer` under `SpatialReach.Ancestry` recurses the single-parent `Contain`/`Aggregate` up-chain (`ParentOf` → `InAncestry`, cycle-guarded, depth the spatial nesting depth) so a space-contained element matches its storey; the `All`/`Any`/`Not` arms recurse the same `Match` so the boolean closure is one total dispatch; a new arm breaks every `Match` site at compile time until added, so a missing query dimension is a build error, not a silent fallthrough.
- Growth: a new query dimension is one `ElementPredicate` arm folded by the same set algebra and one matching `Match` Switch arm (`ByClassificationSystem` and the `ByGeneric` wire-name arm the standing exemplars — the IDS system-only facet and the whole rostered `Generic` long tail each landed as one arm pair); a new value-restriction is one `ValueMatch` arm the value-bearing arms already fold (`Digits` the standing exemplar — xs:totalDigits/fractionDigits over the canonical rendering); a new rostered `Generic` family is ZERO query edits — `ByGeneric` parameterizes the wire-name; a new cross-page value read composes `ElementSet.ValuesOf` over the `ValueSource` axis and a cross-page set AGGREGATE composes `ElementSet.SumOf` (the zone rollup and the system demand accumulation are its two standing consumers), never a re-derived bag merge or a manual `double` fold; a new queryable object attribute is one `ObjectAttribute` row (only when the seam `Node.Object` gains the column); a new incidence flavor is a `SubKind` value the existing arm already parameterizes — `ByComposed`/`ByVoided`/`ByConnected` take the seam sub-kind vocabularies as payload, so a new `ComposeKind`/`VoidKind`/`ConnectKind` row is ZERO query edits; a new set combinator is one `SetOperation` row the derived trio and the gated `Combine` share; never a `Get<Dimension>` operation family and never a parallel selection surface.
- Boundary: `ElementPredicate` is the expression owner and `ElementSet` is minted only by `Query`, `Where`, or a graph-identity-gated `Combine`; a one-field query wrapper, public arbitrary-set constructor, arity family, or cross-graph `NodeId` merge is invalid. `ByProperty` carries independent set/name/value restrictions, every incidence target is a `NodeMatch`, and the Review and Planning consumers compose this same surface.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using NodeSet = LanguageExt.HashSet<Rasm.Element.Graph.NodeId>;   // the selection identity set — equality-keyed (NodeId carries
                                                            // no ordering comparer), aliased so the bare name never collides
                                                            // with the global-using System.Collections.Generic.HashSet.

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The containment reach: Direct the one Compose.Contain parent, Ancestry the transitive Contain/Aggregate
// up-chain — an element contained in a space matches its storey under Ancestry (the level-membership law a
// direct-parent join structurally misses), and the IDS PartOf container facet lowers transitively.
[SmartEnum<string>]
public sealed partial class SpatialReach {
    public static readonly SpatialReach Direct = new("direct");
    public static readonly SpatialReach Ancestry = new("ancestry");
}

// The queryable direct-attribute vocabulary the ByAttribute arm reads off a Node.Object, keyed by the IFC
// attribute name; the Read projection (delegate-backed enum behaviour) lifts the attribute to the typed
// PropertyValue a ValueMatch decides. Name/Tag/GlobalId span the COMPLETE seam Object direct string-column
// surface — a row lands here only when the seam node gains the column, never as a convenience alias.
[SmartEnum<string>]
public sealed partial class ObjectAttribute {
    public static readonly ObjectAttribute Name     = new("Name",     static o => o.Name is { Length: > 0 } n ? Some<PropertyValue>(new PropertyValue.Text(n)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute Tag      = new("Tag",      static o => o.Tag is { Length: > 0 } t ? Some<PropertyValue>(new PropertyValue.Text(t)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute GlobalId = new("GlobalId", static o => o.ExternalId.Map(static e => (PropertyValue)new PropertyValue.Text(e)));

    [UseDelegateFromConstructor]
    public partial Option<PropertyValue> Read(Node.Object value);
}

// The set-combinator vocabulary: each row carries its NodeSet fold as delegate data, so the derived same-graph
// trio and the graph-identity-gated Combine share ONE combination law and a new combinator is one row — never a
// fourth sibling method body.
[SmartEnum<string>]
public sealed partial class SetOperation {
    public static readonly SetOperation Union     = new("union",     static (left, right) => left.Union(right));
    public static readonly SetOperation Intersect = new("intersect", static (left, right) => left.Intersect(right));
    public static readonly SetOperation Except    = new("except",    static (left, right) => left.Except(right));

    [UseDelegateFromConstructor]
    public partial NodeSet Apply(NodeSet left, NodeSet right);
}

// The typed value-restriction the value-bearing arms (ByProperty/ByAttribute/ByMaterial) decide a candidate
// PropertyValue through — the Review/validation#IDS_FACETS IDS ValueConstraint family (exact/pattern/range/
// one-of/length) lowered onto the seam typed value, never a String.Equals. Present is the structural-existence
// match (the candidate exists at all); the arms invoke Matches only on a resolved candidate, so Present + a
// resolved candidate IS "the property/attribute/material is present".
[Union]
public abstract partial record ValueMatch {
    private ValueMatch() { }

    public sealed record Present : ValueMatch;
    public sealed record Exact(PropertyValue Value) : ValueMatch;

    // The XSD pattern facet is ADMITTED, never constructed raw: the private ctor makes a malformed or
    // NonBacktracking-unsupported foreign regex unrepresentable as a ValueMatch value — `Of` is the Fin-railed
    // ingress a raw wire/UI pattern crosses (faulting `pattern-reject` typed), `Lift` the total Option probe a
    // gated boundary (the IDS Unliftable gate) folds — so Decide never meets an uncompilable expression and a
    // malformed pattern is distinguishable from a candidate mismatch on the owning rail, never a silent non-match.
    public sealed record Pattern : ValueMatch {
        private Pattern(string expression) => Expression = expression;
        public string Expression { get; }

        public static Fin<ValueMatch> Of(string expression, Op key) =>
            Lift(expression).ToFin(new BimFault.ModelRejected(key, $"pattern-reject:{expression}"));

        public static Option<ValueMatch> Lift(string expression) =>
            Compiled(expression).Map(_ => (ValueMatch)new Pattern(expression));
    }

    public sealed record Range(Option<RangeBound> Lower, Option<RangeBound> Upper) : ValueMatch;
    public sealed record OneOf(Seq<string> Allowed) : ValueMatch;
    public sealed record Length(Option<int> Min, Option<int> Max) : ValueMatch;
    public sealed record Digits(Option<int> Total, Option<int> Fraction) : ValueMatch;   // xs:totalDigits/fractionDigits over the canonical numeric rendering

    public static readonly ValueMatch Any = new Present();

    // The IDS real-value comparison convention: numeric enumeration/equality decides at this relative
    // tolerance in the SI value space, never an exact bit compare and never a rendered-string compare.
    private const double RealTolerance = 1e-6;

    // A multi-valued candidate (IfcPropertyEnumeratedValue / IfcPropertyListValue) satisfies when ANY selected
    // member satisfies — spread BEFORE the restriction decides, so a Pattern never false-matches across the
    // joined-list Render and an Exact reaches the member, the IDS any-of law over multi-valued properties.
    public bool Matches(PropertyValue value) => Spread(value).Exists(Decide);

    private static Seq<PropertyValue> Spread(PropertyValue value) => value switch {
        PropertyValue.Enumerated e => e.Selected.Bind(Spread),
        PropertyValue.List l       => l.Values.Bind(Spread),
        _                          => Seq(value),
    };

    private bool Decide(PropertyValue value) => Switch(
        state:    value,
        present:  static (_, _) => true,
        exact:    static (v, m) => v.Equals(m.Value),
        pattern:  static (v, m) => Compiled(m.Expression).Exists(regex => regex.IsMatch(v.Render())),
        range:    static (v, m) => v is PropertyValue.Measure measure && InRange(measure.Value, m),
        oneOf:    static (v, m) => InSet(v, m),
        length:   static (v, m) => InLength(v.Render().Length, m),
        digits:   static (v, m) => InDigits(v, m));

    // ONE compile site behind the admission: ANCHORED whole-value (\A(?:…)\z — an XSD pattern facet is a
    // whole-value match, never a substring) and NonBacktracking (linear-time, so a catastrophic foreign pattern can
    // never ReDoS-hang the fold), cached per expression so the total Matches never recompiles per candidate. The
    // null arm is reachable ONLY from Of/Lift — every minted Pattern re-resolves its cached compiled instance.
    private static readonly ConcurrentDictionary<string, Option<Regex>> CompiledPatterns = new();
    private static Option<Regex> Compiled(string pattern) =>
        CompiledPatterns.GetOrAdd(pattern, static expression =>
            Try.lift(() => new Regex($@"\A(?:{expression})\z", RegexOptions.NonBacktracking | RegexOptions.CultureInvariant))
                .Run()
                .ToOption());

    // The numeric bound test reads the SI magnitude the seam MeasureValue carries AND requires the bound to share the
    // candidate's Dimension (a length never satisfies a pressure bound — the same dimension law MeasureValue.Sum rails
    // on), so a cross-dimension bound never produces a meaningless raw-scalar match; an open bound (None) is unbounded
    // on that side and the per-side LowerInclusive/UpperInclusive flag selects >=/> and <=/< so the IDS RangeConstraint
    // min/maxExclusive bound lowers onto the same arm, a non-measure candidate never satisfying (range gates on Measure first).
    private static bool InRange(MeasureValue value, Range match) =>
        match.Lower.ForAll(bound => bound.AllowsLower(value))
        && match.Upper.ForAll(bound => bound.AllowsUpper(value));

    // XSD enumeration equality is VALUE-SPACE equality: a Measure candidate parses each allowed literal invariant
    // and compares SI magnitudes at the relative tolerance; every other candidate compares its Render ordinal,
    // case-SENSITIVE — the xbim IsSatisfiedBy(ignoreCase: false) default, never an OrdinalIgnoreCase fold that
    // admits a token the schema rejects.
    private static bool InSet(PropertyValue v, OneOf m) => v is PropertyValue.Measure { Value: var mv }
        ? m.Allowed.Exists(a => double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                                && Math.Abs(d - mv.Si) <= RealTolerance * Math.Max(Math.Abs(d), Math.Abs(mv.Si)))
        : m.Allowed.Exists(a => string.Equals(a, v.Render(), StringComparison.Ordinal));

    // The IDS StructureConstraint length facet (length/minLength/maxLength) over the rendered value's character count,
    // each open bound (None) unbounded on that side — a string-length restriction lowers onto one arm without a measure.
    private static bool InLength(int len, Length m) =>
        m.Min.Match(Some: lo => len >= lo, None: static () => true) && m.Max.Match(Some: hi => len <= hi, None: static () => true);

    // The XSD totalDigits/fractionDigits facet decided over the candidate's CANONICAL numeric rendering — the "R"
    // invariant of the SI magnitude, sign excluded — total counting the SIGNIFICANT decimal digits (the XSD value
    // space excludes the leading integer zero of a sub-unity rendering: 0.123 carries three digits, not four),
    // fraction those after the point; a non-numeric candidate or a scientific rendering (magnitude exceeding any
    // digits facet) never satisfies, so the facet decides value-space presentation without a locale-forked string.
    private static bool InDigits(PropertyValue v, Digits m) {
        if (v is not PropertyValue.Measure measure) { return false; }
        string text = Math.Abs(measure.Value.Si).ToString("R", CultureInfo.InvariantCulture);
        if (text.Contains('E') || text.Contains('e')) { return false; }
        int point = text.IndexOf('.');
        int fraction = point < 0 ? 0 : text.Length - point - 1;
        int total = text.Count(char.IsAsciiDigit) - (text.StartsWith("0.", StringComparison.Ordinal) ? 1 : 0);
        return m.Total.Match(Some: t => total <= t, None: static () => true)
            && m.Fraction.Match(Some: f => fraction <= f, None: static () => true);
    }
}

[Union]
public abstract partial record RangeBound {
    private RangeBound() { }

    public sealed record Inclusive(MeasureValue Value) : RangeBound;
    public sealed record Exclusive(MeasureValue Value) : RangeBound;

    public bool AllowsLower(MeasureValue candidate) => Switch(
        state: candidate,
        inclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si >= bound.Value.Si,
        exclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si > bound.Value.Si);

    public bool AllowsUpper(MeasureValue candidate) => Switch(
        state: candidate,
        inclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si <= bound.Value.Si,
        exclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si < bound.Value.Si);

    private static bool SameDimension(MeasureValue candidate, MeasureValue bound) =>
        candidate.Dimension == bound.Dimension;
}

// --- [MODELS] -----------------------------------------------------------------------------
// The incidence-target restriction every edge arm carries: Exact the NodeId join, Matching the nested
// ElementPredicate recursed on the related node — the graph-pattern closure ("hosted in a fire-rated wall",
// "contained in a storey named X") as case-owned recursion inside ONE algebra, never a second query pass whose
// materialized ids fold back as an Any of exact joins. The ad-hoc implicit conversions absorb both spellings,
// so an id-joining consumer call site never names the union.
[Union<NodeId, ElementPredicate>(T1Name = "Exact", T2Name = "Matching")]
public readonly partial struct NodeMatch;

// The value-source axis a cross-page consumer names an element value by — the direct ObjectAttribute row or the
// effective (type→occurrence-merged) Pset/Qto property. Review/coordination#COORDINATION Unique carries it; the
// effective-value merge stays THIS page's one owner, exposed through ElementSet.ValuesOf, never re-derived
// at a consumer (the named seam-bag-merge drift).
[Union]
public abstract partial record ValueSource {
    private ValueSource() { }

    public sealed record Attribute(ObjectAttribute Key) : ValueSource;
    public sealed record Property(string Set, string Name) : ValueSource;
}

// The closed predicate algebra: one arm per query dimension over the seam graph, the All/Any/Not boolean
// closure recursing the same Match. The incidence arms span the seam's FIVE neutral edge kinds by the
// projector's directionality, sub-kinds ride as payload — never a typed IfcRel* case, never a per-flavor arm.
[Union]
public abstract partial record ElementPredicate {
    private ElementPredicate() { }

    public sealed record ByClass(IfcClass Class) : ElementPredicate;                               // exact IFC entity class (Classification "ifc", Class.Key)
    public sealed record ByDomain(IfcDomain Domain) : ElementPredicate;                            // the IfcClass.Domain discipline partition
    public sealed record ByPredefinedType(IfcClass Class, PredefinedType Type) : ElementPredicate; // entity class + the typed predefined token
    public sealed record ByClassification(Classification Branch) : ElementPredicate;              // standard-system Classification.Within branch containment
    public sealed record ByClassificationSystem(string System) : ElementPredicate;                // system-only membership — classified in the system at ANY code (the IDS no-identification facet)
    public sealed record ByKind(ObjectKind Kind) : ElementPredicate;                              // occurrence vs type node
    public sealed record ByAttribute(ValueMatch Attribute, ValueMatch Restriction) : ElementPredicate; // attribute rows whose key satisfies Attribute, value deciding Restriction
    public sealed record ByProperty(ValueMatch Set, ValueMatch Name, ValueMatch Restriction) : ElementPredicate; // effective Pset/Qto value; set+name each restrictable (IDS IfcPropertyFacet)
    public sealed record ByMaterial(ValueMatch Restriction) : ElementPredicate;                   // an Associate-bound material's composition material key
    public sealed record BySpatialContainer(NodeMatch Container, SpatialReach Reach) : ElementPredicate; // the Compose{Contain} parent, Direct or the Contain/Aggregate Ancestry chain
    public sealed record ByComposed(ComposeKind SubKind, NodeMatch Whole) : ElementPredicate;     // Aggregate/Nest/Reference part-of membership; Contain routes BySpatialContainer
    public sealed record ByType(NodeMatch Type) : ElementPredicate;                               // the Assign{TypeDefinition} bound type object
    public sealed record ByZone(NodeMatch Group) : ElementPredicate;                              // an Assign{Group} logical OR Compose{Reference} spatial zone membership — the zones MembersOf modality pair
    public sealed record ByConnected(NodeMatch Other, Option<ConnectKind> Kind) : ElementPredicate; // a Connect adjacency over Members, optionally a flavor
    public sealed record ByVoided(VoidKind SubKind, NodeMatch Other) : ElementPredicate;          // a Void{Void|Fill} incidence — the host of an opening, the filler of one
    public sealed record ByAssessment(Discipline Discipline, Option<AssessmentOutcome> Outcome) : ElementPredicate; // an Assign{Assessment} receipt of a discipline, optionally an outcome
    public sealed record ByGeneric(string WireName, NodeMatch Other) : ElementPredicate;          // a Generic-edge incidence by IFC wire-name — the rostered long tail (covers/served/sequence/process/control/space-boundary) queryable through ONE parameterized arm
    public sealed record All(Seq<ElementPredicate> Operands) : ElementPredicate;
    public sealed record Any(Seq<ElementPredicate> Operands) : ElementPredicate;
    public sealed record Not(ElementPredicate Operand) : ElementPredicate;

    public ElementPredicate And(ElementPredicate other) => this is All all
        ? new All(all.Operands.Add(other))
        : new All(Seq(this, other));

    public ElementPredicate Or(ElementPredicate other) => this is Any any
        ? new Any(any.Operands.Add(other))
        : new Any(Seq(this, other));

    public ElementPredicate AndNot(ElementPredicate other) => And(new Not(other));
}

public sealed record ElementSet {
    // The IFC entity-class system token the Projection/semantic#SEMANTIC_PROJECTOR Objects fold stamps onto every
    // Node.Object as Classification("ifc", IfcClass.Key); the seam Classification.Create lower-cases the system, so
    // ByClass/ByDomain/ByPredefinedType match the lower-case token, the IfcClass roster staying the projector's.
    private const string IfcSystem = "ifc";

    private ElementSet(ElementGraph graph, NodeSet ids) => (Graph, Ids) = (graph, ids);

    public ElementGraph Graph { get; }
    public NodeSet Ids { get; }

    public static ElementSet Query(ElementGraph graph, ElementPredicate predicate) =>
        new(graph, new NodeSet(graph.ObjectNodes.Filter(o => Match(graph, o, predicate)).Map(static o => o.Id)));

    public int Count => Ids.Count;
    public Seq<Node.Object> Objects => Graph.ObjectNodes.Filter(o => Ids.Contains(o.Id));
    public Seq<string> GlobalIds => Objects.Choose(static o => o.ExternalId);

    // The one Fin-railed step: bake every selected object into the seam Bake-derived Element ("has it all"),
    // railing ElementFault on a cyclic Compose or an absent root the selection never reaches in a healthy graph.
    public Fin<Seq<Element>> Bake(Op key) =>
        Objects.TraverseM(o => Graph.Bake(o.Id, key)).As();

    // The same-graph set algebra: the named members are one-hop conveniences over the SetOperation rows for the
    // refinement partitions a policy delegate composes (both operands minted from THIS set's graph — the IDS
    // cardinality and coordination Require/Prohibit partition rows); two independently-held sets meet through the
    // graph-identity-gated Combine, never here.
    public ElementSet Union(ElementSet other) => new(Graph, SetOperation.Union.Apply(Ids, other.Ids));
    public ElementSet Intersect(ElementSet other) => new(Graph, SetOperation.Intersect.Apply(Ids, other.Ids));
    public ElementSet Except(ElementSet other) => new(Graph, SetOperation.Except.Apply(Ids, other.Ids));

    // The one cross-set meet: two independently-minted sets (a federation join, a saved selection replayed against
    // a reloaded snapshot) prove they share ONE graph before any id algebra — a cross-graph merge would mint ids
    // the graph never declares, a silently-corrupt selection no downstream Objects read could distinguish from an
    // honest empty refinement.
    public Fin<ElementSet> Combine(ElementSet other, SetOperation operation, Op key) =>
        ReferenceEquals(Graph, other.Graph)
            ? Fin.Succ(new ElementSet(Graph, operation.Apply(Ids, other.Ids)))
            : Fin.Fail<ElementSet>(new BimFault.ModelRejected(key, $"set-cross-graph:{operation.Key}"));

    // One Where, one modality: refine the current set by another ElementPredicate — re-folds ONLY the current
    // members, not the whole graph — so the closed predicate algebra stays the single selection surface; a raw
    // Func<Node.Object, bool> escape hatch is the deleted form (a refinement the algebra cannot express is one new
    // ElementPredicate arm, never an untyped second selection surface beside the IDS/coordination predicate reuse).
    public ElementSet Where(ElementPredicate predicate) =>
        new(Graph, new NodeSet(Objects.Filter(o => Match(Graph, o, predicate)).Map(static o => o.Id)));

    // The total predicate fold: the Thinktecture generated Switch carries the (graph, obj) state into every arm,
    // so a missing arm is a build error at every Match site. The classification arms read the Object node's primary
    // Classification (the entity-class pair) — byClassification ALSO searching the co-applied Classifications set;
    // the incidence arms read the neutral edges through the O(degree) EdgesAt index, every related endpoint decided
    // by MatchesNode so an exact join and a nested predicate share one arm.
    private static bool Match(ElementGraph graph, Node.Object obj, ElementPredicate predicate) => predicate.Switch(
        state: (graph, obj),
        byClass:            static (s, p) => s.obj.Classification.System == IfcSystem
                                             && string.Equals(s.obj.Classification.Code, p.Class.Key, StringComparison.OrdinalIgnoreCase),
        byDomain:           static (s, p) => s.obj.Classification.System == IfcSystem
                                             && IfcClass.TryGet(s.obj.Classification.Code).Exists(c => c.Domain == p.Domain),
        byPredefinedType:   static (s, p) => s.obj.Classification.System == IfcSystem
                                             && string.Equals(s.obj.Classification.Code, p.Class.Key, StringComparison.OrdinalIgnoreCase)
                                             && s.obj.PredefinedType == p.Type,
        byClassification:   static (s, p) => s.obj.Classification.Within(p.Branch) || s.obj.Classifications.Exists(c => c.Within(p.Branch)),
        byClassificationSystem: static (s, p) => string.Equals(s.obj.Classification.System, p.System, StringComparison.OrdinalIgnoreCase)
                                             || s.obj.Classifications.Exists(c => string.Equals(c.System, p.System, StringComparison.OrdinalIgnoreCase)),
        byKind:             static (s, p) => s.obj.Kind == p.Kind,
        byAttribute:        static (s, p) => toSeq(ObjectAttribute.Items).Exists(row =>
                                                 p.Attribute.Matches(new PropertyValue.Text(row.Key))
                                                 && row.Read(s.obj).Exists(v => p.Restriction.Matches(v))),
        byProperty:         static (s, p) => EffectiveValues(s.graph, s.obj.Id, p.Set, p.Name).Exists(v => p.Restriction.Matches(v)),
        byMaterial:         static (s, p) => s.graph.MaterialsOf(s.obj.Id)
                                                 .Exists(m => m.Composition.Materials.Exists(id => p.Restriction.Matches(new PropertyValue.Text(id.Value)))),
        bySpatialContainer: static (s, p) => p.Reach == SpatialReach.Direct
                                                 ? toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Compose c && c.IsContainment && c.Part == s.obj.Id
                                                       && MatchesNode(s.graph, p.Container, c.Whole))
                                                 : InAncestry(s.graph, p.Container, s.obj.Id, new NodeSet()),
        byComposed:         static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Compose c && c.SubKind == p.SubKind && c.Part == s.obj.Id
                                                 && MatchesNode(s.graph, p.Whole, c.Whole)),
        byType:             static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition
                                                 && a.Subject == s.obj.Id && MatchesNode(s.graph, p.Type, a.Definition)),
        byZone:             static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e =>
                                                 (e is Relationship.Assign { SubKind: var k } a && k == AssignKind.Group
                                                     && a.Subject == s.obj.Id && MatchesNode(s.graph, p.Group, a.Definition))
                                                 || (e is Relationship.Compose { SubKind: var ck } c && ck == ComposeKind.Reference
                                                     && c.Part == s.obj.Id && MatchesNode(s.graph, p.Group, c.Whole))),
        byConnected:        static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Connect c && c.Touches(s.obj.Id)
                                                 && p.Kind.Match(Some: k => c.SubKind == k, None: static () => true)
                                                 && c.Members.Exists(m => m != s.obj.Id && MatchesNode(s.graph, p.Other, m))),
        byVoided:           static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Void v && v.SubKind == p.SubKind
                                                 && (v.Host == s.obj.Id ? MatchesNode(s.graph, p.Other, v.Feature)
                                                                        : v.Feature == s.obj.Id && MatchesNode(s.graph, p.Other, v.Host))),
        byAssessment:       static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.Assessment && a.Subject == s.obj.Id
                                                 && s.graph.Nodes.TryGetValue(a.Definition, out Node? n) && n is Node.Assessment asm
                                                 && asm.Payload.Discipline == p.Discipline && p.Outcome.Match(Some: o => asm.Payload.Outcome == o, None: static () => true)),
        byGeneric:          static (s, p) => toSeq(s.graph.EdgesAt(s.obj.Id)).Exists(e => e is Relationship.Generic g && string.Equals(g.WireName, p.WireName, StringComparison.Ordinal)
                                                 && (g.Relating == s.obj.Id ? MatchesNode(s.graph, p.Other, g.Related)
                                                                            : g.Related == s.obj.Id && MatchesNode(s.graph, p.Other, g.Relating))),
        all:                static (s, p) => p.Operands.ForAll(op => Match(s.graph, s.obj, op)),
        any:                static (s, p) => p.Operands.Exists(op => Match(s.graph, s.obj, op)),
        not:                static (s, p) => !Match(s.graph, s.obj, p.Operand));

    // The incidence-target decision every edge arm shares: an Exact id equality, or a Matching nested predicate
    // resolved on the related Object node and recursed through the SAME Match — a non-Object target fails the
    // nested probe structurally, so Matching never lies about a bag/material/assessment node.
    private static bool MatchesNode(ElementGraph graph, NodeMatch target, NodeId candidate) => target.Switch(
        exact:    id   => id == candidate,
        matching: pred => graph.Find<Node.Object>(candidate).Exists(o => Match(graph, o, pred)));

    // The single-parent spatial up-chain, read per candidate off the seam incidence — the SAME chain the
    // Model/spatial Ancestors axis walks over its prebuilt tree. An element crosses its Aggregate host to reach
    // the spatial ancestors (a curtain-wall panel is on the storey through its wall), the Contain parent winning
    // when a malformed graph carries both. Cycle-guarded so a malformed graph terminates, not recurses.
    private static bool InAncestry(ElementGraph graph, NodeMatch target, NodeId node, NodeSet seen) =>
        ParentOf(graph, node).Exists(parent =>
            !seen.Contains(parent)
            && (MatchesNode(graph, target, parent) || InAncestry(graph, target, parent, seen.Add(parent))));

    private static Option<NodeId> ParentOf(ElementGraph graph, NodeId node) =>
        WholeOf(graph, node, ComposeKind.Contain) | WholeOf(graph, node, ComposeKind.Aggregate);

    private static Option<NodeId> WholeOf(ElementGraph graph, NodeId node, ComposeKind kind) =>
        toSeq(graph.EdgesAt(node)).Choose(e =>
            e is Relationship.Compose c && c.SubKind == kind && c.Part == node ? Some(c.Whole) : Option<NodeId>.None).Head;

    // The PUBLIC effective-value read over one node — the ONE exposure of the private EffectiveValues fold a
    // cross-page consumer (Review/coordination#COORDINATION Unique) composes instead of re-deriving the seam
    // bag merge: the direct attribute row, or the type→occurrence-merged effective property values.
    public static Seq<PropertyValue> ValuesOf(ElementGraph graph, Node.Object obj, ValueSource source) => source.Switch(
        attribute: a => a.Key.Read(obj).ToSeq(),
        property:  p => EffectiveValues(graph, obj.Id,
                            new ValueMatch.Exact(new PropertyValue.Text(p.Set)),
                            new ValueMatch.Exact(new PropertyValue.Text(p.Name))));

    // The railed SET-aggregate read over the one ValuesOf exposure — the zone rollup (Model/zones ZoneProjection.
    // Aggregate) and the system demand accumulation (Model/systems SystemTrace.Demand) COMPOSE this one fold: the
    // ids' effective values for one source, admitted as measures before the seam same-type MeasureValue.Sum.
    // None means no value exists; a present non-measure is typed failure, never silently discarded into a partial sum.
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

    // The effective property stream: candidate set names from BOTH bag kinds on occurrence AND type, filtered by
    // the Set restriction; each surviving set resolves through the seam PropertyBag/QuantityBag.Merge under the
    // stamped InheritanceMode (never a re-implemented precedence); entries whose PropertyName satisfies the Name
    // restriction yield their typed values, a Qto_* quantity wrapped as PropertyValue.Measure — so an exact,
    // enumerated, or patterned (SetName, Name) facet reads one typed stream without a heavy full Bake.
    private static Seq<PropertyValue> EffectiveValues(ElementGraph graph, NodeId obj, ValueMatch set, ValueMatch name) {
        (Seq<PropertyBag> occProps, Seq<QuantityBag> occQty) = BagsOf(graph, obj);
        (Seq<PropertyBag> typProps, Seq<QuantityBag> typQty) = TypeIdOf(graph, obj).Match(Some: t => BagsOf(graph, t), None: static () => (Seq<PropertyBag>(), Seq<QuantityBag>()));
        Seq<string> names = (occProps.Map(static b => b.SetName) + typProps.Map(static b => b.SetName)
                           + occQty.Map(static b => b.SetName) + typQty.Map(static b => b.SetName))
            .Distinct().Filter(n => set.Matches(new PropertyValue.Text(n)));
        return names.Bind(n =>
            Resolve(occProps, typProps, n).ToSeq().Bind(bag => Named(bag.Values, name, static v => v))
          + Resolve(occQty, typQty, n).ToSeq().Bind(bag => Named(bag.Values, name, static m => (PropertyValue)new PropertyValue.Measure(m))));
    }

    private static Seq<PropertyValue> Named<V>(Map<PropertyName, V> values, ValueMatch name, Func<V, PropertyValue> lift) =>
        toSeq(values).Choose(pair => name.Matches(new PropertyValue.Text(pair.Key.Value)) ? Some(lift(pair.Value)) : Option<PropertyValue>.None);

    // ONE edge walk gathers BOTH bag kinds a node's own Assign{PropertyDefinition} edges attach — the (Props, Qty)
    // shape the seam Bake.TypeBagsOf reads — so the property and quantity resolution share one walk; the caller reaches
    // the type object's bags by walking from the TypeIdOf id, the SAME Assign{TypeDefinition}/{PropertyDefinition} edge
    // conventions the seam Bake reads, never four near-identical per-bag-kind walks.
    private static (Seq<PropertyBag> Props, Seq<QuantityBag> Qty) BagsOf(ElementGraph graph, NodeId id) =>
        toSeq(graph.EdgesAt(id)).Fold(
            (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>()),
            (acc, e) => e is Relationship.Assign { SubKind: var k, Subject: var subj, Definition: var def } && k == AssignKind.PropertyDefinition && subj == id && graph.Nodes.TryGetValue(def, out Node? n)
                ? n switch {
                    Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) },
                    Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) },
                    _                   => acc,
                }
                : acc);

    private static Option<NodeId> TypeIdOf(ElementGraph graph, NodeId obj) =>
        toSeq(graph.EdgesAt(obj)).Choose(e =>
            e is Relationship.Assign { SubKind: var k, Subject: var subj, Definition: var def } && k == AssignKind.TypeDefinition && subj == obj
                ? Some(def) : Option<NodeId>.None).Head;

    // One named bag resolution for BOTH aliases (PropertyBag/QuantityBag are ValueBag<V> global-using aliases): the
    // occurrence bag matching SetName merges with its type counterpart via the ONE seam ValueBag<V>.Merge (the
    // occurrence carrying the stamped InheritanceMode), a type-only bag inheriting as-is — never a per-alias overload pair.
    private static Option<ValueBag<V>> Resolve<V>(Seq<ValueBag<V>> occurrence, Seq<ValueBag<V>> type, string setName) =>
        occurrence.Find(b => b.SetName == setName).Match(
            Some: occ => Some(type.Find(b => b.SetName == setName).Match(Some: typ => ValueBag<V>.Merge(typ, occ), None: () => occ)),
            None: () => type.Find(b => b.SetName == setName));
}

```

## [03]-[RESEARCH]

- [PREDICATE_ALGEBRA]: the `ElementPredicate` union and the `Match` fold ground against the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` (`ObjectNodes`/`EdgesAt`/`Nodes`/`Find<T>`/`MaterialsOf`/`Bake`; `EdgesAt` yields the frozen `ImmutableArray<Relationship>` incidence row, folded through `toSeq` exactly as the seam's own operations spell it) and the neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` five-kind algebra, covered WHOLE: `BySpatialContainer` matches the `Relationship.Compose` `IsContainment` (`SubKind == ComposeKind.Contain`) whole→part edge (the `Projection/relations#RELATION_ALGEBRA` `IfcRelContainedInSpatialStructure` relating-structure→related-element fold), its `SpatialReach.Ancestry` recursing the cycle-guarded single-parent `ParentOf` chain (`Contain` first, `Aggregate` second — the IFC storey→space→element nesting a direct-parent join misses); `ByComposed` the `Aggregate`/`Nest`/`Reference` part-of membership over the typed seam flavors (`IfcRelAggregates`/`IfcRelNests`/`IfcRelReferencedInSpatialStructure` per the `Projection/relations#RELATION_ALGEBRA` roster — the projector owns how a nest is lowered; the arm parameterizes the seam `ComposeKind`, never the wire name) with `Contain` routed to the dedicated containment arm; `ByType` an `Assign { SubKind: TypeDefinition }` whose `Subject` is the occurrence and `Definition` the type (the `IfcRelDefinesByType` fold the seam `Bake` `TypeBagsOf` also reads); `ByZone` an `Assign { SubKind: Group }` whose `Subject` is the element and `Definition` the group (`IfcRelAssignsToGroup`, the projector's member→group direction the group-centric `Model/zones#ZONE_GRAPH` `MembersOf` reads inverted — group-in-group nesting rides the same `Subject`-side read, a circuit's parent system being the circuit-as-`Subject` edge) OR a `Compose { SubKind: Reference }` whose `Part` is the element and `Whole` the zone (`IfcRelReferencedInSpatialStructure` — the `BimZoneKind.SpatialZone` `IsSpatial` modality `MembersOf` dispatches), so the arm decides membership identically to the zone view's two-modality read and an `IfcSpatialZone` fire/thermal member never falls out of an element-centric query; `ByConnected` a `Connect` adjacency over `Members` so a `Realizing` intermediary stays reachable (`IfcRelConnectsElements`/`ConnectsPorts`/`ConnectsWithRealizingElements`); `ByVoided` the `Void { Void | Fill }` host↔feature incidence (`IfcRelVoidsElement`/`IfcRelFillsElement` — the fifth edge kind the prior algebra left unqueryable); `ByAssessment` an `Assign { SubKind: Assessment }` whose `Definition` resolves a `Node.Assessment` of the requested `Discipline` (and optional `AssessmentOutcome`), so a `Rasm.Compute` analysis receipt the route writes back is itself a selection dimension; `ByGeneric` the wire-name-parameterized `Relationship.Generic` incidence read at EITHER endpoint (the rostered long tail — `IfcRelCoversBldgElements`/`IfcRelServicesBuildings`/`IfcRelSequence`/`IfcRelAssignsToProcess`/`IfcRelAssignsToControl`/`IfcRelSpaceBoundary` per the `Projection/relations#RELATION_ALGEBRA` roster — so the whole landed edge population is selectable and a 4D process join is one arm, never a structurally-unqueryable stratum); every incidence target is the `NodeMatch` ad-hoc union — `Exact` the id join, `Matching` the nested predicate recursed through `Find<Node.Object>` — the case-owned recursion that makes a related-node condition ONE predicate; `ByClass`/`ByDomain`/`ByPredefinedType` read the `Node.Object` generic `Classification("ifc", code)` the projector stamps (never an `IfcClass` field on the node), resolving `Model/elements#IFC_CLASS` `IfcClass.TryGet`/`Domain` for the discipline partition; the Thinktecture generated total `Switch` carrying the `(graph, obj)` state breaks every `Match` site until a new arm is added, so a missing dimension is a build error.
- [VALUE_RESTRICTION]: `ValueMatch` lowers IDS exact, pattern, range, enumeration, length, and presence restrictions onto typed `PropertyValue` candidates. `Pattern.Of` owns regex admission; `RangeBound` owns inclusive/exclusive endpoints; multi-valued candidates spread before matching; `ByProperty` applies set, property-name, and value restrictions after inheritance merge; `ByAttribute` applies the same value algebra to the closed object-attribute vocabulary.
- [ALGEBRA_REUSE]: the `ElementPredicate` algebra IS the one selection surface its consumers reuse — `Review/validation#IDS_FACETS` lowers each IDS facet to an `ElementPredicate` arm (`Entity`→`ByClass`/`ByPredefinedType`, `Property`→`ByProperty`, `Classification`→`ByClassification`, `Material`→`ByMaterial`, `Attribute`→`ByAttribute`, `PartOf`→the incidence arms: `Contained`→`BySpatialContainer`, `Grouped`→`ByZone`, `Aggregated`/`Nested`→`ByComposed`, `Voided`→`ByVoided`, the container-entity facet lowering to `NodeMatch.Matching` directly — so no PartOf relation drops at parse and no materialize-then-`Any` join survives), `Review/coordination#COORDINATION` carries an `ElementPredicate` applicability/requirement pair on every rule arm and folds them through `ElementSet.Query`/`Where`, the `Model/zones#ZONE_GRAPH` group-centric `BimZone` view is the dual of the element-centric `ByZone` arm, and the `Model/spatial#SPATIAL_STRUCTURE` tree indexes the same `Compose.Contain` relation `BySpatialContainer` joins (the `Ancestry` reach reading the identical `Contain`/`Aggregate` up-chain off the seam incidence per candidate — the query never constructs the spatial view); a rejection on those consumers' own rails lowers onto `Model/faults#FAULT_BAND` `BimFault`, while `ElementSet.Query` itself is total and the railed steps are `Bake` (the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` derivation), `SumOf` (the non-measure aggregate admission), and `Combine` (the `set-cross-graph` graph-identity gate).
