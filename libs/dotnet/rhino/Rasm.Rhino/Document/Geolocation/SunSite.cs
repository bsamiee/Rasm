using System.Drawing;
using System.Globalization;
using NodaTime.Text;
using NodaTime.TimeZones;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document.Geolocation;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct NorthAngle : System.Numerics.IMinMaxValue<NorthAngle> {
    private const double Turn = 360d;

    public static NorthAngle MinValue { get; } = new(0d);
    public static NorthAngle MaxValue { get; } = new(double.BitDecrement(Turn));
    public static NorthAngle Default { get; } = new(90d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        (validationError, value) = double.IsFinite(value) ? (null, ((value % Turn) + Turn) % Turn) : (new InvalidRhinoValue(), value);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[ObjectFactory<Offset>]
public readonly partial struct StandardOffset : System.Numerics.IMinMaxValue<StandardOffset>, IConvertible<Offset> {
    public static StandardOffset MinValue { get; } = new(-12d);
    public static StandardOffset MaxValue { get; } = new(13d);
    public static StandardOffset Default { get; } = new(0d);

    public static InvalidRhinoValue? Validate(Offset value, IFormatProvider? provider, out StandardOffset item) =>
        Validate(value.Seconds / (double)NodaConstants.SecondsPerHour, provider, out item);

    public Offset ToValue() => Offset.FromSeconds((int)Math.Round(_value * NodaConstants.SecondsPerHour, MidpointRounding.ToEven));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        (validationError, value) = (Math.Round(value * NodaConstants.SecondsPerHour, MidpointRounding.ToEven) / NodaConstants.SecondsPerHour) switch {
            var whole => (whole.CompareTo(MinValue._value) >= 0 && whole.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue(), whole),
        };
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct DaylightSaving : System.Numerics.IMinMaxValue<DaylightSaving>, IConvertible<Offset> {
    public static DaylightSaving MinValue { get; } = new(0);
    public static DaylightSaving MaxValue { get; } = new(120);

    public Offset ToValue() => Offset.FromSeconds(_value * NodaConstants.SecondsPerMinute);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<LocalDateTime>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[ObjectFactory<string>]
public sealed partial class SunMoment : System.Numerics.IMinMaxValue<SunMoment>, IConvertible<string> {
    public static SunMoment MinValue { get; } = new(new LocalDate(1971, 1, 1).AtMidnight());
    public static SunMoment MaxValue { get; } = new(Instant.FromUnixTimeSeconds(uint.MaxValue).WithOffset(StandardOffset.MinValue.ToValue()).LocalDateTime);

    public static Fin<SunMoment> Read(Sun sun) =>
        Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(LocalDateTime.FromDateTime(sun.GetDateTime(DateTimeKind.Local)));

    public static IO<SunMoment> Now(SunSite site, TimeProvider clock) =>
        IO.lift(() => Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(clock.GetCurrentInstant().WithOffset(site.Wall).LocalDateTime));

    public static InvalidRhinoValue? Validate(string? value, IFormatProvider? provider, out SunMoment? item) {
        item = null;
        return value is null ? null
            : LocalDateTimePattern.ExtendedIso.Parse(value) is { Success: true } parsed ? Validate(parsed.Value, provider, out item)
            : new InvalidRhinoValue();
    }

    public string ToValue() => LocalDateTimePattern.ExtendedIso.Format(_value);

    internal Fin<Unit> Write(Sun sun, TimeProvider clock) =>
        _value.ToDateTimeUnspecified() switch {
            var wall when clock.LocalTimeZone.IsInvalidTime(wall) => new SkippedLocalTime(_value),
            var wall => fun<DateTime, DateTimeKind>(sun.SetDateTime)(wall, DateTimeKind.Local),
        };

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref LocalDateTime value) =>
        (validationError, value) = value.With(TimeAdjusters.TruncateToSecond) switch {
            var whole => (whole.CompareTo(MinValue._value) >= 0 && whole.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue(), whole),
        };
}

public sealed record SunSite(GeoCoordinate Place, NorthAngle North, StandardOffset Standard, Option<DaylightSaving> Saving) {
    private const double SunriseZenith = 90.833d;

    public static SunSite Default { get; } = new(new GeoCoordinate(Latitude.Default, Longitude.Default), NorthAngle.Default, StandardOffset.Default, None);

    public Offset Wall => Standard.ToValue() + Saving.Map(static saving => saving.ToValue()).IfNone(Offset.Zero);

    public static Fin<SunSite> Read(Sun sun) =>
        (GeoCoordinate.From(sun.Latitude, sun.Longitude).ToValidation(),
         Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(sun.North).ToValidation(),
         Conversions.Validated<StandardOffset, double, InvalidRhinoValue>(sun.TimeZone).ToValidation(),
         Callbacks.Found(sun.DaylightSavingOn, sun.DaylightSavingMinutes).Traverse(static minutes => Conversions.Validated<DaylightSaving, int, InvalidRhinoValue>(minutes)).As().ToValidation())
            .Apply(static (place, north, standard, saving) => new SunSite(place, north, standard, saving))
            .As()
            .ToFin();

    public static Fin<SunSite> Placed(GeoCoordinate place, NorthAngle north, SunMoment moment) =>
        from interval in Zone(place).Map(zone => zone.MapLocal((LocalDateTime)moment).EarlyInterval)
        let positive = Some(interval.Savings).Filter(static offset => offset > Offset.Zero)
        from site in (Conversions.Validated<StandardOffset, Offset, InvalidRhinoValue>(interval.WallOffset - positive.IfNone(Offset.Zero)).ToValidation(),
                      positive.Traverse(static offset => Conversions.Whole(Duration.FromSeconds(offset.Seconds), Duration.FromMinutes(1)).Bind(static minutes => Conversions.Validated<DaylightSaving, int, InvalidRhinoValue>(minutes))).As().ToValidation())
            .Apply((standard, saving) => new SunSite(place, north, standard, saving))
            .As()
            .ToFin()
        select site;

    public static Fin<SunSite> FromAnchor(EarthAnchor anchor, SunMoment moment) =>
        from located in anchor.Located
        from north in Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(double.RadiansToDegrees(Math.Atan2(located.Compass.YAxis.Y, located.Compass.YAxis.X)))
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
        Solved(InstantOf(date.At(LocalTime.Noon)), 0d) switch {
            var noon => Horizon(new SolarTerms(noon).Declination, SunriseZenith) switch {
                <= -1d => new SunDay.PolarDay(noon),
                >= 1d => new SunDay.PolarNight(noon),
                _ => new SunDay.RiseAndSet(Solved(noon, -1d), noon, Solved(noon, 1d)),
            },
        };

    public IO<SunPaths> Paths(SunMoment moment, Sphere dome, TimeProvider clock) =>
        from placed in IO.lift(Refused.Unless(dome.IsValid, dome, nameof(Sphere.IsValid)))
        let year = ((LocalDateTime)moment).Year
        let days = toSeq(new DateInterval(new LocalDate(year, 1, 1), new LocalDate(year, 12, 31)))
        let hours = toSeq(Range(0, NodaConstants.HoursPerDay)).Map(static hour => LocalTime.Midnight.PlusHours(hour))
        let standard = Standard.ToValue()
        from loops in Observed(clock, (sun, computer) => hours.Traverse(hour => days.Traverse(date => Direction(sun, computer, date.At(hour).WithOffset(standard).ToInstant()))).As())
        let pole = double.DegreesToRadians(Place.Latitude)
        let declinations = days.Map(date => new SolarTerms(date.At(LocalTime.Noon).WithOffset(standard).ToInstant()).Declination)
        let band = (Lowest: Enumerable.Min(declinations), Highest: Enumerable.Max(declinations))
        let low = Math.Max(band.Lowest, pole - (Math.PI / 2d))
        let high = Math.Min(band.Highest, pole + (Math.PI / 2d))
        let rows = toSeq(Range(0, (int)Math.Ceiling((high - low) / double.DegreesToRadians(1d)) + 1))
        let columns = toSeq(Range(0, 361))
        let frame = Transform.Rotation(double.DegreesToRadians(North) - (Math.PI / 2d), Vector3d.ZAxis, Point3d.Origin) * Transform.Rotation(pole - (Math.PI / 2d), Vector3d.XAxis, Point3d.Origin)
        select new SunPaths(
            toMap(hours.Zip(loops).Map(path => (path.First, Arcs(path.Second.Map(static toward => -toward)).Map(arc => arc.Map(point => Projected(placed, point)))))),
            new SunCoverage(
                from row in rows
                let declination = low + ((high - low) * row / (rows.Count - 1))
                let half = Math.Acos(Math.Clamp(Horizon(declination, 90d), -1d, 1d))
                let tilt = Math.SinCos(declination)
                from column in columns
                let hour = Math.SinCos(half * ((2d * column / (columns.Count - 1)) - 1d))
                select Projected(placed, frame * new Vector3d(-tilt.Cos * hour.Sin, -tilt.Cos * hour.Cos, tilt.Sin)),
                from row in rows.Init
                from column in columns.Init
                let corner = (row * columns.Count) + column
                select (row == 0 && low > band.Lowest, row == rows.Count - 2 && high < band.Highest) switch {
                    (true, _) => new MeshFace(corner, corner + columns.Count + 1, corner + columns.Count),
                    (_, true) => new MeshFace(corner, corner + 1, corner + columns.Count),
                    _ => new MeshFace(corner, corner + 1, corner + columns.Count + 1, corner + columns.Count),
                }));

    private static IO<TResult> Observed<TResult>(TimeProvider clock, Func<Sun, DateTimeZone, Fin<TResult>> read) =>
        use(static () => new Sun { Accuracy = Sun.Accuracies.Maximum }).Bind(sun => IO.lift(() => read(sun, BclDateTimeZone.FromTimeZoneInfo(clock.LocalTimeZone)))).Bracket();

    private Fin<Vector3d> Direction(Sun sun, DateTimeZone computer, Instant at) =>
        at.InZone(computer) switch {
            var held => (Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(held.LocalDateTime).ToValidation(),
                         Conversions.Validated<StandardOffset, Offset, InvalidRhinoValue>(held.Offset).ToValidation())
                .Apply((_, offset) => {
                    SunSiteMapper.Update(this with { Standard = offset, Saving = None }, sun);
                    sun.SetDateTime(at.ToDateTimeUtc(), DateTimeKind.Local);
                    return sun.Vector;
                })
                .As()
                .ToFin(),
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
            .ToFin(new Missing(nameof(TzdbDateTimeZoneSource.ZoneLocations)));

    private Instant Solved(Instant start, double side) =>
        Range(0, 3).Fold(start, (at, _) => new SolarTerms(at) switch {
            var terms => at - Duration.FromDays(Math.IEEERemainder(
                terms.HourAngle(Place.Longitude) - (side * Math.Acos(Math.Clamp(Horizon(terms.Declination, SunriseZenith), -1d, 1d))),
                double.Tau) / double.Tau),
        });

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

    private static Point3d Projected(Sphere dome, Vector3d sky) => dome.Center + (dome.Radius * sky);

    private readonly record struct SolarTerms(Instant At) {
        public double Declination => Math.Asin(Math.Sin(Obliquity) * Math.Sin(ApparentLongitude));

        public double HourAngle(Longitude longitude) =>
            (double.Tau * At.InUtc().TimeOfDay.NanosecondOfDay / NodaConstants.NanosecondsPerDay) + double.DegreesToRadians(longitude) + Equation - Math.PI;

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
public abstract partial record SunWindow {
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

    private static Fin<Option<SunWindow>> Studied(AnimationProperties properties, Func<LocalDateTime, LocalDateTime, AnimationProperties, Fin<SunWindow>> window) =>
        window(
            new LocalDateTime(properties.StartYear, properties.StartMonth, properties.StartDay, properties.StartHour, properties.StartMinutes, properties.StartSeconds),
            new LocalDateTime(properties.EndYear, properties.EndMonth, properties.EndDay, properties.EndHour, properties.EndMinutes, properties.EndSeconds),
            properties).Map(static held => Some(held));

    private static Fin<TWindow> Bounded<TWindow>(LocalDateTime start, LocalDateTime end, Period step, Duration grain, Func<LocalDateTime, LocalDateTime, int, TWindow> window) =>
        (Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(start).ToValidation(),
         Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(end).ToValidation(),
         (start <= end ? Fin.Succ(unit) : new ReversedWindow(start, end)).ToValidation(),
         Some(step).Filter(held => held.Years == 0 && held.Months == 0 && held.ToDuration() >= grain).ToFin(new InvalidStudyStep(step))
             .Bind(held => Conversions.Whole(held.ToDuration(), grain))
             .ToValidation())
            .Apply((_, _, _, count) => window(start, end, count))
            .As()
            .ToFin();

    public sealed record Day : SunWindow {
        private Day(LocalDateTime start, LocalDateTime end, int minutesBetweenFrames) : base(start, end) => MinutesBetweenFrames = minutesBetweenFrames;

        public int MinutesBetweenFrames { get; }

        public static Fin<Day> Create(LocalDate date, LocalTime from, LocalTime until, Period step) =>
            Bounded(date.At(from), date.At(until), step, Duration.FromMinutes(1), static (start, end, minutes) => new Day(start, end, minutes));
    }

    [ValidationError<InvalidRhinoValue>]
    [ObjectFactory<string>]
    public sealed partial record Season : SunWindow, IConvertible<string> {
        private Season(LocalDateTime start, LocalDateTime end, int daysBetweenFrames) : base(start, end) => DaysBetweenFrames = daysBetweenFrames;

        public int DaysBetweenFrames { get; }

        public static Fin<Season> Create(LocalDate from, LocalDate until, LocalTime at, Period step) =>
            Bounded(from.At(at), until.At(at), step, Duration.FromDays(1), static (start, end, days) => new Season(start, end, days));

        public static InvalidRhinoValue? Validate(string? value, IFormatProvider? provider, out Season? item) =>
            (item = value?.Split('/') is [var from, var until, var at, var days]
                && LocalDatePattern.Iso.Parse(from) is { Success: true } start
                && LocalDatePattern.Iso.Parse(until) is { Success: true } end
                && LocalTimePattern.ExtendedIso.Parse(at) is { Success: true } time
                && int.TryParse(days, NumberStyles.None, CultureInfo.InvariantCulture, out int count)
                    ? Create(start.Value, end.Value, time.Value, Period.FromDays(count)).Match<Season?>(Succ: static season => season, Fail: static _ => null)
                    : null) is null && value is not null ? new InvalidRhinoValue() : null;

        public string ToValue() =>
            string.Join('/', LocalDatePattern.Iso.Format(Start.Date), LocalDatePattern.Iso.Format(End.Date), LocalTimePattern.ExtendedIso.Format(Start.TimeOfDay), DaysBetweenFrames.ToString(CultureInfo.InvariantCulture));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class SunSiteMapper {
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
    internal static partial SunAngles ToAngles(Sun sun);

    [MapProperty(nameof(@SunSite.Place.Latitude), nameof(AnimationProperties.Latitude))]
    [MapProperty(nameof(@SunSite.Place.Longitude), nameof(AnimationProperties.Longitude))]
    [MapProperty(nameof(SunSite.North), nameof(AnimationProperties.NorthAngle))]
    [MapperIgnoreSource(nameof(SunSite.Standard), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.Saving), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.Wall), Justification = "AnimationProperties stores no offset")]
    internal static partial void Update(SunSite site, AnimationProperties properties);

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
    [MapperIgnoreSource(nameof(SunWindow.Step), Justification = "Written as the case's frame spacing")]
    [MapperIgnoreSource(nameof(SunWindow.Frames), Justification = "Read by previews alone")]
    private static partial void Map(SunWindow window, AnimationProperties properties);

    [UserMapping]
    private static double Degrees(NorthAngle north) => north;

    [UserMapping]
    private static double Degrees(Latitude latitude) => latitude;

    [UserMapping]
    private static double Degrees(Longitude longitude) => longitude;

    [UserMapping]
    private static double Hours(StandardOffset standard) => standard;
}
