# [COMPUTE_STRUCTURAL]

Rasm.Compute structural-analysis runner owns the `Discipline.Structural` arm of the `Analysis/assessment` spine. It reads the concrete `Rasm.Element` `ElementGraph` directly (member `Node.Object` axes, the M7-resolved `SectionProperties`, the seam `MaterialPropertySet.Mechanical` strengths, the projected structural-connection/activity edges), folds them into one `FrameModel` idealization, solves it over the owned frame spine — the `Solver/contract#SOLVE_CONTRACT` `SolveLane` scattering the `Solver/discretization#DISCRETIZATION_MESH` frame `ElementClass` rows' closed-form 12-DOF member blocks (releases/rigid-end offsets/semi-rigid springs as row behavior), factored through `Tensor/factor#SPARSE_SOLVE` as `FactorKind.Spd` under `SolvePolicy.CanonicalStatic` — recovers the per-combination `MemberResponse` envelope, runs the hand-rolled design-code checks off a `(DesignCode, LimitState)` capacity table of real family-specific delegates (every structural family carrying both its US and its Eurocode route), and returns the governing utilization as one `AssessmentResult` fact stream.

Section properties resolve once upstream — the `VividOrange`-backed `ProfileRef`→section resolution (M7) runs in the `Rasm.Materials` projector and bakes onto the `ProfileSet` composition, so this consumer reads the resolved `SectionProperties` off the graph and Compute admits no VividOrange. Frame assembly enters the shared `SolveLane`; `SolvePolicy.CanonicalStatic` selects `FactorKind.Spd` through `SparseOps.Factor`, while the seismic route selects the `ArpackShiftInvert` row. Buckling, lateral-torsional buckling, and deflection read the member's unbraced length, end-fixity-derived effective-length factor, and FE displacement envelope.

## [01]-[INDEX]

- [01]-[FRAME_MODEL]: the `FrameModel` idealization folded from the graph, the `MemberLoad`/`StructuralCase` vocabulary, the `MemberSupport`/`LoadCombinationSpec`/`StructuralPolicy`/`StructuralMember` inputs (the seam contract `Analysis/assessment` carries on the request), and the `StructuralReads` discipline graph-read boundary.
- [02]-[FRAME_BACKEND]: the owned-spine frame solve over the `Solver/contract` `SolveLane` and the `Solver/discretization` frame `ElementClass` rows (BFE/FEALiTE retired), the `FrameLowering` model→mesh projection, the `StationRecovery` per-station fold, and the per-combination `MemberResponse` internal-force-and-deflection envelope every limit state reads.
- [03]-[DESIGN_CHECK]: the `MaterialFamily`/`SafetyFormat`/`DesignCode`/`LimitState` vocabulary, the `(DesignCode, LimitState)` capacity table of real slenderness-aware delegates (the EN 1992 truss shear pairing and the AISI Seff-invisible cells among them), the `CapacityContext`/`SectionCapacity`/`MemberCheck` carriers, the interaction rule, and the `StructuralAnalysis.Run` governing-utilization fact stream.
- [04]-[SEISMIC_ROUTE]: the response-spectrum route over the `arpack-shift-invert` sparse modal — `DesignSpectrum` code-spectrum policy rows (EN 1998 Type 1/2, ASCE 7), the `ModalCombination` `Srss`/`Cqc` axis, the typed 90% modal-mass-participation floor, and the participation/combination receipt columns.

## [02]-[FRAME_MODEL]

