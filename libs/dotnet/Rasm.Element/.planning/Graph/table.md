# [ELEMENT_TABLE]

One `Tabulate` fold flattens a frozen `ElementGraph` into ten typed row families — the columnar egress every QTO, cost, coverage, commissioning, and dashboard consumer reads without re-folding the graph. `TableRow` closes the family as a `[Union]` whose case IS the dataset, `TableFamily` carries each dataset's column declaration beside its key, temporal spine, and rollup measure, and every row carries the snapshot `ContentAddress` so an analytic answer pins the exact model version it was computed over. Reading a row family is SQL; re-deriving one from the graph is the deleted form.

`Rasm.Persistence` owns the columnar plane whole — writers, storage, provisioning, serving transport — so this page hands typed rows and a wire schema across the `[WIRE]: AnalyticsSchema` contract and names no storage type, dialect token, Arrow field, or landing verb. `TableType` mirrors the custodian's neutral token roster, `PropertyValue` carries every cell, and `Bake` supplies each element row so type→occurrence inheritance applies once. Every family declares its temporal CATEGORY — `element.assessments` stamps the instant its work ran, and every snapshot family is landing-timed.

## [01]-[INDEX]

- [02]-[ROW_FAMILIES]: `TableRow` closes the ten-case dataset family beside its `Cells` projection, its `Family` token, and the `TableSnapshot` product carrying the graph address.
- [03]-[DATASET_ROSTER]: `TableType` and `TableColumn` declare the neutral token roster, `TableDeclaration` is the producer-neutral dataset self-description carrying `Wire`/`Admission`/`Conforms`, `TableFamily` rosters the element-owned datasets as wrapped declarations, and `TableBatch` — keyed on the declaration — crosses the contract for element and foreign producers alike.
- [04]-[TABULATE_FOLD]: `GraphTable.Tabulate` folds a frozen snapshot under its root scope through the per-family row projections it composes.

## [02]-[ROW_FAMILIES]

