# [COMPUTE_STRUCTURAL]

Rasm.Compute structural-analysis runner: the `Discipline.Structural` arm of the `Analysis/assessment` spine. It reads the concrete `Rasm.Element` `ElementGraph` directly (member `Node.Object` axes, the M7-resolved `SectionProperties` baked on the member's `ProfileSet` composition, the seam `MaterialPropertySet.Mechanical` strengths, the projected structural-connection/activity edges), folds them into one `FrameModel` idealization, solves it over a polymorphic `FrameBackend` (the 3D `BriefFiniteElement.Net` `Model` or the 2D-planar `FEALiTE2D` `Structure`, both factoring their linear solve through the ONE shared `api-csparse` owner — the `SparseLU` the `Tensor/factor#SPARSE_SOLVE` `FactorKind.Lu` row holds — never a second linear-algebra rail), recovers the per-combination internal-force-and-deflection `MemberResponse` envelope, runs the hand-rolled design-code checks (AISC 360 / EN 1993 / EN 1992 / NDS / EN 1995 / ACI 318 / TMS 402 / AISI S100 driven by a `(DesignCode, LimitState)` capacity table of REAL family-specific delegates, never imperative arms and never one family's formulas applied to every material, every structural family carrying BOTH its US and its Eurocode route — steel AISC 360 + EN 1993, concrete ACI 318 + EN 1992, timber NDS + EN 1995), and returns the governing utilization as one `AssessmentResult` fact stream the spine writes back. Section properties resolve ONCE upstream — the `VividOrange`-backed `ProfileRef`→section resolution (M7) is performed by the `Rasm.Materials` projector and baked onto the `ProfileSet` composition, so this consumer reads the resolved `SectionProperties` off the graph and Compute admits no VividOrange (one owner, in Materials). The FE library is assembler and solver only: BFE is confined to the sparse-factored `BarElement` frame path (its embedded dense `DenseLU` is binary-incompatible with the unified `CSparse 4.4.0` pin, so the linear solve injects the Rasm `RasmSolverFactory` CSparse-`SparseLU` adapter via `Solve(ISolverFactory)` and never loads `DenseLU`), code-checking is hand-rolled here, and continuum multi-physics is the `Solver/contract#SOLVE_CONTRACT` `SolveLane` this runner never re-derives. Buckling, lateral-torsional buckling, and deflection are REAL: the column/LTB capacity reads the member's unbraced length and end-fixity-derived effective-length factor, and the deflection check reads the FE displacement envelope — no placeholder curve, no constant knockdown, no `0.0` sentinel.

## [01]-[INDEX]

- [01]-[FRAME_MODEL]: the `FrameModel` idealization folded from the graph, the `SolverBackend`/`MemberLoad`/`StructuralCase` vocabulary, the `MemberSupport`/`LoadCombinationSpec`/`StructuralPolicy`/`StructuralMember` inputs (the seam contract `Analysis/assessment` carries on the request), and the `StructuralReads` discipline graph-read boundary.
- [02]-[FRAME_BACKEND]: the `FrameBackend` dispatch (`Bfe` 3D / `Fealite` 2D planar) FE assemble-solve-recover over the shared `CSparse` owner, the `RasmSolverFactory` injection adapter, and the per-combination `MemberResponse` internal-force-and-deflection envelope every limit state reads.
- [03]-[DESIGN_CHECK]: the `MaterialFamily`/`SafetyFormat`/`DesignCode`/`LimitState` vocabulary, the `(DesignCode, LimitState)` capacity table of real slenderness-aware delegates, the `CapacityContext`/`SectionCapacity`/`MemberCheck` carriers, the interaction rule, and the `StructuralAnalysis.Run` governing-utilization fact stream.

## [02]-[FRAME_MODEL]

- Owner: `FrameModel` the analysis idealization (members, combinations, policy); `SolverBackend` the 3-row backend axis (`Analysis/assessment` content-keys its `Key`); `MemberLoad` the per-member applied-action `[Union]` (`Point`/`Uniform`/`Trapezoid`); `StructuralCase` the load-case `[SmartEnum<string>]`; `MemberSupport` the 6-DOF restraint at a member end; `LoadCombinationSpec` the factored case map; `StructuralPolicy` the backend/deflection/station policy; `StructuralMember` the resolved member (axis, section, strength, family, loads, supports). `SolverBackend`/`StructuralCase`/`MemberSupport`/`LoadCombinationSpec`/`StructuralPolicy` are the seam contract `AssessmentRequest.Structural` carries and `Analysis/assessment` `CanonicalBytes` folds — their field shape is load-bearing across the spine.
- Entry: `static Fin<FrameModel> Project(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry)` — folds the request `Targets` member `Node.Object`s into the idealization, reading each member's `StructuralReads.AxisOf` (the analytical line resolved one-hop by content key through the seam `GeometrySource` port off `member.Representations.Axis`), `graph.MechanicalOf` (the seam strength), `graph.SectionOf` (the seam Op-free M7 accessor reading the baked `ProfileSet` section directly — the seam owns the section read, so the runner never re-derives a discipline-local section accessor), `StructuralReads.SupportsOf`, and `StructuralReads.LoadsOf`, `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a member lacks a section, a strength, or an axis.
- Auto: self-weight derives per member from `Section.Area.Si × Mechanical.Density.Si × StandardGravity` as a global-down `Uniform` force-per-length in the `Dead` case; the request's projected `MemberLoad`s supply the live/wind/snow/seismic actions; `LoadCombinationSpec` factors the cases per code (ASCE 7 / EN 1990) so a combination is data the backend reads, never a re-modelled load set; the member's `MaterialFamily` is `Classify`-derived from the strength for the FE material model and validated against the route's `DesignCode.Family` at `Check`.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`), Rasm.Element (project — `ElementGraph`, `Node`, `NodeId`, the seam-owned host-neutral `Vector3` coordinate the `AxisCurve` carries and the load vectors reuse, `AxisCurve`, `GeometrySource` the analytical-line resolution port, `RepresentationContentHash`, `SectionProperties`, `MaterialPropertySet`, `Relationship`, `PropertyName`, `PropertyValue`, `MeasureValue`), BCL inbox (`FrozenDictionary`).
- Growth: a new applied-action kind is one `MemberLoad` case (both backends widen their total load `Switch`); a new restraint is data on `MemberSupport`; a new combination basis is one `LoadCombinationSpec` row — the idealization widens by data, the backends and checks re-read it.
- Boundary: the section is the M7-resolved seam `SectionProperties` read once off the `ProfileSet` composition — the `VividOrange` `ProfileRef`→section resolution happens ONCE in the `Rasm.Materials` projector, so this runner reads `Area`/`Iyy`/`Izz`/`J`/`Wply`/`Wplz`/`Wely`/`Welz`/`AvY`/`AvZ`/`RadiusOfGyrationMinor`/`Depth`/`Width`/`LeastDimension` (`SectionProperties` carries the both-axis shear areas `AvY`/`AvZ` and both-axis radii, so the per-axis shear check reads its own area) and never re-resolves a profile, and Compute admits no VividOrange; the strength is the seam `Mechanical.YieldStrength`/`UltimateStrength`/`YoungsModulus`/`ShearModulus`/`Density`/`PoissonsRatio` (the seam field is `PoissonsRatio`, never `PoissonRatio`) read off the member's associated material; the analytical line is the seam `AxisCurve` (`Start`/`End`/`Up`) the Bim projector content-keyed under `member.Representations.Axis` (NEVER inlined on the node — the deleted §4-RT-M2 phantom `Node.Object.Axis`), the runner resolving it ONE-HOP by that content key through the seam `GeometrySource` port the spine threads (the app wires the resolver over the Persistence object-store byte-stream) — `MemberAxis` is NOT re-declared (it mirrored `AxisCurve` field-for-field, the deleted parallel shape), coplanarity riding a `StructuralReads` `AxisCurve` fold and length the member's own `Vector3.Distance` derivation; supports and loads traverse the projected `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` neutral `Generic` edges by wire-name (the C5 stranded structural families ride the `Generic` passthrough, the Bim projector stamping the 6-DOF restraint, the applied components, the end discriminant, and the load kind), so the runner reads the idealization fully baked, never re-reading IFC; a member with no section/strength/axis rails the typed input fault, never a defaulted section.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The backend axis Analysis/assessment content-keys (Backend.Key) — Auto routes a coplanar in-plane frame to the
// lighter 2D FEALiTE solver and a general 3D frame to BFE; Spatial/Planar force the arm. Three rows, never a
// Bfe3DAnalyzer/Fealite2DAnalyzer sibling family.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolverBackend {
    public static readonly SolverBackend Auto    = new("auto");
    public static readonly SolverBackend Spatial = new("spatial");
    public static readonly SolverBackend Planar  = new("planar");
}

