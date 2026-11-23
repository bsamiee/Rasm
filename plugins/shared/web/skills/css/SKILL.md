---
name: css
description: "Use when writing or reviewing CSS, covering cascade layers, custom properties, selectors and states, accessibility, color, units, layout, typography, top layer, motion, support targets, and Stylelint findings"
---

# [WEB_CSS]

Covers stylesheets organized in cascade layers and checked by Stylelint.

Use `chrome:extensions` for extension pages and injected UI, and `web:web-platform-baseline` for one feature's engine support.

`hk check <paths>` reports each finding of `.config/stylelint.config.ts`, and `hk fix <paths>` applies its fixes and sorts declarations.

## [01]-[STYLESHEETS]

Document stylesheets open with their layer order, and every rule sits in a named layer, as an unlayered rule wins over every layer:

```css
@layer reset, tokens, base, vendor, components;

@import '<package>/styles.css' layer(vendor);

/* --- [RESET] ------------------------------------------------------------------------- */

@layer reset {
    *,
    ::before,
    ::after,
    ::backdrop,
    ::file-selector-button {
        box-sizing: border-box;
        padding: 0;
        margin: 0;
        border: 0 solid;
    }

    :is(img, video) {
        display: block;
        max-inline-size: 100%;
        block-size: auto;
    }

    a,
    button,
    input,
    select,
    textarea,
    ::file-selector-button {
        font: inherit;
        color: inherit;
        letter-spacing: inherit;
        text-decoration: inherit;
        touch-action: manipulation;
        background: transparent;
    }

    [hidden]:not([hidden='until-found']) {
        display: none !important;
    }
}
```

`<package>/styles.css` is a third-party stylesheet, and each layer holds one kind of rule:

| [INDEX] | [LAYER]      | [HOLDS]                                                                                       |
| :-----: | :----------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `reset`      | Box model, zeroed element defaults, `border: 0 solid` for borders drawn by width alone        |
|  [02]   | `tokens`     | `:root` custom properties and `@property` registrations                                       |
|  [03]   | `base`       | Element defaults and library classes every page takes: root type, page colors, focus ring     |
|  [04]   | `vendor`     | Third-party stylesheets                                                                       |
|  [05]   | `components` | Class rules                                                                                   |
|  [06]   | `<chunk>`    | Rules of one lazily loaded stylesheet, its layer listed after `components` in layer statement |

Later layers win over earlier ones at any specificity. `!important` declarations win in reverse layer order, and `reset` alone holds them (`[hidden]`). Layer and `rem` rules govern document stylesheets, and a stylesheet adopted into a shadow root on a third-party page follows `chrome:extensions`. Stylelint reports `px` in `font`, `font-size`, and `line-height` outside a stylesheet isolating its host through a top-level `:host { all: initial }`.

Stylesheets divide by section and by load:
- Sections open with a divider comment of 85 characters between `/*` and `*/` (`--- [<LABEL>] ---`) and reopen their layer by name
- Subsections open with `/* --- [<LABEL>] */`, and every other comment fails Stylelint, disable comments included
- Lazily loaded stylesheets state no layer order, import third-party styles into `vendor`, and reopen `components` or their own layer
- Stylesheets reading tokens, keyframes, or container names of another file list it in `referenceFiles` of a Stylelint override

## [02]-[TOKENS]

Custom properties are declared where their readers sit:

```css
@layer tokens {
    :root {
        --color-foreground: oklch(21% 0.004 107deg);
        --color-line: oklch(from var(--color-foreground) l c h / 0.2);
        --page-gutter: 2rem;

        @media (width < 850px) {
            --page-gutter: 1.25rem;
        }
    }

    @property --media-ratio {
        inherits: true;
        initial-value: 1;
        syntax: '<number>';
    }
}

@layer components {
    .figure {
        --media-height: 64svb;

        inline-size: min(100%, var(--media-height) * var(--media-ratio));
        margin-inline: var(--page-gutter);
        border-block-end: 1px solid var(--color-line);

        [data-presentation='thumbnail'] & {
            --media-height: 12rem;
        }
    }
}
```

