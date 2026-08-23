# [ELEMENT_TABLE]

One `Tabulate` fold flattens a frozen `ElementGraph` into ten typed row families — the columnar egress every QTO, cost, coverage, commissioning, and dashboard consumer reads without re-folding the graph. `TableRow` closes the family as a `[Union]` whose case IS the dataset, `TableFamily` carries each dataset's column declaration beside its key, temporal spine, and rollup measure, and every row carries the snapshot `ContentAddress` so an analytic answer pins the exact model version it was computed over. Reading a row family is SQL; re-deriving one from the graph is the deleted form.

`Rasm.Persistence` owns the columnar plane whole — writers, residence, provisioning, serving transport — so this page hands typed rows and a wire schema across the `[WIRE]: AnalyticsSchema` seam and names no storage type, dialect token, Arrow field, or landing verb. `TableType` mirrors the custodian's neutral token roster, `PropertyValue` carries every cell, and `Bake` supplies each element row so type→occurrence inheritance applies once. Every family declares its temporal CATEGORY — `element.assessments` stamps the instant its work ran, and every snapshot family is landing-timed.

## [01]-[INDEX]

- [02]-[ROW_FAMILIES]: `TableRow` closes the ten-case dataset family beside its `Cells` projection, its `Family` token, and the `TableSnapshot` product carrying the graph address.
- [03]-[DATASET_ROSTER]: `TableType` and `TableColumn` declare the neutral token roster, `TableDeclaration` is the producer-neutral dataset self-description carrying `Wire`/`Admission`/`Conforms`, `TableFamily` rosters the element-owned datasets as wrapped declarations, and `TableBatch` — keyed on the declaration — crosses the seam for element and foreign producers alike.
- [04]-[TABULATE_FOLD]: `GraphTable.Tabulate` folds a frozen snapshot under its root scope through the per-family row projections it composes.

## [02]-[ROW_FAMILIES]

- Owner: `TableRow` — the closed `[Union]` whose ten cases ARE the ten datasets, each carrying its flat column payload and the snapshot address, and each owning the ordered `Cells` projection its dataset's column declaration reads; `TableSnapshot` — the fold product pairing the graph `ContentAddress` with every emitted row.
- Cases: `Classification` (one co-applied standard reference — the system, code, and edition triple keying it, beside the source, edition date, and title annotations; the PRIMARY entity-class triple stays denormalized on the object row because it keys that grain, so this family carries the secondary refs the object row cannot hold) · `Object` (one baked element — identity, kind, external id, the primary classification triple, predefined token, name, tag, type binding, container, containment depth, appearance key, part count) · `Property` (one bag entry — set, name, value kind, rendered text, the measure magnitude and quantity type where the entry is measured, source rank, inheritance mode) · `Quantity` (one quantity entry — set, name, quantity type, SI magnitude, canonical unit, the seven `Dimension` exponents, the optional uncertainty band) · `Material` (one material binding — material key, composition and usage tokens, the inheritance flag, layer count and buildup depth, the profile reference and baked section area) · `Section` (one baked profile-set section — the whole S-E1 algebra: profile key, LTB route token, the nineteen SI design columns, mono-symmetry, centroid, and the optional forming-shape witness — where the material row carries only the takeoff area) · `Edge` (one relationship — the edge content address, neutral kind, sub-kind, endpoints, realizing intermediary, nest ordinal, passthrough wire name, member count, containment predicate) · `Assessment` (one computed receipt — discipline, route, input key, outcome with its three behavior columns, provenance, the typed diagnostic, the result blob, the dependency and result counts) · `Observation` (one measured series — sensor deployment, observed aspect, quantity triple, sampling algebra, cadence, window bounds, chunk and sample counts, the graded census shares, the four summary magnitudes, the instrument audit) · `Coverage` (one raster band — raster key, coverage kind, CRS identity, the twelve index-to-world affine coefficients and the three-axis census, band index with its role, sample type, units, decode scale pair, pyramid depth, timeline depth, uncompressed byte length).
- Entry: `row.Family` projects the dataset token through the generated `Map` over precomputed rows; `row.Cells` projects the ordered `Option<PropertyValue>` sequence the family's column declaration binds positionally, an absent cell reading `None` so a nullable column carries real absence rather than a sentinel; `TableSnapshot.Batches(key)` admits the whole row set through `TableFamily.Admit` and then groups every row under its family in roster order, the one value crossing the seam.
- Auto: declaration order of a case's payload IS the column order its `Cells` arm emits and the order `TableFamily` declares, so a column edit and its field edit are one edit at one site; the event-time payload closes on its own instant, so `element.assessments` trails on the column its `Spine` names; the private lifts (`Text`/`Real`/`Whole`/`Big`/`Flag`/`Moment`/`Day`) are the only cell constructors, so every cell's `PropertyValue` case is fixed at the projection rather than chosen per column; a content key — the snapshot address, an edge address, an assessment input key, a result blob, an appearance key, a raster key — crosses as `Text` through `ContentAddress.ToValue`, the canonical X32 spelling `Projection/address#CONTENT_ADDRESS` already owns as the cross-runtime wire form.
- Output: `TableSnapshot.Rows` is the flat typed read a consumer folds directly; `TableSnapshot.Batches(key)` is the admitted, erased per-family cell projection the columnar custodian lands.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` with the generated total `Switch`/`Map`), LanguageExt.Core (`Seq`/`Option`/`Map`), NodaTime (`Instant` the assessment and window stamps, `LocalDate` the calibration stamp), `Rasm` (the kernel `Op` the admission gates thread), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.ToValue` the content-key cell spelling), BCL inbox (`BigInteger` the whole-number cell payload).
- Growth: a new dataset is one `TableRow` case declaring its temporal category, with its `Cells` arm, its `TableFamily` row, and its projection in `[04]`; a new column is one payload field with its cell in the same arm and its `TableColumn` in the same row; never a sibling row type beside the union and never a dataset whose columns live apart from its payload.
- Boundary: `TableRow.Object` shadows the simple name `Object` inside the union body exactly as `Node.Object` does at its own owner, and `TableRow.Classification` shadows the seam classification type the same way, so every construction spells the nested case and the generated arms read `@object:` and `classification:`; the row is a DERIVED projection carrying zero authority — the graph and its receipt stream own truth, a dropped dataset rebuilds by re-tabulating, and writing a table row back into the graph is the deleted inversion; `Cells` carries no storage type, so a physical width, a nullability dialect, and a partition expression stay the custodian's; heavy payloads never enter a row — geometry, result artifacts, and raster coverages ride their content keys, which cross as text.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

