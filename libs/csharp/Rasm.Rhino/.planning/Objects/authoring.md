# [RASM_RHINO_OBJECTS_AUTHORING]

Custom-object and grip authoring belongs to `Rasm.Rhino.Objects`. Host subclassing stays inside the custom geometry and grip adapters, every verified virtual forwards to an immutable program, and live grip editing resolves value facts inside a document grant. This page also holds the folder's one structured-log egress: `ObjectsTelemetry` publishes every host-callback fault and the process-wide host exception and cloud-log taps through plugin-keyed sinks behind one guarded fan, and `RhinoInstrumentPartition` declares the boundary's instrument projection as data the app root executes. `Display/interaction.md` exclusively owns in-viewport widgets, registration, hit testing, and widget event streams.

## [01]-[INDEX]

- [02]-[OBJECT_PROGRAM]: `ObjectProgram`, `RenderMeshProgram`, the `ObjectsTelemetry` keyed-sink egress with its `HostTap` seat, the `HostSensitivity` classification taxonomy, the `RhinoInstrumentPartition` rows, the `ObjectsHooks` registry mounts, and the forwarding kernel every adapter shares.
- [03]-[ADAPTERS]: the `ClassId`-ready host derivations.
- [04]-[GRIP_PROGRAM]: `GripSeed`, `GripProgram`, `RasmGrip`, `RasmGrips`, and the enabler rig.
- [05]-[GRIP_EDIT]: `GripMove`, `GripEdit`, `GripFacts`, and the `Grips` entry pair.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[OBJECT_PROGRAM]

- Owner: `ObjectProgram` carries every verified draw, duplicate, transform, morph, document, pick, selection, viewport, bounding-box, tight-bounds, and render-mesh hook; `RenderMeshProgram` folds the five-virtual mesh-cache family into one program slot; `ObjectsTelemetry` is the folder's one structured-log egress — every host-callback fault publishes under its `FaultSite` row through the keyed-sink fan, and a new fault seam is one `FaultSite` row with one `Publish` call; `HostTap` seats the host's process-wide exception and cloud-log streams onto that same egress; `HostSensitivity` is the classification taxonomy; `RhinoInstrumentPartition` declares the receipt-to-instrument projection as kind-keyed data; `HostForward` centralizes lifting, the `Fallback`/`Probe` inherited-value recovery pair, and pick capture.
- Law: `ObjectProgram` exposes only callbacks backed by a `RhinoObject` virtual, and each adapter forwards the same algebra; unsupported geometry kinds cannot mint phantom hooks.
- Law: cloud-log severity is data — `Streamed` takes `LogLevel` as a parameter projected once from `HostUtils.LogMessageType` at the tap seam, so one generated event carries every host severity; the host exception rides `Reported`'s typed `Exception` channel, never a stringified hole.
- Law: sinks key on plugin identity — `ObjectsTelemetry.Configure` admits one `(PluginKey, ILogger)` row per plugin, `Publish` fans every event over the live rows with a per-sink guard so one faulted sink never starves siblings, an empty roster is the `NullLogger` no-op composition, and teardown removes only the caller's row; a later plugin can never shadow an earlier plugin's sink, and the `rasm.rhino.objects.fault`, `rasm.rhino.host.exception`, and `rasm.rhino.host.log` hook points bind onto this fan as telemetry-as-tap.
- Law: classification rows mint the suite taxonomy pair — `new DataClassification(nameof(DataClassification), "<row>")` on `Microsoft.Extensions.Compliance.Abstractions`, the contract assembly owning `DataClassification` and `DataClassificationAttribute` — and reuse the app-root values `user-content`, `host-identity`, and `personal`, so every row resolves at the fail-closed redactor map without a second taxonomy; redactor binding stays app-root policy outside the adapters.
- Law: the classification sweep is total over the egress — `Error` and host log messages classify `UserContent` (they embed document names and user text), file and directory payloads classify `HostPath`, process, machine, and version evidence classifies `MachineIdentity`, license and lease facts classify `AccountIdentity`; `FaultSite` keys, host source tokens, and event codes stay unclassified public evidence; exception message and stack admission ride the app-root `LoggerEnrichmentOptions` knobs, never a boundary re-scrub.
- Law: enrichment splits by cost class — `HostStaticEnricher` captures process constants once and `ObjectsTelemetry` composes that classified fact into every generated event; no publish rereads host statics, and each `MachineIdentity` row reaches the app-root redactor through `LogProperties` before egress.
- Law: `HostTap.Mount` is seat arbitration — the first plugin attaches both host delegates and holds the seat, later plugins ride the seat as keyed rows, a rider's disposal removes only its row, and the owner's disposal hands the seat to the senior rider with the delegates still attached or detaches both and returns the seat vacant; delegate identity stays exact, disposal is idempotent, and every plugin mounts beside its own `ObjectsTelemetry.Configure`.
- Law: instrument projection is declared, never executed, in-boundary — `RhinoInstrumentPartition.Rows` maps each receipt kind to `rasm.rhino.<domain>.<measure>` instruments with UCUM units, the source receipt field, and attribution tags, the app root transcribes the kinds into its contributed arm table and merges them into the branch instrument fan, and a row names only a field its receipt already carries — the partition is projection truth, never a second measurement; tenant attribution is app-root baggage promotion, never a boundary field; marshal-seam latency rides the `MarshalLatency` checkpoints on `HostUi/shell.md`, whose `DurationInstrument` constant mirrors the `rasm.rhino.hostui.marshal.duration` label this partition projects.
- Law: `ObjectsHooks.Mount` registers this page's six registry points through `MountRegistry.MountAll` — the three veto points admit only a program already carrying the veto hook (`ObjectProgram.Viewable`, `ObjectProgram.Pick`, `GripProgram` regrow) and grant that program back for adapter composition, the fault point binds a caller `ILogger` onto `ObjectsTelemetry.Configure`, and both host-tap points bind the caller's `PluginKey` onto the one `HostTap.Mount` seat — so every point resolves through `MountRegistry.Bind`, and a later refusal releases every earlier seat.
- Law: the render-mesh surface is the five-virtual cache family — `IsMeshable`, `MeshCount`, `CreateMeshes`, `GetMeshes`, and `DestroyMeshes` refine base-first through one `RenderMeshProgram`; no `OnGetRenderMeshes` virtual exists to forward, the non-virtual RDK accessor trio (`HasCustomRenderMeshes`, `CustomRenderMeshesBoundingBox`, the `RenderMeshes` delegator) stays the Display and Render owners' viewport-and-pipeline context, and the `Rhino.Render.CustomRenderMeshes.RenderMeshProvider` registration adapter belongs to that seam, never an object hook.
- Law: replacement cache meshes are kernel-built — a `Cached` or `Built` hook supplying geometry composes `Meshes.Build` over the `MeshOp` and `MeshEdit` algebra, never a hand-assembled native `Mesh` or a `Mesh.CreateFromSurface` grid; roster meshes handed back become host-owned at the return, and the live `MeshingParameters` each virtual receives crosses to hooks encoded as `MeshPolicy`, never as the native carrier.
- Law: a hook fault never escapes and never degrades to transcript text — every `Fin` refusal publishes the `ObjectCallbackFaulted` event with its `FaultSite` and typed `Error` before the host fallback returns; `NullLogger` is the logger-less composition, and provider policy remains outside the adapters.
- Law: picked objects cross as captures — `PickCandidate` couples a callback-local slot with `PickCapture`; `Pick` returns admitted slots and `OnPicked` stricts the host's picked sequence once before the base call, so a one-shot enumerable feeds base and program alike and neither `ObjRef` nor `PickContext` enters program state.
- Law: base runs first — every forwarding override invokes the host base before its hook, so standard drawing, transform application, and pick behavior survive an inert program, and a program augments rather than re-implements; suppression of base behavior is a genuinely new adapter, never a program flag.
- Growth: a new host virtual is one program field with one forwarding line per adapter.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rasm.Domain;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Rasm.Rhino.Objects;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TightExtent(BoundingBox Current, bool Grow, Transform Motion, bool BaseAnswered);
public readonly record struct PickCandidate(int Slot, PickCapture Capture);

