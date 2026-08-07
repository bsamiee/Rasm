# [MATERIALS_ANALYTICS]

MATERIALS declares its analytics datasets as WIRE and projects catalogue, appearance, texture-plane, and environment-product truth onto flat row streams; `Rasm.Persistence` admits both across the `[WIRE]: AnalyticsSchema` seam as the branch's one columnar custodian. Column types, residences, dialects, DDL, slots, and the serving plane all home at that custodian, so this page names no provider, no residence, and no admitted schema type.

Settled composition: Component, Properties, Appearance, and observability owners supply already-admitted rows and receipts, and `ProjectionContext` carries the projection instant, operation key, and tenancy every stream stamps. Every dataset is EVENT-TIME and declares `observed` filled from the frame, so the Series, Fleet, and Lake residences provision one declaration and no dataset resolves against a single residence shape. Every row stream is a parameterized pure fold reaching no ambient registry.

## [01]-[INDEX]

- [02]-[DATASET_WIRE]: `ColumnToken` transcribes the custodian's physical types and `DatasetWire` carries each dataset's spine and admission projection.
- [03]-[DATASET_ROSTER]: `MaterialsDatasets` declares the `materials.<source>` datasets over one shared event-time spine.
- [04]-[ROW_PROJECTION]: `PropertyColumn` tables every selector, `EnvironmentProduct` closes the stored-product axis, and `AnalyticsProjection` folds each registered input — catalogue rows, property carriers, lifecycle stages, appearance summaries, capacity facts, texture-set wires at both grains, and environment-light wires — onto flat rows.

## [02]-[DATASET_WIRE]

- Owner: `ColumnToken` `[SmartEnum<string>]` — the producer's spelling of the custodian's closed physical-type vocabulary; `SpineToken` `[SmartEnum<string>]` — the same structural transcription of its temporal-category vocabulary; `DatasetColumn` — one named, typed, nullability-carrying column; `DatasetWire` — one dataset declaration carrying its key, its columns, its declared temporal category, its spine column, and the six-slot argument projection the custodian's admission gate consumes in field order.
- Entry: `DatasetWire.Admission` is the whole crossing — the composing root reads it off each roster row and hands it to the custodian's one schema gate, which proves every key, time, and measure name against the columns before a statement composes.
- Auto: `Spine` declares the temporal CATEGORY and `Time` names the column that category obliges — the custodian's gate proves the two agree and refuses either paired with the other's columns, so a declaration cannot claim event-time and hand over no observation column. `Time` is a plain column name rather than an option because this family is EVENT-TIME whole under the branch analytics ruling — a catalogue row carries no version key, so `observed` is what separates two projections of a changed catalogue and a capacity check owns the instant it ran; a Materials dataset without a declared instant is therefore unrepresentable rather than optional, and `Measure` stays optional because a rollup is meaningful only where one numeric column carries the dataset's whole magnitude.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new physical type is one `ColumnToken` row transcribed with the custodian's own row; a new temporal category is one `SpineToken` row transcribed the same way; a new dataset is one `DatasetWire` value at `[03]` with its row record and fold at `[04]`.
- Boundary: `ColumnToken` and `SpineToken` keys transcribe the custodian's vocabularies structurally because peers at one stratum never reference each other and no compiler spans the seam — a token this roster spells and the custodian never admits refuses at that gate rather than provisioning a column no dialect can render. Naming disambiguates at the source: the custodian owns `ColumnType`, `ColumnRow`, and `AnalyticsSchema`, and this page's declarations never wear those names.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------------------
using LanguageExt;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

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

// Structural transcription of the custodian's temporal-category vocabulary beside ColumnToken's physical one. The
// custodian's own gate turns this token into its `TimeSpine` row and REFUSES a dataset whose category and columns
// disagree — a producer stamping an observation column under the landing category defeats a category whose whole
// meaning is custodian admission, and an event-time dataset silently re-dated to admission strands every board
// joining two datasets on one axis. Materials declares `event` on every row and carries no landing dataset, yet
// SpineToken stays the roster's own column rather than a constant, because the category is the custodian's axis and a
// hardcoded literal here asserts a vocabulary this page does not own.
[SmartEnum<string>]
public sealed partial class SpineToken {
    public static readonly SpineToken Event = new("event");
    public static readonly SpineToken Landing = new("landing");
}