- Owner: `FrameModel` the analysis idealization (members, combinations, policy);  `MemberLoad` the per-member applied-action `[Union]` (`Point`/`Uniform`/`Trapezoid`); `StructuralCase` the load-case `[SmartEnum<string>]`; `MemberSupport` the 6-DOF restraint at a member end; `LoadCombinationSpec` the factored case map; `StructuralPolicy` the formulation/deflection/station policy carrying the `Formulation` frame `ElementClass` column (`Analysis/assessment` content-keys its `Key`) and the EN 1992 member-scope `StirrupSpacing`/`CotTheta` truss inputs; `StructuralMember` the resolved member (axis, section, strength, family, loads, supports); `WindExposureClass`/`OccupancyClass` the ASCE 7 exposure-profile and live-load vocabularies; `SiteActionPolicy` the per-engagement wind/snow/occupancy code-parameter record; `ActionDerivation` the load-takedown table minting live/wind/snow `MemberLoad` actions from member geometry plus the site policy. `SolverBackend`/`StructuralCase`/`MemberSupport`/`LoadCombinationSpec`/`StructuralPolicy` are the seam contract `AssessmentRequest.Structural` carries and `Analysis/assessment` `CanonicalBytes` folds — their field shape is load-bearing across the spine.
- Entry: `static Fin<FrameModel> Project(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry)` — folds the request `Targets` member `Node.Object`s into the idealization, reading each member's `StructuralReads.AxisOf` (the analytical line resolved one-hop by content key through the seam `GeometrySource` port off `member.Representations.Axis`), `graph.PropertiesOf(id).Mechanical` (the seam strength read), `graph.SectionOf` (the seam Op-free M7 accessor reading the baked `ProfileSet` section directly — the seam owns the section read, so the runner never re-derives a discipline-local section accessor), `StructuralReads.SupportsOf`, and `StructuralReads.LoadsOf`, `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a member lacks a section, a strength, or an axis.
- Auto: self-weight derives per member from `Section.Area.Si × Mechanical.Density.Si × StandardGravity` as a global-down `Uniform` force-per-length in the `Dead` case; the request's projected `MemberLoad`s supply the applied live/wind/snow/seismic actions, and where a variant carries none `ActionDerivation.Derive` mints them from tributary geometry plus one `SiteActionPolicy` — ASCE 7 velocity pressure `qz = 0.613·Kz·Kzt·Kd·V²` at the member's mean height for wind, `pf = 0.7·Ce·Ct·Is·pg` with the slope factor for roof snow, the `OccupancyClass` row for floor live — so a generated design screens without a human load engineer per variant, exactly the derivation precedent the seismic `DesignSpectrum` rows set; `LoadCombinationSpec` factors the cases per code (ASCE 7 / EN 1990) so a combination is data the backend reads, never a re-modelled load set; the member's `MaterialFamily` is `Classify`-derived from the strength for the FE material model and validated against the route's `DesignCode.Family` at `Check`.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`), Rasm.Element (project — `ElementGraph`, `Node`, `NodeId`, the seam-owned host-neutral `Vector3` coordinate the `AxisCurve` carries and the load vectors reuse, `AxisCurve`, `GeometrySource` the analytical-line resolution port, `RepresentationContentHash`, `SectionProperties`, `MaterialPropertySet`, `Relationship`, `PropertyName`, `PropertyValue`, `MeasureValue`), BCL inbox (`FrozenDictionary`).
- Growth: a new applied-action kind is one `MemberLoad` case (both backends widen their total load `Switch`); a new restraint is data on `MemberSupport`; a new combination basis is one `LoadCombinationSpec` row; a new exposure or occupancy is one `WindExposureClass`/`OccupancyClass` row, a new derived action family (EN 1991 wind/snow rows, drift surcharge, partition allowance) one weighted arm on `ActionDerivation.Derive` reading its `SiteActionPolicy` columns — the idealization widens by data, the backends and checks re-read it.
- Boundary: the section is the M7-resolved seam `SectionProperties` read once off the `ProfileSet` composition (the `VividOrange` `ProfileRef`→section resolution happens once in the `Rasm.Materials` projector, so this runner never re-resolves a profile and Compute admits no VividOrange); `SectionProperties` carries the both-axis shear areas `AvY`/`AvZ` and both-axis radii, so the per-axis shear check reads its own area. Strength is the seam `Mechanical.YieldStrength`/`UltimateStrength`/`YoungsModulus`/`ShearModulus`/`Density`/`PoissonsRatio` (the seam field is `PoissonsRatio`, never `PoissonRatio`) off the member's associated material; the analytical line is the seam `AxisCurve` (`Start`/`End`/`Up`) content-keyed under `member.Representations.Axis`, never inlined on the node, resolved one-hop through the seam `GeometrySource` port (coplanarity a `StructuralReads` `AxisCurve` fold, length the member's own `Vector3.Distance`). Supports and loads traverse the projected `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` neutral `Generic` edges by wire-name (the Bim projector stamping the 6-DOF restraint, applied components, end discriminant, and load kind), so the runner reads the idealization fully baked, never re-reading IFC; a member with no section/strength/axis rails the typed input fault.

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

// Occupancy rows carry the ASCE 7 Table 4.3-1 uniformly distributed live load in SI; a new occupancy is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OccupancyClass {
    public static readonly OccupancyClass Residential = new("residential", liveLoadPa: 1_920.0);
    public static readonly OccupancyClass Office      = new("office",      liveLoadPa: 2_400.0);
    public static readonly OccupancyClass Assembly    = new("assembly",    liveLoadPa: 4_790.0);
    public static readonly OccupancyClass Storage     = new("storage",     liveLoadPa: 6_000.0);
    public static readonly OccupancyClass Roof        = new("roof",        liveLoadPa: 960.0);

    public double LiveLoadPa { get; }
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    const double StandardGravity = 9.80665;             // m/s^2 — self-weight body acceleration
    const double Eps             = 1e-12;               // numeric floor for a capacity/length divisor
    static readonly PropertyName WireAtStart  = PropertyName.Create("AtStart");
    static readonly PropertyName WireLoadKind = PropertyName.Create("LoadKind");
    static readonly PropertyName WireCase     = PropertyName.Create("Case");
}

// --- [MODELS] ------------------------------------------------------------------------------
// One support at a member END (AtStart selects the start vs end joint the Bim projector stamped from the connection
// geometry) — the FE assembler resolves the joint by the chosen endpoint coordinate, never by comparing the connection
// NodeId to the member's own id. At is the connection node the restraint sits on (carried for traceability).
// TranslationK/RotationK carry the [H2] SI spring stiffnesses and Frame the RestraintAxis*/RestraintRef* skewed-support
// axes the Bim edge emits: the owned spine consumes the finite ROTATIONAL end springs directly through the
// FrameMember semi-rigid columns (the exact in-series condensation the discretization rows carry — the
// conservative-rigid lowering the retired BFE boolean-constraint surface forced is DELETED); a finite
// TRANSLATIONAL spring and a skewed frame still lower conservatively (rigid / global axes), both NAMED
// lowerings consuming these columns with zero wire edits when their rows land.
public readonly record struct MemberSupport(
    NodeId At, bool AtStart, bool Dx, bool Dy, bool Dz, bool Rx, bool Ry, bool Rz,
    Vector3 TranslationK = default, Vector3 RotationK = default, (Vector3 Axis, Vector3 Ref)? Frame = null) {
    public bool RotationallyFixed => Rx && Ry && Rz;
}

public sealed record LoadCombinationSpec(string Label, FrozenDictionary<StructuralCase, double> Factors) {
    // Seismic unit combination carries zero action factors because modal solve reads mass and stiffness, so the
    // lowering hands it a zero-action spec — the one static row RunSeismic threads, never a caller obligation.
    public static readonly LoadCombinationSpec SeismicUnit = new("seismic-unit", FrozenDictionary<StructuralCase, double>.Empty);
}

// Structural policy carries frame formulation, serviceability, sampling, and RC shear inputs; AssessmentRequest.CanonicalBytes
// folds every field, while effective-length factor and unbraced length derive from member fixity and span.
// Formulation selects the owned frame ElementClass row; SolverBackend was deleted because Formulation carries its discriminant.
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
// and the governing occupancy. One policy per engagement; per-variant geometry supplies the rest.
public sealed record SiteActionPolicy(
    double BasicWindSpeedMPerS, WindExposureClass Exposure, double Kzt, double Kd, double GcpNet,
    double GroundSnowPa, double Ce, double Ct, double SnowImportance, double RoofSlopeFactor,
    OccupancyClass Occupancy) {
    public static readonly SiteActionPolicy Canonical = new(
        BasicWindSpeedMPerS: 51.0, WindExposureClass.C, Kzt: 1.0, Kd: 0.85, GcpNet: 0.8,
        GroundSnowPa: 1_000.0, Ce: 1.0, Ct: 1.0, SnowImportance: 1.0, RoofSlopeFactor: 1.0,
        OccupancyClass.Office);

    public bool Invalid => BasicWindSpeedMPerS <= 0.0 || Kzt <= 0.0 || Kd <= 0.0 || GcpNet <= 0.0
        || GroundSnowPa < 0.0 || Ce <= 0.0 || Ct <= 0.0 || SnowImportance <= 0.0 || RoofSlopeFactor is <= 0.0 or > 1.0
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
            return Fin.Fail<Seq<MemberLoad>>(ComputeFault.Create("<action-derivation-invalid>"));
        }
        double meanHeight = 0.5 * (member.Axis.Start.Z + member.Axis.End.Z);
        double run = Math.Sqrt(Math.Pow(member.Axis.End.X - member.Axis.Start.X, 2.0) + Math.Pow(member.Axis.End.Y - member.Axis.Start.Y, 2.0));
        bool horizontal = run / member.Length >= HorizontalCosine;
        double windPerLength = site.VelocityPressurePa(meanHeight) * site.GcpNet * tributaryWidthM;
        Seq<MemberLoad> actions = Seq<MemberLoad>(new MemberLoad.Uniform(StructuralCase.Wind, new Vector3(windPerLength, 0.0, 0.0)));
        if (horizontal && !roofMember) {
            actions = actions.Add(new MemberLoad.Uniform(StructuralCase.Live, new Vector3(0.0, 0.0, -site.Occupancy.LiveLoadPa * tributaryWidthM)));
        }
        if (horizontal && roofMember) {
            actions = actions
                .Add(new MemberLoad.Uniform(StructuralCase.Live, new Vector3(0.0, 0.0, -OccupancyClass.Roof.LiveLoadPa * tributaryWidthM)))
                .Add(new MemberLoad.Uniform(StructuralCase.Snow, new Vector3(0.0, 0.0, -site.RoofSnowPa * tributaryWidthM)));
        }
        return Fin.Succ(actions);
    }
}

public static partial class StructuralAnalysis {
    public static Fin<FrameModel> Project(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry) =>
        request.Targets.Fold(
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
                let family     = MaterialFamily.Classify(strength)
                let selfWeight = new MemberLoad.Uniform(StructuralCase.Dead,
                    new Vector3(0d, 0d, -(section.Area.Si * strength.Density.Si * StandardGravity)))
                select members.Add(new StructuralMember(
                    id, axis, section, strength, directional, family, graph.LoadsOf(id).Add(selfWeight), graph.SupportsOf(id)))))
            .Map(members => new FrameModel(members, request.Combinations, request.Policy, graph.Header.Tolerance));

    static Error MissingInput(NodeId id, string what) =>
        new ComputeFault.AssessmentInputMissing($"<member-missing-{what}:{id.Value}>");
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// Compute-owned discipline graph reads extend ElementGraph through seam no-Op primitives
// (MaterialsOf/PropertiesOf(id).Mechanical/Find/EdgesAt) and the projected neutral Generic structural edges by wire-name, the same
// shape Analysis/energy's EnergyGraphReads takes. The seam owns the material/section/mechanical reads (it owns those
// nodes) and the GeometrySource decode CONTRACT; the discipline physics — axis interpretation, 6-DOF restraints,
// applied actions — lives here, never in the seam. AxisOf resolves the analytical line ONE-HOP by content key through
// GeometrySource resolves member.Representations.Axis rather than a phantom node field, and AxisCurve.Length/
// Coplanar fold that resolved line the runner reasons over in double precision.
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

    // One MemberSupport per structural-connection edge the member relates — the fixity booleans, the [H2] spring
    // stiffnesses, the skewed-support frame, and the start/end discriminant read off the neutral Generic edge
    // payload the projector baked from IfcBoundaryNodeCondition (+ its ConditionCoordinateSystem).
    public static Seq<MemberSupport> SupportsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsMember && g.Relating == member
            ? Some(new MemberSupport(g.Related, Flag(g, StructuralAnalysis.WireAtStart),
                Fix(g, "TranslationX"), Fix(g, "TranslationY"), Fix(g, "TranslationZ"),
                Fix(g, "RotationX"), Fix(g, "RotationY"), Fix(g, "RotationZ"),
                new Vector3(Si(g, "TranslationKx"), Si(g, "TranslationKy"), Si(g, "TranslationKz")),
                new Vector3(Si(g, "RotationKx"), Si(g, "RotationKy"), Si(g, "RotationKz")),
                Opt(g, "RestraintAxisX").Map(_ => (Vec(g, "RestraintAxis"), Vec(g, "RestraintRef"))).ToNullable()))
            : None).ToSeq();

    // One MemberLoad per structural-activity edge the member relates — the kind (point/uniform/trapezoid), the load
    // case, and the component vectors read off the neutral Generic edge payload the projector baked from
    // IfcStructuralLoadSingleForce/LinearForce; self-weight is the Dead Uniform Project derives, so these are the
    // applied actions only. An unrecognized kind folds to a midspan Point (the projector's default action shape).
    public static Seq<MemberLoad> LoadsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsActivity && g.Relating == member
            ? Some(ToLoad(g)) : None).ToSeq();

    static MemberLoad ToLoad(Relationship.Generic g) => Kind(g) switch {
        "uniform"   => new MemberLoad.Uniform(CaseOf(g), Vec(g, "Force")),
        "trapezoid" => new MemberLoad.Trapezoid(CaseOf(g), Vec(g, "Start"), Vec(g, "End")),
        // Presence-based station: a TRUE start-joint action (0.0) is a real position — only an ABSENT attr
        // defaults midspan (the projector's honest None), never a truthiness collapse of 0.0.
        _           => new MemberLoad.Point(CaseOf(g), Vec(g, "Force"), Vec(g, "Moment"), Opt(g, "Station").IfNone(0.5)),
    };

    static string Kind(Relationship.Generic g) =>
        g.Attributes.Find(StructuralAnalysis.WireLoadKind).Map(static v => v is PropertyValue.Text t ? t.Value : "point").IfNone("point");

    static StructuralCase CaseOf(Relationship.Generic g) =>
        g.Attributes.Find(StructuralAnalysis.WireCase)
            .Bind(static v => v is PropertyValue.Text t && StructuralCase.TryGet(t.Value, out StructuralCase c) ? Some(c) : None)
            .IfNone(StructuralCase.Live);

    static bool Fix(Relationship.Generic g, string dof) =>
        g.Attributes.Find(PropertyName.Create(dof)).Map(static v => v is PropertyValue.Boolean b && b.Value).IfNone(false);

    static bool Flag(Relationship.Generic g, PropertyName key) =>
        g.Attributes.Find(key).Map(static v => v is PropertyValue.Boolean b && b.Value).IfNone(false);

    static Vector3 Vec(Relationship.Generic g, string component) =>
        new(Si(g, $"{component}X"), Si(g, $"{component}Y"), Si(g, $"{component}Z"));

    static double Si(Relationship.Generic g, string key) =>
        g.Attributes.Find(PropertyName.Create(key)).Map(static v => v is PropertyValue.Measure m ? m.Value.Si : 0.0).IfNone(0.0);

    // Presence-preserving read returns None for an absent attribute and Some(si) for a present value, including 0.0.
    static Option<double> Opt(Relationship.Generic g, string key) =>
        g.Attributes.Find(PropertyName.Create(key)).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None);
}
```

