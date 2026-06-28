# [COMPUTE_AGGREGATOR]

THE MULTI-PLY ASSEMBLY-AGGREGATION ENGINE — relocated from `Rasm.Materials` to the Compute analysis rail because layered-construction property aggregation is ANALYSIS, not material authoring (`§6`: the closed-form physics live in Compute, never the seam). One `AssemblyAggregator` static kernel folds a `Rasm.Element` seam `MaterialComposition` (`LayerSet`/`ConstituentSet`/`ProfileSet`) reading each ply's seam `MaterialPropertySet` cases into one receipt per aggregation discipline: a series-resistance U-value (ISO 6946), a mass-law/coincidence layered sound-reduction index (ISO 12354, fed once through the SEAM `StcContourFit` so the assembly STC and the single-material STC share one contour-fit owner), a constituent-`Fraction`-weighted rule-of-mixtures effective density/conductivity (the IFC `IfcMaterialConstituentSet.Fraction` mixture rule), a worst-ply fire envelope, the EN 15978 per-module embodied-carbon sum, and the supply/install cost rollup. An assembly property is COMPUTED from its plies on demand, never stored as a composite material: the `AssemblyProperty`/`AssemblyLifecycle`/`AssemblyCost` receipts are the layered-physics inputs the `Analysis/physics` thermal/acoustic/fire runner and the `Analysis/lifecycle` LCA/cost runner read, never a second material library re-keyed per assembly. The kernel reads the seam vocabulary settled — `MaterialComposition` and `MaterialLayer` from `Rasm.Element/Composition/material`, the `MaterialPropertySet` `[Union]` cases (`Thermal`/`Mechanical`/`Acoustic`/`Fire`/`Environmental`/`Cost`) and the `AcousticBand`/`LifecycleStage` band vocabularies, the `StcContourFit` single-number kernel from `Rasm.Element/Composition/acoustic`, and the kernel `Dimension` layer thickness — and rails an absent ply property onto the one `ComputeFault.AssessmentInputMissing` band rather than defaulting a sentinel, so an under-specified buildup is a typed fault the analysis surfaces, never a silently-wrong envelope.

## [01]-[INDEX]

- [01]-[ASSEMBLY_RECEIPT]: the `AssemblyProperty` thermal/acoustic/mixture/fire receipt, the `AssemblyLifecycle`/`AssemblyCost` embodied-carbon and cost receipts, the `ConstituentWeight`/`PlyQuantity` fold inputs, and the seam-`MaterialPropertySet` ply projections.
- [02]-[AGGREGATION_FOLD]: the `AssemblyAggregator` static kernel — `Aggregate` (series-U + layered-STC + rule-of-mixtures + fire), `AggregateEnvironmental` (EN 15978 GWP), and `AggregateCost`, each a total `Switch` over the seam `MaterialComposition` trichotomy folding the per-ply seam properties.

## [02]-[ASSEMBLY_RECEIPT]

