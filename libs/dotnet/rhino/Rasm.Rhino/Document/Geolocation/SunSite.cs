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

[ValueObject<LocalDateTime>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[ObjectFactory<string>]
public sealed partial class SunMoment : System.Numerics.IMinMaxValue<SunMoment>, IConvertible<string> {
    public static SunMoment MinValue { get; } = new(Instant.FromUnixTimeSeconds(0).WithOffset(Offset.MaxValue).LocalDateTime);
    public static SunMoment MaxValue { get; } = new(Instant.FromUnixTimeSeconds(uint.MaxValue).WithOffset(Offset.MinValue).LocalDateTime);

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

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref LocalDateTime value) =>
        (validationError, value) = value.With(TimeAdjusters.TruncateToSecond) switch {
            var whole => (whole.CompareTo(MinValue._value) >= 0 && whole.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue(), whole),
        };
}

public sealed record SunSite(GeoCoordinate Place, NorthAngle North, Offset Standard, Option<Offset> Saving) {
    public static SunSite Default { get; } = new(new GeoCoordinate(Latitude.Default, Longitude.Default), NorthAngle.Default, Offset.Zero, None);

    public Offset Wall => Standard + Saving.IfNone(Offset.Zero);

    public double StandardHours => Standard.Seconds / (double)NodaConstants.SecondsPerHour;

    public Option<int> SavingMinutes => Saving.Map(static held => held.Seconds / NodaConstants.SecondsPerMinute);

    public static Fin<SunSite> Read(Sun sun) =>
        (GeoCoordinate.From(sun.Latitude, sun.Longitude).ToValidation(),
         Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(sun.North).ToValidation(),
         StandardOffset(sun.TimeZone).ToValidation())
            .Apply((place, north, standard) => new SunSite(
                place, north, standard,
                Callbacks.Found(sun.DaylightSavingOn, Offset.FromSeconds(sun.DaylightSavingMinutes * NodaConstants.SecondsPerMinute))))
            .As()
            .ToFin();

    public static Fin<SunSite> Placed(GeoCoordinate place, NorthAngle north, SunMoment moment) =>
        from zone in Zone(place)
        let interval = zone.MapLocal((LocalDateTime)moment).EarlyInterval
        select new SunSite(place, north, interval.StandardOffset, Some(interval.Savings).Filter(static savings => savings != Offset.Zero));

    public static Fin<SunSite> FromAnchor(EarthAnchor anchor, SunMoment moment) =>
        from located in anchor.Located
        from north in Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(double.RadiansToDegrees(Math.Atan2(located.Compass.YAxis.Y, located.Compass.YAxis.X)))
        from site in Placed(located.Earth.Place, north, moment)
        select site;

    public static IO<SunSite> Here(NorthAngle north, SunMoment moment) =>
        IO.lift(() => Refused.Unless(Sun.Here(out double latitude, out double longitude), nameof(Sun.Here))
            .Bind(_ => GeoCoordinate.From(latitude, longitude))
            .Bind(place => Placed(place, north, moment)));

    public Instant InstantOf(LocalDateTime local) => local.WithOffset(Wall).ToInstant();

    public IO<SunSample> Sample(LocalDateTime local) =>
        Observed(sun => (Direction(sun, InstantOf(local)), sun.Altitude) switch {
            var (direction, altitude) => new SunSample(
                direction, altitude, Sun.ColorFromAltitude(altitude),
                Sun.JulianDay(StandardHours, SavingMinutes.IfNone(0), local.ToDateTimeUnspecified(), local.TimeOfDay.NanosecondOfDay / (double)NodaConstants.NanosecondsPerHour)),
        });

