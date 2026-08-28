# 1. Admit the complete media-type token grammar

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:33` (`[02]-[MIME]`, `Mime.ValidateFactoryArguments`)

From:

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
    value = value?.Trim().ToLowerInvariant() ?? string.Empty;
    validationError = value.Split('/') is [var type, var subtype]
        && type.Length > 0 && subtype.Length > 0
        && value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '/' or '.' or '-' or '+' or '_')
        ? null
        : new ValidationError(message: $"Mime requires the type/subtype grammar: {value}");
}
```

To:

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
    value = value?.Trim().ToLowerInvariant() ?? string.Empty;
    validationError = value.Split('/') is [var type, var subtype]
        && type.Length > 0 && subtype.Length > 0
        && value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '/' or '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~')
        ? null
        : new ValidationError(message: $"Mime requires the type/subtype grammar: {value}");
}
```

Why: The current whitelist rejects valid media-type token characters even though the validation message claims the complete `type/subtype` grammar.

Change: Retain admission-time canonicalization and ordinal comparison while admitting the standard token character set.

Delta: 0 code LOC; no module-level symbol, member, or type change.

# 2. Delete the absent-read policy

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:68` (`[03]-[PAYLOAD]`, `PayloadPresence`)

From:

```csharp
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
```

To:

```csharp
// PayloadPresence DELETED
```

Why: `Option<T>` already represents an optional read, while a required consumer can convert the same value with `ToFin(new UiFault.AbsentPayload(format))`; the generated row adds a second absence vocabulary and its constructor delegates have the wrong arity for `Settle`.

Change: Remove the policy type and let the consumer select `Option<T>` or `Fin<T>` at the read boundary.

Delta: −11 code LOC, −1 module-level type, and −3 declared members.

# 3. Delete the duplicate well-known-format roster

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:81` (`[03]-[PAYLOAD]`, `WellKnownFormat`)

From:

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WellKnownFormat : ICapability<WellKnownFormat> {
    public static readonly WellKnownFormat Text = new(key: "text", rank: 0);
    public static readonly WellKnownFormat Html = new(key: "html", rank: 1);
    public static readonly WellKnownFormat Picture = new(key: "image", rank: 2);
    public static readonly WellKnownFormat Linked = new(key: "uris", rank: 3);

    public int Rank { get; }
}
```

To:

```csharp
// WellKnownFormat DELETED
```

Why: `IDataObject` already owns `ContainsText`, `ContainsHtml`, `ContainsImage`, and `ContainsUris`; a second keyed vocabulary with handwritten aliases contributes neither admission nor behavior.

Change: Read the host probes directly where inventory is required.

Delta: −9 code LOC, −1 module-level type, and −5 declared members.

# 4. Delete the read-side payload mirror

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:92` (`[03]-[PAYLOAD]`, `PayloadShape`)

From:

```csharp
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
```

To:

```csharp
// PayloadShape DELETED
```

Why: The union repeats every payload case without its value and erases the static result types already exposed by `IDataObject` typed properties and keyed readers.

Change: Read through the corresponding host member and admit its nullable result directly into `Option<T>` or `Fin<T>`.

Delta: −13 code LOC, −1 module-level type, and −9 nested case types.

Ripples: Remove `PayloadShape` from the retired-wrapper mapping in `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md:107`.

# 5. Use the host payload names and remove copied streams

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:110` (`[03]-[PAYLOAD]`, `PayloadSlot` cases)

From:

```csharp
public sealed record Text(string Value) : PayloadSlot;
public sealed record Html(string Value) : PayloadSlot;
public sealed record Picture(Lease<EtoImage> Value) : PayloadSlot;
public sealed record Linked(Seq<Uri> Value) : PayloadSlot;
public sealed record Bytes(Mime Key, Arr<byte> Value) : PayloadSlot;
public sealed record Streamed(Mime Key, Lease<Stream> Value) : PayloadSlot;
public sealed record Stringed(Mime Key, string Value) : PayloadSlot;
```

