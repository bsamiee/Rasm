# [COST_LEDGER]

Rasm.Compute prices every execution once. The rate table enters at the composition root as external policy, `CostPolicy.Price` reads one settled `ComputeOutput` beside its substrate and elapsed for what it cost, `Charges.Settle` publishes the `rasm.compute.charge.priced` CloudEvent whose `data` is the `Charge`, and the tenant-partitioned chargeback dataset folds the landed charge journal into billing truth under one content key. No rate literal or second pricing fold lives in the package.

## [01]-[INDEX]

- [02]-[COST_ALGEBRA]: `CostVector` — the decomposed per-axis cost monoid and its billing diff rail.
- [03]-[RATE_POLICY]: `CostPolicy` — the composition-admitted rate table, its total substrate coverage proof, and the one pricing read over a settled output.
- [04]-[CHARGEBACK_EGRESS]: `Charge`, `Charges`, `ChargebackRow`, `ChargebackDataset` — the priced charge event, its one settle door, the tenant-partitioned billing rows, and their canonical content key.
- [05]-[TS_PROJECTION]: the dataset leaves as the Arrow lake landing; tenant rides the CloudEvents envelope.

## [02]-[COST_ALGEBRA]

- Owner: `CostVector` — the four-axis decomposed cost of one execution, and the monoid the ledger folds it under.
- Entry: `CostVector.Zero` is the identity; `operator +` is the fold; `Total` gates zero-priced charges from publication.
- Auto: every axis is real-valued because a rate is a real number and a priced second, token, byte, or remote-node second is its product — an integer cell would round every charge below its own rate to zero.
- Packages: Generator.Equals (`[Equatable]` + `[PrecisionEquality]` — the billing diff rail), BCL inbox
- Growth: a new cost axis is one `CostVector` field, one `[03]-[RATE_POLICY]` rate column, and the `Price` arm that reads it — every untouched arm breaks loudly.
- Law: this is the PRICED DECOMPOSITION of one execution, distinct by identity regime from the `Rasm.AppHost` `Agent/capability#DESCRIPTOR_AXIS` `MeterVector`, which is the grant/balance algebra over an integer `HashMap<CostUnit, long>` with `Add`/`Subtract`/`Shortfall`. The discriminant is readable from the value: a real-valued fixed-axis product answers WHAT ONE EXECUTION COST, an integer unit map answers WHAT REMAINS OF A GRANT. Both reach one compile leg through this package's legal AppHost reference, so the two names stay distinct and neither renders as the other on any wire.
- Boundary: `[PrecisionEquality]` bands every member at the accumulation noise floor so two chargeback folds compare equal under float re-association and `Inequalities` names the axis that moved. Hashing DIVERGES from equality by design — a precision-banded member leaves `GetHashCode` entirely, so a `CostVector` is NEVER a dictionary key; it rides `HashMap` VALUES alone.

```csharp
[Equatable]
public readonly partial record struct CostVector(
    [property: PrecisionEquality(1e-9)] double ElapsedUnits,
    [property: PrecisionEquality(1e-9)] double TokenUnits,
    [property: PrecisionEquality(1e-9)] double ByteUnits,
    [property: PrecisionEquality(1e-9)] double RemoteUnits) {
    public static readonly CostVector Zero = new(0d, 0d, 0d, 0d);

    public double Total => ElapsedUnits + TokenUnits + ByteUnits + RemoteUnits;

    public static CostVector operator +(CostVector left, CostVector right) =>
        new(left.ElapsedUnits + right.ElapsedUnits,
            left.TokenUnits + right.TokenUnits,
            left.ByteUnits + right.ByteUnits,
            left.RemoteUnits + right.RemoteUnits);
}
```

## [03]-[RATE_POLICY]

