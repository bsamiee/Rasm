# [MATERIALS_CMU]

THE CMU SEED PAGE GROUNDED IN ASTM C90-16a + TMS 602-16/-22 + ACI 216.1 + NCMA TEK 6-2B/7-1D. A concrete block is one `ComponentRow` minted by the ONE generator `CmuSeed.Rows : Context -> Fin<Seq<ComponentRow>>` — `ComponentFamily.Cmu` (`ComponentClass.Minor`, `DetailLane.Realization`, `admits: CellularRectangle`, `crossNominal: GrossRectangleMm.WidthMm`, `rows: CmuSeed.Rows`), never a `ConcreteBlock` type and never a bespoke payload record. The bed-plane geometry is the parent `SectionProfile.CellularRectangle` (`WidthMm` the through-wall thickness — the family cross nominal, `DepthMm` the along-wall unit length, `Seq<VoidCell>` the per-cell fill-state lattice the private `Lattice` generator lays from the ASTM face-shell and web columns); the vocabulary is the six FORM-law SmartEnums (`CmuGrade`/`CmuStrength`/`CmuDensity`/`CmuAggregate`/`CmuSpecialUnit`/`CmuFinish`); the realization columns ride the typed `CmuRow` table with per-column provenance. `NetSection`/`GroutedSection` are ONE `SectionSolver.Solve` `CellularRectangle` arm, whose bed-plane solve IS the TMS 402 net cross-section (net area under axial compression, both-axis moduli for out-of-plane flexure): the `VoidCell.Grouted` flag selects the result through one code path — the seeded lattice yields the AS-BUILT net (only ungrouted cells void, a fully-grouted unit the solid rectangle), and the DESIGN net is the same solve over `CmuPhysics.DesignCells`, the manufactured-basis projection the `Coring` bucket reads — so this page calls no solver and owns no perimeter builder.

DIMENSIONS ARE DERIVED, NOT AUTHORED BESIDE THEIR MODULE. ASTM C90 is an INCH-POUND standard whose SI values are stated as mathematical conversions for information only, and it publishes no dimension table at all — it publishes tolerances about a *specified* dimension. So a `CmuRow` carries the COORDINATING MODULE (the nominal 4/6/8/10/12 in width, the 8 in course, the 16 in run) and every actual dimension is that module less the one ASTM `3/8 in` coordinating joint, which makes the module arithmetic exact by construction rather than a second authored series a reader must check against the first. `CmuSeed.Module` then re-adds the joint and lands back on the nominal, and the C90 §6.1 permissible deviation rides as the manufacturing tolerance band beside it.

`CmuPhysics` owns fire rating, thermal resistance, self-weight, equivalent thickness, solid fraction, and grout fraction over `(SectionProfile.CellularRectangle, CmuDensity, CmuAggregate, CmuSpecialUnit)` as bag-free derivations, and the family's `DetailLane.Realization` bag carries the seed-computed `DetailSchema.ProfileSubtype` IFC profile-def token off `CmuPhysics.IfcSubtypeOf` beside its row's own `Provenance` — the wire datum the `Rasm.Bim` egress profile lane reads; the physics axes never land as bag rows. `CmuSeed.Table` is the `SeedJoin`-built `ComponentId`-keyed typed join from an M7-resolved component to its strength, density, aggregate, finish, and molding axes. `CmuStrength` carries the TMS 602 `f'm` and optional Type-N unit-strength columns, and `masonry#MASONRY_FAMILY` `RuptureModulus.For` supplies the mortar-keyed flexural-tension row the capacity producer derives.

## [01]-[INDEX]

- [02]-[CMU_FAMILY]: the six SmartEnums (`CmuGrade` the ASTM C90 §5.3/§5.4 hollow-solid form, `CmuStrength` TMS 602-16/-22 Table 2 with the `MortarType` inversion, `CmuDensity` the C90 Table 2 class with its NCMA-derived conductivity, `CmuAggregate` the four ACI 216.1 fire categories with the blended-aggregate rule, `CmuSpecialUnit` molding geometry, `CmuFinish` architectural surface), the `CmuRow` provenance-columned seed table over coordinating modules, the `CmuPhysics` fire/thermal/mass receipt with the geometry-derived `Coring` bucket and the `IfcSubtypeOf` wire token, and the `CmuSeed` generator (`Rows` fold seeding the `DetailSchema.Realization` bag + the `Properties` seam lowering + `Module` coursing projection + `Capacity` the basis-threaded producer + the `SeedJoin` axis join + the private `Lattice`).

## [02]-[CMU_FAMILY]

