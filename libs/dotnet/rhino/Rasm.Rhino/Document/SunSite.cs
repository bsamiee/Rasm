using System.Drawing;
using System.Globalization;
using NodaTime.Text;
using NodaTime.TimeZones;
using Rasm.Imaging.Pixels;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NorthAngle : System.Numerics.IMinMaxValue<NorthAngle> {
    private const double Turn = 360d;

    public static NorthAngle MinValue { get; } = new(0d);
    public static NorthAngle MaxValue { get; } = new(double.BitDecrement(Turn));
    public static NorthAngle Default { get; } = new(90d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        (validationError, value) = double.IsFinite(value) ? (null, ((value % Turn) + Turn) % Turn) : (new ValidationError("north angle is not finite"), value);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ObjectFactory<Offset>]
public readonly partial struct StandardOffset : System.Numerics.IMinMaxValue<StandardOffset>, IConvertible<Offset> {
    public static StandardOffset MinValue { get; } = new(-12d);
    public static StandardOffset MaxValue { get; } = new(13d);
    public static StandardOffset Default { get; } = new(0d);

    public static ValidationError? Validate(Offset value, IFormatProvider? provider, out StandardOffset item) =>
        Validate(value.Seconds / (double)NodaConstants.SecondsPerHour, provider, out item);

    public Offset ToValue() => Offset.FromSeconds((int)Math.Round(_value * NodaConstants.SecondsPerHour, MidpointRounding.ToEven));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        value = Math.Round(value * NodaConstants.SecondsPerHour, MidpointRounding.ToEven) / NodaConstants.SecondsPerHour;
        validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
    }
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DaylightSaving : System.Numerics.IMinMaxValue<DaylightSaving>, IConvertible<Offset> {
    public static DaylightSaving MinValue { get; } = new(0);
    public static DaylightSaving MaxValue { get; } = new(120);

    public Offset ToValue() => Offset.FromSeconds(_value * NodaConstants.SecondsPerMinute);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SunIntensity : System.Numerics.IMinMaxValue<SunIntensity> {
    public static SunIntensity MinValue { get; } = new(0d);
    public static SunIntensity MaxValue { get; } = new(double.MaxValue);
    public static SunIntensity Default { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Horizon", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SunAltitude : System.Numerics.IMinMaxValue<SunAltitude> {
    public static SunAltitude MinValue { get; } = new(-90d);
    public static SunAltitude MaxValue { get; } = new(90d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<LocalDateTime>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ObjectFactory<string>]
public sealed partial class SunMoment : System.Numerics.IMinMaxValue<SunMoment>, IConvertible<string> {
    public static SunMoment MinValue { get; } = new(new LocalDate(1971, 1, 1).AtMidnight());
    public static SunMoment MaxValue { get; } = new(Instant.FromUnixTimeSeconds(uint.MaxValue).WithOffset(StandardOffset.MinValue.ToValue()).LocalDateTime);

    public static Fin<SunMoment> Read(Sun sun) =>
        ValidationResults.Validated<SunMoment, LocalDateTime>(LocalDateTime.FromDateTime(sun.GetDateTime(DateTimeKind.Local)));

    public static IO<SunMoment> Now(SunSite site, TimeProvider clock) =>
        IO.lift(() => ValidationResults.Validated<SunMoment, LocalDateTime>(clock.GetCurrentInstant().WithOffset(site.Wall).LocalDateTime));

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out SunMoment? item) {
        item = null;
        return value is null ? null
            : LocalDateTimePattern.ExtendedIso.Parse(value) is { Success: true } parsed ? Validate(parsed.Value, provider, out item)
            : new ValidationError($"{value} is no extended ISO 8601 local date and time");
    }

    public string ToValue() => LocalDateTimePattern.ExtendedIso.Format(_value);

    public Fin<Unit> Write(Sun sun, TimeProvider clock) =>
        clock.LocalTimeZone.IsInvalidTime(_value.ToDateTimeUnspecified())
            ? Error.New(string.Create(CultureInfo.InvariantCulture, $"{_value} does not exist in local time zone"))
            : fun<DateTime, DateTimeKind>(sun.SetDateTime)(_value.ToDateTimeUnspecified(), DateTimeKind.Local);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref LocalDateTime value) {
        value = value.With(TimeAdjusters.TruncateToSecond);
        validationError = ValidationResults.OutsideRange(value, MinValue._value, MaxValue._value);
    }
}

public sealed record SunSite(GeoCoordinate Place, NorthAngle North, StandardOffset Standard, Option<DaylightSaving> Saving) {
    private const double SunriseZenith = 90.833d;

    public static SunSite Default { get; } = new(new GeoCoordinate(Latitude.Default, Longitude.Default), NorthAngle.Default, StandardOffset.Default, None);

    public Offset Wall => Standard.ToValue() + Saving.Map(static saving => saving.ToValue()).IfNone(Offset.Zero);

    public static Fin<SunSite> Read(Sun sun) =>
        (GeoCoordinate.From(sun.Latitude, sun.Longitude),
         NorthAngle.Validated(sun.North),
         ValidationResults.Validated<StandardOffset, double>(sun.TimeZone),
         Callbacks.Found(sun.DaylightSavingOn, sun.DaylightSavingMinutes).Traverse(static minutes => DaylightSaving.Validated(minutes)).As())
            .Apply(static (place, north, standard, saving) => new SunSite(place, north, standard, saving))
            .As();

    public static Fin<SunSite> Placed(GeoCoordinate place, NorthAngle north, SunMoment moment) =>
        from interval in Zone(place).Map(zone => zone.MapLocal((LocalDateTime)moment).EarlyInterval)
        let positive = Some(interval.Savings).Filter(static offset => offset > Offset.Zero)
        from site in (ValidationResults.Validated<StandardOffset, Offset>(interval.WallOffset - positive.IfNone(Offset.Zero)),
                      positive.Traverse(static offset => Conversions.Whole(Duration.FromSeconds(offset.Seconds), Duration.FromMinutes(1)).Bind(static minutes => DaylightSaving.Validated(minutes))).As())
            .Apply((standard, saving) => new SunSite(place, north, standard, saving))
            .As()
        select site;

    public static Fin<SunSite> FromAnchor(EarthAnchor anchor, SunMoment moment) =>
        from located in anchor.Located
        from north in NorthAngle.Validated(double.RadiansToDegrees(Math.Atan2(located.Compass.YAxis.Y, located.Compass.YAxis.X)))
        from site in Placed(located.Earth.Place, north, moment)
        select site;

    public static IO<SunSite> Here(NorthAngle north, SunMoment moment) =>
        IO.lift(() => Refused.Unless(Sun.Here(out double latitude, out double longitude), nameof(Sun.Here))
            .Bind(_ => GeoCoordinate.From(latitude, longitude))
            .Bind(place => Placed(place, north, moment)));

    public EarthAnchor ToAnchor(EarthAnchor held) =>
        Math.SinCos(double.DegreesToRadians(North)) switch {
            var (sin, cos) => held with {
                Earth = new EarthLocation(Place, held.Earth.Map(static earth => earth.Elevation).IfNone(Length.Zero)),
                Compass = new Plane(held.Compass.Map(static compass => compass.Origin).IfNone(Point3d.Origin), new Vector3d(sin, -cos, 0d), new Vector3d(cos, sin, 0d)),
            },
        };

    public Instant InstantOf(LocalDateTime local) => local.WithOffset(Wall).ToInstant();

    public IO<SunSample> Sample(LocalDateTime local, TimeProvider clock) =>
        InstantOf(local) switch {
            var at => Observed(clock, (sun, computer) =>
                from direction in Direction(sun, computer, at)
                let altitude = sun.Altitude
                select new SunSample(direction, altitude, Sun.ColorFromAltitude(altitude), at.ToJulianDate())),
        };

    public SunDay Day(LocalDate date) =>
        new SolarTerms(new SolarTerms(InstantOf(date.At(LocalTime.Noon))).Transit(Place.Longitude)).Transit(Place.Longitude) switch {
            var noon => Horizon(new SolarTerms(noon).Declination, zenith: SunriseZenith) switch {
                <= -1d => new SunDay.PolarDay(noon),
                >= 1d => new SunDay.PolarNight(noon),
                var cosine => Duration.FromDays(Math.Acos(cosine) / double.Tau) switch {
                    var half => new SunDay.RiseAndSet(noon - half, noon, noon + half),
                },
            },
        };

    public IO<SunPaths> Paths(SunMoment moment, Sphere dome, TimeProvider clock) =>
        from placed in IO.lift(Refused.Unless(dome.IsValid, dome, nameof(dome)))
        let year = ((LocalDateTime)moment).Year
        let days = toSeq(new DateInterval(new LocalDate(year, 1, 1), new LocalDate(year, 12, 31)))
        let hours = toSeq(Range(0, NodaConstants.HoursPerDay)).Map(static hour => LocalTime.Midnight.PlusHours(hour))
        let standard = Standard.ToValue()
        from grid in Observed(clock, (sun, computer) => days.Traverse(date => hours.Traverse(hour => Direction(sun, computer, date.At(hour).WithOffset(standard).ToInstant())).As()).As())
        let sky = Arr.createRange(grid.Bind(static row => row).Map(static toward => -toward))
        let solstices = days.Map((date, row) => (Row: row, new SolarTerms(date.At(LocalTime.Noon).WithOffset(standard).ToInstant()).Declination))
        select new SunPaths(
            toMap(hours.Map((hour, column) => (hour, Arcs(toSeq(Range(0, days.Count)).Map(row => sky[(row * hours.Count) + column]))
                .Map(arc => arc.Map(point => Projected(placed, point)))))),
            Meshed(Band(sky, hours.Count, solstices.MaxBy(static day => day.Declination).Row, solstices.MinBy(static day => day.Declination).Row), placed));

    private static IO<TResult> Observed<TResult>(TimeProvider clock, Func<Sun, DateTimeZone, Fin<TResult>> read) =>
        DisposalOps.Within(static () => new Sun { Accuracy = Sun.Accuracies.Maximum }, sun => IO.lift(() => read(sun, BclDateTimeZone.FromTimeZoneInfo(clock.LocalTimeZone))));

    private Fin<Vector3d> Direction(Sun sun, DateTimeZone computer, Instant at) =>
        at.InZone(computer) switch {
            var held => (ValidationResults.Validated<SunMoment, LocalDateTime>(held.LocalDateTime),
                         ValidationResults.Validated<StandardOffset, Offset>(held.Offset))
                .Apply((_, offset) => {
                    SunMapper.Update(this with { Standard = offset, Saving = None }, sun);
                    sun.SetDateTime(at.ToDateTimeUtc(), DateTimeKind.Local);
                    return sun.Vector;
                })
                .As(),
        };

    private static Fin<DateTimeZone> Zone(GeoCoordinate place) =>
        Optional(TzdbDateTimeZoneSource.Default.ZoneLocations)
            .Bind(locations => toSeq(locations
                .OrderBy(location =>
                    Math.Pow(Math.Sin(double.DegreesToRadians(location.Latitude - place.Latitude) / 2d), 2d)
                    + (Math.Cos(double.DegreesToRadians(place.Latitude)) * Math.Cos(double.DegreesToRadians(location.Latitude))
                       * Math.Pow(Math.Sin(double.DegreesToRadians(location.Longitude - place.Longitude) / 2d), 2d)))
                .ThenBy(static location => location.ZoneId, StringComparer.Ordinal)).Head)
            .Map(static location => DateTimeZoneProviders.Tzdb[location.ZoneId])
            .ToFin(new Refused(nameof(TzdbDateTimeZoneSource.ZoneLocations), Refused.Absent));

    private double Horizon(double declination, double zenith) =>
        Math.SinCos(double.DegreesToRadians(Place.Latitude)) switch {
            var (sin, cos) => (Math.Cos(double.DegreesToRadians(zenith)) - (sin * Math.Sin(declination))) / (cos * Math.Cos(declination)),
        };

    private static Seq<Seq<Vector3d>> Arcs(Seq<Vector3d> loop) =>
        loop.Map(static (sky, index) => (Sky: sky, Index: index)).Find(static point => point.Sky.Z <= 0d).Match(
            Some: below => toSeq(Range(below.Index, loop.Count))
                .Map(step => (First: loop[step % loop.Count], Second: loop[(step + 1) % loop.Count]))
                .Fold((Arcs: Seq<Seq<Vector3d>>(), Open: Seq<Vector3d>()), static (cut, edge) => (edge.First.Z > 0d, edge.Second.Z > 0d) switch {
                    (true, true) => (cut.Arcs, cut.Open.Add(edge.Second)),
                    (false, false) => cut,
                    (var leaving, _) => Crossing(edge.First, edge.Second) switch {
                        var at => leaving ? (cut.Arcs.Add(cut.Open.Add(at)), Seq<Vector3d>()) : (cut.Arcs, Seq(at, edge.Second)),
                    },
                }).Arcs,
            None: () => Seq(loop.Concat(loop.Take(1))));

    private static Vector3d Crossing(Vector3d from, Vector3d to) => from + ((to - from) * (from.Z / (from.Z - to.Z)));

    private static Seq<Seq<Vector3d>> Band(Arr<Vector3d> sky, int columns, int june, int december) =>
        (from row in toSeq(Range(june, december - june))
         from column in toSeq(Range(0, columns))
         select Seq(
                (row * columns) + column,
                (row * columns) + ((column + 1) % columns),
                ((row + 1) * columns) + ((column + 1) % columns),
                ((row + 1) * columns) + column))
            .Bind(quad => quad.ForAll(slot => sky[slot].Z > 0d)
                ? Seq(quad.Map(slot => sky[slot]))
                : Seq(Seq(quad[0], quad[1], quad[2]), Seq(quad[0], quad[2], quad[3]))
                    .Map(triangle => Clipped(sky, triangle)).Filter(static polygon => !polygon.IsEmpty));

    private static Seq<Vector3d> Clipped(Arr<Vector3d> sky, Seq<int> corners) =>
        corners.Exists(slot => sky[slot].Z > 0d)
            ? corners.Zip(corners.Tail.Add(corners[0])).Bind(edge => Seq(
                Some(sky[edge.First]).Filter(static point => point.Z >= 0d),
                Some((Low: Math.Min(edge.First, edge.Second), High: Math.Max(edge.First, edge.Second)))
                    .Filter(_ => (sky[edge.First].Z < 0d && sky[edge.Second].Z > 0d) || (sky[edge.First].Z > 0d && sky[edge.Second].Z < 0d))
                    .Map(span => Crossing(sky[span.Low], sky[span.High]))).Somes())
            : Seq<Vector3d>();

    private static SunCoverage Meshed(Seq<Seq<Vector3d>> polygons, Sphere dome) =>
        polygons.Bind(static polygon => polygon).Distinct() switch {
            var points => toHashMap(points.Map(static (point, index) => (point, index))) switch {
                var indices => new SunCoverage(
                    points.Map(point => Projected(dome, point)),
                    polygons.Map(polygon => polygon.Map(point => indices[point])).Map(static at => new MeshFace(at[0], at[1], at[2], at[^1]))),
            },
        };

    private static Point3d Projected(Sphere dome, Vector3d sky) => dome.Center + (dome.Radius * sky);

    private readonly record struct SolarTerms(Instant At) {
        public double Declination => Math.Asin(Math.Sin(Obliquity) * Math.Sin(ApparentLongitude));

        public Instant Transit(Longitude longitude) =>
            At - Duration.FromDays(Math.IEEERemainder(
                (double.Tau * At.InUtc().TimeOfDay.NanosecondOfDay / NodaConstants.NanosecondsPerDay) + double.DegreesToRadians(longitude) + Equation - Math.PI,
                double.Tau) / double.Tau);

        private double Century => (At.ToJulianDate() - 2451545d) / 36525d;

        private double MeanLongitude => double.DegreesToRadians(280.46646d + (Century * (36000.76983d + (Century * 0.0003032d))));

        private double MeanAnomaly => double.DegreesToRadians(357.52911d + (Century * (35999.05029d - (Century * 0.0001537d))));

        private double Eccentricity => 0.016708634d - (Century * (0.000042037d + (Century * 0.0000001267d)));

        private double Node => double.DegreesToRadians(125.04d - (1934.136d * Century));

        private double Obliquity =>
            double.DegreesToRadians(23d + (26d / 60d) + ((21.448d - (Century * (46.8150d + (Century * (0.00059d - (Century * 0.001813d)))))) / 3600d) + (0.00256d * Math.Cos(Node)));

        private double ApparentLongitude =>
            MeanLongitude - double.DegreesToRadians(0.00569d + (0.00478d * Math.Sin(Node)))
            + double.DegreesToRadians(((1.914602d - (Century * (0.004817d + (Century * 0.000014d)))) * Math.Sin(MeanAnomaly))
                + ((0.019993d - (Century * 0.000101d)) * Math.Sin(2d * MeanAnomaly)) + (0.000289d * Math.Sin(3d * MeanAnomaly)));

        private double Equation =>
            (Math.Pow(Math.Tan(Obliquity / 2d), 2d), Eccentricity, MeanAnomaly, MeanLongitude) switch {
                var (y, e, m, l) => (y * Math.Sin(2d * l)) + (2d * e * Math.Sin(m) * ((2d * y * Math.Cos(2d * l)) - 1d))
                    - (0.5d * y * y * Math.Sin(4d * l)) - (1.25d * e * e * Math.Sin(2d * m)),
            };
    }
}

public record SunLight {
    public required bool Enabled { get; init; }

    public required SunIntensity Intensity { get; init; }

    public required bool ManualControlOn { get; init; }

    public required NorthAngle Azimuth { get; init; }

    public required SunAltitude Altitude { get; init; }

    public required SunSite Site { get; init; }
}

public sealed record SunSample(Vector3d Direction, double Altitude, Color Color, double JulianDay);

[Union]
public abstract partial record SunDay {
    public abstract Instant Noon { get; init; }

    public Duration Length => Switch(riseAndSet: static day => day.Set - day.Rise, polarDay: static _ => Duration.FromDays(1), polarNight: static _ => Duration.Zero);

    public sealed record RiseAndSet(Instant Rise, Instant Noon, Instant Set) : SunDay;

    public sealed record PolarDay(Instant Noon) : SunDay;

    public sealed record PolarNight(Instant Noon) : SunDay;
}

public sealed record SunCoverage(Seq<Point3d> Vertices, Seq<MeshFace> Faces) {
    public Mesh ToMesh() {
        Mesh mesh = new();
        mesh.Vertices.AddVertices(Vertices);
        _ = mesh.Faces.AddFaces(Faces);
        _ = mesh.Normals.ComputeNormals();
        return mesh;
    }
}

public sealed record SunPaths(Map<LocalTime, Seq<Seq<Point3d>>> Analemmas, SunCoverage Coverage);

public sealed record SunAngles(double Azimuth, double Altitude);

[Union]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record SunWindow : IConvertible<string> {
    private SunWindow(LocalDateTime start, LocalDateTime end) => (Start, End) = (start, end);

    public LocalDateTime Start { get; }

    public LocalDateTime End { get; }

    public Period Step => Switch(day: static day => Period.FromMinutes(day.MinutesBetweenFrames), season: static season => Period.FromDays(season.DaysBetweenFrames));

    public Seq<LocalDateTime> Frames => toSeq(LanguageExt.List.unfold(Start, at => at <= End ? Some((at, at.Plus(Step))) : None));

    public static Fin<Option<SunWindow>> Read(AnimationProperties properties) =>
        properties.CaptureType switch {
            AnimationProperties.CaptureTypes.DaySunStudy => Studied(properties, static (start, end, held) =>
                Day.Create(start.Date, start.TimeOfDay, end.TimeOfDay, Period.FromMinutes(held.MinutesBetweenFrames)).Map(static day => (SunWindow)day)),
            AnimationProperties.CaptureTypes.SeasonalSunStudy => Studied(properties, static (start, end, held) =>
                Season.Create(start.Date, end.Date, start.TimeOfDay, Period.FromDays(held.DaysBetweenFrames)).Map(static season => (SunWindow)season)),
            _ => Option<SunWindow>.None,
        };

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out SunWindow? item) {
        item = Optional(value).Bind(static text => Parsed(text.Split('/'))).ValueUnsafe();
        return item is null && value is not null
            ? new ValidationError($"{value} is no day/<date>/<from>/<until>/<minutes> or season/<from>/<until>/<at>/<days> sun window")
            : null;
    }

    public string ToValue() =>
        Switch(
            day: static day => string.Join('/', "day", LocalDatePattern.Iso.Format(day.Start.Date), LocalTimePattern.ExtendedIso.Format(day.Start.TimeOfDay),
                LocalTimePattern.ExtendedIso.Format(day.End.TimeOfDay), day.MinutesBetweenFrames.ToString(CultureInfo.InvariantCulture)),
            season: static season => string.Join('/', "season", LocalDatePattern.Iso.Format(season.Start.Date), LocalDatePattern.Iso.Format(season.End.Date),
                LocalTimePattern.ExtendedIso.Format(season.Start.TimeOfDay), season.DaysBetweenFrames.ToString(CultureInfo.InvariantCulture)));

    private static Fin<Option<SunWindow>> Studied(AnimationProperties properties, Func<LocalDateTime, LocalDateTime, AnimationProperties, Fin<SunWindow>> window) =>
        window(
            new LocalDateTime(properties.StartYear, properties.StartMonth, properties.StartDay, properties.StartHour, properties.StartMinutes, properties.StartSeconds),
            new LocalDateTime(properties.EndYear, properties.EndMonth, properties.EndDay, properties.EndHour, properties.EndMinutes, properties.EndSeconds),
            properties).Map(static held => Some(held));

    private static Option<SunWindow> Parsed(string[] parts) =>
        parts switch {
            ["day", var date, var first, var last, var minutes] =>
                from day in Succeeded(LocalDatePattern.Iso.Parse(date))
                from start in Succeeded(LocalTimePattern.ExtendedIso.Parse(first))
                from end in Succeeded(LocalTimePattern.ExtendedIso.Parse(last))
                from count in Invariant.Number<int>(minutes, NumberStyles.None)
                from held in Day.Create(day, start, end, Period.FromMinutes(count)).ToOption()
                select (SunWindow)held,
            ["season", var first, var last, var at, var days] =>
                from start in Succeeded(LocalDatePattern.Iso.Parse(first))
                from end in Succeeded(LocalDatePattern.Iso.Parse(last))
                from time in Succeeded(LocalTimePattern.ExtendedIso.Parse(at))
                from count in Invariant.Number<int>(days, NumberStyles.None)
                from held in Season.Create(start, end, time, Period.FromDays(count)).ToOption()
                select (SunWindow)held,
            _ => None,
        };

    private static Option<T> Succeeded<T>(ParseResult<T> result) where T : struct => Callbacks.Found(result.TryGetValue(default, out T value), value);

    private static Fin<TWindow> Bounded<TWindow>(LocalDateTime start, LocalDateTime end, Period step, Duration grain, Func<LocalDateTime, LocalDateTime, int, TWindow> window) =>
        (ValidationResults.Validated<SunMoment, LocalDateTime>(start),
         ValidationResults.Validated<SunMoment, LocalDateTime>(end),
         start <= end ? Fin.Succ(unit) : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"sun study start {start} is after end {end}")),
         Some(step).Filter(held => held.Years is 0 && held.Months is 0 && held.ToDuration() >= grain)
             .ToFin(Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"sun study step {step} is no positive count of {grain}")))
             .Bind(held => Conversions.Whole(held.ToDuration(), grain)))
            .Apply((_, _, _, count) => window(start, end, count))
            .As();

    public sealed record Day : SunWindow {
        private Day(LocalDateTime start, LocalDateTime end, int minutesBetweenFrames) : base(start, end) => MinutesBetweenFrames = minutesBetweenFrames;

        public int MinutesBetweenFrames { get; }

        public static Fin<Day> Create(LocalDate date, LocalTime from, LocalTime until, Period step) =>
            Bounded(date.At(from), date.At(until), step, Duration.FromMinutes(1), static (start, end, minutes) => new Day(start, end, minutes));
    }

    public sealed record Season : SunWindow {
        private Season(LocalDateTime start, LocalDateTime end, int daysBetweenFrames) : base(start, end) => DaysBetweenFrames = daysBetweenFrames;

        public int DaysBetweenFrames { get; }

        public static Fin<Season> Create(LocalDate from, LocalDate until, LocalTime at, Period step) =>
            Bounded(from.At(at), until.At(at), step, Duration.FromDays(1), static (start, end, days) => new Season(start, end, days));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class SunMapper {
    public static void Update(SunLight light, Sun sun) {
        Map(light, sun);
        Update(light.Site, sun);
        if (light.ManualControlOn)
            Place(light, sun);
    }

    internal static void Update(SunSite site, Sun sun) {
        Map(site, sun);
        _ = site.Saving.IfSome(saving => sun.DaylightSavingMinutes = saving);
    }

    internal static void Update(SunWindow window, AnimationProperties properties) {
        Map(window, properties);
        _ = window.Switch(properties,
            day: static (target, day) => (target.CaptureType, target.MinutesBetweenFrames) = (AnimationProperties.CaptureTypes.DaySunStudy, day.MinutesBetweenFrames),
            season: static (target, season) => (target.CaptureType, target.DaysBetweenFrames) = (AnimationProperties.CaptureTypes.SeasonalSunStudy, season.DaysBetweenFrames));
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial SunAngles ToAngles(Sun sun);

    [MapProperty(nameof(@SunSite.Place.Latitude), nameof(AnimationProperties.Latitude))]
    [MapProperty(nameof(@SunSite.Place.Longitude), nameof(AnimationProperties.Longitude))]
    [MapProperty(nameof(SunSite.North), nameof(AnimationProperties.NorthAngle))]
    [MapperIgnoreSource(nameof(SunSite.Standard), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.Saving), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.Wall), Justification = "AnimationProperties stores no offset")]
    internal static partial void Update(SunSite site, AnimationProperties properties);

    [MapperIgnoreSource(nameof(SunLight.Azimuth), Justification = "Written by Place while ManualControlOn is on")]
    [MapperIgnoreSource(nameof(SunLight.Altitude), Justification = "Written by Place while ManualControlOn is on")]
    [MapperIgnoreSource(nameof(SunLight.Site), Justification = "Written by Update(SunSite, Sun)")]
    private static partial void Map(SunLight light, Sun sun);

    [MapperIgnoreSource(nameof(SunLight.Enabled), Justification = "Written by Map before angles")]
    [MapperIgnoreSource(nameof(SunLight.Intensity), Justification = "Written by Map before angles")]
    [MapperIgnoreSource(nameof(SunLight.ManualControlOn), Justification = "Written by Map before angles")]
    [MapperIgnoreSource(nameof(SunLight.Site), Justification = "Written by Update(SunSite, Sun) before angles")]
    private static partial void Place(SunLight light, Sun sun);

    [MapProperty(nameof(@SunSite.Place.Latitude), nameof(Sun.Latitude))]
    [MapProperty(nameof(@SunSite.Place.Longitude), nameof(Sun.Longitude))]
    [MapProperty(nameof(SunSite.Standard), nameof(Sun.TimeZone))]
    [MapProperty(nameof(@SunSite.Saving.IsSome), nameof(Sun.DaylightSavingOn))]
    [MapperIgnoreSource(nameof(SunSite.Wall), Justification = "Sum of Standard and Saving")]
    private static partial void Map(SunSite site, Sun sun);

    [MapProperty(nameof(@SunWindow.Start.Minute), nameof(AnimationProperties.StartMinutes))]
    [MapProperty(nameof(@SunWindow.Start.Second), nameof(AnimationProperties.StartSeconds))]
    [MapProperty(nameof(@SunWindow.End.Minute), nameof(AnimationProperties.EndMinutes))]
    [MapProperty(nameof(@SunWindow.End.Second), nameof(AnimationProperties.EndSeconds))]
    [MapperIgnoreSource(nameof(SunWindow.Step), Justification = "Written per case as frame spacing")]
    [MapperIgnoreSource(nameof(SunWindow.Frames), Justification = "Read by previews alone")]
    private static partial void Map(SunWindow window, AnimationProperties properties);

    [UserMapping]
    private static double Degrees(Latitude latitude) => latitude;

    [UserMapping]
    private static double Degrees(Longitude longitude) => longitude;

    [UserMapping]
    private static double Degrees(SunAltitude altitude) => altitude;

    [UserMapping]
    private static double Multiplier(SunIntensity intensity) => intensity;

    [UserMapping]
    private static double Hours(StandardOffset standard) => standard;
}