Markup passes `--media-ratio` in an element's `style`, and `data-presentation` is an attribute of an ancestor:

| [INDEX] | [KIND]              | [DECLARED]              | [HOLDS]                                                        |
| :-----: | :------------------ | :---------------------- | :------------------------------------------------------------- |
|  [01]   | Token               | `:root` in `tokens`     | Value many rules read: color roles, font stacks, gutters       |
|  [02]   | Custom property     | Opening of a class rule | Value its declarations and descendants compute from            |
|  [03]   | Registered property | `@property` in `tokens` | Typed value `calc()`, a transition, or a `style()` query reads |

Each custom property takes its name, declaration site, and type from what reads it:
- Names state a role (`--color-background`, `--page-gutter`), and a suffix names one role's variant (`--color-background-muted`)
- Lengths stay literal in their rule until a formula, a query, or a second rule reads them, and a font stack becomes a token at its second reader
- Variants, `:has()` states, and queries reassign a custom property, and formulas declared once in the base rule recompute from it
- Ancestor rules set a nested component's properties (`--media-height: 10rem` on a grid of figures)
- Markup passes continuous values (a ratio, a focus point) as registered properties in `style`, and closed sets as `data-*` attributes
- Values a rule computes or an ancestor inherits select descendant rules through `@container style(--<name>: <value>)`
- `@property` states `syntax`, `inherits`, and `initial-value`, with `inherits: false` for a value its own element alone reads
- Registered properties fall back to `initial-value` where their value is invalid or no element on their inheritance path sets them
- `var(<name>, <default>)` reads a property set in some states, a length default keeping its unit in `calc(var(--sticky-height, 0px) + 1.5rem)`

## [03]-[RULES]

Rules select one class per component and nest each of its states, parts, variants, and queries:

```css
@layer components {
    .button {
        display: inline flex;
        gap: 0.5rem;
        align-items: center;
        min-block-size: 2.75rem;
        padding-inline: 1rem;

        &:disabled {
            opacity: 0.45;
        }

        svg {
            flex-shrink: 0;
        }

        [data-density='compact'] & {
            min-block-size: 2rem;
        }

        @media (hover: hover) {
            &:hover {
                background: color-mix(in oklab, currentcolor 10%, transparent);
            }
        }
    }
}
```

`data-density` is an attribute an ancestor sets, and `:hover` sits inside `@media (hover: hover)`, as a tap leaves `:hover` set on a touch screen.

Selectors and rule bodies follow component structure:
- Class names are kebab-case component names (`button`, `site-header`), and a variant from a closed set is a `data-*` attribute the rule nests
- Portfolio stylesheets alone select library default classes (`react-aria-Button`, `uppy-Dashboard-inner`, `textLayer`) and their `data-*` states
- Nested selectors are relative (`svg`, `> a`, `+ li`), and `&` joins a compound (`&:disabled`) or follows an ancestor (`[data-density='compact'] &`)
- Rule bodies order custom properties, declarations, nested rules, then nested at-rules, with declarations in Recess order
- Selectors hold at most 2 combinators, 2 attributes, and 3 pseudo-classes as written, and specificity up to `0,4,0` resolved through nesting
- Nesting reaches 3 levels below a layer's class rule, and deeper structure takes a class rule of its own
- `@media`, `@container`, and `@supports` count a level where they hold declarations directly, and `@starting-style` counts none
- Library classes one region styles apart from a page sit in `@scope (<root>) to (<limit>)` (`@scope (.editor) to (.media-figure)`)
- `@scope` rules start one level down, match within each root, and stop at each limit, and inherited values cross the limit

## [04]-[STATE]

Selectors read each state from the element or attribute holding it, and a script sets an attribute for a state no selector reads:

