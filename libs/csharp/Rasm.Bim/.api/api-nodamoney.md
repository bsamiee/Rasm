# [RASM_BIM_API_NODAMONEY]

`NodaMoney` is the first-class monetary value owner backing the `Planning/cost#ESTIMATE`
5D cost algebra. The public surface spans FOUR namespaces, not one — the root `NodaMoney`
owns the values (`Money`, `FastMoney`, `Currency`, `CurrencyInfo`, `CurrencyRegistry`,
`MoneyExtensions`, `Transaction`, `MinorUnit`); the FX leg is `NodaMoney.Exchange.ExchangeRate`;
the rounding/precision policy is `NodaMoney.Context.{MoneyContext, MoneyContextOptions,
IRoundingStrategy, NoRounding, StandardRounding, CashDenominationRounding}`; and the boundary
serdes is `NodaMoney.Serialization.{Money,Currency}{Json,Type}Converter` — a cost owner
consuming the FX/context/serdes legs needs `using NodaMoney.Exchange;`/`using
NodaMoney.Context;`/`using NodaMoney.Serialization;`, never the root namespace alone.
`Money` is a `readonly struct` over a `decimal Amount` and a `Currency`, implementing the full
generic-math operator set
(`IAdditionOperators`/`ISubtractionOperators`/`IMultiplyOperators<Money, decimal, Money>`/
`IDivisionOperators<Money, decimal, Money>`/`IDivisionOperators<Money, Money, decimal>`/
`IModulusOperators`/`IComparisonOperators`/`IEqualityOperators`/`IMinMaxValue<Money>`/
`IAdditiveIdentity`/`IMultiplicativeIdentity<Money, decimal>`/`IIncrementOperators`/
`IDecrementOperators`/`IUnaryPlusOperators`/`IUnaryNegationOperators`/`IParsable`/
`ISpanParsable`/`IUtf8SpanParsable`/`ISpanFormattable`/`IUtf8SpanFormattable`/
`IXmlSerializable`/`ISerializable`), so a cost line is an expression over typed money rather
than a free `(double Amount, string Currency)` pair. `Currency` is a `readonly record struct`
over the ISO 4217 `Code`, resolved against the embedded `CurrencyInfo` registry (symbol, minor
unit, minimal amount) which `CurrencyRegistry.TryAdd`/`TryRemove` mutates for a custom/regional
currency. `MoneyExtensions.Split` is the lossless penny-distribution fold (whole-share or
weighted ratios — no remainder lost) returning an `IEnumerable<Money>`, `ExchangeRate.Convert`
is the FX convert leg, and `MoneyContext` is the ambient rounding/precision policy (rounding
strategy, precision, max scale, default currency, zero-currency matching) carried per `Money`
and scoped through a disposable. The `Planning/cost#ESTIMATE` owner realizes this surface: the
retired hand-rolled `MonetaryAmount` `(double Amount, string Currency)` carrier is `Money`, a
`CostItem` value is `Money`, `CostSchedule.Rollup` is a railed `Money` fold, the per-basis rate
is `Money / decimal`, and the cross-currency reprice composes `ExchangeRate.Convert` on the
both-legs-matched fx row. `Money` meets the seam takeoff at the `CostItem.ValueOf` quantity x
rate join — `Rate * (decimal)Quantity.Si` over the seam `MeasureValue` SI magnitude (`Money *
decimal → Money`) — the seam quantity supplying the multiplier and `Money` the priced scalar.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaMoney`
- package: `NodaMoney`
- license: Apache-2.0 (`license type="expression"`, `RemyDuijkeren/NodaMoney`)
- assembly: `NodaMoney` → the `net10.0` consumer binds `lib/net10.0/NodaMoney.dll` (a real `net10.0` asset ships alongside `net9.0`/`net8.0`/`netstandard2.1`/`netstandard2.0`; the bound surface is the net10 build)
- namespace: `NodaMoney` (values: `Money`/`FastMoney`/`Currency`/`CurrencyInfo`/`CurrencyRegistry`/`MoneyExtensions`/`Transaction`/`MinorUnit`), `NodaMoney.Exchange` (`ExchangeRate`), `NodaMoney.Context` (`MoneyContext`/`MoneyContextOptions`/`IRoundingStrategy`/`NoRounding`/`StandardRounding`/`CashDenominationRounding`), `NodaMoney.Serialization` (the four JSON/Type converters)
- asset: IL-only AnyCPU managed assembly; no native binaries, ALC-safe inside the in-Rhino plugin assembly
- dependency: the `net10.0` dependency group is EMPTY — the `System.Text.Encodings.Web`/`System.Text.Json` deps the nuspec declares apply ONLY to the `netstandard2.0`/`netstandard2.1` groups (those facades are in-box under net10), so the net10 bind pulls no transitive package
- scope: ISO 4217 currency registry, `decimal`-precision money arithmetic, lossless allocation/split, FX conversion, the `MoneyContext` rounding/precision policy, and the `System.Text.Json`/`TypeConverter` boundary serdes
- rail: `Planning/cost#ESTIMATE` (the 5D `CostItem`/`CostSchedule` cost algebra)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: money value family
- rail: cost
- note: `Money` is the canonical priced scalar; `FastMoney` is the OACurrency-backed (`long` storage, 4-decimal fixed, `Amount => decimal.FromOACurrency`) performance variant with the SAME operator set plus `long`-typed `*`/`/` overloads (admitted only where a hot inner aggregation needs it and 4-decimal precision suffices, never as a parallel money model). A `CostItem` value is ONE `Money`, never a `(double, string)` pair. `ExchangeRate` lives in `NodaMoney.Exchange`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]       | [RAIL]                                                                                       |
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
- rail: cost
- note: the policy types live in `NodaMoney.Context` (a separate `using`). `MoneyContext` is the NEW ambient policy replacing per-`Money` `MidpointRounding`: a thread-current `sealed record` carrying `RoundingStrategy`/`Precision`/`MaxScale`/`DefaultCurrency`/`EnforceZeroCurrencyMatching`, installed for a `using` block via `CreateScope`, banker's rounding by default. The `IRoundingStrategy` contract is `Round(decimal amount, Currency currency, int? decimals)`. The `Transaction` POCO carries `Money Amount`/`ExchangeRate ExchangeRate`/`Money Tax`/`Money Discount`.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                                                                          |
| :-----: | :------------------------- | :---------------- | :------------------------------------------------------------------------------ |
|  [01]   | `MoneyContext`             | precision policy  | `sealed record` (`IDisposable`), thread-current + scoped (fields in the note)   |
|  [02]   | `MoneyContextOptions`      | policy options    | `record` the `Create`/`CreateScope`/`CreateAndSetDefault` mutates               |
|  [03]   | `MoneyContextIndex`        | context handle    | `readonly struct` the `Money` carries to its registered context (internal slot) |
|  [04]   | `IRoundingStrategy`        | rounding contract | `Round(decimal, Currency, int?)` — the `MoneyContext.RoundingStrategy` seam     |
|  [05]   | `NoRounding`               | rounding strategy | `record` — exact, no rounding applied                                           |
|  [06]   | `StandardRounding`         | rounding strategy | `record(MidpointRounding Mode = ToEven)` — banker's rounding default            |
|  [07]   | `CashDenominationRounding` | rounding strategy | `record(decimal decimals)` — cash rounding to a denomination (e.g. CHF)         |
|  [08]   | `Transaction`              | transaction POCO  | mutable single-transaction `class` (properties in note), not an aggregation bag |

