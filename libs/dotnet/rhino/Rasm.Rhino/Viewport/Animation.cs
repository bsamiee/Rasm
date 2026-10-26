using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;
using UnitsNet;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class AnimationQuality {
    public static readonly AnimationQuality Draft = new("preview", renderFull: false, renderPreview: false);
    public static readonly AnimationQuality RenderedPreview = new("RenderPreview", renderFull: false, renderPreview: true);
    public static readonly AnimationQuality Rendered = new("RenderFull", renderFull: true, renderPreview: false);

    internal string CaptureMethod { get; }

    internal bool RenderFull { get; }

    internal bool RenderPreview { get; }
}

[Union]
public abstract partial record AnimationPath {
    public sealed record FromCurve(Guid CurveId) : AnimationPath;

    public sealed record FromPoints(Seq<Point3d> Points) : AnimationPath;
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
        (FolderPath.Of(folder),
         FileExtension.TryCreate($"{FileExtension.Mark}{extension}", out FileExtension? dotted, out RhinoFault? undotted) ? Fin.Succ(dotted) : undotted,
         NamePart.TryCreate(name, out NamePart? part, out RhinoFault? unnamed) ? Fin.Succ(part) : unnamed)
            .Apply(static (existing, extension, stem) => new AnimationOutput(existing, extension, stem))
            .As();
}

public sealed record AnimationSequence(AnimationKind Kind, int Frames, string ViewportName, Option<Guid> DisplayMode, Option<AnimationOutput> Output, AnimationQuality Quality);

public sealed record AnimationRecord(AnimationSequence Sequence, int CurrentFrame, Seq<string> Images, Seq<string> Dates);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class Animations {
    public static IO<Option<AnimationRecord>> Read(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.AnimationProperties, static copy => IO.lift(() =>
            Kind(copy).Bind(found => found.Traverse(kind => Sequence(copy, kind).Map(sequence => ToRecord(copy, sequence))).As())));

    public static IO<Unit> Write(RhinoDoc doc, AnimationSequence sequence) =>
        DisposalOps.Within(() => doc.AnimationProperties, copy => IO.lift(() => {
            Update(sequence, copy);
            _ = sequence.Output.IfSome(output => Update(output, copy));
            Update(sequence.Kind, copy);
            Update(sequence.Quality, copy);
            doc.AnimationProperties = copy;
        }));

    internal static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        DisposalOps.Within(() => doc.AnimationProperties, copy => IO.lift(() => moves.Find(copy.DisplayMode).Iter(target => {
            copy.DisplayMode = target;
            doc.AnimationProperties = copy;
        })));

    private static Fin<Option<AnimationKind>> Kind(AnimationProperties copy) =>
        copy.CaptureType switch {
            AnimationProperties.CaptureTypes.Turntable => Some<AnimationKind>(new AnimationKind.Turntable(toSeq(copy.CameraPoints), toSeq(copy.TargetPoints))),
            AnimationProperties.CaptureTypes.Path => Some<AnimationKind>(new AnimationKind.CameraAndTarget(Traced(copy.CameraPathId, copy.CameraPoints), Traced(copy.TargetPathId, copy.TargetPoints))),
            AnimationProperties.CaptureTypes.Flythrough => Some<AnimationKind>(new AnimationKind.Flythrough(Traced(copy.CameraPathId, copy.CameraPoints))),
            _ => SunWindow.Read(copy).Map(window => window.Map(held =>
                (AnimationKind)new AnimationKind.Sun(GeoCoordinate.From(copy.Latitude, copy.Longitude), Angle.FromDegrees(copy.NorthAngle), held))),
        };

    private static Fin<AnimationSequence> Sequence(AnimationProperties copy, AnimationKind kind) =>
        (Conversions.Present(copy.FolderName).Traverse(folder => AnimationOutput.From(folder, copy.FileExtension, copy.AnimationName)).As(),
         Quality((copy.RenderFull, copy.RenderPreview)))
            .Apply((output, quality) => new AnimationSequence(kind, copy.FrameCount, copy.ViewportName, Conversions.Present(copy.DisplayMode), output, quality))
            .As();

