# [COMPUTE_ENERGY]

Rasm.Compute owns the whole-building energy-simulation runner — the `Discipline.Energy` arm of the assessment rail. It builds an `NREL.OpenStudio` `Model` in-process from the concrete `Rasm.Element` `ElementGraph` (spaces, bounding surfaces, layered opaque constructions, thermal zones), stamps the annual EnergyPlus run context (weather-file `SimulationControl`, a full-year `RunPeriod`, the attached `EpwFile`), conditions each occupied zone with ideal-air loads driven to policy dual setpoints and policy lighting/equipment gains so demand is the building-envelope-driven load, forward-translates to an IDF through `EnergyPlusForwardTranslator`, runs EnergyPlus as a subprocess over a parameterized binary-discovery boundary, and reads the result SQLite through `SqlFile`. A run publishes TWO outcomes off that one read: the typed `EnergyResult` rows carrying every magnitude on its own `(fuel × end-use × measure × scope)` axis point, and the summary `AssessmentResult` fact stream the assessment spine bands its EUI verdict from. Execution is one route axis — the `EnergyRoute` `[Union]` dispatches a local EnergyPlus subprocess (default row) or a Pollination cloud run — one entry, one `SqlFile` result read, provider variance as row data.

OpenStudio (the SWIG SDK) builds the model and reads the results; it neither runs nor bundles the EnergyPlus binary, and its version fixes the EnergyPlus the toolchain must resolve. Every wrapper owns a native handle and is `IDisposable`, model mutation is single-threaded, and every load/get that can miss returns a SWIG `Optional<T>` lowered onto `Fin<T>`/`Option<T>` at the boundary. Compute admits `NREL.OpenStudio.macOS-arm64` for simulation, distinct from the `Rasm.Bim/Energy/exchange` exchange owner. Cloud runs consume the Bim-lowered HBJSON `EnergyArtifact`; its selected `eplusout.sql` result lands content-keyed through `AssessmentSink`, while unselected downloaded assets remain inside the bracketed scratch directory.

Result vocabulary arrives settled from `Rasm.Bim/Energy/results#RESULTS_ADMISSION` and is COMPOSED here, never re-declared: `ResultFuel` × `ResultEndUse` are the two axes a magnitude is a point on, `ResultMeasure` carries the physics (`Annual`/`Peak`/`Intensity`/`UnmetHours`/`ComfortHours`), `ResultScope` names the granularity (`Building`/`Zone`/`Space`), and `EnergyResult.Of` is the admission gate every row mints through. Each row addresses the run by the `Rasm.Bim/Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` of the energy model the run CONSUMED — the translated IDF for a local run, the app-staged HBJSON for a cloud one — so the Bim admission joins a result set to its model with no parse and no shared store.

## [01]-[INDEX]

- [02]-[TOOLCHAIN_BOUNDARY]: `EnergyToolchain` resolves the EnergyPlus binary under a version lock and `EnergyPolicy` carries the simulation scenario.
- [03]-[MODEL_BUILD]: `EnergySimulation.BuildModel` folds the graph into an in-process `OpenStudio.Model`, translates it to IDF across the SWIG boundary rails, and mints the run's `ArtifactKey` and zone roster.
- [04]-[SIMULATION_RUN]: `EnergySimulation.RunLocal` drives the EnergyPlus subprocess and folds `eplusout.sql` into the typed `EnergyResult` rows beside the summary fact stream.
- [05]-[CLOUD_ROUTE]: `EnergyRoute` selects the local subprocess or the Pollination arm, both converging on the one `ReadResults` fold.

## [02]-[TOOLCHAIN_BOUNDARY]

- Owner: `EnergyToolchain` the static EnergyPlus-executable resolver; `EnergyToolchainPolicy` the discovery policy (configured-directory override, platform executable name, expected-version datum read off the route); `EnergyPolicy` the simulation scenario the `AssessmentRequest.Energy` case carries (the `EnergyRoute` row, the toolchain, the EUI target, the dual setpoints, the lighting/equipment densities the model build reads, the `[04]`-owned `EnergyResultsPort` the typed rows egress through); the version-lock guard.
- Entry: `public static Fin<string> Resolve(EnergyToolchainPolicy policy)` probes candidate paths in priority `ENERGYPLUS_EXE` → `OPENSTUDIO_ENERGYPLUSDIR` (a full OpenStudio installation's bundled binary, dev/CI) → the policy configured directory → the app's RID-native `runtimes/<rid>/native` fallback, returns the first existing executable, and rails `ComputeFault.ToolchainUnresolved` with the full probe trail when none resolves — discovery parameterized end to end, never a hardcoded path.
- Auto: `VersionGate` checks the binary's self-reported `energyplus --version` banner against the policy expected version (the binary is the version authority, never its path) BEFORE any model build or subprocess launch — a REPORTED mismatch rails `ToolchainUnresolved`, so a version-skewed binary never consumes the translated IDF and never mints a result receipt; only an UNDETERMINED probe (a launch failure, an empty banner) degrades to a warning fact riding the result, so an air-gapped or sandboxed probe stays runnable while a real skew gates.
- Packages: LanguageExt.Core, NREL.OpenStudio.macOS-arm64 (the SWIG SDK whose version the toolchain locks EnergyPlus to — it bundles no solver, and the resolver touches no OpenStudio API), BCL inbox (`Environment`/`Path`/`File`/`AppContext`/`RuntimeInformation` for the probes, `System.Diagnostics.Process` for the `--version` self-report).
- Growth: a new discovery source is one probe in the chain; a new platform the executable-name column; a new simulation knob (ventilation rate, infiltration default, sized HVAC plant selector) one `EnergyPolicy` column; a new execution provider one `EnergyRoute` case on `[05]` — resolver widens by probe, scenario by column, provider by row, never a parallel discovery method per host.
- Boundary: a shipped app owns its EnergyPlus provisioning (a bundled RID-native binary or `ENERGYPLUS_EXE`), so the last-resort probe resolves the app's own `runtimes/<rid>/native` location off `RuntimeInformation.RuntimeIdentifier` and never assumes a developer machine or a literal RID. Version-lock is load-bearing: OpenStudio forward-translates an IDF only the version-matched EnergyPlus consumes, so a dev box points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-bundled solver, not a mismatched standalone; the resolver applies no version filter, so a mismatched binary IS selected — and `VersionGate` then REFUSES it before the run (`ToolchainUnresolved`), the expected-version policy governing execution rather than annotating it. The expected version has ONE owner — the `assessment#ROUTE_AXIS` `AssessmentRoute.EnergyPlus` `SolverVersion` pin the content key already folds — and this policy READS its version segment; a second literal here lets a bumped route re-key every assessment while the gate keeps admitting the old binary. Conditioning and internal-load defaults are explicit `EnergyPolicy` knobs, never ambient constants, so a consumer re-targets a climate or building type without an interior edit; an unresolved binary rails `ToolchainUnresolved`, never a default that fails opaquely.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EnergyToolchainPolicy(Option<string> ConfiguredDir, string ExecutableName, string ExpectedVersion) {
    // The route row's SolverVersion IS the pinned solver identity in `<tool>-<version>` form and it already folds the
    // assessment content key, so the gate reads its version segment rather than restating the number. Only the tool
    // prefix is spelled here — the grammar, not the pin.
    const string SolverPrefix = "energyplus-";

    public static readonly EnergyToolchainPolicy Canonical = new(
        ConfiguredDir: None,
        ExecutableName: OperatingSystem.IsWindows() ? "energyplus.exe" : "energyplus",
        ExpectedVersion: AssessmentRoute.EnergyPlus.SolverVersion[SolverPrefix.Length..]);
}

// AssessmentRequest.Energy weather: the EPW the subprocess runs over (-w) and the OSM WeatherFile embeds.
public sealed record WeatherRef(string EpwPath, string Station);

