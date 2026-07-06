# [STYLING]

The stylesheet is an architecture: layers order the cascade, tokens carry the theme, containers size components, selectors carry state. Every rule lands in one layer and reads the design-system tokens; nothing re-declares a palette the floor already owns.

## [01]-[ARCHITECTURE]

The embedded design-system floor declares the layer order — `reset, tokens, base, components, utilities, print, overrides` — and populates the early layers; the template-local sheet appends into the same named layers, where same-name layers merge and later source order wins ties.

```css copy-safe
/* template-local sheet; appends into the floor's named layers — the floor owns the order */
@layer tokens {
  :root { --lane: minmax(14rem, 18rem); --gap: var(--s4); }
}
@layer components {
  .swimlane { display: grid; gap: var(--gap); background: var(--raised); border: 1px solid var(--line) }
}
@layer utilities { .flow > * + * { margin-block-start: var(--gap) } }
@layer overrides { [data-density="compact"] { --gap: var(--s2) } }
```

- [RESET]: element normalization the floor omits; the floor ships box-sizing and margin zeroing, so a template reset layer holds only additions.
- [TOKENS]: template-local custom-property seeds derived from design-system tokens; the palette lives in the floor and never repeats here.
- [BASE]: bare-element defaults for elements a template introduces.
- [COMPONENTS]: every artifact block — swimlane, matrix, rail — one selector family per block.
- [UTILITIES]: single-purpose classes at shallow reach, late in the order.
- [OVERRIDES]: environmental and data-attribute branches, terminal and scarce.

- [FLOOR_OWNS_ORDER]: the floor's `@layer` statement fixes the order once; a template never re-declares it, and a floor component is overridden inside its own layer by later source order or from a later layer — nothing is authored unlayered, since one unlayered rule outranks every layer and re-opens the specificity war the architecture closed.
- [IMPORTANT_INVERTS]: an `!important` declaration flips layer precedence — the earliest layer wins; it rides the overrides layer for forced-state resets alone.
- [REVERT_LAYER]: `revert-layer` rolls a property back to the value from earlier layers, the within-stack escape hatch, distinct from `revert` which drops to the floor.
- [PRECEDENCE]: resolution runs origin, importance, layer, specificity, then source proximity — layer ranks above specificity for normal declarations.

## [02]-[STATE_SYSTEM]

Containers, `:has()`, and registered custom properties form one state machine: data flows down through properties, state flows up through `:has()`, size flows sideways through container queries.

```css copy-safe
@property --tone { syntax: "<percentage>"; inherits: true; initial-value: 0% }
@property --raise { syntax: "<number>"; inherits: true; initial-value: 0 }
@layer components {
  .panel {
    container: panel / inline-size;
    display: grid; gap: var(--s4); padding: var(--s5);
    background: color-mix(in oklch, var(--raised), var(--accent) var(--tone));
    box-shadow: 0 calc(var(--raise) * var(--s1)) calc(var(--raise) * var(--s4)) rgba(0,0,0,.28);
    transition: --tone var(--dur, 0ms), --raise var(--dur, 0ms);
  }
  .panel:has(:is(input, textarea, select):focus-visible) { --tone: 10%; --raise: 1 }
  .panel:has([aria-invalid="true"]) { --tone: 14%; border-color: var(--fail) }
  @container panel (inline-size >= 42rem) {
    .panel__body { display: grid; grid-template-columns: minmax(12rem, .8fr) minmax(0, 1.2fr); gap: var(--s4) }
  }
  .panel[data-density="compact"] .panel__body { gap: var(--s2) }              /* load-bearing branch */
  @container panel style(--density: compact) { .panel__body { gap: var(--s2) } } /* refinement */
}
```

- [DATA_DOWN]: a parent seeds `--tone`, `--raise`, and `--density`, and the whole subtree reads them; one property carries a value every descendant consumes.
- [STATE_UP]: `:has()` reads a descendant condition and rewrites the owner's properties, so focus, invalid, and expanded states flow upward with no class toggle.
- [TYPED_PROPERTY]: `@property` with a declared `syntax` makes a custom property animatable and type-checked; an untyped property is a string and never interpolates.
- [SIZE_SIDEWAYS]: `container-type: inline-size` sizes descendants against the container's inline axis, the default sizing mode.