[PUBLIC_TYPE_SCOPE]: boundary serialization and failure
- rail: cost
- note: the JSON converters and `TypeConverter`s live in `NodaMoney.Serialization` — the boundary-only serdes the wire/persistence seam uses; internal cost code holds the typed `Money`/`Currency`. The exceptions are the package's throwing failure mode the cost rail traps onto `Fin<T>` — domain code never sees them.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [RAIL]                                                                   |
| :-----: | :------------------------------ | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `MoneyJsonConverter`            | STJ converter    | `JsonConverter<Money>` — the `System.Text.Json` money codec              |
|  [02]   | `CurrencyJsonConverter`         | STJ converter    | `JsonConverter<Currency>` — the currency-code codec                      |
|  [03]   | `MoneyTypeConverter`            | TypeConverter    | the `System.ComponentModel` string↔value converter                       |
|  [04]   | `CurrencyTypeConverter`         | TypeConverter    | the currency `System.ComponentModel` string↔value converter              |
|  [05]   | `InvalidCurrencyException`      | currency failure | thrown on an unknown ISO 4217 code (`FromCode` miss)                     |
|  [06]   | `MoneyContextMismatchException` | currency failure | thrown on a cross-currency op when `EnforceZeroCurrencyMatching` rejects |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: money construction and currency resolution
- rail: cost
- note: a `Money` constructs from a `decimal`/`double`/`long`/`ulong` amount plus a currency `code`/`Currency`/`MoneyContext`; the `ToMinorUnits`/`FromMinorUnits` pair is the integer-penny representation a wire/ledger persists losslessly. `Money.Euro`/`Money.USDollar`/`Money.PoundSterling`/`Money.Yen`/`Money.Yuan` are the major-currency shortcut factories (decimal/double/long/ulong overloads).

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY]     | [RAIL]                                 |
| :-----: | :-------------------------------------------------------------------- | :----------------- | :------------------------------------- |
|  [01]   | `new Money(decimal amount, string code)`                              | ctor               | amount + ISO 4217 code                 |
|  [02]   | `new Money(decimal amount, Currency currency)`                        | ctor               | amount + resolved currency             |
|  [03]   | `new Money(decimal amount, Currency currency, MoneyContext? context)` | ctor               | + rounding/precision context           |
|  [04]   | `new Money(double amount, string code)`                               | ctor               | double intake, rounds to minor unit    |
|  [05]   | `new Money(long amount, string code)`                                 | ctor               | long-amount intake                     |
|  [06]   | `Money.FromMinorUnits(long minorUnits, Currency currency)`            | minor-unit factory | integer minor-unit form                |
|  [07]   | `Money.ToMinorUnits()`                                                | minor-unit read    | the lossless minor-unit value          |
|  [08]   | `Money.Amount` / `Money.Currency`                                     | value read         | `decimal` scalar + `Currency` read     |
|  [09]   | `Money.Deconstruct(out decimal, out Currency)`                        | deconstruct        | pattern-match destructuring            |
|  [10]   | `Currency.FromCode(string code)`                                      | currency factory   | code -> `Currency`, throws on miss     |
|  [11]   | `Currency.Code` / `Symbol` / `MinimalAmount`                          | currency read      | code, symbol, smallest amount          |
|  [12]   | `Currency.NoCurrency`                                                 | currency anchor    | the no-currency neutral sentinel       |
|  [13]   | `Money.<Currency>`                                                    | currency shortcut  | major-currency shortcuts (in the note) |

