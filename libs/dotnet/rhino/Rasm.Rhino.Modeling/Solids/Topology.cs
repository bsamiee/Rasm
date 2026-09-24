using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling.Surfaces;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidBooleanMethod {
    public sealed record Union(Seq<Brep> Breps, bool ManifoldOnly) : SolidBooleanMethod;

    public sealed record Intersection(Seq<Brep> First, Seq<Brep> Second, bool ManifoldOnly) : SolidBooleanMethod;

    public sealed record Difference(Seq<Brep> First, Seq<Brep> Second, bool ManifoldOnly) : SolidBooleanMethod;

    public sealed record Split(Seq<Brep> First, Seq<Brep> Second) : SolidBooleanMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanarBooleanMethod {
    public sealed record Union(Seq<Brep> Breps) : PlanarBooleanMethod;

    public sealed record Intersection(Brep A, Brep B) : PlanarBooleanMethod;

    public sealed record Difference(Brep A, Brep B) : PlanarBooleanMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MergeSurfaceSource {
    public sealed record OfBreps(Brep Brep0, Brep Brep1) : MergeSurfaceSource;

    public sealed record AtPoints(Brep Brep0, Brep Brep1, Point2d Point0, Point2d Point1, double Roundness, bool Smooth) : MergeSurfaceSource;

    public sealed record OfSurfaces(Surface Surface0, Surface Surface1) : MergeSurfaceSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConnectMethod {
    public sealed record AtEdges(int Edge0, int Edge1) : ConnectMethod;

    public sealed record AtPoints(Point3d Point0, Point3d Point1) : ConnectMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TrimCutter {
    public sealed record ByBrep(Brep Cutter) : TrimCutter;

    public sealed record ByPlane(Plane Cutter) : TrimCutter;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrepCutters {
    public sealed record ByBreps(Seq<Brep> Cutters) : BrepCutters;

    public sealed record ByCurves(Seq<Curve> Cutters) : BrepCutters;

    public sealed record Projected(Seq<GeometryBase> Cutters, Vector3d Normal, bool PlanView) : BrepCutters;
}

public sealed record SplitResult(Seq<Brep> Pieces, bool ToleranceRaised);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidEdit {
    public sealed record JoinNakedEdges(double Tolerance) : SolidEdit;

    public sealed record MergeCoplanar(double Tolerance, double AngleTolerance) : SolidEdit;

    public sealed record MergeFace(int Face, double Tolerance, double AngleTolerance) : SolidEdit;

    public sealed record MergeFacePair(int Face0, int Face1, double Tolerance, double AngleTolerance) : SolidEdit;

    public sealed record RemoveFins() : SolidEdit;

    public sealed record CullUnusedFaces() : SolidEdit;

    public sealed record Repair(double Tolerance) : SolidEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidDerivation {
    public sealed record CapPlanarHoles(double Tolerance) : SolidDerivation;

    public sealed record RemoveAllHoles(double Tolerance) : SolidDerivation;

    public sealed record RemoveHoles(Seq<ComponentIndex> Loops, double Tolerance) : SolidDerivation;

    public sealed record ChangeSeam(BrepFace Face, int Direction, double Parameter, double Tolerance) : SolidDerivation;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidTopology {
    // --- [BOOLEANS]
    public static IO<Seq<Brep>> Boolean(SolidBooleanMethod method, double tolerance) =>
        method.Switch(
            tolerance,
            union: static (tol, union) =>
                from filled in IO.lift(() => Invalid.Unless(!union.Breps.IsEmpty, nameof(SolidBooleanMethod.Union.Breps)))
                from answer in IO.lift(() => (
                    Breps: Brep.CreateBooleanUnion(union.Breps, tol, union.ManifoldOnly, out Point3d[] naked, out Point3d[] bad, out Point3d[] nonManifold),
                    Failure: new BooleanUnionFailed(toSeq(naked), toSeq(bad), toSeq(nonManifold))))
                from breps in answer.Breps is null
                    ? IO.fail<Seq<Brep>>(answer.Failure)
                    : GeometryResults.Acquire(() => answer.Breps, nameof(Brep.CreateBooleanUnion))
                select breps,
            intersection: static (tol, intersection) =>
                from filled in IO.lift(() => GeometryResults.Sets((intersection.First, nameof(intersection.First)), (intersection.Second, nameof(intersection.Second))))
                from breps in GeometryResults.Acquire(() => Brep.CreateBooleanIntersection(intersection.First, intersection.Second, tol, intersection.ManifoldOnly), nameof(Brep.CreateBooleanIntersection))
                select breps,
            difference: static (tol, difference) =>
                from filled in IO.lift(() => GeometryResults.Sets((difference.First, nameof(difference.First)), (difference.Second, nameof(difference.Second))))
                from breps in GeometryResults.Acquire(() => Brep.CreateBooleanDifference(difference.First, difference.Second, tol, difference.ManifoldOnly), nameof(Brep.CreateBooleanDifference))
                select breps,
            split: static (tol, split) =>
                from filled in IO.lift(() => GeometryResults.Sets((split.First, nameof(split.First)), (split.Second, nameof(split.Second))))
                from breps in GeometryResults.Acquire(() => Brep.CreateBooleanSplit(split.First, split.Second, tol), nameof(Brep.CreateBooleanSplit))
                select breps);

    public static IO<Seq<Brep>> PlanarBoolean(Plane plane, PlanarBooleanMethod method, double tolerance) =>
        from valid in IO.lift(() => Invalid.Unless(plane.IsValid, nameof(Plane.IsValid)))
        from breps in method.Switch(
            (Plane: plane, Tolerance: tolerance),
            union: static (section, union) =>
                from filled in IO.lift(() => Invalid.Unless(!union.Breps.IsEmpty, nameof(PlanarBooleanMethod.Union.Breps)))
                from result in GeometryResults.Acquire(() => Brep.CreatePlanarUnion(union.Breps, section.Plane, section.Tolerance), nameof(Brep.CreatePlanarUnion))
                select result,
            intersection: static (section, intersection) => GeometryResults.Acquire(() => Brep.CreatePlanarIntersection(intersection.A, intersection.B, section.Plane, section.Tolerance), nameof(Brep.CreatePlanarIntersection)),
            difference: static (section, difference) => GeometryResults.Acquire(() => Brep.CreatePlanarDifference(difference.A, difference.B, section.Plane, section.Tolerance), nameof(Brep.CreatePlanarDifference)))
        select breps;

    public static IO<Seq<Brep>> Solidify(Seq<Brep> breps, double tolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!breps.IsEmpty, nameof(breps)))
        from solids in GeometryResults.Acquire(() => Brep.CreateSolid(breps, tolerance), nameof(Brep.CreateSolid))
        select solids;

    // --- [JOINS]
    public static IO<Seq<(Brep Brep, Seq<int> Inputs)>> Join(Seq<Brep> breps, double tolerance, double angleTolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!breps.IsEmpty, nameof(breps)))
        from rows in GeometryResults.Acquire(() => (Brep.JoinBreps(breps, tolerance, angleTolerance, out List<int[]> map), map), nameof(Brep.JoinBreps))
        select rows.Map(static row => (Brep: row.Result, Inputs: toSeq(row.Row)));

    public static IO<Brep> JoinEdges(Brep brep0, int edge0, Brep brep1, int edge1, double tolerance) =>
        IO.lift(() =>
            from first in IndexOutOfRange.Unless(edge0, brep0.Edges.Count, nameof(Brep.Edges))
            from second in IndexOutOfRange.Unless(edge1, brep1.Edges.Count, nameof(Brep.Edges))
            from joined in Missing.Unless(Brep.CreateFromJoinedEdges(brep0, edge0, brep1, edge1, tolerance), nameof(Brep.CreateFromJoinedEdges))
            select joined);

    public static IO<Brep> Merge(Seq<Brep> breps, double tolerance) =>
        IO.lift(() =>
            from filled in Invalid.Unless(!breps.IsEmpty, nameof(breps))
            from merged in Missing.Unless(Brep.MergeBreps(breps, tolerance), nameof(Brep.MergeBreps))
            select merged);

    public static IO<Brep> MergeSurfaces(MergeSurfaceSource source, double tolerance, double angleTolerance) =>
        IO.lift(() => Missing.Unless(source.Switch(
                (Tolerance: tolerance, Angle: angleTolerance),
                ofBreps: static (within, pair) => Brep.MergeSurfaces(pair.Brep0, pair.Brep1, within.Tolerance, within.Angle),
                atPoints: static (within, at) => Brep.MergeSurfaces(at.Brep0, at.Brep1, within.Tolerance, within.Angle, at.Point0, at.Point1, at.Roundness, at.Smooth),
                ofSurfaces: static (within, pair) => Brep.MergeSurfaces(pair.Surface0, pair.Surface1, within.Tolerance, within.Angle)), nameof(Brep.MergeSurfaces)));

    public static IO<(Brep Matched, Brep Target)> Match(BrepEdge edge, Seq<Curve> targets, MatchSrfSettings settings) =>
        from filled in IO.lift(() => Invalid.Unless(!targets.IsEmpty, nameof(targets)))
        from matched in IO.lift(() => Refused.Unless(Brep.CreateFromMatch(edge, targets, settings, out Brep matchedBrep, out Brep targetBrep), (Matched: matchedBrep, Target: targetBrep), nameof(Brep.CreateFromMatch)))
        select matched;

    public static IO<(Brep Face0, Brep Face1)> ExtendToConnect(BrepFace face0, BrepFace face1, ConnectMethod method, double tolerance, double angleTolerance) =>
        IO.lift(() => method.Switch(
            (Face0: face0, Face1: face1, Tolerance: tolerance, Angle: angleTolerance),
            atEdges: static (faces, edges) => Refused.Unless(Brep.ExtendBrepFacesToConnect(faces.Face0, edges.Edge0, faces.Face1, edges.Edge1, faces.Tolerance, faces.Angle, out Brep extended0, out Brep extended1), (Face0: extended0, Face1: extended1), nameof(Brep.ExtendBrepFacesToConnect)),
            atPoints: static (faces, points) => Refused.Unless(Brep.ExtendBrepFacesToConnect(faces.Face0, points.Point0, faces.Face1, points.Point1, faces.Tolerance, faces.Angle, out Brep extended0, out Brep extended1), (Face0: extended0, Face1: extended1), nameof(Brep.ExtendBrepFacesToConnect))));

    // --- [SPLITS]
    public static IO<Seq<Brep>> SplitDisjointPieces(Brep brep) =>
        GeometryResults.Acquire(() => Brep.SplitDisjointPieces(brep), nameof(Brep.SplitDisjointPieces));

    public static IO<SplitResult> SplitBy(Brep brep, Brep cutter, double tolerance) =>
        from answer in IO.lift(() => (Pieces: brep.Split(cutter, tolerance, out bool raised), Raised: raised))
        from pieces in GeometryResults.Acquire(() => answer.Pieces, nameof(Brep.Split))
        select new SplitResult(pieces, answer.Raised);

    public static IO<Seq<Brep>> SplitByMany(Brep brep, BrepCutters cutters, double tolerance) =>
        cutters.Switch(
            (Brep: brep, Tolerance: tolerance),
            byBreps: static (target, cutting) =>
                from filled in IO.lift(() => Invalid.Unless(!cutting.Cutters.IsEmpty, nameof(BrepCutters.ByBreps.Cutters)))
                from pieces in GeometryResults.Acquire(() => target.Brep.Split(cutting.Cutters, target.Tolerance), nameof(Brep.Split))
                select pieces,
            byCurves: static (target, cutting) =>
                from filled in IO.lift(() => Invalid.Unless(!cutting.Cutters.IsEmpty, nameof(BrepCutters.ByCurves.Cutters)))
                from pieces in GeometryResults.Acquire(() => target.Brep.Split(cutting.Cutters, target.Tolerance), nameof(Brep.Split))
                select pieces,
            projected: static (target, cutting) =>
                from filled in IO.lift(() => Invalid.Unless(!cutting.Cutters.IsEmpty, nameof(BrepCutters.Projected.Cutters)))
                from pieces in GeometryResults.Acquire(() => target.Brep.Split(cutting.Cutters, cutting.Normal, cutting.PlanView, target.Tolerance), nameof(Brep.Split))
                select pieces);

    public static IO<Seq<Brep>> Trim(Brep brep, TrimCutter cutter, double tolerance) =>
        cutter.Switch(
            (Brep: brep, Tolerance: tolerance),
            byBrep: static (target, cutting) => GeometryResults.Acquire(() => target.Brep.Trim(cutting.Cutter, target.Tolerance), nameof(Brep.Trim)),
            byPlane: static (target, cutting) =>
                from valid in IO.lift(() => Invalid.Unless(cutting.Cutter.IsValid, nameof(Plane.IsValid)))
                from pieces in GeometryResults.Acquire(() => target.Brep.Trim(cutting.Cutter, target.Tolerance), nameof(Brep.Trim))
                select pieces);

    public static IO<Seq<Brep>> CutUp(Surface surface, Seq<Curve> curves, bool flip, double fitTolerance, double keepTolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!curves.IsEmpty, nameof(curves)))
        from pieces in GeometryResults.Acquire(() => Brep.CutUpSurface(surface, curves, flip: flip, fitTolerance: fitTolerance, keepTolerance: keepTolerance), nameof(Brep.CutUpSurface))
        select pieces;

    public static IO<Brep> CopyTrimCurves(BrepFace source, Surface target, double tolerance) =>
        IO.lift(() => Missing.Unless(Brep.CopyTrimCurves(source, target, tolerance), nameof(Brep.CopyTrimCurves)));

    public static IO<Seq<Brep>> UnjoinEdges(Brep brep, Seq<int> edges) =>
        from valid in IO.lift(() =>
            from filled in Invalid.Unless(!edges.IsEmpty, nameof(edges))
            from listed in Answers.InRange(edges, brep.Edges.Count, nameof(Brep.Edges))
            select unit)
        from pieces in GeometryResults.Acquire(() => brep.UnjoinEdges(edges), nameof(Brep.UnjoinEdges))
        select pieces;

    // --- [EDITS]
    public static IO<Brep> AsBrep(GeometryBase geometry) =>
        IO.lift(() => Optional(Brep.TryConvertBrep(geometry)).ToFin(new WrongGeometry(typeof(Brep), geometry.ObjectType)));

    public static IO<Brep> Edit(Brep source, SolidEdit edit) =>
        from apply in IO.lift(() => Validated(source, edit))
        from edited in GeometryOps.EditCopy(source, apply)
        select edited.Copy;

    public static IO<Brep> Derive(Brep brep, SolidDerivation edit) =>
        IO.lift(() => edit.Switch(
            brep,
            capPlanarHoles: static (source, cap) => Missing.Unless(source.CapPlanarHoles(cap.Tolerance), nameof(Brep.CapPlanarHoles)),
            removeAllHoles: static (source, holes) => Missing.Unless(source.RemoveHoles(holes.Tolerance), nameof(Brep.RemoveHoles)),
            removeHoles: static (source, holes) =>
                from filled in Invalid.Unless(!holes.Loops.IsEmpty, nameof(SolidDerivation.RemoveHoles.Loops))
                from derived in Missing.Unless(source.RemoveHoles(holes.Loops, holes.Tolerance), nameof(Brep.RemoveHoles))
                select derived,
            changeSeam: static (_, seam) =>
                from directed in SurfaceConstruction.Directed(seam.Direction, nameof(Brep.ChangeSeam))
                from derived in Missing.Unless(Brep.ChangeSeam(seam.Face, seam.Direction, seam.Parameter, seam.Tolerance), nameof(Brep.ChangeSeam))
                select derived));

    private static Fin<Func<Brep, Fin<Unit>>> Validated(Brep source, SolidEdit edit) =>
        edit.Switch(
            source,
            joinNakedEdges: static (_, of) => fun((Brep copy) => Fin.Succ(ignore(copy.JoinNakedEdges(of.Tolerance)))),
            mergeCoplanar: static (_, of) => fun((Brep copy) => Fin.Succ(ignore(copy.MergeCoplanarFaces(of.Tolerance, of.AngleTolerance)))),
            mergeFace: static (brep, of) =>
                from inside in IndexOutOfRange.Unless(of.Face, brep.Faces.Count, nameof(Brep.Faces))
                select fun((Brep copy) => Fin.Succ(ignore(copy.MergeCoplanarFaces(of.Face, of.Tolerance, of.AngleTolerance)))),
            mergeFacePair: static (brep, of) =>
                from first in IndexOutOfRange.Unless(of.Face0, brep.Faces.Count, nameof(Brep.Faces))
                from second in IndexOutOfRange.Unless(of.Face1, brep.Faces.Count, nameof(Brep.Faces))
                select fun((Brep copy) => Fin.Succ(ignore(copy.MergeCoplanarFaces(of.Face0, of.Face1, of.Tolerance, of.AngleTolerance)))),
            removeFins: static (_, _) => fun(static (Brep copy) => Refused.Unless(copy.RemoveFins(), nameof(Brep.RemoveFins))),
            cullUnusedFaces: static (_, _) => fun(static (Brep copy) => Refused.Unless(copy.CullUnusedFaces(), nameof(Brep.CullUnusedFaces))),
            repair: static (_, of) => fun((Brep copy) => Refused.Unless(copy.Repair(of.Tolerance), nameof(Brep.Repair))));
}