// --- [MODELS] -----------------------------------------------------------------------------
// Each case IS one dataset, so the column declaration on TableFamily and the payload here are two halves of
// ONE row that a single edit moves together — a sibling record per dataset under a shared `Row` suffix, each with
// its own projection delegate, is the shape this union deletes. A record root buys structural equality and
// cross-case constant-false comparison, which is what a dedup or a diff over emitted rows wants; the graph's own
// class-root [Union] form exists for the member-level merge drill this projection never enters.
// `Object` shadows the simple name inside this body (the Node.Object precedent), so a construction spells
// `TableRow.Object` and the generated dispatch arm reads `@object:`.
// Only the EVENT-TIME case closes on a spine instant: an assessment happened at `At`, so that column is the row's
// own truth. Every snapshot case carries none — re-tabulating one frozen graph reproduces the identical facts, so
// admission is the only honest clock and the custodian owns it.
[Union]
public abstract partial record TableRow {
    private TableRow() { }

    public sealed record Object(
        string Snapshot, string Element, string Kind, Option<string> ExternalId,
        string ClassificationSystem, string ClassificationCode, string ClassificationEdition,
        string Predefined, string Name, string Tag, Option<string> TypeId,
        Option<string> Container, int ContainmentDepth, Option<string> Appearance, int PartCount) : TableRow;

    // One row per CO-APPLIED standard reference, the grain a classification query filters on ("every element carrying
    // a Uniclass Ss code"). The primary entity-class triple stays on the object row because it KEYS that grain; this
    // family is the secondary set, which is exactly the Seq a baked Element carries and no other dataset reaches.
    public sealed record Classification(
        string Snapshot, string Element, string System, string Code, string Edition,
        Option<string> Source, Option<LocalDate> EditionDate, Option<string> Title) : TableRow;

    public sealed record Property(
        string Snapshot, string Element, string SetName, string Name, string Kind, string Rendered,
        Option<double> Si, Option<string> QuantityType, string Source, string Inheritance) : TableRow;

    // Unit is OPTIONAL because the measure owner's `CanonicalUnit` is: a tally, a consumer-minted quantity type, and a
    // dimension-anonymous product each resolve none, so the column carries real absence rather than a blank spelling a
    // schedule cell would read as a unit.
    public sealed record Quantity(
        string Snapshot, string Element, string SetName, string Name, string QuantityType,
        double Si, Option<string> Unit,
        int DimLength, int DimMass, int DimTime, int DimCurrent, int DimTemperature, int DimAmount, int DimLuminous,
        Option<string> Uncertainty, Option<double> LowerSi, Option<double> UpperSi) : TableRow;

    public sealed record Material(
        string Snapshot, string Element, string MaterialKey, string Composition, string Usage, bool Inherited,
        int LayerCount, Option<double> TotalThicknessSi,
        Option<string> ProfileStandard, Option<string> ProfileDesignation, Option<double> SectionAreaSi) : TableRow;

    // One row per BAKED SECTION — the full S-E1 algebra a structural QTO or design screen reads off SQL, where
    // element.materials carries only the takeoff area. LtbRoute rides as the owner's own derived route token so a
    // §6.3.2 screen filters simplified-vs-general without re-deriving symmetry from the shear-centre columns.
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

    // One row per SERIES flattens the descriptor beside its derived summary, so a commissioning board screens
    // completeness and reads the comparable figure without decoding a chunk blob. Samples stay behind their content
    // keys, so the chunk run never enters a row.
    public sealed record Observation(
        string Snapshot, string Element, string Sensor, string Aspect, string QuantityType, string Unit,
        string Sampling, Option<double> CadenceSeconds, Instant WindowStart, Instant WindowEnd,
        int ChunkCount, int SampleCount, double SpanSeconds, int GradedSamples, int ConsumableSamples,
        Option<double> Completeness, Option<double> MinimumSi, Option<double> MaximumSi,
        Option<double> MeanSi, Option<double> TotalSi,
        string Manufacturer, string Model, string Serial, Option<LocalDate> CalibratedAt) : TableRow;

    // One row per BAND, the grain a coverage query filters on ("every element carrying an irradiance band"), with the
    // placement denormalized onto each band row so a spatial predicate needs no join. Placement crosses as the lattice's
    // OWN twelve index-to-world affine coefficients (row-major 3x4; the omitted fourth row is the invariant [0 0 0 1])
    // beside the three-axis census — the projection the kernel lattice publishes — never a north-up origin-and-cell-size
    // quadruple, which reports an axis-aligned fiction the moment the affine rotates or shears. Affine rides as a Seq
    // because its twelve cells expand positionally in the Cells arm, so the family's own arity proof is the length proof.
    // Coverage carries raster bytes by content key exactly as geometry does.
    public sealed record Coverage(
        string Snapshot, string Element, string RasterSha256, long RasterBytes, string Kind, string CrsResolution,
        Option<int> Epsg, string GeodeticDatum,
        Seq<double> Affine, int Columns, int Rows, int Layers,
        // SampleType is the kernel ChannelDtype ROW KEY, and that roster keys on int — so the column carries the
        // ordinal the kernel owns rather than a spelling this page would have to mint beside it.
        int BandIndex, string BandName, int SampleType, string Role, string Units,
        double Offset, double Scale, Option<double> NoData,
        int OverviewCount, long ByteLength) : TableRow;

