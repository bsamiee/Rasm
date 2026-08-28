# [RASM_GRASSHOPPER_PLATFORM_COMPOSITION]

`PlatformRoot` is the folder's ONE in-package composition boundary: plugin identity through the kernel `PackageIdentity` resolve, the session's one `MonotonicTimeline`, one bounded `FaultCell`, the process-wide conversion-broker registry, and the mount roster that reaches every owner family the plugin wires at load. `apps/<app>/` plugin shells compose this page and nothing deeper — every AppHost, DI, and cache lacing stays root material described here as boundary obligations, never package code.

Mount roster is what makes `ARCHITECTURE.md`'s S2 claim a producer rather than prose: each Canvas, Shell, and Components owner family that no interior page composes names its entry HERE, and an owner reachable from no roster row is deleted, never advertised.

## [01]-[INDEX]

- [02]-[IDENTITY]: kernel `PackageIdentity<HookScope, Unit>` composition — the plugin identity mint and the telemetry-capsule lacing obligations.
- [03]-[ROOT]: `PlatformRoot` — the load-time capsule holding session time, fault custody, the broker registry, mount roster, and cache-substrate obligations.

## [02]-[IDENTITY]

- Owner: kernel `PackageIdentity<HookScope, Unit>` (`Rasm/Domain/frame.md#[05]`) — the GH instantiation of the branch's one plugin-identity resolve; this page declares NO identity record of its own. `HookScope` (`Shell/hooks.md`) is the typed plugin key; the host-fact slot is `Unit` because GH publishes no host-package evidence beyond what the assembly answers.
- Entry: `PackageIdentity<HookScope, Unit>.Resolve(pluginRoot, plugin, host: None)` — content root, version, and load context resolve at the kernel; `PackageIdentity<HookScope, Unit>.PluginSlot` is the one plugin dimension key every emitting surface reads.
- Law: the plugin discriminator admits through the typed `HookScope` — the same key space the hook dispatch and the `gh.plugin` meter tag share — so the telemetry resource attribute and every per-plugin surface spell one identity by construction; a raw-string plugin parameter is the deleted fork.
- Law: capsule cardinality is one per plugin `AssemblyLoadContext`, opened once at plugin load, never per canvas or component; a second plugin is a second resolve and a second open with its own discriminator.
- Boundary: the AppHost lacing is the `apps/<app>/` plugin shell's alone — over one resolved identity the shell gates `ProfileSurface.Resolve` on the `HostRows.Gh2` row (`Tenancy.None`, `DeploymentTopology.InHost`, `LifecycleOwner.CallerOwned`, `Isolation.InProc`, no providers) under `TelemetryDomain.Grasshopper.Key`, `Environments.Production`, and the identity's content root and version, then opens `PluginTelemetryHost.Open` on the identity's `Alc` with the one self-minting `TelemetryContributorPort` `Shell/telemetry.md` spells — `TelemetrySource.Grasshopper.Key` scope, empty `Instruments`, `GhInstruments.Rows` published, `GhInstruments.Board` on the pack column — and the plugin discriminator read off the identity as `TelemetryDomain.Host.Measure(PackageIdentity<HookScope, Unit>.PluginSlot)`; `SignalGovernance.Rostered` refuses a bare literal. Lifetime is the capsule's own `AssemblyLoadContext.Unloading` hook — `ForceFlush` then `Dispose` per the AppHost provider-lifetime law; `Environments.Production` floors the environment row and `OTEL_RESOURCE_ATTRIBUTES` outranks it at deploy.
- Packages: `Rasm` and BCL inbox alone — `Rasm.AppHost`, `Microsoft.Extensions.Hosting`, and `NodaTime` are shell references, never this package's.
- Growth: a new plugin-side resource dimension is one kernel `PackageIdentity` column both boundaries answer; a first GH-declared `InstrumentSpec` row is one entry on the port's `Instruments` seq at the shell.

## [03]-[ROOT]

