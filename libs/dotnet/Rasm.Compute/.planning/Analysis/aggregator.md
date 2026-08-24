# [COMPUTE_AGGREGATOR]

Multi-ply assembly-aggregation engine on the Compute analysis rail, because layered-construction property aggregation is analysis, not material authoring — the closed-form physics live in Compute, never the seam. One `AssemblyAggregator` static kernel folds a `Rasm.Element` seam `MaterialComposition` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) into one receipt per aggregation discipline, resolving each ply's `MaterialPropertySet` cases through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` keyed on the composition's native `MaterialId`, never a graph `NodeId`. Over a `LayerSet` it folds the series-resistance U (ISO 6946), the `Σ ρ·c·t` thermal mass (ISO 13786), the `Σ μ·t` vapour `Sd` (EN ISO 13788), and the field-incidence mass-law STC over accumulated areal mass `m' = Σ(ρ·t)`; over a `ConstituentSet`, the `Fraction`-weighted rule-of-mixtures density; worst-rated-ply fire over either; EN 15978 embodied carbon and basis-aware supply/install/lifecycle cost over any composition; and the EN ISO 10077-1 whole-window `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` over glazed/frame `WindowPart` fields — the side-by-side glazing-in-frame composition, with its edge-seal bridge `Ψg` and frame fraction, the perpendicular layer series structurally cannot reach.

Absence is an `Option` on every receipt column and a COUNTED gap on the receipt itself. A ply lacking a discipline no longer contributes a fabricated zero to a running sum that a downstream bool then NaNs: the accumulator for that discipline goes absent and stays absent, so the four completeness bools and the twelve `IfNone(0.0)` defaults collapse into the carrier they were re-deriving, while a `WriterT<PlyGaps, Fin, A>` ledger records WHICH ply lacked WHICH discipline — the question a carbon or fire reviewer asks first and that a NaN column could never answer. The element takeoff is the kernel `MeasureBundle`, whose `MassKind` discriminant survives on every row, rather than a local three-scalar struct the kernel measure owner rules the deleted form.

The kernel reads the seam vocabulary settled — `MaterialComposition`/`MaterialLayer`/`MaterialConstituent`, the `MaterialPropertySet` `[Union]` cases through the `MaterialPropertyAccess` accessors, the `AcousticBand`/`LifecycleStage` band vocabularies, the seam `RatingContour.Stc.Fit` single-number contour kernel, the `MeasurementBasis`/`Currency` cost axes, and the `MaterialLayer.Thickness.Si` SI thickness. Window fields are the runner's — glazed `Ug` and frame `Uf` off each part's `Thermal.UValue.Si`, spacer `Ψg` off the window's `Pset`, areas off the baked `Qto_*BaseQuantities` — so the kernel folds already-resolved fields and never reads `Rasm.Materials`, the seam material and the baked bags its only ingress.

## [01]-[INDEX]

- [02]-[ASSEMBLY_RECEIPT]: `AssemblyProperty`, `AssemblyLifecycle`, `AssemblyCost`, and `WindowU` carry each aggregation discipline's result over the kernel takeoff bundle the folds distribute per ply, each absence an `Option` and each cause a counted `PlyGap`.
- [03]-[AGGREGATION_FOLD]: `AssemblyAggregator` folds a seam composition or a runner-resolved window-part set into one typed accumulator per aggregation discipline, over one gap-accumulating carrier.

## [02]-[ASSEMBLY_RECEIPT]

