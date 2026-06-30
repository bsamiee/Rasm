# [COMPUTE_PHYSICS]

Rasm.Compute closed-form building-physics runner: the `Discipline.Thermal`/`Discipline.Acoustic`/`Discipline.Fire` arms of the `Analysis/assessment` rail collapsed onto ONE `BuildingPhysics` kernel because all three are closed-form ISO/EN folds over an assembly or section read DIRECTLY from the concrete `Rasm.Element` `ElementGraph` (above the seam, no `IElementProjection`) — no external solver, no subprocess. Thermal reads the multi-ply `UValueWM2K` from the relocated `Analysis/aggregator` `AssemblyAggregator.Aggregate` (the ONE ISO 6946 series-resistance owner, never a re-derived U) and runs the EN ISO 13788 Glaser steady-state interstitial-condensation profile over the per-layer thermal-and-vapour resistances, computing the condensation MASS RATE through the genuine lower-convex-hull tangent construction (not a bare boolean) and the per-interface vapour-pressure utilization; for a window assembly it instead composes `AssemblyAggregator.AggregateWindow` for the EN ISO 10077-1 area-and-perimeter-weighted WHOLE-WINDOW `Uw` (the glazed center-of-glass `Ug` off the IGU material's `Thermal.UValue`, the frame `Uf` off the frame material's, the edge-seal `Ψg` off the window's `Pset` thermal-bridge property, the areas off the window's `Qto_*BaseQuantities`) — the side-by-side glazing-in-frame composition the through-thickness series-U structurally cannot reach, so the window assessment reports the genuine whole-window U rather than the raw center-of-glass figure. Acoustic reads the layered mass-law sound-reduction index from `AssemblyAggregator.Aggregate` (whose per-band ISO 12354 fold rides the SEAM `Composition/acoustic#ACOUSTIC_FOLDS` `RatingContour.Stc.Fit` ASTM E413 contour — one contour owner, never a second algorithm — the assembly carrier exposing only that mass-law `StcWeighted`) for a `LayerSet` and the intrinsic seam `Nrc`/`Rw` (the `RatingContour.Rw.Fit` ISO 717-1 contour over the material's measured SRI spectrum, the matched standard for the ISO `RequiredRw` demand the assembly mass-law estimate cannot offer) for a single material. Fire folds the EN 1993-1-2 unprotected-steel critical-temperature method (the exposure-dependent gas curve over the section factor `Am/V` marched with the EN temperature-dependent specific heat `c_a(θ)` to the degree-of-utilization critical temperature) and the EN 1992-1-2 concrete tabulated minimum-dimension check, dispatched by the fire route. Each runner returns one `AssessmentResult` fact stream the spine writes back, the governing ratio THREADED through the fold accumulator (`U / U_target` and the vapour utilization for thermal, `RequiredMinutes / achieved` for fire) so the verdict derives from the in-scope governing quantity and never re-parses the emitted facts. Every measured fact is constructed SI-native through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.OfSi(Dimension, si)` or the raw `MeasureValue` record for a domain-labelled scalar — never the phantom 2-arg `MeasureValue.Of(value, unit)` the seam factory does not expose. The page composes the seam vocabulary settled — `MaterialComposition`/`MaterialLayer`, the `MaterialPropertySet.Thermal`/`Acoustic` cases through the `MaterialPropertyAccess` accessors, `SectionProperties`, `MeasureValue`/`Dimension` — plus the `Analysis/aggregator` engine; the single-material PURE acoustic folds stay seam-owned and the multi-ply layered physics is this runner.

## [01]-[INDEX]

- [01]-[THERMAL_ENVELOPE]: the ISO 6946 series-`U` read from `AssemblyAggregator.Aggregate` and the EN ISO 13788 Glaser interstitial-condensation profile with the lower-convex-hull tangent condensation-rate construction over the per-layer thermal/vapour resistances against a `BoundaryClimate`, PLUS the EN ISO 10077-1 whole-window `Uw` read from `AssemblyAggregator.AggregateWindow` over a window's glazed/frame `WindowField` set the runner resolves Op-free off the window's `Compose` parts + baked `Qto`/`Pset` bags (the side-by-side window the through-thickness series cannot reach).
- [02]-[ACOUSTIC_RATING]: the ISO 12354 layered sound-reduction index read from `AssemblyAggregator.Aggregate` through the SEAM `RatingContour.Stc.Fit` weighted single number plus the single-material seam `Nrc` projection.
- [03]-[FIRE_RESISTANCE]: the EN 1993-1-2 steel critical-temperature incremental fold over the exposure gas curve and the section factor, and the EN 1992-1-2 concrete tabulated minimum-dimension check, dispatched by the fire route.

## [02]-[THERMAL_ENVELOPE]

- Owner: `BuildingPhysics.RunThermal` the thermal runner; `BoundaryClimate` the interior/exterior temperature-and-humidity boundary carried on the request; `GlaserProfile` the per-interface temperature/saturation/actual-vapour-pressure receipt; `GlaserResult` the condensation summary (the vapour utilization, the lower-hull condensation rate, the worst interface) the fold derives; `WindowFields` the Op-free window-assembly resolver that reads a window's glazed/frame `WindowField` set off its `Compose` parts + baked `Qto`/`Pset` bags.
- Entry: `public static Fin<AssessmentResult> RunThermal(ElementGraph graph, AssessmentRequest.Thermal request, ClockPolicy clocks)` — per target, resolves the WINDOW path FIRST (a window assembly carries glazed/frame `WindowField`s — its glazing-infill and frame parts each a `Thermal.UValue` plus the `Qto_*BaseQuantities` areas and the `Pset` spacer `Ψg` `WindowFields` reads): when the field set is non-empty it composes `AssemblyAggregator.AggregateWindow` for the EN ISO 10077-1 whole-window `Uw` and emits the `whole-window-u`/`glazed-u`/`frame-u`/`edge-bridge`/`glazed-fraction` facts threading `Uw / U_target`; ELSE it falls to the envelope path — resolves the target's seam `MaterialComposition`, reads the `UValueWM2K` from `AssemblyAggregator.Aggregate` for a `LayerSet` (and the intrinsic `Thermal.UValue` for a `Single`), runs the EN ISO 13788 Glaser fold over the same `LayerSet` against `request.Climate`, emits the `u-value`/`condensation-risk`/`vapour-utilization`/`condensation-rate`/`condensation-plane` facts, and threads `max(U / U_target, vapour-utilization)` through the accumulator; `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a layer material node is absent or lacks a thermal property, or a window part lacks its frame/glazing `Thermal.UValue`.
- Auto: the per-interface temperature is `T_i = T_int − (ΣR_0..i / R_total)·(T_int − T_ext)` over the cumulative thermal resistances including the `Rsi`/`Rse` surface films; the saturation vapour pressure is the Magnus form `p_sat = 610.5·exp(17.269·T/(237.3+T))`; the straight actual vapour line is `p_int − (Z_i / Z_total)·(p_int − p_ext)` over the cumulative vapour resistance `Z_i = Σ(μ_j·t_j)/δ0`; the EN ISO 13788 condensation construction is the LOWER CONVEX HULL of the `(Z, p)` points from `(0, p_int)` through each interface `(Z_i, p_sat_i)` to `(Z_total, p_ext)` — the interior hull vertices are the condensation planes and the condensation rate `g_c` at each is the flux discontinuity `(p_u − p_v)/(Z_v − Z_u) − (p_v − p_w)/(Z_w − Z_v)` between adjacent hull vertices; the window path resolves each `Compose` part of the window Op-free (`graph.EdgesAt(window)` → the glazing-infill and frame `Object` parts, each part's glazed `Ug` / frame `Uf` off `graph.PropertiesOf(part).Thermal.UValue.Si`), reads the glazed area + frame area off the window's `Qto_*BaseQuantities` quantity bag (`GlazingArea`/`Area`) and the spacer `Ψg` + visible-glazing edge length off the window's `Pset` thermal-bridge property — both Op-free off the `Assign.PropertyDefinition`-bound `QuantitySet`/`PropertySet` nodes — assembling the `WindowField.Glazed`/`Frame` set `AssemblyAggregator.AggregateWindow` folds.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`), Rasm.Element (project — `MaterialComposition`, `MaterialLayer`, `MaterialPropertySet.Thermal` via `MaterialPropertyAccess`, `MaterialId`, `MeasureValue`, `Dimension`, `NodeId`, the `Node.Object`/`Node.QuantitySet`/`Node.PropertySet` cases + `Relationship.Compose`/`Relationship.Assign`/`ComposeKind`/`AssignKind` the window-part/bag incidence read resolves, `PropertyName`, `PropertyValue`, `QuantityBag`/`PropertyBag`), the `Analysis/aggregator` `AssemblyAggregator` (the `Aggregate` series-U AND the `AggregateWindow` whole-window fold + the `WindowField`/`WindowU` shapes), BCL inbox (`Math`, span/array hull kernel).
- Growth: a new thermal check (a dynamic decrement/admittance pair) is one fold over the same `LayerSet` reading the aggregator's `ArealHeatCapacityKJM2K`/`VapourResistanceSdM`, never a parallel envelope owner; the moisture model deepens to the EN 15026 transient form as one fold swap reading the same layer resistances; a 12-month annual condensation/evaporation balance is one fold over a climate series once `BoundaryClimate` carries one; the whole-window `Uw` is the `AssemblyAggregator.AggregateWindow` compose (the EN ISO 10077-1 area-and-perimeter weighting the through-thickness series cannot reach), and a richer EN ISO 10077-2 numerical frame model is one deeper `WindowField` resolution (a per-frame-profile measured `Uf`/`Ψg`) the runner supplies, never a parallel window owner.
- Boundary: the multi-ply `U` composes `AssemblyAggregator.Aggregate` (the ISO 6946 series-resistance owner with the `Rsi`/`Rse` films) so the thermal envelope and the aggregator share ONE series-resistance owner, never a re-derived U; the Glaser fold INDEPENDENTLY reads each layer's `MaterialPropertySet.Thermal.Conductivity.Si` and `VapourResistanceFactor` (μ) for the per-interface profile the aggregator's total `Sd` cannot carry — a layer missing the thermal property rails the typed fault rather than defaulting; the resolver is keyed on the composition's native `MaterialId` (`graph.Material(MaterialId)`), never a graph `NodeId`; the condensation construction is the genuine EN ISO 13788 lower-hull tangent method computing the condensation MASS RATE `g_c` (kg·m⁻²·s⁻¹), not a bare crossing flag, so the persistent-accumulation severity is a reported scalar; the WHOLE-WINDOW `Uw` composes `AssemblyAggregator.AggregateWindow` (the EN ISO 10077-1 area-and-perimeter owner) so the window transmittance and the aggregator share ONE owner — the runner resolves the glazed `Ug` and frame `Uf` off each part material's seam `Thermal.UValue.Si` (the IGU `Ug` `GlazingSection.Performance` lowered), reading them seam-direct and area-weighting them WITH the spacer linear bridge rather than reporting the raw glazing `Ug` as the window U (the deleted single-material-`u-value`-over-a-glazing-node form that drops the frame fraction AND the spacer bridge); the spacer `Ψg` is read off the window's `Pset` thermal-bridge property (`GlazingSection`'s `SpacerType.PsiWmK` lowers there — the seam `MaterialPropertySet.Thermal` carries no perimeter-bridge column, so a `Ψ`-on-`Thermal` read is the phantom the runner never takes), the glazed/frame areas off the window's `Qto_*BaseQuantities`, and a window with no glazed-or-frame field falls through to the envelope path (a non-window target) while a window whose part lacks its `Thermal.UValue` rails the typed fault; the window path is tried FIRST and the envelope path is the fall-through, so a regular wall/slab (no window parts/`Qto_WindowBaseQuantities`) takes the series-U+Glaser path unchanged; the governing ratio is `max(Uw / U_target)` for a window and `max(U / U_target, vapour-utilization)` for an envelope, threaded through the accumulator so the verdict derives from the in-scope governing quantity, never a re-parse of the emitted facts and never a sentinel ratio; every measured fact is SI-native `MeasureValue.OfSi`/`MeasureValue` raw, never the phantom 2-arg `MeasureValue.Of(value, unit)`.

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

    // SI dimensions composed from the seam Dimension algebra (no named row exists for these) — the kernel-true
    // discriminator a downstream unit canonicalization re-reads, never a hand-mapped kind. Each composed-dimension fact
    // builds the 4-arg seam MeasureValue (QuantityType, Dimension, Si, CanonicalUnit) record directly with the
    // dimension-anonymous QuantityType.OfDimension(dim) discriminator and the CONVENTIONAL unit label OfSi's SiSymbol
    // cannot supply for an unnamed dimension — never the phantom 3-arg (Dimension, Si, unit) ctor the seam record has no
    // overload for, and never OfSi(Dimension, _) which would discard the conventional kg/(m2.s)/dB/K/1/m label for "SI".
    static readonly Dimension TemperatureDim = Dimension.Create(0, 0, 0, 0, 1, 0, 0);
    static readonly Dimension PerLengthDim = Dimension.Dimensionless.Divide(Dimension.LengthDim);
    static readonly Dimension VapourFluxDim = Dimension.MassDim.Divide(Dimension.AreaDim).Divide(Dimension.DurationDim);
    // The EN ISO 10077-1 edge-seal bridge Σ lg·Ψg is a thermal conductance W/K = [M·L²·T⁻³·K⁻¹] (the area-U ThermalTransmittance
    // W·m⁻²·K⁻¹ times area m²) — the perimeter linear-bridge term the whole-window fact carries beside the area-U facts.
    static readonly Dimension EdgeBridgeDim = Dimension.ThermalTransmittance.Multiply(Dimension.AreaDim);

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
        AssemblyAggregator.AggregateWindow(fields).Map(w => {
            double uRatio = request.Climate.TargetUValueWM2K > 0.0 ? w.UwWM2K / request.Climate.TargetUValueWM2K : 0.0;
            return (Facts: state.Facts
                    .Add(AssessmentFact.Measure($"{id.Value}/whole-window-u", MeasureValue.OfSi(Dimension.ThermalTransmittance, w.UwWM2K)))
                    .Add(AssessmentFact.Measure($"{id.Value}/glazed-u", MeasureValue.OfSi(Dimension.ThermalTransmittance, w.UgWM2K)))
                    .Add(AssessmentFact.Measure($"{id.Value}/frame-u", MeasureValue.OfSi(Dimension.ThermalTransmittance, w.UfWM2K)))
                    .Add(AssessmentFact.Measure($"{id.Value}/edge-bridge", new MeasureValue(QuantityType.OfDimension(EdgeBridgeDim), EdgeBridgeDim, w.EdgeBridgeW_K, "W/K")))
                    .Add(AssessmentFact.Ratio($"{id.Value}/glazed-fraction", w.GlazedFraction)),
                Governing: Math.Max(state.Governing, uRatio));
        });

    // The through-thickness envelope branch (the prior RunThermal body): the ISO 6946 series-U from AssemblyAggregator
    // .Aggregate (a LayerSet) or the intrinsic Thermal.UValue (a Single), plus the EN ISO 13788 Glaser condensation fold.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> Envelope(ElementGraph graph, AssessmentRequest.Thermal request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) =>
        from composition in graph.CompositionOf(id).ToFin(Missing($"<thermal-element-missing-composition:{id.Value}>"))
        from u in UValue(composition, Resolver(graph))
        from glaser in composition is MaterialComposition.LayerSet set
            ? GlaserOf(set, Resolver(graph), request.Climate)
            : Fin.Succ(GlaserResult.None)
        let uRatio = request.Climate.TargetUValueWM2K > 0.0 ? u / request.Climate.TargetUValueWM2K : 0.0
        select (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/u-value", MeasureValue.OfSi(Dimension.ThermalTransmittance, u)))
                .Add(AssessmentFact.Flag($"{id.Value}/condensation-risk", glaser.Condensing))
                .Add(AssessmentFact.Ratio($"{id.Value}/vapour-utilization", glaser.VapourUtilization))
                .Add(AssessmentFact.Measure($"{id.Value}/condensation-rate", new MeasureValue(QuantityType.OfDimension(VapourFluxDim), VapourFluxDim, glaser.CondensationRateKgM2S, "kg/(m2.s)")))
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
    // The EN ISO 10077-1 window-field resolver: read the window's Compose parts (the glazing-infill + frame Objects) and
    // its baked Qto/Pset bags Op-free off the incidence index, assembling the glazed/frame WindowField set AggregateWindow
    // folds. A target with NO glazed area quantity is NOT a window (an empty set the runner falls through to the envelope
    // path on) — the Qto_*BaseQuantities GlazingArea is the seam-direct window discriminant, present iff the element is a
    // glazed window/door. A part that is a window component but whose material lacks its Thermal.UValue rails the typed
    // fault (an under-specified window, never a silently-dropped field). The frame area is the window Area minus the
    // glazed area (the opaque frame fraction), the visible-glazing edge length the GlazingPerimeter quantity.
    static Fin<Seq<WindowField>> WindowFields(ElementGraph graph, NodeId window) {
        Option<double> glazedArea = Quantity(graph, window, "GlazingArea");
        if (glazedArea.IsNone) { return Fin.Succ(Seq<WindowField>()); }   // not a window — fall through to the envelope path
        double ag = glazedArea.IfNone(0.0);
        double totalArea = Quantity(graph, window, "Area").IfNone(ag);
        double af = Math.Max(totalArea - ag, 0.0);
        double edgeLength = Quantity(graph, window, "GlazingPerimeter").IfNone(0.0);
        double psi = Property(graph, window, "GlazingEdgePsi").IfNone(0.0);
        // The glazing-infill part carries the EN 673 Ug (the GlazingSection.Performance value on Thermal.UValue), the
        // frame part the Uf — each part discriminated by its COMPOSITION shape (the glazing a LayerSet/Single IGU material,
        // the frame a ProfileSet member; WindowPart). The glazed area Ag is the Qto GlazingArea, the frame area Af the
        // residual Area − GlazingArea, the edge length lg the GlazingPerimeter, the spacer Ψg the element Pset property.
        return PartU(graph, window, isGlazing: true).Bind(ug =>
            PartU(graph, window, isGlazing: false).Map(uf =>
                Seq(WindowField.Glazed(ug, ag, edgeLength, psi), WindowField.Frame(uf, af))));
    }

    // The glazed-or-frame part's transmittance: the window's Compose child whose material carries the glazed (low-U IGU)
    // or frame (higher-U profile) Thermal.UValue — resolved Op-free off the incidence index. The glazing infill is the
    // part whose composition is a LayerSet/Single glazing material; the frame the ProfileSet member. A window missing the
    // required part, or a part missing its Thermal.UValue, rails the typed fault (an under-specified window assembly).
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

    // The EN ISO 13788 graphical condensation rate as the lower-convex-hull tangent construction (Exemption: array
    // hull kernel): the realized vapour line is the lower hull of the (Z, p) points from the interior actual pressure
    // through each interface saturation pressure to the exterior actual pressure; the interior hull vertices are the
    // condensation planes and g_c is the inflow-minus-outflow vapour-flux discontinuity at each, summed over the planes.
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
- Entry: `public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, ClockPolicy clocks)` — reads the layered mass-law single number from `AssemblyAggregator.Aggregate` (whose mass-law per-band SRI folds once through the SEAM `RatingContour.Stc.Fit` ASTM E413 contour, yielding `StcWeighted`) for a `LayerSet`, and the intrinsic seam `Nrc`/`Rw` (the SEAM `Acoustic` carrier's `RatingContour.Rw.Fit` ISO 717-1 contour over the material's MEASURED SRI spectrum) off the `PrimaryMaterial`'s `Acoustic` case for a single material, emits the `sound-reduction-index`/`nrc` facts, and threads `request.RequiredRw / Rw` through the accumulator (a higher Rw is better, so required-over-achieved) when the request carries a `RequiredRw` acceptance target. The cross-standard asymmetry is deliberate: the single material has a measured one-third-octave SRI spectrum yielding a TRUE ISO 717-1 `Rw`, while the layered assembly has only a mass-law SRI ESTIMATE whose ASTM E413 `StcWeighted` is all the buildup's density+thickness data supports — so the `RequiredRw` ISO demand judges the single-material path against `Rw` and the assembly path against its mass-law `StcWeighted`, never an ISO demand against an ASTM rating on the same path.
- Auto: the assembly weighted index is the aggregator's `StcWeighted` (the layered mass-law per-band SRI folded once through the seam `RatingContour.Stc.Fit` ASTM E413 contour kernel — the assembly carrier exposes NO `Rw`, the mass-law estimate is all the buildup's areal-mass data supports); the single-material `Nrc` and `Rw` read the seam `MaterialPropertySet.Acoustic` projections directly off the `PrimaryMaterial` (the seam `Acoustic` carrier's `RatingContour.Rw.Fit` ISO 717-1 contour over its measured SRI spectrum, the sibling of `StcWeighted` differing only by the contour row, never recomputed here); both contour rows share one `RatingContour` owner; the single-number rating is a dimensionless dB-weighted `MeasureValue`.
- Packages: LanguageExt.Core, Rasm.Element (project — `MaterialComposition`, `MaterialPropertySet.Acoustic` via `MaterialPropertyAccess`, `MeasureValue`, `Dimension`, `NodeId`), the `Analysis/aggregator` `AssemblyAggregator`, BCL inbox.
- Growth: a new acoustic rating (impact `Ln,w`, flanking `Dn,f,w`, the ISO 717-1 `C`/`Ctr` spectrum-adaptation terms) is one fold over the same `LayerSet` reading the same per-band SRI, never a parallel acoustic owner; the acceptance verdict is now real — `AssessmentRequest.Acoustic` carries a `RequiredRw` the governing ratio reads as `RequiredRw / Rw`.
- Boundary: the multi-ply weighted index composes `AssemblyAggregator.Aggregate` so the layered sound reduction and the contour fit are the seam-owned `RatingContour.Stc.Fit` ASTM E413 mass-law estimate (the assembly carrier models no `Rw`), never a second STC/`Rw` algorithm; the single-material `Nrc`/`Rw` are the seam intrinsic folds read off the `Acoustic` case (the ISO 717-1 `RatingContour.Rw.Fit` over the measured SRI spectrum — judged against the ISO `RequiredRw` demand, the matched-standard pairing the assembly's mass-law `StcWeighted` cannot offer), never recomputed here; the single-material branch resolves the material through the composition's `PrimaryMaterial` (`graph.Material(MaterialId)`), never the element `NodeId` (an element node is not a material node); a target with no acoustic property reports the `absent` text fact rather than a fabricated rating; the `AssessmentRequest.Acoustic` case carries a `RequiredRw` acceptance target, so the governing ratio is `RequiredRw / Rw` (a higher Rw is better — required-over-achieved, the same orientation the fire `RequiredMinutes / achieved` takes) and the verdict is a genuine pass/fail, while a `RequiredRw <= 0` request reverts to the informational rating (governing `double.NaN`, which `Math.Max` propagates across the multi-target fold → `NotApplicable`, the no-target convention the energy, carbon, and cost runners hold — never a misleading `0.0`-ratio `Satisfied` asserting a pass the run never checked).

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BuildingPhysics {
    public static Fin<AssessmentResult> RunAcoustic(ElementGraph graph, AssessmentRequest.Acoustic request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Governing: 0.0)),
            (acc, id) => acc.Bind(state => graph.CompositionOf(id).ToFin(Missing($"<acoustic-element-missing-composition:{id.Value}>"))
                .Bind(composition => composition is MaterialComposition.LayerSet set
                    ? AssemblyAggregator.Aggregate(set, Resolver(graph)).Map(property => RateAcoustic(id, property.StcWeighted, None, request, state))
                    : graph.Material(composition.PrimaryMaterial).Map(static m => m.Properties).ToFin(Missing($"<acoustic-material-absent:{id.Value}>"))
                        .Map(props => props.Acoustic.Match(
                            Some: a => RateAcoustic(id, a.Rw, Some(a.Nrc), request, state),   // ISO 717-1 Rw (the SEAM Acoustic carrier's RatingContour.Rw.Fit over the measured SRI spectrum) — judged against the ISO RequiredRw demand, NOT the ASTM E413 StcWeighted contour the assembly mass-law estimate yields
                            None: () => (state.Facts.Add(AssessmentFact.Text($"{id.Value}/acoustic", "absent")), state.Governing))))))
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
    static (Seq<AssessmentFact> Facts, double Governing) RateAcoustic(NodeId id, int rating, Option<double> nrc, AssessmentRequest.Acoustic request, (Seq<AssessmentFact> Facts, double Governing) state) {
        Seq<AssessmentFact> facts = state.Facts.Add(AssessmentFact.Measure($"{id.Value}/sound-reduction-index", new MeasureValue(QuantityType.OfDimension(Dimension.Dimensionless), Dimension.Dimensionless, rating, "dB")));
        facts = nrc.Match(Some: n => facts.Add(AssessmentFact.Ratio($"{id.Value}/nrc", n)), None: () => facts);
        double ratio = request.RequiredRw > 0.0 ? request.RequiredRw / Math.Max(rating, double.Epsilon) : double.NaN;
        return (facts, Math.Max(state.Governing, ratio));
    }
}
```

## [04]-[FIRE_RESISTANCE]

- Owner: `BuildingPhysics.RunFire` the fire runner; `FireExposure` the `[SmartEnum<string>]` exposure model carrying the exposed-side count, the convection coefficient `α_c`, and the nominal gas-temperature-time curve delegate; `SteelFireState` the incremental steel-temperature march receipt; the EN 1993-1-2 critical-temperature fold and the EN 1992-1-2 concrete tabulated minimum-dimension check dispatched by the fire route.
- Entry: `public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, ClockPolicy clocks)` — resolves each member's `SectionProperties` off its `ProfileSet` composition, dispatches the `en1993-1-2` steel critical-temperature march and the `en1992-1-2` concrete tabulated check, emits the `fire-resistance-minutes`/`critical-temperature`/`section-factor` (steel) and `required-min-dimension`/`least-dimension`/`required-axis-distance`/`axis-distance` (concrete) facts, and threads `max(RequiredMinutes / achieved)` through the accumulator (the concrete `achieved` the WORSE of the dimension- and axis-distance-governed resistances per the member-type EN 1992-1-2 table).
- Auto: the steel fold marches the exposure's gas curve `θ_g(t)` and the net heat flux `ḣ = α_c·(θ_g−θ_a) + ε·σ·((θ_g+273)⁴−(θ_a+273)⁴)` over the section factor `Am/V` at a 5 s step with the EN 1993-1-2 temperature-dependent specific heat `c_a(θ_a)` and the I-section shadow factor `k_sh`, finds the time the steel temperature reaches the critical temperature `θ_a,cr = 39.19·ln(1/(0.9674·μ0^3.833)−1) + 482` for the degree of utilization `μ0`, and the achieved resistance is that time; the concrete fold reads the EN 1992-1-2 member-type table (column/beam/slab/wall, keyed off the `Object` node `Classification.Code`) for the required rating's `(min dimension, min axis distance)` pair and checks BOTH against the section's `LeastDimension` and `AxisDistance` cover, the achieved resistance the worse-governed of the two criteria.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]` gas-curve rows), Rasm.Element (project — `SectionProperties` incl. `AxisDistance`, the seam `ElementGraph.SectionOf` Op-free section accessor, `Node.Object` for the `Classification.Code` member-type read, `MeasureValue`, `Dimension`, `NodeId`), BCL inbox (`Math`, `FrozenDictionary`).
- Growth: a new fire model (a parametric EN 1991-1-2 natural fire, an EN 1995-1-2 timber charring rate, an EN 1993-1-2 PROTECTED-steel march with an insulation `λ_p`/`d_p` term) is one route plus one fold plus one explicit dispatch arm reading the same section (the two-route fire dispatch rails an unrecognized route — `<fire-route-unhandled>` — until its arm lands, never silently charging a timber member against the concrete table); a new exposure is one `FireExposure` row carrying its gas-curve delegate and convection coefficient; the concrete axis-distance (cover-to-rebar) criterion and the member-type-specific (column/beam/slab/wall) tables are now LIVE (the seam `SectionProperties.AxisDistance` cover column + the `Object` `Classification.Code` member type), the 500 °C isotherm method deepening as a fold over the section thermal field where the tabulated check is insufficient.
- Boundary: the section factor `Am/V` reads the seam `SectionProperties.HeatedPerimeter`/`Area` so fire and ambient design share one section source; the section is resolved through the seam's Op-free `SectionOf` accessor (the M7-baked `SectionProperties` read off the member's `ProfileSet` composition), never re-resolving a `ProfileRef` or admitting VividOrange; the degree of utilization `μ0` is the ambient governing ratio carried on the request (the structural check feeds it), never re-solved here; the steel march is a genuine incremental integration with EN 1993-1-2 temperature-dependent specific heat and exposure-dependent convection — never a tabulated approximation and never a fixed heat capacity — and the receipt resistance is the marched time; the concrete check is the FULL EN 1992-1-2 tabulated method — the member-type `(min dimension, min axis distance)` pair (keyed off the `Object` `Classification.Code` column/beam/slab/wall table) checked against BOTH the section's `LeastDimension` and its `AxisDistance` cover, the achieved resistance the worse-governed of the two so a thin-cover section governs; a member with no `ProfileSet` section rails the typed input fault; the governing ratio is `RequiredMinutes / achieved` threaded through the accumulator so an under-resistant member governs, never a re-parse of the emitted facts; every measured fact is SI-native (`Dimension.DurationDim` seconds, `Dimension.LengthDim` metres, the temperature/per-length raw `MeasureValue`).

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

    // EN 1992-1-2 tabulated method: per member type, the (rating minutes, min cross-section dimension, min axis distance
    // to reinforcement) rows — Table 5.2a (braced columns, mu_fi<=0.7, more than one side exposed), 5.4 (load-bearing
    // walls, one side exposed), 5.5 (simply-supported beams), 5.8 (simply-supported one-way slabs). BOTH the minimum
    // dimension AND the axis distance gate the rating; the runner reads the member type off the Object node
    // Classification.Code and the axis distance off the seam SectionProperties.AxisDistance cover column.
    static readonly FrozenDictionary<string, (double Minutes, double MinDimM, double AxisDistanceM)[]> ConcreteFireTable =
        new Dictionary<string, (double, double, double)[]> {
            ["IfcColumn"] = [(30.0, 0.200, 0.025), (60.0, 0.250, 0.046), (90.0, 0.350, 0.053), (120.0, 0.350, 0.057), (180.0, 0.450, 0.070), (240.0, 0.450, 0.075)],
            ["IfcBeam"]   = [(30.0, 0.080, 0.025), (60.0, 0.120, 0.040), (90.0, 0.150, 0.055), (120.0, 0.200, 0.065), (180.0, 0.240, 0.080), (240.0, 0.280, 0.090)],
            ["IfcSlab"]   = [(30.0, 0.060, 0.010), (60.0, 0.080, 0.020), (90.0, 0.100, 0.030), (120.0, 0.120, 0.040), (180.0, 0.150, 0.055), (240.0, 0.175, 0.065)],
            ["IfcWall"]   = [(30.0, 0.100, 0.010), (60.0, 0.110, 0.010), (90.0, 0.120, 0.020), (120.0, 0.150, 0.025), (180.0, 0.180, 0.040), (240.0, 0.230, 0.055)],
        }.ToFrozenDictionary();

    // Explicit dispatch over the TWO fire routes — an unrecognized fire route rails the typed fault rather than
    // defaulting to the concrete arm, so the EN 1995-1-2 timber-charring (and EN 1991-1-2 parametric) growth lands as
    // one added dispatch arm broken loudly, never a timber member silently charged against the concrete table. (A
    // generated total Switch would need a fire-model discriminant on AssessmentRoute — the cross-stratum refinement.)
    public static Fin<AssessmentResult> RunFire(ElementGraph graph, AssessmentRequest.Fire request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Governing: 0.0)),
            (acc, id) => acc.Bind(state => MemberSection(graph, id).Bind(section =>
                request.Route == AssessmentRoute.En1993Fire
                    ? Fin.Succ(SteelFire(section, request, id, state))
                    : request.Route == AssessmentRoute.En1992Fire
                        ? Fin.Succ(ConcreteFire(section, MemberClass(graph, id), request, id, state))
                        : Fin.Fail<(Seq<AssessmentFact> Facts, double Governing)>(Missing($"<fire-route-unhandled:{request.Route.Key}>")))))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts, state.Governing,
                new Provenance("BuildingPhysics", request.Route.Standard, "closed-form", clocks.Now)));

    // The member's IFC class (IfcColumn/IfcBeam/IfcSlab/IfcWall) off its Object node Classification.Code — the EN 1992-1-2
    // member-type table selector (the seam Classification.Code is the IFC entity class, not the PredefinedType sub-type);
    // an absent Object or unrecognised class reads the conservative beam table.
    static string MemberClass(ElementGraph graph, NodeId id) => graph.Find<Node.Object>(id).Map(static o => o.Classification.Code).IfNone("");

    // The member section off the seam's Op-free SectionOf accessor (the M7-baked neutral SectionProperties read directly
    // off the member's ProfileSet composition, no Bake): a member with no resolved profile section rails the typed fault,
    // never a defaulted section — the seam OWNS the section read (Graph/element#ELEMENT_GRAPH SectionOf), so this runner
    // composes it one-hop rather than re-deriving the composition match locally, the same read Analysis/structural takes.
    static Fin<SectionProperties> MemberSection(ElementGraph graph, NodeId id) =>
        graph.SectionOf(id).ToFin(Missing($"<fire-member-section-unresolved:{id.Value}>"));

    static (Seq<AssessmentFact> Facts, double Governing) SteelFire(SectionProperties section, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) {
        double sectionFactor = section.HeatedPerimeter.Si / Math.Max(section.Area.Si, double.Epsilon);
        double criticalTempC = CriticalTemperature(request.Utilization);
        double cap = request.RequiredMinutes + CapMarginMinutes;
        SteelFireState march = March(request.Exposure, sectionFactor, criticalTempC, cap);
        double achieved = march.SteelTempC >= criticalTempC ? march.Minutes : cap;
        return (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/fire-resistance-minutes", MeasureValue.OfSi(Dimension.DurationDim, achieved * 60.0)))
                .Add(AssessmentFact.Measure($"{id.Value}/critical-temperature", new MeasureValue(QuantityType.OfDimension(TemperatureDim), TemperatureDim, criticalTempC + 273.15, "K")))
                .Add(AssessmentFact.Measure($"{id.Value}/section-factor", new MeasureValue(QuantityType.OfDimension(PerLengthDim), PerLengthDim, sectionFactor, "1/m"))),
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

    static (Seq<AssessmentFact> Facts, double Governing) ConcreteFire(SectionProperties section, string memberClass, AssessmentRequest.Fire request, NodeId id, (Seq<AssessmentFact> Facts, double Governing) state) {
        (double MinDimM, double AxisDistanceM) limits = ConcreteFireLimits(memberClass, request.RequiredMinutes);
        double leastM = section.LeastDimension.Si, axisM = section.AxisDistance.Si;
        // The full EN 1992-1-2 tabulated method: BOTH the minimum cross-section dimension AND the axis distance
        // (cover-to-reinforcement) must meet the required-rating row, so the achieved resistance is the WORSE-governed of
        // the two — an otherwise-adequate section with thin cover still governs, the GOVERNING criterion the prior
        // minimum-dimension-only check could not reach.
        double dimAchieved  = leastM >= limits.MinDimM ? request.RequiredMinutes : request.RequiredMinutes * leastM / Math.Max(limits.MinDimM, double.Epsilon);
        double axisAchieved = axisM >= limits.AxisDistanceM ? request.RequiredMinutes : request.RequiredMinutes * axisM / Math.Max(limits.AxisDistanceM, double.Epsilon);
        double achieved = Math.Min(dimAchieved, axisAchieved);
        return (Facts: state.Facts
                .Add(AssessmentFact.Measure($"{id.Value}/fire-resistance-minutes", MeasureValue.OfSi(Dimension.DurationDim, achieved * 60.0)))
                .Add(AssessmentFact.Measure($"{id.Value}/required-min-dimension", MeasureValue.OfSi(Dimension.LengthDim, limits.MinDimM)))
                .Add(AssessmentFact.Measure($"{id.Value}/least-dimension", MeasureValue.OfSi(Dimension.LengthDim, leastM)))
                .Add(AssessmentFact.Measure($"{id.Value}/required-axis-distance", MeasureValue.OfSi(Dimension.LengthDim, limits.AxisDistanceM)))
                .Add(AssessmentFact.Measure($"{id.Value}/axis-distance", MeasureValue.OfSi(Dimension.LengthDim, axisM))),
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

- [ISO_6946_AND_13788]: the steady-state `U` is the ISO 6946 series resistance read from `AssemblyAggregator.Aggregate` (the relocated engine owns the `Rsi`/`Rse` films and the `Σt/λ` fold), and the EN ISO 13788 Glaser model is the steady-state interstitial-condensation check this runner owns over the per-layer resistances — the interface temperature follows the cumulative thermal resistance, the saturation vapour pressure the Magnus form, the actual vapour pressure the cumulative vapour resistance `Z = Σ(μ·t)/δ0`, and the condensation amount is the EN ISO 13788 graphical method realized as the LOWER CONVEX HULL of the `(Z, p)` points: the interior hull vertices are the condensation planes and the condensation mass rate `g_c` at each is the inflow-minus-outflow vapour-flux discontinuity between adjacent hull vertices, so the runner reports a real accumulation rate (kg·m⁻²·s⁻¹) and the worst-interface vapour-pressure utilization rather than a bare crossing flag. The Glaser fold reads the seam `MaterialPropertySet.Thermal.Conductivity` and `VapourResistanceFactor` through the `MaterialPropertyAccess.Thermal` accessor; the boundary climate is the request scenario. Ripple counterpart: `Rasm.Element/Composition/material` (the seam `Thermal` case carrying the `VapourResistanceFactor` μ) and `Rasm.Compute/Analysis/aggregator` (the ISO 6946 series-`U` + EN ISO 13788 `Sd` owner this runner reads the total `U` from). A 12-month condensation/evaporation balance is the EN ISO 13788 annual extension once `BoundaryClimate` carries a monthly climate series — one fold over the per-month profile, never a parallel owner.
- [EN_10077_WHOLE_WINDOW]: a window's transmittance is NOT its glazing's center-of-glass `Ug` — it is the EN ISO 10077-1 area-and-perimeter-weighted whole-window `Uw = (Σ Ag·Ug + Σ Af·Uf + Σ lg·Ψg)/(Σ Ag + Σ Af)`, the side-by-side glazing-in-frame composition the through-thickness ISO 6946 series-U cannot reach. The runner resolves the window's `Compose` parts Op-free (the glazing-infill part's `Ug` and the frame part's `Uf` each off `graph.PropertiesOf(part).Thermal.UValue.Si` — the IGU `Ug` `Rasm.Materials` `Profiles/glazing` `GlazingSection.Performance` computes and `ToProperties` lowers onto `MaterialPropertySet.Thermal.UValue`), reads the glazed area `Ag` + total area off the window's `Qto_*BaseQuantities` (the frame area `Af = Area − GlazingArea`), the visible-glazing edge length `lg` off the `GlazingPerimeter` quantity, and the spacer `Ψg` off the window's `Pset` thermal-bridge property (where `GlazingSection`'s `SpacerType.PsiWmK` warm-edge `0.04`/cold-edge `0.11 W·m⁻¹·K⁻¹` lowers — the seam `MaterialPropertySet.Thermal` carries `Conductivity`/`SpecificHeat`/`UValue`/`VapourResistanceFactor` only, no perimeter-bridge column, so a `Ψ`-on-`Thermal` read is the phantom this runner never takes), assembles the glazed/frame `WindowField` set, and composes `AssemblyAggregator.AggregateWindow` for the `Uw` no single material carries. The Qto `GlazingArea` is the window discriminant (present iff the element is a glazed window/door) so a non-window target falls through to the envelope path unchanged. The full EN ISO 10077-2 numerical frame thermal model (a per-frame-profile measured `Uf`/`Ψg`) is a richer `WindowField` resolution the runner supplies when a frame thermal model is admitted, never a parallel window owner. Ripple counterpart: `Rasm.Compute/Analysis/aggregator` (the `AssemblyAggregator.AggregateWindow` EN ISO 10077-1 owner + the `WindowField`/`WindowU` shapes this runner composes), `Rasm.Materials/Profiles/glazing` (the `GlazingSection.Performance` `Ug` + `SpacerType.PsiWmK` — whole-window `Uw` explicitly the Compute assembly concern), and `Rasm.Element/Composition/material` (the seam `Thermal` case carrying the glazing `Ug` on `UValue`, the spacer `Ψg` riding a `Pset`).
- [ISO_12354_AND_717]: the layered airborne sound-reduction index is the ISO 12354-1 series fold read from `AssemblyAggregator.Aggregate`, whose per-band SRI rides the SEAM `Composition/acoustic#ACOUSTIC_FOLDS` `RatingContour.Stc.Fit` so the weighted single number and the single-material rating share one ASTM E413 / ISO 717 contour owner — a second contour algorithm is the named defect, so the runner emits the aggregator's `StcWeighted` directly and never recomputes the contour. The single-material `Nrc`/`Rw` are the seam intrinsic folds read off the `Acoustic` case via the composition `PrimaryMaterial` (the `RatingContour.Rw.Fit` ISO 717-1 contour over the material's measured SRI spectrum, the matched standard for the ISO `RequiredRw` demand — the assembly's mass-law `StcWeighted` ASTM E413 estimate is the deliberate asymmetry the buildup's areal-mass-only data supports), never recomputed; the `C`/`Ctr` spectrum-adaptation terms, the impact `Ln,w`, and the flanking `Dn,f,w` deepen as additional folds over the same per-band SRI. The `AssessmentRequest.Acoustic` case carries a `RequiredRw` acceptance target, so the governing ratio is `RequiredRw / Rw` (a higher Rw is better — required-over-achieved) and a real pass/fail verdict bands through `AssessmentVerdict.FromRatio`; a `RequiredRw <= 0` request reverts to the informational rating (governing `double.NaN` → `NotApplicable`, never a misleading `0.0`-ratio `Satisfied`). Ripple counterpart: `Rasm.Element/Composition/acoustic` (the seam `RatingContour` contour family with its public `Stc.Fit`/`Rw.Fit` kernel + the `Nrc`/`Saa` intrinsic folds) and `Rasm.Compute/Analysis/assessment` (the `AssessmentRequest.Acoustic` case carrying the `RequiredRw` acceptance target + its `CanonicalBytes` contribution).
- [EN_1993_1_2_STEEL_FIRE]: the steel fire resistance is the EN 1993-1-2 critical-temperature method — the unprotected-steel temperature march `Δθa = k_sh·(Am/V)/(c_a·ρa)·ḣnet·Δt` over the exposure's gas curve (ISO 834 standard, the hydrocarbon curve, or the EN 1991-1-2 external curve, each a per-row delegate on `FireExposure`), the net heat flux the convective `α_c·(θg−θa)` (the convection coefficient `α_c` a `FireExposure` column, 25 standard/external and 50 hydrocarbon) plus the radiative `ε·σ·((θg+273)⁴−(θa+273)⁴)` term, integrated at a 5 s step with the EN 1993-1-2 §3.4.1.2 temperature-dependent specific heat `c_a(θa)` (the 735 °C latent-heat singularity included) to the critical temperature `θa,cr = 39.19·ln(1/(0.9674·μ0^3.833)−1) + 482` for the degree of utilization `μ0` (the ambient governing ratio carried on the request, clamped to the EN validity floor `μ0 ≥ 0.013`). The section factor `Am/V` reads the seam `SectionProperties.HeatedPerimeter`/`Area`, so fire and ambient design share one section. This is a genuine incremental integration, never a tabulated approximation; the I-section shadow factor `k_sh = 0.9` refines to `0.9·[Am/V]_box/[Am/V]` once a boxed section factor rides `SectionProperties`, and a protected-steel march lands as one fold adding the insulation `λ_p`/`d_p` term.
- [EN_1992_1_2_CONCRETE_FIRE]: the concrete fire resistance is the FULL EN 1992-1-2 tabulated method — the member-type `(min cross-section dimension, min axis distance)` pair for the required rating, a member-type-keyed frozen-table lookup (column/beam/slab/wall, Tables 5.2a/5.4/5.5/5.8) read against the seam `SectionProperties.LeastDimension` AND the `AxisDistance` cover, BOTH criteria gating the rating (the achieved resistance the worse-governed of the two, so the GOVERNING axis-distance/cover criterion the prior minimum-dimension-only check could not reach now governs a thin-cover section); the member type reads off the `Object` node `Classification.Code` (the IFC class IfcColumn/IfcBeam/IfcSlab/IfcWall, not the `PredefinedType` sub-type), and the 500 °C isotherm method deepens as a fold over the section thermal field where the tabulated check is insufficient. The two fire models dispatch by route (`en1993-1-2` / `en1992-1-2`) on one `RunFire` kernel, never a parallel steel/concrete fire owner; an unrecognized fire route rails `AssessmentInputMissing` (`<fire-route-unhandled>`) rather than defaulting to the concrete arm, so the EN 1995-1-2 timber-charring growth lands as one explicit dispatch arm broken loudly.
- [DISCIPLINE_COLLAPSE]: thermal, acoustic, and fire collapse onto ONE `BuildingPhysics` kernel because all three are closed-form ISO/EN folds over an assembly/section read from the concrete graph — distinct from the `Analysis/energy` simulation rail (which builds an OpenStudio model and runs the EnergyPlus subprocess) and the `Analysis/structural` FE rail (which assembles and solves a frame). The shared shape — read the seam composition/section through the `MaterialId`-keyed resolver, fold the ISO/EN closed form, thread the governing ratio through the accumulator, emit one SI-native `AssessmentResult` fact stream — is the `COLLAPSE_SCAN` collapse of three parallel runner types into one `Discipline`-dispatched owner. The verdict derives from the in-scope governing quantity threaded through the fold (`U / U_target` and the vapour utilization for thermal, `RequiredMinutes / achieved` for fire), never a re-parse of the emitted facts and never a sentinel ratio; every measured fact is SI-native through `MeasureValue.OfSi(Dimension, si)` or the raw `MeasureValue` record for a domain-labelled scalar, never the phantom 2-arg `MeasureValue.Of(value, unit)` the seam factory does not expose. Each runner composes the `Analysis/aggregator` for the multi-ply rollup where a layered property is needed.
