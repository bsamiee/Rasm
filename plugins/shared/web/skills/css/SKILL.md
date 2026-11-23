---
name: css
description: "Use when writing or reviewing CSS, covering cascade layers, custom properties, selectors, color, layout, typography, state, motion, and feature queries"
---

# [WEB_CSS]

Covers plain CSS organized in cascade layers.

## [01]-[STYLESHEETS]

Document stylesheets open with their layer order, and every rule sits in a named layer:

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

    [hidden]:not([hidden='until-found']) {
        display: none !important;
    }
}
```

| [INDEX] | [LAYER]      | [HOLDS]                                                                     |
| :-----: | :----------- | :-------------------------------------------------------------------------- |
|  [01]   | `reset`      | Box sizing, zero spacing and borders, controls inheriting font and color    |
|  [02]   | `tokens`     | `:root` custom properties and `@property` registrations                     |
|  [03]   | `base`       | Element defaults: root font, page colors, focus ring, selection             |
|  [04]   | `vendor`     | Third-party stylesheets, each an `@import '<path>' layer(vendor)`           |
|  [05]   | `components` | Class rules                                                                 |
|  [06]   | `<chunk>`    | Rules of a lazily loaded stylesheet, named after `components` in that order |

Layer order decides which rule wins:
- Later layers win over earlier ones at any specificity, and an unlayered rule wins over every layer
- `!important` reverses layer order, and `reset` alone holds such declarations
- Sections of a stylesheet each open with a divider and reopen their layer by name
- Lazily loaded stylesheets state no order, import third-party styles into `vendor`, and reopen `components` or their own layer
- `border: 0 solid` in `reset` lets a rule draw a border by width alone (`border-block-end-width: 1px`)

## [02]-[TOKENS]

Custom properties sit at a tier their readers decide:

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

| [INDEX] | [TIER]              | [DECLARED]              | [HOLDS]                                                              |
| :-----: | :------------------ | :---------------------- | :------------------------------------------------------------------- |
|  [01]   | Root token          | `:root` in `tokens`     | Value many rules read: color roles, font stacks, page gutters        |
|  [02]   | Component property  | Opening of a class rule | Value its declarations and descendants compute from                  |
|  [03]   | Registered property | `@property` in `tokens` | Typed value an element's `style` passes or a transition interpolates |

Each property takes its name and tier from what reads it:
- Names state a role (`--color-background`, `--color-line`, `--page-gutter`), and a suffix names one role's variant (`--color-background-muted`)
- Colors and font stacks are root tokens, and a length stays literal in its rule until a formula, a query, or another rule reads it
- Variants, `:has()` states, and queries set a component property again, and formulas reading it follow (`&:has(> figure + figure) { --gaps: 1 }`)
- Rules containing a component set its properties (`--media-height: 10rem` on a grid of figures)
- Markup passes continuous values (a ratio, a focus point) as registered properties in `style`, and closed sets as `data-*` attributes
- Registered properties compute as their type in `calc()` and read `initial-value` where no element sets them
- `inherits: false` keeps a registered property's value off descendants of its element
- `var(<name>, <default>)` reads a property a script or ancestor sets only in some states (`var(--sticky-height, 0px)`)

## [03]-[RULES]

Rules select one class naming a visual role and nest each state, part, variant, and query of that role:

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

        &[aria-pressed='true'] {
            text-decoration: underline;
        }

        svg {
            flex-shrink: 0;
        }

        [data-density='compact'] & {
            min-block-size: 2rem;
        }

        @media (hover: hover) {
            &:hover {
                filter: brightness(92%);
            }
        }
    }
}
```

`.button` holds its states, its icon, a density an ancestor sets, and its hover query in one rule:
- Class names are kebab-case visual roles (`button`, `button-outline`, `site-header`)
- Nested selectors are relative (`svg`, `> a`, `+ li`), and `&` joins a compound (`&:disabled`) or follows an ancestor (`[data-density='compact'] &`)
- Rule bodies order custom properties, declarations, nested rules, then nested at-rules, with declarations in Recess order
- `@media`, `@container`, and `@supports` blocks nest in each rule they change
- Class rules nest their parts and states, a part nests its own states, and structure past that takes a role class of its own
- Selectors hold one class with a state, an attribute, or a descendant step, and layer order decides precedence between groups
- `:is()` groups alternatives of one compound (`:is(input, textarea)`)
- `@scope (<root>) to (<limit>)` inside a layer applies rules within each root and stops at each limit, leaving a component inside to its own rules

