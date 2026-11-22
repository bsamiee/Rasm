# [INTERFACE]

## [01]-[NATIVE_TOAST]

Chrome's own confirmation ("Link copied" after app menu Copy link) is a views bubble `ToastController` owns, and no extension API reaches it. Page UI copies its fixed values:

| [INDEX] | [PROPERTY] | [VALUE]                                                                     | [CHROMIUM_SOURCE]                 |
| :-----: | :--------- | :-------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | Height     | 48px, radius 48px, a full pill                                              | `DISTANCE_TOAST_BUBBLE_HEIGHT`    |
|  [02]   | Padding    | 12px block, 14px start before icon, 16px end with no button                 | `TOAST_BUBBLE_MARGIN_*`           |
|  [03]   | Icon       | 20px, 6px from icon edge to text                                            | `TOAST_BUBBLE_ICON_SIZE`          |
|  [04]   | Type       | `font: 400 13px / 24px system-ui`, one line, ellipsis                       | `CONTEXT_TOAST_BODY_TEXT`         |
|  [05]   | Lifetime   | 4s, 8s with an action button, never paused by Chrome                        | `kToastDefaultTimeout`            |
|  [06]   | Placement  | Top center, 6px below content edge, 4px minimum side margin                 | `ToastView::GetBubbleBounds`      |
|  [07]   | Narrow     | Label shrinks first, width capped at viewport minus 8px                     | `ToastView::Init`                 |
|  [08]   | Shadow     | `0 2px 4px` at alpha `0x33` and `0 2px 6px` at `0x1A` of `kColorShadowBase` | `bubble_border.cc`                |
|  [09]   | Role       | Window role `alert`, text as accessible title                               | `SetAccessibleWindowRole(kAlert)` |
|  [10]   | Text       | Chrome string table wording, localized                                      | `IDS_LINK_COPIED_TOAST_BODY`      |

`kColorShadowBase` is `#3C4043` in light mode and black in dark mode, so shadows read `light-dark(rgb(60 64 67 / 20%), rgb(0 0 0 / 20%))` and its 10% pair. WebUI `cr-toast` (Settings, History, Downloads) is a bottom-start variant: radius 8px, `min-height: 52px`, `min-width: 288px`, `role="alert"` set after render so repeated text announces.

## [02]-[COLOR]

Chrome maps Material 3 system tokens to tones of a user's palette, and its toast is an inverse surface:

| [INDEX] | [TOKEN]                          | [LIGHT]    | [DARK]     | [BASELINE]            | [CSS]                                  |
| :-----: | :------------------------------- | :--------- | :--------- | :-------------------- | :------------------------------------- |
|  [01]   | `kColorSysInverseSurface`        | Neutral 20 | Neutral 90 | `#303030` / `#E3E3E3` | `light-dark(lab(20 0 0), lab(90 0 0))` |
|  [02]   | `kColorSysInverseOnSurface`      | Neutral 95 | Neutral 10 | `#F2F2F2` / `#1F1F1F` | `light-dark(lab(95 0 0), lab(10 0 0))` |
|  [03]   | `kColorSysInverseSurfacePrimary` | Primary 20 | Primary 95 | `#062E6F` / `#ECF3FE` | None, seed hue unreadable              |

