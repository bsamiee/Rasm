# [RASM_EVENT]

Bindings, filters, subscriptions, and `dataref` storage policy seat at their consuming owners; nothing transport-shaped enters. Settled vocabulary arrives from siblings: `Op` and the `Fault` band from `results.md`, the `UInt128` content key AND its one hex projection (`ContentHash.Hex`/`ContentHash.Admit`) from `identity.md`, `TraceCarrier` and `SpanEdge` from `telemetry.md` `[02]-[CAPSULE]`, `Hlc` and `CausalStamp` from `frame.md` `[04]-[STAMP]`. Grammar segment `<domain>` is the capability subject every `rasm.*` metric name carries, so the branch conformance minter resolves it and this page publishes the segment that gate reads.

## [01]-[INDEX]

- [02]-[EVENT_GRAMMAR]: `EventType`, `EventSource`, and `EventId` — admitted Rasm profile vocabularies over the SDK's standard attributes.
- [03]-[HANDLING_POLICY]: `DataGrade` and `BrokerReach` — interior egress policy projected once onto the generated event contract above this foundation.
- [04]-[ENVELOPE_MINT]: generic `CloudEventMint`, typed `RasmEventMint<T>`, descriptor-total `EventExtensionContract<T>`, the mint/admission pair, the one `Publish` door, and `EventCarrier` propagation.
- [05]-[FORMAT_CONTRACT]: `EventFormat` rows over JSON, Protobuf, and Avro with derived structured/batch framing, `EventFrame` the body-and-framing carrier, and the one encode/decode pair.
- [06]-[ACCEPTANCE]: contract, profile, descriptor, and format proofs.
- [07]-[DENSITY_BAR]: one owner per axis.

## [02]-[EVENT_GRAMMAR]

- Owner: `EventType` owns `rasm.<domain>.<subject>.<fact>`; `EventSource` owns the producing capability URI-reference; `EventId` owns the operation identity scoped by that source. `ContentHash` remains the sole owner of the `subject` content-key spelling.
- Entry: `EventType.Of` assembles the type; `EventSource.Of(domain, capability)` admits the independently stated producer coordinate; the Rasm profile proves domain agreement only. `EventId` admits the operation value through generated admission via `key.AcceptValidated<EventId>`; `ContentHash.Hex` and `.Admit` project the optional `subject` at the profile crossing.
- Law: `<fact>` reads past tense and carries the whole announced semantics, so a semantic break mints a fresh fact spelling rather than re-pointing the subscriptions keyed on the standing one. Payload-schema evolution stays independent: `dataschema` moves on its own axis and the type reads unchanged.
- Law: `source` names the producing CAPABILITY and never a host, package, or deployment — a redeployment that re-authors the identity consumers keyed on is the failure the `rasm:` scheme and its two-segment path foreclose, since neither segment has a spelling an environment can move. It never derives from `type.subject`: source context and fact classification are independent CloudEvents attributes.
- Law: `(source, id)` is the uniqueness composite every dedup reads. Producers sharing one capability source draw collision-resistant operation values from that source's namespace; `id` never repeats the capability as a prefix. The payload identity rides `subject`, while `dataref` is the storage URI-reference.
- Law: admission proves the ROUND TRIP, never the parse — a bare `UInt128.TryParse` admits upper-case and short forms this fabric never emits, so `"A"` and a full-width key ending `0a` collapse onto one dedup key while both read correct in isolation. That proof has ONE owner at `identity.md`: `ContentHash.Hex` renders and `ContentHash.Admit` refuses the spellings the outbound half cannot produce, and this page re-declares neither the `x32` literal nor the case rule.
- Packages: Thinktecture.Runtime.Extensions, CloudNative.CloudEvents, LanguageExt.Core (`Fin`, `MapFail`), BCL inbox (`System.Buffers`, `System.Globalization`).
- Growth: a new attribute vocabulary is one value object on this cluster; a new capability subject is one row on the branch conformance roster and none here, because this grammar validates the segment's SHAPE and the minter resolves its MEMBERSHIP.
- Boundary: `EventType.Domain` is the segment `[06]-[OBSERVABILITY_CONFORMANCE]`'s naming gate resolves against the branch roster at the conformance minter, so an unrostered subject refuses at that declaration owner rather than reaching a broker; this page never names that roster, because a kernel page holding an app-platform vocabulary inverts the strata. `subject` is the wire projection of `identity.md`'s `UInt128` currency, so `ContentHash` stays the only digest owner and renderer. `dataref` remains an independent URI-reference on the generated extension message.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Globalization;
using CloudNative.CloudEvents;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
internal static class EventGrammar {
    private static readonly SearchValues<char> SegmentGlyphs =
        SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789-");

