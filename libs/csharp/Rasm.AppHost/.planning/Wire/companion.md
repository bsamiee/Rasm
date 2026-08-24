# [APPHOST_COMPANION_SIDECAR]

The inbound serving counterpart to the outbound boundary: three `ModalityRow` rows key on the Tier-0 `DeploymentTopology` closed axis and carry their spawn-attach-degrade-forward capability set under one legal-corner law, one `PeerRoster` folds every accepted connection into a lease-epoch attached-peer set through verdict-returning transitions, one `ControlVerb` roster folds local control transitions under one continuation, and one `ServiceHost` registration mounts control, capability discovery, health, every composition-supplied served plane, and the co-hosted asset seat over a Unix domain socket. The page also owns the cross-process degradation cascade, peer-credential admission, host-binding acquisition, and CloudEvents ingress door.

## [01]-[INDEX]

- [02]-[PROCESS_MODALITY]: Three modality rows under one capability law, the lease-epoch attached-peer roster, and the fault and signal families this page owns.
- [03]-[CONTROL_SERVICE]: Wire verb roster folded onto its existing owners under one audit-trace-fan-fire continuation.
- [04]-[SERVICE_HOST]: Generated control and capability-discovery services, served-plane mounting, assets, and local intake.
- [05]-[DEGRADATION_CASCADE]: Parent floor written to the child cell over the control hop.
- [06]-[PEER_ADMISSION]: Accept-side peer-credential read over the managed raw-socket-option route.
- [07]-[HOST_BINDING]: `(HostOs, ActivationSource, AddressKind)` bind-policy table, its acquisition arms, reuse, and override.
- [08]-[EVENT_INGRESS]: Authenticated CloudEvents HTTP door — abuse handshake, exact-body custody, trust gates, and dispatch.

## [02]-[PROCESS_MODALITY]

- Owner: `ModalityCapability` `[SmartEnum<string>]` realizing kernel `ICapability<ModalityCapability>` — the four peer-modality capabilities; `ModalityRow` the per-topology policy record carrying that capability SET; `ModalityRows` the frozen row set with its total dispatch and its `CapabilityLaw`; `CompanionPeer` the attached-child capsule the modality row produces; `PeerRoster` the attached-connection cell carrying a monotone lease epoch; `RosterEntry` the per-connection lease record; `RosterReceipt` the join/renew/drop transition projection; `CompanionFault` `[Union]` the page's own fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Companion`); `CompanionSignal` `[Union]` the rail fact every transition on this page carries.
- Cases: three of the closed axis's six values carry a peer modality — `Companion` is the host-spawned single-shot child (`Spawn`, `Degrade`), `Sidecar` is the co-deployed attach-only peer this process never started (`Admit`, `Forward`), `Service` is the independently-managed peer this process dials and admits (`Spawn`, `Admit`, `Degrade`); `InHost`, `Edge`, and `Cli` reach no peer and refuse on the typed rail naming the axis; three roster transitions — join on accept, renew on heartbeat, drop on lease expiry or disconnect; `CompanionSignal.Drain` carries the generated parent-side drain response, the peer pid, and its generated worst deadline outcome as one typed settled case.
- Entry: `Fin<ModalityRow> Row` is the extension property total state-free `Switch` from topology value to frozen row, railing `CompanionFault.Excluded` on the three values this page serves no modality for and admitting the row's corner through `ModalityRows.Law`; `Attach(PeerRoster roster, ModalityRow row, OutboundRuntime outbound, ProcessStartInfo spec, RedrivePolicy attach, Func<Option<int>, Fin<DiscoveryManifest>> manifestOf, Func<DiscoveryManifest, CancellationToken, IO<Unit>> drainFan, GrpcChannelPolicy policy)` returns `IO<CompanionPeer>` and carries the gauged spawn-and-dial effect through `Wire/outbound#DELIVERY_FANOUT` `Discovery.Spawn`'s own five-parameter arity — the outbound runtime and the attach redrive ride the `HopRows.CompanionSpawn` hop — the manifest reader taking the started child's pid as an OPTION so the attach-only arm states that it spawned nothing; `ForwardWrite(PeerRoster roster, ModalityRow row, CommandIntent intent, Func<RosterEntry, CommandIntent, IO<CommandReceipt>> hop)` returns `IO<Option<Seq<CommandReceipt>>>` — the `Forward`-gated durable-write forward; `PeerRoster.Accept(ModalityRow row, ServerCallContext context, DiscoveryManifest manifest)` returns `IO<Fin<RosterReceipt>>` — the serving-side accept hop; `PeerRoster.Admit(PeerCredential credential, DiscoveryManifest manifest, Instant now)`, `.Renew(int pid, Instant now)`, and `.Drop(int pid, Instant now)` each fold one transition over the cell and return `IO<Fin<RosterReceipt>>`, so a verb applied to a pid the roster does not hold answers a REFUSAL rather than a receipt naming a transition that never happened.
- Auto: `Attach` reads the discovery manifest through the bound `Discovery.Read` projection and dials the control channel through `Discovery.Connect`, running the single-shot `Discovery.Spawn` only on rows admitting `Spawn` and the attach-only read only on rows admitting `Admit` — a row holding neither is unrepresentable because the law bars the empty corner — and both arms bracket the dial with the roster's own `ClockPolicy.Line` so one `ModalityReceipt` carries the real outcome, the MEASURED monotone elapsed, and the capability set the cascade consults; `Accept` reads the accepted socket off the connection's `IConnectionSocketFeature`, folds it through `PeerAdmission.Read`, and hands the credential to `Admit`, so the credential chain runs accept to admit with no prose hop between; `Admit` keys the entry by the kernel-reported `PeerCredential.Pid` — never the manifest's self-asserted pid — stamps the lease deadline from `LeasePolicy.Maintenance.CrashStaleness` so a peer's lease lapses on the same crash-staleness window the maintenance lease uses, and fires the bound `Contribute` edge so the local attach reaches the cluster view as a `Joining` row the probe sweep then grades; `Renew` extends the lease and `Sweep(Instant now)` drops every entry whose lease lapsed, so a vanished peer leaves the roster without an explicit disconnect; every landed transition mints one `RosterReceipt` fanned through the one `FactSink` and fired at `AppHostPoint.Companion`; the outbound drain consumer admits the generated reply once, folds its validated step outcomes onto the generated severity vocabulary, and fans the whole reply through the same sink before settling the hop.
- Receipt: `ModalityReceipt` — topology key, the peer pid as an `Option`, attach outcome, elapsed `Duration`, and the row's capability set with `CascadeEligible` DERIVED from it; `RosterReceipt` — transition kind, peer pid, the peer uid as an `Option`, lease epoch, attached-count after the fold, `Instant`; generated `DrainRuntimeResponse` — the parent-side drain evidence, carried unchanged as ProtoJSON while `CompanionSignal.Drain` adds only in-process peer identity and the derived generated outcome.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (kernel `CapabilitySet`/`CapabilityLaw`/`Cell`/`Transition`), Grpc.Net.Client, Grpc.AspNetCore.Server (`ServerCallContextExtensions.GetHttpContext`, `IConnectionSocketFeature`), BCL inbox
- Growth: one `ModalityRow` over an existing `DeploymentTopology` value absorbs a new peer shape — the axis roster is Tier-0's and grows there alone; a new peer capability is one `ModalityCapability` row with the legal corners that admit it, never a bool column; a new roster transition is one `RosterTransition` case with one fold arm; a new observable transition is one `CompanionSignal` case with no roster edit; a new refusal is one `CompanionFault` case and the owning `FaultBand` span edit; zero new surface.
- Boundary: the modality row consumes `OutboundHop.CompanionSpawn` and `OutboundHop.LocalIpc` from the dial-out owner and never re-declares the spawn or connect mechanics — `Discovery.Spawn`, `Discovery.Connect`, and `Discovery.Read` carry the bytes; the row keys on `DeploymentTopology` and mints no vocabulary of its own — the closed axis at Tier-0 `[10]-[CONSUMPTION_MODEL]` already spells `companion` and `sidecar`, and a local re-mint of those two values is the anchoring defect `[CONSUMPTION_DESCRIPTOR]` forecloses; a fourth `paired-peer` value is the rejected form for a second reason — pairing DIRECTION is already two capabilities, so a set holding `Admit` without `Spawn` states the symmetric attach exactly and `Service` is the axis value that peer already carries; FOUR ADJACENT BOOLS WERE A CORNER LAW IN DISGUISE — three of sixteen corners are legal and the other thirteen name peers this page cannot serve (a row spawning without degrading, a row forwarding writes it never admitted, the empty row that inherits the attach arm), so the set rides `CapabilitySet<ModalityCapability>` under an UNCONDITIONAL `CapabilityLaw.Legal` roster and `ModalityRows.Law.Admit` refuses at the `Fin` mint; NAMED LOSS — per-column compile-time exhaustiveness, bought back twice by that admit and by every consumer stating the capability it needs as a value through `Admits`; `ModalityReceipt.CascadeEligible` DERIVES from the carried set rather than storing a copy of one column, because a stored mirror answers the question its source already answers and diverges the moment a row moves; the attach deadline is the `DeadlineClass.HopAttempt` row read by projection and the lease deadline is the `LeasePolicy.Maintenance.CrashStaleness` value, never a literal here; `CompanionPeer` carries the `CompanionChild` produced by the outbound spawn and the `GrpcChannel` produced by the control dial so one capsule owns both legs of an attached child; `PeerRoster` is the single host-side attached-connection owner — the lease epoch is a monotone `ulong` bumped on every join and drop so a stale peer reconnecting under a prior epoch is detectable, and the roster never re-mints presence: it is the beat PRODUCER of the `Rasm.Persistence` `Version/ledger#PRESENCE` EPHEMERAL awareness lane — each join and drop crosses as the Persistence-OWNED `PresenceRow(Actor, State, At, Ttl)` through `Awareness.Present(actor, state, ttl, frame)` on the `Runtime/resources#DRAIN_QUEUES` `DrainSurface` lane, `durable: false`, never the durable store, never the exactly-once CDC envelope, and no AppHost type crosses down; THE ABSENT IDENTIFIER IS AN OPTION, NOT A ZERO — a receipt for a pid the roster does not hold reported uid 0, which names root, and an attach that faulted before a manifest existed reported pid 0, which names the kernel's own scheduler, so both columns are `Option`-shaped and an audit reader can no longer read a fabricated superuser or a fabricated peer out of a missing entry; that same law bars the pid-0 SENTINEL an attach-only manifest read once passed as its argument, so the reader takes `Option<int>` and the absent child is a value rather than a number the callee must know to disbelieve; FOREIGN ERRORS ARE ADOPTED, NEVER LAUNDERED — `CompanionFault.Of` passes a companion fault through untouched and wraps anything else as `Foreign`, carrying the original `Error` so its numeric identity and retry semantics survive instead of being rebuilt from message text; NAMED LOSS — the `Verify` case the ingress signature gate once minted from a message, which `Foreign` replaces at its own offset: the verifier's typed refusal now reaches the delivery tally under the band code its own owner gave it, which is stronger than a companion-band name carrying that owner's text; `WireHealth` reads the attached-count for per-peer serving status, never a second roster; the two-tier membership law holds — `PeerRoster` is the LOCAL kernel-credentialed attach set contributing into `Wire/coordination#MEMBERSHIP_VIEW` through `Membership.Contribute`, `FleetRoll` reads `MembershipView.Serving` (cluster liveness) for its fleet wave while each node's actual roll dials local over this control hop, and `ForwardWrite` reads `PeerRoster.Attached` as the LOCAL forwarding set; the page is host-local and crosses no browser or peer TS wire of its own — the verb messages are Rasm.Compute/Runtime/wire#PROTO_VOCABULARY-owned protobuf consumed here, and `RosterReceipt`/`ModalityReceipt` reconstruct as the `Runtime/ports#TS_PROJECTION` `ReceiptHeaderWire` beside an AppHost family arm the corpus still owes, so the page authors no `TS_PROJECTION` cluster and mints no second wire shape.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.Json;
using CloudNative.CloudEvents;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using LanguageExt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using NodaTime;
using OpenTelemetry.Context;
using Rasm.Domain;
using Thinktecture;
using CapabilityContract = Rasm.Contracts.Capability.V1;
using static LanguageExt.Prelude;

namespace Rasm.AppHost.Wire;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModalityCapability : ICapability<ModalityCapability> {
    public static readonly ModalityCapability Spawn = new("spawn", rank: 0);
    public static readonly ModalityCapability Admit = new("admit", rank: 1);
    public static readonly ModalityCapability Degrade = new("degrade", rank: 2);
    public static readonly ModalityCapability Forward = new("forward", rank: 3);

