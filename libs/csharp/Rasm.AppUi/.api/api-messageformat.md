# [RASM_APPUI_API_MESSAGEFORMAT]

`MessageFormat` (assembly `Jeffijoe.MessageFormat`) is the managed ICU MessageFormat engine — the CLDR-driven `plural` / `selectordinal` / `select` / variable-substitution pattern interpreter materializing `ResolvedLocale.Plural` over the Theme/locale resx pattern vocabulary. One `MessageFormatter` parses an ICU pattern string (with nested arguments, `#` plural-value substitution, and `{0, plural, one {...} other {...}}` / `{g, select, male {...} female {...} other {...}}` blocks) against an args map and a `CultureInfo`, picking the grammatically-correct branch from the per-locale CLDR plural rules. It retires any hand-rolled `count == 1 ? singular : plural` string-concatenation: the pattern is the localizable artifact, the formatter the one resolution authority. The whole surface is a small, deep formatter SPI — `IFormatter` arg-type handlers, `Pluralizer` per-locale rule delegates, and a `CustomValueFormatter` for date/time/number coercion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessageFormat`

- package: `MessageFormat` (NuGet id `MessageFormat`)
- assembly: `Jeffijoe.MessageFormat` (the shipped assembly id differs from the package id)
- namespace: `Jeffijoe.MessageFormat`, `Jeffijoe.MessageFormat.Formatting` (the `IFormatter`/`IFormatterLibrary` SPI), `Jeffijoe.MessageFormat.Formatting.Formatters` (the concrete `PluralFormatter`/`SelectFormatter`/`VariableFormatter` handlers), `Jeffijoe.MessageFormat.Parsing` (the pattern parser)
- license: MIT (repo `github.com/jeffijoe/messageformat.net`; the nuspec ships `<authors>Jeff Hansen</authors>` with no `<license>` expression — MIT per the repository LICENSE)
- build-floor: ships a real `lib/net10.0` asset (plus `net8.0`/`netstandard2.0`/`netstandard2.1`); the `net10.0` consumer binds `lib/net10.0` exactly — the documented surface, no forward-bind fallback
- dependencies: zero — the `net10.0` dependency group is empty (pure-managed, BCL-only: `CultureInfo` + `StringBuilder`)
- rail: locale

## [02]-[PUBLIC_TYPES]

[ENGINE_TYPES]: the formatter contract resolves patterns through one engine, while extension and coercion surfaces adapt inputs and typed values.

- rail: locale

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [ROLE]                 |
| :-----: | :------------------------------------- | :------------- | :--------------------- |
|  [01]   | `IMessageFormatter`                    | contract       | pattern resolution     |
|  [02]   | `MessageFormatter : IMessageFormatter` | engine         | ICU interpretation     |
|  [03]   | `MessageFormatterExtensions`           | static ext     | input adaptation       |
|  [04]   | `CustomValueFormatter` (`abstract`)    | value coercion | typed rendering        |
|  [05]   | `CustomValueFormatters`                | static factory | formatter construction |

[IMessageFormatter]:

- Signature: `string FormatMessage(string pattern, IReadOnlyDictionary<string,object?> argsMap, CultureInfo? culture = null)`

[MessageFormatterExtensions]:

- Inputs: `IDictionary<string,object>` or a POCO `object` reflected to a map

[CustomValueFormatter]:

- Scope: Overrides `date`, `time`, and `number` argument rendering

[CustomValueFormatters]:

- Scope: Constructs the default and composite value formatters

[FORMATTER_SPI]: the arg-type handler library — `Jeffijoe.MessageFormat.Formatting` + `.Formatting.Formatters`

- rail: locale
- the engine resolves each `{name, type, ...}` argument by the first `IFormatter` whose `CanFormat(FormatterRequest)` accepts the `type` keyword; the `IFormatterLibrary` is the ordered, mutable handler list exposed on `MessageFormatter.Formatters`

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]    | [KEYWORD]      |
| :-----: | :---------------------------------------------------- | :--------------- | :------------- |
|  [01]   | `IFormatter`                                          | handler contract | typed dispatch |
|  [02]   | `IFormatterLibrary : IList<IFormatter>`               | handler list     | registry       |
|  [03]   | `FormatterLibrary : List<IFormatter>`                 | handler list     | default        |
|  [04]   | `PluralFormatter : BaseFormatter`                     | handler          | plural modes   |
|  [05]   | `SelectFormatter : BaseFormatter`                     | handler          | `select`       |
|  [06]   | `VariableFormatter : IFormatter`                      | handler          | variable       |
|  [07]   | `Pluralizer` (`delegate string Pluralizer(double n)`) | locale rule      | category       |

[IFormatter]:

- Dispatch: `bool CanFormat(FormatterRequest)` selects the argument `type`, and `Format` renders the argument

[IFormatterLibrary]:

- Registry: `MessageFormatter.Formatters` exposes the ordered handler list

[PluralFormatter]:

- Dispatch: `CanFormat` maps `plural` to `CardinalPluralizers` and `selectordinal` to `OrdinalPluralizers`
- Branches: `zero`, `one`, `two`, `few`, `many`, `other`, and exact `=n`, with `#` value substitution