- Owner: `CostPolicy` — the composition-admitted rate table and the one pricing read over a settled execution.
- Entry: `CostPolicy.Validate` / `CostPolicy.Create` — the generated `[ComplexValueObject]` factory pair over the ordered substrate rate roster and the three scalar rates; `SecondRate(Substrate)` is the total per-route read; `Price(Substrate route, Option<Duration> elapsed, ComputeOutput output)` prices one dispatch — the substrate second rate over the measured elapsed beside the units only the lane result carries; `Staged(long bytes)` prices one process-scoped staging grant.
- Auto: admission proves TOTAL substrate coverage exactly once per row and finite non-negative rates, in one accumulating pass, so a partial or duplicated roster names the missing and repeated rows rather than a bare `"coverage"` token; `Price` is a total `Switch` over `ComputeOutput`, so a landed output case decides its priced units or answers zero explicitly, and an unmeasured elapsed prices no elapsed units rather than a fabricated zero-second charge.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — the `Projection/fault#ADMISSION_SLOTS` accumulating slot algebra), BCL inbox
- Growth: a new rate posture is one admitted policy value at composition; a new rate column is one member with its own slot; zero new surface.
- Law: coverage is proved against `Substrate.Items` — the roster IS the authority, so the proof names the rows the table MISSES and the rows it repeats. The count-plus-frozen-set-count comparison it replaces answered "coverage" and named neither, so a composition root wiring a five-row table with one duplicate fixed one row per boot cycle.
- Law: the read is TOTAL by admission. `SecondRate` scans the admitted roster and cannot miss, because admission already proved every `Substrate` row present exactly once; the roster is five rows, so the scan is the whole index a frozen dictionary would build.
- Boundary: no rate literal lives in the package — the generated factory is the only mint and the composition root supplies the rows. Pricing reads the RESULT: `Price` opens the `ComputeOutput` case for the units its lane measured (`GenerationOutcome.Tally.Tokens`, `RemoteReply.Elapsed`) and never a side stream, so a lane cannot bill a unit its own value does not state.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using static Rasm.Element.Projection.AdmissionSlots;

[ComplexValueObject]
public sealed partial class CostPolicy {
    public Seq<(Substrate Row, double SecondRate)> Rates { get; }
    public double TokenRate { get; }
    public double StagedByteRate { get; }
    public double RemoteNodeSecondRate { get; }

    [IgnoreMember]
    public double SecondRate(Substrate route) =>
        Rates.Find(row => row.Row == route).Map(static row => row.SecondRate).IfNone(0d);

    [IgnoreMember]
    public CostVector Price(Substrate route, Option<Duration> elapsed, ComputeOutput output) =>
        elapsed.Map(measured => CostVector.Zero with { ElapsedUnits = measured.TotalSeconds * SecondRate(route) }).IfNone(CostVector.Zero)
        + output.Switch(
            state: this,
            tensor: static (_, _) => CostVector.Zero,
            model: static (_, _) => CostVector.Zero,
            remote: static (rates, remote) => CostVector.Zero with { RemoteUnits = remote.Reply.Elapsed.TotalSeconds * rates.RemoteNodeSecondRate },
            converted: static (_, _) => CostVector.Zero,
            evaluated: static (_, _) => CostVector.Zero,
            sensor: static (_, _) => CostVector.Zero,
            pipeline: static (rates, line) => line.Stages.Fold(CostVector.Zero, (acc, stage) => acc + rates.Price(Substrate.CpuTensor, None, stage)),
            generated: static (rates, run) => CostVector.Zero with { TokenUnits = run.Outcome.Tally.Tokens * rates.TokenRate });