- Owner: `TableRow` — the closed `[Union]` whose ten cases ARE the ten datasets, each carrying its flat column payload and the snapshot address, and each owning the ordered `Cells` projection its dataset's column declaration reads; `TableSnapshot` — the fold product pairing the graph `ContentAddress` with every emitted row.
- Cases: `Classification` (one co-applied standard reference — the system, code, and edition triple keying it, beside the source, edition date, and title annotations; the PRIMARY entity-class triple stays denormalized on the object row because it keys that grain, so this family carries the secondary refs the object row cannot hold) · `Object` (one baked element — identity, kind, external id, the primary classification triple, predefined token, name, tag, type binding, container, containment depth, appearance key, part count) · `Property` (one bag entry — set, name, value kind, rendered text, the measure magnitude and quantity type where the entry is measured, source rank, inheritance mode) · `Quantity` (one quantity entry — set, name, quantity type, SI magnitude, canonical unit, the seven `Dimension` exponents, the optional uncertainty band) · `Material` (one material binding — material key, composition and usage tokens, the inheritance flag, layer count and buildup depth, the profile reference and baked section area) · `Section` (one baked profile-set section — the whole S-E1 algebra: profile key, LTB route token, the nineteen SI design columns, mono-symmetry, centroid, and the optional forming-shape witness — where the material row carries only the takeoff area) · `Edge` (one relationship — the edge content address, neutral kind, sub-kind, endpoints, realizing intermediary, nest ordinal, passthrough wire name, member count, containment predicate) · `Assessment` (one computed assessment — discipline, route, input key, outcome with its three behavior columns, provenance, the typed diagnostic, the result blob, the dependency and result counts) · `Observation` (one measured series — sensor deployment, observed aspect, quantity triple, sampling algebra, cadence, window bounds, chunk and sample counts, the graded census shares, the four summary magnitudes, the instrument audit) · `Coverage` (one raster band — raster key, coverage kind, CRS identity, the twelve index-to-world affine coefficients and the three-axis census, band index with its role, sample type, units, decode scale pair, pyramid depth, timeline depth, uncompressed byte length).
- Entry: `row.Family` projects the dataset token through the generated `Map` over precomputed rows; `row.Cells` projects the ordered `Option<PropertyValue>` sequence the family's column declaration binds positionally, an absent cell reading `None` so a nullable column carries real absence rather than a sentinel; `TableSnapshot.Batches(key)` admits the whole row set through `TableFamily.Admit` and then groups every row under its family in roster order, the one value crossing the boundary.
- Auto: declaration order of a case's payload IS the column order its `Cells` arm emits and the order `TableFamily` declares, so a column edit and its field edit are one edit at one site; the event-time payload closes on its own instant, so `element.assessments` trails on the column its `Spine` names; the private lifts (`Text`/`Real`/`Whole`/`Big`/`Flag`/`Moment`/`Day`) are the only cell constructors, so every cell's `PropertyValue` case is fixed at the projection rather than chosen per column; a content key — the snapshot address, an edge address, an assessment input key, a result blob, an appearance key, a raster key — crosses as `Text` through kernel `ContentHash.Hex`, the canonical x32 spelling `Projection/address#CONTENT_ADDRESS` composes as the cross-runtime wire form.
- Output: `TableSnapshot.Rows` is the flat typed read a consumer folds directly; `TableSnapshot.Batches(key)` is the admitted, erased per-family cell projection the columnar custodian lands.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` with the generated total `Switch`/`Map`), LanguageExt.Core (`Seq`/`Option`/`Map`), NodaTime (`Instant` the assessment and window stamps, `LocalDate` the calibration stamp), `Rasm` (the kernel `Op` and `ContentHash.Hex` content-key spelling), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.ToValue()` raw-key projection), BCL inbox (`BigInteger` the whole-number cell payload).
- Growth: a new dataset is one `TableRow` case declaring its temporal category, with its `Cells` arm, its `TableFamily` row, and its projection in `[04]`; a new column is one payload field with its cell in the same arm and its `TableColumn` in the same row; never a sibling row type beside the union and never a dataset whose columns live apart from its payload.
- Boundary: `TableRow.Object` shadows the simple name `Object` inside the union body exactly as `Node.Object` does at its own owner, and `TableRow.Classification` shadows the shared classification type the same way, so every construction spells the nested case and the generated arms read `@object:` and `classification:`; the row is a DERIVED projection carrying zero authority — the graph and its delta stream own truth, a dropped dataset rebuilds by re-tabulating, and writing a table row back into the graph is the deleted inversion; `Cells` carries no storage type, so a physical width, a nullability dialect, and a partition expression stay the custodian's; heavy payloads never enter a row — geometry, result artifacts, and raster coverages ride their content keys, which cross as text.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Numerics;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Element.Assessment;
using Rasm.Element.Composition;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Graph;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record TableRow {
    private TableRow() { }

    public sealed record Object(
        string Snapshot, string Element, string Kind, Option<string> ExternalId,
        string ClassificationSystem, string ClassificationCode, string ClassificationEdition,
        string Predefined, string Name, string Tag, Option<string> TypeId,
        Option<string> Container, int ContainmentDepth, Option<string> Appearance, int PartCount) : TableRow;

    public sealed record Classification(
        string Snapshot, string Element, string System, string Code, string Edition,
        Option<string> Source, Option<LocalDate> EditionDate, Option<string> Title) : TableRow;

    public sealed record Property(
        string Snapshot, string Element, string SetName, string Name, string Kind, string Rendered,
        Option<double> Si, Option<string> QuantityType, string Source, string Inheritance) : TableRow;

    public sealed record Quantity(
        string Snapshot, string Element, string SetName, string Name, string QuantityType,
        double Si, Option<string> Unit,
        int DimLength, int DimMass, int DimTime, int DimCurrent, int DimTemperature, int DimAmount, int DimLuminous,
        Option<string> Uncertainty, Option<double> LowerSi, Option<double> UpperSi) : TableRow;

    public sealed record Material(
        string Snapshot, string Element, string MaterialKey, string Composition, string Usage, bool Inherited,
        int LayerCount, Option<double> TotalThicknessSi,
        Option<string> ProfileStandard, Option<string> ProfileDesignation, Option<double> SectionAreaSi) : TableRow;

    public sealed record Section(
        string Snapshot, string Element, string MaterialKey,
        string ProfileStandard, string ProfileDesignation, string LtbRoute,
        double AreaSi, double IyySi, double IzzSi, double JSi, double IwSi,
        double WelySi, double WelzSi, double WplySi, double WplzSi,
        double AvYSi, double AvZSi, double RadiusMajorSi, double RadiusMinorSi,
        double DepthSi, double WidthSi, double HeatedPerimeterSi, double AxisDistanceSi,
        double ShearCentreYSi, double ShearCentreZSi, double MonosymmetryFactor,
        double CentroidX, double CentroidY, double CentroidZ,
        Option<int> FormVertexCount, Option<int> FormCurvedEdges,
        Option<double> FormRadialRatio, Option<double> FormPerimeterSi) : TableRow;

    public sealed record Edge(
        string Snapshot, string EdgeAddress, string Kind, Option<string> SubKind,
        string Relating, string Related, Option<string> Realizing, Option<int> Ordinal,
        Option<string> WireName, int MemberCount, bool Containment) : TableRow;

    public sealed record Assessment(
        string Snapshot, string Element, string Discipline, string Route, string InputKey,
        string Outcome, bool Usable, bool Terminal, bool Dispatchable,
        double ElapsedSeconds, string Author, string Tool, string Version,
        Option<string> DiagnosticPhase, Option<string> DiagnosticKind, Option<string> DiagnosticMessage,
        Option<int> DiagnosticCode, Option<bool> Transient,
        Option<string> ResultSha256, Option<long> ResultBytes,
        int DependsOnCount, int ResultCount, Instant At) : TableRow;

    public sealed record Observation(
        string Snapshot, string Element, string Sensor, string Aspect, string QuantityType, string Unit,
        string Sampling, Option<double> CadenceSeconds, Instant WindowStart, Instant WindowEnd,
        int ChunkCount, int SampleCount, double SpanSeconds, int GradedSamples, int ConsumableSamples,
        Option<double> Completeness, Option<double> MinimumSi, Option<double> MaximumSi,
        Option<double> MeanSi, Option<double> TotalSi,
        string Manufacturer, string Model, string Serial, Option<LocalDate> CalibratedAt) : TableRow;

    public sealed record Coverage(
        string Snapshot, string Element, string RasterSha256, long RasterBytes, string Kind, string CrsResolution,
        Option<int> Epsg, string GeodeticDatum,
        Seq<double> Affine, int Columns, int Rows, int Layers,
        int BandIndex, string BandName, int SampleType, string Role, string Units,
        double Offset, double Scale, Option<double> NoData,
        int OverviewCount, long ByteLength) : TableRow;

    public TableFamily Family => Map(
        @object: TableFamily.Objects,
        classification: TableFamily.Classifications,
        property: TableFamily.Properties,
        quantity: TableFamily.Quantities,
        material: TableFamily.Materials,
        section: TableFamily.Sections,
        edge: TableFamily.Edges,
        assessment: TableFamily.Assessments,
        observation: TableFamily.Observations,
        coverage: TableFamily.Coverages);

    public Seq<Option<PropertyValue>> Cells => Switch(
        @object: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.Kind), Text(r.ExternalId),
            Text(r.ClassificationSystem), Text(r.ClassificationCode), Text(r.ClassificationEdition),
            Text(r.Predefined), Text(r.Name), Text(r.Tag), Text(r.TypeId),
            Text(r.Container), Whole(r.ContainmentDepth), Text(r.Appearance), Whole(r.PartCount)),
        classification: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.System), Text(r.Code), Text(r.Edition),
            Text(r.Source), Day(r.EditionDate), Text(r.Title)),
        property: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.SetName), Text(r.Name), Text(r.Kind), Text(r.Rendered),
            Real(r.Si), Text(r.QuantityType), Text(r.Source), Text(r.Inheritance)),
        quantity: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.SetName), Text(r.Name), Text(r.QuantityType),
            Real(r.Si), Text(r.Unit),
            Whole(r.DimLength), Whole(r.DimMass), Whole(r.DimTime), Whole(r.DimCurrent),
            Whole(r.DimTemperature), Whole(r.DimAmount), Whole(r.DimLuminous),
            Text(r.Uncertainty), Real(r.LowerSi), Real(r.UpperSi)),
        material: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.MaterialKey), Text(r.Composition), Text(r.Usage),
            Flag(r.Inherited), Whole(r.LayerCount), Real(r.TotalThicknessSi),
            Text(r.ProfileStandard), Text(r.ProfileDesignation), Real(r.SectionAreaSi)),
        section: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.MaterialKey),
            Text(r.ProfileStandard), Text(r.ProfileDesignation), Text(r.LtbRoute),
            Real(r.AreaSi), Real(r.IyySi), Real(r.IzzSi), Real(r.JSi), Real(r.IwSi),
            Real(r.WelySi), Real(r.WelzSi), Real(r.WplySi), Real(r.WplzSi),
            Real(r.AvYSi), Real(r.AvZSi), Real(r.RadiusMajorSi), Real(r.RadiusMinorSi),
            Real(r.DepthSi), Real(r.WidthSi), Real(r.HeatedPerimeterSi), Real(r.AxisDistanceSi),
            Real(r.ShearCentreYSi), Real(r.ShearCentreZSi), Real(r.MonosymmetryFactor),
            Real(r.CentroidX), Real(r.CentroidY), Real(r.CentroidZ),
            Whole(r.FormVertexCount), Whole(r.FormCurvedEdges),
            Real(r.FormRadialRatio), Real(r.FormPerimeterSi)),
        edge: static r => Seq(
            Text(r.Snapshot), Text(r.EdgeAddress), Text(r.Kind), Text(r.SubKind),
            Text(r.Relating), Text(r.Related), Text(r.Realizing), Whole(r.Ordinal),
            Text(r.WireName), Whole(r.MemberCount), Flag(r.Containment)),
        assessment: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.Discipline), Text(r.Route), Text(r.InputKey),
            Text(r.Outcome), Flag(r.Usable), Flag(r.Terminal), Flag(r.Dispatchable),
            Real(r.ElapsedSeconds), Text(r.Author), Text(r.Tool), Text(r.Version),
            Text(r.DiagnosticPhase), Text(r.DiagnosticKind), Text(r.DiagnosticMessage),
            Whole(r.DiagnosticCode), Flag(r.Transient), Text(r.ResultSha256), Whole(r.ResultBytes),
            Whole(r.DependsOnCount), Whole(r.ResultCount), Moment(r.At)),
        observation: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.Sensor), Text(r.Aspect), Text(r.QuantityType), Text(r.Unit),
            Text(r.Sampling), Real(r.CadenceSeconds), Moment(r.WindowStart), Moment(r.WindowEnd),
            Whole(r.ChunkCount), Whole(r.SampleCount), Real(r.SpanSeconds),
            Whole(r.GradedSamples), Whole(r.ConsumableSamples), Real(r.Completeness),
            Real(r.MinimumSi), Real(r.MaximumSi), Real(r.MeanSi), Real(r.TotalSi),
            Text(r.Manufacturer), Text(r.Model), Text(r.Serial), Day(r.CalibratedAt)),
        coverage: static r => Seq(
            Text(r.Snapshot), Text(r.Element), Text(r.RasterSha256), Whole(r.RasterBytes),
            Text(r.Kind), Text(r.CrsResolution),
            Whole(r.Epsg), Text(r.GeodeticDatum))
            + r.Affine.Map(static coefficient => Real(coefficient))
            + Seq(
            Whole(r.Columns), Whole(r.Rows), Whole(r.Layers),
            Whole(r.BandIndex), Text(r.BandName), Whole(r.SampleType), Text(r.Role), Text(r.Units),
            Real(r.Offset), Real(r.Scale), Real(r.NoData),
            Whole(r.OverviewCount), Whole(r.ByteLength)));

    static Option<PropertyValue> Text(Option<string> value) => value.Map(static v => (PropertyValue)new PropertyValue.Text(v));
    static Option<PropertyValue> Real(Option<double> value) => value.Map(static v => (PropertyValue)new PropertyValue.Number(v));
    static Option<PropertyValue> Whole<T>(Option<T> value) where T : IBinaryInteger<T> =>
        value.Map(static v => (PropertyValue)new PropertyValue.Integer(BigInteger.CreateChecked(v)));
    static Option<PropertyValue> Flag(Option<bool> value) => value.Map(static v => (PropertyValue)new PropertyValue.Boolean(v));
    static Option<PropertyValue> Moment(Option<Instant> value) => value.Map(static v => (PropertyValue)new PropertyValue.Temporal(new TemporalValue.Stamp(v)));
    static Option<PropertyValue> Day(Option<LocalDate> value) => value.Map(static v => (PropertyValue)new PropertyValue.Temporal(new TemporalValue.Date(v)));
}

