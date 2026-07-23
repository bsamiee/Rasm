# [RASM_BIM_API_NODAMONEY]

`NodaMoney` owns the first-class monetary value algebra backing the `Planning/cost#ESTIMATE` 5D cost rail: `decimal`-precision `Money` arithmetic over the embedded ISO 4217 `Currency` registry, lossless multi-share allocation, FX conversion through `ExchangeRate`, and the ambient `MoneyContext` rounding/precision policy. `Money` is a `readonly struct` implementing the full generic-math operator set, so a priced cost line is an expression over typed money, never a free `(double, string)` pair.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaMoney`
- package: `NodaMoney` (Apache-2.0)
- assembly: `NodaMoney.dll` — net10 build bound; IL-only AnyCPU managed, no native, ALC-safe in the in-Rhino plugin
- namespace: `NodaMoney` (values), `NodaMoney.Exchange` (FX), `NodaMoney.Context` (rounding policy), `NodaMoney.Serialization` (serdes)
- depends: none on net10 — the STJ facades resolve in-box, the nuspec deps bind only the netstandard groups
- rail: `Planning/cost#ESTIMATE` (the 5D `CostItem`/`CostSchedule` cost algebra)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: money value family

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]       | [CAPABILITY]                                                                                 |
| :-----: | :----------------- | :------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `Money`            | money value         | `readonly struct` over `decimal Amount` + `Currency`; the priced scalar                      |
|  [02]   | `FastMoney`        | fast money value    | `readonly struct` OACurrency-backed (`long`, 4-decimal fixed); + `long` `*`/`/`              |
|  [03]   | `Currency`         | currency identity   | `readonly record struct` over ISO 4217 `Code`; `Symbol`/`MinimalAmount`/`NoCurrency`         |
|  [04]   | `CurrencyInfo`     | registry record     | `record`: decimal digits/symbol/name/minor-unit + `IFormatProvider`/`ICustomFormatter`       |
|  [05]   | `CurrencyRegistry` | registry seam       | `static`: `Get`/`TryGet`/`GetAllCurrencies`/`TryAdd`/`TryRemove` — mutable ISO 4217 registry |
|  [06]   | `MoneyExtensions`  | money allocation    | `static`: the `Money.Split` family (`int shares`/`int[] ratios`) -> `IEnumerable<Money>`     |
|  [07]   | `ExchangeRate`     | FX rate             | `readonly record struct` `(BaseCurrency, QuoteCurrency, decimal Rate)`; `Convert(Money)`     |
|  [08]   | `MinorUnit`        | minor-unit exponent | `enum: byte` — the decimal-digit exponent of a currency's minor unit                         |

[PUBLIC_TYPE_SCOPE]: rounding/precision policy and FX context

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                                                             |
| :-----: | :------------------------- | :---------------- | :----------------------------------------------------------------------- |
|  [01]   | `MoneyContext`             | precision policy  | `sealed record` (`IDisposable`), thread-current + scoped                 |
|  [02]   | `MoneyContextOptions`      | policy options    | `record` the `Create`/`CreateScope`/`CreateAndSetDefault` mutates        |
|  [03]   | `MoneyContextIndex`        | context handle    | `readonly struct` 7-bit handle the `Money` carries; `New()` factory only |
|  [04]   | `IRoundingStrategy`        | rounding contract | `Round(decimal, Currency, int?)` — the `MoneyContext.RoundingStrategy`   |
|  [05]   | `NoRounding`               | rounding strategy | `record` — exact, no rounding applied                                    |
|  [06]   | `StandardRounding`         | rounding strategy | `record(MidpointRounding Mode = ToEven)` — banker's rounding default     |
|  [07]   | `CashDenominationRounding` | rounding strategy | `record(decimal decimals)` — cash rounding to a denomination (e.g. CHF)  |
|  [08]   | `Transaction`              | transaction POCO  | mutable single-transaction `class`, not an aggregation bag               |

- `MoneyContext` fields: `RoundingStrategy` `Precision` `MaxScale` `DefaultCurrency` `EnforceZeroCurrencyMatching`.
- `Transaction` properties: `Amount`/`Tax`/`Discount` (`Money`), `ExchangeRate`.

