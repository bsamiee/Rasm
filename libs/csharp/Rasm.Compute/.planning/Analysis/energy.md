# [COMPUTE_ENERGY]

Rasm.Compute owns the whole-building energy-simulation runner — the `Discipline.Energy` arm of the assessment rail. It builds an `NREL.OpenStudio` `Model` in-process from the concrete `Rasm.Element` `ElementGraph` (spaces, bounding surfaces, layered opaque constructions, thermal zones), stamps the annual EnergyPlus run context (weather-file `SimulationControl`, a full-year `RunPeriod`, the attached `EpwFile`), conditions each occupied zone with ideal-air loads driven to policy dual setpoints and policy lighting/equipment gains so demand is the envelope-driven load, forward-translates to an IDF through `EnergyPlusForwardTranslator`, runs EnergyPlus as a subprocess over a parameterized binary-discovery boundary, reads the result SQLite through `SqlFile`, and returns the annual site/source energy, the site/source EUI, the full per-end-use breakdown across every fuel, and the hours-simulated annual-completeness signal as one `AssessmentResult` fact stream. Execution is one route axis — the `EnergyRoute` `[Union]` dispatches a local EnergyPlus subprocess (default row) or a Pollination cloud run — one entry, one `SqlFile` result read, provider variance as row data.

OpenStudio (the SWIG SDK) builds the model and reads the results; it neither runs nor bundles the EnergyPlus binary, and its version fixes the EnergyPlus the toolchain must resolve. Every wrapper owns a native handle and is `IDisposable`, model mutation is single-threaded, and every load/get that can miss returns a SWIG `Optional<T>` lowered onto `Fin<T>`/`Option<T>` at the boundary — so the runner brackets the model, translator, weather, and SQL handles, every optional/point/vector result, and the scratch run directory under `using`/`try-finally`. Compute admits `NREL.OpenStudio.macOS-arm64` for the SIMULATION concern, distinct from the `Rasm.Bim/Energy/exchange` energy-model EXCHANGE owner — the model is folded from the seam graph, never re-authored from IFC. A cloud run consumes the Bim-lowered HBJSON `EnergyArtifact` off the object plane; its downloaded assets and the local `eplusout.sql` both land content-keyed on the Persistence blob lane, and the assessment node keys `(input subgraph, route, policy)` so an identical model and recipe resolve from the index.

## [01]-[INDEX]

- [01]-[TOOLCHAIN_BOUNDARY]: the `EnergyToolchain` parameterized EnergyPlus discovery (env-var → configured-path → bundled-fallback), the `EnergyPolicy` simulation scenario (toolchain + EUI target + conditioning setpoints + internal-load densities), and the OpenStudio/EnergyPlus version-lock guard.
- [02]-[MODEL_BUILD]: the in-process `OpenStudio.Model` built from the graph (spaces/surfaces/constructions/zones), the annual-run context (`SimulationControl`/`RunPeriod`/`WeatherFile`), the ideal-air conditioning and internal-load folds, the `EnergyPlusForwardTranslator`→IDF, and the SWIG `Optional<T>`/`IDisposable`/`Path` boundary discipline.
- [03]-[SIMULATION_RUN]: the EnergyPlus subprocess over the resolved binary, the `SqlFile` annual result read (site/source energy, site/source EUI, the full per-end-use breakdown folded from `SqlFile.endUses()`, and the hours-simulated annual-completeness validity fact), and the `EnergySimulation.Run` fact stream.
- [04]-[CLOUD_ROUTE]: the `EnergyRoute` `[Union]` provider axis on `EnergyPolicy` (`Local` subprocess default · `Cloud` Pollination row) — the `RunCloud` arm over the `PollinationSDK` job/run/asset orchestration converging on the ONE `ReadResults` fold, faults on the existing 2200-band codes.

## [02]-[TOOLCHAIN_BOUNDARY]

- Owner: `EnergyToolchain` the static EnergyPlus-executable resolver; `EnergyToolchainPolicy` the discovery policy (configured-directory override, platform executable name, expected-version datum); `EnergyPolicy` the simulation scenario the `AssessmentRequest.Energy` case carries (the `EnergyRoute` row, the toolchain, the EUI target, the dual setpoints, the lighting/equipment densities the model build reads); the version-lock guard.
- Entry: `public static Fin<string> Resolve(EnergyToolchainPolicy policy)` probes candidate paths in priority `ENERGYPLUS_EXE` → `OPENSTUDIO_ENERGYPLUSDIR` (a full OpenStudio installation's bundled binary, dev/CI) → the policy configured directory → the app's RID-native `runtimes/<rid>/native` fallback, returns the first existing executable, and rails `ComputeFault.ToolchainUnresolved` with the full probe trail when none resolves — discovery parameterized end to end, never a hardcoded path.
- Auto: `VersionGuard` checks the binary's self-reported `energyplus --version` banner against the policy expected version (the binary is the version authority, never its path); a mismatch folds a warning fact rather than emitting a version-skewed IDF the solver rejects opaquely.
- Packages: LanguageExt.Core, NREL.OpenStudio.macOS-arm64 (the SWIG SDK whose version the toolchain locks EnergyPlus to — it bundles no solver, and the resolver touches no OpenStudio API), BCL inbox (`Environment`/`Path`/`File`/`AppContext` for the probes, `System.Diagnostics.Process` for the `--version` self-report).
- Growth: a new discovery source is one probe in the chain; a new platform the executable-name column; a new simulation knob (ventilation rate, infiltration default, sized HVAC plant selector) one `EnergyPolicy` column; a new execution provider one `EnergyRoute` case on `[04]` — resolver widens by probe, scenario by column, provider by row, never a parallel discovery method per host.
- Boundary: `Parametric_Forge` is a dev/CI probe toolchain only — a shipped app owns its EnergyPlus provisioning (a bundled RID-native binary or `ENERGYPLUS_EXE`), so the fallback resolves the app's own `runtimes/<rid>/native` runtime and never assumes a developer machine. Version-lock is load-bearing: OpenStudio forward-translates an IDF only the version-matched EnergyPlus consumes, so dev/CI points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-bundled solver, not a mismatched standalone; the resolver applies no version filter, so a mismatched binary IS selected and `VersionGuard` folds the warning. Conditioning and internal-load defaults are explicit `EnergyPolicy` knobs, never ambient constants, so a consumer re-targets a climate or building type without an interior edit; an unresolved binary rails `ToolchainUnresolved`, never a default that fails opaquely.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EnergyToolchainPolicy(Option<string> ConfiguredDir, string ExecutableName, string ExpectedVersion) {
    public static readonly EnergyToolchainPolicy Canonical = new(
        ConfiguredDir: None,
        ExecutableName: OperatingSystem.IsWindows() ? "energyplus.exe" : "energyplus",
        ExpectedVersion: "25.2.0");   // OpenStudio SWIG version fixes this; a mismatched standalone is rejected, not run
}