To:

```csharp
public sealed record Text(string Value) : PayloadSlot;
public sealed record Html(string Value) : PayloadSlot;
public sealed record Image(Lease<EtoImage> Value) : PayloadSlot;
public sealed record Uris(Seq<Uri> Value) : PayloadSlot;
public sealed record Bytes(Mime Format, Arr<byte> Value) : PayloadSlot;
public sealed record String(Mime Format, string Value) : PayloadSlot;
```

Why: `Picture`, `Linked`, and `Stringed` diverge from Eto's `Image`, `Uris`, and string surface, while both concrete `SetDataStream` implementations copy the complete stream into a byte array before `SetData`.

Change: Align case and field names with the host contract and represent binary transfer once as `Bytes`.

Delta: −1 code LOC and −1 nested case type; no module-level type or member change.

# 6. Make the erased payload factory the only construction path

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:117` (`[03]-[PAYLOAD]`, `Boxed`, `Resourced`, and `Box`)

From:

```csharp
public sealed record Boxed(Mime Key, Type Carried, object Value) : PayloadSlot;
public sealed record Resourced(Mime Key, Lease<IDisposable> Value) : PayloadSlot;

public static Fin<PayloadSlot> Box(Mime key, object value) =>
    from held in Admit.Need(value: value)
    from owned in guard(held is not IDisposable, new KernelFault.InvalidInput())
    select (PayloadSlot)new Boxed(Carried: held.GetType(), Value: held);
```

To:

```csharp
public sealed record Boxed : PayloadSlot {
    private Boxed(Mime format, object value) => (Format, Value) = (format, value);
    public Mime Format { get; }
    public object Value { get; }
}
public sealed record Resource(Mime Format, Lease<IDisposable> Value) : PayloadSlot;

public static Fin<PayloadSlot> Box(Mime format, object? value) =>
    from held in Admit.Need(value)
    from _ in guard(held is not IDisposable, new KernelFault.InvalidInput())
    select (PayloadSlot)new Boxed(format, held);
```

Why: The public positional constructor bypasses the disposable refusal, `Carried` duplicates `Value.GetType()`, and the current factory omits the required format argument.

Change: Seal erased-object admission behind `Box`, retain the format, and remove the duplicated runtime type.

Delta: +4 code LOC and −1 stored data member; no module-level type or declared member change.

Ripples: Rename `Resourced` to `Resource` in `libs/dotnet/Rasm/RULINGS.md:81` and in the transfer implementation's exhaustive dispatch.

# 7. Put host writing on the payload owner

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:125` (`[03]-[PAYLOAD]`, `PayloadSlot.Shape` and `PayloadSlot.Named`)

From:

```csharp
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
```

To:

```csharp
public Fin<Unit> Write(IDataObject target) =>
    Try.lift(() => Switch(
        state: target,
        text:     static (to, slot) => { to.Text = slot.Value; return unit; },
        html:     static (to, slot) => { to.Html = slot.Value; return unit; },
        image:    static (to, slot) => { to.Image = slot.Value.Resource; return unit; },
        uris:     static (to, slot) => { to.Uris = [.. slot.Value]; return unit; },
        bytes:    static (to, slot) => { to.SetData(slot.Value.ToArray(), slot.Format); return unit; },
        @string:  static (to, slot) => { to.SetString(slot.Value, slot.Format); return unit; },
        boxed:    static (to, slot) => { to.SetObject(slot.Value, slot.Format); return unit; },
        resource: static (to, slot) => { to.SetObject(slot.Value.Resource, slot.Format); return unit; })).Run();
```

Why: `Shape` is an uninitialized mirror and `Named` restates the case discriminator; writing is the cohesive behavior that needs exhaustive generated dispatch and host-exception normalization.

Change: Replace both projections with one per-slot write operation over the shared `IDataObject` contract.

