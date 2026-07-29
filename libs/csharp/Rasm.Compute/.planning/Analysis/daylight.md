# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner owns the `Discipline.Daylight` assessment arm. C# derives direct sun-hours, shadow fraction, sky-view factor, and Perez diffuse irradiance from the kernel `Rasm` solar almanac; climate-based CBDM and glare stay with the Python companion. Weather-less requests require an explicit site, and present weather failures remain typed failures rather than silently degrading.

Site and hourly direct-normal/diffuse-horizontal irradiance arrive through the energy lane's own `WeatherRef` surface read by the admitted OpenStudio `EpwFile` reader (`latitude()`/`longitude()`/`timeZone()`/`elevation()` headers, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` hourly reads); shadow and obstruction rays reuse the clash BVH through `ClashScale.Occluded` over the `AccelerationStructure` the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire decodes (`Solver/clash` owns the decode; the retired `ToAcceleration` member is never named) — one ray engine, never a daylight-local walk; the app stages that decoded scene on the request as `ObstructionScene`, its content key riding the assessment content-key fold so a re-shaded site re-keys. Solar position composes the kernel `Rasm/Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition.At`/`SunPath` over the validated `SolarSite` — the same owner `Rasm.AppUi` viewport sun-light and the Materials environment adapter compose, so no package-local ephemeris exists. Zero new central pins — `EpwFile` and the clash BVH are admitted substrate.

## [01]-[INDEX]

- [02]-[SKY_AND_SHADOW]: `RunDaylight` folds the Perez all-weather sky rows over the EPW ingress against the clash-BVH shadow-ray cast.

## [02]-[SKY_AND_SHADOW]