| [INDEX] | [STATE]                           | [SELECTOR]                                                                                      |
| :-----: | :-------------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | Descendant or later sibling state | `:has()` (`&:has(:user-invalid)`, `&:has(> figure + figure)`)                                   |
|  [02]   | Keyboard focus, focus inside      | `:focus-visible`, `:focus-within`                                                               |
|  [03]   | Validity after edit or submit     | `:user-invalid`, `:user-valid`                                                                  |
|  [04]   | Disabled control                  | `:disabled`, `[aria-disabled='true']`                                                           |
|  [05]   | Current, pressed, expanded        | `[aria-current]:not([aria-current='false'])`, `[aria-pressed='true']`, `[aria-expanded='true']` |
|  [06]   | Open disclosure, popover, modal   | `:open`, `:popover-open`, `:modal`, `::backdrop` behind popover or modal                        |
|  [07]   | State a library reports           | `data-*` attribute it sets (`[data-selected]`, `[data-dragging]`)                               |
|  [08]   | Element with no content           | `:empty`                                                                                        |
|  [09]   | Subtree out of interaction        | `[inert]`                                                                                       |

## [05]-[ACCESSIBILITY]

Controls keep a focus ring and a pointer target, and pages follow user preferences and assistive technology:
- `:focus-visible` draws `outline` with `outline-offset` in `base`, a negative offset where an ancestor clips
- Containers a script focuses (a heading, a dialog) take `outline: none`
- Controls take a 2.75rem target through `min-block-size` and `min-inline-size`, or an `::after` at negative `inset` where drawn smaller
- `touch-action` is `none` on a drag handle and `pan-y pinch-zoom` on a horizontal carousel
- Skip links sit at `position: fixed` and leave view until focused (`&:not(:focus) { translate: 0 -200%; }`)
- `@media (prefers-contrast: more)` swaps background and text to system colors (`CanvasText`, `Canvas`)
- `@media (forced-colors: active)` replaces a shape a background drew with a `CanvasText` or `ButtonText` border or outline
- `forced-color-adjust: none` keeps colors that carry meaning (a swatch, a chart) under forced colors
- Screen reader text is `position: absolute` with `inline-size: 1px` and `block-size: 1px`
- Screen reader text clips through `overflow: hidden`, `white-space: nowrap`, and `clip-path: inset(50%)`

## [06]-[COLOR]

Color literals are `oklch(<lightness>% <chroma> <hue>deg)` in custom property declarations, and every other declaration takes a token, `transparent`, `currentcolor`, a system color, or a form below:

| [INDEX] | [NEED]                         | [FORM]                                                                                      |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | One value per color scheme     | `light-dark(var(<light>), var(<dark>))`, or `light-dark(<oklch>, <oklch>)` as a token value |
|  [02]   | Token at another alpha         | `oklch(from var(<token>) l c h / 0.55)` for backdrops, shadows, borders                     |
|  [03]   | Token with one channel moved   | `oklch(from var(<token>) calc(l - 0.1) c h)`                                                |
|  [04]   | Text color at another alpha    | `color-mix(in oklab, currentcolor 50%, transparent)`                                        |
|  [05]   | Text color on another property | `currentcolor` on borders, outlines, and `background` under an icon `mask`                  |

`color-scheme: light dark` on `:root` resolves `light-dark()` by user preference and draws native controls, scrollbars, and canvas to match, and a theme switch sets `light` or `dark` on `:root` by attribute selector. Each element's used `color-scheme` decides its `light-dark()` value, as a section can override the scheme it inherits.

## [07]-[UNITS]

Units state what a length scales with, and `rem` follows the root font size a reader sets:

