# [UI_ROLE_BEHAVIOR]

The headless interaction-role vocabulary as ONE owner-block. The taxonomy is harvested as a vocabulary, not a component library: the eight interaction roles are one `Schema.Literal` owner-block of role leaves sorted internally, and a component is one headless `RoleBehavior` keyed by its role, never a parallel `.tsx` per component. The react-aria/react-stately per-component implementation patterns are discarded — only the role vocabulary and the headless-behavior contract are owned. The page composes the `binding/atom-binding.md` `AtomBinding` for any stateful role and holds no domain state.

## [1]-[INDEX]

One cluster: `ROLE_BEHAVIOR` owns the `InteractionRole` vocabulary and the headless `RoleBehavior` contract.

## [2]-[ROLE_BEHAVIOR]

- Owner: `InteractionRole`, the one `Schema.Literal` interaction-role vocabulary owner-block over the eight roles (actions, collections, inputs, overlays, navigation, feedback, pickers, core), with one headless `RoleBehavior` contract keyed by role. The eight roles are the literal members of the one owner-block; each role's component behaviors are its leaves, never eight parallel vocabularies.
- Cases: each role names the headless behaviors it owns — actions (button/toggle/toolbar), collections (grid-list/table/tag-group/tree), inputs (field/radio/select/slider), overlays (dialog/drawer), navigation (accordion/breadcrumbs/link/menu/tabs), feedback (progress/toast), pickers (color/date/file-preview/file-upload), and core (gesture/anchor); a component is one `RoleBehavior` row carrying its accessibility props, keyboard interaction map, and focus management drawn from the react-aria/react-stately headless hooks — the behavior is owned, the per-component `.tsx` markup is the consumer's, not the library's. Rich-text content passes through the DOM-purification boundary before render. The React Compiler removes manual `useMemo`/`useCallback`/`memo`, so a behavior row carries no hand-authored memoization.
- Entry: a stateful role (a selected collection, an open overlay, a slider value) reads and writes its state through the `binding/atom-binding.md` `AtomBinding`, never a second state binding; an interaction surface composes one `RoleBehavior`; the live-region accessibility broadcast every role reuses is the `accessibility-broadcast.md` cluster, composed by reference; overlay anchoring is the `overlay/floating-anchor.md` owner, composed by the overlays and navigation roles; the pointer-gesture algebra the `core` role exposes is the `motion/gesture-algebra.md` owner.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `@radix-ui/react-slot`, `@radix-ui/react-label`, `@radix-ui/react-separator`, `@radix-ui/react-visually-hidden`, `tailwindcss-react-aria-components`, `class-variance-authority`, `tailwind-merge`, `lucide-react`, `cmdk`, `vaul`, `isomorphic-dompurify`, `effect`.
- Growth: a new interaction role lands as one literal on the `InteractionRole` owner-block; a new component behavior lands as one `RoleBehavior` row under its role leaf, never a parallel vocabulary.
- Boundary: the per-component react-aria `.tsx` implementation pattern is the discarded form — only the role vocabulary and the headless-behavior contract are owned; a component holding domain state is the named defect the binding deletes; design tokens are NOT owned here — they are the `theming/theme-tokens.md` owner, and a token computed inline on a `RoleBehavior` row is the named defect; a manual `useMemo`/`useCallback`/`memo` is the named defect under the React Compiler.

```ts contract
const InteractionRole = Schema.Literal(
  "actions",
  "collections",
  "inputs",
  "overlays",
  "navigation",
  "feedback",
  "pickers",
  "core",
);
type InteractionRole = Schema.Schema.Type<typeof InteractionRole>;

interface FocusManager {
  readonly focus: () => void;
  readonly focusFirst: () => void;
  readonly focusLast: () => void;
}

interface RoleBehavior<Props, State> {
  readonly role: InteractionRole;
  readonly behavior: (props: Props) => {
    readonly aria: Record<string, unknown>;
    readonly state: State;
    readonly focus: FocusManager;
  };
}
```
