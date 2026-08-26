# `frame.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/frame.md`

Only changes proved by the target fence, the checked-in branch and folder `.api` tiers, settled C# law, and current consumers survive. The moves are dependency-ordered. Projected target-fence change: **-9 C# LOC** and **-9 net declared members**. Four unused generated smart-enum dispatch families disappear, `TenantId` gains one canonical admission rail, and `PackageIdentity` loses one accidental public helper surface. No new type or helper is introduced.

## Move 1 — Make hexadecimal text the complete `TenantId` text surface

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:86` — the `[ValueObject<UInt128>(` declaration above `TenantId`.

### From

```csharp
[ValueObject<UInt128>(
    KeyMemberName = "Value",
    KeyMemberAccessModifier = AccessModifier.Public,
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None)]
public readonly partial struct TenantId {
    public string Text => ContentHash.Hex(Value);
```

### To

```csharp
[ValueObject<UInt128>(
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None,
    SkipIParsable = true, SkipIFormattable = true, SkipToString = true)]
public readonly partial struct TenantId {
    public string Text => ContentHash.Hex(ToValue());
    public override string ToString() => Text;
```

### Effect

- Target fenced C# LOC: **0**.
- Declared/generated surface: **-1** public `Value` property and **+1** explicit canonical `ToString()` (net 0 source-declared members); generated `IParsable<TenantId>`, `ISpanParsable<TenantId>`, and `IFormattable` contracts are suppressed, and the generated `ToString()` is replaced one-for-one by the canonical override.
- The generated `ToValue()` remains the sole raw-`UInt128` escape. `Text`, `ToString()`, and text admission all use the same 32-lowercase-hex alphabet instead of also advertising decimal `UInt128` parsing/rendering.

### API / consumer proof

`libs/dotnet/.api/api-thinktecture-runtime-extensions.md:145-186` proves `ToValue()`, the default private `_value` key member, and the three generation opt-outs. `libs/dotnet/Rasm.AppHost/.planning/Agent/identity.md:538` calls `TenantId.ToString()`, so suppression requires the canonical override. The only code read of the current public key member is `libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:645`.

### Ripples

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:645`:

```csharp
// From
byte[] tenant = ColumnCell.Packed(frame.Tenant.TenantId.Value);
// To
byte[] tenant = ColumnCell.Packed(frame.Tenant.TenantId.ToValue());
```

At `libs/dotnet/Rasm.Persistence/.planning/Element/graph.md:174,181`, replace the two prose references to `TenantId.Value` with `TenantId.ToValue()`.

## Move 2 — Collapse trusted/untrusted text twins onto one typed admission

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:94-96` — `TenantId.Of` and `TenantId.TryOf`.

### From

```csharp
public static TenantId Of(ReadOnlySpan<char> text) => Create(ContentHash.Admit(text, Op.Of()).ThrowIfFail());
public static Option<TenantId> TryOf(ReadOnlySpan<char> text) => ContentHash.Admit(text, Op.Of()).ToOption().Map(Create);
```

### To

```csharp
public static Fin<TenantId> Admit(ReadOnlySpan<char> text, Op? key = null) => ContentHash.Admit(text, key.OrDefault()).Map(Create);
```

### Effect

- Target fenced C# LOC: **-1**.
- Type members: **-1** public entrypoint; `Of` and `TryOf` become one `Admit` rail.
- The owner preserves typed failure. A trusted persistence edge chooses `ThrowIfFail`; a best-effort ambient read chooses `ToOption`; a validating wire edge maps the fault into its own wire vocabulary. Trust no longer mints a second owner entrypoint.

### API / consumer proof

`libs/dotnet/.api/api-languageext.md:129-131` verifies `Fin.ToOption()` and `Fin.ToValidation()`. `Domain/validation.md:256-261,347` owns `Op?.OrDefault()`, and branch law permits that optional key only on public entries. `ContentHash.Admit` remains the one hexadecimal authority; the move changes only where callers lower its `Fin`.

### Ripples

`libs/dotnet/Rasm.Persistence/.planning/Element/identity.md:117`:

```csharp
// From
static tenant => tenant.Text, static text => TenantId.Of(text));
// To
static tenant => tenant.Text, static text => TenantId.Admit(text).ThrowIfFail());
```

`libs/dotnet/Rasm.AppHost/.planning/Observability/telemetry.md:103`:

```csharp
// From
.Bind(static text => TenantId.TryOf(text).Map(id => new TenantContext(id, text)))
// To
.Bind(static text => TenantId.Admit(text).ToOption().Map(id => new TenantContext(id, text)))
```

`libs/dotnet/Rasm.AppHost/.planning/Runtime/ports.md:504-508`:

```csharp
// From
: TenantId.TryOf(wire).ToValidation<Error>(Violation(new WireViolation.Tenant(wire))).Map(static tenant => Some(tenant));
// To
: TenantId.Admit(wire).MapFail(_ => Violation(new WireViolation.Tenant(wire))).ToValidation().Map(static tenant => Some(tenant));
```

At `frame.md:69`, replace the trusted/untrusted entrypoint split with the carrier-lowering rule above.

## Move 3 — Generate only the smart-enum surfaces these key vocabularies own

### Location A

`libs/dotnet/Rasm/.planning/Domain/frame.md:32` — `[SmartEnum<string>]` above `TelemetrySource`.

### From

```csharp
[SmartEnum<string>]
```

### To

```csharp
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
```

### Location B

`libs/dotnet/Rasm/.planning/Domain/frame.md:99` — `[SmartEnum<string>]` above `SessionCoordinate`.

### From

```csharp
[SmartEnum<string>]
```

### To

```csharp
[SmartEnum<string>(KeyMemberName = "Guc", SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
```

### Effect

- Target fenced C# LOC and declared members: **0**.
- Generated surface: exactly four generated dispatch families disappear — `Switch*` and `Map*` on each of the two key-only vocabularies; `SessionCoordinate.Key` becomes `SessionCoordinate.Guc` without an alias.
- Rows, `Items`, key lookup, equality, comparison, and conversion remain. Only case-specific behavioral dispatch disappears; neither owner carries case behavior to dispatch.

### API / consumer proof

`libs/dotnet/.api/api-thinktecture-runtime-extensions.md:145-186` proves `KeyMemberName` and independent `SwitchMethods` / `MapMethods` suppression. No corpus consumer dispatches either owner. `libs/dotnet/Rasm.Persistence/.planning/Element/identity.md:1075,1078` already consumes `.Tenant.Guc` and `.Plane.Guc`; no consumer reads `SessionCoordinate.*.Key`. The current fence therefore fails to generate the consumed member while generating two unused behavioral families.

### Ripples

None. The Persistence consumers become valid without a compatibility alias.

## Move 4 — Collapse the partition Boolean into the optional key

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:130-134` — `Partitions` through `Key`.

### From

```csharp
public bool Partitions => !Equals(Root);
public string Entry => TenantId.Text;
public Option<string> Key => Partitions ? Some(Entry) : None;
```

### To

```csharp
public string Entry => TenantId.Text;
public Option<string> Key => !Equals(Root) ? Some(Entry) : None;
```

### Effect

- Target fenced C# LOC: **-1**.
- Type members: **-1** public `Partitions` property.
- `Option<string>` remains the one partition discriminant; a parallel Boolean can no longer be recombined differently by consumers.

### API / consumer proof

`libs/dotnet/Rasm/RULINGS.md:66` settles `TenantContext.Key` as the one optional tenancy read. Corpus search finds one external `Partitions` use and no independent policy keyed on that Boolean.

### Ripples

`libs/dotnet/Rasm.AppHost/.planning/Runtime/ports.md:163`:

```csharp
// From
new(correlation, HostWire.Stamp(stamp), tenant.Partitions ? Some(tenant.TenantId) : None, violations);
// To
new(correlation, HostWire.Stamp(stamp), tenant.Key.Map(_ => tenant.TenantId), violations);
```

Root still yields `None`; every partitioned tenant yields its typed `TenantId`.

## Move 5 — Project the optional tenant tag directly

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:136-137` — `TenantContext.Tags`.

### From

```csharp
public Seq<KeyValuePair<string, object?>> Tags =>
    Key.Map(static entry => Seq(new KeyValuePair<string, object?>(TenantSlot, entry))).IfNone(Seq<KeyValuePair<string, object?>>());
```

### To

```csharp
public Seq<KeyValuePair<string, object?>> Tags => Key.Map(
    static entry => new KeyValuePair<string, object?>(TenantSlot, entry)).ToSeq();
```

### Effect

- Target fenced C# LOC and members: **0**.
- Removes the intermediate `Option<Seq<T>>`, explicit empty sequence, and manual flatten. `Option<T>.ToSeq()` directly expresses the zero-or-one tag cardinality.

### API / consumer proof

`libs/dotnet/.api/api-languageext.md:159,574` verifies `Option<T>.ToSeq()`. Direct consumers at `Domain/instrument.md:365`, `Rasm.AppHost/Runtime/laneguard.md:311`, and `Rasm.AppHost/Wire/outbound.md:636` prove that deleting or relocating `Tags` would be false refinement.

### Ripples

None. Root remains empty; a partition remains one `(TenantSlot, Entry)` pair.

## Move 6 — Use the sole host-null bridge and delete duplicate causal tenant state

### Location A

`libs/dotnet/Rasm/.planning/Domain/frame.md:113-118` — `TenantMirror.Span.Write`.

### From

```csharp
Write: static entry => ignore(Activity.Current?.SetBaggage(
    TenantContext.TenantSlot,
    entry.Match<string?>(Some: static held => held, None: static () => null))))
```

### To

```csharp
Write: static entry => ignore(Activity.Current?.SetBaggage(
    TenantContext.TenantSlot, Op.ToHostSlot(entry)))
```

### Location B

`libs/dotnet/Rasm/.planning/Domain/frame.md:184` — `CausalStamp` declaration.

### From

```csharp
public sealed record CausalStamp(TraceCarrier Trace, TenantContext Tenant, HlcStamp Clock, Instant Recorded) {
```

### To

```csharp
public sealed record CausalStamp(TraceCarrier Trace, HlcStamp Clock, Instant Recorded) {
```

### Location C

`libs/dotnet/Rasm/.planning/Domain/frame.md:191-197` — `CausalStamp.Now`.

### From

```csharp
TenantContext tenant = TenantContext.Current;
Instant wall = clock.Wall;
TraceCarrier trace = Activity.Current is { } span
    ? TraceCarrier.Of(span)
    : TraceCarrier.Admit(null, null, tenant.Key.Map(entry => $"{TenantContext.TenantSlot}={entry}").Match<string?>(Some: static held => held, None: static () => null));
return new(Trace: trace, Tenant: tenant, Clock: clock.Stamp(wall), Recorded: wall);
```

### To

```csharp
Instant wall = clock.Wall;
TraceCarrier trace = Activity.Current is { } span
    ? TraceCarrier.Of(span)
    : TraceCarrier.Admit(null, null, Op.ToHostSlot(TenantContext.Current.Key.Map(
        static entry => $"{TenantContext.TenantSlot}={entry}")));
return new(Trace: trace, Clock: clock.Stamp(wall), Recorded: wall);
```

### Effect

- Target fenced C# LOC: **-1**.
- Type members: **-1** positional `CausalStamp.Tenant`; constructor arity falls from four to three.
- Two handwritten `Option<string> -> string?` eliminations collapse onto the branch bridge. The emitted causal frame is unchanged: tenancy already leaves through `Trace.Baggage`, and no consumer reads a second `TenantContext` copy.

### API / consumer proof

`libs/dotnet/.planning/RULINGS.md:34` and `Domain/results.md:60-61` make `Op.ToHostSlot` the only reference-type `Option<T> -> T?` boundary projection. `Domain/event.md:284-293,434-436` consumes only `stamp.Slots` and `stamp.Clock.Physical`; `Slots` publishes tenancy through `Trace.Baggage`. No corpus consumer reads `CausalStamp.Tenant`.

### Ripples

`libs/dotnet/Rasm.AppHost/.planning/Observability/telemetry.md:80`:

```csharp
// From
Write: static entry => ignore(Baggage.SetBaggage(TenantContext.TenantSlot,
    entry.Match<string?>(Some: static held => held, None: static () => null))));
// To
Write: static entry => ignore(Baggage.SetBaggage(
    TenantContext.TenantSlot, Op.ToHostSlot(entry)));
```

At `frame.md:148`, remove the claim that `CausalStamp` stores a parallel ambient-tenant column; name creation-time trace/baggage instead. Update `Domain/event.md:715` from `trace · tenant · Hlc` to `trace/baggage · Hlc`.

## Move 7 — Inline the one-tick HLC overflow quantum

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:168,179` — `HlcStamp.TickQuantum` and its sole read in `Advance`.

### From

```csharp
private const long TickQuantum = 1L;
```

```csharp
: top.Logical == ulong.MaxValue ? new(top.Physical + Duration.FromTicks(TickQuantum), 0UL)
```

### To

```csharp
: top.Logical == ulong.MaxValue ? new(top.Physical + Duration.FromTicks(1L), 0UL)
```

### Effect

- Target fenced C# LOC: **-1**.
- Type members: **-1** private constant.
- `Duration.FromTicks(1L)` states both the quantity and unit at the only use. The HLC transition, overflow posture, and wire quantum remain identical.

### API / consumer proof

`libs/dotnet/.api/api-nodatime.md:109` verifies `Duration.FromTicks(long)` as the duration-unit mint. Corpus search finds no second `TickQuantum` read. The target law at `frame.md:150-151` fixes this exact one-Unix-tick quantum, so the literal is not an independent policy needing a member.

### Ripples

None.

## Move 8 — Make `Slots` the sole causal-slot correspondence

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:185-205` — the five `*Slot` constants and `CausalStamp.Slots`.

### From

```csharp
public const string TraceparentSlot = "traceparent";
public const string TracestateSlot = "tracestate";
public const string BaggageSlot = "baggage";
public const string SequenceSlot = "sequence";
public const string RecordedtimeSlot = "recordedtime";
```

```csharp
public Seq<(string Slot, Option<object> Value)> Slots => Seq(
    (TraceparentSlot, Optional(Trace.TraceParent).Map(static held => (object)held)),
    (TracestateSlot, Optional(Trace.TraceState).Map(static held => (object)held)),
    (BaggageSlot, Trace.Baggage.Map(static held => (object)held.Value)),
    (SequenceSlot, Some((object)Clock.Sequence)),
    (RecordedtimeSlot, Some((object)Recorded.ToDateTimeOffset())));
```

### To

```csharp
public Seq<(string Slot, Option<object> Value)> Slots => Seq(
    ("traceparent", Optional(Trace.TraceParent).Map(static held => (object)held)),
    ("tracestate", Optional(Trace.TraceState).Map(static held => (object)held)),
    ("baggage", Trace.Baggage.Map(static held => (object)held.Value)),
    ("sequence", Some((object)Clock.Sequence)),
    ("recordedtime", Some((object)Recorded.ToDateTimeOffset())));
```

### Effect

- Target fenced C# LOC: **-5**.
- Type members: **-5** public constants.
- The row table becomes the primary correspondence for both external slot identity and value. The deleted constants were single-use aliases, not an independently consumed vocabulary.

### API / consumer proof

No corpus consumer references any `CausalStamp.*Slot` constant. `Domain/event.md:284-293` iterates `stamp.Slots` and validates each row name against the generated protobuf descriptor before writing, so the same five strings remain boundary-checked at the actual consumer.

### Ripples

None. Row order and emitted names/values remain byte-identical.

## Move 9 — Remove the accidental public `ContentRoot` helper surface

### Location

`libs/dotnet/Rasm/.planning/Domain/frame.md:237-254` — `PackageIdentity`'s positional `ContentRoot` property and same-named static method.

### From

```csharp
public static string ContentRoot(Assembly pluginRoot) =>
    Path.GetDirectoryName(pluginRoot.Location) is { Length: > 0 } held ? held : AppContext.BaseDirectory;
```

### To

```csharp
private static string RootOf(Assembly pluginRoot) =>
    Path.GetDirectoryName(pluginRoot.Location) is { Length: > 0 } held ? held : AppContext.BaseDirectory;
```

### Effect

- Target fenced C# LOC and total members: **0**.
- Public surface: **-1** public helper; private surface: **+1** renamed implementation member.
- Repairs the same-type collision between the record-generated instance property `ContentRoot` and the static method named `ContentRoot`. The helper is callable only by the authored `Resolve` body that owns it.

### API / consumer proof

The positional record declaration generates an instance `ContentRoot` property. C# does not permit that property and a method of the same type to share the identifier. Corpus search finds no external `PackageIdentity.ContentRoot(assembly)` call; both host boundaries consume `identity.ContentRoot` as the record value and enter through `Resolve`.

### Ripples

The authored `Resolve` body calls `RootOf(pluginRoot)` instead of `ContentRoot(pluginRoot)`. At `frame.md:221`, rename the helper in the entry description. No consumer fence changes.

## Rejected temptations

- Do not inline `HlcStamp.Advance` or `Origin` into `Hlc.cell`. `Advance` is the pure HLC transition owned by the value, while `Hlc` owns CAS storage; `Origin` is also consumed as the cell seed. Move 7 removes only the single-use scalar alias inside that transition. `Atom.Swap` replay-safety remains satisfied by the pure helper.
- Do not delete `TenantContext.Tags`: it has multiple direct consumers. Move 5 reduces its carrier expression without moving ownership.
- Do not remove `CorrelationId.None`: current consumers in Compute, Persistence, Element, and Fabrication use it as an explicit absent-correlation posture; substituting `default` only hides the same sentinel.
- Do not replace `SessionCoordinate` with constants: it is a real closed vocabulary across session/RLS. Move 3 narrows its generated shape and supplies the consumed key name.
- Do not remove `TenantContext.Entry`, `HlcStamp.Packed`, or `HlcStamp.Sequence`; each has multiple consumers and owns a canonical text or wire projection.
- Do not inline `CausalStamp.Slots` into `Domain/event.md`; it is the frame-owned primary correspondence. Move 8 removes only the unused alias members around it.
- Do not merge `TenantMirror` into a closed smart enum. AppHost supplies an OpenTelemetry mirror at composition, so the record is the lawful open row shape.
