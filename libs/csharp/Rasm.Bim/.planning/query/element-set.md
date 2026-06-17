# [BIM_ELEMENT_SET]

The set-algebraic element query algebra: one polymorphic `ElementSet.Query` over a closed `ElementPredicate` union, folded by `Union`/`Intersect`/`Except`/`Where` into one set-algebraic expression — never a `GetWalls`/`GetByLevel`/`GetByMaterial` operation family. The query folds over the one `BimModel` the `model/elements#ELEMENT_MODEL` `Project` produces and consumes the `classification/systems#CLASSIFICATION_AXIS` `Classification` axis as settled vocabulary, never re-deriving a classification mapping.

## [1]-[INDEX]

- [2]-[ELEMENT_SET]: `ElementPredicate` closed union, `ElementQuery` expression record, and the `ElementSet` set-algebraic fold.

## [2]-[ELEMENT_SET]

- Owner: `ElementSet` the set-algebraic query fold over a `BimModel` element collection owning union/intersection/difference/filter as a closed algebra over `ElementPredicate`; `ElementPredicate` `[Union]` the closed predicate family carrying each discriminant payload; `ElementQuery` the composed query record folding predicates into one set-algebraic expression.
- Entry: `ElementSet.Query(BimModel model, ElementQuery query)` folds the composed predicate expression over the element collection into a result set — total, pure, no rail; the set-algebra combinators `Union`/`Intersect`/`Except`/`Where` compose `ElementSet` values so a complex selection is one expression, never an imperative accumulation loop.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new query dimension is one `ElementPredicate` union arm folded by the same set algebra; a new set combinator is one member on the closed algebra; never a `Get<Dimension>` operation family.
- Boundary: there is ONE polymorphic query surface (`ElementSet.Query` discriminating on the `ElementQuery` predicate expression) and a `GetWalls`/`GetByLevel`/`GetByMaterial`/`FindBy<Key>` family is the deleted form per the no-operation-family law; the `All`/`Any` composite arms close the union under conjunction and disjunction so `ElementQuery.And`/`Or` compose without a second expression type, and `Match` recurses through them as one total dispatch; the query is a fold over a closed `ElementPredicate` union, never an imperative filter loop with mutable accumulation; the result is an immutable `ElementSet` composing through the set algebra, never a `List<BimElement>` mutated in place; the `ByClassification` arm reads the typed `BimElement.Classifications` `ClassificationRef` binding by `Classification` row and code, never a stringly-keyed property-set lookup that couples the classification axis to the property store; the classification predicate consumes the `classification/systems#CLASSIFICATION_AXIS` `Classification` axis as settled vocabulary; the same predicate algebra backs the `validation/ids#IDS_FACETS` IDS facet fold, so the validation predicate IS the query predicate, never a second selection surface.

```csharp signature
[Union]
public partial record ElementPredicate {
    partial record ByClass(IfcClass Class);
    partial record ByProperty(string SetName, string Name, string Value);
    partial record ByClassification(Classification System, string Code);
    partial record BySpatialContainer(string ContainerGlobalId);
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
        new(model.Elements.Filter(element => Match(element, query.Predicate)));

    public ElementSet Union(ElementSet other) => new(Elements.Concat(other.Elements).DistinctBy(static e => e.GlobalId).ToSeq());
    public ElementSet Intersect(ElementSet other) => new(Elements.Filter(e => other.Elements.Exists(o => o.GlobalId == e.GlobalId)));
    public ElementSet Except(ElementSet other) => new(Elements.Filter(e => !other.Elements.Exists(o => o.GlobalId == e.GlobalId)));
    public ElementSet Where(Func<BimElement, bool> predicate) => new(Elements.Filter(predicate));

    static bool Match(BimElement element, ElementPredicate predicate) => predicate switch {
        ElementPredicate.ByClass byClass => element.Class == byClass.Class,
        ElementPredicate.ByProperty p => element.Properties.Exists(b => b.SetName == p.SetName && b.Name == p.Name && b.Value == p.Value),
        ElementPredicate.ByClassification c => element.Classifications.Exists(r => r.System == c.System && r.Code.Value == c.Code),
        ElementPredicate.BySpatialContainer s => element.SpatialContainerId == s.ContainerGlobalId,
        ElementPredicate.ByType t => element.TypeGlobalId == t.TypeGlobalId,
        ElementPredicate.All all => all.Operands.ForAll(operand => Match(element, operand)),
        ElementPredicate.Any any => any.Operands.Exists(operand => Match(element, operand)),
    };
}
```

## [3]-[RESEARCH]

- [SET_ALGEBRA]: the `ElementSet` set-algebraic fold (`Union`/`Intersect`/`Except`/`Where` over `ElementPredicate`) and the `ElementPredicate` union-arm payloads ground against the LanguageExt immutable-collection combinator surface and the `BimModel` element-collection shape; the recursive `All`/`Any` composite arms backing `ElementQuery.And`/`Or` confirm against the closed-union case-generation contract and the `Seq<ElementPredicate>.ForAll`/`Exists` fold spelling.