| [INDEX] | [UNIT]       | [USE]                                                                    |
| :-----: | :----------- | :----------------------------------------------------------------------- |
|  [01]   | `rem`        | Font sizes, fixed line heights, spacing, control sizes                   |
|  [02]   | Number       | `line-height` of running text                                            |
|  [03]   | `em`         | `letter-spacing`                                                         |
|  [04]   | `ch`         | Text measure (`max-inline-size: 65ch`)                                   |
|  [05]   | `lh`         | Heights counted in lines                                                 |
|  [06]   | `px`         | Borders, outlines, underline offsets, shadow offsets, query breakpoints  |
|  [07]   | `fr`, `%`    | Grid tracks, shares of a parent                                          |
|  [08]   | `cqi`        | Fluid sizes inside a component                                           |
|  [09]   | `vi`, `vb`   | Fluid sizes of a page                                                    |
|  [10]   | `svb`, `dvb` | Heights fitting with browser toolbars shown, or tracking toolbar changes |

Lengths combine through math functions and logical properties:
- Fluid sizes are `clamp(<min>, <rem> + <n>cqi, <max>)`, with a `rem` term text zoom scales (`clamp(2.25rem, 1.25rem + 3.6cqi, 4.5rem)`)
- Fluid font sizes keep `<max>` within 2.5 times a `<min>` of the same unit, as a wider range stops text from reaching 200% under zoom
- `min()`, `max()`, and `clamp()` hold math directly (`min(30rem, 100vi - 2rem)`)
- Properties and keywords are logical (`inline-size`, `margin-inline`, `inset-block-start`, `overflow-block`, `text-align: start`)
- Logical shorthands with 2 values state start, then end (`padding-block: 1.25rem 0.5rem`)

## [08]-[LAYOUT]

Grid places children on rows and columns at once, and flex distributes one axis by content:

| [INDEX] | [NEED]                                 | [FORM]                                                                              |
| :-----: | :------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Columns that fit their container       | `repeat(auto-fit, minmax(min(100%, <min>), 1fr))`, `auto-fill` keeping empty tracks |
|  [02]   | Items wrapping at their own minimum    | `flex: 1 1 <min>` under `flex-wrap: wrap`                                           |
|  [03]   | Children on an ancestor's tracks       | `grid-template-columns: subgrid` with `grid-column: 1 / -1`                         |
|  [04]   | Areas a query rearranges               | `grid-template: '<a> <b>' auto '<c> <c>' minmax(0, 1fr) / minmax(0, 1fr) auto`      |
|  [05]   | Centered row that can overflow         | `justify-content: safe center`                                                      |
|  [06]   | Content centered in a block box        | `align-content: center`                                                             |
|  [07]   | Wrapper with children on parent tracks | `display: contents`, on an element with no role assistive technology reads          |
|  [08]   | Box at a fixed ratio                   | `aspect-ratio`, media filling it through `object-fit` and `object-position`         |
|  [09]   | Field sized by its text                | `field-sizing: content` with `min-block-size: 3lh`                                  |
|  [10]   | Scroller stopping at each item         | `scroll-snap-type: inline mandatory`, `scroll-snap-align: start` on items           |
|  [11]   | Long content rendered when near view   | `contain-intrinsic-size: auto <length>` with `content-visibility: auto`             |

Boxes keep their bounds and spacing:
- Fluid tracks are `minmax(0, 1fr)`, and flex or grid items holding long content take `min-inline-size: 0`
- `overflow: clip` clips with no scroll container, and sticky descendants keep their scroller

Scrollers hold their gutter, offsets, bar, and overscroll:
- `position: sticky` with `inset-block-start` pins inside its scroller, and `scroll-padding-block-start` on the scroller keeps link targets clear
- `scrollbar-gutter: stable` reserves classic scrollbar space on an `auto`, `scroll`, or `hidden` box, and overlay scrollbars take none
- `scrollbar-width` and `scrollbar-color` style a scrollbar
- Scrollers inside a scrolling page take `overscroll-behavior: contain`

