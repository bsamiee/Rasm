# [TS_UI_API_TAILWINDCSS_TYPOGRAPHY]

`@tailwindcss/typography` owns the prose plane: one `prose` class styles a whole tree of unclassed HTML — headings, lists, tables, code, quotes, figures — from an em-relative rhythm, and eighteen `--tw-prose-*` custom properties carry every color it paints. Thirty-three element modifiers reach any tag inside that tree as an ordinary variant, so an override is a utility rather than a stylesheet rule.

Its selectors nest under `:where()` at zero specificity and exclude the `not-prose` subtree, so any utility class on a child wins without `!important` and any region opts out whole.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the color registers — eighteen properties, each mirrored by a `--tw-prose-invert-*` twin

Every color the plugin paints reads one of these variables, so the token bridge overrides the set once at the `prose` element and the whole tree follows. `prose-invert` reassigns each base variable to its `invert-` twin, which is why a dark-mode palette writes the `invert-` half and swaps by class.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------ | :------------- | :--------------------------------------------------------- |
|  [01]   | `--tw-prose-body`                                 | color register | body text and every unstyled inline                        |
|  [02]   | `--tw-prose-headings`                             | color register | `h1`–`h6` and `th`                                         |
|  [03]   | `--tw-prose-lead`                                 | color register | the `[class~="lead"]` opening paragraph                    |
|  [04]   | `--tw-prose-links`                                | color register | anchor text; the one hue a color modifier sets alone       |
|  [05]   | `--tw-prose-bold`                                 | color register | `strong` and nested-context bolds                          |
|  [06]   | `--tw-prose-counters` / `--tw-prose-bullets`      | color register | ordered-list markers and unordered bullets                 |
|  [07]   | `--tw-prose-hr`                                   | color register | the horizontal rule                                        |
|  [08]   | `--tw-prose-quotes` / `--tw-prose-quote-borders`  | color register | blockquote text and its leading border                     |
|  [09]   | `--tw-prose-captions`                             | color register | `figcaption` and table captions                            |
|  [10]   | `--tw-prose-kbd` / `--tw-prose-kbd-shadows`       | color register | `kbd` glyph color and its key-cap shadow RGB triple        |
|  [11]   | `--tw-prose-code`                                 | color register | inline `code` outside a `pre`                              |
|  [12]   | `--tw-prose-pre-code` / `--tw-prose-pre-bg`       | color register | code-block foreground and its surface                      |
|  [13]   | `--tw-prose-th-borders` / `--tw-prose-td-borders` | color register | header-row and body-row table rules                        |
|  [14]   | `--tw-prose-invert-*`                             | color register | the full eighteen-member dark twin `prose-invert` swaps in |

[PUBLIC_TYPE_SCOPE]: the plugin option record

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `className` (default `'prose'`)   | struct        | renames the base class, its size/color modifiers, and the `not-prose` escape |
|  [02]   | `target` (`'modern' \| 'legacy'`) | union         | `'modern'` emits the zero-specificity `:where()` selectors                   |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: loading the plugin and the base class

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                                            |
| :-----: | :----------------------------------- | :------ | :---------------------------------------------------------------------- |
|  [01]   | `@plugin "@tailwindcss/typography";` | fold    | registers the variants and component classes into the CSS-first compile |
|  [02]   | `prose`                              | fold    | the base class — sets every color register and the whole element rhythm |
|  [03]   | `not-prose`                          | fold    | excludes its element and subtree from every generated selector          |

[ENTRYPOINT_SCOPE]: the size and theme modifiers — one modifier per generated component class

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------------- | :------ | :----------------------------------------------------------- |
|  [01]   | `prose-sm` `prose-base` `prose-lg` `prose-xl` `prose-2xl` | fold    | rescales root font size, line height, and every em margin    |
|  [02]   | `prose-slate` `prose-gray` `prose-zinc` `prose-neutral`   | fold    | four of the five neutral ramps; each writes every register   |
|  [03]   | `prose-stone`                                             | fold    | the fifth neutral ramp                                       |
|  [04]   | `prose-red` through `prose-rose`                          | fold    | the accent hues; each rewrites `--tw-prose-links` alone      |
|  [05]   | `prose-invert`                                            | fold    | points every base register at its `--tw-prose-invert-*` twin |

[ENTRYPOINT_SCOPE]: the element modifiers — 33 variants reaching one tag family inside the prose tree