// Simulation scenario carries route row, toolchain, EUI target (kWh/m2.a), conditioning, internal-load defaults the
// build reads, and the typed-results egress. Every scalar column is a knob a consumer re-targets per climate/building-type
// without touching the builder; EnergyRoute owns provider selection as deployment data, never a second entry.
// Results is DELIBERATELY absent from the assessment#REQUEST_FAMILY energy CanonicalBytes arm, which enumerates the
// scalar identity columns: where a result set LANDS is composition, not derivation, so binding a different consumer
// must never re-key a run and force a metered re-simulation.
public sealed record EnergyPolicy(
    EnergyRoute Route, EnergyToolchainPolicy Toolchain, double TargetEui,
    double HeatingSetpointC, double CoolingSetpointC, double LightingPowerWM2, double EquipmentPowerWM2,
    EnergyResultsPort Results) {
    public static readonly EnergyPolicy Canonical = new(
        EnergyRoute.Local, EnergyToolchainPolicy.Canonical, TargetEui: 0.0,
        HeatingSetpointC: 20.0, CoolingSetpointC: 26.0, LightingPowerWM2: 8.0, EquipmentPowerWM2: 10.0,
        EnergyResultsPort.Unbound);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class EnergyToolchain {
    public static Fin<string> Resolve(EnergyToolchainPolicy policy) {
        Option<string> resolved =
            Probe(Environment.GetEnvironmentVariable("ENERGYPLUS_EXE"))
            | Probe(Join(Environment.GetEnvironmentVariable("OPENSTUDIO_ENERGYPLUSDIR"), policy.ExecutableName))
            | policy.ConfiguredDir.Bind(dir => Probe(Join(dir, policy.ExecutableName)))
            | Probe(Join(BundledRuntimeDir(), policy.ExecutableName));
        return resolved.ToFin(new ComputeFault.ToolchainUnresolved(
            $"<energyplus-not-found:ENERGYPLUS_EXE->OPENSTUDIO_ENERGYPLUSDIR->configured({policy.ConfiguredDir})->bundled>"));
    }

    // EnergyPlus binary is the version authority, not its path: self-reporting through `--version` makes the
    // gate probes the executable rather than grepping the path (a version-named directory is no guarantee). The gate
    // GOVERNS execution: a reported mismatch rails ToolchainUnresolved before any model build or run — a skewed solver
    // never consumes the translated IDF — while an UNDETERMINED probe (launch failure, empty banner) degrades to a
    // warning fact the result carries, so version evidence survives every admitted run.
    public static Fin<Seq<AssessmentFact>> VersionGate(string executable, EnergyToolchainPolicy policy) {
        string reported = ProbeVersion(executable);
        return reported.Contains(policy.ExpectedVersion, StringComparison.Ordinal)
            ? Fin.Succ(Seq<AssessmentFact>())
            : reported.StartsWith("<version-", StringComparison.Ordinal)
                ? Fin.Succ(Seq(AssessmentFact.Text("energyplus-version-warning", $"<undetermined:{reported}:{executable}>")))
                : Fin.Fail<Seq<AssessmentFact>>(new ComputeFault.ToolchainUnresolved(
                    $"<energyplus-version-mismatch:expected={policy.ExpectedVersion}:reported={reported}:{executable}>"));
    }

    // Run `<executable> --version` and read the banner (Exemption: native subprocess); a launch failure yields a typed
    // marker so the guard reports an undetermined version, never a false match. ArgumentList escapes the args, no shell.
    static string ProbeVersion(string executable) {
        try {
            using Process probe = new() {
                StartInfo = new ProcessStartInfo(executable) {
                    ArgumentList = { "--version" }, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
                },
            };
            probe.Start();
            Task<string> stderrDrain = probe.StandardError.ReadToEndAsync();   // both pipes drain — the RunSubprocess two-stream law
            string banner = probe.StandardOutput.ReadToEnd().Trim();
            probe.WaitForExit();
            _ = stderrDrain.GetAwaiter().GetResult();
            return banner.Length > 0 ? banner : "<version-unreported>";
        }
        // Same guard set the run entry takes: a SWIG-adjacent launch raises across both hierarchies, and a probe that
        // caught the narrower one reported an undetermined version on one host and escaped the Fin rail on another.
        catch (Exception ex) when (ex is SystemException or ApplicationException) { return $"<version-probe-failed:{ex.GetType().Name}>"; }
    }

    static Option<string> Probe(string? path) => path is not null && File.Exists(path) ? Some(path) : None;
    static string? Join(string? dir, string exe) => dir is null ? null : Path.Combine(dir, exe);
    // Last-resort probe: the app's own RID-native runtimes/<rid>/native location (the .NET native-asset convention) —
    // the RID reads off the running host, so one probe serves every publish target and a re-targeted build needs no edit.
    // The OpenStudio SWIG package bundles no solver; a dev box resolves earlier via OPENSTUDIO_ENERGYPLUSDIR.
    static string BundledRuntimeDir() =>
        Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier, "native", "EnergyPlus");
}
```

## [03]-[MODEL_BUILD]

- Owner: `EnergySimulation.BuildModel` the in-process OpenStudio model builder; `OsmBuild` the build receipt (IDF path, the run's `ArtifactKey`, the `ZoneTarget` roster, translator-and-skip log facts); `ZoneTarget` the per-zone result address (the tabular row name beside the `ResultScope` the zone's rows key on); `ConfigureRun`/`SetpointSchedules`/`InternalLoads`/`Condition`/`BuildSurface`/`BuildOpenings`/`BuildConstruction`/`Layer`/`Vertices` the model-object folds; the SWIG `Optional<T>`→`Fin<T>`, `IDisposable`, and `Path` boundary discipline.
- Entry: `static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch, Instant at)` guards the weather EPW, builds a `Model`, stamps the annual context, folds each spatial node into a named `Space`+`ThermalZone` pair, each bounding surface into a `Surface` with its resolved footprint and layered `Construction`, each `Host`-attributed opening into a typed `SubSurface`, forward-translates to the IDF `Workspace`, and mints the saved IDF's `EnergyArtifact` address, `Fin<T>` lowering a missing weather/composition or a translator error onto `ComputeFault.AssessmentInputMissing`/`AnalysisFailed`.
- Auto: every OpenStudio file API takes a SWIG `Path` (no `Path(string)` ctor), so paths route through `OpenStudioUtilitiesCore.toPath`; the unique `SimulationControl`/`RunPeriod` objects are gotten-or-created through the static `OpenStudioModelSimulation.get*(model)` module functions (neither carries a `(Model)` ctor, and the binding surfaces these as module functions, not `Model` instance methods); the construction fold discriminates on the seam property case — an all-`Optical` set builds `StandardGlazing` layers, any other builds `StandardOpaqueMaterial` through the 6-arg ctor (the shorter ctor forms backfill OpenStudio defaults for the omitted thermal columns — fabricated physics, the rejected admission) so the OSM U-value matches the `Analysis/aggregator` ISO 6946 fold, while absent or mixed compositions rail typed; every load/get that can miss returns a SWIG `Optional<T>` checked with `is_initialized()` before `get()`.
- Packages: NREL.OpenStudio.macOS-arm64, LanguageExt.Core, NodaTime, Rasm.Bim (project — the `Energy/exchange#ENERGY_EXCHANGE` `EnergyArtifact`/`ArtifactKey` address and the `InterchangeFormat` row the translated IDF keys under), Rasm.Element (project — `ElementGraph`, `Node.Object`, `MaterialComposition`, `MaterialPropertySet.Thermal`/`.Mechanical`/`.Optical` via `MaterialPropertyAccess`, `MaterialLayer`, `NodeId`, `RepresentationContentHash`, `FootprintPolygon`, the host-neutral `Vector3` its ring carries, `GeometrySource` the analytical-surface resolution port), BCL inbox.
- Growth: a new model object (HVAC plant, schedule set, daylighting control, infiltration object) is one fold over the matching nodes; conditioning widens from ideal-air to a sized plant by one `EnergyPolicy` selector; `SimpleGlazing` is the assembly-shorthand row one `Layer` arm adds when a whole-window U/SHGC case rides the seam — the build widens by fold and policy column, never a parallel builder per object type.
- Boundary: the model is built from the seam graph for SIMULATION, distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange — Compute reads the graph's already-lowered spaces/surfaces/constructions, never re-authored from IFC. A bounding-surface node carries its OWN `MaterialComposition.LayerSet` — the `Rasm.Bim/Energy/projector#ENERGY_PROJECTOR` raise associates the layer set to the boundary surface itself, opaque plies through `MaterialPropertySet.Thermal` and glazing through `Optical` — so `BuildSurface` reads the surface node directly and NEVER joins to the bounded wall or slab; a join would read the host element's full assembly where the boundary carries the space-facing composition the simulation needs. Every OpenStudio wrapper is `IDisposable` and bracketed under `using` (the `Model`, translator, `Workspace`, `EpwFile`, every point/vector/log-vector, the result optionals) — a dropped handle leaks native memory the GC cannot reclaim; a model-object is owned BY the `Model` it is `new`-ed against and never independently disposed; model mutation is single-threaded so the build is one serialized unit, never a parallel fan-out; the `*PINVOKE` marshaling classes are never a call surface. Absent, non-layered, and mixed compositions rail `AssessmentInputMissing`; an OpenStudio default fabricates building-envelope conductance. Fenestration constructions land only on `SubSurface` openings — EnergyPlus rejects one on a base surface, which is why `BoundingSurfacesOf` excludes the `Host`-attributed opening boundaries. One `ThermalZone` per space makes the zone roster the SPACE roster, so a zone's results address the space's `GlobalId` wherever the graph carries one and fall back to the authored zone name otherwise — the finest identity the graph supports, never both scopes for one physical row.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// One zone's result address: Row is the tabular RowName the SQL store holds (EnergyPlus renders every zone name
// UPPER-CASED there), Scope the Bim granularity its rows key on. The two spellings ride ONE row because they are the
// same zone read from two sides, and a second name table is where the SQL lookup and the published scope drift apart.
public readonly record struct ZoneTarget(string Row, ResultScope Scope);

