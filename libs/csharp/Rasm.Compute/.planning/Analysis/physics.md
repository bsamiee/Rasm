# [COMPUTE_PHYSICS]

Rasm.Compute closed-form building-physics runner collapses the `Discipline.Thermal`/`Discipline.Acoustic`/`Discipline.Fire` arms of the `Analysis/assessment` rail onto one `BuildingPhysics` kernel — all three are closed-form ISO/EN folds over an assembly or section read directly from the concrete `Rasm.Element` `ElementGraph`, no external solver and no subprocess. All three now dispatch through ONE `RouteTable`: a route resolves its `TargetFold` before the first target is read, so the thermal runner no longer hand-writes the fold its two siblings share.

Thermal reads the multi-ply `UValueWM2K` from `AssemblyAggregator.Aggregate` (the one ISO 6946 series-resistance owner) and runs the EN ISO 13788 Glaser interstitial-condensation profile over the SAME receipt's total vapour resistance, computing the condensation mass rate through the genuine lower-convex-hull tangent construction rather than a bare boolean; a window assembly instead composes `AssemblyAggregator.AggregateWindow` for the EN ISO 10077-1 area-and-perimeter-weighted whole-window `Uw`. Acoustic reads the layered mass-law `StcWeighted` (ASTM E413) from the aggregator for a `LayerSet` and the intrinsic seam `Nrc`/`Rw` (ISO 717-1) for a single material, or folds the EN 12354-6 Sabine/Eyring reverberation time over each space's bounding surfaces. Fire folds the EN 1993-1-2 unprotected-steel critical-temperature march and the EN 1992-1-2 concrete tabulated minimum-dimension check.

Each runner returns one `AssessmentResult` fact stream, the governing ratio threaded through the fold accumulator so the verdict derives from the in-scope governing quantity, never a re-parse of the emitted facts. Every measured fact is SI-native through the seam `MeasureValue.OfSi`, and an unreachable quantity is an `Option`, never a measured zero.

## [01]-[INDEX]

- [02]-[THERMAL_ENVELOPE]: `RunThermal` folds the ISO 6946 series-`U` with its EN ISO 13788 Glaser profile for a building envelope, and the EN ISO 10077-1 `Uw` for a window.
- [03]-[ACOUSTIC_RATING]: `RunAcoustic` dispatches its route to the layered transmission index or the EN 12354-6 room-reverberation fold.
- [04]-[FIRE_RESISTANCE]: `RunFire` dispatches its route to the EN 1993-1-2 steel critical-temperature march or the EN 1992-1-2 concrete tabulated check.

## [02]-[THERMAL_ENVELOPE]

- Owner: `TargetFold` the per-target accumulator shape every discipline fold on this page has and `RouteTable` the route→fold dispatch owner ALL THREE runners compose; `BuildingPhysics.RunThermal` the thermal runner; `BoundaryClimate` the interior/exterior temperature-and-humidity boundary carried on the request; `GlaserProfile` the per-interface temperature/saturation/actual-vapour-pressure receipt; `CondensationPlane` the located interstitial plane; `GlaserResult` the condensation summary whose plane is an `Option`; `WindowTarget` the resolved window (its glazed/frame `WindowPart` set and the aggregator inputs), `WindowRole.Of` the discriminant resolving it.
- Entry: `public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, IClock clock)` resolves its arm off the `ThermalRoutes` table exactly as the acoustic and fire runners do; the `iso6946` fold discriminates window from envelope on `WindowRole.Of`, a resolved `WindowTarget` composing `AggregateWindow` for whole-window `Uw` and emitting `whole-window-u`/`glazed-u`/`edge-bridge`/`glazed-fraction` with `frame-u` where the receipt carries one, and a non-window target running the series-`U` + Glaser envelope path. `Fin<T>` aborts onto `ComputeFault.AssessmentInputMissing` carrying its `AssessmentInputReason` row when a required layer, glazing, or frame property is absent.
- Auto: the per-interface temperature, Magnus saturation pressure, and actual-vapour line fold over the CUMULATIVE thermal and vapour resistances the ply walk builds, while the two TOTALS both profiles divide by are read off the `AssemblyProperty` receipt the same fold already produced — so a change to the ISO 6946 film convention or the EN ISO 13788 `Sd` definition moves at one owner and cannot desync the profile from the U-value beside it; the EN ISO 13788 construction derives condensation planes and `g_c` from the lower convex hull. Window resolution discriminates `Compose` parts through the seam composition's own total `Switch`: a `LayerSet`/`Single` part supplies `WindowPart.Glazed` and a `ProfileSet` part supplies `WindowPart.Frame` only where residual frame area is positive.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialPropertySet.Thermal` via `MaterialPropertyAccess`, `MaterialId`, `MeasureValue`, `Dimension`, `QuantityType`, `UnitProvenance`, `NodeId`, the `Node.Object`/`Node.QuantitySet`/`Node.PropertySet` cases + `Relationship.Compose`/`ComposeKind`, `PropertyValue`, and the `QuantityRows`/`EnvelopeRows` row vocabularies), the `Analysis/assessment` `AnalysisReads` bag-read owner, the `Analysis/aggregator` `AssemblyAggregator` (`Aggregate`, `AggregateWindow`, `AssemblyProperty`, `WindowPart`, `WindowU`), CommunityToolkit.HighPerformance (`SpanOwner<T>` — the hull kernel's leases), BCL inbox (`Math`).
- Growth: a new thermal check (a dynamic decrement/admittance pair) is one fold over the same `LayerSet` reading the aggregator's `ArealHeatCapacityKJM2K`, never a parallel building-envelope owner; the moisture model deepens to the EN 15026 transient form as one fold swap reading the same layer resistances; a 12-month annual condensation/evaporation balance is one fold over a climate series once `BoundaryClimate` carries one; a richer EN ISO 10077-2 numerical frame model is one deeper `WindowPart` resolution the runner supplies, never a parallel window owner.
- Boundary: multi-ply `U` and the total vapour resistance BOTH compose `AssemblyAggregator.Aggregate` — the runner no longer re-sums either total, and the Glaser fold reads each ply's `Thermal.Conductivity.Si` and `VapourResistanceFactor` ONLY for the per-interface CUMULATIVE chain. Named loss: none — the per-ply walk survives because a per-interface profile carries an interface ordinal a scalar total structurally cannot, and the two facts a scalar CAN carry now come from the owner that computes them. Witness: `AssemblyProperty` publishes `UValueWM2K` and `VapourResistanceSdM` as `Option` columns, so an unreachable total refuses here rather than being silently re-derived. Whole-window `Uw` composes `AssemblyAggregator.AggregateWindow`; a frameless assembly's absent frame sub-transmittance is an `Option` on the receipt and emits no `frame-u` fact. Spacer `Ψg` reads the window `Pset` through `EnvelopeRows` and every area reads `Qto_*BaseQuantities` through `QuantityRows`, both keyed on `Rasm.Element`-declared statics; the bag reads compose the one `AnalysisReads` owner, never a physics-local copy. Missing acceptance targets propagate `double.NaN` as `NotApplicable`, never a `0.0` satisfied ratio.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// One target absorbed into the running (facts, governing) accumulator — the shape EVERY discipline fold on this
// page has, named so a route table can hold it. The accumulator is threaded rather than returned whole because a
// multi-target run governs on the worst target, not the last.
public delegate Fin<(Seq<AssessmentFact> Facts, double Governing)> TargetFold<TRequest>(
    ElementGraph graph, TRequest request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state);

