using Rasm.Rhino.Document.Units;

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

[SmartEnum]
public sealed partial class LiftDirection {
    public static readonly LiftDirection Normal =
        new(static (curve, surface, height, retain) => Copies.Acquire(() => curve.OffsetNormalToSurface(surface, height), nameof(Curve.OffsetNormalToSurface), retain));

    public static readonly LiftDirection Tangent =
        new(static (curve, surface, height, retain) => Copies.Acquire(() => curve.OffsetTangentToSurface(surface, height), nameof(Curve.OffsetTangentToSurface), retain));

    [UseDelegateFromConstructor]
    public partial IO<Curve> Offset(Curve curve, Surface surface, double height, Action<IDisposable> retain);
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
    public static IO<Seq<Curve>> Offset(Curve curve, OffsetFrame frame, double distance, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Acquire(
            () => frame.Switch(
                (Curve: curve, Distance: distance, Tolerances: tolerances),
                inPlane: static (state, plane) => state.Curve.Offset(plane.Plane, state.Distance, state.Tolerances.Absolute, plane.Corner),
                byNormal: static (state, normal) => state.Curve.Offset(normal.DirectionPoint, normal.Normal, state.Distance, state.Tolerances.Absolute, state.Tolerances.Angle, normal.Loose, normal.Corner, normal.End)),
            nameof(Curve.Offset), retain);

    public static IO<Seq<Curve>> OffsetOnSurface(Curve curve, BrepFace face, SurfaceOffset offset, Tolerances tolerances, Action<IDisposable> retain) =>
        offset.Switch(
            (Curve: curve, Face: face, Fitting: tolerances.Absolute, Retain: retain),
            constant: static (state, constant) => Copies.Acquire(
                () => state.Curve.OffsetOnSurface(state.Face, constant.Distance, state.Fitting),
                nameof(Curve.OffsetOnSurface), state.Retain),
            through: static (state, through) => Copies.Acquire(
                () => state.Curve.OffsetOnSurface(state.Face, through.Point, state.Fitting),
                nameof(Curve.OffsetOnSurface), state.Retain),
            varying: static (state, varying) => (Parameters: varying.Stations.Map(static station => station.Parameter).ToArray(), Distances: varying.Stations.Map(static station => station.Distance).ToArray()) switch {
                var stations => Copies.Acquire(
                    () => state.Curve.OffsetOnSurface(state.Face, stations.Parameters, stations.Distances, state.Fitting), nameof(Curve.OffsetOnSurface), state.Retain),
            });

    public static IO<Seq<Curve>> OffsetOnSurface(Curve curve, Surface surface, SurfaceOffset offset, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Acquire(
            () => offset.Switch(
                (Curve: curve, Surface: surface, Fitting: tolerances.Absolute),
                constant: static (state, constant) => state.Curve.OffsetOnSurface(state.Surface, constant.Distance, state.Fitting),
                through: static (state, through) => state.Curve.OffsetOnSurface(state.Surface, through.Point, state.Fitting),
                varying: static (state, varying) => state.Curve.OffsetOnSurface(state.Surface,
                    [.. varying.Stations.Map(static station => station.Parameter)], [.. varying.Stations.Map(static station => station.Distance)], state.Fitting)),
            nameof(Curve.OffsetOnSurface), retain);

    // --- [RIBBONS]
    public static IO<Ribbon> RibbonOffset(Curve curve, RibbonOffsetParameters parameters, Action<IDisposable> retain) =>
        Copies.Owned(
            () => parameters.RibbonSurfaceGenerationMethod is RibbonOffsetSurfaceMethod.None
                ? Missing.Unless(
                        curve.RibbonOffset(parameters.OffsetDistance, parameters.BlendRadius, parameters.OffsetLocation, parameters.OffsetPlaneVector3d, parameters.OffsetTolerance, out Curve[] sections, out Surface[] surfaces),
                        nameof(Curve.RibbonOffset))
                    .Map<Ribbon>(offset => new Ribbon.Ruled(offset, Conversions.Rows(sections), Conversions.Rows(surfaces)))
                : Missing.Unless(curve.RibbonOffset(parameters, out Curve[] rails, out Curve[] crossSections, out Brep[] breps), nameof(Curve.RibbonOffset))
                    .Map<Ribbon>(offset => new Ribbon.Swept(offset, Conversions.Rows(rails), Conversions.Rows(crossSections), Conversions.Rows(breps))),
            static answer =>
                from ribbon in answer
                from valid in Measurements.Valid(toSeq<GeometryBase?>(ribbon.Parts), nameof(Curve.RibbonOffset))
                select ribbon,
            static answer => answer.Match(Succ: static ribbon => DisposalOps.Release(ribbon.Parts), Fail: static _ => IO.pure(unit)), retain);

    // --- [PROJECTIONS]
    public static IO<Seq<(Curve Result, (Curve Source, Brep Target) Row)>> ProjectToBrep(Seq<Curve> curves, Seq<Brep> breps, Vector3d direction, bool loose, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Acquire<Curve, (Curve Source, Brep Target)>(
            () => (
                Curve.ProjectToBrep(curves, breps, direction, tolerances.Absolute, loose, out int[]? curveIndices, out int[]? brepIndices),
                (curveIndices, brepIndices) is ( { } sources, { } targets) ? [.. sources.Zip(targets, (source, target) => (Source: curves[source], Target: breps[target]))] : null),
            nameof(Curve.ProjectToBrep), retain);
}
