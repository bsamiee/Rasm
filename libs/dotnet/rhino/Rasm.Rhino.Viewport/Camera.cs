using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] ---------------------------------------------------------------------------
public enum CameraProjection { Parallel = 0, Perspective = 1, TwoPoint = 2 }

public enum Staleness { Fresh = 0, Reopened = 1, Modified = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CameraPose(Plane Frame, Point3d Target, double CameraAngle, CameraProjection Projection);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportQuery {
    public sealed record AtPoint(Point3d Point) : ViewportQuery;

    public sealed record OfBounds(BoundingBox Bounds) : ViewportQuery;

    public sealed record OfSphere(Sphere Sphere) : ViewportQuery;

    public sealed record OfGeometry(GeometryBase Geometry) : ViewportQuery;
}

public sealed record DepthExtent(double Near, double Far);

public sealed record CameraFrustum(double Left, double Right, double Bottom, double Top, double Near, double Far, double Aspect, BoundingBox Bounds);

public sealed record FocalBlur(ViewInfoFocalBlurModes FocalBlurMode, double FocalBlurDistance, double FocalBlurAperture, double FocalBlurJitter, uint FocalBlurSampleCount);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DetailLength {
    public sealed record Paper(double Length) : DetailLength;

    public sealed record Model(double Length) : DetailLength;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CameraSnapshot(ViewportInfo viewport, Guid viewportId, uint changeCounter, uint documentSerial) : IDisposable {
    public ViewportInfo Viewport { get; } = viewport;

    public Guid ViewportId { get; } = viewportId;

    public uint ChangeCounter { get; } = changeCounter;

    public uint DocumentSerial { get; } = documentSerial;

    public void Dispose() => Viewport.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cameras {
    // --- [POSE]
    public static IO<CameraPose> ReadPose(RhinoViewport viewport) =>
        from frame in IO.lift(() => Refused.Unless(viewport.GetCameraFrame(out Plane read), read, nameof(RhinoViewport.GetCameraFrame)))
        from target in IO.lift(() => viewport.CameraTarget)
        from angle in IO.lift(() => Some(viewport.CameraAngle).Filter(static half => half > 0.0).ToFin(new Refused(nameof(RhinoViewport.CameraAngle))))
        from projection in ReadProjection(viewport)
        select new CameraPose(frame, target, angle, projection);

    public static IO<uint> WritePose(RhinoViewport viewport, CameraPose pose) =>
        from actual in ReadProjection(viewport)
        from same in IO.lift(() => ProjectionMismatch.Unless(pose.Projection, actual))
        from written in IO.lift(() => {
            viewport.SetCameraLocations(pose.Target, pose.Frame.Origin);
            viewport.SetCameraDirection(-pose.Frame.ZAxis, updateTargetLocation: false);
            viewport.CameraUp = pose.Frame.YAxis;
            viewport.CameraAngle = pose.CameraAngle;
        })
        from counter in IO.lift(() => viewport.ChangeCounter)
        select counter;

    public static IO<ViewportInfo> ToViewportInfo(CameraPose pose, BoundingBox bounds) =>
        from bounded in IO.lift(() => Invalid.Unless(bounds.IsValid, nameof(BoundingBox.IsValid)))
        from viewport in IO.lift(static () => new ViewportInfo())
        from placed in GeometryOps.OnFailure(
            from aimed in IO.lift(() =>
                from located in Refused.Unless(viewport.SetCameraLocation(pose.Frame.Origin), nameof(ViewportInfo.SetCameraLocation))
                from directed in Refused.Unless(viewport.SetCameraDirection(-pose.Frame.ZAxis), nameof(ViewportInfo.SetCameraDirection))
                from upright in Refused.Unless(viewport.SetCameraUp(pose.Frame.YAxis), nameof(ViewportInfo.SetCameraUp))
                select unit)
            from projected in pose.Projection switch {
                CameraProjection.Parallel => IO.lift(() => Refused.Unless(viewport.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection))),
                CameraProjection.Perspective => Lensed(viewport, pose).Bind(lens => IO.lift(() => Refused.Unless(
                    viewport.ChangeToPerspectiveProjection(pose.Frame.Origin.DistanceTo(pose.Target), symmetricFrustum: true, lens),
                    nameof(ViewportInfo.ChangeToPerspectiveProjection)))),
                CameraProjection.TwoPoint => Lensed(viewport, pose).Bind(lens => IO.lift(() => Refused.Unless(
                    viewport.ChangeToTwoPointPerspectiveProjection(pose.Frame.Origin.DistanceTo(pose.Target), pose.Frame.YAxis, lens),
                    nameof(ViewportInfo.ChangeToTwoPointPerspectiveProjection)))),
            }
            from clipped in IO.lift(() => Refused.Unless(viewport.SetFrustumNearFar(bounds), nameof(ViewportInfo.SetFrustumNearFar)))
            select viewport,
            IO.lift(viewport.Dispose))
        select placed;

