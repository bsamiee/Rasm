# `event.md` surgical refinement audit

Target: `libs/dotnet/Rasm/.planning/Domain/event.md`

Counting convention: every `Effect` counts nonblank C# lines inside the target's fences. Ripple edits in other spec-sheets and prose are named but excluded. Authored members, authored module-level types, compiler record surface, and generated surface are reported separately.

API authority: `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`, `api-thinktecture-json.md`, `api-languageext.md`, `api-cloudevents.md`, `api-cloudevents-protobuf.md`, `api-cloudevents-avro.md`, `api-celly-protovalidate.md`, `api-protobuf.md`, `api-generator-equals.md`, `api-nodatime.md`, `api-nodatime-stj.md`, and `api-system-text-json.md`. The complete local tier at `libs/dotnet/Rasm/.api/` adds no event-specific substrate. Public deletions were checked against the full `libs/dotnet` planning/source corpus; deliberate growth surfaces are retained even when they have no current external call site.

## 1. Remove redundant Generator.Equals surface from the mint records

### 1A. Remove the import

Location: `event.md:177`, anchor `using Generator.Equals;`.

From:

```csharp
using Generator.Equals;
```

To:

```csharp
```

### 1B. Restore ordinary record equality on `CloudEventMint`

Location: `event.md:186`, anchor `[Equatable]` above `CloudEventMint`.

From:

```csharp
[Equatable]
public sealed partial record CloudEventMint(
```

To:

```csharp
public sealed record CloudEventMint(
```

Location: `event.md:196`, anchor `[property: OrderedEquality]`.

From:

```csharp
[property: OrderedEquality] Seq<EventField> Extensions);
```

To:

```csharp
Seq<EventField> Extensions);
```

### 1C. Restore ordinary record equality on `RasmEventMint<TExtensions>`

Location: `event.md:198`, anchor `[Equatable]` above `RasmEventMint<TExtensions>`.

From:

```csharp
[Equatable]
public sealed partial record RasmEventMint<TExtensions>(
```

To:

```csharp
public sealed record RasmEventMint<TExtensions>(
```

Effect: `-3` fenced LOC; authored-member/type delta `0`; generated-type delta `-2` nested Generator.Equals comparer types and their `Inequalities` families. Compiler-synthesized record equality remains.

API/consumer proof: `api-generator-equals.md` reserves `[Equatable]` for non-default member policy or the generated diff surface. `Seq<T>` already provides structural equality, which satisfies the repository's record/`Seq` ruling; every other member remains under `EqualityComparer<T>.Default`. No consumer under `libs/dotnet` uses either generated comparer or `Inequalities` family.

Ripple: remove `Generator.Equals` from `[04]-[ENVELOPE_MINT]`'s package line at `event.md:168`.

## 2. Use the actual generated Thinktecture value projection

Location: `event.md:66`, anchor `private string Part(int index) => Value.Split('.')[index];`.

From:

```csharp
private string Part(int index) => Value.Split('.')[index];
```

To:

```csharp
private string Part(int index) => ToValue().Split('.')[index];
```

Location: `event.md:89`, anchor `public Uri Reference`.

From:

```csharp
public Uri Reference => new(uriString: Value, uriKind: UriKind.Absolute);
```

To:

```csharp
public Uri Reference => new(uriString: ToValue(), uriKind: UriKind.Absolute);
```

Location: `event.md:91`, anchor `private string Part(int index) => Value[Scheme.Length..]`.

From:

```csharp
private string Part(int index) => Value[Scheme.Length..].Split('/')[index];
```

To:

```csharp
private string Part(int index) => ToValue()[Scheme.Length..].Split('/')[index];
```

Effect: `0` fenced LOC; symbol delta `0`.

API/consumer proof: `api-thinktecture-runtime-extensions.md` proves `[ValueObject<T>]` defaults to a private key field and publishes `ToValue()` as its public projection. The two `Part` helpers stay: they serve five properties and prevent five independent split expressions.

Ripple: `libs/dotnet/Rasm.Bim/.planning/Exchange/events.md:79`, `row.Type.Value` -> `row.Type.ToValue()`.

## 3. Make Thinktecture own `EventId`

This is one atomic type replacement; 3A and 3B only split the edit into reviewable anchors.

### 3A. Replace manual storage with the generated value-object owner

Location: `event.md:94-97`, anchor `public readonly record struct EventId`.

From:

```csharp
public readonly record struct EventId {
    private EventId(string value) => Value = value;

    public string Value { get; }
```

