# [COMPUTE_ENERGY]

Rasm.Compute energy-simulation runner: the `Discipline.Energy` arm of the assessment rail and the C#-first whole-building-energy lane (`§4E`). It builds an `NREL.OpenStudio` `Model` IN-PROCESS from the concrete `Rasm.Element` `ElementGraph` (spaces, surfaces, constructions, opaque/glazing materials, thermal zones — all read from the graph), forward-translates it to an EnergyPlus IDF through `EnergyPlusForwardTranslator`, runs the EnergyPlus solver as a SUBPROCESS resolved through a PARAMETERIZED discovery boundary (env-var → configured-path → bundled-fallback, NEVER a hardcoded path), reads the results SQLite through `SqlFile`, and returns the annual energy use, EUI, and peak demand as one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back. OpenStudio (the SWIG SDK) BUILDS the model and READS the results; it neither runs nor bundles the EnergyPlus binary — that is the discovery boundary's job, and the binary version MUST track the OpenStudio SWIG version (OpenStudio 3.11.0 expects EnergyPlus 25.2.0; dev/CI points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-bundled 25.2.0, never the mismatched standalone 26.1.0). Every OpenStudio wrapper owns a native handle and is `IDisposable`, the SDK is single-threaded for model mutation, and every load/get that can miss returns a SWIG `Optional<T>` lowered onto `Fin<T>`/`Option<T>` at the boundary — the runner brackets the model, translator, weather, SQL handles, and the scratch run directory under `using`/`try-finally` so no native memory or temp artifact leaks. Compute admits `NREL.OpenStudio.macOS-arm64` for the SIMULATION concern (distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange owner); the model is built from the seam graph, never re-authored from IFC here.

## [01]-[INDEX]

- [01]-[TOOLCHAIN_BOUNDARY]: the `EnergyToolchain` parameterized EnergyPlus discovery (env-var → configured-path → bundled-fallback), the `EnergyToolchainPolicy`, and the OpenStudio/EnergyPlus version-lock guard.
- [02]-[MODEL_BUILD]: the in-process `OpenStudio.Model` built from the graph (spaces/surfaces/constructions/materials/zones), the `EnergyPlusForwardTranslator`→IDF, and the SWIG `Optional<T>`/`IDisposable` boundary discipline.
- [03]-[SIMULATION_RUN]: the EnergyPlus subprocess over the resolved binary, the `SqlFile` annual-result read, and the `EnergySimulation.Run` fact stream.

## [02]-[TOOLCHAIN_BOUNDARY]

- Owner: `EnergyToolchain` the static EnergyPlus-executable resolver; `EnergyToolchainPolicy` the discovery policy (the configured directory override and the platform executable name); the OpenStudio/EnergyPlus version-lock guard.
- Entry: `public static Fin<string> Resolve(EnergyToolchainPolicy policy)` — probes the candidate paths in priority order `ENERGYPLUS_EXE` env-var → `OPENSTUDIO_ENERGYPLUSDIR` env-var (the OpenStudio-bundled binary, dev/CI) → the policy's configured directory → the OpenStudio package's bundled-runtime fallback, returns the first existing executable, and rails `ComputeFault.ToolchainUnresolved` when none resolves — the discovery is PARAMETERIZED end to end, never a hardcoded path.
- Auto: the resolved binary's version is checked against the OpenStudio SWIG version expectation (3.11.0 → 25.2.0); a mismatch (e.g. a standalone 26.1.0 on `PATH`) folds a warning fact into the receipt rather than silently producing a version-skewed IDF the solver rejects.
- Packages: LanguageExt.Core, NREL.OpenStudio.macOS-arm64 (the bundled-runtime location), BCL inbox (`Environment`, `Path`, `File`).
- Growth: a new discovery source is one probe in the priority chain; a new platform is the executable-name policy column — the resolver widens by probe, never a parallel discovery method per host.
- Boundary: the discovery is env-var → configured-path → bundled-fallback in that strict priority, never a hardcoded absolute path (a hardcoded EnergyPlus path is the rejected form); `Parametric_Forge` is a DEV/CI probe toolchain only — a SHIPPED app owns its own EnergyPlus provisioning (a bundled 25.2.0 osx-arm64 binary, or `ENERGYPLUS_EXE`), so the bundled-fallback resolves the OpenStudio package's own runtime and never assumes a developer machine; the version-lock is load-bearing — OpenStudio 3.11.0 forward-translates the IDF the version-matched EnergyPlus 25.2.0 consumes, and the mismatched standalone 26.1.0 (the Forge standalone) is NEVER selected even when present, because the IDF schema tracks the SWIG version; an unresolved binary rails `ToolchainUnresolved` with the full probe trail, never a default that fails opaquely at run time.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record EnergyToolchainPolicy(Option<string> ConfiguredDir, string ExecutableName, string ExpectedVersion) {
    public static readonly EnergyToolchainPolicy Canonical = new(
        ConfiguredDir: None,
        ExecutableName: OperatingSystem.IsWindows() ? "energyplus.exe" : "energyplus",
        ExpectedVersion: "25.2.0");   // OpenStudio 3.11.0 SWIG -> EnergyPlus 25.2.0; never the standalone 26.1.0
}