[SmartEnum<int>]
public sealed partial class FaultSite {
    public static readonly FaultSite ObjectCallback = new(key: 0);
    public static readonly FaultSite Pick = new(key: 1);
    public static readonly FaultSite View = new(key: 2);
    public static readonly FaultSite Bounds = new(key: 3);
    public static readonly FaultSite GripLocation = new(key: 4);
    public static readonly FaultSite GripRegrow = new(key: 5);
    public static readonly FaultSite GripTopology = new(key: 6);
    public static readonly FaultSite GripLifecycle = new(key: 7);
    public static readonly FaultSite GripRegistration = new(key: 8);
    public static readonly FaultSite RenderMesh = new(key: 9);
    public static readonly FaultSite Replay = new(key: 10);
    public static readonly FaultSite Conduit = new(key: 11);
    public static readonly FaultSite HostException = new(key: 12);
}

public static class HostSensitivity {
    public static readonly DataClassification UserContent =
        new(taxonomyName: nameof(DataClassification), value: "user-content");
    public static readonly DataClassification HostPath =
        new(taxonomyName: nameof(DataClassification), value: "user-content");
    public static readonly DataClassification MachineIdentity =
        new(taxonomyName: nameof(DataClassification), value: "host-identity");
    public static readonly DataClassification AccountIdentity =
        new(taxonomyName: nameof(DataClassification), value: "personal");
}

public sealed class UserContentAttribute() : DataClassificationAttribute(HostSensitivity.UserContent);

public sealed class HostPathAttribute() : DataClassificationAttribute(HostSensitivity.HostPath);

public sealed class MachineIdentityAttribute() : DataClassificationAttribute(HostSensitivity.MachineIdentity);

public sealed class AccountIdentityAttribute() : DataClassificationAttribute(HostSensitivity.AccountIdentity);

public sealed record HostFaultFact(string Code, [property: UserContent] string Message, bool Exceptional);

public sealed record HostLogFact(
    string Class,
    [property: UserContent] string Description,
    [property: UserContent] string Message);

public sealed record HostStaticFact(
    [property: MachineIdentity] string Process,
    [property: MachineIdentity] string Version,
    [property: MachineIdentity] bool PreRelease);

public readonly record struct RenderMeshBuild(MeshType Kind, Option<MeshPolicy> Policy, bool IgnoreCustom, int Inherited);

public sealed record RenderMeshProgram(
    Option<Func<MeshType, bool, Fin<bool>>> Meshable = default,
    Option<Func<MeshType, Option<MeshPolicy>, int, Fin<int>>> Tally = default,
    Option<Func<RenderMeshBuild, Fin<int>>> Built = default,
    Option<Func<MeshType, Seq<Mesh>, Fin<Seq<Mesh>>>> Cached = default,
    Option<Func<MeshType, Fin<Unit>>> Dropped = default) {
    public static readonly RenderMeshProgram Inert = new();
}

