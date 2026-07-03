# [API_CATALOGUE] vaul

`vaul` provides the `Drawer` compound component for bottom-sheet and directional drawer overlays built on a bundled `@radix-ui/react-dialog` (the focus-trap, scroll-lock, portal, and a11y shell). It supports snap points, velocity-based swiping, directional opening (`top`/`bottom`/`left`/`right`), background scale, keyboard-aware input repositioning, and nested drawers. The drag-following transform is vaul's own JS (inline `translate3d`); the open/close lifecycle emits the Radix `data-state="open|closed"` attribute plus `data-vaul-drawer`/`-direction`/`-overlay`/`-wrapper` hooks and a `--snap-point-height` CSS var — the styling seams a `cva` recipe and the `tw-animate-css` overlay fade key off. In the ui stack `open`/`onOpenChange`/`activeSnapPoint` bind to the `AtomBinding`, and `Drawer.Root` is the mobile/secondary command surface dialing the same `CommandAction` rows as the `cmdk` palette (`interaction/command.md#COMMAND_SURFACE`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vaul`
- package / version: `vaul` @ `1.1.2`
- license: `MIT`
- module: dual ESM `dist/index.mjs` (`.d.mts`) + CJS `dist/index.js` (`.d.ts`); single `.` export
- peer: `react` / `react-dom` `^16.8 || ^17 || ^18 || ^19` — subcomponents are React `ForwardRefExoticComponent` primitives
- dependency: `@radix-ui/react-dialog` (bundled) — the drawer's Dialog shell and the `data-state` attribute source
- rail: overlay

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: drawer prop types
- rail: overlay

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `DialogProps`          | type alias | full `Drawer.Root` prop union — the base object `& (WithFadeFromProps \| WithoutFadeFromProps)` discriminant |
|  [02]   | `WithFadeFromProps`    | interface  | `snapPoints: (number \| string)[]` (required) + `fadeFromIndex: number` (required) |
|  [03]   | `WithoutFadeFromProps` | interface  | `snapPoints?: (number \| string)[]`; `fadeFromIndex?: never` — the no-fade branch |
|  [04]   | `ContentProps`         | type alias | `React.ComponentPropsWithoutRef<typeof DialogPrimitive.Content>` |
|  [05]   | `HandleProps`          | type alias | `React.ComponentPropsWithoutRef<'div'> & { preventCycle?: boolean }` |
|  [06]   | `PortalProps`          | type alias | `React.ComponentPropsWithoutRef<typeof DialogPrimitive.Portal>` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Drawer` compound namespace (all nine members reachable as `Drawer.*`)
- rail: overlay

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `Drawer.Root`        | root component | full `DialogProps` (see the root-prop table below); the controlled open/close + drag boundary |
|  [02]   | `Drawer.Content`     | subcomponent   | `ContentProps`; `ForwardRefExoticComponent<HTMLDivElement>`; the draggable sheet body |
|  [03]   | `Drawer.Overlay`     | subcomponent   | `DialogPrimitive.DialogOverlayProps`; the scrim — emits `data-vaul-overlay`, fades via `data-state` |
|  [04]   | `Drawer.Trigger`     | subcomponent   | `DialogPrimitive.DialogTriggerProps` (`HTMLButtonElement`ref); namespace-only (not a flat export) |
|  [05]   | `Drawer.Close`       | subcomponent   | `DialogPrimitive.DialogCloseProps` (`HTMLButtonElement`ref); namespace-only |
|  [06]   | `Drawer.Portal`      | subcomponent   | `PortalProps`; portals content; `container` prop for a custom mount node |
|  [07]   | `Drawer.Handle`      | subcomponent   | `HandleProps`; visible drag target; `preventCycle?` stops snap-point cycling on tap |
|  [08]   | `Drawer.Title`       | subcomponent   | `DialogPrimitive.DialogTitleProps` (`HTMLHeadingElement`); namespace-only; a11y label |
|  [09]   | `Drawer.Description` | subcomponent   | `DialogPrimitive.DialogDescriptionProps` (`HTMLParagraphElement`); namespace-only |

[ENTRYPOINT_SCOPE]: flat named exports (a strict subset — `Trigger`/`Close`/`Title`/`Description` are NAMESPACE-ONLY)
- rail: overlay

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `Root`       | root component | same as `Drawer.Root` |
|  [02]   | `NestedRoot` | root component | nested drawer root; internally forwards `onDrag` + `onOpenChange` to the parent drawer |
|  [03]   | `Content`    | subcomponent   | same as `Drawer.Content` |
|  [04]   | `Overlay`    | subcomponent   | same as `Drawer.Overlay` |
|  [05]   | `Handle`     | subcomponent   | same as `Drawer.Handle` |
|  [06]   | `Portal`    | subcomponent   | same as `Drawer.Portal` |

[ENTRYPOINT_SCOPE]: `Drawer.Root` props (the `DialogProps` object)
- rail: overlay

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `open?` / `defaultOpen?` / `onOpenChange?(open)` / `onClose?()` | lifecycle | controlled/uncontrolled open; `defaultOpen` skips the initial enter animation |
|  [02]   | `direction?` | axis | `'top' \| 'bottom' \| 'left' \| 'right'`; default `'bottom'` |
|  [03]   | `snapPoints?` / `activeSnapPoint?` / `setActiveSnapPoint?` | snap | `(number \| string)[]` — fractions `0–1` of screen or px strings; controlled active point |
|  [04]   | `fadeFromIndex?` / `snapToSequentialPoint?` | snap fade | fade-start snap index (requires `snapPoints`; `WithFadeFromProps`); `snapToSequentialPoint` disables velocity skip |
|  [05]   | `closeThreshold?` / `scrollLockTimeout?` | dismissal | drag fraction that closes (default `0.25`); non-draggable window after inner scroll (default `500`ms) |
|  [06]   | `dismissible?` / `modal?` / `handleOnly?` / `nested?` | interaction | `dismissible`/`modal` default `true`; `handleOnly` restricts drag to `Drawer.Handle` (default `false`) |
|  [07]   | `shouldScaleBackground?` / `setBackgroundColorOnScale?` / `noBodyStyles?` | background | scales the `[data-vaul-drawer-wrapper]`; `setBackgroundColorOnScale` default `true`; `noBodyStyles` opts out of Vaul body styles |
|  [08]   | `fixed?` / `disablePreventScroll?` / `repositionInputs?` / `preventScrollRestoration?` | scroll | keyboard-aware height; body-scroll prevention (default off); input reposition (default `true` when `snapPoints`) |
|  [09]   | `container?` / `autoFocus?` | mount | portal `HTMLElement`; `autoFocus` on open |
|  [10]   | `onDrag?(event, percentageDragged)` / `onRelease?(event, open)` / `onAnimationEnd?(open)` | callbacks | live drag fraction, release disposition, and post-transition hook |

## [04]-[IMPLEMENTATION_LAW]

[DRAWER_TOPOLOGY]:
- built on a bundled `@radix-ui/react-dialog`; `Drawer.Root` is the controlled open/close boundary and emits `data-state="open|closed"` (Radix) plus `data-vaul-drawer`, `data-vaul-drawer-direction`, `data-vaul-overlay`, `data-vaul-snap-points`, `data-vaul-drawer-wrapper`, and the `--snap-point-height` CSS var — the styling hooks a `cva` recipe keys off.
- `WithFadeFromProps` vs `WithoutFadeFromProps` discriminant: `fadeFromIndex` present ⇒ `snapPoints` required; absent ⇒ `fadeFromIndex?: never` (the type forbids a fade index without snap points).
- `snapPoints` are fractions of screen (`0–1`) or px strings; `activeSnapPoint`/`setActiveSnapPoint` control the current point; `closeThreshold` + swipe velocity govern dismissal, and `snapToSequentialPoint` forces step-by-step snapping.
- the drag-following transform is vaul's own JS (inline `translate3d`/`scale`), NOT CSS; `shouldScaleBackground` scales the wrapper, `setBackgroundColorOnScale` toggles its background color.
- `handleOnly` restricts drag to `Drawer.Handle` (with `preventCycle` stopping snap-cycling on tap); `dismissible={false}` + controlled `open` yields a non-dismissible sheet; `modal={false}` allows background interaction; `NestedRoot` wraps inner drawers and forwards `onDrag`/`onOpenChange` to the parent.
- keyboard/scroll: `repositionInputs` moves inputs above the on-screen keyboard, `disablePreventScroll`/`preventScrollRestoration`/`scrollLockTimeout` govern body scroll, `fixed` changes height rather than position when the keyboard is open; `container` portals to a custom node; `onAnimationEnd(open)` fires after the open/close transition.

[STACKING]:
- controlled lifecycle under the effect rail: bind `open`/`onOpenChange` (and `activeSnapPoint`/`setActiveSnapPoint`) to the `binding/atom.md#ATOM_BINDING` cell so drawer lifecycle is a projection of Effect state — the `interaction/command.md#COMMAND_SURFACE` `ActionDrawer` mounts `Drawer.Root` with `direction`/`snapPoints`/`dismissible`/`modal` carried as payload.
- overlay enter/exit motion: the `Drawer.Overlay` fade rides the `tw-animate-css` `data-[state=open]:animate-in fade-in` / `data-[state=closed]:animate-out fade-out` layer keyed on the Radix `data-state`; the sheet-body drag transform is vaul's own JS — do NOT layer `tw-animate-css` slide utilities on the dragged body (`tw-animate-css.md`, `theming/tokens.md#THEME_TOKENS`).
- command surface: `ActionDrawer` is the mobile/secondary sibling of the `cmdk` `Command.Dialog` desktop palette, dialing the same `CommandAction` rows through the `interchange` `CommandGateway` — never a second command/intent surface (`cmdk.md`, `interaction/command.md#COMMAND_SURFACE`).
- variant recipe: style `Drawer.Content`/`Overlay`/`Handle` through the one `cn = twMerge(cx(...))` recipe, keying `cva` rows off `data-vaul-drawer-direction` and `data-state` (`class-variance-authority.md`, `tailwind-merge.md`).

