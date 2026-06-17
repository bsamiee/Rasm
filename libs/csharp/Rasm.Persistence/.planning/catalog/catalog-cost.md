# [PERSISTENCE_CATALOG_COST]

Rasm.Persistence owns the classification and cost catalogs powering quantity takeoff and 5D cost: `Classification` is the multi-standard classification axis (Uniclass, OmniClass, MasterFormat, IfcClassification) with hierarchical codes; `ClassificationMap` resolves a federated entity to its classification codes across standards; `CostCode` maps a classification to a cost-code with a rate; and `CostRollup` evaluates a cost formula over a quantity takeoff `ElementSet`, folding the formula-evaluated line items into a hierarchical rollup through the DuckDB analytical lane. The federated entity graph (`federation#ENTITY_GRAPH`), the element-set currency (`federation#ELEMENT_SET_ALGEBRA`), the DuckDB analytical lane (`data-lanes#ANALYTICAL_LANE`), the ltree classification hierarchy, and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled and compose inside the fences.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]               | [OWNS]                                                            |
| :-----: | :---------------------- | :--------------------------------------------------------------- |
|   [1]   | CLASSIFICATION_CATALOG  | Multi-standard classification axis; hierarchical codes; entity map |
|   [2]   | COST_ROLLUP             | Cost-code mapping; formula-evaluated line items; DuckDB rollup    |

## [2]-[CLASSIFICATION_CATALOG]

- Owner: `ClassificationStandard` the standard axis (Uniclass, OmniClass, MasterFormat, IfcClassification); `ClassificationCode` the hierarchical code record carrying the ltree path; `ClassificationMap` the federated-entity-to-codes resolution; `Catalog` the static surface owning the catalog load, the hierarchical code lookup, the cross-standard mapping, and the entity-classification fold.
- Cases: `Uniclass | OmniClass | MasterFormat | IfcClassification` on `ClassificationStandard`; a code carries its standard, its ltree path (so `Pr_20_93_52` and `23-30 00 00` are both hierarchical), its title, and its parent; an entity maps to zero or more codes across standards.
- Entry: `public static ClassificationCode Code(ClassificationStandard standard, string code, string title, string parentPath)` — projects a hierarchical code with its ltree path; `public static Seq<ClassificationCode> Resolve(FederatedEntity entity, Func<string, Seq<ClassificationCode>> byTag)` resolves a federated entity to its classification codes from its property sets and tags.
- Auto: each classification standard loads as a catalog table whose code column is an `ltree` path so a hierarchical query (`every code under Pr_20`) rides the ltree `lquery` operators (`data-lanes#GEO_LANES` ltree row) — no per-standard hierarchy walker; cross-standard mapping is a catalog table relating a code in one standard to its equivalent in another so a Uniclass-classified entity reports its OmniClass and MasterFormat equivalents through one join; a federated entity resolves to codes from its `IfcClassification` reference psets and from a classification rule (`federation#RULE_PLAN` `ByClassification`) so an unclassified entity surfaces in a takeoff as an `unclassified` line item; the catalog is content-addressed so a standard's published edition dedupes.
- Receipt: a catalog load rides `store.catalog.load` carrying the standard and the code count; an entity classification rides `store.catalog.classify`.
- Packages: System.IO.Hashing, Npgsql.EntityFrameworkCore.PostgreSQL, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new classification standard is one `ClassificationStandard` row plus its catalog table; a new cross-standard mapping is one mapping-table row; zero new surface — a per-standard classification model, a hardcoded code hierarchy, or a separate IFC classification handler is the deleted form because each standard is one axis row over the ltree-backed catalog table and the entity mapping rides the federated graph.
- Boundary: the classification standard is one axis, never a per-standard model — Uniclass tables (Ss, Pr, EF, Ac), OmniClass tables (11-49), MasterFormat divisions, and IfcClassification references are all `ClassificationCode` rows differentiated by the `Standard` column, so a cross-standard rollup is one query over the one catalog; the code hierarchy is an ltree path so a roll-up-to-parent or a drill-to-children rides the `lquery` operators the data-lanes ltree row owns, never a recursive code-string parse; cross-standard equivalence is a mapping table so a Uniclass-classified takeoff reports MasterFormat cost divisions through one join, and a hardcoded code-to-code map is the deleted form; the entity classification reads the federated entity's IFC classification psets and a classification rule so it rides the federated graph and the element-set algebra, never a per-entity classification column duplicated across sources; the catalog is content-addressed per published edition so a standard revision is a new content key and a takeoff pins the edition it classified against, so a rate-base change is traceable.

