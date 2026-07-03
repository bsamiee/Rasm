# [UI_THEME]

`token/theme.ts` is the color-token authority and the class rail of the whole folder: OKLCH palettes are computed in `colorjs.io` (perceptually-even ramps, gamut-fit, contrast-gated at decode), emitted as Tailwind v4 `@theme` namespace rows through one CSS fold, and consumed by every component through the single `cn` composer — `extendTailwindMerge` taught every custom token group, over the `clsx` fold. Theme selection is a `data-theme` attribute the `@custom-variant` selectors read; no component branches on theme in JS, hardcodes a color, or imports `clsx`/`twMerge` beside the one rail. The same decoded color object feeds two sinks — the CSS custom-property plane here and the `viewer` render space at `scene/appearance` — so token color and rendered color are one color-space artifact.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                          |
| :-----: | :---------------- | :------------------------------------------------------------------------------ |
|   [1]   | `COLOR_AUTHORITY` | the `Theme.Color` decode brand, ramp/contrast algebra, and the `@theme` CSS fold |
|   [2]   | `CLASS_RAIL`      | the one `cn` composer — `extendTailwindMerge` over `clsx`, group table as data   |
|   [3]   | `THEME_SWITCH`    | the theme vocabulary, the `data-theme` stamp seam, and the persisted-theme law   |

## [2]-[COLOR_AUTHORITY]

- Owner: `Theme` — one assembled owner: the `Color` transform (CSS color string ⇄ `PlainColorObject`, non-throwing via `tryColor`, `ParseError` on a malformed token), the `pair` refinement factory (foreground/background pairs APCA-gated at decode, floor a policy row), the `ramp` fold (`steps` in OKLCH, ΔE-bounded, gamut-fit through `toGamutCSS`), the `linear` projection (the render-space triple `scene/appearance` ingests), and the `css` emission fold turning any token row table into `@theme` declaration text.
- Packages: `colorjs.io/fn` + `colorjs.io/spaces` (browser lane — `sRGB`/`sRGB_Linear`/`P3`/`OKLCH`/`OKLab` registered explicitly, never the full registry), `effect` `Schema`; `tailwindcss` is the emission sink — one `@theme` line per token, each generating its variable and utility family.
- Entry: `Theme.ramp(base, target, count)` is the one palette generator — a hand-listed hex table is the named defect; `Theme.css(namespace, rows)` is the one emission fold every token plane reuses (`token/scale` calls it for its own namespaces).
- Law: contrast is structural, never disciplinary — a `Theme.pair` that fails its APCA floor rejects at decode carrying the floor in the refusal; no component re-checks contrast at render.
- Law: the decoded interior is `PlainColorObject`, the encoded wire shape the `oklch(...)` string; `serialize` emits `inGamut: true`, and gamut fit is `toGamutCSS` (CSS Color 4 OKLCH chroma reduction) selected once here.
- Boundary: the `to("srgb-linear")` conversion feeding three `ColorManagement` is consumed by `viewer/scene/appearance` through `Theme.linear`; the motion/spacing namespaces are `token/scale` rows over this page's `css` fold; the `@import "tailwindcss"` entry stylesheet and `@custom-variant` declarations are app stylesheet data, not module code.
- Growth: a new hue is one `ramp` call emitting one namespace row set; a new contrast tier is one `_APCA` row — never a second color engine or a per-component color literal.