Delta: +1 code LOC and −1 declared member net; no module-level type change.

# 8. Implement exhaustive payload release

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:138` (`[03]-[PAYLOAD]`, `PayloadSlot.Dispose`)

From:

```csharp
public void Dispose();
```

To:

```csharp
public void Dispose() => ignore(Switch(
    text:     static _ => unit,
    html:     static _ => unit,
    image:    static slot => slot.Value.Dispose(),
    uris:     static _ => unit,
    bytes:    static _ => unit,
    @string:  static _ => unit,
    boxed:    static _ => unit,
    resource: static slot => slot.Value.Dispose()));
```

Why: A non-abstract method declaration without a body does not compile, and generated dispatch can express total release without a case ladder at callers.

Change: Release only the two lease-bearing cases through exhaustive dispatch.

Delta: +8 code LOC; no module-level symbol, member, or type change.

# 9. Delete the transfer-surface wrapper

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:166` (`[04]-[SURFACE]`, `TransferSurface`)

From:

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferSurface {
    private TransferSurface() { }
    public sealed record Board(Lease<Clipboard> Value) : TransferSurface;
    public sealed record Bundle(Lease<DataObject> Value) : TransferSurface;

    public static Fin<TransferSurface> System();
    public static Fin<TransferSurface> Staged();
}
```

To:

```csharp
// TransferSurface DELETED
```

Why: The union exists only to rename two existing host values; `Clipboard.Instance` is borrowed process state, while a staged `DataObject` already carries concrete disposal through `Lease<DataObject>`.

Change: Pass `IDataObject` to payload writes, read the clipboard directly, and keep a staged data object in its concrete lease.

Delta: −8 code LOC, −1 module-level type, −2 nested case types, and −2 declared factory members.

# 10. Delete the host-operation mirror

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:176` (`[04]-[SURFACE]`, `TransferOp`)

From:

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferOp {
    private TransferOp() { }
    public sealed record Read(TransferSurface At, PayloadShape Shape, PayloadPresence Presence) : TransferOp;
    public sealed record Write(TransferSurface At, Seq<PayloadSlot> Slots) : TransferOp;
    public sealed record Probe(TransferSurface At) : TransferOp;
    public sealed record Clear(TransferSurface At) : TransferOp;
    public sealed record Drag(Control Source, DragPlan Plan) : TransferOp;

}
```

To:

```csharp
// TransferOp DELETED
```

Why: Four cases duplicate `IDataObject` members, and the drag case has event-delivered completion semantics that do not share their result type.

Change: Use direct typed reads, probes, and clear; traverse payload writes monadically because they mutate one target in order; start drag through `Control.DoDragDrop`.

Delta: −9 code LOC, −1 module-level type, and −5 nested case types.

# 11. Delete non-transactional write evidence

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:188` (`[04]-[SURFACE]`, `TransferWriteFact`)

From:

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferWriteFact : IValidityEvidence {
    private TransferWriteFact() { }
    public sealed record Committed(PayloadShape Slot) : TransferWriteFact;
    public sealed record Rejected(PayloadShape Slot, Error Cause) : TransferWriteFact;

    public bool IsValid => ValidityClaim.All(this is Committed);
}
```

To:

```csharp
// TransferWriteFact DELETED
```

Why: Sequential setters on one mutable `IDataObject` provide no rollback, so a roster of committed and rejected rows cannot substantiate the stated all-or-nothing result.

Change: Sequence `PayloadSlot.Write` with `TraverseM` and return its first typed failure, releasing staged payloads on that failure path through `Custody`.

Delta: −7 code LOC, −1 module-level type, −2 nested case types, and −1 declared member.

# 12. Delete the result-erasing outcome union

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:197` (`[04]-[SURFACE]`, `TransferOutcome`)

From:

```csharp
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
```

To:

```csharp
// TransferOutcome DELETED
```

Why: The union discards the static relation between each operation and its native result, duplicates `Option` for reads, and adds derived write members over evidence that cannot prove atomicity.

