# [MATERIALS_ANALYTICS]

COLUMNAR analytics egress pairs each Materials dataset with a bounded `ColumnType` schema and one pure projection fold. Composite key rows identify every emitted grain; dataset writers, lake custody, and `store.materials.<verb>` slots stay on the Persistence plane.

Settled composition: Component, Properties, Appearance, and observability owners supply already-admitted rows and receipts. Every row stream is a parameterized pure fold; no projection reaches an ambient registry or stores a second truth.

## [01]-[INDEX]

- [02]-[COLUMN_VOCABULARY]: the `ColumnType` rows, `ColumnRow` shape, and `AnalyticsSchema` owner.
- [03]-[SCHEMA_ROWS]: the five dataset schemas under the `materials.<source>` grammar.
- [04]-[PROJECTION_FOLDS]: the `AnalyticsProjection` typed row records and their total folds.

## [02]-[COLUMN_VOCABULARY]

- Owner: `ColumnType` `[SmartEnum<string>]` — the bounded column-type vocabulary whose keys are the neutral tokens the store plane maps onto its columnar types; `ColumnRow` — one named, typed, nullability-carrying column; `AnalyticsSchema` — one dataset with its ordered key columns and column rows.
- Entry: a schema is a value — the store plane reads `Dataset`, `Key`, and `Columns` and mints the physical dataset; a consumer query reads the same rows as its column contract.
- Auto: the dataset name carries the `materials.<source>` grammar, so slot derivation on the store side is one string composition per verb.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new column type is one `ColumnType` row; a new dataset is one `AnalyticsSchema` value at `[03]` with its projection fold at `[04]`.
- Boundary: the vocabulary is storage-neutral — Arrow, parquet, and DuckDB spellings live on the Persistence catalogs, and a provider type never appears here.

```csharp signature
[SmartEnum<string>]
public sealed partial class ColumnType {
    public static readonly ColumnType Utf8 = new("utf8");
    public static readonly ColumnType Float64 = new("float64");
    public static readonly ColumnType Int64 = new("int64");
    public static readonly ColumnType Bool = new("bool");
    public static readonly ColumnType Date = new("date32");
    public static readonly ColumnType KeyHex = new("fixed-hex128");
}

public readonly record struct ColumnRow(string Name, ColumnType Type, bool Nullable);

public sealed record AnalyticsSchema(string Dataset, Seq<string> Key, Seq<ColumnRow> Columns);
```

## [03]-[SCHEMA_ROWS]

- Owner: `MaterialsSchemas` — the dataset registry: `materials.component-rows` (catalogue identity, family and class discriminants, section pin, substance and appearance keys, IFC binding), `materials.property-rows` (admitted scalar property columns with evidence source and expiry), `materials.sustainability` (per-stage GWP, resource fractions, classification, evidence source and expiry), `materials.library-summary` (the seam appearance scalars behind the content key), `materials.capacity-checks` (per-check verdict evidence off the fact stream).
- Entry: `MaterialsSchemas.All` is the roster the store plane enumerates; each schema pairs one `[04]` row record and fold.
- Auto: identity and provenance ride as columns — classification system and code with `PropertyEvidence` source and calendar expiry on the property and sustainability rows, the content-derived appearance key on library rows — so audit queries filter and expiry-screen without joining back into object graphs.
- Packages: LanguageExt.Core.
- Growth: a new dataset is one schema value, one row record, and one fold; a new column is one `ColumnRow` with its field on the owning row record.
- Boundary: schema truth and row truth stay co-located; each schema edit includes the matching row field and projection expression.

