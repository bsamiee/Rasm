using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AnimationQuality {
    public static readonly AnimationQuality Draft = new("preview", renderFull: false, renderPreview: false);

    public static readonly AnimationQuality Recorded = new("full", renderFull: false, renderPreview: false);

    public static readonly AnimationQuality RenderedPreview = new("full", renderFull: false, renderPreview: true);

    public static readonly AnimationQuality Rendered = new("full", renderFull: true, renderPreview: false);

    public string CaptureMethod { get; }

    public bool RenderFull { get; }

    public bool RenderPreview { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnimationPath {
    public sealed record FromCurve(Guid CurveId) : AnimationPath {
        internal override void WriteCamera(AnimationProperties properties) => properties.CameraPathId = CurveId;

        internal override void WriteTarget(AnimationProperties properties) => properties.TargetPathId = CurveId;
    }

    public sealed record FromPoints(Seq<Point3d> Points) : AnimationPath {
        internal override void WriteCamera(AnimationProperties properties) => properties.CameraPoints = [.. Points];

        internal override void WriteTarget(AnimationProperties properties) => properties.TargetPoints = [.. Points];
    }

    internal abstract void WriteCamera(AnimationProperties properties);

    internal abstract void WriteTarget(AnimationProperties properties);
}

public sealed record SunPlace {
    private SunPlace(double latitude, double longitude, double northAngle) => (Latitude, Longitude, NorthAngle) = (latitude, longitude, northAngle);

    public double Latitude { get; }

    public double Longitude { get; }

    public double NorthAngle { get; }

    public static Fin<SunPlace> Create(double latitude, double longitude, double northAngle) =>
        (Limits.AtLeast(-90.0).AtMost(90.0).Check(latitude, nameof(Latitude)),
         Limits.AtLeast(-180.0).AtMost(180.0).Check(longitude, nameof(Longitude)))
        .Apply((placedLatitude, placedLongitude) => new SunPlace(placedLatitude, placedLongitude, northAngle))
        .As();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunWindow {
    private SunWindow(LocalDateTime start, LocalDateTime end) => (Start, End) = (start, end);

    public LocalDateTime Start { get; }

    public LocalDateTime End { get; }

    public sealed record Day : SunWindow {
        private Day(LocalDateTime start, LocalDateTime end, int minutesBetweenFrames) : base(start, end) => MinutesBetweenFrames = minutesBetweenFrames;

        public int MinutesBetweenFrames { get; }

        public static Fin<SunWindow> Create(LocalDate date, LocalTime from, LocalTime until, Duration step) =>
            (Year(date, nameof(AnimationProperties.StartYear)),
             Invalid.Unless(from <= until, nameof(until)),
             Durations.Whole(step, Duration.FromMinutes(1), Limits.AtLeast(1), nameof(step)))
            .Apply((_, _, minutes) => (SunWindow)new Day(date.At(from), date.At(until), minutes))
            .As();
    }

    public sealed record Season : SunWindow {
        private Season(LocalDateTime start, LocalDateTime end, int daysBetweenFrames) : base(start, end) => DaysBetweenFrames = daysBetweenFrames;

        public int DaysBetweenFrames { get; }

        public static Fin<SunWindow> Create(LocalDate from, LocalDate until, Period step) =>
            (Year(from, nameof(AnimationProperties.StartYear)),
             Year(until, nameof(AnimationProperties.EndYear)),
             Invalid.Unless(from <= until, nameof(until)),
             Limits.AtLeast(1).Check(step.Days, nameof(step)).Bind(days => Invalid.Unless(step == Period.FromDays(days), days, nameof(step))))
            .Apply((_, _, _, days) => (SunWindow)new Season(from.AtMidnight(), until.AtMidnight(), days))
            .As();
    }

    private static Fin<int> Year(LocalDate date, string member) =>
        Limits.AtLeast(Instant.FromUnixTimeSeconds(0).InUtc().Year + 1).AtMost(Instant.FromUnixTimeSeconds(uint.MaxValue).InUtc().Year - 1).Check(date.Year, member);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnimationKind {
    public sealed record Turntable() : AnimationKind {
        internal override void Write(AnimationProperties properties) => properties.CaptureType = AnimationProperties.CaptureTypes.Turntable;
    }

    public sealed record CameraAndTarget(AnimationPath Camera, AnimationPath Target) : AnimationKind {
        internal override void Write(AnimationProperties properties) {
            properties.CaptureType = AnimationProperties.CaptureTypes.Path;
            Camera.WriteCamera(properties);
            Target.WriteTarget(properties);
        }
    }

    public sealed record Flythrough(AnimationPath Path) : AnimationKind {
        internal override void Write(AnimationProperties properties) {
            properties.CaptureType = AnimationProperties.CaptureTypes.Flythrough;
            Path.WriteCamera(properties);
        }
    }

    public sealed record Sun(SunPlace Place, SunWindow Window) : AnimationKind {
        internal override void Write(AnimationProperties properties) {
            AnimationMapper.Update(Place, properties);
            AnimationMapper.Update(Window, properties);
            Window.Switch(
                properties,
                day: static (study, day) => {
                    study.CaptureType = AnimationProperties.CaptureTypes.DaySunStudy;
                    study.MinutesBetweenFrames = day.MinutesBetweenFrames;
                },
                season: static (study, season) => {
                    study.CaptureType = AnimationProperties.CaptureTypes.SeasonalSunStudy;
                    study.DaysBetweenFrames = season.DaysBetweenFrames;
                });
        }
    }

    internal abstract void Write(AnimationProperties properties);
}

public sealed record AnimationOutput {
    private AnimationOutput(string folderName, string fileExtension, string animationName) => (FolderName, FileExtension, AnimationName) = (folderName, fileExtension, animationName);

    public string FolderName { get; }

    public string FileExtension { get; }

    public string AnimationName { get; }

    public static Fin<AnimationOutput> Create(string folderName, string fileExtension, string animationName) =>
        Seq(
                Invalid.Unless(Path.IsPathFullyQualified(folderName), nameof(FolderName)),
                FileNamePart(fileExtension, nameof(FileExtension)),
                FileNamePart(animationName, nameof(AnimationName)))
            .Traverse(static check => check)
            .As()
            .Map(_ => new AnimationOutput(folderName, fileExtension, animationName));

    private static Fin<Unit> FileNamePart(string part, string member) =>
        Invalid.Unless((part.Length > 0) && !part.ContainsAny(Path.GetInvalidFileNameChars()), member);
}

public sealed record AnimationSettings(
    AnimationProperties.CaptureTypes CaptureType,
    int FrameCount,
    int CurrentFrame,
    Option<string> ViewportName,
    Option<Guid> DisplayMode,
    Option<string> FolderName,
    Option<string> FileExtension,
    Option<string> AnimationName,
    Option<string> HtmlFullPath,
    Seq<string> Images,
    Seq<string> Dates);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class AnimationMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(AnimationOutput output, AnimationProperties properties);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(AnimationQuality quality, AnimationProperties properties);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(SunPlace place, AnimationProperties properties);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@SunWindow.Start.Minute), nameof(AnimationProperties.StartMinutes))]
    [MapProperty(nameof(@SunWindow.Start.Second), nameof(AnimationProperties.StartSeconds))]
    [MapProperty(nameof(@SunWindow.End.Minute), nameof(AnimationProperties.EndMinutes))]
    [MapProperty(nameof(@SunWindow.End.Second), nameof(AnimationProperties.EndSeconds))]
    internal static partial void Update(SunWindow window, AnimationProperties properties);

    internal static partial AnimationSettings ToSettings(AnimationProperties properties);
}

