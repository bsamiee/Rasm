# [UI_FORMAT]

`intl/format.ts` is the value-formatting plane: react-aria's `I18nProvider` carries the kernel `Locale` brand as the one ambient locale, `useLocale` reads it, and the formatter hooks (`useDateFormatter`, `useNumberFormatter`, `useListFormatter`, `useCollator`) return memoized native `Intl` instances scoped to it — this module owns the POLICY between them: one `as const` option-row table per format concern (date styles, number notations, list styles, relative-time granularity), the kernel-scalar ingress seam (`DateTime`/`Duration` values cross to native formatter inputs exactly once), and the collation/filter instances pickers consume. No `Intl.*` constructor at a call site, no wall-clock `Date` in domain flow, no format options object authored inline.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                        |
| :-----: | :--------------- | :------------------------------------------------------------------------------ |
|   [1]   | `PROVIDER_ROOT`  | the `I18nProvider`/`useLocale` locale spine over the kernel `Locale` brand       |
|   [2]   | `FORMAT_ROWS`    | `Format` — the option-row vocabulary and the kernel-scalar ingress seam          |
|   [3]   | `COLLATE_FILTER` | collation `Order` instances and the locale-aware picker filter                   |

## [2]-[PROVIDER_ROOT]

- Law: the locale is one ambient value — the app root renders `I18nProvider` with the kernel `Locale` brand (a BCP-47 string by construction, so it feeds the provider directly), the brand itself lives in a persisted atom (`atom/binding`'s `Store.kvs` row with the `Locale` schema as codec), and a locale change is one atom write that re-renders every formatter consumer; a second locale source, a `navigator.language` read in a row, or a prop-drilled locale string is the named defect.
- Law: direction derives — RAC and the react-aria hooks resolve RTL from the provided locale; layout rows consume logical properties (`start`/`end` utilities through `cn`) so direction never branches in JS.
- Law: hydration-sensitive reads gate through `useIsSSR` — formatter output that could diverge across server/client (timezone-dependent dates) renders the server-stable form first; `SSRProvider` semantics ride RAC's own infra.
- Boundary: message-catalog folds take the locale as a parameter from `useLocale` (`intl/message`); the provider composition point is the app shell.

## [3]-[FORMAT_ROWS]

- Owner: `Format` — the option-row vocabulary: `Format.date` (the closed date/time style rows — `stamp`, `long`, `time`, `weekday`), `Format.number` (`plain`, `compact`, `percent`, `bytes` unit rows), `Format.list` (`and`/`or` conjunction rows), `Format.relative` (the `Intl.RelativeTimeFormat` granularity ladder with its threshold table — seconds through years — folded from a `Duration` delta), and the two ingress seams: `Format.instant(utc)` converting an effect `DateTime.Utc` to the native `Date` a formatter consumes (the ONE epoch crossing), `Format.span(duration)` projecting a `Duration` onto the relative-time ladder.
- Packages: `react-aria` (`useDateFormatter`, `useNumberFormatter`, `useListFormatter` — memoized `Intl.DateTimeFormat`/`Intl.NumberFormat`/`Intl.ListFormat` over the provider locale), `effect` (`DateTime`, `Duration` — the interior scalar owners).
- Entry: a row consumes `useDateFormatter(Format.date.stamp)` — hook plus row, never an inline options literal; a new presentation is one row on the owning table, and every consumer of that concern re-renders consistently.
- Law: the epoch crossing is single and marked — `DateTime.toEpochMillis` feeds `new Date(millis)` inside `Format.instant` only; a `new Date()` or epoch arithmetic anywhere else in the folder is the named defect, and the interior never sees a native `Date`.
- Law: relative time is a threshold fold, not a branch ladder in consumers — `Format.span` walks the granularity table (an ordered `as const` tuple of `[floor-millis, unit]` rows) and returns the `[value, unit]` pair `Intl.RelativeTimeFormat` consumes; the table is the only edit site for granularity policy.
- Law: `Intl.RelativeTimeFormat` has no react-aria hook — the instance memoizes in an interior per-locale cache exactly like `intl/message`'s `PluralRules` cache; the cache is invisible and the consumer surface stays `Format.relative`.
- Boundary: `DateTime`/`Duration` arithmetic is settled kernel-scalar law; wire timestamps decode to `DateTime.Utc` at `wire` and arrive here already-typed; panel stamp rendering (`viewer/panel/binding`) consumes `Format.instant` + `useDateFormatter`.
- Growth: a new unit family (area, currency) is one `Format.number` row; a new date presentation is one `Format.date` row; a new granularity is one ladder row.

```typescript
import { DateTime, Duration } from "effect"

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

const _rtfs = new Map<string, Intl.RelativeTimeFormat>()

const _rtf = (locale: string): Intl.RelativeTimeFormat => {
  const held = _rtfs.get(locale) ?? new Intl.RelativeTimeFormat(locale, { numeric: "auto", style: "long" })
  _rtfs.set(locale, held)
  return held
}

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
  readonly instant: (utc: DateTime.Utc) => Date
  readonly span: (delta: Duration.Duration) => Format.Span
  readonly relative: (locale: string, delta: Duration.Duration) => string
} = {
  date: _date,
  number: _number,
  list: _list,
  instant: (utc) => new Date(DateTime.toEpochMillis(utc)),
  span: (delta) => {
    const millis = Duration.toMillis(delta)
    const magnitude = Math.abs(millis)
    const row = _ladder.find(([floor]) => magnitude >= floor && floor > 0) ?? _ladder[6]
    const unit = row[1]
    const divisor = row[0] === 0 ? Duration.toMillis(Duration.seconds(1)) : row[0]
    return [Math.round(millis / divisor), unit] as const
  },
  relative: (locale, delta) => {
    const [value, unit] = Format.span(delta)
    return _rtf(locale).format(value, unit)
  },
}
```

## [4]-[COLLATE_FILTER]

- Law: locale-aware ordering is a composed `Order` instance — `useCollator(options)` yields the memoized `Intl.Collator`, and the instance lifts through `Order.mapInput` onto the domain projection so every sorted collection (table columns, picker options) shares one collation policy; a `localeCompare` call inline or an ASCII sort over user-facing strings is the named defect.
- Law: picker matching is `useFilter` — `contains`/`startsWith`/`endsWith` with locale-correct case folding feed the RAC `ComboBox`/`Autocomplete` matcher and the `cmdk` scorer's pre-filter where locale sensitivity matters; a `toLowerCase().includes()` ladder restates it.
- Boundary: the sorted collections and pickers live at `view/compose`; this cluster owns only the instance law.

```typescript
import { Order } from "effect"

declare namespace Collate {
  type Ranked = { readonly label: string }
}

const _byLabel = (collator: Intl.Collator): Order.Order<Collate.Ranked> =>
  Order.mapInput(
    (left: string, right: string) => collator.compare(left, right) as -1 | 0 | 1,
    (row: Collate.Ranked) => row.label,
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { Format }
```
