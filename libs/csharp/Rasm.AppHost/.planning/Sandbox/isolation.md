# [APPHOST_SANDBOX_HOST]

Capability-brokered plugin sandboxing for the runtime spine: two `Isolation` rows reach a vehicle here — `wasm` under a Wasmtime core-module instance with a WASI-Preview-1 granted-descriptor import table, `process` under an out-of-process child — and every other axis row refuses at admission with typed evidence naming the axis. Zero ambient authority is structural: a plugin reaches host capability only through a brokered grant handle, one `CostUnit`-keyed quota table caps every metered dimension, and one eviction cell drains a misbehaving plugin's vehicle and disposes it.

Settled composition: `SupplyChainGate`/`AdmissionSubject`/`PluginArtifact` from Sandbox/admission#SUPPLY_CHAIN_GATE; `Isolation`, `ProfileAxis`, and `AxisEvidence` from Runtime/profiles#PROFILE_AXIS; `CapabilityDescriptor`/`GrantBroker`/`GrantScope`/`MeterVector`/`CostUnit`/`CostModel` and `CommandRuntime`/`CommandArguments`/`CommandReceipt` from Agent/capability#GRANT_BROKER and #COMMAND_ALGEBRA; `McpRuntime`/`McpDispatch`/`ToolResult` from Agent/mcp; `OutboundHop.CompanionSpawn`/`Discovery` and `CompanionPeer`/`PeerAdmission` from Wire/outbound and Wire/companion; `ClockPolicy`/`DeadlineClass` from Runtime/time; `CancelScope` from Runtime/lifecycle#CANCEL_SPINE; `ReceiptSinkPort`/`TelemetrySource`/`TenantContext` from Rasm/Domain/frame; `Transition`/`Cell`/`Fault`/`FaultBand`/`Op`/`IValidityEvidence` from Rasm/Domain/rails; `ReceiptKind` from Observability/instruments#RECEIPT_PROJECTION. Wasmtime owns the embedding, Thinktecture the vocabularies, LanguageExt the rails; this page mints no eighth port.

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
- Receipt: the load transition logs through one `SpineLog` event and mints no receipt — a load either yields the `PluginInstance` or fails on the rail, and a receipt asserting `Loaded: true` beside a returned instance carries nothing the instance does not. Eviction evidence is `[04]`'s `SandboxReceipt`.
- Packages: Wasmtime, Thinktecture.Runtime.Extensions, Generator.Equals, LanguageExt.Core, NodaTime, Rasm (kernel `Cell`/`Transition`/`FaultBand`/`Op`), BCL inbox
- Growth: a new linear-memory or OS-isolation backend settles as one `Isolation` value at the axis owner, one roster arm here, and one `VehicleProvider` case, never a parallel loader and never a sandbox-local axis; a new granted host capability is one `ImportRow` the scope fold already emits; a new fault is one `SandboxFault` case; zero new surface.
- Boundary: seating the axis is not owning it — a sandbox-local isolation vocabulary collides its `process` row with the axis owner's and re-spells its `wasm` one, so admitting a narrower subset of a closed axis is a REFUSAL at this page's own entry and never a second roster; the sandbox is the only plugin-load owner, so a direct `Assembly.LoadFrom`, a plugin `AppDomain`, and an in-process plugin reference are the deleted forms and a plugin never shares the host's managed heap or ambient `IServiceProvider`; the WASM runtime is `Wasmtime` at core-module with WASI-Preview-1 and that is a SETTLED ceiling rather than a pending probe — the managed assembly exposes `Config.WithComponentModel(bool)`, which toggles the NATIVE engine's component support, and not one managed component type, so `Module`, `Linker`, and `Instance` are core-module only, WASI Preview 2 is unreachable from managed code, and `TrapCode.CannotEnterComponent`/`NoAsyncResult` merely surface Rust-core trap rows; the Preview-1 `WasiConfiguration` surface is therefore the axis's whole sandbox-capability vocabulary and no page waits on a wider one; a hand-rolled WASM host is the deleted form; isolation is orthogonal to the composition density law — the host composes its own modules in-process through `CompositionSurface` while a third-party plugin always crosses an isolation boundary, so the two load paths never merge; the wasm import table and the process control-hop verb set are both projections of the granted `CapabilityDescriptor` set, so a plugin's reachable surface is exactly its grant scope in both topologies; the process row reuses the `Discovery`/`CompanionPeer` spawn-attach mechanics verbatim and adds only the quota and grant columns.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Wasmtime;

// --- [ERRORS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SandboxFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Sandbox;
    private SandboxFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    // FOREIGN REFUSALS ARE ADOPTED, NEVER LAUNDERED: the broker answers `GrantFault.ConsentRequired` and
    // `GrantFault.WindowClosed` and the composed policy gate answers its own family, so a sandbox fault passes
    // through untouched and anything else rides WHOLE on `Denied`. Rebuilding those as `NoAuthority(message)`
    // told an operator awaiting consent that the scope forbade the call, erasing the code, recovery, and
    // whatever retriability the broker had just reported.
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

    // Refusal composes the axis owner's own `AxisEvidence` rather than a detail string: `Runtime/profiles` mints
    // that record for exactly this crossing, so one consumer parse reads which coordinate to restate whether an
    // unservable value refused at profile admission or here, one stratum up.
    [FaultCase(4)]
    public sealed partial record AxisUnsupported : SandboxFault {
        public AxisUnsupported(AxisEvidence evidence) : base(evidence.Detail) => Evidence = evidence;

        public AxisEvidence Evidence { get; }
    }

    // Adoption holds the crossing refusal as a VALUE, so `Cause` re-classifies on the original family's own
    // case and code while `Retriability` forwards whatever that family declared — a consent request is a real
    // retry once the operator answers, a closed window is not.
    [FaultCase(5)]
    public sealed partial record Denied : SandboxFault, ICausedFault {
        public Denied(Error cause) : base(cause.Message) => Cause = cause;

        public Error Cause { get; }

        public override Retriability Retriability => Cause is Fault fault ? fault.Retriability : Retriability.Terminal;
    }
}