- Owner: `PlatformRoot` — the load-time capsule the shell opens once per plugin: the resolved identity, the session's ONE `MonotonicTimeline`, its ONE bounded `FaultCell`, the conversion-broker registry cell `Components/data.md`'s `Coerce` reads, and one release roster. Every process-wide registry the folder holds seats here, so no library page carries a composition-root static (folder RULINGS `[02]`).
- Entry: `PlatformRoot.Open(pluginRoot, plugin, faultCapacity, time)` → `Fin<Lease<PlatformRoot>>` FORCES the folder's four roster proofs first — `VerdictPort.RosterProof`, `PaintLog.Proof`, `InteractionLog.Proof`, `JournalLog.Proof`, each internal to this assembly and reachable from no other root — then resolves the identity, mints the timeline and fault cell from one supplied time provider, and seats the empty broker cell; `Hold(Lease<T>)` transfers each mounted lease into root custody; disposal releases the mounted leases in reverse mount order through the lease.
- Law: `MonotonicTimeline.Of(time)` is callable HERE and nowhere else in the folder (folder RULINGS `[02]`) — one injected timeline per session makes gauged spans from one gesture orderable, and each gauged owner takes `Clock` as a REQUIRED parameter. Production shells supply `TimeProvider.System`; test hosts supply fakes through the same slot.
- Law: `Faults` is the whole callback-custody cell handed to every mount row that parks evidence and to `HookSet.Of(cell:)`; the root never accepts the kernel's default cell and no owner mints another ring, so `Parked`/`Shed`/`Lost` describe the complete plugin boundary.
- Law: the broker registry is the root's INSTANCE, never a library static — the root constructs `Components/data.md`'s one `BrokerLedger`, scope-ranked conversion rows enroll against it at plugin load, and `Coerce` reads the ledger it was handed, so a collectible plugin ALC drops exactly its own rows with the root's lease.
- Law: measurements write at their producing site through `Shell/telemetry.md`'s `GhInstruments` members and the journal keeps the event stream alone — the root tees nothing; the one write the root itself owns is `PaintProof.Judge`'s breach, which it hands to `GhInstruments.Proofed` because the judging site is this roster row.
- Law: the mount roster is the S2 producer. At load, the shell walks the rows below in order; teardown is the exact reverse. Owner family no row reaches has no consumer and DELETES with its prose — the roster is the census the fake-density gate reads.
- Law: every lease minted while walking the roster enters `Hold` before the next row starts; a contended custody write refuses as `GhFault.Registration`, and teardown runs every inverse through kernel `Custody.Release`; retained mounts park their own release refusals on the shared `Faults`, while the root parks the fold's thrown-disposal aggregate because `IDisposable` has no outward result.

| [INDEX] | [FAMILY]                | [ENTRY]                                                              | [OWNING_PAGE]                       |
| :-----: | :---------------------- | :------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | plugin registration     | `Catalogue.Exported` handed to the GH2 plugin loader                 | `Components/component`              |
|  [02]   | conversion brokers      | broker rows registered against `PlatformRoot.Brokers`                | `Components/data`                   |
|  [03]   | editor shell            | `EditorShell.Mount` over the GH2 editor singleton                    | `Shell/editor`                      |
|  [04]   | chrome module           | `Chrome.Mount` seating bars, panels, and floats                      | `Shell/chrome`                      |
|  [05]   | canvas owners           | `CanvasOperator` + `SpecResponder` mount + `RouteStyle.Install`      | `Canvas/*`                          |
|  [06]   | paint plan + snap field | `PaintAnchor.Mount` planner (`Traced`→`WirePass.Plan`) + `SnapField` | `Canvas/paint`/`layout`             |
|  [07]   | pacer + display link    | `CanvasPacer.Mount` + `MotionAttachment.Attach`                      | `Canvas/motion` + `Platform/layers` |
|  [08]   | hook dispatch + journal | `HookSet.Of` + `SessionJournal.Mount` over the evidence drain        | `Shell/hooks`/`journal`             |
|  [09]   | capture proof           | `SessionCapture.Open` + `PaintProof.Judge` → `GhInstruments.Proofed` | `Platform/capture`                  |
|  [10]   | telemetry capsule       | `GhTelemetry.Of` seating the meter and logger on the per-ALC cell    | `Shell/telemetry`                   |

