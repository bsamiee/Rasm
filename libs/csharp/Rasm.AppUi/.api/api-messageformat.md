# [RASM_APPUI_API_MESSAGEFORMAT]

`MessageFormat` interprets ICU patterns — `plural`, `selectordinal`, `select`, and variable substitution — picking the correct branch from per-locale CLDR rules, and owns the Theme/locale resolution materializing `ResolvedLocale.Plural`. One `MessageFormatter` parses a pattern against an args map and a `CultureInfo`, retiring every `n == 1 ? singular : plural` concatenation: the pattern is the localizable artifact, the formatter the authority. Its SPI stays small, deep — `IFormatter` arg-type handlers, per-locale `Pluralizer` rules, and a `CustomValueFormatter` for date/time/number coercion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessageFormat`
- package: `MessageFormat` (MIT)
- assembly: `Jeffijoe.MessageFormat`
- namespace: `Jeffijoe.MessageFormat` (engine), `.Formatting` + `.Formatting.Formatters` (handler SPI), `.Parsing` (pattern parser)
- depends: none — pure-managed over `CultureInfo` and `StringBuilder`
- rail: locale

## [02]-[PUBLIC_TYPES]

[ENGINE_TYPES]: one `MessageFormatter` interprets patterns; extension and coercion types adapt inputs and typed values.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :--------------------------- | :------------ | :--------------------------------- |
|  [01]   | `IMessageFormatter`          | interface     | pattern-resolution contract        |
|  [02]   | `MessageFormatter`           | class         | ICU pattern engine                 |
|  [03]   | `MessageFormatterExtensions` | static        | dictionary and POCO input adapter  |
|  [04]   | `CustomValueFormatter`       | abstract      | typed date/time/number coercion    |
|  [05]   | `CustomValueFormatters`      | sealed        | composite delegate-bound formatter |

- `MessageFormatterExtensions.FormatMessage`: its `object` overload reflects the POCO to a map, so it is trim-unsafe (`RequiresUnreferencedCode`).

[FORMATTER_SPI]: the engine resolves each `{name, type, …}` argument by the first `IFormatter` whose `CanFormat(FormatterRequest)` accepts the type keyword; `MessageFormatter.Formatters` is the ordered mutable handler list.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :-------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `IFormatter`                            | interface     | typed arg-handler contract         |
|  [02]   | `IFormatterLibrary : IList<IFormatter>` | interface     | ordered handler registry           |
|  [03]   | `FormatterLibrary : List<IFormatter>`   | class         | default handler set                |
|  [04]   | `PluralFormatter : BaseFormatter`       | class         | `plural` and `selectordinal` modes |
|  [05]   | `SelectFormatter : BaseFormatter`       | class         | `select` key dispatch              |
|  [06]   | `VariableFormatter : IFormatter`        | class         | `{name}` variable render           |
|  [07]   | `Pluralizer`                            | delegate      | locale CLDR category rule          |

- `PluralFormatter`: branches `zero` `one` `two` `few` `many` `other` and exact `=n`, `#` substituting the plural value.

## [03]-[ENTRYPOINTS]

[FORMAT]: every surface returns the resolved `string`; a map or reflected POCO supplies the arguments.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `new MessageFormatter(bool, CultureInfo?, CustomValueFormatter?)`             | ctor     | reusable cached engine           |
|  [02]   | `MessageFormatter.FormatMessage(string, IReadOnlyDictionary, CultureInfo?)`   | instance | resolve a pattern against a map  |
|  [03]   | `MessageFormatterExtensions.FormatMessage(string, IDictionary, CultureInfo?)` | instance | resolve against a mutable map    |
|  [04]   | `MessageFormatterExtensions.FormatMessage(string, object, CultureInfo?)`      | instance | resolve against a reflected POCO |
|  [05]   | `MessageFormatter.Format(string, IReadOnlyDictionary, CultureInfo?)`          | static   | one-shot map resolution          |
|  [06]   | `MessageFormatter.Format(string, object, CultureInfo?)`                       | static   | one-shot POCO resolution         |

- `new MessageFormatter(useCache: true, …)`: compiles each pattern once and reuses it across calls under the default culture; a per-call `culture` overrides for a transient locale.
- `MessageFormatter.Format`: one-shot through a shared default instance, no pattern-cache reuse.

[SPI]: custom handlers, locale rules, and typed-value coercion mutate the resolved engine.

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `MessageFormatter.Formatters`          | property | register an `IFormatter` for a new type keyword  |
|  [02]   | `MessageFormatter.CardinalPluralizers` | property | override a cardinal locale rule                  |
|  [03]   | `MessageFormatter.OrdinalPluralizers`  | property | override an ordinal locale rule                  |
|  [04]   | `CustomValueFormatters`                | ctor     | composite binding the date/time/number delegates |

- `MessageFormatter.CardinalPluralizers`: reads through the resolved `PluralFormatter` and is `null` until one is added.
- `CustomValueFormatters`: settable `TryFormatDate`/`TryFormatTime`/`TryFormatNumber` delegates render forms such as `{d, date, short}` and `{n, number, percent}`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MessageFormatter` resolves `plural`/`selectordinal`/`select`/variable branches by the per-locale CLDR rule; the ICU pattern is the one localizable artifact, retiring every inline `n == 1 ? singular : plural` branch.
- Typed faults lift at the locale edge: `MessageFormatterException` on a malformed pattern, `FormatterNotFoundException` on an argument type absent from the library. A pattern is authored resx content, so a malformed one is a content-validation failure caught at build/load, never a runtime user fault.

[STACKING]:
- `NodaTime`(`.api/api-nodatime.md`): an instant or date argument coerces through the `CustomValueFormatter` date/time delegates, so `{when, date, long}` respects the active culture's calendar without a parallel format path.
- `UnitsNet`(`.api/api-unitsnet.md`): a quantity renders through the same number delegate, so `{qty, number}` shares one culture-aware coercion hook.
- `Wacton.Unicolour`(`.api/api-unicolour.md`) + `Avalonia.Fonts.Inter`(`api-avalonia-fonts.md`): the resolved string feeds the Theme/tokens text pipeline as a plain string — `MessageFormat` owns grammar and plurality only, never layout or styling.
- Theme/locale rail: one cached `MessageFormatter` per active culture materializes `ResolvedLocale.Plural`, and `CardinalPluralizers`/`OrdinalPluralizers` are the CLDR `Pluralizer` data table a new locale extends by insertion.

[LOCAL_ADMISSION]:
- Theme/locale rail holds one cached `MessageFormatter` per active culture; a localized string resolves as `formatter.FormatMessage(pattern, args)` over a resx-carried ICU pattern.

[RAIL_LAW]:
- Package: `MessageFormat`
- Owns: the Theme/locale ICU-MessageFormat resolution — `plural`/`selectordinal`/`select`/variable interpretation over the resx vocabulary, materializing `ResolvedLocale.Plural`.
- Accept: one cached `MessageFormatter` per culture; the resx pattern as the sole localizable artifact; a `CustomValueFormatter` for date/time/number; a registered `Pluralizer` per added locale.
- Reject: an inline `n == 1 ? singular : plural` ternary outside a pattern; a second formatting path for typed values beside `CustomValueFormatter`; per-call engine construction losing the pattern cache.