    // Map projects the dataset token off precomputed roster rows, so no throwaway lambda allocates per row on
    // a fold that runs once per graph node.
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

    // Arm order IS column order, so the family's column declaration binds each cell POSITIONALLY and
    // TableDeclaration.Conforms proves the pairing before any row crosses. Absence reads None — never a sentinel, never
    // an empty string standing in, because a nullable column and an empty text are distinct facts.
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
        // Affine expands POSITIONALLY into its twelve declared cells, so the family's arity proof doubles as the
        // coefficient-count proof and no thirteenth column can slip in beside it.
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

    // These lifts are the ONLY cell constructors a Cells arm reaches, so a column's PropertyValue case fixes at the
    // projection and can never disagree with the TableType its TableColumn declares. Each takes the Option shape
    // uniformly and a required column's value binds through the `implicit operator Option<A>(A?)` conversion, so one
    // member serves both a nullable and a required column — and a null reference lands None rather than Some(null).
    // ONE whole-number lift over the binary-integer floor — int, long, and any future width land the same Integer
    // cell through BigInteger.CreateChecked, so widening a count column never mints a sibling lift.
    static Option<PropertyValue> Text(Option<string> value) => value.Map(static v => (PropertyValue)new PropertyValue.Text(v));
    static Option<PropertyValue> Real(Option<double> value) => value.Map(static v => (PropertyValue)new PropertyValue.Number(v));
    static Option<PropertyValue> Whole<T>(Option<T> value) where T : IBinaryInteger<T> =>
        value.Map(static v => (PropertyValue)new PropertyValue.Integer(BigInteger.CreateChecked(v)));
    static Option<PropertyValue> Flag(Option<bool> value) => value.Map(static v => (PropertyValue)new PropertyValue.Boolean(v));
    static Option<PropertyValue> Moment(Option<Instant> value) => value.Map(static v => (PropertyValue)new PropertyValue.Temporal(new TemporalValue.Stamp(v)));
    static Option<PropertyValue> Day(Option<LocalDate> value) => value.Map(static v => (PropertyValue)new PropertyValue.Temporal(new TemporalValue.Date(v)));
}

// Address rides ONCE here and stamps each row's Snapshot column, so a consumer joins across families on one
// version key and a lake holds many versions of one model without a second identity axis.
public sealed record TableSnapshot(ContentAddress Address, Seq<TableRow> Rows) {
    // Batches is the CROSSING value, so the declaration-versus-projection proof runs here rather than at whoever
    // happened to mint the snapshot: TableFamily.Admit folds Conforms over every row and accumulates each offending
    // column into one failure, and only an admitted row set groups. A snapshot built outside Tabulate reaches the
    // custodian through this member alone, which is what makes the gate total.
    public Fin<Seq<TableBatch>> Batches(Op key) => TableFamily.Admit(Rows, key).ToFin().Map(_ => Grouped());

