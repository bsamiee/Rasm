# [BIM_ELEMENT_SET]

The set-algebraic element query: one polymorphic `ElementSet.Query` over a closed `ElementPredicate` union, folded by the `Union`/`Intersect`/`Except`/`Where` set algebra into one expression over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` — never a `GetWalls`/`GetByLevel`/`GetByMaterial` operation family. The query reads the canonical graph the `Projection/semantic#SEMANTIC_PROJECTOR` projector assembles: it folds `ElementGraph.ObjectNodes`, matching each `Node.Object`'s generic `Classification`/`PredefinedType` and its incident neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` edges through the built-once incidence index (`EdgesAt`, O(degree) per element, never an O(edges) rescan), never the retired `BimModel`/`BimElement` collection. A predicate value is matched by the typed `ValueMatch` restriction (`Present`/`Exact`/`Pattern`/`Range`/`OneOf`/`Length` — the full IDS `ValueConstraint` family: a whole-value anchored XSD `Pattern`, an inclusive-or-exclusive numeric `Range`, and the `Length` structure facet) over the seam `Rasm.Element/Properties/property#PROPERTY_VALUE` typed value family, never a stringly equality; the type→occurrence property precedence reuses the seam `PropertyBag.Merge` under the stamped `InheritanceMode`, never a re-implemented merge. The same `ElementPredicate` algebra IS the selection surface the `Review/validation#IDS_FACETS` IDS facet fold, the `Review/coordination#COORDINATION` coordination rules, and the `Planning/schedule#SCHEDULE` task-assignment selection reuse, the `Model/systems#CONNECTIVITY` `ByDomain` distribution-domain selection, the `Model/spatial#SPATIAL_STRUCTURE` `BySpatialContainer` containment selection, and the `Model/zones#ZONE_GRAPH` `ByZone` grouping selection all read — one selection surface, never a second. The query is total and carries no fault rail; `ElementSet.Bake` is the single `Fin`-railed step, lowering a selected set to the `Bake`-derived `Element` family the consumer reads "has it all" from.

## [01]-[INDEX]

- [01]-[ELEMENT_SET]: the `ElementPredicate` closed union, the `ObjectAttribute`/`ValueMatch` value-restriction vocabulary, the `ElementQuery` composed-expression record, and the `ElementSet` set-algebraic fold over the seam `ElementGraph`.

## [02]-[ELEMENT_SET]