// No live OpenStudio handle escapes the boundary. Artifact addresses the TRANSLATED IDF — the energy model the run
// consumes — so every published magnitude keys the document it was produced from; TranslatorLog folds the
// forward-translate warnings and errors.
public sealed record OsmBuild(string IdfPath, ArtifactKey Artifact, Seq<ZoneTarget> Zones, Seq<AssessmentFact> TranslatorLog);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    // Energy SI dimension composed from the seam Dimension algebra (force x length); EUI divides by area. No hand-mapped
    // kind; the magnitude coerces GJ->J through UnitsNet once (never a literal factor). EnergyPlus reports every value in GJ.
    static readonly Dimension EnergyDim = Dimension.ForceDim.Multiply(Dimension.LengthDim);
    static readonly Dimension EuiDim = EnergyDim.Divide(Dimension.AreaDim);
    static double Joules(double gigajoules) => UnitsNet.Energy.FromGigajoules(gigajoules).Joules;

    static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch, Instant at) {
        if (!File.Exists(request.Weather.EpwPath)) {
            return Fin.Fail<OsmBuild>(new ComputeFault.AssessmentInputMissing($"<energy-weather-missing:{request.Weather.EpwPath}>"));
        }
        using OpenStudio.Model model = new();
        return ConfigureRun(model, request).Bind(_ => {
            OpenStudio.SpaceType spaceType = InternalLoads(model, request.Policy);
            (OpenStudio.ScheduleConstant Heating, OpenStudio.ScheduleConstant Cooling) setpoints = SetpointSchedules(model, request.Policy);
            // The fold threads the zone roster BESIDE the notes: a zone named on one pass and addressed on another is
            // exactly how a per-zone result lands against the wrong space, so the name and its scope are minted together.
            Fin<(Seq<AssessmentFact> Notes, Seq<ZoneTarget> Zones)> built = graph.SpacesOf(request.Targets).Fold(
                Fin.Succ((Notes: Seq<AssessmentFact>(), Zones: Seq<ZoneTarget>())),
                (spaces, space) => spaces.Bind(state => {
                    OpenStudio.Space osSpace = new(model);
                    osSpace.setName(space.Name);
                    osSpace.setSpaceType(spaceType);
                    OpenStudio.ThermalZone zone = new(model);
                    osSpace.setThermalZone(zone);
                    // Name the zone off the space NODE id, never its authored name: two spaces may share a label, and
                    // OpenStudio silently uniquifies a collision — so the assigned name is read BACK rather than assumed.
                    zone.setName($"{ZonePrefix}{space.Id.Value}");
                    ZoneTarget target = new(
                        zone.nameString().ToUpperInvariant(),
                        space.ExternalId.Match(
                            Some: static gid => (ResultScope)new ResultScope.Space(gid),
                            None: () => new ResultScope.Zone(zone.nameString())));
                    if (graph.IsConditioned(space.Id)) { Condition(model, zone, setpoints.Heating, setpoints.Cooling); }
                    return graph.BoundingSurfacesOf(space.Id).Fold(
                        Fin.Succ(state with { Zones = state.Zones.Add(target) }),
                        (surfaces, surface) => surfaces.Bind(current =>
                            BuildSurface(model, osSpace, space.Id, surface, graph, geometry)
                                .Map(next => current with { Notes = current.Notes + next })));
                }));
            return built.Bind(state => {
                using OpenStudio.EnergyPlusForwardTranslator translator = new();
                using OpenStudio.Workspace idf = translator.translateModel(model);
                using OpenStudio.LogMessageVector errors = translator.errors();
                using OpenStudio.LogMessageVector warnings = translator.warnings();
                Seq<AssessmentFact> log = state.Notes
                    + toSeq(Enumerable.Range(0, (int)errors.Count)).Map(i => AssessmentFact.Text($"osm-error-{i}", errors[i].logMessage()))
                    + toSeq(Enumerable.Range(0, (int)warnings.Count)).Map(i => AssessmentFact.Text($"osm-warning-{i}", warnings[i].logMessage()));
                if (errors.Count > 0) { return Fin.Fail<OsmBuild>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<osm-forward-translate-errors:{errors.Count}>")); }
                string idfPath = Path.Combine(scratch, "in.idf");
                using OpenStudio.Path outPath = OpenStudio.OpenStudioUtilitiesCore.toPath(idfPath);
                // Address the model the run CONSUMES, off the bytes actually written: the saved IDF is the producer's
                // own octets, which is the only run the key may claim (the Bim producer-bytes law). Graph pedigree stays
                // None — that column joins a Bim-LOWERED document, and the results join runs on the address alone.
                return idf.save(outPath, overwrite: true)
                    ? Fin.Succ(new OsmBuild(idfPath,
                        EnergyArtifact.Of(InterchangeFormat.Idf, File.ReadAllBytes(idfPath), None, at).Address,
                        state.Zones, log))
                    : Fin.Fail<OsmBuild>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Foreign, $"<osm-idf-save-failed:{idfPath}>"));
            });
        });
    }

    // Zone names carry the space node id so the SQL row name resolves back to one space with no authored-label join.
    const string ZonePrefix = "rasm-zone-";

    // Annual context runs the weather-file period through SimulationControl, spans the year through RunPeriod, and uses EpwFile as the
    // design context (-w is the authoritative run weather), FullExterior solar distribution avoids the interior variants'
    // zone-convexity requirement on imported geometry. SimulationControl/RunPeriod/OutputTableSummaryReports have no
    // (Model) ctor — the binding surfaces their get-or-create as static OpenStudioModelSimulation.get*(model) functions.
    static Fin<Unit> ConfigureRun(OpenStudio.Model model, AssessmentRequest.Energy request) {
        OpenStudio.SimulationControl control = OpenStudio.OpenStudioModelSimulation.getSimulationControl(model);
        control.setRunSimulationforWeatherFileRunPeriods(true);
        control.setSolarDistribution("FullExterior");
        OpenStudio.RunPeriod run = OpenStudio.OpenStudioModelSimulation.getRunPeriod(model);
        run.setBeginMonth(1); run.setBeginDayOfMonth(1); run.setEndMonth(12); run.setEndDayOfMonth(31);
        // Annual SqlFile readers depend on ABUPS and End-Uses reports, emitted only when AllSummary is
        // requested — armed here (get-or-create, idempotent) so a result read never rides an ambient translator default.
        OpenStudio.OpenStudioModelSimulation.getOutputTableSummaryReports(model).addSummaryReport("AllSummary");
        using OpenStudio.Path epwPath = OpenStudio.OpenStudioUtilitiesCore.toPath(request.Weather.EpwPath);
        using OpenStudio.EpwFile epw = new(epwPath);
        using OpenStudio.OptionalWeatherFile attached = OpenStudio.WeatherFile.setWeatherFile(model, epw);
        return attached.is_initialized()
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing($"<energy-weather-attach-failed:{request.Weather.EpwPath}>"));
    }

    // Constant heating/cooling schedules from the policy comfort band, one pair shared across every conditioned zone
    // (model-owned); a ScheduleConstant IS a Schedule, so it admits to the dual-setpoint thermostat.
    static (OpenStudio.ScheduleConstant Heating, OpenStudio.ScheduleConstant Cooling) SetpointSchedules(OpenStudio.Model model, EnergyPolicy policy) {
        OpenStudio.ScheduleConstant heating = new(model);
        heating.setName("rasm-heating-setpoint");
        heating.setValue(policy.HeatingSetpointC);
        OpenStudio.ScheduleConstant cooling = new(model);
        cooling.setName("rasm-cooling-setpoint");
        cooling.setValue(policy.CoolingSetpointC);
        return (heating, cooling);
    }

    // Ideal-air conditioning to the dual setpoints: the minimal envelope-study system, so demand is the envelope-driven
    // load, never a free-floating zero — a sized HVAC plant is the growth axis one policy selector widens to.
    static void Condition(OpenStudio.Model model, OpenStudio.ThermalZone zone, OpenStudio.ScheduleConstant heating, OpenStudio.ScheduleConstant cooling) {
        zone.setUseIdealAirLoads(true);
        OpenStudio.ThermostatSetpointDualSetpoint thermostat = new(model);
        thermostat.setHeatingSetpointTemperatureSchedule(heating);
        thermostat.setCoolingSetpointTemperatureSchedule(cooling);
        zone.setThermostatSetpointDualSetpoint(thermostat);
    }

    // Policy internal gains as one SpaceType (lighting + equipment power density) on every space, so the EUI carries the
    // plug+lighting load an envelope-only model omits — the densities are explicit policy knobs, not fabricated constants.
    static OpenStudio.SpaceType InternalLoads(OpenStudio.Model model, EnergyPolicy policy) {
        OpenStudio.SpaceType spaceType = new(model);
        spaceType.setName("rasm-space-type");
        spaceType.setLightingPowerPerFloorArea(policy.LightingPowerWM2);
        spaceType.setElectricEquipmentPowerPerFloorArea(policy.EquipmentPowerWM2);
        return spaceType;
    }

    // Geometry and construction admission rails when a host or opening lacks required evidence; omitting either shape or
    // material stack opens the zone or fabricates heat flow, invalidating every downstream energy total.
    static Fin<Seq<AssessmentFact>> BuildSurface(OpenStudio.Model model, OpenStudio.Space space, NodeId spaceId, Node.Object surface, ElementGraph graph, GeometrySource geometry) =>
        // Footprints resolve one hop by content key through `GeometrySource`; an absent decode rails because no legal
        // `OpenStudio.Surface` can represent the boundary without its ring.
        geometry.Footprint(surface.Representations)
            .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-surface-footprint-unresolved:{surface.Id.Value}>"))
            .Bind(footprint => {
                using OpenStudio.Point3dVector vertices = Vertices(footprint);
                OpenStudio.Surface osSurface = new(vertices, model);              // owned by the Model
                osSurface.setSpace(space);
                return graph.CompositionOf(surface.Id)
                    .Bind(static composition => composition is MaterialComposition.LayerSet set ? Some(set) : None)
                    .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-surface-layerset-unresolved:{surface.Id.Value}>"))
                    .Bind(set => BuildConstruction(model, set, graph))
                    .Bind(construction => {
                        osSurface.setConstruction(construction);
                        return BuildOpenings(model, osSurface, spaceId, surface.ExternalId.IfNone(surface.Name), graph, geometry);
                    });
            });

    // Host-attributed openings land as SubSurfaces on their host Surface — EnergyPlus accepts a fenestration construction
    // only on a sub-surface (BoundingSurfacesOf excludes the opening edges). IfcWindow -> FixedWindow, or Skylight on a
    // RoofCeiling host (EnergyPlus validates the type against host tilt, so the host's surfaceType() is the discriminant),
    // else Door; the construction builds through the same BuildConstruction fold off the opening's composition.
    static Fin<Seq<AssessmentFact>> BuildOpenings(OpenStudio.Model model, OpenStudio.Surface host, NodeId spaceId, string hostIdentifier, ElementGraph graph, GeometrySource geometry) =>
        graph.OpeningsOf(spaceId, hostIdentifier).TraverseM(opening =>
            geometry.Footprint(opening.Representations)
                .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-opening-footprint-unresolved:{opening.Id.Value}>"))
                .Bind(ring => {
                    using OpenStudio.Point3dVector vertices = Vertices(ring);
                    OpenStudio.SubSurface sub = new(vertices, model);             // owned by the Model
                    sub.setSurface(host);
                    sub.setSubSurfaceType(opening.Classification.Code == EnergyGraphReads.WindowClass
                        ? (host.surfaceType() == "RoofCeiling" ? "Skylight" : "FixedWindow")
                        : "Door");
                    return graph.CompositionOf(opening.Id)
                        .Bind(static composition => composition is MaterialComposition.LayerSet set ? Some(set) : None)
                        .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-opening-layerset-unresolved:{opening.Id.Value}>"))
                        .Bind(set => BuildConstruction(model, set, graph))
                        .Map(construction => {
                            sub.setConstruction(construction);
                            return Seq<AssessmentFact>();
                        });
                })).As().Map(static rows => rows.Fold(Seq<AssessmentFact>(), static (facts, row) => facts + row));

    // One construction fold, the seam property case the discriminant: all-Optical is fenestration, Optical-free is opaque,
    // a mixed set has no legal EnergyPlus construction and rails as missing assessment evidence.
    static Fin<OpenStudio.Construction> BuildConstruction(OpenStudio.Model model, MaterialComposition.LayerSet set, ElementGraph graph) =>
        set.Layers
            .TraverseM(layer => graph.Material(layer.Material)
                .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-layer-material-absent:{layer.Material.Value}>"))
                .Map(node => (Layer: layer, Props: node.Properties))).As()
            .Bind(rows => rows.Exists(static r => r.Props.Optical.IsSome) && !rows.ForAll(static r => r.Props.Optical.IsSome)
                ? Fin.Fail<Seq<(MaterialLayer Layer, Seq<MaterialPropertySet> Props)>>(
                    new ComputeFault.AssessmentInputMissing("<osm-mixed-opaque-glazing-layerset>"))
                : Fin.Succ(rows))
            .Bind(rows => rows.TraverseM(r => Layer(model, r.Layer, r.Props)).As())
            .Map(materials => {
                using OpenStudio.MaterialVector vec = new(materials);
                OpenStudio.Construction construction = new(model);                 // owned by the Model
                construction.setLayers(vec);
                return construction;
            });

    // One layer admission, the seam property case the arm: Optical -> StandardGlazing (the nine [0,1] spectral fractions
    // through the normal-incidence/hemispherical setters, Thermal conductivity when carried), else Thermal -> the 6-arg
    // StandardOpaqueMaterial. Shorter ctor forms backfill OpenStudio defaults for the omitted thermal columns (fabricated
    // physics, rejected), so both arms set every physical column from seam evidence with the neutral MediumRough roughness.
    static Fin<OpenStudio.Material> Layer(OpenStudio.Model model, MaterialLayer layer, Seq<MaterialPropertySet> props) =>
        props.Optical.Match(
            Some: optical => props.Thermal
                .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-glazing-missing-thermal:{layer.Material.Value}>"))
                .Map(thermal => {
                    OpenStudio.StandardGlazing glass = new(model, "SpectralAverage", layer.Thickness.Si);
                    glass.setSolarTransmittanceatNormalIncidence(optical.SolarTransmittance);
                    glass.setFrontSideSolarReflectanceatNormalIncidence(optical.SolarReflectanceFront);
                    glass.setBackSideSolarReflectanceatNormalIncidence(optical.SolarReflectanceBack);
                    glass.setVisibleTransmittanceatNormalIncidence(optical.VisibleTransmittance);
                    glass.setFrontSideVisibleReflectanceatNormalIncidence(optical.VisibleReflectanceFront);
                    glass.setBackSideVisibleReflectanceatNormalIncidence(optical.VisibleReflectanceBack);
                    glass.setInfraredTransmittanceatNormalIncidence(optical.ThermalIrTransmittance);
                    glass.setFrontSideInfraredHemisphericalEmissivity(optical.ThermalIrEmissivityFront);
                    glass.setBackSideInfraredHemisphericalEmissivity(optical.ThermalIrEmissivityBack);
                    glass.setThermalConductivity(thermal.Conductivity.Si);
                    return (OpenStudio.Material)glass;
                }),
            None: () =>
                from thermal in props.Thermal.ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-layer-missing-thermal:{layer.Material.Value}>"))
                from mechanical in props.Mechanical.ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-layer-missing-density:{layer.Material.Value}>"))
                select (OpenStudio.Material)new OpenStudio.StandardOpaqueMaterial(model, "MediumRough",
                    layer.Thickness.Si, thermal.Conductivity.Si, mechanical.Density.Si, thermal.SpecificHeat.Si));

    // Build the OpenStudio vertex vector from the seam Vector3 ring — each Point3d disposed immediately after Add (the
    // vector copies it), so the marshaling leaks nothing and never escapes the Compute energy boundary as an OSM type.
    static OpenStudio.Point3dVector Vertices(FootprintPolygon footprint) {
        OpenStudio.Point3dVector vec = new();
        foreach (Vector3 p in footprint.Ring) { using OpenStudio.Point3d point = new(p.X, p.Y, p.Z); vec.Add(point); }
        return vec;
    }
}
```

## [04]-[SIMULATION_RUN]

- Owner: `EnergySimulation.RunLocal` the subprocess arm; `RunSubprocess` the EnergyPlus subprocess; `EnergyResultsPort` the typed-row egress the composition root binds; `EnergyReadout` the two-outcome result carrier; `ResultContext`/`ResultPoint` the read's addressing and mint carriers; `ResultRows` the ONE correspondence table from the OpenStudio end-use vocabulary onto the Bim result axes; `ReadResults`/`HeadRows`/`Cells`/`Points`/`PeakDemand`/`TabularRows`/`UnmetPoints`/`Tabular`/`Rows`/`SummaryFacts`/`ValidityFacts`/`GoverningEui`/`Lower` the `SqlFile` result read shared by both routes; the scratch run-directory lifetime.
- Entry: `static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, IClock clock)` resolves the binary through `EnergyToolchain.Resolve`, builds the OSM model and IDF, runs the subprocess over the scratch directory, reads `eplusout.sql` through `SqlFile`, publishes the typed rows through the policy's `EnergyResultsPort`, and returns the summary `AssessmentResult`, bracketing the scratch directory and every native handle.
- Auto: the subprocess is `energyplus -w <weather> -d <outdir> -r <idf>`; a non-zero exit rails `ComputeFault.AnalysisFailed` with the stderr tail. The end-use read folds the structured `SqlFile.endUses()` summary CELL BY CELL over the static `EndUses.fuelTypes()`/`categories()` vectors (each handle bracketed, the SWIG marshaling exemption) — one `EnergyResult` per non-zero `(fuel, end-use)` point, its annual magnitude from `getEndUse(fuel, category)` and its peak from the annual maximum of `peakEnergyDemandByMonth(fuel, category, month)`, the `Water` fuel (m³ consumption) excluded before the mapping. Whole-building magnitudes ride the same axes at their un-disaggregated members (`ResultFuel.Total` × `ResultEndUse.Whole`), EUI as the `Intensity` measure over a positive conditioned area only, so a zero-area set carries no intensity row and the verdict bands `NotApplicable` rather than a fabricated 0.0-EUI Satisfied. Per-zone unmet hours read ONE `SystemSummary` `Time Setpoint Not Met` table parameterized on its row name — `Facility` for the building scope, the upper-cased zone name for each `ZoneTarget` — so a per-zone read is one roster row rather than a second query family. Every magnitude mints through the Bim `EnergyResult.Of` gate, which rails a non-finite SI value before a row exists; the GJ→J and hours→s coercions ride `UnitsNet` once, and the verdict converts the SI EUI back to kWh·m⁻²·a⁻¹ against the policy target, projecting `double.NaN` when no target is carried.
- Output: `EnergyReadout` carries BOTH outcomes — the typed `Seq<EnergyResult>` the `Rasm.Bim/Energy/results#RESULTS_ADMISSION` `EnergyResults.Admit` lands as `Pset_EnergyResults` bags, and the `Seq<AssessmentFact>` summary stream the spine verdicts. The rows egress through `EnergyResultsPort` rather than the `AssessmentResult`, because the spine's carrier is a uniform fact stream every discipline shares and widening it for one discipline's typed wire is the rejected form.
- Receipt: the `Assessment` `ComputeReceipt` case carries the energy discipline/route/content-key with elapsed wall time; translator warnings and an undetermined version probe fold in as soft facts, while construction or reported-version failures rail before simulation.
- Packages: Microsoft.Data.Sqlite (the read-only tabular reader for the setpoint-not-met rows the SWIG `SqlFile` exposes no accessor for; folder admission on this first compose, the central pin held), NREL.OpenStudio.macOS-arm64 (the `SqlFile` totals + structured `EndUses` fold + `peakEnergyDemandByMonth` + `hoursSimulated` + the static run-context helpers), UnitsNet (the GJ→J / J→kWh / hours→s coercions), Rasm.Bim (project — the `Energy/results#RESULTS_ADMISSION` `EnergyResult`/`ResultQuantity`/`ResultScope`/`ResultFuel`/`ResultEndUse`/`ResultMeasure` vocabulary and the `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey`), LanguageExt.Core, NodaTime, Rasm.Element (project — `ElementGraph`, `Dimension`, `MeasureValue`, `PropertyValue`, `NodeId`), BCL inbox.
- Growth: a new published magnitude is one `ResultPoint` on whichever AXIS it widens, and the axis row itself lands at the Bim owner — an ASHRAE-55 comfort tally is one `SystemSummary` read onto the settled `ResultMeasure.ComfortHours` row, sub-annual demand shape one fold over `SqlFile.energyConsumptionByMonth`; a newly-metered fuel or service is one `ResultRows` row here beside its Bim axis row, and until both land the token DEGRADES with a named fact carrying its own magnitude.
- Boundary: the EnergyPlus binary is the resolved subprocess (OpenStudio does not run it), so the runner owns the process lifetime, scratch directory, and stderr capture, bracketed in `try-finally` (Exemption: native subprocess + filesystem); the model build and SQL read are the single-threaded native boundary; every OpenStudio handle is disposed; the SQL accessors return SWIG `OptionalDouble` lowered to `Option<double>`, never a bare `get()` faulting in native code. The FUEL AXIS SURVIVES the read — a per-category all-fuel sum publishes the same heating row for a district-heated building and an all-electric one and no consumer can recover which fuel carried it, so the fold mints per cell and only the `Interior`/`Exterior` lighting and equipment pairs collapse, onto the one Bim SERVICE row each names. Source and net-source energy carry no axis point — the axes name the fuel and the service, never the accounting basis — so they stay summary facts beside the typed rows, and the SITE total is the one row that also rides the axis at `Total`×`Whole`. An out-of-roster fuel or category token lands a named degrade fact carrying its own magnitude, never a silent drop and never a fabricated axis member. A non-zero exit or a missing SQL file rails `AnalysisFailed`, never a silent zero-energy result; a missing per-zone unmet row rails too, because a report cannot distinguish a zone that met its setpoints from a zone whose row never landed.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Typed-results egress the composition root binds — the AssessmentSink dual for the Bim graph landing rather than the
// blob lane. Unbound is the STATED opt-out a verdict-only composition binds: the rows still mint and still rail their
// own admission, they simply reach no consumer by declaration rather than by an omission nobody can see.
public sealed record EnergyResultsPort(Func<Seq<EnergyResult>, Fin<Unit>> Publish) {
    public static readonly EnergyResultsPort Unbound = new(static _ => Fin.Succ(unit));
}

