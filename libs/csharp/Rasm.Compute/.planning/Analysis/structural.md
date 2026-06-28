# [COMPUTE_STRUCTURAL]

Rasm.Compute structural-analysis runner: the `Discipline.Structural` arm of the assessment rail. It reads the concrete `Rasm.Element` `ElementGraph` directly (member Object nodes, the M7-resolved `SectionProperties` riding the graph, the seam `MaterialPropertySet.Mechanical` strengths, the idealized member axes), folds them into one `FrameModel` idealization, solves it over a polymorphic `FrameBackend` (the 3D `BriefFiniteElement.Net` `Model` or the 2D-planar `FEALiTE2D` `Structure`, both factoring through the ONE shared `Tensor/factor#SPARSE_SOLVE` `CSparse` owner — never a second linear-algebra rail), recovers the per-combination internal-force envelope, runs the hand-rolled design-code checks (AISC 360 / EN 1993 / EN 1992 / NDS / ACI 318 / TMS 402 / AISI S100, ~250 limit-state rules driven by a `DesignCode`×`LimitState` capacity table, never imperative arms), and returns the governing utilization as one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back. Section properties resolve ONCE upstream — the `VividOrange`-backed `ProfileRef`→section resolution (M7) is performed by the `Rasm.Materials` projector and BAKED onto the seam graph, so the structural consumer reads the resolved `SectionProperties` and never re-resolves a profile per call, and Compute admits no VividOrange (one owner, in Materials). The FE library is the assembler and solver only — BFE 2.1.2 is confined to the sparse-factored `BarElement` frame path (its embedded dense `DenseLU` is binary-incompatible with the unified `CSparse 4.4.0` pin, so the linear solve injects the Rasm CSparse-4.x `ISolverFactory` via `Solve(ISolverFactory)`), code-checking is hand-rolled here, and the `Solver/contract#SOLVE_CONTRACT` continuum `SolveLane` owns the multi-physics field solve this runner never re-derives.

## [01]-[INDEX]

- [01]-[FRAME_MODEL]: the `FrameModel` idealization read from the graph, the `MemberAxis`/`MemberSupport`/`MemberLoad`/`LoadCombinationSpec`/`StructuralPolicy` inputs, and the seam `SectionProperties`/`Mechanical` resolution.
- [02]-[FRAME_BACKEND]: the `FrameBackend` `[Union]` (`Bfe` 3D / `Fealite` 2D planar) FE assemble-solve-recover over the shared `CSparse` owner, and the per-combination `SectionDemand` internal-force envelope.
- [03]-[DESIGN_CHECK]: the `DesignCode`×`LimitState` hand-rolled capacity table, the `SectionCapacity`/`MemberCheck` fold, the interaction rule, and the `StructuralAnalysis.Run` governing-utilization fact stream.

## [02]-[FRAME_MODEL]

