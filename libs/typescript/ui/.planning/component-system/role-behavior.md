# [UI_ROLE_BEHAVIOR]

The headless interaction-role vocabulary as ONE behavior-bearing owner-block. The taxonomy is harvested as a vocabulary, not a component library: the eight interaction roles are one `as const satisfies Record<role, RoleSpec>` owner-block whose rows carry their own behavior columns — live-region politeness, the headless component-behavior set, and the focus model — and a component is one headless `RoleBehavior` keyed by its role, never a parallel `.tsx` per component. The react-aria/react-stately per-component implementation patterns are discarded — only the role vocabulary and the headless-behavior contract are owned. The page composes the `binding/atom-binding.md` `AtomBinding` for any stateful role and holds no domain state.

## [1]-[INDEX]

One cluster: `ROLE_BEHAVIOR` owns the `InteractionRole` behavior-bearing vocabulary and the headless `RoleBehavior` contract.

## [2]-[ROLE_BEHAVIOR]

- Owner: `_Roles`, the one `as const satisfies Record<string, RoleSpec>` interaction-role vocabulary owner-block over the eight roles (actions, collections, inputs, overlays, navigation, feedback, pickers, core); `InteractionRole`, the `keyof typeof _Roles` discriminant projected onto a `Schema.Literal(...R.keys(_Roles))` at the wire seam over the `R`-aliased `effect` `Record` module; and one headless `RoleBehavior` contract keyed by role. The eight rows are the members of the one owner-block; each row carries its `politeness`, `behaviors`, and `focusable` columns, so `announceFor` — the role-to-politeness projection the `accessibility-broadcast.md` announce path composes by reference — is the indexed-access read (`_Roles[role].politeness`) defined here where `_Roles` is module-visible, returning the `Politeness` type the vocabulary derives from its own column, never a parallel record keyed on the same literal.
- Cases: each row names the headless behaviors it owns — actions (button/toggle/toolbar), collections (grid-list/table/tag-group/tree), inputs (field/radio/select/slider), overlays (dialog/drawer), navigation (accordion/breadcrumbs/link/menu/tabs), feedback (progress/toast), pickers (color/date/file-preview/file-upload), and core (gesture/anchor); a component is one `RoleBehavior` row carrying its accessibility props, keyboard interaction map, and focus management drawn from the react-aria/react-stately headless hooks — the behavior is owned, the per-component `.tsx` markup is the consumer's, not the library's. The `focusable` column gates the `FocusModel` projection so a non-focusable feedback or core row carries no focus surface. Rich-text content passes through the DOM-purification boundary before render. The React Compiler removes manual `useMemo`/`useCallback`/`memo`, so a behavior row carries no hand-authored memoization.
- Entry: a stateful role (a selected collection, an open overlay, a slider value) reads and writes its state through the `binding/atom-binding.md` `AtomBinding`, never a second state binding; an interaction surface composes one `RoleBehavior`; the live-region accessibility broadcast every role reuses reads the `politeness` column through the `accessibility-broadcast.md` cluster, composed by reference; overlay anchoring is the `overlay/floating-anchor.md` owner, composed by the overlays and navigation roles; the pointer-gesture algebra the `core` role exposes is the `motion/gesture-algebra.md` owner.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `@radix-ui/react-slot`, `@radix-ui/react-label`, `@radix-ui/react-separator`, `@radix-ui/react-visually-hidden`, `tailwindcss-react-aria-components`, `class-variance-authority`, `tailwind-merge`, `lucide-react`, `cmdk`, `vaul`, `isomorphic-dompurify`, `effect`.
- Growth: a new interaction role lands as one `_Roles` row carrying its politeness, behavior set, and focus column; a new component behavior lands as one entry in the row's `behaviors` set under its role, never a parallel vocabulary; a new cross-cutting role concern lands as one column on `RoleSpec`, never a second record keyed on the role.
- Boundary: the per-component react-aria `.tsx` implementation pattern is the discarded form — only the role vocabulary and the headless-behavior contract are owned; a politeness, behavior, or focus map keyed on `InteractionRole` declared beside `_Roles` is the named defect the column deletes; a component holding domain state is the named defect the binding deletes; design tokens are NOT owned here — they are the `theming/theme-tokens.md` owner, and a token computed inline on a `RoleBehavior` row is the named defect; a manual `useMemo`/`useCallback`/`memo` is the named defect under the React Compiler.

```ts contract
const _Roles = {
  actions:     { politeness: "polite",     behaviors: ["button", "toggle", "toolbar"],                 focusable: true  },
  collections: { politeness: "off",        behaviors: ["grid-list", "table", "tag-group", "tree"],    focusable: true  },
  inputs:      { politeness: "polite",     behaviors: ["field", "radio", "select", "slider"],         focusable: true  },
  overlays:    { politeness: "assertive",  behaviors: ["dialog", "drawer"],                            focusable: true  },
  navigation:  { politeness: "polite",     behaviors: ["accordion", "breadcrumbs", "link", "menu", "tabs"], focusable: true  },
  feedback:    { politeness: "assertive",  behaviors: ["progress", "toast"],                           focusable: false },
  pickers:     { politeness: "polite",     behaviors: ["color", "date", "file-preview", "file-upload"], focusable: true  },
  core:        { politeness: "off",        behaviors: ["gesture", "anchor"],                           focusable: false },
} as const satisfies Record<string, {
  readonly politeness: "assertive" | "polite" | "off";
  readonly behaviors: ReadonlyArray<string>;
  readonly focusable: boolean;
}>;

const InteractionRole = Schema.Literal(...(R.keys(_Roles) as [keyof typeof _Roles, ...Array<keyof typeof _Roles>]));
type InteractionRole = Schema.Schema.Type<typeof InteractionRole>;
type Politeness = (typeof _Roles)[InteractionRole]["politeness"];

const announceFor = (role: InteractionRole): Politeness => _Roles[role].politeness;

interface FocusModel {
  readonly focus: () => void;
  readonly focusFirst: () => void;
  readonly focusLast: () => void;
}

interface RoleBehavior<Props, State> {
  readonly role: InteractionRole;
  readonly behavior: (props: Props) => {
    readonly aria: Record<string, unknown>;
    readonly state: State;
    readonly focus: Option.Option<FocusModel>;
  };
}
```
