# [MATERIALS_CMU]

THE CMU SEED PAGE GROUNDED IN ASTM C90-16a + TMS 602-16/-22 + ACI 216.1 + NCMA TEK 6-2B/7-1D. A concrete block is one `ComponentRow` the ONE `component#COMPONENT_SEED` generator mints from `CmuSeed.Roster` under `CmuSeed.Law` — `ComponentFamily.Cmu` (`ComponentClass.Minor`, `DetailLane.Realization`, `admits: CellularRectangle`, `crossNominal: GrossRectangleMm.WidthMm`), never a `ConcreteBlock` type and never a bespoke payload record. The bed-plane geometry is the parent `SectionProfile.CellularRectangle` (`WidthMm` the through-wall thickness — the family cross nominal, `DepthMm` the along-wall unit length, `Seq<VoidCell>` the per-cell fill-state lattice the private `Lattice` generator lays from the ASTM face-shell and web columns); the vocabulary is the five FORM-law SmartEnums (`CmuGrade`/`CmuDensity`/`CmuAggregate`/`CmuSpecialUnit`/`CmuFinish`) beside the `component#MATERIAL_GRADE` `MaterialGrade` cmu rows whose `GradeProperties.Cmu` arm carries the TMS 602 Table 2 strengths; the realization columns ride the typed `CmuRow` table with per-column provenance. `NetSection`/`GroutedSection` are ONE `SectionSolver.Solve` `CellularRectangle` arm, whose bed-plane solve IS the TMS 402 net cross-section (net area under axial compression, both-axis moduli for out-of-plane flexure): the `VoidCell.Grouted` flag selects the result through one code path — the seeded lattice yields the AS-BUILT net (only ungrouted cells void, a fully-grouted unit the solid rectangle), and the DESIGN net is the same solve over `CmuPhysics.DesignCells`, the manufactured-basis projection the `Coring` bucket reads — so this page calls no solver and owns no perimeter builder.

DIMENSIONS ARE DERIVED, NOT AUTHORED BESIDE THEIR MODULE. ASTM C90 is an INCH-POUND standard whose SI values are stated as mathematical conversions for information only, and it publishes no dimension table at all — it publishes tolerances about a *specified* dimension. A `CmuRow` therefore carries the COORDINATING MODULE (the nominal 4/6/8/10/12 in width, the 8 in course, the 16 in run) and every actual dimension is that module less the one ASTM `3/8 in` coordinating joint, which makes the module arithmetic exact by construction rather than a second authored series a reader must check against the first. `CmuSeed.Module` then re-adds the joint and lands back on the nominal, and the C90 §6.1 permissible deviation rides as the manufacturing tolerance band beside it.

`CmuPhysics` owns fire rating, thermal resistance, self-weight, equivalent thickness, solid fraction, and grout fraction over `(lattice, CmuDensity, CmuAggregate, CmuSpecialUnit)` as bag-free derivations, and the family's `DetailLane.Realization` bag carries the seed-computed `DetailSchema.ProfileSubtype` IFC profile-def token off `CmuPhysics.IfcSubtypeOf` beside its row's own `EvidenceGrade` — the wire datum the `Rasm.Bim` egress profile lane reads; the physics axes never land as bag rows. `CmuSeed.Table` is the `SeedJoin`-built `ComponentId`-keyed typed join from an M7-resolved component to its strength, density, aggregate, finish, and molding axes. The `GradeProperties.Cmu` arm carries the TMS 602 `f'm` and its optional Type-N unit-strength column with the mortar-keyed read and the unit-strength-method inversion landed here, and `masonry#MASONRY_FAMILY` `RuptureModulus.For` supplies the mortar-keyed flexural-tension row the capacity producer derives.

## [01]-[INDEX]

- [02]-[CMU_FAMILY]: the five SmartEnums (`CmuGrade` the ASTM C90 §5.3/§5.4 hollow-solid form, `CmuDensity` the C90 Table 2 class with its NCMA-derived conductivity, `CmuAggregate` the four ACI 216.1 fire categories keyed by rating period with the blended-aggregate rule, `CmuSpecialUnit` molding geometry, `CmuFinish` architectural surface), the `GradeProperties.Cmu` TMS 602-16/-22 Table 2 reads with the `MortarType` inversion, the `CmuRow` provenance-columned seed table over coordinating modules, the `CmuPhysics` fire/thermal/mass receipt with the geometry-derived `Coring` bucket and the `IfcSubtypeOf` wire token, and the `CmuSeed` set (`Roster`/`Law` seeding the `DetailSchema.Realization` bag + the `Properties` seam lowering + `Module` coursing projection + `Capacity` the basis-threaded producer + the `SeedJoin` axis join + the private `Lattice`).

## [02]-[CMU_FAMILY]

