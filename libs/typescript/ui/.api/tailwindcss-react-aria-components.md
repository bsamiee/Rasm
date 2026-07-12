# [TS_UI_API_TAILWINDCSS_REACT_ARIA_COMPONENTS]

`tailwindcss-react-aria-components` is a build-time Tailwind CSS catalog-bound plugin (zero runtime, no component import) that registers a variant for each state a react-aria-components primitive exposes. RAC renders its interaction state as DOM `data-*` attributes — `data-hovered`, `data-pressed`, `data-selected`, `data-focus-visible`, `data-disabled`, `data-open`, `data-entering`/`data-exiting`, and the enum attributes `data-placement`, `data-orientation`, etc. — and this plugin maps each to a Tailwind variant selecting that attribute, through one `addVariant` fold over a data table of `(attribute → selector)` pairs. The payoff is a collapse: RAC's render-prop `className`/`style` functions (`className={({isSelected, isPressed}) => isSelected ? "…" : "…"}`) become static, composable utility strings (`className="selected:… pressed:…"`), so state-conditional styling lives in the `token`/`view` class vocabulary and flows through the same `cn` merge rail as everything else. It is registered CSS-first in catalog-bound via `@plugin "tailwindcss-react-aria-components"`, with an optional `prefix` to namespace the variants; unprefixed, the native-overlapping variants (`hover`/`focus`/`disabled`/…) are emitted as a unified `:is()` selector so they also apply to non-RAC native elements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss-react-aria-components`
- package: `tailwindcss-react-aria-components`
- license: `Apache-2.0` (© Adobe)
- module format: CommonJS Tailwind plugin (`module.exports = plugin.withOptions(...)`); the shipped `.d.ts` types it as `plugin(options?: { prefix?: string }) => { handler }` with `__isOptionsFunction`
- runtime target: build-time only — a PostCSS/Tailwind plugin evaluated during CSS compilation; ZERO runtime, never imported by a component
- peer: `tailwindcss@^catalog` (the folder's `tailwindcss catalog`); catalog registers it CSS-first via `@plugin`, not a JS `plugins: []` array
- mechanism: data-attribute driven — the variants select the `data-*` attributes react-aria-components emits (`.api/react-aria-components.md`); the plugin adds them via `addVariant` and carries no knowledge of React
- rail: the `token`/`view` styling plane — turns RAC render-prop state into the class vocabulary that composes through `cn`/`twMerge`
- not-Effect: build-time CSS transform; entirely outside the runtime and the Effect boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the boolean state variants — one per RAC render-prop flag
- rail: types
- Each row is a *category* of the closed variant set the plugin registers, grouped as its source groups them; a variant name is what the caller write in `className`, and it selects the matching `data-*` attribute RAC emits. This is a data table fed to one `addVariant` loop, not a hand-written selector per state — a new RAC state is a new row upstream, not a new mechanism.

| [INDEX] | [VARIANT_FAMILY]                                                                         | [STATE_KIND]   | [CONSUMER_BOUNDARY]                                                                                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `hover` `focus` `focus-visible` `focus-within` `pressed` `active` `disabled`             | interactive    | `isHovered`/`isFocused`/`isFocusVisible`/`isPressed`/`isDisabled` — the core interaction styling on every `view/primitive` row |
|  [02]   | `selected` `selection-start` `selection-end` `indeterminate`                             | selection      | `isSelected` + range endpoints — list/table/grid item and calendar-cell selection styling                                      |
|  [03]   | `open` `expanded` `entering` `exiting`                                                   | overlay/motion | `isOpen`/`isExpanded` + transition phases — `entering`/`exiting` pair with `tw-animate-css` for enter/exit                     |
|  [04]   | `required` `invalid` `unavailable` `read-only` `placeholder-shown` `pending` `empty`     | field/value    | field validity + value states — the `Schema`→aria `FormBinding` surfaces `invalid`/`required` here                             |
|  [05]   | `dragging` `drop-target` `resizing` `allows-dragging` `allows-removing` `allows-sorting` | drag/resize    | `isDragging`/`isDropTarget` + capability flags — dnd + column-resize affordance styling                                        |
|  [06]   | `has-submenu` `has-child-items` `current` `outside-month` `outside-visible-range`        | structural     | menu/collection structure + calendar range edges — `current` marks the active nav item                                         |

[PUBLIC_TYPE_SCOPE]: the enum variants — one per RAC render-prop enum, as `name-value`
- rail: types
- The enum attributes take a value, so the variant is `name-value` (`placement-bottom`, `orientation-horizontal`); `selection-mode`/`resizable-direction`/`sort-direction` register under the short names `selection`/`resizable`/`sort`. Same one-loop mechanism over a `{ attribute: [values] }` data table.

| [INDEX] | [VARIANT_FAMILY]                    | [ENUM_KIND]       | [CONSUMER_BOUNDARY]                                                                                        |
| :-----: | :---------------------------------- | :---------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `placement-{left,right,top,bottom}` | overlay side      | `data-placement` on popover/tooltip — arrow + entry-direction styling, paired with `floating-ui` placement |
|  [02]   | `orientation-{horizontal,vertical}` | axis              | `data-orientation` on tabs/slider/separator/toolbar — axis-dependent layout                                |
|  [03]   | `selection-{single,multiple}`       | selection mode    | `data-selection-mode` — collection affordance (checkboxes vs highlight) by mode                            |
|  [04]   | `sort-{ascending,descending}`       | sort direction    | `data-sort-direction` on a table column header — sort-indicator styling                                    |
|  [05]   | `resizable-{right,left,both}`       | resize dir        | `data-resizable-direction` on a resizable table column                                                     |
|  [06]   | `type-{literal,year,month,day}`     | date segment      | `data-type` on a date-field segment — per-segment styling in a latent date `view` row                      |
|  [07]   | `layout-{grid,stack}`               | collection layout | `data-layout` on a GridList/collection — layout-dependent item styling                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and the prefix option
- rail: surfaces-and-dispatch
- The whole surface is a catalog-bound `@plugin` directive in the folder's Tailwind entry CSS; there is no JS import. The `prefix` option namespaces every variant (`rac-selected:`) when a name does collide with a project convention.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                                               |
| :-----: | :---------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | `@plugin "tailwindcss-react-aria-components";`                          | registration   | the `token`/CSS entry — registers all boolean + enum variants; catalog-bound CSS-first, not a JS config array     |
|  [02]   | `@plugin "tailwindcss-react-aria-components" { prefix: rac; }`          | namespaced     | prefixes every variant (`rac-selected:`) — only when the unprefixed names collide with an existing convention     |
|  [03]   | `selected:` / `pressed:` / `placement-bottom:` … applied in `className` | variant use    | the composed surface — a variant scopes the utilities after it; stacks with `hover:`/`md:` and flows through `cn` |

[ENTRYPOINT_SCOPE]: the unprefixed native-collapse behavior
- rail: surfaces-and-dispatch
- Unprefixed (`prefix: ''`), variants that overlap a native CSS state (`hover`→`:hover`, `focus`→`:focus`, `disabled`/`read-only`/`open`/…) are emitted as a unified `:is()` selector so the variant applies to BOTH a RAC element (`[data-rac][data-hovered]`) and a plain native element (`:not([data-rac]):hover`) — one variant name covers custom and native. `hover` additionally wraps `@media (hover: hover)` to avoid sticky touch styles.

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                                                                                              |
| :-----: | :------------------------------------------------------------------------------------ | :-------------- | :------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `&:is(:where([data-rac])[data-hovered], :where(:not([data-rac])):hover)` (unprefixed) | native collapse | `hover`/`focus`/`active`/`disabled`/`read-only`/`open`/`expanded` apply to RAC + native; `:where()` keeps specificity at (0,1,0) |
|  [02]   | `@media (hover: hover)` wrapper on `hover`                                            | pointer gate    | prevents sticky `hover:` styles on touch; still composable with `group-hover:`/`peer-hover:`/`not-hover:`                        |
|  [03]   | `placeholder-shown:` merges `[data-placeholder], :placeholder-shown`                  | merge selector  | applies to both a RAC field and a native input placeholder without a second variant                                              |

## [04]-[IMPLEMENTATION_LAW]

[TWRAC_TOPOLOGY]:
- one data-attribute → one variant, via one fold: the plugin holds a table of RAC states (`attributes.boolean`, `attributes.enum`) and runs a single `addVariant` loop registering `<name>:` → `&[data-<attr>]` (boolean) or `<name>-<value>:` → `&[data-<attr>="<value>"]` (enum). The variant vocabulary IS that data table — there is no per-state hand-written selector, and a new RAC state becomes available by an upstream row, not a mechanism change.
- the variant is the state binding: because RAC renders `isSelected`/`isPressed`/… as `data-*` attributes, the plugin lets styling read them declaratively — `className="selected:bg-accent pressed:scale-95"` replaces the render-prop `className={({isSelected,isPressed}) => …}` function. State-conditional styling collapses out of JS ternaries into the utility vocabulary.
- build-time and runtime-free: it is a Tailwind catalog-bound plugin evaluated during CSS compilation. A component never imports it; it emits no JS. Registration is the CSS `@plugin` directive, and the `prefix` option is the only knob.
- unprefixed variants collapse RAC + native: to avoid overriding native `:hover`/`:focus`/`:disabled` on non-RAC elements, the default (empty-prefix) build targets `[data-rac]` for the RAC branch and the native pseudo-class for the non-RAC branch inside one `:is()`, with `@media (hover: hover)` on hover — so `hover:`/`focus:`/`disabled:` are single names that work everywhere and stay composable with `group-*`/`peer-*`/`not-*`.

[INTEGRATION_LAW]:
- Stack with `react-aria-components` (`.api/react-aria-components.md`): this plugin is the styling counterpart of RAC — every RAC render-prop boolean (`isHovered`/`isSelected`/`isInvalid`/…) and enum (`placement`/`orientation`/`selectionMode`/…) has a matching variant, so a `view/primitive` row styles interaction state entirely in classes and never writes a render-prop `className`/`style` function.
- Stack with `tailwindcss@4` (`.api/tailwindcss.md`): registered via the catalog-bound `@plugin` directive in the `token` entry CSS; the variants compose with Tailwind's own (`selected:hover:…`, `md:pressed:…`) and with `group-*`/`peer-*` since RAC exposes group/peer data-attributes on parents.
- Stack with `tailwind-merge` (`.api/tailwind-merge.md`): the variants are modifiers `twMerge` preserves as conflict scopes with no config change, so RAC-state styling flows through the folder's one `cn = twMerge(clsx(...))` rail exactly like any variant; only an order-sensitive interaction needs `orderSensitiveModifiers`.
- Stack with `tw-animate-css` (`.api/tw-animate-css.md`): `entering:`/`exiting:` are the RAC transition phases — pair them with tw-animate enter/exit utilities (`entering:animate-in entering:fade-in exiting:animate-out exiting:fade-out`) so overlay/popover motion is declared in classes, driven by RAC's `data-entering`/`data-exiting`.
- Stack with `class-variance-authority` (`.api/class-variance-authority.md`): a `cva` variant's class string may include these state variants (`selected:` inside a `cva` slot), so a token-driven component still binds RAC state declaratively while `cva` selects the design variant.

[LOCAL_ADMISSION]:
- Register once via `@plugin` in the `token` entry CSS; never import the plugin from a component and never add it to a JS `plugins: []` array (catalog-bound style) under `tailwindcss@4`.
- Style RAC interaction state with variants (`selected:`/`pressed:`/`invalid:`/`placement-bottom:`); never re-implement the branch with a render-prop `className`/`style` function or a JS ternary when a variant exists.
- Pair `entering:`/`exiting:` with `tw-animate-css` for RAC overlay transitions; never hand-write keyframe state machines for enter/exit.
- Keep the variants flowing through the `cn`/`twMerge` rail like any modifier; never special-case them in class composition.
- Choose a `prefix` only when an unprefixed name genuinely collides; otherwise keep the native-collapse default so `hover:`/`focus:`/`disabled:` cover RAC and native elements alike.

[RAIL_LAW]:
- Package: `tailwindcss-react-aria-components`
- Owns: the build-time mapping of react-aria-components state to Tailwind variants — the boolean state variants (interactive/selection/overlay-motion/field/drag/structural), the `name-value` enum variants (`placement`/`orientation`/`selection`/`sort`/`resizable`/`type`/`layout`), the catalog-bound `@plugin` registration + `prefix` option, and the unprefixed native-collapse selector emission
- Accept: CSS-first `@plugin` registration, declarative variant styling of RAC state, `entering:`/`exiting:` + tw-animate for transitions, composition through the `cn`/`twMerge` rail and with `cva`/`group-*`/`peer-*`, the unprefixed native-collapse default
- Reject: importing the plugin from a component, a JS `plugins: []` registration under catalog-bound, render-prop `className`/`style` functions where a variant exists, hand-written enter/exit keyframe machines, and a needless `prefix` that breaks the native-collapse of `hover`/`focus`/`disabled`