- Owner: `AssemblyProperty` the thermal/mass/vapour/acoustic/mixture/fire receipt; `AssemblyLifecycle`/`AssemblyCost` the EN 15978 embodied-carbon and in-place-cost receipts; `WindowU` the EN ISO 10077-1 whole-window receipt, `WindowPart` the resolved glazed-or-frame field the thermal runner assembles and feeds the fold; `ElementTakeoff` the element geometric takeoff — the kernel `MeasureBundle` beside the `Rasm.Fabrication` off-cut column — the GWP/cost folds distribute per ply; `PlyQuantity` the optional per-`MaterialId` exact declared-quantity override an IFC `Qto_*BaseQuantities` takeoff supplies; `PlyDiscipline` the capability vocabulary naming which seam property case a ply held, `PlyGap` one ply's one missing discipline, and `PlyGaps` the `Monoid` ledger the fold accumulates.
- Cases: one `AssemblyProperty` over a `LayerSet` carrying the ISO 6946 series U (with the `Rsi`/`Rse` films), the ISO 13786 areal heat capacity, the EN ISO 13788 vapour `Sd`, the mass-law `StcWeighted` (contour-fit through the seam `RatingContour.Stc.Fit`), the effective bulk density, and the worst-rated-ply fire; one `AssemblyLifecycle` (whole-life + per-module `StageGwp`, intensity, mass-weighted recycled fraction); one `AssemblyCost` (supply/install/lifecycle over one `Currency`); one `WindowU` over a `Seq<WindowPart>` (the `Uw`, area-weighted glazed/frame sub-transmittances, the `Σ lg·Ψg` edge bridge, the `GlazedFraction` the daylight/solar-gain consumer reads) — a new aggregation rating is one fold over the same composition or window parts with one receipt column, never a parallel composite-material owner.
- Entry: the receipts mint through the `[03]-[AGGREGATION_FOLD]` folds; per-ply reads compose the seam `MaterialPropertyAccess` accessors directly (`props.Thermal`/`.Mechanical`/`.Fire`/`.Environmental`/`.Cost`) — the seam exposing the full typed accessor family so every discipline reads seam-direct (ONE_HOP), an `Option<T>` absent case the fold reports or rails, never an `is`-cast the seam owns.
- Packages: UnitsNet (`ThermalResistance` binding the ISO 6946 surface films), LanguageExt.Core (`Fin`/`Seq`/`Option`/`HashMap`/`WriterT`/`Monoid`/`TraverseM`), Thinktecture.Runtime.Extensions (the generated `MaterialComposition.Switch`/`MeasurementBasis.Switch`/`WindowPart.Switch`, `[SmartEnum<string>]`), Rasm (kernel — `MeasureBundle`/`MassKind` the takeoff carrier, `CapabilitySet<TCapability>`/`ICapability<TSelf>`, `EpsilonPolicy.ZeroTolerance` the `ToleranceLane.Identity` degeneracy anchor, `Op`), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `MaterialPropertySet`, `MaterialPropertyAccess`, `MaterialId`, `AcousticBand`, `LifecycleStage`, `Currency`, `MeasurementBasis`, `RatingContour.Stc.Fit`), the `Runtime/admission#DISPATCH_SPINE` `ComputeFault`/`AssessmentInputReason`, Generator.Equals (`[Equatable]`+`[OrderedEquality]` — the `StageGwp` latent-trap repair), CommunityToolkit.HighPerformance (`MemoryOwner<double>` the fold scratch), BCL inbox (`TensorPrimitives`, `ImmutableArray<double>` the `StageGwp` carrier).
- Growth: a new assembly rating is one `AssemblyAggregator` fold reading the same seam `MaterialPropertySet` cases into one receipt column; a new band is one seam `AcousticBand`/`LifecycleStage` row (the vector widens by data, the fold re-reads the new length); a new per-ply discipline is one `PlyDiscipline` row every coverage set and every gap row absorbs with zero fold edits.
- Boundary: receipts carry raw SI scalars, not a seam `MeasureValue` or `MaterialPropertySet` type — the receipt is the analysis input the runners read and the write-back lowers onto `AssessmentFact` values, so the aggregator never re-mints the seam value family; each ply read lifts the member's `MeasureValue.Si` so a later seam unit canonicalization never breaks the fold. Every unreachable column is an `Option`, never a `double.NaN`: a sentinel is a number that survives arithmetic, so a NaN intensity multiplied into a portfolio rollup silently NaNs the portfolio, and the consumer's `double.IsFinite` re-derivation was the presence test the carrier now states. Every absent-discipline CAUSE rides `Gaps` as a counted `(MaterialId, PlyDiscipline)` row — a partial assembly reports WHICH ply lacked WHICH discipline, where the four completeness bools reported only which column went absent. `Coverage` DERIVES from the accumulators that survived, never a hand-kept mirror. `AssemblyCost` carries no `MeasurementBasis` (the per-unit basis is consumed at the fold, the total absolute currency); `WindowPart` likewise carries raw SI scalars the thermal runner lifts from the seam — glazed/frame `U` off `Thermal.UValue.Si`, areas off `Qto_*BaseQuantities`, the spacer `Ψg` off the window's `Pset` thermal-bridge property (NOT `Thermal`, which carries no perimeter-bridge column) — the kernel folds already-resolved parts, never reading `Rasm.Materials`; an assembly property or `Uw` is computed on demand, never stored as a material.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// WHICH seam property case a ply held — the vocabulary both the coverage answer and the gap cause read. The four
// `bool Thermal`/`bool Mechanical` completeness columns this replaces stated the ANSWER twice (once as a flag, once
// as the NaN it produced) and stated the CAUSE nowhere.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlyDiscipline : ICapability<PlyDiscipline> {
    public static readonly PlyDiscipline Thermal = new("thermal");
    public static readonly PlyDiscipline Mechanical = new("mechanical");
    public static readonly PlyDiscipline Fire = new("fire");
    public static readonly PlyDiscipline Environmental = new("environmental");
    public static readonly PlyDiscipline Cost = new("cost");
}

// One ply's one missing discipline. The gap is REPORTED where the multi-physics fold decouples (a missing Thermal
// costs the U and nothing else) and the fold RAILS where the hole invalidates a sum (a missing Environmental case
// means the GWP total is not the assembly's) — the discriminant is which fold is asking, stated at each entry.
public readonly record struct PlyGap(MaterialId Material, PlyDiscipline Discipline);

