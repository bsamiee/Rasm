# [RASM_FABRICATION_ESTIMATION]

`Estimate.Run` converts correlated fabrication evidence into a unit or lot receipt selected by `EstimateRequest`. Lot evaluation transforms parallel money and carbon ledgers by quantity, batching, scrap, commercial loading, validity, capacity, and confidence while preserving every source dimension. Pricing reads authoritative sibling receipts; it never reconstructs clocks, yield, wear, availability, or welding work.

`EstimateEvidence` is one closed union that prices its own activity rows, so a new evidence source is one case and no projection roster changes. `AllocationKind` and `CommercialLoad` carry their multipliers, stages, loading eligibility, ranks, and compounding bases as data, so lot allocation and commercial loading are one ordered fold. `EstimateRow` is the sole signed egress family and every row retains the quantity and rate its amount derives from.

## [01]-[INDEX]

- [01]-[ESTIMATION]: owns `EstimateEvidence`, `EvidenceKind`, `EstimateBasis`, activity rows, tariff/factor policy, `CostReceipt`, `QuotePolicy`, `QuoteReceipt`, `EstimateRequest`, `EstimateReceipt`, and `Estimate.Run`.

## [02]-[ESTIMATION]

- Owner: `EstimateBasis` binds one subject, currency, evaluation instant, evidence corpus, tariff map, carbon-factor map, uncertainty, and remnant policy. `QuotePolicy` binds lot shape, per-kind commercial loading, confidence, and validity, and derives its own batch count. `CapacityQuote` bounds the units admitted by its promise interval and derives that interval, its queue, and its load factor from `LotReceipt` and the bottleneck `AvailabilityPlan` whenever the package planned the lot. `CostReceipt` owns unit money, carbon, clock, per-kind reconciliation, and evidence identity. `QuoteReceipt` owns lot allocation, risk-loaded totals, validity, and promise interval.
- Cases: `EstimateEvidence` covers simulation, fleet match, wear, stock, additive build, welding, operation time, capacity, quality, outside service, logistics, and consumable mass, and each case owns the `CostActivity` and `ImpactActivity` rows it can prove. `EvidenceKind` names each case and declares whether it admits repetition. `CostKind` and `CarbonKind` generate every activity and allocation row from policy data.
- Entry: `Estimate.Run(EstimateRequest request)` admits unit and lot modalities by input case, consumes generated `EstimateBasis` and `QuotePolicy` admission, verifies result-to-subject identity, then evaluates one total `FabricationResult.Switch` into an `EstimateDemand` whose required evidence kinds gate the fold.
- Auto: `SimulationReceipt` supplies authoritative duration and energy, including specialized slices retained in `SimulationReceipt.Specialized`. `MachineMatch.HourlyRate` supplies assessed machine rate, and `MachineInstance.Availability` supplies routing truth. `WearReceipt`, `BuildReceipt`, `NestYield`, `WeldSchedule.Total`, `CapacityQuote`, and explicit operation receipts supply their owned facts.
- Receipt: every `EstimateRow.Money` retains `CostKind`, `CostStage`, quantity, and rate, so `Amount` is always re-derivable and batch, lot, commercial loading, and risk projections never relabel labor as setup. `QuoteReceipt.ExpectedTotal`, `RiskTotal`, and `QuotedTotal` remain disjoint projections over the same ledger, and `ByKind` reconciles per source dimension on both sides of every lot transformation. `CostReceipt.MachineTime` remains the traveler actual-versus-estimated reconciliation seam. `FabricationFact.Estimate.Of` projects either `EstimateReceipt` case onto `rasm.fabrication.estimate.money`, `rasm.fabrication.estimate.carbon`, and `rasm.fabrication.estimate.clock` through `Process/telemetry#FACT_PROJECTION` as kind `estimate` — money and carbon stay parallel dimensions on parallel instruments, never one converted series.
- Packages: `Verify/simulate` (`SimulationReceipt`); `Additive/production` (`BuildReceipt`, `OrientationVerdict.Admitted`, `OrientedPart.RequiredFeedstock`, `FeedstockBlend.VirginFraction`); `Kinematics/fleet` (`MachineMatch.HourlyRate`, `MachineInstance.Availability`, `AvailabilityPlan.LoadFactor`); `Process/derivation` (`LotReceipt.Available`, `LotReceipt.Completion`, `LotReceipt.Queue`); `Tooling/wear` (`WearReceipt`, `WearState`, `ConsumableRow`, `ConsumableKind`); `Nesting/stock` (`NestYield`); `Joining/sequence` (`WeldSchedule.Total`); `Process/owner` (`FabricationResult`, `ContentKey`); `NodaTime` (`Instant`, `Duration`, `Interval`); `MathNet.Numerics.Distributions` (`Normal.InvCDF`); `Rasm.Element` (`Currency`); Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: new evidence is one `EstimateEvidence` case with its `EvidenceKind` row and its own activity arm; a priceable resource is one `CostKind` row with one tariff; an emission source is one `CarbonKind` row with one factor; an allocation regime is one `AllocationKind` row; a commercial transformation is one `CommercialLoad` row carrying its rank and compounding base.
- Boundary: pricing consumes evidence and never invents missing clocks or rates. Every evidence case correlates to `EstimateBasis.Subject`. Credits remain signed rows. Carbon never converts to currency. Quote validity and capacity use one evaluation instant. Machine and depreciation rows belong to the clock spine at the demand locus, so an `OperationTime` at that same locus contributes labor and setup only.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using MathNet.Numerics.Distributions;
using NodaTime;
using Rasm.Element;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CostStage {
    public static readonly CostStage Unit = new("unit");
    public static readonly CostStage Batch = new("batch");
    public static readonly CostStage Lot = new("lot");
    public static readonly CostStage Scrap = new("scrap");
    public static readonly CostStage Contingency = new("contingency");
    public static readonly CostStage Margin = new("margin");
    public static readonly CostStage Tax = new("tax");
    public static readonly CostStage Risk = new("risk");
}

