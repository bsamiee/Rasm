# [COMPUTE_ENERGY]

Rasm.Compute owns the whole-building energy-simulation runner — the `Discipline.Energy` arm of the assessment pipeline. It builds an `NREL.OpenStudio` `Model` in-process from the concrete `Rasm.Element` `ElementGraph` (spaces, bounding surfaces, layered opaque constructions, thermal zones), stamps the annual EnergyPlus run context (weather-file `SimulationControl`, a full-year `RunPeriod`, the attached `EpwFile`), conditions each occupied zone with ideal-air loads driven to policy dual setpoints and policy lighting/equipment gains so demand is the building-envelope-driven load, forward-translates to an IDF through `EnergyPlusForwardTranslator`, runs EnergyPlus as a subprocess over a parameterized binary-discovery boundary, and reads the result SQLite through `SqlFile`. A run publishes TWO outcomes off that one read: the typed result rows carrying every magnitude on its own `(measure × fuel × end-use × scope)` axis point, and the summary `AssessmentResult` fact stream the assessment spine bands its EUI verdict from. Execution is one route axis — the `EnergyRoute` `[Union]` dispatches a local EnergyPlus subprocess (default row) or a Pollination cloud run — one entry, one `SqlFile` result read, provider variance as row data.

OpenStudio (the SWIG SDK) builds the model and reads the results; it neither runs nor bundles the EnergyPlus binary, and its version fixes the EnergyPlus the toolchain must resolve. Every wrapper owns a native handle and is `IDisposable`, model mutation is single-threaded, and every load/get that can miss returns a SWIG `Optional<T>` lowered onto `Fin<T>`/`Option<T>` at the boundary. Compute admits `NREL.OpenStudio.macOS-arm64` for simulation, distinct from the `Rasm.Bim/Energy/exchange` exchange owner. Cloud runs consume the app-staged HBJSON model; its selected `eplusout.sql` result lands content-keyed through `AssessmentSink`, while unselected downloaded assets stay inside the leased scratch directory.

Result vocabulary is TRANSCRIBED, not referenced: Compute holds no `Rasm.Bim` project reference, so the `Rasm.Bim/Energy/results#RESULTS_ADMISSION` axes cross as this page's own `[SmartEnum<string>]` rosters whose keys are the Bim owner's spellings verbatim. `ResultFuel` × `ResultEndUse` are the two axes a magnitude is a point on, `ResultMeasure` carries the physics (`Annual`/`Peak`/`Intensity`/`UnmetHours`/`ComfortHours`), and `ResultScope` names the granularity (`Building`/`Zone`/`Space`). Every published magnitude lowers into the neutral `Analysis/dispatch#DISPATCH_WRITEBACK` `AssessmentRow`, whose ordered facet path IS the axis point, so the Bim admission reads the run's rows off the assessment content key with no parse, no shared store, and no strata edge this package's manifest does not carry.

## [01]-[INDEX]

- [02]-[TOOLCHAIN_BOUNDARY]: `EnergyToolchain` resolves the EnergyPlus binary under a version lock over the `VersionProbe` union, and `EnergyPolicy` carries the simulation scenario.
- [03]-[MODEL_BUILD]: `EnergySimulation.BuildModel` folds the graph into an in-process `OpenStudio.Model`, translates it to IDF across the SWIG boundary bindings, and mints the run's model key and zone roster.
- [04]-[SIMULATION_RUN]: `EnergySimulation.RunLocal` drives the EnergyPlus subprocess over the leased scratch capsule and folds `eplusout.sql` into the typed result rows beside the summary fact stream.
- [05]-[CLOUD_ROUTE]: `EnergyRoute` selects the local subprocess or the Pollination arm under its published re-drive curve, both converging on the one `ReadResults` fold.
- [06]-[GRAPH_READS]: `BoundaryReads` is the projected space-boundary edge owner both this runner and the circulation runner compose over `AnalysisReads`.

## [02]-[TOOLCHAIN_BOUNDARY]