    // ONE pass files each row's Cells under its own family, then the roster projects in declared order — reading
    // Family and Cells inside a per-family walk re-mints both projections once per family for every row. Families
    // with no rows yield an EMPTY batch rather than vanishing, so a landing pass sees the whole declared roster and
    // a truncated model reads as zero rows rather than an absent dataset.
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

- Owner: `TableType` — the producer's neutral physical-token roster carrying its `Admits` predicate over `PropertyValue`; `TableColumn` — one named, typed, nullability-carrying column; `TableSpine` — the closed temporal-category family binding each category to the clock column it implies; `TableDeclaration` — the producer-neutral dataset self-description (dotted name, `KeyColumns` identity, `TableSpine` category, optional `Measure`, ordered `Columns`) carrying `Wire`/`Admission`/`Conforms`, the ONE shape the seam crossing keys on (E-M17 — a foreign `materials.*` producer instantiates it directly); `TableFamily` — the `[SmartEnum<string>]` roster of the ELEMENT-owned datasets, each row wrapping its declaration; `TableBatch` — one declaration's erased cell rows, the value crossing the seam.
- Entry: `declaration.Admission` projects the whole argument set the columnar custodian's admission gate takes, `Wire` its column-triple half; `declaration.Conforms(cells, key)` proves one row's arity and cell types against the declaration; `TableFamily.Admit(rows, key)` folds that proof over every element row; a foreign producer mints `new TableDeclaration(...)` over the same `TableType`/`TableColumn`/`TableSpine` vocabulary and crosses `TableBatch(declaration, rows)` — no roster edit, no seam sibling.
- Auto: `TableType.Admits` is the per-token predicate over the seam's own `PropertyValue` cases, so the producer proves its declaration and its projection agree BEFORE anything crosses — a proof the custodian cannot run, because the custodian never sees a `PropertyValue`; `Conforms` accumulates through `Validation<Error, Unit>` so a malformed dataset reports every bad column at once, while the custodian's own arity gate re-proves the row against admitted identifiers on its side of the seam; `TableSpine` fuses the family's temporal CATEGORY with the clock column that category implies, so `Event` carries the column the row itself stamps and `Landing` carries none and hands the axis to the custodian — a family declaring a category its columns contradict is unrepresentable here rather than refused downstream, and the category follows the dataset's own semantics under the branch analytics ruling.
- Output: `Wire` is the schema handoff and `TableBatch` the row handoff; the pair is the whole producer surface, and nothing else about this page crosses.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with the generated `Items` roster and key lookup), LanguageExt.Core (`Seq`/`Option`/`Validation`/`Error` + the applicative `Traverse` accumulation), `Projection/fault#FAULT_BAND` (`ElementFault.ValueRejected`).
- Growth: a new physical token is one `TableType` row answering its `Admits` predicate; a new ELEMENT dataset is one `TableFamily` row beside its `TableRow` case, its category one `TableSpine` case whose payload names the clock that category owns; a new column is one `TableColumn` beside its payload field; a FOREIGN dataset is one `TableDeclaration` mint at its producer — never a `TableFamily` row, never an unsealed roster, never a batch sibling; a dialect spelling, an Arrow field, a plan literal, and a landing verb all grow at the columnar custodian and never here.
- Boundary: `TableSpine` and `TableType` cross as TEXT because this package references the kernel alone and the custodian's own category and column-type rosters are unreachable from here, so the seam's whole vocabulary is producer-written text the gate admits — typing a producer against the custodian's rows demands a reference the strata forbid and the store already holds in the other direction.
- Boundary: `TableType` carries the token and its cell predicate ALONE — the three SQL dialects, the record-batch field, the binary-COPY wire type, and the Substrait literal are the custodian's row columns, so this roster is the producer half the gate mirrors rather than a second physical vocabulary, and a token this roster mints that the custodian's roster lacks fails at that gate, which is the compiler this seam does not have.
- Boundary: `TableSpine` clocks by evidence, not convenience — `element.assessments` is the one EVENT-TIME family and partitions on `at`, the instant its assessment ran, so a rollup never reports arrival time as work time, while every snapshot family is landing-timed because re-tabulating one frozen graph reproduces its facts unchanged and a tabulation instant there re-dates immutable evidence to whenever it was last projected.
- Boundary: `TableFamily` declares a `Measure` only where a numeric column genuinely folds — a rollup over a count, a token, or a content key is a fabricated statistic the absent measure forecloses.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// KEY is the whole crossing value, and Admits is the local half no custodian can run — it binds the token to the
// PropertyValue case a Cells arm emits, so a column typed `float64` carrying a Text cell fails here rather than at
// a binary import that infers nothing from a column list.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TableType {
    public static readonly TableType Utf8      = new("utf8",         static value => value is PropertyValue.Text);
    public static readonly TableType Float64   = new("float64",      static value => value is PropertyValue.Number);
    public static readonly TableType Int64     = new("int64",        static value => value is PropertyValue.Integer);
    public static readonly TableType Bool      = new("bool",         static value => value is PropertyValue.Boolean);
    public static readonly TableType Date      = new("date32",       static value => value is PropertyValue.Temporal { Value: TemporalValue.Date });
    public static readonly TableType Timestamp = new("timestamp-ns", static value => value is PropertyValue.Temporal { Value: TemporalValue.Stamp });
    // Fixed-width X32 content-key column (E-M1): a dialect renders it as a 32-hex CHAR column while the cell still
    // crosses as Text through ContentAddress.ToValue — the one token the Materials analytics projection adds.
    public static readonly TableType KeyHex    = new("fixed-hex128", static value => value is PropertyValue.Text);

    public Func<PropertyValue, bool> Admits { get; }
}