[PUBLIC_TYPE_SCOPE]: boundary serialization and failure

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [CAPABILITY]                                                             |
| :-----: | :------------------------------ | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `MoneyJsonConverter`            | STJ converter    | `JsonConverter<Money>` — the `System.Text.Json` money codec              |
|  [02]   | `CurrencyJsonConverter`         | STJ converter    | `JsonConverter<Currency>` — the currency-code codec                      |
|  [03]   | `MoneyTypeConverter`            | TypeConverter    | the `System.ComponentModel` string↔value converter                       |
|  [04]   | `CurrencyTypeConverter`         | TypeConverter    | the currency `System.ComponentModel` string↔value converter              |
|  [05]   | `InvalidCurrencyException`      | currency failure | thrown on an unknown ISO 4217 code (`FromCode` miss)                     |
|  [06]   | `MoneyContextMismatchException` | currency failure | thrown on a cross-currency op when `EnforceZeroCurrencyMatching` rejects |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: money construction and currency resolution

| [INDEX] | [SURFACE]                                                             | [SHAPE]            | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------- | :----------------- | :---------------------------------- |
|  [01]   | `new Money(decimal amount, string code)`                              | ctor               | amount + ISO 4217 code              |
|  [02]   | `new Money(decimal amount, Currency currency)`                        | ctor               | amount + resolved currency          |
|  [03]   | `new Money(decimal amount, Currency currency, MoneyContext? context)` | ctor               | + rounding/precision context        |
|  [04]   | `new Money(double amount, string code)`                               | ctor               | double intake, rounds to minor unit |
|  [05]   | `new Money(long amount, string code)`                                 | ctor               | long-amount intake                  |
|  [06]   | `Money.FromMinorUnits(long minorUnits, Currency currency)`            | minor-unit factory | integer minor-unit form             |
|  [07]   | `Money.ToMinorUnits()`                                                | minor-unit read    | the lossless minor-unit value       |
|  [08]   | `Money.Amount` / `Money.Currency`                                     | value read         | `decimal` scalar + `Currency` read  |
|  [09]   | `Money.Deconstruct(out decimal, out Currency)`                        | deconstruct        | pattern-match destructuring         |
|  [10]   | `Currency.FromCode(string code)`                                      | currency factory   | code -> `Currency`, throws on miss  |
|  [11]   | `Currency.Code` / `Symbol` / `MinimalAmount`                          | currency read      | code, symbol, smallest amount       |
|  [12]   | `Currency.NoCurrency`                                                 | currency anchor    | the no-currency neutral sentinel    |
|  [13]   | `Money.<Currency>`                                                    | currency shortcut  | major-currency shortcut factories   |

- `Money.<Currency>` shortcuts: `Euro` `USDollar` `PoundSterling` `Yen` `Yuan` (decimal/double/long/ulong overloads).

[ENTRYPOINT_SCOPE]: money arithmetic, allocation, and FX

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                                              |
| :-----: | :------------------------------------ | :------- | :------------------------------------------------------------------------ |
|  [01]   | `operator +` / `operator -`           | operator | `Money + Money`, `Money + decimal` -> `Money` (same-currency)             |
|  [02]   | `operator *` (`Money, decimal`)       | operator | `Money * decimal -> Money` — unit rate x takeoff quantity                 |
|  [03]   | `operator /` (`Money, decimal`)       | operator | `Money / decimal -> Money` — per-basis rate (the `UnitBasis` divide)      |
|  [04]   | `operator /` (`Money, Money`)         | operator | `Money / Money -> decimal` — dimensionless cost ratio                     |
|  [05]   | `operator %` (`Money, Money`)         | operator | `Money % Money -> Money` — the allocation remainder                       |
|  [06]   | `Money` static ops                    | static   | `Add`/`Subtract`/`Multiply`/`Divide`/`Remainder` (by-ref `in`, no boxing) |
|  [07]   | `Money.Split(int shares)`             | fold     | `MoneyExtensions` ext -> `IEnumerable<Money>`; N equal shares             |
|  [08]   | `Money.Split(int[] ratios)`           | fold     | `MoneyExtensions` ext -> `IEnumerable<Money>`; weighted ratios            |
|  [09]   | `new ExchangeRate(base, quote, rate)` | ctor     | `Currency`/`string` code + `decimal`/`double` rate legs                   |
|  [10]   | `ExchangeRate.Convert(Money money)`   | instance | reprice a `Money` from base into the quote currency                       |
|  [11]   | `Money` comparison                    | operator | `CompareTo`/`<`/`>`/`<=`/`>=`/`Compare(in, in)` — same-currency           |
|  [12]   | `Money` identities                    | static   | `MinValue`/`MaxValue`/`AdditiveIdentity`/`MultiplicativeIdentity`         |
|  [13]   | `Money` sign tests                    | static   | `Abs`/`IsNegative`/`IsPositive`/`IsZero(in)` — never a raw `< 0`          |
|  [14]   | `Money` magnitude                     | static   | `MinMagnitude`/`MaxMagnitude`                                             |

