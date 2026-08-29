# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner owns the `Discipline.Daylight` assessment arm: it answers worst-design-day direct sun hours, mean shadow fraction, sky-view factor, and Perez all-weather diffuse irradiance at each target's reference plane, every one derived from the kernel `Rasm` solar almanac against the clash BVH. Weather-less requests require an explicit site, and present weather failures remain typed failures rather than silently degrading.

## [01]-[INDEX]

- [02]-[SKY_AND_SHADOW]: `RunDaylight` folds the Perez all-weather sky rows over the EPW ingress against the clash-BVH shadow-ray cast at design-day and hourly cadence.

## [02]-[SKY_AND_SHADOW]

- Owner: `PerezBand` `[SmartEnum<string>]` the eight published all-weather clearness bands, each carrying the full six-coefficient `(F11, F12, F13, F21, F22, F23)` row and the two brightening terms it evaluates (the published table, never a hardcoded interpolation nor a truncated column set); `DaylightPolicy` the sampling-cadence value object; `SkyState` the per-hour sky carrier (sun position, DNI + DHI, derived sky brightness Δ, resolved `PerezBand`); `WeatherSource` the two-row ingress axis (the admitted `EpwFile` SWIG row off the `WeatherRef` surface · the gridded netCDF-4/HDF5 corpus row with its declared cell and required explicit-site companion); `WeatherIngress` the boundary folding either row into one `WeatherObservations`; `DaylightAnalysis` the runner fold.
- Cases: with weather — per-target `worst-day-sun-hours`, `mean-shadow-fraction`, `sky-view-factor` (the hemisphere ray fan), `perez-diffuse-irradiance` (the hourly isotropic-dome + circumsolar sum over each hour's resolved band); weather-less — the degrade: the same geometric facts at the design days off the solar kernel over the request's explicit `Site`, the `sky-state` fact stating `"geometry-only"` inline, never a silently-defaulted sky; absent both weather and an explicit site the run fails `AssessmentInputMissing`.
- Law: the design-day sweep yields TWO declared reductions over one sample set, never one mixed aggregate — `worst-day-sun-hours` is the MINIMUM per-day lit-sample total across every requested design day (a day whose sun never clears the horizon reads zero rather than dropping out of the reduction), and `mean-shadow-fraction` is the occluded share of every above-horizon sample pooled across all of them; each is a separately named fact because a design brief accepts on the worst day and reports on the mean. The two take OPPOSITE stances on an empty sample set and both are right: a requested day contributes its honest zero lit hours to the acceptance extremum, while a pooled share over no sample is UNMEASURED evidence rather than the zero occlusion — fully lit — a `double` there once published.
- Law: circumsolar admission is probed PER WEATHER HOUR at the policy cadence — each retained `SkyState` casts its own occlusion ray along its own sun direction, so a target the morning sun reaches and the afternoon sun does not reads two different circumsolar admissions; a design-day-mean visibility ratio smeared across the annual sum reports a fabricated sky at every hour it was not measured at.
- Entry: `Run(graph, request, geometry, sink, clock)` resolves the target points and obstruction scene through the `GeometrySource` port (an unresolvable target fails `AnalysisFailed(Admission, Input)`), reads optional weather through `WeatherIngress.Read` over the source axis (a present-but-malformed source fails typed; an absent one selects the geometry-only degrade over the request's explicit `Site`; the gridded row reads one rank-3 single-cell annual hyperslab per variable at the request's declared cell), and mints the fact stream; the governing ratio is the worst target's required/achieved sun-hours (EN 17037 minimum-sunlight, the route row's citation), ABSENT where the request carries no requirement. Under weather the runner also lands the annual per-sensor irradiance matrix through the threaded `AssessmentSink` — a target-outermost `[targets, probedHours]` grid derived through the one `Runtime/archive#CHUNK_CURSOR` owner, written through the declared-session capsule onto `sink.Store`, with the same evidence crossing `sink.Series` in temporal form — and the result carries the artifact's `BlobKey` on `ResultArtifact`. The assessment content key rides IN from the dispatch spine because both sink legs address their rows by it; a runner-side re-derivation would key the artifact, the series, and the assessment node three ways.
- Result: rides the one `AssessmentPayload` case, no daylight-local result; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node, the `weather-cell` fact names which corpus cell served a gridded run, and the weather-bearing run's `ResultArtifact` names the annual matrix artifact.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own pin), PureHDF (`NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`, `HyperslabSelection`, `IH5Object.Attribute`/`AttributeExists` — the gridded row; the matrix artifact reaches the library only through the archive capsule), CommunityToolkit.HighPerformance (`MemoryOwner<double>` the pooled annual column each gridded variable reads into), Rasm (project — the kernel `SpatialIndex.Wire` node-link wire the staged scene decodes from, the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarSite`/`SunPosition` solar almanac, `Evidence<T>` the measured-versus-absent probe result, `Context`/`ToleranceLane` the footprint degeneracy gate), Rasm.Element (`ElementGraph`, `NodeId`, `FootprintPolygon`, `ContentAddress`, `BlobKey`, `Dimension`), Rasm.Persistence (project — the `Query/datasets#SERIES_ROSTER` `SeriesPoint` the temporal leg lands), the `Runtime/archive#CHUNK_CURSOR` `ChunkGrid`/`ArchiveSession`/`ArchiveSlot`/`ArchiveAttribute` write capsule, Generator.Equals (`[Equatable]`+`[OrderedEquality]` — the plane-vector reference-equality repair), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap on the same `SkyState` carrier; a tilted-plane fact (a window vertical-sky-component) composes the row's `Horizon` term beside the `Circumsolar` one already folded; a re-cadenced sweep or hemisphere fan is one `DaylightPolicy` column; sDA/ASE-class EN 17037 hour-threshold metrics are ONE reduction over the stored matrix rows — the scalar mean cannot reach them, and no re-run is ever needed; a new gridded variable is one `WeatherSource.Gridded` column; zero new surface.
- Boundary: shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — one ray engine on the one acceleration owner, never a daylight-local traversal, and the app stages the decoded scene on the request as `ObstructionScene`, its content key riding the assessment content-key fold so a re-shaded site re-keys; sky ingress rides the two-row `WeatherSource` axis — the energy lane's own `WeatherRef` surface through the admitted `EpwFile` reader, and the gridded corpus row over `Runtime/archive#HDF_ARCHIVE` whose netCDF SEMANTICS (CF `units`/`calendar`, coordinate datasets) resolve ABOVE the raw-HDF5 API (PureHDF surfaces raw objects), the `CfEpoch` object factory admitting the `hours since <date>` grammar and the calendar roster as ONE two-column datum and refusing the rest typed — never a second weather decode path nor a weather column on the sampling policy. Both source rows converge on ONE hour admission: a sky hour is an `(instant, DNI, DHI)` triple whichever wire carried it, so the finite-non-negative pair test and the above-horizon filter are stated once and the two rows cannot drift on which hour a malformed reading drops. Every gridded gate ACCUMULATES — the declared cell bound, the calendar, and the epoch report together — and the handle rides the bracketed archive session, which releases on every outcome arm where a `using` inside a fallible lambda released on the arms the lambda reached; the gridded corpus keys by CONTENT KEY beside its declared cell and year span, never by path; the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac is composed, never re-derived — the same owner `Rasm.AppUi` viewport sun-light and the Materials environment adapter compose — and its ANGLES project into the float clash coordinate at `SurveyRay`, this page's ONE narrowing, so the almanac's double bijection is never floored by a ray engine and no host coordinate reaches a signature here. EN 17037 fixes the reference plane HORIZONTAL, so the plane-of-array form collapses exactly: the isotropic weight `(1 + cos S)/2` becomes the measured sky-view factor, the circumsolar ratio `a/b` is unity under an above-horizon sun, and the horizon band's `sin S` factor is zero — the fold composes `Circumsolar` alone and `Horizon` stays row surface the tilted case reads, never a term applied at a tilt that annihilates it. Every sampling count and step is a `DaylightPolicy` column that folds the assessment content key, never a runner constant a re-cadenced sweep silently re-uses the cached answer of.