- Owner: `CmuSeed` the roster, the seed law, and the capacity producer; `CmuPhysics` the ONE physical receipt; the five FORM-law SmartEnums (runtime key lookup + derivation columns, so they STAY per `SEED_ROW_LAW` tier 3); the `GradeProperties.Cmu` partial members the Table 2 columns mean; `CmuRow` the AUTHORED standards table (no admitted producer exists); `CmuSeed.Table` the railed `ComponentId`-keyed axis join an axis consumer pairs with the M7-resolved `Component.Profile` (the ONE legal axis path — the `DetailLane.Realization` bag carries the wire and evidence rows, never an axis column).
- Cases: grade {hollow, solid} (ASTM C90 §5.3/§5.4, the form alone — the solid net-area floor has its own owner); strength {the five `ComponentFamily.Cmu` `MaterialGrade` rows f2000..f3000, TMS 602-16/-22 Table 2 `f'm` + the two mortar columns, PUBLISHED}; density {lightweight <1680, medium 1680–2000, normal ≥2000 kg/m³, each carrying its C90 absorption caps and its NCMA-band-derived conductivity}; aggregate {the four ACI 216.1 / IBC `722.3.2` categories, each its published equivalent-thickness cells keyed by RATING PERIOD}; special {standard, bond-beam, knockout, channel, lintel, sash, control-joint, open-end}; finish {precision, split-face, scored, ribbed}. A unit is one `CmuRow`; a new grade/strength/density/aggregate/special/finish is one case/row — never a per-block type.
- Entry: `ComponentSeed.Rows(context, CmuSeed.Roster, CmuSeed.Law)` — this page states the roster and the policy, never the fold. The law's coherence proves grade/form, fill/reinforcement correspondence, the C90 Table 1 face-shell floor with its footnote-B split residual, the normalized-web-area floor, and the C90 NOTE 5 density population TOGETHER, so a malformed row names every column it broke in one verdict instead of the first hiding the rest; degenerate, out-of-bounds, or overlapping cells fault inside `CellularRectangle.Of`, and every failure aborts the catalogue. `ComponentRow.Sectioned` reads the constructed profile's own topology, so the M7 map carries the admitted as-built section by derivation rather than by assertion.
- Packages: Rasm.Numerics (`PositiveMagnitude` via the parent profile factories), Rasm.Domain (`Context`/`Op`, the kernel `Tolerance`/`ToleranceLane` the blend closure admits through), Rasm.Element (`MaterialId`, `EvidenceGrade`, the seam `DetailSchema`/`PropertyCategory` currencies), Thinktecture.Runtime.Extensions (`[SmartEnum]`, generated `TryGet`/`Items`/`Switch`; `libs/csharp/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Validation`/`Fin`), the parent `component#COMPONENT_OWNER`/`#SECTION_PROFILE`/`#MATERIAL_GRADE`/`#COMPONENT_SEED` owners and `masonry#MASONRY_FAMILY` for the shared `RuptureModulus`, `WallAcoustics`, and `RatingPeriod`. The cmu generative data is AUTHORED in-fence; ONLY the section integral crosses to VividOrange, through the parent solver (`.api/api-vividorange-sections-sectionproperties.md`).
- Growth: a new ASTM unit (metric A-series, half-high, architectural) is one `CmuRow`; a grouting/reinforcing variant is row columns (`GroutedCells`/`ReinforcedCells`/`RebarBarMm`); molding and finish variants are `CmuSpecialUnit`/`CmuFinish` rows the host extrudes and the lattice reads; a further published fire period is one `RatingPeriod` row and one cell per aggregate that publishes it — never a parallel section owner, never a solver edit.
- Boundary: column provenance per `SEED_ROW_LAW`. PUBLISHED: the C90 Table 1 face-shell minima and the single web minimum, the C90 Table 1 normalized-web-area floor, the C90 Table 2 density bands, absorption caps, and net-area compressive floors, the C90 §5.4.1 solid net-area floor, the C90 §6.1 permissible deviation, the TMS 602 Table 2 strengths, the ACI 216.1 equivalent thicknesses and blended-aggregate rule, the NCMA TEK 6-2B density-resistivity bands, and the ASTM C476 grout density. DEFINED: every actual dimension, which is its coordinating module less the ASTM joint by the standard's own coordination. AUTHORED: the per-class representative oven-dry density inside each published band, and the molding fractions. C90 publishes ONE web thickness minimum rather than a per-width end/cross split, so both web columns seed at that minimum while the SPLIT itself stays real geometry (an open-end unit drops its end webs so the end cells run to the unit ends) — and the normalized web area, whose C140 measurement convention this estate does not possess, rides a DECLARED `Option` column gated against the published floor only where a row declares one, never a computed claim. The wire spelling is the AS-BUILT occupancy derivation `CmuPhysics.IfcSubtypeOf(cell)` (`IfcArbitraryProfileDefWithVoids` iff any UNGROUTED cell remains — the single-void `IfcRectangleHollowProfileDef` cannot carry two distinct cells — `IfcRectangleProfileDef` for a solid or fully-grouted lattice), the derived token seeded as the `DetailSchema.ProfileSubtype` realization-bag row — Bim references no AEC peer, so the wire spelling is carried row data, never a cross-package call; the manufacturing grade never contradicts the grouted state; the element stamp is the `ComponentFamily.Cmu.Ifc` concrete leaf, whose object-type discriminator keeps it distinct from the clay sibling's leaf under the reverse type-candidate read. `DraftDegrees`/`FaceShellFlareMm` are captured generative columns the host materialization reads off the seed table — `VoidCell` carries fill-state only.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;

// The cmu seed declares in the ONE Rasm.Materials.Component namespace; MortarType, RuptureModulus, WallAcoustics,
// RatingPeriod, and every parent owner resolve by bare name, and the ComponentFamily.Cmu policy row binds
// CmuSeed.Roster/Law/Capacity.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The unit FORM, ASTM C90 §5.3 hollow against §5.4 solid — a MANUFACTURING classification independent of fill state.
// The §5.4.1 solid net-area floor has ONE owner (RuptureModulus.SolidNetFloor, which the Coring bucket compares the
// design net against), so this row names the form and restates no number; it carries NO hollow column either, the
// form BEING the row. Load-bearing is likewise not a column — C90 §1.1 suits both applications, so the governing
// SPECIFICATION is what ComponentStandard already carries — and the IFC subtype follows AS-BUILT occupancy, so a
// fully-grouted hollow unit never emits a void-bearing one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuGrade {
    public static readonly CmuGrade Hollow = new("hollow");
    public static readonly CmuGrade Solid  = new("solid");
}

// The CMU arm's reads, co-located with the family that owns it: component#MATERIAL_GRADE declares the TMS 602-16/-22
// Table 2 columns — FmMpa the specified assemblage f'm the design seam takes, NOT the unit strength — and this page
// states the two directions of the unit-strength method over them.
public partial record GradeProperties {
    public sealed partial record Cmu {
        // The mortar-band column read through the generated exhaustive Switch — a new MortarType row breaks HERE at
        // compile time, never an ==-chain a new row silently falls past. Type O/K sit below the TMS 602 loadbearing
        // floor and qualify for no class.
        public Option<double> RequiredUnitMpa(MortarType mortar) => mortar.Switch(
            state: this,
            m: static strength => Some(strength.NetUnitMsMpa),
            s: static strength => Some(strength.NetUnitMsMpa),
            n: static strength => strength.NetUnitNMpa,
            o: static _ => Option<double>.None,
            k: static _ => Option<double>.None);
    }
}

// The proven-arm one-hop the capacity producers bind ONCE at their receipt boundary (the reinforcement RebarArm
// form) — the masonry receipts then store the ARM, so a foreign-armed grade is unrepresentable past the bind.
public sealed partial class MaterialGrade {
    public Option<GradeProperties.Cmu> CmuArm => Columns is GradeProperties.Cmu arm ? Some(arm) : None;
}

