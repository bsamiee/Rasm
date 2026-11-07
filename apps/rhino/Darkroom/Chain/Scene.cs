using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.DocObjects;

namespace Darkroom.Chain;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ChainScene {

    // --- [STATE]

    public static ChainScene Unframed { get; } = new();

    public static ColorEncoding Working { get; } = new(Gamut.StandardRgb, new Transfer.Linear());

    public static DisplayDevice Display { get; } = DisplayDevice.Srgb;

    public Option<Camera> Camera { get; init; }
    public Option<SceneLights> Lights { get; init; }
    public Option<float> Meters { get; init; }
    public Option<Duration> Time { get; init; }
    public Option<Length> FocalLength { get; init; }

    // --- [CONTEXT]

    public PassContext Context(PixelExtent extent, HashMap<GuideChannel, PixelFrame> guides, Derivations derivations, Option<OutputDepth> depth, GaussLegendreRule integration) =>
        new(extent, Working.Gamut, Display, depth, Working.Transfer, 0d, Camera, guides, Time, Lights, derivations, integration);

    // --- [HOSTS]

    public static IO<ChainScene> Sourced(RhinoDoc doc, PixelExtent extent, Option<Duration> time) =>
        (from source in use(() => new Rhino.Render.RenderSourceView(doc))
         from view in Products.Of(source.GetViewInfo)
         from meters in IO.lift(() => Quantities.From(1d, doc.ModelUnits).Meters.ToDouble())
         from viewport in use(() => new ViewportInfo(view.Viewport) { FrustumAspect = (double)extent.Width / extent.Height })
         from sun in use(() => doc.Lights.Sun)
         from scene in IO.lift((Viewed(view, viewport, meters, extent), Lit(doc, sun.Enabled ? Some(-sun.Vector) : None, meters)).Apply((camera, lights) => new ChainScene {
             Camera = Some(camera), Lights = Some(lights), Meters = Some((float)meters), Time = time,
             FocalLength = viewport.IsParallelProjection ? None : Some(Length.FromMillimeters(viewport.Camera35mmLensLength)),
         }).As())
         select scene).Bracket();

    private static Fin<Camera> Viewed(ViewInfo view, ViewportInfo viewport, double meters, PixelExtent extent) =>
        (Pose(viewport.GetXform(CoordinateSystem.Camera, CoordinateSystem.World), meters), Window(viewport, meters))
            .Apply((pose, window) => new Camera(
                pose,
                viewport.IsParallelProjection ? new Projection.Orthographic(extent, window) : new Projection.Rectilinear(extent, window),
                view.FocalBlurMode is ViewInfoFocalBlurModes.Manual && !viewport.IsParallelProjection
                    ? LensFocus.Validated(Length.FromMillimeters(viewport.Camera35mmLensLength), Length.FromMeters(view.FocalBlurDistance * meters), view.FocalBlurAperture).ToOption()
                    : None))
            .As();

    private static Fin<ViewWindow> Window(ViewportInfo viewport, double meters) =>
        DarkroomFault.Unless(viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out _), DarkroomReason.Invalid, viewport.Id)
            .Bind(_ => (float)(viewport.IsParallelProjection ? meters : 1d / near) switch {
                var scale => ViewWindow.Validated(new Vector2((float)(left + right) / 2f, (float)(bottom + top) / 2f) * scale, new Vector2((float)(right - left), (float)(top - bottom)) * scale),
            });

    private static Fin<Matrix4x4> Pose(Transform xform, double meters) =>
        (xform with { M03 = xform.M03 * meters, M13 = xform.M13 * meters, M23 = xform.M23 * meters }).ToFloatArray(rowDominant: false) switch {
            var entries => DarkroomFault.Unless(xform.IsValid && entries.All(float.IsFinite), DarkroomReason.Invalid, xform).Map(_ => MemoryMarshal.Cast<float, Matrix4x4>(entries.AsSpan())[0]),
        };

    private static Fin<SceneLights> Lit(RhinoDoc doc, Option<Vector3d> sun, double meters) =>
        Conversions.Rows(doc.Lights).Filter(static light => !light.IsDeleted && light.LightGeometry.IsEnabled) switch {
            var lights => (
                    sun.Traverse(Numeric).As(),
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
        DarkroomFault.Unless(vector.IsValid && Seq(vector.X, vector.Y, vector.Z).ForAll(static value => double.Abs(value) <= float.MaxValue), DarkroomReason.Invalid, vector)
            .Map(_ => new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z));
}