[SmartEnum<string>]
public sealed partial class AllocationKind {
    public static readonly AllocationKind Variable = new("variable", CostStage.Unit, true, static (quantity, _) => quantity);
    public static readonly AllocationKind Batch = new("batch", CostStage.Batch, true, static (_, batches) => batches);
    public static readonly AllocationKind Lot = new("lot", CostStage.Lot, true, static (_, _) => 1);
    public static readonly AllocationKind Credit = new("credit", CostStage.Unit, false, static (quantity, _) => quantity);

    public CostStage Stage { get; }
    public bool CommerciallyLoadable { get; }

    [UseDelegateFromConstructor]
    public partial int Multiplier(int quantity, int batches);
}

[SmartEnum<string>]
public sealed partial class RateBasis {
    public static readonly RateBasis Hour = new("hour");
    public static readonly RateBasis SquareMeter = new("square-meter");
    public static readonly RateBasis Kilogram = new("kilogram");
    public static readonly RateBasis KilowattHour = new("kilowatt-hour");
    public static readonly RateBasis Life = new("life");
    public static readonly RateBasis CubicCentimeter = new("cubic-centimeter");
    public static readonly RateBasis Unit = new("unit");
    public static readonly RateBasis Lot = new("lot");
    public static readonly RateBasis TonneKilometer = new("tonne-kilometer");
}

[SmartEnum<string>]
public sealed partial class CostKind {
    public static readonly CostKind Machine = new("machine", RateBasis.Hour, AllocationKind.Variable);
    public static readonly CostKind Labor = new("labor", RateBasis.Hour, AllocationKind.Variable);
    public static readonly CostKind Setup = new("setup", RateBasis.Hour, AllocationKind.Batch);
    public static readonly CostKind Material = new("material", RateBasis.SquareMeter, AllocationKind.Variable);
    public static readonly CostKind AdditiveMaterial = new("additive-material", RateBasis.Kilogram, AllocationKind.Variable);
    public static readonly CostKind Energy = new("energy", RateBasis.KilowattHour, AllocationKind.Variable);
    public static readonly CostKind Tooling = new("tooling", RateBasis.Life, AllocationKind.Variable);
    public static readonly CostKind Consumable = new("consumable", RateBasis.Life, AllocationKind.Variable);
    public static readonly CostKind Rework = new("rework", RateBasis.CubicCentimeter, AllocationKind.Variable);
    public static readonly CostKind Quality = new("quality", RateBasis.Unit, AllocationKind.Variable);
    public static readonly CostKind OutsideService = new("outside-service", RateBasis.Unit, AllocationKind.Variable);
    public static readonly CostKind Logistics = new("logistics", RateBasis.Lot, AllocationKind.Lot);
    public static readonly CostKind Depreciation = new("depreciation", RateBasis.Hour, AllocationKind.Variable);
    public static readonly CostKind Remnant = new("remnant", RateBasis.Kilogram, AllocationKind.Credit);

    public RateBasis Basis { get; }
    public AllocationKind Allocation { get; }
}

[SmartEnum<string>]
public sealed partial class CarbonKind {
    public static readonly CarbonKind Electricity = new("electricity", RateBasis.KilowattHour, AllocationKind.Variable);
    public static readonly CarbonKind Material = new("material", RateBasis.Kilogram, AllocationKind.Variable);
    public static readonly CarbonKind RecycledFeedstock = new("recycled-feedstock", RateBasis.Kilogram, AllocationKind.Variable);
    public static readonly CarbonKind Scrap = new("scrap", RateBasis.Kilogram, AllocationKind.Variable);
    public static readonly CarbonKind Recovery = new("recovery", RateBasis.Kilogram, AllocationKind.Credit);
    public static readonly CarbonKind Consumable = new("consumable", RateBasis.Kilogram, AllocationKind.Variable);
    public static readonly CarbonKind Logistics = new("logistics", RateBasis.TonneKilometer, AllocationKind.Lot);

    public RateBasis Basis { get; }
    public AllocationKind Allocation { get; }
}

// Rank fixes the compounding order and `Over` declares the stages each load prices, so tax rides the marked-up total
// while margin never prices tax; a new commercial transformation is one row and no fold changes.
[SmartEnum<string>]
public sealed partial class CommercialLoad {
    public static readonly CommercialLoad Scrap = new("scrap", CostStage.Scrap, rank: 0,
        Set(CostStage.Unit, CostStage.Batch, CostStage.Lot),
        static rate => rate / (1.0 - rate), static rate => rate is >= 0.0 and < 1.0);
    public static readonly CommercialLoad Contingency = new("contingency", CostStage.Contingency, rank: 1,
        Set(CostStage.Unit, CostStage.Batch, CostStage.Lot, CostStage.Scrap),
        static rate => rate, static rate => rate >= 0.0);
    public static readonly CommercialLoad Margin = new("margin", CostStage.Margin, rank: 2,
        Set(CostStage.Unit, CostStage.Batch, CostStage.Lot, CostStage.Scrap, CostStage.Contingency),
        static rate => rate, static rate => rate >= 0.0);
    public static readonly CommercialLoad Tax = new("tax", CostStage.Tax, rank: 3,
        Set(CostStage.Unit, CostStage.Batch, CostStage.Lot, CostStage.Scrap, CostStage.Contingency, CostStage.Margin),
        static rate => rate, static rate => rate >= 0.0);

