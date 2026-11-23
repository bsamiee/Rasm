# [APPEARANCE]

## [01]-[TYPE]

Chrome's own pages set `body { font-size: 81.25% }`, 13px at a 16px default, in each OS's UI font:
- `system-ui, sans-serif` on macOS
- `'Segoe UI', Tahoma, sans-serif` on Windows
- `Roboto`, then its system UI font, then `Arial` on Linux
- `Roboto, sans-serif` on ChromeOS

`system-ui` resolves to each OS's UI font, and `system-ui, sans-serif` matches macOS and Windows and differs on Linux, where Chrome loads its own Roboto.

`ui-sans-serif` is unsupported and falls back to an inherited family, and named system families (`SF Pro`) are unreachable on macOS.

Text takes a size, line, and weight by role:

| [INDEX] | [ROLE]                       | [SIZE] | [LINE] |    [WEIGHT]     |
| :-----: | :--------------------------- | :----: | :----: | :-------------: |
|  [01]   | Body, dialog body, button    |  13px  |  20px  | 400, button 500 |
|  [02]   | Section title                |  14px  | Normal |       500       |
|  [03]   | Dialog title                 |  15px  |  15px  |    Inherited    |
|  [04]   | Side panel heading, tab      |  14px  |  20px  |       500       |
|  [05]   | Side panel empty state title |  16px  |  24px  |       500       |
|  [06]   | Input value, select, chip    |  12px  |  16px  |  400, chip 500  |
|  [07]   | Input label, error, metadata |  11px  |  16px  |       400       |
|  [08]   | Page toolbar title           |  22px  | Normal |       500       |

Chrome draws toasts, bubbles, side panel headers, and badges through Views, at fixed sizes a Font size setting never changes: body 13px on 20px, dialog title 15px on 22px, and side panel header title 14px on 20px at weight 500.

## [02]-[COLOR]

Chrome maps system tokens to tones of one palette, each at its value with no theme color set:

