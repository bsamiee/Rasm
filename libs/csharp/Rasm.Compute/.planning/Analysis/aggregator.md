# [COMPUTE_AGGREGATOR]

THE MULTI-PLY ASSEMBLY-AGGREGATION ENGINE — relocated from `Rasm.Materials` to the Compute analysis rail because layered-construction property aggregation is ANALYSIS, not material authoring (`§6`: the closed-form physics live in Compute, never the seam). One `AssemblyAggregator` static kernel folds a `Rasm.Element` seam `MaterialComposition` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) into one receipt per aggregation discipline, resolving each ply's seam `MaterialPropertySet` cases through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` resolver keyed on the composition's NATIVE `MaterialId` (never a parallel graph `NodeId`): a series-resistance U-value plus the `Σ ρ·c·t` dynamic thermal mass and the `Σ μ·t` vapour-diffusion `Sd` (ISO 6946 / ISO 13786 / EN ISO 13788) and a field-incidence mass-law sound-reduction index over the layer set's accumulated areal mass `m' = Σ(ρ·t)` (the per-band `R(f)=20·log₁₀(m'·f)−47` evaluated at the seam `AcousticBand` one-third-octave centres, fed ONCE through the SEAM `RatingContour.Stc.Fit` so the assembly STC and the single-material STC share one ASTM-E413 contour-fit owner) and the in-plane Voigt effective Young's modulus `Σ(t·E)/ΣT` over a `LayerSet`; a constituent-`Fraction`-weighted rule-of-mixtures effective density/conductivity/Young's modulus (the IFC `IfcMaterialConstituentSet.Fraction` mixture rule, read directly off `MaterialConstituent.Fraction`) over a `ConstituentSet`; a worst-ply fire envelope over either; the EN 15978 per-module embodied-carbon sum and the basis-aware supply/install/lifecycle cost rollup over ANY composition; AND the EN ISO 10077-1 area-and-perimeter-weighted WHOLE-WINDOW transmittance `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` over a window's resolved glazed/frame fields — the assembly fold the through-thickness series-U cannot reach because a window is a SIDE-BY-SIDE composition (glazing pane in a frame) whose linear edge-seal bridge `Ψg` and frame fraction the perpendicular layer series ignores: `GlazingSection.Performance` (the `Rasm.Materials` `Component/glazing` IGU) yields the EN 673 CENTER-OF-GLASS `Ug` and lowers it onto the seam `MaterialPropertySet.Thermal.UValue`, the spacer's `SpacerType.PsiWmK` linear thermal bridge lowers onto a window `Pset` thermal-bridge property, and THIS kernel reads those seam-direct (the glazing `Ug` and the frame `Uf` off each part's `Thermal.UValue`, the `Ψg` + glazed area `Ag` + frame area `Af` + visible-glazing edge length `lg` off the window's baked `Qto_*BaseQuantities`/`Pset` fields the runner assembles) and composes the whole-window `Uw` no single material carries. An assembly property is COMPUTED from its plies on demand, never stored as a composite material: the `AssemblyProperty`/`AssemblyLifecycle`/`AssemblyCost` receipts are the layered-physics inputs the `Analysis/physics` thermal/acoustic/fire runner and the `Analysis/lifecycle` LCA/cost runner read, never a second material library re-keyed per assembly. The kernel reads the seam vocabulary settled — `MaterialComposition`/`MaterialLayer`/`MaterialConstituent` from `Rasm.Element/Composition/material`, the `MaterialPropertySet` `[Union]` cases through the seam `MaterialPropertyAccess` accessors (`props.Thermal`/`props.Mechanical`/`props.Environmental`), the `AcousticBand`/`LifecycleStage` band vocabularies, the `RatingContour.Stc.Fit` single-number contour kernel from `Rasm.Element/Composition/acoustic`, the `MeasurementBasis`/`Currency` cost axes, and the `MaterialLayer.Thickness` `MeasureValue` thickness read SI-native through `.Si` — and rails a missing ply MATERIAL NODE (and a missing single-discipline GWP or cost input) onto the one `ComputeFault.AssessmentInputMissing` band, while a multi-physics field whose discipline a ply lacks projects a per-discipline `double.NaN` (the `IsFinite` not-applicable signal `AssessmentVerdict.FromRatio` bands) so a thermal `U` (resolved from each ply's `Thermal` case) is never coupled to a ply missing its `Mechanical` density (which only NaNs the mass-derived `StcWeighted`/`EffectiveDensity`/`ArealHeatCapacity` fields), an under-specified buildup thus a typed not-applicable the analysis surfaces, never a silently-wrong envelope.

## [01]-[INDEX]

- [01]-[ASSEMBLY_RECEIPT]: the `AssemblyProperty` thermal/mass/vapour/acoustic/mixture/fire receipt, the `AssemblyLifecycle`/`AssemblyCost` embodied-carbon and in-place-cost receipts, the `WindowU` EN ISO 10077-1 whole-window-transmittance receipt with its `WindowField` glazed/frame field input, the `ElementQuantity` geometric takeoff the GWP/cost folds distribute per ply, and the optional `PlyQuantity` exact per-`MaterialId` override.
- [02]-[AGGREGATION_FOLD]: the `AssemblyAggregator` static kernel — `Aggregate` (series-U + heat-capacity + vapour-Sd + mass-law-STC + rule-of-mixtures + fire), `AggregateEnvironmental` (EN 15978 GWP), `AggregateCost` (basis-aware in-place cost), and `AggregateWindow` (EN ISO 10077-1 area-and-perimeter-weighted whole-window `Uw` over the resolved glazed/frame fields), each folding the per-ply/per-field seam properties into a typed `[FOLD_STATE]` accumulator (the composition folds a total `Switch`, the window fold the `WindowFold` over the fields the runner resolved from the parts).

## [02]-[ASSEMBLY_RECEIPT]

