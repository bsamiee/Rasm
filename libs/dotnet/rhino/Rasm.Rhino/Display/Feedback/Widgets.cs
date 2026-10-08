using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Display.Feedback;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedGrip(Point3d location, Func<Point3d, MouseState, IO<Unit>> dragged, IPlugInSink sink) : GripUserInterfaceObject(location) {
    private readonly CallbackSite site = new(sink, typeof(DefinedGrip), nameof(OnDrag));

    protected override void OnDrag(Point3d newLocation, MouseState mouse) =>
        _ = Callbacks.Answer((Location: newLocation, Mouse: mouse), drag => IO.lift(() => base.OnDrag(drag.Location, drag.Mouse)).Bind(_ => dragged(GripLocation, drag.Mouse)), static () => unit, site);
}

public sealed class DefinedDirectionGrip(Point3d location, Vector3d direction, Func<Point3d, MouseState, IO<Unit>> dragged, IPlugInSink sink)
    : DirectionGripUserInterfaceObject(location, direction) {
    private readonly CallbackSite site = new(sink, typeof(DefinedDirectionGrip), nameof(OnDrag));

    protected override void OnDrag(Point3d newLocation, MouseState mouse) =>
        _ = Callbacks.Answer((Location: newLocation, Mouse: mouse), drag => IO.lift(() => base.OnDrag(drag.Location, drag.Mouse)).Bind(_ => dragged(GripLocation, drag.Mouse)), static () => unit, site);
}

public sealed class DefinedRotationGrip(Plane plane, double radius, Func<double, MouseState, IO<Unit>> rotated, IPlugInSink sink)
    : RotationGripUserInterfaceObject(plane, radius) {
    private readonly CallbackSite site = new(sink, typeof(DefinedRotationGrip), nameof(OnRotationDrag));

    protected override void OnRotationDrag(double angle, MouseState mouse) =>
        _ = Callbacks.Answer((Angle: angle, Mouse: mouse), rotation => IO.lift(() => base.OnRotationDrag(rotation.Angle, rotation.Mouse)).Bind(_ => rotated(rotation.Angle, rotation.Mouse)), static () => unit, site);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Widgets {
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Func<IPlugInSink, UserInterfaceObjectBase> widget) =>
        (_, sink) => IO.lift(() => widget(sink))
            .Bind(static created => IO.lift(() => Refused.Unless(created.RegisterForAllDocuments(), created, nameof(UserInterfaceObjectBase.RegisterForAllDocuments))))
            .Map(Registered);

    public static IO<IDisposable> Add(RhinoDoc document, UserInterfaceObjectBase widget) =>
        IO.lift(() => Refused.Unless(document.ViewUserInterface.Add(widget), widget, nameof(ViewUserInterfaceTable.Add))).Map(Registered);

    private static IDisposable Registered(UserInterfaceObjectBase widget) =>
        new Disposal<UserInterfaceObjectBase>(widget, static held => held.Unregister());
}