    [IgnoreMember]
    public CostVector Staged(long bytes) =>
        bytes > 0L ? CostVector.Zero with { ByteUnits = bytes * StagedByteRate } : CostVector.Zero;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<(Substrate Row, double SecondRate)> rates,
        ref double tokenRate, ref double stagedByteRate, ref double remoteNodeSecondRate) =>
        validationError = (
            Missing(rates), Repeated(rates),
            Gate(rates.ForAll(static rate => double.IsFinite(rate.SecondRate) && rate.SecondRate >= 0d), "second-rate", rates.Count, Rejected),
            Gate(double.IsFinite(tokenRate) && tokenRate >= 0d, "token-rate", tokenRate, Rejected),
            Gate(double.IsFinite(stagedByteRate) && stagedByteRate >= 0d, "byte-rate", stagedByteRate, Rejected),
            Gate(double.IsFinite(remoteNodeSecondRate) && remoteNodeSecondRate >= 0d, "remote-rate", remoteNodeSecondRate, Rejected))
            .Apply(static (_, _, _, _, _, _) => unit).As()
            .Match(
                Succ: static _ => null,
                Fail: static errors => new ValidationError(string.Join(" | ", new object?[] { $"<cost-policy-rejected:{Error.Many(errors)}>" })));

    static Validation<Error, Unit> Missing(Seq<(Substrate Row, double SecondRate)> rates) =>
        toSeq(Substrate.Items).Filter(row => !rates.Exists(rate => rate.Row == row)) is { IsEmpty: false } absent
            ? Fail<Error, Unit>(Rejected("rate-missing", string.Join(',', absent.Map(static row => row.Key))))
            : Success<Error, Unit>(unit);

    static Validation<Error, Unit> Repeated(Seq<(Substrate Row, double SecondRate)> rates) =>
        rates.Map(static rate => rate.Row).Collisions(static row => row) is { IsEmpty: false } collided
            ? Fail<Error, Unit>(Rejected("rate-repeated", string.Join(',', collided.Map(static row => row.Key))))
            : Success<Error, Unit>(unit);

    static Error Rejected<T>(string slot, T value) =>
        new ComputeFault.PayloadOverBounds($"<{slot}:{value}>");
}
```

## [04]-[CHARGEBACK_EGRESS]

- Owner: `Charge` the priced charge of one execution or one staging grant — the `data` of the `rasm.compute.charge.priced` CloudEvent; `Charges` the ONE settle door binding the rate table, the event contract, and the kernel clock at composition; `ChargebackRow` the per-`(tenant, route)` billing row; `ChargebackDataset` the windowed, ordered, content-keyed billing egress over the landed charge journal.
- Entry: `Charges.Settle(Charge charge, Op key)` — writes `rasm.compute.cost.units` under the tenant and substrate tags and publishes the charge through `RasmEventEnvelope.Publish`, returning the minted envelope; a zero-priced charge writes nothing and publishes nothing. `ChargebackDataset.Of(Instant windowStart, Instant windowEnd, Seq<(TenantContext Tenant, Charge Charge)> journal)` folds the landed journal into ordered per-`(tenant, route)` rows and mints the content key.
- Auto: the publish door stamps tenant as `rasm.tenant` baggage, correlation as `traceparent`, and order as `time`/`sequence` off the composition `Hlc`, so a charge carries no correlation, tenant, or stamp column of its own; grouping composes the BCL `AggregateBy` keyed fold; ordering is ordinal by tenant slug then route key, so the key is order-stable; the content key folds window, tenant slug, route, vector lanes, and charge counts through the kernel canonical writer so a re-derived dataset over identical evidence re-keys identically on every runtime.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, System.Text.Json, Rasm (project — `Domain/identity#CONTENT_KEY` `ContentHash`/`CanonicalWriter`, `Domain/event#ENVELOPE_MINT` `RasmEventEnvelope`/`RasmEventMint`/`EventExtensionContract`/`EventType`/`EventSource`/`EventId`, the kernel tenancy primitive and `Hlc`), Rasm.Contracts (project — generated `event.Extensions`), BCL inbox
- Growth: a new billing column is one `ChargebackRow` member folded into the canonical preimage; a new charge site is one `Settle` call at the producing owner; zero new surface.
- Law: the CloudEvent is the billing truth — the in-process journal plane records the published events, the `EVIDENCE_RESIDENCE` custodian lands them, and the dataset folds the landed rows the envelope owner already partitioned by `rasm.tenant`.
- Law: the content key streams the kernel `CanonicalWriter` — length-framed text, little-endian fixed-width scalars, and EXACT double bits — so the preimage is byte-identical across osx-arm64, linux-x64, and win-x64. Billing figures take `Bits`, never the tolerance-quantized `Double`: a re-derived charge must key bit-exact, and a banded key would collide two invoices a cent apart.
- Boundary: `Route` is ABSENT on a process-scoped charge (a staging grant) and never a fabricated slug, so the ordering read names the absence with one spelling both the sort key and the content preimage share.
- Boundary: the content-keyed dataset projects columnar for the billing lake through the ONE `Runtime/codecs#ARROW_BATCH` landing — `ArrowBatch.Landing(new LakeDataset.Chargeback(dataset), tenant, allocator)` — which lands the rows LONG on the metering kind, four rows per `(tenant, route)` sorted on `kind`, never a second columnar encoder and never a wide four-column layout this owner would have to keep in step with the lane roster.