public static class Animations {
    public static IO<AnimationSettings> ReadAnimation(RhinoDoc document) =>
        DisposalOps.Using(() => document.AnimationProperties, static copy => IO.lift(() => AnimationMapper.ToSettings(copy)));

    public static IO<Unit> WriteAnimation(
        RhinoDoc document,
        RhinoViewport viewport,
        AnimationKind kind,
        int frames,
        Option<Guid> displayMode,
        Option<AnimationOutput> output,
        Option<AnimationQuality> quality) =>
        from count in IO.lift(Limits.AtLeast(1).Check(frames, nameof(AnimationProperties.FrameCount)))
        from committed in Commits.WithinUndo(document, LOC.STR("Animation"), DisposalOps.Using(() => document.AnimationProperties, copy => IO.lift(() => {
            kind.Write(copy);
            copy.FrameCount = count;
            copy.ViewportName = viewport.Name;
            _ = displayMode.Iter(id => copy.DisplayMode = id);
            _ = output.Iter(named => AnimationMapper.Update(named, copy));
            _ = quality.Iter(level => AnimationMapper.Update(level, copy));
            document.AnimationProperties = copy;
        })))
        select committed;
}

internal static class Durations {
    public static Fin<int> Whole(Duration value, Duration measure, Limits<int> range, string member) =>
        Int128.DivRem(value.ToInt128Nanoseconds(), measure.ToInt128Nanoseconds()) is var (count, remainder) && (remainder == 0)
            ? Limits.AtLeast<Int128>(int.MinValue).AtMost(int.MaxValue).Check(count, member).Bind(whole => range.Check((int)whole, member))
            : new Invalid(member);
}
