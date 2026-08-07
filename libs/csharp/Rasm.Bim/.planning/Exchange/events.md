# [BIM_EVENTS]

`BimEvent` is the closed domain-fact family every model-mutating Bim rail mints — a commit landed, an issue-board mutation, a validation verdict, an export artifact, an energy artifact — and `BimEnvelope` is its one CloudEvents 1.0 projection: type `rasm.bim.<domain>.<fact>`, source the composing service instance, subject the fact's content identity, and the distributed-tracing extension (`traceparent`/`tracestate`) stamped from the ambient `Activity` so W3C context rides brokers end to end. Every event carries content keys, closed vocabulary keys, and `GlobalId` sets — never payload bytes: the addressed artifact, commit, or topic resolves through the one content-key space, so a consumer joins the fact back to its object-plane material without a second identity scheme.

Events complete the observability split the hook rail opens: a `Model/observability#HOOK_RAIL` point is the in-process best-effort tap whose subscriber faults shield into the registry evidence cell, and a `BimEvent` is the durable cross-process fact a lossless consumer reads — the "a subscriber that must never lose an event is a durable outbox consumer" law lands here as the outbox payload. `BimEvent` composes the `Review/versioning#VERSION_GRAPH` `CommitKey`, the `Review/issues#BCF_ARCHIVE` topic identity, the `Review/validation#IDS_FACETS` `IdsAudit` receipt, the `Exchange/export#EXPORT_RAIL` `ExportArtifact` seal, and the `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` grammar as settled vocabulary; each owning rail carries its own `- Events:` mint row naming the case it projects.

Wire posture is HOST-LOCAL, envelope-only: `CloudNative.CloudEvents` and `CloudNative.CloudEvents.SystemTextJson` are the only foreign surfaces, confined to the `BimEnvelope` fences — transport bindings (Kafka, MQTT, NATS, AMQP), broker retry, and delivery policy stay app-tier composition, and the durable outbox row is the `Rasm.Persistence` object plane's, joined by the encoded envelope bytes. Faults route the `Model/faults#FAULT_BAND` `BimFault` arms BARE — a malformed envelope, an unknown event type, an undecodable body, a re-addressed subject, and every slot failing its own canonical admission all lift `CodecReject` under one `event-<subject>-<defect>` detail grammar the raising site composes, zero new cases.

## [01]-[INDEX]

- [02]-[EVENTS]: `BimEvent` the closed `[Union]` domain-fact family with per-case type and subject derivation, the host-free camelCase wire payloads over one source-generated `BimEventContext`, and `BimEnvelope` the CloudEvents projection — `Seal`/`Encode` out over one or many, `Open` back over the framed `EncodedEnvelope`, the traceparent/tracestate rows the one `Declared` roster carries.

## [02]-[EVENTS]