// One published magnitude before admission — three axis members and the SI scalar — so every read site hands the SAME
// shape to one gate; a per-site EnergyResult.Of call re-spells the axis order at each of six read points.
public readonly record struct ResultPoint(ResultMeasure Measure, ResultFuel Fuel, ResultEndUse Use, double Si);

// --- [MODELS] ------------------------------------------------------------------------------
// Everything a result read needs to ADDRESS its rows: the consumed model's key and the zone roster. A cloud run carries
// an empty roster, because the recipe names its own zones and a correspondence Compute never authored is a guess.
public readonly record struct ResultContext(ArtifactKey Artifact, Seq<ZoneTarget> Zones);

// Two outcomes off ONE read. Splitting the read in two walks the SqlFile twice and lets the typed rows and the verdict
// facts disagree about the same run.
public sealed record EnergyReadout(Seq<EnergyResult> Results, Seq<AssessmentFact> Facts);

// --- [TABLES] ------------------------------------------------------------------------------
// ONE correspondence from the OpenStudio EndUses vocabulary onto the Bim result axes. Interior and Exterior rows fold
// onto a single member because the axis names the SERVICE and not the meter, so the pair sums before minting; a token
// in neither index degrades carrying its own magnitude. Growth is a row here beside its axis row at the Bim owner.
static class ResultRows {
    internal const string WaterFuel = "Water";   // metered in m3, never energy — excluded before the mapping runs

