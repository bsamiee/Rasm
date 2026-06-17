# [UI_THEME_TOKENS]

The design-token owner distinct in kind from interaction behavior. `ThemeTokens` generates a perceptually-uniform OKLCH scale and emits it as the Tailwind CSS-variable layer through one `CssVarSync` Stream fold, so a theme is one token-record swap and Tailwind utilities resolve the live variables at runtime with no re-render cascade. The page composes the `binding/atom-binding.md` `AtomBinding` for the active-theme cell and holds no domain state.

## [1]-[INDEX]

One cluster: `THEME_TOKENS` owns the OKLCH token scale and the `CssVarSync` runtime variable sync.

## [2]-[THEME_TOKENS]

- Owner: `ThemeTokens`, the color-space-aware token owner generating a perceptually-uniform OKLCH scale; and `CssVarSync`, the single Tailwind-CSS-variable runtime path binding the token source to the live document `:root` custom-property layer. A theme is one token-record value; the CSS-var sync is the single theme-to-runtime path.
- Cases: `ThemeTokens` generates the token scale in the OKLCH perceptual color space, exposes the token set as a typed record, and derives contrast-safe and dark/high-contrast scales as sibling token-records over the same generation fold; `CssVarSync` writes the token record to the `:root` CSS custom properties at runtime through one `Stream` fold so a theme change is one record swap and Tailwind utilities resolve the live variables with no re-render cascade.
- Entry: the active-theme cell reads and writes through the `binding/atom-binding.md` `AtomBinding`; a theme swap pushes one token-record value and `CssVarSync` propagates it to the document; the token record is the single source consumed by both Tailwind utilities (the `@theme` layer) and runtime CSS.
- Packages: `colorjs.io`, `tailwindcss`, `tailwind-merge`, `class-variance-authority`, `effect`.
- Growth: a new token lands as one `ThemeTokens` field; a new theme lands as one token-record value; a new perceptual scale (high-contrast, color-blind-safe) lands as one derived record over the same generation fold, never a second sync surface.
- Boundary: a hand-rolled color computation or an inline `class-variance-authority` token outside `ThemeTokens` is the named defect; the CSS-var sync is the single theme-to-runtime path and a direct `document.documentElement.style` write outside `CssVarSync` is the named defect; token generation is decoupled from the `component-system/role-behavior.md` vocabulary and a token computed on a `RoleBehavior` row is the named defect.

```ts contract
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

RESEARCH [OKLCH_GENERATION]: the `colorjs.io` `Color`/`range`/`steps` constructors that generate the perceptually-uniform OKLCH ramp and the WCAG contrast computation are unverified; the ramp-generation and contrast-pairing member spellings stay RESEARCH until the folder `.api/` catalogue carries the `colorjs.io` rows.