    public SunDay Day(LocalDate date) =>
        new SolarTerms(InstantOf(date.At(LocalTime.Noon))) switch {
            var terms => (terms.Transit(Place.Longitude), terms.Rising(Place.Latitude)) switch {
                (var noon, < -1d) => new SunDay.PolarDay(noon),
                (var noon, > 1d) => new SunDay.PolarNight(noon),
                (var noon, var rising) => Duration.FromDays(Math.Acos(rising) / double.Tau) switch {
                    var half => new SunDay.RiseAndSet(noon - half, noon, noon + half),
                },
            },
        };

    public IO<SunPaths> Paths(SunMoment moment, Sphere dome) =>
        from placed in IO.lift(Refused.Unless(dome.IsValid, dome, nameof(Sphere.IsValid)))
        let year = ((LocalDateTime)moment).Year
        let days = toSeq(new DateInterval(new LocalDate(year, 1, 1), new LocalDate(year, 12, 31)))
        let hours = toSeq(Range(0, NodaConstants.HoursPerDay)).Map(static hour => LocalTime.Midnight.PlusHours(hour))
        from sky in Observed(sun => Arr.createRange(from date in days from hour in hours select -Direction(sun, date.At(hour).WithOffset(Standard).ToInstant())))
        let solstices = days.Map((date, row) => (Row: row, new SolarTerms(date.At(LocalTime.Noon).WithOffset(Standard).ToInstant()).Declination))
        select new SunPaths(
            hours.Map((hour, column) => new SunPath(
                hour,
                Arcs(toSeq(Range(0, days.Count)).Map(row => sky[(row * hours.Count) + column])).Map(arc => arc.Map(point => Placed(placed, point))))),
            Meshed(sky, Band(sky, hours.Count, solstices.MaxBy(static day => day.Declination).Row, solstices.MinBy(static day => day.Declination).Row), placed));

    private IO<TResult> Observed<TResult>(Func<Sun, TResult> read) =>
        use(static () => new Sun())
            .Bind(sun => IO.lift(() => {
                sun.Accuracy = Sun.Accuracies.Maximum;
                SunSiteMapper.Update(this with { Saving = None }, sun);
                return read(sun);
            }))
            .Bracket();

    private Vector3d Direction(Sun sun, Instant at) {
        SunSiteMapper.Update(at.WithOffset(Standard).LocalDateTime, sun);
        return sun.Vector;
    }

    private static Fin<Offset> StandardOffset(double hours) =>
        Math.Round(hours * NodaConstants.SecondsPerHour, MidpointRounding.ToEven) is var seconds && Math.Abs(seconds) <= Offset.MaxValue.Seconds
            ? Offset.FromSeconds((int)seconds)
            : new OffsetOutOfRange(hours);

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

    private static Seq<Seq<(int A, int B)>> Band(Arr<Vector3d> sky, int columns, int june, int december) =>
        from row in toSeq(Range(june, december - june))
        from column in toSeq(Range(0, columns))
        let quad = Seq((row * columns) + column, (row * columns) + ((column + 1) % columns), ((row + 1) * columns) + ((column + 1) % columns), ((row + 1) * columns) + column)
        from polygon in quad.ForAll(slot => sky[slot].Z > 0d)
            ? Seq(quad.Map(static slot => (slot, slot)))
            : Seq(Seq(quad[0], quad[1], quad[2]), Seq(quad[0], quad[2], quad[3])).Map(triangle => Clipped(sky, triangle)).Filter(static clipped => !clipped.IsEmpty)
        select polygon;

    private static Seq<(int A, int B)> Clipped(Arr<Vector3d> sky, Seq<int> corners) =>
        corners.Zip(corners.Tail.Concat(corners.Take(1))).Bind(edge => Seq(
            Some((edge.First, edge.First)).Filter(_ => sky[edge.First].Z > 0d),
            Some((Math.Min(edge.First, edge.Second), Math.Max(edge.First, edge.Second))).Filter(_ => (sky[edge.First].Z > 0d) != (sky[edge.Second].Z > 0d))).Somes());

