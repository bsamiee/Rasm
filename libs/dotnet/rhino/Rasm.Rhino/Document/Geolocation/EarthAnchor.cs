using Rasm.Rhino.Document.Shapes;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Document.Geolocation;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Latitude : System.Numerics.IMinMaxValue<Latitude> {
    public static Latitude MinValue { get; } = new(-90d);
    public static Latitude MaxValue { get; } = new(90d);
    public static Latitude Default { get; } = new(0d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Longitude : System.Numerics.IMinMaxValue<Longitude> {
    public static Longitude MinValue { get; } = new(-180d);
    public static Longitude MaxValue { get; } = new(180d);
    public static Longitude Default { get; } = new(0d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        (validationError, value) = double.IsFinite(value) ? (null, Math.IEEERemainder(value, 360d)) : (new InvalidRhinoValue(), value);
}

public sealed record GeoCoordinate(Latitude Latitude, Longitude Longitude) {
    public static Fin<GeoCoordinate> From(double latitude, double longitude) =>
        (Conversions.Validated<Latitude, double, InvalidRhinoValue>(latitude).ToValidation(),
         Conversions.Validated<Longitude, double, InvalidRhinoValue>(longitude).ToValidation())
            .Apply(static (north, east) => new GeoCoordinate(north, east))
            .As()
            .ToFin();
}

public sealed record EarthLocation(GeoCoordinate Place, Length Elevation);

public sealed record EarthAnchor(Option<EarthLocation> Earth, Option<EarthCoordinateSystem> Datum, Option<Plane> Compass, Option<string> Name, Option<string> Description) {
    public Fin<(EarthLocation Earth, Plane Compass)> Located =>
        (Earth.ToValidation<Error>(new Missing(nameof(EarthAnchorPoint.EarthLocationIsSet))),
         Compass.ToValidation<Error>(new Missing(nameof(EarthAnchorPoint.ModelLocationIsSet))))
            .Apply(static (earth, compass) => (Earth: earth, Compass: compass))
            .As()
            .ToFin();

    public static Fin<EarthAnchor> Of(EarthAnchorPoint point) =>
        Some(point).Filter(static held => held.EarthLocationIsSet())
            .Traverse(static held => GeoCoordinate.From(held.EarthBasepointLatitude, held.EarthBasepointLongitude)
                .Map(place => new EarthLocation(place, Length.FromMeters(held.EarthBasepointElevation))))
            .As()
            .Map(earth => new EarthAnchor(
                earth,
                Some(point.EarthBasepointElevationCoordinateSystem).Filter(static system => system is not EarthCoordinateSystem.Unset),
                Some(point).Filter(static held => held.ModelLocationIsSet()).Map(static held => held.GetModelCompass()),
                Conversions.Present(point.Name),
                Conversions.Present(point.Description)));
}

public sealed record Georeference {
    private Georeference(Transform modelToEarth, Transform earthToModel) => (ModelToEarth, EarthToModel) = (modelToEarth, earthToModel);

    public Transform ModelToEarth { get; }

    public Transform EarthToModel { get; }

    public static IO<Georeference> Of(EarthAnchor anchor, LengthUnit modelUnits) =>
        (from located in IO.lift(anchor.Located)
         from scratch in use(static () => new EarthAnchorPoint())
         from _ in EarthAnchors.Apply(scratch, anchor with { Earth = new EarthLocation(located.Earth.Place, Length.Zero) })
         from __ in IO.lift(() => Missing.Unless(scratch.ModelLocationIsSet(), nameof(EarthAnchorPoint.ModelLocationIsSet)))
         let modelToEarth = Transform.Translation(0d, 0d, located.Earth.Elevation.Meters.ToDouble()) * scratch.GetModelToEarthTransform(modelUnits)
         from earthToModel in IO.lift(Transformations.Inverse(modelToEarth))
         select new Georeference(modelToEarth, earthToModel)).Bracket();

    public Fin<EarthLocation> ToEarth(Point3d point) =>
        (ModelToEarth * point) switch {
            var earth => GeoCoordinate.From(earth.Y, earth.X).Map(place => new EarthLocation(place, Length.FromMeters(earth.Z))),
        };

    public Point3d ToModel(EarthLocation location) =>
        EarthToModel * new Point3d(location.Place.Longitude, location.Place.Latitude, location.Elevation.Meters.ToDouble());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EarthAnchors {
    // --- [READS]
    public static IO<EarthAnchor> Read(RhinoDoc document) =>
        use(() => document.EarthAnchorPoint).Bind(static point => IO.lift(() => EarthAnchor.Of(point))).Bracket();

    public static IO<Georeference> ReadGeoreference(RhinoDoc document) =>
        Read(document).Bind(anchor => Georeference.Of(anchor, document.ModelUnits));

    // --- [WRITES]
    public static IO<Unit> Apply(EarthAnchorPoint point, EarthAnchor anchor) =>
        IO.lift(() => {
            point.EarthBasepointLatitude = Conversions.Unset(anchor.Earth.Map(static earth => (double)earth.Place.Latitude));
            point.EarthBasepointLongitude = Conversions.Unset(anchor.Earth.Map(static earth => (double)earth.Place.Longitude));
            point.EarthBasepointElevation = Conversions.Unset(anchor.Earth.Map(static earth => earth.Elevation.Meters.ToDouble()));
            point.EarthBasepointElevationCoordinateSystem = anchor.Datum.IfNone(EarthCoordinateSystem.Unset);
            _ = anchor.Compass.Iter(compass => (point.ModelBasePoint, point.ModelEast, point.ModelNorth) = (compass.Origin, compass.XAxis, compass.YAxis));
            point.Name = Conversions.Unset(anchor.Name);
            point.Description = Conversions.Unset(anchor.Description);
        });

    public static IO<Unit> Write(RhinoDoc document, EarthAnchor anchor) =>
        use(() => document.EarthAnchorPoint).Bind(point =>
            unless(
                EarthAnchor.Of(point).Exists(held => held == anchor),
                from applied in Apply(point, anchor)
                from committed in IO.lift(() => { document.EarthAnchorPoint = point; })
                select unit).As()).Bracket();
}
