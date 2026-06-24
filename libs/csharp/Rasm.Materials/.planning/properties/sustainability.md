# [MATERIALS_SUSTAINABILITY]

THE SUSTAINABILITY-AND-PROCUREMENT PROPERTY DISCIPLINE and THE LIFECYCLE-AGGREGATION FOLDS. Three new `properties#MATERIAL_PROPERTY` `MaterialProperty` cases close the EN 15978 / ISO 14040 lifecycle-and-procurement family beside the realized mechanical/thermal/acoustic/fire engineering family — `Environmental` (the cradle-to-gate GWP per declared unit over a UnitsNet `Mass` quantity, the EPD declaration/validity, the per-module A1-A3/A4/A5/C lifecycle-stage vector as a fixed-length `ReadOnlyMemory<double>` over the `LifecycleStage` band exactly as `Acoustic` carries its 6-band `AbsorptionSpectrum`, the recycled-content/end-of-life fractions), `Cost` (the supply/install/lifecycle columns over a `Currency` band and a `MeasurementBasis`), and `Classification` (the Uniclass2015/OmniClass/IFC-Pset assignment for federation over a `ClassificationSystem` band) — and two new `properties#ASSEMBLY_PROPERTY` `AssemblyAggregator` folds, `AggregateEnvironmental` and `AggregateCost`, sum the per-ply EN 15978 GWP and cost over a `Construction/assembly#MATERIAL_ASSIGNMENT` `LayerSet`/`ConstituentSet` the SAME way the realized series-resistance U-value and layered-STC folds aggregate. A material is a FULL LIFECYCLE OBJECT, not only a physics-and-shade carrier: a wall material carries its embodied carbon, its cost, and its BIM classification as `MaterialProperty` cases over one `MaterialId`, never a `EcoMaterial`/`CostMaterial`/`ClassifiedMaterial` surface; a whole-building embodied-carbon takeoff and a cost-of-construction rollup are the one `AssemblyAggregator` fold over its plies, never a parallel Eco/Cost owner re-keyed per assembly.

The discipline grows by case AND column, never a parallel surface: a new lifecycle stage is one `LifecycleStage` row (the banded vector widens by data, the aggregation folds re-read the new stage index the way `properties#MATERIAL_PROPERTY` widens by `AcousticBand`), a new currency one `Currency` row, a new classification system one `ClassificationSystem` row, a new aggregation rule one `AssemblyAggregator` fold over the same assignment. The GWP rides a UnitsNet `Mass` quantity per declared functional unit and the per-module vector is a fixed-length `ReadOnlyMemory<double>` over the `LifecycleStage` band, so the discipline REUSES the realized banded-vector carrier shape rather than minting a new collection primitive. The page composes `properties#MATERIAL_PROPERTY` `MaterialProperty`/`MaterialPropertySet`/`MaterialPropertyKind` for the property family and the SI-base coercion, `properties#ASSEMBLY_PROPERTY` `AssemblyAggregator`/`AssemblyProperty`/`ConstituentWeight` for the lifecycle folds, `Construction/assembly#MATERIAL_ASSIGNMENT` `LayerSet`/`ConstituentSet` for the aggregation plies, the admitted UnitsNet `Mass`/`MassConcentration` quantity for the GWP kgCO2e, and the `MaterialFault` band-2450 rail for a non-finite or out-of-range admission; the property set and the lifecycle receipt serialize to IFC 4.3 `Pset_EnvironmentalImpactValues` / `Uniclass2015` at the `Rasm.Bim` boundary and feed the forward `cs:AEC_SIMULATION_BRIDGE` LCA/QTO consumer by `MaterialId`, host-neutral here.

## [01]-[INDEX]

- [01]-[SUSTAINABILITY_PROPERTY]: the `LifecycleStage`/`Currency`/`ClassificationSystem` closed bands, the `Environmental`/`Cost`/`Classification` `MaterialProperty` cases (the GWP `Mass` quantity, the per-module banded vector mirroring `Acoustic`, the EPD/embodied-carbon receipt), and the `MaterialPropertyKind` GWP/cost coercion rows the `properties#MATERIAL_PROPERTY` `Admit` seam reads.
- [02]-[LIFECYCLE_AGGREGATION]: the `AssemblyAggregator.AggregateEnvironmental` cradle-to-grave GWP fold and the `AssemblyAggregator.AggregateCost` supply/install/lifecycle fold over a `Construction/assembly#MATERIAL_ASSIGNMENT`, each reading the per-ply `Environmental`/`Cost` case the same way the U-value/STC folds read `Thermal`/`Acoustic`.