Material tone is CIELAB L*, so `lab(<tone> 0 0)` states a neutral tone exactly. Page UI takes color from its environment alone:
- `color-scheme: light dark` follows Chrome's Light, Dark, or Device setting in regular tabs, Incognito follows the device, and Page colors overrides both
- `:host { all: initial }` resets `color-scheme` to `normal`, so the popover declares `color-scheme: light dark` again
- `if(media(prefers-contrast: more): CanvasText; else: ...)` swaps surface and text to `CanvasText` on `Canvas` under increased contrast
- `@media (forced-colors: active)` adds `border: 1px solid CanvasText`, as forced colors drop shadows and repaint surface and text
- Seed-tinted variants (`Primary` tones) stay out, as no API exposes a theme seed and a baseline hex turns wrong once themed
- `chrome://theme/colors.css` serves WebUI and `devtools://` alone, and a fetch from an extension page fails
- No `theme` extension API exists in Chrome (`chrome.theme` is Firefox's), and `action` exposes badge, icon, title, and popup alone
- Brand color belongs to action icon and store listing, and page UI carries no accent

System color keywords hold fixed values per scheme, never a Chrome theme:

| [INDEX] | [KEYWORD]     | [LIGHT]          | [DARK]    | [CONDITION]                                                   |
| :-----: | :------------ | :--------------- | :-------- | :------------------------------------------------------------ |
|  [01]   | `Canvas`      | `#FFFFFF`        | `#121212` | Forced-color palettes replace it                              |
|  [02]   | `CanvasText`  | `#000000`        | `#FFFFFF` | Forced-color palettes replace it                              |
|  [03]   | `ButtonFace`  | `#EFEFEF`        | `#6B6B6B` | Fixed                                                         |
|  [04]   | `AccentColor` | `rgb(0 117 255)` | Same      | OS accent in an installed web app on the initial profile only |

`contrast-color()` returns pure black or white where Chrome uses tones 95 and 10, so text color comes from the token pair.

## [03]-[STRUCTURE]

Injected UI is one element under `document.documentElement`, built once per document and reused by every later injection:
- Host `<name>-toast` holds an open shadow root and one adopted `CSSStyleSheet` filled by `replaceSync(css)` from a CSS string argument
- Host stays an undefined element named with a hyphen, as `attachShadow` needs no `customElements.define`
- `:host { all: initial !important }` keeps page CSS (`* { all: unset !important }`, a hidden `:root`) off the host
- `popover="manual"` renders in the top layer, above page stacking, transforms, `overflow`, and fullscreen elements
- `popover="hint"` closes a page's own hint popovers and answers light dismiss and Esc, so page UI uses `manual`
- Popovers and modal dialogs a page opens later stack above the toast
- `pointer-events: none` and `user-select: none` on the pill pass clicks through, and a control inside sets `pointer-events: auto`
- `<output>` with `ariaLive = 'off'` holds text, and `ariaNotify` announces it
- Icons are Material Symbols SVGs drawn as a CSS `mask` over `currentColor`, static ones as a `url()` Vite inlines, runtime ones as `--glyph`
- Markup builds through `createElement`, `textContent`, and `replaceChildren`, and `appendHTML` (157) takes trusted markup with no `innerHTML`
- Page scripts read an open shadow root, so page UI shows nothing a page must not see, and typed or secret input goes in an extension-page iframe
- Host leaves the document once exit finishes: no running animation and `:popover-open` false at `transitionend`

Elements and handlers reuse what a document holds, so a second injection restarts its toast:

```ts
const host = document.querySelector('<name>-toast') ?? document.createElement('<name>-toast');
const root = host.shadowRoot ?? host.attachShadow({ mode: 'open' });
const output = root.querySelector('output') ?? document.createElement('output');
Object.assign(output, { popover: 'manual', onkeydown: closing, onanimationend: closing });
```

`closing` is the close handler. Handlers set as properties replace each other, where `addEventListener` stacks one per injection.

## [04]-[MOTION]

Chrome's toast animates frame and content on Material 3 emphasized curves, and renders none under reduced motion:

| [INDEX] | [PHASE] | [PROPERTIES]                                 | [TIMING]                     | [CURVE]                           |
| :-----: | :------ | :------------------------------------------- | :--------------------------- | :-------------------------------- |
|  [01]   | Entry   | `translate: 0 -50px`, `scale: 1 0.5` to none | 300ms                        | `cubic-bezier(0.05, 0.7, 0.1, 1)` |
|  [02]   | Entry   | Frame opacity, then content opacity          | 50ms, then 150ms after 50ms  | Linear                            |
|  [03]   | Exit    | `translate`, `scale` back to start           | 150ms                        | `cubic-bezier(0.3, 0, 0.8, 0.15)` |
|  [04]   | Exit    | Content opacity, then frame opacity          | 100ms, then 50ms after 100ms | Linear                            |

CSS states each phase with no script timer:
- `transition` on `:popover-open` times entry and on `[popover]` times exit, as a destination style's transition applies
- `@starting-style` inside `:popover-open` holds the entry start state
- `display <ms> allow-discrete` and `overlay <ms> allow-discrete` keep a closing popover rendered and in the top layer
- `@property --hold { syntax: '<number>'; inherits: false; initial-value: 0 }` with `animation: hold linear` times lifetime
- `animationDuration` set per injection passes 4s or 8s, and `animationend` calls `hidePopover()`
- Second injection while open sets each `CSSAnimation`'s `currentTime` to 0, as Chrome's controller restarts its timer
- `:is(:hover, :focus-within) { animation-play-state: paused }` holds a toast with a control while pointer or focus rests on it
- `@media (prefers-reduced-motion: reduce)` sets `transition: none`, reading an OS setting Chrome's views read alike
- Content fades through `color` from `transparent`, which an icon mask follows as `currentColor`

## [05]-[CONTROLS]

Controls inside page UI follow Chrome's toast with an action and Material 3 state layers:
- Hit area 44 by 48px at the pill's end, visual circle 32px inset 8px, glyph 20px, no divider
- State layer `color-mix(in srgb, currentColor <n>%, transparent)` at 8% on hover, 10% on focus and press
- Focus ring `outline: 2px solid currentColor` with `outline-offset: -1px` on `:focus-visible`, `ButtonText` under forced colors
- `button { all: unset }` clears page and UA styles, `cursor: default` matching Chrome UI
- `ariaLabel` from `_locales`, as an icon button holds no text
- Controls with side effects act on `event.isTrusted` alone, so page script cannot press them
- Pill padding drops at a control's end (`&:has(button) { padding-inline-end: 0 }`)

## [06]-[TYPE]

`system-ui` resolves to SF on macOS and Segoe UI on Windows, Chrome UI's own fonts. `ui-sans-serif` is unsupported in Chrome and falls back to an inherited family, and named system families (`SF Pro`) are unreachable on macOS. Linux WebUI puts Roboto first, so Linux renders differ.

## [07]-[ACCESSIBILITY]

Announcements and focus follow the page, never trap it:
- `document.ariaNotify(text)` after `togglePopover(true)` announces at `priority: "normal"`, matching `role="status"`, with no live region
- Live regions inserted holding text, or shown from `display: none`, announce unreliably
- Modal `<dialog>` makes every node outside it inert, so a toast leaves the accessibility tree while `ariaNotify` speaks
- Pages blocking `aria-notify` by Permissions Policy silence `ariaNotify` with no error
- Focus entering a control records `relatedTarget`, and close returns focus there only while focus sits inside the toast
- Escape closes a toast only while focus sits inside it, and page keys pass through otherwise
- Focus arriving from outside a page has no `relatedTarget`, and close leaves focus on its body

## [08]-[ZOOM]

Chrome UI ignores page zoom. Service workers read `chrome.tabs.getZoom(tabId)` and pass it in, and the popover sets `style.zoom = 1 / zoom` to keep its device-independent size.
