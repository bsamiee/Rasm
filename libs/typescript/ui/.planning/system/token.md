# [UI_TOKEN]

Token holds the design-token authority as TWO exports: `Theme` computes OKLCH color in `colorjs.io` and owns the semantic tone vocabulary, its `Palette` resolution plane, the `Scale` dimension sub-plane, and the theme stamp seam; `cn` is the folder's one class rail, `extendTailwindMerge` taught every custom group over the `clsx` fold. Every token emits as Tailwind v4 `@theme` namespace rows through one CSS fold whose head is a policy row, so the base plane and its `data-theme` overrides derive from the same table. Module: `ui/src/system/token.ts`.

Semantic tone is one closed set here and a KEY everywhere else: `Theme.Tone` is the vocabulary, `Theme.Palette` resolves a tone into themed color through the same OKLCH authority, and a surface `_tone` table carries keys alone. One decoded color object feeds two sinks — the CSS custom-property plane here and the viewer render space through `Theme.linear` — so token color and rendered color are one color-space artifact. Theme and density both ride the `data-*` stamp the `@custom-variant` selectors read, and every surface reaches color, dimension, and class merging through these two exports.

## [01]-[INDEX]

- [02]-[COLOR_AUTHORITY]: `Theme.Color` brands the decode, with ramp/contrast algebra and the head-parameterized CSS fold; `Theme`.
- [03]-[TONE_VOCABULARY]: `Theme.Seed` contracts appearance — closed tone set, slot ladder, plane projections — and `Theme.Palette` resolves tone to themed color; `Theme`.
- [04]-[CLASS_RAIL]: `cn` composes every class — `extendTailwindMerge` over `clsx`, group table as data; `cn`.
- [05]-[SCALE_TABLES]: `Theme.Scale` — spacing/text/radius/ease/shadow/z/breakpoint rows and emission; `Theme`.
- [06]-[THEME_SWITCH]: `Theme` stamps `data-theme` — theme vocabulary, stamp seam, persisted-theme law; `Theme`.

## [02]-[COLOR_AUTHORITY]

[COLOR_AUTHORITY]:
- Owner: `Theme` — one assembled owner: the `Color` transform (CSS color string ⇄ `PlainColorObject`, non-throwing via `tryColor`, `ParseError` on a malformed token), the interior `_pair` refinement factory (foreground/background pairs APCA-gated at decode, floor a policy row) the tone plane composes, the `linear` projection (the render-space triple the viewer material plane ingests), and the `css` emission fold turning any token row table into declaration text under a keyed head.
- Packages: `colorjs.io/fn` + `colorjs.io/spaces` (browser lane — `sRGB`/`sRGB_Linear`/`P3`/`OKLCH`/`OKLab` registered explicitly, never the full registry); `effect` (`Schema`, `ParseResult`, `Array`, `Option`, `Record`); `tailwindcss` is the emission sink — one declaration line per token, each generating its variable and utility family.
- Entry: `Theme.css(rows, emit)` is the one emission fold every token plane reuses — `Theme.Scale.css` and `Theme.Palette.css` fold through it and a viewer probe table emits through it; the perceptual generator rides `[03]` because it reads the tone rows.
- Law: the emission head is a keyed policy row, never a literal at a call site — `_HEADS.theme` writes the Tailwind `@theme` block and every other row writes the `:root[data-theme="…"]` override its `@custom-variant` selector reads, so a base row set and each of its counterparts are folds of one table and a hand-authored override block is the named defect; a new emission scope is one `_HEADS` row beside the `[03]` projection row that generates its values.
- Law: contrast is structural, never disciplinary — a `Theme.Palette.pair` that fails its APCA floor rejects at decode carrying the floor in the refusal; no component re-checks contrast at render, and the refinement factory stays interior because every gated pair in the folder is a tone resolution.
- Law: the decoded interior is `PlainColorObject`, the encoded wire shape the `oklch(...)` string; `serialize` emits `inGamut: true`, and gamut fit is `toGamutCSS` (CSS Color 4 OKLCH chroma reduction) selected once here.
- Boundary: the `to("srgb-linear")` conversion feeding three `ColorManagement` leaves through `Theme.linear` — srgb-linear coords are a fixed 3-tuple, so the marked adapter asserts the bound rather than fabricating a fallback coordinate; the `@import "tailwindcss"` entry stylesheet and `@custom-variant` declarations are app stylesheet data, not module code.
- Growth: a new identity is one seed value; a new semantic is one tone row naming its seed anchor; a new contrast tier is one `_APCA` row — never a second color engine or a per-component color literal.

