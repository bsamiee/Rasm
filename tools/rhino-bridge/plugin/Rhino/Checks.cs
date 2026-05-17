using System.Collections.Frozen;
using System.Diagnostics;

namespace Rasm.RhinoBridge.Rhino;

internal static class BridgeChecks {
    private static readonly string[] AllChecks = ["analysis-core", "faces", "curves", "meshes", "intersections", "radyab-assembly"];
    private static readonly FrozenDictionary<string, Func<Analyze.Scope, BridgeCheckResult>> Catalog =
        new Dictionary<string, Func<Analyze.Scope, BridgeCheckResult>>(StringComparer.Ordinal) {
            ["analysis-core"] = AnalysisCore,
            ["faces"] = Faces,
            ["curves"] = Curves,
            ["meshes"] = Meshes,
            ["intersections"] = Intersections,
            ["radyab-assembly"] = RadyabAssembly,
        }.ToFrozenDictionary(StringComparer.Ordinal);
    internal static BridgeReply Doctor() =>
            BridgeReply.DoctorOk(doctor: new BridgeDoctor(
            RhinoName: RhinoApp.Name,
            RhinoVersion: RhinoApp.Version.ToString(),
            RhinoPid: Environment.ProcessId,
            ActiveDocument: RhinoDoc.ActiveDoc is not null,
            ContextStatus: ContextProbe().Status,
            ContextFault: ContextProbe().Fault,
            Assemblies: Assemblies()));
    internal static BridgeReply Check(BridgeRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<string> ids = request.Checks.Count == 0 ? AllChecks : request.Checks;
        List<BridgeCheckResult> results = new(capacity: ids.Count);
        if (RhinoDoc.ActiveDoc is not RhinoDoc doc) {
            BridgeFault fault = BridgeFault.MessageOnly("missing-document", "No active Rhino document is open; check jobs need model tolerances from RhinoDoc.");
            foreach (string id in ids) {
                results.Add(BridgeCheckResult.Skipped(id: id, fault: fault));
            }
            return BridgeReply.CheckOk(jobName: request.JobName, checks: results);
        }
        Analyze.Scope scope = Analyze.From(doc: doc);
        foreach (string id in ids) {
            results.Add(Timed(id: id, scope: scope));
        }
        return BridgeReply.CheckOk(jobName: request.JobName, checks: results);
    }
    private static BridgeCheckResult Timed(string id, Analyze.Scope scope) {
        Stopwatch timer = Stopwatch.StartNew();
        BridgeCheckResult result = Catalog.TryGetValue(id, out Func<Analyze.Scope, BridgeCheckResult>? run)
            ? run(scope)
            : BridgeCheckResult.Unsupported(id: id);
        timer.Stop();
        return result with { DurationMs = (int)timer.ElapsedMilliseconds };
    }
    private static (string Status, BridgeFault? Fault) ContextProbe() {
        if (RhinoDoc.ActiveDoc is not RhinoDoc doc) {
            return (BridgeProtocol.Skipped, BridgeFault.MessageOnly("missing-document", "No active Rhino document is open."));
        }
        Fin<Context> context = Analyze.From(doc: doc).Context;
        return context.Match(
            Succ: static _ => (BridgeProtocol.Ok, (BridgeFault?)null),
            Fail: static error => (BridgeProtocol.Failed, BridgeFault.Of(error)));
    }
    private static IReadOnlyList<AssemblyReport> Assemblies() =>
        [
            AssemblyReport.Loaded("RhinoCommon", typeof(RhinoApp).Assembly),
            AssemblyReport.Loaded("Grasshopper2", typeof(ModularComponent).Assembly),
            AssemblyReport.Loaded("GrasshopperIO", typeof(GrasshopperIO.IReader).Assembly),
            AssemblyReport.Loaded("Rasm", typeof(Analyze).Assembly),
            AssemblyReport.Loaded("Rasm.Grasshopper", typeof(Rasm.Grasshopper.Plugin).Assembly),
            AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(static assembly => string.Equals(assembly.GetName().Name, "Radyab", StringComparison.Ordinal)) switch {
                Assembly assembly => AssemblyReport.Loaded("Radyab", assembly),
                _ => AssemblyReport.Missing("Radyab"),
            },
        ];
    private static BridgeCheckResult AnalysisCore(Analyze.Scope scope) {
        using Curve line = new LineCurve(from: new Point3d(0.0, 0.0, 0.0), to: new Point3d(10.0, 0.0, 0.0));
        using Brep brep = Brep.CreateFromBox(new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(1.0, 1.0, 1.0)));
        return Aggregate(
            id: "analysis-core",
            Run(scope: scope, operation: Analyze.Bounds<GeometryBase, BoundingBox>(aspect: Bounds.AxisAligned), input: brep),
            Run(scope: scope, operation: Analyze.Topologies<GeometryBase, Kind>(aspect: Topologies.Kind), input: brep),
            Run(scope: scope, operation: Analyze.Measure<Curve, double>(aspect: Measure.Length), input: line),
            Run(scope: scope, operation: Analyze.Location<Curve, Point3d>(aspect: Location.Closest(point: new Point3d(5.0, 1.0, 0.0))), input: line));
    }
    private static BridgeCheckResult Faces(Analyze.Scope scope) {
        using Brep brep = Brep.CreateFromBox(new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(1.0, 1.0, 1.0)));
        return Aggregate(
            id: "faces",
            Run(scope: scope, operation: Analyze.Faces<Brep, int>(aspect: Analysis.Faces.All), input: brep),
            Run(scope: scope, operation: Analyze.Faces<Brep, int>(aspect: Analysis.Faces.Top()), input: brep),
            Run(scope: scope, operation: Analyze.Faces<Brep, int>(aspect: Analysis.Faces.Bottom()), input: brep),
            Run(scope: scope, operation: Analyze.Faces<Brep, int>(aspect: Analysis.Faces.At(index: 0)), input: brep));
    }
    private static BridgeCheckResult Curves(Analyze.Scope scope) {
        using Curve line = new PolylineCurve([new Point3d(0.0, 0.0, 0.0), new Point3d(2.0, 0.0, 0.0), new Point3d(2.0, 1.0, 0.0)]);
        using Brep brep = Brep.CreateFromBox(new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(1.0, 1.0, 1.0)));
        return Aggregate(
            id: "curves",
            Run(scope: scope, operation: Analyze.Curves<Curve, CurveFeature>(aspect: Analysis.Curves.Segments()), input: line),
            Run(scope: scope, operation: Analyze.Curves<Brep, CurveFeature>(aspect: Analysis.Curves.Iso(IsoStatus.X, normalized: 0.5)), input: brep),
            Run(scope: scope, operation: Analyze.Curves<Curve, CurveForm>(aspect: Analysis.Curves.Form()), input: line));
    }
    private static BridgeCheckResult Meshes(Analyze.Scope scope) {
        using Mesh mesh = Mesh.CreateFromBox(new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(1.0, 1.0, 1.0)), xCount: 1, yCount: 1, zCount: 1);
        return Aggregate(
            id: "meshes",
            Run(scope: scope, operation: Analyze.Meshes<Mesh, MeshSample>(aspect: Analysis.Meshes.Validity), input: mesh),
            Run(scope: scope, operation: Analyze.Meshes<Mesh, MeshSample>(aspect: Analysis.Meshes.Counts), input: mesh),
            Run(scope: scope, operation: Analyze.Meshes<Mesh, Polyline>(aspect: Analysis.Meshes.NakedEdges), input: mesh),
            Run(scope: scope, operation: Analyze.Meshes<Mesh, Stat>(aspect: Analysis.Meshes.FaceQualitySummary()), input: mesh));
    }
    private static BridgeCheckResult Intersections(Analyze.Scope scope) {
        using Curve line = new LineCurve(from: new Point3d(-1.0, 0.5, 0.5), to: new Point3d(2.0, 0.5, 0.5));
        using Curve crossing = new LineCurve(from: new Point3d(0.5, -1.0, 0.5), to: new Point3d(0.5, 2.0, 0.5));
        using Brep brep = Brep.CreateFromBox(new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(1.0, 1.0, 1.0)));
        using Brep other = Brep.CreateFromBox(new BoundingBox(min: new Point3d(0.5, 0.5, 0.5), max: new Point3d(1.5, 1.5, 1.5)));
        using Mesh mesh = Mesh.CreateFromBox(new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(1.0, 1.0, 1.0)), xCount: 1, yCount: 1, zCount: 1);
        return Aggregate(
            id: "intersections",
            Run(scope: scope, operation: Analyze.Intersect<Curve, Curve, IntersectionKind>(), input: (A: line, B: crossing)),
            Run(scope: scope, operation: Analyze.Intersect<Curve, Brep, IntersectionKind>(), input: (A: line, B: brep)),
            Run(scope: scope, operation: Analyze.Intersect<Mesh, Plane, IntersectionKind>(), input: (A: mesh, B: new Plane(origin: new Point3d(0.0, 0.0, 0.5), normal: Vector3d.ZAxis))),
            Run(scope: scope, operation: Analyze.Intersect<Brep, Brep, IntersectionKind>(), input: (A: brep, B: other)));
    }
    private static BridgeCheckResult RadyabAssembly(Analyze.Scope scope) {
        _ = scope;
        return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(static assembly => string.Equals(assembly.GetName().Name, "Radyab", StringComparison.Ordinal)) switch {
            Assembly assembly => BridgeCheckResult.Ok(id: "radyab-assembly", count: assembly.GetTypes().Length),
            _ => BridgeCheckResult.Skipped(id: "radyab-assembly", fault: BridgeFault.MessageOnly("assembly", "Radyab assembly is not loaded in this Rhino session.")),
        };
    }
    private static BridgeCheckResult Aggregate(string id, params BridgeFault?[] faults) =>
        Array.Find(faults, static fault => fault is not null) switch {
            BridgeFault fault => BridgeCheckResult.Failed(id: id, fault: fault),
            _ => BridgeCheckResult.Ok(id: id, count: faults.Length),
        };
    private static BridgeFault? Run<TGeometry, TOut>(Analyze.Scope scope, Operation<TGeometry, TOut> operation, TGeometry input) where TGeometry : notnull =>
        scope.Run(operation: operation, input: input).ToFin().Match(
            Succ: static _ => (BridgeFault?)null,
            Fail: static error => BridgeFault.Of(error));
}
