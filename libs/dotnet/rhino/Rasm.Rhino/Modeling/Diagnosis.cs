using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;
using Rasm.Rhino.Modeling.Meshes;
using Rhino.FileIO;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DefectKind {
    public static readonly DefectKind Invalid = new("invalid geometry");
    public static readonly DefectKind ZeroAreaFaces = new("zero-area faces");
    public static readonly DefectKind NakedEdges = new("naked edges");
    public static readonly DefectKind NonManifoldEdges = new("non-manifold edges");
    public static readonly DefectKind ShortEdges = new("extremely short edges");
    public static readonly DefectKind BadNormals = new("bad normals");
    public static readonly DefectKind DuplicateFaces = new("duplicate faces");
    public static readonly DefectKind RandomNormals = new("randomly oriented face normals");
    public static readonly DefectKind DisjointPieces = new("disjoint pieces");
    public static readonly DefectKind UnusedVertices = new("unused vertices");
    public static readonly DefectKind SelfIntersections = new("self-intersecting faces");
    public static readonly DefectKind InvalidNgons = new("invalid ngons");
    public static readonly DefectKind InwardSolid = new("solid oriented inward");
    public static readonly DefectKind ShortCurve = new("curve shorter than tolerance");
    public static readonly DefectKind OpenCurve = new("closable open curve");
    public static readonly DefectKind SeamKink = new("kink at the closed seam");

    public string Message { get; }
}

public sealed record Defect<T>(DefectKind Kind, int Count, Seq<ComponentIndex> At, Seq<Step<T>> Repair) where T : GeometryBase;

