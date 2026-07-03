# [UI_MESSAGE]

`intl/message.ts` is the localization plane's message authority with zero i18n package: catalogs are APP DATA — `Schema`-decoded records keyed by the kernel BCP-47 `Locale` brand, each entry a closed `MessageSpec` family (`Text`, `Plural`, `Select`) — and the fold is total: plural category selection rides native `Intl.PluralRules`, select rides case lookup with a mandatory `other` arm, argument slots interpolate from a typed record, and a missing key resolves through the locale fallback chain to the key itself as visible evidence. No ICU engine, no translation runtime, no `.po` toolchain — the message vocabulary is exactly the three cases, and an app grows locales by shipping catalog data, never by editing this module.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         |
| :-----: | :--------------- | :------------------------------------------------------------------------------ |
|   [1]   | `CATALOG_SCHEMA` | the `MessageSpec` closed family and the `Catalog` decode owner                   |
|   [2]   | `FORMAT_FOLD`    | the total `Message.format` fold — plural/select/interpolation over native `Intl` |
|   [3]   | `LOCALE_CHAIN`   | the per-locale catalog map and the fallback-chain resolution law                 |

## [2]-[CATALOG_SCHEMA]

- Owner: `Message` — the assembled owner whose `Catalog` static is the decode surface: a catalog is `Record<key, MessageSpec>` where `MessageSpec` is one `Schema.Union` of three tagged cases — `Text { value }`, `Plural { arg, forms }` (forms keyed by the closed `Intl.LDMLPluralRule` vocabulary with `other` mandatory), `Select { arg, cases, other }` — decoded ONCE at catalog admission (an app fetch, a bundled JSON module crossing `with { type: "json" }` ingress), never re-validated per format call.
- Packages: `effect` `Schema` (the catalog codec — `Schema.Record`, `Schema.TaggedStruct`, `Schema.Literal`), the kernel `Refined.Locale` brand (`@rasm/ts/kernel` — the `Refined` vocabulary owner carries it; no bare `Locale` export exists) as the catalog key.
- Law: the case family is closed — a new message SHAPE (a range case, an ordinal plural) is one tagged case plus one fold arm here, breaking every consumer loudly; a new message INSTANCE is catalog data and touches no code.
- Law: plural forms carry the CLDR category vocabulary (`zero`/`one`/`two`/`few`/`many`/`other`) with only `other` required — sparse forms are legal because the fold falls through to `other`, mirroring CLDR semantics instead of demanding six strings per language.
- Law: interpolation slots are `{name}` spellings inside form strings — bounded, positional-free, and typed: the args record is `Record<string, string | number>`, and an unreferenced slot is inert rather than an error, so catalogs evolve ahead of call sites.
- Boundary: HTML-bearing message VALUES pass the `view/compose` sanitize gate before any `dangerouslySetInnerHTML` sink — this module emits strings only; number/date formatting inside arguments is `intl/format`'s (format the value first, pass the string).
- Growth: a locale is one catalog file; a message is one catalog row; a vocabulary case is one union member + one fold arm.

```typescript
import { Refined } from "@rasm/ts/kernel"
import { HashMap, Option, Schema } from "effect"

type Locale = Refined.Locale

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

## [3]-[FORMAT_FOLD]

- Owner: `Message.format(book, locale, fallback, key, args?)` — the one total fold: resolve the catalog through the fallback chain, dispatch the spec's tag (`Match.valueTags` — the one-shot record dispatch over a held union value), select the plural category through a cached `Intl.PluralRules` instance keyed by locale, select the case string with `other` as the floor, and interpolate `{slot}` spellings from the args record; the return is always a string — a missing key yields the key itself, the deliberate visible-evidence policy, never a throw and never an empty string.
- Law: `Intl.PluralRules` instances are cached per locale in an interior memo — the cache is the module's platform-formatter FFI seam: a JS `Map` keyed by the brand string holding `Intl` instances only, never domain values, and its two-line mutation is the seam's sanctioned statement; instantiation is the expensive step, selection is cheap, and the cache is invisible to consumers.
- Law: plural argument coercion is exact — the `arg` slot must resolve to a number for category selection; a non-numeric plural argument selects `other`, keeping the fold total instead of minting a fault channel for author-time errors the catalog decode already polices.
- Law: the fold is pure given its inputs — no ambient locale read; the locale arrives as a parameter (from `intl/format`'s `useLocale` at the consuming row), so the same catalog+key formats identically on server and client.

```typescript
import { Match, Option, pipe } from "effect"

const _rules = new Map<string, Intl.PluralRules>()

const _plural = (locale: Locale, count: number): Message.Category => {
  const held = _rules.get(locale) ?? new Intl.PluralRules(locale)
  _rules.set(locale, held)
  return held.select(count)
}

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
      const category = typeof count === "number" ? _plural(locale, count) : "other"
      return _fill(forms[category] ?? forms.other, args)
    },
    Select: ({ arg, cases, other }) => {
      const chosen = args[arg]
      return _fill((typeof chosen === "string" ? cases[chosen] : undefined) ?? other, args)
    },
  })
```

## [4]-[LOCALE_CHAIN]

- Owner: the `Book` resolution — catalogs live in one `HashMap<Locale, Catalog>` the app assembles at boot; `Message.format` resolves key lookup through the fallback chain: exact locale → base language (`_base` strips the trailing subtag and re-mints through `Refined.Locale`'s own decode, so an invalid truncation folds to none) → the app's default locale → the key itself. The chain is a fold over `Option.orElse`, and every hop is a plain map read.
- Law: the chain never crosses scripts silently — fallback strips subtags right-to-left (`de-CH` → `de`), the standard BCP-47 truncation; a cross-language fallback (`fr` for a missing `de`) is only the DEFAULT hop the app configured, never an inference.
- Law: catalog assembly is app composition — this module never fetches; the app decodes catalog payloads through `Message.Catalog` at its own ingress and hands the built `Book`; hot locale swap is one atom write of the locale brand (`intl/format`'s provider row), and every message re-renders through the same fold.

```typescript
const _base = (locale: Locale): Option.Option<Locale> =>
  pipe(
    locale.split("-"),
    (subtags) =>
      subtags.length < 2
        ? Option.none()
        : Schema.decodeOption(Refined.Locale)(subtags.slice(0, -1).join("-")),
  )

const _resolve = (book: Message.Book, locale: Locale, fallback: Locale, key: string): Option.Option<Message.Spec> =>
  pipe(
    HashMap.get(book, locale),
    Option.flatMap((catalog) => Option.fromNullable(catalog[key])),
    Option.orElse(() =>
      pipe(
        _base(locale),
        Option.flatMap((base) => HashMap.get(book, base)),
        Option.flatMap((catalog) => Option.fromNullable(catalog[key])),
      )),
    Option.orElse(() =>
      pipe(
        HashMap.get(book, fallback),
        Option.flatMap((catalog) => Option.fromNullable(catalog[key])),
      )),
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

export { Message }
```
