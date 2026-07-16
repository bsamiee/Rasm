# [COMPUTE_AGGREGATOR]

Multi-ply assembly-aggregation engine, relocated from `Rasm.Materials` to the Compute analysis rail because layered-construction property aggregation is analysis, not material authoring — the closed-form physics live in Compute, never the seam. One `AssemblyAggregator` static kernel folds a `Rasm.Element` seam `MaterialComposition` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) into one receipt per aggregation discipline, resolving each ply's `MaterialPropertySet` cases through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` keyed on the composition's native `MaterialId`, never a graph `NodeId`. Over a `LayerSet` it folds the series-resistance U (ISO 6946), the `Σ ρ·c·t` thermal mass (ISO 13786), the `Σ μ·t` vapour `Sd` (EN ISO 13788), the field-incidence mass-law STC over accumulated areal mass `m' = Σ(ρ·t)`, and the in-plane Voigt effective modulus `Σ(t·E)/ΣT`; over a `ConstituentSet`, the `Fraction`-weighted rule-of-mixtures density/conductivity/modulus (read off `MaterialConstituent.Fraction`); worst-ply fire over either; EN 15978 embodied carbon and basis-aware supply/install/lifecycle cost over any composition; and the EN ISO 10077-1 whole-window `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` over glazed/frame fields — the side-by-side glazing-in-frame composition, with its edge-seal bridge `Ψg` and frame fraction, the perpendicular layer series structurally cannot reach. An assembly property is computed from its plies on demand, never stored as a composite material: the `AssemblyProperty`/`AssemblyLifecycle`/`AssemblyCost`/`WindowU` receipts are the layered-physics inputs the `Analysis/physics` and `Analysis/lifecycle` runners read, never a second material library re-keyed per assembly.

Kernel reads the seam vocabulary settled — `MaterialComposition`/`MaterialLayer`/`MaterialConstituent`, the `MaterialPropertySet` `[Union]` cases through the `MaterialPropertyAccess` accessors (`props.Thermal`/`.Mechanical`/`.Fire`/`.Environmental`/`.Cost`), the `AcousticBand`/`LifecycleStage` band vocabularies, the seam `RatingContour.Stc.Fit` single-number contour kernel, the `MeasurementBasis`/`Currency` cost axes, and the `MaterialLayer.Thickness.Si` SI thickness. A missing ply material node (and a missing single-discipline GWP or cost input) rails the one `ComputeFault.AssessmentInputMissing` band, while a multi-physics field whose discipline a ply lacks projects a per-discipline `double.NaN` (the `IsFinite` not-applicable signal `AssessmentVerdict.FromRatio` bands): a thermal U is never coupled to a ply missing its `Mechanical` density, so an under-specified buildup is a typed not-applicable the analysis surfaces, never a silently-wrong envelope. Window fields are the runner's — glazed `Ug` and frame `Uf` off each part's `Thermal.UValue.Si`, spacer `Ψg` off the window's `Pset`, areas off the baked `Qto_*BaseQuantities` — so the kernel folds already-resolved fields and never reads `Rasm.Materials`, the seam material plus the baked bags its only ingress.

## [01]-[INDEX]

- [01]-[ASSEMBLY_RECEIPT]: the `AssemblyProperty`/`AssemblyLifecycle`/`AssemblyCost`/`WindowU` receipts plus the `ElementQuantity`/`PlyQuantity` takeoff inputs the GWP/cost folds distribute per ply.
- [02]-[AGGREGATION_FOLD]: the `AssemblyAggregator` static kernel — `Aggregate` (series-U/mass/vapour/STC/mixture/fire), `AggregateEnvironmental` (EN 15978 GWP), `AggregateCost` (basis-aware cost), `AggregateWindow` (EN ISO 10077-1 whole-window `Uw`) — each folding per-ply/per-field seam properties into a typed `[FOLD_STATE]` accumulator.

## [02]-[ASSEMBLY_RECEIPT]