## [03]-[FRAME_BACKEND]

- Owner: the `Solve` owned-spine route — `FrameModel` lowers onto the `Solver/contract#SOLVE_CONTRACT` `SolveLane` over the `Solver/discretization#DISCRETIZATION_MESH` frame `ElementClass` rows (`beam2-euler`/`beam2-timoshenko`, the `StructuralPolicy.Formulation` column), so the structural lane assembles and factors through the same CSparse owner the continuum lane holds, the owned rows carrying end releases by static condensation, rigid-end offsets by eccentricity transform, and semi-rigid end springs as row behavior; `SectionDemand` the per-combination internal-force envelope; `MemberResponse` the demand-plus-deflection carrier every limit state reads; `FrameLowering` the model→mesh projection (shared joints merged by tolerance-quantized coordinate, per-member `FrameMember` section/release/offset rows off the seam `SectionProperties` and the declared supports, per-member `(E, ν)` on `MaterialField.PerCell`, member loads lowered to fixed-end equivalent nodal actions); `StationRecovery` the per-member station fold off the solved displacement field.
- Entry: `static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model, ClockPolicy clocks)` — lowers the model once, then per `LoadCombinationSpec` scales the case actions, solves through `SolveLane.Solve` (the frame arm scattering each member's closed-form 12-DOF block), recovers the worst-station `SectionDemand` plus the worst-station transverse deflection per member, and envelopes across combinations; `Fin<T>` lowers a singular/ill-conditioned factorization onto the typed `ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, …)` — deterministic, cached by the spine, never re-run blind — and a member missing its section or support set onto `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`.
- Auto: joints merge by tolerance-quantized coordinate (never fragile exact-float `Vector3` equality); each `MemberSupport` lowers to the `BoundaryCondition.Dirichlet` 6-DOF constraint set on its endpoint-resolved shared joint; each `MemberLoad` case lowers through a TOTAL `Switch` to its fixed-end equivalent nodal actions (Point by the closed-form ab²/L² pair, Uniform by wL/2 + wL²/12, Trapezoid by the exact linear-varying closed form — never a flattened uniform average) landing as `Neumann` rows on the member-end DOFs; per-station recovery reads the solved field back through each member's local frame — end displacements gathered and rotated local, local end forces `f = k_l·u_l − f_fixed`, station N/V/M by statics from the end forces plus the span-load particular terms (exact for the three load kinds), station transverse deflection by the Hermite end-displacement interpolation plus the span-load particular deflection — so the `Deflection` limit state is a REAL displacement check, never a 0.0 sentinel.
- Packages: CSparse (shared `SparseCholesky`/`SparseLDL`/`SparseLU`/`SparseQR` family via `Tensor/factor#SPARSE_SOLVE`, selected by `Solver/contract` policy), Rasm.Element (project — `SectionProperties`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new frame formulation is one `ElementClass` frame row (the `Formulation` policy column selects it); a new end condition is a column on `FrameMember` the discretization closed form reads; a new load kind is one `MemberLoad` case plus one fixed-end arm on the total `Switch`; the response envelope is one `MemberResponse` shape the checks read regardless of formulation — a re-admitted external FE backend beside the owned spine is the rejected duplicate-mechanism form, and a `Bfe3DAnalyzer`/`Fealite2DAnalyzer` sibling family stays deleted.
- Boundary: the frame solve is the `Solver/contract` spine — one `SolveLane`, one CSparse factorization owner, one `MaterialField` elasticity admission — and a hand-rolled stiffness assembler beside it is the rejected form; the member releases/rigid-end offsets/semi-rigid springs are ROW BEHAVIOR on the discretization `ElementClass.Member` closed form (condensation/transform/in-series fold); the local frame orders moments `(T=torsion about x, My/Mz=bending)` and the demand maps `SectionDemand(N, Vy, Vz, My, Mz, T)` off the local end-force vector — never a torsion/bending swap; the planar special case is structural, not a second backend — a coplanar model carries zero out-of-plane demand through the same 12-DOF rows; a singular system surfaces as the typed `(Solve, Numeric)` `AnalysisFailed`, never an exception crossing the rail and never an opaque interpolated discriminant.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Per-combination internal-force envelope is formulation-neutral; each component is the signed worst-magnitude over
// every station and every load combination. A per-component envelope is the conservative member-level bound the codes
// check; station-correlated interaction is a growth axis the Check fold would take over the same MemberResponse.
public readonly record struct SectionDemand(double N, double Vy, double Vz, double My, double Mz, double T) {
    public static readonly SectionDemand Zero = new(0, 0, 0, 0, 0, 0);
    public SectionDemand Max(SectionDemand b) => new(
        Worst(N, b.N), Worst(Vy, b.Vy), Worst(Vz, b.Vz), Worst(My, b.My), Worst(Mz, b.Mz), Worst(T, b.T));
    static double Worst(double a, double b) => Math.Abs(a) >= Math.Abs(b) ? a : b;
}

// What the design check reads: the force envelope plus the worst transverse deflection over the policy stations and
// every combination, so the Deflection limit state is a REAL displacement check against StructuralPolicy x span,
// never a 0.0 sentinel.
public readonly record struct MemberResponse(SectionDemand Demand, double MaxDeflection) {
    public static readonly MemberResponse Zero = new(SectionDemand.Zero, 0.0);
    public MemberResponse Max(SectionDemand d, double deflection) => new(Demand.Max(d), Math.Max(MaxDeflection, Math.Abs(deflection)));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    // Owned frame solve lowers once, solves per combination, and recovers per
    // station, envelope across combinations — the one spine entry, never a parallel FE engine.
    public static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model, ClockPolicy clocks) =>
        model.Members.IsEmpty
            ? Fin.Succ(FrozenDictionary<NodeId, MemberResponse>.Empty)
            : FrameLowering.Lower(model).Bind(lowered =>
                model.Combinations.Fold(
                    Fin.Succ(model.Members.Map(static m => (m.Id, Response: MemberResponse.Zero)).ToFrozenDictionary(static p => p.Id, static p => p.Response)),
                    (acc, combo) => acc.Bind(envelope =>
                        lowered.Problem(combo).Bind(problem =>
                            SolveLane.Solve(problem, lowered.Mesh, SolvePolicy.CanonicalStatic, clocks)
                                .MapFail(fault => fault is ComputeFault.ModelRejected reject
                                    ? new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<frame-singular:{combo.Label}:{reject.Message}>")
                                    : fault)
                                .Map(solution => StationRecovery.Envelope(model, lowered, combo, solution.Field, envelope))))));

    // Model -> frame-family DiscreteMesh: joints merged by tolerance-quantized coordinate, one 2-node cell per
    // member, per-member FrameMember rows (section constants off the seam SectionProperties; semi-rigid springs
    // off the declared supports), per-member (E, ν) on MaterialField.PerCell, supports as 6-DOF Dirichlet rows,
    // member loads as fixed-end equivalent Neumann actions per combination. JointOf/EndJoints carry the joint
    // resolution the recovery fold re-reads, so lowering and recovery share one coordinate quantization.
    internal sealed record FrameLowered(DiscreteMesh Mesh, ImmutableArray<FrameMember> Members, MaterialField Field, Func<LoadCombinationSpec, Seq<BoundaryCondition>> Conditions, Func<StructuralMember, (long I, long J)> EndJoints) {
        public Fin<SolveProblem> Problem(LoadCombinationSpec combo) =>
            SolveProblem.Of(PhysicsKind.FeaStatic, Mesh, Conditions(combo), Field, Members);
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

        // Supports lower to per-DOF Dirichlet rows on the endpoint-resolved joint (the declared fixity flags, DOF
        // slots joint*6 + [ux..rz]); member loads lower per combination to fixed-end equivalent Neumann actions on
        // Member-end DOFs receive each case scaled by its combination factor.
        static Seq<BoundaryCondition> Conditions(FrameModel model, Func<(long, long, long), long> jointOf, LoadCombinationSpec combo) =>
            model.Members.Bind(m => m.Supports.Map(s => {
                long joint = jointOf(Quantized(s.AtStart ? m.Axis.Start : m.Axis.End, model.JointTolerance));
                Seq<long> dofs = Seq((s.Dx, 0), (s.Dy, 1), (s.Dz, 2), (s.Rx, 3), (s.Ry, 4), (s.Rz, 5))
                    .Choose(flag => flag.Item1 ? Some(joint * 6 + flag.Item2) : None);
                return (BoundaryCondition)new BoundaryCondition.Dirichlet(FieldStation.Nodal, [.. dofs], [.. dofs.Map(static _ => 0.0)]);
            }))
            + model.Members.Bind(m => m.Loads
                .Choose(load => combo.Factors.TryGetValue(CaseArm(load), out double factor)
                    ? LoadCondition(m, model.JointTolerance, jointOf, load, factor)
                    : None));

        static Option<BoundaryCondition> LoadCondition(StructuralMember member, double jointTolerance, Func<(long, long, long), long> jointOf, MemberLoad load, double factor) {
            long i = jointOf(Quantized(member.Axis.Start, jointTolerance)), j = jointOf(Quantized(member.Axis.End, jointTolerance));
            double[] endForces = Scaled(FixedEnd(load, member.Length).EndForces, factor);
            return Some<BoundaryCondition>(new BoundaryCondition.Neumann(
                [i * 6 + 2, i * 6 + 4, j * 6 + 2, j * 6 + 4], endForces));
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
        // intensity); a flattened trapezoid-to-uniform average is the deleted form. EndForces packs the transverse
        // end shears and end moments [Vi, Mi, Vj, Mj]; the particular arrows carry the span-load interior moment and
        // deflection terms the station recovery adds to the end-force statics — exact for the three load kinds.
        public static (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) FixedEnd(MemberLoad load, double length) =>
            load.Switch(
                point: p => {
                    double magnitude = p.Force.Z, a = p.Station * length, b = length - a, l2 = length * length;
                    double mi = magnitude * a * b * b / l2, mj = -magnitude * a * a * b / l2;
                    double vi = magnitude * b * b * (length + 2.0 * a) / (l2 * length);
                    return (new[] { vi, mi, magnitude - vi, mj },
                        (Func<double, double>)(x => x < a ? 0.0 : -magnitude * (x - a)),
                        (Func<double, double>)(x => x < a ? 0.0 : magnitude * Math.Pow(x - a, 3) / 6.0));
                },
                uniform: u => {
                    double w = u.ForcePerLength.Z, l2 = length * length;
                    return (new[] { w * length / 2.0, w * l2 / 12.0, w * length / 2.0, -w * l2 / 12.0 },
                        (Func<double, double>)(x => -w * x * x / 2.0),
                        (Func<double, double>)(x => w * Math.Pow(x, 4) / 24.0));
                },
                trapezoid: t => {
                    double w1 = t.Start.Z, w2 = t.End.Z, l2 = length * length;
                    double vi = length * (7.0 * w1 + 3.0 * w2) / 20.0, vj = length * (3.0 * w1 + 7.0 * w2) / 20.0;
                    double mi = l2 * (w1 / 20.0 + w2 / 30.0), mj = -l2 * (w1 / 30.0 + w2 / 20.0);
                    return (new[] { vi, mi, vj, mj },
                        (Func<double, double>)(x => -(w1 * x * x / 2.0 + (w2 - w1) * x * x * x / (6.0 * length))),
                        (Func<double, double>)(x => w1 * Math.Pow(x, 4) / 24.0 + (w2 - w1) * Math.Pow(x, 5) / (120.0 * length)));
                });

        static double SpringAt(StructuralMember m, bool atStart, bool axisY) =>
            m.Supports.Find(s => s.AtStart == atStart)
                .Map(s => {
                    double stiffness = axisY ? s.RotationK.Y : s.RotationK.Z;
                    return stiffness > 0.0 && double.IsFinite(stiffness) ? stiffness : double.PositiveInfinity;
                })
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
                .Map(row => (row.Id, Response: prior[row.Id].Max(row.Demand, row.Deflection)))
                .ToFrozenDictionary(static row => row.Id, static row => row.Response);

        // Per-member recovery kernel serves both static envelope and seismic per-mode demand.
        // Exemption: the station march over the solved field is the measured-kernel statement seam.
        public static Seq<(NodeId Id, SectionDemand Demand, double Deflection)> Demands(FrameModel model, FrameLowered lowered, LoadCombinationSpec combo, ReadOnlyMemory<double> field) =>
            model.Members.Map(member => {
                (long i, long j) = lowered.EndJoints(member);
                double length = member.Length;
                // 6-DOF per joint: [ux, uy, uz, rx, ry, rz] — the transverse local pair (uz, ry) drives the Hermite
                // deflection; axial and torsion read the end differentials directly.
                ReadOnlySpan<double> u = field.Span;
                double uzI = At(u, i, 2), ryI = At(u, i, 4), uzJ = At(u, j, 2), ryJ = At(u, j, 4);
                double axial = (At(u, j, 0) - At(u, i, 0)) / Math.Max(length, StructuralAnalysis.Eps) * member.Strength.YoungsModulus.Si * member.Section.Area.Si;
                double torsion = (At(u, j, 3) - At(u, i, 3)) / Math.Max(length, StructuralAnalysis.Eps) * member.Strength.ShearModulus.Si * member.Section.J.Si;
                Seq<MemberLoad> loads = member.Loads.Filter(load => combo.Factors.ContainsKey(FrameLowering.CaseArm(load)));
                Seq<(double Factor, (double[] EndForces, Func<double, double> ParticularMoment, Func<double, double> ParticularDeflection) Action)> actions = loads.Map(load =>
                    (Factor(combo, load), FrameLowering.FixedEnd(load, length)));
                double startShear = actions.Fold(0.0, static (acc, action) => acc + action.Factor * action.Action.EndForces[0]);
                double startMoment = actions.Fold(0.0, static (acc, action) => acc + action.Factor * action.Action.EndForces[1]);
                SectionDemand demand = SectionDemand.Zero;
                double worstDeflection = 0.0;
                // Exact fixed-end decomposition uses M(x) = EI·v_h''(x), with the Hermite homogeneous term the joint
                // displacements drive — the SEISMIC route's whole bending signal) + the fixed-end particular chain
                // (end reactions + span-load integral); V(x) mirrors with the constant EI·v_h''' term.
                double ei = member.Strength.YoungsModulus.Si * member.Section.Iyy.Si;
                for (int s = 0; s < model.Policy.StationCount; s++) {
                    double x = length * s / Math.Max(model.Policy.StationCount - 1, 1);
                    double xi = x / Math.Max(length, StructuralAnalysis.Eps);
                    double homogeneousMoment = ei * HermiteCurvature(uzI, ryI, uzJ, ryJ, xi, length);
                    double homogeneousShear = ei * HermiteJerk(uzI, ryI, uzJ, ryJ, length);
                    double shear = homogeneousShear + startShear;
                    double moment = homogeneousMoment + startMoment + startShear * x
                        + actions.Fold(0.0, (acc, action) => acc + action.Factor * action.Action.ParticularMoment(x));
                    double hermite = Hermite(uzI, ryI, uzJ, ryJ, xi, length)
                        + actions.Fold(0.0, (acc, action) => acc + action.Factor * action.Action.ParticularDeflection(x) / Math.Max(ei, StructuralAnalysis.Eps));
                    demand = demand.Max(new SectionDemand(axial, shear, 0.0, moment, 0.0, torsion));
                    worstDeflection = Math.Max(worstDeflection, Math.Abs(hermite));
                }
                return (member.Id, demand, worstDeflection);
            });

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

- Owner: `MaterialFamily` the constitutive family; `SafetyFormat` the ASD/LRFD/limit-state axis; `DesignCode` `[SmartEnum<string>]` the standard rows carrying the `MaterialFamily`, the `SafetyFormat`, the resistance/partial factors, and the interaction delegate; `LimitState` `[SmartEnum<string>]` the check rows carrying the demand-component selector and the `Applies(MaterialFamily)` predicate; `CapacityContext` the section+isotropic-strength+optional-orthotropic-stiffness+geometry+code bundle every capacity reads (its `ShearModulusSi` reading the realized seam `Orthotropic.ShearModulus` when the member carries the directional case, the derived isotropic `Mechanical` shear otherwise); the `Capacities` `(DesignCode, LimitState)` frozen table of REAL delegates; `SectionCapacity`/`MemberCheck` the carriers; `StructuralAnalysis.Run` the governing-utilization entry.
- Cases: `DesignCode` rows `aisc360`/`en1993`/`en1992`/`nds`/`en1995`/`aci318`/`tms402`/`aisi-s100` — every structural family carries BOTH its US and its Eurocode row (steel `aisc360`+`en1993`, concrete `aci318`+`en1992`, timber `nds`+`en1995`), so a member is assessable under either jurisdiction through the SAME table, never a US-only or EN-only family; `LimitState` rows `axial-tension`/`axial-compression`/`flexure-major`/`flexure-minor`/`shear-major`/`shear-minor`/`combined`/`deflection` (shear split per axis so the major-axis demand `|Vy|` checks against `AvY` and the minor-axis `|Vz|` against `AvZ`, never one shear area for both) — the capacity is a `(code, state)` cell in the frozen table, each cell the GOVERNING formula for THAT code's material model (AISC E3 `Fcr`, EN 1993 `χ` buckling curve, AISC F2 `Mn` with `Lp`/`Lr` LTB, EN 1993 `χLT`, ACI/EN plain-concrete `Mcr`/`φPn`, NDS `CP`/`CL` adjusted reference values, EN 1995 `k_c`/`k_crit` over the `E0,05` 5%-fractile modulus, TMS slenderness-reduced `Fa`, AISI gross-section bound), the per-cell slenderness/compactness branches the rule count; lateral-torsional buckling is FOLDED into the flexure-major `Mn`/`Mₕ` (one capacity, never a duplicate state); an absent cell is not-applicable (capacity `+∞`, ratio `0`).
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks)` — a request whose route discipline is the seam `Seismic` row dispatches the `[04]` response-spectrum chain (`RunSeismic`, gated on the request's `SeismicSpec`); otherwise `Project` reads the idealization, `Solve` recovers the `MemberResponse` envelope, `Check` folds each member through every applicable `LimitState` computing `utilization = demand / capacity` (the `Combined` arm the code interaction, the `Deflection` arm the FE deflection against `StructuralPolicy.DeflectionLimitRatio × span`), and the governing (max-utilization) member yields the `AssessmentResult` fact stream (`max-utilization`, `governing-member`, `governing-limit-state`, per-check ratios) — the verdict the spine DERIVES from the governing ratio.
- Auto: the column capacity reads the `EffectiveLengthFactor × UnbracedLength / RadiusOfGyrationMinor` slenderness (AISC `Fcr`, EN `χ`); the flexure-major capacity reads `Lb` against `Lp` and the elastic LTB moment (EN `χLT`); the deflection check reads `MemberResponse.MaxDeflection`; the combined axial+flexure interaction folds the enveloped demand per the `DesignCode.Interaction` delegate (AISC 360 H1.1 and EN 1993-1-1 §6.3.3 for steel, the EN 1995-1-1 §6.3.2(3) squared-axial + linear-bending form with the `k_m = 0.7` minor-axis factor for timber, the linear sum for the rest), `Combined` applying to steel/cold-formed/timber.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — `SectionProperties`, `MaterialPropertySet` (the isotropic `Mechanical` AND the realized directional `Orthotropic` case), `NodeId`, `Provenance`, the `graph.PropertiesOf(member).Orthotropic` ergonomic read), NodaTime (`Instant`), BCL inbox (`FrozenDictionary`).
- Growth: a new design code is one `DesignCode` row plus its `(code, state)` cells in the table; a new limit state is one `LimitState` row plus its column of cells; a new material family is one `MaterialFamily` row plus its codes' cells — the check fold re-reads the table, never a new check method per code.
- Boundary: the design codes are hand-rolled (no .NET package owns AISC 360 / EN 1993 / EN 1992 / NDS / EN 1995 / ACI 318 / TMS 402 / AISI S100), realized as a `(DesignCode, LimitState)` data table of capacity delegates — the canonical `POLICY_VALUES`/`DERIVED_LOGIC` collapse, never a switch ladder and never one family's formulas applied to every material. Timber's EN 1995 route is the Eurocode parallel to the US `nds` route the way `en1993` parallels `aisc360` and `en1992` parallels `aci318` — its design strength is `f_k / γ_M` over the same seam reference strength the `nds` cells read (the `k_mod` service/duration modifier is applied upstream by the `Rasm.Materials` `TimberDesign` owner onto the graph-baked reference, never re-derived here), the `§6.3.2` `k_c` column buckling and `§6.3.3` `k_crit` LTB reading the `E0,05` 5%-fractile modulus the seam mean `YoungsModulus` does not carry directly, and the `§6.1.7` `k_cr` crack factor on shear. Timber's independent shear modulus the EC5 `§6.3.3` LTB reads is the realized seam `MaterialPropertySet.Orthotropic` case (`Composition/material#MATERIAL_PROPERTY`, same `Discipline.Structural`, discriminated by case TYPE), read off the graph as the optional `graph.PropertiesOf(member).Orthotropic` and threaded onto `CapacityContext.ShearModulusSi`, so the LTB `M꜀ᵣ` reads timber's directional `Orthotropic.ShearModulus` when the member carries the case and the derived isotropic `Mechanical.ShearModulus` otherwise — the `Component/timber#ORTHOTROPIC_STIFFNESS_LAW` contract closed here, never a deferred isotropic approximation. Capacity reads the M7-resolved seam `SectionProperties`, the seam isotropic `Mechanical` strength, and the optional seam `Orthotropic`, so a check never re-derives section geometry, re-resolves a profile, or approximates timber's directional shear; the authoritative family is `DesignCode.Family`, the member's `Classify`-derived family validated against it so a steel code on a concrete member rails `AssessmentInputMissing` rather than computing nonsense. Reinforced-concrete N-M-M capacity is not derivable from the geometric seam section (which carries no rebar) — the concrete cells are the plain-section bound, the reinforced interaction the `Rasm.Materials` `Component/capacity#SECTION_CAPACITY` RC owner's concern, so the `VividOrange` `IForceMomentInteraction` surface is not composed here; cold-formed AISI capacity is the gross-section bound, the effective-width reduction the `Component/steel` `ColdFormedDetail`'s concern. Utilization is `demand/capacity` and the verdict derives downstream from the governing ratio so a member's pass/fail and its reported ratio share one source; a member whose family no `DesignCode` row serves rails `AssessmentInputMissing`, never a silent skip.

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

    // A FE-material-model hint from the constitutive stiffness/density (steel and cold-formed share the high-modulus
    // band — the DesignCode.Family the route declares disambiguates them at Check); the authoritative design family is
    // DesignCode.Family, this is the validation companion.
    public static MaterialFamily Classify(MaterialPropertySet.Mechanical m) =>
        m.YoungsModulus.Si > 150e9 ? Steel : m.YoungsModulus.Si > 20e9 ? Concrete : m.YoungsModulus.Si > 5e9 ? Timber : Masonry;

    // Stiffness-band compatibility: Classify resolves a STIFFNESS band, not the true family, so Admits maps a design
    // family to the bands its members can land in. A steel route admits the steel band (Steel and ColdFormedSteel both
    // classify Steel); a masonry route admits ANY non-steel band because masonry's elastic modulus (Em = 700..900*f'm,
    // ~7..18 GPa for typical f'm) overlaps the Timber and Concrete bands and modulus alone cannot separate them — so a
    // standard masonry member that Classify lands as Timber or Concrete is still assessable under TMS 402 rather than
    // spuriously railing the route's own capacity cells; concrete and timber routes require their own resolvable band.
    // Steel-band threshold (150 GPa) is the boundary modulus that rejects a steel code on a non-steel
    // member is still rejected.
    public bool Admits(MaterialFamily classified) =>
        this == classified
        || ((this == Steel || this == ColdFormedSteel) && classified == Steel)
        || (this == Masonry && classified != Steel && classified != ColdFormedSteel);
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
    public static readonly DesignCode En1992   = new("en1992",    MaterialFamily.Concrete,        SafetyFormat.LimitState, 1.50, LinearInteraction);
    public static readonly DesignCode Nds      = new("nds",       MaterialFamily.Timber,          SafetyFormat.Asd,        1.00, LinearInteraction);
    public static readonly DesignCode En1995   = new("en1995",    MaterialFamily.Timber,          SafetyFormat.LimitState, 1.25, En1995Interaction);
    public static readonly DesignCode Aci318   = new("aci318",    MaterialFamily.Concrete,        SafetyFormat.Lrfd,       1.00, LinearInteraction);
    public static readonly DesignCode Tms402   = new("tms402",    MaterialFamily.Masonry,         SafetyFormat.LimitState, 1.00, LinearInteraction);
    public static readonly DesignCode AisiS100 = new("aisi-s100", MaterialFamily.ColdFormedSteel, SafetyFormat.Lrfd,       1.00, AiscH11);

    public MaterialFamily Family { get; }
    public SafetyFormat Format { get; }
    public double GammaM { get; }

    [UseDelegateFromConstructor]
    public partial double Interaction(SectionDemand demand, SectionCapacity capacity);

    public static Fin<DesignCode> For(AssessmentRoute route) =>
        TryGet(route.Key, out DesignCode code)
            ? Fin.Succ(code)
            : Fin.Fail<DesignCode>(new ComputeFault.AssessmentInputMissing($"<no-design-code:{route.Key}>"));

    static double AiscH11(SectionDemand d, SectionCapacity c) {
        double axial = Math.Abs(d.N) / Math.Max(c.AxialCompression, Eps);
        double bending = Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps) + Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, Eps);
        return axial >= 0.2 ? axial + 8.0 / 9.0 * bending : axial / 2.0 + bending;
    }
    static double En1993Interaction(SectionDemand d, SectionCapacity c) =>
        Math.Abs(d.N) / Math.Max(c.AxialCompression, Eps) + Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps) + Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, Eps);
    // EN 1995-1-1 §6.3.2(3) combined bending + axial compression: the axial term is SQUARED (σ_c0/(k_c·f_c0))², the
    // bending terms linear with the k_m = 0.7 minor-axis stress-redistribution factor (§6.1.6, rectangular section) —
    // distinct from the steel/concrete linear forms, so timber owns its own interaction (the k_c column-buckling is
    // already folded into c.AxialCompression by the en1995 axial-compression cell). FlexureMinor is +inf for timber
    // (no minor cell), so its term is 0 and a pure in-plane check degrades to (N/Nc)² + My/Mmaj.
    static double En1995Interaction(SectionDemand d, SectionCapacity c) {
        double axial = Math.Abs(d.N) / Math.Max(c.AxialCompression, Eps);
        return axial * axial + Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps) + 0.7 * Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, Eps);
    }
    static double LinearInteraction(SectionDemand d, SectionCapacity c) =>
        Math.Abs(d.N) / Math.Max(c.AxialCompression, Eps) + Math.Abs(d.My) / Math.Max(c.FlexureMajor, Eps);
}