To:

```csharp
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct EventId {
```

### 3B. Replace both manual factories and rendering with the validation hook

Location: `event.md:99-107`, anchor `public static Fin<EventId> Of`.

From:

```csharp
public static Fin<EventId> Of(string value, Op key) =>
    value.Length > 0 && !value.Any(char.IsControl)
        ? Fin.Succ(new EventId(value))
        : Fin.Fail<EventId>(new KernelFault.InvalidValue(
            Label: nameof(EventId), Requirement: "a non-empty control-free operation identity", Key: Some(key)));

public static Fin<EventId> Admit(string text, Op key) => Of(value: text, key: key);

public override string ToString() => Value;
```

To:

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
    validationError = value.Length > 0 && !value.Any(char.IsControl)
        ? null
        : new ValidationError(message: "a non-empty control-free operation identity");
```

### 3C. Route every admission through the standard validation bridge

Location: `event.md:484`, anchor `from id in EventId.Admit`.

From:

```csharp
from id in EventId.Admit(text: admitted.Id!, key: key)
```

To:

```csharp
from id in key.AcceptValidated<EventId>(admitted.Id!)
```

Effect: `11 -> 9` fenced LOC for the type, delta `-2`; authored-member delta `-4 public` (five manual members replaced by one private validation hook); module-type delta `0`. One standard Thinktecture generated value-object surface replaces the manual storage, factory, equality, conversion, and rendering surface; storage remains one `string`.

API/consumer proof: the Thinktecture catalog proves the exact `ValidateFactoryArguments` signature, ordinal comparer accessors, generated `ToValue()`/`ToString()`, and generated validation factory. `AcceptValidated<T>` maps the same validation message to `KernelFault.InvalidValue` with label `EventId` and the supplied `Op`; therefore the existing failure semantics survive without a second factory rail.

Ripples: replace each current `EventId.Of(..., key)` or `EventId.Admit(..., key)` with the owning operation's `key.AcceptValidated<EventId>(value)` in:

- `libs/dotnet/Rasm.Bim/.planning/Exchange/events.md:201`
- `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/evidence.md:186`
- `libs/dotnet/Rasm.Compute/.planning/Runtime/ledger.md:137`
- `libs/dotnet/Rasm.Element/.planning/Projection/observe.md:200`
- `libs/dotnet/Rasm.Persistence/.planning/Version/egress.md:588`
- `libs/dotnet/Rasm.Persistence/.planning/Version/ingress.md:261`

Update `[02]-[EVENT_GRAMMAR]` entry prose to name generated admission through `AcceptValidated<EventId>`.

## 4. Retain `EventField` as the typed carrier, but delete its accidental behavior surface

### 4A. Delete the unused constructor alias

Location: `event.md:230`, anchor `public static EventField Of`.

From:

```csharp
public static EventField Of(string name, CloudEventAttributeType type, object value) =>
    new(Declare(name, type), value);
```

To:

```csharp
```

Effect: `-2` fenced LOC; authored-member delta `-1 public`.

API/consumer proof: no `EventField.Of` call exists under `libs/dotnet`; the positional record constructor already creates the carrier.

### 4B. Delete both unused read forwarders

Location: `event.md:233-237`, anchor `public static Fin<Option<T>> Read<T>`.

From:

```csharp
public static Fin<Option<T>> Read<T>(CloudEvent envelope, CloudEventAttribute attribute, Op key) =>
    ReadCore<T>(envelope, attribute, key);

public Fin<Option<T>> Read<T>(CloudEvent envelope, Op key) =>
    ReadCore<T>(envelope, Attribute, key);
```

To:

```csharp
```

Effect: `-4` fenced LOC; authored-member delta `-2 public`.

API/consumer proof: neither overload has a call site. Whole-message reads already enter through `EventExtensionContract.Admit`; keeping per-field reads would preserve a competing, unused admission rail.

### 4C. Delete the now-unreachable reader

Location: `event.md:239-247`, anchor `static Fin<Option<T>> ReadCore<T>`.

From:

```csharp
static Fin<Option<T>> ReadCore<T>(CloudEvent envelope, CloudEventAttribute attribute, Op key) =>
    key.Catch(() => envelope[attribute] switch {
        null => Fin.Succ(Option<T>.None),
        T held => Fin.Succ(Some(held)),
        var foreign => Fin.Fail<Option<T>>(new KernelFault.InvalidValue(
            Label: attribute.Name,
            Requirement: $"a {attribute.Type.ClrType.Name} value, not {foreign.GetType().Name}",
            Key: Some(key))),
    });
