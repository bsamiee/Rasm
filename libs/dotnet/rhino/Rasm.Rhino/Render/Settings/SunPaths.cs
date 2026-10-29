namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SunCoverage(Seq<Point3d> Vertices, Seq<MeshFace> Faces) {
    public Mesh ToMesh() {
        Mesh mesh = new();
        mesh.Vertices.AddVertices(Vertices);
        _ = mesh.Faces.AddFaces(Faces);
        _ = mesh.Normals.ComputeNormals();
        return mesh;
    }
}

public sealed record SunPaths(Map<LocalTime, Seq<Seq<Point3d>>> Analemmas, SunCoverage Coverage) {
    // --- [PATHS]
    public static IO<SunPaths> Of(SunSite site, int year, Sphere dome) =>
        from valid in IO.lift(() => RhinoFault.Unless(dome.IsValid, RhinoReason.Invalid, dome))
        let days = toSeq(new DateInterval(new LocalDate(year, 1, 1), new LocalDate(year, 12, 31)))
        let hours = toSeq(Range(0, NodaConstants.HoursPerDay)).Map(static hour => LocalTime.Midnight.PlusHours(hour))
        from grid in site.Observed(sun => days.Traverse(date => hours.Traverse(hour => SunSite.Universal(date.At(hour).WithOffset((Offset)site.Standard).ToInstant()).Map(utc => {
            sun.SetDateTime(utc, DateTimeKind.Utc);
            return -sun.Vector;
        })).As()).As())
        let sky = Arr.createRange(grid.Bind(static row => row))
        let axis = Polar(site)
        let noon = hours.Count / 2
        let declinations = days.Map((_, row) => (Row: row, Declination: Vector3d.Multiply(sky[(row * hours.Count) + noon], axis)))
        select new SunPaths(
            toMap(hours.Map((hour, column) => (hour, Arcs(toSeq(Range(0, days.Count)).Map(row => sky[(row * hours.Count) + column]))
                .Map(arc => arc.Map(point => Projected(dome, point)))))),
            Covered(Band(sky, hours.Count, declinations.MaxBy(static day => day.Declination).Row, declinations.MinBy(static day => day.Declination).Row), dome));

    // --- [SKY]
    private static Vector3d Polar(SunSite site) =>
        (Math.SinCos(site.North.Radians.ToDouble()), Math.SinCos(site.Place.Latitude.Radians.ToDouble())) switch {
            var ((northSin, northCos), (latitudeSin, latitudeCos)) => new Vector3d(latitudeCos * northCos, latitudeCos * northSin, latitudeSin),
        };

    private static Seq<Seq<Vector3d>> Arcs(Seq<Vector3d> loop) =>
        loop.Map(static (sky, index) => (Sky: sky, Index: index)).Find(static point => point.Sky.Z <= 0d).Match(
            Some: below => toSeq(Range(below.Index, loop.Count))
                .Map(step => (First: loop[step % loop.Count], Second: loop[(step + 1) % loop.Count]))
                .Fold((Arcs: Seq<Seq<Vector3d>>(), Open: Seq<Vector3d>()), static (cut, edge) => (edge.First.Z > 0d, edge.Second.Z > 0d) switch {
                    (true, true) => (cut.Arcs, cut.Open.Add(edge.Second)),
                    (false, false) => cut,
                    (var leaving, _) => Horizon(edge.First, edge.Second) switch {
                        var at => leaving ? (cut.Arcs.Add(cut.Open.Add(at)), Seq<Vector3d>()) : (cut.Arcs, Seq(at, edge.Second)),
                    },
                }).Arcs,
            None: () => Seq(loop.Concat(loop.Take(1))));

    private static Vector3d Horizon(Vector3d from, Vector3d to) => from + ((to - from) * (from.Z / (from.Z - to.Z)));

    private static Seq<Seq<Vector3d>> Band(Arr<Vector3d> sky, int columns, int june, int december) =>
        (from row in toSeq(Range(Math.Min(june, december), Math.Abs(december - june)))
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
                    .Map(span => Horizon(sky[span.Low], sky[span.High]))).Somes())
            : Seq<Vector3d>();

    private static SunCoverage Covered(Seq<Seq<Vector3d>> polygons, Sphere dome) =>
        polygons.Bind(static polygon => polygon).Distinct() switch {
            var points => toHashMap(points.Map(static (point, index) => (point, index))) switch {
                var indices => new SunCoverage(
                    points.Map(point => Projected(dome, point)),
                    polygons.Map(polygon => polygon.Map(point => indices[point])).Map(static at => new MeshFace(at[0], at[1], at[2], at[^1]))),
            },
        };

    private static Point3d Projected(Sphere dome, Vector3d sky) => dome.Center + (dome.Radius * sky);
}
