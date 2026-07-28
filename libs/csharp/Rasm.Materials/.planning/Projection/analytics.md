# [MATERIALS_ANALYTICS]

MATERIALS declares its analytics datasets as WIRE and projects catalogue truth onto flat row streams; `Rasm.Persistence` admits both across the `[WIRE]: AnalyticsSchema` seam as the branch's one columnar custodian. Column types, residences, dialects, DDL, slots, and the serving plane all home at that custodian, so this page names no provider, no residence, and no admitted schema type.

Settled composition: Component, Properties, Appearance, and observability owners supply already-admitted rows and receipts, and `ProjectionContext` carries the projection instant, operation key, and tenancy every stream stamps. Every dataset is EVENT-TIME and declares `observed` filled from the frame, so the Series, Fleet, and Lake residences provision one declaration and no dataset resolves against a single residence shape. Every row stream is a parameterized pure fold reaching no ambient registry.

## [01]-[INDEX]

- [02]-[DATASET_WIRE]: `ColumnToken` transcribes the custodian's physical types and `DatasetWire` carries each dataset's spine and admission projection.
- [03]-[DATASET_ROSTER]: `MaterialsDatasets` declares the five `materials.<source>` datasets over one shared spine.
- [04]-[ROW_PROJECTION]: `PropertyColumn` tables every selector and `AnalyticsProjection` folds each registered input onto flat rows.

## [02]-[DATASET_WIRE]

- Owner: `ColumnToken` `[SmartEnum<string>]` — the producer's spelling of the custodian's closed physical-type vocabulary; `DatasetColumn` — one named, typed, nullability-carrying column; `DatasetWire` — one dataset declaration carrying its key, its columns, its temporal spine, and the argument projection the custodian's admission gate consumes.
- Entry: `DatasetWire.Admission` is the whole crossing — the composing root reads it off each roster row and hands it to the custodian's one schema gate, which proves every key, time, and measure name against the columns before a statement composes.
- Auto: `Time` is a plain column name rather than an option because this family is EVENT-TIME whole under the branch analytics ruling — a catalogue row carries no version key, so `observed` is what separates two projections of a changed catalogue and a capacity check owns the instant it ran; a Materials dataset without a declared instant is therefore unrepresentable rather than optional, and `Measure` stays optional because a rollup is meaningful only where one numeric column carries the dataset's whole magnitude.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new physical type is one `ColumnToken` row transcribed with the custodian's own row; a new dataset is one `DatasetWire` value at `[03]` with its row record and fold at `[04]`.
- Boundary: `ColumnToken` keys transcribe the custodian's vocabulary structurally because peers at one stratum never reference each other and no compiler spans the seam — a token this roster spells and the custodian never admits refuses at that gate rather than provisioning a column no dialect can render. Naming disambiguates at the source: the custodian owns `ColumnType`, `ColumnRow`, and `AnalyticsSchema`, and this page's declarations never wear those names.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------------------
// Structural transcription of the custodian's physical-type vocabulary — the keys are the wire, and a row
// added there lands here in the same change or its datasets refuse at the admission gate.
[SmartEnum<string>]
public sealed partial class ColumnToken {
    public static readonly ColumnToken Utf8 = new("utf8");
    public static readonly ColumnToken Float64 = new("float64");
    public static readonly ColumnToken Int64 = new("int64");
    public static readonly ColumnToken Bool = new("bool");
    public static readonly ColumnToken Date = new("date32");
    public static readonly ColumnToken Timestamp = new("timestamp-ns");
    public static readonly ColumnToken KeyHex = new("fixed-hex128");
}

// --- [MODELS] -----------------------------------------------------------------------------------------
public readonly record struct DatasetColumn(string Name, ColumnToken Token, bool Nullable) {
    public (string Name, string Type, bool Nullable) Wire => (Name, Token.Key, Nullable);
}

