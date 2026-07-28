# [MATERIALS_ANALYTICS]

MATERIALS declares its analytics datasets as WIRE and projects catalogue, appearance, and texture-plane truth onto flat row streams; `Rasm.Persistence` admits both across the `[WIRE]: AnalyticsSchema` seam as the branch's one columnar custodian. Column types, residences, dialects, DDL, slots, and the serving plane all home at that custodian, so this page names no provider, no residence, and no admitted schema type.

Settled composition: Component, Properties, Appearance, and observability owners supply already-admitted rows and receipts, and `ProjectionContext` carries the projection instant, operation key, and tenancy every stream stamps. Every dataset is EVENT-TIME and declares `observed` filled from the frame, so the Series, Fleet, and Lake residences provision one declaration and no dataset resolves against a single residence shape. Every row stream is a parameterized pure fold reaching no ambient registry.

## [01]-[INDEX]

- [02]-[DATASET_WIRE]: `ColumnToken` transcribes the custodian's physical types and `DatasetWire` carries each dataset's spine and admission projection.
- [03]-[DATASET_ROSTER]: `MaterialsDatasets` declares the `materials.<source>` datasets over one shared event-time spine.
- [04]-[ROW_PROJECTION]: `PropertyColumn` tables every selector and `AnalyticsProjection` folds each registered input — catalogue rows, property carriers, lifecycle stages, appearance summaries, capacity facts, and texture-set wires — onto flat rows.

## [02]-[DATASET_WIRE]

- Owner: `ColumnToken` `[SmartEnum<string>]` — the producer's spelling of the custodian's closed physical-type vocabulary; `SpineToken` `[SmartEnum<string>]` — the same structural transcription of its temporal-category vocabulary; `DatasetColumn` — one named, typed, nullability-carrying column; `DatasetWire` — one dataset declaration carrying its key, its columns, its declared temporal category, its spine column, and the six-slot argument projection the custodian's admission gate consumes in field order.
- Entry: `DatasetWire.Admission` is the whole crossing — the composing root reads it off each roster row and hands it to the custodian's one schema gate, which proves every key, time, and measure name against the columns before a statement composes.
- Auto: `Spine` declares the temporal CATEGORY and `Time` names the column that category obliges — the custodian's gate proves the two agree and refuses either paired with the other's columns, so a declaration cannot claim event-time and hand over no observation column. `Time` is a plain column name rather than an option because this family is EVENT-TIME whole under the branch analytics ruling — a catalogue row carries no version key, so `observed` is what separates two projections of a changed catalogue and a capacity check owns the instant it ran; a Materials dataset without a declared instant is therefore unrepresentable rather than optional, and `Measure` stays optional because a rollup is meaningful only where one numeric column carries the dataset's whole magnitude.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new physical type is one `ColumnToken` row transcribed with the custodian's own row; a new temporal category is one `SpineToken` row transcribed the same way; a new dataset is one `DatasetWire` value at `[03]` with its row record and fold at `[04]`.
- Boundary: `ColumnToken` and `SpineToken` keys transcribe the custodian's vocabularies structurally because peers at one stratum never reference each other and no compiler spans the seam — a token this roster spells and the custodian never admits refuses at that gate rather than provisioning a column no dialect can render. Naming disambiguates at the source: the custodian owns `ColumnType`, `ColumnRow`, and `AnalyticsSchema`, and this page's declarations never wear those names.

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