public sealed record ObjectProgram(
    Option<Func<DrawEventArgs, Fin<Unit>>> Draw = default,
    Option<Func<RhinoObject, Fin<Unit>>> Duplicated = default,
    Option<Func<Transform, Fin<Unit>>> Moved = default,
    Option<Func<SpaceMorph, Fin<Unit>>> Morphed = default,
    Option<Func<RhinoDoc, Fin<Unit>>> Entered = default,
    Option<Func<RhinoDoc, Fin<Unit>>> Left = default,
    Option<Func<Seq<PickCandidate>, Fin<Seq<int>>>> Pick = default,
    Option<Func<Seq<PickCapture>, Fin<Unit>>> Picked = default,
    Option<Func<Fin<Unit>>> SelectionChanged = default,
    Option<Func<RhinoViewport, bool, Fin<bool>>> Viewable = default,
    Option<Func<RhinoViewport, BoundingBox, Fin<BoundingBox>>> Bounds = default,
    Option<Func<TightExtent, Fin<BoundingBox>>> TightBounds = default,
    Option<RenderMeshProgram> RenderMeshes = default) {
    public static readonly ObjectProgram Inert = new();
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static partial class ObjectsTelemetry {
    private const int Band = 6400;
    private static readonly Atom<HashMap<PluginKey, ILogger>> Sinks = Atom(HashMap<PluginKey, ILogger>());

    public static Fin<IDisposable> Configure(PluginKey plugin, ILogger sink, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in plugin.Admit(op)
               from row in Optional(sink).ToFin(Fail: op.InvalidInput())
               from seat in Sinks.Swap(held => held.ContainsKey(plugin) ? held : held.Add(plugin, row)).Find(plugin)
                   .Filter(live => ReferenceEquals(live, row))
                   .ToFin(Fail: op.InvalidContext())
               select (IDisposable)Subscription.Of(detach: () => ignore(Sinks.Swap(held =>
                   held.Find(plugin).Filter(live => ReferenceEquals(live, row)).Match(
                       Some: _ => held.Remove(plugin),
                       None: () => held))));
    }

    [LoggerMessage(
        EventId = Band + 1,
        EventName = "ObjectCallbackFaulted",
        Level = LogLevel.Error,
        Message = "object callback faulted at {Site}")]
    private static partial void Faulted(
        ILogger logger,
        FaultSite site,
        [LogProperties(OmitReferenceName = true)] HostFaultFact error,
        [LogProperties(OmitReferenceName = true)] HostStaticFact host);

    [LoggerMessage(
        EventId = Band + 2,
        EventName = "HostExceptionReported",
        Level = LogLevel.Error,
        Message = "host exception at {Site} from {Source}")]
    private static partial void Reported(
        ILogger logger,
        FaultSite site,
        string source,
        Exception cause,
        [LogProperties(OmitReferenceName = true)] HostStaticFact host);

    [LoggerMessage(
        EventId = Band + 3,
        EventName = "HostCloudLog",
        Message = "host cloud log")]
    private static partial void Streamed(
        ILogger logger,
        LogLevel level,
        [LogProperties(OmitReferenceName = true, SkipNullProperties = true)] HostLogFact fact,
        [LogProperties(OmitReferenceName = true)] HostStaticFact host);

    internal static Unit Publish(FaultSite site, Error error) =>
        Fan(sink => Faulted(
            sink,
            site: site,
            error: new HostFaultFact(
                Code: error.Code.ToString(),
                Message: error.Message,
                Exceptional: error.IsExceptional),
            host: HostStaticEnricher.Current));

    internal static Unit Publish(FaultSite site, string source, Exception cause) =>
        Fan(sink => Reported(
            sink,
            site: site,
            source: source,
            cause: cause,
            host: HostStaticEnricher.Current));

    internal static Unit Publish(HostLogFact fact, LogLevel level) =>
        Fan(sink => Streamed(sink, level: level, fact: fact, host: HostStaticEnricher.Current));

    // Per-sink guard: one faulted sink never starves siblings; a logging failure never re-enters host callback flow.
    private static Unit Fan(Action<ILogger> emit) {
        Seq<ILogger> sinks = toSeq(Sinks.Value).Map(static row => row.Value);
        Seq<ILogger> live = sinks.IsEmpty ? Seq<ILogger>(NullLogger.Instance) : sinks;
        return live.Fold(unit, (_, sink) => Try.lift(() => {
                    emit(sink);
                    return unit;
                })
                .Run()
                .IfFail(static _ => unit));
    }
}

public sealed record InstrumentSlice(string Instrument, string Unit, string Source, Seq<string> Tags);

public static class RhinoInstrumentPartition {
    public const string FaultKind = "rhino.fault";
    public const string HostLogKind = "rhino.host-log";
    public const string StreamLossKind = "rhino.stream-loss";
    public const string PointerKind = "rhino.pointer";
    public const string PanelKind = "rhino.panel";
    public const string ContentKind = "rhino.content";
    public const string MarshalKind = "rhino.marshal";
    public const string CensusKind = "rhino.census";
    public const string BenchKind = "rhino.bench";

    public static readonly FrozenDictionary<string, Seq<InstrumentSlice>> Rows = new Dictionary<string, Seq<InstrumentSlice>> {
        [FaultKind] = Seq(
            new InstrumentSlice("rasm.rhino.objects.callback.faults", "{fault}", "ObjectCallbackFaulted", Seq("site", "code"))),
        [HostLogKind] = Seq(
            new InstrumentSlice("rasm.rhino.host.cloud.logs", "{record}", "HostCloudLog", Seq("level", "class"))),
        [StreamLossKind] = Seq(
            new InstrumentSlice("rasm.rhino.document.stream.loss", "{fact}", nameof(StreamReceipt.PacedLoss), Seq("watch", "lane", "loss"))),
        [PointerKind] = Seq(
            new InstrumentSlice("rasm.rhino.display.pointer.submitted", "{fact}", "PointerReceipt.Retired.Submitted", Seq<string>()),
            new InstrumentSlice("rasm.rhino.display.pointer.rejected", "{fact}", "PointerReceipt.Retired.Rejected", Seq<string>())),
        [PanelKind] = Seq(
            new InstrumentSlice("rasm.rhino.hostui.panel.facts", "{fact}", "PanelFact.Ordinal", Seq("panel", "change", "document"))),
        [ContentKind] = Seq(
            new InstrumentSlice("rasm.rhino.render.content.facts", "{fact}", "ContentFact", Seq("pulse", "document")),
            new InstrumentSlice("rasm.rhino.render.content.failures", "{fault}", "ContentStreamFailure", Seq("pulse"))),
        [MarshalKind] = Seq(
            new InstrumentSlice("rasm.rhino.hostui.marshal.duration", "s", "MarshalLatency", Seq("work", "outcome"))),
        [CensusKind] = Seq(
            new InstrumentSlice("rasm.rhino.document.census.objects", "{object}", "DocumentCensus.Kinds", Seq("kind", "space", "document")),
            new InstrumentSlice("rasm.rhino.document.census.layers", "{layer}", "DocumentCensus.Layers", Seq("document")),
            new InstrumentSlice("rasm.rhino.document.census.placements", "{placement}", "DocumentCensus.Blocks", Seq("document")),
            new InstrumentSlice("rasm.rhino.document.census.archive.size", "By", "DocumentCensus.Archive", Seq("document"))),
        [BenchKind] = Seq(
            new InstrumentSlice("rasm.rhino.bench.duration", "s", "BenchEvidence.Duration", Seq("operation", "scale")),
            new InstrumentSlice("rasm.rhino.bench.allocated", "By", "BenchEvidence.AllocatedBytes", Seq("operation", "scale"))),
    }.ToFrozenDictionary(StringComparer.Ordinal);
}

public static class HostStaticEnricher {
    public static HostStaticFact Current { get; } = Capture();

    private static HostStaticFact Capture() {
        HostUtils.GetCurrentProcessInfo(processName: out string process, processVersion: out Version version);
        return new HostStaticFact(Process: process, Version: version.ToString(), PreRelease: HostUtils.IsPreRelease);
    }
}

public sealed class HostTap : IDisposable {
    private static readonly Atom<SeatState> Seat = Atom<SeatState>(new SeatState.Vacant());
    private readonly PluginKey plugin;
    private int released;

    private HostTap(PluginKey plugin) => this.plugin = plugin;

    public static Fin<IDisposable> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in plugin.Admit(op)
               from seat in Seat.Swap(current => current is SeatState.Held held
                       ? held with { Riders = held.Riders.Filter(row => row != plugin).Add(plugin) }
                       : current) is SeatState.Held
                   ? Fin.Succ<IDisposable>(new HostTap(plugin: plugin))
                   : Claim(plugin: plugin, op: op)
               select seat;
    }

    // Delegates attach before the CAS commit; a lost race detaches its own pair and rides the winner's seat.
    private static Fin<IDisposable> Claim(PluginKey plugin, Op op) =>
        op.Catch(() => {
            HostUtils.ExceptionReportDelegate reported = static (source, ex) =>
                ObjectsTelemetry.Publish(
                    site: FaultSite.HostException,
                    source: source ?? nameof(HostUtils),
                    cause: ex);
            HostUtils.SendLogMessageToCloudDelegate streamed = static (kind, sClass, sDesc, sMessage) =>
                ObjectsTelemetry.Publish(
                    fact: new HostLogFact(Class: sClass, Description: sDesc, Message: sMessage),
                    level: Severity(kind: kind));
            HostUtils.OnExceptionReport += reported;
            HostUtils.OnSendLogMessageToCloud += streamed;
            Seq<Action> detachers = Seq<Action>(
                () => HostUtils.OnExceptionReport -= reported,
                () => HostUtils.OnSendLogMessageToCloud -= streamed);
            SeatState committed = Seat.Swap(current => current switch {
                SeatState.Vacant => new SeatState.Held(Owner: plugin, Riders: Seq<PluginKey>(), Detachers: detachers),
                SeatState.Held held => held with { Riders = held.Riders.Filter(row => row != plugin).Add(plugin) },
                var other => other,
            });
            _ = committed is SeatState.Held won && won.Owner == plugin && ReferenceEquals(won.Detachers, detachers)
                ? unit
                : ignore(detachers.Iter(static detach => detach()));
            return Fin.Succ<IDisposable>(new HostTap(plugin: plugin));
        });

    // Severity `_` floor covers `unknown` and every future host value; the host enum is open foreign wire.
    private static LogLevel Severity(HostUtils.LogMessageType kind) => kind switch {
        HostUtils.LogMessageType.information => LogLevel.Information,
        HostUtils.LogMessageType.warning => LogLevel.Warning,
        HostUtils.LogMessageType.error => LogLevel.Error,
        HostUtils.LogMessageType.assert => LogLevel.Critical,
        _ => LogLevel.Warning,
    };

    public void Dispose() {
        if (Interlocked.CompareExchange(ref released, 1, 0) != 0) { return; }
        SeatState prior = new SeatState.Vacant();
        _ = Seat.Swap(current => (prior = current, Departed(current)).Item2);
        _ = prior is SeatState.Held held && held.Owner == plugin && held.Riders.IsEmpty
            ? ignore(held.Detachers.Iter(static detach => detach()))
            : unit;
    }

    // Owner departure hands the seat to the senior rider with the delegates still attached; the last holder detaches.
    private SeatState Departed(SeatState current) => current switch {
        SeatState.Held held when held.Owner == plugin => held.Riders.Head.Match<SeatState>(
            Some: next => held with { Owner = next, Riders = held.Riders.Tail },
            None: () => new SeatState.Vacant()),
        SeatState.Held held => held with { Riders = held.Riders.Filter(row => row != plugin) },
        var other => other,
    };

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    private abstract partial record SeatState {
        private SeatState() { }
        internal sealed record Vacant : SeatState;
        internal sealed record Held(PluginKey Owner, Seq<PluginKey> Riders, Seq<Action> Detachers) : SeatState;
    }
}