// The accumulation the fold carries beside its value. Combine is fact CONCATENATION and Empty the identity, so the
// monoid is associative and NON-commutative by design: the order is ply order, which is the order a reviewer reads
// the buildup in.
public readonly record struct PlyGaps(Seq<PlyGap> Facts) : Monoid<PlyGaps> {
    public static PlyGaps Empty => new(Seq<PlyGap>());
    public static PlyGaps Of(MaterialId material, PlyDiscipline discipline) => new(Seq(new PlyGap(material, discipline)));
    public PlyGaps Combine(PlyGaps rhs) => new(Facts + rhs.Facts);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Element geometric takeoff, read once from the element's baked Qto_*BaseQuantities: the KERNEL MeasureBundle —
// whose MassKind discriminant survives on every row and whose Magnitude answers Option — beside the ONE column the
// kernel does not own. The Fabrication nesting off-cut is a SECOND area, and the bundle's distinct-kind law cannot
// hold two rows under MassKind.Area, so it rides its own slot. The local three-scalar (Area, Volume, Waste) struct
// this replaces is the shape Rasm/Analysis/measure#MEASURE rules deleted: three mutually exclusive columns
// re-derived the discriminant the Kind already carries, and every per-domain read forged a zero at the absent edge.
public readonly record struct ElementTakeoff(MeasureBundle Measures, Option<double> WasteAreaM2) {
    public Option<double> Area => Measures.Magnitude(MassKind.Area);
    public Option<double> Volume => Measures.Magnitude(MassKind.Volume);

    // The FABRICATED area: the idealized face plus its measured off-cut. Absent waste is absent NESTING EVIDENCE,
    // not a measured zero off-cut, and the effective area is then the idealized area — an absent AREA, by contrast,
    // is no area at all and the product stays absent rather than reporting the waste alone.
    public Option<double> EffectiveArea =>
        Area.Map(area => WasteAreaM2.Match(Some: waste => area + waste, None: () => area));
}

// Optional exact per-material quantity (an IFC Qto_*BaseQuantities takeoff) overriding the idealized geometry, already in the
// declared unit; keyed by MaterialId so a buildup and its constituent share one composition key, never a graph NodeId.
public readonly record struct PlyQuantity(MaterialId Material, double DeclaredQuantity);

// Every column OPTIONAL because every one is unreachable for a real buildup: a constituent mixture has no thickness
// to accumulate areal mass over and no series structure at all, a layer set missing one ply's density has no mass
// law to evaluate, and an unrated set has no fire endurance. Publishing 0 there states a MEASURED zero — a wall
// that transmits everything, a fire rating of nothing — and publishing NaN states a number that survives every
// downstream multiply. Coverage names which disciplines the whole buildup held; Gaps names every ply that did not.
public sealed record AssemblyProperty(
    Option<double> UValueWM2K,
    Option<int> StcWeighted,
    Option<double> EffectiveDensityKgM3,
    Option<int> FireResistanceMinutes,
    Option<double> ArealHeatCapacityKJM2K,
    Option<double> VapourResistanceSdM,
    CapabilitySet<PlyDiscipline> Coverage,
    Seq<PlyGap> Gaps);

// `[Equatable]` closes the latent trap: the `StageGwp` ImmutableArray compares by underlying-array REFERENCE
// under record equality, so two identical aggregations read unequal; the per-module vector orders element-wise.
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

// One closed part family preserves the glazed/frame discriminant even when a valid glazed part has zero exposed edge.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowPart {
    private WindowPart() { }

    public sealed record Glazed(double UWM2K, double AreaM2, double EdgeLengthM, double PsiWM_K) : WindowPart;
    public sealed record Frame(double UWM2K, double AreaM2) : WindowPart;
}

