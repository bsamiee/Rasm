# [RASM_APPUI_API_MESSAGEFORMAT]

`MessageFormat` (assembly `Jeffijoe.MessageFormat`) is the managed ICU MessageFormat engine — the CLDR-driven `plural` / `selectordinal` / `select` / variable-substitution pattern interpreter materializing `ResolvedLocale.Plural` over the Theme/locale resx pattern vocabulary. One `MessageFormatter` parses an ICU pattern string (with nested arguments, `#` plural-value substitution, and `{0, plural, one {...} other {...}}` / `{g, select, male {...} female {...} other {...}}` blocks) against an args map and a `CultureInfo`, picking the grammatically-correct branch from the per-locale CLDR plural rules. It retires any hand-rolled `count == 1 ? singular : plural` string-concatenation: the pattern is the localizable artifact, the formatter the one resolution authority. The whole surface is a small, deep formatter SPI — `IFormatter` arg-type handlers, `Pluralizer` per-locale rule delegates, and a `CustomValueFormatter` for date/time/number coercion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessageFormat`
- package: `MessageFormat` (NuGet id `MessageFormat`)
- version: `8.0.0`
- assembly: `Jeffijoe.MessageFormat` (the shipped assembly id differs from the package id)
- namespace: `Jeffijoe.MessageFormat`, `Jeffijoe.MessageFormat.Formatting` (the `IFormatter`/`IFormatterLibrary` SPI), `Jeffijoe.MessageFormat.Formatting.Formatters` (the concrete `PluralFormatter`/`SelectFormatter`/`VariableFormatter` handlers), `Jeffijoe.MessageFormat.Parsing` (the pattern parser)
- license: MIT (repo `github.com/jeffijoe/messageformat.net`; the nuspec ships `<authors>Jeff Hansen</authors>` with no `<license>` expression — MIT per the repository LICENSE)
- build-floor: ships a real `lib/net10.0` asset (plus `net8.0`/`netstandard2.0`/`netstandard2.1`); the `net10.0` consumer binds `lib/net10.0` exactly — the documented surface, no forward-bind fallback
- dependencies: zero — the `net10.0` dependency group is empty (pure-managed, BCL-only: `CultureInfo` + `StringBuilder`)
- rail: locale

## [02]-[PUBLIC_TYPES]

[ENGINE_TYPES]: the formatter and its contract
- rail: locale

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `IMessageFormatter`                      | contract        | `string FormatMessage(string pattern, IReadOnlyDictionary<string,object?> argsMap, CultureInfo? culture = null)` |
|  [02]   | `MessageFormatter : IMessageFormatter`   | engine          | the concrete ICU MessageFormat interpreter (cache + formatter library + culture) |
|  [03]   | `MessageFormatterExtensions`             | static ext      | overloads accepting `IDictionary<string,object>` or a POCO `object` (reflected to a map) |
|  [04]   | `CustomValueFormatter` (`abstract`)      | value coercion  | the SPI to override how `date`/`time`/`number` argument values are rendered |
|  [05]   | `CustomValueFormatters`                  | static factory  | built-in `CustomValueFormatter` constructions (the default/composite value formatters) |

[FORMATTER_SPI]: the arg-type handler library — `Jeffijoe.MessageFormat.Formatting` + `.Formatting.Formatters`
- rail: locale
- the engine resolves each `{name, type, ...}` argument by the first `IFormatter` whose `CanFormat(FormatterRequest)` accepts the `type` keyword; the `IFormatterLibrary` is the ordered, mutable handler list exposed on `MessageFormatter.Formatters`

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `IFormatter`                             | handler contract | one ICU argument-type handler — `bool CanFormat(FormatterRequest)` dispatch + `Format` |
|  [02]   | `IFormatterLibrary : IList<IFormatter>`  | handler list    | the ordered handler registry (`MessageFormatter.Formatters`) |
|  [03]   | `FormatterLibrary : List<IFormatter>`    | handler list    | the concrete default library                                 |
|  [04]   | `PluralFormatter : BaseFormatter`        | `plural` + `selectordinal` handler | ONE handler owning BOTH keywords — `CanFormat` accepts `plural` (→ `CardinalPluralizers`) and `selectordinal` (→ `OrdinalPluralizers`); picks the CLDR branch (`zero`/`one`/`two`/`few`/`many`/`other`, `=n` exact), `#` value substitution |
|  [05]   | `SelectFormatter : BaseFormatter`        | `select` handler | arbitrary keyed branch on a string value (gender, status, enum) |
|  [06]   | `VariableFormatter : IFormatter`         | default handler | plain `{name}` substitution (with `CustomValueFormatter` for typed values) |
|  [07]   | `Pluralizer` (`delegate string Pluralizer(double n)`) | per-locale rule | maps a number to its CLDR plural category for one locale     |

## [03]-[ENTRYPOINTS]

[FORMAT]: the resolution surface
- rail: locale

