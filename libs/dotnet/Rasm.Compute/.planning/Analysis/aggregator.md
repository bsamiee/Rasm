# [COMPUTE_AGGREGATOR]

Multi-ply assembly-aggregation engine on the Compute analysis pipeline, because layered-construction property aggregation is analysis, not material authoring — the closed-form physics live in Compute, never the contract. One `AssemblyAggregator` static kernel folds a `Rasm.Element` contract `MaterialComposition` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) into one result per aggregation discipline, resolving each ply's `MaterialPropertySet` cases through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` keyed on the composition's native `MaterialId`, never a graph `NodeId`. Over a `LayerSet` it folds the series-resistance U (ISO 6946), the `Σ ρ·c·t` thermal mass (ISO 13786), the `Σ μ·t` vapour `Sd` (EN ISO 13788), and the field-incidence mass-law STC over accumulated areal mass `m' = Σ(ρ·t)`; over a `ConstituentSet`, the `Fraction`-weighted rule-of-mixtures density; worst-rated-ply fire over either; EN 15978 embodied carbon and basis-aware supply/install/lifecycle cost over any composition; and the EN ISO 10077-1 whole-window `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` over glazed/frame `WindowPart` fields — the side-by-side glazing-in-frame composition, with its edge-seal bridge `Ψg` and frame fraction, the perpendicular layer series structurally cannot reach.

Absence is an `Option` on every result column and a COUNTED gap on the result itself. A ply lacking a discipline no longer contributes a fabricated zero to a running sum that a downstream bool then NaNs: the accumulator for that discipline goes absent and stays absent, so the four completeness bools and the twelve `IfNone(0.0)` defaults collapse into the carrier they were re-deriving, while a `WriterT<PlyGaps, Fin, A>` ledger records WHICH ply lacked WHICH discipline — the question a carbon or fire reviewer asks first and that a NaN column could never answer. The element takeoff is the kernel `MeasureBundle`, whose `MassKind` discriminant survives on every row, rather than a local three-scalar struct the kernel measure owner rules the deleted form.

The kernel reads the contract vocabulary settled — `MaterialComposition`/`MaterialLayer`/`MaterialConstituent`, the `MaterialPropertySet` `[Union]` cases through the `MaterialPropertyAccess` accessors, the `AcousticBand`/`LifecycleStage` band vocabularies, the contract `RatingContour.Stc.Fit` single-number contour kernel, the `MeasurementBasis`/`Currency` cost axes, and the `MaterialLayer.Thickness.Si` SI thickness. Window fields are the runner's — glazed `Ug` and frame `Uf` off each part's `Thermal.UValue.Si`, spacer `Ψg` off the window's `Pset`, areas off the baked `Qto_*BaseQuantities` — so the kernel folds already-resolved fields and never reads `Rasm.Materials`, the contract material and the baked bags its only ingress.

## [01]-[INDEX]

- [02]-[ASSEMBLY_RESULT]: `AssemblyProperty`, `AssemblyLifecycle`, `AssemblyCost`, and `WindowU` carry each aggregation discipline's result over the kernel takeoff bundle the folds distribute per ply, each absence an `Option` and each cause a counted `PlyGap`.
- [03]-[AGGREGATION_FOLD]: `AssemblyAggregator` folds a contract composition or a runner-resolved window-part set into one typed accumulator per aggregation discipline, over one gap-accumulating carrier.

## [02]-[ASSEMBLY_RESULT]