- Owner: `AssemblyProperty` the thermal/mass/vapour/acoustic/mixture/fire aggregation receipt; `AssemblyLifecycle`/`AssemblyCost` the EN 15978 embodied-carbon and in-place-cost receipts; `WindowU` the EN ISO 10077-1 whole-window-transmittance receipt (the area-weighted `Uw` plus the glazed/frame breakdown the façade designer reads), with `WindowField` the resolved glazed-or-frame field (`Ug`/`Uf`, area, and the glazed field's edge length + spacer `Ψg`) the thermal runner assembles from the window's parts and feeds the fold; `ElementQuantity` the element geometric takeoff (`AreaM2` + `VolumeM3`) the GWP/cost folds distribute per ply; `PlyQuantity` the optional per-`MaterialId` exact declared-quantity override an IFC `Qto_*BaseQuantities` takeoff supplies in place of the idealized geometry.
- Cases: one `AssemblyProperty` over a `LayerSet` — the `UValueWM2K` (ISO 6946 series resistance with the `Rsi`/`Rse` films), the `ArealHeatCapacityKJM2K` (ISO 13786 `Σ ρ·c·t` dynamic thermal mass), the `VapourResistanceSdM` (EN ISO 13788 `Σ μ·t` equivalent-air-layer diffusion resistance), the `StcWeighted` (the field-incidence mass-law SRI over the accumulated areal mass, contour-fit through the seam `RatingContour.Stc.Fit`), the `EffectiveDensityKgM3`/`EffectiveConductivityWMK`/`EffectiveYoungsModulusPa` (the effective bulk density/conductivity/in-plane modulus — series-and-mass over a `LayerSet`, `Fraction`-weighted Voigt rule-of-mixtures over a `ConstituentSet`), the `FireResistanceMinutes` (the minimum ply rating); one `AssemblyLifecycle` (the `WholeLifeGwpKgCo2e`, the per-module `StageGwp` breakdown, the `EmbodiedCarbonIntensityKgCo2eM2`, the mass-weighted `RecycledContentFraction`); one `AssemblyCost` (the supply/install/lifecycle totals over a single `Currency`); one `WindowU` over a `Seq<WindowField>` — the EN ISO 10077-1 whole-window `UwWM2K = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)`, the area-weighted glazed `UgWM2K` and frame `UfWM2K` sub-transmittances, the `EdgeBridgeW_K = Σ lg·Ψg` perimeter-bridge term, and the `GlazedFraction = Σ Ag / (Σ Ag + Σ Af)` the daylight/solar-gain consumer reads — a new aggregation rating is one fold over the same composition (or window fields) + one receipt column, never a parallel composite-material owner.
- Entry: the receipts are minted by the `[03]-[AGGREGATION_FOLD]` `AssemblyAggregator` folds; the per-ply property reads COMPOSE the seam `MaterialPropertyAccess` accessors (`props.Thermal`/`props.Mechanical`/`props.Fire`/`props.Environmental`/`props.Cost`) directly — the aggregator re-derives no `is`-cast accessor the seam owns (ONE_HOP), the seam now exposing the FULL typed accessor family so `Fire`/`Cost` read seam-direct like every other discipline, an `Option<T>` carrying an absent case the fold rails.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`), Thinktecture.Runtime.Extensions (the generated `MaterialComposition.Switch` + `MeasurementBasis.Switch` the kernel dispatches), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `MaterialPropertySet`, `MaterialPropertyAccess`, `MaterialId`, `AcousticBand`, `LifecycleStage`, `Currency`, `MeasurementBasis`, `RatingContour.Stc.Fit`), BCL inbox (`ImmutableArray<double>` the seam-aligned `StageGwp` receipt carrier, `ReadOnlySpan<double>` the fold transient).
- Growth: a new assembly rating is one `AssemblyAggregator` fold reading the same seam `MaterialPropertySet` cases — a thermal-bridge psi-value, a dynamic decrement factor, a flanking sound-reduction term each lands as one fold and one receipt column, never a parallel composite owner; a new band is one seam `AcousticBand`/`LifecycleStage` row (the seam vector widens by data, the fold re-reads the new index).
- Boundary: the receipts carry RAW SI scalars (`W·m⁻²·K⁻¹`, `W·m⁻¹·K⁻¹`, `kJ·m⁻²·K⁻¹`, m, m², dB, `kg·m⁻³`, `Pa`, kgCO2e, monetary), NOT a seam `MeasureValue` or a `MaterialPropertySet` type — the receipt is the analysis input the discipline runners read and the write-back lowers onto `AssessmentFact` typed values, so the aggregator never re-mints the seam value family; the ply reads lift each `MaterialPropertySet` member's `MeasureValue.Si` SI scalar (`Thermal.Conductivity.Si`/`Mechanical.Density.Si`) so a later seam unit canonicalization never breaks the fold; an `AssemblyProperty` is never stored as a material — an assembly's U-value/STC/effective density is computed from its plies on demand, the receipt the analysis input, never a second `MaterialLibrary`-style row table; `AssemblyCost` carries NO `MeasurementBasis` (the per-unit basis is consumed at the fold, the total is absolute currency), the migration source's basis-on-the-total field being the deleted form; the `WindowField` likewise carries RAW SI scalars (`UWM2K`/`AreaM2`/`EdgeLengthM`/`PsiWM_K`) the thermal runner LIFTS from the seam — the glazed/frame `U` off each part material's `Thermal.UValue.Si` (the IGU `Ug` `GlazingSection.Performance` lowered, the frame `Uf`), the areas off the window's baked `Qto_*BaseQuantities` (`GlazingArea`/`Area`), the spacer `Ψg` off the window's `Pset` thermal-bridge property (`GlazingSection`'s `SpacerType.PsiWmK` lowered there, NOT onto `MaterialPropertySet.Thermal` which carries no perimeter-bridge column) — so the kernel folds already-resolved fields and never reads `Rasm.Materials` (the AEC-domain peer Compute never references), the seam material + the baked bags the ONLY ingress; `WindowU` is never stored as a material — a window's `Uw` is computed from its glazed/frame fields on demand.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The element geometric takeoff the GWP/cost folds distribute per ply: a layer scales by its own
// thickness × Area, a constituent by its fraction × Volume, a single/profile material by Volume.
// The composition root reads it once from the element's baked Qto_*BaseQuantities.
// WasteAreaM2 is the Rasm.Fabrication Nesting/stock -> Compute decode-side ingress row: the seam
// NestYield.WasteAreaMm2 (decoded at the lifecycle lane, SI-coerced) joins the element's material basis so
// off-cut waste rolls into the SAME per-ply GWP/cost accumulation — a data column on the existing folds,
// never a new discipline and never a parallel waste fold. Effective area = AreaM2 + WasteAreaM2.
public readonly record struct ElementQuantity(double AreaM2, double VolumeM3, double WasteAreaM2 = 0.0) {
    public static readonly ElementQuantity Zero = new(0.0, 0.0);

    public double EffectiveAreaM2 => AreaM2 + WasteAreaM2;
}

// The optional exact per-material quantity (an IFC Qto_*BaseQuantities takeoff) overriding the idealized
// geometric quantity, ALREADY in the resolved property's declared unit; keyed by MaterialId so a layered
// buildup and its constituent share the one composition key, never a parallel graph NodeId.
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

// One resolved field of a window the EN ISO 10077-1 fold weights — a GLAZED field (the IGU pane area: its center-of-glass
// Ug off the glazing material's Thermal.UValue, its visible-glazing perimeter EdgeLengthM and edge-seal PsiWM_K off the
// window's Qto_*BaseQuantities/Pset) or a FRAME field (the frame profile area: its Uf off the frame material's
// Thermal.UValue, NO edge bridge). Glazed iff EdgeLengthM > 0 — the spacer Ψg rides the glazing/frame INTERFACE perimeter,
// so a frame field carries no edge term (EdgeLengthM = 0 zeroes its lg·Ψg contribution). The thermal runner assembles
// these from the window's parts; the kernel folds them, never re-reading a Rasm.Materials type or a graph node.
public readonly record struct WindowField(double UWM2K, double AreaM2, double EdgeLengthM, double PsiWM_K) {
    public static WindowField Glazed(double ugWM2K, double areaM2, double edgeLengthM, double psiWM_K) => new(ugWM2K, areaM2, edgeLengthM, psiWM_K);
    public static WindowField Frame(double ufWM2K, double areaM2) => new(ufWM2K, areaM2, 0.0, 0.0);
    public bool IsGlazed => EdgeLengthM > 0.0;
}

// The EN ISO 10077-1 whole-window receipt: the area-and-perimeter-weighted Uw a single material cannot carry (a window is a
// side-by-side glazing-in-frame composition, not a through-thickness layer series), plus the area-weighted glazed/frame
// sub-transmittances, the Σ lg·Ψg perimeter edge-seal bridge, and the glazed fraction the daylight/solar-gain consumer reads.
public sealed record WindowU(double UwWM2K, double UgWM2K, double UfWM2K, double EdgeBridgeW_K, double GlazedFraction);
```

## [03]-[AGGREGATION_FOLD]

- Owner: `AssemblyAggregator` the static fold kernel over a seam `MaterialComposition` — `Aggregate` (the thermal/mass/vapour/acoustic/mixture/fire receipt), `AggregateEnvironmental` (the EN 15978 cradle-to-grave GWP receipt), `AggregateCost` (the basis-aware supply/install/lifecycle receipt), `AggregateWindow` (the EN ISO 10077-1 whole-window `Uw` receipt over a `Seq<WindowField>`); each composition fold discriminates the composition through the seam's generated total `Switch` (state threaded positionally, the arm `(state, case)`) and folds the per-ply seam `MaterialPropertySet` resolved through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` resolver keyed on the composition's native `MaterialId` into a typed `[FOLD_STATE]` accumulator (`LayerFold`/`MixtureFold`/`CarbonFold`) that `Absorb`s one ply and `Project`s the receipt, while `AggregateWindow` folds the runner-resolved glazed/frame fields through the `WindowFold` accumulator — the window is a SIDE-BY-SIDE composition (glazing-in-frame), not the perpendicular through-thickness `MaterialComposition` series the other folds traverse, so it folds a field set the runner assembled from the window's parts rather than re-discriminating the composition union.
- Entry: `Aggregate(composition, resolve)` over a `LayerSet`/`ConstituentSet` (a `Single`/`ProfileSet` rails — a homogeneous material has no series structure to aggregate, its intrinsic U/STC read seam-direct), plus the rail-twins `AggregateEnvironmental(composition, resolve, overrides, geometry)` and `AggregateCost(composition, resolve, overrides, geometry)` over ANY composition — each the one aggregation entry over the SAME `(composition, resolve, …)` rail; a `LayerSet` folds the series-resistance U `1/U = Rsi + Σ(t_i/λ_i) + Rse`, the `Σ ρ·c·t` areal heat capacity, the `Σ μ·t` vapour `Sd`, the accumulated areal mass `m' = Σ(ρ_i·t_i)` the mass-law STC reads, and the worst-ply fire over the plies, a `ConstituentSet` the `Fraction`-weighted rule-of-mixtures read off `MaterialConstituent.Fraction`, `Fin<T>` aborting ONLY on a ply whose material NODE is absent (the resolver fail) while a ply present-but-missing-a-discipline projects `double.NaN` for that discipline's field, decoupled per discipline so a thermal U survives a layer missing its mechanical density. `AggregateWindow(fields)` folds a window's resolved `Seq<WindowField>` into the EN ISO 10077-1 `WindowU` — `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` — `Fin<T>` railing `ComputeFault.AssessmentInputMissing` on an empty field set or a zero total area (a window with no glazed-or-frame field is the degenerate under-specification, never a `0/0`-`NaN` `Uw` the verdict would band `NotApplicable` as if checked); the runner resolves each field (the glazed `Ug` + frame `Uf` off each part material's `Thermal.UValue.Si`, the glazed area + frame area + visible-glazing edge length off the window's baked `Qto_*BaseQuantities`, the spacer `Ψg` off the window's `Pset` thermal-bridge property) and this kernel folds them, never reading a graph node or a `Rasm.Materials` type.
- Auto: the resolver reads a ply material node's seam property cases (`mid => graph.Material(mid).Map(m => m.Properties).ToFin(...)`) keyed on the composition's `MaterialId` not a graph `NodeId` so the fold reads the composition's OWN plies; the mass-law STC fold accumulates each layer's areal mass `ρ·t` (the same density+thickness it resolves for the heat-capacity, effective-density, and in-plane Voigt effective-modulus `Σ(t·E)/ΣT` folds), then `MassLawBands` evaluates `R(f)=20·log₁₀(m'·f)−47` at each seam `AcousticBand` one-third-octave centre and feeds the resulting per-band SRI vector ONCE through the seam `RatingContour.Stc.Fit` so the assembly STC and the single-material STC share one ASTM-E413 contour-fit kernel; the surface-film resistances `Rsi`/`Rse` (0.13 / 0.04 m²K/W, ISO 6946 interior/exterior) seed the `LayerFold` resistance at the envelope ends so `UValueWM2K` is the reciprocal of the total resistance; the GWP fold scales each ply's per-module `StageGwp` by the BASIS-matching quantity through the SAME `DeclaredQuantity` derivation the cost fold uses — the seam `Environmental.Basis` selecting per-m³ volume / per-m² face area / per-kg `volume × Mechanical.Density.Si` / per-item unit (the per-ply geometric volume `PliesByVolume` distributes — a `LayerSet` layer `Thickness.Si × AreaM2`, a `ConstituentSet` constituent `Fraction × VolumeM3`, a `Single`/`ProfileSet` `VolumeM3` — feeding the per-m³ branch) unless a `PlyQuantity` override supplies the exact declared quantity, and folds the MASS-weighted recycled-content fraction (each ply weighted by its mass `ρ·V`, a ply lacking density excluded from the average); the cost fold reads the seam `Cost.Basis` through that SAME `DeclaredQuantity` owner (per-m³ volume, per-m² area, per-kg `volume × Mechanical.Density.Si`, per-item unit) — both folds sharing ONE basis-aware quantity derivation.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `MaterialPropertySet`, `MaterialPropertyAccess`, `MaterialId`, `AcousticBand`, `LifecycleStage`, `Currency`, `MeasurementBasis`, `RatingContour.Stc.Fit`), BCL inbox.
- Growth: a new aggregation discipline is one `AssemblyAggregator` fold over the same composition reading the same seam cases into a typed `[FOLD_STATE]` accumulator; the generated total `Switch` over the closed composition family breaks at compile time if the seam adds a composition case, never a runtime-silent `_` arm.
- Boundary: the kernel reads the seam `MaterialComposition`/`MaterialPropertySet` and NEVER the retired `Rasm.Materials` `MaterialAssignment`/`MaterialProperty`/`ConstituentWeight` (the engine relocated here whole, the constituent fraction now riding the seam `MaterialConstituent.Fraction` directly, never a parallel weight input); the resolver is keyed on `MaterialId` — the composition's native key — so an `NodeId`-keyed material lookup is the deleted form, and the migration `Aggregate(composition, resolve, Seq<ConstituentWeight>)` 3-arg form is the deleted shape (the 2-arg `Aggregate(composition, resolve)` is canonical); the series-resistance fold reads each `LayerSet` ply's `MaterialLayer.Thickness.Si` SI thickness and the ply material's `Thermal.Conductivity.Si`/`SpecificHeat.Si`/`VapourResistanceFactor` and `Mechanical.Density.Si`, the mass-law STC fold the accumulated areal mass evaluated against the seam `AcousticBand` centres and contour-fit through the seam kernel (NEVER the naive per-leaf dB sum that over-predicts a rigidly-bonded layer set), the rule-of-mixtures fold the `MaterialConstituent.Fraction` set (normalized to unity at the seam `ConstituentSet` admission, so the aggregator never re-guards it); an absent ply property in the multi-physics `Aggregate` is DECOUPLED per discipline — a `LayerSet` layer whose material lacks the `Thermal` case NaNs only the `U`/`Sd`/`heat-capacity` fields, a lacked `Mechanical` case only the mass/stiffness-derived `StcWeighted`/`EffectiveDensity`/`EffectiveYoungsModulus`/`heat-capacity` — projecting `double.NaN` (the `IsFinite` not-applicable signal) so a thermal runner's U is never aborted by a missing density, and ONLY a ply whose material NODE is absent rails `ComputeFault.AssessmentInputMissing`; the single-discipline `AggregateEnvironmental`/`AggregateCost` folds DO rail when a ply lacks the `Environmental`/`Cost` case (a hole invalidates the per-module sum), while an absent FIRE rating is NON-LIMITING (the worst-ply envelope folds the present ratings through the `IfNone(double.MaxValue)` non-limiting seed, an all-absent set yielding `0`, the SAME treatment the `ConstituentSet` fold gives a constituent with no fire rating), and a per-kg cost over a material with no density rails the same typed fault (mass is unresolvable); the cost rollup is over a single `Currency` (a mismatch rails because the fold carries no exchange rate); the GWP `StageGwp` is the per-module vector on the ply's `Environmental.Basis` (per-m³/per-m²/per-kg/per-item — the same basis axis the `Cost` case carries: the `Rasm.Materials` catalogue authors each row at its EPD's native `declared_unit` basis, the EC3 ingress tagging the EPD's native `declared_unit` basis identically) and the fold scales it by the BASIS-matching element quantity through the shared `DeclaredQuantity` derivation (a `PlyQuantity` override supplies an exact per-ply quantity in place of the idealized geometry; a per-kg ply over a density-less material rails as the cost fold does), so the assembly carbon is the per-module ply sum and the `EmbodiedCarbonIntensityKgCo2eM2` divides by the assembly area; the constituent rule-of-mixtures yields effective bulk density/conductivity/Young's-modulus/fire (a constituent lacking the discipline NaNs that effective property, never folding a partial mixture) and the series-only `UValueWM2K`/`ArealHeatCapacityKJM2K`/`VapourResistanceSdM` plus the thickness-less `StcWeighted` are `double.NaN`/`0` for a homogeneous mix (no series structure, no areal mass) — the `IsFinite` not-applicable signal, never a misleading `0.0` reading as a perfect insulator; the `Single`/`ProfileSet` arm of `Aggregate` rails the typed fault explicitly (a single material or a section profile carries no plies — its intrinsic U/STC read seam-direct); the per-ply `Acoustic` SRI band carrier stays the SINGLE-material rating read seam-direct by `Analysis/physics`, never re-summed across the buildup. `AggregateWindow` is the EN ISO 10077-1 SIDE-BY-SIDE fold (orthogonal to every through-thickness composition fold above): a window is glazing-IN-frame, so its `Uw` area-weights the glazed `Ug` and frame `Uf` and ADDS the perimeter edge-seal bridge `Σ lg·Ψg` the perpendicular layer series structurally cannot reach — the glazing `Ug` is the EN 673 CENTER-OF-GLASS value `GlazingSection.Performance` lowered onto `Thermal.UValue`, and folding it raw as a single-material `u-value` (the deleted form before this fold) reports the center-of-glass figure as the whole-window U, omitting BOTH the frame fraction AND the spacer linear bridge a real window's rating depends on; the spacer `Ψg` is read off the window's `Pset` thermal-bridge property (where `GlazingSection`'s `SpacerType.PsiWmK` lowers — the seam `MaterialPropertySet.Thermal` carries `Conductivity`/`SpecificHeat`/`UValue`/`VapourResistanceFactor` only, NO perimeter-bridge column, so a `Ψ`-on-`Thermal` read is the phantom the runner never takes), and a `WindowField.Frame` carries no edge term (`EdgeLengthM = 0` zeroes its `lg·Ψg`) because the edge seal rides the glazing/frame interface perimeter; an empty field set or a zero total area rails the typed fault (the degenerate window, never a `0/0` `Uw`), and the kernel folds the runner-resolved fields without re-reading a graph node — the `MaterialId`-keyed resolver and the baked-bag reads are the runner's, this kernel pure over the field set.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssemblyAggregator {
    const double RsiWM2K = 0.13;            // ISO 6946 interior surface film
    const double RseWM2K = 0.04;            // ISO 6946 exterior surface film
    const double MassLawConstantDb = 47.0;  // field-incidence mass law: R(f) = 20·log10(m'·f) − 47 dB

    // Series-resistance U + areal heat capacity + vapour Sd + mass-law STC + in-plane Voigt modulus over a LayerSet,
    // Fraction-weighted rule-of-mixtures effective density/conductivity/modulus over a ConstituentSet, worst-ply fire
    // over either; a Single/ProfileSet has no plies to aggregate — its intrinsic U/STC read seam-direct — so it rails
    // the typed fault. State threads positionally through the generated total Switch (the arm shape is (state, case)).
    public static Fin<AssemblyProperty> Aggregate(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        composition.Switch(
            resolve,
            single:         static (_, _)   => Fin.Fail<AssemblyProperty>(Missing("<aggregation-requires-layer-or-constituent-set>")),
            profileSet:     static (_, _)   => Fin.Fail<AssemblyProperty>(Missing("<aggregation-requires-layer-or-constituent-set>")),
            layerSet:       static (r, set) => AggregateLayers(set, r),
            constituentSet: static (r, set) => AggregateConstituents(set, r));

    // EN 15978 embodied carbon over ANY composition: each ply's per-module StageGwp scaled by the BASIS-matching element
    // quantity — the SAME basis-aware DeclaredQuantity derivation the cost fold uses (Environmental.Basis selects per-m³
    // volume / per-m² face area / per-kg volume×density / per-item unit), a PlyQuantity override supplying an exact
    // declared quantity — so a per-m² membrane or per-kg steel EPD folds correctly without a forced per-m³ normalization;
    // the whole-life total is the sum, the intensity per assembly area, and the MASS-weighted recycled-content fraction
    // (recycled is a mass metric, so it weights by ply mass ρ·V — a ply lacking density is excluded from the average,
    // never volume-weighted into a wrong number). A ply missing the Environmental case rails (a hole invalidates the
    // sum); a per-kg ply over a density-less material rails the same way the cost fold does (mass unresolvable).
    public static Fin<AssemblyLifecycle> AggregateEnvironmental(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementQuantity geometry) =>
        PliesByVolume(composition, geometry).Fold(
            Fin.Succ(CarbonFold.Seed),
            (acc, ply) => acc.Bind(state => resolve(ply.Material).Bind(props =>
                from env in props.Environmental.ToFin(Missing($"<assembly-ply-missing-environmental:{ply.Material.Value}>"))
                from gwpQuantity in PlyOverride(overrides, ply.Material).Match(
                    Some: static q => Fin.Succ(q),
                    None: () => DeclaredQuantity(env.Basis, ply.VolumeM3, geometry.AreaM2, DensityKgM3(props), ply.Material))
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

    // Basis-aware supply/install/lifecycle rollup over ANY composition + a single Currency: the seam Cost.Basis
    // selects the geometric quantity the per-unit price scales by; a currency mismatch rails (the fold carries no
    // exchange rate), a per-kg cost over a material with no density rails (mass is unresolvable), a missing Cost rails.
    public static Fin<AssemblyCost> AggregateCost(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, Seq<PlyQuantity> overrides, ElementQuantity geometry) =>
        PliesByVolume(composition, geometry).Fold(
            Fin.Succ(Option<AssemblyCost>.None),
            (acc, ply) => acc.Bind(running => resolve(ply.Material).Bind(props =>
                from cost in props.Cost.ToFin(Missing($"<assembly-missing-cost:{ply.Material.Value}>"))
                from qty in PlyOverride(overrides, ply.Material).Match(
                    Some: static q => Fin.Succ(q),
                    None: () => DeclaredQuantity(cost.Basis, ply.VolumeM3, geometry.AreaM2, DensityKgM3(props), ply.Material))
                from merged in Accumulate(running, cost, qty)
                select Some(merged))))
            .Bind(static o => o.ToFin(Missing("<assembly-cost-empty>")));

    // The EN ISO 10077-1 simplified whole-window transmittance over the runner-resolved glazed/frame fields:
    // Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af) — the glazed Ug area-weighted, the frame Uf area-weighted, and the
    // edge-seal Σ lg·Ψg perimeter linear bridge ADDED, the side-by-side composition the through-thickness layer series
    // cannot reach. The runner assembles each field (Ug/Uf off each part material's Thermal.UValue.Si, the areas off the
    // window's Qto_*BaseQuantities, the spacer Ψg off the window's Pset thermal-bridge property); this kernel folds them.
    // An empty field set or a zero total area rails the typed fault — a window with no glazed-or-frame field is the
    // degenerate under-specification, never a 0/0-NaN Uw the verdict would band NotApplicable as if it had been checked.
    public static Fin<WindowU> AggregateWindow(Seq<WindowField> fields) =>
        fields.IsEmpty
            ? Fin.Fail<WindowU>(Missing("<window-no-glazed-or-frame-field>"))
            : fields.Fold(WindowFold.Seed, static (state, field) => state.Absorb(field)).Project();

    // A LayerSet folds each discipline INDEPENDENTLY into the typed LayerFold: a layer carrying no Thermal case only
    // NaNs the U/Sd, no Mechanical density only the mass-derived STC/effective-density/heat-capacity — it never aborts
    // the U a thermal runner reads, so a thermal assessment is decoupled from a missing density. Only a ply whose
    // material NODE is absent (the resolver Fin.Fail) rails; a present-material-missing-property is a typed not-applicable.
    static Fin<AssemblyProperty> AggregateLayers(MaterialComposition.LayerSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Layers
            .Fold(Fin.Succ(LayerFold.Seed), (acc, layer) => acc.Bind(state => resolve(layer.Material).Map(props => state.Absorb(layer, props))))
            .Map(static state => state.Project());

    // A ConstituentSet folds the Fraction-weighted rule-of-mixtures effective density/conductivity; a constituent
    // lacking the discipline NaNs that effective property (a rule-of-mixtures cannot fold over a hole). The series
    // fields (U/areal-heat-capacity/vapour Sd) and the thickness-less STC are double.NaN/0 — a homogeneous mix has
    // NO series structure or areal mass, and NaN is the IsFinite not-applicable signal. Only a missing NODE rails.
    static Fin<AssemblyProperty> AggregateConstituents(MaterialComposition.ConstituentSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        set.Constituents
            .Fold(Fin.Succ(MixtureFold.Seed), (acc, c) => acc.Bind(state => resolve(c.Material).Map(props => state.Absorb(c, props))))
            .Map(static state => state.Project());

    // The per-ply geometric volume each composition case distributes (paired with the element area the GWP intensity
    // divides by): a Single/ProfileSet the element volume, a layer its thickness × face area, a constituent its
    // fraction of the element volume — the one closed total Switch the GWP and cost folds share, so a new composition
    // case breaks both folds at compile time.
    static Seq<(MaterialId Material, double VolumeM3)> PliesByVolume(MaterialComposition composition, ElementQuantity geometry) =>
        composition.Switch(
            geometry,
            single:         static (g, s) => Seq((s.Material, g.VolumeM3)),
            profileSet:     static (g, s) => Seq((s.Material, g.VolumeM3)),
            layerSet:       static (g, s) => s.Layers.Map(l => (l.Material, l.Thickness.Si * g.AreaM2)),
            constituentSet: static (g, s) => s.Constituents.Map(c => (c.Material, c.Fraction * g.VolumeM3)));

    static Fin<AssemblyCost> Accumulate(Option<AssemblyCost> running, MaterialPropertySet.Cost cost, double qty) =>
        running.Match(
            Some: r => r.Currency == cost.Currency
                ? Fin.Succ(r with { SupplyTotal = r.SupplyTotal + cost.SupplyPerUnit * qty, InstallTotal = r.InstallTotal + cost.InstallPerUnit * qty, LifecycleTotal = r.LifecycleTotal + cost.LifecyclePerUnit * qty })
                : Fin.Fail<AssemblyCost>(Missing($"<cost-currency-mismatch:{r.Currency.Key}<>{cost.Currency.Key}>")),
            None: () => Fin.Succ(new AssemblyCost(cost.Currency, cost.SupplyPerUnit * qty, cost.InstallPerUnit * qty, cost.LifecyclePerUnit * qty)));

    // The seam Cost.Basis selects the geometric quantity the per-unit price scales by; per-kg without a resolved
    // density rails the typed fault (mass is unresolvable), never a silent zero. Total over the four basis rows.
    static Fin<double> DeclaredQuantity(MeasurementBasis basis, double volumeM3, double areaM2, Option<double> density, MaterialId material) =>
        basis.Switch(
            (volumeM3, areaM2, density, material),
            perM3:   static s => Fin.Succ(s.volumeM3),
            perM2:   static s => Fin.Succ(s.areaM2),
            perItem: static _ => Fin.Succ(1.0),
            perKg:   static s => s.density.Map(d => s.volumeM3 * d).ToFin(Missing($"<cost-per-kg-missing-density:{s.material.Value}>")));

    static ComputeFault Missing(string detail) => new ComputeFault.AssessmentInputMissing(detail);

    // The per-ply reads compose the seam MaterialPropertyAccess accessors directly (props.Thermal/.Mechanical/.Fire/
    // .Environmental/.Cost) — never a re-derived is-cast the seam owns: the seam now exposes the FULL typed accessor
    // family (Fire/Cost included), so every discipline read is ONE_HOP off the seam and the local Choose re-derivations are deleted.
    static Option<double> DensityKgM3(Seq<MaterialPropertySet> props) => props.Mechanical.Map(static m => m.Density.Si);
    static Option<double> YoungsModulusPa(Seq<MaterialPropertySet> props) => props.Mechanical.Map(static m => m.YoungsModulus.Si);
    static Option<double> ConductivityWmK(Seq<MaterialPropertySet> props) => props.Thermal.Map(static t => t.Conductivity.Si);
    // The worst-ply fire envelope reads the EN 13501-2 R (load-bearing) criterion off the seam typed FireResistance —
    // the assembly's structural fire endurance is the minimum LOAD-BEARING rating over the plies, never a single
    // conflated resistance scalar (the deleted form a separating EI wall and a load-bearing R column could not distinguish).
    static Option<double> FireMinutes(Seq<MaterialPropertySet> props) => props.Fire.Map(static f => (double)f.Resistance.LoadBearingMinutes);
    static Option<double> PlyOverride(Seq<PlyQuantity> overrides, MaterialId material) => overrides.Find(q => q.Material == material).Map(static q => q.DeclaredQuantity);

    // The field-incidence mass law over the layer set's accumulated areal mass m' (kg·m⁻²): R(f) = 20·log10(m'·f) − 47,
    // evaluated at each seam AcousticBand one-third-octave centre into the per-band SRI vector the seam RatingContour.Stc.Fit
    // contour-fits — so the assembly STC and the single-material STC share ONE ASTM-E413 owner, and a bonded buildup's rating is
    // its combined-mass estimate, never the unphysical per-leaf dB sum that over-predicts a rigidly-connected layer set.
    static double[] MassLawBands(double massKgM2) {
        double[] sri = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) { sri[band.Key] = Math.Max(0.0, 20.0 * Math.Log10(massKgM2 * band.CenterHz) - MassLawConstantDb); }
        return sri;
    }

    // The per-module Stage accumulation: a FRESH array each ply (never mutated in place) so the carbon fold stays
    // immutable. The seam StageGwp is the DERIVED GwpTotal-per-stage row off the Environmental (ImpactCategory ×
    // LifecycleStage) matrix (Environmental.StageGwp = IndicatorAt(GwpTotal, stage) over the stage band, an
    // ImmutableArray<double> of arity LifecycleStage.Count the seam guarantees at OfEnvironmental admission), so the
    // carbon receipt folds the GwpTotal indicator row the seam projects rather than re-slicing the matrix here.
    static double[] AddScaled(double[] accumulated, ImmutableArray<double> stage, double scale) {
        ReadOnlySpan<double> bands = stage.AsSpan();
        double[] next = new double[LifecycleStage.Count];
        for (int i = 0; i < LifecycleStage.Count; i++) { next[i] = accumulated[i] + bands[i] * scale; }
        return next;
    }

    // --- [FOLD_STATE] ---------------------------------------------------------------------
    // The typed per-discipline fold accumulators (private algorithm state co-located with the kernel that folds them):
    // each Absorbs one ply's resolved seam properties and Projects the AssemblyProperty receipt, the per-discipline
    // completeness flags deciding which receipt columns are double.NaN (the not-applicable signal) versus computed.
    readonly record struct LayerFold(double Resistance, double MassKgM2, double ModulusTPa, double HeatJM2K, double SdM, double ThicknessM, double MinFire, bool Thermal, bool Mechanical) {
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
        public AssemblyProperty Project() => new(
            UValueWM2K:               Thermal ? 1.0 / Resistance : double.NaN,
            StcWeighted:              Mechanical && MassKgM2 > 0.0 ? RatingContour.Stc.Fit(MassLawBands(MassKgM2)) : 0,
            EffectiveDensityKgM3:     Mechanical && ThicknessM > 0.0 ? MassKgM2 / ThicknessM : double.NaN,
            EffectiveConductivityWMK: Thermal && Resistance > RsiWM2K + RseWM2K ? ThicknessM / (Resistance - RsiWM2K - RseWM2K) : double.NaN,
            EffectiveYoungsModulusPa: Mechanical && ThicknessM > 0.0 ? ModulusTPa / ThicknessM : double.NaN,
            FireResistanceMinutes:    MinFire is double.MaxValue ? 0.0 : MinFire,
            ArealHeatCapacityKJM2K:   Thermal && Mechanical ? HeatJM2K / 1000.0 : double.NaN,
            VapourResistanceSdM:      Thermal ? SdM : double.NaN);
    }

    readonly record struct MixtureFold(double Density, double Conductivity, double ModulusPa, double MinFire, bool Mechanical, bool Thermal) {
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

    // The EN 15978 carbon accumulator: the per-module Stage vector (kgCO2e) and the recycled/total MASS pair the
    // mass-weighted recycled-content fraction divides; Stage is replaced each ply by a fresh AddScaled array (never
    // mutated in place) so the carbon fold stays immutable.
    readonly record struct CarbonFold(double[] Stage, double RecycledMass, double TotalMass) {
        public static CarbonFold Seed => new(new double[LifecycleStage.Count], 0.0, 0.0);
        public double WholeLife => Stage.Sum();
    }

    // The EN ISO 10077-1 whole-window accumulator: the area-weighted glazed/frame conductance numerators (Σ Ag·Ug, Σ Af·Uf)
    // and their areas, plus the perimeter edge-seal bridge Σ lg·Ψg — a glazed field contributes ALL three (its area·Ug,
    // its area, its lg·Ψg), a frame field only its area·Uf and area (EdgeLengthM = 0 zeroes the edge term). Project
    // area-weights each sub-transmittance and sums the whole-window Uw, railing the typed fault on a zero total area (a
    // window with neither a glazed nor a frame field of positive area — never a 0/0 NaN the consumer would misread).
    readonly record struct WindowFold(double GlazedUA, double GlazedArea, double FrameUA, double FrameArea, double EdgeBridge) {
        public static WindowFold Seed => new(0.0, 0.0, 0.0, 0.0, 0.0);

        public WindowFold Absorb(WindowField f) => f.IsGlazed
            ? this with { GlazedUA = GlazedUA + f.UWM2K * f.AreaM2, GlazedArea = GlazedArea + f.AreaM2, EdgeBridge = EdgeBridge + f.EdgeLengthM * f.PsiWM_K }
            : this with { FrameUA = FrameUA + f.UWM2K * f.AreaM2, FrameArea = FrameArea + f.AreaM2 };

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

- [ISO_6946_SERIES_U]: the `LayerSet` U-value is `1/U = Rsi + Σ(t_i/λ_i) + Rse` (ISO 6946:2017) over the plies' `MaterialLayer.Thickness.Si` SI thickness and the ply material's seam `MaterialPropertySet.Thermal.Conductivity.Si`, with the interior/exterior surface-film resistances `Rsi = 0.13` / `Rse = 0.04 m²K/W` added once at the envelope ends (the `LayerFold.Seed` resistance); the effective through-thickness conductivity is `ΣT / (R − Rsi − Rse)`. The fold reads the seam thermal case through the `props.Thermal` accessor — the retired `Rasm.Materials` `AssemblyAggregator.AggregateLayers` is superseded whole by this relocation. Ripple counterpart: `Rasm.Materials` `Properties/properties#ASSEMBLY_AGGREGATOR_RELOCATED` (RETIRE) and `Rasm.Element/Composition/material` (the seam `MaterialComposition`/`MaterialPropertySet` owner).
- [EN_10077_WHOLE_WINDOW_U]: the `WindowU.UwWM2K` is the EN ISO 10077-1 simplified whole-window transmittance `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)` — the area-weighted mean of the glazed center-of-glass `Ug` and the frame `Uf` PLUS the linear edge-seal thermal bridge `Σ lg·Ψg` over the visible-glazing perimeter, the SIDE-BY-SIDE window composition the through-thickness ISO 6946 series-U (`[ISO_6946_SERIES_U]`) cannot reach (a window is glazing-IN-frame, not a perpendicular layer stack). The glazed `Ug` is the EN 673 CENTER-OF-GLASS value the `Rasm.Materials` `Component/glazing` `GlazingSection.Performance` computes and `GlazingSection.ToProperties` lowers onto the seam `MaterialPropertySet.Thermal.UValue` — so this fold reads it seam-direct off the glazing material (`props.Thermal.Map(t => t.UValue.Si)`) and area-weights it WITH the frame and the spacer bridge, rather than the `Analysis/physics` thermal runner reporting the raw `Ug` as the whole-window U (the deleted form: a single-material `u-value` over a glazing node omits both the frame fraction AND the spacer linear bridge). The spacer `Ψg` is the `SpacerType.PsiWmK` linear thermal bridge (warm-edge `0.04`, cold-edge-aluminium `0.11 W·m⁻¹·K⁻¹`) the glazing family lowers onto the window's `Pset` thermal-bridge property (NOT onto `MaterialPropertySet.Thermal`, whose `Conductivity`/`SpecificHeat`/`UValue`/`VapourResistanceFactor` columns carry no perimeter-bridge term), so the thermal runner reads it off the baked `Pset` and assembles the `WindowField`; the glazed area `Ag` + frame area `Af` are the window's `Qto_*BaseQuantities` (`GlazingArea`/`Area`) the BIM geometry takeoff derives, the visible-glazing edge length `lg` the glazing perimeter. The full EN ISO 10077-2 numerical frame-`Uf`-plus-`Ψ` finite-element thermal model is a richer `WindowField` resolution (a measured `Uf`/`Ψg` per frame profile) the runner supplies when a frame thermal model is admitted, never a parallel window owner — the simplified area-weighted closed form is the honest form the seam-direct `Ug`/`Uf`/`Ψg` data supports. Ripple counterpart: `Rasm.Materials` `Component/glazing` (the `GlazingSection.Performance` EN 673 `Ug` + the `SpacerType.PsiWmK` the window assembly reads, the WHOLE-WINDOW `Uw` explicitly the Compute assembly concern, not a glazing-section field), `Rasm.Compute/Analysis/physics` (the `BuildingPhysics.RunThermal` window-assembly branch that resolves the glazed/frame `WindowField` set and composes this fold), and `Rasm.Element/Composition/material` (the seam `Thermal` case carrying the glazing `Ug` on `UValue`, the spacer `Ψg` riding a `Pset` not a `Thermal` column).
- [ISO_13786_THERMAL_MASS]: the `ArealHeatCapacityKJM2K` is the ISO 13786 dynamic thermal-mass surrogate `κ = Σ(ρ_i · c_i · t_i)` over the plies' `Mechanical.Density.Si` and `Thermal.SpecificHeat.Si` and `MaterialLayer.Thickness.Si`, divided by 1000 to kJ — the `Analysis/physics` dynamic-thermal consumer reads it for the decrement/admittance pair the steady-state U cannot carry. The `LayerFold` accumulates `density` and `thermal.SpecificHeat.Si` it already resolves for the U and mass folds, so the heat-capacity column is one term in the same series fold, never a parallel pass.
- [EN_ISO_13788_VAPOUR]: the `VapourResistanceSdM` is the assembly equivalent-air-layer diffusion resistance `Sd = Σ(μ_i · t_i)` (EN ISO 13788 / EN ISO 12572) over the plies' `Thermal.VapourResistanceFactor` (μ) and SI thickness — the single series-vapour-resistance owner the `Analysis/physics` Glaser interstitial-condensation fold reads instead of re-deriving the cumulative `Z` profile, so the layered vapour resistance and the condensation check share one summation. The per-interface temperature/saturation/actual-vapour profile (the condensation-plane locator) stays in `Analysis/physics` over the per-layer resistances; this owner contributes the assembly total.
- [MASS_LAW_LAYERED_STC]: the layered sound-reduction index is the field-incidence mass law over the buildup's accumulated areal mass `m' = Σ(ρ_i·t_i)` — `R(f) = 20·log₁₀(m'·f) − 47 dB` (the field/random-incidence single-leaf law) evaluated at each seam `AcousticBand` one-third-octave centre (`MassLawBands`) into a per-band SRI vector fed ONCE through the SEAM `RatingContour.Stc.Fit` (the ASTM-E413 single-number contour fit) so the assembly STC and the single-material `MaterialPropertySet.Acoustic.StcWeighted` share one contour-fit owner. The naive per-leaf dB sum (`Σ R_i` across plies) is the DELETED form — it treats rigidly-bonded layers as acoustically independent and over-predicts a real layer set's rating by tens of dB, and the seam carries no decoupling cavity (a `MaterialLayer` is `material + thickness` only) to justify the independent-leaf model, so the combined-mass estimate is the honest closed form the buildup's data supports. The assembly STC thus reads the plies' `Mechanical` density (the areal mass), NOT a per-ply acoustic spectrum — the per-ply `Acoustic` SRI band carrier stays the SINGLE-material rating (`§4B`, read seam-direct by the `Analysis/physics` single-material acoustic branch). Ripple counterpart: `Rasm.Element/Composition/acoustic` (the seam `RatingContour` contour family whose public `Stc.Fit`/`Rw.Fit` kernel this fold shares, its `[LAYERED_STC_SHARE]` note carrying the SAME field-incidence mass-law-over-total-areal-mass model — this fold feeds its per-band layered SRI through the one shared kernel and the naive per-leaf dB sum is the deleted form on both sides, the contour fit itself unchanged).
- [RULE_OF_MIXTURES]: a composite material's effective conductivity/density/Young's modulus is the constituent-`Fraction`-weighted Voigt (iso-strain) arithmetic mean of its `ConstituentSet` member properties `Σ(f_i·k_i)`/`Σ(f_i·ρ_i)`/`Σ(f_i·E_i)` (the IFC `IfcMaterialConstituentSet.Fraction` mixture rule, the Voigt parallel bound), an immutable fold over the seam `ConstituentSet` reading each constituent's `Mechanical`/`Thermal` case (a constituent lacking one NaNs that effective property, the rule-of-mixtures never folding over a hole) AND its `MaterialConstituent.Fraction` DIRECTLY — the migration source's parallel `ConstituentWeight(NodeId, double)` input is RETIRED because the seam constituent already carries its `Fraction`, and the fraction-to-unity normalization is enforced once at the seam `ConstituentSet` admission so the aggregator never re-guards it; the SAME Voigt form gives a `LayerSet` its in-plane (membrane) effective modulus `Σ(t_i·E_i)/ΣT` (the iso-strain estimate for in-plane loading; out-of-plane laminate bending `EI` is a later fold over the same plies), so a composite's stiffness and a layer set's membrane stiffness share one rule-of-mixtures owner reading the seam `Mechanical.YoungsModulus` (the `EffectiveYoungsModulusPa` receipt column the structural runner reads for a composite/laminate member without re-resolving the constituents); a constituent missing a fire rating does not lower the assembly fire envelope (absent → non-limiting), never zeros it; a homogeneous mix has no thickness, so its `StcWeighted` is the not-rated `0` floor.
- [EN_15978_EMBODIED_CARBON]: the assembly embodied carbon sums each ply's seam `MaterialPropertySet.Environmental.StageGwp` (the per-module A1A3/A4/A5/B/C/D vector over the seam `LifecycleStage` band) scaled by the per-ply geometric volume — a `LayerSet` layer by `Thickness.Si × AreaM2`, a `ConstituentSet` constituent by `Fraction × VolumeM3`, a `Single`/`ProfileSet` by `VolumeM3` — so the GWP rail spans ALL compositions (a solid-material element has embodied carbon), the assembly `StageGwp` the per-module ply sum and `WholeLifeGwpKgCo2e` its all-module total; the `EmbodiedCarbonIntensityKgCo2eM2` divides by the assembly area. The `RecycledContentFraction` is the MASS-weighted average of the plies' recycled fractions (recycled content is a mass metric, so each ply weights by its mass `ρ·V` resolved through the `Mechanical` case — a ply lacking density is EXCLUDED from the average rather than volume-weighted into a wrong number). The `StageGwp` is on the ply's `MeasurementBasis` (per-m³/per-m²/per-kg/per-item) — the seam `Environmental` case now carries a `MeasurementBasis` like `Cost`; the `Rasm.Materials` `Properties/sustainability` catalogue authors each row at the EPD's NATIVE `declared_unit` basis (per-kg metals/glass/gypsum/insulation, per-m³ concrete/timber/masonry/stone, per-m² membranes — never a forced curated `PerM3`) and the `Analysis/lifecycle` EC3 ingress tags the EPD's native `declared_unit` basis identically, so this fold scales each ply's `StageGwp` by the BASIS-matching element quantity through the SAME `DeclaredQuantity` owner the cost fold uses (per-m³ volume, per-m² face area, per-kg volume×density, per-item unit) — a per-m² membrane or per-kg steel EPD now folds correctly instead of being skipped, and a baked native-basis declaration and an EC3-resolved native-basis declaration fold under one basis-aware scale. A `PlyQuantity` override supplies an exact per-ply volume (an IFC `Qto_*BaseQuantities` takeoff) in place of the idealized geometric volume. The `Analysis/lifecycle` LCA runner reads this receipt and resolves a missing ply EPD from the EC3 service. Ripple counterpart: `Rasm.Materials` `Properties/sustainability#ASSEMBLY_AGGREGATOR_RELOCATED` (RETIRE).
- [COST_ROLLUP]: the supply/install/lifecycle cost rollup sums each ply's seam `MaterialPropertySet.Cost.SupplyPerUnit`/`InstallPerUnit`/`LifecyclePerUnit` × the per-ply quantity over a single `Currency`, the quantity DERIVED from the seam `Cost.Basis` (per-m³ volume, per-m² area, per-kg `volume × Mechanical.Density.Si`, per-item unit) so the carried basis is load-bearing, not decorative; a currency mismatch rails because the rollup carries no exchange rate, a per-kg cost without a resolved density rails the missing-input fault. The cost rail spans all composition cases (a `Single`/`ProfileSet` member has a unit supply/install cost). Construction SCHEDULING and 4D cost-loading stay in `Rasm.Bim` (MPXJ); this is the embodied material-cost takeoff only. Ripple counterpart: `Rasm.Materials` `Properties/sustainability#ASSEMBLY_AGGREGATOR_RELOCATED` `AggregateCost` (RETIRE).
- [ID_AND_QUANTITY_ALIGNMENT]: the aggregator resolver is keyed on `MaterialId` (the composition's native ply key) not a graph `NodeId`, so the fold reads the composition's own plies through `graph.Material(MaterialId)` — the migration source's `NodeId`-keyed `ConstituentWeight`/`PlyQuantity` were a key-type mismatch against the `MaterialId`-keyed composition. The per-ply geometric quantity comes from one `ElementQuantity(AreaM2, VolumeM3)` element takeoff (a layer scales by `thickness × Area`, a constituent by `Fraction × Volume`, a single/profile by `Volume`), and a consumer with an exact IFC `Qto_*BaseQuantities` takeoff overrides per material through `PlyQuantity(MaterialId, DeclaredQuantity)`. Both consumer runners read this aggregator through the one `MaterialId`-keyed `Fin`-lifted resolver (`mid => graph.Material(mid).Map(m => m.Properties).ToFin(...)`): `Analysis/lifecycle` feeds the 4-arg `AggregateEnvironmental`/`AggregateCost` and `Analysis/physics` the 2-arg `Aggregate(composition, resolve)` — the retired `Seq<ConstituentWeight>` third argument and the `NodeId`-keyed `Option`-returning resolver are the deleted shapes neither runner carries. Ripple counterpart: `Rasm.Element/Graph/element` (the `ElementGraph.Material(MaterialId)`/`CompositionOf` reads + the baked base quantities the takeoff reads).
- [RELOCATION_SEAM]: the engine moved from `Rasm.Materials` to `Rasm.Compute` whole — Materials RETIRES its `AssemblyAggregator`/`AssemblyProperty`/`AssemblyLifecycle`/`AssemblyCost`/`ConstituentWeight`/`PlyQuantity` owners and projects only the single-material subgraph (the per-material `MaterialPropertySet` cases onto the seam `Material` node); the multi-ply aggregation reads the seam composition from the analysis side. The kernel reads the seam vocabulary (`MaterialComposition`, `MaterialLayer.Thickness`, `MaterialConstituent.Fraction`, `MaterialPropertySet` via `MaterialPropertyAccess`, `AcousticBand`, `LifecycleStage`, `RatingContour.Stc.Fit`) and never the retired Materials owners. The new `PlyQuantity` is the `MaterialId`-keyed override this owner declares, not the retired Materials `NodeId`-keyed form. Ripple counterpart: `Rasm.Materials` `Construction/assembly` + `Properties/properties` + `Properties/sustainability` (RETIRE the aggregator half; KEEP the single-material property authoring lowered onto the seam).
