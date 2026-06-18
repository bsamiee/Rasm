# [API_CATALOGUE] vaul

`vaul` provides the `Drawer` compound component for building bottom-sheet and directional drawer overlays built on top of `@radix-ui/react-dialog`. It supports snap points, velocity-based swiping, directional opening (`top`, `bottom`, `left`, `right`), background scale, and nested drawers. The public surface is the `Drawer` namespace object plus flat named component and type exports.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vaul`
- package: `vaul`
- namespace: `vaul`
- asset: runtime component library
- rail: overlay

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: drawer prop types
- rail: overlay

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                                             |
| :-----: | :--------------------- | :------------ | :----------------------------------------------------------------- |
|   [1]   | `DialogProps`          | type alias    | full root props union; `WithFadeFromProps \| WithoutFadeFromProps` |
|   [2]   | `WithFadeFromProps`    | interface     | requires `snapPoints` + `fadeFromIndex`                            |
|   [3]   | `WithoutFadeFromProps` | interface     | `snapPoints?`; `fadeFromIndex` must be absent                      |
|   [4]   | `ContentProps`         | type alias    | `React.ComponentPropsWithoutRef<typeof DialogPrimitive.Content>`   |
|   [5]   | `HandleProps`          | type alias    | `React.ComponentPropsWithoutRef<'div'> & { preventCycle? }`        |
|   [6]   | `PortalProps`          | type alias    | `React.ComponentPropsWithoutRef<typeof DialogPrimitive.Portal>`    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Drawer compound component (namespace)
- rail: overlay

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY] | [RAIL]                                                                                                                                               |
| :-----: | :------------------- | :------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Drawer.Root`        | root component | full `DialogProps`; `direction?`, `snapPoints?`, `fadeFromIndex?`, `activeSnapPoint?`, `shouldScaleBackground?`, `dismissible?`, `modal?`, `nested?` |
|   [2]   | `Drawer.Content`     | subcomponent   | `ContentProps`; `ForwardRefExoticComponent`                                                                                                          |
|   [3]   | `Drawer.Overlay`     | subcomponent   | `DialogPrimitive.DialogOverlayProps`; `ForwardRefExoticComponent`                                                                                    |
|   [4]   | `Drawer.Trigger`     | subcomponent   | `DialogPrimitive.DialogTriggerProps`; `ForwardRefExoticComponent`                                                                                    |
|   [5]   | `Drawer.Close`       | subcomponent   | `DialogPrimitive.DialogCloseProps`; `ForwardRefExoticComponent`                                                                                      |
|   [6]   | `Drawer.Portal`      | subcomponent   | `PortalProps`; portals content outside DOM tree                                                                                                      |
|   [7]   | `Drawer.Handle`      | subcomponent   | `HandleProps`; drag handle; `preventCycle?` stops snap cycling                                                                                       |
|   [8]   | `Drawer.Title`       | subcomponent   | `DialogPrimitive.DialogTitleProps`; `ForwardRefExoticComponent`                                                                                      |
|   [9]   | `Drawer.Description` | subcomponent   | `DialogPrimitive.DialogDescriptionProps`; `ForwardRefExoticComponent`                                                                                |

[ENTRYPOINT_SCOPE]: flat named exports
- rail: overlay

| [INDEX] | [SURFACE]    | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :----------- | :------------- | :----------------------------------------------------- |
|   [1]   | `Root`       | root component | same as `Drawer.Root`                                  |
|   [2]   | `NestedRoot` | root component | nested drawer root; forwards `onDrag` + `onOpenChange` |
|   [3]   | `Content`    | subcomponent   | same as `Drawer.Content`                               |
|   [4]   | `Overlay`    | subcomponent   | same as `Drawer.Overlay`                               |
|   [5]   | `Handle`     | subcomponent   | same as `Drawer.Handle`                                |
|   [6]   | `Portal`     | subcomponent   | same as `Drawer.Portal`                                |

## [4]-[IMPLEMENTATION_LAW]

[DRAWER_TOPOLOGY]:
- built on `@radix-ui/react-dialog`; `Drawer.Root` is the controlled open/close boundary
- `direction` defaults to `'bottom'`; `'top'`, `'left'`, `'right'` also supported
- `snapPoints` is `(number | string)[]`; numbers are fractions of screen height (0–1), strings are pixel values
- `fadeFromIndex` requires `snapPoints` and is the index from which the overlay fade activates; triggers `WithFadeFromProps` variant
- `shouldScaleBackground` animates the page background scale; `setBackgroundColorOnScale` controls background colour change
- `handleOnly` restricts dragging to the `Drawer.Handle` element only
- `dismissible` defaults to `true`; set `false` together with controlled `open` for non-dismissible sheets
- `modal` defaults to `true`; `false` allows background interaction without closing
- `NestedRoot` handles inner drawers; wrap nested drawers in `NestedRoot`, not `Root`
- `onAnimationEnd(open: boolean)` fires after the open/close transition completes

[LOCAL_ADMISSION]:
- Use `Drawer.Root` with controlled `open`/`onOpenChange` for programmatic lifecycle ownership.
- For snap-point sheets, define `snapPoints` as a fraction array and `fadeFromIndex` as the index of the first "open" snap point.
- Use `Drawer.Handle` to give users a visible drag target; set `preventCycle` when snap-cycling conflicts with intended UX.

[RAIL_LAW]:
- package: `vaul`
- owns: swipeable bottom/directional drawer overlays with snap points and background scale
- accept: `snapPoints`, `fadeFromIndex`, `direction`, controlled `open`/`onOpenChange`
- reject: re-implementing drag-and-snap mechanics or Radix Dialog integration for drawer overlays
