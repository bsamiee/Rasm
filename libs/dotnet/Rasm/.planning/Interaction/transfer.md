# [RASM_TRANSFER]

`Rasm.Interaction` owns the one clipboard, drag, and drop payload algebra. A payload is a closed slot family with total one-shot release, a surface is the leased board or bundle it is written to, and every verb — read, write, probe, clear, drag — is a case of one request union under one entry. Drag is a CASE, never a second entrypoint, because a drag is a write to a bundle the host carries rather than a different concern.

Custody is symmetric across the boundary: a slot carrying a stream, an image, or a native resource carries its `Lease<T>` with it, so the write leg disposes what it staged on refusal and the read leg hands the caller a lease it can close. A host event argument never escapes — the drop admits it once, publishes location, permitted effect, and payload as admitted values, and the argument is gone before any consumer sees it.

## [01]-[INDEX]

- [02]-[MIME]: `Mime` — the admitted format key every slot and probe reads.
- [03]-[PAYLOAD]: `PayloadSlot`, `PayloadShape`, `PayloadPresence`, `WellKnownFormat` — the closed slot family, its read-side mirror, the presence gate, and the platform-named format vocabulary.
- [04]-[SURFACE]: `TransferSurface`, `TransferOp`, `TransferWriteFact`, `TransferOutcome`, `Transfer` — the leased boards and the one apply entry.
- [05]-[DRAG]: `DragPlan`, `Drop`, `DropOutcome` — the drag payload plan and the admitted drop with its effect gate.

## [02]-[MIME]