public sealed record DatasetWire(
    string Dataset, Seq<string> Key, Seq<DatasetColumn> Columns, SpineToken Spine, string Time, Option<string> Measure) {
    // Admission projects the SIX-slot handoff the custodian's `AnalyticsSchema.Admit(dataset, columns, key, spine,
    // time, measure)` consumes in field order. Its spine slot is mandatory there, so a five-slot projection composes no
    // gate at all and the column and the slot land together.
    public (string Dataset, Seq<(string Name, string Type, bool Nullable)> Columns,
        Seq<string> Key, string Spine, Option<string> Time, Option<string> Measure) Admission =>
        (Dataset, Columns.Map(static column => column.Wire), Key, Spine.Key, Some(Time), Measure);
}
```

## [03]-[DATASET_ROSTER]

- Owner: `MaterialsDatasets` — the dataset registry: `materials.component-rows` (catalogue identity, family and class discriminants, section pin, substance and appearance keys, IFC binding), `materials.property-rows` (admitted scalar and dimensioned property columns with their UCUM unit, evidence source, and expiry), `materials.sustainability` (per-stage GWP, resource fractions, classification, evidence source and expiry), `materials.library-summary` (the seam appearance scalars behind the content key), `materials.capacity-checks` (per-check verdict evidence off the fact stream, its optional ratio and its optional deferred member check), `materials.texture` (one row per baked or ingested set CHANNEL — its container, payload class, pyramid depth, stored blob and byte length, the set extent and tile verdict it belongs to, and the press evidence a baked set carries and an ingested one leaves absent), `materials.texture-set` (one row per SET — the grain every set-grained fact the evidence plane produces lands on: the tile verdict and its two component signals, the press backend, texel census, duration, downgrade count, and faulted-texel tally), `materials.environment` (one row per resolved light and stored PRODUCT — the equirect, specular chain, BRDF lookup, and optional luminance guide behind each dome, with the sky model and coefficient digest that keyed it).
- Entry: `MaterialsDatasets.All` is the roster the composing root enumerates; each declaration pairs one `[04]` row record and fold.
- Auto: identity and provenance ride as columns — classification system and code with evidence source and calendar expiry on the property and sustainability rows, the content-derived appearance key on library rows — so audit queries filter and expiry-screen without joining back into object graphs; `observed` trails every column list, so every declaration reads identity, then payload, then spine; the residence derives its own sort key from `Key` and `Time`, never from declaration order.
- Packages: LanguageExt.Core.
- Growth: a new dataset is one declaration, one row record, and one fold; a new column is one `DatasetColumn` with its field on the owning row record.
- Boundary: declaration truth and row truth stay co-located, so each dataset edit carries its matching row field and projection expression. `gwp`, `elapsed_s`, and `byte_length` are the measure names the family declares, because summing a mixed-unit long-form property column or a colour channel states a magnitude neither carries, while stored plane bytes sum to exactly the estate-wide texture footprint a storage question asks for. GRAIN decides whether a duration is summable: `materials.texture` excludes a set's OWN press duration because that duration repeats on every channel row of one press and summing it multiplies one bake by its channel count, while `materials.texture-set` declares it — one row IS one bake there, so the same column that lies at channel grain is the honest measure at set grain, which is exactly why the set-grained half is its own declaration rather than more columns on the channel one. `materials.environment` takes stored bytes and never the prefilter duration, under the same rule: a light's four products repeat one sweep's wall time.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------------------
using LanguageExt;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TABLES] -----------------------------------------------------------------------------------------
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
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Spine: SpineToken.Event, Time: "observed", Measure: None);

    public static readonly DatasetWire PropertyRows = new("materials.property-rows", Key: Seq("material", "property"), Seq(
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("property", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("unit", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("central", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("evidence_source", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("evidence_expiry", ColumnToken.Date, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Spine: SpineToken.Event, Time: "observed", Measure: None);

    public static readonly DatasetWire Sustainability = new("materials.sustainability", Key: Seq("material", "stage"), Seq(
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("basis", ColumnToken.Utf8, Nullable: false),
        // The seam LifecycleStage KEY, so the column's physical type is the token the fold writes: the projection
        // stamps `stage.Key` and an integer column would provision a width no producer ever fills.
        new DatasetColumn("stage", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("gwp", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("recycled", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("recovery", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("classification_system", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("classification_code", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("evidence_source", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("evidence_expiry", ColumnToken.Date, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Spine: SpineToken.Event, Time: "observed", Measure: None);

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
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Spine: SpineToken.Event, Time: "observed", Measure: None);

    // Keyed on the operation, the receipt kind, AND the governing action: one op resolving a section reports one
    // verdict per governing action, so the pair alone collides the moment a check reports both a flexure and a
    // shear verdict under one op and one kind — two real rows silently overwriting each other at the residence.
    // The governing column is the third discriminant the fact stream already carries. A per-MEMBER key is not
    // spellable here: `CapacityReceipt` carries no designation, section, or component identity on any case, so a
    // model checking two members of one kind under one op still collides — the residual cut belongs at the capacity
    // owner as an identity column, and this key is the honest bound of what the receipt declares today.
    public static readonly DatasetWire CapacityChecks = new("materials.capacity-checks", Key: Seq("op", "kind", "governing"), Seq(
        new DatasetColumn("op", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("kind", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("governing", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("adequate", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("utilisation", ColumnToken.Float64, Nullable: true),
        // Deferral names the member-level check a section pass still owes, so a query separates a clean pass from
        // a pass-with-deferral — two verdicts a bare adequacy bit renders identical.
        new DatasetColumn("deferral", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("elapsed_s", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Spine: SpineToken.Event, Time: "observed", Measure: "elapsed_s");

    // ONE ROW PER CHANNEL, keyed by the set and the channel it carries — the grain the estate's own storage takes
    // (the persistence landing partitions on `channel`), and the grain a real question asks: which materials carry a
    // normal plane, how many bytes each container costs across the estate, which sets never earned a tile proof. A
    // set-grained row answers none of those without unpacking a nested column every dialect renders differently. Press
    // columns stay NULLABLE because an ingested set was never pressed, a typed absence rather than a zero a rollup
    // sums into a fabricated bake cost.
    public static readonly DatasetWire TextureChannels = new("materials.texture", Key: Seq("set", "channel"), Seq(
        new DatasetColumn("set", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("appearance", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("channel", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("transfer", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("format", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("ktx_payload", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("block_format", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("mips", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("width", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("height", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("layers", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("tiled", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("blob", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("byte_length", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("backend", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("texels", ColumnToken.Int64, Nullable: true),
        new DatasetColumn("elapsed_s", ColumnToken.Float64, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)),
        Spine: SpineToken.Event, Time: "observed", Measure: "byte_length");

    // ONE ROW PER SET, the grain every set-grained texture fact lands on. Every tile and press column is NULLABLE
    // exactly as the channel twin's press columns are: an ingested set was never pressed and an ungraded set was
    // never tiled, so a zero strategy, a zero score, a zero backend, or a zero texel count reads to a cost or
    // quality query as a real bake and a real grading that measured nothing. The two component signals ride BESIDE the product
    // because the product alone tells an operator a tiling failed and never which half failed.
    public static readonly DatasetWire TextureSets = new("materials.texture-set", Key: Seq("set"), Seq(
        new DatasetColumn("set", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("appearance", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("material", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("channels", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("packs", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("tiled", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("tile_strategy", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("tile_score", ColumnToken.Float64, Nullable: true),
        new DatasetColumn("tile_seam_ratio", ColumnToken.Float64, Nullable: true),
        new DatasetColumn("tile_lattice_leak", ColumnToken.Float64, Nullable: true),
        new DatasetColumn("backend", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("texels", ColumnToken.Int64, Nullable: true),
        new DatasetColumn("elapsed_s", ColumnToken.Float64, Nullable: true),
        new DatasetColumn("downgraded", ColumnToken.Int64, Nullable: true),
        new DatasetColumn("faulted_texels", ColumnToken.Int64, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)),
        Spine: SpineToken.Event, Time: "observed", Measure: "elapsed_s");

    // ONE ROW PER LIGHT AND PRODUCT — the environment half of the estate-wide texture footprint, keyed the way the
    // channel dataset is so one storage question sums both. `sky_model` is EMPTY for an ingested HDRI, the spelling the
    // environment row itself publishes rather than a synthesized "none" this page would keep aligned; the coefficient
    // digest is the fit asset that keyed a synthesized dome, so a revised fit reads as new rows rather than as drift.
    public static readonly DatasetWire EnvironmentProducts = new("materials.environment", Key: Seq("light", "product"), Seq(
        new DatasetColumn("light", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("product", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("sky_model", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("coefficient_key", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("blob", ColumnToken.KeyHex, Nullable: false),
        new DatasetColumn("byte_length", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("specular_mips", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)),
        Spine: SpineToken.Event, Time: "observed", Measure: "byte_length");

    public static readonly Seq<DatasetWire> All = Seq(
        ComponentRows, PropertyRows, Sustainability, LibrarySummary, CapacityChecks,
        TextureChannels, TextureSets, EnvironmentProducts);
}
```