[ENTRYPOINT_SCOPE]: money arithmetic, allocation, and FX
- rail: cost
- note: the operator set closes the cost algebra — `Money + Money`, `Money * decimal` (a unit-rate times a takeoff quantity), `Money / decimal` (a per-basis rate), `Money / Money → decimal` (a dimensionless ratio); `Split` is the lossless penny distribution `MonetaryAmount.Scale`-by-ratio cannot do; `ExchangeRate.Convert` is the cross-currency leg.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                                                                    |
| :-----: | :------------------------------------ | :------------- | :------------------------------------------------------------------------ |
|  [01]   | `operator +` / `operator -`           | additive       | `Money + Money`, `Money + decimal` -> `Money` (same-currency)             |
|  [02]   | `operator *` (`Money, decimal`)       | scaling        | `Money * decimal -> Money` — unit rate x takeoff quantity                 |
|  [03]   | `operator /` (`Money, decimal`)       | scaling        | `Money / decimal -> Money` — per-basis rate (the `UnitBasis` divide)      |
|  [04]   | `operator /` (`Money, Money`)         | ratio          | `Money / Money -> decimal` — dimensionless cost ratio                     |
|  [05]   | `operator %` (`Money, Money`)         | modulus        | `Money % Money -> Money` — the allocation remainder                       |
|  [06]   | `Money` static ops                    | static op      | `Add`/`Subtract`/`Multiply`/`Divide`/`Remainder` (by-ref `in`, no boxing) |
|  [07]   | `Money.Split(int shares)`             | allocation     | `MoneyExtensions` ext -> `IEnumerable<Money>`; N equal shares             |
|  [08]   | `Money.Split(int[] ratios)`           | allocation     | `MoneyExtensions` ext -> `IEnumerable<Money>`; weighted ratios            |
|  [09]   | `new ExchangeRate(base, quote, rate)` | FX ctor        | `Currency`/`string` code + `decimal`/`double` rate legs                   |
|  [10]   | `ExchangeRate.Convert(Money money)`   | FX convert     | reprice a `Money` from base into the quote currency                       |
|  [11]   | `Money` comparison                    | ordering       | `CompareTo`/`<`/`>`/`<=`/`>=`/`Compare(in, in)` — same-currency           |
|  [12]   | `Money` identities                    | math identity  | `MinValue`/`MaxValue`/`AdditiveIdentity`/`MultiplicativeIdentity`         |
|  [13]   | `Money` sign tests                    | static numeric | `Abs`/`IsNegative`/`IsPositive`/`IsZero(in)` — never a raw `< 0`          |
|  [14]   | `Money` magnitude                     | static numeric | `MinMagnitude`/`MaxMagnitude`                                             |

