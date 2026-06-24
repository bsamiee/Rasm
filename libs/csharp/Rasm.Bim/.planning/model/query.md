# [BIM_ELEMENT_SET]

The set-algebraic element query algebra: one polymorphic `ElementSet.Query` over a closed `ElementPredicate` union, folded by `Union`/`Intersect`/`Except`/`Where` into one set-algebraic expression — never a `GetWalls`/`GetByLevel`/`GetByMaterial` operation family. The query folds over the one `BimModel` the `Model/elements#ELEMENT_MODEL` `Project` produces and consumes the `Semantics/classification#CLASSIFICATION_AXIS` `Classification` axis as settled vocabulary, never re-deriving a classification mapping.

## [01]-[INDEX]

- [01]-[ELEMENT_SET]: `ElementPredicate` closed union, `ElementQuery` expression record, and the `ElementSet` set-algebraic fold.

## [02]-[ELEMENT_SET]

- Owner: `ElementSet` the set-algebraic query fold over a `BimModel` element collection owning union/intersection/difference/filter as a closed algebra over `ElementPredicate`; `ElementPredicate` `[Union]` the closed predicate family carrying each discriminant payload; `ElementQuery` the composed query record folding predicates into one set-algebraic expression.
- Entry: `ElementSet.Query(BimModel model, ElementQuery query)` folds the composed predicate expression over the element collection into a result set — total, pure, no rail; the set-algebra combinators `Union`/`Intersect`/`Except`/`Where` compose `ElementSet` values so a complex selection is one expression, never an imperative accumulation loop.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new query dimension is one `ElementPredicate` union arm folded by the same set algebra and one matching `Match` Switch arm (the Thinktecture generated total `Switch` breaks every `Match` site at compile time until the new arm is added, so a missing dimension is a build error not a silent fallthrough); a new set combinator is one member on the closed algebra; never a `Get<Dimension>` operation family.
- Boundary: there is ONE polymorphic query surface (`ElementSet.Query` discriminating on the `ElementQuery` predicate expression) and a `GetWalls`/`GetByLevel`/`GetByMaterial`/`FindBy<Key>` family is the deleted form per the no-operation-family law; the `All`/`Any` composite arms close the union under conjunction and disjunction so `ElementQuery.And`/`Or` compose without a second expression type, and `Match` recurses through them as one total dispatch; the query is a fold over a closed `ElementPredicate` union, never an imperative filter loop with mutable accumulation; the result is an immutable `ElementSet` composing through the set algebra, never a `List<BimElement>` mutated in place; the `ByDomain` arm discriminates on the `Model/elements#ELEMENT_MODEL` widened `IfcDomain` seven-case partition so a discipline query selects every architectural/structural/MEP/infrastructure element without a per-class arm, and the `ByPredefinedType` arm filters within a class by the typed `PredefinedType` value-object so a load-bearing-wall / exterior-door query is one arm, never a property-set string lookup; the `ByZone` arm reads the `Model/zones#ZONE_GRAPH` many-to-many overlay so a fire-compartment / thermal-zone selection folds the grouping graph the single-parent `BySpatialContainer` containment arm cannot express, the `BimModel` carrying the zone-assignment index the arm joins by `ZoneGlobalId`; the `ByClassification` arm reads the typed `BimElement.Classifications` `ClassificationRef` binding by `Classification` row and code, never a stringly-keyed property-set lookup that couples the classification axis to the property store; the classification predicate consumes the `Semantics/classification#CLASSIFICATION_AXIS` `Classification` axis as settled vocabulary; the same predicate algebra backs the `Review/validation#IDS_FACETS` IDS facet fold, so the validation predicate IS the query predicate, never a second selection surface, and the `csharp:CAPABILITY_CONTROL_PLANE` `CapabilityDescriptor` projects this `ElementPredicate` algebra as a described capability into MCP/SDK codegen, consuming the settled union by reference and minting no second control plane; the same `ElementPredicate` algebra is also the projection substrate the host-neutral IFC validation rules cross to the durable store on — an `ElementPredicate.ByProperty` mandatory-pset rule projects into a `csharp:Rasm.Persistence/Store/quality#QUALITY_RULE` `QualityRule.NotNullSet`/`RegexShape` row, a `ByClassification`/`ByPredefinedType` requirement into the matching shape rule, and a `Review/validation#IDS_FACETS` `IdsRequirement` (an applicability-`ElementPredicate` plus a `Required`/`Prohibited` cardinality) into the equivalent `QualityRule` row — so a buildingSMART IDS requirement enforced over the federated store is one declarative `QualityRule` row Bim authors and hands across the `Model → csharp:Rasm.Persistence/Store/quality # [SHAPE]: IFC validation rules into QualityRule rows` seam, the `IdsSpecification.Audit` in-process fold staying the immediate host-local self-audit while the durable cross-document enforcement, the lowering to the cheapest CHECK/EXCLUDE/FK/`jsonb_matches_schema` site, and the violating-row `ElementSet` projection stay wholly the Persistence `QualityPlan` concern — Bim mints no constraint, no second integrity engine, and no parallel checker, projecting only the rule row by reference.

