# [PLATFORM_PERMISSION_GRANT_FOLD]

One page owns the permission-grant fold algebra the `capabilities/browser-capability#BROWSER_CAPABILITY` service composes — `PermissionState` (the closed `Granted`/`Denied`/`Prompt` `Data.TaggedEnum` each per-kind cell holds), `CapabilityFault` (the one `Unsupported`/`InsecureContext`/`Denied` `Data.TaggedEnum` fault family the package raises and `browser-capability` re-exports as its service error), `foldNativeState` (the DOM `PermissionState` string -> tag fold the `navigator.storage.persist` boolean rides the same arm of), `grantChanges` (the `PermissionStatus.change` live-edge stream lifted through `runtime-composition/scoped-event-stream#SCOPED_EVENT_STREAM` because a `PermissionStatus` is an `EventTarget` absent from `WindowEventMap` and so `BrowserStream.fromEventListenerWindow` never reaches it), and `rankInput` (the worst-admissible `RankKey` projection feeding `runtime-composition/capability-rank#CAPABILITY_RANK` its persistent-storage health input). It is the pure fold the service owner reaches for so the grant vocabulary, the change ingress, and the rank projection live in one place rather than re-derived at each `CapabilityKind` arm; `browser-capability` holds the `Effect.Service`, the per-kind `SubscriptionRef` table, and the `query`/`request` entrypoint, and this page authors no service, no `navigator.permissions` query, and crosses no wire — a parallel `PermissionState` enum or a hand-rolled `change`-listener at the service is the named duplicate-fold defect.

## [1]-[INDEX]

[PERMISSION_GRANT_FOLD]: the closed `PermissionState` vocabulary, the native-state-and-persist fold, the `PermissionStatus.change` live-edge stream, the `CapabilityFault` family, and the `capability-rank` projection.

## [2]-[PERMISSION_GRANT_FOLD]

- Owner: the permission-grant fold algebra — `PermissionState` is the one `Data.TaggedEnum` (`Granted`/`Denied`/`Prompt`) the per-kind cell carries, `foldNativeState` the one fold from the DOM `PermissionState` string (and the `StorageManager.persist`/`persisted` boolean, which folds through the same `Granted`/`Denied` arm) into that tag, `grantChanges` the one `PermissionStatus.change` edge stream the service subscribes per kind to keep its cell live, `CapabilityFault` the one fault family the package raises, and `rankInput` the one worst-admissible-`RankKey` projection the persistent-storage grant feeds `capability-rank`. A second `PermissionState` enum, a `granted`/`denied`/`prompt` boolean triple, or a `change`-listener authored at the `browser-capability` service is the named duplicate-fold defect.
- Cases: the native `PermissionState` is the W3C string union `"granted"`/`"denied"`/`"prompt"` and `foldNativeState` resolves it through one `_NativeGrant` `as const satisfies Record` whose row carries the domain `PermissionState` constructor, so a `Match.value(raw).pipe(...)` ladder re-encoding the three strings is the rejected form and an unrecognized string (a future `"granted-while-prompt"` host extension) resolves through `R.get(_NativeGrant, raw)` to the `Prompt` fallback rather than a throw; the `persistent-storage` grant has no `navigator.permissions` `PermissionName` — the host reports it through `StorageManager.persisted()` (a `boolean`, already-granted) and grants it through `StorageManager.persist()` (a `boolean`, granted-now), so `foldPersistGrant` folds that boolean through the same `Granted`/`Denied` arms the string fold owns, the one place the two ingress shapes (the `PermissionStatus.state` string and the `StorageManager` boolean) converge on the single `PermissionState` vocabulary; `grantChanges(status)` lifts a live `PermissionStatus` `EventTarget`'s `change` event through `scopedEventStream` and re-folds `status.state` on each edge into a `Stream<PermissionState>` the service drains into the kind's `SubscriptionRef`, so the cell tracks a runtime grant/revoke a user performs in the browser UI without a poll; `rankInput` projects a `PermissionState` to its worst-admissible `RankKey` through one `$match` — a `Denied` persistent-storage grant projects to `OfflineOnly` because the IndexedDB store risks eviction, a `Prompt` to `Degraded`, a `Granted` to `Full` — the one arm `capability-rank`'s candidate fold reads as the persistent-storage health input.
- Auto: `foldNativeState` is total over the `_NativeGrant` keyset and total-by-fallback over an unenumerated string, the row's `ctor` thunk constructing the `PermissionState` variant so the fold reads the vocabulary, never an inline `PermissionState.Granted()` ladder; `grantChanges` rides `scopedEventStream(register, release)` where `register` is `(emit) => { const h = () => emit(foldNativeState(status.state)); status.addEventListener("change", h); return h; }` and `release` is `(h) => status.removeEventListener("change", h)`, the listener attach/detach owned by the bridge's one `Effect.acquireRelease` so scope closure detaches exactly once and the first element is the seed-on-subscribe `status.state` fold prepended through `Stream.prepend`; `fromCause` folds the `@effect/platform-browser` `PermissionsError` (both its `"InvalidStateError"`/`"TypeError"` reasons fold to one `Unsupported` arm — the two-reason split carries no divergent projection here) and the secure-context/feature-absence host facts into the one `Unsupported`/`InsecureContext`/`Denied` family through one `Match` over the typed cause, so the service's `query`/`request` arms raise one fault family rather than three.
- Packages: `effect` `Data.taggedEnum` for the `PermissionState` and `CapabilityFault` families, `Match` for the `rankInput`/`fromCause` folds, `Record`/`Option` for the `_NativeGrant` open-world lookup, and `Stream.prepend`/`Stream.make` for the seed-on-subscribe change stream; `@effect/platform-browser` `Permissions` `PermissionStatus` (the `EventTarget` whose `state` and `change` event the fold reads) and `PermissionsError` (the `reason` the fault fold normalizes) as the upstream shapes — never re-imported into a second owner; `runtime-composition/scoped-event-stream#SCOPED_EVENT_STREAM` `scopedEventStream` for the `change` ingress because `PermissionStatus` is absent from `WindowEventMap` and `BrowserStream.fromEventListenerWindow` cannot type it; the native `StorageManager.persist`/`persisted`/`estimate` boolean the persist grant folds (no `@effect` surface — the named native call confined to the `capabilities/` sub-domain).
- Growth: a new permission state (a future W3C `PermissionState` member) lands as one row on `_NativeGrant` carrying its `ctor` plus one `PermissionState` variant and one `rankInput` `$match` arm, the `Record` fallback absorbing an unenumerated host string until the row lands; a new capability whose grant degrades a different rank lands as one `rankInput` `$match` arm, never a parallel projection; a new fault host-fact (a `permissions-policy` block) lands as one `CapabilityFault` reason and one `fromCause` arm; the `persistent-storage` boolean ingress generalizes to any future non-`Permissions`-backed grant (a `StorageBucket` durability flag) through one more `foldPersistGrant`-shaped boolean arm, never a second fold algebra.
- Boundary: this page is the pure fold the `capabilities/browser-capability#BROWSER_CAPABILITY` service composes — it holds no `Effect.Service`, no `SubscriptionRef`, queries no `navigator.permissions`, calls no `StorageManager.persist`, and crosses no wire; the service owns the per-kind cell table and the `query`/`request` entrypoint and reaches `foldNativeState`/`grantChanges`/`rankInput`/`fromCause` for the mechanic, so a `PermissionState` re-declaration, a `change`-listener, or a native-string `Match` ladder authored at the service is the named duplicate-fold defect; `rankInput` is read by `capability-rank`'s candidate fold as the persistent-storage health input (the one new `Order.max` source), never a behavior call; the `change` ingress rides `scopedEventStream` and never `BrowserStream.fromEventListenerWindow`, so a `PermissionStatus.change` routed through the keyed window constructor is the named redundant-ingress defect; `ui` reads the resulting per-kind grant cell through the `AtomBinding` at the service, never importing `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { PermissionsError } from "@effect/platform-browser/Permissions";
import type { RankKey } from "../runtime-composition/capability-rank.ts";
import { Data, Match, Option, Record as R, Stream } from "effect";
import { scopedEventStream } from "../runtime-composition/scoped-event-stream.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type PermissionState = Data.TaggedEnum<{
  readonly Granted: object;
  readonly Denied: object;
  readonly Prompt: object;
}>;
const PermissionState = Data.taggedEnum<PermissionState>();