```csharp
using Vector3 = System.Numerics.Vector3;
using SeamVector3 = Rasm.Element.Graph.Vector3;

// --- [TYPES] ---------------------------------------------------------------------------
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

    public double Circumsolar(double brightness, double zenithRad) => Math.Max(0.0, F11 + F12 * brightness + F13 * zenithRad);
    public double Horizon(double brightness, double zenithRad) => F21 + F22 * brightness + F23 * zenithRad;

    public static PerezBand OfClearness(double epsilon) =>
        toSeq(Items).Find(band => epsilon >= band.EpsilonLow && epsilon < band.EpsilonHigh).IfNone(Overcast);
}

// --- [MODELS] --------------------------------------------------------------------------
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

public readonly record struct SkyState(Instant At, SunPosition Sun, double DirectNormalWm2, double DiffuseHorizontalWm2, double Brightness, PerezBand Band) {
    const double SolarConstantWm2 = 1367.0;

    public static SkyState Of(SolarSite site, Instant at, double directNormalWm2, double diffuseHorizontalWm2) {
        SunPosition sun = SunPosition.At(site, at);
        double zenithDeg = 90.0 - sun.AltitudeDeg;
        double zenithRad = zenithDeg * Math.PI / 180.0;
        double kappa = 1.041 * zenithRad * zenithRad * zenithRad;
        double clearness = diffuseHorizontalWm2 > 0.0
            ? ((diffuseHorizontalWm2 + directNormalWm2) / diffuseHorizontalWm2 + kappa) / (1.0 + kappa)
            : 1.0;
        return new SkyState(at, sun, directNormalWm2, diffuseHorizontalWm2,
            diffuseHorizontalWm2 * AirMass(zenithDeg) / Extraterrestrial(at.WithOffset(site.StandardOffset).DayOfYear),
            PerezBand.OfClearness(clearness));
    }

    static double AirMass(double zenithDeg) =>
        1.0 / (Math.Cos(zenithDeg * Math.PI / 180.0) + 0.50572 * Math.Pow(96.07995 - zenithDeg, -1.6364));

    static double Extraterrestrial(int dayOfYear) => SolarConstantWm2 * (1.0 + 0.033 * Math.Cos(2.0 * Math.PI * dayOfYear / 365.0));
}

public readonly record struct SunSample(LocalDate Day, bool Occluded);

public sealed record ObstructionScene(UInt128 Key, AccelerationStructure Index, ReadOnlyMemory<float> Triangles);

[Equatable]
public sealed partial record DaylightFinding(
    NodeId Target, double WorstDaySunHours, Evidence<double> MeanShadowFraction, double SkyViewFactor,
    Evidence<double> PerezDiffuseWm2, [property: OrderedEquality] double[] HourlyPlaneWm2);

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeatherSource {
    private WeatherSource() { }

    public sealed record Epw(WeatherRef Weather) : WeatherSource;

    public sealed record Gridded(string Path, UInt128 CorpusKey, int LatIndex, int LonIndex, int Years, string DniVar, string DhiVar) : WeatherSource;
}

public sealed record WeatherObservations(SolarSite Site, Seq<SkyState> Hours, Option<(UInt128 CorpusKey, int LatIndex, int LonIndex, int Years)> Cell);

[ObjectFactory<string>]
public sealed partial class CfEpoch {
    const string Prefix = "hours since ";

    static readonly FrozenSet<string> Calendars = FrozenSet.Create(StringComparer.Ordinal, "standard", "gregorian");

    public LocalDate Epoch { get; }

    public static Validation<Error, CfEpoch> Admit(string units, string calendar) =>
        (AdmissionSlots.Gate(
             Calendars.Contains(calendar),
             new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-calendar:{calendar}>")),
         Origin(units)).Apply(static (_, epoch) => new CfEpoch(epoch)).As();

    static Validation<Error, LocalDate> Origin(string units) =>
        units.StartsWith(Prefix, StringComparison.Ordinal)
            ? LocalDatePattern.Iso.Parse(units[Prefix.Length..].Trim()) is { Success: true, Value: LocalDate date }
                ? date
                : (Validation<Error, LocalDate>)new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-epoch:{units}>")
            : new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-units:{units}>");
}

public static class WeatherIngress {

    public static Fin<WeatherObservations> Read(WeatherSource source, Option<SolarSite> overrideSite = default) =>
        source.Switch(
            state: overrideSite,
            epw: static (site, row) => ReadEpw(row.Weather, site),
            gridded: static (site, row) => site
                .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.SiteAbsent, row.Path))
                .Bind(at => ReadGridded(row, at)));

    static Fin<WeatherObservations> Stream(
        SolarSite site, Seq<(Instant At, double Dni, double Dhi)> readings,
        Option<(UInt128 CorpusKey, int LatIndex, int LonIndex, int Years)> cell) =>
        readings
            .Filter(static row => double.IsFinite(row.Dni) && row.Dni >= 0.0 && double.IsFinite(row.Dhi) && row.Dhi >= 0.0)
            .Map(row => SkyState.Of(site, row.At, row.Dni, row.Dhi))
            .Filter(static hour => hour.Sun.AltitudeDeg > 0.0)
            .Match(
                Empty: () => Fin.Fail<WeatherObservations>(new ComputeFault.AnalysisFailed(
                    SolvePhase.Extraction, FailureKind.Input, "<daylight-weather-no-valid-daylight-hours>")),
                More: (head, tail) => Fin.Succ(new WeatherObservations(site, head.Cons(tail), cell)));

    static Fin<WeatherObservations> ReadEpw(WeatherRef weather, Option<SolarSite> overrideSite) =>
        File.Exists(weather.EpwPath)
            ? Try.lift(() => {
                    using OpenStudio.Path epwPath = OpenStudio.OpenStudioUtilitiesCore.toPath(weather.EpwPath);
                    using OpenStudio.EpwFile epw = new(epwPath);
                    Fin<SolarSite> site = overrideSite.Match(
                        Some: Fin.Succ,
                        None: () => FactoryBridge.Accept(SolarSite.Validate(
                            epw.latitude(), epw.longitude(),
                            Offset.FromTicks((long)(epw.timeZone() * NodaConstants.TicksPerHour)), epw.elevation(),
                            out SolarSite? admitted), admitted));
                    return site.Bind(admitted => {
                        using OpenStudio.EpwDataPointVector data = epw.data();
                        Instant yearStart = new LocalDate(2001, 1, 1).AtMidnight().WithOffset(admitted.StandardOffset).ToInstant();
                        Seq<(Instant At, double Dni, double Dhi)> readings = toSeq(Enumerable.Range(0, checked((int)data.Count)))
                            .Choose(i => {
                                using OpenStudio.EpwDataPoint point = data[i];
                                using OpenStudio.OptionalDouble dni = point.directNormalRadiation();
                                using OpenStudio.OptionalDouble dhi = point.diffuseHorizontalRadiation();
                                return dni.is_initialized() && dhi.is_initialized()
                                    ? Some((At: yearStart + Duration.FromHours(i), Dni: dni.get(), Dhi: dhi.get()))
                                    : None;
                            });
                        return Stream(admitted, readings, None);
                    });
                }).Run().Bind(static inner => inner)
            : Fin.Fail<WeatherObservations>(new ComputeFault.AnalysisFailed(
                SolvePhase.Admission, FailureKind.Input, $"<daylight-weather-missing:{weather.EpwPath}>"));

    static Fin<WeatherObservations> ReadGridded(WeatherSource.Gridded source, SolarSite site) =>
        HdfArchive.Session(new HdfSource.Path(source.Path), HdfArchivePolicy.Interchange, handle =>
            IO.lift(() =>
                (Bounds(handle, source), Epoch(handle), Span(handle))
                    .Apply(static (_, epoch, hours) => (Epoch: epoch, Hours: hours)).As().ToFin()
                    .Bind(dated => Column(handle, source, source.DniVar, dated.Hours)
                        .Bind(dni => Column(handle, source, source.DhiVar, dated.Hours)
                            .Bind(dhi => {
                                Instant epoch = dated.Epoch.Epoch.AtMidnight().WithOffset(site.StandardOffset).ToInstant();
                                return Stream(site,
                                    toSeq(Enumerable.Range(0, dated.Hours)).Map(i => (At: epoch + Duration.FromHours(i), Dni: dni.Span[i], Dhi: dhi.Span[i])),
                                    Some((source.CorpusKey, source.LatIndex, source.LonIndex, source.Years)));
                            })))))
            .Run();

    static Validation<Error, Unit> Bounds(HdfHandle handle, WeatherSource.Gridded source) =>
        source.LatIndex >= 0 && source.LatIndex < (long)handle.Dataset("latitude").Space.Dimensions[0]
        && source.LonIndex >= 0 && source.LonIndex < (long)handle.Dataset("longitude").Space.Dimensions[0]
            ? unit
            : new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-cell:{source.LatIndex}:{source.LonIndex}>");

    static Validation<Error, CfEpoch> Epoch(HdfHandle handle) {
        NativeDataset time = handle.Dataset("time");
        return CfEpoch.Admit(
            time.Attribute("units").Read<string>(),
            time.AttributeExists("calendar") ? time.Attribute("calendar").Read<string>() : "standard");
    }

    static Validation<Error, int> Span(HdfHandle handle) =>
        handle.Dataset("time").Space.Dimensions[0] is ulong extent && extent <= int.MaxValue
            ? (int)extent
            : new ComputeFault.PayloadOverBounds($"<daylight-gridded-hours:{handle.Dataset("time").Space.Dimensions[0]}>");

    static Fin<MemoryOwner<double>> Column(HdfHandle handle, WeatherSource.Gridded source, string variable, int hours) =>
        Try.lift(() => {
                MemoryOwner<double> column = MemoryOwner<double>.Allocate(hours, AllocationMode.Clear);
                handle.Dataset(variable).Read<double>(handle.Access, column.Span,
                    new HyperslabSelection(3, [0UL, (ulong)source.LatIndex, (ulong)source.LonIndex], [(ulong)hours, 1UL, 1UL]));
                return Fin.Succ(column);
            }).Run().Bind(static inner => inner);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DaylightAnalysis {
    const string Unmeasured = "unmeasured";

    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, AssessmentSink sink, ContentAddress key, IClock clock) =>
        from _ in (AdmissionSlots.Gate(
                       !request.DesignDays.IsEmpty,
                       new ComputeFault.AssessmentInputMissing(AssessmentInputReason.DesignDaysEmpty, request.Route.Key)),
                   AdmissionSlots.Gate(
                       double.IsFinite(request.RequiredSunHours) && request.RequiredSunHours >= 0.0,
                       new ComputeFault.AssessmentInputMissing(AssessmentInputReason.PolicyInvalid, $"{request.RequiredSunHours:R}")))
            .Apply(static (_, _) => unit).As().ToFin()
        from scene in DaylightScene.Of(graph, request, geometry)
        from weather in request.Weather.Match(
            Some: source => WeatherIngress.Read(source, request.Site).Map(static value => Some(value)),
            None: static () => Fin.Succ(Option<WeatherObservations>.None))
        from site in (weather.Map(static w => w.Site) | request.Site)
            .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.SiteAbsent, string.Empty))
        let probed = weather.Map(w => Probed(w.Hours, request.Policy))
        from findings in scene.Targets.TraverseM(target => Target(scene, target, site, probed, request)).As()
        let govern = request.RequiredSunHours > 0.0
            ? findings.Fold(Option<double>.None, (worst, f) => Some(worst.Match(
                Some: held => Math.Max(held, request.RequiredSunHours / f.WorstDaySunHours),
                None: () => request.RequiredSunHours / f.WorstDaySunHours)))
            : Option<double>.None
        from perTarget in findings.TraverseM(f => AssessmentFact.Rows(
            AssessmentFact.Measure($"{f.Target.ToValue()}/worst-day-sun-hours", Dimension.DurationDim, f.WorstDaySunHours * 3600.0),
            Shadow(f),
            AssessmentFact.Ratio($"{f.Target.ToValue()}/sky-view-factor", f.SkyViewFactor))).As()
        from skyFacts in weather.Match(
            Some: w => findings.TraverseM(f => Irradiance(f)).As()
                .Map(perez => Seq(AssessmentFact.Text("sky-state", $"perez:{Dominant(w.Hours).Key}"))
                    + w.Cell.Map(static c => AssessmentFact.Text("weather-cell", $"{c.CorpusKey:x32}:{c.LatIndex}:{c.LonIndex}:{c.Years}")).ToSeq()
                    + perez),
            None: () => Fin.Succ(Seq(AssessmentFact.Text("sky-state", "geometry-only"))))
        from matrix in probed.Match(
            Some: sky => Matrix(findings, sky, request, sink, key).Map(Some),
            None: static () => Fin.Succ(Option<ArtifactContent>.None))
        from result in AssessmentResult.Of(
            request.Route,
            perTarget.Bind(static rows => rows) + skyFacts,
            govern,
            clock.GetCurrentInstant(),
            RunKey,
            resultArtifact: matrix)
        select result;

    static Fin<AssessmentFact> Shadow(DaylightFinding finding) =>
        finding.MeanShadowFraction.Value().Match(
            Some: share => AssessmentFact.Ratio($"{finding.Target.ToValue()}/mean-shadow-fraction", share),
            None: () => Fin.Succ(AssessmentFact.Text($"{finding.Target.ToValue()}/mean-shadow-fraction", Unmeasured)));

    static Fin<AssessmentFact> Irradiance(DaylightFinding finding) =>
        finding.PerezDiffuseWm2.Value().Match(
            Some: wm2 => AssessmentFact.Measure($"{finding.Target.ToValue()}/perez-diffuse-irradiance", Dimension.IrradianceDim, wm2),
            None: () => Fin.Succ(AssessmentFact.Text($"{finding.Target.ToValue()}/perez-diffuse-irradiance", Unmeasured)));

    static Seq<SkyState> Probed(Seq<SkyState> hours, DaylightPolicy policy) =>
        toSeq(Enumerable.Range(0, hours.Count).Where(index => index % policy.OcclusionCadenceHours == 0).Select(index => hours[index]));

    static Fin<ArtifactContent> Matrix(Seq<DaylightFinding> findings, Seq<SkyState> probed, AssessmentRequest.Daylight request, AssessmentSink sink, ContentAddress key) =>
        ChunkGrid.Derive([findings.Count], components: probed.Count, targetChunkElements: probed.Count).ToFin()
            .Bind(grid => {
                ArchiveSlot<double> slot = new("irradiance", grid);
                using MemoryStream staged = new();
                return ArchiveSession.Write(staged, HdfArchivePolicy.Interchange, Seq<IArchiveSlot>(slot),
                        Seq((MatrixTargets, (ArchiveAttribute)new ArchiveAttribute.Text(string.Join(' ', findings.Map(static f => f.Target.ToValue())))),
                            (MatrixCadence, new ArchiveAttribute.Whole(request.Policy.OcclusionCadenceHours)),
                            (MatrixHours, new ArchiveAttribute.Whole(probed.Count))),
                        session => IO.lift(() => session.Cursor(slot)
                            .Bind(cursor => findings.TraverseM(finding => cursor.Write(finding.HourlyPlaneWm2)).As())
                            .Map(static _ => unit)))
                    .Run()
                    .Bind(_ => sink.Store(staged.ToArray()).Run())
                    .Bind(blob => sink.Series(findings.Bind(finding => probed
                            .Zip(toSeq(finding.HourlyPlaneWm2))
                            .Map(pair => new SeriesPoint(key.ToValue(), pair.First.At, pair.Second, Seq(finding.Target.ToValue())))))
                        .Run()
                        .Map(_ => blob));
            });

    const string MatrixTargets = "targets";
    const string MatrixCadence = "cadence-hours";
    const string MatrixHours = "hours";

    static Fin<DaylightFinding> Target(DaylightScene scene, NodeId target, SolarSite site, Option<Seq<SkyState>> probed, AssessmentRequest.Daylight request) {
        Vector3 origin = scene.SamplePoints[target];
        return from sweep in DesignSweep(scene, origin, site, request)
               from skyView in SkyView(scene, origin, request.Policy)
               from hourly in probed.Match(
                   Some: sky => HourlyDiffuse(scene, origin, sky, skyView).Map(static measured => (Mean: Evidence.Of(Some(measured.Mean)), measured.PlaneWm2)),
                   None: static () => Fin.Succ((Mean: (Evidence<double>)new Evidence<double>.Absent(), PlaneWm2: System.Array.Empty<double>())))
               select new DaylightFinding(
                   target,
                   WorstDaySunHours(sweep, request.DesignDays, request.Policy.SunStepHours),
                   MeanShadowFraction(sweep),
                   skyView,
                   hourly.Mean,
                   hourly.PlaneWm2);
    }

    static Vector3 SurveyRay(SunPosition sun) =>
        SurveyRay(sun.AzimuthDeg * Math.PI / 180.0, sun.AltitudeDeg * Math.PI / 180.0);

    static Vector3 SurveyRay(double azimuth, double altitude) =>
        new((float)(Math.Cos(altitude) * Math.Sin(azimuth)),
            (float)(Math.Cos(altitude) * Math.Cos(azimuth)),
            (float)Math.Sin(altitude));

    static Fin<Seq<SunSample>> DesignSweep(DaylightScene scene, Vector3 origin, SolarSite site, AssessmentRequest.Daylight request) =>
        request.DesignDays
            .Bind(day => toSeq(Enumerable.Range(0, request.Policy.SunSamplesPerDay))
                .Map(i => day.AtMidnight().WithOffset(site.StandardOffset).ToInstant() + (Duration.FromHours(request.Policy.SunStepHours) * i))
                .Map(at => (Instant: at, Sun: SunPosition.At(site, at)))
                .Filter(static row => row.Sun.AltitudeDeg > 0.0)
                .Map(row => (Day: day, Ray: SurveyRay(row.Sun))))
            .TraverseM(row => ClashScale.Occluded(scene.Scene, origin, row.Ray, scene.SceneDiameter)
                .Map(occluded => new SunSample(row.Day, occluded)))
            .As();

    static double WorstDaySunHours(Seq<SunSample> sweep, Seq<LocalDate> days, double stepHours) {
        HashMap<LocalDate, int> lit = sweep.Filter(static sample => !sample.Occluded)
            .Fold(HashMap<LocalDate, int>(), static (tally, sample) => tally.AddOrUpdate(sample.Day, static held => held + 1, 1));
        return days.Map(day => lit.Find(day).IfNone(0) * stepHours)
            .Fold(double.PositiveInfinity, static (worst, hours) => Math.Min(worst, hours));
    }

    static Evidence<double> MeanShadowFraction(Seq<SunSample> sweep) =>
        Evidence.Of(sweep.IsEmpty
            ? Option<double>.None
            : Some(sweep.Count(static sample => sample.Occluded) / (double)sweep.Count));

    static Fin<double> SkyView(DaylightScene scene, Vector3 origin, DaylightPolicy policy) =>
        toSeq(Enumerable.Range(0, policy.HemisphereAzimuths * policy.HemisphereAltitudes))
            .TraverseM(i => {
                double az = 2.0 * Math.PI * (i % policy.HemisphereAzimuths) / policy.HemisphereAzimuths;
                double alt = Math.PI / 2.0 * (0.5 + i / policy.HemisphereAzimuths) / policy.HemisphereAltitudes;
                double weight = Math.Sin(alt) * Math.Cos(alt);
                return ClashScale.Occluded(scene.Scene, origin, SurveyRay(az, alt), scene.SceneDiameter).Map(occluded => (Weight: weight, Occluded: occluded));
            })
            .As()
            .Map(static rays => rays.Fold((Open: 0.0, Total: 0.0), static (acc, r) => (acc.Open + (r.Occluded ? 0.0 : r.Weight), acc.Total + r.Weight)))
            .Map(static acc => acc.Open / acc.Total);

    static Fin<(double Mean, double[] PlaneWm2)> HourlyDiffuse(DaylightScene scene, Vector3 origin, Seq<SkyState> probed, double skyView) =>
        probed
            .TraverseM(hour => ClashScale.Occluded(scene.Scene, origin, SurveyRay(hour.Sun), scene.SceneDiameter)
                .Map(occluded => {
                    double zenithDeg = 90.0 - hour.Sun.AltitudeDeg;
                    double diffuse = Diffuse(hour, skyView, occluded, zenithDeg);
                    double direct = occluded ? 0.0 : hour.DirectNormalWm2 * Math.Max(0.0, Math.Cos(zenithDeg * Math.PI / 180.0));
                    return (Diffuse: diffuse, Plane: diffuse + direct);
                }))
            .As()
            .Map(static terms => (
                Mean: terms.Map(static term => term.Diffuse).Sum() / terms.Count,
                PlaneWm2: terms.Map(static term => term.Plane).ToArray()));

    static double Diffuse(SkyState hour, double skyView, bool occluded, double zenithDeg) {
        double circumsolar = hour.Band.Circumsolar(hour.Brightness, zenithDeg * Math.PI / 180.0);
        return hour.DiffuseHorizontalWm2 * ((1.0 - circumsolar) * skyView + (occluded ? 0.0 : circumsolar));
    }

    static PerezBand Dominant(Seq<SkyState> hours) =>
        toSeq(hours.GroupBy(static h => h.Band).OrderByDescending(static g => g.Count())).Head.Map(static g => g.Key).IfNone(PerezBand.Overcast);
}

public sealed record DaylightScene(Seq<NodeId> Targets, Map<NodeId, Vector3> SamplePoints, AdmittedScene Scene, float SceneDiameter) {
    const double SampleLiftM = 0.85;

    public static Fin<DaylightScene> Of(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry) =>
        request.Targets
            .TraverseM(id => graph.Find<Node.Object>(id)
                .Bind(o => geometry.Footprint(o.Representations))
                .Filter(static f => !f.IsEmpty)
                .Bind(footprint => Centroid(footprint).Map(point => (Id: id, Point: point)))
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-target-unresolved:{id.ToValue()}>")))
            .As()
            .Bind(points => Diameter(request.Scene.Index)
                .Filter(_ => request.Scene.Key != UInt128.Zero && !request.Scene.Triangles.IsEmpty)
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<daylight-obstruction-scene-invalid>"))
                .Bind(diameter => AdmittedScene.Of(request.Scene.Index, request.Scene.Triangles, ClashPolicy.Canonical)
                    .Map(admitted => new DaylightScene(
                        request.Targets,
                        points.Fold(Map<NodeId, Vector3>(), static (acc, point) => acc.Add(point.Id, point.Point)),
                        admitted,
                        diameter))));

    static Fin<Vector3> Centroid(FootprintPolygon footprint) {
        SeamVector3 apex = footprint.Ring[0];
        (double Cross, double X, double Y, double Z) sum = footprint.Ring
            .Zip(footprint.Ring.Skip(1).Add(apex))
            .Fold(
                (Cross: 0.0, X: 0.0, Y: 0.0, Z: 0.0),
                (acc, edge) => {
                    double cross = ((edge.Item1.X - apex.X) * (edge.Item2.Y - apex.Y))
                        - ((edge.Item2.X - apex.X) * (edge.Item1.Y - apex.Y));
                    return (acc.Cross + cross,
                        acc.X + ((edge.Item1.X + edge.Item2.X + apex.X) * cross),
                        acc.Y + ((edge.Item1.Y + edge.Item2.Y + apex.Y) * cross),
                        acc.Z + ((edge.Item1.Z + edge.Item2.Z + apex.Z) * cross));
                });
        return Math.Abs(sum.Cross) > Context.Canonical.For(ToleranceLane.Identity).Value
            ? Fin.Succ(new Vector3(
                (float)(sum.X / (3.0 * sum.Cross)),
                (float)(sum.Y / (3.0 * sum.Cross)),
                (float)((sum.Z / (3.0 * sum.Cross)) + SampleLiftM)))
            : Fin.Fail<Vector3>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<daylight-target-degenerate-footprint>"));
    }

    static Option<float> Diameter(AccelerationStructure index) =>
        index.Bounds.Span is { Length: >= 6 } bounds
            ? Some(MathF.Sqrt(
                ((bounds[3] - bounds[0]) * (bounds[3] - bounds[0]))
                + ((bounds[4] - bounds[1]) * (bounds[4] - bounds[1]))
                + ((bounds[5] - bounds[2]) * (bounds[5] - bounds[2])))).Filter(static diagonal => diagonal > 0f)
            : None;
}
```