// Check rows carry demand-component selectors and family applicability. Combined/Deflection carry no
// table cell and no force component — they are folded specially (interaction / FE deflection) in Check.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LimitState {
    public static readonly LimitState AxialTension     = new("axial-tension",     static d => Math.Max(d.N, 0.0),                            static f => f != MaterialFamily.Concrete && f != MaterialFamily.Masonry);
    public static readonly LimitState AxialCompression = new("axial-compression", static d => Math.Max(-d.N, 0.0),                           static _ => true);
    public static readonly LimitState FlexureMajor     = new("flexure-major",     static d => Math.Abs(d.My),                                static _ => true);
    public static readonly LimitState FlexureMinor     = new("flexure-minor",     static d => Math.Abs(d.Mz),                                static f => f == MaterialFamily.Steel || f == MaterialFamily.ColdFormedSteel);
    public static readonly LimitState ShearMajor       = new("shear-major",       static d => Math.Abs(d.Vy),                                static _ => true);
    public static readonly LimitState ShearMinor       = new("shear-minor",       static d => Math.Abs(d.Vz),                                static _ => true);
    public static readonly LimitState Combined         = new("combined",          static _ => 0.0,                                           static f => f == MaterialFamily.Steel || f == MaterialFamily.ColdFormedSteel || f == MaterialFamily.Timber);
    public static readonly LimitState Deflection       = new("deflection",        static _ => 0.0,                                           static _ => true);

    [UseDelegateFromConstructor]
    public partial double Demand(SectionDemand demand);

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

