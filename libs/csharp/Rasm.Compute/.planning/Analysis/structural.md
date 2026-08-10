# [COMPUTE_STRUCTURAL]

Rasm.Compute structural-analysis runner owns the `Discipline.Structural` arm of the `Analysis/assessment` spine. It reads the concrete `Rasm.Element` `ElementGraph` directly, folds member axes, the M7-resolved `SectionProperties`, the seam `Mechanical` strengths, and the projected structural edges into one `FrameModel` idealization, solves it over the owned frame spine, recovers the per-combination `MemberResponse` extremes, checks each member through the `(DesignCode, LimitState)` capacity table — every structural family carrying both its US and its Eurocode route — and returns the governing utilization as one `AssessmentResult` fact stream.

Frame assembly enters the shared `SolveLane`: `SolvePolicy.CanonicalStatic` selects `FactorKind.Spd` through `SparseOps.Factor`, and the seismic route runs `SolvePolicy.CanonicalModalCondensed`, condensing the frame's inertia-free rotational rows out of the pencil onto the lane's dense generalized `Evd`. Buckling, lateral-torsional buckling, and deflection read the member's unbraced length, end-fixity-derived effective-length factor, and FE displacement extremes.

## [01]-[INDEX]

- [02]-[FRAME_MODEL]: `FrameModel` folds the graph into the analysis idealization its load, support, combination, and policy vocabulary carries.
- [03]-[FRAME_BACKEND]: `Solve` lowers that model onto the owned frame spine and bounds one per-combination `MemberResponse` every limit state reads.
- [04]-[DESIGN_CHECK]: `StructuralAnalysis.Run` checks each member through the `(DesignCode, LimitState)` capacity table and folds one governing-utilization fact stream.
- [05]-[SEISMIC_ROUTE]: `Run` over `AssessmentRequest.Seismic` folds the sparse modal response against a `DesignSpectrum` row under a typed per-axis participation floor.

## [02]-[FRAME_MODEL]

