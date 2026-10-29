using System.Drawing;
using System.Globalization;
using System.Numerics;
using MathNet.Numerics.RootFinding;
using GeoTimeZone;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Render;
using UnitsNet;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Offset>(SkipFactoryMethods = true)]
public sealed partial class StandardOffset : IMinMaxValue<StandardOffset> {
    public static StandardOffset MinValue { get; } = new(Offset.FromHours(-12));

    public static StandardOffset MaxValue { get; } = new(Offset.FromHours(13));

    public double Hours => _value.Seconds / (double)NodaConstants.SecondsPerHour;

    public static Fin<StandardOffset> Validated(Offset value) =>
        RhinoFault.Unless(value >= MinValue._value && value <= MaxValue._value, RhinoReason.Outside, value).Map(_ => new StandardOffset(value));
}

[ValueObject<Duration>(SkipFactoryMethods = true)]
public sealed partial class DaylightSaving : IMinMaxValue<DaylightSaving> {
    public static DaylightSaving MinValue { get; } = new(Duration.Zero);

    public static DaylightSaving MaxValue { get; } = new(Duration.FromMinutes(120));

    public int Minutes => (int)_value.TotalMinutes;

    public static Fin<DaylightSaving> Validated(Duration value) =>
        RhinoFault.Unless(value >= MinValue._value && value <= MaxValue._value, RhinoReason.Outside, value).Map(_ => new DaylightSaving(value));
}

public sealed record SunSite(GeoCoordinate Place, Angle North, StandardOffset Standard, Option<DaylightSaving> Saving) {
    private const double RiseAltitude = -0.833d;
    private const double HourAccuracy = 1e-6d;
    private const int RootIterations = 100;

    private static readonly DateInterval Dated = new(new LocalDate(1971, 1, 1), new LocalDate(2105, 12, 31));

    public Offset Wall => (Offset)Standard + Offset.FromSeconds(Minutes * NodaConstants.SecondsPerMinute);

    // --- [SITES]
    public static Fin<SunSite> Read(Sun sun) =>
        (GeoCoordinate.From(sun.Latitude, sun.Longitude),
         StandardOffset.Validated(Offset.FromSeconds((int)Math.Round(sun.TimeZone * NodaConstants.SecondsPerHour, MidpointRounding.ToEven))),
         Conversions.Present(sun.DaylightSavingOn, sun.DaylightSavingMinutes).Traverse(static minutes => DaylightSaving.Validated(Duration.FromMinutes(minutes))).As())
            .Apply((place, standard, saving) => new SunSite(place, Angle.FromDegrees(Quantities.Exact(sun.North)), standard, saving))
            .As();

    public static Fin<SunSite> Placed(GeoCoordinate place, Angle north, LocalDateTime moment) =>
        from zone in Conversions.Required(DateTimeZoneProviders.Tzdb.GetZoneOrNull(TimeZoneLookup.GetTimeZone(place.Latitude.Degrees.ToDouble(), place.Longitude.Degrees.ToDouble()).Result))
        let interval = zone.MapLocal(moment).EarlyInterval
        let savings = Some(interval.Savings).Filter(static offset => offset > Offset.Zero)
        from standard in StandardOffset.Validated(interval.WallOffset - savings.IfNone(Offset.Zero))
        from saving in savings.Traverse(static offset => DaylightSaving.Validated(Duration.FromSeconds(offset.Seconds))).As()
        select new SunSite(place, north, standard, saving);

    public static IO<SunSite> Here(Angle north, LocalDateTime moment) =>
        Products.Accepted(static () => (Sun.Here(out double latitude, out double longitude), (Latitude: latitude, Longitude: longitude)))
            .Bind(located => IO.lift(() => GeoCoordinate.From(located.Latitude, located.Longitude).Bind(place => Placed(place, north, moment))));

    // --- [DOCUMENT]
    public static LocalDateTime Moment(Sun sun) => LocalDateTime.FromDateTime(sun.GetDateTime(DateTimeKind.Local)).With(TimeAdjusters.TruncateToSecond);

