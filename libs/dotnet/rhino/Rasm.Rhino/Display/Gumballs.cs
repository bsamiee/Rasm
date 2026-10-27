using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.UI.Gumball;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union<Ray, Plane>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class GumballUpdate {
    public sealed record Ray(Point3d Point, Line WorldLine);
}

public sealed record GumballTransforms(Transform GumballTransform, Transform PreTransform, bool InRelocate, GumballFrame Frame, GumballFrame BaseFrame) {
    public Transform TotalTransform => GumballTransform * PreTransform;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Gumballs {
    // --- [SCOPE]
    public static IO<T> Placed<T>(Func<GumballObject, Fin<Unit>> initialize, ActiveSpace space, Option<GumballAppearanceSettings> appearance, Func<GumballDisplayConduit, IO<T>> body) =>
        (from conduit in use(() => new GumballDisplayConduit(space), static conduit => {
            conduit.Enabled = false;
            conduit.Dispose();
        })
         from placed in (from gumball in use(static () => new GumballObject())
                         from accepted in IO.lift(() => initialize(gumball))
                         from enabled in IO.lift(() => {
                             conduit.SetBaseGumball(gumball, appearance.ValueUnsafe());
                             conduit.Enabled = true;
                         })
                         select enabled).Bracket()
         from answer in body(conduit)
         select answer).Bracket();

    // --- [DRAG]
    public static IO<Option<GumballMode>> Pick(GumballDisplayConduit conduit, PickContext context, Option<GetPoint> point) =>
        from reset in IO.lift(conduit.PickResult.SetToDefault)
        from picked in IO.lift(() => Conversions.Found(conduit.PickGumball(context, point.ValueUnsafe()), conduit.PickResult.Mode))
        select picked;

    public static IO<Option<GumballTransforms>> Update(GumballDisplayConduit conduit, GumballUpdate update) =>
        from modifiers in IO.lift(conduit.CheckShiftAndControlKeys)
        from changed in IO.lift(() => update.Switch(
            conduit,
            ray: static (target, ray) => target.UpdateGumball(ray.Point, ray.WorldLine),
            plane: static (target, frame) => target.UpdateGumball(frame)))
        from snapshot in changed ? Transforms(conduit).Map(Some) : IO.pure(Option<GumballTransforms>.None)
        select snapshot;

    public static IO<GumballTransforms> Transforms(GumballDisplayConduit conduit) =>
        IO.lift(() => Read(conduit, conduit.Gumball.Frame, conduit.BaseGumball.Frame));

    private static partial GumballTransforms Read(GumballDisplayConduit source, GumballFrame frame, GumballFrame baseFrame);
}