- Owner: `BimEvent` the closed `[Union]` over the model-mutating fact shapes, each case deriving its `rasm.bim.<domain>.<fact>` type constant and its subject from its own evidence; `BimIssueMutation` the issue-event mutation vocabulary; `BimEnvelope` the one envelope owner carrying both directions AND both call modalities (`Seal`/`Encode` the forward over one or many, `Open` the inverse consuming the forward's own `EncodedEnvelope`); `EncodedEnvelope` that carrier — the emitted body beside the framing content type the formatter chose, the header a transport binding stamps and the discriminant the inverse reads; `BimEventContext` the source-generated STJ context over the flat wire payload records.
- Cases: one case per owning rail, the fence carrying each type constant, slot set, and subject derivation. Every slot is a content key, a generated vocabulary row projected at the mint site, a primitive receipt field, or a `GlobalId` set — so an S3 owner consumes no S4 sibling type and no payload bytes ride the fabric. `VerdictIssued` subjects on `name#ordinal` because a specification name alone collides across same-named specs, exactly as the `IdsParity` join disambiguates them; both artifact cases subject on the `key:kind` address so a consumer joins straight into the object plane with no second identity scheme.
- Entry: `BimEnvelope.Seal(BimEvent fact, Uri source, string id, Instant at, Op key)` → `Fin<CloudEvent>` — spec version `V1_0`, the one `Declared` extension roster read at construction, `Time` through `Instant.ToDateTimeOffset()`, payload projected to a `JsonElement` through the generated wire mapper and `BimEventContext`, then `Validate()` captured through the one `Try.lift` funnel so a malformed envelope rails `CodecReject` instead of throwing past a rail-typed caller; `BimEnvelope.Encode(params ReadOnlySpan<CloudEvent>)` → `EncodedEnvelope` the ONE emit over both framings, the span's arity discriminating structured-mode `application/cloudevents+json` from batch-mode and the formatter's own `out ContentType` riding forward as the carrier's `Framing`; `BimEnvelope.Open(EncodedEnvelope message, Op key)` → `Fin<Seq<(BimEvent, CloudEvent)>>` the inverse consuming that same carrier — the framing media type selecting the structured or batch reader against the SDK's own batch-media-type prefix, decode under the same `Declared` roster, dispatch on each envelope `Type` constant, admit every host-crossing payload slot through its canonical shape and range gate, then require each envelope subject to equal its admitted fact's derived subject, one malformed member failing the whole message — so forward and inverse are operations of one owner over one carrier, and a `batch` flag, a second `EncodeBatch`/`OpenBatch` pair, or a direction-split sibling is the rejected form.
- Auto: trace continuity costs no signature slot — `Seal` reads `Activity.Current` once, stamping `traceparent` from the W3C activity id and `tracestate` only when non-empty, so a broker hop rejoins the trace the `Model/observability#TELEMETRY_TAP` `ActivitySource` opened; an absent activity stamps nothing and the envelope stays valid; mint sites construct the case from the owning rail's typed receipt at the rail's edge — an event projects its receipt, minted beside it, never a second truth computed independently. Build-time provability splits the union⇄wire correspondence: `Riok.Mapperly` generates the outbound shape half from five declared partial signatures, so a case that grows a column raises an `RMG` diagnostic where a hand-written constructor call answered with silence; the inbound half stays hand-written on member-level refutation — `Required`, `GuidText`, and `ArtifactKey` gate the ONE `string`→`string` type pair user mappings resolve by, so a generated inbound map re-spells every slot as a `[NamedMapping]`+`[MapProperty(Use = …)]` row at no LOC gain, throwing carrier codecs funneled through one entry `Try.lift` replace each slot's `Fin` verdict with exception control flow, a parse-then-validate intermediate destroys the spelling evidence `HexKey`'s round-trip reads (a parsed `UInt128` cannot recover its wire form), and the `Type`-constant dispatch with the subject re-proof stays hand-written under every variant — the generator adds signatures without absorbing one gate.
- Receipt: none minted here — the `BimEvent` IS the projection of the owning rail's receipt (`BimCommit`, the board mutation, `IdsAudit`, `ModelEmit`, `EnergyReceipt`), and the envelope adds only address and trace facts; a parallel event ledger beside the receipts is the deleted form.
- Packages: CloudNative.CloudEvents, CloudNative.CloudEvents.SystemTextJson, Riok.Mapperly, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm, BCL inbox (`System.Text.Json`, `System.Diagnostics.DiagnosticSource`).
- Growth: a new model-mutating fact is one `BimEvent` case, one type constant, one wire record with its context row, one declared `Wire` partial signature the generator fills, one `Admit` arm naming every slot's canonical admission, and one `- Events:` mint row on the owning rail; a new envelope dimension the SDK already owns — a partition key, a sampled rate, an ordering sequence — joins the `Declared` roster as that helper's own `AllAttributes` and rides its `Set*`/`Get*` pair, so the name, its value space, and its validator arrive from the package rather than a hand-spelled `CreateExtension` row that re-declares them and drifts; only a dimension no helper owns is hand-declared, the W3C tracing pair being that case; a new call modality is neither, because `Encode`/`Open` discriminate one from many on the value's own arity and framing; never a per-transport envelope fork.
- Boundary: mint points are the five owning rails and each names its row in place — `Review/versioning#VERSION_GRAPH` the sealed commit, `Review/issues#BCF_ARCHIVE` the board mutation, `Review/validation#IDS_FACETS` the issued verdict, `Exchange/export#EXPORT_RAIL` the sealed artifact, `Energy/exchange#ENERGY_EXCHANGE` the emitted energy artifact — so an emit call inside a projector or codec arm is the rejected form and the composing rail's edge owns the mint; transport bindings and delivery guarantees are app-tier (`CloudNative.CloudEvents.Kafka`/`.Mqtt`/`.Amqp` compose there against this page's one `CloudEventFormatter` identity, each stamping the `EncodedEnvelope.Framing` content type its protocol carries rather than re-deriving it), the durable outbox row is Persistence's, and the Python/TypeScript peers consume the structured-mode JSON body or the batch array as plain CloudEvents — no Bim type crosses, the envelope is the contract; the hook rail stays the in-process tap and an event is never re-fired as a hook point — the two channels project the same receipts from different custody grades.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.SystemTextJson;
using LanguageExt;
using NodaTime;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using Rasm.Bim.Model;                        // BimFault + the Detail roster every admission gate raises through
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// GlobalIdSet closes the IFC GlobalId SET: the lexical law (22 glyphs over the buildingSMART base64 alphabet
// 0-9, A-Z, a-z, `_`, `$`) AND the set law (sorted, distinct) in ONE owner, so no site re-spells the alphabet
// beside a length check that drifts from it and no site re-spells the ordering probe beside a distinct probe that
// disagrees about the comparer. Of NORMALIZES for the mint side — a producer holding a bag of ids gets a canonical
// set — while Admit REFUSES for the wire side, because the wire contract IS sorted-distinct and silently sorting a
// producer's malformed array would hide a producer defect behind a well-formed envelope.
[ValueObject<Seq<string>>]
public sealed partial class GlobalIdSet {
    const int Glyphs = 22;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<string> value) {
        if (!value.ForAll(Admits)) {
            validationError = new ValidationError("global-id-glyphs");
            return;
        }
        value = toSeq(value.Distinct(StringComparer.Ordinal).OrderBy(static id => id, StringComparer.Ordinal));
    }

    public static GlobalIdSet Of(Seq<string> ids) => Create(ids);

    // Wire admission: ordered-distinct is PROVEN, never imposed, and the glyph law rides the same construction —
    // through the TRY factory, because the throwing Create would carry a producer's malformed array out of the
    // typed rail as an exception at the exact boundary the rail exists to answer.
    public static Fin<GlobalIdSet> Admit(ImmutableArray<string> values, Op key) =>
        WireSet.Ordered(values) && TryCreate(toSeq(values), out GlobalIdSet? admitted) && admitted is { } set
            ? Fin.Succ(set)
            : Fin.Fail<GlobalIdSet>(Detail.EventSetMalformed.At(key, "globalIds"));

    static bool Admits(string? value) =>
        value is { Length: Glyphs } && value.All(static glyph =>
            glyph is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '$');
}