- Owner: `AssemblyProperty` the thermal/mass/vapour/acoustic/mixture/fire receipt; `AssemblyLifecycle`/`AssemblyCost` the EN 15978 embodied-carbon and in-place-cost receipts; `WindowU` the EN ISO 10077-1 whole-window receipt, `WindowField` the resolved glazed-or-frame field the thermal runner assembles and feeds the fold; `ElementQuantity` the element geometric takeoff (`AreaM2` + `VolumeM3`, plus the `Rasm.Fabrication` off-cut `WasteAreaM2`) the GWP/cost folds distribute per ply; `PlyQuantity` the optional per-`MaterialId` exact declared-quantity override an IFC `Qto_*BaseQuantities` takeoff supplies.
- Cases: one `AssemblyProperty` over a `LayerSet` carrying the ISO 6946 series U (with the `Rsi`/`Rse` films), the ISO 13786 areal heat capacity, the EN ISO 13788 vapour `Sd`, the mass-law `StcWeighted` (contour-fit through the seam `RatingContour.Stc.Fit`), the effective bulk density/conductivity/in-plane modulus, and the worst-ply fire; one `AssemblyLifecycle` (whole-life + per-module `StageGwp`, intensity, mass-weighted recycled fraction); one `AssemblyCost` (supply/install/lifecycle over one `Currency`); one `WindowU` over a `Seq<WindowField>` (the `Uw`, area-weighted glazed/frame sub-transmittances, the `Σ lg·Ψg` edge bridge, the `GlazedFraction` the daylight/solar-gain consumer reads) — a new aggregation rating is one fold over the same composition or window fields plus one receipt column, never a parallel composite-material owner.
- Entry: the receipts mint through the `[03]-[AGGREGATION_FOLD]` folds; per-ply reads compose the seam `MaterialPropertyAccess` accessors directly (`props.Thermal`/`.Mechanical`/`.Fire`/`.Environmental`/`.Cost`) — the seam exposing the full typed accessor family so every discipline reads seam-direct (ONE_HOP), an `Option<T>` absent case the fold rails, never an `is`-cast the seam owns.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`), Thinktecture.Runtime.Extensions (the generated `MaterialComposition.Switch` + `MeasurementBasis.Switch`), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `MaterialPropertySet`, `MaterialPropertyAccess`, `MaterialId`, `AcousticBand`, `LifecycleStage`, `Currency`, `MeasurementBasis`, `RatingContour.Stc.Fit`), BCL inbox (`ImmutableArray<double>` the `StageGwp` carrier, `ReadOnlySpan<double>` the fold transient).
- Growth: a new assembly rating is one `AssemblyAggregator` fold reading the same seam `MaterialPropertySet` cases into one receipt column — a thermal-bridge psi, a decrement factor, a flanking term each one fold, never a parallel composite owner; a new band is one seam `AcousticBand`/`LifecycleStage` row (the vector widens by data, the fold re-reads the new index).
- Boundary: receipts carry raw SI scalars, not a seam `MeasureValue` or `MaterialPropertySet` type — the receipt is the analysis input the runners read and the write-back lowers onto `AssessmentFact` values, so the aggregator never re-mints the seam value family; each ply read lifts the member's `MeasureValue.Si` (`Thermal.Conductivity.Si`/`Mechanical.Density.Si`) so a later seam unit canonicalization never breaks the fold; `AssemblyCost` carries no `MeasurementBasis` (the per-unit basis is consumed at the fold, the total absolute currency); `WindowField` likewise carries raw SI scalars the thermal runner lifts from the seam — glazed/frame `U` off `Thermal.UValue.Si`, areas off `Qto_*BaseQuantities`, the spacer `Ψg` off the window's `Pset` thermal-bridge property (NOT `Thermal`, which carries no perimeter-bridge column) — the kernel folds already-resolved fields, never reading `Rasm.Materials`; an assembly property or `Uw` is computed on demand, never stored as a material.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Element geometric takeoff the GWP/cost folds distribute per ply, read once from the element's baked Qto_*BaseQuantities.
// WasteAreaM2 is the Rasm.Fabrication Nesting/stock decode-side ingress — the seam NestYield.WasteAreaMm2 (SI-coerced at the
// lifecycle lane) joins the material basis so off-cut waste rolls into the same per-ply GWP/cost fold, a data column not a new discipline.
public readonly record struct ElementQuantity(double AreaM2, double VolumeM3, double WasteAreaM2 = 0.0) {
    public static readonly ElementQuantity Zero = new(0.0, 0.0);

    public double EffectiveAreaM2 => AreaM2 + WasteAreaM2;
}

// Optional exact per-material quantity (an IFC Qto_*BaseQuantities takeoff) overriding the idealized geometry, already in the
// declared unit; keyed by MaterialId so a buildup and its constituent share one composition key, never a graph NodeId.
public readonly record struct PlyQuantity(MaterialId Material, double DeclaredQuantity);

public sealed record AssemblyProperty(
    double UValueWM2K,
    int StcWeighted,
    double EffectiveDensityKgM3,
    double EffectiveConductivityWMK,
    double EffectiveYoungsModulusPa,
    double FireResistanceMinutes,
    double ArealHeatCapacityKJM2K,
    double VapourResistanceSdM);

public sealed record AssemblyLifecycle(
    double WholeLifeGwpKgCo2e,
    ImmutableArray<double> StageGwp,
    double EmbodiedCarbonIntensityKgCo2eM2,
    double RecycledContentFraction);

public sealed record AssemblyCost(Currency Currency, double SupplyTotal, double InstallTotal, double LifecycleTotal) {
    public double TotalInPlace => SupplyTotal + InstallTotal;
}

// One closed field family preserves the glazed/frame discriminant even when a valid glazed field has zero exposed edge.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowField {
    private WindowField() { }

    public sealed record Glazed(double UWM2K, double AreaM2, double EdgeLengthM, double PsiWM_K) : WindowField;
    public sealed record Frame(double UWM2K, double AreaM2) : WindowField;
}

// EN ISO 10077-1 whole-window receipt: the area-and-perimeter-weighted Uw a single material cannot carry (a window is
// side-by-side glazing-in-frame, not a through-thickness series), plus the glazed/frame sub-transmittances, the edge bridge, and glazed fraction.
public sealed record WindowU(double UwWM2K, double UgWM2K, double UfWM2K, double EdgeBridgeW_K, double GlazedFraction);
```

