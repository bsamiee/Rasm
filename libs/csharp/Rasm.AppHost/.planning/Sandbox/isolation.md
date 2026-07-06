# [APPHOST_SANDBOX_HOST]

The capability-brokered plugin sandbox for the runtime spine: a two-row isolation axis runs a plugin under a Wasmtime core-module instance (WASI-Preview-1, granted-descriptor import table) or an out-of-process child, every plugin holds zero ambient authority and reaches host capability only through a brokered grant handle, resource quotas cap CPU, memory, wall-time, and egress per plugin, a kill-or-quarantine rail evicts a misbehaving plugin, and every artifact admits through the ONE `Sandbox/admission#SUPPLY_CHAIN_GATE` `SupplyChainGate.Admit` (as `AdmissionSubject.Plugin`) before it ever loads. The page owns the isolation axis, the grant broker handle, the quota cell, and the kill-quarantine rail; it consumes `SupplyChainGate`/`AdmissionSubject`/`PluginArtifact` from `Sandbox/admission`, `CapabilityDescriptor`/`GrantBroker`/`GrantScope`, `CommandAlgebra`, `OutboundHop.CompanionSpawn`/`Discovery`, `PeerAdmission`, `CancelScope`, `DegradationCell`, and `ReceiptSinkPort` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [01]-[ISOLATION_AXIS]: WASM core-module and process isolation rows with no-ambient-authority load law.
- [02]-[GRANT_HANDLE]: Capability-brokered grant handle with per-call authority mediation.
- [03]-[QUOTA_CONTROL]: CPU/memory/wall/egress quota cell with kill and quarantine rail.

## [02]-[ISOLATION_AXIS]

- Owner: `SandboxIsolation` `[SmartEnum<string>]` the two-row isolation topology under the `ComparerAccessors.StringOrdinal` accessor; `SandboxRow` per-isolation policy record; `SandboxRows` the frozen row set with the total dispatch; `PluginInstance` the loaded-plugin capsule; `SandboxFault` `[Union]` fault family deriving its codes through `FaultBand.Sandbox`.
- Cases: wasm-module, process — wasm-module runs the plugin as a Wasmtime core-module instance with a linear-memory boundary and import-only host access, process runs the plugin as an out-of-process child reached over the local-ipc hop with OS-level isolation; `SandboxFault` = Text | LoadRejected | NoAuthority | QuotaExceeded | Quarantined.
- Entry: `SandboxRow Row` is the extension property total state-free `Switch` from case to frozen row; `Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime)` returns `IO<PluginInstance>` — the ONE `Sandbox/admission` gate admits the artifact as `AdmissionSubject.Plugin` (an all-empty artifact rejects `AttestationMissing` by construction), the row materializes the isolation boundary, and the plugin loads with exactly the brokered grant scope and no ambient authority.
- Auto: the wasm-module row hosts the plugin on the `Wasmtime` embedding as a core module: the `Linker` import table carries only the host functions the grant scope names and `Linker.DefineWasi()` mounts the WASI-Preview-1 descriptors a `WasiConfiguration` pre-open set scopes, so clocks, files, sockets, and http are granted explicitly through the import table and an ungranted host capability is simply absent — the no-ambient-authority law is a structural property of the import linkage, not a runtime check — with `Config.WithFuelConsumption`/`Store.Fuel` metering CPU and `Store.SetLimits` capping linear memory; a process row spawns the child through `OutboundHop.CompanionSpawn` and reaches it over `OutboundHop.LocalIpc`, reading the child's `PeerCredential` at accept through `PeerAdmission`, so the child holds no host handle and every host call crosses the brokered control hop; the row's `QuotaShape` column seats the quota cell at load so the limits arrive with the instance, never bolted on after.
- Receipt: `SandboxReceipt` — plugin id, isolation key, granted scope hash, load outcome, `Instant`; the load transition logs through one `SpineLog` event.
- Packages: Wasmtime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one isolation row absorbs a new sandbox topology — a new linear-memory or OS-isolation backend is one `SandboxRow`, never a parallel loader; a new fault is one `SandboxFault` case; zero new surface.
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

public sealed record PluginInstance(
    string PluginId,
    SandboxIsolation Isolation,
    GrantScope Scope,
    QuotaCell Quota,
    Option<CompanionPeer> Child,
    CancelScope Spine);

public readonly record struct SandboxReceipt(
    string PluginId,
    string Isolation,
    string ScopeHash,
    bool Loaded,
    Instant At);