## [04]-[ROW_PROJECTION]

- Owner: `AnalyticsProjection` — the typed row records and the total folds from registered rows and typed receipts onto flat row streams; `PropertyColumn` — the selector table one scalar or dimensioned property occupies per row; `EnvironmentProduct` — the closed product axis one resolved light's stored blobs occupy, each row projecting its own wire key.
- Cases: `EnvironmentProduct` rows — `equirect` the authored dome, `specular` the prefiltered level chain, `brdfLut` the split-sum lookup, `luminanceCdf` the optional importance guide whose absent key emits no row at all.
- Entry: `Components` folds catalogue rows; `Properties` folds per-material property rows through the admitted `PropertyColumn` table; `Sustainability` folds one row per lifecycle stage; `Library` traverses material keys through an injected admitted appearance lookup; `Capacity` chooses capacity facts off the observability stream; `Textures` folds each `interchange#TEXTURE_EGRESS` `TextureSetWire`'s channel and pack rows into one row apiece; `TextureSets` folds the same wires at SET grain beside the tile evidence its caller pairs in; `Environments` fans each `EnvironmentLightWire` across the product axis against the store's own byte census. Every fold takes `ProjectionContext` and stamps `frame.At` onto its rows.
- Auto: a dimensioned selector reads its SI accessor off the quantity the `Published` carrier holds, so the magnitude and the UCUM unit it is stated in derive from one owner and no fold re-scales; folds are total over their registered inputs — an unregistered library key aborts the library fold typed rather than emitting a partial dataset.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, UnitsNet, BCL inbox.
- Growth: a new scalar or dimensioned property is one `PropertyColumn` row carrying its unit and its selector; a new environment product is one `EnvironmentProduct` row carrying its wire-key projection; a new dataset fold is one row record and one member beside its declaration.
- Boundary: ingress is parameterized — every fold takes its registered input and its frame as arguments and reads no ambient registry; egress is a row `Seq` the custodian batches, so buffer custody, batch sizing, and dataset writes never enter this page. Folds read the already-projected WIRE wherever one exists, so a warehouse column and the document a consumer decoded agree byte for byte; evidence no wire carries — a `TileReceipt`, a blob's stored length — enters as a SECOND ARGUMENT rather than as a re-derivation or a widened wire, and a row whose measured column has no producer is not emitted, because the alternative is a zero the measure sums.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------------------
using LanguageExt;
using NodaTime;                                  // Instant, LocalDate — the frame instant and the evidence expiry
using Rasm.Domain;                               // Op — the correlation key the frame carries into every lookup
using Rasm.Element.Classification;               // Classification — the catalogue's own standard-reference egress
using Rasm.Element.Composition;                  // MaterialId, MaterialPropertySet, PropertyEvidence, LifecycleStage,
                                                 // MaterialPropertyAccess — the seam discipline accessors the table composes
