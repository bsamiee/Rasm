using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Geolocation;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct FrameCount : System.Numerics.IMinMaxValue<FrameCount> {
    public static FrameCount MinValue { get; } = new(1);
    public static FrameCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum]
public sealed partial class AnimationQuality {
    public static readonly AnimationQuality Draft = new("preview", renderFull: false, renderPreview: false);
    public static readonly AnimationQuality Recorded = new("full", renderFull: false, renderPreview: false);
    public static readonly AnimationQuality RenderedPreview = new("full", renderFull: false, renderPreview: true);
    public static readonly AnimationQuality Rendered = new("full", renderFull: true, renderPreview: false);

    public string CaptureMethod { get; }

    public bool RenderFull { get; }

    public bool RenderPreview { get; }
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

    public sealed record Sun(SunSite Site, SunWindow Window) : AnimationKind;
}

public sealed record AnimationOutput {
    private AnimationOutput(string folder, FileExtension extension, NamePart name) => (Folder, Extension, Name) = (folder, extension, name);

    public string Folder { get; }

    public FileExtension Extension { get; }

    public NamePart Name { get; }

    public static Fin<AnimationOutput> Create(string folder, FileExtension extension, NamePart name) =>
        Exchange.QualifiedPath(folder).Map(qualified => new AnimationOutput(qualified, extension, name));

    internal static Fin<AnimationOutput> From(string folder, string extension, string name) =>
        (Exchange.QualifiedPath(folder).ToValidation(),
         Conversions.Validated<FileExtension, string, InvalidRhinoValue>($".{extension}").ToValidation(),
         Conversions.Validated<NamePart, string, InvalidRhinoValue>(name).ToValidation())
            .Apply(static (qualified, dotted, part) => new AnimationOutput(qualified, dotted, part))
            .As()
            .ToFin();
}

public sealed record AnimationSequence(
    AnimationKind Kind,
    FrameCount Frames,
    string ViewportName,
    Option<Guid> DisplayMode,
    Option<AnimationOutput> Output,
    Option<AnimationQuality> Quality);

public sealed record AnimationSettings(
    AnimationProperties.CaptureTypes CaptureType,
    Option<FrameCount> Frames,
    int CurrentFrame,
    Option<string> ViewportName,
    Option<Guid> DisplayMode,
    Option<AnimationOutput> Output,
    Seq<string> Images,
    Seq<string> Dates);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class AnimationMapper {
    internal static void Update(AnimationSequence sequence, AnimationProperties properties) {
        Map(sequence, properties);
        Update(sequence.Kind, properties);
        _ = sequence.DisplayMode.IfSome(id => properties.DisplayMode = id);
        _ = sequence.Output.IfSome(output => Update(output, properties));
        _ = sequence.Quality.IfSome(quality => Update(quality, properties));
    }

    [MapProperty(nameof(AnimationProperties.Images), nameof(AnimationSettings.Images), Use = nameof(@Conversions.Rows))]
    [MapProperty(nameof(AnimationProperties.Dates), nameof(AnimationSettings.Dates), Use = nameof(@Conversions.Rows))]
    internal static partial AnimationSettings ToSettings(AnimationProperties properties, Option<FrameCount> frames, Option<AnimationOutput> output);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(AnimationSequence.Frames), nameof(AnimationProperties.FrameCount))]
    [MapperIgnoreSource(nameof(AnimationSequence.Kind), Justification = "Update writes the kind through its Switch")]
    [MapperIgnoreSource(nameof(AnimationSequence.DisplayMode), Justification = "Update writes a present display mode and keeps the stored id otherwise")]
    [MapperIgnoreSource(nameof(AnimationSequence.Output), Justification = "Update writes a present output and keeps the stored folder otherwise")]
    [MapperIgnoreSource(nameof(AnimationSequence.Quality), Justification = "Update writes a present quality and keeps the stored capture method otherwise")]
    private static partial void Map(AnimationSequence sequence, AnimationProperties properties);

    private static void Update(AnimationKind kind, AnimationProperties properties) =>
        kind.Switch(
            properties,
            turntable: static (target, _) => target.CaptureType = AnimationProperties.CaptureTypes.Turntable,
            cameraAndTarget: static (target, paths) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Path;
                Camera(paths.Camera, target);
                paths.Target.Switch(
                    target,
                    fromCurve: static (host, curve) => host.TargetPathId = curve.CurveId,
                    fromPoints: static (host, points) => host.TargetPoints = [.. points.Points]);
            },
            flythrough: static (target, flight) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Flythrough;
                Camera(flight.Path, target);
            },
            sun: static (target, sun) => {
                SunSiteMapper.Update(sun.Site, target);
                SunSiteMapper.Update(sun.Window, target);
            });

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(AnimationOutput.Folder), nameof(AnimationProperties.FolderName))]
    [MapProperty(nameof(AnimationOutput.Extension), nameof(AnimationProperties.FileExtension), Use = nameof(Undotted))]
    [MapProperty(nameof(AnimationOutput.Name), nameof(AnimationProperties.AnimationName))]
    private static partial void Update(AnimationOutput output, AnimationProperties properties);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    private static partial void Update(AnimationQuality quality, AnimationProperties properties);

    [UserMapping(Default = false)]
    private static string Undotted(FileExtension extension) => ((string)extension)[1..];

    [UserMapping]
    private static int Count(FrameCount frames) => frames;

    [UserMapping]
    private static string Text(NamePart name) => name;

    private static void Camera(AnimationPath path, AnimationProperties properties) =>
        path.Switch(
            properties,
            fromCurve: static (target, curve) => target.CameraPathId = curve.CurveId,
            fromPoints: static (target, points) => target.CameraPoints = [.. points.Points]);
}

public static class Animations {
    public static IO<AnimationSettings> Read(RhinoDoc doc) =>
        use(() => doc.AnimationProperties).Bind(static copy => IO.lift(() => Settings(copy))).Bracket();

    public static IO<Committed<Unit>> Write(RhinoDoc doc, string name, AnimationSequence sequence) =>
        Commits.Commit(doc, name, new RedrawPolicy.Silent(), use(() => doc.AnimationProperties).Bind(copy => IO.lift(() => {
            AnimationMapper.Update(sequence, copy);
            doc.AnimationProperties = copy;
        })).Bracket());

    private static Fin<AnimationSettings> Settings(AnimationProperties copy) =>
        (Callbacks.Found(copy.CaptureType != AnimationProperties.CaptureTypes.None, copy.FrameCount)
            .Traverse(static count => Conversions.Validated<FrameCount, int, InvalidRhinoValue>(count)).As().ToValidation(),
         Conversions.Present(copy.FolderName).Traverse(folder => AnimationOutput.From(folder, copy.FileExtension, copy.AnimationName)).As().ToValidation())
            .Apply((frames, output) => AnimationMapper.ToSettings(copy, frames, output))
            .As()
            .ToFin();
}
