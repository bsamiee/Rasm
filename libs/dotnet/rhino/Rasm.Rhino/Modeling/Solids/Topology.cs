using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record BrepCut {
    public sealed record ByBrep(Brep Cutter) : BrepCut;
    public sealed record ByBreps(IterableNE<Brep> Cutters) : BrepCut;
    public sealed record ByCurves(IterableNE<Curve> Cutters) : BrepCut;
    public sealed record ByProjection(IterableNE<GeometryBase> Cutters, Vector3d Normal, bool PlanView) : BrepCut;
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

    public IO<EditStep<Brep, int>> Step(Tolerances tolerances, Brep brep) =>
        Switch(
            (Brep: brep, Tolerances: tolerances),
            addFace: static (at, of) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.Add(of.Surface) is not null),
            addRuledFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.EdgeA, of.EdgeB)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.AddRuledFace(at.Brep.Edges[of.EdgeA], of.RevEdgeA, at.Brep.Edges[of.EdgeB], of.RevEdgeB) is not null)),
            addConeFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, Seq(new ComponentIndex(ComponentIndexType.BrepVertex, of.Vertex), new ComponentIndex(ComponentIndexType.BrepEdge, of.Edge))))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.AddConeFace(at.Brep.Vertices[of.Vertex], at.Brep.Edges[of.Edge], of.RevEdge) is not null)),
            append: static (at, of) => IO.lift(() => at.Brep.Append(of.Other)) >> Edits.Completed<Brep, int>(at.Brep),
            join: static (at, of) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Join(of.Other, at.Tolerances.Absolute, compact: true)),
            joinNakedEdges: static (at, _) => Edits.Count(at.Brep, at.Brep.JoinNakedEdges(at.Tolerances.Absolute)),
            joinEdges: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, of.Pairs.Bind(static pair => Seq(pair.Edge0, pair.Edge1))))
                .Bind(_ => IO.lift(() => Callbacks.Each(of.Pairs, (pair, index) => RhinoFault.Unless(at.Brep.JoinEdges(pair.Edge0, pair.Edge1, at.Tolerances.Absolute, compact: false), index))))
                .Bind(_ => IO.lift(at.Brep.Compact))
                .Bind(_ => Edits.Completed<Brep, int>(at.Brep)),
            mergeCoplanarFaces: static (at, _) => Edits.Changed<Brep, int>(at.Brep, at.Brep.MergeCoplanarFaces(at.Tolerances.Absolute, at.Tolerances.Angle)),
            mergeCoplanarFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Changed<Brep, int>(at.Brep, at.Brep.MergeCoplanarFaces(of.Face, at.Tolerances.Absolute, at.Tolerances.Angle))),
            mergeCoplanarFacePair: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face0, of.Face1)))
                .Bind(_ => Edits.Changed<Brep, int>(at.Brep, at.Brep.MergeCoplanarFaces(of.Face0, of.Face1, at.Tolerances.Absolute, at.Tolerances.Angle))),
            mergeAllEdges: static (at, _) => Edits.Count(at.Brep, at.Brep.Edges.MergeAllEdges(at.Tolerances.Angle)),
            mergeEdge: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.Edge)))
                .Bind(_ => Edits.Count(at.Brep, at.Brep.Edges.MergeEdge(of.Edge, at.Tolerances.Angle))),
            removeFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => IO.lift(() => at.Brep.Faces.RemoveAt(of.Face)))
                .Bind(_ => Edits.Completed<Brep, int>(at.Brep)),
            removeNakedMicroEdges: static (at, _) => Edits.Count(at.Brep, at.Brep.Edges.RemoveNakedMicroEdges(at.Tolerances.Absolute, cleanUp: true)),
            removeFins: static (at, _) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.RemoveFins()),
            removeSlits: static (at, _) => Edits.Changed<Brep, int>(at.Brep, at.Brep.Faces.RemoveSlits()),
            shrinkFaces: static (at, _) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.ShrinkFaces()),
            shrinkFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces[of.Face].ShrinkFace(of.DisableSide))),
            shrinkSurfaceToEdge: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces[of.Face].ShrinkSurfaceToEdge())),
            splitKinkyFaces: static (at, _) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitKinkyFaces(at.Tolerances.Angle, compact: false)),
            splitKinkyFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitKinkyFace(of.Face, at.Tolerances.Angle))),
            splitKinkyEdge: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.Edge)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Edges.SplitKinkyEdge(of.Edge, at.Tolerances.Angle))),
            splitEdgeAtParameters: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, Seq(of.Edge)))
                .Bind(_ => at.Brep.Edges.SplitEdgeAtParameters(of.Edge, of.Parameters) switch {
                    var split => IO.lift(() => RhinoFault.Unless(split > 0)) >> Edits.Count(at.Brep, split),
                }),
            splitClosedFaces: static (at, of) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitClosedFaces(of.MinimumDegree)),
            splitBipolarFaces: static (at, _) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitBipolarFaces()),
            splitFacesAtTangents: static (at, of) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitFacesAtTangents(of.AggressiveMode)),
            splitFaceAtTangents: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.SplitFaceAtTangents(of.Face, of.AggressiveMode))),
            standardizeFaceSurfaces: static (at, _) => IO.lift(at.Brep.Faces.StandardizeFaceSurfaces) >> Edits.Completed<Brep, int>(at.Brep),
            standardizeFaceSurface: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces.StandardizeFaceSurface(of.Face))),
            standardize: static (at, _) => IO.lift(at.Brep.Standardize) >> Edits.Completed<Brep, int>(at.Brep),
            rebuildEdges: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces[of.Face].RebuildEdges(at.Tolerances.Absolute, of.RebuildSharedEdges, of.RebuildVertices))),
            replaceSurface: static (at, of) =>
                from face in IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                from surface in IO.lift(() => Conversions.Found(Conversions.Present(at.Brep.AddSurface(of.Surface))))
                from changed in IO.lift(() => RhinoFault.Unless(at.Brep.Faces[of.Face].ChangeSurface(surface)))
                from rebuilt in Edits.Accepted<Brep, int>(at.Brep, at.Brep.Faces[of.Face].RebuildEdges(at.Tolerances.Absolute, of.RebuildSharedEdges, of.RebuildVertices))
                select rebuilt,
            matchTrimEnds: static (at, _) => Edits.Changed<Brep, int>(at.Brep, at.Brep.Trims.MatchEnds()),
            matchLoopTrimEnds: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepLoop, Seq(of.Loop)))
                .Bind(_ => Edits.Changed<Brep, int>(at.Brep, at.Brep.Trims.MatchEnds(at.Brep.Loops[of.Loop]))),
            flip: static (at, of) => IO.lift(() => at.Brep.Faces.Flip(of.OnlyReversedFaces)) >> Edits.Completed<Brep, int>(at.Brep),
            repair: static (at, _) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.Repair(at.Tolerances.Absolute)),
            prepareForBooleans: static (at, _) => Edits.Accepted<Brep, int>(at.Brep, at.Brep.PrepareForBooleans()),
            transformComponent: static (at, of) => IO.lift(() => Copies.Within(at.Brep, of.Components))
                .Bind(components => Edits.Accepted<Brep, int>(at.Brep,
                    at.Brep.TransformComponent(components, of.Transform, at.Tolerances.Absolute, Conversions.Unset(of.TimeLimit.Map(static limit => limit.TotalSeconds)), of.UseMultipleThreads))),
            splitFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Replaced<Brep, int>(at.Brep.Faces[of.Face].Split(of.Curves, at.Tolerances.Absolute))),
            removeAllHoles: static (at, _) => Edits.Replaced<Brep, int>(at.Brep.RemoveHoles(at.Tolerances.Absolute)),
            removeHoles: static (at, of) =>
                from loops in IO.lift(() => Copies.Within(at.Brep, toSeq(of.Loops).Map(static loop => new ComponentIndex(ComponentIndexType.BrepLoop, loop))))
                from inner in IO.lift(() => Callbacks.Each(loops, (loop, element) => at.Brep.Loops[loop.Index].LoopType switch {
                    BrepLoopType.Inner => Fin.Succ(loop),
                    var type => new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{type} {ComponentIndexType.BrepLoop} {loop.Index}"), RhinoReason.Mismatched, element),
                }))
                from removed in Edits.Replaced<Brep, int>(at.Brep.RemoveHoles(inner, at.Tolerances.Absolute))
                select removed,
            capPlanarHoles: static (at, _) => Edits.Replaced<Brep, int>(at.Brep.CapPlanarHoles(at.Tolerances.Absolute)),
            untrimFace: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Replaced<Brep, int>(at.Brep.UntrimFace(of.Face, at.Tolerances.Absolute))),
            untrimOuterLoop: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Replaced<Brep, int>(at.Brep.UntrimOuterLoop(of.Face))),
            insetFaces: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, toSeq(of.Faces)))
                .Bind(_ => Edits.Replaced<Brep, int>(at.Brep.InsetFaces(of.Faces, of.Distance, of.Loose, of.IgnoreSeams, of.CreaseCorners, at.Tolerances.Absolute, at.Tolerances.Angle))),
            pushPullExtend: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, Seq(of.Face)))
                .Bind(_ => Edits.Replaced<Brep, int>(at.Brep.PushPullExtend(of.Face, of.Transform, at.Tolerances.Absolute))),
            replaceEdge: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepEdge, toSeq(of.Edges)))
                .Bind(_ => of.Replacement.Arguments switch {
                    var (method, curve) => Edits.Replaced<Brep, int>(at.Brep.ReplaceEdge(of.Edges, method, curve, at.Tolerances.Absolute)),
                }),
            duplicateSubBrep: static (at, of) => IO.lift(() => Copies.Within(at.Brep, ComponentIndexType.BrepFace, toSeq(of.Faces)))
                .Bind(_ => Edits.Replaced<Brep, int>(at.Brep.DuplicateSubBrep(of.Faces))));
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
    public static IO<Seq<Grouped<Brep, int>>> Boolean(Tolerances tolerances, Boolean<Brep> operation, bool manifoldOnly) =>
        operation.Switch(
            (Tolerance: tolerances.Absolute, ManifoldOnly: manifoldOnly),
            union: static (at, union) => Copies.Mapped(Products.Made(Brep.CreateBooleanUnionWithIndexMap(union.Operands, at.Tolerance, at.ManifoldOnly, out int[][] indexMap)), indexMap),
            intersection: static (at, intersection) => Booleans.PerFirst(intersection.First, first =>
                Products.Made(Brep.CreateBooleanIntersection([first], intersection.Second, at.Tolerance, at.ManifoldOnly))),
            difference: static (at, difference) => Copies.Mapped(
                Products.Made(Brep.CreateBooleanDifferenceWithIndexMap(difference.First, difference.Second, at.Tolerance, at.ManifoldOnly, out int[] indexMap)),
                indexMap?.Select(static int[] (input) => [input])));

    public static IO<Seq<Brep>> CreateBooleanUnion(Tolerances tolerances, Seq<Brep> breps, bool manifoldOnly) =>
        Products.Made(Brep.CreateBooleanUnion(breps, tolerances.Absolute, manifoldOnly, out Point3d[] naked, out Point3d[] bad, out Point3d[] nonManifold)).MapFail(error => error + Error.New(
            $"naked edges at [{string.Join("; ", naked)}], bad intersections at [{string.Join("; ", bad)}], non-manifold edges at [{string.Join("; ", nonManifold)}]"));

    public static IO<Seq<Grouped<Brep, int>>> Boolean(Tolerances tolerances, Boolean<Brep> operation, Plane plane) =>
        operation.Switch(
            (Plane: plane, Tolerance: tolerances.Absolute),
            union: static (at, union) => Products.Made(Brep.CreatePlanarUnion(union.Operands, at.Plane, at.Tolerance))
                .Map(regions => regions.Map(region => new Grouped<Brep, int>(region, union.Operands.Map(static (_, index) => index)))),
            intersection: static (at, intersection) => Booleans.Meet(
                intersection.First,
                intersection.Second,
                () => Products.Made(Brep.CreatePlanarUnion(intersection.Second, at.Plane, at.Tolerance)),
                (first, region) => Products.Made(Brep.CreatePlanarIntersection(first, region, at.Plane, at.Tolerance))),
            difference: static (at, difference) => Booleans.PerFirst(difference.First, first => difference.Second.IsEmpty
                ? Copies.Duplicate(first).Map(static copy => Seq(copy))
                : difference.Second.FoldM(Seq(first), (pieces, cutter) => pieces
                        .TraverseM(piece => Products.Made(Brep.CreatePlanarDifference(piece, cutter, at.Plane, at.Tolerance)))
                        .As()
                        .Map(static cut => cut.Bind(static held => held)))
                    .As()));

    // --- [JOINS]
    public static IO<Brep> CreateFromJoinedEdges(Tolerances tolerances, Brep brep0, int edgeIndex0, Brep brep1, int edgeIndex1) =>
        IO.lift(() => (Copies.Within(brep0, ComponentIndexType.BrepEdge, Seq(edgeIndex0)), Copies.Within(brep1, ComponentIndexType.BrepEdge, Seq(edgeIndex1))).Apply(static (_, _) => unit).As())
            .Bind(_ => Products.Made(Brep.CreateFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, tolerances.Absolute)));

    public static IO<(Brep Face0, Brep Face1)> ExtendBrepFacesToConnect(Tolerances tolerances, BrepFace face0, BrepFace face1, ConnectAt at) =>
        at.Switch(
            (Face0: face0, Face1: face1, Tolerances: tolerances),
            edges: static (state, edges) =>
                IO.lift(() => (Copies.Within(state.Face0.Brep, ComponentIndexType.BrepEdge, Seq(edges.Edge0)), Copies.Within(state.Face1.Brep, ComponentIndexType.BrepEdge, Seq(edges.Edge1)))
                        .Apply(static (_, _) => unit).As())
                    .Bind(_ => Connected(
                        Brep.ExtendBrepFacesToConnect(state.Face0, edges.Edge0, state.Face1, edges.Edge1, state.Tolerances.Absolute, state.Tolerances.Angle, out Brep? first, out Brep? second),
                        first, second)),
            picks: static (state, picks) => Connected(
                Brep.ExtendBrepFacesToConnect(state.Face0, picks.Pick0, state.Face1, picks.Pick1, state.Tolerances.Absolute, state.Tolerances.Angle, out Brep? first, out Brep? second),
                first, second));

    private static IO<(Brep Face0, Brep Face1)> Connected(bool accepted, Brep? first, Brep? second, [CallerArgumentExpression(nameof(accepted))] string call = "") =>
        from held0 in Products.Held(first, call)
        from held1 in Products.Held(second, call)
        from connected in IO.lift(() => RhinoFault.Unless(accepted, call: call))
        from face0 in IO.lift(() => Conversions.Found(held0, call: call))
        from face1 in IO.lift(() => Conversions.Found(held1, call: call))
        select (Face0: face0, Face1: face1);

    // --- [SPLITS]
    public static IO<Seq<Grouped<Brep, ComponentIndex>>> SplitDisjointPieces(Brep brep) =>
        Copies.Mapped(Products.Made(Brep.SplitDisjointPieces(brep, out List<int[]> indexMap)), indexMap)
            .Map(static pieces => pieces.Map(static piece => new Grouped<Brep, ComponentIndex>(piece.Value, piece.From.Map(static face => new ComponentIndex(ComponentIndexType.BrepFace, face)))));

    public static IO<(Seq<Brep> Pieces, Option<bool> ToleranceWasRaised)> Split(Tolerances tolerances, Brep brep, BrepCut cut) =>
        cut.Switch(
            (Brep: brep, Tolerance: tolerances.Absolute),
            byBrep: static (at, cutter) => Products.Made(at.Brep.Split(cutter.Cutter, at.Tolerance, out bool raised)).Map(pieces => (Pieces: pieces, ToleranceWasRaised: Some(raised))),
            byBreps: static (at, cutters) => Products.Made(at.Brep.Split(cutters.Cutters, at.Tolerance)).Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)),
            byCurves: static (at, cutters) => Products.Made(at.Brep.Split(cutters.Cutters, at.Tolerance)).Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)),
            byProjection: static (at, cutters) => Products.Made(at.Brep.Split(cutters.Cutters, cutters.Normal, cutters.PlanView, at.Tolerance))
                .Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)));

    public static IO<(Seq<Brep> Kept, Seq<Brep> CutAway)> WireCut(Tolerances tolerances, Brep brep, WireCutter cutter, Vector3d direction, bool splitKinkyFaces) =>
        from wired in IO.lift(() => cutter.Switch(
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
                CutAway: cutAway)))
        from keptPieces in Products.Made(wired.Kept)
        from cutPieces in Products.Made(wired.CutAway)
        from accepted in IO.lift(() => RhinoFault.Unless(wired.Cut))
        select (Kept: keptPieces, CutAway: cutPieces);

    // --- [EDITS]
    public static IO<(Brep Result, Seq<(SolidEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Brep source, Seq<SolidEdit> edits) =>
        Edits.Apply(source, edits, (brep, edit) => edit.Step(tolerances, brep), static brep => IO.lift(brep.Compact));

    // --- [CONVERSIONS]
    public static IO<Brep> TryConvertBrep(GeometryBase geometry) =>
        geometry is Brep brep ? Copies.Duplicate(brep) : Products.Made(Brep.TryConvertBrep(geometry));

    // --- [QUERIES]
    public static IO<BrepPoint> ClosestPoint(Brep brep, Point3d testPoint) =>
        ClosestPoint(brep, testPoint, None).Bind(static found => IO.lift(() => Conversions.Found(found)));

    public static IO<Option<BrepPoint>> ClosestPoint(Brep brep, Point3d testPoint, Option<double> within) =>
        IO.lift(() => Conversions.Found(
                brep.ClosestPoint(testPoint, out Point3d closest, out ComponentIndex component, out double s, out double t, Conversions.Unset(within), out Vector3d normal),
                (Closest: closest, Component: component, S: s, T: t, Normal: normal))
            .Map(static at => at.Component.ComponentIndexType is ComponentIndexType.BrepEdge
                ? (BrepPoint)new BrepPoint.OnEdge(at.Closest, at.Component.Index, at.S, at.Normal)
                : new BrepPoint.OnFace(at.Closest, at.Component.Index, new Point2d(at.S, at.T), at.Normal)));
}
