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
        Callbacks.Found(point.EarthLocationIsSet(), point)
            .Traverse(static held => GeoCoordinate.From(held.EarthBasepointLatitude, held.EarthBasepointLongitude)
                .Map(place => new EarthLocation(place, Length.FromMeters(held.EarthBasepointElevation))))
            .As()
            .Map(earth => new EarthAnchor(
                earth,
                Some(point.EarthBasepointElevationCoordinateSystem).Filter(static system => system is not EarthCoordinateSystem.Unset),
                Callbacks.Found(point.ModelLocationIsSet(), point).Map(static held => held.GetModelCompass()),
                Conversions.Present(point.Name),
                Conversions.Present(point.Description)));

    public static IO<EarthAnchor> Read(RhinoDoc document) =>
        use(() => document.EarthAnchorPoint).Bind(static point => IO.lift(() => Of(point))).Bracket();

    public IO<Unit> Apply(EarthAnchorPoint point) =>
        IO.lift(() => {
            point.EarthBasepointLatitude = Conversions.Unset(Earth.Map(static earth => (double)earth.Place.Latitude));
            point.EarthBasepointLongitude = Conversions.Unset(Earth.Map(static earth => (double)earth.Place.Longitude));
            point.EarthBasepointElevation = Conversions.Unset(Earth.Map(static earth => earth.Elevation.Meters.ToDouble()));
            point.EarthBasepointElevationCoordinateSystem = Datum.IfNone(EarthCoordinateSystem.Unset);
            _ = Compass.Iter(compass => (point.ModelBasePoint, point.ModelEast, point.ModelNorth) = (compass.Origin, compass.XAxis, compass.YAxis));
            point.Name = Conversions.Unset(Name);
            point.Description = Conversions.Unset(Description);
        });

    public IO<Unit> Write(RhinoDoc document) =>
        (from point in use(() => document.EarthAnchorPoint)
         from _ in Apply(point)
         from written in IO.lift(() => { document.EarthAnchorPoint = point; })
         select written).Bracket();
}

public sealed record Georeference {
    private Georeference(Transform modelToEarth, Transform earthToModel) => (ModelToEarth, EarthToModel) = (modelToEarth, earthToModel);

    public Transform ModelToEarth { get; }

    public Transform EarthToModel { get; }

    public static IO<Georeference> Read(RhinoDoc document) => EarthAnchor.Read(document).Bind(anchor => Of(anchor, document.ModelUnits));

    public static IO<Georeference> Of(EarthAnchor anchor, LengthUnit modelUnits) =>
        (from located in IO.lift(anchor.Located)
         from scratch in use(static () => new EarthAnchorPoint())
         from _ in (anchor with { Earth = new EarthLocation(located.Earth.Place, Length.Zero) }).Apply(scratch)
         from modelToEarth in IO.lift(() => Missing.Unless(scratch.ModelLocationIsSet(), nameof(EarthAnchorPoint.ModelLocationIsSet))
             .Map(_ => Transform.Translation(0d, 0d, located.Earth.Elevation.Meters.ToDouble()) * scratch.GetModelToEarthTransform(modelUnits)))
         from earthToModel in IO.lift(() => Refused.Unless(modelToEarth.TryGetInverse(out Transform inverse), inverse, nameof(Transform.TryGetInverse)))
         select new Georeference(modelToEarth, earthToModel)).Bracket();

    public Fin<EarthLocation> ToEarth(Point3d point) =>
        (ModelToEarth * point) switch {
            var earth => GeoCoordinate.From(earth.Y, earth.X).Map(place => new EarthLocation(place, Length.FromMeters(earth.Z))),
        };

    public Point3d ToModel(EarthLocation location) =>
        EarthToModel * new Point3d(location.Place.Longitude, location.Place.Latitude, location.Elevation.Meters.ToDouble());
}