| [INDEX] | [SURFACE]                                                                  | [SHAPE / CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new MessageFormatter(bool useCache = true, CultureInfo? culture = null, CustomValueFormatter? customValueFormatter = null)` | construct a reusable engine (default culture + cached compiled patterns) |
|  [02]   | `string FormatMessage(string pattern, IReadOnlyDictionary<string,object?> args, CultureInfo? culture = null)` | the instance resolution — per-call culture override          |
|  [03]   | `MessageFormatterExtensions.FormatMessage(pattern, IDictionary<string,object> args, culture?)` / `(pattern, object args, culture?)` | the ergonomic dictionary / POCO overloads                    |
|  [04]   | `static string MessageFormatter.Format(string pattern, IReadOnlyDictionary<string,object?> data, CultureInfo? culture = null)` / `(pattern, object data, culture?)` | the one-shot static convenience (no reuse / no cache benefit) |

[SPI]: the customization surface
- rail: locale

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `IFormatterLibrary MessageFormatter.Formatters`                            | the live handler list — insert a custom `IFormatter` for a new argument `type` keyword |
|  [02]   | `IDictionary<string,Pluralizer>? CardinalPluralizers` / `OrdinalPluralizers` | the per-locale CLDR rule maps (off the resolved `PluralFormatter`) — add/replace a locale's plural rule |
|  [03]   | `CustomValueFormatter.TryFormatDate(CultureInfo, object?, string? style, out string?)` / `TryFormatTime(...)` / `TryFormatNumber(...)` | override typed-value rendering (`{d, date, short}` / `{n, number, percent}`) |
|  [04]   | `TryFormatDate` / `TryFormatTime` / `TryFormatNumber` (`delegate bool(CultureInfo, object?, string?, out string?)`) | the delegate shape a composite `CustomValueFormatter` dispatches to |

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: the failure surface lifted at the locale edge
- rail: locale

| [INDEX] | [THROWN]                          | [DISCRIMINANT / CAUSE]                                        |
| :-----: | :-------------------------------- | :----------------------------------------------------------- |
|  [01]   | `MessageFormatterException`       | a malformed ICU pattern — unbalanced braces, bad argument syntax (the parse fault) |
|  [02]   | `FormatterNotFoundException`      | a pattern argument-type keyword with no matching `IFormatter` in the library |

A pattern is authored content (resx), so a `MessageFormatterException` is a content-validation failure caught at build/load, not a runtime user fault; the Theme/locale rail validates patterns against their args before they ship.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- resx pattern → resolved string: the Theme/locale `ResolvedLocale.Plural` is the `MessageFormatter` bound once per active `CultureInfo`; a resx entry holds the ICU pattern (`{count, plural, one {# item} other {# items}}`), and the localized string is `formatter.FormatMessage(pattern, args)` — the pattern is the only localizable artifact, retiring every inline `count == 1 ? ... : ...` branch.
- one engine, cached, per culture: `new MessageFormatter(useCache: true, culture)` compiles each pattern once and reuses it; the formatter is a long-lived singleton in the locale rail, the per-call `culture` override handling a transient locale switch (preview a string in another language) without a second engine.
- typed values through one coercion hook: a `CustomValueFormatter` (constructed via `CustomValueFormatters`) renders `date`/`time`/`number` arguments — the same hook the `UnitsNet` quantity display and `NodaTime` instant formatting flow through, so a `{when, date, long}` or `{qty, number}` argument respects the active culture's calendar/number format without a parallel formatting path.
- CLDR rules are data, not code: `CardinalPluralizers`/`OrdinalPluralizers` are the per-locale `Pluralizer` delegate maps the engine ships from CLDR; a locale the app adds registers its rule by inserting a `Pluralizer` rather than branching in app code — the bounded plural-category vocabulary (`zero`/`one`/`two`/`few`/`many`/`other`) is the dispatch table.
- presentation-token alignment: the formatter output feeds the Theme/tokens text pipeline (`Wacton.Unicolour` colour, `Avalonia.Fonts.Inter` typography) as a plain string — MessageFormat owns grammar/plurality only, never layout or styling, so a localized label and its styled token never diverge.

[RAIL_LAW]:
- Packages: `MessageFormat` (assembly `Jeffijoe.MessageFormat`; zero deps, pure-managed)
- Owns: the Theme/locale ICU-MessageFormat resolution — `plural`/`selectordinal`/`select`/variable pattern interpretation over the resx vocabulary, materializing `ResolvedLocale.Plural`
- Accept: one cached `MessageFormatter` per active culture as a singleton; the resx ICU pattern as the localizable artifact; an args map (or POCO) per call; a per-call `culture` override for transient locale preview; a `CustomValueFormatter` as the single typed-value (date/time/number) coercion hook; a registered `Pluralizer` for a new locale's CLDR rule
- Reject: an inline `n == 1 ? singular : plural` ternary anywhere outside an ICU pattern; a second formatting path for typed argument values beside `CustomValueFormatter`; a per-call engine construction (lose the pattern cache); a malformed pattern crossing build/load unvalidated (→ `MessageFormatterException` at content validation, not runtime)
