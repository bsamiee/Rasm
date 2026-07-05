# [UI_TOKEN]

The design-token authority: OKLCH color computed in `colorjs.io` — perceptually-even ramps, gamut-fit, APCA contrast-gated at decode — the dimension vocabulary (one `--spacing` multiplier, a modular type scale with paired line-heights, radius/easing/breakpoint rows), and the folder's one `cn` class rail (`extendTailwindMerge` taught every custom group, over the `clsx` fold), all emitted as Tailwind v4 `@theme` namespace rows through one CSS fold. Theme selection is a `data-theme` attribute the `@custom-variant` selectors read; no component branches on theme in JS, hardcodes a color, writes a raw pixel, or imports `clsx`/`twMerge` beside the one rail. The decoded color object feeds two sinks — the CSS custom-property plane here and the viewer render space through `Theme.linear` — so token color and rendered color are one color-space artifact. Motion class-row vocabulary lives at `system/act#MOTION_ROWS`; this page teaches the motion class GROUPS to the one merge table as data so those rows resolve conflicts deterministically. The module is `ui/src/system/token.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                           | [PUBLIC]      |
| :-----: | :---------------- | :-------------------------------------------------------------------------------- | :------------ |
|  [01]   | `COLOR_AUTHORITY` | the `Theme.Color` decode brand, ramp/contrast algebra, and the `@theme` CSS fold   | `Theme`       |
|  [02]   | `CLASS_RAIL`      | the one `cn` composer — `extendTailwindMerge` over `clsx`, group table as data     | `cn`          |
|  [03]   | `SCALE_TABLES`    | the spacing multiplier, text/radius/easing/breakpoint rows, and their emission     | `Scale`       |
|  [04]   | `THEME_SWITCH`    | the theme vocabulary, the `data-theme` stamp seam, and the persisted-theme law     | `Theme`       |

## [2]-[COLOR_AUTHORITY]

[COLOR_AUTHORITY]:
- Owner: `Theme` — one assembled owner: the `Color` transform (CSS color string ⇄ `PlainColorObject`, non-throwing via `tryColor`, `ParseError` on a malformed token), the `pair` refinement factory (foreground/background pairs APCA-gated at decode, floor a policy row), the `ramp` fold (`steps` in OKLCH, ΔE-bounded, gamut-fit through `toGamutCSS`), the `linear` projection (the render-space triple the viewer material plane ingests), and the `css` emission fold turning any token row table into `@theme` declaration text.
- Packages: `colorjs.io/fn` + `colorjs.io/spaces` (browser lane — `sRGB`/`sRGB_Linear`/`P3`/`OKLCH`/`OKLab` registered explicitly, never the full registry); `effect` (`Schema`, `ParseResult`, `Array`, `Option`, `Record`); `tailwindcss` is the emission sink — one `@theme` line per token, each generating its variable and utility family.
- Entry: `Theme.ramp(base, target, count)` is the one palette generator — a hand-listed hex table is the named defect; `Theme.css(namespace, rows)` is the one emission fold every token plane reuses (`Scale.css` folds through it; a viewer probe table emits through it).
- Law: contrast is structural, never disciplinary — a `Theme.pair` that fails its APCA floor rejects at decode carrying the floor in the refusal; no component re-checks contrast at render.
- Law: the decoded interior is `PlainColorObject`, the encoded wire shape the `oklch(...)` string; `serialize` emits `inGamut: true`, and gamut fit is `toGamutCSS` (CSS Color 4 OKLCH chroma reduction) selected once here.
- Boundary: the `to("srgb-linear")` conversion feeding three `ColorManagement` leaves through `Theme.linear`; the `@import "tailwindcss"` entry stylesheet and `@custom-variant` declarations are app stylesheet data, not module code.
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

[CLASS_RAIL]:
- Owner: `cn` — the folder's ONE class composer: `clsx` folds conditional inputs, one `extendTailwindMerge` instance resolves last-wins conflicts, and the extension table teaches it every custom group — the project `@theme` color scale and the `tw-animate-css` motion groups (`fade`/`zoom`/`spin`/`blur`/`slide` setters, `animation-duration`/`delay`/`repeat` modifiers) — so a `cva` variant, a `tailwindcss-react-aria-components` state variant, and a caller override all collapse to the intended winner.
- Packages: `tailwind-merge` (`extendTailwindMerge`, `validators`; `fromTheme` where a custom group references a whole scale); `clsx` (`ClassValue` — the shared input vocabulary of the whole styling rail); `class-variance-authority` composes downstream (its `cx` IS `clsx`, so a recipe module imports `cn` from here, never a second composer).
- Law: the theme extension is data over the default scales — the project hues (`mauve`/`olive`/`mist`/`taupe`) extend the `color` theme scale so every default color group (`bg-`/`text-`/`ring-`/…) resolves them; `fromTheme` inside a theme scale is circular and is the named defect.
- Law: exactly one merge instance exists — a raw `twMerge` import or a per-component `extendTailwindMerge` silently mis-resolves custom utilities and is the named defect; `twJoin` is admitted only for provably conflict-free static token strings.
- Law: the group table is data — a new custom utility family is one `classGroups` row over `validators.*` predicates or a `fromTheme` scale reference, never a parser change; `system/act` consumes these groups through its `Motion` row strings and never mints a sibling instance.
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

