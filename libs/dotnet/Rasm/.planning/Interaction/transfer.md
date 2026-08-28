# [RASM_TRANSFER]

`Rasm.Interaction` owns MIME admission, the closed payload family written directly to `IDataObject`, and drop resolution inside the host event frame. Clipboard and drag data remain host values, so typed reads retain their native result and writes dispatch from the payload case without a mirrored request or outcome surface.

An image or native resource carries its `Lease<T>` with it, and the caller retains custody while sequencing writes. A host event argument never escapes: drop resolution reads and settles it synchronously, while deferred consumers receive copied facts only.

## [01]-[INDEX]

- [02]-[MIME]: `Mime` — the admitted format key every keyed slot reads.
- [03]-[PAYLOAD]: `PayloadSlot` — the closed slot family with host-shaped reads and writes.
- [05]-[DRAG]: `Drop` — the event-frame drop effect gate.

## [02]-[MIME]

- Owner: `Mime` `[ValueObject<string>]` — the format key on every keyed payload slot.
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
            && value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '/' or '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~')
            ? null
            : new ValidationError(message: $"Mime requires the type/subtype grammar: {value}");
    }
}
```

## [03]-[PAYLOAD]

- Owner: `PayloadSlot` the closed payload family with total one-shot release.
- Cases: `Text`, `Html`, `Image(Lease<Image>)`, `Uris(Seq<Uri>)`, `Bytes(Mime, Arr<byte>)`, `String(Mime, string)`, `Boxed(Mime, object)`, `Resource(Mime, Lease<IDisposable>)`. The four well-known shapes carry no format because the platform names them; the four keyed shapes carry their `Mime` because the caller does.
- Entry: `PayloadSlot.Box(format, value)` is the ONE `Boxed` mint and refuses a disposable, which belongs on `Resource` with its lease.
- Law: disposal is TOTAL over the family — every case answers `Dispose`, the lease-carrying cases close their lease and the inert cases no-op, so a write leg that refuses mid-roster disposes what it staged. A per-case `using` at each call site is the deleted form.
- Law: writes traverse `PayloadSlot.Write` monadically because every setter mutates the same target in order; custody releases staged payloads on failure.
- Law: `Boxed` is the ONE erased case, and its runtime type derives from `Value` wherever a consumer needs it.
- Law: `Boxed` refuses an `IDisposable` at admission, before any format reaches the platform. `Boxed` carries no custody, so a disposable admitted there is a handle the write leg can neither close on refusal nor hand back on read; `Resource` is where a disposable payload rides, with its lease.
- Packages: Eto.Drawing for `Image` (aliased in the prelude); LanguageExt.Core for `Lease`, `Seq`, `Arr`.
- Growth: a new payload kind is one case, breaking every total dispatch loudly.
- Boundary: a native handle never rides `Bytes` — it rides `Resource` with its lease, so the owner that acquired it is the owner that closes it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using EtoImage = Eto.Drawing.Image;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PayloadSlot : IDisposable {
    private PayloadSlot() { }

    public sealed record Text(string Value) : PayloadSlot;
    public sealed record Html(string Value) : PayloadSlot;
    public sealed record Image(Lease<EtoImage> Value) : PayloadSlot;
    public sealed record Uris(Seq<Uri> Value) : PayloadSlot;
    public sealed record Bytes(Mime Format, Arr<byte> Value) : PayloadSlot;
    public sealed record String(Mime Format, string Value) : PayloadSlot;
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

    public void Dispose() => ignore(Switch(
        text:     static _ => unit,
        html:     static _ => unit,
        image:    static slot => slot.Value.Dispose(),
        uris:     static _ => unit,
        bytes:    static _ => unit,
        @string:  static _ => unit,
        boxed:    static _ => unit,
        resource: static slot => slot.Value.Dispose()));
}
```

## [05]-[DRAG]

- Owner: `Drop` the event-frame effect gate.
- Entry: `Drop.Resolve(source, effect, description)` gates the caller's chosen effect against the permitted set and settles inside the host callback.
- Law: the host event argument NEVER escapes — the boundary resolves it synchronously, and deferred consumers receive copied facts only.
- Law: the resolved effect is GATED against `AllowedEffects` — a caller choosing an effect the source never permitted refuses typed rather than silently landing a copy where a move was offered.
- Boundary: a drag call site stages and writes a leased `DataObject`, selects the image overload from the optional image value, and observes the final effect from `DragEnd`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [SERVICES] ------------------------------------------------------------------------
public static class Drop {
    public static Fin<Unit> Resolve(
        DragEventArgs source, DragEffects effect,
        Option<(string Format, string Inner)> description = default) =>
        from _ in guard(
            (source.AllowedEffects & effect) == effect,
            new KernelFault.InvalidInput())
        from settled in Try.lift(() => {
            description.Iter(row => source.SetDropDescription(row.Format, row.Inner));
            source.Effects = effect;
            return unit;
        }).Run()
        select settled;
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