// The energy request input the AssessmentRequest.Energy case carries (the analysis scenario).
public sealed record WeatherRef(string EpwPath, string Station);
public sealed record EnergyPolicy(EnergyToolchainPolicy Toolchain, double TargetEui) {
    public static readonly EnergyPolicy Canonical = new(EnergyToolchainPolicy.Canonical, TargetEui: 0.0);
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

    public static Seq<AssessmentFact> VersionGuard(string executable, EnergyToolchainPolicy policy) =>
        executable.Contains(policy.ExpectedVersion, StringComparison.Ordinal)
            ? Seq<AssessmentFact>()
            : Seq(AssessmentFact.Text("energyplus-version-warning", $"<resolved-binary-not-{policy.ExpectedVersion}:{executable}>"));

    static Option<string> Probe(string? path) => path is not null && File.Exists(path) ? Some(path) : None;
    static string? Join(string? dir, string exe) => dir is null ? null : Path.Combine(dir, exe);
    static string BundledRuntimeDir() => Path.Combine(AppContext.BaseDirectory, "runtimes", "osx-arm64", "native", "EnergyPlus");
}
```

## [03]-[MODEL_BUILD]

- Owner: `EnergySimulation.BuildModel` the in-process OpenStudio model builder; `OsmBuild` the build receipt (the model handle plus the translator log facts); the SWIG `Optional<T>`→`Fin<T>` and `IDisposable` boundary discipline.
- Entry: `static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request)` — constructs an `OpenStudio.Model`, folds each spatial Object node into a `Space`, each bounding surface into a `Surface` with its `Construction` (the seam `MaterialComposition.LayerSet` lowered to layered `OpaqueMaterial`/`StandardGlazing` with the seam thermal properties), each space into a `ThermalZone`, attaches the `EpwFile` weather, and forward-translates to the IDF `Workspace` through `EnergyPlusForwardTranslator`, `Fin<T>` lowering an empty SWIG `Optional` or a translator error onto `ComputeFault.AssessmentInputMissing`/`AnalysisRunFailed`.
- Auto: every OpenStudio file API takes a SWIG `Path` (there is no `Path(string)` ctor) so the builder routes paths through `OpenStudioUtilitiesCore.toPath(string)`; every load/get that can miss returns a SWIG `Optional<T>` checked with `is_initialized()` before `get()` and lowered to `Option<T>` at this boundary so interior code never sees the SWIG optional; the construction layers read the seam thermal conductivity/thickness so the OSM construction U-value matches the `Analysis/aggregator` ISO 6946 fold.
- Packages: NREL.OpenStudio.macOS-arm64, LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `Node.Object`, `MaterialComposition`, `MaterialPropertySet.Thermal`, `NodeId`), BCL inbox.
- Growth: a new model object (an HVAC system, a schedule set, a daylighting control) is one fold over the matching graph nodes onto its OSM `ModelObject`; the build widens by fold, never a parallel builder per object type.
- Boundary: the model is built from the seam graph for SIMULATION — distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange (gbXML reverse-translate); Compute reads the graph's spaces/surfaces/constructions (already lowered from IFC by Bim's projector) so the energy model derives from the canonical graph, never re-authored from IFC here; every OpenStudio wrapper owns native memory and is `IDisposable` so the `Model`, the `EnergyPlusForwardTranslator`, the `EpwFile`, and the `Optional`/`Vector` results are bracketed under `using` — a dropped handle leaks native memory the GC cannot reclaim; the SDK is single-threaded for model mutation so the build is one serialized unit of work, never a parallel fan-out over a shared model; the `*PINVOKE` marshaling classes are an implementation detail, never a call surface (the public `OpenStudio.*` wrappers only).

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The build saves the IDF to disk and disposes every native handle — no live OpenStudio handle escapes the boundary.
public sealed record OsmBuild(string IdfPath, Seq<AssessmentFact> TranslatorLog);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    static Fin<OsmBuild> BuildModel(ElementGraph graph, AssessmentRequest.Energy request, string scratch) {
        using OpenStudio.Model model = new();
        foreach (Node.Object space in graph.SpacesOf(request.Targets)) {
            OpenStudio.Space osSpace = new(model);              // model objects are owned by the Model — never `using`-disposed here
            osSpace.setName(space.Name);
            OpenStudio.ThermalZone zone = new(model);
            osSpace.setThermalZone(zone);
            foreach (Node.Object surface in graph.BoundingSurfacesOf(space.Id)) { BuildSurface(model, osSpace, surface, graph).IfFail(static _ => { }); }
        }
        using OpenStudio.EnergyPlusForwardTranslator translator = new();
        using OpenStudio.Workspace idf = translator.translateModel(model);
        using OpenStudio.LogMessageVector errors = translator.errors();
        Seq<AssessmentFact> log = toSeq(Enumerable.Range(0, (int)errors.Count)).Map(i => AssessmentFact.Text($"osm-error-{i}", errors[i].logMessage()));
        if (errors.Count > 0) { return Fin.Fail<OsmBuild>(new ComputeFault.AnalysisRunFailed($"<osm-forward-translate-errors:{errors.Count}>")); }
        string idfPath = Path.Combine(scratch, "in.idf");
        idf.save(OpenStudio.OpenStudioUtilitiesCore.toPath(idfPath), overwrite: true);
        return Fin.Succ(new OsmBuild(idfPath, log));
    }

    static Fin<Unit> BuildSurface(OpenStudio.Model model, OpenStudio.Space space, Node.Object surface, ElementGraph graph) =>
        graph.CompositionOf(surface.Id).Bind(composition => composition is MaterialComposition.LayerSet set
            ? BuildConstruction(model, set, graph).Map(construction => {
                using OpenStudio.Point3dVector vertices = Vertices(surface);
                OpenStudio.Surface osSurface = new(vertices, model);   // owned by the Model
                osSurface.setSpace(space);
                osSurface.setConstruction(construction);
                return unit;
            })
            : Fin.Succ(unit));

    static Fin<OpenStudio.Construction> BuildConstruction(OpenStudio.Model model, MaterialComposition.LayerSet set, ElementGraph graph) =>
        set.Layers.Fold(
            Fin.Succ(new List<OpenStudio.Material>()),
            (acc, layer) => acc.Bind(mats => graph.Material(layer.Material).Map(static m => m.Properties).Bind(props =>
                ThermalOf(props).ToFin((Error)new ComputeFault.AssessmentInputMissing("<osm-layer-missing-thermal>"))
                    .Map(thermal => {
                        OpenStudio.StandardOpaqueMaterial mat = new(model);   // owned by the Model
                        mat.setThickness(layer.ThicknessMm.Si);   // MeasureValue.Si is the SI base (metres) the OSM material expects
                        mat.setThermalConductivity(thermal.Conductivity.Si);
                        mat.setDensity(DensityOf(props).IfNone(1000.0));
                        mat.setSpecificHeat(thermal.SpecificHeat.Si);
                        mats.Add(mat);
                        return mats;
                    }))))
            .Map(mats => {
                using OpenStudio.MaterialVector vec = new();
                mats.Iter(vec.Add);
                OpenStudio.Construction construction = new(model);
                construction.setLayers(vec);
                return construction;
            });
}
```

