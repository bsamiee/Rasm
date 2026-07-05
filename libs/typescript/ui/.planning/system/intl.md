# [UI_INTL]

The localization plane with zero i18n package: one locale spine (react-aria's `I18nProvider` carrying the kernel `Refined.Locale` brand as the single ambient locale), one interior per-locale native-`Intl` instance cache shared by every formatter this page constructs itself, the `Format` value-formatting vocabulary (option-row tables for date/number/list, the relative-time granularity ladder, the single epoch crossing, collation `Order` and the picker filter), and the `Message` authority (Schema-decoded catalogs as app data, a closed three-case spec family, a total plural/select/interpolation fold, and the BCP-47 fallback chain with the key itself as visible missing-message evidence). No `Intl.*` constructor at a call site, no wall-clock `Date` in domain flow, no format options object authored inline, no ICU engine, no translation runtime. The module is `ui/src/system/intl.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         | [PUBLIC]  |
| :-----: | :--------------- | :------------------------------------------------------------------------------- | :-------- |
|  [01]   | `LOCALE_SPINE`   | the `I18nProvider`/`useLocale` ambient-locale law over the kernel brand           | —         |
|  [02]   | `NATIVE_CACHE`   | the one per-locale `Intl` instance cache behind every self-constructed formatter  | —         |
|  [03]   | `FORMAT_ROWS`    | `Format` — option-row vocabulary, epoch seam, relative ladder, collation, filter  | `Format`  |
|  [04]   | `MESSAGE_FAMILY` | the `MessageSpec` closed family and the `Catalog` decode owner                    | `Message` |
|  [05]   | `MESSAGE_FOLD`   | the total format fold and the locale fallback chain                               | `Message` |

## [2]-[LOCALE_SPINE]

[LOCALE_SPINE]:
- Law: the locale is one ambient value — the app root renders `I18nProvider` with the kernel `Refined.Locale` brand (a canonical BCP-47 string by construction, so it feeds the provider directly), the brand itself lives in a persisted atom (`system/atom#STORE_ROOT`'s persisted row — `Atom.kvs` with `Refined.Locale` as codec), and a locale change is one atom write that re-renders every formatter consumer; a second locale source, a `navigator.language` read in a row, or a prop-drilled locale string is the named defect.
- Law: direction derives — RAC and the react-aria hooks resolve RTL from the provided locale; layout rows consume logical properties (`start`/`end` utilities through `cn`) so direction never branches in JS.
- Law: hydration-sensitive reads gate through `useIsSSR` — formatter output that diverges across server/client (timezone-dependent dates) renders the server-stable form first; SSR identity rides RAC's own infra.
- Law: react-aria's memoized hooks are the first choice — `useDateFormatter`/`useNumberFormatter`/`useListFormatter`/`useCollator` already cache per locale+options; this page constructs a native instance only where no hook exists, and that construction rides `[3]`'s one cache.
- Boundary: message folds take the locale as a parameter from `useLocale` at the consuming row; the provider composition point is the app shell.

## [3]-[NATIVE_CACHE]

[NATIVE_CACHE]:
- Owner: `_native(kind, locale)` — the ONE per-locale instance cache behind every formatter this page constructs itself: a constructor row table (`plural` → `Intl.PluralRules`, `relative` → `Intl.RelativeTimeFormat`) and one interior `Map` keyed `kind:locale`; the plural fold and the relative-time fold both read through it, so the folder holds exactly one platform-formatter cache — the split per-concern caches are the collapsed defect.
- Law: the cache is the module's platform-formatter FFI seam — a JS `Map` holding `Intl` instances only, never domain values; its two-line mutation is the seam's sanctioned statement, and the cache is invisible to consumers.
- Law: the correlated return is the seam's marked cast — the heterogeneous instance map erases the per-kind type, and the `as` re-assertion is exactly the row the constructor table proves; this card carries the exemption, and the assertion is legal nowhere else in the module.
- Growth: a new hook-less formatter family (`Intl.Segmenter`, `Intl.DisplayNames`) is one constructor row — never a second cache.

```typescript
import { Refined } from "@rasm/ts/core"
import { Array, DateTime, Duration, HashMap, Match, Number, Option, Order, Schema, pipe } from "effect"

type Locale = Refined.Locale

const _NATIVE = {
  plural: (locale: string) => new Intl.PluralRules(locale),
  relative: (locale: string) => new Intl.RelativeTimeFormat(locale, { numeric: "auto", style: "long" }),
} as const

const _held = new Map<string, Intl.PluralRules | Intl.RelativeTimeFormat>()

const _native = <K extends keyof typeof _NATIVE>(kind: K, locale: string): ReturnType<(typeof _NATIVE)[K]> => {
  const key = `${kind}:${locale}`
  const instance = _held.get(key) ?? _NATIVE[kind](locale)
  _held.set(key, instance)
  return instance as ReturnType<(typeof _NATIVE)[K]>
}
```

## [4]-[FORMAT_ROWS]

[FORMAT_ROWS]:
- Owner: `Format` — the option-row vocabulary: `Format.date` (the closed date/time style rows — `stamp`, `long`, `time`, `weekday`), `Format.number` (`plain`, `compact`, `percent`, `bytes` unit rows), `Format.list` (`and`/`or` conjunction rows), `Format.relative` (the `Intl.RelativeTimeFormat` granularity ladder with its threshold table — seconds through years — folded from a `Duration` delta through `[3]`'s cache), the collation lift `Format.collate`, and the two ingress seams: `Format.instant(utc)` converting an effect `DateTime.Utc` to the native `Date` a formatter consumes (the ONE epoch crossing), `Format.span(duration)` projecting a `Duration` onto the relative-time ladder.
- Packages: `react-aria` (`useDateFormatter`, `useNumberFormatter`, `useListFormatter`, `useCollator`, `useFilter` — memoized native instances over the provider locale); `effect` (`DateTime`, `Duration`, `Array`, `Number`, `Option`, `Order` — the interior scalar owners).
- Entry: a row consumes `useDateFormatter(Format.date.stamp)` — hook plus row, never an inline options literal; a new presentation is one row on the owning table, and every consumer of that concern re-renders consistently.
- Law: the epoch crossing is single and marked — `DateTime.toEpochMillis` feeds `new Date(millis)` inside `Format.instant` only; a `new Date()` or epoch arithmetic anywhere else in the folder is the named defect, and the interior never sees a native `Date`.
- Law: relative time is a threshold fold, not a branch ladder in consumers — `Format.span` walks the granularity table (an ordered `as const` tuple of `[floor-millis, unit]` rows) and returns the `[value, unit]` pair `Intl.RelativeTimeFormat` consumes; the table is the only edit site for granularity policy.
- Law: collation lifts, never compares inline — `useCollator(options)` yields the memoized `Intl.Collator`, `Number.sign` folds its numeric verdict into the `Ordering` an `Order` demands cast-free, and `Order.mapInput` lifts the instance onto any domain projection so every sorted collection (table columns, picker options) shares one collation policy; a `localeCompare` call inline or an ASCII sort over user-facing strings is the named defect.
- Law: picker matching is `useFilter` — `contains`/`startsWith`/`endsWith` with locale-correct case folding feed the RAC `ComboBox`/`Autocomplete` matcher and the `cmdk` scorer's pre-filter where locale sensitivity matters; a `toLowerCase().includes()` ladder restates it.
- Law: a `dimension`-carrying magnitude renders as a projection over the SI value — a `Feed.Document` band column formats through `Format.number` rows against the `Dimension` vector (`view/table` consumes this at its band fold), never a `{value, unit}` re-decode.
- Boundary: `DateTime`/`Duration` arithmetic is settled kernel-scalar law; wire timestamps decode to `DateTime.Utc` at the interchange plane and arrive here already-typed; sorted collections and pickers live at `view/table`/`view/overlay`.
- Growth: a new unit family (area, currency) is one `Format.number` row; a new date presentation is one `Format.date` row; a new granularity is one ladder row.

```typescript
const _date = {
  stamp: { dateStyle: "medium", timeStyle: "short" },
  long: { dateStyle: "full" },
  time: { timeStyle: "medium" },
  weekday: { weekday: "long", day: "numeric", month: "short" },
} as const satisfies Record<string, Intl.DateTimeFormatOptions>

const _number = {
  plain: { maximumFractionDigits: 2 },
  compact: { notation: "compact", maximumFractionDigits: 1 },
  percent: { style: "percent", maximumFractionDigits: 1 },
  bytes: { style: "unit", unit: "byte", notation: "compact", unitDisplay: "narrow" },
} as const satisfies Record<string, Intl.NumberFormatOptions>

const _list = {
  and: { type: "conjunction", style: "long" },
  or: { type: "disjunction", style: "short" },
} as const satisfies Record<string, Intl.ListFormatOptions>

const _ladder = [
  [Duration.toMillis(Duration.days(365)), "year"],
  [Duration.toMillis(Duration.days(30)), "month"],
  [Duration.toMillis(Duration.days(7)), "week"],
  [Duration.toMillis(Duration.days(1)), "day"],
  [Duration.toMillis(Duration.hours(1)), "hour"],
  [Duration.toMillis(Duration.minutes(1)), "minute"],
  [0, "second"],
] as const satisfies ReadonlyArray<readonly [number, Intl.RelativeTimeFormatUnit]>

const _collate = (collator: Intl.Collator) =>
  <A>(project: (row: A) => string): Order.Order<A> =>
    Order.mapInput(
      Order.make((left: string, right: string) => Number.sign(collator.compare(left, right))),
      project,
    )

declare namespace Format {
  type DateRow = keyof typeof _date
  type NumberRow = keyof typeof _number
  type ListRow = keyof typeof _list
  type Span = readonly [value: number, unit: Intl.RelativeTimeFormatUnit]
}

const Format: {
  readonly date: typeof _date
  readonly number: typeof _number
  readonly list: typeof _list
  readonly collate: typeof _collate
  readonly instant: (utc: DateTime.Utc) => Date
  readonly span: (delta: Duration.Duration) => Format.Span
  readonly relative: (locale: Locale, delta: Duration.Duration) => string
} = {
  date: _date,
  number: _number,
  list: _list,
  collate: _collate,
  instant: (utc) => new Date(DateTime.toEpochMillis(utc)),
  span: (delta) => {
    const millis = Duration.toMillis(delta)
    const magnitude = Math.abs(millis)
    const row = Option.getOrElse(
      Array.findFirst(_ladder, ([floor]) => floor > 0 && magnitude >= floor),
      () => _ladder[6],
    )
    const divisor = row[0] === 0 ? Duration.toMillis(Duration.seconds(1)) : row[0]
    return [Math.round(millis / divisor), row[1]] as const
  },
  relative: (locale, delta) => {
    const [value, unit] = Format.span(delta)
    return _native("relative", locale).format(value, unit)
  },
}
```

## [5]-[MESSAGE_FAMILY]

[MESSAGE_FAMILY]:
- Owner: `Message` — the assembled owner whose `Catalog` static is the decode surface: a catalog is `Record<key, MessageSpec>` where `MessageSpec` is one `Schema.Union` of three tagged cases — `Text { value }`, `Plural { arg, forms }` (forms keyed by the closed `Intl.LDMLPluralRule` vocabulary with `other` mandatory), `Select { arg, cases, other }` — decoded ONCE at catalog admission (an app fetch, a bundled JSON module crossing `with { type: "json" }` ingress), never re-validated per format call.
- Packages: `effect` (`Schema.Record`, `Schema.TaggedStruct`, `Schema.Literal`, `HashMap`, `Match`, `Option`); `@rasm/ts/core` (`Refined.Locale` — the `Refined` vocabulary owner carries it; no bare `Locale` export exists) as the catalog key.
- Law: the case family is closed — a new message SHAPE (a range case, an ordinal plural) is one tagged case plus one fold arm here, breaking every consumer loudly; a new message INSTANCE is catalog data and touches no code.
- Law: plural forms carry the CLDR category vocabulary (`zero`/`one`/`two`/`few`/`many`/`other`) with only `other` required — sparse forms are legal because the fold falls through to `other`, mirroring CLDR semantics instead of demanding six strings per language.
- Law: interpolation slots are `{name}` spellings inside form strings — bounded, positional-free, and typed: the args record is `Record<string, string | number>`, and an unreferenced slot is inert rather than an error, so catalogs evolve ahead of call sites.
- Boundary: HTML-bearing message VALUES pass `system/primitive#SANITIZE_GATE` before any `dangerouslySetInnerHTML` sink — this module emits strings only; number/date formatting inside arguments is `[4]`'s (format the value first, pass the string).
- Growth: a locale is one catalog file; a message is one catalog row; a vocabulary case is one union member + one fold arm.

```typescript
const _categories = ["zero", "one", "two", "few", "many", "other"] as const

const _Text = Schema.TaggedStruct("Text", {
  value: Schema.NonEmptyString,
})

const _Plural = Schema.TaggedStruct("Plural", {
  arg: Schema.NonEmptyString,
  forms: Schema.Struct(
    { other: Schema.NonEmptyString },
    { key: Schema.Literal(..._categories), value: Schema.NonEmptyString },
  ),
})

const _Select = Schema.TaggedStruct("Select", {
  arg: Schema.NonEmptyString,
  cases: Schema.Record({ key: Schema.NonEmptyString, value: Schema.NonEmptyString }),
  other: Schema.NonEmptyString,
})

const _Spec = Schema.Union(_Text, _Plural, _Select)

const _Catalog = Schema.Record({ key: Schema.NonEmptyString, value: _Spec })

declare namespace Message {
  type Spec = Schema.Schema.Type<typeof _Spec>
  type Catalog = Schema.Schema.Type<typeof _Catalog>
  type Category = (typeof _categories)[number]
  type Args = Readonly<Record<string, string | number>>
  type Book = HashMap.HashMap<Locale, Catalog>
}
```

## [6]-[MESSAGE_FOLD]

[MESSAGE_FOLD]:
- Owner: `Message.format(book, locale, fallback, key, args?)` — the one total fold: resolve the catalog through the fallback chain, dispatch the spec's tag (`Match.valueTags` — the one-shot record dispatch over a held union value), select the plural category through `[3]`'s cached `Intl.PluralRules`, select the case string with `other` as the floor, and interpolate `{slot}` spellings from the args record; the return is always a string — a missing key yields the key itself, the deliberate visible-evidence policy, never a throw and never an empty string.
- Law: plural argument coercion is exact — the `arg` slot must resolve to a number for category selection; a non-numeric plural argument selects `other`, keeping the fold total instead of minting a fault channel for author-time errors the catalog decode already polices.
- Law: the fold is pure given its inputs — no ambient locale read; the locale arrives as a parameter (from `useLocale` at the consuming row), so the same catalog+key formats identically on server and client.
- Law: the chain never crosses scripts silently — fallback strips subtags right-to-left (`de-CH` → `de`, re-minted through `Refined.Locale`'s own decode so an invalid truncation folds to none), the standard BCP-47 truncation; a cross-language fallback (`fr` for a missing `de`) is only the DEFAULT hop the app configured, never an inference.
- Law: catalog assembly is app composition — this module never fetches; the app decodes catalog payloads through `Message.Catalog` at its own ingress and hands the built `Book`; hot locale swap is one atom write of the locale brand, and every message re-renders through the same fold.

```typescript
const _fill = (template: string, args: Message.Args): string =>
  template.replaceAll(/\{(\w+)\}/g, (whole, slot: string) => {
    const value = args[slot]
    return value === undefined ? whole : String(value)
  })

const _render = (locale: Locale, spec: Message.Spec, args: Message.Args): string =>
  Match.valueTags(spec, {
    Text: ({ value }) => _fill(value, args),
    Plural: ({ arg, forms }) => {
      const count = args[arg]
      const category = typeof count === "number" ? _native("plural", locale).select(count) : "other"
      return _fill(forms[category] ?? forms.other, args)
    },
    Select: ({ arg, cases, other }) => {
      const chosen = args[arg]
      return _fill((typeof chosen === "string" ? cases[chosen] : undefined) ?? other, args)
    },
  })

const _base = (locale: Locale): Option.Option<Locale> =>
  pipe(
    locale.split("-"),
    (subtags) =>
      subtags.length < 2
        ? Option.none()
        : Schema.decodeOption(Refined.Locale)(subtags.slice(0, -1).join("-")),
  )

const _chain = (locale: Locale): ReadonlyArray<Locale> =>
  Array.unfold(Option.some(locale), Option.map((current) => [current, _base(current)] as const))

const _resolve = (book: Message.Book, locale: Locale, fallback: Locale, key: string): Option.Option<Message.Spec> =>
  Option.firstSomeOf(
    Array.map(Array.append(_chain(locale), fallback), (hop) =>
      Option.flatMap(HashMap.get(book, hop), (catalog) => Option.fromNullable(catalog[key]))),
  )

const Message: {
  readonly Catalog: typeof _Catalog
  readonly Spec: typeof _Spec
  readonly categories: typeof _categories
  readonly format: (book: Message.Book, locale: Locale, fallback: Locale, key: string, args?: Message.Args) => string
} = {
  Catalog: _Catalog,
  Spec: _Spec,
  categories: _categories,
  format: (book, locale, fallback, key, args = {}) =>
    Option.match(_resolve(book, locale, fallback, key), {
      onNone: () => key,
      onSome: (spec) => _render(locale, spec, args),
    }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Format, Message }
```
