# [COMPUTE_PHYSICS]

Rasm.Compute closed-form building-physics runner collapses the `Discipline.Thermal`/`Discipline.Acoustic`/`Discipline.Fire` arms of the `Analysis/assessment` rail onto one `BuildingPhysics` kernel — all three are closed-form ISO/EN folds over an assembly or section read directly from the concrete `Rasm.Element` `ElementGraph`, no external solver and no subprocess. Thermal reads the multi-ply `UValueWM2K` from `AssemblyAggregator.Aggregate` (the one ISO 6946 series-resistance owner) and runs the EN ISO 13788 Glaser interstitial-condensation profile, computing the condensation mass rate through the genuine lower-convex-hull tangent construction rather than a bare boolean; a window assembly instead composes `AssemblyAggregator.AggregateWindow` for the EN ISO 10077-1 area-and-perimeter-weighted whole-window `Uw` the through-thickness series cannot reach. Acoustic reads the layered mass-law `StcWeighted` (ASTM E413) from the aggregator for a `LayerSet` and the intrinsic seam `Nrc`/`Rw` (ISO 717-1) for a single material; Fire folds the EN 1993-1-2 unprotected-steel critical-temperature march and the EN 1992-1-2 concrete tabulated minimum-dimension check, dispatched by the fire route.

Each runner returns one `AssessmentResult` fact stream, the governing ratio threaded through the fold accumulator (`U / U_target` and the vapour utilization for thermal, `RequiredMinutes / achieved` for fire) so the verdict derives from the in-scope governing quantity, never a re-parse of the emitted facts. Every measured fact is SI-native through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.OfSi(Dimension, si)` or the raw `MeasureValue` record for a domain-labelled scalar, never the phantom 2-arg `MeasureValue.Of(value, unit)` the seam factory does not expose. This runner composes the settled seam vocabulary — `MaterialComposition`/`MaterialLayer`, the `MaterialPropertySet.Thermal`/`Acoustic` cases through `MaterialPropertyAccess`, `SectionProperties`, `MeasureValue`/`Dimension` — plus the `Analysis/aggregator` engine; the single-material pure acoustic folds stay seam-owned, the multi-ply layered physics this runner, whose acoustic mass-law fold rides the seam `Composition/acoustic#ACOUSTIC_FOLDS` `RatingContour.Stc.Fit` contour — one contour owner, never a second algorithm.

## [01]-[INDEX]

- [01]-[THERMAL_ENVELOPE]: the ISO 6946 series-`U` and EN ISO 13788 Glaser condensation profile for an envelope, and the EN ISO 10077-1 whole-window `Uw` for a window.
- [02]-[ACOUSTIC_RATING]: the ISO 12354 layered `StcWeighted` and the single-material seam `Nrc`/`Rw` (ISO 717-1).
- [03]-[FIRE_RESISTANCE]: the EN 1993-1-2 steel critical-temperature march and the EN 1992-1-2 concrete tabulated check, dispatched by the fire route.

## [02]-[THERMAL_ENVELOPE]

- Owner: `BuildingPhysics.RunThermal` the thermal runner; `BoundaryClimate` the interior/exterior temperature-and-humidity boundary carried on the request; `GlaserProfile` the per-interface temperature/saturation/actual-vapour-pressure receipt; `GlaserResult` the condensation summary (the vapour utilization, the lower-hull condensation rate, the worst interface) the fold derives; `WindowFields` the Op-free window-assembly resolver that reads a window's glazed/frame `WindowField` set off its `Compose` parts + baked `Qto`/`Pset` bags.
- Entry: `public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, ClockPolicy clocks)` resolves the WINDOW path first (a window carries glazed/frame `WindowField`s): a non-empty field set composes `AggregateWindow` for the whole-window `Uw` and emits the `whole-window-u`/`glazed-u`/`frame-u`/`edge-bridge`/`glazed-fraction` facts threading `Uw / U_target`; else it falls to the envelope path — the `UValueWM2K` from `Aggregate` for a `LayerSet` (the intrinsic `Thermal.UValue` for a `Single`), the EN ISO 13788 Glaser fold over the `LayerSet`, and the `u-value`/`condensation-risk`/`vapour-utilization`/`condensation-rate`/`condensation-plane` facts threading `max(U / U_target, vapour-utilization)`; `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a layer material is absent or lacks a thermal property, or a window part lacks its `Thermal.UValue`.
- Auto: the per-interface temperature, Magnus saturation pressure, and straight actual-vapour line fold over the cumulative thermal and vapour resistances (including the `Rsi`/`Rse` films); the EN ISO 13788 condensation construction is the lower convex hull of the `(Z, p)` points, its interior vertices the condensation planes and `g_c` the flux discontinuity between adjacent vertices; the window path resolves each `Compose` part Op-free — the glazed `Ug`/frame `Uf` off `graph.PropertiesOf(part).Thermal.UValue.Si`, the glazed/frame areas off the window's `Qto_*BaseQuantities`, the spacer `Ψg` off its `Pset` thermal-bridge property — assembling the `WindowField.Glazed`/`Frame` set `AggregateWindow` folds.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialPropertySet.Thermal` via `MaterialPropertyAccess`, `MaterialId`, `MeasureValue`, `Dimension`, `NodeId`, the `Node.Object`/`Node.QuantitySet`/`Node.PropertySet` cases + `Relationship.Compose`/`Relationship.Assign`/`ComposeKind`/`AssignKind` the window-part/bag incidence read resolves, `PropertyName`, `PropertyValue`, `QuantityBag`/`PropertyBag`), the `Analysis/aggregator` `AssemblyAggregator` (the `Aggregate` series-U AND the `AggregateWindow` whole-window fold + the `WindowField`/`WindowU` shapes), BCL inbox (`Math`, span/array hull kernel).
- Growth: a new thermal check (a dynamic decrement/admittance pair) is one fold over the same `LayerSet` reading the aggregator's `ArealHeatCapacityKJM2K`/`VapourResistanceSdM`, never a parallel envelope owner; the moisture model deepens to the EN 15026 transient form as one fold swap reading the same layer resistances; a 12-month annual condensation/evaporation balance is one fold over a climate series once `BoundaryClimate` carries one; the whole-window `Uw` is the `AssemblyAggregator.AggregateWindow` compose (the EN ISO 10077-1 area-and-perimeter weighting the through-thickness series cannot reach), and a richer EN ISO 10077-2 numerical frame model is one deeper `WindowField` resolution (a per-frame-profile measured `Uf`/`Ψg`) the runner supplies, never a parallel window owner.
- Boundary: the multi-ply `U` composes `AssemblyAggregator.Aggregate` so the thermal envelope and the aggregator share one series-resistance owner, never a re-derived U; the Glaser fold independently reads each layer's `Thermal.Conductivity.Si` and `VapourResistanceFactor` for the per-interface profile the aggregator's total `Sd` cannot carry — a layer missing the thermal property rails the typed fault; the resolver is keyed on the composition's native `MaterialId`, never a graph `NodeId`; the condensation construction is the genuine EN ISO 13788 lower-hull method computing the condensation mass rate `g_c`, not a bare crossing flag. Whole-window `Uw` composes `AssemblyAggregator.AggregateWindow` (the EN ISO 10077-1 owner) — the runner resolves the glazed `Ug` and frame `Uf` seam-direct off each part's `Thermal.UValue.Si` and area-weights them with the spacer linear bridge rather than reporting the raw glazing `Ug` as the window U. Spacer `Ψg` reads off the window's `Pset` thermal-bridge property (`SpacerType.PsiWmK` lowers there — the seam `Thermal` carries no perimeter-bridge column, so a `Ψ`-on-`Thermal` read is the phantom the runner never takes), the areas off its `Qto_*BaseQuantities`; the window path is tried first, so a wall/slab with no window parts takes the series-U+Glaser path unchanged. Governing ratio is `max(Uw / U_target)` for a window and `max(U / U_target, vapour-utilization)` for an envelope, threaded through the accumulator so the verdict never re-parses the emitted facts; every measured fact is SI-native, never the phantom 2-arg `MeasureValue.Of(value, unit)`.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The interior/exterior boundary the AssessmentRequest.Thermal case carries; the five fields the
// Analysis/assessment CanonicalBytes folds. The vapour partial pressures derive from the Magnus
// saturation curve and the boundary relative humidities, so the climate carries no redundant pressure field.
public readonly record struct BoundaryClimate(double InteriorTempC, double InteriorRh, double ExteriorTempC, double ExteriorRh, double TargetUValueWM2K) {
    public static readonly BoundaryClimate WinterDesign = new(20.0, 0.50, -5.0, 0.85, TargetUValueWM2K: 0.30);
    public double InteriorVapourPa => SaturationPa(InteriorTempC) * InteriorRh;
    public double ExteriorVapourPa => SaturationPa(ExteriorTempC) * ExteriorRh;
    public static double SaturationPa(double tC) => 610.5 * Math.Exp(17.269 * tC / (237.3 + tC));
}