// --- [MODELS] ------------------------------------------------------------------------------
// Interior/exterior boundary travels on AssessmentRequest.Thermal; its five fields the seam CanonicalBytes folds.
// The vapour partial pressures derive from the Magnus saturation curve and the boundary relative humidities, so the
// climate carries no redundant pressure field.
public readonly record struct BoundaryClimate(double InteriorTempC, double InteriorRh, double ExteriorTempC, double ExteriorRh, double TargetUValueWM2K) {
    // The EN ISO 13788 winter design condition, published so a caller composing a thermal request states the
    // standard's own boundary rather than four literals; the request carries the value, never this type.
    public static readonly BoundaryClimate WinterDesign = new(20.0, 0.50, -5.0, 0.85, TargetUValueWM2K: 0.30);

    public double InteriorVapourPa => SaturationPa(InteriorTempC) * InteriorRh;
    public double ExteriorVapourPa => SaturationPa(ExteriorTempC) * ExteriorRh;
    public static double SaturationPa(double tC) => 610.5 * Math.Exp(17.269 * tC / (237.3 + tC));
}

// Per-interface Glaser receipt: the 1-based interface ordinal, the steady-state temperature, the saturation
// pressure, the unredistributed straight actual vapour pressure, and the cumulative vapour resistance Z. The
// temperature column is EMITTED as the EN ISO 13788 interface profile a `List` fact carries, so the receipt an
// engineer reads and the receipt the fold computes are one shape.
public readonly record struct GlaserProfile(int Interface, double TempC, double SaturationPa, double ActualPa, double VapourResistanceCum) {
    public double Utilization => SaturationPa > 0.0 ? ActualPa / SaturationPa : 0.0;
}

// The located interstitial plane. Presence IS the condensation, so the retired `bool Condensing` beside a `-1`
// index and a `"none"` magic string collapse into one carrier no consumer can read past.
public readonly record struct CondensationPlane(int Interface, string LayerName) {
    public string Label => $"interface-{Interface}:{LayerName}";
}

// Condensation summary derives from the Glaser fold: the worst-interface vapour-pressure utilization (the governing
// condensation ratio), the EN ISO 13788 condensation MASS rate, the per-interface temperature profile, and the
// located plane where one formed.
public readonly record struct GlaserResult(
    double VapourUtilization, double CondensationRateKgM2S, Seq<GlaserProfile> Profile, Option<CondensationPlane> Plane) {
    // A single-layer assembly has NO internal interface to condense at, which is not the same fact as an assembly
    // checked and found clean — it is the honest empty profile with no plane and no rate.
    public static readonly GlaserResult NoInternalInterface = new(0.0, 0.0, Seq<GlaserProfile>(), None);
}

// A resolved window: the glazed/frame parts AggregateWindow folds and the target's own acceptance ratio inputs.
// WindowRole.Of returning an Option is the discriminant the retired early return spelled as control flow.
public readonly record struct WindowTarget(Seq<WindowPart> Parts);

// --- [OPERATIONS] --------------------------------------------------------------------------
// Route dispatch is a TABLE, not a ternary ladder — the Analysis/capacity (DesignCode, LimitState) table is the
// precedent: a route maps to the fold that serves it, growth is a ROW, and the unhandled arm is the table's own
// miss. The arm resolves ONCE per run, before the first target is read, so an unrouted discipline rails without
// doing per-target work it will discard. The table keys on the AssessmentRoute ROW rather than its Key string,
// because the row already carries [KeyMemberEqualityComparer] and a string key re-opens a closed vocabulary.
public static class RouteTable {
    public static FrozenDictionary<AssessmentRoute, TargetFold<TRequest>> Of<TRequest>(params ReadOnlySpan<(AssessmentRoute Route, TargetFold<TRequest> Fold)> rows) =>
        Seq(rows).ToFrozenDictionary(static row => row.Route, static row => row.Fold);

    public static Fin<AssessmentResult> Run<TRequest>(
        FrozenDictionary<AssessmentRoute, TargetFold<TRequest>> routes, ElementGraph graph, TRequest request,
        AssessmentRoute route, Seq<NodeId> targets, Op key, IClock clock) =>
        (routes.TryGetValue(route, out TargetFold<TRequest> arm)
            ? Fin.Succ(arm)
            : Fin.Fail<TargetFold<TRequest>>(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.RouteUnrouted, route.Key)))
        .Bind(fold => targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Governing: 0.0)),
            (acc, id) => acc.Bind(state => fold(graph, request, id, state))))
        .Bind(state => AssessmentResult.Of(route, state.Facts, Some(state.Governing), clock.GetCurrentInstant(), key));
}

public static partial class BuildingPhysics {
    const double RsiWM2K = 0.13;                  // ISO 6946 interior surface film
    const double RseWM2K = 0.04;                  // ISO 6946 exterior surface film
    const double VapourPermeabilityAir = 2.0e-10; // delta0, kg/(m.s.Pa)

    // SI dimensions composed from the seam Dimension algebra where no named row exists — the kernel-true
    // discriminator a downstream unit canonicalization re-reads. None of the three is on the kernel symbol roster, so
    // each fact mints through OfSi(QuantityType.OfDimension(dim), dim, si, Some(UnitProvenance.Label(token))) under
    // the finite gate: Derive would resolve NO canonical unit for them, where a rostered dimension (TemperatureDim,
    // LengthDim, DurationDim, ThermalTransmittanceDim) already carries its symbol and takes the anonymous one-hop.
    static readonly Dimension PerLengthDim = Dimension.Dimensionless.Divide(Dimension.LengthDim);
    static readonly Dimension VapourFluxDim = Dimension.MassDim.Divide(Dimension.AreaDim).Divide(Dimension.DurationDim);
    // EN ISO 10077-1 edge-seal bridge sum(lg*Psi_g) is a thermal conductance W/K — the area-U transmittance times
    // area — the perimeter linear-bridge term the whole-window fact carries beside the area-U facts.
    static readonly Dimension EdgeBridgeDim = Dimension.ThermalTransmittanceDim.Multiply(Dimension.AreaDim);

    static readonly Op ThermalKey = Op.Of(name: nameof(RunThermal));

    // BOTH thermal routes serve the one fold, because the fold emits the ISO 6946 transmittance and the EN ISO
    // 13788 condensation facts together off one aggregation. Two rows to one fold is what makes `en13788`
    // REACHABLE: the retired hand fold ignored the route entirely, so a request naming the condensation standard
    // ran and reported under a route nothing dispatched on.
    static readonly FrozenDictionary<AssessmentRoute, TargetFold<AssessmentRequest.Thermal>> ThermalRoutes =
        RouteTable.Of<AssessmentRequest.Thermal>((AssessmentRoute.Iso6946, Thermal), (AssessmentRoute.En13788, Thermal));

