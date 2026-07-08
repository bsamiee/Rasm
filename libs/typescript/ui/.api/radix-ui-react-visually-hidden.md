# [TS_UI_API_RADIX_UI_REACT_VISUALLY_HIDDEN]

[PACKAGE_SURFACE]:
- package: `@radix-ui/react-visually-hidden` · version `` · license `MIT`
- module: dual — `dist/index.mjs` (ESM) + `dist/index.js` (CJS); `sideEffects: false`; one `.` barrel, no subpaths.
- asset: `dist/index.d.ts` (`restore: restored`).
- runtime: React render-time; renders one `<span>` via `@radix-ui/react-primitive@catalog` (inheriting its `asChild` merge). Peer react/react-dom 19.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, catalogued here.
- rail: `ui/view` — the visually-hidden accessibility primitive.
- role: `view/primitive.md` visually-hidden rows — icon-button labels, SR status text, skip-link text.

`@radix-ui/react-visually-hidden` hides content from sighted users while keeping it in the accessibility tree — the screen-reader-only technique. `VisuallyHidden` renders a `<span>` carrying the clip styles; `VISUALLY_HIDDEN_STYLES` is that same style object exported as a reusable constant so ANY element is made SR-only without the component (an inline `style`, a cva base, a hand-authored region). Because it composes `@radix-ui/react-primitive`, it inherits `asChild` — `<VisuallyHidden asChild><label/></VisuallyHidden>` clips an existing element instead of nesting a redundant span. The consumed capability is SR-only labeling for icon buttons and status; the style constant is catalogued because the folder OWNS the technique — a bespoke live/status region applies `VISUALLY_HIDDEN_STYLES` directly rather than re-deriving the clip rule.

## [01]-[SURFACE]

Three exports: the component (and its `Root` alias), its props type, and the reusable clip-style constant. The clip technique — not `display:none`/`visibility:hidden` — is what keeps the node readable by assistive tech.

| [INDEX] | [SYMBOL] | [KIND] | [CAPABILITY_BOUNDARY] |
|:-----: |:----------------------------- |:--------- |:------------------------------------------------------------------------- |
| [01] | `VisuallyHidden` (= `Root`) | component | ForwardRef `<span>` with the clip styles; `asChild` clips a passed child |
| [02] | `VisuallyHiddenProps` | type | `ComponentPropsWithoutRef<typeof Primitive.span>` — every span prop + `asChild` |
| [03] | `VISUALLY_HIDDEN_STYLES` | const | the frozen clip-style object, reusable on any element without the component |

```ts contract
// The reusable constant — the exact SR-only clip rule; apply to any element's `style`, a cva base, or a bespoke region.
declare const VISUALLY_HIDDEN_STYLES: Readonly<{
  position: 'absolute'; border: 0; width: 1; height: 1; padding: 0; margin: -1
  overflow: 'hidden'; clip: 'rect(0, 0, 0, 0)'; whiteSpace: 'nowrap'; wordWrap: 'normal'
}>
interface VisuallyHiddenProps extends React.ComponentPropsWithoutRef<typeof Primitive.span> {}   // Primitive.span ⇒ inherits `asChild`
declare const VisuallyHidden: React.ForwardRefExoticComponent<VisuallyHiddenProps & React.RefAttributes<HTMLSpanElement>>
export { Root, VISUALLY_HIDDEN_STYLES, VisuallyHidden, type VisuallyHiddenProps }
```

## [02]-[INTEGRATION]

[BOUNDARY: three SR-only owners — radix vs `react-aria-components` `VisuallyHidden` vs `react-aria` `useVisuallyHidden` (`.api/react-aria-components.md`, `.api/react-aria.md`)] — RAC re-exports its own `VisuallyHidden` (from `react-aria/VisuallyHidden`) and `react-aria` exposes `useVisuallyHidden` (with an `isFocusable` reveal-on-focus mode for skip-links). The radix primitive owns the STYLING-plane cases: the `VISUALLY_HIDDEN_STYLES` constant applied directly to a cva atom or a non-aria element. Within the aria spine use RAC's `VisuallyHidden`; the design never mixes a radix hidden-span into an aria component. One SR-only owner per node.

[STACK: `VISUALLY_HIDDEN_STYLES` + `@react-aria/live-announcer` (`.api/react-aria-live-announcer.md`)] — the live-announcer region is built from the identical clip styles (its vanilla-DOM impl copies them); a hand-authored status region NOT routed through `announce()` applies `VISUALLY_HIDDEN_STYLES` to stay SR-only. The design routes status through `announce()`, so this constant is the recovery for bespoke regions only.

[STACK: `VisuallyHidden` + `class-variance-authority` / `clsx` / `lucide-react` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/lucide-react.md`)] — `VISUALLY_HIDDEN_STYLES` seeds a cva base or an `sr-only` utility; the icon-button atom pairs a `lucide-react` glyph with a `<VisuallyHidden>` label so the control carries an accessible name with no visible text.

[STACK: `VisuallyHidden asChild` + `@radix-ui/react-slot` (`.api/radix-ui-react-slot.md`)] — the `asChild` VisuallyHidden inherits from `@radix-ui/react-primitive` IS the `Slot` merge; clipping an existing `<label>`/`<span>` composes onto that child rather than nesting a second span.

## [03]-[RAIL_LAW]

- Owns: the SR-only clip primitive — the `VisuallyHidden` span and the reusable `VISUALLY_HIDDEN_STYLES` constant.
- Accept: `<VisuallyHidden>` for icon-button labels and SR status text; `asChild` to clip an existing element; `VISUALLY_HIDDEN_STYLES` applied to a cva base or a bespoke element that must be SR-only.
- Reject: `display:none`/`visibility:hidden` where content must reach assistive tech (the named naive defect — those drop it from the tree); a radix hidden-span inside an aria component (RAC's `VisuallyHidden` owns that); re-deriving the clip rule where the exported constant serves.
- Boundary: render-time only; composes `@radix-ui/react-primitive@catalog` (inherits `asChild`). The style object is frozen (`Readonly`). Peer react/react-dom 19.