// Three interaction operands feed DesignCode.Interaction; each is an axis capacity or +inf when its cell is
// absent, so the ratio is 0 and the absent action does not constrain the interaction). The axial term uses the
// compression capacity for both senses (conservative: it is the smaller of tension/compression under buckling).
public readonly record struct SectionCapacity(double AxialCompression, double FlexureMajor, double FlexureMinor);

public readonly record struct MemberCheck(NodeId Member, LimitState State, double Demand, double Capacity, double Utilization);

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
        }.ToFrozenDictionary(static row => row.Key, static row => row.Rule);

    static double Capacity(DesignCode code, LimitState state, CapacityContext ctx) =>
        Capacities.TryGetValue((code.Key, state.Key), out Func<CapacityContext, double> rule) ? rule(ctx) : double.PositiveInfinity;

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
    static double ConcreteFr(double fc) => 0.62 * Math.Sqrt(Math.Max(fc / 1e6, 0.0)) * 1e6;   // modulus of rupture, Pa
    static double ConcreteVc(double fc) => 0.17 * Math.Sqrt(Math.Max(fc / 1e6, 0.0)) * 1e6;   // concrete shear stress, Pa

    // --- [GOVERNING] ---------------------------------------------------------------------
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks) =>
        request.Seismic.Match(
            Some: spec => RunSeismic(graph, request, spec, geometry, sink, clocks),
            None: () => RunStatic(graph, request, geometry, sink, clocks));

    static Fin<AssessmentResult> RunStatic(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks) =>
        from code   in DesignCode.For(request.Route)
        from model  in Project(graph, request, geometry)
        from _      in Validate(model, code)
        from resp   in Solve(model, clocks)
        from blob   in sink.Store(Artifact(resp, graph.Header.Tolerance))
        let checks   = model.Members.Bind(m => Check(m, resp[m.Id], code, model.Policy))
        let govern   = toSeq(checks.OrderByDescending(static c => c.Utilization)).Head
        from ratios in checks.TraverseM(static c => AssessmentFact.Ratio($"{c.Member.Value}/{c.State.Key}", c.Utilization)).As()
        from maxU in AssessmentFact.Ratio("max-utilization", govern.Map(static g => g.Utilization).IfNone(0.0))
        select AssessmentResult.Of(
            request.Route,
            ratios + Seq(
                    maxU,
                    govern.Map(static g => AssessmentFact.Reference("governing-member", g.Member)).IfNone(AssessmentFact.Text("governing-member", "none")),
                    AssessmentFact.Text("governing-limit-state", govern.Map(static g => g.State.Key).IfNone("none"))),
            govern.Map(static g => g.Utilization).IfNone(0.0),
            new Provenance("StructuralAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now),
            Some(blob));

    static Fin<Unit> Validate(FrameModel model, DesignCode code) =>
        model.Members.Find(m => !code.Family.Admits(m.Family))
            .Match(Some: m => Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing($"<material-code-mismatch:{m.Id.Value}:{m.Family.Key}!={code.Family.Key}>")),
                   None: () => Fin.Succ(unit));

    static Seq<MemberCheck> Check(StructuralMember member, MemberResponse response, DesignCode code, StructuralPolicy policy) {
        CapacityContext ctx = CapacityContext.Of(member, code, policy);
        SectionCapacity caps = new(
            Capacity(code, LimitState.AxialCompression, ctx), Capacity(code, LimitState.FlexureMajor, ctx), Capacity(code, LimitState.FlexureMinor, ctx));
        return LimitState.Items.ToSeq().Filter(state => state.Applies(member.Family)).Map(state => {
            double capacity = Capacity(code, state, ctx);
            double demand   = state.Demand(response.Demand);
            double util =
                state == LimitState.Combined   ? code.Interaction(response.Demand, caps)
                : state == LimitState.Deflection ? response.MaxDeflection / Math.Max(policy.DeflectionLimitRatio * member.Length, Eps)
                : demand / Math.Max(capacity, Eps);
            return new MemberCheck(member.Id, state, demand, capacity, util);
        });
    }

    static ReadOnlyMemory<byte> Artifact(FrozenDictionary<NodeId, MemberResponse> responses, double tolerance) {
        CanonicalWriter writer = new(tolerance);
        writer.Ordinal(responses.Count);
        foreach ((NodeId id, MemberResponse response) in responses.OrderBy(static row => row.Key.Value, StringComparer.Ordinal)) {
            writer.String(id.Value)
                .Double(response.Demand.N).Double(response.Demand.Vy).Double(response.Demand.Vz)
                .Double(response.Demand.My).Double(response.Demand.Mz).Double(response.Demand.T)
                .Double(response.MaxDeflection);
        }
        return writer.ToBytes();
    }
}
```

## [05]-[SEISMIC_ROUTE]

- Owner: `DesignSpectrum` `[SmartEnum<string>]` the code design-spectrum rows — EN 1998-1 Type 1, EN 1998-1 Type 2, ASCE 7 — each row carrying its piecewise pseudo-acceleration ordinate as delegate row data over the `SpectrumPolicy` parameters (site class, PGA/mapped accelerations, behavior factor q / response-modification R, damping correction), NEVER a hardcoded curve; `SpectrumPolicy` the parameter record; `ModalCombination` `[SmartEnum<string>]` the modal-combination axis (`srss` · `cqc` — CQC the closely-spaced-mode default, its cross-modal correlation the Der Kiureghian closed form over the damping ratio); `SeismicSpec` the request extension carrying the spectrum row, its policy, the combination row, and the participation floor; `RunSeismic` the route fold over the sparse modal.
- Entry: `static Fin<AssessmentResult> RunSeismic(ElementGraph graph, AssessmentRequest.Structural request, SeismicSpec spec, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks)` — the chain is fully named: `FrameLowering.Lower` builds the same mesh the static route uses, `SolveLane.Solve` under `SolvePolicy.CanonicalModalSparse` (the `arpack-shift-invert` row) recovers `(φ, λ)` and the modal participation factors off the owned lumped-mass field, the 90% modal-mass-participation floor gates TYPED — an achieved fraction below `spec.ParticipationFloor` is `ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, "<modal-mass-shortfall:…>")` naming the achieved fraction, never a silent truncation — the per-mode spectral demand reads `Sa(T_i)` off the `DesignSpectrum` row, the modal responses combine through the `ModalCombination` row, and the combined demands check through the SAME `(DesignCode, LimitState)` capacity table; the achieved participation and the combination key land as the `Participation`/`Combination` columns on `ComputeReceipt.Assessment`/`AssessmentWire`.
- Packages: the route composes `Solver/contract` (`SolveLane`, `SolvePolicy.CanonicalModalSparse`), `Solver/discretization` (the frame rows), Thinktecture.Runtime.Extensions, LanguageExt.Core — zero new packages (the seam `Discipline.Seismic` row and `StructuralCase.Seismic` already exist).
- Growth: a new code spectrum is one `DesignSpectrum` row carrying its ordinate delegate; a new combination rule is one `ModalCombination` row; a site-class table refinement is `SpectrumPolicy` data; zero new surface — a `SeismicAnalyzer` sibling runner is the rejected form (this is a structural ROUTE over the existing spine).
- Boundary: the sparse modal is the `arpack-shift-invert` `SolveMethod` row at building DOF — dense EVD stays the small-DOF terminal and a hand-rolled Lanczos is the deleted form; the spectrum is POLICY DATA (the codes the seam `Seismic` row itself names) and a hardcoded curve, a per-code method ladder, or a spectrum baked into the runner is the deleted form; CQC is the closely-spaced default because SRSS under-combines correlated modes — the choice is a ROW the receipt records, never a silent internal pick; the participation floor is a typed `(Solve, Numeric)` shortfall (deterministic — it caches as a Failed node under the lifecycle-spine law and never re-runs blind).

```csharp signature
// Code design spectra as POLICY ROWS: each row owns its piecewise pseudo-acceleration ordinate over the
// SpectrumPolicy parameters — EN 1998-1 §3.2.2.5 Type 1/2 (S, TB, TC, TD site columns; η damping
// correction; behavior factor q) and ASCE 7 §11.4 (SDS/SD1 plateau-and-decay; R/Ie) — never a hardcoded curve.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignSpectrum {
    public static readonly DesignSpectrum En1998Type1 = new("en1998-type1", static (p, t) => Eurocode(p, t));
    public static readonly DesignSpectrum En1998Type2 = new("en1998-type2", static (p, t) => Eurocode(p, t));
    public static readonly DesignSpectrum Asce7 = new("asce7", static (p, t) =>
        t < 0.2 * p.T1 ? p.Sds * (0.4 + 3.0 * t / p.T1) / p.Behavior
        : t <= p.T1 ? p.Sds / p.Behavior
        : t <= p.TLong ? p.Sd1 / (t * p.Behavior)
        : p.Sd1 * p.TLong / (t * t * p.Behavior));

    [UseDelegateFromConstructor]
    public partial double Sa(SpectrumPolicy policy, double period);

    // EN 1998-1 Type 1/2 share the piecewise form; the site-class S/TB/TC/TD columns and the η damping
    // correction η = max(√(10/(5+ξ)), 0.55) are SpectrumPolicy data.
    static double Eurocode(SpectrumPolicy p, double t) {
        double eta = Math.Max(Math.Sqrt(10.0 / (5.0 + 100.0 * p.DampingRatio)), 0.55);
        double ag = p.Pga * p.SoilFactor;
        return t <= p.Tb ? ag * (1.0 + t / p.Tb * (eta * 2.5 / p.Behavior - 1.0))
            : t <= p.Tc ? ag * eta * 2.5 / p.Behavior
            : t <= p.Td ? ag * eta * 2.5 / p.Behavior * (p.Tc / t)
            : ag * eta * 2.5 / p.Behavior * (p.Tc * p.Td / (t * t));
    }
}