public static class ObjectsHooks {
    public static Fin<Seq<IDisposable>> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return MountRegistry.MountAll(
            mounts: Seq(
                Veto(point: HookPoint.ObjectsViewable, plugin: plugin, op: op,
                    carries: static program => program.Viewable.IsSome),
                Veto(point: HookPoint.ObjectsPick, plugin: plugin, op: op,
                    carries: static program => program.Pick.IsSome),
                (Func<Fin<IDisposable>>)(() => MountRegistry.Mount(
                    mount: new HookMount(
                        Point: HookPoint.ObjectsRegrow,
                        Plugin: plugin,
                        Ask: typeof(GripProgram),
                        Grant: typeof(GripProgram),
                        Bind: ask => Optional(ask as GripProgram).ToFin(Fail: op.InvalidInput()).Map(static program => (object)program)),
                    key: op)),
                (Func<Fin<IDisposable>>)(() => MountRegistry.Mount(
                    mount: new HookMount(
                        Point: HookPoint.ObjectsFault,
                        Plugin: plugin,
                        Ask: typeof(ILogger),
                        Grant: typeof(IDisposable),
                        Bind: ask => ObjectsTelemetry.Configure(plugin: plugin, sink: (ILogger)ask, key: op)
                            .Map(static seat => (object)seat)),
                    key: op)),
                Tap(point: HookPoint.HostException, plugin: plugin, op: op),
                Tap(point: HookPoint.HostCloudLog, plugin: plugin, op: op)),
            key: op);
    }

    private static Func<Fin<IDisposable>> Veto(HookPoint point, PluginKey plugin, Op op, Func<ObjectProgram, bool> carries) =>
        () => MountRegistry.Mount(
            mount: new HookMount(
                Point: point,
                Plugin: plugin,
                Ask: typeof(ObjectProgram),
                Grant: typeof(ObjectProgram),
                Bind: ask => Optional(ask as ObjectProgram)
                    .Filter(carries)
                    .ToFin(Fail: op.InvalidInput())
                    .Map(static program => (object)program)),
            key: op);

    private static Func<Fin<IDisposable>> Tap(HookPoint point, PluginKey plugin, Op op) =>
        () => MountRegistry.Mount(
            mount: new HookMount(
                Point: point,
                Plugin: plugin,
                Ask: typeof(PluginKey),
                Grant: typeof(IDisposable),
                Bind: ask => HostTap.Mount(plugin: (PluginKey)ask, key: op).Map(static seat => (object)seat)),
            key: op);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class HostForward {
    internal static Unit Reported(this Fin<Unit> outcome, FaultSite site) =>
        outcome.IfFail(error => ObjectsTelemetry.Publish(site: site, error: error));

    internal static Unit Run<TArgs>(this Option<Func<TArgs, Fin<Unit>>> hook, TArgs args) =>
        hook.Map(run => Op.Of(name: nameof(ObjectProgram)).Catch(() => run(args))
            .Reported(site: FaultSite.ObjectCallback)).IfNone(noneValue: unit);

    internal static Unit Run(this Option<Func<Fin<Unit>>> hook, FaultSite site) =>
        hook.Map(run => Op.Of(name: nameof(ObjectProgram)).Catch(run).Reported(site: site)).IfNone(noneValue: unit);

    internal static Fin<T> Attempt<T>(Func<Op, Fin<T>> body) {
        Op op = Op.Of(name: nameof(ObjectProgram));
        return op.Catch(() => body(op));
    }

    internal static T Fallback<T>(Option<Fin<T>> attempted, FaultSite site, T inherited) =>
        attempted.Match(
            Some: outcome => outcome.Match(
                Succ: identity,
                Fail: error => (ObjectsTelemetry.Publish(site: site, error: error), inherited).Item2),
            None: () => inherited);

    internal static T Probe<T>(Option<Fin<Option<T>>> attempted, FaultSite site, Func<T> inherited) =>
        attempted.Match(
            Some: outcome => outcome.Match(
                Succ: found => found.IfNone(inherited),
                Fail: error => (ObjectsTelemetry.Publish(site: site, error: error), inherited()).Item2),
            None: inherited);

    internal static Seq<ObjRef> Sift(this ObjectProgram program, Seq<ObjRef> candidates) =>
        Fallback(
            attempted: program.Pick.Map(filter => Attempt(op => candidates
                .Map((candidate, slot) => Picks.Capture(reference: candidate, key: op)
                    .Map(capture => new PickCandidate(Slot: slot, Capture: capture)))
                .TraverseM(identity).As()
                .Bind(filter)
                .Bind(slots => guard(slots.ForAll(slot => slot >= 0 && slot < candidates.Count), op.InvalidResult())
                    .ToFin()
                    .Map(_ => slots.Distinct()))
                .Map(slots => candidates
                    .Map((candidate, slot) => (Candidate: candidate, Slot: slot))
                    .Filter(row => slots.Exists(chosen => chosen == row.Slot))
                    .Map(static row => row.Candidate)))),
            site: FaultSite.Pick,
            inherited: candidates);

    internal static bool Active(this ObjectProgram program, RhinoViewport viewport, bool inherited) =>
        Fallback(
            attempted: program.Viewable.Map(judge => Attempt(_ => judge(viewport, inherited))),
            site: FaultSite.View,
            inherited: inherited);

    internal static BoundingBox Box(this ObjectProgram program, RhinoViewport viewport, BoundingBox inherited) =>
        Fallback(
            attempted: program.Bounds.Map(grow => Attempt(op => grow(viewport, inherited)
                .Bind(answer => guard(answer.IsValid, op.InvalidResult()).ToFin().Map(_ => answer)))),
            site: FaultSite.Bounds,
            inherited: inherited);

    internal static bool Tight(this ObjectProgram program, ref BoundingBox box, bool grow, Transform motion, bool inherited) {
        BoundingBox current = box;
        (bool Answered, BoundingBox Bounds) refined = Fallback(
            attempted: program.TightBounds.Map(refine => Attempt(op =>
                refine(new TightExtent(Current: current, Grow: grow, Motion: motion, BaseAnswered: inherited))
                    .Bind(answer => guard(answer.IsValid, op.InvalidResult()).ToFin().Map(_ => (true, answer))))),
            site: FaultSite.Bounds,
            inherited: (inherited, current));
        box = refined.Bounds;
        return refined.Answered;
    }

    internal static Unit Captured(this ObjectProgram program, Seq<ObjRef> picked) =>
        program.Picked.Map(consume => Attempt(op => picked
                .TraverseM(reference => Picks.Capture(reference: reference, key: op)).As()
                .Bind(consume))
            .Reported(site: FaultSite.Pick)).IfNone(noneValue: unit);

    internal static bool Meshable(this ObjectProgram program, MeshType kind, bool inherited) =>
        Fallback(
            attempted: program.RenderMeshes.Bind(static meshes => meshes.Meshable)
                .Map(judge => Attempt(_ => judge(kind, inherited))),
            site: FaultSite.RenderMesh,
            inherited: inherited);

    internal static int Counted(this ObjectProgram program, MeshType kind, MeshingParameters parameters, int inherited) =>
        Fallback(
            attempted: program.RenderMeshes.Bind(static meshes => meshes.Tally)
                .Map(refine => Attempt(op => Policy(parameters: parameters, op: op)
                    .Bind(policy => refine(kind, policy, inherited))
                    .Bind(answer => guard(answer >= 0, op.InvalidResult()).ToFin().Map(_ => answer)))),
            site: FaultSite.RenderMesh,
            inherited: inherited);

    internal static int Constructed(this ObjectProgram program, MeshType kind, MeshingParameters parameters, bool ignore, int inherited) =>
        Fallback(
            attempted: program.RenderMeshes.Bind(static meshes => meshes.Built)
                .Map(refine => Attempt(op => Policy(parameters: parameters, op: op)
                    .Bind(policy => refine(new RenderMeshBuild(
                        Kind: kind, Policy: policy, IgnoreCustom: ignore, Inherited: inherited)))
                    .Bind(answer => guard(answer >= 0, op.InvalidResult()).ToFin().Map(_ => answer)))),
            site: FaultSite.RenderMesh,
            inherited: inherited);

    internal static Mesh[] Roster(this ObjectProgram program, MeshType kind, Mesh[] inherited) =>
        Fallback(
            attempted: program.RenderMeshes.Bind(static meshes => meshes.Cached)
                .Map(refine => Attempt(_ => refine(kind, toSeq(inherited ?? [])).Map(static refined => refined.ToArray()))),
            site: FaultSite.RenderMesh,
            inherited: inherited);

    internal static Unit Dismantled(this ObjectProgram program, MeshType kind) =>
        program.RenderMeshes.Bind(static meshes => meshes.Dropped)
            .Map(run => Op.Of(name: nameof(RenderMeshProgram)).Catch(() => run(kind)).Reported(site: FaultSite.RenderMesh))
            .IfNone(noneValue: unit);

    private static Fin<Option<MeshPolicy>> Policy(MeshingParameters parameters, Op op) =>
        Optional(parameters)
            .Map(native => MeshPolicy.Capture(native: native, key: op).Map(Some))
            .IfNone(() => Fin.Succ(value: Option<MeshPolicy>.None));
}
```

## [03]-[ADAPTERS]

- Owner: abstract host derivations map the catalogued brep, curve, mesh, and point custom bases onto one program contract; no custom SubD, extrusion, hatch, or annotation base exists.
- Law: a concrete package class supplies `[ClassId("<guid>")]`, `Program`, and its geometry-seeded constructor pass-through. Extra state or overrides bypass the duplication contract.
- Law: `RasmCurveObject` alone carries `SetCurve` — the host gives only the curve kind a restage member, surfaced as a protected pass-through; the other kinds replace geometry through the table rail like any object.
- Law: adapters register nothing themselves — placement is `TableOp.Add` with the constructed instance as source, read-back is the state page's window, and the `ClassId` guid is what rehydrates the subclass when a document reopens.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmBrepObject : CustomBrepObject {
    protected RasmBrepObject() { }
    protected RasmBrepObject(Brep brep) : base(brep) { }

    protected abstract ObjectProgram Program { get; }

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { Seq<ObjRef> picked = toSeq(pickedItems).Strict(); base.OnPicked(context, picked); _ = Program.Captured(picked: picked); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Run(site: FaultSite.ObjectCallback); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        Program.Tight(box: ref tightBox, grow: growBox, motion: xform, inherited: base.GetTightBoundingBox(ref tightBox, growBox, xform));
    public sealed override bool IsMeshable(MeshType meshType) => Program.Meshable(kind: meshType, inherited: base.IsMeshable(meshType));
    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) => Program.Counted(kind: meshType, parameters: parameters, inherited: base.MeshCount(meshType, parameters));
    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) => Program.Constructed(kind: meshType, parameters: parameters, ignore: ignoreCustomParameters, inherited: base.CreateMeshes(meshType, parameters, ignoreCustomParameters));
    public sealed override Mesh[] GetMeshes(MeshType meshType) => Program.Roster(kind: meshType, inherited: base.GetMeshes(meshType));
    public sealed override void DestroyMeshes(MeshType meshType) { base.DestroyMeshes(meshType); _ = Program.Dismantled(kind: meshType); }
}

public abstract class RasmCurveObject : CustomCurveObject {
    protected RasmCurveObject() { }
    protected RasmCurveObject(Curve curve) : base(curve) { }

    protected abstract ObjectProgram Program { get; }

    protected Curve Restage(Curve curve) => SetCurve(curve: curve);

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { Seq<ObjRef> picked = toSeq(pickedItems).Strict(); base.OnPicked(context, picked); _ = Program.Captured(picked: picked); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Run(site: FaultSite.ObjectCallback); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        Program.Tight(box: ref tightBox, grow: growBox, motion: xform, inherited: base.GetTightBoundingBox(ref tightBox, growBox, xform));
    public sealed override bool IsMeshable(MeshType meshType) => Program.Meshable(kind: meshType, inherited: base.IsMeshable(meshType));
    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) => Program.Counted(kind: meshType, parameters: parameters, inherited: base.MeshCount(meshType, parameters));
    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) => Program.Constructed(kind: meshType, parameters: parameters, ignore: ignoreCustomParameters, inherited: base.CreateMeshes(meshType, parameters, ignoreCustomParameters));
    public sealed override Mesh[] GetMeshes(MeshType meshType) => Program.Roster(kind: meshType, inherited: base.GetMeshes(meshType));
    public sealed override void DestroyMeshes(MeshType meshType) { base.DestroyMeshes(meshType); _ = Program.Dismantled(kind: meshType); }
}

public abstract class RasmMeshObject : CustomMeshObject {
    protected RasmMeshObject() { }
    protected RasmMeshObject(Mesh mesh) : base(mesh) { }

    protected abstract ObjectProgram Program { get; }

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { Seq<ObjRef> picked = toSeq(pickedItems).Strict(); base.OnPicked(context, picked); _ = Program.Captured(picked: picked); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Run(site: FaultSite.ObjectCallback); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        Program.Tight(box: ref tightBox, grow: growBox, motion: xform, inherited: base.GetTightBoundingBox(ref tightBox, growBox, xform));
    public sealed override bool IsMeshable(MeshType meshType) => Program.Meshable(kind: meshType, inherited: base.IsMeshable(meshType));
    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) => Program.Counted(kind: meshType, parameters: parameters, inherited: base.MeshCount(meshType, parameters));
    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) => Program.Constructed(kind: meshType, parameters: parameters, ignore: ignoreCustomParameters, inherited: base.CreateMeshes(meshType, parameters, ignoreCustomParameters));
    public sealed override Mesh[] GetMeshes(MeshType meshType) => Program.Roster(kind: meshType, inherited: base.GetMeshes(meshType));
    public sealed override void DestroyMeshes(MeshType meshType) { base.DestroyMeshes(meshType); _ = Program.Dismantled(kind: meshType); }
}

public abstract class RasmPointObject : CustomPointObject {
    protected RasmPointObject() { }
    protected RasmPointObject(Point point) : base(point) { }

    protected abstract ObjectProgram Program { get; }

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { Seq<ObjRef> picked = toSeq(pickedItems).Strict(); base.OnPicked(context, picked); _ = Program.Captured(picked: picked); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Run(site: FaultSite.ObjectCallback); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        Program.Tight(box: ref tightBox, grow: growBox, motion: xform, inherited: base.GetTightBoundingBox(ref tightBox, growBox, xform));
    public sealed override bool IsMeshable(MeshType meshType) => Program.Meshable(kind: meshType, inherited: base.IsMeshable(meshType));
    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) => Program.Counted(kind: meshType, parameters: parameters, inherited: base.MeshCount(meshType, parameters));
    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) => Program.Constructed(kind: meshType, parameters: parameters, ignore: ignoreCustomParameters, inherited: base.CreateMeshes(meshType, parameters, ignoreCustomParameters));
    public sealed override Mesh[] GetMeshes(MeshType meshType) => Program.Roster(kind: meshType, inherited: base.GetMeshes(meshType));
    public sealed override void DestroyMeshes(MeshType meshType) { base.DestroyMeshes(meshType); _ = Program.Dismantled(kind: meshType); }
}
```

