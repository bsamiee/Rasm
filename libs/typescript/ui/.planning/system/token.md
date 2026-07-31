# [UI_TOKEN]

The design-token authority as TWO exports: `Theme` — OKLCH color computed in `colorjs.io` (perceptually-even ramps, gamut-fit, APCA contrast-gated at decode), the closed semantic tone vocabulary and its `Palette` resolution plane, the dimension vocabulary as its `Scale` sub-plane (one `--spacing` multiplier, a modular type scale with paired line-heights, radius/easing/shadow/z/breakpoint rows), and the theme stamp seam — plus `cn`, the folder's one class rail (`extendTailwindMerge` taught every custom group, over the `clsx` fold). Every token emits as Tailwind v4 `@theme` namespace rows through one CSS fold whose head is a policy row, so the light plane and its `data-theme` dark override derive from the same table. Semantic tone is one closed set here and a KEY everywhere else: `Theme.Tone` is the vocabulary and `Theme.Palette` resolves a tone into themed color through the same OKLCH authority, so a surface `_tone` table carries keys and no palette. Theme selection is a `data-theme` attribute the `@custom-variant` selectors read; no component branches on theme in JS, hardcodes a color, writes a raw pixel, mints a local tone union, or imports `clsx`/`twMerge` beside the one rail. The decoded color object feeds two sinks — the CSS custom-property plane here and the viewer render space through `Theme.linear` — so token color and rendered color are one color-space artifact. Motion class-row vocabulary lives at `system/act#MOTION_ROWS`; this page teaches the motion class GROUPS to the one merge table as data so those rows resolve conflicts deterministically. The module is `ui/src/system/token.ts`.

## [01]-[INDEX]

- [02]-[COLOR_AUTHORITY]: the `Theme.Color` decode brand, ramp/contrast algebra, and the head-parameterized CSS fold; `Theme`.
- [03]-[TONE_VOCABULARY]: the closed tone set, the slot ladder, and `Theme.Palette` resolving tone to themed color; `Theme`.
- [04]-[CLASS_RAIL]: the one `cn` composer — `extendTailwindMerge` over `clsx`, group table as data; `cn`.
- [05]-[SCALE_TABLES]: `Theme.Scale` — spacing/text/radius/ease/shadow/z/breakpoint rows and emission; `Theme`.
- [06]-[THEME_SWITCH]: the theme vocabulary, the `data-theme` stamp seam, and the persisted-theme law; `Theme`.

## [02]-[COLOR_AUTHORITY]

[COLOR_AUTHORITY]:
- Owner: `Theme` — one assembled owner: the `Color` transform (CSS color string ⇄ `PlainColorObject`, non-throwing via `tryColor`, `ParseError` on a malformed token), the interior `_pair` refinement factory (foreground/background pairs APCA-gated at decode, floor a policy row) the tone plane composes, the `linear` projection (the render-space triple the viewer material plane ingests), and the `css` emission fold turning any token row table into declaration text under a keyed head.
- Packages: `colorjs.io/fn` + `colorjs.io/spaces` (browser lane — `sRGB`/`sRGB_Linear`/`P3`/`OKLCH`/`OKLab` registered explicitly, never the full registry); `effect` (`Schema`, `ParseResult`, `Array`, `Option`, `Record`); `tailwindcss` is the emission sink — one declaration line per token, each generating its variable and utility family.
- Entry: `Theme.css(rows, emit)` is the one emission fold every token plane reuses — `Theme.Scale.css` and `Theme.Palette.css` fold through it and a viewer probe table emits through it; the perceptual generator rides `[03]` because it reads the tone rows.
- Law: the emission head is a keyed policy row, never a literal at a call site — `_HEADS.theme` writes the Tailwind `@theme` block and `_HEADS.dark` the `:root[data-theme="dark"]` override the stamp seam's `@custom-variant` selector reads, so a light row set and its dark counterpart are two folds of one table and a hand-authored override block is the named defect; a new emission scope is one `_HEADS` row.
- Law: contrast is structural, never disciplinary — a `Theme.Palette.pair` that fails its APCA floor rejects at decode carrying the floor in the refusal; no component re-checks contrast at render, and the refinement factory stays interior because every gated pair in the folder is a tone resolution.
- Law: the decoded interior is `PlainColorObject`, the encoded wire shape the `oklch(...)` string; `serialize` emits `inGamut: true`, and gamut fit is `toGamutCSS` (CSS Color 4 OKLCH chroma reduction) selected once here.
- Boundary: the `to("srgb-linear")` conversion feeding three `ColorManagement` leaves through `Theme.linear` — srgb-linear coords are a fixed 3-tuple, so the marked adapter asserts the bound rather than fabricating a fallback coordinate; the `@import "tailwindcss"` entry stylesheet and `@custom-variant` declarations are app stylesheet data, not module code.
- Growth: a new hue is one tone row emitting its whole slot ladder; a new contrast tier is one `_APCA` row — never a second color engine or a per-component color literal.