- Owner: `EnergyToolchain` the static EnergyPlus-executable resolver; `SolverPin` the `<tool>-<version>` route-token grammar admitted once; `VersionProbe` the three-outcome self-report union; `EnergyToolchainPolicy` the discovery policy (configured-directory override, platform executable name, expected-version datum read off the route); `EnergyPolicy` the simulation scenario the `AssessmentRequest.Energy` case carries (the `EnergyRoute` row, the toolchain, the optional EUI target, the dual setpoints, the lighting/equipment densities the model build reads); the version-lock guard.
- Entry: `public static Fin<string> Resolve(EnergyToolchainPolicy policy)` probes candidate paths in priority `ENERGYPLUS_EXE` → `OPENSTUDIO_ENERGYPLUSDIR` (a full OpenStudio installation's bundled binary, dev/CI) → the policy configured directory → the app's RID-native `runtimes/<rid>/native` fallback, returns the first existing executable, and fails `ComputeFault.ToolchainUnresolved` with the full probe trail when none resolves — discovery parameterized end to end, never a hardcoded path.
- Auto: `VersionGate` checks the binary's self-reported `energyplus --version` banner against the policy expected version BEFORE any model build or subprocess launch (the binary is the version authority, never its path) — a REPORTED mismatch fails `ToolchainUnresolved`, so a version-skewed binary never consumes the translated IDF and never produces a run result; only an UNDETERMINED probe (a launch failure, an empty banner) degrades to a warning fact riding the result, so an air-gapped or sandboxed probe stays runnable while a real skew gates.
- Law: a version has THREE outcomes and they ride three cases, never one string. `VersionProbe` is `Reported(banner)` | `Unreported` | `Failed(Error)`, so the gate SWITCHES on the outcome the probe already determined rather than re-parsing a `"<version-…>"` marker it minted itself two frames earlier — a channel where a real banner that happened to contain the marker prefix read as a failure, and where the probe's own exception message was discarded before anyone could read it.
- Law: the version comparison is EXACT over the parsed segment, never a substring of the raw banner. `"25.1"` is a substring of `"25.10"`, so a substring test admits a solver one minor release ahead of the IDF it is about to consume — the same defect the cloud arm's terminal gate already names and fixes by parsing before comparing.
- Packages: LanguageExt.Core, NREL.OpenStudio.macOS-arm64 (the SWIG SDK whose version the toolchain locks EnergyPlus to — it bundles no solver, and the resolver touches no OpenStudio API), Thinktecture.Runtime.Extensions (`[ValueObject<string>]`, `[Union]`), Rasm (kernel — `Op`), BCL inbox (`Environment`/`Path`/`File`/`AppContext`/`RuntimeInformation` for the probes, `System.Diagnostics.Process` for the `--version` self-report).
- Growth: a new discovery source is one probe in the chain; a new platform the executable-name column; a new simulation knob (ventilation rate, infiltration default, sized HVAC plant selector) one `EnergyPolicy` column; a new execution provider one `EnergyRoute` case on `[05]` — resolver widens by probe, scenario by column, provider by row, never a parallel discovery method per host.
- Boundary: a shipped app owns its EnergyPlus provisioning (a bundled RID-native binary or `ENERGYPLUS_EXE`), so the last-resort probe resolves the app's own `runtimes/<rid>/native` location off `RuntimeInformation.RuntimeIdentifier` and never assumes a developer machine or a literal RID. Version-lock is load-bearing: OpenStudio forward-translates an IDF only the version-matched EnergyPlus consumes, so a dev box points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-bundled solver, not a mismatched standalone; the resolver applies no version filter, so a mismatched binary IS selected — and `VersionGate` then REFUSES it before the run, the expected-version policy governing execution rather than annotating it. The expected version is DERIVED, never declared here: the `assessment#ROUTE_AXIS` `AssessmentRoute.EnergyPlus` `SolverVersion` pin is its one owner and `SolverPin` admits that token's `<tool>-<version>` grammar so a route whose pin lacks the tool prefix refuses instead of yielding a silently sliced garbage version. Because the value derives, it does NOT fold the assessment content key — `assessment#REQUEST_FAMILY` states that law at the key's own owner, and folding a derived spelling beside its source keys one fact twice. Conditioning and internal-load defaults are explicit `EnergyPolicy` knobs, never ambient constants, so a consumer re-targets a climate or building type without an interior edit; an unresolved binary fails `ToolchainUnresolved`, never a default that fails opaquely.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
public sealed partial class SolverPin {
    public const string EnergyPlusPrefix = "energyplus-";

    public string Version => ToValue()[EnergyPlusPrefix.Length..];

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.StartsWith(EnergyPlusPrefix, StringComparison.Ordinal) && value.Length > EnergyPlusPrefix.Length
            ? null
            : new ValidationError(message: $"<solver-pin-grammar:{value}>");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VersionProbe {
    private VersionProbe() { }

    public sealed record Reported(string Banner) : VersionProbe;
    public sealed record Unreported : VersionProbe;
    public sealed record Failed(Error Cause) : VersionProbe;

    public static readonly VersionProbe Silent = new Unreported();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EnergyToolchainPolicy(Option<string> ConfiguredDir, string ExecutableName, SolverPin Pin) {
    public static readonly EnergyToolchainPolicy Canonical = new(
        ConfiguredDir: None,
        ExecutableName: OperatingSystem.IsWindows() ? "energyplus.exe" : "energyplus",
        Pin: SolverPin.Create(AssessmentRoute.EnergyPlus.SolverVersion));
}

public sealed record WeatherRef(string EpwPath, string Station);

public sealed record EnergyPolicy(
    EnergyRoute Route, EnergyToolchainPolicy Toolchain, Option<double> TargetEui,
    double HeatingSetpointC, double CoolingSetpointC, double LightingPowerWM2, double EquipmentPowerWM2) {
    public static readonly EnergyPolicy Canonical = new(
        EnergyRoute.Local, EnergyToolchainPolicy.Canonical, TargetEui: None,
        HeatingSetpointC: 20.0, CoolingSetpointC: 26.0, LightingPowerWM2: 8.0, EquipmentPowerWM2: 10.0);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EnergyToolchain {
    public static Fin<string> Resolve(EnergyToolchainPolicy policy) =>
        (Probe(Environment.GetEnvironmentVariable("ENERGYPLUS_EXE"))
         | Probe(Join(Environment.GetEnvironmentVariable("OPENSTUDIO_ENERGYPLUSDIR"), policy.ExecutableName))
         | policy.ConfiguredDir.Bind(dir => Probe(Join(dir, policy.ExecutableName)))
         | Probe(Join(BundledRuntimeDir(), policy.ExecutableName)))
            .ToFin(new ComputeFault.ToolchainUnresolved(
                $"<energyplus-not-found:ENERGYPLUS_EXE->OPENSTUDIO_ENERGYPLUSDIR->configured({policy.ConfiguredDir})->bundled>"));

    public static Fin<Seq<AssessmentFact>> VersionGate(string executable, EnergyToolchainPolicy policy) =>
        ProbeVersion(executable).Switch(
            reported: banner => Banner(banner.Banner).Exists(token => StringComparer.Ordinal.Equals(token, policy.Pin.Version))
                ? Fin.Succ(Seq<AssessmentFact>())
                : Fin.Fail<Seq<AssessmentFact>>(new ComputeFault.ToolchainUnresolved(
                    $"<energyplus-version-mismatch:expected={policy.Pin.Version}:reported={banner.Banner}:{executable}>")),
            unreported: _ => Fin.Succ(Seq(AssessmentFact.Text(VersionFact, $"<unreported:{executable}>"))),
            failed: probe => Fin.Succ(Seq(AssessmentFact.Text(VersionFact, $"<probe-failed:{probe.Cause.Message}:{executable}>"))));

    public const string VersionFact = "energyplus-version-warning";

    static Option<string> Banner(string banner) =>
        toSeq(banner.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .SkipWhile(static token => !StringComparer.OrdinalIgnoreCase.Equals(token, "Version"))
            .Skip(1).Head
            .Map(static token => token.Split('-', 2)[0]);

    static VersionProbe ProbeVersion(string executable) =>
        Try.lift(() => {
            using Process probe = new() {
                StartInfo = new ProcessStartInfo(executable) {
                    ArgumentList = { "--version" }, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
                },
            };
            probe.Start();
            Task<string> stderrDrain = probe.StandardError.ReadToEndAsync();
            string banner = probe.StandardOutput.ReadToEnd().Trim();
            probe.WaitForExit();
            _ = stderrDrain.GetAwaiter().GetResult();
            return Fin.Succ(banner.Length > 0 ? (VersionProbe)new VersionProbe.Reported(banner) : VersionProbe.Silent);
        }).Run().Bind(static inner => inner).Match(Succ: static probe => probe, Fail: static cause => new VersionProbe.Failed(cause));

    static Option<string> Probe(string? path) => path is not null && File.Exists(path) ? Some(path) : None;
    static string? Join(string? dir, string exe) => dir is null ? null : Path.Combine(dir, exe);

    static string BundledRuntimeDir() =>
        Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier, "native", "EnergyPlus");
}
```

## [03]-[MODEL_BUILD]

- Owner: `EnergySimulation.BuildModel` the in-process OpenStudio model builder; `OsmBuild` the build result (IDF path, the run's model content key, the `ZoneTarget` roster); `ZoneTarget` the per-zone result address and `ResultScope` the closed granularity union the zone roster decides; `ReadLog`/`Reads` the ONE fidelity ledger every note-bearing fold on this page reports through; `OpeningType` the closed sub-surface roster the foreign opening taxonomy admits through; `ConditioningClaim` the three-state occupancy claim; `GlazingWire` the Mapperly optical correspondence; `ConfigureRun`/`SetpointSchedules`/`InternalLoads`/`Condition`/`BuildSurface`/`BuildOpenings`/`BuildConstruction`/`Layer`/`Vertices` the model-object folds; the SWIG `Optional<T>`→`Fin<T>`, `IDisposable`, and `Path` boundary discipline.
- Entry: `static WriterT<ReadLog, Fin, OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch)` guards the weather EPW, builds a `Model`, stamps the annual context, folds each spatial node into a named `Space`+`ThermalZone` pair, each bounding surface into a `Surface` with its resolved footprint and layered `Construction`, each `Host`-attributed opening into a typed `SubSurface`, forward-translates to the IDF `Workspace`, and mints the saved IDF's content key, the result lowering a missing weather/composition onto `AssessmentInputMissing` and a translator error onto `AnalysisFailed`.
- Law: the fidelity ledger is ONE channel. Translator errors and warnings, per-surface skips, and the version-probe verdict all ride the `WriterT<ReadLog, Fin, A>` writer the aggregator's ply ledger already proved on this branch — four hand-threaded note channels, one of which THREW ITS CONTENT AWAY on the failure arm and handed the caller a count where the messages sat three lines above it.
- Auto: every OpenStudio file API takes a SWIG `Path` (no `Path(string)` ctor), so paths route through `OpenStudioUtilitiesCore.toPath`; the unique `SimulationControl`/`RunPeriod` objects are gotten-or-created through the static `OpenStudioModelSimulation.get*(model)` module functions (neither carries a `(Model)` ctor, and the binding surfaces these as module functions, not `Model` instance methods); the construction fold discriminates on the contract property case — an all-`Optical` set builds `StandardGlazing` layers, any other builds `StandardOpaqueMaterial` through the 6-arg ctor (the shorter ctor forms backfill OpenStudio defaults for the omitted thermal columns — fabricated physics, the rejected admission) so the OSM U-value matches the `Analysis/aggregator` ISO 6946 fold, while absent or mixed compositions fail typed; every load/get that can miss returns a SWIG `Optional<T>` checked with `is_initialized()` before `get()`.
- Packages: NREL.OpenStudio.macOS-arm64, LanguageExt.Core (`Fin`/`Seq`/`Option`/`WriterT`/`Monoid`/`TraverseM`), Riok.Mapperly (the existing-target optical correspondence), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), NodaTime, Rasm (kernel — `ContentHash.Of` the model address, `Op`), Rasm.Element (project — `ElementGraph`, `Node.Object`, `MaterialComposition`, `MaterialPropertySet.Thermal`/`.Mechanical`/`.Optical` via `MaterialPropertyAccess`, `MaterialLayer`, `NodeId`, `FootprintPolygon`, the host-neutral `Vector3` its ring carries, `GeometrySource` the analytical-surface resolution port), BCL inbox.
- Growth: a new model object (HVAC plant, schedule set, daylighting control, infiltration object) is one fold over the matching nodes; conditioning widens from ideal-air to a sized plant by one `EnergyPolicy` selector; `SimpleGlazing` is the assembly-shorthand row one `Layer` arm adds when a whole-window U/SHGC case rides the contract; a new opening class is one `OpeningType` row — the build widens by fold, roster row, and policy column, never a parallel builder per object type.
- Boundary: the model is built from the contract graph for SIMULATION, distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange — Compute reads the graph's already-lowered spaces/surfaces/constructions, never re-authored from IFC, and holds no `Rasm.Bim` reference to do it with. A bounding-surface node carries its OWN `MaterialComposition.LayerSet`, so `BuildSurface` reads the surface node directly and NEVER joins to the bounded wall or slab; a join would read the host element's full assembly where the boundary carries the space-facing composition the simulation needs. Every OpenStudio wrapper is `IDisposable` and bracketed under `using` (the `Model`, translator, `Workspace`, `EpwFile`, every point/vector/log-vector, the result optionals) — a dropped handle leaks native memory the GC cannot reclaim; a model-object is owned BY the `Model` it is `new`-ed against and never independently disposed; model mutation is single-threaded so the build is one serialized unit, never a parallel fan-out; the `*PINVOKE` marshaling classes are never a call surface. Absent, non-layered, and mixed compositions fail `AssessmentInputMissing`; an OpenStudio default fabricates building-envelope conductance. Fenestration constructions land only on `SubSurface` openings — EnergyPlus rejects one on a base surface, which is why `BoundaryReads.SurfacesOf` excludes the `Host`-attributed opening boundaries — and the sub-surface type is a ROSTER row over the foreign opening taxonomy with its host-tilt column, never a nested ternary over four bare strings. One `ThermalZone` per space makes the zone roster the SPACE roster, so a zone's results address the space's `GlobalId` wherever the graph carries one and fall back to the authored zone name otherwise — the finest identity the graph supports, never both scopes for one physical row. The model address is the CONTENT KEY of the bytes actually written: the saved IDF is the producer's own octets, which is the only run a key may claim.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpeningType {
    public static readonly OpeningType FixedWindow = new("FixedWindow", classCode: "IfcWindow", roofForm: "Skylight");
    public static readonly OpeningType Door = new("Door", classCode: "IfcDoor", roofForm: "Door");

    public string ClassCode { get; }

    public string RoofForm { get; }

    public const string RoofHost = "RoofCeiling";

    public static OpeningType Of(string classCode) =>
        toSeq(Items).Find(row => StringComparer.Ordinal.Equals(row.ClassCode, classCode)).IfNone(Door);

    public string On(string hostSurfaceType) =>
        StringComparer.Ordinal.Equals(hostSurfaceType, RoofHost) ? RoofForm : Key;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConditioningClaim {
    public static readonly ConditioningClaim Conditioned = new("conditioned", conditions: true);
    public static readonly ConditioningClaim External = new("external", conditions: false);
    public static readonly ConditioningClaim Unstated = new("unstated", conditions: true);

    public bool Conditions { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResultScope {
    private ResultScope() { }

    public sealed record Building : ResultScope;
    public sealed record Zone(string ZoneName) : ResultScope;
    public sealed record Space(string GlobalId) : ResultScope;

    public static readonly ResultScope Whole = new Building();

    public Seq<string> Facets => Switch(
        building: static _ => Seq("building", string.Empty),
        zone: static z => Seq("zone", z.ZoneName),
        space: static s => Seq("space", s.GlobalId));
}

public readonly record struct ReadLog(Seq<AssessmentFact> Facts) : Monoid<ReadLog> {
    public static ReadLog Empty => new(Seq<AssessmentFact>());
    public static ReadLog Of(AssessmentFact fact) => new(Seq(fact));
    public static ReadLog Of(Seq<AssessmentFact> facts) => new(facts);
    public ReadLog Combine(ReadLog rhs) => new(Facts + rhs.Facts);
}

public static class Reads {
    public static WriterT<ReadLog, Fin, A> Held<A>(A value) => WriterT.pure<ReadLog, Fin, A>(value);
    public static WriterT<ReadLog, Fin, A> Noting<A>(AssessmentFact fact, A value) => WriterT.write<ReadLog, Fin, A>(value, ReadLog.Of(fact));
    public static WriterT<ReadLog, Fin, A> Writing<A>(Seq<AssessmentFact> facts, A value) => WriterT.write<ReadLog, Fin, A>(value, ReadLog.Of(facts));
    public static WriterT<ReadLog, Fin, A> Lift<A>(Fin<A> result) => WriterT.lift<ReadLog, Fin, A>(result);
    public static Fin<(A Value, ReadLog Log)> Run<A>(WriterT<ReadLog, Fin, A> writer) => writer.Run().As();
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ZoneTarget(string Row, ResultScope Scope);

public sealed record OsmBuild(string IdfPath, UInt128 Model, Seq<ZoneTarget> Zones);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class GlazingWire {
    [MapProperty(nameof(MaterialPropertySet.Optical.SolarTransmittance), nameof(OpenStudio.StandardGlazing.SolarTransmittanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.SolarReflectanceFront), nameof(OpenStudio.StandardGlazing.FrontSideSolarReflectanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.SolarReflectanceBack), nameof(OpenStudio.StandardGlazing.BackSideSolarReflectanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.VisibleTransmittance), nameof(OpenStudio.StandardGlazing.VisibleTransmittanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.VisibleReflectanceFront), nameof(OpenStudio.StandardGlazing.FrontSideVisibleReflectanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.VisibleReflectanceBack), nameof(OpenStudio.StandardGlazing.BackSideVisibleReflectanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.ThermalIrTransmittance), nameof(OpenStudio.StandardGlazing.InfraredTransmittanceatNormalIncidence))]
    [MapProperty(nameof(MaterialPropertySet.Optical.ThermalIrEmissivityFront), nameof(OpenStudio.StandardGlazing.FrontSideInfraredHemisphericalEmissivity))]
    [MapProperty(nameof(MaterialPropertySet.Optical.ThermalIrEmissivityBack), nameof(OpenStudio.StandardGlazing.BackSideInfraredHemisphericalEmissivity))]
    public static partial void Update(MaterialPropertySet.Optical optical, [MappingTarget] OpenStudio.StandardGlazing glass);
}

public static partial class EnergySimulation {
    static readonly Dimension EnergyDim = Dimension.ForceDim.Multiply(Dimension.LengthDim);
    static readonly Dimension EuiDim = EnergyDim.Divide(Dimension.AreaDim);
    static double Joules(double gigajoules) => UnitsNet.Energy.FromGigajoules(gigajoules).Joules;

    static WriterT<ReadLog, Fin, OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch) {
        if (!File.Exists(request.Weather.EpwPath)) {
            return Reads.Lift(Fin.Fail<OsmBuild>(Missing(AssessmentInputReason.MeasureAbsent, request.Weather.EpwPath)));
        }
        using OpenStudio.Model model = new();
        return from _ in Reads.Lift(ConfigureRun(model, request))
               let spaceType = InternalLoads(model, request.Policy)
               let setpoints = SetpointSchedules(model, request.Policy)
               from zones in graph.SpacesOf(request.Targets)
                   .TraverseM(space => Zone(model, graph, geometry, space, spaceType, setpoints)).As()
               from build in Translate(model, scratch)
               select build with { Zones = zones };
    }

    static WriterT<ReadLog, Fin, ZoneTarget> Zone(
        OpenStudio.Model model, ElementGraph graph, GeometrySource geometry, Node.Object space,
        OpenStudio.SpaceType spaceType, (OpenStudio.ScheduleConstant Heating, OpenStudio.ScheduleConstant Cooling) setpoints) {
        OpenStudio.Space osSpace = new(model);
        osSpace.setName(space.Name);
        osSpace.setSpaceType(spaceType);
        OpenStudio.ThermalZone zone = new(model);
        osSpace.setThermalZone(zone);
        zone.setName($"{ZonePrefix}{space.Id.ToValue()}");
        ZoneTarget target = new(
            zone.nameString().ToUpperInvariant(),
            space.ExternalId.Match(
                Some: static gid => (ResultScope)new ResultScope.Space(gid),
                None: () => new ResultScope.Zone(zone.nameString())));
        ConditioningClaim claim = graph.ConditioningOf(space.Id);
        if (claim.Conditions) { Condition(model, zone, setpoints.Heating, setpoints.Cooling); }
        return from _ in Reads.Noting(AssessmentFact.Text($"{space.Id.ToValue()}/conditioning", claim.Key), unit)
               from surfaces in graph.SurfacesOf(space.Id)
                   .TraverseM(surface => BuildSurface(model, osSpace, space.Id, surface, graph, geometry)).As()
               select target;
    }

    static WriterT<ReadLog, Fin, OsmBuild> Translate(OpenStudio.Model model, string scratch) {
        using OpenStudio.EnergyPlusForwardTranslator translator = new();
        using OpenStudio.Workspace idf = translator.translateModel(model);
        using OpenStudio.LogMessageVector errors = translator.errors();
        using OpenStudio.LogMessageVector warnings = translator.warnings();
        Seq<AssessmentFact> log = Messages("osm-error", errors) + Messages("osm-warning", warnings);
        return from _ in Reads.Writing(log, unit)
               from admitted in Reads.Lift(errors.Count > 0
                   ? Fin.Fail<Unit>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<osm-forward-translate-errors:{errors.Count}>"))
                   : Fin.Succ(unit))
               let idfPath = Path.Combine(scratch, "in.idf")
               from saved in Reads.Lift(Save(idf, idfPath))
               select new OsmBuild(idfPath, saved, Seq<ZoneTarget>());
    }

    static Fin<UInt128> Save(OpenStudio.Workspace idf, string idfPath) {
        using OpenStudio.Path outPath = OpenStudio.OpenStudioUtilitiesCore.toPath(idfPath);
        return idf.save(outPath, overwrite: true)
            ? Fin.Succ(ContentHash.Of(File.ReadAllBytes(idfPath)))
            : Fin.Fail<UInt128>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Foreign, $"<osm-idf-save-failed:{idfPath}>"));
    }

    static Seq<AssessmentFact> Messages(string prefix, OpenStudio.LogMessageVector messages) =>
        toSeq(Enumerable.Range(0, checked((int)messages.Count)))
            .Map(i => AssessmentFact.Text($"{prefix}-{i}", messages[i].logMessage()));

    const string ZonePrefix = "rasm-zone-";

    static Fin<Unit> ConfigureRun(OpenStudio.Model model, AssessmentRequest.Energy request) {
        OpenStudio.SimulationControl control = OpenStudio.OpenStudioModelSimulation.getSimulationControl(model);
        control.setRunSimulationforWeatherFileRunPeriods(true);
        control.setSolarDistribution(SolarDistribution);
        OpenStudio.RunPeriod run = OpenStudio.OpenStudioModelSimulation.getRunPeriod(model);
        run.setBeginMonth(1); run.setBeginDayOfMonth(1); run.setEndMonth(12); run.setEndDayOfMonth(31);
        OpenStudio.OpenStudioModelSimulation.getOutputTableSummaryReports(model).addSummaryReport(SummaryReport);
        using OpenStudio.Path epwPath = OpenStudio.OpenStudioUtilitiesCore.toPath(request.Weather.EpwPath);
        using OpenStudio.EpwFile epw = new(epwPath);
        using OpenStudio.OptionalWeatherFile attached = OpenStudio.WeatherFile.setWeatherFile(model, epw);
        return attached.is_initialized()
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Missing(AssessmentInputReason.MeasureAbsent, request.Weather.EpwPath));
    }

    const string SolarDistribution = "FullExterior";
    const string SummaryReport = "AllSummary";
    const string HeatingScheduleName = "rasm-heating-setpoint";
    const string CoolingScheduleName = "rasm-cooling-setpoint";
    const string SpaceTypeName = "rasm-space-type";
    const string GlazingSpectral = "SpectralAverage";
    const string OpaqueRoughness = "MediumRough";

    static (OpenStudio.ScheduleConstant Heating, OpenStudio.ScheduleConstant Cooling) SetpointSchedules(OpenStudio.Model model, EnergyPolicy policy) {
        OpenStudio.ScheduleConstant heating = new(model);
        heating.setName(HeatingScheduleName);
        heating.setValue(policy.HeatingSetpointC);
        OpenStudio.ScheduleConstant cooling = new(model);
        cooling.setName(CoolingScheduleName);
        cooling.setValue(policy.CoolingSetpointC);
        return (heating, cooling);
    }

    static void Condition(OpenStudio.Model model, OpenStudio.ThermalZone zone, OpenStudio.ScheduleConstant heating, OpenStudio.ScheduleConstant cooling) {
        zone.setUseIdealAirLoads(true);
        OpenStudio.ThermostatSetpointDualSetpoint thermostat = new(model);
        thermostat.setHeatingSetpointTemperatureSchedule(heating);
        thermostat.setCoolingSetpointTemperatureSchedule(cooling);
        zone.setThermostatSetpointDualSetpoint(thermostat);
    }

    static OpenStudio.SpaceType InternalLoads(OpenStudio.Model model, EnergyPolicy policy) {
        OpenStudio.SpaceType spaceType = new(model);
        spaceType.setName(SpaceTypeName);
        spaceType.setLightingPowerPerFloorArea(policy.LightingPowerWM2);
        spaceType.setElectricEquipmentPowerPerFloorArea(policy.EquipmentPowerWM2);
        return spaceType;
    }

    static WriterT<ReadLog, Fin, Unit> BuildSurface(OpenStudio.Model model, OpenStudio.Space space, NodeId spaceId, Node.Object surface, ElementGraph graph, GeometrySource geometry) =>
        from footprint in Reads.Lift(geometry.Footprint(surface.Representations)
            .ToFin(Missing(AssessmentInputReason.MeasureAbsent, surface.Id.ToValue())))
        let osSurface = Seated(model, space, footprint)
        from construction in Reads.Lift(graph.LayerSetOf(surface.Id).Bind(set => BuildConstruction(model, set, graph)))
        let _ = Bind(osSurface, construction)
        from openings in BuildOpenings(model, osSurface, spaceId, surface.ExternalId.IfNone(surface.Name), graph, geometry)
        select unit;

    static OpenStudio.Surface Seated(OpenStudio.Model model, OpenStudio.Space space, FootprintPolygon footprint) {
        using OpenStudio.Point3dVector vertices = Vertices(footprint);
        OpenStudio.Surface osSurface = new(vertices, model);
        osSurface.setSpace(space);
        return osSurface;
    }

    static Unit Bind(OpenStudio.Surface surface, OpenStudio.Construction construction) {
        surface.setConstruction(construction);
        return unit;
    }

    static WriterT<ReadLog, Fin, Unit> BuildOpenings(OpenStudio.Model model, OpenStudio.Surface host, NodeId spaceId, string hostIdentifier, ElementGraph graph, GeometrySource geometry) =>
        graph.OpeningsOf(spaceId, hostIdentifier)
            .TraverseM(opening =>
                from ring in Reads.Lift(geometry.Footprint(opening.Representations)
                    .ToFin(Missing(AssessmentInputReason.MeasureAbsent, opening.Id.ToValue())))
                let sub = Seated(model, host, ring, OpeningType.Of(opening.Classification.Code).On(host.surfaceType()))
                from construction in Reads.Lift(graph.LayerSetOf(opening.Id).Bind(set => BuildConstruction(model, set, graph)))
                select Bind(sub, construction))
            .As().Map(static _ => unit);

    static OpenStudio.SubSurface Seated(OpenStudio.Model model, OpenStudio.Surface host, FootprintPolygon ring, string subSurfaceType) {
        using OpenStudio.Point3dVector vertices = Vertices(ring);
        OpenStudio.SubSurface sub = new(vertices, model);
        sub.setSurface(host);
        sub.setSubSurfaceType(subSurfaceType);
        return sub;
    }

    static Unit Bind(OpenStudio.SubSurface sub, OpenStudio.Construction construction) {
        sub.setConstruction(construction);
        return unit;
    }

    static Fin<OpenStudio.Construction> BuildConstruction(OpenStudio.Model model, MaterialComposition.LayerSet set, ElementGraph graph) =>
        set.Layers
            .TraverseM(layer => graph.Material(layer.Material)
                .ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, layer.Material.ToValue()))
                .Map(node => (Layer: layer, Props: node.Properties))).As()
            .Bind(rows => rows.Exists(static r => r.Props.Optical.IsSome) && !rows.ForAll(static r => r.Props.Optical.IsSome)
                ? Fin.Fail<Seq<(MaterialLayer Layer, Seq<MaterialPropertySet> Props)>>(
                    Missing(AssessmentInputReason.CompositionShape, set.Layers.Head.Map(static l => l.Material.ToValue()).IfNone(string.Empty)))
                : Fin.Succ(rows))
            .Bind(rows => rows.TraverseM(r => Layer(model, r.Layer, r.Props)).As())
            .Map(materials => {
                using OpenStudio.MaterialVector vec = new(materials);
                OpenStudio.Construction construction = new(model);
                construction.setLayers(vec);
                return construction;
            });

    static Fin<OpenStudio.Material> Layer(OpenStudio.Model model, MaterialLayer layer, Seq<MaterialPropertySet> props) =>
        props.Optical.Match(
            Some: optical => props.Thermal
                .ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, layer.Material.ToValue()))
                .Map(thermal => {
                    OpenStudio.StandardGlazing glass = new(model, GlazingSpectral, layer.Thickness.Si);
                    GlazingWire.Update(optical, glass);
                    glass.setThermalConductivity(thermal.Conductivity.Si);
                    return (OpenStudio.Material)glass;
                }),
            None: () =>
                from thermal in props.Thermal.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, layer.Material.ToValue()))
                from mechanical in props.Mechanical.ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, layer.Material.ToValue()))
                select (OpenStudio.Material)new OpenStudio.StandardOpaqueMaterial(model, OpaqueRoughness,
                    layer.Thickness.Si, thermal.Conductivity.Si, mechanical.Density.Si, thermal.SpecificHeat.Si));

    static OpenStudio.Point3dVector Vertices(FootprintPolygon footprint) {
        OpenStudio.Point3dVector vec = new();
        foreach (Vector3 p in footprint.Ring) { using OpenStudio.Point3d point = new(p.X, p.Y, p.Z); vec.Add(point); }
        return vec;
    }
}
```

## [04]-[SIMULATION_RUN]

- Owner: `EnergySimulation.RunLocal` the subprocess arm; `Leased` the ONE run-directory capsule both routes take; `RunSubprocess` the EnergyPlus subprocess; `ResultFuel`/`ResultEndUse`/`ResultMeasure` the transcribed axis and physics rosters (`ResultScope` rides `[03]` with the zone roster that decides it); `ResultPoint` the pre-admission magnitude, `ResultContext` the read's addressing, and `EnergyReadout` the two-outcome carrier; `EndUseCell` the two-monoid cell value; `ReadResults`/`HeadRows`/`Cells`/`PeakDemand`/`TabularRows`/`UnmetPoints`/`Tabular`/`Rows`/`SummaryFacts`/`ValidityFacts`/`GoverningEui`/`Lower` the `SqlFile` result read shared by both routes.
- Entry: `static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, ContentAddress key, IClock clock)` resolves the binary through `EnergyToolchain.Resolve`, builds the OSM model and IDF, runs the subprocess over the leased scratch directory, reads `eplusout.sql` through `SqlFile`, publishes the typed rows through `sink.Rows`, and returns the summary `AssessmentResult`.
- Law: the typed result catalog leaves through the ONE `AssessmentSink` egress port, its `Rows` leg. A private per-discipline publisher whose default binding SUCCEEDS ON A DISCARD is the deleted form — the whole read module minted, ran its own admission, and then reached no consumer at all, with no operator-visible trace that it had not. The composition that genuinely wants a verdict-only run binds `AssessmentSink.Discarding`, which names the drop as a decision recoverable from the value.
- Law: the assessment content key rides IN. Every row this run lands is addressed by it — the sink's own law — and a runner that re-derived a key of its own over a narrower preimage would address the rows, the artifact, and the assessment node three ways.
- Auto: the subprocess is `energyplus -w <weather> -d <outdir> -r <idf>`; a non-zero exit fails `ComputeFault.AnalysisFailed` carrying the exit status in the contract `Diagnostic`'s own slot with the stderr tail as detail. The end-use read folds the structured `SqlFile.endUses()` summary as a PRODUCT over the static `EndUses.fuelTypes()`/`categories()` vectors into one `HashMap` (each handle bracketed, the SWIG marshaling exemption) — one row per non-zero `(fuel, end-use)` point, its annual magnitude from `getEndUse(fuel, category)` and its peak from the annual maximum of `peakEnergyDemandByMonth(fuel, category, month)`, the `Water` fuel (m³ consumption) excluded before the mapping. Whole-building magnitudes ride the same axes at their un-disaggregated members (`ResultFuel.Total` × `ResultEndUse.Whole`), EUI as the `Intensity` measure over a positive conditioned area only, so a zero-area set carries no intensity row and the verdict bands `NotApplicable` rather than a fabricated 0.0-EUI Satisfied. Per-zone unmet hours read ONE `SystemSummary` `Time Setpoint Not Met` table parameterized on its row name — `Facility` for the building scope, the upper-cased zone name for each `ZoneTarget` — so a per-zone read is one roster row rather than a second query family. Every magnitude mints through the transcribed roster's own `ResultMeasure.Admit` — the Bim admission's mint verbatim, so the run's `QuantityType`/`Dimension` stamp and the bag's are one fact — and its `MeasureValue.OfSi` finite gate refuses a non-finite SI value before a row exists; the GJ→J and hours→s coercions ride `UnitsNet` once.
- Law: a peak that no month reported is ABSENT, not zero. The monthly accessor answers a SWIG `OptionalDouble`, so absence is observable there and rides `Option<double>` through the cell. The ANNUAL magnitude carries a NAMED loss the foreign API forces: `EndUses.getEndUse` returns a bare `double` and publishes no absence signal, so an unmetered cell and a genuinely zero-draw metered one are one value at that surface — the cell is skipped when both its annual is zero and its peak absent, and that conflation is the OpenStudio structure's, recorded here rather than hidden behind an `Option` this lane cannot honestly mint.
- Output: the typed rows land through `sink.Rows` as `AssessmentRow`s whose ordered facet path IS the axis point — `(measure, fuel, end-use, scope-kind, scope-target)` — so the `Rasm.Bim/Energy/results#RESULTS_ADMISSION` admission reads them off the assessment content key and lands them as `Pset_EnergyResults` bags with no parse and no shared store. The `AssessmentResult` carries the summary fact stream the spine verdicts.
- Result: `AssessmentResult` carries the energy discipline, route, content key, elapsed time, translator warnings, conditioning-claim census, version probe, and retained scratch directory; construction or reported-version failures fail before simulation.
- Packages: Microsoft.Data.Sqlite (the read-only tabular reader for the setpoint-not-met rows the SWIG `SqlFile` exposes no accessor for), NREL.OpenStudio.macOS-arm64 (the `SqlFile` totals + structured `EndUses` fold + `peakEnergyDemandByMonth` + `hoursSimulated` + the static run-context helpers), UnitsNet (the GJ→J / J→kWh / hours→s coercions), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]` — the transcribed result vocabulary), LanguageExt.Core (`Fin`/`Seq`/`Option`/`HashMap`/`WriterT`/`Monoid`/`Traverse`/`TraverseM`), NodaTime, Rasm (kernel — `ContentHash.Of`, `Op`), Rasm.Element (project — `ElementGraph`, `Dimension`, `QuantityType`, `UnitProvenance`, `MeasureValue`, `PropertyValue`, `NodeId`, `ContentAddress`, `BlobKey`), the `Analysis/dispatch#DISPATCH_WRITEBACK` `AssessmentSink`/`AssessmentRow`, BCL inbox.
- Growth: a new published magnitude is one `ResultPoint` on whichever AXIS it widens — an ASHRAE-55 comfort tally is one `SystemSummary` read onto the settled `ResultMeasure.ComfortHours` row, sub-annual demand shape one fold over `SqlFile.energyConsumptionByMonth`; a newly-metered fuel or service is one roster row here MIRRORING its Bim axis row, and until both land the token DEGRADES with a named fact carrying its own magnitude.
- Boundary: the EnergyPlus binary is the resolved subprocess (OpenStudio does not run it), so the runner owns the process lifetime, scratch directory, and stderr capture through the ONE leased capsule (Exemption: native subprocess + filesystem); the model build and SQL read are the single-threaded native boundary; every OpenStudio handle is disposed; the SQL accessors return SWIG `OptionalDouble` lowered to `Option<double>`, never a bare `get()` faulting in native code. The FUEL AXIS SURVIVES the read — a per-category all-fuel sum publishes the same heating row for a district-heated building and an all-electric one and no consumer can recover which fuel carried it, so the fold mints per cell and only the `Interior`/`Exterior` lighting and equipment pairs collapse, onto the one SERVICE row each names, summing their annual energy and taking the MAX of their peaks because two coincident peaks are one demand instant. Source and net-source energy carry no axis point — the axes name the fuel and the service, never the accounting basis — so they stay summary facts beside the typed rows, and the SITE total is the one row that also rides the axis at `Total`×`Whole`. An out-of-roster fuel or category token lands a named degrade fact carrying its own magnitude, never a silent drop and never a fabricated axis member. The transcribed rosters MIRROR the Bim owner and mint no member of their own: a local row, a flat slug, or a mint-side aggregate forks the admission axis, so a Bim axis row and its transcription land in one pass or the token degrades until both do. A non-zero exit or a missing SQL file fails `AnalysisFailed`, never a silent zero-energy result; a missing per-zone unmet row fails too, because a report cannot distinguish a zone that met its setpoints from a zone whose row never landed.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResultFuel {
    public static readonly ResultFuel Total = new("Total", wireKey: None);
    public static readonly ResultFuel Electricity = new("Electricity", wireKey: Some("Electricity"));
    public static readonly ResultFuel NaturalGas = new("NaturalGas", wireKey: Some("Gas"));
    public static readonly ResultFuel DistrictHeating = new("DistrictHeating", wireKey: Some("DistrictHeating"));
    public static readonly ResultFuel DistrictCooling = new("DistrictCooling", wireKey: Some("DistrictCooling"));

    public Option<string> WireKey { get; }

    public const string WaterToken = "Water";

    public static Option<ResultFuel> Of(string token) => toSeq(Items).Find(row => row.WireKey.Exists(key => key == token));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResultEndUse {
    public static readonly ResultEndUse Whole = new("Whole", wireKeys: Seq<string>());
    public static readonly ResultEndUse Heating = new("Heating", wireKeys: Seq("Heating"));
    public static readonly ResultEndUse Cooling = new("Cooling", wireKeys: Seq("Cooling"));
    public static readonly ResultEndUse Lighting = new("Lighting", wireKeys: Seq("InteriorLights", "ExteriorLights"));
    public static readonly ResultEndUse Equipment = new("Equipment", wireKeys: Seq("InteriorEquipment", "ExteriorEquipment"));
    public static readonly ResultEndUse Fans = new("Fans", wireKeys: Seq("Fans"));
    public static readonly ResultEndUse Pumps = new("Pumps", wireKeys: Seq("Pumps"));
    public static readonly ResultEndUse WaterSystems = new("WaterSystems", wireKeys: Seq("WaterSystems"));

    public Seq<string> WireKeys { get; }

    public static Option<ResultEndUse> Of(string token) => toSeq(Items).Find(row => row.WireKeys.Contains(token));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResultMeasure {
    static readonly Dimension EnergyDim = Dimension.Create(2, 1, -2, 0, 0, 0, 0);
    static readonly Dimension PowerDim = Dimension.Create(2, 1, -3, 0, 0, 0, 0);
    static readonly Dimension IntensityDim = Dimension.Create(0, 1, -2, 0, 0, 0, 0);
    static readonly QuantityType EnergyType = QuantityType.Create("Energy");
    static readonly QuantityType PowerType = QuantityType.Create("Power");
    static readonly QuantityType IntensityType = QuantityType.Create("EnergyUseIntensity");

    public static readonly ResultMeasure Annual = new("Annual", EnergyType, EnergyDim, UnitProvenance.Derive);
    public static readonly ResultMeasure Peak = new("Peak", PowerType, PowerDim, UnitProvenance.Derive);
    public static readonly ResultMeasure Intensity = new("Intensity", IntensityType, IntensityDim, UnitProvenance.Label("J/m2"));
    public static readonly ResultMeasure UnmetHours = new("UnmetHours", QuantityType.Duration, Dimension.DurationDim, UnitProvenance.Derive);
    public static readonly ResultMeasure ComfortHours = new("ComfortHours", QuantityType.Duration, Dimension.DurationDim, UnitProvenance.Derive);

    public QuantityType Type { get; }
    public Dimension Dimension { get; }
    public UnitProvenance Provenance { get; }

    private ResultMeasure(string key, QuantityType type, Dimension dimension, UnitProvenance provenance) : this(key) =>
        (Type, Dimension, Provenance) = (type, dimension, provenance);

    public Fin<MeasureValue> Admit(double si) => MeasureValue.OfSi(Type, Dimension, si, Some(Provenance));
}

public readonly record struct ResultPoint(ResultMeasure Measure, ResultFuel Fuel, ResultEndUse Use, double Si);

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ResultContext(ContentAddress Key, UInt128 Model, Seq<ZoneTarget> Zones);

public readonly record struct EndUseCell(double AnnualGj, Option<double> PeakW) : Monoid<EndUseCell> {
    public static EndUseCell Empty => new(0.0, None);

    public EndUseCell Combine(EndUseCell rhs) =>
        new(AnnualGj + rhs.AnnualGj,
            (PeakW, rhs.PeakW) switch {
                ({ IsSome: true, Case: double a }, { IsSome: true, Case: double b }) => Some(Math.Max(a, b)),
                ({ IsSome: true }, _) => PeakW,
                _ => rhs.PeakW,
            });

    public bool Reported => AnnualGj != 0.0 || PeakW.IsSome;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class EnergySimulation {

    static Fin<(A Value, Seq<AssessmentFact> Notes)> Leased<A>(string prefix, Func<string, WriterT<ReadLog, Fin, A>> use) =>
        Try.lift(() => Fin.Succ(Directory.CreateTempSubdirectory(prefix).FullName)).Run().Bind(static inner => inner)
            .Bind(scratch => {
                Fin<(A Value, ReadLog Log)> outcome = Try.lift(() => Reads.Run(use(scratch))).Run().Bind(static inner => inner);
                Seq<AssessmentFact> release = Try.lift(() => { Directory.Delete(scratch, recursive: true); return Fin.Succ(unit); }).Run().Bind(static inner => inner)
                    .Match(Succ: static _ => Seq<AssessmentFact>(),
                           Fail: error => Seq(AssessmentFact.Text(ScratchRetained, $"{scratch}:{Tail(error.Message)}")));
                return outcome.Map(ran => (ran.Value, ran.Log.Facts + release));
            });

    const string ScratchRetained = "scratch-retained";

    static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, ContentAddress key, IClock clock) {
        Instant at = clock.GetCurrentInstant();
        return Leased(LocalScratch, scratch =>
                    from binary in Reads.Lift(EnergyToolchain.Resolve(request.Policy.Toolchain))
                    from versionFacts in Reads.Lift(EnergyToolchain.VersionGate(binary, request.Policy.Toolchain))
                    from _ in Reads.Writing(versionFacts, unit)
                    from build in BuildModel(graph, request, geometry, scratch)
                    from sqlPath in Reads.Lift(RunSubprocess(binary, build.IdfPath, request, scratch))
                    from readout in ReadResults(sqlPath, graph, request, new ResultContext(build.Model, build.Zones))
                    from blob in Reads.Lift(sink.Store(File.ReadAllBytes(sqlPath)).Run())
                    from published in Reads.Lift(sink.Rows(readout.Rows).Run())
                    select (Readout: readout, Blob: blob))
            .Bind(leased => Publish(request, leased.Value.Readout, leased.Value.Blob, leased.Notes, at, request.Weather));
    }

    const string LocalScratch = "rasm-eplus-";

    static Fin<AssessmentResult> Publish(AssessmentRequest.Energy request, EnergyReadout readout, ArtifactContent blob, Seq<AssessmentFact> notes, Instant at, WeatherRef weather) =>
        AssessmentResult.Of(request.Route,
            readout.Facts + notes + Seq(AssessmentFact.Text(WeatherStationFact, weather.Station)),
            GoverningEui(readout.Facts, request.Policy),
            at, RunKey, resultArtifact: Some(blob));

    const string WeatherStationFact = "weather-station";

    static Fin<string> RunSubprocess(string binary, string idfPath, AssessmentRequest.Energy request, string scratch) {
        using Process process = new() {
            StartInfo = new ProcessStartInfo(binary) {
                ArgumentList = { "-w", request.Weather.EpwPath, "-d", scratch, "-r", idfPath },
                RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = scratch,
            },
        };
        process.Start();
        Task<string> stderrDrain = process.StandardError.ReadToEndAsync();
        Task<string> stdoutDrain = process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();
        string stderr = stderrDrain.GetAwaiter().GetResult();
        _ = stdoutDrain.GetAwaiter().GetResult();
        string sqlPath = Path.Combine(scratch, ResultFile);
        return process.ExitCode == 0 && File.Exists(sqlPath)
            ? Fin.Succ(sqlPath)
            : Fin.Fail<string>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<energyplus-exit:{Tail(stderr)}>", Some(process.ExitCode)));
    }

    const string ResultFile = "eplusout.sql";

    public sealed record EnergyReadout(Seq<AssessmentRow> Rows, Seq<AssessmentFact> Facts);

    static WriterT<ReadLog, Fin, EnergyReadout> ReadResults(string sqlPath, ElementGraph graph, AssessmentRequest.Energy request, ResultContext context) {
        using OpenStudio.Path resultsPath = OpenStudio.OpenStudioUtilitiesCore.toPath(sqlPath);
        using OpenStudio.SqlFile sql = new(resultsPath);
        return from floorAreaM2 in Reads.Lift(graph.ConditionedFloorArea(request.Targets))
               from totals in Reads.Lift((Head(sql.totalSiteEnergy(), SiteTotal), Head(sql.totalSourceEnergy(), SourceTotal), Head(sql.netSourceEnergy(), NetSourceTotal))
                   .Apply(static (site, source, net) => (Site: site, Source: source, Net: net)).As().ToFin())
               from head in Reads.Lift(HeadRows(context, totals.Site, floorAreaM2))
               from cells in Cells(sql, context)
               from unmet in Reads.Lift(TabularRows(sqlPath, context))
               from facts in Reads.Lift(SummaryFacts(sql, totals.Site, totals.Source, totals.Net, floorAreaM2))
               select new EnergyReadout(head + cells + unmet, facts);
    }

    static Validation<Error, double> Head(OpenStudio.OptionalDouble optional, string name) =>
        Lower(optional).Match(
            Some: static value => value,
            None: () => (Validation<Error, double>)new ComputeFault.AnalysisFailed(
                SolvePhase.Extraction, FailureKind.Foreign, $"<energyplus-sql-no-{name}>"));

    const string SiteTotal = "total-site-energy";
    const string SourceTotal = "total-source-energy";
    const string NetSourceTotal = "net-source-energy";

    static Fin<Seq<AssessmentFact>> SummaryFacts(OpenStudio.SqlFile sql, double siteGj, double sourceGj, double netSourceGj, double floorAreaM2) =>
        from intensity in floorAreaM2 > 0.0
            ? AssessmentFact.Rows(
                AssessmentFact.Measure(EuiFact, EuiDim, Joules(siteGj) / floorAreaM2),
                AssessmentFact.Measure("source-eui", EuiDim, Joules(sourceGj) / floorAreaM2))
            : Fin.Succ(Seq<AssessmentFact>())
        from head in AssessmentFact.Rows(
            AssessmentFact.Measure(SiteTotal, EnergyDim, Joules(siteGj)),
            AssessmentFact.Measure(SourceTotal, EnergyDim, Joules(sourceGj)),
            AssessmentFact.Measure(NetSourceTotal, EnergyDim, Joules(netSourceGj)))
        from validity in ValidityFacts(sql)
        select head + intensity + validity;

    const string EuiFact = "eui";

    static Fin<Seq<AssessmentRow>> HeadRows(ResultContext context, double siteGj, double floorAreaM2) =>
        Rows(context, ResultScope.Whole,
            Seq(new ResultPoint(ResultMeasure.Annual, ResultFuel.Total, ResultEndUse.Whole, Joules(siteGj)))
                .Append(floorAreaM2 > 0.0
                    ? Seq(new ResultPoint(ResultMeasure.Intensity, ResultFuel.Total, ResultEndUse.Whole, Joules(siteGj) / floorAreaM2))
                    : Seq<ResultPoint>()));

    static WriterT<ReadLog, Fin, Seq<AssessmentRow>> Cells(OpenStudio.SqlFile sql, ResultContext context) {
        using OpenStudio.OptionalEndUses optional = sql.endUses();
        if (!optional.is_initialized()) {
            return Reads.Lift(Fin.Fail<Seq<AssessmentRow>>(new ComputeFault.AnalysisFailed(
                SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-end-uses>")));
        }
        using OpenStudio.EndUses uses = optional.get();
        using OpenStudio.EndUseCategoryTypeVector categories = OpenStudio.EndUses.categories();
        using OpenStudio.EndUseFuelTypeVector fuels = OpenStudio.EndUses.fuelTypes();
        Seq<(int Fuel, int Category)> product =
            toSeq(Enumerable.Range(0, checked((int)fuels.Count)))
                .Bind(f => toSeq(Enumerable.Range(0, checked((int)categories.Count))).Map(c => (Fuel: f, Category: c)));
        (HashMap<(ResultFuel, ResultEndUse), EndUseCell> cells, Seq<AssessmentFact> notes) = product.Fold(
            (Cells: HashMap<(ResultFuel, ResultEndUse), EndUseCell>(), Notes: Seq<AssessmentFact>()),
            (state, index) => {
                using OpenStudio.EndUseFuelType fuel = fuels[index.Fuel];
                using OpenStudio.EndUseCategoryType category = categories[index.Category];
                string fuelToken = fuel.valueName();
                string useToken = category.valueName();
                if (StringComparer.Ordinal.Equals(fuelToken, ResultFuel.WaterToken)) { return state; }
                EndUseCell cell = new(uses.getEndUse(fuel, category), PeakDemand(sql, fuel, category));
                if (!cell.Reported) { return state; }
                return (ResultFuel.Of(fuelToken), ResultEndUse.Of(useToken)) switch {
                    ({ IsSome: true, Case: ResultFuel mappedFuel }, { IsSome: true, Case: ResultEndUse mappedUse }) =>
                        state with { Cells = state.Cells.AddOrUpdate((mappedFuel, mappedUse), held => held.Combine(cell), cell) },
                    _ => state with { Notes = state.Notes.Add(AssessmentFact.Text(UnmappedFact, $"{fuelToken}:{useToken}:{cell.AnnualGj:R}")) },
                };
            });
        return from _ in Reads.Writing(notes, unit)
               from rows in Reads.Lift(cells.AsIterable().TraverseM(cell => Rows(context, ResultScope.Whole, Points(cell.Key, cell.Value))).As())
               select rows.Bind(static row => row);
    }

    const string UnmappedFact = "end-use-unmapped";

    static Seq<ResultPoint> Points((ResultFuel Fuel, ResultEndUse Use) axis, EndUseCell cell) =>
        (cell.AnnualGj != 0.0 ? Seq(new ResultPoint(ResultMeasure.Annual, axis.Fuel, axis.Use, Joules(cell.AnnualGj))) : Seq<ResultPoint>())
        + cell.PeakW.Map(peak => new ResultPoint(ResultMeasure.Peak, axis.Fuel, axis.Use, peak)).ToSeq();

    static Option<double> PeakDemand(OpenStudio.SqlFile sql, OpenStudio.EndUseFuelType fuel, OpenStudio.EndUseCategoryType category) =>
        toSeq(Enumerable.Range(1, MonthsPerYear))
            .Choose(m => { using OpenStudio.MonthOfYear month = new(m); return Lower(sql.peakEnergyDemandByMonth(fuel, category, month)); })
            .Fold(Option<double>.None, static (peak, monthly) => Some(peak.Match(Some: held => Math.Max(held, monthly), None: () => monthly)));

    const int MonthsPerYear = 12;

    static Fin<Seq<AssessmentFact>> ValidityFacts(OpenStudio.SqlFile sql) =>
        Lower(sql.hoursSimulated())
            .ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-hours-simulated>"))
            .Bind(static hours => AssessmentFact.Measure("hours-simulated", Dimension.DurationDim, Seconds(hours)).Map(static fact => Seq(fact)));

    static Fin<Seq<AssessmentRow>> TabularRows(string sqlPath, ResultContext context) {
        using Microsoft.Data.Sqlite.SqliteConnection connection = new($"Data Source={sqlPath};Mode=ReadOnly;Pooling=False;");
        connection.Open();
        return (Seq(new ZoneTarget(FacilityRow, ResultScope.Whole)) + context.Zones)
            .TraverseM(target => UnmetPoints(connection, target).Bind(points => Rows(context, target.Scope, points))).As()
            .Map(static rows => rows.Bind(static row => row));
    }

    static Fin<Seq<ResultPoint>> UnmetPoints(Microsoft.Data.Sqlite.SqliteConnection connection, ZoneTarget target) =>
        Seq((Use: ResultEndUse.Heating, Column: OccupiedHeating), (Use: ResultEndUse.Cooling, Column: OccupiedCooling))
            .Traverse(read => Tabular(connection, UnmetReport, UnmetTable, target.Row, read.Column)
                .ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign,
                    $"<energyplus-sql-unmet-missing:{target.Row}:{read.Column}>"))
                .Map(hours => new ResultPoint(ResultMeasure.UnmetHours, ResultFuel.Total, read.Use, Seconds(hours)))
                .ToValidation())
            .As().ToFin();

    const string UnmetReport = "SystemSummary";
    const string UnmetTable = "Time Setpoint Not Met";
    const string FacilityRow = "Facility";
    const string OccupiedHeating = "During Occupied Heating";
    const string OccupiedCooling = "During Occupied Cooling";

    static Fin<Seq<AssessmentRow>> Rows(ResultContext context, ResultScope scope, Seq<ResultPoint> points) =>
        points.Traverse(p =>
                p.Measure.Admit(p.Si, RunKey)
                    .Map(value => new AssessmentRow(context.Key.ToValue(), Discipline.Energy,
                        Seq(p.Measure.Key, p.Fuel.Key, p.Use.Key) + scope.Facets,
                        AssessmentFact.Measure($"{ModelPrefix}{context.Model:x32}", value)))
                    .ToValidation())
            .As().ToFin();

    const string ModelPrefix = "model:";

    static Option<double> Tabular(Microsoft.Data.Sqlite.SqliteConnection connection, string report, string table, string row, string column) {
        using Microsoft.Data.Sqlite.SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM TabularDataWithStrings WHERE ReportName = $report AND TableName = $table AND RowName = $row AND ColumnName = $column LIMIT 1";
        command.Parameters.AddWithValue("$report", report);
        command.Parameters.AddWithValue("$table", table);
        command.Parameters.AddWithValue("$row", row);
        command.Parameters.AddWithValue("$column", column);
        return Optional(command.ExecuteScalar()).Bind(static value => double.TryParse($"{value}", CultureInfo.InvariantCulture, out double parsed) ? Some(parsed) : None);
    }

    static double Seconds(double hours) => UnitsNet.Duration.FromHours(hours).Seconds;

    static Option<double> GoverningEui(Seq<AssessmentFact> facts, EnergyPolicy policy) =>
        from target in policy.TargetEui.Filter(static value => value > 0.0)
        from euiSi in facts.Choose(static f => f.Name.ToValue() == EuiFact && f.Value is PropertyValue.Measure m ? Some(m.Value.Si) : None).Head
        select UnitsNet.Energy.FromJoules(euiSi).KilowattHours / target;

    static Option<double> Lower(OpenStudio.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    const int DetailTailChars = 256;

    static string Tail(string s) => s.Length <= DetailTailChars ? s : s[^DetailTailChars..];

    static ComputeFault Missing(AssessmentInputReason reason, string witness) => new ComputeFault.AssessmentInputMissing(reason, witness);
}
```

## [05]-[CLOUD_ROUTE]

- Owner: `EnergyRoute` the closed execution-provider `[Union]` on `EnergyPolicy` (`Subprocess` the local default · `Cloud` the Pollination row carrying owner/project/job-descriptor/platform/model key as neutral values beside its published watch curve); `EnergySimulation.Run` the one entry whose generated total `Switch` dispatches the row; `RunCloud` the Pollination arm; `Orchestrate` the bracketed async SDK kernel.
- Entry: `public static Fin<AssessmentResult> Run(...)` dispatches `request.Policy.Route` — `Subprocess` enters `RunLocal` (`[04]`), `Cloud` enters `RunCloud`, which submits the app-authored job descriptor, watches the run to a terminal status under the route's own bounded curve, gates on an exact `RunStatusEnum.Succeeded` parse, pulls the result assets, locates the downloaded `eplusout.sql`, and converges on the same `ReadResults` fold `[04]` owns — one result read serves both providers, so the typed rows, fact stream, EUI verdict, and result shape are route-invariant.
- Law: the watch is a BOUNDED wait on a stated curve, never an open poll. `WatchJobStatusAsync` blocks to a terminal with no bound, no backoff, and no exit — a cloud job that never terminates held the runner forever and reported nothing. The route PUBLISHES a `RedrivePolicy` whose `Curve` the in-process wait repeats on and whose bound turns an unfinished run into a TYPED exhaustion fault naming the last status seen, because a success-shaped fall-through certifies unconverged as converged.
- Law: transport retriability is PUBLISHED, never executed here. The pinned `PollinationSDK` surfaces no response headers on `ApiException` (decompile-proven: `ErrorCode` + `ErrorContent` alone), so a `Retry-After` window has no spelling on this transport and every transient refusal — 429, 408, 5xx — raises `EndpointUnreachable`, which publishes `Transient`; `EndpointThrottled` re-enters WITH the header surface the day the SDK grows one. The root-bound executor spends those postures — this arm spells no attempt counter and no delay literal of its own.
- Auto: the HBJSON payload inside the job descriptor is the app-staged energy model, and `Model` is that artifact's own content key hoisted out of the descriptor so the results join needs no descriptor parse — the column adds no content-key surface, the descriptor it is read from already folding verbatim. Downloaded assets land content-keyed on the Persistence object plane exactly as the local `eplusout.sql` does, and the assessment node keys the same `(input subgraph, route, policy)` content key, so a re-submitted identical model+recipe resolves from the Persistence index; the SDK's `Wrapper.LocalDatabase` and its path-existence `CheckCached` are not composed (path-existence reuse without hash verification is the integrity gap the content-keyed index closes).
- Result: the `Assessment` result carries the cloud provenance beside the route/content-key columns; the watch-status trail folds into the same `ReadLog` ledger the local route uses.
- Packages: PollinationSDK (the `Wrapper` job/run/asset orchestration + `RunStatusEnum` terminal vocabulary + `Client.ApiException` — sidecar-isolated: its vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` closure never meets the STJ serializers nor loads in-Rhino), LanguageExt.Core (`IO`/`Schedule`/`Fin`), Rasm (kernel — `RedrivePolicy`/`Retriability`), NodaTime (`Duration`), BCL inbox.
- Growth: a new cloud provider is one `EnergyRoute` case with one arm (the `Switch` breaks every dispatch site at compile time); a recipe change is job-descriptor data, never a signature; a re-cadenced watch is one `RedrivePolicy` value on the row; per-output typed decodes beyond the SQLite widen `Orchestrate` by one asset row.
- Boundary: `Configuration`/`TokenRepo` auth is composition-root input to the ambient SDK configuration, never a policy column or fence member (the Persistence token-lifecycle law). Async orchestration is one blocking boundary kernel inside the leased scratch capsule (Exemption: sidecar HTTP + filesystem). The terminal gate PARSES the watched status against the `RunStatusEnum` token set and demands equality with `Succeeded`: a substring test over the raw string admits any status that merely contains the token and publishes a failed run's partial assets as an answer. Cloud rows publish BUILDING scope only, because zone naming belongs to the recipe and a correspondence Compute never authored is a guess. The staged model key crosses as a `UInt128` content key rather than a foreign address string, so a malformed key is unrepresentable rather than a refusal path this lane has to carry — the address grammar belongs to whoever staged the artifact, and this package holds no reference to it. Artifact residency (presigned-grant transfer, reuse index, PROV attribution) stays the Persistence owners' rows composed at the boundary. Cloud-side model rebuild from the graph is the rejected form — cloud consumes the app-staged HBJSON, local consumes the in-process OSM build, two rows on one axis.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyRoute {
    private EnergyRoute() { }

    public sealed record Subprocess : EnergyRoute;

    public sealed record Cloud(string Owner, string Project, string JobDescriptor, string Platform, UInt128 Model, RedrivePolicy Watch) : EnergyRoute;

    public static readonly EnergyRoute Local = new Subprocess();

    public static readonly RedrivePolicy CanonicalWatch = RedrivePolicy.Of(
        law: Schedule.exponential(Duration.FromSeconds(5))
            | Schedule.jitter(Duration.FromSeconds(1))
            | Schedule.maxDelay(Duration.FromMinutes(2)),
        bound: 240);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class EnergySimulation {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, ContentAddress key, IClock clock) =>
        request.Policy.Route.Switch(
            subprocess: _ => RunLocal(graph, request, geometry, sink, clock),
            cloud:      c => RunCloud(graph, request, c, sink, clock));

    static Fin<AssessmentResult> RunCloud(ElementGraph graph, AssessmentRequest.Energy request, EnergyRoute.Cloud route, AssessmentSink sink, ContentAddress key, IClock clock) {
        Instant at = clock.GetCurrentInstant();
        return Leased(CloudScratch, scratch =>
                    from sqlPath in Reads.Lift(Orchestrate(route, scratch).Run())
                    from readout in ReadResults(sqlPath, graph, request, new ResultContext(route.Model, Seq<ZoneTarget>()))
                    from blob in Reads.Lift(sink.Store(File.ReadAllBytes(sqlPath)).Run())
                    from published in Reads.Lift(sink.Rows(readout.Rows).Run())
                    select (Readout: readout, Blob: blob))
            .Bind(leased => Publish(request, leased.Value.Readout, leased.Value.Blob, leased.Notes, at, request.Weather));
    }

    const string CloudScratch = "rasm-pollination-";

    static IO<Fin<string>> Orchestrate(EnergyRoute.Cloud route, string scratch) =>
        IO.liftAsync(async () => {
                PollinationSDK.Wrapper.JobInfo job = PollinationSDK.Wrapper.JobInfo.FromJson(route.JobDescriptor);
                return await job.RunJobAsync();
            })
            .Bind(scheduled => Watch(scheduled, route)
                .Bind(status => Terminal(status).Match(
                    Succ: _ => IO.liftAsync(async () => {
                        PollinationSDK.Wrapper.RunInfo run = new(scheduled);
                        await run.DownloadRunAssetsAsync([.. run.GetOutputAssets(route.Platform)], saveAsDir: scratch);
                        return Located(scratch);
                    }),
                    Fail: error => IO.pure(Fin.Fail<string>(error)))))
            .Catch(static (Error error) => IO.pure(Fin.Fail<string>(error)));

    static IO<string> Watch(PollinationSDK.Wrapper.ScheduledJobInfo scheduled, EnergyRoute.Cloud route) =>
        IO.liftAsync(async () => await scheduled.WatchJobStatusAsync())
            .RepeatWhile(schedule: route.Watch.Curve, predicate: static status => !Settled(status));

    static Fin<Unit> Terminal(string status) =>
        Enum.TryParse(status.Trim(), ignoreCase: true, out PollinationSDK.RunStatusEnum parsed) && parsed == PollinationSDK.RunStatusEnum.Succeeded
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<pollination-terminal:{Tail(status)}>"));

    static bool Settled(string status) =>
        Enum.TryParse(status.Trim(), ignoreCase: true, out PollinationSDK.RunStatusEnum parsed)
        && parsed is PollinationSDK.RunStatusEnum.Succeeded or PollinationSDK.RunStatusEnum.Failed or PollinationSDK.RunStatusEnum.Cancelled;

    static Fin<string> Located(string scratch) =>
        Optional(Directory.EnumerateFiles(scratch, ResultFile, SearchOption.AllDirectories).FirstOrDefault())
            .ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<pollination-no-sql-asset>"));

}
```

## [06]-[GRAPH_READS]

- Owner: `BoundaryReads` the projected space-boundary edge owner every discipline that reads spatial containment composes — `SpacesOf`/`SurfacesOf`/`OpeningsOf`/`ConditioningOf`/`ConditionedFloorArea`/`LayerSetOf`/`Descend`, the `WireName` roster key, and the `BoundaryRole` discriminant the two edge filters fold onto.
- Law: this owner is DISCIPLINE-NEUTRAL by name and by content. It reads the projected `IfcRelSpaceBoundary` edges and the contract containment decomposition, both of which the energy runner, the circulation runner, and the acoustic room fold all need — an `Energy`-named owner two other disciplines composed was a name that lied about who it served, and a per-discipline copy of the same edge walk is the fork it prevents.
- Law: the two boundary filters are ONE fold. `SurfacesOf` and `OpeningsOf` selected the same edge set with the same three conjuncts and differed only on whether the `Host` attribute was absent or matched — `BoundaryRole` names that discriminant and one partition answers both, so a change to the edge predicate cannot land on one and miss the other.
- Entry: `SpacesOf(targets)` answers the reachable `IfcSpace` nodes, `SurfacesOf(space)` the bounding surfaces at the finest declared boundary level, `OpeningsOf(space, host)` the host-attributed opening boundaries, `ConditioningOf(space)` the three-state occupancy claim, `ConditionedFloorArea(targets)` the EUI denominator, and `LayerSetOf(node)` the layered composition an admission path needs.
- Packages: LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `Node.Object`, `NodeId`, `Relationship.Generic`/`Compose`, `WireName`, `PropertyValue`, `MaterialComposition`, `QuantityRows`/`BoundaryRows`/`EnvelopeRows`), the `Analysis/assessment#ANALYSIS_READS` `AnalysisReads` bag-and-edge read owner, the `Runtime/admission#DISPATCH_SPINE` fault family.
- Boundary: every bag and edge-attribute read composes `AnalysisReads` — the one owner over `Rasm.Element`-declared rows — so this page adds discipline SELECTION over that owner's shapes and re-declares none of them; the set-scoped overload narrows to the named bag where the discipline needs it. The containment descent is ITERATIVE over an explicit frontier with a visited set, never recursion: the descent runs before any `Bake`, so a cyclic `Compose` chain is a real input, and the hand recursion it replaces admitted its own StackOverflow risk in a comment without closing it. Only the owning `Compose` flavours descend — the non-owning `Reference` flavour is excluded, so a reference edge cannot pull an unrelated subtree into a building's space roster.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoundaryRole {
    public static readonly BoundaryRole Bounding = new("bounding");
    public static readonly BoundaryRole Opening = new("opening");
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class BoundaryReads {
    public static readonly WireName SpaceBoundary = WireName.Create("IfcRelSpaceBoundary");

    const string SpaceClass = "IfcSpace";
    const string SecondLevel = "2nd";

    extension(ElementGraph graph) {
        public Seq<Node.Object> SpacesOf(Seq<NodeId> targets) =>
            targets.IsEmpty
                ? graph.ObjectNodes.Filter(IsSpace)
                : targets.Bind(t => graph.Descend(t)).Distinct().Choose(graph.Find<Node.Object>).Filter(IsSpace).ToSeq();

        public Seq<Node.Object> SurfacesOf(NodeId space) {
            Seq<(Relationship.Generic Edge, Node.Object Surface)> bounding = graph.Boundaries(space, BoundaryRole.Bounding, static _ => true);
            Seq<(Relationship.Generic Edge, Node.Object Surface)> secondLevel = bounding.Filter(static b =>
                b.Edge.Text(BoundaryRows.BoundaryLevel).Exists(static level => level == SecondLevel));
            return (secondLevel.IsEmpty ? bounding : secondLevel).Map(static b => b.Surface);
        }

        public Seq<Node.Object> OpeningsOf(NodeId space, string hostIdentifier) =>
            graph.Boundaries(space, BoundaryRole.Opening, host => host == hostIdentifier).Map(static b => b.Surface);

        Seq<(Relationship.Generic Edge, Node.Object Surface)> Boundaries(NodeId space, BoundaryRole role, Func<string, bool> host) =>
            graph.EdgesAt(space).Choose(e =>
                e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
                && (role == BoundaryRole.Opening
                    ? g.Text(BoundaryRows.Host).Exists(host)
                    : g.Attribute(BoundaryRows.Host).IsNone)
                    ? graph.Find<Node.Object>(g.Related).Map(s => (Edge: g, Surface: s))
                    : None).ToSeq();

        public Fin<double> ConditionedFloorArea(Seq<NodeId> targets) =>
            graph.SpacesOf(targets).Filter(s => graph.ConditioningOf(s.Id).Conditions)
                .TraverseM(space => graph.NetFloorAreaM2(space.Id)).As()
                .Map(static areas => areas.Sum());

        public ConditioningClaim ConditioningOf(NodeId space) =>
            graph.Property(space, EnvelopeRows.IsExternal, Some(EnvelopeRows.SpaceCommon)).Match(
                Some: static v => v is PropertyValue.Boolean { Value: true } ? ConditioningClaim.External : ConditioningClaim.Conditioned,
                None: static () => ConditioningClaim.Unstated);

        Fin<double> NetFloorAreaM2(NodeId space) =>
            graph.Quantity(space, QuantityRows.NetFloorArea, Some(QuantityRows.SpaceBaseQuantities)).Map(static m => m.Si)
                .ToFin((Error)new ComputeFault.AssessmentInputMissing(AssessmentInputReason.MeasureAbsent, space.ToValue()));

        public Fin<MaterialComposition.LayerSet> LayerSetOf(NodeId node) =>
            graph.CompositionOf(node)
                .Bind(static composition => composition is MaterialComposition.LayerSet set ? Some(set) : None)
                .ToFin((Error)new ComputeFault.AssessmentInputMissing(AssessmentInputReason.CompositionShape, node.Value));

        Seq<NodeId> Descend(NodeId root) {
            Seq<NodeId> frontier = Seq(root);
            HashSet<NodeId> seen = HashSet<NodeId>();
            Seq<NodeId> reached = Seq<NodeId>();
            while (frontier.Head is { IsSome: true, Case: NodeId node }) {
                frontier = frontier.Tail;
                if (seen.Contains(node)) { continue; }
                seen = seen.Add(node);
                reached = reached.Add(node);
                frontier += graph.EdgesAt(node).Choose(e =>
                    e is Relationship.Compose c && c.Whole == node && c.SubKind != ComposeKind.Reference ? Some(c.Part) : None).ToSeq();
            }
            return reached;
        }
    }

    static bool IsSpace(Node.Object o) => o.Classification.Code == SpaceClass;
}
```