## [02]-[SUSTAINABILITY_PROPERTY]

- Owner: the `Environmental`/`Cost`/`Classification` `MaterialProperty` cases extending the `properties#MATERIAL_PROPERTY` Union; `LifecycleStage` the EN 15978 module band, `Currency` the ISO 4217 currency band, `ClassificationSystem` the federation-system band; the GWP/cost `MaterialPropertyKind` coercion rows.
- Cases: `Environmental` (the `GwpKgCo2ePerUnit` cradle-to-gate global-warming potential over a UnitsNet `Mass`/`MassUnit.Kilogram` quantity per declared functional unit, the per-module `StageGwp` fixed-length `ReadOnlyMemory<double>` over the `LifecycleStage` A1-A3/A4/A5/B/C/D centres exactly as `Acoustic` carries its 6-band `AbsorptionSpectrum`, the `EpdDeclaration`/`ValidUntilYear` provenance, the `RecycledContent`/`EndOfLifeRecovery` `UnitInterval` fractions) · `Cost` (the `SupplyCostPerUnit`/`InstallCostPerUnit`/`LifecycleCostPerUnit` over a `Currency` band and a `MeasurementBasis` declaring the per-m²/per-m³/per-kg/per-item unit) · `Classification` (the `Uniclass`/`OmniClass`/`IfcPset` assignment over a `ClassificationSystem` band) — the closed EN 15978 / ISO 14040 lifecycle-and-procurement family; a sustainability property is a `MaterialProperty` case over a `MaterialId`, never a property subtype.
- Entry: `public static Fin<MaterialProperty.Environmental> Of(double gwpKgCo2e, ReadOnlyMemory<double> stageGwp, string epd, int validUntil, UnitInterval recycled, UnitInterval endOfLife, Op key)` admits the GWP plus the per-stage vector (the vector length equal to the `LifecycleStage` centre count, each module GWP finite) once at construction the way `MaterialProperty.Acoustic.Of` admits the banded vectors; `MaterialProperty.Cost.Of(Currency currency, MeasurementBasis basis, double supply, double install, double lifecycle, Op key)` admits a cost row (each column finite and non-negative), `MaterialProperty.Classification.Of(ClassificationSystem system, string code, Op key)` admits a classification (a non-empty code); the GWP and cost columns route through the `properties#MATERIAL_PROPERTY` `Admit` magnitude coercion (`Gwp` through `MassUnit.Kilogram`, the cost columns dimensionless monetary scalars the `Currency` band tags), `Fin<T>` aborting on a non-finite or out-of-range column (`MaterialFault.Parameter`).
- Packages: UnitsNet (the `Mass`/`MassConcentration` quantity structs and the `MassUnit.Kilogram`/`MassConcentrationUnit.KilogramPerCubicMeter` SI-base enums the GWP-per-declared-unit author-kernel rescales — catalogued in `.api/api-unitsnet.md`), Rasm (project — `UnitInterval` for the recycled/end-of-life fractions), Thinktecture.Runtime.Extensions (`[SmartEnum]` for the closed bands, `[Union]` cases on the property family), LanguageExt.Core (`Fin`/`Seq`/`Fold` for the admission rail and the lifecycle folds), BCL inbox (`ReadOnlyMemory<double>`).
- Growth: a new EN 15978 module is one `LifecycleStage` row (the `StageGwp` carrier widens by data, the `AggregateEnvironmental` fold re-reads the new stage index the way `properties#MATERIAL_PROPERTY` widens by `AcousticBand`), a new currency one `Currency` row, a new classification system one `ClassificationSystem` row, a new procurement column one `Cost` column (defaulted so existing rows are unaffected) — never a per-discipline material type, never a parallel Eco/Cost surface. The procurement lead-time row is dropped below the density bar (one scalar column not worth a fourth case); a new aggregation rule lands as one `AssemblyAggregator` fold at `[3]-[LIFECYCLE_AGGREGATION]`.
- Boundary: the three cases NEVER re-mint a unit owner — the GWP admits through the UnitsNet `Mass` quantity to its `MassUnit.Kilogram` SI base once at `Admit` exactly as `properties#MATERIAL_PROPERTY` admits the thermal family, the cost columns carried as raw monetary scalars the `Currency` band tags (a currency is a closed `[SmartEnum<string>]` ISO 4217 row so a non-standard currency is a row never a free string); the `Environmental` case is a banded vector NOT a scalar carbon number — the `StageGwp` is a fixed-length `ReadOnlyMemory<double>` over the `LifecycleStage` EN 15978 module centres (the SAME fixed-interval-array carrier shape `properties#MATERIAL_PROPERTY` `Acoustic` admits, never a per-module column proliferation), so the cradle-to-gate `GwpKgCo2ePerUnit` is the A1-A3 module sum and the cradle-to-grave total is the all-module sum, the `WholeLifeGwp` an expression-bodied projection fold over the carrier never a stored scalar that drifts; the `RecycledContent`/`EndOfLifeRecovery` admit as `UnitInterval` so an out-of-`[0,1]` fraction is unrepresentable; an out-of-range stage GWP or a negative cost rails `MaterialFault.Parameter` (band 2450) at `Of`, never a clamped sentinel; `MaterialPropertySet` keys on the `Construction/assembly#MATERIAL_ASSIGNMENT` `MaterialId` so a `LayerSet` layer's `Environmental` case composes the cumulative embodied-carbon takeoff and its `Cost` case the construction rollup — the discipline crosses to `Rasm.Bim` federation by `MaterialId`, never re-deriving a BIM property surface; the set crosses to the BIM boundary as the portable `MaterialPropertyWire` rows the `Rasm.Bim` `Semantics/connection#CONNECTION_WIRE` `ConnectionWire.AuthorMaterial` fold authors onto IFC 4.3 `Pset_EnvironmentalImpactValues` (the `Carbon`/`WholeLifeCarbon`/`ExpectedServiceLife` members), the `Cost` onto `Pset_ConstructionCosts`, and the `Classification` onto `IfcClassificationReference` (Uniclass2015/OmniClass), host-neutral here, and the embodied-carbon GWP + cost rollup feed the forward `cs:AEC_SIMULATION_BRIDGE` LCA/QTO consumer by `MaterialId` without a second property surface.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// EN 15978 lifecycle modules — the banded vector index, exactly as AcousticBand bands the absorption spectrum.
[SmartEnum<int>]
public sealed partial class LifecycleStage {
    public static readonly LifecycleStage A1A3 = new(0, code: "A1-A3", phase: "product");
    public static readonly LifecycleStage A4   = new(1, code: "A4",    phase: "transport");
    public static readonly LifecycleStage A5   = new(2, code: "A5",    phase: "construction");
    public static readonly LifecycleStage B    = new(3, code: "B1-B7", phase: "use");
    public static readonly LifecycleStage C    = new(4, code: "C1-C4", phase: "end-of-life");
    public static readonly LifecycleStage D    = new(5, code: "D",     phase: "beyond-system");