// The per-internal-interface Glaser receipt: the interface ordinal (1-based, between layer k and k+1),
// the steady-state temperature, the saturation pressure at that temperature, the un-redistributed straight
// actual vapour pressure, and the cumulative vapour resistance Z. Condensing/Utilization are derived reads.
public readonly record struct GlaserProfile(int Interface, double TempC, double SaturationPa, double ActualPa, double VapourResistanceCum) {
    public bool Condensing => ActualPa >= SaturationPa;
    public double Utilization => SaturationPa > 0.0 ? ActualPa / SaturationPa : 0.0;
}

// The condensation summary the Glaser fold derives: whether condensation occurs (a positive lower-hull rate or a
// saturated interface), the worst-interface vapour-pressure utilization (the governing condensation ratio), the
// EN ISO 13788 condensation MASS rate, and the worst interface index + label. None is the clean (no-condensation) seed.
public readonly record struct GlaserResult(bool Condensing, double VapourUtilization, double CondensationRateKgM2S, int PlaneIndex, string PlaneLabel) {
    public static readonly GlaserResult None = new(false, 0.0, 0.0, -1, "none");
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    const double RsiWM2K = 0.13;                 // ISO 6946 interior surface film
    const double RseWM2K = 0.04;                 // ISO 6946 exterior surface film
    const double VapourPermeabilityAir = 2.0e-10; // delta0, kg/(m.s.Pa)
    const double VapourRateTolerance = 1.0e-12;   // a condensation rate below this reads as no accumulation

    // SI dimensions composed from the seam Dimension algebra (no named row exists) — the kernel-true discriminator a
    // downstream unit canonicalization re-reads, never a hand-mapped kind. Each fact mints through the seam's labeled
    // OfSi(QuantityType.OfDimension(dim), dim, si, unit), the conventional label riding CanonicalUnit under the finite
    // gate — never the private record ctor nor the unlabeled OfSi that discards the label.
    static readonly Dimension TemperatureDim = Dimension.Create(0, 0, 0, 0, 1, 0, 0);
    static readonly Dimension PerLengthDim = Dimension.Dimensionless.Divide(Dimension.LengthDim);
    static readonly Dimension VapourFluxDim = Dimension.MassDim.Divide(Dimension.AreaDim).Divide(Dimension.DurationDim);
    // The EN ISO 10077-1 edge-seal bridge Σ lg·Ψg is a thermal conductance W/K = [M·L²·T⁻³·K⁻¹] (the area-U ThermalTransmittance
    // W·m⁻²·K⁻¹ times area m²) — the perimeter linear-bridge term the whole-window fact carries beside the area-U facts.
    static readonly Dimension EdgeBridgeDim = Dimension.ThermalTransmittanceDim.Multiply(Dimension.AreaDim);

    // The seam-keyed resolver every layered fold shares: a ply MaterialId -> its material node's property set,
    // railing the missing-input fault on an absent material node so a fold reads the composition's OWN plies by
    // native key (graph.Material(MaterialId)), never a graph NodeId lookup.
    static Func<MaterialId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) =>
        mid => graph.Material(mid).Map(static m => m.Properties).ToFin(Missing($"<material-absent:{mid.Value}>"));

    static Error Missing(string detail) => new ComputeFault.AssessmentInputMissing(detail);

    public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Governing: 0.0)),
            // The window path is tried FIRST: a window assembly resolves a non-empty glazed/frame WindowField set, and
            // AggregateWindow composes the EN ISO 10077-1 whole-window Uw; a NON-window target (empty field set) falls
            // through to the through-thickness envelope path (series-U + Glaser). The two are mutually exclusive per target.
            (acc, id) => acc.Bind(state => WindowFields(graph, id).Bind(fields =>
                fields.IsEmpty ? Envelope(graph, request, id, state) : Window(fields, request, id, state))))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts, state.Governing,
                new Provenance("BuildingPhysics", request.Route.Standard, "closed-form", clocks.Now)));

    // The EN ISO 10077-1 whole-window branch: AggregateWindow folds the resolved glazed/frame fields into the area-and-
    // perimeter-weighted Uw, and the runner emits the whole-window-u plus the glazed/frame/edge-bridge/glazed-fraction
    // breakdown, threading Uw/U_target (a LOWER Uw is better, so achieved-over-target like the envelope U). A degenerate
    // window (zero total area) rails inside AggregateWindow; a window with a non-empty field set always yields a Uw fact.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Window(Seq<WindowField> fields, AssessmentRequest.Thermal request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from w in AssemblyAggregator.AggregateWindow(fields)
        from uw in MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, w.UwWM2K)
        from ug in MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, w.UgWM2K)
        from uf in MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, w.UfWM2K)
        from edge in MeasureValue.OfSi(QuantityType.OfDimension(EdgeBridgeDim), EdgeBridgeDim, w.EdgeBridgeW_K, "W/K")
        from fraction in AssessmentFact.Ratio($"{id.Value}/glazed-fraction", w.GlazedFraction)
        let uRatio = request.Climate.TargetUValueWM2K > 0.0 ? w.UwWM2K / request.Climate.TargetUValueWM2K : 0.0
        select (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/whole-window-u", uw))
                .Add(AssessmentFact.Measure($"{id.Value}/glazed-u", ug))
                .Add(AssessmentFact.Measure($"{id.Value}/frame-u", uf))
                .Add(AssessmentFact.Measure($"{id.Value}/edge-bridge", edge))
                .Add(fraction),
            Governing: Math.Max(state.Governing, uRatio));

    // The through-thickness envelope branch (the prior RunThermal body): the ISO 6946 series-U from AssemblyAggregator
    // .Aggregate (a LayerSet) or the intrinsic Thermal.UValue (a Single), plus the EN ISO 13788 Glaser condensation fold.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Envelope(ElementGraph graph, AssessmentRequest.Thermal request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from composition in graph.CompositionOf(id).ToFin(Missing($"<thermal-element-missing-composition:{id.Value}>"))
        from u in UValue(composition, Resolver(graph))
        from glaser in composition is MaterialComposition.LayerSet set
            ? GlaserOf(set, Resolver(graph), request.Climate)
            : Fin.Succ(GlaserResult.None)
        from uMeasure in MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, u)
        from vapour in AssessmentFact.Ratio($"{id.Value}/vapour-utilization", glaser.VapourUtilization)
        from rate in MeasureValue.OfSi(QuantityType.OfDimension(VapourFluxDim), VapourFluxDim, glaser.CondensationRateKgM2S, "kg/(m2.s)")
        let uRatio = request.Climate.TargetUValueWM2K > 0.0 ? u / request.Climate.TargetUValueWM2K : 0.0
        select (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/u-value", uMeasure))
                .Add(AssessmentFact.Flag($"{id.Value}/condensation-risk", glaser.Condensing))
                .Add(vapour)
                .Add(AssessmentFact.Measure($"{id.Value}/condensation-rate", rate))
                .Add(AssessmentFact.Text($"{id.Value}/condensation-plane", glaser.PlaneLabel)),
            Governing: Math.Max(state.Governing, Math.Max(uRatio, glaser.VapourUtilization)));

    // The U-value owner: a LayerSet reads the ISO 6946 series U from the relocated aggregator (one owner), a Single
    // its material's intrinsic Thermal.UValue; a ProfileSet/ConstituentSet rails (no through-thickness envelope). The
    // generated total Switch breaks at compile time if the seam adds a composition case, never a runtime-silent _ arm.
    static Fin<double> UValue(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve) =>
        composition.Switch(
            single:         s   => resolve(s.Material).Bind(props => props.Thermal.Map(static t => t.UValue.Si).ToFin(Missing("<thermal-single-missing-u-value>"))),
            layerSet:       set => AssemblyAggregator.Aggregate(set, resolve).Map(static p => p.UValueWM2K),
            profileSet:     _   => Fin.Fail<double>(Missing("<thermal-requires-layerset-or-single>")),
            constituentSet: _   => Fin.Fail<double>(Missing("<thermal-requires-layerset-or-single>")));

    // --- [WINDOW_ASSEMBLY] ---------------------------------------------------------------
    // The EN ISO 10077-1 window-field resolver: read the window's Compose parts and its baked Qto/Pset bags Op-free,
    // assembling the glazed/frame WindowField set. A target with no GlazingArea quantity is not a window (an empty set
    // falling through to the envelope path) — GlazingArea is the seam-direct discriminant. A window component whose
    // material lacks its Thermal.UValue rails the typed fault; the frame area is Area − GlazingArea, the edge length GlazingPerimeter.
    static Fin<Seq<WindowField>> WindowFields(ElementGraph graph, NodeId window) {
        Option<double> glazedArea = Quantity(graph, window, "GlazingArea");
        if (glazedArea.IsNone) { return Fin.Succ(Seq<WindowField>()); }   // not a window — fall through to the envelope path
        double ag = glazedArea.IfNone(0.0);
        double totalArea = Quantity(graph, window, "Area").IfNone(ag);
        double af = Math.Max(totalArea - ag, 0.0);
        double edgeLength = Quantity(graph, window, "GlazingPerimeter").IfNone(0.0);
        double psi = Property(graph, window, "GlazingEdgePsi").IfNone(0.0);
        // The glazing-infill part carries the EN 673 Ug, the frame part the Uf — each discriminated by its composition
        // shape (glazing a LayerSet/Single IGU, frame a ProfileSet member). Ag is the Qto GlazingArea, Af the residual
        // Area − GlazingArea, lg the GlazingPerimeter, Ψg the element Pset property.
        return PartU(graph, window, isGlazing: true).Bind(ug =>
            PartU(graph, window, isGlazing: false).Map(uf =>
                Seq(WindowField.Glazed(ug, ag, edgeLength, psi), WindowField.Frame(uf, af))));
    }

    // The glazed-or-frame part's transmittance: the Compose child whose material carries the glazed or frame
    // Thermal.UValue, resolved Op-free — glazing a LayerSet/Single composition, frame a ProfileSet. A missing part, or a
    // part missing its Thermal.UValue, rails the typed fault (an under-specified window assembly).
    static Fin<double> PartU(ElementGraph graph, NodeId window, bool isGlazing) =>
        WindowPart(graph, window, isGlazing)
            .ToFin(Missing($"<window-missing-{(isGlazing ? "glazing" : "frame")}-part:{window.Value}>"))
            .Bind(part => graph.PropertiesOf(part).Thermal.Map(static t => t.UValue.Si)
                .ToFin(Missing($"<window-{(isGlazing ? "glazing" : "frame")}-missing-u-value:{part.Value}>")));

    // The window's glazing-infill (LayerSet/Single composition — the IGU) or frame (ProfileSet composition — the profile)
    // Compose child: the OWNING Compose parts (Aggregate/Nest) off the incidence index, discriminated by the part's
    // composition shape, never a baked Element (the runner holds no Op) and never a host type.
    static Option<NodeId> WindowPart(ElementGraph graph, NodeId window, bool isGlazing) =>
        toSeq(graph.EdgesAt(window))
            .Choose(e => e is Relationship.Compose c && c.Whole == window && (c.SubKind == ComposeKind.Aggregate || c.SubKind == ComposeKind.Nest) ? Some(c.Part) : None)
            .Find(part => graph.CompositionOf(part).Match(
                Some: comp => isGlazing ? comp is MaterialComposition.LayerSet or MaterialComposition.Single : comp is MaterialComposition.ProfileSet,
                None: () => false));

    // A window's Qto_*BaseQuantities value read Op-free off the Assign.PropertyDefinition-bound QuantitySet node (the SI
    // magnitude of the measured quantity); None when the window carries no such quantity (the non-window fall-through).
    static Option<double> Quantity(ElementGraph graph, NodeId window, string name) =>
        toSeq(graph.EdgesAt(window))
            .Choose(e => e is Relationship.Assign a && a.Subject == window && a.SubKind == AssignKind.PropertyDefinition && graph.Find<Node.QuantitySet>(a.Definition).IsSome
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Choose(q => q.Bag.Values.Find(PropertyName.Create(name)))
            .Head.Map(static m => m.Si);

    // A window's Pset thermal-bridge property read Op-free off the Assign.PropertyDefinition-bound PropertySet node — the
    // spacer Ψg (SpacerType.PsiWmK) the glazing family lowers onto the element Pset (NOT onto MaterialPropertySet.Thermal,
    // which carries no perimeter-bridge column). A Measure value yields its SI magnitude; an absent/non-Measure property None.
    static Option<double> Property(ElementGraph graph, NodeId window, string name) =>
        toSeq(graph.EdgesAt(window))
            .Choose(e => e is Relationship.Assign a && a.Subject == window && a.SubKind == AssignKind.PropertyDefinition && graph.Find<Node.PropertySet>(a.Definition).IsSome
                ? graph.Find<Node.PropertySet>(a.Definition) : None)
            .Choose(p => p.Bag.Values.Find(PropertyName.Create(name)))
            .Head.Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None);

    // --- [GLASER_TANGENT] ----------------------------------------------------------------
    // The EN ISO 13788 steady-state interstitial-condensation fold over the LayerSet plies: each layer's thermal
    // resistance R = t/lambda and vapour resistance Z = mu.t/delta0 resolved through the native-key resolver, then the
    // per-interface temperature/saturation/actual profile and the lower-hull condensation-rate construction.
    static Fin<GlaserResult> GlaserOf(MaterialComposition.LayerSet set, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, BoundaryClimate climate) =>
        set.Layers.Fold(
            Fin.Succ(Seq<LayerResistance>()),
            (acc, layer) => acc.Bind(steps => resolve(layer.Material)
                .Bind(props => props.Thermal.ToFin(Missing($"<glaser-layer-missing-thermal:{layer.Material.Value}>")))
                .Map(thermal => steps.Add(new LayerResistance(
                    R: layer.Thickness.Si / Math.Max(thermal.Conductivity.Si, double.Epsilon),
                    Z: thermal.VapourResistanceFactor * layer.Thickness.Si / VapourPermeabilityAir,
                    Name: layer.LayerName)))))
            .Map(steps => Condensation(steps, climate));

    // The local fold state: one ply's thermal + vapour resistance plus its layer name for the condensation-plane label.
    readonly record struct LayerResistance(double R, double Z, string Name);

    static GlaserResult Condensation(Seq<LayerResistance> steps, BoundaryClimate climate) {
        int n = steps.Count;
        double rTot = RsiWM2K + RseWM2K + steps.Sum(static s => s.R);
        double zTot = steps.Sum(static s => s.Z);
        if (n < 2 || zTot <= 0.0) { return GlaserResult.None; }   // a single layer has no internal interface to condense at
        double pInt = climate.InteriorVapourPa, pExt = climate.ExteriorVapourPa;
        double dTemp = climate.InteriorTempC - climate.ExteriorTempC;
        Seq<GlaserProfile> profile = toSeq(System.Linq.Enumerable.Range(1, n - 1)).Fold(
            (Built: Seq<GlaserProfile>(), RCum: RsiWM2K, ZCum: 0.0),
            (acc, k) => {
                double rCum = acc.RCum + steps[k - 1].R, zCum = acc.ZCum + steps[k - 1].Z;
                double tempC = climate.InteriorTempC - rCum / rTot * dTemp;
                double actual = pInt - zCum / zTot * (pInt - pExt);
                return (acc.Built.Add(new GlaserProfile(k, tempC, BoundaryClimate.SaturationPa(tempC), actual, zCum)), rCum, zCum);
            }).Built;
        return toSeq(profile.OrderByDescending(static g => g.Utilization)).Head.Match(
            Some: worst => {
                double rate = CondensationRate(profile, zTot, pInt, pExt);
                bool condensing = rate > VapourRateTolerance || worst.Utilization >= 1.0;
                return new GlaserResult(condensing, worst.Utilization, rate,
                    condensing ? worst.Interface : -1,
                    condensing ? $"interface-{worst.Interface}:{steps[worst.Interface - 1].Name}" : "none");
            },
            None: () => GlaserResult.None);
    }

    // The EN ISO 13788 condensation rate as the lower-convex-hull construction (Exemption: array hull kernel): the
    // realized vapour line is the lower hull of the (Z, p) points, its interior vertices the condensation planes and g_c
    // the inflow-minus-outflow flux discontinuity at each, summed over the planes.
    static double CondensationRate(Seq<GlaserProfile> profile, double zTot, double pInt, double pExt) {
        int m = profile.Count;
        double[] z = new double[m + 2], y = new double[m + 2];
        z[0] = 0.0; y[0] = pInt;
        for (int i = 0; i < m; i++) { z[i + 1] = profile[i].VapourResistanceCum; y[i + 1] = profile[i].SaturationPa; }
        z[m + 1] = zTot; y[m + 1] = pExt;
        int[] hull = LowerHull(z, y);
        double gc = 0.0;
        for (int i = 1; i < hull.Length - 1; i++) {
            int u = hull[i - 1], v = hull[i], w = hull[i + 1];
            gc += (y[u] - y[v]) / (z[v] - z[u]) - (y[v] - y[w]) / (z[w] - z[v]);
        }
        return gc;
    }

    // Andrew's monotone-chain LOWER hull over points pre-sorted by Z (the cumulative vapour resistance is monotone):
    // pop while the last triple does not turn counter-clockwise, so a saturation point dipping below the straight
    // interior->exterior line enters the hull as a condensation plane and a point above it is excluded.
    static int[] LowerHull(double[] z, double[] y) {
        int count = z.Length, h = 0;
        int[] hull = new int[count];
        for (int i = 0; i < count; i++) {
            while (h >= 2 && Cross(z, y, hull[h - 2], hull[h - 1], i) <= 0.0) { h--; }
            hull[h++] = i;
        }
        return hull[..h];
    }

    static double Cross(double[] z, double[] y, int o, int a, int b) =>
        (z[a] - z[o]) * (y[b] - y[o]) - (y[a] - y[o]) * (z[b] - z[o]);
}
```

## [03]-[ACOUSTIC_RATING]

- Owner: `BuildingPhysics.RunAcoustic` the acoustic runner; the weighted single-number sound-reduction projection over the aggregator's layered mass-law `StcWeighted` (ASTM E413) and the single-material seam `Nrc`/`Rw` (ISO 717-1).
- Entry: `public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, ClockPolicy clocks)` reads the layered mass-law single number from `Aggregate` (whose per-band SRI folds through the seam `RatingContour.Stc.Fit` ASTM E413 contour, yielding `StcWeighted`) for a `LayerSet`, and the intrinsic seam `Nrc`/`Rw` (the `RatingContour.Rw.Fit` ISO 717-1 contour over the material's measured SRI spectrum) off the `PrimaryMaterial` for a single material, emits the `sound-reduction-index`/`nrc` facts, and threads `request.RequiredRw / Rw` through the accumulator when the request carries a `RequiredRw`. A deliberate cross-standard asymmetry: the single material has a measured spectrum yielding a true ISO 717-1 `Rw`, the layered assembly only a mass-law estimate whose ASTM E413 `StcWeighted` is all its areal-mass data admits — so the ISO `RequiredRw` demand judges the single-material path against `Rw` and the assembly against its `StcWeighted`, never an ISO demand against an ASTM rating on one path.
- Auto: the assembly weighted index is the aggregator's `StcWeighted` (the mass-law per-band SRI folded once through the seam `RatingContour.Stc.Fit` contour — the assembly carrier exposes no `Rw`); the single-material `Nrc`/`Rw` read the seam `Acoustic` projections off the `PrimaryMaterial` (the `RatingContour.Rw.Fit` ISO 717-1 contour over the measured SRI spectrum, differing from `StcWeighted` only by the contour row, never recomputed here); both contour rows share one `RatingContour` owner; the single-number rating is a dimensionless dB-weighted `MeasureValue`.
- Packages: LanguageExt.Core, Rasm.Element (project — `MaterialComposition`, `MaterialPropertySet.Acoustic` via `MaterialPropertyAccess`, `MeasureValue`, `Dimension`, `NodeId`), the `Analysis/aggregator` `AssemblyAggregator`, BCL inbox.
- Growth: the airborne spectrum-adaptation deepening (flanking `Dn,f,w`, the ISO 717-1 `C`/`Ctr` terms) is one fold over the same per-band SRI; the impact `Ln,w` (ISO 717-2 / IIC) is the descending sibling that lands once this runner carries the assembly normalized-impact spectrum — the EN 12354-2 floating-floor `ΔL_w` computed from the resilient layer's seam `Acoustic.DynamicStiffnessMNPerM3` and rated through the deferred descending `RatingContour` row via the shared sign-agnostic `RatingContour.Fit`, the seam `Composition/acoustic#ACOUSTIC_FOLDS` note carrying the counterpart.
- Boundary: the multi-ply index composes `Aggregate` so the layered sound reduction is the seam-owned `RatingContour.Stc.Fit` ASTM E413 mass-law estimate (the assembly carrier models no `Rw`), never a second STC/`Rw` algorithm; the single-material `Nrc`/`Rw` are the seam intrinsic folds off the `Acoustic` case (the ISO 717-1 `RatingContour.Rw.Fit`, judged against the ISO `RequiredRw` demand, the matched-standard pairing the assembly's `StcWeighted` cannot offer), never recomputed; the single-material branch resolves through the composition's `PrimaryMaterial`, never the element `NodeId`, and a target with no acoustic property reports the `absent` text fact rather than a fabricated rating. A `RequiredRw` acceptance target yields the governing ratio `RequiredRw / Rw` (a higher Rw is better) and a genuine pass/fail, while a `RequiredRw <= 0` request reverts to the informational rating (governing `double.NaN`, propagated by `Math.Max` across the fold → `NotApplicable`, the no-target convention, never a misleading `0.0`-ratio `Satisfied`).

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Governing: 0.0)),
            (acc, id) => acc.Bind(state => graph.CompositionOf(id).ToFin(Missing($"<acoustic-element-missing-composition:{id.Value}>"))
                .Bind(composition => composition is MaterialComposition.LayerSet set
                    ? AssemblyAggregator.Aggregate(set, Resolver(graph)).Bind(property => RateAcoustic(id, property.StcWeighted, None, request, state))
                    : graph.Material(composition.PrimaryMaterial).Map(static m => m.Properties).ToFin(Missing($"<acoustic-material-absent:{id.Value}>"))
                        .Bind(props => props.Acoustic.Match(
                            Some: a => RateAcoustic(id, a.Rw, Some(a.Nrc), request, state),   // ISO 717-1 Rw (the SEAM Acoustic carrier's RatingContour.Rw.Fit over the measured SRI spectrum) — judged against the ISO RequiredRw demand, NOT the ASTM E413 StcWeighted contour the assembly mass-law estimate yields
                            None: () => FinSucc((state.Facts.Add(AssessmentFact.Text($"{id.Value}/acoustic", "absent")), state.Governing))))))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts, state.Governing,
                new Provenance("BuildingPhysics", request.Route.Standard, "closed-form", clocks.Now)));

    // The single-number sound-reduction fact (+ the single-material Nrc when present) plus the RequiredRw/rating governing
    // ratio: a HIGHER rating is better, so the demand/capacity ratio is required-over-achieved (the same orientation as the
    // fire RequiredMinutes/achieved) — RequiredRw <= 0 is informational (ratio double.NaN, which Math.Max propagates across
    // the multi-target fold so the run bands NotApplicable, never a misleading 0.0-ratio Satisfied — the energy/carbon/cost
    // no-target convention), a real RequiredRw target yields a genuine pass/fail the spine bands through AssessmentVerdict.FromRatio.
    // The `rating` slot is the SINGLE-material ISO 717-1 Rw (caller passes a.Rw over the measured SRI spectrum, the matched
    // standard for the ISO RequiredRw demand) or the ASSEMBLY ASTM E413 StcWeighted mass-law estimate (the layered buildup
    // exposes no Rw) — a dual-standard single-number slot, never an ISO demand judged against an ASTM rating on the same path.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> RateAcoustic(NodeId id, int rating, Option<double> nrc, AssessmentRequest.Acoustic request, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from sri in MeasureValue.OfSi(QuantityType.OfDimension(Dimension.Dimensionless), Dimension.Dimensionless, rating, "dB")
        from facts in nrc.Match(
            Some: n => AssessmentFact.Ratio($"{id.Value}/nrc", n)
                .Map(fact => state.Facts.Add(AssessmentFact.Measure($"{id.Value}/sound-reduction-index", sri)).Add(fact)),
            None: () => FinSucc(state.Facts.Add(AssessmentFact.Measure($"{id.Value}/sound-reduction-index", sri))))
        let ratio = request.RequiredRw > 0.0 ? request.RequiredRw / Math.Max(rating, double.Epsilon) : double.NaN
        select (facts, Math.Max(state.Governing, ratio));
}
```

## [04]-[FIRE_RESISTANCE]

- Owner: `BuildingPhysics.RunFire` the fire runner; `FireExposure` the `[SmartEnum<string>]` exposure model carrying the exposed-side count, the convection coefficient `α_c`, and the nominal gas-temperature-time curve delegate; `SteelFireState` the incremental steel-temperature march receipt; the EN 1993-1-2 critical-temperature fold and the EN 1992-1-2 concrete tabulated minimum-dimension check dispatched by the fire route.
- Entry: `public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, ClockPolicy clocks)` — resolves each member's `SectionProperties` off its `ProfileSet` composition, dispatches the `en1993-1-2` steel critical-temperature march and the `en1992-1-2` concrete tabulated check, emits the `fire-resistance-minutes`/`critical-temperature`/`section-factor` (steel) and `required-min-dimension`/`least-dimension`/`required-axis-distance`/`axis-distance` (concrete) facts, and threads `max(RequiredMinutes / achieved)` through the accumulator (the concrete `achieved` the WORSE of the dimension- and axis-distance-governed resistances per the member-type EN 1992-1-2 table).
- Auto: the steel fold marches the exposure's gas curve and the net convective+radiative flux over the section factor `Am/V` at a 5 s step with the EN 1993-1-2 temperature-dependent specific heat `c_a(θ_a)` and the I-section shadow factor `k_sh`, the achieved resistance the time the steel reaches the critical temperature for the degree of utilization `μ0`; the concrete fold reads the EN 1992-1-2 member-type table (column/beam/slab/wall, keyed off the `Object` `Classification.Code`) for the required rating's `(min dimension, min axis distance)` pair and checks both against the section's `LeastDimension` and `AxisDistance` cover, the achieved resistance the worse-governed of the two.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]` gas-curve rows), Rasm.Element (project — `SectionProperties` incl. `AxisDistance`, the seam `ElementGraph.SectionOf` Op-free section accessor, `Node.Object` for the `Classification.Code` member-type read, `MeasureValue`, `Dimension`, `NodeId`), BCL inbox (`Math`, `FrozenDictionary`).
- Growth: a new fire model (a parametric EN 1991-1-2 natural fire, an EN 1995-1-2 timber charring rate, an EN 1993-1-2 PROTECTED-steel march with an insulation `λ_p`/`d_p` term) is one route plus one fold plus one explicit dispatch arm reading the same section (the two-route fire dispatch rails an unrecognized route — `<fire-route-unhandled>` — until its arm lands, never silently charging a timber member against the concrete table); a new exposure is one `FireExposure` row carrying its gas-curve delegate and convection coefficient; the concrete axis-distance (cover-to-rebar) criterion and the member-type-specific (column/beam/slab/wall) tables are now LIVE (the seam `SectionProperties.AxisDistance` cover column + the `Object` `Classification.Code` member type), the 500 °C isotherm method deepening as a fold over the section thermal field where the tabulated check is insufficient.
- Boundary: the section factor `Am/V` reads the seam `SectionProperties.HeatedPerimeter`/`Area` so fire and ambient design share one section source; the section resolves through the seam's Op-free `SectionOf` accessor (the M7-baked read off the member's `ProfileSet`), never re-resolving a `ProfileRef` or admitting VividOrange; the degree of utilization `μ0` is the ambient governing ratio carried on the request, never re-solved here. Steel marches a genuine incremental integration with EN 1993-1-2 temperature-dependent specific heat and exposure-dependent convection, never a tabulated approximation or a fixed heat capacity, the receipt resistance the marched time; the concrete check is the full EN 1992-1-2 tabulated method — the member-type `(min dimension, min axis distance)` pair checked against both the section's `LeastDimension` and `AxisDistance` cover, the achieved resistance the worse-governed so a thin-cover section governs; a member with no `ProfileSet` section rails the typed input fault. Governing ratio is `RequiredMinutes / achieved` threaded through the accumulator so an under-resistant member governs, never a re-parse of the emitted facts; every measured fact is SI-native.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FireExposure {
    public static readonly FireExposure Standard    = new("iso834",      sides: 4, convectionWM2K: 25.0, gasTempC: Iso834);
    public static readonly FireExposure Hydrocarbon = new("hydrocarbon", sides: 4, convectionWM2K: 50.0, gasTempC: HydrocarbonCurve);
    public static readonly FireExposure External    = new("external",    sides: 3, convectionWM2K: 25.0, gasTempC: ExternalCurve);

    public int Sides { get; }
    public double ConvectionWM2K { get; }   // EN 1991-1-2: alpha_c = 25 (standard/external), 50 (hydrocarbon)

    // The nominal gas-temperature-time curve (EN 1991-1-2 §3.2) as a per-row delegate — POLICY_VALUES, never a Key
    // switch: a new exposure is one row carrying its own curve, never a parallel arm in a shared method.
    [UseDelegateFromConstructor]
    public partial double GasTempC(double minutes);

    static double Iso834(double minutes)          => 20.0 + 345.0 * Math.Log10(8.0 * minutes + 1.0);
    static double HydrocarbonCurve(double minutes) => 20.0 + 1080.0 * (1.0 - 0.325 * Math.Exp(-0.167 * minutes) - 0.675 * Math.Exp(-2.5 * minutes));
    static double ExternalCurve(double minutes)    => 20.0 + 660.0 * (1.0 - 0.687 * Math.Exp(-0.32 * minutes) - 0.313 * Math.Exp(-3.8 * minutes));
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct SteelFireState(double Minutes, double SteelTempC);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    const double Emissivity = 0.7;        // EN 1993-1-2 resultant emissivity (eps_m.eps_f.Phi) for carbon steel
    const double Sigma = 5.67e-8;         // Stefan-Boltzmann, W/(m2.K4)
    const double RhoSteel = 7850.0;       // kg/m3
    const double ShadowFactor = 0.9;      // k_sh, I-section nominal correction (EN 1993-1-2 §4.2.5.1)
    const double StepSeconds = 5.0;       // EN 1993-1-2 §4.2.5.1 caps the increment at 5 s
    const double CapMarginMinutes = 30.0; // march beyond the requirement so a surviving member reports a bounded resistance

    // EN 1992-1-2 tabulated method: per member type, the (rating minutes, min dimension, min axis distance) rows (Tables
    // 5.2a/5.4/5.5/5.8). Both the minimum dimension and the axis distance gate the rating; the member type reads off the
    // Object Classification.Code, the axis distance off the seam SectionProperties.AxisDistance cover column.
    static readonly FrozenDictionary<string, (double Minutes, double MinDimM, double AxisDistanceM)[]> ConcreteFireTable =
        new Dictionary<string, (double, double, double)[]> {
            ["IfcColumn"] = [(30.0, 0.200, 0.025), (60.0, 0.250, 0.046), (90.0, 0.350, 0.053), (120.0, 0.350, 0.057), (180.0, 0.450, 0.070), (240.0, 0.450, 0.075)],
            ["IfcBeam"]   = [(30.0, 0.080, 0.025), (60.0, 0.120, 0.040), (90.0, 0.150, 0.055), (120.0, 0.200, 0.065), (180.0, 0.240, 0.080), (240.0, 0.280, 0.090)],
            ["IfcSlab"]   = [(30.0, 0.060, 0.010), (60.0, 0.080, 0.020), (90.0, 0.100, 0.030), (120.0, 0.120, 0.040), (180.0, 0.150, 0.055), (240.0, 0.175, 0.065)],
            ["IfcWall"]   = [(30.0, 0.100, 0.010), (60.0, 0.110, 0.010), (90.0, 0.120, 0.020), (120.0, 0.150, 0.025), (180.0, 0.180, 0.040), (240.0, 0.230, 0.055)],
        }.ToFrozenDictionary();

    // Explicit dispatch over the two fire routes — an unrecognized route rails the typed fault rather than defaulting to
    // the concrete arm, so the EN 1995-1-2 timber-charring growth lands as one added dispatch arm broken loudly, never a
    // timber member silently charged against the concrete table.
    public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Governing: 0.0)),
            (acc, id) => acc.Bind(state => MemberSection(graph, id).Bind(section =>
                request.Route == AssessmentRoute.En1993Fire
                    ? SteelFire(section, request, id, state)
                    : request.Route == AssessmentRoute.En1992Fire
                        ? ConcreteFire(section, MemberClass(graph, id), request, id, state)
                        : Fin.Fail<(Seq<AssessmentFact> Facts, double Governing)>(Missing($"<fire-route-unhandled:{request.Route.Key}>")))))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts, state.Governing,
                new Provenance("BuildingPhysics", request.Route.Standard, "closed-form", clocks.Now)));

    // The member's IFC class (IfcColumn/IfcBeam/IfcSlab/IfcWall) off its Object node Classification.Code — the EN 1992-1-2
    // member-type table selector (the seam Classification.Code is the IFC entity class, not the PredefinedType sub-type);
    // an absent Object or unrecognised class reads the conservative beam table.
    static string MemberClass(ElementGraph graph, NodeId id) => graph.Find<Node.Object>(id).Map(static o => o.Classification.Code).IfNone("");

    // The member section off the seam's Op-free SectionOf accessor (the M7-baked read off the member's ProfileSet, no
    // Bake): a member with no resolved section rails the typed fault. The seam owns the section read, so this runner
    // composes it one-hop rather than re-deriving the match locally, the same read Analysis/structural takes.
    static Fin<SectionProperties> MemberSection(ElementGraph graph, NodeId id) =>
        graph.SectionOf(id).ToFin(Missing($"<fire-member-section-unresolved:{id.Value}>"));

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> SteelFire(SectionProperties section, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) {
        double sectionFactor = section.HeatedPerimeter.Si / Math.Max(section.Area.Si, double.Epsilon);
        double criticalTempC = CriticalTemperature(request.Utilization);
        double cap = request.RequiredMinutes + CapMarginMinutes;
        SteelFireState march = March(request.Exposure, sectionFactor, criticalTempC, cap);
        double achieved = march.SteelTempC >= criticalTempC ? march.Minutes : cap;
        return from minutes in MeasureValue.OfSi(Dimension.DurationDim, achieved * 60.0)
               from critical in MeasureValue.OfSi(QuantityType.OfDimension(TemperatureDim), TemperatureDim, criticalTempC + 273.15, "K")
               from factor in MeasureValue.OfSi(QuantityType.OfDimension(PerLengthDim), PerLengthDim, sectionFactor, "1/m")
               select (Facts: state.Facts
                    .Add(AssessmentFact.Measure($"{id.Value}/fire-resistance-minutes", minutes))
                    .Add(AssessmentFact.Measure($"{id.Value}/critical-temperature", critical))
                    .Add(AssessmentFact.Measure($"{id.Value}/section-factor", factor)),
                Governing: Math.Max(state.Governing, request.RequiredMinutes / Math.Max(achieved, double.Epsilon)));
    }

    // The EN 1993-1-2 §4.2.5.1 unprotected-steel march: a genuine forward integration of the net convective+radiative
    // flux over the section factor with the temperature-dependent specific heat c_a(theta_a) (the 735 C latent-heat
    // spike delays heating) and the exposure's convection coefficient + gas curve; never a tabulated approximation.
    static SteelFireState March(FireExposure exposure, double sectionFactor, double criticalTempC, double capMinutes) {
        double steelTempC = 20.0, minutes = 0.0;
        while (steelTempC < criticalTempC && minutes < capMinutes) {
            double gasTempC = exposure.GasTempC(minutes);
            double netConv = exposure.ConvectionWM2K * (gasTempC - steelTempC);
            double netRad = Emissivity * Sigma * (Pow4(gasTempC + 273.0) - Pow4(steelTempC + 273.0));
            steelTempC += ShadowFactor * sectionFactor / (SpecificHeatSteel(steelTempC) * RhoSteel) * (netConv + netRad) * StepSeconds;
            minutes += StepSeconds / 60.0;
        }
        return new SteelFireState(minutes, steelTempC);
    }

    // EN 1993-1-2 §3.4.1.2 carbon-steel specific heat c_a(theta) [J/(kg.K)] — the piecewise law with the latent-heat
    // singularity at 735 C, never a fixed 600; a derived policy table, not an imperative per-band re-solve.
    static double SpecificHeatSteel(double tempC) =>
        tempC < 600.0 ? 425.0 + 0.773 * tempC - 1.69e-3 * tempC * tempC + 2.22e-6 * tempC * tempC * tempC
        : tempC < 735.0 ? 666.0 + 13002.0 / (738.0 - tempC)
        : tempC < 900.0 ? 545.0 + 17820.0 / (tempC - 731.0)
        : 650.0;

    // EN 1993-1-2 eq 4.22: the critical steel temperature for the degree of utilization mu0, clamped to the standard's
    // validity floor (mu0 >= 0.013) and below unity so the logarithm argument stays positive.
    static double CriticalTemperature(double utilization) {
        double mu0 = Math.Clamp(utilization, 0.013, 0.99);
        return 39.19 * Math.Log(1.0 / (0.9674 * Math.Pow(mu0, 3.833)) - 1.0) + 482.0;
    }

    static double Pow4(double x) { double s = x * x; return s * s; }

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> ConcreteFire(SectionProperties section, string memberClass, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) {
        (double MinDimM, double AxisDistanceM) limits = ConcreteFireLimits(memberClass, request.RequiredMinutes);
        double leastM = section.LeastDimension.Si, axisM = section.AxisDistance.Si;
        // The full EN 1992-1-2 tabulated method: BOTH the minimum cross-section dimension AND the axis distance
        // (cover-to-reinforcement) must meet the required-rating row, so the achieved resistance is the WORSE-governed of
        // the two — an otherwise-adequate section with thin cover still governs, the GOVERNING criterion the prior
        // minimum-dimension-only check could not reach.
        double dimAchieved  = leastM >= limits.MinDimM ? request.RequiredMinutes : request.RequiredMinutes * leastM / Math.Max(limits.MinDimM, double.Epsilon);
        double axisAchieved = axisM >= limits.AxisDistanceM ? request.RequiredMinutes : request.RequiredMinutes * axisM / Math.Max(limits.AxisDistanceM, double.Epsilon);
        double achieved = Math.Min(dimAchieved, axisAchieved);
        return from minutes in MeasureValue.OfSi(Dimension.DurationDim, achieved * 60.0)
               from requiredDim in MeasureValue.OfSi(Dimension.LengthDim, limits.MinDimM)
               from least in MeasureValue.OfSi(Dimension.LengthDim, leastM)
               from requiredAxis in MeasureValue.OfSi(Dimension.LengthDim, limits.AxisDistanceM)
               from axis in MeasureValue.OfSi(Dimension.LengthDim, axisM)
               select (Facts: state.Facts
                    .Add(AssessmentFact.Measure($"{id.Value}/fire-resistance-minutes", minutes))
                    .Add(AssessmentFact.Measure($"{id.Value}/required-min-dimension", requiredDim))
                    .Add(AssessmentFact.Measure($"{id.Value}/least-dimension", least))
                    .Add(AssessmentFact.Measure($"{id.Value}/required-axis-distance", requiredAxis))
                    .Add(AssessmentFact.Measure($"{id.Value}/axis-distance", axis)),
                Governing: Math.Max(state.Governing, request.RequiredMinutes / Math.Max(achieved, double.Epsilon)));
    }

    // The EN 1992-1-2 (min cross-section dimension, min axis distance) pair for the required rating and member type: the
    // first band whose threshold covers the requirement, else the top band; an unrecognised class reads the conservative
    // beam table — a member-type-keyed frozen-table lookup, never a ternary cascade.
    static (double MinDimM, double AxisDistanceM) ConcreteFireLimits(string memberClass, double requiredMinutes) {
        (double Minutes, double MinDimM, double AxisDistanceM)[] table = ConcreteFireTable.GetValueOrDefault(memberClass, ConcreteFireTable["IfcBeam"]);
        var row = table.FirstOrDefault(r => r.Minutes >= requiredMinutes, table[^1]);
        return (row.MinDimM, row.AxisDistanceM);
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
