using System.Globalization;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FrameCount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"frame count {value} is not positive"));
}

[SmartEnum]
public sealed partial class AnimationQuality {
    public static readonly AnimationQuality Draft = new("preview", renderFull: false, renderPreview: false);
    public static readonly AnimationQuality Recorded = new("full", renderFull: false, renderPreview: false);
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
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnimationKind {
    public sealed record Turntable() : AnimationKind;

    public sealed record CameraAndTarget(AnimationPath Camera, AnimationPath Target) : AnimationKind;

    public sealed record Flythrough(AnimationPath Path) : AnimationKind;

    public sealed record Sun(GeoCoordinate Place, NorthAngle North, SunWindow Window) : AnimationKind;
}

public sealed record AnimationOutput {
    private AnimationOutput(string folder, FileExtension extension, NamePart name) => (Folder, Extension, Name) = (folder, extension, name);

    public string Folder { get; }

    public FileExtension Extension { get; }

    public NamePart Name { get; }

    public static Fin<AnimationOutput> Create(string folder, FileExtension extension, NamePart name) =>
        Exchange.QualifiedPath(folder).Map(qualified => new AnimationOutput(qualified, extension, name));

    internal static Fin<AnimationOutput> From(string folder, string extension, string name) =>
        (Exchange.QualifiedPath(folder),
         FileExtension.Validated($"{FileExtension.Mark}{extension}"),
         NamePart.Validated(name))
            .Apply(static (qualified, dotted, part) => new AnimationOutput(qualified, dotted, part))
            .As();
}

public sealed record AnimationSequence(AnimationKind Kind, FrameCount Frames, string ViewportName, Option<Guid> DisplayMode, Option<AnimationOutput> Output, AnimationQuality Quality);

public sealed record AnimationRecord(AnimationSequence Sequence, int CurrentFrame, Seq<string> Images, Seq<string> Dates);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class Animations {
    public static IO<Option<AnimationRecord>> Read(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.AnimationProperties, static copy => IO.lift(() =>
            Kind(copy).Bind(found => found.Traverse(kind => Sequence(copy, kind).Map(sequence => ToRecord(copy, sequence))).As())));

    public static IO<Committed<Unit>> Write(RhinoDoc doc, AnimationSequence sequence) =>
        Commits.Commit(doc, nameof(RhinoDoc.AnimationProperties), new RedrawPolicy.Silent(), DisposalOps.Within(() => doc.AnimationProperties, copy => IO.lift(() => {
            Update(sequence, copy);
            _ = sequence.Output.IfSome(output => Update(output, copy));
            Update(sequence.Kind, copy);
            Update(sequence.Quality, copy);
            doc.AnimationProperties = copy;
        })));

    private static Fin<Option<AnimationKind>> Kind(AnimationProperties copy) =>
        copy.CaptureType switch {
            AnimationProperties.CaptureTypes.Turntable => Some<AnimationKind>(new AnimationKind.Turntable()),
            AnimationProperties.CaptureTypes.Path => Some<AnimationKind>(new AnimationKind.CameraAndTarget(Traced(copy.CameraPathId, copy.CameraPoints), Traced(copy.TargetPathId, copy.TargetPoints))),
            AnimationProperties.CaptureTypes.Flythrough => Some<AnimationKind>(new AnimationKind.Flythrough(Traced(copy.CameraPathId, copy.CameraPoints))),
            _ => SunWindow.Read(copy).Bind(window => window.Traverse(held =>
                (GeoCoordinate.From(copy.Latitude, copy.Longitude), NorthAngle.Validated(copy.NorthAngle))
                    .Apply((place, north) => (AnimationKind)new AnimationKind.Sun(place, north, held))
                    .As()).As()),
        };

    private static Fin<AnimationSequence> Sequence(AnimationProperties copy, AnimationKind kind) =>
        (FrameCount.Validated(copy.FrameCount),
         Conversions.Present(copy.FolderName).Traverse(folder => AnimationOutput.From(folder, copy.FileExtension, copy.AnimationName)).As(),
         Quality((copy.CaptureMethod, copy.RenderFull, copy.RenderPreview)))
            .Apply((frames, output, quality) => new AnimationSequence(kind, frames, copy.ViewportName, Conversions.Present(copy.DisplayMode), output, quality))
            .As();

    private static Fin<AnimationQuality> Quality((string Method, bool Full, bool Preview) held) =>
        toSeq(AnimationQuality.Items).Find(quality => (quality.CaptureMethod, quality.RenderFull, quality.RenderPreview) == held)
            .ToFin(Error.New(ErrorCodes.ValidationFailed, $"animation capture {held.Method} with full {held.Full} and preview {held.Preview} is no quality"));

    private static AnimationPath Traced(Guid curve, Point3d[] points) =>
        Conversions.Present(curve).Match<AnimationPath>(Some: static id => new AnimationPath.FromCurve(id), None: () => new AnimationPath.FromPoints(toSeq(points)));

    private static (Guid Curve, Point3d[] Points) Stored(AnimationPath path) =>
        path.Switch<(Guid Curve, Point3d[] Points)>(fromCurve: static curve => (curve.CurveId, []), fromPoints: static points => (Guid.Empty, [.. points.Points]));

    private static void Update(AnimationKind kind, AnimationProperties properties) =>
        kind.Switch(
            properties,
            turntable: static (target, _) => target.CaptureType = AnimationProperties.CaptureTypes.Turntable,
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
                (target.Latitude, target.Longitude, target.NorthAngle) = (sun.Place.Latitude, sun.Place.Longitude, sun.North);
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
    private static string Folder(Option<AnimationOutput> output) => Conversions.Unset(output.Map(static held => held.Folder));

    [UserMapping(Default = false)]
    private static string Undotted(FileExtension extension) => ((string)extension).TrimStart(FileExtension.Mark);

    [UserMapping]
    private static string Text(NamePart name) => name;

    [UserMapping]
    private static int Count(FrameCount frames) => frames;
}
