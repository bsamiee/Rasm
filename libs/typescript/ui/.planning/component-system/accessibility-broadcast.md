# [UI_ACCESSIBILITY_BROADCAST]

The one accessibility-broadcast surface every interaction role reuses — the `@react-aria/live-announcer` live-region announce path and the `react-stately` `ToastQueue` external-store queue. Politeness is the `politeness` column on the `role-behavior.md` `_Roles` vocabulary, so the announce path composes the vocabulary's `announceFor` projection by reference, a role missing its column is a compile error at the vocabulary, and an `"off"` role short-circuits before `announce` since the `Assertiveness` axis admits only `'assertive' | 'polite'`. The toast queue is one `ToastQueue` instance triggered from anywhere, rendered through the one `UNSTABLE_ToastRegion` landmark with F6 region navigation. The page composes the role vocabulary and holds no domain state.

## [1]-[INDEX]

One cluster: `ACCESSIBILITY_BROADCAST` owns the live-region announce path and the external-store toast queue.

## [2]-[ACCESSIBILITY_BROADCAST]

- Owner: the live-region broadcast every role reuses over the `role-behavior.md` `announceFor` projection — the indexed-access read of the `_Roles[role].politeness` column returning the vocabulary's derived `Politeness` type, defined where `_Roles` is module-visible and composed here by reference; the `@react-aria/live-announcer` `announce(message, assertiveness?, timeout?)` imperative as the single process-wide live-region driver; and `ToastBroadcast`, the `react-stately` `ToastQueue` external-store queue triggered from any leaf and rendered through the one `react-aria-components` `UNSTABLE_ToastRegion`/`UNSTABLE_Toast` region. The announce path is the single live-region surface, never a per-role region; `announceFor`, the role-to-politeness map, and its `Politeness` type are owned by the role vocabulary, never re-declared here.
- Cases: the composed `announceFor` reads `_Roles[role].politeness` by indexed access, so feedback and overlays announce assertively, actions/inputs/pickers/navigation announce politely, and collections/core stay off; the role-vocabulary `satisfies` constraint makes a role missing its politeness column a compile error at the owner, never a `Match` chain re-encoding the vocabulary and never a parallel record beside the roles. The `Politeness` `"off"` arm carries no live-region announcement — the `@react-aria/live-announcer` `Assertiveness` admits only `'assertive' | 'polite'`, so an `"off"` role short-circuits before `announce` rather than coercing to a default level. `ToastBroadcast` holds the queue on a `ToastQueue` class instance so a toast fires from a non-React effect, a fold, or a leaf and `useToastQueue(queue)` subscribes the region; the `UNSTABLE_ToastRegion` renders the queue as an aria-live landmark reachable by F6, following the alertdialog pattern for an assertive interrupt.
- Entry: any role that produces a status, completion, or error message routes it through the one `announceRole` path that gates on the role politeness; the `ToastQueue` drains into the `UNSTABLE_ToastRegion` mounted once at the application root and subscribed through `useToastQueue`; the broadcast composes no domain state and reads no `projection` fold of its own.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `@react-aria/live-announcer`, `effect`.
- Growth: a new politeness assignment lands as one column value on an existing `_Roles` row; a new role on the `_Roles` owner-block forces its politeness column under the `satisfies` constraint; a new toast kind lands as one `ToastQueue<A>` content shape, never a second region or a second announce path.
- Boundary: a politeness record keyed on `InteractionRole` re-declared beside the role vocabulary, or a `Match` chain re-encoding the role-to-politeness map, is the named defect the column deletes; a per-role live region beside the one `announce` path is the named defect; an `"off"` politeness coerced to a default announce level rather than short-circuited is the named defect; a toast triggered through component state instead of the `ToastQueue` external store is the named defect; the `UNSTABLE_ToastRegion` is mounted once at the root and a second region is the named defect.

```ts contract
import { announceFor, type InteractionRole } from "@rasm/ts/ui/component-system/role-behavior";
import { announce } from "@react-aria/live-announcer";
import { ToastQueue } from "react-stately";
import { Effect, Match } from "effect";

const announceRole = (role: InteractionRole, message: string, timeout?: number): Effect.Effect<void> =>
  Match.value(announceFor(role)).pipe(
    Match.when("off", () => Effect.void),
    Match.orElse((level) => Effect.sync(() => announce(message, level, timeout))),
  );

interface ToastBroadcast<A> {
  readonly queue: ToastQueue<A>;
  readonly add: (content: A, options?: { readonly timeout?: number }) => Effect.Effect<string>;
  readonly close: (key: string) => Effect.Effect<void>;
}

const makeToastBroadcast = <A>(queue: ToastQueue<A>): ToastBroadcast<A> => ({
  queue,
  add: (content, options) => Effect.sync(() => queue.add(content, options)),
  close: (key) => Effect.sync(() => queue.close(key)),
});
```

The `react-stately` `ToastQueue<A>` constructor (`{ maxVisibleToasts?, hasExitAnimation? }`), the `add(content, options?) => key` queue-key return, and the `close(key)` dismissal are catalogued rows the `makeToastBroadcast` fence binds directly. The `ToastQueue`/`useToastQueue`/`UNSTABLE_ToastRegion`/`UNSTABLE_Toast` symbols and the `@react-aria/live-announcer` `announce(message, assertiveness?, timeout?)` are verified.