// ContentKeySet is the same law over the content-key space: sorted-distinct at construction, and the wire form is
// the 32-hex rendering each key round-trips through. The two sets wore raw Seq<string>/Seq<UInt128> slots with
// their ordering and distinctness re-proved at each admission site, so a case that grew a set had to grow a gate
// too — and the parents set proved ordering on the HEX TEXT while the fact carried the numeric values, two
// orderings that agree only because the hex rendering is fixed-width.
[ValueObject<Seq<UInt128>>]
public sealed partial class ContentKeySet {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<UInt128> value) =>
        value = toSeq(value.Distinct().OrderBy(static k => k));

    public static ContentKeySet Of(Seq<UInt128> keys) => Create(keys);
}

// The one wire-side set probe both admissions share: distinct under the ordinal comparer AND already in ordinal
// order. It reads the ARRAY the producer sent, so a set the producer sorted differently fails here rather than
// being quietly re-sorted into a shape the sender never emitted.
static class WireSet {
    public static bool Ordered(ImmutableArray<string> values) =>
        values.Distinct(StringComparer.Ordinal).Count() == values.Length
        && values.SequenceEqual(values.OrderBy(static value => value, StringComparer.Ordinal), StringComparer.Ordinal);
}

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

    public sealed record CommitLanded(UInt128 CommitKey, ContentKeySet Parents, string Branch, int Elements) : BimEvent {
        public override string EventType => CommittedType;
        public override string Subject => CommitKey.ToString("x32", CultureInfo.InvariantCulture);
    }

    public sealed record IssueMutated(string Topic, BimIssueMutation Mutation, Option<string> Comment, GlobalIdSet GlobalIds) : BimEvent {
        public override string EventType => IssueType;
        public override string Subject => Topic;
    }

    public sealed record VerdictIssued(string Specification, int Spec, bool Conforms, int Findings, GlobalIdSet GlobalIds) : BimEvent {
        public override string EventType => VerdictType;
        public override string Subject => $"{Specification}#{Spec}";
    }

    // Format is the format#FORMAT_AXIS ROW, not its text: the subject address a consumer joins on is
    // `<content-key>:<format-key>`, and a raw string let a producer address an artifact under a token no
    // InterchangeFormat row carries — an address that resolves to nothing and reads as a valid one.
    public sealed record ArtifactMinted(UInt128 ContentKey, InterchangeFormat Format, long Bytes) : BimEvent {
        public override string EventType => ArtifactType;
        public override string Subject => $"{ContentKey.ToString("x32", CultureInfo.InvariantCulture)}:{Format.Key}";
    }

    // ArtifactKey is the Energy/exchange#ENERGY_EXCHANGE value object that OWNS the `<content-key>:<format-key>`
    // grammar, so this case carries the admitted value and this page holds no second parser for it.
    public sealed record EnergyMinted(ArtifactKey ArtifactKey, string Leg, InterchangeFormat Format, int Warnings) : BimEvent {
        public override string EventType => EnergyType;
        public override string Subject => ArtifactKey.Value;
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

// EncodedEnvelope carries the forward's result and the inverse's ingress: emitted body beside the FRAMING the
// formatter chose for it. That framing is the content-type header an app-tier transport binding stamps AND the
// discriminant Open reads to pick its reader, so both travel as one value and neither half is re-derived — a
// discarded `out _` threw away the only evidence distinguishing a structured body from a batch array.
public sealed record EncodedEnvelope(ReadOnlyMemory<byte> Body, ContentType Framing);

// --- [BOUNDARIES] -------------------------------------------------------------------------
// Mapperly owns the outbound SHAPE half — the generator fills these five bodies from member correspondence;
// `Admit` stays the hand-written inbound rail. Five signatures stand in for one `[MapDerivedType]` switch: the
// wire records share no base, because a common base forces STJ polymorphism and its `$type` discriminator then
// enters the structured-mode body every peer parses as a flat camelCase record.
[Mapper]
[UseStaticMapper(typeof(WireCodec))]
public static partial class BimEventWire {
    public static partial CommitLandedWire Wire(BimEvent.CommitLanded fact);
    public static partial IssueMutatedWire Wire(BimEvent.IssueMutated fact);
    public static partial VerdictIssuedWire Wire(BimEvent.VerdictIssued fact);
    public static partial ArtifactMintedWire Wire(BimEvent.ArtifactMinted fact);
    public static partial EnergyMintedWire Wire(BimEvent.EnergyMinted fact);
}

// WireCodec holds the converters the generator resolves by type pair, each the ONE spelling of its crossing: a
// content key to its 32-hex form, a generated vocabulary row to its key, an absent option to the nullable slot, a
// Seq to the wire array. User mappings win over Mapperly's built-ins, so `UInt128` never falls to `ToString`.
public static class WireCodec {
    public static string Hex(UInt128 contentKey) => contentKey.ToString("x32", CultureInfo.InvariantCulture);
    public static string Key(BimIssueMutation mutation) => mutation.Key;
    public static string Key(InterchangeFormat format) => format.Key;
    public static string Key(ArtifactKey artifact) => artifact.Value;
    public static string? Text(Option<string> value) => value.Match(static v => v, static () => (string?)null);
    // The sets render off their VALUE, already sorted-distinct by construction, so the wire array is canonical
    // without a sort at the mapper and two mints of one fact render byte-identically.
    public static ImmutableArray<string> Keys(ContentKeySet keys) => [.. keys.Value.Map(Hex)];
    public static ImmutableArray<string> Texts(GlobalIdSet values) => [.. values.Value];
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// One envelope owner covers both directions and both call modalities: Seal mints, Encode emits one or many, Open
// admits the forward's own carrier back. Declaration reads ONE roster at construction and on every decode, so
// traceparent/tracestate round-trip typed; one static formatter is the codec identity every app-tier transport
// binding shares.
public static class BimEnvelope {
    public const string PayloadContentType = "application/json";

    // W3C tracing has no SDK helper, so these two rows hand-declare. Every attribute the SDK ALREADY owns joins this
    // roster as that helper's own surface (Partitioning.AllAttributes, Sampling.AllAttributes,
    // Sequence.AllAttributes, each beside its Set*/Get* pair), never a second CreateExtension row re-spelling a name
    // this package already parses, formats, and range-validates.
    public static readonly CloudEventAttribute TraceParent = CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String);
    public static readonly CloudEventAttribute TraceState = CloudEventAttribute.CreateExtension("tracestate", CloudEventAttributeType.String);

    // Declaration spells the roster ONCE: seal and every decode read this one list, so an attribute declared for
    // one write and forgotten at its read — where it silently decodes as an untyped string row — cannot happen.
    public static readonly Seq<CloudEventAttribute> Declared = Seq(TraceParent, TraceState);

    static readonly JsonEventFormatter Formatter = new();

    // Seal is a BOUNDARY, not a pure projection: the SDK's Validate throws on a malformed envelope, so the mint
    // captures through the one Try.lift funnel onto the typed rail and a caller composing events never has a
    // construction fault escape past its own Fin signature.
    public static Fin<CloudEvent> Seal(BimEvent fact, Uri source, string id, Instant at, Op key) =>
        Try.lift(() => Traced(new CloudEvent(CloudEventsSpecVersion.V1_0, Declared) {
                Id = id,
                Source = source,
                Type = fact.EventType,
                Subject = fact.Subject,
                Time = at.ToDateTimeOffset(),
                DataContentType = PayloadContentType,
                Data = Payload(fact),
            }).Validate())
            .Run()
            .MapFail(error => (Error)Detail.EventEnvelopeMalformed.At(key, error.Message));

    // Ambient activity reads ONCE at the mint; tracestate rides only when non-empty and an absent activity
    // stamps nothing, so an untraced process emits a valid envelope rather than an empty header.
    static CloudEvent Traced(CloudEvent envelope) {
        if (Activity.Current is { Id: { } parent } span) {
            envelope[TraceParent] = parent;
            if (span.TraceStateString is { Length: > 0 } state) { envelope[TraceState] = state; }
        }
        return envelope;
    }

    // ONE encode entry covers both framings, the ARITY of the value discriminating: one envelope emits the
    // structured-mode `application/cloudevents+json` body, two or more the batch-mode array — a `batch` flag beside
    // these events would re-describe what the span's own length already answers. Framing each formatter member
    // yields through its `out ContentType` CARRIES forward, so the transport binding stamps it and Open reads it.
    // ZERO is neither framing and it is not a message: the batch formatter renders an empty array, which decodes
    // back as a batch that carried nothing — indistinguishable at the consumer from a broker that dropped every
    // member. The arity is a THIRD state, so the entry rails and a caller that composed no facts learns it here
    // rather than shipping an envelope whose emptiness reads as loss.
    public static Fin<EncodedEnvelope> Encode(Op key, params ReadOnlySpan<CloudEvent> envelopes) {
        if (envelopes.IsEmpty) {
            return Fin.Fail<EncodedEnvelope>(Detail.EventBatchEmpty.At(key, "encode"));
        }
        ContentType framing;
        ReadOnlyMemory<byte> body = envelopes is [var single]
            ? Formatter.EncodeStructuredModeMessage(single, out framing)
            : Formatter.EncodeBatchModeMessage(Iterable<CloudEvent>.FromSpan(envelopes), out framing);
        return Fin.Succ(new EncodedEnvelope(body, framing));
    }

    // ONE decode entry consuming the forward's OWN carrier: the framing media type selects the reader through the
    // package's own batch prefix constant (`application/cloudevents-batch`, which each formatter extends with its
    // format suffix — so the probe never spells a literal), and BOTH legs land the same Seq, a structured body
    // yielding one row. Every decoded envelope re-admits through its type-dispatched shape and re-proves its
    // subject, so a batch admits member by member and one malformed member fails the whole message rather than
    // half-admitting it.
    public static Fin<Seq<(BimEvent Fact, CloudEvent Envelope)>> Open(EncodedEnvelope message, Op key) =>
        Try.lift(() => message.Framing.MediaType.StartsWith(MimeUtilities.BatchMediaType, StringComparison.Ordinal)
                ? toSeq(Formatter.DecodeBatchModeMessage(message.Body, message.Framing, Declared))
                : Seq(Formatter.DecodeStructuredModeMessage(message.Body, message.Framing, Declared)))
            .Run()
            .MapFail(error => (Error)Detail.EventEnvelopeMalformed.At(key, error.Message))
            .Bind(envelopes => envelopes.IsEmpty
                ? Fin.Fail<Seq<CloudEvent>>(Detail.EventBatchEmpty.At(key, "open"))
                : Fin.Succ(envelopes))
            .Bind(envelopes => envelopes.TraverseM(envelope => Admitted(envelope, key)).As());

    // One envelope back to its fact: the body admits through the type-dispatched wire shape, then the envelope
    // subject must equal the admitted fact's DERIVED subject, so a re-addressed envelope never passes as its payload.
    static Fin<(BimEvent Fact, CloudEvent Envelope)> Admitted(CloudEvent envelope, Op key) =>
        envelope.Data is JsonElement data
            ? Admit(envelope.Type, data, key).Bind(fact => StringComparer.Ordinal.Equals(envelope.Subject, fact.Subject)
                ? Fin.Succ((fact, envelope))
                : Fin.Fail<(BimEvent, CloudEvent)>(Detail.EventSubjectMismatch.At(key, envelope.Subject ?? "", fact.Subject)))
            : Fin.Fail<(BimEvent, CloudEvent)>(Detail.EventBodyMiss.At(key, envelope.Type ?? ""));

    // Dispatch stays here because each case serializes through its OWN JsonTypeInfo — a flat camelCase record
    // per type IS the contract — so this Switch owns only the type-info pairing while the generator owns every
    // arm's slot correspondence.
    static JsonElement Payload(BimEvent fact) => fact.Switch(
        commitLanded:   static c => JsonSerializer.SerializeToElement(BimEventWire.Wire(c), BimEventContext.Default.CommitLandedWire),
        issueMutated:   static i => JsonSerializer.SerializeToElement(BimEventWire.Wire(i), BimEventContext.Default.IssueMutatedWire),
        verdictIssued:  static v => JsonSerializer.SerializeToElement(BimEventWire.Wire(v), BimEventContext.Default.VerdictIssuedWire),
        artifactMinted: static a => JsonSerializer.SerializeToElement(BimEventWire.Wire(a), BimEventContext.Default.ArtifactMintedWire),
        energyMinted:   static e => JsonSerializer.SerializeToElement(BimEventWire.Wire(e), BimEventContext.Default.EnergyMintedWire));

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
            from globalIds in GlobalIdSet.Admit(w.GlobalIds, key)
            select (BimEvent)new BimEvent.IssueMutated(topic, mutation, comment, globalIds)),
        BimEvent.VerdictType => Wire(data, BimEventContext.Default.VerdictIssuedWire, key).Bind(w =>
            from specification in Required(w.Specification, "specification", key)
            from spec in NonNegative(w.Spec, "spec", key)
            from findings in NonNegative(w.Findings, "findings", key)
            from globalIds in GlobalIdSet.Admit(w.GlobalIds, key)
            select (BimEvent)new BimEvent.VerdictIssued(specification, spec, w.Conforms, findings, globalIds)),
        BimEvent.ArtifactType => Wire(data, BimEventContext.Default.ArtifactMintedWire, key).Bind(w =>
            from content in HexKey(w.ContentKey, key)
            from format in InterchangeFormat.Detect(w.Format ?? "", key)
            from bytes in NonNegative(w.Bytes, "bytes", key)
            select (BimEvent)new BimEvent.ArtifactMinted(content, format, bytes)),
        BimEvent.EnergyType => Wire(data, BimEventContext.Default.EnergyMintedWire, key).Bind(w =>
            from artifact in ArtifactKey.Admit(w.ArtifactKey, key)
            from leg in Required(w.Leg, "leg", key)
            from format in InterchangeFormat.Detect(w.Format ?? "", key)
            from warnings in NonNegative(w.Warnings, "warnings", key)
            select (BimEvent)new BimEvent.EnergyMinted(artifact, leg, format, warnings)),
        var unknown => Fin.Fail<BimEvent>(Detail.EventTypeMiss.At(key, unknown ?? "")),
    };

    static Fin<T> Wire<T>(JsonElement data, JsonTypeInfo<T> shape, Op key) where T : class =>
        Try.lift(() => data.Deserialize(shape)).Run()
            .MapFail(error => (Error)Detail.EventPayloadDecode.At(key, error.Message))
            .Bind(wire => wire is null
                ? Fin.Fail<T>(Detail.EventBodyMiss.At(key, "payload-null"))
                : Fin.Succ(wire));

    static Fin<BimIssueMutation> IssueMutation(string? value, Op key) =>
        value is not null && BimIssueMutation.TryGet(value, out var mutation)
            ? Fin.Succ(mutation)
            : Fin.Fail<BimIssueMutation>(Detail.EventMutationMiss.At(key, value ?? ""));

    // The slot-parameterized admissions carry their wire-slot name as a SUBJECT on their own roster row, so the
    // family has the fixed grep prefix the retired infixed grammar (`event-<slot>-malformed`) could never own.
    // Producer text NORMALIZES once at the envelope boundary rather than being refused: surrounding whitespace is
    // a producer's formatting artifact and never a semantic difference, so rejecting it turned away legal upstream
    // values over a defect the boundary can simply remove. Emptiness after the trim is the real failure.
    static Fin<string> Required(string? value, string slot, Op key) =>
        value?.Trim() is { Length: > 0 } trimmed
            ? Fin.Succ(trimmed)
            : Fin.Fail<string>(Detail.EventSlotMalformed.At(key, slot));

    static Fin<T> NonNegative<T>(T value, string slot, Op key) where T : INumber<T> => value >= T.Zero
        ? Fin.Succ(value)
        : Fin.Fail<T>(Detail.EventSlotNegative.At(key, slot, value.ToString() ?? ""));

    static Fin<string> GuidText(string? value, string slot, Op key) =>
        value is not null
        && Guid.TryParseExact(value, "D", out Guid parsed)
        && StringComparer.Ordinal.Equals(value, parsed.ToString("D"))
            ? Fin.Succ(value)
            : Fin.Fail<string>(Detail.EventSlotMalformed.At(key, slot, value ?? ""));

    static Fin<Option<string>> OptionalGuid(string? value, string slot, Op key) => value is null
        ? Fin.Succ(Option<string>.None)
        : GuidText(value, slot, key).Map(Some);

    // The content-key set proves its ordering on the WIRE TEXT — the fixed-width 32-hex rendering the outbound
    // half emits — then mints through the set owner, which re-normalizes on the numeric values. The two agree by
    // construction because the rendering is order-preserving at fixed width.
    static Fin<ContentKeySet> ContentKeys(ImmutableArray<string> values, string slot, Op key) =>
        WireSet.Ordered(values)
            ? toSeq(values).TraverseM(value => HexKey(value, key)).As().Map(ContentKeySet.Of)
            : Fin.Fail<ContentKeySet>(Detail.EventSetMalformed.At(key, slot));

    // Round-tripping against the ONE outbound hex spelling IS the gate: a parse alone admits upper-case and
    // short forms this fabric never emits, so the admitted key re-renders and must match byte for byte.
    static Fin<UInt128> HexKey(string? hex, Op key) =>
        hex is { Length: 32 }
        && UInt128.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 value)
        && StringComparer.Ordinal.Equals(hex, WireCodec.Hex(value))
            ? Fin.Succ(value)
            : Fin.Fail<UInt128>(Detail.EventKeyMalformed.At(key, hex ?? ""));
}
```

## [03]-[RESEARCH]

(none)