```csharp signature
public sealed class CatalogKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CatalogKeyPolicy, string>]
[KeyMemberComparer<CatalogKeyPolicy, string>]
public sealed partial class ClassificationStandard {
    public static readonly ClassificationStandard Uniclass = new("uniclass", separator: "_");
    public static readonly ClassificationStandard OmniClass = new("omniclass", separator: " ");
    public static readonly ClassificationStandard MasterFormat = new("masterformat", separator: " ");
    public static readonly ClassificationStandard IfcClassification = new("ifc-classification", separator: ".");

    public string Separator { get; }
}

public sealed record ClassificationCode(
    ClassificationStandard Standard,
    string Code,
    string LtreePath,
    string Title,
    Option<string> ParentPath,
    UInt128 EditionKey);

public sealed record ClassificationMap(Guid Entity, Seq<ClassificationCode> Codes);

public static class Catalog {
    public static ClassificationCode Code(ClassificationStandard standard, string code, string title, string parentPath, UInt128 editionKey) =>
        new(standard, code, Ltree(standard, code), title,
            parentPath.Length == 0 ? None : Some(parentPath), editionKey);

    public static Seq<ClassificationCode> Resolve(FederatedEntity entity, Func<string, Seq<ClassificationCode>> byTag) =>
        entity.PropertySets
            .Find("ifc-classification")
            .Map(field => field is CrdtField.LwwRegister reg ? Encoding.UTF8.GetString(reg.Value.Span) : "")
            .Filter(static code => code.Length > 0)
            .Map(byTag)
            .IfNone(Seq<ClassificationCode>());

    public static Seq<ClassificationCode> Descendants(Seq<ClassificationCode> catalog, string ancestorPath) =>
        catalog.Filter(code => code.LtreePath.StartsWith(ancestorPath, StringComparison.Ordinal));

    public static Option<ClassificationCode> CrossMap(Seq<(string From, string To)> mapping, Seq<ClassificationCode> target, ClassificationCode source) =>
        mapping.Find(pair => pair.From == source.LtreePath)
            .Bind(pair => target.Find(code => code.LtreePath == pair.To));

    private static string Ltree(ClassificationStandard standard, string code) =>
        code.Replace(standard.Separator, ".", StringComparison.Ordinal).Replace("-", ".", StringComparison.Ordinal);
}
```

| [INDEX] | [STANDARD]         | [CODE_FORM]               | [LTREE_PATH]                                    |
| :-----: | :----------------- | :------------------------ | :---------------------------------------------- |
|   [1]   | Uniclass           | `Pr_20_93_52`             | `Pr.20.93.52`; tables Ss/Pr/EF/Ac differentiate |
|   [2]   | OmniClass          | `23-30 00 00`             | `23.30.00.00`; tables 11-49                      |
|   [3]   | MasterFormat       | `03 30 00`                | `03.30.00`; divisions                            |
|   [4]   | IfcClassification  | `IfcClassificationReference` | reference-pset code path                      |

## [3]-[COST_ROLLUP]

