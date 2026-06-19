# [PLATFORM_CAPABILITY]

One page owns the browser permission/host-capability grant edge — `BrowserCapability`, one closed `CapabilityKind` `Schema.Literal` axis (`notifications`/`clipboard-read`/`clipboard-write`/`geolocation`/`persistent-storage`), the per-kind `PermissionState` `Data.TaggedEnum` (`Granted`/`Denied`/`Prompt`) held in ONE `Record<CapabilityKind, SubscriptionRef<PermissionState>>` table never a sibling `Ref` per kind, the single `query`/`request` total-dispatch entrypoint that discriminates by `CapabilityKind` value, and the native `navigator.storage.persist`/`estimate` persistent-storage grant the offline cache reads for quota durability. It is the one permission-grant owner the host policies resolve through — a host owner reaching `navigator.notification`/`navigator.clipboard`/`navigator.storage.persist()`/`navigator.permissions` directly is the named ungated-native-call defect; the grant cell `ui` greys a denied affordance from and the denied-`persistent-storage` health input `folder:Runtime/rank.md#CAPABILITY_RANK` folds to `OfflineOnly` both read this one owner. Each `PermissionStatus.change` event rides `folder:Runtime/events.md#SCOPED_EVENT_STREAM` (a `PermissionStatus` is an `EventTarget` absent from `WindowEventMap`, so `BrowserStream.fromEventListenerWindow` does not reach it); the page authors no decode and crosses no wire.

## [1]-[INDEX]

- [1]-[BROWSER_CAPABILITY]: the closed `CapabilityKind` axis, the per-kind `PermissionState` cell table, the `query`/`request` total dispatch, and the native storage-persist grant.

## [2]-[BROWSER_CAPABILITY]

