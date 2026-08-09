# [APPHOST_SANDBOX_HOST]

Capability-brokered plugin sandboxing for the runtime spine: two `Isolation` rows reach a vehicle here — `wasm` under a Wasmtime core-module instance with a WASI-Preview-1 granted-descriptor import table, `process` under an out-of-process child — and every other axis row refuses at admission with typed evidence naming the axis. Zero ambient authority is structural: a plugin reaches host capability only through a brokered grant handle, quotas cap CPU, memory, wall, and egress, and a kill-or-quarantine rail evicts a misbehaving plugin and disposes its vehicle.

Every artifact admits through the ONE `Sandbox/admission#SUPPLY_CHAIN_GATE` `SupplyChainGate.Admit` as `AdmissionSubject.Plugin` before it loads. `Isolation`, `ProfileAxis`, and `AxisEvidence` arrive settled from `Runtime/profiles#PROFILE_AXIS`, and `SupplyChainGate`/`AdmissionSubject`/`PluginArtifact`, `CapabilityDescriptor`/`GrantBroker`/`GrantScope`, `CommandAlgebra`, `OutboundHop.CompanionSpawn`/`Discovery`, `PeerAdmission`, `CancelScope`, `DegradationCell`, and `ReceiptSinkPort` follow as settled vocabulary; this page mints no eighth port.

## [01]-[INDEX]

- [02]-[ISOLATION_AXIS]: Sandbox seating of the two reachable `Isolation` rows, the axis refusal covering the rest, and the no-ambient-authority load law.
- [03]-[GRANT_HANDLE]: Capability-brokered grant handle with per-call authority mediation.
- [04]-[QUOTA_CONTROL]: CPU/memory/wall/egress quota cell over call-entry gating and epoch preemption, with kill and quarantine rail.

## [02]-[ISOLATION_AXIS]

- Owner: `SandboxRow` per-isolation policy record; `SandboxRows` the frozen row set carrying the admitting dispatch and the axis refusal; `PluginInstance` the loaded-plugin capsule carrying one isolation vehicle per row; `WasmCapsule` the owned `Wasmtime` store/instance/module/linker lifetime; `EpochPacer` the engine-wide interruption ticker; `SandboxFault` `[Union]` fault family deriving its codes through `FaultBand.Sandbox`; `Runtime/profiles#PROFILE_AXIS` `Isolation` owns the axis itself, composed whole here and never re-spelled sandbox-locally.
- Cases: two of the five `Isolation` rows seat a sandbox vehicle — `wasm` runs the plugin as a Wasmtime core-module instance with a linear-memory boundary and import-only host access, `process` runs it as an out-of-process child reached over the local-ipc hop with OS-level isolation; `in-proc`, `thread`, and `remote` carry no vehicle and refuse at admission; the two vehicles are SYMMETRIC slots on `PluginInstance` (`Option<WasmCapsule> Capsule` beside `Option<CompanionPeer> Child`), so neither row is prose while the other is a handle; `SandboxFault` = Text | LoadRejected | NoAuthority | QuotaExceeded | Quarantined | AxisUnsupported.
- Entry: `Fin<SandboxRow> Row` is the extension property total state-free `Switch` from axis value to frozen row, admitting the two sandbox-reachable rows and refusing the other three with `SandboxFault.AxisUnsupported` carrying the `AxisEvidence` that names the `isolation` axis; `Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime)` returns `IO<PluginInstance>` — the ONE `Sandbox/admission` gate admits the artifact as `AdmissionSubject.Plugin` (an all-empty artifact rejects `AttestationMissing` by construction), the row materializes its isolation vehicle, and the plugin loads with exactly the brokered grant scope and no ambient authority; `Enter<T>(PluginInstance plugin, Func<Instance, T> call)` returns `IO<T>` — the ONE crossing into a loaded guest, so the embedding's `TrapException` is observed at exactly one seat and its code lands on the instance's capsule before any caller re-classifies it; `SandboxRuntime.Preempting(int stackBytes)` mints the one engine every row shares with both preemption mechanisms armed, and `EpochPacer.Open(SandboxRuntime runtime, Func<Seq<PluginInstance>> live)` starts the one engine-wide `TimeProvider` ticker whose every tick advances the epoch and sweeps the live set through `Enforce`, returning its lease.
- Auto: the `wasm` row MATERIALIZES the embedding rather than describing it — `Module.FromBytes` compiles the admitted `PluginArtifact.Component`, one `Store` per instance takes `SetWasiConfiguration` over a `WasiConfiguration` whose `WithPreopenedDirectory(host, guest, WasiDirectoryPermissions, WasiFilePermissions)` rows are exactly the `GrantScope` filesystem grants, `Linker.DefineWasi()` mounts the WASI-Preview-1 descriptors that pre-open set scopes, and one `Linker.DefineFunction(module, name, callback, parameterKinds, resultKinds)` lands per granted `CapabilityDescriptor` — so the import table IS the grant scope and an ungranted host capability is absent from the linkage, the no-ambient-authority law being a structural property rather than a runtime check; `Store.SetLimits(memorySize: row.QuotaShape.MaxMemoryBytes)` caps linear memory, `Config.WithFuelConsumption(true)` with the seeded `Store.Fuel` meters instructions, and `Config.WithMaximumStackSize` bounds recursion depth; the host callback reaches its call frame through `Caller` alone — `Caller.GetMemory(name)` then `Memory.GetSpan(address, length)` reads the guest's serialized `CommandArguments` and writes the result back — never a `Store` captured in the closure, which outlives the frame; a process row spawns the child through `OutboundHop.CompanionSpawn` and reaches it over `OutboundHop.LocalIpc`, reading the child's `PeerCredential` at accept through `PeerAdmission`, so the child holds no host handle and every host call crosses the brokered control hop; the row's `QuotaShape` column seats the quota cell at load so the limits arrive with the instance, never bolted on after; dispose follows the embedding hierarchy — store before engine — so the capsule releases its store, instance, module, and linker while the engine outlives every plugin.
- Receipt: `SandboxReceipt` — plugin id, the typed `Isolation` row, granted scope hash, eviction reason, and the evicted arm's own convergence discriminant (the observed `TrapCode` for a wasm vehicle, the independent post-kill residual census for a child), `Instant`; the load transition logs through one `SpineLog` event and mints no receipt, because a load either yields the `PluginInstance` or fails on the rail and a receipt asserting `Loaded: true` beside a returned instance carries nothing the instance does not.
- Packages: Wasmtime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new linear-memory or OS-isolation backend settles as one `Isolation` value at the axis owner and one `SandboxRow` with one vehicle slot here, never a parallel loader and never a sandbox-local axis; a new granted host capability is one `DefineFunction` row the scope fold already emits; a new fault is one `SandboxFault` case; zero new surface.
- Boundary: seating the axis is not owning it — a sandbox-local isolation vocabulary collides its `process` row with the axis owner's and re-spells its `wasm` one, so admitting a narrower subset of a closed axis is a REFUSAL at this page's own entry and never a second roster, and the axis stays whole for every consumer that reaches it; the sandbox is the only plugin-load owner — a direct `Assembly.LoadFrom`, a plugin `AppDomain`, or an in-process plugin reference is the deleted form, so a plugin never shares the host's managed heap or ambient `IServiceProvider`; the WASM runtime is `Wasmtime` (the NuGet package id) at core-module + WASI-Preview-1, and that is a SETTLED ceiling rather than a pending probe: the managed assembly exposes `Config.WithComponentModel(bool)`, which toggles the NATIVE engine's component support, and NOT ONE managed component type — `Module`, `Linker`, and `Instance` are core-module only, while `TrapCode.CannotEnterComponent`/`NoAsyncResult` merely surface Rust-core trap rows — so WASI Preview 2 is unreachable from managed code, a component host is a native-side embedding rather than a `Wasmtime.NET` row, and the granted-descriptor import-table law, fuel metering, and linear-memory caps all run on the core-module `Engine`/`Linker`/`Store` surface; the Preview-1 `WasiConfiguration` surface — the `wasi_config_*` wraps, the four-argument `WithPreopenedDirectory` over `WasiDirectoryPermissions`/`WasiFilePermissions`, and `Store.SetWasiConfiguration` — is therefore the axis's whole sandbox-capability vocabulary and no page waits on a wider one; a hand-rolled WASM host is the deleted form; isolation is orthogonal to the composition density law — the host composes its own modules in-process through `CompositionSurface`, but a third-party plugin always crosses an isolation boundary, so the two load paths never merge; the wasm import table and the process control-hop verb set are both projections of the granted `CapabilityDescriptor` set, so a plugin's reachable surface is exactly its grant scope in both topologies; the process row reuses the `Discovery`/`CompanionPeer` spawn-attach mechanics verbatim and adds only the quota and grant columns, never re-declaring the spawn or connect bytes.