using Rasm.Element.Projection;                   // ProjectionContext — the frame every fold stamps
using Rasm.Materials.Appearance;                 // AppearanceSummary
using Rasm.Materials.Appearance.Interchange;     // TextureSetWire, EnvironmentLightWire — the already-projected wires
using Rasm.Materials.Component;                  // ComponentRow, QuantityRow, Utilisation
using Rasm.Materials.Raster;                     // TileReceipt, TileScore, PlaneTransfer, BlockFormat, KtxPayload
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [MODELS] -----------------------------------------------------------------------------------------
public readonly record struct ComponentAnalyticsRow(
    string Component, string Family, string Class, bool Sectioned, string Substance, string Appearance,
    string IfcEntity, string Predefined, Instant Observed);

public readonly record struct PropertyAnalyticsRow(
    string Material, string Property, string Unit, double Central,
    string EvidenceSource, Option<LocalDate> EvidenceExpiry, Instant Observed);

public readonly record struct SustainabilityAnalyticsRow(
    string Material, string Basis, string Stage, double Gwp, double Recycled, double Recovery,
    Option<string> ClassificationSystem, Option<string> ClassificationCode,
    string EvidenceSource, Option<LocalDate> EvidenceExpiry, Instant Observed);

public readonly record struct LibrarySummaryRow(
    string Material, string AppearanceKey, double BaseR, double BaseG, double BaseB,
    double Metallic, double Roughness, double Opacity, bool Transmissive, Instant Observed);

// Utilisation reads the verdict's OWN optional ratio, so the unbounded case contributes absence rather than a
// zero and no reader re-enumerates which cases happen to hold a value. Deferral names the member-level check a
// section pass still owes.
public readonly record struct CapacityCheckRow(
    string Op, string Kind, string Governing, bool Adequate, Option<double> Utilisation, Option<string> Deferral,
    double ElapsedSeconds, Instant Observed);

// One channel of one set. Press columns are Option-typed: an ingested set was never pressed, and a zero backend,
// zero texel count, or zero duration would read to a cost query as a real bake that took no time.
public readonly record struct TextureChannelAnalyticsRow(
    string Set, string Appearance, Option<string> Material, string Channel, string Transfer, string Format,
    Option<string> KtxPayload, Option<string> BlockFormat, int Mips, int Width, int Height, int Layers, bool Tiled,
    string Blob, long ByteLength, Option<string> Backend, Option<long> Texels, Option<double> ElapsedSeconds,
    Instant Observed);

// One SET. The tile columns are Option-typed for the reason the press columns are: an ungraded set carries no
// strategy and no score, and a zero product would read to a quality query as the worst tiling in the estate
// rather than as one nobody graded.
public readonly record struct TextureSetAnalyticsRow(
    string Set, string Appearance, Option<string> Material, int Channels, int Packs, bool Tiled,
    Option<string> TileStrategy, Option<double> TileScore, Option<double> TileSeamRatio, Option<double> TileLatticeLeak,
    Option<string> Backend, Option<long> Texels, Option<double> ElapsedSeconds,
    Option<long> Downgraded, Option<long> FaultedTexels, Instant Observed);

// One STORED PRODUCT of one resolved light. Every column is present by construction: a product whose blob the
// store census cannot price emits no row, so `byte_length` is always a measured length.
public readonly record struct EnvironmentProductRow(
    string Light, string Product, string SkyModel, string CoefficientKey, string Blob, long ByteLength,
    int SpecularMips, Instant Observed);

