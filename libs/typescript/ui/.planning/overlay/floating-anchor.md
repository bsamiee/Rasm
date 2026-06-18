# [UI_FLOATING_ANCHOR]

The floating-surface positioning owner distinct in kind from the headless role behavior. `FloatingAnchor` owns the @floating-ui/react anchoring (tooltip/popover/menu/dialog/select/combobox placement), the CSS Anchor Positioning bridge, and the focus-trap/dismiss contract, composed by the `component-system/role-behavior.md` overlays and navigation roles. The owner holds no domain state; an anchored surface reads its open state through the `binding/atom-binding.md` `AtomBinding`.

## [1]-[INDEX]

One cluster: `FLOATING_ANCHOR` owns the overlay placement, the anchor bridge, and the dismiss law.

## [2]-[FLOATING_ANCHOR]

- Owner: `FloatingAnchor`, the @floating-ui/react placement owner over the overlay surfaces (tooltip, popover, menu, dialog, select, combobox); the CSS Anchor Positioning bridge that lifts placement onto the native anchor layer where supported; and the focus-trap/dismiss contract every anchored surface reuses. Placement is owned once, not per overlay.
- Cases: `FloatingAnchor` resolves placement, shift, flip, offset, and arrow positioning through the floating-ui middleware stack keyed by the overlay kind; the CSS Anchor Positioning bridge maps the same placement onto the native `anchor-name`/`position-anchor` layer where the browser admits it and falls back to the floating-ui computed position otherwise; the focus-trap traps and restores focus on open/close and the dismiss contract closes on outside-press, escape, and scroll per the overlay kind.
- Entry: an overlays or navigation `RoleBehavior` composes one `FloatingAnchor` for its placement and dismiss; the open state reads and writes through the `binding/atom-binding.md` `AtomBinding`; the anchor element and the floating element bind through the floating-ui reference/floating refs held inside the anchor, never a free React ref leaked past it.
- Packages: `@floating-ui/react`, `@floating-ui/react-dom`, `react-aria`, `react-stately`, `effect`.
- Growth: a new overlay kind lands as one `FloatingAnchor` placement row keyed by the kind; a new dismiss trigger lands as one row on the dismiss contract, never a parallel positioning owner.
- Boundary: a hand-rolled `getBoundingClientRect` placement computation beside `FloatingAnchor` is the named defect; placement is owned here, not on the overlay `RoleBehavior`, and a per-overlay positioning calculation is the named defect; the dismiss contract is the single focus-trap/dismiss path and a per-overlay outside-press handler is the named defect.

```ts contract
const OverlayKind = Schema.Literal("tooltip", "popover", "menu", "dialog", "select", "combobox");
type OverlayKind = Schema.Schema.Type<typeof OverlayKind>;

interface FloatingAnchor {
  readonly kind: OverlayKind;
  readonly placement: (open: boolean) => {
    readonly refs: { readonly setReference: (el: Element | null) => void; readonly setFloating: (el: HTMLElement | null) => void };
    readonly floatingStyles: Record<string, string | number>;
    readonly dismiss: { readonly outsidePress: boolean; readonly escapeKey: boolean; readonly ancestorScroll: boolean };
  };
}
```

RESEARCH [FLOATING_UI]: the `@floating-ui/react` `useFloating`/`useDismiss`/`useFocus`/`useInteractions`/`useRole` hooks, the `offset`/`flip`/`shift`/`arrow` middleware spellings, and the CSS Anchor Positioning `anchor-name`/`position-anchor` bridge are unverified; the placement and dismiss member spellings stay RESEARCH until the folder `.api/` catalogue carries the `@floating-ui/react` rows.