Change: Keep each direct host operation's result under its own `Option<T>` or `Fin<T>` carrier and observe the final drag effect from `DragEnd`.

Delta: −13 code LOC, −1 module-level type, −5 nested case types, and −2 declared convenience members.

# 13. Delete the single-dispatch transfer entry

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:212` (`[04]-[SURFACE]`, `Transfer`)

From:

```csharp
public static class Transfer {
    public static ValueTask<Fin<TransferOutcome>> Apply(TransferOp operation);
}
```

To:

```csharp
// Transfer DELETED
```

Why: Once the mirrored request and outcome unions are removed, `Apply` only forwards synchronous host members through an unnecessary `ValueTask`.

Change: Call the typed host member or `PayloadSlot.Write` directly and compose drag completion from the existing event source.

Delta: −3 code LOC, −1 module-level type, and −1 declared member.

Ripples: Replace the kernel `Transfer` mapping in `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md:107` with direct `IDataObject` reads and `PayloadSlot.Write`.

# 14. Delete passive drag argument and result records

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:239` (`[05]-[DRAG]`, `DragPlan` and `DropOutcome`)

From:

```csharp
public sealed record DragPlan(Seq<PayloadSlot> Slots, DragEffects Permitted, Option<(EtoImage Image, EtoPointF Offset)> Ghost);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DropOutcome(EtoPointF Location, DragEffects Allowed, DragEffects Resolved);
```

To:

```csharp
// DragPlan and DropOutcome DELETED
```

Why: `DragPlan` repeats the arguments of the two `Control.DoDragDrop` overloads, while `DropOutcome` echoes values already available in the event frame.

Change: Materialize and write a leased `DataObject` at the drag call site, select the image overload from the optional image value, and return only the effect fact delivered by `DragEnd`.

Delta: −3 code LOC and −2 module-level types; no declared member change.

Ripples: Remove the obsolete `DragPlan` name-collision explanation from `libs/dotnet/Rasm.Rhino/.planning/Commands/acquisition.md:583`.

# 15. Keep drop resolution inside the event frame

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:245` (`[05]-[DRAG]`, `Drop`)

From:

```csharp
public sealed class Drop {
    public static Fin<Drop> Of(DragEventArgs provider);

    public EtoPointF Location { get; }
    public DragEffects Allowed { get; }
    public TransferSurface Payload { get; }

    public Fin<DropOutcome> Resolve(DragEffects effect, Option<(string Format, string Inner)> description = default);
}
```

To:

```csharp
public static class Drop {
    public static Fin<Unit> Resolve(
        DragEventArgs source, DragEffects effect,
        Option<(string Format, string Inner)> description = default);
}
```

Why: The stateful wrapper lets the borrowed `DataObject` outlive its event callback, and its properties duplicate the event argument instead of enforcing the only invariant-bearing operation.

Change: Retain one synchronous boundary operation that checks the selected effect against `AllowedEffects`, applies the optional drop description, and assigns `Effects` before the callback returns.

Delta: −3 code LOC and −4 declared members; no module-level type change.

Ripples: Route drop handling in `libs/dotnet/Rasm/.planning/Interaction/input.md:488` through `Drop.Resolve` inside the `DragEventArgs` callback; the deferred `UiFact.DragCase` must contain only copied facts, never `Data` or the event argument.

# 16. Remove aliases retired with the drag records

`libs/dotnet/Rasm/.planning/Interaction/transfer.md:232` (`[05]-[DRAG]`, drawing aliases)

From:

```csharp
using EtoImage = Eto.Drawing.Image;
using EtoPointF = Eto.Drawing.PointF;
```

To:

```csharp
// EtoImage and EtoPointF DELETED
```

Why: No declaration in the drag fence names either drawing type after the passive records are removed.

Change: Delete both unused aliases.

Delta: −2 code LOC; no module-level symbol, member, or type change.
