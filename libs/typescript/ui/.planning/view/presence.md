# [UI_PRESENCE]

Presence owns the faces of the collaborative session as render projections over the core presence algebra: `Presence.roster`/`crowd`/`status` verdicts arrive settled from `core/state/presence`, ops leave as `Presence.Move` values, and this page computes no liveness, no merge, and no clock. `Anchor` registers one coordinate space per surface, so a comment pins to a paragraph, a graph node, an image region, or an element with zero per-surface overlay code and no lateral view→view import. Module: `ui/src/view/presence.ts`.

Composed facts: `Presence.State`'s `caret` axis (ProseMirror `anchor`/`head` positions beside their surface) is a landed `_AXES` row at `core/state/presence`, and this page consumes it exactly as `cursor`. `view/overlay#PRESENCE_COHORT` owns the ephemeral cursor COHORT rendering (`Overlay.cursors` — roster fold to point-anchored floats); this page supplies its `project` parameter from the assembled spaces and adds the glide law. Durable text anchors ride content's thread-mark roster row, so ProseMirror's own mapping carries them and this registry re-resolves rather than re-deriving positions.

## [01]-[INDEX]

- [02]-[ANCHOR_PLANE]: `Anchor` registers the coordinate spaces — posture vocabulary, durable re-entry, rail sweep, density clustering; `Anchor`.
- [03]-[ROSTER_FACES]: avatar stack and status tones over the core roster reads; `Face`.
- [04]-[CURSOR_PLANE]: `Face` supplies cursors into the overlay cohort under the glide and departure laws; `Face`.
- [05]-[THREAD_PLANE]: `Face.Thread` records comments — clamps, read marker, composer modes; `Face`.

## [02]-[ANCHOR_PLANE]

