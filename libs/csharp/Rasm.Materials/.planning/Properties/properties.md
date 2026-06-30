# [MATERIALS_PROPERTIES]

THE TYPED-ENGINEERING-PROPERTY SOURCE. The typed engineering-property family is SEAM-owned: the `Rasm.Element` `MaterialPropertySet` `[Union]` (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`, keyed to the one `Discipline`) is the canonical material-physics carrier the `Material` node holds, the seam `MeasureValue` is the SI-coerced measure each dimensional column carries ([H2]), and the intrinsic single-material acoustic folds (`Nrc`/`Saa`/`StcWeighted`/`Rw` over the shared `RatingContour.Fit` contour kernel) live on the seam `Acoustic` case (`Rasm.Element` `Composition/acoustic`) — so the migration source's Materials-owned `MaterialProperty` `[Union]`, its `MaterialPropertyKind` coercion enum, and its acoustic projection folds are RETIRED, ROUTED into the seam. This owner is now the Materials SOURCE: one `MaterialPropertyCatalogue` is the registered-row database of known-material engineering data (the published mechanical/thermal/acoustic/fire values per `MaterialId`, grounded in EN 1993 / EN 1992 / EN 338 / EN 14080 / EN ISO 10456 and the published acoustic/fire datasheets) the `Admit` lowering coerces into the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire` cases, composing the seam `MaterialPropertySet.OfMechanical`/`OfThermal`/`OfAcoustic`/`OfFire` smart-constructors with raw doubles (the seam owns the `UnitsNet` SI coercion and the union), and `Lookup` is the projector-facing resolution the `Projection/component#COMPONENT_PROJECTOR` calls. A material's engineering properties are NEVER a per-discipline material type: a wall material carries its conductivity, sound-transmission spectrum, fire rating, and structural grade as one `Seq<MaterialPropertySet>` over one `MaterialId`, a full engineering object the projector lowers into the seam `Material` node, never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface; the multi-ply `AssemblyAggregator` (the series-resistance U-value, layered-STC, rule-of-mixtures, and lifecycle GWP/cost folds) is RELOCATED to `Rasm.Compute` (the seam carries the per-material INPUT, never the assembly aggregation). The page composes the seam (`MaterialPropertySet`/`MeasureValue`/`PropertyValue`/`PropertyName`/`Discipline`/`FireRating`/`SmokeClass`/`DropletClass`/`FireResistance`/`Acoustic`/`AcousticBand`/`MaterialId`), the `Properties/sustainability#SUSTAINABILITY_PROPERTY` `Environmental`/`Cost`/`Classification` lowering for the lifecycle disciplines, and the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` band-2500 rail (the SAME band every seam smart-constructor and `FireRating.Parse` rail, so this SOURCE catalogue never mints the appearance `MaterialFault` 2450 of another concern across its admission); it re-mints NO seam type, names NO `UnitsNet` unit enum (the seam owns the registry — `OfMechanical`/`OfThermal` take RAW doubles), and reads the seam acoustic folds it never re-authors.

## [01]-[INDEX]

- [01]-[MATERIAL_PROPERTY_CATALOGUE]: the `MaterialPropertyRow` published-data ingress record, the `MaterialPropertyCatalogue` registered-row database (the structural-materials roster across steel/concrete/timber/aluminium/masonry/glass/insulation/gypsum/membrane), the `Admit` row→seam-case lowering composing the seam smart-constructors with raw doubles, and the `Lookup` resolution the projector calls.
- [02]-[ASSESSMENT_INPUT]: why Materials authors NO assessment-input node — the material's `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the input `Rasm.Compute` reads off the graph directly, the migration `MaterialAssessmentInput` marshaller retired and the case→`Discipline` map the seam's own accessor.

## [02]-[MATERIAL_PROPERTY_CATALOGUE]

- Owner: `MaterialPropertyRow` the published-data ingress record; `MaterialPropertyCatalogue` the registered-row database; `Admit` the row→seam-case lowering; `Lookup` the projector-facing resolution.
- Cases: one `MaterialPropertyRow` shape across all materials — the mechanical (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`UltimateStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`), thermal (`ConductivityWMK`/`SpecificHeatJKgK`/`UValueWM2K`/`VapourResistanceFactorMu`), optional acoustic (the seventeen-band absorption + sound-reduction vectors), and optional fire (the EN 13501-1 reaction `(Reaction, Smoke, Droplets)` triple + the EN 13501-2 R/E/I minutes) published columns; the `Admit` lowering produces a `Seq<MaterialPropertySet>` of the seam `Mechanical`/`Thermal`/`Acoustic`/`Fire` cases, the lifecycle `Environmental`/`Cost`/`Classification` cases lowering from `Properties/sustainability#SUSTAINABILITY_PROPERTY`, each case a `MaterialPropertySet` over a `MaterialId`, never a property subtype.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key)` — the published-row lowering passing RAW doubles to the seam `MaterialPropertySet.OfMechanical`/`OfThermal` (the seam owns the `UnitsNet` SI coercion [H2] and guards the dimensionless ratios), the seventeen-band acoustic vectors through the seam `Acoustic.Of` band gate (the intrinsic folds ride the seam case), and the fire `(Reaction, Smoke, Droplets, minutes)` columns through the seam `FireRating.Parse`/`SmokeClass`/`DropletClass`/`FireResistance.Rei`, `Fin<T>` aborting on a non-finite or out-of-range column (the seam admission's fault lifts unchanged); `MaterialPropertyCatalogue.Lookup(MaterialId id, Op key)` resolves a registered material to its lowered seam-case set the projector reads, faulting the seam `ElementFault.ValueRejected` for an unregistered material (engineering properties are REQUIRED for a known structural material — unlike the OPTIONAL sustainability data `SustainabilityCatalogue.Lookup` returns empty for) — one polymorphic resolution, never a `GetMechanical`/`GetThermal` family.
- Packages: Rasm.Element (project — the `MaterialPropertySet` cases, `MeasureValue`, `FireRating`/`SmokeClass`/`DropletClass`/`FireResistance`, `Acoustic`/`AcousticBand`, `Discipline`, `MaterialId`, the SI coercion + acoustic folds), Rasm (project — `Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Seq`/`Fin`/`Option` + `Map`/`Match`), BCL inbox (`FrozenDictionary`/`ReadOnlyMemory<double>`).
- Growth: a new engineering property shared across materials is one column on the matching seam `MaterialPropertySet` case (a seam growth) the `MaterialPropertyRow` gains a published column for and `Admit` lowers; a new known material is one `MaterialPropertyCatalogue.Rows` entry — a `MaterialId` key and a `MaterialPropertyRow` value (the roster grows by row to thousands without a seam touch); a new property discipline with no fit is one seam `MaterialPropertySet` case carrying its mapping (the lifecycle `Environmental`/`Cost`/`Classification` discipline lands exactly this way on `Properties/sustainability#SUSTAINABILITY_PROPERTY`) — never a parallel Materials union, never a per-discipline material type. The catalogue grows by row, the property vocabulary by seam case.
- Boundary: `MaterialPropertyRow` is the published-DATA ingress (the raw EN-standard/datasheet values), NOT a parallel domain union — the seam `MaterialPropertySet` is the one typed carrier, `Admit` the `BOUNDARY_ADMISSION` that lowers the raw row into the seam cases once; the dimensional columns coerce to SI THROUGH the seam smart-constructors (the `UltimateStrengthMpa` column the ACI/EN concrete + EN 1993 net-section checks read and the `VapourResistanceFactorMu` column the EN ISO 13788 Glaser condensation route reads pass RAW to `OfMechanical`/`OfThermal`, which call `MeasureValue.Of(value, UnitsNet.Units.X, key)` internally — the seam's `Of(double value, Enum unit, Op key)` registry coercion, NOT a Materials `QuantityType`-leading overload that does not exist), the `PoissonsRatio` guarded by the seam `OfMechanical` to the isotropic `[0,0.5]` (a thermodynamically-impossible ratio unrepresentable) and the vapour factor guarded `>= 1`, the acoustic vectors validated once through the seam `Acoustic.Of` band-arity gate, the fire reaction parsed to the seam `FireRating` `[SmartEnum<string>]` (a non-standard class is a row never a free string) and the published minutes lifted to a typed EN 13501-2 R/E/I `FireResistance` never a bare scalar; the seam `Acoustic` case carries the `Nrc`/`Saa`/`StcWeighted` intrinsic folds over the shared `RatingContour.Fit` contour kernel — this owner READS them (never re-authors), so a Materials consumer reading a single-number STC reads the seam contour-fit, never a second algorithm; the catalogue NEVER stores a coercion enum or a unit type — the only unit boundary is the seam smart-constructor, so this folder admits `UnitsNet` solely THROUGH the seam ([H2]) and never reaches DOWN to the app-platform `Rasm.Compute` units owner; the lowered `Seq<MaterialPropertySet>` is what the `Projection/component#COMPONENT_PROJECTOR` writes onto the seam `Material` node, and the per-material set feeds the `[02]-[ASSESSMENT_INPUT]` discipline marshalling — the property catalogue crosses to `Rasm.Compute`/`Rasm.Bim` only through the seam graph, never a Materials wire carrier.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op
using Rasm.Element;                  // MaterialId, Discipline, MaterialPropertySet (Mechanical|Thermal|Acoustic|Fire|Environmental|Cost),
                                     // FireRating, SmokeClass, DropletClass, FireResistance, Acoustic, MeasureValue, PropertyValue, PropertyName,
                                     // ElementFault (the seam value-admission band 2500 — this SOURCE catalogue rails the SAME band the seam
                                     // smart-constructors and FireRating.Parse rail, never the appearance MaterialFault 2450 of another concern)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // the property-catalogue folder owner — MaterialPropertyCatalogue lives here, the Projection/component projector imports it as Rasm.Materials.Properties

// --- [MODELS] ------------------------------------------------------------------------------
// The published engineering data for one material — the ingress row Admit lowers into the seam MaterialPropertySet
// cases. A flat DATA record (the raw EN-standard/datasheet values), NOT a parallel domain union: the seam owns the
// union. The acoustic OPTION carries the two seventeen-band ReadOnlyMemory<double> vectors (the AcousticBand resolution
// the seam Acoustic.Of gates); the fire OPTION carries the EN 13501-1 reaction triple (Reaction/Smoke/Droplets tokens)
// PLUS the EN 13501-2 resistance minutes — a reaction-class-only datasheet supplies "" smoke/droplet tokens.
public sealed record MaterialPropertyRow(
    double DensityKgM3,
    double YoungsModulusMpa,
    double YieldStrengthMpa,
    double UltimateStrengthMpa,
    double PoissonsRatio,
    double ThermalExpansionPerK,
    double ConductivityWMK,
    double SpecificHeatJKgK,
    double UValueWM2K,
    double VapourResistanceFactorMu,
    Option<(ReadOnlyMemory<double> Absorption, ReadOnlyMemory<double> Sri)> Acoustic,
    Option<(string Reaction, string Smoke, string Droplets, int LoadBearingMin, int IntegrityMin, int InsulationMin)> Fire);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialPropertyCatalogue {
    // Lowers a published row into the seam MaterialPropertySet cases. The seam OfMechanical/OfThermal coerce every
    // dimensional column to SI through MeasureValue.Of(value, UnitsNet.Units.X, key) INTERNALLY ([H2] — the seam owns the
    // UnitsNet registry), guard Poisson to the isotropic [0,0.5] and the vapour factor >= 1 inline, and own the case
    // shapes; the row passes RAW doubles, so this folder admits UnitsNet ONLY through the seam and names no unit enum and
    // no QuantityType token. The acoustic vectors flow through the seam Acoustic.Of band gate (the intrinsic
    // Nrc/Saa/StcWeighted folds ride the seam Acoustic case); the fire triple parses to the seam FireRating/SmokeClass/
    // DropletClass and the published minutes lift to a typed EN 13501-2 R/E/I FireResistance. EVERY value flows through a
    // seam admission (the OfMechanical/OfThermal/OfAcoustic constructors, FireRating.Parse, the Sub TryGet gate) so the
    // WHOLE chain rails ONE band — the seam ElementFault.ValueRejected (2500) — never the appearance MaterialFault 2450 of
    // another concern: a mixed-band Admit chain forks the telemetry the fault federation bands by code (an out-of-domain
    // smoke token reading "Parameter"/appearance rather than "Value"/seam-admission is the rejected cross-concern leak).
    public static Fin<Seq<MaterialPropertySet>> Admit(MaterialPropertyRow row, Op key) =>
        from mechanical in MaterialPropertySet.OfMechanical(row.DensityKgM3, row.YoungsModulusMpa, row.YieldStrengthMpa, row.UltimateStrengthMpa, row.PoissonsRatio, row.ThermalExpansionPerK, key)
        from thermal in MaterialPropertySet.OfThermal(row.ConductivityWMK, row.SpecificHeatJKgK, row.UValueWM2K, row.VapourResistanceFactorMu, key)
        from acoustic in row.Acoustic.Match(
            None: () => Fin.Succ(Seq<MaterialPropertySet>()),
            Some: a => Acoustic.Of(a.Absorption, a.Sri, key).Map(static spectrum => Seq1(MaterialPropertySet.OfAcoustic(spectrum))))
        from fire in row.Fire.Match(
            None: () => Fin.Succ(Seq<MaterialPropertySet>()),
            Some: f => from reaction in FireRating.Parse(f.Reaction, key)                 // the seam's Parse over the generated TryGet — the ONE fire-reaction admission
                       from smoke in Sub(SmokeClass.TryGet, f.Smoke, key, "smoke")        // SmokeClass/DropletClass expose NO Parse; resolve through the generated TryGet
                       from droplets in Sub(DropletClass.TryGet, f.Droplets, key, "droplet")
                       select Seq1(MaterialPropertySet.OfFire(reaction, smoke, droplets, new FireResistance(f.LoadBearingMin, f.IntegrityMin, f.InsulationMin))))
        select Seq(mechanical, thermal) + acoustic + fire;

    // The EN 13501-1 sub-class admission: SmokeClass/DropletClass are seam [SmartEnum<string>] with NO Parse wrapper
    // (only FireRating/Discipline/MeasurementBasis carry one), so an empty token defaults the seam's NotSpecified row and a
    // present token resolves through the Thinktecture-generated TryGet — railing the seam ElementFault.ValueRejected on an
    // out-of-domain class, the SAME band (and "Value" telemetry Category) FireRating.Parse rails for the sibling reaction
    // token, so the whole fire admission carries one band. The TryGetter delegate is the generated
    // `static bool TryGet(string?, out T?)`, so the one helper serves both sub-classes.
    delegate bool TryGetter<T>(string? token, out T? value);
    static Fin<T> Sub<T>(TryGetter<T> tryGet, string token, Op key, string label) where T : class =>
        tryGet(token, out T? value) && value is { } row
            ? Fin.Succ(row)
            : ElementFault.ValueRejected(key, $"<fire-{label}-class-unknown:{token}>");

    // The structural-materials roster — every row a published EN-standard datasheet keyed by the canonical MaterialId
    // (seam-generated ordinal-ignore-case equality keys the table). The roster grows by ROW to thousands of grades; the
    // seam vocabulary never changes. Mechanical: EN 1993 steel grades (S235/S355/S460 fy/fu, E=210 GPa, ν=0.3, α=12e-6;
    // plus the metal.steel generic-grade alias the connection family keys when a grade is unspecified, an S235 baseline),
    // EN 1992 concrete classes (C25/C30/C40 fck plenum, Ecm, ν=0.2, α=10e-6), EN 338/EN 14080 timber (C24 solid, GL24h
    // glulam — fmk as the yield surrogate, E0,mean, ν~0.4, α~5e-6), EN 1999 aluminium 6061-T6, masonry (clay/AAC), float
    // glass, mineral-wool/EPS/XPS/PIR insulation, gypsum board, EPDM/PVC membrane. Thermal: EN ISO 10456 design λ + the
    // EN ISO 13788 vapour-resistance factor μ. Acoustic: the seventeen-band absorption + field-incidence SRI vectors a
    // datasheet declares (only the hard-surfaced / acoustically-characterized rows carry them; a structural steel section
    // carries None). Fire: EN 13501-1 reaction (A1 noncombustible steel/concrete/stone/mineral-wool through F) + the
    // EN 13501-2 R/E/I minutes where a bare material is classified (most carry 0 — resistance is an assembly property).
    public static readonly FrozenDictionary<MaterialId, MaterialPropertyRow> Rows = new (MaterialId Id, MaterialPropertyRow Row)[] {
        // --- structural steel (EN 1993-1-1; λ EN ISO 10456; A1 noncombustible)
        (MaterialId.Of("steel.s235"), new(7850.0, 210_000.0, 235.0, 360.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // metal.steel — the generic-structural-steel grade alias the Component/component#COMPONENT_OWNER Component.CapacityKey
        // resolves (a galvanized fastener/connector whose grade is unspecified): the conservative EN 1993-1-1 S235 baseline (fy 235, fu 360),
        // so the connection-design seam reads a real Mechanical row by metal.steel rather than faulting; a graded connector keys steel.s355 directly.
        (MaterialId.Of("metal.steel"), new(7850.0, 210_000.0, 235.0, 360.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.s355"), new(7850.0, 210_000.0, 355.0, 490.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.s460"), new(7850.0, 210_000.0, 460.0, 540.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- reinforcing/stainless (EN 1993-1-4; A1)
        (MaterialId.Of("steel.b500"),     new(7850.0, 200_000.0, 500.0, 540.0, 0.30, 12.0e-6, 50.0, 460.0, 5.88, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("steel.1.4301"),   new(8000.0, 200_000.0, 230.0, 540.0, 0.30, 16.0e-6, 15.0, 500.0, 1.76, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- concrete (EN 1992-1-1; fck as the "yield" surrogate, fcm ~ fck+8 as ultimate; A1; insulation U via λ/0.2 m)
        (MaterialId.Of("concrete.c25"), new(2400.0, 31_000.0, 25.0, 33.0, 0.20, 10.0e-6, 2.3, 1000.0, 11.5, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c30"), new(2400.0, 33_000.0, 30.0, 38.0, 0.20, 10.0e-6, 2.3, 1000.0, 11.5, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("concrete.c40"), new(2450.0, 35_000.0, 40.0, 48.0, 0.20, 10.0e-6, 2.3, 1000.0, 11.5, 80.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        // --- structural timber (EN 338 solid C24, EN 14080 glulam GL24h; fmk as the bending-strength surrogate; D reaction)
        (MaterialId.Of("timber.c24"),    new(420.0, 11_000.0, 24.0, 40.0, 0.40, 5.0e-6, 0.13, 1600.0, 0.43, 50.0,
            Some((Absorb(0.10, 0.11, 0.10, 0.08, 0.06, 0.06, 0.07, 0.07, 0.08, 0.08, 0.09, 0.09, 0.09, 0.10, 0.10, 0.10, 0.11),
                  Sri(14, 16, 18, 20, 22, 24, 26, 27, 29, 31, 33, 34, 36, 38, 40, 41, 42)),
                 Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("timber.gl24h"),  new(385.0, 11_500.0, 24.0, 40.0, 0.40, 5.0e-6, 0.12, 1600.0, 0.40, 50.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("D", "s2", "d0", 30, 30, 30)))),
        (MaterialId.Of("wood.oak"),      new(700.0, 11_000.0, 40.0, 90.0, 0.35, 5.0e-6, 0.17, 2400.0, 0.49, 50.0,
            Some((Absorb(0.05, 0.06, 0.07, 0.08, 0.10, 0.10, 0.11, 0.10, 0.10, 0.10, 0.10, 0.10, 0.09, 0.09, 0.09, 0.09, 0.09),
                  Sri(18, 20, 22, 24, 26, 29, 31, 33, 35, 37, 38, 39, 40, 40, 39, 35, 33)),
                 Some(("D", "s2", "d0", 0, 0, 0)))),
        // --- aluminium (EN 1999-1-1 6061-T6; α=23e-6; B reaction)
        (MaterialId.Of("aluminium.6061t6"), new(2700.0, 70_000.0, 240.0, 290.0, 0.33, 23.0e-6, 160.0, 900.0, 18.8, 1e9,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("B", "s1", "d0", 0, 0, 0)))),
        // --- masonry (EN 1996; clay brick + autoclaved aerated; A1)
        (MaterialId.Of("masonry.clay"),  new(1800.0, 7_000.0, 10.0, 20.0, 0.25, 6.0e-6, 0.77, 1000.0, 3.50, 16.0,
            Some((Absorb(0.02, 0.02, 0.03, 0.03, 0.03, 0.04, 0.04, 0.05, 0.05, 0.05, 0.05, 0.06, 0.06, 0.06, 0.07, 0.07, 0.07),
                  Sri(30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 53, 54, 55, 56, 57)),
                 Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("masonry.aac"),   new(500.0, 2_000.0, 4.0, 5.0, 0.20, 8.0e-6, 0.13, 1000.0, 0.43, 6.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("stone.marble"),  new(2700.0, 70_000.0, 15.0, 100.0, 0.25, 7.0e-6, 2.8, 880.0, 3.50, 10_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("A1", "", "", 0, 120, 120)))),
        // --- glazing (float glass; EN 572; A1)
        (MaterialId.Of("glass.float"),   new(2500.0, 70_000.0, 45.0, 50.0, 0.22, 9.0e-6, 1.00, 720.0, 5.88, 1e9,
            Some((Absorb(0.18, 0.10, 0.07, 0.05, 0.04, 0.03, 0.03, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02),
                  Sri(25, 27, 29, 30, 31, 32, 33, 34, 33, 32, 30, 29, 31, 34, 37, 39, 40)),
                 Some(("A1", "", "", 0, 0, 0)))),
        // --- insulation (EN ISO 10456 design λ; mineral wool A1, EPS/XPS/PIR E reaction; absorptive mineral wool)
        (MaterialId.Of("insulation.mineralwool"), new(40.0, 1.0, 0.001, 0.002, 0.0, 0.0, 0.035, 1030.0, 0.13, 1.0,
            Some((Absorb(0.15, 0.25, 0.40, 0.55, 0.70, 0.80, 0.90, 0.95, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                  Sri(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 13, 13, 14, 14, 15)),
                 Some(("A1", "", "", 0, 0, 0)))),
        (MaterialId.Of("insulation.eps"),  new(20.0, 5.0, 0.05, 0.10, 0.10, 60.0e-6, 0.036, 1450.0, 0.13, 60.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.xps"),  new(33.0, 15.0, 0.20, 0.45, 0.10, 70.0e-6, 0.034, 1450.0, 0.12, 150.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        (MaterialId.Of("insulation.pir"),  new(32.0, 10.0, 0.10, 0.20, 0.10, 70.0e-6, 0.022, 1400.0, 0.08, 60.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
        // --- gypsum board (EN 520; A2; absorptive interior lining)
        (MaterialId.Of("gypsum.board"),  new(700.0, 2_500.0, 2.0, 4.0, 0.25, 18.0e-6, 0.25, 1000.0, 20.0, 10.0,
            Some((Absorb(0.29, 0.20, 0.12, 0.10, 0.08, 0.06, 0.06, 0.05, 0.04, 0.04, 0.04, 0.04, 0.05, 0.05, 0.06, 0.06, 0.07),
                  Sri(15, 16, 17, 18, 20, 22, 24, 26, 28, 30, 31, 32, 33, 34, 35, 36, 37)),
                 Some(("A2", "s1", "d0", 0, 0, 0)))),
        // --- membrane (EPDM roofing; E reaction)
        (MaterialId.Of("membrane.epdm"), new(1150.0, 5.0, 5.0, 9.0, 0.45, 160.0e-6, 0.25, 1000.0, 12.5, 50_000.0,
            Option<(ReadOnlyMemory<double>, ReadOnlyMemory<double>)>.None, Some(("E", "s2", "d0", 0, 0, 0)))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    // The projector-facing resolution: a registered material lowers to its full engineering set; an UNREGISTERED material
    // rails the seam ElementFault.ValueRejected (band 2500) — the SAME band Admit's columns rail, so a Lookup and the
    // admission it delegates to never split bands. Engineering properties are REQUIRED for a known structural material the
    // Profiles#capacity / Rasm.Compute design-code routes read, the asymmetric dual of the OPTIONAL Properties/
    // sustainability#SUSTAINABILITY_PROPERTY Lookup (lifecycle data is declared-or-absent, so it returns Fin.Succ(empty)).
    // An app authoring a material with bespoke properties supplies them at the wire and does not route this catalogue.
    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialPropertyRow? row)
            ? Admit(row!, key)
            : ElementFault.ValueRejected(key, $"<unregistered-material-properties:{id.Value}>");

    // The seventeen-band literal-vector helpers — the AcousticBand resolution (100..4000 Hz) the seam Acoustic.Of gates;
    // a row that omits the acoustic Option carries None. params ReadOnlySpan<double> collapses the seventeen positional
    // bands to one boundary; ToArray().AsMemory() materializes the ReadOnlyMemory<double> the row + seam carry.
    static ReadOnlyMemory<double> Absorb(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
    static ReadOnlyMemory<double> Sri(params ReadOnlySpan<double> bands) => bands.ToArray().AsMemory();
}
// The case→Discipline map is the seam's own MaterialPropertySet.Discipline accessor (one owner); a consumer reads
// set.Discipline directly — Rasm.Compute selects its analysis route by it, this folder mints no parallel map.
```

## [03]-[ASSESSMENT_INPUT]

- Owner: NONE — the Materials folder authors NO assessment-input marshaller and NO `Assessment` node; the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected seam `Material` node IS the analysis input.
- Cases: zero — there is no input shape to model; `Rasm.Compute` reads the typed `MaterialPropertySet` cases off the graph and dispatches on `set.Discipline`, so a per-discipline input bag is the deleted form.
- Entry: `Rasm.Compute` reads the `Material` node plies DIRECTLY above the seam (`id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the relocated multi-ply `AssemblyAggregator` + the ISO/EN closed-form routes + the VividOrange/FE structural solvers), and writes the seam `Assessment` `Result` node back content-keyed on `(input key, route)`; the case→`Discipline` map is the seam's own `MaterialPropertySet.Discipline` accessor (`Mechanical`→`Structural`, `Thermal`→`Thermal`, `Acoustic`→`Acoustic`, `Fire`→`Fire`, `Environmental`→`Environmental`, `Cost`→`Cost`; `Energy` rides an `Assessment` node only), so Compute selects its route by `set.Discipline` with no parallel Materials marshaller.
- Boundary: the migration `MaterialAssessmentInput` marshaller is RETIRED — a Materials-authored typed-input bag is redundant with Compute reading the typed `MaterialPropertySet` cases off the graph, so the seam carries ONE property surface (the `Material` node), not a parallel input node; the seam `Acoustic` case's intrinsic `Nrc`/`StcWeighted`/`SoundReductionIndexDb` folds (`Composition/acoustic`) are the single-material ratings Compute's ISO 12354 layered fold reads through the SAME `RatingContour.Fit` contour kernel, so the assembly STC and the material STC share one contour owner; the multi-ply aggregation is `Rasm.Compute`'s, this folder retaining only the single-material property SOURCE and crossing to Compute solely through the seam graph.

## [04]-[RESEARCH]

- [SEAM_OWNS_PROPERTY_UNION]: the typed engineering-property family is the seam `MaterialPropertySet` `[Union]` (`Mechanical`/`Thermal`/`Acoustic`/`Fire`/`Environmental`/`Cost`, keyed to the seam `Discipline`), so the migration Materials `MaterialProperty` `[Union]`, its `MaterialPropertyKind` coercion enum, and the inline contour-fit/`Nrc`/`Saa` projection folds are RETIRED and ROUTED into the seam (the contour fit now the seam's shared `RatingContour.Fit` kernel). This owner keeps only the Materials SOURCE: the `MaterialPropertyCatalogue` published-row database and the `Admit` lowering. SEAM CONTRACT (Rasm.Element side; this folder consumes): the `MaterialPropertySet` smart-constructors `OfMechanical(density, youngs, yield, ultimate, poisson, expansion, key)` / `OfThermal(conductivity, specificHeat, uValue, vapour, key)` / `OfAcoustic(Acoustic)` / `OfFire(FireRating, SmokeClass, DropletClass, FireResistance)` (the cases carry `MeasureValue` columns; the `Acoustic` case forwards `AbsorptionSpectrum`/`SoundReductionIndexDb` over `AcousticBand` + the intrinsic `Nrc`/`Saa`/`StcWeighted`/`Rw` folds over the shared `RatingContour.Fit` contour kernel), `Acoustic.Of(absorption, sri, key)`, `FireRating.Parse` (the seam's `Parse` over the generated `TryGet`), the generated `SmokeClass.TryGet`/`DropletClass.TryGet` (the seam gives these sub-classes NO `Parse` wrapper, so this folder resolves them through the Thinktecture-generated `TryGet`), the `FireResistance(loadBearing, integrity, insulation)` ctor, and `MaterialPropertySet.Discipline`. The seam smart-constructors do the SI coercion (`MeasureValue.Of(value, UnitsNet.Units.X, key)` over the registry), so this folder passes RAW doubles and names no unit enum, no `QuantityType` token, and no `MeasureValue.Of` overload.
- [CATALOGUE_DOMAIN_COVERAGE]: the `MaterialPropertyCatalogue` is the SOURCE of known-material engineering data — the structural-materials domain, not a three-row toy: EN 1993 steel (`S235`/`S355`/`S460` carbon, `B500` rebar, `1.4301` austenitic stainless, and the `metal.steel` generic-grade alias — an S235 baseline — the `Component/component#COMPONENT_OWNER` `Component.CapacityKey` resolves when a connector's grade is unspecified, so the connection-design seam reads a real `Mechanical` row rather than faulting), EN 1992 concrete (`C25`/`C30`/`C40`, `fck` the yield surrogate and `fcm ≈ fck+8` the ultimate), EN 338/EN 14080 timber (`C24` solid, `GL24h` glulam, `wood.oak` hardwood — `fmk` the bending-strength surrogate), EN 1999 aluminium (`6061-T6`), EN 1996 masonry (`clay` brick, `aac` autoclaved aerated, `marble` dimension stone), EN 572 float glass, EN ISO 10456 insulation (`mineralwool`/`eps`/`xps`/`pir` — design λ + the EN ISO 13788 vapour factor μ), EN 520 gypsum board, and EPDM membrane. Each row's mechanical columns are the published characteristic values, the thermal columns the design λ + μ, the acoustic Option the seventeen-band absorption + field-incidence SRI vectors a datasheet declares (only the acoustically-characterized rows carry them — a structural steel section carries `None`), and the fire Option the EN 13501-1 reaction triple + EN 13501-2 R/E/I minutes (`A1` for steel/concrete/stone/mineral-wool through `F`; most bare materials carry 0 minutes — resistance is an assembly property a `Rasm.Compute` route computes over the buildup). The roster grows by ROW to thousands of grades with NO seam touch — the catalogue is the open data surface, the seam the closed vocabulary.
- [SEAM_MEASURE_COERCION]: the engineering-property SI coercion rides the seam `MeasureValue` ([H2] — a `Dimension` 7-SI-exponent value-object discriminator plus a `QuantityType` `[ValueObject<string>]` name plus `MeasureValue(QuantityType, Dimension, double Si, string CanonicalUnit)`, `UnitsNet` SI-coerced once at admission through `Quantity.TryFrom` → `ToUnit(UnitSystem.SI)`), so the migration in-folder `MaterialPropertyKind.CanonicalUnit`/`MaterialUnits.Coerce` is RETIRED — `Admit` calls the seam `OfMechanical`/`OfThermal` smart-constructors with RAW doubles and the seam coerces each column via `MeasureValue.Of(value, UnitsNet.Units.X, key)` (the `Of(double value, Enum unit, Op key)` overload — there is NO `MeasureValue.Of(QuantityType, value, unit, key)` form), owning the `Pset_*` measure family (`ThermalTransmittance`/`Pressure`/`MassDensity`/…) the migration 6-value `QuantityKind` could not admit. UnitsNet v5 removed the `QuantityType` enum, so the `QuantityType` `[ValueObject<string>]` (the `QuantityInfo.Name`) is the discriminator and the `Dimension` 7-vector the physical signature, and this folder names neither token. The photometric/appearance luminous coercion stays in-folder on `Appearance/photometric#PHOTOMETRIC` (an appearance concern not lowered to the seam); only the engineering-property coercion moved.
- [SEAM_OWNS_ACOUSTIC_FOLDS]: the intrinsic single-material acoustic pure folds — `Nrc` (the four octave-coincident one-third-octave bands 250/500/1k/2k arithmetic mean rounded to 0.05, ASTM C423), `Saa` (the twelve one-third-octave bands 200-2500 mean rounded to 0.01), `StcWeighted` (the ASTM E413 contour fit over the SRI bands) and `Rw` (the ISO 717-1 fit), and the shared `RatingContour.Fit` contour kernel both the rating and the `Rasm.Compute` layered fold invoke — live on the seam `Acoustic` case (`Rasm.Element` `Composition/acoustic`), so this owner READS them (a Materials consumer reads `a.Nrc`/`a.StcWeighted` off the case) and never re-authors them. The seam `Acoustic.Of` band-arity gate (the seventeen one-third-octave `AcousticBand` centres, absorption in `[0,1]`, SRI finite) is the one admission; the inline `OutOfUnit`/`NonFinite`/contour-fit kernels the migration page carried are the seam's.
- [ASSEMBLY_AGGREGATOR_RELOCATED]: the multi-ply `AssemblyAggregator` (the series-resistance U-value `1/U = Rsi + Σ(t_i/λ_i) + Rse` ISO 6946 fold, the mass-law layered-STC ISO 12354 fold over the accumulated areal mass, the rule-of-mixtures effective density/conductivity fold, the worst-ply fire envelope, and the lifecycle GWP/cost folds from `Properties/sustainability`) is RELOCATED to `Rasm.Compute` — the seam carries the per-material `MaterialPropertySet` cases on the `Material` node, Compute reads them directly, runs the assembly aggregation over the element's `MaterialComposition` plies, and writes the `Assessment` `Result` node back content-keyed on `(input key, route)`. The migration `AssemblyProperty`/`ConstituentWeight`/`AggregateLayers`/`AggregateConstituents` owners and the surface-film `Rsi`/`Rse` constants leave this folder entirely. Ripple counterpart: `Rasm.Compute` `Analysis/aggregator` (the multi-ply `AssemblyAggregator` folds reading the seam `MaterialComposition` + the per-ply `MaterialPropertySet`) + `Analysis/assessment` (the discipline solvers writing `Assessment` `Result` nodes).
- [IFC_MATERIAL_PROPERTIES]: the IFC 4.3 `IfcMaterialProperties` extends `IfcExtendedProperties` with a `Material` reference and a `Properties` set; the standard `Pset_MaterialMechanical`/`Pset_MaterialThermal`/`Pset_MaterialCommon` carry the canonical column names (`MassDensity`/`YoungModulus`/`PoissonRatio`/`ThermalConductivity`/`SpecificHeatCapacity`). The seam `MaterialPropertySet` cases map one-to-one onto the Psets and the `PropertyName` tokens use the canonical Pset member names, so `Rasm.Bim` reads the projected seam `Material` node's property set and emits `IfcMaterialProperties` from the seam graph — no Materials→IFC carrier, the Pset member-name mapping the `Rasm.Bim` side, the property computation this side, the seam the alignment.