    private static SunCoverage Meshed(Arr<Vector3d> sky, Seq<Seq<(int A, int B)>> polygons, Sphere dome) =>
        polygons.Bind(static polygon => polygon).Distinct() switch {
            var keys => toHashMap(keys.Map(static (key, index) => (key, index))) switch {
                var indices => new SunCoverage(
                    keys.Map(key => Placed(dome, key.A == key.B ? sky[key.A] : Crossing(sky[key.A], sky[key.B]))),
                    polygons.Map(polygon => polygon.Map(key => indices[key])).Map(static at => new MeshFace(at[0], at[1], at[2], at[^1]))),
            },
        };

    private static Vector3d Crossing(Vector3d from, Vector3d to) => from + ((to - from) * (from.Z / (from.Z - to.Z)));

    private static Point3d Placed(Sphere dome, Vector3d sky) => dome.Center + (dome.Radius * sky);

    private readonly record struct SolarTerms(Instant At) {
        public double Declination => Math.Asin(Math.Sin(Obliquity) * Math.Sin(ApparentLongitude));

        public Instant Transit(Longitude longitude) =>
            At - Duration.FromDays(Math.IEEERemainder(
                (double.Tau * At.InUtc().TimeOfDay.NanosecondOfDay / NodaConstants.NanosecondsPerDay) + double.DegreesToRadians(longitude) + Equation - Math.PI,
                double.Tau) / double.Tau);

        public double Rising(Latitude latitude) =>
            (Math.SinCos(double.DegreesToRadians(latitude)), Math.SinCos(Declination)) switch {
                var (site, sun) => (Math.Cos(double.DegreesToRadians(90.833d)) - (site.Sin * sun.Sin)) / (site.Cos * sun.Cos),
            };

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
    private SunDay(Instant noon) => Noon = noon;

    public Instant Noon { get; }

    public abstract Duration Length { get; }

    public sealed record RiseAndSet(Instant Rise, Instant Noon, Instant Set) : SunDay(Noon) {
        public override Duration Length => Set - Rise;
    }

    public sealed record PolarDay(Instant Noon) : SunDay(Noon) {
        public override Duration Length => Duration.FromDays(1);
    }

    public sealed record PolarNight(Instant Noon) : SunDay(Noon) {
        public override Duration Length => Duration.Zero;
    }
}

public sealed record SunPath(LocalTime Hour, Seq<Seq<Point3d>> Arcs);

public sealed record SunCoverage(Seq<Point3d> Vertices, Seq<MeshFace> Faces) {
    public Mesh ToMesh() {
        Mesh mesh = new();
        mesh.Vertices.AddVertices(Vertices);
        _ = mesh.Faces.AddFaces(Faces);
        _ = mesh.Normals.ComputeNormals();
        return mesh;
    }
}

public sealed record SunPaths(Seq<SunPath> Analemmas, SunCoverage Coverage);

public sealed record SunAngles(double Azimuth, double Altitude);

[Union]
public abstract partial record SunWindow {
    public abstract LocalDateTime Start { get; }

    public abstract LocalDateTime End { get; }

    public abstract Period Step { get; }

    public Seq<LocalDateTime> Frames => toSeq(LanguageExt.List.unfold(Start, at => at <= End ? Some((at, at.Plus(Step))) : None));

    private static Validation<Error, SunMoment> Moment(LocalDateTime at) =>
        Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(at).ToValidation();

    private static Validation<Error, Unit> Ordered(LocalDateTime start, LocalDateTime end) =>
        start <= end ? unit : new ReversedWindow(start, end);

    public sealed record Day : SunWindow {
        private Day(LocalDate date, LocalTime from, LocalTime until, int minutesBetweenFrames) =>
            (Date, From, Until, MinutesBetweenFrames) = (date, from, until, minutesBetweenFrames);

        public LocalDate Date { get; }

        public LocalTime From { get; }

        public LocalTime Until { get; }

        public int MinutesBetweenFrames { get; }

        public override LocalDateTime Start => Date.At(From);

        public override LocalDateTime End => Date.At(Until);