// EN ISO 10077-1 whole-window receipt: the area-and-perimeter-weighted Uw a single material cannot carry (a window is
// side-by-side glazing-in-frame, not a through-thickness series), plus the glazed/frame sub-transmittances, the edge
// bridge, and the glazed fraction. A frameless assembly has no frame sub-transmittance and says so.
public sealed record WindowU(double UwWM2K, Option<double> UgWM2K, Option<double> UfWM2K, double EdgeBridgeW_K, double GlazedFraction);
```

## [03]-[AGGREGATION_FOLD]

- Owner: `AssemblyAggregator` the static fold kernel over a seam `MaterialComposition` — `Aggregate` (thermal/mass/vapour/acoustic/mixture/fire), `AggregateEnvironmental` (EN 15978 GWP), `AggregateCost` (basis-aware cost), `AggregateWindow` (EN ISO 10077-1 whole-window `Uw` over a `Seq<WindowPart>`); each composition fold discriminates through the seam's generated total `Switch` and traverses each ply's `MaterialPropertySet`, resolved through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>`, into a typed `[FOLD_STATE]` accumulator that `Absorb`s one admitted ply and `Project`s the receipt; `Plies` the `WriterT<PlyGaps, Fin, A>` carrier vocabulary every per-ply read reports through.
- Entry: `Aggregate(composition, resolve)` over a `LayerSet`/`ConstituentSet` (a `Single`/`ProfileSet` rails — no series structure to aggregate, its intrinsic U/STC read seam-direct), with `AggregateEnvironmental(composition, resolve, overrides, geometry)` and `AggregateCost(…)` over any composition; `AggregateWindow(parts)` folds a `Seq<WindowPart>` into the `WindowU`. Every entry returns the receipt alone — the gap ledger the run accumulated rides ON the receipt, so a caller never threads a second output and never re-runs the fold to learn the cause.
- Auto: the resolver reads a ply node's seam property cases keyed on `MaterialId`, not a graph `NodeId`, so the fold reads the composition's own plies; per-ply resolution runs through `TraverseM` over the writer carrier, so each ply's admitted contribution and its gap facts land in one pass and the accumulator fold that follows is PURE and total. The mass-law STC evaluates `R(f)=20·log₁₀(m'·f)−47` at each seam `AcousticBand` centre through one `TensorPrimitives` chain over one leased buffer and feeds the vector ONCE through the seam `RatingContour.Stc.Fit`, so the assembly STC and the single-material STC share one ASTM-E413 contour-fit owner. The `Rsi`/`Rse` films (ISO 6946) are bound `UnitsNet.ThermalResistance` values whose one SI projection seeds the resistance at the building-envelope ends. The GWP and cost folds share ONE basis-aware `DeclaredQuantity` derivation off the seam `Environmental`/`Cost` `Basis` unless a `PlyQuantity` overrides, and the recycled-content fraction weights each ply by its mass `ρ·V`, excluding a density-less ply and NAMING it in the ledger.
- Packages: UnitsNet, LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel — `MeasureBundle`/`MassKind`, `CapabilitySet`, `EpsilonPolicy`, `Op`), Rasm.Element (project — the seam composition and property vocabulary), the `Runtime/admission#DISPATCH_SPINE` fault family, CommunityToolkit.HighPerformance, BCL inbox (`TensorPrimitives`).
- Growth: a new aggregation discipline is one `AssemblyAggregator` fold reading the same seam cases into a typed `[FOLD_STATE]` accumulator; the generated total `Switch` over the closed composition family breaks at compile time if the seam adds a composition case, never a runtime-silent `_` arm.
- Boundary: the constituent fraction rides the seam `MaterialConstituent.Fraction` (normalized to unity at seam admission, the aggregator never re-guarding) and the resolver keys on `MaterialId`, so a `NodeId`-keyed lookup cannot reach a composition's own plies. An absent ply property in the multi-physics `Aggregate` is DECOUPLED per discipline — a missing `Thermal` costs only `U`/`Sd`/heat-capacity, a missing `Mechanical` only the mass-derived fields — and ONLY a missing material NODE rails; the single-discipline `AggregateEnvironmental`/`AggregateCost` DO rail on a missing `Environmental`/`Cost` case (a hole invalidates the sum) and on a per-kg basis over a density-less material. A discipline's accumulator goes ABSENT the moment one ply lacks it and never recovers, which is the "every ply must hold it" law stated by the carrier rather than by a `bool` conjunction beside twelve fabricated zeros — and the ply that broke it is named in `Gaps`. Worst-ply fire is the MINIMUM over the plies that carry an EN 13501-2 load-bearing rating; an unrated ply is non-limiting and COUNTED, and an entirely unrated set reports absence, where the `double.MaxValue` sentinel and its `? 0.0` collapse reported the worst possible rating from no evidence at all. A conductivity at or below the kernel `ToleranceLane.Identity` degeneracy anchor is ABSENT thermal evidence rather than a floored divisor: `double.Epsilon` as a physical λ yields `R ≈ 1e300` and reports a wall as a perfect insulator. Cost is over one `Currency` (a mismatch rails, the fold carrying no exchange rate); the mass-law STC contour-fits through the seam kernel (the naive per-leaf dB sum, which over-predicts a rigidly-bonded set, the deleted form); a `Single`/`ProfileSet` rails explicitly and an EMPTY `LayerSet`/`ConstituentSet` rails the same fault, because the vacuous seed otherwise projects a films-only `U` over full coverage. Same-material plies coalesce by SUM in FIRST-APPEARANCE order — a hash-map enumeration order is unspecified, so summing in map order makes the floating-point total depend on hash layout and a content-keyed receipt stops reproducing. `AggregateWindow` is the EN ISO 10077-1 side-by-side fold: it area-weights the glazed `Ug` (EN 673 center-of-glass off `Thermal.UValue`, never folded raw as a single-material whole-window U — the deleted form) and frame `Uf` and ADDS the `Σ lg·Ψg` edge-seal bridge the layer series cannot reach, a frame part zeroing its edge term; the kernel folds the runner-resolved parts pure, never reading a graph node.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The gap-accumulating carrier every per-ply read reports through: Fin refuses, PlyGaps accumulates, and the two
// stack rather than folding evidence into the failure payload. Held is the clean pass, Missing the report, Lift the
// door a railed resolver enters by, and Run the ONE egress handing the value and the whole ledger together.
public static class Plies {
    public static WriterT<PlyGaps, Fin, A> Held<A>(A value) => WriterT.pure<PlyGaps, Fin, A>(value);
    public static WriterT<PlyGaps, Fin, A> Missing<A>(MaterialId material, PlyDiscipline discipline, A value) =>
        WriterT.write<PlyGaps, Fin, A>(value, PlyGaps.Of(material, discipline));
    public static WriterT<PlyGaps, Fin, A> Lift<A>(Fin<A> rail) => WriterT.lift<PlyGaps, Fin, A>(rail);
    public static Fin<(A Value, PlyGaps Log)> Run<A>(WriterT<PlyGaps, Fin, A> writer) => writer.Run().As();