## [04]-[GRIP_PROGRAM]

- Owner: `GripSeed` admits each grip and the complete zero-based roster consumed by regrow. `GripProgram` is one positional record — required seed and regrow functions, every verified location, reset, mesh-update, topology, draw, and disposal hook as optional slots — matching `ObjectProgram`'s form. `RasmGrip` repairs the host weight sentinel and forwards location changes; `RasmGrips` forwards the set program; `GripRig` registers the enabler.
- Law: `NewGeometry` fires once at drag end — the shim collects each grip's index and current location, hands them to `Regrow`, and a refusal publishes through `ObjectsTelemetry` then returns the inherited result so the host keeps existing geometry; host `NewLocation`/`GripsMoved` and global `Dragging()` remain the rebuild gates.
- Law: `Weight` must be carried by the shim — the custom grip base deliberately stubs the member with a sentinel getter (`-1.234...E+308`) and a no-op setter, so `RasmGrip` overrides both accessors over a real field seeded from `GripSeed.Weight`; an authored grip trusting the base member reads garbage.
- Law: the enabler keys on the grips type's `[Guid]` — `RegisterGripsEnabler` resolves `typeof(TGrips).GUID`, not `ClassIdAttribute`, re-registration replaces the prior enabler, and the enabler installs through `EnableCustomGrips` only when the mint answers `Some`; a non-`Some` candidate keeps standard host grips. Registration demands the declared `GuidAttribute` because the runtime synthesizes a fallback for an unattributed type, so `Type.GUID` is never empty and only the attribute probe proves a stable key. `Sow` accumulates seed admission, admits exactly the zero-based contiguous index roster, stages every shim before mutation, and disposes the new grip owner when any incremental host addition fails.
- Law: the grip draw hook runs before base — the base `OnDraw` draws the grips themselves, so a program draws dynamic elements first and the shim calls base after. Reset and mesh-update hooks augment the completed base operation; disposal notifies before base releases the carrier.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class GripSeed {
    public int Index { get; }
    public Point3d Origin { get; }
    public double Weight { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int index,
        ref Point3d origin,
        ref double weight) {
        validationError = index >= 0 && origin.IsValid && double.IsFinite(weight) && weight >= 0.0
            ? validationError
            : new ValidationError(message: "grip seed is invalid");
    }

    internal static Fin<Seq<GripSeed>> AdmitRoster(Seq<GripSeed> seeds, Op key) =>
        guard(
            seeds.Map(static seed => seed.Index).Order()
                .SequenceEqual(Enumerable.Range(start: 0, count: seeds.Count)),
            key.InvalidInput())
        .ToFin()
        .Map(_ => seeds);
}