- [CONTAINER_SELF]: a size container never styles itself from its own query; the query targets descendants, so a queried component wraps in a container element carrying `container-type`.
- [SIZE_AXIS]: `container-type: size` detaches block size from content and collapses a height-auto container; `inline-size` sizes on the inline axis alone.
- [HAS_SCOPE]: `:has()` binds to a stable named container, never the document root; a body-wide `:has()` reevaluates the whole tree on every mutation.
- [STYLE_FALLBACK]: a `style()` container query rides only beside an attribute-selector branch carrying the same rule; the selector is load-bearing and the style query refines it.

## [03]-[SELECTORS]

`:where()` carries reach at zero weight, `:is()` carries branching at its heaviest arm, and nesting with `@scope` bounds reach — selector weight is engineered, never accidental.

```css copy-safe
@layer base {
  :where(article, aside, section) :where(h1, h2, h3, p, ul, ol) { max-inline-size: var(--measure) }
}
@layer components {
  @scope (.metric) to (.metric .metric) {
    :scope { display: grid; gap: var(--s2); padding: var(--s4); background: var(--raised) }
    :where(h3, p) { margin: 0 }
    :is(a, button):focus-visible { outline: 2px solid var(--accent-muted); outline-offset: 2px }
    &:has(> [data-trend="down"]) { --edge: var(--fail) }
  }
}
```

- [WHERE_ZEROES]: `:where()` contributes zero specificity, so a wrapped reset or reach selector never outranks a later component rule.
- [IS_MAXES]: `:is()` takes the specificity of its heaviest argument, so an ID inside `:is()` raises the whole selector to ID weight.
- [NESTING]: `&` resolves to the enclosing selector; a nested rule keeps its layer and takes the parent's heaviest branch as its specificity.
- [SCOPE_DONUT]: `@scope (root) to (limit)` styles the donut between root and limit — a component's own tree without its nested instances; scope proximity resolves ties only after layer and specificity settle.

- [ID_POISON]: an ID inside `:is()`, `:has()`, or `:not()` raises the entire selector to ID weight; IDs stay out of every functional-pseudo argument.
- [PROXIMITY_LAST]: `@scope` proximity ranks below specificity, so a higher-specificity outer rule beats a nearer inner one — scope bounds reach, never precedence.
- [NEST_CEILING]: two nesting levels is the ceiling for one component; a descendant chain past the component boundary is a coupling the sheet cannot hold true.

## [04]-[COMPOSITION]

Grid owns page shells and two-dimensional rhythm, subgrid aligns nested internals, and flex packs one axis — every interior gap rides `gap`, never a margin.

```css copy-safe
@layer components {
  .shell {
    display: grid;
    grid-template-areas: "mast mast" "side main" "side foot";
    grid-template-columns: minmax(14rem, 18rem) minmax(0, 1fr);
    grid-template-rows: auto 1fr auto;
    min-block-size: 100vb; gap: var(--s4);
  }
  .shell__main { grid-area: main; min-inline-size: 0 }
  .deck { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 18rem), 1fr)); gap: var(--s4) }
  .deck > .card { display: grid; grid-template-rows: subgrid; grid-row: span 3; gap: var(--s2) }
  .field-grid { display: grid; grid-template-columns: max-content minmax(0, 1fr); gap: var(--s2) var(--s4) }
}
@container shell (inline-size < 52rem) {
  .shell { grid-template-areas: "mast" "main" "side" "foot"; grid-template-columns: minmax(0, 1fr) }
}
```

- [AREAS]: `grid-template-areas` names a shell's regions as ASCII, and a container query rewrites the map to reflow the whole page at one edit.
- [SUBGRID]: a card adopting `grid-template-rows: subgrid` aligns its header, body, and footer to sibling cards across the parent tracks; a label/field grid adopts column subgrid the same way.
- [AUTO_FIT]: `repeat(auto-fit, minmax(min(100%, <floor>), 1fr))` wraps a collection with no query and never overflows a narrow container; `auto-fill` holds empty tracks where `auto-fit` collapses them.
- [INTRINSIC]: `min-content`, `max-content`, and `fit-content()` size a track to its content, and `minmax(0, 1fr)` is the flexible track that permits shrink below content size.