| [INDEX] | [TOKEN]                 | [LIGHT]   | [DARK]    | [CSS]                                                                    |
| :-----: | :---------------------- | :-------- | :-------- | :----------------------------------------------------------------------- |
|  [01]   | Surface                 | `#FFFFFF` | `#1F1F1F` | `light-dark(oklch(100% 0 0deg), oklch(23.93% 0 0deg))`                   |
|  [02]   | Surface 1               | `#F8FAFD` | `#27282A` | `light-dark(oklch(98.44% 0.005 258.3deg), oklch(27.66% 0.004 264.5deg))` |
|  [03]   | Surface 2               | `#F3F6FC` | `#2D2F31` | `light-dark(oklch(97.26% 0.009 264.5deg), oklch(30.4% 0.005 248deg))`    |
|  [04]   | Base container          | `#EDF2FA` | `#282828` | `light-dark(oklch(95.97% 0.012 259.8deg), oklch(27.68% 0 0deg))`         |
|  [05]   | Base container elevated | `#FFFFFF` | `#3C3C3C` | `light-dark(oklch(100% 0 0deg), oklch(35.62% 0 0deg))`                   |
|  [06]   | Neutral container       | `#F2F2F2` | `#282828` | `light-dark(oklch(96.12% 0 0deg), oklch(27.68% 0 0deg))`                 |
|  [07]   | Surface variant         | `#E1E3E1` | `#444746` | `light-dark(oklch(91.37% 0.003 145.5deg), oklch(39.48% 0.004 174.3deg))` |
|  [08]   | On surface              | `#1F1F1F` | `#E3E3E3` | `light-dark(oklch(23.93% 0 0deg), oklch(91.58% 0 0deg))`                 |
|  [09]   | On surface subtle       | `#474747` | `#C7C7C7` | `light-dark(oklch(39.79% 0 0deg), oklch(82.97% 0 0deg))`                 |
|  [10]   | On surface variant      | `#444746` | `#C4C7C5` | `light-dark(oklch(39.48% 0.004 174.3deg), oklch(82.67% 0.004 157.2deg))` |
|  [11]   | Primary                 | `#0B57D0` | `#A8C7FA` | `light-dark(oklch(49.49% 0.199 260.6deg), oklch(82.48% 0.08 260.3deg))`  |
|  [12]   | On primary              | `#FFFFFF` | `#062E6F` | `light-dark(oklch(100% 0 0deg), oklch(32.06% 0.121 260.2deg))`           |
|  [13]   | Tonal container         | `#D3E3FD` | `#004A77` | `light-dark(oklch(91.19% 0.04 260.1deg), oklch(39.48% 0.099 244.6deg))`  |
|  [14]   | On tonal container      | `#041E49` | `#C2E7FF` | `light-dark(oklch(24.62% 0.087 259.7deg), oklch(90.92% 0.051 236.6deg))` |
|  [15]   | Outline                 | `#747775` | `#8E918F` | `light-dark(oklch(56.6% 0.005 157.1deg), oklch(65.35% 0.005 157.1deg))`  |
|  [16]   | Neutral outline         | `#C7C7C7` | `#757575` | `light-dark(oklch(82.97% 0 0deg), oklch(56.24% 0 0deg))`                 |
|  [17]   | Tonal outline           | `#A8C7FA` | `#047DB7` | `light-dark(oklch(82.48% 0.08 260.3deg), oklch(56.18% 0.1268 239.6deg))` |
|  [18]   | Divider                 | `#D3E3FD` | `#5E5E5E` | `light-dark(oklch(91.19% 0.04 260.1deg), oklch(48.19% 0 0deg))`          |
|  [19]   | Error                   | `#B3261E` | `#F2B8B5` | `light-dark(oklch(50.13% 0.178 28.7deg), oklch(83.45% 0.068 22deg))`     |
|  [20]   | Inverse surface         | `#303030` | `#E3E3E3` | `light-dark(oklch(30.92% 0 0deg), oklch(91.58% 0 0deg))`                 |
|  [21]   | Inverse on surface      | `#F2F2F2` | `#1F1F1F` | `light-dark(oklch(96.12% 0 0deg), oklch(23.93% 0 0deg))`                 |
|  [22]   | Inverse surface primary | `#062E6F` | `#ECF3FE` | `light-dark(oklch(32.06% 0.121 260.2deg), oklch(96.21% 0.017 259.4deg))` |
|  [23]   | Inverse primary         | `#A8C7FA` | `#0B57D0` | `light-dark(oklch(82.48% 0.08 260.3deg), oklch(49.49% 0.199 260.6deg))`  |
|  [24]   | Shadow base             | `#3C4043` | `#000000` | `light-dark(oklch(36.88% 0.007 240deg), oklch(0% 0 0deg))`               |

Each CSS form renders the bytes of its hex. Chrome's neutral tones are fixed hex that `lab(<tone>% 0 0)` misses at Neutral 10, 90, and 95 by up to 4 levels.

State layers and scrims are text or black at an alpha:

| [INDEX] | [STATE]                   | [VALUE]                                                  |
| :-----: | :------------------------ | :------------------------------------------------------- |
|  [01]   | Hover                     | On surface at 7.8%                                       |
|  [02]   | Pressed, focus            | On surface at 11.8%                                      |
|  [03]   | Hover on a filled control | Near-white at 10.2% in light, near-black at 5.9% in dark |
|  [04]   | Disabled content          | On surface at 38%                                        |
|  [05]   | Disabled container        | On surface at 12%                                        |
|  [06]   | Scrim                     | Black at 60%                                             |

Chrome's pages take tokens by role:
- Pages, dialogs, bubbles, and menus sit on surface, and cards on surface in light and surface 1 in dark
- Side panels sit on base container around cards of base container elevated, with divider lines and tonal outline scrollbar thumbs
- Body text takes on surface, secondary text on surface subtle, and links and selected tabs primary
- Text fields fill with surface variant over a 1px outline underline, and outlined ones take a neutral outline border

Theme colors a user sets regenerate every palette from their seed, and themed Chrome then differs from each baseline value an extension keeps.

System colors ignore a user's Chrome theme: `AccentColor` stays `rgb(0 117 255)`, and `Canvas` is `#121212` in dark where Chrome's surface is `#1F1F1F`.

## [03]-[SHAPE]

Corners and spacing hold fixed values:

