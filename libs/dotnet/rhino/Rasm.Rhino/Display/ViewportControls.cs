using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.UI;
using UUIDNext;

namespace Rasm.Rhino.Display;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class DraggedGrip(Point3d location, Func<Point3d, MouseState, IO<Unit>> dragged) : GripUserInterfaceObject(location) {
    protected override void OnDrag(Point3d newLocation, MouseState mouse) {
        base.OnDrag(newLocation, mouse);
        _ = Session.Run(dragged(GripLocation, mouse));
    }
}

internal sealed class DraggedDirectionGrip(Point3d location, Vector3d direction, Func<Point3d, MouseState, IO<Unit>> dragged)
    : DirectionGripUserInterfaceObject(location, direction) {
    protected override void OnDrag(Point3d newLocation, MouseState mouse) {
        base.OnDrag(newLocation, mouse);
        _ = Session.Run(dragged(GripLocation, mouse));
    }
}

internal sealed class RotatedGrip(Plane plane, double radius, Func<double, MouseState, IO<Unit>> rotated) : RotationGripUserInterfaceObject(plane, radius) {
    protected override void OnRotationDrag(double angle, MouseState mouse) {
        base.OnRotationDrag(angle, mouse);
        _ = Session.Run(rotated(angle, mouse));
    }
}

