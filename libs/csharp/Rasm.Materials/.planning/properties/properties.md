# [MATERIALS_PROPERTIES]

THE TYPED MATERIAL-PROPERTY OWNER and THE ASSEMBLY-AGGREGATION ENGINE. One `MaterialProperty` `[Union]` closes the IFC 4.3 `IfcMaterialProperties` engineering-property family — `Mechanical` (density, Young's modulus, yield strength, Poisson's ratio, thermal-expansion coefficient), `Thermal` (conductivity, specific heat, U-value), `Acoustic` (the per-octave-band absorption spectrum and per-band sound-reduction-index vector over the `AcousticBand` centres), `Fire` (reaction-to-fire class, fire-resistance rating) — and one `MaterialPropertySet` is the `MaterialId`-keyed bundle a `Construction/assembly#MATERIAL_ASSIGNMENT` element reads. One `AssemblyProperty` is the aggregation receipt the construction model folds from a `LayerSet`/`ConstituentSet`: a series-resistance U-value (ISO 6946), a constituent-`Fraction`-weighted rule-of-mixtures density/conductivity (the IFC `IfcMaterialConstituentSet.Fraction` mixture rule), and a mass-law/coincidence layered sound-reduction index (ISO 12354) read from the per-band SRI vectors of each ply. A property is NEVER a per-discipline material type: a wall material carries its U-value, sound-transmission spectrum, fire rating, and structural grade as `MaterialProperty` cases over one `MaterialId`, so a material is a full engineering object rather than a shade — never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface; an assembly's U-value/STC/fire envelope is the one `AssemblyProperty` fold over its plies, never a parallel composite-material owner re-keyed per assembly. The property family grows by data — a new property is one `MaterialProperty` case (admitted only when no existing case's column set carries it), a new quantity one column on an existing case, a new acoustic band one `AcousticBand` row, and a new aggregation rule one `AssemblyProperty` fold over the same assignment. The page composes the admitted `UnitsNet` quantity/unit enums for every measured property (the `physical-properties` author-kernel coercing each quantity to its SI base exactly as `Appearance/photometric#PHOTOMETRIC` coerces the luminous family), `Construction/assembly#MATERIAL_ASSIGNMENT` `LayerSet`/`ConstituentSet`/`ProfileSet` for the `MaterialId` key and the aggregation plies, and the `MaterialFault` band-2450 rail (`bsdf#SHADING_FRAME`) for a non-finite or out-of-range admission; the property set and the assembly receipt serialize to IFC 4.3 `IfcMaterialProperties`/`Pset_` at the `Rasm.Bim` boundary and feed the forward `cs:AEC_SIMULATION_BRIDGE` analysis consumer by `MaterialId`, host-neutral here.

## [01]-[INDEX]

- [01]-[MATERIAL_PROPERTY]: the `MaterialProperty` `[Union]` mechanical/thermal/acoustic/fire family, the `AcousticBand` octave-centre vocabulary and the banded `Acoustic` carrier, the `MaterialPropertySet` `MaterialId`-keyed bundle, the `Admit` quantity-coercion seam composing the `UnitsNet` enums, and the `MaterialPropertyLibrary` registered-row table.
- [02]-[ASSEMBLY_PROPERTY]: the `AssemblyProperty` aggregation receipt and the `AssemblyAggregator` series-resistance U-value, rule-of-mixtures, and mass-law/coincidence layered-STC folds over a `Construction/assembly#MATERIAL_ASSIGNMENT` `LayerSet`/`ConstituentSet`, reading each ply's `MaterialPropertySet`.

## [02]-[MATERIAL_PROPERTY]

