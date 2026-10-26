using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record BrepCut {
    public sealed record Brep(global::Rhino.Geometry.Brep Cutter) : BrepCut;
    public sealed record Breps(IterableNE<global::Rhino.Geometry.Brep> Cutters) : BrepCut;
    public sealed record Curves(IterableNE<Curve> Cutters) : BrepCut;
    public sealed record Projected(IterableNE<GeometryBase> Cutters, Vector3d Normal, bool PlanView) : BrepCut;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ConnectAt {
    public sealed record Edges(int Edge0, int Edge1) : ConnectAt;
    public sealed record Picks(Point3d Pick0, Point3d Pick1) : ConnectAt;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record WireCutter {
    public sealed record Closed(IterableNE<Curve> Curves, Option<(double Depth, bool BothSides)> Depth) : WireCutter;
    public sealed record Open(Curve Curve, Vector3d TrimDirection, bool BothSides, Option<(double Depth, double TrimDepth, bool TrimBothSides)> Depth) : WireCutter;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record EdgeReplacement {
    public sealed record Line : EdgeReplacement;
    public sealed record ExtendSideEdges : EdgeReplacement;
    public sealed record Along(Curve Curve) : EdgeReplacement;

    internal (BrepReplaceEdgeMethod Method, Curve? Curve) Arguments => Switch(
        line: static _ => (BrepReplaceEdgeMethod.Line, null),
        extendSideEdges: static _ => (BrepReplaceEdgeMethod.ExtendSideEdges, (Curve?)null),
        along: static along => (BrepReplaceEdgeMethod.Curve, (Curve?)along.Curve));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SolidEdit {
    public sealed record AddFace(Surface Surface) : SolidEdit;
    public sealed record AddRuledFace(int EdgeA, bool RevEdgeA, int EdgeB, bool RevEdgeB) : SolidEdit;
    public sealed record AddConeFace(int Vertex, int Edge, bool RevEdge) : SolidEdit;
    public sealed record Append(Brep Other) : SolidEdit;
    public sealed record Join(Brep Other) : SolidEdit;
    public sealed record JoinNakedEdges : SolidEdit;
    public sealed record JoinEdges(Seq<(int Edge0, int Edge1)> Pairs) : SolidEdit;
    public sealed record MergeCoplanarFaces : SolidEdit;
    public sealed record MergeCoplanarFace(int Face) : SolidEdit;
    public sealed record MergeCoplanarFacePair(int Face0, int Face1) : SolidEdit;
    public sealed record MergeAllEdges : SolidEdit;
    public sealed record MergeEdge(int Edge) : SolidEdit;
    public sealed record RemoveFace(int Face) : SolidEdit;
    public sealed record RemoveNakedMicroEdges : SolidEdit;
    public sealed record RemoveFins : SolidEdit;
    public sealed record RemoveSlits : SolidEdit;
    public sealed record ShrinkFaces : SolidEdit;
    public sealed record ShrinkFace(int Face, BrepFace.ShrinkDisableSide DisableSide) : SolidEdit;
    public sealed record ShrinkSurfaceToEdge(int Face) : SolidEdit;
    public sealed record SplitKinkyFaces : SolidEdit;
    public sealed record SplitKinkyFace(int Face) : SolidEdit;
    public sealed record SplitKinkyEdge(int Edge) : SolidEdit;
    public sealed record SplitEdgeAtParameters(int Edge, IterableNE<double> Parameters) : SolidEdit;
    public sealed record SplitClosedFaces(int MinimumDegree) : SolidEdit;
    public sealed record SplitBipolarFaces : SolidEdit;
    public sealed record SplitFacesAtTangents(bool AggressiveMode) : SolidEdit;
    public sealed record SplitFaceAtTangents(int Face, bool AggressiveMode) : SolidEdit;
    public sealed record StandardizeFaceSurfaces : SolidEdit;
    public sealed record StandardizeFaceSurface(int Face) : SolidEdit;
    public sealed record Standardize : SolidEdit;
    public sealed record RebuildEdges(int Face, bool RebuildSharedEdges, bool RebuildVertices) : SolidEdit;
    public sealed record ReplaceSurface(int Face, Surface Surface, bool RebuildSharedEdges, bool RebuildVertices) : SolidEdit;
    public sealed record MatchTrimEnds : SolidEdit;
    public sealed record MatchLoopTrimEnds(int Loop) : SolidEdit;
    public sealed record Flip(bool OnlyReversedFaces) : SolidEdit;
    public sealed record Repair : SolidEdit;
    public sealed record PrepareForBooleans : SolidEdit;
    public sealed record TransformComponent(Seq<ComponentIndex> Components, Transform Transform, Option<Duration> TimeLimit, bool UseMultipleThreads) : SolidEdit;
    public sealed record SplitFace(int Face, IterableNE<Curve> Curves) : SolidEdit;
    public sealed record RemoveAllHoles : SolidEdit;
    public sealed record RemoveHoles(Set<int> Loops) : SolidEdit;
    public sealed record CapPlanarHoles : SolidEdit;
    public sealed record UntrimFace(int Face) : SolidEdit;
    public sealed record UntrimOuterLoop(int Face) : SolidEdit;
    public sealed record InsetFaces(Set<int> Faces, double Distance, bool Loose, bool IgnoreSeams, bool CreaseCorners) : SolidEdit;
    public sealed record PushPullExtend(int Face, Transform Transform) : SolidEdit;
    public sealed record ReplaceEdge(Set<int> Edges, EdgeReplacement Replacement) : SolidEdit;
    public sealed record DuplicateSubBrep(Set<int> Faces) : SolidEdit;

    public IO<EditStep<Brep, int>> Step(Tolerances tolerances, Brep brep, Action<IDisposable> retain) =>
        Switch(
            (Brep: brep, Tolerances: tolerances, Retain: retain),
            addFace: static (at, of) => Copies.Acquire(nameof(BrepFaceList.Add), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.Add(of.Surface) is not null), at.Retain),
            addRuledFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.EdgeA, of.EdgeB)))
                >> Copies.Acquire(nameof(BrepFaceList.AddRuledFace), _ => Edits.Accepted<Brep, int>(at.Brep,
                    at.Brep.Faces.AddRuledFace(at.Brep.Edges[of.EdgeA], of.RevEdgeA, at.Brep.Edges[of.EdgeB], of.RevEdgeB) is not null), at.Retain),
            addConeFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, Seq(new ComponentIndex(ComponentIndexType.BrepVertex, of.Vertex), new ComponentIndex(ComponentIndexType.BrepEdge, of.Edge))))
                >> Copies.Acquire(nameof(BrepFaceList.AddConeFace), _ => Edits.Accepted<Brep, int>(at.Brep,
                    at.Brep.Faces.AddConeFace(at.Brep.Vertices[of.Vertex], at.Brep.Edges[of.Edge], of.RevEdge) is not null), at.Retain),
            append: static (at, of) => Copies.Acquire(nameof(Brep.Append), _ => {
                at.Brep.Append(of.Other);
                return Edits.Completed<Brep, int>(at.Brep);
            }, at.Retain),
            join: static (at, of) => Copies.Acquire(nameof(Brep.Join), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Join(of.Other, at.Tolerances.Absolute, compact: true)), at.Retain),
            joinNakedEdges: static (at, _) => Copies.Acquire(nameof(Brep.JoinNakedEdges), _ => Edits.Count(at.Brep, at.Brep.JoinNakedEdges(at.Tolerances.Absolute)), at.Retain),
            joinEdges: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, of.Pairs.Bind(static pair => Seq(pair.Edge0, pair.Edge1))))
                >> IO.lift(() => Callbacks.Each(of.Pairs, pair => at.Brep.JoinEdges(pair.Edge0, pair.Edge1, at.Tolerances.Absolute, compact: false), nameof(Brep.JoinEdges)))
                >> Copies.Acquire(nameof(Brep.Compact), _ => {
                    at.Brep.Compact();
                    return Edits.Completed<Brep, int>(at.Brep);
                }, at.Retain),
            mergeCoplanarFaces: static (at, _) => Copies.Acquire(nameof(Brep.MergeCoplanarFaces), _ =>
                Edits.Changed<Brep, int>(at.Brep, at.Brep.MergeCoplanarFaces(at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            mergeCoplanarFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(Brep.MergeCoplanarFaces), _ => Edits.Changed<Brep, int>(at.Brep, at.Brep.MergeCoplanarFaces(of.Face, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            mergeCoplanarFacePair: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face0, of.Face1)))
                >> Copies.Acquire(nameof(Brep.MergeCoplanarFaces), _ => Edits.Changed<Brep, int>(at.Brep, at.Brep.MergeCoplanarFaces(of.Face0, of.Face1, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            mergeAllEdges: static (at, _) => Copies.Acquire(nameof(BrepEdgeList.MergeAllEdges), _ => Edits.Count(at.Brep, at.Brep.Edges.MergeAllEdges(at.Tolerances.Angle)), at.Retain),
            mergeEdge: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.Edge)))
                >> Copies.Acquire(nameof(BrepEdgeList.MergeEdge), _ => Edits.Count(at.Brep, at.Brep.Edges.MergeEdge(of.Edge, at.Tolerances.Angle)), at.Retain),
            removeFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFaceList.RemoveAt), _ => {
                    at.Brep.Faces.RemoveAt(of.Face);
                    return Edits.Completed<Brep, int>(at.Brep);
                }, at.Retain),
            removeNakedMicroEdges: static (at, _) => Copies.Acquire(nameof(BrepEdgeList.RemoveNakedMicroEdges), _ =>
                Edits.Count(at.Brep, at.Brep.Edges.RemoveNakedMicroEdges(at.Tolerances.Absolute, cleanUp: true)), at.Retain),
            removeFins: static (at, _) => Copies.Acquire(nameof(Brep.RemoveFins), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.RemoveFins()), at.Retain),
            removeSlits: static (at, _) => Copies.Acquire(nameof(BrepFaceList.RemoveSlits), _ => Edits.Changed<Brep, int>(at.Brep, at.Brep.Faces.RemoveSlits()), at.Retain),
            shrinkFaces: static (at, _) => Copies.Acquire(nameof(BrepFaceList.ShrinkFaces), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.ShrinkFaces()), at.Retain),
            shrinkFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFace.ShrinkFace), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces[of.Face].ShrinkFace(of.DisableSide)), at.Retain),
            shrinkSurfaceToEdge: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFace.ShrinkSurfaceToEdge), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces[of.Face].ShrinkSurfaceToEdge()), at.Retain),
            splitKinkyFaces: static (at, _) => Copies.Acquire(nameof(BrepFaceList.SplitKinkyFaces), _ =>
                Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitKinkyFaces(at.Tolerances.Angle, compact: false)), at.Retain),
            splitKinkyFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFaceList.SplitKinkyFace), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitKinkyFace(of.Face, at.Tolerances.Angle)), at.Retain),
            splitKinkyEdge: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.Edge)))
                >> Copies.Acquire(nameof(BrepEdgeList.SplitKinkyEdge), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Edges.SplitKinkyEdge(of.Edge, at.Tolerances.Angle)), at.Retain),
            splitEdgeAtParameters: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.Edge)))
                >> Copies.Acquire(nameof(BrepEdgeList.SplitEdgeAtParameters), _ => at.Brep.Edges.SplitEdgeAtParameters(of.Edge, of.Parameters) switch {
                    var split => (Answer.Accepted(split > 0), Edits.Count(at.Brep, split)).Apply(static (_, step) => step).As(),
                }, at.Retain),
            splitClosedFaces: static (at, of) => Copies.Acquire(nameof(BrepFaceList.SplitClosedFaces), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitClosedFaces(of.MinimumDegree)), at.Retain),
            splitBipolarFaces: static (at, _) => Copies.Acquire(nameof(BrepFaceList.SplitBipolarFaces), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitBipolarFaces()), at.Retain),
            splitFacesAtTangents: static (at, of) => Copies.Acquire(nameof(BrepFaceList.SplitFacesAtTangents), _ =>
                Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitFacesAtTangents(of.AggressiveMode)), at.Retain),
            splitFaceAtTangents: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFaceList.SplitFaceAtTangents), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitFaceAtTangents(of.Face, of.AggressiveMode)), at.Retain),
            standardizeFaceSurfaces: static (at, _) => Copies.Acquire(nameof(BrepFaceList.StandardizeFaceSurfaces), _ => {
                at.Brep.Faces.StandardizeFaceSurfaces();
                return Edits.Completed<Brep, int>(at.Brep);
            }, at.Retain),
            standardizeFaceSurface: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFaceList.StandardizeFaceSurface), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.StandardizeFaceSurface(of.Face)), at.Retain),
            standardize: static (at, _) => Copies.Acquire(nameof(Brep.Standardize), _ => {
                at.Brep.Standardize();
                return Edits.Completed<Brep, int>(at.Brep);
            }, at.Retain),
            rebuildEdges: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFace.RebuildEdges), _ => Edits.Accepted<Brep, int>(at.Brep,
                    at.Brep.Faces[of.Face].RebuildEdges(at.Tolerances.Absolute, of.RebuildSharedEdges, of.RebuildVertices)), at.Retain),
            replaceSurface: static (at, of) =>
                from face in IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                from surface in Copies.Acquire(nameof(Brep.AddSurface), _ => at.Brep.AddSurface(of.Surface) switch {
                    var index => Answer.Accepted(index >= 0).Map(_ => index),
                }, at.Retain)
                from changed in Copies.Acquire(nameof(BrepFace.ChangeSurface), _ => Answer.Accepted(at.Brep.Faces[of.Face].ChangeSurface(surface)), at.Retain)
                from rebuilt in Copies.Acquire(nameof(BrepFace.RebuildEdges), _ => Edits.Accepted<Brep, int>(at.Brep,
                    at.Brep.Faces[of.Face].RebuildEdges(at.Tolerances.Absolute, of.RebuildSharedEdges, of.RebuildVertices)), at.Retain)
                select rebuilt,
            matchTrimEnds: static (at, _) => Copies.Acquire(nameof(BrepTrimList.MatchEnds), _ => Edits.Changed<Brep, int>(at.Brep, at.Brep.Trims.MatchEnds()), at.Retain),
            matchLoopTrimEnds: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepLoop, Seq(of.Loop)))
                >> Copies.Acquire(nameof(BrepTrimList.MatchEnds), _ => Edits.Changed<Brep, int>(at.Brep, at.Brep.Trims.MatchEnds(at.Brep.Loops[of.Loop])), at.Retain),
            flip: static (at, of) => Copies.Acquire(nameof(BrepFaceList.Flip), _ => {
                at.Brep.Faces.Flip(of.OnlyReversedFaces);
                return Edits.Completed<Brep, int>(at.Brep);
            }, at.Retain),
            repair: static (at, _) => Copies.Acquire(nameof(Brep.Repair), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Repair(at.Tolerances.Absolute)), at.Retain),
            prepareForBooleans: static (at, _) => Copies.Acquire(nameof(Brep.PrepareForBooleans), _ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.PrepareForBooleans()), at.Retain),
            transformComponent: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, of.Components))
                    .Bind(components => Copies.Acquire(nameof(Brep.TransformComponent), _ => Edits.Accepted<Brep, int>(at.Brep,
                        at.Brep.TransformComponent(components, of.Transform, at.Tolerances.Absolute, Conversions.Unset(of.TimeLimit.Map(static limit => limit.TotalSeconds)), of.UseMultipleThreads)), at.Retain)),
            splitFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(BrepFace.Split), _ => Edits.Replaced<Brep, int>(at.Brep.Faces[of.Face].Split(of.Curves, at.Tolerances.Absolute)), at.Retain),
            removeAllHoles: static (at, _) => Copies.Acquire(nameof(Brep.RemoveHoles), _ => Edits.Replaced<Brep, int>(at.Brep.RemoveHoles(at.Tolerances.Absolute)), at.Retain),
            removeHoles: static (at, of) =>
                from loops in IO.lift(() => Copies.Within(at.Brep, toSeq(of.Loops).Map(static loop => new ComponentIndex(ComponentIndexType.BrepLoop, loop))))
                from inner in IO.lift(() => Callbacks.Each(loops, (loop, element) => at.Brep.Loops[loop.Index].LoopType switch {
                    BrepLoopType.Inner => Fin.Succ(loop),
                    var type => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture,
                        $"{nameof(BrepLoop)} {loop.Index} at element {element} is {type}, {nameof(Brep.RemoveHoles)} takes {BrepLoopType.Inner} loops")),
                }))
                from removed in Copies.Acquire(nameof(Brep.RemoveHoles), _ => Edits.Replaced<Brep, int>(at.Brep.RemoveHoles(inner, at.Tolerances.Absolute)), at.Retain)
                select removed,
            capPlanarHoles: static (at, _) => Copies.Acquire(nameof(Brep.CapPlanarHoles), _ => Edits.Replaced<Brep, int>(at.Brep.CapPlanarHoles(at.Tolerances.Absolute)), at.Retain),
            untrimFace: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(Brep.UntrimFace), _ => Edits.Replaced<Brep, int>(at.Brep.UntrimFace(of.Face, at.Tolerances.Absolute)), at.Retain),
            untrimOuterLoop: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(Brep.UntrimOuterLoop), _ => Edits.Replaced<Brep, int>(at.Brep.UntrimOuterLoop(of.Face)), at.Retain),
            insetFaces: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, toSeq(of.Faces)))
                >> Copies.Acquire(nameof(Brep.InsetFaces), _ => Edits.Replaced<Brep, int>(
                    at.Brep.InsetFaces(of.Faces, of.Distance, of.Loose, of.IgnoreSeams, of.CreaseCorners, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            pushPullExtend: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                >> Copies.Acquire(nameof(Brep.PushPullExtend), _ => Edits.Replaced<Brep, int>(at.Brep.PushPullExtend(of.Face, of.Transform, at.Tolerances.Absolute)), at.Retain),
            replaceEdge: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, toSeq(of.Edges)))
                >> Copies.Acquire(nameof(Brep.ReplaceEdge), _ => of.Replacement.Arguments switch {
                    var (method, curve) => Edits.Replaced<Brep, int>(at.Brep.ReplaceEdge(of.Edges, method, curve, at.Tolerances.Absolute)),
                }, at.Retain),
            duplicateSubBrep: static (at, of) =>
                IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, toSeq(of.Faces)))
                >> Copies.Acquire(nameof(Brep.DuplicateSubBrep), _ => Edits.Replaced<Brep, int>(at.Brep.DuplicateSubBrep(of.Faces)), at.Retain));
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record BrepPoint {
    private BrepPoint(Point3d point) => Point = point;

    public Point3d Point { get; }

    public sealed record OnFace(Point3d Point, int Face, Point2d Parameters, Vector3d Normal) : BrepPoint(Point);

    public sealed record OnEdge(Point3d Point, int Edge, double Parameter, Vector3d Tangent) : BrepPoint(Point);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidTopology {
    // --- [BOOLEANS]
    public static IO<Seq<Grouped<Brep, int>>> Boolean(Tolerances tolerances, Boolean<Brep> operation, bool manifoldOnly, Action<IDisposable> retain) =>
        operation.Switch(
            (Tolerance: tolerances.Absolute, ManifoldOnly: manifoldOnly, Retain: retain),
            union: static (at, union) => Copies.Acquire(nameof(Brep.CreateBooleanUnionWithIndexMap), _ => Answer.Mapped(
                Answer.Many(Brep.CreateBooleanUnionWithIndexMap(union.Operands, at.Tolerance, at.ManifoldOnly, out int[][] indexMap)), indexMap), at.Retain),
            intersection: static (at, intersection) => Booleans.PerFirst(intersection.First, first => Copies.Acquire(nameof(Brep.CreateBooleanIntersection), _ => Answer.Many(
                Brep.CreateBooleanIntersection([first], intersection.Second, at.Tolerance, at.ManifoldOnly)), at.Retain)),
            difference: static (at, difference) => Copies.Acquire(nameof(Brep.CreateBooleanDifferenceWithIndexMap), _ => Answer.Mapped(
                Answer.Many(Brep.CreateBooleanDifferenceWithIndexMap(difference.First, difference.Second, at.Tolerance, at.ManifoldOnly, out int[] indexMap)),
                indexMap?.Select(static int[] (input) => [input])), at.Retain));

    public static IO<Seq<Brep>> CreateBooleanUnion(Tolerances tolerances, Seq<Brep> breps, bool manifoldOnly, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.CreateBooleanUnion), _ => Answer.Diagnosed(
            Brep.CreateBooleanUnion(breps, tolerances.Absolute, manifoldOnly, out Point3d[] naked, out Point3d[] bad, out Point3d[] nonManifold), naked, bad, nonManifold), retain);

    public static IO<Seq<Grouped<Brep, int>>> Boolean(Tolerances tolerances, Boolean<Brep> operation, Plane plane, Action<IDisposable> retain) =>
        operation.Switch(
            (Plane: plane, Tolerance: tolerances.Absolute, Retain: retain),
            union: static (at, union) => Copies.Acquire(nameof(Brep.CreatePlanarUnion), _ => Answer.Many(Brep.CreatePlanarUnion(union.Operands, at.Plane, at.Tolerance))
                .Map(regions => regions.Map(region => new Grouped<Brep, int>(region, union.Operands.Map(static (_, index) => index)))), at.Retain),
            intersection: static (at, intersection) => Booleans.Meet(
                intersection.First,
                intersection.Second,
                temporary => Copies.Acquire(nameof(Brep.CreatePlanarUnion), _ => Answer.Many(Brep.CreatePlanarUnion(intersection.Second, at.Plane, at.Tolerance)), temporary),
                (first, region) => Copies.Acquire(nameof(Brep.CreatePlanarIntersection), _ => Answer.Many(Brep.CreatePlanarIntersection(first, region, at.Plane, at.Tolerance)), at.Retain)),
            difference: static (at, difference) => Booleans.PerFirst(difference.First, first => difference.Second.IsEmpty
                ? Copies.Duplicate(first, at.Retain).Map(static copy => Seq(copy))
                : DisposalOps.Within(
                    (Action<IDisposable> temporary) => difference.Second
                        .Map((cutter, index) => (Cutter: cutter, Held: index == difference.Second.Count - 1 ? at.Retain : temporary))
                        .FoldM(Seq(first), (pieces, step) => pieces
                            .TraverseM(piece => Copies.Acquire(nameof(Brep.CreatePlanarDifference), _ => Answer.Many(Brep.CreatePlanarDifference(piece, step.Cutter, at.Plane, at.Tolerance)), step.Held))
                            .As()
                            .Map(static cut => cut.Bind(static held => held)))
                        .As(),
                    DisposalOps.Release)));

    // --- [JOINS]
    public static IO<Brep> CreateFromJoinedEdges(Tolerances tolerances, Brep brep0, int edgeIndex0, Brep brep1, int edgeIndex1, Action<IDisposable> retain) =>
        IO.lift(() => (Copies.Within(brep0, ComponentIndexType.BrepEdge, Seq(edgeIndex0)), Copies.Within(brep1, ComponentIndexType.BrepEdge, Seq(edgeIndex1))).Apply(static (_, _) => unit).As())
        >> Copies.Acquire(nameof(Brep.CreateFromJoinedEdges), _ => Answer.One(Brep.CreateFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, tolerances.Absolute)), retain);

    public static IO<(Brep Face0, Brep Face1)> ExtendBrepFacesToConnect(Tolerances tolerances, BrepFace face0, BrepFace face1, ConnectAt at, Action<IDisposable> retain) =>
        at.Switch(
            (Face0: face0, Face1: face1, Tolerances: tolerances, Retain: retain),
            edges: static (state, edges) =>
                IO.lift(() => (Copies.Within(state.Face0.Brep, ComponentIndexType.BrepEdge, Seq(edges.Edge0)), Copies.Within(state.Face1.Brep, ComponentIndexType.BrepEdge, Seq(edges.Edge1)))
                    .Apply(static (_, _) => unit).As())
                >> Copies.Acquire(nameof(Brep.ExtendBrepFacesToConnect), _ => Connected(
                    Brep.ExtendBrepFacesToConnect(state.Face0, edges.Edge0, state.Face1, edges.Edge1, state.Tolerances.Absolute, state.Tolerances.Angle, out Brep? first, out Brep? second),
                    first, second), state.Retain),
            picks: static (state, picks) => Copies.Acquire(nameof(Brep.ExtendBrepFacesToConnect), _ => Connected(
                Brep.ExtendBrepFacesToConnect(state.Face0, picks.Pick0, state.Face1, picks.Pick1, state.Tolerances.Absolute, state.Tolerances.Angle, out Brep? first, out Brep? second),
                first, second), state.Retain));

    private static Answer<(Brep Face0, Brep Face1)> Connected(bool accepted, Brep? first, Brep? second) =>
        (Answer.Accepted(accepted), Answer.One(first), Answer.One(second)).Apply(static (_, face0, face1) => (Face0: face0, Face1: face1)).As();

    // --- [SPLITS]
    public static IO<Seq<Grouped<Brep, ComponentIndex>>> SplitDisjointPieces(Brep brep, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.SplitDisjointPieces), _ => Answer.Mapped(Answer.Many(Brep.SplitDisjointPieces(brep, out List<int[]> indexMap)), indexMap)
            .Map(static pieces => pieces.Map(static piece => new Grouped<Brep, ComponentIndex>(piece.Value, piece.From.Map(static face => new ComponentIndex(ComponentIndexType.BrepFace, face))))), retain);

    public static IO<(Seq<Brep> Pieces, Option<bool> ToleranceWasRaised)> Split(Tolerances tolerances, Brep brep, BrepCut cut, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.Split), _ => cut.Switch(
            (Brep: brep, Tolerance: tolerances.Absolute),
            brep: static (at, cutter) => Answer.Many(at.Brep.Split(cutter.Cutter, at.Tolerance, out bool raised)).Map(pieces => (Pieces: pieces, ToleranceWasRaised: Some(raised))),
            breps: static (at, cutters) => Answer.Many(at.Brep.Split(cutters.Cutters, at.Tolerance)).Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)),
            curves: static (at, cutters) => Answer.Many(at.Brep.Split(cutters.Cutters, at.Tolerance)).Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)),
            projected: static (at, cutters) => Answer.Many(at.Brep.Split(cutters.Cutters, cutters.Normal, cutters.PlanView, at.Tolerance))
                .Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None))), retain);

    public static IO<(Seq<Brep> Kept, Seq<Brep> CutAway)> WireCut(Tolerances tolerances, Brep brep, WireCutter cutter, Vector3d direction, bool splitKinkyFaces, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.WireCut), _ => cutter.Switch(
                (Brep: brep, Direction: direction, Tolerance: tolerances.Absolute, SplitKinkyFaces: splitKinkyFaces),
                closed: static (at, closed) => (
                    Cut: at.Brep.WireCut(
                        closed.Curves, at.Direction, Conversions.Unset(closed.Depth.Map(static depth => depth.Depth)), closed.Depth.Exists(static depth => depth.BothSides),
                        at.Tolerance, at.SplitKinkyFaces, out Brep[]? kept, out Brep[]? cutAway),
                    Kept: kept,
                    CutAway: cutAway),
                open: static (at, open) => (
                    Cut: at.Brep.WireCut(
                        open.Curve, at.Direction, open.TrimDirection, Conversions.Unset(open.Depth.Map(static depth => depth.Depth)), Conversions.Unset(open.Depth.Map(static depth => depth.TrimDepth)),
                        open.BothSides, open.Depth.Exists(static depth => depth.TrimBothSides), at.Tolerance, at.SplitKinkyFaces, out Brep[]? kept, out Brep[]? cutAway),
                    Kept: kept,
                    CutAway: cutAway)) switch {
                        var (cut, kept, cutAway) => (Answer.Accepted(cut), Answer.Many(kept), Answer.Many(cutAway)).Apply(static (_, keptPieces, cutPieces) => (Kept: keptPieces, CutAway: cutPieces)).As(),
                    }, retain);

    // --- [EDITS]
    public static IO<(Brep Result, Seq<(SolidEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Brep source, Seq<SolidEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (brep, edit, held) => edit.Step(tolerances, brep, held), static brep => IO.lift(brep.Compact), retain);

    // --- [CONVERSIONS]
    public static IO<Brep> TryConvertBrep(GeometryBase geometry, Action<IDisposable> retain) =>
        geometry is Brep brep
            ? Copies.Duplicate(brep, retain)
            : Copies.Acquire(nameof(Brep.TryConvertBrep), _ => Answer.One(Brep.TryConvertBrep(geometry)), retain);

    // --- [QUERIES]
    public static IO<BrepPoint> ClosestPoint(Brep brep, Point3d testPoint) =>
        ClosestPoint(brep, testPoint, None).Bind(static found => IO.lift(found.ToFin(new Refused(nameof(Brep.ClosestPoint), Refused.Declined))));

    public static IO<Option<BrepPoint>> ClosestPoint(Brep brep, Point3d testPoint, Option<double> within) =>
        IO.lift(() => Callbacks.Found(
                brep.ClosestPoint(testPoint, out Point3d closest, out ComponentIndex component, out double s, out double t, Conversions.Unset(within), out Vector3d normal),
                (Closest: closest, Component: component, S: s, T: t, Normal: normal))
            .Map(static at => at.Component.ComponentIndexType is ComponentIndexType.BrepEdge
                ? (BrepPoint)new BrepPoint.OnEdge(at.Closest, at.Component.Index, at.S, at.Normal)
                : new BrepPoint.OnFace(at.Closest, at.Component.Index, new Point2d(at.S, at.T), at.Normal)));
}
