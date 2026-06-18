# [UI_ACCESSIBILITY_BROADCAST]

The one accessibility-broadcast surface every interaction role reuses — the live-region announce path and the external-store toast queue. Politeness is the `politeness` column on the `role-behavior.md` `_Roles` vocabulary, so the announce path composes the vocabulary's `announceFor` projection by reference and a role missing its column is a compile error at the vocabulary, never a second record keyed on the same literal. The toast queue is one external store triggered from anywhere, following the alertdialog landmark pattern with F6 region navigation. The page composes the role vocabulary and holds no domain state.

## [1]-[INDEX]

One cluster: `ACCESSIBILITY_BROADCAST` owns the live-region announce path and the external-store toast queue.

## [2]-[ACCESSIBILITY_BROADCAST]

- Owner: the live-region broadcast every role reuses over the `role-behavior.md` `announceFor` projection — the indexed-access read of the `_Roles[role].politeness` column returning the vocabulary's derived `Politeness` type, defined where `_Roles` is module-visible and composed here by reference; and `ToastBroadcast`, the external-store toast queue triggered from any leaf and rendered through one toast region. The announce path is the single live-region surface, never a per-role region; `announceFor`, the role-to-politeness map, and its `Politeness` type are owned by the role vocabulary, never re-declared here.
- Cases: the composed `announceFor` reads `_Roles[role].politeness` by indexed access, so feedback and overlays announce assertively, actions/inputs/pickers/navigation announce politely, and collections/core stay off; the role-vocabulary `satisfies` constraint makes a role missing its politeness column a compile error at the owner, never a `Match` chain re-encoding the vocabulary and never a parallel record beside the roles. `ToastBroadcast` holds the queue in an external store so a toast fires from a non-React effect, a fold, or a leaf; the toast region renders the queue as an aria-live landmark reachable by F6, following the alertdialog pattern for an assertive interrupt.
- Entry: any role that produces a status, completion, or error message routes it through the one announce path; the toast queue drains its external store into the toast region mounted once at the application root; the broadcast composes no domain state and reads no `projection` fold of its own.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `@react-aria/live-announcer`, `effect`.
- Growth: a new politeness assignment lands as one column value on an existing `_Roles` row; a new role on the `_Roles` owner-block forces its politeness column under the `satisfies` constraint; a new toast kind lands as one queue entry shape, never a second region or a second announce path.
- Boundary: a politeness record keyed on `InteractionRole` re-declared beside the role vocabulary, or a `Match` chain re-encoding the role-to-politeness map, is the named defect the column deletes; a per-role live region beside the one announce path is the named defect; a toast triggered through component state instead of the external store is the named defect; the toast region is mounted once at the root and a second region is the named defect.

```ts contract
import { announceFor } from "@rasm/ts/ui/component-system/role-behavior";

interface ToastBroadcast<A> {
  readonly add: (content: A, options?: { readonly timeout?: number }) => Effect.Effect<void>;
  readonly close: (key: string) => Effect.Effect<void>;
}
```

RESEARCH [TOAST_QUEUE]: the `react-aria-components` `ToastQueue`/`useToastRegion`/`Toast`/`ToastRegion` external-store toast surface and the `@react-aria/live-announcer` `announce(message, politeness)` member are unverified; the `ToastQueue` constructor signature, the `useToastRegion` hook shape, and the `announce` spelling stay RESEARCH until the folder `.api/` catalogue carries the rows.