```typescript
import { ColorSpace, contrastAPCA, type PlainColorObject, serialize, steps, to, toGamutCSS, tryColor } from "colorjs.io/fn"
import { OKLCH, OKLab, P3, sRGB, sRGB_Linear } from "colorjs.io/spaces"
import { Array, Option, ParseResult, Record, Schema } from "effect"

ColorSpace.register(sRGB)
ColorSpace.register(sRGB_Linear)
ColorSpace.register(P3)
ColorSpace.register(OKLCH)
ColorSpace.register(OKLab)

const _APCA = { body: 75, large: 60, muted: 45 } as const

const _Plain = Schema.declare(
  (input: unknown): input is PlainColorObject =>
    typeof input === "object" && input !== null && "space" in input && "coords" in input,
  { identifier: "PlainColor" },
)

const _Color = Schema.transformOrFail(Schema.NonEmptyString, _Plain, {
  strict: true,
  decode: (raw, _, ast) => {
    const parsed = tryColor(raw)
    return parsed === null
      ? ParseResult.fail(new ParseResult.Type(ast, raw, "<unparseable-color>"))
      : ParseResult.succeed(toGamutCSS(parsed, { space: sRGB }))
  },
  encode: (plain) => ParseResult.succeed(serialize(plain, { inGamut: true })),
})

const _pair = (floor: keyof typeof _APCA) =>
  Schema.Struct({ fg: _Color, bg: _Color }).pipe(
    Schema.filter((duo) => Math.abs(contrastAPCA(duo.bg, duo.fg)) >= _APCA[floor] || `<apca-below-${floor}>`),
  )

const _ramp = (base: string, target: string, count: number): Option.Option<ReadonlyArray<string>> =>
  Option.map(
    Option.all([Option.fromNullable(tryColor(base)), Option.fromNullable(tryColor(target))]),
    ([from, into]) =>
      Array.map(
        steps(from, into, { space: "oklch", outputSpace: "oklch", steps: count, maxDeltaE: 3, hue: "shorter" }),
        (stop) => serialize(toGamutCSS(stop, { space: sRGB }), { inGamut: true }),
      ),
  )

const _linear = (color: Schema.Schema.Type<typeof _Color>): readonly [number, number, number] => {
  const converted = to(color, "srgb-linear")
  return [converted.coords[0] ?? 0, converted.coords[1] ?? 0, converted.coords[2] ?? 0] as const
}

const _css = (namespace: string, rows: Record.ReadonlyRecord<string, string>): string =>
  `@theme {\n${Record.collect(rows, (key, value) => `  --${namespace}${key === "" ? "" : `-${key}`}: ${value};`).join("\n")}\n}`
```

## [3]-[CLASS_RAIL]

- Owner: `cn` — the folder's ONE class composer: `clsx` folds conditional inputs, one `extendTailwindMerge` instance resolves last-wins conflicts, and the extension table teaches it every custom group — the project `@theme` color scale and the `tw-animate-css` motion groups (`fade`/`zoom`/`spin`/`blur`/`slide` setters, `animation-duration`/`delay`/`repeat` modifiers) — so a `cva` variant, a `tailwindcss-react-aria-components` state variant, and a caller override all collapse to the intended winner.
- Packages: `tailwind-merge` (`extendTailwindMerge`, `validators`; `fromTheme` where a custom group must reference a whole scale), `clsx` (`ClassValue` — the shared input vocabulary of the whole styling rail); `class-variance-authority` composes downstream (its `cx` IS `clsx`, so a `cva` module imports `cn` from here, never a second composer).
- Law: the theme extension is data over the default scales — the project hues (`mauve`/`olive`/`mist`/`taupe`) extend the `color` theme scale so every default color group (`bg-`/`text-`/`ring-`/…) resolves them; `fromTheme` inside a theme scale is circular and is the named defect.
- Law: exactly one merge instance exists — a raw `twMerge` import or a per-component `extendTailwindMerge` silently mis-resolves custom utilities and is the named defect; `twJoin` is admitted only for provably conflict-free static token strings.
- Law: the group table is data — a new custom utility family is one `classGroups` row over `validators.*` predicates or a `fromTheme` scale reference, never a parser change; `token/scale` contributes its motion rows to this one table at authoring time and never mints a sibling instance.
- Law: `cn` is pure synchronous string work below the Effect boundary — it runs inside render, memoized by `tailwind-merge`'s LRU, and never lifts onto a rail.

