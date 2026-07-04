# [COMPUTE_ENERGY]

Rasm.Compute energy-simulation runner: the `Discipline.Energy` arm of the assessment rail and the C#-first whole-building-energy lane (`§4E`). It builds an `NREL.OpenStudio` `Model` IN-PROCESS from the concrete `Rasm.Element` `ElementGraph` (spaces, bounding surfaces, layered opaque constructions, thermal zones — all read from the graph), STAMPS the annual-run context the EnergyPlus solver requires (`SimulationControl` weather-file run period, a full-year `RunPeriod`, the attached `EpwFile` weather), CONDITIONS each occupied zone with ideal-air loads driven to the policy heating/cooling dual setpoints and the policy lighting/equipment internal gains (so the demand is the envelope-driven load, never a free-floating zero), forward-translates to an EnergyPlus IDF through `EnergyPlusForwardTranslator`, runs the EnergyPlus solver as a SUBPROCESS resolved through a PARAMETERIZED discovery boundary (env-var → configured-path → bundled-fallback, NEVER a hardcoded path), reads the results SQLite through `SqlFile`, and returns the annual site/source energy, the site/source EUI, the FULL per-end-use breakdown (heating, cooling, interior/exterior lighting, interior/exterior equipment, fans, pumps, heat-rejection, humidification, heat-recovery, water-systems, refrigeration, generators — each summed across every energy fuel), and the hours-simulated annual-completeness validity signal as one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back. OpenStudio (the SWIG SDK) BUILDS the model and READS the results; it neither runs nor bundles the EnergyPlus binary — that is the discovery boundary's job, and the binary version MUST track the OpenStudio SWIG version (OpenStudio 3.11.0 expects EnergyPlus 25.2.0; dev/CI points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-bundled 25.2.0, never the mismatched standalone 26.1.0). Every OpenStudio wrapper owns a native handle and is `IDisposable`, the SDK is single-threaded for model mutation, and every load/get that can miss returns a SWIG `Optional<T>` lowered onto `Fin<T>`/`Option<T>` at the boundary — the runner brackets the model, translator, weather, SQL handles, every `OptionalDouble`/`Point3d`/`*Vector` result, and the scratch run directory under `using`/`try-finally` so no native memory or temp artifact leaks. Compute admits `NREL.OpenStudio.macOS-arm64` for the SIMULATION concern (distinct from the `Rasm.Bim/Exchange/energy` energy-model EXCHANGE owner); the model is built from the seam graph, never re-authored from IFC here. Execution is ONE route axis: the `EnergyRoute` `[Union]` on the policy dispatches the local EnergyPlus subprocess (the default row) or the Pollination cloud run (`PollinationSDK` — submit the app-authored job descriptor, watch the run to a terminal status, pull the result assets, and read the SAME `SqlFile` fold), one entry, provider variance as row data.

## [01]-[INDEX]

- [01]-[TOOLCHAIN_BOUNDARY]: the `EnergyToolchain` parameterized EnergyPlus discovery (env-var → configured-path → bundled-fallback), the `EnergyPolicy` simulation scenario (toolchain + EUI target + conditioning setpoints + internal-load densities), and the OpenStudio/EnergyPlus version-lock guard.
- [02]-[MODEL_BUILD]: the in-process `OpenStudio.Model` built from the graph (spaces/surfaces/constructions/zones), the annual-run context (`SimulationControl`/`RunPeriod`/`WeatherFile`), the ideal-air conditioning and internal-load folds, the `EnergyPlusForwardTranslator`→IDF, and the SWIG `Optional<T>`/`IDisposable`/`Path` boundary discipline.
- [03]-[SIMULATION_RUN]: the EnergyPlus subprocess over the resolved binary, the `SqlFile` annual result read (site/source energy, site/source EUI, the full per-end-use breakdown folded from `SqlFile.endUses()`, and the hours-simulated annual-completeness validity fact), and the `EnergySimulation.Run` fact stream.
- [04]-[CLOUD_ROUTE]: the `EnergyRoute` `[Union]` provider axis on `EnergyPolicy` (`Local` subprocess default · `Cloud` Pollination row) — the `RunCloud` arm over the `PollinationSDK` job/run/asset orchestration converging on the ONE `ReadResults` fold, faults on the existing 2200-band codes.

## [02]-[TOOLCHAIN_BOUNDARY]

- Owner: `EnergyToolchain` the static EnergyPlus-executable resolver; `EnergyToolchainPolicy` the discovery policy (the configured directory override and the platform executable name); `EnergyPolicy` the simulation scenario the `AssessmentRequest.Energy` case carries (the `EnergyRoute` execution row, the toolchain, the EUI target, the heating/cooling dual setpoints, and the lighting/equipment power densities the model build reads); the OpenStudio/EnergyPlus version-lock guard.
- Entry: `public static Fin<string> Resolve(EnergyToolchainPolicy policy)` — probes the candidate paths in priority order `ENERGYPLUS_EXE` env-var → `OPENSTUDIO_ENERGYPLUSDIR` env-var (a full OpenStudio INSTALLATION's bundled binary, dev/CI) → the policy's configured directory → the app's RID-native bundled-runtime fallback (`runtimes/<rid>/native`, the shipped-app packaging convention — the OpenStudio SWIG package itself bundles NO solver, `§4E`), returns the first existing executable, and rails `ComputeFault.ToolchainUnresolved` when none resolves — the discovery is PARAMETERIZED end to end, never a hardcoded path.
- Auto: the resolved binary's SELF-REPORTED version (`energyplus --version`, the binary as version authority — never its filesystem path) is checked against the OpenStudio SWIG version expectation (3.11.0 → 25.2.0); a mismatch (e.g. a standalone 26.1.0 the discovery chain resolved) folds a warning fact into the receipt rather than silently producing a version-skewed IDF the solver rejects.
- Packages: LanguageExt.Core, NREL.OpenStudio.macOS-arm64 (the SWIG SDK whose 3.11.0 version the toolchain version-locks EnergyPlus 25.2.0 to — it bundles NO solver, `§4E`, and the resolver touches no OpenStudio API), BCL inbox (`Environment`, `Path`, `File`, `AppContext` — the app-base RID-native probe — `System.Diagnostics.Process` — the `--version` self-report probe).
- Growth: a new discovery source is one probe in the priority chain; a new platform is the executable-name policy column; a new simulation knob (a ventilation rate, an infiltration default, a real HVAC plant selector) is one `EnergyPolicy` column the model build reads; a new execution provider is one `EnergyRoute` case on the `[04]` axis — the resolver widens by probe, the scenario by policy column, the provider by route row, never a parallel discovery method or a parallel request shape per host.
- Boundary: the discovery is env-var → configured-path → bundled-fallback in that strict priority, never a hardcoded absolute path (a hardcoded EnergyPlus path is the rejected form); `Parametric_Forge` is a DEV/CI probe toolchain only — a SHIPPED app owns its own EnergyPlus provisioning (a bundled 25.2.0 osx-arm64 binary, or `ENERGYPLUS_EXE`), so the bundled-fallback resolves the app's OWN RID-native `runtimes/<rid>/native` runtime (the OpenStudio SWIG package bundles no solver, `§4E`) and never assumes a developer machine; the version-lock is load-bearing — OpenStudio 3.11.0 forward-translates the IDF the version-matched EnergyPlus 25.2.0 consumes, so dev/CI points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-BUNDLED 25.2.0 rather than the mismatched standalone 26.1.0 (the Forge standalone); the resolver applies NO version filter (it probes paths in priority order), so a mismatched binary the discovery chain resolves IS selected and the `VersionGuard` `--version` probe folds a warning fact rather than silently emitting a version-skewed IDF the solver rejects; the conditioning and internal-load defaults are EXPLICIT `EnergyPolicy` knobs (the comfort setpoints and the lighting/equipment densities), never ambient constants buried in the build, so a consumer re-targets a climate or a building type without an interior edit; an unresolved binary rails `ToolchainUnresolved` with the full probe trail, never a default that fails opaquely at run time.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EnergyToolchainPolicy(Option<string> ConfiguredDir, string ExecutableName, string ExpectedVersion) {
    public static readonly EnergyToolchainPolicy Canonical = new(
        ConfiguredDir: None,
        ExecutableName: OperatingSystem.IsWindows() ? "energyplus.exe" : "energyplus",
        ExpectedVersion: "25.2.0");   // OpenStudio 3.11.0 SWIG -> EnergyPlus 25.2.0; never the standalone 26.1.0
}

// The weather scenario the AssessmentRequest.Energy case carries — the EPW path the subprocess runs over (-w) and the OSM WeatherFile embeds.
public sealed record WeatherRef(string EpwPath, string Station);

