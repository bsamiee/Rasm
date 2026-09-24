using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.UI.Gumball;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballSource {
    public sealed record Bounds(BoundingBox Box, Option<Plane> Frame) : GumballSource;

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballUpdate {
    public sealed record Ray(Point3d Point, Line WorldLine) : GumballUpdate;

    public sealed record Frame(Plane Plane) : GumballUpdate;
}

public sealed record GumballTransforms(Transform TotalTransform, Transform GumballTransform, Transform PreTransform, bool InRelocate, GumballFrame Frame);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GumballHandle : IDisposable {
    private readonly GumballObject gumball;

    private readonly GumballDisplayConduit conduit;

    private readonly Disposal disposal;

    private GumballHandle(GumballObject gumball, GumballDisplayConduit conduit) {
        this.gumball = gumball;
        this.conduit = conduit;
        disposal = new(() => {
            conduit.Enabled = false;
            conduit.Dispose();
            gumball.Dispose();
        });
    }

    public static IO<GumballHandle> Enable(GumballSource source, ActiveSpace space, Option<GumballAppearanceSettings> appearance) =>
        IO.lift(static () => new GumballObject()).Bind(gumball => GeometryOps.OnFailure(
            from set in IO.lift(() => SetFrom(gumball, source))
            from conduit in IO.lift(() => new GumballDisplayConduit(space))
            from enabled in GeometryOps.OnFailure(
                IO.lift(() => {
                    conduit.SetBaseGumball(gumball, appearance.ValueUnsafe());
                    conduit.Enabled = true;
                }),
                IO.lift(conduit.Dispose))
            select new GumballHandle(gumball, conduit),
            IO.lift(gumball.Dispose)));

    public IO<GumballMode> Pick(PickContext context, Option<GetPoint> point) =>
        IO.lift(() => Refused.Unless(conduit.PickGumball(context, point.ValueUnsafe()), nameof(GumballDisplayConduit.PickGumball)).Map(_ => conduit.PickResult.Mode));

    public IO<Unit> Update(GumballUpdate update) =>
        IO.lift(() => update.Switch(
            conduit,
            ray: static (target, ray) => Refused.Unless(target.UpdateGumball(ray.Point, ray.WorldLine), nameof(GumballDisplayConduit.UpdateGumball)),
            frame: static (target, frame) => Refused.Unless(target.UpdateGumball(frame.Plane), nameof(GumballDisplayConduit.UpdateGumball))));

    public IO<GumballTransforms> Transforms() =>
        IO.lift(() => GumballMapper.ToTransforms(conduit));

    public IO<Unit> CheckShiftAndControlKeys() =>
        IO.lift(conduit.CheckShiftAndControlKeys);

    public void Dispose() => disposal.Dispose();

    private static Fin<Unit> SetFrom(GumballObject gumball, GumballSource source) =>
        source.Switch(
            gumball,
            bounds: static (target, bounds) => Refused.Unless(
                bounds.Frame.Match(Some: frame => target.SetFromBoundingBox(frame, bounds.Box), None: () => target.SetFromBoundingBox(bounds.Box)),
                nameof(GumballObject.SetFromBoundingBox)),
            fromLine: static (target, line) => Refused.Unless(target.SetFromLine(line.Line), nameof(GumballObject.SetFromLine)),
            fromPlane: static (target, plane) => Refused.Unless(target.SetFromPlane(plane.Plane), nameof(GumballObject.SetFromPlane)),
            fromArc: static (target, arc) => Refused.Unless(target.SetFromArc(arc.Arc), nameof(GumballObject.SetFromArc)),
            fromCircle: static (target, circle) => Refused.Unless(target.SetFromCircle(circle.Circle), nameof(GumballObject.SetFromCircle)),
            fromEllipse: static (target, ellipse) => Refused.Unless(target.SetFromEllipse(ellipse.Ellipse), nameof(GumballObject.SetFromEllipse)),
            fromCurve: static (target, curve) => Refused.Unless(target.SetFromCurve(curve.Curve), nameof(GumballObject.SetFromCurve)),
            fromExtrusion: static (target, extrusion) => Refused.Unless(target.SetFromExtrusion(extrusion.Extrusion), nameof(GumballObject.SetFromExtrusion)),
            fromLight: static (target, light) => Refused.Unless(target.SetFromLight(light.Light), nameof(GumballObject.SetFromLight)),
            fromHatch: static (target, hatch) => Refused.Unless(target.SetFromHatch(hatch.Hatch), nameof(GumballObject.SetFromHatch)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class GumballMapper {
    [MapProperty(nameof(@GumballDisplayConduit.Gumball.Frame), nameof(GumballTransforms.Frame), SuppressNullMismatchDiagnostic = true)]
    internal static partial GumballTransforms ToTransforms(GumballDisplayConduit conduit);
}