Components size against a container, and pages against a viewport:
- Ancestors declare `container-type: inline-size`, and descendants nest `@container (inline-size >= 48rem)` and measure in `cqi`
- Queries read their nearest ancestor container, never their own element, and `@container <name>` the nearest one declaring `<name>`
- Media queries hold page gutters, orientation, input, and user preferences, comparing `width` and `height` in range syntax (`(width < 850px)`)
- Under a viewport `<meta>` setting `viewport-fit=cover`, boxes touching a screen edge pad by `max(<length>, env(safe-area-inset-<side>))`

## [09]-[TYPOGRAPHY]

`html` in `base` states a family stack and a number `line-height`, and each rule states what departs from it:
- Headings that wrap take `text-wrap: balance`, and paragraphs `text-wrap: pretty`
- User-generated text takes `overflow-wrap: anywhere`, and `white-space: pre-line` where its line breaks show
- One-line truncation is `overflow: hidden`, `text-overflow: ellipsis`, and `white-space: nowrap` on an item with `min-inline-size: 0`
- Numbers that align or change take `font-variant-numeric: tabular-nums`
- `letter-spacing` is negative on display sizes and positive on uppercase labels
- Justified text pairs `text-align-last: start` with `hyphens: auto`, which hyphenates under a `lang` attribute
- Underlines state their offset, and thickness or color where either departs (`text-underline-offset: 4px`, `text-decoration: underline 2px`)
- Weights are numbers (`font-weight: 500`)
- Labels centered in a control take `text-box: trim-both cap alphabetic`, and `min-block-size` keeps the control's target
- Counters number items: `counter-reset` on a list, `counter-increment` on each item, `counter(<name>)` in `::before`

Web fonts declare each face at stylesheet root:
- `@font-face` states `font-family`, `src: url('<file>.woff2') format('woff2')`, `font-display: swap`, and a variable face's `font-weight: 300 800`
- Faces no `font-family` in the same stylesheet reads fail Stylelint

## [10]-[TOP_LAYER]

Popovers and modal dialogs take no `z-index`, as the top layer draws them above every stacking context and outside ancestor `overflow` and `transform`. Fixed and sticky boxes outside the top layer take `z-index` from one order (sticky bar, overlay, dialog, skip link), and `isolation: isolate` on a component keeps each descendant `z-index` inside it.

Anchor positioning places a box against an anchor element:
- `anchor-name: --<name>` on a trigger and `position-anchor: --<name>` on a box anchor the box to its trigger
- Anchored boxes take `position: absolute`, or `position: fixed` where an ancestor clips
- Popovers a `popovertarget` button opens anchor to their button with no `anchor-name`
- `position-area` places a box on a grid of 9 areas around its anchor (`block-end span-inline-end`)
- `anchor(<side>)` in an inset property aligns one side, and `anchor-size(inline)` reads an anchor dimension
- `position-try-fallbacks: flip-block, flip-inline` lists placements a box tries in order on overflow, `@position-try --<name>` declaring one
- `anchor-scope: --<name>` on a repeated component limits `--<name>` to each instance

## [11]-[MOTION]

Transitions name each property they animate, and an element entering or leaving animates between states of its own rule:

```css
@layer components {
    .menu {
        opacity: 0;
        translate: 0 -1rem;
        transition:
            translate 150ms ease-in,
            opacity 150ms linear,
            display 150ms allow-discrete,
            overlay 150ms allow-discrete;

        &:popover-open {
            opacity: 1;
            translate: none;
            transition:
                translate 250ms ease-out,
                opacity 250ms linear,
                display 250ms allow-discrete,
                overlay 250ms allow-discrete;

            @starting-style {
                opacity: 0;
                translate: 0 -1rem;
            }
        }

        @media (prefers-reduced-motion: reduce) {
            &,
            &:popover-open {
                transition: none;
            }
        }
    }
}
```

`.menu` is a popover's class, and `@starting-style` holds values a first render or a change from `display: none` starts from.

