using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;

namespace Rasm.Rhino.Objects.Authored;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GripPoint(
    Point3d Location,
    Option<double> Weight,
    Option<(Vector3d U, Vector3d V, Vector3d Normal)> Directions,
    Option<double> CurveParameter,
    Option<(double U, double V)> SurfaceParameters,
    Seq<int> CurveCVIndices,
    Seq<IndexPair> SurfaceCVIndices) {
    public static GripPoint At(Point3d location) => new(location, None, None, None, None, Seq<int>(), Seq<IndexPair>());
}

[Union]
public abstract partial record GripControl {
    public abstract ObjectGripsType GripsType { get; }

    public sealed record FreePoints(bool DragLine) : GripControl {
        public override ObjectGripsType GripsType => DragLine ? ObjectGripsType.Custom : ObjectGripsType.CustomNoDragLine;
    }

    public sealed record CurveControlPoint(NurbsCurve Curve) : GripControl {
        public override ObjectGripsType GripsType => ObjectGripsType.CurveControlPoint;
    }

    public sealed record SurfaceControlPoint(NurbsSurface Surface) : GripControl {
        public override ObjectGripsType GripsType => ObjectGripsType.SurfaceControlPoint;
    }
}

[Union]
public abstract partial record GripKind {
    public static GripKind For<TGrips>() where TGrips : CustomObjectGrips => new Registered(typeof(TGrips).GUID);

    public sealed record Off : GripKind;

    public sealed record ControlPoints : GripKind;

    public sealed record EditPoints : GripKind;

    public sealed record Registered(Guid Id) : GripKind;
}

[Union]
public abstract partial record GripMove {
    public sealed record ToPoint(Point3d Location) : GripMove;

    public sealed record ByVector(Vector3d Delta) : GripMove;

    public sealed record ByXform(Transform Xform) : GripMove;

    public sealed record Undo : GripMove;
}

public sealed record GripState(
    int Index,
    Guid OwnerId,
    Point3d OriginalLocation,
    bool Moved,
    Option<(double U, double V, double W)> CageParameters,
    Option<uint> SubDComponent,
    GripPoint Point);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedGripObject : CustomGripObject {
    private double weight;

    internal DefinedGripObject(GripPoint point) {
        Point = point;
        weight = Conversions.Unset(point.Weight);
        OriginalLocation = point.Location;
    }

    public GripPoint Point { get; }

    public GripPoint Current => Point with { Location = CurrentLocation, Weight = Conversions.Present(weight) };

    public override double Weight {
        get => weight;
        set {
            MovedOtherThanLocation |= value != weight;
            weight = value;
        }
    }

    protected override void OnUndoMove() => weight = Conversions.Unset(Point.Weight);

    protected override bool EvaluateGripDirections(out Vector3d x, out Vector3d y, out Vector3d z) {
        (x, y, z) = Point.Directions.IfNone(default((Vector3d U, Vector3d V, Vector3d Normal)));
        return Point.Directions.IsSome;
    }

    protected override bool EvaluateCurveParameter(out double t) {
        t = Conversions.Unset(Point.CurveParameter);
        return Point.CurveParameter.IsSome;
    }

    protected override bool EvaluateSurfaceParameters(out double u, out double v) {
        (u, v) = Point.SurfaceParameters.IfNone(default((double U, double V)));
        return Point.SurfaceParameters.IsSome;
    }

    protected override int[] EvaluateCurveCVIndices() => [.. Point.CurveCVIndices];

    protected override IndexPair[] EvaluateSurfaceCVIndices() => [.. Point.SurfaceCVIndices];
}

public abstract class DefinedObjectGrips : CustomObjectGrips {
    protected DefinedObjectGrips(GripControl control, IterableNE<GripPoint> points) : base(control.GripsType) {
        Control = control;
        Grips = toSeq(points).Map(static point => new DefinedGripObject(point)).Strict();
        _ = Grips.Iter(AddGrip);
    }

    protected GripControl Control { get; }

    protected Seq<DefinedGripObject> Grips { get; }

    internal IO<Unit> AttachTo(RhinoObject owner) =>
        DisposalOps.OnFailure(IO.lift(() => Refused.Unless(owner.EnableCustomGrips(this), nameof(RhinoObject.EnableCustomGrips))), IO.lift(Dispose));

    protected abstract Fin<GeometryBase> Rebuild(Seq<GripPoint> grips);

    protected sealed override GeometryBase? NewGeometry() =>
        Callbacks.Answer(
            IO.lift(() => Rebuild(Grips.Map(static grip => grip.Current))).Map(static GeometryBase? (geometry) => geometry),
            static () => null,
            CallbackSite.Of(this));

    protected sealed override NurbsCurve? NurbsCurve() =>
        Control.Switch<NurbsCurve?>(freePoints: static _ => null, curveControlPoint: static control => control.Curve, surfaceControlPoint: static _ => null);