    private static IO<double> Lensed(ViewportInfo viewport, CameraPose pose) =>
        IO.lift(() => {
            viewport.CameraAngle = pose.CameraAngle;
            return viewport.Camera35mmLensLength;
        });

    private static IO<CameraProjection> ReadProjection(RhinoViewport viewport) =>
        IO.lift(() => viewport switch {
            { IsTwoPointPerspectiveProjection: true } => CameraProjection.TwoPoint,
            { IsPerspectiveProjection: true } => CameraProjection.Perspective,
            _ => CameraProjection.Parallel,
        });

    // --- [FRUSTUM]
    public static IO<CameraFrustum> GetFrustum(RhinoViewport viewport) =>
        from frustum in IO.lift(() => Refused.Unless(
            viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far),
            new CameraFrustum(left, right, bottom, top, near, far, viewport.FrustumAspect, viewport.GetFrustumBoundingBox()),
            nameof(RhinoViewport.GetFrustum)))
        from bounded in IO.lift(() => Refused.Unless(frustum.Bounds.IsValid, nameof(RhinoViewport.GetFrustumBoundingBox)))
        select frustum;

    public static IO<Transform> Xform(RhinoViewport viewport, CoordinateSystem source, CoordinateSystem destination) =>
        Disposal.Using(() => new ViewportInfo(viewport), info => IO.lift(() => Some(info.GetXform(source, destination)).Filter(static transform => transform.IsValid).ToFin(new Refused(nameof(ViewportInfo.GetXform)))));

    public static IO<double> GetWorldToScreenScale(RhinoViewport viewport, Point3d at) =>
        IO.lift(() => Refused.Unless(viewport.GetWorldToScreenScale(at, out double pixelsPerUnit) && (pixelsPerUnit > 0.0), pixelsPerUnit, nameof(RhinoViewport.GetWorldToScreenScale)));

    public static IO<Line> GetFrustumLine(RhinoViewport viewport, double screenX, double screenY) =>
        IO.lift(() => Refused.Unless(viewport.GetFrustumLine(screenX, screenY, out Line line), line, nameof(RhinoViewport.GetFrustumLine)));

    public static IO<ConstructionPlane> GetConstructionPlane(RhinoViewport viewport) =>
        IO.lift(() => Optional(viewport.GetConstructionPlane()).Filter(static cplane => cplane.Plane.IsValid).ToFin(new Refused(nameof(RhinoViewport.GetConstructionPlane))));

    // --- [QUERIES]
    public static IO<DepthExtent> GetDepth(RhinoViewport viewport, ViewportQuery query) =>
        query.Switch(
            viewport,
            atPoint: static (port, at) => IO.lift(() => Refused.Unless(port.GetDepth(at.Point, out double distance), new DepthExtent(distance, distance), nameof(RhinoViewport.GetDepth))),
            ofBounds: static (port, of) => BoxDepth(port, of.Bounds),
            ofSphere: static (port, of) => IO.lift(() => Refused.Unless(port.GetDepth(of.Sphere, out double near, out double far), new DepthExtent(near, far), nameof(RhinoViewport.GetDepth))),
            ofGeometry: static (port, of) => GeometryOps.Bounds(of.Geometry, new BoundsFrame.AxisAligned(Accurate: false)).Bind(box => BoxDepth(port, box)));