- Owner: `CmuSeed` the ONE generator; `CmuPhysics` the ONE physical receipt; the six FORM-law SmartEnums (runtime key lookup + derivation columns, so they STAY per `SEED_ROW_LAW` tier 3); `CmuRow` the AUTHORED standards table (no admitted producer exists); `CmuSeed.Table` the railed `ComponentId`-keyed axis join an axis consumer pairs with the M7-resolved `Component.Profile` (the ONE legal axis path — the `DetailLane.Realization` bag carries the wire and provenance rows, never an axis column).
- Cases: grade {hollow, solid} (ASTM C90 §5.3/§5.4, the form alone — the solid net-area floor has its own owner); strength {f2000..f3000} (TMS 602-16/-22 Table 2, `f'm` + the two mortar columns, PUBLISHED); density {lightweight <1680, medium 1680–2000, normal ≥2000 kg/m³, each carrying its C90 absorption caps and its NCMA-band-derived conductivity}; aggregate {the four ACI 216.1 / IBC `722.3.2` categories, each its equivalent-thickness band}; special {standard, bond-beam, knockout, channel, lintel, sash, control-joint, open-end}; finish {precision, split-face, scored, ribbed}. A unit is one `CmuRow`; a new grade/strength/density/aggregate/special/finish is one case/row — never a per-block type.
- Entry: `CmuSeed.Rows(Context) : Fin<Seq<ComponentRow>>` traverses the typed table under a per-row `Op` identity and proves grade/form, fill/reinforcement correspondence, the C90 Table 1 face-shell floor, and the split-face residual floor before constructing the lattice. Invalid counts, bar presence, or a face shell under its published minimum fault `ComponentFault.Dimension`; degenerate, out-of-bounds, or overlapping cells fault inside `CellularRectangle.Of`; every failure aborts the catalogue. `ComponentRow.Sectioned` reads the constructed profile's own topology, so the M7 map carries the admitted as-built section by derivation rather than by assertion.
- Packages: Rasm.Numerics (`PositiveMagnitude` via the parent profile factories), Rasm.Domain (`Context`/`Op`), Rasm.Element (`MaterialId`, the seam `DetailSchema`/`PropertyCategory` currencies), Thinktecture.Runtime.Extensions (`[SmartEnum]`, generated `TryGet`/`Items`/`Switch`; `libs/csharp/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Traverse`/`Fin`), the parent `component#COMPONENT_OWNER`/`#SECTION_PROFILE` owners and `masonry#MASONRY_FAMILY` for the shared `RuptureModulus`, `WallAcoustics`, and `FireBand`. The cmu generative data is AUTHORED in-fence; ONLY the section integral crosses to VividOrange, through the parent solver (`.api/api-vividorange-sections-sectionproperties.md`).
- Growth: a new ASTM unit (metric A-series, half-high, architectural) is one `CmuRow`; a grouting/reinforcing variant is row columns (`GroutedCells`/`ReinforcedCells`/`RebarBarMm`); molding and finish variants are `CmuSpecialUnit`/`CmuFinish` rows the host extrudes and the lattice reads — never a parallel section owner, never a solver edit.
- Boundary: column provenance per `SEED_ROW_LAW`. PUBLISHED: the C90 Table 1 face-shell minima and the single web minimum, the C90 Table 1 normalized-web-area floor, the C90 Table 2 density bands, absorption caps, and net-area compressive floors, the C90 §5.4.1 solid net-area floor, the C90 §6.1 permissible deviation, the TMS 602 Table 2 strengths, the ACI 216.1 equivalent thicknesses and blended-aggregate rule, the NCMA TEK 6-2B density-resistivity bands, and the ASTM C476 grout density. DEFINED: every actual dimension, which is its coordinating module less the ASTM joint by the standard's own coordination. AUTHORED: the per-class representative oven-dry density inside each published band, and the molding fractions. C90 publishes ONE web thickness minimum rather than a per-width end/cross split, so both web columns seed at that minimum while the SPLIT itself stays real geometry (an open-end unit drops its end webs so the end cells run to the unit ends) — and the normalized web area, whose C140 measurement convention this estate does not possess, rides a DECLARED `Option` column gated against the published floor only where a row declares one, never a computed claim. The wire spelling is the AS-BUILT occupancy derivation `CmuPhysics.IfcSubtypeOf(cell)` (`IfcArbitraryProfileDefWithVoids` iff any UNGROUTED cell remains — the single-void `IfcRectangleHollowProfileDef` cannot carry two distinct cells — `IfcRectangleProfileDef` for a solid or fully-grouted lattice), the derived token seeded as the `DetailSchema.ProfileSubtype` realization-bag row — Bim references no AEC peer, so the wire spelling is carried row data, never a cross-package call; the manufacturing grade never contradicts the grouted state; the element stamp is the `ComponentFamily.Cmu.Ifc` concrete leaf, whose object-type discriminator keeps it distinct from the clay sibling's leaf under the reverse type-candidate read. `DraftDegrees`/`FaceShellFlareMm` are captured generative columns the host materialization reads off the seed table — `VoidCell` carries fill-state only.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                     // FrozenDictionary — the SeedJoin-built CmuRow axis join
using LanguageExt;                                   // Fin, Option, Seq, Traverse
using Rasm.Domain;                                   // Context, Op
using Rasm.Element.Composition;                      // MaterialId (the substance/appearance rows the seed assigns), MaterialPropertySet + its OfThermal/OfAcoustic/OfFire admissions
using Rasm.Element.Properties;                       // DetailSchema, FireRating, FireResistance — the EN 13501 pair the Properties lowering mints
using Thinktecture;                                  // [SmartEnum], KeyMemberEqualityComparer, ComparerAccessors, TryGet/Items/Switch
using static LanguageExt.Prelude;