public sealed record Diagnosis<T>(bool Valid, string Log, Seq<Defect<T>> Defects) where T : GeometryBase {
    public Seq<Step<T>> Repair => Defects.Bind(static defect => defect.Repair);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Diagnostics {
    // --- [DIAGNOSE]
    extension<T>(T geometry) where T : GeometryBase {
        public IO<Diagnosis<T>> Diagnose(Tolerances tolerances) =>
            from validity in IO.lift(() => (Valid: geometry.IsValidWithLog(out string log), Log: log))
            from found in geometry switch {
                Mesh mesh => MeshDefects<T>(tolerances, mesh),
                Brep brep => IO.lift(() => BrepDefects<T>(tolerances, brep)),
                Curve curve => IO.lift(() => CurveDefects<T>(tolerances, curve)),
                _ => IO.pure((Log: "", Defects: Seq<Defect<T>>())),
            }
            select new Diagnosis<T>(validity.Valid, string.Join(Environment.NewLine, Seq(validity.Log, found.Log).Filter(static text => text.Length > 0)), validity.Valid
                ? found.Defects
                : Seq(Located<T, GeometryBase>(DefectKind.Invalid, 1, [], geometry switch {
                    Brep => [Lifted<GeometryBase, Brep>(Steps.Of<Brep>(brep => brep.Repair(tolerances.Absolute)))],
                    Curve => [Lifted<GeometryBase, Curve>(Steps.Of<Curve>(curve => curve.Repair(tolerances.Absolute)))],
                    _ => Seq<Step<GeometryBase>>(),
                })) + found.Defects);
    }

    // --- [MESHES]
    private static IO<(string Log, Seq<Defect<T>> Defects)> MeshDefects<T>(Tolerances tolerances, Mesh mesh) where T : GeometryBase =>
        from checkedMesh in IO.lift(() => {
            using TextLog log = new();
            MeshCheckParameters counts = MeshCheckParameters.Defaults();
            _ = mesh.Check(log, ref counts);
            return (Log: log.ToString(), Counts: counts);
        })
        from manifold in MeshManifold.Of(mesh)
        let counts = checkedMesh.Counts
        select (checkedMesh.Log, Seq(
                (Count: counts.DegenerateFaceCount, Locate: (Func<Defect<T>>)(() => {
                    _ = mesh.Faces.GetZeroAreaFaces(out int[] whole, out int[] partial);
                    return Located<T, Mesh>(DefectKind.ZeroAreaFaces, counts.DegenerateFaceCount, Components(ComponentIndexType.MeshFace, toSeq(whole) + toSeq(partial)), [Steps.Count<Mesh>(static held => {
                        int repaired = 0;
                        return held.Faces.RemoveZeroAreaFaces(ref repaired) + repaired;
                    }, static count => count >= 0)]);
                })),
                (Count: counts.NakedEdgeCount, Locate: () => Located<T, Mesh>(DefectKind.NakedEdges, counts.NakedEdgeCount, Edges(mesh, static faces => faces is 1), [Steps.Of<Mesh>(static held => held.FillHoles())])),
                (Count: counts.NonManifoldEdgeCount, Locate: () => Located<T, Mesh>(DefectKind.NonManifoldEdges, counts.NonManifoldEdgeCount, Edges(mesh, static faces => faces > 2), [])),
                (Count: counts.ExtremelyShortEdgeCount, Locate: () => Located<T, Mesh>(DefectKind.ShortEdges, counts.ExtremelyShortEdgeCount, [],
                    [Steps.Count<Mesh>(held => held.CollapseFacesByEdgeLength(bGreaterThan: false, tolerances.Absolute), static count => count >= 0)])),
                (Count: counts.NonUnitVectorNormalCount + counts.ZeroLengthNormalCount + counts.VertexFaceNormalsDifferCount, Locate: () => Located<T, Mesh>(
                    DefectKind.BadNormals, counts.NonUnitVectorNormalCount + counts.ZeroLengthNormalCount + counts.VertexFaceNormalsDifferCount, [],
                    [Steps.Of<Mesh>(static held => held.FaceNormals.ComputeFaceNormals()), Steps.Of<Mesh>(static held => held.Normals.ComputeNormals())])),
                (Count: counts.DuplicateFaceCount, Locate: () => Located<T, Mesh>(DefectKind.DuplicateFaces, counts.DuplicateFaceCount, Components(ComponentIndexType.MeshFace, toSeq(mesh.Faces.GetDuplicateFaces())),
                    [Steps.Of<Mesh>(static held => {
                        using Mesh? removed = held.Faces.ExtractDuplicateFaces();
                        return removed is not null;
                    })])),
                (Count: counts.RandomFaceNormalCount, Locate: () => Located<T, Mesh>(DefectKind.RandomNormals, counts.RandomFaceNormalCount, [],
                    [Steps.Count<Mesh>(static held => held.UnifyNormals(), static count => count >= 0)])),
                (Count: counts.DisjointMeshCount - 1, Locate: () => Located<T, Mesh>(DefectKind.DisjointPieces, counts.DisjointMeshCount, [], [])),
                (Count: counts.UnusedVertexCount, Locate: () => Located<T, Mesh>(DefectKind.UnusedVertices, counts.UnusedVertexCount, [],
                    [Steps.Count<Mesh>(static held => held.Vertices.CullUnused(), static count => count >= 0)])),
                (Count: counts.SelfIntersectingPairsCount, Locate: () => Located<T, Mesh>(DefectKind.SelfIntersections, counts.SelfIntersectingPairsCount,
                    Components(ComponentIndexType.MeshFace, toSeq(mesh.Faces.GetClashingFacePairs(counts.SelfIntersectingPairsCount)).Bind(static pair => Seq(pair.I, pair.J)).Distinct()), [])),
                (Count: counts.InvalidNgonCount, Locate: () => Located<T, Mesh>(DefectKind.InvalidNgons, counts.InvalidNgonCount, Components(ComponentIndexType.MeshNgon, Ngons(mesh)),
                    [Steps.Of<Mesh>(static held => Ngons(held) switch { var invalid => held.Ngons.RemoveNgons(invalid) == invalid.Count })])),
                (Count: manifold is MeshManifold.Solid { Outward: false } ? 1 : 0, Locate: static () => Located<T, Mesh>(DefectKind.InwardSolid, 1, [],
                    [Steps.Of<Mesh>(static held => {
                        held.Flip(vertexNormals: true, faceNormals: true, faceOrientation: true);
                        return true;
                    })])))
            .Filter(static row => row.Count > 0)
            .Map(static row => row.Locate())
            .Strict());

    private static Seq<ComponentIndex> Edges(Mesh mesh, Func<int, bool> faces) =>
        Components(ComponentIndexType.MeshTopologyEdge, toSeq(Range(0, mesh.TopologyEdges.Count)).Filter(edge => faces(mesh.TopologyEdges.GetConnectedFaces(edge).Length)));

    private static Seq<int> Ngons(Mesh mesh) => toSeq(Range(0, mesh.Ngons.Count)).Filter(ngon => mesh.Ngons.IsValid(ngon) is 0u).Strict();

    // --- [BREPS]
    private static (string Log, Seq<Defect<T>> Defects) BrepDefects<T>(Tolerances tolerances, Brep brep) where T : GeometryBase =>
        (Logs: Seq(brep.IsValidTopology(out string topology) ? "" : topology, brep.IsValidGeometry(out string shape) ? "" : shape, brep.IsValidTolerancesAndFlags(out string flags) ? "" : flags),
            Edges: toSeq(brep.Edges).Map(static edge => (Index: edge.EdgeIndex, edge.Valence)).Strict()) switch {
                var read => (string.Join(Environment.NewLine, read.Logs.Filter(static text => text.Length > 0)), Seq(
                        Located<T, Brep>(DefectKind.NakedEdges, read.Edges.Count(static edge => edge.Valence is EdgeAdjacency.Naked),
                            Components(ComponentIndexType.BrepEdge, read.Edges.Filter(static edge => edge.Valence is EdgeAdjacency.Naked).Map(static edge => edge.Index)),
                            [Steps.Count<Brep>(held => held.JoinNakedEdges(tolerances.Absolute), static count => count >= 0)]),
                        Located<T, Brep>(DefectKind.NonManifoldEdges, read.Edges.Count(static edge => edge.Valence is EdgeAdjacency.NonManifold),
                            Components(ComponentIndexType.BrepEdge, read.Edges.Filter(static edge => edge.Valence is EdgeAdjacency.NonManifold).Map(static edge => edge.Index)), []),
                        Located<T, Brep>(DefectKind.InwardSolid, brep.SolidOrientation is BrepSolidOrientation.Inward ? 1 : 0, [], [Steps.Of<Brep>(static held => {
                        held.Flip();
                        return true;
                    })]))
                    .Filter(static defect => defect.Count > 0)),
            };

    // --- [CURVES]
    private static (string Log, Seq<Defect<T>> Defects) CurveDefects<T>(Tolerances tolerances, Curve curve) where T : GeometryBase =>
        ("", Seq(
                Located<T, Curve>(DefectKind.ShortCurve, curve.IsShort(tolerances.Absolute) ? 1 : 0, [], []),
                Located<T, Curve>(DefectKind.OpenCurve, !curve.IsClosed && curve.IsClosable(tolerances.Absolute) ? 1 : 0, [], [Steps.Of<Curve>(held => held.MakeClosed(tolerances.Absolute))]),
                Located<T, Curve>(DefectKind.SeamKink, curve.IsClosed && CurveEvaluation.Kinks(tolerances, curve, Continuity.G1_continuous)
                    .Exists(t => t >= curve.Domain.T1) ? 1 : 0, [], []))
            .Filter(static defect => defect.Count > 0));

    // --- [STEPS]
    private static Defect<T> Located<T, TKind>(DefectKind kind, int count, Seq<ComponentIndex> at, Seq<Step<TKind>> repair) where T : GeometryBase where TKind : GeometryBase =>
        new(kind, count, at, repair.Map(Lifted<T, TKind>));

    private static Step<T> Lifted<T, TKind>(Step<TKind> step) where T : GeometryBase where TKind : GeometryBase =>
        new(target => step.Run((TKind)(GeometryBase)target).Map(static result => (T)(GeometryBase)result), step.Member);

    private static Seq<ComponentIndex> Components(ComponentIndexType kind, Seq<int> indices) => indices.Map(index => new ComponentIndex(kind, index)).Strict();
}
