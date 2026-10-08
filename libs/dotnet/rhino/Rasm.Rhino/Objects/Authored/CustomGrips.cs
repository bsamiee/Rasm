using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Authored;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GripPoint(
    Point3d Location,
    Option<double> Weight = default,
    Option<(Vector3d U, Vector3d V, Vector3d Normal)> Directions = default,
    Option<double> CurveParameter = default,
    Option<(double U, double V)> SurfaceParameters = default,
    Seq<int> CurveCVIndices = default,
    Seq<IndexPair> SurfaceCVIndices = default);

[Union<bool, NurbsCurve, NurbsSurface>(T1Name = "FreePoints", T2Name = "CurveControlPoint", T3Name = "SurfaceControlPoint", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class GripControl;

[Union<bool, Guid, EditPoints>(T1Name = "ControlPoints", T2Name = "Registered", T3IsStateless = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class GripKind {
    public readonly record struct EditPoints;
    public static GripKind For<TGrips>() where TGrips : CustomObjectGrips => new(typeof(TGrips).GUID);
}

[Union<Point3d, Vector3d, Transform, Undo>(T1Name = "ToPoint", T2Name = "ByVector", T3Name = "ByXform", T4IsStateless = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class GripMove {
    public readonly record struct Undo;
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
internal sealed class DefinedGripObject : CustomGripObject {
    // --- [STATE]
    private double weight;

    internal DefinedGripObject(GripPoint point) {
        Point = point;
        weight = Conversions.Unset(point.Weight);
        GripMapper.Initialize(point.Location, this);
    }

    internal GripPoint Point { get; }

    // --- [CALLBACKS]
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
    // --- [STATE]
    private readonly GripControl control;
    private readonly Seq<DefinedGripObject> grips;

    protected DefinedObjectGrips(GripControl control, IterableNE<GripPoint> points) : base(control.Switch(
        freePoints: static dragLine => dragLine ? ObjectGripsType.Custom : ObjectGripsType.CustomNoDragLine,
        curveControlPoint: static _ => ObjectGripsType.CurveControlPoint,
        surfaceControlPoint: static _ => ObjectGripsType.SurfaceControlPoint)) {
        this.control = control;
        grips = toSeq(points).Map(static point => new DefinedGripObject(point)).Strict();
        _ = grips.Iter(AddGrip);
    }

    // --- [OWNERSHIP]
    internal IO<Unit> AttachTo(RhinoObject owner) =>
        DisposalOps.OnFailure(IO.lift(() => Refused.Unless(owner.EnableCustomGrips(this), nameof(RhinoObject.EnableCustomGrips))), IO.lift(Dispose));

    // --- [CALLBACKS]
    protected abstract Fin<GeometryBase> Rebuild(Seq<GripPoint> grips);

    protected sealed override GeometryBase? NewGeometry() =>
        Callbacks.Answer(
            IO.lift(() => Rebuild(grips.Map(static grip => grip.Point with { Location = grip.CurrentLocation, Weight = Conversions.Present(grip.Weight) }).Strict()))
                .Map(static GeometryBase? (geometry) => geometry),
            static () => null,
            CallbackSite.Of(this));

    protected sealed override NurbsCurve? NurbsCurve() =>
        control.Switch(freePoints: static NurbsCurve? (_) => null, curveControlPoint: static curve => curve, surfaceControlPoint: static _ => null);

    protected sealed override NurbsSurface? NurbsSurface() =>
        control.Switch(freePoints: static NurbsSurface? (_) => null, curveControlPoint: static _ => null, surfaceControlPoint: static surface => surface);

    protected sealed override GripObject? NurbsCurveGrip(int i) =>
        grips.Find(grip => grip.Point.CurveCVIndices.Exists(index => index == i)).ValueUnsafe();

    protected sealed override GripObject? NurbsSurfaceGrip(int i, int j) =>
        grips.Find(grip => grip.Point.SurfaceCVIndices.Exists(pair => pair.I == i && pair.J == j)).ValueUnsafe();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class GripMapper {
    [MapPropertyFromSource(nameof(CustomGripObject.OriginalLocation))]
    internal static partial void Initialize(Point3d location, CustomGripObject grip);
}

public static class CustomGrips {
    // --- [REGISTRATION]
    public static IO<Unit> Register<TGrips>(Func<RhinoObject, Option<TGrips>> grips, IPlugInSink sink) where TGrips : DefinedObjectGrips {
        TurnOnGripsEventHandler handler = owner => _ = Callbacks.Answer(
            from candidate in IO.lift(() => grips(owner))
            from attached in candidate.Match(Some: built => built.AttachTo(owner), None: static () => IO.pure(unit))
            select attached,
            static () => unit,
            new CallbackSite(sink, typeof(TGrips), nameof(CustomObjectGrips.RegisterGripsEnabler)));
        return IO.lift(() => CustomObjectGrips.RegisterGripsEnabler(handler, typeof(TGrips)));
    }

    // --- [ENABLING]
    public static IO<Unit> EnableGrips(RhinoObject owner, GripKind kind) =>
        IO.lift(() => kind.Switch(
            owner,
            controlPoints: static (target, enabled) => {
                target.GripsOn = enabled;
                return Refused.Unless(target.GripsOn == enabled, nameof(RhinoObject.GripsOn));
            },
            registered: static (target, id) => Refused.Unless(target.EnableGrips(id), nameof(RhinoObject.EnableGrips)),
            editPoints: static (target, _) => Refused.Unless(target.EnableEditPointGrips(), nameof(RhinoObject.EnableEditPointGrips))));

    public static IO<GripKind> EnabledGrips(RhinoObject owner) =>
        IO.lift(GripKind () =>
            !owner.GripsOn ? false
            : owner.EditPointGripsOn ? new GripKind.EditPoints()
            : Conversions.Present(owner.EnabledGripsId).Match(Some: static GripKind (id) => id, None: static () => true));

    // --- [READS]
    public static IO<Seq<GripState>> GetGrips(RhinoObject owner) =>
        from available in IO.lift(() => Conversions.Rows(owner.GetGrips()))
        select available.Map(static grip => {
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
        }).Strict();

    // --- [EDITS]
    public static IO<int> Move(RhinoObject owner, Option<int> index, GripMove move) =>
        from available in IO.lift(() => Conversions.Rows(owner.GetGrips()))
        from chosen in IO.lift(Conversions.NonEmpty(available.Filter(grip => index.ForAll(wanted => grip.Index == wanted)).Strict(), nameof(RhinoObject.GetGrips)))
        from moved in IO.lift(() => chosen.Iter(grip => move.Switch(
            grip,
            toPoint: static (target, point) => target.Move(point),
            byVector: static (target, delta) => target.Move(delta),
            byXform: static (target, xform) => target.Move(xform),
            undo: static (target, _) => target.UndoMove())))
        select chosen.Count;

    public static IO<RhinoObject> GripUpdate(RhinoObject owner, bool deleteOriginal) =>
        IO.lift(() => Missing.Unless(owner.Document, nameof(RhinoObject.Document))
            .Bind(document => Missing.Unless(document.Objects.GripUpdate(owner, deleteOriginal), nameof(ObjectTable.GripUpdate))));
}
