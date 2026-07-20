# [RASM_FABRICATION_TRAVELER]

`TravelerDocument` is the deterministic shop-execution document assembled from admitted fabrication results and cross-domain receipts. It preserves each upstream receipt at its owning type; `ContentKey.Of(EgressKind.Traveler, bytes)` mints document and amendment identities, and `TravelerArtifact` carries canonical bytes for rendering and persistence.

`Fabrication.Run` remains the sole public package entry. `Traveler.Assemble` is internal, owns canonical encoding, and parameterizes the clock and result projection; rendering and persistence consume the resulting `TravelerArtifact`.

## [01]-[INDEX]

- [01]-[TRAVELER]: scalar owners and `TravelerReceiptCorpus` admit fan-in; `TravelerControl` owns instructions; `TravelerSection.Outputs` preserves every result; `TravelerAmendment` owns the `TravelerStepState` arrow; `Traveler.Assemble` builds, encodes, keys, chains, and projects one artifact.

## [02]-[TRAVELER]

`TravelerReceiptCorpus` composes tooling, setup, feature-frame, capability, manufacturability, procedure, sealed-record, inspection-link, control, and amendment owners. Its digital-product-passport identity derives from sealed records, so no writable twin can diverge. `TravelerText`, `TravelerQuantity`, and ordinal owners admit shared scalar regimes; `TravelerIdentity` composes work order, part, revision, quantity, heat lot, and serial identity.

`TravelerControl` is one generated family over `TravelerLocus`. Global, step, operation, setup, and characteristic loci bind instructions; `Material` retains unit identity, and `Package` fixes the global locus with label, method, and destination policy. `Safety` carries residual-risk rank; `Inspect` carries every, first-article, skip-lot, or attribute sampling evidence. New capability grows as a case, and multiplicity grows as corpus rows.

`BindRoutes` proves every control locus, amendment step, and inspection link against the planned route, accumulating the three classes independently: a corpus whose controls, amendments, and inspection links all dangle reports all three witnesses with their counts in one verdict, so a planner never re-runs assembly to discover the next class of break.

`TravelerSection` collapses the document model into direct `Header`, `Route`, `Tooling`, `Specification`, `Procedure`, `Outputs`, and `Quality` cases. `Outputs` retains the complete `FabricationResult` sequence and document dialect instead of reducing program, projection, placement, additive, verification, inspection, plan, forming, motion, or prior-traveler evidence to selected fields. Section order follows construction, so no parallel rank roster restates the closed family.

`TravelerCanonicalCodec.Encode` receives one `TravelerCanonicalSource` case carrying the whole `TravelerDocument` or `TravelerAmendment`, serializes through `QualityReport.CanonicalJson`, normalizes text, sorts object properties ordinally, preserves semantic array order, and returns canonical UTF-8 JSON with its fixed artifact descriptor. `Traveler` owns identity minting from those bytes, and each document or amendment receipt retains its descriptor and canonical bytes beside the minted identity.

`TravelerAmendment` models execution without mutating the planned document. `Completed`, `Held`, `Released`, `Deviated`, and `Scrapped` cases record predecessor key, admitted step and actor, timestamp, evidence, and case-specific duration or disposition; `Completed.Estimate` retains the `CostReceipt` clock and derives actual-versus-estimated variance. `Deviated` and `Scrapped` carry `TravelerUnits`, so a lot-wide disposition and a named-serial disposition are distinct cases and partial scrap of a serialized run records the exact units it consumed.

`TravelerAmendment.Advance` owns the step-state arrow as one total generated dispatch, and `Disposition.Terminal` with `Accepted` supplies the `Deviated` target: an accepted terminal disposition completes the step, a refused terminal disposition scraps it, and a nonterminal disposition retains prior state. `SealAmendments` folds the sequence against the document key and per-step `TravelerStepState`, rejecting broken predecessors, non-monotone time, illegal transitions, and post-terminal events before emitting an immutable content-key chain.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Fabrication.Tooling;
using Rasm.Fabrication.Verify;
using Rasm.Numerics;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Documentation;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Rank orders the hierarchy of controls ascending by residual risk, so a safety fold reports the
// weakest admitted control without re-deriving the ordering at each reader.
[SmartEnum<string>]
public sealed partial class SafetyControlLevel {
    public static readonly SafetyControlLevel Elimination    = new("elimination", rank: 0);
    public static readonly SafetyControlLevel Substitution   = new("substitution", rank: 1);
    public static readonly SafetyControlLevel Engineering    = new("engineering", rank: 2);
    public static readonly SafetyControlLevel Administrative = new("administrative", rank: 3);
    public static readonly SafetyControlLevel Protective     = new("protective", rank: 4);