```csharp signature
[Union]
public abstract partial record SandboxFault : Expected, IValidationError<SandboxFault> {
    private SandboxFault(string detail, int code) : base(detail, code, None) { }
    public static SandboxFault Create(string message) => new Text(message);
    public sealed record Text : SandboxFault { public Text(string detail) : base(detail, FaultBand.Sandbox.Code(0)) { } }
    public sealed record LoadRejected : SandboxFault { public LoadRejected(string detail) : base(detail, FaultBand.Sandbox.Code(1)) { } }
    public sealed record NoAuthority : SandboxFault { public NoAuthority(string detail) : base(detail, FaultBand.Sandbox.Code(2)) { } }
    public sealed record QuotaExceeded : SandboxFault { public QuotaExceeded(string unit, long over) : base($"{unit}:+{over}", FaultBand.Sandbox.Code(3)) => Unit = unit; public string Unit { get; } }
    public sealed record Quarantined : SandboxFault { public Quarantined(string detail) : base(detail, FaultBand.Sandbox.Code(4)) { } }

    // Refusal composes the axis owner's own `AxisEvidence` rather than a detail string: `Runtime/profiles`
    // mints that record for exactly this crossing, so one consumer parse reads which coordinate to restate
    // whether an unservable value refused at profile admission or here, one stratum up.
    public sealed record AxisUnsupported : SandboxFault {
        public AxisUnsupported(AxisEvidence evidence) : base(evidence.Detail, FaultBand.Sandbox.Code(5)) => Evidence = evidence;

        public AxisEvidence Evidence { get; }
    }
}

public sealed record SandboxRow(
    Isolation Isolation,
    bool LinearMemory,
    bool OutOfProcess,
    DeadlineClass Wall,
    QuotaShape QuotaShape);

// Two isolation vehicles seat as SYMMETRIC slots — the wasm row's capsule beside the process row's child —
// so `Kill` and `Enforce` each dispatch on a real handle rather than reaching a handle on one row and prose
// on the other. Exactly one slot inhabits per row, decided by the SandboxRow the load resolved. Disposition
// is the eviction cell the grant handle's own dispatch gates on, so a reference held past a kill dies with
// the plugin instead of reaching the host through a closure nothing invalidated; the observed TrapCode lives
// on the wasm capsule beside the store that raised it, because the process row has no trap vocabulary and a
// cell here would fabricate one. The admitted artifact rides the instance because Quarantine.Quarantined
// holds it for review, and an eviction cannot fetch what the load already discarded.
public sealed record PluginInstance(
    string PluginId,
    PluginArtifact Artifact,
    Isolation Isolation,
    GrantScope Scope,
    QuotaCell Quota,
    Option<WasmCapsule> Capsule,
    Option<CompanionPeer> Child,
    Atom<Quarantine> Disposition,
    CancelScope Spine);

// This capsule owns the Wasmtime lifetime of one loaded plugin. Seeded is the fuel the store opened with, so the
// instruction spend is `Seeded - Store.Fuel` — a DELTA off the embedding's own counter rather than a
// host-side tally the guest could outrun. Dispose follows the embedding hierarchy (store before engine),
// and the engine is the runtime's, never this capsule's, because one engine serves every plugin. Trapped is
// the convergence slot: the frame that OBSERVES a trap lands its code here, so the kill rail reads a measured
// code rather than re-deriving one from a thread it does not own.
public sealed record WasmCapsule(Module Module, Linker Linker, Store Store, Instance Instance, ulong Seeded, Atom<Option<TrapCode>> Trapped) : IDisposable {
    public long Spent => (long)(Seeded - Store.Fuel);

    public void Dispose() {
        Store.Dispose();
        Linker.Dispose();
        Module.Dispose();
    }
}

// Eviction evidence carries the OBSERVED trap, never a `Loaded: false` boolean: that boolean cannot tell a
// converged kill from a guest that returned on its own, while the code discriminates an epoch `Interrupt`, a
// fuel `OutOfFuel`, and a guest-authored trap. Its wire projection is the stable NAME — TrapCode's numeric
// values shift as the Rust core grows rows, so a numeric on the wire re-reads as a different cause.
// Isolation rides TYPED, because a raw string forks the axis key space one projection at a time: a receipt
// spelling its own row cannot be compared against the profile that admitted the plugin, and every reader then
// re-parses prose back into a coordinate the closed vocabulary already carries.
// Both `Option<T>` slots tail the positional list carrying `= default`: the suite's `OmitAbsent` modifier
// drops an absent one at write, so a slot without a default reads back wire-required under
// `RespectRequiredConstructorParameters` and fails the decode of the payload this producer emitted.
public readonly record struct SandboxReceipt(
    string PluginId,
    Isolation Isolation,
    string ScopeHash,
    string Reason,
    Instant At,
    [property: JsonConverter(typeof(JsonStringEnumConverter<TrapCode>))] Option<TrapCode> Trap = default,
    Option<int> Residual = default) : IValidityEvidence {
    // Each vehicle carries its OWN convergence discriminant and neither is a boolean: `wasm` answers with its
    // observed TrapCode, `process` with an INDEPENDENT post-kill census count, because nothing on the process
    // handle can — `WaitForExit` returns true the instant the direct child's handle closes, 0 ms over a
    // still-live grandchild, and `ExitCode` is 137 on every SIGKILL path whether the tree converged or not, so
    // handle facts report a clean kill over a leak. Converged eviction is a census of zero, and an absent
    // census reads as an UNPROVEN kill rather than as success. Dispatch runs the WHOLE axis, so the three rows
    // no vehicle serves cannot mint a receipt at all — an `IsValid` ladder testing two keys admits them silently.
    [JsonIgnore]
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(!string.IsNullOrEmpty(PluginId) && !string.IsNullOrEmpty(Reason)),
        Isolation.Switch(
            state: this,
            inProc: static _ => ValidityClaim.Of(false),
            thread: static _ => ValidityClaim.Of(false),
            process: static receipt => ValidityClaim.Of(receipt.Residual == 0),
            wasm: static receipt => ValidityClaim.Of(receipt.Trap.IsSome),
            remote: static _ => ValidityClaim.Of(false))).Holds;
}

// Engine is ONE per host and outlives every plugin — it holds the compiled-code cache and the epoch counter
// EpochPacer advances, so a per-plugin engine would fork both. Preempting is a CONSTRUCTION INVARIANT, not a
// knob: neither preemption mechanism is settable after the engine exists, WithFuelConsumption is what makes
// Store.Fuel live, and WithEpochInterruption is what makes SetEpochDeadline enforceable — an engine composed
// without it silently renders the whole kill rail inert, because IncrementEpoch then advances a counter no
// store consults. Every engine on this page comes from this one factory for that reason. Imports projects the
// granted descriptor set into linker rows, so composition owns the marshalling shape and this page the linkage.
// ChildResidual is the process arm's convergence oracle and it is INDEPENDENT by construction — composition binds
// a process census over the child's spawn-time tag, because no member of the process handle can answer it.
public sealed record SandboxRuntime(
    SupplyChainGate.Runtime Gate,
    CommandRuntime Command,
    Engine Engine,
    Duration EpochPeriod,
    Func<PluginArtifact, GrantScope, IO<CompanionPeer>> Spawn,
    Func<GrantScope, Seq<(string Module, string Name, Function.UntypedCallbackDelegate Callback, IReadOnlyList<ValueKind> Parameters, IReadOnlyList<ValueKind> Results)>> Imports,
    Func<GrantScope, WasiConfiguration> Wasi,
    Func<CompanionPeer, CostVector> ChildSpend,
    Func<CompanionPeer, IO<int>> ChildResidual,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    CancelScope Spine,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy = default) {
    public GrantBroker Broker => Command.Broker;

    // The factory returns the ENGINE, never the Config: a caller handed a Config may compose an engine without
    // it, and the two mechanisms are unsettable afterwards, so the only shape that cannot be bypassed is the
    // one that mints the engine itself. Exemption: the Config is IDisposable and its `using` scope closes as
    // soon as the engine has consumed it — the named platform-forced statement seam.
    public static Engine Preempting(int stackBytes) {
        using var config = new Config()
            .WithFuelConsumption(true)
            .WithEpochInterruption(true)
            .WithMaximumStackSize(stackBytes);
        return new Engine(config);
    }
}

// Epoch interruption is the ONLY preemption the embedding exposes, and it is counter-driven rather than
// wall-driven: the deadline a store sets is a number of ENGINE EPOCHS, so a host that never increments the
// counter arms a deadline that never arrives. One TimeProvider timer per engine advances it at EpochPeriod,
// making a store's tick budget a real wall budget. The SAME tick sweeps the live set, because enforcement
// and the epoch share one cadence by construction: a sweep on a second timer double-charges the spend it
// folds, and a fold nothing calls leaves QuotaCell.Spent at zero so the whole breach vocabulary degenerates
// to a bare deadline read. The lease closes at the sandbox drain band.
public static class EpochPacer {
    public static IDisposable Open(SandboxRuntime runtime, Func<Seq<PluginInstance>> live) =>
        runtime.Clocks.Time.CreateTimer(
            _ => {
                runtime.Engine.IncrementEpoch();
                live().Iter(plugin => ignore(QuotaControl.Enforce(runtime, plugin, runtime.Clocks.Now)));
            },
            state: null,
            runtime.EpochPeriod.ToTimeSpan(),
            runtime.EpochPeriod.ToTimeSpan());
}

public static class SandboxRows {
    public static readonly SandboxRow Wasm = new(Isolation.Wasm, LinearMemory: true, OutOfProcess: false, DeadlineClass.HopTotal, QuotaShape.Canonical);
    public static readonly SandboxRow Process = new(Isolation.Process, LinearMemory: false, OutOfProcess: true, DeadlineClass.HopTotal, QuotaShape.Canonical);

    extension(Isolation isolation) {
        // Two of five axis values reach a vehicle, and the other three REFUSE here rather than narrowing the
        // axis to a sandbox-local pair: narrowing hides the coordinate a composition root must restate, while
        // dispatching over all five makes a sixth axis value a compile break at this seat instead of a silent
        // fall-through. Refusal is a capability this seat ADDS — every arm answers, none degrades.
        public Fin<SandboxRow> Row => isolation.Switch(
            inProc: static () => Unreachable(Isolation.InProc),
            thread: static () => Unreachable(Isolation.Thread),
            process: static () => Fin.Succ(Process),
            wasm: static () => Fin.Succ(Wasm),
            remote: static () => Unreachable(Isolation.Remote));
    }

    // Evidence names the AXIS and the value, so a refusal here reads identically to one raised at profile
    // admission and no consumer parses a sandbox-shaped detail string back into a coordinate.
    static Fin<SandboxRow> Unreachable(Isolation isolation) =>
        Fin.Fail<SandboxRow>(new SandboxFault.AxisUnsupported(new AxisEvidence(
            ProfileAxis.Isolation, isolation.Key, "sandbox vehicles are linear-memory and out-of-process alone")));

    public static IO<PluginInstance> Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime) =>
        SupplyChainGate.Admit(runtime.Gate, new AdmissionSubject.Plugin(artifact), runtime.Spine.Token).Bind(admitted => admitted.Match(
            Succ: _ => row.OutOfProcess
                ? runtime.Spawn(artifact, scope).Map(peer => Instance(row, artifact, scope, runtime, None, Some(peer)))
                : Capsule(row, artifact, scope, runtime).Map(capsule => Instance(row, artifact, scope, runtime, Some(capsule), None)),
            Fail: faults => IO.fail<PluginInstance>(faults.Head)));

    // Enter is the ONE frame that runs guest code, and it exists because a TrapException is only observable
    // where the call was made: the epoch-incrementing thread never sees it, so a crossing scattered across
    // callers would leave each of them re-classifying an embedding code and none of them recording it. Every
    // trap therefore projects through TrapDisposition — which lands the raw code on the capsule as it passes —
    // and the quota vocabulary reaches the rail while the receipt keeps the discriminant. A process row has no
    // instance to enter; its calls cross the brokered control hop instead.
    public static IO<T> Enter<T>(PluginInstance plugin, Func<Instance, T> call) =>
        plugin.Capsule.Match(
            Some: capsule =>
                from held in IO.lift(() => Try.lift(() => call(capsule.Instance)).Run())
                from result in held.Match(
                    Succ: IO.pure,
                    Fail: error => IO.fail<T>(error.Exception.Case is TrapException trap
                        ? TrapDisposition.Of(trap, plugin)
                        : new SandboxFault.LoadRejected($"{plugin.PluginId}: {error.Message}")))
                select result,
            None: () => IO.fail<T>(new SandboxFault.NoAuthority($"{plugin.PluginId}: no wasm instance on the process row")));

    // Capsule materializes the wasm vehicle in the order the embedding requires: compile, open the store, seat the
    // WASI environment and the limits BEFORE instantiation (both are ignored on an already-instantiated store),
    // fold the granted import rows onto the linker, arm the epoch deadline off the row's own wall allotment,
    // seed the fuel off its CPU allotment, then instantiate. Exemption: the embedding's construct-and-configure
    // sequence is the named platform-forced statement seam — every handle it mints lands on the returned capsule
    // and the capture rail carries the trap.
    static IO<WasmCapsule> Capsule(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime) =>
        IO.lift(() => Try.lift(() => {
            var module = Module.FromBytes(runtime.Engine, artifact.PluginId, artifact.Component.Span);
            var store = new Store(runtime.Engine);
            store.SetWasiConfiguration(runtime.Wasi(scope));
            store.SetLimits(memorySize: row.QuotaShape.MaxMemoryBytes);
            store.SetEpochDeadline(Ticks(row.QuotaShape.Wall.Allotted, runtime.EpochPeriod));
            store.Fuel = (ulong)row.QuotaShape.MaxCpuMillis * FuelPerMilli;
            var linker = new Linker(runtime.Engine);
            linker.DefineWasi();
            foreach (var (importModule, name, callback, parameters, results) in runtime.Imports(scope)) {
                linker.DefineFunction(importModule, name, callback, parameters, results);
            }
            return new WasmCapsule(module, linker, store, linker.Instantiate(store, module), store.Fuel, Atom(Option<TrapCode>.None));
        }).Run().MapFail(error => new SandboxFault.LoadRejected($"{artifact.PluginId}: {error.Message}")))
        .Bind(static minted => minted.Match(Succ: IO.pure, Fail: IO.fail<WasmCapsule>));

    // Ticks expresses the wall allotment in engine epochs, rounded UP so a budget shorter than one period still
    // arms one tick rather than zero — a zero deadline traps on the first instruction.
    static ulong Ticks(Duration allotted, Duration period) =>
        (ulong)long.Max(1L, (long)Math.Ceiling(allotted.TotalNanoseconds / period.TotalNanoseconds));

    // Fuel is instruction-shaped, so the CPU allotment converts once here against the embedding's own
    // per-instruction accounting rather than at each caller.
    const ulong FuelPerMilli = 1_000_000UL;

    static PluginInstance Instance(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime, Option<WasmCapsule> capsule, Option<CompanionPeer> child) =>
        new(artifact.PluginId, artifact, row.Isolation, scope, QuotaCell.Open(row.QuotaShape, runtime.Clocks.Now), capsule, child,
            Atom<Quarantine>(new Quarantine.Active()), runtime.Spine.Derive($"plugin-{artifact.PluginId}", runtime.Clocks.Time));
}
```