// The cmu seed declares in the ONE Rasm.Materials.Component namespace; MortarType, RuptureModulus, WallAcoustics,
// FireBand, and every parent owner resolve by bare name, and the ComponentFamily.Cmu policy row folds CmuSeed.Rows.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The unit FORM, ASTM C90 §5.3 hollow against §5.4 solid — a MANUFACTURING classification, deliberately independent
// of fill state. C90 §5.4.1 fixes a solid unit's net cross-sectional area at 75% of gross in every plane parallel to
// the bearing surface, and that floor has ONE owner — masonry#MASONRY_FAMILY RuptureModulus.SolidNetFloor, which the
// Coring bucket compares the design net against — so this row names the form and never restates the number.
// LOAD-BEARING IS NOT A GRADE COLUMN: C90 §1.1 states its units suit both loadbearing and nonloadbearing
// applications, so the application is the governing SPECIFICATION (C90 against C129) that `ComponentStandard`
// already carries, and a fifth and sixth row crossing form with application would have split one axis in two.
// The IFC profile subtype is likewise not a grade column: it follows the AS-BUILT cell occupancy through
// CmuPhysics.IfcSubtypeOf, so a fully-grouted hollow unit never emits a void-bearing subtype.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuGrade {
    public static readonly CmuGrade Hollow = new("hollow", hollow: true);
    public static readonly CmuGrade Solid  = new("solid",  hollow: false);
    public bool Hollow { get; }
}

// The TMS 602-16/-22 Table 2 specified-masonry-strength class: FmMpa IS the specified f'm (the assemblage strength the
// design seam reads, NOT the unit strength — the capacity#SECTION_CAPACITY lift pairs it with the M7-cached as-built
// section and the RuptureModulus-resolved MasonryUnreinforced.FlexuralTensionMpa column); the two PUBLISHED net-area
// UNIT-strength columns key the mortar band (Type M/S the lower, Type N the higher; the empty Type-N cells for
// f2750/f3000 are None).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuStrength {
    public static readonly CmuStrength F2000 = new("f2000", fmMpa: 13.79, netUnitMsMpa: 13.79, netUnitNMpa: Some(18.27));
    public static readonly CmuStrength F2250 = new("f2250", fmMpa: 15.51, netUnitMsMpa: 17.93, netUnitNMpa: Some(23.44));
    public static readonly CmuStrength F2500 = new("f2500", fmMpa: 17.24, netUnitMsMpa: 22.41, netUnitNMpa: Some(28.96));
    public static readonly CmuStrength F2750 = new("f2750", fmMpa: 18.96, netUnitMsMpa: 26.89, netUnitNMpa: Option<double>.None);
    public static readonly CmuStrength F3000 = new("f3000", fmMpa: 20.69, netUnitMsMpa: 31.03, netUnitNMpa: Option<double>.None);
    public double FmMpa { get; }              // the specified masonry compressive strength f'm (MPa)
    public double NetUnitMsMpa { get; }       // required net-area unit strength with Type M or S mortar (MPa), PUBLISHED
    public Option<double> NetUnitNMpa { get; }

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

    // The unit-strength method, inverted: the HIGHEST f'm class the supplied net-area unit strength qualifies for
    // under the mortar band — the descending-by-f'm scan's first passing row; a below-floor unit rails None.
    public static Option<CmuStrength> Resolve(double netUnitStrengthMpa, MortarType mortar) =>
        !double.IsFinite(netUnitStrengthMpa) || netUnitStrengthMpa <= 0.0
            ? Option<CmuStrength>.None
            : toSeq(Items.OrderByDescending(static c => c.FmMpa))
                .Filter(c => c.RequiredUnitMpa(mortar).Exists(required => netUnitStrengthMpa >= required))
                .Head;
}

// The ASTM C90 Table 2 density classification. The class BOUNDS are the standard's own band edges and the row carries
// them as DATA, so the band a measured density falls in is a read of the published table rather than a literal
// repeated at each comparison; the representative oven-dry density inside each band is AUTHORED, because C90 bands
// densities and does not name a representative for a band.
// CONDUCTIVITY IS DERIVED, never stored: NCMA TEK 6-2B publishes concrete thermal RESISTANCE PER INCH as a BAND
// against oven-dry density, so a single stored conductivity per class could only ever agree with that band or
// contradict it. ConductivityWPerMK interpolates the published band at the row's own density and takes the band
// midpoint — the estate's stated reading of a published range, with the range itself carried so a consumer wanting
// the conservative or the optimistic end reads the same table this derivation does.
// The absorption caps and the net-area compressive floors are the C90 Table 2 columns in full — the AVERAGE-of-three
// and the INDIVIDUAL-unit limits are separate published requirements and a submittal lane checks both.
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

