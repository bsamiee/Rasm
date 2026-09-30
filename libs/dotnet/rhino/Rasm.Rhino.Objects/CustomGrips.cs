using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GripCallbacks(
    Action<Error> Reject,
    Func<GeometryBase, Fin<Seq<CustomGripObject>>> Grips,
    Func<Seq<Point3d>, IO<Option<GeometryBase>>> NewGeometry);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GripMove {
    public sealed record ToPoint(Point3d Location) : GripMove;

    public sealed record ByVector(Vector3d Delta) : GripMove;

    public sealed record ByXform(Transform Xform) : GripMove;

    public sealed record Undo() : GripMove;
}

public sealed record GripState(
    int Index,
    Guid OwnerId,
    Point3d CurrentLocation,
    Point3d OriginalLocation,
    bool Moved,
    Option<double> Weight,
    Option<(Vector3d U, Vector3d V, Vector3d Normal)> GripDirections,
    Option<(double U, double V)> SurfaceParameters,
    Option<double> CurveParameters,
    Option<(double U, double V, double W)> CageParameters,
    Seq<int> CurveCVIndices,
    Seq<(int I, int J)> SurfaceCVIndices);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackObjectGrips : CustomObjectGrips {
    protected abstract GripCallbacks Callbacks { get; }

    public IO<Unit> AddGrips(GeometryBase geometry) =>
        from grips in IO.lift(() => Callbacks.Grips(geometry))
        from added in IO.lift(() => grips.Iter(AddGrip))
        select unit;

    protected sealed override GeometryBase? NewGeometry() =>
        Answers.Answer(
            Callbacks.NewGeometry(toSeq(Range(0, GripCount)).Map(index => Grip(index).CurrentLocation).Strict()).Map(static value => value.ValueUnsafe()),
            Callbacks.Reject,
            base.NewGeometry());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomGrips {
    // --- [REGISTRATION]
    public static IO<Unit> RegisterGrips<TGrips>(Func<RhinoObject, Option<TGrips>> create, Action<Error> reject) where TGrips : CallbackObjectGrips =>
        IO.lift(() => CustomObjectGrips.RegisterGripsEnabler(candidate => Enabled(create, reject, candidate), typeof(TGrips)));

    private static Unit Enabled<TGrips>(Func<RhinoObject, Option<TGrips>> create, Action<Error> reject, RhinoObject candidate) where TGrips : CallbackObjectGrips =>
        Answers.Answer(create(candidate).Map(grips => Added(candidate, grips)), reject, static () => unit);

    private static IO<Unit> Added(RhinoObject candidate, CallbackObjectGrips grips) =>
        DisposalOps.OnFailure(
            from geometry in IO.lift(() => Missing.Unless(candidate.Geometry, nameof(RhinoObject.Geometry)))
            from added in grips.AddGrips(geometry)
            from enabled in IO.lift(() => Refused.Unless(candidate.EnableCustomGrips(grips), nameof(RhinoObject.EnableCustomGrips)))
            select enabled,
            IO.lift(grips.Dispose));

    // --- [READS]
    public static IO<Seq<GripState>> ReadGrips(RhinoObject o) =>
        from grips in IO.lift(() => Grips(o))
        from states in IO.lift(() => grips.Map(static grip => {
            _ = grip.GetCurveCVIndices(out int[] curveIndices);
            _ = grip.GetSurfaceCVIndices(out Tuple<int, int>[] surfaceIndices);
            return new GripState(
                grip.Index,
                grip.OwnerId,
                grip.CurrentLocation,
                grip.OriginalLocation,
                grip.Moved,
                Some(grip.Weight).Filter(RhinoMath.IsValidDouble),
                Answers.Found(grip.GetGripDirections(out Vector3d u, out Vector3d v, out Vector3d normal), (U: u, V: v, Normal: normal)),
                Answers.Found(grip.GetSurfaceParameters(out double su, out double sv), (U: su, V: sv)),
                Answers.Found(grip.GetCurveParameters(out double t), t),
                Answers.Found(grip.GetCageParameters(out double cu, out double cv, out double cw), (U: cu, V: cv, W: cw)),
                toSeq(curveIndices),
                toSeq(surfaceIndices).Map(static pair => (I: pair.Item1, J: pair.Item2)));
        }).Strict())
        select states;

    private static Fin<Seq<GripObject>> Grips(RhinoObject o) =>
        Missing.Unless(o.GetGrips(), nameof(RhinoObject.GetGrips)).Map(static rows => toSeq(rows));

    // --- [MOVES]
    public static IO<int> MoveGrip(RhinoObject o, Option<int> index, GripMove move) =>
        from grips in IO.lift(() => Grips(o))
        from chosen in IO.lift(() => Answers.NonEmpty(grips.Filter(grip => index.ForAll(wanted => grip.Index == wanted)), nameof(GripObject.Index)))
        from moved in IO.lift(() => chosen.Iter(grip => move.Switch(
            grip,
            toPoint: static (target, toPoint) => target.Move(toPoint.Location),
            byVector: static (target, byVector) => target.Move(byVector.Delta),
            byXform: static (target, byXform) => target.Move(byXform.Xform),
            undo: static (target, _) => target.UndoMove())))
        select chosen.Count;
}