// The unit-strength method INVERTED, on the grade owner because its answer is a grade: the highest-f'm cmu row the
// supplied net-area unit strength qualifies for under the mortar band, a below-floor unit answering None rather than
// the weakest class. Bounded to the cmu family, so no other family's row can elect across.
public sealed partial class MaterialGrade {
    public static Option<MaterialGrade> CmuOf(double netUnitStrengthMpa, MortarType mortar) =>
        !double.IsFinite(netUnitStrengthMpa) || netUnitStrengthMpa <= 0.0
            ? Option<MaterialGrade>.None
            : toSeq(toSeq(Items)
                    .Choose(static row => row.Columns is GradeProperties.Cmu arm ? Some((Row: row, Arm: arm)) : None)
                    .Filter(pair => pair.Arm.RequiredUnitMpa(mortar).Exists(required => netUnitStrengthMpa >= required))
                    .OrderByDescending(static pair => pair.Arm.FmMpa))
                .Map(static pair => pair.Row)
                .Head;
}

// The ASTM C90 Table 2 density classification, its band EDGES carried as data so a membership read is a table read
// rather than a literal repeated at each comparison; the representative oven-dry density inside each band is
// AUTHORED, C90 banding densities without naming a representative. CONDUCTIVITY IS DERIVED, never stored: NCMA TEK
// 6-2B publishes resistance per inch as a BAND against density, so a stored per-class conductivity could only agree
// with that band or contradict it — the read takes the band midpoint and carries the range beside it. The absorption
// caps and compressive floors are separate published requirements a submittal lane checks independently.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuDensity {
    public static readonly CmuDensity Lightweight = new("lightweight", ovenDryKgPerM3: 1600.0, lowerBoundKgPerM3: Option<double>.None, upperBoundKgPerM3: Some(1680.0),
        maxAbsorptionKgPerM3: 288.0, maxAbsorptionIndividualKgPerM3: 320.0);
    public static readonly CmuDensity Medium = new("medium", ovenDryKgPerM3: 1840.0, lowerBoundKgPerM3: Some(1680.0), upperBoundKgPerM3: Some(2000.0),
        maxAbsorptionKgPerM3: 240.0, maxAbsorptionIndividualKgPerM3: 272.0);
    public static readonly CmuDensity Normal = new("normal", ovenDryKgPerM3: 2160.0, lowerBoundKgPerM3: Some(2000.0), upperBoundKgPerM3: Option<double>.None,
        maxAbsorptionKgPerM3: 208.0, maxAbsorptionIndividualKgPerM3: 240.0);

    public double OvenDryKgPerM3 { get; }
    public Option<double> LowerBoundKgPerM3 { get; }
    public Option<double> UpperBoundKgPerM3 { get; }
    public double MaxAbsorptionKgPerM3 { get; }
    public double MaxAbsorptionIndividualKgPerM3 { get; }

    // ASTM C90 Table 2 — one net-area compressive requirement across all three density classes, average of three
    // units and individual unit. A single pair, because the standard prints a single pair.
    public const double MinNetAreaCompressiveMpa = 13.8;
    public const double MinNetAreaCompressiveIndividualMpa = 12.4;
    // C90 NOTE 5 — the overall range oven-dry densities generally fall within; the seed proves its representatives lie
    // inside it, so an authored representative can never wander outside the population the standard describes.
    public const double PopulationFloorKgPerM3 = 1360.0;
    public const double PopulationCeilingKgPerM3 = 2320.0;

    public bool Holds(double densityKgPerM3) =>
        LowerBoundKgPerM3.Match(Some: lo => densityKgPerM3 >= lo, None: () => true)
        && UpperBoundKgPerM3.Match(Some: hi => densityKgPerM3 < hi, None: () => true);

    public double ConductivityWPerMK => ConcreteResistivity.ConductivityAt(OvenDryKgPerM3);
    public (double LowWPerMK, double HighWPerMK) ConductivityBandWPerMK => ConcreteResistivity.BandAt(OvenDryKgPerM3);
}

// NCMA TEK 6-2B — the PUBLISHED concrete thermal-resistance band against oven-dry density, transcribed in the units
// the table prints (hr·ft²·°F per Btu·inch) and converted once here. Resistance per inch is a BAND rather than a
// point because concrete of one density spans a real conductivity range, so the table is the primary and every
// conductivity on this page is its projection.
public static class ConcreteResistivity {
    // The imperial-to-SI conversion for thermal conductivity, applied once at the boundary of this table.
    const double BtuInchPerHrFt2FToWPerMK = 0.1442279;
    const double PoundPerFt3ToKgPerM3 = 16.018463;

    // Density (lb/ft³) → resistance per inch, low and high. PUBLISHED verbatim.
    static readonly Seq<(double Pcf, double RLow, double RHigh)> Bands = Seq(
        (85.0,  0.23, 0.34),
        (95.0,  0.18, 0.28),
        (105.0, 0.14, 0.23),
        (115.0, 0.11, 0.19),
        (125.0, 0.08, 0.15),
        (135.0, 0.07, 0.12),
        (140.0, 0.06, 0.11));

    // The mortar resistance the same table publishes beside the concrete rows — the joint path an assembly-level
    // fold reads, carried here because it belongs to this table and nowhere else.
    public static readonly double MortarConductivityWPerMK = BtuInchPerHrFt2FToWPerMK / 0.10;

    // The band at a density, linearly interpolated between the two bracketing published rows and clamped to the
    // table's own ends — an extrapolation past 85 or 140 lb/ft³ would state a resistance the table does not carry.
    public static (double LowWPerMK, double HighWPerMK) BandAt(double densityKgPerM3) {
        double pcf = Math.Clamp(densityKgPerM3 / PoundPerFt3ToKgPerM3, Bands.Head.Map(static b => b.Pcf).IfNone(85.0), Bands.Last.Map(static b => b.Pcf).IfNone(140.0));
        (double Pcf, double RLow, double RHigh) lo = Bands.Filter(b => b.Pcf <= pcf).Last.IfNone(Bands.Head.IfNone((85.0, 0.23, 0.34)));
        (double Pcf, double RLow, double RHigh) hi = Bands.Filter(b => b.Pcf >= pcf).Head.IfNone(Bands.Last.IfNone((140.0, 0.06, 0.11)));
        double t = hi.Pcf > lo.Pcf ? (pcf - lo.Pcf) / (hi.Pcf - lo.Pcf) : 0.0;
        // Resistance interpolates, then inverts: the HIGH resistance is the LOW conductivity, so the pair swaps ends.
        return (BtuInchPerHrFt2FToWPerMK / (lo.RHigh + (hi.RHigh - lo.RHigh) * t),
                BtuInchPerHrFt2FToWPerMK / (lo.RLow + (hi.RLow - lo.RLow) * t));
    }