- Owner: `Mime` `[ValueObject<string>]` — the format key on every payload slot, every probe, and every inventory row.
- Entry: the generated `Validate` admission over the `type/subtype` grammar — never `Create`, which throws, nor `TryCreate`, which downgrades the generated error to a bare miss — so a host format string canonicalizes and admits once at the boundary and the interior compares admitted values.
- Law: the key CANONICALIZES at admission and compares ordinally after. The standard's own rule is that type and subtype are ASCII case-insensitive, so admission lowers by `ref` before storage and one board's `TEXT/PLAIN` and another's `text/plain` land as one key; the comparison then stays ordinal and never culture-folded, because a culture-sensitive fold admits a Turkish-dotless mismatch on `text/plain`. Canonicalizing without the ordinal comparison, or comparing ordinally without canonicalizing, each strands one half of the rule.
- Growth: a new format is a value, never a row — the vocabulary is the platform's and unbounded, so this owner admits rather than enumerating.
- Boundary: the host's own format constants (`Eto` clipboard type strings, platform UTIs) enter through admission and never as raw strings past this page.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct Mime {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim().ToLowerInvariant() ?? string.Empty;
        validationError = value.Split('/') is [var type, var subtype]
            && type.Length > 0 && subtype.Length > 0
            && value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '/' or '.' or '-' or '+' or '_')
            ? null
            : new ValidationError(message: $"Mime requires the type/subtype grammar: {value}");
    }
}
```

## [03]-[PAYLOAD]

- Owner: `PayloadSlot` the closed payload family with total one-shot release; `PayloadShape` its read-side mirror naming what a caller ASKS for without carrying a value; `PayloadPresence` the two-row gate carrying the absent-read settle itself; `WellKnownFormat` the capability vocabulary over the four shapes the platform names itself.
- Cases: `Text`, `Html`, `Picture(Lease<Image>)`, `Linked(Seq<Uri>)`, `Bytes(Mime, Arr<byte>)`, `Streamed(Mime, Lease<Stream>)`, `Stringed(Mime, string)`, `Boxed(Mime, Type, object)`, `Resourced(Mime, Lease<IDisposable>)`. The four well-known shapes carry no key because the platform names them; the five keyed shapes carry their `Mime` because the caller does.
- Entry: `PayloadSlot.Box(value)` is the ONE `Boxed` mint — it derives the declared type from the value and refuses a disposable, which belongs on `Resourced` with its lease.
- Law: disposal is TOTAL over the family and runs once — every case answers `Dispose`, the value-carrying cases close their lease and the inert cases no-op, so a write leg that refuses mid-roster disposes exactly what it staged. A per-case `using` at each call site is the deleted form, and a slot disposed twice is a no-op rather than a fault.
- Law: `Boxed` is the ONE erased case and it carries its `Type` beside its `Mime`, so a consumer that must interrogate an unmodelled host payload reads the declared type rather than probing the object. The type DERIVES from the value at admission — a caller-stated type beside the value is two authorities the first mismatch cannot arbitrate.
- Law: `Boxed` refuses an `IDisposable` at admission AND at the write leg, before any format reaches the platform. `Boxed` carries no custody, so a disposable admitted there is a handle the write leg can neither close on refusal nor hand back on read; `Resourced` is where a disposable payload rides, with its lease.
- Law: the four platform-named shapes answer their `WellKnownFormat` row through a total fold and the five keyed shapes answer absence, so the write leg picks the platform's own typed setter for the first group and the keyed setter for the second without probing a case a second time. Each row's KEY is the host accessor it stands for, so the roster's stated provenance is a fact a reader can check rather than a claim beside four invented tokens.
- Law: `PayloadShape` mirrors the readable slots arm for arm — a read asks for a SHAPE and receives a SLOT, so the request cannot carry a value and the response cannot lose one.
- Law: `Required` refuses an absent read with `UiFault.AbsentPayload` carrying the wanted `Mime`; `Optional` lands `None`. The two are a ROW carrying its own `Settle`, not a boolean: a mirror bool restating the key leaves every read leg to re-derive the refusal, and the row that carries it makes this law executable rather than prose the fold has to honour.
- Packages: Eto.Drawing for `Image` (aliased in the prelude); LanguageExt.Core for `Lease`, `Seq`, `Arr`; `Domain/validation` for `ICapability` and `CapabilitySet`.
- Growth: a new payload kind is one case plus one shape row, breaking every total dispatch loudly; a new platform-named format is one `WellKnownFormat` row keyed on the accessor the host publishes; a new presence posture is one row carrying its own settle.
- Boundary: a native handle never rides `Bytes` — it rides `Resourced` with its lease, so the owner that acquired it is the owner that closes it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoImage = Eto.Drawing.Image;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PayloadPresence {
    public static readonly PayloadPresence Optional = new(key: 0,
        settle: static (found, _, _) => Fin.Succ(found));
    public static readonly PayloadPresence Required = new(key: 1,
        settle: static (found, wanted, key) => found
            .ToFin(new UiFault.AbsentPayload(Wanted: wanted))
            .Map(Some));

    [UseDelegateFromConstructor]
    internal partial Fin<Option<PayloadSlot>> Settle(Option<PayloadSlot> found, Mime wanted);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WellKnownFormat : ICapability<WellKnownFormat> {
    public static readonly WellKnownFormat Text = new(key: "text", rank: 0);
    public static readonly WellKnownFormat Html = new(key: "html", rank: 1);
    public static readonly WellKnownFormat Picture = new(key: "image", rank: 2);
    public static readonly WellKnownFormat Linked = new(key: "uris", rank: 3);

    public int Rank { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PayloadShape {
    private PayloadShape() { }
    public sealed record Text : PayloadShape;
    public sealed record Html : PayloadShape;
    public sealed record Picture : PayloadShape;
    public sealed record Linked : PayloadShape;
    public sealed record Bytes(Mime Key) : PayloadShape;
    public sealed record Streamed(Mime Key) : PayloadShape;
    public sealed record Stringed(Mime Key) : PayloadShape;
    public sealed record Boxed(Mime Key, Type Carried) : PayloadShape;
    public sealed record Resourced(Mime Key) : PayloadShape;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PayloadSlot : IDisposable {
    private PayloadSlot() { }

    public sealed record Text(string Value) : PayloadSlot;
    public sealed record Html(string Value) : PayloadSlot;
    public sealed record Picture(Lease<EtoImage> Value) : PayloadSlot;
    public sealed record Linked(Seq<Uri> Value) : PayloadSlot;
    public sealed record Bytes(Mime Key, Arr<byte> Value) : PayloadSlot;
    public sealed record Streamed(Mime Key, Lease<Stream> Value) : PayloadSlot;
    public sealed record Stringed(Mime Key, string Value) : PayloadSlot;
    public sealed record Boxed(Mime Key, Type Carried, object Value) : PayloadSlot;
    public sealed record Resourced(Mime Key, Lease<IDisposable> Value) : PayloadSlot;

    public static Fin<PayloadSlot> Box(Mime key, object value) =>
        from held in Admit.Need(value: value)
        from owned in guard(held is not IDisposable, new KernelFault.InvalidInput())
        select (PayloadSlot)new Boxed(Carried: held.GetType(), Value: held);

    public PayloadShape Shape { get; }

    public Option<WellKnownFormat> Named => Switch(
        text:      static _ => Some(WellKnownFormat.Text),
        html:      static _ => Some(WellKnownFormat.Html),
        picture:   static _ => Some(WellKnownFormat.Picture),
        linked:    static _ => Some(WellKnownFormat.Linked),
        bytes:     static _ => Option<WellKnownFormat>.None,
        streamed:  static _ => Option<WellKnownFormat>.None,
        stringed:  static _ => Option<WellKnownFormat>.None,
        boxed:     static _ => Option<WellKnownFormat>.None,
        resourced: static _ => Option<WellKnownFormat>.None);

    public void Dispose();
}
```