// --- [TYPES] --------------------------------------------------------------------------------
// Exactly one vehicle inhabits, decided by the row the load resolved, so the both-absent and both-present
// corners the two `Option` slots admitted are unrepresentable and every eviction arm dispatches on a real
// handle. `Isolation` is DERIVED here rather than stored beside the arm — the arm is its only authority and a
// stored copy is the mirror that reads correct until a third vehicle lands.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Vehicle {
    private Vehicle() { }
    public sealed record WasmCase(WasmCapsule Capsule) : Vehicle;
    // Child arms carry their own provider: spend and residual are per-child effects the composition supplies,
    // and a consumer that re-resolved them off the runtime by axis value would narrow the provider union a
    // second time at every read for a fact the arm already decided.
    public sealed record ChildCase(CompanionPeer Peer, VehicleProvider.ChildCase Provider) : Vehicle;
    public Isolation Isolation => Switch(
        wasmCase: static _ => Isolation.Wasm,
        childCase: static _ => Isolation.Process);
}

// Providers are the composition's half of the vehicle and split BY vehicle: a wasm-only host seats the
// wasm case alone and never supplies a spawn, spend, and residual triple it cannot honour, which the six flat
// delegate columns forced. `Spend` and `Residual` are per-call effects over a live child, which is the one
// shape the capsule law admits a `Func<>` column for.
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

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ImportRow(
    string Module,
    string Name,
    Function.UntypedCallbackDelegate Callback,
    Seq<ValueKind> Parameters,
    Seq<ValueKind> Results);

public sealed record SandboxRow(Isolation Isolation, QuotaShape Quota);

// Equality keys the plugin IDENTITY: the capsule holds a store, an atom, and a cancellation scope, and
// synthesized record equality over live handles answers by reference on some members and by value on others,
// so two reads of one plugin compare unequal the moment a disposition swaps.
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

// This capsule owns the Wasmtime lifetime of one loaded plugin. `Seeded` is the fuel the store opened with, so
// instruction spend reads `Seeded - Store.Fuel` — a DELTA off the embedding's own counter rather than a
// host-side tally the guest outruns between samples. Dispose follows the embedding hierarchy (store before engine), and
// disposal leaves the engine to the runtime, since one engine serves every plugin. `Trapped` is the drain-evidence slot: the
// frame that OBSERVES a trap seats its code here first-wins, so the kill rail reads a measured code rather
// than re-deriving one from a thread it does not own.
public sealed record WasmCapsule(Module Module, Linker Linker, Store Store, Instance Instance, ulong Seeded, Atom<Option<TrapCode>> Trapped) : IDisposable {
    public long Spent => (long)(Seeded - Store.Fuel);

    public void Dispose() {
        Store.Dispose();
        Linker.Dispose();
        Module.Dispose();
    }
}

