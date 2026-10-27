using Rasm.Rhino.UI;
using Rhino;
using Rhino.UI;

namespace Rasm.Rhino.Display;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedGrip(Point3d location, Func<Point3d, MouseState, IO<Unit>> dragged, IPlugInViews owner) : GripUserInterfaceObject(location) {
    protected override void OnDrag(Point3d newLocation, MouseState mouse) =>
        _ = owner.Run(
            from moved in IO.lift(() => base.OnDrag(newLocation, mouse))
            from answer in dragged(GripLocation, mouse)
            select answer);
}

public sealed class DefinedDirectionGrip(Point3d location, Vector3d direction, Func<Point3d, MouseState, IO<Unit>> dragged, IPlugInViews owner)
    : DirectionGripUserInterfaceObject(location, direction) {
    protected override void OnDrag(Point3d newLocation, MouseState mouse) =>
        _ = owner.Run(
            from moved in IO.lift(() => base.OnDrag(newLocation, mouse))
            from answer in dragged(GripLocation, mouse)
            select answer);
}

public sealed class DefinedRotationGrip(Plane plane, double radius, Func<double, MouseState, IO<Unit>> rotated, IPlugInViews owner)
    : RotationGripUserInterfaceObject(plane, radius) {
    protected override void OnRotationDrag(double angle, MouseState mouse) =>
        _ = owner.Run(
            from moved in IO.lift(() => base.OnRotationDrag(angle, mouse))
            from answer in rotated(angle, mouse)
            select answer);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Widgets {
    public static Func<IPlugInViews, IO<Unit>> Register(Func<IPlugInViews, UserInterfaceObjectBase> widget) =>
        views => IO.lift(() => widget(views)).Bind(static created => Register(created, None));

    public static IO<Unit> Register(UserInterfaceObjectBase widget, Option<RhinoDoc> document) =>
        use(IO.lift(() => document.Match(
                Some: target => RhinoFault.Unless(target.ViewUserInterface.Add(widget)),
                None: () => RhinoFault.Unless(widget.RegisterForAllDocuments())).Map(_ => widget)),
            static target => IO.lift(target.Unregister).Map(unit)).Map(unit);
}