## [03]-[GRANT_HANDLE]

- Owner: `CallerModality` `[SmartEnum<string>]` the operator/agent/plugin caller axis under the `ComparerAccessors.StringOrdinal` accessor; `GrantHandle` the brokered capability handle a plugin reaches host functionality through; `BrokeredCall` the per-call mediation record discriminating caller modality; `GrantHandleSurface` the one grant-and-charge mediation surface.
- Cases: three caller modalities — operator (an interactive host call), agent (an in-process reasoning or MCP tool call), plugin (a sandboxed-plugin call over the grant handle) — each routing through one `Mediate` fold where modality is a discriminant on the record, never a parallel broker per caller.
- Entry: `Mediate(MediationRuntime runtime, CallerModality caller, GrantScope scope, string descriptorId, CommandArguments arguments, Func<string, CommandArguments, IO<ToolResult>> dispatch)` returns `IO<(BrokeredCall Call, ToolResult Result)>` — the one mediation fold the operator, agent, and plugin front doors share: it resolves the descriptor, runs the single `Scope.Covers` policy gate, debits the one `Budget` through `GrantBroker.Admit`, and dispatches through the supplied closure exactly as a command-algebra call; `Invoke(SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle, string descriptorId, CommandArguments arguments)` returns `IO<ToolResult>` — the plugin front door that seats `CallerModality.Plugin` and the handle's scope+dispatch-closure onto `Mediate` under the quota window.
- Auto: the grant handle carries no host references — it carries the plugin's `GrantScope` and a dispatch closure bound to the command algebra, so a plugin cannot reach a host capability the scope does not name even by reflection, because the handle holds no object to reflect on; `Mediate` runs ONE `Scope.Covers` policy gate and ONE `GrantBroker.Admit` charge regardless of caller modality, so an operator, an agent, and a plugin call debit the same per-tenant `Budget` (or the `DistributedBudget` fenced store when bound) against one broker and the per-call charge is metered identically — the caller modality is a `BrokeredCall` discriminant on one evidence record, not a second admission path; a call outside the scope returns `SandboxFault.NoAuthority` and never reaches the dispatch closure, and the `RuntimePolicy` ABAC verdict (when bound) gates the same `Mediate` fold before the scope check so identity, policy, and cost meet on one mediation.
- Receipt: each mediated call mints a `CommandReceipt` through the command algebra carrying the surface keyed by caller modality (the plugin id for a plugin call), so an operator, agent, and plugin call land on the same evidence stream and the `BrokeredCall` record carries the caller modality, permitted flag, and charged vector — never a parallel plugin log or a per-caller receipt.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new caller modality is one `CallerModality` row plus one `BrokeredCall` discriminant value the `Mediate` fold reads, never a parallel broker; the brokered call rides the existing command algebra, so a new plugin capability is one `CapabilityDescriptor` row the grant scope names; zero new surface.
- Boundary: the grant handle is the only authority a plugin holds — a plugin that imports a host type directly is impossible because the wasm import table and the process control verbs are both scoped to the granted descriptors, so the handle is the sole bridge; the no-ambient-authority law is enforced by construction, not by audit — the host never passes a service provider, a configuration root, or a clock into a plugin, only the grant handle, so the plugin path carries ONLY scope + dispatch-closure and the unified `Mediate` surface preserves that invariant: `CallerModality.Plugin` seats the handle's closure, never a service provider, so merging the mediation in a way that hands a plugin a service provider is the deleted form; the operator and agent modalities carry the host-side `CommandRuntime` closure but the plugin modality carries only the handle, so one mediation fold serves three callers without leaking host references into the plugin path; a plugin requesting a capability outside its standing scope raises a `Consent.Elevated` request the operator approves, landing a wider transient scope on the handle through `GrantBroker.Open` — the one seeding entry, because a ceiling GRANTS an opening balance and an elevated scope seated without it draws against zero and refuses its first call — so a plugin's authority grows only through explicit consent, never through ambient access; the handle's dispatch crosses the wasm boundary as a serialized `CommandArguments` and crosses the process boundary as the control-hop `DispatchTool` verb, so one mediation semantic serves both isolation rows; the `RuntimePolicy` verdict resolves against the branch `ONE_IDENTITY_STORE` principal/role rows and the per-call charge debits the branch `ONE_FENCED_LEASE_STORE` `Budget`, both consumed at the seam, so the unified admission point is the one gate identity, policy, and cost meet on (`Agent/capability#GRANT_BROKER` `DistributedBudget`).