[ENTRYPOINT_SCOPE]: parse, format, and the rounding/precision context
- rail: cost
- note: `Money`/`Currency`/`ExchangeRate` are `IParsable`/`ISpanParsable`/`ISpanFormattable`; the `MoneyContext` is the ambient rounding/precision policy a `using` scope installs so a whole estimate folds under one rounding rule without threading `MidpointRounding` through every call.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [RAIL]                                                                  |
| :-----: | :--------------------------------------- | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `Money.Parse(...)`                       | parse           | currency-aware `string`/`ReadOnlySpan<char>` (`IFormatProvider?`) parse |
|  [02]   | `Money.TryParse(...)`                    | try-parse       | the non-throwing money parse (the boundary intake)                      |
|  [03]   | `Money.ToString(...)` / `TryFormat(...)` | format          | culture/format money rendering (UTF-8 span included)                    |
|  [04]   | `ExchangeRate.Parse` / `TryParse`        | FX parse        | parse a `"USD/EUR"`-style rate string                                   |
|  [05]   | `MoneyContext.Create(...)`               | context build   | construct a named rounding/precision policy                             |
|  [06]   | `MoneyContext.CreateScope(...)`          | context scope   | install an ambient context for a `using` block (`IDisposable`)          |
|  [07]   | `MoneyContext.CreateAndSetDefault(...)`  | context default | set the process-default policy; `Get(name)` looks a named context up    |
|  [08]   | `MoneyContext` current reads             | context read    | `CurrentContext`/`DefaultThreadContext`/`ThreadContext` in force        |
|  [09]   | `MoneyContext` policy reads (1)          | policy read     | `RoundingStrategy`/`Precision`/`MaxScale`                               |
|  [10]   | `MoneyContext` policy reads (2)          | policy read     | `DefaultCurrency`/`EnforceZeroCurrencyMatching`                         |

## [04]-[IMPLEMENTATION_LAW]