## [4]-[SCALE_TABLES]

[SCALE_TABLES]:
- Owner: `Scale` — one assembled owner over five interior anchors: `_spacing` (the single multiplier — a density change is this one token, never a scale rewrite), `_text` (step → `{ size, leading }` pairs, each emitting the `--text-*` + `--text-*--line-height` twin), `_radius`, `_ease` (easing curves as cubic-bezier rows — `system/act` motion and the overlay `useTransitionStyles` phases consume them), and `_breakpoint`; `Scale.css()` folds all of them through `Theme.css` into the `@theme` declarations the build stylesheet inlines.
- Packages: `tailwindcss` v4 namespaces are the sink (`--spacing`, `--text-*`, `--radius-*`, `--ease-*`, `--breakpoint-*` — one namespace row generates the variable and its utility family); `effect` (`Array`, `Record`) folds the emission.
- Law: the type scale is paired data — a text step without its line-height is half a token; the emission writes both variables from one row so `text-<step>` always carries its leading.
- Law: scale values are derivation, not enumeration — the text ladder derives from a ratio fold over the base size (a modular scale), so retuning typography is two numbers, never twelve edits.
- Law: no JS reads these values at runtime — the tables exist to emit CSS; a component consumes `p-4`/`text-lg`/`rounded-md` utilities through `cn`, and a runtime pixel computation over `_spacing` marks logic that belongs in CSS.
- Boundary: container-query and aspect namespaces join as rows here when a consumer earns them; the Vite integration (`@tailwindcss/vite`) is app build wiring.
- Growth: a new axis (a z-index ladder, a shadow ramp) is one interior anchor plus one line in `Scale.css` — never a hand-written utility or a component-local constant.

```typescript
const _spacing = "0.25rem"

const _RATIO = 1.2
const _BASE = 1

const _steps = ["xs", "sm", "base", "lg", "xl", "2xl", "3xl", "4xl"] as const

const _text = Record.fromEntries(
  Array.map(_steps, (step, rank) => {
    const size = _BASE * _RATIO ** (rank - 2)
    return [step, { size: `${size.toFixed(3)}rem`, leading: `${Math.max(1.2, 1.6 - rank * 0.05).toFixed(2)}` }] as const
  }),
)

const _radius = { sm: "0.25rem", md: "0.5rem", lg: "0.75rem", full: "9999px" } as const

const _ease = {
  out: "cubic-bezier(0.16, 1, 0.3, 1)",
  in: "cubic-bezier(0.7, 0, 0.84, 0)",
  spring: "cubic-bezier(0.34, 1.56, 0.64, 1)",
} as const

const _breakpoint = { sm: "40rem", md: "48rem", lg: "64rem", xl: "80rem" } as const

declare namespace Scale {
  type Step = (typeof _steps)[number]
  type Radius = keyof typeof _radius
  type Ease = keyof typeof _ease
}

const Scale: {
  readonly steps: typeof _steps
  readonly radius: typeof _radius
  readonly ease: typeof _ease
  readonly breakpoint: typeof _breakpoint
  readonly css: () => string
} = {
  steps: _steps,
  radius: _radius,
  ease: _ease,
  breakpoint: _breakpoint,
  css: () =>
    [
      Theme.css("spacing", { "": _spacing }),
      Theme.css("text", Record.map(_text, (row) => row.size)),
      Theme.css("text", Record.fromEntries(Record.collect(_text, (step, row) => [`${step}--line-height`, row.leading] as const))),
      Theme.css("radius", _radius),
      Theme.css("ease", _ease),
      Theme.css("breakpoint", _breakpoint),
    ].join("\n"),
}
```

## [5]-[THEME_SWITCH]

[THEME_SWITCH]:
- Owner: the theme vocabulary and its stamp seam riding `Theme`: `Theme.kinds` (the closed `as const` tuple — `light`, `dark`, `system`), `Theme.Kind` derived from it, and `Theme.stamp(kind)` — the one `documentElement.dataset` write, an `Effect.sync` boundary row resolving `system` through the `prefers-color-scheme` media query at stamp time.
- Law: theme is CSS-selected, never JS-branched — `@custom-variant dark (&:where([data-theme=dark] *))` in the token stylesheet reads the stamped attribute; a component styles with `dark:` variants through `cn`, and a `kind === "dark"` conditional in render is the named defect.
- Law: persistence rides the one binding — the theme atom is `Atom.kvs` with `Schema.Literal(...Theme.kinds)` as codec (`system/atom#STORE_ROOT`'s persisted row), and a `useAtomSubscribe` on it runs `Theme.stamp` so the attribute tracks the store without re-render.
- Boundary: the media-query read and the dataset write are this page's platform-forced seam; the atom mechanics are `system/atom`'s; the `@custom-variant` declaration is stylesheet data beside `@plugin "tailwindcss-react-aria-components"`.

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

export { cn, Scale, Theme }
```
