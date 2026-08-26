# [MATERIALS_ANALYTICS]

MATERIALS declares its analytics datasets as WIRE and projects catalogue, appearance, texture-plane, and environment-product truth onto flat row streams; `Rasm.Persistence` admits both across the `[WIRE]: AnalyticsSchema` boundary as the branch's one columnar custodian. Column types, storage tiers, dialects, DDL, slots, and the serving plane all home at that custodian, so this page names no provider, no storage tier, and no admitted schema type.

Settled composition: the declaration vocabulary is `Rasm.Element/Graph/table#DATASET_ROSTER`'s — `TableType` carries the physical tokens with their `Admits` predicate, `TableColumn` one named typed column, `TableSpine` the temporal category fused to the clock column it implies — so this folder declares `materials.<source>` rows over that vocabulary and mints none of its own.

Component, Properties, Appearance, and observability owners supply already-admitted rows and run records, and `ProjectionContext` carries the projection instant, operation key, and tenancy every stream stamps. Every dataset is EVENT-TIME, so the Series, Fleet, and Lake storage tiers provision one declaration. Every row stream is a parameterized pure fold reaching no ambient registry.

## [01]-[INDEX]

- [02]-[DATASET_ROSTER]: `MaterialsDataset` declares the `materials.<source>` datasets over the Element column vocabulary, each row carrying its key, columns, spine, and rollup measure, and projecting the custodian's admission handoff.
- [03]-[ROW_PROJECTION]: `PropertyColumn` tables every selector, and `AnalyticsProjection` folds each registered input — catalogue rows, property carriers, lifecycle stages, appearance summaries, capacity facts, generated surface sets at set and stored-level grains, and generated environment products — onto flat rows.

## [02]-[DATASET_ROSTER]