```typescript
import { ColorSpace, contrastAPCA, type PlainColorObject, serialize, steps, to, toGamutCSS, tryColor } from "colorjs.io/fn"
import { OKLCH, OKLab, P3, sRGB, sRGB_Linear } from "colorjs.io/spaces"
import { Array, Effect, ParseResult, Record, Schema, type Types } from "effect"

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

const _linear = (color: Schema.Schema.Type<typeof _Color>): readonly [number, number, number] => {
  // BOUNDARY ADAPTER
  const coords = to(color, "srgb-linear").coords as [number, number, number]
  return [coords[0], coords[1], coords[2]] as const
}

// the emission scope is a keyed row, so the dark plane is a second fold of one table rather than a hand-authored
// override block; the selector matches the stamp seam's `@custom-variant` so custom properties inherit from :root
const _HEADS = { theme: "@theme", dark: ':root[data-theme="dark"]' } as const

const _css = (rows: Record.ReadonlyRecord<string, string>, emit: Theme.Emit): string =>
  `${_HEADS[emit.head]} {\n${
    Record.collect(rows, (key, value) => `  --${emit.namespace}${key === "" ? "" : `-${key}`}: ${value};`).join("\n")
  }\n}`
```

## [03]-[TONE_VOCABULARY]

[TONE_VOCABULARY]:
- Owner: `Theme.Palette` — the semantic plane over `[02]`'s decode: `_tones` is the closed roster and `_TONES` its row table (one OKLCH hue and one base chroma per semantic — every slot derives, so a tone is exactly two numbers), `_LADDER` is the one perceptual ramp policy every tone walks, `Theme.ramp` generates a tone's rungs through `steps` and gamut-fits each, `_SLOTS` maps each slot to the rung it reads, `_PAIRS` declares every readable pairing with its APCA floor, and `Theme.Palette.css(plane)` folds the whole tone × slot cross product into `--color-*` rows through `[02]`'s one emission on the parse rail.
- Packages: `colorjs.io/fn` (`steps` over the registered `OKLCH` space — the perceptually-even generator; `toGamutCSS` fits each rung; `serialize` renders the rung the pair schema re-admits); `effect` (`Array`, `Effect`, `Record`, `Schema`).
- Entry: `Theme.Palette.css(plane)` per emission plane; `Theme.ramp(tone)` is the generator a probe or a contrast audit reads, and no other surface computes a color.
- Law: tone is a KEY everywhere but here — a surface's `_tone` table maps its own closed axis (a grade, a lifecycle status, a binding phase, a verdict, a change kind) onto `Theme.Tone` and carries no color, no hue, and no class string, so restyling every surface is one row edit here; a surface-local tone union, a hex literal, or a second palette is the fork this roster exists to foreclose.
- Law: the roster is closed and semantic, never chromatic — two tones may resolve to neighbouring hues (`added` beside `success`, `removed` beside `danger`) because the row carries the MEANING and the hue is its current derivation; collapsing them onto one row would key a diff board to a grade vocabulary and freeze the two apart forever.
- Law: the slot ladder is one ramp read at rungs, never five authored colors — `_SLOTS` names each slot's rank on the shared ladder and the dark plane reads the SAME ladder from the far end (`_rung` reverses the rank), so a light row and its dark counterpart are one derivation and a hand-authored dark palette is the named defect.
- Law: every readable pairing is gated at generation — `_PAIRS` is the closed pairing table and each row decodes through `[02]`'s `_pair(floor)` refinement, so a hue whose ramp cannot clear its floor refuses at emission carrying the floor in the refusal; the emission therefore rides `Effect` while `Theme.Scale.css` stays pure, because a dimension carries no contrast invariant and a color does.
- Law: `Theme.Palette.pair` is generation-time, never render-time — the gate proves the emitted variables, components consume the generated `bg-*`/`text-*`/`border-*` utilities through `cn`, and a component resolving a pair at render re-runs a proof the stylesheet already carries.
- Boundary: which semantic a surface's axis maps to is that surface's law (`system/vital` grades, `system/primitive` recipe and note rows, `viewer/mark` lifecycle, `viewer/panel` phase, `viewer/probe` verdict, the review plane's change kinds); this page owns the roster, the ramp, and the gate alone.
- Growth: a new semantic is one `_TONES` row emitting its whole slot ladder; a new slot is one `_SLOTS` rank plus the `_PAIRS` rows it participates in; a new contrast tier is one `_APCA` row — never a second color engine, a per-surface union, or a hand-written dark block.

```typescript
const _tones = ["neutral", "accent", "success", "caution", "danger", "added", "removed", "changed"] as const

// two numbers per semantic: the whole slot ladder derives, so restyling a tone never touches a consumer
const _TONES = {
  neutral: { hue: 262, chroma: 0.014 },
  accent: { hue: 258, chroma: 0.152 },
  success: { hue: 152, chroma: 0.138 },
  caution: { hue: 76, chroma: 0.148 },
  danger: { hue: 27, chroma: 0.178 },
  added: { hue: 146, chroma: 0.132 },
  removed: { hue: 14, chroma: 0.162 },
  changed: { hue: 96, chroma: 0.128 },
} as const

// one ramp policy every tone walks: near-white head washed of chroma, near-black foot carrying it
const _LADDER = { rungs: 9, head: 0.985, foot: 0.22, wash: 0.12, deep: 0.85 } as const

// each slot reads one rank of the shared ladder; the dark plane reads the same ladder from the far end
const _SLOTS = { on: 0, surface: 1, border: 3, solid: 5, text: 7 } as const

// the closed pairing table: every pair a surface can read is generated and gated, none is left to discipline
const _PAIRS = [
  { bg: "solid", fg: "on", floor: "body" },
  { bg: "surface", fg: "text", floor: "body" },
  { bg: "surface", fg: "border", floor: "muted" },
] as const satisfies ReadonlyArray<{ readonly bg: Theme.Slot; readonly fg: Theme.Slot; readonly floor: Theme.Floor }>

const _ramp = (tone: Theme.Tone): ReadonlyArray<string> =>
  Array.map(
    steps(
      { space: OKLCH, coords: [_LADDER.head, _TONES[tone].chroma * _LADDER.wash, _TONES[tone].hue], alpha: 1 },
      { space: OKLCH, coords: [_LADDER.foot, _TONES[tone].chroma * _LADDER.deep, _TONES[tone].hue], alpha: 1 },
      { space: OKLCH, steps: _LADDER.rungs },
    ),
    (rung) => serialize(toGamutCSS(rung, { space: sRGB }), { inGamut: true }),
  )

const _rung = (plane: Theme.Plane, rank: number): number => (plane === "dark" ? _LADDER.rungs - 1 - rank : rank)

// the ladder projects onto the slot record in one pass; a rank the ladder cannot serve is a construction defect
// between _LADDER and _SLOTS that no consumer arm could act on, so the miss escalates rather than joining a channel
const _slotted = (ladder: ReadonlyArray<string>, plane: Theme.Plane): Effect.Effect<Record.ReadonlyRecord<Theme.Slot, string>> =>
  Effect.orDie(
    Effect.map(
      Effect.forEach(Record.toEntries(_SLOTS), ([name, rank]) =>
        Effect.map(Array.get(ladder, _rung(plane, rank)), (value) => [name, value] as const)),
      Record.fromEntries,
    ),
  )

// the gate runs over the SERIALIZED rungs, so a generated pair re-enters through the same decode a token string does
const _pairOf = (plane: Theme.Plane, tone: Theme.Tone, row: Theme.PairRow): Effect.Effect<Theme.Pair, ParseResult.ParseError> =>
  Effect.flatMap(_slotted(_ramp(tone), plane), (slots) =>
    Schema.decodeUnknown(_pair(row.floor))({ bg: slots[row.bg], fg: slots[row.fg] }))

const _toned = (plane: Theme.Plane, tone: Theme.Tone): Effect.Effect<Theme.Rows, ParseResult.ParseError> =>
  Effect.gen(function* () {
    const slots = yield* _slotted(_ramp(tone), plane)
    yield* Effect.forEach(_PAIRS, (row) =>
      Schema.decodeUnknown(_pair(row.floor))({ bg: slots[row.bg], fg: slots[row.fg] }))
    return Record.fromEntries(Record.collect(slots, (name, value) => [`${tone}-${name}`, value] as const))
  })

const _Palette: {
  readonly tones: typeof _tones
  readonly rows: typeof _TONES
  readonly slots: typeof _SLOTS
  readonly pairs: typeof _PAIRS
  readonly ramp: typeof _ramp
  readonly pair: typeof _pairOf
  readonly css: (plane: Theme.Plane) => Effect.Effect<string, ParseResult.ParseError>
} = {
  tones: _tones,
  rows: _TONES,
  slots: _SLOTS,
  pairs: _PAIRS,
  ramp: _ramp,
  pair: _pairOf,
  css: (plane) =>
    Effect.map(
      Effect.forEach(_tones, (tone) => _toned(plane, tone)),
      (toned) => _css(Record.fromEntries(Array.flatMap(toned, Record.toEntries)), { head: plane, namespace: "color" }),
    ),
}

// every emitted key is a custom color-scale value the one merge instance must know, so the group list DERIVES
const _paletteKeys: ReadonlyArray<string> = Array.flatMap(_tones, (tone) =>
  Array.map(Record.keys(_SLOTS), (slot) => `${tone}-${slot}`))
```

## [04]-[CLASS_RAIL]

[CLASS_RAIL]:
- Owner: `cn` — the folder's ONE class composer: `clsx` folds conditional inputs, one `extendTailwindMerge` instance resolves last-wins conflicts, and the extension table teaches it every custom group — the project `@theme` color scale and the `tw-animate-css` motion groups (`fade`/`zoom`/`spin`/`blur`/`slide` setters, `animation-duration`/`delay`/`repeat` modifiers) — so a `cva` variant, a `tailwindcss-react-aria-components` state variant, and a caller override all collapse to the intended winner.
- Packages: `tailwind-merge` (`extendTailwindMerge`, `validators`; `fromTheme` where a custom group references a whole scale); `clsx` (`ClassValue` — the shared input vocabulary of the whole styling rail); `class-variance-authority` composes downstream (its `cx` IS `clsx`, so a recipe module imports `cn` from here, never a second composer).
- Law: the theme extension DERIVES from the emission — the `color` scale list is `[03]`'s `_paletteKeys`, the exact key set `Theme.Palette.css` writes, so every default color group (`bg-`/`text-`/`ring-`/`border-`/…) resolves precisely the variables the stylesheet carries and neither list can drift; a hand-listed hue teaches the merge instance a name no `@theme` row emits, and `fromTheme` inside a theme scale is circular — both are named defects.
- Law: exactly one merge instance exists — a raw `twMerge` import or a per-component `extendTailwindMerge` silently mis-resolves custom utilities and is the named defect; `twJoin` is admitted only for provably conflict-free static token strings.
- Law: ONE group owns every `animate-*` trigger — the `tw-animate-css` enter/exit pair, the Tailwind-core sustained animations `system/act#MOTION_ROWS` holds a surface in, and the `animate-none` guard together — because a trigger split across two groups leaves `motion-reduce:animate-none` unable to override its own animation; group membership is what makes the reduced-motion guard structural rather than incidental.
- Law: the group table is data — a new custom utility family is one `classGroups` row over `validators.*` predicates or a `fromTheme` scale reference, never a parser change; `system/act` consumes these groups through its `Motion` row strings and never mints a sibling instance.
- Law: `cn` is pure synchronous string work below the Effect boundary — it runs inside render, memoized by `tailwind-merge`'s LRU, and never lifts onto a rail.

```typescript
import { type ClassValue, clsx } from "clsx"
import { extendTailwindMerge, validators } from "tailwind-merge"

const _motion = (stem: string) => ({ [stem]: ["", validators.isNumber, validators.isArbitraryValue] })

const _merge = extendTailwindMerge({
  extend: {
    theme: { color: _paletteKeys },
    classGroups: {
      // one group for every animate-* trigger: the reduced-motion guard can only win against a sibling in its own group
      "animate-trigger": ["animate-in", "animate-out", "animate-none", "animate-pulse", "animate-spin", "animate-ping"],
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

## [05]-[SCALE_TABLES]

[SCALE_TABLES]:
- Owner: `Theme.Scale` — the dimension sub-plane of the one token authority, seven interior anchors: `_spacing` (the single multiplier — a density change is this one token, never a scale rewrite), `_text` (step → `{ size, leading }` pairs, each emitting the `--text-*` + `--text-*--line-height` twin), `_radius`, `_ease` (easing curves as cubic-bezier rows — `system/act` motion and the overlay `useTransitionStyles` phases consume them), `_shadow` (the elevation ramp), `_z` (the stacking ladder — overlay, sheet, palette, toast, cursor ranks as data), and `_breakpoint`; `Theme.Scale.css()` folds all of them through the one `_css` emission into the `@theme` declarations the build stylesheet inlines.
- Packages: `tailwindcss` v4 namespaces are the sink (`--spacing`, `--text-*`, `--radius-*`, `--ease-*`, `--shadow-*`, `--z-*`, `--breakpoint-*` — one namespace row generates the variable and its utility family); `effect` (`Array`, `Record`) folds the emission.
- Law: the type scale is paired data — a text step without its line-height is half a token; the emission writes both variables from one row so `text-<step>` always carries its leading.
- Law: scale values are derivation, not enumeration — the text ladder derives from a ratio fold over the base size (a modular scale), so retuning typography is two numbers, never twelve edits.
- Law: the z ladder is the only stacking authority — overlay classes consume `z-overlay`/`z-toast` utilities generated from `_z`, so stacking order across floats, sheets, palettes, toasts, and presence cursors is one table, never per-component integers.
- Law: no JS reads these values at runtime — the tables exist to emit CSS; a component consumes `p-4`/`text-lg`/`rounded-md` utilities through `cn`, and a runtime pixel computation over `_spacing` marks logic that belongs in CSS. Container-query responsiveness is Tailwind v4 core `@container` variants — styling adapts in CSS while chart geometry measures through its own owner.
- Law: emission is a fold over one namespace table, never a call per axis — `_EMITTED` keys each interior anchor by the Tailwind namespace it writes and `Theme.Scale.css` collects it through `[02]`'s `_css`, so the paired type row lands as one merged record rather than two emissions of one namespace.
- Boundary: aspect namespaces join as rows here when a consumer earns them; the Vite integration (`@tailwindcss/vite`) is app build wiring; color namespaces are `[03]`'s and ride the gated rail.
- Growth: a new axis is one interior anchor plus one `_EMITTED` row — never a hand-written utility, a second emission call, or a component-local constant.

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

const _shadow = {
  low: "0 1px 2px 0 oklch(0% 0 0 / 0.06)",
  mid: "0 4px 12px -2px oklch(0% 0 0 / 0.12)",
  high: "0 12px 32px -4px oklch(0% 0 0 / 0.18)",
} as const

const _z = { panel: "10", overlay: "40", sheet: "50", palette: "60", toast: "70", cursor: "80" } as const

const _breakpoint = { sm: "40rem", md: "48rem", lg: "64rem", xl: "80rem" } as const

// one row per Tailwind namespace: the paired type row merges into its own namespace rather than emitting it twice
const _EMITTED = {
  spacing: { "": _spacing },
  text: {
    ...Record.map(_text, (row) => row.size),
    ...Record.fromEntries(Record.collect(_text, (step, row) => [`${step}--line-height`, row.leading] as const)),
  },
  radius: _radius,
  ease: _ease,
  shadow: _shadow,
  z: _z,
  breakpoint: _breakpoint,
} as const satisfies Record.ReadonlyRecord<string, Theme.Rows>

const _Scale: {
  readonly steps: typeof _steps
  readonly radius: typeof _radius
  readonly ease: typeof _ease
  readonly shadow: typeof _shadow
  readonly z: typeof _z
  readonly breakpoint: typeof _breakpoint
  readonly css: () => string
} = {
  steps: _steps,
  radius: _radius,
  ease: _ease,
  shadow: _shadow,
  z: _z,
  breakpoint: _breakpoint,
  css: () => Record.collect(_EMITTED, (namespace, rows) => _css(rows, { head: "theme", namespace })).join("\n"),
}
```

## [06]-[THEME_SWITCH]

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
  type Floor = keyof typeof _APCA
  type Plane = keyof typeof _HEADS
  type Emit = { readonly head: Theme.Plane; readonly namespace: string }
  type Tone = (typeof _tones)[number]
  type Slot = keyof typeof _SLOTS
  type PairRow = (typeof _PAIRS)[number]
  type _Tones<T extends Record<Theme.Tone, { readonly hue: number; readonly chroma: number }> = typeof _TONES> = T
  type Kind = (typeof _kinds)[number]
  type Step = (typeof _steps)[number]
  type Radius = keyof typeof _radius
  type Ease = keyof typeof _ease
  type Shadow = keyof typeof _shadow
  type Layer = keyof typeof _z
  type Shape = Types.Simplify<{
    readonly Color: typeof _Color
    readonly Palette: typeof _Palette
    readonly Scale: typeof _Scale
    readonly kinds: typeof _kinds
    readonly linear: typeof _linear
    readonly css: typeof _css
    readonly stamp: typeof _stamp
  }>
}

const Theme: Theme.Shape = {
  Color: _Color,
  Palette: _Palette,
  Scale: _Scale,
  kinds: _kinds,
  linear: _linear,
  css: _css,
  stamp: _stamp,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { cn, Theme }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

[MERGE_SCALE_SEGMENTS]-[OPEN]: does a `tailwind-merge` `theme.color` entry carrying a hyphenated value (`danger-surface`) resolve `bg-danger-surface` into the default `bg-*` group, or does the matcher split on the first segment and strand the slot suffix; verify against `tailwind-merge`'s shipped class-group matcher under `node_modules`, and land the group form the matcher proves.