// Site class, ground motion, behavior/response-modification, and damping as one parameter record — the
// spectrum rows read it, the content key folds it (a changed site class or q re-keys the assessment).
public sealed record SpectrumPolicy(
    string SiteClass, double Pga, double SoilFactor, double Tb, double Tc, double Td,
    double Sds, double Sd1, double T1, double TLong, double Behavior, double DampingRatio);

// SRSS and the CQC closely-spaced default: CQC's cross-modal correlation is the Der Kiureghian closed form
// ρ_ij = 8ξ²(1+r)r^1.5 / ((1−r²)² + 4ξ²r(1+r)²) with r = ω_j/ω_i — SRSS is its ρ_ij = δ_ij degenerate.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModalCombination {
    public static readonly ModalCombination Srss = new("srss", static (r, _, _) => Math.Sqrt(r.Sum(static v => v * v)));
    public static readonly ModalCombination Cqc = new("cqc", static (r, omega, xi) => {
        double sum = 0.0;
        for (int i = 0; i < r.Count; i++)
            for (int j = 0; j < r.Count; j++) {
                double ratio = omega[j] / Math.Max(omega[i], 1e-12);
                double rho = 8.0 * xi * xi * (1.0 + ratio) * Math.Pow(ratio, 1.5)
                    / (Math.Pow(1.0 - ratio * ratio, 2.0) + 4.0 * xi * xi * ratio * Math.Pow(1.0 + ratio, 2.0));
                sum += rho * r[i] * r[j];
            }
        return Math.Sqrt(Math.Max(sum, 0.0));
    });

    [UseDelegateFromConstructor]
    public partial double Combine(Seq<double> modal, Seq<double> omega, double dampingRatio);
}

