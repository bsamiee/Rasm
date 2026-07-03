# [@radix-ui/react-separator] — ARIA divider primitive projecting one `(decorative × orientation)` matrix to a role, emitting `data-orientation`, backing the composition-plane separator row

`@radix-ui/react-separator` is the composition-plane divider primitive: one `forwardRef` component rendering `@radix-ui/react-primitive`'s `Primitive.div`, whose two knobs — `orientation` (`"horizontal" | "vertical"`, default horizontal, invalid values coerced back to horizontal) and `decorative` (boolean) — drive one parameterized accessibility projection. `decorative` splits semantics-vs-presentation: a decorative separator is removed from the a11y tree (`role="none"`), a semantic one emits `role="separator"` with `aria-orientation` only in the vertical case. Every render also emits `data-orientation={orientation}` as the token-plane styling hook. The token plane owns the pixel line; this row owns the ARIA role projection, the `data-orientation` attribute, and the `asChild` merge inherited whole from `@radix-ui/react-slot`. Core `ui` only (`runtime:browser`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-separator`
- package: `@radix-ui/react-separator`
- version: `1.1.11`
- license: `MIT`
- react-peer: `react ^16.8 || ^17 || ^18 || ^19` (React 19 spine; `@types/react` `*`); depends on `@radix-ui/react-primitive@2.1.3` → `@radix-ui/react-slot@1.3.0` (`.api/radix-ui-react-slot.md`)
- catalog-verdict: KEEP
- runtime: `runtime:browser`, core `ui` — composition plane; not `scope:viewer`
- modules: single `.` barrel — `Separator` / `Root` component, `SeparatorProps` type (verified exports: `Root`, `Separator`, `SeparatorProps`); the `ORIENTATIONS` tuple and its `Orientation` type are package-internal (NOT exported)

```ts contract
// Verified dist/index.d.ts — SeparatorProps = Primitive.div props + two own knobs; asChild rides in from Primitive.
const ORIENTATIONS = ['horizontal', 'vertical'] as const   // internal — not exported
type Orientation = (typeof ORIENTATIONS)[number]           // internal — code depends on the `orientation` prop
interface SeparatorProps extends React.ComponentPropsWithoutRef<typeof Primitive.div> { orientation?: Orientation; decorative?: boolean }
declare const Separator: React.ForwardRefExoticComponent<SeparatorProps & React.RefAttributes<HTMLDivElement>>
declare const Root: typeof Separator
export { Root, Separator, type SeparatorProps }

// Verified dist/index.mjs — the total (decorative × orientation) projection + the always-on data-orientation, with caller domProps spread LAST.
const orientation = isValidOrientation(orientationProp) ? orientationProp : 'horizontal'   // invalid coerces to horizontal
const semanticProps = decorative ? { role: 'none' } : { role: 'separator', 'aria-orientation': orientation === 'vertical' ? 'vertical' : undefined }
return <Primitive.div data-orientation={orientation} {...semanticProps} {...domProps} ref={forwardedRef} />   // domProps override the computed set
```

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: separator primitive + prop contract
- rail: view/primitive, view/compose
- `Separator` and `Root` are the same `forwardRef` component under two names. `SeparatorProps extends React.ComponentPropsWithoutRef<typeof Primitive.div>` and adds two own knobs plus the `asChild` inherited from `Primitive.div`. `orientation` is a closed two-value axis (the internal `ORIENTATIONS` tuple); the public surface is the `orientation` prop, not the tuple. `decorative` selects the a11y contract.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]      | [CONSUMER / BOUNDARY]                                              |
| :-----: | :------------------------------------------------------------------- | :----------------- | :---------------------------------------------------------------- |
|  [01]   | `Separator` / `Root` (`ForwardRefExoticComponent<SeparatorProps & RefAttributes<HTMLDivElement>>`) | primitive component | `view/primitive` divider row; ref forwards to the `<div>` element |
|  [02]   | `SeparatorProps` (`= PrimitiveDivProps & { orientation?; decorative? }`) | prop contract      | native `<div>` attrs + orientation axis + decorative gate + inherited `asChild` |
|  [03]   | `orientation?: "horizontal" \| "vertical"` (default `"horizontal"`)  | closed axis        | drives `aria-orientation` (vertical only) + the always-emitted `data-orientation` token hook |
|  [04]   | `decorative?: boolean`                                               | a11y gate          | `true` → `role="none"` (removed from a11y tree); else `role="separator"` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: render divider under the `(decorative × orientation)` projection
- rail: view/primitive
- One component owns the full divider space: `(decorative, orientation)` selects the ARIA role and (via `data-orientation`) the token-plane line axis in a single render — the four render shapes are policy rows of one primitive, never four components. `data-orientation` is emitted on every render (decorative and semantic alike). `asChild` merges divider semantics onto a caller element through `Primitive.div` → the `createSlot('Primitive.div')` instance.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]  | [CONSUMER / BOUNDARY]                                              |
| :-----: | :------------------------------------------------------------------ | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `<Separator orientation="horizontal" \| "vertical" />`              | semantic divider | `role="separator"`; `aria-orientation="vertical"` emitted for vertical only; `data-orientation` set |
|  [02]   | `<Separator decorative />`                                          | presentational   | `role="none"`; removed from a11y tree; `data-orientation` still set for the token line |
|  [03]   | `<Separator asChild><Slot-mergeable element/></Separator>`         | slot-merge       | render-as-child via `Primitive.div`→`createSlot('Primitive.div')`; `view/compose` slot row |