```typescript
import { type ClassValue, clsx } from "clsx"
import { extendTailwindMerge, validators } from "tailwind-merge"

const _motion = (stem: string) => ({ [stem]: ["", validators.isNumber, validators.isArbitraryValue] })

const _merge = extendTailwindMerge({
  extend: {
    theme: { color: ["mauve", "olive", "mist", "taupe"] },
    classGroups: {
      "animate-trigger": ["animate-in", "animate-out", "animate-none"],
      "motion-fade": [_motion("fade-in"), _motion("fade-out")],
      "motion-zoom": [_motion("zoom-in"), _motion("zoom-out")],
      "motion-spin": [_motion("spin-in"), _motion("spin-out")],
      "motion-blur": [_motion("blur-in"), _motion("blur-out")],
      "motion-slide": [
        _motion("slide-in-from-top"),
        _motion("slide-in-from-bottom"),
        _motion("slide-in-from-left"),
        _motion("slide-in-from-right"),
        _motion("slide-out-to-top"),
        _motion("slide-out-to-bottom"),
        _motion("slide-out-to-left"),
        _motion("slide-out-to-right"),
      ],
      "motion-duration": [{ "animation-duration": [validators.isNumber, validators.isArbitraryValue] }],
      "motion-delay": [{ delay: [validators.isNumber, validators.isArbitraryValue] }],
      "motion-repeat": [{ repeat: ["0", "1", "infinite"] }],
    },
  },
})

const cn = (...inputs: ReadonlyArray<ClassValue>): string => _merge(clsx(inputs))
```

## [4]-[THEME_SWITCH]

- Owner: the theme vocabulary and its stamp seam riding `Theme`: `Theme.kinds` (the closed `as const` tuple — `light`, `dark`, `system`), `Theme.Kind` derived from it, and `Theme.stamp(kind)` — the one `documentElement.dataset` write, an `Effect.sync` boundary row resolving `system` through the `prefers-color-scheme` media query at stamp time.
- Law: theme is CSS-selected, never JS-branched — `@custom-variant dark (&:where([data-theme=dark] *))` in the token stylesheet reads the stamped attribute; a component styles with `dark:` variants through `cn`, and a `kind === "dark"` conditional in render is the named defect.
- Law: persistence rides the one binding — the theme atom is `Atom.kvs` with `Schema.Literal(...Theme.kinds)` as codec (`atom/binding`'s persisted row), and a `useAtomSubscribe` on it runs `Theme.stamp` so the attribute tracks the store without re-render.
- Boundary: the media-query read and the dataset write are this page's platform-forced seam; the atom mechanics are `atom/binding`'s; the `@custom-variant` declaration is stylesheet data beside `@plugin "tailwindcss-react-aria-components"`.

```typescript
import { Effect } from "effect"

const _kinds = ["light", "dark", "system"] as const

const _resolved = (kind: (typeof _kinds)[number]): "light" | "dark" =>
  kind === "system"
    ? globalThis.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light"
    : kind

const _stamp = (kind: (typeof _kinds)[number]): Effect.Effect<void> =>
  Effect.sync(() => {
    globalThis.document.documentElement.dataset["theme"] = _resolved(kind)
  })

declare namespace Theme {
  type Color = Schema.Schema.Type<typeof _Color>
  type Pair = { readonly fg: Color; readonly bg: Color }
  type Rows = Record.ReadonlyRecord<string, string>
  type Kind = (typeof _kinds)[number]
}

const Theme: {
  readonly Color: typeof _Color
  readonly kinds: typeof _kinds
  readonly pair: typeof _pair
  readonly ramp: typeof _ramp
  readonly linear: typeof _linear
  readonly css: typeof _css
  readonly stamp: typeof _stamp
} = {
  Color: _Color,
  kinds: _kinds,
  pair: _pair,
  ramp: _ramp,
  linear: _linear,
  css: _css,
  stamp: _stamp,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { cn, Theme }
```