[ENTRYPOINT_SCOPE]: parse, format, and the rounding/precision context

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------- | :------- | :---------------------------------------------------------------------- |
|  [01]   | `Money.Parse(...)`                       | static   | currency-aware `string`/`ReadOnlySpan<char>` (`IFormatProvider?`) parse |
|  [02]   | `Money.TryParse(...)`                    | static   | the non-throwing money parse (the boundary intake)                      |
|  [03]   | `Money.ToString(...)` / `TryFormat(...)` | instance | culture/format money rendering (UTF-8 span included)                    |
|  [04]   | `ExchangeRate.Parse` / `TryParse`        | static   | parse a `"USD/EUR"`-style rate string                                   |
|  [05]   | `MoneyContext.Create(...)`               | factory  | construct a named rounding/precision policy                             |
|  [06]   | `MoneyContext.CreateScope(...)`          | factory  | install an ambient context for a `using` block (`IDisposable`)          |
|  [07]   | `MoneyContext.CreateAndSetDefault(...)`  | factory  | set the process-default policy; `Get(name)` looks a named context up    |
|  [08]   | `MoneyContext` current reads             | property | `CurrentContext`/`DefaultThreadContext`/`ThreadContext` in force        |
|  [09]   | `MoneyContext` policy reads              | property | `RoundingStrategy`/`Precision`/`MaxScale`/`DefaultCurrency`             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Money` is a `readonly struct` over `decimal Amount` + `Currency` carrying a `MoneyContextIndex` to its registered rounding/precision policy; the generic-math operators make `Money + Money`, `Money * decimal`, `Money / decimal`, `Money / Money -> decimal`, and the comparison/equality/parse/format surface native, never a hand-written arithmetic helper.
- `Money` satisfies the generic-math interfaces: `IAdditionOperators<Money,Money,Money>` `IAdditionOperators<Money,decimal,Money>` `ISubtractionOperators` `IMultiplyOperators<Money,decimal,Money>` `IDivisionOperators<Money,decimal,Money>` `IDivisionOperators<Money,Money,decimal>` `IModulusOperators<Money,Money,Money>` `IComparisonOperators<Money,Money,bool>` `IEqualityOperators` `IMinMaxValue<Money>` `IAdditiveIdentity<Money,Money>` `IMultiplicativeIdentity<Money,decimal>` `IParsable<Money>` `ISpanParsable<Money>` `IUtf8SpanParsable<Money>` `ISpanFormattable` `IUtf8SpanFormattable`.
- `Currency` is a `readonly record struct` over the ISO 4217 `Code`; `Symbol`/`MinimalAmount` resolve through `CurrencyInfo.GetInstance(currency)` against the embedded registry, and `Currency.NoCurrency` is the neutral sentinel.
- `Money`'s `decimal` payload is the exact base-10 form a financial amount requires; `double`-amount ctors round to the currency's minor unit on construction, and `ToMinorUnits`/`FromMinorUnits` is the integer-penny round-trip a wire/ledger persists without float drift.
- `MoneyExtensions.Split` distributes a total into equal shares or weighted integer ratios losslessly — the remainder pennies spread by the rounding rule so the parts sum exactly to the whole, the operation a naive `amount / n` multiply silently loses.
- `MoneyContext` is the ambient rounding/precision policy — a thread-current `sealed record` installed through `CreateScope` — so a whole estimate folds under one `IRoundingStrategy` (`StandardRounding(ToEven)` banker's default, `NoRounding` exact, `CashDenominationRounding(decimals)` smallest-coin) rather than threading `MidpointRounding` through every constructor.

[STACKING]:
- seam `MeasureValue` (`Rasm.Element/Properties/quantity#MEASURE_VALUE`): the `CostItem.ValueOf` join is the canonical stack — the seam owns the dimensioned takeoff read as the SI magnitude `Quantity.Si`, `Money` owns the priced scalar, and the line value is `Rate * (decimal)Quantity.Si` (`Money * decimal -> Money`); `Money / decimal` applies the `UnitBasis` per-basis denominator.
- `GeometryGymIFC_Core` (`.api/api-geometrygym-ifc.md`): the `CostProjection.ValueOf` fold lifts the IFC `(double AppliedValue, string Currency, double UnitBasis)` triple off `IfcCostValue.AppliedValue`/`UnitBasis.UnitComponent`(`IfcMonetaryUnit.Currency`)/`UnitBasis.ValueComponent` into `new Money((decimal)amount, Currency.FromCode(iso4217))` then `Money / (decimal)basis`, so the foreign amount becomes typed money at the boundary.
- `MPXJ.Net` (`Rasm.Persistence/.api/api-mpxj.md`): the 5D cost-input peer of the IFC graph — `ResourceAssignment.Cost`/`.BudgetCost`/`.Units` and `Resource.StandardRate`/`.Cost`/`.CostPerUse`/`.OvertimeRate` surface as parse-only foreign `double?`; the same `CostProjection.ValueOf` fold lifts each into `Money` at the boundary, the rate as `new Money((decimal)StandardRate.Amount, ...)` then `Money * (decimal)Units`. `QuikGraph` (`libs/csharp/.api/api-quikgraph.md`) owns the CPM schedule math over `Task.Predecessors`/`Successors`; `NodaMoney` owns only the priced scalar the resource loading projects onto.
- `LanguageExt.Core`: an unknown ISO 4217 code traps through `CostMoney.Of` (`Try.lift<Money>(...).Run()` catching `InvalidCurrencyException`) onto the typed `Model/faults#FAULT_BAND` `BimFault.CodecReject` lifted bare onto `Fin<T>` (the band is `Expected`-derived, no `.ToError()` hop); `CostSchedule.Rollup` is a `Fold` over the `Money` additive operator with `Money.AdditiveIdentity` the no-currency anchor, and a cross-currency rollup reprices each line through `ExchangeRate.Convert` into the reporting currency first.
- `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-json.md`): the `CostCategory`/`CostScheduleKind`/`ResourceKind` `[SmartEnum]` rows own the cost discriminants and `Money` is the payload a priced line carries; the JSON wire serializes `Money`/`Currency` through `MoneyJsonConverter`/`CurrencyJsonConverter` at the `Exchange/wire` boundary.