    public static bool Segment(string text) =>
        text.Length > 0
        && text[0] != '-'
        && text[^1] != '-'
        && !text.AsSpan().Contains("--", StringComparison.Ordinal)
        && !text.AsSpan().ContainsAnyExcept(SegmentGlyphs);
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct EventType {
    private const string Prefix = "rasm";

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is [Prefix, var domain, var subject, var fact]
            && EventGrammar.Segment(domain) && EventGrammar.Segment(subject) && EventGrammar.Segment(fact)
            ? null
            : new ValidationError(message: $"EventType requires the rasm.<domain>.<subject>.<fact> grammar: {value}");

    public static EventType Of(string domain, string subject, string fact) =>
        Create(value: $"{Prefix}.{domain}.{subject}.{fact}");

    public string Domain => Part(index: 1);
    public string Subject => Part(index: 2);
    public string Fact => Part(index: 3);

    private string Part(int index) => ToValue().Split('.')[index];
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct EventSource {
    private const string Scheme = "rasm:";

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.StartsWith(Scheme, StringComparison.Ordinal)
            && value[Scheme.Length..].Split('/') is [var domain, var capability]
            && EventGrammar.Segment(domain) && EventGrammar.Segment(capability)
            && Uri.TryCreate(value, UriKind.Absolute, out _)
            ? null
            : new ValidationError(message: $"EventSource requires the rasm:<domain>/<capability> spelling: {value}");

    public static EventSource Of(string domain, string capability) =>
        Create(value: $"{Scheme}{domain}/{capability}");

    public string Domain => Part(index: 0);
    public string Capability => Part(index: 1);

    public Uri Reference => new(uriString: ToValue(), uriKind: UriKind.Absolute);

    private string Part(int index) => ToValue()[Scheme.Length..].Split('/')[index];
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct EventId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Any(char.IsControl)
            ? null
            : new ValidationError(message: "a non-empty control-free operation identity");
}
```

## [03]-[HANDLING_POLICY]

- Owner: `DataGrade` and `BrokerReach` own interior redaction and egress-reach policy. Generated `Extensions.dataclassification` carries the standard string attribute; a publisher projects the admitted interior row's key at its one generated-message boundary.
- Law: `Redact` and `Broker` are independent policy columns read by different gates. The local rows survive because they carry behavior the wire string cannot; they never declare an extension name or field number.
- Packages: Thinktecture.Runtime.Extensions.
- Growth: a handling case is one interior row and every binding must state its posture before a publisher may emit that row's key. The event schema remains the standard non-empty string attribute rather than mirroring this policy table as an enum.

| [INDEX] | [GRADE]      | [REDACT]             | [BROKER]  | [REACH]                              |
| :-----: | :----------- | :------------------- | :-------- | :----------------------------------- |
|  [01]   | `public`     | no obligation        | `every`   | every binding                        |
|  [02]   | `internal`   | no obligation        | `trusted` | deployment-trusted bindings alone    |
|  [03]   | `restricted` | redaction route runs | `trusted` | deployment-trusted bindings alone    |
|  [04]   | `secret`     | redaction route runs | `barred`  | no binding — reference-only carriage |

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BrokerReach {
    public static readonly BrokerReach Every = new("every");
    public static readonly BrokerReach Trusted = new("trusted");
    public static readonly BrokerReach Barred = new("barred");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DataGrade {
    public static readonly DataGrade Public = new("public", redact: false, broker: BrokerReach.Every);
    public static readonly DataGrade Internal = new("internal", redact: false, broker: BrokerReach.Trusted);
    public static readonly DataGrade Restricted = new("restricted", redact: true, broker: BrokerReach.Trusted);
    public static readonly DataGrade Secret = new("secret", redact: true, broker: BrokerReach.Barred);

    public bool Redact { get; }

    public BrokerReach Broker { get; }
}
```

## [04]-[ENVELOPE_MINT]

- Owner: `CloudEventMint` owns the generic standard construction shape; `RasmEventMint<T>` composes the Rasm grammar, typed content-key subject, and whole generated extension message; `EventExtensionContract<T>` derives the SDK projection from generated descriptors, validates the message, and stamps the causal slots by descriptor name; `EventEnvelope` owns the generic mint/raise funnel; `RasmEventEnvelope` owns profile mint/admission and the ONE publish door every durable kernel fact crosses.
- Entry: `EventEnvelope.Mint(request, key)` returns the generic strict SDK envelope. `RasmEventEnvelope.Publish(request, contract, clock, key)` is the producer's door — it takes `CausalStamp.Now(clock)`, writes the five causal slots onto the generated message through `contract.Stamp`, seals `time` with the stamp's physical half, and mints; `RasmEventEnvelope.Mint(request, contract, key)` is the already-stamped form a relay re-mints through, and `.Admit(envelope, contract, key)` returns the admitted typed profile. `EventEnvelope.Raise` remains the binary-mode inverse.
- Auto: `CloudEvent.Validate()` throws on a malformed envelope, so construction, projected extension writes, and validation funnel through one `Op.Catch`; the first refused field is the verdict and no partly stamped instance escapes.
- Law: the SDK indexer stamps IN PLACE, so a refused write leaves the instance partly stamped; what the result guarantees is that such an instance is UNREACHABLE — `Mint` holds the only reference until `Validate()` returns it, and a refusal returns no envelope at all. A result claiming the stronger "no half-stamped envelope exists" would be a law with no producer.
- Law: the creation-time trace is the generated `event.Extensions` `traceparent`/`tracestate`/`baggage` triplet, stamped ONCE at `Publish` from the live span's `TraceCarrier` and projected descriptor-total by `EventExtensionContract<T>`; `sequence` carries the stamp's logical half and `recordedtime` the wall instant the mint read. The transport carrier remains the current-hop context, and no ingress re-stamps a creation-time slot — `Stamp` resolves each slot through `Descriptor.FindFieldByName`, so a generated contract missing one refuses typed instead of dropping the frame.
- Law: `datacontenttype` and `dataschema` are row data off the serdes arrow that produced the body; both collapse to the SDK's nullable slot through the shared `HostEdge.Slot`/`HostEdge.Nullable` projection at this one crossing, exactly as optional `subject` does.
- Law: `time` is the occurrence stamp — the HLC physical half on every published profile event, so `(time, sequence)` IS the causal order — and `recordedtime` is when the producer created the CloudEvent. A receiver preserves both, records its own arrival time only in its interior delivery carrier, and measures skew from `recordedtime` against that arrival; re-stamping `recordedtime` at ingress erases the producer-to-receiver interval and violates the extension.
- Law: the SDK's `DateTimeOffset` timestamp surface resolves 100-nanosecond ticks. Mint refuses a finer `Instant`, and descriptor projection refuses a generated `Timestamp` whose nanos are not tick-aligned; admission never rounds producer time silently.
- Law: `subject` is OPTIONAL under a non-empty validator, so a fact whose payload carries no content key omits the slot — a required slot makes every lifecycle and topic producer fabricate an address, and the empty string such a producer reaches for is the one value the specification's own validator refuses.
- Law: descriptor field number order is the sole projection walk. String fields carrying Protovalidate `uri` or `uri_ref` rules become the SDK's URI or URI-reference type; timestamps, integers, booleans, bytes, and ordinary strings map by generated field kind. Any unsupported field kind refuses at the contract bridge instead of silently degrading to a string.
- Law: `EventCarrier` publishes absence on both halves. It resolves only attributes already declared on the envelope, so an unknown or over-ceiling peer field drops without this foundation inventing or mirroring a roster.
- Law: the message envelope PROJECTS the producing operation's own result as `data` and adds address, trace, tenant, stamp, and handling facts alone; a parallel event ledger, header wire, or fact stream beside the results is the deleted form.
- Packages: CloudNative.CloudEvents, Celly.Protovalidate, Google.Protobuf, LanguageExt.Core, NodaTime, BCL inbox (`System.Net.Mime`).
- Growth: a new solution extension changes only the generated descriptor; the projection walk, declaration set, construction, and decode consume it automatically. A new unsupported protobuf field kind fails visibly until one CloudEvents abstract-type correspondence is added.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Net.Mime;
using Buf.Validate;
using Celly.Protovalidate;
using CloudNative.CloudEvents;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using NodaTime;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CloudEventMint(
    string Type,
    Uri Source,
    string Id,
    Option<string> Subject,
    Option<Instant> Time,
    Option<Uri> DataSchema,
    Option<string> DataContentType,
    object? Data,
    Seq<EventField> Extensions);

public sealed record RasmEventMint<TExtensions>(
    EventType Type,
    EventSource Source,
    EventId Id,
    Option<UInt128> Subject,
    Instant Time,
    Option<Uri> DataSchema,
    Option<string> DataContentType,
    object? Data,
    TExtensions Extensions)
    where TExtensions : class, IMessage<TExtensions>;

public sealed record RasmEvent<TExtensions>(
    CloudEvent Envelope,
    EventType Type,
    EventSource Source,
    EventId Id,
    Option<UInt128> Subject,
    Instant Time,
    Option<Uri> DataSchema,
    Option<string> DataContentType,
    object? Data,
    TExtensions Extensions)
    where TExtensions : class, IMessage<TExtensions>;

public readonly record struct EventField(CloudEventAttribute Attribute, object Value) {
    public static CloudEventAttribute Declare(string name, CloudEventAttributeType type) =>
        name.Length is > 0 and <= 20
            ? CloudEventAttribute.CreateExtension(name, type)
            : throw new ArgumentOutOfRangeException(nameof(name), name, "CloudEvents extension names carry at most twenty characters");
}

public sealed record EventExtensionContract<TExtensions>(
    MessageParser<TExtensions> Parser,
    MessageDescriptor Descriptor,
    Validator Validator)
    where TExtensions : class, IMessage<TExtensions> {

    public Fin<Seq<CloudEventAttribute>> Declarations() => Try.lift(() => Fin.Succ(
        toSeq(Descriptor.Fields.InFieldNumberOrder()).Map(Declare))).Run().Bind(static inner => inner);

    public Fin<Seq<EventField>> Project(TExtensions message) =>
        message.Descriptor == Descriptor
            ? Valid(message).Bind(_ => toSeq(Descriptor.Fields.InFieldNumberOrder())
                .Filter(field => field.Accessor.HasValue(message))
                .Traverse(field => Project(field: field, value: field.Accessor.GetValue(message))).As())
            : Fin.Fail<Seq<EventField>>(new KernelFault.InvalidValue(
                Label: message.Descriptor.FullName, Requirement: Descriptor.FullName));

    public Fin<TExtensions> Admit(CloudEvent envelope) => Try.lift(() => {
        TExtensions message = Parser.ParseFrom(ByteString.Empty);
        if (message.Descriptor != Descriptor) {
            return Fin.Fail<TExtensions>(new KernelFault.InvalidValue(
                Label: message.Descriptor.FullName, Requirement: Descriptor.FullName));
        }
        foreach (FieldDescriptor field in Descriptor.Fields.InFieldNumberOrder()) {
            CloudEventAttribute attribute = Declare(field);
            object? held = envelope[attribute];
            if (held is not null) field.Accessor.SetValue(message, ToGenerated(field: field, value: held));
        }
        return Valid(message).Map(_ => message);
    }).Run().Bind(static inner => inner);

    public Fin<TExtensions> Stamp(TExtensions message, CausalStamp stamp) => Try.lift(() => {
        TExtensions stamped = message.Clone();
        foreach ((string slot, Option<object> value) in stamp.Slots) {
            FieldDescriptor? field = Descriptor.FindFieldByName(slot);
            if (field is null) {
                return Fin.Fail<TExtensions>(new KernelFault.InvalidValue(
                    Label: slot, Requirement: $"a {Descriptor.FullName} field carrying the causal slot"));
            }
            value.Iter(held => field.Accessor.SetValue(stamped, ToGenerated(field: field, value: held)));
        }
        return Fin.Succ(stamped);
    }).Run().Bind(static inner => inner);

    private Fin<Unit> Valid(TExtensions message) => Try.lift(() => {
        IReadOnlyList<Violation> violations = Validator.Validate(message);
        return violations.Count == 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue(
                Label: Descriptor.FullName,
                Requirement: string.Join("; ", violations.Select(static violation => $"{violation.RuleId}: {violation.Message}"))));
    }).Run().Bind(static inner => inner);

    private static Fin<EventField> Project(FieldDescriptor field, object value) =>
        value is Timestamp stamp && stamp.Nanos % TimeSpan.NanosecondsPerTick != 0
            ? Fin.Fail<EventField>(new KernelFault.InvalidValue(
                Label: field.FullName,
                Requirement: "a timestamp aligned to the CloudEvents SDK's 100-nanosecond instant"))
            : Try.lift(() => Fin.Succ(new EventField(
                Attribute: Declare(field), Value: ToEnvelope(field: field, value: value)))).Run().Bind(static inner => inner);

    private static CloudEventAttribute Declare(FieldDescriptor field) =>
        EventField.Declare(field.JsonName, AttributeType(field));

    private static CloudEventAttributeType AttributeType(FieldDescriptor field) => field.FieldType switch {
        FieldType.Bool => CloudEventAttributeType.Boolean,
        FieldType.Int32 or FieldType.SInt32 or FieldType.SFixed32
            or FieldType.UInt32 or FieldType.Fixed32 => CloudEventAttributeType.Integer,
        FieldType.String when StringRule(field) is StringRules.WellKnownOneofCase.Uri => CloudEventAttributeType.Uri,
        FieldType.String when StringRule(field) is StringRules.WellKnownOneofCase.UriRef => CloudEventAttributeType.UriReference,
        FieldType.String => CloudEventAttributeType.String,
        FieldType.Bytes => CloudEventAttributeType.Binary,
        FieldType.Message when field.MessageType.FullName == Timestamp.Descriptor.FullName => CloudEventAttributeType.Timestamp,
        _ => throw new NotSupportedException($"{field.FullName} cannot project {field.FieldType} onto a CloudEvents attribute type"),
    };

    private static StringRules.WellKnownOneofCase StringRule(FieldDescriptor field) {
        FieldRules? rules = field.GetOptions().GetExtension(ValidateExtensions.Field);
        return rules?.TypeCase is FieldRules.TypeOneofCase.String
            ? rules.String.WellKnownCase
            : StringRules.WellKnownOneofCase.None;
    }

    private static object ToEnvelope(FieldDescriptor field, object value) => AttributeType(field) switch {
        var type when type == CloudEventAttributeType.Integer => checked(Convert.ToInt32(value, CultureInfo.InvariantCulture)),
        var type when type == CloudEventAttributeType.Uri => new Uri((string)value, UriKind.Absolute),
        var type when type == CloudEventAttributeType.UriReference => new Uri((string)value, UriKind.RelativeOrAbsolute),
        var type when type == CloudEventAttributeType.Binary => ((ByteString)value).ToByteArray(),
        var type when type == CloudEventAttributeType.Timestamp => ((Timestamp)value).ToDateTimeOffset(),
        _ => value,
    };

    private static object ToGenerated(FieldDescriptor field, object value) => field.FieldType switch {
        FieldType.Bool => (bool)value,
        FieldType.Int32 or FieldType.SInt32 or FieldType.SFixed32 => (int)value,
        FieldType.UInt32 or FieldType.Fixed32 => checked((uint)(int)value),
        FieldType.String when value is Uri reference => reference.OriginalString,
        FieldType.String => (string)value,
        FieldType.Bytes => ByteString.CopyFrom((byte[])value),
        FieldType.Message when field.MessageType.FullName == Timestamp.Descriptor.FullName =>
            Timestamp.FromDateTimeOffset((DateTimeOffset)value),
        _ => throw new NotSupportedException($"{field.FullName} cannot admit {field.FieldType} from a CloudEvents attribute"),
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class EventEnvelope {
    public static Fin<CloudEvent> Mint(CloudEventMint request) =>
        from _time in request.Time.Traverse(value => guard(
            Instant.FromDateTimeOffset(value.ToDateTimeOffset()) == value,
            new KernelFault.InvalidValue(
                Label: nameof(CloudEvent.Time),
                Requirement: "an instant aligned to the CloudEvents SDK's 100-nanosecond precision")).ToFin()).As()
        from envelope in Try.lift(() => Fin.Succ(new CloudEvent(
            CloudEventsSpecVersion.V1_0,
            request.Extensions.Map(static field => field.Attribute)) {
            Id = request.Id,
            Source = request.Source,
            Type = request.Type,
            Subject = HostEdge.Slot(request.Subject),
            Time = HostEdge.Nullable(request.Time.Map(static value => value.ToDateTimeOffset())),
            DataSchema = HostEdge.Slot(request.DataSchema),
            DataContentType = HostEdge.Slot(request.DataContentType),
            Data = request.Data,
        })).Run().Bind(static inner => inner)
        from _extensions in request.Extensions.TraverseM(field => Try.lift(() =>
            Fin.Succ(envelope[field.Attribute] = field.Value)).Run().Bind(static inner => inner).Map(static _ => unit)).As()
        from validated in Try.lift(() => Fin.Succ(envelope.Validate())).Run().Bind(static inner => inner)
        select validated;

    public static Fin<CloudEvent> Admit(CloudEvent envelope) =>
        Try.lift(() => Fin.Succ(envelope.Validate())).Run().Bind(static inner => inner);

    public static Fin<CloudEvent> Raise(
        Seq<(string Name, string Value)> attributes,
        Seq<CloudEventAttribute> declared,
        ReadOnlyMemory<byte> data,
        Option<ContentType> dataType) =>
        attributes.Select(static row => row.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != attributes.Count
            ? Fin.Fail<CloudEvent>(new KernelFault.InvalidValue(
                Label: nameof(attributes), Requirement: "one value per CloudEvents attribute name"))
            : Try.lift(() => Fin.Succ(attributes.Fold(
            new CloudEvent(CloudEventsSpecVersion.V1_0, declared) {
                Data = data,
                DataContentType = HostEdge.Slot(dataType.Map(static type => type.ToString())),
            },
            static (held, row) => Admitted(envelope: held, name: row.Name, value: row.Value)))).Run().Bind(static inner => inner)
            .Bind(envelope => Admit(envelope));

    private static CloudEvent Admitted(CloudEvent envelope, string name, string value) {
        if (name == CloudEventsSpecVersion.SpecVersionAttribute.Name) {
            CloudEventsSpecVersion version = CloudEventsSpecVersion.FromVersionId(value)
                ?? throw new ArgumentException($"Unknown CloudEvents specversion: {value}", nameof(value));
            if (version != envelope.SpecVersion) {
                throw new ArgumentException($"Expected CloudEvents specversion {envelope.SpecVersion.VersionId}, not {value}", nameof(value));
            }
            return envelope;
        }
        ignore(Optional(envelope.SpecVersion.GetAttribute(name)).Match(
            Some: core => ignore(envelope[core] = core.Parse(value)),
            None: () => ignore(EventCarrier.Write(envelope: envelope, field: name, value: value))));
        return envelope;
    }

}

public static class RasmEventEnvelope {
    public static Fin<CloudEvent> Publish<TExtensions>(
        RasmEventMint<TExtensions> request,
        EventExtensionContract<TExtensions> contract,
        Hlc clock)
        where TExtensions : class, IMessage<TExtensions> {
        CausalStamp stamp = CausalStamp.Now(clock);
        return contract.Stamp(message: request.Extensions, stamp: stamp)
            .Bind(stamped => Mint(request with { Time = stamp.Clock.Physical, Extensions = stamped }, contract));
    }

    public static Fin<CloudEvent> Mint<TExtensions>(
        RasmEventMint<TExtensions> request,
        EventExtensionContract<TExtensions> contract)
        where TExtensions : class, IMessage<TExtensions> =>
        from _domain in guard(request.Source.Domain == request.Type.Domain, new KernelFault.InvalidValue(
            Label: nameof(EventSource), Requirement: "the same domain as EventType")).ToFin()
        from extensions in contract.Project(message: request.Extensions)
        from envelope in EventEnvelope.Mint(new CloudEventMint(
            Type: request.Type.ToString(),
            Source: request.Source.Reference,
            Id: request.Id.ToString(),
            Subject: request.Subject.Map(ContentHash.Hex),
            Time: Some(request.Time),
            DataSchema: request.DataSchema,
            DataContentType: request.DataContentType,
            Data: request.Data,
            Extensions: extensions))
        select envelope;

    public static Fin<RasmEvent<TExtensions>> Raise<TExtensions>(
        Seq<(string Name, string Value)> attributes,
        EventExtensionContract<TExtensions> contract,
        ReadOnlyMemory<byte> data,
        Option<ContentType> dataType)
        where TExtensions : class, IMessage<TExtensions> =>
        from declared in contract.Declarations()
        from envelope in EventEnvelope.Raise(
            attributes: attributes, declared: declared, data: data, dataType: dataType)
        from admitted in Admit(envelope: envelope, contract: contract)
        select admitted;

    public static Fin<RasmEvent<TExtensions>> Admit<TExtensions>(
        CloudEvent envelope,
        EventExtensionContract<TExtensions> contract)
        where TExtensions : class, IMessage<TExtensions> =>
        from admitted in EventEnvelope.Admit(envelope: envelope)
        from type in FactoryBridge.Accept<EventType>(admitted.Type!).MapFail(_ => new KernelFault.InvalidValue(
            Label: nameof(EventType), Requirement: "the generated EventType admission"))
        from source in FactoryBridge.Accept<EventSource>(admitted.Source!.ToString()).MapFail(_ => new KernelFault.InvalidValue(
            Label: nameof(EventSource), Requirement: "the generated EventSource admission"))
        from id in FactoryBridge.Accept<EventId>(admitted.Id!)
        from _domain in guard(source.Domain == type.Domain, new KernelFault.InvalidValue(
            Label: nameof(EventSource), Requirement: "the same domain as EventType"))
        from subject in Optional(admitted.Subject).Traverse(value => ContentHash.Admit(hex: value)
            .MapFail(_ => new KernelFault.InvalidValue(
                Label: nameof(CloudEvent.Subject),
                Requirement: "thirty-two lowercase hex digits round-tripping ContentHash.Hex"))).As()
        from time in Optional(admitted.Time).ToFin(new KernelFault.InvalidValue(
            Label: nameof(CloudEvent.Time), Requirement: "a present occurrence instant"))
        from extensions in contract.Admit(envelope: admitted)
        select new RasmEvent<TExtensions>(
            Envelope: admitted,
            Type: type,
            Source: source,
            Id: id,
            Subject: subject,
            Time: Instant.FromDateTimeOffset(time),
            DataSchema: Optional(admitted.DataSchema),
            DataContentType: Optional(admitted.DataContentType),
            Data: admitted.Data,
            Extensions: extensions);
}

public static class EventCarrier {
    public static Option<string> Read(CloudEvent envelope, string field) =>
        Optional(envelope.GetAttribute(field))
            .Bind(attribute => Optional(envelope[attribute]).Map(attribute.Format));

    public static Option<CloudEventAttribute> Write(CloudEvent envelope, string field, string value) =>
        Optional(envelope.GetAttribute(field)).Bind(attribute => Op.Of(name: nameof(EventCarrier))
            .Catch(() => Fin.Succ(envelope[attribute] = attribute.Parse(value)))
            .ToOption()
            .Map(_ => attribute));
}
```

## [05]-[FORMAT_CONTRACT]

- Owner: `EventFormat` the closed row family over the three admitted event formats, each carrying its batch reach and the one formatter instance every binding shares, and the seat of the one JSON serializer options identity `EventFormat.JsonOptions`; `EventFrame` carries encoded bytes beside framing; `EventEnvelope` owns bytes and official protobuf message crossings.
- Cases: JSON and Protobuf each define a structured envelope and a distinct batch envelope; Avro defines structured mode alone. A protocol binding owns binary mode, where attributes ride transport metadata and already-encoded data rides the body; the formatter's binary-data helper is package mechanics rather than an event-format capability. Framing derives from `MimeUtilities.MediaType` and `BatchMediaType` with `+` and the row's key, so no literal media type or suffix is spelled anywhere.
- Entry: `EventEnvelope.Encode(format, envelopes)` discriminates on arity; generic `.Decode(frame, declared)` takes SDK declarations while the profile overload takes `EventExtensionContract<T>` and returns typed admitted extensions. `EventEnvelope.ToProtobuf` and `EventEnvelope.FromProtobuf` expose the formatter's official `CloudEvent` and `CloudEventBatch` messages for registry-framed legs.
- Auto: the local encode convenience refuses an empty span because it carries no send intent. Decode preserves the specification's zero-or-more batch semantics for every batching format, so both JSON `[]` and an empty official protobuf `CloudEventBatch` return an empty sequence.
- Law: structured Protobuf selects its official data `oneof` from the admitted SDK value: string to `text_data`, bytes to `binary_data`, and `IMessage` to `proto_data` packed as `Any`. Binary-mode bindings carry explicitly encoded body bytes and never ask an event-format row to decide transport placement.
- Law: ONE formatter instance per row is the codec identity every transport binding, every mint, and every decode shares — serializer options fix at construction, never per event, and a per-transport or per-event formatter is the rejected form; the JSON row's options identity registers the branch's own converters so a typed payload carrying instants, generated owners, or functional carriers round-trips through the same handle a raw `JsonElement` crosses.
- Law: duplicate JSON object keys are REFUSED at both levels — `JsonDocumentOptions.AllowDuplicateProperties` gates the envelope's own attribute object and `JsonSerializerOptions.AllowDuplicateProperties` gates a typed payload, and both default to admitting duplicates on this runtime, so an unset pair decodes a twice-emitted attribute as last-write-wins with no party raising.
- Law: the Protobuf format's generated envelope message shares the simple name `CloudEvent` with the SDK's own envelope, so every fence touching that surface qualifies both sides; the generated batch message is `CloudNative.CloudEvents.V1.CloudEventBatch`, and `ConvertToProto`/`ConvertFromProto` are the public crossings a registry-framed leg composes instead of re-encoding a body the formatter already holds.
- Law: the Avro formatter's schema is the package's own embedded `RecordSchema` read through the static `AvroSchema` property, and a custom `IGenericRecordSerializer` is the seat where a registry-framed Avro leg binds its own reader and writer — so a schema-registry frame joins the format at that seat and never by re-spelling the envelope schema.
- Boundary: `EventFrame` equality is the CARRIER's — `ReadOnlyMemory<T>` is unreachable to every ordered-equality generator, so two frames compare by the memory's reference and range and never by content; a consumer wanting body identity addresses the bytes through `identity.md`'s `ContentHash` rather than comparing frames. The obsolete top-level `CloudNative.CloudEvents.AvroEventFormatter` never lands — it exists for backward compatibility, carries `[Obsolete]`, and derives from the namespaced formatter this page names; CBOR and XML are working drafts and take no row. A binding chooses structured, batch, or binary placement; this contract supplies event-format codecs only for structured and batch bodies, while binding headers and key mapping seat at the consuming owner.
- Packages: CloudNative.CloudEvents, CloudNative.CloudEvents.SystemTextJson, CloudNative.CloudEvents.Protobuf, CloudNative.CloudEvents.Avro, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, NodaTime, NodaTime.Serialization.SystemTextJson, LanguageExt.Core, BCL inbox (`System.Net.Mime`, `System.Text.Json`).
- Growth: a new event format is one `EventFormat` row carrying its columns and formatter instance — its media suffix derives from the key — and every encode, exact framing probe, and refusal reads it untouched; a typed payload lane binds `JsonEventFormatter<T>` against the same `EventFormat.JsonOptions` identity.

| [INDEX] | [FORMAT]   | [STRUCTURED] | [BATCH] |
| :-----: | :--------- | :----------: | :-----: |
|  [01]   | `json`     |     yes      |   yes   |
|  [02]   | `protobuf` |     yes      |   yes   |
|  [03]   | `avro`     |     yes      |   no    |

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Avro;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.Protobuf;
using CloudNative.CloudEvents.SystemTextJson;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Thinktecture.Text.Json.Serialization;
using ProtoCloudEvent = CloudNative.CloudEvents.V1.CloudEvent;
using ProtoCloudEventBatch = CloudNative.CloudEvents.V1.CloudEventBatch;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EventFormat {
    public static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            AllowDuplicateProperties = false,
            Converters = { new LanguageExtJsonConverterFactory(), new ThinktectureJsonConverterFactory() },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static readonly EventFormat Json = new(
        "json", new JsonEventFormatter(
            JsonOptions, new JsonDocumentOptions { AllowDuplicateProperties = false }), batches: true);

    public static readonly EventFormat Protobuf = new(
        "protobuf", new ProtobufEventFormatter(ProtobufEventFormatter.DefaultTypeUrlPrefix), batches: true);

    public static readonly EventFormat Avro = new(
        "avro", new AvroEventFormatter(), batches: false);

    public CloudEventFormatter Formatter { get; }

    public bool Batches { get; }

    public string Structured => MimeUtilities.MediaType + "+" + Key;

    public string Batch => MimeUtilities.BatchMediaType + "+" + Key;

    public static Option<EventFormat> Of(ContentType framing) =>
        toSeq(Items).Find(row =>
            string.Equals(framing.MediaType, row.Structured, StringComparison.OrdinalIgnoreCase)
            || row.Batches && string.Equals(framing.MediaType, row.Batch, StringComparison.OrdinalIgnoreCase));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EventFrame(ReadOnlyMemory<byte> Body, ContentType Framing);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class EventEnvelope {
    public static Fin<EventFrame> Encode(EventFormat format, params ReadOnlySpan<CloudEvent> envelopes) =>
        envelopes switch {
            [] => Fin.Fail<EventFrame>(new KernelFault.InvalidValue(
                Label: format.Key, Requirement: "at least one envelope to frame")),
            [_, _, ..] when !format.Batches => Fin.Fail<EventFrame>(new KernelFault.InvalidValue(
                Label: format.Key, Requirement: "a batching event format")),
            _ => toSeq(envelopes.ToArray()).TraverseM(envelope => Admit(envelope)).As()
                .Bind(admitted => Try.lift(() => {
                    CloudEvent[] rows = admitted.ToArray();
                    ContentType framing;
                    ReadOnlyMemory<byte> body = rows is [CloudEvent single]
                        ? format.Formatter.EncodeStructuredModeMessage(single, out framing)
                        : format.Formatter.EncodeBatchModeMessage(rows, out framing);
                    return Fin.Succ(new EventFrame(Body: body, Framing: framing));
                }).Run().Bind(static inner => inner)),
        };

    public static Fin<Seq<CloudEvent>> Decode(EventFrame frame, Seq<CloudEventAttribute> declared) =>
        EventFormat.Of(frame.Framing)
            .ToFin(new KernelFault.InvalidValue(Label: frame.Framing.MediaType, Requirement: "an admitted event format"))
            .Bind(format => Try.lift(() => Fin.Succ(string.Equals(frame.Framing.MediaType, format.Batch, StringComparison.OrdinalIgnoreCase)
                    ? toSeq(format.Formatter.DecodeBatchModeMessage(frame.Body, frame.Framing, declared))
                    : Seq(format.Formatter.DecodeStructuredModeMessage(frame.Body, frame.Framing, declared)))).Run().Bind(static inner => inner)
                .Bind(rows => rows.TraverseM(envelope => Admit(envelope)).As()));

    public static Fin<Seq<RasmEvent<TExtensions>>> Decode<TExtensions>(
        EventFrame frame,
        EventExtensionContract<TExtensions> contract)
        where TExtensions : class, IMessage<TExtensions> =>
        from declared in contract.Declarations()
        from envelopes in Decode(frame: frame, declared: declared)
        from admitted in envelopes.TraverseM(envelope => RasmEventEnvelope.Admit(
            envelope: envelope, contract: contract)).As()
        select admitted;

    public static Fin<ProtoCloudEvent> ToProtobuf(CloudEvent envelope) =>
        Admit(envelope: envelope).Bind(admitted => Try.lift(() =>
            Fin.Succ(((ProtobufEventFormatter)EventFormat.Protobuf.Formatter).ConvertToProto(admitted))).Run().Bind(static inner => inner));

    public static Fin<ProtoCloudEventBatch> ToProtobuf(Seq<CloudEvent> envelopes) =>
        envelopes.IsEmpty
            ? Fin.Fail<ProtoCloudEventBatch>(new KernelFault.InvalidValue(
                Label: EventFormat.Protobuf.Key, Requirement: "at least one envelope to frame"))
            : envelopes.TraverseM(envelope => ToProtobuf(envelope: envelope)).As()
                .Map(rows => new ProtoCloudEventBatch { Events = { rows } });

    public static Fin<CloudEvent> FromProtobuf(
        ProtoCloudEvent wire,
        Seq<CloudEventAttribute> declared) => Try.lift(() => Fin.Succ(
            ((ProtobufEventFormatter)EventFormat.Protobuf.Formatter).ConvertFromProto(wire, declared))).Run().Bind(static inner => inner)
            .Bind(envelope => Admit(envelope: envelope));

    public static Fin<Seq<CloudEvent>> FromProtobuf(
        ProtoCloudEventBatch wire,
        Seq<CloudEventAttribute> declared) => toSeq(wire.Events).TraverseM(row => FromProtobuf(wire: row, declared: declared)).As();

    public static Fin<RasmEvent<TExtensions>> FromProtobuf<TExtensions>(
        ProtoCloudEvent wire,
        EventExtensionContract<TExtensions> contract)
        where TExtensions : class, IMessage<TExtensions> =>
        from declared in contract.Declarations()
        from envelope in FromProtobuf(wire: wire, declared: declared)
        from admitted in RasmEventEnvelope.Admit(envelope: envelope, contract: contract)
        select admitted;

    public static Fin<Seq<RasmEvent<TExtensions>>> FromProtobuf<TExtensions>(
        ProtoCloudEventBatch wire,
        EventExtensionContract<TExtensions> contract)
        where TExtensions : class, IMessage<TExtensions> =>
        from declared in contract.Declarations()
        from envelopes in FromProtobuf(wire: wire, declared: declared)
        from admitted in envelopes.TraverseM(envelope => RasmEventEnvelope.Admit(
            envelope: envelope, contract: contract)).As()
        select admitted;
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Kernel message-envelope algebra
    accDescr: The generic SDK envelope remains open to future apps while the Rasm profile composes typed grammar, content identity, and one whole generated extension message through a descriptor-total bridge.
    Grammar["grammar · EventType · EventSource · EventId"] -->|Rasm profile values| Profile["RasmEventEnvelope"]
    Key["UInt128 subject"] -->|ContentHash.Hex| Profile
    Contract["generated Extensions message"] -->|descriptor-total projection| Profile
    Contract -->|descriptor-derived declarations| Decode["EventEnvelope.Decode"]
    Stamp["frame.md CausalStamp — trace/baggage · Hlc"] -->|"Stamp — five slots by descriptor name"| Publish["RasmEventEnvelope.Publish — ONE producer door"]
    Publish -->|"time = physical half"| Profile
    Profile -->|CloudEventMint| Mint["EventEnvelope.Mint — ONE Validate funnel"]
    Mint -->|Fin CloudEvent| Format["EventFormat — json · protobuf · avro"]
    Format -->|arity discriminates| Encode["EventEnvelope.Encode"]
    Format -.->|structured and batch codec reach| Bindings["consuming binding owners"]
    Encode -->|body + framing| Frame["EventFrame"]
    Frame -->|exact media type selects the leg| Decode
    Frame -.->|stamped as the binding's content type| Bindings
```

## [06]-[ACCEPTANCE]

- Generic surface: mint and admit a non-Rasm CloudEvent with omitted `time`, a valid app-owned extension, and SDK-owned standard attributes; the round trip must not invoke the Rasm grammar.
- Rasm identity: mint and admit independently constructed source and type values whose domains agree, and refuse a profile segment with uppercase, a leading or trailing hyphen, or repeated hyphens, plus a type whose segment count misses the grammar's arity. Mint and admit an operation id whose text contains no capability prefix, prove uniqueness is the `(source, id)` pair, prove a present `subject` is exactly `ContentHash.Hex`, and refuse uppercase or short digest text.
- Standard attributes: preserve occurrence `time`, optional absolute `dataschema`, `datacontenttype`, and payload independently; a registry subject, package coordinate, or contract generation presented as `dataschema` must have no special admission path.
- Generated extensions: populate every field on one generated `Extensions` value and prove descriptor-number-order construction and decode return the same generated value; `Publish` under a live tenant-stamped span must land `traceparent`, `baggage` carrying `rasm.tenant`, `sequence`, and `recordedtime`, seal `time` with the stamp's physical half, and a span-less publish must still carry the tenant pair. The fixture must cover ordinary strings, the `dataref` URI-reference, integer sample rate, and timestamp fields; a generated timestamp with sub-tick nanos and a mint `Instant` finer than 100 nanoseconds must refuse rather than round. Adding a descriptor field must break this proof until its CloudEvents abstract type is supported.
- Generated validation: one invalid generated value must refuse before mint and after each decode path with the generated rule id preserved. Duplicate extension declarations and duplicate binary attributes refuse; unknown peer attributes and peer names beyond the CloudEvents ceiling do not enter the returned generated message and do not fault the whole event.
- Formats: round-trip JSON structured and non-empty batch, admit inbound JSON `[]`, and round-trip every official protobuf data and attribute `oneof` arm plus non-empty and empty `CloudEventBatch` messages. Round-trip Avro structured mode. Refuse an empty local encode request, Avro batch mode, and unrelated media types that merely end in `+json`, `+protobuf`, or `+avro`; binary placement remains a binding proof over already-encoded data.

## [07]-[DENSITY_BAR]

One owner per axis; capability is a row, case, or column, never a sibling surface, and a consuming stratum composes one instance of this algebra rather than re-declaring a mint.

| [INDEX] | [AXIS_CONCERN]       | [OWNER]                               | [RESULT]                               |
| :-----: | :------------------- | :------------------------------------ | :------------------------------------- |
|  [01]   | Profile grammar      | `EventGrammar`                        | strict shared segment admission        |
|  [02]   | Type grammar         | `EventType`                           | generated factory + segment projection |
|  [03]   | Producer identity    | `EventSource`                         | generated factory + `Reference`        |
|  [04]   | Operation identity   | `EventId`                             | source-scoped value render/admit       |
|  [05]   | Content-key slots    | `ContentHash`                         | `Hex` / `Admit` at profile crossing    |
|  [06]   | Extension vocabulary | generated `event.Extensions`          | whole generated message                |
|  [07]   | Projected field      | `EventField`                          | SDK attribute + value                  |
|  [08]   | Handling policy      | `DataGrade` + `BrokerReach`           | `Redact` obligation / `Broker` reach   |
|  [09]   | Generated bridge     | `EventExtensionContract<T>`           | descriptor walk + validation           |
|  [10]   | Construction shape   | `CloudEventMint` / `RasmEventMint<T>` | generic standard / typed profile       |
|  [11]   | Mint boundary        | `EventEnvelope` / `RasmEventEnvelope` | generic funnel / `Publish` + admission |
|  [12]   | Creation-time stamp  | `CausalStamp` + `Contract.Stamp`      | five slots, once, at `Publish`         |
|  [13]   | Propagation edge     | `EventCarrier`                        | `Option`-publishing accessor pair      |
|  [14]   | Format rows          | `EventFormat`                         | structured/batch rows + one formatter  |
|  [15]   | Codec options        | `EventFormat.JsonOptions`             | one serializer identity on the rows    |
|  [16]   | Framing carriage     | `EventFrame`                          | body + chosen `ContentType`            |
|  [17]   | Encode / decode      | `EventEnvelope`                       | bytes plus official proto messages     |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