    public int Rank { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ModalityRow(
    DeploymentTopology Topology,
    CapabilitySet<ModalityCapability> Capabilities,
    HopIdempotency Idempotency,
    DeadlineClass Attach);

public sealed record CompanionPeer(
    DeploymentTopology Topology,
    Option<CompanionChild> Child,
    GrpcChannel Control,
    DiscoveryManifest Manifest);

// THE ABSENT PID IS AN OPTION, NOT A ZERO: an attach that faulted before a manifest existed has no peer to
// name, and pid 0 names the kernel's own scheduler on both supported platforms.
public readonly record struct ModalityReceipt(
    DeploymentTopology Topology,
    Option<int> PeerPid,
    HopOutcome Attach,
    Duration Elapsed,
    CapabilitySet<ModalityCapability> Capabilities) {
    public bool CascadeEligible => Capabilities.Admits(ModalityCapability.Degrade);
}



// Numeric identity is generated from each direct leaf's `[FaultCase]`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CompanionFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Companion;
    private CompanionFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    public static CompanionFault Of(Error error) => error as CompanionFault ?? new Foreign(error);


    [FaultCase(0)]
    public sealed partial record Excluded : CompanionFault { public Excluded(string detail) : base(detail) { } }

    [FaultCase(1)]
    public sealed partial record Credential : CompanionFault { public Credential(string detail) : base(detail) { } }

    [FaultCase(2)]
    public sealed partial record Unattached : CompanionFault {
        public Unattached(int pid) : base(string.Create(CultureInfo.InvariantCulture, $"pid:{pid}")) { }
    }

    [FaultCase(3)]
    public sealed partial record Bind : CompanionFault { public Bind(string detail) : base(detail) { } }

    [FaultCase(4)]
    public sealed partial record Held : CompanionFault {
        public Held(string service, string path) : base($"{service}:{path}") { }
        public override Retriability Retriability => Retriability.Transient;
    }

    [FaultCase(5)]
    public sealed partial record Activation : CompanionFault { public Activation(string detail) : base(detail) { } }

    [FaultCase(6)]
    public sealed partial record Handshake : CompanionFault {
        public Handshake(string field, int httpStatus = StatusCodes.Status400BadRequest) : base(field) => HttpStatus = httpStatus;
        public int HttpStatus { get; }
    }

    // Adoption carries the cause WHOLE as the inner, so its own code, recovery, and
    // retriability survive the crossing. The signature gate's refusal reaches a reader as the verifier's own
    // typed fault rather than rebuilding its message in this band, which would erase the verifier's code.
    [FaultCase(7)]
    public sealed partial record Foreign : CompanionFault, ICausedFault {
        public Foreign(Error inner) : base(inner.Message) => Cause = inner;
        public Error Cause { get; }
        public override Retriability Retriability => Cause is Fault fault ? fault.Retriability : Retriability.Terminal;
    }
}

[Union]
public abstract partial record CompanionSignal {
    private CompanionSignal() { }

    public sealed record Modality(ModalityReceipt Settled) : CompanionSignal;

    public sealed record Roster(RosterReceipt Settled) : CompanionSignal;

    public sealed record Verb(VerbReceipt Settled) : CompanionSignal;

    public sealed record Cascade(CascadeReceipt Settled) : CompanionSignal;

    public sealed record Bound(BindReceipt Settled) : CompanionSignal;

    public sealed record Ingress(Delivery Settled) : CompanionSignal;

    public sealed record Drain(
        int PeerPid,
        global::Rasm.Contracts.Compute.V1.DeadlineOutcome Outcome,
        DrainRuntimeResponse Reply) : CompanionSignal;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ModalityRows {
    public static readonly ModalityRow Companion = new(
        DeploymentTopology.Companion,
        CapabilitySet<ModalityCapability>.Of(ModalityCapability.Spawn, ModalityCapability.Degrade),
        HopIdempotency.SingleShot,
        DeadlineClass.HopAttempt);

    public static readonly ModalityRow Sidecar = new(
        DeploymentTopology.Sidecar,
        CapabilitySet<ModalityCapability>.Of(ModalityCapability.Admit, ModalityCapability.Forward),
        HopIdempotency.Keyed,
        DeadlineClass.HopAttempt);

    public static readonly ModalityRow Service = new(
        DeploymentTopology.Service,
        CapabilitySet<ModalityCapability>.Of(ModalityCapability.Spawn, ModalityCapability.Admit, ModalityCapability.Degrade),
        HopIdempotency.Keyed,
        DeadlineClass.HopAttempt);

    public static readonly CapabilityLaw<ModalityCapability> Law = new(Seq(
        Companion.Capabilities, Sidecar.Capabilities, Service.Capabilities));

    extension(DeploymentTopology topology) {
        public Fin<ModalityRow> Row =>
            topology.Switch(
                inHost: static () => Fin.Fail<ModalityRow>(new CompanionFault.Excluded($"topology:{DeploymentTopology.InHost.Key}:reaches-no-peer")),
                sidecar: static () => Fin.Succ(Sidecar),
                companion: static () => Fin.Succ(Companion),
                service: static () => Fin.Succ(Service),
                edge: static () => Fin.Fail<ModalityRow>(new CompanionFault.Excluded($"topology:{DeploymentTopology.Edge.Key}:reaches-no-peer")),
                cli: static () => Fin.Fail<ModalityRow>(new CompanionFault.Excluded($"topology:{DeploymentTopology.Cli.Key}:reaches-no-peer")))
            .Bind(row => Law.Admit(row.Capabilities).Map(_ => row));
    }

    public static IO<CompanionPeer> Attach(
        PeerRoster roster, ModalityRow row, OutboundRuntime outbound, ProcessStartInfo spec, RedrivePolicy attach,
        Func<Option<int>, Fin<DiscoveryManifest>> manifestOf,
        Func<DiscoveryManifest, CancellationToken, IO<Unit>> drainFan, GrpcChannelPolicy policy) =>
        from start in roster.Stamp
        from peer in Dial(row, outbound, spec, attach, manifestOf, drainFan, policy)
            .Catch(error => Settled(roster, row, None, new HopOutcome.Faulted(error), start).Bind(_ => IO.fail<CompanionPeer>(error)))
        from _ in Settled(roster, row, Some(peer.Manifest.Pid), new HopOutcome.Delivered(), start)
        select peer;

    public static IO<Option<Seq<CommandReceipt>>> ForwardWrite(
        PeerRoster roster, ModalityRow row, CommandIntent intent, Func<RosterEntry, CommandIntent, IO<CommandReceipt>> hop) =>
        row.Capabilities.Admits(ModalityCapability.Forward)
            ? roster.Attached.TraverseM(entry => hop(entry, intent)).As().Map(Some)
            : IO.pure(Option<Seq<CommandReceipt>>.None);

    // Manifest reads take the started child's pid as an OPTION, so the attach-only arm states that it spawned none
    // instead of asking for pid 0 — the spawn arm adapts to `Discovery.Spawn`'s own started-child arity at the
    // one call site that has a real pid to give.
    static IO<CompanionPeer> Dial(
        ModalityRow row, OutboundRuntime outbound, ProcessStartInfo spec, RedrivePolicy attach,
        Func<Option<int>, Fin<DiscoveryManifest>> manifestOf,
        Func<DiscoveryManifest, CancellationToken, IO<Unit>> drainFan, GrpcChannelPolicy policy) =>
        row.Capabilities.Admits(ModalityCapability.Spawn)
            ? Discovery.Spawn(outbound, spec, attach, pid => manifestOf(Some(pid)), drainFan)
                .Bind(child => IO.pure(new CompanionPeer(row.Topology, child, Discovery.Connect(child.Manifest, policy), child.Manifest)))
            : IO.lift(() => manifestOf(None))
                .Bind(read => read.Match(
                    Succ: manifest => IO.pure(new CompanionPeer(row.Topology, None, Discovery.Connect(manifest, policy), manifest)),
                    Fail: fault => IO.fail<CompanionPeer>(fault)));

    static IO<Unit> Settled(PeerRoster roster, ModalityRow row, Option<int> pid, HopOutcome outcome, MonotonicStamp start) =>
        from finish in roster.Stamp
        from span in roster.Clocks.Line.Elapsed(start, finish, roster.Key).Match(Succ: IO.pure, Fail: IO.fail<TimeSpan>)
        let receipt = new ModalityReceipt(row.Topology, pid, outcome, Duration.FromTimeSpan(span), row.Capabilities)
        from _ in roster.Fan.Fan(Correlation.Mint(), nameof(ModalityRows), receipt, new CompanionSignal.Modality(receipt))
        select unit;
}

[SmartEnum]
public sealed partial class RosterTransition {
    public static readonly RosterTransition Joined = new();
    public static readonly RosterTransition Renewed = new();
    public static readonly RosterTransition Dropped = new();
}

public sealed record RosterEntry(
    int Pid,
    uint Uid,
    DiscoveryManifest Manifest,
    ulong Epoch,
    Instant JoinedAt,
    Instant LeaseUntil);

public readonly record struct RosterReceipt(
    RosterTransition Transition,
    int Pid,
    Option<uint> Uid,
    ulong Epoch,
    int Attached,
    Instant At);

public sealed record PeerRoster(
    string Service,
    Atom<(HashMap<int, RosterEntry> Entries, ulong Epoch)> Peers,
    Func<PeerCredential, DiscoveryManifest, Unit> Contribute,
    FactSink<CompanionSignal> Fan,
    ClockPolicy Clocks,
    Op Key) {
    public static PeerRoster Boot(
        string service, Func<PeerCredential, DiscoveryManifest, Unit> contribute,
        FactSink<CompanionSignal> fan, ClockPolicy clocks, Op key) =>
        new(service, Atom((HashMap<int, RosterEntry>.Empty, 0UL)), contribute, fan, clocks, key);

    public Seq<RosterEntry> Attached => Peers.Value.Entries.Values.ToSeq();

    public IO<MonotonicStamp> Stamp => Clocks.Line.Capture(Key).Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>);

    // Kestrel exposes the accepted socket through `IConnectionSocketFeature`, so the kernel-reported uid and
    // pid are read off THAT socket; the manifest's own pid is a claim the peer writes about itself.
    public IO<Fin<RosterReceipt>> Accept(ModalityRow row, ServerCallContext context, DiscoveryManifest manifest) =>
        !row.Capabilities.Admits(ModalityCapability.Admit)
            ? IO.pure(Fin.Fail<RosterReceipt>(new CompanionFault.Excluded($"{Service}:{row.Topology.Key}:does-not-admit")))
            : IO.lift(() => Optional(context.GetHttpContext().Features.Get<IConnectionSocketFeature>()))
                .Bind(feature => feature.Match(
                    Some: socket => PeerAdmission.Read(socket.Socket).Match(
                        Succ: credential => Admit(credential, manifest, Clocks.Now),
                        Fail: error => IO.pure(Fin.Fail<RosterReceipt>(error))),
                    None: () => IO.pure(Fin.Fail<RosterReceipt>(new CompanionFault.Credential($"{Service}:no-accepted-socket")))));

    public IO<Fin<RosterReceipt>> Admit(PeerCredential credential, DiscoveryManifest manifest, Instant now) =>
        Commit(RosterTransition.Joined, credential.Pid, Some(credential.Uid), now, held => Some((
                held.Entries.AddOrUpdate(credential.Pid, new RosterEntry(
                    credential.Pid, credential.Uid, manifest, held.Epoch + 1UL, now,
                    now + LeasePolicy.Maintenance.CrashStaleness)),
                held.Epoch + 1UL)))
            .Bind(receipt => receipt.Match(
                Succ: landed => IO.lift(() => Contribute(credential, manifest)).Map(_ => Fin.Succ(landed)),
                Fail: error => IO.pure(Fin.Fail<RosterReceipt>(error))));

    // A renew on a pid the roster does not hold DECLINES: the prior fold swapped an unchanged map and fanned a
    // receipt announcing a renewal that never happened, which reads to every consumer as a live peer.
    public IO<Fin<RosterReceipt>> Renew(int pid, Instant now) =>
        Commit(RosterTransition.Renewed, pid, Uid(pid), now, held => held.Entries.Find(pid).Map(entry => (
            held.Entries.SetItem(pid, entry with { LeaseUntil = now + LeasePolicy.Maintenance.CrashStaleness }),
            held.Epoch)));

    public IO<Fin<RosterReceipt>> Drop(int pid, Instant now) =>
        Commit(RosterTransition.Dropped, pid, Uid(pid), now, held => held.Entries.ContainsKey(pid)
            ? Some((held.Entries.Remove(pid), held.Epoch + 1UL))
            : None);

    public IO<Seq<Fin<RosterReceipt>>> Sweep(Instant now) =>
        Peers.Value.Entries.Values.Filter(entry => entry.LeaseUntil <= now).ToSeq()
            .TraverseM(entry => Drop(entry.Pid, now)).As();

    // Absent means ABSENT: uid 0 is root, so a receipt for a pid the roster does not hold once named the
    // superuser as the peer that transitioned.
    Option<uint> Uid(int pid) => Peers.Value.Entries.Find(pid).Map(static entry => entry.Uid);