    internal static readonly (string Token, ResultFuel Fuel)[] FuelRows = [
        ("Electricity",     ResultFuel.Electricity),
        ("Gas",             ResultFuel.NaturalGas),
        ("DistrictHeating", ResultFuel.DistrictHeating),
        ("DistrictCooling", ResultFuel.DistrictCooling),
    ];

    internal static readonly (string Token, ResultEndUse Use)[] EndUseRows = [
        ("Heating",           ResultEndUse.Heating),
        ("Cooling",           ResultEndUse.Cooling),
        ("InteriorLights",    ResultEndUse.Lighting),
        ("ExteriorLights",    ResultEndUse.Lighting),
        ("InteriorEquipment", ResultEndUse.Equipment),
        ("ExteriorEquipment", ResultEndUse.Equipment),
        ("Fans",              ResultEndUse.Fans),
        ("Pumps",             ResultEndUse.Pumps),
        ("WaterSystems",      ResultEndUse.WaterSystems),
    ];

    internal static readonly FrozenDictionary<string, ResultFuel> ByFuel =
        FuelRows.ToFrozenDictionary(static r => r.Token, static r => r.Fuel, StringComparer.Ordinal);

    internal static readonly FrozenDictionary<string, ResultEndUse> ByEndUse =
        EndUseRows.ToFrozenDictionary(static r => r.Token, static r => r.Use, StringComparer.Ordinal);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, IClock clock) {
        // Native and subprocess boundary rails onto Fin: a SWIG ctor, model mutation, Process.Start over a bad binary,
        // or a corrupt SqlFile throws a SystemException the entry owes the caller as AnalysisFailed, never an escape. Scratch
        // creation is inside the bracket and cleanup is best-effort, so a delete fault never masks the run's Fin result.
        string scratch = "";
        // ONE instant per run: the artifact mint, every result row, and the provenance read it, so a result set carries
        // the simulation's stamp rather than whichever moment each fold happened to reach the clock.
        Instant at = clock.GetCurrentInstant();
        try {
            scratch = Directory.CreateTempSubdirectory("rasm-eplus-").FullName;
            return from binary in EnergyToolchain.Resolve(request.Policy.Toolchain)
                   // Version gate runs before model build or subprocess: a reported mismatch rails here and no
                   // IDF, run, or receipt exists for a skewed solver; the undetermined-probe warning rides the result.
                   from versionFacts in EnergyToolchain.VersionGate(binary, request.Policy.Toolchain)
                   from build in BuildModel(graph, request, geometry, scratch, at)
                   from sqlPath in RunSubprocess(binary, build.IdfPath, request, scratch)
                   from readout in ReadResults(sqlPath, graph, request, new ResultContext(build.Artifact, build.Zones), at)
                   // eplusout.sql bytes land through AssessmentSink before the scratch bracket deletes them
                   // — content-addressed onto the Persistence blob lane (ArtifactKind.Assessment), the key riding ResultBlob.
                   from blob in sink.Store(File.ReadAllBytes(sqlPath))
                   // Typed rows egress LAST and on the success rail alone, so a failed read never lands a partial
                   // result set the Bim admission would then treat as the run's whole answer.
                   from published in request.Policy.Results.Publish(readout.Results)
                   select AssessmentResult.Of(request.Route,
                       readout.Facts + build.TranslatorLog + versionFacts,
                       GoverningEui(readout.Facts, request.Policy),
                       new Provenance("EnergySimulation", request.Route.Standard, $"EnergyPlus {request.Policy.Toolchain.ExpectedVersion}", at),
                       Some(blob));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<AssessmentResult>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<energy-native-fault:{ex.GetType().Name}:{Tail(ex.Message)}>"));
        }
        finally { if (scratch.Length > 0) { try { Directory.Delete(scratch, recursive: true); } catch (IOException) { } } }
    }

    static Fin<string> RunSubprocess(string binary, string idfPath, AssessmentRequest.Energy request, string scratch) {
        using Process process = new() {
            // ArgumentList escapes each token, so a path with spaces (the macOS norm) round-trips intact — manual
            // quote-injection into a single Arguments string is the fragile form it replaces.
            StartInfo = new ProcessStartInfo(binary) {
                ArgumentList = { "-w", request.Weather.EpwPath, "-d", scratch, "-r", idfPath },
                RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = scratch,
            },
        };
        process.Start();
        // Both redirected pipes drain concurrently: EnergyPlus streams progress on stdout, so a redirected-but-undrained
        // stream fills its buffer and deadlocks the child against WaitForExit (stderr the evidence read, stdout discarded).
        Task<string> stderrDrain = process.StandardError.ReadToEndAsync();
        Task<string> stdoutDrain = process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();
        string stderr = stderrDrain.GetAwaiter().GetResult();
        _ = stdoutDrain.GetAwaiter().GetResult();
        string sqlPath = Path.Combine(scratch, "eplusout.sql");
        return process.ExitCode == 0 && File.Exists(sqlPath)
            ? Fin.Succ(sqlPath)
            : Fin.Fail<string>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<energyplus-exit:{Tail(stderr)}>", Some(process.ExitCode)));
    }

    // totalSiteEnergy is the required headline output (its absence is a failed run, never a silent zero). One walk of the
    // SqlFile answers both outcomes: the typed rows (whole-building heads, per-cell end uses, per-scope unmet hours) and
    // the summary facts the spine verdicts.
    static Fin<EnergyReadout> ReadResults(string sqlPath, ElementGraph graph, AssessmentRequest.Energy request, ResultContext context, Instant at) {
        using OpenStudio.Path resultsPath = OpenStudio.OpenStudioUtilitiesCore.toPath(sqlPath);
        using OpenStudio.SqlFile sql = new(resultsPath);
        return from floorAreaM2 in graph.ConditionedFloorArea(request.Targets)
            from siteGj in Lower(sql.totalSiteEnergy()).ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-total-site-energy>"))
            from sourceGj in Lower(sql.totalSourceEnergy()).ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-total-source-energy>"))
            from netSourceGj in Lower(sql.netSourceEnergy()).ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-net-source-energy>"))
            from head in HeadRows(context.Artifact, at, siteGj, floorAreaM2)
            from cells in Cells(sql, context.Artifact, at)
            from unmet in TabularRows(sqlPath, context, at)
            from facts in SummaryFacts(sql, siteGj, sourceGj, netSourceGj, floorAreaM2)
            select new EnergyReadout(head + cells.Rows + unmet, facts + cells.Notes);
    }

    // Summary stream carries what the SPINE reads and nothing more: the governing EUI pair, the accounting-basis totals
    // the axes hold no point for (source and net-source name a basis, never a fuel or a service), and the annual
    // completeness signal. Every disaggregated magnitude left this stream for the typed rows.
    static Fin<Seq<AssessmentFact>> SummaryFacts(OpenStudio.SqlFile sql, double siteGj, double sourceGj, double netSourceGj, double floorAreaM2) =>
        from intensity in floorAreaM2 > 0.0
            ? AssessmentFact.Rows(
                AssessmentFact.Measure("eui", EuiDim, Joules(siteGj) / floorAreaM2),
                AssessmentFact.Measure("source-eui", EuiDim, Joules(sourceGj) / floorAreaM2))
            : Fin.Succ(Seq<AssessmentFact>())
        from head in AssessmentFact.Rows(
            AssessmentFact.Measure("total-site-energy", EnergyDim, Joules(siteGj)),
            AssessmentFact.Measure("total-source-energy", EnergyDim, Joules(sourceGj)),
            AssessmentFact.Measure("net-source-energy", EnergyDim, Joules(netSourceGj)))
        from validity in ValidityFacts(sql)
        select head + intensity + validity;

    // Whole-building magnitudes ride the SAME axes at their un-disaggregated members, so a building total and a per-fuel
    // breakdown are one kind of fact at two grains. Site EUI is the ONE Intensity point per scope: a second Intensity row
    // under a different accounting basis collides on the Bim quantity key and faults the whole admission.
    static Fin<Seq<EnergyResult>> HeadRows(ArtifactKey artifact, Instant at, double siteGj, double floorAreaM2) {
        Seq<ResultPoint> points = Seq(new ResultPoint(ResultMeasure.Annual, ResultFuel.Total, ResultEndUse.Whole, Joules(siteGj)));
        return Rows(artifact, ResultScope.Whole, at,
            floorAreaM2 > 0.0
                ? points.Add(new ResultPoint(ResultMeasure.Intensity, ResultFuel.Total, ResultEndUse.Whole, Joules(siteGj) / floorAreaM2))
                : points);
    }

    // One result per NON-ZERO (fuel, end-use) cell of the structured SqlFile.endUses() summary. The retired fold summed
    // every fuel into one per-category number, so a district-heated building and an all-electric one published the same
    // heating row and no consumer could recover which fuel carried it. The index-loop + per-element `using` is the SWIG
    // disposal boundary (each vector indexer hands back its own handle) — the same marshaling exemption Vertices takes.
    static Fin<(Seq<EnergyResult> Rows, Seq<AssessmentFact> Notes)> Cells(OpenStudio.SqlFile sql, ArtifactKey artifact, Instant at) {
        using OpenStudio.OptionalEndUses optional = sql.endUses();
        if (!optional.is_initialized()) {
            return Fin.Fail<(Seq<EnergyResult>, Seq<AssessmentFact>)>(
                new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-end-uses>"));
        }
        using OpenStudio.EndUses uses = optional.get();
        using OpenStudio.EndUseCategoryTypeVector categories = OpenStudio.EndUses.categories();
        using OpenStudio.EndUseFuelTypeVector fuels = OpenStudio.EndUses.fuelTypes();
        Dictionary<(ResultFuel Fuel, ResultEndUse Use), (double AnnualGj, double PeakW)> cells = [];
        List<AssessmentFact> notes = [];
        for (int f = 0; f < fuels.Count; f++) {
            using OpenStudio.EndUseFuelType fuel = fuels[f];
            string fuelToken = fuel.valueName();
            if (StringComparer.Ordinal.Equals(fuelToken, ResultRows.WaterFuel)) { continue; }
            if (!ResultRows.ByFuel.TryGetValue(fuelToken, out ResultFuel? mappedFuel)) {
                notes.Add(AssessmentFact.Text("end-use-fuel-unmapped", fuelToken));
                continue;
            }
            for (int c = 0; c < categories.Count; c++) {
                using OpenStudio.EndUseCategoryType category = categories[c];
                double annualGj = uses.getEndUse(fuel, category);
                double peakW = PeakDemand(sql, fuel, category);
                if (annualGj == 0.0 && peakW == 0.0) { continue; }
                if (!ResultRows.ByEndUse.TryGetValue(category.valueName(), out ResultEndUse? mappedUse)) {
                    notes.Add(AssessmentFact.Text($"end-use-category-unmapped:{category.valueName()}", $"{fuelToken}:{annualGj}"));
                    continue;
                }
                // Interior and Exterior rows land on one service member, so the pair SUMS its annual energy and takes the
                // MAX of its peaks — two coincident peaks are one demand instant, never their arithmetic sum.
                cells[(mappedFuel, mappedUse)] = cells.TryGetValue((mappedFuel, mappedUse), out (double AnnualGj, double PeakW) held)
                    ? (held.AnnualGj + annualGj, Math.Max(held.PeakW, peakW))
                    : (annualGj, peakW);
            }
        }
        return toSeq(cells)
            .Fold(Fin.Succ(Seq<EnergyResult>()), (rail, cell) => rail.Bind(rows =>
                Rows(artifact, ResultScope.Whole, at, Points(cell.Key, cell.Value)).Map(minted => rows + minted)))
            .Map(rows => (rows, toSeq(notes)));
    }

    // A zero magnitude publishes NO row: the axis point's absence IS the zero, and the full cross product otherwise lands
    // scores of empty measures in every bag.
    static Seq<ResultPoint> Points((ResultFuel Fuel, ResultEndUse Use) axis, (double AnnualGj, double PeakW) cell) {
        Seq<ResultPoint> points = cell.AnnualGj != 0.0
            ? Seq(new ResultPoint(ResultMeasure.Annual, axis.Fuel, axis.Use, Joules(cell.AnnualGj)))
            : Seq<ResultPoint>();
        return cell.PeakW > 0.0 ? points.Add(new ResultPoint(ResultMeasure.Peak, axis.Fuel, axis.Use, cell.PeakW)) : points;
    }

    // Annual peak demand is the MAXIMUM of the twelve monthly peaks, never their sum. The typed monthly accessor keeps
    // this read off the tabular store's display-name vocabulary entirely; an absent month contributes nothing.
    // `MonthOfYear(int)` is a real SWIG ctor (beside `MonthOfYear(string)` and `value()`/`valueName()`), decompile-proven
    // on the installed assembly, so the ordinal walk needs no static enumerator.
    static double PeakDemand(OpenStudio.SqlFile sql, OpenStudio.EndUseFuelType fuel, OpenStudio.EndUseCategoryType category) {
        double peak = 0.0;
        for (int m = 1; m <= MonthsPerYear; m++) {
            using OpenStudio.MonthOfYear month = new(m);
            peak = Math.Max(peak, Lower(sql.peakEnergyDemandByMonth(fuel, category, month)).IfNone(0.0));
        }
        return peak;
    }

    const int MonthsPerYear = 12;

    // Annual simulated hours read SqlFile.hoursSimulated, the binding's one hours accessor; a full run
    // reports ~8760 h, so a short count means the solver terminated early and the energy is a partial-year artifact a
    // downstream verdict must reject; an absent hoursSimulated contributes no fact, never a fabricated zero.
    static Fin<Seq<AssessmentFact>> ValidityFacts(OpenStudio.SqlFile sql) =>
        Lower(sql.hoursSimulated())
            .ToFin((Error)new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-hours-simulated>"))
            .Bind(static hours => HoursFact("hours-simulated", hours).Map(static fact => Seq(fact)));

    // ONE SystemSummary table serves every scope, its RowName the parameter — `Facility` for the building and the
    // upper-cased zone name for each roster row, which is how EnergyPlus renders them in the tabular store. The reader
    // is Microsoft.Data.Sqlite read-only and unpooled, so it neither mutates nor locks the solver's file; SWIG SqlFile
    // exposes no accessor and no generic SQL exec for TabularDataWithStrings.
    static Fin<Seq<EnergyResult>> TabularRows(string sqlPath, ResultContext context, Instant at) {
        using Microsoft.Data.Sqlite.SqliteConnection connection = new($"Data Source={sqlPath};Mode=ReadOnly;Pooling=False;");
        connection.Open();
        return (Seq(new ZoneTarget(FacilityRow, ResultScope.Whole)) + context.Zones).Fold(
            Fin.Succ(Seq<EnergyResult>()),
            (rail, target) => rail.Bind(rows => UnmetPoints(connection, target)
                .Bind(points => Rows(context.Artifact, target.Scope, at, points))
                .Map(minted => rows + minted)));
    }

    // Unmet hours are a SERVICE fact — heating hours ride the Heating member, cooling the Cooling — so a tally lands on
    // the same axis every energy magnitude does instead of a per-metric row name. A missing row RAILS: a report cannot
    // distinguish a zone that met its setpoints from a zone whose row never landed.
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
    const string FacilityRow = "Facility";              // the whole-model row inside the same table the zone rows live in
    const string OccupiedHeating = "During Occupied Heating";
    const string OccupiedCooling = "During Occupied Cooling";

    // ONE mint for every published magnitude: the Bim EnergyResult.Of gate rails a non-finite SI value through the seam
    // MeasureValue finite check before a row exists, so no read site carries a raw double past this boundary and a
    // rejected read names the point that failed rather than the batch.
    static Fin<Seq<EnergyResult>> Rows(ArtifactKey artifact, ResultScope scope, Instant at, Seq<ResultPoint> points) =>
        points.Traverse(p => EnergyResult.Of(artifact, scope, ResultQuantity.Of(p.Measure, p.Fuel, p.Use), p.Si, at).ToValidation()).As().ToFin();

    static Option<double> Tabular(Microsoft.Data.Sqlite.SqliteConnection connection, string report, string table, string row, string column) {
        using Microsoft.Data.Sqlite.SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM TabularDataWithStrings WHERE ReportName = $report AND TableName = $table AND RowName = $row AND ColumnName = $column LIMIT 1";
        command.Parameters.AddWithValue("$report", report);
        command.Parameters.AddWithValue("$table", table);
        command.Parameters.AddWithValue("$row", row);
        command.Parameters.AddWithValue("$column", column);
        return Optional(command.ExecuteScalar()).Bind(static value => double.TryParse($"{value}", System.Globalization.CultureInfo.InvariantCulture, out double parsed) ? Some(parsed) : None);
    }

    static Fin<AssessmentFact> HoursFact(string name, double hours) => AssessmentFact.Measure(name, Dimension.DurationDim, Seconds(hours));

    static double Seconds(double hours) => UnitsNet.Duration.FromHours(hours).Seconds;

    // Governing ratio compares emitted site EUI against policy target; with no
    // target (or no eui fact) the ratio is double.NaN so the verdict bands NotApplicable, never a 0.0-ratio Satisfied.
    static double GoverningEui(Seq<AssessmentFact> facts, EnergyPolicy policy) =>
        policy.TargetEui > 0.0
            ? facts.Choose(static f => f.Name.Value == "eui" && f.Value is PropertyValue.Measure m ? Some(m.Value.Si) : None)
                .Head.Map(euiSi => UnitsNet.Energy.FromJoules(euiSi).KilowattHours / policy.TargetEui).IfNone(double.NaN)
            : double.NaN;

    // Read a SWIG OptionalDouble onto Option<double> and dispose the handle (a getter's OptionalDouble is itself
    // disposable, so a bare read leaks) — the one place a missing output becomes None, never a faulting get().
    static Option<double> Lower(OpenStudio.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    static string Tail(string s) => s.Length <= 256 ? s : s[^256..];
}

// Compute-owned ElementGraph extensions (not seam members) composing the seam primitives and the projected neutral
// Generic space-boundary edges: the seam owns the material/composition reads and the GeometrySource decode contract, the
// discipline spatial reads live here. Spaces are the IfcSpace-classified nodes reachable from the targets; bounding
// surfaces ride the projected IfcRelSpaceBoundary edges (Host-attributed openings split to OpeningsOf), each footprint
// resolved one-hop by content key through GeometrySource; the conditioned floor area sums the spaces' net area.
public static class EnergyGraphReads {
    internal const string WindowClass = "IfcWindow";    // the opening-class discriminant BuildOpenings maps to FixedWindow
    const string SpaceBoundary  = "IfcRelSpaceBoundary";
    const string SpaceClass     = "IfcSpace";
    const string SecondLevel    = "2nd";                // prefer 2nd-level (space-to-space) boundaries so a 1st+2nd export never double-counts the envelope; the "" undeclared rows read 1st-equivalent

    public static Seq<Node.Object> SpacesOf(this ElementGraph graph, Seq<NodeId> targets) =>
        targets.IsEmpty
            ? graph.ObjectNodes.Filter(IsSpace)
            : targets.Bind(t => Descend(graph, t)).Distinct().Choose(graph.Find<Node.Object>).Filter(IsSpace).ToSeq();

    // Bounding surfaces ride each space's projected IfcRelSpaceBoundary edges; a Host-attributed edge is an OPENING
    // and is excluded here (it folds as a SubSurface via OpeningsOf). When a model carries both 1st- and 2nd-level boundaries,
    // only the 2nd-level set is read so the envelope is never double-counted; a 1st-level-only or undeclared-level ""
    // model rides the 1st-equivalent arm — the secondLevel filter excludes it beside a declared set, the fallback includes it.
    public static Seq<Node.Object> BoundingSurfacesOf(this ElementGraph graph, NodeId space) {
        Seq<(Relationship.Generic Edge, Node.Object Surface)> boundaries =
            graph.EdgesAt(space).Choose(e => e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
                && g.Attribute(BoundaryRows.Host).IsNone
                ? graph.Find<Node.Object>(g.Related).Map(s => (Edge: g, Surface: s)) : None).ToSeq();
        Seq<(Relationship.Generic Edge, Node.Object Surface)> secondLevel = boundaries.Filter(static b =>
            b.Edge.Text(BoundaryRows.Level).Exists(static level => level == SecondLevel));
        return (secondLevel.IsEmpty ? boundaries : secondLevel).Map(static b => b.Surface);
    }

    // Host-attributed opening boundaries select space edges whose Host attribute names
    // host identifier; identifier matching replaces a NodeId join because rooted ids are raise-local.
    public static Seq<Node.Object> OpeningsOf(this ElementGraph graph, NodeId space, string hostIdentifier) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
                && g.Text(BoundaryRows.Host).Exists(host => host == hostIdentifier)
                ? graph.Find<Node.Object>(g.Related) : None).ToSeq();

    // Conditioned floor area is an admission rail: every conditioned space contributes a positive `NetFloorArea`, so a
    // missing denominator cannot suppress `eui` and disguise incomplete graph evidence as `NotApplicable`.
    public static Fin<double> ConditionedFloorArea(this ElementGraph graph, Seq<NodeId> targets) =>
        graph.SpacesOf(targets).Filter(s => graph.IsConditioned(s.Id)).Fold(
            Fin.Succ(0.0),
            (total, space) => total.Bind(area => graph.NetFloorAreaM2(space.Id).Map(value => area + value)));

    // Every space reads conditioned unless Pset_SpaceCommon marks it external, an absent flag included. One predicate
    // gates the OSM ideal-air conditioning and the EUI denominator, so the model and the intensity agree.
    public static bool IsConditioned(this ElementGraph graph, NodeId space) =>
        graph.Property(space, EnvelopeRows.IsExternal, Some(EnvelopeRows.SpaceCommon)).Match(
            Some: static v => v is not PropertyValue.Boolean { Value: true }, None: static () => true);

    static bool IsSpace(Node.Object o) => o.Classification.Code == SpaceClass;

    // Bag reads compose the one Analysis/assessment AnalysisReads owner over Rasm.Element-declared rows — the
    // set-scoped overload narrows to the named bag where the discipline needs it.
    static Fin<double> NetFloorAreaM2(this ElementGraph graph, NodeId space) =>
        graph.Quantity(space, QuantityRows.NetFloorArea, Some(QuantityRows.SpaceBaseQuantities)).Bind(static measure => measure.Area)
            .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<energy-space-net-floor-area-missing:{space.Value}>"));

    // Transitive descent over the owning Compose decomposition (aggregate/nest/contain) so a building/storey target reaches
    // its spaces; the non-owning Reference flavor is excluded. A path-ancestry set guards a cyclic Compose chain — this
    // descent runs before any Bake, so a corrupt graph yields an empty branch rather than an uncatchable StackOverflow.
    static Seq<NodeId> Descend(ElementGraph graph, NodeId node) => Descend(graph, node, ImmutableHashSet<NodeId>.Empty);

    static Seq<NodeId> Descend(ElementGraph graph, NodeId node, ImmutableHashSet<NodeId> ancestry) =>
        ancestry.Contains(node)
            ? Seq<NodeId>()
            : node.Cons(graph.EdgesAt(node).Choose(e => e is Relationship.Compose c && c.Whole == node && c.SubKind != ComposeKind.Reference ? Some(c.Part) : None).ToSeq()
                .Bind(child => Descend(graph, child, ancestry.Add(node))));
}
```

## [05]-[CLOUD_ROUTE]

- Owner: `EnergyRoute` the closed execution-provider `[Union]` on `EnergyPolicy` (`Subprocess` the local default · `Cloud` the Pollination row carrying owner/project/job-descriptor/platform/model-key as neutral values); `EnergySimulation.Run` the one entry whose generated total `Switch` dispatches the row; `RunCloud` the Pollination arm; `Orchestrate` the bracketed async SDK kernel.
- Entry: `public static Fin<AssessmentResult> Run(...)` dispatches `request.Policy.Route` — `Subprocess` enters `RunLocal` (`[04]`), `Cloud` enters `RunCloud`, which admits the staged model address, submits the app-authored job descriptor, watches the run to a terminal status, gates on an exact `RunStatusEnum.Succeeded` parse, pulls the result assets, locates the downloaded `eplusout.sql`, and converges on the same `ReadResults` fold `[04]` owns — one result read serves both providers, so the typed rows, fact stream, EUI verdict, and receipt shape are route-invariant.
- Auto: the HBJSON payload inside the job descriptor is the `Rasm.Bim/Energy/exchange#ENERGY_EXCHANGE` content-keyed `EnergyArtifact` the app root staged, and `ModelKey` is that artifact's own `ArtifactKey` hoisted out of the descriptor so the results join needs no descriptor parse — the column adds no content-key surface, the descriptor it is read from already folding verbatim. Downloaded assets land content-keyed on the Persistence object plane exactly as the local `eplusout.sql` does, and the assessment node keys the same `(input subgraph, route, policy)` content key, so a re-submitted identical model+recipe resolves from the Persistence index; the SDK's `Wrapper.LocalDatabase` and its path-existence `CheckCached` are not composed (path-existence reuse without hash verification is the integrity gap the content-keyed index closes).
- Receipt: the `Assessment` receipt carries the cloud provenance beside the route/content-key columns; the watch-status trail folds in as soft notes.
- Packages: PollinationSDK (the `Wrapper` job/run/asset orchestration + `RunStatusEnum` terminal vocabulary — sidecar-isolated: its vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` closure never meets the STJ rails nor loads in-Rhino), Rasm.Bim (project — `ArtifactKey.Admit` gating the staged model address), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new cloud provider is one `EnergyRoute` case with one arm (the `Switch` breaks every dispatch site at compile time); a recipe change is job-descriptor data, never a signature; per-output typed decodes beyond the SQLite widen `Orchestrate` by one asset row.
- Boundary: `Configuration`/`TokenRepo` auth is composition-root input to the ambient SDK configuration, never a policy column or fence member (the Persistence token-lifecycle law). Async orchestration is one blocking boundary kernel bracketed with the scratch directory (Exemption: sidecar HTTP + filesystem), and classification is exception-typed — an `ApiException`/HTTP transport fault maps `ComputeFault.EndpointUnreachable` while every other raise, a failed terminal, or a missing SQL asset maps `AnalysisFailed` — zero new band codes. The terminal gate PARSES the watched status against the `RunStatusEnum` token set and demands equality with `Succeeded`: a substring test over the raw string admits any status that merely contains the token and publishes a failed run's partial assets as an answer. A malformed `ModelKey` refuses at the Bim mint and lowers onto this lane's own `AssessmentInputMissing` — the address grammar is Bim's, the rail is Compute's, and a Bim band code crossing a Compute rail is the strata leak that mapping closes. Cloud rows publish BUILDING scope only, because zone naming belongs to the recipe and a correspondence Compute never authored is a guess. Artifact residency (presigned-grant transfer, `ArtifactKind.CloudRun` reuse index, PROV attribution) stays the Persistence owners' rows composed at the seam. Cloud-side model rebuild from the graph is the rejected form — cloud consumes the Bim-lowered HBJSON, local consumes the in-process OSM build, two rows on one axis.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Execution-provider case is the route; provider coordinates ride it as neutral values, with no PollinationSDK
// type enters the policy, the SDK closure stays inside the RunCloud boundary (sidecar law).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyRoute {
    private EnergyRoute() { }

    public sealed record Subprocess : EnergyRoute;

    // Owner/Project name the Pollination project; JobDescriptor is the app-authored Wrapper.JobInfo JSON (recipe ref +
    // inputs incl. the staged HBJSON artifact); Platform keys GetOutputAssets; ModelKey is the staged artifact's own
    // Bim ArtifactKey, hoisted out of the descriptor so the results join reads a typed address rather than re-parsing
    // JSON, and adding no content-key surface because the descriptor it comes from already folds verbatim. The
    // descriptor is canonical analysis identity, so it references inputs by object-plane content key and carries no
    // volatile token (no local path, signed URL, timestamp, auth material, or SDK Local* provisioning column) — a
    // volatile token over-keys the cache and silently re-runs a token-metered cloud job, the named defect.
    public sealed record Cloud(string Owner, string Project, string JobDescriptor, string Platform, string ModelKey) : EnergyRoute;

    public static readonly EnergyRoute Local = new Subprocess();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    // One entry, provider rows: the generated Switch makes a new provider a compile-broken case, never a knob.
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, IClock clock) =>
        request.Policy.Route.Switch(
            subprocess: _ => RunLocal(graph, request, geometry, sink, clock),
            cloud:      c => RunCloud(graph, request, c, sink, clock));

    // Submit -> watch -> pull -> the same ReadResults fold. The async orchestration is one blocking boundary kernel
    // (Exemption: sidecar HTTP + filesystem) bracketed with the scratch directory; auth is composition-root input.
    // Classification is exception-typed: an ApiException/HTTP transport fault is EndpointUnreachable; every other raise,
    // a failed terminal, or a missing SQL asset is AnalysisFailed — a descriptor defect is never misreported as unreachable.
    static Fin<AssessmentResult> RunCloud(ElementGraph graph, AssessmentRequest.Energy request, EnergyRoute.Cloud route, AssessmentSink sink, IClock clock) {
        string scratch = "";
        Instant at = clock.GetCurrentInstant();
        try {
            scratch = Directory.CreateTempSubdirectory("rasm-pollination-").FullName;
            // Model address admits FIRST: a malformed key is a descriptor defect, and discovering it after a metered
            // recipe ran means paying for a run whose results have nowhere to land. The Bim value object owns the
            // grammar; its refusal lowers onto this lane's own fault so no Bim band code rides a Compute rail.
            return ArtifactKey.Admit(route.ModelKey, Op.Of())
                .MapFail(_ => (Error)new ComputeFault.AssessmentInputMissing($"<energy-cloud-model-key-malformed:{route.ModelKey}>"))
                .Bind(artifact => Try.lift(() => Orchestrate(route, scratch).GetAwaiter().GetResult()).Run()
                    .MapFail(error => (Error)(error.Exception.Case is PollinationSDK.Client.ApiException or HttpRequestException
                        ? new ComputeFault.EndpointUnreachable($"<pollination:{Tail(error.Message)}>")
                        : new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<pollination-run:{Tail(error.Message)}>")))
                    .Bind(static fin => fin)
                    // Empty zone roster: the recipe names its own zones, so the cloud arm publishes building scope alone.
                    .Bind(sqlPath => ReadResults(sqlPath, graph, request, new ResultContext(artifact, Seq<ZoneTarget>()), at)
                        .Bind(readout => sink.Store(File.ReadAllBytes(sqlPath))
                            .Bind(blob => request.Policy.Results.Publish(readout.Results)
                                .Map(_ => AssessmentResult.Of(request.Route, readout.Facts, GoverningEui(readout.Facts, request.Policy),
                                    new Provenance("EnergySimulation", request.Route.Standard, $"Pollination {route.Owner}/{route.Project}", at),
                                    Some(blob)))))));
        }
        finally { if (scratch.Length > 0) { try { Directory.Delete(scratch, recursive: true); } catch (IOException) { } } }
    }

    // Wrapper orchestration: JobInfo.FromJson -> RunJobAsync (upload + schedule) -> WatchJobStatusAsync (poll to terminal)
    // -> RunInfo.GetOutputAssets(platform) -> DownloadRunAssetsAsync into scratch. The SDK's LocalDatabase/CheckCached
    // reuse is not composed — the Persistence content-keyed index owns reuse.
    static async Task<Fin<string>> Orchestrate(EnergyRoute.Cloud route, string scratch) {
        PollinationSDK.Wrapper.JobInfo job = PollinationSDK.Wrapper.JobInfo.FromJson(route.JobDescriptor);
        PollinationSDK.Wrapper.ScheduledJobInfo scheduled = await job.RunJobAsync();
        // Terminal gate PARSES against the enum token set and demands equality: a substring test admitted every status
        // that merely contained the token, so a failed run's partial assets read as a completed simulation.
        string status = await scheduled.WatchJobStatusAsync();
        if (!Enum.TryParse(status.Trim(), ignoreCase: true, out PollinationSDK.RunStatusEnum terminal)
            || terminal != PollinationSDK.RunStatusEnum.Succeeded) {
            return Fin.Fail<string>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<pollination-terminal:{Tail(status)}>"));
        }
        PollinationSDK.Wrapper.RunInfo run = new(scheduled);
        List<PollinationSDK.Wrapper.RunAssetBase> assets = [.. run.GetOutputAssets(route.Platform)];
        await run.DownloadRunAssetsAsync(assets, saveAsDir: scratch);
        string sqlPath = Directory.EnumerateFiles(scratch, "eplusout.sql", SearchOption.AllDirectories).FirstOrDefault() ?? "";
        return sqlPath.Length > 0
            ? Fin.Succ(sqlPath)
            : Fin.Fail<string>(new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<pollination-no-sql-asset>"));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