- Owner: `PerezBand` `[SmartEnum<string>]` the eight all-weather clearness bands, each carrying the five published `(a, b, c, d, e)` brightening coefficients as row data over the clearness index ε (the published table, never a hardcoded interpolation); `SkyState` the per-hour sky carrier (DNI + DHI, derived ε, resolved `PerezBand`); `WeatherIngress` the `EpwFile` boundary off the `WeatherRef` surface; `DaylightAnalysis` the runner fold.
- Cases: with weather — per-target `direct-sun-hours` (the `SunPath` sweep × the clash-BVH occlusion ray per above-horizon sample), `shadow-fraction`, `sky-view-factor` (the hemisphere ray fan), `perez-diffuse-irradiance` (the circumsolar + horizon-band + isotropic-dome three-term sum over the resolved band); weather-less — the degrade: the same geometric facts at the design days off the solar kernel over the request's explicit `Site`, the `sky-state` fact stating `"geometry-only"` inline, never a silently-defaulted sky; absent both weather and an explicit site the run rails `AssessmentInputMissing`.
- Entry: `Run(graph, request, geometry, clock)` resolves the target points and obstruction scene through the `GeometrySource` port (an unresolvable target rails `AnalysisFailed(Admission, Input)`), reads optional weather through `WeatherIngress.Read` (a present-but-malformed EPW rails typed; an absent EPW selects the geometry-only degrade over the request's explicit `Site`), and mints the fact stream; the governing ratio is the worst target's required/achieved sun-hours (EN 17037 minimum-sunlight, the route row's citation).
- Receipt: rides the one `ComputeReceipt.Assessment` case, no daylight-local receipt; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own pin), Rasm (project — the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire the staged scene decodes from, and the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition`/`SolarSite`/`SunPosition` solar almanac), Rasm.Element, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap on the same `SkyState` carrier; a new daylight fact (a window vertical-sky-component) is one fold over the same rays; annual CBDM/glare stays the Python companion's, an in-process Radiance-class loop the rejected form; zero new surface.
- Boundary: shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — one ray engine on the one acceleration owner, never a daylight-local traversal; sky ingress is the energy lane's own `WeatherRef` surface through the admitted `EpwFile` reader, never a second weather decode path nor a weather column on the daylight policy; the Perez coefficients ride `PerezBand` as the published table; the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac is composed, never re-derived.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Eight published Perez clearness bands carry five brightening coefficients as row data (overcast
// 1.000–1.065 through clear >6.2): the band resolves from the derived ε, the coefficients drive the circumsolar/horizon terms.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PerezBand {
    public static readonly PerezBand Overcast = new("overcast", 1.000, 1.065, a: -0.008, b: 0.588, c: -0.062, d: -0.060, e: 0.072);
    public static readonly PerezBand MostlyOvercast = new("mostly-overcast", 1.065, 1.230, a: 0.130, b: 0.683, c: -0.151, d: -0.019, e: 0.066);
    public static readonly PerezBand PartlyOvercast = new("partly-overcast", 1.230, 1.500, a: 0.330, b: 0.487, c: -0.221, d: 0.055, e: -0.064);
    public static readonly PerezBand Intermediate = new("intermediate", 1.500, 1.950, a: 0.568, b: 0.187, c: -0.295, d: 0.109, e: -0.152);
    public static readonly PerezBand MostlyClear = new("mostly-clear", 1.950, 2.800, a: 0.873, b: -0.392, c: -0.362, d: 0.226, e: -0.462);
    public static readonly PerezBand Clear = new("clear", 2.800, 4.500, a: 1.132, b: -1.237, c: -0.412, d: 0.288, e: -0.823);
    public static readonly PerezBand VeryClear = new("very-clear", 4.500, 6.200, a: 1.060, b: -1.600, c: -0.359, d: 0.264, e: -1.127);
    public static readonly PerezBand Pristine = new("pristine", 6.200, double.PositiveInfinity, a: 0.678, b: -0.327, c: -0.250, d: 0.156, e: -1.377);

    public double EpsilonLow { get; }
    public double EpsilonHigh { get; }
    public double A { get; }
    public double B { get; }
    public double C { get; }
    public double D { get; }
    public double E { get; }

    public static PerezBand OfClearness(double epsilon) =>
        Items.ToSeq().Find(band => epsilon >= band.EpsilonLow && epsilon < band.EpsilonHigh).IfNone(Overcast);
}

// --- [MODELS] ------------------------------------------------------------------------------
// One hour of sky: the EPW DNI/DHI pair, the derived Perez clearness ε (a function of DHI/DNI/solar-zenith), and the resolved band.
public readonly record struct SkyState(Instant At, double DirectNormalWm2, double DiffuseHorizontalWm2, PerezBand Band);

// App-staged obstruction scene carries the kernel spatial-wire snapshot content key,
// assessment content-key fold reads it — a re-shaded site re-keys), the AccelerationStructure the Solver/clash decode
// produced from Spatial.Apply(SpatialOp.Wire), and the federated triangle wire the occlusion rays walk.
public sealed record ObstructionScene(UInt128 Key, AccelerationStructure Index, ReadOnlyMemory<float> Triangles);

// Per-target finding crosses weather and geometry-only paths in one shape.
public readonly record struct DaylightFinding(NodeId Target, double SunHours, double ShadowFraction, double SkyViewFactor, double PerezDiffuseWm2);

// --- [BOUNDARIES] --------------------------------------------------------------------------
// EPW ingress off the WeatherRef surface: header site + hourly sky states under the SWIG OptionalDouble
// is_initialized()-then-get() discipline — an absent hourly value contributes no SkyState hour, never a fabricated zero.
// Exemption: the SWIG vector walk and per-handle using discipline are the native marshaling statement seam.
public static class WeatherIngress {
    public static Fin<(SolarSite Site, Seq<SkyState> Hours)> Read(WeatherRef weather, Option<SolarSite> overrideSite = default) {
        if (!File.Exists(weather.EpwPath)) {
            return Fin.Fail<(SolarSite, Seq<SkyState>)>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-weather-missing:{weather.EpwPath}>"));
        }
        try {
            using OpenStudio.Path epwPath = OpenStudio.OpenStudioUtilitiesCore.toPath(weather.EpwPath);
            using OpenStudio.EpwFile epw = new(epwPath);
            SolarSite site = overrideSite.Match(
                Some: static value => value,
                None: () => SolarSite.Create(epw.latitude(), epw.longitude(), epw.timeZone(), epw.elevation()));
            using OpenStudio.EpwDataPointVector data = epw.data();
            Instant yearStart = new LocalDate(2001, 1, 1).AtMidnight().WithOffset(Offset.FromTicks((long)(site.TimezoneHours * NodaConstants.TicksPerHour))).ToInstant();
            List<SkyState> hours = new((int)data.Count);
            for (int i = 0; i < data.Count; i++) {
                using OpenStudio.EpwDataPoint point = data[i];
                using OpenStudio.OptionalDouble dni = point.directNormalRadiation();
                using OpenStudio.OptionalDouble dhi = point.diffuseHorizontalRadiation();
                if (dni.is_initialized() && dhi.is_initialized() && double.IsFinite(dni.get()) && dni.get() >= 0.0 && double.IsFinite(dhi.get()) && dhi.get() >= 0.0) {
                    Instant at = yearStart + Duration.FromHours(i);
                    if (SolarPosition.At(site, at).AboveHorizon) { hours.Add(new SkyState(at, dni.get(), dhi.get(), Band(site, at, dni.get(), dhi.get()))); }
                }
            }
            return hours.Count > 0
                ? Fin.Succ((site, toSeq(hours)))
                : Fin.Fail<(SolarSite, Seq<SkyState>)>(new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Input, "<daylight-weather-no-valid-daylight-hours>"));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(SolarSite, Seq<SkyState>)>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-weather-malformed:{ex.GetType().Name}>"));
        }
    }

    // Perez clearness ε uses the hour's solar zenith: ε = ((DHI+DNI)/DHI + κz³)/(1 + κz³), κ = 1.041 (z radians);
    // Zero-diffuse hours read the overcast floor because clearness is undefined; derivation composes the one solar kernel.
    static PerezBand Band(SolarSite site, Instant at, double dni, double dhi) {
        double z = SolarPosition.At(site, at).ZenithDeg * Math.PI / 180.0;
        double kappa = 1.041 * z * z * z;
        return PerezBand.OfClearness(dhi > 0.0 ? ((dhi + dni) / dhi + kappa) / (1.0 + kappa) : 1.0);
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DaylightAnalysis {
    const int SunSamplesPerDay = 96;
    const double SunStepHours = 0.25;
    const int HemisphereAzimuths = 72;
    const int HemisphereAltitudes = 18;

    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, IClock clock) =>
        from _ in !request.DesignDays.IsEmpty && double.IsFinite(request.RequiredSunHours) && request.RequiredSunHours >= 0.0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing("<daylight-policy-invalid>"))
        from scene in DaylightScene.Of(graph, request, geometry)
        from weather in request.Weather.Match(
            Some: weather => WeatherIngress.Read(weather).Map(static value => Some(value)),
            None: static () => Fin.Succ(Option<(SolarSite Site, Seq<SkyState> Hours)>.None))
        // Site evidence is REQUIRED for any sun sweep: the EPW header supplies it under weather, the request's
        // explicit Site carries the geometry-only run, and absent both the run rails typed — never a fabricated site.
        from site in (weather.Map(static w => w.Site) | request.Site)
            .ToFin(new ComputeFault.AssessmentInputMissing("<daylight-site-unresolved:no-weather-and-no-explicit-site>"))
        from findings in scene.Targets.TraverseM(target => Target(scene, target, site, weather, request)).As()
        let govern = findings.Map(f => request.RequiredSunHours > 0.0
            ? f.SunHours > 0.0 ? request.RequiredSunHours / f.SunHours : double.MaxValue
            : double.NaN).Max() | double.NaN
        from perTarget in findings.TraverseM(f => AssessmentFact.Rows(
            AssessmentFact.Measure($"{f.Target.Value}/direct-sun-hours", Dimension.DurationDim, f.SunHours * 3600.0),
            AssessmentFact.Ratio($"{f.Target.Value}/shadow-fraction", f.ShadowFraction),
            AssessmentFact.Ratio($"{f.Target.Value}/sky-view-factor", f.SkyViewFactor))).As()
        from skyFacts in weather.Match(
            Some: w => findings.TraverseM(f => AssessmentFact.Measure($"{f.Target.Value}/perez-diffuse-irradiance", Dimension.IrradianceDim, f.PerezDiffuseWm2)).As()
                .Map(perez => Seq(AssessmentFact.Text("sky-state", $"perez:{Dominant(w.Hours).Key}")) + perez),
            // Degrade stated inline on the result — never a silently-defaulted sky.
            None: () => Fin.Succ(Seq(AssessmentFact.Text("sky-state", "geometry-only"))))
        select AssessmentResult.Of(
            request.Route,
            perTarget.Bind(static rows => rows) + skyFacts,
            govern,
            new Provenance("DaylightAnalysis", request.Route.Standard, request.Route.SolverVersion, clock.GetCurrentInstant()));

    // Per-target fold: the quarter-hour `SunPath` sweep over every design day, `ClashScale.Occluded` per sun sample,
    // (one ray engine — the clash BVH over the decoded kernel wire), the cosine-weighted sky-view hemisphere fan, and
    // Mean Perez three-term diffuse applies where sky evidence exists; a failed occlusion probe rails the typed wire fault.
    static Fin<DaylightFinding> Target(DaylightScene scene, NodeId target, SolarSite site, Option<(SolarSite Site, Seq<SkyState> Hours)> weather, AssessmentRequest.Daylight request) {
        Vector3 origin = scene.SamplePoints[target];
        Offset offset = Offset.FromTicks((long)(site.TimezoneHours * NodaConstants.TicksPerHour));
        Fin<Seq<(LocalDate Day, SunPosition Sun, bool Occluded)>> sweep = request.DesignDays
            .Bind(day => SolarPosition.SunPath(site, day.AtMidnight().WithOffset(offset).ToInstant(), Duration.FromHours(SunStepHours), SunSamplesPerDay)
                .Map(row => (Day: day, row.Sun)))
            .Filter(static row => row.Sun.AboveHorizon)
            .TraverseM(row => ClashScale.Occluded(scene.Scene, origin, row.Sun.Direction, scene.SceneDiameter)
                .Map(occluded => (row.Day, row.Sun, occluded)))
            .As();
        return sweep.Bind(samples => SkyView(scene, origin).Map(skyView => {
            int lit = samples.Count(static sample => !sample.Occluded);
            double sunHours = samples.GroupBy(static sample => sample.Day)
                .Map(day => day.Count(static sample => !sample.Occluded) * SunStepHours).Min() | 0.0;
            double shadow = samples.IsEmpty ? 0.0 : 1.0 - (double)lit / samples.Count;
            double perez = weather.Map(w => PerezDiffuse(w.Site, w.Hours, skyView, samples)).IfNone(0.0);
            return new DaylightFinding(target, sunHours, shadow, skyView, perez);
        }));
    }

    // Cosine-weighted hemisphere rays span azimuth/altitude, each weighted `sin(alt)·cos(alt)` so the
    // zenith patch and the horizon band contribute their solid-angle-projected share; SVF = unoccluded weight fraction.
    static Fin<double> SkyView(DaylightScene scene, Vector3 origin) =>
        toSeq(Enumerable.Range(0, HemisphereAzimuths * HemisphereAltitudes))
            .TraverseM(i => {
                double az = 2.0 * Math.PI * (i % HemisphereAzimuths) / HemisphereAzimuths;
                double alt = Math.PI / 2.0 * (0.5 + i / HemisphereAzimuths) / HemisphereAltitudes;
                Vector3 ray = new(Math.Cos(alt) * Math.Sin(az), Math.Cos(alt) * Math.Cos(az), Math.Sin(alt));
                double weight = Math.Sin(alt) * Math.Cos(alt);
                return ClashScale.Occluded(scene.Scene, origin, ray, scene.SceneDiameter).Map(occluded => (Weight: weight, Occluded: occluded));
            })
            .As()
            .Map(static rays => rays.Fold((Open: 0.0, Total: 0.0), static (acc, r) => (acc.Open + (r.Occluded ? 0.0 : r.Weight), acc.Total + r.Weight)))
            .Map(static acc => acc.Total > 0.0 ? acc.Open / acc.Total : 0.0);

    // Mean Perez three-term horizontal diffuse over sky hours combines an isotropic dome scaled by target SVF,
    // circumsolar (F1) passed only when the design-day sun samples read the sun visible, horizon band (F2) at grade —
    // F1 = A + B·Δ + C·z and F2 = D + E·Δ over the resolved band's row coefficients, Δ the sky-brightness surrogate.
    static double PerezDiffuse(SolarSite site, Seq<SkyState> hours, double skyView, Seq<(LocalDate Day, SunPosition Sun, bool Occluded)> samples) {
        double sunVisible = samples.IsEmpty ? 0.0 : samples.Count(static s => !s.Occluded) / (double)samples.Count;
        return hours.Filter(static h => h.DiffuseHorizontalWm2 > 0.0).Map(h => {
            double delta = Math.Min(h.DiffuseHorizontalWm2 / 1000.0, 1.0);
            double zenith = SolarPosition.At(site, h.At).ZenithDeg * Math.PI / 180.0;
            double f1 = Math.Max(0.0, h.Band.A + h.Band.B * delta + h.Band.C * zenith), f2 = h.Band.D + h.Band.E * delta;
            return h.DiffuseHorizontalWm2 * ((1.0 - f1) * skyView + f1 * sunVisible + Math.Max(f2, 0.0));
        }).Sum() / Math.Max(hours.Count, 1);
    }

    static PerezBand Dominant(Seq<SkyState> hours) =>
        hours.GroupBy(static h => h.Band).OrderByDescending(static g => g.Count()).Head.Map(static g => g.Key).IfNone(PerezBand.Overcast);
}

// Resolved scene: target sample points (GeometrySource-resolved footprint centroids, lifted off the plane) and the
// clash-admitted obstruction scene the occlusion rays walk (one Admit per assessment); SceneDiameter reads the root AABB off the wire so
// every ray's reach covers the federated scene.
public sealed record DaylightScene(Seq<NodeId> Targets, Map<NodeId, Vector3> SamplePoints, AdmittedScene Scene, float SceneDiameter) {
    const double SampleLiftM = 0.85;    // EN 17037 reference plane height above the resolved footprint

    public static Fin<DaylightScene> Of(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry) =>
        request.Targets
            .TraverseM(id => graph.Find<Node.Object>(id)
                .Bind(o => geometry.Footprint(o.Representations))
                .Filter(static f => !f.IsEmpty)
                .Bind(footprint => Centroid(footprint).Map(point => (Id: id, Point: point)))
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-target-unresolved:{id.Value}>")))
            .As()
            .Bind(points => {
                float diameter = Diameter(request.Scene.Index);
                return request.Scene.Key != UInt128.Zero && !request.Scene.Triangles.IsEmpty && diameter > 0f
                    // ONE clash admission per assessment: the scene admits here and every sun/sky ray reads it.
                    ? AdmittedScene.Of(request.Scene.Index, request.Scene.Triangles, ClashPolicy.Canonical)
                        .Map(admitted => new DaylightScene(
                            request.Targets,
                            points.Fold(Map<NodeId, Vector3>(), static (acc, point) => acc.Add(point.Id, point.Point)),
                            admitted,
                            diameter))
                    : Fin.Fail<DaylightScene>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<daylight-obstruction-scene-invalid>"));
            });

    static Fin<Vector3> Centroid(FootprintPolygon footprint) {
        Seq<Vector3> next = footprint.Ring.Skip(1).Add(footprint.Ring[0]);
        (double Cross, double X, double Y, double Z) sum = footprint.Ring.Zip(next).Fold(
            (Cross: 0.0, X: 0.0, Y: 0.0, Z: 0.0),
            static (acc, edge) => {
                double cross = edge.Item1.X * edge.Item2.Y - edge.Item2.X * edge.Item1.Y;
                return (acc.Cross + cross, acc.X + (edge.Item1.X + edge.Item2.X) * cross,
                    acc.Y + (edge.Item1.Y + edge.Item2.Y) * cross, acc.Z + edge.Item1.Z);
            });
        return Math.Abs(sum.Cross) > 1e-12
            ? Fin.Succ(new Vector3(sum.X / (3.0 * sum.Cross), sum.Y / (3.0 * sum.Cross), sum.Z / footprint.Ring.Count + SampleLiftM))
            : Fin.Fail<Vector3>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<daylight-target-degenerate-footprint>"));
    }

    // Root AABB occupies the wire's first bounds slot (min xyz, max xyz) on both decoded kinds; its diagonal is the
    // scene diameter every occlusion ray reaches across.
    static float Diameter(AccelerationStructure index) => Diagonal(index.Bounds.Span);

    static float Diagonal(ReadOnlySpan<float> bounds) =>
        bounds.Length >= 6
            ? MathF.Sqrt(
                (bounds[3] - bounds[0]) * (bounds[3] - bounds[0])
                + (bounds[4] - bounds[1]) * (bounds[4] - bounds[1])
                + (bounds[5] - bounds[2]) * (bounds[5] - bounds[2]))
            : 0f;
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