    private SafetyControlLevel(string key, int rank) : this(key) => Rank = rank;

    public int Rank { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct TravelerText {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0 ? new ValidationError("traveler:text") : null;
    }
}

[ValueObject<int>]
public readonly partial struct TravelerQuantity {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 1 ? new ValidationError("traveler:quantity") : null;
}

[ValueObject<int>]
public readonly partial struct TravelerStep {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 0 ? new ValidationError("traveler:step") : null;
}

[ValueObject<int>]
public readonly partial struct TravelerOperation {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 0 ? new ValidationError("traveler:operation") : null;
}

[ValueObject<int>]
public readonly partial struct TravelerSetup {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 0 ? new ValidationError("traveler:setup") : null;
}

// Lot and serialized dispositions remain separate; a count beside serials would force consumer sniffing.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerUnits {
    private TravelerUnits() { }

    public sealed record Lot(TravelerQuantity Value) : TravelerUnits;
    public sealed record Serialized(Seq<TravelerText> Values) : TravelerUnits;

    public int Count => Switch(
        lot:        static value => value.Value.ToValue(),
        serialized: static value => value.Values.Count);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerSampling {
    private TravelerSampling() { }

    public sealed record Every : TravelerSampling;
    public sealed record FirstArticle : TravelerSampling;
    public sealed record Skip(TravelerQuantity Interval) : TravelerSampling;
    public sealed record AttributePlan(TravelerQuantity SampleSize, int Accept, int Reject) : TravelerSampling;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerLocus {
    private TravelerLocus() { }

    public sealed record Global : TravelerLocus;
    public sealed record Step(TravelerStep Value) : TravelerLocus;
    public sealed record Operation(TravelerStep Step, TravelerOperation Value) : TravelerLocus;
    public sealed record Setup(TravelerSetup Value) : TravelerLocus;
    public sealed record Characteristic(CharacteristicId Value) : TravelerLocus;
}

[ComplexValueObject]
public sealed partial class TravelerIdentity {
    public TravelerText WorkOrder { get; }
    public TravelerText PartNumber { get; }
    public TravelerText Revision { get; }
    public TravelerQuantity Quantity { get; }
    public Option<TravelerText> HeatLot { get; }
    public Seq<TravelerText> Serials { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TravelerText workOrder,
        ref TravelerText partNumber,
        ref TravelerText revision,
        ref TravelerQuantity quantity,
        ref Option<TravelerText> heatLot,
        ref Seq<TravelerText> serials) {
        bool valuesValid = workOrder != default && partNumber != default && revision != default && quantity != default
            && heatLot.ForAll(static value => value != default);
        bool serialsValid = serials.ForAll(static value => value != default)
            && serials.Distinct().Count == serials.Count
            && (serials.IsEmpty || serials.Count == quantity.ToValue());
        if (!valuesValid || !serialsValid)
            validationError = new ValidationError("traveler:identity");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerControl {
    private TravelerControl(TravelerLocus locus) => Locus = locus;

    public TravelerLocus Locus { get; }

    public sealed record Work : TravelerControl {
        public Work(TravelerLocus locus, TravelerText instruction) : base(locus) => Instruction = instruction;
        public TravelerText Instruction { get; }
    }

    public sealed record Hold : TravelerControl {
        public Hold(TravelerLocus locus, TravelerText authority) : base(locus) => Authority = authority;
        public TravelerText Authority { get; }
    }

    public sealed record Safety : TravelerControl {
        public Safety(
            TravelerLocus locus,
            TravelerText hazard,
            SafetyControlLevel level,
            TravelerText control,
            Seq<TravelerText> protectiveEquipment)
            : base(locus) => (Hazard, Level, Control, ProtectiveEquipment) = (hazard, level, control, protectiveEquipment);
        public TravelerText Hazard { get; }
        public SafetyControlLevel Level { get; }
        public TravelerText Control { get; }
        public Seq<TravelerText> ProtectiveEquipment { get; }
    }

    public sealed record Material : TravelerControl {
        public Material(TravelerLocus locus, TravelerText item, TravelerText lot, IQuantity quantity)
            : base(locus) => (Item, Lot, Quantity) = (item, lot, quantity);
        public TravelerText Item { get; }
        public TravelerText Lot { get; }
        public IQuantity Quantity { get; }
    }

    public sealed record Resource : TravelerControl {
        public Resource(TravelerLocus locus, TravelerText name, TravelerQuantity quantity)
            : base(locus) => (Name, Quantity) = (name, quantity);
        public TravelerText Name { get; }
        public TravelerQuantity Quantity { get; }
    }

    public sealed record Inspect : TravelerControl {
        public Inspect(
            TravelerLocus locus,
            TravelerText method,
            TravelerText gauge,
            TravelerSampling sampling,
            TravelerText authority)
            : base(locus) => (Method, Gauge, Sampling, Authority) = (method, gauge, sampling, authority);
        public TravelerText Method { get; }
        public TravelerText Gauge { get; }
        public TravelerSampling Sampling { get; }
        public TravelerText Authority { get; }
    }

    public sealed record Approve : TravelerControl {
        public Approve(TravelerLocus locus, TravelerText role, TravelerText authority)
            : base(locus) => (Role, Authority) = (role, authority);
        public TravelerText Role { get; }
        public TravelerText Authority { get; }
    }

    public sealed record Package : TravelerControl {
        public Package(TravelerText label, TravelerText method, TravelerText destination)
            : base(new TravelerLocus.Global()) => (Label, Method, Destination) = (label, method, destination);
        public TravelerText Label { get; }
        public TravelerText Method { get; }
        public TravelerText Destination { get; }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerAmendment {
    private TravelerAmendment(ContentKey previous, TravelerStep step, TravelerText actor, Instant at, Seq<ContentKey> evidence) =>
        (Previous, Step, Actor, At, Evidence) = (previous, step, actor, at, evidence);

    public ContentKey Previous { get; }
    public TravelerStep Step { get; }
    public TravelerText Actor { get; }
    public Instant At { get; }
    public Seq<ContentKey> Evidence { get; }

    public sealed record Completed : TravelerAmendment {
        public Completed(
            ContentKey previous,
            TravelerStep step,
            TravelerText actor,
            Instant started,
            Instant completed,
            Duration actual,
            Option<CostReceipt> estimate,
            Seq<ContentKey> evidence)
            : base(previous, step, actor, completed, evidence) => (Started, Actual, Estimate) = (started, actual, estimate);
        public Instant Started { get; }
        public Duration Actual { get; }
        public Option<CostReceipt> Estimate { get; }
        public Option<Duration> Variance => Estimate.Map(value => Actual - value.MachineTime);
    }

    public sealed record Held : TravelerAmendment {
        public Held(ContentKey previous, TravelerStep step, TravelerText actor, Instant at, TravelerText cause, Seq<ContentKey> evidence)
            : base(previous, step, actor, at, evidence) => Cause = cause;
        public TravelerText Cause { get; }
    }

    public sealed record Released : TravelerAmendment {
        public Released(ContentKey previous, TravelerStep step, TravelerText actor, Instant at, TravelerText authority, Seq<ContentKey> evidence)
            : base(previous, step, actor, at, evidence) => Authority = authority;
        public TravelerText Authority { get; }
    }

    public sealed record Deviated : TravelerAmendment {
        public Deviated(
            ContentKey previous,
            TravelerStep step,
            TravelerText actor,
            Instant at,
            TravelerText deviation,
            Disposition disposition,
            TravelerUnits units,
            TravelerText authority,
            Seq<ContentKey> evidence)
            : base(previous, step, actor, at, evidence) =>
            (Deviation, Disposition, Units, Authority) = (deviation, disposition, units, authority);
        public TravelerText Deviation { get; }
        public Disposition Disposition { get; }
        public TravelerUnits Units { get; }
        public TravelerText Authority { get; }
    }

    public sealed record Scrapped : TravelerAmendment {
        public Scrapped(
            ContentKey previous,
            TravelerStep step,
            TravelerText actor,
            Instant at,
            TravelerText reason,
            TravelerUnits units,
            TravelerText authority,
            Seq<ContentKey> evidence)
            : base(previous, step, actor, at, evidence) => (Reason, Units, Authority) = (reason, units, authority);
        public TravelerText Reason { get; }
        public TravelerUnits Units { get; }
        public TravelerText Authority { get; }
    }

    // Total over the case family: a sixth amendment breaks this dispatch at the owner rather than
    // falling through a catch-all into a refusal that reads like a legal transition.
    public Fin<TravelerStepState> Advance(TravelerStepState prior, Op key) => Switch(
        state: (Prior: prior, Key: key),
        completed: static (state, _) => state.Prior == TravelerStepState.Open
            ? Fin.Succ(TravelerStepState.Completed)
            : Fin.Fail<TravelerStepState>(state.Key.InvalidInput()),
        held: static (state, _) => state.Prior == TravelerStepState.Open
            ? Fin.Succ(TravelerStepState.Held)
            : Fin.Fail<TravelerStepState>(state.Key.InvalidInput()),
        released: static (state, _) => state.Prior == TravelerStepState.Held
            ? Fin.Succ(TravelerStepState.Open)
            : Fin.Fail<TravelerStepState>(state.Key.InvalidInput()),
        deviated: static (state, value) => state.Prior.Terminal
            ? Fin.Fail<TravelerStepState>(state.Key.InvalidInput())
            : Fin.Succ(value.Disposition.Terminal
                ? value.Disposition.Accepted ? TravelerStepState.Completed : TravelerStepState.Scrapped
                : state.Prior),
        scrapped: static (state, _) => state.Prior.Terminal
            ? Fin.Fail<TravelerStepState>(state.Key.InvalidInput())
            : Fin.Succ(TravelerStepState.Scrapped));
}

[SmartEnum<int>]
public sealed partial class TravelerStepState {
    public static readonly TravelerStepState Open = new(0, terminal: false);
    public static readonly TravelerStepState Held = new(1, terminal: false);
    public static readonly TravelerStepState Completed = new(2, terminal: true);
    public static readonly TravelerStepState Scrapped = new(3, terminal: true);

    public bool Terminal { get; }
}

public sealed record TravelerAmendmentReceipt(
    ContentKey Key,
    TravelerAmendment Amendment,
    TravelerArtifactDescriptor Descriptor,
    ReadOnlyMemory<byte> Canonical);

public sealed record TravelerInspectionLink(InspectionFeature Feature, ContentKey Record);

[ComplexValueObject]
public sealed partial class TravelerReceiptCorpus {
    public TravelerIdentity Identity { get; }
    public Seq<ToolChange> ToolChanges { get; }
    public Seq<ToolAssembly> ToolAssemblies { get; }
    public Seq<SetupSchedule> Setups { get; }
    public Seq<FeatureFrameReceipt> Frames { get; }
    public Seq<CapabilityReport> Capabilities { get; }
    public Seq<DfmReport> Manufacturability { get; }
    public Seq<ProcedureReceipt> Procedures { get; }
    public Seq<SealedRecord> Records { get; }
    public Option<ContentKey> DigitalProductPassport => Records
        .Bind(static value => value.DigitalProductPassport.ToSeq())
        .Distinct()
        .Head;
    public Seq<TravelerInspectionLink> Inspections { get; }
    public Seq<TravelerControl> Controls { get; }
    public Seq<TravelerAmendment> Amendments { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TravelerIdentity identity,
        ref Seq<ToolChange> toolChanges,
        ref Seq<ToolAssembly> toolAssemblies,
        ref Seq<SetupSchedule> setups,
        ref Seq<FeatureFrameReceipt> frames,
        ref Seq<CapabilityReport> capabilities,
        ref Seq<DfmReport> manufacturability,
        ref Seq<ProcedureReceipt> procedures,
        ref Seq<SealedRecord> records,
        ref Seq<TravelerInspectionLink> inspections,
        ref Seq<TravelerControl> controls,
        ref Seq<TravelerAmendment> amendments) {
        bool membersValid = identity is not null
            && toolChanges.ForAll(static value => value is not null)
            && toolAssemblies.ForAll(static value => value is not null)
            && setups.ForAll(static value => value is not null)
            && frames.ForAll(static value => value is not null)
            && capabilities.ForAll(static value => value is not null)
            && manufacturability.ForAll(static value => value is not null)
            && procedures.ForAll(static value => value is not null)
            && records.ForAll(static value => value is not null)
            && inspections.ForAll(static value => value is not null
                && value.Feature is not null && value.Record is not null)
            && controls.ForAll(static value => value is not null)
            && amendments.ForAll(static value => value is not null);
        bool recordsUnique = membersValid
            && records.Fold(Set<ContentKey>(), static (keys, value) => keys.Add(value.Key)).Count == records.Count;
        bool passportBound = membersValid
            && records.Bind(static value => value.DigitalProductPassport.ToSeq()).Distinct().Count <= 1;
        bool inspectionsBound = membersValid
            && inspections.Distinct().Count == inspections.Count
            && inspections.ForAll(link => records.Exists(record => record.Key == link.Record
                && record.Records.Bind(static value => value.InspectionFeatures).Contains(link.Feature)));
        if (!membersValid || !recordsUnique || !passportBound || !inspectionsBound
            || !controls.ForAll(ValidControl) || !amendments.ForAll(ValidAmendment))
            validationError = new ValidationError("traveler:corpus");
    }

    // Only the characteristic locus decides admission; the routing loci prove membership later
    // against the planned route, where the step, operation, and setup identities exist.
    static bool ValidControl(TravelerControl control) =>
        control.Locus is not null
        && (control.Locus is not TravelerLocus.Characteristic characteristic || characteristic.Value != default)
        && control.Switch(
            work: static value => value.Instruction != default,
            hold: static value => value.Authority != default,
            safety: static value => value.Hazard != default && value.Control != default && value.Level is not null
                && value.ProtectiveEquipment.ForAll(static item => item != default),
            material: static value => value.Item != default && value.Lot != default && value.Quantity is not null
                && double.IsFinite((double)value.Quantity.Value) && (double)value.Quantity.Value > 0.0,
            resource: static value => value.Name != default && value.Quantity != default,
            inspect: static value => value.Method != default && value.Gauge != default
                && ValidSampling(value.Sampling) && value.Authority != default,
            approve: static value => value.Role != default && value.Authority != default,
            package: static value => value.Label != default && value.Method != default && value.Destination != default);

    static bool ValidSampling(TravelerSampling sampling) =>
        sampling is not null && sampling.Switch(
            every: static _ => true,
            firstArticle: static _ => true,
            skip: static value => value.Interval != default,
            attributePlan: static value => value.SampleSize != default
                && value.Accept >= 0
                && value.Reject == value.Accept + 1
                && value.Accept < value.SampleSize.ToValue());

    static bool ValidUnits(TravelerUnits units) =>
        units is not null && units.Switch(
            lot: static value => value.Value != default,
            serialized: static value => !value.Values.IsEmpty
                && value.Values.ForAll(static serial => serial != default)
                && value.Values.Distinct().Count == value.Values.Count);

    static bool ValidAmendment(TravelerAmendment amendment) =>
        amendment.Previous is not null && amendment.Actor != default
        && amendment.Evidence.ForAll(static value => value is not null)
        && amendment.Switch(
            completed: static value => value.Started <= value.At
                && value.Actual >= Duration.Zero
                && value.Actual <= value.At - value.Started
                && value.Estimate.ForAll(static estimate => estimate is not null && estimate.MachineTime >= Duration.Zero),
            held: static value => value.Cause != default,
            released: static value => value.Authority != default,
            deviated: static value => value.Deviation != default && value.Disposition is not null
                && ValidUnits(value.Units) && value.Authority != default,
            scrapped: static value => value.Reason != default && ValidUnits(value.Units) && value.Authority != default);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerSection {
    private TravelerSection() { }

    public sealed record Header(
        TravelerIdentity Identity,
        ProcessKind Process,
        Machine Machine,
        ProjectionDir View,
        Instant StampedAt,
        Seq<ContentKey> Sources) : TravelerSection;
    public sealed record Route(
        Seq<PlannedStep> Steps,
        Seq<SetupSchedule> Setups,
        Seq<StockSnapshot> Stock,
        Seq<TravelerControl> Controls) : TravelerSection;
    public sealed record Tooling(Seq<ToolChange> Changes, Seq<ToolAssembly> Assemblies) : TravelerSection;
    public sealed record Specification(
        Seq<FeatureFrameReceipt> Frames,
        Seq<CapabilityReport> Capabilities,
        Seq<DfmReport> Manufacturability) : TravelerSection;
    public sealed record Procedure(Seq<ProcedureReceipt> Receipts) : TravelerSection;
    public sealed record Outputs(Option<PostDialect> Dialect, Seq<FabricationResult> Results) : TravelerSection;
    public sealed record Quality(Seq<SealedRecord> Records, Seq<TravelerInspectionLink> Inspections) : TravelerSection;
}

public sealed record TravelerDocument(Instant StampedAt, Seq<TravelerSection> Sections, Seq<ContentKey> Composed);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerCanonicalSource {
    private TravelerCanonicalSource() { }

    public sealed record Document(TravelerDocument Value) : TravelerCanonicalSource;
    public sealed record Amendment(TravelerAmendment Value) : TravelerCanonicalSource;
}

public sealed record TravelerArtifactDescriptor(string Schema, string MediaType, string Encoding);
public sealed record TravelerEncoding(TravelerArtifactDescriptor Descriptor, ReadOnlyMemory<byte> Canonical);

public sealed record TravelerArtifact(
    TravelerDocument Document,
    TravelerArtifactDescriptor Descriptor,
    ReadOnlyMemory<byte> Canonical,
    ContentKey Key,
    Seq<ContentKey> Consumed,
    Seq<ContentKey> Produced,
    Option<ContentKey> DigitalProductPassport,
    Seq<TravelerAmendmentReceipt> Amendments);

internal static class TravelerCanonicalCodec {
    static readonly TravelerArtifactDescriptor Descriptor = new(
        "rasm.fabrication.traveler", "application/json", "utf-8");

    public static Fin<TravelerEncoding> Encode(TravelerCanonicalSource source) =>
        Try.lift(() => {
                JsonNode root = source.Switch<JsonNode>(
                    document: static value => JsonSerializer.SerializeToNode(value.Value, QualityReport.CanonicalJson)!,
                    amendment: static value => JsonSerializer.SerializeToNode(
                        value.Value,
                        value.Value.GetType(),
                        QualityReport.CanonicalJson)!);
                using MemoryStream stream = new();
                using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false }))
                    Write(writer, root);
                return new TravelerEncoding(Descriptor, stream.ToArray());
            })
            .Run()
            .MapFail(static error => Op.Of(name: "fabrication:traveler-codec").InvalidResult(detail: error.Message));

    static void Write(Utf8JsonWriter writer, JsonNode node) {
        switch (node) {
            case JsonObject value:
                writer.WriteStartObject();
                foreach ((string key, JsonNode? item) in value.OrderBy(static pair => pair.Key, StringComparer.Ordinal)) {
                    writer.WritePropertyName(key.Normalize(NormalizationForm.FormC));
                    if (item is null) writer.WriteNullValue(); else Write(writer, item);
                }
                writer.WriteEndObject();
                break;
            case JsonArray value:
                writer.WriteStartArray();
                foreach (JsonNode? item in value)
                    if (item is null) writer.WriteNullValue(); else Write(writer, item);
                writer.WriteEndArray();
                break;
            case JsonValue value when value.GetValue<JsonElement>() is { ValueKind: JsonValueKind.String } element:
                writer.WriteStringValue(element.GetString()!.Normalize(NormalizationForm.FormC));
                break;
            case JsonValue value:
                value.GetValue<JsonElement>().WriteTo(writer);
                break;
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
internal static class Traveler {
    static readonly Op TravelerOp = Op.Of(name: "fabrication:traveler");

    internal static Fin<FabricationResult> Assemble(
        FabricationPolicy.Document request,
        FabricationInput input,
        IClock clock,
        Func<TravelerArtifact, FabricationResult> egress) =>
        from document in Build(request, input, clock.GetCurrentInstant())
        from encoded in TravelerCanonicalCodec.Encode(new TravelerCanonicalSource.Document(document))
        let key = ContentKey.Of(EgressKind.Traveler, encoded.Canonical.Span)
        from amendments in SealAmendments(key, document.StampedAt, request.Corpus.Amendments)
        let consumed = (Seq(key)
            + document.Composed
            + amendments.Map(static value => value.Amendment.Previous)
            + amendments.Bind(static value => value.Amendment.Evidence)
            + amendments.Choose(static value => value.Amendment is TravelerAmendment.Completed completed
                ? completed.Estimate.Map(static estimate => estimate.Subject)
                : None))
            .Distinct()
            .OrderBy(static value => value.Kind.Key)
            .ThenBy(static value => value.Digest)
            .ToSeq()
        select egress(new TravelerArtifact(
            document,
            encoded.Descriptor,
            encoded.Canonical,
            key,
            consumed,
            Seq(key) + amendments.Map(static value => value.Key),
            request.Corpus.DigitalProductPassport,
            amendments));

    static Fin<TravelerDocument> Build(FabricationPolicy.Document request, FabricationInput input, Instant stampedAt) =>
        request.Results.Fold(
            (Steps: Seq<PlannedStep>(), Keys: Seq<ContentKey>()),
            Gather) switch {
                var harvested =>
                    from _ in BindRoutes(request.Corpus, harvested.Steps, request.Results)
                    let composed = (
                        input.ParentRuns
                        + input.Sources
                        + input.MaterialCertificate.ToSeq()
                        + request.Corpus.DigitalProductPassport.ToSeq()
                        + request.Corpus.Records.Map(static value => value.Key)
                        + harvested.Keys)
                        .Distinct()
                        .OrderBy(static value => value.Kind.Key)
                        .ThenBy(static value => value.Digest)
                        .ToSeq()
                    let sections = Seq<TravelerSection>(
                        new TravelerSection.Header(request.Corpus.Identity, input.Process, input.Machine, input.View, stampedAt, input.Sources),
                        new TravelerSection.Route(harvested.Steps, request.Corpus.Setups, input.Snapshots, request.Corpus.Controls),
                        new TravelerSection.Tooling(request.Corpus.ToolChanges, request.Corpus.ToolAssemblies),
                        new TravelerSection.Specification(request.Corpus.Frames, request.Corpus.Capabilities, request.Corpus.Manufacturability),
                        new TravelerSection.Procedure(request.Corpus.Procedures),
                        new TravelerSection.Outputs(request.Dialect, request.Results),
                        new TravelerSection.Quality(request.Corpus.Records, request.Corpus.Inspections))
                    select new TravelerDocument(stampedAt, sections, composed),
            };

    readonly record struct RouteIndex(
        Set<int> Steps,
        Set<int> Setups,
        Set<CharacteristicId> Characteristics,
        Seq<InspectionFeature> Inspections);

    // Dangling controls, amendments, and inspection links are independent faults: a planner
    // correcting one route must see the other two in the same verdict, so the three gates
    // accumulate rather than short-circuit on whichever class happens to fail first.
    static Fin<Unit> BindRoutes(
        TravelerReceiptCorpus corpus,
        Seq<PlannedStep> planned,
        Seq<FabricationResult> results) =>
        Index(planned, results) switch {
            var available => (
                Bound(corpus.Controls.Filter(control => !Routed(control.Locus, available, planned)), "traveler:control-route"),
                Bound(corpus.Amendments.Filter(value => !available.Steps.Contains(value.Step.ToValue())), "traveler:amendment-step"),
                Bound(corpus.Inspections.Filter(link => !available.Inspections.Contains(link.Feature)), "traveler:inspection-feature"))
                .Apply(static (_, _, _) => unit)
                .As()
                .ToFin(),
        };

    static RouteIndex Index(Seq<PlannedStep> planned, Seq<FabricationResult> results) =>
        new(toSet(planned.Map(static value => value.Order)),
            toSet(planned.Map(static value => value.Setup)),
            results
                .Choose(static result => result is FabricationResult.HiddenLineResult projection
                    ? Some(projection.Projection.Characteristics)
                    : None)
                .Bind(static values => values)
                .Map(static value => value.Characteristic.Id)
                .ToSet(),
            results
                .Choose(static result => result is FabricationResult.InspectionResult inspection
                    ? Some(inspection.Features)
                    : None)
                .Bind(static values => values)
                .ToSeq());

    static bool Routed(TravelerLocus locus, RouteIndex available, Seq<PlannedStep> planned) =>
        locus.Switch(
            global: static _ => true,
            step: value => available.Steps.Contains(value.Value.ToValue()),
            operation: value => planned.Exists(step => step.Order == value.Step.ToValue()
                && step.Operations.Contains(value.Value.ToValue())),
            setup: value => available.Setups.Contains(value.Value.ToValue()),
            characteristic: value => available.Characteristics.Contains(value.Value));

    static K<Validation<Error>, Unit> Bound<T>(Seq<T> unbound, string locus) =>
        guard(unbound.IsEmpty, new GeometryFault.DegenerateInput(Kind.Brep, -1, $"{locus}:{unbound.Count}").ToError())
            .ToFin()
            .ToValidation();

    static (Seq<PlannedStep> Steps, Seq<ContentKey> Keys) Gather(
        (Seq<PlannedStep> Steps, Seq<ContentKey> Keys) state,
        FabricationResult result) =>
        result.Switch(
            hiddenLineResult: value => state with { Keys = state.Keys + value.Projection.Sources },
            motion: _ => state,
            placement: value => state with { Keys = state.Keys.Add(value.Key) },
            additiveResult: value => state with { Keys = state.Keys + value.Artifacts },
            verificationResult: value => state with {
                Keys = state.Keys + value.Snapshots.Map(static snapshot => snapshot.Key) + Seq(value.Residual.Key),
            },
            inspectionResult: _ => state,
            postedProgram: value => state with { Keys = state.Keys.Add(value.Key) },
            travelerDocument: value => state with { Keys = state.Keys + Seq(value.Key) + value.Consumed + value.Produced },
            fabricationPlan: value => state with {
                Steps = state.Steps + value.Steps,
                Keys = state.Keys + Seq(value.Key) + value.Artifacts,
            },
            formedResult: value => state with { Keys = state.Keys.Add(value.Key) });

    static Fin<Seq<TravelerAmendmentReceipt>> SealAmendments(
        ContentKey root,
        Instant stampedAt,
        Seq<TravelerAmendment> amendments) =>
        amendments.FoldM(
            (Previous: root,
             At: stampedAt,
             Receipts: Seq<TravelerAmendmentReceipt>(),
             States: HashMap<TravelerStep, TravelerStepState>()),
            (state, amendment) =>
                from _ in guard(amendment.Previous == state.Previous, TravelerOp.InvalidInput()).ToFin()
                from __ in guard(
                    amendment.At >= state.At
                    && (amendment is not TravelerAmendment.Completed completed || completed.Started >= stampedAt),
                    TravelerOp.InvalidInput()).ToFin()
                let prior = state.States.Find(amendment.Step).IfNone(TravelerStepState.Open)
                from next in amendment.Advance(prior, TravelerOp)
                from encoded in TravelerCanonicalCodec.Encode(new TravelerCanonicalSource.Amendment(amendment))
                let key = ContentKey.Of(EgressKind.Traveler, encoded.Canonical.Span)
                select (
                    Previous: key,
                    At: amendment.At,
                    Receipts: state.Receipts.Add(new TravelerAmendmentReceipt(
                        key,
                        amendment,
                        encoded.Descriptor,
                        encoded.Canonical)),
                    States: state.States.SetItem(amendment.Step, next)))
            .Map(static state => state.Receipts);
}
```