- [MIN_ZERO]: a `1fr` track carries a min-content floor and overflows long content; `minmax(0, 1fr)` on the track and `min-inline-size: 0` on the child release the floor.
- [GAP_NOT_MARGIN]: interior rhythm rides `gap`; a child margin inside a grid or flex stack double-counts against the gap and breaks alignment.
- [PLACE]: `place-content` distributes tracks inside the container, `place-items` positions items inside their areas — a distribution need never reaches for `place-items`.
- [ALIGN_SELF]: mixed-height rows resolve per item through `align-self`; the row default `stretch` fills the track until an item opts to `start`.

## [05]-[COLOR]

OKLCH is the working space and `color-mix` in OKLCH derives every state variant from a design-system token — hover, active, disabled, and transparency ladders are computed, never hand-picked.

```css copy-safe
@layer tokens {
  :root {
    --accent-hover: color-mix(in oklch, var(--accent) 88%, white);
    --accent-active: color-mix(in oklch, var(--accent) 82%, black);
    --accent-weak: color-mix(in oklch, var(--accent) 18%, var(--surface));
    --hairline: color-mix(in oklch, var(--text) 16%, transparent);
    --fail-fill: color-mix(in oklch, var(--fail) 14%, transparent);
    --scrim: color-mix(in oklch, var(--text) 55%, transparent);
  }
}
@layer components {
  .action { color: var(--bg); background: var(--accent) }
  .action:hover { background: var(--accent-hover) }
  .action:active { background: var(--accent-active) }
  .action:disabled { color: color-mix(in oklch, var(--muted) 60%, transparent); background: var(--surface) }
}
@media (forced-colors: active) {
  .action { color: ButtonText; background: ButtonFace; border: 1px solid ButtonText }
}
```

- [OKLCH_ANATOMY]: OKLCH separates perceptual lightness, chroma, and hue, so equal lightness reads equal across hues and a token ladder holds contrast where an HSL ladder drifts.
- [STATE_LADDER]: `color-mix(in oklch, <token> N%, white|black)` lifts or drops a variant along one axis; hover, active, and pressed states are one mix each off the base token.
- [ALPHA_LADDER]: `color-mix(in oklch, <token> N%, transparent)` derives fills, hairlines, and scrims; a soft fill is a transparency of the status token, not a second hue.
- [FORCED_COLORS]: `forced-colors: active` swaps author color for system keywords — `Canvas`, `CanvasText`, `ButtonText`, `Highlight` — and borders and outlines carry the semantics color drops.

- [CHROMA_CLIP]: high chroma clips to the device gamut and shifts hue; a token ramp holds chroma conservative so each mix stays in gamut across themes.
- [PREFERS_CONTRAST]: `prefers-contrast: more` strengthens separation tokens and focus width; it never authorizes a lower-contrast custom palette.
- [FORCED_NOT_DARK]: `forced-colors` selects a light or dark system palette independent of `prefers-color-scheme`, so a forced-colors branch reads system colors and resets decorative shadows rather than restating a dark theme.
- [OVERLAY_ALPHA]: a real overlay scrim carries genuine alpha, a distinct decision from a `color-mix` toward transparent that blends toward a design value.

## [06]-[FLUID]

`clamp()` ramps type and space between a rem floor and a rem ceiling with a container-relative middle — the rem anchors survive 200% zoom while the middle grows with the component.

```css copy-safe
@layer tokens {
  :root {
    --step-0: clamp(1rem, 0.96rem + 0.3cqi, 1.125rem);
    --step-1: clamp(1.25rem, 1.08rem + 0.8cqi, 1.75rem);
    --step-2: clamp(1.6rem, 1.24rem + 1.4cqi, 2.5rem);
    --space-s: clamp(var(--s2), 0.6rem + 0.8cqi, var(--s4));
    --space-l: clamp(var(--s5), 1rem + 2cqi, var(--s6));
  }
}
@layer components {
  .prose { container: prose / inline-size; font-size: var(--step-0); line-height: 1.55 }
  .prose h2 { font-size: var(--step-2); line-height: 1.12; margin-block: var(--space-l) var(--space-s) }
}
```

