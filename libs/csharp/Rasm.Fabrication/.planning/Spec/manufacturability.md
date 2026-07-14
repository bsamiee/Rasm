# [RASM_FABRICATION_MANUFACTURABILITY]

The cross-modality DfM owner folds an `AdmittedComponent` and input-carried `DfmPolicy` evidence into typed producibility receipts through one `Assess` entry. Removal, additive, formed, joined, shared-wall, and tolerance-capability lanes preserve their `Fin` faults as `Validation`, join applicatively, and lower once, so one kernel failure never suppresses an independent lane. The policy carries the admitted modality-physics map, machine approach vectors, and demanded-frame capability rows; no string property lookup or caller-faith achievable scalar enters the fold. Each `DfmCheck` owns its `DfmMeasureKind`, and process-specific tolerance blockers carry `DfmLocus.AtProcess`, so routing interprets every scalar and applies evidence only to its process.

Process routing is the second product of the same fold: one `RoutingRow` per `ProcessKind` is viable only when its class lane is admitted, the policy physics map contains its modality, its general blockers are empty, and no `AtProcess` tolerance blocker targets it. `Kinematics/fleet` remains the machine-capability owner. Stackup feasibility remains the assembly-chain pre-check, while correlated distribution stackup stays `Spec/capability`. `Additive/support` keeps layer-exact overhang truth, `Fixturing/assembly` keeps approach derivation and holding standoff, and `Forming/sheet` keeps unfold-time bend-radius enforcement.

Wire posture: HOST-LOCAL. `DfmReport` crosses only the in-process seam to the derivation orchestrator, the traveler fan-in, and the capability gate — never a browser or peer wire; no verdict row sits between wire and rail. Rendering is the artifacts plane's: this page never draws, annotates, or emits a document.

## [01]-[INDEX]

- [01]-[MANUFACTURABILITY]: owns severity, measure, check, and locus vocabularies; the policy evidence carrier; the verdict/routing/report receipt family; and the one `Manufacturability.Assess` fold over applicative lanes, shared wall evidence, process routing, tolerance capability, and stackup pre-check.

## [02]-[MANUFACTURABILITY]

