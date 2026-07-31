# [APPHOST_SANDBOX_HOST]

The capability-brokered plugin sandbox for the runtime spine: a two-row isolation axis runs a plugin under a Wasmtime core-module instance (WASI-Preview-1, granted-descriptor import table) or an out-of-process child, each row materializing its own owned vehicle; every plugin holds zero ambient authority and reaches host capability only through a brokered grant handle; resource quotas cap CPU, memory, wall-time, and egress per plugin over a call-entry gate and an epoch preemption that reaches a guest the gate cannot; a kill-or-quarantine rail evicts a misbehaving plugin and disposes its vehicle; and every artifact admits through the ONE `Sandbox/admission#SUPPLY_CHAIN_GATE` `SupplyChainGate.Admit` (as `AdmissionSubject.Plugin`) before it ever loads. The page owns the isolation axis, the grant broker handle, the quota cell, and the kill-quarantine rail; it consumes `SupplyChainGate`/`AdmissionSubject`/`PluginArtifact` from `Sandbox/admission`, `CapabilityDescriptor`/`GrantBroker`/`GrantScope`, `CommandAlgebra`, `OutboundHop.CompanionSpawn`/`Discovery`, `PeerAdmission`, `CancelScope`, `DegradationCell`, and `ReceiptSinkPort` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [02]-[ISOLATION_AXIS]: WASM core-module and process isolation rows, each materializing its own vehicle under the no-ambient-authority load law.
- [03]-[GRANT_HANDLE]: Capability-brokered grant handle with per-call authority mediation.
- [04]-[QUOTA_CONTROL]: CPU/memory/wall/egress quota cell over call-entry gating and epoch preemption, with kill and quarantine rail.

## [02]-[ISOLATION_AXIS]