[ANCHOR_PLANE]:
- Owner: `Anchor` — the coordinate-space registry: `Anchor.space(row)` is the one lane constructor, closing a space's locator type inside a monomorphic `Anchor.Lane` (decode/encode through the row's own locator `Schema`, `locate` to an `Option` of viewport geometry, `track` folding the row's `carry` arm over its `epoch` stream), `Anchor.spaces(lanes)` the admission gate refusing duplicate surfaces with evidence, `Anchor.admit(lanes, anchored)` the durable re-entry that turns a persisted `{ space, locator }` back into a held locator or a named refusal, `Anchor.postures` the closed render vocabulary, `Anchor.swept` the margin-rail collision fold, and `Anchor.clustered` the density fold that collapses a crowded viewport into `+n` chips.
- Packages: `effect` (`Array`, `Data`, `Either`, `Option`, `Order`, `Record`, `Schema`, `Stream`); `@rasm/core` (`Fault.Class` behind the fault family); `system/act` (`Motion.useFollow` — the rendered transform rides motion values, never per-frame React state); `system/token` (the `z` ladder's `cursor` rank is the layer's one stacking coordinate).
- Entry: `Anchor.space` per surface, exported from the OWNING page as a value; the composition root hands the assembled record to this plane at boot — the same admission pattern `Overlay.commands` runs, so a malformed registry refuses at composition, never mid-render.
- Law: spaces arrive as VALUES, never imports — content, canvas, media, and the viewer each export their row from their own page and no `view/` sibling imports another; the registry is the meeting point and the composition root is the assembler, which is exactly how `Hook.Rows` and the port Tags already cross this folder.
- Law: the registry keys on SURFACE, never kind — `kind` names the codec FAMILY a row's locators speak while `surface` names the mounted instance (`Presence.Point.surface` is the join), so two images, two canvases, or two editors on one screen register two lanes sharing one family and a kind-keyed record collapses them onto whichever mounted last; durable anchors persist the surface name beside the locator, and a surface the app never re-registers parks its anchors.
- Law: a locator is opaque past its lane — `Anchor.Held` brands the decoded locator so a consumer can hold, persist (through `encode`), and re-resolve one without ever reading its interior; a durable anchor is `{ space, locator }` DATA whose locator re-enters through the lane's own codec, so a thread record persists anchors with no per-space arm anywhere downstream.
- Law: a durable anchor re-enters through `Anchor.admit` alone — the registry answers the lane by surface and that lane's own codec answers the locator, so a record naming an unregistered surface and a record whose payload the codec refuses arrive as two named refusals instead of one indistinguishable absence; the parked pin below is a resolved locator with no geometry and never travels this channel.
- Law: `resolve` answers `Option`, and `Option.none` is the PARKED posture — a collapsed branch, an unmounted surface, a position past the document's current extent all park their items rather than painting a stale pixel; parking is a render verdict, never a fault, and the item re-resolves on the space's next epoch emission.
- Law: `carry` is the mutation-mapping arm and its burden seats at the owner — content's space carries ephemeral carets through ProseMirror step maps because raw positions go stale on every transaction; canvas, media, and the viewer key by stable identity (node id, fraction, `GlobalId`) and declare no arm; a space whose locators go stale under mutation and declares no `carry` ships the drift this column exists to close.
- Law: the epoch is the space's OWN invalidation source — a ProseMirror transaction stream, the camera atom's change stream, a resize observation — and the overlay re-resolves exactly the lanes whose epoch fired; a global rAF re-resolve of every anchor spends the frame budget the per-space epoch exists to protect.
- Law: the posture vocabulary is closed at three — `pin` renders at the resolved point, `float` mounts the landed anchored host on an `Overlay.virtual` of the resolved rect, `rail` books the item into the margin sweep — and a fourth presentation is a recipe on one of the three, never a new arm.
- Law: the rail sweep is a pure two-pass fold — descending pass pushes each card below its predecessor's bottom edge, ascending pass pulls the tail back inside the viewport ceiling — both passes one `Array.mapAccum` over rows sorted by resolved top, so the sweep is deterministic data-in/data-out and every rail consumer — thread rails today, any margin board later — replays it identically.
- Law: density clusters structurally — past the viewport cap, pins collapse by quantized cell into one `+n` chip carrying its member keys (the avatar-stack overflow model applied to anchors), so ten thousand comments bound the overlay's node count by construction.
- Boundary: the one overlay LAYER mounts at app composition — `FloatingPortal` at the `z-cursor` rank with `pointer-events: none` on non-interactive items — and this plane owns its rows, folds, and postures, never the mount; camera math stays the viewer's, ProseMirror position math stays content's.
- Growth: a new surface class is one exported `Anchor.space` row; a new invalidation source is that row's `epoch`; a new presentation is a recipe over an existing posture — the registry, sweep, and cluster folds never fork.