## [04]-[COLOR]

Color literals are `oklch()` values in custom property declarations, and every other declaration takes a token or one of these forms:

| [INDEX] | [NEED]                          | [FORM]                                                                     |
| :-----: | :------------------------------ | :------------------------------------------------------------------------- |
|  [01]   | Literal                         | `oklch(63.5% 0.206 31.5deg)`: lightness percentage, chroma, hue angle      |
|  [02]   | One value per color scheme      | `light-dark(<light>, <dark>)`                                              |
|  [03]   | Token at another alpha          | `oklch(from var(<token>) l c h / 0.55)` for backdrops, shadows, borders    |
|  [04]   | Token with one channel moved    | `oklch(from var(<token>) calc(l - 0.1) c h)`                               |
|  [05]   | `currentcolor` at another alpha | `color-mix(in oklab, currentcolor 50%, transparent)`                       |
|  [06]   | Text color on another property  | `currentcolor` on borders, outlines, and `background` under an icon `mask` |

One `color-scheme` declaration decides which value `light-dark()` returns:
- `color-scheme: light dark` on `:root` resolves `light-dark()` by user preference and draws native controls, scrollbars, and canvas to match
- Theme switches set `color-scheme: light` or `dark` on `:root` by attribute selector, and each token keeps its one `light-dark()` value

## [05]-[LENGTHS]

Units state what a length scales with:

| [INDEX] | [UNIT]       | [SCALES_WITH]                         | [USE]                                                               |
| :-----: | :----------- | :------------------------------------ | :------------------------------------------------------------------ |
|  [01]   | `rem`        | Root font size a reader sets          | Font sizes, fixed line heights, spacing, control sizes              |
|  [02]   | Number       | Element font size                     | `line-height` of running text                                       |
|  [03]   | `em`         | Element font size                     | `letter-spacing`                                                    |
|  [04]   | `ch`         | Width of `0` in element font          | Text measure (`max-inline-size: 65ch`)                              |
|  [05]   | `lh`         | Line height                           | Heights counted in lines (`min-block-size: 3lh`)                    |
|  [06]   | `px`         | Nothing                               | Borders, outlines, underline offsets                                |
|  [07]   | `fr`, `%`    | Free track space, containing block    | Grid tracks, shares of a parent                                     |
|  [08]   | `cqi`        | Inline size of nearest container      | Fluid sizes inside a component                                      |
|  [09]   | `vi`, `vb`   | Viewport inline and block size        | Fluid sizes of a page                                               |
|  [10]   | `svb`, `dvb` | Small and dynamic viewport block size | Heights that fit with browser bars shown, or follow them retracting |

Lengths combine through math functions and logical properties:
- Fluid sizes are `clamp(<min>, <rem> + <n>cqi, <max>)`, with a `rem` term text zoom scales (`clamp(2.25rem, 1.25rem + 3.6cqi, 4.5rem)`)
- `min()`, `max()`, and `clamp()` hold math with no `calc()` (`min(30rem, 100vi - 2rem)`)
- Boxes reaching a display's sides pad past its cutout through `max(0.75rem, env(safe-area-inset-bottom))`
- Properties and keywords are logical (`inline-size`, `margin-inline`, `inset-block-start`, `text-align: start`, `float: inline-start`)
- Logical shorthands with 2 values state start, then end (`padding-block: 1.25rem 0.5rem`)
- Media queries compare `width` and `height` in range syntax (`(width < 850px)`), and container queries `inline-size`

## [06]-[LAYOUT]

Grid places children on tracks of both axes, and flex distributes one axis by content:

| [INDEX] | [NEED]                               | [FORM]                                                                      |
| :-----: | :----------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | Columns that fit their container     | `grid-template-columns: repeat(auto-fit, minmax(min(100%, <min>), 1fr))`    |
|  [02]   | Items wrapping at their own minimum  | `flex: 1 1 <min>` under `flex-wrap: wrap`                                   |
|  [03]   | Children on an ancestor's tracks     | `grid-template-columns: subgrid` with `grid-column: 1 / -1`                 |
|  [04]   | Child centered on both axes          | `display: grid` with `place-items: center`                                  |
|  [05]   | Item pushed to an end                | `margin-inline-start: auto`                                                 |
|  [06]   | Wrapper outside its parent's layout  | `display: contents`                                                         |
|  [07]   | Box at a fixed ratio                 | `aspect-ratio`, media filling it through `object-fit` and `object-position` |
|  [08]   | Field sized by its text              | `field-sizing: content` with `min-block-size: 3lh`                          |
|  [09]   | Box covering its positioned ancestor | `position: absolute` with `inset: 0`                                        |

Boxes keep their bounds through these declarations:
- Fluid tracks are `minmax(0, 1fr)`, and flex or grid items holding long content take `min-inline-size: 0`
- `gap` spaces children of a flex or grid container
- `display` takes separate outer and inner keywords (`inline flex`, `inline grid`)
- `position: sticky` with `inset-block-start` pins inside its scroller, and `scroll-padding-block-start` on that scroller keeps link targets clear
- `scrollbar-gutter: stable` reserves scrollbar space before content overflows, and `scrollbar-width` and `scrollbar-color` style a scrollbar
- `isolation: isolate` on a component keeps each descendant `z-index` inside it
- `overflow: clip` clips with no scroll container, and sticky descendants keep their scroller

Components size against a container, and pages against a viewport:
- Ancestors declare `container-type: inline-size`, and descendants nest `@container (inline-size >= 48rem)` and measure in `cqi`
- Queries read their nearest ancestor container, never their own element, and `@container <name> (<condition>)` reads one `container-name` names
- Media queries hold what belongs to a viewport or device: page gutters, orientation, input capability, user preferences

## [07]-[TYPOGRAPHY]

`html` sets type once, and each rule states what departs from it:
- `html` states a family stack and a number `line-height`, and controls inherit both through `font: inherit` in `reset`
- Headings take `text-wrap: balance`, and paragraphs `text-wrap: pretty` inside `@supports (text-wrap: pretty)`
- Text a user wrote takes `overflow-wrap: anywhere`, and `white-space: pre-line` where its line breaks show
- One-line truncation is `overflow: hidden`, `text-overflow: ellipsis`, and `white-space: nowrap` on an item with `min-inline-size: 0`
- Numbers that align or change take `font-variant-numeric: tabular-nums`
- `letter-spacing` is negative on display sizes and positive on uppercase labels
- Justified text pairs `text-align-last: start` with `hyphens: auto`
- Underlines state their offset, and thickness or color where either departs (`text-underline-offset: 4px`, `text-decoration: underline 2px`)
- Weights are numbers (`font-weight: 500`)
- Counters number items with no markup: `counter-reset` on a list, `counter-increment` on each item, `counter(<name>)` in `::before`

## [08]-[STATE]

Selectors read each state from an element or attribute that holds it:

| [INDEX] | [STATE]                           | [SELECTOR]                                                          |
| :-----: | :-------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Descendant or later sibling state | `:has()` (`&:has(:user-invalid)`, `&:has(> figure + figure)`)       |
|  [02]   | Keyboard focus, focus inside      | `:focus-visible`, `:focus-within`                                   |
|  [03]   | Field validity after a user edit  | `:user-invalid`, `:user-valid`                                      |
|  [04]   | Disabled control                  | `:disabled`, `[aria-disabled='true']`                               |
|  [05]   | Current, pressed, expanded        | `[aria-current]`, `[aria-pressed='true']`, `[aria-expanded='true']` |
|  [06]   | Open disclosure, popover, modal   | `[open]`, `:popover-open`, `:modal`, `::backdrop` behind the last 2 |
|  [07]   | State a script or library reports | `data-*` attribute it sets (`[data-selected]`, `[data-dragging]`)   |
|  [08]   | Element with no content           | `:empty`, hiding a box of optional text                             |
|  [09]   | Subtree out of interaction        | `[inert]`                                                           |