- Owner: `AssemblyProperty` the thermal/mass/vapour/acoustic/mixture/fire result; `AssemblyLifecycle`/`AssemblyCost` the EN 15978 embodied-carbon and in-place-cost results; `WindowU` the EN ISO 10077-1 whole-window result, `WindowPart` the resolved glazed-or-frame field the thermal runner assembles and feeds the fold; `ElementTakeoff` the element geometric takeoff — the kernel `MeasureBundle` beside the `Rasm.Fabrication` off-cut column — the GWP/cost folds distribute per ply; `PlyQuantity` the optional per-`MaterialId` exact declared-quantity override an IFC `Qto_*BaseQuantities` takeoff supplies; `PlyDiscipline` the capability vocabulary naming which contract property case a ply held, `PlyGap` one ply's one missing discipline, and `PlyGaps` the `Monoid` ledger the fold accumulates.
- Cases: one `AssemblyProperty` over a `LayerSet` carrying the ISO 6946 series U (with the `Rsi`/`Rse` films), the ISO 13786 areal heat capacity, the EN ISO 13788 vapour `Sd`, the mass-law `StcWeighted` (contour-fit through the contract `RatingContour.Stc.Fit`), the effective bulk density, and the worst-rated-ply fire; one `AssemblyLifecycle` (whole-life + per-module `StageGwp`, intensity, mass-weighted recycled fraction); one `AssemblyCost` (supply/install/lifecycle over one `Currency`); one `WindowU` over a `Seq<WindowPart>` (the `Uw`, area-weighted glazed/frame sub-transmittances, the `Σ lg·Ψg` edge bridge, the `GlazedFraction` the daylight/solar-gain consumer reads) — a new aggregation rating is one fold over the same composition or window parts with one result column, never a parallel composite-material owner.
- Entry: the results mint through the `[03]-[AGGREGATION_FOLD]` folds; per-ply reads compose the contract `MaterialPropertyAccess` accessors directly (`props.Thermal`/`.Mechanical`/`.Fire`/`.Environmental`/`.Cost`) — the contract exposing the full typed accessor family so every discipline reads contract-direct (ONE_HOP), an `Option<T>` absent case the fold reports or fails, never an `is`-cast the contract owns.
- Packages: UnitsNet (`ThermalResistance` binding the ISO 6946 surface films), LanguageExt.Core (`Fin`/`Seq`/`Option`/`HashMap`/`WriterT`/`Monoid`/`TraverseM`), Thinktecture.Runtime.Extensions (the generated `MaterialComposition.Switch`/`MeasurementBasis.Switch`/`WindowPart.Switch`, `[SmartEnum<string>]`), Rasm (kernel — `MeasureBundle`/`MassKind` the takeoff carrier, `CapabilitySet<TCapability>`/`ICapability<TSelf>`, `EpsilonPolicy.ZeroTolerance` the `ToleranceLane.Identity` degeneracy anchor, `Op`), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `MaterialPropertySet`, `MaterialPropertyAccess`, `MaterialId`, `AcousticBand`, `LifecycleStage`, `Currency`, `MeasurementBasis`, `RatingContour.Stc.Fit`), the `Runtime/admission#DISPATCH_SPINE` `ComputeFault`/`AssessmentInputReason`, Generator.Equals (`[Equatable]`+`[OrderedEquality]` — the `StageGwp` latent-trap repair), CommunityToolkit.HighPerformance (`MemoryOwner<double>` the fold scratch), BCL inbox (`TensorPrimitives`, `ImmutableArray<double>` the `StageGwp` carrier).
- Growth: a new assembly rating is one `AssemblyAggregator` fold reading the same contract `MaterialPropertySet` cases into one result column; a new band is one contract `AcousticBand`/`LifecycleStage` row (the vector widens by data, the fold re-reads the new length); a new per-ply discipline is one `PlyDiscipline` row every coverage set and every gap row absorbs with zero fold edits.
- Boundary: results carry raw SI scalars, not a contract `MeasureValue` or `MaterialPropertySet` type — the result is the analysis input the runners read and the write-back lowers onto `AssessmentFact` values, so the aggregator never re-mints the contract value family; each ply read lifts the member's `MeasureValue.Si` so a later contract unit canonicalization never breaks the fold. Every unreachable column is an `Option`, never a `double.NaN`: a sentinel is a number that survives arithmetic, so a NaN intensity multiplied into a portfolio rollup silently NaNs the portfolio, and the consumer's `double.IsFinite` re-derivation was the presence test the carrier now states. Every absent-discipline CAUSE rides `Gaps` as a counted `(MaterialId, PlyDiscipline)` row — a partial assembly reports WHICH ply lacked WHICH discipline, where the four completeness bools reported only which column went absent. `Coverage` DERIVES from the accumulators that survived, never a hand-kept mirror. `AssemblyCost` carries no `MeasurementBasis` (the per-unit basis is consumed at the fold, the total absolute currency); `WindowPart` likewise carries raw SI scalars the thermal runner lifts from the contract — glazed/frame `U` off `Thermal.UValue.Si`, areas off `Qto_*BaseQuantities`, the spacer `Ψg` off the window's `Pset` thermal-bridge property (NOT `Thermal`, which carries no perimeter-bridge column) — the kernel folds already-resolved parts, never reading `Rasm.Materials`; an assembly property or `Uw` is computed on demand, never stored as a material.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlyDiscipline : ICapability<PlyDiscipline> {
    public static readonly PlyDiscipline Thermal = new("thermal");
    public static readonly PlyDiscipline Mechanical = new("mechanical");
    public static readonly PlyDiscipline Fire = new("fire");
    public static readonly PlyDiscipline Environmental = new("environmental");
    public static readonly PlyDiscipline Cost = new("cost");
}

