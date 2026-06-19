# [MATERIALS_PROPERTIES]

THE TYPED MATERIAL-PROPERTY OWNER. One `MaterialProperty` `[Union]` closes the IFC 4.3 `IfcMaterialProperties` engineering-property family — `Mechanical` (density, Young's modulus, yield strength, Poisson's ratio, thermal-expansion coefficient), `Thermal` (conductivity, specific heat, U-value), `Acoustic` (sound-transmission class, sound-absorption coefficient), `Fire` (reaction-to-fire class, fire-resistance rating) — and one `MaterialPropertySet` is the `MaterialId`-keyed bundle a `construction/assembly#MATERIAL_ASSIGNMENT` element reads. A property is NEVER a per-discipline material type: a wall material carries its U-value, sound-transmission class, fire rating, and structural grade as `MaterialProperty` cases over one `MaterialId`, so a material is a full engineering object rather than a shade — never a `StructuralMaterial`/`ThermalMaterial`/`AcousticMaterial` surface. The property family grows by data — a new property is one `MaterialProperty` case (admitted only when no existing case's column set carries it) and a new quantity is one column on an existing case. The page composes the admitted `UnitsNet` quantity/unit enums for every measured property (the `physical-properties` author-kernel coercing each quantity to its SI base exactly as `appearance/photometric#PHOTOMETRIC` coerces the luminous family), `construction/assembly#MATERIAL_ASSIGNMENT` for the `MaterialId` key, and the `MaterialFault` band-2450 rail (`bsdf#SHADING_FRAME`) for a non-finite or out-of-range admission; the property set serializes to IFC 4.3 `IfcMaterialProperties`/`Pset_` at the `Rasm.Bim` boundary, host-neutral here.

## [1]-[INDEX]

One cluster: `[2]-[MATERIAL_PROPERTY]` owns the `MaterialProperty` `[Union]` mechanical/thermal/acoustic/fire family, the `MaterialPropertySet` `MaterialId`-keyed bundle, the `Admit` quantity-coercion seam composing the `UnitsNet` enums, and the `MaterialPropertyLibrary` registered-row table.

## [2]-[MATERIAL_PROPERTY]

- Owner: `MaterialProperty` `[Union]` over the IFC 4.3 `IfcMaterialProperties` family; `MaterialPropertySet` the `MaterialId`-keyed bundle; `MaterialPropertyKind`/`FireRating` the closed coercion and reaction-to-fire bands; `MaterialPropertyLibrary` the registered-row table.
- Cases: `Mechanical` (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`) · `Thermal` (`ConductivityWMK`/`SpecificHeatJKgK`/`UValueWM2K`) · `Acoustic` (`SoundTransmissionClass`/`AbsorptionCoefficient`) · `Fire` (`Reaction`/`ResistanceMinutes`) — the closed `IfcMaterialProperties` Pset family; a property is a `MaterialProperty` case over a `MaterialId`, never a property subtype.
- Entry: `public static Fin<MaterialProperty> Admit(MaterialPropertyKind kind, double value, Enum unit, UnitPolicy policy, Op key, Guid correlation)` — the magnitude coercion routing the raw value through the kind's `UnitsNet` quantity to its SI base (`Density.KilogramPerCubicMeter` and `Pressure.Pascal` are the dedicated quantities; conductivity, specific heat, and U-value ride the catalogued `PowerUnit.Watt`/`EnergyUnit.Joule` proxy units until the dedicated `ThermalConductivityUnit`/`SpecificEntropyUnit` enums are catalogued — [UNITSNET_CANONICAL_UNIT]; the dimensionless ratios pass through `RatioUnit.DecimalFraction`), `Fin<T>` aborting on a non-finite or out-of-range column (`MaterialFault.Parameter`); `MaterialPropertySet.Of(MaterialId id, Seq<MaterialProperty> properties, Op key)` bundles a set, `Resolve` projects the `UValueWM2K` thermal envelope and the `ResistanceMinutes` fire rating the BIM federation reads.
- Packages: UnitsNet (the `Density`/`Pressure` quantity structs and the `DensityUnit.KilogramPerCubicMeter`/`PressureUnit.Megapascal`/`PowerUnit.Watt`/`EnergyUnit.Joule`/`RatioUnit.DecimalFraction` unit enums the author-kernel rescales to SI base; the dedicated `ThermalConductivityUnit`/`SpecificEntropyUnit` enums are the flagged `.api` catalogue gap riding the catalogued `Watt`/`Joule` proxy units), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new engineering property shared across materials is one column on its `MaterialProperty` case (defaulted so existing rows are unaffected); a new property discipline with no fit is one `MaterialProperty` case carrying its `IfcMaterialProperties` Pset mapping — never a per-discipline material type, never a parallel property surface. A new fire-reaction class is one `FireRating` row, a new coercible quantity one `MaterialPropertyKind` row; the bands grow by data the way `masonry#PROFILE_FAMILY` `Coring` grows.
- Boundary: `MaterialProperty` NEVER re-mints a unit owner — the measured columns admit through the `UnitsNet` quantity exactly as `appearance/photometric#PHOTOMETRIC` admits the luminous family, the value coerced to its SI base once at `Admit` through `UnitAlgebra.Numeric(value, unit, CanonicalUnit)` and interior columns carried as raw SI doubles; the `PoissonsRatio`/`AbsorptionCoefficient` dimensionless ratios admit as `UnitInterval` so an out-of-`[0,1]` ratio is unrepresentable, the `FireRating` reaction-to-fire class as a closed `[SmartEnum<string>]` band so a non-standard class is a row never a free string, the `SoundTransmissionClass` as the raw integer STC rating its standard defines; `MaterialPropertySet` keys on the `construction/assembly#MATERIAL_ASSIGNMENT` `MaterialId` so a `LayerSet` layer's U-value composes the cumulative thermal envelope and a `ProfileSet` member's `YieldStrengthMpa` feeds the `profiles/steel#STEEL_FAMILY` `SteelSection.IsCompact` design seam — the property model crosses to `Rasm.Bim` federation by `MaterialId`, never re-deriving a BIM property surface; the set serializes to IFC 4.3 `IfcMaterialProperties`/`Pset_MaterialMechanical`/`Pset_MaterialThermal` at the BIM boundary, host-neutral here, and a non-finite or out-of-range admission rails `MaterialFault` (band 2450, parameter case), never a sentinel property.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MaterialPropertyKind {
    public static readonly MaterialPropertyKind Density            = new(0, canonicalUnit: DensityUnit.KilogramPerCubicMeter);
    public static readonly MaterialPropertyKind YoungsModulus      = new(1, canonicalUnit: PressureUnit.Megapascal);
    public static readonly MaterialPropertyKind YieldStrength      = new(2, canonicalUnit: PressureUnit.Megapascal);
    public static readonly MaterialPropertyKind PoissonsRatio      = new(3, canonicalUnit: RatioUnit.DecimalFraction, ratio: true);
    public static readonly MaterialPropertyKind ThermalExpansion   = new(4, canonicalUnit: RatioUnit.DecimalFraction);
    public static readonly MaterialPropertyKind Conductivity       = new(5, canonicalUnit: PowerUnit.Watt);
    public static readonly MaterialPropertyKind SpecificHeat       = new(6, canonicalUnit: EnergyUnit.Joule);
    public static readonly MaterialPropertyKind UValue             = new(7, canonicalUnit: PowerUnit.Watt);
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialProperty {
    private MaterialProperty() { }

    public sealed record Mechanical(double DensityKgM3, double YoungsModulusMpa, double YieldStrengthMpa, UnitInterval PoissonsRatio, double ThermalExpansionPerK) : MaterialProperty;
    public sealed record Thermal(double ConductivityWMK, double SpecificHeatJKgK, double UValueWM2K) : MaterialProperty;
    public sealed record Acoustic(int SoundTransmissionClass, UnitInterval AbsorptionCoefficient) : MaterialProperty;
    public sealed record Fire(FireRating Reaction, double ResistanceMinutes) : MaterialProperty;

    public double Magnitude => Switch(
        mechanical: static m => m.YieldStrengthMpa,
        thermal:    static t => t.UValueWM2K,
        acoustic:   static a => a.SoundTransmissionClass,
        fire:       static f => f.ResistanceMinutes);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record MaterialPropertySet(MaterialId Material, Seq<MaterialProperty> Properties) {
    public Option<MaterialProperty.Thermal> Thermal => Properties.Choose(static p => p is MaterialProperty.Thermal t ? Some(t) : None).HeadOrNone();
    public Option<MaterialProperty.Mechanical> Mechanical => Properties.Choose(static p => p is MaterialProperty.Mechanical m ? Some(m) : None).HeadOrNone();

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
            new MaterialProperty.Acoustic(33, UnitInterval.Create(0.10)),
            new MaterialProperty.Fire(FireRating.D, 30.0)))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Set, MaterialKeyPolicy.EqualityComparer);

    public static Fin<MaterialPropertySet> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialPropertySet? set) ? Fin.Succ(set!) : MaterialFault.Parameter(key, $"<unregistered-material-properties:{id.Value}>");
}
```

## [3]-[RESEARCH]

- [IFC_MATERIAL_PROPERTIES]: the IFC 4.3 `IfcMaterialProperties` extends `IfcExtendedProperties` with a `Material` reference and a `Properties` set of `IfcProperty`; the standard `Pset_MaterialMechanical`/`Pset_MaterialThermal`/`Pset_MaterialCommon` property sets carry the canonical column names (`MassDensity`/`YoungModulus`/`PoissonRatio`/`ThermalConductivity`/`SpecificHeatCapacity`). The `MaterialProperty` cases map one-to-one onto the Psets so a `MaterialPropertySet` serializes to `IfcMaterialProperties` at the `Rasm.Bim` boundary; the probe is the per-case Pset member-name mapping, authored as portable SI data here.
- [UNITSNET_CANONICAL_UNIT]: the `MaterialPropertyKind.CanonicalUnit` column names the SI-base `UnitsNet` enum the author-kernel rescales to; `Pressure.Pascal`/`Density.KilogramPerCubicMeter` are catalogued in `.api/api-unitsnet.md`, but `ThermalConductivity`/`SpecificEntropy`/`HeatTransferCoefficient` and their unit enums are NOT yet catalogued — the conductivity/specific-heat/U-value columns currently route their `CanonicalUnit` through the catalogued `PowerUnit.Watt`/`EnergyUnit.Joule` proxies and pass the raw SI magnitude. Admitting the dedicated `ThermalConductivityUnit.WattPerMeterKelvin`/`CoefficientOfThermalConductanceUnit` enums is an `.api` catalogue extension (queued in `TASKLOG.md`) that flips each thermal kind's `CanonicalUnit` to its own SI-base enum, two-column edits per row, zero new surface — until then the thermal rows carry an `.api` verification gap and ride the proxy unit.
- [CONSTITUENT_FRACTION]: the IFC `IfcMaterialConstituentSet` `Fraction` column drives a rule-of-mixtures property aggregation — a composite material's effective conductivity/density is the constituent-fraction-weighted sum of its `ConstituentSet` member properties; the aggregation is a fold over the `construction/assembly#MATERIAL_ASSIGNMENT` `ConstituentSet` reading each constituent's `MaterialPropertySet`, never a parallel composite-material owner. The probe is the mixture-rule fold, queued with the BIM federation seam.
```