    public IO<Unit> Write(Sun sun) =>
        IO.lift(() => {
            (sun.Latitude, sun.Longitude, sun.North) = (Place.Latitude.Degrees.ToDouble(), Place.Longitude.Degrees.ToDouble(), North.Degrees.ToDouble());
            (sun.TimeZone, sun.DaylightSavingOn) = (Standard.Hours, Saving.IsSome);
            _ = Saving.Iter(saving => sun.DaylightSavingMinutes = saving.Minutes);
        });

    public IO<Unit> SetDateTime(Sun sun, LocalDateTime local) =>
        IO.lift(() => Universal(InstantOf(local))).Bind(utc => IO.lift(() => sun.SetDateTime(utc, DateTimeKind.Utc)));

    public IO<LocalDateTime> Now(TimeProvider clock) =>
        IO.lift(() => clock.GetCurrentInstant().WithOffset(Wall).LocalDateTime.With(TimeAdjusters.TruncateToSecond));

    public Instant InstantOf(LocalDateTime local) => local.WithOffset(Wall).ToInstant();

    // --- [SAMPLES]
    public IO<SunSample> Sample(LocalDateTime local) =>
        Observed(sun => Universal(InstantOf(local)).Map(utc => {
            sun.SetDateTime(utc, DateTimeKind.Utc);
            return new SunSample(
                            sun.Vector, Angle.FromDegrees(Quantities.Exact(sun.Azimuth)), Angle.FromDegrees(Quantities.Exact(sun.Altitude)), Sun.ColorFromAltitude(sun.Altitude),
                            Sun.JulianDay(Standard.Hours, Minutes, local.ToDateTimeUnspecified(), local.TimeOfDay.TickOfDay / (double)NodaConstants.TicksPerHour));
        }));

    internal IO<T> Observed<T>(Func<Sun, Fin<T>> read) =>
        (from sun in use(static () => new Sun { Accuracy = Sun.Accuracies.Maximum })
         from placed in Write(sun)
         from value in IO.lift(() => read(sun))
         select value).Bracket();

    internal static Fin<DateTime> Universal(Instant at) =>
        RhinoFault.Unless(Dated.Contains(at.InUtc().Date), RhinoReason.Outside, at).Map(_ => DateTime.SpecifyKind(at.ToDateTimeUtc(), DateTimeKind.Unspecified));

    // --- [DAYS]
    public Fin<SunDay> Day(LocalDate date) =>
        from dated in RhinoFault.Unless(Dated.Contains(date), RhinoReason.Outside, date)
        let altitude = fun((double hour) => Sun.AltitudeFromValues(
            Place.Latitude.Degrees.ToDouble(), Place.Longitude.Degrees.ToDouble(), Standard.Hours, Minutes, date.ToDateTimeUnspecified(), hour))
        let peak = Enumerable.Range(0, NodaConstants.HoursPerDay + 1).Select(static hour => (double)hour).MaxBy(altitude)
        from noon in Root(hour => altitude(hour + HourAccuracy) - altitude(hour - HourAccuracy), Math.Max(peak - 1d, 0d), Math.Min(peak + 1d, NodaConstants.HoursPerDay))
        from daylight in Lit(altitude, noon, RiseAltitude)
        from twilight in Lit(altitude, noon, Sun.TwilightZone())
        select new SunDay(At(date, noon), new NodaTime.Interval(At(date, daylight.Rise), At(date, daylight.Set)), new NodaTime.Interval(At(date, twilight.Rise), At(date, twilight.Set)));

    private int Minutes => Saving.Map(static saving => saving.Minutes).IfNone(0);

    private Instant At(LocalDate date, double hour) =>
        date.AtMidnight().WithOffset(Wall).ToInstant() + Duration.FromHours(hour);

    private static Fin<(double Rise, double Set)> Lit(Func<double, double> altitude, double noon, double threshold) =>
        altitude(noon) <= threshold
            ? (noon, noon)
            : (Edge(altitude, threshold, 0d, noon), Edge(altitude, threshold, NodaConstants.HoursPerDay, noon)).Apply(static (rise, set) => (rise, set)).As();

    private static Fin<double> Edge(Func<double, double> altitude, double threshold, double outer, double noon) =>
        altitude(outer) >= threshold ? outer : Root(hour => altitude(hour) - threshold, Math.Min(outer, noon), Math.Max(outer, noon));