[SelectFormatter]:

- Dispatch: Selects an arbitrary string key such as gender, status, or enum

[VariableFormatter]:

- Dispatch: Renders `{name}` through `CustomValueFormatter` for typed values

[Pluralizer]:

- Dispatch: Maps a number to its locale's CLDR plural category

## [03]-[ENTRYPOINTS]

[FORMAT]: the resolution surface

- rail: locale

| [INDEX] | [ENTRYPOINT]               | [MODE]   | [INPUT] |
| :-----: | :------------------------- | :------- | :------ |
|  [01]   | `MessageFormatter`         | reusable | map     |
|  [02]   | `FormatMessage`            | instance | map     |
|  [03]   | `Extensions.FormatMessage` | ext      | map     |
|  [04]   | `Extensions.FormatMessage` | ext      | POCO    |
|  [05]   | `MessageFormatter.Format`  | static   | map     |
|  [06]   | `MessageFormatter.Format`  | static   | POCO    |

[MessageFormatter]:

- Constructor: `new MessageFormatter(bool useCache = true, CultureInfo? culture = null, CustomValueFormatter? customValueFormatter = null)`
- Lifecycle: Reuses cached compiled patterns under the default culture

[FormatMessage]:

- Signature: `string FormatMessage(string pattern, IReadOnlyDictionary<string,object?> args, CultureInfo? culture = null)`
- Culture: Accepts a per-call override

[Extensions.FormatMessage]:

- Overloads: `MessageFormatterExtensions.FormatMessage(pattern, IDictionary<string,object> args, culture?)` and `(pattern, object args, culture?)`

[MessageFormatter.Format]:

- Overloads: `static string MessageFormatter.Format(string pattern, IReadOnlyDictionary<string,object?> data, CultureInfo? culture = null)` and `(pattern, object data, culture?)`
- Lifecycle: Performs one-shot resolution without cache reuse

[SPI]: the customization surface

- rail: locale

| [INDEX] | [SURFACE]                     | [CAPABILITY]  |
| :-----: | :---------------------------- | :------------ |
|  [01]   | `MessageFormatter.Formatters` | handler list  |
|  [02]   | `CardinalPluralizers`         | cardinal map  |
|  [03]   | `OrdinalPluralizers`          | ordinal map   |
|  [04]   | `CustomValueFormatter`        | typed values  |
|  [05]   | `TryFormatDate`               | date delegate |
|  [06]   | `TryFormatTime`               | time delegate |
|  [07]   | `TryFormatNumber`             | number hook   |

[MessageFormatter.Formatters]:

- Signature: `IFormatterLibrary MessageFormatter.Formatters`
- Mutation: Inserts a custom `IFormatter` for a new argument `type` keyword

[Pluralizers]:

- Signatures: `IDictionary<string,Pluralizer>? CardinalPluralizers` and `OrdinalPluralizers`
- Mutation: Adds or replaces a locale rule on the resolved `PluralFormatter`

[CustomValueFormatter]:

- Hooks: `TryFormatDate(CultureInfo, object?, string? style, out string?)`, `TryFormatTime(...)`, and `TryFormatNumber(...)`
- Patterns: Renders typed values for forms such as `{d, date, short}` and `{n, number, percent}`

[TryFormatDelegates]:

- Shape: `delegate bool(CultureInfo, object?, string?, out string?)`
- Dispatch: A composite `CustomValueFormatter` binds the date, time, and number delegates

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: the failure surface lifted at the locale edge

- rail: locale

The parser reports unbalanced braces and invalid argument syntax, while handler resolution reports an argument `type` absent from the formatter library.

| [INDEX] | [THROWN]                     | [CAUSE]               |
| :-----: | :--------------------------- | :-------------------- |
|  [01]   | `MessageFormatterException`  | malformed ICU pattern |
|  [02]   | `FormatterNotFoundException` | unmatched formatter   |

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