- [RAMP_ANCHORS]: a `clamp()` floor and ceiling in rem hold the accessibility bound, and the middle term mixes a rem base with a container unit so text scales locally.
- [CQI_LOCAL]: `cqi` grows a component against its nearest inline container; a component fluid scale demands a container ancestor or it falls back to viewport query units.
- [VW_PAGE]: viewport units drive page-scale display type alone, and body or control text never rides a viewport-only size.
- [SPACE_PAIRS]: a fluid space pair binds to the `--s` scale at its floor and ceiling, so dense controls and editorial blocks carry distinct ramps.

- [ZOOM_FLOOR]: a viewport-only or px-only font size defeats 200% text zoom; the rem anchors inside `clamp()` are the zoom contract.
- [NO_CONTAINER]: `cqi` with no query container resolves against small viewport units, a silent fallback that breaks component-local intent.
- [ONE_SCALE]: one global fluid scale for every density flattens the type system; ramps stay domain-specific.

## [07]-[MOTION]

Motion is opt-in: the resting styles are motionless and `@media (prefers-reduced-motion: no-preference)` grants every transition, so the reduced-motion reader never receives an animation to cancel.

```css copy-safe
@layer tokens {
  :root { --dur: 0ms; --ease: cubic-bezier(.16, 1, .3, 1) }
  @media (prefers-reduced-motion: no-preference) { :root { --dur: 180ms } }
}
@layer components {
  @property --lift { syntax: "<number>"; inherits: false; initial-value: 0 }
  .card {
    --lift: 0; transform: translateY(calc(var(--lift) * -1 * var(--s1)));
    transition: --lift var(--dur) var(--ease), background-color var(--dur) var(--ease);
  }
  .card:hover { --lift: 1 }
  .pop {
    opacity: 1; transform: translateY(0);
    transition: opacity var(--dur) var(--ease), transform var(--dur) var(--ease), display var(--dur) allow-discrete;
  }
  @starting-style { .pop:popover-open { opacity: 0; transform: translateY(var(--s2)) } }
}
@supports (animation-timeline: scroll()) {
  @media (prefers-reduced-motion: no-preference) {
    .reading-bar { animation: fill linear both; animation-timeline: scroll(root block); transform-origin: left }
    @keyframes fill { from { scale: 0 1 } to { scale: 1 1 } }
  }
}
```

- [OPT_IN]: `--dur` is `0ms` at rest and lifts to a real duration only inside `prefers-reduced-motion: no-preference`, so a transition reading `--dur` collapses to instant for the reduced-motion reader.
- [TYPED_TRANSITION]: a `@property`-typed number interpolates, so a custom property drives transform and color transitions a raw variable cannot.
- [DISCRETE]: `transition-behavior: allow-discrete` with `@starting-style` animates a `display`-toggled overlay in and out; opacity and transform carry the visible motion while `display` flips discretely.
- [SCROLL_ENHANCE]: `animation-timeline: scroll()` behind its `@supports` guard drives orientation-only progress, and comprehension never depends on a scroll animation.

- [LATE_OPT_OUT]: an animation that runs by default and cancels under reduced-motion starts before the cancel applies; the resting state carries no motion at all.
- [TRANSITION_ALL]: `transition: all` hides ownership and animates unintended properties; a component lists its transitioned properties.
- [DISPLAY_ONLY]: a `display`-only transition never renders; opacity and transform carry the visible change or the reveal cuts.
- [DUP_VT_NAME]: a duplicate `view-transition-name` aborts capture, so each animated element carries a unique name.

## [08]-[LOGICAL]

Layout speaks the inline and block axes so a sheet survives writing-mode and localization; physical sides stay only where geometry is physical — shadows, transforms, and print imposition.