// Seismic request extension carries spectrum row, policy, combination row, and
// typed participation floor (0.90 the code default) — all content-key folded.
public sealed record SeismicSpec(DesignSpectrum Spectrum, SpectrumPolicy Policy, ModalCombination Combination, double ParticipationFloor = 0.90);

public static partial class StructuralAnalysis {
    // Response-spectrum route uses arpack-shift-invert sparse modal: (φ, λ) plus participation off the owned
    // lumped-mass field, the 90% modal-mass floor a TYPED (Solve, Numeric) shortfall naming the achieved
    // fraction, per-mode Sa(T_i) demand off the spectrum row, modal responses combined through the
    // ModalCombination row, the combined demands checked through the SAME capacity table — a fully-named
    // chain, never a new runner.
    static Fin<AssessmentResult> RunSeismic(ElementGraph graph, AssessmentRequest.Structural request, SeismicSpec spec, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks) =>
        from code  in DesignCode.For(request.Route)
        from model in Project(graph, request, geometry)
        from lowered in FrameLowering.Lower(model)
        from problem in lowered.Problem(LoadCombinationSpec.SeismicUnit)
        from modal in SolveLane.Solve(problem, lowered.Mesh, SolvePolicy.CanonicalModalSparse, clocks)
        from gate  in Participation(modal, spec)
        let periods = modal.EigenValues.Map(static values => toSeq(values.ToArray()).Map(static w2 => 2.0 * Math.PI / Math.Sqrt(Math.Max(w2, 1e-12)))).IfNone(Seq<double>())
        let demands = SpectralDemands(model, lowered, modal, spec, periods)
        from blob in sink.Store(Artifact(demands, graph.Header.Tolerance))
        let checks  = model.Members.Bind(m => Check(m, demands[m.Id], code, model.Policy))
        let govern  = toSeq(checks.OrderByDescending(static c => c.Utilization)).Head
        from ratios in checks.TraverseM(static c => AssessmentFact.Ratio($"{c.Member.Value}/{c.State.Key}", c.Utilization)).As()
        from participation in AssessmentFact.Ratio("modal-mass-participation", gate)
        select AssessmentResult.Of(
            request.Route,
            ratios + Seq(
                    participation,
                    AssessmentFact.Text("modal-combination", spec.Combination.Key)),
            govern.Map(static g => g.Utilization).IfNone(0.0),
            new Provenance("StructuralAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now),
            Some(blob));

    // Typed participation floor compares ΣΓ² over recovered modes against SolveResult.TotalMass — the real
    // cumulative effective-mass ratio, never a self-normalized quotient that reads ~1 for any spectrum. A
    // shortfall is deterministic (Solve, Numeric) NAMING the achieved fraction — it caches as a Failed node
    // and never re-runs blind; an absent participation stream (a non-vibration result) is its own typed decline.
    static Fin<double> Participation(SolveResult modal, SeismicSpec spec) =>
        modal.Participation
            .Bind(gammas => modal.TotalMass.Map(total =>
                toSeq(gammas.ToArray()).Map(static g => g * g).Sum() / Math.Max(total, 1e-30)))
            .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, "<modal-mass-shortfall:participation-stream-absent>"))
            .Bind(fraction => fraction >= spec.ParticipationFloor
                ? Fin.Succ(fraction)
                : Fin.Fail<double>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<modal-mass-shortfall:achieved={fraction:0.000}:floor={spec.ParticipationFloor:0.00}>")));

    // Per-member combined seismic demand: each mode's spectral displacement field u_i = Γ_i·Sa(T_i)/ω_i²·φ_i
    // recovers member responses through the SAME StationRecovery.Demands kernel the static route uses, and the
    // per-mode component responses combine through the ModalCombination row component-by-component (SRSS/CQC over
    // Modal frequencies combine into one enveloped SectionDemand plus deflection per member, never a second recovery.
    static FrozenDictionary<NodeId, MemberResponse> SpectralDemands(FrameModel model, FrameLowered lowered, SolveResult modal, SeismicSpec spec, Seq<double> periods) {
        ReadOnlyMemory<double> shapes = modal.Field;                                    // column-major mode shapes, n DOFs x k modes
        Seq<double> omegaSq = modal.EigenValues.Map(static v => toSeq(v.ToArray())).IfNone(Seq<double>());
        Seq<double> gammas = modal.Participation.Map(static v => toSeq(v.ToArray())).IfNone(Seq<double>());
        Seq<double> omega = omegaSq.Map(static w2 => Math.Sqrt(Math.Max(w2, 1e-12)));
        int dofs = omegaSq.Count > 0 ? shapes.Length / omegaSq.Count : shapes.Length;
        // Per mode: scale the shape column into a displacement field, recover per-member demands via the shared kernel.
        Seq<Seq<(NodeId Id, SectionDemand Demand, double Deflection)>> perMode = toSeq(Enumerable.Range(0, omegaSq.Count)).Map(mode => {
            double scale = gammas[mode] * spec.Spectrum.Sa(spec.Policy, periods[mode]) / Math.Max(omegaSq[mode], 1e-12);
            double[] field = new double[dofs];
            shapes.Span.Slice(mode * dofs, dofs).CopyTo(field);                          // Exemption: the mode-column scale is the numeric kernel seam
            TensorPrimitives.Multiply(field, scale, field);
            return StationRecovery.Demands(model, lowered, LoadCombinationSpec.SeismicUnit, field);
        });
        return model.Members.Map(member => {
            Seq<(SectionDemand Demand, double Deflection)> rows = perMode.Map(demands =>
                demands.Find(row => row.Id == member.Id).Match(
                    Some: static row => (row.Demand, row.Deflection), None: static () => (SectionDemand.Zero, 0.0)));
            double Combined(Func<SectionDemand, double> component) =>
                spec.Combination.Combine(rows.Map(row => component(row.Demand)), omega, spec.Policy.DampingRatio);
            SectionDemand combined = new(
                Combined(static d => d.N), Combined(static d => d.Vy), Combined(static d => d.Vz),
                Combined(static d => d.My), Combined(static d => d.Mz), Combined(static d => d.T));
            return (member.Id, Response: new MemberResponse(combined, spec.Combination.Combine(rows.Map(static row => row.Deflection), omega, spec.Policy.DampingRatio)));
        }).ToFrozenDictionary(static row => row.Id, static row => row.Response);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
