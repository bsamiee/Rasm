# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner owns the `Discipline.Daylight` assessment arm. C# derives direct sun-hours, shadow fraction, sky-view factor, and Perez diffuse irradiance from one package solar-position kernel; climate-based CBDM and glare stay with the Python companion. Weather-less requests require an explicit site, and present weather failures remain typed failures rather than silently degrading.

Site and hourly direct-normal/diffuse-horizontal irradiance arrive through the energy lane's own `WeatherRef` surface read by the admitted OpenStudio `EpwFile` reader (`latitude()`/`longitude()`/`timeZone()`/`elevation()` headers, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` hourly reads); shadow and obstruction rays reuse the clash BVH through `ClashScale.Occluded` over the `AccelerationStructure` the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire decodes (`Solver/clash` owns the decode; the retired `ToAcceleration` member is never named) — one ray engine, never a daylight-local walk; the app stages that decoded scene on the request as `ObstructionScene`, its content key riding the assessment content-key fold so a re-shaded site re-keys. `SolarPosition` exports across the package boundary as the federation's one solar kernel: `Rasm.AppUi` viewport sun-light composes its `At`/`SunPath` from site plus instant, foreclosing an AppUi-side duplicate. Zero new central pins — `EpwFile` and the clash BVH are admitted substrate.

## [01]-[INDEX]

- [01]-[SOLAR_POSITION]: the owned apparent-solar closed form — package-boundary azimuth/altitude from admitted site + instant.
- [02]-[SKY_AND_SHADOW]: the Perez all-weather sky rows over the EPW ingress, the clash-BVH shadow-ray fold, and the daylight runner.

## [02]-[SOLAR_POSITION]

- Owner: validated `SolarSite`; `SunPosition` the apparent result; `SolarPosition` the package solar computation exported across the package boundary.
- Entry: `At(site, instant)` derives apparent azimuth/altitude, including elevation-adjusted atmospheric refraction; `SunPath` samples that same total function from a NodaTime `Instant`.
- Packages: NodaTime (`Instant`/`Duration`), BCL inbox (`Math`); the hand-owned NOAA/Meeus-style apparent-solar approximation carries no package admission.
- Growth: an accuracy refinement (full SPA periodic-term tables over the truncated form) is a body change on the same two entries; a new consumer composes `At`/`SunPath`, never a duplicate kernel; zero new surface.
- Boundary: `SolarPosition` is the declared `[APPUI_SUN_EXPORT]` package-boundary surface — `Rasm.AppUi/Render/pathtrace` composes `At`/`SunPath` for viewport sun-light and sun studies under that row name, this page's sky/shadow folds its first consumer; it carries no `Fin` rail (closed-form astronomy is total, effect-free), the only nontotal seam being `[03]`'s EPW read; a `DateTime`-taking overload is the deleted form.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SolarSite {
    public double LatitudeDeg { get; }
    public double LongitudeDeg { get; }
    public double TimezoneHours { get; }
    public double ElevationM { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double latitudeDeg, ref double longitudeDeg,
        ref double timezoneHours, ref double elevationM) =>
        validationError = double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0
            && double.IsFinite(longitudeDeg) && longitudeDeg is >= -180.0 and <= 180.0
            && double.IsFinite(timezoneHours) && timezoneHours is >= -14.0 and <= 14.0
            && double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0
                ? null
                : new ValidationError(message: "<solar-site-invalid>");
}

