# [BIM_EVENTS]

`BimEvent` is the closed domain-fact family every model-mutating Bim rail mints — a commit landed, an issue-board mutation, a validation verdict, an export artifact, an energy artifact — and `BimEnvelope` is its one CloudEvents 1.0 projection: type `rasm.bim.<domain>.<fact>`, source the composing service instance, subject the fact's content identity, and the distributed-tracing extension (`traceparent`/`tracestate`) stamped from the ambient `Activity` so W3C context rides brokers end to end. An event carries content keys, closed vocabulary keys, and `GlobalId` sets — never payload bytes: the addressed artifact, commit, or topic resolves through the one content-key space, so a consumer joins the fact back to its object-plane material without a second identity scheme.

Events complete the observability split the hook rail opens: a `Model/observability#HOOK_RAIL` point is the in-process best-effort tap whose subscriber faults shield into the registry evidence cell, and a `BimEvent` is the durable cross-process fact a lossless consumer reads — the "a subscriber that must never lose an event is a durable outbox consumer" law lands here as the outbox payload. `BimEvent` composes the `Review/versioning#VERSION_GRAPH` `CommitKey`, the `Review/issues#BCF_ARCHIVE` topic identity, the `Review/validation#IDS_FACETS` `IdsAudit` receipt, the `Exchange/export#EXPORT_RAIL` `ExportArtifact` seal, and the `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` grammar as settled vocabulary; each owning rail carries its own `- Events:` mint row naming the case it projects.

Wire posture is HOST-LOCAL, envelope-only: `CloudNative.CloudEvents` and `CloudNative.CloudEvents.SystemTextJson` are the only foreign surfaces, confined to the `BimEnvelope` fences — transport bindings (Kafka, MQTT, NATS, AMQP), broker retry, and delivery policy stay app-tier composition, and the durable outbox row is the `csharp:Rasm.Persistence` object plane's, joined by the encoded envelope bytes. Faults route the `Model/faults#FAULT_BAND` `BimFault` arms BARE — a malformed envelope, an unknown event type, or an undecodable payload lifts `CodecReject` (`event-envelope-malformed`/`event-type-miss`/`event-body-miss`/`event-payload-decode`/`event-key-malformed`), zero new cases.

## [01]-[INDEX]

- [01]-[EVENTS]: `BimEvent` the closed `[Union]` domain-fact family with per-case type and subject derivation, the host-free camelCase wire payloads over one source-generated `BimEventContext`, and `BimEnvelope` the CloudEvents projection — `Seal`/`Encode` out, `Open` back, traceparent/tracestate extension rows.

## [02]-[EVENTS]

