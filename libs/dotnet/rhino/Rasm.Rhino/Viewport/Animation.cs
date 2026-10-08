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
public readonly partial struct FrameCount {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value > 0 ? null : new InvalidRhinoValue();
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
         Conversions.Validated<FileExtension, string, InvalidRhinoValue>($"{FileExtension.Mark}{extension}").ToValidation(),
         Conversions.Validated<NamePart, string, InvalidRhinoValue>(name).ToValidation())
            .Apply(static (qualified, dotted, part) => new AnimationOutput(qualified, dotted, part))
            .As()
            .ToFin();
}

public sealed record AnimationSequence(AnimationKind Kind, FrameCount Frames, string ViewportName, Option<Guid> DisplayMode, Option<AnimationOutput> Output, Option<AnimationQuality> Quality);

public sealed record AnimationSettings(AnimationProperties.CaptureTypes CaptureType, FrameCount Frames, int CurrentFrame, Option<string> ViewportName, Option<Guid> DisplayMode, Option<AnimationOutput> Output, Seq<string> Images, Seq<string> Dates);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class Animations {
    public static IO<Option<AnimationSettings>> Read(RhinoDoc doc) =>
        use(() => doc.AnimationProperties).Bind(static copy => IO.lift(() => Callbacks.Found(copy.CaptureType != AnimationProperties.CaptureTypes.None, copy).Traverse(Settings).As())).Bracket();

    public static IO<Committed<Unit>> Write(RhinoDoc doc, string name, AnimationSequence sequence) =>
        Commits.Commit(doc, name, new RedrawPolicy.Silent(), use(() => doc.AnimationProperties).Bind(copy => IO.lift(() => {
            Update(sequence, copy);
            Update(sequence.Kind, copy);
            _ = sequence.DisplayMode.IfSome(id => copy.DisplayMode = id);
            _ = sequence.Output.IfSome(output => Update(output, copy));
            _ = sequence.Quality.IfSome(quality => Update(quality, copy));
            doc.AnimationProperties = copy;
        })).Bracket());

    private static Fin<AnimationSettings> Settings(AnimationProperties copy) =>
        (Conversions.Validated<FrameCount, int, InvalidRhinoValue>(copy.FrameCount).ToValidation(),
         Conversions.Present(copy.FolderName).Traverse(folder => AnimationOutput.From(folder, copy.FileExtension, copy.AnimationName)).As().ToValidation())
            .Apply((frames, output) => ToSettings(copy, frames, output))
            .As()
            .ToFin();

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(AnimationProperties.Images), nameof(AnimationSettings.Images), Use = nameof(@Conversions.Rows))]
    [MapProperty(nameof(AnimationProperties.Dates), nameof(AnimationSettings.Dates), Use = nameof(@Conversions.Rows))]
    private static partial AnimationSettings ToSettings(AnimationProperties properties, FrameCount frames, Option<AnimationOutput> output);

    [MapProperty(nameof(AnimationSequence.Frames), nameof(AnimationProperties.FrameCount))]
    [MapperIgnoreSource(nameof(AnimationSequence.Kind), Justification = "Written through its Switch")]
    [MapperIgnoreSource(nameof(AnimationSequence.DisplayMode), Justification = "A present id replaces the stored one")]
    [MapperIgnoreSource(nameof(AnimationSequence.Output), Justification = "A present output replaces the stored folder, extension, and name")]
    [MapperIgnoreSource(nameof(AnimationSequence.Quality), Justification = "A present quality replaces the stored capture method and render flags")]
    private static partial void Update(AnimationSequence sequence, AnimationProperties properties);

    [MapProperty(nameof(AnimationOutput.Folder), nameof(AnimationProperties.FolderName))]
    [MapProperty(nameof(AnimationOutput.Extension), nameof(AnimationProperties.FileExtension), Use = nameof(Undotted))]
    [MapProperty(nameof(AnimationOutput.Name), nameof(AnimationProperties.AnimationName))]
    private static partial void Update(AnimationOutput output, AnimationProperties properties);

    private static partial void Update(AnimationQuality quality, AnimationProperties properties);

    private static void Update(AnimationKind kind, AnimationProperties properties) =>
        kind.Switch(
            properties,
            turntable: static (target, _) => target.CaptureType = AnimationProperties.CaptureTypes.Turntable,
            cameraAndTarget: static (target, paths) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Path;
                Camera(paths.Camera, target);
                paths.Target.Switch(target, fromCurve: static (host, curve) => host.TargetPathId = curve.CurveId, fromPoints: static (host, points) => host.TargetPoints = [.. points.Points]);
            },
            flythrough: static (target, flight) => {
                target.CaptureType = AnimationProperties.CaptureTypes.Flythrough;
                Camera(flight.Path, target);
            },
            sun: static (target, sun) => {
                SunSiteMapper.Update(sun.Site, target);
                SunSiteMapper.Update(sun.Window, target);
            });

    private static void Camera(AnimationPath path, AnimationProperties properties) =>
        path.Switch(properties, fromCurve: static (target, curve) => target.CameraPathId = curve.CurveId, fromPoints: static (target, points) => target.CameraPoints = [.. points.Points]);

    [UserMapping(Default = false)]
    private static string Undotted(FileExtension extension) => ((string)extension).TrimStart(FileExtension.Mark);

    [UserMapping]
    private static int Count(FrameCount frames) => frames;

    [UserMapping]
    private static string Text(NamePart name) => name;
}
