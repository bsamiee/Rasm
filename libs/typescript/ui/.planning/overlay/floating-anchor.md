# [UI_FLOATING_ANCHOR]

The floating-surface positioning owner distinct in kind from the headless role behavior. `useFloatingAnchor` owns the `@floating-ui/react` anchoring (tooltip/popover/menu/dialog/select/combobox placement) over one `useFloating` positioner and one `useInteractions` merge, the CSS Anchor Positioning bridge, and the focus-trap/dismiss contract, composed by the `component-system/role-behavior.md` overlays and navigation roles. The owner holds no domain state; an anchored surface reads its open state through the `binding/atom-binding.md` `AtomBinding`.

## [1]-[INDEX]

One cluster: `FLOATING_ANCHOR` owns the overlay placement, the anchor bridge, and the dismiss law.

## [2]-[FLOATING_ANCHOR]

- Owner: `useFloatingAnchor`, the `@floating-ui/react` placement owner over the overlay surfaces (tooltip, popover, menu, dialog, select, combobox); the `ROLE_OF` overlay-kind-to-ARIA-role table; the CSS Anchor Positioning bridge that lifts placement onto the native anchor layer where supported; and the focus-trap/dismiss contract every anchored surface reuses. Placement is owned once, not per overlay.
- Cases: `useFloatingAnchor` resolves placement through the `useFloating` `whileElementsMounted: autoUpdate` positioner with the `offset`/`flip`/`shift`/`arrow` middleware stack, merges the `useClick`/`useFocus`/`useRole`/`useDismiss` interaction hooks through one `useInteractions` so a manual handler never collides, and keys the ARIA role off the `ROLE_OF` table by overlay kind; the CSS Anchor Positioning bridge maps the same placement onto the native `anchor-name`/`position-anchor` layer where the browser admits it and falls back to the floating-ui computed position otherwise; the `useDismiss` contract closes on outside-press, escape, and ancestor scroll, and `FloatingFocusManager` traps and restores focus on open/close.
- Entry: an overlays or navigation `RoleBehavior` composes one `useFloatingAnchor` for its placement and dismiss; the open state reads and writes through the `binding/atom-binding.md` `AtomBinding` and drives the `onOpenChange` callback; the anchor and floating elements bind through `refs.setReference`/`refs.setFloating`, never a free React ref leaked past the hook.
- Packages: `@floating-ui/react`, `@floating-ui/react-dom`, `react-aria`, `react-stately`, `effect`.
- Growth: a new overlay kind lands as one `FloatingAnchor` placement row keyed by the kind; a new dismiss trigger lands as one row on the dismiss contract, never a parallel positioning owner.
- Boundary: a hand-rolled `getBoundingClientRect` placement computation beside `FloatingAnchor` is the named defect; placement is owned here, not on the overlay `RoleBehavior`, and a per-overlay positioning calculation is the named defect; the dismiss contract is the single focus-trap/dismiss path and a per-overlay outside-press handler is the named defect.

```ts contract
import {
  arrow,
  autoUpdate,
  flip,
  offset,
  shift,
  useClick,
  useDismiss,
  useFloating,
  useFocus,
  useInteractions,
  useRole,
  type Placement,
  type UseRoleProps,
} from "@floating-ui/react";
import { Schema } from "effect";

const OverlayKind = Schema.Literal("tooltip", "popover", "menu", "dialog", "select", "combobox");
type OverlayKind = Schema.Schema.Type<typeof OverlayKind>;

const ROLE_OF: Record<OverlayKind, NonNullable<UseRoleProps["role"]>> = {
  tooltip: "tooltip",
  popover: "dialog",
  menu: "menu",
  dialog: "dialog",
  select: "listbox",
  combobox: "listbox",
};

const useFloatingAnchor = (
  kind: OverlayKind,
  open: boolean,
  onOpenChange: (next: boolean) => void,
  arrowRef: React.RefObject<SVGSVGElement>,
  placement: Placement = "bottom",
) => {
  const floating = useFloating({
    open,
    onOpenChange,
    placement,
    whileElementsMounted: autoUpdate,
    middleware: [offset(8), flip(), shift({ padding: 8 }), arrow({ element: arrowRef })],
  });
  const interactions = useInteractions([
    useClick(floating.context),
    useFocus(floating.context),
    useRole(floating.context, { role: ROLE_OF[kind] }),
    useDismiss(floating.context, { outsidePress: true, escapeKey: true, ancestorScroll: true }),
  ]);
  return { ...floating, ...interactions };
};
```