// --- [OPERATIONS] -------------------------------------------------------------------------------------
// ONE selector table over the ADMITTED seam cases, never over the ingress row. The catalogue's own Admit fold is
// where a published magnitude becomes a typed measure, so a warehouse column read straight off the raw row
// publishes a number no admission gate ever saw — a row the catalogue REFUSED would still reach a dashboard,
// with the graph beside it carrying nothing. Reading the admitted set is also what makes the columns TOTAL over
// the disciplines: the mechanical triple and every acoustic, fire, hygrothermal, and optical column comes off a
// case the same set carries, where the ingress record exposed the seven scalars one shape happened to declare
// and silently omitted every case the catalogue lowers beside it. `unit` DERIVES from the QuantityRow the
// measure was minted through, so the long-form `central` column is self-describing under the same registry the
// mint used and no reader infers a scale from a property name — a hand-typed unit string beside a typed mint is
// the second spelling that drifts. A column ABSENT from a material's set emits NO row: absence is a discipline
// the catalogue carries no data for, and a zero in a long-form warehouse reads as a measured zero. Case selection
// composes the seam's OWN `MaterialPropertyAccess` extension — `sets.Mechanical`, `sets.Optical`,
// `sets.Hygrothermal` — so a discipline reader is one member on the owner that publishes it, never a local
// `Choose` re-spelling a projection the seam already carries and would drift from on the next case it adds.
public sealed record PropertyColumn(
    string Property, Option<QuantityRow> Quantity, string Dimensionless,
    Func<Seq<MaterialPropertySet>, Option<double>> Central) {

    // A DIMENSIONED column reads its unit off the QuantityRow it minted through — one registry, one spelling, and
    // a hand-typed unit string beside a typed mint is the second spelling that drifts. A DIMENSIONLESS column has
    // no row to read, because the registry rosters SI dimensions and a ratio carries none, so its UCUM literal is
    // the honest source rather than a registry row minted to carry a unit string it has no dimension for.
    public string Unit => Quantity.Map(static row => row.Unit).IfNone(Dimensionless);

    public static readonly Seq<PropertyColumn> Rows = Seq(
        // The MECHANICAL group, read off the admitted case rather than off loose ingress scalars: density,
        // modulus, yield, ultimate, Poisson, and expansion all travel on ONE case, so a material carrying a grade
        // carries the whole group and a query never sees three of six columns.
        new PropertyColumn("density", Some(QuantityRow.Density), "1", static sets => sets.Mechanical.Map(static m => m.Density.Si)),
        new PropertyColumn("modulus", Some(QuantityRow.Pressure), "1", static sets => sets.Mechanical.Map(static m => m.YoungsModulus.Si)),
        new PropertyColumn("yield_strength", Some(QuantityRow.Pressure), "1", static sets => sets.Mechanical.Map(static m => m.YieldStrength.Si)),
        new PropertyColumn("ultimate_strength", Some(QuantityRow.Pressure), "1", static sets => sets.Mechanical.Map(static m => m.UltimateStrength.Si)),
        new PropertyColumn("poisson", None, "1", static sets => sets.Mechanical.Map(static m => m.PoissonsRatio)),
        new PropertyColumn("expansion_per_k", None, "/K", static sets => sets.Mechanical.Map(static m => m.ThermalExpansionPerK)),
        // The THERMAL group — conductivity, specific heat, transmittance, and vapour resistance ride one case
        // exactly as the seam admission seats them, so no column here re-homes a seam decision.
        new PropertyColumn("conductivity", Some(QuantityRow.ThermalConductivity), "1", static sets => sets.Thermal.Map(static t => t.Conductivity.Si)),
        new PropertyColumn("specific_heat", Some(QuantityRow.SpecificEntropy), "1", static sets => sets.Thermal.Map(static t => t.SpecificHeat.Si)),
        // Transmittance is OPTIONAL AT THE SEAM because a SUBSTANCE declares none — U-value belongs to the EN ISO
        // 6946 assembly fold at an installed thickness — so the column reads THROUGH that option and a substance
        // carrying no transmittance emits no row. Zero-filling would publish a perfectly conducting substance under
        // a measured column, and deleting the column instead would strand every row that genuinely declares one.
        new PropertyColumn("u_value", Some(QuantityRow.HeatTransferCoefficient), "1",
            static sets => sets.Thermal.Bind(static t => t.UValue).Map(static v => v.Si)),
        new PropertyColumn("vapour_mu", None, "1", static sets => sets.Thermal.Map(static t => t.VapourResistanceFactor)),
        // The two disciplines the ingress row exposed NO column for at all. Each is one defining scalar the seam
        // case already carries, so a query asking which materials carry a fire rating or an acoustic absorption
        // answers off the same long-form table as one asking which carry a density.
        new PropertyColumn("nrc", None, "1", static sets => sets.Acoustic.Map(static a => a.Nrc)),
        new PropertyColumn("fire_resistance_minutes", None, "min",
            // R is the load-bearing criterion a structural query asks for; a material tested on E/I alone declares
            // no R and emits no row rather than publishing a separating rating under a load-bearing column.
            static sets => sets.Fire.Bind(static f => f.Resistance.LoadBearingMinutes).Map(static minutes => (double)minutes)),
        // The HYGROTHERMAL group. Porosity and the two storage anchors are the columns a moisture-risk query reads;
        // the A-value is genuinely optional at the seam and stays a column that emits no row where a material
        // declares none. The three SampledCurve members carry no scalar and therefore no long-form column at all —
        // a curve is not a magnitude, and flattening one to its midpoint would publish a number nothing measured.
        new PropertyColumn("porosity", None, "1", static sets => sets.Hygrothermal.Map(static h => h.Porosity)),
        new PropertyColumn("water_content_80rh", None, "kg/m3", static sets => sets.Hygrothermal.Map(static h => h.WaterContent80Rh.Si)),
        new PropertyColumn("free_water_saturation", None, "kg/m3", static sets => sets.Hygrothermal.Map(static h => h.FreeWaterSaturation.Si)),
        new PropertyColumn("water_absorption", None, "kg/(m2.s0.5)", static sets => sets.Hygrothermal.Bind(static h => h.WaterAbsorptionKgPerM2SqrtS)),
        // The OPTICAL group — nine stored fractions, each its own column because front and back genuinely differ on
        // a coated pane and a query asking which side carries the coating cannot recover that from one averaged
        // number. The two solar absorptances are DERIVED at the seam from the stored triple, so publishing them
        // would carry one truth twice and drift the day the seam changes the identity.
        new PropertyColumn("visible_transmittance", None, "1", static sets => sets.Optical.Map(static o => o.VisibleTransmittance)),
        new PropertyColumn("visible_reflectance_front", None, "1", static sets => sets.Optical.Map(static o => o.VisibleReflectanceFront)),
        new PropertyColumn("visible_reflectance_back", None, "1", static sets => sets.Optical.Map(static o => o.VisibleReflectanceBack)),
        new PropertyColumn("solar_transmittance", None, "1", static sets => sets.Optical.Map(static o => o.SolarTransmittance)),
        new PropertyColumn("solar_reflectance_front", None, "1", static sets => sets.Optical.Map(static o => o.SolarReflectanceFront)),
        new PropertyColumn("solar_reflectance_back", None, "1", static sets => sets.Optical.Map(static o => o.SolarReflectanceBack)),
        new PropertyColumn("ir_transmittance", None, "1", static sets => sets.Optical.Map(static o => o.ThermalIrTransmittance)),
        new PropertyColumn("ir_emissivity_front", None, "1", static sets => sets.Optical.Map(static o => o.ThermalIrEmissivityFront)),
        new PropertyColumn("ir_emissivity_back", None, "1", static sets => sets.Optical.Map(static o => o.ThermalIrEmissivityBack)),
        // The DAMPING group is ONE column: the ratio is the catalogue datum, the structural loss factor derives
        // from it at the seam, and the Rayleigh pair is a per-model FE input no catalogue declares.
        new PropertyColumn("damping_ratio", None, "1", static sets => sets.Damping.Map(static d => d.DampingRatio)));
}