- Owner: `DfmSeverity` owns gate posture; `DfmMeasureKind` owns scalar interpretation; `DfmCheck` binds applicability and measure; `DfmLocus` owns point/edge/face/joint/process/global sites; `DfmPolicy` carries thresholds plus approach, physics, and tolerance evidence; `DfmVerdict`, `RoutingRow`, and `DfmReport` are the receipt family; `Manufacturability.Assess` is the sole fold.
- Cases: `DfmSeverity` rows 3; `DfmMeasureKind` rows 4; `DfmCheck` rows 14 — lane admission, geometry evidence, material physics, draft, undercut, tool access, aspect ratio, corner radius, minimum wall, bend radius, weld access, overhang, integrity, and tolerance capability; `DfmLocus` cases 6. Formed evidence exists only when both sheet thickness and forming physics exist, while missing joined or additive geometry is a typed failure rather than clear access.
- Entry: `public static Fin<DfmReport> Assess(AdmittedComponent component, DfmPolicy policy)` — the ONE cross-modality fold; `Fin<T>` routes only composed kernel failures (`GeometryFault` band-2400 pass-through) — a failed spatial build or ray query PROPAGATES as typed failure, never reads as clear access; an infeasible component is a REPORT full of blockers, never a fault — `Process/derivation` reads the report and routes its own `RoutingInfeasible` 2730 when routing exhausts.
- Auto: `Assess` joins policy admission plus six validation lanes, derives routing from typed geometry evidence, the admitted physics map, and process-targeted blockers, and folds the connection stackup pre-check. Removal composes `Faces.Bottom`, isolated native face normals, normalized approach-vector `SpatialQuery.Ray` probes, and straight and bulged concave corners. The shared wall census is total over admitted geometry — one `OffsetOp.Medial` query per profile, the sheet-thickness fast-check, and one inward centroid ray probe per mesh face — so a mesh-only removal or additive shape still lands `MinWall` evidence or a typed lane failure, never a silent pass. Formed compares bulge radii with input-carried forming physics. Joined and additive propagate absent required geometry, build failures, and query failures. Tolerance capability spends MMC/LMC departure through `Tolerance.Effective` and blocks only the targeted process when historical achievable width exceeds the effective demanded zone.
- Receipt: `DfmReport` IS the evidence — typed verdict rows with measured/bound scalars and typed loci, routing rows with typed blocker sets, one stackup boolean; no score-only summary, no generic issue list, no string diagnosis.
- Packages: `Rasm.Analysis` (`Analyze.Run`/`AnalysisQuery`, `Faces.Bottom`, `Meshes.Defects`), `Rasm.Meshing` (`Offsetting.Apply`, `OffsetOp.Medial`), `Rasm.Spatial` (`Spatial.Apply`, `SpatialQuery.Ray`), `Rasm.Domain` (`Op`), `Process/owner`, `Process/family`, `Process/physics` (`ModalityPhysics`), `Spec/tolerance` (`FeatureControl`, `Tolerance.Effective`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Validation`/`TraverseM`/`Apply`), RhinoCommon, BCL inbox.
- Growth: a new producibility concern is one `DfmCheck` row plus one lane term; a new scalar class is one `DfmMeasureKind` row; a new process-targeted concern uses `DfmLocus.AtProcess`. Feature recognition remains gated on signed per-edge concavity, analytic face classification, and exposed face adjacency in the kernel; zero new entrypoints.
- Boundary: ONE DfM surface — per-modality `MillingDfm`/`PrintabilityChecker`/`WeldabilityAudit` siblings are the deleted form; verdicts are receipts and a fault arm for "hard to make" is the named misuse (the receipts-only law — hard gates belong to the owning folds); a spatial failure mapped to a clear-access verdict is the named seam-erasure defect — build and query failures stay on the rail; the routing row answers process viability and a machine join here is fleet's stolen concern; kernel geometry composes at the verified fronts and a second medial, normal classifier, or ray walker is the named re-implementation defect; rendering is the artifacts plane's and a drawing/annotation emission here is the boundary violation; a `DfmCheck` re-encoding modality (a `mill-draft` beside `draft`) is the deleted axis-duplication.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using Rasm.Analysis;                      // Analyze.Run · AnalysisQuery · Faces.Bottom · Meshes.Defects census
using Rasm.Domain;
using Rasm.Fabrication.Process;           // AdmittedComponent · ModalityClass · ProcessModality · ProcessKind · Material · ModalityPhysics
using Rasm.Meshing;                       // MeshSpace · Offsetting.Apply · OffsetOp.Medial · ClearanceNode (K1)
using Rasm.Spatial;                       // SpatialIndex · SpatialQuery.Ray
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DfmSeverity {
    public static readonly DfmSeverity Advisory = new("advisory", gate: false);
    public static readonly DfmSeverity Warning = new("warning", gate: false);
    public static readonly DfmSeverity Blocker = new("blocker", gate: true);

    public bool Gate { get; }
}

// One check row per concern; the Classes set selects it into lanes — never a per-modality check sibling.
[SmartEnum<string>]
public sealed partial class DfmMeasureKind {
    public static readonly DfmMeasureKind LengthMm = new("mm");
    public static readonly DfmMeasureKind AngleDeg = new("deg");
    public static readonly DfmMeasureKind Ratio = new("ratio");
    public static readonly DfmMeasureKind Count = new("count");
}

[SmartEnum<string>]
public sealed partial class DfmCheck {
    public static readonly DfmCheck LaneAdmission = new(
        "lane-admission",
        Set(ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined),
        DfmMeasureKind.Count);
    public static readonly DfmCheck GeometryEvidence = new(
        "geometry-evidence",
        Set(ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined),
        DfmMeasureKind.Count);
    public static readonly DfmCheck MaterialPhysics = new(
        "material-physics",
        Set(ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined),
        DfmMeasureKind.Count);
    public static readonly DfmCheck Draft = new("draft", Set(ModalityClass.Removal), DfmMeasureKind.AngleDeg);
    public static readonly DfmCheck Undercut = new("undercut", Set(ModalityClass.Removal), DfmMeasureKind.AngleDeg);
    public static readonly DfmCheck ToolAccess = new("tool-access", Set(ModalityClass.Removal), DfmMeasureKind.LengthMm);
    public static readonly DfmCheck AspectRatio = new("aspect-ratio", Set(ModalityClass.Removal), DfmMeasureKind.Ratio);
    public static readonly DfmCheck CornerRadius = new("corner-radius", Set(ModalityClass.Removal), DfmMeasureKind.LengthMm);
    public static readonly DfmCheck MinWall = new("min-wall", Set(ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed), DfmMeasureKind.LengthMm);
    public static readonly DfmCheck BendRadius = new("bend-radius", Set(ModalityClass.Formed), DfmMeasureKind.LengthMm);
    public static readonly DfmCheck WeldAccess = new("weld-access", Set(ModalityClass.Joined), DfmMeasureKind.AngleDeg);
    public static readonly DfmCheck Overhang = new("overhang", Set(ModalityClass.Additive), DfmMeasureKind.AngleDeg);
    public static readonly DfmCheck Integrity = new("integrity", Set(ModalityClass.Additive), DfmMeasureKind.Count);
    public static readonly DfmCheck ToleranceCapability = new(
        "tolerance-capability",
        Set(ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined),
        DfmMeasureKind.LengthMm);

    public Set<ModalityClass> Classes { get; }
    public DfmMeasureKind Measure { get; }

    public bool AppliesTo(ModalityClass cls) => Classes.Contains(cls);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmLocus {
    private DfmLocus() { }

    public sealed record AtPoint(Point3d Point) : DfmLocus;
    public sealed record AtEdge(Edge3 Edge) : DfmLocus;
    public sealed record AtFace(int Face) : DfmLocus;
    public sealed record AtJoint(int Joint) : DfmLocus;
    public sealed record AtProcess(ProcessKind Process) : DfmLocus;
    public sealed record Global() : DfmLocus;
}

// Threshold row: every bound a lane reads is a policy datum; OverhangCriticalDeg aligns with SupportPolicy.CriticalAngleDeg by seed, not by reference.
public sealed record DfmPolicy(
    double MinDraftDeg,
    double AccessConeHalfAngleDeg,
    double MinWallMm,
    double MinInternalCornerRadiusMm,
    double MaxDepthToDiameter,
    double OverhangCriticalDeg,
    double TorchConeHalfAngleDeg,
    double StackupAllowancePerJointMm,
    double StackupBoundMm,
    double ProbeReachMm,
    Arr<Vector3d> ToolApproaches,
    Map<ProcessModality, ModalityPhysics> Physics,
    Seq<(FeatureControl Frame, ProcessKind Process, double AchievableMm, double DepartureMm)> Tolerances,
    Set<ModalityClass> Lanes) {
    public static DfmPolicy Canonical =>
        new(
            2.0, 30.0, 1.0, 1.0, 4.0, 45.0, 35.0, 0.5, 3.0, 500.0,
            Arr(Vector3d.ZAxis), Map<ProcessModality, ModalityPhysics>(),
            Seq<(FeatureControl Frame, ProcessKind Process, double AchievableMm, double DepartureMm)>(),
            Set(ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined));
}

public sealed record DfmVerdict(DfmCheck Check, DfmSeverity Severity, DfmLocus Locus, double Measured, double Bound);

public sealed record RoutingRow(ProcessKind Process, bool Viable, Seq<DfmCheck> Blockers, int Friction);

public sealed record DfmReport(UInt128 ComponentKey, Seq<DfmVerdict> Verdicts, Seq<RoutingRow> Rows, bool StackupPrecheck) {
    public Seq<DfmVerdict> Blockers(ModalityClass cls) => Verdicts.Filter(v => v.Severity.Gate && v.Check.AppliesTo(cls));

    // THE derivation contract: the RANKED viable-process sequence (least advisory/warning friction first);
    // empty ⇒ derivation routes ITS RoutingInfeasible 2730 at the routing stage. Rows keep the typed blocker evidence.
    public Seq<ProcessKind> Routing => Rows.Filter(r => r.Viable).OrderBy(r => r.Friction).ToSeq().Map(r => r.Process);

    public bool Feasible(ModalityClass cls) => Blockers(cls).IsEmpty;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Manufacturability {
    static readonly Op DfmOp = Op.Of(name: "fabrication:manufacturability");

    // ONE cross-modality fold: independent lanes join applicatively — a removal-lane kernel failure never suppresses
    // additive evidence, and every lane failure is a typed rail value. Infeasibility is a REPORT, never a fault.
    public static Fin<DfmReport> Assess(AdmittedComponent component, DfmPolicy policy) =>
        (Admit(policy).Map(static _ => Seq<DfmVerdict>()).ToValidation(),
         (policy.Lanes.Contains(ModalityClass.Removal) ? RemovalLane(component, policy) : Fin.Succ(Seq<DfmVerdict>())).ToValidation(),
         (policy.Lanes.Contains(ModalityClass.Additive) ? AdditiveLane(component, policy) : Fin.Succ(Seq<DfmVerdict>())).ToValidation(),
         (policy.Lanes.Contains(ModalityClass.Formed) ? FormedLane(component, policy) : Fin.Succ(Seq<DfmVerdict>())).ToValidation(),
         (policy.Lanes.Contains(ModalityClass.Joined) ? JoinedLane(component, policy) : Fin.Succ(Seq<DfmVerdict>())).ToValidation(),
         WallLane(component, policy).ToValidation(),
         ToleranceLane(policy).ToValidation())
            .Apply(static (policyAdmission, removal, additive, formed, joined, walls, tolerances) =>
                policyAdmission + removal + additive + formed + joined + walls + tolerances)
            .As()
            .ToFin()
            .Map(verdicts => new DfmReport(
                component.RepresentationKey,
                verdicts,
                Route(component, policy, verdicts),
                component.Connections.Count * policy.StackupAllowancePerJointMm <= policy.StackupBoundMm));

    // Routing = family-axis viability: the material map must hold the modality AND the class lane must have been
    // ADMITTED and gate-clean — an unassessed lane never reads as clear. Machine matching is Fleet.Capable's.
    static Seq<RoutingRow> Route(AdmittedComponent component, DfmPolicy policy, Seq<DfmVerdict> verdicts) =>
        toSeq(ProcessKind.Items).Map(kind => {
            Seq<DfmVerdict> lane = verdicts.Filter(v => v.Check.AppliesTo(kind.Modality.Class)
                && (v.Locus is not DfmLocus.AtProcess process || process.Process == kind));
            bool geometry = GeometryAdmits(component, kind.Modality.Class);
            bool assessed = policy.Lanes.Contains(kind.Modality.Class);
            bool material = policy.Physics.Find(kind.Modality).IsSome;
            Seq<DfmCheck> blockers = (lane.Filter(v => v.Severity.Gate).Map(v => v.Check)
                + (assessed ? Seq<DfmCheck>() : Seq1(DfmCheck.LaneAdmission))
                + (geometry ? Seq<DfmCheck>() : Seq1(DfmCheck.GeometryEvidence))
                + (material ? Seq<DfmCheck>() : Seq1(DfmCheck.MaterialPhysics))).Distinct();
            return new RoutingRow(kind, assessed && material && blockers.IsEmpty, blockers, lane.Filter(v => !v.Severity.Gate).Count);
        });

    static Fin<Unit> Admit(DfmPolicy policy) =>
        Seq(
            guard(double.IsFinite(policy.MinDraftDeg) && policy.MinDraftDeg is >= 0.0 and < 90.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.AccessConeHalfAngleDeg) && policy.AccessConeHalfAngleDeg is > 0.0 and < 90.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.MinWallMm) && policy.MinWallMm > 0.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.MinInternalCornerRadiusMm) && policy.MinInternalCornerRadiusMm > 0.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.MaxDepthToDiameter) && policy.MaxDepthToDiameter > 0.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.OverhangCriticalDeg) && policy.OverhangCriticalDeg is > 0.0 and < 90.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.TorchConeHalfAngleDeg) && policy.TorchConeHalfAngleDeg is > 0.0 and < 90.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.StackupAllowancePerJointMm) && policy.StackupAllowancePerJointMm >= 0.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.StackupBoundMm) && policy.StackupBoundMm > 0.0, DfmOp.InvalidInput()).ToValidation(),
            guard(double.IsFinite(policy.ProbeReachMm) && policy.ProbeReachMm > 0.0, DfmOp.InvalidInput()).ToValidation(),
            guard(!policy.ToolApproaches.IsEmpty && policy.ToolApproaches.ForAll(static vector => vector.IsValid && vector.Length > 0.0), DfmOp.InvalidInput()).ToValidation(),
            guard(policy.Tolerances.ForAll(static row =>
                double.IsFinite(row.AchievableMm) && row.AchievableMm > 0.0
                && double.IsFinite(row.DepartureMm) && row.DepartureMm >= 0.0), DfmOp.InvalidInput()).ToValidation())
            .Traverse(static validation => validation)
            .As()
            .ToFin();

    static bool GeometryAdmits(AdmittedComponent component, ModalityClass modality) =>
        modality == ModalityClass.Removal
            ? component.Mesh.IsSome || !component.Profiles.IsEmpty
            : modality == ModalityClass.Additive
                ? component.Mesh.IsSome
                : modality == ModalityClass.Formed
                    ? component.SheetThicknessMm.IsSome && !component.Profiles.IsEmpty
                    : modality != ModalityClass.Joined || component.Mesh.IsSome && !component.Connections.IsEmpty;

    // The SHARED wall census over EVERY admitted geometry source: one Medial query per profile (2·min Radius vs
    // the floor), a present sheet thickness below the floor as the formed fast-check, and one inward centroid ray
    // probe per mesh face as the solid census — so a mesh-only removal or additive shape still lands MinWall
    // evidence and the check's Classes set and its execution never disagree; the aspect column emits only under
    // the removal lane, and a failed BVH build or ray query stays a typed lane failure, never silence.
    static Fin<Seq<DfmVerdict>> WallLane(AdmittedComponent component, DfmPolicy policy) {
        if (!policy.Lanes.Contains(ModalityClass.Removal) && !policy.Lanes.Contains(ModalityClass.Additive) && !policy.Lanes.Contains(ModalityClass.Formed))
            return Fin.Succ(Seq<DfmVerdict>());
        bool aspect = policy.Lanes.Contains(ModalityClass.Removal);
        bool solid = policy.Lanes.Contains(ModalityClass.Removal) || policy.Lanes.Contains(ModalityClass.Additive);
        double heightMm = component.Mesh.Map(static m => {
            BoundingBox box = m.Native.GetBoundingBox(accurate: false);
            return box.Max.Z - box.Min.Z;
        }).IfNone(0.0);
        Seq<DfmVerdict> sheet = component.SheetThicknessMm
            .Filter(t => t < policy.MinWallMm)
            .Map(t => new DfmVerdict(DfmCheck.MinWall, DfmSeverity.Blocker, new DfmLocus.Global(), t, policy.MinWallMm))
            .ToSeq();
        Fin<Seq<DfmVerdict>> seeded = solid
            ? component.Mesh.Match(
                Some: mesh => MeshWallVerdicts(mesh, policy).Map(rows => sheet + rows),
                None: () => Fin.Succ(sheet))
            : Fin.Succ(sheet);
        return seeded.Bind(rows => component.Profiles.ToSeq()
            .Fold(Fin.Succ(rows), (acc, loop) => acc.Bind(seq =>
                Offsetting.Apply(new OffsetOp.Medial(ToPolyline(loop), OffsetPolicy.Canonical)).Map(medial => seq + WallVerdicts(medial, heightMm, aspect, policy)))));
    }

    // The solid half of the ONE MinWall identity: an inward ray from each face centroid against the face BVH reads
    // the local thickness to the opposite wall; the standoff keeps the probe off its own seed face and rides back
    // onto the measured value, and the ray range caps at the floor — hits past it are conforming walls.
    static Fin<Seq<DfmVerdict>> MeshWallVerdicts(MeshSpace mesh, DfmPolicy policy) {
        double standoff = policy.MinWallMm / 100.0;
        return BvhOf(mesh).Bind(index => FaceNormals(mesh)
            .Fold(Fin.Succ(Seq<DfmVerdict>()), (acc, row) => acc.Bind(seq => {
                Vector3d inward = -row.Normal;
                inward.Unitize();
                Point3d origin = mesh.Native.Faces.GetFaceCenter(row.Face) + (inward * standoff);
                return RayHitT(index, new Ray3d(origin, inward), policy.MinWallMm).Map(hit => seq + hit
                    .Filter(t => t + standoff < policy.MinWallMm)
                    .Map(t => new DfmVerdict(DfmCheck.MinWall, DfmSeverity.Blocker, new DfmLocus.AtFace(row.Face), t + standoff, policy.MinWallMm))
                    .ToSeq());
            })));
    }

    // Removal lane: draft/undercut off one face-normal partition against the ranked resting set, access off BVH rays,
    // sharp internal corners off the vertex/bulge capture — each term Fin-shaped so a kernel failure stays typed.
    static Fin<Seq<DfmVerdict>> RemovalLane(AdmittedComponent component, DfmPolicy policy) =>
        (DraftVerdicts(component, policy).ToValidation(), AccessVerdicts(component, policy).ToValidation())
            .Apply((draft, access) => draft + access + component.Profiles.ToSeq().Bind(loop => CornerVerdicts(loop, policy)))
            .As()
            .ToFin();

    // Formed lane: fillet radius from the bulge span (R = c·(1+b²)/(4|b|)) vs Forming.MinBendRadiusFactor·T — the DfM
    // altitude; Forming/sheet re-derives per-bend at unfold and owns MinBendRadiusViolated 2743.
    static Fin<Seq<DfmVerdict>> FormedLane(AdmittedComponent component, DfmPolicy policy) =>
        (from thickness in component.SheetThicknessMm
         from physics in policy.Physics.Find(ProcessModality.Formed)
         from row in physics is ModalityPhysics.Forming forming ? Some(forming) : Option<ModalityPhysics.Forming>.None
         select component.Profiles.ToSeq().Bind(loop => BulgeRadii(loop)
             .Filter(radius => radius.Radius < row.MinBendRadiusFactor * thickness)
             .Map(radius => new DfmVerdict(
                 DfmCheck.BendRadius,
                 DfmSeverity.Blocker,
                 new DfmLocus.AtEdge(radius.Span),
                 radius.Radius,
                 row.MinBendRadiusFactor * thickness))))
        .Match(
            Some: static verdicts => Fin.Succ(verdicts),
            None: static () => Fin.Succ(Seq<DfmVerdict>()));

    static Fin<Seq<DfmVerdict>> ToleranceLane(DfmPolicy policy) =>
        policy.Tolerances.TraverseM(static row =>
            Tolerance.Effective(row.Frame, row.DepartureMm).Map(effective =>
                row.AchievableMm <= effective
                    ? Option<DfmVerdict>.None
                    : Some(new DfmVerdict(
                        DfmCheck.ToleranceCapability,
                        DfmSeverity.Blocker,
                        new DfmLocus.AtProcess(row.Process),
                        row.AchievableMm,
                        effective))))
            .As()
            .Map(static rows => rows.Somes().ToSeq());

    // Joined lane: one torch-cone probe per connection along the assembly-owned approach law (left XY normal of the
    // At edge); assembly gates holding standoff, this lane gates the torch cone — Joining/weld reads the verdict.
    static Fin<Seq<DfmVerdict>> JoinedLane(AdmittedComponent component, DfmPolicy policy) =>
        toSeq(Enumerable.Range(0, component.Connections.Count))
            .Fold(Fin.Succ(Seq<DfmVerdict>()), (acc, joint) => acc.Bind(seq =>
                ConeClear(component, component.Connections[joint].At, policy).Map(clear => clear
                    ? seq
                    : seq + Seq1(new DfmVerdict(DfmCheck.WeldAccess, DfmSeverity.Blocker, new DfmLocus.AtJoint(joint), 0.0, policy.TorchConeHalfAngleDeg)))));

    // Additive lane: mesh-level census — down-facing ranked faces below the critical angle + the Meshes defect rows;
    // the layer-exact overhang set-algebra stays Support.Grow's, and layer HEIGHT stays the kernel LayerPlan (K3).
    static Fin<Seq<DfmVerdict>> AdditiveLane(AdmittedComponent component, DfmPolicy policy) =>
        component.Mesh.Match(
            Some: mesh => (OverhangVerdicts(mesh, policy).ToValidation(), IntegrityVerdicts(mesh).ToValidation())
                .Apply(static (overhang, integrity) => overhang + integrity)
                .As()
                .ToFin(),
            None: () => Fin.Fail<Seq<DfmVerdict>>(DfmOp.InvalidInput()));

    // Min-wall = 2·min(ClearanceNode.Radius) under the floor; aspect = the conservative global pocket depth (part
    // height) over 2·local radius. The Medial op answers OffsetResult.Axis carrying the K1 SkeletonGraph node set.
    static Seq<DfmVerdict> WallVerdicts(OffsetResult medial, double heightMm, bool aspect, DfmPolicy policy) =>
        medial is not OffsetResult.Axis axis
            ? Seq<DfmVerdict>()
            : axis.Medial.Nodes.Bind(node => {
                double diameter = 2.0 * node.Radius;
                return (diameter < policy.MinWallMm
                    ? Seq1(new DfmVerdict(DfmCheck.MinWall, DfmSeverity.Blocker, new DfmLocus.AtPoint(node.At), diameter, policy.MinWallMm))
                    : Seq<DfmVerdict>())
                    + (aspect && diameter > 0.0 && heightMm > 0.0 && heightMm / diameter > policy.MaxDepthToDiameter
                        ? Seq1(new DfmVerdict(DfmCheck.AspectRatio, DfmSeverity.Warning, new DfmLocus.AtPoint(node.At), heightMm / diameter, policy.MaxDepthToDiameter))
                        : Seq<DfmVerdict>());
            });

    // Draft + undercut off ONE face-normal partition: the wall draft is 90° − angle(normal, pull axis +Z); a face in
    // [0, MinDraftDeg) is the insufficient-draft warning, a negative draft the re-entrant undercut blocker. Bottom
    // extremum faces (Faces.Bottom, the K20 ranked decomposition) are the resting partition and never flag; a failed
    // resting census PROPAGATES — profiles-only parts earn no draft verdicts, draft is a solid-face concern.
    static Fin<Seq<DfmVerdict>> DraftVerdicts(AdmittedComponent component, DfmPolicy policy) =>
        component.Mesh.Match(
            None: static () => Fin.Succ(Seq<DfmVerdict>()),
            Some: mesh => BottomFaces(mesh).Map(resting =>
                FaceNormals(mesh)
                    .Filter(row => !resting.Contains(row.Face))
                    .Bind(row => {
                        double draftDeg = 90.0 - (Vector3d.VectorAngle(row.Normal, Vector3d.ZAxis) * (180.0 / Math.PI));
                        return draftDeg < 0.0
                            ? Seq1(new DfmVerdict(DfmCheck.Undercut, DfmSeverity.Blocker, new DfmLocus.AtFace(row.Face), draftDeg, 0.0))
                            : draftDeg < policy.MinDraftDeg
                                ? Seq1(new DfmVerdict(DfmCheck.Draft, DfmSeverity.Warning, new DfmLocus.AtFace(row.Face), draftDeg, policy.MinDraftDeg))
                                : Seq<DfmVerdict>();
                    })));

    // Tool access probes every admitted machine approach at every profile vertex against the mesh-face BVH. A hit
    // inside ProbeReachMm carries the limiting distance; build and query failures remain on the typed rail.
    static Fin<Seq<DfmVerdict>> AccessVerdicts(AdmittedComponent component, DfmPolicy policy) =>
        component.Mesh.Match(
            None: static () => Fin.Succ(Seq<DfmVerdict>()),
            Some: mesh => BvhOf(mesh).Bind(index => component.Profiles.ToSeq()
                .Bind(static loop => loop.AsCcw().Vertices.ToSeq())
                .Bind(point => policy.ToolApproaches.Map(approach => {
                    Vector3d direction = approach;
                    direction.Unitize();
                    return (Point: point, Approach: direction);
                }))
                .Fold(Fin.Succ(Seq<DfmVerdict>()), (acc, probe) => acc.Bind(seq =>
                    RayHitT(index, new Ray3d(probe.Point, probe.Approach), policy.ProbeReachMm).Map(hit => seq + hit
                        .Map(distance => new DfmVerdict(
                            DfmCheck.ToolAccess,
                            DfmSeverity.Blocker,
                            new DfmLocus.AtPoint(probe.Point),
                            distance,
                            policy.ProbeReachMm))
                        .ToSeq())))));

    // Sharp internal corners: a concave vertex whose adjacent spans are both straight is a zero-radius corner only
    // EDM reaches; a concave bulged span below the floor flags with its real radius. CCW winding makes concavity the
    // negative z-cross of the incoming/outgoing span directions.
    static Seq<DfmVerdict> CornerVerdicts(Loop loop, DfmPolicy policy) {
        Loop ccw = loop.AsCcw();
        Seq<DfmVerdict> sharp = toSeq(Enumerable.Range(0, ccw.Count))
            .Filter(i => {
                Point3d prev = ccw.At((i + ccw.Count - 1) % ccw.Count), at = ccw.At(i), next = ccw.At((i + 1) % ccw.Count);
                double cross = ((at.X - prev.X) * (next.Y - at.Y)) - ((at.Y - prev.Y) * (next.X - at.X));
                return cross < 0.0 && Math.Abs(ccw.BulgeAt((i + ccw.Count - 1) % ccw.Count)) == 0.0 && Math.Abs(ccw.BulgeAt(i)) == 0.0;
            })
            .Map(i => new DfmVerdict(DfmCheck.CornerRadius, DfmSeverity.Warning, new DfmLocus.AtPoint(ccw.At(i)), 0.0, policy.MinInternalCornerRadiusMm));
        return sharp + BulgeRadii(ccw)
            .Filter(row => ccw.BulgeAt(row.Index) < 0.0 && row.Radius < policy.MinInternalCornerRadiusMm)
            .Map(row => new DfmVerdict(
                DfmCheck.CornerRadius,
                DfmSeverity.Warning,
                new DfmLocus.AtEdge(row.Span),
                row.Radius,
                policy.MinInternalCornerRadiusMm));
    }

    // Torch cone: five probes — the assembly-law approach (the left XY normal of the joint edge) plus four rays tilted
    // to the cone half-angle. ANY occluded probe fails the cone; a failed spatial operation PROPAGATES, never reads
    // clear. An unresolved joint locus (the admission default) and a profiles-only part cannot be probed and read
    // clear here — assembly resolves the edge and re-gates standoff.
    static Fin<bool> ConeClear(AdmittedComponent component, Edge3 at, DfmPolicy policy) =>
        at.A.DistanceTo(at.B) <= 0.0
            ? Fin.Fail<bool>(DfmOp.InvalidInput())
            : component.Mesh.Match(
                None: static () => Fin.Fail<bool>(DfmOp.InvalidInput()),
                Some: mesh => BvhOf(mesh).Bind(index =>
                    ConeRays(at, policy.TorchConeHalfAngleDeg).Fold(Fin.Succ(true), (acc, ray) => acc.Bind(clear =>
                        clear ? RayHitT(index, ray, policy.ProbeReachMm).Map(static hit => hit.IsNone) : Fin.Succ(false)))));

    static Seq<Ray3d> ConeRays(Edge3 at, double halfAngleDeg) {
        Point3d mid = new((at.A.X + at.B.X) / 2.0, (at.A.Y + at.B.Y) / 2.0, (at.A.Z + at.B.Z) / 2.0);
        Vector3d edge = at.B - at.A;
        Vector3d approach = new(-edge.Y, edge.X, 0.0);
        approach.Unitize();
        double half = halfAngleDeg * (Math.PI / 180.0);
        Vector3d side = Vector3d.CrossProduct(approach, Vector3d.ZAxis);
        Point3d origin = mid + approach;  // 1 mm standoff keeps the probe off its own seed surface
        return Seq1(new Ray3d(origin, approach))
            + Seq(Vector3d.ZAxis, -Vector3d.ZAxis, side, -side).Map(tilt => {
                Vector3d v = approach;
                v.Rotate(half, tilt);
                return new Ray3d(origin, v);
            });
    }

    // Additive overhang census (mesh-level DfM altitude): a non-resting face whose normal sits within (90° − critical)
    // of straight down needs support — layer-exact set algebra stays Support.Grow's.
    static Fin<Seq<DfmVerdict>> OverhangVerdicts(MeshSpace mesh, DfmPolicy policy) =>
        BottomFaces(mesh).Map(resting =>
            FaceNormals(mesh)
                .Filter(row => !resting.Contains(row.Face))
                .Map(row => (row.Face, DownDeg: Vector3d.VectorAngle(row.Normal, -Vector3d.ZAxis) * (180.0 / Math.PI)))
                .Filter(row => row.DownDeg < 90.0 - policy.OverhangCriticalDeg)
                .Map(row => new DfmVerdict(DfmCheck.Overhang, DfmSeverity.Warning, new DfmLocus.AtFace(row.Face), row.DownDeg, 90.0 - policy.OverhangCriticalDeg)));

    // Printability census (K36): every positive kernel defect sample is one Integrity blocker; a census that cannot
    // run at all is itself the blocker — the additive lane never silently passes an uninspectable mesh.
    static Fin<Seq<DfmVerdict>> IntegrityVerdicts(MeshSpace mesh) =>
        Analyze.Run<Mesh, MeshSample>(new AnalysisQuery.MeshesCase(Meshes.Defects), mesh.Native)
            .ToFin()
            .Map(static samples => samples.Filter(static sample => sample.Value > 0)
                .Map(static sample => new DfmVerdict(
                    DfmCheck.Integrity,
                    DfmSeverity.Blocker,
                    new DfmLocus.Global(),
                    sample.Value,
                    0.0)));

    // --- [BOUNDARIES] -------------------------------------------------------------------------------------------------------------------------------
    // Loop → closed Rhino ring for the K1 medial query; AsCcw is the winding law, the closing vertex the ring convention.
    static Polyline ToPolyline(Loop loop) {
        Polyline ring = new(loop.AsCcw().Vertices);
        if (ring.Count > 0)
            ring.Add(ring[0]);
        return ring;
    }

    // The resting-set census stays on the rail: Analyze.Run answers Validation and lowers through ToFin — a failed
    // ranked decomposition is a typed lane failure, never an empty resting set that turns bottom faces into undercuts.
    static Fin<Set<int>> BottomFaces(MeshSpace mesh) =>
        Analyze.Run<Mesh, int>(new AnalysisQuery.FacesCase(Faces.Bottom()), mesh.Native).ToFin().Map(static faces => toSet(faces));

    static Seq<(int Face, Vector3d Normal)> FaceNormals(MeshSpace mesh) {
        Mesh native = mesh.Native.DuplicateMesh();
        native.FaceNormals.ComputeFaceNormals();
        return toSeq(Enumerable.Range(0, native.Faces.Count)).Map(i => (i, (Vector3d)native.FaceNormals[i]));
    }

    static Fin<SpatialIndex> BvhOf(MeshSpace mesh) =>
        Spatial.Apply(new SpatialOp.Build(SpatialKind.Bvh, FaceBoxes(mesh.Native), BuildPolicy.Canonical), DfmOp)
            .Bind(static answer => answer is SpatialAnswer.Index built
                ? Fin.Succ(built.Value)
                : Fin.Fail<SpatialIndex>(DfmOp.InvalidResult()));

    static Fin<Option<double>> RayHitT(SpatialIndex index, Ray3d ray, double maxT) =>
        Spatial.Apply(new SpatialOp.Query(index, new SpatialQuery.Ray(ray, maxT)), DfmOp)
            .Bind(static answer => answer switch {
                SpatialAnswer.Result { Value: QueryResult.RayHit { Id.IsSome: true } hit } => Fin.Succ(Some(hit.T)),
                SpatialAnswer.Result { Value: QueryResult.RayHit } => Fin.Succ(Option<double>.None),
                _ => Fin.Fail<Option<double>>(DfmOp.InvalidResult()),
            });

    static BoundingBox[] FaceBoxes(Mesh native) =>
        Enumerable.Range(0, native.Faces.Count)
            .Select(i => {
                MeshFace face = native.Faces[i];
                BoundingBox box = BoundingBox.Empty;
                box.Union(native.Vertices[face.A]);
                box.Union(native.Vertices[face.B]);
                box.Union(native.Vertices[face.C]);
                if (face.IsQuad)
                    box.Union(native.Vertices[face.D]);
                return box;
            })
            .ToArray();

    // Bulge span → (chord, radius): R = c·(1+b²)/(4|b|); zero-bulge spans are straight and never earn a radius verdict.
    static Seq<(int Index, Edge3 Span, double Radius)> BulgeRadii(Loop loop) =>
        toSeq(Enumerable.Range(0, loop.Count))
            .Filter(i => Math.Abs(loop.BulgeAt(i)) > 0.0)
            .Map(i => {
                double b = Math.Abs(loop.BulgeAt(i));
                double c = loop.At(i).DistanceTo(loop.At(i + 1));
                return (i, new Edge3(loop.At(i), loop.At(i + 1)), c * (1.0 + b * b) / (4.0 * b));
            });

}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    mainBkg: "#44475A"
    nodeBorder: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Manufacturability plane seams
    accDescr: The admitted component folds through applicatively joined class lanes into the DfM report whose routing projection feeds derivation, capability, and the traveler fan-in.
    Component["AdmittedComponent (owner#atoms)"] -->|"one fold"| Assess["Manufacturability.Assess"]
    Policy["DfmPolicy thresholds + admitted lanes"] -->|"policy"| Assess
    Assess -->|"removal lane"| Kernel["Faces.Bottom (K20) · SpatialQuery.Ray · OffsetOp.Medial ClearanceNode.Radius (K1)"]
    Assess -->|"formed lane"| Physics["ModalityPhysics.Forming.MinBendRadiusFactor"]
    Assess -->|"joined lane"| Approach["assembly approach law · torch cone probes"]
    Assess -->|"additive lane"| Census["Meshes.Defects census · overhang faces"]
    Assess -->|"receipt"| Report["DfmReport verdicts + RoutingRow per ProcessKind + stackup pre-check"]
    Report -->|"Routing : ranked Seq&lt;ProcessKind&gt;"| Derive["Process/derivation Run(Derive)"]
    Report -->|"TYPE seam"| Capability["Spec/capability Gate"]
    Report -.->|"fan-in"| Traveler["Run(Document) traveler Dfm lane"]
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    class Component,Policy primary
    class Assess boundary
    class Kernel,Physics,Approach,Census data
    class Report,Derive,Capability,Traveler annotation
    linkStyle 6 stroke:#50FA7B,color:#F8F8F2
    linkStyle 9 stroke:#6272A4,color:#F8F8F2,stroke-dasharray:4 6
```