## [09]-[MOTION]

Transitions name each property they animate, and an element entering or leaving animates between states of its own rule:

```css
@layer components {
    [popover] {
        opacity: 0;
        translate: 0 -1rem;
        transition:
            translate 200ms ease-out,
            opacity 200ms linear,
            display 200ms allow-discrete,
            overlay 200ms allow-discrete;

        &:popover-open {
            opacity: 1;
            translate: none;

            @starting-style {
                opacity: 0;
                translate: 0 -1rem;
            }
        }

        @media (prefers-reduced-motion: reduce) {
            transition: none;
        }
    }
}
```

Entry reads `@starting-style`, and exit reads the base rule:
- `@starting-style` inside an open state holds values a first render or a change from `display: none` transitions from
- `display` and `overlay` with `allow-discrete` keep a closing element rendered and in its top layer until its transition ends
- Destination states own timing: `transition` on an open state times entry, and one on its base rule times exit
- `translate`, `rotate`, and `scale` are separate properties with separate transitions
- Keyframes of one `0%` frame animate from its values to an element's own, and `reverse` plays exit
- Keyframes over a custom property time a lifetime: `animation-play-state: paused` holds it under hover or focus, and script closes at `animationend`
- Animations and `scroll-behavior: smooth` sit inside `@media (prefers-reduced-motion: no-preference)`, and transitions set `none` under `reduce`
- Scroll-driven keyframes take `animation-timeline: view()` or `scroll()` with `animation-range`, inside `@supports` testing both properties

## [10]-[TOP_LAYER]

Popovers and modal dialogs render in a top layer, above every `z-index` and outside each ancestor's `overflow` and `transform`. Anchor positioning places one against another element's box:
- `anchor-name: --<name>` on a trigger and `position-anchor: --<name>` on a `position: fixed` box tie that box to it
- `position-area` places a box on a grid of 9 areas around its anchor (`block-end span-inline-end`)
- `anchor(<side>)` in an inset property aligns one side, and `anchor-size(inline)` reads an anchor dimension
- `position-try-fallbacks: flip-block, flip-inline` lists placements a box tries in order when it overflows
- `anchor-scope: --<name>` on a repeated component limits that name to each instance

## [11]-[ACCESSIBILITY]

Controls keep a focus ring and a pointer target, and rules depending on an input or user preference nest its query:
- `:focus-visible` draws `outline` with `outline-offset` in `base`, a negative offset where an ancestor clips
- Containers a script focuses (a heading, a dialog) take `outline: none`, and every control keeps its ring
- Controls reserve their pointer target through `min-block-size` and `min-inline-size`, and padding sets their visual size
- `touch-action` is `manipulation` on links and controls, `none` on a drag handle, and `pan-y pinch-zoom` on a horizontal carousel
- `:hover` rules sit inside `@media (hover: hover)`, as a tap leaves `:hover` set on a touch screen
- `@media (prefers-contrast: more)` swaps background and text to system colors (`CanvasText`, `Canvas`)
- `@media (forced-colors: active)` draws borders and outlines in system colors (`CanvasText`, `ButtonText`) where a background drew them
- Text for assistive technology alone takes `position: absolute`, a 1px size, `overflow: hidden`, and `clip-path: inset(50%)`

## [12]-[SUPPORT]

Checker reports of a feature below a product's Baseline target clear inside an `@supports` block testing it, where a page reads correctly without that feature:
- `@supports (<property>: <value>)` tests a declaration and `@supports selector(<selector>)` a selector
- Blocks clear only features their condition tests, and `and` joins one test per feature
- Blocks around a feature inside that target are themselves reported
- Properties, values, and selectors are unprefixed
- `hk check <paths>` reports each finding of `.config/stylelint.config.ts`, and `hk fix <paths>` applies its fixes
- Use `web:web-platform-baseline` for one feature's support
- Use `chrome:extensions` for Chrome releases, colors, control sizes, and extension pages