public sealed record TableSnapshot(ContentAddress Address, Seq<TableRow> Rows) {
    public Fin<Seq<TableBatch>> Batches(Op key) => TableFamily.Admit(Rows, key).ToFin().Map(_ => Grouped());

    Seq<TableBatch> Grouped() {
        HashMap<TableFamily, Seq<Seq<Option<PropertyValue>>>> filed = Rows.Fold(
            HashMap<TableFamily, Seq<Seq<Option<PropertyValue>>>>(),
            static (map, row) => map.AddOrUpdate(
                row.Family,
                cells => cells.Add(row.Cells),
                Seq<Seq<Option<PropertyValue>>>().Add(row.Cells)));
        return toSeq(TableFamily.Items).Map(family =>
            new TableBatch(family.Declaration, filed.Find(family).IfNone(Seq<Seq<Option<PropertyValue>>>())));
    }
}
```

## [03]-[DATASET_ROSTER]

- Owner: `TableType` — the producer's neutral physical-token roster carrying its `Admits` predicate over `PropertyValue`; `TableColumn` — one named, typed, nullability-carrying column; `TableSpine` — the closed temporal-category family binding each category to the clock column it implies; `TableDeclaration` — the producer-neutral dataset self-description (dotted name, `KeyColumns` identity, `TableSpine` category, optional `Measure`, ordered `Columns`) carrying `Wire`/`Admission`/`Conforms`, the ONE shape the boundary crossing keys on (E-M17 — a foreign `materials.*` producer instantiates it directly); `TableFamily` — the `[SmartEnum<string>]` roster of the ELEMENT-owned datasets, each row wrapping its declaration; `TableBatch` — one declaration's erased cell rows, the value crossing the boundary.
- Entry: `declaration.Admission` projects the whole argument set the columnar custodian's admission gate takes, `Wire` its column-triple half; `declaration.Conforms(cells, key)` proves one row's arity and cell types against the declaration; `TableFamily.Admit(rows, key)` folds that proof over every element row; a foreign producer mints `new TableDeclaration(...)` over the same `TableType`/`TableColumn`/`TableSpine` vocabulary and crosses `TableBatch(declaration, rows)` — no roster edit, no contract sibling.
- Auto: `TableType.Admits` is the per-token predicate over the contract's own `PropertyValue` cases, so the producer proves its declaration and its projection agree BEFORE anything crosses — a proof the custodian cannot run, because the custodian never sees a `PropertyValue`; `Conforms` accumulates through `Validation<Error, Unit>` so a malformed dataset reports every bad column at once, while the custodian's own arity gate re-proves the row against admitted identifiers on its side of the boundary; `TableSpine` fuses the family's temporal CATEGORY with the clock column that category implies, so `Event` carries the column the row itself stamps and `Landing` carries none and hands the axis to the custodian — a family declaring a category its columns contradict is unrepresentable here rather than refused downstream, and the category follows the dataset's own semantics under the branch analytics ruling.
- Output: `Wire` is the schema handoff and `TableBatch` the row handoff; the pair is the whole producer surface, and nothing else about this page crosses.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with the generated `Items` roster and key lookup), LanguageExt.Core (`Seq`/`Option`/`Validation`/`Error` + the applicative `Traverse` accumulation), `Projection/fault#FAULT_BAND` (`ElementFault.ValueRejected`).
- Growth: a new physical token is one `TableType` row answering its `Admits` predicate; a new ELEMENT dataset is one `TableFamily` row beside its `TableRow` case, its category one `TableSpine` case whose payload names the clock that category owns; a new column is one `TableColumn` beside its payload field; a FOREIGN dataset is one `TableDeclaration` mint at its producer — never a `TableFamily` row, never an unsealed roster, never a batch sibling; a dialect spelling, an Arrow field, a plan literal, and a landing verb all grow at the columnar custodian and never here.
- Boundary: `TableSpine` and `TableType` cross as TEXT because this package references the kernel alone and the custodian's own category and column-type rosters are unreachable from here, so the contract's whole vocabulary is producer-written text the gate admits — typing a producer against the custodian's rows demands a reference the strata forbid and the store already holds in the other direction.
- Boundary: `TableType` carries the token and its cell predicate ALONE — the three SQL dialects, the record-batch field, the binary-COPY wire type, and the Substrait literal are the custodian's row columns, so this roster is the producer half the gate mirrors rather than a second physical vocabulary, and a token this roster mints that the custodian's roster lacks fails at that gate, which is the compiler this contract does not have.
- Boundary: `TableSpine` clocks by evidence, not convenience — `element.assessments` is the one EVENT-TIME family and partitions on `at`, the instant its assessment ran, so a rollup never reports arrival time as work time, while every snapshot family is landing-timed because re-tabulating one frozen graph reproduces its facts unchanged and a tabulation instant there re-dates immutable evidence to whenever it was last projected.
- Boundary: `TableFamily` declares a `Measure` only where a numeric column genuinely folds — a rollup over a count, a token, or a content key is a fabricated statistic the absent measure forecloses.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TableType {
    public static readonly TableType Utf8      = new("utf8",         static value => value is PropertyValue.Text);
    public static readonly TableType Float64   = new("float64",      static value => value is PropertyValue.Number);
    public static readonly TableType Int64     = new("int64",        static value => value is PropertyValue.Integer);
    public static readonly TableType Bool      = new("bool",         static value => value is PropertyValue.Boolean);
    public static readonly TableType Date      = new("date32",       static value => value is PropertyValue.Temporal { Value: TemporalValue.Date });
    public static readonly TableType Timestamp = new("timestamp-ns", static value => value is PropertyValue.Temporal { Value: TemporalValue.Stamp });
    public static readonly TableType KeyHex    = new("fixed-hex128", static value => value is PropertyValue.Text);

    public Func<PropertyValue, bool> Admits { get; }
}