// The energy simulation scenario: the execution route row, the toolchain, the EUI target (kWh/m2.a), and the
// conditioning + internal-load defaults the model build reads. The defaults are a modern comfort band and
// office-baseline load densities; every column is a knob a consumer re-targets per climate/building-type without
// touching the builder, and the provider is the [04] EnergyRoute row — deployment data, never a second entry.
public sealed record EnergyPolicy(
    EnergyRoute Route, EnergyToolchainPolicy Toolchain, double TargetEui,
    double HeatingSetpointC, double CoolingSetpointC, double LightingPowerWM2, double EquipmentPowerWM2) {
    public static readonly EnergyPolicy Canonical = new(
        EnergyRoute.Local, EnergyToolchainPolicy.Canonical, TargetEui: 0.0,
        HeatingSetpointC: 20.0, CoolingSetpointC: 26.0, LightingPowerWM2: 8.0, EquipmentPowerWM2: 10.0);
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

    // The binary is the version authority, NOT its filesystem path: EnergyPlus self-reports "EnergyPlus, Version
    // 25.2.0-<sha>" on `--version`, so the guard probes the resolved executable rather than grepping its path (a
    // version-named directory is not a version guarantee, and a correctly-versioned binary at an unversioned path is the
    // common case). An undetermined probe reports the marker rather than asserting a false match, so a real skew surfaces.
    public static Seq<AssessmentFact> VersionGuard(string executable, EnergyToolchainPolicy policy) {
        string reported = ProbeVersion(executable);
        return reported.Contains(policy.ExpectedVersion, StringComparison.Ordinal)
            ? Seq<AssessmentFact>()
            : Seq(AssessmentFact.Text("energyplus-version-warning",
                $"<resolved-binary-not-{policy.ExpectedVersion}:reported={reported}:{executable}>"));
    }

    // Run `<executable> --version` and read the self-reported banner — the version authority is the binary, never its
    // path. Bracketed at the OS boundary (Exemption: native subprocess); a launch failure yields a typed marker so the
    // guard reports an undetermined version rather than a false match. ArgumentList escapes the (zero) args; no shell.
    static string ProbeVersion(string executable) {
        try {
            using Process probe = new() {
                StartInfo = new ProcessStartInfo(executable) {
                    ArgumentList = { "--version" }, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
                },
            };
            probe.Start();
            string banner = probe.StandardOutput.ReadToEnd().Trim();
            probe.WaitForExit();
            return banner.Length > 0 ? banner : "<version-unreported>";
        }
        catch (SystemException ex) { return $"<version-probe-failed:{ex.GetType().Name}>"; }
    }

    static Option<string> Probe(string? path) => path is not null && File.Exists(path) ? Some(path) : None;
    static string? Join(string? dir, string exe) => dir is null ? null : Path.Combine(dir, exe);
    // The LAST-resort probe: the APP's own RID-native bundled location (a shipped app drops its version-matched EnergyPlus
    // under runtimes/<rid>/native per the .NET native-asset convention) — NOT the OpenStudio SWIG package, which bundles no
    // solver (§4E). A dev/CI box resolves earlier via OPENSTUDIO_ENERGYPLUSDIR; absent every probe, Resolve rails ToolchainUnresolved.
    static string BundledRuntimeDir() => Path.Combine(AppContext.BaseDirectory, "runtimes", "osx-arm64", "native", "EnergyPlus");
}
```

## [03]-[MODEL_BUILD]

- Owner: `EnergySimulation.BuildModel` the in-process OpenStudio model builder; `OsmBuild` the build receipt (the IDF path plus the translator-and-skip log facts); `ConfigureRun`/`SetpointSchedules`/`InternalLoads`/`Condition`/`BuildSurface`/`BuildConstruction`/`Vertices` the model-object folds; the SWIG `Optional<T>`→`Fin<T>`, `IDisposable`, and `Path` boundary discipline.
- Entry: `static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch)` — guards the weather EPW exists, constructs an `OpenStudio.Model`, stamps the annual-run context (`ConfigureRun`: the unique `SimulationControl` weather-file run, the full-year `RunPeriod`, the `AllSummary` tabular-report request every `SqlFile` annual read depends on, the attached `EpwFile` weather), folds each spatial Object node into a `Space`+`ThermalZone` carrying the policy internal-load `SpaceType` and (where conditioned) ideal-air loads driven to shared dual-setpoint `ScheduleConstant`s, each bounding surface into a `Surface` with its `GeometrySource`-resolved `FootprintPolygon` vertices and its layered `Construction` (the seam `MaterialComposition.LayerSet` lowered to layered `StandardOpaqueMaterial` reading the seam thermal properties), and forward-translates to the IDF `Workspace` through `EnergyPlusForwardTranslator`, `Fin<T>` lowering a missing weather/composition or a translator error onto `ComputeFault.AssessmentInputMissing`/`AnalysisRunFailed`.
- Auto: every OpenStudio file API takes a SWIG `Path` (there is no `Path(string)` ctor) so the builder routes paths through `OpenStudioUtilitiesCore.toPath(string)`; the run context is the UNIQUE `SimulationControl`/`RunPeriod` objects gotten-or-created through the static `OpenStudioModelSimulation.getSimulationControl(model)`/`getRunPeriod(model)` module helpers (neither type carries a `(Model)` ctor, and the SWIG binding surfaces these unique-object getters as static module functions, never as `Model` instance methods), `SimulationControl.setRunSimulationforWeatherFileRunPeriods(true)` arming the annual weather run and `RunPeriod` spanning Jan 1 → Dec 31; `WeatherFile.setWeatherFile(model, epw)` attaches the `EpwFile` design context (the subprocess `-w` is the authoritative run weather); conditioning is `ThermalZone.setUseIdealAirLoads(true)` plus a `ThermostatSetpointDualSetpoint` over the two shared heating/cooling `ScheduleConstant`s so the demand is the envelope-driven load rather than a free-floating zero; internal gains are one `SpaceType` carrying `setLightingPowerPerFloorArea`/`setElectricEquipmentPowerPerFloorArea` assigned to every space; each construction layer is one `StandardOpaqueMaterial` built through its 6-arg ctor (the only admission — no `(Model)`-only ctor exists: roughness + thickness/conductivity/density/specific-heat) reading the seam `Thermal` conductivity/specific-heat and the `Mechanical` density through the `MaterialPropertyAccess` accessors so the OSM construction U-value matches the `Analysis/aggregator` ISO 6946 fold; every load/get that can miss returns a SWIG `Optional<T>` checked with `is_initialized()` before `get()` and lowered to `Option<T>` so interior code never sees the SWIG optional.
- Packages: NREL.OpenStudio.macOS-arm64, LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `Node.Object`, `MaterialComposition`, `MaterialPropertySet.Thermal`/`.Mechanical` read through the `MaterialPropertyAccess` accessors, `MaterialLayer`, `NodeId`, `RepresentationContentHash`, `FootprintPolygon`, the seam-owned host-neutral `Vector3` coordinate the `FootprintPolygon` ring carries, `GeometrySource` the analytical-surface resolution port), BCL inbox.
- Growth: a new model object (a real HVAC plant, a schedule set, a daylighting control, an infiltration object) is one fold over the matching graph nodes onto its OSM `ModelObject`; conditioning widens from ideal-air to a sized plant by one `EnergyPolicy` selector; a transparent construction widens to `StandardGlazing`/`SimpleGlazing` once the seam `MaterialPropertySet` carries an optical case — the build widens by fold and policy column, never a parallel builder per object type.
- Boundary: the model is built from the seam graph for SIMULATION — distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange (gbXML reverse-translate); Compute reads the graph's spaces/surfaces/constructions (already lowered from IFC by Bim's projector) so the energy model derives from the canonical graph, never re-authored from IFC here; every OpenStudio wrapper owns native memory and is `IDisposable` so the `Model`, the `EnergyPlusForwardTranslator`, the `Workspace`, the `EpwFile`, every `Point3d`/`Point3dVector`/`MaterialVector`/`LogMessageVector`, and the result optionals are bracketed under `using` — a dropped handle leaks native memory the GC cannot reclaim; a model-object (`Space`/`Surface`/`ThermalZone`/`Construction`/`StandardOpaqueMaterial`/`ThermostatSetpointDualSetpoint`/`ScheduleConstant`/`SpaceType`) is owned BY the `Model` it is `new`-ed against and is never independently `using`-disposed; the SDK is single-threaded for model mutation so the build is one serialized unit of work, never a parallel fan-out over a shared model; the `*PINVOKE` marshaling classes are an implementation detail, never a call surface; a bounding surface whose composition is absent or non-layered gets the OpenStudio default construction and a logged fact rather than a silently-dropped surface (a missing surface is an envelope hole that wrecks the thermal balance — the geometry is always added, only the construction degrades); glazing is a growth axis — the seam `MaterialPropertySet` carries no optical transmittance, so a transparent construction cannot be built from thermal conductivity alone, opaque layers build from the `Thermal` case and a seam optical case is the addition that unlocks `StandardGlazing`/`SimpleGlazing`.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The build saves the IDF to disk and disposes every native handle — no live OpenStudio handle escapes the boundary.
// TranslatorLog folds the forward-translate warnings/errors AND any surface that fell back to a default construction.
public sealed record OsmBuild(string IdfPath, Seq<AssessmentFact> TranslatorLog);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    // Energy SI dimension composed from the seam Dimension algebra (force x length = M.L^2.T^-2); the EUI intensity divides
    // by area (J.m^-2). No hand-mapped kind and no UnitsNet quantity needed for the dimension; the magnitude coerces GJ->J
    // through UnitsNet once (never a literal factor). EnergyPlus reports site/source energy AND every end use in GJ.
    static readonly Dimension EnergyDim = Dimension.ForceDim.Multiply(Dimension.LengthDim);
    static readonly Dimension EuiDim = EnergyDim.Divide(Dimension.AreaDim);
    const string WaterFuel = "Water";   // the one EndUseFuelType reported in m3 (consumption, not energy) — excluded from the GJ end-use sum
    static double Joules(double gigajoules) => UnitsNet.Energy.FromGigajoules(gigajoules).Joules;

    static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch) {
        if (!File.Exists(request.Weather.EpwPath)) {
            return Fin.Fail<OsmBuild>(new ComputeFault.AssessmentInputMissing($"<energy-weather-missing:{request.Weather.EpwPath}>"));
        }
        using OpenStudio.Model model = new();
        ConfigureRun(model, request);                                        // annual run period + weather + solar distribution
        OpenStudio.SpaceType spaceType = InternalLoads(model, request.Policy); // policy lighting/equipment gains (model-owned)
        var (heating, cooling) = SetpointSchedules(model, request.Policy);     // constant dual-setpoint schedules (model-owned)
        Seq<AssessmentFact> notes = Seq<AssessmentFact>();
        foreach (Node.Object space in graph.SpacesOf(request.Targets)) {
            OpenStudio.Space osSpace = new(model);                            // model objects are owned by the Model — never `using`-disposed here
            osSpace.setName(space.Name);
            osSpace.setSpaceType(spaceType);
            OpenStudio.ThermalZone zone = new(model);
            osSpace.setThermalZone(zone);
            if (graph.IsConditioned(space.Id)) { Condition(model, zone, heating, cooling); }  // ideal-air loads only where conditioned
            foreach (Node.Object surface in graph.BoundingSurfacesOf(space.Id)) { notes += BuildSurface(model, osSpace, surface, graph, geometry); }
        }
        using OpenStudio.EnergyPlusForwardTranslator translator = new();
        using OpenStudio.Workspace idf = translator.translateModel(model);
        using OpenStudio.LogMessageVector errors = translator.errors();
        using OpenStudio.LogMessageVector warnings = translator.warnings();
        Seq<AssessmentFact> log = notes
            + toSeq(Enumerable.Range(0, (int)errors.Count)).Map(i => AssessmentFact.Text($"osm-error-{i}", errors[i].logMessage()))
            + toSeq(Enumerable.Range(0, (int)warnings.Count)).Map(i => AssessmentFact.Text($"osm-warning-{i}", warnings[i].logMessage()));
        if (errors.Count > 0) { return Fin.Fail<OsmBuild>(new ComputeFault.AnalysisRunFailed($"<osm-forward-translate-errors:{errors.Count}>")); }
        string idfPath = Path.Combine(scratch, "in.idf");
        idf.save(OpenStudio.OpenStudioUtilitiesCore.toPath(idfPath), overwrite: true);
        return Fin.Succ(new OsmBuild(idfPath, log));
    }

    // The annual-simulation context the EnergyPlus solver requires: the unique SimulationControl runs the weather-file run
    // period, the unique RunPeriod spans the full year, the request EpwFile is attached as the design context (the subprocess
    // -w is the authoritative run weather), and FullExterior solar distribution avoids the zone-convexity requirement of the
    // interior variants on arbitrary imported geometry. SimulationControl/RunPeriod/OutputTableSummaryReports have NO
    // (Model) ctor; the SWIG binding surfaces their get-or-create as static OpenStudioModelSimulation.get*(model) module
    // functions (NOT Model instance methods — only Building/geometry uniques are surfaced on Model), each returning the bare object.
    static void ConfigureRun(OpenStudio.Model model, AssessmentRequest.Energy request) {
        OpenStudio.SimulationControl control = OpenStudio.OpenStudioModelSimulation.getSimulationControl(model);
        control.setRunSimulationforWeatherFileRunPeriods(true);
        control.setSolarDistribution("FullExterior");
        OpenStudio.RunPeriod run = OpenStudio.OpenStudioModelSimulation.getRunPeriod(model);
        run.setBeginMonth(1); run.setBeginDayOfMonth(1); run.setEndMonth(12); run.setEndDayOfMonth(31);
        // The annual SqlFile readers (totalSiteEnergy/endUses/hoursSimulated) read the EnergyPlus
        // AnnualBuildingUtilityPerformanceSummary + End-Uses tabular reports, emitted only when AllSummary is requested —
        // armed here (get-or-create, idempotent with the translator default) so a result read never depends on an ambient FT default.
        OpenStudio.OpenStudioModelSimulation.getOutputTableSummaryReports(model).addSummaryReport("AllSummary");
        using OpenStudio.EpwFile epw = new(OpenStudio.OpenStudioUtilitiesCore.toPath(request.Weather.EpwPath));
        OpenStudio.WeatherFile.setWeatherFile(model, epw);                    // OptionalWeatherFile — embeds the design context
    }

    // Constant heating/cooling setpoint schedules from the policy comfort band — one pair shared across every conditioned zone
    // (model-owned, never `using`-disposed); a ScheduleConstant IS a Schedule so it admits to the dual-setpoint thermostat.
    static (OpenStudio.ScheduleConstant Heating, OpenStudio.ScheduleConstant Cooling) SetpointSchedules(OpenStudio.Model model, EnergyPolicy policy) {
        OpenStudio.ScheduleConstant heating = new(model);
        heating.setName("rasm-heating-setpoint");
        heating.setValue(policy.HeatingSetpointC);
        OpenStudio.ScheduleConstant cooling = new(model);
        cooling.setName("rasm-cooling-setpoint");
        cooling.setValue(policy.CoolingSetpointC);
        return (heating, cooling);
    }

    // Ideal-air conditioning to the dual setpoints: the minimal envelope-study system so the EnergyPlus heating/cooling demand
    // is the envelope-driven load, never a free-floating zero — a sized HVAC plant is the growth axis one policy selector widens to.
    static void Condition(OpenStudio.Model model, OpenStudio.ThermalZone zone, OpenStudio.ScheduleConstant heating, OpenStudio.ScheduleConstant cooling) {
        zone.setUseIdealAirLoads(true);
        OpenStudio.ThermostatSetpointDualSetpoint thermostat = new(model);
        thermostat.setHeatingSetpointTemperatureSchedule(heating);
        thermostat.setCoolingSetpointTemperatureSchedule(cooling);
        zone.setThermostatSetpointDualSetpoint(thermostat);
    }

    // The policy internal gains as one SpaceType (lighting + equipment power density) assigned to every space, so the EUI carries
    // the plug+lighting load an envelope-only model omits — the densities are explicit policy knobs, not fabricated-as-fact constants.
    static OpenStudio.SpaceType InternalLoads(OpenStudio.Model model, EnergyPolicy policy) {
        OpenStudio.SpaceType spaceType = new(model);
        spaceType.setName("rasm-space-type");
        spaceType.setLightingPowerPerFloorArea(policy.LightingPowerWM2);
        spaceType.setElectricEquipmentPowerPerFloorArea(policy.EquipmentPowerWM2);
        return spaceType;
    }

    // The bounding-surface geometry is ALWAYS added so the zone stays enclosed; the construction is attached when the surface's
    // seam composition resolves to a LayerSet, else the surface keeps the OpenStudio default construction and a logged note —
    // a silently-dropped surface is an envelope hole, the rejected form.
    static Seq<AssessmentFact> BuildSurface(OpenStudio.Model model, OpenStudio.Space space, Node.Object surface, ElementGraph graph, GeometrySource geometry) =>
        // The surface polygon resolves ONE-HOP by content key through the seam GeometrySource off surface.Representations
        // .FootPrint — the Object node carries NO inline BoundaryPolygon (the deleted §4-RT-M2 phantom). A surface whose
        // footprint blob is absent/undecodable is a logged envelope-hole note rather than a silently-dropped zero-area
        // surface (the rejected form), the same graceful-skip shape the missing-construction note takes.
        geometry.Footprint(surface.Representations).Match(
            Some: footprint => {
                using OpenStudio.Point3dVector vertices = Vertices(footprint);
                OpenStudio.Surface osSurface = new(vertices, model);              // owned by the Model
                osSurface.setSpace(space);
                return graph.CompositionOf(surface.Id).Bind(static c => c is MaterialComposition.LayerSet set ? Some(set) : None).Match(
                    Some: set => BuildConstruction(model, set, graph).Match(
                        Succ: construction => { osSurface.setConstruction(construction); return Seq<AssessmentFact>(); },
                        Fail: error => Seq(AssessmentFact.Text($"osm-surface-default-construction:{surface.Id.Value}", error.Message))),
                    None: () => Seq(AssessmentFact.Text($"osm-surface-default-construction:{surface.Id.Value}", "<no-layerset-composition>")));
            },
            None: () => Seq(AssessmentFact.Text($"osm-surface-footprint-unresolved:{surface.Id.Value}", "<no-footprint-representation>")));

    static Fin<OpenStudio.Construction> BuildConstruction(OpenStudio.Model model, MaterialComposition.LayerSet set, ElementGraph graph) =>
        set.Layers.Fold(
            Fin.Succ(new List<OpenStudio.Material>()),
            (acc, layer) => acc.Bind(mats => graph.Material(layer.Material).Map(static m => m.Properties)
                .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-layer-material-absent:{layer.Material.Value}>"))
                .Bind(props => props.Thermal.ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-layer-missing-thermal:{layer.Material.Value}>"))
                    .Map(thermal => {
                        // The 6-arg ctor is the ONLY StandardOpaqueMaterial admission (no (Model)-only ctor exists): the
                        // IFC-neutral MediumRough roughness drives the exterior convection coefficient, then the seam thermal
                        // columns SI-native — Thickness.Si m, Conductivity.Si W/mK, the Mechanical.Density.Si kg/m3 fallback, SpecificHeat.Si J/kgK.
                        OpenStudio.StandardOpaqueMaterial mat = new(model, "MediumRough",
                            layer.Thickness.Si, thermal.Conductivity.Si,
                            props.Mechanical.Map(static m => m.Density.Si).IfNone(1000.0), thermal.SpecificHeat.Si);   // owned by the Model
                        mats.Add(mat);
                        return mats;
                    }))))
            .Map(mats => {
                using OpenStudio.MaterialVector vec = new(mats);
                OpenStudio.Construction construction = new(model);
                construction.setLayers(vec);
                return construction;
            });
}
```

