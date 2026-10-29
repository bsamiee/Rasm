using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GripPoint(Point3d Location) {
    public Option<double> Weight { get; init; }

    public Option<(Vector3d U, Vector3d V, Vector3d Normal)> Directions { get; init; }

    public Option<double> CurveParameter { get; init; }

    public Option<(double U, double V)> SurfaceParameters { get; init; }

    public Seq<int> CurveCVIndices { get; init; }

    public Seq<IndexPair> SurfaceCVIndices { get; init; }

    public Option<Func<Point3d, Point3d>> Constraint { get; init; }
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record GripControl {
    private GripControl(ObjectGripsType type, Option<NurbsCurve> curve, Option<NurbsSurface> surface) => (Type, Curve, Surface) = (type, curve, surface);

    internal ObjectGripsType Type { get; }

    internal Option<NurbsCurve> Curve { get; }

    internal Option<NurbsSurface> Surface { get; }

    public sealed record Free(bool DragLine) : GripControl(DragLine ? ObjectGripsType.Custom : ObjectGripsType.CustomNoDragLine, None, None);

    public sealed record CurveControlPoints(NurbsCurve Points) : GripControl(ObjectGripsType.CurveControlPoint, Some(Points), None);

    public sealed record SurfaceControlPoints(NurbsSurface Points) : GripControl(ObjectGripsType.SurfaceControlPoint, None, Some(Points));
}

public sealed record GripDefinition(GripControl Control, Seq<GripPoint> Points, Func<Seq<GripPoint>, Fin<GeometryBase>> Rebuild) {
    public Seq<(int From, int To)> Polygon { get; init; }

    public Option<(int Rows, int Columns)> Grid { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class DefinedGripObject : CustomGripObject {
    // --- [STATE]
    private double weight;

    internal DefinedGripObject(GripPoint point) {
        Point = point;
        weight = Conversions.Unset(point.Weight);
        OriginalLocation = point.Location;
    }

    internal GripPoint Point { get; }

    internal GripPoint Current => Point with { Location = CurrentLocation, Weight = Conversions.Present(weight) };

    // --- [CALLBACKS]
    public override double Weight {
        get => weight;
        set {
            MovedOtherThanLocation |= value != weight;
            weight = value;
        }
    }

    public override void NewLocation() {
        _ = Point.Constraint.Map(constrain => constrain(CurrentLocation)).Filter(constrained => constrained != CurrentLocation).Iter(Move);
        base.NewLocation();
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

internal sealed class GripBody(GripDefinition definition) {
    // --- [STATE]
    internal Seq<DefinedGripObject> Points { get; } = definition.Points.Map(static point => new DefinedGripObject(point)).Strict();

    private readonly Atom<Option<GeometryBase>> preview = Atom(Option<GeometryBase>.None);

    // --- [GEOMETRY]
    internal Option<GeometryBase> Rebuilt() =>
        definition.Rebuild(Points.Map(static grip => grip.Current).Strict()).Match(
            Succ: static geometry => Some(geometry),
            Fail: static error => {
                _ = Session.Failed(error);
                return Option<GeometryBase>.None;
            });

    // --- [DRAW]
    internal void Draw(CustomObjectGrips grips, GripsDrawEventArgs args) {
        if (args.DrawDynamicStuff && grips.NewLocation) {
            grips.NewLocation = false;
            Option<GeometryBase> rebuilt = Rebuilt();
            _ = preview.Swap(_ => rebuilt);
        }
        if (args.DrawDynamicStuff) _ = preview.Value.Bind(geometry => Drawn(args.Display, geometry)).Iter(static draw => draw());
        _ = definition.Polygon.Iter(edge => args.DrawControlPolygonLine(new Line(Points[edge.From].CurrentLocation, Points[edge.To].CurrentLocation), edge.From, edge.To));
    }

    private static Option<Action> Drawn(DisplayPipeline display, GeometryBase geometry) =>
        geometry switch {
            Curve curve => Some(() => display.DrawCurve(curve, AppearanceSettings.FeedbackColor)),
            Brep brep => Some(() => display.DrawBrepWires(brep, AppearanceSettings.FeedbackColor)),
            Extrusion extrusion => Some(() => display.DrawExtrusionWires(extrusion, AppearanceSettings.FeedbackColor)),
            Mesh mesh => Some(() => display.DrawMeshWires(mesh, AppearanceSettings.FeedbackColor)),
            SubD subd => Some(() => display.DrawSubDWires(subd, AppearanceSettings.FeedbackColor, thickness: 1f)),
            Point point => Some(() => display.DrawPoint(point.Location, AppearanceSettings.FeedbackColor)),
            _ => None,
        };

    // --- [NAVIGATION]
    internal GripObject? Neighbor(int index, int dr, int ds, bool wrap) =>
        definition.Grid.Bind(grid => (Row: (index / grid.Columns) + dr, Column: (index % grid.Columns) + ds) switch {
            var (row, column) when wrap => Some((((row % grid.Rows) + grid.Rows) % grid.Rows * grid.Columns) + (((column % grid.Columns) + grid.Columns) % grid.Columns)),
            var (row, column) => Conversions.Present(row >= 0 && row < grid.Rows && column >= 0 && column < grid.Columns, (row * grid.Columns) + column),
        }).Bind(at => Points.At(at)).Map(static GripObject (grip) => grip).ValueUnsafe();

    internal GripObject? Curve(int index) => Points.Find(grip => grip.Point.CurveCVIndices.Exists(held => held == index)).ValueUnsafe();

    internal GripObject? Surface(int i, int j) => Points.Find(grip => grip.Point.SurfaceCVIndices.Exists(pair => pair.I == i && pair.J == j)).ValueUnsafe();
}

public abstract class DefinedObjectGrips : CustomObjectGrips {
    // --- [STATE]
    private readonly GripControl control;
    private readonly GripBody body;

    protected DefinedObjectGrips(GripDefinition definition) : base(definition.Control.Type) {
        (control, body) = (definition.Control, new GripBody(definition));
        _ = body.Points.Iter(AddGrip);
    }

    // --- [CALLBACKS]
    protected sealed override GeometryBase? NewGeometry() => body.Rebuilt().ValueUnsafe();

    protected sealed override void OnDraw(GripsDrawEventArgs args) {
        body.Draw(this, args);
        base.OnDraw(args);
    }

    protected sealed override GripObject? NeighborGrip(int gripIndex, int dr, int ds, int dt, bool wrap) => dt is 0 ? body.Neighbor(gripIndex, dr, ds, wrap) : null;

    protected sealed override NurbsCurve? NurbsCurve() => control.Curve.ValueUnsafe();

    protected sealed override NurbsSurface? NurbsSurface() => control.Surface.ValueUnsafe();

    protected sealed override GripObject? NurbsCurveGrip(int i) => body.Curve(i);

    protected sealed override GripObject? NurbsSurfaceGrip(int i, int j) => body.Surface(i, j);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Grips {
    // --- [REGISTRATION]
    public static IO<Unit> Register<TGrips>(Func<RhinoObject, Option<TGrips>> grips) where TGrips : DefinedObjectGrips =>
        IO.lift(() => CustomObjectGrips.RegisterGripsEnabler(target => Session.Run(
            (from candidate in IO.lift(() => grips(target))
             from attached in candidate.Traverse(built => Products.Transfer(built, held => Products.Accepted(() => target.EnableCustomGrips(held)))).As()
             select unit).Uninterruptible()), typeof(TGrips)));
}