- Owner: `MaterialProperty` `[Union]` over the IFC 4.3 `IfcMaterialProperties` family; `MaterialPropertySet` the `MaterialId`-keyed bundle; `MaterialPropertyKind`/`FireRating`/`AcousticBand` the closed coercion, reaction-to-fire, and octave-centre bands; `MaterialPropertyLibrary` the registered-row table.
- Cases: `Mechanical` (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`) · `Thermal` (`ConductivityWMK`/`SpecificHeatJKgK`/`UValueWM2K`) · `Acoustic` (the six-band `ReadOnlyMemory<double>` `AbsorptionSpectrum` over the 125/250/500/1k/2k/4k Hz `AcousticBand` centres and the per-band `SoundReductionIndexDb` vector, with `Nrc`/`Saa`/`StcWeighted` derived projection folds) · `Fire` (`Reaction`/`ResistanceMinutes`) — the closed `IfcMaterialProperties` Pset family; a property is a `MaterialProperty` case over a `MaterialId`, never a property subtype.
- Entry: `public static Fin<MaterialProperty> Admit(MaterialPropertyKind kind, double value, Enum unit, UnitPolicy policy, Op key, Guid correlation)` — the magnitude coercion routing the raw value through the kind's `UnitsNet` quantity to its SI base (`Density.KilogramPerCubicMeter` and `Pressure.Pascal` carry the mechanical columns; conductivity coerces through `ThermalConductivityUnit.WattPerMeterKelvin`, specific heat through `SpecificEntropyUnit.JoulePerKilogramKelvin`, and U-value through `HeatTransferCoefficientUnit.WattPerSquareMeterKelvin` — the dedicated thermal SI-base enums catalogued in `.api/api-unitsnet.md`; the dimensionless ratios pass through `RatioUnit.DecimalFraction`), `Fin<T>` aborting on a non-finite or out-of-range column (`MaterialFault.Parameter`); `MaterialProperty.Acoustic.Of(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri, Op key)` admits the two six-band vectors (each band in `[0,1]` for absorption, each SRI finite, both lengths equal to the six `AcousticBand` centres) once at construction; `MaterialPropertySet.Of(MaterialId id, Seq<MaterialProperty> properties, Op key)` bundles a set, `Resolve` projects the `UValueWM2K` thermal envelope and the `ResistanceMinutes` fire rating the BIM federation reads.
- Packages: UnitsNet (the `Density`/`Pressure`/`ThermalConductivity`/`SpecificEntropy`/`HeatTransferCoefficient` quantity structs and the `DensityUnit.KilogramPerCubicMeter`/`PressureUnit.Megapascal`/`ThermalConductivityUnit.WattPerMeterKelvin`/`SpecificEntropyUnit.JoulePerKilogramKelvin`/`HeatTransferCoefficientUnit.WattPerSquareMeterKelvin`/`RatioUnit.DecimalFraction` unit enums the author-kernel rescales to SI base), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`/`ReadOnlyMemory<double>`).
- Growth: a new engineering property shared across materials is one column on its `MaterialProperty` case (defaulted so existing rows are unaffected); a new property discipline with no fit is one `MaterialProperty` case carrying its `IfcMaterialProperties` Pset mapping — never a per-discipline material type, never a parallel property surface. A new fire-reaction class is one `FireRating` row, a new coercible quantity one `MaterialPropertyKind` row, a new acoustic band one `AcousticBand` row (the band carrier widens by data, the projection folds re-read the new band index), a new acoustic rating one expression-bodied fold over the band carrier (NEVER a stored scalar column); the bands grow by data the way `masonry#PROFILE_FAMILY` `Coring` grows.
- Boundary: `MaterialProperty` NEVER re-mints a unit owner — the measured columns admit through the `UnitsNet` quantity exactly as `Appearance/photometric#PHOTOMETRIC` admits the luminous family, the value coerced to its SI base once at `Admit` through `UnitAlgebra.Numeric(value, unit, CanonicalUnit)` and interior columns carried as raw SI doubles; the `PoissonsRatio` dimensionless ratio admits as `UnitInterval` so an out-of-`[0,1]` ratio is unrepresentable, the `FireRating` reaction-to-fire class as a closed `[SmartEnum<string>]` band so a non-standard class is a row never a free string; the `Acoustic` case is a banded spectrum NOT a scalar STC — the `AbsorptionSpectrum` is a fixed-length six-band `ReadOnlyMemory<double>` over the `AcousticBand` octave centres (the same fixed-interval-array carrier shape the appearance side admits in the Unicolour `new Pigment(int start, int interval, double[])` / `Spd` SPD, never the 3-band RGB `bsdf#SPECTRAL_UPSAMPLE` `SpectralBand` `[SmartEnum]` whose role here is only the band-centre vocabulary precedent), the `SoundReductionIndexDb` the matching per-band SRI vector the `[3]-[ASSEMBLY_PROPERTY]` layered-STC fold reads, and `Nrc` (the four-band 250/500/1k/2k arithmetic mean, ASTM C423), `Saa` (the twelve-third-octave SAA approximated as the six-band mean), and `StcWeighted` (the single-number STC contour fit over the SRI bands, ASTM E413) are expression-bodied projection folds over the carriers, never stored ratings that could drift from the spectrum; an out-of-`[0,1]` absorption band or a non-finite SRI band rails `MaterialFault.Parameter` (band 2450) at `Acoustic.Of`, never a clamped sentinel; `MaterialPropertySet` keys on the `Construction/assembly#MATERIAL_ASSIGNMENT` `MaterialId` so a `LayerSet` layer's U-value composes the cumulative thermal envelope and a `ProfileSet` member's `YieldStrengthMpa` feeds the `Profiles/steel#STEEL_FAMILY` `SteelSection.Classify`/`Capacity` design seam — the property model crosses to `Rasm.Bim` federation by `MaterialId`, never re-deriving a BIM property surface; the set serializes to IFC 4.3 `IfcMaterialProperties`/`Pset_MaterialMechanical`/`Pset_MaterialThermal`/`Pset_MaterialAcoustic` at the BIM boundary, host-neutral here, and a non-finite or out-of-range admission rails `MaterialFault` (band 2450, parameter case), never a sentinel property.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MaterialPropertyKind {
    public static readonly MaterialPropertyKind Density            = new(0, canonicalUnit: DensityUnit.KilogramPerCubicMeter);
    public static readonly MaterialPropertyKind YoungsModulus      = new(1, canonicalUnit: PressureUnit.Megapascal);
    public static readonly MaterialPropertyKind YieldStrength      = new(2, canonicalUnit: PressureUnit.Megapascal);
    public static readonly MaterialPropertyKind PoissonsRatio      = new(3, canonicalUnit: RatioUnit.DecimalFraction, ratio: true);
    public static readonly MaterialPropertyKind ThermalExpansion   = new(4, canonicalUnit: RatioUnit.DecimalFraction);
    public static readonly MaterialPropertyKind Conductivity       = new(5, canonicalUnit: ThermalConductivityUnit.WattPerMeterKelvin);
    public static readonly MaterialPropertyKind SpecificHeat       = new(6, canonicalUnit: SpecificEntropyUnit.JoulePerKilogramKelvin);
    public static readonly MaterialPropertyKind UValue             = new(7, canonicalUnit: HeatTransferCoefficientUnit.WattPerSquareMeterKelvin);
    public static readonly MaterialPropertyKind Absorption         = new(8, canonicalUnit: RatioUnit.DecimalFraction, ratio: true);

    public Enum CanonicalUnit { get; }
    public bool Ratio { get; }

    public Fin<double> Coerce(double value, Enum unit, Op key) =>
        Ratio
            ? value is >= 0.0 and <= 1.0 ? Fin.Succ(value) : MaterialFault.Parameter(key, $"<ratio-out-of-unit:{Key}:{value:R}>")
            : UnitAlgebra.Numeric(value, unit, CanonicalUnit);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
public sealed partial class FireRating {
    public static readonly FireRating A1 = new("A1", combustible: false);
    public static readonly FireRating A2 = new("A2", combustible: false);
    public static readonly FireRating B  = new("B", combustible: true);
    public static readonly FireRating C  = new("C", combustible: true);
    public static readonly FireRating D  = new("D", combustible: true);
    public static readonly FireRating E  = new("E", combustible: true);
    public static readonly FireRating F  = new("F", combustible: true);
    public bool Combustible { get; }
}

[SmartEnum<int>]
public sealed partial class AcousticBand {
    public static readonly AcousticBand Hz125  = new(0, centerHz: 125);
    public static readonly AcousticBand Hz250  = new(1, centerHz: 250);
    public static readonly AcousticBand Hz500  = new(2, centerHz: 500);
    public static readonly AcousticBand Hz1000 = new(3, centerHz: 1000);
    public static readonly AcousticBand Hz2000 = new(4, centerHz: 2000);
    public static readonly AcousticBand Hz4000 = new(5, centerHz: 4000);

    public int CenterHz { get; }
    public int Index => Key;
    public static readonly int Count = Items.Count;
    public static bool IsNrcBand(int index) => index is >= 1 and <= 4;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialProperty {
    private MaterialProperty() { }

    public sealed record Mechanical(double DensityKgM3, double YoungsModulusMpa, double YieldStrengthMpa, UnitInterval PoissonsRatio, double ThermalExpansionPerK) : MaterialProperty;
    public sealed record Thermal(double ConductivityWMK, double SpecificHeatJKgK, double UValueWM2K) : MaterialProperty;

    public sealed record Acoustic : MaterialProperty {
        public ReadOnlyMemory<double> AbsorptionSpectrum { get; }
        public ReadOnlyMemory<double> SoundReductionIndexDb { get; }

        private Acoustic(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri) =>
            (AbsorptionSpectrum, SoundReductionIndexDb) = (absorption, sri);

        public static Fin<Acoustic> Of(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri, Op key) =>
            guard(absorption.Length == AcousticBand.Count && sri.Length == AcousticBand.Count, MaterialFault.Parameter(key, $"<acoustic-band-arity:absorption={absorption.Length}:sri={sri.Length}:expected={AcousticBand.Count}>"))
                .Bind(_ => OutOfUnit(absorption.Span) is { } badAbs
                    ? MaterialFault.Parameter(key, $"<acoustic-absorption-out-of-unit:{badAbs:R}>")
                    : NonFinite(sri.Span) is { } badSri
                        ? MaterialFault.Parameter(key, $"<acoustic-sri-non-finite:{badSri:R}>")
                        : Fin.Succ(new Acoustic(absorption, sri)));

        internal static Acoustic Seed(double[] absorption, double[] sri) => new(absorption, sri);

        static double? OutOfUnit(ReadOnlySpan<double> bands) {
            foreach (double b in bands) { if (b is < 0.0 or > 1.0 || !double.IsFinite(b)) { return b; } }
            return null;
        }

        static double? NonFinite(ReadOnlySpan<double> bands) {
            foreach (double b in bands) { if (!double.IsFinite(b)) { return b; } }
            return null;
        }

        public double At(AcousticBand band) => AbsorptionSpectrum.Span[band.Index];
        public double SriAt(AcousticBand band) => SoundReductionIndexDb.Span[band.Index];

        public double Nrc =>
            Math.Round(toSeq(Enumerable.Range(0, AcousticBand.Count))
                .Filter(AcousticBand.IsNrcBand)
                .Map(i => AbsorptionSpectrum.Span[i])
                .Average() * 20.0, MidpointRounding.AwayFromZero) / 20.0;

        public double Saa =>
            Math.Round(toSeq(Enumerable.Range(0, AcousticBand.Count))
                .Map(i => AbsorptionSpectrum.Span[i])
                .Average() * 100.0, MidpointRounding.AwayFromZero) / 100.0;

        public int StcWeighted => StcContourFit(SoundReductionIndexDb);
    }

    public sealed record Fire(FireRating Reaction, double ResistanceMinutes) : MaterialProperty;

    public double Magnitude => Switch(
        mechanical: static m => m.YieldStrengthMpa,
        thermal:    static t => t.UValueWM2K,
        acoustic:   static a => a.StcWeighted,
        fire:       static f => f.ResistanceMinutes);

    internal static int StcContourFit(ReadOnlyMemory<double> sri) {
        ReadOnlySpan<double> bands = sri.Span;
        int best = 0;
        for (int stc = 80; stc >= 0; stc--) {
            double deficiencySum = 0.0;
            bool maxOk = true;
            for (int i = 0; i < bands.Length; i++) {
                double contour = StcContourDb(stc, i);
                double deficiency = Math.Max(0.0, contour - bands[i]);
                deficiencySum += deficiency;
                if (deficiency > 8.0) { maxOk = false; }
            }
            if (deficiencySum <= 32.0 && maxOk) { best = stc; break; }
        }
        return best;
    }

    static double StcContourDb(int stc, int bandIndex) =>
        bandIndex switch {
            0 => stc - 16.0,
            1 => stc - 5.0,
            2 => stc + 0.0,
            3 => stc + 1.0,
            4 => stc + 4.0,
            _ => stc + 4.0,
        };
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record MaterialPropertySet(MaterialId Material, Seq<MaterialProperty> Properties) {
    public Option<MaterialProperty.Thermal> Thermal => Properties.Choose(static p => p is MaterialProperty.Thermal t ? Some(t) : None).HeadOrNone();
    public Option<MaterialProperty.Mechanical> Mechanical => Properties.Choose(static p => p is MaterialProperty.Mechanical m ? Some(m) : None).HeadOrNone();
    public Option<MaterialProperty.Acoustic> Acoustic => Properties.Choose(static p => p is MaterialProperty.Acoustic a ? Some(a) : None).HeadOrNone();

    public static Fin<MaterialPropertySet> Of(MaterialId material, Seq<MaterialProperty> properties, Op key) =>
        properties.IsEmpty
            ? MaterialFault.Parameter(key, $"<material-property-set-empty:{material.Value}>")
            : Fin.Succ(new MaterialPropertySet(material, properties));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialPropertyLibrary {
    public static Fin<double> Admit(MaterialPropertyKind kind, double value, Enum unit, UnitPolicy policy, Op key, Guid correlation) =>
        double.IsFinite(value) && value >= 0.0
            ? kind.Coerce(value, unit, key)
            : MaterialFault.Parameter(key, $"<material-property-non-finite:{kind.Key}:{value:R}>");

    public static readonly FrozenDictionary<MaterialId, MaterialPropertySet> Rows = new (MaterialId Id, MaterialPropertySet Set)[] {
        (MaterialId.Of("metal.iron"), new(MaterialId.Of("metal.iron"), Seq<MaterialProperty>(
            new MaterialProperty.Mechanical(7850.0, 200_000.0, 250.0, UnitInterval.Create(0.30), 12.0e-6),
            new MaterialProperty.Thermal(50.0, 490.0, 5.88),
            new MaterialProperty.Fire(FireRating.A1, 30.0)))),
        (MaterialId.Of("stone.marble"), new(MaterialId.Of("stone.marble"), Seq<MaterialProperty>(
            new MaterialProperty.Mechanical(2700.0, 70_000.0, 15.0, UnitInterval.Create(0.25), 7.0e-6),
            new MaterialProperty.Thermal(2.8, 880.0, 3.50),
            new MaterialProperty.Fire(FireRating.A1, 120.0)))),
        (MaterialId.Of("wood.oak"), new(MaterialId.Of("wood.oak"), Seq<MaterialProperty>(
            new MaterialProperty.Mechanical(700.0, 11_000.0, 40.0, UnitInterval.Create(0.35), 5.0e-6),
            new MaterialProperty.Thermal(0.17, 2400.0, 0.49),
            MaterialProperty.Acoustic.Seed(
                absorption: new[] { 0.05, 0.08, 0.10, 0.12, 0.10, 0.09 },
                sri:        new[] { 18.0, 24.0, 31.0, 36.0, 40.0, 33.0 }),
            new MaterialProperty.Fire(FireRating.D, 30.0)))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Set, MaterialKeyPolicy.EqualityComparer);

    public static Fin<MaterialPropertySet> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialPropertySet? set) ? Fin.Succ(set!) : MaterialFault.Parameter(key, $"<unregistered-material-properties:{id.Value}>");
}
```

## [03]-[ASSEMBLY_PROPERTY]

- Owner: `AssemblyProperty` the aggregation receipt (the assembly U-value, the layered weighted sound-reduction index, the rule-of-mixtures effective density/conductivity, the worst-ply fire envelope); `AssemblyAggregator` the static fold kernel over a `Construction/assembly#MATERIAL_ASSIGNMENT` reading each ply's `MaterialPropertySet`; `ConstituentWeight` the `(MaterialId, Fraction)` aggregation input the IFC `IfcMaterialConstituentSet.Fraction` supplies.
- Cases: one `AssemblyProperty` receipt over a `LayerSet` or a `ConstituentSet` — the thermal `UValueWM2K` (ISO 6946 series resistance), the acoustic `StcWeighted` (the ISO 12354 mass-law/coincidence layered SRI fold), the `EffectiveDensityKgM3`/`EffectiveConductivityWMK` (the rule-of-mixtures constituent-fraction sums), and the `FireResistanceMinutes` (the minimum ply rating); a new aggregation rule is one `AssemblyAggregator` fold over the same assignment, never a parallel composite-material owner.
- Entry: `public static Fin<AssemblyProperty> Aggregate(MaterialAssignment assignment, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<ConstituentWeight> weights, Op key)` — the one aggregation entry discriminating the assignment shape: a `LayerSet` folds the series-resistance U-value `1/U = Σ(t_i/λ_i)` over the plies' `MaterialProperty.Thermal` and the mass-law/coincidence layered SRI over the plies' `MaterialProperty.Acoustic.SoundReductionIndexDb`; a `ConstituentSet` folds the constituent-`Fraction`-weighted rule-of-mixtures effective density/conductivity; each fold an immutable `Fold` over the assignment plies/constituents reading the resolver-supplied per-material `MaterialPropertySet`, `Fin<T>` aborting on an absent ply property (`MaterialFault.Parameter`, never a default) or a non-normalizing constituent fraction set.
- Packages: no new package — composes `Construction/assembly#MATERIAL_ASSIGNMENT` `LayerSet.Layers`/`TotalThickness`/`ConstituentSet`, the `[2]-[MATERIAL_PROPERTY]` `Thermal`/`Mechanical`/`Acoustic` cases and the banded `SoundReductionIndexDb` vector, the `UnitsNet` thermal coercion already on the page (the conductivity/U-value `ThermalConductivityUnit.WattPerMeterKelvin`/`HeatTransferCoefficientUnit.WattPerSquareMeterKelvin` SI base), Rasm (project — `Dimension` layer thickness), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new assembly rating is one `AssemblyAggregator` fold over the same assignment reading the same `MaterialPropertySet` cases — a thermal-bridge psi-value, a vapor-resistance fold, a thermal-mass capacity sum each lands as one fold and one `AssemblyProperty` column, never a parallel composite owner and never a re-keyed per-assembly property; the `AssemblyProperty` receipt grows by column the way `MaterialPropertySet` grows by case.
- Boundary: `AssemblyAggregator` NEVER stores a composite material — an assembly's U-value/STC/effective density is COMPUTED from its plies on demand, the `AssemblyProperty` the receipt the BIM Pset and the `cs:AEC_SIMULATION_BRIDGE` consumer read keyed by the assignment's `MaterialId` set, never a second `MaterialLibrary`-style row table; the series-resistance fold reads each `LayerSet` ply's `MaterialLayer.ThicknessMm` `Dimension` and the ply material's `MaterialProperty.Thermal.ConductivityWMK`, the surface-film resistances `Rsi`/`Rse` (0.13 / 0.04 m²K/W, ISO 6946 interior/exterior) added once at the envelope ends so the `UValueWM2K` is the reciprocal of the total resistance, NEVER re-derived per ply; the layered-STC fold reads each ply's `MaterialProperty.Acoustic.SoundReductionIndexDb` and sums the per-band sound-reduction indices in series (the ISO 12354 simplified composite where each leaf's transmission multiplies, so the per-band SRI adds in dB), the resulting per-band layered SRI vector fed once through the SAME `MaterialProperty.StcContourFit` single-number kernel the per-material `StcWeighted` uses so the assembly STC and the material STC share one contour-fit owner, never a second STC algorithm; the rule-of-mixtures fold reads the `ConstituentWeight` `Fraction` set (the IFC `IfcMaterialConstituentSet.Fraction` the assembly `MaterialConstituent` Category keys, supplied to the fold until a `Fraction` column lands on `Construction/assembly#MATERIAL_ASSIGNMENT` `MaterialConstituent`) and sums the fraction-weighted `Mechanical.DensityKgM3`/`Thermal.ConductivityWMK`, a fraction set that does not sum to one within tolerance railing `MaterialFault.Parameter`; an absent ply property (a `LayerSet` layer whose material's `MaterialPropertySet` lacks the `Thermal`/`Acoustic` case the fold reads) rails `MaterialFault.Parameter` (band 2450) rather than defaulting to a sentinel conductivity, so an under-specified buildup is a typed fault the construction model surfaces, never a silently-wrong envelope; the `AssemblyProperty` serializes to the IFC `Pset_` aggregate (`ThermalTransmittance`/`AcousticRating`) at the `Rasm.Bim` boundary, host-neutral here.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ConstituentWeight(MaterialId Material, double Fraction);

public sealed record AssemblyProperty(
    double UValueWM2K,
    int StcWeighted,
    double EffectiveDensityKgM3,
    double EffectiveConductivityWMK,
    double FireResistanceMinutes);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssemblyAggregator {
    const double RsiWM2K = 0.13;
    const double RseWM2K = 0.04;
    const double FractionToleranceUnit = 1e-3;

    public static Fin<AssemblyProperty> Aggregate(MaterialAssignment assignment, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<ConstituentWeight> weights, Op key) =>
        assignment switch {
            MaterialAssignment.LayerSet set => AggregateLayers(set, resolve, key),
            MaterialAssignment.ConstituentSet set => AggregateConstituents(set, resolve, weights, key),
            _ => MaterialFault.Parameter(key, "<assembly-aggregation-requires-layer-or-constituent-set>"),
        };

    static Fin<AssemblyProperty> AggregateLayers(MaterialAssignment.LayerSet set, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Op key) =>
        set.Layers.Fold(
            Fin.Succ((Resistance: RsiWM2K + RseWM2K, Sri: new double[AcousticBand.Count], MassKgM2: 0.0, ThicknessM: 0.0, MinFire: double.MaxValue)),
            (acc, layer) => acc.Bind(state => resolve(layer.Material).Bind(props =>
                from thermal in props.Thermal.ToFin(MaterialFault.Parameter(key, $"<assembly-layer-missing-thermal:{layer.Material.Value}>"))
                from acoustic in props.Acoustic.ToFin(MaterialFault.Parameter(key, $"<assembly-layer-missing-acoustic:{layer.Material.Value}>"))
                from mech in props.Mechanical.ToFin(MaterialFault.Parameter(key, $"<assembly-layer-missing-mechanical:{layer.Material.Value}>"))
                let thicknessM = layer.ThicknessMm.Value / 1000.0
                let fire = props.Properties.Choose(static p => p is MaterialProperty.Fire f ? Some(f.ResistanceMinutes) : None).HeadOrNone().IfNone(0.0)
                select state with {
                    Resistance = state.Resistance + thicknessM / Math.Max(thermal.ConductivityWMK, double.Epsilon),
                    Sri = AddBands(state.Sri, acoustic.SoundReductionIndexDb),
                    MassKgM2 = state.MassKgM2 + mech.DensityKgM3 * thicknessM,
                    ThicknessM = state.ThicknessM + thicknessM,
                    MinFire = Math.Min(state.MinFire, fire) })))
            .Map(state => new AssemblyProperty(
                UValueWM2K: 1.0 / state.Resistance,
                StcWeighted: MaterialProperty.StcContourFit(state.Sri.AsMemory()),
                EffectiveDensityKgM3: state.ThicknessM > 0.0 ? state.MassKgM2 / state.ThicknessM : 0.0,
                EffectiveConductivityWMK: state.Resistance > RsiWM2K + RseWM2K ? state.ThicknessM / (state.Resistance - RsiWM2K - RseWM2K) : 0.0,
                FireResistanceMinutes: state.MinFire is double.MaxValue ? 0.0 : state.MinFire));

    static Fin<AssemblyProperty> AggregateConstituents(MaterialAssignment.ConstituentSet set, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<ConstituentWeight> weights, Op key) =>
        guard(Math.Abs(weights.Sum(w => w.Fraction) - 1.0) <= FractionToleranceUnit, MaterialFault.Parameter(key, $"<constituent-fraction-not-normalized:{weights.Sum(w => w.Fraction):R}>"))
            .Bind(_ => weights.Fold(
                Fin.Succ((Density: 0.0, Conductivity: 0.0, MinFire: double.MaxValue)),
                (acc, w) => acc.Bind(state => resolve(w.Material).Bind(props =>
                    from mech in props.Mechanical.ToFin(MaterialFault.Parameter(key, $"<constituent-missing-mechanical:{w.Material.Value}>"))
                    from thermal in props.Thermal.ToFin(MaterialFault.Parameter(key, $"<constituent-missing-thermal:{w.Material.Value}>"))
                    let fire = props.Properties.Choose(static p => p is MaterialProperty.Fire f ? Some(f.ResistanceMinutes) : None).HeadOrNone().IfNone(0.0)
                    select state with {
                        Density = state.Density + w.Fraction * mech.DensityKgM3,
                        Conductivity = state.Conductivity + w.Fraction * thermal.ConductivityWMK,
                        MinFire = Math.Min(state.MinFire, fire) }))))
            .Map(state => new AssemblyProperty(
                UValueWM2K: 0.0,
                StcWeighted: 0,
                EffectiveDensityKgM3: state.Density,
                EffectiveConductivityWMK: state.Conductivity,
                FireResistanceMinutes: state.MinFire is double.MaxValue ? 0.0 : state.MinFire));

    static double[] AddBands(double[] accumulated, ReadOnlyMemory<double> sri) {
        ReadOnlySpan<double> bands = sri.Span;
        double[] next = new double[AcousticBand.Count];
        for (int i = 0; i < AcousticBand.Count; i++) { next[i] = accumulated[i] + bands[i]; }
        return next;
    }
}
```

## [04]-[RESEARCH]

- [IFC_MATERIAL_PROPERTIES]: the IFC 4.3 `IfcMaterialProperties` extends `IfcExtendedProperties` with a `Material` reference and a `Properties` set of `IfcProperty`; the standard `Pset_MaterialMechanical`/`Pset_MaterialThermal`/`Pset_MaterialCommon` property sets carry the canonical column names (`MassDensity`/`YoungModulus`/`PoissonRatio`/`ThermalConductivity`/`SpecificHeatCapacity`). The `MaterialProperty` cases map one-to-one onto the Psets so a `MaterialPropertySet` serializes to `IfcMaterialProperties` at the `Rasm.Bim` boundary; the `AssemblyProperty` receipt maps to the element-level `Pset_` aggregate (`ThermalTransmittance` on `Pset_WallCommon`, `AcousticRating`); the probe is the per-case Pset member-name mapping, authored as portable SI data here.
- [UNITSNET_CANONICAL_UNIT]: REALIZED — the `MaterialPropertyKind.CanonicalUnit` column names the SI-base `UnitsNet` enum the author-kernel rescales to, every thermal kind on its own dedicated quantity enum: `Conductivity` on `ThermalConductivityUnit.WattPerMeterKelvin`, `SpecificHeat` on `SpecificEntropyUnit.JoulePerKilogramKelvin` (the `SpecificEntropy` quantity carries specific-heat-capacity values), `UValue` on `HeatTransferCoefficientUnit.WattPerSquareMeterKelvin`, beside the mechanical `Density.KilogramPerCubicMeter`/`Pressure.Pascal` columns. The `ThermalConductivity`/`SpecificEntropy`/`HeatTransferCoefficient` quantity and unit enum families are catalogued in `.api/api-unitsnet.md`, and the `AssemblyAggregator` series-resistance fold composes the same SI-base working form, so the thermal rows carry no `.api` verification gap.
- [CONSTITUENT_FRACTION]: REALIZED as the `[3]-[ASSEMBLY_PROPERTY]` `AssemblyAggregator.AggregateConstituents` rule-of-mixtures fold — a composite material's effective conductivity/density is the constituent-`Fraction`-weighted sum of its `ConstituentSet` member properties, an immutable `Fold` over the `Construction/assembly#MATERIAL_ASSIGNMENT` `ConstituentSet` reading each constituent's `MaterialPropertySet`, never a parallel composite-material owner. The `Fraction` weights ride the `ConstituentWeight` fold input until the `Fraction` column lands on `Construction/assembly#MATERIAL_ASSIGNMENT` `MaterialConstituent` (a one-column growth on the assembly owner, the only residual).
- [LAYER_BUILDUP_GEOMETRY]: REALIZED as the `[3]-[ASSEMBLY_PROPERTY]` `AssemblyAggregator.AggregateLayers` series-resistance fold — the `LayerSet` U-value is `1/U = Rsi + Σ(t_i/λ_i) + Rse` (ISO 6946) over the plies' `MaterialLayer.ThicknessMm`/`MaterialProperty.Thermal.ConductivityWMK`, the layered SRI the per-band series sum of ply sound-reduction indices fed once through the shared `StcContourFit` (ISO 12354 / ASTM E413), and the effective density the through-thickness mass average; the cumulative-thickness placement-offset geometry stays the realized `Construction/layout#ASSEMBLY_FOLD` `LayerOffset`/`StackLayers` fold, the property aggregation the matching engineering fold on this page.
- [THERMAL_SCALAR_PEER_EXPORT]: the `Thermal.ConductivityWMK`/`Thermal.SpecificHeatJKgK` and `Mechanical.DensityKgM3` columns are the STABLE peer-export contract the AEC-domain peer `Rasm.Fabrication/Process/physics#CUT_PARAMETER` reads to scale its thermal pierce-time and abrasive cut budgets — emitted as RAW SI doubles (W·m⁻¹·K⁻¹, J·kg⁻¹·K⁻¹, kg·m⁻³) at the Properties boundary, NEVER a `MaterialProperty` type crossing the seam. The contract decouples the Fabrication read from whichever `UnitsNet` proxy quantity Materials canonicalizes through (`ThermalConductivityUnit.WattPerMeterKelvin`, `SpecificEntropyUnit.JoulePerKilogramKelvin`, `Density.KilogramPerCubicMeter` per `UNITSNET_CANONICAL_UNIT`): the boundary emits the SI base double the value-object already carries, so a later Materials unit canonicalization does not break the Fabrication budget scaling. The acyclic AEC-domain-peer read is owned both ways — Fabrication reads a stated stable surface and Materials knows the Fabrication consumer is real design pressure on these three accessors. Ripple counterpart: `Rasm.Fabrication` `[TOOL_CUTTING_DATA_TABLE]`.
```