- Owner: `SandboxIsolation` `[SmartEnum<string>]` the two-row isolation topology under the `ComparerAccessors.StringOrdinal` accessor; `SandboxRow` per-isolation policy record; `SandboxRows` the frozen row set with the total dispatch; `PluginInstance` the loaded-plugin capsule carrying one isolation vehicle per row; `WasmCapsule` the owned `Wasmtime` store/instance/module/linker lifetime; `EpochPacer` the engine-wide interruption ticker; `SandboxFault` `[Union]` fault family deriving its codes through `FaultBand.Sandbox`.
- Cases: wasm-module, process — wasm-module runs the plugin as a Wasmtime core-module instance with a linear-memory boundary and import-only host access, process runs the plugin as an out-of-process child reached over the local-ipc hop with OS-level isolation; the two vehicles are SYMMETRIC slots on `PluginInstance` (`Option<WasmCapsule> Capsule` beside `Option<CompanionPeer> Child`), so neither row is prose while the other is a handle; `SandboxFault` = Text | LoadRejected | NoAuthority | QuotaExceeded | Quarantined.
- Entry: `SandboxRow Row` is the extension property total state-free `Switch` from case to frozen row; `Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime)` returns `IO<PluginInstance>` — the ONE `Sandbox/admission` gate admits the artifact as `AdmissionSubject.Plugin` (an all-empty artifact rejects `AttestationMissing` by construction), the row materializes its isolation vehicle, and the plugin loads with exactly the brokered grant scope and no ambient authority; `EpochPacer.Open(SandboxRuntime runtime)` starts the one engine-wide `TimeProvider` ticker and returns its lease.
- Auto: the wasm-module row MATERIALIZES the embedding rather than describing it — `Module.FromBytes` compiles the admitted `PluginArtifact.Component`, one `Store` per instance takes `SetWasiConfiguration` over a `WasiConfiguration` whose `WithPreopenedDirectory(host, guest, WasiDirectoryPermissions, WasiFilePermissions)` rows are exactly the `GrantScope` filesystem grants, `Linker.DefineWasi()` mounts the WASI-Preview-1 descriptors that pre-open set scopes, and one `Linker.DefineFunction(module, name, callback, parameterKinds, resultKinds)` lands per granted `CapabilityDescriptor` — so the import table IS the grant scope and an ungranted host capability is absent from the linkage, the no-ambient-authority law being a structural property rather than a runtime check; `Store.SetLimits(memorySize: row.QuotaShape.MaxMemoryBytes)` caps linear memory, `Config.WithFuelConsumption(true)` with the seeded `Store.Fuel` meters instructions, and `Config.WithMaximumStackSize` bounds recursion depth; the host callback reaches its call frame through `Caller` alone — `Caller.GetMemory(name)` then `Memory.GetSpan(address, length)` reads the guest's serialized `CommandArguments` and writes the result back — never a `Store` captured in the closure, which outlives the frame; a process row spawns the child through `OutboundHop.CompanionSpawn` and reaches it over `OutboundHop.LocalIpc`, reading the child's `PeerCredential` at accept through `PeerAdmission`, so the child holds no host handle and every host call crosses the brokered control hop; the row's `QuotaShape` column seats the quota cell at load so the limits arrive with the instance, never bolted on after; dispose follows the embedding hierarchy — store before engine — so the capsule releases its store, instance, module, and linker while the engine outlives every plugin.
- Receipt: `SandboxReceipt` — plugin id, isolation key, granted scope hash, load outcome, `Instant`; the load transition logs through one `SpineLog` event.
- Packages: Wasmtime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one isolation row absorbs a new sandbox topology — a new linear-memory or OS-isolation backend is one `SandboxRow` with one vehicle slot, never a parallel loader; a new granted host capability is one `DefineFunction` row the scope fold already emits; a new fault is one `SandboxFault` case; zero new surface.
- Boundary: the sandbox is the only plugin-load owner — a direct `Assembly.LoadFrom`, a plugin `AppDomain`, or an in-process plugin reference is the deleted form, so a plugin never shares the host's managed heap or ambient `IServiceProvider`; the WASM runtime is `Wasmtime` (the NuGet package id; core-module + WASI-Preview-1 — the 44.0.0 embedding carries NO Component-Model surface, so the granted-descriptor import-table law, fuel metering, and linear-memory caps run on the core-module `Engine`/`Linker`/`Store` surface, and the WASI-Preview-2 Component-Model runtime is a RECORDED domain-gap growth line re-opened when the binding lands it — a vendored P/Invoke over the wasmtime C-API component surface is admissible only on a consumer-named demand with the maintenance burden recorded) — a hand-rolled WASM host is the deleted form; isolation is orthogonal to the composition density law — the host composes its own modules in-process through `CompositionSurface`, but a third-party plugin always crosses an isolation boundary, so the two load paths never merge; the wasm import table and the process control-hop verb set are both projections of the granted `CapabilityDescriptor` set, so a plugin's reachable surface is exactly its grant scope in both topologies; the process row reuses the `Discovery`/`CompanionPeer` spawn-attach mechanics verbatim and adds only the quota and grant columns, never re-declaring the spawn or connect bytes.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SandboxIsolation {
    public static readonly SandboxIsolation WasmModule = new("wasm-module");
    public static readonly SandboxIsolation Process = new("process");
}

[Union]
public abstract partial record SandboxFault : Expected, IValidationError<SandboxFault> {
    private SandboxFault(string detail, int code) : base(detail, code, None) { }
    public static SandboxFault Create(string message) => new Text(message);
    public sealed record Text : SandboxFault { public Text(string detail) : base(detail, FaultBand.Sandbox.Code(0)) { } }
    public sealed record LoadRejected : SandboxFault { public LoadRejected(string detail) : base(detail, FaultBand.Sandbox.Code(1)) { } }
    public sealed record NoAuthority : SandboxFault { public NoAuthority(string detail) : base(detail, FaultBand.Sandbox.Code(2)) { } }
    public sealed record QuotaExceeded : SandboxFault { public QuotaExceeded(string unit, long over) : base($"{unit}:+{over}", FaultBand.Sandbox.Code(3)) => Unit = unit; public string Unit { get; } }
    public sealed record Quarantined : SandboxFault { public Quarantined(string detail) : base(detail, FaultBand.Sandbox.Code(4)) { } }
}