```csharp signature
// The caller axis: operator/agent/plugin are discriminants on one mediation, never parallel brokers.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CallerModality {
    public static readonly CallerModality Operator = new("operator");
    public static readonly CallerModality Agent = new("agent");
    public static readonly CallerModality Plugin = new("plugin");
}

public sealed record GrantHandle(
    string PluginId,
    GrantScope Scope,
    Func<string, CommandArguments, IO<ToolResult>> Dispatch) {
    public bool Permits(CapabilityDescriptor descriptor, Instant now) =>
        Scope.Covers(descriptor.Permission, now);
}

public readonly record struct BrokeredCall(
    CallerModality Caller,
    string Subject,
    string Descriptor,
    bool Permitted,
    CostVector Charged,
    Instant At);

public sealed record MediationRuntime(
    CommandRuntime Command,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy,
    ClockPolicy Clocks);

public static class GrantHandleSurface {
    // The one grant-and-charge fold all three callers share: policy gate, scope cover, broker charge,
    // then the supplied dispatch closure. Caller modality is a discriminant on the BrokeredCall, never a
    // parallel admission. The plugin closure carries only scope+dispatch; operator/agent carry CommandRuntime.
    public static IO<(BrokeredCall Call, ToolResult Result)> Mediate(
        MediationRuntime runtime, CallerModality caller, GrantScope scope, string descriptorId,
        CommandArguments arguments, Func<string, CommandArguments, IO<ToolResult>> dispatch) =>
        runtime.Command.Registry.Resolve(descriptorId).Match(
            Some: descriptor =>
                (from _policy in runtime.Policy.Match(Some: gate => gate(scope, descriptor, arguments), None: () => Fin.Succ(unit))
                 from _scope in scope.Covers(descriptor.Permission, runtime.Clocks.Now) ? Fin.Succ(unit) : Fin.Fail<Unit>(new SandboxFault.NoAuthority(descriptorId))
                 from charged in runtime.Command.Broker.Admit(descriptor, arguments, dryRun: false)
                 select charged).Match(
                    Succ: charged =>
                        from result in dispatch(descriptorId, arguments)
                        let call = new BrokeredCall(caller, Subject(caller, arguments), descriptorId, Permitted: true, charged, runtime.Clocks.Now)
                        select (call, result),
                    Fail: fault => IO.pure((
                        new BrokeredCall(caller, Subject(caller, arguments), descriptorId, Permitted: false, CostVector.Zero, runtime.Clocks.Now),
                        new ToolResult(descriptorId, [JsonValue.Create(fault.Message)!], IsError: true, arguments.Correlation)))),
            None: () => IO.pure((
                new BrokeredCall(caller, Subject(caller, arguments), descriptorId, Permitted: false, CostVector.Zero, runtime.Clocks.Now),
                new ToolResult(descriptorId, [JsonValue.Create(new SandboxFault.Text($"unknown:{descriptorId}").Message)!], IsError: true, arguments.Correlation))));

    // The refusal names the MEASURED breach: the cell answers which budget went and by how much, so a CPU or
    // egress breach never reports itself as a zero-overage wall breach the way a hardcoded pair did.
    public static IO<ToolResult> Invoke(SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle, string descriptorId, CommandArguments arguments) =>
        plugin.Quota.Breach(runtime.Clocks.Now).Match(
            None: () => Mediate(new MediationRuntime(runtime.Command, runtime.Policy, runtime.Clocks), CallerModality.Plugin, handle.Scope, descriptorId, arguments, handle.Dispatch).Map(static outcome => outcome.Result),
            Some: breach => IO.pure(new ToolResult(descriptorId, [JsonValue.Create(new SandboxFault.QuotaExceeded(breach.Unit, breach.Over).Message)!], IsError: true, arguments.Correlation)));

    // The dispatch closure gates on the plugin's own disposition cell, which is what makes eviction reach a
    // handle the host already gave away: Kill cancels a scope and disposes a vehicle, neither of which a
    // captured closure observes, so without this read a held handle keeps calling the host after its plugin
    // is gone. The composed McpRuntime is THREADED in — re-constructing one here would bind a fresh runtime
    // against a record this page does not own, with every column it forgot silently defaulted.
    public static GrantHandle Bind(PluginInstance plugin, McpRuntime mcp) =>
        new(plugin.PluginId, plugin.Scope, (descriptorId, arguments) =>
            plugin.Disposition.Value is Quarantine.Active or Quarantine.Released
                ? McpDispatch.Call(mcp, descriptorId, arguments)
                : IO.pure(new ToolResult(descriptorId, [JsonValue.Create(new SandboxFault.Quarantined(plugin.PluginId).Message)!], IsError: true, arguments.Correlation)));

    static string Subject(CallerModality caller, CommandArguments arguments) =>
        caller == CallerModality.Plugin ? arguments.Correlation.ToString() : arguments.Tenant.Entry;
}
```