// Rows close the product axis one resolved light's stored blobs occupy, each projecting its own wire key off the
// already-projected light, so the environment fan is one Items sweep and a fifth product lands as one row rather
// than a fifth arm. Wire spells an ABSENT blob as the empty key (the luminance guide a dome may not carry), so
// absence is the key's own emptiness and no row carries a synthesized sentinel.
[SmartEnum<string>]
public sealed partial class EnvironmentProduct {
    public static readonly EnvironmentProduct Equirect = new("equirect", static light => light.EquirectKey);
    public static readonly EnvironmentProduct Specular = new("specular", static light => light.SpecularKey);
    public static readonly EnvironmentProduct BrdfLut = new("brdfLut", static light => light.BrdfLutKey);
    public static readonly EnvironmentProduct LuminanceCdf = new("luminanceCdf", static light => light.LuminanceCdfKey);

    [UseDelegateFromConstructor]
    public partial string Blob(EnvironmentLightWire light);
}

public static class AnalyticsProjection {
    public static Seq<ComponentAnalyticsRow> Components(Seq<ComponentRow> rows, ProjectionContext frame) =>
        rows.Map(row => new ComponentAnalyticsRow(
            row.Item.Designation.Value, row.Item.Family.Key, row.Item.Class.Key, row.Sectioned,
            row.Item.SubstanceId.Value, row.Item.AppearanceId.Value, row.Item.IfcEntity,
            row.Item.PredefinedToken, frame.At));

    // The ADMITTED set per material, never the ingress row: every value here already crossed the catalogue's own
    // admission, so the warehouse and the graph publish the same numbers. Evidence rides the CASE that produced
    // the column, because two disciplines on one material carry two different declarations and a row-level
    // evidence stamp would attribute a thermal figure to the mechanical source that never published it.
    // Magnitude and declaration come off ONE walk: the first admitted set the column reads answers both, so the
    // selector runs once per set rather than once for the value and again to recover the case that produced it.
    // The hoist is CORRECTNESS before it is cost — two independent walks can answer a magnitude off one case and a
    // declaration off another the day a material carries two sets one column reads, publishing a figure under a
    // source that never stated it. A column no case produces answers ABSENCE and emits no row, so no neutral
    // evidence value is ever fabricated for a row that has none.
    public static Seq<PropertyAnalyticsRow> Properties(
        Seq<(MaterialId Id, Seq<MaterialPropertySet> Admitted)> rows, ProjectionContext frame) =>
        rows.Bind(entry => PropertyColumn.Rows.Choose(column => entry.Admitted
            .Choose(set => column.Central(Seq(set)).Map(central => (Central: central, set.Evidence)))
            .Head
            .Map(read => new PropertyAnalyticsRow(entry.Id.Value, column.Property, column.Unit, read.Central,
                read.Evidence.Source, read.Evidence.ValidUntil, frame.At))));

