# [UI_PRESENCE_OVERLAY]

The collaborative-awareness overlay over the settled `projection` presence and CRDT folds — live collaborator cursors, selections, and viewport frusta rendered as a floating cohort over the active document surface. Each peer is one `Atom.family` cell keyed by participant; the editing-surface claim reads the `projection/convergence/presence#PRESENCE` `ConflictPresenceStore.presence` `PresenceRowWire` TTL fold and the high-cadence cursor/selection/camera state reads the `causality-graph/version-vector#CRDT_SEMILATTICE` `CrdtField.EphemeralMap` `beat`/`leave` arm over `HlcWire` dominance — both settled C#-minted wire arms, never re-decoded and never re-minted here. Presence is a render-only projection composed through the `floating-anchor.md` placement owner; the overlay holds no domain state and never dials a presence transport.

## [1]-[INDEX]

One cluster: `PRESENCE_OVERLAY` owns the per-participant presence cohort, the durable-claim-plus-ephemeral-beat projection, and the floating cursor/selection surface.

## [2]-[PRESENCE_OVERLAY]

- Owner: `PresenceOverlay`, the collaborative-awareness surface over the settled `projection` presence and CRDT semilattice folds; `PresenceRow`, the render projection fusing the durable `PresenceRowWire` editing-surface claim (`actor`/`entityKind`/`entityKey`/`expiresAt`) the `projection/convergence/presence#PRESENCE` fold keys with the ephemeral cursor/selection/frustum bytes the `causality-graph/version-vector#CRDT_SEMILATTICE` `CrdtField.EphemeralMap` `beat` state carries — a projection over two settled wire arms, never a third wire shape; and the per-participant `Atom.family` subscription keyed by `actor` so each peer memoizes one cell. The cohort renders each live `PresenceRow` as a floating surface composing the `floating-anchor.md` placement owner over the active document surface (the `viewport/glb-viewport.md` canvas, the `cartography/geo-series-layer.md` map, or an `observation/observation-routes.md` route).
- Cases: each participant's presence is one `Atom.family((actor) => …)` cell folding the presence fold's row and the CRDT cell's `EphemeralMap` slot for that actor, so a remote cursor move (`beat`), a remote selection change (`beat`), and a remote camera frustum (`beat`) each update one cell without re-rendering the cohort; the editing-surface claim is the durable-TTL `PresenceRowWire` (dropped past `expiresAt` by the `presenceMerge` arm) and the cursor/selection/camera is the lossy `EphemeralMap` `beat` state dropped on `leave`, both distinct from the durable LWW-converged op-log state the `lww-merge#LWW_MERGE` scalar fold owns; the cohort renders over the active surface through the `floating-anchor.md` placement keyed to the decoded cursor coordinate, and a viewport frustum renders as a camera-state overlay on the `viewport` leaf.
- Entry: the overlay subscribes through the `binding/atom-binding.md` `AtomBinding` to the `projection/convergence/presence#PRESENCE` `ConflictPresenceStore.presence` `SubscriptionRef` and the `causality-graph/version-vector#CRDT_SEMILATTICE` `crdtStore` `EphemeralMap` cell; each participant cell is one `Atom.family` arg over `actor`; the cohort composes the `floating-anchor.md` placement; the overlay reads and never emits a presence op of its own — a local cursor move publishes through the `interchange` `CommandGateway` as one ephemeral presence intent, never a transport directly.
- Packages: `react`, `react-dom`, `@floating-ui/react`, `@effect-atom/atom`, `effect`; no presence/awareness transport package on this card.
- Growth: a new presence facet (a remote tool selection, a remote annotation-in-progress) lands as one decode arm over the `EphemeralMap` `beat` state bytes, never a second presence stream; a new surface the cohort overlays (a new document leaf) composes the same `PresenceOverlay`, never a parallel awareness surface; a new participant lands as one `Atom.family` arg, never a manual subscription.
- Boundary: a branch-side presence op vocabulary or a second CRDT op union beside the upstream `CrdtOpWire` is the named cross-language drift defect; a second awareness transport (a parallel WebSocket presence channel) beside the one `projection` fold is the named defect; a presence row read off the durable LWW register cell instead of the `presence` TTL fold or the `EphemeralMap` arm is the named defect; a hand-rolled `getBoundingClientRect` cursor placement beside the `floating-anchor.md` owner is the named defect; a local cursor move dialed at a transport directly instead of the `interchange` `CommandGateway` ephemeral intent is the named defect; the cursor/selection/frustum decode reads the `EphemeralMap` `beat` `state: Uint8Array` the C# `Awareness.Cursor`/`Selection`/`Camera` beat mints — re-minting the `AwarenessKind` discriminant or a parallel beat envelope is the named defect.

