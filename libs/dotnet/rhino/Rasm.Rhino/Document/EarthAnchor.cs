using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Latitude : System.Numerics.IMinMaxValue<Latitude> {
    public static Latitude MinValue { get; } = new(-90d);
    public static Latitude MaxValue { get; } = new(90d);
    public static Latitude Default { get; } = new(0d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Longitude : System.Numerics.IMinMaxValue<Longitude> {
    public static Longitude MinValue { get; } = new(-180d);
    public static Longitude MaxValue { get; } = new(180d);
    public static Longitude Default { get; } = new(0d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        (validationError, value) = double.IsFinite(value) ? (null, Math.IEEERemainder(value, 360d)) : (new ValidationError("longitude is not finite"), value);
}

public sealed record GeoCoordinate(Latitude Latitude, Longitude Longitude) {
    public static Fin<GeoCoordinate> From(double latitude, double longitude) =>
        (Latitude.Validated(latitude),
         Longitude.Validated(longitude))
            .Apply(static (north, east) => new GeoCoordinate(north, east))
            .As();
}

public sealed record EarthLocation(GeoCoordinate Place, Length Elevation);

public sealed record EarthAnchor(Option<EarthLocation> Earth, Option<EarthCoordinateSystem> Datum, Option<Plane> Compass, Option<string> Name, Option<string> Description) {
    public Fin<(EarthLocation Earth, Plane Compass)> Located =>
        (Earth.ToFin(new Refused(nameof(EarthAnchorPoint.EarthLocationIsSet), Refused.Absent)),
         Compass.ToFin(new Refused(nameof(EarthAnchorPoint.ModelLocationIsSet), Refused.Absent)))
            .Apply(static (earth, compass) => (Earth: earth, Compass: compass))
            .As();

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

    public static IO<EarthAnchor> Read(Func<EarthAnchorPoint> point) =>
        DisposalOps.Within(point, static held => IO.lift(() => Of(held)));

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

    public IO<Unit> Write(Func<EarthAnchorPoint> point, Action<EarthAnchorPoint> commit) =>
        DisposalOps.Within(point, held => Apply(held).Bind(_ => IO.lift(() => commit(held))));
}

public sealed record Georeference {
    private Georeference(Transform modelToEarth, Transform earthToModel) => (ModelToEarth, EarthToModel) = (modelToEarth, earthToModel);

    public Transform ModelToEarth { get; }

    public Transform EarthToModel { get; }

    public static IO<Georeference> Read(RhinoDoc document) => EarthAnchor.Read(() => document.EarthAnchorPoint).Bind(anchor => Of(anchor, document.ModelUnits));

    public static IO<Georeference> Of(EarthAnchor anchor, LengthUnit modelUnits) =>
        from located in IO.lift(anchor.Located)
        from georeference in DisposalOps.Within(static () => new EarthAnchorPoint(), scratch =>
            from applied in (anchor with { Earth = new EarthLocation(located.Earth.Place, Length.Zero) }).Apply(scratch)
            from modelToEarth in IO.lift(() => Refused.UnlessFound(scratch.ModelLocationIsSet(), nameof(EarthAnchorPoint.ModelLocationIsSet))
                .Map(_ => Transform.Translation(0d, 0d, located.Earth.Elevation.Meters.ToDouble()) * scratch.GetModelToEarthTransform(modelUnits)))
            from earthToModel in IO.lift(() => Refused.Unless(modelToEarth.TryGetInverse(out Transform inverse), inverse, nameof(Transform.TryGetInverse)))
            select new Georeference(modelToEarth, earthToModel))
        select georeference;

    public Fin<EarthLocation> ToEarth(Point3d point) =>
        (ModelToEarth * point) switch {
            var earth => GeoCoordinate.From(earth.Y, earth.X).Map(place => new EarthLocation(place, Length.FromMeters(earth.Z))),
        };

    public Point3d ToModel(EarthLocation location) =>
        EarthToModel * new Point3d(location.Place.Longitude, location.Place.Latitude, location.Elevation.Meters.ToDouble());
}
