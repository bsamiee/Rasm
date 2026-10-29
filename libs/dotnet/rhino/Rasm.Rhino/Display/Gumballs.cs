using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Display;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class GumballGetter : GetTransform {
    private readonly GumballDisplayConduit conduit;

    internal GumballGetter(GumballDisplayConduit conduit, string prompt, Seq<RhinoObject> objects) {
        this.conduit = conduit;
        SetCommandPrompt(prompt);
        _ = objects.Iter(ObjectList.Add);
    }

    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) => conduit.InRelocate ? conduit.PreTransform : conduit.GumballTransform;

    protected override void OnMouseDown(GetPointMouseEventArgs e) {
        if (conduit.PickResult.Mode is not GumballMode.None)
            return;
        conduit.PickResult.SetToDefault();
        using PickContext pick = new() { View = e.Viewport.ParentView, PickStyle = PickStyle.PointPick };
        pick.SetPickTransform(e.Viewport.GetPickTransform(e.WindowPoint));
        if (e.Viewport.GetFrustumLine(e.WindowPoint.X, e.WindowPoint.Y, out Line line))
            pick.PickLine = line;
        pick.UpdateClippingPlanes();
        _ = conduit.PickGumball(pick, this);
    }

    protected override void OnMouseMove(GetPointMouseEventArgs e) {
        if (conduit.PickResult.Mode is GumballMode.None)
            return;
        conduit.CheckShiftAndControlKeys();
        if (conduit.UpdateGumball(e.Point, e.Viewport.GetFrustumLine(e.WindowPoint.X, e.WindowPoint.Y, out Line line) ? line : Line.Unset))
            base.OnMouseMove(e);
    }

    protected override void OnDynamicDraw(GetPointDrawEventArgs e) {
        if (conduit.PickResult.Mode is not GumballMode.None)
            base.OnDynamicDraw(e);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Gumballs {
    // --- [DRAG]
    public static IO<Transform> Drag(
        RhinoDoc doc, Func<GumballObject, bool> placed, GumballAppearanceSettings appearance, string prompt, Seq<RhinoObject> objects, Func<Transform, IO<Unit>> dragged) =>
        (from conduit in use(static () => new GumballDisplayConduit(ActiveSpace.ModelSpace), static conduit => {
            conduit.Enabled = false;
            conduit.Dispose();
        })
         from gumball in use(static () => new GumballObject())
         from set in Products.Accepted(() => placed(gumball))
         from total in Monad.recur(Transform.Identity, held =>
                 from shown in Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: false), IO.lift(() => {
                     conduit.SetBaseGumball(gumball, appearance);
                     conduit.Enabled = true;
                 }))
                 from answer in use(() => new GumballGetter(conduit, prompt, objects)).Map(static getter => (Result: getter.Get(onMouseUp: true), Command: getter.CommandResult())).Bracket()
                 from hidden in IO.lift(() => { conduit.Enabled = false; })
                 from next in answer is { Command: Result.Success, Result: GetResult.Point }
                     ? Moved(conduit, gumball, dragged).Map(moved => Next.Loop<Transform, Transform>(moved * held))
                     : IO.pure(Next.Done<Transform, Transform>(held))
                 select next).As()
         select total).Bracket();

    private static IO<Transform> Moved(GumballDisplayConduit conduit, GumballObject gumball, Func<Transform, IO<Unit>> dragged) =>
        from moved in IO.lift(() => conduit.InRelocate ? Transform.Identity : conduit.TotalTransform)
        from applied in when(!conduit.InRelocate, dragged(moved)).As()
        from reset in IO.lift(() => {
            GumballFrame frame = gumball.Frame;
            (frame.Plane, frame.ScaleGripDistance) = (conduit.Gumball.Frame.Plane, conduit.Gumball.Frame.ScaleGripDistance);
            gumball.Frame = frame;
        })
        select moved;
}
