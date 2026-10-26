using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino;
using Rasm.Rhino.Render.Effects;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChainScene {
    private const double SharpAperture = 0.00001d;

    public static ChainScene Unframed { get; } = new();

    public Option<Camera> Camera { get; init; }
    public Option<SceneLights> Lights { get; init; }
    public Option<float> Meters { get; init; }
    public Option<TimeSpan> Time { get; init; }
    public Option<Length> FocalLength { get; init; }
    public Option<float> Aperture { get; init; }

    // --- [CONTEXT]
    public PassContext Context(PixelExtent extent, HashMap<GuideChannel, PixelFrame> guides, Derivations derivations, bool rendering, Option<OutputDepth> depth) =>
        new(extent, DisplayFormation.Working, DisplayFormation.Display, depth, new Transfer.Linear(), 0d, Camera, guides, Time, Lights, derivations, rendering);

    public IO<HashMap<GuideChannel, PixelFrame>> Guides(Seq<GuideChannel> channels, Func<GuideChannel, IO<PixelFrame>> read) =>
        channels.TraverseM(channel => read(channel).Map(frame => (channel, Meters.Map(meters => channel.Measured(frame, meters)).IfNone(frame)))).As()
            .Map(static frames => toHashMap(frames));

    // --- [HOST]
    public static IO<PixelFrame> Frame(Func<Guid, PostEffectChannel?> open, RenderWindow.StandardChannels channel, Rectangle rect, PixelExtent extent) =>
        from pixels in Channels.Read(open, channel, rect)
        from size in IO.lift(() => PixelExtent.Validated(rect.Width, rect.Height))
        select new PixelFrame(rect.Location, size, extent, block => pixels.AsSpan().CopyTo(block));

    public static IO<ChainScene> Read(RhinoDoc doc, ViewInfo view, PixelExtent extent, Option<TimeSpan> time) =>
        from meters in IO.lift(() => LengthUnit.Scale(doc.ModelUnits, LengthUnit.Meters))
        from optics in IO.lift(() => Optics(view))
        from camera in IO.lift(() => Viewed(view, meters, extent, Focus(view, meters, optics.FocalLength, optics.Aperture)))
        from lights in IO.lift(() => Lit(doc, meters))
        select new ChainScene { Camera = Some(camera), Lights = Some(lights), Meters = Some((float)meters), Time = time, FocalLength = optics.FocalLength, Aperture = optics.Aperture };

    private static (Option<Length> FocalLength, Option<float> Aperture) Optics(ViewInfo view) =>
        view.Viewport.IsParallelProjection ? (None, None)
            : (Length.FromMillimeters(view.Viewport.Camera35mmLensLength),
                view.FocalBlurMode is ViewInfoFocalBlurModes.Manual && view.FocalBlurAperture >= SharpAperture ? Some((float)view.FocalBlurAperture) : None);

    private static Option<LensFocus> Focus(ViewInfo view, double meters, Option<Length> focal, Option<float> aperture) =>
        from length in aperture.IsSome ? focal : None
        from lens in LensFocus.Validate(length, Length.FromMeters(view.FocalBlurDistance * meters), out LensFocus focus) switch { null => Some(focus), _ => None }
        select lens;

    private static Fin<Camera> Viewed(ViewInfo view, double meters, PixelExtent extent, Option<LensFocus> lens) =>
        (view.Viewport.IsParallelProjection, view.Viewport.GetXform(CoordinateSystem.Camera, CoordinateSystem.World)) switch {
            var (parallel, xform) => (
                    Refused.Unless(xform.IsValid, xform, nameof(ViewportInfo.GetXform))
                        .Bind(frame => Pose(frame with { M03 = frame.M03 * meters, M13 = frame.M13 * meters, M23 = frame.M23 * meters })),
                    Window(view.Viewport, parallel, meters))
                .Apply((pose, window) => new Camera(pose, parallel ? new Frustum.Parallel(window) : new Frustum.Perspective(window), extent, lens))
                .As(),
        };

    private static Fin<ViewWindow> Window(ViewportInfo viewport, bool parallel, double meters) {
        if (!viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out _))
            return new Refused(nameof(ViewportInfo.GetFrustum), Refused.Declined);
        double scale = parallel ? meters : 1d / near;
        return ViewWindow.Validate(scale * ((left + right) / 2d), scale * ((bottom + top) / 2d), scale * (right - left), scale * (top - bottom), out ViewWindow held) switch {
            null => held,
            var fault => fault,
        };
    }

    private static Fin<Matrix4x4> Pose(Transform xform) =>
        xform.ToFloatArray(rowDominant: false) is var entries && entries.All(float.IsFinite)
            ? MemoryMarshal.Cast<float, Matrix4x4>(entries.AsSpan())[0]
            : new Refused(nameof(Transform.ToFloatArray), Refused.Invalid);

    private static Fin<SceneLights> Lit(RhinoDoc doc, double meters) =>
        Conversions.Rows(doc.Lights).Filter(static light => !light.IsDeleted && light.LightGeometry.IsEnabled) switch {
            var lights => (
                    (doc.Lights.Sun is { Enabled: true } sun ? Some(-sun.Vector) : None).Traverse(Numeric).As(),
                    (lights.Filter(static light => !light.LightGeometry.IsDirectionalLight).Map(static light => (light.Id, light.LightGeometry.Location))
                        + Conversions.Rows(doc.Objects.GetObjectsByType<PointObject>()).Map(static point => (point.Id, point.PointGeometry.Location))
                        + Conversions.Rows(doc.Objects.GetObjectsByType<InstanceObject>()).Map(static block => (block.Id, Location: block.InsertionPoint)))
                    .Traverse(row => Numeric((Vector3d)(row.Location * meters)).Map(position => (row.Id, position))).As(),
                    lights.Filter(static light => light.LightGeometry.IsDirectionalLight)
                        .Traverse(static light => Numeric(-light.LightGeometry.Direction).Map(direction => (light.Id, direction))).As())
                .Apply(static (sun, points, directions) => new SceneLights(sun, toHashMap(points), toHashMap(directions)))
                .As(),
        };

    private static Fin<Vector3> Numeric(Vector3d vector) =>
        Seq(vector.X, vector.Y, vector.Z).ForAll(static value => double.IsFinite(value) && double.Abs(value) <= float.MaxValue)
            ? new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z)
            : new Refused(nameof(Vector3d), Refused.Invalid);
}