// AssessmentRequest.Energy weather: the EPW the subprocess runs over (-w) and the OSM WeatherFile embeds.
public sealed record WeatherRef(string EpwPath, string Station);

// The simulation scenario: route row, toolchain, EUI target (kWh/m2.a), and conditioning + internal-load defaults the
// build reads. Every column is a knob a consumer re-targets per climate/building-type without touching the builder;
// the provider is the [04] EnergyRoute row, deployment data, never a second entry.
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

    // The binary is the version authority, not its path: EnergyPlus self-reports its version on `--version`, so the
    // guard probes the executable rather than grepping the path (a version-named directory is no guarantee). An
    // undetermined probe reports the marker rather than a false match, so a real skew surfaces.
    public static Seq<AssessmentFact> VersionGuard(string executable, EnergyToolchainPolicy policy) {
        string reported = ProbeVersion(executable);
        return reported.Contains(policy.ExpectedVersion, StringComparison.Ordinal)
            ? Seq<AssessmentFact>()
            : Seq(AssessmentFact.Text("energyplus-version-warning",
                $"<resolved-binary-not-{policy.ExpectedVersion}:reported={reported}:{executable}>"));
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
        catch (SystemException ex) { return $"<version-probe-failed:{ex.GetType().Name}>"; }
    }

    static Option<string> Probe(string? path) => path is not null && File.Exists(path) ? Some(path) : None;
    static string? Join(string? dir, string exe) => dir is null ? null : Path.Combine(dir, exe);
    // Last-resort probe: the app's own RID-native runtimes/<rid>/native location (the .NET native-asset convention), not
    // the OpenStudio SWIG package, which bundles no solver. A dev/CI box resolves earlier via OPENSTUDIO_ENERGYPLUSDIR.
    static string BundledRuntimeDir() => Path.Combine(AppContext.BaseDirectory, "runtimes", "osx-arm64", "native", "EnergyPlus");
}
```

## [03]-[MODEL_BUILD]

- Owner: `EnergySimulation.BuildModel` the in-process OpenStudio model builder; `OsmBuild` the build receipt (IDF path plus translator-and-skip log facts); `ConfigureRun`/`SetpointSchedules`/`InternalLoads`/`Condition`/`BuildSurface`/`BuildOpenings`/`BuildConstruction`/`Layer`/`Vertices` the model-object folds; the SWIG `Optional<T>`→`Fin<T>`, `IDisposable`, and `Path` boundary discipline.
- Entry: `static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, string scratch)` guards the weather EPW, builds a `Model`, stamps the annual context, folds each spatial node into a `Space`+`ThermalZone`, each bounding surface into a `Surface` with its resolved footprint and layered `Construction`, each `Host`-attributed opening into a typed `SubSurface`, and forward-translates to the IDF `Workspace`, `Fin<T>` lowering a missing weather/composition or a translator error onto `ComputeFault.AssessmentInputMissing`/`AnalysisFailed`.
- Auto: every OpenStudio file API takes a SWIG `Path` (no `Path(string)` ctor), so paths route through `OpenStudioUtilitiesCore.toPath`; the unique `SimulationControl`/`RunPeriod` objects are gotten-or-created through the static `OpenStudioModelSimulation.get*(model)` module functions (neither carries a `(Model)` ctor, and the binding surfaces these as module functions, not `Model` instance methods); the construction fold discriminates on the seam property case — an all-`Optical` set builds `StandardGlazing` layers, any other builds `StandardOpaqueMaterial` through the 6-arg ctor (the shorter ctor forms backfill OpenStudio defaults for the omitted thermal columns — fabricated physics, the rejected admission) so the OSM U-value matches the `Analysis/aggregator` ISO 6946 fold, and a mixed opaque+optical set rails to the default-construction note; every load/get that can miss returns a SWIG `Optional<T>` checked with `is_initialized()` before `get()`.
- Packages: NREL.OpenStudio.macOS-arm64, LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `Node.Object`, `MaterialComposition`, `MaterialPropertySet.Thermal`/`.Mechanical`/`.Optical` via `MaterialPropertyAccess`, `MaterialLayer`, `NodeId`, `RepresentationContentHash`, `FootprintPolygon`, the host-neutral `Vector3` its ring carries, `GeometrySource` the analytical-surface resolution port), BCL inbox.
- Growth: a new model object (HVAC plant, schedule set, daylighting control, infiltration object) is one fold over the matching nodes; conditioning widens from ideal-air to a sized plant by one `EnergyPolicy` selector; `SimpleGlazing` is the assembly-shorthand row one `Layer` arm adds when a whole-window U/SHGC case rides the seam — the build widens by fold and policy column, never a parallel builder per object type.
- Boundary: the model is built from the seam graph for SIMULATION, distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange — Compute reads the graph's already-lowered spaces/surfaces/constructions, never re-authored from IFC. Every OpenStudio wrapper is `IDisposable` and bracketed under `using` (the `Model`, translator, `Workspace`, `EpwFile`, every point/vector/log-vector, the result optionals) — a dropped handle leaks native memory the GC cannot reclaim; a model-object is owned BY the `Model` it is `new`-ed against and never independently disposed; model mutation is single-threaded so the build is one serialized unit, never a parallel fan-out; the `*PINVOKE` marshaling classes are never a call surface. A bounding surface whose composition is absent or non-layered keeps the OpenStudio default construction and a logged fact rather than a dropped surface (a missing surface is an envelope hole — geometry always added, only the construction degrades). Fenestration constructions land only on `SubSurface` openings — EnergyPlus rejects one on a base surface, which is why `BoundingSurfacesOf` excludes the `Host`-attributed opening boundaries.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// No live OpenStudio handle escapes the boundary; TranslatorLog folds the forward-translate warnings/errors and any
// surface that fell back to a default construction.
public sealed record OsmBuild(string IdfPath, Seq<AssessmentFact> TranslatorLog);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    // Energy SI dimension composed from the seam Dimension algebra (force x length); EUI divides by area. No hand-mapped
    // kind; the magnitude coerces GJ->J through UnitsNet once (never a literal factor). EnergyPlus reports every value in GJ.
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
            foreach (Node.Object surface in graph.BoundingSurfacesOf(space.Id)) { notes += BuildSurface(model, osSpace, space.Id, surface, graph, geometry); }
        }
        using OpenStudio.EnergyPlusForwardTranslator translator = new();
        using OpenStudio.Workspace idf = translator.translateModel(model);
        using OpenStudio.LogMessageVector errors = translator.errors();
        using OpenStudio.LogMessageVector warnings = translator.warnings();
        Seq<AssessmentFact> log = notes
            + toSeq(Enumerable.Range(0, (int)errors.Count)).Map(i => AssessmentFact.Text($"osm-error-{i}", errors[i].logMessage()))
            + toSeq(Enumerable.Range(0, (int)warnings.Count)).Map(i => AssessmentFact.Text($"osm-warning-{i}", warnings[i].logMessage()));
        if (errors.Count > 0) { return Fin.Fail<OsmBuild>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<osm-forward-translate-errors:{errors.Count}>")); }
        string idfPath = Path.Combine(scratch, "in.idf");
        using OpenStudio.Path outPath = OpenStudio.OpenStudioUtilitiesCore.toPath(idfPath);   // the SWIG Path is itself a native handle
        idf.save(outPath, overwrite: true);
        return Fin.Succ(new OsmBuild(idfPath, log));
    }

    // The annual context: SimulationControl runs the weather-file period, RunPeriod spans the year, the EpwFile is the
    // design context (-w is the authoritative run weather), FullExterior solar distribution avoids the interior variants'
    // zone-convexity requirement on imported geometry. SimulationControl/RunPeriod/OutputTableSummaryReports have no
    // (Model) ctor — the binding surfaces their get-or-create as static OpenStudioModelSimulation.get*(model) functions.
    static void ConfigureRun(OpenStudio.Model model, AssessmentRequest.Energy request) {
        OpenStudio.SimulationControl control = OpenStudio.OpenStudioModelSimulation.getSimulationControl(model);
        control.setRunSimulationforWeatherFileRunPeriods(true);
        control.setSolarDistribution("FullExterior");
        OpenStudio.RunPeriod run = OpenStudio.OpenStudioModelSimulation.getRunPeriod(model);
        run.setBeginMonth(1); run.setBeginDayOfMonth(1); run.setEndMonth(12); run.setEndDayOfMonth(31);
        // The annual SqlFile readers depend on the ABUPS + End-Uses tabular reports, emitted only when AllSummary is
        // requested — armed here (get-or-create, idempotent) so a result read never rides an ambient translator default.
        OpenStudio.OpenStudioModelSimulation.getOutputTableSummaryReports(model).addSummaryReport("AllSummary");
        using OpenStudio.Path epwPath = OpenStudio.OpenStudioUtilitiesCore.toPath(request.Weather.EpwPath);
        using OpenStudio.EpwFile epw = new(epwPath);
        using OpenStudio.OptionalWeatherFile attached = OpenStudio.WeatherFile.setWeatherFile(model, epw);  // the returned optional is itself a native handle — bracketed, never dropped
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

    // Geometry is always added so the zone stays enclosed; the construction attaches when the seam composition resolves to
    // a LayerSet, else the surface keeps the default construction and a logged note (a dropped surface is an envelope hole).
    // The Host-attributed openings fold as SubSurfaces in the same pass.
    static Seq<AssessmentFact> BuildSurface(OpenStudio.Model model, OpenStudio.Space space, NodeId spaceId, Node.Object surface, ElementGraph graph, GeometrySource geometry) =>
        // The polygon resolves one-hop by content key through GeometrySource off surface.Representations.FootPrint (no
        // inline BoundaryPolygon on the node). An absent/undecodable footprint is a logged envelope-hole note, not a
        // dropped zero-area surface — the same graceful-skip shape the missing-construction note takes.
        geometry.Footprint(surface.Representations).Match(
            Some: footprint => {
                using OpenStudio.Point3dVector vertices = Vertices(footprint);
                OpenStudio.Surface osSurface = new(vertices, model);              // owned by the Model
                osSurface.setSpace(space);
                Seq<AssessmentFact> notes = graph.CompositionOf(surface.Id).Bind(static c => c is MaterialComposition.LayerSet set ? Some(set) : None).Match(
                    Some: set => BuildConstruction(model, set, graph).Match(
                        Succ: construction => { osSurface.setConstruction(construction); return Seq<AssessmentFact>(); },
                        Fail: error => Seq(AssessmentFact.Text($"osm-surface-default-construction:{surface.Id.Value}", error.Message))),
                    None: () => Seq(AssessmentFact.Text($"osm-surface-default-construction:{surface.Id.Value}", "<no-layerset-composition>")));
                return notes + BuildOpenings(model, osSurface, spaceId, surface.ExternalId.IfNone(surface.Name), graph, geometry);
            },
            None: () => Seq(AssessmentFact.Text($"osm-surface-footprint-unresolved:{surface.Id.Value}", "<no-footprint-representation>")));

    // Host-attributed openings land as SubSurfaces on their host Surface — EnergyPlus accepts a fenestration construction
    // only on a sub-surface (BoundingSurfacesOf excludes the opening edges). IfcWindow -> FixedWindow, or Skylight on a
    // RoofCeiling host (EnergyPlus validates the type against host tilt, so the host's surfaceType() is the discriminant),
    // else Door; the construction builds through the same BuildConstruction fold off the opening's composition.
    static Seq<AssessmentFact> BuildOpenings(OpenStudio.Model model, OpenStudio.Surface host, NodeId spaceId, string hostIdentifier, ElementGraph graph, GeometrySource geometry) =>
        graph.OpeningsOf(spaceId, hostIdentifier).Bind(opening =>
            geometry.Footprint(opening.Representations).Match(
                Some: ring => {
                    using OpenStudio.Point3dVector vertices = Vertices(ring);
                    OpenStudio.SubSurface sub = new(vertices, model);             // owned by the Model
                    sub.setSurface(host);
                    sub.setSubSurfaceType(opening.Classification.Code == EnergyGraphReads.WindowClass
                        ? (host.surfaceType() == "RoofCeiling" ? "Skylight" : "FixedWindow")
                        : "Door");
                    return graph.CompositionOf(opening.Id).Bind(static c => c is MaterialComposition.LayerSet set ? Some(set) : None).Match(
                        Some: set => BuildConstruction(model, set, graph).Match(
                            Succ: construction => { sub.setConstruction(construction); return Seq<AssessmentFact>(); },
                            Fail: error => Seq(AssessmentFact.Text($"osm-opening-default-construction:{opening.Id.Value}", error.Message))),
                        None: () => Seq<AssessmentFact>());
                },
                None: () => Seq(AssessmentFact.Text($"osm-opening-footprint-unresolved:{opening.Id.Value}", "<no-footprint-representation>"))));

    // One construction fold, the seam property case the discriminant: all-Optical is fenestration, Optical-free is opaque,
    // a mixed set has no legal EnergyPlus construction and rails to the caller's default-construction note.
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
            Some: optical => {
                OpenStudio.StandardGlazing glass = new(model, "SpectralAverage", layer.Thickness.Si);   // owned by the Model
                glass.setSolarTransmittanceatNormalIncidence(optical.SolarTransmittance);
                glass.setFrontSideSolarReflectanceatNormalIncidence(optical.SolarReflectanceFront);
                glass.setBackSideSolarReflectanceatNormalIncidence(optical.SolarReflectanceBack);
                glass.setVisibleTransmittanceatNormalIncidence(optical.VisibleTransmittance);
                glass.setFrontSideVisibleReflectanceatNormalIncidence(optical.VisibleReflectanceFront);
                glass.setBackSideVisibleReflectanceatNormalIncidence(optical.VisibleReflectanceBack);
                glass.setInfraredTransmittanceatNormalIncidence(optical.ThermalIrTransmittance);
                glass.setFrontSideInfraredHemisphericalEmissivity(optical.ThermalIrEmissivityFront);
                glass.setBackSideInfraredHemisphericalEmissivity(optical.ThermalIrEmissivityBack);
                props.Thermal.Iter(thermal => glass.setThermalConductivity(thermal.Conductivity.Si));
                return Fin.Succ<OpenStudio.Material>(glass);
            },
            None: () => props.Thermal
                .ToFin((Error)new ComputeFault.AssessmentInputMissing($"<osm-layer-missing-thermal:{layer.Material.Value}>"))
                .Map(thermal => (OpenStudio.Material)new OpenStudio.StandardOpaqueMaterial(model, "MediumRough",
                    layer.Thickness.Si, thermal.Conductivity.Si,
                    props.Mechanical.Map(static m => m.Density.Si).IfNone(1000.0), thermal.SpecificHeat.Si)));
}
```