public readonly record struct GripMotion(bool NewLocation, bool Moved, bool Dragging);

public sealed record GripProgram(
    Func<GeometryBase, Fin<Seq<GripSeed>>> Seeds,
    Func<Seq<(int Index, Point3d Location)>, Fin<GeometryBase>> Regrow,
    Option<Func<int, Point3d, Fin<Unit>>> LocationChanged = default,
    Option<Func<GripsDrawEventArgs, Fin<Unit>>> Draw = default,
    Option<Func<Fin<Unit>>> Reset = default,
    Option<Func<Fin<Unit>>> ResetMeshes = default,
    Option<Func<MeshType, GripMotion, Fin<Unit>>> UpdateMesh = default,
    Option<Func<int, int, int, int, bool, Fin<Option<GripObject>>>> Neighbor = default,
    Option<Func<int, int, Fin<Option<GripObject>>>> SurfaceGrip = default,
    Option<Func<Fin<Option<NurbsSurface>>>> Surface = default,
    Option<Func<bool, Fin<Unit>>> Disposing = default);

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RasmGrip : CustomGripObject {
    private double weight;
    private readonly Option<Func<int, Point3d, Fin<Unit>>> locationChanged;

    internal RasmGrip(GripSeed seed, Option<Func<int, Point3d, Fin<Unit>>> locationChanged) {
        Index = seed.Index;
        OriginalLocation = seed.Origin;
        weight = seed.Weight;
        this.locationChanged = locationChanged;
    }

    // Base CustomGripObject.Weight is a sentinel getter and a no-op setter; the shim owns the real value.
    public override double Weight {
        get => weight;
        set => weight = value;
    }

    public sealed override void NewLocation() {
        base.NewLocation();
        _ = locationChanged.Map(run => Op.Of().Catch(() => run(Index, CurrentLocation)).Reported(FaultSite.GripLocation)).IfNone(noneValue: unit);
    }
}

public abstract class RasmGrips : CustomObjectGrips {
    protected abstract GripProgram Program { get; }