- Owner: `BimEvent` the closed `[Union]` over the model-mutating fact shapes, each case deriving its `rasm.bim.<domain>.<fact>` type constant and its subject from its own evidence; `BimIssueMutation` the issue-event mutation vocabulary; `BimEnvelope` the one envelope owner carrying both directions (`Seal`/`Encode` the forward, `Open` the inverse consuming the forward's encoded bytes); `BimEventContext` the source-generated STJ context over the flat wire payload records.
- Cases: `CommitLanded` (`rasm.bim.review.committed` — `CommitKey`, sorted-distinct `Parents`, branch name, fingerprint count; subject the `CommitKey` hex) · `IssueMutated` (`rasm.bim.review.issue-mutated` — topic `Guid` string, one `BimIssueMutation` row, the optional comment `Guid`, the viewpoint-anchored `GlobalId` set; subject the topic `Guid`) · `VerdictIssued` (`rasm.bim.review.verdict-issued` — specification name, the `Spec` document ordinal, `IdsAudit.Conforms`, the failed-facet tally, the failing `GlobalId` set; subject `name#ordinal`, the ordinal disambiguating same-named specs exactly as the `IdsParity` join does) · `ArtifactMinted` (`rasm.bim.exchange.artifact-minted` — the Compute-computed `ContentKey`, the `InterchangeFormat.Key`, the byte count; subject the `key:kind` artifact address) · `EnergyMinted` (`rasm.bim.energy.artifact-minted` — the `ArtifactKey`, the `EnergyLeg` key, the format key, the warnings tally; subject the `ArtifactKey`) (5) — every slot a content key, a typed vocabulary row projected at the mint site, a primitive receipt field, or a `GlobalId` set, so the S3 owner consumes no S4 sibling type and no payload bytes ride the fabric.
- Entry: `BimEnvelope.Seal(BimEvent fact, Uri source, string id, Instant at)` → `CloudEvent` — spec version `V1_0`, the two tracing extension rows declared at construction, `Time` through `Instant.ToDateTimeOffset()`, payload projected to a `JsonElement` through `BimEventContext`, validated before return; `BimEnvelope.Encode(CloudEvent)` → `ReadOnlyMemory<byte>` the structured-mode `application/cloudevents+json` body; `BimEnvelope.Open(ReadOnlyMemory<byte> body, Op key)` → `Fin<(BimEvent, CloudEvent)>` the inverse — decode under the same declared extension roster, dispatch on the envelope `Type` constant, admit every host-crossing payload slot through its canonical shape and range gate, then require the envelope subject to equal the admitted fact's derived subject — so forward and inverse are operations of one owner and a direction-split sibling is the rejected form.
- Auto: trace continuity costs no signature slot — `Seal` reads `Activity.Current` once, stamping `traceparent` from the W3C activity id and `tracestate` only when non-empty, so a broker hop rejoins the trace the `Model/observability#TELEMETRY_TAP` `ActivitySource` opened; an absent activity stamps nothing and the envelope stays valid; mint sites construct the case from the owning rail's typed receipt at the rail's edge — the event is a projection of the receipt, minted beside it, never a second truth computed independently.
- Receipt: none minted here — the `BimEvent` IS the projection of the owning rail's receipt (`BimCommit`, the board mutation, `IdsAudit`, `ModelEmit`, `EnergyReceipt`), and the envelope adds only address and trace facts; a parallel event ledger beside the receipts is the deleted form.
- Packages: CloudNative.CloudEvents, CloudNative.CloudEvents.SystemTextJson, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm, BCL inbox (`System.Text.Json`, `System.Diagnostics.DiagnosticSource`).
- Growth: a new model-mutating fact is one `BimEvent` case, one type constant, one wire record with its context row, one `Payload`/`Admit` arm pair whose inverse names every slot's canonical admission, and one `- Events:` mint row on the owning rail; a new envelope dimension (a partition key, a sequence) is one declared `CloudEventAttribute` row beside the tracing pair, never a per-transport envelope fork.
- Boundary: mint points are the five owning rails and each names its row in place — `Review/versioning#VERSION_GRAPH` the sealed commit, `Review/issues#BCF_ARCHIVE` the board mutation, `Review/validation#IDS_FACETS` the issued verdict, `Exchange/export#EXPORT_RAIL` the sealed artifact, `Energy/exchange#ENERGY_EXCHANGE` the emitted energy artifact — so an emit call inside a projector or codec arm is the rejected form and the composing rail's edge owns the mint; transport bindings and delivery guarantees are app-tier (`CloudNative.CloudEvents.Kafka`/`.Mqtt`/`.Amqp` compose there against this page's one `CloudEventFormatter` identity), the durable outbox row is Persistence's, and the Python/TypeScript peers consume the structured-mode JSON body as plain CloudEvents — no Bim type crosses, the envelope is the contract; the hook rail stays the in-process tap and an event is never re-fired as a hook point — the two channels project the same receipts from different custody grades.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using LanguageExt;
using NodaTime;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// Issue mutation keys live on one generated owner, so envelope admission cannot invent a sixth mutation.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimIssueMutation {
    public static readonly BimIssueMutation TopicOpened = new("topic-opened");
    public static readonly BimIssueMutation TopicRevised = new("topic-revised");
    public static readonly BimIssueMutation CommentAdded = new("comment-added");
    public static readonly BimIssueMutation ViewpointAdded = new("viewpoint-added");
    public static readonly BimIssueMutation StatusAdvanced = new("status-advanced");
}