## [04]-[SIMULATION_RUN]

- Owner: `EnergySimulation.Run` the ONE energy entry dispatching the `EnergyRoute` row (`[04]` declares the axis and the cloud arm); `RunLocal` the subprocess arm; `RunSubprocess` the EnergyPlus subprocess over the resolved binary; `ReadResults`/`EndUseFacts`/`ValidityFacts`/`GoverningEui`/`Eui`/`Slug`/`Lower`/`Vertices` the `SqlFile` result read (the site/source totals, the site/source EUI, the structured per-end-use fold, the hours-simulated annual-completeness validity fact) and the measure projection SHARED by both routes; the scratch run-directory lifetime.
- Entry: `static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, ClockPolicy clocks)` — the `EnergyRoute.Local` arm the `Run` dispatch enters — resolves the EnergyPlus binary through `EnergyToolchain.Resolve`, builds the OSM model and IDF through `BuildModel` (threading the seam `GeometrySource` so each bounding surface's analytical `FootprintPolygon` resolves one-hop by content key off `surface.Representations.FootPrint`), runs EnergyPlus as a subprocess over the scratch directory, reads `eplusout.sql` through `SqlFile`, and emits the `total-site-energy`/`total-source-energy`/`net-source-energy`/`eui`/`source-eui` scalars plus the `end-use:<category>` breakdown family and the `hours-simulated` annual-completeness validity fact, bracketing the scratch directory and every native handle so nothing leaks.
- Auto: the subprocess is `energyplus -w <weather.epw> -d <outdir> -r <in.idf>` over the resolved binary; a non-zero exit code rails `ComputeFault.AnalysisRunFailed` with the captured stderr tail; the `SqlFile` annual accessors return SWIG `OptionalDouble` lowered to `Option<double>` AND disposed at the lowering boundary (a getter's optional is itself disposable); the per-end-use breakdown folds the structured `SqlFile.endUses()` summary — one all-energy-fuel sum per `EndUseCategoryType` enumerated from the static `EndUses.fuelTypes()`/`categories()` vectors (each handle bracketed under `using`, the SWIG marshaling exemption), the `Water` fuel (m³ consumption, never energy) the one excluded column — so a gas- or district-heated building's heating end use stays fuel-agnostic AND the lighting/equipment/fan/pump loads that dominate a modern EUI are reported, never the electricity-only or HVAC-only slice; the site/source EUI divides the total site/source energy by the conditioned floor area read from the graph; the `hours-simulated` validity fact carries `SqlFile.hoursSimulated()` as a `Duration` measure so a downstream verdict can reject a partial-year run (a short count means the solver terminated early and the totals are not a full annual result) — the setpoint-not-met hours are an EnergyPlus tabular read the binding exposes through no `SqlFile` accessor and no generic SQL exec, a growth axis a SQLite reader opens; every result measure is constructed SI-native through `MeasureValue.OfSi` with the energy `Dimension` composed from the seam `Force×Length` algebra (and `÷Area` for the EUI intensity), the GJ→J coercion riding `UnitsNet.Energy` once (never a literal conversion factor); the governing verdict converts the SI EUI (J·m⁻²) back to the conventional kWh·m⁻²·a⁻¹ through `UnitsNet.Energy` and divides by the policy target, projecting `double.NaN` (the `AssessmentVerdict.FromRatio` not-applicable signal) when the policy carries no target rather than a misleading `0.0`-ratio satisfied verdict.
- Receipt: the `Assessment` `ComputeReceipt` case carries the energy discipline/route/content-key plus the elapsed wall time the subprocess dominated; the translator log, the surface-default-construction notes, and the version-guard warning fold into the fact stream as soft notes.
- Packages: NREL.OpenStudio.macOS-arm64 (the `SqlFile` totals + the structured `EndUses` end-use fold + the `hoursSimulated` accessor + the static `OpenStudioModelSimulation.get*(model)` run-context helpers), UnitsNet (the GJ→J / J→kWh dimensioned conversion AND the hours→s coercion), LanguageExt.Core, NodaTime, Rasm.Element (project — `ElementGraph`, `Dimension`, `MeasureValue`, `PropertyValue`, `NodeId`), BCL inbox (`System.Diagnostics.Process`, `System.IO`, `System.Text.StringBuilder`).
- Boundary: the EnergyPlus binary is the resolved subprocess (OpenStudio does NOT run it) so the runner owns the process lifetime, the scratch directory, and the stderr capture, bracketing them in `try-finally` (the platform-forced statement boundary — Exemption: native subprocess + filesystem); the model build and the SQL read are the single-threaded native OpenStudio boundary, one serialized unit of work; every `OpenStudio.*` handle (`Model`, translator, `Workspace`, `EpwFile`, `SqlFile`, every `OptionalDouble`/`Point3d`/`*Vector`) is disposed; the SQL accessors return SWIG `OptionalDouble` lowered to `Option<double>` so a missing output is carried, never a bare `get()` faulting in native code; result measures are SI-native `MeasureValue.OfSi` (a phantom 2-arg `MeasureValue.Of(value, "unit")` is the rejected form — the seam factory is `Of(value, unit, key)`/`OfSi(dimension, si)`, and OfSi is the entry a Compute-computed result writes through); the verdict is the EUI against a target when the policy carries one, else the energy use is reported informationally; a subprocess non-zero exit or a missing SQL file rails `AnalysisRunFailed`, never a silent zero-energy result; the sub-annual per-month consumption profile is a growth axis over `SqlFile.energyConsumptionByMonth(EndUseFuelType, EndUseCategoryType, MonthOfYear)` — the annual fact stream reports the site/source energy, the site/source EUI, the full per-end-use breakdown, and the hours-simulated annual-completeness validity fact, the per-month consumption deepening as one fold when a seasonal/demand-shape route requires it.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, ClockPolicy clocks) {
        // The native + subprocess boundary rails onto Fin: a SWIG handle ctor, an OpenStudio model mutation, a Process.Start
        // over a bad binary, or a corrupt SqlFile throws a SystemException the Fin-returning entry OWES the caller as
        // AnalysisRunFailed, never an unhandled escape (the boundary discipline VersionGuard's --version probe already
        // applies); scratch creation is inside the bracket and the recursive cleanup is best-effort so a delete fault never
        // masks the run's Fin result. The provenance records the version-LOCKED EnergyPlus identity the toolchain pins.
        string scratch = "";
        try {
            scratch = Directory.CreateTempSubdirectory("rasm-eplus-").FullName;
            return from binary in EnergyToolchain.Resolve(request.Policy.Toolchain)
                   from build in BuildModel(graph, request, geometry, scratch)
                   from sqlPath in RunSubprocess(binary, build.IdfPath, request, scratch)
                   from facts in ReadResults(sqlPath, graph, request)
                   select AssessmentResult.Of(request.Route,
                       facts + build.TranslatorLog + EnergyToolchain.VersionGuard(binary, request.Policy.Toolchain),
                       GoverningEui(facts, request.Policy),
                       new Provenance("EnergySimulation", request.Route.Standard, $"EnergyPlus {request.Policy.Toolchain.ExpectedVersion}", clocks.Now));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<AssessmentResult>(new ComputeFault.AnalysisRunFailed($"<energy-native-fault:{ex.GetType().Name}:{Tail(ex.Message)}>"));
        }
        finally { if (scratch.Length > 0) { try { Directory.Delete(scratch, recursive: true); } catch (IOException) { } } }
    }

    static Fin<string> RunSubprocess(string binary, string idfPath, AssessmentRequest.Energy request, string scratch) {
        using Process process = new() {
            // ArgumentList escapes each token by the runtime, so a weather/scratch/IDF path with spaces (the macOS norm)
            // round-trips intact — manual quote-injection into a single Arguments string is the fragile form it replaces.
            StartInfo = new ProcessStartInfo(binary) {
                ArgumentList = { "-w", request.Weather.EpwPath, "-d", scratch, "-r", idfPath },
                RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = scratch,
            },
        };
        process.Start();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        string sqlPath = Path.Combine(scratch, "eplusout.sql");
        return process.ExitCode == 0 && File.Exists(sqlPath)
            ? Fin.Succ(sqlPath)
            : Fin.Fail<string>(new ComputeFault.AnalysisRunFailed($"<energyplus-exit:{process.ExitCode}:{Tail(stderr)}>"));
    }

    static Fin<Seq<AssessmentFact>> ReadResults(string sqlPath, ElementGraph graph, AssessmentRequest.Energy request) {
        using OpenStudio.SqlFile sql = new(OpenStudio.OpenStudioUtilitiesCore.toPath(sqlPath));
        double floorAreaM2 = graph.ConditionedFloorArea(request.Targets);
        // totalSiteEnergy is the REQUIRED headline output (its absence is a failed run, never a silent zero); the source
        // energy, the site/source EUI, the FULL per-end-use breakdown, and the hours-simulated annual-completeness validity fact ride alongside.
        return Lower(sql.totalSiteEnergy()).Match(
            Some: siteGj => {
                double sourceGj = Lower(sql.totalSourceEnergy()).IfNone(0.0);
                return Fin.Succ(Seq(
                    AssessmentFact.Measure("total-site-energy", MeasureValue.OfSi(EnergyDim, Joules(siteGj))),
                    AssessmentFact.Measure("total-source-energy", MeasureValue.OfSi(EnergyDim, Joules(sourceGj))),
                    AssessmentFact.Measure("net-source-energy", MeasureValue.OfSi(EnergyDim, Joules(Lower(sql.netSourceEnergy()).IfNone(0.0)))),
                    AssessmentFact.Measure("eui", MeasureValue.OfSi(EuiDim, Eui(siteGj, floorAreaM2))),
                    AssessmentFact.Measure("source-eui", MeasureValue.OfSi(EuiDim, Eui(sourceGj, floorAreaM2))))
                    + EndUseFacts(sql) + ValidityFacts(sql));
            },
            None: () => Fin.Fail<Seq<AssessmentFact>>(new ComputeFault.AnalysisRunFailed("<energyplus-sql-no-total-site-energy>")));
    }

    // The FULL annual end-use breakdown folded from the structured SqlFile.endUses() summary: one all-energy-fuel sum per
    // EndUseCategoryType (heating, cooling, interior/exterior lighting, interior/exterior equipment, fans, pumps, heat
    // rejection, humidification, heat recovery, water systems, refrigeration, generators), so the EUI carries the load
    // breakdown the heating/cooling-only slice omitted — lighting+plug+fan+pump energy that dominates a modern building's EUI.
    // The fuel taxonomy is enumerated from the static EndUses.fuelTypes(); the Water fuel is reported in m3 (consumption, not
    // energy) and is the one fuel excluded from the GJ sum (getEndUse returns the category-fuel cell in GJ). The index-loop +
    // per-element `using` is the SWIG native-handle disposal boundary (the vector indexer returns a disposable handle per
    // EndUseCategoryType/EndUseFuelType) — the same marshaling exemption Vertices takes.
    static Seq<AssessmentFact> EndUseFacts(OpenStudio.SqlFile sql) {
        using OpenStudio.OptionalEndUses optional = sql.endUses();
        if (!optional.is_initialized()) { return Seq<AssessmentFact>(); }
        using OpenStudio.EndUses uses = optional.get();
        using OpenStudio.EndUseCategoryTypeVector categories = OpenStudio.EndUses.categories();
        using OpenStudio.EndUseFuelTypeVector fuels = OpenStudio.EndUses.fuelTypes();
        List<AssessmentFact> facts = new(categories.Count);
        for (int c = 0; c < categories.Count; c++) {
            using OpenStudio.EndUseCategoryType category = categories[c];
            double categoryGj = 0.0;
            for (int f = 0; f < fuels.Count; f++) {
                using OpenStudio.EndUseFuelType fuel = fuels[f];
                if (fuel.valueName() != WaterFuel) { categoryGj += uses.getEndUse(fuel, category); }
            }
            facts.Add(AssessmentFact.Measure($"end-use:{Slug(category.valueName())}", MeasureValue.OfSi(EnergyDim, Joules(categoryGj))));
        }
        return toSeq(facts);
    }

    // The model-validity fact: the EnergyPlus annual hours actually simulated (SqlFile.hoursSimulated — the ONE hours
    // accessor the SWIG binding exposes). A full annual run reports ~8760 h (8784 leap); a short count means the solver
    // terminated early, so the reported energy is a partial-year artifact a downstream verdict must reject — a more
    // fundamental validity gate than any single output's absence (an absent hoursSimulated simply contributes no fact,
    // never a fabricated zero). The setpoint-not-met hours (ASHRAE 90.1 caps ~300 h/yr) are an EnergyPlus tabular read
    // (the SystemSummary "Time Setpoint Not Met" report) the binding's SqlFile exposes through NO accessor and NO generic
    // SQL exec, so that refinement lands once a SQLite reader opens eplusout.sql's tabular table — a growth axis, not a slice.
    static Seq<AssessmentFact> ValidityFacts(OpenStudio.SqlFile sql) =>
        Lower(sql.hoursSimulated()).Map(static h => HoursFact("hours-simulated", h)).ToSeq();

    static AssessmentFact HoursFact(string name, double hours) =>
        AssessmentFact.Measure(name, MeasureValue.OfSi(Dimension.DurationDim, UnitsNet.Duration.FromHours(hours).Seconds));

    // The governing ratio is the SITE EUI (read back from the emitted fact, one source) against the policy target in
    // kWh.m^-2.a^-1; with NO target (or no eui fact) the ratio is double.NaN so AssessmentVerdict.FromRatio bands it
    // NotApplicable — the energy use is reported informationally, never a misleading 0.0-ratio "Satisfied".
    static double GoverningEui(Seq<AssessmentFact> facts, EnergyPolicy policy) =>
        policy.TargetEui > 0.0
            ? facts.Choose(static f => f.Name.Value == "eui" && f.Value is PropertyValue.Measure m ? Some(m.Value.Si) : None)
                .Head.Map(euiSi => UnitsNet.Energy.FromJoules(euiSi).KilowattHours / policy.TargetEui).IfNone(double.NaN)
            : double.NaN;

    static double Eui(double gigajoules, double floorAreaM2) => floorAreaM2 > 0.0 ? Joules(gigajoules) / floorAreaM2 : 0.0;

    // The EndUseCategoryType.valueName() PascalCase token lowered to the page's kebab fact-name convention (InteriorLights -> interior-lights).
    static string Slug(string valueName) {
        System.Text.StringBuilder b = new(valueName.Length + 4);
        for (int i = 0; i < valueName.Length; i++) {
            char ch = valueName[i];
            if (i > 0 && char.IsUpper(ch) && !char.IsUpper(valueName[i - 1])) { b.Append('-'); }
            b.Append(char.ToLowerInvariant(ch));
        }
        return b.ToString();
    }

    // Read a SWIG OptionalDouble onto Option<double> and DISPOSE the native handle — a getter's OptionalDouble is itself
    // disposable, so a bare read leaks native memory; this lower is the ONE place a missing output becomes None, never a faulting get().
    static Option<double> Lower(OpenStudio.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    // Read the seam-neutral surface boundary (seam Rasm.Element Vector3 coordinates) and build the OpenStudio vertex vector — each Point3d
    // is a native handle disposed immediately after Add (the vector copies it), so the marshaling leaks nothing; the seam never
    // returns an OpenStudio type, the OSM marshaling stays inside the Compute energy boundary.
    static OpenStudio.Point3dVector Vertices(FootprintPolygon footprint) {
        OpenStudio.Point3dVector vec = new();
        foreach (Vector3 p in footprint.Ring) { using OpenStudio.Point3d point = new(p.X, p.Y, p.Z); vec.Add(point); }
        return vec;
    }
    static string Tail(string s) => s.Length <= 256 ? s : s[^256..];
}

// The discipline graph reads the energy runner composes from the seam primitives (ObjectNodes/EdgesAt/Find + the neutral
// Generic space-boundary edges by wire-name + the bag nodes off the Assign.PropertyDefinition edges) — Compute-OWNED
// ElementGraph extensions, NOT seam members: the seam owns the material/composition reads and the GeometrySource decode
// CONTRACT, the discipline spatial reads live here. Spaces are the "IfcSpace"-classified Object nodes reachable from the
// targets; bounding surfaces ride the projected IfcRelSpaceBoundary edges, each surface's analytical FootprintPolygon
// resolved one-hop by content key off surface.Representations.FootPrint through the threaded GeometrySource (never a
// phantom node field); the conditioned floor area sums the spaces' Qto_SpaceBaseQuantities net area.
public static class EnergyGraphReads {
    const string SpaceBoundary  = "IfcRelSpaceBoundary";
    const string SpaceClass     = "IfcSpace";
    const string BaseQuantities = "Qto_SpaceBaseQuantities";
    const string BoundaryLevelAttr = "BoundaryLevel";   // the Bim SpatialBoundaries Generic-edge payload key — THREE-valued: "1st"/"2nd" plus "" for a base-class IfcRelSpaceBoundary whose level the file never declared (the fabricated-"1st" upgrade is deleted)
    const string SecondLevel    = "2nd";                // prefer 2nd-level (space-to-space adjacency) boundaries so a 1st+2nd export never double-counts the envelope; the "" undeclared rows read 1st-EQUIVALENT by policy — included when no 2nd-level set exists, excluded beside one

    public static Seq<Node.Object> SpacesOf(this ElementGraph graph, Seq<NodeId> targets) =>
        targets.IsEmpty
            ? graph.ObjectNodes.Filter(IsSpace)
            : targets.Bind(t => Descend(graph, t)).Distinct().Choose(graph.Find<Node.Object>).Filter(IsSpace).ToSeq();

    // A space's bounding surfaces ride the Bim-projected IfcRelSpaceBoundary neutral Generic edges (Relating=space,
    // Related=bounding element). When a model carries BOTH 1st- and 2nd-level boundaries (the common Revit/ArchiCAD export),
    // only the 2nd-level set is read so the envelope is never double-counted; a 1st-level-only model reads its 1st-level
    // set, and an undeclared-level "" row (a base-class IfcRelSpaceBoundary) rides that same 1st-equivalent arm — the
    // secondLevel filter excludes it beside a declared 2nd-level set, the empty-set fallback includes it.
    public static Seq<Node.Object> BoundingSurfacesOf(this ElementGraph graph, NodeId space) {
        Seq<(Relationship.Generic Edge, Node.Object Surface)> boundaries =
            graph.EdgesAt(space).Choose(e => e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
                ? graph.Find<Node.Object>(g.Related).Map(s => (Edge: g, Surface: s)) : None).ToSeq();
        Seq<(Relationship.Generic Edge, Node.Object Surface)> secondLevel = boundaries.Filter(static b =>
            b.Edge.Attributes.Find(PropertyName.Create(BoundaryLevelAttr)).Exists(static v => v is PropertyValue.Text { Value: SecondLevel }));
        return (secondLevel.IsEmpty ? boundaries : secondLevel).Map(static b => b.Surface);
    }

    // The EUI denominator: the net floor area of every conditioned space under the targets, summed in SI m2 off each space's
    // Qto_SpaceBaseQuantities NetFloorArea; an external/unconditioned space is excluded and a space lacking the quantity
    // contributes zero rather than faulting the run.
    public static double ConditionedFloorArea(this ElementGraph graph, Seq<NodeId> targets) =>
        graph.SpacesOf(targets).Filter(s => graph.IsConditioned(s.Id)).Fold(0.0, (acc, s) => acc + graph.NetFloorAreaM2(s.Id));

    // A space is conditioned unless Pset_SpaceCommon marks it external; absent the flag it is treated conditioned. The same
    // predicate gates the OSM ideal-air conditioning (Condition) AND the EUI denominator, so the model and the intensity agree.
    public static bool IsConditioned(this ElementGraph graph, NodeId space) =>
        graph.Property(space, "Pset_SpaceCommon", "IsExternal").Match(
            Some: static v => v is not PropertyValue.Boolean { Value: true }, None: static () => true);

    static bool IsSpace(Node.Object o) => o.Classification.Code == SpaceClass;

    static double NetFloorAreaM2(this ElementGraph graph, NodeId space) =>
        graph.Quantity(space, BaseQuantities, "NetFloorArea").Bind(static m => m.Area).IfNone(0.0);

    static Option<PropertyValue> Property(this ElementGraph graph, NodeId obj, string set, string name) =>
        graph.Bags<Node.PropertySet>(obj).Filter(ps => ps.Bag.SetName == set).Head
            .Bind(ps => ps.Bag.Values.Find(PropertyName.Create(name)));

    static Option<MeasureValue> Quantity(this ElementGraph graph, NodeId obj, string set, string name) =>
        graph.Bags<Node.QuantitySet>(obj).Filter(qs => qs.Bag.SetName == set).Head
            .Bind(qs => qs.Bag.Values.Find(PropertyName.Create(name)));

    // The bag nodes off an object's Assign.PropertyDefinition edges the Bim DefinesProperties fold projected.
    static Seq<T> Bags<T>(this ElementGraph graph, NodeId obj) where T : Node =>
        graph.EdgesAt(obj).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == obj
            ? graph.Find<T>(a.Definition) : None).ToSeq();

    // Transitive descent over the OWNING Compose decomposition (aggregate/nest/contain) — a building/storey target reaches its
    // spaces; the non-owning Reference compose flavor is excluded so a referenced (not contained) node never inflates the set.
    // A path-ancestry set guards a cyclic/self-aggregating Compose chain: the seam Apply validates endpoint EXISTENCE, not
    // compose-acyclicity (Bake catches a cycle lazily — but this spatial descent runs BEFORE any Bake), so a corrupt graph
    // yields an empty branch rather than an UNCATCHABLE StackOverflowException the run-boundary try/catch cannot rail.
    static Seq<NodeId> Descend(ElementGraph graph, NodeId node) => Descend(graph, node, ImmutableHashSet<NodeId>.Empty);

    static Seq<NodeId> Descend(ElementGraph graph, NodeId node, ImmutableHashSet<NodeId> ancestry) =>
        ancestry.Contains(node)
            ? Seq<NodeId>()
            : node.Cons(graph.EdgesAt(node).Choose(e => e is Relationship.Compose c && c.Whole == node && c.SubKind != ComposeKind.Reference ? Some(c.Part) : None).ToSeq()
                .Bind(child => Descend(graph, child, ancestry.Add(node))));
}
```

## [05]-[CLOUD_ROUTE]

- Owner: `EnergyRoute` the closed execution-provider `[Union]` on `EnergyPolicy` (`Subprocess` the local default row · `Cloud` the Pollination row carrying owner/project/job-descriptor/platform as NEUTRAL values); `EnergySimulation.Run` the ONE entry whose generated total `Switch` dispatches the row; `RunCloud` the Pollination arm; `Orchestrate` the bracketed async SDK kernel.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, ClockPolicy clocks)` dispatches `request.Policy.Route` — `Subprocess` enters `RunLocal` (`[04]`), `Cloud` enters `RunCloud`, which submits the app-authored job descriptor (`PollinationSDK.Wrapper.JobInfo.FromJson` → `RunJobAsync` uploading input assets and scheduling), polls `ScheduledJobInfo.WatchJobStatusAsync` to a terminal status, gates on `RunStatusEnum.Succeeded`, pulls the result assets (`new RunInfo(scheduled)` → `GetOutputAssets(platform)` → `DownloadRunAssetsAsync(assets, saveAsDir: scratch)`), locates the downloaded `eplusout.sql`, and converges on the SAME `ReadResults` fold `[04]` owns — one result read serves both providers, so the fact stream, the EUI verdict, and the receipt shape are route-invariant.
- Auto: the HBJSON model payload inside the job descriptor is the `Rasm.Bim/Exchange/energy` content-keyed `EnergyArtifact` the app root staged — Compute references no Bim type, the model travels as a document artifact on the object plane (the seam-graph alignment law); the downloaded run assets land content-keyed on the Persistence object plane through the spine's retention seam exactly as the local `eplusout.sql` payload does, and the assessment node keys the SAME `(input subgraph, route, policy)` content key, so a re-submitted identical model+recipe resolves from the Persistence index — the SDK's `Wrapper.LocalDatabase` SQLite ledger and its path-existence `CheckCached` are NOT composed (path-existence reuse without hash verification is the integrity gap the content-keyed index closes).
- Receipt: the `Assessment` receipt carries the cloud provenance (`Pollination {owner}/{project}`) beside the route/content-key columns; the watch-status trail folds into the fact stream as soft notes.
- Packages: PollinationSDK (the `Wrapper` job/run/asset orchestration + `RunStatusEnum` terminal vocabulary — sidecar-isolated: its vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` closure never meets the STJ rails and never loads in-Rhino), LanguageExt.Core, NodaTime, BCL inbox (`Directory`, `Path`).
- Growth: a new cloud provider is one `EnergyRoute` case plus one arm — the `Switch` breaks every dispatch site at compile time; a recipe change is job-descriptor DATA, never a signature; per-output typed decodes beyond the SQLite (a parquet result frame, a comfort-map asset) widen `Orchestrate` by one asset row.
- Boundary: `Configuration`/`TokenRepo` auth is composition-root connection input handed to the ambient SDK configuration — never a policy column, never a fence member (the Persistence catalog's token-lifecycle law verbatim); the async SDK orchestration is ONE blocking boundary kernel bracketed with the scratch directory (Exemption: sidecar HTTP + filesystem), and an `ApiException`/transport fault maps `ComputeFault.EndpointUnreachable` (2208, the EC3 precedent) while a failed/cancelled terminal or a missing SQL asset maps `AnalysisRunFailed` (2219) — ZERO new band codes, the classification rows the compute brief `[V2]` calls for; the cloud row composes the Persistence object plane for artifact residency (presigned-grant transfer, `ArtifactKind.CloudRun` reuse index, PROV attribution are the Persistence owners' rows — seam alignment, never an import); a cloud-side model REBUILD from the graph is the rejected form — the cloud consumes the Bim-lowered HBJSON artifact, the local route consumes the in-process OSM build, and the two stay two rows on one axis.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The execution-provider axis: the case IS the route, provider coordinates ride the case as neutral values —
// no PollinationSDK type enters the policy; the SDK closure stays inside the RunCloud boundary (sidecar law).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyRoute {
    private EnergyRoute() { }

    public sealed record Subprocess : EnergyRoute;

    // Owner/Project name the Pollination project; JobDescriptor is the app-authored Wrapper.JobInfo JSON
    // (recipe ref + inputs incl. the staged Bim-lowered HBJSON artifact); Platform keys GetOutputAssets.
    public sealed record Cloud(string Owner, string Project, string JobDescriptor, string Platform) : EnergyRoute;

    public static readonly EnergyRoute Local = new Subprocess();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    // ONE entry, provider rows: the generated Switch makes a new provider a compile-broken case, never a knob.
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, ClockPolicy clocks) =>
        request.Policy.Route.Switch(
            subprocess: _ => RunLocal(graph, request, geometry, clocks),
            cloud:      c => RunCloud(graph, request, c, clocks));

    // Submit -> watch -> pull -> the SAME ReadResults fold. The async SDK orchestration is one blocking boundary
    // kernel (Exemption: sidecar HTTP + filesystem) bracketed with the scratch directory; auth is composition-root
    // input. A transport fault is EndpointUnreachable (2208); a failed terminal or missing SQL is AnalysisRunFailed (2219).
    static Fin<AssessmentResult> RunCloud(ElementGraph graph, AssessmentRequest.Energy request, EnergyRoute.Cloud route, ClockPolicy clocks) {
        string scratch = "";
        try {
            scratch = Directory.CreateTempSubdirectory("rasm-pollination-").FullName;
            return Try.lift(() => Orchestrate(route, scratch).GetAwaiter().GetResult()).Run()
                .MapFail(error => (Error)new ComputeFault.EndpointUnreachable($"<pollination:{Tail(error.Message)}>"))
                .Bind(static fin => fin)
                .Bind(sqlPath => ReadResults(sqlPath, graph, request))
                .Map(facts => AssessmentResult.Of(request.Route, facts, GoverningEui(facts, request.Policy),
                    new Provenance("EnergySimulation", request.Route.Standard, $"Pollination {route.Owner}/{route.Project}", clocks.Now)));
        }
        finally { if (scratch.Length > 0) { try { Directory.Delete(scratch, recursive: true); } catch (IOException) { } } }
    }

    // Verified Wrapper orchestration: JobInfo.FromJson -> RunJobAsync (upload + schedule) -> WatchJobStatusAsync
    // (poll to terminal) -> RunInfo.GetOutputAssets(platform) -> DownloadRunAssetsAsync into the scratch. The SDK's
    // LocalDatabase/CheckCached reuse is NOT composed — the Persistence content-keyed index owns reuse.
    static async Task<Fin<string>> Orchestrate(EnergyRoute.Cloud route, string scratch) {
        PollinationSDK.Wrapper.JobInfo job = PollinationSDK.Wrapper.JobInfo.FromJson(route.JobDescriptor);
        PollinationSDK.Wrapper.ScheduledJobInfo scheduled = await job.RunJobAsync();
        string status = await scheduled.WatchJobStatusAsync();
        if (!status.Contains(nameof(PollinationSDK.RunStatusEnum.Succeeded), StringComparison.OrdinalIgnoreCase)) {
            return Fin.Fail<string>(new ComputeFault.AnalysisRunFailed($"<pollination-terminal:{Tail(status)}>"));
        }
        PollinationSDK.Wrapper.RunInfo run = new(scheduled);
        List<PollinationSDK.Wrapper.RunAssetBase> assets = [.. run.GetOutputAssets(route.Platform)];
        await run.DownloadRunAssetsAsync(assets, saveAsDir: scratch);
        string sqlPath = Directory.EnumerateFiles(scratch, "eplusout.sql", SearchOption.AllDirectories).FirstOrDefault() ?? "";
        return sqlPath.Length > 0
            ? Fin.Succ(sqlPath)
            : Fin.Fail<string>(new ComputeFault.AnalysisRunFailed("<pollination-no-sql-asset>"));
    }
}
```

## [06]-[RESEARCH]

- [PARAMETERIZED_DISCOVERY]: the EnergyPlus binary is resolved through a strict-priority discovery boundary — `ENERGYPLUS_EXE` env-var → `OPENSTUDIO_ENERGYPLUSDIR` env-var (a full OpenStudio installation's bundled binary) → the policy configured directory → the app's RID-native `runtimes/<rid>/native` bundled-runtime fallback (the OpenStudio SWIG package bundles no solver) — never a hardcoded path (`§4E`); a shipped app owns its own provisioning (a bundled 25.2.0 osx-arm64 binary or the env-var), and `Parametric_Forge` is a dev/CI probe toolchain only, never a shipped dependency. The discovery is fully parameterized by `EnergyToolchainPolicy`, and the simulation scenario by `EnergyPolicy` (the EUI target, the heating/cooling dual setpoints, the lighting/equipment densities), so a CI lane, a developer machine, and a shipped app each supply the binary and re-target the scenario their own way without an interior edit.
- [VERSION_LOCK]: the EnergyPlus version MUST track the OpenStudio SWIG version — OpenStudio 3.11.0 forward-translates an IDF the version-matched EnergyPlus 25.2.0 consumes. DEV/CI points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-BUNDLED 25.2.0 so the version-matched binary is the resolved default; the standalone EnergyPlus 26.1.0 (the Forge standalone) is mismatched and is NOT the dev/CI target. The resolver applies NO version filter (it probes paths in priority order), so should the discovery chain resolve a mismatched binary, `VersionGuard` checks the binary's SELF-REPORTED `--version` against the SWIG expectation (the binary is the version authority, never its path) and folds a warning fact into the receipt rather than producing an IDF the solver rejects opaquely.
- [OPENSTUDIO_IN_PROCESS]: OpenStudio (the SWIG SDK, `NREL.OpenStudio.macOS-arm64` 3.11.0, RID-locked osx-arm64) BUILDS the model in-process and READS the `SqlFile` results; it neither runs nor bundles the EnergyPlus solver. Every wrapper owns a native handle and is `IDisposable` (bracketed under `using` — the `Model`, the translator, the `Workspace`, the `EpwFile`, the `SqlFile`, and every `OptionalDouble`/`Point3d`/`*Vector`/`LogMessageVector` result), a model-object (`Space`/`Surface`/`ThermalZone`/`Construction`/`StandardOpaqueMaterial`/`ThermostatSetpointDualSetpoint`/`ScheduleConstant`/`SpaceType`) is owned BY the `Model` and never independently disposed, the SDK is single-threaded for model mutation (one serialized unit of work), the unique `SimulationControl`/`RunPeriod` objects are gotten-or-created off the model (neither carries a `(Model)` ctor), every load/get that can miss returns a SWIG `Optional<T>` lowered to `Option<T>` AND disposed at the boundary, and every file API takes a SWIG `Path` built through `OpenStudioUtilitiesCore.toPath` (there is no `Path(string)` ctor). The `*PINVOKE` marshaling classes are never a call surface. Compute admits OpenStudio for the SIMULATION concern, distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange owner; the model derives from the seam graph, never re-authored from IFC here. The OpenStudio surface catalog the energy runner mines is `Rasm.Compute/.api/api-openstudio` (the simulation-scoped surface — `Model` construction, the run-control + conditioning objects, `EnergyPlusForwardTranslator`, `SqlFile`, the SWIG `Optional<T>`/`IDisposable`/`Path` boundary), the per-folder twin of `Rasm.Bim/.api/api-openstudio` (the gbXML↔OSM semantic-exchange surface): one RID-locked osx-arm64 SDK, two folder-scoped `.api` catalogs each framing its own concern, aligned not coupled.
- [GRAPH_TO_OSM]: the OSM model is folded from the graph's spatial Object nodes — each `IfcSpace`-classified node a `Space` + `ThermalZone`, each bounding surface a `Surface` with a `Construction` lowered from the seam `MaterialComposition.LayerSet` (each layer a `StandardOpaqueMaterial` reading the seam `MaterialPropertySet.Thermal.Conductivity`/`SpecificHeat` and the `Mechanical.Density`, so the OSM construction U-value matches the `Analysis/aggregator` ISO 6946 fold) — then STAMPED with the annual-run context (`SimulationControl`/`RunPeriod`/`WeatherFile`), the ideal-air conditioning to the policy dual setpoints, and the policy internal-load `SpaceType`, so the translated IDF is a runnable conditioned annual model rather than a free-floating shell. The graph carries the spatial/semantic model baked in (spaces, surfaces, constructions, thermal properties) and references the analytical surface geometry BY CONTENT KEY, so the build reads the canonical graph plus the `GeometrySource`-resolved footprints and the conditioned floor area for the EUI; conditioning and the EUI denominator share one `IsConditioned` predicate so the model and the intensity agree. The spatial reads `SpacesOf`/`BoundingSurfacesOf`/`ConditionedFloorArea`/`IsConditioned` are COMPUTE-owned `ElementGraph` extensions (`EnergyGraphReads`, this page) composing the seam primitives + the projected `IfcRelSpaceBoundary` neutral `Generic` edges (`BoundingSurfacesOf` preferring the 2nd-level space-to-space boundaries when a model carries both levels — read off the Bim `SpatialBoundaries` `BoundaryLevel` payload — so a 1st+2nd export never double-counts the envelope) + the space `Qto_SpaceBaseQuantities`; the surface `FootprintPolygon` is resolved one-hop by content key off `surface.Representations.FootPrint` through the threaded `GeometrySource` port (the Object node carries NO inline `BoundaryPolygon` — the deleted §4-RT-M2 phantom); `CompositionOf`/`Material` are the seam-owned material reads. A glazing layer is the growth axis — the seam `MaterialPropertySet` carries no optical-transmittance case, so a transparent construction folds to `StandardGlazing`/`SimpleGlazing` only once the seam adds an optical case; opaque layers build from the `Thermal` case today. Ripple counterpart: `Rasm.Compute/Analysis/assessment` (the spine threading `GeometrySource` through `Assess`/`Run` to this runner), `Rasm.Bim/Projection/semantic` (the `EdgeProjection.SpatialBoundaries`/`DefinesProperties` folds + `IfcRepresentation.Keys` content-keying the analytical `FootPrint` into `Representations` feeding these reads, and the open question of whether a bounding-surface node carries its bounded element's construction), `Rasm.Element/Graph/element` (the seam-declared `FootprintPolygon` analytical-geometry type + the `GeometrySource` resolution port + the `RepresentationContentHash.FootPrint` content key + `CompositionOf`/`Material` reads), and `Rasm.Element/Composition/material` (the seam optical case that would unlock glazing).
- [SUBPROCESS_RESULTS]: the EnergyPlus subprocess (`energyplus -w weather -d outdir -r in.idf`) runs over the resolved binary in a bracketed scratch directory; a non-zero exit rails `AnalysisRunFailed` with the stderr tail; the `SqlFile` annual accessors yield the total site/source energy, the net source energy, and the site/source EUI (energy / conditioned floor area), and the structured `SqlFile.endUses()` summary folds the FULL per-end-use breakdown — one all-energy-fuel sum per `EndUseCategoryType` (`uses.getEndUse(fuel, category)` over the static `EndUses.fuelTypes()`/`categories()` vectors, the `Water` fuel excluded as m³ consumption rather than energy), so a gas- or district-heated building reports its real heating end use AND the interior/exterior lighting, interior/exterior equipment, fan, pump, and water-systems loads that dominate a modern EUI surface, never the electricity-only or HVAC-only slice; the `hoursSimulated()` hours ride the stream as a `Duration` annual-completeness validity fact — a full-year ideal-air run reports ~8760 h, so a short count signals the solver terminated early and the reported energy is a partial-year artifact, not a real annual result; the setpoint-not-met hours (the ASHRAE 90.1 ~300 h/yr cap) are an EnergyPlus tabular read (the `SystemSummary` "Time Setpoint Not Met" report) the C# `SqlFile` binding exposes through no accessor and no generic SQL exec, deferred to a SQLite reader over `eplusout.sql` — a growth axis, never a fabricated zero. Every result measure is SI-native `MeasureValue.OfSi` over the energy `Dimension` (composed `Force×Length`, `÷Area` for the EUI intensity) with the GJ→J coercion riding `UnitsNet.Energy` once — never a literal conversion factor and never the phantom 2-arg `MeasureValue.Of(value, "unit")` the seam factory does not expose; the verdict converts the SI EUI back to kWh·m⁻²·a⁻¹ through `UnitsNet.Energy` against the policy target, banding `NotApplicable` (`double.NaN`) when no target is carried. The sub-annual per-month consumption is the growth axis over `SqlFile.energyConsumptionByMonth(EndUseFuelType, EndUseCategoryType, MonthOfYear)`; the EnergyPlus `eplusout.sql` is the heavy artifact the spine's `AssessmentResult.ResultBlob` is meant to thread onto the persisted payload — realizable once the assessment dispatch carries an object-store sink (today the 3-arg `Run(graph, request, clocks)` dispatch has no sink, so the runner returns no blob and the scratch SQL is ephemeral). The assessment NODE keys into the Persistence artifact index by the same `(input subgraph, route, policy)` content-key the spine mints, so an identical building+weather is a cache hit and a 412-noop. The energy run currently has a Python (lbt-recipes) rail; this C#-first lane is additive (`§4E`).
- [CLOUD_PROVIDER_ROW]: the Pollination arm grounds against the decompile-verified `PollinationSDK 1.10.0` surface (`Rasm.Persistence/.api/api-pollination-sdk` the catalog tier) — `Wrapper.JobInfo.FromJson(string)` static / `ToJson()`, `JobInfo.RunJobAsync(Action<string>, CancellationToken) → Task<ScheduledJobInfo>` (upload assets + schedule in one), `ScheduledJobInfo.WatchJobStatusAsync(...) → Task<string>` (poll to a terminal status), the `RunInfo(ScheduledJobInfo)` ctor, `RunInfo.GetOutputAssets(string platform) → List<RunOutputAsset>`, `DownloadRunAssetsAsync(List<RunAssetBase>, string saveAsDir, ...)`, and the `RunStatusEnum` terminal vocabulary (`Succeeded`/`Failed`/`Cancelled` among the eight members). The row realizes the counterpart the Persistence integration blueprint routes to Compute: the job/orchestration half lands HERE as one `EnergyRoute` provider row while the durable half (presigned-grant object transfer, `ArtifactKind.CloudRun` reuse index, W3C-PROV run attribution) stays the Persistence owners' rows composed at the seam; the SDK runs OUTSIDE-RHINO with its vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` closure isolated from the STJ rails, token lifecycle rides the composition root's `Configuration`/`TokenRepo`, and the SDK-local `Wrapper.LocalDatabase`/`CheckCached` reuse is rejected in favor of the content-keyed index. The HBJSON payload inside the job descriptor is the `Rasm.Bim/Exchange/energy` content-keyed artifact — the cloud row consumes the Bim-lowered document over the object plane, never a Compute-side model derivation and never a Bim reference, so exchange (Bim), simulation dispatch (this page), and durable landing (Persistence) stay three owners on one artifact key.