## [04]-[QUOTA_CONTROL]

- Owner: `QuotaShape` the per-plugin resource-ceiling record; `QuotaCell` the live-metering boundary capsule; `Quarantine` `[Union]` the eviction disposition; `TrapDisposition` the trap-to-fault projection; `QuotaControl` the static enforcement surface.
- Cases: `Quarantine` = Active | Killed | Quarantined | Released — Active is the running plugin, Killed terminates immediately, Quarantined disables the grant handle and holds the artifact for inspection, Released reinstates after review.
- Entry: `Observed(SandboxRuntime runtime, PluginInstance plugin)` returns `CostVector` — the measured spend read off the isolation vehicle itself; `Enforce(SandboxRuntime runtime, PluginInstance plugin, Instant now)` returns `Quarantine` — the `EpochPacer`-driven fold that charges the measurement, decides the breach, and SEATS the verdict on the instance's disposition cell; `Release(PluginInstance plugin, Instant at)` returns `Fin<Quarantine>` — the operator review arm reinstating a quarantined plugin and refusing every other disposition; `TrapDisposition.Of(TrapException trap, PluginInstance plugin)` returns `SandboxFault` — the guest-side preemption projection that lands the raw code on the capsule as it passes; `Kill(SandboxRuntime runtime, PluginInstance plugin, string reason)` returns `IO<SandboxReceipt>` — converges the vehicle through `Engine.IncrementEpoch` past the store's armed deadline (wasm) or `Process.Kill(entireProcessTree: true)` past the cooperative drain (process), disposes it, withdraws the grant handle, and mints the receipt carrying the reason beside its arm's own convergence discriminant — the observed `TrapCode` for wasm, the independent residual census for a child.
- Auto: enforcement is TWO mechanisms with disjoint jurisdictions, and the split is what makes the wall guarantee real. `QuotaCell.Breach(now)` is a CALL-ENTRY gate: it refuses the next brokered call, and it cannot touch a guest already inside a host-free loop, so a plugin that stops calling out is invisible to it. `Store.Fuel` meters INSTRUCTIONS, not wall time, so a guest spinning cheaply runs past its deadline with fuel to spare. Epoch interruption is the only preemption the embedding exposes and therefore the only mechanism that can deliver the wall budget: the store's `SetEpochDeadline` arms it, `EpochPacer` advances the engine counter on the injected `TimeProvider`, and the guest traps with `TrapCode.Interrupt` the instant its budget elapses wherever it happens to be. `Observed` reads the wasm spend as the `Store.Fuel` DELTA off the seeded value and the process spend off the child's `ResourceQuota`-graded `UtilizationCell`, so both rows measure at their own vehicle and neither runs a parallel meter, and `Enforce` CHARGES what it observes so the cell's spend advances — a fold that only read would leave every resource arm evaluating a zero vector and the wall deadline the sole reachable verdict; the disposition cell is the one seat every eviction cause writes and the grant handle's own dispatch reads, so a killed plugin's held handle answers `SandboxFault.Quarantined` instead of reaching a host its vehicle no longer has; `TrapCode.OutOfFuel` and `TrapCode.Interrupt` project onto the SAME `QuotaExceeded` fault under their breached unit, so a CPU trap and a wall trap read as one quota vocabulary rather than two embedding codes, while the RAW code is retained on the capsule and rides the eviction receipt — the fault answers "which budget", the code answers "did the kill land", and collapsing the second into the first is what leaves an unconverged kill indistinguishable from a guest that returned. That same asymmetry rules the kill arms: epoch interruption converges a wasm guest and `Store.SetLimits` never does, because a limiter caps the resources a store may acquire and has no effect on a guest that acquires nothing while spinning — the caps and the preemption are orthogonal owners and neither substitutes for the other.
- Receipt: the eviction mints a `SandboxReceipt` and the kill rides the existing `DegradationCell` only when the plugin failure escalates a host capability — a plugin kill is process-local evidence, never a host degradation by itself.
- Packages: Wasmtime, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one quota dimension is one field on `QuotaShape` riding the `CostUnit` axis and one arm on `Observed`; one disposition is one `Quarantine` case; one trap class is one `TrapDisposition` row; zero new surface.
- Boundary: the quota cell is the only plugin-resource owner — an unbounded plugin, a best-effort timeout, and a parallel plugin watchdog are the deleted forms; a wall guarantee resting on the call-entry gate alone is likewise deleted, because that gate is unreachable from inside a spinning guest and the page then promises preemption it has no mechanism for; the quota shape's units are the same `CostUnit` rows the cost model meters, so a plugin's quota and a tenant's budget speak one resource vocabulary; the kill rail is the consequence of a quota breach, a supply-chain revocation, or an operator command — all three land on `Quarantine` and all three write the ONE disposition cell, never three eviction paths and never a verdict a caller may apply differently; quarantine holds the admitted artifact so a repeat offender's evidence survives its vehicle, `Release` is the single path back and refuses any disposition not under review, and a disposition case with no producing arm is the deleted form the four-case roster is proven against; the wall-time ceiling is a `DeadlineClass` row read by projection, never a literal here, and the epoch deadline derives from that same row so the gate and the preemption cannot disagree.