// The ACI 216.1 / IBC 722.3.2 fire aggregate — the FOUR published categories and no more. The standard groups
// calcareous with siliceous gravel and expanded slag with pumice, so two rows apiece would have been one published
// category wearing two names and a consumer choosing between them would have chosen nothing.
// Each row carries the equivalent thickness required at each RATED PERIOD, not merely at one hour: the rating is a
// table read at the tabulated periods, and only the periods this estate holds are carried — the 1-hour column, and
// the 3-hour column for the two categories whose 3-hour figures the source publishes in full. An absent period is
// Option, so a read at that period reports not-applicable rather than interpolating a certificate.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuAggregate {
    public static readonly CmuAggregate CalcareousOrSiliceousGravel = new("calcareous-or-siliceous-gravel", eqThick1HrMm: 71.1, eqThick3HrMm: Some(135.0));
    public static readonly CmuAggregate LimestoneCindersOrSlag      = new("limestone-cinders-or-slag",      eqThick1HrMm: 68.6, eqThick3HrMm: Option<double>.None);   // limestone / cinders / air-cooled slag
    public static readonly CmuAggregate ExpandedClayShaleOrSlate    = new("expanded-clay-shale-or-slate",   eqThick1HrMm: 66.0, eqThick3HrMm: Some(112.0));
    public static readonly CmuAggregate ExpandedSlagOrPumice        = new("expanded-slag-or-pumice",        eqThick1HrMm: 53.3, eqThick3HrMm: Option<double>.None);   // most fire-efficient category
    public double EqThick1HrMm { get; }
    public Option<double> EqThick3HrMm { get; }

    // The BLENDED-AGGREGATE rule: a unit made from more than one aggregate requires the volume-weighted sum of the
    // per-aggregate required thicknesses at the target period, Tr = Σ(Ti · Vi). A published operation rather than a
    // note, because a blended unit is an ordinary product and the alternative is every caller re-deriving the sum.
    // MODAL_ARITY: one entrypoint over the mix, the single-aggregate call being the one-element case of it.
    public static Fin<double> BlendedThicknessMm(double ratedHours, Op key, params ReadOnlySpan<(CmuAggregate Aggregate, double Fraction)> mix) {
        Seq<(CmuAggregate Aggregate, double Fraction)> blend = toSeq([.. mix]);
        return blend.IsEmpty || Math.Abs(blend.Sum(static m => m.Fraction) - 1.0) > VolumeClosureTolerance
            ? ComponentFault.Dimension(key, $"<aggregate-blend-not-unit-volume:{blend.Sum(static m => m.Fraction):R}>")
            : blend
                .Traverse(m => m.Aggregate.RequiredThicknessMm(ratedHours)
                    .ToFin(ComponentFault.Capacity(key, $"<aggregate-period-unpossessed:{m.Aggregate.Key}:{ratedHours:R}>"))
                    .Map(required => required * m.Fraction)).As()
                .Map(static weighted => weighted.Sum());
    }

    const double VolumeClosureTolerance = 1e-9;

    public Option<double> RequiredThicknessMm(double ratedHours) =>
        Math.Abs(ratedHours - 1.0) < VolumeClosureTolerance ? Some(EqThick1HrMm)
        : Math.Abs(ratedHours - 3.0) < VolumeClosureTolerance ? EqThick3HrMm
        : Option<double>.None;
}

// The NCMA special-unit molding geometry, every column of which the LATTICE reads. CrossWebFraction is the surviving
// height of the cross webs as a share of the moulded web (a bond beam's are knocked down, a channel's removed
// outright), and at zero the cells MERGE into one continuous trough — which is what a channel or lintel unit is, so
// the trough is derived from the column rather than declared as a second shape. TroughDepthFraction is the depth of
// that trough as a share of unit height and is what keeps the merged cell from voiding the whole unit: the trough
// removes VOLUME over its own depth, so equivalent thickness and self-weight read it while the bed-plane section
// reads the merged footprint. ControlSlotFraction is the depth of the end-face groove as a share of the face shell —
// a sash unit's jamb rebate at half, a control-joint unit's full-depth shear key at one.
// EndWebsPresent stays its own column because dropping the end webs is a topology change (an open-end A/H-block's end
// cells run to the unit ends) rather than a proportion.
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

