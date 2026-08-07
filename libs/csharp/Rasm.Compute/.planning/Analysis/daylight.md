# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner owns the `Discipline.Daylight` assessment arm: it answers worst-design-day direct sun hours, mean shadow fraction, sky-view factor, and Perez all-weather diffuse irradiance at each target's reference plane, every one derived from the kernel `Rasm` solar almanac against the clash BVH. Weather-less requests require an explicit site, and present weather failures remain typed failures rather than silently degrading.

Site and hourly direct-normal/diffuse-horizontal irradiance arrive through the energy lane's own `WeatherRef` surface read by the admitted OpenStudio `EpwFile` reader (`latitude()`/`longitude()`/`timeZone()`/`elevation()` headers, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` hourly reads); shadow and obstruction rays reuse the clash BVH through `ClashScale.Occluded` over the `AccelerationStructure` the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire decodes (`Solver/clash` owns the decode) — one ray engine, never a daylight-local walk; the app stages that decoded scene on the request as `ObstructionScene`, its content key riding the assessment content-key fold so a re-shaded site re-keys. Solar position composes the kernel `Rasm/Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition.At`/`SunPath` over the validated `SolarSite` — the same owner `Rasm.AppUi` viewport sun-light and the Materials environment adapter compose, so no package-local ephemeris exists. Zero new central pins — `EpwFile` and the clash BVH are admitted substrate.

## [01]-[INDEX]

- [02]-[SKY_AND_SHADOW]: `RunDaylight` folds the Perez all-weather sky rows over the EPW ingress against the clash-BVH shadow-ray cast at design-day and hourly cadence.

## [02]-[SKY_AND_SHADOW]

