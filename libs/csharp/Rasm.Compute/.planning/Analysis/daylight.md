# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner: the `Discipline.Daylight` arm of the `Analysis/assessment` spine, realizing a seam row that shipped runnerless. C# owns geometry-derived daylight facts — direct sun-hours, shadow fraction, sky-view factor, single-hour Perez diffuse — off an owned NREL-SPA-grade closed-form solar-position kernel; climate-based CBDM and glare (Radiance-class annual simulation) stay the Python companion's, decoded over the graduation-evidence seam by content key, the same C#-facts/Python-annual split the energy lane holds. No admissible package owns the solar computation (`CoordinateSharp` fails the AGPL/commercial license gate), so the kernel is hand-owned public-domain law. A weather-less request degrades to geometry-only shadow facts stated inline on the result, never a silently-defaulted sky.

Site and hourly direct-normal/diffuse-horizontal irradiance arrive through the energy lane's own `WeatherRef` surface read by the admitted OpenStudio `EpwFile` reader (`latitude()`/`longitude()`/`timeZone()`/`elevation()` headers, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` hourly reads); shadow and obstruction rays reuse the clash BVH through `ClashScale.Occluded` over the decoded kernel `ToAcceleration` wire — one ray engine, never a daylight-local walk. `SolarPosition` exports across the package boundary as the federation's one solar kernel: `Rasm.AppUi` viewport sun-light composes its `At`/`SunPath` from site plus instant, foreclosing an AppUi-side duplicate. Zero new central pins — `EpwFile` and the clash BVH are admitted substrate.

## [01]-[INDEX]

- [01]-[SOLAR_POSITION]: the owned NREL-SPA-grade closed-form solar kernel — package-boundary-consumable azimuth/altitude from site + instant.
- [02]-[SKY_AND_SHADOW]: the Perez all-weather sky rows over the EPW ingress, the clash-BVH shadow-ray fold, and the daylight runner.

## [02]-[SOLAR_POSITION]

- Owner: `SolarSite` the site (its `EpwFile`-header or caller-supplied projection); `SunPosition` the topocentric result (zenith and incidence derived, never stored); `SolarPosition` the static closed-form kernel, the federation's one solar computation exported public across the package boundary.
- Entry: `At(site, instant)` folds the closed-form astronomical chain to topocentric azimuth (from-north) and altitude; `SunPath(site, midnight, step, samples)` folds `At` over a day at the policy step for the sun-hours sweep. Both are total — no state, effects, or fault rail, astronomy being total — and the Julian day derives from the NodaTime `Instant`, never a BCL `DateTime`.
- Packages: NodaTime (`Instant`/`Duration`), BCL inbox (`Math`); the algorithm is the public-domain NREL SPA closed form, hand-owned, no package admission at the license gate.
- Growth: an accuracy refinement (full SPA periodic-term tables over the truncated form) is a body change on the same two entries; a new consumer composes `At`/`SunPath`, never a duplicate kernel; zero new surface.
- Boundary: `SolarPosition` is public package-boundary surface — `Rasm.AppUi/Render` composes it for viewport sun-light, this page's sky/shadow folds its first consumer; it carries no `Fin` rail (closed-form astronomy is total, effect-free), the only nontotal seam being `[03]`'s EPW read; a `DateTime`-taking overload is the deleted form.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct SolarSite(double LatitudeDeg, double LongitudeDeg, double TimezoneHours, double ElevationM);