- Owner: `FrameModel` the analysis idealization (members, combinations, policy);  `MemberLoad` the per-member applied-action `[Union]` (`Point`/`Uniform`/`Trapezoid`); `DofRestraint` the per-degree-of-freedom restraint reading `[Union]` (`Free`/`Rigid`/`Spring`); `StructuralCase` the load-case `[SmartEnum<string>]`; `MemberSupport` the 6-DOF restraint at a member end and `SupportFrame` its skewed orientation basis; `LoadCombinationSpec` the factored case map; `StructuralPolicy` the formulation/deflection/station policy carrying the `Formulation` frame `ElementClass` column (`Analysis/assessment` content-keys its `Key`) and the EN 1992 member-scope `StirrupSpacing`/`CotTheta` truss inputs; `StructuralMember` the resolved member (axis, section, strength, family, loads, supports); `WindExposureClass`/`LiveLoadClass` the ASCE 7 exposure-profile and live-load vocabularies; `SiteActionPolicy` the per-engagement wind/snow/live-load code-parameter record; `ActionDerivation` the load-takedown table minting live/wind/snow `MemberLoad` actions from member geometry and the site policy; `FrameInputs` the projection shape both structural request cases supply, so one projector serves the static and the seismic route off one overload set. `StructuralCase`/`LoadCombinationSpec`/`StructuralPolicy`/`SiteActionPolicy`/`SeismicSpec` are the seam contract `AssessmentRequest.Structural`/`.Seismic` carry and `Analysis/assessment` `CanonicalBytes` folds — their field shape is load-bearing across the spine, while `MemberSupport` and `MemberLoad` are read off the graph per run and are this page's shape alone.
- Entry: `static Fin<FrameModel> Project(ElementGraph graph, FrameInputs inputs, GeometrySource geometry)` — folds the input `Targets` member `Node.Object`s into the idealization, reading each member's `StructuralReads.AxisOf` (the analytical line resolved one-hop by content key through the seam `GeometrySource` port off `member.Representations.Axis`), `graph.PropertiesOf(id).Mechanical` (the seam strength read), `graph.SectionOf` (the seam Op-free M7 accessor reading the baked `ProfileSet` section directly — the seam owns the section read, so the runner never re-derives a discipline-local section accessor), `StructuralReads.SupportsOf`, and `StructuralReads.LoadsOf`, `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a member lacks a section, a strength, or an axis.
- Auto: self-weight derives per member from `Section.Area.Si × Mechanical.Density.Si × StandardGravity` as a global-down `Uniform` force-per-length in the `Dead` case; the request's projected `MemberLoad`s supply the applied live/wind/snow/seismic actions, and where a variant carries none `ActionDerivation.Derive` mints them from tributary geometry under one `SiteActionPolicy` — ASCE 7 velocity pressure `qz = 0.613·Kz·Kzt·Kd·V²` at the member's mean height for wind, `pf = 0.7·Ce·Ct·Is·pg` with the slope factor for roof snow, the `LiveLoadClass` row for floor live — so a generated design screens without a human load engineer per variant, exactly the derivation precedent the seismic `DesignSpectrum` rows set; `LoadCombinationSpec` factors the cases per code (ASCE 7 / EN 1990) so a combination is data the backend reads, never a re-modelled load set; the member's `MaterialFamily` is `Classify`-derived off the seam evidence — the realized `Orthotropic` case naming a directional material outright, the constitutive modulus band the residual — and validated against the code's `DesignCode.Family` at `Check`.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`), Rasm.Element (project — `ElementGraph`, `Node`, `NodeId`, the seam-owned host-neutral `Vector3` coordinate the `AxisCurve` carries and the load vectors reuse, `AxisCurve`, `GeometrySource` the analytical-line resolution port, `RepresentationContentHash`, `SectionProperties`, `MaterialPropertySet`, `Relationship`, `PropertyName`, `PropertyValue`, `MeasureValue`, and `StructuralRows` the owner-declared structural row vocabulary every edge read keys through), BCL inbox (`FrozenDictionary`).
- Growth: a new applied-action kind is one `MemberLoad` case (both backends widen their total load `Switch`); a new restraint reading is one `DofRestraint` case every lowering's total dispatch absorbs, and a new per-support fact one `MemberSupport` column; a new combination basis is one `LoadCombinationSpec` row; a new exposure or live-load category is one `WindExposureClass`/`LiveLoadClass` row, a new derived action family (EN 1991 wind/snow rows, drift surcharge, partition allowance) one weighted arm on `ActionDerivation.Derive` reading its `SiteActionPolicy` columns — the idealization widens by data, the backends and checks re-read it.
- Boundary: the section is the M7-resolved seam `SectionProperties` read once off the `ProfileSet` composition (the `VividOrange` `ProfileRef`→section resolution happens once in the `Rasm.Materials` projector, so this runner never re-resolves a profile and Compute admits no VividOrange); `SectionProperties` carries the both-axis shear areas `AvY`/`AvZ` and both-axis radii, so the per-axis shear check reads its own area. Strength is the seam `Mechanical.YieldStrength`/`UltimateStrength`/`YoungsModulus`/`ShearModulus`/`Density`/`PoissonsRatio` (the seam field is `PoissonsRatio`, never `PoissonRatio`) off the member's associated material; the analytical line is the seam `AxisCurve` (`Start`/`End`/`Up`) content-keyed under `member.Representations.Axis`, never inlined on the node, resolved one-hop through the seam `GeometrySource` port (coplanarity a `StructuralReads` `AxisCurve` fold, length the member's own `Vector3.Distance`). Supports and loads traverse the projected `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` neutral `Generic` edges by wire-name (the Bim projector stamping the 6-DOF restraint, applied components, end discriminant, and load kind), so the runner reads the idealization fully baked, never re-reading IFC; a member with no section/strength/axis rails the typed input fault. Every row name the reader keys resolves to a `Rasm.Element` `StructuralRows` static — a call-site `PropertyName.Create` here forks the bag's key space between the projector and this non-referencing reader — and the shape of the read follows the declarer: ONE row per degree of freedom whose `PropertyValue` case carries restraint-versus-spring, and ONE positional `StructuralRows.Frame` list of six direction ratios.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------

// Per-member applied action is one polymorphic load mapped through a total Switch: a Point at a
// span fraction, a Uniform force-per-length, a Trapezoid linearly varying end-to-end. A new action kind is one case.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MemberLoad {
    private MemberLoad() { }
    public sealed record Point(StructuralCase Case, Vector3 Force, Vector3 Moment, double Station) : MemberLoad;
    public sealed record Uniform(StructuralCase Case, Vector3 ForcePerLength) : MemberLoad;
    public sealed record Trapezoid(StructuralCase Case, Vector3 Start, Vector3 End) : MemberLoad;
}

// One restraint reading per degree of freedom, because the projector stamps ONE row per DOF and the PropertyValue
// CASE is the discriminant: a Boolean is the rigid-or-free fixity and a Measure the SI spring rate, so a DOF can
// never carry a fixity its own stiffness contradicts.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DofRestraint {
    private DofRestraint() { }
    public sealed record Free : DofRestraint;
    public sealed record Rigid : DofRestraint;
    public sealed record Spring(double RateSi) : DofRestraint;

    public static readonly DofRestraint Released = new Free();
    public static readonly DofRestraint Locked = new Rigid();

    // Of reads the seam row's own case: an absent row and a FALSE Boolean both resolve Free, a TRUE Boolean Rigid,
    // and a finite positive Measure the spring. Free also owns every reading that restrains nothing — a
    // non-positive or non-finite stiffness, and any other PropertyValue case, since no third case of the fourteen
    // spells a restraint — never a silently rigid support the assembler over-constrains. Finiteness guards at this
    // one admission rather than at every consumer that once re-spelled it.
    public static DofRestraint Of(Option<PropertyValue> row) => row.Match(
        Some: static value => value switch {
            PropertyValue.Boolean fixity => fixity.Value ? Locked : Released,
            PropertyValue.Measure spring when spring.Value.Si > 0.0 && double.IsFinite(spring.Value.Si) => new Spring(spring.Value.Si),
            _ => Released,
        },
        None: static () => Released);

    // Constrains gates the Dirichlet row: a finite TRANSLATIONAL spring lowers conservatively rigid (the named
    // lowering the frame rows carry no in-series translational condensation for), so both restraining cases
    // constrain the joint DOF and only Free leaves it open.
    public bool Constrains => Switch(free: static _ => false, rigid: static _ => true, spring: static _ => true);

    // Rate carries the finite ROTATIONAL end spring the FrameMember semi-rigid column consumes, Some on the sprung
    // reading alone — a rigid or free DOF carries None because its member-end attachment is itself rigid, so its
    // joint restraint rides the Dirichlet row rather than a rate.
    public Option<double> Rate => Switch<Option<double>>(
        free: static _ => None, rigid: static _ => None, spring: static spring => Some(spring.RateSi));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StructuralCase {
    public static readonly StructuralCase Dead    = new("dead");
    public static readonly StructuralCase Live    = new("live");
    public static readonly StructuralCase Snow    = new("snow");
    public static readonly StructuralCase Wind    = new("wind");
    public static readonly StructuralCase Seismic = new("seismic");
}

// Wind exposure rows carry the ASCE 7 power-law profile constants (Table 26.11-1); Kz derives per member height,
// never a stored per-member coefficient.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WindExposureClass {
    public static readonly WindExposureClass B = new("b", alpha: 7.0, gradientHeightM: 365.76);
    public static readonly WindExposureClass C = new("c", alpha: 9.5, gradientHeightM: 274.32);
    public static readonly WindExposureClass D = new("d", alpha: 11.5, gradientHeightM: 213.36);

    public double Alpha { get; }
    public double GradientHeightM { get; }

    public double Kz(double heightM) => 2.01 * Math.Pow(Math.Max(heightM, 4.6) / GradientHeightM, 2.0 / Alpha);
}

// Live-load rows carry the ASCE 7 Table 4.3-1 uniformly distributed live load in SI; a new category is one row. The
// name is the LOAD category, not the occupant-load one: the IBC Ch.10 occupant density the Analysis/circulation
// egress runner rates against is a different quantity under a different table, and one OccupancyClass spelling
// serving both puts two incompatible declarations of one name in one namespace.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LiveLoadClass {
    public static readonly LiveLoadClass Residential = new("residential", liveLoadPa: 1_920.0);
    public static readonly LiveLoadClass Office      = new("office",      liveLoadPa: 2_400.0);
    public static readonly LiveLoadClass Assembly    = new("assembly",    liveLoadPa: 4_790.0);
    public static readonly LiveLoadClass Storage     = new("storage",     liveLoadPa: 6_000.0);
    public static readonly LiveLoadClass Roof        = new("roof",        liveLoadPa: 960.0);

    public double LiveLoadPa { get; }
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    const double StandardGravity = 9.80665;             // m/s^2 — self-weight body acceleration
    const double Eps             = 1e-12;               // numeric floor for a capacity/length divisor
}

// --- [MODELS] ------------------------------------------------------------------------------
// SupportFrame carries the skewed-support orientation basis: the connection's ConditionCoordinateSystem Axis and
// RefDirection direction ratios, read off the ONE positional StructuralRows.Frame row. A global-axes placement emits
// no row at all, so absence IS the basis the assembler already assumes and no consumer resolves a half-built frame.
public readonly record struct SupportFrame(Vector3 Axis, Vector3 Ref);

// One support at a member END (AtStart selects the start vs end joint the Bim projector stamped from the connection
// geometry) — the FE assembler resolves the joint by the chosen endpoint coordinate, never by comparing the connection
// NodeId to the member's own id. At is the connection node the restraint sits on (carried for traceability). Each DOF
// is ONE DofRestraint reading off the projector's one-row-per-DOF wire, so fixity and spring rate cannot disagree.
// Owned spine consumes a finite ROTATIONAL end spring directly through the FrameMember semi-rigid columns (the exact
// in-series condensation the discretization rows carry), while a finite TRANSLATIONAL spring and a skewed Frame each
// lower conservatively (rigid / global axes) — both NAMED lowerings reading the same cases with zero wire edits when
// their rows land. A CLASS carrier, never a record struct: zero-init storage would mint null restraints past the one
// Of admission that exists to close them.
public sealed record MemberSupport(
    NodeId At, bool AtStart,
    DofRestraint Dx, DofRestraint Dy, DofRestraint Dz, DofRestraint Rx, DofRestraint Ry, DofRestraint Rz,
    Option<SupportFrame> Frame = default) {
    // Only a RIGID rotational triple reads as a fixed end for the effective-length factor: a sprung end is partial
    // restraint and folding it into "fixed" would under-length the column against its own compliance.
    public bool RotationallyFixed => Degrees.Skip(3).ForAll(static dof => dof is DofRestraint.Rigid);

    // Slot order IS the FE DOF order (ux, uy, uz, rx, ry, rz), so the Dirichlet fold indexes joint*6 + slot off this
    // ONE projection rather than re-spelling the six columns at every consumer.
    public Seq<DofRestraint> Degrees => Seq(Dx, Dy, Dz, Rx, Ry, Rz);
}

public sealed record LoadCombinationSpec(string Label, FrozenDictionary<StructuralCase, double> Factors) {
    // Seismic unit combination carries zero action factors because modal solve reads mass and stiffness, so the
    // lowering hands it a zero-action spec — the one static row the seismic route threads, never a caller obligation.
    public static readonly LoadCombinationSpec SeismicUnit = new("seismic-unit", FrozenDictionary<StructuralCase, double>.Empty);
}

// Structural policy carries frame formulation, serviceability, sampling, and RC shear inputs; AssessmentRequest.CanonicalBytes
// folds every field, while effective-length factor and unbraced length derive from member fixity and span.
// Formulation selects the owned frame ElementClass row and carries the backend discriminant with it.
// StirrupSpacing and CotTheta are the
// V_Rd,s member-scope inputs the EN 1992 truss pairing reads (the Materials capacity owner defers them by
// design — a section does not carry its stirrup spacing): spacing 0 marks the linkless arm, cot(θ) defaults
// 2.5 matching the Materials V_Rd,max ceiling so the pair is consistent by construction.
public sealed record StructuralPolicy(ElementClass Formulation, double DeflectionLimitRatio, int StationCount, double StirrupSpacing = 0.0, double CotTheta = 2.5) {
    public static readonly StructuralPolicy Canonical = new(ElementClass.Beam2Euler, DeflectionLimitRatio: 1.0 / 250.0, StationCount: 11);
}

public sealed record StructuralMember(
    NodeId Id, AxisCurve Axis, SectionProperties Section, MaterialPropertySet.Mechanical Strength,
    Option<MaterialPropertySet.Orthotropic> Directional, MaterialFamily Family, Seq<MemberLoad> Loads, Seq<MemberSupport> Supports,
    Option<RcShearLink> ShearLink = default) {
    public double Length => Vector3.Distance(Axis.Start, Axis.End);

    // K from the end-fixity the supports declare: both ends rotationally fixed -> 0.5, one fixed -> 0.7, a single
    // (cantilever) support -> 2.0, otherwise the pinned-pinned 1.0 — the slenderness divisor buckling/LTB read.
    public double EffectiveLengthFactor {
        get {
            int fixedEnds = Supports.Count(static s => s.RotationallyFixed);
            return Supports.Count == 1 && fixedEnds == 1 ? 2.0 : fixedEnds >= 2 ? 0.5 : fixedEnds == 1 ? 0.7 : 1.0;
        }
    }
}

public sealed record FrameModel(Seq<StructuralMember> Members, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy, double JointTolerance) {
    public bool Planar => Members.IsEmpty || Members.ForAll(m => m.Axis.Coplanar(Members.Head.Axis.Start.Z));
}

// Site action policy: the code parameters a load takedown reads — basic wind speed, exposure, topographic and
// directionality factors, the net pressure coefficient, ground snow with its exposure/thermal/importance chain,
// and the governing live-load category. One policy per engagement; per-variant geometry supplies the rest.
public sealed record SiteActionPolicy(
    double BasicWindSpeedMPerS, WindExposureClass Exposure, double Kzt, double Kd, double GcpNet,
    double GroundSnowPa, double Ce, double Ct, double SnowImportance, double RoofSlopeFactor,
    LiveLoadClass LiveLoad, double TributaryWidthM, double RoofBandM) {
    public static readonly SiteActionPolicy Canonical = new(
        BasicWindSpeedMPerS: 51.0, WindExposureClass.C, Kzt: 1.0, Kd: 0.85, GcpNet: 0.8,
        GroundSnowPa: 1_000.0, Ce: 1.0, Ct: 1.0, SnowImportance: 1.0, RoofSlopeFactor: 1.0,
        LiveLoadClass.Office, TributaryWidthM: 3.0, RoofBandM: 0.5);

    public bool Invalid => BasicWindSpeedMPerS <= 0.0 || Kzt <= 0.0 || Kd <= 0.0 || GcpNet <= 0.0
        || GroundSnowPa < 0.0 || Ce <= 0.0 || Ct <= 0.0 || SnowImportance <= 0.0 || RoofSlopeFactor is <= 0.0 or > 1.0
        || TributaryWidthM <= 0.0 || RoofBandM < 0.0 || !double.IsFinite(TributaryWidthM) || !double.IsFinite(RoofBandM)
        || !double.IsFinite(BasicWindSpeedMPerS) || !double.IsFinite(GroundSnowPa);

    // ASCE 7 velocity pressure qz = 0.613·Kz·Kzt·Kd·V² (SI, Pa) at the member's mean height.
    public double VelocityPressurePa(double heightM) =>
        0.613 * Exposure.Kz(heightM) * Kzt * Kd * BasicWindSpeedMPerS * BasicWindSpeedMPerS;

    // ASCE 7 flat-roof snow pf = 0.7·Ce·Ct·Is·pg, sloped through the slope shape factor Cs.
    public double RoofSnowPa => 0.7 * Ce * Ct * SnowImportance * GroundSnowPa * RoofSlopeFactor;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// Load takedown as a derivation table: geometry plus one site policy mint the live/wind/snow Uniform actions the
// seam would otherwise demand hand-computed upstream per variant — the same derivation precedent the seismic
// DesignSpectrum rows set for their case. Tributary width is the member's load-collection strip; a horizontal
// member takes the gravity actions (live on floor members, snow on roof members), every member takes the wind
// pressure over its exposed strip at its mean height.
public static class ActionDerivation {
    const double HorizontalCosine = 0.9;                 // |cos| floor above which a member axis reads as gravity-collecting

    public static Fin<Seq<MemberLoad>> Derive(StructuralMember member, double tributaryWidthM, bool roofMember, SiteActionPolicy site) {
        if (site.Invalid || !double.IsFinite(tributaryWidthM) || tributaryWidthM <= 0.0 || member.Length <= 0.0) {
            return Fin.Fail<Seq<MemberLoad>>(new ComputeFault.AssessmentInputMissing($"<action-derivation-invalid:{member.Id.Value}>"));
        }
        double meanHeight = 0.5 * (member.Axis.Start.Z + member.Axis.End.Z);
        double run = Math.Sqrt(Math.Pow(member.Axis.End.X - member.Axis.Start.X, 2.0) + Math.Pow(member.Axis.End.Y - member.Axis.Start.Y, 2.0));
        bool horizontal = run / member.Length >= HorizontalCosine;
        double windPerLength = site.VelocityPressurePa(meanHeight) * site.GcpNet * tributaryWidthM;
        Seq<MemberLoad> actions = Seq<MemberLoad>(new MemberLoad.Uniform(StructuralCase.Wind, new Vector3(windPerLength, 0.0, 0.0)));
        if (horizontal && !roofMember) {
            actions = actions.Add(new MemberLoad.Uniform(StructuralCase.Live, new Vector3(0.0, 0.0, -site.LiveLoad.LiveLoadPa * tributaryWidthM)));
        }
        if (horizontal && roofMember) {
            actions = actions
                .Add(new MemberLoad.Uniform(StructuralCase.Live, new Vector3(0.0, 0.0, -LiveLoadClass.Roof.LiveLoadPa * tributaryWidthM)))
                .Add(new MemberLoad.Uniform(StructuralCase.Snow, new Vector3(0.0, 0.0, -site.RoofSnowPa * tributaryWidthM)));
        }
        return Fin.Succ(actions);
    }
}

// Frame projection inputs BOTH structural request cases supply — the static case its own factored combinations, the
// seismic case the zero-action unit spec the modal solve reads. One overload set discriminates on the request shape,
// so the projector, the derivation fold, and the lowering never learn which route called them.
public readonly record struct FrameInputs(Seq<NodeId> Targets, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy, Option<SiteActionPolicy> Site) {
    public static FrameInputs Of(AssessmentRequest.Structural request) => new(request.Targets, request.Combinations, request.Policy, request.Site);
    public static FrameInputs Of(AssessmentRequest.Seismic request) => new(request.Targets, Seq(LoadCombinationSpec.SeismicUnit), request.Policy, request.Site);
}

public static partial class StructuralAnalysis {
    public static Fin<FrameModel> Project(ElementGraph graph, FrameInputs inputs, GeometrySource geometry) =>
        inputs.Targets.Fold(
            Fin.Succ(Seq<StructuralMember>()),
            (acc, id) => acc.Bind(members =>
                from axis     in graph.AxisOf(id, geometry)
                from strength in graph.PropertiesOf(id).Mechanical.ToFin(MissingInput(id, "mechanical"))
                from section  in graph.SectionOf(id).ToFin(MissingInput(id, "section"))
                // Realized seam Orthotropic case (Composition/material#MATERIAL_PROPERTY, same Discipline.Structural,
                // discriminated by case TYPE) — an OPTIONAL directional-stiffness refinement read off the seam graph via
                // `props.Orthotropic` exposes directional material moduli for timber along/across grain and carries its
                // independent G ≈ E0/16 here, an isotropic member carries None and the EC5 LTB falls back to the derived
                // Mechanical shear. Isotropic Mechanical stays required for E/ν and family classification; Orthotropic
                // supplies the independent §6.3.3 shear-stiffness refinement.
                let directional = graph.PropertiesOf(id).Orthotropic
                // Seam-published RC shear-link triple reads off the inherited derived bag — absence is the
                // link-less (or yield-less) section the producer declared, and the en1992 shear cells then take
                // their linkless V_Rd,c arm honestly instead of a dead truss pairing.
                let shearLink  = graph.ShearLinkOf(id)
                let family     = MaterialFamily.Classify(strength, directional)
                let selfWeight = new MemberLoad.Uniform(StructuralCase.Dead,
                    new Vector3(0d, 0d, -(section.Area.Si * strength.Density.Si * StandardGravity)))
                select members.Add(new StructuralMember(
                    id, axis, section, strength, directional, family, graph.LoadsOf(id).Add(selfWeight), graph.SupportsOf(id), shearLink))))
            .Bind(members => DeriveAbsent(members, inputs.Site))
            .Map(members => new FrameModel(members, inputs.Combinations, inputs.Policy, graph.Header.Tolerance));

    // Variant screening without a load engineer: a member whose graph carries NO applied action (self-weight is
    // this projector's own mint, never evidence of loading) takes the ActionDerivation live/wind/snow set under the
    // request's SiteActionPolicy — roof membership reads the model's top band, tributary width the policy's strip —
    // while explicit projected actions stay authoritative and derivation never runs beside them; an absent Site
    // leaves the empty set honest instead of fabricating code actions from an undeclared site.
    static Fin<Seq<StructuralMember>> DeriveAbsent(Seq<StructuralMember> members, Option<SiteActionPolicy> declared) =>
        declared.Match(
            None: () => Fin.Succ(members),
            Some: site => {
                double top = members.Map(static m => Math.Max(m.Axis.Start.Z, m.Axis.End.Z)).Max(double.NegativeInfinity);
                return members.TraverseM(member => member.Loads.Count > 1
                    ? Fin.Succ(member)
                    : ActionDerivation.Derive(member, site.TributaryWidthM, top - Math.Max(member.Axis.Start.Z, member.Axis.End.Z) <= site.RoofBandM, site)
                        .Map(derived => member with { Loads = member.Loads + derived })).As();
            });

    static Error MissingInput(NodeId id, string what) =>
        new ComputeFault.AssessmentInputMissing($"<member-missing-{what}:{id.Value}>");
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// Compute-owned discipline graph reads extend ElementGraph through seam no-Op primitives
// (MaterialsOf/PropertiesOf(id).Mechanical/Find/EdgesAt) and the projected neutral Generic structural edges by
// wire-name. The edge-attribute reads themselves compose the one Analysis/assessment AnalysisReads owner, so this
// page holds the structural INTERPRETATION of a row and never a fourth copy of the access shape. The seam owns the
// material/section/mechanical reads (it owns those nodes) and the GeometrySource decode CONTRACT; the discipline
// physics — axis interpretation, 6-DOF restraints, applied actions — lives here, never in the seam. AxisOf resolves the analytical line ONE-HOP by content key,
// GeometrySource reading member.Representations.Axis rather than a phantom node field, and AxisCurve.Length/
// Coplanar fold that resolved line the runner reasons over in double precision. Every row this boundary keys is a
// seam-declared StructuralRows static — the projector and this reader are non-referencing peers, so a call-site
// PropertyName.Create or a hand-built `{stem}X` spelling forks the key space the moment either side renames.
public static class StructuralReads {
    const string ConnectsMember   = "IfcRelConnectsStructuralMember";
    const string ConnectsActivity = "IfcRelConnectsStructuralActivity";

    public static bool Coplanar(this AxisCurve a, double z) => Math.Abs(a.Start.Z - z) < 1e-6 && Math.Abs(a.End.Z - z) < 1e-6;

    // Idealized analytical line resolves one hop by content key through GeometrySource — the Object node
    // carries NO inline Axis coordinate (the seam carries only `member.Representations.Axis`, an Option<UInt128> content key
    // into the blob store), so the runner reads the member's Object node, pulls its `Representations.Axis` key, and decodes
    // it to an AxisCurve through the app-wired resolver — never a phantom node field. A member with no Object node, no Axis
    // key, or an undecodable blob rails the typed input fault, never a defaulted axis.
    public static Fin<AxisCurve> AxisOf(this ElementGraph graph, NodeId member, GeometrySource geometry) =>
        graph.Find<Node.Object>(member).Bind(o => geometry.Axis(o.Representations))
            .ToFin(new ComputeFault.AssessmentInputMissing($"<member-axis-absent:{member.Value}>"));

    // Materials capacity screen publishes the RC shear-link triple ALL THREE rows or NONE (its whole-or-nothing
    // mint), so a partial read here names bag corruption and answers absence like every other missing
    // idealization input. The walk follows the seam's own Assign/PropertyDefinition shape over the uniform
    // edge accessors — the member's baked Object carries the inherited derived Realization bag — and every row name
    // resolves through the Element StructuralRows statics, never a call-site spelling.
    public static Option<RcShearLink> ShearLinkOf(this ElementGraph graph, NodeId member) =>
        from area in MeasuredRow(graph, member, StructuralRows.ShearLinkArea)
        from fywd in MeasuredRow(graph, member, StructuralRows.ShearLinkYield)
        from ceiling in MeasuredRow(graph, member, StructuralRows.ShearLinkCeiling)
        select new RcShearLink(area, fywd, ceiling);

    static Option<double> MeasuredRow(ElementGraph graph, NodeId owner, PropertyName row) =>
        toSeq(graph.EdgesAt(owner))
            .Filter(e => e.Kind == RelationshipKind.Assign && e.Relating == owner)
            .Choose(e => graph.Find(e.Related))
            .Choose(node => node is Node.PropertySet set ? set.Bag.Find(row) : Option<PropertyValue>.None)
            .Choose(value => value is PropertyValue.Measure m ? Some(m.Value.Si) : Option<double>.None)
            .Head;

    // One MemberSupport per structural-connection edge the member relates — one DofRestraint per degree of freedom
    // off the projector's ONE row per DOF, the skewed basis off the ONE positional StructuralRows.Frame row, and the
    // start/end discriminant off AtStart, all read from the neutral Generic edge payload the projector baked from
    // IfcBoundaryNodeCondition (+ its ConditionCoordinateSystem).
    public static Seq<MemberSupport> SupportsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsMember && g.Relating == member
            ? Some(new MemberSupport(g.Related, g.Flag(StructuralRows.AtStart),
                Dof(g, StructuralRows.Translation["X"]), Dof(g, StructuralRows.Translation["Y"]), Dof(g, StructuralRows.Translation["Z"]),
                Dof(g, StructuralRows.Rotation["X"]), Dof(g, StructuralRows.Rotation["Y"]), Dof(g, StructuralRows.Rotation["Z"]),
                FrameOf(g)))
            : None).ToSeq();

    // One MemberLoad per structural-activity edge the member relates — the kind (point/uniform/trapezoid), the load
    // case, and the component vectors read off the neutral Generic edge payload the projector baked from
    // IfcStructuralLoadSingleForce/LinearForce; self-weight is the Dead Uniform Project derives, so these are the
    // applied actions only. An unrecognized kind folds to a midspan Point (the projector's default action shape).
    public static Seq<MemberLoad> LoadsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsActivity && g.Relating == member
            ? Some(ToLoad(g)) : None).ToSeq();

    static MemberLoad ToLoad(Relationship.Generic g) => Kind(g) switch {
        "uniform"   => new MemberLoad.Uniform(CaseOf(g), Vec(g, StructuralRows.Force)),
        "trapezoid" => new MemberLoad.Trapezoid(CaseOf(g), Vec(g, StructuralRows.Start), Vec(g, StructuralRows.End)),
        // Presence-based station: a TRUE start-joint action (0.0) is a real position — only an ABSENT attr
        // defaults midspan (the projector's honest None), never a truthiness collapse of 0.0.
        _           => new MemberLoad.Point(CaseOf(g), Vec(g, StructuralRows.Force), Vec(g, StructuralRows.Moment), g.Magnitude(StructuralRows.Station).IfNone(0.5)),
    };

    static string Kind(Relationship.Generic g) => g.Text(StructuralRows.LoadKind).IfNone("point");

    static StructuralCase CaseOf(Relationship.Generic g) =>
        g.Text(StructuralRows.Case)
            .Bind(static value => StructuralCase.TryGet(value, out StructuralCase c) ? Some(c) : None)
            .IfNone(StructuralCase.Live);

    // Dof yields that degree's whole reading in one probe — DofRestraint.Of owns the case discrimination, so no
    // consumer here knows a Boolean from a Measure.
    static DofRestraint Dof(Relationship.Generic g, PropertyName row) => DofRestraint.Of(g.Attribute(row));

    // FrameOf reads ONE positional list of six Number direction ratios (AxisX..Z then RefX..Z); the projector emits
    // that whole row or none, so a short or non-numeric list reads ABSENT rather than a half-built basis.
    static Option<SupportFrame> FrameOf(Relationship.Generic g) =>
        g.Attribute(StructuralRows.Frame)
            .Bind(static value => value is PropertyValue.List list
                ? list.Values.Traverse(static ratio => ratio is PropertyValue.Number number ? Some(number.Value) : None).As()
                : None)
            .Bind(static ratios => ratios.ToArray() is [var ax, var ay, var az, var rx, var ry, var rz]
                ? Some(new SupportFrame(new Vector3(ax, ay, az), new Vector3(rx, ry, rz)))
                : None);

    // Vec takes the axis FAMILY itself — one owner-declared Map per component family — so three ordinates resolve
    // through its own axis keys and no call site rebuilds a `{stem}X` spelling the declarer already holds. The
    // zero-defaulting Si is the right read here: an unstamped ordinate genuinely contributes no component, while the
    // presence-preserving Magnitude serves the station read, where an absent attribute and a real 0.0 differ.
    static Vector3 Vec(Relationship.Generic g, Map<string, PropertyName> family) =>
        new(g.Si(family["X"]), g.Si(family["Y"]), g.Si(family["Z"]));
}
```

## [03]-[FRAME_BACKEND]

