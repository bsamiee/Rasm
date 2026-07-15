# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner: the `Discipline.Daylight` arm of the `Analysis/assessment` spine — the seam row EXISTED with no runner; this page is the runner. C# owns GEOMETRY-DERIVED daylight facts: an OWNED NREL-SPA-grade solar-position kernel (the public-domain closed-form astronomical algorithm — Julian ephemeris day, geocentric solar longitude/declination, equation of time, local hour angle folding to topocentric azimuth/altitude; zero solar computation existed corpus-wide, and no admissible package owns it — `CoordinateSharp` fails the AGPL/commercial license gate), Perez all-weather sky rows whose ingress is NAMED — site latitude/longitude/timezone and the hourly direct-normal/diffuse-horizontal irradiance arrive through the energy lane's OWN weather surface (the `WeatherRef`-shaped request input) read through the admitted OpenStudio `EpwFile` reader (`latitude()`/`longitude()`/`timeZone()`/`elevation()` header accessors and the `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` hourly reads — decompile-verified, the once-gated member set discharged), the solar kernel computing position while the EPW supplies sky state — a weather-less request DEGRADES TO GEOMETRY-ONLY SHADOW FACTS, stated inline on the result, never a silently-defaulted sky — and shadow/obstruction rays REUSING the `Solver/clash#CLASH_AND_TWIN` decoded BVH through its `ClashScale.Occluded` Möller–Trumbore ray entry over the kernel `ToAcceleration` wire (one ray engine, never a daylight-local BVH walk). Climate-based CBDM and glare (Radiance-class annual simulation) stay the Python companion's, decoded over the existing graduation-evidence seam — C# owns geometry-derived facts, Python owns the annual simulation, the same law the energy lane follows. The solar-position kernel is PACKAGE-BOUNDARY-CONSUMABLE public surface: the `Rasm.AppUi` viewport sun-light is its second named consumer (composing azimuth/altitude from site + instant through the declared seam), because a Compute-internal-only kernel would force the AppUi-side duplicate AppUi's own one-kernel clause forbids. Zero new central pins.

## [01]-[INDEX]

- [02]-[SOLAR_POSITION]: the owned NREL-SPA-grade closed-form solar kernel — package-boundary-consumable azimuth/altitude from site + instant.
- [03]-[SKY_AND_SHADOW]: the Perez all-weather sky rows over the EPW ingress, the clash-BVH shadow-ray fold, and the daylight runner.

## [02]-[SOLAR_POSITION]

- Owner: `SolarSite` the site record (latitude, longitude, timezone offset, elevation — the `EpwFile` header projection or a caller-supplied site); `SunPosition` the topocentric result (azimuth from north clockwise, altitude above horizon, the zenith and incidence conveniences derived); `SolarPosition` the static closed-form kernel — the ONE solar computation in the federation, its export public across the package boundary.
- Entry: `public static SunPosition At(SolarSite site, Instant instant)` — the closed-form chain: Julian day from the NodaTime `Instant` (never a BCL `DateTime` between rail and computation), mean solar longitude and anomaly, ecliptic longitude with the nutation-scale terms, geocentric declination and right ascension, the equation of time, the local hour angle off longitude + timezone, then topocentric altitude `asin(sinφ·sinδ + cosφ·cosδ·cosH)` and azimuth `atan2(sinH, cosH·sinφ − tanδ·cosφ)` folded to the from-north convention; `SunPath(site, date, step)` folds `At` over a day at the policy step for the sun-hours sweep; total functions of their inputs — no state, no effects, no fault rail (astronomy is total).
- Packages: NodaTime (`Instant`/`Duration` — the one time carrier), BCL inbox (`Math`); the algorithm is the public-domain NREL SPA closed form, hand-owned — no package admission exists for it at the license gate.
- Growth: a refinement in ordinal accuracy (the full SPA periodic-term tables over the truncated closed form) is a body refinement on the SAME two entries; a new consumer (the AppUi viewport sun, a shading-design sweep) composes `At`/`SunPath`, never a duplicate kernel; zero new surface.
- Boundary: the kernel is PUBLIC package-boundary surface — `Rasm.AppUi/Render` composes it for the viewport sun-light (its brief pins this export; an AppUi-side duplicate is the form its own one-kernel clause forbids) and this page's sky/shadow folds are its first consumer; the computation is pure closed-form astronomy (total, effect-free) so it carries no `Fin` rail — the ONLY nontotal seam is the EPW site read, which belongs to `[03]`; NodaTime `Instant` is the one time input and a `DateTime`-taking overload is the deleted form.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct SolarSite(double LatitudeDeg, double LongitudeDeg, double TimezoneHours, double ElevationM);

