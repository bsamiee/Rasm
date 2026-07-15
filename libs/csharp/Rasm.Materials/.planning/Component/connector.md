# [MATERIALS_CONNECTOR]

THE FRAMING-CONNECTOR SEED PAGE owns the `ComponentFamily.Connector` fold, manufacturer catalogue, directional resistance algebra, and host-neutral plate receipt. `ConnectorInstall` directly carries the attaching fastener's designation, canonical `FastenerKind`, allowable, shank diameter, and duration policy. Every row admits its published directional values and fastener-group fidelity before `Component.Of`, and `ConnectorCapacity.DemandRatio` rails unsupported demanded directions.

## [01]-[INDEX]

- [02]-[CONNECTOR_FAMILY]: the `LoadDirection` vocabulary, `ConnectorType` discriminant, `ConnectorInstall` attachment policy, frozen gauge/duration tables, admitted resistance and demand values, `ConnectorPlate` host receipt, typed catalogue, detail builder, and `ConnectorSeed.Rows` fold.

## [02]-[CONNECTOR_FAMILY]

- Owner: `ConnectorType` carries accessory identity, resisted directions, and plate dispatch; `ConnectorInstall` carries attaching-fastener policy directly; `LoadDirection` owns directional reads; `LoadResistance` and `LoadDemand` admit resistance and demand; `ConnectorRow` owns catalogue capacity; `ConnectorCapacity` owns the unit-check; `ConnectorPlate` carries host materialization; `ConnectorDetail` and `ConnectorSeed` own realization and construction.
- Cases: type {`joist-hanger` (face-mount saddle — `SHOE`, seat-bearing download, resists download + uplift, builds `ConnectorPlate.Saddle`) · `framing-angle` (L-bend clip — `BRACKET`, resists uplift + lateral, builds `.Angle`) · `strap` (flat tension tie — `BRACKET`, uplift only, builds `.Strap`) · `hold-down` (shear-wall anchor — `ANCHORPLATE`, bolt-fastened, uplift only, builds `.AnchorPlate`)} × gauge {18/16/14/12/10 ga} × install {nailed (10d common — `NAIL`) · screwed (SD structural screw — `SCREW`) · bolted (through-bolt — `BOLT`)} — a connector is one `ConnectorRow` over one type, one `GaugeRow`, one `ConnectorInstall`, and its published fastener schedule, never a connector subtype.
- Entry: `ConnectorSeed.Rows(context)` traverses each row through `Allowable`, `SectionProfile.Rectangle.Of`, and `Component.Of`. `ConnectorRow.GovernedCapacity(duration, key)` scales only admitted resisted values. `ConnectorCapacity.DemandRatio(demand, key)` returns the maximum supported-direction ratio on `Fin` and faults when positive demand targets an unresisted direction. `ConnectorRow.Plate` dispatches host materialization through `ConnectorType.BuildPlate`.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (`MaterialId`, `DetailSchema`, `PropertyBag`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile.Rectangle.Of` the railed profile admission/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + comparer accessors for the type/direction/install vocabularies, `[UseDelegateFromConstructor]` for the direction reads and the plate builder, `[ComplexValueObject]` for `LoadResistance`, `[Union]` for `ConnectorPlate`), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`.As()`/`guard`), BCL (`ImmutableArray`, `FrozenSet`). No structural-connector package exists among admitted surfaces (VividOrange is member-catalogue + EN-grade scope; the AISI/NDS bodies have no typed code object), so the rows are AUTHORED/PUBLISHED in-fence and the citations ride `ComponentAuthority.Aisi` plus per-column provenance.
- Growth: a new connector is one `ConnectorRow` entry (typed vocabulary refs, published allowables + the report's fastener schedule); a new gauge one `GaugeRow`; a new duration case one `DurationRow`; a new connector class one `ConnectorType` row reusing an existing plate builder (a twist-strap reuses `BuildStrap`); a new attachment one `ConnectorInstall` row; a new resisted-load direction one `LoadDirection` row — its delegate columns force every read site to answer at compile time — never a per-connector type, never a parallel per-direction member family. A new host body form is one `ConnectorPlate` case plus one builder.
- Boundary: the bespoke `ConnectorSection` payload and its `ComponentSection` arm are DELETED — the carried-member fit is the closed `SectionProfile.Rectangle` arm (gross facts base-constructor state), the realization identity rides the seed-built bag (`AccessoryType` + the install-sourced separate `FastenerType` token + carried-member measured columns, dimension-only SI mints — row names and mint shapes identical to the retired projector switch, the `FastenerType` value reading the actual install schedule so a screwed row stamps `SCREW` and the type-level `NAILPLATE` misstamp on nailed hangers/straps is corrected), and the capacity algebra rides the typed `ConnectorRow`; the published allowable is the manufacturer-evaluated capacity, never a re-derived analysis — the fastener-group bound (`Fasteners · PerFastenerKn`) is the ADMISSION fidelity gate that keeps the schedule columns load-bearing (a published value no schedule can deliver faults at seed), and `GaugeRow.AxialSectionCapacityKnPerMm` (`Fy·t`) is the AISI net-section datum a `Rasm.Compute` developed-width section check reads off the seam — the connector owns the datum, Compute owns the check; the row admission is ONE site (`Allowable`) — the `LoadResistance` factory guards each resisted allowable positive, each UNRESISTED column exactly zero, and at least one direction resisted, so an unrepresentable connector never constructs, a value transcribed into an unresisted column faults instead of silently dropping, and `GoverningKn` never reads a placeholder column; the `ConnectorPlate` receipt derives every field from row columns plus the AISI forming radius (`1.5·t`) and the fastener shank — the shared sheet/hole facts as ONE `PlateStock` base-state product — so the host solid traces to the row, never a literal, and this owner never constructs a host brep; substance and appearance are independent `MaterialId` slots (`steel.g33`/`steel.g50` substance off the gauge, galvanized `metal.steel` appearance); the IFC entity class is the `Rasm.Bim` egress gate's read over the verified accessory token, the attaching fastener a SEPARATE `IfcMechanicalFastener` related at egress; a connector schedule station-steps the layout fold over these rows, never a parallel connector-layout owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;        // FrozenSet (the ConnectorType resisted-direction set)
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Domain;                      // Op, Context
using Rasm.Element.Composition;                     // MaterialId, DetailSchema, PropertyBag
using Rasm.Element.Properties;
using Thinktecture;
using SiDim = Rasm.Element.Properties.Dimension;   // the SI-dimension axis the detail-bag mints ride
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The resisted-load direction vocabulary — the [Flags] bitfield's replacement: membership is set algebra
// (ConnectorType.Resists.Contains), behavior is the delegate columns owning every per-direction slot read, so the
// governing-min, the capacity build, and the demand-ratio max are LoadDirection.Items folds instead of triplicated
// member chains; a fourth direction is one row that breaks every read site at compile time. SeatBorne names the one
// direction a carried-member saddle transfers through seat bearing (the hanger download), exempting it from the
// fastener-group fidelity gate.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LoadDirection {
    public static readonly LoadDirection Download = new("download", static r => r.DownloadKn, static d => d.DownloadKn, static c => c.DownloadKn, static t => t.CarriesMember);
    public static readonly LoadDirection Uplift   = new("uplift",   static r => r.UpliftKn,   static d => d.UpliftKn,   static c => c.UpliftKn,   static _ => false);
    public static readonly LoadDirection Lateral  = new("lateral",  static r => r.LateralKn,  static d => d.LateralKn,  static c => c.LateralKn,  static _ => false);

    [UseDelegateFromConstructor] public partial double Published(LoadResistance resistance);
    [UseDelegateFromConstructor] public partial double Demand(LoadDemand demand);
    [UseDelegateFromConstructor] public partial double Adjusted(ConnectorCapacity capacity);
    [UseDelegateFromConstructor] public partial bool SeatBorne(ConnectorType type);
    public bool Resisted(ConnectorType type) => type.Resists.Contains(this);
}

// ConnectorType carries the portable IfcDiscreteAccessoryTypeEnum token the Rasm.Bim egress reads (the fabricated
// body the connector IS — the SEPARATE attaching-fastener token rides ConnectorInstall, the actual per-row
// schedule), the resisted-direction set, and the BuildPlate body-form column: the vocabulary item owns its plate
// geometry, so a new type names its body by reusing a builder — the BRACKET token splitting into Angle and Strap
// bodies is why the form is a per-type delegate, never derivable from the accessory token.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConnectorType {
    public static readonly ConnectorType JoistHanger  = new("joist-hanger",  ifcDesignation: "joist-hanger",  ifcAccessoryType: "SHOE",        carriesMember: true,  resists: new[] { LoadDirection.Download, LoadDirection.Uplift }.ToFrozenSet(), BuildSaddle);
    public static readonly ConnectorType FramingAngle = new("framing-angle", ifcDesignation: "framing-angle", ifcAccessoryType: "BRACKET",     carriesMember: false, resists: new[] { LoadDirection.Uplift, LoadDirection.Lateral }.ToFrozenSet(),  BuildAngle);
    public static readonly ConnectorType Strap        = new("strap",         ifcDesignation: "strap-tie",     ifcAccessoryType: "BRACKET",     carriesMember: false, resists: new[] { LoadDirection.Uplift }.ToFrozenSet(),                         BuildStrap);
    public static readonly ConnectorType HoldDown     = new("hold-down",     ifcDesignation: "hold-down",     ifcAccessoryType: "ANCHORPLATE", carriesMember: false, resists: new[] { LoadDirection.Uplift }.ToFrozenSet(),                         BuildAnchorPlate);
    public string IfcDesignation { get; }     // the ObjectType discriminant the federation reads past the predefined enum
    public string IfcAccessoryType { get; }   // IFC4.3 IfcDiscreteAccessoryTypeEnum member — the connector body
    public bool CarriesMember { get; }        // true only for the saddle-seat hanger
    public FrozenSet<LoadDirection> Resists { get; }

    [UseDelegateFromConstructor]
    public partial ConnectorPlate BuildPlate(ConnectorRow row);

    const double NailPitchMm = 19.05;     // 3/4in standard cold-formed hole-grid pitch
    const double BendFactor = 1.5;        // AISI S100 minimum inside forming radius ≈ 1.5·t
    const double HoleClearanceMm = 0.8;   // nail/screw hole over the fastener shank
    const double SeatFloorMm = 38.1;      // 2x-lumber minimum seat/leg dimension

    // The shared stamped-sheet + hole-schedule product derived ONCE per row — never re-spelled per form.
    static PlateStock Sheet(ConnectorRow r) => new(
        SheetThicknessMm: r.Gauge.BaseThicknessMm,
        BendRadiusMm: r.Gauge.BaseThicknessMm * BendFactor,
        FastenerDesignation: r.Install.FastenerDesignation,
        FastenerKind: r.Install.FastenerKind,
        HoleDiameterMm: r.Install.ShankDiameterMm + HoleClearanceMm,
        HoleCount: r.Fasteners,
        HolePitchMm: NailPitchMm);

    // Face-mount U-saddle: seat cradles the joist width, side flanges rise the joist depth, back flange face-nails
    // the support; the host distributes Stock.HoleCount holes at Stock.HolePitchMm over the side and back faces.
    static ConnectorPlate BuildSaddle(ConnectorRow r) => new ConnectorPlate.Saddle(
        SeatWidthMm: r.CarriedMemberWidthMm,
        SeatDepthMm: Math.Max(SeatFloorMm, r.CarriedMemberWidthMm),
        SideFlangeHeightMm: r.CarriedMemberDepthMm,
        BackFlangeWidthMm: 25.4 + r.Gauge.BaseThicknessMm * 6.0,
        Stock: Sheet(r));

    // L-bend: two formed legs (the carried-member fit columns) at one bend, each leg a fastened face.
    static ConnectorPlate BuildAngle(ConnectorRow r) => new ConnectorPlate.Angle(
        LegAMm: r.CarriedMemberWidthMm,
        LegBMm: r.CarriedMemberDepthMm,
        WidthMm: Math.Max(SeatFloorMm, r.CarriedMemberWidthMm),
        Stock: Sheet(r));

    // Flat tension strap — no bend, so the stock's forming radius is zero; a wide strap stacks two gauge lines.
    static ConnectorPlate BuildStrap(ConnectorRow r) => new ConnectorPlate.Strap(
        LengthMm: r.CarriedMemberDepthMm,
        WidthMm: r.CarriedMemberWidthMm,
        GaugeLines: r.CarriedMemberWidthMm >= 50.0 ? 2 : 1,
        Stock: Sheet(r) with { BendRadiusMm = 0.0 });

    // Shear-wall hold-down: post seat, cast-in anchor-rod hole (the larger clearance); the stock hole schedule is
    // the post-attachment bolt grid.
    static ConnectorPlate BuildAnchorPlate(ConnectorRow r) => new ConnectorPlate.AnchorPlate(
        SeatWidthMm: r.CarriedMemberWidthMm,
        SeatHeightMm: r.CarriedMemberDepthMm,
        StandoffMm: r.Gauge.BaseThicknessMm * 2.0,
        AnchorBoltHoleDiameterMm: r.Install.ShankDiameterMm + 2.0,
        Stock: Sheet(r));
}

// The attachment policy: the per-fastener datum the row's schedule multiplies plus the wood-duration sensitivity
// flag the Cd scaling reads (a bolted hold-down passes unscaled).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConnectorInstall {
    public static readonly ConnectorInstall Nailed  = new("nailed",  fastenerDesignation: "10d-common",   fastenerKind: FastenerKind.Nail,  perFastenerKn: 0.62, shankDiameterMm: 3.76,   durationSensitive: true);
    public static readonly ConnectorInstall Screwed = new("screwed", fastenerDesignation: "sd9-screw",    fastenerKind: FastenerKind.Screw, perFastenerKn: 1.05, shankDiameterMm: 4.50,   durationSensitive: true);
    public static readonly ConnectorInstall Bolted  = new("bolted",  fastenerDesignation: "through-bolt", fastenerKind: FastenerKind.Bolt,  perFastenerKn: 18.0, shankDiameterMm: 15.875, durationSensitive: false);
    public string FastenerDesignation { get; }
    public FastenerKind FastenerKind { get; }
    public double PerFastenerKn { get; }
    public double ShankDiameterMm { get; }
    public bool DurationSensitive { get; }
}

// AISI S100 cold-formed sheet row — PUBLISHED base-metal (uncoated) and design (as-formed) thickness, the gauge-band
// SS Grade 33/50 minimum yield; DEFINED per-unit-width axial section datum Fy·t — the net-section strength per mm of
// developed width a Rasm.Compute section check reads off the seam, never a re-derived runtime cap here.
public readonly record struct GaugeRow(string Key, int GaugeNumber, double BaseThicknessMm, double DesignThicknessMm, double YieldMpa, string SubstanceId) {
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public double AxialSectionCapacityKnPerMm => YieldMpa * DesignThicknessMm * 1e-3;   // DEFINED: AISI S100 Fy·t per mm width
}

// NDS Table 2.3.2 duration-of-load row (Cd) — the wood-connector scale, disjoint from the EC5 timber kmod: two codes,
// two factors, never conflated.
public readonly record struct DurationRow(string Key, double Cd);

// The admitted three-direction allowable — the factory guards every resisted allowable positive, every UNRESISTED
// column exactly zero (a strap row carrying a download is a transcription fault, never a silently-ignored value),
// and at least one direction resisted. GoverningKn folds ONLY the resisted directions; the indexer reads a
// direction's allowable (0 when unresisted, so a demand there over-stresses).
[ComplexValueObject]
public readonly partial struct LoadResistance {
    public ConnectorType Type { get; }
    public double DownloadKn { get; }
    public double UpliftKn { get; }
    public double LateralKn { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ConnectorType type, ref double downloadKn, ref double upliftKn, ref double lateralKn) {
        bool degenerate =
            type is null ||
            !double.IsFinite(downloadKn) || !double.IsFinite(upliftKn) || !double.IsFinite(lateralKn) ||
            (LoadDirection.Download.Resisted(type) ? !(downloadKn > 0.0) : downloadKn != 0.0) ||
            (LoadDirection.Uplift.Resisted(type) ? !(upliftKn > 0.0) : upliftKn != 0.0) ||
            (LoadDirection.Lateral.Resisted(type) ? !(lateralKn > 0.0) : lateralKn != 0.0);
        if (degenerate)
            validationError = new ValidationError($"<load-resistance-degenerate:{type?.Key}:d={downloadKn:R}:u={upliftKn:R}:l={lateralKn:R}>");
    }

    public double GoverningKn {
        get { LoadResistance self = this; return Type.Resists.Min(direction => direction.Published(self)); }
    }

    public double this[LoadDirection direction] => direction.Resisted(Type) ? direction.Published(this) : 0.0;
}

// The stamped-sheet stock every plate form shares — gauge sheet, AISI forming radius, the fastener hole schedule;
// a flat (unformed) strap carries BendRadiusMm 0.
public readonly record struct PlateStock(
    double SheetThicknessMm,
    double BendRadiusMm,
    string FastenerDesignation,
    FastenerKind FastenerKind,
    double HoleDiameterMm,
    int HoleCount,
    double HolePitchMm);

// The host-materialization body — one [Union] over the four cold-formed forms; the shared PlateStock is BASE-
// CONSTRUCTOR STATE (the SectionProfile gross-fact pattern), so a case carries ONLY its form fields and the host
// reads sheet/hole facts polymorphically off Stock. Every field derives from row columns plus the forming radius
// and fastener shank. NEVER a host brep here.
[Union]
public abstract partial record ConnectorPlate {
    private ConnectorPlate(PlateStock stock) => Stock = stock;
    public PlateStock Stock { get; }

    public sealed record Saddle(double SeatWidthMm, double SeatDepthMm, double SideFlangeHeightMm, double BackFlangeWidthMm, PlateStock Stock) : ConnectorPlate(Stock);
    public sealed record Angle(double LegAMm, double LegBMm, double WidthMm, PlateStock Stock) : ConnectorPlate(Stock);
    public sealed record Strap(double LengthMm, double WidthMm, int GaugeLines, PlateStock Stock) : ConnectorPlate(Stock);
    public sealed record AnchorPlate(double SeatWidthMm, double SeatHeightMm, double StandoffMm, double AnchorBoltHoleDiameterMm, PlateStock Stock) : ConnectorPlate(Stock);
}

// The duration-adjusted allowable receipt the design seam reads.
public readonly record struct ConnectorCapacity(ConnectorType Type, double DownloadKn, double UpliftKn, double LateralKn, double Cd) {
    public double GoverningKn {
        get { ConnectorCapacity self = this; return Type.Resists.Min(direction => direction.Adjusted(self)); }
    }

    public Fin<double> DemandRatio(LoadDemand demand, Op key) {
        ConnectorCapacity self = this;
        return guard(
                LoadDirection.Items.All(direction => direction.Demand(demand) == 0.0 || direction.Resisted(Type)),
                ComponentFault.Capacity(key, "<connector-demand-in-unresisted-direction>"))
            .ToFin()
            .Map(_ => LoadDirection.Items.Max(direction => direction.Demand(demand) == 0.0 ? 0.0 : direction.Demand(demand) / direction.Adjusted(self)));
    }
}

[ComplexValueObject]
public readonly partial struct LoadDemand {
    public double DownloadKn { get; }
    public double UpliftKn { get; }
    public double LateralKn { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double downloadKn, ref double upliftKn, ref double lateralKn) =>
        validationError = double.IsFinite(downloadKn) && downloadKn >= 0.0
            && double.IsFinite(upliftKn) && upliftKn >= 0.0
            && double.IsFinite(lateralKn) && lateralKn >= 0.0
            ? null
            : new ValidationError($"<connector-demand-invalid:d={downloadKn:R}:u={upliftKn:R}:l={lateralKn:R}>");

    public static Fin<LoadDemand> Of(double downloadKn, double upliftKn, double lateralKn, Op key) =>
        Validate(downloadKn, upliftKn, lateralKn, out LoadDemand demand) is { } error
            ? Fin.Fail<LoadDemand>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(demand);
}

// The typed catalogue row AND the capacity owner: vocabulary refs are symbolic (no string re-resolution, no unknown-
// key fault cases), the published allowables AND the report's fastener schedule are raw columns admitted ONCE through
// Allowable, and the design algebra rides the row — GovernedCapacity scales the admitted values by Cd; the schedule
// fidelity bound keeps the Fasteners/install columns load-bearing. The gauge designation rides the GaugeRow constant,
// never a re-admitted discrete column.
public readonly record struct ConnectorRow(
    string Designation, ConnectorType Type, GaugeRow Gauge, ConnectorInstall Install, int Fasteners,
    double CarriedMemberWidthMm, double CarriedMemberDepthMm,
    double DownloadKn, double UpliftKn, double LateralKn) {

    public double FastenerGroupAllowableKn => Fasteners * Install.PerFastenerKn;
    public MaterialId Substance => Gauge.Substance;
    public ConnectorPlate Plate => Type.BuildPlate(this);

    // The ONE admission of the published columns, two disjoint gates: resisted flags come from the type-level
    // direction set, so a strap row carrying a download (or a hanger row missing one) rails ComponentFault.Dimension —
    // a transcription fault; then transcription FIDELITY — every fastener-transferred resisted allowable must sit
    // within the row's own schedule bound Fasteners·PerFastenerKn (the seat-borne hanger download bears on the saddle,
    // exempt via LoadDirection.SeatBorne), railing ComponentFault.Capacity — a certified allowable is NEVER lowered at
    // runtime, it is proven deliverable at seed. The leading Fin.Succ(this) lift anchors the row as a range variable —
    // a struct query lambda cannot read `this` (CS1673).
    public Fin<LoadResistance> Allowable(Op key) =>
        from row in Fin.Succ(this)
        from admitted in LoadResistance.Validate(
                type: row.Type, downloadKn: row.DownloadKn, upliftKn: row.UpliftKn, lateralKn: row.LateralKn,
                out LoadResistance built) is { } error
            ? Fin.Fail<LoadResistance>(ComponentFault.Dimension(key, $"<resistance-mismatch:{row.Type.Key}:{row.Designation}:{error.Message}>"))
            : Fin.Succ(built)
        from delivered in guard(
            row.Type.Resists.Where(d => !d.SeatBorne(row.Type)).All(d => d.Published(admitted) <= row.FastenerGroupAllowableKn),
            ComponentFault.Capacity(key, $"<published-exceeds-fastener-group:{row.Designation}:{row.Fasteners}x{row.Install.PerFastenerKn:R}>"))
        select admitted;

    // The duration-adjusted receipt: the admitted published values (already proven deliverable at admission) scaled by
    // Cd for a wood-driven duration-sensitive install; an unresisted direction reads 0 through the indexer.
    public Fin<ConnectorCapacity> GovernedCapacity(DurationRow duration, Op key) =>
        from row in Fin.Succ(this)
        from allowable in row.Allowable(key)
        let cd = row.Install.DurationSensitive ? duration.Cd : 1.0
        select new ConnectorCapacity(
            row.Type,
            allowable[LoadDirection.Download] * cd,
            allowable[LoadDirection.Uplift] * cd,
            allowable[LoadDirection.Lateral] * cd,
            cd);
}

// --- [TABLES] ------------------------------------------------------------------------------
// 7 AISI S100 gauge rows — PUBLISHED manufacturer base-metal thickness, ≈0.90·base design thickness, gauge-implied
// SS Grade 33 (230 MPa, light 22/20/18/16 ga) / Grade 50 (340 MPa, structural 14/12/10 ga) yield. The thin 22/20 ga
// rows serve the panel#PANEL_FAMILY steel-deck seed (deck-b/deck-a keys) — one cold-formed gauge vocabulary, no parallel deck enum.
public static class Gauges {
    public static readonly GaugeRow Ga22 = new("22ga", 22, 0.759, 0.683, 230.0, "steel.g33");
    public static readonly GaugeRow Ga20 = new("20ga", 20, 0.912, 0.821, 230.0, "steel.g33");
    public static readonly GaugeRow Ga18 = new("18ga", 18, 1.214, 1.092, 230.0, "steel.g33");
    public static readonly GaugeRow Ga16 = new("16ga", 16, 1.519, 1.367, 230.0, "steel.g33");
    public static readonly GaugeRow Ga14 = new("14ga", 14, 1.897, 1.707, 340.0, "steel.g50");
    public static readonly GaugeRow Ga12 = new("12ga", 12, 2.657, 2.391, 340.0, "steel.g50");
    public static readonly GaugeRow Ga10 = new("10ga", 10, 3.416, 3.074, 340.0, "steel.g50");
    public static readonly ImmutableArray<GaugeRow> Rows = [Ga22, Ga20, Ga18, Ga16, Ga14, Ga12, Ga10];
}

// 6 NDS Table 2.3.2 duration rows — PUBLISHED Cd values.
public static class Durations {
    public static readonly DurationRow Permanent   = new("permanent",    0.90);
    public static readonly DurationRow TenYear     = new("ten-year",     1.00);
    public static readonly DurationRow TwoMonth    = new("two-month",    1.15);
    public static readonly DurationRow SevenDay    = new("seven-day",    1.25);
    public static readonly DurationRow WindSeismic = new("wind-seismic", 1.60);
    public static readonly DurationRow Impact      = new("impact",       2.00);
    public static readonly ImmutableArray<DurationRow> Rows = [Permanent, TenYear, TwoMonth, SevenDay, WindSeismic, Impact];
}

// The AUTHORED catalogue: ICC-ES ten-year-duration allowables AND the report's fastener schedule (both PUBLISHED —
// the same evaluation-report table row; no producer package, no cross-product); an unresisted direction carries 0.0
// the type-level set ignores. Typed refs replace the prior string columns and their TryGet re-resolution.
public static class Connectors {
    internal static readonly ComponentStandard Standard = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Aisi);
    internal static readonly MaterialId Galvanized = MaterialId.Of("metal.steel");

    public static readonly ImmutableArray<ConnectorRow> Rows = [
        new("connector.jh-2x6-18ga",        ConnectorType.JoistHanger,  Gauges.Ga18, ConnectorInstall.Nailed,   6,  38.1,  139.7,  4.85,  0.78,  0.0),
        new("connector.jh-2x8-18ga",        ConnectorType.JoistHanger,  Gauges.Ga18, ConnectorInstall.Nailed,   8,  38.1,  184.2,  7.83,  0.78,  0.0),
        new("connector.jh-2x10-16ga",       ConnectorType.JoistHanger,  Gauges.Ga16, ConnectorInstall.Nailed,  10,  38.1,  235.0, 10.45,  1.00,  0.0),
        new("connector.jh-2x12-16ga",       ConnectorType.JoistHanger,  Gauges.Ga16, ConnectorInstall.Nailed,  12,  38.1,  285.8, 12.46,  1.00,  0.0),
        new("connector.jh-2x14-16ga",       ConnectorType.JoistHanger,  Gauges.Ga16, ConnectorInstall.Nailed,  14,  38.1,  336.6, 13.50,  1.00,  0.0),
        new("connector.jh-4x8-14ga",        ConnectorType.JoistHanger,  Gauges.Ga14, ConnectorInstall.Screwed, 12,  88.9,  184.2, 14.23,  1.33,  0.0),
        new("connector.jh-4x10-14ga",       ConnectorType.JoistHanger,  Gauges.Ga14, ConnectorInstall.Screwed, 14,  88.9,  235.0, 17.79,  1.33,  0.0),
        new("connector.jh-4x12-14ga",       ConnectorType.JoistHanger,  Gauges.Ga14, ConnectorInstall.Screwed, 16,  88.9,  285.8, 20.50,  1.33,  0.0),
        new("connector.jh-6x10-12ga",       ConnectorType.JoistHanger,  Gauges.Ga12, ConnectorInstall.Screwed, 18, 139.7,  235.0, 26.70,  1.78,  0.0),
        new("connector.angle-a23-18ga",     ConnectorType.FramingAngle, Gauges.Ga18, ConnectorInstall.Nailed,   6,  38.1,   38.1,  0.0,   2.45,  1.45),
        new("connector.angle-a35-18ga",     ConnectorType.FramingAngle, Gauges.Ga18, ConnectorInstall.Nailed,  12,  38.1,   38.1,  0.0,   3.96,  2.18),
        new("connector.angle-l50-16ga",     ConnectorType.FramingAngle, Gauges.Ga16, ConnectorInstall.Nailed,   8,  50.8,   50.8,  0.0,   4.60,  2.55),
        new("connector.angle-l70-16ga",     ConnectorType.FramingAngle, Gauges.Ga16, ConnectorInstall.Screwed,  6,  63.5,   63.5,  0.0,   5.74,  3.11),
        new("connector.angle-l90-14ga",     ConnectorType.FramingAngle, Gauges.Ga14, ConnectorInstall.Screwed, 10,  88.9,   88.9,  0.0,   8.45,  4.60),
        new("connector.strap-pa18-18ga",    ConnectorType.Strap,        Gauges.Ga18, ConnectorInstall.Nailed,   8,  38.1,  228.6,  0.0,   4.20,  0.0),
        new("connector.strap-cs16-16ga",    ConnectorType.Strap,        Gauges.Ga16, ConnectorInstall.Nailed,  16,  38.1,  600.0,  0.0,   9.34,  0.0),
        new("connector.strap-cs14-14ga",    ConnectorType.Strap,        Gauges.Ga14, ConnectorInstall.Nailed,  24,  50.8,  900.0,  0.0,  14.50,  0.0),
        new("connector.strap-mst27-14ga",   ConnectorType.Strap,        Gauges.Ga14, ConnectorInstall.Nailed,  30,  50.8,  685.8,  0.0,  18.30,  0.0),
        new("connector.strap-cmst16-16ga",  ConnectorType.Strap,        Gauges.Ga16, ConnectorInstall.Nailed,  40,  63.5, 1219.2,  0.0,  24.50,  0.0),
        new("connector.holdown-hd5b-12ga",  ConnectorType.HoldDown,     Gauges.Ga12, ConnectorInstall.Bolted,   2,  63.5,  254.0,  0.0,  23.13,  0.0),
        new("connector.holdown-hdu8-12ga",  ConnectorType.HoldDown,     Gauges.Ga12, ConnectorInstall.Bolted,   2,  63.5,  298.5,  0.0,  29.30,  0.0),
        new("connector.holdown-hd12-10ga",  ConnectorType.HoldDown,     Gauges.Ga10, ConnectorInstall.Bolted,   4,  76.2,  330.0,  0.0,  53.38,  0.0),
        new("connector.holdown-hdu14-10ga", ConnectorType.HoldDown,     Gauges.Ga10, ConnectorInstall.Bolted,   4,  76.2,  384.2,  0.0,  65.40,  0.0)];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-time DetailLane.Realization bag — row names and mint shapes identical to the retired projector switch
// (AccessoryType + the SEPARATE attaching FastenerType token, dimension-only SI mints for the carried-member
// columns); the FastenerType value reads the INSTALL schedule (the actual fastener), never a type-level constant.
public static class ConnectorDetail {
    public static Fin<PropertyBag> Of(ConnectorType type, ConnectorInstall install, double carriedWidthMm, double carriedDepthMm) =>
        from width in ComponentDetail.Measured(DetailSchema.CarriedMemberWidth, SiDim.LengthDim, carriedWidthMm * 1e-3)
        from depth in ComponentDetail.Measured(DetailSchema.CarriedMemberDepth, SiDim.LengthDim, carriedDepthMm * 1e-3)
        select ComponentDetail.RealizationRows(
            ComponentDetail.Token(DetailSchema.AccessoryType, type.IfcAccessoryType),
            ComponentDetail.Token(DetailSchema.FastenerType, install.FastenerKind.IfcPredefinedType),
            width,
            depth);
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The ONE catalogue fold: every authored row admits its allowable once, rails its profile through the parent
// SectionProfile.Rectangle.Of, then Component.Of — Traverse is the rail, a rejected row ABORTS the build; an
// intentional exclusion is an explicit Filter BEFORE construction (the current range stocks every row).
// Sectioned: false — a connector contributes no ComputedSection.
public static class ConnectorSeed {
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Connectors.Rows.ToSeq()
            .Traverse(row =>
                from allowable in row.Allowable(context.Key)
                from profile in SectionProfile.Rectangle.Of(row.CarriedMemberWidthMm, row.CarriedMemberDepthMm, context.Key)
                from detail in ConnectorDetail.Of(row.Type, row.Install, row.CarriedMemberWidthMm, row.CarriedMemberDepthMm)
                from item in Component.Of(
                    ComponentFamily.Connector, row.Designation, profile,
                    IfcBinding.Of("IfcDiscreteAccessory", row.Type.IfcAccessoryType),
                    Coring.None, Connectors.Standard, substanceId: row.Substance, appearanceId: Connectors.Galvanized,
                    detail: Some(detail), context.Key)
                select new ComponentRow(item, Sectioned: false)).As();
}
```

## [03]-[RESEARCH]

- [SEED_FOLD]: REALIZED — the fault-swallowing `Choose` fold and the string-keyed row table are retired: `Connectors.Rows` carries typed vocabulary references (`ConnectorType`/`GaugeRow`/`ConnectorInstall` symbolic refs, deleting the `TryGet` re-resolution and its unknown-key fault cases), and `ConnectorSeed.Rows` `Traverse`s every row through the ONE `Allowable` admission, the railed `SectionProfile.Rectangle.Of`, then `Component.Of` into `Fin<Seq<ComponentRow>>` (`Sectioned: false`), so a malformed row ABORTS the build instead of vanishing. The bespoke `ConnectorSection` payload and its section arm are deleted: the carried-member fit is `SectionProfile.Rectangle` gross state, the IFC stamp is `IfcBinding.Of("IfcDiscreteAccessory", type.IfcAccessoryType)` with the separate attaching-fastener token riding the bag (`AccessoryType`/`FastenerType` tokens + `CarriedMemberWidth`/`CarriedMemberDepth` dimension-only mints — the `FastenerType` value reads the install schedule, correcting the type-level `NAILPLATE` misstamp). The 23 designations (`connector.jh-2x6-18ga` .. `connector.holdown-hdu14-10ga`) and every published allowable survive verbatim, widened by the report's `Fasteners` schedule column (PUBLISHED — the same ICC-ES table row).
- [DIRECTION_VOCABULARY]: `LoadDirection` owns the per-axis resistance, demand, adjustment, and seat-bearing reads. `LoadResistance` admits finite positive resisted values and exact zero unresisted values. `ConnectorCapacity.DemandRatio(LoadDemand, Op)` returns `Fin<double>` and faults when positive demand targets an unresisted direction; unsupported load cannot become an infinity sentinel.
- [ROW_TABLE_CONVERSION]: `GaugeRow` and `DurationRow` remain frozen printed-data tables. `ConnectorInstall` stays a policy `[SmartEnum]` and carries its fastener columns directly; no wrapper or runtime lookup separates the schedule from its consumers.
- [CAPACITY_OWNER]: REALIZED — the design algebra rides the typed `ConnectorRow`: `Allowable(key)` is the single admission of the published three-direction columns through the `LoadResistance` `[ComplexValueObject]` gated by the type-level resisted set (a strap row declaring a nonzero download, a hanger row missing one, or a connector resisting nothing rails `ComponentFault.Dimension` — the unresisted-column-must-be-zero arm makes the transcription gate total, never a silently-ignored value) plus the schedule fidelity gate — every fastener-transferred resisted allowable proven within `Fasteners·PerFastenerKn` (the seat-borne hanger download exempt via `LoadDirection.SeatBorne`), a published value no schedule can deliver railing the disjoint `ComponentFault.Capacity` at seed. The prior runtime `Math.Min(published, bound)` capping is the DELETED form: a fixed 10-nail group bound and a member-width `Fy·t` cap arithmetically falsified certified rows (`jh-6x10-12ga` 26.7 kN collapsed to 8.4; `strap-cmst16` 24.5 to 6.2) — an ICC-ES allowable is the certified SYSTEM capacity, never lowered by a re-derived cap, so the bounds became admission proofs (the per-row `Fasteners` schedule, PUBLISHED) and a seam datum (`AxialSectionCapacityKnPerMm`, the `Rasm.Compute` developed-width read). `GovernedCapacity(duration, key)` scales the admitted values by `Cd` only for a duration-sensitive wood-driven install; `ConnectorCapacity.DemandRatio` owns the unit-check on the receipt (it reads no row state — the fold rides the capacity, not the row). The `ConnectorPlate` `[Union]` and the `ConnectorType.BuildPlate` delegate column are kept re-typed over the row, the shared sheet/forming/hole-schedule columns collapsed into ONE `PlateStock` product carried as union base state (the `SectionProfile` gross-fact pattern — the per-form repetition of `SheetThicknessMm`/`BendRadiusMm`/`HoleDiameterMm`/`HoleCount`/`HolePitchMm` across four builders is the deleted form), every remaining plate field tracing to row columns plus the AISI `1.5·t` forming radius and the fastener shank — the host-neutral receipt the host materializes, never a brep here.
- [STANDARD_CITATION]: REALIZED as authority rows — the AISI S100 and NDS bodies have no typed code object among admitted packages (the typed standards floor is EN/Eurocode-only), so the citation is `ComponentAuthority.Aisi` on the rows' `ComponentStandard` plus per-column provenance; a typed AISI/NDS citation lands as one column swap if a producer is ever admitted, with the fastener page's typed `En1993Part.Part1_8` pattern the template.
- [IFC_DISCRETE_ACCESSORY_WIRE]: REALIZED — a framing connector round-trips as `IfcDiscreteAccessory` over the verified `IfcDiscreteAccessoryTypeEnum` tokens {`SHOE`, `BRACKET`, `ANCHORPLATE`}, fastened by a SEPARATE `IfcMechanicalFastener` over the verified {`NAIL`, `SCREW`, `BOLT`} tokens the install schedule stamps, related at the `Rasm.Bim` egress (the prior type-level `NAILPLATE` on nailed hangers/straps was a misstamp — a hanger is nailed, not a toothed mending plate); the `IfcDesignation` column is the `ObjectType` discriminant read past the predefined enum. This page emits only the portable token columns and the bag rows the connection reader recovers one-hop; the per-token egress gate validates every string against the generated roster, so entity choice and Pset naming stay Bim-owned.