- Owner: the `Solve` owned-spine route — `FrameModel` lowers onto the `Solver/contract#SOLVE_CONTRACT` `SolveLane` over the `Solver/discretization#DISCRETIZATION_MESH` frame `ElementClass` rows (`beam2-euler`/`beam2-timoshenko`, the `StructuralPolicy.Formulation` column), so the structural lane assembles and factors through the same CSparse owner the continuum lane holds, the owned rows carrying end releases by static condensation, rigid-end offsets by eccentricity transform, and semi-rigid end springs as row behavior; `SectionDemand` the signed per-station internal-force sample; `MemberResponse` the SIGNED two-extreme bound with deflection every limit state reads; `FrameLowering` the model→mesh projection (shared joints merged by tolerance-quantized coordinate, per-member `FrameMember` section/release/offset rows off the seam `SectionProperties` and the declared supports, per-member `(E, ν, ρ)` on `MaterialField.PerCellElastic`, member loads lowered to fixed-end equivalent nodal actions); `StationRecovery` the per-member station fold off the solved displacement field.
- Entry: `static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model, IClock clock)` — lowers the model once, then per `LoadCombinationSpec` scales the case actions, solves through `SolveLane.Solve` (the frame arm scattering each member's closed-form 12-DOF block), recovers the worst-station `SectionDemand` and transverse deflection per member, and folds `StationRecovery.Envelope` across combinations; `Fin<T>` lowers a singular/ill-conditioned factorization onto the typed `ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, …)` — deterministic, cached by the spine, never re-run blind — and a member missing its section or support set onto `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`.
- Auto: joints merge by tolerance-quantized coordinate (never fragile exact-float `Vector3` equality); each `MemberSupport` lowers its `Degrees` projection to the `BoundaryCondition.Dirichlet` constraint set on its endpoint-resolved shared joint, `DofRestraint.Constrains` selecting the slots; each `MemberLoad` case lowers through a TOTAL `Switch` to its fixed-end equivalent nodal actions (Point by the closed-form ab²/L² pair, Uniform by wL/2 + wL²/12, Trapezoid by the exact linear-varying closed form — never a flattened uniform average) landing as `Neumann` rows on the member-end DOFs; per-station recovery reads the solved field back through each member's local frame — end displacements gathered and rotated local, local end forces `f = k_l·u_l − f_fixed`, station N/V/M by statics from the end forces and the span-load particular terms (exact for the three load kinds), station transverse deflection by the Hermite end-displacement interpolation with the span-load particular deflection — so the `Deflection` limit state is a REAL displacement check, never a 0.0 sentinel.
- Packages: CSparse (shared `SparseCholesky`/`SparseLDL`/`SparseLU`/`SparseQR` family via `Tensor/factor#SPARSE_SOLVE`, selected by `Solver/contract` policy), Rasm.Element (project — `SectionProperties`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new frame formulation is one `ElementClass` frame row (the `Formulation` policy column selects it); a new end condition is a column on `FrameMember` the discretization closed form reads; a new load kind is one `MemberLoad` case with one fixed-end arm on the total `Switch`; the response bound is one `MemberResponse` shape the checks read regardless of formulation — an external FE backend beside the owned spine is the rejected duplicate-mechanism form.
- Boundary: the frame solve is the `Solver/contract` spine — one `SolveLane`, one CSparse factorization owner, one `MaterialField` elasticity admission — and a hand-rolled stiffness assembler beside it is the rejected form; the member releases/rigid-end offsets/semi-rigid springs are ROW BEHAVIOR on the discretization `ElementClass.Member` closed form (condensation/transform/in-series fold); the local frame orders moments `(T=torsion about x, My/Mz=bending)` and the demand maps `SectionDemand(N, Vy, Vz, My, Mz, T)` off the local end-force vector — never a torsion/bending swap; `MemberResponse` keeps BOTH signed extremes per component, so a sense-selecting limit state reads the extreme its own capacity bounds and an `|magnitude|` fold reporting a tension-carrying member as untensioned cannot form; the planar special case is structural, not a second backend — a coplanar model carries zero out-of-plane demand through the same 12-DOF rows; a singular system surfaces as the typed `(Solve, Numeric)` `AnalysisFailed`, never an exception crossing the rail and never an opaque interpolated discriminant.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// One signed internal-force sample at one station under one combination; the envelope keeps its two extremes rather
// than folding them, so the component-wise Lower/Upper are the only reductions this shape offers.
public readonly record struct SectionDemand(double N, double Vy, double Vz, double My, double Mz, double T) {
    public static readonly SectionDemand Zero = new(0, 0, 0, 0, 0, 0);
    public SectionDemand Lower(SectionDemand b) => new(
        Math.Min(N, b.N), Math.Min(Vy, b.Vy), Math.Min(Vz, b.Vz), Math.Min(My, b.My), Math.Min(Mz, b.Mz), Math.Min(T, b.T));
    public SectionDemand Upper(SectionDemand b) => new(
        Math.Max(N, b.N), Math.Max(Vy, b.Vy), Math.Max(Vz, b.Vz), Math.Max(My, b.My), Math.Max(Mz, b.Mz), Math.Max(T, b.T));
    // Response-spectrum modal combinations are sign-indefinite (±): the negation is the companion extreme the
    // envelope pairs it with, so the seismic route folds one magnitude into two signed states without a second shape.
    public static SectionDemand operator -(SectionDemand d) => new(-d.N, -d.Vy, -d.Vz, -d.My, -d.Mz, -d.T);
}

// What the design check reads: the SIGNED per-component envelope — Min the most-negative extreme, Max the most-positive
// — over every station and every combination, plus the worst transverse deflection, so the Deflection limit state is a
// REAL displacement check against StructuralPolicy × span. Signed is load-bearing: a member carrying +50 kN tension
// under one combination and −80 kN compression under another has TWO governing states, and an |magnitude| fold keeps
// only −80, so the tension check reads max(−80, 0) = 0 and publishes a perfect pass on a member in tension. A
// per-component envelope is the conservative member-level bound the codes check; station-correlated interaction is a
// growth axis the Check fold would take over this same shape.
public readonly record struct MemberResponse(SectionDemand Min, SectionDemand Max, double MaxDeflection) {
    public static readonly MemberResponse Zero = new(SectionDemand.Zero, SectionDemand.Zero, 0.0);
    public MemberResponse Absorb(SectionDemand d, double deflection) =>
        new(Min.Lower(d), Max.Upper(d), Math.Max(MaxDeflection, Math.Abs(deflection)));
    public MemberResponse Merge(MemberResponse b) =>
        new(Min.Lower(b.Min), Max.Upper(b.Max), Math.Max(MaxDeflection, b.MaxDeflection));

    // Span is the worst |magnitude| of a component whose SENSE does not select a different capacity — a moment or a
    // shear reverses without changing which cell bounds it, so both extremes collapse to one demand there while the
    // axial component keeps its two senses apart.
    public double Span(Func<SectionDemand, double> component) =>
        Math.Max(Math.Abs(component(Min)), Math.Abs(component(Max)));

    // TensionCorner and CompressionCorner are the two signed corner states a code interaction evaluates: tension
    // takes the positive-N extreme, compression the negative one, both against every reversing component's worst
    // magnitude. Together they bound the member conservatively under a per-component envelope, and Check governs
    // on the worse of the two.
    public SectionDemand TensionCorner => Corner(Math.Max(Max.N, 0.0));
    public SectionDemand CompressionCorner => Corner(Math.Min(Min.N, 0.0));

    SectionDemand Corner(double n) => new(n,
        Span(static d => d.Vy), Span(static d => d.Vz), Span(static d => d.My), Span(static d => d.Mz), Span(static d => d.T));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    // Owned frame solve lowers once, solves per combination, and recovers per
    // station, envelope across combinations — the one spine entry, never a parallel FE engine.
    public static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model, IClock clock) =>
        model.Members.IsEmpty
            ? Fin.Succ(FrozenDictionary<NodeId, MemberResponse>.Empty)
            : FrameLowering.Lower(model).Bind(lowered =>
                model.Combinations.Fold(
                    Fin.Succ(model.Members.Map(static m => (m.Id, Response: MemberResponse.Zero)).ToFrozenDictionary(static p => p.Id, static p => p.Response)),
                    (acc, combo) => acc.Bind(envelope =>
                        lowered.Problem(PhysicsKind.FeaStatic, combo).Bind(problem =>
                            SolveLane.Solve(problem, lowered.Mesh, SolvePolicy.CanonicalStatic, clock)
                                .MapFail(fault => fault is ComputeFault.ModelRejected reject
                                    ? new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<frame-singular:{combo.Label}:{reject.Message}>")
                                    : fault)
                                .Map(solution => StationRecovery.Envelope(model, lowered, combo, solution.Field, envelope))))));

    // Model -> frame-family DiscreteMesh: joints merged by tolerance-quantized coordinate, one 2-node cell per
    // member, per-member FrameMember rows (section constants off the seam SectionProperties; semi-rigid springs
    // off the declared supports), per-member (E, ν, ρ) on MaterialField.PerCellElastic, supports as 6-DOF Dirichlet
    // rows, member loads as fixed-end equivalent Neumann actions per combination. JointOf/EndJoints carry the joint
    // resolution the recovery fold re-reads, so lowering and recovery share one coordinate quantization.
    internal sealed record FrameLowered(DiscreteMesh Mesh, ImmutableArray<FrameMember> Members, MaterialField Field, Func<LoadCombinationSpec, Seq<BoundaryCondition>> Conditions, Func<StructuralMember, (long I, long J)> EndJoints) {
        // PHYSICS is the caller's discriminant, not a lowering constant: the static envelope lowers fea-static and the
        // response-spectrum route fea-modal, and the SolveLane routes its eigen arm off PhysicsKind.Eigen alone — a
        // lowering that hardcoded the static row would send the modal request down the direct solve and return a
        // displacement field with no spectrum, which the participation gate could only report as an absent stream.
        // Payload stays Continuum (frame member blocks carry their own geometry) and the constitutive law None
        // (frame rows are linear-elastic closed forms), both stated positionally rather than defaulted away.
        public Fin<SolveProblem> Problem(PhysicsKind physics, LoadCombinationSpec combo) =>
            SolveProblem.Of(physics, Mesh, Conditions(combo), Field, new PhysicsPayload.Continuum(), Members, None);
    }

    internal static class FrameLowering {
        static (long X, long Y, long Z) Quantized(Vector3 point, double quantum) =>
            ((long)Math.Round(point.X / quantum), (long)Math.Round(point.Y / quantum), (long)Math.Round(point.Z / quantum));

        public static Fin<FrameLowered> Lower(FrameModel model) =>
            MaterialField.OfMechanical(model.Members.Map(static m => Some(m.Strength)))
                .Bind(field => {
                    (Seq<Vector3> joints, Func<(long, long, long), long> jointOf) = Joints(model);
                    return Mesh(model, joints, jointOf).Map(mesh => new FrameLowered(
                        mesh,
                        [.. model.Members.Map(static m => new FrameMember(
                            m.Section.Area.Si, m.Section.Iyy.Si, m.Section.Izz.Si, m.Section.J.Si,
                            UpX: m.Axis.Up.X, UpY: m.Axis.Up.Y, UpZ: m.Axis.Up.Z,
                            SpringYi: SpringAt(m, atStart: true, axisY: true), SpringZi: SpringAt(m, atStart: true, axisY: false),
                            SpringYj: SpringAt(m, atStart: false, axisY: true), SpringZj: SpringAt(m, atStart: false, axisY: false),
                            ShearAreaY: m.Section.AvY.Si, ShearAreaZ: m.Section.AvZ.Si))],
                        field,
                        combo => Conditions(model, jointOf, combo),
                        member => (jointOf(Quantized(member.Axis.Start, model.JointTolerance)), jointOf(Quantized(member.Axis.End, model.JointTolerance)))));
                });

        // Merged joint set quantizes and deduplicates every endpoint; jointOf serves lowering,
        // Dirichlet rows, and recovery through one coordinate policy.
        static (Seq<Vector3> Joints, Func<(long, long, long), long> JointOf) Joints(FrameModel model) {
            (Seq<Vector3> Order, Map<(long, long, long), long> Map) index = model.Members.Bind(static m => Seq(m.Axis.Start, m.Axis.End))
                .Fold((Order: Seq<Vector3>(), Map: Map<(long, long, long), long>()), (acc, p) =>
                    acc.Map.ContainsKey(Quantized(p, model.JointTolerance))
                        ? acc
                        : (acc.Order.Add(p), acc.Map.Add(Quantized(p, model.JointTolerance), acc.Order.Count)));
            return (index.Order, key => index.Map.Find(key).IfNone(-1L));
        }

        // Frame mesh stores joints and one 2-node line cell per member as the FLAT row-major buffers the DiscreteMesh
        // ReadOnlyMemory<float>/<long> members carry (its NodeTensor/ElementTensor views derive from them — a Tensor<T>
        // never crosses this ctor). Exemption: the node/connectivity buffer fill is the measured-kernel statement seam.
        static Fin<DiscreteMesh> Mesh(FrameModel model, Seq<Vector3> joints, Func<(long, long, long), long> jointOf) {
            float[] nodes = new float[joints.Count * 3];
            long[] connectivity = new long[model.Members.Count * 2];
            Vector3[] jointArray = joints.ToArray();
            for (int i = 0; i < jointArray.Length; i++) { nodes[i * 3] = (float)jointArray[i].X; nodes[i * 3 + 1] = (float)jointArray[i].Y; nodes[i * 3 + 2] = (float)jointArray[i].Z; }
            StructuralMember[] memberArray = model.Members.ToArray();
            for (int c = 0; c < memberArray.Length; c++) {
                connectivity[c * 2] = jointOf(Quantized(memberArray[c].Axis.Start, model.JointTolerance));
                connectivity[c * 2 + 1] = jointOf(Quantized(memberArray[c].Axis.End, model.JointTolerance));
            }
            return Fin.Succ(new DiscreteMesh(model.Policy.Formulation, MeshAlgorithm.Sweep, nodes, connectivity,
                joints.Count, model.Members.Count, 0, 0, 0, MeshMetric.ScaledJacobian, 1.0, None, default));
        }

        // Supports lower to per-DOF Dirichlet rows on the endpoint-resolved joint, the slot ordinal of the support's
        // own Degrees projection carrying the DOF address (joint*6 + [ux..rz]) and Constrains the gate — so the
        // rigid and the conservatively-lowered translational-spring readings both constrain and only Free stays open;
        // member loads lower per combination to fixed-end equivalent Neumann actions on the member-end DOFs, each
        // case scaled by its combination factor.
        static Seq<BoundaryCondition> Conditions(FrameModel model, Func<(long, long, long), long> jointOf, LoadCombinationSpec combo) =>
            model.Members.Bind(m => m.Supports.Map(s => {
                long joint = jointOf(Quantized(s.AtStart ? m.Axis.Start : m.Axis.End, model.JointTolerance));
                Seq<long> dofs = s.Degrees
                    .Map(static (dof, slot) => (Dof: dof, Slot: slot))
                    .Choose(row => row.Dof.Constrains ? Some(joint * 6 + row.Slot) : None);
                return (BoundaryCondition)new BoundaryCondition.Dirichlet(FieldStation.Nodal, [.. dofs], [.. dofs.Map(static _ => 0.0)]);
            }))
            + model.Members.Bind(m => m.Loads
                .Choose(load => combo.Factors.TryGetValue(CaseArm(load), out double factor)
                    ? LoadCondition(m, model.JointTolerance, jointOf, load, factor)
                    : None));

        // Loads resolve in the MEMBER LOCAL frame: the Topology.Triad (x̂ along axis, ẑ from the AxisCurve Up, ŷ = ẑ×x̂)
        // projects every applied vector into axial + BOTH transverse planes — z-plane pair (uz, ry) bending about ŷ,
        // y-plane pair (uy, rz) bending about ẑ — each plane taking its own fixed-end set, a Point force splitting its
        // axial component by station, and a Point moment contributing its torsional (x̂) component on rx and the
        // concentrated-moment fixed-end pair per bending plane; no admitted force or moment component vanishes.
        static Option<BoundaryCondition> LoadCondition(StructuralMember member, double jointTolerance, Func<(long, long, long), long> jointOf, MemberLoad load, double factor) {
            long i = jointOf(Quantized(member.Axis.Start, jointTolerance)), j = jointOf(Quantized(member.Axis.End, jointTolerance));
            double[] triad = LocalTriad(member);
            double Ax(Vector3 v) => v.X * triad[0] + v.Y * triad[1] + v.Z * triad[2];
            double Ly(Vector3 v) => v.X * triad[3] + v.Y * triad[4] + v.Z * triad[5];
            double Lz(Vector3 v) => v.X * triad[6] + v.Y * triad[7] + v.Z * triad[8];
            double length = member.Length;
            double[] z = FixedEnd(load, length, Lz).EndForces;
            double[] y = FixedEnd(load, length, Ly).EndForces;
            double[] flux = new double[12];
            flux[2] += z[0]; flux[4] += z[1]; flux[8] += z[2]; flux[10] += z[3];
            flux[1] += y[0]; flux[5] += y[1]; flux[7] += y[2]; flux[11] += y[3];
            switch (load) {
                case MemberLoad.Point point: {
                    double axial = Ax(point.Force);
                    flux[0] += axial * (1.0 - point.Station); flux[6] += axial * point.Station;
                    double torque = Ax(point.Moment);
                    flux[3] += torque * (1.0 - point.Station); flux[9] += torque * point.Station;
                    double a = point.Station * length, b = length - a, l2 = length * length;
                    // Concentrated end-moment pair per bending plane: V = ∓6·M₀·ab/L³, Mi = M₀·b(2a−b)/L², Mj = M₀·a(2b−a)/L².
                    foreach ((double m0, int shear, int mi, int mj) in Seq((Ly(point.Moment), 2, 4, 10), (Lz(point.Moment), 1, 5, 11))) {
                        flux[shear] += -6.0 * m0 * a * b / (l2 * length); flux[shear + 6] += 6.0 * m0 * a * b / (l2 * length);
                        flux[mi] += m0 * b * (2.0 * a - b) / l2; flux[mj] += m0 * a * (2.0 * b - a) / l2;
                    }
                    break;
                }
                case MemberLoad.Uniform uniform: { double w = Ax(uniform.ForcePerLength) * length; flux[0] += w / 2.0; flux[6] += w / 2.0; break; }
                case MemberLoad.Trapezoid trapezoid: { double w = (Ax(trapezoid.Start) + Ax(trapezoid.End)) / 2.0 * length; flux[0] += w / 2.0; flux[6] += w / 2.0; break; }
                default: break;
            }
            return Some<BoundaryCondition>(new BoundaryCondition.Neumann(
                [i * 6 + 0, i * 6 + 1, i * 6 + 2, i * 6 + 3, i * 6 + 4, i * 6 + 5, j * 6 + 0, j * 6 + 1, j * 6 + 2, j * 6 + 3, j * 6 + 4, j * 6 + 5],
                Scaled(flux, factor)));
        }

        internal static double[] LocalTriad(StructuralMember member) {
            Span<double> r = stackalloc double[9];
            Topology.Triad(
                member.Axis.End.X - member.Axis.Start.X, member.Axis.End.Y - member.Axis.Start.Y, member.Axis.End.Z - member.Axis.Start.Z,
                member.Axis.Up.X, member.Axis.Up.Y, member.Axis.Up.Z, r);
            return r.ToArray();
        }

        internal static StructuralCase CaseArm(MemberLoad load) => load.Switch(
            point: static p => p.Case, uniform: static u => u.Case, trapezoid: static t => t.Case);

        static double[] Scaled(double[] forces, double factor) {
            double[] scaled = new double[forces.Length];
            TensorPrimitives.Multiply(forces, factor, scaled);
            return scaled;
        }

        // Fixed-end equivalent nodal actions per MemberLoad case — the TOTAL Switch: Point by the ab²/L² closed-form
        // pair, Uniform by (wL/2, wL²/12), Trapezoid by the exact linear-varying form (w1 the start, w2 the end
        // intensity); a flattened trapezoid-to-uniform average is the deleted form. The `local` projector selects the
        // member-local transverse component (Lz for the z-plane, Ly for the y-plane) so ONE closed form serves both
        // bending planes — a hard-coded global Force.Z read is the deleted form. EndForces packs the transverse end
        // shears and end moments [Vi, Mi, Vj, Mj]; the particular arrows carry the span-load interior moment and
        // deflection terms the station recovery adds to the end-force statics — exact for the three load kinds.
        public static (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) FixedEnd(MemberLoad load, double length, Func<Vector3, double> local) =>
            load.Switch(
                point: p => {
                    double magnitude = local(p.Force), a = p.Station * length, b = length - a, l2 = length * length;
                    double mi = magnitude * a * b * b / l2, mj = -magnitude * a * a * b / l2;
                    double vi = magnitude * b * b * (length + 2.0 * a) / (l2 * length);
                    return (new[] { vi, mi, magnitude - vi, mj },
                        (Func<double, double>)(x => x < a ? 0.0 : -magnitude * (x - a)),
                        (Func<double, double>)(x => x < a ? 0.0 : magnitude * Math.Pow(x - a, 3) / 6.0));
                },
                uniform: u => {
                    double w = local(u.ForcePerLength), l2 = length * length;
                    return (new[] { w * length / 2.0, w * l2 / 12.0, w * length / 2.0, -w * l2 / 12.0 },
                        (Func<double, double>)(x => -w * x * x / 2.0),
                        (Func<double, double>)(x => w * Math.Pow(x, 4) / 24.0));
                },
                trapezoid: t => {
                    double w1 = local(t.Start), w2 = local(t.End), l2 = length * length;
                    double vi = length * (7.0 * w1 + 3.0 * w2) / 20.0, vj = length * (3.0 * w1 + 7.0 * w2) / 20.0;
                    double mi = l2 * (w1 / 20.0 + w2 / 30.0), mj = -l2 * (w1 / 30.0 + w2 / 20.0);
                    return (new[] { vi, mi, vj, mj },
                        (Func<double, double>)(x => -(w1 * x * x / 2.0 + (w2 - w1) * x * x * x / (6.0 * length))),
                        (Func<double, double>)(x => w1 * Math.Pow(x, 4) / 24.0 + (w2 - w1) * Math.Pow(x, 5) / (120.0 * length)));
                });

        // SpringAt yields the rotational end spring the FrameMember semi-rigid column reads: that DOF's own finite
        // rate where the projector stamped a spring, +∞ otherwise — a rigid or free reading means its member-end
        // attachment is itself rigid and its joint restraint rides the Dirichlet row. Finiteness now guards at
        // DofRestraint.Of, so an absent support and an unsprung one resolve through the same None.
        static double SpringAt(StructuralMember m, bool atStart, bool axisY) =>
            m.Supports.Find(s => s.AtStart == atStart)
                .Bind(s => (axisY ? s.Ry : s.Rz).Rate)
                .IfNone(double.PositiveInfinity);
    }

    internal static class StationRecovery {
        // Per-member station fold off the solved global field: end displacements gathered and rotated local (the
        // direction-cosine frame the stiffness used), local end forces f = k_l·u_l − f_fixed, station N/V/M by
        // statics from the end forces plus the span-load particular terms, station transverse deflection by the
        // Hermite end-displacement interpolation plus the particular deflection — exact for the three load kinds,
        // enveloped over StructuralPolicy.StationCount stations and across combinations into the prior envelope.
        public static FrozenDictionary<NodeId, MemberResponse> Envelope(FrameModel model, FrameLowered lowered, LoadCombinationSpec combo, ReadOnlyMemory<double> field, FrozenDictionary<NodeId, MemberResponse> prior) =>
            Demands(model, lowered, combo, field)
                .Map(row => (row.Id, Response: prior[row.Id].Merge(row.Response)))
                .ToFrozenDictionary(static row => row.Id, static row => row.Response);

        // Per-member recovery kernel serves both static envelope and seismic per-mode demand.
        // Exemption: the station march over the solved field is the measured-kernel statement seam.
        public static Seq<(NodeId Id, MemberResponse Response)> Demands(FrameModel model, FrameLowered lowered, LoadCombinationSpec combo, ReadOnlyMemory<double> field) =>
            model.Members.Map(member => {
                (long i, long j) = lowered.EndJoints(member);
                double length = member.Length;
                // 6-DOF per joint: [ux, uy, uz, rx, ry, rz] — end displacement AND rotation vectors rotate into the
                // SAME member-local triad the stiffness was rotated with (LocalTriad), so both transverse pairs —
                // (uz, ry) bending about ŷ over Iyy and (uy, rz) bending about ẑ over Izz — recover in the local
                // frame; global component reads on a rolled member are the deleted form.
                ReadOnlySpan<double> u = field.Span;
                double[] r = FrameLowering.LocalTriad(member);
                double[] li = Localized(u, i, r), lj = Localized(u, j, r);
                double axial = (lj[0] - li[0]) / Math.Max(length, StructuralAnalysis.Eps) * member.Strength.YoungsModulus.Si * member.Section.Area.Si;
                double torsion = (lj[3] - li[3]) / Math.Max(length, StructuralAnalysis.Eps) * member.Strength.ShearModulus.Si * member.Section.J.Si;
                double[] triad = r;
                Func<Vector3, double> ly = v => v.X * triad[3] + v.Y * triad[4] + v.Z * triad[5];
                Func<Vector3, double> lz = v => v.X * triad[6] + v.Y * triad[7] + v.Z * triad[8];
                Seq<MemberLoad> loads = member.Loads.Filter(load => combo.Factors.ContainsKey(FrameLowering.CaseArm(load)));
                var zActions = loads.Map(load => (Factor: Factor(combo, load), Action: FrameLowering.FixedEnd(load, length, lz)));
                var yActions = loads.Map(load => (Factor: Factor(combo, load), Action: FrameLowering.FixedEnd(load, length, ly)));
                MemberResponse response = MemberResponse.Zero;
                // Exact fixed-end decomposition per plane: M(x) = EI·v_h''(x) from the Hermite homogeneous term the
                // local joint displacements drive, plus the fixed-end particular chain; V(x) mirrors with EI·v_h'''.
                double eiY = member.Strength.YoungsModulus.Si * member.Section.Iyy.Si;
                double eiZ = member.Strength.YoungsModulus.Si * member.Section.Izz.Si;
                double zStartShear = zActions.Fold(0.0, static (acc, action) => acc + action.Factor * action.Action.EndForces[0]);
                double zStartMoment = zActions.Fold(0.0, static (acc, action) => acc + action.Factor * action.Action.EndForces[1]);
                double yStartShear = yActions.Fold(0.0, static (acc, action) => acc + action.Factor * action.Action.EndForces[0]);
                double yStartMoment = yActions.Fold(0.0, static (acc, action) => acc + action.Factor * action.Action.EndForces[1]);
                for (int s = 0; s < model.Policy.StationCount; s++) {
                    double x = length * s / Math.Max(model.Policy.StationCount - 1, 1);
                    double xi = x / Math.Max(length, StructuralAnalysis.Eps);
                    double vz = eiY * HermiteJerk(li[2], li[4], lj[2], lj[4], length) + zStartShear;
                    double my = eiY * HermiteCurvature(li[2], li[4], lj[2], lj[4], xi, length) + zStartMoment + zStartShear * x
                        + zActions.Fold(0.0, (acc, action) => acc + action.Factor * action.Action.ParticularMoment(x));
                    double vy = eiZ * HermiteJerk(li[1], li[5], lj[1], lj[5], length) + yStartShear;
                    double mz = eiZ * HermiteCurvature(li[1], li[5], lj[1], lj[5], xi, length) + yStartMoment + yStartShear * x
                        + yActions.Fold(0.0, (acc, action) => acc + action.Factor * action.Action.ParticularMoment(x));
                    double zDeflection = Hermite(li[2], li[4], lj[2], lj[4], xi, length)
                        + zActions.Fold(0.0, (acc, action) => acc + action.Factor * action.Action.ParticularDeflection(x) / Math.Max(eiY, StructuralAnalysis.Eps));
                    double yDeflection = Hermite(li[1], li[5], lj[1], lj[5], xi, length)
                        + yActions.Fold(0.0, (acc, action) => acc + action.Factor * action.Action.ParticularDeflection(x) / Math.Max(eiZ, StructuralAnalysis.Eps));
                    response = response.Absorb(new SectionDemand(axial, vy, vz, my, mz, torsion),
                        Math.Sqrt(zDeflection * zDeflection + yDeflection * yDeflection));
                }
                return (member.Id, response);
            });

        // Joint DOF vector rotated local: translations and rotations each map through the triad rows.
        static double[] Localized(ReadOnlySpan<double> field, long joint, double[] r) {
            double[] g = [At(field, joint, 0), At(field, joint, 1), At(field, joint, 2), At(field, joint, 3), At(field, joint, 4), At(field, joint, 5)];
            return [
                r[0] * g[0] + r[1] * g[1] + r[2] * g[2], r[3] * g[0] + r[4] * g[1] + r[5] * g[2], r[6] * g[0] + r[7] * g[1] + r[8] * g[2],
                r[0] * g[3] + r[1] * g[4] + r[2] * g[5], r[3] * g[3] + r[4] * g[4] + r[5] * g[5], r[6] * g[3] + r[7] * g[4] + r[8] * g[5],
            ];
        }

        static double At(ReadOnlySpan<double> field, long joint, int dof) =>
            field[checked((int)(joint * 6 + dof))];

        static double Hermite(double uzI, double ryI, double uzJ, double ryJ, double xi, double length) =>
            (1.0 - 3.0 * xi * xi + 2.0 * xi * xi * xi) * uzI + length * (xi - 2.0 * xi * xi + xi * xi * xi) * ryI
            + (3.0 * xi * xi - 2.0 * xi * xi * xi) * uzJ + length * (xi * xi * xi - xi * xi) * ryJ;

        // v''(x): the Hermite basis second derivatives over ξ = x/L — the curvature the homogeneous moment reads.
        static double HermiteCurvature(double uzI, double ryI, double uzJ, double ryJ, double xi, double length) =>
            ((-6.0 + 12.0 * xi) * uzI + length * (-4.0 + 6.0 * xi) * ryI
            + (6.0 - 12.0 * xi) * uzJ + length * (6.0 * xi - 2.0) * ryJ) / Math.Max(length * length, StructuralAnalysis.Eps);

        // v'''(x): constant over the cubic — the homogeneous shear term.
        static double HermiteJerk(double uzI, double ryI, double uzJ, double ryJ, double length) =>
            (12.0 * uzI + 6.0 * length * ryI - 12.0 * uzJ + 6.0 * length * ryJ) / Math.Max(length * length * length, StructuralAnalysis.Eps);

        // End-force statics seed from combination-scaled fixed-end shear/moment reactions at the start joint;
        // span statics then march N/V/M station-by-station with particular terms.
        static double Factor(LoadCombinationSpec combo, MemberLoad load) =>
            combo.Factors.TryGetValue(FrameLowering.CaseArm(load), out double factor) ? factor : 0.0;

    }
}
```

## [04]-[DESIGN_CHECK]

- Owner: `MaterialFamily` the constitutive family; `SafetyFormat` the ASD/LRFD/limit-state axis; `DesignCode` `[SmartEnum<string>]` the standard rows carrying the `MaterialFamily`, the `SafetyFormat`, the resistance/partial factors, and the interaction delegate; `LimitState` `[SmartEnum<string>]` the check rows carrying the demand-component selector and the `Applies(MaterialFamily)` predicate; `CapacityContext` the section+isotropic-strength+optional-orthotropic-stiffness+geometry+code bundle every capacity reads (its `ShearModulusSi` reading the realized seam `Orthotropic.ShearModulus` when the member carries the directional case, the derived isotropic `Mechanical` shear otherwise); the `Capacities` `(DesignCode, LimitState)` frozen table of REAL delegates; `MemberCapacity` the four sense-aware interaction operands and `MemberCheck` the per-check carrier whose optional utilization distinguishes a resolved ratio from an unserved `(code, state)` pair; `CheckFacts` the one fact-and-governing projection both routes fold; `StructuralAnalysis.Run` the governing-utilization entry, overloaded on the request case.
- Cases: `DesignCode` rows `aisc360`/`en1993`/`en1994`/`en1992`/`nds`/`en1995`/`aci318`/`tms402`/`en1996`/`aisi-s100` — every structural family carries BOTH its US and its Eurocode row (steel `aisc360`+`en1993` with the composite `en1994`, concrete `aci318`+`en1992`, timber `nds`+`en1995`, masonry `tms402`+`en1996`), so a member is assessable under either jurisdiction through the SAME table, never a US-only or EN-only family; the key SET is the `Rasm.Materials` `Component/capacity#SECTION_CAPACITY` `DesignBasis` roster spelled identically, so a section-altitude verdict and this member-altitude one name one jurisdiction; `LimitState` rows `axial-tension`/`axial-compression`/`flexure-major`/`flexure-minor`/`shear-major`/`shear-minor`/`combined`/`deflection` (shear split per axis so the major-axis demand `|Vy|` checks against `AvY` and the minor-axis `|Vz|` against `AvZ`, never one shear area for both) — the capacity is a `(code, state)` cell in the frozen table, each cell the GOVERNING formula for THAT code's material model (AISC E3 `Fcr`, EN 1993 `χ` buckling curve, AISC F2 `Mn` with `Lp`/`Lr` LTB, EN 1993 `χLT`, ACI/EN plain-concrete `Mcr`/`φPn`, NDS `CP`/`CL` adjusted reference values, EN 1995 `k_c`/`k_crit` over the `E0,05` 5%-fractile modulus, TMS slenderness-reduced `Fa`, AISI gross-section bound), the per-cell slenderness/compactness branches the rule count; lateral-torsional buckling is FOLDED into the flexure-major `Mn`/`Mₕ` (one capacity, never a duplicate state); an absent cell yields the `NotApplicable` verdict and NO ratio, so an unserved pair never publishes a `0.0`-utilization pass.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, IClock clock)` — `Project` reads the idealization off `FrameInputs`, `Solve` recovers the signed `MemberResponse` extremes, `Check` folds each member through every applicable `LimitState` computing `utilization = demand / capacity` (the `Combined` arm the code interaction over both signed corners, the `Deflection` arm the FE deflection against `StructuralPolicy.DeflectionLimitRatio × span`), and `CheckFacts` yields the fact stream (`max-utilization`, `governing-member`, `governing-limit-state`, per-check ratios, per-unserved-pair `not-applicable` verdicts) with the governing ratio the spine DERIVES the verdict from. `AssessmentRequest.Seismic` dispatches the `[05]` response-spectrum chain as the SIBLING OVERLOAD through the spine's own case `Switch`, never an `Option` gate inside this arm.
- Auto: the column capacity reads the `EffectiveLengthFactor × UnbracedLength / RadiusOfGyrationMinor` slenderness (AISC `Fcr`, EN `χ`); the flexure-major capacity reads `Lb` against `Lp` and the elastic LTB moment (EN `χLT`); the deflection check reads `MemberResponse.MaxDeflection`; the combined axial+flexure interaction folds each signed corner per the `DesignCode.Interaction` delegate (AISC 360 H1.1 and EN 1993-1-1 §6.3.3 for steel, the EN 1995-1-1 §6.3.2(3) squared-axial + linear-bending form with the `k_m = 0.7` minor-axis factor for timber, the linear sum for the rest), `Combined` applying to steel/cold-formed/timber.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, PureHDF (`H5File`, `H5Dataset<T>` — the shared demands/modal artifact), Generator.Equals (`[Equatable]`+`[PrecisionEquality]` — the MemberCheck variant diff), Rasm.Element (project — `SectionProperties`, `MaterialPropertySet` (the isotropic `Mechanical` AND the realized directional `Orthotropic` case), `NodeId`, `Provenance`, the `graph.PropertiesOf(member).Orthotropic` ergonomic read), NodaTime (`Instant`), BCL inbox (`FrozenDictionary`).
- Growth: a new design code is one `DesignCode` row with its `(code, state)` cells in the table, its key taken from the `Rasm.Materials` `DesignBasis` roster whenever the section-altitude owner already names that jurisdiction; a new limit state is one `LimitState` row with its column of cells; a new material family is one `MaterialFamily` row with its codes' cells — the check fold re-reads the table, never a new check method per code and never a parallel verdict family beside `MemberCheck`.
- Boundary: the DESIGN-BASIS VOCABULARY is shared with `Rasm.Materials` `Component/capacity#SECTION_CAPACITY` as one KEY SET carried by two typed rows — that owner's `DesignBasis` and `SafetyFormat` against this page's `DesignCode` and `SafetyFormat` — because the branch strata forbid a reference in either direction and the `[WIRE]: SectionCapacity` seam carries portable scalars keyed by section. Both owners spell the keys identically — `aisc360`/`aisi-s100`/`en1992`/`en1993`/`en1994`/`en1995`/`en1996`/`tms402`/`nds`/`aci318`/`sdpws` all carry BOTH typed rows (the `sdpws` member row is cell-less by design: wood-structural-panel lateral capacity is section-altitude, so every member-altitude pair reports `NotApplicable` while the key still resolves) — and the carve is that owner's section-and-load-path-only glazing and connection rows alone, so a basis a section verdict names resolves a code row here through the standing `DesignCode.For` lookup and neither side re-spells a jurisdiction. Altitudes stay split, each carrying what its own inputs support: the section-altitude owner holds the published strength tables a geometric seam cannot carry — the RC rebar interaction, the AISI effective width, the EN 1994 composite couple, the EN 1996 `f_xk`/`f_vk0` rows — and the closed `GoverningAction`/`Utilisation` verdict vocabulary; this page holds the slenderness, unbraced-length, and deflection facts a cross-section cannot decide, and its `MemberCheck` carriers report the `(code, state)` cells the seam DOES support, an unserved pair reporting `NotApplicable` rather than a fabricated resistance. `MemberCapacity` is this page's own member-altitude interaction operand carrier and is NOT the seam's `SectionCapacity` union — one name for two shapes across a declared seam is the collision that rename retires.
- Boundary: the design codes are hand-rolled (no .NET package owns the AISC, Eurocode, NDS, ACI, TMS, or AISI design rules), realized as a `(DesignCode, LimitState)` data table of capacity delegates — the canonical `POLICY_VALUES`/`DERIVED_LOGIC` collapse, never a switch ladder and never one family's formulas applied to every material. Timber's EN 1995 route is the Eurocode parallel to the US `nds` route the way `en1993` parallels `aisc360` and `en1992` parallels `aci318` — its design strength is `f_k / γ_M` over the same seam reference strength the `nds` cells read (the `k_mod` service/duration modifier is applied upstream by the `Rasm.Materials` `TimberDesign` owner onto the graph-baked reference, never re-derived here), the `§6.3.2` `k_c` column buckling and `§6.3.3` `k_crit` LTB reading the `E0,05` 5%-fractile modulus the seam mean `YoungsModulus` does not carry directly, and the `§6.1.7` `k_cr` crack factor on shear. Timber's independent shear modulus the EC5 `§6.3.3` LTB reads is the realized seam `MaterialPropertySet.Orthotropic` case (`Composition/material#MATERIAL_PROPERTY`, same `Discipline.Structural`, discriminated by case TYPE), read off the graph as the optional `graph.PropertiesOf(member).Orthotropic` and threaded onto `CapacityContext.ShearModulusSi`, so the LTB `M꜀ᵣ` reads timber's directional `Orthotropic.ShearModulus` when the member carries the case and the derived isotropic `Mechanical.ShearModulus` otherwise — the `Component/timber#TIMBER_FAMILY` contract closed here, never a deferred isotropic approximation. Capacity reads the M7-resolved seam `SectionProperties`, the seam isotropic `Mechanical` strength, and the optional seam `Orthotropic`, so a check never re-derives section geometry, re-resolves a profile, or approximates timber's directional shear; the authoritative family is `DesignCode.Family`, the member's `Classify`-derived family validated against it so a steel code on a concrete member rails `AssessmentInputMissing` rather than computing nonsense; `Classify` reads the seam's realized `Orthotropic` case as the directional declaration it is and falls back to the constitutive modulus band, which resolves only the bands the seam leaves ambiguous — a member the band lands outside its code's family is a typed mismatch, never an admissibility predicate widened until the band's imprecision stops showing. Reinforced-concrete N-M-M capacity is not derivable from the geometric seam section (which carries no rebar) — the concrete cells are the plain-section bound, the reinforced interaction the `Rasm.Materials` `Component/capacity#SECTION_CAPACITY` RC owner's concern, so the `VividOrange` `IForceMomentInteraction` surface is not composed here; cold-formed AISI capacity is the gross-section bound, the effective-width reduction the `Component/steel` `ColdFormedDetail`'s concern. Utilization is `demand/capacity` and the verdict derives downstream from the governing ratio so a member's pass/fail and its reported ratio share one source; applicability is TWO scoped questions the shape keeps apart — `LimitState.Applies` the family-scoped one and cell presence the code-scoped one — so an unserved `(code, state)` pair reports `NotApplicable` and contributes no ratio, never a real demand divided by an infinite capacity into a Satisfied pass; a member whose family no `DesignCode` row serves rails `AssessmentInputMissing`, never a silent skip.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialFamily {
    public static readonly MaterialFamily Steel           = new("steel");
    public static readonly MaterialFamily Concrete        = new("concrete");
    public static readonly MaterialFamily Timber          = new("timber");
    public static readonly MaterialFamily Masonry         = new("masonry");
    public static readonly MaterialFamily ColdFormedSteel = new("cold-formed-steel");

    // Classify reads the SEAM's own evidence before any heuristic: a member carrying the realized Orthotropic case is
    // directional BY DECLARATION (the seam's timber home — an isotropic material never mints that case), so the family
    // is decided by the case TYPE the graph carries. The constitutive modulus band is the residual for an isotropic
    // member the seam declares nothing further about, and it stays a BAND — steel and cold-formed share the
    // high-modulus one, so DesignCode.Family remains the authoritative family and disambiguates them at Check.
    public static MaterialFamily Classify(MaterialPropertySet.Mechanical m, Option<MaterialPropertySet.Orthotropic> directional) =>
        directional.IsSome ? Timber
        : m.YoungsModulus.Si > 150e9 ? Steel
        : m.YoungsModulus.Si > 20e9 ? Concrete
        : m.YoungsModulus.Si > 5e9 ? Timber
        : Masonry;

    // Admits maps a design family onto the bands its members can land in, and the ONLY band collision the seam cannot
    // resolve is steel-versus-cold-formed: both classify Steel at 150 GPa, and DesignCode.Family decides which. Every
    // other family answers for its own band — a masonry member whose modulus (Em = 700..900·f'm) lands in the Timber
    // or Concrete band is a typed material-code mismatch the caller fixes by declaring the material, never a TMS check
    // run against a timber classification because the predicate was relaxed to hide the band's own imprecision.
    public bool Admits(MaterialFamily classified) =>
        this == classified || ((this == Steel || this == ColdFormedSteel) && classified == Steel);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafetyFormat {
    public static readonly SafetyFormat Asd        = new("asd");
    public static readonly SafetyFormat Lrfd       = new("lrfd");
    public static readonly SafetyFormat LimitState = new("limit-state");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignCode {
    public static readonly DesignCode Aisc360  = new("aisc360",   MaterialFamily.Steel,           SafetyFormat.Lrfd,       1.00, AiscH11);
    public static readonly DesignCode En1993   = new("en1993",    MaterialFamily.Steel,           SafetyFormat.LimitState, 1.00, En1993Interaction);
    // EN 1994-1-1 governs a STEEL section acting compositely, so its family is Steel and Classify's modulus band
    // lands the member there — a composite MaterialFamily row would ask the seam to declare something no geometric
    // section carries. Its cells are the BARE-STEEL bound for the same reason the concrete cells are the plain
    // bound: the slab, the studs, and the §6.7.3.2 plastic couple are the Rasm.Materials Component/steel owner's.
    public static readonly DesignCode En1994   = new("en1994",    MaterialFamily.Steel,           SafetyFormat.LimitState, 1.00, En1993Interaction);
    public static readonly DesignCode En1992   = new("en1992",    MaterialFamily.Concrete,        SafetyFormat.LimitState, 1.50, LinearInteraction);
    public static readonly DesignCode Nds      = new("nds",       MaterialFamily.Timber,          SafetyFormat.Asd,        1.00, LinearInteraction);
    public static readonly DesignCode En1995   = new("en1995",    MaterialFamily.Timber,          SafetyFormat.LimitState, 1.25, En1995Interaction);
    public static readonly DesignCode Aci318   = new("aci318",    MaterialFamily.Concrete,        SafetyFormat.Lrfd,       1.00, LinearInteraction);
    public static readonly DesignCode Tms402   = new("tms402",    MaterialFamily.Masonry,         SafetyFormat.LimitState, 1.00, LinearInteraction);
    // EN 1996-1-1 §2.4.3 Table 2.3 bands γM 1.5-3.0 over execution class × unit category; 2.00 is the row's declared
    // reference and matches the Rasm.Materials DesignBasis.En1996 row this key mirrors.
    public static readonly DesignCode En1996   = new("en1996",    MaterialFamily.Masonry,         SafetyFormat.LimitState, 2.00, LinearInteraction);
    public static readonly DesignCode AisiS100 = new("aisi-s100", MaterialFamily.ColdFormedSteel, SafetyFormat.Lrfd,       1.00, AiscH11);
    // Key-set mirror of the Materials sdpws basis — wood-structural-panel lateral capacity is SECTION-altitude
    // (the Materials LateralPanel case owns it), so this row carries NO (code, state) cells and every member-altitude
    // pair reports NotApplicable; the row exists so a basis a lateral verdict names resolves here without re-spelling.
    public static readonly DesignCode Sdpws    = new("sdpws",     MaterialFamily.Timber,          SafetyFormat.Asd,        1.00, LinearInteraction);

    public MaterialFamily Family { get; }
    public SafetyFormat Format { get; }
    public double GammaM { get; }

    [UseDelegateFromConstructor]
    public partial double Interaction(SectionDemand demand, MemberCapacity capacity);

    public static Fin<DesignCode> For(AssessmentRoute route) =>
        TryGet(route.Key, out DesignCode code)
            ? Fin.Succ(code)
            : Fin.Fail<DesignCode>(new ComputeFault.AssessmentInputMissing($"<no-design-code:{route.Key}>"));

    static double AiscH11(SectionDemand d, MemberCapacity c) {
        double axial = c.AxialRatio(d.N);
        double bending = Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps) + Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, Eps);
        return axial >= 0.2 ? axial + 8.0 / 9.0 * bending : axial / 2.0 + bending;
    }
    static double En1993Interaction(SectionDemand d, MemberCapacity c) =>
        c.AxialRatio(d.N) + Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps) + Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, Eps);
    // EN 1995-1-1 §6.3.2(3) combined bending + axial compression: the axial term is SQUARED (σ_c0/(k_c·f_c0))², the
    // bending terms linear with the k_m = 0.7 minor-axis stress-redistribution factor (§6.1.6, rectangular section) —
    // distinct from the steel/concrete linear forms, so timber owns its own interaction (the k_c column-buckling is
    // already folded into c.AxialCompression by the en1995 axial-compression cell). FlexureMinor is +inf for timber
    // (no minor cell), so its term is 0 and a pure in-plane check degrades to (N/Nc)² + My/Mmaj.
    static double En1995Interaction(SectionDemand d, MemberCapacity c) {
        double axial = c.AxialRatio(d.N);
        return axial * axial + Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps) + 0.7 * Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, Eps);
    }
    static double LinearInteraction(SectionDemand d, MemberCapacity c) =>
        c.AxialRatio(d.N) + Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps);
}

