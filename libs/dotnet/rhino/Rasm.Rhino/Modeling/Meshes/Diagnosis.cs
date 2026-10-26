using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.FileIO;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshDefect {
    public sealed record ZeroArea(Seq<int> Whole, Seq<int> Partial) : MeshDefect;
    public sealed record Naked(Seq<Polyline> Loops) : MeshDefect;
    public sealed record SelfIntersecting(Seq<IndexPair> Pairs, Seq<Polyline> Perforations, Seq<Polyline> Overlaps) : MeshDefect;
    public sealed record Disjoint(Seq<Mesh> Pieces) : MeshDefect;

    public Seq<MeshEdit> Repair(Tolerances tolerances) =>
        Switch(
            tolerances,
            zeroArea: static (_, _) => Seq<MeshEdit>(new MeshEdit.RemoveZeroAreaFaces()),
            naked: static (held, _) => Seq<MeshEdit>(new MeshEdit.HealNakedEdges(held.Absolute), new MeshEdit.FillHoles()),
            selfIntersecting: static (_, _) => Seq<MeshEdit>(),
            disjoint: static (_, _) => Seq<MeshEdit>());
}

public sealed record MeshDiagnosis(bool Valid, MeshCheckParameters Counts, string Log, Seq<MeshDefect> Located) {
    public static IO<MeshDiagnosis> Diagnose(Tolerances tolerances, Mesh mesh, MeshCheckParameters checks, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        from report in DisposalOps.Within(static () => new TextLog(), log => IO.lift(() => {
            MeshCheckParameters counts = checks;
            bool valid = mesh.Check(log, ref counts);
            return (Valid: valid, Counts: counts, Log: log.ToString());
        }))
        from located in Seq(
                (Counted: report.Counts.DegenerateFaceCount > 0, Locate: IO.lift(MeshDefect () => {
                    _ = mesh.Faces.GetZeroAreaFaces(out int[] whole, out int[] partial);
                    return new MeshDefect.ZeroArea(toSeq(whole), toSeq(partial));
                })),
                (Counted: report.Counts.NakedEdgeCount > 0, Locate: IO.lift(MeshDefect () => new MeshDefect.Naked(Conversions.Rows(mesh.GetNakedEdges())))),
                (Counted: report.Counts.SelfIntersectingPairsCount > 0, Locate: Copies.Acquire(nameof(Mesh.GetSelfIntersections), token => {
                    using TextLog log = new();
                    bool accepted = mesh.GetSelfIntersections(tolerances.MeshIntersection, out Polyline[] perforations, overlapsPolylines: true, out Polyline[] overlaps,
                        overlapsMesh: false, out Mesh _, log, token, progress.ValueUnsafe());
                    return Answer.Accepted(accepted, log.ToString()).Map(MeshDefect (_) => new MeshDefect.SelfIntersecting(
                        toSeq(mesh.Faces.GetClashingFacePairs(report.Counts.SelfIntersectingPairsCount)), Conversions.Rows(perforations), Conversions.Rows(overlaps)));
                }, retain)),
                (Counted: report.Counts.DisjointMeshCount > 1, Locate: Copies.Acquire(nameof(Mesh.SplitDisjointPieces), _ =>
                    Answer.Many(mesh.SplitDisjointPieces()).Map(static MeshDefect (pieces) => new MeshDefect.Disjoint(pieces)), retain)))
            .Filter(static kind => kind.Counted)
            .TraverseM(static kind => kind.Locate).As()
        select new MeshDiagnosis(report.Valid, report.Counts, report.Log, located);
}
