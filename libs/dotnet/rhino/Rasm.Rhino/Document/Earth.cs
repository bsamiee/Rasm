using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.DocObjects;
using UnitsNet;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GeoCoordinate {
    private GeoCoordinate(Angle latitude, Angle longitude) => (Latitude, Longitude) = (latitude, longitude);

    public Angle Latitude { get; }

    public Angle Longitude { get; }

    public static Fin<GeoCoordinate> Validated(Angle latitude, Angle longitude) =>
        (RhinoFault.Unless(Math.Abs(latitude.Degrees.ToDouble()) <= 90d, RhinoReason.Outside, latitude),
         RhinoFault.Unless(Math.Abs(longitude.Degrees.ToDouble()) <= 180d, RhinoReason.Outside, longitude))
            .Apply((_, _) => new GeoCoordinate(latitude, longitude))
            .As();

    public static Fin<GeoCoordinate> From(double latitude, double longitude) => Validated(Angle.FromDegrees(Quantities.Exact(latitude)), Angle.FromDegrees(Quantities.Exact(longitude)));
}

public sealed record EarthLocation(GeoCoordinate Place, Length Elevation);

public sealed record Georeference {
    private Georeference(Transform modelToEarth, Transform earthToModel) => (ModelToEarth, EarthToModel) = (modelToEarth, earthToModel);

    public Transform ModelToEarth { get; }

    public Transform EarthToModel { get; }

    public static IO<Georeference> Read(RhinoDoc doc) =>
        (from scratch in use(() => doc.EarthAnchorPoint)
         from located in IO.lift(() => RhinoFault.Unless(scratch.EarthLocationIsSet() && scratch.ModelLocationIsSet(), RhinoReason.Missing, scratch.Name))
         from elevation in IO.lift(() => scratch.EarthBasepointElevation)
         from projective in IO.lift(() => { scratch.EarthBasepointElevation = 0d; })
         from modelToEarth in IO.lift(() => Transform.Translation(0d, 0d, elevation) * scratch.GetModelToEarthTransform(doc.ModelUnits))
         from earthToModel in Products.Accepted(() => (modelToEarth.TryGetInverse(out Transform inverse), inverse))
         select new Georeference(modelToEarth, earthToModel)).Bracket();

    public Fin<EarthLocation> ToEarth(Point3d point) =>
        (ModelToEarth * point) switch {
            var earth => GeoCoordinate.From(earth.Y, earth.X).Map(place => new EarthLocation(place, Length.FromMeters(Quantities.Exact(earth.Z)))),
        };

    public Point3d ToModel(EarthLocation location) =>
        EarthToModel * new Point3d(location.Place.Longitude.Degrees.ToDouble(), location.Place.Latitude.Degrees.ToDouble(), location.Elevation.Meters.ToDouble());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Earth {
    // --- [STAGE]
    public static Stage<EarthAnchorPoint> Stage(RhinoDoc doc, Seq<RowEdit<EarthAnchorPoint>> edits) =>
        new(
            Copy: _ => IO.lift(() => doc.EarthAnchorPoint),
            Unchanged: (live, _) => edits.ForAll(edit => edit.Holds.Exists(holds => holds(live))),
            Commit: (live, staged) =>
                from written in Commits.Undoable(doc, nameof(RhinoDoc.EarthAnchorPoint), IO.pure(live), anchor => IO.lift(() => { doc.EarthAnchorPoint = anchor; }), staged)
                from landed in use(() => doc.EarthAnchorPoint).Bind(read => IO.lift(() => Products.Each(edits, (edit, index) =>
                    edit.Holds.ForAll(holds => holds(read)) ? Fin.Succ(unit) : new RhinoFault(edit.Write.Member, RhinoReason.Declined, index)))).Bracket()
                select staged);

    public static IO<EarthAnchorPoint> Edit(RhinoDoc doc, Seq<RowEdit<EarthAnchorPoint>> edits) =>
        IO.lift(() => doc.EarthAnchorPoint).Bind(live => Edits.Apply(Stage(doc, edits), live, edits.Map(static edit => edit.Write)));

    // --- [EDITS]
    public static Seq<RowEdit<EarthAnchorPoint>> Located(EarthLocation location) => [
        Rows.Set<EarthAnchorPoint, double>(static anchor => anchor.EarthBasepointLatitude, static (anchor, held) => anchor.EarthBasepointLatitude = held, location.Place.Latitude.Degrees.ToDouble()),
        Rows.Set<EarthAnchorPoint, double>(static anchor => anchor.EarthBasepointLongitude, static (anchor, held) => anchor.EarthBasepointLongitude = held, location.Place.Longitude.Degrees.ToDouble()),
        Rows.Set<EarthAnchorPoint, double>(static anchor => anchor.EarthBasepointElevation, static (anchor, held) => anchor.EarthBasepointElevation = held, location.Elevation.Meters.ToDouble()),
    ];

    public static RowEdit<EarthAnchorPoint> North(Angle north) =>
        Rows.Write<EarthAnchorPoint>(anchor => IO.lift(() => {
            (double sin, double cos) = Math.SinCos(north.Radians.ToDouble());
            (anchor.ModelNorth, anchor.ModelEast) = (new Vector3d(-sin, cos, 0d), new Vector3d(cos, sin, 0d));
        }));
}