| [INDEX] | [ITEM]                                 | [VALUE]                                                     |
| :-----: | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Button                                 | 100px corners                                               |
|  [02]   | Card, dialog, text field, select, chip | 8px corners                                                 |
|  [03]   | Side panel card and dialog             | 12px corners                                                |
|  [04]   | Menu in a page                         | 4px corners                                                 |
|  [05]   | Checkbox                               | 2px corners                                                 |
|  [06]   | Centered card                          | 680px at most, 96% wide                                     |
|  [07]   | Row                                    | 48px tall, 64px with 2 lines, 20px side padding             |
|  [08]   | Side panel body, card, gap             | 8px padding, `8px 16px` padding, 12px gap                   |
|  [09]   | Dialog                                 | 512px wide, `20px 20px 16px` title padding, 16px button row |
|  [10]   | Toolbar of a page                      | 56px tall                                                   |

Elevation pairs a key shadow at 30% of shadow base with an ambient one at 15%:

| [INDEX] | [LEVEL]              | [KEY]         | [AMBIENT]       |
| :-----: | :------------------- | :------------ | :-------------- |
|  [01]   | 1                    | `0 1px 2px 0` | `0 1px 3px 1px` |
|  [02]   | 2, cards and menus   | `0 1px 2px 0` | `0 2px 6px 2px` |
|  [03]   | 3, side panel dialog | `0 1px 3px 0` | `0 4px 8px 3px` |

Dialogs in pages take `0 0 16px` black at 12% and `0 16px 16px` black at 24%.

Side panel scrollbars are 8px wide with a 100px-cornered thumb on a transparent track and `scrollbar-gutter: stable`.

## [04]-[CONTROLS]

Chrome's page controls hold fixed sizes and states:

| [INDEX] | [CONTROL]        | [SIZE]                                                | [STATES]                                                   |
| :-----: | :--------------- | :---------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Button, outlined | 36px tall, `8px 16px`, at least 5.14em wide           | Primary text, 1px tonal outline border, hover state layer  |
|  [02]   | Button, filled   | Same                                                  | Primary fill, on primary text, filled hover state layer    |
|  [03]   | Button, tonal    | Same                                                  | Tonal container fill, on tonal container text              |
|  [04]   | Button with icon | 20px icon, 8px gap, 12px padding on its side          | Same as its kind                                           |
|  [05]   | Icon button      | 32px circle, 20px icon, 24px and 16px in a side panel | Hover state layer, 2px inset focus ring, 38% when disabled |
|  [06]   | Toggle           | 26 by 16px track, 8px knob, 12px when on              | Primary track when on, outline border when off             |
|  [07]   | Checkbox, radio  | 16px box, 2px border, 20px to its label               | Primary fill when checked, 32px hover circle               |
|  [08]   | Text field       | 10px padding                                          | 2px primary underline on focus, error color when invalid   |
|  [09]   | Select           | 36px tall, 200px wide, 10px side padding              | 1px neutral outline border, 2px focus ring at -1px         |
|  [10]   | Slider           | 32px tall, 2px track, 10px knob                       | Primary active track, 12px value label                     |
|  [11]   | Tabs             | 48px tall, 2px indicator                              | Primary selected text and indicator                        |
|  [12]   | Chip             | 28px tall, 8px side padding, 4px gap, 16px icon       | Tonal outline border, tonal container when selected        |
|  [13]   | Menu item        | 32px tall at least, `8px 24px`                        | Hover state layer on focus, 2px ring at -2px               |
|  [14]   | Tooltip          | `8px` padding, 12px text, 330px at most               | `#616161` at 90% under white text, 500ms fade              |
|  [15]   | Progress         | 200px wide, 4px tall                                  | `#34A853` bar                                              |

Controls share focus, forced colors, and icon rules:
- Focus rings are `2px solid` primary, and inverse primary on an inverse surface
- Focus rings sit at 2px offset on buttons, checkboxes, toggles, and chips, and inset on rows and menu items
- Forced colors swap disabled content to `GrayText`, selection to `Highlight` and `HighlightText`, and icon masks to `ButtonText`
- Native checkboxes, radios, and ranges default to accent `rgb(0 117 255)`, and `accent-color` set to primary recolors them

## [05]-[CONTAINERS]

Chrome draws a container for each extension page and for its install prompt:

| [INDEX] | [CONTAINER]      | [GEOMETRY]                                                                       |
| :-----: | :--------------- | :------------------------------------------------------------------------------- |
|  [01]   | Action popup     | 25 by 25px to 800 by 600px, standard bubble shadow                               |
|  [02]   | Side panel       | Content at least 360px wide, 8px corners, 8px base container border, 40px header |
|  [03]   | Embedded options | At least 400px wide, at most 640px or 90% of window height                       |
|  [04]   | Install prompt   | 448px wide, 32px icon, permission list scrolling past 192px                      |

Chrome fills each container by its own rule:
- Popups show a page's own canvas with no background from Chrome
- Side panel headers hold icon, name, pin, and close
- Side panels open on a browser's right unless its user moved them, and `sidePanel.getLayout()` reads which
- Embedded options pages open in a dialog on `chrome://extensions` titled by icon and name
- Install prompts list each install warning

Toolbar actions and badges hold fixed sizes:
- Action icons draw at 16px in a 28px area, from the 16 and 32px images of `action.default_icon` or `icons`
- Badges are 14px tall with 4px corners, 9px bold text, and 2px side padding, at the icon's bottom end
- Badge text clips past 23px of width, about 4 characters
- Badge background defaults to Chrome's themed neutral container, and unset text color takes black or white, whichever contrasts more with it

## [06]-[TOAST]

Chrome's toast, shown for "Link copied" and each confirmation of its own, holds fixed values:

| [INDEX] | [PROPERTY]  | [VALUE]                                                                                         |
| :-----: | :---------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | Shape       | 48px tall with 48px corners                                                                     |
|  [02]   | Padding     | 14px before its icon, 16px after its text                                                       |
|  [03]   | Icon        | 20px, 6px before its text                                                                       |
|  [04]   | Type        | 13px on a 24px line at weight 400, one line                                                     |
|  [05]   | Fill        | Inverse surface primary under inverse on surface text                                           |
|  [06]   | Icon button | 28px circle around a 20px icon, 4px after text, 10px before its end                             |
|  [07]   | Text button | 36px tall in inverse primary, 16px after text, 6px before its end                               |
|  [08]   | Lifetime    | 4s, 8s with a text button or menu                                                               |
|  [09]   | Placement   | Top center, straddling content's top edge, 6px below it in fullscreen                           |
|  [10]   | Shadow      | `0 2px` shadows at 20% and at 10% of shadow base on Windows and Linux, window shadow on macOS   |
|  [11]   | Entry       | 50px slide down and 50% vertical scale on emphasized entry                                      |
|  [12]   | Exit        | Same path back on emphasized exit                                                               |
|  [13]   | Fades       | Container over 50ms then content over 150ms on entry, content 100ms then container 50ms on exit |

Toasts render no motion under reduced motion and announce as an alert.

Toasts inside Chrome's own pages differ: bottom start, 8px corners, at least 52px tall and 288px wide, 300ms slide, and inverse surface fill under inverse on surface text and inverse primary buttons.

## [07]-[MOTION]

Chrome's pages and Views UI animate each change on a fixed curve:

| [INDEX] | [USE]                           | [DURATION]     | [CURVE]                                                    |
| :-----: | :------------------------------ | :------------- | :--------------------------------------------------------- |
|  [01]   | Toggle, checkbox, button shadow | 80ms           | `linear`                                                   |
|  [02]   | Icon button fill                | 150ms          | `ease-in-out`                                              |
|  [03]   | Checkmark                       | 140ms          | `ease-out`                                                 |
|  [04]   | Tab indicator                   | 150ms, 180ms   | `cubic-bezier(0.4, 0, 1, 1)`, `cubic-bezier(0, 0, 0.2, 1)` |
|  [05]   | Collapse                        | 300ms          | `ease-out`                                                 |
|  [06]   | Standard move                   | 150ms          | `cubic-bezier(0.4, 0, 0.2, 1)`                             |
|  [07]   | Emphasized entry                | 300ms          | `cubic-bezier(0.05, 0.7, 0.1, 1)`                          |
|  [08]   | Emphasized exit                 | 150ms          | `cubic-bezier(0.3, 0, 0.8, 0.15)`                          |
|  [09]   | Ripple                          | 800ms at least | `cubic-bezier(0.2, 0.9, 0.1, 0.9)`                         |

Chrome's pages stop animating under `prefers-reduced-motion: reduce`, and Views UI reads the same OS setting.