public sealed record SandboxRow(
    SandboxIsolation Isolation,
    bool LinearMemory,
    bool OutOfProcess,
    DeadlineClass Wall,
    QuotaShape QuotaShape);

// Two isolation vehicles seat as SYMMETRIC slots — the wasm row's capsule beside the process row's child —
// so `Kill` and `Enforce` each dispatch on a real handle rather than reaching a handle on one row and prose
// on the other. Exactly one slot inhabits per row, decided by the SandboxRow the load resolved.
public sealed record PluginInstance(
    string PluginId,
    SandboxIsolation Isolation,
    GrantScope Scope,
    QuotaCell Quota,
    Option<WasmCapsule> Capsule,
    Option<CompanionPeer> Child,
    CancelScope Spine);

// This capsule owns the Wasmtime lifetime of one loaded plugin. Seeded is the fuel the store opened with, so the
// instruction spend is `Seeded - Store.Fuel` — a DELTA off the embedding's own counter rather than a
// host-side tally the guest could outrun. Dispose follows the embedding hierarchy (store before engine),
// and the engine is the runtime's, never this capsule's, because one engine serves every plugin.
public sealed record WasmCapsule(Module Module, Linker Linker, Store Store, Instance Instance, ulong Seeded) : IDisposable {
    public long Spent => (long)(Seeded - Store.Fuel);

    public void Dispose() {
        Store.Dispose();
        Linker.Dispose();
        Module.Dispose();
    }
}

public readonly record struct SandboxReceipt(
    string PluginId,
    string Isolation,
    string ScopeHash,
    bool Loaded,
    Instant At);

// Engine is ONE per host and outlives every plugin — it holds the compiled-code cache and the epoch counter
// EpochPacer advances, so a per-plugin engine would fork both. Config arms the two preemption mechanisms at
// engine construction because neither is settable afterwards: WithFuelConsumption makes Store.Fuel live and
// WithEpochInterruption makes SetEpochDeadline enforceable. Imports projects the granted descriptor set
// into linker rows, so composition owns the marshalling shape and this page owns the linkage law.
public sealed record SandboxRuntime(
    SupplyChainGate.Runtime Gate,
    CommandRuntime Command,
    Engine Engine,
    Duration EpochPeriod,
    Func<PluginArtifact, GrantScope, IO<CompanionPeer>> Spawn,
    Func<GrantScope, Seq<(string Module, string Name, Function.UntypedCallbackDelegate Callback, IReadOnlyList<ValueKind> Parameters, IReadOnlyList<ValueKind> Results)>> Imports,
    Func<GrantScope, WasiConfiguration> Wasi,
    Func<CompanionPeer, CostVector> ChildSpend,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    CancelScope Spine,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy = default) {
    public GrantBroker Broker => Command.Broker;

    public static Config Configured(int stackBytes) =>
        new Config().WithFuelConsumption(true).WithEpochInterruption(true).WithMaximumStackSize(stackBytes);
}

// Epoch interruption is the ONLY preemption the embedding exposes, and it is counter-driven rather than
// wall-driven: the deadline a store sets is a number of ENGINE EPOCHS, so a host that never increments the
// counter arms a deadline that never arrives. One TimeProvider timer per engine advances it at EpochPeriod,
// making a store's tick budget a real wall budget, and the lease closes at the sandbox drain band.
public static class EpochPacer {
    public static IDisposable Open(SandboxRuntime runtime) =>
        runtime.Clocks.Time.CreateTimer(
            static state => ((Engine)state!).IncrementEpoch(),
            runtime.Engine,
            runtime.EpochPeriod.ToTimeSpan(),
            runtime.EpochPeriod.ToTimeSpan());
}