[MONEY_TOPOLOGY]:
- `Money` is a `readonly struct` over `decimal Amount` + `Currency` carrying a `MoneyContextIndex` slot to its registered rounding/precision policy; it implements the full generic-math operator set, so `Money + Money`, `Money * decimal`, `Money / decimal`, `Money / Money → decimal`, and the comparison/equality/parse/format interfaces are native, never a hand-written arithmetic helper
- `Currency` is a `readonly record struct` over the ISO 4217 `Code`; `Symbol`/`MinimalAmount` resolve through `CurrencyInfo.GetInstance(currency)` against the embedded registry, and `Currency.NoCurrency` is the neutral sentinel
- the `decimal` payload is the exact base-10 representation a financial amount requires — `double`-amount ctors round to the currency's minor unit on construction, and `ToMinorUnits`/`FromMinorUnits` is the integer-penny round-trip a wire/ledger persists without float drift
- `MoneyExtensions.Split` (an extension over `Money`, returning `IEnumerable<Money>`) distributes a total into equal shares or weighted integer ratios LOSSLESSLY — the remainder pennies are spread by the rounding rule so the parts sum EXACTLY to the whole, the operation a naive `amount / n` multiply silently loses
- `MoneyContext` (in `NodaMoney.Context`) is the ambient rounding/precision policy (a thread-current `sealed record` implementing `IDisposable`, installed through `CreateScope`); the rounding strategy is an `IRoundingStrategy` — `StandardRounding(ToEven)` banker's rounding by default, `NoRounding` for exact, `CashDenominationRounding(decimals)` for a smallest-coin rule — and a whole estimate folds under one context rather than threading `MidpointRounding` through every constructor

[INTEGRATION_STACK]:
- with the seam `MeasureValue` (`Rasm.Element/Properties/quantity#MEASURE_VALUE`): the `Planning/cost#ESTIMATE` `CostItem.ValueOf` join is the canonical stack — the seam owns the dimensioned takeoff (the `Element.Quantities` bag the `Bake` fold derives, read as the SI magnitude `Quantity.Si`), `Money` owns the priced scalar, and the line value is `Rate * (decimal)Quantity.Si` (`Money * decimal → Money`) — one rail from dimensioned quantity and priced rate to a `Money` line, never a `double` cross-multiply and never a parallel `UnitsNet` quantity on the cost page; `Money / decimal` applies the `UnitBasis` per-basis denominator
- with `GeometryGymIFC_Core` (`api-geometrygym-ifc`): the IFC cost graph surfaces a raw `(double AppliedValue, string Currency, double UnitBasis)` off `IfcCostValue.AppliedValue`/`UnitBasis.UnitComponent`(`IfcMonetaryUnit.Currency`)/`UnitBasis.ValueComponent` — the `CostProjection.ValueOf` fold lifts that triple into a `Money` through `new Money((decimal)amount, Currency.FromCode(iso4217))` then `Money / (decimal)basis`, so the foreign IFC amount becomes the typed money at the boundary and the rest of the cost algebra is `Money`
- with `MPXJ.Net` (`api-mpxj`, the `Rasm.Persistence` schedule-file codec): the parsed `ProjectFile` resource loading is the 5D cost-input PEER of the IFC cost graph — `ResourceAssignment.Cost`/`.BudgetCost`/`.Units` and `Resource.StandardRate` (a `Rate`)/`.Cost`/`.CostPerUse`/`.OvertimeRate` surface as foreign `double?` amounts (the codec is parse-only, carrying P6/MS-Project cost fields as raw doubles, not money). The `CostProjection.ValueOf` fold lifts each into a `Money` at the boundary exactly as the IFC triple does — `new Money((decimal)resourceAssignment.Cost.Value, Currency.FromCode(iso4217))`, the rate via `new Money((decimal)resource.StandardRate.Amount, Currency.FromCode(iso4217))` then `Money * (decimal)assignment.Units` — so the parsed schedule's resource-loading becomes typed `Money` once and the rest of the 5D algebra (the `CostSchedule.Rollup` sum, the cross-currency `ExchangeRate.Convert`) is the same `Money` fold. The MPXJ schedule MATH (the CPM order over `Task.Predecessors`/`Successors`) is `QuikGraph`'s (`api-quikgraph`), not this owner's — `NodaMoney` owns only the priced scalar the resource loading projects onto
- with `LanguageExt.Core`: an unknown ISO 4217 code traps through the railed `CostMoney.Of` (`Try.lift<Money>(...).Run()` catching `InvalidCurrencyException`) onto the typed `Model/faults#FAULT_BAND` `BimFault.CodecReject` lifted BARE onto `Fin<T>` (the band is `Expected`-derived — no `.ToError()` hop) — never a thrown currency exception in domain code; the `CostSchedule.Rollup` per-category total is a `Fold` over the `Money` additive operator (`Money.AdditiveIdentity` the no-currency anchor), never a manual `double` accumulation, and a cross-currency rollup reprices each line through `ExchangeRate.Convert` on the both-legs-matched fx row into the schedule's reporting currency first
- with `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-json.md`): the `CostCategory`/`CostScheduleKind`/`ResourceKind` `[SmartEnum]` rows own the cost discriminants and `Money` is the payload a priced line carries — the `[SmartEnum]` owns the partition key, `Money` owns the amount; the JSON wire serializes `Money`/`Currency` through `MoneyJsonConverter`/`CurrencyJsonConverter` at the `Exchange/wire` boundary, never a bespoke `(double, string)` DTO