## [03]-[AGGREGATION_FOLD]

- Owner: `AssemblyAggregator` the static fold kernel over a seam `MaterialComposition` — `Aggregate` (thermal/mass/vapour/acoustic/mixture/fire), `AggregateEnvironmental` (EN 15978 GWP), `AggregateCost` (basis-aware cost), `AggregateWindow` (EN ISO 10077-1 whole-window `Uw` over a `Seq<WindowField>`); each composition fold discriminates through the seam's generated total `Switch` (state threaded positionally) and folds each ply's `MaterialPropertySet`, resolved through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>`, into a typed `[FOLD_STATE]` accumulator (`LayerFold`/`MixtureFold`/`CarbonFold`) that `Absorb`s one ply and `Project`s the receipt; `AggregateWindow` folds the runner-resolved fields through `WindowFold` — the side-by-side glazing-in-frame window, not a through-thickness series, folds a field set rather than re-discriminating the union.
- Entry: `Aggregate(composition, resolve)` over a `LayerSet`/`ConstituentSet` (a `Single`/`ProfileSet` rails — no series structure to aggregate, its intrinsic U/STC read seam-direct), plus `AggregateEnvironmental(composition, resolve, overrides, geometry)` and `AggregateCost(…)` over any composition; a `LayerSet` folds the series-U `1/U = Rsi + Σ(t_i/λ_i) + Rse`, the areal heat capacity, the vapour `Sd`, the accumulated areal mass the mass-law STC reads, and worst-ply fire, a `ConstituentSet` the `Fraction`-weighted rule-of-mixtures, `Fin` aborting ONLY on a missing ply material node while a present-but-discipline-missing ply projects `double.NaN` decoupled per discipline (a thermal U survives a layer missing its mechanical density). `AggregateWindow(fields)` folds a `Seq<WindowField>` into the `WindowU`, railing `ComputeFault.AssessmentInputMissing` on an empty field set or a zero total area (never a `0/0`-`NaN` `Uw` the verdict bands as if checked); the runner resolves each field, this kernel folds them, never reading a graph node or a `Rasm.Materials` type.
- Auto: the resolver reads a ply node's seam property cases keyed on `MaterialId`, not a graph `NodeId`, so the fold reads the composition's own plies; the mass-law STC accumulates each layer's areal mass, then `MassLawBands` evaluates `R(f)=20·log₁₀(m'·f)−47` at each seam `AcousticBand` centre and feeds the vector ONCE through the seam `RatingContour.Stc.Fit` so the assembly STC and the single-material STC share one ASTM-E413 contour-fit owner; the `Rsi`/`Rse` films (0.13 / 0.04 m²K/W, ISO 6946) seed the resistance at the envelope ends; the GWP and cost folds share ONE basis-aware `DeclaredQuantity` derivation off the seam `Environmental`/`Cost` `Basis` (per-m³ volume, per-m² area, per-kg `volume × Mechanical.Density.Si`, per-item unit, the per-ply volume `PliesByVolume` distributes) unless a `PlyQuantity` overrides, and the recycled-content fraction weights each ply by its mass `ρ·V`, excluding a density-less ply.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `MaterialPropertySet`, `MaterialPropertyAccess`, `MaterialId`, `AcousticBand`, `LifecycleStage`, `Currency`, `MeasurementBasis`, `RatingContour.Stc.Fit`), BCL inbox.
- Growth: a new aggregation discipline is one `AssemblyAggregator` fold over the same composition reading the same seam cases into a typed `[FOLD_STATE]` accumulator; the generated total `Switch` over the closed composition family breaks at compile time if the seam adds a composition case, never a runtime-silent `_` arm.
- Boundary: the kernel reads the seam `MaterialComposition`/`MaterialPropertySet` and NEVER the retired `Rasm.Materials` `MaterialAssignment`/`MaterialProperty`/`ConstituentWeight` — the constituent fraction rides the seam `MaterialConstituent.Fraction` (normalized to unity at seam admission, the aggregator never re-guarding), the resolver keyed on `MaterialId` so a `NodeId`-keyed lookup and the migration `Aggregate(composition, resolve, Seq<ConstituentWeight>)` 3-arg form are the deleted shapes (2-arg canonical); an absent ply property in the multi-physics `Aggregate` is DECOUPLED per discipline — a missing `Thermal` NaNs only `U`/`Sd`/heat-capacity, a missing `Mechanical` only the mass/stiffness-derived fields — projecting `double.NaN`, and ONLY a missing material NODE rails; the single-discipline `AggregateEnvironmental`/`AggregateCost` DO rail on a missing `Environmental`/`Cost` case (a hole invalidates the sum) and on a per-kg basis over a density-less material, while an absent FIRE rating is non-limiting (`IfNone(double.MaxValue)`, an all-absent set yielding `0`); cost is over one `Currency` (a mismatch rails, the fold carrying no exchange rate); the mass-law STC contour-fits through the seam kernel (the naive per-leaf dB sum, which over-predicts a rigidly-bonded set, the deleted form), a homogeneous mix's series `U`/heat-capacity/`Sd` and thickness-less STC are `double.NaN`/`0`; a `Single`/`ProfileSet` rails explicitly (no plies to aggregate), and an EMPTY `LayerSet`/`ConstituentSet` rails the same fault — the vacuous fold seed would otherwise project a films-only `U` or zero-mixture receipt with every completeness flag true. `AggregateWindow` is the EN ISO 10077-1 side-by-side fold: it area-weights the glazed `Ug` (EN 673 center-of-glass off `Thermal.UValue`, never folded raw as a single-material whole-window U — the deleted form) and frame `Uf` and ADDS the `Σ lg·Ψg` edge-seal bridge the layer series cannot reach, the spacer `Ψg` off the window's `Pset` (NOT `Thermal`, which carries no perimeter-bridge column), a frame field zeroing its edge term; the kernel folds the runner-resolved fields pure, never reading a graph node.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssemblyAggregator {
    private const double RsiWM2K = 0.13;            // ISO 6946 interior surface film
    private const double RseWM2K = 0.04;            // ISO 6946 exterior surface film
    private const double MassLawConstantDb = 47.0;  // field-incidence mass law: R(f) = 20·log10(m'·f) − 47 dB

    // Series-U + areal heat capacity + vapour Sd + mass-law STC + in-plane Voigt modulus over a LayerSet, Fraction-weighted
    // rule-of-mixtures over a ConstituentSet, worst-ply fire over either; a Single/ProfileSet rails (no plies). State threads positionally.
    public static Fin<AssemblyProperty> Aggregate(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        composition.Switch(
            resolve,
            single:         static (_, _)   => Fin.Fail<AssemblyProperty>(Missing("<aggregation-requires-layer-or-constituent-set>")),
            profileSet:     static (_, _)   => Fin.Fail<AssemblyProperty>(Missing("<aggregation-requires-layer-or-constituent-set>")),
            layerSet:       static (r, set) => AggregateLayers(set, r),
            constituentSet: static (r, set) => AggregateConstituents(set, r));

    // EN 15978 embodied carbon over ANY composition: each ply's per-module StageGwp scaled by the BASIS-matching quantity
    // through the same DeclaredQuantity derivation the cost fold uses (Environmental.Basis per-m³/per-m²/per-kg/per-item, a
    // PlyQuantity override the exact quantity), so a per-m² membrane or per-kg steel EPD folds without a forced per-m³ normalization;
    // recycled content weights by ply mass ρ·V (a density-less ply excluded). A missing Environmental case or a per-kg density-less ply rails.
    public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementQuantity geometry) =>
        // Vacuous-composition admission shared with the cost fold: an empty LayerSet/ConstituentSet yields zero
        // plies and would map the untouched Seed onto a fabricated zero-GWP lifecycle — absent plies rail
        // AssessmentInputMissing, and only a NON-EMPTY zero-impact composition can honestly report zero.
        PliesByVolume(composition, geometry) is var plies && plies.IsEmpty
            ? Fin.Fail<AssemblyLifecycle>(Missing("<assembly-environmental-empty>"))
            : plies.Fold(
            Fin.Succ(CarbonFold.Seed),
            (acc, ply) => acc.Bind(state => resolve(ply.Material).Bind(props =>
                from env in props.Environmental.ToFin(Missing($"<assembly-ply-missing-environmental:{ply.Material.Value}>"))
                from gwpQuantity in PlyOverride(overrides, ply.Material).Match(
                    Some: static q => Fin.Succ(q),
                    None: () => DeclaredQuantity(env.Basis, ply.VolumeM3, geometry.EffectiveAreaM2, DensityKgM3(props), ply.Material))
                let plyMass = DensityKgM3(props).Map(d => d * ply.VolumeM3)
                select state with {
                    Stage = AddScaled(state.Stage, env.StageGwp, gwpQuantity),
                    RecycledMass = state.RecycledMass + plyMass.Map(m => env.RecycledContent * m).IfNone(0.0),
                    TotalMass = state.TotalMass + plyMass.IfNone(0.0) })))
            .Map(state => new AssemblyLifecycle(
                WholeLifeGwpKgCo2e: state.WholeLife,
                StageGwp: [.. state.Stage],
                EmbodiedCarbonIntensityKgCo2eM2: geometry.AreaM2 > 0.0 ? state.WholeLife / geometry.AreaM2 : 0.0,
                RecycledContentFraction: state.TotalMass > 0.0 ? state.RecycledMass / state.TotalMass : 0.0));

    // Basis-aware supply/install/lifecycle rollup over ANY composition + a single Currency: the Cost.Basis selects the
    // geometric quantity the per-unit price scales by; a currency mismatch, a per-kg density-less ply, or a missing Cost rails.
    public static Fin<AssemblyCost> AggregateCost(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementQuantity geometry) =>
        PliesByVolume(composition, geometry).Fold(
            Fin.Succ(Option<AssemblyCost>.None),
            (acc, ply) => acc.Bind(running => resolve(ply.Material).Bind(props =>
                from cost in props.Cost.ToFin(Missing($"<assembly-missing-cost:{ply.Material.Value}>"))
                from qty in PlyOverride(overrides, ply.Material).Match(
                    Some: static q => Fin.Succ(q),
                    None: () => DeclaredQuantity(cost.Basis, ply.VolumeM3, geometry.EffectiveAreaM2, DensityKgM3(props), ply.Material))
                from merged in Accumulate(running, cost, qty)
                select Some(merged))))
            .Bind(static o => o.ToFin(Missing("<assembly-cost-empty>")));

    // EN ISO 10077-1 whole-window Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af) over the runner-resolved glazed/frame
    // fields — the side-by-side composition the through-thickness layer series cannot reach. An empty field set or a zero
    // total area rails the typed fault, never a 0/0-NaN Uw the verdict would band NotApplicable as if checked.
    public static Fin<WindowU> AggregateWindow(Seq<WindowField> fields) =>
        fields.IsEmpty
            ? Fin.Fail<WindowU>(Missing("<window-no-glazed-or-frame-field>"))
            : fields.Fold(WindowFold.Seed, static (state, field) => state.Absorb(field)).Project();

    // A LayerSet folds each discipline INDEPENDENTLY: a layer with no Thermal NaNs only U/Sd, no Mechanical density only the
    // mass-derived fields — never aborting the U a thermal runner reads. Only a missing material NODE rails; a missing property is
    // typed not-applicable. An EMPTY layer set rails: zero plies is no series, and the vacuous Seed would otherwise project a
    // films-only U = 1/(Rsi+Rse) receipt with every completeness flag true — a fabricated envelope, the AggregateWindow empty-rail sibling.
    private static Fin<AssemblyProperty> AggregateLayers(MaterialComposition.LayerSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Layers.IsEmpty
            ? Fin.Fail<AssemblyProperty>(Missing("<assembly-empty-layer-set>"))
            : set.Layers
                .Fold(Fin.Succ(LayerFold.Seed), (acc, layer) => acc.Bind(state => resolve(layer.Material).Map(props => state.Absorb(layer, props))))
                .Bind(static state => state.Project());

    // A ConstituentSet folds the Fraction-weighted rule-of-mixtures; a constituent lacking a discipline NaNs that effective
    // property. The series fields (U/heat-capacity/Sd) and thickness-less STC are double.NaN/0 (no series structure). Only a
    // missing NODE rails — plus the empty set, whose vacuous Seed would project zero density/conductivity as computed values.
    private static Fin<AssemblyProperty> AggregateConstituents(MaterialComposition.ConstituentSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Constituents.IsEmpty
            ? Fin.Fail<AssemblyProperty>(Missing("<assembly-empty-constituent-set>"))
            : set.Constituents
                .Fold(Fin.Succ(MixtureFold.Seed), (acc, c) => acc.Bind(state => resolve(c.Material).Map(props => state.Absorb(c, props))))
                .Map(static state => state.Project());

    // Per-ply geometric volume follows composition shape: a Single/ProfileSet uses element volume, while a layer uses
    // thickness × face area, a constituent its fraction of volume — one closed Switch the GWP and cost folds share.
    private static Seq<(MaterialId Material, double VolumeM3)> PliesByVolume(MaterialComposition composition, ElementQuantity geometry) =>
        toSeq(composition.Switch(
                geometry,
                single:         static (g, s) => Seq((s.Material, g.VolumeM3)),
                profileSet:     static (g, s) => Seq((s.Material, g.VolumeM3)),
                layerSet:       static (g, s) => s.Layers.Map(l => (l.Material, l.Thickness.Si * g.EffectiveAreaM2)),
                constituentSet: static (g, s) => s.Constituents.Map(c => (c.Material, c.Fraction * g.VolumeM3)))
            .GroupBy(static ply => ply.Material)
            .Select(static group => (group.Key, group.Sum(static ply => ply.VolumeM3))));

    private static Fin<AssemblyCost> Accumulate(Option<AssemblyCost> running, MaterialPropertySet.Cost cost, double qty) =>
        running.Match(
            Some: r => r.Currency == cost.Currency
                ? Fin.Succ(r with { SupplyTotal = r.SupplyTotal + cost.SupplyPerUnit * qty, InstallTotal = r.InstallTotal + cost.InstallPerUnit * qty, LifecycleTotal = r.LifecycleTotal + cost.LifecyclePerUnit * qty })
                : Fin.Fail<AssemblyCost>(Missing($"<cost-currency-mismatch:{r.Currency.Key}<>{cost.Currency.Key}>")),
            None: () => Fin.Succ(new AssemblyCost(cost.Currency, cost.SupplyPerUnit * qty, cost.InstallPerUnit * qty, cost.LifecyclePerUnit * qty)));

    // Seam `Cost.Basis` selects the geometric quantity the per-unit price scales by; per-kg without a resolved
    // density rails the typed fault (mass is unresolvable), never a silent zero. Total over the four basis rows.
    private static Fin<double> DeclaredQuantity(MeasurementBasis basis, double volumeM3, double areaM2, Option<double> density, MaterialId material) =>
        basis.Switch(
            (volumeM3, areaM2, density, material),
            perM3:   static s => s.volumeM3 > 0.0 ? Fin.Succ(s.volumeM3) : Fin.Fail<double>(Missing($"<declared-per-m3-missing-volume:{s.material.Value}>")),
            perM2:   static s => s.areaM2 > 0.0 ? Fin.Succ(s.areaM2) : Fin.Fail<double>(Missing($"<declared-per-m2-missing-area:{s.material.Value}>")),
            perItem: static _ => Fin.Succ(1.0),
            perKg:   static s => s.volumeM3 > 0.0
                ? s.density.Map(d => s.volumeM3 * d).ToFin(Missing($"<declared-per-kg-missing-density:{s.material.Value}>"))
                : Fin.Fail<double>(Missing($"<declared-per-kg-missing-volume:{s.material.Value}>")));

    private static ComputeFault Missing(string detail) => new ComputeFault.AssessmentInputMissing(detail);

    // Per-ply reads compose the seam MaterialPropertyAccess accessors directly (props.Thermal/.Mechanical/.Fire/
    // .Environmental/.Cost) — never a re-derived is-cast; the seam exposes the FULL typed accessor family, so every read is ONE_HOP.
    private static Option<double> DensityKgM3(Seq<MaterialPropertySet> props) => props.Mechanical.Map(static m => m.Density.Si);
    private static Option<double> YoungsModulusPa(Seq<MaterialPropertySet> props) => props.Mechanical.Map(static m => m.YoungsModulus.Si);
    private static Option<double> ConductivityWmK(Seq<MaterialPropertySet> props) => props.Thermal.Map(static t => t.Conductivity.Si);
    // Worst-ply fire envelope reads the EN 13501-2 R (load-bearing) criterion off the seam FireResistance — the assembly's
    // structural fire endurance is the minimum LOAD-BEARING rating, never a single conflated resistance scalar (the deleted form).
    private static Option<double> FireMinutes(Seq<MaterialPropertySet> props) => props.Fire.Map(static f => (double)f.Resistance.LoadBearingMinutes);
    private static Option<double> PlyOverride(Seq<PlyQuantity> overrides, MaterialId material) => overrides.Find(q => q.Material == material).Map(static q => q.DeclaredQuantity);

    // Field-incidence mass law over the layer set's accumulated areal mass m' (kg·m⁻²): R(f) = 20·log10(m'·f) − 47,
    // evaluated at each seam AcousticBand centre into the per-band SRI vector the seam RatingContour.Stc.Fit contour-fits —
    // so the assembly STC and single-material STC share ONE ASTM-E413 owner, never the unphysical per-leaf dB sum.
    private static double[] MassLawBands(double massKgM2) {
        double[] sri = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) { sri[band.Key] = Math.Max(0.0, 20.0 * Math.Log10(massKgM2 * band.CenterHz) - MassLawConstantDb); }
        return sri;
    }

    // Op-key stamps the railed seam RatingContour.Fit window admission; LayerFold.Project preserves that fault.
    private static readonly Op ContourKey = Op.Of(name: nameof(MassLawBands));

    // Per-module Stage accumulation allocates a fresh array per ply, so the carbon fold stays immutable.
    // Seam StageGwp is the DERIVED GwpTotal-per-stage row (Environmental.StageGwp = IndicatorAt(GwpTotal, stage) over the
    // stage band, an ImmutableArray<double> of arity LifecycleStage.Count), so the fold folds it rather than re-slicing the matrix.
    private static double[] AddScaled(double[] accumulated, ImmutableArray<double> stage, double scale) {
        ReadOnlySpan<double> bands = stage.AsSpan();
        double[] next = new double[LifecycleStage.Count];
        for (int i = 0; i < LifecycleStage.Count; i++) { next[i] = accumulated[i] + bands[i] * scale; }
        return next;
    }

    // --- [FOLD_STATE] ---------------------------------------------------------------------
    // Typed per-discipline fold accumulators co-locate algorithm state with the kernel: each Absorbs one ply and
    // Projects the AssemblyProperty receipt, the per-discipline completeness flags deciding which columns are double.NaN vs computed.
    private readonly record struct LayerFold(double Resistance, double MassKgM2, double ModulusTPa, double HeatJM2K, double SdM, double ThicknessM, double MinFire, bool Thermal, bool Mechanical) {
        public static LayerFold Seed => new(RsiWM2K + RseWM2K, 0.0, 0.0, 0.0, 0.0, 0.0, double.MaxValue, true, true);

        public LayerFold Absorb(MaterialLayer layer, Seq<MaterialPropertySet> props) {
            Option<MaterialPropertySet.Thermal> thermal = props.Thermal;
            Option<double> density = DensityKgM3(props);
            double t = layer.Thickness.Si;
            return this with {
                Resistance = Resistance + thermal.Map(th => t / Math.Max(th.Conductivity.Si, double.Epsilon)).IfNone(0.0),
                MassKgM2   = MassKgM2 + density.Map(d => d * t).IfNone(0.0),
                ModulusTPa = ModulusTPa + YoungsModulusPa(props).Map(e => e * t).IfNone(0.0),
                HeatJM2K   = HeatJM2K + thermal.Bind(th => density.Map(d => d * th.SpecificHeat.Si * t)).IfNone(0.0),
                SdM        = SdM + thermal.Map(th => th.VapourResistanceFactor * t).IfNone(0.0),
                ThicknessM = ThicknessM + t,
                MinFire    = Math.Min(MinFire, FireMinutes(props).IfNone(double.MaxValue)),
                Thermal    = Thermal && thermal.IsSome,
                Mechanical = Mechanical && density.IsSome };
        }

        // EffectiveYoungsModulusPa is the IN-PLANE (membrane) thickness-weighted Voigt average Σ(t_i·E_i)/ΣT — the
        // iso-strain estimate for a layer set loaded in-plane; out-of-plane bending stiffness (laminate EI) is deferred.
        public Fin<AssemblyProperty> Project() =>
            Mechanical && MassKgM2 > 0.0
                ? RatingContour.Stc.Fit(MassLawBands(MassKgM2), ContourKey).Map(Receipt)
                : Fin.Succ(Receipt(0));

        private AssemblyProperty Receipt(int stcWeighted) => new(
            UValueWM2K:               Thermal ? 1.0 / Resistance : double.NaN,
            StcWeighted:              stcWeighted,
            EffectiveDensityKgM3:     Mechanical && ThicknessM > 0.0 ? MassKgM2 / ThicknessM : double.NaN,
            EffectiveConductivityWMK: Thermal && Resistance > RsiWM2K + RseWM2K ? ThicknessM / (Resistance - RsiWM2K - RseWM2K) : double.NaN,
            EffectiveYoungsModulusPa: Mechanical && ThicknessM > 0.0 ? ModulusTPa / ThicknessM : double.NaN,
            FireResistanceMinutes:    MinFire is double.MaxValue ? 0.0 : MinFire,
            ArealHeatCapacityKJM2K:   Thermal && Mechanical ? HeatJM2K / 1000.0 : double.NaN,
            VapourResistanceSdM:      Thermal ? SdM : double.NaN);
    }

    private readonly record struct MixtureFold(double Density, double Conductivity, double ModulusPa, double MinFire, bool Mechanical, bool Thermal) {
        public static MixtureFold Seed => new(0.0, 0.0, 0.0, double.MaxValue, true, true);

        public MixtureFold Absorb(MaterialConstituent c, Seq<MaterialPropertySet> props) {
            Option<double> density = DensityKgM3(props);
            Option<double> conductivity = ConductivityWmK(props);
            return this with {
                Density      = Density + density.Map(d => c.Fraction * d).IfNone(0.0),
                Conductivity = Conductivity + conductivity.Map(k => c.Fraction * k).IfNone(0.0),
                ModulusPa    = ModulusPa + YoungsModulusPa(props).Map(e => c.Fraction * e).IfNone(0.0),
                MinFire      = Math.Min(MinFire, FireMinutes(props).IfNone(double.MaxValue)),
                Mechanical   = Mechanical && density.IsSome,
                Thermal      = Thermal && conductivity.IsSome };
        }

        // EffectiveYoungsModulusPa is the Fraction-weighted Voigt average Σ(f_i·E_i) — the iso-strain rule-of-mixtures
        // estimate for a composite, the same Voigt form the conductivity/density mixtures use.
        public AssemblyProperty Project() => new(
            UValueWM2K:               double.NaN,
            StcWeighted:              0,
            EffectiveDensityKgM3:     Mechanical ? Density : double.NaN,
            EffectiveConductivityWMK: Thermal ? Conductivity : double.NaN,
            EffectiveYoungsModulusPa: Mechanical ? ModulusPa : double.NaN,
            FireResistanceMinutes:    MinFire is double.MaxValue ? 0.0 : MinFire,
            ArealHeatCapacityKJM2K:   double.NaN,
            VapourResistanceSdM:      double.NaN);
    }

    // EN 15978 carbon accumulator carries the per-module Stage vector (kgCO2e) and the recycled/total MASS pair the
    // mass-weighted recycled-content fraction divides; Stage is a fresh AddScaled array each ply, never mutated in place.
    private readonly record struct CarbonFold(double[] Stage, double RecycledMass, double TotalMass) {
        public static CarbonFold Seed => new(new double[LifecycleStage.Count], 0.0, 0.0);
        public double WholeLife => Stage.Sum();
    }

    // EN ISO 10077-1 whole-window accumulator carries the area-weighted glazed/frame conductance numerators (Σ Ag·Ug, Σ Af·Uf)
    // and their areas, plus the perimeter edge-seal bridge Σ lg·Ψg — a glazed field contributes all three, a frame field only
    // its area·Uf and area. Project area-weights each sub-transmittance and rails the typed fault on a zero total area.
    private readonly record struct WindowFold(double GlazedUA, double GlazedArea, double FrameUA, double FrameArea, double EdgeBridge) {
        public static WindowFold Seed => new(0.0, 0.0, 0.0, 0.0, 0.0);

        public WindowFold Absorb(WindowField field) => field.Switch(
            this,
            glazed: static (state, value) => state with {
                GlazedUA = state.GlazedUA + value.UWM2K * value.AreaM2,
                GlazedArea = state.GlazedArea + value.AreaM2,
                EdgeBridge = state.EdgeBridge + value.EdgeLengthM * value.PsiWM_K,
            },
            frame: static (state, value) => state with {
                FrameUA = state.FrameUA + value.UWM2K * value.AreaM2,
                FrameArea = state.FrameArea + value.AreaM2,
            });

        public Fin<WindowU> Project() {
            double totalArea = GlazedArea + FrameArea;
            return totalArea <= 0.0
                ? Fin.Fail<WindowU>(Missing("<window-zero-total-area>"))
                : Fin.Succ(new WindowU(
                    UwWM2K:        (GlazedUA + FrameUA + EdgeBridge) / totalArea,
                    UgWM2K:        GlazedArea > 0.0 ? GlazedUA / GlazedArea : double.NaN,
                    UfWM2K:        FrameArea > 0.0 ? FrameUA / FrameArea : double.NaN,
                    EdgeBridgeW_K: EdgeBridge,
                    GlazedFraction: GlazedArea / totalArea));
        }
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