public readonly record struct TableColumn(string Name, TableType Type, bool Nullable) {
    public Validation<Error, Unit> Conforms(Option<PropertyValue> cell, Op key) => cell.Match(
        None: () => Gate(Nullable, key, $"<table-cell-absent:{Name}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
        Some: value => Gate(Type.Admits(value), key, $"<table-cell-type:{Name}:{Type.Key}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableSpine {
    private TableSpine() { }

    public sealed record Event(string Column) : TableSpine;
    public sealed record Landing : TableSpine;

    public string Key => Map(@event: "event", landing: "landing");

    public Option<string> Time => Switch(
        @event:  static spine => Optional(spine.Column),
        landing: static _ => Option<string>.None);
}

public sealed record TableDeclaration(
    string Dataset, Seq<string> KeyColumns, TableSpine Spine, Option<string> Measure, Seq<TableColumn> Columns) {

    public Seq<(string Name, string Type, bool Nullable)> Wire =>
        Columns.Map(static column => (column.Name, column.Type.Key, column.Nullable));

    public (string Dataset, Seq<(string Name, string Type, bool Nullable)> Columns,
        Seq<string> Key, string Spine, Option<string> Time, Option<string> Measure) Admission =>
        (Dataset, Wire, KeyColumns, Spine.Key, Spine.Time, Measure);

    public Validation<Error, Unit> Conforms(Seq<Option<PropertyValue>> cells, Op key) =>
        cells.Count != Columns.Count
            ? new ElementFault.ValueRejected(key, $"<table-arity:{Dataset}:{cells.Count}/{Columns.Count}>")
            : Columns.Zip(cells, static (column, cell) => (Column: column, Cell: cell))
                .Traverse(pair => pair.Column.Conforms(pair.Cell, key))
                .As()
                .Map(static _ => unit);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TableFamily {
    private TableFamily(string key, Seq<string> keyColumns, TableSpine spine, Option<string> measure, Seq<TableColumn> columns)
        : this(key, declaration: new TableDeclaration(key, keyColumns, spine, measure, columns)) { }

    public static readonly TableFamily Objects = new("element.objects",
        Seq("snapshot", "element"), spine: new TableSpine.Landing(), Option<string>.None, Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("kind", TableType.Utf8, Nullable: false),
            new TableColumn("external_id", TableType.Utf8, Nullable: true),
            new TableColumn("classification_system", TableType.Utf8, Nullable: false),
            new TableColumn("classification_code", TableType.Utf8, Nullable: false),
            new TableColumn("classification_edition", TableType.Utf8, Nullable: false),
            new TableColumn("predefined", TableType.Utf8, Nullable: false),
            new TableColumn("name", TableType.Utf8, Nullable: false),
            new TableColumn("tag", TableType.Utf8, Nullable: false),
            new TableColumn("type_id", TableType.Utf8, Nullable: true),
            new TableColumn("container", TableType.Utf8, Nullable: true),
            new TableColumn("containment_depth", TableType.Int64, Nullable: false),
            new TableColumn("appearance", TableType.Utf8, Nullable: true),
            new TableColumn("part_count", TableType.Int64, Nullable: false)));

    public static readonly TableFamily Classifications = new("element.classifications",
        Seq("snapshot", "element", "system", "code", "edition"), spine: new TableSpine.Landing(), Option<string>.None, Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("system", TableType.Utf8, Nullable: false),
            new TableColumn("code", TableType.Utf8, Nullable: false),
            new TableColumn("edition", TableType.Utf8, Nullable: false),
            new TableColumn("source", TableType.Utf8, Nullable: true),
            new TableColumn("edition_date", TableType.Date, Nullable: true),
            new TableColumn("title", TableType.Utf8, Nullable: true)));

    public static readonly TableFamily Properties = new("element.properties",
        Seq("snapshot", "element", "set_name", "name"), spine: new TableSpine.Landing(), Some("si"), Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("set_name", TableType.Utf8, Nullable: false),
            new TableColumn("name", TableType.Utf8, Nullable: false),
            new TableColumn("kind", TableType.Utf8, Nullable: false),
            new TableColumn("rendered", TableType.Utf8, Nullable: false),
            new TableColumn("si", TableType.Float64, Nullable: true),
            new TableColumn("quantity_type", TableType.Utf8, Nullable: true),
            new TableColumn("source", TableType.Utf8, Nullable: false),
            new TableColumn("inheritance", TableType.Utf8, Nullable: false)));

    public static readonly TableFamily Quantities = new("element.quantities",
        Seq("snapshot", "element", "set_name", "name"), spine: new TableSpine.Landing(), Some("si"), Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("set_name", TableType.Utf8, Nullable: false),
            new TableColumn("name", TableType.Utf8, Nullable: false),
            new TableColumn("quantity_type", TableType.Utf8, Nullable: false),
            new TableColumn("si", TableType.Float64, Nullable: false),
            new TableColumn("unit", TableType.Utf8, Nullable: true),
            new TableColumn("dim_length", TableType.Int64, Nullable: false),
            new TableColumn("dim_mass", TableType.Int64, Nullable: false),
            new TableColumn("dim_time", TableType.Int64, Nullable: false),
            new TableColumn("dim_current", TableType.Int64, Nullable: false),
            new TableColumn("dim_temperature", TableType.Int64, Nullable: false),
            new TableColumn("dim_amount", TableType.Int64, Nullable: false),
            new TableColumn("dim_luminous", TableType.Int64, Nullable: false),
            new TableColumn("uncertainty", TableType.Utf8, Nullable: true),
            new TableColumn("lower_si", TableType.Float64, Nullable: true),
            new TableColumn("upper_si", TableType.Float64, Nullable: true)));

    public static readonly TableFamily Materials = new("element.materials",
        Seq("snapshot", "element", "material"), spine: new TableSpine.Landing(), Option<string>.None, Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("material", TableType.Utf8, Nullable: false),
            new TableColumn("composition", TableType.Utf8, Nullable: false),
            new TableColumn("usage", TableType.Utf8, Nullable: false),
            new TableColumn("inherited", TableType.Bool, Nullable: false),
            new TableColumn("layer_count", TableType.Int64, Nullable: false),
            new TableColumn("total_thickness_si", TableType.Float64, Nullable: true),
            new TableColumn("profile_standard", TableType.Utf8, Nullable: true),
            new TableColumn("profile_designation", TableType.Utf8, Nullable: true),
            new TableColumn("section_area_si", TableType.Float64, Nullable: true)));

    public static readonly TableFamily Sections = new("element.sections",
        Seq("snapshot", "element", "material"), spine: new TableSpine.Landing(), Some("area_si"), Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("material", TableType.Utf8, Nullable: false),
            new TableColumn("profile_standard", TableType.Utf8, Nullable: false),
            new TableColumn("profile_designation", TableType.Utf8, Nullable: false),
            new TableColumn("ltb_route", TableType.Utf8, Nullable: false),
            new TableColumn("area_si", TableType.Float64, Nullable: false),
            new TableColumn("iyy_si", TableType.Float64, Nullable: false),
            new TableColumn("izz_si", TableType.Float64, Nullable: false),
            new TableColumn("j_si", TableType.Float64, Nullable: false),
            new TableColumn("iw_si", TableType.Float64, Nullable: false),
            new TableColumn("wely_si", TableType.Float64, Nullable: false),
            new TableColumn("welz_si", TableType.Float64, Nullable: false),
            new TableColumn("wply_si", TableType.Float64, Nullable: false),
            new TableColumn("wplz_si", TableType.Float64, Nullable: false),
            new TableColumn("av_y_si", TableType.Float64, Nullable: false),
            new TableColumn("av_z_si", TableType.Float64, Nullable: false),
            new TableColumn("radius_major_si", TableType.Float64, Nullable: false),
            new TableColumn("radius_minor_si", TableType.Float64, Nullable: false),
            new TableColumn("depth_si", TableType.Float64, Nullable: false),
            new TableColumn("width_si", TableType.Float64, Nullable: false),
            new TableColumn("heated_perimeter_si", TableType.Float64, Nullable: false),
            new TableColumn("axis_distance_si", TableType.Float64, Nullable: false),
            new TableColumn("shear_centre_y_si", TableType.Float64, Nullable: false),
            new TableColumn("shear_centre_z_si", TableType.Float64, Nullable: false),
            new TableColumn("monosymmetry_factor", TableType.Float64, Nullable: false),
            new TableColumn("centroid_x", TableType.Float64, Nullable: false),
            new TableColumn("centroid_y", TableType.Float64, Nullable: false),
            new TableColumn("centroid_z", TableType.Float64, Nullable: false),
            new TableColumn("form_vertex_count", TableType.Int64, Nullable: true),
            new TableColumn("form_curved_edges", TableType.Int64, Nullable: true),
            new TableColumn("form_radial_ratio", TableType.Float64, Nullable: true),
            new TableColumn("form_perimeter_si", TableType.Float64, Nullable: true)));

    public static readonly TableFamily Edges = new("element.edges",
        Seq("snapshot", "edge"), spine: new TableSpine.Landing(), Option<string>.None, Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("edge", TableType.Utf8, Nullable: false),
            new TableColumn("kind", TableType.Utf8, Nullable: false),
            new TableColumn("sub_kind", TableType.Utf8, Nullable: true),
            new TableColumn("relating", TableType.Utf8, Nullable: false),
            new TableColumn("related", TableType.Utf8, Nullable: false),
            new TableColumn("realizing", TableType.Utf8, Nullable: true),
            new TableColumn("ordinal", TableType.Int64, Nullable: true),
            new TableColumn("wire_name", TableType.Utf8, Nullable: true),
            new TableColumn("member_count", TableType.Int64, Nullable: false),
            new TableColumn("containment", TableType.Bool, Nullable: false)));

    public static readonly TableFamily Assessments = new("element.assessments",
        Seq("snapshot", "element", "discipline", "route", "input_key"), spine: new TableSpine.Event("at"), Some("elapsed_s"), Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("discipline", TableType.Utf8, Nullable: false),
            new TableColumn("route", TableType.Utf8, Nullable: false),
            new TableColumn("input_key", TableType.Utf8, Nullable: false),
            new TableColumn("outcome", TableType.Utf8, Nullable: false),
            new TableColumn("usable", TableType.Bool, Nullable: false),
            new TableColumn("terminal", TableType.Bool, Nullable: false),
            new TableColumn("dispatchable", TableType.Bool, Nullable: false),
            new TableColumn("elapsed_s", TableType.Float64, Nullable: false),
            new TableColumn("author", TableType.Utf8, Nullable: false),
            new TableColumn("tool", TableType.Utf8, Nullable: false),
            new TableColumn("version", TableType.Utf8, Nullable: false),
            new TableColumn("diagnostic_phase", TableType.Utf8, Nullable: true),
            new TableColumn("diagnostic_kind", TableType.Utf8, Nullable: true),
            new TableColumn("diagnostic_message", TableType.Utf8, Nullable: true),
            new TableColumn("diagnostic_code", TableType.Int64, Nullable: true),
            new TableColumn("transient", TableType.Bool, Nullable: true),
            new TableColumn("result_sha256", TableType.Utf8, Nullable: true),
            new TableColumn("result_bytes", TableType.Int64, Nullable: true),
            new TableColumn("depends_on_count", TableType.Int64, Nullable: false),
            new TableColumn("result_count", TableType.Int64, Nullable: false),
            new TableColumn("at", TableType.Timestamp, Nullable: false)));

    public static readonly TableFamily Observations = new("element.observations",
        Seq("snapshot", "element", "sensor", "aspect"), spine: new TableSpine.Landing(), Some("span_s"), Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("sensor", TableType.Utf8, Nullable: false),
            new TableColumn("aspect", TableType.Utf8, Nullable: false),
            new TableColumn("quantity_type", TableType.Utf8, Nullable: false),
            new TableColumn("unit", TableType.Utf8, Nullable: false),
            new TableColumn("sampling", TableType.Utf8, Nullable: false),
            new TableColumn("cadence_s", TableType.Float64, Nullable: true),
            new TableColumn("window_start", TableType.Timestamp, Nullable: false),
            new TableColumn("window_end", TableType.Timestamp, Nullable: false),
            new TableColumn("chunk_count", TableType.Int64, Nullable: false),
            new TableColumn("sample_count", TableType.Int64, Nullable: false),
            new TableColumn("span_s", TableType.Float64, Nullable: false),
            new TableColumn("graded_samples", TableType.Int64, Nullable: false),
            new TableColumn("consumable_samples", TableType.Int64, Nullable: false),
            new TableColumn("completeness", TableType.Float64, Nullable: true),
            new TableColumn("minimum_si", TableType.Float64, Nullable: true),
            new TableColumn("maximum_si", TableType.Float64, Nullable: true),
            new TableColumn("mean_si", TableType.Float64, Nullable: true),
            new TableColumn("total_si", TableType.Float64, Nullable: true),
            new TableColumn("manufacturer", TableType.Utf8, Nullable: true),
            new TableColumn("model", TableType.Utf8, Nullable: true),
            new TableColumn("serial", TableType.Utf8, Nullable: true),
            new TableColumn("calibrated_at", TableType.Date, Nullable: true)));

    public static readonly TableFamily Coverages = new("element.coverages",
        Seq("snapshot", "element", "raster_sha256", "band"), spine: new TableSpine.Landing(), Some("byte_length"), Seq(
            new TableColumn("snapshot", TableType.Utf8, Nullable: false),
            new TableColumn("element", TableType.Utf8, Nullable: false),
            new TableColumn("raster_sha256", TableType.Utf8, Nullable: false),
            new TableColumn("raster_bytes", TableType.Int64, Nullable: false),
            new TableColumn("kind", TableType.Utf8, Nullable: false),
            new TableColumn("crs_resolution", TableType.Utf8, Nullable: false),
            new TableColumn("epsg", TableType.Int64, Nullable: true),
            new TableColumn("geodetic_datum", TableType.Utf8, Nullable: false),
            new TableColumn("affine_m00", TableType.Float64, Nullable: false),
            new TableColumn("affine_m01", TableType.Float64, Nullable: false),
            new TableColumn("affine_m02", TableType.Float64, Nullable: false),
            new TableColumn("affine_m03", TableType.Float64, Nullable: false),
            new TableColumn("affine_m10", TableType.Float64, Nullable: false),
            new TableColumn("affine_m11", TableType.Float64, Nullable: false),
            new TableColumn("affine_m12", TableType.Float64, Nullable: false),
            new TableColumn("affine_m13", TableType.Float64, Nullable: false),
            new TableColumn("affine_m20", TableType.Float64, Nullable: false),
            new TableColumn("affine_m21", TableType.Float64, Nullable: false),
            new TableColumn("affine_m22", TableType.Float64, Nullable: false),
            new TableColumn("affine_m23", TableType.Float64, Nullable: false),
            new TableColumn("columns", TableType.Int64, Nullable: false),
            new TableColumn("rows", TableType.Int64, Nullable: false),
            new TableColumn("layers", TableType.Int64, Nullable: false),
            new TableColumn("band", TableType.Int64, Nullable: false),
            new TableColumn("band_name", TableType.Utf8, Nullable: false),
            new TableColumn("sample_type", TableType.Int64, Nullable: false),
            new TableColumn("role", TableType.Utf8, Nullable: false),
            new TableColumn("units", TableType.Utf8, Nullable: false),
            new TableColumn("offset", TableType.Float64, Nullable: false),
            new TableColumn("scale", TableType.Float64, Nullable: false),
            new TableColumn("no_data", TableType.Float64, Nullable: true),
            new TableColumn("overview_count", TableType.Int64, Nullable: false),
            new TableColumn("byte_length", TableType.Int64, Nullable: false)));

    public TableDeclaration Declaration { get; }

    public static Validation<Error, Unit> Admit(Seq<TableRow> rows, Op key) =>
        rows.Traverse(row => row.Family.Declaration.Conforms(row.Cells, key)).As().Map(static _ => unit);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TableBatch(TableDeclaration Declaration, Seq<Seq<Option<PropertyValue>>> Rows);