```csharp signature
public static class MaterialsSchemas {
    public static readonly AnalyticsSchema ComponentRows = new("materials.component-rows", Key: Seq("component"), Seq(
        new ColumnRow("component", ColumnType.Utf8, Nullable: false),
        new ColumnRow("family", ColumnType.Utf8, Nullable: false),
        new ColumnRow("class", ColumnType.Utf8, Nullable: false),
        new ColumnRow("sectioned", ColumnType.Bool, Nullable: false),
        new ColumnRow("substance", ColumnType.Utf8, Nullable: false),
        new ColumnRow("appearance", ColumnType.Utf8, Nullable: false),
        new ColumnRow("ifc_entity", ColumnType.Utf8, Nullable: false),
        new ColumnRow("predefined", ColumnType.Utf8, Nullable: false)));

    public static readonly AnalyticsSchema PropertyRows = new("materials.property-rows", Key: Seq("material", "property"), Seq(
        new ColumnRow("material", ColumnType.Utf8, Nullable: false),
        new ColumnRow("property", ColumnType.Utf8, Nullable: false),
        new ColumnRow("central", ColumnType.Float64, Nullable: false),
        new ColumnRow("evidence_source", ColumnType.Utf8, Nullable: false),
        new ColumnRow("evidence_expiry", ColumnType.Date, Nullable: true)));

    public static readonly AnalyticsSchema Sustainability = new("materials.sustainability", Key: Seq("material", "stage"), Seq(
        new ColumnRow("material", ColumnType.Utf8, Nullable: false),
        new ColumnRow("basis", ColumnType.Utf8, Nullable: false),
        new ColumnRow("stage", ColumnType.Int64, Nullable: false),
        new ColumnRow("gwp", ColumnType.Float64, Nullable: false),
        new ColumnRow("recycled", ColumnType.Float64, Nullable: false),
        new ColumnRow("recovery", ColumnType.Float64, Nullable: false),
        new ColumnRow("classification_system", ColumnType.Utf8, Nullable: true),
        new ColumnRow("classification_code", ColumnType.Utf8, Nullable: true),
        new ColumnRow("evidence_source", ColumnType.Utf8, Nullable: false),
        new ColumnRow("evidence_expiry", ColumnType.Date, Nullable: true)));

    public static readonly AnalyticsSchema LibrarySummary = new("materials.library-summary", Key: Seq("material"), Seq(
        new ColumnRow("material", ColumnType.Utf8, Nullable: false),
        new ColumnRow("appearance_key", ColumnType.KeyHex, Nullable: false),
        new ColumnRow("base_r", ColumnType.Float64, Nullable: false),
        new ColumnRow("base_g", ColumnType.Float64, Nullable: false),
        new ColumnRow("base_b", ColumnType.Float64, Nullable: false),
        new ColumnRow("metallic", ColumnType.Float64, Nullable: false),
        new ColumnRow("roughness", ColumnType.Float64, Nullable: false),
        new ColumnRow("opacity", ColumnType.Float64, Nullable: false),
        new ColumnRow("transmissive", ColumnType.Bool, Nullable: false)));

    public static readonly AnalyticsSchema CapacityChecks = new("materials.capacity-checks", Key: Seq("op", "kind"), Seq(
        new ColumnRow("op", ColumnType.Utf8, Nullable: false),
        new ColumnRow("kind", ColumnType.Utf8, Nullable: false),
        new ColumnRow("governing", ColumnType.Utf8, Nullable: false),
        new ColumnRow("adequate", ColumnType.Bool, Nullable: false),
        new ColumnRow("utilisation", ColumnType.Float64, Nullable: true),
        new ColumnRow("elapsed_s", ColumnType.Float64, Nullable: false)));

    public static readonly Seq<AnalyticsSchema> All =
        Seq(ComponentRows, PropertyRows, Sustainability, LibrarySummary, CapacityChecks);
}
```

## [04]-[PROJECTION_FOLDS]

- Owner: `AnalyticsProjection` — the typed row records and the total folds from registered rows and typed receipts onto flat row streams; `PropertyColumn` — the selector table one scalar property occupies per row.
- Entry: `Components` folds catalogue rows; `Properties` folds per-material property rows through the admitted `PropertyColumn` table; `Sustainability` folds one row per lifecycle stage; `Library` traverses material keys through an injected admitted appearance lookup; `Capacity` chooses capacity facts off the observability stream.
- Auto: folds are total over their registered inputs — an unregistered library key aborts the library fold typed rather than emitting a partial dataset; the capacity fold is the one flat row projection the eventual instrument arm mirrors.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new scalar property is one `PropertyColumn` row; a new dataset fold is one row record and one member beside its schema.
- Boundary: ingress is parameterized — every fold takes its registered input as an argument and reads no ambient registry; egress is a row `Seq` the store plane batches, so buffer custody, batch sizing, and dataset writes never enter this page.