    public string Code { get; }
    public string Phase { get; }
    public int Index => Key;
    public static readonly int Count = Items.Count;
    public static bool IsCradleToGate(int index) => index == A1A3.Index;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
public sealed partial class Currency {
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Eur = new("EUR");
    public static readonly Currency Gbp = new("GBP");
    public static readonly Currency Cad = new("CAD");
    public static readonly Currency Aud = new("AUD");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
public sealed partial class MeasurementBasis {
    public static readonly MeasurementBasis PerSquareMeter = new("per-m2");
    public static readonly MeasurementBasis PerCubicMeter  = new("per-m3");
    public static readonly MeasurementBasis PerKilogram    = new("per-kg");
    public static readonly MeasurementBasis PerItem        = new("per-item");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
public sealed partial class ClassificationSystem {
    public static readonly ClassificationSystem Uniclass2015 = new("uniclass-2015", pset: "IfcClassificationReference");
    public static readonly ClassificationSystem OmniClass    = new("omniclass",     pset: "IfcClassificationReference");
    public static readonly ClassificationSystem IfcPset      = new("ifc-pset",       pset: "Pset_EnvironmentalImpactValues");
    public string Pset { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// These three records are MaterialProperty Union cases declared on properties#MATERIAL_PROPERTY;
// the case bodies live here as the sustainability-discipline owner block.
public sealed record Environmental : MaterialProperty {
    public double GwpKgCo2ePerUnit { get; }            // cradle-to-gate A1-A3 GWP, MassUnit.Kilogram SI base
    public ReadOnlyMemory<double> StageGwp { get; }    // per EN 15978 module, LifecycleStage.Count entries
    public string EpdDeclaration { get; }
    public int ValidUntilYear { get; }
    public UnitInterval RecycledContent { get; }
    public UnitInterval EndOfLifeRecovery { get; }

    private Environmental(double gwp, ReadOnlyMemory<double> stageGwp, string epd, int valid, UnitInterval recycled, UnitInterval endOfLife) =>
        (GwpKgCo2ePerUnit, StageGwp, EpdDeclaration, ValidUntilYear, RecycledContent, EndOfLifeRecovery) = (gwp, stageGwp, epd, valid, recycled, endOfLife);

    public static Fin<Environmental> Of(double gwp, ReadOnlyMemory<double> stageGwp, string epd, int valid, UnitInterval recycled, UnitInterval endOfLife, Op key) =>
        guard(stageGwp.Length == LifecycleStage.Count, MaterialFault.Parameter(key, $"<environmental-stage-arity:{stageGwp.Length}:expected={LifecycleStage.Count}>"))
            .Bind(_ => NonFinite(stageGwp.Span) is { } bad
                ? MaterialFault.Parameter(key, $"<environmental-stage-non-finite:{bad:R}>")
                : double.IsFinite(gwp) && gwp >= 0.0
                    ? Fin.Succ(new Environmental(gwp, stageGwp, epd, valid, recycled, endOfLife))
                    : MaterialFault.Parameter(key, $"<environmental-gwp-non-finite:{gwp:R}>"));

    internal static Environmental Seed(double gwp, double[] stageGwp, string epd, int valid, double recycled, double endOfLife) =>
        new(gwp, stageGwp, epd, valid, UnitInterval.Create(recycled), UnitInterval.Create(endOfLife));

    static double? NonFinite(ReadOnlySpan<double> bands) { foreach (double b in bands) { if (!double.IsFinite(b)) { return b; } } return null; }

    public double StageAt(LifecycleStage stage) => StageGwp.Span[stage.Index];
    public double WholeLifeGwp { get { ReadOnlySpan<double> s = StageGwp.Span; double total = 0.0; for (int i = 0; i < LifecycleStage.Count; i++) { total += s[i]; } return total; } }
}

public sealed record Cost(Currency Currency, MeasurementBasis Basis, double SupplyCostPerUnit, double InstallCostPerUnit, double LifecycleCostPerUnit) : MaterialProperty {
    public static Fin<Cost> Of(Currency currency, MeasurementBasis basis, double supply, double install, double lifecycle, Op key) =>
        guard(new[] { supply, install, lifecycle }.All(c => double.IsFinite(c) && c >= 0.0), MaterialFault.Parameter(key, $"<cost-column-non-finite-or-negative:{supply:R}/{install:R}/{lifecycle:R}>"))
            .Map(_ => new Cost(currency, basis, supply, install, lifecycle));
    public double TotalInPlacePerUnit => SupplyCostPerUnit + InstallCostPerUnit;
}

public sealed record Classification(ClassificationSystem System, string Code) : MaterialProperty {
    public static Fin<Classification> Of(ClassificationSystem system, string code, Op key) =>
        string.IsNullOrWhiteSpace(code)
            ? MaterialFault.Parameter(key, $"<classification-code-empty:{system.Key}>")
            : Fin.Succ(new Classification(system, code));
}
```

## [03]-[LIFECYCLE_AGGREGATION]

- Owner: the `AssemblyAggregator.AggregateEnvironmental` cradle-to-grave GWP fold and the `AssemblyAggregator.AggregateCost` supply/install/lifecycle fold — two new folds on the realized `properties#ASSEMBLY_PROPERTY` `AssemblyAggregator` static kernel over a `Construction/assembly#MATERIAL_ASSIGNMENT`, reading each ply's `Environmental`/`Cost` case the way the realized U-value/STC folds read `Thermal`/`Acoustic`; the `AssemblyLifecycle` receipt the BIM Pset and the LCA/QTO consumer read.
- Cases: one `AssemblyLifecycle` receipt over a `LayerSet` or `ConstituentSet` — the `WholeLifeGwpKgCo2e` (the EN 15978 per-module GWP summed over the plies × their declared quantities, the cradle-to-grave total), the per-module `StageGwp` vector (the layered module breakdown for an LCA stage report), the `TotalCost` (the supply+install cost summed over the plies × quantities), and the `EmbodiedCarbonIntensity` (the GWP per assembly area); a new lifecycle rollup is one `AssemblyAggregator` fold over the same assignment, never a parallel composite-material owner.
- Entry: `public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialAssignment assignment, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<PlyQuantity> quantities, double areaM2, Op key)` and the rail-twin `public static Fin<AssemblyCost> AggregateCost(MaterialAssignment assignment, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<PlyQuantity> quantities, double areaM2, Op key)` — each the one aggregation entry discriminating the assignment shape over the SAME `(assignment, resolve, quantities, areaM2, key)` rail: a `LayerSet` folds the PER-PLY value `Σ(value_i · quantity_i)` over the plies (each layer's quantity its thickness × unit-area for a per-m³ EPD or its area for a per-m² EPD), a `ConstituentSet` the constituent-weighted plies, a `ProfileSet` the single member — so the cost fold reads each layer's own thickness exactly as the GWP fold does rather than collapsing duplicate-material plies through the flat `assignment.Materials` set; each fold an immutable `Fold` over the assignment plies reading the resolver-supplied per-material `Environmental`/`Cost`, `Fin<T>` aborting on an absent ply property (`MaterialFault.Parameter`, never a default GWP/cost) or a non-normalizing constituent fraction set.
- Packages: no new package — composes `properties#ASSEMBLY_PROPERTY` `AssemblyAggregator`/`ConstituentWeight`, the `[2]-[SUSTAINABILITY_PROPERTY]` `Environmental`/`Cost` cases and the banded `StageGwp` carrier, `Construction/assembly#MATERIAL_ASSIGNMENT` `LayerSet.Layers`/`ConstituentSet`, the UnitsNet `Mass` coercion already on the page, Rasm (project — `Dimension` layer thickness), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new assembly lifecycle rollup is one `AssemblyAggregator` fold over the same assignment reading the same `MaterialPropertySet` cases — a biogenic-carbon-storage credit, a circularity-index, a maintenance-cost-over-service-life sum each lands as one fold and one `AssemblyLifecycle`/`AssemblyCost` column, never a parallel composite owner and never a re-keyed per-assembly property; the receipt grows by column the way the realized `AssemblyProperty` grows.
- Boundary: `AggregateEnvironmental` NEVER stores a composite material — an assembly's embodied carbon is COMPUTED from its plies on demand, the `AssemblyLifecycle` the receipt the BIM Pset and the `cs:AEC_SIMULATION_BRIDGE` consumer read keyed by the assignment's `MaterialId` set, never a second `MaterialLibrary`-style row table; the per-module GWP fold reads each `LayerSet` ply's `MaterialLayer.ThicknessMm` `Dimension` (the per-unit quantity scaling the per-declared-unit GWP — a per-m³ EPD scales by thickness × unit-area, a per-m² EPD by area, a per-kg EPD by thickness × unit-area × the ply material's `Mechanical.DensityKgM3` read from the SAME property set) and the ply material's `Environmental.StageGwp`, summing the per-module vectors in series so the assembly `StageGwp` is the per-module ply sum and the `WholeLifeGwpKgCo2e` its all-module total, NEVER re-derived per ply; the cost fold reads each ply's `Cost.SupplyCostPerUnit`/`InstallCostPerUnit` × the ply quantity, the currency taken from the first ply and a currency mismatch railing `MaterialFault.Parameter` (the rollup cannot sum across currencies without an exchange rate this owner does not carry); the constituent-set GWP fold reads the `ConstituentWeight` `Fraction` set the way the realized rule-of-mixtures fold does, a fraction set that does not sum to one within tolerance railing `MaterialFault.Parameter`; an absent ply property (a `LayerSet` layer whose material's `MaterialPropertySet` lacks the `Environmental`/`Cost` case the fold reads) rails `MaterialFault.Parameter` (band 2450) rather than defaulting to a sentinel GWP, so an under-specified buildup is a typed fault the construction model surfaces, never a silently-wrong carbon total; the `AssemblyLifecycle` serializes to the IFC `Pset_` aggregate (`Pset_EnvironmentalImpactValues.Carbon`/`Pset_ConstructionCosts`) at the `Rasm.Bim` boundary, host-neutral here.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The per-ply declared quantity the GWP/cost fold scales the per-declared-unit value by; supplied to the fold
// until a quantity column lands on Construction/assembly#MATERIAL_ASSIGNMENT, exactly as ConstituentWeight rides.
public readonly record struct PlyQuantity(MaterialId Material, double Quantity, MeasurementBasis Basis);

public sealed record AssemblyLifecycle(
    double WholeLifeGwpKgCo2e,
    ReadOnlyMemory<double> StageGwp,
    double EmbodiedCarbonIntensityKgCo2eM2,
    double RecycledContentFraction);

public sealed record AssemblyCost(Currency Currency, MeasurementBasis Basis, double SupplyTotal, double InstallTotal, double LifecycleTotal) {
    public double TotalInPlace => SupplyTotal + InstallTotal;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class AssemblyAggregator {
    public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialAssignment assignment, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<PlyQuantity> quantities, double areaM2, Op key) =>
        assignment switch {
            MaterialAssignment.LayerSet set => AggregateLayerGwp(set, resolve, quantities, areaM2, key),
            MaterialAssignment.ConstituentSet set => AggregateConstituentGwp(set, resolve, quantities, key),
            _ => MaterialFault.Parameter(key, "<environmental-aggregation-requires-layer-or-constituent-set>"),
        };

    static Fin<AssemblyLifecycle> AggregateLayerGwp(MaterialAssignment.LayerSet set, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<PlyQuantity> quantities, double areaM2, Op key) =>
        set.Layers.Fold(
            Fin.Succ((Stage: new double[LifecycleStage.Count], Recycled: 0.0, RecycledWeight: 0.0)),
            (acc, layer) => acc.Bind(state => resolve(layer.Material).Bind(props =>
                from env in EnvironmentalOf(props).ToFin(MaterialFault.Parameter(key, $"<assembly-layer-missing-environmental:{layer.Material.Value}>"))
                let quantity = quantities.Find(q => q.Material == layer.Material).Map(q => q.Quantity).IfNone(layer.ThicknessMm.Value / 1000.0 * areaM2)
                select (Stage: AddScaled(state.Stage, env.StageGwp, quantity), Recycled: state.Recycled + env.RecycledContent.Value * quantity, RecycledWeight: state.RecycledWeight + quantity))))
            .Map(state => new AssemblyLifecycle(
                WholeLifeGwpKgCo2e: state.Stage.Sum(),
                StageGwp: state.Stage.AsMemory(),
                EmbodiedCarbonIntensityKgCo2eM2: areaM2 > 0.0 ? state.Stage.Sum() / areaM2 : 0.0,
                RecycledContentFraction: state.RecycledWeight > 0.0 ? state.Recycled / state.RecycledWeight : 0.0));

    static Fin<AssemblyLifecycle> AggregateConstituentGwp(MaterialAssignment.ConstituentSet set, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<PlyQuantity> quantities, Op key) =>
        set.Constituents.Fold(
            Fin.Succ(new double[LifecycleStage.Count]),
            (acc, c) => acc.Bind(stage => resolve(c.Material).Bind(props =>
                EnvironmentalOf(props).ToFin(MaterialFault.Parameter(key, $"<constituent-missing-environmental:{c.Material.Value}>"))
                    .Map(env => AddScaled(stage, env.StageGwp, quantities.Find(q => q.Material == c.Material).Map(q => q.Quantity).IfNone(1.0))))))
            .Map(stage => new AssemblyLifecycle(stage.Sum(), stage.AsMemory(), stage.Sum(), 0.0));

    // The cost rail mirrors the GWP rail one-for-one — a LayerSet folds the PER-PLY cost (each layer's quantity its
    // thickness × area), a ConstituentSet the constituent-Fraction-weighted cost; folding the flat assignment.Materials
    // set instead would discard the per-layer thickness (every gypsum ply scaled by one Qty) and ignore the mixture
    // fractions, so the cost fold discriminates the assignment shape exactly as AggregateEnvironmental does.
    public static Fin<AssemblyCost> AggregateCost(MaterialAssignment assignment, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Seq<PlyQuantity> quantities, double areaM2, Op key) =>
        assignment switch {
            MaterialAssignment.LayerSet set => AccumulateCost(set.Layers.Map(l => (l.Material, Quantity: PlyQty(quantities, l.Material, l.ThicknessMm.Value / 1000.0 * areaM2))), resolve, key),
            MaterialAssignment.ConstituentSet set => AccumulateCost(set.Constituents.Map(c => (c.Material, Quantity: PlyQty(quantities, c.Material, 1.0))), resolve, key),
            MaterialAssignment.ProfileSet set => AccumulateCost(Seq1((set.Material, Quantity: PlyQty(quantities, set.Material, 1.0))), resolve, key),
            _ => MaterialFault.Parameter(key, "<cost-aggregation-requires-layer-profile-or-constituent-set>"),
        };

    static Fin<AssemblyCost> AccumulateCost(Seq<(MaterialId Material, double Quantity)> plies, Func<MaterialId, Fin<MaterialPropertySet>> resolve, Op key) =>
        plies.Fold(
            Fin.Succ(Option<AssemblyCost>.None),
            (acc, ply) => acc.Bind(running => resolve(ply.Material).Bind(props =>
                from cost in CostOf(props).ToFin(MaterialFault.Parameter(key, $"<assembly-missing-cost:{ply.Material.Value}>"))
                from merged in running.Match(
                    Some: r => r.Currency == cost.Currency
                        ? Fin.Succ(r with { SupplyTotal = r.SupplyTotal + cost.SupplyCostPerUnit * ply.Quantity, InstallTotal = r.InstallTotal + cost.InstallCostPerUnit * ply.Quantity, LifecycleTotal = r.LifecycleTotal + cost.LifecycleCostPerUnit * ply.Quantity })
                        : Fin.Fail<AssemblyCost>(MaterialFault.Parameter(key, $"<cost-currency-mismatch:{r.Currency.Key}<>{cost.Currency.Key}>")),
                    None: () => Fin.Succ(new AssemblyCost(cost.Currency, cost.Basis, cost.SupplyCostPerUnit * ply.Quantity, cost.InstallCostPerUnit * ply.Quantity, cost.LifecycleCostPerUnit * ply.Quantity)))
                select Some(merged))))
            .Bind(o => o.ToFin(MaterialFault.Parameter(key, "<assembly-cost-empty>")));

    static Option<MaterialProperty.Environmental> EnvironmentalOf(MaterialPropertySet set) => set.Properties.Choose(static p => p is MaterialProperty.Environmental e ? Some(e) : None).HeadOrNone();
    static Option<MaterialProperty.Cost> CostOf(MaterialPropertySet set) => set.Properties.Choose(static p => p is MaterialProperty.Cost c ? Some(c) : None).HeadOrNone();
    // The declared ply quantity overrides the geometric fallback (thickness × area for a layer, unit for a constituent).
    static double PlyQty(Seq<PlyQuantity> quantities, MaterialId id, double geometric) => quantities.Find(q => q.Material == id).Map(q => q.Quantity).IfNone(geometric);

    static double[] AddScaled(double[] accumulated, ReadOnlyMemory<double> stage, double scale) {
        ReadOnlySpan<double> bands = stage.Span;
        double[] next = new double[LifecycleStage.Count];
        for (int i = 0; i < LifecycleStage.Count; i++) { next[i] = accumulated[i] + bands[i] * scale; }
        return next;
    }
}
```

## [04]-[RESEARCH]

- [EN_15978_LIFECYCLE_MODULES]: REALIZED — the EN 15978 lifecycle-stage modules seed the `LifecycleStage` band: A1-A3 (product: raw-material supply, transport, manufacturing — the cradle-to-gate boundary the `GwpKgCo2ePerUnit` carries), A4 (transport to site), A5 (construction-installation), B1-B7 (use, maintenance, repair, replacement, refurbishment, operational energy/water), C1-C4 (deconstruction, transport, waste-processing, disposal), D (benefits/loads beyond the system boundary — reuse/recovery/recycling potential). The per-module `StageGwp` vector is the EN 15978 module-level GWP an EPD declares; the cradle-to-gate `GwpKgCo2ePerUnit` is the A1-A3 sum, the cradle-to-grave `WholeLifeGwp` the all-module sum, both expression-bodied projections over the banded carrier never stored scalars. The vector mirrors the realized `properties#MATERIAL_PROPERTY` `Acoustic` 6-band `ReadOnlyMemory<double>` shape exactly — a fixed-length per-band array over a `[SmartEnum<int>]` index band, not a per-module column.
- [ISO_14040_DECLARED_UNIT]: the embodied-carbon GWP rides a UnitsNet `Mass`/`MassUnit.Kilogram` quantity (kgCO2e) per the EPD declared functional unit, the `MeasurementBasis` declaring whether the unit is per-m²/per-m³/per-kg/per-item so the `AssemblyAggregator` GWP fold scales the per-declared-unit GWP by the correct ply quantity (a per-m³ EPD by thickness × area, a per-kg EPD by thickness × area × density read from the same `MaterialPropertySet.Mechanical`). The GWP coerces through the `properties#MATERIAL_PROPERTY` `Admit` SI-base seam exactly as the thermal columns coerce through `ThermalConductivityUnit.WattPerMeterKelvin`, no unit type crossing the interior signature. The `MassConcentration`/`MassConcentrationUnit.KilogramPerCubicMeter` quantity is the carbon-intensity-per-volume working form the `EmbodiedCarbonIntensity` projection composes; both quantity families are catalogued in `.api/api-unitsnet.md`.
- [IFC_PSET_ENVIRONMENTAL]: the `Environmental` case maps to IFC 4.3 `Pset_EnvironmentalImpactValues` (the `Carbon`/`WholeLifeCarbon`/`RecycledContent`/`ExpectedServiceLife` member names), the `Cost` case to the element-level `Pset_ConstructionCosts` (the `SupplyCost`/`InstallationCost`/`LifeCycleCost` columns over the `IfcMonetaryUnit` currency), and the `Classification` case to `IfcClassificationReference` (the `ReferencedSource` naming Uniclass2015/OmniClass and the `Identification` the code). This owner emits the three cases AND the `AssemblyLifecycle`/`AssemblyCost` receipts as the host-neutral portable `MaterialPropertyWire` `[Union]` family (`Environmental`/`Cost`/`Classification` SI scalar rows keyed by `MaterialId`) the `Rasm.Bim` `ConnectionWire.AuthorMaterial` fold reads and authors onto the IFC Psets — the Pset member-name mapping and the `IfcMaterialProperties`/`IfcRelAssociatesClassification` authoring are the `Rasm.Bim` side, the embodied-carbon/cost/classification computation this side, the seam aligning by `MaterialId`, never a `MaterialProperty`/`AssemblyLifecycle` type crossing the boundary. Ripple counterpart: `Rasm.Bim` `[PSET_ENVIRONMENTAL_PROJECTION]` (the realized `Semantics/connection#CONNECTION_WIRE` `ConnectionWire.AuthorMaterial` owner).
- [AEC_SIMULATION_BRIDGE_LCA]: the whole-building embodied-carbon takeoff and the cost-of-construction rollup compute from the same `Construction/assembly#MATERIAL_ASSIGNMENT` plies the realized thermal/acoustic envelope folds, so the construction model feeds the forward `cs:AEC_SIMULATION_BRIDGE` LCA/QTO consumer by `MaterialId` without a second property surface — a `LayerSet` wall reports its U-value, its STC, its embodied carbon, and its cost from one assignment over four `AssemblyAggregator` folds. The bridge consumer reads the `AssemblyLifecycle`/`AssemblyCost` receipt as portable scalar data, never a `MaterialProperty` type crossing the seam. Ripple counterpart: `cs:AEC_SIMULATION_BRIDGE` `[EMBODIED_CARBON_QTO]` (deferred cross-package leg).