// Engine is ONE per host and outlives every plugin — it holds the compiled-code cache and the epoch counter
// `EpochPacer` advances, so a per-plugin engine forks both. `Preempting` is a CONSTRUCTION INVARIANT, not a
// knob: neither preemption mechanism is settable after the engine exists, `WithFuelConsumption` is what makes
// `Store.Fuel` live, and `WithEpochInterruption` is what makes `SetEpochDeadline` enforceable — an engine
// composed without it renders the whole kill rail inert because `IncrementEpoch` then advances a counter no
// store consults. `Vehicles` is keyed by axis value, so a composition that seats no provider for a row refuses
// that row's load with the same evidence an unservable axis value refuses with.
public sealed record SandboxRuntime(
    SupplyChainGate.Runtime Gate,
    CommandRuntime Command,
    Engine Engine,
    Duration EpochPeriod,
    HashMap<Isolation, VehicleProvider> Vehicles,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    CancelScope Spine,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy = default) {
    // This factory returns the ENGINE, never the Config: a caller handed a Config may compose an engine without
    // it, and the two mechanisms are unsettable afterwards. Exemption: the Config is IDisposable and its
    // `using` scope closes as soon as the engine has consumed it — the named platform-forced statement seam.
    public static Engine Preempting(int stackBytes) {
        using var config = new Config()
            .WithFuelConsumption(true)
            .WithEpochInterruption(true)
            .WithMaximumStackSize(stackBytes);
        return new Engine(config);
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------
// Epoch interruption is the ONLY preemption the embedding exposes, and it is counter-driven rather than
// wall-driven: the deadline a store sets is a number of ENGINE EPOCHS, so a host that never increments arms a
// deadline that never arrives. One `TimeProvider` timer per engine advances it at `EpochPeriod`, making a
// store's tick budget a real wall budget. The SAME tick sweeps the live set, because enforcement and the epoch
// share one cadence by construction: a sweep on a second timer double-charges the spend it folds, and a fold
// nothing calls leaves `QuotaCell.Spent` at zero so the whole breach vocabulary degenerates to a bare deadline
// read. The timer callback is the one synchronous `Run` seam on this page — a `TimerCallback` returns void and
// has no rail to hand the effect to — and the lease closes at the sandbox drain band.
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SandboxRows {
    // Two of five axis values reach a vehicle and the other three REFUSE here rather than narrowing the axis
    // to a sandbox-local pair: narrowing hides the coordinate a composition root must restate, while seeding
    // this table from `Isolation.Items` through the generated total `Switch` makes a sixth axis value a compile
    // break at this seat instead of a silent absence. Refusal is a capability this seat ADDS — every arm
    // answers, none degrades.
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

    // Evidence names the AXIS and the value, so a refusal here reads identically to one raised at profile
    // admission and no consumer parses a sandbox-shaped detail string back into a coordinate.
    static Fin<SandboxRow> Unreachable(Isolation isolation) =>
        Fin.Fail<SandboxRow>(new SandboxFault.AxisUnsupported(new AxisEvidence(
            ProfileAxis.Isolation, isolation.Key, "sandbox vehicles are linear-memory and out-of-process alone")));

    public static IO<PluginInstance> Load(SandboxRow row, PluginArtifact artifact, GrantScope scope, SandboxRuntime runtime, Op key) =>
        from admitted in SupplyChainGate.Admit(runtime.Gate, new AdmissionSubject.Plugin(artifact), runtime.Spine.Token)
        from _proven in admitted.Match(
            Succ: IO.pure,
            // Every accumulated cause survives: `Error` is a monoid, so a subject both forged AND out of
            // contract fails naming both rather than whichever leg the fold read first.
            Fail: faults => IO.fail<SupplyChainReceipt>(faults.Map(static fault => (Error)fault).Reduce(static (all, next) => all + next)))
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

    // `Enter` is the ONE frame that runs guest code, and it exists because a `TrapException` is only observable
    // where the call was made: the epoch-incrementing thread never sees it, so a crossing scattered across
    // callers would leave each of them re-classifying an embedding code and none of them recording it. The
    // observed code SEATS on the capsule first-wins as it passes, so the quota vocabulary reaches the rail
    // while the receipt keeps the drain discriminant. A process row has no instance to enter; its calls
    // cross the brokered control hop instead.
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

    // `Capsule` materializes the wasm vehicle in the order the embedding requires: compile, open the store,
    // seat the WASI environment and the limits BEFORE instantiation (both are ignored on an already-instantiated
    // store), fold the granted import rows onto the linker, arm the epoch deadline off the row's own wall
    // allotment, seed the fuel off its CPU ceiling, then instantiate. Partial handles RELEASE on a refused
    // instantiate — three native handles are live before `Instantiate` runs and a throw between them left every
    // one unreferenced while the load reported an ordinary failure. Exemption: the construct-and-configure
    // sequence is the named platform-forced statement seam.
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

    // `Ticks` expresses the wall bound in engine epochs, rounded UP so a budget shorter than one period still
    // arms one tick rather than zero — a zero deadline traps on the first instruction.
    static ulong Ticks(TimeSpan bound, Duration period) =>
        (ulong)long.Max(1L, (long)Math.Ceiling(bound.TotalNanoseconds / period.TotalNanoseconds));

    // Fuel is instruction-shaped, so the CPU ceiling converts once here against the embedding's own
    // per-instruction accounting rather than at each caller.
    const ulong FuelPerMilli = 1_000_000UL;
}
```

## [03]-[GRANT_HANDLE]

- Owner: `CallerModality` `[SmartEnum<string>]` the operator/agent/plugin caller axis under the `ComparerAccessors.StringOrdinal` accessor, carrying its subject projection as a `[UseDelegateFromConstructor]` column; `Mediation` `[Union]` the per-call verdict; `GrantHandle` the brokered capability handle a plugin reaches host functionality through; `BrokeredCall` the per-call mediation evidence; `MediationRuntime` the mediation dependency capsule; `GrantHandleSurface` the one grant-and-charge mediation surface.
- Cases: three caller modalities — operator (an interactive host call), agent (an in-process reasoning or MCP tool call), plugin (a sandboxed-plugin call over the grant handle) — each routing through one `Mediate` fold where modality is a discriminant on the record, never a parallel broker per caller; `Mediation` = `AdmittedCase(MeterVector Charged, ToolResult Result)` | `RefusedCase(SandboxFault Cause)`.
- Entry: `Mediate(MediationRuntime runtime, CallerModality caller, GrantScope scope, string descriptorId, CommandArguments arguments, Func<string, CommandArguments, IO<ToolResult>> dispatch)` returns `IO<BrokeredCall>` — the one mediation fold the operator, agent, and plugin front doors share: it resolves the descriptor, runs the single `Scope.Covers` policy gate, debits the one `Budget` through `GrantBroker.Admit`, and dispatches through the supplied closure exactly as a command-algebra call; `Invoke(SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle, string descriptorId, CommandArguments arguments, Op key)` returns `IO<ToolResult>` — the plugin front door that seats `CallerModality.Plugin` and the handle's scope-plus-dispatch closure onto `Mediate` under the quota window and CHARGES the admitted spend onto the plugin's own cell; `GrantHandleSurface.Bind(PluginInstance plugin, McpRuntime mcp)` mints the handle whose dispatch gates on the plugin's disposition cell.
- Law: the charged vector and the permitted flag were a two-column invariant nothing enforced — every `Permitted: false` paired with `MeterVector.Zero` by convention alone. `Mediation` makes the pairing structural: the refused arm carries the fault WHOLE, `SandboxFault.Of` adopts a broker refusal without laundering it, and the `ToolResult` projects the bounded fault observation as a JSON node, so consent remains transient and a closed window remains terminal without a consumer parsing prose.
- Law: the brokered spend is the plugin's OWN metered spend. `Invoke` charges the admitted `MeterVector` onto the plugin's quota cell, which is what makes `CostUnit.BytesEgress` and `CostUnit.Calls` reachable ceilings on a wasm plugin at all — the vehicle meter answers instructions, and every byte a guest sends leaves through a granted import this fold already prices.
- Law: `Mediate` runs ONE `Scope.Covers` policy gate and ONE `GrantBroker.Admit` charge regardless of caller modality, so an operator, an agent, and a plugin call debit the same per-tenant `Budget` (or the `DistributedBudget` fenced store when bound) against one broker and the per-call charge is metered identically; the caller modality is a `BrokeredCall` discriminant on one evidence record, not a second admission path.
- Law: the subject projection rides the modality ROW as a behaviour column — a plugin call names its correlation, an operator or agent call its tenant — so a fourth modality lands its projection with its key and no external `switch` re-derives what the row already carries.
- Receipt: each mediated call mints a `CommandReceipt` through the command algebra carrying the surface keyed by caller modality (the plugin id for a plugin call), so operator, agent, and plugin calls land on one evidence stream and the `BrokeredCall` carries the modality and the verdict — never a parallel plugin log or a per-caller receipt.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new caller modality is one `CallerModality` row with its subject column, never a parallel broker; the brokered call rides the existing command algebra, so a new plugin capability is one `CapabilityDescriptor` row the grant scope names; zero new surface.
- Boundary: the grant handle is the only authority a plugin holds — it carries the plugin's `GrantScope` and a dispatch closure bound to the command algebra, so a plugin cannot reach a host capability the scope does not name even by reflection, because the handle holds no object to reflect on; the no-ambient-authority law is enforced by construction rather than by audit — the host never passes a service provider, a configuration root, or a clock into a plugin, only the grant handle, so `CallerModality.Plugin` seats the handle's closure and merging the mediation in a way that hands a plugin a service provider is the deleted form; the operator and agent modalities carry the host-side `CommandRuntime` closure while the plugin modality carries only the handle, so one mediation fold serves three callers without leaking host references into the plugin path; a plugin requesting a capability outside its standing scope raises a `Consent.Elevated` request the operator approves, landing a wider transient scope on the handle through `GrantBroker.Open` — the one seeding entry, because a ceiling GRANTS an opening balance and an elevated scope seated without it draws against zero and refuses its first call — so a plugin's authority grows only through explicit consent; the handle's dispatch crosses the wasm boundary as serialized `CommandArguments` and the process boundary as the control-hop `DispatchTool` verb, so one mediation semantic serves both isolation rows; the `RuntimePolicy` verdict resolves against the branch `ONE_IDENTITY_STORE` principal and role rows and the per-call charge debits the branch `ONE_FENCED_LEASE_STORE` `Budget`, both consumed at the seam, so the unified admission point is the one gate identity, policy, and cost meet on (`Agent/capability#GRANT_BROKER` `DistributedBudget`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Caller axis: operator/agent/plugin are discriminants on one mediation, never parallel brokers. The
// subject projection is a row COLUMN — a plugin call is subjected by its correlation and a host call by its
// tenant, and an external switch over the roster re-derives a fact the row can carry.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CallerModality {
    public static readonly CallerModality Operator = new("operator", subject: static arguments => arguments.Tenant.Entry);
    public static readonly CallerModality Agent = new("agent", subject: static arguments => arguments.Tenant.Entry);
    public static readonly CallerModality Plugin = new("plugin", subject: static arguments => arguments.Correlation.ToString());

    [UseDelegateFromConstructor]
    public partial string Subject(CommandArguments arguments);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Mediation {
    private Mediation() { }
    public sealed record AdmittedCase(MeterVector Charged, ToolResult Result) : Mediation;
    public sealed record RefusedCase(SandboxFault Cause) : Mediation;
    public MeterVector Charged => Switch(
        admittedCase: static row => row.Charged,
        refusedCase: static _ => MeterVector.Zero);
}

// --- [MODELS] -------------------------------------------------------------------------------
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
    Mediation Outcome,
    Instant At);

public sealed record MediationRuntime(
    CommandRuntime Command,
    Option<Func<GrantScope, CapabilityDescriptor, CommandArguments, Fin<Unit>>> Policy,
    ClockPolicy Clocks);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class GrantHandleSurface {
    // One grant-and-charge fold serves all three callers: policy gate, scope cover, broker charge, then the
    // supplied dispatch closure. Caller modality is a discriminant on the `BrokeredCall`, never a parallel
    // admission. The plugin closure carries only scope and dispatch; operator and agent carry `CommandRuntime`.
    public static IO<BrokeredCall> Mediate(
        MediationRuntime runtime, CallerModality caller, GrantScope scope, string descriptorId,
        CommandArguments arguments, Func<string, CommandArguments, IO<ToolResult>> dispatch) =>
        (from descriptor in runtime.Command.Registry.Resolve(descriptorId)
             .ToFin(new SandboxFault.LoadRejected($"unknown:{descriptorId}"))
         from _policy in runtime.Policy.Match(Some: gate => gate(scope, descriptor, arguments), None: static () => Fin.Succ(unit))
         from _scope in scope.Covers(descriptor.Permission, runtime.Clocks.Now)
             ? Fin.Succ(unit)
             : Fin.Fail<Unit>(new SandboxFault.NoAuthority(descriptorId))
         from charged in runtime.Command.Broker.Admit(descriptor, arguments, DrawMode.Live)
         select charged).Match(
            Succ: charged =>
                from result in dispatch(descriptorId, arguments)
                select Call(caller, descriptorId, arguments, runtime.Clocks.Now, new Mediation.AdmittedCase(charged, result)),
            Fail: fault => IO.pure(Call(caller, descriptorId, arguments, runtime.Clocks.Now,
                new Mediation.RefusedCase(SandboxFault.Of(fault)))));

    // Refusals name the MEASURED breach: the cell answers which budget went and by how much, so a CPU or
    // egress breach never reports itself as a zero-overage wall breach. The admitted spend charges the plugin's
    // OWN cell — the brokered import is the guest's only egress, so a spend counted at the broker alone leaves
    // every non-CPU ceiling on a wasm row unreachable forever.
    public static IO<ToolResult> Invoke(
        SandboxRuntime runtime, PluginInstance plugin, GrantHandle handle,
        string descriptorId, CommandArguments arguments, Op key) =>
        from breach in plugin.Quota.Breach(runtime.Clocks, key).Match(Succ: IO.pure, Fail: IO.fail<Option<Breach>>)
        from result in breach.Match(
            Some: hit => IO.pure(Refusal(descriptorId, arguments, new SandboxFault.QuotaExceeded(hit))),
            None: () =>
                from call in Mediate(new MediationRuntime(runtime.Command, runtime.Policy, runtime.Clocks),
                    CallerModality.Plugin, handle.Scope, descriptorId, arguments, handle.Dispatch)
                from _charged in IO.lift(() => plugin.Quota.Charge(call.Outcome.Charged))
                select call.Outcome.Switch(
                    admittedCase: static admitted => admitted.Result,
                    refusedCase: refused => Refusal(descriptorId, arguments, refused.Cause)))
        select result;

    // This dispatch closure gates on the plugin's own disposition cell, which is what makes eviction reach a
    // handle the host already gave away: eviction cancels a scope and disposes a vehicle, neither of which a
    // captured closure observes, so without this read a held handle keeps calling the host after its plugin is
    // gone. The composed `McpRuntime` is THREADED in — re-constructing one here would bind a fresh runtime
    // against a record this page does not own, with every column it forgot silently defaulted.
    //
    // Dispatch gates on the union's own TOTAL switch: a type-test ladder answered `Quarantined(pluginId)` for
    // a KILLED plugin, which reads on the audit trail as an artifact held for operator review when the vehicle
    // is already disposed and nothing is held at all. Two arms serve — `Active`, and the `Released` one the
    // operator's `QuotaControl.Release` re-seats after review — and each refusing arm names its own state under
    // its own code, carrying the eviction's own `Detail` so the refusal says which trigger took the plugin down.
    public static GrantHandle Bind(PluginInstance plugin, McpRuntime mcp) =>
        new(plugin.PluginId, plugin.Scope, (descriptorId, arguments) =>
            plugin.Disposition.Value.Switch(
                state: (Id: plugin.PluginId, Mcp: mcp, Descriptor: descriptorId, Arguments: arguments),
                active: static (bind, _) => McpDispatch.Call(bind.Mcp, bind.Descriptor, bind.Arguments),
                killed: static (bind, row) => IO.pure(Refusal(bind.Descriptor, bind.Arguments,
                    new SandboxFault.NoAuthority($"{bind.Id}: killed on {row.Cause.Detail}"))),
                quarantined: static (bind, row) => IO.pure(Refusal(bind.Descriptor, bind.Arguments,
                    new SandboxFault.Quarantined($"{bind.Id}: held for review on {row.Cause.Detail}"))),
                released: static (bind, _) => McpDispatch.Call(bind.Mcp, bind.Descriptor, bind.Arguments)));

    static BrokeredCall Call(CallerModality caller, string descriptorId, CommandArguments arguments, Instant at, Mediation outcome) =>
        new(caller, caller.Subject(arguments), descriptorId, outcome, at);

    // MCP content accepts JSON nodes, so refusal carries the app's structured fault projection rather than a
    // code-and-message string a consumer would have to parse back into evidence.
    static ToolResult Refusal(string descriptorId, CommandArguments arguments, SandboxFault fault) =>
        new(descriptorId, [JsonSerializer.SerializeToNode(AppHostFaultMap.Wire(fault), SuiteContracts.Host)!],
            IsError: true, arguments.Correlation);
}
```

## [04]-[QUOTA_CONTROL]

- Owner: `Breach` the measured overage carrier; `QuotaShape` the per-plugin ceiling table; `QuotaCell` the live-metering boundary capsule; `EvictionCause` `[Union]` the three eviction triggers; `Quarantine` `[Union]` the eviction disposition; `DrainProof` `[Union]` the per-vehicle drain evidence; `SandboxReceipt` the eviction receipt; `TrapDisposition` the trap-to-fault projection; `QuotaControl` the static enforcement surface.
- Cases: `EvictionCause` = `BreachedCase(Breach)` | `RevokedCase(SupplyChainFault)` | `CommandedCase(string Operator)` — the three triggers the eviction rail has always claimed, now representable; `Quarantine` = `Active` | `Killed(EvictionCause)` | `Quarantined(EvictionCause, PluginArtifact Held)` | `Released(Instant)`; `DrainProof` = `TrappedCase(TrapCode)` | `IdleCase` | `CensusedCase(int Residual)`.
- Entry: `Observed(PluginInstance plugin)` returns `MeterVector` — the measured spend read off the isolation vehicle itself, each arm reading the provider its own case carries; `Enforce(SandboxRuntime runtime, PluginInstance plugin, Op key)` returns `IO<Option<Breach>>` — the metered arm charging the observation and grading it against the ceiling table; `Evict(SandboxRuntime runtime, PluginInstance plugin, EvictionCause cause, Op key)` returns `IO<Option<SandboxReceipt>>` — the ONE seat-and-drain entry every trigger takes, minting a receipt on the transition that landed the eviction and none on a cell another cause already held; `Sweep(SandboxRuntime runtime, Seq<PluginInstance> live, Op key)` returns `IO<Seq<SandboxReceipt>>` — the `EpochPacer`-driven fold grading every live plugin and evicting the ones a breach caught; `Release(PluginInstance plugin, Instant at)` returns `Fin<Quarantine>` — the operator review arm reinstating a quarantined plugin and refusing every other disposition; `TrapDisposition.Seat(WasmCapsule capsule, TrapCode observed)` returns the drain-evidence code first-wins, and `TrapDisposition.Of(TrapCode code, WasmCapsule capsule, QuotaShape shape)` projects it onto the quota vocabulary.
- Law: enforcement is TWO mechanisms with disjoint jurisdictions, and the split is what makes the wall guarantee real. `QuotaCell.Breach` is a CALL-ENTRY gate: it refuses the next brokered call and cannot touch a guest already inside a host-free loop. `Store.Fuel` meters INSTRUCTIONS, not wall time, so a guest spinning cheaply runs past its deadline with fuel to spare. Epoch interruption is the only preemption the embedding exposes and therefore the only mechanism that delivers the wall budget: the store's `SetEpochDeadline` arms it, `EpochPacer` advances the engine counter on the injected `TimeProvider`, and the guest traps with `TrapCode.Interrupt` the instant its budget elapses wherever it stands.
- Law: the ceiling is a `CostUnit`-keyed TABLE, so `Breach` visits every declared dimension in the vocabulary's own ordinal order and a dimension added to a shape is graded the moment it lands. Four flat scalar columns graded two of themselves — a declared `bytes-egress` ceiling no arm tested, and a memory cap that never reached the fold at all.
- Law: linear memory is the one ceiling the EMBEDDING enforces, so it rides its own column rather than the metered table: `Store.SetLimits` refuses the guest's allocation at the source, and a host-side grade of a dimension the guest can never exceed measures nothing. Every other ceiling is host-graded because the host is what observes it.
- Law: the wall bound measures on the kernel `MonotonicTimeline`, never on `Instant` arithmetic — a semantic clock steps backwards under NTP correction and a quota reading it grants a plugin the correction as free budget; `QuotaCell` opens on a captured `MonotonicStamp` and grades elapsed against the row's own `DeadlineClass.Bound`.
- Law: `Observed` reads the wasm spend as the `Store.Fuel` DELTA off the seeded value and the process spend off the provider's own child census, so both rows measure at their own vehicle and neither runs a parallel meter; `Enforce` CHARGES what it observes, because a fold that only reads leaves every resource arm of `Breach` evaluating a zero vector forever and the wall deadline the sole reachable verdict.
- Law: the eviction verdict RIDES the transition, and `Evict` is its ONE seat. `Cell.Commit` computes the next disposition outside the cell and commits by snapshot, so the fold that landed the kill is the fold that drains the vehicle and mints the receipt, while a fold that lost the race reads `Ceded` and drains nothing — two triggers firing at once evicted twice and disposed a store the other arm still held; a quarantine seating over an already-killed plugin drains nothing, since the vehicle the first kill disposed is already gone.
- Law: the drain evidence leaves its seat through `Cell.Take` at the eviction rather than being read and left seated, so the receipt carries the code once and a second read cannot republish a retired capsule's trap; a wasm guest idle when the eviction landed answers `Idle` rather than a defaulted `Interrupt` naming a preemption the engine never performed.
- Law: `TrapCode` seats FIRST-WINS through `Cell.Seat`, retaining the earliest observed trap as drain evidence the receipt reads; `Swap(_ => Some(code))` overwrote it on every later trap, and the `.IfNone(code)` beside it was dead because a swap answering the post-state is always `Some`; the `Ceded` verdict is what tells this frame an earlier code holds, so the fault it raises and the receipt the kill mints read one code.
- Law: `TrapCode.OutOfFuel` and `TrapCode.Interrupt` project onto the SAME `QuotaExceeded` fault under their breached unit, so a CPU trap and a wall trap read as one quota vocabulary rather than two embedding codes, while the RAW code rides the receipt — "which budget" is the fault's answer, "did the kill land" the code's, and collapsing the second into the first leaves an undrained kill indistinguishable from a guest that returned.
- Receipt: `SandboxReceipt` — plugin id, granted scope hash, the wire-safe `EvictionCauseWire`, the arm's own `DrainProof` evidence, and the `Instant`; revocation lowers its live supply-chain fault once through `AppHostFaultMap`, while breach and operator cases retain their typed evidence; `Isolation` DERIVES off the drain-proof arm, so the receipt states its axis row without storing a second authority for it; the receipt fans under `ReceiptKind.Eviction`, and the kill rides the existing `DegradationCell` only where a plugin failure escalates a host capability, because a plugin kill is process-local evidence rather than a host degradation by itself.
- Packages: Wasmtime, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (kernel `Cell`/`Transition`/`MonotonicTimeline`/`IValidityEvidence`), BCL inbox
- Growth: one quota dimension is one `CostUnit` row the ceiling table already grades and one arm on `Observed`; one eviction trigger is one `EvictionCause` case; one disposition is one `Quarantine` case; one trap class is one `TrapDisposition` row; zero new surface.
- Boundary: the quota cell is the only plugin-resource owner — an unbounded plugin, a best-effort timeout, and a parallel plugin watchdog are the deleted forms, and a wall guarantee resting on the call-entry gate alone is likewise deleted because that gate is unreachable from inside a spinning guest; the quota table's units are the same `CostUnit` rows the cost model meters, so a plugin's quota and a tenant's budget speak one resource vocabulary; the eviction rail is the consequence of a quota breach, a supply-chain revocation, or an operator command — all three mint an `EvictionCause`, all three enter through `Evict`, and all three write the ONE disposition cell, never three eviction paths and never a verdict a caller may apply differently; the process drain is proved by CENSUS and only by census — `WaitForExit` returns true the instant the direct child's handle closes, zero milliseconds over a still-live grandchild, and `ExitCode` is 137 on every SIGKILL path whether the tree drained or leaked, so handle facts report a clean kill over a leak; the forced edge is the whole-tree kill because a plain `Kill` signals one pid and leaves a spawned worker orphaned, the cooperative `ShutdownAsync` ahead of it buys a grace window and nothing else, and the tree walk being a snapshot makes a child spawning mid-kill the one escape, foreclosed by the grant-scoped spawn bound; a wasm guest is not preemptible by disposal — `Store.Dispose` releases a `SafeHandle` and a `SafeHandle` cannot release while a native call is in flight on that store, so `IncrementEpoch` past the armed deadline is the drain and disposal then reclaims the linear memory; quarantine holds the admitted artifact so a repeat offender's evidence survives its vehicle, `Release` is the single path back and refuses any disposition not under review — the operator reaches it through the `sandbox release` verb at `Runtime/modules#COMMAND_SURFACE` and nothing else does, so the reinstatement is an audited act rather than an ambient one — and the `Released` disposition it seats is exactly the arm the grant handle's disposition switch serves again, which is what closes the loop a disposition case with no producing arm or no reading arm would leave open; the wall ceiling is a `DeadlineClass` row read by projection and the epoch deadline derives from that same row, so the gate and the preemption cannot disagree.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// Breaches carry their typed UNIT and their overage, because every consumer of this verdict needs both: the
// refusal message, the eviction cause, and the quota fault read one measured pair rather than three sites
// hardcoding a unit key and a zero. The unit stays `CostUnit` rather than its `.Key` string so a consumer
// grades against the same vocabulary the ceiling table is keyed by.
public readonly record struct Breach(CostUnit Unit, long Over) {
    public string Detail => $"{Unit.Key}:+{Over}";
}

// Ceilings are a TABLE over the metered vocabulary so the breach fold visits every declared dimension; a flat
// column set graded whichever two someone remembered to test. `MemoryBytes` rides its own column because the
// EMBEDDING enforces it — `Store.SetLimits` refuses the guest's allocation, so a host-side grade of a value the
// guest cannot exceed is a measurement of nothing. `[UnorderedEquality]`: `HashMap` under synthesized record
// equality compares by reference, so two identical shapes read unequal at every comparison.
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

    // Ordinal over the unit key, because a `HashMap` publishes no order and a breach fold that enumerated it
    // would name a different first-offending unit per process for one measured overrun.
    public Option<Breach> Breach(MeterVector observed) =>
        toSeq(Ceilings.AsIterable().OrderBy(static row => row.Key.Key, StringComparer.Ordinal))
            .Map(row => new Breach(row.Key, observed.Of(row.Key) - row.Value))
            .Find(static breach => breach.Over > 0L);
}

