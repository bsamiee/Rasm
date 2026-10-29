using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using UnitsNet;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class AnimationQuality {
    public static readonly AnimationQuality Draft = new("preview", renderFull: false, renderPreview: false);
    public static readonly AnimationQuality RenderedPreview = new("full", renderFull: false, renderPreview: true);
    public static readonly AnimationQuality Rendered = new("full", renderFull: true, renderPreview: false);

    internal string CaptureMethod { get; }

    internal bool RenderFull { get; }

    internal bool RenderPreview { get; }
}

[Union]
public abstract partial record AnimationPath {
    public sealed record FromCurve(Guid CurveId) : AnimationPath;

    public sealed record FromPoints(Seq<Point3d> Points) : AnimationPath;

    internal Seq<Action<AnimationProperties>> Written(Action<AnimationProperties, Guid> curve, Action<AnimationProperties, Point3d[]> points) =>
        Switch(
            (Curve: curve, Points: points),
            fromCurve: static (write, path) => Seq<Action<AnimationProperties>>(target => write.Curve(target, path.CurveId), target => write.Points(target, [])),
            fromPoints: static (write, path) => Seq<Action<AnimationProperties>>(target => write.Curve(target, Guid.Empty), target => write.Points(target, [.. path.Points])));

    internal static AnimationPath Of(Guid curve, Point3d[] points) =>
        Conversions.Present(curve).Match<AnimationPath>(Some: static id => new FromCurve(id), None: () => new FromPoints(toSeq(points)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnimationKind {
    public sealed record Turntable(Seq<Point3d> Camera, Seq<Point3d> Target) : AnimationKind {
        public static Turntable Orbit(RhinoViewport viewport, int frames) =>
            new(toSeq(Enumerable.Range(0, frames)).Map(frame => Transform.Rotation(frame * Math.Tau / frames, Vector3d.ZAxis, viewport.CameraTarget) * viewport.CameraLocation).Strict(),
                toSeq(Enumerable.Repeat(viewport.CameraTarget, frames)));
    }

    public sealed record CameraAndTarget(AnimationPath Camera, AnimationPath Target) : AnimationKind;

    public sealed record Flythrough(AnimationPath Path) : AnimationKind;

    public sealed record Sun(GeoCoordinate Place, Angle North, SunWindow Window) : AnimationKind;
}

public sealed record AnimationOutput(FolderPath Folder, FileExtension Extension, NamePart Name) {
    internal static Fin<AnimationOutput> From(string folder, string extension, string name) =>
        (FolderPath.Of(folder), FileExtension.Validated($"{FileExtension.Mark}{extension}"), NamePart.Validated(name))
            .Apply(static (existing, dotted, stem) => new AnimationOutput(existing, dotted, stem))
            .As();
}

public sealed record AnimationSequence(AnimationKind Kind, int Frames, string ViewportName, Option<Guid> DisplayMode, Option<AnimationOutput> Output, AnimationQuality Quality);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Animations {
    // --- [DOCUMENT]
    public static IO<Option<AnimationSequence>> Read(RhinoDoc doc) =>
        Products.Of(() => doc.AnimationProperties).Bind(static held => IO.lift(() =>
            Kind(held).Bind(found => found.Traverse(kind => Sequence(held, kind)).As()))).Bracket();

    public static IO<Unit> Write(RhinoDoc doc, AnimationSequence sequence) =>
        Products.Of(() => doc.AnimationProperties).Bind(held => IO.lift(() => {
            _ = Written(sequence).Iter(write => write(held));
            doc.AnimationProperties = held;
        })).Bracket();

    internal static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        Products.Of(() => doc.AnimationProperties).Bind(held => IO.lift(() => moves.Find(held.DisplayMode).Iter(target => {
            held.DisplayMode = target;
            doc.AnimationProperties = held;
        }))).Bracket();

    private static Fin<Option<AnimationKind>> Kind(AnimationProperties held) =>
        held.CaptureType switch {
            AnimationProperties.CaptureTypes.Turntable => Some<AnimationKind>(new AnimationKind.Turntable(toSeq(held.CameraPoints), toSeq(held.TargetPoints))),
            AnimationProperties.CaptureTypes.Path => Some<AnimationKind>(new AnimationKind.CameraAndTarget(
                AnimationPath.Of(held.CameraPathId, held.CameraPoints), AnimationPath.Of(held.TargetPathId, held.TargetPoints))),
            AnimationProperties.CaptureTypes.Flythrough => Some<AnimationKind>(new AnimationKind.Flythrough(AnimationPath.Of(held.CameraPathId, held.CameraPoints))),
            _ => SunWindow.Read(held).Bind(window => window.Traverse(sun => GeoCoordinate.From(held.Latitude, held.Longitude)
                .Map(place => (AnimationKind)new AnimationKind.Sun(place, Angle.FromDegrees(Quantities.Exact(held.NorthAngle)), sun))).As()),
        };

    private static Fin<AnimationSequence> Sequence(AnimationProperties held, AnimationKind kind) =>
        (Conversions.Present(held.FolderName).Traverse(folder => AnimationOutput.From(folder, held.FileExtension, held.AnimationName)).As(),
         toSeq(AnimationQuality.Items).Find(quality => (quality.RenderFull, quality.RenderPreview) == (held.RenderFull, held.RenderPreview))
             .ToFin(new RhinoFault($"animation capture (full, preview) = ({held.RenderFull}, {held.RenderPreview})", RhinoReason.Missing)))
            .Apply((output, quality) => new AnimationSequence(kind, held.FrameCount, held.ViewportName, Conversions.Present(held.DisplayMode), output, quality))
            .As();

    private static Seq<Action<AnimationProperties>> Written(AnimationSequence sequence) =>
        Seq<Action<AnimationProperties>>(
            target => target.FrameCount = sequence.Frames,
            target => target.ViewportName = sequence.ViewportName,
            target => target.DisplayMode = Conversions.Unset(sequence.DisplayMode),
            target => target.CaptureMethod = sequence.Quality.CaptureMethod,
            target => target.RenderFull = sequence.Quality.RenderFull,
            target => target.RenderPreview = sequence.Quality.RenderPreview)
        + sequence.Output.ToSeq().Bind(static output => Seq<Action<AnimationProperties>>(
            target => target.FolderName = output.Folder,
            target => target.FileExtension = ((string)output.Extension).TrimStart(FileExtension.Mark),
            target => target.AnimationName = output.Name))
        + Written(sequence.Kind);

    private static Seq<Action<AnimationProperties>> Written(AnimationKind kind) =>
        kind.Switch(
            turntable: static orbit => Seq<Action<AnimationProperties>>(static target => target.CaptureType = AnimationProperties.CaptureTypes.Turntable)
                + Camera(new AnimationPath.FromPoints(orbit.Camera)) + Target(new AnimationPath.FromPoints(orbit.Target)),
            cameraAndTarget: static paths => Seq<Action<AnimationProperties>>(static target => target.CaptureType = AnimationProperties.CaptureTypes.Path)
                + Camera(paths.Camera) + Target(paths.Target),
            flythrough: static flight => Seq<Action<AnimationProperties>>(static target => target.CaptureType = AnimationProperties.CaptureTypes.Flythrough) + Camera(flight.Path),
            sun: static sun => Seq(
                target => target.Latitude = sun.Place.Latitude.Degrees.ToDouble(),
                target => target.Longitude = sun.Place.Longitude.Degrees.ToDouble(),
                target => target.NorthAngle = sun.North.Degrees.ToDouble(),
                sun.Window.Write));

    private static Seq<Action<AnimationProperties>> Camera(AnimationPath path) =>
        path.Written(static (target, id) => target.CameraPathId = id, static (target, points) => target.CameraPoints = points);

    private static Seq<Action<AnimationProperties>> Target(AnimationPath path) =>
        path.Written(static (target, id) => target.TargetPathId = id, static (target, points) => target.TargetPoints = points);
}
