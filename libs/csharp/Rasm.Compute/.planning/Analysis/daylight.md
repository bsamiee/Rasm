# [COMPUTE_DAYLIGHT]

Rasm.Compute daylight runner owns the `Discipline.Daylight` assessment arm: it answers worst-design-day direct sun hours, mean shadow fraction, sky-view factor, and Perez all-weather diffuse irradiance at each target's reference plane, every one derived from the kernel `Rasm` solar almanac against the clash BVH. Weather-less requests require an explicit site, and present weather failures remain typed failures rather than silently degrading.

## [01]-[INDEX]

- [02]-[SKY_AND_SHADOW]: `RunDaylight` folds the Perez all-weather sky rows over the EPW ingress against the clash-BVH shadow-ray cast at design-day and hourly cadence.

## [02]-[SKY_AND_SHADOW]

- Owner: `PerezBand` `[SmartEnum<string>]` the eight published all-weather clearness bands, each carrying the full six-coefficient `(F11, F12, F13, F21, F22, F23)` row and the two brightening terms it evaluates (the published table, never a hardcoded interpolation nor a truncated column set); `DaylightPolicy` the sampling-cadence value object; `SkyState` the per-hour sky carrier (sun position, DNI + DHI, derived sky brightness Δ, resolved `PerezBand`); `WeatherSource` the two-row ingress axis (the admitted `EpwFile` SWIG row off the `WeatherRef` surface · the gridded netCDF-4/HDF5 corpus row with its declared cell and required explicit-site companion); `WeatherIngress` the boundary folding either row into one `WeatherObservations`; `DaylightAnalysis` the runner fold.
- Cases: with weather — per-target `worst-day-sun-hours`, `mean-shadow-fraction`, `sky-view-factor` (the hemisphere ray fan), `perez-diffuse-irradiance` (the hourly isotropic-dome + circumsolar sum over each hour's resolved band); weather-less — the degrade: the same geometric facts at the design days off the solar kernel over the request's explicit `Site`, the `sky-state` fact stating `"geometry-only"` inline, never a silently-defaulted sky; absent both weather and an explicit site the run rails `AssessmentInputMissing`.
- Law: the design-day sweep yields TWO declared reductions over one sample set, never one mixed aggregate — `worst-day-sun-hours` is the MINIMUM per-day lit-sample total across every requested design day (a day whose sun never clears the horizon reads zero rather than dropping out of the reduction), and `mean-shadow-fraction` is the occluded share of every above-horizon sample pooled across all of them; each is a separately named fact because a design brief accepts on the worst day and reports on the mean. The two take OPPOSITE stances on an empty sample set and both are right: a requested day contributes its honest zero lit hours to the acceptance extremum, while a pooled share over no sample is UNMEASURED evidence rather than the zero occlusion — fully lit — a `double` there once published.
- Law: circumsolar admission is probed PER WEATHER HOUR at the policy cadence — each retained `SkyState` casts its own occlusion ray along its own sun direction, so a target the morning sun reaches and the afternoon sun does not reads two different circumsolar admissions; a design-day-mean visibility ratio smeared across the annual sum reports a fabricated sky at every hour it was not measured at.
- Entry: `Run(graph, request, geometry, sink, key, clock)` resolves the target points and obstruction scene through the `GeometrySource` port (an unresolvable target rails `AnalysisFailed(Admission, Input)`), reads optional weather through `WeatherIngress.Read` over the source axis (a present-but-malformed source rails typed; an absent one selects the geometry-only degrade over the request's explicit `Site`; the gridded row reads one rank-3 single-cell annual hyperslab per variable at the request's declared cell), and mints the fact stream; the governing ratio is the worst target's required/achieved sun-hours (EN 17037 minimum-sunlight, the route row's citation), ABSENT where the request carries no requirement. Under weather the runner also lands the annual per-sensor irradiance matrix through the threaded `AssessmentSink` — a target-outermost `[targets, probedHours]` grid derived through the one `Runtime/archive#CHUNK_CURSOR` owner, written through the declared-session capsule onto `sink.Store`, with the same evidence crossing `sink.Series` in temporal form — and the result carries the artifact's `BlobKey` on `ResultBlob`. The assessment content key rides IN from the dispatch spine because both sink legs address their rows by it; a runner-side re-derivation would key the artifact, the series, and the assessment node three ways.
- Receipt: rides the one `ComputeReceipt.Assessment` case, no daylight-local receipt; the `sky-state` fact (`perez:<band>` or `geometry-only`) makes the degrade auditable off the baked node, the `weather-cell` fact names which corpus cell served a gridded run, and the weather-bearing run's `ResultBlob` names the annual matrix artifact.
- Packages: NREL.OpenStudio.macOS-arm64 (the `EpwFile` reader — `latitude()`/`longitude()`/`timeZone()`/`elevation()`, `data()` → `EpwDataPoint.directNormalRadiation()`/`diffuseHorizontalRadiation()` `OptionalDouble` under the SWIG `is_initialized()`-then-`get()` discipline — the energy lane's own pin), PureHDF (`NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`, `HyperslabSelection`, `IH5Object.Attribute`/`AttributeExists` — the gridded row; the matrix artifact reaches the library only through the archive capsule), CommunityToolkit.HighPerformance (`MemoryOwner<double>` the pooled annual column each gridded variable reads into), Rasm (project — the kernel `Spatial.Apply(SpatialOp.Wire)` node-link wire the staged scene decodes from, the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition`/`SolarSite`/`SunPosition` solar almanac, `Evidence<T>` the measured-versus-absent probe receipt, `Context`/`ToleranceLane` the footprint degeneracy gate, `Op`), Rasm.Element (`ElementGraph`, `NodeId`, `FootprintPolygon`, `ContentAddress`, `BlobKey`, `Dimension`), Rasm.Persistence (project — the `Query/datasets#SERIES_ROSTER` `SeriesPoint` the temporal leg lands), the `Runtime/archive#CHUNK_CURSOR` `ChunkGrid`/`ArchiveSession`/`ArchiveSlot`/`ArchiveAttribute` write capsule, Generator.Equals (`[Equatable]`+`[OrderedEquality]` — the plane-vector reference-equality repair), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new sky model is one band-table swap on the same `SkyState` carrier; a tilted-plane fact (a window vertical-sky-component) composes the row's `Horizon` term beside the `Circumsolar` one already folded; a re-cadenced sweep or hemisphere fan is one `DaylightPolicy` column; sDA/ASE-class EN 17037 hour-threshold metrics are ONE reduction over the stored matrix rows — the scalar mean cannot reach them, and no re-run is ever needed; a new gridded variable is one `WeatherSource.Gridded` column; zero new surface.
- Boundary: shadow rays are `Solver/clash#CLASH_AND_TWIN` `ClashScale.Occluded` over the decoded kernel BVH — one ray engine on the one acceleration owner, never a daylight-local traversal, and the app stages the decoded scene on the request as `ObstructionScene`, its content key riding the assessment content-key fold so a re-shaded site re-keys; sky ingress rides the two-row `WeatherSource` axis — the energy lane's own `WeatherRef` surface through the admitted `EpwFile` reader, and the gridded corpus row over `Runtime/archive#HDF_ARCHIVE` whose netCDF SEMANTICS (CF `units`/`calendar`, coordinate datasets) resolve ABOVE the raw-HDF5 rail (PureHDF surfaces raw objects), the `CfEpoch` object factory admitting the `hours since <date>` grammar and the calendar roster as ONE two-column datum and refusing the rest typed — never a second weather decode path nor a weather column on the sampling policy. Both source rows converge on ONE hour admission: a sky hour is an `(instant, DNI, DHI)` triple whichever wire carried it, so the finite-non-negative pair test and the above-horizon filter are stated once and the two rows cannot drift on which hour a malformed reading drops. Every gridded gate ACCUMULATES — the declared cell bound, the calendar, and the epoch report together — and the handle rides the bracketed archive session, which releases on every outcome arm where a `using` inside a rail lambda released on the arms the lambda reached; the gridded corpus keys by CONTENT KEY beside its declared cell and year span, never by path; the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac is composed, never re-derived — the same owner `Rasm.AppUi` viewport sun-light and the Materials environment adapter compose — and its ANGLES project into the float clash coordinate at `SurveyRay`, this page's ONE narrowing, so the almanac's double bijection is never floored by a ray engine and no host coordinate reaches a signature here. EN 17037 fixes the reference plane HORIZONTAL, so the plane-of-array form collapses exactly: the isotropic weight `(1 + cos S)/2` becomes the measured sky-view factor, the circumsolar ratio `a/b` is unity under an above-horizon sun, and the horizon band's `sin S` factor is zero — the fold composes `Circumsolar` alone and `Horizon` stays row surface the tilted case reads, never a term applied at a tilt that annihilates it. Every sampling count and step is a `DaylightPolicy` column that folds the assessment content key, never a runner constant a re-cadenced sweep silently re-uses the cached answer of.

```csharp signature
using Vector3 = System.Numerics.Vector3;             // the clash engine's float coordinate owns the bare name; the seam Rasm.Element.Graph.Vector3 double triple is spelled whole
using SeamVector3 = Rasm.Element.Graph.Vector3;      // footprint-ring coordinate the centroid fan folds in double before its one narrowing

// --- [TYPES] -------------------------------------------------------------------------------
// PerezBand transcribes the eight published clearness bands (overcast 1.000–1.065 through pristine >6.200),
// each row carrying the whole six-coefficient set: the band resolves from the derived ε, and F11..F13 / F21..F23
// are the two independent linear forms in sky brightness Δ and solar zenith. Dropping F13/F23 collapses the
// zenith dependence the model's whole low-sun behaviour rides on, so the row transcribes all six or none.
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

// SkyState derives one hour of sky ONCE at ingress because every column is site-and-instant data no target
// varies: almanac sun position the circumsolar ray reuses, EPW DNI/DHI pair, Perez sky brightness Δ, and the
// band the clearness ε resolves. Per-target re-derivation runs the ephemeris once per target per hour for one answer.
public readonly record struct SkyState(Instant At, SunPosition Sun, double DirectNormalWm2, double DiffuseHorizontalWm2, double Brightness, PerezBand Band) {
    const double SolarConstantWm2 = 1367.0;

    // The site's own `Offset` dates the day-of-year the eccentricity term reads, so the mint takes the site and
    // nothing beside it — the three `Offset.FromTicks(site.TimezoneHours * ...)` re-derivations that once threaded
    // an offset parameter through both ingress arms and the design sweep were one column read three ways.
    public static SkyState Of(SolarSite site, Instant at, double directNormalWm2, double diffuseHorizontalWm2) {
        SunPosition sun = SolarPosition.At(site, at);
        double zenithRad = sun.ZenithDeg * Math.PI / 180.0;
        // Clearness ε carries the κZ³ term (κ = 1.041 for Z in radians) so a low sun is not read as an
        // overcast sky; a zero-diffuse hour has no defined ratio and reads the overcast floor.
        double kappa = 1.041 * zenithRad * zenithRad * zenithRad;
        double clearness = diffuseHorizontalWm2 > 0.0
            ? ((diffuseHorizontalWm2 + directNormalWm2) / diffuseHorizontalWm2 + kappa) / (1.0 + kappa)
            : 1.0;
        return new SkyState(at, sun, directNormalWm2, diffuseHorizontalWm2,
            diffuseHorizontalWm2 * AirMass(sun.ZenithDeg) / Extraterrestrial(at.WithOffset(site.Timezone).DayOfYear),
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

// One design-day occlusion sample: the day it belongs to and the clash verdict along that instant's sun ray. The
// day tag rides the row because the two reductions partition on it differently, and a named row rather than an
// anonymous pair is what lets the sweep, the worst-day tally, and the pooled share read one shape across three folds.
public readonly record struct SunSample(LocalDate Day, bool Occluded);

// App-staged obstruction scene carries the kernel spatial-wire snapshot content key the assessment content-key
// fold reads — a re-shaded site re-keys — the AccelerationStructure the Solver/clash decode produced from
// Spatial.Apply(SpatialOp.Wire), and the federated triangle wire the occlusion rays walk.
public sealed record ObstructionScene(UInt128 Key, AccelerationStructure Index, ReadOnlyMemory<float> Triangles);

// Per-target finding crosses weather and geometry-only paths in one shape; each sun column names its own
// reduction, so no consumer can read the worst day's hours as an annual mean or the pooled shadow share as a daily one.
// `HourlyPlaneWm2` is the per-probed-hour reference-plane global irradiance (diffuse + unoccluded direct) the
// annual matrix artifact rows — EMPTY on a geometry-only run, which measures no sky. The two REDUCED columns are
// `Evidence<double>` because each has a state where no probe ran at all — a target whose sun never clears the
// horizon pools no shadow sample, a geometry-only run reads no sky — and a `double` there cannot say so: `0.0`
// shadow means FULLY LIT and `0.0` irradiance means a dark sky, both readings a design review acts on. The two
// UNREDUCED columns stay bare doubles because their folds are structurally non-empty (the requested design-day
// roster and the validated hemisphere fan each guarantee at least one sample).
// `[Equatable]` closes the array's reference-equality trap: two identical runs' plane vectors compare unequal
// under record equality, so the matrix rows order element-wise instead.
[Equatable]
public sealed partial record DaylightFinding(
    NodeId Target, double WorstDaySunHours, Evidence<double> MeanShadowFraction, double SkyViewFactor,
    Evidence<double> PerezDiffuseWm2, [property: OrderedEquality] double[] HourlyPlaneWm2);

// --- [BOUNDARIES] --------------------------------------------------------------------------
// Two-row weather source axis. netCDF SEMANTICS resolve ABOVE the raw-HDF5 rail — PureHDF surfaces raw groups,
// datasets, and attributes — so the CF `units`/`calendar` attributes read and GATE at this boundary, and a
// corpus outside `hours since <date>` on a standard calendar refuses typed rather than mis-dating a year of sky.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeatherSource {
    private WeatherSource() { }

    public sealed record Epw(WeatherRef Weather) : WeatherSource;

    // One shared gridded corpus serves many sites, so the assessment fold reads the corpus CONTENT KEY — never the
    // path — with the declared cell indices and year span joined beside it. On the request the app stages the
    // resolved cell exactly as it stages the obstruction scene, and the explicit `SolarSite` is the REQUIRED
    // companion — a gridded corpus names cells, never a site.
    public sealed record Gridded(string Path, UInt128 CorpusKey, int LatIndex, int LonIndex, int Years, string DniVar, string DhiVar) : WeatherSource;
}

// `Cell` reports which corpus cell served — None on the EPW row, whose file IS its site.
public sealed record WeatherObservations(SolarSite Site, Seq<SkyState> Hours, Option<(UInt128 CorpusKey, int LatIndex, int LonIndex, int Years)> Cell);

// CF time-axis grammar as a foreign shape admitted ONCE. `units` and `calendar` are a two-column datum, not two
// independent string tests, and the pair either yields a dated epoch or refuses naming which column failed — the
// unchecked `[.. 10]` slice and the `GetValueOrThrow` the ladder once carried both raised on a `units` this
// grammar simply declines.
[ObjectFactory<string>]
public sealed partial class CfEpoch {
    const string Prefix = "hours since ";

    // The two calendars whose day count is the proleptic Gregorian one NodaTime's ISO chronology answers; a 360-day
    // or no-leap corpus dates every hour after the first February differently and is refused rather than mis-read.
    static readonly FrozenSet<string> Calendars = FrozenSet.Create(StringComparer.Ordinal, "standard", "gregorian");

    public LocalDate Epoch { get; }

    public static Validation<Error, CfEpoch> Admit(string units, string calendar) =>
        (Calendar(calendar), Origin(units)).Apply(static (_, epoch) => new CfEpoch(epoch)).As();

    static Validation<Error, Unit> Calendar(string calendar) =>
        Calendars.Contains(calendar)
            ? unit
            : new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-calendar:{calendar}>");

    static Validation<Error, LocalDate> Origin(string units) =>
        units.StartsWith(Prefix, StringComparison.Ordinal)
            ? LocalDatePattern.Iso.Parse(units[Prefix.Length..].Trim()) is { Success: true, Value: LocalDate date }
                ? date
                : (Validation<Error, LocalDate>)new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-epoch:{units}>")
            : new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<daylight-gridded-units:{units}>");
}

// Weather ingress off the two-row source axis. Both rows end in ONE admission — `Stream` — because a sky hour is
// the same datum whichever wire carried it: an (instant, DNI, DHI) triple, admitted when both readings are finite
// and non-negative and the sun clears the horizon. The twin `List<SkyState>` accumulators the two arms once
// carried let the EPW leg and the gridded leg drift on which hour a malformed reading drops.
// Exemption: the SWIG vector walk and per-handle using discipline are the native marshaling statement seam.
public static class WeatherIngress {
    static readonly Op ReadKey = Op.Of(name: nameof(Read));

    public static Fin<WeatherObservations> Read(WeatherSource source, Option<SolarSite> overrideSite = default) =>
        source.Switch(
            state: overrideSite,
            epw: static (site, row) => ReadEpw(row.Weather, site),
            gridded: static (site, row) => site
                .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.SiteAbsent, row.Path))
                .Bind(at => ReadGridded(row, at)));

    // THE admission both rows converge on. Presence is preserved to this one gate: a reading the wire never
    // declared contributes no hour, never a fabricated zero, and an above-horizon filter that survives nothing
    // is a REFUSAL rather than an empty year the folds would read as a sunless site.
    static Fin<WeatherObservations> Stream(
        SolarSite site, Seq<(Instant At, double Dni, double Dhi)> readings,
        Option<(UInt128 CorpusKey, int LatIndex, int LonIndex, int Years)> cell) =>
        readings
            .Filter(static row => double.IsFinite(row.Dni) && row.Dni >= 0.0 && double.IsFinite(row.Dhi) && row.Dhi >= 0.0)
            .Map(row => SkyState.Of(site, row.At, row.Dni, row.Dhi))
            .Filter(static hour => hour.Sun.AboveHorizon)
            .Match(
                Empty: () => Fin.Fail<WeatherObservations>(new ComputeFault.AnalysisFailed(
                    SolvePhase.Extraction, FailureKind.Input, "<daylight-weather-no-valid-daylight-hours>")),
                More: (head, tail) => Fin.Succ(new WeatherObservations(site, head.Cons(tail), cell)));

    static Fin<WeatherObservations> ReadEpw(WeatherRef weather, Option<SolarSite> overrideSite) =>
        File.Exists(weather.EpwPath)
            ? ReadKey.Catch(() => {
                    using OpenStudio.Path epwPath = OpenStudio.OpenStudioUtilitiesCore.toPath(weather.EpwPath);
                    using OpenStudio.EpwFile epw = new(epwPath);
                    // The header timezone is an offset in hours; the site column is a NodaTime Offset, so the one
                    // conversion happens here at the wire and no consumer re-derives it from a fractional double.
                    Fin<SolarSite> site = overrideSite.Match(
                        Some: Fin.Succ,
                        None: () => ReadKey.AcceptValidated(SolarSite.Validate(
                            epw.latitude(), epw.longitude(),
                            Offset.FromTicks((long)(epw.timeZone() * NodaConstants.TicksPerHour)), epw.elevation(),
                            out SolarSite? admitted), admitted));
                    return site.Bind(admitted => {
                        using OpenStudio.EpwDataPointVector data = epw.data();
                        Instant yearStart = new LocalDate(2001, 1, 1).AtMidnight().WithOffset(admitted.Timezone).ToInstant();
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
                })
            : Fin.Fail<WeatherObservations>(new ComputeFault.AnalysisFailed(
                SolvePhase.Admission, FailureKind.Input, $"<daylight-weather-missing:{weather.EpwPath}>"));

    // Gridded arm over Runtime/archive#HDF_ARCHIVE: the BRACKETED `Session` scope owns the handle on every outcome
    // arm, where the `Open`-plus-`using`-inside-a-lambda form released only on the arms the lambda reached. Each
    // irradiance variable reads as ONE rank-3 single-cell annual hyperslab `[hours, 1, 1]` at the DECLARED cell, so
    // a continent-scale corpus costs one column per variable. The /latitude and /longitude coordinate datasets
    // bound the declared cell and the /time attributes date the year — the netCDF convention gates here, above the
    // raw-HDF5 rail — and the cell bound, the calendar, and the epoch ACCUMULATE, so a corpus wrong on two axes
    // reports both rather than whichever guard the ladder reached first.
    static Fin<WeatherObservations> ReadGridded(WeatherSource.Gridded source, SolarSite site) =>
        HdfArchive.Session(new HdfSource.Path(source.Path), HdfArchivePolicy.Interchange, handle =>
            IO.lift(() =>
                (Bounds(handle, source), Epoch(handle), Span(handle))
                    .Apply(static (_, epoch, hours) => (Epoch: epoch, Hours: hours)).As().ToFin()
                    .Bind(dated => Column(handle, source, source.DniVar, dated.Hours)
                        .Bind(dni => Column(handle, source, source.DhiVar, dated.Hours)
                            .Bind(dhi => {
                                Instant epoch = dated.Epoch.Epoch.AtMidnight().WithOffset(site.Timezone).ToInstant();
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

    // Pooled scratch: an annual column is one rented buffer the caller releases, never a per-variable
    // `new double[hours]` the ingress allocates twice and abandons.
    static Fin<MemoryOwner<double>> Column(HdfHandle handle, WeatherSource.Gridded source, string variable, int hours) =>
        ReadKey.Catch(() => {
                MemoryOwner<double> column = MemoryOwner<double>.Allocate(hours, AllocationMode.Clear);
                handle.Dataset(variable).Read<double>(handle.Access, column.Span,
                    new HyperslabSelection(3, [0UL, (ulong)source.LatIndex, (ulong)source.LonIndex], [(ulong)hours, 1UL, 1UL]));
                return Fin.Succ(column);
            });
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class DaylightAnalysis {
    static readonly Op RunKey = Op.Of(name: nameof(Run));
    const string Unmeasured = "unmeasured";

    // `key` is the assessment content key the `Analysis/dispatch#DISPATCH_WRITEBACK` spine already derived. It
    // rides in rather than being re-derived because every typed row and every temporal point this run lands is
    // addressed BY it — the sink's own law — and a runner-side second derivation over a narrower preimage would
    // key the artifact, the series, and the assessment node three ways.
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Daylight request, GeometrySource geometry, AssessmentSink sink, ContentAddress key, IClock clock) =>
        // Both request columns admit at once and report together: a request carrying neither design days nor a
        // finite requirement once named whichever conjunct the `&&` reached first.
        from _ in (DesignDays(request), Requirement(request)).Apply(static (_, _) => unit).As().ToFin()
        from scene in DaylightScene.Of(graph, request, geometry)
        from weather in request.Weather.Match(
            Some: source => WeatherIngress.Read(source, request.Site).Map(static value => Some(value)),
            None: static () => Fin.Succ(Option<WeatherObservations>.None))
        // Site evidence is REQUIRED for any sun sweep: the EPW header supplies it under weather, the request's
        // explicit Site carries the geometry-only run and the gridded row, and absent both the run rails typed.
        from site in (weather.Map(static w => w.Site) | request.Site)
            .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.SiteAbsent, string.Empty))
        // The probed stride is derived ONCE and threaded: the matrix rows, the series instants, and the per-hour
        // plane vector are three views of one sample set, and the two independent `index % cadence == 0` walks
        // this replaces agreed only by coincidence — nothing made the Zip that pairs them well-founded.
        let probed = weather.Map(w => Probed(w.Hours, request.Policy))
        from findings in scene.Targets.TraverseM(target => Target(scene, target, site, probed, request)).As()
        // Ratio is required-over-achieved, so a target under a positive requirement that receives no sun divides
        // to a true infinity the verdict bands `exceeded`; the `double.MaxValue` stand-in it replaces was a
        // magnitude an operator reads as a measurement. An unrequired run contributes NO ratio at all.
        let govern = request.RequiredSunHours > 0.0
            ? findings.Fold(Option<double>.None, (worst, f) => Some(worst.Match(
                Some: held => Math.Max(held, request.RequiredSunHours / f.WorstDaySunHours),
                None: () => request.RequiredSunHours / f.WorstDaySunHours)))
            : Option<double>.None
        from perTarget in findings.TraverseM(f => AssessmentFact.Rows(
            AssessmentFact.Measure($"{f.Target.Value}/worst-day-sun-hours", Dimension.DurationDim, f.WorstDaySunHours * 3600.0),
            Shadow(f),
            AssessmentFact.Ratio($"{f.Target.Value}/sky-view-factor", f.SkyViewFactor))).As()
        from skyFacts in weather.Match(
            Some: w => findings.TraverseM(f => Irradiance(f)).As()
                // The served corpus cell rides the result beside the sky state, so a report reading the baked node
                // can say WHICH cell of a continent-scale corpus answered — the column the ingress resolved and
                // nothing read, while the EPW row's own file IS its site and carries none.
                .Map(perez => Seq(AssessmentFact.Text("sky-state", $"perez:{Dominant(w.Hours).Key}"))
                    + w.Cell.Map(static c => AssessmentFact.Text("weather-cell", $"{c.CorpusKey:x32}:{c.LatIndex}:{c.LonIndex}:{c.Years}")).ToSeq()
                    + perez),
            // Degrade stated inline on the result — never a silently-defaulted sky.
            None: () => Fin.Succ(Seq(AssessmentFact.Text("sky-state", "geometry-only"))))
        from matrix in probed.Match(
            Some: sky => Matrix(findings, sky, request, sink, key).Map(Some),
            None: static () => Fin.Succ(Option<BlobKey>.None))
        from result in AssessmentResult.Of(
            request.Route,
            perTarget.Bind(static rows => rows) + skyFacts,
            govern,
            clock.GetCurrentInstant(),
            RunKey,
            resultBlob: matrix)
        select result;

    static Validation<Error, Unit> DesignDays(AssessmentRequest.Daylight request) =>
        request.DesignDays.IsEmpty
            ? new ComputeFault.AssessmentInputMissing(AssessmentInputReason.DesignDaysEmpty, request.Route.Key)
            : unit;

    static Validation<Error, Unit> Requirement(AssessmentRequest.Daylight request) =>
        double.IsFinite(request.RequiredSunHours) && request.RequiredSunHours >= 0.0
            ? unit
            : new ComputeFault.AssessmentInputMissing(AssessmentInputReason.PolicyInvalid, $"{request.RequiredSunHours:R}");

    // The occluded share is EVIDENCE, not a number: a target with no above-horizon sample measured nothing, and
    // the `0.0` that stood there published FULLY LIT — the exact inverse of the conservatism the sun-hour
    // reduction takes for the same condition, and a reading a design review would act on.
    static Fin<AssessmentFact> Shadow(DaylightFinding finding) =>
        finding.MeanShadowFraction.Value().Match(
            Some: share => AssessmentFact.Ratio($"{finding.Target.Value}/mean-shadow-fraction", share),
            None: () => Fin.Succ(AssessmentFact.Text($"{finding.Target.Value}/mean-shadow-fraction", Unmeasured)));

    static Fin<AssessmentFact> Irradiance(DaylightFinding finding) =>
        finding.PerezDiffuseWm2.Value().Match(
            Some: wm2 => AssessmentFact.Measure($"{finding.Target.Value}/perez-diffuse-irradiance", Dimension.IrradianceDim, wm2),
            None: () => Fin.Succ(AssessmentFact.Text($"{finding.Target.Value}/perez-diffuse-irradiance", Unmeasured)));

    // THE probed stride, declared once. Index 0 always survives, so a non-empty hour stream yields a non-empty
    // probe set and every downstream divisor is structurally positive.
    static Seq<SkyState> Probed(Seq<SkyState> hours, DaylightPolicy policy) =>
        toSeq(Enumerable.Range(0, hours.Count).Where(index => index % policy.OcclusionCadenceHours == 0).Select(index => hours[index]));

    // Annual per-sensor irradiance MATRIX through the assessment sink: `[targets, probedHours]` chunked
    // TARGET-OUTERMOST — one chunk per target at the target's own ordinal — landing content-addressed through
    // sink.Store, with the probed instants and per-target hour rows also crossing sink.Series so the series lane
    // holds the same evidence in temporal form. The stored matrix is what the scalar mean cannot be: sDA/ASE-class
    // EN 17037 hour-threshold metrics are ONE reduction over its rows. The grid DERIVES through the one
    // `Runtime/archive#CHUNK_CURSOR` owner and the write rides the declared-session capsule, so the chunk shape is
    // stated once instead of at the slot and again at every write, and the ordinal the cursor holds makes an
    // out-of-order or repeated chunk a value no caller can spell.
    static Fin<BlobKey> Matrix(Seq<DaylightFinding> findings, Seq<SkyState> probed, AssessmentRequest.Daylight request, AssessmentSink sink, ContentAddress key) =>
        ChunkGrid.Derive([findings.Count], components: probed.Count, targetChunkElements: probed.Count).ToFin()
            .Bind(grid => {
                ArchiveSlot<double> slot = new("irradiance", grid);
                using MemoryStream staged = new();
                return ArchiveSession.Write(staged, HdfArchivePolicy.Interchange, Seq<IArchiveSlot>(slot),
                        Seq((MatrixTargets, (ArchiveAttribute)new ArchiveAttribute.Text(string.Join(' ', findings.Map(static f => f.Target.Value)))),
                            (MatrixCadence, new ArchiveAttribute.Whole(request.Policy.OcclusionCadenceHours)),
                            (MatrixHours, new ArchiveAttribute.Whole(probed.Count))),
                        session => IO.lift(() => session.Cursor(slot)
                            .Bind(cursor => findings.TraverseM(finding => cursor.Write(finding.HourlyPlaneWm2)).As())
                            .Map(static _ => unit)))
                    .Run()
                    .Bind(_ => sink.Store(staged.ToArray()).Run())
                    // Series identity IS the assessment content key — the sink's own law, so the heavy artifact
                    // and its queryable temporal projection share one origin — and the target rides the point's
                    // own facet path. The hand `ArrayBufferWriter` preimage this replaces minted a SECOND key
                    // space off the scene key, which no reader joining on the assessment could ever reach.
                    .Bind(blob => sink.Series(findings.Bind(finding => probed
                            .Zip(toSeq(finding.HourlyPlaneWm2))
                            .Map(pair => new SeriesPoint(key.Value, pair.First.At, pair.Second, Seq(finding.Target.Value)))))
                        .Run()
                        .Map(_ => blob));
            });

    const string MatrixTargets = "targets";
    const string MatrixCadence = "cadence-hours";
    const string MatrixHours = "hours";

    // Per-target fold over three independent ray families on the ONE clash BVH — the design-day sun sweep, the
    // cosine-weighted sky-view fan, and the hourly circumsolar gate; a failed occlusion probe rails the typed wire fault.
    static Fin<DaylightFinding> Target(DaylightScene scene, NodeId target, SolarSite site, Option<Seq<SkyState>> probed, AssessmentRequest.Daylight request) {
        Vector3 origin = scene.SamplePoints[target];
        return from sweep in DesignSweep(scene, origin, site, request)
               from skyView in SkyView(scene, origin, request.Policy)
               from hourly in probed.Match(
                   Some: sky => HourlyDiffuse(scene, origin, sky, skyView).Map(static measured => (Mean: Evidence.Of(Some(measured.Mean)), measured.PlaneWm2)),
                   // A geometry-only run measures no sky, so the diffuse column is ABSENT rather than a zero
                   // irradiance a report would chart beside a measured one.
                   None: static () => Fin.Succ((Mean: (Evidence<double>)new Evidence<double>.Absent(), PlaneWm2: System.Array.Empty<double>())))
               select new DaylightFinding(
                   target,
                   WorstDaySunHours(sweep, request.DesignDays, request.Policy.SunStepHours),
                   MeanShadowFraction(sweep),
                   skyView,
                   hourly.Mean,
                   hourly.PlaneWm2);
    }

    // Survey-frame ray in the clash engine's float coordinate — +Y north, +X east, altitude off the horizon — and
    // this page's ONE narrowing: the design-day sun and the cosine-weighted hemisphere fan span the SAME frame, so
    // both families read one projection and a per-site `(float)` cast is the forked-floor form. Kernel angles carry
    // double through the trig and shed their tail only at the traversal boundary, where the BVH's own float
    // triangle wire fixes the floor at `1.1e-3°` — a loss the ray engine owns and the almanac never inherits.
    static Vector3 SurveyRay(SunPosition sun) =>
        SurveyRay(sun.AzimuthDeg * Math.PI / 180.0, sun.AltitudeDeg * Math.PI / 180.0);

    static Vector3 SurveyRay(double azimuth, double altitude) =>
        new((float)(Math.Cos(altitude) * Math.Sin(azimuth)),
            (float)(Math.Cos(altitude) * Math.Cos(azimuth)),
            (float)Math.Sin(altitude));

    // Design-day sun sweep: the policy step walks each requested day from local midnight and every above-horizon
    // sample casts one occlusion ray along its own sun direction. The day tag rides each sample because the two
    // reductions below partition on it differently.
    static Fin<Seq<SunSample>> DesignSweep(DaylightScene scene, Vector3 origin, SolarSite site, AssessmentRequest.Daylight request) =>
        request.DesignDays
            .Bind(day => SolarPosition
                .SunPath(site, day.AtMidnight().WithOffset(site.Timezone).ToInstant(), Duration.FromHours(request.Policy.SunStepHours), request.Policy.SunSamplesPerDay)
                .Filter(static row => row.Sun.AboveHorizon)
                .Map(row => (Day: day, Ray: SurveyRay(row.Sun))))
            .TraverseM(row => ClashScale.Occluded(scene.Scene, origin, row.Ray, scene.SceneDiameter)
                .Map(occluded => new SunSample(row.Day, occluded)))
            .As();

    // Acceptance reduction takes the MINIMUM lit-hour total over the requested design days, walking the requested
    // days rather than the sample groups, so a day the sun never clears the horizon on contributes its honest
    // zero instead of vanishing from the extremum and lifting the worst case. The lit tally INDEXES by day, where
    // a per-day rescan of the whole sweep cost days x samples for a fact each sample already carries.
    static double WorstDaySunHours(Seq<SunSample> sweep, Seq<LocalDate> days, double stepHours) {
        HashMap<LocalDate, int> lit = sweep.Filter(static sample => !sample.Occluded)
            .Fold(HashMap<LocalDate, int>(), static (tally, sample) => tally.AddOrUpdate(sample.Day, static held => held + 1, 1));
        return days.Map(day => lit.Find(day).IfNone(0) * stepHours)
            .Fold(double.PositiveInfinity, static (worst, hours) => Math.Min(worst, hours));
    }

    // Reporting reduction pools the occluded share of every above-horizon sample across all design days — and
    // reports ABSENCE where the sweep pooled nothing, because a target whose sun never clears the horizon on any
    // requested day measured no shadow at all, while the `0.0` this replaces published a fully lit target.
    static Evidence<double> MeanShadowFraction(Seq<SunSample> sweep) =>
        Evidence.Of(sweep.IsEmpty
            ? Option<double>.None
            : Some(sweep.Count(static sample => sample.Occluded) / (double)sweep.Count));

    // Cosine-weighted hemisphere rays span azimuth/altitude, each weighted `sin(alt)·cos(alt)` so the
    // zenith patch and the horizon band contribute their solid-angle-projected share; SVF = unoccluded weight
    // fraction. The fan samples patch CENTRES, so no ray sits on a pole and every weight is strictly positive
    // under the policy's own positive-count admission — the total is structurally non-zero and the guarded
    // `: 0.0` arm it replaces was a forged zero on an unreachable branch.
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

    // Mean reference-plane diffuse over the PROBED sky hours: each hour casts ITS OWN circumsolar ray, so the disc
    // term enters exactly at the hours whose sun this target can see. The mean divides by what was probed, so
    // cadence changes fidelity, never scale. The per-hour PLANE vector returns beside the mean — global horizontal
    // at the reference plane, diffuse plus the unoccluded direct projection — because the matrix artifact rows it
    // verbatim, and BOTH read the one probed stride the caller derived rather than each re-striding the year.
    // The probe set is non-empty by construction (a retained hour stream always keeps index zero), so the mean's
    // divisor needs no guard and the empty-check forging a zero mean is gone with the second stride.
    static Fin<(double Mean, double[] PlaneWm2)> HourlyDiffuse(DaylightScene scene, Vector3 origin, Seq<SkyState> probed, double skyView) =>
        probed
            .TraverseM(hour => ClashScale.Occluded(scene.Scene, origin, SurveyRay(hour.Sun), scene.SceneDiameter)
                .Map(occluded => {
                    double diffuse = Diffuse(hour, skyView, occluded);
                    double direct = occluded ? 0.0 : hour.DirectNormalWm2 * Math.Max(0.0, Math.Cos(hour.Sun.ZenithDeg * Math.PI / 180.0));
                    return (Diffuse: diffuse, Plane: diffuse + direct);
                }))
            .As()
            .Map(static terms => (
                Mean: terms.Map(static term => term.Diffuse).Sum() / terms.Count,
                PlaneWm2: terms.Map(static term => term.Plane).ToArray()));

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
            .Bind(points => Diameter(request.Scene.Index)
                .Filter(_ => request.Scene.Key != UInt128.Zero && !request.Scene.Triangles.IsEmpty)
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<daylight-obstruction-scene-invalid>"))
                // ONE clash admission per assessment: the scene admits here and every sun/sky ray reads it.
                .Bind(diameter => AdmittedScene.Of(request.Scene.Index, request.Scene.Triangles, ClashPolicy.Canonical)
                    .Map(admitted => new DaylightScene(
                        request.Targets,
                        points.Fold(Map<NodeId, Vector3>(), static (acc, point) => acc.Add(point.Id, point.Point)),
                        admitted,
                        diameter))));

    // ONE centroid definition across all three components: the area-weighted triangle fan about the ring's FIRST
    // vertex, so every axis carries an apex sitting on the footprint's own plane. An ORIGIN apex stays exact on
    // whichever two axes its cross product spans and reads a storey slab at two thirds of its elevation on the
    // third, while a vertex mean re-weights a dense-tessellated edge and slides the reference plane off the area
    // centre. Fan coordinates ride the seam double triple and the clash frame rides float, so the fold closes
    // whole in double and narrows once on the way out.
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
        // The degeneracy gate is the kernel Identity lane — the "is this exactly zero" question the twice-signed-area
        // sum asks — and its derivation is context-free, so the read carries no model-scale claim the ring's own
        // frame would have to justify. The page literal it replaces was a magnitude in no stated unit.
        return Math.Abs(sum.Cross) > Context.Canonical.For(ToleranceLane.Identity).Value
            ? Fin.Succ(new Vector3(
                (float)(sum.X / (3.0 * sum.Cross)),
                (float)(sum.Y / (3.0 * sum.Cross)),
                (float)((sum.Z / (3.0 * sum.Cross)) + SampleLiftM)))
            : Fin.Fail<Vector3>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<daylight-target-degenerate-footprint>"));
    }

    // Root AABB occupies the wire's first bounds slot (min xyz, max xyz) on both decoded kinds; its diagonal is the
    // scene diameter every occlusion ray reaches across. A wire too short to hold the slot, or a slot whose corners
    // coincide, describes NO scene — so the read is absent rather than a `0f` the caller then had to re-test, which
    // made one refusal two facts that could disagree.
    static Option<float> Diameter(AccelerationStructure index) =>
        index.Bounds.Span is { Length: >= 6 } bounds
            ? Some(MathF.Sqrt(
                ((bounds[3] - bounds[0]) * (bounds[3] - bounds[0]))
                + ((bounds[4] - bounds[1]) * (bounds[4] - bounds[1]))
                + ((bounds[5] - bounds[2]) * (bounds[5] - bounds[2])))).Filter(static diagonal => diagonal > 0f)
            : None;
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