    private static Fin<double> Root(Func<double, double> function, double lower, double upper) =>
        Brent.TryFindRoot(function, lower, upper, HourAccuracy, RootIterations, out double root)
            ? root
            : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"root in [{lower}, {upper}]"), RhinoReason.Missing);
}

public sealed record SunSample(Vector3d Direction, Angle Azimuth, Angle Altitude, Color Color, double JulianDay);

public sealed record SunDay(Instant Noon, NodaTime.Interval Daylight, NodaTime.Interval Twilight);

[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class SunWindow {
    public LocalDateTime Start { get; }

    public LocalDateTime End { get; }

    public Period Step { get; }

    public AnimationProperties.CaptureTypes CaptureType => Step.HasTimeComponent ? AnimationProperties.CaptureTypes.DaySunStudy : AnimationProperties.CaptureTypes.SeasonalSunStudy;

    public Seq<LocalDateTime> Frames => toSeq(LanguageExt.List.unfold(Start, at => at <= End ? Some((at, at.Plus(Step))) : None));

    // --- [WINDOWS]
    public static Fin<SunWindow> Day(LocalDate date, LocalTime from, LocalTime until, Period step) =>
        Validated(date.At(from), date.At(until), step);

    public static Fin<SunWindow> Season(LocalDate from, LocalDate until, LocalTime at, Period step) =>
        Validated(from.At(at), until.At(at), step);

    public static Fin<SunWindow> Validated(LocalDateTime start, LocalDateTime end, Period step) =>
        TryCreate(start, end, step, out SunWindow? window, out RhinoFault? fault) ? window : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref LocalDateTime start, ref LocalDateTime end, ref Period step) =>
        validationError = Rule(start, end, step).Match(Succ: static _ => null, Fail: static error => error.Filter<RhinoFault>().Head as RhinoFault);

    private static Fin<Unit> Rule(LocalDateTime start, LocalDateTime end, Period step) =>
        (RhinoFault.Unless(start <= end, RhinoReason.Invalid, (start, end)),
         RhinoFault.Unless(step.Years is 0 && step.Months is 0 && (step.HasTimeComponent ? !step.HasDateComponent && start.Date == end.Date : start.TimeOfDay == end.TimeOfDay), RhinoReason.Invalid, step)
             .Bind(_ => Conversions.Whole(step.ToDuration(), step.HasTimeComponent ? Duration.FromMinutes(1) : Duration.FromDays(1)))
             .Bind(static count => RhinoFault.Unless(count > 0, RhinoReason.Outside, count)))
            .Apply(static (_, _) => unit)
            .As();

    // --- [ANIMATION]
    public static Fin<Option<SunWindow>> Read(AnimationProperties properties) =>
        (properties.CaptureType, Start: new LocalDateTime(properties.StartYear, properties.StartMonth, properties.StartDay, properties.StartHour, properties.StartMinutes, properties.StartSeconds),
            End: new LocalDateTime(properties.EndYear, properties.EndMonth, properties.EndDay, properties.EndHour, properties.EndMinutes, properties.EndSeconds)) switch {
                (AnimationProperties.CaptureTypes.DaySunStudy, var start, var end) => Validated(start, end, Period.FromMinutes(properties.MinutesBetweenFrames)).Map(Some),
                (AnimationProperties.CaptureTypes.SeasonalSunStudy, var start, var end) => Validated(start, end, Period.FromDays(properties.DaysBetweenFrames)).Map(Some),
                _ => Option<SunWindow>.None,
            };

    public void Write(AnimationProperties properties) {
        (properties.StartYear, properties.StartMonth, properties.StartDay, properties.StartHour, properties.StartMinutes, properties.StartSeconds) =
            (Start.Year, Start.Month, Start.Day, Start.Hour, Start.Minute, Start.Second);
        (properties.EndYear, properties.EndMonth, properties.EndDay, properties.EndHour, properties.EndMinutes, properties.EndSeconds) =
            (End.Year, End.Month, End.Day, End.Hour, End.Minute, End.Second);
        properties.CaptureType = CaptureType;
        if (Step.HasTimeComponent)
            properties.MinutesBetweenFrames = (int)Step.ToDuration().TotalMinutes;
        else
            properties.DaysBetweenFrames = (int)Step.ToDuration().TotalDays;
    }
}
