using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.UI.Gumball;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display.Feedback;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
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

[Union]
public abstract partial record GumballUpdate {
    public sealed record Ray(Point3d Point, Line WorldLine) : GumballUpdate;

    public sealed record Frame(Plane Plane) : GumballUpdate;
}

public sealed record GumballTransforms(Transform TotalTransform, Transform GumballTransform, Transform PreTransform, bool InRelocate, GumballFrame Frame, GumballFrame BaseFrame);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class GumballMapper {
    [MapProperty(nameof(@GumballDisplayConduit.Gumball.Frame), nameof(GumballTransforms.Frame), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(@GumballDisplayConduit.BaseGumball.Frame), nameof(GumballTransforms.BaseFrame), SuppressNullMismatchDiagnostic = true)]
    internal static partial GumballTransforms ToTransforms(GumballDisplayConduit conduit);
}

public static class Gumballs {
    // --- [SCOPE]
    public static IO<A> Placed<A>(GumballSource source, ActiveSpace space, Option<GumballAppearanceSettings> appearance, Func<GumballDisplayConduit, IO<A>> body) =>
        (from conduit in use(() => new GumballDisplayConduit(space), static conduit => {
            conduit.Enabled = false;
            conduit.Dispose();
        })
         from _ in Place(conduit, source, appearance)
         from answer in body(conduit)
         select answer).Bracket();

    private static IO<Unit> Place(GumballDisplayConduit conduit, GumballSource source, Option<GumballAppearanceSettings> appearance) =>
        (from gumball in use(static () => new GumballObject())
         from _ in IO.lift(() => SetFrom(gumball, source))
         from placed in IO.lift(() => {
             conduit.SetBaseGumball(gumball, appearance.ValueUnsafe());
             conduit.Enabled = true;
         })
         select placed).Bracket();

    private static Fin<Unit> SetFrom(GumballObject gumball, GumballSource source) =>
        source.Switch(
            gumball,
            fromBoundingBox: static (target, box) => Refused.Unless(target.SetFromBoundingBox(box.Box), nameof(GumballObject.SetFromBoundingBox)),
            fromFramedBoundingBox: static (target, box) => Refused.Unless(target.SetFromBoundingBox(box.Frame, box.Box), nameof(GumballObject.SetFromBoundingBox)),
            fromLine: static (target, line) => Refused.Unless(target.SetFromLine(line.Line), nameof(GumballObject.SetFromLine)),
            fromPlane: static (target, plane) => Refused.Unless(target.SetFromPlane(plane.Plane), nameof(GumballObject.SetFromPlane)),
            fromArc: static (target, arc) => Refused.Unless(target.SetFromArc(arc.Arc), nameof(GumballObject.SetFromArc)),
            fromCircle: static (target, circle) => Refused.Unless(target.SetFromCircle(circle.Circle), nameof(GumballObject.SetFromCircle)),
            fromEllipse: static (target, ellipse) => Refused.Unless(target.SetFromEllipse(ellipse.Ellipse), nameof(GumballObject.SetFromEllipse)),
            fromCurve: static (target, curve) => Refused.Unless(target.SetFromCurve(curve.Curve), nameof(GumballObject.SetFromCurve)),
            fromExtrusion: static (target, extrusion) => Refused.Unless(target.SetFromExtrusion(extrusion.Extrusion), nameof(GumballObject.SetFromExtrusion)),
            fromLight: static (target, light) => Refused.Unless(target.SetFromLight(light.Light), nameof(GumballObject.SetFromLight)),
            fromHatch: static (target, hatch) => Refused.Unless(target.SetFromHatch(hatch.Hatch), nameof(GumballObject.SetFromHatch)));

    // --- [DRAG]
    public static IO<Option<GumballMode>> Pick(GumballDisplayConduit conduit, PickContext context, Option<GetPoint> point) =>
        IO.lift(() => Callbacks.Found(conduit.PickGumball(context, point.ValueUnsafe()), conduit.PickResult.Mode));

    public static IO<Option<GumballTransforms>> Update(GumballDisplayConduit conduit, GumballUpdate update) =>
        IO.lift(() => Callbacks.Found(
                update.Switch(
                    conduit,
                    ray: static (target, ray) => target.UpdateGumball(ray.Point, ray.WorldLine),
                    frame: static (target, frame) => target.UpdateGumball(frame.Plane)),
                conduit)
            .Map(GumballMapper.ToTransforms));

    public static IO<GumballTransforms> Transforms(GumballDisplayConduit conduit) =>
        IO.lift(() => GumballMapper.ToTransforms(conduit));
}