Each spells `prose-<name>:<utility>` and compiles to `& :is(:where(<selector>):not(:where([class~="not-prose"], [class~="not-prose"] *)))`, so a modifier composes with every other Tailwind variant and stacks under `hover:`, `dark:`, or an arbitrary selector.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `prose-headings:`                                                       | fold    | the `h1`–`h6` + `th` cohort in one variant           |
|  [02]   | `prose-h1:` `prose-h2:` `prose-h3:` `prose-h4:` `prose-h5:` `prose-h6:` | fold    | one heading level each                               |
|  [03]   | `prose-p:` `prose-lead:` `prose-a:` `prose-strong:` `prose-em:`         | fold    | body text, the `lead` paragraph, links, and emphasis |
|  [04]   | `prose-blockquote:` `prose-hr:`                                         | fold    | quote block and rule                                 |
|  [05]   | `prose-ol:` `prose-ul:` `prose-li:` `prose-dl:` `prose-dt:` `prose-dd:` | fold    | both list families and definition lists              |
|  [06]   | `prose-code:` `prose-pre:` `prose-kbd:`                                 | fold    | inline code, code block, and key caps                |
|  [07]   | `prose-table:` `prose-thead:` `prose-tr:` `prose-th:` `prose-td:`       | fold    | the table cohort at every grain                      |
|  [08]   | `prose-img:` `prose-picture:` `prose-video:` `prose-figure:`            | fold    | media and its frame                                  |
|  [09]   | `prose-figcaption:`                                                     | fold    | the figure caption                                   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every generated selector wraps in `:where()` and excludes the `not-prose` subtree, so the whole plane carries zero specificity: a utility class on any child overrides the prose rule with no `!important` and no ordering fight, and `not-prose` on a wrapper drops its entire subtree out of the plane for a rendered component, table widget, or code viewer.
- `prose` assigns every register and each rule reads `var(--tw-prose-*)`, so overriding the set at the `prose` element retints the whole tree, and the token bridge writes the OKLCH palette into those registers once while the shipped neutral ramps go unused.
- `prose-invert` is a variable remap rather than a second stylesheet — it points each base register at its `--tw-prose-invert-*` twin — so a dark palette supplies the `invert-` half of the set and the class does the swap.
- Sizing is one em-relative system: `prose-sm` through `prose-2xl` rescale the root `font-size` and `line-height`, and every margin, padding, and inset size in the plane is authored in `em`, so one modifier moves the whole rhythm coherently.
- Color modifiers split by reach: the five neutral ramps write all eighteen registers, while the seventeen accent hues write `--tw-prose-links` and its invert twin alone, so `prose-slate prose-sky` composes a body ramp with a link accent.
- `prose` sets a `65ch` measure; `max-w-none` releases it wherever the surrounding layout owns width.
- `className` renames the base class, every size and color modifier, the element-variant prefix, and the `not-prose` escape together, so a rename lands total or not at all.

[STACKING]:
- `tailwindcss` (`.api/tailwindcss.md`): `@plugin "@tailwindcss/typography"` after `@import "tailwindcss"` in the theme entry registers the variants and component classes into the single CSS-first compile — the same wire directive `tailwindcss-react-aria-components` rides.
- `colorjs.io` (`.api/colorjs.io.md`): the token bridge computes each register in OKLCH and emits it through `serialize()`, checking text pairs with `contrast()` and deriving the invert half through `range()`/`steps()`, so the `--tw-prose-*` set and every `@theme --color-*` token descend from one palette authority.
- `tailwind-merge` (`.api/tailwind-merge.md`): the one shared `extendTailwindMerge(ConfigExtension)` instance declares the `prose` size and color groups so `twMerge` resolves them last-wins; without those groups two competing `prose-lg`/`prose-sm` classes both survive the `cn` fold.
- `isomorphic-dompurify` (`.api/isomorphic-dompurify.md`): `DOMPurify.sanitize` bounds the plane — the plugin styles whatever tags arrive and enforces nothing, so the sanitizer's allowed-tag set decides which of the element modifiers can ever fire on remote or user-authored HTML.
- `prosemirror-view` (`.api/prosemirror-view.md`): `EditorView`'s content DOM mounts as a `prose` container, so authoring and reading render off one vocabulary; a `NodeView` rendering its own chrome carries `not-prose` and the document text it wraps stays inside the plane.
- `lucide-react` (`.api/lucide-react.md`): inline glyphs inside prose ride `not-prose` or a `prose-img:` override, since the plane's `img` rules set block display and vertical rhythm an inline icon must not inherit.

[LOCAL_ADMISSION]:
- Load the plugin through `@plugin` in the theme entry; nothing imports it from TypeScript.
- Assign the `--tw-prose-*` registers from the estate palette at the `prose` element and leave the shipped ramps unreferenced.
- Reach for an element modifier before authoring a descendant selector; the variant composes with every other Tailwind variant and the raw selector does not.
- Mark every embedded component, widget, or rendered fence `not-prose` rather than unwinding the plane's rules on it.
- Keep `target: 'modern'`; the legacy selectors carry real specificity and re-open the override fight the plane exists to end.
