# [RASM_FRAME]

`Rasm.Domain` owns the causal frame every emitting stratum threads: the package-identity roster, the correlation and tenancy pair with their one text and one slot spelling, the session-GUC namespace, the trace-plane vocabulary, the hybrid-logical stamp cell with the causal frame a published fact carries, and the package-identity resolve both host boundaries reach. Emitting packages need no neutral `Guid` twin, because every coordinate this page seats is already in scope wherever a fact is produced.

Identity text federates rather than being re-rendered: the tenant reads `ContentHash.Hex` and admits through `ContentHash.Admit`, so the fixed-width thirty-two-lowercase-hex spelling is the one currency an ambient store, an RLS predicate, a durable partition column, an object-name prefix, and a meter tag all compare byte-identically. Hybrid stamps hold one half order, shared with the compute interchange identity, so a content key and a causal stamp seal one frame every peer re-derives.

## [01]-[INDEX]

- [02]-[SOURCE]: `TelemetrySource`, `CorrelationId` — the minted package roster and the root correlation identity.
- [03]-[TENANCY]: `TenantId`, `TenantMirror`, `TenantContext`, `SessionCoordinate` — the tenancy pair, its ambient stores, and the one `rasm.*` session namespace.
- [04]-[STAMP]: `HlcStamp`, `CausalStamp`, `Hlc` — the hybrid-logical stamp, the causal frame a published fact carries, and the one cell that mints both.
- [05]-[PACKAGE_IDENTITY]: `PackageIdentity<TKey,THostFact>` — the one plugin-identity resolve both host boundaries compose.

## [02]-[SOURCE]

- Owner: `TelemetrySource` is the minted package-identity roster every emitter names its scope by and every `FaultBand` row names its owner by; `CorrelationId` is the boot-minted root identity carrying `Slot`, its one dimension and span-attribute spelling.
- Cases: `TelemetrySource` rows are the branch's own packages alone — a foreign meter or source this branch never authors is an app-platform admission row, never a row here.
- Law: `TraceScope` seats on `Domain/hooks`, the lowest of its three readers, so this page reads it downward and declares no plane vocabulary of its own.
- Law: `CorrelationId.Slot` is the dotted spelling every sibling slot obeys; a `nameof(CorrelationId)` tag key spells PascalCase against that grammar and forks per emitting package.
- Auto: the generator owns `IFormattable` for a formattable key member, so a hand-written twin collides on the partial; both span writers are the declaration's own because the generator emits neither `ISpanFormattable` nor `IUtf8SpanFormattable`.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new minted package is one `TelemetrySource` row, and every `FaultBand` row naming it already compiles.
- Boundary: the roster is the branch package census and nothing else — a runtime-discovered source, a foreign exporter identity, and a resource attribute set are the app platform's composition rows.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetrySource {
    public static readonly TelemetrySource Kernel = new("rasm.kernel");
    public static readonly TelemetrySource Element = new("Rasm.Element");
    public static readonly TelemetrySource AppHost = new("Rasm.AppHost");
    public static readonly TelemetrySource Materials = new("Rasm.Materials");
    public static readonly TelemetrySource Bim = new("Rasm.Bim");
    public static readonly TelemetrySource Fabrication = new("Rasm.Fabrication");
    public static readonly TelemetrySource Persistence = new("Rasm.Persistence");
    public static readonly TelemetrySource Compute = new("Rasm.Compute");
    public static readonly TelemetrySource AppUi = new("Rasm.AppUi");
    public static readonly TelemetrySource Rhino = new("Rasm.Rhino");
    public static readonly TelemetrySource Grasshopper = new("Rasm.Grasshopper");
}