- Owner: `AssemblyProperty` the thermal/acoustic/mixture/fire aggregation receipt; `AssemblyLifecycle`/`AssemblyCost` the embodied-carbon and cost receipts; `ConstituentWeight` the `(NodeId, Fraction)` mixture input the IFC `IfcMaterialConstituentSet.Fraction` supplies; `PlyQuantity` the per-ply declared quantity the GWP/cost folds scale the per-declared-unit value by.
- Cases: one `AssemblyProperty` over a `LayerSet`/`ConstituentSet` — the `UValueWM2K` (ISO 6946 series resistance), the `StcWeighted` (ISO 12354 layered SRI through the seam `StcContourFit`), the `EffectiveDensityKgM3`/`EffectiveConductivityWMK` (rule-of-mixtures), the `FireResistanceMinutes` (the minimum ply rating); one `AssemblyLifecycle` (the `WholeLifeGwpKgCo2e`, the per-module `StageGwp` breakdown, the `EmbodiedCarbonIntensityKgCo2eM2`, the `RecycledContentFraction`); one `AssemblyCost` (the supply/install/lifecycle totals over a single currency) — a new aggregation rule is one fold over the same composition, never a parallel composite-material owner.
- Entry: the receipts are minted by the `[3]-[AGGREGATION_FOLD]` `AssemblyAggregator` folds; the seam-`MaterialPropertySet` ply projections (`ConductivityWmK`/`DensityKgM3`/`SoundReductionIndexDb`/`FireMinutes`/`StageGwp`/`RecycledContent`/`CostOf`) `Choose` the matching seam case off a ply's `Seq<MaterialPropertySet>` and lift its SI scalar, `Option<T>` carrying an absent case the fold rails.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — `MaterialPropertySet`, `AcousticBand`, `LifecycleStage`, `MeasureValue`), BCL inbox (`ReadOnlyMemory<double>`).
- Growth: a new assembly rating is one `AssemblyAggregator` fold reading the same seam `MaterialPropertySet` cases — a thermal-bridge psi-value, a vapor-resistance sum, a thermal-mass capacity each lands as one fold and one receipt column, never a parallel composite owner; a new band is one seam `AcousticBand`/`LifecycleStage` row (the seam vector widens by data, the fold re-reads the new index).
- Boundary: the receipts carry RAW SI scalars (`W·m⁻²·K⁻¹`, dB, `kg·m⁻³`, kgCO2e, monetary), NOT a seam `MeasureValue` or a `MaterialPropertySet` type — the receipt is the analysis input the discipline runners read and the write-back lowers onto `AssessmentFact` typed values, so the aggregator never re-mints the seam value family; the ply projections read the seam `MaterialPropertySet.Thermal.Conductivity`/`Mechanical.Density`/`Acoustic.SoundReductionIndexDb`/`Environmental.StageGwp` members and lift their `MeasureValue.Si` SI scalar so a later seam unit canonicalization does not break the fold; an `AssemblyProperty` is never stored as a material — an assembly's U-value/STC/effective density is computed from its plies on demand, the receipt the analysis input, never a second `MaterialLibrary`-style row table.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ConstituentWeight(NodeId Material, double Fraction);
public readonly record struct PlyQuantity(NodeId Material, double Quantity, MeasurementBasis Basis);

public sealed record AssemblyProperty(
    double UValueWM2K,
    int StcWeighted,
    double EffectiveDensityKgM3,
    double EffectiveConductivityWMK,
    double FireResistanceMinutes);

public sealed record AssemblyLifecycle(
    double WholeLifeGwpKgCo2e,
    ReadOnlyMemory<double> StageGwp,
    double EmbodiedCarbonIntensityKgCo2eM2,
    double RecycledContentFraction);

