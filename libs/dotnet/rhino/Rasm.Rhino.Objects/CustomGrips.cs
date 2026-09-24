using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GripSource(Point3d Origin, double Weight);

public sealed record GripFlags(bool NewLocation, bool GripsMoved, bool Dragging);

public sealed record NeighborGripOptions(int Grip, int Dr, int Ds, int Dt, bool Wrap);

public sealed record GripCallbacks(
    Action<Error> Reject,
    Func<GeometryBase, Fin<Seq<GripSource>>> Sources,
    Func<Seq<Point3d>, IO<Option<GeometryBase>>> NewGeometry,
    Option<Func<int, Point3d, IO<Unit>>> LocationChanged,
    Option<Func<GripsDrawEventArgs, IO<Unit>>> Draw,
    Option<IO<Unit>> Reset,
    Option<IO<Unit>> ResetMeshes,
    Option<Func<MeshType, GripFlags, IO<Unit>>> UpdateMesh,
    Option<Func<NeighborGripOptions, IO<Option<GripObject>>>> Neighbor,
    Option<Func<int, int, IO<Option<GripObject>>>> SurfaceGrip,
    Option<IO<Option<NurbsSurface>>> Surface,
    Option<Func<int, IO<Option<GripObject>>>> CurveGrip,
    Option<IO<Option<NurbsCurve>>> Curve);

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
public sealed class CallbackGripObject : CustomGripObject {
    private readonly Option<Func<int, Point3d, IO<Unit>>> locationChanged;

    private readonly Action<Error> reject;

    public CallbackGripObject(GripSource source, Option<Func<int, Point3d, IO<Unit>>> locationChanged, Action<Error> reject) {
        OriginalLocation = source.Origin;
        Weight = source.Weight;
        this.locationChanged = locationChanged;
        this.reject = reject;
    }

    public override double Weight { get; set; }

    public override void NewLocation() {
        base.NewLocation();
        _ = Answers.Answer(locationChanged.Map(changed => changed(Index, CurrentLocation)), reject, static () => unit);
    }
}

public abstract class CallbackObjectGrips : CustomObjectGrips {
    protected abstract GripCallbacks Callbacks { get; }

    public IO<Unit> AddGrips(GeometryBase geometry) =>
        from sources in IO.lift(() => Callbacks.Sources(geometry))
        from added in IO.lift(() => sources.Iter(source => AddGrip(new CallbackGripObject(source, Callbacks.LocationChanged, Callbacks.Reject))))
        select unit;

    protected sealed override GeometryBase? NewGeometry() =>
        Answers.Answer(
            Callbacks.NewGeometry(toSeq(Range(0, GripCount)).Map(index => Grip(index).CurrentLocation).Strict()).Map(static value => value.ValueUnsafe()),
            Callbacks.Reject,
            base.NewGeometry());

    protected sealed override void OnDraw(GripsDrawEventArgs args) {
        _ = Answers.Answer(Callbacks.Draw.Map(draw => draw(args)), Callbacks.Reject, static () => unit);
        base.OnDraw(args);
    }

    protected sealed override void OnReset() {
        base.OnReset();
        _ = Answers.Answer(Callbacks.Reset, Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnResetMeshes() {
        base.OnResetMeshes();
        _ = Answers.Answer(Callbacks.ResetMeshes, Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnUpdateMesh(MeshType meshType) {
        base.OnUpdateMesh(meshType);
        _ = Answers.Answer(Callbacks.UpdateMesh.Map(update => update(meshType, new GripFlags(NewLocation, GripsMoved, Dragging()))), Callbacks.Reject, static () => unit);
        NewLocation = false;
    }

    protected sealed override GripObject? NeighborGrip(int gripIndex, int dr, int ds, int dt, bool wrap) =>
        Answers.Answer(Callbacks.Neighbor.Map(neighbor => neighbor(new NeighborGripOptions(gripIndex, dr, ds, dt, wrap)).Map(static value => value.ValueUnsafe())), Callbacks.Reject, () => base.NeighborGrip(gripIndex, dr, ds, dt, wrap));

    protected sealed override GripObject? NurbsSurfaceGrip(int i, int j) =>
        Answers.Answer(Callbacks.SurfaceGrip.Map(grip => grip(i, j).Map(static value => value.ValueUnsafe())), Callbacks.Reject, () => base.NurbsSurfaceGrip(i, j));

    protected sealed override NurbsSurface? NurbsSurface() =>
        Answers.Answer(Callbacks.Surface.Map(static surface => surface.Map(static value => value.ValueUnsafe())), Callbacks.Reject, base.NurbsSurface);

    protected sealed override GripObject? NurbsCurveGrip(int i) =>
        Answers.Answer(Callbacks.CurveGrip.Map(grip => grip(i).Map(static value => value.ValueUnsafe())), Callbacks.Reject, () => base.NurbsCurveGrip(i));

    protected sealed override NurbsCurve? NurbsCurve() =>
        Answers.Answer(Callbacks.Curve.Map(static curve => curve.Map(static value => value.ValueUnsafe())), Callbacks.Reject, base.NurbsCurve);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomGrips {
    // --- [REGISTRATION]
    public static IO<Unit> RegisterGrips<TGrips>(Func<RhinoObject, Option<TGrips>> create, Action<Error> reject) where TGrips : CallbackObjectGrips =>
        IO.lift(() => CustomObjectGrips.RegisterGripsEnabler(candidate => Enabled(create, reject, candidate), typeof(TGrips)));

    private static Unit Enabled<TGrips>(Func<RhinoObject, Option<TGrips>> create, Action<Error> reject, RhinoObject candidate) where TGrips : CallbackObjectGrips =>
        Answers.Answer(create(candidate).Map(grips => Added(candidate, grips)), reject, static () => unit);

    private static IO<Unit> Added(RhinoObject candidate, CallbackObjectGrips grips) =>
        GeometryOps.OnFailure(
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
    public static IO<Unit> SetGripsOn(RhinoObject o, bool on) =>
        IO.lift(() => {
            o.GripsOn = on;
            return Mismatch.Unless(o.GripsOn == on, nameof(RhinoObject.GripsOn));
        });

    public static IO<int> MoveGrip(RhinoObject o, Option<int> index, GripMove move) =>
        from apply in IO.lift(move.Switch(
            toPoint: static toPoint => Fin.Succ<Action<GripObject>>(target => target.Move(toPoint.Location)),
            byVector: static byVector => Fin.Succ<Action<GripObject>>(target => target.Move(byVector.Delta)),
            byXform: static byXform => Invalid.Unless<Action<GripObject>>(byXform.Xform.IsValid, target => target.Move(byXform.Xform), nameof(Transform.IsValid)),
            undo: static _ => Fin.Succ<Action<GripObject>>(static target => target.UndoMove())))
        from grips in IO.lift(() => Grips(o))
        from chosen in IO.lift(() => index.Match(
            Some: wanted => grips.Find(grip => grip.Index == wanted).Map(static grip => Seq(grip)).ToFin(new Missing(nameof(GripObject.Index))),
            None: () => grips))
        from moved in IO.lift(() => chosen.Iter(apply))
        select chosen.Count;
}