public readonly record struct PlyGap(MaterialId Material, PlyDiscipline Discipline);

public readonly record struct PlyGaps(Seq<PlyGap> Facts) : Monoid<PlyGaps> {
    public static PlyGaps Empty => new(Seq<PlyGap>());
    public static PlyGaps Of(MaterialId material, PlyDiscipline discipline) => new(Seq(new PlyGap(material, discipline)));
    public PlyGaps Combine(PlyGaps rhs) => new(Facts + rhs.Facts);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ElementTakeoff(MeasureBundle Measures, Option<double> WasteAreaM2) {
    public Option<double> Area => Measures.Magnitude(MassKind.Area);
    public Option<double> Volume => Measures.Magnitude(MassKind.Volume);

    public Option<double> EffectiveArea =>
        Area.Map(area => WasteAreaM2.Match(Some: waste => area + waste, None: () => area));
}

public readonly record struct PlyQuantity(MaterialId Material, double DeclaredQuantity);

public sealed record AssemblyProperty(
    Option<double> UValueWM2K,
    Option<int> StcWeighted,
    Option<double> EffectiveDensityKgM3,
    Option<int> FireResistanceMinutes,
    Option<double> ArealHeatCapacityKJM2K,
    Option<double> VapourResistanceSdM,
    CapabilitySet<PlyDiscipline> Coverage,
    Seq<PlyGap> Gaps);

[Equatable]
public sealed partial record AssemblyLifecycle(
    double WholeLifeGwpKgCo2e,
    [property: OrderedEquality] ImmutableArray<double> StageGwp,
    Option<double> EmbodiedCarbonIntensityKgCo2eM2,
    Option<double> RecycledContentFraction,
    [property: OrderedEquality] Seq<PlyGap> Gaps);

public sealed record AssemblyCost(Currency Currency, double SupplyTotal, double InstallTotal, double LifecycleTotal) {
    public double TotalInPlace => SupplyTotal + InstallTotal;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowPart {
    private WindowPart() { }

    public sealed record Glazed(double UWM2K, double AreaM2, double EdgeLengthM, double PsiWM_K) : WindowPart;
    public sealed record Frame(double UWM2K, double AreaM2) : WindowPart;
}

public sealed record WindowU(double UwWM2K, Option<double> UgWM2K, Option<double> UfWM2K, double EdgeBridgeW_K, double GlazedFraction);
```

## [03]-[AGGREGATION_FOLD]

- Owner: `AssemblyAggregator` the static fold kernel over a contract `MaterialComposition` — `Aggregate` (thermal/mass/vapour/acoustic/mixture/fire), `AggregateEnvironmental` (EN 15978 GWP), `AggregateCost` (basis-aware cost), `AggregateWindow` (EN ISO 10077-1 whole-window `Uw` over a `Seq<WindowPart>`); each composition fold discriminates through the contract's generated total `Switch` and traverses each ply's `MaterialPropertySet`, resolved through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>`, into a typed `[FOLD_STATE]` accumulator that `Absorb`s one admitted ply and `Project`s the result; `Plies` the `WriterT<PlyGaps, Fin, A>` carrier vocabulary every per-ply read reports through.
- Entry: `Aggregate(composition, resolve)` over a `LayerSet`/`ConstituentSet` (a `Single`/`ProfileSet` fails — no series structure to aggregate, its intrinsic U/STC read contract-direct), with `AggregateEnvironmental(composition, resolve, overrides, geometry)` and `AggregateCost(…)` over any composition; `AggregateWindow(parts)` folds a `Seq<WindowPart>` into the `WindowU`. Every entry returns the result alone — the gap ledger the run accumulated rides ON the result, so a caller never threads a second output and never re-runs the fold to learn the cause.
- Auto: the resolver reads a ply node's contract property cases keyed on `MaterialId`, not a graph `NodeId`, so the fold reads the composition's own plies; per-ply resolution runs through `TraverseM` over the writer carrier, so each ply's admitted contribution and its gap facts land in one pass and the accumulator fold that follows is PURE and total. The mass-law STC evaluates `R(f)=20·log₁₀(m'·f)−47` at each contract `AcousticBand` centre through one `TensorPrimitives` chain over one leased buffer and feeds the vector ONCE through the contract `RatingContour.Stc.Fit`, so the assembly STC and the single-material STC share one ASTM-E413 contour-fit owner. The `Rsi`/`Rse` films (ISO 6946) are bound `UnitsNet.ThermalResistance` values whose one SI projection seeds the resistance at the building-envelope ends. The GWP and cost folds share ONE basis-aware `DeclaredQuantity` derivation off the contract `Environmental`/`Cost` `Basis` unless a `PlyQuantity` overrides, and the recycled-content fraction weights each ply by its mass `ρ·V`, excluding a density-less ply and NAMING it in the ledger.
- Packages: UnitsNet, LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel — `MeasureBundle`/`MassKind`, `CapabilitySet`, `EpsilonPolicy`, `Op`), Rasm.Element (project — the contract composition and property vocabulary), the `Runtime/admission#DISPATCH_SPINE` fault family, CommunityToolkit.HighPerformance, BCL inbox (`TensorPrimitives`).
- Growth: a new aggregation discipline is one `AssemblyAggregator` fold reading the same contract cases into a typed `[FOLD_STATE]` accumulator; the generated total `Switch` over the closed composition family breaks at compile time if the contract adds a composition case, never a runtime-silent `_` arm.
- Boundary: the constituent fraction rides the contract `MaterialConstituent.Fraction` (normalized to unity at contract admission, the aggregator never re-guarding) and the resolver keys on `MaterialId`, so a `NodeId`-keyed lookup cannot reach a composition's own plies. An absent ply property in the multi-physics `Aggregate` is DECOUPLED per discipline — a missing `Thermal` costs only `U`/`Sd`/heat-capacity, a missing `Mechanical` only the mass-derived fields — and ONLY a missing material NODE fails; the single-discipline `AggregateEnvironmental`/`AggregateCost` DO fail on a missing `Environmental`/`Cost` case (a hole invalidates the sum) and on a per-kg basis over a density-less material. A discipline's accumulator goes ABSENT the moment one ply lacks it and never recovers, which is the "every ply must hold it" law stated by the carrier rather than by a `bool` conjunction beside twelve fabricated zeros — and the ply that broke it is named in `Gaps`. Worst-ply fire is the MINIMUM over the plies that carry an EN 13501-2 load-bearing rating; an unrated ply is non-limiting and COUNTED, and an entirely unrated set reports absence, where the `double.MaxValue` sentinel and its `? 0.0` collapse reported the worst possible rating from no evidence at all. A conductivity at or below the kernel `ToleranceLane.Identity` degeneracy anchor is ABSENT thermal evidence rather than a floored divisor: `double.Epsilon` as a physical λ yields `R ≈ 1e300` and reports a wall as a perfect insulator. Cost is over one `Currency` (a mismatch fails, the fold carrying no exchange rate); the mass-law STC contour-fits through the contract kernel (the naive per-leaf dB sum, which over-predicts a rigidly-bonded set, the deleted form); a `Single`/`ProfileSet` fails explicitly and an EMPTY `LayerSet`/`ConstituentSet` fails the same fault, because the vacuous seed otherwise projects a films-only `U` over full coverage. Same-material plies coalesce by SUM in FIRST-APPEARANCE order — a hash-map enumeration order is unspecified, so summing in map order makes the floating-point total depend on hash layout and a content-keyed result stops reproducing. `AggregateWindow` is the EN ISO 10077-1 side-by-side fold: it area-weights the glazed `Ug` (EN 673 center-of-glass off `Thermal.UValue`, never folded raw as a single-material whole-window U — the deleted form) and frame `Uf` and ADDS the `Σ lg·Ψg` edge-seal bridge the layer series cannot reach, a frame part zeroing its edge term; the kernel folds the runner-resolved parts pure, never reading a graph node.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Plies {
    public static WriterT<PlyGaps, Fin, A> Held<A>(A value) => WriterT.pure<PlyGaps, Fin, A>(value);
    public static WriterT<PlyGaps, Fin, A> Missing<A>(MaterialId material, PlyDiscipline discipline, A value) =>
        WriterT.write<PlyGaps, Fin, A>(value, PlyGaps.Of(material, discipline));
    public static WriterT<PlyGaps, Fin, A> Lift<A>(Fin<A> result) => WriterT.lift<PlyGaps, Fin, A>(result);
    public static Fin<(A Value, PlyGaps Log)> Run<A>(WriterT<PlyGaps, Fin, A> writer) => writer.Run().As();

    public static WriterT<PlyGaps, Fin, Option<A>> Reading<A>(MaterialId material, PlyDiscipline discipline, Option<A> read) =>
        read.Match(Some: value => Held(Some(value)), None: () => Missing(material, discipline, Option<A>.None));
}

public static class AssemblyAggregator {
    private static readonly ThermalResistance Rsi = ThermalResistance.From(0.13, ThermalResistanceUnit.SquareMeterKelvinPerWatt);
    private static readonly ThermalResistance Rse = ThermalResistance.From(0.04, ThermalResistanceUnit.SquareMeterKelvinPerWatt);

    private static readonly double FilmResistanceM2KW =
        Rsi.As(ThermalResistanceUnit.SquareMeterKelvinPerWatt) + Rse.As(ThermalResistanceUnit.SquareMeterKelvinPerWatt);
    private const double MassLawConstantDb = 47.0;

    private static readonly ImmutableArray<double> BandCentresHz = [.. AcousticBand.Items.Select(static band => band.CenterHz)];

    private static readonly Op ContourKey = Op.Of(name: nameof(Stc));

    public static Fin<AssemblyProperty> Aggregate(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        composition.Switch(
            resolve,
            single:         static (_, s) => Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionShape, s.Material.Value)),
            profileSet:     static (_, s) => Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionShape, s.Material.Value)),
            layerSet:       static (r, set) => AggregateLayers(set, r),
            constituentSet: static (r, set) => AggregateConstituents(set, r));

    private static Fin<AssemblyProperty> AggregateLayers(MaterialComposition.LayerSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Layers.IsEmpty
            ? Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionEmpty, nameof(MaterialComposition.LayerSet)))
            : Plies.Run(set.Layers.TraverseM(layer => LayerPlyOf(layer, resolve)).As()
                    .Map(static plies => plies.Fold(LayerFold.Seed, static (state, ply) => state.Absorb(ply))))
                .Bind(static run => run.Value.Project(run.Log.Facts));

    private static Fin<AssemblyProperty> AggregateConstituents(MaterialComposition.ConstituentSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Constituents.IsEmpty
            ? Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionEmpty, nameof(MaterialComposition.ConstituentSet)))
            : Plies.Run(set.Constituents.TraverseM(c => MixturePlyOf(c, resolve)).As()
                    .Map(static plies => plies.Fold(MixtureFold.Seed, static (state, ply) => state.Absorb(ply))))
                .Map(static run => run.Value.Project(run.Log.Facts));

    public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementTakeoff geometry) =>
        PliesByVolume(composition, geometry).Bind(plies => plies.IsEmpty
            ? Fin.Fail<AssemblyLifecycle>(Missing(AssessmentInputReason.CompositionEmpty, nameof(AggregateEnvironmental)))
            : Plies.Run(plies.TraverseM(ply => CarbonPlyOf(ply, resolve, overrides, geometry)).As())
                .Map(run => Carbon(run.Value, geometry, run.Log.Facts)));

    public static Fin<AssemblyCost> AggregateCost(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementTakeoff geometry) =>
        PliesByVolume(composition, geometry).Bind(plies => plies.IsEmpty
            ? Fin.Fail<AssemblyCost>(Missing(AssessmentInputReason.CompositionEmpty, nameof(AggregateCost)))
            : plies.TraverseM(ply => CostPlyOf(ply, resolve, overrides, geometry)).As()
                .Bind(static priced => priced.Fold(Fin.Succ(Option<AssemblyCost>.None),
                        static (acc, ply) => acc.Bind(running => Accumulate(running, ply.Cost, ply.Quantity)))
                    .Bind(static o => o.ToFin(Missing(AssessmentInputReason.CompositionEmpty, nameof(AggregateCost))))));

    public static Fin<WindowU> AggregateWindow(Seq<WindowPart> parts) =>
        parts.IsEmpty
            ? Fin.Fail<WindowU>(Missing(AssessmentInputReason.WindowFieldAbsent, string.Empty))
            : parts.Fold(WindowFold.Seed, static (state, part) => state.Absorb(part)).Project();

    // --- [PLY_ADMISSION]
    private static WriterT<PlyGaps, Fin, LayerPly> LayerPlyOf(MaterialLayer layer, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        from props in Plies.Lift(resolve(layer.Material))
        from thermal in Plies.Reading(layer.Material, PlyDiscipline.Thermal, Conductive(props))
        from density in Plies.Reading(layer.Material, PlyDiscipline.Mechanical, props.Mechanical.Map(static m => m.Density.Si))
        from fire in Plies.Reading(layer.Material, PlyDiscipline.Fire, FireMinutes(props))
        select new LayerPly(layer.Thickness.Si, thermal, density, fire);

    private static WriterT<PlyGaps, Fin, MixturePly> MixturePlyOf(MaterialConstituent constituent, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        from props in Plies.Lift(resolve(constituent.Material))
        from density in Plies.Reading(constituent.Material, PlyDiscipline.Mechanical, props.Mechanical.Map(static m => m.Density.Si))
        from fire in Plies.Reading(constituent.Material, PlyDiscipline.Fire, FireMinutes(props))
        select new MixturePly(constituent.Fraction, density, fire);

    private static WriterT<PlyGaps, Fin, CarbonPly> CarbonPlyOf(
        (MaterialId Material, double VolumeM3) ply, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve,
        Seq<PlyQuantity> overrides, ElementTakeoff geometry) =>
        from props in Plies.Lift(resolve(ply.Material))
        from env in Plies.Lift(props.Environmental.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, ply.Material.Value)))
        from quantity in Plies.Lift(Quantity(env.Basis, ply, overrides, props, geometry))
        from density in Plies.Reading(ply.Material, PlyDiscipline.Mechanical, props.Mechanical.Map(static m => m.Density.Si))
        select new CarbonPly(env.StageGwp, quantity,
            density.Map(d => d * ply.VolumeM3),
            density.Bind(d => env.RecycledContent.Map(share => share * d * ply.VolumeM3)));

    private static WriterT<PlyGaps, Fin, (MaterialPropertySet.Cost Cost, double Quantity)> CostPlyOf(
        (MaterialId Material, double VolumeM3) ply, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve,
        Seq<PlyQuantity> overrides, ElementTakeoff geometry) =>
        from props in Plies.Lift(resolve(ply.Material))
        from cost in Plies.Lift(props.Cost.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, ply.Material.Value)))
        from quantity in Plies.Lift(Quantity(cost.Basis, ply, overrides, props, geometry))
        select (cost, quantity);

    private static Option<ThermalPly> Conductive(Seq<MaterialPropertySet> props) =>
        props.Thermal
            .Filter(static t => t.Conductivity.Si > EpsilonPolicy.ZeroTolerance)
            .Map(static t => new ThermalPly(t.Conductivity.Si, t.SpecificHeat.Si, t.VapourResistanceFactor));

    private static Option<int> FireMinutes(Seq<MaterialPropertySet> props) =>
        props.Fire.Bind(static f => f.Resistance.LoadBearingMinutes);

    private static Fin<double> Quantity(
        MeasurementBasis basis, (MaterialId Material, double VolumeM3) ply, Seq<PlyQuantity> overrides,
        Seq<MaterialPropertySet> props, ElementTakeoff geometry) =>
        overrides.Find(q => q.Material == ply.Material).Map(static q => q.DeclaredQuantity).Match(
            Some: Fin.Succ,
            None: () => DeclaredQuantity(basis, ply.VolumeM3, geometry, props.Mechanical.Map(static m => m.Density.Si), ply.Material));

    private static Fin<double> DeclaredQuantity(MeasurementBasis basis, double volumeM3, ElementTakeoff geometry, Option<double> density, MaterialId material) =>
        basis.Switch(
            (volumeM3, geometry, density, material),
            perM3:   static s => s.volumeM3 > 0.0 ? Fin.Succ(s.volumeM3) : Fin.Fail<double>(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value)),
            perM2:   static s => s.geometry.EffectiveArea.Filter(static area => area > 0.0).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value)),
            perItem: static _ => Fin.Succ(1.0),
            perKg:   static s => s.volumeM3 > 0.0
                ? s.density.Map(d => s.volumeM3 * d).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value))
                : Fin.Fail<double>(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value)));

    private static Fin<Seq<(MaterialId Material, double VolumeM3)>> PliesByVolume(MaterialComposition composition, ElementTakeoff geometry) =>
        composition.Switch(
            geometry,
            single:         static (g, s) => g.Volume.Map(v => Seq((s.Material, v))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.Material.Value)),
            profileSet:     static (g, s) => g.Volume.Map(v => Seq((s.Material, v))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.Material.Value)),
            layerSet:       static (g, s) => g.EffectiveArea.Map(a => s.Layers.Map(l => (l.Material, l.Thickness.Si * a))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, nameof(MassKind.Area))),
            constituentSet: static (g, s) => g.Volume.Map(v => s.Constituents.Map(c => (c.Material, c.Fraction * v))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, nameof(MassKind.Volume))))
        .Map(Coalesce);

    private static Seq<(MaterialId Material, double VolumeM3)> Coalesce(Seq<(MaterialId Material, double VolumeM3)> plies) {
        HashMap<MaterialId, double> sums = plies.Fold(HashMap<MaterialId, double>(),
            static (held, ply) => held.AddOrUpdate(ply.Material, existing => existing + ply.VolumeM3, () => ply.VolumeM3));
        return plies.Map(static ply => ply.Material).Distinct().Map(id => (id, sums[id]));
    }

    private static Fin<AssemblyCost> Accumulate(Option<AssemblyCost> running, MaterialPropertySet.Cost cost, double qty) =>
        running.Match(
            Some: r => r.Currency == cost.Currency
                ? Fin.Succ(r with { SupplyTotal = r.SupplyTotal + cost.SupplyPerUnit * qty, InstallTotal = r.InstallTotal + cost.InstallPerUnit * qty, LifecycleTotal = r.LifecycleTotal + cost.LifecyclePerUnit * qty })
                : Fin.Fail<AssemblyCost>(Missing(AssessmentInputReason.CurrencyMismatch, $"{r.Currency.Key}<>{cost.Currency.Key}")),
            None: () => Fin.Succ(new AssemblyCost(cost.Currency, cost.SupplyPerUnit * qty, cost.InstallPerUnit * qty, cost.LifecyclePerUnit * qty)));

    private static ComputeFault Missing(AssessmentInputReason reason, string witness) =>
        new ComputeFault.AssessmentInputMissing(reason, witness);

    // --- [SPAN_FOLDS]
    private static Fin<int> Stc(double massKgM2) {
        using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(BandCentresHz.Length);
        Span<double> sri = scratch.Span;
        TensorPrimitives.Multiply(BandCentresHz.AsSpan(), massKgM2, sri);
        TensorPrimitives.Log10(sri, sri);
        TensorPrimitives.MultiplyAdd(sri, 20.0, -MassLawConstantDb, sri);
        TensorPrimitives.Max(sri, 0.0, sri);
        return RatingContour.Stc.Fit(sri, ContourKey);
    }

    private static AssemblyLifecycle Carbon(Seq<CarbonPly> plies, ElementTakeoff geometry, Seq<PlyGap> gaps) {
        using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(LifecycleStage.Count, AllocationMode.Clear);
        Span<double> stages = scratch.Span;
        foreach (CarbonPly ply in plies) { TensorPrimitives.MultiplyAdd(ply.StageGwp.AsSpan(), ply.Quantity, stages, stages); }
        double wholeLife = TensorPrimitives.Sum(stages);
        Option<double> recycled = plies.Fold(Option<double>.None, static (held, ply) => Combine(held, ply.RecycledMassKg));
        Option<double> mass = plies.Fold(Option<double>.None, static (held, ply) => Combine(held, ply.MassKg));
        return new AssemblyLifecycle(
            WholeLifeGwpKgCo2e: wholeLife,
            StageGwp: [.. stages],
            EmbodiedCarbonIntensityKgCo2eM2: geometry.Area.Filter(static area => area > 0.0).Map(area => wholeLife / area),
            RecycledContentFraction: mass.Filter(static total => total > 0.0).Bind(total => recycled.Map(share => share / total)),
            Gaps: gaps);
    }

    private static Option<double> Combine(Option<double> running, Option<double> addend) =>
        addend.Match(Some: value => Some(running.Match(Some: held => held + value, None: () => value)), None: () => running);

    // --- [FOLD_STATE] ------------------------------------------------------------------
    private readonly record struct ThermalPly(double ConductivityWmK, double SpecificHeatJKgK, double VapourResistanceFactor);

    private readonly record struct LayerPly(double ThicknessM, Option<ThermalPly> Thermal, Option<double> DensityKgM3, Option<int> FireMinutes);

    private readonly record struct MixturePly(double Fraction, Option<double> DensityKgM3, Option<int> FireMinutes);

    private readonly record struct CarbonPly(ImmutableArray<double> StageGwp, double Quantity, Option<double> MassKg, Option<double> RecycledMassKg);

    private readonly record struct LayerFold(
        Option<double> ResistanceM2KW, Option<double> MassKgM2, Option<double> HeatJM2K, Option<double> SdM,
        double ThicknessM, Option<int> MinFireMinutes) {
        public static LayerFold Seed => new(Some(FilmResistanceM2KW), Some(0.0), Some(0.0), Some(0.0), 0.0, None);

        public LayerFold Absorb(LayerPly ply) => this with {
            ResistanceM2KW = ResistanceM2KW.Bind(held => ply.Thermal.Map(t => held + ply.ThicknessM / t.ConductivityWmK)),
            MassKgM2 = MassKgM2.Bind(held => ply.DensityKgM3.Map(d => held + d * ply.ThicknessM)),
            HeatJM2K = HeatJM2K.Bind(held => ply.Thermal.Bind(t => ply.DensityKgM3.Map(d => held + d * t.SpecificHeatJKgK * ply.ThicknessM))),
            SdM = SdM.Bind(held => ply.Thermal.Map(t => held + t.VapourResistanceFactor * ply.ThicknessM)),
            ThicknessM = ThicknessM + ply.ThicknessM,
            MinFireMinutes = Least(MinFireMinutes, ply.FireMinutes),
        };

        public CapabilitySet<PlyDiscipline> Coverage => Covered(ResistanceM2KW.IsSome, MassKgM2.IsSome, MinFireMinutes.IsSome);

        public Fin<AssemblyProperty> Project(Seq<PlyGap> gaps) =>
            MassKgM2.Filter(static mass => mass > 0.0).Map(Stc).Sequence().Map(stc => new AssemblyProperty(
                UValueWM2K:             ResistanceM2KW.Filter(static r => r > 0.0).Map(static r => 1.0 / r),
                StcWeighted:            stc,
                EffectiveDensityKgM3:   ThicknessM > 0.0 ? MassKgM2.Map(mass => mass / ThicknessM) : None,
                FireResistanceMinutes:  MinFireMinutes,
                ArealHeatCapacityKJM2K: HeatJM2K.Map(static joules => joules / 1000.0),
                VapourResistanceSdM:    SdM,
                Coverage:               Coverage,
                Gaps:                   gaps));
    }

    private readonly record struct MixtureFold(Option<double> DensityKgM3, Option<int> MinFireMinutes) {
        public static MixtureFold Seed => new(Some(0.0), None);

        public MixtureFold Absorb(MixturePly ply) => this with {
            DensityKgM3 = DensityKgM3.Bind(held => ply.DensityKgM3.Map(d => held + ply.Fraction * d)),
            MinFireMinutes = Least(MinFireMinutes, ply.FireMinutes),
        };

        public AssemblyProperty Project(Seq<PlyGap> gaps) => new(
            UValueWM2K:             None,
            StcWeighted:            None,
            EffectiveDensityKgM3:   DensityKgM3,
            FireResistanceMinutes:  MinFireMinutes,
            ArealHeatCapacityKJM2K: None,
            VapourResistanceSdM:    None,
            Coverage:               Covered(thermal: false, DensityKgM3.IsSome, MinFireMinutes.IsSome),
            Gaps:                   gaps);
    }

    private readonly record struct WindowFold(double GlazedUA, double GlazedArea, double FrameUA, double FrameArea, double EdgeBridge) {
        public static WindowFold Seed => new(0.0, 0.0, 0.0, 0.0, 0.0);

        public WindowFold Absorb(WindowPart part) => part.Switch(
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
                ? Fin.Fail<WindowU>(Missing(AssessmentInputReason.WindowZeroArea, string.Empty))
                : Fin.Succ(new WindowU(
                    UwWM2K:         (GlazedUA + FrameUA + EdgeBridge) / totalArea,
                    UgWM2K:         GlazedArea > 0.0 ? Some(GlazedUA / GlazedArea) : None,
                    UfWM2K:         FrameArea > 0.0 ? Some(FrameUA / FrameArea) : None,
                    EdgeBridgeW_K:  EdgeBridge,
                    GlazedFraction: GlazedArea / totalArea));
        }
    }

    private static Option<int> Least(Option<int> running, Option<int> candidate) =>
        candidate.Match(Some: minutes => Some(running.Match(Some: held => int.Min(held, minutes), None: () => minutes)), None: () => running);

    private static CapabilitySet<PlyDiscipline> Covered(bool thermal, bool mechanical, bool fire) =>
        CapabilitySet<PlyDiscipline>.Of([.. Seq(
            (PlyDiscipline.Thermal, thermal), (PlyDiscipline.Mechanical, mechanical), (PlyDiscipline.Fire, fire))
            .Filter(static row => row.Item2).Map(static row => row.Item1)]);
}
```