// BimEvent closes the domain-fact family: one case per model-mutating rail, each deriving its
// rasm.bim.<domain>.<fact> type constant and its subject from its own evidence — content keys,
// typed vocabulary rows, primitive receipt fields, and GlobalId sets, never payload bytes, so the
// S3 owner consumes no S4 sibling type and a consumer joins through the one content-key space.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimEvent {
    public const string CommittedType = "rasm.bim.review.committed";
    public const string IssueType = "rasm.bim.review.issue-mutated";
    public const string VerdictType = "rasm.bim.review.verdict-issued";
    public const string ArtifactType = "rasm.bim.exchange.artifact-minted";
    public const string EnergyType = "rasm.bim.energy.artifact-minted";

    private BimEvent() { }

    public abstract string EventType { get; }
    public abstract string Subject { get; }

    public sealed record CommitLanded(UInt128 CommitKey, Seq<UInt128> Parents, string Branch, int Elements) : BimEvent {
        public override string EventType => CommittedType;
        public override string Subject => CommitKey.ToString("x32", CultureInfo.InvariantCulture);
    }

    public sealed record IssueMutated(string Topic, BimIssueMutation Mutation, Option<string> Comment, Seq<string> GlobalIds) : BimEvent {
        public override string EventType => IssueType;
        public override string Subject => Topic;
    }

    public sealed record VerdictIssued(string Specification, int Spec, bool Conforms, int Findings, Seq<string> GlobalIds) : BimEvent {
        public override string EventType => VerdictType;
        public override string Subject => $"{Specification}#{Spec}";
    }

    public sealed record ArtifactMinted(UInt128 ContentKey, string Format, long Bytes) : BimEvent {
        public override string EventType => ArtifactType;
        public override string Subject => $"{ContentKey.ToString("x32", CultureInfo.InvariantCulture)}:{Format}";
    }

    public sealed record EnergyMinted(string ArtifactKey, string Leg, string Format, int Warnings) : BimEvent {
        public override string EventType => EnergyType;
        public override string Subject => ArtifactKey;
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// Host-free wire payloads — the structured-mode data body per case, camelCase through the context,
// UInt128 keys as 32-hex strings; the source-generated context keeps the formatter reflection-free.
public sealed record CommitLandedWire(string CommitKey, ImmutableArray<string> Parents, string Branch, int Elements);
public sealed record IssueMutatedWire(string Topic, string Mutation, string? Comment, ImmutableArray<string> GlobalIds);
public sealed record VerdictIssuedWire(string Specification, int Spec, bool Conforms, int Findings, ImmutableArray<string> GlobalIds);
public sealed record ArtifactMintedWire(string ContentKey, string Format, long Bytes);
public sealed record EnergyMintedWire(string ArtifactKey, string Leg, string Format, int Warnings);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(CommitLandedWire))]
[JsonSerializable(typeof(IssueMutatedWire))]
[JsonSerializable(typeof(VerdictIssuedWire))]
[JsonSerializable(typeof(ArtifactMintedWire))]
[JsonSerializable(typeof(EnergyMintedWire))]
public sealed partial class BimEventContext : JsonSerializerContext;

// --- [OPERATIONS] -------------------------------------------------------------------------
// One envelope owner, both directions: Seal/Encode the forward projection, Open the inverse over
// forward bytes. Tracing extensions are declared at envelope construction and
// on every decode, so traceparent/tracestate round-trip typed; one static formatter is the codec
// identity every app-tier transport binding shares.
public static class BimEnvelope {
    public const string PayloadContentType = "application/json";