```

To:

```csharp
```

Effect: `-9` fenced LOC; authored-member delta `-1 private`.

API/consumer proof: after 4B the helper is unreachable, and no admission path depends on its custom mismatch fault.

### 4D. Inline the sole write while preserving its per-field catch boundary

Location: `event.md:378`, anchor `request.Extensions.TraverseM(field => field.Write`.

From:

```csharp
from _extensions in request.Extensions.TraverseM(field => field.Write(envelope: envelope, key: key)).As()
```

To:

```csharp
from _extensions in request.Extensions.TraverseM(field => key.Catch(() =>
    Fin.Succ(envelope[field.Attribute] = field.Value)).Map(static _ => unit)).As()
```

Location: `event.md:249-250`, anchor `public Fin<Unit> Write`.

From:

```csharp
public Fin<Unit> Write(CloudEvent envelope, Op key) =>
    key.Catch(() => { envelope[Attribute] = Value; return Fin.Succ(unit); });
```

To:

```csharp
```

Effect: `3 -> 2` fenced LOC, delta `-1`; authored-member delta `-1 public`.

API/consumer proof: this is the only `EventField.Write` call. The replacement leaves the SDK attribute assignment inside the same per-field `Op.Catch`, maps the assignment result to `Unit`, and retains `TraverseM` first-failure sequencing.

Ripple for 4A-4D: none. Keep `EventField.Declare`; descriptor projection consumes it and it owns the extension-name ceiling.

## 5. Use the kernel's single `Option` to host-null bridge

### 5A. Generic mint slots

Location: `event.md:370-375`, anchor `Subject = request.Subject.Match`.

From:

```csharp
Subject = request.Subject.Match<string?>(Some: static held => held, None: static () => null),
Time = request.Time.Match(
    Some: static value => (DateTimeOffset?)value.ToDateTimeOffset(),
    None: static () => null),
DataSchema = request.DataSchema.Match<Uri?>(Some: static held => held, None: static () => null),
DataContentType = request.DataContentType.Match<string?>(Some: static held => held, None: static () => null),
```

To:

```csharp
Subject = Op.ToHostSlot(request.Subject),
Time = Op.ToHostNullable(request.Time.Map(static value => value.ToDateTimeOffset())),
DataSchema = Op.ToHostSlot(request.DataSchema),
DataContentType = Op.ToHostSlot(request.DataContentType),
```

Effect: `6 -> 4` fenced LOC, delta `-2`; symbol delta `0`.

### 5B. Binary raise content type

Location: `event.md:405`, anchor `DataContentType = dataType.Map`.

From:

```csharp
DataContentType = dataType.Map(static type => type.ToString()).Match<string?>(Some: static held => held, None: static () => null),
```

To:

```csharp
DataContentType = Op.ToHostSlot(dataType.Map(static type => type.ToString())),
```

Effect: `0` fenced LOC; symbol delta `0`.

API/consumer proof: `Domain/results.md` establishes `Op.ToHostSlot`/`ToHostNullable` as the host-null crossing. These SDK properties are nullable host slots; timestamp conversion remains inside `Option.Map` and therefore only executes on `Some`.

Ripple: update `[04]-[ENVELOPE_MINT]`'s host-slot law to name the shared `Op` projection.

## 6. Inline the single-use timestamp guard

Location: `event.md:363`, anchor `request.Time.Traverse(value => Aligned(value, key))`.

From:

```csharp
from _time in request.Time.Traverse(value => Aligned(value, key)).As()
```

To:

```csharp
from _time in request.Time.Traverse(value => guard(
    Instant.FromDateTimeOffset(value.ToDateTimeOffset()) == value,
    new KernelFault.InvalidValue(
        Label: nameof(CloudEvent.Time),
        Requirement: "an instant aligned to the CloudEvents SDK's 100-nanosecond precision",
        Key: Some(key))).ToFin()).As()
```

Location: `event.md:382-388`, anchor `private static Fin<Unit> Aligned`.

From:

```csharp
private static Fin<Unit> Aligned(Instant instant, Op key) =>
    Instant.FromDateTimeOffset(instant.ToDateTimeOffset()) == instant
        ? Fin.Succ(unit)
        : Fin.Fail<Unit>(new KernelFault.InvalidValue(
            Label: nameof(CloudEvent.Time),
            Requirement: "an instant aligned to the CloudEvents SDK's 100-nanosecond precision",
            Key: Some(key)));
```

To:

```csharp
```

Effect: `8 -> 6` fenced LOC, delta `-2`; authored-member delta `-1 private`.

API/consumer proof: `guard(bool, Error).ToFin()` is the checked-in predicate-to-failure bridge. `Option.Traverse` still executes the proof only when `Time` exists; `Aligned` has one call and no independent domain role.

## 7. Express both domain-equality refusals with `guard`

### 7A. Typed mint

Location: `event.md:444-447`, anchor `from _domain in request.Source.Domain == request.Type.Domain`.

From:

```csharp
from _domain in request.Source.Domain == request.Type.Domain
    ? Fin.Succ(unit)
    : Fin.Fail<Unit>(new KernelFault.InvalidValue(
        Label: nameof(EventSource), Requirement: "the same domain as EventType", Key: Some(key)))
```

To:

```csharp
from _domain in guard(request.Source.Domain == request.Type.Domain, new KernelFault.InvalidValue(
    Label: nameof(EventSource), Requirement: "the same domain as EventType", Key: Some(key))).ToFin()
```

### 7B. Typed admission

Location: `event.md:485-488`, anchor `from _domain in source.Domain == type.Domain`.

From:

```csharp
from _domain in source.Domain == type.Domain
    ? Fin.Succ(unit)
    : Fin.Fail<Unit>(new KernelFault.InvalidValue(
        Label: nameof(EventSource), Requirement: "the same domain as EventType", Key: Some(key)))
```

To:

```csharp
from _domain in guard(source.Domain == type.Domain, new KernelFault.InvalidValue(
    Label: nameof(EventSource), Requirement: "the same domain as EventType", Key: Some(key))).ToFin()
```

Effect: `8 -> 4` fenced LOC, delta `-4`; symbol delta `0`.

API/consumer proof: the local LanguageExt catalog and `Domain/results.md` prove `guard(bool, Error).ToFin()` returns success `Unit` on `true` and the supplied error on `false`. Both labels, requirements, and operation keys remain exact.

Ripple: none.

## 8. Collapse JSON configuration into the format owner

Apply 8A-8C together; the split keeps every anchored change below ten fenced lines.

### 8A. Delete the standalone JSON owner

Location: `event.md:562-573`, anchor `public static class EventJson`.

From:

```csharp
public static class EventJson {
    public static readonly JsonDocumentOptions Documents = new() { AllowDuplicateProperties = false };

    public static readonly JsonSerializerOptions Options = Configured();

    private static JsonSerializerOptions Configured() {
```

To:

```csharp
```

Location: continuation of the same `EventJson` block.

From:

```csharp
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web) { AllowDuplicateProperties = false };
        options.Converters.Add(new LanguageExtJsonConverterFactory());
        options.Converters.Add(new ThinktectureJsonConverterFactory());
        return options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    }
}
```

To:

```csharp
```

### 8B. Put the retained serializer identity on `EventFormat`

Location: immediately inside `EventFormat` at `event.md:579`.

From:

```csharp
public sealed partial class EventFormat {
```

To:

```csharp
public sealed partial class EventFormat {
    public static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            AllowDuplicateProperties = false,
            Converters = { new LanguageExtJsonConverterFactory(), new ThinktectureJsonConverterFactory() },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
```

### 8C. Inline the one-use document options value

Location: `event.md:580-581`, anchor `public static readonly EventFormat Json`.

From:

```csharp
public static readonly EventFormat Json = new(
    "json", "+json", new JsonEventFormatter(EventJson.Options, EventJson.Documents), batches: true);
```

To:

```csharp
public static readonly EventFormat Json = new(
    "json", "+json", new JsonEventFormatter(
        JsonOptions, new JsonDocumentOptions { AllowDuplicateProperties = false }), batches: true);
```

Effect: `13 -> 9` fenced LOC across the class and JSON row, delta `-4`; authored module-level type delta `-1` (`EventJson`); authored-member delta `-2` (`Documents` public, `Configured` private). `Options` is retained and renamed as `EventFormat.JsonOptions`.

API/consumer proof: collection initialization is supported by `JsonSerializerOptions.Converters`; `ConfigureForNodaTime` preserves the one options identity. The CloudEvents catalog proves `JsonDocumentOptions` is a separate formatter input, so duplicate-property refusal remains armed at both serializer and document levels. `EventFormat` already owns the only formatter that consumes these options and is the stronger owner. No code consumer uses `EventJson`; its only external mention is README prose.

Ripples: `libs/dotnet/Rasm/README.md:161`, `EventJson` -> `EventFormat.JsonOptions`; update `[05]-[FORMAT_CONTRACT]` owner/growth prose and `[15]` density to name the single owner.

## 9. Derive media suffixes from the smart-enum key

### 9A. Delete the duplicated constructor column from all rows

Location: `event.md:580-587`, anchors `EventFormat Json`, `Protobuf`, and `Avro`.

From:

```csharp
"json", "+json", new JsonEventFormatter(
```

To:

```csharp
"json", new JsonEventFormatter(
```

From:

```csharp
"protobuf", "+protobuf", new ProtobufEventFormatter(ProtobufEventFormatter.DefaultTypeUrlPrefix), batches: true);
```

To:

```csharp
"protobuf", new ProtobufEventFormatter(ProtobufEventFormatter.DefaultTypeUrlPrefix), batches: true);
```

From:

```csharp
"avro", "+avro", new AvroEventFormatter(), batches: false);
```

To:

```csharp
"avro", new AvroEventFormatter(), batches: false);
```

### 9B. Delete the generated column and derive both media types

Location: `event.md:589-597`, anchors `Suffix`, `Structured`, and `Batch`.

From:

```csharp
public string Suffix { get; }
```

To:

```csharp
```

From:

```csharp
public string Structured => MimeUtilities.MediaType + Suffix;