public sealed record AssemblyCost(Currency Currency, MeasurementBasis Basis, double SupplyTotal, double InstallTotal, double LifecycleTotal) {
    public double TotalInPlace => SupplyTotal + InstallTotal;
}
```

## [03]-[AGGREGATION_FOLD]

- Owner: `AssemblyAggregator` the static fold kernel over a seam `MaterialComposition` — `Aggregate` (the thermal/acoustic/mixture/fire receipt), `AggregateEnvironmental` (the EN 15978 cradle-to-grave GWP receipt), `AggregateCost` (the supply/install/lifecycle receipt); each discriminates the composition trichotomy through the seam's generated total `Switch` and folds the per-ply seam `MaterialPropertySet`.
- Entry: `public static Fin<AssemblyProperty> Aggregate(MaterialComposition composition, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<ConstituentWeight> weights)` plus the rail-twins `AggregateEnvironmental(composition, resolve, quantities, areaM2)` and `AggregateCost(composition, resolve, quantities, areaM2)` — each the one aggregation entry over the SAME `(composition, resolve, …)` rail; a `LayerSet` folds the series-resistance U-value `1/U = Rsi + Σ(t_i/λ_i) + Rse` over the plies' `Thermal` and the layered SRI over the plies' `Acoustic`, a `ConstituentSet` the constituent-`Fraction`-weighted rule-of-mixtures, a `Single`/`ProfileSet` rails (no plies to aggregate), `Fin<T>` aborting on an absent ply property.
- Auto: the resolver `Func<NodeId, Fin<Seq<MaterialPropertySet>>>` reads a ply material node's seam property cases (`id => graph.Material(id).Map(static m => m.Properties)`); the layered-STC fold sums the per-band SRI in series and feeds the resulting per-band vector ONCE through the seam `Acoustics.StcContourFit` so the assembly STC and the single-material STC share one ASTM-E413 contour-fit kernel, never a second algorithm; the surface-film resistances `Rsi`/`Rse` (0.13 / 0.04 m²K/W, ISO 6946 interior/exterior) add once at the envelope ends so `UValueWM2K` is the reciprocal of the total resistance.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialPropertySet`, `AcousticBand`, `LifecycleStage`, `Acoustics.StcContourFit`, `NodeId`), Rasm (project — `Dimension`), BCL inbox.
- Growth: a new aggregation discipline is one `AssemblyAggregator` fold over the same composition reading the same seam cases; the generated total `Switch` over the closed composition trichotomy breaks at compile time if the seam adds a composition case, never a runtime-silent `_` arm.
- Boundary: the kernel reads the seam `MaterialComposition` and `MaterialPropertySet` and NEVER the retired `Rasm.Materials` `MaterialAssignment`/`MaterialProperty` (those owners are retired — this engine relocated here whole); the series-resistance fold reads each `LayerSet` ply's `MaterialLayer.Thickness` `Dimension` and the ply material's `Thermal.Conductivity.Si`, the layered-STC fold each ply's `Acoustic.SoundReductionIndexDb` summed in series and contour-fit through the seam kernel, the rule-of-mixtures fold the `ConstituentWeight.Fraction` set (a fraction set that does not sum to one within tolerance rails `AssessmentInputMissing`), the GWP fold each ply's `Environmental.StageGwp` scaled by the per-ply quantity (a per-m³ EPD by thickness × area, a per-kg EPD by thickness × area × `Mechanical.Density.Si`), the cost fold each ply's `Cost.Supply`/`Install` × quantity with a currency mismatch railing (the rollup cannot sum across currencies without an exchange rate this owner does not carry); an absent ply property (a layer whose material lacks the `Thermal`/`Acoustic`/`Environmental`/`Cost` case the fold reads) rails `ComputeFault.AssessmentInputMissing` rather than defaulting to a sentinel, so an under-specified buildup is a typed fault the analysis surfaces; the `Single`/`ProfileSet` arm rails the same typed fault explicitly (a single material or a section profile carries no plies to aggregate).

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssemblyAggregator {
    const double RsiWM2K = 0.13;
    const double RseWM2K = 0.04;
    const double FractionToleranceUnit = 1e-3;

    public static Fin<AssemblyProperty> Aggregate(MaterialComposition composition, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<ConstituentWeight> weights) =>
        composition.Switch(
            single:         static _   => Fin.Fail<AssemblyProperty>(Missing("<assembly-aggregation-requires-layer-or-constituent-set>")),
            layerSet:       set => AggregateLayers(set, resolve),
            profileSet:     static _   => Fin.Fail<AssemblyProperty>(Missing("<assembly-aggregation-requires-layer-or-constituent-set>")),
            constituentSet: set => AggregateConstituents(set, resolve, weights));

    static Fin<AssemblyProperty> AggregateLayers(MaterialComposition.LayerSet set, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Layers.Fold(
            Fin.Succ((Resistance: RsiWM2K + RseWM2K, Sri: new double[AcousticBand.Count], MassKgM2: 0.0, ThicknessM: 0.0, MinFire: double.MaxValue)),
            (acc, layer) => acc.Bind(state => resolve(layer.Material).Bind(props =>
                from conductivity in ConductivityWmK(props).ToFin(Missing($"<assembly-layer-missing-thermal:{layer.Material.Value}>"))
                from sri in SoundReductionIndexDb(props).ToFin(Missing($"<assembly-layer-missing-acoustic:{layer.Material.Value}>"))
                from density in DensityKgM3(props).ToFin(Missing($"<assembly-layer-missing-mechanical:{layer.Material.Value}>"))
                from fire in FireMinutes(props).ToFin(Missing($"<assembly-layer-missing-fire:{layer.Material.Value}>"))
                let thicknessM = layer.Thickness.Meters
                select state with {
                    Resistance = state.Resistance + thicknessM / Math.Max(conductivity, double.Epsilon),
                    Sri = AddBands(state.Sri, sri),
                    MassKgM2 = state.MassKgM2 + density * thicknessM,
                    ThicknessM = state.ThicknessM + thicknessM,
                    MinFire = Math.Min(state.MinFire, fire) })))
            .Map(static state => new AssemblyProperty(
                UValueWM2K: 1.0 / state.Resistance,
                StcWeighted: Acoustics.StcContourFit(state.Sri.AsMemory()),
                EffectiveDensityKgM3: state.ThicknessM > 0.0 ? state.MassKgM2 / state.ThicknessM : 0.0,
                EffectiveConductivityWMK: state.Resistance > RsiWM2K + RseWM2K ? state.ThicknessM / (state.Resistance - RsiWM2K - RseWM2K) : 0.0,
                FireResistanceMinutes: state.MinFire is double.MaxValue ? 0.0 : state.MinFire));

    static Fin<AssemblyProperty> AggregateConstituents(MaterialComposition.ConstituentSet set, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<ConstituentWeight> weights) =>
        guard(Math.Abs(weights.Sum(static w => w.Fraction) - 1.0) <= FractionToleranceUnit, Missing($"<constituent-fraction-not-normalized:{weights.Sum(static w => w.Fraction):R}>"))
            .Bind(_ => weights.Fold(
                Fin.Succ((Density: 0.0, Conductivity: 0.0, MinFire: double.MaxValue)),
                (acc, w) => acc.Bind(state => resolve(w.Material).Bind(props =>
                    from density in DensityKgM3(props).ToFin(Missing($"<constituent-missing-mechanical:{w.Material.Value}>"))
                    from conductivity in ConductivityWmK(props).ToFin(Missing($"<constituent-missing-thermal:{w.Material.Value}>"))
                    select state with {
                        Density = state.Density + w.Fraction * density,
                        Conductivity = state.Conductivity + w.Fraction * conductivity,
                        MinFire = Math.Min(state.MinFire, FireMinutes(props).IfNone(0.0)) }))))
            .Map(static state => new AssemblyProperty(0.0, 0, state.Density, state.Conductivity, state.MinFire is double.MaxValue ? 0.0 : state.MinFire));

    public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialComposition composition, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> quantities, double areaM2) =>
        composition.Switch(
            single:         static _ => MissingLifecycle("<environmental-aggregation-requires-layer-or-constituent-set>"),
            profileSet:     static _ => MissingLifecycle("<environmental-aggregation-requires-layer-or-constituent-set>"),
            constituentSet: set => FoldGwp(set.Constituents.Map(c => (c.Material, Qty: PlyQty(quantities, c.Material, c.Fraction))), resolve, areaM2),
            layerSet:       set => FoldGwp(set.Layers.Map(l => (l.Material, Qty: PlyQty(quantities, l.Material, l.Thickness.Meters * areaM2))), resolve, areaM2));

    static Fin<AssemblyLifecycle> FoldGwp(Seq<(NodeId Material, double Qty)> plies, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve, double areaM2) =>
        plies.Fold(
            Fin.Succ((Stage: new double[LifecycleStage.Count], Recycled: 0.0, Weight: 0.0)),
            (acc, ply) => acc.Bind(state => resolve(ply.Material).Bind(props =>
                from stage in StageGwp(props).ToFin(Missing($"<assembly-ply-missing-environmental:{ply.Material.Value}>"))
                select (Stage: AddScaled(state.Stage, stage, ply.Qty), Recycled: state.Recycled + RecycledContent(props).IfNone(0.0) * ply.Qty, Weight: state.Weight + ply.Qty))))
            .Map(state => new AssemblyLifecycle(
                WholeLifeGwpKgCo2e: state.Stage.Sum(),
                StageGwp: state.Stage.AsMemory(),
                EmbodiedCarbonIntensityKgCo2eM2: areaM2 > 0.0 ? state.Stage.Sum() / areaM2 : 0.0,
                RecycledContentFraction: state.Weight > 0.0 ? state.Recycled / state.Weight : 0.0));

    public static Fin<AssemblyCost> AggregateCost(MaterialComposition composition, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> quantities, double areaM2) =>
        composition.Switch(
            single:         set => AccumulateCost(Seq1((set.Material, Qty: PlyQty(quantities, set.Material, 1.0))), resolve),
            profileSet:     set => AccumulateCost(Seq1((set.Material, Qty: PlyQty(quantities, set.Material, 1.0))), resolve),
            constituentSet: set => AccumulateCost(set.Constituents.Map(c => (c.Material, Qty: PlyQty(quantities, c.Material, c.Fraction))), resolve),
            layerSet:       set => AccumulateCost(set.Layers.Map(l => (l.Material, Qty: PlyQty(quantities, l.Material, l.Thickness.Meters * areaM2))), resolve));

    static Fin<AssemblyCost> AccumulateCost(Seq<(NodeId Material, double Qty)> plies, Func<NodeId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        plies.Fold(
            Fin.Succ(Option<AssemblyCost>.None),
            (acc, ply) => acc.Bind(running => resolve(ply.Material).Bind(props =>
                from cost in CostOf(props).ToFin(Missing($"<assembly-missing-cost:{ply.Material.Value}>"))
                from merged in running.Match(
                    Some: r => r.Currency == cost.Currency
                        ? Fin.Succ(r with { SupplyTotal = r.SupplyTotal + cost.Supply * ply.Qty, InstallTotal = r.InstallTotal + cost.Install * ply.Qty, LifecycleTotal = r.LifecycleTotal + cost.Lifecycle * ply.Qty })
                        : Fin.Fail<AssemblyCost>(Missing($"<cost-currency-mismatch:{r.Currency.Key}<>{cost.Currency.Key}>")),
                    None: () => Fin.Succ(new AssemblyCost(cost.Currency, cost.Basis, cost.Supply * ply.Qty, cost.Install * ply.Qty, cost.Lifecycle * ply.Qty)))
                select Some(merged))))
            .Bind(static o => o.ToFin(Missing("<assembly-cost-empty>")));

    static ComputeFault Missing(string detail) => new ComputeFault.AssessmentInputMissing(detail);
    static Fin<AssemblyLifecycle> MissingLifecycle(string detail) => Fin.Fail<AssemblyLifecycle>(Missing(detail));

    static Option<double> ConductivityWmK(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Thermal t ? Some(t.Conductivity.Si) : None).HeadOrNone();
    static Option<double> DensityKgM3(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Mechanical m ? Some(m.Density.Si) : None).HeadOrNone();
    static Option<ReadOnlyMemory<double>> SoundReductionIndexDb(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Acoustic a ? Some(a.SoundReductionIndexDb) : None).HeadOrNone();
    static Option<double> FireMinutes(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Fire f ? Some(f.ResistanceMinutes) : None).HeadOrNone();
    static Option<ReadOnlyMemory<double>> StageGwp(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Environmental e ? Some(e.StageGwp) : None).HeadOrNone();
    static Option<double> RecycledContent(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Environmental e ? Some(e.RecycledContent.Value) : None).HeadOrNone();
    static Option<MaterialPropertySet.Cost> CostOf(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Cost c ? Some(c) : None).HeadOrNone();
    static double PlyQty(Seq<PlyQuantity> quantities, NodeId id, double geometric) => quantities.Find(q => q.Material == id).Map(static q => q.Quantity).IfNone(geometric);

    static double[] AddBands(double[] accumulated, ReadOnlyMemory<double> sri) {
        ReadOnlySpan<double> bands = sri.Span;
        double[] next = new double[AcousticBand.Count];
        for (int i = 0; i < AcousticBand.Count; i++) { next[i] = accumulated[i] + bands[i]; }
        return next;
    }

    static double[] AddScaled(double[] accumulated, ReadOnlyMemory<double> stage, double scale) {
        ReadOnlySpan<double> bands = stage.Span;
        double[] next = new double[LifecycleStage.Count];
        for (int i = 0; i < LifecycleStage.Count; i++) { next[i] = accumulated[i] + bands[i] * scale; }
        return next;
    }
}
```

## [04]-[RESEARCH]

- [ISO_6946_SERIES_U]: the `LayerSet` U-value is `1/U = Rsi + Σ(t_i/λ_i) + Rse` (ISO 6946:2017) over the plies' `MaterialLayer.Thickness` `Dimension` and the ply material's seam `MaterialPropertySet.Thermal.Conductivity.Si`, with the interior/exterior surface-film resistances `Rsi = 0.13` / `Rse = 0.04 m²K/W` added once at the envelope ends; the effective through-thickness conductivity is `ΣT / (R − Rsi − Rse)`. The fold reads the seam thermal case directly — the retired `Rasm.Materials` `AssemblyAggregator.AggregateLayers` is superseded whole by this relocation. Ripple counterpart: `Rasm.Materials` `Properties/properties#ASSEMBLY_PROPERTY` (RETIRE) and `Rasm.Element/Composition/material` (the seam `MaterialComposition`/`MaterialPropertySet` owner).
- [ISO_12354_LAYERED_STC]: the layered sound-reduction index is the ISO 12354-1 simplified composite where each leaf's transmission multiplies, so the per-band SRI adds in dB across the plies; the resulting per-band vector feeds ONCE through the SEAM `Acoustics.StcContourFit` (the ASTM-E413 single-number contour fit) so the assembly STC and the single-material `MaterialPropertySet.Acoustic.StcWeighted` share one contour-fit owner, never a second algorithm. The single-material PURE folds (`Nrc`/`Saa`/`StcWeighted`) are SEAM-owned on the `Acoustic` case (`§4B`); the multi-ply aggregation is this relocated engine. Ripple counterpart: `Rasm.Element/Composition/acoustic` (the seam `StcContourFit` + intrinsic acoustic folds owner).
- [RULE_OF_MIXTURES]: a composite material's effective conductivity/density is the constituent-`Fraction`-weighted sum of its `ConstituentSet` member properties (the IFC `IfcMaterialConstituentSet.Fraction` mixture rule), an immutable fold over the seam `ConstituentSet` reading each constituent's `Mechanical`/`Thermal` case, a fraction set not summing to one within `1e-3` railing the typed fault; the `ConstituentWeight` `Fraction` rides the fold input until the seam carries a `Fraction` column on its constituent.
- [EN_15978_EMBODIED_CARBON]: the assembly embodied carbon sums each ply's seam `MaterialPropertySet.Environmental.StageGwp` (the per-module A1-A3/A4/A5/B/C/D vector over the seam `LifecycleStage` band) scaled by the per-ply declared quantity — a per-m³ EPD by thickness × area, a per-m² EPD by area, a per-kg EPD by thickness × area × the ply `Mechanical.Density.Si` — so the assembly `StageGwp` is the per-module ply sum and `WholeLifeGwpKgCo2e` its all-module total; the `EmbodiedCarbonIntensityKgCo2eM2` divides by the assembly area. The `Analysis/lifecycle` LCA runner reads this receipt and resolves a missing ply EPD from the EC3 service. Ripple counterpart: `Rasm.Materials` `Properties/sustainability#LIFECYCLE_AGGREGATION` (RETIRE).
- [COST_ROLLUP]: the supply/install/lifecycle cost rollup sums each ply's seam `MaterialPropertySet.Cost.Supply`/`Install`/`Lifecycle` × the per-ply quantity over a single `Currency`, a currency mismatch railing because the rollup carries no exchange rate; the cost rail spans all composition cases (a `Single`/`ProfileSet` member has a unit supply/install cost) where the GWP/thermal rails require plies. Construction SCHEDULING and 4D cost-loading stay in `Rasm.Bim` (MPXJ); this is the embodied material-cost takeoff only. Ripple counterpart: `Rasm.Materials` `Properties/sustainability#LIFECYCLE_AGGREGATION` `AggregateCost` (RETIRE).
- [RELOCATION_SEAM]: the engine moved from `Rasm.Materials` to `Rasm.Compute` whole — Materials RETIRES its `AssemblyAggregator`/`AssemblyProperty`/`AssemblyLifecycle`/`AssemblyCost`/`ConstituentWeight`/`PlyQuantity` owners and projects only the single-material subgraph (the per-material `MaterialPropertySet` cases onto the seam `Material` node); the multi-ply aggregation reads the seam composition from the analysis side. The kernel reads the seam vocabulary (`MaterialComposition`, `MaterialLayer.Thickness`, `MaterialPropertySet`, `AcousticBand`, `LifecycleStage`, `Acoustics.StcContourFit`) and never the retired Materials owners. Ripple counterpart: `Rasm.Materials` `Construction/assembly` + `Properties/properties` + `Properties/sustainability` (RETIRE the aggregator half; KEEP the single-material property authoring lowered onto the seam).
