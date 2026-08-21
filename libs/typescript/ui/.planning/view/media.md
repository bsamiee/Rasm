# [UI_MEDIA]

Media owns every byte-borne presentation class this folder renders as ONE owner: classes are rows, loading posture is policy data, and bytes arrive content-keyed through app-composed ports. `<img>`/`<video>`/`<audio>` render, CSS scroll-snap carries strip physics, and every capability past the baseline is a probed row refusing typed instead of assuming a host. `RULINGS.md` `[01]` rejects every media engine package, so depth comes from composition — `system/cache` holds the byte residency band and each sibling plane owns its own leg. Module: `ui/src/view/media.ts`.

Composition facts arrive settled: `Digest.Key<"content">` is the one byte identity and `assets/<digest>/<file>` the served join (`Glb.assetPath`'s law; this page mints no address); the cache `Budget` verdict vocabulary and the connection grade vocabulary are peer-owned and spelled field-for-field where a policy row keys them; `Presence.Profile` carries the avatar as a content key beside an `Option` hue; object URLs bracket acquire-to-revoke on one scope; strings resolve through the `system/intl` catalog; tones key `Theme.Tone` and no literal appearance lands here.

## [01]-[INDEX]

- [02]-[SOURCE_PLANE]: `Media.Source` admits the class rows, the serve port, and the loading-posture policy; `Media`, `MediaFault`.
- [03]-[IMAGE_PLANE]: attribute derivation, the reveal coordinator, and the fraction hotspot with its anchor-space export; `Media`.
- [04]-[AVATAR_PLANE]: `Media` seats the face recipe, the initials fold, and the stack overflow fold; `Media`.
- [05]-[TRANSPORT_PLANE]: `Media` bridges one media element — gauge rows, track rows, probed capability rows; `Media`.
- [06]-[GALLERY_PLANE]: `Media` drives the snap strip — windowing, announcements, prefetch, lightbox composition; `Media`.

## [02]-[SOURCE_PLANE]

[SOURCE_PLANE]:
- Owner: `Media` — one assembled owner: `_classes` is the closed media-class vocabulary and `_CLASSES` its row table (element kind, whether the class streams, whether a poster stands in), `Media.Source` the Schema owner every rendered byte class admits through (content key, class, mime, declared extent, the `Option` poster key a video carries), `Serve` the one byte port, and `Media.Policy` the loading-posture value the composition root supplies.
- Owner: `Serve` — the folder-declared byte capability Tag: `address(key, name)` answers the served URL the platform loaders fetch natively, and `pull(key)` answers verified octets for the byte lane (prefetch into the cache band, integrity-verified preview). Declared HERE and satisfied at the browser composition root from the runtime depot plane — this page opens no scheduler, holds no fetch, and mints no address; the two members are two lanes because an `<img src>` load is the platform's own pipeline while a cache warm needs octets in hand.
- Packages: `@rasm/ts/core` (`Digest`, `Fault.Class`, `Shape.Record`); `effect` (`Context`, `Schema`, `Effect`, `Option`).
- Law: each refusal renders the subject it actually holds — a byte refusal names the address that would not decode, a host refusal names the command it denied, and a withdrawn grant names the capability by its own closed key, so no refusal writes a stand-in word into a field meant for a source address.
- Law: the class roster is closed and each row carries its whole posture — a per-class code path beside the table is the named defect, and a new byte class (a document preview, a font specimen) is one row whose columns answer element kind, stream posture, and poster stand-in before it lands.
- Law: loading posture is POLICY DATA from the root, never a module constant — `Media.Policy` keys the connection-grade vocabulary field-for-field (the grades are the runtime connection plane's own words, transcribed exactly as the cache spells the budget verdicts) and each grade row answers `preload`, `autoplay`, and `prefetch` depth, so a frugal or strained session narrows every media surface through one value and no component branches on a profile it never sees.
- Law: the byte lane rides the cache — a `pull` that must survive reload lands through `Cache.resident("media", key, mint)` with `Serve.pull` as the mint, so verification, two-phase commit, and quota pressure are the cache's one path and this page re-implements none of them; the `media` band row on the cache `_BANDS` table (keyed, remintable) is that page's counterpart landing.
- Law: local intake previews bracket — a `File` picked for upload previews through `createObjectURL` acquired and `revokeObjectURL` released on one scope, the bracket `viewer/scene` already states; the upload itself is `view/form#UPLOAD_LANE`'s tus session and this page adds no second transfer mechanism.
- Boundary: which assets exist, their addresses, and their transformation ladder are the data plane's and arrive as manifest facts; the depot's haul scheduling, budgets, and connectivity governors are the runtime plane's; this page renders what the ports answer.
- Growth: a new media class is one `_CLASSES` row with its `Media.Source` admission; a new posture axis is one column on the policy rows — never a sibling source owner or a per-surface fetch.

```typescript signature
import { Digest, Fault, Shape } from "@rasm/ts/core"
import { Context, Effect, Option, Schema } from "effect"

const _classes = ["image", "video", "audio"] as const

// one row per class: the rendering element, whether playback streams over time, and whether a poster stands in
// before bytes arrive — every class decision below reads off this table, never a switch beside it
const _CLASSES = {
  image: { element: "img", timed: false, postered: false },
  video: { element: "video", timed: true, postered: true },
  audio: { element: "audio", timed: true, postered: false },
} as const

// grade keys transcribe the runtime connection plane's own vocabulary, field-for-field as the cache spells its
// budget verdicts; each row answers the whole posture so no component ever reads a profile
const _POLICY = Schema.Struct({
  preload: Schema.Literal("none", "metadata", "auto"),
  autoplay: Schema.Boolean,
  prefetch: Schema.Int.pipe(Schema.between(0, 8)),
})

const _Policy = Shape.Record(Schema.Literal("swift", "steady", "strained", "frugal"), _POLICY)

// the two extended arms that can refuse AT invocation; the media-session arm writes a host register synchronously
// and carries no refusal, so it is absent here rather than a member nothing ever raises
const _Grant = Schema.Literal("fullscreen", "pictureInPicture")

// two legs partition the refusal: `source` is byte truth — no lane answers the key, or the bytes will not decode —
// and `host` is the user agent's own verdict on a command that arrived whole
const _family = Fault.Class.family(
  ["source-absent", "decode-refused", "playback-denied", "capability-absent"] as const,
  {
    "source-absent": Fault.Class.row({
      class: "absent",
      leg: "source",
      detail: Schema.Struct({ key: Digest.Key.content }),
      render: ({ key }) => `no serve lane answers ${key}`,
    }),
    "decode-refused": Fault.Class.row({
      class: "malformed",
      leg: "source",
      detail: Schema.Struct({ source: Schema.String, cause: Schema.String }),
      render: ({ source, cause }) => `${source} would not decode: ${cause}`,
    }),
    "playback-denied": Fault.Class.row({
      class: "denied",
      leg: "host",
      detail: Schema.Struct({ source: Schema.String, cause: Schema.String }),
      render: ({ source, cause }) => `host refused playback of ${source}: ${cause}`,
    }),
    "capability-absent": Fault.Class.row({
      class: "unavailable",
      leg: "host",
      detail: Schema.Struct({ grant: _Grant, cause: Schema.String }),
      render: ({ grant, cause }) => `host withdrew ${grant} at invocation: ${cause}`,
    }),
  },
)

class MediaFault extends Schema.TaggedError<MediaFault>()("MediaFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

class Source extends Schema.Class<Source>("Source")({
  key: Digest.Key.content,
  media: Schema.Literal(..._classes),
  mime: Schema.NonEmptyString,
  extent: Schema.Int.pipe(Schema.nonNegative()),
  poster: Schema.optionalWith(Digest.Key.content, { as: "Option" }), // a stand-in frame only a postered class carries
}).pipe(Schema.filter((source) => _CLASSES[source.media].postered || Option.isNone(source.poster) || "<poster-on-unpostered-class>")) {}

// two lanes, one port: `address` feeds the platform loaders (the element fetches natively), `pull` answers octets
// for the cache mint and the verified preview; neither lane schedules — the satisfier wraps the runtime depot
class Serve extends Context.Tag("ui/media/Serve")<Serve, {
  readonly address: (key: Digest.Key<"content">, name: string) => string
  readonly pull: (key: Digest.Key<"content">) => Effect.Effect<Uint8Array<ArrayBuffer>, MediaFault>
}>() {}
```

## [03]-[IMAGE_PLANE]

[IMAGE_PLANE]:
- Owner: the image attribute derivation and the reveal coordinator riding `Media`: `_LOADING` maps each priority class onto the platform attribute triple (`loading`, `decoding`, `fetchpriority`), and `Media.coordinate` is the reveal-together fold — enroll a plane's images, settle each as it decodes, and reveal ONCE when the last settles — so a hero grid never pops in raggedly and a skeleton hands off to a whole plane.
- Law: the attribute triple is one row read — an `<img>` composes `_LOADING[priority]` whole, so eager-versus-lazy, sync-versus-async decode, and fetch priority travel together and a hand-set `loading` beside a priority class is the fork; `fetchpriority` needs no probe because an unknown attribute is inert by platform contract, which is exactly the degrade the baseline gate asks for.
- Law: the coordinator is a counting fold, never a timeout — `enroll` raises the expected count, `settle` raises the decoded count, and `revealed` derives as their equality above zero, published as a `SubscriptionRef` the atom bridge binds; a raced timer beside it fabricates a reveal no image proved. Nested coordinators pass their counts to the outermost, so one plane reveals as one unit however its tree composes.
- Law: the reveal LATCHES — once true it never derives back false, so a slot enrolling after the reveal (a lazy row scrolling in, a late avatar) joins the next paint without re-veiling content the user already saw; un-revealing a shown plane is the flicker this fold exists to close.
- Law: a settle is a decode PROOF — the enrollment binds the element's `decode()` promise through the fault rail (`decode-refused` on the platform's refusal) so a broken byte stream fails the coordinator visibly rather than holding the reveal forever; the failed slot reveals its fallback posture and the plane still settles.
- Law: the hotspot is a FRACTION pair — `Media.Spot` carries `x`/`y` in `[0,1]` of the intrinsic box, so an annotation survives every rendered size, crop, and container query; `Media.space(surface, target, resized)` exports the anchor-space row VALUE (`kind`, its own `locator` codec, `resolve` folding a spot over the element's live content box to a viewport rect, `carry` absent because fractions are intrinsic-stable, `epoch` the element's resize observation as a stream) that the composition root hands the presence anchor plane — the shape is structural and the root's assembly is the proof, because a lateral import between view siblings is the strata defect.
- Boundary: which images share a coordinator, and which priority class a slot takes, are the consuming surface's rows; the skeleton posture that hides an unrevealed plane is `view/status`'s.
- Growth: a new priority class is one `_LOADING` row; a new reveal condition (fonts, poster frames) is one enrollment kind on the same counting fold.

```typescript signature
import { type Stream, SubscriptionRef } from "effect"

const _priorities = ["critical", "visible", "deferred"] as const

// one row per priority class: the platform triple travels together, and `fetchpriority` degrades inert on a
// host predating it — no probe, no branch
const _LOADING = {
  critical: { loading: "eager", decoding: "sync", fetchpriority: "high" },
  visible: { loading: "eager", decoding: "async", fetchpriority: "auto" },
  deferred: { loading: "lazy", decoding: "async", fetchpriority: "low" },
} as const satisfies Record<
  (typeof _priorities)[number],
  { readonly loading: "eager" | "lazy"; readonly decoding: "sync" | "async"; readonly fetchpriority: "high" | "auto" | "low" }
>

declare namespace Media {
  type Priority = (typeof _priorities)[number]
  type Coordinator = {
    readonly enroll: Effect.Effect<void>
    readonly settle: Effect.Effect<void>
    readonly revealed: SubscriptionRef.SubscriptionRef<boolean> // derives as enrolled === settled above zero; the bridge binds it
  }
  type Spot = Schema.Schema.Type<typeof _Spot>
  // this row spells the structural anchor-space contract field-for-field against view/presence#ANCHOR_PLANE's
  // Anchor.Space<L, C>: the presence plane admits this row at the composition root, so the coupling is a value
  // handed at assembly and neither view sibling imports the other
  type Space<L, C> = {
    readonly kind: string
    readonly surface: string // the mounted instance the registry keys on: two images register two lanes of one family
    readonly locator: Schema.Schema<L>
    readonly resolve: (locator: L) => Option.Option<{ readonly x: number; readonly y: number; readonly width: number; readonly height: number }>
    readonly carry: Option.Option<(locator: L, change: C) => Option.Option<L>>
    readonly epoch: Stream.Stream<C> // one invalidation emission per content-box change: the overlay re-resolves on it
  }
}

const _Spot = Schema.Struct({
  x: Schema.Number.pipe(Schema.between(0, 1)),
  y: Schema.Number.pipe(Schema.between(0, 1)),
})

// revealed derives as enrolled === settled above zero, so reveal is a proof over the tally and
// no timer can fabricate it; a nested coordinator enrolls ONE slot on its parent and settles it when it reveals,
// so a composed plane reveals as one unit however its tree nests
const _coordinate: Effect.Effect<Media.Coordinator> = Effect.gen(function* () {
  const tally = yield* SubscriptionRef.make({ enrolled: 0, settled: 0 })
  const revealed = yield* SubscriptionRef.make(false)
  // this latch is MONOTONIC: a slot enrolling after the plane revealed joins the next paint quietly — flipping a
  // shown plane back behind the veil re-hides content the user already saw
  const derive = SubscriptionRef.get(tally).pipe(
    Effect.flatMap((held) =>
      SubscriptionRef.update(revealed, (shown) => shown || (held.enrolled > 0 && held.settled >= held.enrolled))
    ),
  )
  return {
    enroll: Effect.zipRight(SubscriptionRef.update(tally, (held) => ({ ...held, enrolled: held.enrolled + 1 })), derive),
    settle: Effect.zipRight(SubscriptionRef.update(tally, (held) => ({ ...held, settled: held.settled + 1 })), derive),
    revealed,
  }
})

// enroll binds the element's own decode proof onto the rail: a refused decode fails visibly and still settles the
// plane, so the coordinator can never hang on a broken byte stream
const _enrolled = (held: Media.Coordinator, image: HTMLImageElement): Effect.Effect<void, MediaFault> =>
  Effect.zipRight(
    held.enroll,
    Effect.ensuring(
      Effect.tryPromise({
        try: () => image.decode(),
        catch: (defect) => new MediaFault({ case: { reason: "decode-refused", source: image.currentSrc, cause: String(defect) } }),
      }),
      held.settle,
    ),
  )

const _space = (surface: string, target: HTMLElement, resized: Stream.Stream<void>): Media.Space<Media.Spot, void> => ({
  kind: "media",
  surface,
  locator: _Spot,
  resolve: (spot) => {
    const box = target.getBoundingClientRect() // BOUNDARY ADAPTER: the live content box is the platform's own read
    return box.width === 0 || box.height === 0
      ? Option.none()
      : Option.some({ x: box.left + spot.x * box.width, y: box.top + spot.y * box.height, width: 0, height: 0 })
  },
  carry: Option.none(), // fractions are intrinsic-stable: no mutation maps them, so the arm states its absence
  epoch: resized, // the element's resize observation as a stream: the caller brackets the observer per the landed pattern
})
```

## [04]-[AVATAR_PLANE]

[AVATAR_PLANE]:
- Owner: the face and stack rows riding `Media`: `_face` is the one avatar recipe (size axis over the token scale, the identity ring), `Media.initials` folds a display name into its grapheme initials, and `Media.stack` is the pure overflow fold — clamp, dedupe, order, and split into the shown faces beside the `+n` remainder — every avatar surface in the estate reads.
- Packages: `class-variance-authority` (the recipe rides `Primitive.styled`'s spine); `effect` (`Array`, `HashSet`, `Option`, `Order`).
- Law: the face resolves in one ladder — image (the profile's content key through `Serve.address`), then initials, then the neutral glyph — and every rung renders under the same recipe so a fallback is a content swap, never a sibling component; the ladder position derives from what the profile row carries, and a broken image demotes one rung through the same fault rail the image plane rides.
- Law: the identity ring is EVIDENCE, not appearance — the profile's `Option` hue is the actor's own identity fact, rendered through one fixed lightness-chroma envelope pair so every ring sits in the same perceptual band whatever hue arrives; the envelope values are this page's two policy literals and the hue is never authored here, so restyling rings is one row edit and no actor's color forks per surface.
- Law: initials are grapheme-safe — the fold segments through the locale segmenter and takes each name part's first grapheme (at most two), so a combining mark, a surrogate pair, or a non-Latin name never splits mid-glyph; a `charAt(0)` fold is the named defect.
- Law: the stack fold is pure data — `max` clamps to at least two (one face beside `+n` reads as nothing), identity dedupes on the actor key, order is a rank VALUE the caller supplies (the registry-keys-stay-domain-blind ruling: self-first is the presence surface's rank, alphabetical is a member list's), and the remainder count is exact; the hidden roster travels with the fold's answer so the `+n` affordance lists names without a second computation.
- Boundary: the hover list behind `+n` is an anchored overlay host row (`view/overlay`); who is IN the roster — presence reads, membership tables — is the consuming surface's; this page renders faces it is handed.
- Growth: a new face rung is one ladder row; a new stack posture (a grid burst, a typing indicator slot) is one column on the fold's answer — never a second avatar component.

```typescript signature
import { cva } from "class-variance-authority"
import { Array, HashSet, Option, Order } from "effect"

// in this one avatar recipe size rides the token scale, and the identity law fills the ring's CSS custom property
const _face = cva("relative inline-flex shrink-0 items-center justify-center overflow-hidden rounded-full bg-neutral-surface text-neutral-text", {
  variants: {
    size: { xs: "size-5 text-xs", sm: "size-6 text-xs", md: "size-8 text-sm", lg: "size-10 text-base" },
    ringed: { true: "ring-2 ring-[var(--face-ring)]", false: "" },
  },
  defaultVariants: { size: "md", ringed: false },
})

// one lightness-chroma pair renders every identity hue in the same perceptual band, so the hue stays the actor's
// evidence and the band stays this page's one appearance decision
const _RING = { lightness: 0.72, chroma: 0.14 } as const

const _ring = (hue: Option.Option<number>): Option.Option<string> =>
  Option.map(hue, (held) => `oklch(${_RING.lightness} ${_RING.chroma} ${held})`)

// grapheme-safe initials: the locale segmenter splits name parts, and each part yields its first grapheme whole
const _initials = (name: string, locale: string): string => {
  // BOUNDARY ADAPTER: Intl.Segmenter is the platform's grapheme authority; the iterator is consumed at this seam
  const segmenter = new Intl.Segmenter(locale, { granularity: "grapheme" })
  return Array.join(
    Array.map(
      Array.take(name.split(/\s+/u).filter((part) => part.length > 0), 2),
      (part) => segmenter.segment(part)[Symbol.iterator]().next().value?.segment ?? "",
    ),
    "",
  )
}

declare namespace Media {
  type Stacked<A> = { readonly shown: ReadonlyArray<A>; readonly hidden: ReadonlyArray<A> } // hidden travels whole: the +n list renders without a second fold
}

const _stack = <A, K>(
  faces: ReadonlyArray<A>,
  key: (face: A) => K,
  rank: Order.Order<A>, // the caller's own rank value: self-first, alphabetical, arrival — domain-blind here
  max: number,
): Media.Stacked<A> => {
  const bound = Math.max(2, max) // one face beside +n reads as nothing: the clamp is structural
  const unique = Array.reduce(faces, { seen: HashSet.empty<K>(), held: Array.empty<A>() }, (taken, face) =>
    HashSet.has(taken.seen, key(face))
      ? taken
      : { seen: HashSet.add(taken.seen, key(face)), held: Array.append(taken.held, face) })
  const ordered = Array.sort(unique.held, rank)
  return ordered.length <= bound
    ? { shown: ordered, hidden: [] }
    : { shown: Array.take(ordered, bound - 1), hidden: Array.drop(ordered, bound - 1) }
}
```

## [05]-[TRANSPORT_PLANE]

[TRANSPORT_PLANE]:
- Owner: `Media.transport` — the ONE media-element bridge: a scoped acquisition binding the element's event families into a single `SubscriptionRef` transport state (`position`, `duration`, `playing`, `waiting`) the atom bridge binds, beside the command members (`play`, `pause`, `seek`, `rate`) on the typed fault rail; every timed surface reads this bridge and none binds a listener of its own.
- Law: the bridge is one registration per element on one scope — each event family (`timeupdate`, `durationchange`, `play`/`pause`, `waiting`/`playing`, `ended`, `error`) folds into the same state value inside the acquisition, and release removes every listener with the scope; a component-held `addEventListener` beside the bridge is a second reader of a surface this page already owns.
- Law: `play` is fallible by platform contract — the promise refuses on autoplay policy and on an unloadable source, so the command lifts through the rail as `playback-denied` and a caller folds it into an affordance (a tap-to-play poster) instead of a silent dead button; the policy row's `autoplay` decides whether a surface ever attempts an unprompted play.
- Law: gauges are the landed rows — the scrub and volume surfaces are RAC `Slider` rows bound controlled over the transport atom (seek commits on the slider's own commit boundary, not per-frame), and the buffering posture is a `view/status` progress row reading `waiting`; a bespoke scrub track or a per-frame seek write is the named defect.
- Law: text tracks are addressed rows — a `_TRACKS` row names the platform track kind and the track's own content key, the element composes `<track src>` through `Serve.address`, and the kinds are the platform's closed vocabulary spelled whole; a track that fails to load surfaces `decode-refused` with the track name as its source.
- Law: extended capability is a probed Option, never an assumption — `Media.extended` probes fullscreen, picture-in-picture, and the media session ONCE per call against the live host and answers each as an `Option` member whose absence a consumer renders as a missing affordance; invoking an absent member is unrepresentable, and a present member still refuses `capability-absent` when the host revokes at invocation. That metadata row carries title, artist, and artwork keys resolved through `Serve.address`, so the lock-screen face and the in-app face read one identity.
- Boundary: WHAT plays, playlists, and position persistence are app policy; the poster-versus-autoplay decision reads the policy row; this page owns the bridge, the gauges' binding law, and the probes.
- Growth: a new transport fact (playback rate, seekable ranges) is one field on the state fold; a new extended capability (remote playback, casting) is one probed Option member — never a per-surface capability read.

```typescript signature
import { Scope, Stream, SubscriptionRef } from "effect"

declare namespace Media {
  type Transport = {
    readonly position: number // seconds; the element's own clock, folded on timeupdate
    readonly duration: Option.Option<number> // absent until metadata lands: a live stream may never answer
    readonly playing: boolean
    readonly waiting: boolean
  }
  type Deck = {
    readonly state: SubscriptionRef.SubscriptionRef<Media.Transport>
    readonly play: Effect.Effect<void, MediaFault>
    readonly pause: Effect.Effect<void>
    readonly seek: (position: number) => Effect.Effect<void>
    readonly rate: (factor: number) => Effect.Effect<void>
  }
  type Session = {
    readonly title: string
    readonly artist: Option.Option<string>
    readonly artwork: ReadonlyArray<{ readonly key: Digest.Key<"content">; readonly sizes: string; readonly mime: string }>
  }
  type Extended = {
    readonly fullscreen: Option.Option<(target: HTMLElement) => Effect.Effect<void, MediaFault>>
    readonly pictureInPicture: Option.Option<(target: HTMLVideoElement) => Effect.Effect<void, MediaFault>>
    readonly session: Option.Option<(row: Media.Session, address: (key: Digest.Key<"content">) => string) => Effect.Effect<void>>
  }
}

// this closed track-kind vocabulary is the platform's own, spelled whole; a track row names kind, label key, bytes
const _trackKinds = ["subtitles", "captions", "descriptions", "chapters", "metadata"] as const

const _Track = Schema.Struct({
  kind: Schema.Literal(..._trackKinds),
  label: Schema.NonEmptyString, // an intl catalog key: track menus localize like every other string
  language: Schema.NonEmptyString,
  key: Digest.Key.content,
})

// one registration, one scope, one state value: every event family folds into the same SubscriptionRef, and
// release removes the whole binding — no surface ever holds a second listener on the element
const _transport = (element: HTMLMediaElement): Effect.Effect<Media.Deck, never, Scope.Scope> =>
  Effect.gen(function* () {
    const state = yield* SubscriptionRef.make<Media.Transport>({ position: 0, duration: Option.none(), playing: false, waiting: false })
    const fold = (step: (held: Media.Transport) => Media.Transport) => SubscriptionRef.update(state, step)
    yield* Stream.runForEach(
      Stream.fromEventListener(element, "timeupdate"),
      () => fold((held) => ({ ...held, position: element.currentTime })),
    ).pipe(Effect.forkScoped)
    yield* Stream.runForEach(
      Stream.fromEventListener(element, "durationchange"),
      () => fold((held) => ({ ...held, duration: Number.isFinite(element.duration) ? Option.some(element.duration) : Option.none() })),
    ).pipe(Effect.forkScoped)
    yield* Effect.forEach(
      [["play", { playing: true }], ["pause", { playing: false }], ["ended", { playing: false }], ["waiting", { waiting: true }], ["playing", { waiting: false }]] as const,
      ([kind, patch]) =>
        Stream.runForEach(Stream.fromEventListener(element, kind), () => fold((held) => ({ ...held, ...patch }))).pipe(Effect.forkScoped),
      { discard: true },
    )
    return {
      state,
      // hosts refuse play on autoplay policy and unloadable sources: the rail carries that refusal as a typed
      // affordance, never a silently dead control
      play: Effect.tryPromise({
        try: () => element.play(),
        catch: (defect) => new MediaFault({ case: { reason: "playback-denied", source: element.currentSrc, cause: String(defect) } }),
      }),
      pause: Effect.sync(() => element.pause()),
      seek: (position) => Effect.sync(() => void (element.currentTime = position)),
      rate: (factor) => Effect.sync(() => void (element.playbackRate = factor)),
    }
  })

// probed once per call against the live host: absence is an Option a consumer renders as a missing affordance,
// and a present arm still refuses typed when the host revokes at invocation
const _extended: Effect.Effect<Media.Extended> = Effect.sync(() => ({
  fullscreen: typeof globalThis.document.documentElement.requestFullscreen === "function"
    ? Option.some((target: HTMLElement) =>
      Effect.tryPromise({
        try: () => target.requestFullscreen(),
        catch: (defect) => new MediaFault({ case: { reason: "capability-absent", grant: "fullscreen", cause: String(defect) } }),
      }))
    : Option.none(),
  pictureInPicture: globalThis.document.pictureInPictureEnabled === true
    ? Option.some((target: HTMLVideoElement) =>
      Effect.asVoid(Effect.tryPromise({
        try: () => target.requestPictureInPicture(),
        catch: (defect) => new MediaFault({ case: { reason: "capability-absent", grant: "pictureInPicture", cause: String(defect) } }),
      })))
    : Option.none(),
  session: "mediaSession" in globalThis.navigator
    ? Option.some((row: Media.Session, address: (key: Digest.Key<"content">) => string) =>
      Effect.sync(() => {
        // BOUNDARY ADAPTER: the media session is an imperative host register; artwork resolves through the serve join
        globalThis.navigator.mediaSession.metadata = new MediaMetadata({
          title: row.title,
          artist: Option.getOrElse(row.artist, () => ""),
          artwork: row.artwork.map((art) => ({ src: address(art.key), sizes: art.sizes, type: art.mime })),
        })
      }))
    : Option.none(),
}))
```

## [06]-[GALLERY_PLANE]

[GALLERY_PLANE]:
- Owner: `Media.useStrip` — the one gallery engine: CSS scroll-snap carries the physics (the browser IS the momentum model), `@tanstack/react-virtual` windows the items horizontally, `scrollToIndex` is the one programmatic move the prev/next buttons and the deep-link reveal share, and the position announcement rides the landed announce rail through an intl catalog key; the strip is one hook whose options are rows, and a second carousel mechanism beside it is the rejection the embla ruling already records.
- Law: snap is a recipe, never a scroll handler — the strip container composes `snap-x snap-mandatory` and each cell `snap-center` through `cn`, so settling, overscroll, and touch physics are the platform's; the virtualizer's `scrollToFn` stays default because a snap container's smooth scroll IS the animation, and a JS momentum model beside snap is the double-physics defect.
- Law: the window and the count are logical — `count` is the full roster, `getVirtualItems` mounts the visible span, and the announced position (`i` of `n`) reads the logical index; the announcement is polite and fires on settle, not per frame, so a fast swipe speaks once.
- Law: prefetch is policy-deep, cache-landed — on settle the strip warms `prefetch`-many neighbor keys (the policy row's depth for the session's grade) through `Cache.resident("media", key, Serve.pull)`, so a strained session warms nothing and a swift one warms ahead; a prefetch that bypasses the cache re-derives residency this folder already owns.
- Law: the lightbox is a composition, not a component — the overlay MODAL host row presents it, `Motion.overlay` animates it, and its pan-zoom is one `Gesture.useCanvas` call whose `emit` folds the recognizer's reading over the lightbox camera (center and zoom; bearing unused and held zero), writing through one atom exactly as the geo camera law composes; a pinch handler or wheel listener beside `useCanvas` is the double-bind defect `system/act` names.
- Law: lightbox egress rides the landed matrix — save and share of the presented image hand octets to `view/export`'s parcel lane, and the strip mints nothing; a `toBlob` or anchor-download inside this plane is the ungated-native-call defect that page already forecloses.
- Boundary: masonry and grid walls are the same virtualizer under `lanes` and belong to the consuming surface's rows; the strip owns the snap axis, the window, the announcement, and the prefetch fold.
- Growth: a new strip posture (peek, full-bleed, thumbnail rail) is a recipe variant row; a new reveal trigger is one `scrollToIndex` call site — never a second engine or a JS physics fork.

```typescript signature
import { useVirtualizer } from "@tanstack/react-virtual"
import { useEffect } from "react"
import type { RefObject } from "react"

declare namespace Media {
  type Strip = {
    readonly target: RefObject<HTMLElement | null>
    readonly count: number
    readonly extent: (index: number) => number // per-cell size estimate the window corrects by measurement
    readonly settled: (index: number) => void // the announcement + prefetch seam: fires on snap settle, never per frame
  }
  type Camera = { readonly center: readonly [number, number]; readonly zoom: number } // the lightbox's two axes; bearing stays zero
  type Shape = {
    readonly Fault: typeof MediaFault
    readonly Source: typeof Source
    readonly Spot: typeof _Spot
    readonly Track: typeof _Track
    readonly Policy: typeof _Policy
    readonly classes: typeof _classes
    readonly rows: typeof _CLASSES
    readonly loading: typeof _LOADING
    readonly face: typeof _face
    readonly ring: typeof _ring
    readonly initials: typeof _initials
    readonly stack: typeof _stack
    readonly space: typeof _space
    readonly coordinate: typeof _coordinate
    readonly enrolled: typeof _enrolled
    readonly transport: typeof _transport
    readonly extended: typeof _extended
    readonly useStrip: typeof _useStrip
  }
}

// snap physics live in CSS, windowing here, reveal through the one imperative move; the virtualizer's default
// scrollToFn stands because the snap container's own smooth scroll is the animation
const _useStrip = (options: Media.Strip) => {
  const window = useVirtualizer({
    count: options.count,
    getScrollElement: () => options.target.current,
    estimateSize: options.extent,
    horizontal: true,
    overscan: 2,
  })
  // this settle seam WIRES here — scrollend where the host ships it, a scroll-quiesce window where it does not
  // (the event sits past the baseline gate), so announcement and prefetch fire once per settle, never per frame;
  // centered logical index derives from the window's own measurements
  useEffect(() => {
    const host = options.target.current
    if (host === null) return undefined
    const onSettle = () => {
      const centre = host.scrollLeft + host.clientWidth / 2
      const hit = window.getVirtualItems().find((item) => item.start <= centre && centre < item.end)
      if (hit !== undefined) options.settled(hit.index)
    }
    if ("onscrollend" in host) {
      host.addEventListener("scrollend", onSettle)
      return () => host.removeEventListener("scrollend", onSettle)
    }
    let quiesce = 0
    const onScroll = () => {
      globalThis.clearTimeout(quiesce)
      quiesce = globalThis.setTimeout(onSettle, 150)
    }
    host.addEventListener("scroll", onScroll, { passive: true })
    return () => {
      globalThis.clearTimeout(quiesce)
      host.removeEventListener("scroll", onScroll)
    }
  }, [options.count])
  return {
    items: window.getVirtualItems(),
    extent: window.getTotalSize(),
    reveal: (index: number) => window.scrollToIndex(index, { align: "center", behavior: "smooth" }),
    measure: window.measureElement,
  }
}

const Media: Media.Shape = {
  Fault: MediaFault,
  Source,
  Spot: _Spot,
  Track: _Track,
  Policy: _Policy,
  classes: _classes,
  rows: _CLASSES,
  loading: _LOADING,
  face: _face,
  ring: _ring,
  initials: _initials,
  stack: _stack,
  space: _space,
  coordinate: _coordinate,
  enrolled: _enrolled,
  transport: _transport,
  extended: _extended,
  useStrip: _useStrip,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Media, MediaFault, Serve }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
