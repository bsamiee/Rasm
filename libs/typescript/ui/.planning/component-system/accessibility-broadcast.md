# [UI_ACCESSIBILITY_BROADCAST]

The one accessibility-broadcast surface every interaction role reuses — the live-region announce path and the external-store toast queue. Politeness is one `as const satisfies Record<InteractionRole, "assertive" | "polite" | "off">` row table over the `role-behavior.md` `InteractionRole` so each of the eight roles maps to exactly one value by indexed access, and a role missing its row is a compile error. The toast queue is one external store triggered from anywhere, following the alertdialog landmark pattern with F6 region navigation. The page composes the role vocabulary and holds no domain state.

## [1]-[INDEX]

One cluster: `ACCESSIBILITY_BROADCAST` owns the live-region announce path and the external-store toast queue.

## [2]-[ACCESSIBILITY_BROADCAST]

- Owner: `_Politeness`, the `as const satisfies Record<InteractionRole, "assertive" | "polite" | "off">` row table that maps each role to its live-region politeness; `announceFor`, the indexed-access read over it; the live-region broadcast every role reuses; and `ToastBroadcast`, the external-store toast queue triggered from any leaf and rendered through one toast region. The announce path is the single live-region surface, never a per-role region.
- Cases: `announceFor` reads `_Politeness[role]` by indexed access, so feedback and overlays announce assertively, actions/inputs/pickers/navigation announce politely, and collections/core stay off; the `satisfies Record<InteractionRole, ...>` constraint makes a role missing its row a compile error, never a `Match` chain re-encoding the vocabulary. `ToastBroadcast` holds the queue in an external store so a toast fires from a non-React effect, a fold, or a leaf; the toast region renders the queue as an aria-live landmark reachable by F6, following the alertdialog pattern for an assertive interrupt.
- Entry: any role that produces a status, completion, or error message routes it through the one announce path; the toast queue drains its external store into the toast region mounted once at the application root; the broadcast composes no domain state and reads no `projection` fold of its own.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `@react-aria/live-announcer`, `effect`.
- Growth: a new politeness assignment lands as one `_Politeness` row keyed by an existing role; a new role on the `InteractionRole` owner-block forces a new row under the `satisfies` constraint; a new toast kind lands as one queue entry shape, never a second region or a second announce path.
- Boundary: a `Match` chain re-encoding the role-to-politeness map beside the `_Politeness` table is the named defect; a per-role live region beside the one announce path is the named defect; a toast triggered through component state instead of the external store is the named defect; the toast region is mounted once at the root and a second region is the named defect.

```ts contract
const _Politeness = {
  feedback:    "assertive",
  overlays:    "assertive",
  actions:     "polite",
  inputs:      "polite",
  pickers:     "polite",
  navigation:  "polite",
  collections: "off",
  core:        "off",
} as const satisfies Record<InteractionRole, "assertive" | "polite" | "off">;

const announceFor = (role: InteractionRole): (typeof _Politeness)[InteractionRole] => _Politeness[role];

interface ToastBroadcast<A> {
  readonly add: (content: A, options?: { readonly timeout?: number }) => Effect.Effect<void>;
  readonly close: (key: string) => Effect.Effect<void>;
}
```

RESEARCH [TOAST_QUEUE]: the `react-aria-components` `ToastQueue`/`useToastRegion`/`Toast`/`ToastRegion` external-store toast surface and the `@react-aria/live-announcer` `announce(message, politeness)` member are unverified; the `ToastQueue` constructor signature, the `useToastRegion` hook shape, and the `announce` spelling stay RESEARCH until the folder `.api/` catalogue carries the rows.