    // The ADMITTED Environmental case, never the ingress vector: the catalogue's Lower embeds the carbon-only
    // vector into the seam's full impact matrix under its own arity and finiteness gates, so reading the raw
    // StageGwp array publishes magnitudes that never crossed them. Stage crosses as the seam LifecycleStage KEY
    // rather than as the vector's ordinal — an ordinal is positional truth that silently re-points every stored
    // row the day the seam adds an EN 15978 module, while the key survives the insertion.
    // Classification travels BESIDE the admitted set rather than off it: the seam carries classification as its own
    // Object-node value object and never as a MaterialPropertySet case, so reading it off the Environmental case
    // would name a column the seam does not declare. The caller supplies the catalogue's own Classification egress.
    public static Seq<SustainabilityAnalyticsRow> Sustainability(
        Seq<(MaterialId Id, Seq<MaterialPropertySet> Admitted, Option<Classification> Classification)> rows, ProjectionContext frame) =>
        rows.Bind(entry => entry.Admitted
            .Choose(static set => set as MaterialPropertySet.Environmental)
            .Bind(environmental => toSeq(LifecycleStage.Items).Map(stage =>
                new SustainabilityAnalyticsRow(
                    entry.Id.Value, environmental.Basis.Key, stage.Key, environmental.StageAt(stage),
                    environmental.RecycledContent, environmental.EndOfLifeRecovery,
                    entry.Classification.Map(static c => c.System),
                    entry.Classification.Map(static c => c.Code),
                    environmental.Evidence.Source, environmental.Evidence.ValidUntil, frame.At))));

    public static Fin<Seq<LibrarySummaryRow>> Library(
        Seq<MaterialId> materials, ProjectionContext frame,
        Func<MaterialId, Op, Fin<AppearanceSummary>> lookup) =>
        materials.TraverseM(id => lookup(id, frame.Key).Map(summary =>
            new LibrarySummaryRow(
                // X32 — the wire spelling. A warehouse column is a WIRE VALUE the texture dataset's own
                // TextureSetWire.AppearanceKey joins against; lowering here forks the one join both datasets exist
                // to serve, and the path-segment lowering happens at egress-name construction alone.
                id.Value, summary.AppearanceKey.ToString("X32"), summary.BaseColorR, summary.BaseColorG,
                summary.BaseColorB, summary.Metallic, summary.Roughness, summary.Opacity,
                summary.Transmissive, frame.At))).As();

    // Textures reads the already-projected TextureSetWire rather than the TextureSet: every column it needs is a wire
    // column the interchange projection rendered once — the egress leaf, the payload class, the lowered digests — so the
    // analytics row re-derives NOTHING and a set's storage footprint in a warehouse matches the document its consumers
    // decoded byte for byte. Packed sheets fold beside standalone channels under their own pack name in the `channel`
    // slot, because a query asking which sets carry roughness must see an `orm` sheet as the row that carries it rather
    // than as a set with no roughness at all.
    public static Seq<TextureChannelAnalyticsRow> Textures(Seq<TextureSetWire> sets, ProjectionContext frame) =>
        sets.Bind(set => toSeq(set.Channels)
            // ONE spelling of absence: a non-KTX2 channel's wire "none" lowers to the SAME typed absence the pack
            // rows carry — otherwise a payload grouping counts the literal string "none" as a declaration while
            // the pack rows' NULL sits outside it, and the query the design justifies itself with double-counts.
            .Map(row => Row(set, row.Role, row.Transfer, row.Format,
                row.KtxPayload == KtxPayload.None.Key ? None : Some(row.KtxPayload),
                row.BlockFormat == BlockFormat.None.Key ? None : Some(row.BlockFormat),
                (int)row.Mips, row.Blob, row.ByteLength, frame))
            // Packs declare NO single payload class or block format: a sheet's lanes carry three different channels'
            // policies, so the two columns stay absent rather than filled with one lane's value promoted to speak for the
            // sheet — a query grouping by payload then counts real declarations alone. The two row streams CONCATENATE:
            // `Append` seats ONE element beside a sequence, so it cannot take the pack stream at all.
            + toSeq(set.Packs).Map(pack => Row(set, pack.Pack, PlaneTransfer.Raw.Key, pack.Format,
                None, None, (int)pack.Mips, pack.Blob, pack.ByteLength, frame)));

    static TextureChannelAnalyticsRow Row(
        TextureSetWire set, string channel, string transfer, string format, Option<string> payload, Option<string> block,
        int mips, string blob, ulong byteLength, ProjectionContext frame) =>
        new(Set: set.SetKey, Appearance: set.AppearanceKey,
            Material: string.IsNullOrEmpty(set.MaterialId) ? None : Some(set.MaterialId),
            Channel: channel, Transfer: transfer, Format: format, KtxPayload: payload, BlockFormat: block,
            Mips: mips, Width: (int)set.Width, Height: (int)set.Height, Layers: (int)set.Layers, Tiled: set.Tiled,
            Blob: blob, ByteLength: (long)byteLength,
            Backend: set.Press is { } press ? Some(press.Backend) : None,
            Texels: set.Press is { } counted ? Some((long)counted.Texels) : None,
            ElapsedSeconds: set.Press is { } timed ? Some(timed.ElapsedMs / 1000.0) : None,
            Observed: frame.At);