public static class SandboxRows {
    public static readonly SandboxRow WasmModule = new(SandboxIsolation.WasmModule, LinearMemory: true, OutOfProcess: false, DeadlineClass.HopTotal, QuotaShape.Canonical);
    public static readonly SandboxRow Process = new(SandboxIsolation.Process, LinearMemory: false, OutOfProcess: true, DeadlineClass.HopTotal, QuotaShape.Canonical);

    extension(SandboxIsolation isolation) {
        public SandboxRow Row => isolation.Switch(
            wasmModule: static () => WasmModule,
            process: static () => Process);
    }

    public static IO<PluginInstance> Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime) =>
        SupplyChainGate.Admit(runtime.Gate, new AdmissionSubject.Plugin(artifact), runtime.Spine.Token).Bind(admitted => admitted.Match(
            Succ: _ => row.OutOfProcess
                ? runtime.Spawn(artifact, scope).Map(peer => Instance(row, artifact, scope, runtime, None, Some(peer)))
                : Capsule(row, artifact, scope, runtime).Map(capsule => Instance(row, artifact, scope, runtime, Some(capsule), None)),
            Fail: faults => IO.fail<PluginInstance>(faults.Head)));

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
            return new WasmCapsule(module, linker, store, linker.Instantiate(store, module), store.Fuel);
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
        new(artifact.PluginId, row.Isolation, scope, QuotaCell.Open(row.QuotaShape, runtime.Clocks.Now), capsule, child, runtime.Spine.Derive($"plugin-{artifact.PluginId}", runtime.Clocks.Time));
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
- Boundary: the grant handle is the only authority a plugin holds — a plugin that imports a host type directly is impossible because the wasm import table and the process control verbs are both scoped to the granted descriptors, so the handle is the sole bridge; the no-ambient-authority law is enforced by construction, not by audit — the host never passes a service provider, a configuration root, or a clock into a plugin, only the grant handle, so the plugin path carries ONLY scope + dispatch-closure and the unified `Mediate` surface preserves that invariant: `CallerModality.Plugin` seats the handle's closure, never a service provider, so merging the mediation in a way that hands a plugin a service provider is the deleted form; the operator and agent modalities carry the host-side `CommandRuntime` closure but the plugin modality carries only the handle, so one mediation fold serves three callers without leaking host references into the plugin path; a plugin requesting a capability outside its standing scope raises a `Consent.Elevated` request the operator approves, landing a wider transient scope on the handle, so a plugin's authority grows only through explicit consent, never through ambient access; the handle's dispatch crosses the wasm boundary as a serialized `CommandArguments` and crosses the process boundary as the control-hop `DispatchTool` verb, so one mediation semantic serves both isolation rows; the `RuntimePolicy` verdict resolves against the branch `ONE_IDENTITY_STORE` principal/role rows and the per-call charge debits the branch `ONE_FENCED_LEASE_STORE` `Budget`, both consumed at the seam, so the unified admission point is the one gate identity, policy, and cost meet on (`Agent/capability#GRANT_BROKER` `DistributedBudget`).

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

    public static IO<ToolResult> Invoke(SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle, string descriptorId, CommandArguments arguments) =>
        plugin.Quota.Within(runtime.Clocks.Now)
            ? Mediate(new MediationRuntime(runtime.Command, runtime.Policy, runtime.Clocks), CallerModality.Plugin, handle.Scope, descriptorId, arguments, handle.Dispatch).Map(static outcome => outcome.Result)
            : IO.pure(new ToolResult(descriptorId, [JsonValue.Create(new SandboxFault.QuotaExceeded("wall-millis", 0L).Message)!], IsError: true, arguments.Correlation));

    public static GrantHandle Bind(PluginInstance plugin, CommandRuntime command) =>
        new(plugin.PluginId, plugin.Scope, (descriptorId, arguments) =>
            McpDispatch.Call(new McpRuntime(command.Registry, command, command.Broker, () => DegradationLevel.Full, _ => JsonValue.Create(string.Empty)!, command.Clocks, command.Sink, command.Wire), descriptorId, arguments));

    static string Subject(CallerModality caller, CommandArguments arguments) =>
        caller == CallerModality.Plugin ? arguments.Correlation.ToString() : arguments.Tenant.Entry;
}
```

## [04]-[QUOTA_CONTROL]

- Owner: `QuotaShape` the per-plugin resource-ceiling record; `QuotaCell` the live-metering boundary capsule; `Quarantine` `[Union]` the eviction disposition; `TrapDisposition` the trap-to-fault projection; `QuotaControl` the static enforcement surface.
- Cases: `Quarantine` = Active | Killed | Quarantined | Released — Active is the running plugin, Killed terminates immediately, Quarantined disables the grant handle and holds the artifact for inspection, Released reinstates after review.
- Entry: `Observed(SandboxRuntime runtime, PluginInstance plugin)` returns `CostVector` — the measured spend read off the isolation vehicle itself; `Enforce(SandboxRuntime runtime, PluginInstance plugin, Instant now)` returns `Quarantine` — the enforcement fold reads that measurement against the quota shape and disposes the plugin; `Trapped(TrapException trap, PluginInstance plugin)` returns `SandboxFault` — the guest-side preemption projection; `Kill(SandboxRuntime runtime, PluginInstance plugin, string reason)` returns `IO<SandboxReceipt>` — disposes the wasm capsule or drains the child process and withdraws the grant handle.
- Auto: enforcement is TWO mechanisms with disjoint jurisdictions, and the split is what makes the wall guarantee real. `QuotaCell.Within(now)` is a CALL-ENTRY gate: it refuses the next brokered call, and it cannot touch a guest already inside a host-free loop, so a plugin that stops calling out is invisible to it. `Store.Fuel` meters INSTRUCTIONS, not wall time, so a guest spinning cheaply runs past its deadline with fuel to spare. Epoch interruption is the only preemption the embedding exposes and therefore the only mechanism that can deliver the wall budget: the store's `SetEpochDeadline` arms it, `EpochPacer` advances the engine counter on the injected `TimeProvider`, and the guest traps with `TrapCode.Interrupt` the instant its budget elapses wherever it happens to be. `Observed` reads the wasm spend as the `Store.Fuel` DELTA off the seeded value and the process spend off the child's `ResourceQuota`-graded `UtilizationCell`, so both rows measure at their own vehicle and neither runs a parallel meter; a killed plugin's grant handle dispatch returns `SandboxFault.Quarantined` so a held reference cannot reach the host after eviction; `TrapCode.OutOfFuel` and `TrapCode.Interrupt` project onto the SAME `QuotaExceeded` fault under their breached unit, so a CPU trap and a wall trap read as one quota vocabulary rather than two embedding codes.
- Receipt: the eviction mints a `SandboxReceipt` and the kill rides the existing `DegradationCell` only when the plugin failure escalates a host capability — a plugin kill is process-local evidence, never a host degradation by itself.
- Packages: Wasmtime, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one quota dimension is one field on `QuotaShape` riding the `CostUnit` axis and one arm on `Observed`; one disposition is one `Quarantine` case; one trap class is one `TrapDisposition` row; zero new surface.
- Boundary: the quota cell is the only plugin-resource owner — an unbounded plugin, a best-effort timeout, and a parallel plugin watchdog are the deleted forms; a wall guarantee resting on the call-entry gate alone is likewise deleted, because that gate is unreachable from inside a spinning guest and the page then promises preemption it has no mechanism for; the quota shape's units are the same `CostUnit` rows the cost model meters, so a plugin's quota and a tenant's budget speak one resource vocabulary; the kill rail is the consequence of a quota breach, a supply-chain revocation, or an operator command — all three land on `Quarantine`, never three eviction paths; quarantine holds the artifact and the last receipt for inspection so a suspected-malicious plugin's evidence survives the eviction, distinct from a clean kill that discards the instance; the wall-time ceiling is a `DeadlineClass` row read by projection, never a literal here, and the epoch deadline derives from that same row so the gate and the preemption cannot disagree.

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

    public Option<string> Breach(CostVector observed) =>
        observed.Of(CostUnit.CpuMillis) > MaxCpuMillis ? Some(CostUnit.CpuMillis.Key)
        : observed.Of(CostUnit.BytesEgress) > MaxBytesEgress ? Some(CostUnit.BytesEgress.Key)
        : None;
}

public sealed record QuotaCell(QuotaShape Shape, Atom<CostVector> Spent, Instant Opened, Instant Deadline) {
    public static QuotaCell Open(QuotaShape shape, Instant now) =>
        new(shape, Atom(CostVector.Zero), now, now + shape.Wall.Allotted);

    public bool Within(Instant now) => now < Deadline && Shape.Breach(Spent.Value).IsNone;

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
// a caller would re-classify. Every other trap is a plugin defect, not a quota event.
public static class TrapDisposition {
    public static SandboxFault Of(TrapException trap, PluginInstance plugin) => trap.Type switch {
        TrapCode.Interrupt => new SandboxFault.QuotaExceeded(CostUnit.WallMillis.Key, plugin.Quota.Shape.Wall.Allotted.ToInt64Milliseconds()),
        TrapCode.OutOfFuel => new SandboxFault.QuotaExceeded(CostUnit.CpuMillis.Key, plugin.Quota.Shape.MaxCpuMillis),
        var code => new SandboxFault.LoadRejected($"{plugin.PluginId}:{code}"),
    };
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

    public static Quarantine Enforce(SandboxRuntime runtime, PluginInstance plugin, Instant now) =>
        plugin.Quota.Shape.Breach(plugin.Quota.Charge(Observed(runtime, plugin))) is { IsSome: true, Case: string unit }
            ? new Quarantine.Killed($"{unit}-exceeded")
            : now >= plugin.Quota.Deadline
                ? new Quarantine.Killed("wall-deadline")
                : new Quarantine.Active();

    // Kill disposes the VEHICLE, not only the cancel scope: cancelling a scope no guest observes leaves a
    // spinning wasm instance holding its linear memory for process life, which is the leak the capsule slot
    // exists to close. Store disposal tears the instance down synchronously; the child drains cooperatively
    // and the derived scope's cancellation is its forced edge.
    public static IO<SandboxReceipt> Kill(SandboxRuntime runtime, PluginInstance plugin, string reason) =>
        from _cancel in IO.lift(() => { plugin.Spine.Source.Cancel(); return unit; })
        from _capsule in plugin.Capsule.Match(
            Some: capsule => IO.lift(() => { capsule.Dispose(); return unit; }),
            None: () => IO.pure(unit))
        from _child in plugin.Child.Match(
            Some: peer => IO.liftAsync(async () => { await peer.Control.ShutdownAsync(); return unit; }),
            None: () => IO.pure(unit))
        from at in IO.lift(() => runtime.Clocks.Now)
        let receipt = new SandboxReceipt(plugin.PluginId, plugin.Isolation.Key, plugin.Scope.ScopeHash, Loaded: false, at)
        from _ in runtime.Sink.Send(Correlation.Mint(), TenantContext.Current, TelemetrySource.AppHost.Key, nameof(QuotaControl), JsonSerializer.SerializeToElement(receipt, AppHostWireContext.Default.SandboxReceipt))
        select receipt;
}
```

## [05]-[RESEARCH]

- [WASM_RUNTIME]-[BLOCKED]: does the admitted `Wasmtime` binding expose a Component-Model surface, so the isolation axis can host a WASI-Preview-2 component beside its core module; route: `tools.assay api query --key Wasmtime --symbol Wasmtime.Component`, and `[02]-[ISOLATION_AXIS]` binds the core-module surface whole until that symbol resolves.
- [KILL_CONVERGENCE]-[OPEN]: does `QuotaControl.Kill` converge on both isolation rows against a guest that never calls out — store disposal tearing down a spinning wasm instance and the derived scope's forced edge terminating a child that ignores `ShutdownAsync`; route: run the integrated host with a host-free spin plugin per row and observe the `SandboxReceipt` `Loaded: false` beside a zero residual instance and child-process count.