```csharp
public sealed record Charge(Option<Substrate> Route, CostVector Vector);

public sealed class Charges(CostPolicy rates, EventExtensionContract<Extensions> contract, Hlc clock, ComputeWireContext wire) {
    public static readonly EventType Priced = EventType.Of("compute", "charge", "priced");
    public static readonly EventSource Source = EventSource.Of("compute", "ledger");

    public CostPolicy Rates => rates;

    public Fin<Option<CloudEvent>> Settle(Charge charge, Op key) =>
        charge.Vector.Total > 0d
            ? EventId.Of(key.ToString(), key)
                .Bind(id => RasmEventEnvelope.Publish(
                    new RasmEventMint<Extensions>(
                        Type: Priced, Source: Source, Id: id, Subject: None, Time: clock.Wall,
                        DataSchema: None, DataContentType: Some(MediaTypeNames.Application.Json),
                        Data: JsonSerializer.SerializeToUtf8Bytes(charge, wire.Charge),
                        Extensions: new Extensions()),
                    contract, clock, key))
                .Map(Some)
            : Fin.Succ(Option<CloudEvent>.None);
}

public sealed record ChargebackRow(TenantContext Tenant, Option<Substrate> Route, CostVector Vector, long Charges) {
    public const string ProcessRoute = "process";

    public string RouteKey => Route.Map(static route => route.Key).IfNone(ProcessRoute);
}

public sealed record ChargebackDataset(Instant WindowStart, Instant WindowEnd, Seq<ChargebackRow> Rows, UInt128 ContentKey) {
    public static ChargebackDataset Of(Instant windowStart, Instant windowEnd, Seq<(TenantContext Tenant, Charge Charge)> journal) {
        Seq<ChargebackRow> rows = toSeq(journal
            .AggregateBy(
                static row => (row.Tenant, row.Charge.Route),
                (Vector: CostVector.Zero, Charges: 0L),
                static (held, row) => (held.Vector + row.Charge.Vector, held.Charges + 1L))
            .Select(static slot => new ChargebackRow(slot.Key.Tenant, slot.Key.Route, slot.Value.Vector, slot.Value.Charges))
            .OrderBy(static row => row.Tenant.Slug, StringComparer.Ordinal)
            .ThenBy(static row => row.RouteKey, StringComparer.Ordinal));
        return new ChargebackDataset(windowStart, windowEnd, rows, Keyed(windowStart, windowEnd, rows));
    }

    private static UInt128 Keyed(Instant start, Instant end, Seq<ChargebackRow> rows) =>
        ContentHash.Of((Start: start, End: end, Rows: rows), static (state, writer) =>
            ignore(state.Rows.Fold(
                writer.I64(state.Start.ToUnixTimeTicks()).I64(state.End.ToUnixTimeTicks()).Ordinal(state.Rows.Count),
                static (canonical, row) => canonical
                    .String(row.Tenant.Slug)
                    .String(row.RouteKey)
                    .Bits(row.Vector.ElapsedUnits)
                    .Bits(row.Vector.TokenUnits)
                    .Bits(row.Vector.ByteUnits)
                    .Bits(row.Vector.RemoteUnits)
                    .I64(row.Charges))));
}
```

## [05]-[TS_PROJECTION]

- Law: the chargeback dataset leaves this package ONLY through the `Runtime/codecs#ARROW_BATCH` lake landing — `ArrowBatch.Landing(new LakeDataset.Chargeback(dataset), tenant, allocator)` — so no hand TS interface for the priced decomposition or the dataset lives here; tenant rides the `rasm.tenant` baggage member of the CloudEvents envelope every charge publishes under, never a Compute-minted JSON row. The AppHost `Agent/capability#DESCRIPTOR_AXIS` `MeterVector` crosses `Runtime/wire#PROTO_VOCABULARY` as generated `Meter` rows under its own `CostUnit` keys — two shapes, two names, two readers, and neither renders as the other. NAMED LOSS: no JSON projection of `CostVector` exists. Witness: the retired `CostVectorWire`/`ChargebackRowWire` mirror named no reader, and the billing reader consumes the landed Arrow generation.