[LOCAL_ADMISSION]:
- `Money` is the `Planning/cost#ESTIMATE` priced scalar and `Money.Currency` the currency identity; the cost algebra is `Money + Money`, `Money * decimal`, and `Money / decimal`.
- currency resolution enters through `Currency.FromCode` against the ISO 4217 registry; a regional or custom currency registers through `CurrencyRegistry.TryAdd`/`TryRemove`.
- a multi-share allocation (a lump-sum split across line items, a contingency drawdown across packages) enters through `MoneyExtensions.Split` so the parts sum exactly.
- cross-currency repricing enters through `ExchangeRate.Convert` on a both-legs-matched fx row.
- `MoneyContext` scope owns the rounding/precision policy, folding a whole estimate under one ambient rule.
- `Money`/`Currency` cross the `Exchange/wire` boundary through `MoneyJsonConverter`/`CurrencyJsonConverter` or the integer `ToMinorUnits` form; internal code holds the typed value per the boundary-mapping law.
- `FastMoney` is admitted only where a hot inner aggregation needs its `long` 4-decimal storage, never as a parallel money model.

[RAIL_LAW]:
- Package: `NodaMoney`
- Owns: the `Money`/`Currency`/`ExchangeRate` monetary value algebra — `decimal`-precision arithmetic, the embedded ISO 4217 registry (mutable through `CurrencyRegistry.TryAdd`/`TryRemove`), lossless `MoneyExtensions.Split` allocation, FX conversion, the `MoneyContext` rounding/precision policy, and the `NodaMoney.Serialization.*` `System.Text.Json`/`TypeConverter` boundary serdes
- Accept: a `CostItem` value carried as `Money`, the `CostSchedule.Rollup` as a railed `Money` fold, the unit rate as `Money / decimal`, the quantity x rate join as `Money * (decimal)Quantity.Si` over the seam `MeasureValue`, lossless multi-share allocation via `MoneyExtensions.Split`, and cross-currency reprice via `ExchangeRate.Convert` on a both-legs-matched rate
- Reject: a hand-rolled `MonetaryAmount` `(double Amount, string Currency)` carrier beside `Money`; a `double` cost-arithmetic helper where the `Money` operators discriminate; a naive `total / n` allocation losing remainder pennies where `Split` is lossless; a stringly-typed currency field where `Currency.FromCode` resolves the ISO 4217 registry; a thrown `InvalidCurrencyException` in domain code instead of the typed `BimFault.CodecReject` lifted bare; a `Transaction`-based rollup where the per-currency partition owns mixed-currency aggregation; a second rounding policy threaded as a `MidpointRounding` argument where one `MoneyContext` scope governs