- Owner: `MaterialsDatasets` — the dataset registry: `materials.component-rows` (catalogue identity, family and class discriminants, section pin, substance and appearance keys, IFC binding), `materials.property-rows` (admitted scalar and dimensioned property columns with their UCUM unit, evidence source, and expiry), `materials.sustainability` (per-stage GWP, resource fractions, classification, evidence source and expiry), `materials.library-summary` (the seam appearance scalars behind the content key), `materials.capacity-checks` (per-check verdict evidence off the fact stream), `materials.texture` (one row per baked or ingested set CHANNEL — its container, payload class, pyramid depth, stored blob and byte length, the set extent and tile verdict it belongs to, and the press evidence a baked set carries and an ingested one leaves absent).
- Entry: `MaterialsDatasets.All` is the roster the composing root enumerates; each declaration pairs one `[04]` row record and fold.
- Auto: identity and provenance ride as columns — classification system and code with evidence source and calendar expiry on the property and sustainability rows, the content-derived appearance key on library rows — so audit queries filter and expiry-screen without joining back into object graphs; `observed` trails every column list, so every declaration reads identity, then payload, then spine; the residence derives its own sort key from `Key` and `Time`, never from declaration order.
- Packages: LanguageExt.Core.
- Growth: a new dataset is one declaration, one row record, and one fold; a new column is one `DatasetColumn` with its field on the owning row record.
- Boundary: declaration truth and row truth stay co-located, so each dataset edit carries its matching row field and projection expression. `gwp`, `elapsed_s`, and `byte_length` are the three measures the family declares, because summing a mixed-unit long-form property column or a colour channel states a magnitude neither carries, while stored plane bytes sum to exactly the estate-wide texture footprint a storage question asks for. `materials.texture` deliberately excludes a set's OWN press duration from its measure: that duration repeats on every channel row of one press, so summing it multiplies one bake by its channel count.

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
        new DatasetColumn("stage", ColumnToken.Int64, Nullable: false),
        new DatasetColumn("gwp", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("recycled", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("recovery", ColumnToken.Float64, Nullable: false),
        new DatasetColumn("classification_system", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("classification_code", ColumnToken.Utf8, Nullable: true),
        new DatasetColumn("evidence_source", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("evidence_expiry", ColumnToken.Date, Nullable: true),
        new DatasetColumn("observed", ColumnToken.Timestamp, Nullable: false)), Spine: SpineToken.Event, Time: "observed", Measure: "gwp");

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

    public static readonly DatasetWire CapacityChecks = new("materials.capacity-checks", Key: Seq("op", "kind"), Seq(
        new DatasetColumn("op", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("kind", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("governing", ColumnToken.Utf8, Nullable: false),
        new DatasetColumn("adequate", ColumnToken.Bool, Nullable: false),
        new DatasetColumn("utilisation", ColumnToken.Float64, Nullable: true),
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

    public static readonly Seq<DatasetWire> All =
        Seq(ComponentRows, PropertyRows, Sustainability, LibrarySummary, CapacityChecks, TextureChannels);
}
```

## [04]-[ROW_PROJECTION]

- Owner: `AnalyticsProjection` — the typed row records and the total folds from registered rows and typed receipts onto flat row streams; `PropertyColumn` — the selector table one scalar or dimensioned property occupies per row.
- Entry: `Components` folds catalogue rows; `Properties` folds per-material property rows through the admitted `PropertyColumn` table; `Sustainability` folds one row per lifecycle stage; `Library` traverses material keys through an injected admitted appearance lookup; `Capacity` chooses capacity facts off the observability stream; `Textures` folds each `interchange#TEXTURE_EGRESS` `TextureSetWire`'s channel and pack rows into one row apiece. Every fold takes `ProjectionContext` and stamps `frame.At` onto its rows.
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

// One channel of one set. Press columns are Option-typed: an ingested set was never pressed, and a zero backend,
// zero texel count, or zero duration would read to a cost query as a real bake that took no time.
public readonly record struct TextureChannelAnalyticsRow(
    string Set, string Appearance, Option<string> Material, string Channel, string Transfer, string Format,
    Option<string> KtxPayload, Option<string> BlockFormat, int Mips, int Width, int Height, int Layers, bool Tiled,
    string Blob, long ByteLength, Option<string> Backend, Option<long> Texels, Option<double> ElapsedSeconds,
    Instant Observed);

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
                row.KtxPayload == "none" ? None : Some(row.KtxPayload),  // "none" is the frozen wire literal for a non-KTX2 file; the KtxPayload roster carries no such row
                row.BlockFormat == BlockFormat.None.Key ? None : Some(row.BlockFormat),
                (int)row.Mips, row.Blob, row.ByteLength, frame))
            // Packs declare NO single payload class or block format: a sheet's lanes carry three different channels'
            // policies, so the two columns stay absent rather than filled with one lane's value promoted to speak for the
            // sheet — a query grouping by payload then counts real declarations alone.
            .Append(toSeq(set.Packs).Map(pack => Row(set, pack.Pack, PlaneTransfer.Raw.Key, pack.Format,
                None, None, (int)pack.Mips, pack.Blob, pack.ByteLength, frame))));

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