    public static readonly CloudEventAttribute TraceParent = CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String);
    public static readonly CloudEventAttribute TraceState = CloudEventAttribute.CreateExtension("tracestate", CloudEventAttributeType.String);

    static readonly JsonEventFormatter Formatter = new();

    public static CloudEvent Seal(BimEvent fact, Uri source, string id, Instant at) {
        CloudEvent envelope = new(CloudEventsSpecVersion.V1_0, [TraceParent, TraceState]) {
            Id = id,
            Source = source,
            Type = fact.EventType,
            Subject = fact.Subject,
            Time = at.ToDateTimeOffset(),
            DataContentType = PayloadContentType,
            Data = Payload(fact),
        };
        if (Activity.Current is { Id: { } parent } span) {
            envelope[TraceParent] = parent;
            if (span.TraceStateString is { Length: > 0 } state) { envelope[TraceState] = state; }
        }
        return envelope.Validate();
    }

    public static ReadOnlyMemory<byte> Encode(CloudEvent envelope) =>
        Formatter.EncodeStructuredModeMessage(envelope, out _);

    public static Fin<(BimEvent Fact, CloudEvent Envelope)> Open(ReadOnlyMemory<byte> body, Op key) =>
        Try.lift(() => Formatter.DecodeStructuredModeMessage(body, contentType: null, [TraceParent, TraceState])).Run()
            .MapFail(error => (Error)new BimFault.CodecReject(key, $"event-envelope-malformed:{error.Message}"))
            .Bind(envelope => envelope.Data is JsonElement data
                ? Admit(envelope.Type, data, key).Bind(fact => StringComparer.Ordinal.Equals(envelope.Subject, fact.Subject)
                    ? Fin.Succ((fact, envelope))
                    : Fin.Fail<(BimEvent, CloudEvent)>(new BimFault.CodecReject(
                        key,
                        $"event-subject-mismatch:{envelope.Subject}:{fact.Subject}")))
                : Fin.Fail<(BimEvent, CloudEvent)>(new BimFault.CodecReject(key, $"event-body-miss:{envelope.Type}")));

    static JsonElement Payload(BimEvent fact) => fact.Switch(
        commitLanded: static c => JsonSerializer.SerializeToElement(
            new CommitLandedWire(Hex(c.CommitKey), [.. c.Parents.Map(Hex)], c.Branch, c.Elements),
            BimEventContext.Default.CommitLandedWire),
        issueMutated: static i => JsonSerializer.SerializeToElement(
            new IssueMutatedWire(i.Topic, i.Mutation.Key, i.Comment.Match(static c => c, static () => (string?)null), [.. i.GlobalIds]),
            BimEventContext.Default.IssueMutatedWire),
        verdictIssued: static v => JsonSerializer.SerializeToElement(
            new VerdictIssuedWire(v.Specification, v.Spec, v.Conforms, v.Findings, [.. v.GlobalIds]),
            BimEventContext.Default.VerdictIssuedWire),
        artifactMinted: static a => JsonSerializer.SerializeToElement(
            new ArtifactMintedWire(Hex(a.ContentKey), a.Format, a.Bytes),
            BimEventContext.Default.ArtifactMintedWire),
        energyMinted: static e => JsonSerializer.SerializeToElement(
            new EnergyMintedWire(e.ArtifactKey, e.Leg, e.Format, e.Warnings),
            BimEventContext.Default.EnergyMintedWire));

    // Type-dispatched inverse: the envelope Type constant selects the wire shape, and the context admits
    // its body; every key, tally, identifier, and set re-enters through its canonical gate — an unknown
    // type or malformed payload rails CodecReject BARE, never a partially admitted domain fact.
    static Fin<BimEvent> Admit(string? type, JsonElement data, Op key) => type switch {
        BimEvent.CommittedType => Wire(data, BimEventContext.Default.CommitLandedWire, key).Bind(w =>
            from commit in HexKey(w.CommitKey, key)
            from parents in ContentKeys(w.Parents, "parents", key)
            from branch in Required(w.Branch, "branch", key)
            from elements in NonNegative(w.Elements, "elements", key)
            select (BimEvent)new BimEvent.CommitLanded(commit, parents, branch, elements)),
        BimEvent.IssueType => Wire(data, BimEventContext.Default.IssueMutatedWire, key).Bind(w =>
            from topic in GuidText(w.Topic, "topic", key)
            from mutation in IssueMutation(w.Mutation, key)
            from comment in OptionalGuid(w.Comment, "comment", key)
            from globalIds in GlobalIds(w.GlobalIds, key)
            select (BimEvent)new BimEvent.IssueMutated(topic, mutation, comment, globalIds)),
        BimEvent.VerdictType => Wire(data, BimEventContext.Default.VerdictIssuedWire, key).Bind(w =>
            from specification in Required(w.Specification, "specification", key)
            from spec in NonNegative(w.Spec, "spec", key)
            from findings in NonNegative(w.Findings, "findings", key)
            from globalIds in GlobalIds(w.GlobalIds, key)
            select (BimEvent)new BimEvent.VerdictIssued(specification, spec, w.Conforms, findings, globalIds)),
        BimEvent.ArtifactType => Wire(data, BimEventContext.Default.ArtifactMintedWire, key).Bind(w =>
            from content in HexKey(w.ContentKey, key)
            from format in Required(w.Format, "format", key)
            from bytes in NonNegative(w.Bytes, "bytes", key)
            select (BimEvent)new BimEvent.ArtifactMinted(content, format, bytes)),
        BimEvent.EnergyType => Wire(data, BimEventContext.Default.EnergyMintedWire, key).Bind(w =>
            from artifact in ArtifactKey(w.ArtifactKey, key)
            from leg in Required(w.Leg, "leg", key)
            from format in Required(w.Format, "format", key)
            from warnings in NonNegative(w.Warnings, "warnings", key)
            select (BimEvent)new BimEvent.EnergyMinted(artifact, leg, format, warnings)),
        var unknown => Fin.Fail<BimEvent>(new BimFault.CodecReject(key, $"event-type-miss:{unknown}")),
    };

    static Fin<T> Wire<T>(JsonElement data, JsonTypeInfo<T> shape, Op key) where T : class =>
        Try.lift(() => data.Deserialize(shape)).Run()
            .MapFail(error => (Error)new BimFault.CodecReject(key, $"event-payload-decode:{error.Message}"))
            .Bind(wire => wire is null
                ? Fin.Fail<T>(new BimFault.CodecReject(key, "event-body-miss:payload-null"))
                : Fin.Succ(wire));

    static Fin<BimIssueMutation> IssueMutation(string? value, Op key) =>
        value is not null && BimIssueMutation.TryGet(value, out var mutation)
            ? Fin.Succ(mutation)
            : Fin.Fail<BimIssueMutation>(new BimFault.CodecReject(key, $"event-mutation-miss:{value}"));

    static Fin<string> Required(string? value, string slot, Op key) =>
        value is { Length: > 0 } && StringComparer.Ordinal.Equals(value, value.Trim())
            ? Fin.Succ(value)
            : Fin.Fail<string>(new BimFault.CodecReject(key, $"event-{slot}-malformed"));

    static Fin<int> NonNegative(int value, string slot, Op key) => value >= 0
        ? Fin.Succ(value)
        : Fin.Fail<int>(new BimFault.CodecReject(key, $"event-{slot}-negative:{value}"));

    static Fin<long> NonNegative(long value, string slot, Op key) => value >= 0
        ? Fin.Succ(value)
        : Fin.Fail<long>(new BimFault.CodecReject(key, $"event-{slot}-negative:{value}"));

    static Fin<string> GuidText(string? value, string slot, Op key) =>
        value is not null
        && Guid.TryParseExact(value, "D", out Guid parsed)
        && StringComparer.Ordinal.Equals(value, parsed.ToString("D"))
            ? Fin.Succ(value)
            : Fin.Fail<string>(new BimFault.CodecReject(key, $"event-{slot}-malformed:{value}"));

    static Fin<Option<string>> OptionalGuid(string? value, string slot, Op key) => value is null
        ? Fin.Succ(Option<string>.None)
        : GuidText(value, slot, key).Map(Some);

    static Fin<Seq<UInt128>> ContentKeys(ImmutableArray<string> values, string slot, Op key) =>
        OrderedDistinct(values)
            ? toSeq(values).TraverseM(value => HexKey(value, key)).As()
            : Fin.Fail<Seq<UInt128>>(new BimFault.CodecReject(key, $"event-{slot}-order"));

    static Fin<Seq<string>> GlobalIds(ImmutableArray<string> values, Op key) =>
        OrderedDistinct(values) && values.All(static value =>
            value is { Length: 22 } && value.All(static glyph => glyph is >= '0' and <= '9'
                or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '$'))
            ? Fin.Succ(toSeq(values))
            : Fin.Fail<Seq<string>>(new BimFault.CodecReject(key, "event-global-ids-malformed"));

    static bool OrderedDistinct(ImmutableArray<string> values) =>
        values.Distinct(StringComparer.Ordinal).Count() == values.Length
        && values.SequenceEqual(values.OrderBy(static value => value, StringComparer.Ordinal), StringComparer.Ordinal);

    static Fin<string> ArtifactKey(string? value, Op key) {
        int separator = value?.IndexOf(':', StringComparison.Ordinal) ?? -1;
        return value is not null && separator == 32
            ? from content in HexKey(value[..separator], key)
              from format in Required(value[(separator + 1)..], "artifact-format", key)
              select value
            : Fin.Fail<string>(new BimFault.CodecReject(key, $"event-artifact-key-malformed:{value}"));
    }

    static string Hex(UInt128 contentKey) => contentKey.ToString("x32", CultureInfo.InvariantCulture);

    static Fin<UInt128> HexKey(string? hex, Op key) =>
        hex is { Length: 32 }
        && UInt128.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 value)
        && StringComparer.Ordinal.Equals(hex, Hex(value))
            ? Fin.Succ(value)
            : Fin.Fail<UInt128>(new BimFault.CodecReject(key, $"event-key-malformed:{hex}"));
}
```

## [03]-[RESEARCH]

(none)
