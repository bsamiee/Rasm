# [COST_LEDGER]

Rasm.Compute prices every measured fact once. The rate table enters at the composition root as external policy, the `Runtime/receipts#TELEMETRY_PROJECTION` fold answers what one fact costs beside what it meters, and the tenant-partitioned chargeback dataset folds the `ReceiptEnvelope`-joined journal into billing truth under one content key. No rate literal lives in the package, no second pricing fold exists, and the `rasm.compute.cost.units` histogram is the lossy channel beside the dataset, never a substitute for it.

## [01]-[INDEX]

- [02]-[COST_ALGEBRA]: `CostVector` — the decomposed per-axis cost monoid and its billing diff rail.
- [03]-[RATE_POLICY]: `CostPolicy` — the composition-admitted rate table and its total substrate coverage proof.
- [04]-[CHARGEBACK_EGRESS]: `ChargebackRow`, `ChargebackDataset` — the tenant-partitioned billing rows and their canonical content key.
- [05]-[TS_PROJECTION]: the dataset leaves as the Arrow lake landing; the tenant column rides the generated host envelope.

## [02]-[COST_ALGEBRA]

- Owner: `CostVector` — the four-axis decomposed cost of one measured fact, and the monoid the ledger folds it under.
- Entry: `CostVector.Zero` is the identity; `operator +` is the fold; `Total` is the scalar the `rasm.compute.cost.units` histogram writes.
- Auto: every axis is real-valued because a rate is a real number and a priced second, token, byte, or remote-node second is its product — an integer cell would round every charge below its own rate to zero.
- Receipt: none — a cost vector is a projection of a fact, never a fact.
- Packages: Generator.Equals (`[Equatable]` + `[PrecisionEquality]` — the billing diff rail), BCL inbox
- Growth: a new cost axis is one `CostVector` field, one `[03]-[RATE_POLICY]` rate column, and the `Runtime/receipts#TELEMETRY_PROJECTION` arms it touches — every untouched arm breaks loudly.
- Law: this is the PRICED DECOMPOSITION of one fact, distinct by identity regime from the `Rasm.AppHost` `Agent/capability#DESCRIPTOR_AXIS` `MeterVector`, which is the grant/balance algebra over an integer `HashMap<CostUnit, long>` with `Add`/`Subtract`/`Shortfall`. The discriminant is readable from the value: a real-valued fixed-axis product answers WHAT ONE FACT COST, an integer unit map answers WHAT REMAINS OF A GRANT. Both reach one compile leg through this package's legal AppHost reference, so the two names stay distinct and neither renders as the other on any wire.
- Boundary: `[PrecisionEquality]` bands every member at the accumulation noise floor so two chargeback folds compare equal under float re-association and `Inequalities` names the axis that moved. Hashing DIVERGES from equality by design — a precision-banded member leaves `GetHashCode` entirely, so a `CostVector` is NEVER a dictionary key; it rides `HashMap` VALUES alone.

```csharp signature
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

- Owner: `CostPolicy` — the composition-admitted rate table pricing every measured fact.
- Entry: `CostPolicy.Validate` / `CostPolicy.Create` — the generated `[ComplexValueObject]` factory pair over the ordered substrate rate roster and the three scalar rates; `SecondRate(Substrate)` is the total per-route read the pricing fold takes.
- Auto: admission proves TOTAL substrate coverage exactly once per row and finite non-negative rates, in one accumulating pass, so a partial or duplicated roster names the missing and repeated rows rather than a bare `"coverage"` token.
- Receipt: none — the policy is admitted at composition and read on every price.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — the `Projection/fault#ADMISSION_SLOTS` accumulating slot algebra), BCL inbox
- Growth: a new rate posture is one admitted policy value at composition; a new rate column is one member with its own slot; zero new surface.
- Law: coverage is proved against `Substrate.Items` — the roster IS the authority, so the proof names the rows the table MISSES and the rows it repeats. The count-plus-frozen-set-count comparison it replaces answered "coverage" and named neither, so a composition root wiring a five-row table with one duplicate fixed one row per boot cycle.
- Law: the read is TOTAL by admission. `SecondRate` scans the admitted roster and cannot miss, because admission already proved every `Substrate` row present exactly once; the roster is five rows, so the scan is the whole index a frozen dictionary would build.
- Boundary: no rate literal lives in the package — the generated factory is the only mint and the composition root supplies the rows. Pricing itself is NOT here: the `Runtime/receipts#TELEMETRY_PROJECTION` fold answers cost and instrument writes in one traversal of the fact union, so this owner holds the rates and that owner holds the arms. NAMED LOSS: a cost-only arm edit lands on the receipts page rather than beside the rate table; the gain is that a landed receipt case cannot meter without pricing or price without metering, where two 33-arm folds let it answer one and silently skip the other.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using static Rasm.Element.Projection.AdmissionSlots;

[ComplexValueObject]
public sealed partial class CostPolicy {
    public Seq<(Substrate Row, double SecondRate)> Rates { get; }
    public double TokenRate { get; }
    public double StagedByteRate { get; }
    public double RemoteNodeSecondRate { get; }