// Cells open on a MONOTONIC stamp: a semantic clock steps backwards under an NTP correction and a wall
// budget graded against it hands the guest the correction as free budget. `Spent` accumulates rather than
// transitions — an accumulate has no losing contender, so the post-state is the whole answer and no verdict
// exists to ride, which is why this one swap reads its own result instead of composing a `Cell` shape.
public sealed record QuotaCell(QuotaShape Shape, Atom<MeterVector> Spent, MonotonicStamp Opened) {
    public static Fin<QuotaCell> Open(QuotaShape shape, ClockPolicy clocks, Op key) =>
        clocks.Line.Capture(key).Map(opened => new QuotaCell(shape, Atom(MeterVector.Zero), opened));

    // Wall and resource breaches answer on ONE surface so the call-entry gate, the enforcement fold, and the
    // refused call cannot disagree about which budget went and by how much; the wall overage is the elapsed
    // excess over the bound the row's own `DeadlineClass` fixed.
    public Fin<Option<Breach>> Breach(ClockPolicy clocks, Op key) =>
        from now in clocks.Line.Capture(key)
        from ran in clocks.Line.Elapsed(Opened, now, key)
        select ran >= Shape.Wall.Bound
            ? Some(new Breach(CostUnit.WallMillis, (long)(ran - Shape.Wall.Bound).TotalMilliseconds))
            : Shape.Breach(Spent.Value);