    // ONE read shape: a present value passes clean, an absent one passes ABSENT and names itself in the ledger. The
    // twelve `.IfNone(0.0)` sites this replaces each turned an absent discipline into a MEASURED zero inside a
    // running sum, then let a separate bool decide afterwards whether that sum was meaningful — two representations
    // of one fact, disagreeing wherever the bool was forgotten.
    public static WriterT<PlyGaps, Fin, Option<A>> Reading<A>(MaterialId material, PlyDiscipline discipline, Option<A> read) =>
        read.Match(Some: value => Held(Some(value)), None: () => Missing(material, discipline, Option<A>.None));
}

public static class AssemblyAggregator {
    // ISO 6946 surface films BIND their quantity rather than carrying the unit in a comment: a bare double whose
    // unit lives beside it is the one shape a resistance can be summed into a conductance without complaint. Both
    // mint at the tabulated magnitude in their own unit row, and the series fold reads ONE projection at the seed
    // rather than paying a conversion per ply.
    private static readonly ThermalResistance Rsi = ThermalResistance.From(0.13, ThermalResistanceUnit.SquareMeterKelvinPerWatt);
    private static readonly ThermalResistance Rse = ThermalResistance.From(0.04, ThermalResistanceUnit.SquareMeterKelvinPerWatt);

    private static readonly double FilmResistanceM2KW =
        Rsi.As(ThermalResistanceUnit.SquareMeterKelvinPerWatt) + Rse.As(ThermalResistanceUnit.SquareMeterKelvinPerWatt);
    private const double MassLawConstantDb = 47.0;  // field-incidence mass law: R(f) = 20·log10(m'·f) − 47 dB

    // Band centres in ROSTER DECLARATION order — the order RatingContour.Stc.Fit reads the vector in — never indexed
    // by the row's integer Key. The key is the band index TODAY and a dense 0..17 run is asserted nowhere, so a
    // keyed write into a count-sized buffer is one inserted row away from an out-of-range write or a transposed
    // spectrum the contour fit would happily rate.
    private static readonly ImmutableArray<double> BandCentresHz = [.. AcousticBand.Items.Select(static band => band.CenterHz)];

    private static readonly Op ContourKey = Op.Of(name: nameof(Stc));