// Azimuth from north clockwise (the survey convention the sun-path arc and shadow rays share); zenith and below-horizon derive, never stored.
public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public double ZenithDeg => 90.0 - AltitudeDeg;
    public bool AboveHorizon => AltitudeDeg > 0.0;

    // Unit sun direction the shadow rays cast toward — from the target, so occlusion tests point at the sun.
    public Vector3 Direction {
        get {
            double alt = AltitudeDeg * Math.PI / 180.0, az = AzimuthDeg * Math.PI / 180.0;
            return new Vector3(Math.Cos(alt) * Math.Sin(az), Math.Cos(alt) * Math.Cos(az), Math.Sin(alt));
        }
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// Apparent-solar closed form; the federation's one solar kernel, exported to `Rasm.AppUi` viewport sun-light.
public static class SolarPosition {
    public static SunPosition At(SolarSite site, Instant instant) {
        double jd = 2440587.5 + instant.ToUnixTimeTicks() / (double)NodaConstants.TicksPerDay;
        double t = (jd - 2451545.0) / 36525.0;
        double meanLongitude = Wrap360(280.46646 + t * (36000.76983 + t * 0.0003032));
        double meanAnomaly = (357.52911 + t * (35999.05029 - 0.0001537 * t)) * Math.PI / 180.0;
        double center = Math.Sin(meanAnomaly) * (1.914602 - t * (0.004817 + 0.000014 * t))
            + Math.Sin(2.0 * meanAnomaly) * (0.019993 - 0.000101 * t)
            + Math.Sin(3.0 * meanAnomaly) * 0.000289;
        double eclipticLongitude = (meanLongitude + center - 0.00569 - 0.00478 * Math.Sin((125.04 - 1934.136 * t) * Math.PI / 180.0)) * Math.PI / 180.0;
        double obliquity = (23.0 + (26.0 + (21.448 - t * (46.815 + t * (0.00059 - t * 0.001813))) / 60.0) / 60.0) * Math.PI / 180.0;
        double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));
        double y = Math.Tan(obliquity / 2.0) * Math.Tan(obliquity / 2.0);
        double meanLonRad = meanLongitude * Math.PI / 180.0;
        double equationOfTime = 4.0 * (180.0 / Math.PI) * (
            y * Math.Sin(2.0 * meanLonRad) - 2.0 * 0.016708634 * Math.Sin(meanAnomaly)
            + 4.0 * 0.016708634 * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLonRad)
            - 0.5 * y * y * Math.Sin(4.0 * meanLonRad) - 1.25 * 0.016708634 * 0.016708634 * Math.Sin(2.0 * meanAnomaly));
        double fractionalDay = jd - Math.Floor(jd) - 0.5 + site.TimezoneHours / 24.0;
        double trueSolarMinutes = Wrap(fractionalDay * 1440.0 + equationOfTime + 4.0 * site.LongitudeDeg - 60.0 * site.TimezoneHours, 1440.0);
        double hourAngle = ((trueSolarMinutes / 4.0) - 180.0) * Math.PI / 180.0;
        double phi = site.LatitudeDeg * Math.PI / 180.0;
        double altitude = Math.Asin(Math.Sin(phi) * Math.Sin(declination) + Math.Cos(phi) * Math.Cos(declination) * Math.Cos(hourAngle));
        double azimuth = Math.Atan2(Math.Sin(hourAngle), Math.Cos(hourAngle) * Math.Sin(phi) - Math.Tan(declination) * Math.Cos(phi));
        double altitudeDeg = altitude * 180.0 / Math.PI;
        double pressureRatio = Math.Pow(1.0 - 2.25577e-5 * Math.Max(site.ElevationM, -500.0), 5.25588);
        double refractionDeg = altitudeDeg is > -1.0 and < 90.0
            ? pressureRatio * 1.02 / Math.Tan((altitudeDeg + 10.3 / (altitudeDeg + 5.11)) * Math.PI / 180.0) / 60.0
            : 0.0;
        return new SunPosition(Wrap360(azimuth * 180.0 / Math.PI + 180.0), altitudeDeg + refractionDeg);
    }

    // One day's positions at the policy step — the sun-hours sweep and the viewport sun-path arc share it.
    public static Seq<(Instant Instant, SunPosition Sun)> SunPath(SolarSite site, Instant midnight, Duration step, int samples) =>
        toSeq(Enumerable.Range(0, samples)).Map(i => {
            Instant at = midnight + step * i;
            return (at, At(site, at));
        });

    static double Wrap360(double degrees) => Wrap(degrees, 360.0);
    static double Wrap(double value, double period) => value - period * Math.Floor(value / period);
}
```

## [03]-[SKY_AND_SHADOW]

- Owner: `PerezBand` `[SmartEnum<string>]` the eight all-weather clearness bands, each carrying the five published `(a, b, c, d, e)` brightening coefficients as row data over the clearness index ε (the published table, never a hardcoded interpolation); `SkyState` the per-hour sky carrier (DNI + DHI, derived ε, resolved `PerezBand`); `WeatherIngress` the `EpwFile` boundary off the `WeatherRef` surface; `DaylightAnalysis` the runner fold.
- Cases: with weather — per-target `direct-sun-hours` (the `SunPath` sweep × the clash-BVH occlusion ray per above-horizon sample), `shadow-fraction`, `sky-view-factor` (the hemisphere ray fan), `perez-diffuse-irradiance` (the circumsolar + horizon-band + isotropic-dome three-term sum over the resolved band); weather-less — the degrade: the same geometric facts at the design days off the solar kernel over the request's explicit `Site`, the `sky-state` fact stating `"geometry-only"` inline, never a silently-defaulted sky; absent both weather and an explicit site the run rails `AssessmentInputMissing`.
- Entry: `Run(graph, request, geometry, clocks)` resolves the target points and obstruction scene through the `GeometrySource` port (an unresolvable target rails `AnalysisFailed(Admission, Input)`), reads optional weather through `WeatherIngress.Read` (a present-but-malformed EPW rails typed; an absent EPW selects the geometry-only degrade over the request's explicit `Site`), and mints the fact stream; the governing ratio is the worst target's required/achieved sun-hours (EN 17037 minimum-sunlight, the route row's citation).
- Receipt: rides the one `ComputeReceipt.Assessment` case, no daylight-local receipt; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own pin), Rasm (project — the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire the staged scene decodes from), Rasm.Element, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap on the same `SkyState` carrier; a new daylight fact (a window vertical-sky-component) is one fold over the same rays; annual CBDM/glare stays the Python companion's, an in-process Radiance-class loop the rejected form; zero new surface.
- Boundary: shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — one ray engine on the one acceleration owner, never a daylight-local traversal; sky ingress is the energy lane's own `WeatherRef` surface through the admitted `EpwFile` reader, never a second weather decode path nor a weather column on the daylight policy; the Perez coefficients ride `PerezBand` as the published table; `[02]`'s solar kernel is composed, never re-derived.

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

    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, ClockPolicy clocks) =>
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
            new Provenance("DaylightAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now));

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
            .TraverseM(row => ClashScale.Occluded(scene.Obstructions, scene.Triangles, origin, row.Sun.Direction, scene.SceneDiameter)
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
                return ClashScale.Occluded(scene.Obstructions, scene.Triangles, origin, ray, scene.SceneDiameter).Map(occluded => (Weight: weight, Occluded: occluded));
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
// decoded obstruction BVH + triangle wire the occlusion rays walk; SceneDiameter reads the root AABB off the wire so
// every ray's reach covers the federated scene.
public sealed record DaylightScene(Seq<NodeId> Targets, Map<NodeId, Vector3> SamplePoints, AccelerationStructure Obstructions, ReadOnlyMemory<float> Triangles, float SceneDiameter) {
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
                    ? Fin.Succ(new DaylightScene(
                        request.Targets,
                        points.Fold(Map<NodeId, Vector3>(), static (acc, point) => acc.Add(point.Id, point.Point)),
                        request.Scene.Index,
                        request.Scene.Triangles,
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
