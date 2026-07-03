# [UI_TRANSITION]

`act/transition.ts` owns document-level motion: the native View Transitions API is the current transition rail — `Transition.run(scope)` snapshots the document, commits the state change synchronously through `flushSync`, and animates between snapshots, degrading to a plain commit where the API or motion budget is absent — while React's `<ViewTransition>`/`addTransitionType` stay a gated upgrade row (`[R16]`) and `<Activity>` is the stable pre-render/hide row consumers compose today. Element-level enter/exit motion is NOT this page: RAC surfaces animate through `entering:`/`exiting:` variants over `token/scale`'s `Motion` rows, and floating overlays through `useTransitionStyles` — this module owns only the whole-document crossfade and the concurrency rows around it.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                        |
| :-----: | :----------------- | :----------------------------------------------------------------------------- |
|   [1]   | `NATIVE_RAIL`      | `Transition` — the `startViewTransition` + `flushSync` rail with degrade policy |
|   [2]   | `ACTIVITY_ROW`     | the stable `<Activity>` pre-render/hide row and its frame-loop consequence      |
|   [3]   | `GATED_UPGRADE`    | the `[R16]` `<ViewTransition>`/`addTransitionType` row and the degrade law      |

## [2]-[NATIVE_RAIL]

- Owner: `Transition` — one entrypoint: `Transition.run(commit, options?)` gates on capability (`document.startViewTransition` present) and motion budget (`prefers-reduced-motion`), wraps the state commit in `flushSync` inside the transition callback so the DOM the browser snapshots is the post-commit DOM, and returns the transition's `finished` promise lifted onto the rail; absent the API or under reduced motion the commit runs bare — the caller never branches.
- Packages: `react-dom` (`flushSync` — the synchronous commit the snapshot requires), the platform View Transitions API (lib.dom), `effect` (`Effect.tryPromise` lifting `finished`).
- Entry: route changes, panel-set swaps, theme flips — any whole-surface state change whose old/new crossfade earns a document snapshot; per-element motion stays on `Motion` rows.
- Law: the commit inside `run` is synchronous by construction — an async commit leaves the snapshot pair torn; awaited work completes BEFORE `run` and the transition wraps only the final atom write.
- Law: named transition regions are CSS data — `view-transition-name` styles assign region identity in the stylesheet (through `cn` where dynamic), and `::view-transition-*` pseudo-element animation is authored beside the token stylesheet; this module never touches per-region JS.
- Law: reduced motion degrades to instant — the gate reads `matchMedia("(prefers-reduced-motion: reduce)")` at call time, mirroring `Motion`'s `motion-reduce:` law at the document tier.
- Boundary: `flushSync` also serves `FocusScope` restoration (`view/primitive`); the atom write being committed is `atom/binding` material; interrupted-transition policy (a second `run` while one is live skips the old — the platform's own `skipTransition` semantics) rides the returned handle.

```typescript
import { Effect } from "effect"
import { flushSync } from "react-dom"

declare namespace Transition {
  type Options = { readonly force?: boolean }
}

const _eligible = (force: boolean): boolean =>
  typeof globalThis.document.startViewTransition === "function"
  && (force || !globalThis.matchMedia("(prefers-reduced-motion: reduce)").matches)

const Transition: {
  readonly run: (commit: () => void, options?: Transition.Options) => Effect.Effect<void>
} = {
  run: (commit, options) =>
    _eligible(options?.force ?? false)
      ? Effect.tryPromise({
          try: () => globalThis.document.startViewTransition(() => flushSync(commit)).finished,
          catch: () => undefined,
        }).pipe(Effect.ignore)
      : Effect.sync(() => flushSync(commit)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Transition }
```

## [3]-[ACTIVITY_ROW]

- Law: `<Activity mode="hidden">` is the stable pre-render/hide row — a subtree keeps its state and defers its effects while hidden, and pre-renders a cold route before navigation; the mode value is the whole knob and rides an atom-derived string.
- Law: hidden means paused — the `viewer` frame loops read the activity state and park (`setAnimationLoop(null)`, overlay `setProps` withheld) while their viewport subtree is hidden; a render loop burning under a hidden `Activity` is the named defect, and the wake path re-arms the loop on `mode` flipping visible.
- Law: `Activity` composes with `Suspense` — a hidden pre-render suspends and resolves in the background, so navigation lands on a warm subtree; the fallback never flashes for a pre-rendered route.
- Boundary: which routes pre-render is app routing policy (`browser` owns navigation); the loop-park consequence is consumed by `viewer/scene/glb` and `viewer/geo/layers` as settled law.

## [4]-[GATED_UPGRADE]

- Law: `<ViewTransition>` and `addTransitionType` are `[R16]` — typed in `@types/react` `./canary`, absent from the stable 19.2 runtime, so no shipping row imports them; the gate closing rewrites `Transition.run`'s interior to the component form and deletes zero call sites because the entrypoint signature already owns the modality.
- Law: canary imports are fenced — a `./canary` import on the stable path is the named defect; the upgrade row lives behind a capability flag until the runtime carries the exports.
- Law: the degrade chain is total — `<ViewTransition>` (gated) → native `startViewTransition` (current) → bare `flushSync` commit (floor); every tier preserves the commit semantics, so callers are transition-agnostic by construction and no public tier probe exists — a caller branching on the tier would re-open the modality `Transition.run` already owns.
