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

    internal (Guid Curve, Point3d[] Points) Stored =>
        Switch<(Guid Curve, Point3d[] Points)>(fromCurve: static path => (path.CurveId, []), fromPoints: static path => (Guid.Empty, [.. path.Points]));

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

public sealed record AnimationOutput(FolderPath Folder, FileExtension Extension, NamePart Name);

public sealed record AnimationSequence(AnimationKind Kind, int Frames, string ViewportName, Option<Guid> DisplayMode, Option<AnimationOutput> Output, AnimationQuality Quality);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Animations {
    public static IO<Option<AnimationSequence>> Read(RhinoDoc doc) =>
        use(Products.Of(() => doc.AnimationProperties)).Bind(static held => IO.lift(Kind(held).Bind(found => found.Traverse(kind => Sequence(held, kind)).As()))).Bracket();

    public static IO<Unit> Write(RhinoDoc doc, AnimationSequence sequence) =>
        use(Products.Of(() => doc.AnimationProperties)).Bind(held => IO.lift(() => {
            (held.FrameCount, held.ViewportName, held.DisplayMode) = (sequence.Frames, sequence.ViewportName, Conversions.Unset(sequence.DisplayMode));
            (held.CaptureMethod, held.RenderFull, held.RenderPreview) = (sequence.Quality.CaptureMethod, sequence.Quality.RenderFull, sequence.Quality.RenderPreview);
            sequence.Output.Iter(output => (held.FolderName, held.FileExtension, held.AnimationName) = (output.Folder, ((string)output.Extension).TrimStart(FileExtension.Mark), output.Name));
            sequence.Kind.Switch(
                held,
                turntable: static (target, orbit) => (target.CaptureType, (target.CameraPathId, target.CameraPoints), (target.TargetPathId, target.TargetPoints)) =
                    (AnimationProperties.CaptureTypes.Turntable, new AnimationPath.FromPoints(orbit.Camera).Stored, new AnimationPath.FromPoints(orbit.Target).Stored),
                cameraAndTarget: static (target, paths) => (target.CaptureType, (target.CameraPathId, target.CameraPoints), (target.TargetPathId, target.TargetPoints)) =
                    (AnimationProperties.CaptureTypes.Path, paths.Camera.Stored, paths.Target.Stored),
                flythrough: static (target, flight) => (target.CaptureType, (target.CameraPathId, target.CameraPoints)) = (AnimationProperties.CaptureTypes.Flythrough, flight.Path.Stored),
                sun: static (target, sun) => {
                    (target.Latitude, target.Longitude, target.NorthAngle) = (sun.Place.Latitude.Degrees.ToDouble(), sun.Place.Longitude.Degrees.ToDouble(), sun.North.Degrees.ToDouble());
                    sun.Window.Write(target);
                });
            doc.AnimationProperties = held;
        })).Bracket();

    internal static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        use(Products.Of(() => doc.AnimationProperties)).Bind(held => IO.lift(() => moves.Find(held.DisplayMode).Iter(target => {
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
        (Conversions.Present(held.FolderName).Traverse(folder =>
                (FolderPath.Of(folder), FileExtension.Validated($"{FileExtension.Mark}{held.FileExtension}"), NamePart.Validated(held.AnimationName))
                    .Apply(static (existing, dotted, stem) => new AnimationOutput(existing, dotted, stem))
                    .As()).As(),
         toSeq(AnimationQuality.Items).Find(quality => (quality.RenderFull, quality.RenderPreview) == (held.RenderFull, held.RenderPreview))
             .ToFin(new RhinoFault($"animation capture (full, preview) = ({held.RenderFull}, {held.RenderPreview})", RhinoReason.Missing)))
            .Apply((output, quality) => new AnimationSequence(kind, held.FrameCount, held.ViewportName, Conversions.Present(held.DisplayMode), output, quality))
            .As();
}
