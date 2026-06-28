# [COMPUTE_PHYSICS]

Rasm.Compute closed-form building-physics runner: the `Discipline.Thermal`/`Discipline.Acoustic`/`Discipline.Fire` arms of the assessment rail, collapsed onto ONE `BuildingPhysics` kernel because all three are closed-form ISO/EN folds over an assembly or section read DIRECTLY from the `Rasm.Element` `ElementGraph` — no external solver, no subprocess. Thermal folds the ISO 6946 series-U through the relocated `Analysis/aggregator` and runs the EN ISO 13788 Glaser steady-state interstitial-condensation profile over the layer thermal-and-vapour resistances against the boundary climate; acoustic folds the ISO 12354 layered sound-reduction index through the aggregator (whose layered SRI rides the SEAM `StcContourFit`) and derives the ISO 717 single-number `Rw`; fire folds the EN 1993-1-2 steel critical-temperature method (the ISO 834 incremental gas-curve temperature rise over the section factor `Am/V` against the degree-of-utilization critical temperature) and the EN 1992-1-2 concrete tabulated-cover check. Each fold returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, verdict derived from the governing ratio (U vs target, `Rw` vs target, achieved-vs-required fire minutes). The page composes the seam vocabulary settled — `MaterialComposition`/`MaterialLayer`, the `MaterialPropertySet.Thermal`/`Acoustic` cases, the `AcousticBand` band, `SectionProperties` — and the `Analysis/aggregator` `AssemblyAggregator` for the multi-ply rollup; the single-material PURE acoustic folds (`Nrc`/`Saa`/`StcWeighted`) stay seam-owned and the multi-ply layered physics is this runner.

## [01]-[INDEX]

- [01]-[THERMAL_ENVELOPE]: the ISO 6946 series-U over the aggregator and the EN ISO 13788 Glaser interstitial-condensation profile over the layer thermal/vapour resistances against a `BoundaryClimate`.
- [02]-[ACOUSTIC_RATING]: the ISO 12354 layered sound-reduction index through the aggregator and the ISO 717 single-number `Rw` plus the single-material `Nrc` projection.
- [03]-[FIRE_RESISTANCE]: the EN 1993-1-2 steel critical-temperature incremental ISO-834 fold over the section factor and the EN 1992-1-2 concrete tabulated-cover check, dispatched by the fire route.

## [02]-[THERMAL_ENVELOPE]