- Boundary: cache substrate — the folder currently CACHES NOTHING (`Shell/session.md`'s cache module deleted with zero consumers), so no `HybridCache` registers and the package reference is retired. Future cached carrier re-mints at the shell under these standing obligations: the codec for any non-inbuilt carrier registers BEFORE `AddHybridCache` (the substrate's try-add seeding makes the earlier registration the binding one); `MaximumPayloadBytes` sizes against the largest admitted payload because an over-quota payload logs and returns uncached silently; `ReportTagMetrics` arms the per-document hit/miss dimension and no folder instrument doubles it; no L2 registers unless the shell binds a real `IDistributedCache` — the block is pure L1 by construction.
- Packages: `Rasm.Domain` (`Lease<T>`, `PackageIdentity`, `FaultCell`, `Custody`), `Rasm.Numerics` (`Dimension`), `Rasm.Parametric` (`MonotonicTimeline`), `Components/data.md` (`BrokerLedger`), LanguageExt.Core.
- Growth: a new mounted family is one roster row naming its entry; a new process-wide registry is one cell on this capsule, never a static on a library page.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Reflection;
using Rasm.Domain;
using Rasm.Grasshopper.Components;
using Rasm.Grasshopper.Shell;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Platform;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlatformRoot : IDisposable {
    private static readonly HookId ReleasePoint = HookId.Create(value: "rasm.grasshopper.platform.root");
    private readonly Atom<Seq<Func<Fin<Unit>>>> mounts = Atom(Seq<Func<Fin<Unit>>>());

    private PlatformRoot(
        PackageIdentity<HookScope, Unit> identity,
        MonotonicTimeline clock,
        FaultCell faults) =>
        (Identity, Clock, Faults, Brokers) = (identity, clock, faults, new BrokerLedger());

    public PackageIdentity<HookScope, Unit> Identity { get; }

    public MonotonicTimeline Clock { get; }

    public FaultCell Faults { get; }

    public BrokerLedger Brokers { get; }

    public Fin<Unit> Hold<T>(Lease<T> mount) where T : class, IDisposable {
        return Admit.Need(mount).Bind(held => Cell.Commit(mounts, rows => rows.Add(() => Fin.Succ(held.Dispose()))).Switch(
            committed: static _ => Fin.Succ(unit),
            ceded: static () => Unwind(new GhFault.Registration(nameof(Hold))),
            refused: static row => Unwind(row.Cause),
            contended: static () => Unwind(new GhFault.Registration(nameof(PlatformRoot)))));
    }

    private static Fin<Unit> Unwind<T>(Lease<T> held, Error primary) where T : class, IDisposable =>
        Fin.Fail<Unit>(primary).Rollback(release: () => Fin.Succ(held.Dispose()));

    public static Fin<Lease<PlatformRoot>> Open(
        Assembly pluginRoot,
        HookScope plugin,
        Dimension faultCapacity,
        TimeProvider time) {
        return from _rosters in Seq(VerdictPort.RosterProof(), PaintLog.Proof(), InteractionLog.Proof(), JournalLog.Proof())
                   .Traverse(static proof => proof.ToValidation()).As().ToFin()
               from provider in Admit.Need(time)
               from capacity in Acceptance.Value(value: faultCapacity)
               from identity in PackageIdentity<HookScope, Unit>.Resolve(pluginRoot: pluginRoot, plugin: plugin)
               from clock in MonotonicTimeline.Of(provider: provider)
               select (Lease<PlatformRoot>)new Lease<PlatformRoot>.Owned(Value: new PlatformRoot(
                   identity: identity,
                   clock: clock,
                   faults: new FaultCell(cap: capacity, clock: provider)));
    }

    public void Dispose() {
        Seq<Func<Fin<Unit>>> releases = Cell.Take(mounts).Current.Rev().Strict();
        Fin<Unit> settled = Custody.Release(releases, ReleaseOp);
        settled.IfFail(error => ignore(Faults.Park(point: ReleasePoint, cause: error)));
    }
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]       | [OWNER]                  | [RESULT]                              | [CASES] |
| :-----: | :-------------- | :----------------------- | :------------------------------------ | :-----: |
|  [01]   | plugin identity | kernel `PackageIdentity` | one `Resolve`, typed `HookScope` key  |    1    |
|  [02]   | session clock   | `PlatformRoot.Clock`     | one mint, injected into gauged owners |    1    |
|  [03]   | fault custody   | `PlatformRoot.Faults`    | one bounded cell across every mount   |    1    |
|  [04]   | mount roster    | `[03]` table             | one ordered walk, reverse teardown    |    8    |

`PlatformIdentity`/`PlatformTelemetry` deleted onto the kernel resolve; the CoreAnimation module lives at `Platform/layers.md`; the AppHost and cache lacings are shell obligations, never package fences.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
