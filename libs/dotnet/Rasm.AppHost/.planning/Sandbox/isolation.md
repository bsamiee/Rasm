# [APPHOST_SANDBOX_HOST]

Capability-brokered plugin sandboxing for the runtime spine: two `Isolation` rows reach a vehicle here — `wasm` under a Wasmtime core-module instance with a WASI-Preview-1 granted-descriptor import table, `process` under an out-of-process child — and every other axis row refuses at admission with typed evidence naming the axis. Zero ambient authority is structural: a plugin reaches host capability only through a brokered grant handle, one `CostUnit`-keyed quota table caps every metered dimension, and one eviction cell drains a misbehaving plugin's vehicle and disposes it.

Settled composition: `SupplyChainGate`/`AdmissionSubject`/`PluginArtifact` from Sandbox/admission#SUPPLY_CHAIN_GATE; `Isolation`, `ProfileAxis`, and `AxisEvidence` from Runtime/profiles#PROFILE_AXIS; `CapabilityDescriptor`/`GrantBroker`/`GrantScope`/`MeterVector`/`CostUnit`/`CostModel` and `CommandRuntime`/`CommandArguments`/`CommandResult` from Agent/capability#GRANT_BROKER and #COMMAND_ALGEBRA; `McpRuntime`/`McpDispatch` from Agent/mcp; `OutboundHop.CompanionSpawn`/`Discovery` and `CompanionPeer`/`PeerAdmission` from Wire/outbound and Wire/companion; `ClockPolicy`/`DeadlineClass` from Runtime/time; `CancelScope` from Runtime/lifecycle#CANCEL_SPINE; `Transition`/`Cell`/`Fault`/`FaultBand`/`Op` from Rasm/Domain/rails. Wasmtime owns the embedding, Thinktecture the vocabularies, LanguageExt the rails; this page mints no eighth port.

## [01]-[INDEX]

- [02]-[ISOLATION_AXIS]: Sandbox seating of the two vehicle-bearing `Isolation` rows, the axis refusal covering the rest, and the no-ambient-authority load law.
- [03]-[GRANT_HANDLE]: Capability-brokered grant handle with per-call authority mediation over one verdict union.
- [04]-[QUOTA_CONTROL]: `CostUnit`-keyed quota table, epoch preemption, and the one seat-and-drain eviction rail.

## [02]-[ISOLATION_AXIS]