[ValueObject<Guid>(
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None)]
public readonly partial struct CorrelationId : ISpanFormattable, IUtf8SpanFormattable {
    public const string Slot = "rasm.correlation";

    public static readonly CorrelationId None = Create(Guid.Empty);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((Guid)this).TryFormat(destination, out charsWritten, format);
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((Guid)this).TryFormat(utf8Destination, out bytesWritten, format);
}
```

## [03]-[TENANCY]

- Owner: `TenantId` is the tenancy value whose text is the branch's one identity render; `TenantMirror` is an ambient-store row a scope threads; `TenantContext` binds the pair with `TenantSlot` — the one GUC, baggage, and meter-tag key spelling — and `Key` the one optional partition read every consumer folds; `SessionCoordinate` is the four-coordinate `rasm.*` session namespace as a keyed vocabulary.
- Cases: three ambient stores partition by owner — the kernel `AsyncLocal` tenancy slot and the BCL `Activity` baggage store are the rows this assembly reaches, and the OpenTelemetry baggage store registers as one composition-supplied row at the app platform, so no OpenTelemetry type enters this assembly. `TenantContext.Root` is the single-tenant ambient default; a multi-tenant host mints one row per admitted tenant at boot.
- Entry: `Stamp(params ReadOnlySpan<TenantMirror>)` returns `Fin<Lease<IDisposable>>` — the restoring scope over the ambient slot and every mirror row — so a partial stamp reports in the result rather than throwing past the boundary the caller crossed to reach it.
- Auto: the absent-tenant arm is structural and reads through ONE member — `Key` is the one optional partition entry, so `Tags` projects it, `Stamp` writes it, and every consuming store, GUC, partition predicate, and series key folds that `Option` rather than re-deriving a partitioned-or-absent ternary. `Text` and `Admit` compose the ONE hex projection `Domain/identity` seats, so the alphabet, the width, and the case are decided once for the whole federation.
- Law: the tenant text is `ContentHash.Hex` — thirty-two LOWERCASE hex digits — and admission is `ContentHash.Admit`, which REFUSES uppercase. One alphabet both renders and admits, so a value admitted at one boundary renders back the identical spelling and every equality against an ambient entry, an RLS predicate, an object prefix, or a meter tag holds. Both directions read one owner and the round trip is exact.
- Law: admission is ONE typed rail — `Admit` answers `Fin<TenantId>` and the CALLER lowers the carrier by its own evidence, never a second owner entrypoint: a trusted persistence edge takes `ThrowIfFail`, a best-effort ambient read takes `ToOption`, and a validating wire edge maps the fault into its own wire vocabulary.
- Law: `SessionCoordinate` is the C# transcription of the cross-branch `[SESSION_GUC]` law — the four coordinates every RLS predicate and session `set_config` read VERBATIM and byte-share with the TypeScript spine, since disagreeing `SET` and predicate spellings read zero rows fail-closed under FORCE RLS. `Tenant` composes `TenantSlot`, so the telemetry dimension and the session pin stay one vocabulary; `Maintenance` is `Plane`'s sole admitted value, and the maintenance-plane posture pins transaction-locally through a STATED arm, never a role accident.
- Law: stamping is ALL-OR-NOTHING and the restore fold is ONE body both the failed stamp and disposal read — every row is attempted in reverse admission order even when one raises, so a single raising mirror never strands its siblings under the retiring tenant, and each restoration failure appends onto the error the fold carries in.
- Law: disposal residue PARKS on the composition's evidence cell and never throws from `Dispose` — a typed fault a consumer boundary cannot carry outward is parked, never `ignore`d and never re-raised out of a using-block exit (branch RULINGS `[02]`).
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Diagnostics`, `System.Threading`).
- Growth: a new ambient store is one `TenantMirror` row supplied at composition, never a second stamping owner; a new session coordinate is one `SessionCoordinate` row.
- Boundary: tenancy rides an `AsyncLocal` slot rather than a named process-wide registry, so two compositions in one process — an app root beside a plugin load-context capsule — each hold their own tenancy with no duplicate-name registration fault. Foreign-source rows, resource lacing, exporter wiring, and the OpenTelemetry baggage mirror stay at the app platform, which binds its registered mirror set behind one stamping surface so a kernel caller spells `Stamp()` bare.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics;
using System.Threading;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<UInt128>(
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None,
    SkipIParsable = true, SkipIFormattable = true, SkipToString = true)]
public readonly partial struct TenantId {
    public string Text => ContentHash.Hex(ToValue());
    public override string ToString() => Text;

    public static Fin<TenantId> Admit(ReadOnlySpan<char> text) => ContentHash.Admit(text).Map(Create);
}