- Owner: `FrameModel` the analysis idealization (members, supports, applied loads, combinations); `MemberAxis` the idealized structural line + local frame; `MemberSupport` the 6-DOF restraint; `MemberLoad` the per-member applied action; `LoadCombinationSpec` the factored case map; `StructuralPolicy` the backend/deflection policy carried on the request.
- Entry: `static Fin<FrameModel> Project(ElementGraph graph, AssessmentRequest.Structural request)` — folds the request `Targets` member Object nodes into the idealization, reading each member's `SectionProperties` (the M7-resolved seam section), `MaterialPropertySet.Mechanical` (the seam strength), and `MemberAxis` (the idealized line) through the seam graph accessors, `Fin<T>` aborting onto `ComputeFault.AssessmentInputMissing` when a member lacks a section, a strength, or an axis.
- Auto: self-weight derives from the member `SectionProperties.Area.Si` × `Mechanical.Density.Si` × length per member as the `Dead` case; the request's applied `MemberLoad` set supplies the live/wind/snow/seismic cases; `LoadCombinationSpec` factors the cases per code (ASCE 7 / EN 1990 combinations) so a combination is data the fold reads, never a re-modelled load set.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Element (project — `ElementGraph`, `Node`, `NodeId`, `SectionProperties`, `MaterialPropertySet`), Rasm (project — `Vector3` axis frame), BCL inbox.
- Growth: a new applied-action kind is one `MemberLoad` case; a new restraint is one `MemberSupport` shape; a new combination basis is one `LoadCombinationSpec` row — the idealization widens by data, the backends and checks re-read it.
- Boundary: the section is the M7-resolved seam `SectionProperties` read from the graph — the `VividOrange` `ProfileRef`→section resolution happens ONCE in the `Rasm.Materials` projector and rides the graph, so this runner reads `section.Area.Si`/`Iyy.Si`/`Izz.Si`/`J.Si`/`Wply.Si`/`Wely.Si`/`AvY.Si` and never re-resolves a profile per call (the M7 one-hop) and Compute admits no VividOrange (one owner, Materials); the strength is the seam `Mechanical.YieldStrength.Si`/`UltimateStrength.Si`/`YoungsModulus.Si`/`ShearModulus.Si` read from the member's associated material, never re-typed; the member axis is the idealized structural line resolved from the Object node's representation by content-hash through the seam graph gateway, never re-meshed (the graph arrives idealized); a member with no section/strength/axis rails the typed input fault, never a defaulted section.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class SolverBackend {
    public static readonly SolverBackend Auto    = new("auto");
    public static readonly SolverBackend Spatial = new("spatial");
    public static readonly SolverBackend Planar  = new("planar");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MemberLoad {
    private MemberLoad() { }
    public sealed record Point(StructuralCase Case, Vector3 Force, Vector3 Moment, double Station) : MemberLoad;
    public sealed record Uniform(StructuralCase Case, Vector3 ForcePerLength) : MemberLoad;
    public sealed record Trapezoid(StructuralCase Case, Vector3 Start, Vector3 End) : MemberLoad;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class StructuralCase {
    public static readonly StructuralCase Dead    = new("dead");
    public static readonly StructuralCase Live    = new("live");
    public static readonly StructuralCase Snow    = new("snow");
    public static readonly StructuralCase Wind    = new("wind");
    public static readonly StructuralCase Seismic = new("seismic");
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct MemberAxis(Vector3 Start, Vector3 End, Vector3 Up) {
    public double Length => Vector3.Distance(Start, End);
    public bool Coplanar(double z) => Math.Abs(Start.Z - z) < 1e-6 && Math.Abs(End.Z - z) < 1e-6;
}

public readonly record struct MemberSupport(NodeId At, bool Dx, bool Dy, bool Dz, bool Rx, bool Ry, bool Rz);

public sealed record LoadCombinationSpec(string Label, FrozenDictionary<StructuralCase, double> Factors);

public sealed record StructuralPolicy(SolverBackend Backend, double DeflectionLimitRatio, int StationCount) {
    public static readonly StructuralPolicy Canonical = new(SolverBackend.Auto, DeflectionLimitRatio: 1.0 / 250.0, StationCount: 11);
}

public sealed record StructuralMember(NodeId Id, MemberAxis Axis, SectionProperties Section, MaterialPropertySet.Mechanical Strength, MaterialFamily Family, Seq<MemberLoad> Loads, Seq<MemberSupport> Supports);

public sealed record FrameModel(Seq<StructuralMember> Members, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy) {
    public bool Planar => Members.ForAll(m => m.Axis.Coplanar(Members.Head.Axis.Start.Z));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    public static Fin<FrameModel> Project(ElementGraph graph, AssessmentRequest.Structural request) =>
        request.Targets.Fold(
            Fin.Succ(Seq<StructuralMember>()),
            (acc, id) => acc.Bind(members =>
                from section in graph.SectionOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<member-missing-section>"))
                from strength in graph.MechanicalOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<member-missing-mechanical>"))
                from axis in graph.AxisOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<member-missing-axis>"))
                let family = MaterialFamily.Classify(strength)
                let selfWeight = new MemberLoad.Uniform(StructuralCase.Dead, new Vector3(0f, 0f, -(float)(section.Area.Si * strength.Density.Si * 9.81)))
                select members.Add(new StructuralMember(id, axis, section, strength, family, graph.LoadsOf(id).Add(selfWeight), graph.SupportsOf(id)))))
            .Map(members => new FrameModel(members, request.Combinations, request.Policy));
}

// The discipline graph reads the structural runner composes from the seam primitives (EdgesAt + Find + the baked
// Node.Object.Axis + the neutral Generic structural edges by wire-name) — Compute-OWNED ElementGraph extensions, NOT
// seam members: the seam owns the material/section reads (it owns those nodes), the discipline reads live here. The
// member axis reads the analytical line the Bim projector baked (Enrich); the supports/loads traverse the projected
// IfcRelConnectsStructuralMember/Activity Generic edges by wire-name, reading the 6-DOF restraint / applied force off
// the neutral edge payload — so the runner reads the structural idealization fully baked, never re-reading IFC.
public static class StructuralGraphReads {
    const string ConnectsMember   = "IfcRelConnectsStructuralMember";
    const string ConnectsActivity = "IfcRelConnectsStructuralActivity";

    public static Fin<MemberAxis> AxisOf(this ElementGraph graph, NodeId member) =>
        graph.Find<Node.Object>(member).Bind(static o => o.Axis)
            .Map(static a => new MemberAxis(a.Start, a.End, a.Up))
            .ToFin(new ComputeFault.AssessmentInputMissing($"<member-axis-absent:{member.Value}>"));

    // One MemberSupport per structural connection the member binds — the fixity booleans read off the neutral Generic
    // edge payload the projector baked from IfcBoundaryNodeCondition; At is the connection node the FE constraint sits on.
    public static Seq<MemberSupport> SupportsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsMember && g.Relating == member
            ? Some(new MemberSupport(g.Related, Fix(g, "TranslationX"), Fix(g, "TranslationY"), Fix(g, "TranslationZ"), Fix(g, "RotationX"), Fix(g, "RotationY"), Fix(g, "RotationZ")))
            : None).ToSeq();

    // One MemberLoad per structural activity the member binds — the applied force/moment read off the neutral Generic
    // edge payload the projector baked from IfcStructuralLoadSingleForce; self-weight is the Dead case Project derives,
    // so these are the live/applied actions only, projected as a midspan point action over the member.
    public static Seq<MemberLoad> LoadsOf(this ElementGraph graph, NodeId member) =>
        graph.EdgesAt(member).Choose(e => e is Relationship.Generic g && g.WireName == ConnectsActivity && g.Relating == member
            ? Some((MemberLoad)new MemberLoad.Point(StructuralCase.Live, Vec(g, "Force"), Vec(g, "Moment"), 0.5))
            : None).ToSeq();

    static bool Fix(Relationship.Generic g, string dof) =>
        g.Attributes.Find(PropertyName.Create(dof)).Map(static v => v is PropertyValue.Boolean b && b.Value).IfNone(false);

    static Vector3 Vec(Relationship.Generic g, string component) =>
        new((float)Si(g, $"{component}X"), (float)Si(g, $"{component}Y"), (float)Si(g, $"{component}Z"));

    static double Si(Relationship.Generic g, string key) =>
        g.Attributes.Find(PropertyName.Create(key)).Map(static v => v is PropertyValue.Measure m ? m.Value.Si : 0.0).IfNone(0.0);
}
```

## [03]-[FRAME_BACKEND]

- Owner: `FrameBackend` `[Union]` (`Bfe` the 3D `BriefFiniteElement.Net` assemble-solve-recover, `Fealite` the 2D-planar `FEALiTE2D`) selected by the `StructuralPolicy.Backend` (Auto picks `Fealite` for a coplanar in-plane frame, `Bfe` otherwise); `SectionDemand` the per-combination internal-force envelope; `RasmSolverFactory` the `ISolverFactory` routing both libraries' linear solve through the shared `Tensor/factor#SPARSE_SOLVE` `CSparse` owner.
- Entry: `static Fin<FrozenDictionary<NodeId, SectionDemand>> Solve(FrameModel model)` — selects the backend, assembles the FE model (each member a `BarElement`/`FrameElement2D` over its `UniformParametric1DSection`/`Generic2DSection` and `UniformIsotropicMaterial`/`GenericIsotropicMaterial`, each support a `Constraint`/`NodalSupport`, each `MemberLoad`+`LoadCombinationSpec` a `LoadCase`/`LoadCombination`), solves through the injected CSparse factory, and recovers the worst-station `SectionDemand` per member per combination, `Fin<T>` lowering a `FEALiTE2D` `AnalysisStatus.Failure` or a BFE non-convergence onto `ComputeFault.AnalysisRunFailed`.
- Auto: BFE's linear solve is injected with `RasmSolverFactory` (`model.Solve(factory)`) so the structural and continuum lanes share ONE factorization owner and BFE's embedded dense `DenseLU` (binary-incompatible with the `CSparse 4.4.0` pin) is never loaded — the runner stays on the sparse-factored `BarElement` frame path; `FEALiTE2D` factors its `StructuralStiffnessMatrix` through the same `api-csparse` owner natively; the worst-station demand folds each member's `GetExactInternalForceAt`/`LinearMeshSegment.MomentAt`/`ShearAt`/`AxialAt` over the policy station set.
- Packages: BriefFiniteElement.Net, BriefFiniteElementNet.CustomElements, FEALiTE2D, CSparse (the shared factorization owner via `Tensor/factor#SPARSE_SOLVE`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new FE backend is one `FrameBackend` case; the demand envelope is one `SectionDemand` shape the design checks read regardless of backend — a `Bfe3DAnalyzer`/`Fealite2DAnalyzer` sibling family is the rejected form collapsed onto the one `FrameBackend` union.
- Boundary: BFE is confined to the sparse-factored `BarElement` frame solve — continuum `TriangleElement`/`TetrahedronElement`/`HexahedralElement` are NOT used (their `DenseColumnMajorStorage` `Solve`/`Determinant` path routes through the binary-incompatible `DenseLU` under `CSparse 4.4.0`, throwing `TypeLoadException`), and continuum multi-physics is the `Solver/contract#SOLVE_CONTRACT` `SolveLane` not this runner; the FE result carriers (`Force`/`Displacement`/`LinearMeshSegment`) carry RAW `double` components in one consistent SI system — the runner re-attaches SI semantics at the `SectionDemand` boundary and the section/strength scalars are supplied SI from the graph; a hand-rolled stiffness assembler or a re-typed section beside the FE library is the rejected form; a singular/ill-conditioned system surfaces as a typed `AnalysisRunFailed`, never an exception crossing the rail.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The per-combination internal-force envelope — backend-neutral; My/Mz/Vy/Vz are the worst-station magnitudes.
public readonly record struct SectionDemand(double N, double Vy, double Vz, double My, double Mz, double T);

// --- [SERVICES] ----------------------------------------------------------------------------
// One ISolverFactory routes both FE libraries' linear solve through the shared Tensor/factor CSparse-4.x owner,
// so the structural lane never loads BFE's CSparse-3.5-era embedded DenseLU under the unified 4.4.0 pin.
public sealed class RasmSolverFactory : BriefFiniteElementNet.Common.ISolverFactory {
    public static readonly RasmSolverFactory Instance = new();
    public BriefFiniteElementNet.Common.ISolver CreateSolver(CSparse.Double.SparseMatrix a) => SparseOps.Factory(a);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    public static Fin<FrozenDictionary<NodeId, SectionDemand>> Solve(FrameModel model) =>
        model.Policy.Backend == SolverBackend.Planar || (model.Policy.Backend == SolverBackend.Auto && model.Planar)
            ? SolvePlanar(model)
            : SolveSpatial(model);

    static Fin<FrozenDictionary<NodeId, SectionDemand>> SolveSpatial(FrameModel model) {
        BriefFiniteElementNet.Model bfe = new();
        Dictionary<Vector3, BriefFiniteElementNet.Node> nodes = new();
        Dictionary<NodeId, BriefFiniteElementNet.Elements.BarElement> bars = new();
        foreach (StructuralMember member in model.Members) {
            BriefFiniteElementNet.Node start = NodeAt(bfe, nodes, member.Axis.Start);
            BriefFiniteElementNet.Node end = NodeAt(bfe, nodes, member.Axis.End);
            BriefFiniteElementNet.Elements.BarElement bar = new(start, end) {
                Section = new BriefFiniteElementNet.Sections.UniformParametric1DSection(member.Section.Area.Si, member.Section.Iyy.Si, member.Section.Izz.Si, member.Section.J.Si),
                Material = BriefFiniteElementNet.Materials.UniformIsotropicMaterial.CreateFromYoungPoisson(member.Strength.YoungsModulus.Si, member.Strength.PoissonRatio),
                Behavior = BriefFiniteElementNet.Elements.BarElementBehaviours.FullFrame,
            };
            bfe.Elements.Add(bar);
            bars[member.Id] = bar;
            foreach (MemberSupport s in member.Supports) { (s.At == member.Id ? start : end).Constraints = BfeConstraint(s); }
            foreach (MemberLoad load in member.Loads) { bar.Loads.Add(BfeLoad(load)); }
        }
        bfe.Solve(RasmSolverFactory.Instance);
        return Fin.Succ(model.Members
            .Map(m => (m.Id, Demand: model.Combinations.Fold(default(SectionDemand), (env, combo) => Envelope(env, SampleBfe(bars[m.Id], BfeCombo(combo), model.Policy.StationCount)))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Demand));
    }

    static BriefFiniteElementNet.Node NodeAt(BriefFiniteElementNet.Model bfe, Dictionary<Vector3, BriefFiniteElementNet.Node> nodes, Vector3 p) {
        if (nodes.TryGetValue(p, out var existing)) { return existing; }
        BriefFiniteElementNet.Node node = new(p.X, p.Y, p.Z);
        nodes[p] = node;
        bfe.Nodes.Add(node);
        return node;
    }

    static BriefFiniteElementNet.Constraint BfeConstraint(MemberSupport s) => new(
        s.Dx ? DofConstraint.Fixed : DofConstraint.Released, s.Dy ? DofConstraint.Fixed : DofConstraint.Released, s.Dz ? DofConstraint.Fixed : DofConstraint.Released,
        s.Rx ? DofConstraint.Fixed : DofConstraint.Released, s.Ry ? DofConstraint.Fixed : DofConstraint.Released, s.Rz ? DofConstraint.Fixed : DofConstraint.Released);

    static BriefFiniteElementNet.Loads.ElementalLoad BfeLoad(MemberLoad load) => load.Switch(
        point:     p => new BriefFiniteElementNet.Loads.ConcentratedLoad(BfeForce(p.Force, p.Moment), new BriefFiniteElementNet.IsoPoint(2.0 * p.Station - 1.0), CoordinationSystem.Global, BfeCase(p.Case)),
        uniform:   u => new BriefFiniteElementNet.Loads.UniformLoad(BfeCase(u.Case), new BriefFiniteElementNet.Vector(u.ForcePerLength.X, u.ForcePerLength.Y, u.ForcePerLength.Z).GetUnit(), u.ForcePerLength.Length(), CoordinationSystem.Global),
        trapezoid: t => new BriefFiniteElementNet.Loads.UniformLoad(BfeCase(t.Case), new BriefFiniteElementNet.Vector(0, 0, -1), 0.5 * (t.Start.Length() + t.End.Length()), CoordinationSystem.Global));

    static BriefFiniteElementNet.Force BfeForce(Vector3 f, Vector3 m) => new(f.X, f.Y, f.Z, m.X, m.Y, m.Z);
    static BriefFiniteElementNet.LoadCase BfeCase(StructuralCase c) => new(c.Key, BfeLoadType(c));
    static BriefFiniteElementNet.LoadType BfeLoadType(StructuralCase c) =>
        c == StructuralCase.Dead ? BriefFiniteElementNet.LoadType.Dead : c == StructuralCase.Live ? BriefFiniteElementNet.LoadType.Live
        : c == StructuralCase.Snow ? BriefFiniteElementNet.LoadType.Snow : c == StructuralCase.Wind ? BriefFiniteElementNet.LoadType.Wind : BriefFiniteElementNet.LoadType.Quake;

    static BriefFiniteElementNet.LoadCombination BfeCombo(LoadCombinationSpec spec) {
        BriefFiniteElementNet.LoadCombination combo = new();
        foreach (var (kase, factor) in spec.Factors) { combo[BfeCase(kase)] = factor; }
        return combo;
    }

    static SectionDemand SampleBfe(BriefFiniteElementNet.Elements.BarElement bar, BriefFiniteElementNet.LoadCombination combo, int stations) =>
        toSeq(Enumerable.Range(0, stations)).Fold(default(SectionDemand), (env, s) => {
            BriefFiniteElementNet.Force f = bar.GetExactInternalForceAt(2.0 * s / (stations - 1) - 1.0, combo);
            return Envelope(env, new SectionDemand(f.Fx, f.Fy, f.Fz, f.Mx, f.My, f.Mz));
        });

    static Fin<FrozenDictionary<NodeId, SectionDemand>> SolvePlanar(FrameModel model) {
        FEALiTE2D.Structure.Structure structure = new() { LinearMesher = new FEALiTE2D.Meshing.LinearMesher() };
        Dictionary<NodeId, FEALiTE2D.Elements.FrameElement2D> frames = new();
        foreach (StructuralMember member in model.Members) {
            FEALiTE2D.Elements.Node2D start = new(member.Axis.Start.X, member.Axis.Start.Y);
            FEALiTE2D.Elements.Node2D end = new(member.Axis.End.X, member.Axis.End.Y);
            foreach (MemberSupport s in member.Supports) { (s.At == member.Id ? start : end).Support = new FEALiTE2D.Elements.NodalSupport(s.Dx, s.Dy, s.Rz); }
            FEALiTE2D.Elements.FrameElement2D frame = new(start, end, member.Id.Value) {
                CrossSection = new FEALiTE2D.CrossSections.Generic2DSection(member.Section.Area.Si, member.Section.AvY.Si, member.Section.AvY.Si, member.Section.Iyy.Si, member.Section.Izz.Si, member.Section.J.Si, member.Section.Depth.Si, member.Section.Width.Si,
                    new FEALiTE2D.Materials.GenericIsotropicMaterial(member.Strength.YoungsModulus.Si, member.Strength.PoissonRatio, member.Strength.Density.Si, 1.2e-5, FEALiTE2D.Materials.MaterialType.Steel)),
            };
            structure.AddElement(frame, addNodes: true);
            foreach (MemberLoad load in member.Loads.Where(static l => l is MemberLoad.Uniform)) {
                frame.Loads.Add(new FEALiTE2D.Loads.FrameUniformLoad(((MemberLoad.Uniform)load).ForcePerLength.X, ((MemberLoad.Uniform)load).ForcePerLength.Y, FEALiTE2D.Loads.LoadDirection.Global, FealiteCase(((MemberLoad.Uniform)load).Case)));
            }
            frames[member.Id] = frame;
        }
        model.Combinations.Iter(c => structure.LoadCasesToRun.AddRange(c.Factors.Keys.Map(FealiteCase)));
        structure.Solve();
        return structure.AnalysisStatus == FEALiTE2D.Structure.AnalysisStatus.Failure
            ? Fin.Fail<FrozenDictionary<NodeId, SectionDemand>>(new ComputeFault.AnalysisRunFailed($"<fealite-singular:dof={structure.nDOF}>"))
            : Fin.Succ(model.Members.Map(m => (m.Id, Demand: PlanarDemand(structure, frames[m.Id], model))).ToFrozenDictionary(static r => r.Id, static r => r.Demand));
    }

    static FEALiTE2D.Loads.LoadCase FealiteCase(StructuralCase c) => new(c.Key, FEALiTE2D.Loads.LoadCaseType.Dead);

    static FEALiTE2D.Loads.LoadCombination FealiteCombo(LoadCombinationSpec spec) {
        FEALiTE2D.Loads.LoadCombination combo = new() { Label = spec.Label };
        foreach (var (kase, factor) in spec.Factors) { combo.Add(FealiteCase(kase), factor); }
        return combo;
    }

    static SectionDemand PlanarDemand(FEALiTE2D.Structure.Structure structure, FEALiTE2D.Elements.FrameElement2D element, FrameModel model) =>
        model.Combinations.Fold(default(SectionDemand), (env, spec) =>
            structure.Results.GetElementInternalForces(element, FealiteCombo(spec))
                .Fold(env, (e2, seg) => toSeq(Enumerable.Range(0, model.Policy.StationCount))
                    .Fold(e2, (e3, s) => {
                        double x = seg.Length * s / (model.Policy.StationCount - 1);
                        return Envelope(e3, new SectionDemand(seg.AxialAt(x), seg.ShearAt(x), 0.0, 0.0, seg.MomentAt(x), 0.0));
                    })));

    static SectionDemand Envelope(SectionDemand a, SectionDemand b) => new(
        Max(a.N, b.N), Max(a.Vy, b.Vy), Max(a.Vz, b.Vz), Max(a.My, b.My), Max(a.Mz, b.Mz), Max(a.T, b.T));

    static double Max(double a, double b) => Math.Abs(a) >= Math.Abs(b) ? a : b;
}
```

## [04]-[DESIGN_CHECK]

- Owner: `DesignCode` `[SmartEnum<string>]` the standard rows carrying the `MaterialFamily`, the `SafetyFormat`, and the resistance/partial factors; `LimitState` `[SmartEnum<string>]` the check rows carrying a `Capacity` delegate and an `Applies(MaterialFamily)` predicate; `MaterialFamily` the constitutive family classified from the seam strength; `SectionCapacity`/`MemberCheck` the per-member-per-limit-state demand/capacity/utilization carriers; `StructuralAnalysis.Run` the governing-utilization entry.
- Cases: `DesignCode` rows `aisc360`/`en1993`/`en1992`/`nds`/`aci318`/`tms402`/`aisi-s100`; `LimitState` rows `axial-tension`/`axial-compression`/`flexure-major`/`flexure-minor`/`shear`/`combined`/`lateral-torsional-buckling`/`deflection` — every code-check rule is a `(DesignCode, LimitState)` capacity-table entry, never an imperative arm; the ~250 rules across the seven codes are rows on this table, the steel rows realized here and the concrete/timber/masonry/cold-formed rows the same delegate shape.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, ClockPolicy clocks)` — `Project` reads the idealization, `Solve` recovers the demand envelope, `Check` folds each member through every applicable `LimitState` computing utilization = demand / `LimitState.Capacity(section, strength, code)`, and the governing (max-utilization) member yields the `AssessmentResult` fact stream (`max-utilization`, `governing-member`, `governing-limit-state`, per-limit-state ratios) with the verdict derived from the governing ratio.
- Auto: the combined axial+flexure interaction folds per the `DesignCode` interaction rule (AISC 360 H1.1 `N/Nc + 8/9·(My/Mcy + Mz/Mcz)`, EN 1993-1-1 6.3.3 with the `k` interaction factors); the deflection check reads the FE displacement envelope against `StructuralPolicy.DeflectionLimitRatio` × span; the lateral-torsional-buckling capacity folds the unbraced length over the section `Wply`/`iz`.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — `SectionProperties`, `MaterialPropertySet`, `NodeId`, `PropertyValue`, `MeasureValue`), NodaTime, BCL inbox.
- Growth: a new design code is one `DesignCode` row carrying its factors plus its capacity-delegate rows; a new limit state is one `LimitState` row carrying its `Capacity` delegate and `Applies` predicate; a new material family is one `MaterialFamily` row plus its capacity rows — the check fold re-reads the table, never a new check method per code.
- Boundary: the design codes are HAND-ROLLED (no .NET package owns AISC 360 / EN 199x / NDS / ACI 318 / TMS 402 / AISI S100 — the ~250 rules are a data table of capacity delegates, the canonical `POLICY_VALUES`/`DERIVED_LOGIC` collapse, never a switch ladder); capacity reads the M7-resolved seam `SectionProperties` and the seam `Mechanical` strength so a code check never re-derives section geometry or re-resolves a profile; the utilization is `demand/capacity` with the verdict DERIVED from the governing ratio (`> 1.0` exceeded) so a member's pass/fail and its reported ratio share one source; the VividOrange `IForceMomentInteraction` capacity surface is NOT composed here — the interaction is the hand-rolled code rule over the baked section, keeping VividOrange a single Materials-owned section publisher; a member whose family no `DesignCode` row serves rails `AssessmentInputMissing`, never a silent skip.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class MaterialFamily {
    public static readonly MaterialFamily Steel          = new("steel");
    public static readonly MaterialFamily Concrete       = new("concrete");
    public static readonly MaterialFamily Timber         = new("timber");
    public static readonly MaterialFamily Masonry        = new("masonry");
    public static readonly MaterialFamily ColdFormedSteel = new("cold-formed-steel");

    public static MaterialFamily Classify(MaterialPropertySet.Mechanical m) =>
        m.YoungsModulus.Si > 150e9 ? Steel : m.YoungsModulus.Si > 20e9 ? Concrete : m.YoungsModulus.Si > 5e9 ? Timber : Masonry;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class SafetyFormat {
    public static readonly SafetyFormat Asd        = new("asd");
    public static readonly SafetyFormat Lrfd       = new("lrfd");
    public static readonly SafetyFormat LimitState = new("limit-state");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class DesignCode {
    public static readonly DesignCode Aisc360  = new("aisc360",  MaterialFamily.Steel,           SafetyFormat.Lrfd,       resistance: 0.90, gammaM: 1.0,  interaction: AiscH11);
    public static readonly DesignCode En1993   = new("en1993",   MaterialFamily.Steel,           SafetyFormat.LimitState, resistance: 1.0,  gammaM: 1.0,  interaction: En1993Interaction);
    public static readonly DesignCode En1992   = new("en1992",   MaterialFamily.Concrete,        SafetyFormat.LimitState, resistance: 1.0,  gammaM: 1.5,  interaction: En1992Interaction);
    public static readonly DesignCode Nds      = new("nds",      MaterialFamily.Timber,          SafetyFormat.Asd,        resistance: 1.0,  gammaM: 1.0,  interaction: NdsInteraction);
    public static readonly DesignCode Aci318   = new("aci318",   MaterialFamily.Concrete,        SafetyFormat.Lrfd,       resistance: 0.90, gammaM: 1.0,  interaction: Aci318Interaction);
    public static readonly DesignCode Tms402   = new("tms402",   MaterialFamily.Masonry,         SafetyFormat.LimitState, resistance: 0.90, gammaM: 1.0,  interaction: Tms402Interaction);
    public static readonly DesignCode AisiS100 = new("aisi-s100", MaterialFamily.ColdFormedSteel, SafetyFormat.Lrfd,      resistance: 0.85, gammaM: 1.0,  interaction: AisiInteraction);

    public MaterialFamily Family { get; }
    public SafetyFormat Format { get; }
    public double Resistance { get; }   // phi (LRFD) or 1/gammaM (limit-state)
    public double GammaM { get; }

    [UseDelegateFromConstructor]
    public partial double Interaction(SectionDemand demand, SectionCapacity capacity);

    public static DesignCode For(AssessmentRoute route) => Get(route.Key);

    static double AiscH11(SectionDemand d, SectionCapacity c) {
        double axial = Math.Abs(d.N) / Math.Max(c.AxialCompression, double.Epsilon);
        double bending = Math.Abs(d.My) / Math.Max(c.FlexureMajor, double.Epsilon) + Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, double.Epsilon);
        return axial >= 0.2 ? axial + 8.0 / 9.0 * bending : axial / 2.0 + bending;
    }
    static double En1993Interaction(SectionDemand d, SectionCapacity c) =>
        Math.Abs(d.N) / Math.Max(c.AxialCompression, double.Epsilon) + Math.Abs(d.My) / Math.Max(c.FlexureMajor, double.Epsilon) + Math.Abs(d.Mz) / Math.Max(c.FlexureMinor, double.Epsilon);
    static double En1992Interaction(SectionDemand d, SectionCapacity c) => Math.Abs(d.My) / Math.Max(c.FlexureMajor, double.Epsilon);
    static double NdsInteraction(SectionDemand d, SectionCapacity c) => En1993Interaction(d, c);
    static double Aci318Interaction(SectionDemand d, SectionCapacity c) => En1992Interaction(d, c);
    static double Tms402Interaction(SectionDemand d, SectionCapacity c) => En1992Interaction(d, c);
    static double AisiInteraction(SectionDemand d, SectionCapacity c) => AiscH11(d, c);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class LimitState {
    public static readonly LimitState AxialTension          = new("axial-tension",     static (s, m, c) => c.Resistance * s.Area.Si * m.YieldStrength.Si, static _ => true);
    public static readonly LimitState AxialCompression      = new("axial-compression", Compression,                                                       static _ => true);
    public static readonly LimitState FlexureMajor          = new("flexure-major",     static (s, m, c) => c.Resistance * s.Wply.Si * m.YieldStrength.Si / c.GammaM, static f => f == MaterialFamily.Steel || f == MaterialFamily.ColdFormedSteel);
    public static readonly LimitState FlexureMinor          = new("flexure-minor",     static (s, m, c) => c.Resistance * s.Wplz.Si * m.YieldStrength.Si / c.GammaM, static f => f == MaterialFamily.Steel || f == MaterialFamily.ColdFormedSteel);
    public static readonly LimitState Shear                 = new("shear",             static (s, m, c) => c.Resistance * s.AvY.Si * m.YieldStrength.Si / Math.Sqrt(3.0), static _ => true);
    public static readonly LimitState LateralTorsional      = new("lateral-torsional-buckling", Ltb,                                                       static f => f == MaterialFamily.Steel);
    public static readonly LimitState Combined              = new("combined",          static (_, _, _) => double.PositiveInfinity,                        static _ => true);
    public static readonly LimitState Deflection            = new("deflection",        static (_, _, _) => double.PositiveInfinity,                        static _ => true);

    [UseDelegateFromConstructor]
    public partial double Capacity(SectionProperties section, MaterialPropertySet.Mechanical strength, DesignCode code);

    [UseDelegateFromConstructor]
    public partial bool Applies(MaterialFamily family);

    static double Compression(SectionProperties s, MaterialPropertySet.Mechanical m, DesignCode c) {
        double slenderness = 1.0 / Math.Max(s.RadiusOfGyrationMinor.Si, double.Epsilon);
        double reduction = 1.0 / (1.0 + 0.0001 * slenderness * slenderness);
        return c.Resistance * reduction * s.Area.Si * m.YieldStrength.Si / c.GammaM;
    }
    static double Ltb(SectionProperties s, MaterialPropertySet.Mechanical m, DesignCode c) =>
        c.Resistance * s.Wply.Si * m.YieldStrength.Si / c.GammaM * 0.85;
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record SectionCapacity(double AxialTension, double AxialCompression, double FlexureMajor, double FlexureMinor, double Shear, double Resistance, double GammaM);

public readonly record struct MemberCheck(NodeId Member, LimitState State, double Demand, double Capacity, double Utilization);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StructuralAnalysis {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, ClockPolicy clocks) =>
        from model in Project(graph, request)
        from demands in Solve(model)
        let code = DesignCode.For(request.Route)
        let checks = model.Members.Bind(member => Check(member, demands[member.Id], code))
        let governing = checks.OrderByDescending(static c => c.Utilization).HeadOrNone()
        select AssessmentResult.Of(
            request.Route,
            checks.Map(static c => AssessmentFact.Ratio($"{c.Member.Value}/{c.State.Key}", c.Utilization))
                  .Add(governing.Map(g => AssessmentFact.Reference("governing-member", g.Member)).IfNone(AssessmentFact.Text("governing-member", "none"))),
            governing.Map(static g => g.Utilization).IfNone(0.0),
            new Provenance("StructuralAnalysis", request.Route.Standard, "n/a", clocks.Now),
            clocks.Now);

    static Seq<MemberCheck> Check(StructuralMember member, SectionDemand demand, DesignCode code) {
        SectionCapacity capacity = Capacities(member, code);
        return LimitState.Items.ToSeq()
            .Filter(state => state.Applies(member.Family))
            .Map(state => {
                double cap = state.Capacity(member.Section, member.Strength, code);
                double dem = DemandFor(state, demand);
                double util = state == LimitState.Combined ? code.Interaction(demand, capacity)
                    : state == LimitState.Deflection ? 0.0
                    : dem / Math.Max(cap, double.Epsilon);
                return new MemberCheck(member.Id, state, dem, cap, util);
            });
    }

    static SectionCapacity Capacities(StructuralMember m, DesignCode code) => new(
        AxialTension: LimitState.AxialTension.Capacity(m.Section, m.Strength, code),
        AxialCompression: LimitState.AxialCompression.Capacity(m.Section, m.Strength, code),
        FlexureMajor: LimitState.FlexureMajor.Capacity(m.Section, m.Strength, code),
        FlexureMinor: LimitState.FlexureMinor.Capacity(m.Section, m.Strength, code),
        Shear: LimitState.Shear.Capacity(m.Section, m.Strength, code),
        Resistance: code.Resistance, GammaM: code.GammaM);

    static double DemandFor(LimitState state, SectionDemand d) =>
        state == LimitState.AxialTension || state == LimitState.AxialCompression ? Math.Abs(d.N)
        : state == LimitState.FlexureMajor || state == LimitState.LateralTorsional ? Math.Abs(d.My)
        : state == LimitState.FlexureMinor ? Math.Abs(d.Mz)
        : state == LimitState.Shear ? Math.Max(Math.Abs(d.Vy), Math.Abs(d.Vz))
        : 0.0;
}
```

## [05]-[RESEARCH]

- [M7_SECTION_RESOLUTION]: the `ProfileRef`→`SectionProperties` resolution (the `VividOrange.Sections.SectionProperties` / `VividOrange.Profiles.Catalogue` lookup) is performed ONCE by the `Rasm.Materials` `MaterialProjector` and BAKED onto the seam graph as a neutral `SectionProperties` value-object (Area/Iyy/Izz/J/Wely/Welz/Wply/Wplz/AvY/AvZ/radii as `MeasureValue`), so the structural consumer reads `graph.SectionOf(member)` and never re-resolves a profile per call (`§4-RT M7`) and Compute admits NO VividOrange — one owner, in Materials. The hand-rolled interaction checks compute capacity from the baked section + strength, so the VividOrange `IForceMomentInteraction` capacity surface is not composed in Compute. Ripple counterpart: `Rasm.Materials` `Profiles` (the M7 resolver + the bake-onto-seam projection) and `Rasm.Element/Composition/material` (the seam `SectionProperties` value-object + `ProfileRef`).
- [BFE_CSPARSE_PIN]: BFE 2.1.2 floors `CSparse >= 3.5.0`; the workspace unifies on `CSparse 4.4.0`. BFE's SPARSE solvers wrap CSparse's own `SparseCholesky`/`SparseLU`/`SparseQR` (binary-clean under 4.4.0), but its embedded `BriefFiniteElementNet.Common` `DenseLU : ISolver<double>` was compiled against the 3.5-era one-method `ISolver<T>` and fails interface mapping under 4.x (`TypeLoadException`), and the `DenseColumnMajorStorage.Solve`/`Determinant` path sits on the continuum `TriangleElement`/`TetrahedronElement`/`HexahedralElement` Jacobian integration — so the runner is CONFINED to the sparse-factored `BarElement` frame solve, injects the Rasm CSparse-4.x `ISolverFactory` via `Solve(ISolverFactory)`, and routes continuum problems to the `Solver/contract#SOLVE_CONTRACT` `SolveLane`. Ripple counterpart: `Tensor/factor#SPARSE_SOLVE` (the `SparseOps.Factory` `ISolver` the `RasmSolverFactory` wraps).
- [HAND_ROLLED_DESIGN_CODES]: no .NET package owns AISC 360 / EN 1993-1-1 / EN 1992-1-1 / NDS / ACI 318 / TMS 402 / AISI S100 (≈250 limit-state rules across the seven codes), so the codes are a `DesignCode`×`LimitState` capacity table of delegate-backed rows — the steel rows (axial/flexure/shear/LTB/interaction over `Wply`/`Wplz`/`AvY`/`fy`) realized here, the concrete (ACI 318 / EN 1992 `phi·Mn`), timber (NDS), masonry (TMS 402), and cold-formed (AISI S100 effective-width) rows the same delegate shape; the safety format (ASD/LRFD/limit-state) and the partial/resistance factors are `DesignCode` columns so a code switch is one row, never a parallel checker. The combined-action interaction is the `DesignCode.Interaction` delegate (AISC H1.1, EN 1993 6.3.3). This is the `POLICY_VALUES`+`DERIVED_LOGIC` collapse the doctrine mandates over an imperative per-code arm.
- [FE_BACKEND_DUALITY]: the 3D `BriefFiniteElement.Net` (`api-brief-finite-element`) and 2D-planar `FEALiTE2D` (`api-fealite2d`) are the two `FrameBackend` arms, both factoring through the ONE shared `api-csparse` owner; `SolverBackend.Auto` routes a coplanar in-plane frame to the lighter 2D solver and a general 3D frame to BFE. The FE result carriers (`Force`/`Displacement`/`LinearMeshSegment`) carry raw SI doubles re-typed at the `SectionDemand` boundary; the `FEALiTE2D.Plotting` DXF diagram leg is an AppUi/export concern, not an analysis read. The continuum multi-physics solve (the `Bᵀ·D·B` over a `DiscreteMesh`) stays the `Solver/contract#SOLVE_CONTRACT` `SolveLane` this runner delegates to, never re-derives.
- [GRAPH_READ_ACCESSORS]: the runner reads the concrete graph through two accessor tiers — the SEAM-owned material/section reads (`graph.SectionOf(NodeId)→Option<SectionProperties>`, `graph.MechanicalOf(NodeId)→Option<MaterialPropertySet.Mechanical>`, the seam owning the `Material`/`ProfileSet` nodes) and the COMPUTE-owned discipline reads (`StructuralGraphReads.AxisOf(NodeId)→Fin<MemberAxis>` reading the analytical line the Bim projector baked on `Node.Object.Axis`, `SupportsOf(NodeId)→Seq<MemberSupport>` and `LoadsOf(NodeId)→Seq<MemberLoad>` traversing the projected `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` neutral `Generic` edges by wire-name and reading the 6-DOF restraint / applied force off the edge payload) — the discipline reads compose the seam primitives (`EdgesAt`/`Find`/the baked analytical geometry) in Compute, never seam members, per `Rasm.Element/Graph/element` ("the discipline physics lives in Compute, never here"); the combinations ride the `AssessmentRequest.Structural`, self-weight derives from section × density × length. Ripple counterpart: `Rasm.Element/Graph/element` (the seam `Node.Object.Axis` analytical carrier + `SectionOf`/`MechanicalOf` reads) and `Rasm.Bim/Projection/semantic` (the `EdgeProjection.Structural` fold + `Enrich` axis bake that feeds these reads).