// One declared column. Nullable is the producer's own contract — a required column refusing an absent cell is what
// keeps a downstream NOT NULL honest, since the custodian receives a positional cell and cannot recover intent.
public readonly record struct TableColumn(string Name, TableType Type, bool Nullable) {
    public Validation<Error, Unit> Conforms(Option<PropertyValue> cell, Op key) => cell.Match(
        None: () => Gate(Nullable, key, $"<table-cell-absent:{Name}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
        Some: value => Gate(Type.Admits(value), key, $"<table-cell-type:{Name}:{Type.Key}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)));
}

// Temporal CATEGORY and the column it implies are ONE value, so a family cannot declare a category its columns
// contradict — an event-time family carries the column its own batch stamps and a landing-time family carries no
// column at all, which is what hands the clock to the custodian. Declaring the pair separately is what lets a
// category be inferred from whether a time argument happened to arrive, and that inference re-dates an event-time
// family to admission with nothing raising. `Event` shadows the contextual keyword exactly as `TableRow.Object`
// shadows its simple name, so the generated arm reads `@event:`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableSpine {
    private TableSpine() { }

    public sealed record Event(string Column) : TableSpine;
    public sealed record Landing : TableSpine;

    // Wire token spelled byte-identically to the custodian's own category row, since the seam compares text.
    public string Key => Map(@event: "event", landing: "landing");

    public Option<string> Time => Switch(
        @event:  static spine => Optional(spine.Column),
        landing: static _ => Option<string>.None);
}

// The DECLARATION carrier (E-M17): everything a dataset states about itself — dotted name, key identity,
// temporal spine, optional measure, ordered columns — extracted from the closed roster below so a FOREIGN
// producer (`materials.*`, any package publishing datasets over this seam vocabulary) instantiates the SAME
// carrier the `element.*` rows wrap. `Wire`/`Admission`/`Conforms` ride HERE and `TableBatch` keys on the
// declaration, so the family/batch crossing is producer-neutral while `element.*` stays a closed roster.
// KeyColumns names the identity every row is unique under — Snapshot leads every family, so a lake holding
// many versions of one model prunes on the version key before any predicate applies.
public sealed record TableDeclaration(
    string Dataset, Seq<string> KeyColumns, TableSpine Spine, Option<string> Measure, Seq<TableColumn> Columns) {

    // Neutral triples are the schema handoff the columnar custodian's gate turns into admitted identifiers.
    // Producers state name, token, and nullability; every physical decision past that is the custodian's.
    public Seq<(string Name, string Type, bool Nullable)> Wire =>
        Columns.Map(static column => (column.Name, column.Type.Key, column.Nullable));

    // Admission carries the custodian gate's whole argument set, so the composing root splats one value and cannot
    // pair a dataset's columns with another's key, spine, or measure. Category and clock travel together because the
    // custodian refuses a dataset its columns contradict, and both cross as text: this package references the kernel
    // alone, so the custodian's own category type is unreachable here exactly as its column-type roster is, and
    // `TableSpine` mirrors that vocabulary the same way `TableType` mirrors the physical tokens.
    public (string Dataset, Seq<(string Name, string Type, bool Nullable)> Columns,
        Seq<string> Key, string Spine, Option<string> Time, Option<string> Measure) Admission =>
        (Dataset, Wire, KeyColumns, Spine.Key, Spine.Time, Measure);

    // One row's declaration-versus-projection proof, accumulating so a malformed dataset reports every offending
    // column in one failure rather than the first. Arity is checked ahead of the pairwise walk because a short or
    // long cell sequence has no meaningful per-column verdict to report.
    public Validation<Error, Unit> Conforms(Seq<Option<PropertyValue>> cells, Op key) =>
        cells.Count != Columns.Count
            ? new ElementFault.ValueRejected(key, $"<table-arity:{Dataset}:{cells.Count}/{Columns.Count}>")
            : Columns.Zip(cells, static (column, cell) => (Column: column, Cell: cell))
                .Traverse(pair => pair.Column.Conforms(pair.Cell, key))
                .As()
                .Map(static _ => unit);
}

// KEY is the dotted `element.<source>` dataset name the custodian keeps as its wire value, so the producer
// segment declares once by construction and two producers cannot collide on one physical table. Each row WRAPS
// its `TableDeclaration` — the chaining ctor packs the five row arguments, so a dataset row reads as its
// declaration and the roster stays the closed `element.*` census alone.
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

    // Classification rows file the co-applied standard references. Keying on the full (system, code, edition) triple makes a row unique
    // per element: one element carries a Uniclass and an OmniClass reference at once, and two editions of one system
    // are two facts. No measure — a rollup over a classification code is a fabricated statistic.
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

    // The FULL baked-section dataset — element.materials stays the takeoff row; this family carries the whole
    // S-E1 algebra one row per baked ProfileSet section. Measure is area: the one column that genuinely sums.
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

    // Measured evidence rides one row per series beside the computed receipt. `span_s` measures, because covered
    // metering duration genuinely sums across streams where a sample count or a completeness ratio does not.
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

    // BAND is the grain: a coverage answers "which elements carry an irradiance field" only when the band role and
    // its units are columns. The lattice PLACEMENT denormalizes onto every band row as its twelve row-major affine
    // coefficients plus the three-axis census, so a rotated or sheared grid reports true and a spatial predicate
    // reconstructs the exact index-to-world map with no join; `byte_length` is the measure because uncompressed
    // footprint sums across a model.
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

    // Producer-side gating runs once over a whole snapshot before the batches cross, so a declaration drift is a
    // typed refusal at this owner rather than a partial landing the custodian discards at row n.
    public static Validation<Error, Unit> Admit(Seq<TableRow> rows, Op key) =>
        rows.Traverse(row => row.Family.Declaration.Conforms(row.Cells, key)).As().Map(static _ => unit);
}

// --- [MODELS] -----------------------------------------------------------------------------
// One dataset's erased rows — the crossing value, keyed on the DECLARATION so a foreign producer's batch and an
// element.* batch are one type at the seam. Cells stay positional against the declaration's own Columns, which is
// exactly the contract Conforms proves, so the custodian binds by ordinal with no name matching on its side.
public readonly record struct TableBatch(TableDeclaration Declaration, Seq<Seq<Option<PropertyValue>>> Rows);
```

## [04]-[TABULATE_FOLD]

- Owner: `GraphTable` — the one fold from a frozen `ElementGraph` to a `TableSnapshot`, and the per-family row projections it composes over the baked read and the raw edge array.
- Entry: `GraphTable.Tabulate(graph, key, roots)` folds the whole snapshot by default and a named root set when supplied, railing `ElementFault.NodeAbsent` on a root the graph does not declare and lifting every `Bake` failure — an absent root, a cyclic `Compose` ancestry — unchanged onto its own rail.
- Auto: the fold reaches no clock at all — every snapshot family is landing-timed, so nothing here stamps an instant and `element.assessments` carries the one the assessment payload already holds; element, classification, property, quantity, material, assessment, observation, and coverage rows all project from the `Bake`-derived `Element`, so the named type→occurrence inheritance is applied exactly once and a table row can never disagree with what a consumer reads off the same baked element; edge rows project from `graph.Edges` directly because an edge carries no inheritance and needs no bake; the snapshot address mints once through `ContentAddress.OfGraph` and stamps every row, so one fold pays one graph hash; a scoped fold narrows edges to those whose `Members` touch the selected set, so a partial re-tabulation after a delta emits exactly the rows its roots own.
- Output: a `TableSnapshot` whose `Rows` a consumer folds typed and whose admitted `Batches(key)` the columnar custodian lands.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option` + `TraverseM`/`Choose`/`Bind`/`Fold`/`Exists`), QuikGraph (`BidirectionalGraph`/`TryFunc` + `AlgorithmExtensions.TreeBreadthFirstSearch` over the graph's own `View(EdgeFilter.Spatial, EdgeOrientation.Ascending)` — the object row's two spatial columns, never a view this page builds), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph`/`Of`/`ToValue`), `Projection/fault#FAULT_BAND` (`ElementFault.NodeAbsent`), `Assessment/observation#SERIES_STATISTICS` (`Completeness`/`Observed`/`Consumable` + `Expected`), `Geospatial/coverage#COVERAGE_NODE` (`ByteLength`/`Grid`/`Bands`), NodaTime (`Duration.TotalSeconds`).
- Growth: a new dataset is one projection member returning its `TableRow` case; a new column on an existing dataset is one argument in the projection that already builds its case; a scoped variant is a root set, never a second entrypoint.
- Boundary: `Tabulate` is PURE over an already-frozen snapshot — it opens no store, resolves no geometry through `GeometrySource`, and reaches no ambient registry, so a caller supplies the graph and receives rows; heavy payloads stay behind their content keys, so a representation hash, a result blob, and a raster key cross as text and the artifact itself never enters a row; the edge row keys on the edge's own content address, so two structurally identical edges address as the one edge they are — the positional array index keying them apart is the deleted form, because array order is a snapshot artifact no consumer may join on; a family's row count is the graph's, never capped — the columnar residences carry no cardinality ceiling and a truncating fold silently under-reports a takeoff.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GraphTable {
    // ONE fold, `roots` its scope: absent folds every object node, present folds exactly the named set and
    // narrows edges to those its nodes touch — the partial re-tabulation a delta-driven landing takes. No clock
    // enters: snapshot rows are landing-timed and the assessment row carries the instant its payload already holds.
    public static Fin<TableSnapshot> Tabulate(ElementGraph graph, Op key, Option<Seq<NodeId>> roots = default) =>
        Selected(graph, key, roots)
            .Bind(objects => objects.TraverseM(node => graph.Bake(node.Id, key)).As())
            .Map(elements => Project(graph, elements, roots.IsSome));

    // Named roots the graph does not declare rail rather than silently narrowing the fold — a scoped landing that
    // drops an unknown id under-reports exactly the rows the caller asked for. Repeats collapse through the SAME
    // frozen set the edge scope probes, built once per fold: a scoped re-tabulation after a delta can name every node
    // a large change touched, so a linear dedup and a linear scope probe are each quadratic in exactly the case the
    // scoped path exists for.
    static Fin<Seq<Node.Object>> Selected(ElementGraph graph, Op key, Option<Seq<NodeId>> roots) =>
        roots.Match(
            None: () => Fin.Succ(graph.ObjectNodes),
            Some: ids => toSeq(ids.ToFrozenSet())
                .TraverseM(id => graph.Find<Node.Object>(id).Match(
                    Some: Fin.Succ,
                    None: () => Fin.Fail<Node.Object>(new ElementFault.NodeAbsent(key, $"<tabulate-root-absent:{id.Value}>")))).As());

    static TableSnapshot Project(ElementGraph graph, Seq<Element> elements, bool scoped) {
        ContentAddress address = ContentAddress.OfGraph(graph);
        string snapshot = address.ToValue();
        double tolerance = graph.Header.Tolerance;
        FrozenSet<NodeId> scope = elements.Map(static element => element.Id).ToFrozenSet();
        return new TableSnapshot(address, elements.Bind(element => Rows(graph, element, snapshot)) + Edges(graph, scope, scoped, snapshot, tolerance));
    }

    // One element contributes its object row plus every classification reference, bag, binding, receipt, series, and
    // coverage band it carries — ONE walk over the baked read, so no element-scoped family re-enumerates the element.
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
            snapshot, element.Id.Value, element.Kind.Key, element.ExternalId,
            element.Classification.System, element.Classification.Code, element.Classification.Edition,
            element.PredefinedType.Token, element.Name, element.Tag,
            element.TypeId.Map(static id => id.Value),
            container.Map(static id => id.Value), depth,
            element.Appearance.Map(static summary => ContentAddress.Of(summary.AppearanceKey).ToValue()),
            element.Parts.Count);
    }

    // Both spatial columns read the graph's OWN ascending Spatial view and nothing else: rooted Contain-then-Aggregate
    // precedence is Bim SpatialStructure.Ancestry's alone (element#GROUP_READS), so this row REPORTS what the view
    // states — the nearest ascending neighbour as the container and the longest climb as the depth — and arbitrates no
    // multi-parent fan. One breadth-first climb per element serves both columns off the memoized view.
    static (Option<NodeId> Container, int Depth) Ancestry(ElementGraph graph, NodeId member) {
        BidirectionalGraph<NodeId, TypedEdge> ascending = graph.View(EdgeFilter.Spatial, EdgeOrientation.Ascending);
        TryFunc<NodeId, IEnumerable<TypedEdge>> climb = ascending.TreeBreadthFirstSearch(member);
        return (toSeq(ascending.OutEdges(member)).Head.Map(static leg => leg.Target),
            toSeq(ascending.Vertices).Fold(0, (deepest, vertex) =>
                vertex != member && climb(vertex, out IEnumerable<TypedEdge>? legs)
                    ? Math.Max(deepest, Enumerable.Count(legs))
                    : deepest));
    }

    // Classifications files the baked element's secondary references, already unioned with the inherited type's set by
    // Bake and deduped there, so this projection needs no second merge. The primary triple is not re-emitted here: it keys the
    // object row, and duplicating it makes one classification two rows under two grains.
    static Seq<TableRow> Classifications(Element element, string snapshot) =>
        element.Classifications.Map(reference => (TableRow)new TableRow.Classification(
            snapshot, element.Id.Value, reference.System, reference.Code, reference.Edition,
            reference.Source, reference.EditionDate, reference.Title));

    // Kind spells the analytics DIALECT token for a PropertyValue case — this page owns it, because the value
    // owner carries no such token and a query filtering on typed evidence needs one. Precomputed Map arms.
    static Seq<TableRow> Properties(PropertyBag bag, Element element, string snapshot) =>
        toSeq(bag.Values).Map(entry => (TableRow)new TableRow.Property(
            snapshot, element.Id.Value, bag.SetName, entry.Key.Value,
            Kind(entry.Value), entry.Value.Render(),
            entry.Value is PropertyValue.Measure measured ? Some(measured.Value.Si) : Option<double>.None,
            entry.Value is PropertyValue.Measure typed ? Some(typed.Value.Type.Value) : Option<string>.None,
            bag.Source.Token, bag.Inheritance.Key));

    static string Kind(PropertyValue value) => value.Map(
        text: "text", measure: "measure", boolean: "boolean", logical: "logical", integer: "integer",
        number: "number", binary: "binary", enumerated: "enumerated", reference: "reference",
        bounded: "bounded", list: "list", table: "table", complex: "complex", temporal: "temporal");

    // Seven exponents ride as columns so a dimensional filter (`every L^3 quantity`) is a predicate rather than a
    // token match, and the band rides as three columns so an uncertainty-aware rollup reads bounds without a join.
    static Seq<TableRow> Quantities(QuantityBag bag, Element element, string snapshot) =>
        toSeq(bag.Values).Map(entry => (TableRow)new TableRow.Quantity(
            snapshot, element.Id.Value, bag.SetName, entry.Key.Value,
            entry.Value.Type.Value, entry.Value.Si, entry.Value.CanonicalUnit,
            entry.Value.Dimension.Length, entry.Value.Dimension.Mass, entry.Value.Dimension.Time,
            entry.Value.Dimension.Current, entry.Value.Dimension.Temperature,
            entry.Value.Dimension.Amount, entry.Value.Dimension.LuminousIntensity,
            entry.Value.Uncertainty.Map(static band => band.Kind.Key),
            entry.Value.Uncertainty.Map(static band => band.LowerSi),
            entry.Value.Uncertainty.Map(static band => band.UpperSi)));

    // `Inherited` separates a binding the occurrence carries from one the Component supplied, which a takeoff needs:
    // an inherited material is the type's declaration realized here, not a second physical assignment.
    static Seq<TableRow> Materials(Element element, string snapshot) {
        Seq<string> inherited = element.Type.Map(static binding =>
            binding.Materials.Map(static baked => baked.Material.MaterialKey.Value)).IfNone(Seq<string>());
        return element.Materials.Map(baked => (TableRow)new TableRow.Material(
            snapshot, element.Id.Value, baked.Material.MaterialKey.Value,
            Composition(baked.Material.Composition), Usage(baked.Usage),
            inherited.Exists(id => id == baked.Material.MaterialKey.Value),
            baked.Material.Composition is MaterialComposition.LayerSet layers ? layers.Layers.Count : 0,
            baked.Material.Composition is MaterialComposition.LayerSet depth ? Some(depth.TotalThickness) : Option<double>.None,
            baked.Material.Composition is MaterialComposition.ProfileSet standard ? Some(standard.Profile.Standard) : Option<string>.None,
            baked.Material.Composition is MaterialComposition.ProfileSet designation ? Some(designation.Profile.Designation) : Option<string>.None,
            baked.Material.Composition is MaterialComposition.ProfileSet { Section: { IsSome: true, Case: SectionProperties section } }
                ? Some(section.Area.Si)
                : Option<double>.None));
    }

    // One row per baked ProfileSet section — the S-E1 columns whole, so the analytics plane is never takeoff-only.
    static Seq<TableRow> Sections(Element element, string snapshot) =>
        element.Materials.Bind(baked => baked.Material.Composition is MaterialComposition.ProfileSet
            { Section: { IsSome: true, Case: SectionProperties section }, Profile: var profile }
            ? Seq<TableRow>(new TableRow.Section(
                snapshot, element.Id.Value, baked.Material.MaterialKey.Value,
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

    // ONE dispatch over the edge collects every per-case column, so the six-arm walk runs once rather than four
    // parallel Switches restating the same case list — the repeated-arm collapse the graph's own `Ends` accessor makes.
    static Seq<TableRow> Edges(ElementGraph graph, FrozenSet<NodeId> scope, bool scoped, string snapshot, double tolerance) =>
        toSeq(graph.Edges)
            // Each scope probe hits a hash set built once for the whole fold, and the whole-graph fold
            // short-circuits the predicate entirely.
            .Filter(edge => !scoped || edge.Members.Exists(scope.Contains))
            .Map(edge => Edge(edge, snapshot, tolerance));

    static TableRow Edge(Relationship edge, string snapshot, double tolerance) {
        (Option<string> subKind, Option<string> realizing, Option<int> ordinal, Option<string> wireName) =
            edge.Switch<(Option<string> SubKind, Option<string> Realizing, Option<int> Ordinal, Option<string> WireName)>(
                compose:   static e => (Some(e.SubKind.Key), Option<string>.None, e.Ordinal, Option<string>.None),
                assign:    static e => (Some(e.SubKind.Key), Option<string>.None, Option<int>.None, Option<string>.None),
                associate: static _ => (Option<string>.None, Option<string>.None, Option<int>.None, Option<string>.None),
                connect:   static e => (Some(e.SubKind.Key), e.Realizing.Map(static node => node.Value), Option<int>.None, Option<string>.None),
                @void:     static e => (Some(e.SubKind.Key), Option<string>.None, Option<int>.None, Option<string>.None),
                generic:   static e => (Option<string>.None, Option<string>.None, Option<int>.None, Some(e.WireName.Value)));
        return new TableRow.Edge(
            snapshot, ContentAddress.Of(edge, tolerance).ToValue(), edge.Kind.Key, subKind,
            edge.Relating.Value, edge.Related.Value, realizing, ordinal, wireName,
            edge.Members.Count, edge.IsContainment);
    }

    // Three behavior columns ride the row, so a dashboard filters usable results and a sweep counts dispatchable
    // ones without re-deriving the lifecycle vocabulary in SQL.
    static TableRow Assessment(AssessmentPayload payload, Element element, string snapshot) => new TableRow.Assessment(
        snapshot, element.Id.Value, payload.Discipline.Key, payload.Route.Value,
        ContentAddress.Of(payload.InputKey).ToValue(),
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

    // Descriptor and derived summary flatten together, so a commissioning board screens completeness and reads the
    // comparable magnitude off SQL. Completeness resolves through the series' own Expected over its whole window,
    // so an event-driven stream reads None rather than a fabricated denominator, and chunk bytes stay by reference.
    static TableRow Observation(ObservationSeries series, Element element, string snapshot) =>
        new TableRow.Observation(
            snapshot, element.Id.Value, series.Sensor.Value, series.Aspect.Value,
            series.Observed.Value, series.CanonicalUnit, series.Sampling.Key,
            series.Cadence.Map(static cadence => cadence.TotalSeconds),
            series.Window.Start, series.Window.End,
            series.Chunks.Count, series.SampleCount, series.Statistics.Span.TotalSeconds,
            series.Statistics.Observed, series.Statistics.Consumable,
            series.Statistics.Completeness(series.Expected(series.Window)),
            series.Statistics.Minimum.Map(static measure => measure.Si),
            series.Statistics.Maximum.Map(static measure => measure.Si),
            series.Statistics.Mean.Map(static measure => measure.Si),
            series.Statistics.Total.Map(static measure => measure.Si),
            // Instrument audit is OPTIONAL on the series (the blank-string sentinel died at the owner), so the four
            // columns carry real absence.
            series.Provenance.Map(static audit => audit.Manufacturer),
            series.Provenance.Map(static audit => audit.Model),
            series.Provenance.Map(static audit => audit.Serial),
            series.Provenance.Bind(static audit => audit.CalibratedAt));

    // One row per BAND: the placement, the CRS identity, and the pyramid/timeline depths denormalize onto every band so
    // a spatial or role predicate needs no join. ByteLength reads the BASE level, the footprint a storage rollup means.
    // Placement crosses as the kernel lattice's OWN twelve coefficients and three-axis census — the exact index-to-world
    // map a consumer inverts — rather than a derived origin-and-span pair, which is an axis-aligned reading the moment the
    // affine rotates or shears and which the lattice owner names as the fiction it forbids. No host coordinate and
    // no derived Vector3 reaches a cell; the coefficients are the neutral doubles the kernel publishes.
    static Seq<TableRow> Coverages(CoverageGrid grid, Element element, string snapshot) =>
        grid.Bands.Map(band => (TableRow)new TableRow.Coverage(
            snapshot, element.Id.Value, grid.Raster.Sha256, checked((long)grid.Raster.Bytes), grid.Kind.Key,
            grid.Crs.Resolution.Key, grid.Crs.Epsg, grid.Crs.GeodeticDatum,
            toSeq<double>([.. grid.Grid.Affine]),
            grid.Grid.Columns.Value, grid.Grid.Rows.Value, grid.Grid.Layers.Value,
            band.Index, band.Name, band.SampleType.Key, band.Role.Key, band.Units,
            band.Offset, band.Scale, band.NoData,
            // Overviews are the run past the base (the head IS the base); ByteLength sizes the base level, the
            // footprint a storage rollup means.
            grid.Levels.Count - 1, grid.ByteLength(grid.Base)));
}
```

## [05]-[IMPLEMENTATION_LAW]

- [DATASET_IS_A_CASE]: one dataset spells itself at two co-edited sites, coupled at RUNTIME and not by the compiler — a `TableRow` case carries the payload with its ordered `Cells` arm, its `TableFamily` row carries the matching `TableColumn` list, and `Conforms` is the ONLY proof of the pairing: an arity drift fails at `Batches` and a per-cell type drift names its column there. One column projector over the union base takes `Func<TableRow, Option<PropertyValue>>`, downcasting per cell and trading the generated `Switch`'s compile-time arm exhaustiveness — the one proof catching a case that gains a field — for a runtime cast, so the per-column declaration deriving both halves never lands. That pairing therefore rides one edit at two sites under one runtime gate, and sibling row records with their own projection delegates stay the deleted form: they scatter the dataset across two independently-editable OWNERS with no gate at all.
- [TEMPORAL_CATEGORY]: each family declares a temporal category, never a spine convenience. `element.assessments` is event-time and stamps the instant its work ran; every snapshot family is landing-time, since re-tabulating one frozen graph reproduces identical facts and the snapshot address already carries the version identity a consumer joins on. Tabulation instants on a snapshot re-date immutable evidence, and a receipt's arrival read as its work time inverts the same error.
- [ELEMENT_MODALITY_CLOSURE]: every `Seq` a baked `Element` carries reaches a dataset — co-applied classification references, property and quantity bags, material bindings, computed assessments, measured observation series, coverage bands — so no consumer re-folds the graph for a modality the egress skipped. `Parts` is the one exception the object row already answers, carrying the part count while each part tabulates as its own element. `Graph/element#NODE_MODEL` admitting a node case lands its row family here in the same pass; scalar element columns stay on the object row, needing no grain of their own.
- [PRODUCER_HALF]: `Rasm.Persistence` owns the branch's columnar plane, so this page hands a wire schema with typed rows and owns nothing physical. `TableType` mirrors the custodian's neutral token roster — token and the `Admits` predicate over `PropertyValue`, never a dialect spelling, an Arrow field, a binary-COPY wire type, or a plan literal.
- [CELL_CURRENCY]: `PropertyValue` carries every cell because the seam already owns it, so no second value vocabulary enters. Content keys cross as `Text` through `ContentAddress.ToValue`, the canonical X32 form the address owner fixes as the cross-runtime wire spelling, because a raw 128-bit number loses precision at a JSON boundary.
- [VERSION_PINNED_ROW]: every row leads with the snapshot `ContentAddress` and every family keys on it, so an analytic answer pins the model version it was computed over, a lake holds many versions of one model with no second identity axis, and a cross-family join resolves within one version by construction. Edge rows key on the edge's own content address rather than array position, so two structurally identical edges address as the one edge they are and array order never becomes a join key.
- [DERIVED_TABLE]: row families carry ZERO authority — graph and receipt stream own truth, a dropped dataset rebuilds at re-tabulation cost, and no path writes a row back into the graph. `Tabulate` runs pure over an already-frozen snapshot, `Bake` supplying every element-scoped row so type→occurrence inheritance applies exactly once and a table never disagrees with the element a consumer reads, while edges project raw because an edge inherits nothing.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