- Owner: `PerezBand` `[SmartEnum<string>]` the eight published all-weather clearness bands, each carrying the full six-coefficient `(F11, F12, F13, F21, F22, F23)` row and the two brightening terms it evaluates (the published table, never a hardcoded interpolation nor a truncated column set); `DaylightPolicy` the sampling-cadence value object; `SkyState` the per-hour sky carrier (sun position, DNI + DHI, derived sky brightness Δ, resolved `PerezBand`); `WeatherIngress` the `EpwFile` boundary off the `WeatherRef` surface; `DaylightAnalysis` the runner fold.
- Cases: with weather — per-target `worst-day-sun-hours`, `mean-shadow-fraction`, `sky-view-factor` (the hemisphere ray fan), `perez-diffuse-irradiance` (the hourly isotropic-dome + circumsolar sum over each hour's resolved band); weather-less — the degrade: the same geometric facts at the design days off the solar kernel over the request's explicit `Site`, the `sky-state` fact stating `"geometry-only"` inline, never a silently-defaulted sky; absent both weather and an explicit site the run rails `AssessmentInputMissing`.
- Law: the design-day sweep yields TWO declared reductions over one sample set, never one mixed aggregate — `worst-day-sun-hours` is the MINIMUM per-day lit-sample total across every requested design day (a day whose sun never clears the horizon reads zero rather than dropping out of the reduction), and `mean-shadow-fraction` is the occluded share of every above-horizon sample pooled across all of them; each is a separately named fact because a design brief accepts on the worst day and reports on the mean.
- Law: circumsolar admission is probed PER WEATHER HOUR at the policy cadence — each retained `SkyState` casts its own occlusion ray along its own sun direction, so a target the morning sun reaches and the afternoon sun does not reads two different circumsolar admissions; a design-day-mean visibility ratio smeared across the annual sum reports a fabricated sky at every hour it was not measured at.
- Entry: `Run(graph, request, geometry, clock)` resolves the target points and obstruction scene through the `GeometrySource` port (an unresolvable target rails `AnalysisFailed(Admission, Input)`), reads optional weather through `WeatherIngress.Read` (a present-but-malformed EPW rails typed; an absent EPW selects the geometry-only degrade over the request's explicit `Site`), and mints the fact stream; the governing ratio is the worst target's required/achieved sun-hours (EN 17037 minimum-sunlight, the route row's citation).
- Receipt: rides the one `ComputeReceipt.Assessment` case, no daylight-local receipt; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own pin), Rasm (project — the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire the staged scene decodes from, and the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition`/`SolarSite`/`SunPosition` solar almanac), Rasm.Element, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap on the same `SkyState` carrier; a tilted-plane fact (a window vertical-sky-component) composes the row's `Horizon` term beside the `Circumsolar` one already folded; a re-cadenced sweep or hemisphere fan is one `DaylightPolicy` column; zero new surface.
- Boundary: shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — one ray engine on the one acceleration owner, never a daylight-local traversal; sky ingress is the energy lane's own `WeatherRef` surface through the admitted `EpwFile` reader, never a second weather decode path nor a weather column on the sampling policy; the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac is composed, never re-derived. The EN 17037 reference plane is HORIZONTAL, so the plane-of-array form collapses exactly: the isotropic weight `(1 + cos S)/2` becomes the measured sky-view factor, the circumsolar ratio `a/b` is unity under an above-horizon sun, and the horizon band's `sin S` factor is zero — the fold composes `Circumsolar` alone and `Horizon` stays row surface the tilted case reads, never a term applied at a tilt that annihilates it. Every sampling count and step is a `DaylightPolicy` column that folds the assessment content key, never a runner constant a re-cadenced sweep silently re-uses the cached answer of.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The eight published Perez clearness bands (overcast 1.000–1.065 through pristine >6.200) carry the whole
// six-coefficient row: the band resolves from the derived ε, and F11..F13 / F21..F23 are the two independent
// linear forms in sky brightness Δ and solar zenith. Dropping F13/F23 collapses the zenith dependence the
// model's whole low-sun behaviour rides on, so the row transcribes all six or none.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PerezBand {
    public static readonly PerezBand Overcast = new("overcast", 1.000, 1.065, f11: -0.008, f12: 0.588, f13: -0.062, f21: -0.060, f22: 0.072, f23: -0.022);
    public static readonly PerezBand MostlyOvercast = new("mostly-overcast", 1.065, 1.230, f11: 0.130, f12: 0.683, f13: -0.151, f21: -0.019, f22: 0.066, f23: -0.029);
    public static readonly PerezBand PartlyOvercast = new("partly-overcast", 1.230, 1.500, f11: 0.330, f12: 0.487, f13: -0.221, f21: 0.055, f22: -0.064, f23: -0.026);
    public static readonly PerezBand Intermediate = new("intermediate", 1.500, 1.950, f11: 0.568, f12: 0.187, f13: -0.295, f21: 0.109, f22: -0.152, f23: -0.014);
    public static readonly PerezBand MostlyClear = new("mostly-clear", 1.950, 2.800, f11: 0.873, f12: -0.392, f13: -0.362, f21: 0.226, f22: -0.462, f23: 0.001);
    public static readonly PerezBand Clear = new("clear", 2.800, 4.500, f11: 1.132, f12: -1.237, f13: -0.412, f21: 0.288, f22: -0.823, f23: 0.056);
    public static readonly PerezBand VeryClear = new("very-clear", 4.500, 6.200, f11: 1.060, f12: -1.600, f13: -0.359, f21: 0.264, f22: -1.127, f23: 0.131);
    public static readonly PerezBand Pristine = new("pristine", 6.200, double.PositiveInfinity, f11: 0.678, f12: -0.327, f13: -0.250, f21: 0.156, f22: -1.377, f23: 0.251);

    public double EpsilonLow { get; }
    public double EpsilonHigh { get; }
    public double F11 { get; }
    public double F12 { get; }
    public double F13 { get; }
    public double F21 { get; }
    public double F22 { get; }
    public double F23 { get; }

    // Circumsolar brightening F1 clamps at zero because the fitted form goes negative for a bright overcast
    // sky, where the physical disc contributes nothing; horizon brightening F2 is signed and stays signed —
    // a negative F2 is the model DARKENING the horizon band and clamping it publishes a glow that is not there.
    public double Circumsolar(double brightness, double zenithRad) => Math.Max(0.0, F11 + F12 * brightness + F13 * zenithRad);
    public double Horizon(double brightness, double zenithRad) => F21 + F22 * brightness + F23 * zenithRad;

    public static PerezBand OfClearness(double epsilon) =>
        toSeq(Items).Find(band => epsilon >= band.EpsilonLow && epsilon < band.EpsilonHigh).IfNone(Overcast);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Sampling cadence for both ray families and the circumsolar gate. The sweep invariant is load-bearing:
// SunSamplesPerDay × SunStepHours may not exceed one civil day, or the design-day walk runs into the next
// date and double-counts its sun. OcclusionCadenceHours = 1 probes every retained EPW hour — the honest gate;
// a larger stride trades sky fidelity for rays and states that trade as a keyed value.
[ComplexValueObject]
public sealed partial class DaylightPolicy {
    public int SunSamplesPerDay { get; }
    public double SunStepHours { get; }
    public int HemisphereAzimuths { get; }
    public int HemisphereAltitudes { get; }
    public int OcclusionCadenceHours { get; }

    public static readonly DaylightPolicy Canonical = Create(96, 0.25, 72, 18, 1);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int sunSamplesPerDay,
        ref double sunStepHours, ref int hemisphereAzimuths, ref int hemisphereAltitudes, ref int occlusionCadenceHours) =>
        validationError = sunSamplesPerDay > 0 && hemisphereAzimuths > 0 && hemisphereAltitudes > 0 && occlusionCadenceHours > 0
            && double.IsFinite(sunStepHours) && sunStepHours > 0.0 && sunSamplesPerDay * sunStepHours <= 24.0
                ? null
                : new ValidationError(message: "<daylight-policy-invalid>");
}

// One hour of sky, derived ONCE at ingress because every column is site-and-instant data no target varies:
// the almanac sun position the circumsolar ray reuses, the EPW DNI/DHI pair, the Perez sky brightness Δ, and
// the band the clearness ε resolves. A per-target re-derivation runs the ephemeris once per target per hour
// for one answer.
public readonly record struct SkyState(Instant At, SunPosition Sun, double DirectNormalWm2, double DiffuseHorizontalWm2, double Brightness, PerezBand Band) {
    const double SolarConstantWm2 = 1367.0;

    public static SkyState Of(SolarSite site, Offset offset, Instant at, double directNormalWm2, double diffuseHorizontalWm2) {
        SunPosition sun = SolarPosition.At(site, at);
        double zenithRad = sun.ZenithDeg * Math.PI / 180.0;
        // Clearness ε carries the κZ³ term (κ = 1.041 for Z in radians) so a low sun is not read as an
        // overcast sky; a zero-diffuse hour has no defined ratio and reads the overcast floor.
        double kappa = 1.041 * zenithRad * zenithRad * zenithRad;
        double clearness = diffuseHorizontalWm2 > 0.0
            ? ((diffuseHorizontalWm2 + directNormalWm2) / diffuseHorizontalWm2 + kappa) / (1.0 + kappa)
            : 1.0;
        return new SkyState(at, sun, directNormalWm2, diffuseHorizontalWm2,
            diffuseHorizontalWm2 * AirMass(sun.ZenithDeg) / Extraterrestrial(at.WithOffset(offset).DayOfYear),
            PerezBand.OfClearness(clearness));
    }

    // Kasten–Young 1989 relative optical air mass; only above-horizon hours reach here, so the 96.07995°
    // pole of the power term is unreachable and the reciprocal never crosses zero.
    static double AirMass(double zenithDeg) =>
        1.0 / (Math.Cos(zenithDeg * Math.PI / 180.0) + 0.50572 * Math.Pow(96.07995 - zenithDeg, -1.6364));

    // Extraterrestrial normal incidence: the solar constant under the orbital eccentricity correction. The
    // ±3.3% annual swing is the whole normalizer of Δ, so a fixed 1367 reports a winter sky as brighter than it is.
    static double Extraterrestrial(int dayOfYear) => SolarConstantWm2 * (1.0 + 0.033 * Math.Cos(2.0 * Math.PI * dayOfYear / 365.0));
}

// App-staged obstruction scene carries the kernel spatial-wire snapshot content key the assessment content-key
// fold reads — a re-shaded site re-keys — the AccelerationStructure the Solver/clash decode produced from
// Spatial.Apply(SpatialOp.Wire), and the federated triangle wire the occlusion rays walk.
public sealed record ObstructionScene(UInt128 Key, AccelerationStructure Index, ReadOnlyMemory<float> Triangles);

// Per-target finding crosses weather and geometry-only paths in one shape; each sun column names its own
// reduction, so no consumer can read the worst day's hours as an annual mean or the pooled shadow share as a daily one.
public readonly record struct DaylightFinding(NodeId Target, double WorstDaySunHours, double MeanShadowFraction, double SkyViewFactor, double PerezDiffuseWm2);

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
            Offset offset = Offset.FromTicks((long)(site.TimezoneHours * NodaConstants.TicksPerHour));
            Instant yearStart = new LocalDate(2001, 1, 1).AtMidnight().WithOffset(offset).ToInstant();
            List<SkyState> hours = new((int)data.Count);
            for (int i = 0; i < data.Count; i++) {
                using OpenStudio.EpwDataPoint point = data[i];
                using OpenStudio.OptionalDouble dni = point.directNormalRadiation();
                using OpenStudio.OptionalDouble dhi = point.diffuseHorizontalRadiation();
                if (dni.is_initialized() && dhi.is_initialized() && double.IsFinite(dni.get()) && dni.get() >= 0.0 && double.IsFinite(dhi.get()) && dhi.get() >= 0.0) {
                    SkyState hour = SkyState.Of(site, offset, yearStart + Duration.FromHours(i), dni.get(), dhi.get());
                    if (hour.Sun.AboveHorizon) { hours.Add(hour); }
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
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DaylightAnalysis {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, IClock clock) =>
        from _ in !request.DesignDays.IsEmpty && double.IsFinite(request.RequiredSunHours) && request.RequiredSunHours >= 0.0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing("<daylight-request-invalid>"))
        from scene in DaylightScene.Of(graph, request, geometry)
        from weather in request.Weather.Match(
            Some: weather => WeatherIngress.Read(weather).Map(static value => Some(value)),
            None: static () => Fin.Succ(Option<(SolarSite Site, Seq<SkyState> Hours)>.None))
        // Site evidence is REQUIRED for any sun sweep: the EPW header supplies it under weather, the request's
        // explicit Site carries the geometry-only run, and absent both the run rails typed — never a fabricated site.
        from site in (weather.Map(static w => w.Site) | request.Site)
            .ToFin(new ComputeFault.AssessmentInputMissing("<daylight-site-unresolved:no-weather-and-no-explicit-site>"))
        from findings in scene.Targets.TraverseM(target => Target(scene, target, site, weather.Map(static w => w.Hours), request)).As()
        let govern = findings.Map(f => request.RequiredSunHours > 0.0
            ? f.WorstDaySunHours > 0.0 ? request.RequiredSunHours / f.WorstDaySunHours : double.MaxValue
            : double.NaN).Fold(double.NaN, static (worst, ratio) => double.IsNaN(worst) ? ratio : Math.Max(worst, ratio))
        from perTarget in findings.TraverseM(f => AssessmentFact.Rows(
            AssessmentFact.Measure($"{f.Target.Value}/worst-day-sun-hours", Dimension.DurationDim, f.WorstDaySunHours * 3600.0),
            AssessmentFact.Ratio($"{f.Target.Value}/mean-shadow-fraction", f.MeanShadowFraction),
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

    // Per-target fold over three independent ray families on the ONE clash BVH — the design-day sun sweep, the
    // cosine-weighted sky-view fan, and the hourly circumsolar gate; a failed occlusion probe rails the typed wire fault.
    static Fin<DaylightFinding> Target(DaylightScene scene, NodeId target, SolarSite site, Option<Seq<SkyState>> hours, AssessmentRequest.Daylight request) {
        Vector3 origin = scene.SamplePoints[target];
        return from sweep in DesignSweep(scene, origin, site, request)
               from skyView in SkyView(scene, origin, request.Policy)
               from diffuse in hours.Match(
                   Some: sky => HourlyDiffuse(scene, origin, sky, skyView, request.Policy),
                   None: static () => Fin.Succ(0.0))
               select new DaylightFinding(
                   target,
                   WorstDaySunHours(sweep, request.DesignDays, request.Policy.SunStepHours),
                   MeanShadowFraction(sweep),
                   skyView,
                   diffuse);
    }

    // Design-day sun sweep: the policy step walks each requested day from local midnight and every above-horizon
    // sample casts one occlusion ray along its own sun direction. The day tag rides each sample because the two
    // reductions below partition on it differently.
    static Fin<Seq<(LocalDate Day, bool Occluded)>> DesignSweep(DaylightScene scene, Vector3 origin, SolarSite site, AssessmentRequest.Daylight request) {
        Offset offset = Offset.FromTicks((long)(site.TimezoneHours * NodaConstants.TicksPerHour));
        return request.DesignDays
            .Bind(day => SolarPosition
                .SunPath(site, day.AtMidnight().WithOffset(offset).ToInstant(), Duration.FromHours(request.Policy.SunStepHours), request.Policy.SunSamplesPerDay)
                .Filter(static row => row.Sun.AboveHorizon)
                .Map(row => (Day: day, row.Sun.Direction)))
            .TraverseM(row => ClashScale.Occluded(scene.Scene, origin, row.Direction, scene.SceneDiameter)
                .Map(occluded => (row.Day, Occluded: occluded)))
            .As();
    }

    // The acceptance reduction: the MINIMUM lit-hour total over the requested design days. The fold walks the
    // requested days rather than the sample groups, so a day the sun never clears the horizon on contributes its
    // honest zero instead of vanishing from the extremum and lifting the worst case.
    static double WorstDaySunHours(Seq<(LocalDate Day, bool Occluded)> sweep, Seq<LocalDate> days, double stepHours) =>
        days.Map(day => sweep.Count(sample => sample.Day == day && !sample.Occluded) * stepHours)
            .Fold(double.PositiveInfinity, static (worst, hours) => Math.Min(worst, hours));

    // The reporting reduction: the occluded share of every above-horizon sample pooled across all design days.
    static double MeanShadowFraction(Seq<(LocalDate Day, bool Occluded)> sweep) =>
        sweep.IsEmpty ? 0.0 : sweep.Count(static sample => sample.Occluded) / (double)sweep.Count;

    // Cosine-weighted hemisphere rays span azimuth/altitude, each weighted `sin(alt)·cos(alt)` so the
    // zenith patch and the horizon band contribute their solid-angle-projected share; SVF = unoccluded weight fraction.
    static Fin<double> SkyView(DaylightScene scene, Vector3 origin, DaylightPolicy policy) =>
        toSeq(Enumerable.Range(0, policy.HemisphereAzimuths * policy.HemisphereAltitudes))
            .TraverseM(i => {
                double az = 2.0 * Math.PI * (i % policy.HemisphereAzimuths) / policy.HemisphereAzimuths;
                double alt = Math.PI / 2.0 * (0.5 + i / policy.HemisphereAzimuths) / policy.HemisphereAltitudes;
                Vector3 ray = new(Math.Cos(alt) * Math.Sin(az), Math.Cos(alt) * Math.Cos(az), Math.Sin(alt));
                double weight = Math.Sin(alt) * Math.Cos(alt);
                return ClashScale.Occluded(scene.Scene, origin, ray, scene.SceneDiameter).Map(occluded => (Weight: weight, Occluded: occluded));
            })
            .As()
            .Map(static rays => rays.Fold((Open: 0.0, Total: 0.0), static (acc, r) => (acc.Open + (r.Occluded ? 0.0 : r.Weight), acc.Total + r.Weight)))
            .Map(static acc => acc.Total > 0.0 ? acc.Open / acc.Total : 0.0);

    // Mean reference-plane diffuse over the retained sky hours at the policy cadence: each hour casts ITS OWN
    // circumsolar ray, so the disc term enters exactly at the hours whose sun this target can see. The stride
    // subsamples the hour stream and the mean divides by what was probed, so cadence changes fidelity, never scale.
    static Fin<double> HourlyDiffuse(DaylightScene scene, Vector3 origin, Seq<SkyState> hours, double skyView, DaylightPolicy policy) =>
        toSeq(Enumerable.Range(0, hours.Count).Where(index => index % policy.OcclusionCadenceHours == 0).Select(index => hours[index]))
            .TraverseM(hour => ClashScale.Occluded(scene.Scene, origin, hour.Sun.Direction, scene.SceneDiameter)
                .Map(occluded => Diffuse(hour, skyView, occluded)))
            .As()
            .Map(static probed => probed.Fold((Sum: 0.0, Count: 0), static (acc, term) => (acc.Sum + term, acc.Count + 1)))
            .Map(static acc => acc.Count > 0 ? acc.Sum / acc.Count : 0.0);

    // Perez plane-of-array diffuse specialized to the horizontal reference plane: the isotropic dome carries the
    // measured sky-view factor in place of the analytic `(1 + cos S)/2`, the circumsolar disc rides `a/b = 1` under
    // an above-horizon sun and drops whole when this hour's ray is blocked, and the horizon band's `sin S` is zero.
    static double Diffuse(SkyState hour, double skyView, bool occluded) {
        double circumsolar = hour.Band.Circumsolar(hour.Brightness, hour.Sun.ZenithDeg * Math.PI / 180.0);
        return hour.DiffuseHorizontalWm2 * ((1.0 - circumsolar) * skyView + (occluded ? 0.0 : circumsolar));
    }

    static PerezBand Dominant(Seq<SkyState> hours) =>
        toSeq(hours.GroupBy(static h => h.Band).OrderByDescending(static g => g.Count())).Head.Map(static g => g.Key).IfNone(PerezBand.Overcast);
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

    // ONE centroid definition across all three components: the area-weighted shoelace, every axis divided by the
    // same 3·Σcross. A vertex-mean on the elevation axis alone re-weights a dense-tessellated edge and slides the
    // reference plane off the area centre the other two axes name.
    static Fin<Vector3> Centroid(FootprintPolygon footprint) {
        Seq<Vector3> next = footprint.Ring.Skip(1).Add(footprint.Ring[0]);
        (double Cross, double X, double Y, double Z) sum = footprint.Ring.Zip(next).Fold(
            (Cross: 0.0, X: 0.0, Y: 0.0, Z: 0.0),
            static (acc, edge) => {
                double cross = edge.Item1.X * edge.Item2.Y - edge.Item2.X * edge.Item1.Y;
                return (acc.Cross + cross, acc.X + (edge.Item1.X + edge.Item2.X) * cross,
                    acc.Y + (edge.Item1.Y + edge.Item2.Y) * cross, acc.Z + (edge.Item1.Z + edge.Item2.Z) * cross);
            });
        return Math.Abs(sum.Cross) > 1e-12
            ? Fin.Succ(new Vector3(sum.X / (3.0 * sum.Cross), sum.Y / (3.0 * sum.Cross), sum.Z / (3.0 * sum.Cross) + SampleLiftM))
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