public string Batch => MimeUtilities.BatchMediaType + Suffix;
```

To:

```csharp
public string Structured => MimeUtilities.MediaType + "+" + Key;

public string Batch => MimeUtilities.BatchMediaType + "+" + Key;
```

Effect: `-1` fenced LOC; authored-member delta `-1 public`; generated smart-enum constructor/backing-column delta `-1`; three independent suffix literals removed.

API/consumer proof: Thinktecture smart-enum rows derive constructor columns from declared properties, and `Key` is the generated string key. Every current suffix is exactly `"+" + Key`; no consumer reads `Suffix`. `Structured` and `Batch` preserve byte-for-byte media strings.

Ripple: update `[05]-[FORMAT_CONTRACT]` to state that the suffix is derived from the key; remove the mirrored suffix column from its case table and adjust the density count.

## 10. Delete the second format-roster scan

Location: `event.md:638`, anchor `EventFormat.Batched(frame.Framing)`.

From:

```csharp
.Bind(format => key.Catch(() => Fin.Succ(EventFormat.Batched(frame.Framing)
```

To:

```csharp
.Bind(format => key.Catch(() => Fin.Succ(string.Equals(frame.Framing.MediaType, format.Batch, StringComparison.OrdinalIgnoreCase)
```

Location: `event.md:604-605`, anchor `public static bool Batched`.

From:

```csharp
public static bool Batched(ContentType framing) =>
    toSeq(Items).Exists(row => string.Equals(framing.MediaType, row.Batch, StringComparison.OrdinalIgnoreCase));
```

To:

```csharp
```

Effect: `-2` fenced LOC; authored-member delta `-1 public`; roster walks per decode `2 -> 1`.

API/consumer proof: `EventFormat.Of` has already selected the unique row by structured media type or `row.Batches && row.Batch`. Comparing the admitted framing with that selected row's `Batch` is sufficient; rescanning `Items` cannot admit a case the first scan rejected. No external `EventFormat.Batched` call exists.

Ripple: remove `Batched` from acceptance/density prose if named.

## 11. Inline the two single-use encode helpers

Apply 11A and 11B as one final expression; the staged presentation exposes each deleted helper's effect.

### 11A. Inline `Admitted`

Location: `event.md:619`, anchor `_ => Admitted(format: format`.

From:

```csharp
_ => Admitted(format: format, rows: envelopes.ToArray(), key: key),
```

To:

```csharp
_ => toSeq(envelopes.ToArray()).TraverseM(envelope => Admit(envelope, key)).As()
    .Bind(admitted => Framed(format: format, rows: admitted.ToArray(), key: key)),
```

Location: `event.md:622-624`, anchor `private static Fin<EventFrame> Admitted`.

From:

```csharp
private static Fin<EventFrame> Admitted(EventFormat format, CloudEvent[] rows, Op key) =>
    toSeq(rows).TraverseM(envelope => Admit(envelope, key)).As()
        .Bind(admitted => Framed(format: format, rows: admitted.ToArray(), key: key));
```

To:

```csharp
```

Effect: `4 -> 2` fenced LOC, delta `-2`; authored-member delta `-1 private`.

### 11B. Inline `Framed` into the expression produced by 11A

Location: the default `Encode` arm after 11A, anchor `.Bind(admitted => Framed`.

From:

```csharp
_ => toSeq(envelopes.ToArray()).TraverseM(envelope => Admit(envelope, key)).As()
    .Bind(admitted => Framed(format: format, rows: admitted.ToArray(), key: key)),
```

To:

```csharp
_ => toSeq(envelopes.ToArray()).TraverseM(envelope => Admit(envelope, key)).As()
    .Bind(admitted => key.Catch(() => {
        CloudEvent[] rows = admitted.ToArray();
        ContentType framing;
        ReadOnlyMemory<byte> body = rows is [CloudEvent single]
            ? format.Formatter.EncodeStructuredModeMessage(single, out framing)
            : format.Formatter.EncodeBatchModeMessage(rows, out framing);
        return Fin.Succ(new EventFrame(Body: body, Framing: framing));
    })),
```

Location: `event.md:626-633`, anchor `private static Fin<EventFrame> Framed`.

From:

```csharp
private static Fin<EventFrame> Framed(EventFormat format, CloudEvent[] rows, Op key) =>
    key.Catch(() => {
        ContentType framing;
        ReadOnlyMemory<byte> body = rows is [CloudEvent single]
            ? format.Formatter.EncodeStructuredModeMessage(single, out framing)
            : format.Formatter.EncodeBatchModeMessage(rows, out framing);
        return Fin.Succ(new EventFrame(Body: body, Framing: framing));
    });
```

To:

```csharp
```

Effect: `10 -> 9` fenced LOC, delta `-1`; authored-member delta `-1 private`.

API/consumer proof: each helper has one call and only sequences its arguments. `envelopes.ToArray()` still materializes the ref-like span before traversal; `TraverseM` still stops at first failed envelope. The admitted sequence becomes one array before the arity pattern, and both official formatter calls plus frame construction remain inside the same `Op.Catch`.

Ripple: none.

## Net accepted effect

- Fenced target LOC: `-39`.
- Authored module-level types: `-1` (`EventJson`).
- Authored members: `-16` total (`-11 public`, `-5 private`).
- Generated surface: removes two Generator.Equals nested comparer/diff families and one smart-enum constructor/backing column; replaces `EventId`'s five manual members with the standard Thinktecture value-object surface and no additional storage.
- Repeated format scans per decode: `2 -> 1`.
- Required code ripples: `EventType.ToValue()` in Bim; six `EventId` admissions; `EventFormat.JsonOptions` README reference.
- Required target prose ripples: package/entry/host-slot/format-owner/suffix/density repairs named above.

## Protected non-moves

- Keep `EventType.Part` and `EventSource.Part`. They are shared; deleting them duplicates parsing five times. Move 2 fixes the generated-API defect without manufacturing repeated logic.
- Keep `EventField` as the named typed boundary between descriptor projection and minting. Replacing it with a tuple erases ownership without removing behavior; only its unused methods are accidental surface.
- Keep `EventCarrier`: AppHost telemetry binds `EventCarrier.Read` and `.Write` directly. Keep `BrokerReach`: AppHost `BindingTrust` carries `Seq<BrokerReach>` and its rows are real typed policy, not semantic enum noise.
- Keep `EventExtensionContract<T>`'s parser, descriptor, validator, and projection surface. It is the descriptor-total generic boundary; consumer count alone is not a deletion argument for that growth rail.
- Keep both JSON duplicate-property guards. `JsonSerializerOptions` and `JsonDocumentOptions` protect different levels of CloudEvents JSON processing.
- Do not inline `EventEnvelope.Admitted` by calling `SetAttributeFromString` directly. `EventCarrier.Write` currently converts an invalid declared extension into `None`; moving the SDK call under the outer catch changes that silent drop into failed raise.
- Do not claim `Project(...).Traverse(...)` -> `TraverseM` as refinement. It removes no LOC or symbol and concrete `Fin` traversal already preserves first-failure semantics.
- `EventType`'s missing positive major segment is a real cross-sheet semantic mismatch, not a surgical cleanup: `libs/dotnet/Rasm/RULINGS.md:92` and Bim/Persistence consumers call `EventType.Of(..., major)` while this target declares a four-segment grammar and a three-argument factory. Resolve that owner decision before changing grammar, constructor arity, subscriptions, or wire spelling; no move above silently chooses a side.