    protected Fin<Unit> Sow(GeometryBase geometry) {
        Op op = Op.Of(name: nameof(RasmGrips));
        return from source in op.Need(geometry)
               from seeds in op.Catch(() => Program.Seeds(source))
               from admitted in seeds.Traverse(seed => op.Need(seed).ToValidation()).As().ToFin()
               from roster in GripSeed.AdmitRoster(seeds: admitted, key: op)
               from _ in op.Catch(() => roster
                   .Map(seed => new RasmGrip(seed: seed, locationChanged: Program.LocationChanged))
                   .Strict()
                   .TraverseM(grip => op.Catch(() => AddGrip(grip: grip))).As()
                   .Map(static _ => unit))
                   .MapFail(primary => op.Catch(() => {
                       Dispose();
                       return Fin.Succ(value: unit);
                   }).Match(
                       Succ: _ => primary,
                       Fail: cleanup => primary + cleanup))
               select unit;
    }

    protected sealed override GeometryBase NewGeometry() {
        GeometryBase inherited = base.NewGeometry();
        Op op = Op.Of(name: nameof(RasmGrips));
        return op.Catch(() => Program.Regrow(toSeq(Enumerable.Range(start: 0, count: GripCount))
                    .Map(index => Grip(index: index))
                    .Map(static grip => (grip.Index, grip.CurrentLocation)))
                .Bind(grown => Optional(grown).ToFin(Fail: op.InvalidResult())))
            .Match(
                Succ: static grown => grown,
                Fail: error => {
                    _ = ObjectsTelemetry.Publish(site: FaultSite.GripRegrow, error: error);
                    return inherited;
                });
    }

    protected sealed override void OnDraw(GripsDrawEventArgs args) {
        _ = Program.Draw.Map(run => Op.Of(name: nameof(RasmGrips)).Catch(() => run(args))
            .Reported(FaultSite.GripLifecycle)).IfNone(noneValue: unit);
        base.OnDraw(args);
    }

    protected sealed override void OnReset() {
        base.OnReset();
        _ = Program.Reset.Run(site: FaultSite.GripLifecycle);
    }

    protected sealed override void OnResetMeshes() {
        base.OnResetMeshes();
        _ = Program.ResetMeshes.Run(site: FaultSite.GripLifecycle);
    }

    protected sealed override void OnUpdateMesh(MeshType meshType) {
        base.OnUpdateMesh(meshType);
        _ = Program.UpdateMesh.Map(run => Op.Of().Catch(() => run(
                meshType,
                new GripMotion(NewLocation: NewLocation, Moved: GripsMoved, Dragging: Dragging())))
            .Reported(FaultSite.GripLifecycle)).IfNone(noneValue: unit);
        NewLocation = false;
    }

    protected sealed override GripObject NeighborGrip(int gripIndex, int dr, int ds, int dt, bool wrap) =>
        HostForward.Probe(
            attempted: Program.Neighbor.Map(find => Op.Of(name: nameof(RasmGrips)).Catch(() => find(gripIndex, dr, ds, dt, wrap))),
            site: FaultSite.GripTopology,
            inherited: () => base.NeighborGrip(gripIndex, dr, ds, dt, wrap));

    protected sealed override GripObject NurbsSurfaceGrip(int i, int j) =>
        HostForward.Probe(
            attempted: Program.SurfaceGrip.Map(find => Op.Of(name: nameof(RasmGrips)).Catch(() => find(i, j))),
            site: FaultSite.GripTopology,
            inherited: () => base.NurbsSurfaceGrip(i, j));

    protected sealed override NurbsSurface NurbsSurface() =>
        HostForward.Probe(
            attempted: Program.Surface.Map(find => Op.Of(name: nameof(RasmGrips)).Catch(find)),
            site: FaultSite.GripTopology,
            inherited: base.NurbsSurface);

