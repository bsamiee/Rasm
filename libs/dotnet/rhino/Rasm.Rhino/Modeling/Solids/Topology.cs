using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Modeling.Curves;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SolidBoolean {
    public sealed record BooleanUnion(Seq<Brep> Breps, bool ManifoldOnly) : SolidBoolean;
    public sealed record BooleanIntersection(Seq<Brep> First, Seq<Brep> Second, bool ManifoldOnly) : SolidBoolean;
    public sealed record BooleanDifference(Seq<Brep> First, Seq<Brep> Second, bool ManifoldOnly) : SolidBoolean;
    public sealed record BooleanSplit(Seq<Brep> First, Seq<Brep> Second) : SolidBoolean;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PlanarBoolean {
    public sealed record PlanarUnion(Seq<Brep> Regions) : PlanarBoolean;
    public sealed record PlanarIntersection(Brep First, Brep Second) : PlanarBoolean;
    public sealed record PlanarDifference(Brep First, Brep Second) : PlanarBoolean;
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MergeRoundness : System.Numerics.IMinMaxValue<MergeRoundness> {
    public static MergeRoundness MinValue { get; } = new(0d);
    public static MergeRoundness MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SolidEdit {
    public sealed record JoinNakedEdges : SolidEdit;
    public sealed record JoinEdges(Seq<(int Edge0, int Edge1)> Pairs) : SolidEdit;
    public sealed record MergeCoplanarFaces(Option<int> Face) : SolidEdit;
    public sealed record MergeCoplanarFacePair(int Face0, int Face1) : SolidEdit;
    public sealed record MergeEdges(Option<int> Edge) : SolidEdit;
    public sealed record RemoveNakedMicroEdges : SolidEdit;
    public sealed record RemoveFins : SolidEdit;
    public sealed record RemoveSlits : SolidEdit;
    public sealed record ShrinkFaces : SolidEdit;
    public sealed record SplitKinkyFaces(Option<int> Face) : SolidEdit;
    public sealed record SplitKinkyEdge(int Edge) : SolidEdit;
    public sealed record SplitEdgeAtParameters(int Edge, IterableNE<double> Parameters) : SolidEdit;
    public sealed record SplitClosedFaces(int MinimumDegree) : SolidEdit;
    public sealed record SplitBipolarFaces : SolidEdit;
    public sealed record SplitFacesAtTangents(Option<int> Face, bool AggressiveMode) : SolidEdit;
    public sealed record StandardizeFaceSurfaces : SolidEdit;
    public sealed record Standardize : SolidEdit;
    public sealed record Flip(bool OnlyReversedFaces) : SolidEdit;
    public sealed record Repair : SolidEdit;
    public sealed record PrepareForBooleans : SolidEdit;
    public sealed record TransformComponent(Seq<ComponentIndex> Components, Transform Transform, Option<Duration> TimeLimit, bool UseMultipleThreads) : SolidEdit;
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
public abstract partial record SolidDerivation {
    public sealed record RemoveHoles(Set<int> Loops) : SolidDerivation;
    public sealed record UntrimFace(int Face) : SolidDerivation;
    public sealed record UntrimOuterLoop(int Face) : SolidDerivation;
    public sealed record InsetFaces(Set<int> Faces, double Distance, bool Loose, bool IgnoreSeams, bool CreaseCorners) : SolidDerivation;
    public sealed record PushPullExtend(int Face, Transform Transform) : SolidDerivation;
    public sealed record ReplaceEdge(Set<int> Edges, EdgeReplacement Replacement) : SolidDerivation;
    public sealed record DuplicateSubBrep(Set<int> Faces) : SolidDerivation;
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
    public static IO<Seq<Brep>> Boolean(SolidBoolean operation, Tolerances tolerances) =>
        operation.Switch(
            tolerances.Absolute,
            booleanUnion: static (tolerance, union) => Copies.Acquire<Brep>(
                () => Brep.CreateBooleanUnion(union.Breps, tolerance, union.ManifoldOnly, out Point3d[] naked, out Point3d[] bad, out Point3d[] nonManifold) is { } results
                    ? results
                    : new BooleanUnionFailed(toSeq(naked), toSeq(bad), toSeq(nonManifold)),
                nameof(Brep.CreateBooleanUnion)),
            booleanIntersection: static (tolerance, intersection) => Copies.Acquire(() => Brep.CreateBooleanIntersection(intersection.First, intersection.Second, tolerance, intersection.ManifoldOnly), nameof(Brep.CreateBooleanIntersection)),
            booleanDifference: static (tolerance, difference) => Copies.Acquire(() => Brep.CreateBooleanDifference(difference.First, difference.Second, tolerance, difference.ManifoldOnly), nameof(Brep.CreateBooleanDifference)),
            booleanSplit: static (tolerance, split) => Copies.AcquireNonEmpty(() => Brep.CreateBooleanSplit(split.First, split.Second, tolerance), nameof(Brep.CreateBooleanSplit)));

    public static IO<Seq<Brep>> Boolean(PlanarBoolean operation, Plane plane, Tolerances tolerances) =>
        operation.Switch(
            (Plane: plane, Tolerance: tolerances.Absolute),
            planarUnion: static (at, union) => Copies.Acquire(() => Brep.CreatePlanarUnion(union.Regions, at.Plane, at.Tolerance), nameof(Brep.CreatePlanarUnion)),
            planarIntersection: static (at, pair) => Copies.Acquire(() => Brep.CreatePlanarIntersection(pair.First, pair.Second, at.Plane, at.Tolerance), nameof(Brep.CreatePlanarIntersection)),
            planarDifference: static (at, pair) => Copies.Acquire(() => Brep.CreatePlanarDifference(pair.First, pair.Second, at.Plane, at.Tolerance), nameof(Brep.CreatePlanarDifference)));

    public static IO<Seq<Grouped<Brep>>> CreateBooleanUnionWithIndexMap(Seq<Brep> breps, bool manifoldOnly, Tolerances tolerances) =>
        Copies.Acquire(() => (Brep.CreateBooleanUnionWithIndexMap(breps, tolerances.Absolute, manifoldOnly, out int[][] indexMap), indexMap), nameof(Brep.CreateBooleanUnionWithIndexMap))
            .Map(static rows => rows.Map(static row => new Grouped<Brep>(row.Result, toSeq(row.Row))));

    public static IO<Seq<Grouped<Brep>>> CreateBooleanDifferenceWithIndexMap(Seq<Brep> firstSet, Seq<Brep> secondSet, bool manifoldOnly, Tolerances tolerances) =>
        Copies.Acquire(() => (Brep.CreateBooleanDifferenceWithIndexMap(firstSet, secondSet, tolerances.Absolute, manifoldOnly, out int[] indexMap), indexMap), nameof(Brep.CreateBooleanDifferenceWithIndexMap))
            .Map(static rows => rows.Map(static row => new Grouped<Brep>(row.Result, Seq(row.Row))));

    // --- [JOINS]
    public static IO<Seq<Grouped<Brep>>> JoinBreps(Seq<Brep> brepsToJoin, Tolerances tolerances) =>
        Copies.Acquire(() => (Brep.JoinBreps(brepsToJoin, tolerances.Absolute, tolerances.Angle, out List<int[]> indexMap), indexMap), nameof(Brep.JoinBreps))
            .Map(static rows => rows.Map(static row => new Grouped<Brep>(row.Result, toSeq(row.Row))));

    public static IO<Brep> CreateFromJoinedEdges(Brep brep0, int edgeIndex0, Brep brep1, int edgeIndex1, Tolerances tolerances) =>
        Copies.Acquire(
            () => (IndexOutOfRange.Unless(edgeIndex0, brep0.Edges.Count, nameof(Brep.CreateFromJoinedEdges)).ToValidation(), IndexOutOfRange.Unless(edgeIndex1, brep1.Edges.Count, nameof(Brep.CreateFromJoinedEdges)).ToValidation())
                .Apply((_, _) => Brep.CreateFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, tolerances.Absolute))
                .As().ToFin(),
            nameof(Brep.CreateFromJoinedEdges));

    public static IO<(Brep Matched, Option<Brep> Target)> CreateFromMatch(BrepEdge edge, Seq<Curve> targetCurves, MatchSrfSettings settings) =>
        Copies.Owned(
            IO.lift(() => (Accepted: Brep.CreateFromMatch(edge, targetCurves, settings, out Brep? matched, out Brep? target), Matched: matched, Target: target)),
            static made => made.Accepted ?
                (Written(made.Matched, nameof(Brep.CreateFromMatch)), Optional(made.Target).Traverse(static target => Measurements.Valid(target, nameof(Brep.CreateFromMatch))).As().ToValidation())
                    .Apply(static (matched, target) => (Matched: matched, Target: target))
                    .As().ToFin() : new Refused(nameof(Brep.CreateFromMatch)),
            static made => DisposalOps.Release(Conversions.Rows([made.Matched, made.Target])));

    public static IO<(Brep Face0, Brep Face1)> ExtendBrepFacesToConnect(BrepFace face0, int edgeIndex0, BrepFace face1, int edgeIndex1, Tolerances tolerances) =>
        from indices in IO.lift(() =>
                (IndexOutOfRange.Unless(edgeIndex0, face0.Brep.Edges.Count, nameof(Brep.ExtendBrepFacesToConnect)).ToValidation(), IndexOutOfRange.Unless(edgeIndex1, face1.Brep.Edges.Count, nameof(Brep.ExtendBrepFacesToConnect)).ToValidation())
                    .Apply(static (_, _) => unit)
                    .As().ToFin())
        from connected in Connected(() => (Brep.ExtendBrepFacesToConnect(face0, edgeIndex0, face1, edgeIndex1, tolerances.Absolute, tolerances.Angle, out Brep? first, out Brep? second), first, second))
        select connected;

    public static IO<(Brep Face0, Brep Face1)> ExtendBrepFacesToConnect(BrepFace face0, Point3d pick0, BrepFace face1, Point3d pick1, Tolerances tolerances) =>
        Connected(() => (Brep.ExtendBrepFacesToConnect(face0, pick0, face1, pick1, tolerances.Absolute, tolerances.Angle, out Brep? first, out Brep? second), first, second));

    private static IO<(Brep Face0, Brep Face1)> Connected(Func<(bool Accepted, Brep? First, Brep? Second)> extend) =>
        Copies.Owned(
            IO.lift(extend),
            static made => made.Accepted ?
                (Written(made.First, nameof(Brep.ExtendBrepFacesToConnect)), Written(made.Second, nameof(Brep.ExtendBrepFacesToConnect)))
                    .Apply(static (face0, face1) => (Face0: face0, Face1: face1))
                    .As().ToFin() : new Refused(nameof(Brep.ExtendBrepFacesToConnect)),
            static made => DisposalOps.Release(Conversions.Rows([made.First, made.Second])));

    // --- [SPLITS]
    public static IO<Seq<Grouped<Brep>>> SplitDisjointPieces(Brep brep) =>
        Copies.Acquire(() => (Brep.SplitDisjointPieces(brep, out List<int[]> indexMap), indexMap), nameof(Brep.SplitDisjointPieces))
            .Map(static rows => rows.Map(static row => new Grouped<Brep>(row.Result, toSeq(row.Row))));

    public static IO<(Seq<Brep> Pieces, bool ToleranceWasRaised)> Split(Brep brep, Brep cutter, Tolerances tolerances) =>
        Copies.Owned(
            IO.lift(() => (Pieces: brep.Split(cutter, tolerances.Absolute, out bool raised), Raised: raised)),
            static made => Measurements.Valid([.. made.Pieces], nameof(Brep.Split)).Map(pieces => (Pieces: pieces, ToleranceWasRaised: made.Raised)),
            static made => DisposalOps.Release(Conversions.Rows(made.Pieces)));

    public static IO<Seq<Brep>> Split(Brep brep, IterableNE<Brep> cutters, Tolerances tolerances) =>
        Copies.Acquire(() => brep.Split(cutters, tolerances.Absolute), nameof(Brep.Split));

    public static IO<Seq<Brep>> Split(Brep brep, IterableNE<Curve> cutters, Tolerances tolerances) =>
        Copies.Acquire(() => brep.Split(cutters, tolerances.Absolute), nameof(Brep.Split));

    public static IO<Seq<Brep>> Split(Brep brep, IterableNE<GeometryBase> cutters, Vector3d normal, bool planView, Tolerances tolerances) =>
        Copies.Acquire(() => brep.Split(cutters, normal, planView, tolerances.Absolute), nameof(Brep.Split));

    public static IO<(Seq<Brep> Kept, Seq<Brep> CutAway)> WireCut(Brep brep, Seq<Curve> closedCurves, Vector3d direction, Option<(double Depth, bool BothSides)> depth, bool splitKinkyFaces, Tolerances tolerances) =>
        Cut(() => (
            brep.WireCut(closedCurves, direction, Conversions.Unset(depth.Map(static at => at.Depth)), depth.Exists(static at => at.BothSides), tolerances.Absolute, splitKinkyFaces, out Brep[]? kept, out Brep[]? cutAway),
            kept,
            cutAway));

    public static IO<(Seq<Brep> Kept, Seq<Brep> CutAway)> WireCut(
        Brep brep, Curve openCurve, Vector3d direction, Vector3d trimDirection, bool bothSides, Option<(double Depth, double TrimDepth, bool TrimBothSides)> depth, bool splitKinkyFaces, Tolerances tolerances) =>
        Cut(() => (
            brep.WireCut(
                openCurve, direction, trimDirection, Conversions.Unset(depth.Map(static at => at.Depth)), Conversions.Unset(depth.Map(static at => at.TrimDepth)),
                bothSides, depth.Exists(static at => at.TrimBothSides), tolerances.Absolute, splitKinkyFaces, out Brep[]? kept, out Brep[]? cutAway),
            kept,
            cutAway));

    public static IO<Seq<Brep>> UnjoinEdges(Brep brep, Set<int> edgesToUnjoin) =>
        Copies.AcquireNonEmpty(
            () => Copies.InRange(toSeq(edgesToUnjoin), brep.Edges.Count, nameof(Brep.UnjoinEdges)).Map(_ => brep.UnjoinEdges(edgesToUnjoin)),
            nameof(Brep.UnjoinEdges));

    private static IO<(Seq<Brep> Kept, Seq<Brep> CutAway)> Cut(Func<(bool Accepted, Brep[]? Kept, Brep[]? CutAway)> wireCut) =>
        Copies.Owned(
            IO.lift(wireCut),
            static made => made.Accepted ?
                (Measurements.Valid(toSeq<Brep?>(made.Kept), nameof(Brep.WireCut)).ToValidation(), Measurements.Valid(toSeq<Brep?>(made.CutAway), nameof(Brep.WireCut)).ToValidation())
                    .Apply(static (kept, cutAway) => (Kept: kept, CutAway: cutAway))
                    .As().ToFin() : new Refused(nameof(Brep.WireCut)),
            static made => DisposalOps.Release(Conversions.Rows(made.Kept).Concat(Conversions.Rows(made.CutAway))));

    // --- [EDITS]
    public static IO<(Brep Copy, Seq<(SolidEdit Edit, EditResult<int> Result)> Results)> Edit(Brep source, Seq<SolidEdit> edits, Tolerances tolerances) =>
        from copy in Copies.Duplicate(source)
        from result in DisposalOps.OnFailure(
            from answers in edits.TraverseM(edit => Applied(copy, edit, tolerances).Map(answer => (Edit: edit, Result: answer))).As()
            from compacted in IO.lift(copy.Compact)
            from valid in IO.lift(() => Measurements.Valid(copy, nameof(Brep.Compact)))
            select (Copy: valid, Results: answers),
            IO.lift(copy.Dispose))
        select result;

    private static IO<EditResult<int>> Applied(Brep copy, SolidEdit edit, Tolerances tolerances) =>
        edit.Switch(
            (Copy: copy, Tolerances: tolerances),
            joinNakedEdges: static (edited, _) => IO.lift(() => EditResult<int>.CreateCount(edited.Copy.JoinNakedEdges(edited.Tolerances.Absolute))),
            joinEdges: static (edited, joining) =>
                (IO.lift(() => Copies.InRange(joining.Pairs.Bind(static pair => Seq(pair.Edge0, pair.Edge1)), edited.Copy.Edges.Count, nameof(Brep.JoinEdges)))
                 >> IO.lift(() => Callbacks.Each(joining.Pairs, pair => edited.Copy.JoinEdges(pair.Edge0, pair.Edge1, edited.Tolerances.Absolute, compact: false), nameof(Brep.JoinEdges))))
                    .As().Map(EditResult<int>.CreateCompleted()),
            mergeCoplanarFaces: static (edited, merge) =>
                (IO.lift(() => Copies.InRange(merge.Face.ToSeq(), edited.Copy.Faces.Count, nameof(Brep.MergeCoplanarFaces)))
                 >> IO.lift(() => new EditResult<int>(merge.Face.Match(
                     Some: face => edited.Copy.MergeCoplanarFaces(face, edited.Tolerances.Absolute, edited.Tolerances.Angle),
                     None: () => edited.Copy.MergeCoplanarFaces(edited.Tolerances.Absolute, edited.Tolerances.Angle))))).As(),
            mergeCoplanarFacePair: static (edited, pair) =>
                (IO.lift(() => Copies.InRange(Seq(pair.Face0, pair.Face1), edited.Copy.Faces.Count, nameof(Brep.MergeCoplanarFaces)))
                 >> IO.lift(() => new EditResult<int>(edited.Copy.MergeCoplanarFaces(pair.Face0, pair.Face1, edited.Tolerances.Absolute, edited.Tolerances.Angle)))).As(),
            mergeEdges: static (edited, merge) =>
                (IO.lift(() => Copies.InRange(merge.Edge.ToSeq(), edited.Copy.Edges.Count, nameof(BrepEdgeList.MergeEdge)))
                 >> IO.lift(() => EditResult<int>.CreateCount(merge.Edge.Match(
                     Some: edge => edited.Copy.Edges.MergeEdge(edge, edited.Tolerances.Angle),
                     None: () => edited.Copy.Edges.MergeAllEdges(edited.Tolerances.Angle))))).As(),
            removeNakedMicroEdges: static (edited, _) =>
                (IO.lift(() => EditResult<int>.CreateCount(edited.Copy.Edges.RemoveNakedMicroEdges(edited.Tolerances.Absolute, cleanUp: false)))
                 >> IO.lift(() => edited.Copy.SetTolerancesBoxesAndFlags())).As(),
            removeFins: static (edited, _) => IO.lift(() => Refused.Unless(edited.Copy.RemoveFins(), nameof(Brep.RemoveFins))).Map(EditResult<int>.CreateCompleted()),
            removeSlits: static (edited, _) => IO.lift(() => new EditResult<int>(edited.Copy.Faces.RemoveSlits())),
            shrinkFaces: static (edited, _) => IO.lift(() => Refused.Unless(edited.Copy.Faces.ShrinkFaces(), nameof(BrepFaceList.ShrinkFaces))).Map(EditResult<int>.CreateCompleted()),
            splitKinkyFaces: static (edited, split) =>
                (IO.lift(() => Copies.InRange(split.Face.ToSeq(), edited.Copy.Faces.Count, nameof(BrepFaceList.SplitKinkyFace)))
                 >> IO.lift(() => split.Face.Match(
                     Some: face => Refused.Unless(edited.Copy.Faces.SplitKinkyFace(face, edited.Tolerances.Angle), nameof(BrepFaceList.SplitKinkyFace)),
                     None: () => Refused.Unless(edited.Copy.Faces.SplitKinkyFaces(edited.Tolerances.Angle, compact: false), nameof(BrepFaceList.SplitKinkyFaces)))))
                    .As().Map(EditResult<int>.CreateCompleted()),
            splitKinkyEdge: static (edited, split) =>
                (IO.lift(() => IndexOutOfRange.Unless(split.Edge, edited.Copy.Edges.Count, nameof(BrepEdgeList.SplitKinkyEdge)))
                 >> IO.lift(() => Refused.Unless(edited.Copy.Edges.SplitKinkyEdge(split.Edge, edited.Tolerances.Angle), nameof(BrepEdgeList.SplitKinkyEdge))))
                    .As().Map(EditResult<int>.CreateCompleted()),
            splitEdgeAtParameters: static (edited, split) =>
                (IO.lift(() => IndexOutOfRange.Unless(split.Edge, edited.Copy.Edges.Count, nameof(BrepEdgeList.SplitEdgeAtParameters)))
                 >> IO.lift(() => edited.Copy.Edges.SplitEdgeAtParameters(split.Edge, split.Parameters)))
                    .As().Bind(static count => IO.lift(Refused.Unless(count > 0, count, nameof(BrepEdgeList.SplitEdgeAtParameters))))
                    .Map(EditResult<int>.CreateCount),
            splitClosedFaces: static (edited, split) => IO.lift(() => Refused.Unless(edited.Copy.Faces.SplitClosedFaces(split.MinimumDegree), nameof(BrepFaceList.SplitClosedFaces))).Map(EditResult<int>.CreateCompleted()),
            splitBipolarFaces: static (edited, _) => IO.lift(() => Refused.Unless(edited.Copy.Faces.SplitBipolarFaces(), nameof(BrepFaceList.SplitBipolarFaces))).Map(EditResult<int>.CreateCompleted()),
            splitFacesAtTangents: static (edited, split) =>
                (IO.lift(() => Copies.InRange(split.Face.ToSeq(), edited.Copy.Faces.Count, nameof(BrepFaceList.SplitFaceAtTangents)))
                 >> IO.lift(() => split.Face.Match(
                     Some: face => Refused.Unless(edited.Copy.Faces.SplitFaceAtTangents(face, split.AggressiveMode), nameof(BrepFaceList.SplitFaceAtTangents)),
                     None: () => Refused.Unless(edited.Copy.Faces.SplitFacesAtTangents(split.AggressiveMode), nameof(BrepFaceList.SplitFacesAtTangents)))))
                    .As().Map(EditResult<int>.CreateCompleted()),
            standardizeFaceSurfaces: static (edited, _) => IO.lift(() => edited.Copy.Faces.StandardizeFaceSurfaces()).Map(EditResult<int>.CreateCompleted()),
            standardize: static (edited, _) => IO.lift(() => edited.Copy.Standardize()).Map(EditResult<int>.CreateCompleted()),
            flip: static (edited, flip) => IO.lift(() => edited.Copy.Faces.Flip(flip.OnlyReversedFaces)).Map(EditResult<int>.CreateCompleted()),
            repair: static (edited, _) => IO.lift(() => Refused.Unless(edited.Copy.Repair(edited.Tolerances.Absolute), nameof(Brep.Repair))).Map(EditResult<int>.CreateCompleted()),
            prepareForBooleans: static (edited, _) => IO.lift(() => Refused.Unless(edited.Copy.PrepareForBooleans(), nameof(Brep.PrepareForBooleans))).Map(EditResult<int>.CreateCompleted()),
            transformComponent: static (edited, move) => IO.lift(() => Refused.Unless(
                edited.Copy.TransformComponent(move.Components, move.Transform, edited.Tolerances.Absolute, Conversions.Unset(move.TimeLimit.Map(static limit => limit.TotalSeconds)), move.UseMultipleThreads),
                nameof(Brep.TransformComponent))).Map(EditResult<int>.CreateCompleted()));

    // --- [DERIVATIONS]
    public static IO<Brep> Derive(Brep source, SolidDerivation derivation, Tolerances tolerances) =>
        derivation.Switch(
            (Source: source, Tolerances: tolerances),
            removeHoles: static (of, holes) =>
                (IO.lift(() => Callbacks.Each(toSeq(holes.Loops), (loop, _) =>
                    IndexOutOfRange.Unless(loop, of.Source.Loops.Count, nameof(Brep.RemoveHoles)) switch {
                        Fin<Unit>.Succ => RefusedElement.Unless(of.Source.Loops[loop].LoopType == BrepLoopType.Inner, nameof(Brep.RemoveHoles), loop),
                        var failed => failed,
                    }))
                 >> Copies.Acquire(() => of.Source.RemoveHoles(toSeq(holes.Loops).Map(static loop => new ComponentIndex(ComponentIndexType.BrepLoop, loop)), of.Tolerances.Absolute), nameof(Brep.RemoveHoles))).As(),
            untrimFace: static (of, untrim) =>
                (IO.lift(() => IndexOutOfRange.Unless(untrim.Face, of.Source.Faces.Count, nameof(Brep.UntrimFace)))
                 >> Copies.Acquire(() => of.Source.UntrimFace(untrim.Face, of.Tolerances.Absolute), nameof(Brep.UntrimFace))).As(),
            untrimOuterLoop: static (of, untrim) =>
                (IO.lift(() => IndexOutOfRange.Unless(untrim.Face, of.Source.Faces.Count, nameof(Brep.UntrimOuterLoop)))
                 >> Copies.Acquire(() => of.Source.UntrimOuterLoop(untrim.Face), nameof(Brep.UntrimOuterLoop))).As(),
            insetFaces: static (of, inset) =>
                (IO.lift(() => Copies.InRange(toSeq(inset.Faces), of.Source.Faces.Count, nameof(Brep.InsetFaces)))
                 >> Copies.Acquire(() => of.Source.InsetFaces(inset.Faces, inset.Distance, inset.Loose, inset.IgnoreSeams, inset.CreaseCorners, of.Tolerances.Absolute, of.Tolerances.Angle), nameof(Brep.InsetFaces))).As(),
            pushPullExtend: static (of, push) =>
                (IO.lift(() => IndexOutOfRange.Unless(push.Face, of.Source.Faces.Count, nameof(Brep.PushPullExtend)))
                 >> Copies.Acquire(() => of.Source.PushPullExtend(push.Face, push.Transform, of.Tolerances.Absolute), nameof(Brep.PushPullExtend))).As(),
            replaceEdge: static (of, replace) =>
                (IO.lift(() => Copies.InRange(toSeq(replace.Edges), of.Source.Edges.Count, nameof(Brep.ReplaceEdge)))
                 >> Copies.Acquire(() => replace.Replacement.Arguments switch {
                     var (method, curve) => of.Source.ReplaceEdge(replace.Edges, method, curve, of.Tolerances.Absolute),
                 }, nameof(Brep.ReplaceEdge))).As(),
            duplicateSubBrep: static (of, sub) =>
                (IO.lift(() => Copies.InRange(toSeq(sub.Faces), of.Source.Faces.Count, nameof(Brep.DuplicateSubBrep)))
                 >> Copies.Acquire(() => of.Source.DuplicateSubBrep(sub.Faces), nameof(Brep.DuplicateSubBrep))).As());

    public static IO<Brep> TryConvertBrep(GeometryBase geometry) =>
        geometry is Brep brep
            ? Copies.Duplicate(brep)
            : Copies.Acquire<Brep>(
                () => Brep.TryConvertBrep(geometry) is { } converted ? converted : new WrongType(typeof(Brep), geometry.GetType()),
                nameof(Brep.TryConvertBrep));

    // --- [QUERIES]
    public static IO<BrepPoint> ClosestPoint(Brep brep, Point3d testPoint) =>
        ClosestPoint(brep, testPoint, None).Bind(static found => IO.lift(found.ToFin(new Refused(nameof(Brep.ClosestPoint)))));

    public static IO<Option<BrepPoint>> ClosestPoint(Brep brep, Point3d testPoint, Option<SearchDistance> within) =>
        IO.lift(() => Callbacks.Found(
                brep.ClosestPoint(testPoint, out Point3d closest, out ComponentIndex component, out double s, out double t, Conversions.Unset(within.Map(static distance => (double)distance)), out Vector3d normal),
                (Closest: closest, Component: component, S: s, T: t, Normal: normal))
            .Map(static at => at.Component.ComponentIndexType == ComponentIndexType.BrepEdge
                ? (BrepPoint)new BrepPoint.OnEdge(at.Closest, at.Component.Index, at.S, at.Normal)
                : new BrepPoint.OnFace(at.Closest, at.Component.Index, new Point2d(at.S, at.T), at.Normal)));

    public static IO<PointContainment> Containment(Brep brep, Point3d point, Tolerances tolerances) =>
        IO.lift(() => NotSolid.Unless(brep.IsSolid, nameof(Brep.IsPointInside)).Map(_ =>
            brep.IsPointInside(point, tolerances.Absolute, strictlyIn: true) ? PointContainment.Inside
            : brep.IsPointInside(point, tolerances.Absolute, strictlyIn: false) ? PointContainment.Coincident
            : PointContainment.Outside));

    public static IO<(Seq<int> Faces, Seq<int> Edges, Seq<int> Vertices)> FindCoincidentBrepComponents(Brep brep, Point3d point, Tolerances tolerances) =>
        IO.lift(() => {
            brep.FindCoincidentBrepComponents(point, tolerances.Absolute, out int[] faces, out int[] edges, out int[] vertices);
            return (Faces: toSeq(faces), Edges: toSeq(edges), Vertices: toSeq(vertices));
        });

    // --- [ACCEPTANCE]
    private static Validation<Error, Brep> Written(Brep? product, string member) =>
        (from made in Missing.Unless(product, member)
         from valid in Measurements.Valid(made, member)
         select valid).ToValidation();
}