- Owner: `ElementSet` the set-algebraic query fold over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` object-node set, owning union/intersection/difference/refinement as a closed algebra over `ElementPredicate` and carrying the source graph so the selection bakes and resolves `ExternalId`s without a second store; `ElementPredicate` the `[Union]` closed predicate family carrying each discriminant payload — the IFC classification axes (`ByClass`/`ByDomain`/`ByPredefinedType`/`ByClassification`), the node-attribute and value arms (`ByKind`/`ByAttribute`/`ByProperty`/`ByMaterial`), the neutral-edge incidence arms (`BySpatialContainer`/`ByType`/`ByZone`/`ByConnected`/`ByAssessment`), and the `All`/`Any`/`Not` boolean closure; `ElementQuery` the composed query record folding predicates into one set-algebraic expression; `ObjectAttribute` the `[SmartEnum<string>]` queryable-attribute vocabulary (`Name`/`Tag`/`GlobalId`) the `ByAttribute` arm reads off the `Node.Object`; `ValueMatch` the `[Union]` typed value-restriction (`Present`/`Exact`/`Pattern`/`Range`/`OneOf`/`Length`) the value-bearing arms decide a `PropertyValue` through — the whole IDS `ValueConstraint` restriction family, the `Pattern` arm a whole-value-anchored NonBacktracking XSD regex, the `Range` arm carrying per-side inclusivity so `min/maxExclusive` lowers onto it, and `Length` the structure facet.
- Entry: `ElementSet.Query(ElementGraph graph, ElementQuery query)` folds the composed predicate over `graph.ObjectNodes` into the matching `NodeId` set — total, pure, no rail; the set-algebra combinators `Union`/`Intersect`/`Except`/`Where` compose `ElementSet` values over the SAME graph so a complex selection is one expression, never an imperative accumulation loop; `ElementQuery.And`/`Or`/`AndNot` compose the boolean expression, flattening into the `All`/`Any` composite arms so a multi-condition query is one flat operand list, never a nested binary tree; `ElementSet.Bake(Op key)` lowers the selected object set to the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` family (the one `Fin`-railed step, railing `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` on a cyclic `Compose` chain or an absent root), and `ElementSet.GlobalIds` projects the IFC `ExternalId` set a `Review/validation#IDS_FACETS` IDS receipt or a `Review/issues#BCF_ARCHIVE` BCF viewpoint keys on.
- Auto: `Match` dispatches the Thinktecture generated total `Switch` carrying the `(graph, obj)` state tuple into every arm — `ByClass`/`ByDomain`/`ByPredefinedType` read the `Node.Object`'s generic `Classification("ifc", code)` the projector stamps (resolving the `Model/elements#IFC_CLASS` `IfcClass` row for the `IfcDomain` partition), `ByClassification` the `Classification.Within` branch-containment over any standard system, `ByKind` the `ObjectKind` occurrence/type discriminant, `ByAttribute` the `ObjectAttribute.Read` projection, `ByProperty` the effective `PropertyValue` (the occurrence bag merged with the type bag via the seam `PropertyBag.Merge` under the stamped `InheritanceMode`, over BOTH the `PropertySet` and `QuantitySet` bags so a `Pset_*` property and a `Qto_*` quantity share one arm), `ByMaterial` the `Associate`-bound `Material` node's composition material set, and the incidence arms (`BySpatialContainer`/`ByType`/`ByZone`/`ByConnected`/`ByAssessment`) the neutral `Compose`/`Assign`/`Connect` edges through the built-once `EdgesAt` incidence index (`ByAssessment` the `Assign{Assessment}` receipt of a `Discipline`, optionally an `AssessmentOutcome`); the `All`/`Any`/`Not` arms recurse the same `Match` so the boolean closure is one total dispatch; a new arm breaks every `Match` site at compile time until added, so a missing query dimension is a build error, not a silent fallthrough.
- Growth: a new query dimension is one `ElementPredicate` arm folded by the same set algebra and one matching `Match` Switch arm; a new value-restriction is one `ValueMatch` arm the value-bearing arms already fold; a new queryable object attribute is one `ObjectAttribute` row; a new set combinator is one member on the closed algebra; never a `Get<Dimension>` operation family and never a parallel selection surface.
- Boundary: there is ONE polymorphic query surface (`ElementSet.Query` discriminating on the `ElementQuery` predicate expression) and a `GetWalls`/`GetByLevel`/`GetByMaterial`/`FindBy<Key>` family is the deleted form per the no-operation-family law; the query folds the seam `ElementGraph` object nodes and the retired `BimModel.Elements`/`BimElement` element record is GONE — a `new ElementSet(model.Elements)` over a second stored record is the deleted form, the selection reading the graph the `Bake` fold derives the consumer `Element` from; the predicate is a fold over a closed `ElementPredicate` union, never an imperative filter loop with mutable accumulation, and the result is an immutable `ElementSet` composing through the LanguageExt `HashSet<NodeId>` set algebra (O(n) union/intersect/difference), never a `Seq<BimElement>` mutated in place or an O(n·m) `DistinctBy`/`Filter` rescan; `ByProperty` carries the typed `ValueMatch` over the seam `PropertyValue`/`MeasureValue` and the stringly `ByProperty(string, string, string)` triple is the deleted form, the value match deciding pattern/range/exact/one-of/length typed (the whole-value-anchored NonBacktracking regex, the inclusive-or-exclusive dimension-checked bound, the rendered-length structure facet), never a substring `IsMatch` nor a raw-scalar bound; `ByClass` matches the `Node.Object`'s generic `Classification("ifc", code)` exactly and `ByClassification` the `Classification.Within` standard-system branch, never an `IfcClass` field on the seam node (the seam carries the generic `Classification`, the `IfcClass` vocabulary being the `Model/elements#IFC_CLASS` projector's); the incidence arms read the neutral edge algebra with the projector's directionality (`BySpatialContainer` the `Compose` containment whole→part, `ByType` the `Assign` type-definition occurrence→type, `ByZone` the `Assign` group membership, `ByConnected` the `Connect` adjacency) and a typed `IfcRel*` case crossing the query is the deleted form; the `ByZone` arm reads the many-to-many grouping edges the single-parent `BySpatialContainer` containment arm cannot express and the `ByConnected` arm the `Model/systems#CONNECTIVITY` flow adjacency; the same `ElementPredicate` algebra backs the `Review/validation#IDS_FACETS` IDS facet fold and the `Review/coordination#COORDINATION` rules, so the validation predicate IS the query predicate, never a second selection surface.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text.RegularExpressions;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;
using NodeSet = LanguageExt.HashSet<Rasm.Element.NodeId>;   // the selection identity set — equality-keyed (NodeId carries
                                                            // no ordering comparer), aliased so the bare name never collides
                                                            // with the global-using System.Collections.Generic.HashSet.

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The queryable direct-attribute vocabulary the ByAttribute arm reads off a Node.Object, keyed by the IFC
// attribute name so a Review/validation#IDS_FACETS Attribute facet resolves ObjectAttribute.TryGet(name); the
// Read projection (delegate-backed enum behaviour) lifts the attribute to the typed PropertyValue a ValueMatch
// decides. Name/Tag are the seam Object string columns, GlobalId the optional ExternalId (the IFC GlobalId).
[SmartEnum<string>]
public sealed partial class ObjectAttribute {
    public static readonly ObjectAttribute Name     = new("Name",     static o => o.Name is { Length: > 0 } n ? Some<PropertyValue>(new PropertyValue.Text(n)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute Tag      = new("Tag",      static o => o.Tag is { Length: > 0 } t ? Some<PropertyValue>(new PropertyValue.Text(t)) : Option<PropertyValue>.None);
    public static readonly ObjectAttribute GlobalId = new("GlobalId", static o => o.ExternalId.Map(static e => (PropertyValue)new PropertyValue.Text(e)));

    public Func<Node.Object, Option<PropertyValue>> Read { get; }
}

// The typed value-restriction the value-bearing arms (ByProperty/ByAttribute/ByMaterial) decide a candidate
// PropertyValue through — the Review/validation#IDS_FACETS IDS ValueConstraint family (exact/pattern/range/
// one-of) lowered onto the seam typed value, never a String.Equals. Present is the structural-existence match
// (the candidate exists at all); the arms invoke Matches only on a resolved candidate, so Present + a resolved
// candidate IS "the property/attribute/material is present".
[Union]
public abstract partial record ValueMatch {
    private ValueMatch() { }

    public sealed record Present : ValueMatch;
    public sealed record Exact(PropertyValue Value) : ValueMatch;
    public sealed record Pattern(string Expression) : ValueMatch;
    public sealed record Range(Option<MeasureValue> Lower, Option<MeasureValue> Upper, bool LowerInclusive = true, bool UpperInclusive = true) : ValueMatch;
    public sealed record OneOf(Seq<string> Allowed) : ValueMatch;
    public sealed record Length(Option<int> Min, Option<int> Max) : ValueMatch;

    public static readonly ValueMatch Any = new Present();

    public bool Matches(PropertyValue value) => Switch(
        state:    value,
        present:  static (_, _) => true,
        exact:    static (v, m) => v.Equals(m.Value),
        pattern:  static (v, m) => Compiled(m.Expression) is { } rx && rx.IsMatch(v.Render()),
        range:    static (v, m) => v is PropertyValue.Measure measure && InRange(measure.Value, m),
        oneOf:    static (v, m) => m.Allowed.Exists(a => string.Equals(a, v.Render(), StringComparison.OrdinalIgnoreCase)),
        length:   static (v, m) => InLength(v.Render().Length, m));

    // The Review/validation#IDS_FACETS facet lowers an XSD pattern facet onto a Pattern arm as a FOREIGN, untrusted
    // regex string: compile it once per pattern (cached), ANCHORED whole-value (\A(?:…)\z — an XSD pattern facet is a
    // whole-value match, never a substring) and NonBacktracking (linear-time, so a catastrophic foreign pattern can
    // never ReDoS-hang the fold); an uncompilable or NonBacktracking-unsupported pattern is a non-match, so the total
    // Matches never recompiles per candidate, never substring-matches an XSD facet, and never throws into domain logic.
    static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Regex?> compiledPatterns = new();
    static Regex? Compiled(string pattern) =>
        compiledPatterns.GetOrAdd(pattern, static p => {
            try { return new Regex($@"\A(?:{p})\z", RegexOptions.NonBacktracking | RegexOptions.CultureInvariant); }
            catch (Exception e) when (e is ArgumentException or NotSupportedException) { return null; }
        });

    // The numeric bound test reads the SI magnitude the seam MeasureValue carries AND requires the bound to share the
    // candidate's Dimension (a length never satisfies a pressure bound — the same dimension law MeasureValue.Sum rails
    // on), so a cross-dimension bound never produces a meaningless raw-scalar match; an open bound (None) is unbounded
    // on that side and the per-side LowerInclusive/UpperInclusive flag selects >=/> and <=/< so the IDS RangeConstraint
    // min/maxExclusive bound lowers onto the same arm, a non-measure candidate never satisfying (range gates on Measure first).
    static bool InRange(MeasureValue v, Range m) =>
        m.Lower.Match(Some: l => v.Dimension == l.Dimension && (m.LowerInclusive ? v.Si >= l.Si : v.Si > l.Si), None: static () => true)
        && m.Upper.Match(Some: u => v.Dimension == u.Dimension && (m.UpperInclusive ? v.Si <= u.Si : v.Si < u.Si), None: static () => true);

    // The IDS StructureConstraint length facet (length/minLength/maxLength) over the rendered value's character count,
    // each open bound (None) unbounded on that side — a string-length restriction lowers onto one arm without a measure.
    static bool InLength(int len, Length m) =>
        m.Min.Match(Some: lo => len >= lo, None: static () => true) && m.Max.Match(Some: hi => len <= hi, None: static () => true);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The closed predicate algebra: one arm per query dimension over the seam graph, the All/Any/Not boolean
// closure recursing the same Match. The neutral-edge arms read the Rasm.Element/Relations/relation#EDGE_ALGEBRA
// Compose/Assign/Connect edges by the projector's directionality, never a typed IfcRel* case.
[Union]
public abstract partial record ElementPredicate {
    private ElementPredicate() { }

    public sealed record ByClass(IfcClass Class) : ElementPredicate;                               // exact IFC entity class (Classification "ifc", Class.Key)
    public sealed record ByDomain(IfcDomain Domain) : ElementPredicate;                            // the IfcClass.Domain discipline partition
    public sealed record ByPredefinedType(IfcClass Class, PredefinedType Type) : ElementPredicate; // entity class + the typed predefined token
    public sealed record ByClassification(Classification Branch) : ElementPredicate;              // standard-system Classification.Within branch containment
    public sealed record ByKind(ObjectKind Kind) : ElementPredicate;                              // occurrence vs type node
    public sealed record ByAttribute(ObjectAttribute Attribute, ValueMatch Restriction) : ElementPredicate; // a Node.Object direct attribute (Name/Tag/GlobalId)
    public sealed record ByProperty(string SetName, PropertyName Name, ValueMatch Restriction) : ElementPredicate; // effective Pset/Qto value (type→occurrence merged)
    public sealed record ByMaterial(ValueMatch Restriction) : ElementPredicate;                   // an Associate-bound material's composition material key
    public sealed record BySpatialContainer(NodeId Container) : ElementPredicate;                 // the Compose{Contain} spatial parent
    public sealed record ByType(NodeId Type) : ElementPredicate;                                  // the Assign{TypeDefinition} bound type object
    public sealed record ByZone(NodeId Group) : ElementPredicate;                                 // an Assign{Group} grouping/zone/system membership
    public sealed record ByConnected(NodeId Other, Option<ConnectKind> Kind) : ElementPredicate;  // a Connect adjacency, optionally a flavor
    public sealed record ByAssessment(Discipline Discipline, Option<AssessmentOutcome> Outcome) : ElementPredicate; // an Assign{Assessment} receipt of a discipline, optionally an outcome
    public sealed record All(Seq<ElementPredicate> Operands) : ElementPredicate;
    public sealed record Any(Seq<ElementPredicate> Operands) : ElementPredicate;
    public sealed record Not(ElementPredicate Operand) : ElementPredicate;
}

public sealed record ElementQuery(ElementPredicate Predicate) {
    public static ElementQuery Of(ElementPredicate predicate) => new(predicate);

    // And/Or flatten into the existing composite so a chain of conjunctions is one flat All operand list rather
    // than a right-nested binary tree, keeping Match recursion shallow; AndNot conjoins the negation.
    public ElementQuery And(ElementPredicate other) => new(Predicate is ElementPredicate.All all
        ? new ElementPredicate.All(all.Operands.Add(other))
        : new ElementPredicate.All(Seq(Predicate, other)));

    public ElementQuery Or(ElementPredicate other) => new(Predicate is ElementPredicate.Any any
        ? new ElementPredicate.Any(any.Operands.Add(other))
        : new ElementPredicate.Any(Seq(Predicate, other)));

    public ElementQuery AndNot(ElementPredicate other) => And(new ElementPredicate.Not(other));
}

public sealed record ElementSet(ElementGraph Graph, NodeSet Ids) {
    // The IFC entity-class system token the Projection/semantic#SEMANTIC_PROJECTOR Objects fold stamps onto every
    // Node.Object as Classification("ifc", IfcClass.Key); the seam Classification.Create lower-cases the system, so
    // ByClass/ByDomain/ByPredefinedType match the lower-case token, the IfcClass roster staying the projector's.
    const string IfcSystem = "ifc";

    public static ElementSet Query(ElementGraph graph, ElementQuery query) =>
        new(graph, new NodeSet(graph.ObjectNodes.Filter(o => Match(graph, o, query.Predicate)).Map(static o => o.Id)));

    public int Count => Ids.Count;
    public Seq<Node.Object> Objects => Graph.ObjectNodes.Filter(o => Ids.Contains(o.Id));
    public Seq<string> GlobalIds => Objects.Choose(static o => o.ExternalId);

    // The one Fin-railed step: bake every selected object into the seam Bake-derived Element ("has it all"),
    // railing ElementFault on a cyclic Compose or an absent root the selection never reaches in a healthy graph.
    public Fin<Seq<Element>> Bake(Op key) =>
        Objects.TraverseM(o => Graph.Bake(o.Id, key)).As().Map(static es => es.ToSeq());

    // The set algebra composes selections over the SAME graph through the LanguageExt HashSet operators, so a
    // complex selection is one expression, never an imperative accumulation loop; the graph carries through.
    public ElementSet Union(ElementSet other)     => this with { Ids = Ids.Union(other.Ids) };
    public ElementSet Intersect(ElementSet other) => this with { Ids = Ids.Intersect(other.Ids) };
    public ElementSet Except(ElementSet other)    => this with { Ids = Ids.Except(other.Ids) };

    // One Where, one modality: refine the current set by another ElementPredicate — re-folds ONLY the current
    // members, not the whole graph — so the closed predicate algebra stays the single selection surface; a raw
    // Func<Node.Object, bool> escape hatch is the deleted form (a refinement the algebra cannot express is one new
    // ElementPredicate arm, never an untyped second selection surface beside the IDS/coordination predicate reuse).
    public ElementSet Where(ElementPredicate predicate) =>
        this with { Ids = new NodeSet(Objects.Filter(o => Match(Graph, o, predicate)).Map(static o => o.Id)) };

    // The total predicate fold: the Thinktecture generated Switch carries the (graph, obj) state into every arm,
    // so a missing arm is a build error at every Match site. The classification arms read the Object node's
    // primary Classification (the entity-class pair) — byClassification ALSO searching the Classifications set of co-applied
    // standard references (Uniclass + OmniClass); the incidence arms read the neutral edges through the O(degree) EdgesAt index.
    static bool Match(ElementGraph graph, Node.Object obj, ElementPredicate predicate) => predicate.Switch(
        state: (graph, obj),
        byClass:            static (s, p) => s.obj.Classification.System == IfcSystem
                                             && string.Equals(s.obj.Classification.Code, p.Class.Key, StringComparison.OrdinalIgnoreCase),
        byDomain:           static (s, p) => s.obj.Classification.System == IfcSystem
                                             && IfcClass.TryGet(s.obj.Classification.Code).Exists(c => c.Domain == p.Domain),
        byPredefinedType:   static (s, p) => s.obj.Classification.System == IfcSystem
                                             && string.Equals(s.obj.Classification.Code, p.Class.Key, StringComparison.OrdinalIgnoreCase)
                                             && s.obj.PredefinedType == p.Type,
        byClassification:   static (s, p) => s.obj.Classification.Within(p.Branch) || s.obj.Classifications.Exists(c => c.Within(p.Branch)),
        byKind:             static (s, p) => s.obj.Kind == p.Kind,
        byAttribute:        static (s, p) => p.Attribute.Read(s.obj).Exists(v => p.Restriction.Matches(v)),
        byProperty:         static (s, p) => EffectiveValue(s.graph, s.obj.Id, p.SetName, p.Name).Exists(v => p.Restriction.Matches(v)),
        byMaterial:         static (s, p) => s.graph.MaterialsOf(s.obj.Id)
                                                 .Exists(m => m.Composition.Materials.Exists(id => p.Restriction.Matches(new PropertyValue.Text(id.Value)))),
        bySpatialContainer: static (s, p) => s.graph.EdgesAt(s.obj.Id)
                                                 .Any(e => e is Relationship.Compose c && c.IsContainment && c.Part == s.obj.Id && c.Whole == p.Container),
        byType:             static (s, p) => s.graph.EdgesAt(s.obj.Id)
                                                 .Any(e => e is Relationship.Assign a && a.SubKind == AssignKind.TypeDefinition && a.Subject == s.obj.Id && a.Definition == p.Type),
        byZone:             static (s, p) => s.graph.EdgesAt(s.obj.Id)
                                                 .Any(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Touches(p.Group)),
        byConnected:        static (s, p) => s.graph.EdgesAt(s.obj.Id)
                                                 .Any(e => e is Relationship.Connect c && c.Touches(p.Other) && p.Kind.Match(Some: k => c.SubKind == k, None: static () => true)),
        byAssessment:       static (s, p) => s.graph.EdgesAt(s.obj.Id)
                                                 .Any(e => e is Relationship.Assign a && a.SubKind == AssignKind.Assessment && a.Subject == s.obj.Id
                                                           && s.graph.Nodes.TryGetValue(a.Definition, out Node? n) && n is Node.Assessment asm
                                                           && asm.Payload.Discipline == p.Discipline && p.Outcome.Match(Some: o => asm.Payload.Outcome == o, None: static () => true)),
        all:                static (s, p) => p.Operands.ForAll(op => Match(s.graph, s.obj, op)),
        any:                static (s, p) => p.Operands.Exists(op => Match(s.graph, s.obj, op)),
        not:                static (s, p) => !Match(s.graph, s.obj, p.Operand));

    // The effective property value: the occurrence bag merged with the type bag via the seam PropertyBag/QuantityBag.Merge
    // under the stamped InheritanceMode (never a re-implemented precedence), property first then a Qto_* quantity wrapped
    // as a PropertyValue.Measure so a (SetName, Name) targeting either bag reads one typed value — over the SAME edge
    // conventions the seam Bake reads, so query selection and Bake agree on inheritance without a heavy full Bake.
    static Option<PropertyValue> EffectiveValue(ElementGraph graph, NodeId obj, string setName, PropertyName name) {
        var (occProps, occQty) = BagsOf(graph, obj);
        var (typProps, typQty) = TypeIdOf(graph, obj).Match(Some: t => BagsOf(graph, t), None: static () => (Seq<PropertyBag>(), Seq<QuantityBag>()));
        return Resolve(occProps, typProps, setName).Bind(b => b.Find(name))
            .OrElse(() => Resolve(occQty, typQty, setName).Bind(b => b.Find(name)).Map(static m => (PropertyValue)new PropertyValue.Measure(m)));
    }

    // ONE edge walk gathers BOTH bag kinds a node's own Assign{PropertyDefinition} edges attach — the (Props, Qty)
    // shape the seam Bake.TypeBagsOf reads — so the property and quantity resolution share one walk; the caller reaches
    // the type object's bags by walking from the TypeIdOf id, the SAME Assign{TypeDefinition}/{PropertyDefinition} edge
    // conventions the seam Bake reads, never four near-identical per-bag-kind walks.
    static (Seq<PropertyBag> Props, Seq<QuantityBag> Qty) BagsOf(ElementGraph graph, NodeId id) =>
        graph.EdgesAt(id).Fold(
            (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>()),
            (acc, e) => e is Relationship.Assign { SubKind: var k, Subject: var subj, Definition: var def } && k == AssignKind.PropertyDefinition && subj == id && graph.Nodes.TryGetValue(def, out Node? n)
                ? n switch {
                    Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) },
                    Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) },
                    _                   => acc,
                }
                : acc);

    static Option<NodeId> TypeIdOf(ElementGraph graph, NodeId obj) =>
        graph.EdgesAt(obj).Choose(e =>
            e is Relationship.Assign { SubKind: var k, Subject: var subj, Definition: var def } && k == AssignKind.TypeDefinition && subj == obj
                ? Some(def) : Option<NodeId>.None).Head;

    // One named bag resolution, two arities: the occurrence bag matching SetName merged with its type counterpart via
    // the seam Merge (the occurrence carrying the stamped InheritanceMode), or the type-only bag inherited as-is.
    static Option<PropertyBag> Resolve(Seq<PropertyBag> occurrence, Seq<PropertyBag> type, string setName) =>
        occurrence.Find(b => b.SetName == setName).Match(
            Some: occ => Some(type.Find(b => b.SetName == setName).Match(Some: typ => PropertyBag.Merge(typ, occ), None: () => occ)),
            None: () => type.Find(b => b.SetName == setName));

    static Option<QuantityBag> Resolve(Seq<QuantityBag> occurrence, Seq<QuantityBag> type, string setName) =>
        occurrence.Find(b => b.SetName == setName).Match(
            Some: occ => Some(type.Find(b => b.SetName == setName).Match(Some: typ => QuantityBag.Merge(typ, occ), None: () => occ)),
            None: () => type.Find(b => b.SetName == setName));
}
```

## [03]-[RESEARCH]

- [PREDICATE_ALGEBRA]: the `ElementPredicate` union and the `Match` fold ground against the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` (`ObjectNodes`/`EdgesAt`/`Nodes`/`MaterialsOf`/`Bake`) and the neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` algebra — the incidence arms read the projector's edge directionality verbatim: `BySpatialContainer` matches a `Relationship.Compose` whose `IsContainment` (`SubKind == ComposeKind.Contain`) `Part` is the element and `Whole` the container (the `Projection/relations#RELATION_ALGEBRA` `IfcRelContainedInSpatialStructure` relating-structure→related-element fold), `ByType` an `Assign { SubKind: TypeDefinition }` whose `Subject` is the occurrence and `Definition` the type (the `IfcRelDefinesByType` fold the seam `Bake` `TypeBagsOf` also reads), `ByZone` an `Assign { SubKind: Group }` incident to the element and the group (`IfcRelAssignsToGroup`), `ByConnected` a `Connect` adjacency through `Touches` (`IfcRelConnectsElements`/`ConnectsPorts`), and `ByAssessment` an `Assign { SubKind: Assessment }` whose `Definition` resolves a `Graph/element#NODE_MODEL` `Node.Assessment` of the requested `Classification/classification#DISCIPLINE_AXIS` `Discipline` (and optional `Assessment/assessment#ASSESSMENT_NODE` `AssessmentOutcome`), so a `Rasm.Compute` analysis receipt the route writes back is itself a selection dimension; `ByClass`/`ByDomain`/`ByPredefinedType` read the `Node.Object` generic `Classification("ifc", code)` the projector stamps (never an `IfcClass` field on the node), resolving the `Model/elements#IFC_CLASS` `IfcClass.TryGet`/`Domain` for the discipline partition; the Thinktecture generated total `Switch` carrying the `(graph, obj)` state breaks every `Match` site until a new arm is added, so a missing dimension is a build error.
- [VALUE_RESTRICTION]: the `ValueMatch` union grounds against the `Review/validation#IDS_FACETS` IDS `ValueConstraint` restriction family (the `ExactConstraint`/`PatternConstraint`/`RangeConstraint`/`StructureConstraint` components) lowered onto the seam `Rasm.Element/Properties/property#PROPERTY_VALUE` typed value and `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue.Si` SI magnitude — `Exact` decides the seam `PropertyValue` structural equality, `Pattern` the `PropertyValue.Render` projection through a whole-value-anchored (`\A(?:…)\z`) NonBacktracking XSD regex (so a foreign IDS pattern can neither substring-match nor ReDoS-hang the fold), `Range` the dimension-checked SI bounds with per-side inclusivity so `min/maxExclusive` lowers onto the one arm, `Length` the `StructureConstraint` length/min/max over the rendered character count, `OneOf` the rendered enumeration, and `Present` structural existence; `ByProperty` reads the EFFECTIVE value by merging the occurrence bag with the type bag through the seam `PropertyBag.Merge`/`QuantityBag.Merge` under the stamped `Rasm.Element/Properties/property#PROPERTY_BAG` `InheritanceMode` (the seam owns the precedence, the query never re-implements it) over both the `PropertySet` and `QuantitySet` bags, so a `Pset_*` property and a `Qto_*` quantity unify on one arm and a type-inherited property selects without a heavy full `Bake`.
- [ALGEBRA_REUSE]: the `ElementPredicate` algebra IS the one selection surface its consumers reuse — `Review/validation#IDS_FACETS` lowers each IDS facet to an `ElementPredicate` arm for structural selection (`Entity`→`ByClass`, `Property`→`ByProperty`, `Classification`→`ByClassification`, `Material`→`ByMaterial`, `Attribute`→`ByAttribute`, `PartOf`→`BySpatialContainer`), `Review/coordination#COORDINATION` folds its rule applicability/requirement predicates through `ElementSet.Query`, `Planning/schedule#SCHEDULE` selects an assigned element set, and `Model/systems#CONNECTIVITY`/`Model/structural#ANALYSIS_MODEL` read `ByDomain(IfcDomain.HvacFire)`/`ByDomain(IfcDomain.Structural)`, `Model/spatial#SPATIAL_STRUCTURE` `BySpatialContainer`, and `Model/zones#ZONE_GRAPH` `ByZone` — never a second `RuleSelector`/`IdsValidator` selection surface; a rejection on those consumers' own rails lowers onto `Model/faults#FAULT_BAND` `BimFault`, while `ElementSet.Query` itself is total and `ElementSet.Bake` is the single `Fin` step railing the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault`.