// The per-member applied action — one polymorphic load both backends map through their total Switch: a Point at a
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
public readonly record struct MemberSupport(NodeId At, bool AtStart, bool Dx, bool Dy, bool Dz, bool Rx, bool Ry, bool Rz) {
    public bool RotationallyFixed => Rx && Ry && Rz;
}

public sealed record LoadCombinationSpec(string Label, FrozenDictionary<StructuralCase, double> Factors);

// Backend + serviceability + sampling policy. Exactly three fields — Analysis/assessment CanonicalBytes folds
// Backend.Key/DeflectionLimitRatio/StationCount, so adding a field here without folding it there would collide two
// analyses in the content-keyed cache; the effective-length factor and unbraced length are DERIVED from the member's
// end fixity and span (real engineering), never a free policy knob that would silently bypass the key.
public sealed record StructuralPolicy(SolverBackend Backend, double DeflectionLimitRatio, int StationCount) {
    public static readonly StructuralPolicy Canonical = new(SolverBackend.Auto, DeflectionLimitRatio: 1.0 / 250.0, StationCount: 11);
}

public sealed record StructuralMember(
    NodeId Id, AxisCurve Axis, SectionProperties Section, MaterialPropertySet.Mechanical Strength,
    Option<MaterialPropertySet.Orthotropic> Directional, MaterialFamily Family, Seq<MemberLoad> Loads, Seq<MemberSupport> Supports) {
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

public sealed record FrameModel(Seq<StructuralMember> Members, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy) {
    public bool Planar => Members.IsEmpty || Members.ForAll(m => m.Axis.Coplanar(Members.Head.Axis.Start.Z));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    public static Fin<FrameModel> Project(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry) =>
        request.Targets.Fold(
            Fin.Succ(Seq<StructuralMember>()),
            (acc, id) => acc.Bind(members =>
                from axis     in graph.AxisOf(id, geometry)
                from strength in graph.MechanicalOf(id).ToFin(MissingInput(id, "mechanical"))
                from section  in graph.SectionOf(id).ToFin(MissingInput(id, "section"))
                // The realized seam Orthotropic case (Composition/material#MATERIAL_PROPERTY, same Discipline.Structural,
                // discriminated by case TYPE) — an OPTIONAL directional-stiffness refinement read off the seam graph via
                // the ergonomic props.Orthotropic; a directional material (timber along/across grain) carries its
                // independent G ≈ E0/16 here, an isotropic member carries None and the EC5 LTB falls back to the derived
                // Mechanical shear. The isotropic Mechanical stays REQUIRED (every member has one E/ν the family hint and
                // the FE constitutive model read); the Orthotropic is the §6.3.3 shear-stiffness input the seam now exposes.
                let directional = graph.PropertiesOf(id).Orthotropic
                let family     = MaterialFamily.Classify(strength)
                let selfWeight = new MemberLoad.Uniform(StructuralCase.Dead,
                    new Vector3(0d, 0d, -(section.Area.Si * strength.Density.Si * StandardGravity)))
                select members.Add(new StructuralMember(
                    id, axis, section, strength, directional, family, graph.LoadsOf(id).Add(selfWeight), graph.SupportsOf(id)))))
            .Map(members => new FrameModel(members, request.Combinations, request.Policy));

    static Error MissingInput(NodeId id, string what) =>
        new ComputeFault.AssessmentInputMissing($"<member-missing-{what}:{id.Value}>");
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The Compute-OWNED discipline graph reads — extensions on the seam ElementGraph composing the seam no-Op primitives
// (MaterialsOf/MechanicalOf/Find/EdgesAt) and the projected neutral Generic structural edges by wire-name, the same
// shape Analysis/energy's EnergyGraphReads takes. The seam owns the material/section/mechanical reads (it owns those
// nodes) and the GeometrySource decode CONTRACT; the discipline physics — axis interpretation, 6-DOF restraints,
// applied actions — lives here, never in the seam. AxisOf resolves the analytical line ONE-HOP by content key through
// the threaded GeometrySource port (off member.Representations.Axis, never a phantom node field), and AxisCurve.Length/
// Coplanar fold that resolved line the runner reasons over in double precision.
public static class StructuralReads {
    const string ConnectsMember   = "IfcRelConnectsStructuralMember";
    const string ConnectsActivity = "IfcRelConnectsStructuralActivity";

    public static bool Coplanar(this AxisCurve a, double z) => Math.Abs(a.Start.Z - z) < 1e-6 && Math.Abs(a.End.Z - z) < 1e-6;

    // The idealized analytical line resolved ONE-HOP by content key through the seam GeometrySource port — the Object node
    // carries NO inline Axis coordinate (the seam carries only `member.Representations.Axis`, an Option<UInt128> content key
    // into the blob store), so the runner reads the member's Object node, pulls its `Representations.Axis` key, and decodes
    // it to an AxisCurve through the app-wired resolver — never a phantom node field. A member with no Object node, no Axis
    // key, or an undecodable blob rails the typed input fault, never a defaulted axis.
    public static Fin<AxisCurve> AxisOf(this ElementGraph graph, NodeId member, GeometrySource geometry) =>
        graph.Find<Node.Object>(member).Bind(o => geometry.Axis(o.Representations))
            .ToFin(new ComputeFault.AssessmentInputMissing($"<member-axis-absent:{member.Value}>"));

    // One MemberSupport per structural-connection edge the member relates — the fixity booleans and the start/end
    // discriminant read off the neutral Generic edge payload the projector baked from IfcBoundaryNodeCondition.
    public static Seq<MemberSupport> SupportsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsMember && g.Relating == member
            ? Some(new MemberSupport(g.Related, Flag(g, StructuralAnalysis.WireAtStart),
                Fix(g, "TranslationX"), Fix(g, "TranslationY"), Fix(g, "TranslationZ"),
                Fix(g, "RotationX"), Fix(g, "RotationY"), Fix(g, "RotationZ")))
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
        _           => new MemberLoad.Point(CaseOf(g), Vec(g, "Force"), Vec(g, "Moment"), Si(g, "Station") is var s && s > 0.0 ? s : 0.5),
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
}
```

## [03]-[FRAME_BACKEND]

- Owner: the `Solve` backend dispatch (the 3D `BriefFiniteElement.Net` `SolveSpatial`, the 2D-planar `FEALiTE2D` `SolvePlanar`) selected by `StructuralPolicy.Backend` (Auto routes a coplanar in-plane frame to `Fealite`, else `Bfe`); `SectionDemand` the per-combination internal-force envelope; `MemberResponse` the demand-plus-deflection carrier every limit state reads; `RasmSolverFactory` the `BriefFiniteElementNet.Common.ISolverFactory` adapter routing BFE's linear solve through the shared `api-csparse` `SparseLU` owner.
- Entry: `static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model)` — selects the backend, assembles the FE model (each member a `BarElement`/`FrameElement2D` over its `UniformParametric1DSection`/`Generic2DSection` and `UniformIsotropicMaterial`/`GenericIsotropicMaterial`, each `MemberSupport` a 6-DOF `Constraint`/`NodalSupport` placed on the endpoint-resolved shared joint, each `MemberLoad`+`LoadCombinationSpec` mapped through a TOTAL load `Switch`), solves through the injected CSparse factory, and recovers the worst-station `SectionDemand` plus the worst-station deflection per member per combination, `Fin<T>` lowering a `FEALiTE2D` `AnalysisStatus.Failure` or a BFE singular-factorization throw onto `ComputeFault.AnalysisRunFailed`.
- Auto: BFE's linear solve is injected with `RasmSolverFactory` (`model.Solve(factory)`) whose `CreateSolver(SparseMatrix)` returns a `CSparse.Double.Factorization.SparseLU`-backed `ISolver` adapter, so the structural lane factors through the SAME CSparse owner the continuum lane holds and BFE's embedded dense `DenseLU` (binary-incompatible with the `CSparse 4.4.0` pin) is never loaded — the runner stays on the sparse-factored `BarElement` frame path; `FEALiTE2D` factors its `StructuralStiffnessMatrix` through the same CSparse owner natively; shared joints merge by tolerance-quantized coordinate (not fragile exact-float `Vector3` equality); the demand folds each member's exact internal force over the policy stations and every combination, and the deflection folds the transverse FE displacement over the same stations.
- Packages: BriefFiniteElement.Net (`Model`/`BarElement`/`UniformParametric1DSection`/`UniformIsotropicMaterial`/`Constraint`/`ConcentratedLoad`/`UniformLoad`/`PartialNonUniformLoad`/`LoadCase`/`LoadCombination`/`Force`/`Displacement`), BriefFiniteElementNet.Common (`ISolver`/`ISolverFactory`), CSparse (the shared `SparseLU` factorization via `Tensor/factor#SPARSE_SOLVE`), FEALiTE2D (`Structure`/`FrameElement2D`/`Node2D`/`NodalSupport`/`Generic2DSection`/`GenericIsotropicMaterial`/`FramePointLoad`/`FrameUniformLoad`/`FrameTrapezoidalLoad`/`LinearMeshSegment`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new FE backend is one `Solve` arm; the response envelope is one `MemberResponse` shape the checks read regardless of backend; a new load kind widens both backends' total load `Switch` at compile time — a `Bfe3DAnalyzer`/`Fealite2DAnalyzer` sibling family is the rejected form.
- Boundary: BFE is confined to the sparse-factored `BarElement` frame solve — continuum `TriangleElement`/`TetrahedronElement`/`HexahedralElement` are NOT used (their `DenseColumnMajorStorage` path routes through the binary-incompatible `DenseLU` under `CSparse 4.4.0`), and continuum multi-physics is the `Solver/contract#SOLVE_CONTRACT` `SolveLane`; the BFE local frame orders moments `(Mx=torsion, My/Mz=bending)`, so the demand maps `SectionDemand(N=Fx, Vy=Fy, Vz=Fz, My=My, Mz=Mz, T=Mx)` — never `T=Mz`/`My=Mx`, the silently-wrong torsion/bending swap; the FE result carriers (`Force`/`Displacement`/`LinearMeshSegment`) carry RAW `double` components in one consistent SI system, re-attached at the `SectionDemand`/`MemberResponse` boundary; every `MemberLoad` case maps to its FE primitive (Point→`ConcentratedLoad`/`FramePointLoad`, Uniform→`UniformLoad`/`FrameUniformLoad`, Trapezoid→`PartialNonUniformLoad`/`FrameTrapezoidalLoad`) — neither backend drops a load kind or flattens a trapezoid to a uniform average; a hand-rolled stiffness assembler or a re-typed section beside the FE library is the rejected form; a singular/ill-conditioned system surfaces as a typed `AnalysisRunFailed`, never an exception crossing the rail.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The per-combination internal-force envelope — backend-neutral; each component is the signed worst-magnitude over
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

// --- [SERVICES] ----------------------------------------------------------------------------
// One ISolverFactory routes BFE's linear solve through the shared api-csparse owner — the same CSparse SparseLU the
// Tensor/factor#SPARSE_SOLVE FactorKind.Lu row factors through — so the structural lane never loads BFE's
// CSparse-3.5-era embedded DenseLU under the unified 4.4.0 pin (DenseLU sits only on the continuum element path this
// runner never touches). The adapter implements BFE's Common.ISolver over a CSparse factorization of A; LU (not
// Cholesky) handles the released-DOF indefinite reduced system, a structurally singular factor surfacing as the NaN
// solution the BFE solve raises and the Solve rail lowers to AnalysisRunFailed.
public sealed class RasmSolverFactory : BriefFiniteElementNet.Common.ISolverFactory {
    public static readonly RasmSolverFactory Instance = new();
    public BriefFiniteElementNet.Common.ISolver CreateSolver(CSparse.Double.SparseMatrix a) => new CSparseSolver(a);

    sealed class CSparseSolver(CSparse.Double.SparseMatrix a) : BriefFiniteElementNet.Common.ISolver {
        CSparse.Double.Factorization.SparseLU? factor;
        public CSparse.Double.SparseMatrix A { get; set; } = a;
        public bool IsInitialized => factor is not null;
        public void Initialize() => factor = CSparse.Double.Factorization.SparseLU.Create(A, CSparse.ColumnOrdering.MinimumDegreeAtPlusA, 1.0);
        public void Solve(double[] b, double[] x) {
            factor ??= CSparse.Double.Factorization.SparseLU.Create(A, CSparse.ColumnOrdering.MinimumDegreeAtPlusA, 1.0);
            factor.Solve(b, x);
        }
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    public static Fin<FrozenDictionary<NodeId, MemberResponse>> Solve(FrameModel model) =>
        model.Members.IsEmpty
            ? Fin.Succ(FrozenDictionary<NodeId, MemberResponse>.Empty)
            : model.Policy.Backend == SolverBackend.Planar || (model.Policy.Backend == SolverBackend.Auto && model.Planar)
                ? SolvePlanar(model)
                : SolveSpatial(model);

    // --- [BFE_3D] ------------------------------------------------------------------------
    static Fin<FrozenDictionary<NodeId, MemberResponse>> SolveSpatial(FrameModel model) {
        const double tol = 1e-6;
        BriefFiniteElementNet.Model bfe = new();
        Dictionary<(long, long, long), BriefFiniteElementNet.Node> joints = new();
        Dictionary<NodeId, BriefFiniteElementNet.Elements.BarElement> bars = new();
        Dictionary<(long, long, long), (bool Dx, bool Dy, bool Dz, bool Rx, bool Ry, bool Rz)> fixity = new();
        foreach (StructuralMember member in model.Members) {
            BriefFiniteElementNet.Node start = JointAt(bfe, joints, member.Axis.Start, tol);
            BriefFiniteElementNet.Node end   = JointAt(bfe, joints, member.Axis.End, tol);
            BriefFiniteElementNet.Elements.BarElement bar = new(start, end) {
                Section  = new BriefFiniteElementNet.Sections.UniformParametric1DSection(member.Section.Area.Si, member.Section.Iyy.Si, member.Section.Izz.Si, member.Section.J.Si),
                Material = BriefFiniteElementNet.Materials.UniformIsotropicMaterial.CreateFromYoungPoisson(member.Strength.YoungsModulus.Si, member.Strength.PoissonsRatio),
                Behavior = BriefFiniteElementNet.Elements.BarElementBehaviours.FullFrame,
            };
            bfe.Elements.Add(bar);
            bars[member.Id] = bar;
            // Accumulate every support's fixity onto its endpoint-resolved joint (UNION over a free start), so a shared
            // joint a free-default node would lose is fixed once and a second support there never overwrites the first.
            foreach (MemberSupport s in member.Supports) {
                var jk = Key3(s.AtStart ? member.Axis.Start : member.Axis.End, tol);
                var f = fixity.GetValueOrDefault(jk);
                fixity[jk] = (f.Dx || s.Dx, f.Dy || s.Dy, f.Dz || s.Dz, f.Rx || s.Rx, f.Ry || s.Ry, f.Rz || s.Rz);
            }
            foreach (MemberLoad load in member.Loads) { bar.Loads.Add(BfeLoad(load)); }
        }
        foreach (var (jk, f) in fixity) {
            joints[jk].Constraints = new BriefFiniteElementNet.Constraint(Dof(f.Dx), Dof(f.Dy), Dof(f.Dz), Dof(f.Rx), Dof(f.Ry), Dof(f.Rz));
        }
        return Try.lift(() => { bfe.Solve(RasmSolverFactory.Instance); return Unit.Default; }).Run()
            .MapFail(static error => (Error)new ComputeFault.AnalysisRunFailed($"<bfe-solve-break:{error.Message}>"))
            .Bind(_ => Recover(model, m => SampleBfe(bars[m.Id], m, model)));
    }

    static (long, long, long) Key3(Vector3 p, double tol) =>
        ((long)Math.Round(p.X / tol), (long)Math.Round(p.Y / tol), (long)Math.Round(p.Z / tol));

    static BriefFiniteElementNet.Node JointAt(BriefFiniteElementNet.Model bfe, Dictionary<(long, long, long), BriefFiniteElementNet.Node> joints, Vector3 p, double tol) {
        var key = Key3(p, tol);
        if (joints.TryGetValue(key, out var existing)) { return existing; }
        BriefFiniteElementNet.Node node = new(p.X, p.Y, p.Z);
        joints[key] = node;
        bfe.Nodes.Add(node);
        return node;
    }

    static BriefFiniteElementNet.DofConstraint Dof(bool fixedDof) =>
        fixedDof ? BriefFiniteElementNet.DofConstraint.Fixed : BriefFiniteElementNet.DofConstraint.Released;

    // Total Switch over MemberLoad — every kind maps to its true BFE primitive: a Trapezoid is a PartialNonUniformLoad
    // with a linear SingleVariablePolynomial severity over [-1,1], NEVER a UniformLoad averaging the two ends.
    static BriefFiniteElementNet.ElementalLoad BfeLoad(MemberLoad load) => load.Switch(
        point:     p => new BriefFiniteElementNet.Loads.ConcentratedLoad(
                            new BriefFiniteElementNet.Force(p.Force.X, p.Force.Y, p.Force.Z, p.Moment.X, p.Moment.Y, p.Moment.Z),
                            new BriefFiniteElementNet.IsoPoint(2.0 * p.Station - 1.0), BriefFiniteElementNet.CoordinationSystem.Global)
                          { Case = BfeCase(p.Case) },
        uniform:   u => BfeUniform(u.Case, u.ForcePerLength),
        trapezoid: t => BfeTrapezoid(t.Case, t.Start, t.End));

    static BriefFiniteElementNet.ElementalLoad BfeUniform(StructuralCase kase, Vector3 w) {
        double magnitude = Vector3.Distance(w, default);   // |w| via the seam-declared static Distance (no System.Numerics .Length())
        BriefFiniteElementNet.Vector direction = magnitude > Eps
            ? new BriefFiniteElementNet.Vector(w.X / magnitude, w.Y / magnitude, w.Z / magnitude)
            : new BriefFiniteElementNet.Vector(0, 0, -1);
        return new BriefFiniteElementNet.Loads.UniformLoad(BfeCase(kase), direction, magnitude, BriefFiniteElementNet.CoordinationSystem.Global);
    }

    static BriefFiniteElementNet.ElementalLoad BfeTrapezoid(StructuralCase kase, Vector3 startW, Vector3 endW) {
        double s = Vector3.Distance(startW, default), e = Vector3.Distance(endW, default);
        Vector3 axis = s >= e ? startW : endW;
        double mag = Math.Max(s, e);
        BriefFiniteElementNet.Vector direction = mag > Eps
            ? new BriefFiniteElementNet.Vector(axis.X / mag, axis.Y / mag, axis.Z / mag)
            : new BriefFiniteElementNet.Vector(0, 0, -1);
        double mid = 0.5 * (s + e), slope = 0.5 * (e - s);            // severity(xi) = slope*xi + mid over xi in [-1, 1]
        return new BriefFiniteElementNet.Loads.PartialNonUniformLoad {
            Case = BfeCase(kase), Direction = direction, CoordinationSystem = BriefFiniteElementNet.CoordinationSystem.Global,
            StartLocation = new BriefFiniteElementNet.IsoPoint(-1.0), EndLocation = new BriefFiniteElementNet.IsoPoint(1.0),
            // SingleVariablePolynomial coefs are DESCENDING powers ([x^1, x^0]); (slope, mid) -> slope*xi + mid, so the
            // severity is +s at the start (xi=-1) and +e at the end (xi=+1) — (mid, slope) would yield -s at the start.
            SeverityFunction = new BriefFiniteElementNet.Mathh.SingleVariablePolynomial(slope, mid),
        };
    }

    static BriefFiniteElementNet.LoadCase BfeCase(StructuralCase c) => new(c.Key, BfeLoadType(c));
    static BriefFiniteElementNet.LoadType BfeLoadType(StructuralCase c) =>
        c == StructuralCase.Dead ? BriefFiniteElementNet.LoadType.Dead : c == StructuralCase.Live ? BriefFiniteElementNet.LoadType.Live
        : c == StructuralCase.Snow ? BriefFiniteElementNet.LoadType.Snow : c == StructuralCase.Wind ? BriefFiniteElementNet.LoadType.Wind
        : BriefFiniteElementNet.LoadType.Quake;

    static BriefFiniteElementNet.LoadCombination BfeCombo(LoadCombinationSpec spec) {
        BriefFiniteElementNet.LoadCombination combo = new();
        foreach (var (kase, factor) in spec.Factors) { combo[BfeCase(kase)] = factor; }
        return combo;
    }

    static MemberResponse SampleBfe(BriefFiniteElementNet.Elements.BarElement bar, StructuralMember member, FrameModel model) =>
        model.Combinations.Fold(MemberResponse.Zero, (env, spec) => {
            BriefFiniteElementNet.LoadCombination combo = BfeCombo(spec);
            return toSeq(Enumerable.Range(0, model.Policy.StationCount)).Fold(env, (e, station) => {
                double xi = 2.0 * station / Math.Max(model.Policy.StationCount - 1, 1) - 1.0;
                BriefFiniteElementNet.Force f = bar.GetExactInternalForceAt(xi, combo);
                BriefFiniteElementNet.Displacement d = bar.GetInternalDisplacementAt(xi, combo);
                // BFE Displacement components are upper-cased (DX/DY/DZ/RX/RY/RZ), unlike Force (Fx/Fy/Fz/Mx/My/Mz);
                // the transverse deflection is the global-YZ magnitude excluding the axial DX component.
                return e.Max(new SectionDemand(f.Fx, f.Fy, f.Fz, f.My, f.Mz, f.Mx), Math.Sqrt(d.DY * d.DY + d.DZ * d.DZ));
            });
        });

    // --- [FEALITE_2D] --------------------------------------------------------------------
    static Fin<FrozenDictionary<NodeId, MemberResponse>> SolvePlanar(FrameModel model) {
        const double tol = 1e-6;
        FEALiTE2D.Structure.Structure structure = new() { LinearMesher = new FEALiTE2D.Meshing.LinearMesher() };
        Dictionary<(long, long), FEALiTE2D.Elements.Node2D> joints = new();
        Dictionary<NodeId, FEALiTE2D.Elements.FrameElement2D> frames = new();
        Dictionary<(long, long), (bool Ux, bool Uy, bool Rz)> fixity = new();
        FrozenDictionary<StructuralCase, FEALiTE2D.Loads.LoadCase> cases = FealiteCases(model);  // ONE instance per case
        foreach (StructuralMember member in model.Members) {
            FEALiTE2D.Elements.Node2D start = PlanarJoint(joints, member.Axis.Start, tol);
            FEALiTE2D.Elements.Node2D end   = PlanarJoint(joints, member.Axis.End, tol);
            foreach (MemberSupport s in member.Supports) {
                var jk = Key2(s.AtStart ? member.Axis.Start : member.Axis.End, tol);
                var f = fixity.GetValueOrDefault(jk);
                fixity[jk] = (f.Ux || s.Dx, f.Uy || s.Dy, f.Rz || s.Rz);
            }
            FEALiTE2D.Elements.FrameElement2D frame = new(start, end, member.Id.Value) { CrossSection = PlanarSection(member) };
            structure.AddElement(frame, addNodes: true);
            foreach (MemberLoad load in member.Loads) { frame.Loads.Add(FealiteLoad(load, cases, member.Length)); }
            frames[member.Id] = frame;
        }
        foreach (var (jk, f) in fixity) { joints[jk].Support = new FEALiTE2D.Elements.NodalSupport(f.Ux, f.Uy, f.Rz); }
        structure.LoadCasesToRun.AddRange(cases.Values);
        structure.Solve();
        return structure.AnalysisStatus == FEALiTE2D.Structure.AnalysisStatus.Failure
            ? Fin.Fail<FrozenDictionary<NodeId, MemberResponse>>(new ComputeFault.AnalysisRunFailed($"<fealite-singular:dof={structure.nDOF}>"))
            : Recover(model, m => PlanarResponse(structure, frames[m.Id], cases, model));
    }

    static (long, long) Key2(Vector3 p, double tol) => ((long)Math.Round(p.X / tol), (long)Math.Round(p.Y / tol));

    static FEALiTE2D.Elements.Node2D PlanarJoint(Dictionary<(long, long), FEALiTE2D.Elements.Node2D> joints, Vector3 p, double tol) {
        var key = Key2(p, tol);
        if (joints.TryGetValue(key, out var existing)) { return existing; }
        FEALiTE2D.Elements.Node2D node = new(p.X, p.Y, $"n{key.Item1}_{key.Item2}");
        joints[key] = node;
        return node;
    }

    static FEALiTE2D.CrossSections.Generic2DSection PlanarSection(StructuralMember m) => new(
        m.Section.Area.Si, m.Section.AvY.Si, m.Section.AvZ.Si, m.Section.Iyy.Si, m.Section.Izz.Si, m.Section.J.Si,
        m.Section.Depth.Si, m.Section.Width.Si,
        new FEALiTE2D.Materials.GenericIsotropicMaterial {
            E = m.Strength.YoungsModulus.Si, U = m.Strength.PoissonsRatio, Alpha = m.Strength.ThermalExpansionPerK,
            Gama = m.Strength.Density.Si, MaterialType = FealiteMaterial(m.Family),
        });

    static FEALiTE2D.Materials.MaterialType FealiteMaterial(MaterialFamily f) =>
        f == MaterialFamily.Concrete ? FEALiTE2D.Materials.MaterialType.Concrete
        : f == MaterialFamily.Timber ? FEALiTE2D.Materials.MaterialType.Timber : FEALiTE2D.Materials.MaterialType.Steel;

    static FrozenDictionary<StructuralCase, FEALiTE2D.Loads.LoadCase> FealiteCases(FrameModel model) =>
        model.Combinations.Bind(static c => c.Factors.Keys.ToSeq())
            .Append(model.Members.Bind(static m => m.Loads.Map(LoadCaseOf)))
            .Distinct()
            .ToFrozenDictionary(static c => c, static c => new FEALiTE2D.Loads.LoadCase(c.Key, FealiteLoadType(c)));

    static StructuralCase LoadCaseOf(MemberLoad l) => l.Switch(point: static p => p.Case, uniform: static u => u.Case, trapezoid: static t => t.Case);

    // FEALiTE has no Snow case — snow is a gravity action mapped to Live; the others map directly.
    static FEALiTE2D.Loads.LoadCaseType FealiteLoadType(StructuralCase c) =>
        c == StructuralCase.Dead ? FEALiTE2D.Loads.LoadCaseType.Dead : c == StructuralCase.Wind ? FEALiTE2D.Loads.LoadCaseType.Wind
        : c == StructuralCase.Seismic ? FEALiTE2D.Loads.LoadCaseType.Seismic : FEALiTE2D.Loads.LoadCaseType.Live;

    // Total Switch over MemberLoad in the planar lane too — Point and Trapezoid are NO longer dropped. FEALiTE's
    // FramePointLoad L1 is an ABSOLUTE distance from the start node (it divides L1/length internally), so the seam's
    // [0,1] Station fraction scales by the member length here; BFE's IsoPoint is the dimensionless [-1,1] iso-coordinate
    // (2*Station-1), hence the divergent map. FrameUniformLoad/FrameTrapezoidalLoad default L1=L2=0 -> full span.
    static FEALiTE2D.Loads.ILoad FealiteLoad(MemberLoad load, FrozenDictionary<StructuralCase, FEALiTE2D.Loads.LoadCase> cases, double length) => load.Switch<FEALiTE2D.Loads.ILoad>(
        point:     p => new FEALiTE2D.Loads.FramePointLoad(p.Force.X, p.Force.Y, p.Moment.Z, p.Station * length, FEALiTE2D.Loads.LoadDirection.Global, cases[p.Case]),
        uniform:   u => new FEALiTE2D.Loads.FrameUniformLoad(u.ForcePerLength.X, u.ForcePerLength.Y, FEALiTE2D.Loads.LoadDirection.Global, cases[u.Case]),
        trapezoid: t => new FEALiTE2D.Loads.FrameTrapezoidalLoad(t.Start.X, t.End.X, t.Start.Y, t.End.Y, FEALiTE2D.Loads.LoadDirection.Global, cases[t.Case]));

    static FEALiTE2D.Loads.LoadCombination FealiteCombo(LoadCombinationSpec spec, FrozenDictionary<StructuralCase, FEALiTE2D.Loads.LoadCase> cases) {
        FEALiTE2D.Loads.LoadCombination combo = new() { Label = spec.Label };
        foreach (var (kase, factor) in spec.Factors) { combo.Add(cases[kase], factor); }
        return combo;
    }

    static MemberResponse PlanarResponse(FEALiTE2D.Structure.Structure structure, FEALiTE2D.Elements.FrameElement2D element, FrozenDictionary<StructuralCase, FEALiTE2D.Loads.LoadCase> cases, FrameModel model) =>
        model.Combinations.Fold(MemberResponse.Zero, (env, spec) =>
            structure.Results.GetElementInternalForces(element, FealiteCombo(spec, cases))
                .Fold(env, (e, seg) => toSeq(Enumerable.Range(0, model.Policy.StationCount)).Fold(e, (acc, station) => {
                    double x = seg.x1 + (seg.x2 - seg.x1) * station / Math.Max(model.Policy.StationCount - 1, 1);
                    // The planar moment is the IN-PLANE MAJOR-axis bending -> My (the flexure-major demand selector),
                    // never Mz (minor) which would silently bypass the governing flexure check; Vz/Mz/T are out-of-plane.
                    return acc.Max(new SectionDemand(seg.AxialAt(x), seg.ShearAt(x), 0.0, seg.MomentAt(x), 0.0, 0.0), seg.VerticalDisplacementAt(x));
                })));

    static Fin<FrozenDictionary<NodeId, MemberResponse>> Recover(FrameModel model, Func<StructuralMember, MemberResponse> sample) =>
        Fin.Succ(model.Members.ToFrozenDictionary(static m => m.Id, sample));
}
```

## [04]-[DESIGN_CHECK]

- Owner: `MaterialFamily` the constitutive family; `SafetyFormat` the ASD/LRFD/limit-state axis; `DesignCode` `[SmartEnum<string>]` the standard rows carrying the `MaterialFamily`, the `SafetyFormat`, the resistance/partial factors, and the interaction delegate; `LimitState` `[SmartEnum<string>]` the check rows carrying the demand-component selector and the `Applies(MaterialFamily)` predicate; `CapacityContext` the section+isotropic-strength+optional-orthotropic-stiffness+geometry+code bundle every capacity reads (its `ShearModulusSi` reading the realized seam `Orthotropic.ShearModulus` when the member carries the directional case, the derived isotropic `Mechanical` shear otherwise); the `Capacities` `(DesignCode, LimitState)` frozen table of REAL delegates; `SectionCapacity`/`MemberCheck` the carriers; `StructuralAnalysis.Run` the governing-utilization entry.
- Cases: `DesignCode` rows `aisc360`/`en1993`/`en1992`/`nds`/`en1995`/`aci318`/`tms402`/`aisi-s100` — every structural family carries BOTH its US and its Eurocode row (steel `aisc360`+`en1993`, concrete `aci318`+`en1992`, timber `nds`+`en1995`), so a member is assessable under either jurisdiction through the SAME table, never a US-only or EN-only family; `LimitState` rows `axial-tension`/`axial-compression`/`flexure-major`/`flexure-minor`/`shear-major`/`shear-minor`/`combined`/`deflection` (shear split per axis so the major-axis demand `|Vy|` checks against `AvY` and the minor-axis `|Vz|` against `AvZ`, never one shear area for both) — the capacity is a `(code, state)` cell in the frozen table, each cell the GOVERNING formula for THAT code's material model (AISC E3 `Fcr`, EN 1993 `χ` buckling curve, AISC F2 `Mn` with `Lp`/`Lr` LTB, EN 1993 `χLT`, ACI/EN plain-concrete `Mcr`/`φPn`, NDS `CP`/`CL` adjusted reference values, EN 1995 `k_c`/`k_crit` over the `E0,05` 5%-fractile modulus, TMS slenderness-reduced `Fa`, AISI gross-section bound), the per-cell slenderness/compactness branches the rule count; lateral-torsional buckling is FOLDED into the flexure-major `Mn`/`Mₕ` (one capacity, never a duplicate state); an absent cell is not-applicable (capacity `+∞`, ratio `0`).
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, ClockPolicy clocks)` — `Project` reads the idealization, `Solve` recovers the `MemberResponse` envelope, `Check` folds each member through every applicable `LimitState` computing `utilization = demand / capacity` (the `Combined` arm the code interaction, the `Deflection` arm the FE deflection against `StructuralPolicy.DeflectionLimitRatio × span`), and the governing (max-utilization) member yields the `AssessmentResult` fact stream (`max-utilization`, `governing-member`, `governing-limit-state`, per-check ratios) — the verdict the spine DERIVES from the governing ratio.
- Auto: the column capacity reads the member's `EffectiveLengthFactor × UnbracedLength / RadiusOfGyrationMinor` slenderness (AISC `Fe = π²E/(KL/r)²` → `Fcr`; EN `λ̄ = √(Afy/Ncr)` → `χ`); the flexure-major capacity reads `Lb = UnbracedLength` against `Lp = 1.76·ry·√(E/Fy)` and the elastic LTB moment (EN `Mcr = (π/L)·√(E·Izz·G·J)` → `χLT`); the deflection check reads `MemberResponse.MaxDeflection`; the combined axial+flexure interaction folds the enveloped demand per the `DesignCode.Interaction` delegate (AISC 360 H1.1 and EN 1993-1-1 6.3.3 for steel, the EN 1995-1-1 §6.3.2(3) squared-axial + linear-bending form with the `k_m = 0.7` minor-axis factor for timber, the linear sum for the remaining codes), `Combined` applying to steel/cold-formed/timber.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — `SectionProperties`, `MaterialPropertySet` (the isotropic `Mechanical` AND the realized directional `Orthotropic` case), `NodeId`, `Provenance`, the `graph.PropertiesOf(member).Orthotropic` ergonomic read), NodaTime (`Instant`), BCL inbox (`FrozenDictionary`).
- Growth: a new design code is one `DesignCode` row plus its `(code, state)` cells in the table; a new limit state is one `LimitState` row plus its column of cells; a new material family is one `MaterialFamily` row plus its codes' cells — the check fold re-reads the table, never a new check method per code.
- Boundary: the design codes are HAND-ROLLED (no .NET package owns AISC 360 / EN 1993 / EN 1992 / NDS / EN 1995 / ACI 318 / TMS 402 / AISI S100), realized as a `(DesignCode, LimitState)` data table of capacity delegates — the canonical `POLICY_VALUES`/`DERIVED_LOGIC` collapse, never a switch ladder and never one family's formulas applied to every material (a steel `Wply·fy` charged against a concrete member is the deleted defect this rebuild removes); the timber EN 1995 route is the Eurocode parallel to the US `nds` route the way `en1993` parallels `aisc360` and `en1992` parallels `aci318` — its design strength is `f_k / γ_M` (`γ_M = 1.25`, EN 1995-1-1 Table 2.3) over the SAME seam reference strength the `nds` cells read (the `k_mod` service/duration modifier is applied UPSTREAM by the `Rasm.Materials` `TimberDesign` owner onto the graph-baked reference, never re-derived here), the `§6.3.2` `k_c` column buckling and `§6.3.3` `k_crit` LTB reading the `E0,05` 5%-fractile modulus (`≈0.67·E0,mean`) the seam mean `YoungsModulus` does not carry directly, and the `§6.1.7` `k_cr = 0.67` crack factor on shear; the timber INDEPENDENT shear modulus (`G ≈ E0/16`) the EC5 §6.3.3 LTB reads is the REALIZED seam `MaterialPropertySet.Orthotropic` case (`Composition/material#MATERIAL_PROPERTY`, same `Discipline.Structural`, discriminated by case TYPE) the runner reads off the graph as the optional `graph.PropertiesOf(member).Orthotropic` and threads onto `CapacityContext.ShearModulusSi`, so the LTB `M꜀ᵣ` reads timber's directional `Orthotropic.ShearModulus` when the member carries the directional case and the derived isotropic `Mechanical.ShearModulus` otherwise — the cross-file ripple `Profiles/timber#ORTHOTROPIC_STIFFNESS_LAW` names is CLOSED here, never a deferred isotropic approximation; capacity reads the M7-resolved seam `SectionProperties`, the seam isotropic `Mechanical` strength, and the optional seam `Orthotropic` directional stiffness so a check never re-derives section geometry, re-resolves a profile, or approximates timber's directional shear; the authoritative family is `DesignCode.Family` (the route dictates the material model), the member's `Classify`-derived family validated against it so a steel code on a concrete member rails `AssessmentInputMissing` rather than computing nonsense; reinforced-concrete N-M-M capacity is NOT derivable from the geometric seam section (which carries no rebar) — the concrete cells are the PLAIN-section bound (`Mcr`, `φ·0.85·f'c·Ag`, `Vc`), the reinforced interaction the `Rasm.Materials` `Profiles/capacity#SECTION_CAPACITY` RC owner's concern, so the `VividOrange` `IForceMomentInteraction` surface is NOT composed here; cold-formed AISI capacity is the GROSS-section bound, the effective-width reduction the `Rasm.Materials` `Profiles/steel` `ColdFormedDetail`'s concern; the utilization is `demand/capacity` and the verdict is DERIVED downstream from the governing ratio so a member's pass/fail and its reported ratio share one source; a member whose family no `DesignCode` row serves rails `AssessmentInputMissing`, never a silent skip.

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
    // DesignCode.Family, this is the validation companion and the FEALiTE MaterialType source.
    public static MaterialFamily Classify(MaterialPropertySet.Mechanical m) =>
        m.YoungsModulus.Si > 150e9 ? Steel : m.YoungsModulus.Si > 20e9 ? Concrete : m.YoungsModulus.Si > 5e9 ? Timber : Masonry;

    // Stiffness-band compatibility: Classify resolves a STIFFNESS band, not the true family, so Admits maps a design
    // family to the bands its members can land in. A steel route admits the steel band (Steel and ColdFormedSteel both
    // classify Steel); a masonry route admits ANY non-steel band because masonry's elastic modulus (Em = 700..900*f'm,
    // ~7..18 GPa for typical f'm) overlaps the Timber and Concrete bands and modulus alone cannot separate them — so a
    // standard masonry member that Classify lands as Timber or Concrete is still assessable under TMS 402 rather than
    // spuriously railing the route's own capacity cells; concrete and timber routes require their own resolvable band.
    // The steel-band threshold (150 GPa) is the one boundary modulus reliably draws, so a steel code on a non-steel
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

// The check rows: each carries its demand-component selector and family applicability. Combined/Deflection carry no
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
public readonly record struct CapacityContext(
    SectionProperties Section, MaterialPropertySet.Mechanical Strength, Option<MaterialPropertySet.Orthotropic> Directional,
    MaterialFamily Family, DesignCode Code, double Length, double UnbracedLength, double EffectiveLengthFactor) {
    public static CapacityContext Of(StructuralMember m, DesignCode code) =>
        new(m.Section, m.Strength, m.Directional, m.Family, code, m.Length, m.Length, m.EffectiveLengthFactor);
    public double Slenderness => EffectiveLengthFactor * UnbracedLength / Math.Max(Section.RadiusOfGyrationMinor.Si, StructuralAnalysis.Eps);
    // The §6.3.3 LTB shear-stiffness datum: the realized seam Orthotropic case's INDEPENDENT in-plane G (timber's
    // G ≈ E0/16) when a directional material carries it, the isotropic Mechanical derived G = E/(2(1+ν)) otherwise —
    // so the EC5 lateral-torsional moment reads the directional stiffness off the seam graph, never the ~6× too-stiff
    // isotropic shear for a timber member, while an isotropic member still resolves a finite G.
    public double ShearModulusSi => Directional.Map(static o => o.ShearModulus.Si).IfNone(() => Strength.ShearModulus.Si);
}

// The three interaction operands the DesignCode.Interaction reads — each an axis capacity (or +inf when its cell is
// absent, so the ratio is 0 and the absent action does not constrain the interaction). The axial term uses the
// compression capacity for both senses (conservative: it is the smaller of tension/compression under buckling).
public readonly record struct SectionCapacity(double AxialCompression, double FlexureMajor, double FlexureMinor);

public readonly record struct MemberCheck(NodeId Member, LimitState State, double Demand, double Capacity, double Utilization);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    // The (DesignCode, LimitState) capacity table — the canonical POLICY_VALUES + DERIVED_LOGIC collapse. Each cell is
    // the GOVERNING formula for that code's material model in SI base units (Pa stress, m^2 area, m^3 modulus, m^4
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
            // --- AISI S100 (cold-formed steel, LRFD) — gross-section bound ------------------
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
            (("en1992", "shear-major"),        static c => ConcreteVc(c.Strength.UltimateStrength.Si) * c.Section.AvY.Si / c.Code.GammaM),
            (("en1992", "shear-minor"),        static c => ConcreteVc(c.Strength.UltimateStrength.Si) * c.Section.AvZ.Si / c.Code.GammaM),
            // --- NDS (timber, ASD) — reference values adjusted by CP/CL --------------------
            (("nds", "axial-tension"),         static c => c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("nds", "axial-compression"),     static c => NdsCp(c) * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("nds", "flexure-major"),         static c => NdsCl(c) * c.Strength.YieldStrength.Si * c.Section.Wely.Si),
            // Timber rolling/horizontal shear is axis-independent for a rectangular sawn/glulam section (full-area 2/3·Fv·A), so both axes share the formula.
            (("nds", "shear-major"),           static c => (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            (("nds", "shear-minor"),           static c => (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si),
            // --- EN 1995-1-1 (timber, limit-state, gammaM=1.25) — the EC5 parallel to the NDS rows ---
            // The EN parallel to the US NDS timber route (the way en1993 parallels aisc360 and en1992 parallels aci318):
            // design strength = f_k / gammaM (§2.4.1; the k_mod service/duration modifier is a Rasm.Materials TimberDesign
            // input on the graph-baked reference strength, NOT re-derived here — the seam strength is the already-modified
            // reference stress, mirroring the NDS cells' use of YieldStrength). The seam neutral Mechanical carries the
            // timber reference strength on YieldStrength (the Rasm.Materials projector maps f_c0,k/f_m,k onto it, the same
            // contract the NDS cells read) and the mean E0 on YoungsModulus; the EN 338/14080 5%-fractile stability modulus
            // E0,05 the §6.3.2 buckling needs is ~0.67·E0,mean for softwood (Ec5E005), so the slender-column and LTB checks
            // read a fractile-correct modulus rather than the mean. Tension is the net-section reference; compression is
            // the §6.3.2 k_c-reduced reference (Ylinen-shaped EN buckling curve); flexure-major is the §6.3.3 k_crit-reduced
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
        Capacities.TryGetValue((code.Key, state.Key), out var rule) ? rule(ctx) : double.PositiveInfinity;

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
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, ClockPolicy clocks) =>
        from code   in DesignCode.For(request.Route)
        from model  in Project(graph, request, geometry)
        from _      in Validate(model, code)
        from resp   in Solve(model)
        let checks   = model.Members.Bind(m => Check(m, resp[m.Id], code, model.Policy))
        let govern   = toSeq(checks.OrderByDescending(static c => c.Utilization)).Head
        select AssessmentResult.Of(
            request.Route,
            checks.Map(static c => AssessmentFact.Ratio($"{c.Member.Value}/{c.State.Key}", c.Utilization))
                .Append(Seq(
                    AssessmentFact.Ratio("max-utilization", govern.Map(static g => g.Utilization).IfNone(0.0)),
                    govern.Map(static g => AssessmentFact.Reference("governing-member", g.Member)).IfNone(AssessmentFact.Text("governing-member", "none")),
                    AssessmentFact.Text("governing-limit-state", govern.Map(static g => g.State.Key).IfNone("none")))),
            govern.Map(static g => g.Utilization).IfNone(0.0),
            new Provenance("StructuralAnalysis", request.Route.Standard, "FE + design-code", clocks.Now));

    static Fin<Unit> Validate(FrameModel model, DesignCode code) =>
        model.Members.Find(m => !code.Family.Admits(m.Family))
            .Match(Some: m => Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing($"<material-code-mismatch:{m.Id.Value}:{m.Family.Key}!={code.Family.Key}>")),
                   None: () => Fin.Succ(unit));

    static Seq<MemberCheck> Check(StructuralMember member, MemberResponse response, DesignCode code, StructuralPolicy policy) {
        CapacityContext ctx = CapacityContext.Of(member, code);
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
}
```

## [05]-[RESEARCH]

- [M7_SECTION_RESOLUTION]: the `ProfileRef`→`SectionProperties` resolution (the `VividOrange.Sections.SectionProperties` / `VividOrange.Profiles.Catalogue` lookup) is performed ONCE by the `Rasm.Materials` `MaterialProjector` and BAKED onto the `ProfileSet` composition (`MaterialComposition.WithSection`) as a neutral `SectionProperties` value-object (`Area`/`Iyy`/`Izz`/`J`/`Wely`/`Welz`/`Wply`/`Wplz`/`AvY`/`AvZ`/`RadiusOfGyrationMajor`/`RadiusOfGyrationMinor`/`Depth`/`Width`/`HeatedPerimeter`/`LeastDimension`/`AxisDistance` — the both-axis shear areas `AvY`/`AvZ` (the per-axis `shear-major`/`shear-minor` checks each read their own); `AxisDistance` is the EN 1992-1-2 cover-to-reinforcement the concrete-fire check reads), so the structural consumer reads the seam `graph.SectionOf(member)` Op-FREE accessor and never re-resolves a profile per call (`§4-RT M7`); Compute admits NO VividOrange. The seam `ElementGraph.SectionOf(NodeId)` is now Op-free — it reads the member's directly-associated `ProfileSet` section WITHOUT `Bake` (a section is occurrence-direct, not type-inherited, so no fold is needed), so the runner (which holds no `Op` key) reads it seam-direct and the discipline no longer re-derives a local section accessor — ONE_HOP through the seam owner. Ripple counterpart: `Rasm.Materials` `Profiles/profile` + `Projection/material` (the M7 resolver + the `WithSection` bake) and `Rasm.Element` `Composition/material` (the `SectionProperties` value-object + `ProfileSet`) and `Graph/element` (the Op-free `SectionOf(NodeId)` seam accessor).
- [BFE_CSPARSE_PIN]: BFE 2.1.2 floors `CSparse >= 3.5.0`; the workspace unifies on `CSparse 4.4.0`. BFE's `Common.ISolver` is `{ SparseMatrix A; bool IsInitialized; void Initialize(); void Solve(double[] b, double[] x); }` and `Common.ISolverFactory.CreateSolver(CSparse.Double.SparseMatrix A)` returns an `ISolver` — so `RasmSolverFactory` returns a `CSparse.Double.Factorization.SparseLU`-backed adapter (LU handles the released-DOF indefinite reduced system; `SparseLU.Create(A, ColumnOrdering.MinimumDegreeAtPlusA, 1.0)` + `Solve(double[], double[])`), the SAME CSparse owner the `Tensor/factor#SPARSE_SOLVE` `FactorKind.Lu` row factors through — there is NO `SparseOps.Factory(SparseMatrix)` (the phantom the prior page called; `SparseOps.Factor` consumes a CSR `SparseCompressedRowMatrixStorage`, a format the BFE `SparseMatrix` is not). BFE's embedded dense `DenseLU : ISolver<double>` was compiled against the 3.5-era one-method `ISolver<T>` and fails interface mapping under 4.x, and the `DenseColumnMajorStorage` `Solve`/`Determinant` path sits ONLY on the continuum `TriangleElement`/`TetrahedronElement`/`HexahedralElement` Jacobian integration — so confining the runner to the sparse-factored `BarElement` frame solve means `DenseLU` is never loaded. Ripple counterpart: `Tensor/factor#SPARSE_SOLVE` (the `CSparse` `SparseLU` owner shared with the continuum lane).
- [HAND_ROLLED_DESIGN_CODES]: no .NET package owns AISC 360 / EN 1993-1-1 / EN 1992-1-1 / NDS / EN 1995-1-1 / ACI 318 / TMS 402 / AISI S100, so the codes are a `(DesignCode, LimitState)` frozen table of delegate-backed cells, each cell the GOVERNING formula for that code's material model: steel/cold-formed REAL slenderness-aware capacity (AISC E3 `Fcr` over `KL/r`, AISC F2 `Mn` with `Lp`/`Lr` LTB, EN 1993 `χ`/`χLT` buckling curves over the warping-free `Mcr = (π/L)·√(E·Izz·G·J)`), concrete the PLAIN-section bound (`Mcr = fr·Wel`, `φ·0.85·f'c·Ag`, `Vc = 0.17√f'c·Av`) because the geometric seam section carries no rebar, timber BOTH the US `nds` (Ylinen `CP` column-stability and `CL` beam-stability over the reference values) AND the Eurocode `en1995` (the parallel `Rasm.Materials` `Profiles/timber#EC5_DESIGN_CAPACITY` `TimberDesign.Capacity` names Compute as the reader: `f_k/γ_M` at `γ_M = 1.25`, the §6.3.2 `k_c` column buckling and §6.3.3 `k_crit` LTB over the `E0,05 ≈ 0.67·E0,mean` 5%-fractile modulus, the §6.1.7 `k_cr = 0.67` shear-crack factor, the §6.3.2(3) squared-axial + linear-bending interaction with the `k_m = 0.7` minor-axis factor), masonry the TMS slenderness-reduced `Fa`, AISI the gross-section bound — every structural family thus carrying BOTH its US and its Eurocode route. The §6.3.3 LTB shear-stiffness datum is the second half of the timber contract: `Profiles/timber#ORTHOTROPIC_STIFFNESS_LAW` names Compute as the reader of timber's INDEPENDENT directional shear (`G ≈ E0/16`, ~6× softer than the isotropic `E/(2(1+ν))`), so the runner reads the REALIZED seam `MaterialPropertySet.Orthotropic` case (`Composition/material#MATERIAL_PROPERTY`, `props.Orthotropic`, `Discipline.Structural` discriminated by case TYPE) as the optional `CapacityContext.Directional` and the `Ec5Kcrit` `M꜀ᵣ` reads `CapacityContext.ShearModulusSi` (the directional `Orthotropic.ShearModulus` when the member carries the case, the isotropic `Mechanical` derived `G` otherwise) — the directional stiffness off the seam graph, never an isotropic approximation, the cross-file ripple now CLOSED both sides. The safety format (ASD/LRFD/limit-state) and partial/resistance factors are `DesignCode` columns; the combined-action interaction is the `DesignCode.Interaction` delegate (AISC H1.1, EN 1993 6.3.3). This is the `POLICY_VALUES`+`DERIVED_LOGIC` collapse over an imperative per-code arm — and is HONEST: it never charges one family's formula against another (the prior page's steel `Wply·fy` applied to every family is the deleted defect). The reinforced-concrete N-M-M interaction is the `Rasm.Materials` `Profiles/capacity#SECTION_CAPACITY` `RcInteraction` owner's concern (the seam section has no rebar), the cold-formed effective-section the `Profiles/steel` `ColdFormedDetail`'s — neither composed here. Ripple counterpart: `Rasm.Materials` `Profiles/timber#EC5_DESIGN_CAPACITY` + `#ORTHOTROPIC_STIFFNESS_LAW` (the `TimberDesign.Capacity` resistances and the `TimberSection.OrthotropicLaw` directional stiffness this runner reads) and `Rasm.Element` `Composition/material#MATERIAL_PROPERTY` (the realized `MaterialPropertySet.Orthotropic` case + `props.Orthotropic` read).
- [FE_BACKEND_DUALITY]: the 3D `BriefFiniteElement.Net` and 2D-planar `FEALiTE2D` are the two `Solve` arms, both factoring through the ONE shared CSparse owner; `SolverBackend.Auto` routes a coplanar in-plane frame to the lighter 2D solver and a general 3D frame to BFE. Both lanes map EVERY `MemberLoad` kind through a TOTAL `Switch` (BFE: `ConcentratedLoad`/`UniformLoad`/`PartialNonUniformLoad` with a linear `SingleVariablePolynomial` severity; FEALiTE: `FramePointLoad`/`FrameUniformLoad`/`FrameTrapezoidalLoad`) — the prior page dropped point and trapezoidal loads in the planar lane and flattened a trapezoid to a uniform average in both, the deleted defects. Two boundary-fidelity invariants the load map MUST preserve: `SingleVariablePolynomial(params double[])` evaluates DESCENDING powers (`coefs[0]·xⁿ⁻¹ + … + coefs[last]·x⁰`), so the trapezoid severity is `(slope, mid)` not `(mid, slope)` — the swapped order yields `-s` at the start node; and `FramePointLoad`'s fourth argument is the ABSOLUTE distance from the start node (divided by length internally), so the `[0,1]` `Station` fraction scales by the member length, while BFE's `IsoPoint` takes the dimensionless `[-1,1]` iso-coordinate (`2·Station−1`). The BFE local frame orders `(Mx=torsion, My/Mz=bending)`, so the demand maps `SectionDemand(N=Fx, Vy=Fy, Vz=Fz, My=My, Mz=Mz, T=Mx)` (the prior page's `(…, Mx, My, Mz)` charged torsion as `My` and bending as `T`). Deflection is recovered from `BarElement.GetInternalDisplacementAt`/`LinearMeshSegment.VerticalDisplacementAt`, so the `Deflection` limit state is a REAL FE-displacement check, never a `0.0` sentinel. The continuum multi-physics solve stays the `Solver/contract#SOLVE_CONTRACT` `SolveLane`; the `FEALiTE2D.Plotting` DXF leg is an AppUi/export concern.
- [GRAPH_READ_ACCESSORS]: the runner reads the concrete graph through two accessor tiers — the SEAM-owned material/section/mechanical reads (`graph.MaterialsOf`/`graph.MechanicalOf`/`graph.SectionOf`, all Op-free) and the COMPUTE-owned `StructuralReads` discipline reads (`AxisOf` resolving the analytical `AxisCurve` ONE-HOP by content key through the seam `GeometrySource` port off `member.Representations.Axis` — the Object node carries NO inline `Axis` coordinate, the deleted §4-RT-M2 phantom; `SupportsOf`/`LoadsOf` traversing the projected `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` neutral `Generic` edges by wire-name and reading the 6-DOF restraint + end discriminant + applied components + load kind off the edge payload), the same `EnergyGraphReads` shape `Analysis/energy` takes; `AxisCurve` is the seam-DECLARED analytical-geometry type (`Rasm.Element/Graph/element`) the runner reasons over (the prior page's `MemberAxis` mirrored it field-for-field — the deleted parallel shape — `Coplanar` now a `StructuralReads` `AxisCurve` fold, length the member's own `Vector3.Distance`), the FE assembler merging shared joints by tolerance-quantized coordinate and placing each support on its `AtStart`-resolved endpoint (the prior page's `s.At == member.Id` test was always false, sending every support to the end). Ripple counterpart: `Rasm.Element` `Graph/element` (the seam-declared `AxisCurve`/`FootprintPolygon` analytical-geometry types + the `GeometrySource` resolution port + the `RepresentationContentHash.Axis` content key + `MaterialsOf`/`MechanicalOf` reads), `Rasm.Compute` `Analysis/assessment` (the spine threading `GeometrySource` through `Assess`/`Run` to this runner), and `Rasm.Bim` `Projection/semantic` (the `EdgeProjection.Structural` fold that stamps the restraint booleans, the `AtStart` end discriminant, the `LoadKind`/`Case`/component attributes, and `IfcRepresentation.Keys` content-keying the analytical axis into `Representations` these reads resolve).