    public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, IClock clock) =>
        RouteTable.Run(ThermalRoutes, graph, request, request.Route, request.Targets, ThermalKey, clock);

    // The window and the envelope are two arms of ONE target fold, discriminated on the resolved WindowTarget: a
    // target with no GlazingArea quantity is not a window and its absence is the discriminant, never a fall-through.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Thermal(ElementGraph graph, AssessmentRequest.Thermal request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        WindowRole.Of(graph, id).Bind(target => target.Match(
            Some: window => Window(window, request, id, state),
            None: () => Envelope(graph, request, id, state)));

    // Seam-keyed resolver maps each ply MaterialId to its material node's property set, railing the missing-input
    // fault on an absent material node so a fold reads the composition's OWN plies by native key, never a graph
    // NodeId lookup.
    static Func<MaterialId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) =>
        mid => graph.Material(mid).Map(static m => m.Properties).ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, mid.Value));

    static ComputeFault Missing(AssessmentInputReason reason, string witness) =>
        new ComputeFault.AssessmentInputMissing(reason, witness);

    // EN ISO 10077-1 whole-window branch folds resolved glazed/frame parts through AggregateWindow into the area-and-
    // perimeter-weighted Uw, threading Uw/U_target (a LOWER Uw is better, so achieved-over-target like the envelope
    // U). Every sub-transmittance on the receipt is an Option, so the glazed and frame facts are emitted where the
    // receipt carries them and are absent where it does not — the retired `double.IsFinite` probe re-derived a
    // presence the carrier now states.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Window(WindowTarget target, AssessmentRequest.Thermal request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from w in AssemblyAggregator.AggregateWindow(target.Parts)
        from uw in MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, w.UwWM2K)
        from glazed in Transmittance(id, "glazed-u", w.UgWM2K)
        from frame in Transmittance(id, "frame-u", w.UfWM2K)
        from edge in MeasureValue.OfSi(QuantityType.OfDimension(EdgeBridgeDim), EdgeBridgeDim, w.EdgeBridgeW_K, Some(UnitProvenance.Label("W/K")))
        from fraction in AssessmentFact.Ratio($"{id.Value}/glazed-fraction", w.GlazedFraction)
        let uRatio = request.Climate.TargetUValueWM2K > 0.0 ? w.UwWM2K / request.Climate.TargetUValueWM2K : double.NaN
        select (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/whole-window-u", uw))
                .Add(AssessmentFact.Measure($"{id.Value}/edge-bridge", edge))
                .Add(fraction) + glazed + frame,
            Governing: Math.Max(state.Governing, uRatio));

    static Fin<Seq<AssessmentFact>> Transmittance(NodeId id, string name, Option<double> value) =>
        value.Traverse(si => MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, si)
            .Map(measure => AssessmentFact.Measure($"{id.Value}/{name}", measure))).As().Map(static fact => fact.ToSeq());

    // Through-thickness envelope branch reads the ISO 6946 series U and the EN ISO 13788 total vapour resistance off
    // ONE AssemblyProperty receipt, then folds the Glaser condensation profile over the ply chain.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Envelope(ElementGraph graph, AssessmentRequest.Thermal request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from composition in graph.CompositionOf(id).ToFin(Missing(AssessmentInputReason.CompositionShape, id.Value))
        from folded in Series(composition, Resolver(graph), request.Climate)
        let u = folded.U
        let glaser = folded.Glaser
        from uMeasure in MeasureValue.OfSi(Dimension.ThermalTransmittanceDim, u)
        from vapour in AssessmentFact.Ratio($"{id.Value}/vapour-utilization", glaser.VapourUtilization)
        from rate in MeasureValue.OfSi(QuantityType.OfDimension(VapourFluxDim), VapourFluxDim, glaser.CondensationRateKgM2S, Some(UnitProvenance.Label("kg/(m2.s)")))
        from profile in glaser.Profile.TraverseM(step =>
            MeasureValue.OfSi(Dimension.TemperatureDim, step.TempC + 273.15)
                .Map(static value => (PropertyValue)new PropertyValue.Measure(value))).As()
        let uRatio = request.Climate.TargetUValueWM2K > 0.0 ? u / request.Climate.TargetUValueWM2K : double.NaN
        select (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/u-value", uMeasure))
                .Add(vapour)
                .Add(AssessmentFact.Measure($"{id.Value}/condensation-rate", rate))
                .Add(AssessmentFact.List($"{id.Value}/interface-temperatures", profile))
                + glaser.Plane.Map(plane => AssessmentFact.Text($"{id.Value}/condensation-plane", plane.Label)).ToSeq(),
            Governing: Math.Max(state.Governing, Math.Max(uRatio, glaser.VapourUtilization)));

    // The series owner routes a LayerSet through ONE Aggregate call that yields BOTH the transmittance and the
    // total vapour resistance the Glaser fold divides by; a Single carries an intrinsic transmittance and has no
    // internal interface to condense at; a ProfileSet/ConstituentSet has no through-thickness envelope at all. The
    // generated total Switch breaks at compile time if the seam adds a composition case, and a Single never
    // fabricates the receipt columns an aggregation would have produced.
    static Fin<(double U, GlaserResult Glaser)> Series(MaterialComposition composition, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, BoundaryClimate climate) =>
        composition.Switch(
            layerSet: set => AssemblyAggregator.Aggregate(set, resolve).Bind(receipt =>
                from u in receipt.UValueWM2K.ToFin(Missing(AssessmentInputReason.MeasureAbsent, nameof(AssemblyProperty.UValueWM2K)))
                from sd in receipt.VapourResistanceSdM.ToFin(Missing(AssessmentInputReason.MeasureAbsent, nameof(AssemblyProperty.VapourResistanceSdM)))
                from glaser in GlaserOf(set, u, sd, resolve, climate)
                select (U: u, Glaser: glaser)),
            single: s => resolve(s.Material).Bind(props => props.Thermal
                .Bind(static t => t.UValue)
                .Map(static u => (U: u.Si, Glaser: GlaserResult.NoInternalInterface))
                .ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, s.Material.Value))),
            profileSet: static s => Fin.Fail<(double, GlaserResult)>(Missing(AssessmentInputReason.CompositionShape, s.Material.Value)),
            constituentSet: static s => Fin.Fail<(double, GlaserResult)>(Missing(AssessmentInputReason.CompositionShape, s.PrimaryMaterial.Value)));

    // --- [WINDOW_ASSEMBLY] ---------------------------------------------------------------
    // EN ISO 10077-1 window-part resolver reads Compose parts and baked Qto/Pset bags Op-free. GlazingArea is the
    // seam-direct discriminant: a target carrying none is NOT a window and resolves None, which is the fact the
    // envelope arm needs. Every other read is either a real refusal (an absent total area cannot yield a frame
    // fraction) or a STATED policy default (an absent perimeter or spacer Psi means no edge bridge — a measured
    // zero, because an unstamped edge seal contributes no linear conductance).
    public static class WindowRole {
        public static Fin<Option<WindowTarget>> Of(ElementGraph graph, NodeId window) =>
            graph.Magnitude(window, QuantityRows.GlazingArea).Match(
                None: static () => Fin.Succ(Option<WindowTarget>.None),
                Some: glazedArea => Parts(graph, window, glazedArea).Map(Some));

        static Fin<WindowTarget> Parts(ElementGraph graph, NodeId window, double glazedArea) {
            double totalArea = graph.Magnitude(window, QuantityRows.Area).IfNone(glazedArea);
            double frameArea = Math.Max(totalArea - glazedArea, 0.0);
            double edgeLength = graph.Magnitude(window, QuantityRows.GlazingPerimeter).IfNone(0.0);
            double psi = graph.Scalar(window, EnvelopeRows.GlazingEdgePsi).IfNone(0.0);
            Seq<NodeId> parts = Members(graph, window);
            return Find(graph, parts, static composition => composition is MaterialComposition.LayerSet or MaterialComposition.Single, "glazing", window)
                .Bind(glazing => PartU(graph, glazing, "glazing").Bind(ug =>
                    frameArea <= 0.0
                        ? Fin.Succ(new WindowTarget(Seq<WindowPart>(new WindowPart.Glazed(ug, glazedArea, edgeLength, psi))))
                        : Find(graph, parts, static composition => composition is MaterialComposition.ProfileSet, "frame", window)
                            .Bind(frame => PartU(graph, frame, "frame").Map(uf => new WindowTarget(Seq<WindowPart>(
                                new WindowPart.Glazed(ug, glazedArea, edgeLength, psi), new WindowPart.Frame(uf, frameArea)))))));
        }

        static Fin<NodeId> Find(ElementGraph graph, Seq<NodeId> parts, Func<MaterialComposition, bool> shape, string role, NodeId window) =>
            parts.Find(part => graph.CompositionOf(part).Exists(shape))
                .ToFin(Missing(AssessmentInputReason.WindowFieldAbsent, $"{role}:{window.Value}"));

        // The role slug is the CALLER's already-resolved discriminant, not a knob: it names which part the window
        // resolver was looking for, which is the only thing a reader of the refusal needs.
        static Fin<double> PartU(ElementGraph graph, NodeId part, string role) =>
            graph.PropertiesOf(part).Thermal.Bind(static thermal => thermal.UValue).Map(static u => u.Si)
                .ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, $"{role}:{part.Value}"));

        static Seq<NodeId> Members(ElementGraph graph, NodeId window) =>
            toSeq(graph.EdgesAt(window))
                .Choose(e => e is Relationship.Compose c && c.Whole == window && (c.SubKind == ComposeKind.Aggregate || c.SubKind == ComposeKind.Nest) ? Some(c.Part) : None)
                .Distinct();
    }

    // --- [GLASER_TANGENT] ----------------------------------------------------------------
    // EN ISO 13788 steady-state interstitial-condensation fold walks the LayerSet plies for the per-interface
    // CUMULATIVE resistances a scalar total cannot carry, while the two TOTALS the profile divides by come off the
    // AssemblyProperty receipt the same aggregation already produced — one owner for the ISO 6946 series R and the
    // EN ISO 13788 Sd, so a film-convention or Sd-definition change cannot move under this fold.
    static Fin<GlaserResult> GlaserOf(MaterialComposition.LayerSet set, double uValueWM2K, double vapourSdM, Func<MaterialId, Fin<Seq<MaterialPropertySet>>> resolve, BoundaryClimate climate) =>
        from steps in set.Layers.TraverseM(layer => resolve(layer.Material)
            .Bind(props => props.Thermal.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, layer.Material.Value)))
            .Map(thermal => new LayerResistance(
                R: layer.Thickness.Si / Math.Max(thermal.Conductivity.Si, EpsilonPolicy.SeamUlp),
                Z: thermal.VapourResistanceFactor * layer.Thickness.Si / VapourPermeabilityAir,
                Name: layer.LayerName))).As()
        select Condensation(steps, rTot: 1.0 / uValueWM2K, zTot: vapourSdM / VapourPermeabilityAir, climate);

    // Local fold state carries one ply's thermal and vapour resistance plus its condensation-plane label.
    readonly record struct LayerResistance(double R, double Z, string Name);

    static GlaserResult Condensation(Seq<LayerResistance> steps, double rTot, double zTot, BoundaryClimate climate) {
        int n = steps.Count;
        if (n < 2 || zTot <= 0.0) { return GlaserResult.NoInternalInterface; }
        double pInt = climate.InteriorVapourPa, pExt = climate.ExteriorVapourPa;
        double dTemp = climate.InteriorTempC - climate.ExteriorTempC;
        Seq<GlaserProfile> profile = toSeq(Enumerable.Range(1, n - 1)).Fold(
            (Built: Seq<GlaserProfile>(), RCum: RsiWM2K, ZCum: 0.0),
            (acc, k) => {
                double rCum = acc.RCum + steps[k - 1].R, zCum = acc.ZCum + steps[k - 1].Z;
                double tempC = climate.InteriorTempC - (rCum / rTot * dTemp);
                double actual = pInt - (zCum / zTot * (pInt - pExt));
                return (acc.Built.Add(new GlaserProfile(k, tempC, BoundaryClimate.SaturationPa(tempC), actual, zCum)), rCum, zCum);
            }).Built;
        return toSeq(profile.OrderByDescending(static g => g.Utilization)).Head.Match(
            Some: worst => {
                double rate = CondensationRate(profile, zTot, pInt, pExt);
                // A rate above the seam band OR a saturated interface locates a plane; below both, the assembly is
                // checked and clean and the plane is genuinely absent.
                return new GlaserResult(worst.Utilization, rate, profile,
                    rate > EpsilonPolicy.SeamUlp || worst.Utilization >= 1.0
                        ? Some(new CondensationPlane(worst.Interface, steps[worst.Interface - 1].Name))
                        : None);
            },
            None: () => GlaserResult.NoInternalInterface);
    }

    // EN ISO 13788 condensation rate uses the lower-convex-hull construction: the realized vapour line is the lower
    // hull of the (Z, p) points, its interior vertices the condensation planes, and g_c the inflow-minus-outflow
    // flux discontinuity summed over every plane. Exemption: the array hull kernel is the measured statement seam —
    // the buffers are leased, the loops stay.
    static double CondensationRate(Seq<GlaserProfile> profile, double zTot, double pInt, double pExt) {
        int m = profile.Count;
        using SpanOwner<double> owner = SpanOwner<double>.Allocate((m + 2) * 2);
        Span2D<double> points = owner.Span.AsSpan2D(m + 2, 2);
        points[0, 0] = 0.0; points[0, 1] = pInt;
        for (int i = 0; i < m; i++) { points[i + 1, 0] = profile[i].VapourResistanceCum; points[i + 1, 1] = profile[i].SaturationPa; }
        points[m + 1, 0] = zTot; points[m + 1, 1] = pExt;
        using SpanOwner<int> hullOwner = SpanOwner<int>.Allocate(m + 2);
        Span<int> hull = LowerHull(points, hullOwner.Span);
        double gc = 0.0;
        for (int i = 1; i < hull.Length - 1; i++) {
            int u = hull[i - 1], v = hull[i], w = hull[i + 1];
            gc += ((points[u, 1] - points[v, 1]) / (points[v, 0] - points[u, 0]))
                - ((points[v, 1] - points[w, 1]) / (points[w, 0] - points[v, 0]));
        }
        return gc;
    }

    // Andrew's monotone-chain LOWER hull over points pre-sorted by Z (the cumulative vapour resistance is
    // monotone): pop while the last triple does not turn counter-clockwise, so a saturation point dipping below the
    // straight interior-to-exterior line enters the hull as a condensation plane and a point above it is excluded.
    // The result is a SLICE of the caller's lease — the retired `hull[..h]` minted a second array to trim.
    static Span<int> LowerHull(ReadOnlySpan2D<double> points, Span<int> hull) {
        int count = points.Height, h = 0;
        for (int i = 0; i < count; i++) {
            while (h >= 2 && Cross(points, hull[h - 2], hull[h - 1], i) <= 0.0) { h--; }
            hull[h++] = i;
        }
        return hull[..h];
    }

    static double Cross(ReadOnlySpan2D<double> p, int o, int a, int b) =>
        ((p[a, 0] - p[o, 0]) * (p[b, 1] - p[o, 1])) - ((p[a, 1] - p[o, 1]) * (p[b, 0] - p[o, 0]));
}
```

## [03]-[ACOUSTIC_RATING]

- Owner: `BuildingPhysics.RunAcoustic` the acoustic runner dispatching the acoustic route off the shared `RouteTable` — `Transmission` the weighted single-number sound-reduction projection over the aggregator's layered mass-law `StcWeighted` (ASTM E413) and the single-material seam `Nrc`/`Rw` (ISO 717-1); `Room` the EN 12354-6 / ISO 3382-1 Sabine/Eyring reverberation fold over each space's bounding surfaces and the seam eighteen-band `AbsorptionSpectrum`.
- Entry: `public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, IClock clock)` resolves its arm off the `AcousticRoutes` table — `iso12354` to `Transmission`, `iso3382` to `Room` — before the first target is read. `Transmission` reads the layered mass-law single number from `Aggregate` for a `LayerSet` and the intrinsic seam `Nrc`/`Rw` off the `PrimaryMaterial` for a single material, and threads `request.RequiredRw / Rw` when the request carries a `RequiredRw`. Cross-standard asymmetry is deliberate: the single material has a measured spectrum yielding a true ISO 717-1 `Rw`, the layered assembly only a mass-law estimate whose ASTM E413 `StcWeighted` is all its areal-mass data admits — so the ISO demand judges the single-material path against `Rw` and the assembly against its `StcWeighted`, never an ISO demand against an ASTM rating on one path. `Room` folds per space target the per-band absorption area `A(b) = Σ S_i·α_i(b)` over the bounding surfaces (the `Analysis/energy` `BoundaryReads.SurfacesOf` owner), the Sabine and Eyring mid-frequency reverberation times, and threads `T_mf / TargetReverberationS`.
- Auto: the assembly weighted index is the aggregator's `StcWeighted`, already an `Option<int>` on the receipt, so a mass-less fold that reached no contour fit arrives here as an absence rather than a fabricated rating; the single-material `Nrc`/`Rw` read the seam `Acoustic` projections off the `PrimaryMaterial`; both contour rows share one `RatingContour` owner; the room fold reads its volume and per-surface areas off the baked `Qto` evidence and each surface's absorption off `props.Acoustic.At(band)`.
- Packages: LanguageExt.Core, Rasm.Element (project — `MaterialComposition`, `MaterialPropertySet.Acoustic` via `MaterialPropertyAccess`, `AcousticBand`, `MeasureValue`, `Dimension`, `QuantityType`, `UnitProvenance`, `NodeId`), the `Analysis/aggregator` `AssemblyAggregator`, the `Analysis/energy` `BoundaryReads`, Rasm (kernel — `EpsilonPolicy.SeamUlp`), the `Analysis/frame` `DivisorBand` guarded quotient, BCL inbox.
- Growth: a new acoustic route is one `AcousticRoutes` row over one `TargetFold`; the airborne spectrum-adaptation deepening (flanking `Dn,f,w`, the ISO 717-1 `C`/`Ctr` terms) is one fold over the same per-band SRI; the impact `Ln,w` (ISO 717-2 / IIC) is the descending sibling that lands once this runner carries the assembly normalized-impact spectrum — the EN 12354-2 floating-floor `ΔL_w` computed from the resilient layer's seam `Acoustic.DynamicStiffnessMNPerM3` and rated through the LANDED `Composition/acoustic#ACOUSTIC_FOLDS` `RatingContour.Iic`/`Lnw` rows via the shared `RatingContour.Fit`, so only the runner-side spectrum fold remains open; a per-band reverberation `List` fact is one projection over the same `Room` band fold.
- Boundary: the multi-ply index composes `Aggregate` so the layered sound reduction is the seam-owned `RatingContour.Stc.Fit` ASTM E413 mass-law estimate, never a second STC/`Rw` algorithm; the single-material `Nrc`/`Rw` are the seam intrinsic folds off the `Acoustic` case, never recomputed; the single-material branch resolves through the composition's `PrimaryMaterial`, never the element `NodeId`, and BOTH absent-rating paths report the `absent` text fact against a `NaN` governing rather than a fabricated rating. Every `RequiredRw` target yields `RequiredRw / Rw` (a higher Rw is better) and a genuine pass/fail, while a `RequiredRw <= 0` request reverts to the informational rating (governing `double.NaN`, propagated by `Math.Max` across the fold, banding `NotApplicable`). `Room` reads the space-surface incidence through the ONE `BoundaryReads.SurfacesOf` owner; a surface material with no `Acoustic` case, an absent volume/area quantity, or an empty bounding-surface set rails typed. Reverberation governs `T_mf / TargetReverberationS` — an OVER-reverberant room exceeds — and the Eyring time is an `Option`: a fully absorptive room (mean α at or above unity) has NO Eyring correction to report, and the retired `: 0.0` published that unreportable case as a measured instantaneous decay.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    const double SabineConstant = 0.161;   // s/m — the Sabine/Eyring reverberation coefficient at 20 degC

    static readonly Op AcousticKey = Op.Of(name: nameof(RunAcoustic));

    static readonly FrozenDictionary<AssessmentRoute, TargetFold<AssessmentRequest.Acoustic>> AcousticRoutes = RouteTable.Of<AssessmentRequest.Acoustic>(
        (AssessmentRoute.Iso12354, Transmission), (AssessmentRoute.Iso3382, Room));

    public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, IClock clock) =>
        RouteTable.Run(AcousticRoutes, graph, request, request.Route, request.Targets, AcousticKey, clock);

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Transmission(ElementGraph graph, AssessmentRequest.Acoustic request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        graph.CompositionOf(id).ToFin(Missing(AssessmentInputReason.CompositionShape, id.Value))
            .Bind(composition => composition.Switch(
                // The assembly carrier models no Rw: its single number is the ASTM E413 mass-law estimate, already
                // an Option on the receipt, so a mass-less fold arrives as an absence.
                layerSet: set => AssemblyAggregator.Aggregate(set, Resolver(graph))
                    .Bind(property => RateAcoustic(id, property.StcWeighted, None, request, state)),
                // ISO 717-1 Rw over the material's own measured SRI spectrum — the matched standard for the ISO
                // RequiredRw demand, never the ASTM contour the assembly estimate yields.
                single: s => Material(graph, s.Material, id, request, state),
                profileSet: s => Material(graph, s.Material, id, request, state),
                constituentSet: s => Material(graph, s.PrimaryMaterial, id, request, state)));

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Material(ElementGraph graph, MaterialId material, NodeId id, AssessmentRequest.Acoustic request, (Seq<AssessmentFact> Facts, double Governing) state) =>
        graph.Material(material).Map(static m => m.Properties).ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, material.Value))
            .Bind(props => RateAcoustic(id, props.Acoustic.Map(static a => a.Rw), props.Acoustic.Map(static a => a.Nrc), request, state));

    // EN 12354-6 / ISO 3382-1 room-reverberation fold over the seam eighteen-band AbsorptionSpectrum: per band the
    // absorption area A(b) = sum(S_i*alpha_i(b)) over the space's bounding surfaces (the BoundaryReads
    // IfcRelSpaceBoundary owner — one space-surface read, never a second), Sabine T = 0.161*V/A, the Eyring
    // T = 0.161*V/(-S*ln(1-alpha_mf)) correction beside it for the high-absorption room Sabine over-predicts; T_mf
    // is the 500/1000 Hz mean the target judges. Volume and per-surface areas are baked Qto evidence; a surface
    // material with no Acoustic case rails typed, never a defaulted alpha.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Room(ElementGraph graph, AssessmentRequest.Acoustic request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from volume in graph.Magnitude(id, QuantityRows.NetVolume).Filter(static v => v > 0.0).ToFin(Missing(AssessmentInputReason.MeasureAbsent, $"net-volume:{id.Value}"))
        from panels in graph.BoundingSurfacesOf(id).TraverseM(surface =>
            from area in graph.Magnitude(surface.Id, QuantityRows.SurfaceArea)
                .Filter(static a => a > 0.0).ToFin(Missing(AssessmentInputReason.MeasureAbsent, $"surface-area:{surface.Id.Value}"))
            from composition in graph.CompositionOf(surface.Id).ToFin(Missing(AssessmentInputReason.CompositionShape, surface.Id.Value))
            from props in graph.Material(composition.PrimaryMaterial).Map(static m => m.Properties).ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, surface.Id.Value))
            from acoustic in props.Acoustic.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, $"absorption:{surface.Id.Value}"))
            select (AreaM2: area, Absorber: acoustic)).As()
        from _ in panels.IsEmpty ? Fin.Fail<Unit>(Missing(AssessmentInputReason.CompositionEmpty, $"bounding-surfaces:{id.Value}")) : Fin.Succ(unit)
        let surfaceArea = panels.Sum(static p => p.AreaM2)
        let absorptionMid = panels.Sum(p => p.AreaM2 * (p.Absorber.At(AcousticBand.Hz500) + p.Absorber.At(AcousticBand.Hz1000)) / 2.0)
        let meanAlpha = absorptionMid / Math.Max(surfaceArea, EpsilonPolicy.SeamUlp)
        let sabineMid = SabineConstant * volume / Math.Max(absorptionMid, EpsilonPolicy.SeamUlp)
        // A room whose mean absorption reaches unity has no Eyring correction to state: -ln(1-alpha) diverges, and
        // the decay it would describe is instantaneous. Absence is the honest answer; the retired `: 0.0` published
        // it as a measured reverberation time of nothing.
        let eyringMid = meanAlpha < 1.0
            ? Some(SabineConstant * volume / Math.Max(-surfaceArea * Math.Log(1.0 - meanAlpha), EpsilonPolicy.SeamUlp))
            : Option<double>.None
        from sabineFact in AssessmentFact.Measure($"{id.Value}/reverberation-time-mid", Dimension.DurationDim, sabineMid)
        from eyringFact in eyringMid.Traverse(t => AssessmentFact.Measure($"{id.Value}/reverberation-time-eyring", Dimension.DurationDim, t)).As()
        from alphaFact in AssessmentFact.Ratio($"{id.Value}/mean-absorption-mid", meanAlpha)
        let ratio = request.TargetReverberationS.Filter(static t => t > 0.0).Map(t => sabineMid / t).IfNone(double.NaN)
        select (state.Facts.Add(sabineFact).Add(alphaFact) + eyringFact.ToSeq(), Math.Max(state.Governing, ratio));

    // Single-number sound-reduction fact and optional single-material Nrc combine with the RequiredRw/rating
    // governing ratio: a HIGHER rating is better, so the demand/capacity ratio is required-over-achieved (the same
    // orientation as the fire RequiredMinutes/achieved). A RequiredRw <= 0 is informational — the ratio is
    // double.NaN, which Math.Max propagates across the multi-target fold, so a no-target run bands NotApplicable.
    // `rating` holds EITHER the single-material ISO 717-1 Rw OR the ASSEMBLY ASTM E413 StcWeighted mass-law
    // estimate — a dual-standard single-number slot, never an ISO demand judged against an ASTM rating on the same
    // path. An ABSENT rating is its own outcome: both paths reach here with None and report the `absent` text fact
    // against a NaN governing rather than dividing a real RequiredRw by a rating nothing measured. The `dB` label is
    // a REPORT token on a dimensionless single-number rating: the seam refuses a UnitsNet decibel by name because a
    // logarithmic magnitude cannot enter the linear algebra, and this value is published, never Summed or Combined.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> RateAcoustic(NodeId id, Option<int> rating, Option<double> nrc, AssessmentRequest.Acoustic request, (Seq<AssessmentFact> Facts, double Governing) state) =>
        rating.Match(
            None: () => Fin.Succ((state.Facts.Add(AssessmentFact.Text($"{id.Value}/acoustic", "absent")), Math.Max(state.Governing, double.NaN))),
            Some: value =>
                from sri in MeasureValue.OfSi(QuantityType.Scalar, Dimension.Dimensionless, value, Some(UnitProvenance.Label("dB")))
                from nrcFact in nrc.Traverse(n => AssessmentFact.Ratio($"{id.Value}/nrc", n)).As()
                let ratio = request.RequiredRw > 0.0 ? request.RequiredRw / Math.Max(value, EpsilonPolicy.SeamUlp) : double.NaN
                select (state.Facts.Add(AssessmentFact.Measure($"{id.Value}/sound-reduction-index", sri)) + nrcFact.ToSeq(),
                    Math.Max(state.Governing, ratio)));
}
```

## [04]-[FIRE_RESISTANCE]

- Owner: `BuildingPhysics.RunFire` the fire runner; `FireExposure` the `[SmartEnum<string>]` exposure model carrying the convection coefficient, the nominal gas-temperature-time curve, AND its own exposed-perimeter reduction as a delegate column; `FireMemberClass` the `[SmartEnum<string>]` EN 1992-1-2 member-type row carrying its own `(minutes, min dimension, axis distance)` band table; `SteelFireState` the march receipt carrying the solver's own bounded-budget `Convergence` verdict; the EN 1993-1-2 critical-temperature fold and the EN 1992-1-2 tabulated check, dispatched by the fire route.
- Entry: `public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, IClock clock)` — resolves its arm off the `FireRoutes` table (`en1993-1-2` the steel march, `en1992-1-2` the concrete tabulated check), reads each member's `SectionProperties` off its `ProfileSet` composition, emits the `fire-resistance-minutes`/`critical-temperature`/`section-factor` (steel) and `required-min-dimension`/`least-dimension`/`required-axis-distance`/`axis-distance` (concrete) facts, and threads `max(RequiredMinutes / achieved)`.
- Auto: the steel fold marches the exposure's gas curve and the net convective+radiative flux over the section factor `Am/V` at a 5 s step with the EN 1993-1-2 temperature-dependent specific heat `c_a(θ_a)` read off a BANDED ROW TABLE and the section's own shape-derived shadow factor `k_sh`; the march terminates on ONE of two typed verdicts — `Converged` at the time the steel reached its critical temperature, which is a MEASURED resistance, or `Exhausted` at the budget, which is a LOWER BOUND and is published as one; the concrete fold reads the `FireMemberClass` row's own band table for the required rating's `(min dimension, min axis distance)` pair and checks both against the section's `LeastDimension` and `AxisDistance` cover, the achieved resistance the worse-governed of the two.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]` — the gas-curve, exposed-perimeter, and member-band delegate columns), Rasm.Element (project — `SectionProperties` incl. `AxisDistance` and the derived `LeastDimension`, the seam `ElementGraph.SectionOf` accessor, `Node.Object` for the `Classification.Code` member-type read, `MeasureValue`, `Dimension`, `QuantityType`, `UnitProvenance`, `NodeId`), `Solver/contract` (`Convergence` — the ONE bounded-budget verdict), the `RouteTable` dispatch owner, Rasm (kernel — `EpsilonPolicy.SeamUlp`), the `Analysis/frame` `DivisorBand` guarded quotient, BCL inbox.
- Growth: a new fire model (a parametric EN 1991-1-2 natural fire, an EN 1995-1-2 timber charring rate, an EN 1993-1-2 PROTECTED-steel march with an insulation `λ_p`/`d_p` term) is one `FireRoutes` row over one `TargetFold` reading the same section — the table rails an unrecognized route until its row lands, never silently charging a timber member against the concrete table; a new EN 1992-1-2 member type is one `FireMemberClass` row carrying its own bands; a new exposure is one `FireExposure` row carrying its gas curve, its convection coefficient, and its perimeter reduction; the 500 °C isotherm method deepens as a fold over the section thermal field where the tabulated check is insufficient.
- Boundary: the section factor `Am/V` reads the seam `SectionProperties.HeatedPerimeter`/`Area` reduced by the EXPOSURE ROW's own perimeter delegate, so a 3-sided exposure shields the top flange and the standard and external rows genuinely differ in the marched heating rate — the retired bare `Sides` int the branch read is gone and the row carries the behavior; the section resolves through the seam's Op-free `SectionOf` accessor, never re-resolving a `ProfileRef` and never admitting VividOrange; the degree of utilization `μ0` is the ambient governing ratio carried on the request and is ADMITTED, not clamped — a utilization at or above unity means the member already fails at ambient and publishing a fire rating for it is the refused fabrication the retired `Math.Clamp` performed silently. Steel marches a genuine incremental integration and its resistance is the marched time ONLY where the march reached the critical temperature; a member that survived its whole budget has a resistance the march did not measure, and its `fire-resistance-floor` fact says exactly that. The concrete check is the full EN 1992-1-2 tabulated method with both criteria gating and the worse-governed achieved; the member class is a DISCRIMINANT railed like every sibling, so an unrecognized class refuses by name rather than reading the beam table, whose thresholds are anti-conservative for a column.
- Boundary: the EN 1993-1-2 §4.2.5.1 march exists on BOTH sides of the Materials seam and the concerns are DISTINCT, discriminated by exposure model and utilisation source. `Rasm.Materials` `Component/steel` marches ISO 834 alone at its own `SteelFire.DefaultUtilisation` to bake a SECTION-altitude `SteelFireFacts` receipt (section factor, the Table 3.1 `Ky`/`KE` retention pair, the §4.2.4 critical temperature) consumed only by `CapacityReceipt.Fire`; this runner marches the REQUEST's own `FireExposure` row — including the hydrocarbon and external curves that owner does not model — at the run's own ambient governing ratio, to answer a MEMBER-altitude time-to-critical question a section receipt cannot hold. Composition is named but not takeable: the Materials receipt publishes no member rows, so there is nothing here to read the way `ShearLinkOf` and `BucklingOf` read their producers' rows. Where those rows land, the retention pair and the critical temperature come from the section owner and this runner marches only the exposure-and-time integration the seam cannot decide.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Exposure rows carry the gas curve, the convection coefficient, AND the exposed-perimeter reduction: a 3-sided
// exposure shields the top flange, so the exposed perimeter drops by the section width. The retired `Sides` int and
// the `exposure.Sides >= 4` branch that read it both dissolve into the row idiom this class already used one
// declaration above for its gas curve.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FireExposure {
    public static readonly FireExposure Standard    = new("iso834",      convectionWM2K: 25.0, gasTempC: Iso834,           exposedPerimeterM: AllSides);
    public static readonly FireExposure Hydrocarbon = new("hydrocarbon", convectionWM2K: 50.0, gasTempC: HydrocarbonCurve, exposedPerimeterM: AllSides);
    public static readonly FireExposure External    = new("external",    convectionWM2K: 25.0, gasTempC: ExternalCurve,    exposedPerimeterM: ThreeSided);

    public double ConvectionWM2K { get; }   // EN 1991-1-2: alpha_c = 25 (standard/external), 50 (hydrocarbon)

    // Nominal gas-temperature-time curve (EN 1991-1-2 §3.2) is a per-row delegate, never a Key switch.
    [UseDelegateFromConstructor]
    public partial double GasTempC(double minutes);

    [UseDelegateFromConstructor]
    public partial double ExposedPerimeterM(SectionProperties section);

    static double Iso834(double minutes)           => 20.0 + (345.0 * Math.Log10((8.0 * minutes) + 1.0));
    static double HydrocarbonCurve(double minutes) => 20.0 + (1080.0 * (1.0 - (0.325 * Math.Exp(-0.167 * minutes)) - (0.675 * Math.Exp(-2.5 * minutes))));
    static double ExternalCurve(double minutes)    => 20.0 + (660.0 * (1.0 - (0.687 * Math.Exp(-0.32 * minutes)) - (0.313 * Math.Exp(-3.8 * minutes))));

    static double AllSides(SectionProperties section) => section.HeatedPerimeter.Si;
    static double ThreeSided(SectionProperties section) => Math.Max(section.HeatedPerimeter.Si - section.Width.Si, section.Width.Si);
}