internal sealed class MouseMoveHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseMove(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class EndMouseMoveHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnEndMouseMove(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class MouseDownHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseDown(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class EndMouseDownHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnEndMouseDown(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class MouseUpHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseUp(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class EndMouseUpHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnEndMouseUp(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class MouseDoubleClickHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class MouseEnterHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseEnter(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class MouseHoverHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseHover(MouseCallbackEventArgs e) => deliver(e);
}

internal sealed class MouseLeaveHook(Action<MouseCallbackEventArgs> deliver) : MouseCallback {
    protected override void OnMouseLeave(MouseCallbackEventArgs e) => deliver(e);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewportControls {
    // --- [MOUSE]
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseMove = Mouse(static deliver => new MouseMoveHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseMove = Mouse(static deliver => new EndMouseMoveHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseDown = Mouse(static deliver => new MouseDownHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseDown = Mouse(static deliver => new EndMouseDownHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseUp = Mouse(static deliver => new MouseUpHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseUp = Mouse(static deliver => new EndMouseUpHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseDoubleClick = Mouse(static deliver => new MouseDoubleClickHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseEnter = Mouse(static deliver => new MouseEnterHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseHover = Mouse(static deliver => new MouseHoverHook(deliver));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseLeave = Mouse(static deliver => new MouseLeaveHook(deliver));

    private static DocumentEvent<MouseCallbackEventArgs> Mouse(Func<Action<MouseCallbackEventArgs>, MouseCallback> hook, [CallerArgumentExpression(nameof(hook))] string label = "") =>
        new(
            label,
            deliver => use(() => {
                MouseCallback callback = hook(args => Session.Run(deliver(args), label));
                callback.Enabled = true;
                return callback;
            }, static callback => callback.Enabled = false).Map(unit),
            static args => Conversions.Serial(args.View.Document));

    // --- [GRIPS]
    public static GripUserInterfaceObject Grip(Point3d location, Func<Point3d, MouseState, IO<Unit>> dragged) => new DraggedGrip(location, dragged);

    public static DirectionGripUserInterfaceObject Grip(Point3d location, Vector3d direction, Func<Point3d, MouseState, IO<Unit>> dragged) =>
        new DraggedDirectionGrip(location, direction, dragged);

    public static RotationGripUserInterfaceObject Grip(Plane plane, double radius, Func<double, MouseState, IO<Unit>> rotated) => new RotatedGrip(plane, radius, rotated);

    // --- [CONTROLS]
    public static IO<UserInterfaceButton> Button(string text, IO<Unit> clicked) =>
        Bound(new UserInterfaceButton { Text = text }, static (button, h) => button.Click += h, static (button, h) => button.Click -= h, _ => clicked);

    public static IO<UserInterfaceSlider> Slider(Interval range, IO<double> read, Func<double, IO<Unit>> write) =>
        read.Bind(value => Bound(new UserInterfaceSlider { Range = range, Value = value }, Changed<UserInterfaceSlider>(static (slider, h) => slider.ValueChanged += h),
            Changed<UserInterfaceSlider>(static (slider, h) => slider.ValueChanged -= h), slider => write(slider.Value)));

    public static IO<UserInterfaceCheckBox> Check(string text, IO<bool> read, Func<bool, IO<Unit>> write) =>
        read.Bind(value => Bound(new UserInterfaceCheckBox(text) { Checked = value }, Changed<UserInterfaceCheckBox>(static (check, h) => check.CheckedChanged += h),
            Changed<UserInterfaceCheckBox>(static (check, h) => check.CheckedChanged -= h), check => write(check.Checked)));

    public static IO<UserInterfaceTextBox> Text(IO<string> read, Func<string, IO<Unit>> commit) =>
        read.Bind(value => Bound(new UserInterfaceTextBox(value), Changed<UserInterfaceTextBox>(static (box, h) => box.TextCommitted += h),
            Changed<UserInterfaceTextBox>(static (box, h) => box.TextCommitted -= h), box => commit(box.Text)));

    public static UserInterfaceGrid Grid(Option<string> heading, Seq<(string Label, UserInterfaceControl Control)> rows) {
        UserInterfaceGrid grid = new(columnCount: 2);
        grid.SetColumnAutoWidth(0);
        _ = heading.Iter(text => grid.AddHeading(text));
        _ = rows.Iter(row => {
            _ = grid.AddLabel(row.Label);
            grid.AddChild(row.Control);
        });
        return grid;
    }

    private static Action<TControl, EventHandler<EventArgs>> Changed<TControl>(Action<TControl, EventHandler> attach) =>
        (control, handler) => attach(control, handler.Invoke);

    private static IO<TControl> Bound<TControl>(TControl control, Action<TControl, EventHandler<EventArgs>> add, Action<TControl, EventHandler<EventArgs>> remove, Func<TControl, IO<Unit>> changed) =>
        Subscriptions.On<EventArgs>(handler => add(control, handler), handler => remove(control, handler)).Attach(_ => changed(control)).Map(_ => control);

    // --- [LEASE]
    public static IO<Unit> Show(RhinoDoc doc, string name, Seq<UserInterfaceObjectBase> items) =>
        Uuid.NewNameBased(Session.PlugIn.Id, name) switch {
            var groupId =>
                from cleared in IO.lift(() => doc.ViewUserInterface.RemoveByGroupId(groupId))
                from released in Products.Released(() => ignore(doc.ViewUserInterface.RemoveByGroupId(groupId)))
                from added in Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: true),
                    IO.lift(() => Products.Each(items, (item, index) => RhinoFault.Unless(doc.ViewUserInterface.Add(item, groupId), RhinoReason.Declined, item, index))))
                select unit,
        };

    public static IO<Unit> Show(Seq<UserInterfaceObjectBase> items) =>
        items.TraverseM(static item => use(IO.lift(() => RhinoFault.Unless(item.RegisterForAllDocuments(), RhinoReason.Declined, item).Map(_ => item)),
            static held => IO.lift(held.Unregister))).As().Map(unit);

    // --- [MESSAGES]
    public static IO<Seq<uint>> Toast(RhinoDoc doc, ViewportSet views, string message) =>
        Viewports.Resolve(doc, views).Bind(rows => IO.lift(() =>
            toSeq(rows.Map(static row => row.View).DistinctBy(static view => view.RuntimeSerialNumber)).Map(view => view.ShowToast(message)).Strict())).Bracket();
}