// Check rows carry demand selectors over the SIGNED envelope and family applicability. Each sense-selecting row
// reads the extreme its own capacity bounds — tension the positive-N extreme, compression and buckling the negative
// one — while a reversing component collapses to its worst magnitude through Span. Combined/Deflection carry no
// table cell and select no component: they fold specially (signed-corner interaction / FE deflection) in Check.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LimitState {
    public static readonly LimitState AxialTension     = new("axial-tension",     static r => Math.Max(r.Max.N, 0.0),                        static f => f != MaterialFamily.Concrete && f != MaterialFamily.Masonry);
    public static readonly LimitState AxialCompression = new("axial-compression", static r => Math.Max(-r.Min.N, 0.0),                       static _ => true);
    public static readonly LimitState FlexureMajor     = new("flexure-major",     static r => r.Span(static d => d.My),                      static _ => true);
    public static readonly LimitState FlexureMinor     = new("flexure-minor",     static r => r.Span(static d => d.Mz),                      static f => f == MaterialFamily.Steel || f == MaterialFamily.ColdFormedSteel);
    public static readonly LimitState ShearMajor       = new("shear-major",       static r => r.Span(static d => d.Vy),                      static _ => true);
    public static readonly LimitState ShearMinor       = new("shear-minor",       static r => r.Span(static d => d.Vz),                      static _ => true);
    public static readonly LimitState Combined         = new("combined",          static _ => 0.0,                                           static f => f == MaterialFamily.Steel || f == MaterialFamily.ColdFormedSteel || f == MaterialFamily.Timber);
    public static readonly LimitState Deflection       = new("deflection",        static _ => 0.0,                                           static _ => true);

    [UseDelegateFromConstructor]
    public partial double Demand(MemberResponse response);

    [UseDelegateFromConstructor]
    public partial bool Applies(MaterialFamily family);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Everything a capacity cell reads: the M7-baked section, the seam strength, the design family, the code (for its
// factors), and the slenderness inputs (member length, unbraced length, end-fixity effective-length factor) — so
// buckling/LTB are REAL, not a placeholder reduction over a missing length.
// Seam-baked RC shear-link read uses Asw, the link area the Materials capacity screen carries as
// ShearLinkAreaMm2), f_ywd (the link design yield), and V_Rd,max (the section-decidable web-crushing
// ceiling the Materials owner ALONE computes and returns — the ceiling assumes the same cot(θ) the policy
// row defaults, so the pair is consistent by construction). Materials defers V_Rd,s to this forward member
// check by design: the stirrup SPACING is member-scope, not section data.
public readonly record struct RcShearLink(double AswSi, double FywdSi, double VrdMaxSi);

public readonly record struct CapacityContext(
    SectionProperties Section, MaterialPropertySet.Mechanical Strength, Option<MaterialPropertySet.Orthotropic> Directional,
    MaterialFamily Family, DesignCode Code, double Length, double UnbracedLength, double EffectiveLengthFactor,
    Option<RcShearLink> ShearLink = default, double StirrupSpacing = 0.0, double CotTheta = 2.5) {
    public static CapacityContext Of(StructuralMember m, DesignCode code, StructuralPolicy policy) =>
        new(m.Section, m.Strength, m.Directional, m.Family, code, m.Length, m.Length, m.EffectiveLengthFactor,
            m.ShearLink, policy.StirrupSpacing, policy.CotTheta);
    public double Slenderness => EffectiveLengthFactor * UnbracedLength / Math.Max(Section.RadiusOfGyrationMinor.Si, StructuralAnalysis.Eps);
    // §6.3.3 LTB shear-stiffness reads the realized seam Orthotropic case's independent in-plane G (timber's
    // G ≈ E0/16) when a directional material carries it, the isotropic Mechanical derived G = E/(2(1+ν)) otherwise —
    // so the EC5 lateral-torsional moment reads the directional stiffness off the seam graph, never the ~6× too-stiff
    // isotropic shear for a timber member, while an isotropic member still resolves a finite G.
    public double ShearModulusSi => Directional.Map(static o => o.ShearModulus.Si).IfNone(() => Strength.ShearModulus.Si);
}

// Four interaction operands feed DesignCode.Interaction; each is an axis capacity or +inf when its cell is absent,
// so the ratio is 0 and the absent action does not constrain the interaction. Naming is MEMBER altitude on purpose:
// SectionCapacity is the Rasm.Materials Component/capacity#SECTION_CAPACITY union the [WIRE]: SectionCapacity seam
// carries, and one name for two shapes across a declared seam is the collision this altitude word retires.
public readonly record struct MemberCapacity(double AxialTension, double AxialCompression, double FlexureMajor, double FlexureMinor) {
    // Axial utilization reads the capacity its own SENSE governs — the gross tension cell for a positive N, the
    // slenderness-reduced compression cell for a negative one — so a tension-governed interaction is never divided by
    // a buckling-reduced capacity and a compression-governed one never by the gross tension cell.
    public double AxialRatio(double n) => n >= 0.0 ? n / Math.Max(AxialTension, Eps) : -n / Math.Max(AxialCompression, Eps);
}

// Unserved (code, state) pairs are NOT a zero-utilization pass: LimitState.Applies answers the
// FAMILY-scoped question and cell absence the CODE-scoped one, so an unserved pair carries None and reports the
// NotApplicable verdict rather than dividing a real demand by +inf into a Satisfied 0.0 — the shape that published a
// TMS 402 shear check on a masonry member as a clean pass. Combined/Deflection carry None capacity by construction
// (they read no table cell) and Some utilization, so the two absences stay distinguishable.
// `[Equatable]` is a capability ADD — the VARIANT DIFF: two design iterations' check sets compare through the
// generated `Inequalities`, whose MemberPath names exactly the member and column that moved between variants.
// `[PrecisionEquality]` bands the demand at the solver noise floor and therefore leaves GetHashCode — a
// MemberCheck is NEVER a dictionary key; the Option-carried capacity and utilization compare by value.
[Equatable]
public readonly partial record struct MemberCheck(NodeId Member, LimitState State, [property: PrecisionEquality(1e-9)] double Demand, Option<double> Capacity, Option<double> Utilization) {
    public AssessmentVerdict Verdict => Utilization.Match(
        Some: AssessmentVerdict.FromRatio, None: static () => AssessmentVerdict.NotApplicable);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    // (DesignCode, LimitState) capacity table collapses policy values with derived logic. Each cell is
    // governing formula for that code's material model in SI base units (Pa stress, m^2 area, m^3 modulus, m^4
    // inertia -> N / N*m). Absent (code, state) pairs are not-applicable. Steel buckling/LTB read the context
    // slenderness; concrete cells are the PLAIN-section bound (rebar is the Rasm.Materials RC owner's input); AISI
    // cells are the GROSS bound (effective width is the Rasm.Materials cold-formed owner's input).
    static readonly FrozenDictionary<(string Code, string State), Func<CapacityContext, double>> Capacities = Seed();

    static FrozenDictionary<(string, string), Func<CapacityContext, double>> Seed() =>
        new ((string Code, string State) Key, Func<CapacityContext, double> Rule)[] {
            // --- AISC 360 (steel, LRFD) -----------------------------------------------------
            (("aisc360", "axial-tension"),     static c => 0.90 * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("aisc360", "axial-compression"), static c => 0.90 * AiscFcr(c) * c.Section.Area.Si),
            (("aisc360", "flexure-major"),     static c => 0.90 * AiscMn(c)),
            (("aisc360", "flexure-minor"),     static c => 0.90 * Math.Min(c.Strength.YieldStrength.Si * c.Section.Wplz.Si, 1.6 * c.Strength.YieldStrength.Si * c.Section.Welz.Si)),
            (("aisc360", "shear-major"),       static c => 1.00 * 0.60 * c.Strength.YieldStrength.Si * c.Section.AvY.Si),
            (("aisc360", "shear-minor"),       static c => 1.00 * 0.60 * c.Strength.YieldStrength.Si * c.Section.AvZ.Si),
            // --- EN 1993-1-1 (steel, limit-state, gammaM=1.0) -------------------------------
            (("en1993", "axial-tension"),      static c => c.Strength.YieldStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            (("en1993", "axial-compression"),  static c => EnChi(EnLambdaBar(c), 0.34) * c.Section.Area.Si * c.Strength.YieldStrength.Si / c.Code.GammaM),
            (("en1993", "flexure-major"),      static c => EnChiLt(c) * c.Section.Wply.Si * c.Strength.YieldStrength.Si / c.Code.GammaM),
            (("en1993", "flexure-minor"),      static c => c.Section.Wplz.Si * c.Strength.YieldStrength.Si / c.Code.GammaM),
            (("en1993", "shear-major"),        static c => c.Section.AvY.Si * c.Strength.YieldStrength.Si / (Math.Sqrt(3.0) * c.Code.GammaM)),
            (("en1993", "shear-minor"),        static c => c.Section.AvZ.Si * c.Strength.YieldStrength.Si / (Math.Sqrt(3.0) * c.Code.GammaM)),
            // --- EN 1994-1-1 (composite steel-concrete, limit-state) — the BARE-STEEL bound ------
            // Composite-member slab width, stud count, and §6.7.3.2 plastic couple live on the Rasm.Materials
            // Component/steel CompositeDetail, which bakes the composite resistance into the section-altitude
            // receipt; the geometric seam here carries the steel section alone, so these cells hold the §6.2 steel
            // resistances the composite couple can only exceed — the same standing bound the aci318/en1992 plain
            // cells and the aisi-s100 gross cells already state, never a fabricated slab.
            (("en1994", "axial-tension"),      static c => c.Strength.YieldStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            (("en1994", "axial-compression"),  static c => EnChi(EnLambdaBar(c), 0.34) * c.Section.Area.Si * c.Strength.YieldStrength.Si / c.Code.GammaM),
            (("en1994", "flexure-major"),      static c => EnChiLt(c) * c.Section.Wply.Si * c.Strength.YieldStrength.Si / c.Code.GammaM),
            (("en1994", "flexure-minor"),      static c => c.Section.Wplz.Si * c.Strength.YieldStrength.Si / c.Code.GammaM),
            (("en1994", "shear-major"),        static c => c.Section.AvY.Si * c.Strength.YieldStrength.Si / (Math.Sqrt(3.0) * c.Code.GammaM)),
            (("en1994", "shear-minor"),        static c => c.Section.AvZ.Si * c.Strength.YieldStrength.Si / (Math.Sqrt(3.0) * c.Code.GammaM)),
            // --- AISI S100 (cold-formed steel, LRFD) — the section moduli on a cold-formed member's seam
            // SectionProperties ARE the Materials capacity owner's Seff-derived EFFECTIVE values (the
            // stress-aware effective-width derivation lives at steel#STEEL_FAMILY DesignCapacity, its owner
            // by strata law), so the same cells read gross for hot-rolled and effective for cold-formed with
            // ZERO Compute-side dispatch — gross-vs-effective is invisible here, and a Compute-side
            // SectionProfile overload dispatch or a ComputedSection bake is the strata-forbidden form.
            (("aisi-s100", "axial-tension"),     static c => 0.90 * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("aisi-s100", "axial-compression"), static c => 0.85 * AiscFcr(c) * c.Section.Area.Si),
            (("aisi-s100", "flexure-major"),     static c => 0.90 * c.Strength.YieldStrength.Si * c.Section.Wely.Si),
            (("aisi-s100", "flexure-minor"),     static c => 0.90 * c.Strength.YieldStrength.Si * c.Section.Welz.Si),
            (("aisi-s100", "shear-major"),       static c => 0.95 * 0.60 * c.Strength.YieldStrength.Si * c.Section.AvY.Si),
            (("aisi-s100", "shear-minor"),       static c => 0.95 * 0.60 * c.Strength.YieldStrength.Si * c.Section.AvZ.Si),
            // --- ACI 318 (concrete, LRFD) — plain-section bound ----------------------------
            (("aci318", "axial-compression"),  static c => 0.65 * 0.85 * c.Strength.UltimateStrength.Si * c.Section.Area.Si),
            (("aci318", "flexure-major"),      static c => 0.90 * ConcreteFr(c.Strength.UltimateStrength.Si) * c.Section.Wely.Si),
            (("aci318", "shear-major"),        static c => 0.75 * ConcreteVc(c.Strength.UltimateStrength.Si) * c.Section.AvY.Si),
            (("aci318", "shear-minor"),        static c => 0.75 * ConcreteVc(c.Strength.UltimateStrength.Si) * c.Section.AvZ.Si),
            // --- EN 1992-1-1 (concrete, gammaC=1.5) — plain-section bound -------------------
            (("en1992", "axial-compression"),  static c => 0.85 * c.Strength.UltimateStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            (("en1992", "flexure-major"),      static c => ConcreteFr(c.Strength.UltimateStrength.Si) * c.Section.Wely.Si / c.Code.GammaM),
            // EN 1992 §6.2 truss-model pairing: a LINKED section (Asw baked by the Materials capacity owner,
            // stirrup spacing s the member-scope policy input Materials defers by design) governs at
            // min(V_Rd,s, V_Rd,max) with V_Rd,s = (Asw/s)·z·f_ywd·cot(θ), z = 0.9d, cot(θ) the policy row
            // defaulting 2.5 (the same assumption the Materials V_Rd,max ceiling carries, so the pair is
            // consistent by construction); the linkless arm keeps the plain V_Rd,c concrete resistance.
            (("en1992", "shear-major"),        static c => c.ShearLink.Filter(_ => c.StirrupSpacing > 0.0).Match(
                Some: link => Math.Min(link.AswSi / c.StirrupSpacing * 0.9 * c.Section.Depth.Si * link.FywdSi * c.CotTheta, link.VrdMaxSi),
                None: () => ConcreteVc(c.Strength.UltimateStrength.Si) * c.Section.AvY.Si / c.Code.GammaM)),
            (("en1992", "shear-minor"),        static c => c.ShearLink.Filter(_ => c.StirrupSpacing > 0.0).Match(
                Some: link => Math.Min(link.AswSi / c.StirrupSpacing * 0.9 * c.Section.Width.Si * link.FywdSi * c.CotTheta, link.VrdMaxSi),
                None: () => ConcreteVc(c.Strength.UltimateStrength.Si) * c.Section.AvZ.Si / c.Code.GammaM)),
            // --- NDS (timber, ASD) — reference values adjusted by CP/CL --------------------
            (("nds", "axial-tension"),         static c => c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("nds", "axial-compression"),     static c => NdsCp(c) * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("nds", "flexure-major"),         static c => NdsCl(c) * c.Strength.YieldStrength.Si * c.Section.Wely.Si),
            // Timber rolling/horizontal shear is axis-independent for a rectangular sawn/glulam section (full-area 2/3·Fv·A), so both axes share the formula.
            (("nds", "shear-major"),           static c => (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("nds", "shear-minor"),           static c => (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            // --- EN 1995-1-1 (timber, limit-state, gammaM=1.25) — the EC5 parallel to the NDS rows ---
            // EN timber route parallels NDS as en1993 parallels aisc360 and en1992 parallels aci318:
            // design strength = f_k / gammaM (§2.4.1; the k_mod service/duration modifier is a Rasm.Materials TimberDesign
            // input on the graph-baked reference strength, NOT re-derived here — the seam strength is the already-modified
            // reference stress, mirroring the NDS cells' use of YieldStrength). The seam neutral Mechanical carries the
            // timber reference strength on YieldStrength (the Rasm.Materials projector maps f_c0,k/f_m,k onto it, the same
            // contract the NDS cells read) and the mean E0 on YoungsModulus; the EN 338/14080 5%-fractile stability modulus
            // E0,05 the §6.3.2 buckling needs is ~0.67·E0,mean for softwood (Ec5E005), so the slender-column and LTB checks
            // read a fractile-correct modulus rather than the mean. Tension is the net-section reference; compression is
            // §6.3.2 k_c-reduced reference uses the Ylinen-shaped EN buckling curve; flexure-major is the §6.3.3 k_crit-reduced
            // reference over the elastic modulus; shear is the §6.1.7 k_cr = 0.67 crack-reduced full-area 2/3·f_v·A (the
            // CLT rolling-shear governing a panel is the Rasm.Materials TimberDesign owner's concern, baked into the
            // reference strength upstream — the geometric seam section cannot see the ply layup here).
            (("en1995", "axial-tension"),      static c => c.Strength.YieldStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            (("en1995", "axial-compression"),  static c => Ec5Kc(c) * c.Strength.YieldStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            (("en1995", "flexure-major"),      static c => Ec5Kcrit(c) * c.Strength.YieldStrength.Si * c.Section.Wely.Si / c.Code.GammaM),
            (("en1995", "shear-major"),        static c => 0.67 * (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            (("en1995", "shear-minor"),        static c => 0.67 * (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si / c.Code.GammaM),
            // --- TMS 402 (masonry, allowable) — slenderness-reduced ------------------------
            (("tms402", "axial-compression"),  static c => 0.25 * c.Strength.UltimateStrength.Si * TmsSlender(c) * c.Section.Area.Si),
            (("tms402", "flexure-major"),      static c => (1.0 / 3.0) * c.Strength.UltimateStrength.Si * c.Section.Wely.Si),
            // --- EN 1996-1-1 (masonry, gammaM per Table 2.3) — the EU parallel to the tms402 rows ---
            // §6.1.2.1 N_Rd = Phi·A·f_d over the §6.1.2.2 capacity-reduction factor, the seam UltimateStrength
            // carrying f_k exactly as it carries f'm for the tms402 row. ONE cell and no more: EN unreinforced
            // flexure is governed by f_xk and shear by f_vk0 + 0.4·sigma_d, and BOTH strengths are published tables
            // keyed by unit group and mortar class that no geometric seam carries — they live on the Rasm.Materials
            // Component/masonry FlexuralStrengthEn row and resolve at section altitude, so the absent cells report
            // NotApplicable here rather than dividing a real demand by a fabricated resistance.
            (("en1996", "axial-compression"),  static c => EnMasonryPhi(c) * c.Strength.UltimateStrength.Si * c.Section.Area.Si / c.Code.GammaM),
        }.ToFrozenDictionary(static row => row.Key, static row => row.Rule);

    // Cell absence is the CODE-scoped not-applicable answer, carried as None so a check can report it rather than
    // dividing by a +inf the verdict then bands Satisfied. The interaction operands lift the same None to +inf, where
    // an absent axis genuinely contributes a zero term.
    static Option<double> Capacity(DesignCode code, LimitState state, CapacityContext ctx) =>
        Capacities.TryGetValue((code.Key, state.Key), out Func<CapacityContext, double> rule) ? Some(rule(ctx)) : None;

    // --- [STRENGTH_KERNELS] --------------------------------------------------------------
    static double AiscFcr(CapacityContext c) {                                   // AISC 360 E3
        double fy = c.Strength.YieldStrength.Si, fe = Math.PI * Math.PI * c.Strength.YoungsModulus.Si / Math.Max(c.Slenderness * c.Slenderness, Eps);
        return fy / fe <= 2.25 ? Math.Pow(0.658, fy / fe) * fy : 0.877 * fe;
    }
    static double AiscMn(CapacityContext c) {                                    // AISC 360 F2 (Mp / inelastic / elastic LTB)
        double fy = c.Strength.YieldStrength.Si, e = c.Strength.YoungsModulus.Si, ry = c.Section.RadiusOfGyrationMinor.Si, lb = c.UnbracedLength;
        double mp = fy * c.Section.Wply.Si, lp = 1.76 * ry * Math.Sqrt(e / Math.Max(fy, Eps)), lr = Math.PI * ry * Math.Sqrt(e / Math.Max(0.7 * fy, Eps));
        double mr = 0.7 * fy * c.Section.Wely.Si;
        return lb <= lp ? mp
            : lb <= lr ? Math.Min(mp, mp - (mp - mr) * (lb - lp) / Math.Max(lr - lp, Eps))
            : Math.Min(mp, Math.PI * Math.PI * e / Math.Max((lb / ry) * (lb / ry), Eps) * c.Section.Wely.Si);
    }
    static double EnLambdaBar(CapacityContext c) {                               // EN 1993 6.3.1 non-dimensional slenderness
        double i = c.Section.RadiusOfGyrationMinor.Si, l = c.EffectiveLengthFactor * c.UnbracedLength;
        double ncr = Math.PI * Math.PI * c.Strength.YoungsModulus.Si * c.Section.Area.Si * i * i / Math.Max(l * l, Eps);
        return Math.Sqrt(c.Section.Area.Si * c.Strength.YieldStrength.Si / Math.Max(ncr, Eps));
    }
    static double EnChi(double lambdaBar, double alpha) {                        // EN 1993 6.3.1 buckling-curve reduction
        double phi = 0.5 * (1.0 + alpha * (lambdaBar - 0.2) + lambdaBar * lambdaBar);
        return Math.Min(1.0, 1.0 / (phi + Math.Sqrt(Math.Max(phi * phi - lambdaBar * lambdaBar, Eps))));
    }
    static double EnChiLt(CapacityContext c) {                                   // EN 1993 6.3.2 LTB (warping-free Mcr, curve c)
        double mcr = Math.PI / Math.Max(c.UnbracedLength, Eps) * Math.Sqrt(Math.Max(c.Strength.YoungsModulus.Si * c.Section.Izz.Si * c.Strength.ShearModulus.Si * c.Section.J.Si, 0.0));
        return EnChi(Math.Sqrt(c.Section.Wply.Si * c.Strength.YieldStrength.Si / Math.Max(mcr, Eps)), 0.49);
    }
    static double NdsCp(CapacityContext c) {                                     // NDS column stability (Ylinen)
        double fcStar = c.Strength.YieldStrength.Si, slender = c.EffectiveLengthFactor * c.UnbracedLength / Math.Max(c.Section.LeastDimension.Si, Eps);
        double ratio = 0.822 * c.Strength.YoungsModulus.Si / Math.Max(slender * slender, Eps) / Math.Max(fcStar, Eps), term = (1.0 + ratio) / 1.6;
        return term - Math.Sqrt(Math.Max(term * term - ratio / 0.8, 0.0));
    }
    static double NdsCl(CapacityContext c) {                                     // NDS beam stability
        double fbStar = c.Strength.YieldStrength.Si, rb2 = c.UnbracedLength * c.Section.Depth.Si / Math.Max(c.Section.Width.Si * c.Section.Width.Si, Eps);
        double ratio = 1.20 * c.Strength.YoungsModulus.Si / Math.Max(rb2, Eps) / Math.Max(fbStar, Eps), term = (1.0 + ratio) / 1.9;
        return term - Math.Sqrt(Math.Max(term * term - ratio / 0.95, 0.0));
    }
    // EN 338/14080 5%-fractile axial modulus: the stability checks (§6.3.2 column, §6.3.3 LTB) read E0,05, not the mean
    // E0 the seam Mechanical carries — E0,05 ≈ 0.67·E0,mean for softwood (the EN 338 ratio), the canonical Rasm timber
    // factor, so a slender member's buckling reads a fractile-correct modulus rather than the unconservative mean.
    static double Ec5E005(CapacityContext c) => 0.67 * c.Strength.YoungsModulus.Si;
    static double Ec5Kc(CapacityContext c) {                                     // EN 1995-1-1 §6.3.2 column buckling
        double slender = c.Slenderness, fc0 = c.Strength.YieldStrength.Si;
        double sigmaCrit = slender > Eps ? Math.PI * Math.PI * Ec5E005(c) / (slender * slender) : double.PositiveInfinity;
        double lambdaRel = Math.Sqrt(fc0 / Math.Max(sigmaCrit, Eps));
        // beta_c imperfection factor: 0.1 glulam/LVL/CLT, 0.2 solid sawn — the seam family cannot see the product form,
        // so the conservative 0.2 solid-timber value (the lower k_c) is used here; the Rasm.Materials TimberDesign owner
        // (which holds the TimberForm) applies the form-specific 0.1 to the reference strength when the form is known.
        double k = 0.5 * (1.0 + 0.2 * (lambdaRel - 0.3) + lambdaRel * lambdaRel);
        return lambdaRel <= 0.3 ? 1.0 : 1.0 / (k + Math.Sqrt(Math.Max(k * k - lambdaRel * lambdaRel, 0.0)));
    }
    static double Ec5Kcrit(CapacityContext c) {                                  // EN 1995-1-1 §6.3.3 lateral-torsional beam stability
        // sigma_m,crit = pi·sqrt(E0,05·Iz·G·Itor) / (Lef·Wy) (§6.3.3(2), warping-free) over the elastic modulus; G is
        // c.ShearModulusSi — timber's INDEPENDENT in-plane shear (G ≈ E0/16) read off the realized seam Orthotropic case
        // (Composition/material#MATERIAL_PROPERTY, props.Orthotropic) when the directional material carries it, the
        // derived isotropic shear only when it does not, so a timber beam's LTB reads the ~6× softer directional G rather
        // than the unconservative isotropic value. lambda_rel,m = sqrt(f_m,k / sigma_m,crit); the three-band k_crit reduction.
        double mcr = Math.PI * Math.Sqrt(Math.Max(Ec5E005(c) * c.Section.Izz.Si * c.ShearModulusSi * c.Section.J.Si, 0.0)) / Math.Max(c.UnbracedLength * c.Section.Wely.Si, Eps);
        double lambdaRelM = Math.Sqrt(c.Strength.YieldStrength.Si / Math.Max(mcr, Eps));
        return lambdaRelM <= 0.75 ? 1.0 : lambdaRelM <= 1.4 ? 1.56 - 0.75 * lambdaRelM : 1.0 / Math.Max(lambdaRelM * lambdaRelM, Eps);
    }
    static double TmsSlender(CapacityContext c) =>                               // TMS 402 slenderness reduction
        Math.Max(0.0, 1.0 - Math.Pow(c.UnbracedLength / (140.0 * Math.Max(c.Section.RadiusOfGyrationMinor.Si, Eps)), 2.0));
    // EN 1996-1-1 §6.1.2.2 capacity-reduction factor over the §5.5.1.1 initial eccentricity e_init = h_ef/450: for the
    // solid rectangle whose i = t/√12, Phi = 1 − (2/(450·√12))·(h/i), so it reads the same slenderness the TMS bracket
    // does. The Rasm.Materials MasonryReduction owner mints the SAME derivation at section altitude, so a wall checked
    // on both rails is reduced identically and the two verdicts differ only by the resistances each altitude carries.
    static double EnMasonryPhi(CapacityContext c) =>
        Math.Max(0.0, 1.0 - 2.0 / (450.0 * Math.Sqrt(12.0)) * c.UnbracedLength / Math.Max(c.Section.RadiusOfGyrationMinor.Si, Eps));
    static double ConcreteFr(double fc) => 0.62 * Math.Sqrt(Math.Max(fc / 1e6, 0.0)) * 1e6;   // modulus of rupture, Pa
    static double ConcreteVc(double fc) => 0.17 * Math.Sqrt(Math.Max(fc / 1e6, 0.0)) * 1e6;   // concrete shear stress, Pa

    // --- [GOVERNING] ---------------------------------------------------------------------
    // Static route resolves its design code from the ROUTE, so the join stays here where a structural route and a
    // DesignCode row are the same standard under one key; the seismic overload below takes its capacity code off
    // the request instead, because there the route names the ACTION standard.
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, IClock clock) =>
        from code   in DesignCode.For(request.Route)
        from model  in Project(graph, FrameInputs.Of(request), geometry)
        from _      in Validate(model, code)
        from resp   in Solve(model, clock)
        from blob   in sink.Store(Artifact(resp, graph.Header.Tolerance, None))
        let checks   = model.Members.Bind(m => Check(m, resp[m.Id], code, model.Policy))
        from folded in CheckFacts(checks)
        select AssessmentResult.Of(
            request.Route,
            folded.Facts,
            folded.Governing,
            new Provenance("StructuralAnalysis", request.Route.Standard, request.Route.SolverVersion, clock.GetCurrentInstant()),
            Some(blob));

    // One projection for both routes: a check with a resolved utilization emits its ratio, and a check whose (code,
    // state) cell the table does not serve emits the NotApplicable verdict as a text fact — never a ratio the finite
    // gate would have to invent and never a 0.0 the spine bands Satisfied. The governing fold reads the resolved
    // checks alone, so an unserved pair can neither win nor dilute the max.
    static Fin<(Seq<AssessmentFact> Facts, double Governing)> CheckFacts(Seq<MemberCheck> checks) {
        Seq<(MemberCheck Check, double Utilization)> resolved =
            toSeq(checks.Choose(static c => c.Utilization.Map(u => (Check: c, Utilization: u))).OrderByDescending(static row => row.Utilization));
        Option<(MemberCheck Check, double Utilization)> govern = resolved.Head;
        Seq<AssessmentFact> inapplicable = checks.Filter(static c => c.Utilization.IsNone)
            .Map(static c => AssessmentFact.Text($"{c.Member.Value}/{c.State.Key}", AssessmentVerdict.NotApplicable.Key));
        return from ratios in resolved.TraverseM(static row => AssessmentFact.Ratio($"{row.Check.Member.Value}/{row.Check.State.Key}", row.Utilization)).As()
               from maxU in AssessmentFact.Ratio("max-utilization", govern.Map(static g => g.Utilization).IfNone(0.0))
               select (ratios + inapplicable + Seq(
                        maxU,
                        govern.Map(static g => AssessmentFact.Reference("governing-member", g.Check.Member)).IfNone(AssessmentFact.Text("governing-member", "none")),
                        AssessmentFact.Text("governing-limit-state", govern.Map(static g => g.Check.State.Key).IfNone("none"))),
                    govern.Map(static g => g.Utilization).IfNone(0.0));
    }

    static Fin<Unit> Validate(FrameModel model, DesignCode code) =>
        model.Members.Find(m => !code.Family.Admits(m.Family))
            .Match(Some: m => Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing($"<material-code-mismatch:{m.Id.Value}:{m.Family.Key}!={code.Family.Key}>")),
                   None: () => Fin.Succ(unit));

    static Seq<MemberCheck> Check(StructuralMember member, MemberResponse response, DesignCode code, StructuralPolicy policy) {
        CapacityContext ctx = CapacityContext.Of(member, code, policy);
        // Interactions over an ABSENT operand are unstatable: the retired +∞ stand-in read demand/∞ = 0 and
        // silently DROPPED that axis from the combined verdict — the inverse-polarity twin of the sentinel class
        // deleted by the Materials Worst fold. Combined therefore gates on ALL FOUR operands, and one unserved
        // operand lands the same NotApplicable fact every absent per-state cell already takes.
        Option<MemberCapacity> caps =
            from tension in Capacity(code, LimitState.AxialTension, ctx)
            from compression in Capacity(code, LimitState.AxialCompression, ctx)
            from flexureMajor in Capacity(code, LimitState.FlexureMajor, ctx)
            from flexureMinor in Capacity(code, LimitState.FlexureMinor, ctx)
            select new MemberCapacity(tension, compression, flexureMajor, flexureMinor);
        return LimitState.Items.ToSeq().Filter(state => state.Applies(member.Family)).Map(state => {
            Option<double> capacity = Capacity(code, state, ctx);
            double demand = state.Demand(response);
            // Combined evaluates BOTH signed corner states and governs on the worse, so a member whose tension corner
            // interacts hardest is never scored on its compression corner alone.
            Option<double> util =
                state == LimitState.Combined ? caps.Map(operands => Math.Max(
                    code.Interaction(response.TensionCorner, operands), code.Interaction(response.CompressionCorner, operands)))
                : state == LimitState.Deflection ? Some(response.MaxDeflection / Math.Max(policy.DeflectionLimitRatio * member.Length, Eps))
                : capacity.Map(value => demand / Math.Max(value, Eps));
            return new MemberCheck(member.Id, state, demand, capacity, util);
        });
    }

    // ONE artifact shape for BOTH routes over Runtime/codecs#HDF_ARCHIVE, absorbing the opaque CanonicalWriter byte
    // stream no reader can slice: `/demands` rows one member each — min/max SectionDemand sextets with the
    // deflection, 13 columns — with the ordinal-sorted member roster an attribute, and the modal route adds
    // `/modes` `[modes, dofs]` chunked `[1, dofs]` MODE-OUTERMOST (one chunk per mode shape, so a checking or
    // viz consumer reads one mode without the pencil) beside `/periods`. Values quantize to the graph tolerance
    // so the stored bytes keep the canonical-identity discipline the writer they replace carried.
    static ReadOnlyMemory<byte> Artifact(
        FrozenDictionary<NodeId, MemberResponse> responses, double tolerance,
        Option<(ReadOnlyMemory<double> Shapes, int Modes, int Dofs, Seq<double> Periods)> modal) {
        double Q(double value) => tolerance > 0.0 ? Math.Round(value / tolerance) * tolerance : value;
        Seq<(NodeId Id, MemberResponse Response)> ordered =
            toSeq(responses.OrderBy(static row => row.Key.Value, StringComparer.Ordinal)).Map(static row => (Id: row.Key, Response: row.Value));
        double[,] demands = new double[ordered.Count, 13];
        for (int row = 0; row < ordered.Count; row++) {
            MemberResponse response = ordered[row].Response;
            int column = 0;
            foreach (SectionDemand extreme in Seq(response.Min, response.Max)) {
                demands[row, column++] = Q(extreme.N);
                demands[row, column++] = Q(extreme.Vy);
                demands[row, column++] = Q(extreme.Vz);
                demands[row, column++] = Q(extreme.My);
                demands[row, column++] = Q(extreme.Mz);
                demands[row, column++] = Q(extreme.T);
            }

            demands[row, column] = Q(response.MaxDeflection);
        }

        H5DatasetCreation creation = HdfArchivePolicy.Interchange.Creation();
        H5File graph = new() { ["demands"] = new H5Dataset<double[,]>(demands, chunks: [1u, 13u], datasetCreation: creation) };
        graph.Attributes["members"] = ordered.Map(static row => row.Id.Value).ToArray();
        Option<H5Dataset<double[]>> modeSlot = modal.Map(m => {
            H5Dataset<double[]> slot = new(fileDims: [(ulong)m.Modes, (ulong)m.Dofs], chunks: [1u, (uint)m.Dofs], datasetCreation: creation);
            graph["modes"] = slot;
            graph["periods"] = new H5Dataset<double[]>(m.Periods.ToArray(), chunks: [(uint)Math.Max(1, m.Periods.Count)], datasetCreation: creation);
            return slot;
        });
        using MemoryStream staged = new();
        using (HdfWriter session = HdfArchive.Begin(graph, staged, HdfArchivePolicy.Interchange)) {
            // Column-major mode shapes: mode k's dofs are contiguous, so each chunk write is one slice copy.
            if (modal.Case is (ReadOnlyMemory<double> shapes, int modes, int dofs, Seq<double> _) && modeSlot.Case is H5Dataset<double[]> slot) {
                for (int mode = 0; mode < modes; mode++) {
                    session.WriteChunk(slot, shapes.Span.Slice(mode * dofs, dofs).ToArray(), mode, grid: [modes, 1], chunkShape: [1u, (uint)dofs]);
                }
            }
        }

        return staged.ToArray();
    }
}
```

## [05]-[SEISMIC_ROUTE]

- Owner: `DesignSpectrum` `[SmartEnum<string>]` the code design-spectrum rows — EN 1998-1 Type 1, EN 1998-1 Type 2, ASCE 7 — each row carrying its piecewise pseudo-acceleration ordinate as a delegate over the `SpectrumPolicy` parameters AND its own `GroundShape` table, NEVER a hardcoded curve; `GroundShape` the per-ground-type `(S, T_B, T_C, T_D)` row a Eurocode spectrum resolves off the policy's site class; `SpectrumPolicy` the site/ground-motion/behaviour/damping parameter record; `ExcitationAxis` `[SmartEnum<string>]` the direction the request excites, each row carrying its own projection off the per-axis `ModalParticipation`; `ModalCombination` `[SmartEnum<string>]` the modal-combination axis (`srss` · `cqc` — CQC the closely-spaced-mode default) and `ModalCorrelation` the once-per-solve cross-modal matrix both rows fold through; `SeismicSpec` the request payload carrying the spectrum row, its policy, the excitation axis, the combination row, the participation floor, and the CAPACITY `DesignCode` the member checks run under; `Run` the seismic overload folding the condensed modal pencil.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Seismic request, GeometrySource geometry, AssessmentSink sink, IClock clock)` — the chain is fully named: `FrameLowering.Lower` builds the same mesh the static route uses and lowers it as `PhysicsKind.FeaModal` so the lane routes its eigen arm, `SolveLane.Solve` under `SolvePolicy.CanonicalModalCondensed` (the `condensed-evd` row) condenses the frame's inertia-free rotational rows out of the pencil and recovers full-length `(φ, λ)` with the per-axis `ModalParticipation` factors off the owned lumped-mass field, the 90% effective-mass floor gates TYPED AND PER AXIS — `Σ Γ_d² / TotalMass_d` for the axis `spec.Direction` names, an achieved fraction below `spec.ParticipationFloor` `ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, "<modal-mass-shortfall:…>")` naming the axis and the fraction, never a silent truncation — the per-mode spectral demand scales by that same axis's `Γ_d` and reads `Sa(T_i)` off the `DesignSpectrum` row, the modal responses combine through ONE `ModalCorrelation` built for the solve, and the combined demands check through the SAME `(DesignCode, LimitState)` capacity table under `spec.Capacity`; the achieved participation, the excitation axis, and the combination key ride the fact stream, the receipt's `Participation`/`Combination` columns projecting the first and last of the three, and the reduction's own measured evidence — retained-row count, reduction residual, pencil conditioning — rides that same stream as three ratio facts. `sink.Store` archives the modal basis on the SAME artifact shape the static route writes — `/demands` member rows with `/modes` `[modes, dofs]` chunked `[1, dofs]` mode-outermost beside `/periods` — so static and seismic converge on one sliceable container and the opaque byte stream is absorbed.
- Packages: the route composes `Solver/contract` (`SolveLane`, `SolvePolicy.CanonicalModalCondensed`, `ModalParticipation`), `Solver/discretization` (the frame rows), PureHDF (`H5Dataset<T>` — the mode-outermost `/modes` chunks on the shared artifact), Generator.Equals (`[Equatable]`+`[OrderedEquality]` — the ModalCorrelation latent-trap repair), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary` the ground-type tables, `ImmutableArray` the correlation matrix) — zero new packages beyond the folder roster (the seam `Discipline.Seismic` row and `StructuralCase.Seismic` already exist).
- Growth: a new code spectrum is one `DesignSpectrum` row carrying its ordinate delegate and its ground-type table; a new combination rule is one `ModalCombination` row minting its own `ModalCorrelation`; a ground-type refinement is one row in the spectrum's own table; a new excitation direction is one `ExcitationAxis` row; zero new surface — a `SeismicAnalyzer` sibling runner is the rejected form (this is a structural ROUTE over the existing spine).
- Boundary: the building-scale modal route is the `condensed-evd` `SolveMethod` row — the reduction's necessity, its exactness over a lumped-mass frame, and the refuted eigensolver substrates are settled law at `RULINGS.md` `[02]`. Spectrum rows are POLICY DATA (the codes the seam `Seismic` row itself names) and a hardcoded curve, a per-code method ladder, or a spectrum baked into the runner is the deleted form; two rows differing only in their KEY are one row — EN 1998-1 Type 1 and Type 2 share one piecewise ordinate and differ in the Table 3.2/3.3 ground-type shapes alone, so the shape table is the row's own data and an unresolvable ground type rails at `Admit` before the demand fold rather than folding a defaulted shape into every member's check. CQC is the closely-spaced default because SRSS under-combines correlated modes — the choice is a ROW the receipt records, never a silent internal pick — and its cross-modal correlation depends only on the mode frequencies and the damping ratio, so it is built ONCE per solve and read across every member and component, a per-component rebuild being `O(k²)` work multiplied by the member count for a matrix the solve already determined. Participation gating runs PER EXCITATION AXIS because a mass-participation total summed across axes reads healthy while the direction the request excites is unrepresented, and because a torsional mode carries no translational `Γ_d` it contributes to neither the axis fraction nor the axis's spectral demand; the shortfall is a typed `(Solve, Numeric)` fault (deterministic — it caches as a Failed node under the lifecycle-spine law and never re-runs blind). Per the `RULINGS.md` `[02]` seismic action/capacity split, the capacity `DesignCode` rides the request.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// EN 1998-1 Table 3.2/3.3 ground-type shape: the S soil factor and the TB/TC/TD corner periods. Mapped is the
// neutral row a code parameterized by mapped spectral accelerations (ASCE 7) carries instead of a ground-type shape.
public readonly record struct GroundShape(double S, double Tb, double Tc, double Td) {
    public static readonly GroundShape Mapped = new(1.0, 0.0, 0.0, 0.0);
}

// Excitation direction is a ROW carrying its own projection off the per-axis ModalParticipation, so the mass
// gate, the spectral scale, and the emitted evidence read ONE selector and a fourth consumer cannot pick a
// different axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExcitationAxis {
    public static readonly ExcitationAxis X = new("x", static p => p.X);
    public static readonly ExcitationAxis Y = new("y", static p => p.Y);
    public static readonly ExcitationAxis Z = new("z", static p => p.Z);

    [UseDelegateFromConstructor]
    public partial double Of(ModalParticipation participation);
}

// Code design spectra as POLICY ROWS: each row owns its piecewise pseudo-acceleration ordinate over the
// SpectrumPolicy parameters and its resolved GroundShape — EN 1998-1 §3.2.2.5 (η damping correction; behavior factor
// q) and ASCE 7 §11.4 (SDS/SD1 plateau-and-decay; R/Ie) — never a hardcoded curve. Type 1 and Type 2 share ONE
// ordinate and differ ONLY in their ground-type table, which is what makes them two rows rather than two identically
// bodied delegates whose keys carried the whole distinction.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignSpectrum {
    public static readonly DesignSpectrum En1998Type1 = new("en1998-type1", Eurocode, Type1Ground);
    public static readonly DesignSpectrum En1998Type2 = new("en1998-type2", Eurocode, Type2Ground);
    public static readonly DesignSpectrum Asce7       = new("asce7",        Asce,     FrozenDictionary<string, GroundShape>.Empty);

    // Ground-type table the row's own ordinate reads; empty on a row whose code parameterizes by mapped spectral
    // accelerations, which is exactly the case Admit passes through on the Mapped row.
    public FrozenDictionary<string, GroundShape> Ground { get; }

    [UseDelegateFromConstructor]
    public partial double Sa(SpectrumPolicy policy, GroundShape ground, double period);

    // Ground-type admission RESOLVES rather than validates: the caller threads the returned shape into every Sa read,
    // so an unresolvable site class rails once at (Admission, Input) and no evaluation path carries a fallback shape.
    public Fin<GroundShape> Admit(SpectrumPolicy policy) =>
        Ground.IsEmpty ? Fin.Succ(GroundShape.Mapped)
        : Ground.TryGetValue(policy.SiteClass, out GroundShape shape) ? Fin.Succ(shape)
        : Fin.Fail<GroundShape>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input,
            $"<seismic-ground-type-unresolved:{Key}:{policy.SiteClass}>"));

    static readonly FrozenDictionary<string, GroundShape> Type1Ground = Shapes(
        ("a", 1.00, 0.15, 0.40, 2.0), ("b", 1.20, 0.15, 0.50, 2.0), ("c", 1.15, 0.20, 0.60, 2.0),
        ("d", 1.35, 0.20, 0.80, 2.0), ("e", 1.40, 0.15, 0.50, 2.0));

    static readonly FrozenDictionary<string, GroundShape> Type2Ground = Shapes(
        ("a", 1.00, 0.05, 0.25, 1.2), ("b", 1.35, 0.05, 0.25, 1.2), ("c", 1.50, 0.10, 0.25, 1.2),
        ("d", 1.80, 0.10, 0.30, 1.2), ("e", 1.60, 0.05, 0.25, 1.2));

    static FrozenDictionary<string, GroundShape> Shapes(params ReadOnlySpan<(string Ground, double S, double Tb, double Tc, double Td)> rows) =>
        Seq(rows).ToFrozenDictionary(static row => row.Ground, static row => new GroundShape(row.S, row.Tb, row.Tc, row.Td), StringComparer.Ordinal);

    // EN 1998-1 §3.2.2.5 elastic-to-design ordinate; η = max(√(10/(5+ξ)), 0.55) is the damping correction.
    static double Eurocode(SpectrumPolicy p, GroundShape g, double t) {
        double eta = Math.Max(Math.Sqrt(10.0 / (5.0 + 100.0 * p.DampingRatio)), 0.55);
        double ag = p.Pga * g.S;
        return t <= g.Tb ? ag * (1.0 + t / g.Tb * (eta * 2.5 / p.Behavior - 1.0))
            : t <= g.Tc ? ag * eta * 2.5 / p.Behavior
            : t <= g.Td ? ag * eta * 2.5 / p.Behavior * (g.Tc / t)
            : ag * eta * 2.5 / p.Behavior * (g.Tc * g.Td / (t * t));
    }

    static double Asce(SpectrumPolicy p, GroundShape _, double t) =>
        t < 0.2 * p.T1 ? p.Sds * (0.4 + 3.0 * t / p.T1) / p.Behavior
        : t <= p.T1 ? p.Sds / p.Behavior
        : t <= p.TLong ? p.Sd1 / (t * p.Behavior)
        : p.Sd1 * p.TLong / (t * t * p.Behavior);
}

// SRSS and the CQC closely-spaced default. Each row mints the cross-modal correlation its rule implies and the
// quadratic fold below is SHARED, so a new combination rule is one correlation mint and never a second summation.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModalCombination {
    public static readonly ModalCombination Srss = new("srss", static (_, _, modes) => ModalCorrelation.Identity(modes));
    public static readonly ModalCombination Cqc  = new("cqc",  static (omega, xi, _) => ModalCorrelation.Of(omega, xi));

    [UseDelegateFromConstructor]
    public partial ModalCorrelation Correlate(Seq<double> omega, double dampingRatio, int modes);
}

// Cross-modal correlation depends ONLY on the mode frequencies and the damping ratio, so it is built ONCE per solve
// and read across every member and every component — the per-call rebuild it replaces was O(k²) work repeated per
// component per member for a matrix the solve had already determined. Row-major over the recovered mode count.
// `[Equatable]` closes the latent trap: an ImmutableArray member compares by underlying-array REFERENCE under
// record-struct equality, so two identical correlations built by two solves read unequal; `Rho` orders element-wise.
[Equatable]
public readonly partial record struct ModalCorrelation([property: OrderedEquality] ImmutableArray<double> Rho, int Modes) {
    // Der Kiureghian closed form ρ_ij = 8ξ²(1+r)r^1.5 / ((1−r²)² + 4ξ²r(1+r)²) with r = ω_j/ω_i.
    // Exemption: the symmetric matrix fill is the numeric kernel statement seam.
    public static ModalCorrelation Of(Seq<double> omega, double xi) {
        int modes = omega.Count;
        double[] rho = new double[modes * modes];
        for (int i = 0; i < modes; i++) {
            for (int j = 0; j < modes; j++) {
                double ratio = omega[j] / Math.Max(omega[i], 1e-12);
                rho[i * modes + j] = 8.0 * xi * xi * (1.0 + ratio) * Math.Pow(ratio, 1.5)
                    / (Math.Pow(1.0 - ratio * ratio, 2.0) + 4.0 * xi * xi * ratio * Math.Pow(1.0 + ratio, 2.0));
            }
        }
        return new ModalCorrelation([.. rho], modes);
    }

    // SRSS is the ρ_ij = δ_ij degenerate, minted as the identity so both rows read ONE combination fold.
    public static ModalCorrelation Identity(int modes) {
        double[] rho = new double[modes * modes];
        for (int i = 0; i < modes; i++) { rho[i * modes + i] = 1.0; }
        return new ModalCorrelation([.. rho], modes);
    }

    public double Combine(Seq<double> modal) {
        double sum = 0.0;
        for (int i = 0; i < Modes; i++) { for (int j = 0; j < Modes; j++) { sum += Rho[i * Modes + j] * modal[i] * modal[j]; } }
        return Math.Sqrt(Math.Max(sum, 0.0));
    }
}

// --- [MODELS] ------------------------------------------------------------------------------
// Site class, ground motion, behavior/response-modification, and damping as one parameter record — the spectrum rows
// read it, the content key folds it (a changed site class or q re-keys the assessment). The ground-type S/TB/TC/TD
// columns are NOT here: they are the spectrum row's own code table, resolved once through Admit.
public sealed record SpectrumPolicy(
    string SiteClass, double Pga, double Sds, double Sd1, double T1, double TLong, double Behavior, double DampingRatio);

// Seismic request payload: the spectrum row, its policy, the excitation axis the mass gate and the spectral scale
// both read, the combination row, the participation floor (0.90 the code default), and the CAPACITY DesignCode the
// member checks run under — the route names the seismic ACTION standard, so the material code arrives HERE and the
// (DesignCode, LimitState) table stays material-code-keyed. Every column content-key folded.
public sealed record SeismicSpec(
    DesignSpectrum Spectrum, SpectrumPolicy Policy, ExcitationAxis Direction, ModalCombination Combination,
    DesignCode Capacity, double ParticipationFloor = 0.90);

public static partial class StructuralAnalysis {
    // Reduction evidence names are LOCAL because only this runner mints and reads them; the participation and
    // combination names live on `Analysis` because the assessment receipt projects those two columns off the same
    // stream, so one spelling serves the mint and the projection instead of forking across the two pages.
    const string RetainedFact     = "modal-retained-dofs";
    const string ReductionFact    = "modal-reduction-residual";
    const string ConditioningFact = "modal-pencil-conditioning";
    const string ExcitationFact   = "modal-excitation-axis";

    // Response-spectrum route runs the condensed modal pencil: full-length (φ, λ) plus per-axis participation off the
    // owned lumped-mass field, the 90% effective-mass floor a TYPED (Solve, Numeric) shortfall on the REQUESTED axis
    // naming the achieved fraction, per-mode Sa(T_i) demand off the spectrum row, modal responses combined through
    // ONE correlation built for the solve, the combined demands checked through the SAME capacity table under the
    // request's own capacity code — a fully-named chain, never a new runner. The reduction's three measured columns
    // and the excitation axis join the fact stream so an operator can see WHAT was condensed, how well, and along
    // which direction; a route that recovered no evidence rails rather than reporting zeros.
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Seismic request, GeometrySource geometry, AssessmentSink sink, IClock clock) =>
        from ground in request.Spec.Spectrum.Admit(request.Spec.Policy)
        from model in Project(graph, FrameInputs.Of(request), geometry)
        from _     in Validate(model, request.Spec.Capacity)
        from lowered in FrameLowering.Lower(model)
        from problem in lowered.Problem(PhysicsKind.FeaModal, LoadCombinationSpec.SeismicUnit)
        from modal in SolveLane.Solve(problem, lowered.Mesh, SolvePolicy.CanonicalModalCondensed, clock)
        from gate  in Participation(modal, request.Spec)
        from reduction in modal.Condensation.ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, "<modal-reduction-evidence-absent>"))
        let periods = modal.EigenValues.Map(static values => toSeq(values.ToArray()).Map(static w2 => 2.0 * Math.PI / Math.Sqrt(Math.Max(w2, 1e-12)))).IfNone(Seq<double>())
        let demands = SpectralDemands(model, lowered, modal, request.Spec, ground, periods)
        from blob in sink.Store(Artifact(demands, graph.Header.Tolerance,
            Some((modal.Field, periods.Count, periods.Count > 0 ? modal.Field.Length / periods.Count : 0, periods))))
        let checks = model.Members.Bind(m => Check(m, demands[m.Id], request.Spec.Capacity, model.Policy))
        from folded in CheckFacts(checks)
        from evidence in AssessmentFact.Rows(
            AssessmentFact.Ratio(Analysis.ParticipationFact, gate),
            AssessmentFact.Ratio(RetainedFact, reduction.Retained),
            AssessmentFact.Ratio(ReductionFact, reduction.Residual),
            AssessmentFact.Ratio(ConditioningFact, reduction.Conditioning))
        select AssessmentResult.Of(
            request.Route,
            folded.Facts + evidence + Seq(
                    AssessmentFact.Text(Analysis.CombinationFact, request.Spec.Combination.Key),
                    AssessmentFact.Text(ExcitationFact, request.Spec.Direction.Key)),
            folded.Governing,
            new Provenance("StructuralAnalysis", request.Route.Standard, request.Route.SolverVersion, clock.GetCurrentInstant()),
            Some(blob));

    // Effective-mass floor gates PER EXCITATION AXIS: Σ Γ_d² over the recovered modes against SolveResult.TotalMass
    // on the SAME axis — the real directional effective-mass ratio, never a cross-axis total that reads healthy
    // while the requested direction is unrepresented, and never a self-normalized quotient that reads ~1 for any
    // spectrum. Torsional modes carry no translational Γ_d, so they contribute nothing to the axis they never
    // excite; the shortfall is deterministic (Solve, Numeric) NAMING the axis and the achieved fraction — it caches
    // as a Failed node and never re-runs blind; an absent participation stream (a non-vibration result) is its own
    // typed decline.
    static Fin<double> Participation(SolveResult modal, SeismicSpec spec) =>
        modal.Participation
            .Bind(gammas => modal.TotalMass.Map(total => {
                double excited = toSeq(gammas.ToArray()).Sum(row => spec.Direction.Of(row) * spec.Direction.Of(row));
                return excited / Math.Max(spec.Direction.Of(total), 1e-30);
            }))
            .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, "<modal-mass-shortfall:participation-stream-absent>"))
            .Bind(fraction => fraction >= spec.ParticipationFloor
                ? Fin.Succ(fraction)
                : Fin.Fail<double>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric,
                    $"<modal-mass-shortfall:axis={spec.Direction.Key}:achieved={fraction:0.000}:floor={spec.ParticipationFloor:0.00}>")));

    // Per-member combined seismic demand: each mode's spectral displacement field u_i = Γ_d,i·Sa(T_i)/ω_i²·φ_i scales
    // by the participation factor of the axis the request excites, and recovers member responses through the SAME
    // StationRecovery.Demands kernel the static route uses. The per-mode component responses combine through ONE
    // ModalCorrelation built for this solve, component by component, and the combined magnitude is SIGN-INDEFINITE —
    // it enters the envelope as BOTH extremes, so an axially combined member is checked in tension and in compression
    // rather than in whichever sense the fold happened to emit.
    static FrozenDictionary<NodeId, MemberResponse> SpectralDemands(FrameModel model, FrameLowered lowered, SolveResult modal, SeismicSpec spec, GroundShape ground, Seq<double> periods) {
        ReadOnlyMemory<double> shapes = modal.Field;                                    // column-major mode shapes, n DOFs x k modes
        Seq<double> omegaSq = modal.EigenValues.Map(static v => toSeq(v.ToArray())).IfNone(Seq<double>());
        Seq<ModalParticipation> gammas = modal.Participation.Map(static v => toSeq(v.ToArray())).IfNone(Seq<ModalParticipation>());
        Seq<double> omega = omegaSq.Map(static w2 => Math.Sqrt(Math.Max(w2, 1e-12)));
        ModalCorrelation rho = spec.Combination.Correlate(omega, spec.Policy.DampingRatio, omegaSq.Count);
        int dofs = omegaSq.Count > 0 ? shapes.Length / omegaSq.Count : shapes.Length;
        // Per mode: scale the shape column into a displacement field, recover per-member demands via the shared kernel.
        Seq<Seq<(NodeId Id, MemberResponse Response)>> perMode = toSeq(Enumerable.Range(0, omegaSq.Count)).Map(mode => {
            double scale = spec.Direction.Of(gammas[mode]) * spec.Spectrum.Sa(spec.Policy, ground, periods[mode]) / Math.Max(omegaSq[mode], 1e-12);
            double[] field = new double[dofs];
            shapes.Span.Slice(mode * dofs, dofs).CopyTo(field);                          // Exemption: the mode-column scale is the numeric kernel seam
            TensorPrimitives.Multiply(field, scale, field);
            return StationRecovery.Demands(model, lowered, LoadCombinationSpec.SeismicUnit, field);
        });
        return model.Members.Map(member => {
            Seq<MemberResponse> rows = perMode.Map(demands =>
                demands.Find(row => row.Id == member.Id).Map(static row => row.Response).IfNone(MemberResponse.Zero));
            double Combined(Func<SectionDemand, double> component) => rho.Combine(rows.Map(row => row.Span(component)));
            SectionDemand magnitude = new(
                Combined(static d => d.N), Combined(static d => d.Vy), Combined(static d => d.Vz),
                Combined(static d => d.My), Combined(static d => d.Mz), Combined(static d => d.T));
            return (member.Id, Response: new MemberResponse(-magnitude, magnitude, rho.Combine(rows.Map(static row => row.MaxDeflection))));
        }).ToFrozenDictionary(static row => row.Id, static row => row.Response);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