```typescript
import { Fault } from "@rasm/core"
import { Array, Data, Either, Option, Order, Record, Schema, type Stream, type Types } from "effect"

declare const _held: unique symbol

declare namespace Anchor {
  type Held = { readonly [_held]: "AnchorLocator" }
  type Rect = { readonly x: number; readonly y: number; readonly width: number; readonly height: number }
  type Posture = (typeof _postures)[number]
  type Space<L, C> = {
    readonly kind: string
    readonly surface: string
    readonly locator: Schema.Schema<L>
    readonly resolve: (locator: L) => Option.Option<Anchor.Rect>
    readonly carry: Option.Option<(locator: L, change: C) => Option.Option<L>>
    readonly epoch: Stream.Stream<C>
  }
  type Lane = {
    readonly kind: string
    readonly surface: string
    readonly decode: (raw: unknown) => Option.Option<Anchor.Held>
    readonly encode: (held: Anchor.Held) => unknown
    readonly locate: (held: Anchor.Held) => Option.Option<Anchor.Rect>
    readonly track: Stream.Stream<(held: Anchor.Held) => Option.Option<Anchor.Held>>
  }
  type Item = {
    readonly key: string
    readonly space: string
    readonly locator: Anchor.Held
    readonly posture: Anchor.Posture
  }
  type Anchored = { readonly space: string; readonly locator: unknown }
  type Card = { readonly key: string; readonly top: number; readonly height: number }
  type Cluster =
    | { readonly _tag: "Lone"; readonly key: string; readonly rect: Anchor.Rect }
    | { readonly _tag: "Crowd"; readonly keys: Array.NonEmptyReadonlyArray<string>; readonly rect: Anchor.Rect }
  type Shape = Types.Simplify<{
    readonly postures: typeof _postures
    readonly space: typeof _space
    readonly spaces: typeof _spaces
    readonly admit: typeof _admit
    readonly swept: typeof _swept
    readonly clustered: typeof _clustered
  }>
}

const _postures = ["pin", "float", "rail"] as const

const _family = Fault.Class.family(["space-absent", "space-doubled", "locator-refused"] as const, {
  "space-absent": Fault.Class.row({
    class: "absent",
    leg: "anchor",
    detail: Schema.Struct({ space: Schema.String }),
    render: ({ space }) => `surface ${space} holds no registered lane`,
  }),
  "space-doubled": Fault.Class.row({
    class: "conflicted",
    leg: "registry",
    detail: Schema.Struct({ space: Schema.String, lanes: Schema.Int.pipe(Schema.greaterThan(1)) }),
    render: ({ space, lanes }) => `surface ${space} registers ${lanes} lanes`,
  }),
  "locator-refused": Fault.Class.row({
    class: "malformed",
    leg: "anchor",
    detail: Schema.Struct({ space: Schema.String, kind: Schema.String }),
    render: ({ space, kind }) => `${kind} codec on surface ${space} refused a persisted locator`,
  }),
})

class AnchorFault extends Schema.TaggedError<AnchorFault>()("AnchorFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

const _space = <L, C>(row: Anchor.Space<L, C>): Anchor.Lane => ({
  kind: row.kind,
  surface: row.surface,
  decode: (raw) => Option.map(Schema.decodeUnknownOption(row.locator)(raw), (held) => held as unknown as Anchor.Held),
  encode: (held) => Schema.encodeSync(row.locator)(held as unknown as L),
  locate: (held) => row.resolve(held as unknown as L),
  track: Option.match(row.carry, {
    onNone: () => Stream.empty,
    onSome: (carry) =>
      Stream.map(row.epoch, (change) => (held: Anchor.Held) =>
        Option.map(carry(held as unknown as L, change), (next) => next as unknown as Anchor.Held)),
  }),
})

const _spaces = (
  lanes: ReadonlyArray<Anchor.Lane>,
): Either.Either<Record.ReadonlyRecord<string, Anchor.Lane>, AnchorFault> =>
  Option.match(
    Array.findFirst(
      Record.toEntries(Array.groupBy(lanes, (lane) => lane.surface)),
      ([, held]) => held.length > 1,
    ),
    {
      onNone: () => Either.right(Record.fromEntries(Array.map(lanes, (lane) => [lane.surface, lane] as const))),
      onSome: ([surface, held]) =>
        Either.left(new AnchorFault({ case: { reason: "space-doubled", space: surface, lanes: held.length } })),
    },
  )

const _admit = (
  lanes: Record.ReadonlyRecord<string, Anchor.Lane>,
  anchored: Anchor.Anchored,
): Either.Either<Anchor.Held, AnchorFault> =>
  Option.match(Record.get(lanes, anchored.space), {
    onNone: () => Either.left(new AnchorFault({ case: { reason: "space-absent", space: anchored.space } })),
    onSome: (lane) =>
      Either.fromOption(
        lane.decode(anchored.locator),
        () => new AnchorFault({ case: { reason: "locator-refused", space: anchored.space, kind: lane.kind } }),
      ),
  })

const _byTop: Order.Order<Anchor.Card> = Order.mapInput(Order.number, (card: Anchor.Card) => card.top)

const _swept = (
  cards: ReadonlyArray<Anchor.Card>,
  gap: number,
  ceiling: number,
): ReadonlyArray<Anchor.Card> => {
  const [, pushed] = Array.mapAccum(Array.sortBy(cards, _byTop), Number.NEGATIVE_INFINITY, (floor, card) => {
    const top = Math.max(card.top, floor)
    return [top + card.height + gap, { ...card, top }] as const
  })
  const [, pulled] = Array.mapAccum(Array.reverse(pushed), ceiling, (roof, card) => {
    const top = Math.min(card.top, roof - card.height)
    return [top - gap, { ...card, top }] as const
  })
  return Array.reverse(pulled)
}

const _CELL = 48

const _clustered = (
  resolved: ReadonlyArray<{ readonly key: string; readonly rect: Anchor.Rect }>,
  cap: number,
): ReadonlyArray<Anchor.Cluster> =>
  resolved.length <= cap
    ? Array.map(resolved, ({ key, rect }) => ({ _tag: "Lone", key, rect } as const))
    : Array.map(
      Record.values(
        Array.groupBy(resolved, ({ rect }) => `${Math.round(rect.x / _CELL)}:${Math.round(rect.y / _CELL)}`),
      ),
      (members) =>
        members.length === 1
          ? ({ _tag: "Lone", key: Array.headNonEmpty(members).key, rect: Array.headNonEmpty(members).rect } as const)
          : ({
            _tag: "Crowd",
            keys: Array.map(members, (member) => member.key),
            rect: Array.headNonEmpty(members).rect,
          } as const),
    )

const Anchor: Anchor.Shape = {
  postures: _postures,
  space: _space,
  spaces: _spaces,
  admit: _admit,
  swept: _swept,
  clustered: _clustered,
}
```