// EN 1992-1-2 tabulated method: each member type is a ROW carrying its own (rating minutes, min dimension, min axis
// distance) bands from Tables 5.2a/5.4/5.5/5.8, keyed on the seam Object Classification.Code (the IFC entity class,
// not the PredefinedType sub-type). The retired string-keyed FrozenDictionary re-opened a closed member-type family
// and refused an unknown class by empty slug; this row refuses by name.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FireMemberClass {
    public static readonly FireMemberClass Column = new("IfcColumn", static _ => Bands(
        (30.0, 0.200, 0.025), (60.0, 0.250, 0.046), (90.0, 0.350, 0.053), (120.0, 0.350, 0.057), (180.0, 0.450, 0.070), (240.0, 0.450, 0.075)));
    public static readonly FireMemberClass Beam = new("IfcBeam", static _ => Bands(
        (30.0, 0.080, 0.025), (60.0, 0.120, 0.040), (90.0, 0.150, 0.055), (120.0, 0.200, 0.065), (180.0, 0.240, 0.080), (240.0, 0.280, 0.090)));
    public static readonly FireMemberClass Slab = new("IfcSlab", static _ => Bands(
        (30.0, 0.060, 0.010), (60.0, 0.080, 0.020), (90.0, 0.100, 0.030), (120.0, 0.120, 0.040), (180.0, 0.150, 0.055), (240.0, 0.175, 0.065)));
    public static readonly FireMemberClass Wall = new("IfcWall", static _ => Bands(
        (30.0, 0.100, 0.010), (60.0, 0.110, 0.010), (90.0, 0.120, 0.020), (120.0, 0.150, 0.025), (180.0, 0.180, 0.040), (240.0, 0.230, 0.055)));

    [UseDelegateFromConstructor]
    public partial ImmutableArray<(double Minutes, double MinDimM, double AxisDistanceM)> Table(Unit _);

    static ImmutableArray<(double Minutes, double MinDimM, double AxisDistanceM)> Bands(params ReadOnlySpan<(double Minutes, double MinDimM, double AxisDistanceM)> rows) => [.. rows];

    // First band whose threshold covers the requirement, else the top band. The tuple element names the declared
    // type carries survive the read, so no consumer re-derives which of three doubles is the axis distance.
    public (double MinDimM, double AxisDistanceM) Limits(double requiredMinutes) {
        ImmutableArray<(double Minutes, double MinDimM, double AxisDistanceM)> table = Table(unit);
        (double Minutes, double MinDimM, double AxisDistanceM) row = table.FirstOrDefault(r => r.Minutes >= requiredMinutes, table[^1]);
        return (row.MinDimM, row.AxisDistanceM);
    }

    public static Fin<FireMemberClass> Of(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Map(static o => o.Classification.Code)
            .Bind(static code => TryGet(code, out FireMemberClass row) ? Some(row) : None)
            .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.MemberClassUnhandled, $"fire-member-class:{id.Value}"));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The march receipt carries the solver lane's own bounded-budget verdict: Converged names the time the steel