- Owner: `BuildingPhysics.RunThermal` the thermal runner; `BoundaryClimate` the interior/exterior temperature-and-humidity boundary carried on the request; `GlaserProfile` the per-interface temperature/saturation/actual-vapour-pressure profile; `CondensationPlane` the interstitial-condensation interface receipt.
- Entry: `public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, ClockPolicy clocks)` — resolves each target's seam `MaterialComposition`, folds the ISO 6946 series-U through `AssemblyAggregator.Aggregate`, runs the EN ISO 13788 Glaser fold over the same `LayerSet` against `request.Climate`, and emits the `u-value`/`condensation-risk`/`condensation-plane` facts, `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a layer lacks a thermal or vapour property.
- Auto: the interface temperature is `T_i = T_int − (ΣR_0..i / R_total)·(T_int − T_ext)` over the cumulative thermal resistances (the aggregator's series resistances); the saturation vapour pressure is the Magnus form `p_sat = 610.5·exp(17.269·T/(237.3+T))`; the actual vapour pressure follows the cumulative vapour resistance `Z_i = Σ(μ_j·t_j)·δ0` profile between the boundary partial pressures; an interface where `p_actual ≥ p_sat` is a `CondensationPlane`.
- Packages: LanguageExt.Core, Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialPropertySet.Thermal`, `NodeId`), the `Analysis/aggregator` `AssemblyAggregator`, Rasm (project — `Dimension`), BCL inbox.
- Growth: a new thermal check (a thermal-bridge psi-value, a dynamic decrement factor) is one fold over the same `LayerSet`, never a parallel envelope owner; the moisture model deepens to the EN 15026 transient form as one fold swap reading the same layer resistances.
- Boundary: the multi-ply U-value composes `AssemblyAggregator.Aggregate` (ISO 6946 series resistance with the `Rsi`/`Rse` surface films) so the thermal envelope and the aggregator share ONE series-resistance owner, never a re-derived U; the Glaser fold reads each layer's `MaterialPropertySet.Thermal.Conductivity.Si` and `VapourResistanceFactor` (μ) — a layer missing the vapour factor rails the typed fault rather than defaulting; the verdict is `U / U_target` when the climate carries a target transmittance, else the condensation-risk flag governs; a Glaser interstitial condensation that re-evaporates within the annual cycle is reported as a fact, the persistent-accumulation case as the critical verdict, never a clamped sentinel.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BoundaryClimate(double InteriorTempC, double InteriorRh, double ExteriorTempC, double ExteriorRh, double TargetUValueWM2K) {
    public static readonly BoundaryClimate WinterDesign = new(20.0, 0.50, -5.0, 0.85, TargetUValueWM2K: 0.30);
    public double InteriorVapourPa => SaturationPa(InteriorTempC) * InteriorRh;
    public double ExteriorVapourPa => SaturationPa(ExteriorTempC) * ExteriorRh;
    public static double SaturationPa(double tC) => 610.5 * Math.Exp(17.269 * tC / (237.3 + tC));
}

public readonly record struct GlaserProfile(double TempC, double SaturationPa, double ActualPa) {
    public bool Condensing => ActualPa >= SaturationPa;
}

public readonly record struct CondensationPlane(int LayerIndex, double MarginPa);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    const double VapourPermeabilityAir = 2.0e-10; // δ0, kg/(m·s·Pa)

    public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ(Seq<AssessmentFact>()),
            (acc, id) => acc.Bind(facts =>
                from composition in graph.CompositionOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<element-missing-composition>"))
                from property in AssemblyAggregator.Aggregate(composition, Resolver(graph), Seq<ConstituentWeight>())
                from glaser in Glaser(graph, composition, request.Climate)
                let plane = FirstCondensing(glaser)
                select facts
                    .Add(AssessmentFact.Measure($"{id.Value}/u-value", MeasureValue.Of(property.UValueWM2K, "W/m²·K")))
                    .Add(AssessmentFact.Flag($"{id.Value}/condensation-risk", plane.IsSome))
                    .Add(plane.Map(i => AssessmentFact.Ratio($"{id.Value}/condensation-plane", i)).IfNone(AssessmentFact.Text($"{id.Value}/condensation-plane", "none")))))
            .Map(facts => AssessmentResult.Of(request.Route, facts, GoverningU(facts, request.Climate),
                new Provenance("BuildingPhysics", request.Route.Standard, "n/a", clocks.Now), clocks.Now));

    static Fin<Seq<GlaserProfile>> Glaser(ElementGraph graph, MaterialComposition composition, BoundaryClimate climate) =>
        composition is MaterialComposition.LayerSet set
            ? set.Layers.Fold(
                Fin.Succ(Seq<(double R, double Z)>()),
                (acc, layer) => acc.Bind(steps => graph.Material(layer.Material).Map(static m => m.Properties).Bind(props =>
                    ThermalOf(props).ToFin((Error)new ComputeFault.AssessmentInputMissing("<glaser-layer-missing-thermal>"))
                        .Map(thermal => steps.Add((
                            R: layer.Thickness.Meters / Math.Max(thermal.Conductivity.Si, double.Epsilon),
                            Z: thermal.VapourResistanceFactor * layer.Thickness.Meters / VapourPermeabilityAir))))))
                .Map(steps => Resolve(steps, climate))
            : Fin.Succ(Seq<GlaserProfile>());

    static Seq<GlaserProfile> Resolve(Seq<(double R, double Z)> steps, BoundaryClimate climate) {
        double totalR = 0.13 + 0.04 + steps.Sum(static s => s.R);
        double totalZ = steps.Sum(static s => s.Z);
        double rCum = 0.13, zCum = 0.0;
        return steps.Map(step => {
            rCum += step.R; zCum += step.Z;
            double tempC = climate.InteriorTempC - rCum / Math.Max(totalR, double.Epsilon) * (climate.InteriorTempC - climate.ExteriorTempC);
            double actualPa = climate.InteriorVapourPa - (totalZ > 0.0 ? zCum / totalZ : 0.0) * (climate.InteriorVapourPa - climate.ExteriorVapourPa);
            return new GlaserProfile(tempC, BoundaryClimate.SaturationPa(tempC), actualPa);
        });
    }

    static double GoverningU(Seq<AssessmentFact> facts, BoundaryClimate climate) =>
        climate.TargetUValueWM2K > 0.0
            ? facts.Choose(static f => f.Value is PropertyValue.Measure m ? Some(m.Value.Si) : None).Map(u => u / climate.TargetUValueWM2K).HeadOrNone().IfNone(0.0)
            : facts.Exists(static f => f.Value is PropertyValue.Boolean { Value: true }) ? 1.5 : 0.0;

    static Option<int> FirstCondensing(Seq<GlaserProfile> glaser) {
        int i = 0;
        foreach (GlaserProfile g in glaser) { if (g.Condensing) { return Some(i); } i++; }
        return None;
    }

    static Func<NodeId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) => id => graph.Material(id).Map(static m => m.Properties);
    static Option<MaterialPropertySet.Thermal> ThermalOf(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Thermal t ? Some(t) : None).HeadOrNone();
}
```

## [03]-[ACOUSTIC_RATING]

- Owner: `BuildingPhysics.RunAcoustic` the acoustic runner; the ISO 717 `Rw` single-number projection over the aggregator's layered SRI.
- Entry: `public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, ClockPolicy clocks)` — folds the ISO 12354 layered SRI through `AssemblyAggregator.Aggregate` (whose layered fold rides the SEAM `StcContourFit`), derives the ISO 717 `Rw` (the airborne sound-reduction single number) and the single-material `Nrc` where the target is a single material, and emits the `stc`/`rw`/`nrc` facts.
- Auto: the assembly STC is `AssemblyProperty.StcWeighted` (the aggregator's layered fold through the seam contour kernel); `Rw` is the same contour fit reported on the ISO 717 reference curve so the airborne rating and the STC share one contour owner; the `Nrc` reads the single-material seam `MaterialPropertySet.Acoustic.Nrc` projection where the target carries no `LayerSet`.
- Packages: LanguageExt.Core, Rasm.Element (project — `MaterialComposition`, `MaterialPropertySet.Acoustic`, `AcousticBand`), the `Analysis/aggregator` `AssemblyAggregator`, BCL inbox.
- Growth: a new acoustic rating (impact `Ln,w`, flanking `Dn,f,w`) is one fold over the same `LayerSet` reading the same SRI bands, never a parallel acoustic owner.
- Boundary: the multi-ply STC composes `AssemblyAggregator.Aggregate` so the layered sound-reduction and the contour fit are the seam-owned `StcContourFit`, never re-derived; the single-material `Nrc`/`Saa` are the seam intrinsic folds read directly off the `Acoustic` case, never recomputed here; a target with no acoustic property rails the typed input fault; the verdict is `Rw_target / Rw` when the route carries a target, else the rating is reported informationally.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ(Seq<AssessmentFact>()),
            (acc, id) => acc.Bind(facts => graph.CompositionOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<element-missing-composition>"))
                .Bind(composition => composition is MaterialComposition.LayerSet
                    ? AssemblyAggregator.Aggregate(composition, Resolver(graph), Seq<ConstituentWeight>())
                        .Map(property => facts
                            .Add(AssessmentFact.Ratio($"{id.Value}/stc", property.StcWeighted))
                            .Add(AssessmentFact.Ratio($"{id.Value}/rw", property.StcWeighted)))
                    : graph.Material(id).Map(static m => m.Properties).Map(props =>
                        AcousticOf(props).Match(
                            Some: a => facts.Add(AssessmentFact.Ratio($"{id.Value}/nrc", a.Nrc)).Add(AssessmentFact.Ratio($"{id.Value}/stc", a.StcWeighted)),
                            None: () => facts.Add(AssessmentFact.Text($"{id.Value}/acoustic", "absent")))))))
            .Map(facts => AssessmentResult.Of(request.Route, facts,
                facts.Choose(static f => f.Name.Value.EndsWith("/stc") && f.Value is PropertyValue.Bounded b ? Some(b.Value) : None).HeadOrNone().Map(static stc => 50.0 / Math.Max(stc, 1.0)).IfNone(0.0),
                new Provenance("BuildingPhysics", request.Route.Standard, "n/a", clocks.Now), clocks.Now));

    static Option<MaterialPropertySet.Acoustic> AcousticOf(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Acoustic a ? Some(a) : None).HeadOrNone();
}
```

## [04]-[FIRE_RESISTANCE]

- Owner: `BuildingPhysics.RunFire` the fire runner; `FireExposure` the exposure model (ISO 834 standard / hydrocarbon / external) and exposed-side count carried on the request; `SteelFireState` the incremental steel-temperature march; the EN 1993-1-2 critical-temperature fold and the EN 1992-1-2 concrete tabulated-cover check dispatched by the fire route.
- Entry: `public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, ClockPolicy clocks)` — dispatches the `en1993-1-2` steel critical-temperature march and the `en1992-1-2` concrete tabulated check, computing the achieved fire-resistance time against `request.RequiredMinutes`, and emits the `fire-resistance-minutes`/`critical-temperature`/`section-factor` facts.
- Auto: the steel fold marches the ISO 834 gas curve `θg = 20 + 345·log10(8t+1)` and the net heat flux `ḣ = αc(θg−θa) + Φεσ((θg+273)⁴−(θa+273)⁴)` over the section factor `Am/V` at a 5 s step, finds the time the steel temperature reaches the critical temperature `θa,cr = 39.19·ln(1/(0.9674·μ0³·⁸³³)−1) + 482` for the degree of utilization `μ0`, and the achieved resistance is that time; the concrete fold reads the tabulated minimum dimension/cover for the required rating.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Element (project — `SectionProperties`, `MaterialPropertySet.Mechanical`, `NodeId`), BCL inbox.
- Growth: a new fire model (a parametric EN 1991-1-2 natural fire, an EN 1995-1-2 timber charring rate) is one route plus one fold reading the same section, never a parallel fire owner; a new exposure is one `FireExposure` row.
- Boundary: the section factor `Am/V` reads the seam `SectionProperties` heated perimeter and area so fire and ambient design share one section source; the degree of utilization `μ0` is the ambient governing ratio carried on the request (the structural check feeds it), never re-solved here; the steel march is a genuine incremental integration to the critical temperature, not a tabulated approximation, and the receipt resistance is the marched time; a section with no fire-relevant geometry rails the typed input fault; the verdict is `RequiredMinutes / achieved` so an under-resistant member governs.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class FireExposure {
    public static readonly FireExposure Standard    = new("iso834",      sides: 4);
    public static readonly FireExposure Hydrocarbon = new("hydrocarbon", sides: 4);
    public static readonly FireExposure External    = new("external",    sides: 3);
    public int Sides { get; }
    public double GasTempC(double minutes) => Key == "hydrocarbon"
        ? 20.0 + 1080.0 * (1.0 - 0.325 * Math.Exp(-0.167 * minutes) - 0.675 * Math.Exp(-2.5 * minutes))
        : 20.0 + 345.0 * Math.Log10(8.0 * minutes + 1.0);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct SteelFireState(double Minutes, double SteelTempC);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    const double AlphaC = 25.0, Emissivity = 0.7, Sigma = 5.67e-8, RhoSteel = 7850.0, CSteel = 600.0, StepSeconds = 5.0;

    public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ(Seq<AssessmentFact>()),
            (acc, id) => acc.Bind(facts => graph.SectionOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<fire-member-missing-section>"))
                .Map(section => request.Route == AssessmentRoute.En1993Fire
                    ? SteelFire(section, request, facts, id)
                    : ConcreteFire(section, request, facts, id))))
            .Map(facts => AssessmentResult.Of(request.Route, facts,
                facts.Choose(static f => f.Name.Value.EndsWith("/fire-resistance-minutes") && f.Value is PropertyValue.Bounded b ? Some(b.Value) : None).HeadOrNone()
                    .Map(achieved => request.RequiredMinutes / Math.Max(achieved, double.Epsilon)).HeadOrNone().IfNone(0.0),
                new Provenance("BuildingPhysics", request.Route.Standard, "n/a", clocks.Now), clocks.Now));

    static Seq<AssessmentFact> SteelFire(SectionProperties section, AssessmentRequest.Fire request, Seq<AssessmentFact> facts, NodeId id) {
        double sectionFactor = section.HeatedPerimeter.Si / Math.Max(section.Area.Si, double.Epsilon);
        double criticalTempC = CriticalTemperature(request.Utilization);
        SteelFireState march = March(request.Exposure, sectionFactor, criticalTempC, request.RequiredMinutes + 30.0);
        return facts
            .Add(AssessmentFact.Ratio($"{id.Value}/fire-resistance-minutes", march.SteelTempC >= criticalTempC ? march.Minutes : request.RequiredMinutes + 30.0))
            .Add(AssessmentFact.Measure($"{id.Value}/critical-temperature", MeasureValue.Of(criticalTempC, "°C")))
            .Add(AssessmentFact.Measure($"{id.Value}/section-factor", MeasureValue.Of(sectionFactor, "1/m")));
    }

    static SteelFireState March(FireExposure exposure, double sectionFactor, double criticalTempC, double capMinutes) {
        double steelTempC = 20.0, minutes = 0.0;
        double ksh = 0.9; // shadow factor for I-sections (EN 1993-1-2 4.2.5.1)
        while (steelTempC < criticalTempC && minutes < capMinutes) {
            double gasTempC = exposure.GasTempC(minutes);
            double net = AlphaC * (gasTempC - steelTempC) + Emissivity * Sigma * (Math.Pow(gasTempC + 273.0, 4) - Math.Pow(steelTempC + 273.0, 4));
            steelTempC += ksh * sectionFactor / (CSteel * RhoSteel) * net * StepSeconds;
            minutes += StepSeconds / 60.0;
        }
        return new SteelFireState(minutes, steelTempC);
    }

    static double CriticalTemperature(double utilization) {
        double mu0 = Math.Clamp(utilization, 0.01, 0.99);
        return 39.19 * Math.Log(1.0 / (0.9674 * Math.Pow(mu0, 3.833)) - 1.0) + 482.0;
    }

    static Seq<AssessmentFact> ConcreteFire(SectionProperties section, AssessmentRequest.Fire request, Seq<AssessmentFact> facts, NodeId id) {
        double minDimensionMm = TabulatedMinimum(request.RequiredMinutes);
        double achieved = section.LeastDimension.Si * 1000.0 >= minDimensionMm ? request.RequiredMinutes : request.RequiredMinutes * section.LeastDimension.Si * 1000.0 / minDimensionMm;
        return facts
            .Add(AssessmentFact.Ratio($"{id.Value}/fire-resistance-minutes", achieved))
            .Add(AssessmentFact.Measure($"{id.Value}/required-min-dimension", MeasureValue.Of(minDimensionMm / 1000.0, "m")));
    }

    static double TabulatedMinimum(double requiredMinutes) =>
        requiredMinutes <= 30.0 ? 80.0 : requiredMinutes <= 60.0 ? 120.0 : requiredMinutes <= 90.0 ? 150.0 : requiredMinutes <= 120.0 ? 200.0 : 300.0;
}
```