    public MeterVector Charge(MeterVector cost) => Spent.Swap(spent => spent.Add(cost));
}

// --- [TYPES] --------------------------------------------------------------------------------
// Three triggers the eviction boundary has always named, now representable: a metered breach, a revoked
// supply chain, and an operator command each carry their own evidence rather than collapsing to a reason
// string every reader re-parses.
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

// The live disposition retains the exact supply-chain fault needed by host decisions; the receipt lowers that
// error graph at its ONE serialization boundary instead of asking source generation to discover LanguageExt
// internals or minting the fault's Message as a second identity.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonDerivedType(typeof(BreachedCase), typeDiscriminator: "breached")]
[JsonDerivedType(typeof(RevokedCase), typeDiscriminator: "revoked")]
[JsonDerivedType(typeof(CommandedCase), typeDiscriminator: "commanded")]
public abstract partial record EvictionCauseWire {
    private EvictionCauseWire() { }
    public sealed record BreachedCase(Breach Breach) : EvictionCauseWire;
    public sealed record RevokedCase(FaultObservationWire Fault) : EvictionCauseWire;
    public sealed record CommandedCase(string Operator) : EvictionCauseWire;

    public static EvictionCauseWire Of(EvictionCause cause) => cause.Switch(
        breachedCase: static row => new BreachedCase(row.Breach),
        revokedCase: static row => new RevokedCase(AppHostFaultMap.Wire(row.Cause)),
        commandedCase: static row => new CommandedCase(row.Operator));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Quarantine {
    private Quarantine() { }
    public sealed record Active : Quarantine;
    public sealed record Killed(EvictionCause Cause) : Quarantine;
    public sealed record Quarantined(EvictionCause Cause, PluginArtifact Held) : Quarantine;
    public sealed record Released(Instant At) : Quarantine;
}

// Each vehicle carries its OWN drain discriminant and neither is a boolean: `wasm` answers with its
// observed `TrapCode`, `process` with an INDEPENDENT post-kill census count. Two `Option` slots admitted a
// both-absent receipt and a both-present one, and the five-arm `IsValid` ladder that re-derived the pairing was
// its witness that both states were representable. Wire projection is the arm's stable NAME — `TrapCode`
// numerics shift as the Rust core grows rows, so a numeric on the wire re-reads as a different cause.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonDerivedType(typeof(TrappedCase), typeDiscriminator: "trapped")]
[JsonDerivedType(typeof(IdleCase), typeDiscriminator: "idle")]
[JsonDerivedType(typeof(CensusedCase), typeDiscriminator: "censused")]
public abstract partial record DrainProof {
    private DrainProof() { }
    public sealed record TrappedCase([property: JsonConverter(typeof(JsonStringEnumConverter<TrapCode>))] TrapCode Code) : DrainProof;
    // Wasm guests not executing when the eviction lands drain on DISPOSAL alone, with no trap to report.
    // Folding that state into a defaulted `Interrupt` publishes a preemption the engine never performed, which
    // is precisely the forged reading the raw code exists to prevent.
    public sealed record IdleCase : DrainProof;
    public sealed record CensusedCase(int Residual) : DrainProof;
    // A drained eviction is a trap the frame measured, an idle disposal, or a census of ZERO; a positive
    // residual is a PROVEN leak, which is evidence rather than an invalid receipt.
    public bool Drained => Switch(
        trappedCase: static _ => true,
        idleCase: static _ => true,
        censusedCase: static row => row.Residual == 0);
    public Isolation Isolation => Switch(
        trappedCase: static _ => Isolation.Wasm,
        idleCase: static _ => Isolation.Wasm,
        censusedCase: static _ => Isolation.Process);
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SandboxReceipt(
    string PluginId,
    string ScopeHash,
    EvictionCauseWire Cause,
    DrainProof Proof,
    Instant At) : IValidityEvidence {
    // `Isolation` rides TYPED and DERIVED: a raw string forks the axis key space one projection at a time, and
    // a stored copy beside the drain-proof arm is a second authority for one fact.
    public Isolation Isolation => Proof.Isolation;

    [JsonIgnore]
    public bool IsValid => ValidityClaim.All(
        !string.IsNullOrEmpty(PluginId),
        !string.IsNullOrEmpty(ScopeHash)).Holds;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// `TrapDisposition` carries the guest-side half of the quota vocabulary: a trap the embedding raises is already
// an enforcement verdict, so it projects onto the quota fault under the unit it breached rather than surfacing
// as an embedding code a caller re-classifies. Every other trap is a plugin defect, not a quota event.
public static class TrapDisposition {
    // First-wins through the kernel seat: the earliest observed trap IS the drain evidence the receipt
    // reads, so a later benign trap CEDES and both this frame's fault and the eviction receipt name one code.
    // `Ceded` alone can report the held code — a swap answering the post-state
    // reports success to every contender, which is exactly how the first trap was lost.
    public static TrapCode Seat(WasmCapsule capsule, TrapCode observed) =>
        Cell.Seat(capsule.Trapped, () => observed) is Transition<Option<TrapCode>>.Ceded { State.Case: TrapCode held }
            ? held
            : observed;

    // Overage on the interrupt arm is STRUCTURALLY zero: epoch preemption cuts the guest AT the armed
    // deadline, so the wall excess is bounded by one epoch period and the ceiling is the whole measurement.
    // Fuel arms measure a real excess — the seeded delta against the row's own CPU ceiling.
    public static SandboxFault Of(TrapCode code, WasmCapsule capsule, QuotaShape shape) => code switch {
        TrapCode.Interrupt => new SandboxFault.QuotaExceeded(new Breach(CostUnit.WallMillis, 0L)),
        TrapCode.OutOfFuel => new SandboxFault.QuotaExceeded(new Breach(
            CostUnit.CpuMillis, long.Max(0L, (capsule.Spent / 1_000_000L) - shape.Ceiling(CostUnit.CpuMillis)))),
        var other => new SandboxFault.LoadRejected(other.ToString()),
    };
}

public static class QuotaControl {
    // Spend is READ from the vehicle that owns it: the wasm store's own fuel delta, the child's provider-bound
    // census. A host-side tally beside either is a second truth the guest can outrun between samples. Brokered
    // spend — every byte and every call a guest sends through a granted import — charges at `Invoke`, so the
    // two sources meet on one cell and no unit is graded against a meter that never populates it.
    public static MeterVector Observed(PluginInstance plugin) =>
        plugin.Vehicle.Switch(
            wasmCase: static wasm => new MeterVector(HashMap((CostUnit.CpuMillis, wasm.Capsule.Spent / 1_000_000L))),
            childCase: static child => child.Provider.Spend(child.Peer));

    // `Enforce` CHARGES then GRADES and seats nothing: charging is what advances `QuotaCell.Spent`, so a fold
    // that only read would leave every resource arm of `Breach` evaluating a zero vector forever and the wall
    // deadline the sole reachable verdict. Seating stays `Evict`'s alone, so the three triggers share ONE
    // transition site and a breach cannot seat a disposition by a path the operator command does not take.
    public static IO<Option<Breach>> Enforce(SandboxRuntime runtime, PluginInstance plugin, Op key) =>
        from _charged in IO.lift(() => plugin.Quota.Charge(Observed(plugin)))
        from breach in plugin.Quota.Breach(runtime.Clocks, key).Match(Succ: IO.pure, Fail: IO.fail<Option<Breach>>)
        select breach;

    // Every trigger takes this ONE seat-and-drain entry. A first eviction KILLS and drains the vehicle; a
    // second on an already-evicted plugin QUARANTINES and holds the admitted artifact, which is what makes a
    // repeat offender's evidence survive for review instead of dying with its vehicle — and it drains
    // nothing, because the vehicle the first kill disposed is already gone. The `Committed` verdict is what
    // separates the two: a losing trigger reads `Ceded`, disposes no store the winner holds, and publishes no
    // second account of one kill.
    public static IO<Option<SandboxReceipt>> Evict(SandboxRuntime runtime, PluginInstance plugin, EvictionCause cause, Op key) =>
        Cell.Commit(plugin.Disposition, held => held is Quarantine.Active
            ? new Quarantine.Killed(cause)
            : new Quarantine.Quarantined(cause, plugin.Artifact)) is Transition<Quarantine>.Committed { State: Quarantine.Killed }
            ? Drain(runtime, plugin, cause, key).Map(Some)
            : IO.pure(Option<SandboxReceipt>.None);

    // Pacer fold: one epoch tick charges and grades every live plugin and evicts the ones a breach
    // caught, so the wall guarantee reaches a vehicle instead of seating a disposition nothing acts on.
    public static IO<Seq<SandboxReceipt>> Sweep(SandboxRuntime runtime, Seq<PluginInstance> live, Op key) =>
        live.TraverseM(plugin =>
                from breach in Enforce(runtime, plugin, key)
                from receipt in breach.Match(
                    Some: hit => Evict(runtime, plugin, new EvictionCause.BreachedCase(hit), key),
                    None: static () => IO.pure(Option<SandboxReceipt>.None))
                select receipt)
            .As()
            .Map(static rows => rows.Somes().ToSeq());

    // `Release` is the operator's review arm and the ONLY path back to service: it re-seats a quarantined
    // plugin's disposition so the grant handle admits again, and refuses an Active or Killed one because
    // neither has evidence under review — a killed plugin's vehicle is gone and reinstating it would hand a
    // caller a handle onto a disposed store.
    public static Fin<Quarantine> Release(PluginInstance plugin, Instant at) =>
        Cell.Commit(plugin.Disposition, held => held is Quarantine.Quarantined ? new Quarantine.Released(at) : held) switch {
            Transition<Quarantine>.Committed { State: Quarantine.Released released } => Fin.Succ<Quarantine>(released),
            var other => Fin.Fail<Quarantine>(new SandboxFault.Quarantined($"{plugin.PluginId}: {other.Current} is not under review")),
        };

    // The drain disposes what the eviction seated. A wasm guest drains on `IncrementEpoch` past the
    // deadline the store armed at load — the observing frame seated the code on the capsule and disposal then
    // reclaims the linear memory the guest held for process life. `Cell.Take` DRAINS that seat, so the receipt
    // reads the code once and a guest that was idle when the eviction landed answers `Idle` rather than a
    // defaulted preemption the engine never performed. A child drains on its FORCED EDGE, the whole-tree
    // kill: a plain `Kill` signals one pid and orphans a spawned worker, while the cooperative `ShutdownAsync`
    // ahead of it buys a grace window and nothing else, since a hostile child ignores SIGTERM and reports no
    // progress. Both facts ride the receipt — the census as the verdict and the tree kill as the act.
    static IO<SandboxReceipt> Drain(SandboxRuntime runtime, PluginInstance plugin, EvictionCause cause, Op key) =>
        from _cancel in IO.lift(() => { plugin.Spine.Source.Cancel(); return unit; })
        from proof in plugin.Vehicle.Switch(
            state: runtime,
            wasmCase: static (host, wasm) => IO.lift(() => {
                host.Engine.IncrementEpoch();
                Transition<Option<TrapCode>> drained = Cell.Take(wasm.Capsule.Trapped);
                wasm.Capsule.Dispose();
                return drained.Current.Match(
                    Some: static code => (DrainProof)new DrainProof.TrappedCase(code),
                    None: static () => new DrainProof.IdleCase());
            }),
            childCase: static (_, child) =>
                from _drain in IO.liftAsync(async () => { await child.Peer.Control.ShutdownAsync(); return unit; })
                from _forced in IO.lift(() => { child.Peer.Child.Iter(static spawned => spawned.Child.Kill(entireProcessTree: true)); return unit; })
                from count in child.Provider.Residual(child.Peer)
                select (DrainProof)new DrainProof.CensusedCase(count))
        let receipt = new SandboxReceipt(
            plugin.PluginId, plugin.Scope.ScopeHash, EvictionCauseWire.Of(cause), proof, runtime.Clocks.Now)
        from _fanned in runtime.Sink.Send(
            Correlation.Mint(), TenantContext.Current, TelemetrySource.AppHost, ReceiptKind.Eviction.Key,
            JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host))
        select receipt;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