[LOCAL_ADMISSION]:
- Use `Drawer.Root` with controlled `open`/`onOpenChange` for programmatic lifecycle; define `snapPoints` as a fraction array and `fadeFromIndex` as the first "open" snap index for snap sheets.
- Compose `Drawer.Portal` > `Drawer.Overlay` + `Drawer.Content` > `Drawer.Handle`; import `Trigger`/`Close`/`Title`/`Description` from the `Drawer` namespace — they are not flat exports.
- Give a visible `Drawer.Handle`; set `preventCycle` when snap-cycling conflicts and `handleOnly` to restrict drag to the handle.
- Wrap nested drawers in `NestedRoot`, never `Root`.

[RAIL_LAW]:
- package: `vaul`
- owns: swipeable bottom/directional drawer overlays with snap points, velocity swiping, background scale, keyboard-aware repositioning, and nesting on a bundled Radix Dialog
- accept: `snapPoints`/`fadeFromIndex`/`direction`, controlled `open`/`onOpenChange`, `activeSnapPoint`/`setActiveSnapPoint`, `handleOnly`/`dismissible`/`modal`, `NestedRoot` for nesting
- reject: re-implementing drag-and-snap mechanics or the Radix Dialog integration; layering `tw-animate-css` slide utilities on the JS-driven drag body; a second command/intent surface beside the gateway