```typescript signature
import { ColorSpace, contrastAPCA, type PlainColorObject, serialize, steps, to, toGamutCSS, tryColor } from "colorjs.io/fn"
import { OKLCH, OKLab, P3, sRGB, sRGB_Linear } from "colorjs.io/spaces"
import { Array, Effect, ParseResult, Record, Schema, type Types } from "effect"

ColorSpace.register(sRGB)
ColorSpace.register(sRGB_Linear)
ColorSpace.register(P3)
ColorSpace.register(OKLCH)
ColorSpace.register(OKLab)

// every gated pair reads this tier ladder; `high` is the lifted tier a high-contrast projection raises every pair to
const _APCA = { body: 75, large: 60, muted: 45, high: 90 } as const

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

// every ramp derives from this perceptual read on the seed plane: lightness, chroma, hue off an admitted pigment
const _oklch = (color: Schema.Schema.Type<typeof _Color>): readonly [number, number, number] => {
  // BOUNDARY ADAPTER
  const coords = to(color, "oklch").coords as [number, number, number]
  return [coords[0], coords[1], coords[2]] as const
}

// emission scope rides a keyed row, so every override plane is a second fold of one table rather than a
// hand-authored block; each selector matches the stamp seam's `@custom-variant` so custom properties inherit from :root
const _HEADS = { theme: "@theme", dark: ':root[data-theme="dark"]', contrast: ':root[data-theme="contrast"]' } as const

const _css = (rows: Record.ReadonlyRecord<string, string>, emit: Theme.Emit): string =>
  `${_HEADS[emit.head]} {\n${
    Record.collect(rows, (key, value) => `  --${emit.namespace}${key === "" ? "" : `-${key}`}: ${value};`).join("\n")
  }\n}`
```

## [03]-[TONE_VOCABULARY]