## [04]-[SIMULATION_RUN]

- Owner: `EnergySimulation.Run` the one energy entry dispatching the `EnergyRoute` row; `RunLocal` the subprocess arm; `RunSubprocess` the EnergyPlus subprocess; `ReadResults`/`EndUseFacts`/`ValidityFacts`/`GoverningEui`/`Slug`/`Lower`/`Vertices` the `SqlFile` result read shared by both routes; the scratch run-directory lifetime.
- Entry: `static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks)` resolves the binary through `EnergyToolchain.Resolve`, builds the OSM model and IDF, runs the subprocess over the scratch directory, reads `eplusout.sql` through `SqlFile`, and emits the site/source totals, the `eui`/`source-eui` pair (positive conditioned area only), the `end-use:<category>` family, and the `hours-simulated` validity fact, bracketing the scratch directory and every native handle.
- Auto: the subprocess is `energyplus -w <weather> -d <outdir> -r <idf>`; a non-zero exit rails `ComputeFault.AnalysisFailed` with the stderr tail. Per-end-use breakdown folds the structured `SqlFile.endUses()` summary — one all-fuel sum per `EndUseCategoryType` over the static `EndUses.fuelTypes()`/`categories()` vectors (each handle bracketed, the SWIG marshaling exemption), the `Water` fuel (m³ consumption) the one excluded column — so a gas- or district-heated building reports its real heating end use and the lighting/equipment/fan/pump loads a whole-building EUI carries. EUI divides energy by conditioned floor area and is emitted only over a positive area, so a zero-area set carries no intensity fact and the verdict bands `NotApplicable` rather than a fabricated 0.0-EUI Satisfied. A `hours-simulated` fact carries `SqlFile.hoursSimulated()` as a `Duration` so a downstream verdict rejects a partial-year run; the setpoint-not-met hours have no `SqlFile` accessor and no generic SQL exec, a growth axis a SQLite reader opens. Every measure is SI-native `MeasureValue.OfSi` with the GJ→J coercion riding `UnitsNet.Energy` once; the verdict converts the SI EUI back to kWh·m⁻²·a⁻¹ against the policy target, projecting `double.NaN` when no target is carried.
- Receipt: the `Assessment` `ComputeReceipt` case carries the energy discipline/route/content-key plus the elapsed wall time; the translator log, the default-construction notes, and the version-guard warning fold in as soft notes.
- Packages: Microsoft.Data.Sqlite (the read-only tabular reader for the setpoint-not-met rows the SWIG `SqlFile` exposes no accessor for; folder admission on this first compose, the central pin held), NREL.OpenStudio.macOS-arm64 (the `SqlFile` totals + structured `EndUses` fold + `hoursSimulated` + the static run-context helpers), UnitsNet (the GJ→J / J→kWh / hours→s coercions), LanguageExt.Core, NodaTime, Rasm.Element (project — `ElementGraph`, `Dimension`, `MeasureValue`, `PropertyValue`, `NodeId`), BCL inbox.
- Boundary: the EnergyPlus binary is the resolved subprocess (OpenStudio does not run it), so the runner owns the process lifetime, scratch directory, and stderr capture, bracketed in `try-finally` (Exemption: native subprocess + filesystem); the model build and SQL read are the single-threaded native boundary; every OpenStudio handle is disposed; the SQL accessors return SWIG `OptionalDouble` lowered to `Option<double>`, never a bare `get()` faulting in native code. Result measures are SI-native `MeasureValue.OfSi` (a phantom 2-arg `MeasureValue.Of(value, "unit")` is the rejected form — the seam factory is `Of(value, unit, key)`/`OfSi(dimension, si)`); a non-zero exit or a missing SQL file rails `AnalysisFailed`, never a silent zero-energy result. Sub-annual per-month consumption is a growth axis over `SqlFile.energyConsumptionByMonth`, one fold deeper when a seasonal/demand-shape route requires it.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    static Fin<AssessmentResult> RunLocal(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks) {
        // The native + subprocess boundary rails onto Fin: a SWIG ctor, a model mutation, a Process.Start over a bad binary,
        // or a corrupt SqlFile throws a SystemException the entry owes the caller as AnalysisFailed, never an escape. Scratch
        // creation is inside the bracket and cleanup is best-effort, so a delete fault never masks the run's Fin result.
        string scratch = "";
        try {
            scratch = Directory.CreateTempSubdirectory("rasm-eplus-").FullName;
            return from binary in EnergyToolchain.Resolve(request.Policy.Toolchain)
                   from build in BuildModel(graph, request, geometry, scratch)
                   from sqlPath in RunSubprocess(binary, build.IdfPath, request, scratch)
                   from facts in ReadResults(sqlPath, graph, request)
                   from tabular in TabularFacts(sqlPath)
                   // The eplusout.sql bytes land through the AssessmentSink egress before the scratch bracket deletes them
                   // — content-addressed onto the Persistence blob lane (ArtifactKind.Assessment), the key riding ResultBlob.
                   from blob in sink.Store(File.ReadAllBytes(sqlPath))
                   select AssessmentResult.Of(request.Route,
                       facts + tabular + build.TranslatorLog + EnergyToolchain.VersionGuard(binary, request.Policy.Toolchain),
                       GoverningEui(facts, request.Policy),
                       new Provenance("EnergySimulation", request.Route.Standard, $"EnergyPlus {request.Policy.Toolchain.ExpectedVersion}", clocks.Now),
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

    static Fin<Seq<AssessmentFact>> ReadResults(string sqlPath, ElementGraph graph, AssessmentRequest.Energy request) {
        using OpenStudio.Path resultsPath = OpenStudio.OpenStudioUtilitiesCore.toPath(sqlPath);
        using OpenStudio.SqlFile sql = new(resultsPath);
        double floorAreaM2 = graph.ConditionedFloorArea(request.Targets);
        // totalSiteEnergy is the required headline output (its absence is a failed run, never a silent zero); the source
        // energy, the site/source EUI, the per-end-use breakdown, and the hours-simulated validity fact ride alongside.
        return Lower(sql.totalSiteEnergy()).Match(
            Some: siteGj => {
                double sourceGj = Lower(sql.totalSourceEnergy()).IfNone(0.0);
                // The EUI exists only over a positive conditioned floor area: a zero-area set emits no eui/source-eui fact,
                // GoverningEui reads NaN, and the verdict bands NotApplicable rather than a fabricated 0.0-EUI Satisfied.
                Fin<Seq<AssessmentFact>> intensity = floorAreaM2 > 0.0
                    ? AssessmentFact.Rows(
                        AssessmentFact.Measure("eui", EuiDim, Joules(siteGj) / floorAreaM2),
                        AssessmentFact.Measure("source-eui", EuiDim, Joules(sourceGj) / floorAreaM2))
                    : FinSucc(Seq<AssessmentFact>());
                return from head in AssessmentFact.Rows(
                        AssessmentFact.Measure("total-site-energy", EnergyDim, Joules(siteGj)),
                        AssessmentFact.Measure("total-source-energy", EnergyDim, Joules(sourceGj)),
                        AssessmentFact.Measure("net-source-energy", EnergyDim, Joules(Lower(sql.netSourceEnergy()).IfNone(0.0))))
                    from perArea in intensity
                    from uses in EndUseFacts(sql)
                    from validity in ValidityFacts(sql)
                    select head + perArea + uses + validity;
            },
            None: () => Fin.Fail<Seq<AssessmentFact>>(new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, "<energyplus-sql-no-total-site-energy>")));
    }

    // One all-fuel sum per EndUseCategoryType folded from the structured SqlFile.endUses() summary, the fuel taxonomy
    // enumerated from the static EndUses.fuelTypes(); the Water fuel (m3 consumption) is the one column excluded from the
    // GJ sum. The index-loop + per-element `using` is the SWIG disposal boundary (the vector indexer returns a disposable
    // handle per category/fuel) — the same marshaling exemption Vertices takes.
    static Fin<Seq<AssessmentFact>> EndUseFacts(OpenStudio.SqlFile sql) {
        using OpenStudio.OptionalEndUses optional = sql.endUses();
        if (!optional.is_initialized()) { return FinSucc(Seq<AssessmentFact>()); }
        using OpenStudio.EndUses uses = optional.get();
        using OpenStudio.EndUseCategoryTypeVector categories = OpenStudio.EndUses.categories();
        using OpenStudio.EndUseFuelTypeVector fuels = OpenStudio.EndUses.fuelTypes();
        List<Fin<AssessmentFact>> facts = new(categories.Count);
        for (int c = 0; c < categories.Count; c++) {
            using OpenStudio.EndUseCategoryType category = categories[c];
            double categoryGj = 0.0;
            for (int f = 0; f < fuels.Count; f++) {
                using OpenStudio.EndUseFuelType fuel = fuels[f];
                if (fuel.valueName() != WaterFuel) { categoryGj += uses.getEndUse(fuel, category); }
            }
            facts.Add(AssessmentFact.Measure($"end-use:{Slug(category.valueName())}", EnergyDim, Joules(categoryGj)));
        }
        return toSeq(facts).TraverseM(identity).As();
    }

    // The annual hours actually simulated (SqlFile.hoursSimulated, the one hours accessor the binding exposes): a full run
    // reports ~8760 h, so a short count means the solver terminated early and the energy is a partial-year artifact a
    // downstream verdict must reject; an absent hoursSimulated contributes no fact, never a fabricated zero.
    static Fin<Seq<AssessmentFact>> ValidityFacts(OpenStudio.SqlFile sql) =>
        Lower(sql.hoursSimulated()).Match(
            Some: static h => HoursFact("hours-simulated", h).Map(static fact => Seq(fact)),
            None: static () => FinSucc(Seq<AssessmentFact>()));

    // The eplusout.sql tabular reader (Microsoft.Data.Sqlite, read-only + unpooled so it neither mutates nor locks the
    // solver's file): the SWIG SqlFile exposes no accessor and no generic SQL exec for TabularDataWithStrings, so the
    // setpoint-not-met validity hours read here through one query family parameterized on (report, table, row, column),
    // never a per-metric ladder; a missing/malformed table stays railed as an absent fact, never a fabricated zero.
    static Fin<Seq<AssessmentFact>> TabularFacts(string sqlPath) {
        using Microsoft.Data.Sqlite.SqliteConnection connection = new($"Data Source={sqlPath};Mode=ReadOnly;Pooling=False;");
        connection.Open();
        return Seq(
                ("time-setpoint-not-met-heating", "AnnualBuildingUtilityPerformanceSummary", "Comfort and Setpoint Not Met Summary", "Time Setpoint Not Met During Occupied Heating"),
                ("time-setpoint-not-met-cooling", "AnnualBuildingUtilityPerformanceSummary", "Comfort and Setpoint Not Met Summary", "Time Setpoint Not Met During Occupied Cooling"))
            .Choose(row => Tabular(connection, row.Item2, row.Item3, row.Item4).Map(hours => HoursFact(row.Item1, hours)))
            .TraverseM(identity).As();
    }

    static Option<double> Tabular(Microsoft.Data.Sqlite.SqliteConnection connection, string report, string table, string row) {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM TabularDataWithStrings WHERE ReportName = $report AND TableName = $table AND RowName = $row LIMIT 1";
        command.Parameters.AddWithValue("$report", report);
        command.Parameters.AddWithValue("$table", table);
        command.Parameters.AddWithValue("$row", row);
        return Optional(command.ExecuteScalar()).Bind(static value => double.TryParse($"{value}", System.Globalization.CultureInfo.InvariantCulture, out double parsed) ? Some(parsed) : None);
    }

    static Fin<AssessmentFact> HoursFact(string name, double hours) =>
        AssessmentFact.Measure(name, Dimension.DurationDim, UnitsNet.Duration.FromHours(hours).Seconds);

    // The governing ratio is the site EUI (read back from the emitted fact, one source) against the policy target; with no
    // target (or no eui fact) the ratio is double.NaN so the verdict bands NotApplicable, never a 0.0-ratio Satisfied.
    static double GoverningEui(Seq<AssessmentFact> facts, EnergyPolicy policy) =>
        policy.TargetEui > 0.0
            ? facts.Choose(static f => f.Name.Value == "eui" && f.Value is PropertyValue.Measure m ? Some(m.Value.Si) : None)
                .Head.Map(euiSi => UnitsNet.Energy.FromJoules(euiSi).KilowattHours / policy.TargetEui).IfNone(double.NaN)
            : double.NaN;

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

    // Read a SWIG OptionalDouble onto Option<double> and dispose the handle (a getter's OptionalDouble is itself
    // disposable, so a bare read leaks) — the one place a missing output becomes None, never a faulting get().
    static Option<double> Lower(OpenStudio.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    // Build the OpenStudio vertex vector from the seam Vector3 ring — each Point3d disposed immediately after Add (the
    // vector copies it), so the marshaling leaks nothing and never escapes the Compute energy boundary as an OSM type.
    static OpenStudio.Point3dVector Vertices(FootprintPolygon footprint) {
        OpenStudio.Point3dVector vec = new();
        foreach (Vector3 p in footprint.Ring) { using OpenStudio.Point3d point = new(p.X, p.Y, p.Z); vec.Add(point); }
        return vec;
    }
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
    const string BaseQuantities = "Qto_SpaceBaseQuantities";
    const string SecondLevel    = "2nd";                // prefer 2nd-level (space-to-space) boundaries so a 1st+2nd export never double-counts the envelope; the "" undeclared rows read 1st-equivalent
    // The two hot edge-attr keys minted once (a per-edge PropertyName.Create inside the Choose lambdas re-validated the key on every read).
    static readonly PropertyName BoundaryLevelAttr = PropertyName.Create("BoundaryLevel");   // the Bim payload key — three-valued: "1st"/"2nd" plus "" for a base-class boundary whose level the file never declared
    static readonly PropertyName HostAttr = PropertyName.Create("Host");                     // a Host-bearing boundary edge is an OPENING, never an opaque base surface

    public static Seq<Node.Object> SpacesOf(this ElementGraph graph, Seq<NodeId> targets) =>
        targets.IsEmpty
            ? graph.ObjectNodes.Filter(IsSpace)
            : targets.Bind(t => Descend(graph, t)).Distinct().Choose(graph.Find<Node.Object>).Filter(IsSpace).ToSeq();

    // A space's bounding surfaces ride the projected IfcRelSpaceBoundary edges; a Host-attributed edge is an OPENING and
    // is excluded here (it folds as a SubSurface via OpeningsOf). When a model carries both 1st- and 2nd-level boundaries,
    // only the 2nd-level set is read so the envelope is never double-counted; a 1st-level-only or undeclared-level ""
    // model rides the 1st-equivalent arm — the secondLevel filter excludes it beside a declared set, the fallback includes it.
    public static Seq<Node.Object> BoundingSurfacesOf(this ElementGraph graph, NodeId space) {
        Seq<(Relationship.Generic Edge, Node.Object Surface)> boundaries =
            graph.EdgesAt(space).Choose(e => e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
                && g.Attributes.Find(HostAttr).IsNone
                ? graph.Find<Node.Object>(g.Related).Map(s => (Edge: g, Surface: s)) : None).ToSeq();
        Seq<(Relationship.Generic Edge, Node.Object Surface)> secondLevel = boundaries.Filter(static b =>
            b.Edge.Attributes.Find(BoundaryLevelAttr).Exists(static v => v is PropertyValue.Text { Value: SecondLevel }));
        return (secondLevel.IsEmpty ? boundaries : secondLevel).Map(static b => b.Surface);
    }

    // The Host-attributed opening boundaries of one host surface: the space's boundary edges whose Host attribute names
    // the host identifier — read back by identifier, never a NodeId join (rooted ids are raise-local).
    public static Seq<Node.Object> OpeningsOf(this ElementGraph graph, NodeId space, string hostIdentifier) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
                && g.Attributes.Find(HostAttr).Exists(v => v is PropertyValue.Text t && t.Value == hostIdentifier)
                ? graph.Find<Node.Object>(g.Related) : None).ToSeq();

    // The EUI denominator: the net floor area of every conditioned space under the targets, summed off each space's
    // Qto NetFloorArea; an unconditioned space is excluded and a space lacking the quantity contributes zero, not a fault.
    public static double ConditionedFloorArea(this ElementGraph graph, Seq<NodeId> targets) =>
        graph.SpacesOf(targets).Filter(s => graph.IsConditioned(s.Id)).Fold(0.0, (acc, s) => acc + graph.NetFloorAreaM2(s.Id));

    // A space is conditioned unless Pset_SpaceCommon marks it external; absent the flag, conditioned. The same predicate
    // gates the OSM ideal-air conditioning and the EUI denominator, so the model and the intensity agree.
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

- Owner: `EnergyRoute` the closed execution-provider `[Union]` on `EnergyPolicy` (`Subprocess` the local default · `Cloud` the Pollination row carrying owner/project/job-descriptor/platform as neutral values); `EnergySimulation.Run` the one entry whose generated total `Switch` dispatches the row; `RunCloud` the Pollination arm; `Orchestrate` the bracketed async SDK kernel.
- Entry: `public static Fin<AssessmentResult> Run(...)` dispatches `request.Policy.Route` — `Subprocess` enters `RunLocal` (`[04]`), `Cloud` enters `RunCloud`, which submits the app-authored job descriptor, watches the run to a terminal status, gates on `RunStatusEnum.Succeeded`, pulls the result assets, locates the downloaded `eplusout.sql`, and converges on the same `ReadResults` fold `[04]` owns — one result read serves both providers, so the fact stream, EUI verdict, and receipt shape are route-invariant.
- Auto: the HBJSON payload inside the job descriptor is the `Rasm.Bim/Energy/exchange` content-keyed `EnergyArtifact` the app root staged — Compute references no Bim type, the model travels as a document artifact on the object plane. Downloaded assets land content-keyed on the Persistence object plane exactly as the local `eplusout.sql` does, and the assessment node keys the same `(input subgraph, route, policy)` content key, so a re-submitted identical model+recipe resolves from the Persistence index; the SDK's `Wrapper.LocalDatabase` and its path-existence `CheckCached` are not composed (path-existence reuse without hash verification is the integrity gap the content-keyed index closes).
- Receipt: the `Assessment` receipt carries the cloud provenance beside the route/content-key columns; the watch-status trail folds in as soft notes.
- Packages: PollinationSDK (the `Wrapper` job/run/asset orchestration + `RunStatusEnum` terminal vocabulary — sidecar-isolated: its vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` closure never meets the STJ rails nor loads in-Rhino), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new cloud provider is one `EnergyRoute` case plus one arm (the `Switch` breaks every dispatch site at compile time); a recipe change is job-descriptor data, never a signature; per-output typed decodes beyond the SQLite widen `Orchestrate` by one asset row.
- Boundary: `Configuration`/`TokenRepo` auth is composition-root input to the ambient SDK configuration, never a policy column or fence member (the Persistence token-lifecycle law). Async orchestration is one blocking boundary kernel bracketed with the scratch directory (Exemption: sidecar HTTP + filesystem), and classification is exception-typed — an `ApiException`/HTTP transport fault maps `ComputeFault.EndpointUnreachable` while every other raise, a failed terminal, or a missing SQL asset maps `AnalysisFailed` — zero new band codes. Artifact residency (presigned-grant transfer, `ArtifactKind.CloudRun` reuse index, PROV attribution) stays the Persistence owners' rows composed at the seam. A cloud-side model rebuild from the graph is the rejected form — cloud consumes the Bim-lowered HBJSON, local consumes the in-process OSM build, two rows on one axis.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The execution-provider axis: the case IS the route, provider coordinates ride it as neutral values — no PollinationSDK
// type enters the policy, the SDK closure stays inside the RunCloud boundary (sidecar law).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyRoute {
    private EnergyRoute() { }

    public sealed record Subprocess : EnergyRoute;

    // Owner/Project name the Pollination project; JobDescriptor is the app-authored Wrapper.JobInfo JSON (recipe ref +
    // inputs incl. the staged HBJSON artifact); Platform keys GetOutputAssets. The descriptor is canonical analysis
    // identity — it folds verbatim into the assessment content key, so it references inputs by object-plane content key
    // and carries no volatile token (no local path, signed URL, timestamp, auth material, or SDK Local* provisioning
    // column) — a volatile token over-keys the cache and silently re-runs a token-metered cloud job, the named defect.
    public sealed record Cloud(string Owner, string Project, string JobDescriptor, string Platform) : EnergyRoute;

    public static readonly EnergyRoute Local = new Subprocess();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    // One entry, provider rows: the generated Switch makes a new provider a compile-broken case, never a knob.
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, GeometrySource geometry, AssessmentSink sink, ClockPolicy clocks) =>
        request.Policy.Route.Switch(
            subprocess: _ => RunLocal(graph, request, geometry, sink, clocks),
            cloud:      c => RunCloud(graph, request, c, sink, clocks));

    // Submit -> watch -> pull -> the same ReadResults fold. The async orchestration is one blocking boundary kernel
    // (Exemption: sidecar HTTP + filesystem) bracketed with the scratch directory; auth is composition-root input.
    // Classification is exception-typed: an ApiException/HTTP transport fault is EndpointUnreachable; every other raise,
    // a failed terminal, or a missing SQL asset is AnalysisFailed — a descriptor defect is never misreported as unreachable.
    static Fin<AssessmentResult> RunCloud(ElementGraph graph, AssessmentRequest.Energy request, EnergyRoute.Cloud route, AssessmentSink sink, ClockPolicy clocks) {
        string scratch = "";
        try {
            scratch = Directory.CreateTempSubdirectory("rasm-pollination-").FullName;
            return Try.lift(() => Orchestrate(route, scratch).GetAwaiter().GetResult()).Run()
                .MapFail(error => (Error)(error.Exception.Case is PollinationSDK.Client.ApiException or HttpRequestException
                    ? new ComputeFault.EndpointUnreachable($"<pollination:{Tail(error.Message)}>")
                    : new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Foreign, $"<pollination-run:{Tail(error.Message)}>")))
                .Bind(static fin => fin)
                .Bind(sqlPath => ReadResults(sqlPath, graph, request)
                    .Bind(facts => TabularFacts(sqlPath)
                        .Bind(tabular => sink.Store(File.ReadAllBytes(sqlPath))
                            .Map(blob => AssessmentResult.Of(request.Route, facts + tabular, GoverningEui(facts, request.Policy),
                                new Provenance("EnergySimulation", request.Route.Standard, $"Pollination {route.Owner}/{route.Project}", clocks.Now),
                                Some(blob))))));
        }
        finally { if (scratch.Length > 0) { try { Directory.Delete(scratch, recursive: true); } catch (IOException) { } } }
    }

    // Wrapper orchestration: JobInfo.FromJson -> RunJobAsync (upload + schedule) -> WatchJobStatusAsync (poll to terminal)
    // -> RunInfo.GetOutputAssets(platform) -> DownloadRunAssetsAsync into scratch. The SDK's LocalDatabase/CheckCached
    // reuse is not composed — the Persistence content-keyed index owns reuse.
    static async Task<Fin<string>> Orchestrate(EnergyRoute.Cloud route, string scratch) {
        PollinationSDK.Wrapper.JobInfo job = PollinationSDK.Wrapper.JobInfo.FromJson(route.JobDescriptor);
        PollinationSDK.Wrapper.ScheduledJobInfo scheduled = await job.RunJobAsync();
        string status = await scheduled.WatchJobStatusAsync();
        if (!status.Contains(nameof(PollinationSDK.RunStatusEnum.Succeeded), StringComparison.OrdinalIgnoreCase)) {
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

- [BOUNDING_SURFACE_CONSTRUCTION]-[BLOCKED]: does the Bim-projected `IfcRelSpaceBoundary` surface node carry its bounded element's `MaterialComposition.LayerSet`, or must `BuildSurface` join to the bounded wall/slab for the construction; verify against `Rasm.Bim/Projection/semantic` `EdgeProjection.SpatialBoundaries`.