[SmartEnum<string>(KeyMemberName = "Guc", SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SessionCoordinate {
    public static readonly SessionCoordinate Tenant = new(TenantContext.TenantSlot);
    public static readonly SessionCoordinate Scope = new("rasm.scope");
    public static readonly SessionCoordinate Subject = new("rasm.subject");
    public static readonly SessionCoordinate Plane = new("rasm.plane");

    public const string Maintenance = "maintenance";
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TenantMirror(string Store, Func<Option<string>> Read, Action<Option<string>> Write) {
    public static readonly TenantMirror Span = new(
        Store: nameof(Activity),
        Read: static () => Optional(Activity.Current?.GetBaggageItem(TenantContext.TenantSlot)),
        Write: static entry => ignore(Activity.Current?.SetBaggage(
            TenantContext.TenantSlot, HostEdge.Slot(entry))));
}

public sealed record TenantContext(TenantId TenantId, string Slug) {
    public const string TenantSlot = "rasm.tenant";

    public static readonly TenantContext Root = new(TenantId.Create(UInt128.Zero), "root");

    private static readonly AsyncLocal<TenantContext?> Ambient = new();

    public static TenantContext Current => Ambient.Value ?? Root;

    public string Entry => TenantId.Text;

    public Option<string> Key => !Equals(Root) ? Some(Entry) : None;

    public Seq<KeyValuePair<string, object?>> Tags => Key.Map(
        static entry => new KeyValuePair<string, object?>(TenantSlot, entry)).ToSeq();

    public Fin<Lease<IDisposable>> Stamp(FaultCell residue, params ReadOnlySpan<TenantMirror> mirrors);
    private static readonly HookId StampPoint = HookId.Create(value: "rasm.domain.frame.stamp");

    private static Option<Error> Restored(TenantContext? prior, Seq<(TenantMirror Row, Option<string> Prior)> held, Option<Error> carried);
}
```

## [04]-[STAMP]

- Owner: `HlcStamp` the hybrid-logical stamp — physical `Instant` and logical `ulong` halves, `Packed` the shared `UInt128` layout, `Sequence` the D20 spelling of the logical half; `Hlc` the one per-composition cell over an `IClock`; `CausalStamp` the causal frame a durable fact publishes under — the creation-time `TraceCarrier` carrying tenancy in its baggage, stamp, and the wall instant the mint read — with the five extension slot names it fills.
- Entry: `Hlc.Stamp(wall, seen)` advances the cell on a send and folds a received peer stamp on a receive through one body; `CausalStamp.Now(clock)` captures the live span, `TenantContext.Current`, and a fresh stamp; `RasmEventEnvelope.Publish` (`event.md` `[04]`) is the one publish door that consumes it.
- Auto: `Advance` is the one hybrid-logical mint — the greater of the held and the seen stamp is the floor, the logical half resets to zero on a physical advance and increments on a same-instant repeat; the counter is BOUNDED, so an exhausted one advances the physical half by the WIRE quantum and restarts rather than wrapping, because a wrap re-issues a stamp the stream already carried and every causal comparison after it reads reversed. The escape steps by the packed half's own resolution rather than by the clock's smallest representable step, since a step below the pack quantum re-issues the exact stamp it escaped.
- Law: the half order and its UNIT are FIXED and load-bearing — physical half first as the NodaTime `Instant` Unix-tick `long` at one tick per hundred nanoseconds (I63 inside the `uint64` slot), logical half second as the monotone `ulong`, packed `physical_ticks << 64 | logical`, byte-identical to the compute interchange identity, so a content key and a causal stamp seal one frame every peer re-derives and an off-by-one-half pack corrupts the whole causal order.
- Law: the stamp crosses on the event envelope alone — `time` carries the physical half, `sequence` the logical half, `recordedtime` the wall instant the mint read, `traceparent`/`tracestate`/`baggage` the creation-time carrier with `rasm.tenant` inside `baggage` — so a consumer orders on `(time, sequence)`, joins on `traceparent`, attributes on the baggage member, and measures skew from `recordedtime` against its own arrival; no header, kind, payload, or skew column exists beside those slots.
- Law: a span-less publish admits the one `rasm.tenant` pair through the same W3C codec `TraceCarrier` owns, so tenancy never drops when no bracket is live and no page formats baggage by hand.
- Packages: NodaTime, LanguageExt.Core, BCL inbox (`System.Diagnostics`, `System.Globalization`).
- Growth: a new causal slot is one `CausalStamp` column and one `Slots` row, proven against the generated contract at `EventExtensionContract.Stamp`.
- Boundary: the clock is constructor material; the cell mints no envelope and holds no announcement — the message envelope is `event.md`'s and the span is `telemetry.md`'s.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics;
using System.Globalization;
using NodaTime;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct HlcStamp(Instant Physical, ulong Logical) {
    public static readonly HlcStamp Origin = new(Instant.FromUnixTimeTicks(0L), 0UL);

    public UInt128 Packed => ((UInt128)(ulong)Physical.ToUnixTimeTicks() << 64) | Logical;

    public string Sequence => Logical.ToString("D20", CultureInfo.InvariantCulture);

    public static HlcStamp Advance(HlcStamp last, Instant wall, Option<HlcStamp> seen = default) {
        HlcStamp top = seen.Filter(remote => remote.Packed > last.Packed).IfNone(last);
        return wall > top.Physical ? new(wall, 0UL)
            : top.Logical == ulong.MaxValue ? new(top.Physical + Duration.FromTicks(1L), 0UL)
            : new(top.Physical, top.Logical + 1UL);
    }
}

public sealed record CausalStamp(TraceCarrier Trace, HlcStamp Clock, Instant Recorded) {
    public static CausalStamp Now(Hlc clock) {
        Instant wall = clock.Wall;
        TraceCarrier trace = Activity.Current is { } span
            ? TraceCarrier.Of(span)
            : TraceCarrier.Admit(null, null, HostEdge.Slot(TenantContext.Current.Key.Map(
                static entry => $"{TenantContext.TenantSlot}={entry}")));
        return new(Trace: trace, Clock: clock.Stamp(wall), Recorded: wall);
    }

    public Seq<(string Slot, Option<object> Value)> Slots => Seq(
        ("traceparent", Optional(Trace.TraceParent).Map(static held => (object)held)),
        ("tracestate", Optional(Trace.TraceState).Map(static held => (object)held)),
        ("baggage", Trace.Baggage.Map(static held => (object)held.Value)),
        ("sequence", Some((object)Clock.Sequence)),
        ("recordedtime", Some((object)Recorded.ToDateTimeOffset())));
}

public sealed class Hlc(IClock clock) {
    private readonly Atom<HlcStamp> cell = Atom(HlcStamp.Origin);

    public Instant Wall => clock.GetCurrentInstant();

    public HlcStamp Stamp(Instant wall, Option<HlcStamp> seen = default) =>
        cell.Swap(last => HlcStamp.Advance(last, wall, seen));
}
```

## [05]-[PACKAGE_IDENTITY]

- Owner: `PackageIdentity<TKey,THostFact>` — the one plugin-identity resolve. `TKey` is the host's own typed key (`PluginKey` at the Rhino boundary, `HookScope` at the Grasshopper boundary), so a raw-string plugin parameter cannot enter and the key spaces stay each boundary's; `THostFact` is the host-package evidence the kernel cannot name, carried as an `Option` column rather than forcing a wrapper record at one boundary and not the other.
- Entry: `Resolve(pluginRoot, plugin, host, key)` reads the load context and the assembly version off the plugin root, folds the optional host probe, and lands the identity in the result; `RootOf(Assembly)` is the ONE spelling of the directory read both boundaries hand-wrote byte-identically.
- Law: `PluginSlot` is the owner-declared dimension key beside `CorrelationId.Slot` and `TenantContext.TenantSlot` — a bare noun at an emitting boundary forks the dimension vocabulary (branch RULINGS `[02]`).
- Law: package self-identity homes at the kernel causal frame, so a distant emitter never hand-spells a string-typed scope for a meter, a span, or an event source.
- Law: this owner resolves identity and mints no meter — the metered identity is `Domain/instrument`'s `TelemetryIdentity`, and merging them puts a semantic-convention pin on a value the host resolves at load time.
- Packages: LanguageExt.Core, BCL inbox (`System.Reflection`, `System.Runtime.Loader`).
- Growth: a new host boundary is one instantiation naming its key and its host-fact type; a new resolved column extends the record and both boundaries answer it.
- Boundary: the assembly, its load context, and its host snapshot are the boundary's material — this owner reads them and holds none of them live, so a retired plugin's identity carries no reference keeping its context alive.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Reflection;
using System.Runtime.Loader;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PackageIdentity<TKey, THostFact>(
    TKey Plugin,
    Version Version,
    string ContentRoot,
    AssemblyLoadContext Alc,
    Option<THostFact> Host)
    where TKey : notnull {

    public const string PluginSlot = "rasm.plugin";

    public static Fin<PackageIdentity<TKey, THostFact>> Resolve(
        Assembly pluginRoot,
        TKey plugin,
        Option<Func< Fin<Option<THostFact>>>> host = default);

    private static string RootOf(Assembly pluginRoot) =>
        Path.GetDirectoryName(pluginRoot.Location) is { Length: > 0 } held ? held : AppContext.BaseDirectory;
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