```csharp signature
public readonly record struct ComponentAnalyticsRow(
    string Component, string Family, string Class, bool Sectioned, string Substance, string Appearance, string IfcEntity, string Predefined);

public readonly record struct PropertyAnalyticsRow(
    string Material, string Property, double Central, string EvidenceSource, Option<LocalDate> EvidenceExpiry);

public readonly record struct SustainabilityAnalyticsRow(
    string Material, string Basis, int Stage, double Gwp, double Recycled, double Recovery,
    Option<string> ClassificationSystem, Option<string> ClassificationCode,
    string EvidenceSource, Option<LocalDate> EvidenceExpiry);

public readonly record struct LibrarySummaryRow(
    string Material, string AppearanceKey, double BaseR, double BaseG, double BaseB,
    double Metallic, double Roughness, double Opacity, bool Transmissive);

public readonly record struct CapacityCheckRow(
    string Op, string Kind, string Governing, bool Adequate, Option<double> Utilisation, double ElapsedSeconds);

// Dimensionless selectors are settled. Dimensioned selectors stay behind [UNITSNET_ANALYTICS_SELECTORS]
// until both catalogue tiers admit their exact SI property spellings.
public sealed record PropertyColumn(string Property, Func<MaterialPropertyRow, double> Central) {
    public static readonly Seq<PropertyColumn> Rows = Seq(
        new PropertyColumn("poisson", static row => row.Poisson.Central),
        new PropertyColumn("expansion_per_k", static row => row.Expansion.Central),
        new PropertyColumn("vapour_mu", static row => row.VapourMu.Central));
}

public static class AnalyticsProjection {
    public static Seq<ComponentAnalyticsRow> Components(Seq<ComponentRow> rows) =>
        rows.Map(static row => new ComponentAnalyticsRow(
            row.Item.Designation.Value, row.Item.Family.Key, row.Item.Class.Key, row.Sectioned,
            row.Item.SubstanceId.Value, row.Item.AppearanceId.Value, row.Item.IfcEntity, row.Item.PredefinedToken));

    public static Seq<PropertyAnalyticsRow> Properties(Seq<(MaterialId Id, MaterialPropertyRow Row)> rows) =>
        rows.Bind(entry => PropertyColumn.Rows.Map(column =>
            new PropertyAnalyticsRow(entry.Id.Value, column.Property, column.Central(entry.Row),
                entry.Row.Evidence.Source, entry.Row.Evidence.ValidUntil)));

    public static Seq<SustainabilityAnalyticsRow> Sustainability(Seq<(MaterialId Id, SustainabilityRow Row)> rows) =>
        rows.Bind(entry => toSeq(Published.Centrals(entry.Row.StageGwp).ToArray()
            .Select((gwp, stage) => new SustainabilityAnalyticsRow(
                entry.Id.Value, entry.Row.EnvironmentalBasis, stage, gwp,
                entry.Row.Recycled.Central, entry.Row.Recovery.Central,
                entry.Row.Classification.Map(static c => c.System),
                entry.Row.Classification.Map(static c => c.Code),
                entry.Row.Evidence.Source, entry.Row.Evidence.ValidUntil))));

    public static Fin<Seq<LibrarySummaryRow>> Library(
        Seq<MaterialId> materials, Op key, Func<MaterialId, Op, Fin<AppearanceSummary>> lookup) =>
        materials.TraverseM(id => lookup(id, key).Map(summary =>
            new LibrarySummaryRow(
                id.Value, $"{summary.AppearanceKey:x32}", summary.BaseColorR, summary.BaseColorG, summary.BaseColorB,
                summary.Metallic, summary.Roughness, summary.Opacity, summary.Transmissive))).As();

    public static Seq<CapacityCheckRow> Capacity(Seq<MaterialsFact> facts) =>
        facts.Choose(static fact => fact is MaterialsFact.CapacityCheck check
            ? Some(new CapacityCheckRow(
                check.Key.ToString(), CapacityKindOf(check.Receipt), check.Verdict.Governing.Key,
                check.Verdict.Adequate,
                check.Verdict is Utilisation.Bounded bounded ? Some(bounded.Value) : Option<double>.None,
                check.Elapsed.TotalSeconds))
            : Option<CapacityCheckRow>.None);

    static string CapacityKindOf(CapacityReceipt receipt) => receipt.Switch(
        steel: static _ => nameof(CapacityReceipt.Steel),
        timber: static _ => nameof(CapacityReceipt.Timber),
        masonry: static _ => nameof(CapacityReceipt.Masonry),
        reinforcedMasonry: static _ => nameof(CapacityReceipt.ReinforcedMasonry),
        glass: static _ => nameof(CapacityReceipt.Glass),
        weld: static _ => nameof(CapacityReceipt.Weld),
        adhesive: static _ => nameof(CapacityReceipt.Adhesive),
        stud: static _ => nameof(CapacityReceipt.Stud),
        connector: static _ => nameof(CapacityReceipt.Connector));
}
```

## [05]-[RESEARCH]

- [UNITSNET_ANALYTICS_SELECTORS]-[BLOCKED]: which exact SI properties expose density, conductivity, specific heat, and U-value from each central quantity; `libs/csharp/Rasm.Materials/.api/api-unitsnet.md`, then `libs/csharp/.api/api-unitsnet.md`, against `libs/csharp/Rasm.Materials/.planning/Properties/measure.md`.
