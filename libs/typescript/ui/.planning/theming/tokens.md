# [UI_TOKENS]

The design-token owner distinct in kind from interaction behavior. `ThemeTokens` generates a perceptually-uniform OKLCH scale and emits it as the Tailwind CSS-variable layer through one `CssVarSync` Stream fold, so a theme is one token-record swap and Tailwind utilities resolve the live variables at runtime with no re-render cascade. Beside the token scale the page registers the `tw-animate-css` enter/exit utility layer as the second styling substrate — a pure CSS `@import` carrying the `data-[state=open|closed]:animate-*` Radix-data-state selectors every `@radix-ui/*`-derived overlay emits (the `interaction/command.md#COMMAND_SURFACE` `cmdk` `Command.Dialog` and `vaul` `Drawer`) and the `@media (prefers-reduced-motion: reduce)` keyframe collapse the `interaction/transition.md#VIEW_TRANSITIONS` reduced-motion gate rides — so component enter/exit and route-level motion suppression are declarative utilities resolving against the live `@theme` OKLCH tokens, never a JS animation controller. The page composes the `binding/atom.md` `AtomBinding` for the active-theme cell and holds no domain state.

## [1]-[INDEX]

- [1]-[THEME_TOKENS]: the `ThemeTokens` OKLCH token scale via `colorjs.io`, the `CssVarSync` CSS-variable runtime sync, and the `tw-animate-css` `data-[state]` enter/exit utility layer.

## [2]-[THEME_TOKENS]

- Owner: `ThemeTokens`, the color-space-aware token owner generating a perceptually-uniform OKLCH scale; and `CssVarSync`, the single Tailwind-CSS-variable runtime path binding the token source to the live document `:root` custom-property layer. A theme is one token-record value; the CSS-var sync is the single theme-to-runtime path.
- Cases: `oklchScale` generates the token scale through the `colorjs.io` `Color.steps` perceptual interpolant in the OKLCH space between an `l: 0.98` light anchor and an `l: 0.12` dark anchor, serializing each step with `toString({ format: "oklch" })`; `contrastWith` reads the `Color.contrastWCAG21` ratio for the accessibility gate; `ThemeTokens` exposes the token set as a typed record and derives contrast-safe and dark/high-contrast scales as sibling token-records over the same generation fold; `CssVarSync` writes the token record to the `:root` CSS custom properties at runtime through one `Stream` fold so a theme change is one record swap and Tailwind utilities resolve the live variables with no re-render cascade. The animation substrate is the `tw-animate-css` CSS layer carrying no JS runtime: the `@import "tw-animate-css"` directive lands at the design-token stylesheet root immediately after `@import "tailwindcss"` and beside the `@theme` OKLCH token layer; the `data-[state=open]:animate-in` enter selector and the `data-[state=closed]:animate-out` exit selector bind the keyframe set to the Radix-style `data-state` attribute every `@radix-ui/*`-derived overlay emits, with the `animate-in`/`animate-out` base utilities composed against `fade-*`/`zoom-*`/`slide-in-from-*`/`spin` modifiers and the `duration-*`/`delay-*`/`ease-*` timing utilities resolving against the live `@theme` token values; the `@media (prefers-reduced-motion: reduce)` row is the keyframe-collapse the layer supplies natively, so a motion-suppressed render is correct with no JS gate. These three selectors are rows of the one styling substrate — the enter selector, the exit selector, and the reduced-motion collapse — never a sibling stylesheet beside the token layer.
- Entry: the active-theme cell reads and writes through the `binding/atom.md` `AtomBinding`; a theme swap pushes one token-record value and `CssVarSync` propagates it to the document; the token record is the single source consumed by both Tailwind utilities (the `@theme` layer) and runtime CSS; the `@import "tw-animate-css"` layer is global once present at the stylesheet root and is never re-imported per component, the `data-[state]` enter/exit utilities and the reduced-motion collapse available to every Tailwind class consumer thereafter.
- Packages: `colorjs.io`, `tailwindcss`, `tailwind-merge`, `class-variance-authority`, `tw-animate-css`, `effect`.
- Growth: a new token lands as one `ThemeTokens` field; a new theme lands as one token-record value; a new perceptual scale (high-contrast, color-blind-safe) lands as one derived record over the same generation fold, never a second sync surface; a new enter/exit motion lands as one `data-[state]` utility composition row over the `tw-animate-css` layer keyed by the component's own `data-state`, never a hand-authored `@keyframes` block.
- Boundary: a hand-rolled OKLCH ramp or WCAG contrast formula beside the `colorjs.io` `Color.steps`/`contrastWCAG21` owner is the named defect the catalogue forbids; an inline `class-variance-authority` token outside `ThemeTokens` is the named defect; the CSS-var sync is the single theme-to-runtime path and a direct `document.documentElement.style` write outside `CssVarSync` is the named defect — extended to animation, a direct `document.documentElement.style` animation write or a JS animation controller beside the declarative `data-[state]` utility layer is the named defect; a hand-authored `@keyframes` for enter/exit the `tw-animate-css` layer already supplies is the named defect; token generation is decoupled from the `interaction/role.md` vocabulary and a token computed on a `RoleBehavior` row is the named defect.

```ts contract
import Color from "colorjs.io";
import { Effect, Scope, Stream, SubscriptionRef } from "effect";

// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
// The design-token stylesheet root composes the Tailwind engine, the `tw-animate-css`
// enter/exit utility layer, and the `@theme` OKLCH token layer in declaration order; the
// animation layer carries no JS surface and is registered once here, not per component:
//
//   @import "tailwindcss";
//   @import "tw-animate-css";
//   @theme {
//     /* the OKLCH token custom properties `CssVarSync` writes onto `:root` at runtime;
//        `duration-*`/`ease-*` animation utilities resolve their timing against these */
//   }
//   /* `data-[state=open]:animate-in`  — overlay enter keyframes keyed by the Radix data-state
//      `data-[state=closed]:animate-out` — overlay exit keyframes keyed by the Radix data-state
//      `@media (prefers-reduced-motion: reduce)` — the layer-native keyframe collapse the
//        `interaction/transition.md#VIEW_TRANSITIONS` reduced-motion gate rides */

interface ThemeTokens {
  readonly tokens: SubscriptionRef.SubscriptionRef<Record<string, string>>;
  readonly scale: (base: string, steps: number) => ReadonlyArray<string>;
  readonly contrastWith: (fg: string, bg: string) => number;
  readonly swap: (next: Record<string, string>) => Effect.Effect<void>;
}

const oklchScale = (base: string, steps: number): ReadonlyArray<string> => {
  const anchor = new Color(base).to("oklch");
  const light = anchor.clone();
  light.set("l", 0.98);
  const dark = anchor.clone();
  dark.set("l", 0.12);
  return light
    .steps(dark, { space: "oklch", steps })
    .map((c) => c.toString({ format: "oklch" }));
};

const contrastWith = (fg: string, bg: string): number => new Color(fg).contrastWCAG21(new Color(bg));

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