```css copy-safe
@layer base {
  .flow {
    margin-inline: auto; max-inline-size: var(--measure);
    padding-block: var(--s6); padding-inline: clamp(var(--s4), 4cqi, var(--s6));
  }
  .callout { border-inline-start: var(--s1) solid var(--accent); padding-inline-start: var(--s4); margin-block: var(--s4) }
}
```

- [INLINE_BLOCK]: `margin-inline`, `padding-block`, `inset-inline`, and `border-inline-start` map to the writing mode, and `min-inline-size` with `max-block-size` replaces width and height for flow-relative sizing.
- [PHYSICAL_KEEP]: `box-shadow` offsets, `transform` coordinates, gradient angles, and print crop geometry stay physical — the axis is the medium, not the text flow.

- [SHORTHAND_RESET]: `margin-inline` sets both inline sides at once, so mixing it with a physical longhand in one layer silently resets the intended side unless the physical override is deliberate.
- [CORNER_MAP]: logical corner radii map by writing mode and demand an explicit check before a component ships localized.

## [09]-[PREFERENCES]

Preference queries are peers — color scheme, reduced motion, contrast, forced colors, and print each carry an environmental truth into one token branch, so a preference rewrites values, never a parallel component.

```css copy-safe
@layer overrides {
  @media (prefers-contrast: more) { :root { --hairline: var(--text); --focus-w: 3px } }
  @media (forced-colors: active) {
    :root { --accent: Highlight; --line: CanvasText }
    * { box-shadow: none; text-shadow: none }
  }
}
```

- [SCHEME]: `color-scheme` and `data-theme` carry the palette; the design-system floor owns the mechanism and a template composes the token branch alone.
- [MOTION_GATE]: `prefers-reduced-motion: no-preference` gates every transition through `--dur`, a single seam the whole sheet reads.
- [CONTRAST]: `prefers-contrast: more` strengthens hairline and focus tokens, and `forced-colors: active` swaps to system keywords and drops decoration.
- [RECIPE]: one preference-respecting artifact composes the color-scheme mechanism, the `--dur` motion gate, the contrast and forced-colors token rewrites, and the print branch — every environmental truth a token rewrite in the overrides layer, no component fork.

- [TOKEN_BRANCH]: a preference-specific component one-off forks the component; the branch belongs in a token rewrite the component already reads.
- [FOCUS_KEPT]: no preference branch removes a focus outline; contrast and forced-colors strengthen it.

## [10]-[PRINT]

Print is the terminal medium branch: a print-scoped token flip drives a dark page to a light sheet, fragmentation rules hold repeated units whole, and screen-only controls drop.

```css copy-safe
@layer print {
  @page { size: letter }
  @media print {
    .shell, .deck, .panel__body { display: block }
    .card, figure, table, pre, tr { break-inside: avoid }
    h1, h2, h3 { break-after: avoid }
    .section--major { break-before: page }
    [data-print="omit"], .no-print { display: none }
    .print-only { display: revert }
    a[href]:not([href^="#"])::after { content: " (" attr(href) ")"; font-size: .85em; overflow-wrap: anywhere }
  }
}
.print-only { display: none }
```

- [FLOOR_OWNS_FLIP]: the floor's print layer already flips the tokens to the light sheet and hides screen controls; the template print layer carries only its own reflow, fragmentation, and utilities — never a second token flip.
- [FRAGMENTATION]: `break-inside: avoid` holds a card, row, figure, or code block whole, `break-after: avoid` keeps a heading with its content, and `break-before: page` opens a major section on a fresh sheet.
- [PRINT_UTILITIES]: `.no-print` and `[data-print="omit"]` hide screen controls while `.print-only` reveals print-only content, both resident in the overrides layer.

- [AVOID_IS_REQUEST]: `break-inside: avoid` is a request an oversized box overrides by fragmenting; a unit that must stay whole is sized to a single sheet.
- [MARGIN_BOXES]: `@page` margin boxes, bleeds, and marks never carry required content; the sheet body holds everything a reader must see.
- [BG_SUPPRESSED]: a print setting can drop dark backgrounds and images, so the light token flip prints the page independent of that setting.
- [LINK_EXPAND]: an off-page link expands to its `href` through `::after`, and the `:not([href^="#"])` guard excludes an in-page anchor.
