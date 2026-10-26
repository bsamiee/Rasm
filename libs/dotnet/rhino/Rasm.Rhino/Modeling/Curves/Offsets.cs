using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Surfaces;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record OffsetFrame {
    public sealed record InPlane(Plane Plane, CurveOffsetCornerStyle Corner) : OffsetFrame;
    public sealed record ByNormal(Point3d DirectionPoint, Vector3d Normal, bool Loose, CurveOffsetCornerStyle Corner, CurveOffsetEndStyle End) : OffsetFrame;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SurfaceOffset {
    public sealed record Constant(double Distance) : SurfaceOffset;
    public sealed record Through(Point2d Point) : SurfaceOffset;
    public sealed record Varying(Seq<(double Parameter, double Distance)> Stations) : SurfaceOffset;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Ribbon {
    public sealed record Ruled(Curve Offset, Seq<Curve> CrossSections, Seq<Surface> RuledSurfaces) : Ribbon;
    public sealed record Swept(Curve Offset, Seq<Curve> Rails, Seq<Curve> CrossSections, Seq<Brep> BrepSurfaces) : Ribbon;

    public Seq<GeometryBase> Parts => Switch<Seq<GeometryBase>>(
        ruled: static ribbon => [ribbon.Offset, .. ribbon.CrossSections, .. ribbon.RuledSurfaces],
        swept: static ribbon => [ribbon.Offset, .. ribbon.Rails, .. ribbon.CrossSections, .. ribbon.BrepSurfaces]);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveOffsets {
    // --- [OFFSETS]
    public static IO<Seq<Curve>> Offset(Tolerances tolerances, Curve curve, OffsetFrame frame, double distance, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.Offset), _ => Answer.Many(frame.Switch(
            (Curve: curve, Distance: distance, Tolerances: tolerances),
            inPlane: static (state, plane) => state.Curve.Offset(plane.Plane, state.Distance, state.Tolerances.Absolute, plane.Corner),
            byNormal: static (state, normal) => state.Curve.Offset(normal.DirectionPoint, normal.Normal, state.Distance, state.Tolerances.Absolute, state.Tolerances.Angle, normal.Loose, normal.Corner, normal.End))), retain);

    public static IO<Seq<Curve>> OffsetOnSurface(Tolerances tolerances, Curve curve, SurfaceTarget target, SurfaceOffset offset, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.OffsetOnSurface), _ => Answer.Many(target.Switch(
            (Curve: curve, Offset: offset, Fitting: tolerances.Absolute),
            brepFace: static (state, face) => state.Offset.Switch(
                (state.Curve, Face: face, state.Fitting),
                constant: static (at, constant) => at.Curve.OffsetOnSurface(at.Face, constant.Distance, at.Fitting),
                through: static (at, through) => at.Curve.OffsetOnSurface(at.Face, through.Point, at.Fitting),
                varying: static (at, varying) => at.Curve.OffsetOnSurface(at.Face,
                    [.. varying.Stations.Map(static station => station.Parameter)], [.. varying.Stations.Map(static station => station.Distance)], at.Fitting)),
            surface: static (state, surface) => state.Offset.Switch(
                (state.Curve, Surface: surface, state.Fitting),
                constant: static (at, constant) => at.Curve.OffsetOnSurface(at.Surface, constant.Distance, at.Fitting),
                through: static (at, through) => at.Curve.OffsetOnSurface(at.Surface, through.Point, at.Fitting),
                varying: static (at, varying) => at.Curve.OffsetOnSurface(at.Surface,
                    [.. varying.Stations.Map(static station => station.Parameter)], [.. varying.Stations.Map(static station => station.Distance)], at.Fitting)))), retain);

    public static IO<Curve> OffsetNormalToSurface(Curve curve, Surface surface, double height, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.OffsetNormalToSurface), _ => Answer.One(curve.OffsetNormalToSurface(surface, height)), retain);

    public static IO<Curve> OffsetTangentToSurface(Curve curve, Surface surface, double height, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.OffsetTangentToSurface), _ => Answer.One(curve.OffsetTangentToSurface(surface, height)), retain);

    // --- [RIBBONS]
    public static IO<Ribbon> RibbonOffset(Curve curve, RibbonOffsetParameters parameters, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.RibbonOffset), _ => parameters.RibbonSurfaceGenerationMethod is RibbonOffsetSurfaceMethod.None
            ? (Answer.One(curve.RibbonOffset(parameters.OffsetDistance, parameters.BlendRadius, parameters.OffsetLocation, parameters.OffsetPlaneVector3d, parameters.OffsetTolerance, out Curve[] sections, out Surface[] surfaces)),
                    Answer.Many(sections), Answer.Many(surfaces))
                .Apply(static (offset, crossSections, ruledSurfaces) => (Ribbon)new Ribbon.Ruled(offset, crossSections, ruledSurfaces)).As()
            : (Answer.One(curve.RibbonOffset(parameters, out Curve[] rails, out Curve[] crossSections, out Brep[] breps)), Answer.Many(rails), Answer.Many(crossSections), Answer.Many(breps))
                .Apply(static (offset, railCurves, sectionCurves, brepSurfaces) => (Ribbon)new Ribbon.Swept(offset, railCurves, sectionCurves, brepSurfaces)).As(), retain);
}
