# [UI_COMPONENT_SYSTEM]

One page owns the role-based headless component taxonomy as ONE interaction-role vocabulary owner-block plus the theme-token surface and runtime CSS-var sync. The taxonomy is harvested as a vocabulary, not a component library: the eight interaction roles are one `Schema.Literal` owner-block of role leaves sorted internally, and a component is one headless behavior keyed by its role, never a parallel `.tsx` per component. The react-aria per-component implementation patterns are discarded — only the role vocabulary and the headless-behavior contract are owned. The theme tokens are a color-space-aware token owner synced to runtime CSS variables. The page composes the `binding.md` `AtomBinding` for any stateful role and holds no domain state.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                           |
| :-----: | :--------------- | :--------------------------------------------------------------- |
|   [1]   | COMPONENT_SYSTEM | the interaction-role vocabulary owner-block and the theme tokens |

## [2]-[COMPONENT_SYSTEM]

- Owner: `InteractionRole`, the one `Schema.Literal` interaction-role vocabulary owner-block over the eight roles (actions, collections, inputs, overlays, navigation, feedback, pickers, core), with one headless `RoleBehavior` contract keyed by role; `ThemeTokens`, the color-space-aware token owner; and `CssVarSync`, the runtime CSS-variable sync binding the token source to the live document. The eight roles are sub-folder leaves of the one owner-block, sorted internally by role, never eight parallel vocabularies.
- Cases: each role names the headless behaviors it owns — actions (button/toggle/toolbar), collections (grid-list/table/tag-group/tree), inputs (field/radio/select/slider), overlays (dialog/drawer), navigation (accordion/breadcrumbs/link/menu/tabs), feedback (progress/toast), pickers (color/date/file-preview/file-upload), and core (announce/floating/gesture); a component is one `RoleBehavior` row carrying its accessibility props, keyboard interaction map, and focus management drawn from the react-aria/react-stately headless hooks — the behavior is owned, the per-component `.tsx` markup is the consumer's, not the library's; `ThemeTokens` generates the token scale in a perceptually-uniform color space and exposes the token set as a typed record; `CssVarSync` writes the token record to `:root` CSS custom properties at runtime so a theme change is one token-record swap, never a re-render cascade.
- Entry: a stateful role (a selected collection, an open overlay, a slider value) reads and writes its state through the `binding.md` `AtomBinding`, never a second state binding; an interaction surface composes one `RoleBehavior` plus the theme tokens; the announce/live-region behavior is the one accessibility broadcast surface every role reuses, its live-region politeness resolved by `announceFor` over `Match.exhaustive` so each of the eight roles maps to exactly one `assertive`/`polite`/`off` arm and a new role without an arm is a typecheck failure; sanitized rich-text content passes through the DOM-purification boundary before render.
- Packages: `react-aria`, `react-aria-components`, and `react-stately` for the headless interaction hooks and the accessibility/keyboard/focus contracts, `colorjs.io` for the perceptually-uniform color-space token generation, `tailwindcss` for the utility token consumption, `isomorphic-dompurify` for the rich-text sanitization boundary, and `effect` for the `Schema.Literal` role vocabulary and the `Match` total dispatch over roles.
- Growth: a new interaction role lands as one literal on the `InteractionRole` owner-block; a new component behavior lands as one `RoleBehavior` row under its role leaf; a new token lands as one `ThemeTokens` field; a new theme lands as one token-record value, never a second sync surface.
- Boundary: the per-component react-aria `.tsx` implementation pattern is the discarded form — only the role vocabulary and the headless-behavior contract are owned; a component holding domain state is the named defect the binding deletes; a hand-rolled color computation or an inline `class-variance-authority` token outside `ThemeTokens` is the named defect; the CSS-var sync is the single theme-to-runtime path and a direct `document.documentElement.style` write outside `CssVarSync` is the named defect.

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

interface RoleBehavior<Props, State> {
  readonly role: InteractionRole;
  readonly behavior: (props: Props) => { readonly aria: Record<string, unknown>; readonly state: State; readonly focus: FocusManager };
}

const announceFor = (role: InteractionRole): "assertive" | "polite" | "off" =>
  Match.value(role).pipe(
    Match.whenOr("feedback", "overlays", () => "assertive" as const),
    Match.whenOr("actions", "inputs", "pickers", "navigation", () => "polite" as const),
    Match.whenOr("collections", "core", () => "off" as const),
    Match.exhaustive,
  );

interface ThemeTokens {
  readonly tokens: SubscriptionRef.SubscriptionRef<Record<string, string>>;
  readonly scale: (base: string, steps: number) => ReadonlyArray<string>;
  readonly swap: (next: Record<string, string>) => Effect.Effect<void>;
}

interface CssVarSync {
  readonly mount: Effect.Effect<void, never, Scope.Scope>;
}

const mountCssVarSync = (tokens: ThemeTokens): Effect.Effect<void, never, Scope.Scope> =>
  tokens.tokens.changes.pipe(
    Stream.runForEach((record) =>
      Effect.sync(() => {
        for (const [k, v] of Object.entries(record)) document.documentElement.style.setProperty(`--${k}`, v);
      })),
    Effect.forkScoped,
    Effect.asVoid,
  );
```