```csharp signature
[Union]
public partial record ElementPredicate {
    partial record ByClass(IfcClass Class);
    partial record ByDomain(IfcDomain Domain);
    partial record ByPredefinedType(IfcClass Class, PredefinedType Type);
    partial record ByProperty(string SetName, string Name, string Value);
    partial record ByClassification(Classification System, ClassificationCode Code);
    partial record BySpatialContainer(string ContainerGlobalId);
    partial record ByZone(string ZoneGlobalId);
    partial record ByType(string TypeGlobalId);
    partial record All(Seq<ElementPredicate> Operands);
    partial record Any(Seq<ElementPredicate> Operands);
}

public sealed record ElementQuery(ElementPredicate Predicate) {
    public ElementQuery And(ElementPredicate other) => new(new ElementPredicate.All(Seq(Predicate, other)));
    public ElementQuery Or(ElementPredicate other) => new(new ElementPredicate.Any(Seq(Predicate, other)));
}

public sealed record ElementSet(Seq<BimElement> Elements) {
    public static ElementSet Query(BimModel model, ElementQuery query) =>
        new(model.Elements.Filter(element => Match(model, element, query.Predicate)));

    public ElementSet Union(ElementSet other) => new(Elements.Concat(other.Elements).DistinctBy(static e => e.GlobalId).ToSeq());
    public ElementSet Intersect(ElementSet other) => new(Elements.Filter(e => other.Elements.Exists(o => o.GlobalId == e.GlobalId)));
    public ElementSet Except(ElementSet other) => new(Elements.Filter(e => !other.Elements.Exists(o => o.GlobalId == e.GlobalId)));
    public ElementSet Where(Func<BimElement, bool> predicate) => new(Elements.Filter(predicate));

    static bool Match(BimModel model, BimElement element, ElementPredicate predicate) => predicate.Switch(
        state: (model, element),
        byClass:            static (s, p) => s.element.Class == p.Class,
        byDomain:           static (s, p) => s.element.Class.Domain == p.Domain,
        byPredefinedType:   static (s, p) => s.element.Class == p.Class && s.element.Predefined == p.Type,
        byProperty:         static (s, p) => s.element.Properties.Exists(b => b.SetName == p.SetName && b.Name == p.Name && b.Value == p.Value),
        byClassification:   static (s, p) => s.element.Classifications.Exists(r => r.System == p.System && r.Code == p.Code),
        bySpatialContainer: static (s, p) => s.element.SpatialContainerId == p.ContainerGlobalId,
        byZone:             static (s, p) => s.model.Zones.Find(p.ZoneGlobalId).Exists(members => members.Contains(s.element.GlobalId)),
        byType:             static (s, p) => s.element.TypeGlobalId == p.TypeGlobalId,
        all:                static (s, p) => p.Operands.ForAll(operand => Match(s.model, s.element, operand)),
        any:                static (s, p) => p.Operands.Exists(operand => Match(s.model, s.element, operand)));
}
```

## [03]-[RESEARCH]

- [SET_ALGEBRA]: the `ElementSet` set-algebraic fold (`Union`/`Intersect`/`Except`/`Where` over `ElementPredicate`) and the `ElementPredicate` union-arm payloads ground against the LanguageExt immutable-collection combinator surface and the `BimModel` element-collection shape; the recursive `All`/`Any` composite arms backing `ElementQuery.And`/`Or` confirm against the closed-union generated `Switch` total-dispatch contract carrying the `(model, element)` state tuple into every arm and the `Seq<ElementPredicate>.ForAll`/`Exists` recursive-fold spelling, so the new `ByDomain`/`ByPredefinedType`/`ByZone` arms break the generated `Switch` at every `Match` site until they are added; the `ByClassification` arm matches the typed `ClassificationRef.Code` `ClassificationCode` value-object by equality, the `ByPredefinedType` arm the typed `PredefinedType` value-object, never a stringly-keyed code, so the classification and predefined axes stay typed end-to-end; the `ByZone` arm reads the `model.Zones` index the `Model/zones#ZONE_GRAPH` overlay populates so the grouping query folds the many-to-many assignment graph rather than the single-parent containment tree.