```csharp signature
public sealed record QuotaShape(
    long MaxCpuMillis,
    long MaxMemoryBytes,
    DeadlineClass Wall,
    long MaxBytesEgress) {
    public static readonly QuotaShape Canonical = new(
        MaxCpuMillis: 30_000L,
        MaxMemoryBytes: 256L << 20,
        Wall: DeadlineClass.HopTotal,
        MaxBytesEgress: 64L << 20);

    // The breach carries its UNIT and its OVERAGE, because every consumer of this verdict needs both and a
    // bare presence answer forces each to invent them: the refusal message, the eviction reason, and the
    // quota fault all read one measured pair here rather than three sites hardcoding a unit and a zero.
    public Option<(string Unit, long Over)> Breach(CostVector observed) =>
        observed.Of(CostUnit.CpuMillis) - MaxCpuMillis is var cpu and > 0L ? Some((CostUnit.CpuMillis.Key, cpu))
        : observed.Of(CostUnit.BytesEgress) - MaxBytesEgress is var egress and > 0L ? Some((CostUnit.BytesEgress.Key, egress))
        : None;
}

public sealed record QuotaCell(QuotaShape Shape, Atom<CostVector> Spent, Instant Opened, Instant Deadline) {
    public static QuotaCell Open(QuotaShape shape, Instant now) =>
        new(shape, Atom(CostVector.Zero), now, now + shape.Wall.Allotted);

    // Wall and resource breaches answer on ONE surface so the gate, the enforcement fold, and the refused
    // call cannot disagree about which budget went and by how much; the wall overage is the elapsed excess,
    // measured off the deadline the row's own DeadlineClass fixed.
    public Option<(string Unit, long Over)> Breach(Instant now) =>
        now >= Deadline
            ? Some((CostUnit.WallMillis.Key, (now - Deadline).ToInt64Milliseconds()))
            : Shape.Breach(Spent.Value);

    public CostVector Charge(CostVector cost) => Spent.Swap(spent => spent.Add(cost));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Quarantine {
    private Quarantine() { }
    public sealed record Active : Quarantine;
    public sealed record Killed(string Reason) : Quarantine;
    public sealed record Quarantined(string Reason, PluginArtifact Held) : Quarantine;
    public sealed record Released(Instant At) : Quarantine;
}

// TrapDisposition carries the guest-side half of the same vocabulary: a trap the embedding raises is already an enforcement verdict,
// so it projects onto the quota fault under the unit it breached rather than surfacing as an embedding code
// a caller would re-classify. Every other trap is a plugin defect, not a quota event. The code also LANDS on
// the capsule as it passes, because this frame is the only one that observes it — the killing thread never
// sees the TrapException, so a kill receipt reading anywhere else would report a cause it did not measure.
public static class TrapDisposition {
    public static SandboxFault Of(TrapException trap, PluginInstance plugin) => Record(trap.Type, plugin) switch {
        TrapCode.Interrupt => new SandboxFault.QuotaExceeded(CostUnit.WallMillis.Key, plugin.Quota.Shape.Wall.Allotted.ToInt64Milliseconds()),
        TrapCode.OutOfFuel => new SandboxFault.QuotaExceeded(CostUnit.CpuMillis.Key, plugin.Quota.Shape.MaxCpuMillis),
        var code => new SandboxFault.LoadRejected($"{plugin.PluginId}:{code}"),
    };

    static TrapCode Record(TrapCode code, PluginInstance plugin) =>
        plugin.Capsule.Match(
            Some: capsule => capsule.Trapped.Swap(_ => Some(code)).IfNone(code),
            None: () => code);
}

public static class QuotaControl {
    // Spend is READ from the vehicle that owns it: the wasm store's own fuel delta, the child's utilization
    // grade. A host-side tally beside either is a second truth the guest can outrun between samples.
    public static CostVector Observed(SandboxRuntime runtime, PluginInstance plugin) =>
        plugin.Capsule.Match(
            Some: static capsule => new CostVector(HashMap((CostUnit.CpuMillis, capsule.Spent / 1_000_000L))),
            None: () => plugin.Child.Match(
                Some: peer => runtime.ChildSpend(peer),
                None: static () => CostVector.Zero));

    // Enforce CHARGES then decides then SEATS: charging is what advances QuotaCell.Spent, so a fold that only
    // read would leave every resource arm of Breach evaluating a zero vector forever and the wall deadline the
    // sole reachable verdict. The verdict lands on the disposition cell rather than being returned for a caller
    // to apply, because two callers applying it differently is the split the cell exists to foreclose; a first
    // breach kills, a breach on a plugin already evicted quarantines and HOLDS the admitted artifact, which is
    // what makes a repeat offender's evidence survive for review instead of dying with its vehicle.
    public static Quarantine Enforce(SandboxRuntime runtime, PluginInstance plugin, Instant now) =>
        plugin.Quota.Charge(Observed(runtime, plugin)) is var _
            && plugin.Quota.Breach(now) is { IsSome: true, Case: (string Unit, long Over) breach }
                ? plugin.Disposition.Swap(held => held is Quarantine.Active
                    ? new Quarantine.Killed($"{breach.Unit}:+{breach.Over}")
                    : new Quarantine.Quarantined($"{breach.Unit}:+{breach.Over}", plugin.Artifact))
                : plugin.Disposition.Value;

    // Release is the operator's review arm and the ONLY path back to service: it re-seats a quarantined
    // plugin's disposition so the grant handle admits again, and it refuses an Active or Killed one because
    // neither has evidence under review — a killed plugin's vehicle is already gone and reinstating it would
    // hand a caller a handle onto a disposed store.
    public static Fin<Quarantine> Release(PluginInstance plugin, Instant at) =>
        plugin.Disposition.Value is Quarantine.Quarantined
            ? Fin.Succ(plugin.Disposition.Swap(_ => new Quarantine.Released(at)))
            : Fin.Fail<Quarantine>(new SandboxFault.Quarantined($"{plugin.PluginId}: {plugin.Disposition.Value.GetType().Name} is not under review"));

    // Kill CONVERGES the vehicle; it does not merely release a handle. A wasm guest is not preemptible by
    // disposal: Store.Dispose releases a SafeHandle, and a SafeHandle cannot release while a native call is in
    // flight on that store, so a guest inside a host-free loop never observes it and the dispose blocks behind
    // the very call it meant to end. IncrementEpoch is the convergence — the store armed its deadline at load,
    // so one increment past that deadline traps the guest wherever it stands, the observing frame lands the
    // code on the capsule, and disposal then reclaims the linear memory it was holding for process life.
    // A child process converges on its FORCED EDGE, and that edge is the whole-tree kill: a plain Kill signals
    // the one pid, so a child that spawned its own worker leaves that worker orphaned and running. The cooperative
    // ShutdownAsync ahead of it buys a grace window and NOTHING else — a hostile child ignores SIGTERM and reports
    // no progress, so the grace budget is a pure deadline and never a convergence gate. The forced edge DOES
    // converge on the darwin host: SIGKILL is unignorable and the tree kill reaches pre-spawned descendants, so
    // a hostile spin child that ignored SIGTERM and had spawned its own spinning worker censuses ZERO residuals
    // post-kill; the tree walk is a snapshot, so a child spawning mid-kill is the one escape — foreclosed by the
    // grant-scoped spawn bound. Convergence is still proved by census and only by census: the handle says the
    // direct child exited the moment its own pid died, and the exit code is the same 137 whether the tree
    // drained or leaked. Both facts ride the receipt, the census as the verdict and the tree kill as the act
    // that earns it.
    public static IO<SandboxReceipt> Kill(SandboxRuntime runtime, PluginInstance plugin, string reason) =>
        from _seated in IO.lift(() => plugin.Disposition.Swap(held => held is Quarantine.Quarantined ? held : new Quarantine.Killed(reason)))
        from _cancel in IO.lift(() => { plugin.Spine.Source.Cancel(); return unit; })
        from _capsule in plugin.Capsule.Match(
            Some: capsule => IO.lift(() => { runtime.Engine.IncrementEpoch(); capsule.Dispose(); return unit; }),
            None: () => IO.pure(unit))
        from residual in plugin.Child.Match(
            Some: peer => from _drain in IO.liftAsync(async () => { await peer.Control.ShutdownAsync(); return unit; })
                          from _forced in IO.lift(() => { peer.Child.Iter(static spawned => spawned.Child.Kill(entireProcessTree: true)); return unit; })
                          from count in runtime.ChildResidual(peer)
                          select Some(count),
            None: () => IO.pure(Option<int>.None))
        from at in IO.lift(() => runtime.Clocks.Now)
        let receipt = new SandboxReceipt(plugin.PluginId, plugin.Isolation, plugin.Scope.ScopeHash, reason, at,
            plugin.Capsule.Bind(static capsule => capsule.Trapped.Value), residual)
        from _ in runtime.Sink.Send(Correlation.Mint(), TenantContext.Current, TelemetrySource.AppHost.Key, nameof(QuotaControl), JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host))
        select receipt;
}
```

## [05]-[RESEARCH]

(none)