    public static IO<bool> IsVisible(RhinoViewport viewport, ViewportQuery query) =>
        query.Switch(
            viewport,
            atPoint: static (port, at) => IO.lift(() => port.IsVisible(at.Point)),
            ofBounds: static (port, of) => IO.lift(() => port.IsVisible(of.Bounds)),
            ofSphere: static (port, of) => IO.lift(() => port.IsVisible(of.Sphere.BoundingBox)),
            ofGeometry: static (port, of) => GeometryOps.Bounds(of.Geometry, new BoundsFrame.AxisAligned(Accurate: false)).Map(port.IsVisible));

    private static IO<DepthExtent> BoxDepth(RhinoViewport viewport, BoundingBox box) =>
        IO.lift(() => Refused.Unless(viewport.GetDepth(box, out double near, out double far), new DepthExtent(near, far), nameof(RhinoViewport.GetDepth)));

    // --- [FOCAL_BLUR]
    public static IO<FocalBlur> ReadFocalBlur(RhinoDoc document, string namedView) =>
        WithNamedView(document, namedView, static view => IO.lift(() => CameraMapper.ToFocalBlur(view)));

    public static IO<int> WriteFocalBlur(RhinoDoc document, string namedView, FocalBlur value) =>
        WithNamedView(document, namedView, view => IO.lift(() => {
            CameraMapper.Update(value, view);
            return Answers.NonNegative(document.NamedViews.Add(view), nameof(NamedViewTable.Add));
        }));

    private static IO<TValue> WithNamedView<TValue>(RhinoDoc document, string name, Func<ViewInfo, IO<TValue>> body) =>
        Disposal.Using(
            NamedViews.Resolve(document, name).Bind(index => IO.lift(() => Missing.Unless(document.NamedViews[index], nameof(NamedViewTable)))),
            body);

    // --- [DETAIL]
    public static IO<DetailLength> ConvertDetailLength(DetailViewObject detail, DetailLength length) =>
        IO.lift(() => length.Switch(
            detail,
            paper: static (view, paper) =>
                Refused.Unless<DetailLength>(view.TryGetModelLength(paper.Length, out double model), new DetailLength.Model(model), nameof(DetailViewObject.TryGetModelLength)),
            model: static (view, model) =>
                Refused.Unless<DetailLength>(view.TryGetPaperLength(model.Length, out double paper), new DetailLength.Paper(paper), nameof(DetailViewObject.TryGetPaperLength))));

    // --- [SNAPSHOTS]
    public static IO<CameraSnapshot> Snapshot(RhinoDoc document, RhinoViewport viewport) =>
        IO.lift(() => new CameraSnapshot(new ViewportInfo(viewport), viewport.Id, viewport.ChangeCounter, document.RuntimeSerialNumber));

    public static IO<Staleness> Stale(RhinoDoc document, RhinoViewport viewport, CameraSnapshot snapshot) =>
        IO.lift(() => document.RuntimeSerialNumber != snapshot.DocumentSerial
            ? Staleness.Reopened
            : (viewport.ChangeCounter != snapshot.ChangeCounter ? Staleness.Modified : Staleness.Fresh));

    public static IO<uint> Restore(RhinoDoc document, RhinoViewport viewport, CameraSnapshot snapshot) =>
        from same in IO.lift(() => Reopened.Unless(snapshot.DocumentSerial, document.RuntimeSerialNumber))
        from restored in IO.lift(() => Refused.Unless(viewport.SetViewProjection(snapshot.Viewport, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
        from counter in IO.lift(() => viewport.ChangeCounter)
        select counter;
}

[Mapper]
internal static partial class CameraMapper {
    internal static partial FocalBlur ToFocalBlur(ViewInfo view);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(FocalBlur value, ViewInfo view);
}