// --- [ERRORS] --------------------------------------------------------------------------
type CapabilityFault = Data.TaggedEnum<{
  readonly Unsupported: { readonly cause: unknown };
  readonly InsecureContext: object;
  readonly Denied: { readonly cause: unknown };
}>;
const CapabilityFault = Data.taggedEnum<CapabilityFault>();

// --- [CONSTANTS] -----------------------------------------------------------------------
const _NativeGrant = {
  granted: { ctor: () => PermissionState.Granted() },
  denied: { ctor: () => PermissionState.Denied() },
  prompt: { ctor: () => PermissionState.Prompt() },
} as const satisfies Record<string, { ctor: () => PermissionState }>;

// --- [OPERATIONS] ----------------------------------------------------------------------
const foldNativeState = (raw: string): PermissionState => // boundary-widened: the host may emit a state outside the lib union
  R.get(_NativeGrant, raw).pipe(
    Option.map((row) => row.ctor()),
    Option.getOrElse(() => PermissionState.Prompt()),
  );

const foldPersistGrant = (persisted: boolean): PermissionState =>
  persisted ? PermissionState.Granted() : PermissionState.Denied();

const rankInput: (state: PermissionState) => RankKey = PermissionState.$match({
  Granted: () => "Full" as RankKey,
  Denied: () => "OfflineOnly" as RankKey,
  Prompt: () => "Degraded" as RankKey,
});

const isGranted: (state: PermissionState) => boolean = PermissionState.$match({
  Granted: () => true,
  Denied: () => false,
  Prompt: () => false,
});

const grantChanges = (status: PermissionStatus): Stream.Stream<PermissionState> =>
  scopedEventStream<PermissionState, () => void>(
    (emit) => {
      const handler = (): void => emit(foldNativeState(status.state)); // BOUNDARY ADAPTER: PermissionStatus.change
      status.addEventListener("change", handler);
      return handler;
    },
    (handler) => status.removeEventListener("change", handler),
  ).pipe(Stream.prepend(Stream.make(foldNativeState(status.state))));

const fromCause = (cause: unknown): CapabilityFault =>
  Match.value(cause).pipe(
    Match.when({ _tag: "PermissionsError" }, (e: PermissionsError) => CapabilityFault.Unsupported({ cause: e })),
    Match.orElse((c) =>
      globalThis.isSecureContext === false
        ? CapabilityFault.InsecureContext()
        : CapabilityFault.Denied({ cause: c }),
    ),
  );

// --- [EXPORTS] -------------------------------------------------------------------------
export {
  type CapabilityFault,
  type PermissionState,
  CapabilityFault,
  PermissionState,
  foldNativeState,
  foldPersistGrant,
  fromCause,
  grantChanges,
  isGranted,
  rankInput,
};
```
