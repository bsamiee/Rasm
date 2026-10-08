using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Display.Feedback;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedGrip(Point3d location, Func<Point3d, MouseState, IO<Unit>> dragged, IPlugInSink sink) : GripUserInterfaceObject(location) {
    protected override void OnDrag(Point3d newLocation, MouseState mouse) =>
        _ = Callbacks.Answer(
            from moved in IO.lift(() => base.OnDrag(newLocation, mouse))
            from answer in dragged(GripLocation, mouse)
            select answer, static () => unit, new(sink, typeof(DefinedGrip), nameof(OnDrag)));
}

public sealed class DefinedDirectionGrip(Point3d location, Vector3d direction, Func<Point3d, MouseState, IO<Unit>> dragged, IPlugInSink sink)
    : DirectionGripUserInterfaceObject(location, direction) {
    protected override void OnDrag(Point3d newLocation, MouseState mouse) =>
        _ = Callbacks.Answer(
            from moved in IO.lift(() => base.OnDrag(newLocation, mouse))
            from answer in dragged(GripLocation, mouse)
            select answer, static () => unit, new(sink, typeof(DefinedDirectionGrip), nameof(OnDrag)));
}

public sealed class DefinedRotationGrip(Plane plane, double radius, Func<double, MouseState, IO<Unit>> rotated, IPlugInSink sink)
    : RotationGripUserInterfaceObject(plane, radius) {
    protected override void OnRotationDrag(double angle, MouseState mouse) =>
        _ = Callbacks.Answer(
            from moved in IO.lift(() => base.OnRotationDrag(angle, mouse))
            from answer in rotated(angle, mouse)
            select answer, static () => unit, new(sink, typeof(DefinedRotationGrip), nameof(OnRotationDrag)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Widgets {
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Func<IPlugInSink, UserInterfaceObjectBase> widget) =>
        (_, sink) => IO.lift(() => widget(sink)).Bind(static created => Register(created, None));

    public static IO<IDisposable> Register(UserInterfaceObjectBase widget, Option<RhinoDoc> document) =>
        IO.lift(() => document.Match(
                Some: target => Refused.Unless(target.ViewUserInterface.Add(widget), widget, nameof(ViewUserInterfaceTable.Add)),
                None: () => Refused.Unless(widget.RegisterForAllDocuments(), widget, nameof(UserInterfaceObjectBase.RegisterForAllDocuments))))
            .Map(static IDisposable (registered) => new Disposal<UserInterfaceObjectBase>(registered, static held => held.Unregister()));
}