    // The estate's stated reading of the published band: its midpoint. One derivation, one place, so a density row
    // and a Compute thermal runner never disagree about which end of a published range they took.
    public static double ConductivityAt(double densityKgPerM3) =>
        BandAt(densityKgPerM3) is var band ? (band.LowWPerMK + band.HighWPerMK) / 2.0 : 0.0;
}

// The ACI 216.1 / IBC 722.3.2 fire aggregate — the FOUR published categories and no more, since the standard groups
// calcareous with siliceous gravel and expanded slag with pumice. OneHourMm is REQUIRED, being the power law's own
// reference c_n that every category prints; every further tabulated period rides Further as a (RatingPeriod,
// thickness) cell, so a period a category does not publish is absent from its row and reports not-applicable rather
// than interpolating a certificate. The cells key on the RatingPeriod vocabulary because a rating is ISSUED at a
// tabulated period — the period is a row and the match is identity, never a double compared against an epsilon.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuAggregate {
    public static readonly CmuAggregate CalcareousOrSiliceousGravel = new("calcareous-or-siliceous-gravel", oneHourMm: 71.1, further: Seq((RatingPeriod.ThreeHour, 135.0)));
    public static readonly CmuAggregate LimestoneCindersOrSlag      = new("limestone-cinders-or-slag",      oneHourMm: 68.6, further: Seq<(RatingPeriod, double)>());   // limestone / cinders / air-cooled slag
    public static readonly CmuAggregate ExpandedClayShaleOrSlate    = new("expanded-clay-shale-or-slate",   oneHourMm: 66.0, further: Seq((RatingPeriod.ThreeHour, 112.0)));
    public static readonly CmuAggregate ExpandedSlagOrPumice        = new("expanded-slag-or-pumice",        oneHourMm: 53.3, further: Seq<(RatingPeriod, double)>());   // most fire-efficient category
    public double OneHourMm { get; }
    public Seq<(RatingPeriod Period, double Mm)> Further { get; }

    public Option<double> RequiredThicknessMm(RatingPeriod period) =>
        period == RatingPeriod.OneHour
            ? Some(OneHourMm)
            : Further.Find(cell => cell.Period == period).Map(static cell => cell.Mm);

    // The BLENDED-AGGREGATE rule Tr = Σ(Ti · Vi), a published operation rather than a note because a blended unit is
    // an ordinary product. MODAL_ARITY: one entrypoint over the mix, the single-aggregate call its one-element case.
    // The volume closure is a DIMENSIONLESS residual and admits through the kernel Conservation lane — the one lane
    // whose band and dimension both take a fraction sum — so a band outside it refuses at that admission instead of
    // quietly widening the closure every blend is judged against.
    public static Fin<double> BlendedThicknessMm(RatingPeriod period, Op key, params ReadOnlySpan<(CmuAggregate Aggregate, double Fraction)> mix) {
        Seq<(CmuAggregate Aggregate, double Fraction)> blend = toSeq([.. mix]);
        return
            from closure in Tolerance.Of(ToleranceLane.Conservation, VolumeClosureBand, key)
            from unitVolume in guard(!blend.IsEmpty && Math.Abs(blend.Sum(static m => m.Fraction) - 1.0) <= closure.Value,
                new KernelFault.OutOfRange(nameof(blend), blend.Sum(static m => m.Fraction), "fractions summing to one", Some(key)))
            from weighted in blend.Traverse(m => m.Aggregate.RequiredThicknessMm(period)
                .ToFin(new ComponentFault.FireThicknessMissing(key, period))
                .Map(required => required * m.Fraction)).As()
            select weighted.Sum();
    }

    const double VolumeClosureBand = 1e-9;
}

// The NCMA special-unit molding geometry, every column of which the LATTICE reads. CrossWebFraction is the surviving
// cross-web height as a share of the moulded web, and at ZERO the cells merge into the one continuous trough a
// channel or lintel unit is — the trough derived from the column, never declared as a second shape. Its depth share
// is what keeps that merged cell from voiding the whole unit: a trough removes VOLUME over its own depth, so mass and
// equivalent thickness read the share while the bed plane reads the merged footprint. EndWebsPresent stays its own
// column because dropping the end webs is a TOPOLOGY change rather than a proportion.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuSpecialUnit {
    public static readonly CmuSpecialUnit Standard     = new("standard",      endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit BondBeam     = new("bond-beam",     endWebsPresent: true,  crossWebFraction: 0.25, troughDepthFraction: 0.55, controlSlotFraction: 0.00);   // knocked-down cross webs form the horizontal-bar channel
    public static readonly CmuSpecialUnit Knockout     = new("knockout",      endWebsPresent: true,  crossWebFraction: 0.50, troughDepthFraction: 0.00, controlSlotFraction: 0.00);   // pre-scored webs for field removal
    public static readonly CmuSpecialUnit Channel      = new("channel",       endWebsPresent: true,  crossWebFraction: 0.00, troughDepthFraction: 0.70, controlSlotFraction: 0.00);   // continuous U-trough
    public static readonly CmuSpecialUnit Lintel       = new("lintel",        endWebsPresent: true,  crossWebFraction: 0.00, troughDepthFraction: 0.85, controlSlotFraction: 0.00);   // deep beam trough
    public static readonly CmuSpecialUnit Sash         = new("sash",          endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.50);   // jamb groove for a frame / sealant
    public static readonly CmuSpecialUnit ControlJoint = new("control-joint", endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 1.00);   // full-depth vertical shear key
    public static readonly CmuSpecialUnit OpenEnd      = new("open-end",      endWebsPresent: false, crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.00);   // A/H-block — end webs omitted to drop over rebar
    public bool EndWebsPresent { get; }
    public double CrossWebFraction { get; }
    public double TroughDepthFraction { get; }
    public double ControlSlotFraction { get; }

    // The cells a unit's cross webs still separate: a web knocked to nothing separates nothing, so the moulded cell
    // count collapses to one continuous trough and the lattice lays that instead of a partition it does not have.
    public int SeparatedCells(int mouldedCells) => CrossWebFraction > 0.0 ? Math.Max(1, mouldedCells) : 1;
    public double CrossWebMm(double mouldedCrossWebMm) => mouldedCrossWebMm * CrossWebFraction;
    // The VOLUME share a trough removes from the merged cell: unit height less the trough's own depth is still solid
    // beneath it, so the void a trough contributes is its depth share and never its whole footprint.
    public double TroughVoidShare => Math.Clamp(TroughDepthFraction, 0.0, 1.0);
}