    public static Seq<CapacityCheckRow> Capacity(Seq<MaterialsFact> facts, ProjectionContext frame) =>
        facts.Choose(fact => fact as MaterialsFact.CapacityCheck).Map(check =>
            new CapacityCheckRow(
                // Op is a [ValueObject<string>], so the admitted key reads back as Value — the generated accessor
                // over the key member, never a Name property the type does not declare.
                check.Key.Value, check.Receipt.Kind, check.Verdict.Governing.Key,
                check.Verdict.Adequate,
                // Verdict projects its OWN optional ratio: two cases carry one demand-over-capacity number while
                // an unbounded verdict carries none, so a reader re-enumerating which cases hold a value strands
                // whatever ratio a deferring verdict carries and re-forks whenever the owner mints a fourth case.
                check.Verdict.Ratio,
                check.Verdict is Utilisation.RequiresMemberCheck deferred
                    ? Some(deferred.Requirement.Key)
                    : Option<string>.None,
                check.Elapsed.TotalSeconds, frame.At));

    // Set grain takes the tile evidence as a SECOND ARGUMENT because a TileProof is not a wire column — the wire
    // carries the boolean projection of the proof's presence and never the score behind it — so pairing the
    // receipt in keeps the two component signals honest without widening the document.
    public static Seq<TextureSetAnalyticsRow> TextureSets(
        Seq<(TextureSetWire Set, Option<TileReceipt> Tile)> sets, ProjectionContext frame) =>
        sets.Map(entry => (entry.Set, Strategy: entry.Tile.Map(static receipt => receipt.Strategy.Key), Score: Scored(entry.Tile)))
            .Map(row => new TextureSetAnalyticsRow(
                Set: row.Set.SetKey, Appearance: row.Set.AppearanceKey,
                Material: string.IsNullOrEmpty(row.Set.MaterialId) ? None : Some(row.Set.MaterialId),
                Channels: row.Set.Channels.Length, Packs: row.Set.Packs.Length, Tiled: row.Set.Tiled,
                TileStrategy: row.Strategy,
                TileScore: row.Score.Map(static score => score.Value),
                TileSeamRatio: row.Score.Map(static score => score.SeamRatio),
                TileLatticeLeak: row.Score.Map(static score => score.LatticeLeak),
                Backend: row.Set.Press is { } press ? Some(press.Backend) : None,
                Texels: row.Set.Press is { } counted ? Some((long)counted.Texels) : None,
                ElapsedSeconds: row.Set.Press is { } timed ? Some(timed.ElapsedMs / 1000.0) : None,
                // Quality tallies ride the press receipt's own wire columns, so a set-grained quality query and the
                // channel-keyed press counters at `observability#INSTRUMENT_TAP` answer the same numbers on two
                // grains rather than one number re-derived twice.
                Downgraded: row.Set.Press is { } fell ? Some((long)fell.Downgraded) : None,
                FaultedTexels: row.Set.Press is { } faulted ? Some((long)faulted.FaultedTexels) : None,
                Observed: frame.At));

    // The receipt's score is the OPTION the gate answered with, so the triple filters on PRESENCE once here rather
    // than per column. Absence means one thing: the second signal could not be measured. A LOW score is not absence
    // — a graded plane that fell short carries the real number it earned and belongs in the warehouse, because a
    // quality query asking how far short an estate's tilings fall cannot answer off rows that dropped themselves.
    static Option<TileScore> Scored(Option<TileReceipt> tile) => tile.Bind(static receipt => receipt.Score);

    // Environments fans the CLOSED product axis against the store's own byte census keyed by the same X32 blob
    // spelling the wire carries, so the join a footprint query runs is the join the fold already ran. A product
    // whose key is empty (an absent luminance guide) or whose blob the census cannot price emits NO ROW — a zero
    // length would sum into the estate footprint as a stored object nobody stored.
    public static Seq<EnvironmentProductRow> Environments(
        Seq<(EnvironmentLightWire Light, HashMap<string, long> Stored)> lights, ProjectionContext frame) =>
        lights.Bind(entry => toSeq(EnvironmentProduct.Items).Choose(product =>
            product.Blob(entry.Light) is { Length: > 0 } blob
            && entry.Stored.Find(blob) is { IsSome: true, Case: long bytes }
                ? Some(new EnvironmentProductRow(
                    Light: entry.Light.Key, Product: product.Key, SkyModel: entry.Light.SkyModel,
                    CoefficientKey: entry.Light.CoefficientKey, Blob: blob, ByteLength: bytes,
                    SpecularMips: entry.Light.SpecularMips, Observed: frame.At))
                : Option<EnvironmentProductRow>.None));
}
```

## [05]-[RESEARCH]

(none)