    // Series-U + areal heat capacity + vapour Sd + mass-law STC over a LayerSet, Fraction-weighted rule-of-mixtures
    // over a ConstituentSet, worst-rated-ply fire over either; a Single/ProfileSet rails (no plies). State threads
    // positionally through the seam's generated Switch.
    public static Fin<AssemblyProperty> Aggregate(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        composition.Switch(
            resolve,
            single:         static (_, s) => Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionShape, s.Material.Value)),
            profileSet:     static (_, s) => Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionShape, s.Material.Value)),
            layerSet:       static (r, set) => AggregateLayers(set, r),
            constituentSet: static (r, set) => AggregateConstituents(set, r));

    // Each LayerSet discipline folds INDEPENDENTLY: a layer with no Thermal costs only U/Sd/heat-capacity, no
    // Mechanical density only the mass-derived fields — never aborting the U a thermal runner reads. Only a missing
    // material NODE rails; a missing property is a COUNTED absence. Zero plies is no series, so an EMPTY layer set
    // rails — its vacuous seed otherwise projects a films-only U = 1/(Rsi+Rse) receipt over full coverage, the
    // AggregateWindow empty-rail sibling.
    private static Fin<AssemblyProperty> AggregateLayers(MaterialComposition.LayerSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Layers.IsEmpty
            ? Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionEmpty, nameof(MaterialComposition.LayerSet)))
            : Plies.Run(set.Layers.TraverseM(layer => LayerPlyOf(layer, resolve)).As()
                    .Map(static plies => plies.Fold(LayerFold.Seed, static (state, ply) => state.Absorb(ply))))
                .Bind(static run => run.Value.Project(run.Log.Facts));

    // ConstituentSet plies fold through the Fraction-weighted rule-of-mixtures. Series fields (U/heat-capacity/Sd)
    // and the thickness-less STC are structurally ABSENT rather than NaN — a homogeneous mix has no series to have
    // one — and only a missing NODE or the empty set rails.
    private static Fin<AssemblyProperty> AggregateConstituents(MaterialComposition.ConstituentSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Constituents.IsEmpty
            ? Fin.Fail<AssemblyProperty>(Missing(AssessmentInputReason.CompositionEmpty, nameof(MaterialComposition.ConstituentSet)))
            : Plies.Run(set.Constituents.TraverseM(c => MixturePlyOf(c, resolve)).As()
                    .Map(static plies => plies.Fold(MixtureFold.Seed, static (state, ply) => state.Absorb(ply))))
                .Map(static run => run.Value.Project(run.Log.Facts));

    // EN 15978 embodied carbon over ANY composition: each ply's per-module StageGwp scaled by the BASIS-matching
    // quantity through the same DeclaredQuantity derivation the cost fold uses, so a per-m² membrane or per-kg steel
    // EPD folds without a forced per-m³ normalization; recycled content weights by ply mass ρ·V and a density-less
    // ply is EXCLUDED AND COUNTED rather than silently contributing zero to both halves of the quotient. An empty
    // composition yields zero plies and would map the untouched seed onto a fabricated zero-GWP lifecycle, so absent
    // plies rail — only a NON-EMPTY zero-impact composition can honestly report zero.
    public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementTakeoff geometry) =>
        PliesByVolume(composition, geometry).Bind(plies => plies.IsEmpty
            ? Fin.Fail<AssemblyLifecycle>(Missing(AssessmentInputReason.CompositionEmpty, nameof(AggregateEnvironmental)))
            : Plies.Run(plies.TraverseM(ply => CarbonPlyOf(ply, resolve, overrides, geometry)).As())
                .Map(run => Carbon(run.Value, geometry, run.Log.Facts)));

    // Basis-aware supply/install/lifecycle rollup over ANY composition + a single Currency: the Cost.Basis selects
    // the geometric quantity the per-unit price scales by; a currency mismatch, a per-kg density-less ply, or a
    // missing Cost rails. The empty guard is the environmental fold's, spelled once here rather than left to fall
    // through to a different fault the way the two arms once disagreed.
    public static Fin<AssemblyCost> AggregateCost(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementTakeoff geometry) =>
        PliesByVolume(composition, geometry).Bind(plies => plies.IsEmpty
            ? Fin.Fail<AssemblyCost>(Missing(AssessmentInputReason.CompositionEmpty, nameof(AggregateCost)))
            : plies.TraverseM(ply => CostPlyOf(ply, resolve, overrides, geometry)).As()
                .Bind(static priced => priced.Fold(Fin.Succ(Option<AssemblyCost>.None),
                        static (acc, ply) => acc.Bind(running => Accumulate(running, ply.Cost, ply.Quantity)))
                    .Bind(static o => o.ToFin(Missing(AssessmentInputReason.CompositionEmpty, nameof(AggregateCost))))));

    // EN ISO 10077-1 whole-window Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af) over the runner-resolved
    // glazed/frame parts — the side-by-side composition the through-thickness layer series cannot reach. An empty
    // part set and a zero total area rail their OWN reason rows, so a caller distinguishes "nothing was resolved"
    // from "everything resolved to nothing" without parsing a slug.
    public static Fin<WindowU> AggregateWindow(Seq<WindowPart> parts) =>
        parts.IsEmpty
            ? Fin.Fail<WindowU>(Missing(AssessmentInputReason.WindowFieldAbsent, string.Empty))
            : parts.Fold(WindowFold.Seed, static (state, part) => state.Absorb(part)).Project();

    // --- [PLY_ADMISSION]
    // Per-ply reads compose the seam MaterialPropertyAccess accessors directly — never a re-derived is-cast; the
    // seam exposes the FULL typed accessor family, so every read is ONE_HOP and every absence is a named gap.
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
        // A missing Environmental case RAILS: a hole in a GWP sum makes the total a different assembly's, where a
        // hole in the multi-physics fold costs only the column it feeds.
        from env in Plies.Lift(props.Environmental.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, ply.Material.Value)))
        from quantity in Plies.Lift(Quantity(env.Basis, ply, overrides, props, geometry))
        from density in Plies.Reading(ply.Material, PlyDiscipline.Mechanical, props.Mechanical.Map(static m => m.Density.Si))
        // RecycledContent is Option on the seam — an EPD that declares no recycled fraction is not an EPD declaring
        // zero — so the recycled mass is present only where BOTH the fraction and the ply mass are.
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

    // A conductivity at or below the kernel degeneracy anchor is ABSENT thermal evidence, never a floored divisor.
    // The floor is ToleranceLane.Identity's own value (EpsilonPolicy.ZeroTolerance) — the identity-residual anchor
    // stated once at the kernel — because the BCL denormal the guard once used (~5e-324) yields R ≈ 1e300 from a
    // λ of 1e-300 and publishes a wall as a perfect insulator, which the mass-law and heat-capacity columns then
    // inherit. The three thermal magnitudes travel together because they come from one seam case.
    private static Option<ThermalPly> Conductive(Seq<MaterialPropertySet> props) =>
        props.Thermal
            .Filter(static t => t.Conductivity.Si > EpsilonPolicy.ZeroTolerance)
            .Map(static t => new ThermalPly(t.Conductivity.Si, t.SpecificHeat.Si, t.VapourResistanceFactor));

    // Worst-ply fire reads the EN 13501-2 R (load-bearing) criterion off the seam FireResistance, whose minutes are
    // Option<int> — an unrated ply has no rating, and the (double) cast this replaces coerced that absence.
    private static Option<int> FireMinutes(Seq<MaterialPropertySet> props) =>
        props.Fire.Bind(static f => f.Resistance.LoadBearingMinutes);

    // ONE quantity derivation both value folds share: the declared override where the caller supplied one, the
    // basis-matched geometric quantity otherwise.
    private static Fin<double> Quantity(
        MeasurementBasis basis, (MaterialId Material, double VolumeM3) ply, Seq<PlyQuantity> overrides,
        Seq<MaterialPropertySet> props, ElementTakeoff geometry) =>
        overrides.Find(q => q.Material == ply.Material).Map(static q => q.DeclaredQuantity).Match(
            Some: Fin.Succ,
            None: () => DeclaredQuantity(basis, ply.VolumeM3, geometry, props.Mechanical.Map(static m => m.Density.Si), ply.Material));

    // Seam `Basis` selects the geometric quantity the per-unit price or impact scales by; per-kg without a resolved
    // density rails (mass is unresolvable), never a silent zero. Total over the four basis rows.
    private static Fin<double> DeclaredQuantity(MeasurementBasis basis, double volumeM3, ElementTakeoff geometry, Option<double> density, MaterialId material) =>
        basis.Switch(
            (volumeM3, geometry, density, material),
            perM3:   static s => s.volumeM3 > 0.0 ? Fin.Succ(s.volumeM3) : Fin.Fail<double>(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value)),
            perM2:   static s => s.geometry.EffectiveArea.Filter(static area => area > 0.0).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value)),
            perItem: static _ => Fin.Succ(1.0),
            perKg:   static s => s.volumeM3 > 0.0
                ? s.density.Map(d => s.volumeM3 * d).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value))
                : Fin.Fail<double>(Missing(AssessmentInputReason.DeclaredUnitBasis, s.material.Value)));

    // Per-ply geometric volume follows composition shape: a Single/ProfileSet uses element volume, a layer its
    // thickness × effective face area, a constituent its fraction of volume — one closed Switch the GWP and cost
    // folds share. Each arm reads the kernel bundle's own Option, so an unmeasured domain rails here rather than
    // reaching a fold as a fabricated zero volume.
    private static Fin<Seq<(MaterialId Material, double VolumeM3)>> PliesByVolume(MaterialComposition composition, ElementTakeoff geometry) =>
        composition.Switch(
            geometry,
            single:         static (g, s) => g.Volume.Map(v => Seq((s.Material, v))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.Material.Value)),
            profileSet:     static (g, s) => g.Volume.Map(v => Seq((s.Material, v))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, s.Material.Value)),
            layerSet:       static (g, s) => g.EffectiveArea.Map(a => s.Layers.Map(l => (l.Material, l.Thickness.Si * a))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, nameof(MassKind.Area))),
            constituentSet: static (g, s) => g.Volume.Map(v => s.Constituents.Map(c => (c.Material, c.Fraction * v))).ToFin(Missing(AssessmentInputReason.DeclaredUnitBasis, nameof(MassKind.Volume))))
        .Map(Coalesce);

    // Same-material plies coalesce by SUM, and the surviving order is FIRST APPEARANCE. A HashMap enumeration order
    // is unspecified, so folding the GWP total in map order makes the floating-point sum depend on hash layout — and
    // a receipt the assessment content key addresses must reproduce bit-for-bit across runs.
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
    // Field-incidence mass law over the layer set's accumulated areal mass m' (kg·m⁻²): R(f) = 20·log10(m'·f) − 47,
    // evaluated at each seam AcousticBand centre into the per-band SRI vector the seam RatingContour.Stc.Fit
    // contour-fits — so the assembly STC and the single-material STC share ONE ASTM-E413 owner, never the unphysical
    // per-leaf dB sum. The whole evaluation is a four-operator span chain over ONE pooled buffer that never escapes,
    // so a per-element takeoff over a large model allocates nothing per band and nothing per assembly.
    private static Fin<int> Stc(double massKgM2) {
        using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(BandCentresHz.Length);
        Span<double> sri = scratch.Span;
        TensorPrimitives.Multiply(BandCentresHz.AsSpan(), massKgM2, sri);
        TensorPrimitives.Log10(sri, sri);
        TensorPrimitives.MultiplyAdd(sri, 20.0, -MassLawConstantDb, sri);
        TensorPrimitives.Max(sri, 0.0, sri);
        return RatingContour.Stc.Fit(sri, ContourKey);
    }

    // Per-module accumulation is ONE fused multiply-add pass into ONE leased buffer, so the whole-life total reads
    // off the same span it was built in. The fresh per-ply array this replaces allocated once PER PLY per element
    // per assessment to preserve an immutability the lease already gives — the buffer is local to this fold and the
    // ImmutableArray the receipt carries is materialized once, at the end, from a span no caller can reach.
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
            // An area-less element has no intensity to report and a mass-less buildup no recycled fraction: both are
            // honestly ABSENT, where a 0 reads as a MEASURED zero publishing a carbon-free element and a wholly
            // virgin one, and a NaN reads as a number that survives every downstream rollup multiply.
            EmbodiedCarbonIntensityKgCo2eM2: geometry.Area.Filter(static area => area > 0.0).Map(area => wholeLife / area),
            RecycledContentFraction: mass.Filter(static total => total > 0.0).Bind(total => recycled.Map(share => share / total)),
            Gaps: gaps);
    }

    // Additive accumulation over an optional term: a present addend advances a present-or-absent running total, and
    // an absent addend leaves it untouched. Absence here is a ply excluded from the weighting, never a zero added.
    private static Option<double> Combine(Option<double> running, Option<double> addend) =>
        addend.Match(Some: value => Some(running.Match(Some: held => held + value, None: () => value)), None: () => running);

    // --- [FOLD_STATE] ---------------------------------------------------------------------
    // Typed per-discipline accumulators co-locate algorithm state with the kernel: each Absorbs one ADMITTED ply and
    // Projects the receipt. Every discipline column is an Option ACCUMULATOR, so the "every ply must hold this
    // discipline" law is the carrier's — one absent ply takes the accumulator absent and it never recovers — and the
    // four bool completeness flags that re-derived exactly that conjunction have no work left to do.
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
            // Worst-rated-ply fire is the MINIMUM over the plies that CARRY a rating: an unrated ply is
            // non-limiting and already counted in the ledger, and an entirely unrated set reports absence rather
            // than the worst possible rating derived from no evidence.
            MinFireMinutes = Least(MinFireMinutes, ply.FireMinutes),
        };

        public CapabilitySet<PlyDiscipline> Coverage => Covered(ResistanceM2KW.IsSome, MassKgM2.IsSome, MinFireMinutes.IsSome);

        public Fin<AssemblyProperty> Project(Seq<PlyGap> gaps) =>
            MassKgM2.Filter(static mass => mass > 0.0).Map(Stc).Sequence().Map(stc => new AssemblyProperty(
                UValueWM2K:             ResistanceM2KW.Filter(static r => r > 0.0).Map(static r => 1.0 / r),
                StcWeighted:            stc,
                // Bulk density is the areal mass over the accumulated thickness; a zero-thickness set has no
                // thickness to divide by and reports absence rather than a division that produces infinity.
                EffectiveDensityKgM3:   ThicknessM > 0.0 ? MassKgM2.Map(mass => mass / ThicknessM) : None,
                FireResistanceMinutes:  MinFireMinutes,
                ArealHeatCapacityKJM2K: HeatJM2K.Map(static joules => joules / 1000.0),
                VapourResistanceSdM:    SdM,
                Coverage:               Coverage,
                Gaps:                   gaps));
    }

    private readonly record struct MixtureFold(Option<double> DensityKgM3, Option<int> MinFireMinutes) {
        public static MixtureFold Seed => new(Some(0.0), None);

        // The Fraction-weighted Voigt average Σ(f_i·ρ_i) — the iso-strain rule-of-mixtures estimate for a composite.
        public MixtureFold Absorb(MixturePly ply) => this with {
            DensityKgM3 = DensityKgM3.Bind(held => ply.DensityKgM3.Map(d => held + ply.Fraction * d)),
            MinFireMinutes = Least(MinFireMinutes, ply.FireMinutes),
        };

        // A homogeneous mixture has NO series structure, so the series columns are structurally absent rather than
        // computed: there is no through-thickness order to accumulate a resistance, a heat capacity, or a vapour
        // path along, and no thickness for the mass law to read an areal mass off.
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

    // EN ISO 10077-1 whole-window accumulator carries the area-weighted glazed/frame conductance numerators
    // (Σ Ag·Ug, Σ Af·Uf) and their areas, plus the perimeter edge-seal bridge Σ lg·Ψg — a glazed part contributes
    // all three, a frame part only its area·Uf and area.
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

        // A zero total area rails rather than dividing: a 0/0 Uw is a NaN the verdict band would report as
        // not-applicable exactly as though the check had run and found no target.
        public Fin<WindowU> Project() {
            double totalArea = GlazedArea + FrameArea;
            return totalArea <= 0.0
                ? Fin.Fail<WindowU>(Missing(AssessmentInputReason.WindowZeroArea, string.Empty))
                : Fin.Succ(new WindowU(
                    UwWM2K:         (GlazedUA + FrameUA + EdgeBridge) / totalArea,
                    // A frameless assembly has no frame sub-transmittance and an all-frame one no glazed
                    // sub-transmittance; the absence is the honest answer, not a NaN the consumer re-tests.
                    UgWM2K:         GlazedArea > 0.0 ? Some(GlazedUA / GlazedArea) : None,
                    UfWM2K:         FrameArea > 0.0 ? Some(FrameUA / FrameArea) : None,
                    EdgeBridgeW_K:  EdgeBridge,
                    GlazedFraction: GlazedArea / totalArea));
        }
    }

    // The two column-shape helpers both accumulators share, so the minimum rule and the coverage derivation are each
    // spelled once rather than once per fold.
    private static Option<int> Least(Option<int> running, Option<int> candidate) =>
        candidate.Match(Some: minutes => Some(running.Match(Some: held => int.Min(held, minutes), None: () => minutes)), None: () => running);

    private static CapabilitySet<PlyDiscipline> Covered(bool thermal, bool mechanical, bool fire) =>
        CapabilitySet<PlyDiscipline>.Of([.. Seq(
            (PlyDiscipline.Thermal, thermal), (PlyDiscipline.Mechanical, mechanical), (PlyDiscipline.Fire, fire))
            .Filter(static row => row.Item2).Map(static row => row.Item1)]);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