    public CostStage Stage { get; }
    public int Rank { get; }
    public Set<CostStage> Over { get; }

    [UseDelegateFromConstructor]
    public partial double Factor(double rate);

    [UseDelegateFromConstructor]
    public partial bool Admits(double rate);
}

[SmartEnum<string>]
public sealed partial class EvidenceKind {
    public static readonly EvidenceKind Simulation = new("simulation", repeatable: false);
    public static readonly EvidenceKind Machine = new("machine", repeatable: false);
    public static readonly EvidenceKind Wear = new("wear", repeatable: false);
    public static readonly EvidenceKind Stock = new("stock", repeatable: false);
    public static readonly EvidenceKind Additive = new("additive", repeatable: false);
    public static readonly EvidenceKind Welding = new("welding", repeatable: false);
    public static readonly EvidenceKind Operation = new("operation", repeatable: true);
    public static readonly EvidenceKind Capacity = new("capacity", repeatable: false);
    public static readonly EvidenceKind Quality = new("quality", repeatable: false);
    public static readonly EvidenceKind OutsideService = new("outside-service", repeatable: false);
    public static readonly EvidenceKind Logistics = new("logistics", repeatable: false);
    public static readonly EvidenceKind ConsumableMass = new("consumable-mass", repeatable: false);

    public bool Repeatable { get; }
}

[ComplexValueObject]
public sealed partial class StockConsumption {
    public NestYield Yield { get; }
    public double ConsumedAreaMm2 { get; }
    public double ThicknessMm { get; }
    public double DensityKgM3 { get; }
    public double RemnantMassKg { get; }

    public double ConsumedMassKg => ConsumedAreaMm2 * ThicknessMm * DensityKgM3 / 1e9;
    public double WasteMassKg => Yield.WasteAreaMm2 * ThicknessMm * DensityKgM3 / 1e9;
    public double ScrapMassKg => WasteMassKg - RemnantMassKg;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref NestYield yield,
        ref double consumedAreaMm2, ref double thicknessMm, ref double densityKgM3, ref double remnantMassKg) {
        bool finite = new[] { consumedAreaMm2, thicknessMm, densityKgM3, remnantMassKg }.ForAll(double.IsFinite);
        double recoverableWasteKg = yield is null ? double.NaN : yield.WasteAreaMm2 * thicknessMm * densityKgM3 / 1e9;
        if (yield is null || !finite || consumedAreaMm2 < yield.TruePartAreaMm2 || consumedAreaMm2 > yield.StockAreaMm2
            || thicknessMm <= 0.0 || densityKgM3 <= 0.0 || remnantMassKg < 0.0 || remnantMassKg > recoverableWasteKg)
            validationError = new ValidationError(message: "stock consumption requires bounded part, stock, waste, and remnant mass with positive section and density");
    }
}

[ComplexValueObject]
public sealed partial class OperationTime {
    public string Locus { get; }
    public Duration Machine { get; }
    public Duration Labor { get; }
    public Duration Setup { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string locus,
        ref Duration machine, ref Duration labor, ref Duration setup) {
        locus = locus?.Trim() ?? string.Empty;
        if (locus.Length == 0 || machine < Duration.Zero || labor < Duration.Zero || setup < Duration.Zero)
            validationError = new ValidationError(message: "operation time requires one locus and nonnegative machine, labor, and setup clocks");
    }
}

[ComplexValueObject]
public sealed partial class CapacityQuote {
    public Interval Promise { get; }
    public Duration Queue { get; }
    public double LoadFactor { get; }
    public int Units { get; }

    // A planned lot never asserts its own promise: availability, calendar completion, and the queue the shift
    // calendar imposed are already derived facts, and the bottleneck machine's committed load is the fourth.
    // Scalar admission survives only for capacity the package did not plan, such as an outside service window.
    public static Fin<CapacityQuote> Of(LotReceipt lot, AvailabilityPlan bottleneck, int units) =>
        Validate(new Interval(lot.Available, lot.Completion), lot.Queue, bottleneck.LoadFactor, units,
            out CapacityQuote quote) is { } error
                ? Fin.Fail<CapacityQuote>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, error.Message).ToError())
                : Fin.Succ(quote);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Interval promise,
        ref Duration queue, ref double loadFactor, ref int units) {
        if (promise is null || !promise.HasStart || !promise.HasEnd || promise.Duration <= Duration.Zero
            || queue < Duration.Zero || !double.IsFinite(loadFactor)
            || loadFactor is < 0.0 or > 1.0 || units <= 0)
            validationError = new ValidationError(message: "capacity quote requires a promise interval, positive unit capacity, nonnegative queue, and bounded load factor");
    }
}

[ComplexValueObject]
public sealed partial class LogisticsActivity {
    public double TonneKilometers { get; }
    public int Lots { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double tonneKilometers, ref int lots) {
        if (!double.IsFinite(tonneKilometers) || tonneKilometers < 0.0 || lots <= 0)
            validationError = new ValidationError(message: "logistics activity requires nonnegative freight activity and positive lots");
    }
}

public readonly record struct CostActivity(CostKind Kind, string Locus, double Quantity);

public readonly record struct ImpactActivity(CarbonKind Kind, string Locus, double Quantity);