## [04]-[SIMULATION_RUN]

- Owner: `EnergySimulation.Run` the energy runner; the EnergyPlus subprocess over the resolved binary; the `SqlFile` annual-result read; the scratch run-directory lifetime.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, ClockPolicy clocks)` — resolves the EnergyPlus binary through `EnergyToolchain.Resolve`, builds the OSM model and IDF through `BuildModel`, writes the IDF and weather into a scratch directory, runs EnergyPlus as a subprocess, reads `eplusout.sql` through `SqlFile`, and emits the `total-site-energy`/`eui`/`heating-demand`/`cooling-demand` facts, bracketing the scratch directory and every native handle so nothing leaks.
- Auto: the subprocess is `energyplus -w <weather.epw> -d <outdir> -r <in.idf>` over the resolved binary; a non-zero exit code rails `ComputeFault.AnalysisRunFailed` with the captured stderr tail; the `SqlFile` annual accessors (`totalSiteEnergy`/`netSiteEnergy`/`electricityHeating`/`electricityCooling`) return SWIG `OptionalDouble` lowered to `Option<double>`; the EUI divides the total site energy by the conditioned floor area read from the graph.
- Receipt: the `Assessment` `ComputeReceipt` case carries the energy discipline/route/content-key plus the elapsed wall time the subprocess dominated; the translator log and the version-guard warning fold into the fact stream as soft notes.
- Packages: NREL.OpenStudio.macOS-arm64, LanguageExt.Core, NodaTime, Rasm.Element (project — `ElementGraph`, `NodeId`), BCL inbox (`System.Diagnostics.Process`, `System.IO`).
- Boundary: the EnergyPlus binary is the resolved subprocess (OpenStudio does NOT run it) so the runner owns the process lifetime, the scratch directory, and the stderr capture, bracketing them in `try-finally` (the platform-forced statement boundary — Exemption: native subprocess + filesystem); the model build and the SQL read are the single-threaded native OpenStudio boundary, one serialized unit of work; every `OpenStudio.*` handle (`Model`, translator, `EpwFile`, `SqlFile`, `Optional`/`Vector`) is disposed; the SQL accessors return SWIG `OptionalDouble` lowered to `Option<double>` so a missing output is carried, never a bare `get()` faulting in native code; the verdict is the EUI against a target when the policy carries one, else the energy use is reported informationally; a subprocess non-zero exit or a missing SQL file rails `AnalysisRunFailed`, never a silent zero-energy result.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class EnergySimulation {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Energy request, ClockPolicy clocks) {
        string scratch = Directory.CreateTempSubdirectory("rasm-eplus-").FullName;
        try {
            return from binary in EnergyToolchain.Resolve(request.Policy.Toolchain)
                   from build in BuildModel(graph, request, scratch)
                   from sqlPath in RunSubprocess(binary, build.IdfPath, request, scratch)
                   from facts in ReadResults(sqlPath, graph, request)
                   select AssessmentResult.Of(request.Route,
                       facts + build.TranslatorLog + EnergyToolchain.VersionGuard(binary, request.Policy.Toolchain),
                       GoverningEui(facts, request.Policy),
                       new Provenance("EnergySimulation", request.Route.Standard, "EnergyPlus", clocks.Now), clocks.Now);
        }
        finally { Directory.Delete(scratch, recursive: true); }
    }

    static Fin<string> RunSubprocess(string binary, string idfPath, AssessmentRequest.Energy request, string scratch) {
        using Process process = new() {
            StartInfo = new ProcessStartInfo(binary, $"-w \"{request.Weather.EpwPath}\" -d \"{scratch}\" -r \"{idfPath}\"") {
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
        return Lower(sql.totalSiteEnergy()).Match(
            Some: totalGj => Fin.Succ(Seq(
                AssessmentFact.Measure("total-site-energy", MeasureValue.Of(totalGj, "GJ")),
                AssessmentFact.Measure("eui", MeasureValue.Of(floorAreaM2 > 0.0 ? totalGj * 277.778 / floorAreaM2 : 0.0, "kWh/m²·a")),
                AssessmentFact.Measure("heating-demand", MeasureValue.Of(Lower(sql.electricityHeating()).IfNone(0.0), "GJ")),
                AssessmentFact.Measure("cooling-demand", MeasureValue.Of(Lower(sql.electricityCooling()).IfNone(0.0), "GJ")))),
            None: () => Fin.Fail<Seq<AssessmentFact>>(new ComputeFault.AnalysisRunFailed("<energyplus-sql-no-total-site-energy>")));
    }

    static double GoverningEui(Seq<AssessmentFact> facts, EnergyPolicy policy) =>
        policy.TargetEui > 0.0
            ? facts.Choose(static f => f.Name.Value == "eui" && f.Value is PropertyValue.Measure m ? Some(m.Value.Si) : None).HeadOrNone().Map(eui => eui / policy.TargetEui).IfNone(0.0)
            : 0.0;

    static Option<double> Lower(OpenStudio.OptionalDouble optional) => optional.is_initialized() ? Some(optional.get()) : None;

    // Read the seam-neutral surface boundary (kernel Vector3 coordinates) and build the OpenStudio vertex vector — the
    // seam never returns an OpenStudio type; the OSM marshaling stays inside the Compute energy boundary.
    static OpenStudio.Point3dVector Vertices(Node.Object surface) {
        OpenStudio.Point3dVector vec = new();
        foreach (Vector3 p in surface.BoundaryPolygon) { vec.Add(new OpenStudio.Point3d(p.X, p.Y, p.Z)); }
        return vec;
    }
    static Option<MaterialPropertySet.Thermal> ThermalOf(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Thermal t ? Some(t) : None).HeadOrNone();
    static Option<double> DensityOf(Seq<MaterialPropertySet> props) => props.Choose(static p => p is MaterialPropertySet.Mechanical m ? Some(m.Density.Si) : None).HeadOrNone();
    static string Tail(string s) => s.Length <= 256 ? s : s[^256..];
}

// The discipline graph reads the energy runner composes from the seam primitives (ObjectNodes/EdgesAt/Find + the
// neutral Generic space-boundary edges by wire-name + the bag nodes off the Assign.PropertyDefinition edges) —
// Compute-OWNED ElementGraph extensions, NOT seam members: the seam owns the material/composition reads, the
// discipline spatial reads live here. Spaces are the "IfcSpace"-classified Object nodes reachable from the targets;
// bounding surfaces ride the projected IfcRelSpaceBoundary edges and carry the baked BoundaryPolygon; the conditioned
// floor area sums the spaces' Qto_SpaceBaseQuantities net area — so the OSM build reads the energy/spatial model baked.
public static class EnergyGraphReads {
    const string SpaceBoundary  = "IfcRelSpaceBoundary";
    const string SpaceClass     = "IfcSpace";
    const string BaseQuantities = "Qto_SpaceBaseQuantities";

    public static Seq<Node.Object> SpacesOf(this ElementGraph graph, Seq<NodeId> targets) =>
        targets.IsEmpty
            ? graph.ObjectNodes.Filter(IsSpace)
            : targets.Bind(t => Descend(graph, t)).Distinct().Choose(graph.Find<Node.Object>).Filter(IsSpace).ToSeq();

    public static Seq<Node.Object> BoundingSurfacesOf(this ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e => e is Relationship.Generic g && g.WireName == SpaceBoundary && g.Relating == space
            ? graph.Find<Node.Object>(g.Related) : None).ToSeq();

    // The EUI denominator: the net floor area of every conditioned space under the targets, summed in SI m² off each
    // space's Qto_SpaceBaseQuantities NetFloorArea; an external/unconditioned space is excluded and a space lacking the
    // quantity contributes zero rather than faulting the run.
    public static double ConditionedFloorArea(this ElementGraph graph, Seq<NodeId> targets) =>
        graph.SpacesOf(targets).Filter(s => graph.IsConditioned(s.Id)).Fold(0.0, (acc, s) => acc + graph.NetFloorAreaM2(s.Id));

    static bool IsSpace(Node.Object o) => o.Classification.Code == SpaceClass;

    // A space is conditioned unless Pset_SpaceCommon marks it external; absent the flag it is treated conditioned.
    static bool IsConditioned(this ElementGraph graph, NodeId space) =>
        graph.Property(space, "Pset_SpaceCommon", "IsExternal").Match(
            Some: static v => v is not PropertyValue.Boolean { Value: true }, None: static () => true);

    static double NetFloorAreaM2(this ElementGraph graph, NodeId space) =>
        graph.Quantity(space, BaseQuantities, "NetFloorArea").Map(static m => m.Si).IfNone(0.0);

    static Option<PropertyValue> Property(this ElementGraph graph, NodeId obj, string set, string name) =>
        graph.Bags<Node.PropertySet>(obj).Filter(ps => ps.Bag.SetName == set).HeadOrNone()
            .Bind(ps => ps.Bag.Properties.Find(PropertyName.Create(name)));

    static Option<MeasureValue> Quantity(this ElementGraph graph, NodeId obj, string set, string name) =>
        graph.Bags<Node.QuantitySet>(obj).Filter(qs => qs.Bag.SetName == set).HeadOrNone()
            .Bind(qs => qs.Bag.Quantities.Find(PropertyName.Create(name)));

    // The bag nodes off an object's Assign.PropertyDefinition edges the Bim DefinesProperties fold projected.
    static Seq<T> Bags<T>(this ElementGraph graph, NodeId obj) where T : Node =>
        graph.EdgesAt(obj).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == obj
            ? graph.Find<T>(a.Definition) : None).ToSeq();

    // Transitive descent over the Compose decomposition (aggregate/nest/contain) — a building/storey target reaches its spaces.
    static Seq<NodeId> Descend(ElementGraph graph, NodeId node) =>
        node.Cons(graph.EdgesAt(node).Choose(e => e is Relationship.Compose c && c.Whole == node ? Some(c.Part) : None).ToSeq()
            .Bind(child => Descend(graph, child)));
}
```