Transitions animate between states of one rule:
- `display` and `overlay` with `allow-discrete` keep a closing element rendered and in the top layer until its transition ends
- `transition` on an open state times entry, `transition` on the base rule times exit, and a `reduce` block clears both
- Motion animates `translate`, `rotate`, and `scale` in place of `transform`, each with its own transition
- `will-change` names at most 2 composite properties (`transform`, `opacity`) on an element about to transition
- `<details>` bodies animate through `::details-content` from `block-size: 0` with `overflow: clip` to `block-size: auto` under `:open`
- `interpolate-size: allow-keywords` on `:root` lets `block-size: auto` transition, and `content-visibility` joins it with `allow-discrete`

Animations run from keyframes or view transitions:
- `@keyframes` sit in the stylesheet reading them, with percentage selectors
- Keyframes holding one `0%` frame animate from it to an element's own values, and `animation-direction: reverse` plays them as exit
- Decorative animations and `scroll-behavior: smooth` sit inside `@media (prefers-reduced-motion: no-preference)`
- Timed elements (a toast) run their lifetime animation outside every reduced-motion query, as `animationend` closes them from script
- Lifetime animations pause under `:hover` and `:focus-within` through `animation-play-state: paused`
- Scroll-driven keyframes take `animation-timeline: view()` or `scroll()` with `animation-range`, after the `animation` shorthand resetting both
- `view-transition-name` names each element a `startViewTransition` swap animates, and `view-transition-class` groups them
- `::view-transition-group(*)` times every group, with `animation: none` under `reduce`

## [12]-[SUPPORT]

Support follows the engines each product targets:
- Portfolio stylesheets use features current Chrome, Safari, and Firefox all ship
- Chrome extension pages and injected UI use features current Chrome ships
- Features a target engine lacks sit inside `@supports` testing them, where the page reads correctly without them
- `@supports (<property>: <value>)` tests a declaration and `@supports selector(<selector>)` a selector, and `and` joins one test per feature
- Stylelint reads no engine support, and `if()`, `sibling-index()`, `contrast-color()`, `random()`, and typed `attr()` pass it unchecked
- `if()` appears in stylesheets targeting current Chrome alone, and its branches name tokens, as color rules skip a value holding `if()`
- Stylesheets targeting more engines state each branch in a nested `@media` or `@container` block
- Selects take `appearance: base-select` on themselves and `::picker(select)` inside `@supports (appearance: base-select)`

## [13]-[CHECKS]

Stylelint reports each form below, and stylesheets take the form beside it:

| [INDEX] | [REPORTED]                                                                | [FORM]                                                   |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | Block holding only a comment                                              | Block holding a declaration or rule                      |
|  [02]   | `display: inline-flex` and other `inline-*` values                        | Outer and inner keywords (`inline flex`, `inline grid`)  |
|  [03]   | Duration under 50ms                                                       | 50ms or longer, delays excepted                          |
|  [04]   | Hex, named colors, `rgb()`, `hsl()`, `hwb()`, `lab()`, `lch()`, `oklab()` | `oklch()` in a token                                     |
|  [05]   | Alpha as a percentage                                                     | Number (`/ 0.55`)                                        |
|  [06]   | `grid-template-areas`, `-rows`, and `-columns` together                   | `grid-template`                                          |
|  [07]   | `url()` with an `http` or `https` scheme                                  | Relative path or `data:` URL                             |
|  [08]   | `@counter-style`, `@function`, `@namespace`, `@font-palette-values`       | Built-in counter styles, `calc()` over custom properties |
|  [09]   | Vendor-prefixed property, value, selector, or media feature               | Unprefixed form                                          |
|  [10]   | ID selector, `[class]`, `[id]`, `[style]`                                 | Class, element, or state selector                        |
|  [11]   | Zero length with a unit outside `calc()` (`0px`)                          | `0`                                                      |