    protected sealed override NurbsSurface? NurbsSurface() =>
        Control.Switch<NurbsSurface?>(freePoints: static _ => null, curveControlPoint: static _ => null, surfaceControlPoint: static control => control.Surface);

    protected sealed override GripObject? NurbsCurveGrip(int i) =>
        Grips.Find(grip => grip.Point.CurveCVIndices.Exists(index => index == i)).ValueUnsafe();

    protected sealed override GripObject? NurbsSurfaceGrip(int i, int j) =>
        Grips.Find(grip => grip.Point.SurfaceCVIndices.Exists(pair => pair.I == i && pair.J == j)).ValueUnsafe();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomGrips {
    // --- [REGISTRATION]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register<TGrips>(Func<RhinoObject, Option<TGrips>> grips) where TGrips : DefinedObjectGrips =>
        (_, sink) => IO.lift(() => CustomObjectGrips.RegisterGripsEnabler(
                owner => _ = Callbacks.Answer(
                    owner,
                    held => Enabled(held, grips(held)),
                    static () => unit,
                    new CallbackSite(sink, typeof(TGrips), nameof(CustomObjectGrips.RegisterGripsEnabler))),
                typeof(TGrips)))
            .Map(static _ => Thinktecture.Empty.Disposable());

    private static IO<Unit> Enabled<TGrips>(RhinoObject owner, Option<TGrips> grips) where TGrips : DefinedObjectGrips =>
        grips.Match(Some: built => built.AttachTo(owner), None: static () => IO.pure(unit));

    // --- [ENABLING]
    public static IO<Unit> EnableGrips(RhinoObject owner, GripKind kind) =>
        kind.Switch(
            owner,
            off: static (target, _) => IO.lift(() => { target.GripsOn = false; }),
            controlPoints: static (target, _) => IO.lift(() => { target.GripsOn = true; }),
            editPoints: static (target, _) => IO.lift(() => Refused.Unless(target.EnableEditPointGrips(), nameof(RhinoObject.EnableEditPointGrips))),
            registered: static (target, registered) => IO.lift(() => Refused.Unless(target.EnableGrips(registered.Id), nameof(RhinoObject.EnableGrips))));

    public static IO<GripKind> EnabledGrips(RhinoObject owner) =>
        IO.lift(() =>
            !owner.GripsOn ? new GripKind.Off()
            : owner.EditPointGripsOn ? new GripKind.EditPoints()
            : Conversions.Present(owner.EnabledGripsId).Match<GripKind>(Some: static id => new GripKind.Registered(id), None: static () => new GripKind.ControlPoints()));

    // --- [READS]
    public static IO<Seq<GripState>> GetGrips(RhinoObject owner) =>
        IO.lift(() => Grips(owner).Map(State).Strict());

    private static Seq<GripObject> Grips(RhinoObject owner) => Conversions.Rows(owner.GetGrips());

    private static GripState State(GripObject grip) {
        _ = grip.GetCurveCVIndices(out int[] curve);
        _ = grip.GetSurfaceCVIndices(out Tuple<int, int>[] surface);
        return new GripState(
            grip.Index,
            grip.OwnerId,
            grip.OriginalLocation,
            grip.Moved,
            Callbacks.Found(grip.GetCageParameters(out double cu, out double cv, out double cw), (U: cu, V: cv, W: cw)),
            Conversions.Present(grip.SubDComponentId),
            new GripPoint(
                grip.CurrentLocation,
                Conversions.Present(grip.Weight),
                Callbacks.Found(grip.GetGripDirections(out Vector3d u, out Vector3d v, out Vector3d normal), (U: u, V: v, Normal: normal)),
                Callbacks.Found(grip.GetCurveParameters(out double t), t),
                Callbacks.Found(grip.GetSurfaceParameters(out double su, out double sv), (U: su, V: sv)),
                toSeq(curve),
                toSeq(surface).Map(static pair => new IndexPair(pair.Item1, pair.Item2))));
    }

    // --- [EDITS]
    public static IO<int> Move(RhinoObject owner, Option<int> index, GripMove move) =>
        from chosen in IO.lift(() => Conversions.NonEmpty(Grips(owner).Filter(grip => index.ForAll(wanted => grip.Index == wanted)), nameof(RhinoObject.GetGrips)))
        from _ in IO.lift(() => chosen.Iter(grip => move.Switch(
            grip,
            toPoint: static (target, toPoint) => target.Move(toPoint.Location),
            byVector: static (target, byVector) => target.Move(byVector.Delta),
            byXform: static (target, byXform) => target.Move(byXform.Xform),
            undo: static (target, _) => target.UndoMove())))
        select chosen.Count;

    public static IO<RhinoObject> GripUpdate(RhinoObject owner, bool deleteOriginal) =>
        IO.lift(() => Missing.Unless(owner.Document, nameof(RhinoObject.Document))
            .Bind(document => Missing.Unless(document.Objects.GripUpdate(owner, deleteOriginal), nameof(ObjectTable.GripUpdate))));
}