- Owner: `MaterialsDataset` — the `[SmartEnum<string>]` dataset roster whose key IS the dotted `materials.<source>` dataset name, each row carrying its ordered `TableColumn` set, its `KeyColumns` identity, its `TableSpine` category, and the optional `Measure` a rollup folds; the row derives its Element `TableDeclaration`, and `Wire`/`Admission`/`Conforms` are that contract owner's own members, never local twins.
- Cases: `materials.component-rows` (catalogue identity, family and class discriminants, section pin, substance and appearance keys, IFC binding), `materials.property-rows`, `materials.sustainability`, `materials.library-summary`, `materials.capacity-checks`, `materials.texture` (one row per baked or ingested set CHANNEL), `materials.texture-set` (one row per SET), `materials.environment` (one row per resolved light and stored PRODUCT). Each row's columns, grain, and measure are the DECLARATION below — the code row is the authority, and restating a dataset's column list in prose publishes a second roster that drifts on the first column edit.
- Entry: `MaterialsDataset.Rows` is the roster the composing root enumerates; `row.Declaration.Admission` is the whole crossing, splatted into the custodian's one schema gate, which proves every key, time, and measure name against the columns before a statement composes; the batch crossing is `TableBatch(row.Declaration, rows)`.
- Auto: `Spine` FUSES the temporal category with the clock column that category implies, so a declaration claiming event-time and handing over no observation column is unrepresentable here rather than refused downstream, and `Admission` reads the time slot off the spine instead of off a second field a producer contradicts. Identity and provenance ride as columns — classification system and code with evidence source and calendar expiry, the content-derived appearance key on library rows — so audit queries filter and expiry-screen without joining back into object graphs; `observed` trails every column list, so every declaration reads identity, then payload, then spine; the storage tier derives its own sort key from `KeyColumns` and the spine, never from declaration order.
- Packages: Rasm.Element (project — `TableType`/`TableColumn`/`TableSpine`, the contract declaration vocabulary), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new dataset is one `MaterialsDataset` row with its `[03]` row record and fold; a new column is one `TableColumn` with its field on the owning row record; a new physical token is one `TableType` row at the Element owner, never a second roster here.
- Law: GRAIN decides whether a duration is summable. `materials.texture` excludes a set's OWN press duration because that duration repeats on every channel row of one press and summing it multiplies one bake by its channel count, while `materials.texture-set` declares it — one row IS one bake there, which is exactly why the set-grained half is its own declaration rather than more columns on the channel one. `materials.environment` takes stored bytes and never the prefilter duration, under the same rule.
- Law: `gwp`, `elapsed_s`, and `byte_length` are the only measures the family declares, because summing a mixed-unit long-form property column or a colour channel states a magnitude neither carries, while stored plane bytes sum to exactly the whole texture footprint a storage question asks for.
- Boundary: declaration truth and row truth stay co-located, so each dataset edit carries its matching row field and projection expression. Tokens cross as TEXT and the physical decision past that is the custodian's: this folder reaches Element and never `Rasm.Persistence`, so a token Element's roster mints that the custodian's roster lacks fails at that gate, which is the compiler this boundary does not have — and `TableType.KeyHex` (`fixed-hex128`) is the one token Materials' fixed-width X32 content keys added to that roster. Naming disambiguates at the source: the custodian owns `ColumnType`, `ColumnRow`, and `AnalyticsSchema`, and this page's declarations never wear those names.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialsDataset {
    public static readonly MaterialsDataset ComponentRows = new("materials.component-rows",
        Seq("component"), spine: new TableSpine.Event("observed"), Option<string>.None, Seq(
            new TableColumn("component", TableType.Utf8, Nullable: false),
            new TableColumn("family", TableType.Utf8, Nullable: false),
            new TableColumn("class", TableType.Utf8, Nullable: false),
            new TableColumn("sectioned", TableType.Bool, Nullable: false),
            new TableColumn("substance", TableType.Utf8, Nullable: false),
            new TableColumn("appearance", TableType.Utf8, Nullable: false),
            new TableColumn("ifc_entity", TableType.Utf8, Nullable: false),
            new TableColumn("predefined", TableType.Utf8, Nullable: false),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset PropertyRows = new("materials.property-rows",
        Seq("material", "property"), spine: new TableSpine.Event("observed"), Option<string>.None, Seq(
            new TableColumn("material", TableType.Utf8, Nullable: false),
            new TableColumn("property", TableType.Utf8, Nullable: false),
            new TableColumn("unit", TableType.Utf8, Nullable: false),
            new TableColumn("central", TableType.Float64, Nullable: false),
            new TableColumn("evidence_source", TableType.Utf8, Nullable: false),
            new TableColumn("evidence_expiry", TableType.Date, Nullable: true),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset Sustainability = new("materials.sustainability",
        Seq("material", "stage"), spine: new TableSpine.Event("observed"), Some("gwp"), Seq(
            new TableColumn("material", TableType.Utf8, Nullable: false),
            new TableColumn("basis", TableType.Utf8, Nullable: false),
            new TableColumn("stage", TableType.Utf8, Nullable: false),
            new TableColumn("gwp", TableType.Float64, Nullable: false),
            new TableColumn("recycled", TableType.Float64, Nullable: false),
            new TableColumn("recovery", TableType.Float64, Nullable: false),
            new TableColumn("classification_system", TableType.Utf8, Nullable: true),
            new TableColumn("classification_code", TableType.Utf8, Nullable: true),
            new TableColumn("evidence_source", TableType.Utf8, Nullable: false),
            new TableColumn("evidence_expiry", TableType.Date, Nullable: true),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset LibrarySummary = new("materials.library-summary",
        Seq("material"), spine: new TableSpine.Event("observed"), Option<string>.None, Seq(
            new TableColumn("material", TableType.Utf8, Nullable: false),
            new TableColumn("appearance_key", TableType.KeyHex, Nullable: false),
            new TableColumn("base_r", TableType.Float64, Nullable: false),
            new TableColumn("base_g", TableType.Float64, Nullable: false),
            new TableColumn("base_b", TableType.Float64, Nullable: false),
            new TableColumn("metallic", TableType.Float64, Nullable: false),
            new TableColumn("roughness", TableType.Float64, Nullable: false),
            new TableColumn("opacity", TableType.Float64, Nullable: false),
            new TableColumn("transmissive", TableType.Bool, Nullable: false),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset CapacityChecks = new("materials.capacity-checks",
        Seq("op", "kind", "governing"), spine: new TableSpine.Event("observed"), Some("elapsed_s"), Seq(
            new TableColumn("op", TableType.Utf8, Nullable: false),
            new TableColumn("kind", TableType.Utf8, Nullable: false),
            new TableColumn("governing", TableType.Utf8, Nullable: false),
            new TableColumn("adequate", TableType.Bool, Nullable: false),
            new TableColumn("utilisation", TableType.Float64, Nullable: true),
            new TableColumn("deferral", TableType.Utf8, Nullable: true),
            new TableColumn("elapsed_s", TableType.Float64, Nullable: false),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset TextureChannels = new("materials.texture",
        Seq("set", "channel", "level"), spine: new TableSpine.Event("observed"), Some("byte_length"), Seq(
            new TableColumn("set", TableType.KeyHex, Nullable: false),
            new TableColumn("appearance", TableType.KeyHex, Nullable: true),
            new TableColumn("material", TableType.Utf8, Nullable: true),
            new TableColumn("channel", TableType.Utf8, Nullable: false),
            new TableColumn("level", TableType.Int64, Nullable: false),
            new TableColumn("transfer", TableType.Utf8, Nullable: false),
            new TableColumn("format", TableType.Utf8, Nullable: false),
            new TableColumn("ktx_payload", TableType.Utf8, Nullable: true),
            new TableColumn("block_format", TableType.Utf8, Nullable: true),
            new TableColumn("mips", TableType.Int64, Nullable: false),
            new TableColumn("width", TableType.Int64, Nullable: false),
            new TableColumn("height", TableType.Int64, Nullable: false),
            new TableColumn("layers", TableType.Int64, Nullable: false),
            new TableColumn("tiled", TableType.Bool, Nullable: false),
            new TableColumn("blob", TableType.KeyHex, Nullable: false),
            new TableColumn("byte_length", TableType.Int64, Nullable: false),
            new TableColumn("texels", TableType.Int64, Nullable: true),
            new TableColumn("elapsed_s", TableType.Float64, Nullable: true),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset TextureSets = new("materials.texture-set",
        Seq("set"), spine: new TableSpine.Event("observed"), Some("elapsed_s"), Seq(
            new TableColumn("set", TableType.KeyHex, Nullable: false),
            new TableColumn("appearance", TableType.KeyHex, Nullable: true),
            new TableColumn("material", TableType.Utf8, Nullable: true),
            new TableColumn("channels", TableType.Int64, Nullable: false),
            new TableColumn("packs", TableType.Int64, Nullable: false),
            new TableColumn("tiled", TableType.Bool, Nullable: false),
            new TableColumn("tile_strategy", TableType.Utf8, Nullable: true),
            new TableColumn("tile_score", TableType.Float64, Nullable: true),
            new TableColumn("tile_seam_ratio", TableType.Float64, Nullable: true),
            new TableColumn("tile_lattice_leak", TableType.Float64, Nullable: true),
            new TableColumn("texels", TableType.Int64, Nullable: true),
            new TableColumn("elapsed_s", TableType.Float64, Nullable: true),
            new TableColumn("downgraded", TableType.Int64, Nullable: true),
            new TableColumn("faulted_texels", TableType.Int64, Nullable: true),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public static readonly MaterialsDataset EnvironmentProducts = new("materials.environment",
        Seq("set", "product", "level"), spine: new TableSpine.Event("observed"), Some("byte_length"), Seq(
            new TableColumn("set", TableType.KeyHex, Nullable: false),
            new TableColumn("product", TableType.Utf8, Nullable: false),
            new TableColumn("level", TableType.Int64, Nullable: false),
            new TableColumn("container", TableType.Utf8, Nullable: false),
            new TableColumn("format", TableType.Utf8, Nullable: false),
            new TableColumn("transfer", TableType.Utf8, Nullable: false),
            new TableColumn("primaries", TableType.Utf8, Nullable: false),
            new TableColumn("depth", TableType.Utf8, Nullable: false),
            new TableColumn("layers", TableType.Int64, Nullable: false),
            new TableColumn("mips", TableType.Int64, Nullable: false),
            new TableColumn("blob", TableType.KeyHex, Nullable: false),
            new TableColumn("byte_length", TableType.Int64, Nullable: false),
            new TableColumn("observed", TableType.Timestamp, Nullable: false)));

    public Seq<string> KeyColumns { get; }
    public TableSpine Spine { get; }
    public Option<string> Measure { get; }
    public Seq<TableColumn> Columns { get; }

    public static Seq<MaterialsDataset> Rows => toSeq(Items).Strict();

    public TableDeclaration Declaration => new(Key, KeyColumns, Spine, Measure, Columns);
}
```

## [03]-[ROW_PROJECTION]

- Owner: `AnalyticsProjection` — the typed row records and the total folds from registered rows and typed results onto flat row streams; `PropertyColumn` — the selector table one scalar or dimensioned property occupies per row. Generated `Set.product`, `EnvironmentSet.product`, and `PlaneRef` are the only product, environment, and stored-level discriminants.
- Entry: `Components` folds catalogue rows; `Properties` folds per-material property rows through the admitted `PropertyColumn` table; `Sustainability` folds one row per lifecycle stage; `Library` traverses material keys through an injected admitted appearance lookup; `Capacity` chooses capacity facts off the observability stream; `Textures` exhaustively unwraps each generated `Set` surface arm and emits every `PlaneRef` level; `TextureSets` reads the same surface at set grain beside its caller-supplied tile evidence; `Environments` exhaustively unwraps `EnvironmentSet` and emits every generated environment product, including every specular level. Every fold stamps `frame.At`.
- Auto: a dimensioned selector reads its SI accessor off the quantity the `Published` carrier holds, so the magnitude and the UCUM unit it is stated in derive from one owner and no fold re-scales; folds are total over their registered inputs — an unregistered library key aborts the library fold typed rather than emitting a partial dataset.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, UnitsNet, BCL inbox.
- Growth: a new scalar or dimensioned property is one `PropertyColumn` row carrying its unit and its selector; a new generated environment oneof arm breaks the exhaustive fold until its level projection lands; a new dataset fold is one row record and one member beside its declaration.
- Boundary: ingress is parameterized — every fold takes its registered input and its frame as arguments and reads no ambient registry; egress is a row `Seq` the custodian batches through its own generic record-batch fold, so buffer custody, batch sizing, and dataset writes never enter this page. Folds read the already-projected WIRE wherever one exists, so a warehouse column and the document a consumer decoded agree byte for byte; evidence no wire carries — a `TileRun`, a blob's stored length — enters as a SECOND ARGUMENT rather than as a re-derivation or a widened wire, and a row whose measured column has no producer is not emitted, because the alternative is a zero the measure sums.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Materials.Appearance;
using Rasm.Materials.Appearance.Interchange;
using Rasm.Materials.Component;
using Rasm.Materials.Raster;
using Thinktecture;
// Contracts are retired from this logic.
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [MODELS] --------------------------------------------------------------------------
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

public readonly record struct CapacityCheckRow(
    string Op, string Kind, string Governing, bool Adequate, Option<double> Utilisation, Option<string> Deferral,
    double ElapsedSeconds, Instant Observed);

public readonly record struct TextureChannelAnalyticsRow(
    string Set, Option<string> Appearance, Option<string> Material, string Channel, int Level, string Transfer, string Format,
    Option<string> KtxPayload, Option<string> BlockFormat, int Mips, int Width, int Height, int Layers, bool Tiled,
    string Blob, long ByteLength, Option<long> Texels, Option<double> ElapsedSeconds,
    Instant Observed);

public readonly record struct TextureSetAnalyticsRow(
    string Set, Option<string> Appearance, Option<string> Material, int Channels, int Packs, bool Tiled,
    Option<string> TileStrategy, Option<double> TileScore, Option<double> TileSeamRatio, Option<double> TileLatticeLeak,
    Option<long> Texels, Option<double> ElapsedSeconds,
    Option<long> Downgraded, Option<long> FaultedTexels, Instant Observed);

public readonly record struct EnvironmentProductRow(
    string Set, string Product, int Level, string Container, string Format, string Transfer, string Primaries,
    string Depth, int Layers, int Mips, string Blob, long ByteLength, Instant Observed);

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed record PropertyColumn(
    string Property, Option<QuantityRow> Quantity, string Dimensionless,
    Func<Seq<MaterialPropertySet>, Option<double>> Central) {

    public string Unit => Quantity.Map(static row => row.Unit).IfNone(Dimensionless);

    public static readonly Seq<PropertyColumn> Rows = Seq(
        new PropertyColumn("density", Some(QuantityRow.Density), "1", static sets => sets.Mechanical.Map(static m => m.Density.Si)),
        new PropertyColumn("modulus", Some(QuantityRow.Pressure), "1", static sets => sets.Mechanical.Map(static m => m.YoungsModulus.Si)),
        new PropertyColumn("yield_strength", Some(QuantityRow.Pressure), "1", static sets => sets.Mechanical.Map(static m => m.YieldStrength.Si)),
        new PropertyColumn("ultimate_strength", Some(QuantityRow.Pressure), "1", static sets => sets.Mechanical.Map(static m => m.UltimateStrength.Si)),
        new PropertyColumn("poisson", None, "1", static sets => sets.Mechanical.Map(static m => m.PoissonsRatio)),
        new PropertyColumn("expansion_per_k", None, "/K", static sets => sets.Mechanical.Map(static m => m.ThermalExpansionPerK)),
        new PropertyColumn("conductivity", Some(QuantityRow.ThermalConductivity), "1", static sets => sets.Thermal.Map(static t => t.Conductivity.Si)),
        new PropertyColumn("specific_heat", Some(QuantityRow.SpecificEntropy), "1", static sets => sets.Thermal.Map(static t => t.SpecificHeat.Si)),
        new PropertyColumn("u_value", Some(QuantityRow.HeatTransferCoefficient), "1",
            static sets => sets.Thermal.Bind(static t => t.UValue).Map(static v => v.Si)),
        new PropertyColumn("vapour_mu", None, "1", static sets => sets.Thermal.Map(static t => t.VapourResistanceFactor)),
        new PropertyColumn("nrc", None, "1", static sets => sets.Acoustic.Map(static a => a.Nrc)),
        new PropertyColumn("fire_resistance_minutes", None, "min",
            static sets => sets.Fire.Bind(static f => f.Resistance.LoadBearingMinutes).Map(static minutes => (double)minutes)),
        new PropertyColumn("porosity", None, "1", static sets => sets.Hygrothermal.Map(static h => h.Porosity)),
        new PropertyColumn("water_content_80rh", None, "kg/m3", static sets => sets.Hygrothermal.Map(static h => h.WaterContent80Rh.Si)),
        new PropertyColumn("free_water_saturation", None, "kg/m3", static sets => sets.Hygrothermal.Map(static h => h.FreeWaterSaturation.Si)),
        new PropertyColumn("water_absorption", None, "kg/(m2.s0.5)", static sets => sets.Hygrothermal.Bind(static h => h.WaterAbsorptionKgPerM2SqrtS)),
        new PropertyColumn("visible_transmittance", None, "1", static sets => sets.Optical.Map(static o => o.VisibleTransmittance)),
        new PropertyColumn("visible_reflectance_front", None, "1", static sets => sets.Optical.Map(static o => o.VisibleReflectanceFront)),
        new PropertyColumn("visible_reflectance_back", None, "1", static sets => sets.Optical.Map(static o => o.VisibleReflectanceBack)),
        new PropertyColumn("solar_transmittance", None, "1", static sets => sets.Optical.Map(static o => o.SolarTransmittance)),
        new PropertyColumn("solar_reflectance_front", None, "1", static sets => sets.Optical.Map(static o => o.SolarReflectanceFront)),
        new PropertyColumn("solar_reflectance_back", None, "1", static sets => sets.Optical.Map(static o => o.SolarReflectanceBack)),
        new PropertyColumn("ir_transmittance", None, "1", static sets => sets.Optical.Map(static o => o.ThermalIrTransmittance)),
        new PropertyColumn("ir_emissivity_front", None, "1", static sets => sets.Optical.Map(static o => o.ThermalIrEmissivityFront)),
        new PropertyColumn("ir_emissivity_back", None, "1", static sets => sets.Optical.Map(static o => o.ThermalIrEmissivityBack)),
        new PropertyColumn("damping_ratio", None, "1", static sets => sets.Damping.Map(static d => d.DampingRatio)));
}

public static class AnalyticsProjection {
    public static Seq<ComponentAnalyticsRow> Components(Seq<ComponentRow> rows, ProjectionContext frame) =>
        rows.Map(row => new ComponentAnalyticsRow(
            row.Item.Designation.Value, row.Item.Family.Key, row.Item.Class.Key, row.Sectioned,
            row.Item.SubstanceId.Value, row.Item.AppearanceId.Value, row.Item.IfcEntity,
            row.Item.PredefinedToken, frame.At));

    public static Seq<PropertyAnalyticsRow> Properties(
        Seq<(MaterialId Id, Seq<MaterialPropertySet> Admitted)> rows, ProjectionContext frame) =>
        rows.Bind(entry => PropertyColumn.Rows.Choose(column => entry.Admitted
            .Choose(set => column.Central(Seq(set)).Map(central => (Central: central, set.Evidence)))
            .Head
            .Map(read => new PropertyAnalyticsRow(entry.Id.ToValue(), column.Property, column.Unit, read.Central,
                read.Evidence.Source, read.Evidence.ValidUntil, frame.At))));

    public static Seq<SustainabilityAnalyticsRow> Sustainability(
        Seq<(MaterialId Id, Seq<MaterialPropertySet> Admitted, Option<Classification> Classification)> rows, ProjectionContext frame) =>
        rows.Bind(entry => entry.Admitted
            .Choose(static set => set as MaterialPropertySet.Environmental)
            .Bind(environmental => toSeq(LifecycleStage.Items).Map(stage =>
                new SustainabilityAnalyticsRow(
                    entry.Id.ToValue(), environmental.Basis.Key, stage.Key, environmental.StageAt(stage),
                    environmental.RecycledContent, environmental.EndOfLifeRecovery,
                    entry.Classification.Map(static c => c.System),
                    entry.Classification.Map(static c => c.Code),
                    environmental.Evidence.Source, environmental.Evidence.ValidUntil, frame.At))));

    public static Fin<Seq<LibrarySummaryRow>> Library(
        Seq<MaterialId> materials, ProjectionContext frame,
        Func<MaterialId, Op, Fin<AppearanceSummary>> lookup) =>
        materials.TraverseM(id => lookup(id, frame.Key).Map(summary =>
            new LibrarySummaryRow(
                id.ToValue(), summary.AppearanceKey.ToString("X32"), summary.BaseColorR, summary.BaseColorG,
                summary.BaseColorB, summary.Metallic, summary.Roughness, summary.Opacity,
                summary.Transmissive, frame.At))).As();

    sealed record SurfaceProjection(Wire.Set Set, Wire.SurfaceSet Surface, Option<Wire.BakedSet> Baked);

    static Fin<Option<SurfaceProjection>> Surface(Wire.Set set, Op key) => set.ProductCase switch {
        Wire.Set.ProductOneofCase.Pbr => Fin.Succ(Some(new SurfaceProjection(set, set.Pbr, None))),
        Wire.Set.ProductOneofCase.Baked => Fin.Succ(Some(new SurfaceProjection(set, set.Baked.Surface, Some(set.Baked)))),
        Wire.Set.ProductOneofCase.Environment => Fin.Succ(Option<SurfaceProjection>.None),
        Wire.Set.ProductOneofCase.None or _ => Fin.Fail<Option<SurfaceProjection>>(key.InvalidInput()),
    };

    public static Fin<Seq<TextureChannelAnalyticsRow>> Textures(
        Seq<Wire.Set> sets, ProjectionContext frame, Op key) =>
        sets.Traverse(set => Surface(set, key).Map(surface => surface
                .Map(value => TextureRows(value, frame))
                .IfNone(Seq<TextureChannelAnalyticsRow>())))
            .As()
            .Map(static rows => rows.Bind(static row => row));

    static Seq<TextureChannelAnalyticsRow> TextureRows(SurfaceProjection set, ProjectionContext frame) =>
        toSeq(set.Surface.Planes).Bind(plane => toSeq(plane.Levels.Select((level, at) =>
                TextureRow(set, plane.Role.ToString(), at, plane.HasTransfer ? plane.Transfer.ToString() : string.Empty,
                    plane.Format.ToString(), Some(plane.KtxPayload.ToString()),
                    plane.HasBlockFormat ? Some(plane.BlockFormat.ToString()) : None,
                    checked((int)plane.Mips), level, frame))))
        + toSeq(set.Surface.Packs).Bind(pack => toSeq(pack.Levels.Select((level, at) =>
                TextureRow(set, pack.Pack.ToString(), at, Wire.Transfer.Raw.ToString(), pack.Format.ToString(),
                    None, None, checked((int)pack.Mips), level, frame))));

    static TextureChannelAnalyticsRow TextureRow(
        SurfaceProjection set, string channel, int level, string transfer, string format,
        Option<string> payload, Option<string> block, int mips, Wire.PlaneRef stored, ProjectionContext frame) {
        Option<Wire.Press> press = set.Baked.Bind(static baked => Optional(baked.Press));
        return new(
            Set: Hex(set.Set.Key), Appearance: set.Baked.Map(static baked => Hex(baked.AppearanceKey)),
            Material: Optional(set.Surface.MaterialId).Filter(static value => value.Length > 0),
            Channel: channel, Level: level, Transfer: transfer, Format: format,
            KtxPayload: payload, BlockFormat: block, Mips: mips,
            Width: checked((int)set.Surface.Width), Height: checked((int)set.Surface.Height),
            Layers: checked((int)set.Surface.Layers), Tiled: set.Surface.Tiled,
            Blob: Hex(stored.Digest), ByteLength: checked((long)stored.ByteLength),
            Texels: press.Map(static value => checked((long)value.Texels)),
            ElapsedSeconds: press.Map(static value => value.Elapsed.ToNodaDuration().TotalSeconds),
            Observed: frame.At);
    }

    public static Seq<CapacityCheckRow> Capacity(Seq<MaterialsFact> facts, ProjectionContext frame) =>
        facts.Choose(fact => fact as MaterialsFact.CapacityCheck).Map(check =>
            new CapacityCheckRow(
                check.Key.Value, check.Lift.Kind, check.Verdict.Governing.Key,
                check.Verdict.Adequate,
                check.Verdict.Ratio,
                check.Verdict is Utilisation.RequiresMemberCheck deferred
                    ? Some(deferred.Requirement.Key)
                    : Option<string>.None,
                check.Elapsed.TotalSeconds, frame.At));

    public static Fin<Seq<TextureSetAnalyticsRow>> TextureSets(
        Seq<(Wire.Set Set, Option<TileRun> Tile)> sets, ProjectionContext frame, Op key) =>
        sets.Traverse(entry => Surface(entry.Set, key).Map(surface => surface.Map(value => {
            Option<TileScore> score = entry.Tile.Bind(static run => run.Score.Value());
            Option<Wire.Press> press = value.Baked.Bind(static baked => Optional(baked.Press));
            return new TextureSetAnalyticsRow(
                Set: Hex(value.Set.Key), Appearance: value.Baked.Map(static baked => Hex(baked.AppearanceKey)),
                Material: Optional(value.Surface.MaterialId).Filter(static material => material.Length > 0),
                Channels: value.Surface.Planes.Count, Packs: value.Surface.Packs.Count, Tiled: value.Surface.Tiled,
                TileStrategy: entry.Tile.Map(static run => run.Strategy.Key),
                TileScore: score.Map(static measured => measured.Value),
                TileSeamRatio: score.Map(static measured => measured.SeamRatio),
                TileLatticeLeak: score.Map(static measured => measured.LatticeLeak),
                Texels: press.Map(static wire => checked((long)wire.Texels)),
                ElapsedSeconds: press.Map(static wire => wire.Elapsed.ToNodaDuration().TotalSeconds),
                Downgraded: press.Map(static wire => checked((long)wire.Downgraded)),
                FaultedTexels: press.Map(static wire => checked((long)wire.FaultedTexels)),
                Observed: frame.At);
        }))).As().Map(static rows => rows.Choose(static row => row));

    public static Fin<Seq<EnvironmentProductRow>> Environments(
        Seq<Wire.Set> sets, ProjectionContext frame, Op key) =>
        sets.Traverse(set => EnvironmentRows(set, frame, key)).As()
            .Map(static rows => rows.Bind(static row => row));

    static Fin<Seq<EnvironmentProductRow>> EnvironmentRows(Wire.Set set, ProjectionContext frame, Op key) =>
        set.ProductCase switch {
            Wire.Set.ProductOneofCase.Pbr or Wire.Set.ProductOneofCase.Baked =>
                Fin.Succ(Seq<EnvironmentProductRow>()),
            Wire.Set.ProductOneofCase.Environment => Products(set.Environment, key).Map(products =>
                products.Map(product => new EnvironmentProductRow(
                    Set: Hex(set.Key), Product: product.Product, Level: product.Level,
                    Container: product.Plane.Container.ToString(), Format: product.Plane.Format.ToString(),
                    Transfer: product.Plane.Transfer.ToString(), Primaries: product.Plane.Primaries.ToString(),
                    Depth: product.Plane.Depth.ToString(), Layers: checked((int)product.Plane.Layers),
                    Mips: checked((int)product.Plane.Mips), Blob: Hex(product.Plane.Plane.Digest),
                    ByteLength: checked((long)product.Plane.Plane.ByteLength), Observed: frame.At))),
            Wire.Set.ProductOneofCase.None or _ => Fin.Fail<Seq<EnvironmentProductRow>>(key.InvalidInput()),
        };

    static Fin<Seq<(string Product, int Level, Wire.EnvironmentPlane Plane)>> Products(
        Wire.EnvironmentSet environment, Op key) => environment.ProductCase switch {
            Wire.EnvironmentSet.ProductOneofCase.Hdri => Fin.Succ(SourceProducts(environment.Hdri.Source)),
            Wire.EnvironmentSet.ProductOneofCase.Ibl => Fin.Succ(
                SourceProducts(environment.Ibl.Source)
                + toSeq(environment.Ibl.Specular.Select((plane, at) => ("specular", at, plane)))
                + Seq(("brdfLut", 0, environment.Ibl.BrdfLut))
                + Optional(environment.Ibl.LuminanceCdf)
                    .Map(static plane => Seq(("luminanceCdf", 0, plane)))
                    .IfNone(Seq<(string, int, Wire.EnvironmentPlane)>())),
            Wire.EnvironmentSet.ProductOneofCase.None or _ =>
                Fin.Fail<Seq<(string, int, Wire.EnvironmentPlane)>>(key.InvalidInput()),
        };

    static Seq<(string Product, int Level, Wire.EnvironmentPlane Plane)> SourceProducts(Wire.EnvironmentSource source) =>
        Seq(("equirect", 0, source.Equirect))
        + Optional(source.Cubemap).Map(static plane => Seq(("cubemap", 0, plane))).IfNone(Seq<(string, int, Wire.EnvironmentPlane)>())
        + Optional(source.Preview).Map(static plane => Seq(("preview", 0, plane))).IfNone(Seq<(string, int, Wire.EnvironmentPlane)>());

    static string Hex(Google.Protobuf.ByteString key) => Convert.ToHexString(key.Span);
}
```

## [04]-[RESEARCH]

(none)