public sealed record SandboxRuntime(
    SupplyChainGate.Runtime Gate,
    CommandRuntime Command,
    Func<PluginArtifact, GrantScope, IO<CompanionPeer>> Spawn,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    CancelScope Spine,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy = default) {
    public GrantBroker Broker => Command.Broker;
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
                ? runtime.Spawn(artifact, scope).Map(peer => Instance(row, artifact, scope, runtime, Some(peer)))
                : IO.pure(Instance(row, artifact, scope, runtime, None)),
            Fail: faults => IO.fail<PluginInstance>(faults.Head)));

    static PluginInstance Instance(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime, Option<CompanionPeer> child) =>
        new(artifact.PluginId, row.Isolation, scope, QuotaCell.Open(row.QuotaShape, runtime.Clocks.Now), child, runtime.Spine.Derive($"plugin-{artifact.PluginId}", runtime.Clocks.Time));
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
        caller == CallerModality.Plugin ? arguments.Correlation.ToString() : arguments.Tenant.TenantId.ToString();
}
```

## [04]-[QUOTA_CONTROL]

- Owner: `QuotaShape` the per-plugin resource-ceiling record; `QuotaCell` the live-metering boundary capsule; `Quarantine` `[Union]` the eviction disposition; `QuotaControl` the static enforcement surface.
- Cases: `Quarantine` = Active | Killed | Quarantined | Released — Active is the running plugin, Killed terminates immediately, Quarantined disables the grant handle and holds the artifact for inspection, Released reinstates after review.
- Entry: `Enforce(PluginInstance plugin, CostVector observed, Instant now)` returns `Quarantine` — the enforcement fold compares observed resource use against the quota shape and disposes the plugin; `Kill(SandboxRuntime runtime, PluginInstance plugin, string reason)` returns `IO<SandboxReceipt>` — terminates the wasm instance or the child process and withdraws the grant handle.
- Auto: a wasm-module plugin's CPU and memory are read from the `Wasmtime` `Store.Fuel` counter and the `Store.SetLimits` linear-memory bound, a process plugin's from its `ResourceQuota`-graded `UtilizationCell` over the child's resource-monitoring instruments, so quota enforcement reads the same observable-instrument and quota path the host pressure grade reads, never a parallel meter; a quota breach kills the wasm instance synchronously and signals the child process to drain then terminates it on the forced deadline, so a runaway plugin cannot exceed its wall budget; a killed plugin's grant handle dispatch returns `SandboxFault.Quarantined` so a held reference cannot reach the host after eviction.
- Receipt: the eviction mints a `SandboxReceipt` and the kill rides the existing `DegradationCell` only when the plugin failure escalates a host capability — a plugin kill is process-local evidence, never a host degradation by itself.
- Packages: Wasmtime, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one quota dimension is one field on `QuotaShape` riding the `CostUnit` axis; one disposition is one `Quarantine` case; zero new surface.
- Boundary: the quota cell is the only plugin-resource owner — an unbounded plugin, a best-effort timeout, and a parallel plugin watchdog are the deleted forms; the quota shape's units are the same `CostUnit` rows the cost model meters, so a plugin's quota and a tenant's budget speak one resource vocabulary; the kill rail is the consequence of a quota breach, a supply-chain revocation, or an operator command — all three land on `Quarantine`, never three eviction paths; quarantine holds the artifact and the last receipt for inspection so a suspected-malicious plugin's evidence survives the eviction, distinct from a clean kill that discards the instance; the wall-time ceiling is a `DeadlineClass` row read by projection, never a literal here.

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

public static class QuotaControl {
    public static Quarantine Enforce(PluginInstance plugin, CostVector observed, Instant now) =>
        plugin.Quota.Shape.Breach(plugin.Quota.Charge(observed)) is { IsSome: true, Case: string unit }
            ? new Quarantine.Killed($"{unit}-exceeded")
            : now >= plugin.Quota.Deadline
                ? new Quarantine.Killed("wall-deadline")
                : new Quarantine.Active();

    public static IO<SandboxReceipt> Kill(SandboxRuntime runtime, PluginInstance plugin, string reason) =>
        from _cancel in IO.lift(() => { plugin.Spine.Source.Cancel(); return unit; })
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

- [WASM_RUNTIME]: the `Wasmtime` embedding hosts core modules under WASI-Preview-1 — `Module.FromBytes`, `Linker.DefineWasi()` over a `WasiConfiguration` pre-open set, `Config.WithFuelConsumption`/`WithEpochInterruption`, `Store.Fuel`/`Store.SetLimits`/`Store.SetEpochDeadline`, and the store-before-engine dispose hierarchy per `api-wasmtime.md`; the scope-derived import set confirms at the integrated host. The WASI-Preview-2 Component-Model runtime is the recorded domain-gap growth line — the 44.0.0 assembly ships no Component type — re-opened when the binding lands it.
- [CHILD_SHUTDOWN]: the `CompanionPeer.Control.ShutdownAsync` graceful-then-forced child termination over the control hop and the wasm-instance synchronous teardown compile through the G7 spec-compile gate until the `Grpc.Core.Api` assay source map registers the transitive package; the kill-then-quarantine convergence against a runaway plugin is the open distinction the live host resolves.