[LOCAL_ADMISSION]:
- the `Planning/cost#ESTIMATE` priced scalar is `Money`; the hand-rolled `MonetaryAmount` `(double Amount, string Currency)` record with its manual `Add`/`Scale` and raw-`double` `Amount / UnitBasis` divide is the RETIRED form — `Money + Money`, `Money * decimal`, and `Money / decimal` own that algebra and `Money.Currency` owns the currency identity
- currency resolution enters through `Currency.FromCode`; a stringly-typed three-letter currency field validated by hand is the rejected form (the ISO 4217 registry is `Currency`'s)
- a multi-share cost allocation (a lump-sum split across line items, a contingency drawdown across packages) enters through `MoneyExtensions.Split` (the `Money` extension returning `IEnumerable<Money>`) so the parts sum exactly; a naive `total / n` multiply that loses remainder pennies is the deleted form
- cross-currency repricing enters through `ExchangeRate.Convert`; a hand-multiplied FX factor on a raw `double` is the deleted form
- the rounding/precision policy is a `MoneyContext` scope, not a `MidpointRounding` argument threaded through every `CostValue` construction; the schedule folds under one ambient context
- `Money`/`Currency` cross the `Exchange/wire` boundary through `MoneyJsonConverter`/`CurrencyJsonConverter` (or the integer `ToMinorUnits` form); `NodaMoney` types never leak past the cost owner into unrelated domain code — internal code holds the typed value per the boundary-mapping law

[RAIL_LAW]:
- Package: `NodaMoney` (, Apache-2.0, pure-managed `lib/net10.0` AnyCPU IL, empty net10 dependency group)
- Owns: the `Money`/`Currency`/`NodaMoney.Exchange.ExchangeRate` monetary value algebra — `decimal`-precision arithmetic, the embedded ISO 4217 registry (mutable through `CurrencyRegistry.TryAdd`/`TryRemove`), lossless `MoneyExtensions.Split` allocation, FX conversion, the `NodaMoney.Context.MoneyContext` rounding/precision policy (`StandardRounding`/`NoRounding`/`CashDenominationRounding`), and the `NodaMoney.Serialization.*` `System.Text.Json`/`TypeConverter` boundary serdes
- Accept: a `Planning/cost#ESTIMATE` `CostItem` value carried as `Money`, the `CostSchedule.Rollup` as a railed `Money` fold, the unit rate as `Money / decimal`, the quantity x rate join as `Money * (decimal)Quantity.Si` over the seam `MeasureValue`, lossless multi-share allocation via `MoneyExtensions.Split`, and cross-currency reprice via `ExchangeRate.Convert` on a both-legs-matched rate
- Reject: a hand-rolled `MonetaryAmount` `(double Amount, string Currency)` carrier beside `Money`; a `double` cost-arithmetic helper where the `Money` operators discriminate; a naive `total / n` allocation losing remainder pennies where `Split` is lossless; a stringly-typed currency field where `Currency.FromCode` resolves the ISO 4217 registry; a thrown `InvalidCurrencyException` in domain code instead of the typed `BimFault.CodecReject` lifted BARE; a `Transaction`-based rollup where the per-currency partition owns mixed-currency aggregation; a second rounding policy threaded as a `MidpointRounding` argument where one `MoneyContext` scope governs