## [04]-[SURFACE]

- Owner: `TransferSurface` the leased destination — the system board or a per-drag bundle; `TransferOp` the closed verb family; `TransferWriteFact` the per-slot write outcome; `TransferOutcome` the closed response family; `Transfer` the one apply entry.
- Cases: `TransferOp` is `Read(At, Shape, Presence)`, `Write(At, Seq<PayloadSlot>)`, `Probe(At)`, `Clear(At)`, and `Drag(Source, DragPlan)` — five verbs, one entry, one total dispatch, and a sixth verb breaks every site.
- Entry: `Transfer.Apply(operation)` returns `ValueTask<Fin<TransferOutcome>>`; the outcome's case is recoverable from the verb, so no caller casts and no verb returns a different shape.
- Auto: a write is ALL-OR-NOTHING at the slot grain and reports per slot — `Written` carries one `TransferWriteFact` per slot with its committed-or-rejected case, so a caller reads which format the platform refused instead of one aggregate failure. `Count` and `Failure` derive from that roster and are never stored.
- Auto: the inventory's well-known presence is ONE `CapabilitySet<WellKnownFormat>` column read off the board's four probes. NAMED LOSS: the four independently named flags. Bought back by set algebra — a caller asking "does this board carry text or html" reads `AdmitsAll` over a set literal rather than composing two bools, and a fifth platform-named format is one vocabulary row rather than a fifth column on every reader.
- Law: the surface is a LEASE and every case carries it, so a bundle staged for a drag that the host never starts is closed by the caller's own custody rather than surviving as a leaked data object.
- Law: a refused write disposes every slot it staged, reverse order, every disposer running even when one throws — the total `Dispose` on the slot family is what makes that fold one expression rather than a per-case ladder.
- Law: an inventory `Probe` reports the format roster and the well-known set as MEASURED facts off the surface, never as a cached census — a board another process just wrote is stale the instant it is remembered.
- Output: `TransferOutcome` — `Read(Option<Lease<PayloadSlot>>)`, `Written(Seq<TransferWriteFact>)`, `Cleared`, `Inventory(Seq<Mime>, CapabilitySet<WellKnownFormat>)`, `Dragged(DragEffects)`.
- Packages: Eto.Forms for `Clipboard`, `DataObject`, `DragEffects` (verified in `libs/dotnet/.api/api-eto-runtime.md` and `api-eto-forms.md`); LanguageExt.Core for the types.
- Growth: a new verb is one case, one arm, and one `Key` row the generator already minted; a new outcome shape rides its verb's case.
- Boundary: NAMED LOSS — both boundaries carried their own `Transfer`, `TransferQuery`, `TransferOp`, `TransferOutcome`, `TransferWriteFact`, `TransferTarget`, `PayloadSlot`, `PayloadPresence`, `DragPlan`, and `Drop`, and every one of them deletes. What is genuinely lost is each side's bespoke naming at its own call sites; what survives is stronger, because the two implementations disagreed on whether a write reported per slot and only one of them did.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferSurface {
    private TransferSurface() { }
    public sealed record Board(Lease<Clipboard> Value) : TransferSurface;
    public sealed record Bundle(Lease<DataObject> Value) : TransferSurface;

    public static Fin<TransferSurface> System();
    public static Fin<TransferSurface> Staged();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferOp {
    private TransferOp() { }
    public sealed record Read(TransferSurface At, PayloadShape Shape, PayloadPresence Presence) : TransferOp;
    public sealed record Write(TransferSurface At, Seq<PayloadSlot> Slots) : TransferOp;
    public sealed record Probe(TransferSurface At) : TransferOp;
    public sealed record Clear(TransferSurface At) : TransferOp;
    public sealed record Drag(Control Source, DragPlan Plan) : TransferOp;

}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferWriteFact : IValidityEvidence {
    private TransferWriteFact() { }
    public sealed record Committed(PayloadShape Slot) : TransferWriteFact;
    public sealed record Rejected(PayloadShape Slot, Error Cause) : TransferWriteFact;

    public bool IsValid => ValidityClaim.All(this is Committed);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferOutcome {
    private TransferOutcome() { }
    public sealed record Read(Option<Lease<PayloadSlot>> Slot) : TransferOutcome;
    public sealed record Cleared : TransferOutcome;
    public sealed record Inventory(Seq<Mime> Formats, CapabilitySet<WellKnownFormat> Named) : TransferOutcome;
    public sealed record Dragged(DragEffects Resolved) : TransferOutcome;

    public sealed record Written(Seq<TransferWriteFact> Slots) : TransferOutcome {
        public int Count => Slots.Count(static fact => fact.IsValid);
        public Option<Error> Failure =>
            Slots.Choose(static fact => fact is TransferWriteFact.Rejected row ? Some(row.Cause) : None).Head;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Transfer {
    public static ValueTask<Fin<TransferOutcome>> Apply(TransferOp operation);
}
```

## [05]-[DRAG]

- Owner: `DragPlan` the staged drag payload with its permitted effects and optional ghost; `Drop` the admitted drop with its location, allowed effects, and payload; `DropOutcome` the settled effect.
- Entry: `Drop.Of(provider)` admits the host event argument ONCE and publishes admitted values; `Resolve(effect, description)` gates the caller's chosen effect against the permitted set and settles.
- Law: the host event argument NEVER escapes — it is read at admission and gone, so no consumer holds an argument the platform recycles after the callback returns, and no consumer reads a mutable effect field after the frame that owned it.
- Law: the resolved effect is GATED against `Allowed` — a caller choosing an effect the source never permitted refuses typed rather than silently landing a copy where a move was offered.
- Law: the ghost is an `Option` pair of image and offset, so a drag with no visual carries no half-specified placement; a null image beside a real offset is unrepresentable.
- Output: `DropOutcome` carries the location, the allowed set, and the resolved effect together, so a consumer auditing a drop reads what was offered beside what was taken.
- Growth: a new drag coordinate is one column on the plan; a new effect is the platform's own flag set, read and never mirrored.
- Boundary: the drag SOURCE control is the host's and rides the `Drag` case; the plan carries no control, so a plan is stageable before a source exists.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using EtoImage = Eto.Drawing.Image;
using EtoPointF = Eto.Drawing.PointF;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DragPlan(Seq<PayloadSlot> Slots, DragEffects Permitted, Option<(EtoImage Image, EtoPointF Offset)> Ghost);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DropOutcome(EtoPointF Location, DragEffects Allowed, DragEffects Resolved);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Drop {
    public static Fin<Drop> Of(DragEventArgs provider);

    public EtoPointF Location { get; }
    public DragEffects Allowed { get; }
    public TransferSurface Payload { get; }

    public Fin<DropOutcome> Resolve(DragEffects effect, Option<(string Format, string Inner)> description = default);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