```

## [04]-[TABULATE_FOLD]

- Owner: `GraphTable` — the one fold from a frozen `ElementGraph` to a `TableSnapshot`, and the per-family row projections it composes over the baked read and the raw edge array.
- Entry: `GraphTable.Tabulate(graph, key, roots)` folds the whole snapshot by default and a named root set when supplied, refusing `ElementFault.NodeAbsent` on a root the graph does not declare and lifting every `Bake` failure — an absent root, a cyclic `Compose` ancestry — unchanged onto its own result.
- Auto: the fold reaches no clock at all — every snapshot family is landing-timed, so nothing here stamps an instant and `element.assessments` carries the one the assessment payload already holds; element, classification, property, quantity, material, assessment, observation, and coverage rows all project from the `Bake`-derived `Element`, so the named type→occurrence inheritance is applied exactly once and a table row can never disagree with what a consumer reads off the same baked element; edge rows project from `graph.Edges` directly because an edge carries no inheritance and needs no bake; the snapshot address mints once through `ContentAddress.OfGraph` and stamps every row, so one fold pays one graph hash; a scoped fold narrows edges to those whose `Members` touch the selected set, so a partial re-tabulation after a delta emits exactly the rows its roots own.
- Output: a `TableSnapshot` whose `Rows` a consumer folds typed and whose admitted `Batches(key)` the columnar custodian lands.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option` + `TraverseM`/`Choose`/`Bind`/`Fold`/`Exists`), QuikGraph (`BidirectionalGraph`/`TryFunc` + `AlgorithmExtensions.TreeBreadthFirstSearch` over the graph's own `View(EdgeFilter.Spatial, EdgeOrientation.Ascending)` — the object row's two spatial columns, never a view this page builds), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph`/`Of` plus generated raw-key `ToValue()`), `Rasm` (`ContentHash.Hex`), `Projection/fault#FAULT_BAND` (`ElementFault.NodeAbsent`), `Assessment/observation#SERIES_STATISTICS` (`Completeness`/`Observed`/`Consumable` + `Expected`), `Geospatial/coverage#COVERAGE_NODE` (`ByteLength`/`Grid`/`Bands`), NodaTime (`Duration.TotalSeconds`).
- Growth: a new dataset is one projection member returning its `TableRow` case; a new column on an existing dataset is one argument in the projection that already builds its case; a scoped variant is a root set, never a second entrypoint.
- Boundary: `Tabulate` is PURE over an already-frozen snapshot — it opens no store, resolves no geometry through `GeometrySource`, and reaches no ambient registry, so a caller supplies the graph and receives rows; heavy payloads stay behind their content keys, so a representation hash, a result blob, and a raster key cross as text and the artifact itself never enters a row; the edge row keys on the edge's own content address, so two structurally identical edges address as the one edge they are — the positional array index keying them apart is the deleted form, because array order is a snapshot artifact no consumer may join on; a family's row count is the graph's, never capped — the columnar stores carry no cardinality ceiling and a truncating fold silently under-reports a takeoff.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GraphTable {
    public static Fin<TableSnapshot> Tabulate(ElementGraph graph, Op key, Option<Seq<NodeId>> roots = default) =>
        Selected(graph, key, roots)
            .Bind(objects => objects.TraverseM(node => graph.Bake(node.Id, key)).As())
            .Map(elements => Project(graph, elements, roots.IsSome));

    static Fin<Seq<Node.Object>> Selected(ElementGraph graph, Op key, Option<Seq<NodeId>> roots) =>
        roots.Match(
            None: () => Fin.Succ(graph.ObjectNodes),
            Some: ids => toSeq(ids.ToFrozenSet())
                .TraverseM(id => graph.Find<Node.Object>(id)
                    .ToFin(new ElementFault.NodeAbsent(key, $"<tabulate-root-absent:{id.ToValue()}>"))).As());

    static TableSnapshot Project(ElementGraph graph, Seq<Element> elements, bool scoped) {
        ContentAddress address = ContentAddress.OfGraph(graph);
        string snapshot = ContentHash.Hex(address.ToValue());
        double tolerance = graph.Header.Tolerance;
        FrozenSet<NodeId> scope = elements.Map(static element => element.Id).ToFrozenSet();
        return new TableSnapshot(address, elements.Bind(element => Rows(graph, element, snapshot)) + Edges(graph, scope, scoped, snapshot, tolerance));
    }

    static Seq<TableRow> Rows(ElementGraph graph, Element element, string snapshot) =>
        Seq<TableRow>(Object(graph, element, snapshot))
        + Classifications(element, snapshot)
        + element.Properties.Bind(bag => Properties(bag, element, snapshot))
        + element.Quantities.Bind(bag => Quantities(bag, element, snapshot))
        + Materials(element, snapshot)
        + Sections(element, snapshot)
        + element.Assessments.Map(payload => Assessment(payload, element, snapshot))
        + element.Observations.Map(series => Observation(series, element, snapshot))
        + element.Coverages.Bind(grid => Coverages(grid, element, snapshot));

    static TableRow Object(ElementGraph graph, Element element, string snapshot) {
        (Option<NodeId> container, int depth) = Ancestry(graph, element.Id);
        return new TableRow.Object(
            snapshot, element.Id.ToValue(), element.Kind.Key, element.ExternalId,
            element.Classification.System, element.Classification.Code, element.Classification.Edition,
            element.PredefinedType.ToValue(), element.Name, element.Tag,
            element.TypeId.Map(static id => id.ToValue()),
            container.Map(static id => id.ToValue()), depth,
            element.Appearance.Map(static summary => ContentHash.Hex(ContentAddress.Create(summary.AppearanceKey).ToValue())),
            element.Parts.Count);
    }

    static (Option<NodeId> Container, int Depth) Ancestry(ElementGraph graph, NodeId member) {
        BidirectionalGraph<NodeId, TypedEdge> ascending = graph.View(EdgeFilter.Spatial, EdgeOrientation.Ascending);
        TryFunc<NodeId, IEnumerable<TypedEdge>> climb = ascending.TreeBreadthFirstSearch(member);
        return (toSeq(ascending.OutEdges(member)).Head.Map(static leg => leg.Target),
            toSeq(ascending.Vertices).Fold(0, (deepest, vertex) =>
                vertex != member && climb(vertex, out IEnumerable<TypedEdge>? legs)
                    ? Math.Max(deepest, Enumerable.Count(legs))
                    : deepest));
    }

    static Seq<TableRow> Classifications(Element element, string snapshot) =>
        element.Classifications.Map(reference => (TableRow)new TableRow.Classification(
            snapshot, element.Id.ToValue(), reference.System, reference.Code, reference.Edition,
            reference.Source, reference.EditionDate, reference.Title));

    static Seq<TableRow> Properties(PropertyBag bag, Element element, string snapshot) =>
        toSeq(bag.Values).Map(entry => (TableRow)new TableRow.Property(
            snapshot, element.Id.ToValue(), bag.SetName, entry.Key.ToValue(),
            Kind(entry.Value), entry.Value.Render(),
            entry.Value is PropertyValue.Measure measured ? Some(measured.Value.Si) : Option<double>.None,
            entry.Value is PropertyValue.Measure typed ? Some(typed.Value.Type.ToValue()) : Option<string>.None,
            bag.Source.Token, bag.Inheritance.Key));

    static string Kind(PropertyValue value) => value.Map(
        text: "text", measure: "measure", boolean: "boolean", logical: "logical", integer: "integer",
        number: "number", binary: "binary", enumerated: "enumerated", reference: "reference",
        bounded: "bounded", list: "list", table: "table", complex: "complex", temporal: "temporal");

    static Seq<TableRow> Quantities(QuantityBag bag, Element element, string snapshot) =>
        toSeq(bag.Values).Map(entry => (TableRow)new TableRow.Quantity(
            snapshot, element.Id.ToValue(), bag.SetName, entry.Key.ToValue(),
            entry.Value.Type.ToValue(), entry.Value.Si, entry.Value.CanonicalUnit,
            entry.Value.Dimension.Length, entry.Value.Dimension.Mass, entry.Value.Dimension.Time,
            entry.Value.Dimension.Current, entry.Value.Dimension.Temperature,
            entry.Value.Dimension.Amount, entry.Value.Dimension.LuminousIntensity,
            entry.Value.Uncertainty.Map(static band => band.Kind.Key),
            entry.Value.Uncertainty.Map(static band => band.LowerSi),
            entry.Value.Uncertainty.Map(static band => band.UpperSi)));

    static Seq<TableRow> Materials(Element element, string snapshot) {
        Seq<string> inherited = element.Type.Map(static binding =>
            binding.Materials.Map(static baked => baked.Material.MaterialKey.ToValue())).IfNone(Seq<string>());
        return element.Materials.Map(baked => (TableRow)new TableRow.Material(
            snapshot, element.Id.ToValue(), baked.Material.MaterialKey.ToValue(),
            Composition(baked.Material.Composition), Usage(baked.Usage),
            inherited.Exists(id => id == baked.Material.MaterialKey.ToValue()),
            baked.Material.Composition is MaterialComposition.LayerSet layers ? layers.Layers.Count : 0,
            baked.Material.Composition is MaterialComposition.LayerSet depth ? Some(depth.TotalThickness) : Option<double>.None,
            baked.Material.Composition is MaterialComposition.ProfileSet standard ? Some(standard.Profile.Standard) : Option<string>.None,
            baked.Material.Composition is MaterialComposition.ProfileSet designation ? Some(designation.Profile.Designation) : Option<string>.None,
            baked.Material.Composition is MaterialComposition.ProfileSet { Section: { IsSome: true, Case: SectionProperties section } }
                ? Some(section.Area.Si)
                : Option<double>.None));
    }

    static Seq<TableRow> Sections(Element element, string snapshot) =>
        element.Materials.Bind(baked => baked.Material.Composition is MaterialComposition.ProfileSet
            { Section: { IsSome: true, Case: SectionProperties section }, Profile: var profile }
            ? Seq<TableRow>(new TableRow.Section(
                snapshot, element.Id.ToValue(), baked.Material.MaterialKey.ToValue(),
                profile.Standard, profile.Designation, section.Ltb.Key,
                section.Area.Si, section.Iyy.Si, section.Izz.Si, section.J.Si, section.Iw.Si,
                section.Wely.Si, section.Welz.Si, section.Wply.Si, section.Wplz.Si,
                section.AvY.Si, section.AvZ.Si, section.RadiusOfGyrationMajor.Si, section.RadiusOfGyrationMinor.Si,
                section.Depth.Si, section.Width.Si, section.HeatedPerimeter.Si, section.AxisDistance.Si,
                section.ShearCentreY.Si, section.ShearCentreZ.Si, section.MonosymmetryFactor,
                section.Centroid.X, section.Centroid.Y, section.Centroid.Z,
                section.Form.Map(static f => f.VertexCount), section.Form.Map(static f => f.CurvedEdges),
                section.Form.Map(static f => f.RadialRatio), section.Form.Map(static f => f.Perimeter.Si)))
            : Seq<TableRow>());

    static string Composition(MaterialComposition composition) => composition.Map(
        single: "single", layerSet: "layer-set", profileSet: "profile-set", constituentSet: "constituent-set");

    static string Usage(MaterialUsage usage) => usage.Map(
        unbound: "unbound", layerSet: "layer-set", profileSet: "profile-set");

    static Seq<TableRow> Edges(ElementGraph graph, FrozenSet<NodeId> scope, bool scoped, string snapshot, double tolerance) =>
        toSeq(graph.Edges)
            .Filter(edge => !scoped || edge.Members.Exists(scope.Contains))
            .Map(edge => Edge(edge, snapshot, tolerance));

    static TableRow Edge(Relationship edge, string snapshot, double tolerance) {
        (Option<string> subKind, Option<string> realizing, Option<int> ordinal, Option<string> wireName) =
            edge.Switch<(Option<string> SubKind, Option<string> Realizing, Option<int> Ordinal, Option<string> WireName)>(
                compose:   static e => (Some(e.SubKind.Key), Option<string>.None, e.Ordinal, Option<string>.None),
                assign:    static e => (Some(e.SubKind.Key), Option<string>.None, Option<int>.None, Option<string>.None),
                associate: static _ => (Option<string>.None, Option<string>.None, Option<int>.None, Option<string>.None),
                connect:   static e => (Some(e.SubKind.Key), e.Realizing.Map(static node => node.ToValue()), Option<int>.None, Option<string>.None),
                @void:     static e => (Some(e.SubKind.Key), Option<string>.None, Option<int>.None, Option<string>.None),
                generic:   static e => (Option<string>.None, Option<string>.None, Option<int>.None, Some(e.WireName.ToValue())));
        return new TableRow.Edge(
            snapshot, ContentHash.Hex(ContentAddress.Of(edge, tolerance).ToValue()), edge.Kind.Key, subKind,
            edge.Relating.ToValue(), edge.Related.ToValue(), realizing, ordinal, wireName,
            edge.Members.Count, edge.IsContainment);
    }

    static TableRow Assessment(AssessmentPayload payload, Element element, string snapshot) => new TableRow.Assessment(
        snapshot, element.Id.ToValue(), payload.Discipline.Key, payload.Route.Value,
        ContentHash.Hex(ContentAddress.Create(payload.InputKey).ToValue()),
        payload.Outcome.Key,
        payload.Outcome.Capabilities.Admits(OutcomeCapability.Consumable),
        payload.Outcome.Capabilities.Admits(OutcomeCapability.Settled),
        payload.Outcome.Capabilities.Admits(OutcomeCapability.Dispatchable),
        payload.Provenance.Elapsed.TotalSeconds,
        payload.Provenance.Author, payload.Provenance.Tool, payload.Provenance.Version,
        payload.Diagnostic.Map(static d => d.Phase.Key), payload.Diagnostic.Map(static d => d.Kind.Key),
        payload.Diagnostic.Map(static d => d.Message), payload.Diagnostic.Bind(static d => d.Code),
        payload.Diagnostic.Map(static d => d.Kind.Transient),
        payload.ResultArtifact.Map(static artifact => artifact.Sha256),
        payload.ResultArtifact.Map(static artifact => checked((long)artifact.Bytes)),
        payload.DependsOn.Count, payload.Results.Count, payload.Provenance.At);

    static TableRow Observation(ObservationSeries series, Element element, string snapshot) =>
        new TableRow.Observation(
            snapshot, element.Id.ToValue(), series.Sensor.Value, series.Aspect.Value,
            series.Observed.ToValue(), series.CanonicalUnit, series.Sampling.Key,
            series.Cadence.Map(static cadence => cadence.TotalSeconds),
            series.Window.Start, series.Window.End,
            series.Chunks.Count, series.SampleCount, series.Statistics.Span.TotalSeconds,
            series.Statistics.Observed, series.Statistics.Consumable,
            series.Statistics.Completeness(series.Expected(series.Window)),
            series.Statistics.Minimum.Map(static measure => measure.Si),
            series.Statistics.Maximum.Map(static measure => measure.Si),
            series.Statistics.Mean.Map(static measure => measure.Si),
            series.Statistics.Total.Map(static measure => measure.Si),
            series.Provenance.Map(static audit => audit.Manufacturer),
            series.Provenance.Map(static audit => audit.Model),
            series.Provenance.Map(static audit => audit.Serial),
            series.Provenance.Bind(static audit => audit.CalibratedAt));

    static Seq<TableRow> Coverages(CoverageGrid grid, Element element, string snapshot) =>
        grid.Bands.Map(band => (TableRow)new TableRow.Coverage(
            snapshot, element.Id.ToValue(), grid.Raster.Sha256, checked((long)grid.Raster.Bytes), grid.Kind.Key,
            grid.Crs.Resolution.Key, grid.Crs.Epsg, grid.Crs.GeodeticDatum,
            toSeq<double>([.. grid.Grid.Affine]),
            grid.Grid.Columns.Value, grid.Grid.Rows.Value, grid.Grid.Layers.Value,
            band.Index, band.Name, band.SampleType.Key, band.Role.Key, band.Units,
            band.Offset, band.Scale, band.NoData,
            grid.Levels.Count - 1, grid.ByteLength(grid.Base)));
}
```

## [05]-[IMPLEMENTATION_LAW]

- [DATASET_IS_A_CASE]: one dataset spells itself at two co-edited sites, coupled at RUNTIME and not by the compiler — a `TableRow` case carries the payload with its ordered `Cells` arm, its `TableFamily` row carries the matching `TableColumn` list, and `Conforms` is the ONLY proof of the pairing: an arity drift fails at `Batches` and a per-cell type drift names its column there. One column projector over the union base takes `Func<TableRow, Option<PropertyValue>>`, downcasting per cell and trading the generated `Switch`'s compile-time arm exhaustiveness — the one proof catching a case that gains a field — for a runtime cast, so the per-column declaration deriving both halves never lands. That pairing therefore rides one edit at two sites under one runtime gate, and sibling row records with their own projection delegates stay the deleted form: they scatter the dataset across two independently-editable OWNERS with no gate at all.
- [TEMPORAL_CATEGORY]: each family declares a temporal category, never a spine convenience. `element.assessments` is event-time and stamps the instant its work ran; every snapshot family is landing-time, since re-tabulating one frozen graph reproduces identical facts and the snapshot address already carries the version identity a consumer joins on. Tabulation instants on a snapshot re-date immutable evidence, and an assessment's arrival read as its work time inverts the same error.
- [ELEMENT_MODALITY_CLOSURE]: every `Seq` a baked `Element` carries reaches a dataset — co-applied classification references, property and quantity bags, material bindings, computed assessments, measured observation series, coverage bands — so no consumer re-folds the graph for a modality the egress skipped. `Parts` is the one exception the object row already answers, carrying the part count while each part tabulates as its own element. `Graph/element#NODE_MODEL` admitting a node case lands its row family here in the same pass; scalar element columns stay on the object row, needing no grain of their own.
- [PRODUCER_HALF]: `Rasm.Persistence` owns the branch's columnar plane, so this page hands a wire schema with typed rows and owns nothing physical. `TableType` mirrors the custodian's neutral token roster — token and the `Admits` predicate over `PropertyValue`, never a dialect spelling, an Arrow field, a binary-COPY wire type, or a plan literal.
- [CELL_CURRENCY]: `PropertyValue` carries every cell because the contract already owns it, so no second value vocabulary enters. Content keys cross as `Text` through `ContentHash.Hex(address.ToValue())`, the canonical x32 form the kernel fixes as the cross-runtime wire spelling, because a raw 128-bit number loses precision at a JSON boundary.
- [VERSION_PINNED_ROW]: every row leads with the snapshot `ContentAddress` and every family keys on it, so an analytic answer pins the model version it was computed over, a lake holds many versions of one model with no second identity axis, and a cross-family join resolves within one version by construction. Edge rows key on the edge's own content address rather than array position, so two structurally identical edges address as the one edge they are and array order never becomes a join key.
- [DERIVED_TABLE]: row families carry ZERO authority — graph and delta stream own truth, a dropped dataset rebuilds at re-tabulation cost, and no path writes a row back into the graph. `Tabulate` runs pure over an already-frozen snapshot, `Bake` supplying every element-scoped row so type→occurrence inheritance applies exactly once and a table never disagrees with the element a consumer reads, while edges project raw because an edge inherits nothing.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