    private static Fin<AnimationQuality> Quality((bool Full, bool Preview) held) =>
        toSeq(AnimationQuality.Items).Find(quality => (quality.RenderFull, quality.RenderPreview) == held)
            .ToFin(Error.New(ErrorCodes.ValidationFailed, $"animation capture with full {held.Full} and preview {held.Preview} is no quality"));

    private static AnimationPath Traced(Guid curve, Point3d[] points) =>
        Conversions.Present(curve).Match<AnimationPath>(Some: static id => new AnimationPath.FromCurve(id), None: () => new AnimationPath.FromPoints(toSeq(points)));

    private static (Guid Curve, Point3d[] Points) Stored(AnimationPath path) =>
        path.Switch<(Guid Curve, Point3d[] Points)>(fromCurve: static curve => (curve.CurveId, []), fromPoints: static points => (Guid.Empty, [.. points.Points]));

    private static void Update(AnimationKind kind, AnimationProperties properties) =>
        kind.Switch(
            properties,
            turntable: static (target, orbit) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Turntable;
                (target.CameraPathId, target.CameraPoints) = (Guid.Empty, [.. orbit.Camera]);
                (target.TargetPathId, target.TargetPoints) = (Guid.Empty, [.. orbit.Target]);
            },
            cameraAndTarget: static (target, paths) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Path;
                (target.CameraPathId, target.CameraPoints) = Stored(paths.Camera);
                (target.TargetPathId, target.TargetPoints) = Stored(paths.Target);
            },
            flythrough: static (target, flight) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Flythrough;
                (target.CameraPathId, target.CameraPoints) = Stored(flight.Path);
            },
            sun: static (target, sun) => {
                (target.Latitude, target.Longitude, target.NorthAngle) = (sun.Place.Latitude.Degrees.ToDouble(), sun.Place.Longitude.Degrees.ToDouble(), sun.North.Degrees.ToDouble());
                SunMapper.Update(sun.Window, target);
            });

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(AnimationProperties.Images), nameof(AnimationRecord.Images), Use = nameof(@Conversions.Rows))]
    [MapProperty(nameof(AnimationProperties.Dates), nameof(AnimationRecord.Dates), Use = nameof(@Conversions.Rows))]
    private static partial AnimationRecord ToRecord(AnimationProperties properties, AnimationSequence sequence);

    [MapProperty(nameof(AnimationSequence.Frames), nameof(AnimationProperties.FrameCount))]
    [MapProperty(nameof(AnimationSequence.Output), nameof(AnimationProperties.FolderName), Use = nameof(Folder))]
    [MapperIgnoreSource(nameof(AnimationSequence.Kind), Justification = "Written per case through Switch")]
    [MapperIgnoreSource(nameof(AnimationSequence.Quality), Justification = "Written through the quality update")]
    private static partial void Update(AnimationSequence sequence, AnimationProperties properties);

    [MapProperty(nameof(AnimationOutput.Extension), nameof(AnimationProperties.FileExtension), Use = nameof(Undotted))]
    [MapProperty(nameof(AnimationOutput.Name), nameof(AnimationProperties.AnimationName))]
    [MapperIgnoreSource(nameof(AnimationOutput.Folder), Justification = "Written through the sequence update")]
    private static partial void Update(AnimationOutput output, AnimationProperties properties);

    private static partial void Update(AnimationQuality quality, AnimationProperties properties);

    [UserMapping(Default = false)]
    private static string Folder(Option<AnimationOutput> output) => Conversions.Unset(output.Map(static held => (string)held.Folder));

    [UserMapping(Default = false)]
    private static string Undotted(FileExtension extension) => ((string)extension).TrimStart(FileExtension.Mark);

    [UserMapping]
    private static string Text(NamePart name) => name;
}
