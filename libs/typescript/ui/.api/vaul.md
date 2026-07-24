# [TS_UI_API_VAUL]

`vaul` owns the drag-dismissable drawer/sheet: the `Drawer` compound wraps a `@radix-ui/react-dialog` and layers pointer-drag translation, snap-point detents, velocity dismiss past `closeThreshold`, four-way `direction`, and background scale over it — Radix owns the accessible modal semantics, vaul owns the pointer math. It hosts the `view/compose` overlay sheet, distinct from the `@floating-ui/react` anchored-overlay rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vaul`
- package: `vaul` (MIT)
- module: ESM + CJS, single `.` entry (`dist/index.mjs`/`.js`, types `.d.mts`/`.d.ts`)
- runtime: browser React DOM client component; peer `react`/`react-dom`
- depends: `@radix-ui/react-dialog` — vaul IS a Radix Dialog, drag physics layered on top
- rail: `view/compose` overlay — the drag-dismissable sheet host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the drawer control surface — one prop object owns every sheet modality; `snapPoints` toggles the `WithFadeFromProps`/`WithoutFadeFromProps` discriminant, so only a snap-point sheet takes `fadeFromIndex`.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `DialogProps`                                | object        | the one `Drawer.Root` control object; roster below           |
|  [02]   | `WithFadeFromProps` / `WithoutFadeFromProps` | interface     | the snap-point fade discriminant                             |
|  [03]   | `ContentProps` / `HandleProps`               | type          | forward Radix `Content` props; `Handle` adds `preventCycle?` |

[DIALOG_PROPS]: `open: boolean` `defaultOpen: boolean` `onOpenChange(boolean) -> void` `onClose() -> void` `onAnimationEnd(boolean) -> void` `snapPoints: (number|string)[]` `activeSnapPoint: number|string|null` `setActiveSnapPoint(number|string|null) -> void` `fadeFromIndex: number` `snapToSequentialPoint: boolean` `closeThreshold: number` `scrollLockTimeout: number` `dismissible: boolean` `handleOnly: boolean` `onDrag(PointerEvent, number) -> void` `onRelease(PointerEvent, boolean) -> void` `direction: 'top'|'bottom'|'left'|'right'` `modal: boolean` `nested: boolean` `container: HTMLElement|null` `autoFocus: boolean` `shouldScaleBackground: boolean` `setBackgroundColorOnScale: boolean` `noBodyStyles: boolean` `disablePreventScroll: boolean` `repositionInputs: boolean` `preventScrollRestoration: boolean` `fixed: boolean`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the compound drawer — `Drawer.Root` owns the state machine, children are the parts, each a React `component`. Import `import { Drawer } from 'vaul'`; `Root`/`NestedRoot`/`Content`/`Overlay`/`Handle`/`Portal` are also top-level named exports, while `Trigger`/`Close`/`Title`/`Description` live on the `Drawer` namespace only (re-exported from Radix Dialog).

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Drawer.Root(DialogProps)` / `Drawer.NestedRoot` | sheet state machine; `NestedRoot` adds cumulative scale     |
|  [02]   | `Drawer.Trigger` / `Drawer.Close`                | toggle without controlling `open`                           |
|  [03]   | `Drawer.Portal` / `Drawer.Overlay`               | render into `container`; scrim fades from `fadeFromIndex`   |
|  [04]   | `Drawer.Content` / `Drawer.Handle(HandleProps)`  | draggable panel; `handleOnly` limits drag to `Handle`       |
|  [05]   | `Drawer.Title` / `Drawer.Description`            | `aria-labelledby`/`describedby`; required for the a11y tree |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Drawer.Root` wraps Radix `Dialog.Root` and adds pointer-drag translation, snap-point resolution, and velocity dismiss: Radix owns `role="dialog"`, `aria-modal`, the focus trap, escape/outside-press dismiss, and the portal, while vaul owns the pointer math — `onDrag` reports `percentageDragged`, and release past `closeThreshold` or a high velocity dismisses. `Drawer.Content` is the draggable panel, `Drawer.Handle` the drag affordance `handleOnly` makes the sole origin, `Drawer.Overlay` the scrim.
- `snapPoints` are detents — fractions `0..1` of screen or px strings; `activeSnapPoint`/`setActiveSnapPoint` control the current detent, `fadeFromIndex` the point the overlay begins fading, and `snapToSequentialPoint` disables velocity-skip so no detent is jumped. On release the sheet settles at the nearest snap by position and velocity.
- `open`/`onOpenChange` bind visibility to external state, and `dismissible={false}` requires a controlled `open` or the sheet traps open. `direction` is four-way (`'bottom'` default, `'top'`, `'left'`/`'right'`); `shouldScaleBackground` scales the `[vaul-drawer-wrapper]` root behind the sheet, and `repositionInputs` lifts focused inputs above the mobile keyboard.
- `Drawer.Title` + `Drawer.Description` satisfy Radix's labelling contract — omit `Title` and Radix warns and the sheet goes unlabeled for screen readers.

[STACKING]:
- `@radix-ui/react-dialog` (bundled dep, no separate catalog): vaul IS this Dialog — reach the `Trigger`/`Close`/`Title`/`Description` parts through `Drawer.*`, never a second `Dialog.Root` over one sheet (double focus trap).
- `@radix-ui/react-visually-hidden`(`.api/radix-ui-react-visually-hidden.md`): `VisuallyHidden` wraps a required `Drawer.Title` when the sheet has no visible heading, keeping the label in the accessibility tree unrendered.
- `cmdk`(`.api/cmdk.md`): a command palette in a sheet mounts a bare `Command` inside `Drawer.Content`, never `cmdk`'s `CommandDialog`, whose own portal and focus trap double vaul's.
- `@floating-ui/react`(`.api/floating-ui-react.md`): the overlay-class split — floating-ui owns anchored overlays through `useFloating`/`FloatingPortal`/`FloatingFocusManager`/`FloatingOverlay`, and vaul takes portal, focus-trap, and scroll-lock (`scrollLockTimeout`) from the bundled Radix Dialog; the two overlay stacks never compose over one surface.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): `open`/`onOpenChange` and `activeSnapPoint`/`setActiveSnapPoint` bind to atoms, so sheet visibility and detent are store state, undoable and URL-syncable.
- `class-variance-authority`(`.api/class-variance-authority.md`): `cva` selectors style `Drawer.Content`/`Overlay`/`Handle` through the one `cn` rail; vaul sets `[vaul-drawer]` data attributes and drag-transform CSS vars, and `tw-animate-css` keys enter/exit off `direction`.
- `react`(`.api/react.md`): the parts are `ForwardRefExoticComponent`s rendering real DOM with no `asChild` slot — pass a ref to `Drawer.Content` directly.

[LOCAL_ADMISSION]:
- Every new sheet mounts a `Drawer.Root` with `direction`/`snapPoints`, never a hand-rolled drag or a raw Radix Dialog with custom pointer handlers.
- Render `Drawer.Title` (visually hidden when headless), bind controlled `open`/`activeSnapPoint` to atoms, and set `dismissible={false}` only with a controlled `open`.
- `Drawer.Content` hosts a palette as a bare `Command`; `@floating-ui/react` owns anchored overlays, never vaul.

[RAIL_LAW]:
- Package: `vaul`
- Owns: the drag-dismissable drawer/sheet host — the `Drawer` compound over Radix Dialog, snap-point physics (`snapPoints`/`activeSnapPoint`/`fadeFromIndex`), four-way `direction`, velocity dismiss (`closeThreshold`), background scale, `handleOnly` drag, nested drawers, and mobile-keyboard `repositionInputs`
- Accept: the `Drawer.*` compound with Radix labelling, controlled `open`/`activeSnapPoint` from atoms, `@radix-ui/react-visually-hidden` for headless titles, a bare `cmdk` `Command` as content, `cva`/`cn`/`tw-animate` styling through the data-attribute + CSS-var seam
- Reject: a hand-rolled drag or a second Radix `Dialog.Root` over one sheet, a missing `Drawer.Title`, `dismissible={false}` without a controlled `open`, `CommandDialog` in `Drawer.Content`, and vaul for an anchored popover