[TONE_VOCABULARY]:
- Owner: `Theme.Seed` — the appearance-seed contract both heads render one identity from: six pigments, four surface postures, and the three tone ladders with their cast chroma; `Theme.Palette` is the semantic plane expanding it over `[02]`'s decode — `_tones` the closed roster, `_TONES` each semantic's seed ANCHOR with its bounded hue shift, `_PROJECTIONS` the per-plane variant projection, `Theme.ramp(seed, plane, tone)` the rung generator over `steps`, `_SLOTS` each slot's rank, `_PAIRS` every readable pairing with its APCA floor, `_postured` the posture and scrim rows, and `Theme.Palette.css(seed, plane)` folding the whole tone × slot cross product and the posture plane into `--color-*` rows through `[02]`'s one emission on the parse rail.
- Packages: `colorjs.io/fn` (`steps` over the registered `OKLCH` space — the perceptually-even generator; `to` reads a pigment's perceptual coordinates; `toGamutCSS` fits each rung; `serialize` renders the rung the pair schema re-admits); `effect` (`Array`, `Effect`, `Option`, `Record`, `Schema`); `@rasm/core` (`Shape.Record`).
- Entry: `Theme.Palette.css(seed, plane)` per emission plane; `Theme.ramp(seed, plane, tone)` is the generator a probe or a contrast audit reads, and no other surface computes a color.
- Law: the SEED is the whole authored identity and it crosses as COMPOSITION DATA, never as a wire family — the composing root hands this authority the same seed its desktop peer resolves, both heads expand it through their own perceptual machinery under their own contrast gate, and a re-seed re-tints both from one edit; `Theme.seed` is the shipped floor a standalone head runs on, so a composed seed always wins and a head attached to a host never renders its own identity. Parity is seed-level by construction: pinning resolved values couples two gamut and gate policies that legitimately land on different pixels.
- Law: a semantic names its ANCHOR, never its own colour — `_TONES` carries the seed anchor each tone derives from with the bounded hue shift keeping it legible beside its neighbour, so hue and chroma both read off the seed pigment and a hand-authored hue here forks the identity the seed owns the moment either side moves.
- Law: tone is a KEY everywhere but here — a surface's `_tone` table maps its own closed axis (a grade, a lifecycle status, a binding phase, a verdict, a change kind) onto `Theme.Tone` and carries no color, no hue, and no class string, so restyling every surface is one row edit here; a surface-local tone union, a hex literal, or a second palette is the fork this roster exists to foreclose.
- Law: the roster is closed and semantic, never chromatic — two tones may resolve to neighbouring hues (`added` beside `success`, `removed` beside `danger`) because the row carries the MEANING and the hue is its current derivation; collapsing them onto one row keys a diff board to a grade vocabulary and freezes the two apart forever.
- Law: the ladder ENVELOPE is the seed's and the resolution is this head's — an anchor's ramp spans the extremes of that anchor's own seed tone list at this head's own rung count, so the two heads agree on how dark a surface plane goes and how luminous an accent sits while neither borrows the other's step count; enumerating the producer's rungs verbatim imports a generation whose interaction semantics this head does not run.
- Law: every plane is one PROJECTION of one generation — `_PROJECTIONS` carries the ladder direction, the chroma scale, and the floor lift per plane exactly as the producer's variant row does, so the base plane reads the ladder forward, the dark plane reads the SAME ladder from the far end, and the high-contrast plane scales near-neutral chroma toward zero and raises every pair to its lifted tier; a per-plane colour column is the drift three hand-authored blocks make inevitable.
- Law: every readable pairing is gated at generation — `_PAIRS` is the closed pairing table and each row decodes through `[02]`'s `_pair(floor)` refinement under the plane's own lift, so a seed whose ramp cannot clear its floor refuses at emission carrying the floor in the refusal; the emission therefore rides `Effect` while `Theme.Scale.css` stays pure, because a dimension carries no contrast invariant and a color does.
- Law: `Theme.Palette.pair` is generation-time, never render-time — the gate proves the emitted variables, components consume the generated `bg-*`/`text-*`/`border-*` utilities through `cn`, and a component resolving a pair at render re-runs a proof the stylesheet already carries.
- Law: a posture is a surface CLASS, not a ninth tone — the four seed postures shift the neutral base by their own tone offset under their own chroma ceiling and emit as their own colour rows, and the overlay posture's coverage is the one scrim weight, so a panel, a raised card, a well, and an overlay read one identity at four depths with no tone row spent on any of them.
- Boundary: which semantic a surface's axis maps to is that surface's law (`system/vital` grades, `system/primitive` recipe and note rows, `viewer/mark` lifecycle, `viewer/panel` phase, `viewer/probe` verdict, the review plane's change kinds); this page owns the seed contract, the roster, the ramp, and the gate alone, and the seed VALUES a deployment runs on are the composing root's.
- Growth: a new appearance identity is one seed value; a new semantic is one `_TONES` row naming its anchor; a new slot is one `_SLOTS` rank with the `_PAIRS` rows it participates in; a new plane is one `_HEADS` row beside its `_PROJECTIONS` row; a new contrast tier is one `_APCA` row — never a second color engine, a per-surface union, or a hand-written override block.

```typescript signature
import { Option } from "effect"
import { Shape } from "@rasm/core"

const _tones = ["neutral", "accent", "success", "caution", "danger", "added", "removed", "changed"] as const
const _anchors = ["surface", "accent", "error", "warning", "success", "info"] as const
const _postures = ["panel", "raised", "well", "overlay"] as const

// Posture is the per-surface-class offset a depth plane requests: the tone shift moves the surface away from the
// canvas, the chroma ceiling keeps it near-neutral under a tinted seed, and the coverage is the veil weight a
// scrim over that posture takes.
const _Posture = Schema.Struct({
  toneShift: Schema.Number.pipe(Schema.between(-1, 1)),
  chromaCeiling: Schema.Number.pipe(Schema.between(0, 1)),
  coverage: Schema.Number.pipe(Schema.between(0, 1)),
})

const _Unit = Schema.Number.pipe(Schema.between(0, 1))

// every pigment in this seed contract admits through `[02]`'s decode, so a malformed identity refuses where every
// other colour in this folder refuses rather than reaching a ramp as text no parser admits.
const _Seed = Schema.Struct({
  surface: _Color,
  accent: _Color,
  status: Shape.Record(Schema.Literal("error", "warning", "success", "info"), _Color),
  postures: Shape.Record(Schema.Literal(..._postures), _Posture),
  ramp: Schema.Struct({
    surfaceTones: Schema.NonEmptyArray(_Unit),
    accentTones: Schema.NonEmptyArray(_Unit),
    statusTones: Schema.NonEmptyArray(_Unit),
    castChroma: _Unit,
  }),
})

// this shipped floor is the reference identity — a near-neutral cool grey band and a restrained desaturated cool
// accent, because a saturated brand accent spent on chrome leaves no headroom for the status inks that must
// out-read it. A composed seed replaces it whole; nothing here is a per-value default a caller overrides.
const _SEED: Schema.Schema.Encoded<typeof _Seed> = {
  surface: "#17191d",
  accent: "#3d7eaa",
  status: { error: "#c5484d", warning: "#c08a2e", success: "#4a9a5b", info: "#4c86b8" },
  postures: {
    panel: { toneShift: 0.06, chromaCeiling: 0.04, coverage: 0 },
    raised: { toneShift: 0.11, chromaCeiling: 0.04, coverage: 0 },
    well: { toneShift: -0.04, chromaCeiling: 0.03, coverage: 0 },
    overlay: { toneShift: 0.15, chromaCeiling: 0.05, coverage: 0.62 },
  },
  ramp: {
    surfaceTones: [0.1, 0.14, 0.19, 0.25, 0.32],
    accentTones: [0.58, 0.66, 0.5, 0.74, 0.4],
    statusTones: [0.56, 0.64, 0.46, 0.72],
    castChroma: 0.04,
  },
}

// each semantic names the seed anchor it derives from; the two structural anchors carry a direct pigment and every
// status anchor reads its own row, exactly as the seed declares them
const _TONES = {
  neutral: { anchor: "surface", hueShift: 0 },
  accent: { anchor: "accent", hueShift: 0 },
  success: { anchor: "success", hueShift: 0 },
  caution: { anchor: "warning", hueShift: 0 },
  danger: { anchor: "error", hueShift: 0 },
  added: { anchor: "success", hueShift: -6 },
  removed: { anchor: "error", hueShift: -13 },
  changed: { anchor: "warning", hueShift: 20 },
} as const satisfies Record<Theme.Tone, { readonly anchor: Theme.Anchor; readonly hueShift: number }>

// each anchor carries its own ladder, exactly as the seed's ramp policy assigns it
const _LADDERS = {
  surface: (ramp: Theme.Ramp) => ramp.surfaceTones,
  accent: (ramp: Theme.Ramp) => ramp.accentTones,
  error: (ramp: Theme.Ramp) => ramp.statusTones,
  warning: (ramp: Theme.Ramp) => ramp.statusTones,
  success: (ramp: Theme.Ramp) => ramp.statusTones,
  info: (ramp: Theme.Ramp) => ramp.statusTones,
} as const satisfies Record<Theme.Anchor, (ramp: Theme.Ramp) => Schema.Schema.Type<typeof _Seed>["ramp"]["surfaceTones"]>

// this head's own perceptual resolution over the seed's envelope: the rung count and the chroma wash at the light
// end are the generator's, the extremes are the seed's
const _RUNGS = 9
const _WASH = 0.12

// each plane projects its variant as data — ladder direction, near-neutral chroma scale, and the floor every pair
// lifts to on this plane
const _PROJECTIONS = {
  theme: { ascending: true, chromaScale: 1, lift: Option.none<Theme.Floor>() },
  dark: { ascending: false, chromaScale: 1, lift: Option.none<Theme.Floor>() },
  contrast: { ascending: false, chromaScale: 0.15, lift: Option.some<Theme.Floor>("high") },
} as const satisfies Record<
  Theme.Plane,
  { readonly ascending: boolean; readonly chromaScale: number; readonly lift: Option.Option<Theme.Floor> }
>

// each slot reads one rank of the shared ladder; a plane reading descending takes the same ladder from the far end
const _SLOTS = { on: 0, surface: 1, border: 3, solid: 5, text: 7 } as const

// this closed pairing table generates and gates every pair a surface can read, leaving none to discipline
const _PAIRS = [
  { bg: "solid", fg: "on", floor: "body" },
  { bg: "surface", fg: "text", floor: "body" },
  { bg: "surface", fg: "border", floor: "muted" },
] as const satisfies ReadonlyArray<{ readonly bg: Theme.Slot; readonly fg: Theme.Slot; readonly floor: Theme.Floor }>

const _pigment = (seed: Theme.Seed, anchor: Theme.Anchor): Schema.Schema.Type<typeof _Color> =>
  anchor === "surface" ? seed.surface : anchor === "accent" ? seed.accent : seed.status[anchor]

const _rendered = (rung: PlainColorObject): string => serialize(toGamutCSS(rung, { space: sRGB }), { inGamut: true })

const _ramp = (seed: Theme.Seed, plane: Theme.Plane, tone: Theme.Tone): ReadonlyArray<string> => {
  const row = _TONES[tone]
  const [, chroma, hue] = _oklch(_pigment(seed, row.anchor))
  const tones = _LADDERS[row.anchor](seed.ramp)
  const scaled = chroma * _PROJECTIONS[plane].chromaScale
  const shifted = hue + row.hueShift
  return Array.map(
    steps(
      // each reducer spells as a lambda: passing `Math.max` itself hands `reduce` the index and the array as
      // extra arguments and the whole envelope collapses to NaN
      { space: OKLCH, coords: [tones.reduce((a, b) => Math.max(a, b)), scaled * _WASH, shifted], alpha: 1 },
      { space: OKLCH, coords: [tones.reduce((a, b) => Math.min(a, b)), scaled, shifted], alpha: 1 },
      { space: OKLCH, steps: _RUNGS },
    ),
    _rendered,
  )
}

const _rung = (plane: Theme.Plane, rank: number): number =>
  _PROJECTIONS[plane].ascending ? rank : _RUNGS - 1 - rank

// each ladder projects onto the slot record in one pass; a rank the ladder cannot serve is a construction defect
// between _RUNGS and _SLOTS that no consumer arm could act on, so the miss escalates rather than joining a channel
const _slotted = (ladder: ReadonlyArray<string>, plane: Theme.Plane): Effect.Effect<Record.ReadonlyRecord<Theme.Slot, string>> =>
  Effect.orDie(
    Effect.map(
      Effect.forEach(Record.toEntries(_SLOTS), ([name, rank]) =>
        Effect.map(Array.get(ladder, _rung(plane, rank)), (value) => [name, value] as const)),
      Record.fromEntries,
    ),
  )

// a plane's lift wins over a row's authored floor, so raising contrast is one projection column rather than a
// second pairing table
const _floor = (plane: Theme.Plane, row: Theme.PairRow): Theme.Floor =>
  Option.getOrElse(_PROJECTIONS[plane].lift, () => row.floor)

// this gate runs over the SERIALIZED rungs, so a generated pair re-enters through the same decode a token string does
const _pairOf = (
  seed: Theme.Seed,
  plane: Theme.Plane,
  tone: Theme.Tone,
  row: Theme.PairRow,
): Effect.Effect<Theme.Pair, ParseResult.ParseError> =>
  Effect.flatMap(_slotted(_ramp(seed, plane, tone), plane), (slots) =>
    Schema.decodeUnknown(_pair(_floor(plane, row)))({ bg: slots[row.bg], fg: slots[row.fg] }))

const _toned = (seed: Theme.Seed, plane: Theme.Plane, tone: Theme.Tone): Effect.Effect<Theme.Rows, ParseResult.ParseError> =>
  Effect.gen(function* () {
    const slots = yield* _slotted(_ramp(seed, plane, tone), plane)
    yield* Effect.forEach(_PAIRS, (row) =>
      Schema.decodeUnknown(_pair(_floor(plane, row)))({ bg: slots[row.bg], fg: slots[row.fg] }))
    return Record.fromEntries(Record.collect(slots, (name, value) => [`${tone}-${name}`, value] as const))
  })

// on the depth plane each posture shifts the neutral base by its own offset under its own chroma ceiling, and the
// overlay posture's coverage is the one scrim weight the whole folder veils with
const _postured = (seed: Theme.Seed, plane: Theme.Plane): Theme.Rows => {
  const [lightness, chroma, hue] = _oklch(_pigment(seed, "surface"))
  const direction = _PROJECTIONS[plane].ascending ? 1 : -1
  const rows = Record.map(seed.postures, (posture) =>
    _rendered({
      space: OKLCH,
      coords: [
        lightness + direction * posture.toneShift,
        Math.min(chroma * _PROJECTIONS[plane].chromaScale, posture.chromaCeiling),
        hue,
      ],
      alpha: 1,
    }))
  return { ...rows, scrim: _rendered({ space: OKLCH, coords: [lightness, 0, hue], alpha: seed.postures.overlay.coverage }) }
}

const _Palette: {
  readonly tones: typeof _tones
  readonly anchors: typeof _anchors
  readonly postures: typeof _postures
  readonly rows: typeof _TONES
  readonly slots: typeof _SLOTS
  readonly pairs: typeof _PAIRS
  readonly projections: typeof _PROJECTIONS
  readonly ramp: typeof _ramp
  readonly pair: typeof _pairOf
  readonly css: (seed: Theme.Seed, plane: Theme.Plane) => Effect.Effect<string, ParseResult.ParseError>
} = {
  tones: _tones,
  anchors: _anchors,
  postures: _postures,
  rows: _TONES,
  slots: _SLOTS,
  pairs: _PAIRS,
  projections: _PROJECTIONS,
  ramp: _ramp,
  pair: _pairOf,
  css: (seed, plane) =>
    Effect.map(
      Effect.forEach(_tones, (tone) => _toned(seed, plane, tone)),
      (toned) =>
        _css(
          { ...Record.fromEntries(Array.flatMap(toned, Record.toEntries)), ..._postured(seed, plane) },
          { head: plane, namespace: "color" },
        ),
    ),
}

// every emitted key is a custom color-scale value the one merge instance must know, so the group list DERIVES
const _paletteKeys: ReadonlyArray<string> = [
  ...Array.flatMap(_tones, (tone) => Array.map(Record.keys(_SLOTS), (slot) => `${tone}-${slot}`)),
  ..._postures,
  "scrim",
]
```

## [04]-[CLASS_RAIL]

[CLASS_RAIL]:
- Owner: `cn` — the folder's ONE class composer: `clsx` folds conditional inputs, one `extendTailwindMerge` instance resolves last-wins conflicts, and the extension table teaches it every custom group — the project `@theme` color scale and the `tw-animate-css` motion groups (`fade`/`zoom`/`spin`/`blur`/`slide` setters, the `animation-duration`/`delay`/`repeat`/`direction`/`fill-mode` timing modifiers, and the play-state pair) — so a `cva` variant, a `tailwindcss-react-aria-components` state variant, and a caller override all collapse to the intended winner.
- Packages: `tailwind-merge` (`extendTailwindMerge`, `validators`; `fromTheme` where a custom group references a whole scale); `clsx` (`ClassValue` — the shared input vocabulary of the whole styling rail); `class-variance-authority` composes downstream (its `cx` IS `clsx`, so a recipe module imports `cn` from here, never a second composer).
- Law: the theme extension DERIVES from the emission — the `color` scale list is `[03]`'s `_paletteKeys`, the exact key set `Theme.Palette.css` writes, so every default color group (`bg-`/`text-`/`ring-`/`border-`/…) resolves precisely the variables the stylesheet carries and neither list can drift; a hand-listed hue teaches the merge instance a name no `@theme` row emits, and `fromTheme` inside a theme scale is circular — both are named defects.
- Law: a tone-slot key rides the theme scale WHOLE, hyphen and all — registration splits each theme value on the class separator into a trie chain, so `danger-surface` seats as the `bg → danger → surface` descent and lookup walks the same separator to reach it; `bg-danger-surface` therefore lands in the default `bg-color` group with its slot suffix intact and no flattened key, per-slot group, or separator escape is bought. Every dead-end descent falls back to that node's validators against the REJOINED remainder rather than stranding, which is what keeps an arbitrary value valid beside the scale — but the default color group's fallback is the arbitrary-value pair alone, with no catch-all beneath it, so a key the derived list omits joins no group at all and merges as an unknown class: the derivation above is the whole guarantee, not a convenience.
- Law: exactly one merge instance exists — a raw `twMerge` import or a per-component `extendTailwindMerge` silently mis-resolves custom utilities and is the named defect; `twJoin` is admitted only for provably conflict-free static token strings.
- Law: ONE group owns every `animate-*` trigger — the `tw-animate-css` enter/exit pair, the Tailwind-core sustained animations `system/act#MOTION_ROWS` holds a surface in, and the `animate-none` guard together — because a trigger split across two groups leaves `motion-reduce:animate-none` unable to override its own animation; group membership is what makes the reduced-motion guard structural rather than incidental.
- Law: the typography plugin's modifiers join the table as data — `prose-size` holds the five size modifiers and `prose-ramp` the five neutral ramps, while the seventeen link accents ride `prose-accent` as their own group because a ramp and an accent COMPOSE by the plugin's contract (neutrals write every register, accents rewrite the link register alone), so seating them together erases a lawful pairing; `prose`, `not-prose`, and `prose-invert` join no group — the first two are structural and conflict-free, and the invert remap is foreclosed because the `data-theme` plane already owns dark. `@plugin "@tailwindcss/typography"` loads beside the tailwind entry — stylesheet data like the `@custom-variant` declarations, never module code.
- Law: the group table is data — a new custom utility family is one `classGroups` row over `validators.*` predicates or a `fromTheme` scale reference, never a parser change; `system/act` consumes these groups through its `Motion` row strings and never mints a sibling instance.
- Law: `cn` is pure synchronous string work below the Effect boundary — it runs inside render, memoized by `tailwind-merge`'s LRU, and never lifts onto a rail.

```typescript signature
import { type ClassValue, clsx } from "clsx"
import { extendTailwindMerge, validators } from "tailwind-merge"

const _motion = (stem: string) => ({ [stem]: ["", validators.isNumber, validators.isArbitraryValue] })

const _merge = extendTailwindMerge({
  extend: {
    theme: { color: _paletteKeys },
    classGroups: {
      // one group for every animate-* trigger — enter/exit, every platform-sustained animation, the named component
      // animations, and the guard: the reduced-motion guard can only win against a sibling in its own group
      "animate-trigger": [
        "animate-in",
        "animate-out",
        "animate-none",
        "animate-pulse",
        "animate-spin",
        "animate-ping",
        "animate-bounce",
        "animate-accordion-down",
        "animate-accordion-up",
        "animate-collapsible-down",
        "animate-collapsible-up",
        "animate-caret-blink",
      ],
      "motion-fade": [_motion("fade-in"), _motion("fade-out")],
      "motion-zoom": [_motion("zoom-in"), _motion("zoom-out")],
      "motion-spin": [_motion("spin-in"), _motion("spin-out")],
      "motion-blur": [_motion("blur-in"), _motion("blur-out")],
      // dir-aware `start`/`end` edges join their physical siblings: one group, or an RTL row and an LTR row both survive
      "motion-slide": [
        _motion("slide-in-from-top"),
        _motion("slide-in-from-bottom"),
        _motion("slide-in-from-left"),
        _motion("slide-in-from-right"),
        _motion("slide-in-from-start"),
        _motion("slide-in-from-end"),
        _motion("slide-out-to-top"),
        _motion("slide-out-to-bottom"),
        _motion("slide-out-to-left"),
        _motion("slide-out-to-right"),
        _motion("slide-out-to-start"),
        _motion("slide-out-to-end"),
      ],
      "motion-duration": [{ "animation-duration": [validators.isNumber, validators.isArbitraryValue] }],
      "motion-delay": [{ delay: [validators.isNumber, validators.isArbitraryValue] }],
      "motion-repeat": [{ repeat: ["0", "1", "infinite"] }],
      "motion-direction": [{ direction: ["normal", "reverse", "alternate", "alternate-reverse"] }],
      "motion-fill": [{ "fill-mode": ["none", "forwards", "backwards", "both"] }],
      "motion-play": ["running", "paused", { "play-state": ["running", "paused"] }],
      // ramp and accent are separate groups by the plugin's own contract: a neutral ramp writes every register while
      // an accent rewrites the link register alone, so `prose-slate prose-sky` is a lawful pairing no group may erase
      "prose-size": ["prose-sm", "prose-base", "prose-lg", "prose-xl", "prose-2xl"],
      "prose-ramp": ["prose-slate", "prose-gray", "prose-zinc", "prose-neutral", "prose-stone"],
      "prose-accent": [
        "prose-red",
        "prose-orange",
        "prose-amber",
        "prose-yellow",
        "prose-lime",
        "prose-green",
        "prose-emerald",
        "prose-teal",
        "prose-cyan",
        "prose-sky",
        "prose-blue",
        "prose-indigo",
        "prose-violet",
        "prose-purple",
        "prose-fuchsia",
        "prose-pink",
        "prose-rose",
      ],
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
- Law: no JS reads these values at runtime — the tables exist to emit CSS; a component consumes `p-4`/`text-lg`/`rounded-md` utilities through `cn`, and a runtime pixel computation over `_spacing` marks logic that belongs in CSS. Container-query responsiveness is Tailwind v4 core `@container` variants — a baseline-verified platform surface — so styling adapts in CSS while chart geometry measures through its own owner.
- Law: density is the spacing multiplier's own two-value table — `_density` names the comfortable floor and the touch raise, `_spacing` derives the floor, and the touch override restates `--spacing` under the `data-density` selector as stylesheet data; a third scale or a per-component density knob never exists.
- Law: motion timing stays `system/act`'s row data — the `Motion` rows carry their `animation-duration-*` modifiers as whole policy strings no second table re-derives, so a `--duration-*` namespace here is an axis with no consumer; it enters when a surface consumes a named step.
- Law: emission is a fold over one namespace table, never a call per axis — `_EMITTED` keys each interior anchor by the Tailwind namespace it writes and `Theme.Scale.css` collects it through `[02]`'s `_css`, so the paired type row lands as one merged record rather than two emissions of one namespace.
- Boundary: aspect namespaces join as rows here when a consumer earns them; the Vite integration (`@tailwindcss/vite`) is app build wiring; color namespaces are `[03]`'s and ride the gated rail.
- Growth: a new axis is one interior anchor with one `_EMITTED` row — never a hand-written utility, a second emission call, or a component-local constant.

```typescript signature
// this touch raise is the one density move: coarse pointers take a wider rhythm from one value, and every
// spacing utility follows because the whole dimension plane multiplies through --spacing
const _density = { comfortable: "0.25rem", touch: "0.3125rem" } as const

const _spacing = _density.comfortable

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
  readonly density: typeof _density
  readonly radius: typeof _radius
  readonly ease: typeof _ease
  readonly shadow: typeof _shadow
  readonly z: typeof _z
  readonly breakpoint: typeof _breakpoint
  readonly css: () => string
} = {
  steps: _steps,
  density: _density,
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
- Owner: the theme vocabulary and its stamp seam riding `Theme`: `Theme.kinds` (the closed `as const` tuple — `light`, `dark`, `contrast`, `system`), `Theme.Kind` derived from it, and `Theme.stamp(kind)` — the one `documentElement.dataset` write, an `Effect.sync` boundary row resolving `system` through the host contrast and scheme queries at stamp time.
- Law: the elected kind resolves to an EMISSION PLANE, so the attribute a stamp writes is exactly the plane `[03]` generated rows under and a kind whose plane no `_HEADS` row serves is unspellable; the base plane clears the attribute rather than stamping a name no selector matches, because its rows live in the `@theme` block every override inherits from.
- Law: theme is CSS-selected, never JS-branched — `@custom-variant dark (&:where([data-theme=dark] *))` and its contrast sibling in the token stylesheet read the stamped attribute; a component styles with those variants through `cn`, and a `kind === "dark"` conditional in render is the named defect.
- Law: persistence rides the one binding — the theme atom is `Atom.kvs` with `Schema.Literal(...Theme.kinds)` as codec, its key minted through `system/atom#STORE_ROOT`'s seal-versioned key member and never hand-spelled, and a `useAtomSubscribe` on it runs `Theme.stamp` so the attribute tracks the store without re-render.
- Law: density stamps beside theme — a coarse-pointer environment (the negation of `(hover: hover) and (pointer: fine)`) elects the touch multiplier, the same stamp writes `data-density` in the pass that writes `data-theme` (the comfortable floor clears the attribute exactly as the base plane does), and the `:root[data-density="touch"]` override restating `--spacing` from `Theme.Scale.density.touch` is stylesheet data; a JS pixel derivation or a second dimension table is the named defect.
- Boundary: the media-query reads and the dataset writes are this page's platform-forced seam; the atom mechanics are `system/atom`'s; the `@custom-variant` declaration is stylesheet data beside `@plugin "tailwindcss-react-aria-components"`.

```typescript signature
import { Effect } from "effect"

const _kinds = ["light", "dark", "contrast", "system"] as const

// `system` resolves both host axes at stamp time, contrast outranking scheme because a stated contrast need is a
// requirement where a scheme preference is a taste
const _resolved = (kind: (typeof _kinds)[number]): Theme.Plane =>
  kind !== "system"
    ? kind === "light" ? "theme" : kind
    : globalThis.matchMedia("(prefers-contrast: more)").matches
    ? "contrast"
    : globalThis.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "theme"

// this base plane carries no attribute: its rows live in the `@theme` block every override inherits from; density
// rides the same stamp — coarse pointers elect the touch multiplier and the comfortable floor clears the attribute
const _stamp = (kind: (typeof _kinds)[number]): Effect.Effect<void> =>
  Effect.sync(() => {
    const root = globalThis.document.documentElement
    const plane = _resolved(kind)
    if (plane === "theme") {
      delete root.dataset["theme"]
    } else {
      root.dataset["theme"] = plane
    }
    if (globalThis.matchMedia("(hover: hover) and (pointer: fine)").matches) {
      delete root.dataset["density"]
    } else {
      root.dataset["density"] = "touch"
    }
  })

declare namespace Theme {
  type Color = Schema.Schema.Type<typeof _Color>
  type Pair = { readonly fg: Color; readonly bg: Color }
  type Rows = Record.ReadonlyRecord<string, string>
  type Floor = keyof typeof _APCA
  type Plane = keyof typeof _HEADS
  type Emit = { readonly head: Theme.Plane; readonly namespace: string }
  type Tone = (typeof _tones)[number]
  type Anchor = (typeof _anchors)[number]
  type Posture = (typeof _postures)[number]
  type Seed = Schema.Schema.Type<typeof _Seed>
  type Ramp = Theme.Seed["ramp"]
  type Slot = keyof typeof _SLOTS
  type PairRow = (typeof _PAIRS)[number]
  type _Tones<T extends Record<Theme.Tone, { readonly anchor: Theme.Anchor; readonly hueShift: number }> = typeof _TONES> = T
  type Kind = (typeof _kinds)[number]
  type Step = (typeof _steps)[number]
  type Radius = keyof typeof _radius
  type Ease = keyof typeof _ease
  type Shadow = keyof typeof _shadow
  type Layer = keyof typeof _z
  type Shape = Types.Simplify<{
    readonly Color: typeof _Color
    readonly Seed: typeof _Seed
    readonly seed: typeof _SEED
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
  Seed: _Seed,
  seed: _SEED,
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

(none)