// The ASTM C90 §7 architectural surface finish. SplitDepthMm is STRUCTURAL as well as visual and the lattice reads
// it — splitting removes face-shell material, so the effective shell is the moulded shell less the split depth, and
// Table 1 footnote B floors that residual however deep the split. Score and rib are relief the host extrudes and
// change no section. A BURNISHED unit is NOT a row: grinding changes no dimension this page carries, so it differs
// on the APPEARANCE axis the component already owns as its own MaterialId.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuFinish {
    public static readonly CmuFinish Precision = new("precision",  splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish SplitFace = new("split-face", splitDepthMm: 6.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Scored    = new("scored",     splitDepthMm: 0.0, scoreCount: 2, scoreSpacingMm: 130.0, ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Ribbed    = new("ribbed",     splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 8, ribDepthMm: 10.0);
    public double SplitDepthMm { get; }
    public int ScoreCount { get; }
    public double ScoreSpacingMm { get; }
    public int RibCount { get; }
    public double RibDepthMm { get; }

    // ASTM C90 Table 1 footnote B — a split surface may fall below the tabulated face-shell minimum but never below
    // this residual, which is the floor the seed proves each split row against.
    public const double SplitResidualFloorMm = 19.1;

    public double EffectiveFaceShellMm(double mouldedFaceShellMm) => mouldedFaceShellMm - SplitDepthMm;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The AUTHORED seed row: TYPED axis singletons + coordinating MODULES, so an unknown axis key is unrepresentable and
// the fold carries no per-axis TryGet lift. Every actual dimension DERIVES from its module by subtracting the one
// ASTM joint, so the module arithmetic cannot drift from what it coordinates. EndWebMm and CrossWebMm seed at the
// single published web minimum, the split between them being real topology rather than two printed numbers.
// NormalizedWebAreaMm2PerM2 is the manufacturer's DECLARED value where one exists — its C140 measurement convention
// is one this estate does not possess — so an undeclared row carries absence and the floor gates only what is
// declared. Special/Finish are init columns: a SmartEnum row is no compile-time constant and cannot ride a
// positional default.
public readonly record struct CmuRow(
    string Designation, CmuGrade Grade, MaterialGrade Strength, CmuDensity Density, CmuAggregate Aggregate,
    double WModuleMm, double HModuleMm, double LModuleMm,
    double FaceShellMm, double EndWebMm, double CrossWebMm, int Cells,
    int GroutedCells = 0, int ReinforcedCells = 0, double RebarBarMm = 0.0,
    double FaceShellFlareMm = 0.0, double DraftDegrees = 1.5) {
    public CmuSpecialUnit Special { get; init; } = CmuSpecialUnit.Standard;
    public CmuFinish Finish { get; init; } = CmuFinish.Precision;
    public Option<double> NormalizedWebAreaMm2PerM2 { get; init; } = Option<double>.None;
    // The dimensional columns are DEFINED — each actual dimension is its published coordinating module less the one
    // published joint, by ASTM C90's own coordination rather than by a second authored series.
    public EvidenceGrade Source { get; init; } = EvidenceGrade.Defined;

    public double WMm => WModuleMm - CmuSeed.CoordinatingJointMm;
    public double HMm => HModuleMm - CmuSeed.CoordinatingJointMm;
    public double LMm => LModuleMm - CmuSeed.CoordinatingJointMm;

    // The face shell that survives manufacture — a split face loses its split depth, every other finish loses nothing.
    public double EffectiveFaceShellMm => Finish.EffectiveFaceShellMm(FaceShellMm);
}

// The ONE physical receipt over (CellularRectangle, CmuDensity, CmuAggregate, CmuSpecialUnit) — every quantity is a
// per-face or per-length ratio in which the unit HEIGHT cancels exactly, so seed time and any axis-holding consumer
// (capacity, a Compute fire/thermal runner, the host) compute the identical receipt off the profile fill-state and the
// molding row. The molding row enters because a TROUGH is the one void whose depth is not the unit's: a channel or
// lintel unit's merged cell is a partial-height scoop, and pricing it as a full-height cell would under-state the
// equivalent thickness, the self-weight, and the fire rating of exactly the units that carry the most concrete.
public readonly record struct CmuPhysics(
    double EquivalentThicknessMm,        // ACI 216.1 te: (gross − ungrouted void volume) / length — grout counts as solid
    Option<RatingPeriod> FireRating,     // the ACI 216.1 / IBC 722.3.2 power law floored onto the published periods
    double SelfWeightKnPerM2,            // oven-dry net solid + grouted-cell grout, per wall-face m²
    double ThermalResistanceM2KPerW,     // NCMA TEK 6-2B isothermal planes, material-only (films are the assembly's)
    double SolidFraction,                // design net (every cell open) / gross — the manufactured basis
    double GroutedSolidFraction,         // as-built net / gross
    double GroutedCellFraction) {        // grout-filled share of the cell void, lattice-honest

    public const double GroutDensityKgPerM3 = 2243.0;        // ASTM C476 grout — 140 pcf
    public const double GroutConductivityWPerMK = 1.40;      // a grouted cell conducts like normal-weight concrete
    public const double CellAirResistanceM2KPerW = 0.18;     // ASHRAE vertical air-cavity — the ungrouted-cell path
    const double GravityMPerS2 = 9.80665;

    // The rating is an OPTION because a wall thinner than its category's one-hour equivalent thickness earns no
    // tabulated period at all — the prior floor-to-zero read spelled that absence as a zero-hour rating, which the
    // seam then minted as a real EN 13501-2 certificate of nothing.
    public static CmuPhysics Of(SectionProfile.CellularRectangle cell, CmuDensity density, CmuAggregate aggregate, CmuSpecialUnit special) {
        double w = cell.WidthMm.Value, len = cell.DepthMm.Value, gross = w * len;
        // A trough voids only its own depth share of the cell it merged; every other cell voids its full height.
        double depthShare = special.TroughDepthFraction > 0.0 ? special.TroughVoidShare : 1.0;
        double allVoid = cell.Cells.Sum(static c => c.WidthMm * c.HeightMm);
        double openVoid = cell.Cells.Filter(static c => !c.Grouted).Sum(static c => c.WidthMm * c.HeightMm);
        double net = gross - allVoid;
        double te = (gross - openVoid * depthShare) / len;
        return new(
            EquivalentThicknessMm: te,
            FireRating: RatingPeriod.Floor(Math.Pow(te / aggregate.OneHourMm, 1.7)),
            SelfWeightKnPerM2: ((gross - allVoid * depthShare) * density.OvenDryKgPerM3 + (allVoid - openVoid) * depthShare * GroutDensityKgPerM3) * GravityMPerS2 / (len * 1e6),
            ThermalResistanceM2KPerW: IsothermalPlanes(cell, density),
            SolidFraction: gross > 0.0 ? Math.Clamp(net / gross, 0.0, 1.0) : 1.0,
            GroutedSolidFraction: gross > 0.0 ? Math.Clamp((gross - openVoid) / gross, 0.0, 1.0) : 1.0,
            GroutedCellFraction: allVoid > 0.0 ? (allVoid - openVoid) / allVoid : 0.0);
    }

    // The areal mass the masonry#MASONRY_FAMILY WallAcoustics mass law reads — one fold for both coursing families.
    public double ArealMassKgPerM2 => SelfWeightKnPerM2 * 1000.0 / GravityMPerS2;

    // The DESIGN lattice — every cell open, the MANUFACTURED basis a unit is classified and sold on, PROJECTED from
    // the as-built cells rather than stored beside them: a grouted unit is classified by the block that was made, not
    // by the wall it ended in.
    public static Seq<VoidCell> DesignCells(Seq<VoidCell> cells) =>
        cells.Map(static c => c with { Grouted = false, Reinforced = false });

    // Two face shells in SERIES with the core, the core a PARALLEL combination of the solid-web path and the per-cell
    // paths. Each cell resists over ITS OWN through-wall width with the remaining core depth conducting as solid body
    // beside it — a narrow cell in a wide core is a short cavity in series with concrete, which is what the
    // isothermal-planes method models; scaling a cavity's area resistance by a width ratio inverts the sense, making
    // a narrow cell read LESS resistant than a wide one. The core LAYER thickness stays the widest cell.
    static double IsothermalPlanes(SectionProfile.CellularRectangle cell, CmuDensity density) {
        double k = density.ConductivityWPerMK, widthM = cell.WidthMm.Value / 1000.0, len = cell.DepthMm.Value;
        if (cell.Cells.IsEmpty) { return widthM / k; }
        double coreWidthM = cell.Cells.Max(static c => c.WidthMm) / 1000.0;
        double webConductance = (len - cell.Cells.Sum(static c => c.HeightMm)) / len * (k / coreWidthM);
        double cellConductance = cell.Cells.Sum(c => c.HeightMm / len / CellPathResistance(c, coreWidthM, k));
        return (widthM - coreWidthM) / k + 1.0 / (webConductance + cellConductance);
    }

    // One cell's through-core resistance: the cell's own material over its own width, plus the solid concrete filling
    // the rest of the core layer behind it.
    static double CellPathResistance(VoidCell cell, double coreWidthM, double k) {
        double cellWidthM = cell.WidthMm / 1000.0;
        return (cell.Grouted ? cellWidthM / GroutConductivityWPerMK : CellAirResistanceM2KPerW)
            + Math.Max(0.0, coreWidthM - cellWidthM) / k;
    }

    // The Coring bucket on the MANUFACTURED basis. The class is a JOINT read of the design net fraction and the
    // lattice's own cell COUNT, because the vocabulary's rows name counts: a 12-in 3-cell unit and an 8-in 2-cell unit
    // sharing a net fraction must not share a token. The ASTM C90 §5.4.1 solid floor is the SHARED published number
    // masonry#MASONRY_FAMILY RuptureModulus.SolidNetFloor carries — one standard, one value, two readers — and the
    // remaining band edges are the C216/C652 clay classes this vocabulary also spans.
    const double CoredNetFloor = 0.60;   // ASTM C652 H40V perforated boundary; below it the C90 hollow classes

    // The bucket takes the LATTICE and its gross bed plane rather than an admitted profile, because the seed law
    // needs the void class BEFORE Component.Of and the lattice is already pure at that point — the same cells the
    // profile factory then admits, so the two readings cannot disagree.
    public static Coring CoringOf(double widthMm, double lengthMm, Seq<VoidCell> cells) {
        double gross = widthMm * lengthMm;
        Seq<VoidCell> design = DesignCells(cells);
        double solid = gross > 0.0 ? (gross - design.Sum(static c => c.WidthMm * c.HeightMm)) / gross : 1.0;
        return (solid, design.Count) switch {
            (>= RuptureModulus.SolidNetFloor, _) => Coring.None,
            (>= CoredNetFloor, _)                => Coring.Perforated10Cell,
            (_, >= 3)                            => Coring.Hollow3Cell,
            _                                    => Coring.Hollow2Cell,
        };
    }

    // The IFC profile subtype on the AS-BUILT basis: any UNGROUTED cell demands the per-void
    // IfcArbitraryProfileDefWithVoids, the single-void hollow subtype carrying no second cell; a solid or fully
    // grouted lattice is the outer rectangle. Derived from the ONE occupancy fact, never a column contradicting it.
    public static string IfcSubtypeOf(Seq<VoidCell> cells) =>
        cells.Exists(static c => !c.Grouted) ? "IfcArbitraryProfileDefWithVoids" : "IfcRectangleProfileDef";
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class CmuSeed {
    // The 3/8 in coordinating joint serves BOTH the bed joint (the Module coursing rise) and the head joint (the run
    // advance), and it is the ONE number every actual dimension on this page derives by.
    public const double CoordinatingJointMm = 9.5;
    // ASTM C90 §6.1 — the manufacturing envelope the coursing tolerance and the GLB tessellation read: 1/8 in on any
    // overall dimension, 1/16 in on a moulded feature's size and placement.
    public const double PermissibleDeviationMm = 3.2;
    public const double MouldedFeatureDeviationMm = 1.6;
    // ASTM C90 Table 1 — ONE web thickness minimum across every nominal width, and the normalized web area floor.
    public const double WebFloorMm = 19.0;
    public const double NormalizedWebAreaFloorMm2PerM2 = 45140.0;

    static readonly MaterialId ConcreteCmu = MaterialId.Of("concrete.cmu");   // substance and appearance coincide for a plain CMU

    // ASTM C90 Table 1 face-shell minima keyed by NOMINAL width, at the standard's own printed SI conversions and
    // read as a FLOOR each moulded shell proves against rather than a value a row is forced to.
    static readonly Seq<(double NominalWidthMm, double FloorMm)> FaceShellFloors = Seq(
        (102.0, 19.0),
        (152.0, 25.0),
        (double.PositiveInfinity, 32.0));

    public static double FaceShellFloorMm(double nominalWidthMm) =>
        FaceShellFloors.Filter(f => nominalWidthMm <= f.NominalWidthMm).Head.Map(static f => f.FloorMm).IfNone(32.0);

    // The roster over COORDINATING MODULES. Face shells take the Table 1 minimum for each nominal width except the
    // two solids, whose thicker shells are the moulded form of a unit with no cells at all; webs seed at the single
    // published minimum. PUBLIC: the Generation course fold and the host materialization read these by designation.
    public static readonly Seq<CmuRow> Roster = Seq(
        new CmuRow("cmu.4in-hollow",     CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   101.6, 203.2, 406.4, 19.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.6in-hollow",     CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   152.4, 203.2, 406.4, 25.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.8in-hollow",     CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.8in-hollow-lw",  CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedClayShaleOrSlate, 203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.10in-hollow",    CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   254.0, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.12in-hollow",    CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   304.8, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 3),
        new CmuRow("cmu.4in-solid",      CmuGrade.Solid,  MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   101.6, 203.2, 406.4, 46.1, WebFloorMm, WebFloorMm, 0),
        new CmuRow("cmu.8in-solid",      CmuGrade.Solid,  MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.CalcareousOrSiliceousGravel, 203.2, 203.2, 406.4, 96.9, WebFloorMm, WebFloorMm, 0),
        new CmuRow("cmu.8in-grouted",    CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, GroutedCells: 2),
        new CmuRow("cmu.8in-reinforced", CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, ReinforcedCells: 1, RebarBarMm: 15.9),
        new CmuRow("cmu.8in-bondbeam",   CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, GroutedCells: 2, ReinforcedCells: 1, RebarBarMm: 12.7) { Special = CmuSpecialUnit.BondBeam },
        new CmuRow("cmu.8in-openend",    CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, ReinforcedCells: 1, RebarBarMm: 15.9) { Special = CmuSpecialUnit.OpenEnd },
        new CmuRow("cmu.8in-splitface",  CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Medium,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2) { Finish = CmuFinish.SplitFace },
        new CmuRow("cmu.8in-halfhigh",   CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedSlagOrPumice,     203.2, 101.6, 406.4, 32.0, WebFloorMm, WebFloorMm, 2));

    // The TYPED axis join through the ONE railed SeedJoin, keyed by the SAME ComponentId the resolved Component
    // carries: a consumer makes ONE keyed lookup for the strength, density, aggregate, and demold axes and CmuPhysics
    // then computes the identical receipt anywhere. Admission runs inside the Lazy body, so a malformed or duplicated
    // designation lands typed rather than as a TypeInitializationException no composition root can attribute.
    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, CmuRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    // The seed POLICY value. The regional receipt is ASTM's own — the C90 coordinating joint is the ONE dimension
    // every actual dimension derives by, so it crosses as the standard's joint thickness rather than a second
    // constant — and both MaterialId slots take the same substance, the coincidence a plain CMU's own seed states.
    public static readonly SeedLaw<CmuRow> Law = SeedLaw<CmuRow>.Of(
        family: ComponentFamily.Cmu,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: static (r, key) => SectionProfile.CellularRectangle.Of(r.WMm, r.LMm, Lattice(r), key),
        substance: static _ => ConcreteCmu,
        source: static r => r.Source,
        standard: static r => new ComponentStandard(r.Strength.Authority.Region, CoordinatingJointMm, r.Strength.Authority),
        detail: Some<Func<CmuRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        voids: static r => CmuPhysics.CoringOf(r.WMm, r.LMm, Lattice(r)));

    // The row census, ACCUMULATING — seven INDEPENDENT proofs, so a malformed row names every column it broke in one
    // verdict instead of the first hiding the rest. Geometry admits after: degenerate, out-of-bounds, and overlapping
    // cells are CellularRectangle.Of's own rail.
    static Validation<Error, Unit> Coherence(CmuRow r, Op key) =>
        (guard(r.Strength.Family == ComponentFamily.Cmu,
             new ComponentFault.GradeFamilyMismatch(key, r.Strength, ComponentFamily.Cmu)).ToValidation(),
         guard(r.Strength.Columns is GradeProperties.Cmu,
             new ComponentFault.GradeBodyMissing(key, r.Strength, ComponentFamily.Cmu)).ToValidation(),
         guard((r.Grade == CmuGrade.Hollow) == (r.Cells > 0),
             new KernelFault.InvalidValue(nameof(r.Grade), "hollow exactly when the row declares cells", Some(key))).ToValidation(),
         guard(r.Cells >= 0
                 && r.GroutedCells >= 0 && r.GroutedCells <= r.Cells
                 && r.ReinforcedCells >= 0 && r.ReinforcedCells <= r.Cells
                 && double.IsFinite(r.RebarBarMm)
                 && (r.ReinforcedCells == 0 ? r.RebarBarMm == 0.0 : r.RebarBarMm > 0.0),
             new KernelFault.InvalidValue(nameof(r.ReinforcedCells), "cell, fill, and bar declarations that agree", Some(key))).ToValidation(),
         guard(r.FaceShellMm >= FaceShellFloorMm(r.WModuleMm)
                 && r.EndWebMm >= WebFloorMm && r.CrossWebMm >= WebFloorMm
                 && r.EffectiveFaceShellMm >= CmuFinish.SplitResidualFloorMm,
             new KernelFault.InvalidValue(nameof(r.FaceShellMm), "published face-shell and effective-shell floors", Some(key))).ToValidation(),
         guard(r.NormalizedWebAreaMm2PerM2.ForAll(static anw => anw >= NormalizedWebAreaFloorMm2PerM2),
             new KernelFault.InvalidValue(nameof(r.NormalizedWebAreaMm2PerM2), "the published normalized web-area floor", Some(key))).ToValidation(),
         guard(r.Density.Holds(r.Density.OvenDryKgPerM3)
                 && r.Density.OvenDryKgPerM3 >= CmuDensity.PopulationFloorKgPerM3
                 && r.Density.OvenDryKgPerM3 <= CmuDensity.PopulationCeilingKgPerM3,
             new KernelFault.InvalidValue(nameof(r.Density), "a density inside the published population band", Some(key))).ToValidation())
            .Apply(static (_, _, _, _, _, _, _) => unit).As();

    // The Realization bag: the ProfileSubtype token off the ADMITTED lattice — the same cells the profile carries,
    // never a second Lattice run — beside the row's own evidence grade.
    static Fin<PropertyBag> Detail(CmuRow r, SectionProfile profile, Op key) =>
        profile is SectionProfile.CellularRectangle lattice
            ? Fin.Succ(ComponentDetail.RealizationRows(
                ComponentDetail.Token(DetailSchema.ProfileSubtype, CmuPhysics.IfcSubtypeOf(lattice.Cells)),
                ComponentDetail.Sourced(r.Source)))
            : new ComponentFault.ProfileMismatch(key, ComponentFamily.Cmu, profile.GetType());

    // The ComponentFamily.Cmu CAPACITY producer: the railed Table restores the strength and fill axes, the solved
    // section is the AS-BUILT net the seeded lattice already encodes, and the placement carries the member height and
    // the mortar keys. The RuptureModulus row is DERIVED rather than taken from the caller — direction and bond ride
    // the placement's own row, the grout state the lattice's, the solid form the profile's — so a partially grouted
    // wall routes the TMS footnote interpolation through the ONE masonry-owned selector instead of snapping to a
    // bounding row. A reinforced unit routes the §9.3 couple, decided by its own ReinforcedCells and never a flag.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in SeedJoin.Resolve(Table, component.Designation, key)
        from solved in section.ToFin(new ComponentFault.SectionUnavailable(key, component.Designation))
        from lattice in component.Profile is SectionProfile.CellularRectangle cell
            ? Fin.Succ(cell)
            : Fin.Fail<SectionProfile.CellularRectangle>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Cmu, component.Profile.GetType()))
        from strength in row.Strength.CmuArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Strength, ComponentFamily.Cmu))
        from capacity in SectionCapacity.Lift(row.ReinforcedCells > 0
            ? new CapacityReceipt.ReinforcedMasonry(component.Designation, strength, solved, placement.HeightMm, placement.Basis, row, placement.BarGrade)
            : new CapacityReceipt.Masonry(
                component.Designation, strength, solved, placement.HeightMm, placement.Basis,
                RuptureModulus.For(component.Profile, placement.Rupture,
                    CmuPhysics.Of(lattice, row.Density, row.Aggregate, row.Special).GroutedCellFraction),
                placement.Flexural, placement.System, placement.Mortar),
            key)
        select capacity;

    // The seam-lowering door, the masonry#MASONRY_FAMILY MasonryDetail.Properties twin over the concrete lattice: the
    // receipt every unit already computes reaches the seam, so a CMU material carries its thermal, acoustic, and fire
    // physics beside its capacity. The unit U-value inverts the isothermal-planes resistance and the assembly
    // EN ISO 6946 fold at Rasm.Compute supersedes it; the AS-BUILT lattice is the basis, so a grouted or bond-beam
    // unit lowers its own filled physics.
    const double ConcreteSpecificHeatJKgK = 1000.0;   // ASTM C90 normal-weight concrete masonry
    const double ConcreteVapourMu = 6.0;              // EN ISO 13788 concrete-masonry water-vapour resistance factor

    public static Fin<Seq<MaterialPropertySet>> Properties(CmuRow row, SectionProfile.CellularRectangle cell, Op key) =>
        from physics in Fin.Succ(CmuPhysics.Of(cell, row.Density, row.Aggregate, row.Special))
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: row.Density.ConductivityWPerMK,
            specificHeat: ConcreteSpecificHeatJKgK,
            uValue: 1.0 / physics.ThermalResistanceM2KPerW,
            vapourResistanceFactor: ConcreteVapourMu, key)
        from spectrum in WallAcoustics.Of(physics.ArealMassKgPerM2, key)
        // ACI 216.1 equivalent thickness measures INSULATION alone — R and E ride absence, never a copied figure —
        // and a unit reaching NO tabulated period lowers no Fire set at all, absence being the honest state where a
        // zero-minute certificate would have been a rating no table issued.
        from fire in physics.FireRating
            .Map(period => FireResistance.I(period.Key, key).Map(static r => Seq(MaterialPropertySet.OfFire(FireRating.A1, r))))
            .IfNone(Fin.Succ(Seq<MaterialPropertySet>()))
        select Seq(thermal, MaterialPropertySet.OfAcoustic(spectrum)) + fire;

    // The coursing module the Generation course row reads: the actual height already derives from the module by
    // subtracting the joint, so re-adding it lands back exactly — the point of carrying the module at all.
    public static Fin<ComponentUnit> Module(CmuRow row, Op key) =>
        ComponentUnit.Of(row.WMm, row.HMm, row.LMm, row.HMm + CoordinatingJointMm, key);

    // The ASTM lattice, PURE: min-corner cells inset by the EFFECTIVE face shell across the WIDTH, bounded by the end
    // webs and separated by the surviving cross webs along the LENGTH. A knocked-down cross web merges the cells into
    // the continuous trough a channel or bond-beam unit is; an open-end unit drops its end webs so the end cells run
    // out; a control-joint or sash unit adds its end-face groove as a further cell. A degenerate span yields a cell
    // the CellularRectangle.Of containment gate faults — the boundary owns that rail.
    static Seq<VoidCell> Lattice(CmuRow r) {
        if (r.Cells <= 0) { return Seq<VoidCell>(); }
        double faceShell = r.EffectiveFaceShellMm;
        double endWeb = r.Special.EndWebsPresent ? r.EndWebMm : 0.0;
        int cells = r.Special.SeparatedCells(r.Cells);
        double crossWeb = r.Special.CrossWebMm(r.CrossWebMm);
        double slotMm = r.Special.ControlSlotFraction * faceShell;
        double usable = r.LMm - slotMm;
        double cellLen = (usable - (2.0 * endWeb + (cells - 1) * crossWeb)) / cells;
        Seq<VoidCell> cores = toSeq(Enumerable.Range(0, cells)).Map(i => new VoidCell(
            XMm: faceShell, YMm: endWeb + i * (cellLen + crossWeb),
            WidthMm: r.WMm - 2.0 * faceShell, HeightMm: cellLen,
            // A REINFORCED cell is necessarily grouted — a bar is set in grout, never in a void — so the grouted
            // count is the maximum of the two columns and a row declaring more reinforced cells than grouted ones
            // still lays a physically real lattice rather than a bar floating in air. A merged trough carries the
            // fill of the cells it merged.
            Grouted: i < Math.Max(r.GroutedCells, r.ReinforcedCells) || cells < r.Cells && r.GroutedCells > 0,
            Reinforced: i < r.ReinforcedCells));
        // The end-face groove: a full-width slot at the far end of the length axis, cut to the molding row's share of
        // the face shell. A standard unit's share is zero and the slot is absent by construction.
        return slotMm > 0.0
            ? cores.Add(new VoidCell(XMm: 0.0, YMm: r.LMm - slotMm, WidthMm: r.WMm, HeightMm: slotMm))
            : cores;
    }
}
```

## [03]-[RESEARCH]

(none)