// Azimuth from NORTH clockwise (the survey convention the viewport sun and the shadow rays share),
// altitude above horizon; zenith and the below-horizon read derive — never stored duplicates.
public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public double ZenithDeg => 90.0 - AltitudeDeg;
    public bool AboveHorizon => AltitudeDeg > 0.0;

    // The unit sun DIRECTION the shadow rays cast toward (from the target, so occlusion tests point AT the sun).
    public Vector3 Direction {
        get {
            double alt = AltitudeDeg * Math.PI / 180.0, az = AzimuthDeg * Math.PI / 180.0;
            return new Vector3((float)(Math.Cos(alt) * Math.Sin(az)), (float)(Math.Cos(alt) * Math.Cos(az)), (float)Math.Sin(alt));
        }
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The ONE solar-position kernel in the federation (NREL-SPA-grade closed form, public-domain law):
// package-boundary-consumable — Rasm.AppUi viewport sun-light is the second named consumer.
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
        return new SunPosition(Wrap360(azimuth * 180.0 / Math.PI + 180.0), altitude * 180.0 / Math.PI);
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

- Owner: `PerezBand` `[SmartEnum<string>]` the eight Perez all-weather clearness bands — each row carrying the five published Perez coefficients `(a, b, c, d, e)` for the circumsolar/horizon brightening model as ROW DATA over the clearness index ε (the 1.000–1.065 overcast row through the >6.2 clear row), the published coefficient table as policy rows, never a hardcoded interpolation; `SkyState` the per-hour sky carrier (direct-normal + diffuse-horizontal irradiance, the derived clearness ε and brightness Δ, the resolved `PerezBand`); `WeatherIngress` the `EpwFile` boundary — header site + hourly `SkyState` sequence off the energy lane's own `WeatherRef` surface; `DaylightAnalysis` the runner fold.
- Cases: WITH WEATHER — per-target `direct-sun-hours` (the `SunPath` sweep × the clash-BVH occlusion ray per above-horizon sample), `shadow-fraction`, `sky-view-factor` (the hemisphere ray fan against the BVH), and `perez-diffuse-irradiance` (the band-weighted diffuse on the target plane — the Perez circumsolar + horizon-band + isotropic-dome three-term sum over the resolved band's coefficients); WEATHER-LESS — the DEGRADE case: the same geometric facts (sun-hours at the equinox/solstice design days off the solar kernel alone, shadow fraction, sky-view factor) with the `sky-state` fact stating `"geometry-only"` INLINE on the result — the degrade is a stated fact, never a silently-defaulted sky.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, ClockPolicy clocks)` — resolves the target sample points and the obstruction scene through the `GeometrySource` port (the decoded kernel `ToAcceleration` BVH + triangle wire — an unresolvable target is `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`), reads the optional weather through `WeatherIngress.Read` (an absent/malformed EPW under a weather-REQUIRING policy is `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`; an absent EPW otherwise selects the degrade case), sweeps `SolarPosition.SunPath` over the request's design days casting `ClashScale.Occluded(structure, triangles, samplePoint, sun.Direction, sceneDiameter)` per above-horizon sample, folds the Perez diffuse where sky state exists, and mints the uniform fact stream with the governing ratio the worst target's achieved/required sun-hours (EN 17037's minimum-sunlight criterion the route row cites).
- Receipt: the run rides the one `ComputeReceipt.Assessment` case — no daylight-local receipt; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` header/data reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` reads under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own admitted package, no new pin), Rasm (project — the decoded `ToAcceleration` wire), Rasm.Element (project), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap or one sibling row set on the SAME `SkyState` carrier; a new daylight fact (a window-level vertical sky component) is one fold over the same rays; annual climate-based CBDM/glare stays the PYTHON companion decoded over the graduation-evidence seam — an in-process Radiance-class annual loop is the rejected form; zero new surface.
- Boundary: the shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — ONE ray engine on the one acceleration owner, never a daylight-local traversal; the sky ingress is the energy lane's OWN `WeatherRef` surface through the admitted `EpwFile` reader — never a second weather decode path and never a weather column smuggled onto the daylight policy; the weather-less degrade is a STATED fact on the result (`sky-state = geometry-only`), never a defaulted sky silently reported as Perez; the Perez coefficients are the published table as `PerezBand` ROW DATA; the solar kernel is `[02]`'s — the sky/shadow folds compose it and re-derive nothing; CBDM/glare is Python's by the graduation law — this page's facts are geometry-derived (sun-hours, shadows, view factors, single-hour Perez diffuse), the honest C# half.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The eight published Perez all-weather clearness bands with their five brightening coefficients as ROW
// DATA (overcast 1.000–1.065 through clear >6.2): the band resolves from the derived clearness index ε and
// the coefficients drive the circumsolar/horizon terms — the published table, never a hardcoded fit.
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
// One hour of sky: the EPW direct-normal/diffuse-horizontal pair, the derived Perez clearness index ε
// (a function of DHI, DNI, and solar zenith), and the resolved band.
public readonly record struct SkyState(Instant At, double DirectNormalWm2, double DiffuseHorizontalWm2, PerezBand Band);

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The EPW ingress off the energy lane's own WeatherRef surface: the header site (latitude/longitude/
// timezone/elevation) and the hourly sky states under the SWIG OptionalDouble is_initialized()-then-get()
// discipline — an absent hourly value contributes NO SkyState hour, never a fabricated zero irradiance.
public static class WeatherIngress {
    public static Fin<(SolarSite Site, Seq<SkyState> Hours)> Read(WeatherRef weather, SolarSite? overrideSite = null) { /* EpwFile(toPath(weather.EpwPath))
        header + data() fold; a missing/malformed file is the typed (Admission, Input) failure at the caller's policy */ }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DaylightAnalysis {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, ClockPolicy clocks) =>
        from scene in Scene.Of(graph, request, geometry)
        let weather = request.Weather.Bind(w => WeatherIngress.Read(w).ToOption())
        from findings in scene.Targets.Traverse(target => Target(scene, target, weather, request, clocks))
        let govern = findings.Map(f => request.RequiredSunHours > 0.0 ? request.RequiredSunHours / Math.Max(f.SunHours, 1e-9) : double.NaN).Max() | double.NaN
        from perTarget in findings.TraverseM(f => AssessmentFact.Rows(
            AssessmentFact.Measure($"{f.Target.Value}/direct-sun-hours", Dimension.DurationDim, f.SunHours * 3600.0),
            AssessmentFact.Ratio($"{f.Target.Value}/shadow-fraction", f.ShadowFraction),
            AssessmentFact.Ratio($"{f.Target.Value}/sky-view-factor", f.SkyViewFactor))).As()
        from skyFacts in weather.Match(
            Some: w => findings.TraverseM(f => AssessmentFact.Measure($"{f.Target.Value}/perez-diffuse-irradiance", Dimension.IrradianceDim, f.PerezDiffuseWm2)).As()
                .Map(perez => Seq(AssessmentFact.Text("sky-state", $"perez:{Dominant(w.Hours).Key}")) + perez),
            // The DEGRADE is a stated fact inline on the result — never a silently-defaulted sky.
            None: () => FinSucc(Seq(AssessmentFact.Text("sky-state", "geometry-only"))))
        select AssessmentResult.Of(
            request.Route,
            perTarget.Bind(static rows => rows) + skyFacts,
            govern,
            new Provenance("DaylightAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now));

    // Per-target fold: the SunPath sweep over the request design days, ClashScale.Occluded per
    // above-horizon sample (ONE ray engine — the clash BVH over the decoded kernel wire), the hemisphere
    // fan for the sky-view factor, and the Perez three-term diffuse where sky state exists.
    static Fin<(NodeId Target, double SunHours, double ShadowFraction, double SkyViewFactor, double PerezDiffuseWm2)> Target(
        DaylightScene scene, NodeId target, Option<(SolarSite Site, Seq<SkyState> Hours)> weather, AssessmentRequest.Daylight request, ClockPolicy clocks) { /* the
        design-day sweep composing SolarPosition.SunPath + ClashScale.Occluded + the Perez band fold */ }

    static PerezBand Dominant(Seq<SkyState> hours) =>
        hours.GroupBy(static h => h.Band).OrderByDescending(static g => g.Count()).Head.Map(static g => g.Key).IfNone(PerezBand.Overcast);
}

// The resolved scene: target sample points (GeometrySource-resolved by content key) and the decoded
// obstruction BVH + triangle wire the occlusion rays walk.
public sealed record DaylightScene(Seq<NodeId> Targets, Map<NodeId, Vector3> SamplePoints, AccelerationStructure Obstructions, ReadOnlyMemory<float> Triangles, float SceneDiameter) {
    public static Fin<DaylightScene> Of(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry) { /* the one
        GeometrySource resolution — an unresolvable target is the typed (Admission, Input) failure */ }
}
```

## [04]-[RESEARCH]

- [CBDM_COMPANION]: annual climate-based daylight modelling (spatial daylight autonomy, annual sunlight exposure) and glare (DGP) are the Python companion's Radiance-class simulation, decoded over the existing graduation-evidence seam by content key — the C#-facts/Python-annual split is the SAME law the energy lane holds (C# owns the OSM build + SQL extraction, Python owns nothing energy needs; here C# owns geometry-derived facts, Python the annual integration). An in-process Radiance loop is the rejected form.
- [APPUI_SUN_EXPORT]: the `SolarPosition` kernel is the declared package-boundary export the `Rasm.AppUi` viewport sun-light composes (azimuth/altitude from site + instant) — the AppUi brief pins this consumer; the seam is the public `At`/`SunPath` surface, and an AppUi-side solar duplicate is the form its own one-kernel clause forbids.