```ts contract
import type { PresenceRowWire } from "@rasm/interchange";
import { Atom } from "@effect-atom/atom";
import { Either, HashMap, Match, Option, Schema } from "effect";

const FrustumWire = Schema.Struct({ azimuth: Schema.Number, elevation: Schema.Number, distance: Schema.Number });

const BeatStateWire = Schema.parseJson(
  Schema.Union(
    Schema.Struct({ _tag: Schema.Literal("cursor"), point: Schema.Tuple(Schema.Number, Schema.Number) }),
    Schema.Struct({ _tag: Schema.Literal("selection"), ids: Schema.Array(Schema.String) }),
    Schema.Struct({ _tag: Schema.Literal("camera"), frustum: FrustumWire }),
  ),
);
type BeatState = typeof BeatStateWire.Type;

interface PresenceRow {
  readonly participantId: string;
  readonly entityKey: string;
  readonly expiresAt: string;
  readonly cursor: Option.Option<readonly [number, number]>;
  readonly selection: ReadonlyArray<string>;
  readonly frustum: Option.Option<typeof FrustumWire.Type>;
}

const decodeBeat = (state: Uint8Array): Option.Option<BeatState> =>
  Either.match(Schema.decodeUnknownEither(BeatStateWire)(new TextDecoder().decode(state)), {
    onLeft: () => Option.none(),
    onRight: (beat) => Option.some(beat),
  });

const foldBeat = (row: PresenceRow, beat: BeatState): PresenceRow =>
  Match.value(beat).pipe(
    Match.tagsExhaustive({
      cursor: (c) => ({ ...row, cursor: Option.some(c.point) }),
      selection: (s) => ({ ...row, selection: s.ids }),
      camera: (m) => ({ ...row, frustum: Option.some(m.frustum) }),
    }),
  );

const projectPresence = (claim: PresenceRowWire, beats: ReadonlyArray<BeatState>): PresenceRow =>
  beats.reduce(foldBeat, {
    participantId: claim.actor,
    entityKey: claim.entityKey,
    expiresAt: claim.expiresAt,
    cursor: Option.none(),
    selection: [],
    frustum: Option.none(),
  });

const presenceFamily = (
  rowsOf: (actor: string) => Option.Option<PresenceRowWire>,
  ephemeralOf: (actor: string) => HashMap.HashMap<string, { readonly state: Uint8Array }>,
): (actor: string) => Atom.Atom<Option.Option<PresenceRow>> =>
  Atom.family((actor: string) =>
    Atom.make(() =>
      Option.map(rowsOf(actor), (claim) =>
        projectPresence(
          claim,
          [...HashMap.values(ephemeralOf(actor))].flatMap((slot) => Option.toArray(decodeBeat(slot.state))),
        ))));
```

The durable editing-surface claim rides the `projection/convergence/presence#PRESENCE` `presenceMerge` TTL fold keyed `actor@entityKey`, and the high-cadence cursor/selection/camera state rides the `causality-graph/version-vector#CRDT_SEMILATTICE` `CrdtField.EphemeralMap` `live` map keyed by `origin` under `HlcWire` dominance — the C# `Awareness.Cursor`/`Selection`/`Camera`/`Follow` beats minting the `EphemeralMap` `beat` `state` bytes, the `leave` arm dropping a departed peer. `BeatStateWire` decodes the `beat` `state: Uint8Array` (the C# beat `payload`) through `Schema.parseJson` into the `BeatState` family; `projectPresence` folds the decoded beats onto the durable claim, so one render row carries the participant identity, the TTL horizon, the live cursor, the selection set, and the camera frustum off two settled wire arms with no third shape. The `Atom.family`/`Atom.make` constructors are the `atom-binding#ATOM_BINDING` surface; the `floating-anchor#FLOATING_ANCHOR` placement keys to the decoded `cursor` coordinate.

RESEARCH [BEAT_PAYLOAD]: the C# `Awareness` beat `state` byte layout — the `Awareness.Cursor(point)`/`Selection(halo)`/`Camera(frustum)` `ReadOnlyMemory<byte>` payload encoding the `BeatState` family decodes — confirms against the C# `csharp:Rasm.Persistence/sync/collaboration#PRESENCE_AND_BLOB` `Awareness` beat owner at cross-language alignment; the `BeatState` tagged shape above is the decode target and the JSON `parseJson` admission is the assumed beat encoding until the C# `Awareness` beat serializer (JSON versus MessagePack `state` bytes) is pinned. The `PresenceRowWire` claim shape, the `EphemeralMap` `beat`/`leave` arm, and the `Atom.family`/`floating-anchor` composition are verified and settled.