- Owner: `SandboxFault` `[Union]` the fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Sandbox`; `Code` derive SEALED); `Vehicle` `[Union]` the isolation vehicle one loaded plugin holds; `VehicleProvider` `[Union]` the composition-supplied per-vehicle capability set; `SandboxRow` the per-isolation policy row; `SandboxRows` the `Isolation.Items`-seeded roster carrying the admitting dispatch and the axis refusal; `PluginInstance` the loaded-plugin capsule; `WasmCapsule` the owned Wasmtime store/instance/module/linker lifetime; `ImportRow` one granted host import; `EpochPacer` the engine-wide interruption ticker; `SandboxRuntime` the held composition state. `Runtime/profiles#PROFILE_AXIS` owns the axis itself, composed whole and never re-spelled sandbox-locally.
- Cases: `Vehicle` = `WasmCase(WasmCapsule)` | `ChildCase(CompanionPeer)` — the two vehicles are ONE case each rather than two `Option` slots, so both-absent and both-present are unrepresentable and `Isolation` derives off the arm instead of riding beside it; `VehicleProvider` = `WasmCase(imports, wasi)` | `ChildCase(spawn, spend, residual)`, so a wasm-only composition seats one entry and never supplies a process delegate it cannot honour; `SandboxFault` = LoadRejected | NoAuthority | QuotaExceeded | Quarantined | AxisUnsupported | Denied, the last being the adoption carrier `SandboxFault.Of` wraps a crossing refusal in so the broker's and the policy gate's own typed cause survives whole.
- Entry: `Fin<SandboxRow> Row` is the extension property reading the `Isolation.Items`-seeded roster, admitting the two vehicle-bearing rows and refusing the other three with `SandboxFault.AxisUnsupported` carrying the `AxisEvidence` that names the `isolation` axis; `Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime, Op key)` returns `IO<PluginInstance>` — the ONE `Sandbox/admission` gate admits the artifact as `AdmissionSubject.Plugin`, the seated provider materializes the row's vehicle, and the plugin loads with exactly the brokered grant scope; `Enter<T>(PluginInstance plugin, Func<Instance, T> call)` returns `IO<T>` — the ONE crossing into a loaded guest, so the embedding's `TrapException` is observed at exactly one seat and seats its code on the capsule before any caller re-classifies it; `SandboxRuntime.Preempting(int stackBytes)` mints the one engine every row shares with both preemption mechanisms armed; `EpochPacer.Open(SandboxRuntime runtime, Func<Seq<PluginInstance>> live, Op key)` starts the one engine-wide `TimeProvider` ticker whose every tick advances the epoch and sweeps the live set through `QuotaControl.Sweep`.
- Law: the two bools the row carried (`LinearMemory`, `OutOfProcess`) were TOTAL FUNCTIONS of `Isolation` and both delete — `LinearMemory` had zero readers corpus-wide, and the one `OutOfProcess` read chose a load branch the seated `VehicleProvider` now chooses by case. Every dispatch site breaks loudly when a third backend lands as one axis value, one roster arm, and one provider case.
- Law: the roster SEEDS from `Isolation.Items` through the generated total `Switch`, so a sixth axis value lands at type initialization as a compile break at this seat rather than as a silent absence from a hand-written pair — the same provenance law `Sandbox/solver#SOLVER_KIND` applies to its `PackKind` producer index.
- Law: the `wasm` row MATERIALIZES the embedding rather than describing it. `Module.FromBytes` compiles the admitted `PluginArtifact.Component`, one `Store` per instance takes `SetWasiConfiguration` over a `WasiConfiguration` whose `WithPreopenedDirectory(host, guest, WasiDirectoryPermissions, WasiFilePermissions)` rows are exactly the `GrantScope` filesystem grants, `Linker.DefineWasi()` mounts the WASI-Preview-1 descriptors those pre-opens scope, and one `Linker.DefineFunction` lands per granted `CapabilityDescriptor` — the import table IS the grant scope, so an ungranted host capability is absent from the linkage and the no-ambient-authority law is a structural property rather than a runtime check. `Store.SetLimits(memorySize: row.Quota.MemoryBytes)` caps linear memory, `Config.WithFuelConsumption(true)` with the seeded `Store.Fuel` meters instructions, and `Config.WithMaximumStackSize` bounds recursion depth. Host callbacks reach their frame through `Caller.TryGetMemorySpan<byte>(name, address, length, out span)` — the one bounded, null-safe window that collapses the `GetMemory`-then-`GetSpan` pair — never a `Store` captured in a closure that outlives the frame.
- Law: the `process` row spawns its child through the provider's `OutboundHop.CompanionSpawn` closure and reaches it over `OutboundHop.LocalIpc`, reading the child's `PeerCredential` at accept through `PeerAdmission`, so the child holds no host handle and every host call crosses the brokered control hop; the row's `QuotaShape` seats the quota cell at load so limits arrive with the instance rather than bolted on after.
- Law: a rejected admission carries EVERY accumulated fault — `Validation` accumulated them for exactly this, and the prior `.Head` read discarded all but one, so a plugin both forged and out-of-contract reported one cause and re-admitted after a partial fix.
- Law: the capsule mint disposes its PARTIAL handles when instantiation refuses. Compile, store, and linker are minted before `Instantiate` and a throw between them left three native handles unreferenced with the load reported as a clean failure.
- Output: `Load` yields the `PluginInstance` or fails on the rail.
- Packages: Wasmtime, Thinktecture.Runtime.Extensions, Generator.Equals, LanguageExt.Core, NodaTime, Rasm (kernel `Cell`/`Transition`/`FaultBand`/`Op`), BCL inbox
- Growth: a new linear-memory or OS-isolation backend settles as one `Isolation` value at the axis owner, one roster arm here, and one `VehicleProvider` case, never a parallel loader and never a sandbox-local axis; a new granted host capability is one `ImportRow` the scope fold already emits; a new fault is one `SandboxFault` case; zero new surface.
- Boundary: seating the axis is not owning it — a sandbox-local isolation vocabulary collides its `process` row with the axis owner's and re-spells its `wasm` one, so admitting a narrower subset of a closed axis is a REFUSAL at this page's own entry and never a second roster; the sandbox is the only plugin-load owner, so a direct `Assembly.LoadFrom`, a plugin `AppDomain`, and an in-process plugin reference are the deleted forms and a plugin never shares the host's managed heap or ambient `IServiceProvider`; the WASM runtime is `Wasmtime` at core-module with WASI-Preview-1 and that is a SETTLED ceiling rather than a pending probe — the managed assembly exposes `Config.WithComponentModel(bool)`, which toggles the NATIVE engine's component support, and not one managed component type, so `Module`, `Linker`, and `Instance` are core-module only, WASI Preview 2 is unreachable from managed code, and `TrapCode.CannotEnterComponent`/`NoAsyncResult` merely surface Rust-core trap rows; the Preview-1 `WasiConfiguration` surface is therefore the axis's whole sandbox-capability vocabulary and no page waits on a wider one; a hand-rolled WASM host is the deleted form; isolation is orthogonal to the composition density law — the host composes its own modules in-process through `CompositionSurface` while a third-party plugin always crosses an isolation boundary, so the two load paths never merge; the wasm import table and the process control-hop verb set are both projections of the granted `CapabilityDescriptor` set, so a plugin's reachable surface is exactly its grant scope in both topologies; the process row reuses the `Discovery`/`CompanionPeer` spawn-attach mechanics verbatim and adds only the quota and grant columns.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using Wasmtime;

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SandboxFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Sandbox;
    private SandboxFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    public static SandboxFault Of(Error error) => error as SandboxFault ?? new Denied(error);

    [FaultCase(0)]
    public sealed partial record LoadRejected : SandboxFault { public LoadRejected(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record NoAuthority : SandboxFault { public NoAuthority(string detail) : base(detail) { } }

    [FaultCase(2)]
    public sealed partial record QuotaExceeded : SandboxFault {
        public QuotaExceeded(Breach breach) : base(breach.Detail) => Breach = breach;

        public Breach Breach { get; }
    }

    [FaultCase(3)]
    public sealed partial record Quarantined : SandboxFault { public Quarantined(string detail) : base(detail) { } }

    [FaultCase(4)]
    public sealed partial record AxisUnsupported : SandboxFault {
        public AxisUnsupported(AxisEvidence evidence) : base(evidence.Detail) => Evidence = evidence;

        public AxisEvidence Evidence { get; }
    }

    [FaultCase(5)]
    public sealed partial record Denied : SandboxFault, ICausedFault {
        public Denied(Error cause) : base(cause.Message) => Cause = cause;

        public Error Cause { get; }

        public override Retriability Retriability => Cause is Fault fault ? fault.Retriability : Retriability.Terminal;
    }
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Vehicle {
    private Vehicle() { }
    public sealed record WasmCase(WasmCapsule Capsule) : Vehicle;
    public sealed record ChildCase(CompanionPeer Peer, VehicleProvider.ChildCase Provider) : Vehicle;
    public Isolation Isolation => Switch(
        wasmCase: static _ => Isolation.Wasm,
        childCase: static _ => Isolation.Process);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VehicleProvider {
    private VehicleProvider() { }
    public sealed record WasmCase(
        Func<GrantScope, Seq<ImportRow>> Imports,
        Func<GrantScope, WasiConfiguration> Wasi) : VehicleProvider;
    public sealed record ChildCase(
        Func<PluginArtifact, GrantScope, IO<CompanionPeer>> Spawn,
        Func<CompanionPeer, MeterVector> Spend,
        Func<CompanionPeer, IO<int>> Residual) : VehicleProvider;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ImportRow(
    string Module,
    string Name,
    Function.UntypedCallbackDelegate Callback,
    Seq<ValueKind> Parameters,
    Seq<ValueKind> Results);

public sealed record SandboxRow(Isolation Isolation, QuotaShape Quota);

[Equatable]
public sealed partial record PluginInstance(
    [property: DefaultEquality] string PluginId,
    [property: IgnoreEquality] PluginArtifact Artifact,
    [property: IgnoreEquality] GrantScope Scope,
    [property: IgnoreEquality] QuotaCell Quota,
    [property: IgnoreEquality] Vehicle Vehicle,
    [property: IgnoreEquality] Atom<Quarantine> Disposition,
    [property: IgnoreEquality] CancelScope Spine) {
    public Isolation Isolation => Vehicle.Isolation;
}

public sealed record WasmCapsule(Module Module, Linker Linker, Store Store, Instance Instance, ulong Seeded, Atom<Option<TrapCode>> Trapped) : IDisposable {
    public long Spent => (long)(Seeded - Store.Fuel);

    public void Dispose() {
        Store.Dispose();
        Linker.Dispose();
        Module.Dispose();
    }
}

public sealed record SandboxRuntime(
    SupplyChainGate.Runtime Gate,
    CommandRuntime Command,
    Engine Engine,
    Duration EpochPeriod,
    HashMap<Isolation, VehicleProvider> Vehicles,
    ClockPolicy Clocks,
    CancelScope Spine,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy = default) {
    public static Engine Preempting(int stackBytes) {
        using var config = new Config()
            .WithFuelConsumption(true)
            .WithEpochInterruption(true)
            .WithMaximumStackSize(stackBytes);
        return new Engine(config);
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class EpochPacer {
    public static IDisposable Open(SandboxRuntime runtime, Func<Seq<PluginInstance>> live, Op key) =>
        runtime.Clocks.Time.CreateTimer(
            _ => {
                runtime.Engine.IncrementEpoch();
                ignore(QuotaControl.Sweep(runtime, live(), key).Run());
            },
            state: null,
            runtime.EpochPeriod.ToTimeSpan(),
            runtime.EpochPeriod.ToTimeSpan());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SandboxRows {
    static readonly FrozenDictionary<Isolation, Fin<SandboxRow>> Table =
        Isolation.Items.ToFrozenDictionary(static row => row, static row => row.Switch(
            inProc: static () => Unreachable(Isolation.InProc),
            thread: static () => Unreachable(Isolation.Thread),
            process: static () => Fin.Succ(new SandboxRow(Isolation.Process, QuotaShape.Canonical)),
            wasm: static () => Fin.Succ(new SandboxRow(Isolation.Wasm, QuotaShape.Canonical)),
            remote: static () => Unreachable(Isolation.Remote)));

    extension(Isolation isolation) {
        public Fin<SandboxRow> Row => Table[isolation];
    }

    static Fin<SandboxRow> Unreachable(Isolation isolation) =>
        Fin.Fail<SandboxRow>(new SandboxFault.AxisUnsupported(new AxisEvidence(
            ProfileAxis.Isolation, isolation.Key, "sandbox vehicles are linear-memory and out-of-process alone")));

    public static IO<PluginInstance> Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime, Op key) =>
        from admitted in SupplyChainGate.Admit(runtime.Gate, new AdmissionSubject.Plugin(artifact), runtime.Spine.Token)
        from _proven in admitted.Match(
            Succ: IO.pure,
            Fail: faults => IO.fail<SupplyChainAdmission>(faults.Map(static fault => (Error)fault).Reduce(static (all, next) => all + next)))
        from provider in runtime.Vehicles.Find(row.Isolation)
            .ToFin(new SandboxFault.AxisUnsupported(new AxisEvidence(
                ProfileAxis.Isolation, row.Isolation.Key, "no vehicle provider seated at this composition")))
            .Match(Succ: IO.pure, Fail: IO.fail<VehicleProvider>)
        from vehicle in provider.Switch(
            state: (Row: row, Artifact: artifact, Scope: scope, Runtime: runtime),
            wasmCase: static (seat, wasm) => Capsule(seat.Row, seat.Artifact, seat.Scope, seat.Runtime, wasm).Map(static capsule => (Vehicle)new Vehicle.WasmCase(capsule)),
            childCase: static (seat, child) => child.Spawn(seat.Artifact, seat.Scope).Map(peer => (Vehicle)new Vehicle.ChildCase(peer, child)))
        from opened in QuotaCell.Open(row.Quota, runtime.Clocks, key).Match(Succ: IO.pure, Fail: IO.fail<QuotaCell>)
        select new PluginInstance(
            artifact.PluginId, artifact, scope, opened, vehicle,
            Atom<Quarantine>(new Quarantine.Active()),
            runtime.Spine.Derive(Op.Of($"plugin-{artifact.PluginId}"), runtime.Clocks));

    public static IO<T> Enter<T>(PluginInstance plugin, Func<Instance, T> call) =>
        plugin.Vehicle.Switch(
            state: (Body: call, Plugin: plugin),
            wasmCase: static (seat, wasm) =>
                from held in IO.lift(() => Op.Of().Catch(
                    body: () => Fin.Succ(seat.Body(wasm.Capsule.Instance)),
                    token: seat.Plugin.Spine.Token))
                from result in held.Match(
                    Succ: IO.pure,
                    Fail: error => error.Exception.Case is TrapException trap
                        ? IO.fail<T>(TrapDisposition.Of(TrapDisposition.Seat(wasm.Capsule, trap.Type), wasm.Capsule, seat.Plugin.Quota.Shape))
                        : IO.fail<T>(error))
                select result,
            childCase: static (seat, _) => IO.fail<T>(new SandboxFault.NoAuthority($"{seat.Plugin.PluginId}: no wasm instance on the process row")));

    static IO<WasmCapsule> Capsule(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime, VehicleProvider.WasmCase wasm) =>
        IO.lift(() => Op.Of().Catch(() => {
            Module module = Module.FromBytes(runtime.Engine, artifact.PluginId, artifact.Component.Span);
            Store store = new(runtime.Engine);
            Linker linker = new(runtime.Engine);
            try {
                store.SetWasiConfiguration(wasm.Wasi(scope));
                store.SetLimits(memorySize: row.Quota.MemoryBytes);
                store.SetEpochDeadline(Ticks(row.Quota.Wall.Bound, runtime.EpochPeriod));
                store.Fuel = (ulong)row.Quota.Ceiling(CostUnit.CpuMillis) * FuelPerMilli;
                linker.DefineWasi();
                foreach (ImportRow import in wasm.Imports(scope)) {
                    linker.DefineFunction(import.Module, import.Name, import.Callback, import.Parameters, import.Results);
                }
                return Fin.Succ(new WasmCapsule(module, linker, store, linker.Instantiate(store, module), store.Fuel, Atom(Option<TrapCode>.None)));
            }
            catch {
                linker.Dispose();
                store.Dispose();
                module.Dispose();
                throw;
            }
        }, token: runtime.Spine.Token))
        .Bind(static minted => minted.Match(Succ: IO.pure, Fail: IO.fail<WasmCapsule>));

    static ulong Ticks(TimeSpan bound, Duration period) =>
        (ulong)long.Max(1L, (long)Math.Ceiling(bound.TotalNanoseconds / period.TotalNanoseconds));

    const ulong FuelPerMilli = 1_000_000UL;
}
```

## [03]-[GRANT_HANDLE]

- Owner: `CallerModality` `[SmartEnum<string>]` the operator/agent/plugin caller axis under the `ComparerAccessors.StringOrdinal` accessor; `GrantHandle` the brokered capability handle a plugin reaches host functionality through; `GrantHandleSurface` the one plugin-scope mediation surface.
- Cases: three caller modalities — operator, agent, and plugin — carried on the command intent the canonical command result settles.
- Entry: `Invoke(SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle, string descriptorId, CommandArguments arguments, Op key)` returns `IO<CommandResult>` — the plugin front door that checks the quota and standing scope, dispatches through the handle onto the command algebra, and charges the returned result onto the plugin's own quota cell; `GrantHandleSurface.Bind(PluginInstance plugin, McpRuntime mcp)` mints the handle whose dispatch gates on the plugin's disposition cell.
- Law: the brokered spend is the plugin's OWN metered spend. `Invoke` charges the admitted `MeterVector` onto the plugin's quota cell, which is what makes `CostUnit.BytesEgress` and `CostUnit.Calls` reachable ceilings on a wasm plugin at all — the vehicle meter answers instructions, and every byte a guest sends leaves through a granted import this fold already prices.
- Law: the command algebra remains the only broker debit; `Invoke` applies the plugin's standing scope before dispatch and charges its isolation quota from the returned `CommandResult.Charged`, so no wrapper re-runs admission or copies the result.
- Output: each mediated call returns the `CommandResult` the command algebra produces.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new caller modality is one `CallerModality` row with its subject column, never a parallel broker; the brokered call rides the existing command algebra, so a new plugin capability is one `CapabilityDescriptor` row the grant scope names; zero new surface.
- Boundary: the grant handle is the only authority a plugin holds — it carries the plugin's `GrantScope` and a dispatch closure bound to the command algebra, so a plugin cannot reach a host capability the scope does not name even by reflection, because the handle holds no object to reflect on; the no-ambient-authority law is enforced by construction rather than by audit — the host never passes a service provider, a configuration root, or a clock into a plugin, only the grant handle; a plugin requesting a capability outside its standing scope raises a `Consent.Elevated` request the operator approves, landing a wider transient scope on the handle through `GrantBroker.Open` — the one seeding entry, because a ceiling GRANTS an opening balance and an elevated scope seated without it draws against zero and refuses its first call — so a plugin's authority grows only through explicit consent; the handle's dispatch crosses the wasm boundary as serialized `CommandArguments`, so one mediation semantic serves both isolation rows; the `RuntimePolicy` verdict resolves against the branch `ONE_IDENTITY_STORE` principal and role rows and the per-call charge debits the branch `ONE_FENCED_LEASE_STORE` `Budget`, both consumed at the seam, so the unified admission point is the one gate identity, policy, and cost meet on (`Agent/capability#GRANT_BROKER` `DistributedBudget`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CallerModality {
    public static readonly CallerModality Operator = new("operator");
    public static readonly CallerModality Agent = new("agent");
    public static readonly CallerModality Plugin = new("plugin");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrantHandle(
    string PluginId,
    GrantScope Scope,
    Func<string, CommandArguments, IO<CommandResult>> Dispatch) {
    public bool Permits(CapabilityDescriptor descriptor, Instant now) =>
        Scope.Covers(descriptor.Permission, now);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrantHandleSurface {
    public static IO<CommandResult> Invoke(
        SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle,
        string descriptorId, CommandArguments arguments, Op key) =>
        from breach in plugin.Quota.Breach(runtime.Clocks, key).Match(Succ: IO.pure, Fail: IO.fail<Option<Breach>>)
        from result in breach.Match(
            Some: hit => Refused(runtime, descriptorId, arguments, new SandboxFault.QuotaExceeded(hit)),
            None: () => runtime.Command.Registry.Resolve(descriptorId).Match(
                None: () => handle.Dispatch(descriptorId, arguments),
                Some: descriptor => Admitted(runtime, handle, descriptor, arguments)))
        from _charged in IO.lift(() => plugin.Quota.Charge(result.Charged))
        select result;

    static IO<CommandResult> Admitted(
        SandboxRuntime runtime, GrantHandle handle, CapabilityDescriptor descriptor, CommandArguments arguments) =>
        (from _policy in runtime.Policy.Match(
             Some: gate => gate(handle.Scope, descriptor, arguments),
             None: static () => Fin.Succ(unit))
         from _scope in handle.Permits(descriptor, runtime.Clocks.Now)
             ? Fin.Succ(unit)
             : Fin.Fail<Unit>(new SandboxFault.NoAuthority(descriptor.Id))
         select unit).Match(
            Succ: _ => handle.Dispatch(descriptor.Id, arguments),
            Fail: fault => Refused(runtime, descriptor.Id, arguments, SandboxFault.Of(fault)));

    public static GrantHandle Bind(PluginInstance plugin, McpRuntime mcp) =>
        new(plugin.PluginId, plugin.Scope, (descriptorId, arguments) =>
            plugin.Disposition.Value.Switch(
                state: (Id: plugin.PluginId, Mcp: mcp, Descriptor: descriptorId, Arguments: arguments),
                active: static (bind, _) => McpDispatch.Call(bind.Mcp, bind.Descriptor, bind.Arguments),
                killed: static (bind, row) => CommandAlgebra.Refuse(bind.Mcp.Command, bind.Descriptor,
                    new CommandFault.GrantDenied(bind.Descriptor,
                        new SandboxFault.NoAuthority($"{bind.Id}: killed on {row.Cause.Detail}")), bind.Arguments),
                quarantined: static (bind, row) => CommandAlgebra.Refuse(bind.Mcp.Command, bind.Descriptor,
                    new CommandFault.GrantDenied(bind.Descriptor,
                        new SandboxFault.Quarantined($"{bind.Id}: held for review on {row.Cause.Detail}")), bind.Arguments),
                released: static (bind, _) => McpDispatch.Call(bind.Mcp, bind.Descriptor, bind.Arguments)));

    static IO<CommandResult> Refused(
        SandboxRuntime runtime, string descriptorId, CommandArguments arguments, SandboxFault fault) =>
        CommandAlgebra.Refuse(runtime.Command, descriptorId,
            new CommandFault.GrantDenied(descriptorId, fault), arguments);
}
```

## [04]-[QUOTA_CONTROL]

- Owner: `Breach` the measured overage carrier; `QuotaShape` the per-plugin ceiling table; `QuotaCell` the live-metering boundary capsule; `EvictionCause` `[Union]` the three eviction triggers; `Quarantine` `[Union]` the eviction disposition; `TrapDisposition` the trap-to-fault projection; `QuotaControl` the static enforcement surface.
- Cases: `EvictionCause` = `BreachedCase(Breach)` | `RevokedCase(SupplyChainFault)` | `CommandedCase(string Operator)` — the three triggers the eviction rail has always claimed, now representable; `Quarantine` = `Active` | `Killed(EvictionCause)` | `Quarantined(EvictionCause, PluginArtifact Held)` | `Released(Instant)`.
- Entry: `Observed(PluginInstance plugin)` returns `MeterVector` — the measured spend read off the isolation vehicle itself, each arm reading the provider its own case carries; `Enforce(SandboxRuntime runtime, PluginInstance plugin, Op key)` returns `IO<Option<Breach>>` — the metered arm charging the observation and grading it against the ceiling table; `Evict(SandboxRuntime runtime, PluginInstance plugin, EvictionCause cause, Op key)` returns `IO<Unit>` — the ONE seat-and-drain entry every trigger takes, draining only on the transition that landed the eviction; `Sweep(SandboxRuntime runtime, Seq<PluginInstance> live, Op key)` returns `IO<Unit>` — the `EpochPacer`-driven fold grading every live plugin and evicting the ones a breach caught; `Release(PluginInstance plugin, Instant at)` returns `Fin<Quarantine>` — the operator review arm reinstating a quarantined plugin and refusing every other disposition; `TrapDisposition.Seat(WasmCapsule capsule, TrapCode observed)` returns the drain-evidence code first-wins, and `TrapDisposition.Of(TrapCode code, WasmCapsule capsule, QuotaShape shape)` projects it onto the quota vocabulary.
- Law: enforcement is TWO mechanisms with disjoint jurisdictions, and the split is what makes the wall guarantee real. `QuotaCell.Breach` is a CALL-ENTRY gate: it refuses the next brokered call and cannot touch a guest already inside a host-free loop. `Store.Fuel` meters INSTRUCTIONS, not wall time, so a guest spinning cheaply runs past its deadline with fuel to spare. Epoch interruption is the only preemption the embedding exposes and therefore the only mechanism that delivers the wall budget: the store's `SetEpochDeadline` arms it, `EpochPacer` advances the engine counter on the injected `TimeProvider`, and the guest traps with `TrapCode.Interrupt` the instant its budget elapses wherever it stands.
- Law: the ceiling is a `CostUnit`-keyed TABLE, so `Breach` visits every declared dimension in the vocabulary's own ordinal order and a dimension added to a shape is graded the moment it lands. Four flat scalar columns graded two of themselves — a declared `bytes-egress` ceiling no arm tested, and a memory cap that never reached the fold at all.
- Law: linear memory is the one ceiling the EMBEDDING enforces, so it rides its own column rather than the metered table: `Store.SetLimits` refuses the guest's allocation at the source, and a host-side grade of a dimension the guest can never exceed measures nothing. Every other ceiling is host-graded because the host is what observes it.
- Law: the wall bound measures on the kernel `MonotonicTimeline`, never on `Instant` arithmetic — a semantic clock steps backwards under NTP correction and a quota reading it grants a plugin the correction as free budget; `QuotaCell` opens on a captured `MonotonicStamp` and grades elapsed against the row's own `DeadlineClass.Bound`.
- Law: `Observed` reads the wasm spend as the `Store.Fuel` DELTA off the seeded value and the process spend off the provider's own child census, so both rows measure at their own vehicle and neither runs a parallel meter; `Enforce` CHARGES what it observes, because a fold that only reads leaves every resource arm of `Breach` evaluating a zero vector forever and the wall deadline the sole reachable verdict.
- Law: the eviction verdict RIDES the transition, and `Evict` is its ONE seat. `Cell.Commit` computes the next disposition outside the cell and commits by snapshot, so the fold that landed the kill is the fold that drains the vehicle, while a fold that lost the race reads `Ceded` and drains nothing — two triggers firing at once evicted twice and disposed a store the other arm still held; a quarantine seating over an already-killed plugin drains nothing, since the vehicle the first kill disposed is already gone.
- Law: `TrapCode` seats FIRST-WINS through `Cell.Seat`, retaining the earliest observed trap until eviction consumes it; `Swap(_ => Some(code))` overwrote it on every later trap, and the `.IfNone(code)` beside it was dead because a swap answering the post-state is always `Some`; the `Ceded` verdict tells this frame an earlier code holds.
- Law: `TrapCode.OutOfFuel` and `TrapCode.Interrupt` project onto the SAME `QuotaExceeded` fault under their breached unit, so a CPU trap and a wall trap read as one quota vocabulary rather than two embedding codes.
- Packages: Wasmtime, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (kernel `Cell`/`Transition`/`MonotonicTimeline`), BCL inbox
- Growth: one quota dimension is one `CostUnit` row the ceiling table already grades and one arm on `Observed`; one eviction trigger is one `EvictionCause` case; one disposition is one `Quarantine` case; one trap class is one `TrapDisposition` row; zero new surface.
- Boundary: the quota cell is the only plugin-resource owner — an unbounded plugin, a best-effort timeout, and a parallel plugin watchdog are the deleted forms, and a wall guarantee resting on the call-entry gate alone is likewise deleted because that gate is unreachable from inside a spinning guest; the quota table's units are the same `CostUnit` rows the cost model meters, so a plugin's quota and a tenant's budget speak one resource vocabulary; the eviction rail is the consequence of a quota breach, a supply-chain revocation, or an operator command — all three mint an `EvictionCause`, all three enter through `Evict`, and all three write the ONE disposition cell, never three eviction paths and never a verdict a caller may apply differently; the process drain is proved by CENSUS and only by census — `WaitForExit` returns true the instant the direct child's handle closes, zero milliseconds over a still-live grandchild, and `ExitCode` is 137 on every SIGKILL path whether the tree drained or leaked, so handle facts report a clean kill over a leak; the forced edge is the whole-tree kill because a plain `Kill` signals one pid and leaves a spawned worker orphaned, the cooperative `ShutdownAsync` ahead of it buys a grace window and nothing else, and the tree walk being a snapshot makes a child spawning mid-kill the one escape, foreclosed by the grant-scoped spawn bound; a wasm guest is not preemptible by disposal — `Store.Dispose` releases a `SafeHandle` and a `SafeHandle` cannot release while a native call is in flight on that store, so `IncrementEpoch` past the armed deadline is the drain and disposal then reclaims the linear memory; quarantine holds the admitted artifact so a repeat offender's evidence survives its vehicle, `Release` is the single path back and refuses any disposition not under review — the operator reaches it through the `sandbox release` verb at `Runtime/modules#COMMAND_SURFACE` and nothing else does, so the reinstatement is an audited act rather than an ambient one — and the `Released` disposition it seats is exactly the arm the grant handle's disposition switch serves again, which closes the loop a disposition case with no producing arm or no reading arm leaves open; the wall ceiling is a `DeadlineClass` row read by projection and the epoch deadline derives from that same row, so the gate and the preemption cannot disagree.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Breach(CostUnit Unit, long Over) {
    public string Detail => $"{Unit.Key}:+{Over}";
}

[Equatable]
public sealed partial record QuotaShape(
    [property: UnorderedEquality] HashMap<CostUnit, long> Ceilings,
    long MemoryBytes,
    DeadlineClass Wall) {
    public static readonly QuotaShape Canonical = new(
        Ceilings: HashMap(
            (CostUnit.CpuMillis, 30_000L),
            (CostUnit.BytesEgress, 64L << 20),
            (CostUnit.Calls, 100_000L)),
        MemoryBytes: 256L << 20,
        Wall: DeadlineClass.HopTotal);

    public long Ceiling(CostUnit unit) => Ceilings.Find(unit).IfNone(long.MaxValue);

    public Option<Breach> Breach(MeterVector observed) =>
        toSeq(Ceilings.AsIterable().OrderBy(static row => row.Key.Key, StringComparer.Ordinal))
            .Map(row => new Breach(row.Key, observed.Of(row.Key) - row.Value))
            .Find(static breach => breach.Over > 0L);
}

public sealed record QuotaCell(QuotaShape Shape, Atom<MeterVector> Spent, MonotonicStamp Opened) {
    public static Fin<QuotaCell> Open(QuotaShape shape, ClockPolicy clocks, Op key) =>
        clocks.Line.Capture(key).Map(opened => new QuotaCell(shape, Atom(MeterVector.Zero), opened));

    public Fin<Option<Breach>> Breach(ClockPolicy clocks, Op key) =>
        from now in clocks.Line.Capture(key)
        from ran in clocks.Line.Elapsed(Opened, now, key)
        select ran >= Shape.Wall.Bound
            ? Some(new Breach(CostUnit.WallMillis, (long)(ran - Shape.Wall.Bound).TotalMilliseconds))
            : Shape.Breach(Spent.Value);

    public MeterVector Charge(MeterVector cost) => Spent.Swap(spent => spent.Add(cost));
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonDerivedType(typeof(BreachedCase), typeDiscriminator: "breached")]
[JsonDerivedType(typeof(RevokedCase), typeDiscriminator: "revoked")]
[JsonDerivedType(typeof(CommandedCase), typeDiscriminator: "commanded")]
public abstract partial record EvictionCause {
    private EvictionCause() { }
    public sealed record BreachedCase(Breach Breach) : EvictionCause;
    public sealed record RevokedCase(SupplyChainFault Cause) : EvictionCause;
    public sealed record CommandedCase(string Operator) : EvictionCause;
    public string Detail => Switch(
        breachedCase: static row => row.Breach.Detail,
        revokedCase: static row => row.Cause.Message,
        commandedCase: static row => row.Operator);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Quarantine {
    private Quarantine() { }
    public sealed record Active : Quarantine;
    public sealed record Killed(EvictionCause Cause) : Quarantine;
    public sealed record Quarantined(EvictionCause Cause, PluginArtifact Held) : Quarantine;
    public sealed record Released(Instant At) : Quarantine;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TrapDisposition {
    public static TrapCode Seat(WasmCapsule capsule, TrapCode observed) =>
        Cell.Seat(capsule.Trapped, () => observed) is Transition<Option<TrapCode>>.Ceded { State.Case: TrapCode held }
            ? held
            : observed;

    public static SandboxFault Of(TrapCode code, WasmCapsule capsule, QuotaShape shape) => code switch {
        TrapCode.Interrupt => new SandboxFault.QuotaExceeded(new Breach(CostUnit.WallMillis, 0L)),
        TrapCode.OutOfFuel => new SandboxFault.QuotaExceeded(new Breach(
            CostUnit.CpuMillis, long.Max(0L, (capsule.Spent / 1_000_000L) - shape.Ceiling(CostUnit.CpuMillis)))),
        var other => new SandboxFault.LoadRejected(other.ToString()),
    };
}

public static class QuotaControl {
    public static MeterVector Observed(PluginInstance plugin) =>
        plugin.Vehicle.Switch(
            wasmCase: static wasm => new MeterVector(HashMap((CostUnit.CpuMillis, wasm.Capsule.Spent / 1_000_000L))),
            childCase: static child => child.Provider.Spend(child.Peer));

    public static IO<Option<Breach>> Enforce(SandboxRuntime runtime, PluginInstance plugin, Op key) =>
        from _charged in IO.lift(() => plugin.Quota.Charge(Observed(plugin)))
        from breach in plugin.Quota.Breach(runtime.Clocks, key).Match(Succ: IO.pure, Fail: IO.fail<Option<Breach>>)
        select breach;

    public static IO<Unit> Evict(SandboxRuntime runtime, PluginInstance plugin, EvictionCause cause) =>
        Cell.Commit(plugin.Disposition, held => held is Quarantine.Active
            ? new Quarantine.Killed(cause)
            : new Quarantine.Quarantined(cause, plugin.Artifact)) is Transition<Quarantine>.Committed { State: Quarantine.Killed }
            ? Drain(runtime, plugin)
            : IO.pure(unit);

    public static IO<Unit> Sweep(SandboxRuntime runtime, Seq<PluginInstance> live, Op key) =>
        live.TraverseM(plugin =>
                from breach in Enforce(runtime, plugin, key)
                from _ in breach.Match(
                    Some: hit => Evict(runtime, plugin, new EvictionCause.BreachedCase(hit)),
                    None: static () => IO.pure(unit))
                select unit)
            .As();

    public static Fin<Quarantine> Release(PluginInstance plugin, Instant at) =>
        Cell.Commit(plugin.Disposition, held => held is Quarantine.Quarantined ? new Quarantine.Released(at) : held) switch {
            Transition<Quarantine>.Committed { State: Quarantine.Released released } => Fin.Succ<Quarantine>(released),
            var other => Fin.Fail<Quarantine>(new SandboxFault.Quarantined($"{plugin.PluginId}: {other.Current} is not under review")),
        };

    static IO<Unit> Drain(SandboxRuntime runtime, PluginInstance plugin) =>
        from _cancel in IO.lift(() => { plugin.Spine.Source.Cancel(); return unit; })
        from _ in plugin.Vehicle.Switch(
            state: runtime,
            wasmCase: static (host, wasm) => IO.lift(() => {
                host.Engine.IncrementEpoch();
                Cell.Take(wasm.Capsule.Trapped);
                wasm.Capsule.Dispose();
                return unit;
            }),
            childCase: static (_, child) =>
                from _drain in IO.liftAsync(async () => { await child.Peer.Control.ShutdownAsync(); return unit; })
                from _forced in IO.lift(() => { child.Peer.Child.Iter(static spawned => spawned.Child.Kill(entireProcessTree: true)); return unit; })
                from _residual in child.Provider.Residual(child.Peer)
                select unit)
        select unit;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