## [04]-[IMPLEMENTATION_LAW]

[PRIMITIVE_TOPOLOGY]:
- one component, two names: `Separator === Root`; a `HorizontalSeparator`/`VerticalSeparator` split is the named defect — `orientation` is the axis knob.
- one a11y projection: `(decorative, orientation) → { role, aria-orientation }` is a total function over a two-by-two space; `decorative` gates semantics-vs-presentation, `orientation` gates `aria-orientation` (only vertical is emitted, since ARIA defaults separators to horizontal). An out-of-range `orientation` coerces to horizontal via the internal `isValidOrientation` guard. `data-orientation={orientation}` is emitted unconditionally as the styling hook.
- `asChild` is inherited: `Primitive.div` is `asChild ? createSlot('Primitive.div') : 'div'` (verified `react-primitive` runtime); the separator never re-implements slot merging.

[INTEGRATION_LAW]:
- Stack with the token plane (`class-variance-authority`+`clsx`+`tailwind-merge`, `.api/*`): the line variant keys off the emitted `data-orientation` attribute (`[data-orientation=horizontal]` → `h-px w-full`, `[data-orientation=vertical]` → `w-px h-full`) or an equivalent `cva` row; the primitive emits the role + `data-orientation`, the token plane emits the pixel.
- Stack with `@radix-ui/react-slot` (`.api/radix-ui-react-slot.md` [02]): `asChild` inherits the EXACT Slot prop/ref merge — handlers chain (child first, then the separator's), `style` shallow-merges child-wins, `className` concatenates (separator then child), other props child-wins (`{ ...separatorProps, ...childProps }`), `ref` composes via `@radix-ui/react-compose-refs`. Two-layer precedence: the separator already spreads `{ data-orientation, ...semanticProps, ...domProps }` (caller `domProps` override the computed `role`/`aria-orientation`/`data-orientation`), then `Slot` merges the child over that result — so an `asChild` child can further override the role. Exactly one React-element child, enforced by the `createSlot('Primitive.div')` invariant.
- Boundary vs `react-aria-components` (`.api/react-aria-components.md`): RAC ships its OWN `Separator` (structure family) over react-aria's `useSeparator`, reading `SeparatorContext`. Inside an RAC `Toolbar`/`Menu`/`Group`/`ListBox` that already emits group semantics, the RAC `Separator` is the owner — a radix `Separator` there is the double-primitive defect (two separator roles, off the RAC state machine). Radix `Separator` serves the NON-aria styling plane: a standalone semantic (`decorative={false}`) divider off any RAC region. Where a purely visual rule sits inside an already-grouped RAC region, use radix `decorative` so it stays visual-only and adds no second a11y boundary. One separator owner per region.

[LOCAL_ADMISSION]:
- core `ui` composition/primitive plane only; `scope:viewer` never imports it.
- one separator primitive; a new divider style is a token-plane variant off `data-orientation`, a new render target is `asChild`, never a second component.
- `ORIENTATIONS`/`Orientation` are package-internal — code depends on the `orientation` prop, never on importing the tuple or the type.
- radix `Separator` off the react-aria state machine only; inside an RAC grouped region the RAC `Separator` (via `SeparatorContext`) is the owner.

[RAIL_LAW]:
- Package: `@radix-ui/react-separator`
- Owns: the composition-plane divider primitive, the `(decorative × orientation) → role` accessibility projection, and the always-emitted `data-orientation` token hook
- Accept: `Separator`/`Root` as the one divider component, `orientation` as the axis knob, `decorative` as the a11y gate, `asChild` inheriting the Slot merge through `Primitive.div`→`createSlot('Primitive.div')`, `data-orientation`-keyed token-plane class for the line
- Reject: per-orientation component variants, a decorative-vs-semantic component split, importing the internal `ORIENTATIONS` tuple, a radix `Separator` inside an RAC grouped region that already owns the divider via `SeparatorContext`, separator-local re-implementation of slot merging or line styling
