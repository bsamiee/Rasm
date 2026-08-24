# [RASM_FRAME]

`Rasm.Domain` owns the causal frame every emitting stratum threads: the package-identity roster, the correlation and tenancy pair with their one text and one slot spelling, the session-GUC namespace, the trace-plane vocabulary, the stamped receipt envelope with its hybrid-logical mint, and the package-identity resolve both host boundaries reach. Emitting packages need no neutral `Guid` twin, because every coordinate this page seats is already in scope wherever a receipt is minted.

Identity text federates rather than being re-rendered: the tenant reads `ContentHash.Hex` and admits through `ContentHash.Admit`, so the fixed-width thirty-two-lowercase-hex spelling is the one currency an ambient store, an RLS predicate, a durable partition column, an object-name prefix, and a meter tag all compare byte-identically. Hybrid stamps hold one half order, shared with the compute interchange identity, so a content key and a causal stamp seal one frame every peer re-derives.

## [01]-[INDEX]

- [02]-[SOURCE]: `TelemetrySource`, `CorrelationId` — the minted package roster and the root correlation identity.
- [03]-[TENANCY]: `TenantId`, `TenantMirror`, `TenantContext`, `SessionCoordinate` — the tenancy pair, its ambient stores, and the one `rasm.*` session namespace.
- [04]-[RECEIPT_PORT]: `ReceiptEnvelope`, `ReceiptSinkPort` — the stamped evidence value and the one hybrid-logical mint behind it.
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

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
// Minted rows alone: a foreign meter or source this branch never authors is an app-platform admission row.
[SmartEnum<string>]
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

    // `IFormattable` belongs to the generator unless `SkipIFormattable` opts out, so a hand-written twin
    // collides on the partial; the two span writers are this declaration's because the generator emits neither.
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((Guid)this).TryFormat(destination, out charsWritten, format);
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((Guid)this).TryFormat(utf8Destination, out bytesWritten, format);
}
```

## [03]-[TENANCY]

- Owner: `TenantId` is the tenancy value whose text is the branch's one identity render; `TenantMirror` is an ambient-store row a scope threads; `TenantContext` binds the pair with `TenantSlot` — the one GUC, baggage, and meter-tag key spelling — and `Key` the one optional partition read every consumer folds; `SessionCoordinate` is the four-coordinate `rasm.*` session namespace as a keyed vocabulary.
- Cases: three ambient stores partition by owner — the kernel `AsyncLocal` tenancy slot and the BCL `Activity` baggage store are the rows this assembly reaches, and the OpenTelemetry baggage store registers as one composition-supplied row at the app platform, so no OpenTelemetry type enters this assembly. `TenantContext.Root` is the single-tenant ambient default; a multi-tenant host mints one row per admitted tenant at boot.
- Entry: `Stamp(params ReadOnlySpan<TenantMirror>)` returns `Fin<Lease<IDisposable>>` — the restoring scope over the ambient slot and every mirror row — so a partial stamp reports on the rail rather than throwing past the boundary the caller crossed to reach it.
- Auto: the absent-tenant arm is structural and reads through ONE member — `Key` is the optional entry `Partitions` decides, so `Tags` projects it, `Stamp` writes it, and every consuming store, GUC, partition predicate, and series key folds that `Option` rather than re-deriving a `Partitions ? Entry : absent` ternary. `Text` and `Admits` compose the ONE hex projection `Domain/identity` seats, so the alphabet, the width, and the case are decided once for the whole federation.
- Law: the tenant text is `ContentHash.Hex` — thirty-two LOWERCASE hex digits — and admission is `ContentHash.Admit`, which REFUSES uppercase. One alphabet both renders and admits, so a value admitted at one seam renders back the identical spelling and every equality against an ambient entry, an RLS predicate, an object prefix, or a meter tag holds. Both directions read one owner and the round trip is exact.
- Law: admission splits by the CALLER's evidence, never by a second rule — trusted text (a boot roster row, a re-read of text this seam already rendered) takes `Of` and a violation is a program defect the argument contract names, while untrusted text (a wire claim, a request header, a config cell) takes `TryOf` and folds `None` onto the caller's own refusal rail.
- Law: `SessionCoordinate` is the C# transcription of the cross-branch `[SESSION_GUC]` law — the four coordinates every RLS predicate and session `set_config` read VERBATIM and byte-share with the TypeScript spine, since disagreeing `SET` and predicate spellings read zero rows fail-closed under FORCE RLS. `Tenant` composes `TenantSlot`, so the telemetry dimension and the session pin stay one vocabulary; `Maintenance` is `Plane`'s sole admitted value, and the maintenance-plane posture pins transaction-locally through a STATED arm, never a role accident.
- Law: stamping is ALL-OR-NOTHING and the restore fold is ONE body both the failed stamp and disposal read — every row is attempted in reverse admission order even when one raises, so a single raising mirror never strands its siblings under the retiring tenant, and each restoration failure appends onto the error the fold carries in.
- Law: disposal residue PARKS on the composition's evidence cell and never throws from `Dispose` — a typed rail a consumer seam cannot carry outward is parked, never `ignore`d and never re-raised out of a using-block exit (branch RULINGS `[02]`).
- Receipt: none — the tenancy pair is its own evidence, and the stamped envelope is `[04]`'s.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Diagnostics`, `System.Threading`).
- Growth: a new ambient store is one `TenantMirror` row supplied at composition, never a second stamping owner; a new session coordinate is one `SessionCoordinate` row.
- Boundary: tenancy rides an `AsyncLocal` slot rather than a named process-wide registry, so two compositions in one process — an app root beside a plugin load-context capsule — each hold their own tenancy with no duplicate-name registration fault. Foreign-source rows, resource lacing, exporter wiring, and the OpenTelemetry baggage mirror stay at the app platform, which binds its registered mirror set behind one stamping surface so a kernel caller spells `Stamp()` bare.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics;
using System.Threading;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<UInt128>(
    KeyMemberName = "Value",
    KeyMemberAccessModifier = AccessModifier.Public,
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None)]
public readonly partial struct TenantId {
    // ONE identity text for the whole federation: `ContentHash.Hex` renders and `ContentHash.Admit` gates, so the
    // width, the alphabet, and the CASE are decided at the identity owner and this seam re-spells none of them.
    // `Text` and `ContentHash.Admit` share ONE alphabet, so a round trip preserves the spelling every
    // ambient store, RLS predicate, object prefix, and meter tag compares against.
    public string Text => ContentHash.Hex(Value);

    // TRUSTED-TEXT entry by argument contract — the composition's own frozen literal — so the one raise sits here;
    // foreign text enters through `TryOf` and never reaches it.
    public static TenantId Of(ReadOnlySpan<char> text) => Create(ContentHash.Admit(text, Op.Of()).ThrowIfFail());

    public static Option<TenantId> TryOf(ReadOnlySpan<char> text) => ContentHash.Admit(text, Op.Of()).ToOption().Map(Create);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SessionCoordinate {
    public static readonly SessionCoordinate Tenant = new(TenantContext.TenantSlot);
    public static readonly SessionCoordinate Scope = new("rasm.scope");
    public static readonly SessionCoordinate Subject = new("rasm.subject");
    public static readonly SessionCoordinate Plane = new("rasm.plane");

    public const string Maintenance = "maintenance";
}

// --- [MODELS] -------------------------------------------------------------------------------
// One ambient store per row: `Read` snapshots the prior entry, `Write` seats it or clears it. The OpenTelemetry
// baggage store is the app platform's, so its row registers there and no OpenTelemetry type enters this assembly.
public sealed record TenantMirror(string Store, Func<Option<string>> Read, Action<Option<string>> Write) {
    // Activity baggage clears an entry the current activity itself added; an entry inherited from a parent stays the
    // parent's to own, which is why the span mirror RESTORES rather than deleting unconditionally.
    public static readonly TenantMirror Span = new(
        Store: nameof(Activity),
        Read: static () => Optional(Activity.Current?.GetBaggageItem(TenantContext.TenantSlot)),
        Write: static entry => ignore(Activity.Current?.SetBaggage(
            TenantContext.TenantSlot,
            entry.Match<string?>(Some: static held => held, None: static () => null))));
}

public sealed record TenantContext(TenantId TenantId, string Slug) {
    public const string TenantSlot = "rasm.tenant";

    public static readonly TenantContext Root = new(TenantId.Create(UInt128.Zero), "root");

    private static readonly AsyncLocal<TenantContext?> Ambient = new();

    public static TenantContext Current => Ambient.Value ?? Root;

    public bool Partitions => !Equals(Root);

    public string Entry => TenantId.Text;

    // `Key` is the ONE optional-key read: absence IS the root row, so every store write, GUC bind, partition predicate,
    // and series key folds this Option instead of re-deriving the ternary at its own seam.
    public Option<string> Key => Partitions ? Some(Entry) : None;

    public Seq<KeyValuePair<string, object?>> Tags =>
        Key.Map(static entry => Seq(new KeyValuePair<string, object?>(TenantSlot, entry))).IfNone(Seq<KeyValuePair<string, object?>>());

    // Stamping is ALL-OR-NOTHING on the RAIL: a mirror write that fails after the ambient slot and its
    // predecessors already moved leaves a scope no caller holds, so the partial fold rolls back through the same
    // reverse-order restore disposal runs and reports every restoration failure beside the original cause.
    public Fin<Lease<IDisposable>> Stamp(FaultCell residue, params ReadOnlySpan<TenantMirror> mirrors);
    // `StampPoint` parks every restore residue — the cell stamps and orders it; disposal idempotence is the lease's.
    private static readonly HookId StampPoint = HookId.Create(value: "rasm.domain.frame.stamp");

    // ONE restore fold both the failed stamp and disposal read: every row is attempted in reverse admission order
    // even when one raises, and each restoration failure appends onto the error the fold carries in, so the caller
    // reads the original cause and every secondary fault on one value rather than the first raise winning.
    private static Option<Error> Restored(TenantContext? prior, Seq<(TenantMirror Row, Option<string> Prior)> held, Option<Error> carried);
}
```

## [04]-[RECEIPT_PORT]

- Owner: `ReceiptEnvelope` the stamped evidence value carrying the one causal frame; `ReceiptSinkPort` the emit port carrying the one hybrid-logical mint.
- Entry: `Send(correlation, tenant, package, kind, payload)` returns the `IO<ReceiptEnvelope>` whose value IS the emission evidence.
- Auto: `Advance` is the one hybrid-logical mint — the logical half resets to zero on a physical advance and increments on a same-instant repeat; the counter is BOUNDED, so an exhausted one advances the physical component by the WIRE quantum and restarts rather than wrapping, because a wrap re-issues a stamp the stream already carried and every causal comparison after it reads reversed. The escape steps by the packed half's own resolution rather than by the clock's smallest representable step, since a step below the pack quantum re-issues the exact stamp it escaped. `SkewBound` derives at stamp time as the wall-clock lag the advance observed.
- Law: the half order and its UNIT are FIXED and load-bearing — physical half first as the NodaTime `Instant` Unix-tick `long` at one tick per hundred nanoseconds, logical half second as the monotone `ulong`, both little-endian on the wire and byte-identical to the compute interchange identity, so a content key and a causal stamp seal one frame every peer re-derives and an off-by-one-half pack corrupts the whole causal order.
- Receipt: `ReceiptEnvelope` is the receipt — correlation, tenant, and the two-half stamp threaded together.
- Packages: NodaTime, LanguageExt.Core, BCL inbox (`System.Text.Json`).
- Growth: a new stamped surface draws from the same cell; a new envelope column extends the record and every producer answers it.
- Boundary: a clock, a hybrid-logical cell, and an emit delegate are constructor material this port never mints.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Text.Json;
using NodaTime;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ReceiptEnvelope(
    CorrelationId Correlation,
    TenantContext Tenant,
    TelemetrySource Package,
    string Kind,
    JsonElement Payload,
    Instant Physical,
    ulong Logical,
    Duration SkewBound);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record ReceiptSinkPort(
    IClock Clock,
    Atom<(Instant Physical, ulong Logical)> Hlc,
    Func<ReceiptEnvelope, IO<Unit>> Emit) {
    // Bounded logical half: an exhausted counter advances the physical component by ONE NodaTime tick — a hundred
    // nanoseconds, the exact resolution the packed Unix-tick physical half carries — rather than wrapping, because a
    // wrap re-issues a stamp the stream already carried. `Duration.Epsilon` is one NANOSECOND and packs to the value
    // it escaped, so the quantum is the pack's, never the `Duration` type's.
    private const long TickQuantum = 1L;

    public static (Instant Physical, ulong Logical) Advance(
        (Instant Physical, ulong Logical) last, Instant wall) =>
        wall > last.Physical ? (wall, 0UL)
        : last.Logical == ulong.MaxValue ? (last.Physical + Duration.FromTicks(TickQuantum), 0UL)
        : (last.Physical, last.Logical + 1UL);

    public IO<ReceiptEnvelope> Send(CorrelationId correlation, TenantContext tenant, TelemetrySource package, string kind, JsonElement payload) =>
        IO.lift(() => Clock.GetCurrentInstant())
            .Map(wall => (Wall: wall, Cell: Hlc.Swap(last => Advance(last, wall))))
            .Map(state => new ReceiptEnvelope(
                correlation, tenant, package, kind, payload,
                state.Cell.Physical, state.Cell.Logical, state.Cell.Physical - state.Wall))
            .Bind(envelope => Emit(envelope).Map(_ => envelope));
}
```

## [05]-[PACKAGE_IDENTITY]

- Owner: `PackageIdentity<TKey,THostFact>` — the one plugin-identity resolve. `TKey` is the host's own typed key (`PluginKey` at the Rhino boundary, `HookScope` at the Grasshopper boundary), so a raw-string plugin parameter cannot enter and the key spaces stay each boundary's; `THostFact` is the host-package evidence the kernel cannot name, carried as an `Option` column rather than forcing a wrapper record at one boundary and not the other.
- Entry: `Resolve(pluginRoot, plugin, host, key)` reads the load context and the assembly version off the plugin root, folds the optional host probe, and lands the identity on the rail; `ContentRoot(Assembly)` is the ONE spelling of the directory read both boundaries hand-wrote byte-identically.
- Law: `PluginSlot` is the owner-declared dimension key beside `CorrelationId.Slot` and `TenantContext.TenantSlot` — a bare noun at an emitting seam forks the dimension vocabulary (branch RULINGS `[02]`).
- Law: package self-identity homes at the kernel causal frame, so a distant emitter never hand-spells a string-typed scope for the receipt port.
- Law: this owner resolves identity and mints no meter — the metered identity is `Domain/instrument`'s `TelemetryIdentity`, and merging them puts a semantic-convention pin on a value the host resolves at load time.
- Packages: LanguageExt.Core, BCL inbox (`System.Reflection`, `System.Runtime.Loader`).
- Growth: a new host boundary is one instantiation naming its key and its host-fact type; a new resolved column extends the record and both boundaries answer it.
- Boundary: the assembly, its load context, and its host snapshot are the boundary's material — this owner reads them and holds none of them live, so a retired plugin's identity carries no reference keeping its context alive.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Reflection;
using System.Runtime.Loader;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PackageIdentity<TKey, THostFact>(
    TKey Plugin,
    Version Version,
    string ContentRoot,
    AssemblyLoadContext Alc,
    Option<THostFact> Host)
    where TKey : notnull {

    // `PluginSlot` seats the dimension key beside `CorrelationId.Slot` and `TenantContext.TenantSlot`: owner-declared, never a bare noun a distant emitter re-spells.
    public const string PluginSlot = "rasm.plugin";

    // Host probes ride OPTIONAL and railed: a boundary with no host facts passes nothing and lands `None`,
    // while a boundary whose probe refuses fails the whole resolve rather than publishing a half identity.
    [BoundaryAdapter]
    public static Fin<PackageIdentity<TKey, THostFact>> Resolve(
        Assembly pluginRoot,
        TKey plugin,
        Option<Func<Op, Fin<Option<THostFact>>>> host = default,
        Op? key = null);

    // ONE spelling of the directory read both boundaries hand-wrote byte-identically.
    public static string ContentRoot(Assembly pluginRoot) =>
        Path.GetDirectoryName(pluginRoot.Location) is { Length: > 0 } held ? held : AppContext.BaseDirectory;
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