// The ASTM C90 §7 architectural surface finish — face texture the host relief-models and the Bim surface-style egress
// reads. SplitDepthMm is STRUCTURAL as well as visual and the lattice reads it: splitting removes material from the
// face shell, so the effective shell is the moulded shell less the split depth, and C90 Table 1 footnote B floors
// that residual at 19,1 mm however deep the split. The score and rib columns are surface relief the host extrudes and
// change no section.
// A BURNISHED (ground or honed) unit is NOT a row here: grinding polishes the moulded face without changing any
// dimension this page carries, so it differs from a precision unit on the APPEARANCE axis alone — which the component
// already carries as its own MaterialId — and a second row identical in every column to Precision would have been one
// finish wearing two names with nothing to choose between them.
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
// The AUTHORED seed row: TYPED axis singletons + coordinating MODULES — an unknown grade/strength/density/aggregate
// key is UNREPRESENTABLE at compile time, so the fold carries no per-axis TryGet lift. The three module columns are
// the unit's NOMINAL coordinating dimensions and every actual dimension DERIVES from them by subtracting the one
// ASTM joint, so the module arithmetic cannot drift from the dimensions it coordinates. FaceShellMm is the MOULDED
// shell the manufacturer declares (at or above the C90 Table 1 minimum for the unit's nominal width); EndWebMm and
// CrossWebMm seed at the single C90 web minimum, the split between them being real topology rather than two published
// numbers. NormalizedWebAreaMm2PerM2 is the manufacturer's DECLARED value where one exists — C90 sets the floor but
// its cross-sectional-area measurement runs to C140, which this estate does not possess, so an undeclared row carries
// absence and the published floor gates only what is declared. DraftDegrees/FaceShellFlareMm are the captured demold
// geometry the host materialization reads. Special/Finish are init columns defaulting to the Standard/Precision
// singletons (a SmartEnum row is no compile-time constant, so they cannot ride the positional default slots).
public readonly record struct CmuRow(
    string Designation, CmuGrade Grade, CmuStrength Strength, CmuDensity Density, CmuAggregate Aggregate,
    double WModuleMm, double HModuleMm, double LModuleMm,
    double FaceShellMm, double EndWebMm, double CrossWebMm, int Cells,
    int GroutedCells = 0, int ReinforcedCells = 0, double RebarBarMm = 0.0,
    double FaceShellFlareMm = 0.0, double DraftDegrees = 1.5) {
    public CmuSpecialUnit Special { get; init; } = CmuSpecialUnit.Standard;
    public CmuFinish Finish { get; init; } = CmuFinish.Precision;
    public Option<double> NormalizedWebAreaMm2PerM2 { get; init; } = Option<double>.None;
    // The dimensional columns are DEFINED — each actual dimension is its published coordinating module less the one
    // published joint, by ASTM C90's own coordination rather than by a second authored series.
    public Provenance Source { get; init; } = Provenance.Defined;

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
    double FireRatingHours,              // the ACI 216.1 / IBC 722.3.2 power law floored onto the published rating periods
    double SelfWeightKnPerM2,            // oven-dry net solid + grouted-cell grout, per wall-face m²
    double ThermalResistanceM2KPerW,     // NCMA TEK 6-2B isothermal planes, material-only (films are the assembly's)
    double SolidFraction,                // design net (every cell open) / gross — the manufactured basis
    double GroutedSolidFraction,         // as-built net / gross
    double GroutedCellFraction) {        // grout-filled share of the cell void, lattice-honest

    public const double GroutDensityKgPerM3 = 2243.0;        // ASTM C476 grout — 140 pcf
    public const double GroutConductivityWPerMK = 1.40;      // a grouted cell conducts like normal-weight concrete
    public const double CellAirResistanceM2KPerW = 0.18;     // ASHRAE vertical air-cavity — the ungrouted-cell path
    const double GravityMPerS2 = 9.80665;

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
            FireRatingHours: FireBand.Floor(Math.Pow(te / aggregate.EqThick1HrMm, 1.7)),
            SelfWeightKnPerM2: ((gross - allVoid * depthShare) * density.OvenDryKgPerM3 + (allVoid - openVoid) * depthShare * GroutDensityKgPerM3) * GravityMPerS2 / (len * 1e6),
            ThermalResistanceM2KPerW: IsothermalPlanes(cell, density),
            SolidFraction: gross > 0.0 ? Math.Clamp(net / gross, 0.0, 1.0) : 1.0,
            GroutedSolidFraction: gross > 0.0 ? Math.Clamp((gross - openVoid) / gross, 0.0, 1.0) : 1.0,
            GroutedCellFraction: allVoid > 0.0 ? (allVoid - openVoid) / allVoid : 0.0);
    }

    // The areal mass the masonry#MASONRY_FAMILY WallAcoustics single-leaf mass law reads — the same fold serves both
    // coursing families, so a CMU party wall's banded spectrum is one composition: WallAcoustics.Of(ArealMassKgPerM2, key).
    public double ArealMassKgPerM2 => SelfWeightKnPerM2 * 1000.0 / GravityMPerS2;

    // The DESIGN lattice — every cell open, the MANUFACTURED basis on which a unit is classified and sold. It is a
    // projection of the as-built lattice rather than a second stored geometry, so the two can never disagree, and the
    // Coring bucket below is its one consumer: a grouted unit is classified by the block that was manufactured, not
    // by the wall it ended up in.
    public static Seq<VoidCell> DesignCells(SectionProfile.CellularRectangle cell) =>
        cell.Cells.Map(static c => c with { Grouted = false, Reinforced = false });

    // Two face shells in SERIES with the core; the core a PARALLEL combination of the solid-web path and the per-cell
    // paths. Each cell path is resisted over ITS OWN through-wall width with the REMAINING core depth conducting as
    // solid concrete beside it — a narrow cell inside a wide core layer is a short cavity in series with body, which
    // is what the isothermal-planes method actually models; scaling the cavity's area resistance by a width ratio had
    // over-credited exactly the shaped and bond-beam units that mix a wide core with narrow end cells, and inverted
    // the sense besides (a narrower cell came out LESS resistant than a wide one). The core LAYER thickness is still
    // the widest cell — that is the depth the series split consumes — and a cell-free solid is one homogeneous layer.
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

    public static Coring CoringOf(SectionProfile.CellularRectangle cell) {
        double gross = cell.WidthMm.Value * cell.DepthMm.Value;
        Seq<VoidCell> design = DesignCells(cell);
        double solid = gross > 0.0 ? (gross - design.Sum(static c => c.WidthMm * c.HeightMm)) / gross : 1.0;
        return (solid, design.Count) switch {
            (>= RuptureModulus.SolidNetFloor, _) => Coring.None,
            (>= CoredNetFloor, _)                => Coring.Perforated10Cell,
            (_, >= 3)                            => Coring.Hollow3Cell,
            _                                    => Coring.Hollow2Cell,
        };
    }

    // The IFC profile subtype on the AS-BUILT basis: any UNGROUTED cell demands the explicit per-void
    // IfcArbitraryProfileDefWithVoids (the single-void IfcRectangleHollowProfileDef cannot carry two distinct
    // cells); a solid or fully-grouted lattice is the outer IfcRectangleProfileDef. Derived from the ONE occupancy
    // fact the lattice carries and seeded as the DetailSchema.ProfileSubtype realization-bag row the Bim egress
    // profile lane reads — never a grade column that contradicts the grouted state.
    public static string IfcSubtypeOf(SectionProfile.CellularRectangle cell) =>
        cell.Cells.Exists(static c => !c.Grouted) ? "IfcArbitraryProfileDefWithVoids" : "IfcRectangleProfileDef";
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class CmuSeed {
    // ASTM C90 dimensional authority (region "us"). The 3/8 in coordinating joint serves BOTH the bed joint (the
    // Module coursing rise) and the head joint (the run advance), and it is the ONE number every actual dimension on
    // this page is derived by — the inch-pound value C90 states as standard, its SI form carried at conversion
    // precision because that is what the geometry is built in.
    public const double CoordinatingJointMm = 9.5;
    // ASTM C90 §6.1 — no overall dimension of a standard unit differs from its specified dimension by more than
    // 1/8 in; a moulded feature holds to 1/16 in of both its dimension and its placement. The manufacturing envelope
    // the coursing tolerance and the GLB tessellation read, the CmuRow parity of the masonry SizeTolerance envelope.
    public const double PermissibleDeviationMm = 3.2;
    public const double MouldedFeatureDeviationMm = 1.6;
    // ASTM C90 Table 1 — ONE web thickness minimum across every nominal width, and the normalized web area floor.
    public const double WebFloorMm = 19.0;
    public const double NormalizedWebAreaFloorMm2PerM2 = 45140.0;

    static readonly ComponentStandard AstmC90 = new("us", StandardJointThicknessMm: CoordinatingJointMm, Authority: ComponentAuthority.Astm);
    static readonly MaterialId ConcreteCmu = MaterialId.Of("concrete.cmu");   // substance and appearance coincide for a plain CMU

    // ASTM C90 Table 1 face-shell minima, keyed by NOMINAL width: 3 and 4 in take 3/4 in, 6 in takes 1 in, and 8 in
    // and greater take 1-1/4 in. Transcribed at the standard's own printed SI conversions, and read as a floor the
    // seed proves each moulded shell against rather than as a value a row is forced to.
    static readonly Seq<(double NominalWidthMm, double FloorMm)> FaceShellFloors = Seq(
        (102.0, 19.0),
        (152.0, 25.0),
        (double.PositiveInfinity, 32.0));

    public static double FaceShellFloorMm(double nominalWidthMm) =>
        FaceShellFloors.Filter(f => nominalWidthMm <= f.NominalWidthMm).Head.Map(static f => f.FloorMm).IfNone(32.0);

    // The roster, over COORDINATING MODULES: 4/6/8/10/12-in hollow, the 4/8-in solids, an 8-in lightweight
    // expanded-clay unit, an 8-in fully-grouted unit, an 8-in single-cell reinforced unit (#5 / 15.9 mm), an 8-in
    // bond-beam, an 8-in open-end reinforced unit, an 8-in split-face architectural unit, and the half-high. Face
    // shells are the C90 Table 1 minimum for each nominal width except the two solids, whose thicker shells are the
    // moulded form of a unit with no cells at all. Webs seed at the single published minimum. A new metric A-series or
    // finish is one further row.
    // PUBLIC: the Generation course fold (Module) and the host materialization read these columns by designation.
    public static readonly Seq<CmuRow> AstmRows = Seq(
        new CmuRow("cmu.4in-hollow",     CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   101.6, 203.2, 406.4, 19.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.6in-hollow",     CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   152.4, 203.2, 406.4, 25.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.8in-hollow",     CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.8in-hollow-lw",  CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedClayShaleOrSlate, 203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.10in-hollow",    CmuGrade.Hollow, CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   254.0, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.12in-hollow",    CmuGrade.Hollow, CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   304.8, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 3),
        new CmuRow("cmu.4in-solid",      CmuGrade.Solid,  CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   101.6, 203.2, 406.4, 46.1, WebFloorMm, WebFloorMm, 0),
        new CmuRow("cmu.8in-solid",      CmuGrade.Solid,  CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.CalcareousOrSiliceousGravel, 203.2, 203.2, 406.4, 96.9, WebFloorMm, WebFloorMm, 0),
        new CmuRow("cmu.8in-grouted",    CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, GroutedCells: 2),
        new CmuRow("cmu.8in-reinforced", CmuGrade.Hollow, CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, ReinforcedCells: 1, RebarBarMm: 15.9),
        new CmuRow("cmu.8in-bondbeam",   CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, GroutedCells: 2, ReinforcedCells: 1, RebarBarMm: 12.7) { Special = CmuSpecialUnit.BondBeam },
        new CmuRow("cmu.8in-openend",    CmuGrade.Hollow, CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, ReinforcedCells: 1, RebarBarMm: 15.9) { Special = CmuSpecialUnit.OpenEnd },
        new CmuRow("cmu.8in-splitface",  CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Medium,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2) { Finish = CmuFinish.SplitFace },
        new CmuRow("cmu.8in-halfhigh",   CmuGrade.Hollow, CmuStrength.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedSlagOrPumice,     203.2, 101.6, 406.4, 32.0, WebFloorMm, WebFloorMm, 2));

    // The TYPED axis join through the ONE railed component#COMPONENT_OWNER SeedJoin, keyed by the SAME ComponentId the
    // resolved Component carries: the consumer holds the M7 ResolvedComponent and makes ONE keyed lookup for the
    // Density/Aggregate/Strength axes and the demold columns, then CmuPhysics.Of computes the identical receipt
    // anywhere. Admission runs inside the Lazy body, so a malformed or duplicated designation lands typed on the same
    // ComponentFault rail Component.Of would have taken rather than as a TypeInitializationException no composition
    // root can attribute. Both key spaces mint from the identical designation column of the identical rows.
    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, CmuRow>>> Table =
        SeedJoin.Of(AstmRows, static r => r.Designation);

    // The ONE generator fold (RAIL law): Traverse, never Choose — a grade/lattice form mismatch, a face shell under
    // its published floor, an out-of-bounds or overlapping cell, or a Component.Of rejection ABORTS
    // ComponentCatalogue.Of loudly. Each row mints its OWN Op identity so a fault names the exact designation.
    public static Fin<Seq<ComponentRow>> Rows(Context context) => AstmRows.Traverse(RowOf).As();

    // The per-row arm. Four admissions run before any geometry is laid, each naming what it refused: the grade/form
    // correspondence, the fill/reinforcement correspondence, the C90 Table 1 face-shell floor together with the
    // footnote-B split residual, the C90 Table 1 normalized-web-area floor where the row declares one, and the C90
    // NOTE 5 density population the authored representative must lie inside. Every lattice profile is then solvable
    // and ComponentRow.Sectioned reads its topology, so the M7 map carries the admitted as-built section by
    // derivation. Every row seeds the Realization bag: the ProfileSubtype token off the admitted lattice beside the
    // row's own Provenance.
    static Fin<ComponentRow> RowOf(CmuRow r) {
        Op key = Op.Of(name: r.Designation);
        return from formed in guard(r.Grade.Hollow == (r.Cells > 0), ComponentFault.Family(key, $"<cmu-grade-form-mismatch:{r.Designation}>"))
               from occupied in guard(
                   r.Cells >= 0
                       && r.GroutedCells >= 0 && r.GroutedCells <= r.Cells
                       && r.ReinforcedCells >= 0 && r.ReinforcedCells <= r.Cells
                       && double.IsFinite(r.RebarBarMm)
                       && (r.ReinforcedCells == 0 ? r.RebarBarMm == 0.0 : r.RebarBarMm > 0.0),
                   ComponentFault.Dimension(key, $"<cmu-fill-reinforcement-mismatch:{r.Designation}>"))
               from shelled in guard(
                   r.FaceShellMm >= FaceShellFloorMm(r.WModuleMm)
                       && r.EndWebMm >= WebFloorMm && r.CrossWebMm >= WebFloorMm
                       && r.EffectiveFaceShellMm >= CmuFinish.SplitResidualFloorMm,
                   ComponentFault.Dimension(key, $"<cmu-shell-or-web-below-floor:{r.Designation}:shell={r.FaceShellMm:R}:effective={r.EffectiveFaceShellMm:R}>"))
               from webbed in guard(
                   r.NormalizedWebAreaMm2PerM2.Match(Some: static anw => anw >= NormalizedWebAreaFloorMm2PerM2, None: static () => true),
                   ComponentFault.Dimension(key, $"<cmu-normalized-web-area-below-floor:{r.Designation}>"))
               from massed in guard(
                   r.Density.Holds(r.Density.OvenDryKgPerM3)
                       && r.Density.OvenDryKgPerM3 >= CmuDensity.PopulationFloorKgPerM3
                       && r.Density.OvenDryKgPerM3 <= CmuDensity.PopulationCeilingKgPerM3,
                   ComponentFault.Dimension(key, $"<cmu-density-outside-published-band:{r.Density.Key}>"))
               from profile in SectionProfile.CellularRectangle.Of(r.WMm, r.LMm, Lattice(r), key)
               let cell = (SectionProfile.CellularRectangle)profile
               from item in Component.Of(
                   ComponentFamily.Cmu, r.Designation, profile,
                   ComponentFamily.Cmu.Ifc, CmuPhysics.CoringOf(cell),
                   AstmC90, substanceId: ConcreteCmu, appearanceId: ConcreteCmu,
                   detail: Some(ComponentDetail.RealizationRows(
                       ComponentDetail.Token(DetailSchema.ProfileSubtype, CmuPhysics.IfcSubtypeOf(cell)),
                       ComponentDetail.Sourced(r.Source))),
                   key)
               select new ComponentRow(item, r.Source);
    }

    // The ComponentFamily.Cmu CAPACITY producer: the railed Table restores the strength and fill axes, the solved
    // section is the AS-BUILT net the seeded lattice already encodes, and the placement carries the member height and
    // the mortar keys. The RuptureModulus row is DERIVED rather than taken from the caller — the span direction and
    // bond pattern ride the placement's own row, the grout state is the lattice's own GroutedCellFraction, and the
    // solid form is the profile's — so a partially grouted wall routes the TMS footnote interpolation through the ONE
    // masonry-owned selector instead of snapping to a bounding row a caller happened to pass. A reinforced unit routes
    // the §9.3 steel-couple case; the lattice's own ReinforcedCells decide it, never a caller flag.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in SeedJoin.Resolve(Table, component.Designation, key)
        from solved in section.ToFin(ComponentFault.Section(key, $"<cmu-section-unresolved:{component.Designation.Value}>"))
        from lattice in component.Profile is SectionProfile.CellularRectangle cell
            ? Fin.Succ(cell)
            : Fin.Fail<SectionProfile.CellularRectangle>(ComponentFault.Section(key, $"<cmu-profile-not-cellular:{component.Designation.Value}>"))
        select SectionCapacity.Lift(row.ReinforcedCells > 0
            ? new CapacityReceipt.ReinforcedMasonry(component.Designation, row.Strength, solved, placement.HeightMm, placement.Basis, row, placement.BarGrade)
            : new CapacityReceipt.Masonry(
                component.Designation, row.Strength, solved, placement.HeightMm, placement.Basis,
                RuptureModulus.For(component.Profile, placement.Rupture.SpanParallelToBed, placement.Rupture.StackBond,
                    CmuPhysics.Of(lattice, row.Density, row.Aggregate, row.Special).GroutedCellFraction),
                placement.Flexural, placement.System, placement.Mortar));

    // The seam-lowering door, the masonry#MASONRY_FAMILY MasonryDetail.Properties twin over the concrete lattice: the
    // CmuPhysics receipt every unit already computes lowers onto the seam so a CMU material carries its thermal,
    // acoustic, and fire physics beside its capacity. Thermal reads the density row's DERIVED conductivity, the C90
    // concrete specific heat, the isothermal-planes resistance inverted to the unit U-value (the assembly EN ISO 6946
    // fold at Rasm.Compute supersedes it), and the EN ISO 13788 vapour factor for concrete masonry; Acoustic the ONE
    // WallAcoustics single-leaf mass law over the receipt's areal mass; Fire the banded ACI 216.1 rating as EN 13501-2
    // minutes under the A1 reaction a concrete unit always carries. The GROUTED lattice is the as-built basis the
    // receipt already uses, so a grouted or bond-beam unit lowers its own filled physics, and
    // Projection/component#COMPONENT_PROJECTOR registers the lowering beside the glazing and masonry compositions.
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
        // ACI 216.1 equivalent thickness measures INSULATION alone — R and E ride absence, never a copied figure.
        from resistance in FireResistance.I(FireBand.Minutes(physics.FireRatingHours), key)
        select Seq(thermal, MaterialPropertySet.OfAcoustic(spectrum), MaterialPropertySet.OfFire(FireRating.A1, resistance));

    // The coursing module the Generation course row reads off the seed table (spec [05]). The actual height already
    // derives from the height module by subtracting the joint, so re-adding it lands back on the module exactly —
    // which is the point of carrying the module rather than a second dimensional series beside it.
    public static Fin<ComponentUnit> Module(CmuRow row, Op key) =>
        ComponentUnit.Of(row.WMm, row.HMm, row.LMm, row.HMm + CoordinatingJointMm, key);

    // The ASTM lattice, PURE: cells inset by the EFFECTIVE face shell across the WIDTH axis (a split face has already
    // lost its split depth), bounded by the end webs and separated by the surviving cross webs along the DEPTH
    // (length) axis — min-corner VoidCells in the profile corner frame, web span 2·EndWeb + (cells−1)·CrossWeb over
    // the cells the molding row still separates. A knocked-down or removed cross web merges the cells into the one
    // continuous trough a channel, lintel, or bond-beam unit actually is; an open-end unit drops its end webs so the
    // end cells run to the unit ends; a control-joint or sash unit adds its end-face groove as a further cell, cut to
    // the molding row's own share of the face shell. The first GroutedCells carry grout, the first ReinforcedCells the
    // bar. A degenerate span yields an out-of-bounds or non-positive cell the CellularRectangle.Of containment gate
    // faults — the boundary owns the rail.
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