// Azimuth from north clockwise (the survey convention the sun-path arc and shadow rays share); zenith and below-horizon derive, never stored.
public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public double ZenithDeg => 90.0 - AltitudeDeg;
    public bool AboveHorizon => AltitudeDeg > 0.0;

    // Unit sun direction the shadow rays cast toward — from the target, so occlusion tests point at the sun.
    public Vector3 Direction {
        get {
            double alt = AltitudeDeg * Math.PI / 180.0, az = AzimuthDeg * Math.PI / 180.0;
            return new Vector3((float)(Math.Cos(alt) * Math.Sin(az)), (float)(Math.Cos(alt) * Math.Cos(az)), (float)Math.Sin(alt));
        }
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// NREL-SPA-grade closed form (public-domain law); the federation's one solar kernel, exported to Rasm.AppUi viewport sun-light.
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

- Owner: `PerezBand` `[SmartEnum<string>]` the eight all-weather clearness bands, each carrying the five published `(a, b, c, d, e)` brightening coefficients as row data over the clearness index ε (the published table, never a hardcoded interpolation); `SkyState` the per-hour sky carrier (DNI + DHI, derived ε, resolved `PerezBand`); `WeatherIngress` the `EpwFile` boundary off the `WeatherRef` surface; `DaylightAnalysis` the runner fold.
- Cases: with weather — per-target `direct-sun-hours` (the `SunPath` sweep × the clash-BVH occlusion ray per above-horizon sample), `shadow-fraction`, `sky-view-factor` (the hemisphere ray fan), `perez-diffuse-irradiance` (the circumsolar + horizon-band + isotropic-dome three-term sum over the resolved band); weather-less — the degrade: the same geometric facts at the design days off the solar kernel alone, the `sky-state` fact stating `"geometry-only"` inline, never a silently-defaulted sky.
- Entry: `Run(graph, request, geometry, clocks)` resolves the target points and obstruction scene through the `GeometrySource` port (an unresolvable target rails `AnalysisFailed(Admission, Input)`), reads optional weather through `WeatherIngress.Read` (an absent/malformed EPW under a weather-requiring policy rails, an absent EPW otherwise selects the degrade), and mints the fact stream; the governing ratio is the worst target's required/achieved sun-hours (EN 17037 minimum-sunlight, the route row's citation).
- Receipt: rides the one `ComputeReceipt.Assessment` case, no daylight-local receipt; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own pin), Rasm (project — the decoded `ToAcceleration` wire), Rasm.Element, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap on the same `SkyState` carrier; a new daylight fact (a window vertical-sky-component) is one fold over the same rays; annual CBDM/glare stays the Python companion's, an in-process Radiance-class loop the rejected form; zero new surface.
- Boundary: shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — one ray engine on the one acceleration owner, never a daylight-local traversal; sky ingress is the energy lane's own `WeatherRef` surface through the admitted `EpwFile` reader, never a second weather decode path nor a weather column on the daylight policy; the Perez coefficients ride `PerezBand` as the published table; `[02]`'s solar kernel is composed, never re-derived.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The eight published Perez clearness bands with their five brightening coefficients as row data (overcast
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

// --- [BOUNDARIES] --------------------------------------------------------------------------
// EPW ingress off the WeatherRef surface: header site + hourly sky states under the SWIG OptionalDouble
// is_initialized()-then-get() discipline — an absent hourly value contributes no SkyState hour, never a fabricated zero.
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
            // Degrade stated inline on the result — never a silently-defaulted sky.
            None: () => FinSucc(Seq(AssessmentFact.Text("sky-state", "geometry-only"))))
        select AssessmentResult.Of(
            request.Route,
            perTarget.Bind(static rows => rows) + skyFacts,
            govern,
            new Provenance("DaylightAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now));

    // Per-target fold: the SunPath sweep over the design days, ClashScale.Occluded per above-horizon sample
    // (one ray engine — the clash BVH over the decoded kernel wire), the sky-view hemisphere fan, and the Perez diffuse where sky exists.
    static Fin<(NodeId Target, double SunHours, double ShadowFraction, double SkyViewFactor, double PerezDiffuseWm2)> Target(
        DaylightScene scene, NodeId target, Option<(SolarSite Site, Seq<SkyState> Hours)> weather, AssessmentRequest.Daylight request, ClockPolicy clocks) { /* the
        design-day sweep composing SolarPosition.SunPath + ClashScale.Occluded + the Perez band fold */ }

    static PerezBand Dominant(Seq<SkyState> hours) =>
        hours.GroupBy(static h => h.Band).OrderByDescending(static g => g.Count()).Head.Map(static g => g.Key).IfNone(PerezBand.Overcast);
}

// Resolved scene: target sample points (GeometrySource-resolved) and the decoded obstruction BVH + triangle wire the occlusion rays walk.
public sealed record DaylightScene(Seq<NodeId> Targets, Map<NodeId, Vector3> SamplePoints, AccelerationStructure Obstructions, ReadOnlyMemory<float> Triangles, float SceneDiameter) {
    public static Fin<DaylightScene> Of(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry) { /* the one
        GeometrySource resolution — an unresolvable target is the typed (Admission, Input) failure */ }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