    IO<Fin<RosterReceipt>> Commit(
        RosterTransition transition, int pid, Option<uint> uid, Instant now,
        Func<(HashMap<int, RosterEntry> Entries, ulong Epoch), Option<(HashMap<int, RosterEntry> Entries, ulong Epoch)>> step) =>
        IO.lift(() => Cell.Step(Peers, step, new CompanionFault.Unattached(pid)))
            .Bind(settled => settled.Switch(
                committed: landed => Fanned(new RosterReceipt(transition, pid, uid, landed.State.Epoch, landed.State.Entries.Count, now)),
                ceded: _ => IO.pure(Fin.Fail<RosterReceipt>(new CompanionFault.Unattached(pid))),
                refused: declined => IO.pure(Fin.Fail<RosterReceipt>(CompanionFault.Of(declined.Cause))),
                contended: _ => IO.pure(Fin.Fail<RosterReceipt>(new CompanionFault.Unattached(pid)))));

    IO<Fin<RosterReceipt>> Fanned(RosterReceipt receipt) =>
        Fan.Fan(Correlation.Mint(), nameof(PeerRoster), receipt, new CompanionSignal.Roster(receipt)).Map(Fin.Succ);
}
```

```mermaid
stateDiagram-v2
    accTitle: Companion process lifecycle
    accDescr: A discovered companion spawning or attaching directly, serving under a control dial, cascading down and back on parent floor pressure, and draining to termination through the fan hop.
    [*] --> Discovered
    Discovered --> Spawned: Spawn capability
    Discovered --> Attaching: Admit capability
    Spawned --> Attaching: manifest read
    Attaching --> Serving: control dial
    Serving --> Cascading: parent floor
    Cascading --> Serving: parent release
    Serving --> Drained: FanDrain hop
    Drained --> [*]
```

## [03]-[CONTROL_SERVICE]

- Owner: `ControlVerb` `[SmartEnum<string>]` the local verb roster carrying each operator-audit projection as a delegate column; `ControlInbound` the static handler folding each verb onto its existing transition owner; `ControlRuntime` the dependency record; `VerbReceipt` the per-verb projection the sink receives; `ControlServiceImpl` the generated-base implementation `ServiceHost.Map` mounts; `ControlContractInterceptor` the one request/response contract seat; `ControlReplyMap` the one `[Mapper]` projecting the two surviving RPC receipts onto their response messages.
- Cases: set-degradation folds onto `DegradationCell.Force` and drain-runtime onto `Runtime/lifecycle#DRAIN_CONDUCTOR`; both survive as peer-called `ControlService` RPCs.
- Entry: `SetDegradation(ControlRuntime runtime, ServerCallContext context, Control.DegradationLevel level, string reason)` returns `IO<DegradationState>` and `DrainRuntime(runtime, context, Duration inherited, string reason)` returns `IO<DrainReceipt>`; their generated overrides are the complete control RPC surface. `CapabilityDiscoveryServiceImpl.Discover` is the generated discovery request/response boundary and projects the current permitted catalog once.
- Auto: `ControlContractInterceptor` admits each generated request once before the handler and each generated response once after it through `WireAdmission.Validate`; `Continued` then resolves the episode correlation, runs the row's audit projection, continues the caller trace, and fans the typed `VerbReceipt`. The wire level is the generated enum and admits through `DegradationLevel.OfWire` — the roster-derived inverse of the row's `Wire` column, folding `Unspecified` and any foreign ordinal to `None` — and drain inherits the tighter of the caller remainder and `DeadlineClass.DrainCooperative`.
- Receipt: `DegradationState` and the conductor's own `DrainReceipt` cross the wire. `VerbReceipt` carries the local verb row and serialized typed payload the sink fans.
- Packages: LanguageExt.Core, NodaTime, NodaTime.Serialization.Protobuf, Thinktecture.Runtime.Extensions, Riok.Mapperly, Grpc.Core.Api (`Interceptor`), BCL inbox
- Growth: a new RPC exists only when an independently real peer caller and server override both land.
- Boundary: THE PROTO IS THE WIRE ROSTER'S EVIDENCE — `ControlService` declares only `SetDegradation` and `DrainRuntime`, and this page implements exactly those two generated overrides. Reload, tool dispatch, patch dispatch, and support capture stay with their local owners rather than becoming speculative operator RPCs. Response messages project typed receipts field-for-field through one `[Mapper]`; drain-runtime threads the admitted inherited allotment unchanged to the conductor, which owns the one `min(local, inherited)` intersection. `ControlContractInterceptor` reads the central two-rail verdict directly: authored request refusals raise `InvalidArgument`, authored response refusals raise `Internal`, and both pass the unchanged violation sequence into `FaultContext`; an unrostered type or CEL failure raises `Internal` with no fabricated field detail. It is registered for `ControlServiceImpl` alone, so generated health and externally supplied service planes do not pass through a corpus roster they do not belong to. No handler, mapper, or second validator rechecks a message. Ingress tenancy is admitted per carrier: `ControlRuntime.Adoption` is `TenantAdoption.Adopted` because `PeerAdmission` reads the connecting peer's kernel-reported uid and pid from the accepted socket.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ControlVerb {
    public static readonly ControlVerb SetDegradation = new("set-degradation", static _ => Option<string>.None);
    public static readonly ControlVerb DrainRuntime = new("drain-runtime", static reason => Some($"drain-runtime:{reason}"));

    [UseDelegateFromConstructor]
    public partial Option<string> Audit(string detail);
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct VerbReceipt(ControlVerb Verb, JsonElement Payload);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record ControlRuntime(
    DegradationCell Degradation,
    Func<Duration, IO<DrainReceipt>> Drain,
    ClockPolicy Clocks,
    CorrelationId Correlation,
    ActivitySource Source,
    SupportRuntime Support,
    FactSink<CompanionSignal> Fan,
    JsonSerializerOptions Wire) {
    public static readonly TenantAdoption Adoption = TenantAdoption.Adopted;
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
// `Target`, not `Both`: the response messages are DELIBERATELY narrower than the receipts they project, which the
// proto states at `SetDegradationResponse` — every target member is answered and an unread receipt column stays host-side.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class ControlReplyMap {
    public static partial SetDegradationResponse Reply(DegradationState state);

    public static partial DrainRuntimeResponse Reply(DrainReceipt receipt);

    // `~ExplicitCast` bars the silent enum cast, so the SmartEnum-to-wire projection is this ONE
    // user-implemented member Mapperly consumes for the `Level` column — the row's own `Wire` column
    // stays the map, and no ordinal arithmetic crosses the seam.
    static Control.DegradationLevel Wire(DegradationLevel level) => level.Wire;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ControlInbound {
    // The request's `Level` is the GENERATED wire enum, never a string key: `OfWire` is the roster-derived
    // inverse of the row's own `Wire` column, so `Unspecified` and any foreign ordinal fold to `None` and
    // `Force` re-derives rather than forcing a phantom level.
    public static IO<DegradationState> SetDegradation(ControlRuntime runtime, ServerCallContext context, Control.DegradationLevel level, string reason) =>
        Continued(runtime, context, ControlVerb.SetDegradation, reason, _ =>
            IO.lift(() => runtime.Degradation.Force(DegradationLevel.OfWire(level)))
                .Map(state => (Value: state, Payload: (object)state)));

    public static IO<DrainReceipt> DrainRuntime(ControlRuntime runtime, ServerCallContext context, Duration inherited, string reason) =>
        Continued(runtime, context, ControlVerb.DrainRuntime, reason, _ =>
            runtime.Drain(inherited)
                .Map(receipt => (Value: receipt, Payload: (object)receipt)));

    // The scope restores prior baggage on dispose, so the bracket is a statement body — a fold that continued
    // without disposing would leak the caller's tenancy into whatever ran next on the thread.
    // Correlation, audit, trace, fan, and fire happen HERE or nowhere.
    static IO<A> Continued<A>(
        ControlRuntime runtime, ServerCallContext context, ControlVerb verb, string detail,
        Func<CorrelationId, IO<(A Value, object Payload)>> fold) =>
        from correlation in IO.lift(() => runtime.Support.Active.Value.IfNone(Correlation.Mint))
        from _audit in verb.Audit(detail).Match(
            Some: reason => SupportCapture.Capture(runtime.Support, new SupportTrigger.Requested(correlation, SupportTriggerKind.ExternalCommand, reason)).Map(static _ => unit),
            None: () => IO.pure(unit))
        from settled in IO.liftAsync(async () => {
            using var scope = TraceContext.Continue(runtime.Source, context.RequestHeaders, verb.Key, ControlRuntime.Adoption);
            return await fold(correlation).RunAsync();
        })
        let receipt = new VerbReceipt(verb, JsonSerializer.SerializeToElement(settled.Payload, runtime.Wire))
        from _fan in runtime.Fan.Fan(correlation, verb.Key, receipt, new CompanionSignal.Verb(receipt))
        select settled.Value;

}

// --- [ENTRY] --------------------------------------------------------------------------------
public sealed class ControlServiceImpl(ControlRuntime runtime) : ControlService.ControlServiceBase {
    public override Task<SetDegradationResponse> SetDegradation(SetDegradationRequest request, ServerCallContext context) =>
        ControlInbound.SetDegradation(runtime, context, request.Level, request.Reason).Map(ControlReplyMap.Reply).RunAsync().AsTask();

    public override Task<DrainRuntimeResponse> DrainRuntime(DrainRuntimeRequest request, ServerCallContext context) =>
        ControlInbound.DrainRuntime(runtime, context, request.Cooperative.ToNodaDuration(), request.Reason).Map(ControlReplyMap.Reply).RunAsync().AsTask();
}

public sealed class CapabilityDiscoveryServiceImpl(
    CapabilityRegistry registry,
    DegradationCell degradation,
    DescriptorPin pin) : CapabilityContract.CapabilityDiscoveryService.CapabilityDiscoveryServiceBase {
    public override Task<CapabilityContract.DiscoverResponse> Discover(
        CapabilityContract.DiscoverRequest request,
        ServerCallContext context) =>
        Task.FromResult(CapabilityDiscovery.Project(registry, degradation.Level, pin));
}

// Contract admission surrounds the generated verb edge exactly once in each direction. It consumes the
// accumulated violation sequence directly; `WireAdmission.Admit` is intentionally not used because it collapses it.
public sealed class ControlContractInterceptor(ControlRuntime runtime) : Interceptor {
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation) {
        Op key = Op.Of(context.Method);
        TRequest admitted = Direction(request, context, key, request: true);
        TResponse response = await continuation(admitted, context).ConfigureAwait(false);
        return Direction(response, context, key, request: false);
    }

    private T Direction<T>(T message, ServerCallContext context, Op key, bool request)
        where T : class, Google.Protobuf.IMessage =>
        WireAdmission.Validate(message, key).Match(
            Succ: admission => admission.Match(
                Fail: violations => Refused(message, context, key, request, violations),
                Succ: static admitted => admitted),
            Fail: error => Broken<T>(context, key, error));

    private T Refused<T>(T message, ServerCallContext context, Op key, bool request,
        Seq<Google.Rpc.BadRequest.Types.FieldViolation> violations)
        where T : Google.Protobuf.IMessage {
        Error fault = request
            ? key.InvalidInput(message.Descriptor.FullName)
            : key.InvalidResult(message.Descriptor.FullName);
        throw FaultWire.Raise(fault, Context(context, violations));
    }

    private T Broken<T>(ServerCallContext context, Op key, Error error) {
        throw FaultWire.Raise(key.InvalidResult(error.Message), Context(context, Seq<Google.Rpc.BadRequest.Types.FieldViolation>()));
    }

    private FaultContext Context(ServerCallContext context, Seq<Google.Rpc.BadRequest.Types.FieldViolation> violations) {
        Baggage baggage = TraceContext.Extract(context.RequestHeaders).Baggage;
        CorrelationId correlation = Optional(baggage.GetBaggage(CorrelationId.Slot))
            .Bind(static text => Guid.TryParse(text, out Guid parsed) ? Some(CorrelationId.Create(parsed)) : None)
            .IfNone(runtime.Correlation);
        TenantContext tenant = ControlRuntime.Adoption.Adopt(baggage).IfNone(TenantContext.Root);
        return FaultContext.Of(correlation, (runtime.Clocks.Now, 0UL), tenant, violations);
    }
}
```

## [04]-[SERVICE_HOST]

- Owner: `ServiceHost` mounts generated control, capability-discovery, and health services, every supplied served plane, assets, and control intake; `ServedPlane` binds DI and endpoint arms.
- Cases: unix-domain-socket binds Kestrel over the `sun_path` endpoint, inherited-fd mounts Kestrel over a socket-activated descriptor the `HostBinding` owner acquired — the two local control-plane intake shapes on every supported platform.
- Entry: `CapabilityDiscoveryPlane` is the one generated discovery registration/mapping row; `Register` and `Map` fold it with supplied planes, control, and health.
- Auto: `WireAdmission.Warm` precedes the serving graph; control and discovery both run the contract interceptor, and discovery projects the current degradation-permitted catalog.
- Receipt: an acquisition mints one `BindReceipt` — service, rendered address, origin, reuse policy, listener count — fanned through the one `FactSink` and fired at `AppHostPoint.Companion`; the served `ServingStatus` transition logs through one `SpineLog` delegate inside the `FaultBand.SpineEvents` stride; no parallel host receipt.
- Packages: Grpc.AspNetCore.Server (`AddServiceOptions`/server interceptor pipeline), Grpc.AspNetCore.HealthChecks, Grpc.HealthCheck (transitive: `HealthServiceImpl`/`SetStatus`/`Grpc.Health.V1.ServingStatus`), Microsoft.AspNetCore.App (shared framework: `UseStaticFiles`/`StaticFileOptions`/`PhysicalFileProvider`), LanguageExt.Core, BCL inbox
- Growth: a new served service is one `ServedPlane` row carrying registration beside mapping; a new intake is one `ControlTransport` case.
- Boundary: a served plane arrives as a port and never as a named sibling type; one row binds both its registration and endpoint mapping. An empty row set serves control and health only. Contract warming is synchronous composition work, not the first request's work; the interceptor is scoped to `ControlServiceImpl`, because health and external planes carry package-owned generated messages outside `WireAdmission.Files`. The Unix leg reuses the `Discovery` `sun_path` law at the 104-byte cap and the inherited-fd leg consumes each activated listener through `ListenHandle`. `Grpc.HealthCheck.HealthServiceImpl` owns wire health; no diagnostic service is mounted.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlTransport {
    private ControlTransport() { }

    public sealed record UnixDomainSocket(string SocketPath) : ControlTransport;
    public sealed record InheritedHandle(SafeSocketHandle Handle) : ControlTransport;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct BindReceipt(string Service, string Address, BindOrigin Origin, ReusePolicy Reuse, int Listeners);

public sealed record ServedPlane(
    string Key,
    Func<IGrpcServerBuilder, IGrpcServerBuilder> Registration,
    Action<IEndpointRouteBuilder> Map);

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class ServiceHost {
    public static readonly ServedPlane CapabilityDiscoveryPlane = new(
        "capability-discovery",
        static grpc => grpc.AddServiceOptions<CapabilityDiscoveryServiceImpl>(
            static options => options.Interceptors.Add<ControlContractInterceptor>()),
        static endpoints => ignore(endpoints.MapGrpcService<CapabilityDiscoveryServiceImpl>()));

    public static IServiceCollection Register(IServiceCollection services, params ReadOnlySpan<ServedPlane> planes) {
        ignore(WireAdmission.Warm());
        IGrpcServerBuilder grpc = services.AddGrpc()
            .AddServiceOptions<ControlServiceImpl>(static options => options.Interceptors.Add<ControlContractInterceptor>());
        return Iterable<ServedPlane>.FromSpan(planes).ToSeq()
            .Fold(grpc, static (builder, plane) => plane.Registration(builder))
            .Services
            .AddGrpcHealthChecks().Services
            .AddSingleton(static _ => new HealthServiceImpl());
    }

    public static void Map(IEndpointRouteBuilder endpoints, params ReadOnlySpan<ServedPlane> planes) {
        ignore(endpoints.MapGrpcService<ControlServiceImpl>());
        endpoints.MapGrpcHealthChecksService();
        Iterable<ServedPlane>.FromSpan(planes).ToSeq().Iter(plane => plane.Map(endpoints));
    }

    public static IApplicationBuilder Assets(IApplicationBuilder app, ResolvedProfile resolved, string bundleRoot) =>
        resolved.Holds(HostCapability.CoHostedAssets)
            ? app.UseStaticFiles(new StaticFileOptions {
                FileProvider = new PhysicalFileProvider(bundleRoot),
                RequestPath = PathString.Empty,
            })
            : app;

    public static Unit Serving(HealthServiceImpl health, string service, HealthCheckResponse.Types.ServingStatus status) =>
        (health.SetStatus(service, status), unit).Item2;

    public static Unit Bind(KestrelServerOptions kestrel, ControlTransport transport) => transport.Switch(
        unixDomainSocket: uds => (kestrel.ListenUnixSocket(uds.SocketPath), unit).Item2,
        inheritedHandle: inherited => (kestrel.ListenHandle((ulong)inherited.Handle.DangerousGetHandle()), unit).Item2);

    // Every acquired descriptor mounts: a launchd `Sockets` entry with no `SockFamily` hands back one listener
    // per family and a systemd pair exists only under `BindIPv6Only=ipv6-only`, so the fold is count-driven and
    // binding the first alone leaves a sibling open, unlistened, and undiagnosed. `ListenHandle` ADOPTS without
    // taking the close, so the `SafeSocketHandle` stays the owning side and `Release` is the one close.
    // Dispatch is the union's OWN total `Switch`: the `var other` catch-all it replaces swallowed every address
    // kind but one, so the Growth claim that a fourth `BindAddress` case is a compile break held nowhere.
    public static Fin<Unit> BindEndpoint(KestrelServerOptions kestrel, BoundEndpoint endpoint) =>
        endpoint.Listeners.IsEmpty
            ? endpoint.Address.Switch(
                unixPath: unix => unix.SocketPath.Length > 0
                    ? Fin.Succ(Bind(kestrel, new ControlTransport.UnixDomainSocket(unix.SocketPath)))
                    : Fin.Fail<Unit>(new CompanionFault.Bind($"{endpoint.Service}: fresh unix endpoint carries an empty sun_path")),
                loopbackTcp: tcp => Fin.Fail<Unit>(new CompanionFault.Bind(string.Create(
                    CultureInfo.InvariantCulture, $"{endpoint.Service}: listenerless loopback-tcp endpoint on port {tcp.Port} cannot mount"))),
                inheritedFd: inherited => Fin.Fail<Unit>(new CompanionFault.Bind(string.Create(
                    CultureInfo.InvariantCulture, $"{endpoint.Service}: listenerless inherited-fd endpoint naming {inherited.Handles.Count} handles cannot mount"))))
            : Fin.Succ(endpoint.Listeners.Fold(unit, (_, listener) =>
                Bind(kestrel, new ControlTransport.InheritedHandle(listener.SafeHandle))));
}
```

## [05]-[DEGRADATION_CASCADE]

- Owner: `DegradationCascade` static write surface threading a parent-observed level onto the child `DegradationCell.Cascade` floor over the control hop; `CascadeReceipt` the cascade-decision projection.
- Entry: `Cascade(PeerRoster roster, CompanionPeer peer, DegradationLevel level, string reason, ModalityRow row)` returns `IO<CascadeReceipt>` — the parent forwards its own effective level to the child over the control hop on rows admitting `Degrade`, fans the decision, and fires it.
- Auto: the cascade rides the existing `degraded` lifecycle trigger receipt — no new instrument; the child re-derives on parent release because `DegradationCell.Cascade(None)` withdraws the floor and the existing `Derive` fold reclaims control; the floor never escalates below local pressure because `DegradationState.Floor` keeps the worse of the cascaded and derived ranks; the forwarding call carries `TraceContext.Inject(new Metadata())` as its headers so the child's cascade span descends from the parent's, the client half of the pair `CONTROL_SERVICE`'s `Continued` bracket closes.
- Receipt: `CascadeReceipt` carries the source level, the child pid, and the `Option<DegradationLevel>` the child acknowledged over the wire reply — the parent never fabricates the child's `DegradationState`, because the child's real state is owned by the child cell and only the acknowledged level crosses the contract.
- Packages: LanguageExt.Core, NodaTime, Grpc.Core.Api, BCL inbox
- Growth: a new cascade trigger is one call site over the existing `Cascade` fold; zero new surface — the parent-to-child cascade is a WRITE consumer of `DegradationCell.Cascade`, never a second `DegradationLevel` or `DegradationCell` owner.
- Boundary: only a row admitting `ModalityCapability.Degrade` cascades, so a sidecar never floors its externally-supervised peer; the parent forwards its own `DegradationCell.Level` value as data to the child over the control hop, so the level value READ stays the parent's degradation owner and the floor WRITE lands on the child cell through `Cascade`, never the operator `Force` the set-degradation verb owns — the seam-split owner on `Observability/health#DEGRADATION_RAIL` keeps the level vocabulary, the `Derive` fold, and the `Cascade` floor admit; the child admits the cascaded wire enum through the same `DegradationLevel.OfWire` admission the wire verb uses so an unknown ordinal never floors the cell; NAMED LOSS — none: the child-side `Apply(cell, parent)` member DELETES because it forwarded verbatim to `DegradationCell.Cascade` and resolved no name in one hop, so the child's inbound leg calls that owner directly.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct CascadeReceipt(
    DegradationLevel Source,
    int ChildPid,
    Option<DegradationLevel> Acknowledged);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class DegradationCascade {
    public static IO<CascadeReceipt> Cascade(
        PeerRoster roster, CompanionPeer peer, DegradationLevel level, string reason, ModalityRow row) =>
        (row.Capabilities.Admits(ModalityCapability.Degrade)
            ? Forward(peer, level, reason)
            : IO.pure(Option<DegradationLevel>.None))
        .Map(acked => new CascadeReceipt(level, peer.Manifest.Pid, acked))
        .Bind(receipt => roster.Fan.Fan(Correlation.Mint(), nameof(DegradationCascade), receipt, new CompanionSignal.Cascade(receipt)));

    // The client half of the local-ipc propagation pair: an uninjected call makes the child's span a fresh root
    // and severs the multi-process trace at the one hop the propagation composite names.
    static IO<Option<DegradationLevel>> Forward(CompanionPeer peer, DegradationLevel level, string reason) =>
        IO.liftAsync(async () => {
            var client = new ControlService.ControlServiceClient(peer.Control);
            SetDegradationResponse reply = await client.SetDegradationAsync(
                new SetDegradationRequest { Level = level.Wire, Reason = reason },
                TraceContext.Inject(new Metadata()));
            return DegradationLevel.OfWire(reply.Level);
        });
}
```

## [06]-[PEER_ADMISSION]

- Owner: `PeerAdmission` static accept-side credential read over the managed `Socket.GetRawSocketOption` route; `PeerCredential` the resolved uid-pid record; `Ucred` and `Xucred` the blittable platform-shaped credential structs read into a stack span.
- Cases: linux reads `SO_PEERCRED` at `SOL_SOCKET` into a 12-byte `ucred`, macos reads `LOCAL_PEERCRED` at `SOL_LOCAL` into a 76-byte `xucred` then a second `LOCAL_PEERPID` read at `SOL_LOCAL` for the 4-byte peer pid — the platform branch selects the level, option name, struct width, and pid-read count at the single accept seam.
- Entry: `Read(Socket accepted)` returns `Fin<PeerCredential>` — `Socket.GetRawSocketOption(level, name, span)` fills the platform struct off the connected socket and the read folds to the connecting peer's uid and pid, aborting when the returned count is fewer bytes than the struct width or the macOS `cr_version` word is non-zero; a kernel `getsockopt` failure surfaces as a `SocketException` the `Try` rail traps into `CompanionFault.Credential` carrying the `SocketException.SocketErrorCode`/`NativeErrorCode`, never an escaping exception.
- Auto: the credential read targets a stack `Span<byte>` sized to the platform struct, the macOS pid arrives from a separate `LOCAL_PEERPID` read into a 4-byte span because `xucred` carries no pid field, and the Linux `ucred` carries pid, uid, and gid in one 12-byte read; the returned byte count is the filled-length proof the read compares against the declared struct width before reinterpreting the bytes through `MemoryMarshal.Read`; because `GetRawSocketOption` is the managed seam it raises `SocketException` rather than setting the P/Invoke last error, so the errno is read from `SocketException.SocketErrorCode`/`NativeErrorCode` on the trapped error, never from a stale `Marshal.GetLastPInvokeError()` after a managed call.
- Receipt: `PeerCredential` carries the uid and pid the admission row trusts — read once at accept off the connected socket, never trusted from the manifest.
- Packages: LanguageExt.Core, BCL inbox
- Growth: a new platform is one branch on `Read` with one struct width and one credential layout; zero new surface.
- Boundary: the read is `Socket.GetRawSocketOption(int level, int optionName, Span<byte> optionValue)` returning the kernel-filled byte count — the raw `getsockopt` P/Invoke and the managed `Socket.GetSocketOption` path are both rejected, the former because the BCL already owns the raw-option seam over the safe handle and the latter because the PAL carries no `SocketOptionLevel.Local`, no `SO_PEERCRED`/`LOCAL_PEERCRED` translation, and `SocketOptionName.BlockSource=17` shares the integer with Linux `SO_PEERCRED=17` only by coincidence; Linux `SOL_SOCKET=1`/`SO_PEERCRED=17` fills `ucred{pid,uid,gid}` 12 bytes captured at connect time so a later exec cannot launder identity, macOS `SOL_LOCAL=0`/`LOCAL_PEERCRED=1` fills `xucred{cr_version,cr_uid,cr_ngroups,cr_groups[16]}` 76 bytes with `cr_version` mandated to equal `XUCRED_VERSION=0` and `SOL_LOCAL=0`/`LOCAL_PEERPID=2` reads the 4-byte peer pid `xucred` omits; the accepted-socket credential read is the admission row the `Discovery` manifest read defers to, so a connecting peer's identity is the kernel-reported value, never the manifest's self-asserted pid, and `PeerRoster.Admit` keys the entry on this `PeerCredential.Pid`; the credential faults are the INBOUND band's own — they name a serving-side admission refusal, and reporting them on the outbound hop band made an unreadable peer identity indistinguishable from a failed dial at every reader keying on the code; the peer leg this read gates is `python:runtime/transport/serve#SERVE`, whose UDS serve row admits `insecure_loopback` alone precisely because identity arrives here through `SO_PEERCRED`/`LOCAL_PEERCRED` rather than a wire-carried PEM — so the two ends name one credential source and neither seats a second.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[StructLayout(LayoutKind.Sequential)]
public readonly struct Ucred {
    public readonly int Pid;
    public readonly uint Uid;
    public readonly uint Gid;
}

[StructLayout(LayoutKind.Sequential, Size = 76)]
public readonly struct Xucred {
    public readonly uint Version;
    public readonly uint Uid;
    public readonly short Ngroups;
}

public readonly record struct PeerCredential(int Pid, uint Uid);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PeerAdmission {
    public const int SolSocketLinux = 1;
    public const int SoPeerCred = 17;
    public const int SolLocalMacos = 0;
    public const int LocalPeerCred = 1;
    public const int LocalPeerPid = 2;
    public const uint XucredVersion = 0;
    public const int UcredSize = 12;
    public const int XucredSize = 76;
    public const int PidSize = 4;

    public static Fin<PeerCredential> Read(Socket accepted) =>
        OperatingSystem.IsLinux() ? ReadLinux(accepted)
        : OperatingSystem.IsMacOS() ? ReadDarwin(accepted)
        : Fin.Fail<PeerCredential>(new CompanionFault.Excluded("peer-credential unavailable on this platform"));

    static Fin<PeerCredential> ReadLinux(Socket accepted) =>
        Op.Of().Catch(() => {
            Span<byte> buffer = stackalloc byte[UcredSize];
            return Fin.Succ(accepted.GetRawSocketOption(SolSocketLinux, SoPeerCred, buffer) >= UcredSize
                ? Optional(MemoryMarshal.Read<Ucred>(buffer))
                : None);
        })
            .MapFail(static error => (Error)CompanionFault.Of(error))
            .Bind(read => read.Match(
                cred => Fin.Succ(new PeerCredential(cred.Pid, cred.Uid)),
                () => Fin.Fail<PeerCredential>(new CompanionFault.Credential("SO_PEERCRED short read"))));

    static Fin<PeerCredential> ReadDarwin(Socket accepted) =>
        Op.Of().Catch(() => {
            Span<byte> credBuffer = stackalloc byte[XucredSize];
            Span<byte> pidBuffer = stackalloc byte[PidSize];
            return Fin.Succ(accepted.GetRawSocketOption(SolLocalMacos, LocalPeerCred, credBuffer) >= XucredSize
                && MemoryMarshal.Read<Xucred>(credBuffer) is var cred && cred.Version == XucredVersion
                && accepted.GetRawSocketOption(SolLocalMacos, LocalPeerPid, pidBuffer) >= PidSize
                    ? Optional(new PeerCredential(BinaryPrimitives.ReadInt32LittleEndian(pidBuffer), cred.Uid))
                    : None);
        })
            .MapFail(static error => (Error)CompanionFault.Of(error))
            .Bind(read => read.ToFin(new CompanionFault.Credential("LOCAL_PEERCRED/LOCAL_PEERPID short read or version mismatch")));

}
```

## [07]-[HOST_BINDING]

- Owner: `HostBinding` static acquisition surface folding the OS, the activation source, and the address shape into one serving-endpoint claim binding through `ServiceHost.Bind`; `HostOs` `[SmartEnum<string>]` and `AddressKind` `[SmartEnum<string>]` the two axes the policy key needs beside the source; `BindAddress` `[Union]` the three address shapes; `BindOrigin` `[SmartEnum]` the three provenance cases; `ActivationSource` `[SmartEnum<string>]` the three socket-activation rows, each binding its own inheritance arm as a delegate column; `ReusePolicy` `[SmartEnum<string>]` the port-reuse semantics axis; `PortOverride` the explicit-port value record; `BindRequest` the acquisition input; `BoundEndpoint` the resolved listener artifact; `HostBindPolicy` the per-row policy record carrying its own key triple; `HostBindRows` the frozen keyed table; the boundary [LibraryImport]/env adapters `SystemdActivation`, `LaunchdActivation`, `SecretAcquisition`, and `ReusePort`.
- Cases: three address shapes — unix-path for the credential-gated control plane, loopback-tcp for a host without a UDS budget, inherited-fd for a socket-activated listener; three provenance cases — fresh on a self-bound socket, inherited on a manager-passed fd, reclaimed on a stale-file takeover; three activation sources — systemd-socket reads the `LISTEN_FDS` env protocol, launchd-socket calls `launch_activate_socket`, fresh-bind inherits nothing; three reuse policies — load-balance on Linux `SO_REUSEPORT`, last-wins on macOS `SO_REUSEPORT`, none where reuse is rejected; twelve policy rows over the `(HostOs, ActivationSource, AddressKind)` cross-product each platform admits, so a Linux row asking for launchd activation is an unrostered key that REFUSES rather than a ternary's fall-through.
- Entry: `Acquire(BindRequest request, FactSink<CompanionSignal> fan)` returns `IO<Fin<BoundEndpoint>>` — resolves the policy row by its key triple, runs the source's own inheritance arm, and settles the acquisition on the descriptors it returned or falls to a fresh bind when it returned none, applying the `ReusePolicy` through `ReusePort.Apply` on each held socket before bind; `Release(BoundEndpoint endpoint)` returns `IO<Unit>` unlinking a fresh-bound or reclaimed unix path and disposing every held socket exactly once, never an accepted socket.
- Auto: the two activation platforms INVERT each other on descriptor count and neither adapter is written to expect the other's shape, so each rides its OWN row's arm rather than a source comparison at the call site. A systemd row consumes `LISTEN_FDS` directly — no libsystemd binding — checking `$LISTEN_PID` equals `Environment.ProcessId`, taking the count off `LISTEN_FDS` (never an assumed family pair), adopting every fd of the named service's contiguous run from `SD_LISTEN_FDS_START=3`, and self-setting `FD_CLOEXEC` through `fcntl` on each because systemd passes them without the flag; a bare `ListenStream=<port>` yields ONE dual-mode `AF_INET6` descriptor serving IPv4 as `::ffff:*` under the default `net.ipv6.bindv6only=0`, and a second descriptor exists ONLY where the unit declares `BindIPv6Only=ipv6-only`; `$LISTEN_FDNAMES` REPEATS the unit name once per fd rather than naming each distinctly, so a name lookup returns the run's FIRST index and never disambiguates within it, and POSITION is the only selector — the adapter reads the name only to find and skip a foreign unit's block, then takes the whole matching span, answering an OPTION rather than a `(-1, 0)` sentinel a caller could arithmetic on; a launchd row calls `launch_activate_socket(name, &fds, &cnt)` and adopts EVERY descriptor the count reports before freeing the array through `free` — a `Sockets` entry declaring no `SockFamily` yields one listener per family, so taking `fds[0]` alone leaves its sibling open, unlistened, and undiagnosed, and each adopted descriptor's family is read off the `Socket` it opens rather than assumed from its array position; the call RETURNS its errno as the `int` result and never sets the errno global, so the mapping reads the return value alone and `EALREADY=37`, `ESRCH=3`, and `ENOENT=2` are separate typed cases routing to separate repairs; the set is captured once at composition-root startup and threaded, never re-derived per listener, and `free(fds)` is owed on the success arm alone because both failure arms answer NULL; an inherited row carries each activated descriptor as a held `Socket` Kestrel adopts through `ListenHandle`, a fresh loopback-tcp row binds and listens the held socket with `SO_REUSEPORT` applied before bind, and a fresh unix-path row holds no socket — it defers the `ListenUnixSocket` bind to `ServiceHost.Bind` at the `Discovery.SocketPath` `sun_path`, and a bind onto an existing path probes it first: a live peer answers and the acquisition refuses, a dead file unlinks and re-binds as `Reclaimed`, which is the bind-failure-is-mutex law spelled as a fold; `SO_REUSEPORT` applies through `ReusePort.Apply` over `Socket.SetRawSocketOption` so the Linux load-balance and macOS last-wins kernel behaviors are one option write whose semantic divergence is the `ReusePolicy` row's documented evidence, never a code branch.
- Receipt: `BoundEndpoint` carries the bound `BindAddress`, the `BindOrigin`, the `ReusePolicy`, and the held `Seq<Socket>` listeners — one entry per fresh-tcp socket, one per activated descriptor, and empty for a unix path Kestrel binds and the drain unlinks; the acquisition itself fans one `BindReceipt` at `SERVICE_HOST`; readiness notify stays the `SystemdNotifier` mirror and the SIGTERM/SIGQUIT/SIGHUP traps stay `FaultSpine.ArmTraps`.
- Packages: Microsoft.Extensions.Hosting.Systemd, Rasm (kernel `CapabilitySet`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new OS or activation source is one `HostOs`/`ActivationSource` row carrying its own inheritance arm beside the policy rows its platform admits; a new address shape is one `BindAddress` case and one `AddressKind` row breaking every dispatch at compile time; a new reuse semantic is one `ReusePolicy` row; the macOS secret-acquisition route is one `SecretAcquisition` adapter call, never a child-process credential surface; zero new surface.
- Boundary: THE POLICY KEY IS THE ROW'S OWN — the prior form re-derived the key inside a nested ternary that asked `is BindAddress.LoopbackTcp` four times and read `OperatingSystem.IsMacOS()` inside one arm of it, so the table's key existed only in that expression and a row could never be added without editing it; keying on `(HostOs, ActivationSource, AddressKind)` makes an unrostered combination a REFUSAL (Linux with launchd activation is exactly that) where the ternary answered a neighboring row; DISPATCH IS THE ROW'S OWN TOO — `Acquire` compared `request.Source` against two of the three rows over a `[SmartEnum]` that generates a total `Switch`, so a fourth source silently took the fresh-bind arm; the inheritance arm now rides the row and fresh-bind's arm answers an empty descriptor set, which is what selects the fresh path; `ProfileRoots` LEAVES the signature — it was never read and the activation-name lookup it claimed to scope is `BindRequest.ActivationName`; the host-binding owner resides beside `SERVICE_HOST` because `ServiceHost.Bind`/`KestrelServerOptions.ListenUnixSocket` is the listener seam it binds through — `host-profiles` owns profile variance and never the bind() call; `Microsoft.Extensions.Hosting.Systemd` carries the `SystemdNotifier` readiness mirror but no socket-activation fd intake, so `SystemdActivation` reads the listen protocol directly with no libsystemd P/Invoke — through the `Runtime/profiles#LIFETIME_ADAPTERS` `BootVariable` roster, the one owner of a coordinate resolved before any configuration source mounts, so the three handoff variables sit beside the watchdog pair rather than as bare reads at this boundary; there is no `Microsoft.Extensions.Hosting.Launchd` package, so `LaunchdActivation` is a `[LibraryImport("/usr/lib/libSystem.B.dylib")]` adapter over `launch_activate_socket(3)` whose `int**` out-parameter is a heap array of `getaddrinfo(3)`-derived descriptors the caller adopts WHOLE and whose `size_t*` count is the discriminant, with one `free(3)` release the man page mandates — the import carries no `SetLastError` because the call's own return value IS the errno, and the descriptors copy out of the array in ONE span read before the free rather than accumulating through a quadratic append; `SafeSocketHandle(nint preexistingHandle, bool ownsHandle)` is the adoption ctor — the `int` fd widens implicitly and the parameter is `nint`, so a fence spelling `(int, bool)` names a member that does not exist; descriptor OWNERSHIP settles at the `SafeSocketHandle` alone — `KestrelServerOptions.ListenHandle(ulong)` adopts the descriptor for listening and never takes the close, so `Release` disposing each held handle is the one close; the macOS secret-acquisition route is an in-process `Security.framework` `[LibraryImport]` over `SecItemCopyMatching`/`SecItemAdd` for parity with the launchd adapter, avoiding a child-process credential surface, and returns an exact kernel refusal which the `Runtime/secrets#SECRET_LEASE` owner wraps only after redacting the key id — never a second credential-fault owner and never this page's own band; its live execution triggers an OS keychain dialog and stays a tier-3 live-host residual the headless session never invokes; the abstract-unix namespace lands on Linux and refuses on macOS because no directory mode gates it, riding the policy row's own column, never a fourth address case; `NOTIFY_SOCKET` exists only on systemd so a launchd or fresh-bind row carries no readiness notify.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HostOs {
    public static readonly HostOs Linux = new("linux");
    public static readonly HostOs Macos = new("macos");

    public static Fin<HostOs> Current =>
        OperatingSystem.IsLinux() ? Fin.Succ(Linux)
        : OperatingSystem.IsMacOS() ? Fin.Succ(Macos)
        : Fin.Fail<HostOs>(new CompanionFault.Excluded("host binding unavailable on this platform"));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AddressKind {
    public static readonly AddressKind UnixPath = new("unix-path");
    public static readonly AddressKind LoopbackTcp = new("loopback-tcp");
    public static readonly AddressKind InheritedFd = new("inherited-fd");

    public static AddressKind Of(BindAddress address) => address.Switch(
        unixPath: static _ => UnixPath,
        loopbackTcp: static _ => LoopbackTcp,
        inheritedFd: static _ => InheritedFd);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BindAddress {
    private BindAddress() { }

    public sealed record UnixPath(string SocketPath, bool AbstractAllowed) : BindAddress;
    public sealed record LoopbackTcp(int Port) : BindAddress;

    public sealed record InheritedFd(Seq<int> Handles) : BindAddress;
}

[SmartEnum]
public sealed partial class BindOrigin {
    public static readonly BindOrigin Fresh = new();
    public static readonly BindOrigin Inherited = new();
    public static readonly BindOrigin Reclaimed = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ActivationSource {
    public static readonly ActivationSource SystemdSocket = new("systemd-socket", inherit: SystemdActivation.Inherit);
    public static readonly ActivationSource LaunchdSocket = new("launchd-socket", inherit: LaunchdActivation.Inherit);
    public static readonly ActivationSource FreshBind = new("fresh-bind", inherit: static _ => Fin.Succ(Seq<SafeSocketHandle>()));

    [UseDelegateFromConstructor]
    public partial Fin<Seq<SafeSocketHandle>> Inherit(string activationName);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ReusePolicy {
    public static readonly ReusePolicy LoadBalance = new("load-balance");
    public static readonly ReusePolicy LastWins = new("last-wins");
    public static readonly ReusePolicy None = new("none");
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PortOverride(Option<int> Port) {
    public static readonly PortOverride Unset = new(None);
}

public sealed record BindRequest(
    string Service,
    BindAddress Address,
    ActivationSource Source,
    PortOverride Override,
    string ActivationName);

public sealed record BoundEndpoint(
    string Service,
    BindAddress Address,
    BindOrigin Origin,
    ReusePolicy Reuse,
    Seq<Socket> Listeners);

// --- [POLICIES] -----------------------------------------------------------------------------
public sealed record HostBindPolicy(
    HostOs Os,
    ActivationSource Source,
    AddressKind Address,
    ReusePolicy Reuse,
    bool AbstractUnixAllowed,
    bool ReadinessNotify);

public static class HostBindRows {
    public static readonly Seq<HostBindPolicy> Items = Seq(
        new HostBindPolicy(HostOs.Linux, ActivationSource.SystemdSocket, AddressKind.UnixPath, ReusePolicy.None, true, true),
        new HostBindPolicy(HostOs.Linux, ActivationSource.SystemdSocket, AddressKind.LoopbackTcp, ReusePolicy.LoadBalance, true, true),
        new HostBindPolicy(HostOs.Linux, ActivationSource.SystemdSocket, AddressKind.InheritedFd, ReusePolicy.None, true, true),
        new HostBindPolicy(HostOs.Linux, ActivationSource.FreshBind, AddressKind.UnixPath, ReusePolicy.None, true, false),
        new HostBindPolicy(HostOs.Linux, ActivationSource.FreshBind, AddressKind.LoopbackTcp, ReusePolicy.LoadBalance, true, false),
        new HostBindPolicy(HostOs.Linux, ActivationSource.FreshBind, AddressKind.InheritedFd, ReusePolicy.None, true, false),
        new HostBindPolicy(HostOs.Macos, ActivationSource.LaunchdSocket, AddressKind.UnixPath, ReusePolicy.None, false, false),
        new HostBindPolicy(HostOs.Macos, ActivationSource.LaunchdSocket, AddressKind.LoopbackTcp, ReusePolicy.LastWins, false, false),
        new HostBindPolicy(HostOs.Macos, ActivationSource.LaunchdSocket, AddressKind.InheritedFd, ReusePolicy.None, false, false),
        new HostBindPolicy(HostOs.Macos, ActivationSource.FreshBind, AddressKind.UnixPath, ReusePolicy.None, false, false),
        new HostBindPolicy(HostOs.Macos, ActivationSource.FreshBind, AddressKind.LoopbackTcp, ReusePolicy.LastWins, false, false),
        new HostBindPolicy(HostOs.Macos, ActivationSource.FreshBind, AddressKind.InheritedFd, ReusePolicy.None, false, false));

    private static readonly Lazy<FrozenDictionary<(HostOs Os, ActivationSource Source, AddressKind Address), HostBindPolicy>> Index =
        new(static () => Items.ToFrozenDictionary(static row => (row.Os, row.Source, row.Address)),
            LazyThreadSafetyMode.ExecutionAndPublication);

    public static Fin<HostBindPolicy> Of(BindRequest request) =>
        HostOs.Current.Bind(os => Seated(os, request.Source, AddressKind.Of(request.Address)));

    static Fin<HostBindPolicy> Seated(HostOs os, ActivationSource source, AddressKind address) =>
        Index.Value.TryGetValue((os, source, address), out HostBindPolicy? row)
            ? Fin.Succ(row)
            : Fin.Fail<HostBindPolicy>(new CompanionFault.Bind($"{os.Key}:{source.Key}:{address.Key}"));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostBinding {
    // The acquisition is the transition, so it fans here: a `BoundEndpoint` that reached Kestrel with no receipt
    // left the one host-binding fact readable nowhere but a debugger.
    public static IO<Fin<BoundEndpoint>> Acquire(BindRequest request, FactSink<CompanionSignal> fan) =>
        Bound(request).Match(
            Succ: endpoint => Fanned(fan, endpoint).Map(Fin.Succ),
            Fail: error => IO.pure(Fin.Fail<BoundEndpoint>(error)));

    static Fin<BoundEndpoint> Bound(BindRequest request) =>
        from row in HostBindRows.Of(request)
        from handles in request.Source.Inherit(request.ActivationName)
        from bound in handles.IsEmpty ? FreshBind(request, row) : Fin.Succ(Settle(request, row, handles))
        select bound;

    static IO<BoundEndpoint> Fanned(FactSink<CompanionSignal> fan, BoundEndpoint endpoint) =>
        IO.lift(() => new BindReceipt(
                endpoint.Service, Rendered(endpoint.Address), endpoint.Origin, endpoint.Reuse, endpoint.Listeners.Count))
            .Bind(receipt => fan.Fan(Correlation.Mint(), nameof(HostBinding), receipt, new CompanionSignal.Bound(receipt)))
            .Map(_ => endpoint);

    static string Rendered(BindAddress address) => address.Switch(
        unixPath: static unix => unix.SocketPath,
        loopbackTcp: static tcp => string.Create(CultureInfo.InvariantCulture, $"127.0.0.1:{tcp.Port}"),
        inheritedFd: static inherited => string.Create(CultureInfo.InvariantCulture, $"fd:{inherited.Handles.Count}"));

    // A fresh AND a reclaimed unix path both own their file, so both unlink; every held socket disposes once,
    // which is also the one close of the descriptor `ListenHandle` adopted for listening.
    public static IO<Unit> Release(BoundEndpoint endpoint) =>
        IO.lift(() => {
            if (endpoint.Origin != BindOrigin.Inherited && endpoint.Address is BindAddress.UnixPath { AbstractAllowed: false } unix && File.Exists(unix.SocketPath)) {
                File.Delete(unix.SocketPath);
            }
            endpoint.Listeners.Iter(static listener => listener.Dispose());
            return unit;
        });

    static Fin<BoundEndpoint> FreshBind(BindRequest request, HostBindPolicy row) => request.Address.Switch(
        unixPath: unix => Reclaim(request, unix).Map(origin =>
            new BoundEndpoint(request.Service, unix, origin, row.Reuse, Seq<Socket>())),
        loopbackTcp: tcp => Op.Of().Catch(() => {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReusePort.Apply(socket, row.Reuse);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, request.Override.Port.IfNone(tcp.Port)));
            socket.Listen();
            return Fin.Succ(socket);
        }).MapFail(static error => (Error)CompanionFault.Of(error))
            .Map(socket => new BoundEndpoint(request.Service, tcp, BindOrigin.Fresh, row.Reuse, [socket])),
        inheritedFd: inherited => Fin.Fail<BoundEndpoint>(
            new CompanionFault.Bind($"inherited-fd-not-fresh:{inherited.Handles.Count}")));

    // Bind-failure-is-mutex, stated where the origin is stamped: an absent file is Fresh, a file a live peer
    // answers on is a held mutex the acquisition refuses, and a file nothing answers on unlinks as Reclaimed.
    // A blind unlink would evict a serving peer.
    static Fin<BindOrigin> Reclaim(BindRequest request, BindAddress.UnixPath unix) =>
        !File.Exists(unix.SocketPath)
            ? Fin.Succ(BindOrigin.Fresh)
            : Op.Of().Catch(() => {
                using var probe = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                probe.Connect(new UnixDomainSocketEndPoint(unix.SocketPath));
                return Fin.Succ(true);
            }).Match(
                Succ: _ => Fin.Fail<BindOrigin>(new CompanionFault.Held(request.Service, unix.SocketPath)),
                Fail: _ => Op.Of().Catch(() => { File.Delete(unix.SocketPath); return Fin.Succ(BindOrigin.Reclaimed); })
                    .MapFail(static error => (Error)CompanionFault.Of(error)));

    // `Socket(SafeSocketHandle)` loads the descriptor's real family off the kernel, so each listener is TAGGED
    // by what it is rather than by its array position; descriptor numbers repeat across activations and key nothing.
    static BoundEndpoint Settle(BindRequest request, HostBindPolicy row, Seq<SafeSocketHandle> handles) =>
        new(request.Service,
            new BindAddress.InheritedFd(handles.Map(static handle => (int)handle.DangerousGetHandle())),
            BindOrigin.Inherited,
            row.Reuse,
            handles.Map(static handle => new Socket(handle)));
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
public static partial class SystemdActivation {
    public const int ListenFdsStart = 3;

    // `$LISTEN_FDNAMES` repeats the unit name once per fd, so a name lookup returns the run's FIRST index and
    // never disambiguates within it — the service's fds are the maximal contiguous name-matching span and every
    // one adopts, which serves the count-one norm and the explicit `BindIPv6Only=ipv6-only` pair alike.
    public static Fin<Seq<SafeSocketHandle>> Inherit(string activationName) =>
        int.TryParse(BootVariable.ListenOwner.Read().IfNone(string.Empty), CultureInfo.InvariantCulture, out int pid) && pid == Environment.ProcessId
        && int.TryParse(BootVariable.ListenCount.Read().IfNone(string.Empty), CultureInfo.InvariantCulture, out int count) && count >= 1
            ? NameRun(BootVariable.ListenNames.Read(), activationName, count).Match(
                Some: run => Op.Of().Catch(() => Fin.Succ(Range(ListenFdsStart + run.Offset, run.Length).Map(Cloexec).ToSeq().Strict()))
                    .MapFail(static error => (Error)CompanionFault.Of(error)),
                None: () => Fin.Fail<Seq<SafeSocketHandle>>(new CompanionFault.Activation($"no systemd fd run: {activationName}")))
            : Fin.Fail<Seq<SafeSocketHandle>>(new CompanionFault.Activation($"no systemd socket activation: {activationName}"));

    // A name matching nothing answers ABSENCE rather than a `(-1, 0)` pair a caller can still index with.
    static Option<(int Offset, int Length)> NameRun(Option<string> listenNames, string activationName, int count) =>
        string.IsNullOrEmpty(activationName)
            ? Some((0, count))
            : listenNames.Match(
                None: () => count == 1 ? Some((0, 1)) : Option<(int, int)>.None,
                Some: names => names.Split(':') is var rows && Array.IndexOf(rows, activationName) is var offset && offset >= 0
                    ? Some((offset, rows.Skip(offset).TakeWhile(name => string.Equals(name, activationName, StringComparison.Ordinal)).Count()))
                    : None);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int fcntl(int fd, int cmd, int arg);

    static SafeSocketHandle Cloexec(int fd) {
        const int FSetFd = 2;
        const int FdCloexec = 1;
        ignore(fcntl(fd, FSetFd, FdCloexec));
        return new SafeSocketHandle((nint)fd, ownsHandle: true);
    }
}

public static partial class LaunchdActivation {
    public const int ENoEnt = 2;
    public const int ESrch = 3;
    public const int EAlready = 37;

    // No `SetLastError`: this call RETURNS the errno as its int result and never sets the errno global, so
    // `Marshal.GetLastPInvokeError()` reads a stale unrelated value here. Zero is success; every other value
    // is the diagnosis itself.
    [LibraryImport("/usr/lib/libSystem.B.dylib", StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial int launch_activate_socket(string name, int** fds, nuint* count);

    [LibraryImport("/usr/lib/libSystem.B.dylib")]
    private static unsafe partial void free(void* ptr);

    // The out-parameter is a heap ARRAY the count sizes — one entry per `getaddrinfo(3)` result — so taking
    // `fds[0]` alone abandons its sibling open and undiagnosable, and emission ORDER is launchd's own detail.
    // The call is once-per-process: a repeat answers `EALREADY` with count 0 and a NULL out-pointer, so both
    // failure arms answer NULL and `free` is owed on the success arm alone. The descriptors copy out in ONE
    // span read, so the array releases before any managed handle is minted.
    public static unsafe Fin<Seq<SafeSocketHandle>> Inherit(string activationName) {
        int* fds = null;
        nuint count = 0;
        int status = launch_activate_socket(activationName, &fds, &count);
        if (status != 0 || count == 0 || fds is null) {
            return Fin.Fail<Seq<SafeSocketHandle>>(status switch {
                EAlready => new CompanionFault.Activation($"launch_activate_socket {activationName}: already activated in this process"),
                ESrch => new CompanionFault.Excluded($"launch_activate_socket {activationName}: no such socket entry"),
                ENoEnt => new CompanionFault.Excluded($"launch_activate_socket {activationName}: job holds no sockets"),
                _ => new CompanionFault.Activation($"launch_activate_socket {activationName}: errno {status}, count {count}"),
            });
        }
        int[] adopted = new ReadOnlySpan<int>(fds, (int)count).ToArray();
        free(fds);
        return Fin.Succ(toSeq(adopted).Map(static fd => new SafeSocketHandle((nint)fd, ownsHandle: true)).Strict());
    }
}

public static partial class SecretAcquisition {
    public const int ErrSecSuccess = 0;
    public const int ErrSecItemNotFound = -25300;

    [LibraryImport("/System/Library/Frameworks/Security.framework/Security", EntryPoint = "SecItemCopyMatching")]
    private static partial int SecItemCopyMatching(nint query, out nint result);

    [LibraryImport("/System/Library/Frameworks/Security.framework/Security", EntryPoint = "SecItemAdd")]
    private static partial int SecItemAdd(nint attributes, nint result);

    public static Fin<int> Probe(nint query) =>
        SecItemCopyMatching(query, out _) switch {
            ErrSecSuccess => Fin.Succ(ErrSecSuccess),
            ErrSecItemNotFound => Fin.Fail<int>(Op.Of().InvalidResult("keychain item absent")),
            var status => Fin.Fail<int>(Op.Of().InvalidResult($"SecItemCopyMatching status {status}")),
        };

    public static Fin<int> Store(nint attributes) =>
        SecItemAdd(attributes, nint.Zero) is var status && status == ErrSecSuccess
            ? Fin.Succ(status)
            : Fin.Fail<int>(Op.Of().InvalidResult($"SecItemAdd status {status}"));
}

public static class ReusePort {
    public const int SolSocketLinux = 1;
    public const int SoReusePortLinux = 15;
    public const int SolSocketMacos = 0xffff;
    public const int SoReusePortMacos = 0x0200;

    public static Unit Apply(Socket listener, ReusePolicy policy) =>
        policy == ReusePolicy.None
            ? unit
            : Set(listener, OperatingSystem.IsMacOS() ? SolSocketMacos : SolSocketLinux, OperatingSystem.IsMacOS() ? SoReusePortMacos : SoReusePortLinux);

    static Unit Set(Socket listener, int level, int name) {
        Span<byte> enable = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(enable, 1);
        listener.SetRawSocketOption(level, name, enable);
        return unit;
    }
}
```

```mermaid
stateDiagram-v2
    accTitle: Companion socket acquisition arms
    accDescr: A requested listener resolving its policy row by OS, source, and address kind, then running the source's own inheritance arm through systemd, launchd, or a fresh self-bind that reclaims a stale file under mutex, every arm converging on serve and releasing through drain unlink.
    [*] --> Requested
    Requested --> Rowed: (HostOs, ActivationSource, AddressKind)
    Rowed --> Systemd: LISTEN_FDS env
    Rowed --> Launchd: launch_activate_socket
    Rowed --> FreshBind: empty inheritance
    Systemd --> Inherited: fd >= 3, FD_CLOEXEC
    Launchd --> Inherited: every fd, one free
    FreshBind --> Fresh: bind + listen
    FreshBind --> Reclaimed: dead-file probe + unlink
    Inherited --> Serving: ServiceHost.Bind
    Fresh --> Serving: SO_REUSEPORT apply
    Reclaimed --> Serving: SO_REUSEPORT apply
    Serving --> Released: drain unlink
    Released --> [*]
```

## [08]-[EVENT_INGRESS]

- Owner: `WebhookOrigin` and `WebhookRate` admit the abuse-protection DNS expression and positive requests-per-minute value; `WebhookAllowance` renders a numeric grant or the standard `*` unlimited grant; `IngressBody` owns one bounded immutable capture whose exact bytes feed both integrity verification and event decoding; `EventSemconv` stamps only CloudEvents attributes at this HTTP door; `Delivery` carries the per-request tally and refusal causes; `EventIngress` consumes the identity owner, `PolicyDescriptor.WebhookDelivery`, source and classification trust gates, an optional integrity verifier, and `WireAdmission.EventExtensions`.
- Cases: required request origin plus optional positive request rate against allowed origin and an optional policy ceiling, whose absence means unlimited; every immediate consent carries `WebHook-Allowed-Rate`, bounded to the request when one exists and rendered as `*` only when neither request nor policy imposes a limit; token transport is exactly one `Authorization: Bearer` header or one `access_token` query value; JSON and Protobuf structured or batch media plus Avro structured media select their exact `EventFormat` row; every generated `Extensions` field is declared from the generated descriptor; five `cloudevents.*` attributes stamp each admitted delivery.
- Entry: `EventIngress.Validate(HttpRequest request, HttpResponse response, IngressPolicy policy)` handles the `OPTIONS` abuse-protection request and conveys consent only through grant headers. `EventIngress.Deliver(HttpRequest request, HttpResponse response, IngressPolicy policy, EventBus.Cell bus, Op key)` authenticates and authorizes the delivery, verifies the exact request body when the app supplied an integrity dialect, admits the generated extension message through `WireAdmission.EventExtensions.Admit`, applies the injected domain projection under the principal's roster-resolved tenant, deduplicates, and dispatches each admitted envelope through `EventBus.Dispatch`.
- Law: the CloudEvents ASP.NET package supplies no handshake, origin policy, or cross-format body custody. This boundary captures once under `IngressPolicy.BodyLimit`, refuses an empty body, assigns typed HTTP 415 evidence to absent or unsupported content media, verifies the immutable bytes when configured, and decodes those same bytes through the exact JSON, Protobuf, or Avro formatter row.
- Law: origin is a DNS name expression, never a URL; callback is the separate URL-shaped field and the synchronous response does not reinterpret either. Request rate is absent or a positive integer greater than zero. A configured policy rate is the ceiling; absence is unlimited, so a requested rate receives that rate or the lower ceiling and an unrequested unlimited grant renders `*`.
- Law: `WebHook-Request-Origin` rides EVERY delivery request, not the handshake alone, so a target re-reads the claimed origin per message rather than trusting one validation forever; an origin the policy no longer allows refuses at that message without unregistering the whole subscription.
- Law: the abuse handshake establishes no authentication context. Delivery admits exactly one token transport through `TokenValidation`, evaluates `PolicyDescriptor.WebhookDelivery` through `PolicyGate`, and uses the resulting `Principal.Tenant`; trace baggage never grants authority. An app-defined signature sees the exact received octets before CloudEvents parsing and adds integrity only — it never substitutes for token authorization.
- Law: source trust and `DataGrade` classification both admit before domain projection, dedup, or dispatch. Dedup then reads the envelope's own `(source, id)` composite through the one `Runtime/resources#DEDUPE_WINDOW`, so an admitted HTTP redelivery collapses before bus dispatch.
- Auto: batch and single share one `EventEnvelope.Decode` door whose parsed media chooses the exact formatter and framing. Every admitted envelope stamps `EventSemconv` before `EventBus.Dispatch`; the durable outbox relay remains an outbound hop over the exact Persistence envelope.
- Receipt: one `Delivery` per request carrying accepted, duplicate, and externalized counts beside the REFUSAL CAUSES themselves, fanned through the one `FactSink` and fired at `AppHostPoint.Companion` — a 4xx names the axis or claim that refused it rather than a bare count, which is what the receipt line promised while both refusal arms discarded the cause.
- Packages: Rasm.Contracts (generated `Extensions` message), CloudNative.CloudEvents, Microsoft.AspNetCore.App (shared framework), Rasm (the `Rasm/Domain/event` envelope algebra), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new generated extension field joins declaration and reconstructed whole-message admission through `WireAdmission.EventExtensions`; a new protobuf value space is one kernel structural-kind bridge, never a field-name row here; a new HTTP event format is one `EventFormat` row this door consumes unchanged; a foreign refusal needs no case at all, since `CompanionFault.Of` adopts it whole — never a second door.
- Boundary: `WireAdmission.EventExtensions` composes the kernel `EventExtensionContract<event.v1.Extensions>` over AppHost's one descriptor-root validator; a private per-handler validator is the deleted duplicate rule graph. `IngressPolicy.Project` receives the admitted message and typed `DataGrade` whole, while this door reads only `HasDataref` for the externalized tally; binding applications resolve the URI-reference. `subject` and `time` remain CloudEvents context attributes, and foreign envelopes pass the generic `EventEnvelope` gate without being forced through the Rasm type/source/id grammar. Messaging semantic conventions do not describe this HTTP handler, so no stranded non-HTTP binding roster or `messaging.*` masquerade survives here.

| [INDEX] | [ATTRIBUTE]                      | [CARRIES]                                       |
| :-----: | :------------------------------- | :---------------------------------------------- |
|  [01]   | `cloudevents.event_id`           | the envelope `id`, the operation identity       |
|  [02]   | `cloudevents.event_source`       | the producing capability reference              |
|  [03]   | `cloudevents.event_spec_version` | the specification version the envelope declares |
|  [04]   | `cloudevents.event_type`         | the fact identity a subscription filters on     |
|  [05]   | `cloudevents.event_subject`      | the payload's own address                       |

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public static class WebhookHeader {
    public const string RequestOrigin = "WebHook-Request-Origin";
    public const string RequestRate = "WebHook-Request-Rate";
    public const string AllowedOrigin = "WebHook-Allowed-Origin";
    public const string AllowedRate = "WebHook-Allowed-Rate";
    public const string AccessToken = "access_token";
}

public readonly record struct WebhookOrigin {
    private WebhookOrigin(string value) => Value = value;

    public string Value { get; }

    public static Fin<WebhookOrigin> Admit(string value) =>
        Uri.CheckHostName(value) == UriHostNameType.Dns
            ? Fin.Succ(new WebhookOrigin(value))
            : Fin.Fail<WebhookOrigin>(new CompanionFault.Handshake(WebhookHeader.RequestOrigin));
}

public readonly record struct WebhookRate(Dimension PerMinute) {
    public static Fin<WebhookRate> Admit(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed) && parsed > 0
            ? Fin.Succ(new WebhookRate(Dimension.Create(parsed)))
            : Fin.Fail<WebhookRate>(new CompanionFault.Handshake(WebhookHeader.RequestRate));

    public WebhookRate BoundedBy(WebhookRate ceiling) =>
        PerMinute.Value <= ceiling.PerMinute.Value ? this : ceiling;
}

public readonly record struct WebhookAllowance(Option<WebhookRate> Limit) {
    public string Header => Limit
        .Map(static rate => rate.PerMinute.Value.ToString(CultureInfo.InvariantCulture))
        .IfNone("*");
}

// --- [CONSTANTS] ----------------------------------------------------------------------------
public static class EventSemconv {
    public const string EventId = "cloudevents.event_id";
    public const string EventSource = "cloudevents.event_source";
    public const string SpecVersion = "cloudevents.event_spec_version";
    public const string EventType = "cloudevents.event_type";
    public const string EventSubject = "cloudevents.event_subject";
    public static Seq<(string Slot, object? Value)> Of(CloudEvent envelope) =>
        Seq<(string, object?)>(
            (EventId, envelope.Id),
            (EventSource, envelope.Source?.ToString()),
            (SpecVersion, envelope.SpecVersion.VersionId),
            (EventType, envelope.Type),
            (EventSubject, envelope.Subject));
}

// --- [MODELS] -------------------------------------------------------------------------------
[Equatable]
public sealed partial record Delivery(
    int Accepted, int Duplicate, int Externalized, [property: OrderedEquality] Seq<CompanionFault> Refusals) {
    public static readonly Delivery Empty = new(0, 0, 0, Seq<CompanionFault>());

    public int Refused => Refusals.Count;

    public Delivery Add(Delivery member) => new(
        Accepted + member.Accepted,
        Duplicate + member.Duplicate,
        Externalized + member.Externalized,
        Refusals + member.Refusals);
}

public readonly record struct WebhookGrant(WebhookOrigin Origin, WebhookAllowance Rate);

public readonly record struct WebhookCredential(string Token, bool Query);

public readonly record struct AuthenticatedWebhook(Principal Principal, WebhookCredential Credential);

// A private array is filled exactly once, never exposed mutably, and the parsed framing travels beside it.
// Verification and decode therefore consume one bounded carrier rather than two reads of a request stream.
public sealed class IngressBody {
    private readonly byte[] bytes;

    private IngressBody(byte[] bytes, ContentType framing) {
        this.bytes = bytes;
        Framing = framing;
    }

    public ReadOnlyMemory<byte> Bytes => bytes;

    public ContentType Framing { get; }

    public EventFrame Frame => new(Bytes, Framing);

    public static async Task<Fin<IngressBody>> Capture(HttpRequest request, Dimension limit, Op key) =>
        await AdmittedFraming(request, key).Match(
            Fail: error => Task.FromResult(Fin.Fail<IngressBody>(error)),
            Succ: framing => Captured(request, limit, framing, key)).ConfigureAwait(false);

    private static Fin<ContentType> AdmittedFraming(HttpRequest request, Op key) =>
        request.ContentType is not { Length: > 0 } media
            ? Fin.Fail<ContentType>(new CompanionFault.Handshake(
                HeaderNames.ContentType, StatusCodes.Status415UnsupportedMediaType))
            : key.Catch(() => Fin.Succ(new ContentType(media)))
                .MapFail(_ => (Error)new CompanionFault.Handshake(
                    HeaderNames.ContentType, StatusCodes.Status415UnsupportedMediaType))
                .Bind(framing => EventFormat.Of(framing).IsSome
                    ? Fin.Succ(framing)
                    : Fin.Fail<ContentType>(new CompanionFault.Handshake(
                        framing.MediaType, StatusCodes.Status415UnsupportedMediaType)));

    private static Task<Fin<IngressBody>> Captured(
        HttpRequest request, Dimension limit, ContentType framing, Op key) =>
        key.Catch(async _ => {
            byte[] staging = GC.AllocateUninitializedArray<byte>(checked(limit.Value + 1));
            int count = 0;
            while (count <= limit.Value) {
                int read = await request.Body.ReadAsync(
                    staging.AsMemory(count, staging.Length - count), request.HttpContext.RequestAborted).ConfigureAwait(false);
                if (read == 0) break;
                count += read;
            }
            if (count == 0) {
                return Fin.Fail<IngressBody>(new CompanionFault.Handshake(nameof(HttpRequest.Body)));
            }
            if (count > limit.Value) {
                return Fin.Fail<IngressBody>(new CompanionFault.Handshake(
                    nameof(IngressPolicy.BodyLimit), StatusCodes.Status413PayloadTooLarge));
            }

            byte[] exact = GC.AllocateUninitializedArray<byte>(count);
            staging.AsSpan(0, count).CopyTo(exact);
            return Fin.Succ(new IngressBody(exact, framing));
        }, request.HttpContext.RequestAborted);
}

public sealed record IngressPolicy(
    Func<WebhookOrigin, bool> Origin,
    Option<WebhookRate> Rate,
    IdentityRuntime Identity,
    Option<Func<HttpRequest, ReadOnlyMemory<byte>, Fin<Unit>>> Verify,
    Dimension BodyLimit,
    Func<Uri, Fin<Unit>> Source,
    Func<DataGrade, Fin<Unit>> Classification,
    DedupeWindow Dedupe,
    Func<CloudEvent, global::Rasm.Contracts.Event.V1.Extensions, DataGrade, TenantContext, Op, Fin<DomainEvent>> Project,
    ClockPolicy Clocks,
    FactSink<CompanionSignal> Fan);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class EventIngress {
    public static IResult Validate(HttpRequest request, HttpResponse response, IngressPolicy policy) {
        response.Headers[HeaderNames.Allow] = HttpMethods.Post;
        return Agreement(request, policy).Match(
            Succ: grant => {
                response.Headers[WebhookHeader.AllowedOrigin] = grant.Origin.Value;
                response.Headers[WebhookHeader.AllowedRate] = grant.Rate.Header;
                return Results.NoContent();
            },
            Fail: static _ => Results.NoContent());
    }

    public static IO<Fin<Delivery>> Deliver(
        HttpRequest request, HttpResponse response, IngressPolicy policy, EventBus.Cell bus, Op key) =>
        Authorized(request, policy).Bind(admission => admission.Match(
            Fail: error => IO.pure(Fin.Fail<Delivery>(error)),
            Succ: access => {
                if (access.Credential.Query) response.Headers[HeaderNames.CacheControl] = "private";
                return AllowedOrigin(request, policy).Match(
                    Fail: error => IO.pure(Fin.Fail<Delivery>(error)),
                    Succ: _ => IO.liftAsync(async () => await Decoded(request, policy, key).ConfigureAwait(false))
                        .Bind(rail => rail.Match(
                            Succ: envelopes => envelopes
                                .TraverseM(envelope => Admitted(envelope, access.Principal, policy, bus, key)).As()
                                .Map(static members => members.Fold(
                                    Delivery.Empty, static (tally, member) => tally.Add(member)))
                                .Bind(tally => policy.Fan.Fan(
                                    Correlation.Mint(), nameof(EventIngress), tally, new CompanionSignal.Ingress(tally)))
                                .Map(Fin.Succ),
                            Fail: error => IO.pure(Refused(response, error))));
            }));

    static IO<Fin<AuthenticatedWebhook>> Authorized(HttpRequest request, IngressPolicy policy) =>
        Credential(request).Match(
            Fail: error => IO.pure(Fin.Fail<AuthenticatedWebhook>(error)),
            Succ: credential => TokenValidation.Validate(policy.Identity, credential.Token, Correlation.Mint())
                .Bind(validation => validation.ToFin().Match(
                    Fail: error => IO.pure(Fin.Fail<AuthenticatedWebhook>(error)),
                    Succ: principal => PolicyGate.Authorize(
                            policy.Identity, principal, PolicyDescriptor.WebhookDelivery, request)
                        .Map(verdict => verdict.ToFin().Map(_ => new AuthenticatedWebhook(principal, credential))))));

    static Fin<WebhookCredential> Credential(HttpRequest request) {
        bool hasHeader = request.Headers.TryGetValue(HeaderNames.Authorization, out var header);
        bool hasQuery = request.Query.TryGetValue(WebhookHeader.AccessToken, out var query);
        if (hasHeader == hasQuery) {
            return Fin.Fail<WebhookCredential>(new IdentityFault.Malformed(
                $"{HeaderNames.Authorization}|{WebhookHeader.AccessToken}"));
        }
        if (hasHeader && header.Count == 1
            && AuthenticationHeaderValue.TryParse(header[0], out AuthenticationHeaderValue? parsed)
            && string.Equals(parsed.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            && parsed.Parameter is { Length: > 0 } bearer) {
            return Fin.Succ(new WebhookCredential(bearer, Query: false));
        }
        return hasQuery && query.Count == 1 && query[0] is { Length: > 0 } token
            ? Fin.Succ(new WebhookCredential(token, Query: true))
            : Fin.Fail<WebhookCredential>(new IdentityFault.Malformed(
                hasHeader ? HeaderNames.Authorization : WebhookHeader.AccessToken));
    }

    static Fin<WebhookGrant> Agreement(HttpRequest request, IngressPolicy policy) =>
        from origin in AllowedOrigin(request, policy)
        from requested in RequestedRate(request)
        from granted in Granted(requested, policy.Rate)
        select new WebhookGrant(origin, granted);

    static Fin<WebhookOrigin> AllowedOrigin(HttpRequest request, IngressPolicy policy) =>
        Header(request, WebhookHeader.RequestOrigin)
            .Bind(WebhookOrigin.Admit)
            .Bind(origin => policy.Origin(origin)
                ? Fin.Succ(origin)
                : Fin.Fail<WebhookOrigin>(new CompanionFault.Handshake(WebhookHeader.RequestOrigin)));

    static Fin<Option<WebhookRate>> RequestedRate(HttpRequest request) =>
        !request.Headers.TryGetValue(WebhookHeader.RequestRate, out var values)
            ? Fin.Succ(Option<WebhookRate>.None)
            : values.Count == 1 && values[0] is { Length: > 0 } value
                ? WebhookRate.Admit(value).Map(Some)
                : Fin.Fail<Option<WebhookRate>>(new CompanionFault.Handshake(WebhookHeader.RequestRate));

    static Fin<WebhookAllowance> Granted(Option<WebhookRate> requested, Option<WebhookRate> allowed) =>
        requested.Match(
            Some: ask => Fin.Succ(new WebhookAllowance(Some(
                allowed.Map(ask.BoundedBy).IfNone(ask)))),
            None: () => Fin.Succ(new WebhookAllowance(allowed)));

    static Fin<string> Header(HttpRequest request, string name) =>
        request.Headers.TryGetValue(name, out var values)
            && values.Count == 1 && values[0] is { Length: > 0 } value
            ? Fin.Succ(value)
            : Fin.Fail<string>(new CompanionFault.Handshake(name));

    static async Task<Fin<Seq<CloudEvent>>> Decoded(HttpRequest request, IngressPolicy policy, Op key) {
        Fin<IngressBody> captured = await IngressBody.Capture(request, policy.BodyLimit, key).ConfigureAwait(false);
        return captured.Bind(body => policy.Verify
            .Traverse(verify => verify(request, body.Bytes)).As()
            .Bind(_ => WireAdmission.EventExtensions.Declarations(key))
            .Bind(declared => EventEnvelope.Decode(body.Frame, declared, key)));
    }

    static IO<Delivery> Admitted(
        CloudEvent envelope, Principal principal, IngressPolicy policy, EventBus.Cell bus, Op key) =>
        (from _ in EventEnvelope.Admit(envelope, key)
         from source in Optional(envelope.Source).ToFin(new KernelFault.InvalidValue(
             Label: nameof(CloudEvent.Source), Requirement: "a present URI-reference source", Key: Some(key)))
         from _source in policy.Source(source)
         from extensions in WireAdmission.EventExtensions.Admit(envelope, key)
         from grade in DataGrade.Validate(
                 extensions.Dataclassification, provider: null, out DataGrade? admittedGrade) is null
                 && admittedGrade is { } classification
             ? Fin.Succ(classification)
             : Fin.Fail<DataGrade>(new KernelFault.InvalidValue(
                 Label: nameof(extensions.Dataclassification), Requirement: "an admitted DataGrade", Key: Some(key)))
         from _classification in policy.Classification(grade)
         from raised in policy.Project(envelope, extensions, grade, principal.Tenant, key)
         select (Event: raised, Externalized: extensions.HasDataref)).Match(
            Fail: error => IO.pure(Delivery.Empty with { Refusals = Seq(CompanionFault.Of(error)) }),
            Succ: admitted => policy.Dedupe.Admit($"{envelope.Source}\u0000{envelope.Id}", policy.Clocks.Now)
                ? Dispatched(envelope, admitted.Event, admitted.Externalized, policy, bus)
                : IO.pure(Delivery.Empty with { Duplicate = 1 }));

    static IO<Delivery> Dispatched(
        CloudEvent envelope, DomainEvent admitted, bool externalized, IngressPolicy policy, EventBus.Cell bus) =>
        IO.lift(() => Stamped(envelope))
            .Bind(_ => EventBus.Dispatch(bus, admitted))
            .Map(_ => Delivery.Empty with { Accepted = 1, Externalized = externalized ? 1 : 0 });

    static Unit Stamped(CloudEvent envelope) =>
        EventSemconv.Of(envelope)
            .Fold(unit, static (_, pair) => (Activity.Current?.SetTag(pair.Slot, pair.Value), unit).Item2);

    static Fin<Delivery> Refused(HttpResponse response, Error error) {
        if (error is CompanionFault.Handshake handshake) response.StatusCode = handshake.HttpStatus;
        return Fin.Fail<Delivery>(error);
    }

}
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