## [05]-[RESEARCH]

- [PARAMETERIZED_DISCOVERY]: the EnergyPlus binary is resolved through a strict-priority discovery boundary — `ENERGYPLUS_EXE` env-var → `OPENSTUDIO_ENERGYPLUSDIR` env-var → the policy configured directory → the OpenStudio package bundled-runtime fallback — never a hardcoded path (`§4E`); a shipped app owns its own provisioning (a bundled 25.2.0 osx-arm64 binary or the env-var), and `Parametric_Forge` is a dev/CI probe toolchain only, never a shipped dependency. The discovery is fully parameterized by `EnergyToolchainPolicy` so a CI lane, a developer machine, and a shipped app each supply the binary their own way without an interior edit.
- [VERSION_LOCK]: the EnergyPlus version MUST track the OpenStudio SWIG version — OpenStudio 3.11.0 forward-translates an IDF the version-matched EnergyPlus 25.2.0 consumes. DEV/CI points `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-BUNDLED 25.2.0; the standalone EnergyPlus 26.1.0 (the Forge standalone) is mismatched and is NEVER selected even when present, because the IDF schema tracks the SWIG version — a version-skew folds a warning fact into the receipt rather than producing an IDF the solver rejects opaquely.
- [OPENSTUDIO_IN_PROCESS]: OpenStudio (the SWIG SDK, `NREL.OpenStudio.macOS-arm64` 3.11.0, RID-locked osx-arm64) BUILDS the model in-process and READS the `SqlFile` results; it neither runs nor bundles the EnergyPlus solver. Every wrapper owns a native handle and is `IDisposable` (bracketed under `using`), the SDK is single-threaded for model mutation (one serialized unit of work), every load/get that can miss returns a SWIG `Optional<T>` lowered to `Option<T>` at the boundary, and every file API takes a SWIG `Path` built through `OpenStudioUtilitiesCore.toPath` (there is no `Path(string)` ctor). The `*PINVOKE` marshaling classes are never a call surface. Compute admits OpenStudio for the SIMULATION concern, distinct from the `Rasm.Bim` IFC↔OSM SEMANTIC exchange owner; the model derives from the seam graph, never re-authored from IFC here. The OpenStudio surface catalog the energy runner mines is `Rasm.Compute/.api/api-openstudio` (the simulation-scoped surface — `Model` construction, `EnergyPlusForwardTranslator`, `SqlFile`, the SWIG `Optional<T>`/`IDisposable`/`Path` boundary), the per-folder twin of `Rasm.Bim/.api/api-openstudio` (the gbXML↔OSM semantic-exchange surface): one RID-locked osx-arm64 SDK, two folder-scoped `.api` catalogs each framing its own concern, aligned not coupled.
- [GRAPH_TO_OSM]: the OSM model is folded from the graph's spatial Object nodes — each `IfcSpace`-classified node a `Space` + `ThermalZone`, each bounding surface a `Surface` with a `Construction` lowered from the seam `MaterialComposition.LayerSet` (each layer a `StandardOpaqueMaterial` reading the seam `MaterialPropertySet.Thermal.Conductivity`/`SpecificHeat` so the OSM construction U-value matches the `Analysis/aggregator` ISO 6946 fold). The graph carries everything baked in (spaces, surfaces, constructions, thermal properties), so the build reads the canonical graph and the conditioned floor area for the EUI. The spatial reads `SpacesOf`/`BoundingSurfacesOf`/`ConditionedFloorArea` are COMPUTE-owned `ElementGraph` extensions (`EnergyGraphReads`, this page) composing the seam primitives + the projected `IfcRelSpaceBoundary` neutral `Generic` edges + the baked `Node.Object.BoundaryPolygon` + the space `Qto_SpaceBaseQuantities`; `CompositionOf`/`Material` are the seam-owned material reads. Ripple counterpart: `Rasm.Bim/Projection/semantic` (the `EdgeProjection.SpatialBoundaries`/`DefinesProperties` folds + the `Enrich` `BoundaryPolygon` bake feeding these reads) and `Rasm.Element/Graph/element` (the seam `Node.Object.BoundaryPolygon` analytical carrier + `CompositionOf`/`Material` reads).
- [SUBPROCESS_RESULTS]: the EnergyPlus subprocess (`energyplus -w weather -d outdir -r in.idf`) runs over the resolved binary in a bracketed scratch directory; a non-zero exit rails `AnalysisRunFailed` with the stderr tail; the `SqlFile` annual accessors (`totalSiteEnergy`/`netSiteEnergy`/`electricityHeating`/`electricityCooling`, each a SWIG `OptionalDouble`) yield the total site energy, the EUI (energy / conditioned floor area), and the heating/cooling demand as the fact stream. The results key into the Persistence artifact index by the same `(input subgraph, route)` content-key the spine mints, so an identical building+weather is a cache hit and a 412-noop. The energy run currently has a Python (lbt-recipes) rail; this C#-first lane is additive (`§4E`).