        public override Period Step => Period.FromMinutes(MinutesBetweenFrames);

        public static Fin<Day> Create(LocalDate date, LocalTime from, LocalTime until, Period step) =>
            (Moment(date.At(from)), Moment(date.At(until)), Ordered(date.At(from), date.At(until)),
             (step.HasDateComponent ? Fin.Fail<int>(new InvalidStudyStep(step)) : Conversions.Whole(step.ToDuration(), Duration.FromMinutes(1)))
                .Bind(minutes => minutes >= 1 ? Fin.Succ(minutes) : Fin.Fail<int>(new InvalidStudyStep(step)))
                .ToValidation())
                .Apply((_, _, _, minutes) => new Day(date, from, until, minutes))
                .As()
                .ToFin();
    }

    [ValidationError<InvalidRhinoValue>]
    [ObjectFactory<string>]
    public sealed partial record Season : SunWindow, IConvertible<string> {
        private Season(DateInterval dates, LocalTime at, int daysBetweenFrames) =>
            (Dates, At, DaysBetweenFrames) = (dates, at, daysBetweenFrames);

        public DateInterval Dates { get; }

        public LocalTime At { get; }

        public int DaysBetweenFrames { get; }

        public override LocalDateTime Start => Dates.Start.At(At);

        public override LocalDateTime End => Dates.End.At(At);

        public override Period Step => Period.FromDays(DaysBetweenFrames);

        public static Fin<Season> Create(LocalDate from, LocalDate until, LocalTime at, Period step) =>
            (Moment(from.At(at)), Moment(until.At(at)), Ordered(from.At(at), until.At(at)),
             step.Normalize() is var normal && normal == Period.FromDays(normal.Days) && normal.Days >= 1
                 ? Validation.Success<Error, int>(normal.Days)
                 : Validation.Fail<Error, int>(new InvalidStudyStep(step)))
                .Apply((_, _, _, days) => new Season(new DateInterval(from, until), at, days))
                .As()
                .ToFin();

        public static InvalidRhinoValue? Validate(string? value, IFormatProvider? provider, out Season? item) {
            item = value?.Split('/') is [var from, var until, var at, var days]
                && LocalDatePattern.Iso.Parse(from) is { Success: true } start
                && LocalDatePattern.Iso.Parse(until) is { Success: true } end
                && LocalTimePattern.ExtendedIso.Parse(at) is { Success: true } time
                && int.TryParse(days, NumberStyles.None, CultureInfo.InvariantCulture, out int count)
                    ? Create(start.Value, end.Value, time.Value, Period.FromDays(count)).Match<Season?>(Succ: static season => season, Fail: static _ => null)
                    : null;
            return item is null && value is not null ? new InvalidRhinoValue() : null;
        }

        public string ToValue() =>
            string.Join('/', LocalDatePattern.Iso.Format(Dates.Start), LocalDatePattern.Iso.Format(Dates.End), LocalTimePattern.ExtendedIso.Format(At), DaysBetweenFrames.ToString(CultureInfo.InvariantCulture));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SunSiteMapper {
    internal static void Update(SunSite site, Sun sun) {
        Map(site, sun);
        _ = site.SavingMinutes.IfSome(minutes => sun.DaylightSavingMinutes = minutes);
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@SunSite.Place.Latitude), nameof(Sun.Latitude))]
    [MapProperty(nameof(@SunSite.Place.Longitude), nameof(Sun.Longitude))]
    [MapProperty(nameof(SunSite.StandardHours), nameof(Sun.TimeZone))]
    [MapProperty(nameof(@SunSite.Saving.IsSome), nameof(Sun.DaylightSavingOn))]
    [MapperIgnoreSource(nameof(SunSite.SavingMinutes), Justification = "Update writes minutes only when daylight saving is present")]
    [MapperIgnoreTarget(nameof(Sun.DaylightSavingMinutes), Justification = "Update preserves stored minutes when daylight saving is absent")]
    [MapperIgnoreSource(nameof(SunSite.Standard), Justification = "Written as StandardHours")]
    [MapperIgnoreSource(nameof(SunSite.Wall), Justification = "Sum of Standard and Saving")]
    private static partial void Map(SunSite site, Sun sun);

