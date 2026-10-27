using Rhino;
using Rhino.DocObjects;
using UnitsNet;

namespace Rasm.RhinoCommon.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GeoCoordinate(Angle Latitude, Angle Longitude) {
    public static GeoCoordinate From(double latitude, double longitude) => new(Angle.FromDegrees(latitude), Angle.FromDegrees(longitude));
}

public sealed record EarthLocation(GeoCoordinate Place, Length Elevation);

public sealed record EarthAnchor(Option<EarthLocation> Earth, Option<EarthCoordinateSystem> Datum, Option<Plane> Compass, Option<string> Name, Option<string> Description) {
    public Fin<(EarthLocation Earth, Plane Compass)> Located =>
        (Conversions.Found(Earth),
         Conversions.Found(Compass))
            .Apply(static (earth, compass) => (Earth: earth, Compass: compass))
            .As();

    public static EarthAnchor Of(EarthAnchorPoint point) =>
        new(
            Conversions.Found(point.EarthLocationIsSet(), point)
                .Map(static held => new EarthLocation(GeoCoordinate.From(held.EarthBasepointLatitude, held.EarthBasepointLongitude), Length.FromMeters(held.EarthBasepointElevation))),
            Some(point.EarthBasepointElevationCoordinateSystem).Filter(static system => system is not EarthCoordinateSystem.Unset),
            Conversions.Found(point.ModelLocationIsSet(), point).Map(static held => held.GetModelCompass()),
            Conversions.Present(point.Name),
            Conversions.Present(point.Description));

    public static IO<EarthAnchor> Read(Func<EarthAnchorPoint> point) =>
        use(point).Bind(static held => IO.lift(() => Of(held))).Bracket();

    public IO<Unit> Apply(EarthAnchorPoint point) =>
        IO.lift(() => {
            point.EarthBasepointLatitude = Conversions.Unset(Earth.Map(static earth => earth.Place.Latitude.Degrees.ToDouble()));
            point.EarthBasepointLongitude = Conversions.Unset(Earth.Map(static earth => earth.Place.Longitude.Degrees.ToDouble()));
            point.EarthBasepointElevation = Conversions.Unset(Earth.Map(static earth => earth.Elevation.Meters.ToDouble()));
            point.EarthBasepointElevationCoordinateSystem = Datum.IfNone(EarthCoordinateSystem.Unset);
            _ = Compass.Iter(compass => (point.ModelBasePoint, point.ModelEast, point.ModelNorth) = (compass.Origin, compass.XAxis, compass.YAxis));
            point.Name = Conversions.Unset(Name);
            point.Description = Conversions.Unset(Description);
        });

    public IO<Unit> Write(Func<EarthAnchorPoint> point, Action<EarthAnchorPoint> commit) =>
        (from held in use(point)
         from applied in Apply(held)
         from committed in IO.lift(() => commit(held))
         select committed).Bracket();
}

public sealed record Georeference {
    private Georeference(Transform modelToEarth, Transform earthToModel) => (ModelToEarth, EarthToModel) = (modelToEarth, earthToModel);

    public Transform ModelToEarth { get; }

    public Transform EarthToModel { get; }

    public static IO<Georeference> Read(RhinoDoc document) => EarthAnchor.Read(() => document.EarthAnchorPoint).Bind(anchor => Of(anchor, document.ModelUnits));

    public static IO<Georeference> Of(EarthAnchor anchor, LengthUnit modelUnits) =>
        from located in IO.lift(anchor.Located)
        from georeference in (from scratch in use(static () => new EarthAnchorPoint())
                              from applied in (anchor with { Earth = new EarthLocation(located.Earth.Place, Length.Zero) }).Apply(scratch)
                              from modelToEarth in IO.lift(() => RhinoFault.Unless(scratch.ModelLocationIsSet())
                                  .Map(_ => Transform.Translation(0d, 0d, located.Earth.Elevation.Meters.ToDouble()) * scratch.GetModelToEarthTransform(modelUnits)))
                              from earthToModel in IO.lift(() => RhinoFault.Unless(modelToEarth.TryGetInverse(out Transform inverse)).Map(_ => inverse))
                              select new Georeference(modelToEarth, earthToModel)).Bracket()
        select georeference;

    public EarthLocation ToEarth(Point3d point) =>
        (ModelToEarth * point) switch {
            var earth => new EarthLocation(GeoCoordinate.From(earth.Y, earth.X), Length.FromMeters(earth.Z)),
        };

    public Point3d ToModel(EarthLocation location) =>
        EarthToModel * new Point3d(location.Place.Longitude.Degrees.ToDouble(), location.Place.Latitude.Degrees.ToDouble(), location.Elevation.Meters.ToDouble());
}
