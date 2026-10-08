using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.UI.Gumball;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display.Feedback;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record GumballSource {
    public sealed record FromBoundingBox(BoundingBox Box) : GumballSource;
    public sealed record FromFramedBoundingBox(Plane Frame, BoundingBox Box) : GumballSource;
    public sealed record FromLine(Line Line) : GumballSource;
    public sealed record FromPlane(Plane Plane) : GumballSource;
    public sealed record FromArc(Arc Arc) : GumballSource;
    public sealed record FromCircle(Circle Circle) : GumballSource;
    public sealed record FromEllipse(Ellipse Ellipse) : GumballSource;
    public sealed record FromCurve(Curve Curve) : GumballSource;
    public sealed record FromExtrusion(Extrusion Extrusion) : GumballSource;
    public sealed record FromLight(Light Light) : GumballSource;
    public sealed record FromHatch(Hatch Hatch) : GumballSource;
}

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
    public static IO<A> Placed<A>(GumballSource source, ActiveSpace space, Option<GumballAppearanceSettings> appearance, Func<GumballDisplayConduit, IO<A>> body) =>
        (from conduit in use(() => new GumballDisplayConduit(space), static conduit => {
            conduit.Enabled = false;
            conduit.Dispose();
        })
         from placed in (
             from gumball in use(static () => new GumballObject())
             from initialized in IO.lift(() => source.Switch(
                 gumball,
                 fromBoundingBox: static (target, box) => (Accepted: target.SetFromBoundingBox(box.Box), Member: nameof(GumballObject.SetFromBoundingBox)),
                 fromFramedBoundingBox: static (target, box) => (Accepted: target.SetFromBoundingBox(box.Frame, box.Box), Member: nameof(GumballObject.SetFromBoundingBox)),
                 fromLine: static (target, line) => (Accepted: target.SetFromLine(line.Line), Member: nameof(GumballObject.SetFromLine)),
                 fromPlane: static (target, plane) => (Accepted: target.SetFromPlane(plane.Plane), Member: nameof(GumballObject.SetFromPlane)),
                 fromArc: static (target, arc) => (Accepted: target.SetFromArc(arc.Arc), Member: nameof(GumballObject.SetFromArc)),
                 fromCircle: static (target, circle) => (Accepted: target.SetFromCircle(circle.Circle), Member: nameof(GumballObject.SetFromCircle)),
                 fromEllipse: static (target, ellipse) => (Accepted: target.SetFromEllipse(ellipse.Ellipse), Member: nameof(GumballObject.SetFromEllipse)),
                 fromCurve: static (target, curve) => (Accepted: target.SetFromCurve(curve.Curve), Member: nameof(GumballObject.SetFromCurve)),
                 fromExtrusion: static (target, extrusion) => (Accepted: target.SetFromExtrusion(extrusion.Extrusion), Member: nameof(GumballObject.SetFromExtrusion)),
                 fromLight: static (target, light) => (Accepted: target.SetFromLight(light.Light), Member: nameof(GumballObject.SetFromLight)),
                 fromHatch: static (target, hatch) => (Accepted: target.SetFromHatch(hatch.Hatch), Member: nameof(GumballObject.SetFromHatch))))
             from admitted in IO.lift(Refused.Unless(initialized.Accepted, initialized.Member))
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
        from picked in IO.lift(() => Callbacks.Found(conduit.PickGumball(context, point.ValueUnsafe()), conduit.PickResult.Mode))
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