- Owner: `BrowserCapability`, the single browser permission-grant owner — one `CapabilityKind` `Schema.Literal` axis, one frozen `Record<CapabilityKind, SubscriptionRef<PermissionState>>` cell table (one `SubscriptionRef` per kind built once at boot, never N siblings scattered across the service), the `query` read-and-seed entrypoint discriminating `persistent-storage` (the `StorageManager.persisted()` ingress) from the four `Permissions.query`-backed kinds and the `request` prompt-and-fold entrypoint total-dispatching by `CapabilityKind` through `Match.exhaustive`, the native `navigator.storage.persist`/`persisted`/`estimate` persistent-storage grant, and the `storageRetained` `PermissionState` cell `Runtime/rank.md` reads as a health input. The service composes the `Shell/grant.md` fold algebra (`PermissionState`/`CapabilityFault`/`foldNativeState`/`foldPersistGrant`/`grantChanges`/`fromCause`/`isGranted`) and re-declares none of it; a private `navigator.permissions.query`/`Notification.requestPermission`/`navigator.storage.persist` call at any consumer is the named ungated-native-call defect.
- Cases: the four `navigator.permissions`-backed kinds map through the one `permissionName` data table to their DOM `PermissionName`, so `queryPermission(kind)` composes the verified `@effect/platform-browser` `Permissions.query` (`PermissionName`-generic, returning the narrowed `PermissionStatus`), folds the native `state` string through the `Shell/grant.md` `foldNativeState` into the `PermissionState` `Data.TaggedEnum`, seeds the kind's cell, and forks the fold's `grantChanges(status)` seed-prepended `change` stream into the same cell so a later host-side grant revocation advances the cell without a re-query — never a parallel listener per kind; `persistent-storage` has no `navigator.permissions` `PermissionName`, so `queryStorage` reads `navigator.storage.persisted()` and folds the boolean through `foldPersistGrant`, the one place the storage boolean ingress converges on the same `PermissionState` vocabulary; the polymorphic `query(kind)` discriminates `persistent-storage` to `queryStorage` and the rest to `queryPermission`. `request(kind)` is the one prompt entrypoint total-dispatching the active grant ceremony by kind through `Match.value(kind)` over the `Schema.Literal` string axis closed by `Match.exhaustive` — `notifications` rides `Notification.requestPermission()` folded through `foldNativeState`, `clipboard-read`/`clipboard-write` ride the verified `Clipboard` capsule's `readString`/`writeString` as the prompt trigger, `geolocation` rides the verified `Geolocation.getCurrentPosition` one-shot, and `persistent-storage` rides the native `navigator.storage.persist()` folded through `foldPersistGrant` — each ceremony's outcome folding back through the shared grant vocabulary and updating the kind's cell, so the prompt result and the queried state share one fold and one cell; `retained(kind)` is the one grant query a consumer reads — one `SubscriptionRef.get` over the kind's cell projected to `Granted` membership through the fold's `isGranted` — so `ui` greys a denied clipboard affordance by asking `retained("clipboard-write")` rather than reading the native `permissions` API, the read riding the effect channel because the live cell is never read off-effect; `storageRetained` is the `persistent-storage` cell `Runtime/rank.md` joins, a denied persist grant degrading the rank toward `OfflineOnly` since the IndexedDB store risks eviction under memory pressure.
- Entry: `ui` reads each kind's grant cell and the `retained` query through the `AtomBinding` to grey a denied affordance, never importing `platform`; `Runtime/rank.md` joins the `storageRetained` cell as its fifth health input over the merged-stream recompute; `local-persistence` reads the `persistent-storage` grant before relying on durable IndexedDB quota; `request` runs the active grant ceremony in-place and emits no command, so the prompt never rides the `interchange` `CommandGateway`.
- Packages: `effect` `SubscriptionRef` for the per-kind cell table, `Match.value`/`Match.exhaustive` for the `request` ceremony dispatch over the `CapabilityKind` `Schema.Literal` string axis, `Stream.runForEach`/`Effect.forkScoped` for the per-cell `grantChanges` ingress, and `Schema.Literal` for the `CapabilityKind` axis; `@effect/platform-browser` `Permissions.query` (verified `PermissionName`-generic narrowed `PermissionStatus`), `Clipboard` (verified `readString`/`writeString` prompt triggers), and `Geolocation` (verified `getCurrentPosition` one-shot) as the admitted capsules behind the one axis; the native `navigator.storage.persist`/`persisted`/`estimate` `StorageManager` and `Notification.requestPermission` for the two surfaces no `@effect` binding covers, confined to this owner; `Shell/grant.md#PERMISSION_GRANT_FOLD` for the `PermissionState`/`CapabilityFault` vocabulary, the `foldNativeState`/`foldPersistGrant`/`grantChanges`/`fromCause`/`isGranted` fold algebra (the `change` ingress rides `scopedEventStream` inside `grantChanges`, never re-spelled here); `BrowserPlatform` for the `Permissions`/`Clipboard`/`Geolocation` layer binding.
- Growth: a new permission surface lands as one row on the `CapabilityKind` axis plus one row on the `permissionName` table and one `request` ceremony arm, the closed `Schema.Literal` discriminant and the `Match.exhaustive` request dispatch breaking every site at compile time until the arm is authored — never a parallel capability owner; a Web-Push subscription grant lands as one `notifications`-adjacent ceremony arm reusing the same cell fold; the eventual single AppHost `CapabilityDescriptor` catalog (`CAPABILITY_CONTROL_PLANE`/`ONE_CAPABILITY_CATALOG`) absorbs these browser host-capability grants by aligning the `CapabilityKind` axis descriptor-shaped, so the browser is the descriptor-aligned host consumer rather than a parallel grant model.
- Boundary: `BrowserCapability` is the single permission-grant owner — it holds the per-kind cell table, the `query`/`request` dispatch, and the native storage grant, so any native `navigator.notification`/`navigator.clipboard`/`navigator.storage`/`navigator.permissions` call outside this owner is the named ungated-native-call defect; the per-kind cell is ONE `Record<CapabilityKind, SubscriptionRef<PermissionState>>` built once at boot, so an `N`-sibling-`Ref`-per-kind shape is the named parallel-cell defect; the cell is advanced only by the `query` seed, the `request` ceremony fold, and the `change` ingress — a consumer never sets it; `retained`/`storageRetained` are read-only projections holding no behavior, so a grant-side retry or redial authored here is the god-object defect; the `Notification.requestPermission`/`navigator.storage.persist`/`estimate` `BOUNDARY ADAPTER` calls are the platform-forced statement seam, no other site issues them; `BrowserCapability` emits no command, dials no transport, authors no decode, and `ui` reads the grant cells through the `AtomBinding`, never importing `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Effect, Match, Record as Rec, Schema, Stream, SubscriptionRef } from "effect";
import * as Permissions from "@effect/platform-browser/Permissions";
import * as Clipboard from "@effect/platform-browser/Clipboard";
import * as Geolocation from "@effect/platform-browser/Geolocation";
import {
  type CapabilityFault,
  type PermissionState,
  PermissionState as PermissionStateEnum,
  foldNativeState,
  foldPersistGrant,
  fromCause,
  grantChanges,
  isGranted,
} from "./permission-grant-fold.ts";

// --- [TYPES] ---------------------------------------------------------------------------
const CapabilityKind = Schema.Literal(
  "notifications",
  "clipboard-read",
  "clipboard-write",
  "geolocation",
  "persistent-storage",
);
type CapabilityKind = typeof CapabilityKind.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------
const permissionName = {
  notifications: "notifications",
  "clipboard-read": "clipboard-read",
  "clipboard-write": "clipboard-write",
  geolocation: "geolocation",
  "persistent-storage": "persistent-storage",
} as const satisfies Record<CapabilityKind, PermissionName>;

const allKinds: ReadonlyArray<CapabilityKind> = CapabilityKind.literals;

// --- [SERVICES] ------------------------------------------------------------------------
interface BrowserCapability {
  readonly grants: Record<CapabilityKind, SubscriptionRef.SubscriptionRef<PermissionState>>;
  readonly query: (kind: CapabilityKind) => Effect.Effect<PermissionState, CapabilityFault>;
  readonly request: (kind: CapabilityKind) => Effect.Effect<PermissionState, CapabilityFault>;
  readonly retained: (kind: CapabilityKind) => Effect.Effect<boolean>;
  readonly storageRetained: SubscriptionRef.SubscriptionRef<PermissionState>;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
class BrowserCapabilityLive extends Effect.Service<BrowserCapabilityLive>()(
  "@rasm/ts/platform/BrowserCapability",
  {
    scoped: Effect.gen(function* () {
      const permissions = yield* Permissions.Permissions;
      const clipboard = yield* Clipboard.Clipboard;
      const geolocation = yield* Geolocation.Geolocation;

      const grants = yield* Effect.forEach(allKinds, (kind) =>
        SubscriptionRef.make<PermissionState>(PermissionStateEnum.Prompt()).pipe(
          Effect.map((cell) => [kind, cell] as const),
        ),
      ).pipe(Effect.map((pairs) => Rec.fromEntries(pairs) as Record<CapabilityKind, SubscriptionRef.SubscriptionRef<PermissionState>>));

      // persistent-storage has no `navigator.permissions` PermissionName — it reads through StorageManager.persisted()
      const queryStorage: Effect.Effect<PermissionState, CapabilityFault> =
        Effect.tryPromise({ try: () => navigator.storage.persisted(), catch: fromCause }).pipe(
          Effect.map(foldPersistGrant),
          Effect.tap((seed) => SubscriptionRef.set(grants["persistent-storage"], seed)),
        );

      const queryPermission = (kind: Exclude<CapabilityKind, "persistent-storage">): Effect.Effect<PermissionState, CapabilityFault> =>
        permissions.query(permissionName[kind]).pipe(
          Effect.mapError(fromCause),
          Effect.flatMap((status) => {
            const seed = foldNativeState(status.state);
            return SubscriptionRef.set(grants[kind], seed).pipe(
              Effect.zipRight(
                grantChanges(status).pipe(
                  Stream.runForEach((next) => SubscriptionRef.set(grants[kind], next)),
                  Effect.forkScoped,
                ),
              ),
              Effect.as(seed),
            );
          }),
        );

      const query = (kind: CapabilityKind): Effect.Effect<PermissionState, CapabilityFault> =>
        kind === "persistent-storage" ? queryStorage : queryPermission(kind);

      const request = (kind: CapabilityKind): Effect.Effect<PermissionState, CapabilityFault> =>
        Match.value(kind).pipe(
          Match.when("notifications", () =>
            Effect.tryPromise({
              try: () => Notification.requestPermission(), // BOUNDARY ADAPTER: notification prompt
              catch: fromCause,
            }).pipe(Effect.map(foldNativeState)),
          ),
          Match.when("clipboard-read", () =>
            clipboard.readString.pipe(
              Effect.as(PermissionStateEnum.Granted()),
              Effect.catchAll(() => Effect.succeed(PermissionStateEnum.Denied())),
            ),
          ),
          Match.when("clipboard-write", () =>
            clipboard.writeString("").pipe(
              Effect.as(PermissionStateEnum.Granted()),
              Effect.catchAll(() => Effect.succeed(PermissionStateEnum.Denied())),
            ),
          ),
          Match.when("geolocation", () =>
            geolocation.getCurrentPosition().pipe(
              Effect.as(PermissionStateEnum.Granted()),
              Effect.catchAll(() => Effect.succeed(PermissionStateEnum.Denied())),
            ),
          ),
          Match.when("persistent-storage", () =>
            Effect.tryPromise({
              try: () => navigator.storage.persist(), // BOUNDARY ADAPTER: StorageManager.persist grant
              catch: fromCause,
            }).pipe(Effect.map(foldPersistGrant)),
          ),
          Match.exhaustive,
        ).pipe(Effect.tap((next) => SubscriptionRef.set(grants[kind], next)));

      const retained = (kind: CapabilityKind): Effect.Effect<boolean> =>
        SubscriptionRef.get(grants[kind]).pipe(Effect.map(isGranted));

      yield* Effect.forEach(allKinds, (kind) => query(kind).pipe(Effect.ignore), { discard: true });

      return {
        grants,
        query,
        request,
        retained,
        storageRetained: grants["persistent-storage"],
      } satisfies BrowserCapability;
    }),
  },
) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type BrowserCapability, type CapabilityFault, type CapabilityKind, type PermissionState, BrowserCapabilityLive };
export { PermissionState as PermissionStateEnum } from "./permission-grant-fold.ts";
```