## [03]-[ROSTER_FACES]

[ROSTER_FACES]:
- Owner: the roster faces riding `Face` — `Face.tone` maps the core status verdict onto the token vocabulary, `Face.stacked(roster, seen, self, max)` folds the roster into the avatar stack's shown/overflow split, and the avatar recipe itself is `view/media`'s (this page hands it the worn `face` profile — name, hue `Option`, content-keyed avatar — and never fetches a byte).
- Packages: `@rasm/core` (`Presence.roster`, `Presence.status`, `Presence.Lease`, `Clock.Hlc` — verdicts arrive computed; this page renders them); `system/token` (`Theme.Tone` keys); `effect` (`Array`, `HashMap`, `Option`, `Order`).
- Law: the horizon is SUPPLIED — a `Clock.Hlc` the composition root derives from its own clock source crosses as a parameter into every roster read, so liveness policy stays a `Presence.Lease` value and no ambient clock read exists on this page; the frame-cadence re-read is the app's subscription, never a timer here.
- Law: the tone table is total over the status vocabulary — `live` reads `success`, `idle` reads `neutral` (an idle collaborator is dimmed, never warned), `gone` reads `removed` (rendered only where history shows departures) — and the `satisfies` contract breaks the build when the core vocabulary grows a member this table has not decided.
- Law: the stack fold is structural — self leads, others sort by join stamp descending, the cap clamps at two or more, and past-cap actors collapse into one `+n` slot carrying its member keys for the hover list; a stack that hides overflow without the count misreports the room.
- Law: identity is the whole `Presence.Key` — tenant scope beside actor — everywhere a face keys a list or a map, because an actor id alone collides across tenants (`view/overlay#PRESENCE_COHORT`'s law, restated at neither end: both compose the same key).
- Boundary: avatar pixels, size derivation, and the image element are `view/media`'s; the presence fold table enters through `system/atom#LIVE_BRIDGE` as an atom like every host fold.

```typescript
import { Presence, type Clock, type Fold } from "@rasm/core"
import { Array, HashMap, Option, Order } from "effect"
import { Theme } from "../system/token.ts"

declare namespace Face {
  type Stack = {
    readonly shown: ReadonlyArray<{ readonly key: Presence.Key; readonly face: Option.Option<Presence.Profile>; readonly idle: boolean }>
    readonly overflow: ReadonlyArray<Presence.Key>
  }
}

const _tone = {
  live: { tone: "success" },
  idle: { tone: "neutral" },
  gone: { tone: "removed" },
} as const satisfies Record<Presence.Status, { readonly tone: Theme.Tone }>

const _byJoined: Order.Order<readonly [Presence.Key, Presence.State]> = Order.mapInput(
  Order.reverse(Order.bigint),
  ([, state]) => Option.match(state.joined, { onNone: () => 0n, onSome: (joined) => joined.physical }),
)

const _stacked = (
  seen: Fold.Table<Presence.Key, Presence.State>,
  horizon: Clock.Hlc,
  lease: Presence.Lease,
  self: Presence.Key,
  max: number,
): Face.Stack => {
  const cap = Math.max(2, max)
  const present = Array.filter(
    HashMap.toEntries(seen),
    ([, state]) => Presence.status(state, horizon, lease) !== "gone",
  )
  const ranked = Array.sortBy(
    present,
    Order.mapInput(Order.boolean, ([key]: readonly [Presence.Key, Presence.State]) => key !== self),
    _byJoined,
  )
  const shown = Array.map(Array.take(ranked, ranked.length > cap ? cap - 1 : cap), ([key, state]) => ({
    key,
    face: Option.map(state.face, (worn) => worn.value),
    idle: Presence.status(state, horizon, lease) === "idle",
  }))
  return {
    shown,
    overflow: ranked.length > cap ? Array.map(Array.drop(ranked, cap - 1), ([key]) => key) : [],
  }
}
```

## [04]-[CURSOR_PLANE]

[CURSOR_PLANE]:
- Owner: the cursor supply riding `Face` — `Face.project(lanes)` derives the viewport projection `Overlay.cursors` takes from the assembled anchor lanes, under the glide law: the projected pair feeds two `Motion.useFollow` values whose live reads back the cursor float's virtual rect, so remote cursors spring toward arrivals with zero per-frame React renders and the cohort's own `autoUpdate` tracks the sprung anchor.
- Packages: `view/overlay` (`Overlay.cursors`, `Overlay.point` — the cohort fold and the point anchor are landed; this page supplies and smooths); `system/act` (`Motion.useFollow` with the `glide` temperament); `@rasm/core` (`Presence.Point` — the tagged `Sheet`/`Scene` coordinate the projection folds).
- Law: the projection is derived, not authored — a `Sheet` point resolves through the lane registered for its `surface` and a `Scene` point through the viewer's `GlobalId`-space lane's camera projection, so this page holds no coordinate math and a surface class absent from the registry drops its cursors rather than guessing; the supply answers `Option` and `Face.sighted` pre-filters the fold table to actors whose point resolves, so the cohort composition at the root narrows over a proven table rather than forging a coordinate for an unresolvable point.
- Law: the local pointer broadcasts through `Presence.Move` ops on the owning surface's own cadence — the recognizer throttles at the transport, never the render — and the caret axis broadcasts the same way from content's selection changes; both are Move patches on the one op family, so replay and history read one vocabulary.
- Law: a broadcast point speaks its surface's OWN locator shape — the `Sheet` pair carries whatever coordinate the owning lane's codec decodes (media broadcasts fractions, canvas broadcasts graph coordinates), so the lane's locator schema is the cursor contract between broadcaster and renderer and a peer on either end reads one spelling.
- Law: departure is silence, never an op — pointer-leave and window blur STOP the broadcast (a departing Move re-stamps `last` and extends the liveness it means to end), the cohort dims the actor on the roster's `idle` verdict, and disappearance is the `Presence.Lease.idle` window; an app wanting fast cursor decay supplies a short lease, and a hand-rolled per-cursor timer beside the lease is the named defect.
- Boundary: the cohort fold, the point anchor, and the portal mount are `view/overlay#PRESENCE_COHORT`'s; the recognizer and its throttle are `system/act`'s; which surfaces broadcast is each owner's declaration.

```typescript
import { Presence } from "@rasm/core"
import { HashMap, Option, Record } from "effect"

const _project = (
  lanes: Record.ReadonlyRecord<string, Anchor.Lane>,
) =>
(point: Presence.Point): Option.Option<readonly [number, number]> =>
  Option.flatMap(Record.get(lanes, point.surface), (lane) =>
    Option.flatMap(lane.decode(point), (held) =>
      Option.map(lane.locate(held), (rect) => [rect.x, rect.y] as const)))

const _sighted = (
  seen: HashMap.HashMap<Presence.Key, Presence.State>,
  project: (point: Presence.Point) => Option.Option<readonly [number, number]>,
): HashMap.HashMap<Presence.Key, Presence.State> =>
  HashMap.filter(seen, (state) =>
    Option.match(state.cursor, {
      onNone: () => false,
      onSome: (worn) => Option.isSome(project(worn.value)),
    }))
```

## [05]-[THREAD_PLANE]

[THREAD_PLANE]:
- Owner: `Face.Thread` — the generic comment-thread record and its render folds: the `Thread` Schema owns identity, the durable `{ space, locator }` anchor, resolution, and the comment list; `Face.clamped` folds the show-more window, `Face.unread` pins the new-comment divider, `Face.watched` is the mark-as-read observation bracket, and `Face.Compose` is the closed three-mode composer vocabulary. `viewer/review` stays the BCF-specific instance of this concept — decoded wire topics with their own board — and neither page renders the other's records.
- Packages: `effect` (`Array`, `Data`, `Option`, `Schema`, `Stream`); `@rasm/core` (`Clock.Hlc` stamps); `system/primitive` (`Primitive.sanitize` — every rendered body passes the gate; the announce rail carries non-toast arrival notes); `system/intl` (`Message` — every static string is a catalog key).
- Law: the durable anchor is DATA — `{ space, locator }` persisted whole, re-entering through the named lane's own codec at render; a text-anchored thread's locator is the content thread-mark identity, so ProseMirror's own mapping keeps it placed and this record never stores a raw position.
- Law: authors are the app's principal spelling — a thread outlives every ephemeral presence actor, so the `author` field carries the app's principal string and the roster face join (worn profile for a live author) happens at render against the presence table; minting a presence-actor brand into this record couples a durable document to a session identity.
- Law: the list semantics are structural — the thread container reads `role="feed"`, each comment `role="article"` with `aria-posinset`/`aria-setsize` from the LOGICAL comment count (clamped rendering never changes the announced size), and keyboard traversal is the feed pattern's.
- Law: the clamp is a policy row — `{ max, show: "oldest" | "both" | "newest" }` selects which window survives and the collapsed remainder renders as one show-more affordance carrying its count; `both` keeps the head and tail so context and recency both survive.
- Law: the unread divider PINS — its index is the minimum of the first-unread position across arrivals, so new comments landing while the divider shows never push it down; a divider that tracks the live unread edge re-reads as "nothing new" mid-scroll.
- Law: mark-as-read is an observation, never a scroll math — one `IntersectionObserver` on the terminal marker element commits mark-as-read when it enters the viewport, acquired and released as one bracket so an unmounted thread observes nothing.
- Law: the composer is a closed three-mode family — `Start` (new thread, carries the anchor), `Reply` (existing thread), `Amend` (existing comment) — and mode is the value, never boolean props; the mention draft flips the editor's combobox aria (`role`, `aria-expanded`, `aria-activedescendant`) only while a draft is open, and the resolve affordance renders in the FIRST comment's action bar, never as thread chrome.
- Boundary: where threads persist, how they sync, and who may resolve are the app's; the rich-text composer body is content's editor at its smallest roster; toast-versus-inline arrival policy is the app's fold over `Primitive.notify`.
- Growth: a new anchor surface is one registry lane (threads follow for free); a new clamp policy is one `show` member; a new composer capability is one mode arm every consumer breaks on loudly.

```typescript
import { Clock } from "@rasm/core"
import { Array, Data, Effect, Option, Schema, Stream, type Types } from "effect"

const _Anchored = Schema.Struct({
  space: Schema.NonEmptyString,
  locator: Schema.Unknown,
})

class Comment extends Schema.Class<Comment>("Face.Comment")({
  key: Schema.NonEmptyString,
  author: Schema.NonEmptyString,
  at: Clock.Hlc,
  body: Schema.String,
  replyTo: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

class Thread extends Schema.Class<Thread>("Face.Thread")({
  key: Schema.NonEmptyString,
  anchor: _Anchored,
  resolved: Schema.Boolean,
  comments: Schema.NonEmptyArray(Comment),
}) {}

declare namespace Face {
  type Clamp = { readonly max: number; readonly show: "oldest" | "both" | "newest" }
  type Window = {
    readonly lead: ReadonlyArray<Comment>
    readonly hidden: number
    readonly tail: ReadonlyArray<Comment>
  }
  type Compose = Data.TaggedEnum<{
    Start: { readonly anchor: Schema.Schema.Type<typeof _Anchored> }
    Reply: { readonly thread: string }
    Amend: { readonly thread: string; readonly comment: string }
  }>
  type Shape = Types.Simplify<{
    readonly Thread: typeof Thread
    readonly Comment: typeof Comment
    readonly Compose: typeof _Compose
    readonly tone: typeof _tone
    readonly stacked: typeof _stacked
    readonly project: typeof _project
    readonly sighted: typeof _sighted
    readonly clamped: typeof _clamped
    readonly unread: typeof _unread
    readonly watched: typeof _watched
  }>
}

const _Compose = Data.taggedEnum<Face.Compose>()

const _WINDOWS: Record<
  Face.Clamp["show"],
  (comments: ReadonlyArray<Comment>, max: number, hidden: number) => Face.Window
> = {
  oldest: (comments, max, hidden) => ({ lead: Array.take(comments, max), hidden, tail: [] }),
  newest: (comments, max, hidden) => ({ lead: [], hidden, tail: Array.takeRight(comments, max) }),
  both: (comments, max, hidden) => ({
    lead: Array.take(comments, Math.ceil(max / 2)),
    hidden,
    tail: Array.takeRight(comments, max - Math.ceil(max / 2)),
  }),
}

const _clamped = (comments: Array.NonEmptyReadonlyArray<Comment>, clamp: Face.Clamp): Face.Window =>
  comments.length <= clamp.max
    ? { lead: comments, hidden: 0, tail: [] }
    : _WINDOWS[clamp.show](comments, clamp.max, comments.length - clamp.max)

const _unread = (
  comments: ReadonlyArray<Comment>,
  since: Option.Option<Clock.Hlc>,
  held: Option.Option<number>,
): Option.Option<number> =>
  Option.match(since, {
    onNone: () => Option.none(),
    onSome: (seen) =>
      Option.match(
        Array.findFirstIndex(comments, (comment) => comment.at.physical > seen.physical),
        {
          onNone: () => held,
          onSome: (fresh) =>
            Option.some(Option.match(held, { onNone: () => fresh, onSome: (prior) => Math.min(prior, fresh) })),
        },
      ),
  })

const _watched = (marker: HTMLElement): Stream.Stream<boolean> =>
  Stream.asyncScoped<boolean>((emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        const observer = new IntersectionObserver((entries) => {
          for (const entry of entries) void emit.single(entry.isIntersecting)
        })
        observer.observe(marker)
        return observer
      }),
      (observer) => Effect.sync(() => observer.disconnect()),
    ))

const Face: Face.Shape = {
  Thread,
  Comment,
  Compose: _Compose,
  tone: _tone,
  stacked: _stacked,
  project: _project,
  sighted: _sighted,
  clamped: _clamped,
  unread: _unread,
  watched: _watched,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Anchor, AnchorFault, Face }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