- Owner: `CostCode` the classification-to-cost-code mapping with rate; `CostLineItem` a formula-evaluated takeoff line; `CostRollup` the static surface owning the formula evaluation, the quantity-takeoff line projection, and the hierarchical DuckDB rollup over the element-set.
- Cases: a cost code maps a classification path to a unit rate and a unit of measure; a line item evaluates a cost formula (`quantity * rate`, or a parametric expression over the element's quantities) into an extended amount; the rollup folds line items up the classification ltree hierarchy.
- Entry: `public static CostLineItem Evaluate(FederatedEntity entity, CostCode code, Func<FederatedEntity, string, double> quantity)` — projects a line item by evaluating the cost code's quantity measure against the entity and applying the rate; `public static IO<Seq<(string Path, double Amount)>> Rollup(DuckDBConnection lane, ElementSet subject, Seq<CostLineItem> lines)` folds the line items up the classification hierarchy through a DuckDB `GROUP BY ROLLUP` over the ltree path.
- Auto: the cost formula evaluates through the DuckDB analytical lane's SQL — `quantity * rate` and parametric expressions over the entity's property-set quantities lower to a DuckDB scalar expression so the formula is data, never a hand-rolled expression evaluator (no NCalc, no expression-tree compiler); the takeoff subject is an `ElementSet` (`federation#ELEMENT_SET_ALGEBRA`) so a cost rollup over a discipline, a level, or a clash-result set is one element-set selection; the hierarchical rollup rides DuckDB's `GROUP BY ROLLUP` over the classification ltree path so a cost summary at every hierarchy level (project → building → level → element-class) folds in one query; the rate base pins the catalog edition so a re-priced takeoff is traceable to its rate source.
- Receipt: a rollup rides `store.cost.rollup` carrying the line count and the rolled total; a re-price rides `store.cost.reprice`.
- Packages: DuckDB.NET.Data.Full, Npgsql.EntityFrameworkCore.PostgreSQL, System.IO.Hashing, LanguageExt.Core, NodaTime.
- Growth: a new cost-code dimension is one column on `CostCode`; a new formula form is one DuckDB scalar expression (data, never code); a new rollup cut is one `GROUP BY ROLLUP` grouping over the same lane; zero new surface — a hand-rolled formula-expression evaluator, a per-discipline cost calculator, or a second analytical engine is the deleted form because the formula lowers to DuckDB SQL, the takeoff subject is an element-set, and the rollup is one DuckDB ROLLUP over the ltree hierarchy.
- Boundary: the cost formula lowers to DuckDB SQL so the takeoff is an analytical query, never an imperative cost loop — a hand-rolled expression evaluator (NCalc, a custom expression-tree compiler) is the deleted form because the DuckDB engine already evaluates parametric expressions over the quantity columns, and a parametric formula like `area * 2.5 + perimeter * waste_factor` is a DuckDB scalar expression the rollup query carries; the takeoff subject is an `ElementSet` so a cost rollup composes with any selection — a cost-per-discipline, a cost-per-level, or a cost-of-the-clash-result set is one element-set the rollup folds over, never a per-subject calculator; the hierarchical rollup is one DuckDB `GROUP BY ROLLUP` over the classification ltree path so the project/building/level/class cost breakdown folds in one query, never a recursive hand-fold; the rate base pins the catalog edition content key so a re-price re-runs the same rollup against a new rate base and the cost delta is the difference of two content-addressed rollups; the line item's quantity reads the federated entity's quantity property sets (areas, volumes, lengths, counts the IFC `IfcElementQuantity` or a measured geometry carries) so the takeoff rides the federated graph, never a per-entity quantity column; the rollup export rides the analytical lane's parquet export (`data-lanes#ANALYTICAL_LANE`) so a cost report exports through the one tabular path.

```csharp signature
public sealed record CostCode(
    string ClassificationPath,
    string Code,
    double Rate,
    string Unit,
    string QuantityMeasure,
    string Formula,
    UInt128 RateBaseEdition);

public readonly record struct CostLineItem(
    Guid Entity,
    string ClassificationPath,
    string CostCode,
    double Quantity,
    double Rate,
    double Amount,
    string Unit,
    Instant At);

public static class CostRollup {
    public static CostLineItem Evaluate(FederatedEntity entity, CostCode code, Func<FederatedEntity, string, double> quantity) {
        var measured = quantity(entity, code.QuantityMeasure);
        return new CostLineItem(
            entity.Identity.Origin, code.ClassificationPath, code.Code,
            measured, code.Rate, measured * code.Rate, code.Unit, entity.At);
    }

    public static string RollupSql(ElementSet subject, string formula) =>
        $"""
        SELECT classification_path, cost_code,
               SUM({formula}) AS amount,
               SUM(quantity) AS quantity, unit
        FROM cost_line_item
        WHERE entity = ANY($keys)
        GROUP BY ROLLUP (classification_path), cost_code, unit
        ORDER BY classification_path
        """;

    public static IO<Seq<(string Path, double Amount)>> Rollup(
        DuckDBConnection lane,
        ElementSet subject,
        Seq<CostLineItem> lines,
        Func<DuckDBConnection, string, Seq<UInt128>, IO<Seq<(string Path, double Amount)>>> query) =>
        query(lane, RollupSql(subject, "quantity * rate"), subject.Keys);

    public static double Reprice(CostLineItem line, double newRate) => line.Quantity * newRate;
}
```

| [INDEX] | [CONCERN]          | [SURFACE]                                       | [LAW]                                             |
| :-----: | :----------------- | :---------------------------------------------- | :------------------------------------------------ |
|   [1]   | formula evaluation | DuckDB scalar expression over quantity columns  | data formula, never a hand-rolled evaluator       |
|   [2]   | takeoff subject    | `ElementSet` selection                          | composes with any element-set selection           |
|   [3]   | hierarchical rollup | DuckDB `GROUP BY ROLLUP` over ltree path        | one query for every hierarchy level               |
|   [4]   | rate base          | content-addressed catalog edition               | re-price is a content-addressed rollup delta      |
|   [5]   | report export      | analytical-lane parquet export                  | one tabular path, never a second report pipeline  |

## [4]-[RESEARCH]

- [CLASSIFICATION_LTREE_LOAD]: the published Uniclass/OmniClass/MasterFormat code-table ingestion into the `ltree`-pathed catalog table — the canonical code-to-ltree-path transform per standard separator and the cross-standard mapping-table source, verified against a published edition before the catalog-load fence pins a code form.
- [COST_FORMULA_PUSHDOWN]: the DuckDB scalar-expression evaluation of a parametric cost formula over the quantity columns and the `GROUP BY ROLLUP` hierarchical fold over the ltree classification path — whether a parametric formula referencing multiple quantity measures lowers to one DuckDB expression and the ROLLUP groups correctly over the ltree hierarchy on the live in-process engine.