// reached its critical temperature (a MEASURED resistance), Exhausted names the budget the march ran out at (a
// LOWER BOUND, never a resistance). The retired `march.SteelTempC >= critical ? Minutes : cap` reported an
// unconverged march as a resistance of exactly `cap` and divided a real requirement by it.
public readonly record struct SteelFireState(double Minutes, double SteelTempC, Convergence Verdict);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    const double Emissivity = 0.7;        // EN 1993-1-2 resultant emissivity (eps_m.eps_f.Phi) for carbon steel
    const double Sigma = 5.67e-8;         // Stefan-Boltzmann, W/(m2.K4)
    const double RhoSteel = 7850.0;       // kg/m3
    const double StepSeconds = 5.0;       // EN 1993-1-2 §4.2.5.1 caps the increment at 5 s
    const double CapMarginMinutes = 30.0; // march beyond the requirement so a surviving member reports a bounded floor

    static readonly Op FireKey = Op.Of(name: nameof(RunFire));

    // EN 1993-1-2 §3.4.1.2 carbon-steel specific heat c_a(theta) [J/(kg.K)] — a BANDED ROW TABLE, which is what the
    // retired four-arm ternary ladder's own comment claimed it was. The 735 degC latent-heat singularity delays
    // heating and is a real discontinuity in the standard, so it is a band edge rather than a smoothing.
    static readonly ImmutableArray<(double UpperC, Func<double, double> Heat)> SpecificHeatBands = [
        (600.0,           static t => 425.0 + (0.773 * t) - (1.69e-3 * t * t) + (2.22e-6 * t * t * t)),
        (735.0,           static t => 666.0 + (13002.0 / (738.0 - t))),
        (900.0,           static t => 545.0 + (17820.0 / (t - 731.0))),
        (double.MaxValue, static _ => 650.0)];

    // EN 1993-1-2 §4.2.5.1 shadow factor k_sh: the correction is the ratio of the box-perimeter section factor to
    // the section's own, so it reads the SECTION rather than a class constant — an RHS or a hollow section no longer
    // takes the I-section nominal 0.9 the retired const applied to every profile.
    static double ShadowFactor(SectionProperties section) =>
        Math.Min(1.0, 0.9 * (2.0 * (section.Depth.Si + section.Width.Si)) / Math.Max(section.HeatedPerimeter.Si, EpsilonPolicy.SeamUlp));

    static readonly FrozenDictionary<AssessmentRoute, TargetFold<AssessmentRequest.Fire>> FireRoutes = RouteTable.Of<AssessmentRequest.Fire>(
        (AssessmentRoute.En1993Fire, SteelFire), (AssessmentRoute.En1992Fire, ConcreteFire));

    public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, IClock clock) =>
        RouteTable.Run(FireRoutes, graph, request, request.Route, request.Targets, FireKey, clock);

    // Member section reads the seam Op-free SectionOf off the M7-baked ProfileSet; a member with no resolved section
    // rails the typed fault. The seam owns the section read, so this runner composes it one-hop — the same read
    // Analysis/frame takes.
    static Fin<SectionProperties> MemberSection(ElementGraph graph, NodeId id) =>
        graph.SectionOf(id).ToFin(Missing(AssessmentInputReason.MeasureAbsent, $"section:{id.Value}"));

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> SteelFire(ElementGraph graph, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from section in MemberSection(graph, id)
        // The degree of utilization mu0 is ADMITTED, not clamped: a member at or above unity already fails at
        // ambient and the eq. 4.22 logarithm has no argument for it, so publishing a fire rating there is the
        // fabrication the retired Math.Clamp(0.013, 0.99) performed in silence. The 0.013 floor IS the standard's
        // own validity bound and stays a band edge.
        from utilization in request.Utilization is > 0.013 and < 1.0
            ? Fin.Succ(request.Utilization)
            : Fin.Fail<double>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input,
                $"<fire-utilization-out-of-band:{id.Value}:{request.Utilization:R}>"))
        let sectionFactor = request.Exposure.ExposedPerimeterM(section).Over(section.Area.Si)
        let criticalTempC = CriticalTemperature(utilization)
        let cap = request.RequiredMinutes + CapMarginMinutes
        let march = March(request.Exposure, section, sectionFactor, criticalTempC, cap)
        from resistance in Resistance(id, march, cap)
        from critical in MeasureValue.OfSi(Dimension.TemperatureDim, criticalTempC + 273.15)
        from factor in MeasureValue.OfSi(QuantityType.OfDimension(PerLengthDim), PerLengthDim, sectionFactor, Some(UnitProvenance.Label("1/m")))
        select (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/critical-temperature", critical))
                .Add(AssessmentFact.Measure($"{id.Value}/section-factor", factor)) + resistance.Facts,
            Governing: Math.Max(state.Governing, request.RequiredMinutes.Over(resistance.Minutes)));

    // The march's verdict decides WHICH fact the resistance is. A converged march measured the time to critical and
    // publishes it; an exhausted one measured only that the member outlasted its budget and publishes that BOUND
    // under its own name, so no consumer reads a floor as a measurement. A stalled march measured nothing at all.
    static Fin<(Seq<AssessmentFact> Facts, double Minutes)> Resistance(NodeId id, SteelFireState march, double cap) =>
        march.Verdict.Switch(
            converged: _ => MeasureValue.OfSi(Dimension.DurationDim, march.Minutes * 60.0)
                .Map(value => (Seq(AssessmentFact.Measure($"{id.Value}/fire-resistance-minutes", value)), march.Minutes)),
            exhausted: budget => MeasureValue.OfSi(Dimension.DurationDim, march.Minutes * 60.0)
                .Map(value => (Seq(AssessmentFact.Measure($"{id.Value}/fire-resistance-floor", value)), (double)budget.Budget)),
            stalled: _ => Fin.Fail<(Seq<AssessmentFact>, double)>(new ComputeFault.AnalysisFailed(
                SolvePhase.Extraction, FailureKind.Numeric, $"<steel-fire-march-stalled:{id.Value}:cap={cap:R}>")));

    // EN 1993-1-2 §4.2.5.1 unprotected-steel march integrates net convective+radiative flux over the section factor
    // with the temperature-dependent specific heat c_a(theta_a) and the exposure's convection coefficient and gas
    // curve; never a tabulated approximation. Exemption: the incremental integration is the measured-kernel
    // statement seam — but the exemption licenses the loop, never a success-shaped fall-through, so the terminating
    // condition IS the verdict.
    static SteelFireState March(FireExposure exposure, SectionProperties section, double sectionFactor, double criticalTempC, double capMinutes) {
        double steelTempC = 20.0, minutes = 0.0, shadow = ShadowFactor(section);
        while (steelTempC < criticalTempC && minutes < capMinutes) {
            double gasTempC = exposure.GasTempC(minutes);
            double netConv = exposure.ConvectionWM2K * (gasTempC - steelTempC);
            double netRad = Emissivity * Sigma * (Pow4(gasTempC + 273.0) - Pow4(steelTempC + 273.0));
            steelTempC += (shadow * sectionFactor).Over(SpecificHeatSteel(steelTempC) * RhoSteel) * (netConv + netRad) * StepSeconds;
            minutes += StepSeconds / 60.0;
        }
        return new SteelFireState(minutes, steelTempC,
            steelTempC >= criticalTempC ? new Convergence.Converged(steelTempC - criticalTempC) : new Convergence.Exhausted((int)capMinutes));
    }

    static double SpecificHeatSteel(double tempC) =>
        SpecificHeatBands.First(band => tempC < band.UpperC).Heat(tempC);

    // EN 1993-1-2 eq 4.22: the critical steel temperature for the admitted degree of utilization mu0.
    static double CriticalTemperature(double utilization) =>
        (39.19 * Math.Log(1.0.Over(0.9674 * Math.Pow(utilization, 3.833)) - 1.0)) + 482.0;

    static double Pow4(double x) { double s = x * x; return s * s; }

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> ConcreteFire(ElementGraph graph, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from section in MemberSection(graph, id)
        from row in FireMemberClass.Of(graph, id)
        from folded in ConcreteTabulated(section, row.Limits(request.RequiredMinutes), request, id, state)
        select folded;

    static Fin<(Seq<AssessmentFact> Facts, double Governing)> ConcreteTabulated(SectionProperties section, (double MinDimM, double AxisDistanceM) limits, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) {
        double leastM = section.LeastDimension.Si, axisM = section.AxisDistance.Si;
        // EN 1992-1-2 gates on BOTH the minimum cross-section dimension and the axis distance (cover-to-
        // reinforcement), so the achieved resistance is the WORSE-governed of the two: an otherwise adequate section
        // with thin cover fails on the criterion a dimension-only check could not reach.
        double dimAchieved = leastM >= limits.MinDimM ? request.RequiredMinutes : request.RequiredMinutes * leastM.Over(limits.MinDimM);
        double axisAchieved = axisM >= limits.AxisDistanceM ? request.RequiredMinutes : request.RequiredMinutes * axisM.Over(limits.AxisDistanceM);
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
                Governing: Math.Max(state.Governing, request.RequiredMinutes.Over(achieved)));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