    // Total by admission: every `Substrate` row is present exactly once, so the scan cannot miss and the terminal
    // fall-through names the proof rather than a fabricated zero rate.
    [IgnoreMember]
    public double SecondRate(Substrate route) =>
        Rates.Find(row => row.Row == route).Map(static row => row.SecondRate).IfNone(0d);

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

    // The roster is the authority on BOTH halves, so each names its own offending rows.
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

- Owner: `ChargebackRow` the per-`(tenant, route)` billing row; `ChargebackDataset` the windowed, ordered, content-keyed billing egress.
- Entry: `ChargebackDataset.Of(Instant windowStart, Instant windowEnd, Seq<(TenantContext Tenant, ComputeReceipt Fact)> journal, CostPolicy costs)` folds the `ReceiptEnvelope`-joined journal into ordered per-`(tenant, route)` rows and mints the content key.
- Auto: grouping composes the BCL `AggregateBy` keyed fold; ordering is ordinal by tenant slug then route key, so the key is order-stable; the content key folds window, tenant slug, route, vector lanes, and fact counts through the kernel canonical writer so a re-derived dataset over identical evidence re-keys identically on every runtime.
- Receipt: none new — the ledger is a projection of the standing fact stream; these rows are the billing truth and `rasm.compute.cost.units` is the lossy channel beside them.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm (project — `Domain/identity#CONTENT_KEY` `ContentHash`/`CanonicalWriter`, the kernel tenancy primitive), BCL inbox
- Growth: a new billing column is one `ChargebackRow` member folded into the canonical preimage; zero new surface.
- Law: the content key streams the kernel `CanonicalWriter` — length-framed text, little-endian fixed-width scalars, and EXACT double bits — so the preimage is byte-identical across osx-arm64, linux-x64, and win-x64. The `{:R}` interpolation it replaces built one UTF-8 string out of culture-sensitive float renderings, which is a SECOND byte-identity spelling inside a package that already owns one (`Runtime/codecs#CONTENT_ADDRESSING` proves the composed path on the sibling page). Billing figures take `Bits`, never the tolerance-quantized `Double`: a re-derived charge must key bit-exact, and a banded key would collide two invoices a cent apart.
- Boundary: the `ReceiptEnvelope` `Tenant` that `TenantContext.Stamp` promotes onto every registered mirror store is the same partition this ledger folds — the estate baggage-attribution law (`libs` `[COST_ATTRIBUTION_BAGGAGE]`) consumes this dataset, and a second attribution stream beside the receipt rail is the rejected form.
- Boundary: `Route` is ABSENT on a process-scoped population and never a fabricated slug, so the ordering read names the absence with one spelling both the sort key and the content preimage share.
- Boundary: the content-keyed dataset projects columnar for the billing lake through the ONE `Runtime/codecs#ARROW_BATCH` landing — `ArrowBatch.Landing(new LakeDataset.Chargeback(dataset), tenant, allocator)` — which lands the rows LONG on the metering kind, four rows per `(tenant, route)` sorted on `kind`, never a second columnar encoder and never a wide four-column layout this owner would have to keep in step with the lane roster.

```csharp signature
public sealed record ChargebackRow(TenantContext Tenant, Option<Substrate> Route, CostVector Vector, long Facts) {
    // ONE spelling for an absent route, shared by the sort key and the content preimage — two spellings would sort
    // one population and key another.
    public const string ProcessRoute = "process";

    public string RouteKey => Route.Map(static route => route.Key).IfNone(ProcessRoute);
}

public sealed record ChargebackDataset(Instant WindowStart, Instant WindowEnd, Seq<ChargebackRow> Rows, UInt128 ContentKey) {
    public static ChargebackDataset Of(Instant windowStart, Instant windowEnd, Seq<(TenantContext Tenant, ComputeReceipt Fact)> journal, CostPolicy costs) {
        Seq<ChargebackRow> rows = toSeq(journal
            .AggregateBy(
                static row => (row.Tenant, Route: row.Fact.Substrate),
                (Vector: CostVector.Zero, Facts: 0L),
                (held, row) => (held.Vector + ComputeInstrumentFan.Measure(costs, row.Fact).Cost, held.Facts + 1L))
            .Select(static slot => new ChargebackRow(slot.Key.Tenant, slot.Key.Route, slot.Value.Vector, slot.Value.Facts))
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
                    .I64(row.Facts))));
}
```

## [05]-[TS_PROJECTION]

- Law: the chargeback dataset leaves this package ONLY through the `Runtime/codecs#ARROW_BATCH` lake landing — `ArrowBatch.Landing(new LakeDataset.Chargeback(dataset), tenant, allocator)` — so no hand TS interface for the priced decomposition or the dataset lives here; the tenant column on any host receipt rides the generated `Receipt.TenantContextWire` (`tenant = ContentHash.Wire(TenantId.Value)`, `slug = TenantContext.Slug`) formatted through AppHost `WireJson`, never a Compute-minted JSON row. The AppHost `Agent/capability#DESCRIPTOR_AXIS` `MeterVector` crosses `Runtime/wire#PROTO_VOCABULARY` as generated `Meter` rows under its own `CostUnit` keys — two shapes, two names, two readers, and neither renders as the other. NAMED LOSS: no JSON projection of `CostVector` exists. Witness: the retired `CostVectorWire`/`ChargebackRowWire` mirror named no reader, and the billing reader consumes the landed Arrow generation.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