public readonly record struct ActivityRows(Seq<CostActivity> Cost, Seq<ImpactActivity> Impact) {
    public static ActivityRows Empty { get; } = new(Seq<CostActivity>(), Seq<ImpactActivity>());

    public ActivityRows Concat(ActivityRows other) => new(Cost.Concat(other.Cost), Impact.Concat(other.Impact));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimateEvidence(ContentKey Subject) {

    public sealed record Simulation(ContentKey Subject, SimulationReceipt Receipt) : EstimateEvidence(Subject);
    public sealed record Machine(ContentKey Subject, MachineMatch Receipt) : EstimateEvidence(Subject);
    public sealed record Wear(ContentKey Subject, WearReceipt Receipt) : EstimateEvidence(Subject);
    public sealed record Stock(ContentKey Subject, StockConsumption Receipt) : EstimateEvidence(Subject);
    public sealed record Additive(ContentKey Subject, BuildReceipt Receipt) : EstimateEvidence(Subject);
    public sealed record Welding(ContentKey Subject, WeldSchedule Receipt) : EstimateEvidence(Subject);
    public sealed record Operation(ContentKey Subject, OperationTime Receipt) : EstimateEvidence(Subject);
    public sealed record Capacity(ContentKey Subject, CapacityQuote Receipt) : EstimateEvidence(Subject);
    public sealed record Quality(ContentKey Subject, int Units) : EstimateEvidence(Subject);
    public sealed record OutsideService(ContentKey Subject, int Units) : EstimateEvidence(Subject);
    public sealed record Logistics(ContentKey Subject, LogisticsActivity Receipt) : EstimateEvidence(Subject);
    public sealed record ConsumableMass(ContentKey Subject, Map<ConsumableKind, double> Kilograms) : EstimateEvidence(Subject);

    public EvidenceKind Kind => Switch(
        simulation: static _ => EvidenceKind.Simulation,
        machine: static _ => EvidenceKind.Machine,
        wear: static _ => EvidenceKind.Wear,
        stock: static _ => EvidenceKind.Stock,
        additive: static _ => EvidenceKind.Additive,
        welding: static _ => EvidenceKind.Welding,
        operation: static _ => EvidenceKind.Operation,
        capacity: static _ => EvidenceKind.Capacity,
        quality: static _ => EvidenceKind.Quality,
        outsideService: static _ => EvidenceKind.OutsideService,
        logistics: static _ => EvidenceKind.Logistics,
        consumableMass: static _ => EvidenceKind.ConsumableMass);

    public bool Payload => Switch(
        simulation: static value => value.Receipt is not null,
        machine: static value => value.Receipt is not null && value.Receipt.Instance is not null,
        wear: static value => value.Receipt is not null,
        stock: static value => value.Receipt is not null,
        additive: static value => value.Receipt is not null
            && value.Receipt.Orientations.Exists(static verdict => verdict is OrientationVerdict.Admitted),
        welding: static value => value.Receipt is not null,
        operation: static value => value.Receipt is not null,
        capacity: static value => value.Receipt is not null,
        quality: static value => value.Units >= 0,
        outsideService: static value => value.Units >= 0,
        logistics: static value => value.Receipt is not null,
        consumableMass: static value => value.Kilograms.ForAll(static item => item.Key is not null
            && double.IsFinite(item.Value) && item.Value >= 0.0));

    // Every case prices what its own receipt proves. The clock spine owns machine, depreciation, and energy at the
    // demand locus, so an operation receipt sharing that locus contributes labor and setup only.
    public ActivityRows Rows(EstimateBasis basis, string clockLocus) => Switch(
        state: (Basis: basis, Clock: clockLocus),
        simulation: static (_, _) => ActivityRows.Empty,
        machine: static (_, _) => ActivityRows.Empty,
        capacity: static (_, _) => ActivityRows.Empty,
        wear: static (_, value) => new ActivityRows(
            value.Receipt.States.Choose(static state => state is WearState.Tool row && row.Limit > 0.0
                    ? Some(new CostActivity(CostKind.Tooling, $"tool:{row.Target}", Math.Clamp(row.Current / row.Limit, 0.0, 1.0)))
                    : None)
                .Concat(value.Receipt.Consumables.Filter(static row => row.Limit > 0.0)
                    .Map(static row => new CostActivity(CostKind.Consumable, $"consumable:{row.Kind.Key}",
                        Math.Clamp(row.Used / row.Limit, 0.0, 1.0)))),
            Seq<ImpactActivity>()),
        stock: static (context, value) => new ActivityRows(
            Seq(new CostActivity(CostKind.Material, "stock", value.Receipt.ConsumedAreaMm2 / 1e6),
                new CostActivity(CostKind.Remnant, "remnant", -value.Receipt.RemnantMassKg * context.Basis.RemnantCreditFactor)),
            Seq(new ImpactActivity(CarbonKind.Material, "stock", value.Receipt.ConsumedMassKg),
                new ImpactActivity(CarbonKind.Scrap, "stock", value.Receipt.ScrapMassKg),
                new ImpactActivity(CarbonKind.Recovery, "remnant", -value.Receipt.RemnantMassKg))),
        additive: static (_, value) => Feedstock(value.Receipt),
        welding: static (_, value) => new ActivityRows(
            Seq(new CostActivity(CostKind.Machine, "welding", value.Receipt.Total.TotalHours),
                new CostActivity(CostKind.Labor, "welding", value.Receipt.Total.TotalHours)),
            Seq<ImpactActivity>()),
        operation: static (context, value) => new ActivityRows(
            Seq(new CostActivity(CostKind.Labor, value.Receipt.Locus, value.Receipt.Labor.TotalHours),
                new CostActivity(CostKind.Setup, value.Receipt.Locus, value.Receipt.Setup.TotalHours))
                .Concat(value.Receipt.Locus == context.Clock
                    ? Seq<CostActivity>()
                    : Seq(new CostActivity(CostKind.Machine, value.Receipt.Locus, value.Receipt.Machine.TotalHours))),
            Seq<ImpactActivity>()),
        quality: static (_, value) => new ActivityRows(
            Seq(new CostActivity(CostKind.Quality, "quality", value.Units)), Seq<ImpactActivity>()),
        outsideService: static (_, value) => new ActivityRows(
            Seq(new CostActivity(CostKind.OutsideService, "outside-service", value.Units)), Seq<ImpactActivity>()),
        logistics: static (_, value) => new ActivityRows(
            Seq(new CostActivity(CostKind.Logistics, "logistics", value.Receipt.Lots)),
            Seq(new ImpactActivity(CarbonKind.Logistics, "logistics", value.Receipt.TonneKilometers))),
        consumableMass: static (_, value) => new ActivityRows(Seq<CostActivity>(), value.Kilograms
            .Map(static row => new ImpactActivity(CarbonKind.Consumable, $"consumable:{row.Key.Key}", row.Value)).ToSeq()));

    private static ActivityRows Feedstock(BuildReceipt receipt) {
        Seq<OrientedPart> parts = receipt.Orientations.Choose(static verdict => verdict is OrientationVerdict.Admitted admitted
            ? Some(admitted.Part) : None);
        double requiredKg = parts.Sum(static part => part.RequiredFeedstock.Kilograms);
        double virginKg = parts.Sum(static part => part.RequiredFeedstock.Kilograms
            * part.Part.Feedstock.VirginFraction.DecimalFractions);
        return new ActivityRows(
            Seq(new CostActivity(CostKind.AdditiveMaterial, "additive", requiredKg)),
            Seq(new ImpactActivity(CarbonKind.Material, "additive:virgin", virginKg),
                new ImpactActivity(CarbonKind.RecycledFeedstock, "additive:recycled", requiredKg - virginKg)));
    }
}

[ComplexValueObject]
public sealed partial class EstimateBasis {
    public ContentKey Subject { get; }
    public Currency Currency { get; }
    public Instant EvaluatedAt { get; }
    public Seq<EstimateEvidence> Evidence { get; }
    public Map<CostKind, decimal> Tariffs { get; }
    public Map<CarbonKind, double> CarbonFactors { get; }
    public Map<CostKind, double> CoefficientOfVariation { get; }
    public double RemnantCreditFactor { get; }

    public Option<T> Find<T>() where T : EstimateEvidence => Evidence.Find(static row => row is T).Map(static row => (T)row);

    public bool Carries(EvidenceKind kind) => Evidence.Exists(row => row.Kind == kind);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentKey subject,
        ref Currency currency,
        ref Instant evaluatedAt,
        ref Seq<EstimateEvidence> evidence,
        ref Map<CostKind, decimal> tariffs,
        ref Map<CarbonKind, double> carbonFactors,
        ref Map<CostKind, double> coefficientOfVariation,
        ref double remnantCreditFactor) {
        bool priceComplete = toSeq(CostKind.Items).ForAll(kind => tariffs.Find(kind).Exists(static rate => rate >= decimal.Zero));
        bool carbonComplete = toSeq(CarbonKind.Items).ForAll(kind => carbonFactors.Find(kind)
            .Exists(static factor => double.IsFinite(factor) && factor >= 0.0));
        bool uncertaintyComplete = toSeq(CostKind.Items).ForAll(kind => coefficientOfVariation.Find(kind)
            .Exists(static value => double.IsFinite(value) && value >= 0.0));
        bool correlated = evidence.ForAll(row => row is not null && row.Subject == subject);
        bool cardinality = evidence.ForAll(row => row is not null && (row.Kind.Repeatable
            || evidence.Count(candidate => candidate is not null && candidate.Kind == row.Kind) == 1));
        Seq<OperationTime> operations = evidence.Choose(static row => row is EstimateEvidence.Operation value ? Some(value.Receipt) : None);
        bool operationIdentity = operations.Map(static value => value.Locus).Distinct().Count == operations.Count;
        Option<EstimateEvidence.Machine> machine = evidence.Find(static row => row is EstimateEvidence.Machine)
            .Map(static row => (EstimateEvidence.Machine)row);
        Option<EstimateEvidence.Capacity> capacity = evidence.Find(static row => row is EstimateEvidence.Capacity)
            .Map(static row => (EstimateEvidence.Capacity)row);
        bool feasible = machine.ForAll(static value => value.Receipt is not null && value.Receipt.Checks.Feasible);
        bool temporal = capacity.ForAll(value => value.Receipt is not null
            && value.Receipt.Promise is { HasStart: true, HasEnd: true } promise
            && (promise.Contains(evaluatedAt) || promise.Start >= evaluatedAt))
            && (capacity.IsSome || machine.ForAll(value => value.Receipt is not null && value.Receipt.Instance is not null
                && value.Receipt.Instance.Availability.IsRoutable(evaluatedAt)));
        bool payloads = evidence.ForAll(row => row is not null && row.Payload);
        if (subject.Kind is null || currency is null || !priceComplete || !carbonComplete || !uncertaintyComplete
            || !correlated || !cardinality || !operationIdentity || !feasible || !temporal || !payloads
            || !double.IsFinite(remnantCreditFactor) || remnantCreditFactor is < 0.0 or > 1.0)
            validationError = new ValidationError(message: "estimate basis requires correlated evidence, complete rates and factors, bounded uncertainty, and one subject");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimateRow {
    private EstimateRow() { }

    public sealed record Money(CostKind Kind, CostStage Stage, string Locus, Currency Currency,
        double Quantity, decimal Rate) : EstimateRow {
        public decimal Amount => Rate * (decimal)Quantity;
    }

    public sealed record Carbon(CarbonKind Kind, string Locus, double Quantity, double Factor) : EstimateRow {
        public double KgCo2e => Quantity * Factor;
    }

    public AllocationKind Allocation => Switch(
        money: static value => value.Kind.Allocation,
        carbon: static value => value.Kind.Allocation);

    public EstimateRow Allocate(int quantity, int batches) => Switch(
        state: (Quantity: quantity, Batches: batches),
        money: static (lot, value) => (EstimateRow)(value with {
            Stage = value.Kind.Allocation.Stage,
            Quantity = value.Quantity * value.Kind.Allocation.Multiplier(lot.Quantity, lot.Batches),
        }),
        carbon: static (lot, value) => value with {
            Quantity = value.Quantity * value.Kind.Allocation.Multiplier(lot.Quantity, lot.Batches),
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimateClock {
    private EstimateClock() { }

    public sealed record Simulation(Duration Duration) : EstimateClock;
    public sealed record Declared(Duration Duration) : EstimateClock;

    public Duration Value => Switch(
        simulation: static value => value.Duration,
        declared: static value => value.Duration);
}

public sealed record CostReceipt(
    ContentKey Subject,
    Currency Currency,
    Instant EvaluatedAt,
    Seq<EstimateRow> Rows,
    EstimateClock Clock) {
    public Duration MachineTime => Clock.Value;
    public bool SimulationBacked => Clock is EstimateClock.Simulation;
    public Seq<EstimateRow.Money> Money => Rows.Choose(static row => row is EstimateRow.Money value ? Some(value) : None);
    public Seq<EstimateRow.Carbon> Carbon => Rows.Choose(static row => row is EstimateRow.Carbon value ? Some(value) : None);
    public decimal MoneyTotal => Money.Sum(static row => row.Amount);
    public double CarbonTotalKgCo2e => Carbon.Sum(static row => row.KgCo2e);
    public Map<CostKind, decimal> ByKind => Estimate.Reconcile(Money);
    public Map<CarbonKind, double> CarbonByKind => Estimate.Reconcile(Carbon);
}

[ComplexValueObject]
public sealed partial class QuotePolicy {
    public int Quantity { get; }
    public int BatchCapacity { get; }
    public Map<(CommercialLoad Load, CostKind Kind), double> Loading { get; }
    public double Confidence { get; }
    public Duration ValidFor { get; }

    public int Batches => (int)Math.Ceiling((double)Quantity / BatchCapacity);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int quantity,
        ref int batchCapacity,
        ref Map<(CommercialLoad Load, CostKind Kind), double> loading,
        ref double confidence,
        ref Duration validFor) {
        bool complete = toSeq(CommercialLoad.Items).ForAll(load => toSeq(CostKind.Items)
            .ForAll(kind => loading.Find((load, kind)).Exists(rate => double.IsFinite(rate) && load.Admits(rate))));
        if (quantity <= 0 || batchCapacity <= 0 || !complete || !double.IsFinite(confidence)
            || confidence is <= 0.5 or >= 1.0 || validFor <= Duration.Zero)
            validationError = new ValidationError(message: "quote policy requires positive quantity, capacity, validity, confidence, and one commercial loading per stage and cost kind");
    }
}

public sealed record QuoteReceipt(
    CostReceipt Unit,
    QuotePolicy Policy,
    Seq<EstimateRow.Money> Money,
    Seq<EstimateRow.Carbon> Carbon,
    Option<CapacityQuote> Capacity) {
    public int Batches => Policy.Batches;
    public decimal ExpectedTotal => Money.Filter(static row => row.Stage != CostStage.Risk).Sum(static row => row.Amount);
    public decimal RiskTotal => Money.Filter(static row => row.Stage == CostStage.Risk).Sum(static row => row.Amount);
    public decimal QuotedTotal => Money.Sum(static row => row.Amount);
    public double CarbonTotalKgCo2e => Carbon.Sum(static row => row.KgCo2e);
    public Map<CostKind, decimal> ByKind => Estimate.Reconcile(Money);
    public Map<CarbonKind, double> CarbonByKind => Estimate.Reconcile(Carbon);
    public Interval Validity => new(Unit.EvaluatedAt, Unit.EvaluatedAt + Policy.ValidFor);
    public Option<Interval> Promise => Capacity.Map(static value => value.Promise);
    public Duration Queue => Capacity.Map(static value => value.Queue).IfNone(Duration.Zero);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimateRequest {
    private EstimateRequest() { }

    public sealed record Unit(FabricationResult Result, EstimateBasis Basis) : EstimateRequest;
    public sealed record Lot(FabricationResult Result, EstimateBasis Basis, QuotePolicy Policy) : EstimateRequest;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EstimateReceipt {
    private EstimateReceipt() { }

    public sealed record Unit(CostReceipt Receipt) : EstimateReceipt;
    public sealed record Lot(QuoteReceipt Receipt) : EstimateReceipt;
}

// One demand per result case: the locus every clock-derived row carries, whether a simulation clock is mandatory, the
// declared fallback, the evidence kinds the result cannot be priced without, and the rows only the result can prove.
internal sealed record EstimateDemand(
    string Locus,
    bool ClockRequired,
    Option<Duration> Declared,
    Set<EvidenceKind> Required,
    Seq<CostActivity> Intrinsic);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Estimate {
    public static Fin<EstimateReceipt> Run(EstimateRequest request) =>
        Optional(request).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:request").ToError()).Bind(value => value.Switch(
            unit: static value => Priced(value.Result, value.Basis).Map(static receipt => (EstimateReceipt)new EstimateReceipt.Unit(receipt)),
            lot: static value => Quoted(value.Result, value.Basis, value.Policy).Map(static receipt => (EstimateReceipt)new EstimateReceipt.Lot(receipt))));

    private static Fin<CostReceipt> Priced(FabricationResult result, EstimateBasis basis) =>
        from admitted in Admit(result, basis)
        from demand in Demand(result, admitted)
        from clock in Clock(admitted, demand)
        let rows = Spine(admitted, clock, demand)
            .Concat(admitted.Evidence.Map(row => row.Rows(admitted, demand.Locus))
                .Fold(ActivityRows.Empty, static (all, next) => all.Concat(next)))
        let costs = rows.Cost.Map(activity => Price(activity, admitted)).ToSeq()
        let impacts = rows.Impact.Map(activity => Impact(activity, admitted)).ToSeq()
        select new CostReceipt(admitted.Subject, admitted.Currency, admitted.EvaluatedAt,
            costs.Map(static row => (EstimateRow)row).Concat(impacts.Map(static row => (EstimateRow)row)),
            clock.Backed
                ? (EstimateClock)new EstimateClock.Simulation(clock.Duration)
                : new EstimateClock.Declared(clock.Duration));

    private static Fin<QuoteReceipt> Quoted(FabricationResult result, EstimateBasis basis, QuotePolicy policy) =>
        from admittedPolicy in Optional(policy).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:quote-policy").ToError())
        from unit in Priced(result, basis)
        let allocated = unit.Money.Map(row => (EstimateRow.Money)row.Allocate(admittedPolicy.Quantity, admittedPolicy.Batches)).ToSeq()
        let expected = toSeq(CommercialLoad.Items).OrderBy(static load => load.Rank).ToSeq()
            .Fold(allocated, (rows, load) => rows.Concat(Scale(
                rows.Filter(row => row.Kind.Allocation.CommerciallyLoadable && load.Over.Contains(row.Stage)), load, admittedPolicy)))
        let money = expected.Concat(Risk(expected, basis.CoefficientOfVariation, admittedPolicy.Confidence))
        let carbon = unit.Carbon.Map(row => (EstimateRow.Carbon)row.Allocate(admittedPolicy.Quantity, admittedPolicy.Batches)).ToSeq()
        let capacity = basis.Find<EstimateEvidence.Capacity>().Map(static value => value.Receipt)
        from _ in capacity.ForAll(value => value.Units >= admittedPolicy.Quantity)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:lot-capacity").ToError())
        select new QuoteReceipt(unit, admittedPolicy, money, carbon, capacity);

    private static Fin<EstimateBasis> Admit(FabricationResult result, EstimateBasis basis) =>
        from admittedResult in Optional(result).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:result").ToError())
        from admittedBasis in Optional(basis).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:basis").ToError())
        from _ in ResultSubject(admittedResult, admittedBasis.Subject)
        select admittedBasis;

    private static Fin<Unit> ResultSubject(FabricationResult result, ContentKey subject) {
        Seq<ContentKey> keys = result.Switch(
            hiddenLineResult: static value => value.Subjects,
            motion: static value => value.Subjects,
            placement: static value => Seq(value.Key),
            additiveResult: static value => value.Artifacts,
            verificationResult: static value => Seq(value.Residual.Key).Concat(value.Snapshots.Map(static row => row.Key)),
            inspectionResult: static value => value.Subjects,
            postedProgram: static value => Seq(value.Key),
            travelerDocument: static value => Seq(value.Key),
            fabricationPlan: static value => Seq(value.Key).Concat(value.Artifacts),
            formedResult: static value => Seq(value.Key));
        return !keys.IsEmpty && keys.Contains(subject)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:result-subject").ToError());
    }

    private static Fin<EstimateDemand> Demand(FabricationResult result, EstimateBasis basis) =>
        result.Switch(
            state: basis,
            hiddenLineResult: static (_, _) => Unpriceable("hidden-line"),
            travelerDocument: static (_, _) => Unpriceable("traveler"),
            motion: static (_, value) => Fin.Succ(new EstimateDemand("motion", ClockRequired: false,
                Some(Duration.FromSeconds(value.Duration)), Set<EvidenceKind>(), Seq<CostActivity>())),
            postedProgram: static (_, _) => Fin.Succ(new EstimateDemand("posted-program", ClockRequired: true,
                None, Set(EvidenceKind.Simulation), Seq<CostActivity>())),
            placement: static (_, _) => Fin.Succ(new EstimateDemand("stock", ClockRequired: false,
                Some(Duration.Zero), Set(EvidenceKind.Stock), Seq<CostActivity>())),
            additiveResult: static (_, _) => Fin.Succ(new EstimateDemand("additive", ClockRequired: true,
                None, Set(EvidenceKind.Simulation, EvidenceKind.Additive), Seq<CostActivity>())),
            verificationResult: static (_, value) => Fin.Succ(new EstimateDemand("verification", ClockRequired: true,
                None, Set(EvidenceKind.Simulation), Seq(
                    new CostActivity(CostKind.Rework, "uncut", value.UncutVolume / 1e3),
                    new CostActivity(CostKind.Rework, "overcut", value.OvercutVolume / 1e3)))),
            inspectionResult: static (_, _) => Fin.Succ(new EstimateDemand("inspection", ClockRequired: false,
                Some(Duration.Zero), Set(EvidenceKind.Operation), Seq<CostActivity>())),
            formedResult: static (_, _) => Fin.Succ(new EstimateDemand("forming", ClockRequired: false,
                Some(Duration.Zero), Set(EvidenceKind.Operation), Seq<CostActivity>())),
            fabricationPlan: static (context, value) => value.Steps.ForAll(step => context.Evidence.Exists(row =>
                    row is EstimateEvidence.Operation operation
                    && operation.Receipt.Locus == $"step:{step.Order}:{step.Process.Key}"))
                ? Fin.Succ(new EstimateDemand("plan", ClockRequired: false, Some(Duration.Zero),
                    Set(EvidenceKind.Operation), Seq<CostActivity>()))
                : Fin.Fail<EstimateDemand>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "estimate:plan-operation-evidence").ToError()))
        .Bind(demand => demand.Required.ToSeq().TraverseM(kind => basis.Carries(kind)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"estimate:{demand.Locus}:{kind.Key}").ToError())).As()
            .Map(_ => demand));

    private static Fin<EstimateDemand> Unpriceable(string locus) =>
        Fin.Fail<EstimateDemand>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"estimate:{locus}").ToError());

    private static Fin<(Duration Duration, bool Backed)> Clock(EstimateBasis basis, EstimateDemand demand) =>
        basis.Find<EstimateEvidence.Simulation>()
            .Map(static value => (Duration: value.Receipt.Cycle, Backed: true))
            .OrElse(demand.ClockRequired
                ? Option<(Duration Duration, bool Backed)>.None
                : demand.Declared.Map(static value => (Duration: value, Backed: false)))
            .Filter(static value => value.Duration >= Duration.Zero)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"estimate:{demand.Locus}-clock").ToError());

    // Machine, depreciation, and energy are the clock's own rows; every other row belongs to the evidence case proving it.
    private static ActivityRows Spine(EstimateBasis basis, (Duration Duration, bool Backed) clock, EstimateDemand demand) {
        double hours = clock.Duration.TotalHours;
        double energy = basis.Find<EstimateEvidence.Simulation>().Map(static value => value.Receipt.EnergyKwh).IfNone(0.0);
        return new ActivityRows(
            Seq(new CostActivity(CostKind.Machine, demand.Locus, hours),
                new CostActivity(CostKind.Depreciation, demand.Locus, hours),
                new CostActivity(CostKind.Energy, demand.Locus, energy))
                .Concat(demand.Intrinsic),
            Seq(new ImpactActivity(CarbonKind.Electricity, demand.Locus, energy)));
    }

    private static EstimateRow.Money Price(CostActivity activity, EstimateBasis basis) =>
        new(activity.Kind, CostStage.Unit, activity.Locus, basis.Currency, activity.Quantity,
            activity.Kind == CostKind.Machine
                ? basis.Find<EstimateEvidence.Machine>().Map(static value => (decimal)value.Receipt.HourlyRate)
                    .IfNone(basis.Tariffs[activity.Kind])
                : basis.Tariffs[activity.Kind]);

    private static EstimateRow.Carbon Impact(ImpactActivity activity, EstimateBasis basis) =>
        new(activity.Kind, activity.Locus, activity.Quantity, basis.CarbonFactors[activity.Kind]);

    private static Seq<EstimateRow.Money> Scale(Seq<EstimateRow.Money> source, CommercialLoad load, QuotePolicy policy) =>
        source.Map(row => (Row: row, Rate: load.Factor(policy.Loading[(load, row.Kind)])))
            .Filter(static item => item.Row.Amount != decimal.Zero && item.Rate != 0.0)
            .Map(item => item.Row with {
                Stage = load.Stage,
                Locus = $"{item.Row.Locus}:{load.Stage.Key}",
                Quantity = (double)item.Row.Amount,
                Rate = (decimal)item.Rate,
            });

    private static Seq<EstimateRow.Money> Risk(
        Seq<EstimateRow.Money> rows,
        Map<CostKind, double> variation,
        double confidence) {
        Seq<(EstimateRow.Money Row, double Variance)> weighted = rows.Map(row => (row,
            Math.Pow((double)decimal.Abs(row.Amount) * variation[row.Kind], 2.0)));
        double sigma = Math.Sqrt(weighted.Sum(static item => item.Variance));
        double z = Normal.InvCDF(0.0, 1.0, confidence);
        return sigma == 0.0 ? Seq<EstimateRow.Money>() : weighted.Filter(static item => item.Variance > 0.0)
            .Map(item => item.Row with {
                Stage = CostStage.Risk,
                Locus = $"{item.Row.Locus}:{CostStage.Risk.Key}",
                Quantity = item.Variance / sigma,
                Rate = (decimal)z,
            });
    }

    internal static Map<CostKind, decimal> Reconcile(Seq<EstimateRow.Money> rows) =>
        rows.Fold(Map<CostKind, decimal>(), static (totals, row) =>
            totals.AddOrUpdate(row.Kind, totals.Find(row.Kind).IfNone(decimal.Zero) + row.Amount));

    internal static Map<CarbonKind, double> Reconcile(Seq<EstimateRow.Carbon> rows) =>
        rows.Fold(Map<CarbonKind, double>(), static (totals, row) =>
            totals.AddOrUpdate(row.Kind, totals.Find(row.Kind).IfNone(0.0) + row.KgCo2e));
}
```