    internal static partial SunAngles ToAngles(Sun sun);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@SunSite.Place.Latitude), nameof(AnimationProperties.Latitude))]
    [MapProperty(nameof(@SunSite.Place.Longitude), nameof(AnimationProperties.Longitude))]
    [MapProperty(nameof(SunSite.North), nameof(AnimationProperties.NorthAngle))]
    [MapperIgnoreSource(nameof(SunSite.Standard), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.Saving), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.StandardHours), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.SavingMinutes), Justification = "AnimationProperties stores no offset")]
    [MapperIgnoreSource(nameof(SunSite.Wall), Justification = "AnimationProperties stores no offset")]
    internal static partial void Update(SunSite site, AnimationProperties properties);

    internal static void Update(SunWindow window, AnimationProperties properties) =>
        window.Switch(properties, day: static (target, day) => Update(day, target), season: static (target, season) => Update(season, target));

    internal static void Update(LocalDateTime wall, Sun sun) =>
        sun.SetDateTime(wall.ToDateTimeUnspecified(), DateTimeKind.Local);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapValue(nameof(AnimationProperties.CaptureType), AnimationProperties.CaptureTypes.DaySunStudy)]
    [MapProperty(nameof(@SunWindow.Start.Minute), nameof(AnimationProperties.StartMinutes))]
    [MapProperty(nameof(@SunWindow.Start.Second), nameof(AnimationProperties.StartSeconds))]
    [MapProperty(nameof(@SunWindow.End.Minute), nameof(AnimationProperties.EndMinutes))]
    [MapProperty(nameof(@SunWindow.End.Second), nameof(AnimationProperties.EndSeconds))]
    [MapperIgnoreSource(nameof(SunWindow.Day.Date), Justification = "Written through Start and End")]
    [MapperIgnoreSource(nameof(SunWindow.Day.From), Justification = "Written through Start")]
    [MapperIgnoreSource(nameof(SunWindow.Day.Until), Justification = "Written through End")]
    [MapperIgnoreSource(nameof(SunWindow.Step), Justification = "Written as MinutesBetweenFrames")]
    [MapperIgnoreSource(nameof(SunWindow.Frames), Justification = "Read by previews alone")]
    private static partial void Update(SunWindow.Day day, AnimationProperties properties);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapValue(nameof(AnimationProperties.CaptureType), AnimationProperties.CaptureTypes.SeasonalSunStudy)]
    [MapProperty(nameof(@SunWindow.Start.Minute), nameof(AnimationProperties.StartMinutes))]
    [MapProperty(nameof(@SunWindow.Start.Second), nameof(AnimationProperties.StartSeconds))]
    [MapProperty(nameof(@SunWindow.End.Minute), nameof(AnimationProperties.EndMinutes))]
    [MapProperty(nameof(@SunWindow.End.Second), nameof(AnimationProperties.EndSeconds))]
    [MapperIgnoreSource(nameof(SunWindow.Season.Dates), Justification = "Written through Start and End")]
    [MapperIgnoreSource(nameof(SunWindow.Season.At), Justification = "Written through Start and End")]
    [MapperIgnoreSource(nameof(SunWindow.Step), Justification = "Written as DaysBetweenFrames")]
    [MapperIgnoreSource(nameof(SunWindow.Frames), Justification = "Read by previews alone")]
    private static partial void Update(SunWindow.Season season, AnimationProperties properties);

    [UserMapping]
    private static double Degrees(NorthAngle north) => north;

    [UserMapping]
    private static double Degrees(Latitude latitude) => latitude;

    [UserMapping]
    private static double Degrees(Longitude longitude) => longitude;
}