## [05]-[RESEARCH]

- [ISO_6946_AND_13788]: the steady-state U-value is the ISO 6946 series resistance composed through `AssemblyAggregator.Aggregate` (the relocated engine owns the `Rsi`/`Rse` films and the `Σt/λ` fold), and the EN ISO 13788 Glaser model is the steady-state interstitial-condensation check — the interface temperature follows the cumulative thermal resistance, the saturation vapour pressure the Magnus form, the actual vapour pressure the cumulative vapour resistance `Z = Σ(μ·t)/δ0` between the boundary partial pressures, and an interface where the actual reaches the saturation pressure is a condensation plane. The fold reads the seam `MaterialPropertySet.Thermal.Conductivity` and `VapourResistanceFactor`; the boundary climate is the request scenario. Ripple counterpart: `Rasm.Element/Composition/material` (the seam `Thermal` case carrying the `VapourResistanceFactor` μ).
- [ISO_12354_AND_717]: the layered airborne sound-reduction index is the ISO 12354-1 series fold composed through `AssemblyAggregator.Aggregate`, whose per-band SRI rides the SEAM `StcContourFit` so the ISO 717 `Rw` single number and the STC share one ASTM-E413/ISO-717 contour owner; the single-material `Nrc`/`Saa` are the seam intrinsic folds read off the `Acoustic` case, never recomputed. The impact `Ln,w` and flanking `Dn,f,w` deepen as additional folds over the same SRI bands. Ripple counterpart: `Rasm.Element/Composition/acoustic` (the seam `StcContourFit` + `Nrc`/`Saa` intrinsic folds).
- [EN_1993_1_2_STEEL_FIRE]: the steel fire resistance is the EN 1993-1-2 critical-temperature method — the unprotected-steel temperature march `Δθa = ksh·(Am/V)/(ca·ρa)·ḣnet·Δt` over the ISO 834 gas curve (or the hydrocarbon curve for the exposure row), the net heat flux the convective `αc(θg−θa)` plus radiative `Φεσ((θg+273)⁴−(θa+273)⁴)` terms, integrated at a 5 s step to the critical temperature `θa,cr = 39.19·ln(1/(0.9674·μ0³·⁸³³)−1) + 482` for the degree of utilization `μ0` (the ambient governing ratio carried on the request). The section factor `Am/V` reads the seam `SectionProperties.HeatedPerimeter`/`Area`, so fire and ambient design share one section. This is a genuine incremental integration, never a tabulated approximation.
- [EN_1992_1_2_CONCRETE_FIRE]: the concrete fire resistance is the EN 1992-1-2 tabulated method — the minimum cross-section dimension and axis distance for the required rating, read against the seam `SectionProperties.LeastDimension`; the 500 °C isotherm deepens as a fold over the section thermal field where a tabulated check is insufficient. The two fire models dispatch by route (`en1993-1-2` / `en1992-1-2`) on one `RunFire` kernel, never a parallel steel/concrete fire owner.
- [DISCIPLINE_COLLAPSE]: thermal, acoustic, and fire collapse onto ONE `BuildingPhysics` kernel because all three are closed-form ISO/EN folds over an assembly/section read from the graph — distinct from the `Analysis/energy` simulation rail (which builds an OpenStudio model and runs the EnergyPlus subprocess) and the `Analysis/structural` FE rail (which assembles and solves a frame). The shared shape (read composition/section → fold ISO/EN closed form → emit fact stream) is one owner dispatched by `Discipline`, the `COLLAPSE_SCAN` collapse of three parallel runner types. Each composes the `Analysis/aggregator` for the multi-ply rollup where a layered property is needed.