    protected sealed override void Dispose(bool disposing) {
        if (disposing) {
            _ = Program.Disposing.Map(run => Op.Of(name: nameof(RasmGrips)).Catch(() => run(disposing))
                .Reported(FaultSite.GripLifecycle)).IfNone(noneValue: unit);
        }
        base.Dispose(disposing);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GripRig {
    public static Fin<Unit> Register<TGrips>(Func<RhinoObject, Option<TGrips>> mint) where TGrips : CustomObjectGrips {
        Op op = Op.Of(name: nameof(GripRig));
        return from factory in op.Need(mint)
               from __ in guard(
                   typeof(TGrips).IsDefined(typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false),
                   op.InvalidInput()).ToFin()
               from _ in op.Catch(() => {
                   CustomObjectGrips.RegisterGripsEnabler(
                       enabler: candidate => {
                           Fin<Unit> enabled = op.Catch(() => factory(candidate).Match(
                               Some: grips => op.Confirm(success: candidate.EnableCustomGrips(customGrips: grips))
                                   .MapFail(error => {
                                       grips.Dispose();
                                       return error;
                                   }),
                               None: () => Fin.Succ(value: unit)));
                           _ = enabled.Reported(FaultSite.GripRegistration);
                       },
                       customGripsType: typeof(TGrips));
               })
               select unit;
    }
}
```

## [05]-[GRIP_EDIT]

- Owner: `GripMove` `[Union]` — the relocation verbs: absolute point, delta vector, transform, and single-step undo; `GripEdit` `[Union]` — the two grip mutations: `Rig` toggles `GripsOn`, `Move` relocates one indexed grip or every grip through a `GripMove` verb; `GripFacts` — the whole grip read in one pass: identity, positions, movement state, weight, local frame, and the surface, curve, and cage parameter coordinates with their control-vertex indices, each projected as absence where the grip kind carries none; `ObjectReceipt<Guid>`/`GripCensus` — the detached results; `Grips` — the two entries: `Census` the read, `Touch` the immediate mutation.
- Law: grips resolve from their owner — `GripEdit.Rig` toggles `GripsOn`, `Census` and `GripEdit.Move` read `GetGrips` inside the grant, and a grip index addresses into that roster; no `GripObject` leases outward, because grip lifetime ends when the owner's grips turn off.
- Law: parameter reads are capability probes — `GetSurfaceParameters`, `GetCurveParameters`, `GetCageParameters`, and the CV-index members answer `false` or empty on grips of another kind, and the facts project absence rather than faulting, so one census serves every grip kind.
- Law: movement is immediate visual state under the host's drag machinery — `Move` and `UndoMove` mutate the grip, `Touch` opens no undo record, and the geometry consequence lands when the host drives the owner's grip pipeline; a program wanting transactional geometry replacement routes the regrown value through `TableOp.Replace`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GripMove {
    private GripMove() { }
    public sealed record To(Point3d Location) : GripMove;
    public sealed record By(Vector3d Delta) : GripMove;
    public sealed record Via(Transform Motion) : GripMove;
    public sealed record Back : GripMove;

    internal Fin<GripMove> Admit(Op op) =>
        Switch(
            op,
            to: static (key, move) => key.AcceptInput(value: move.Location).Map(_ => (GripMove)move),
            by: static (key, move) => key.AcceptInput(value: move.Delta).Map(_ => (GripMove)move),
            via: static (key, move) => key.AcceptInput(value: move.Motion).Map(_ => (GripMove)move),
            back: static (_, move) => Fin.Succ<GripMove>(move));

    internal Fin<Unit> Apply(GripObject grip, Op op) =>
        Switch(
            (Grip: grip, Op: op),
            to: static (context, move) => context.Op.Catch(() => context.Grip.Move(newLocation: move.Location)),
            by: static (context, move) => context.Op.Catch(() => context.Grip.Move(delta: move.Delta)),
            via: static (context, move) => context.Op.Catch(() => context.Grip.Move(xform: move.Motion)),
            back: static (context, _) => context.Op.Catch(() => context.Grip.UndoMove()));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GripEdit {
    private GripEdit() { }
    public sealed record Rig(ObjectSignal Signal) : GripEdit;
    public sealed record Move(Option<int> Index, GripMove Motion) : GripEdit;

    internal Fin<GripEdit> Admit(Op op) =>
        Switch(
            op,
            rig: static (key, edit) => key.Need(edit.Signal).Map(_ => (GripEdit)edit),
            move: static (key, edit) =>
                from motion in key.Need(edit.Motion).Bind(value => value.Admit(op: key))
                from _ in guard(edit.Index.Map(static value => value >= 0).IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (GripEdit)new Move(Index: edit.Index, Motion: motion));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record GripCensus(Seq<(Guid Owner, Seq<GripFacts> Rows)> Rows) : IDetachedDocumentResult;

public sealed record GripFacts(
    int Index,
    Guid OwnerId,
    Point3d Current,
    Point3d Origin,
    bool Moved,
    double Weight,
    Option<(Vector3d U, Vector3d V, Vector3d Normal)> Directions,
    Option<Point2d> SurfaceUv,
    Option<double> CurveT,
    Option<Point3d> CageUvw,
    Seq<int> CurveCvs,
    Seq<(int I, int J)> SurfaceCvs) : IDetachedDocumentResult {
    internal static Fin<GripFacts> Of(GripObject grip, Op key) =>
        key.Catch(() => {
            bool framed = grip.GetGripDirections(u: out Vector3d u, v: out Vector3d v, normal: out Vector3d normal);
            bool onSurface = grip.GetSurfaceParameters(u: out double su, v: out double sv);
            bool onCurve = grip.GetCurveParameters(t: out double t);
            bool inCage = grip.GetCageParameters(u: out double cu, v: out double cv, w: out double cw);
            int curveCount = grip.GetCurveCVIndices(cvIndices: out int[] curveCvs);
            int surfaceCount = grip.GetSurfaceCVIndices(cvIndices: out Tuple<int, int>[] surfaceCvs);
            return Fin.Succ(value: new GripFacts(
                Index: grip.Index,
                OwnerId: grip.OwnerId,
                Current: grip.CurrentLocation,
                Origin: grip.OriginalLocation,
                Moved: grip.Moved,
                Weight: grip.Weight,
                Directions: framed ? Some((u, v, normal)) : Option<(Vector3d, Vector3d, Vector3d)>.None,
                SurfaceUv: onSurface ? Some(new Point2d(x: su, y: sv)) : Option<Point2d>.None,
                CurveT: onCurve ? Some(t) : Option<double>.None,
                CageUvw: inCage ? Some(new Point3d(x: cu, y: cv, z: cw)) : Option<Point3d>.None,
                CurveCvs: curveCount > 0 ? toSeq(curveCvs) : Seq<int>(),
                SurfaceCvs: surfaceCount > 0
                    ? toSeq(surfaceCvs).Map(static pair => (pair.Item1, pair.Item2))
                    : Seq<(int, int)>()));
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Grips {
    public static Fin<GripCensus> Census(DocumentSession session, TableTarget target) {
        Op op = Op.Of();
        return Optional(session).ToFin(Fail: op.MissingContext()).Bind(owner => owner.Demand(
            use: document =>
                from natives in Objects.Resolve(document: document, target: target, key: op)
                from rows in natives.TraverseM(native => op.Catch(() =>
                    Optional(native.GetGrips()).Map(static held => toSeq(held)).IfNone(Seq<GripObject>())
                        .TraverseM(grip => GripFacts.Of(grip: grip, key: op)).As()
                        .Map(facts => (native.Id, facts)))).As()
                select new GripCensus(Rows: rows),
            key: op,
            needs: [SessionNeed.Read]));
    }

    public static Fin<ObjectReceipt<Guid>> Touch(DocumentSession session, TableTarget target, GripEdit edit) {
        Op op = Op.Of();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from active in op.Need(edit).Bind(value => value.Admit(op: op))
               from receipt in owner.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target, key: op)
                       from ids in natives.TraverseM(native => active.Switch(
                           (Native: native, Op: op),
                           rig: static (ctx, edit) => ctx.Op.Catch(() => {
                               ctx.Native.GripsOn = edit.Signal.On;
                               return Fin.Succ(value: ctx.Native.Id);
                           }),
                           move: static (ctx, edit) =>
                               from roster in ctx.Op.Catch(() => Fin.Succ(value: Optional(ctx.Native.GetGrips())
                                   .Map(static held => toSeq(held)).IfNone(Seq<GripObject>())))
                               from chosen in edit.Index.Case switch {
                                   int at => roster.Filter(grip => grip.Index == at) switch {
                                       [var only] => Fin.Succ(value: Seq(only)),
                                       _ => Fin.Fail<Seq<GripObject>>(error: ctx.Op.MissingContext()),
                                   },
                                   _ => Fin.Succ(value: roster),
                               }
                               from _ in guard(!chosen.IsEmpty, ctx.Op.MissingContext()).ToFin()
                               from __ in chosen.TraverseM(grip => edit.Motion.Apply(grip: grip, op: ctx.Op)).As()
                               select ctx.Native.Id)).As()
                       select new ObjectReceipt<Guid>(Facts: ids, UndoSerials: Seq<uint>()),
                   key: op,
                   needs: [SessionNeed.Mutate])
               select receipt;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]                | [FORM]                                                 | [ENTRY]                         |
| :-----: | :--------------- | :--------------------- | :----------------------------------------------------- | :------------------------------ |
|  [01]   | override program | `ObjectProgram`        | optional `Fin` hooks over the complete verified roster | adapter `Program` slots         |
|  [02]   | host derivations | `Rasm*Object`          | sealed forwarding over one shared kernel, base-first   | `[ClassId]` concrete subclasses |
|  [03]   | grip authoring   | `GripProgram`          | required seed/regrow core plus optional hook slots     | `RasmGrips` overrides           |
|  [04]   | grip shims       | `RasmGrip`/`RasmGrips` | sentinel-weight repair and roster forwarding           | `GripRig.Register<TGrips>`      |
|  [05]   | grip value edits | `GripEdit`             | rig and move over `GripMove` verbs, detached receipts  | `Grips.Touch` / `Census`        |
|  [06]   | render-mesh cache | `RenderMeshProgram`   | base-first refinement over the five cache virtuals     | adapter mesh overrides          |
|  [07]   | telemetry egress | `ObjectsTelemetry`     | generated fault and host-stream events over keyed sinks | `Publish` / `Configure`         |
|  [08]   | host taps        | `HostTap`              | seat arbitration with rider handoff over both host taps | `HostTap.Mount`                 |
|  [09]   | classification   | `HostSensitivity`      | suite taxonomy rows and member annotation attributes   | payload attributes              |
|  [10]   | instrument rows  | `RhinoInstrumentPartition` | kind-keyed projection data the app root executes   | `Rows`                          |
|  [11]   | hook mounts      | `ObjectsHooks`         | six registry points over veto programs and tap seats   | `ObjectsHooks.Mount`            |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