public sealed record DatasetWire(
    string Dataset, Seq<string> Key, Seq<DatasetColumn> Columns, string Time, Option<string> Measure) {
    public (string Dataset, Seq<(string Name, string Type, bool Nullable)> Columns,
        Seq<string> Key, Option<string> Time, Option<string> Measure) Admission =>
        (Dataset, Columns.Map(static column => column.Wire), Key, Some(Time), Measure);
}
```

## [03]-[DATASET_ROSTER]

- Owner: `MaterialsDatasets` — the dataset registry: `materials.component-rows` (catalogue identity, family and class discriminants, section pin, substance and appearance keys, IFC binding), `materials.property-rows` (admitted scalar and dimensioned property columns with their UCUM unit, evidence source, and expiry), `materials.sustainability` (per-stage GWP, resource fractions, classification, evidence source and expiry), `materials.library-summary` (the seam appearance scalars behind the content key), `materials.capacity-checks` (per-check verdict evidence off the fact stream).
- Entry: `MaterialsDatasets.All` is the roster the composing root enumerates; each declaration pairs one `[04]` row record and fold.
- Auto: identity and provenance ride as columns — classification system and code with evidence source and calendar expiry on the property and sustainability rows, the content-derived appearance key on library rows — so audit queries filter and expiry-screen without joining back into object graphs; `observed` trails every column list, so every declaration reads identity, then payload, then spine; the residence derives its own sort key from `Key` and `Time`, never from declaration order.
- Packages: LanguageExt.Core.
- Growth: a new dataset is one declaration, one row record, and one fold; a new column is one `DatasetColumn` with its field on the owning row record.
- Boundary: declaration truth and row truth stay co-located, so each dataset edit carries its matching row field and projection expression. `gwp` and `elapsed_s` are the two measures the family declares, because summing a mixed-unit long-form property column or a colour channel states a magnitude neither carries.

```csharp signature
public static class MaterialsDatasets {
    public static readonly DatasetWire ComponentRows = new("materials.component-rows", Key: Seq("component"), Seq(
        new DatasetColumn("component", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("family", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("class", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("sectioned", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("substance", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("appearance", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("ifc_entity", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("predefined", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Time: "observed", Measure: None);

    public static readonly DatasetWire PropertyRows = new("materials.property-rows", Key: Seq("material", "property"), Seq(
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("property", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("unit", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("central", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("evidence_source", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("evidence_expiry", ColumnToken.Date, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Time: "observed", Measure: None);

    public static readonly DatasetWire Sustainability = new("materials.sustainability", Key: Seq("material", "stage"), Seq(
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("basis", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("stage", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("gwp", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("recycled", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("recovery", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("classification_system", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("classification_code", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("evidence_source", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("evidence_expiry", ColumnToken.Date, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Time: "observed", Measure: "gwp");

    public static readonly DatasetWire LibrarySummary = new("materials.library-summary", Key: Seq("material"), Seq(
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("appearance_key", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("base_r", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("base_g", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("base_b", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("metallic", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("roughness", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("opacity", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("transmissive", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Time: "observed", Measure: None);

    public static readonly DatasetWire CapacityChecks = new("materials.capacity-checks", Key: Seq("op", "kind"), Seq(
        new DatasetColumn("op", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("kind", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("governing", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("adequate", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("utilisation", ColumnToken.Float64, Nullable: true),
        new DatasetColumn("elapsed_s", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Time: "observed", Measure: "elapsed_s");

    public static readonly Seq<DatasetWire> All =
        Seq(ComponentRows, PropertyRows, Sustainability, LibrarySummary, CapacityChecks);
}
```

## [04]-[ROW_PROJECTION]

- Owner: `AnalyticsProjection` — the typed row records and the total folds from registered rows and typed receipts onto flat row streams; `PropertyColumn` — the selector table one scalar or dimensioned property occupies per row.
- Entry: `Components` folds catalogue rows; `Properties` folds per-material property rows through the admitted `PropertyColumn` table; `Sustainability` folds one row per lifecycle stage; `Library` traverses material keys through an injected admitted appearance lookup; `Capacity` chooses capacity facts off the observability stream. Every fold takes `ProjectionContext` and stamps `frame.At` onto its rows.
- Auto: a dimensioned selector reads its SI accessor off the quantity the `Published` carrier holds, so the magnitude and the UCUM unit it is stated in derive from one owner and no fold re-scales; folds are total over their registered inputs — an unregistered library key aborts the library fold typed rather than emitting a partial dataset.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, UnitsNet, BCL inbox.
- Growth: a new scalar or dimensioned property is one `PropertyColumn` row carrying its unit and its selector; a new dataset fold is one row record and one member beside its declaration.
- Boundary: ingress is parameterized — every fold takes its registered input and its frame as arguments and reads no ambient registry; egress is a row `Seq` the custodian batches, so buffer custody, batch sizing, and dataset writes never enter this page.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------------------
public readonly record struct ComponentAnalyticsRow(
    string Component, string Family, string Class, bool Sectioned, string Substance, string Appearance,
    string IfcEntity, string Predefined, Instant Observed);

public readonly record struct PropertyAnalyticsRow(
    string Material, string Property, string Unit, double Central,
    string EvidenceSource, Option<LocalDate> EvidenceExpiry, Instant Observed);

public readonly record struct SustainabilityAnalyticsRow(
    string Material, string Basis, int Stage, double Gwp, double Recycled, double Recovery,
    Option<string> ClassificationSystem, Option<string> ClassificationCode,
    string EvidenceSource, Option<LocalDate> EvidenceExpiry, Instant Observed);

public readonly record struct LibrarySummaryRow(
    string Material, string AppearanceKey, double BaseR, double BaseG, double BaseB,
    double Metallic, double Roughness, double Opacity, bool Transmissive, Instant Observed);

public readonly record struct CapacityCheckRow(
    string Op, string Kind, string Governing, bool Adequate, Option<double> Utilisation,
    double ElapsedSeconds, Instant Observed);

// --- [OPERATIONS] -------------------------------------------------------------------------------------
// One selector table over both carriers: the scalar rows read the raw central and the dimensioned rows read
// their quantity's own SI accessor, so the long-form `central` column is self-describing through `Unit` and
// a reader never infers a scale from the property name.
public sealed record PropertyColumn(string Property, string Unit, Func<MaterialPropertyRow, double> Central) {
    public static readonly Seq<PropertyColumn> Rows = Seq(
        new PropertyColumn("poisson", "1", static row => row.Poisson.Central),
        new PropertyColumn("expansion_per_k", "/K", static row => row.Expansion.Central),
        new PropertyColumn("vapour_mu", "1", static row => row.VapourMu.Central),
        new PropertyColumn("density", "kg/m3", static row => row.Density.Central.KilogramsPerCubicMeter),
        new PropertyColumn("conductivity", "W/(m.K)", static row => row.Conductivity.Central.WattsPerMeterKelvin),
        new PropertyColumn("specific_heat", "J/(kg.K)", static row => row.SpecificHeat.Central.JoulesPerKilogramKelvin),
        new PropertyColumn("u_value", "W/(m2.K)", static row => row.UValue.Central.WattsPerSquareMeterKelvin));
}

public static class AnalyticsProjection {
    public static Seq<ComponentAnalyticsRow> Components(Seq<ComponentRow> rows, ProjectionContext frame) =>
        rows.Map(row => new ComponentAnalyticsRow(
            row.Item.Designation.Value, row.Item.Family.Key, row.Item.Class.Key, row.Sectioned,
            row.Item.SubstanceId.Value, row.Item.AppearanceId.Value, row.Item.IfcEntity,
            row.Item.PredefinedToken, frame.At));

    public static Seq<PropertyAnalyticsRow> Properties(
        Seq<(MaterialId Id, MaterialPropertyRow Row)> rows, ProjectionContext frame) =>
        rows.Bind(entry => PropertyColumn.Rows.Map(column =>
            new PropertyAnalyticsRow(entry.Id.Value, column.Property, column.Unit, column.Central(entry.Row),
                entry.Row.Evidence.Source, entry.Row.Evidence.ValidUntil, frame.At)));

    public static Seq<SustainabilityAnalyticsRow> Sustainability(
        Seq<(MaterialId Id, SustainabilityRow Row)> rows, ProjectionContext frame) =>
        rows.Bind(entry => toSeq(Published.Centrals(entry.Row.StageGwp).ToArray())
            .Map((gwp, stage) => new SustainabilityAnalyticsRow(
                entry.Id.Value, entry.Row.EnvironmentalBasis, stage, gwp,
                entry.Row.Recycled.Central, entry.Row.Recovery.Central,
                entry.Row.Classification.Map(static c => c.System),
                entry.Row.Classification.Map(static c => c.Code),
                entry.Row.Evidence.Source, entry.Row.Evidence.ValidUntil, frame.At)));

    public static Fin<Seq<LibrarySummaryRow>> Library(
        Seq<MaterialId> materials, ProjectionContext frame,
        Func<MaterialId, Op, Fin<AppearanceSummary>> lookup) =>
        materials.TraverseM(id => lookup(id, frame.Key).Map(summary =>
            new LibrarySummaryRow(
                id.Value, $"{summary.AppearanceKey:x32}", summary.BaseColorR, summary.BaseColorG,
                summary.BaseColorB, summary.Metallic, summary.Roughness, summary.Opacity,
                summary.Transmissive, frame.At))).As();

    public static Seq<CapacityCheckRow> Capacity(Seq<MaterialsFact> facts, ProjectionContext frame) =>
        facts.Choose(fact => fact is MaterialsFact.CapacityCheck check
            ? Some(new CapacityCheckRow(
                check.Key.ToString(), check.Receipt.Kind, check.Verdict.Governing.Key,
                check.Verdict.Adequate,
                check.Verdict is Utilisation.Bounded bounded ? Some(bounded.Value) : Option<double>.None,
                check.Elapsed.TotalSeconds, frame.At))
            : Option<CapacityCheckRow>.None);
}
```

## [05]-[RESEARCH]

(none)
