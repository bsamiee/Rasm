using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Viewport;

// --- [TYPES] ---------------------------------------------------------------------------
public enum AnimationQuality { Draft = 0, Recorded = 1, RenderedPreview = 2, Rendered = 3 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnimationPath {
    public sealed record FromCurve(Guid CurveId) : AnimationPath;

    public sealed record FromPoints(Seq<Point3d> Points) : AnimationPath;
}

public sealed record SunPlace(double LatitudeDegrees, double LongitudeDegrees, double NorthDegrees, Option<int> LightIndex);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunWindow {
    public sealed record Day(LocalDate Date, LocalTime From, LocalTime Until, Duration Step) : SunWindow;

    public sealed record Season(LocalDate From, LocalDate Until, Period Step) : SunWindow;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnimationKind {
    public sealed record Turntable() : AnimationKind;

    public sealed record CameraAndTarget(AnimationPath Camera, AnimationPath Target) : AnimationKind;

    public sealed record Flythrough(AnimationPath Path) : AnimationKind;

    public sealed record Sun(SunPlace Place, SunWindow Window) : AnimationKind;
}

public sealed record AnimationOutput(string FolderName, string FileExtension, string AnimationName);

public sealed record AnimationSettings(
    AnimationProperties.CaptureTypes CaptureType,
    int FrameCount,
    int CurrentFrame,
    Option<string> ViewportName,
    Guid DisplayMode,
    Option<string> FolderName,
    Option<string> FileExtension,
    Option<string> AnimationName,
    Option<string> HtmlFullPath,
    Seq<string> Images,
    Seq<string> Dates);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Animations {
    // --- [LIMITS]
    private static readonly Fin<Limits<int>> Year = Limits.AtLeast(1800).AtMost(2199, nameof(Year));

    // --- [SETTINGS]
    public static IO<AnimationSettings> ReadAnimation(RhinoDoc document) =>
        Disposal.Using(() => document.AnimationProperties, static copy => IO.lift(() => AnimationMapper.ToSettings(copy)));

    public static IO<AnimationSettings> WriteAnimation(
        RhinoDoc document,
        RhinoViewport viewport,
        AnimationKind kind,
        int frames,
        Option<Guid> displayMode,
        Option<AnimationOutput> output,
        Option<AnimationQuality> quality) =>
        from accepted in IO.lift(() => Accepted(frames, output))
        from qualified in output.Traverse(static named => Answers.QualifiedPath(named.FolderName)).As()
        from study in IO.lift(() => kind.Switch(
            turntable: static _ => Option<SunStudy>.None,
            cameraAndTarget: static _ => Option<SunStudy>.None,
            flythrough: static _ => Option<SunStudy>.None,
            sun: static sun => Study(sun.Window).Map(Some)))
        let staged = Disposal.Using(() => document.AnimationProperties, copy => IO.lift(() => {
            Write(copy, kind, study);
            copy.FrameCount = frames;
            copy.ViewportName = viewport.Name;
            _ = displayMode.Iter(id => copy.DisplayMode = id);
            _ = output.Iter(named => AnimationMapper.Update(named, copy));
            _ = quality.Iter(level => {
                copy.CaptureMethod = level == AnimationQuality.Draft ? "preview" : "full";
                copy.RenderFull = level == AnimationQuality.Rendered;
                copy.RenderPreview = level == AnimationQuality.RenderedPreview;
            });
            document.AnimationProperties = copy;
        }))
        from committed in Commits.WithinUndo(document, LOC.STR("Animation"), staged)
        from settings in ReadAnimation(document)
        select settings;

    private static Fin<Unit> Accepted(int frames, Option<AnimationOutput> output) =>
        (Seq(Limits.AtLeast(1).Check(frames, nameof(AnimationProperties.FrameCount)).Map(static _ => unit))
            + output.ToSeq().Bind(static named => Seq(
                FileNamePart(named.FileExtension, nameof(AnimationOutput.FileExtension)),
                FileNamePart(named.AnimationName, nameof(AnimationOutput.AnimationName)))))
        .Traverse(static check => check.ToValidation())
        .As()
        .ToFin()
        .Map(static _ => unit);

    private static Fin<Unit> FileNamePart(string part, string member) =>
        Invalid.Unless((part.Length > 0) && (part.IndexOfAny(Path.GetInvalidFileNameChars()) < 0), member);

    private static Fin<SunStudy> Study(SunWindow window) =>
        window.Switch(
            day: static day =>
                from years in Years(day.Date, day.Date)
                from ordered in Invalid.Unless(day.From <= day.Until, nameof(SunWindow.Day.Until))
                from step in Invalid.Unless(
                    (day.Step > Duration.Zero) && (day.Step.TotalMinutes <= int.MaxValue) && (day.Step == Duration.FromMinutes((long)day.Step.TotalMinutes)),
                    nameof(SunWindow.Day.Step))
                select new SunStudy(AnimationProperties.CaptureTypes.DaySunStudy, day.Date.At(day.From), day.Date.At(day.Until), (int)day.Step.TotalMinutes),
            season: static season =>
                from years in Years(season.From, season.Until)
                from ordered in Invalid.Unless(season.From <= season.Until, nameof(SunWindow.Season.Until))
                from step in Invalid.Unless((season.Step.Days > 0) && (season.Step == Period.FromDays(season.Step.Days)), nameof(SunWindow.Season.Step))
                select new SunStudy(AnimationProperties.CaptureTypes.SeasonalSunStudy, season.From.AtMidnight(), season.Until.AtMidnight(), season.Step.Days));

    private static Fin<Unit> Years(LocalDate start, LocalDate end) =>
        from limits in Year
        from startYear in limits.Check(start.Year, nameof(AnimationProperties.StartYear))
        from endYear in limits.Check(end.Year, nameof(AnimationProperties.EndYear))
        select unit;

    private static void Write(AnimationProperties copy, AnimationKind kind, Option<SunStudy> study) =>
        kind.Switch(
            (Copy: copy, Study: study),
            turntable: static (state, _) => state.Copy.CaptureType = AnimationProperties.CaptureTypes.Turntable,
            cameraAndTarget: static (state, path) => {
                state.Copy.CaptureType = AnimationProperties.CaptureTypes.Path;
                WritePath(path.Camera, id => state.Copy.CameraPathId = id, points => state.Copy.CameraPoints = points);
                WritePath(path.Target, id => state.Copy.TargetPathId = id, points => state.Copy.TargetPoints = points);
            },
            flythrough: static (state, flythrough) => {
                state.Copy.CaptureType = AnimationProperties.CaptureTypes.Flythrough;
                WritePath(flythrough.Path, id => state.Copy.CameraPathId = id, points => state.Copy.CameraPoints = points);
            },
            sun: static (state, sun) => {
                state.Copy.Latitude = sun.Place.LatitudeDegrees;
                state.Copy.Longitude = sun.Place.LongitudeDegrees;
                state.Copy.NorthAngle = sun.Place.NorthDegrees;
                _ = sun.Place.LightIndex.Iter(index => state.Copy.LightIndex = index);
                _ = state.Study.Iter(study => {
                    state.Copy.CaptureType = study.Type;
                    state.Copy.StartYear = study.Start.Year;
                    state.Copy.StartMonth = study.Start.Month;
                    state.Copy.StartDay = study.Start.Day;
                    state.Copy.StartHour = study.Start.Hour;
                    state.Copy.StartMinutes = study.Start.Minute;
                    state.Copy.StartSeconds = study.Start.Second;
                    state.Copy.EndYear = study.End.Year;
                    state.Copy.EndMonth = study.End.Month;
                    state.Copy.EndDay = study.End.Day;
                    state.Copy.EndHour = study.End.Hour;
                    state.Copy.EndMinutes = study.End.Minute;
                    state.Copy.EndSeconds = study.End.Second;
                    if (study.Type == AnimationProperties.CaptureTypes.DaySunStudy)
                        state.Copy.MinutesBetweenFrames = study.Between;
                    else
                        state.Copy.DaysBetweenFrames = study.Between;
                });
            });

    private static void WritePath(AnimationPath path, Action<Guid> pathId, Action<Point3d[]> points) =>
        path.Switch(
            (PathId: pathId, Points: points),
            fromCurve: static (write, curve) => write.PathId(curve.CurveId),
            fromPoints: static (write, listed) => write.Points([.. listed.Points]));

    private sealed record SunStudy(AnimationProperties.CaptureTypes Type, LocalDateTime Start, LocalDateTime End, int Between);
}

[Mapper]
internal static partial class AnimationMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(AnimationOutput output, AnimationProperties properties);

    internal static partial AnimationSettings ToSettings(AnimationProperties properties);
}
