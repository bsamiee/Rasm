# [RUNTIME_ROUTE]

The Navigation-API typed router under the zero-routing-package law, the browser session-residency plane, and the navigation admission fold — one page because the three concerns meet at one commit path: the route table is app DATA (one row per route carrying its segment template, its `nuqs` query codec, and its guard policy value), the lib owns the algebra over it — segment-param types derived from the template literal itself, the query plane decoded and serialized through the `nuqs/server` codec core, one interceptable ingress over `window.navigation` owning link clicks, form submits, back/forward, and programmatic traversal alike, and one commit path so the location cell and the URL cannot tear. `nuqs` is the codec, the Navigation API is the traversal, `Schema`-grade typing rides the derivation — no router package, no `popstate`/`pushState` split, no hand-mashed `URLSearchParams`. The session plane holds the residency law: the access session travels as an `HttpOnly` cookie the page never reads (`security/authn/session` frames it, the edge writes it — the browser holds zero token bytes), the CSRF token is the one deliberately readable cookie echoed into a header per the double-submit law, session STATUS is memory-only state in one cell reconstructed on every boot, and the OAuth redirect continuity a full-page IdP navigation destroys rides `persist#DOMAIN_ROWS`'s `flow` domain — persisted before departure, consumed single-use at landing. The guard produces the router's own `Admission` verdict directly, handed into the admission slot at composition so the router never imports the guard. The module is `runtime/src/browser/route.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                | [PUBLIC]                             |
| :-----: | :---------------- | :------------------------------------------------------------------------ | :----------------------------------- |
|  [01]   | `TABLE_ALGEBRA`   | the row shape, segment-type derivation, match/href, the query codec        | `Router`                              |
|  [02]   | `TRAVERSAL_OWNER` | the minted Tag, the intercept ingress, cells, restore, commit law          | `Router`, `RouteFault`                |
|  [03]   | `SESSION_PLANE`   | the status family, the residency cell, refresh, cross-tab, CSRF, continuity | `SessionStatus`, `Vault`, `FlowFault` |
|  [04]   | `ADMISSION_FOLD`  | the policy value, the resolve chain, the dirty registry, the confirm       | `Guard`, `Confirm`                    |

## [2]-[TABLE_ALGEBRA]

[TABLE_ALGEBRA]:
- Owner: the `Router` type plane and pure algebra — `Router.Row<P>` (`path` template, `query` `ParserMap`, `policy` the guard-interpreted value, `title` `Option`), `Router.Params<Path>` deriving the segment record from the template literal (`"/doc/:key/:rev"` yields `{ key: string; rev: string }` — the correspondence computes, never restates), `Router.Location<Rows>` (the key-discriminated union of `{ key, segments, query }`), `_matched` (template against pathname, one fold, `Option` of captures), and the per-row `createLoader`/`createSerializer` pair — decode and write of the query plane through one `ParserMap`, so loader and serializer cannot drift.
- Law: the query codec is `nuqs/server` and only that — the `parseAs*` atoms, `createParser` for bespoke encodings, and `Router.param(shape)` for structured values: the fusion of `parseAsJson` over `Schema.standardSchemaV1`, so a core-Schema-refined value rides one URL param, decodes once into branded interior shape, and the kernel-Schema-into-URL seam has one spelling; the React hook/adapter surface is the ui wave's tier and never imported here.
- Law: `Router.standard(map)` projects a row's whole `ParserMap` through `createStandardSchemaV1` as the one view handed to foreign validation consumers — a form layer, a TanStack-shaped adapter — an egress projection, never an interior re-validation, because the loader already decoded once at the intercept.
- Law: a route is one row — path, codec, policy, title travel together, so `[5]`'s fold reads its policy from the matched row and no parallel policy registry exists; a new route is one row plus its compile-time ripple through every exhaustive location consumer.
- Law: `href` is the one URL mint — segments substitute into the template, the serializer writes the query with `clearOnDefault` semantics, and a hand-concatenated URL anywhere in an app is the string-mash this owner deletes.
- Growth: a new query key is one `ParserMap` field on its row; a new route axis (a transition hint, a preload row) is one `Row` field every table inherits.
- Boundary: what a policy MEANS is `[5]`'s fold; this cluster carries it as an opaque row value.
- Packages: `nuqs/server` (`createLoader`, `createSerializer`, `createStandardSchemaV1`, `parseAsJson`, type `ParserMap`, type `inferParserType`); `effect` (`Array`, `Option`, `Record`, `Schema`).

```typescript
import { Array, Context, Data, Effect, Layer, Option, Record, Runtime, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import { createLoader, createSerializer, createStandardSchemaV1, type inferParserType, parseAsJson, type ParserMap } from "nuqs/server"
import { Kv, type KvFault } from "./persist.ts"

type _Names<Path extends string> = Path extends `${string}:${infer Rest}`
  ? Rest extends `${infer Name}/${infer Tail}` ? Name | _Names<`/${Tail}`> : Rest
  : never

declare namespace Router {
  type Params<Path extends string> = { readonly [K in _Names<Path>]: string }
  type Row<P> = {
    readonly path: string
    readonly query: ParserMap
    readonly policy: P
    readonly title: Option.Option<string>
  }
  type Rows<P = unknown> = Record<string, Row<P>>
  type Location<Rows extends Router.Rows> = {
    readonly [K in keyof Rows & string]: {
      readonly key: K
      readonly segments: Params<Rows[K]["path"]>
      readonly query: inferParserType<Rows[K]["query"]>
    }
  }[keyof Rows & string]
  type Admission = Data.TaggedEnum<{
    Proceed: {}
    Divert: { readonly to: string }
    Refuse: {}
  }>
  type Shape<Rows extends Router.Rows> = {
    readonly location: Subscribable.Subscribable<Location<Rows>>
    readonly pending: Subscribable.Subscribable<Option.Option<Location<Rows>>>
    readonly href: <K extends keyof Rows & string>(
      key: K,
      segments: Params<Rows[K]["path"]>,
      query: Partial<inferParserType<Rows[K]["query"]>>,
    ) => string
    readonly go: (href: string, history?: "push" | "replace") => Effect.Effect<void>
    readonly back: Effect.Effect<void>
    readonly forward: Effect.Effect<void>
  }
  type Spec<Rows extends Router.Rows> = {
    readonly identifier: string
    readonly rows: Rows
    readonly fallback: keyof Rows & string
  }
}

const Admission: Data.TaggedEnum.Constructor<Router.Admission> = Data.taggedEnum<Router.Admission>()

const _param = <A, I>(shape: Schema.Schema<A, I>) => parseAsJson(Schema.standardSchemaV1(shape))

const _matched = (template: string, pathname: string): Option.Option<Record<string, string>> => {
  const want = template.split("/")
  const live = pathname.split("/")
  return want.length === live.length
    ? Array.reduce(Array.zip(want, live), Option.some<Record<string, string>>({}), (acc, [shape, got]) =>
        Option.flatMap(acc, (held) =>
          shape.startsWith(":")
            ? Option.some({ ...held, [shape.slice(1)]: got })
            : shape === got
              ? Option.some(held)
              : Option.none(),
        ),
      )
    : Option.none()
}

const _located = <Rows extends Router.Rows>(rows: Rows, url: URL): Option.Option<Router.Location<Rows>> =>
  Array.head(
    Array.filterMap(Record.toEntries(rows), ([key, row]) =>
      Option.map(_matched(row.path, url.pathname), (segments) =>
        ({ key, segments, query: createLoader(row.query)(url) }) as Router.Location<Rows>,
      ),
    ),
  )

const _href = <Rows extends Router.Rows, K extends keyof Rows & string>(
  rows: Rows,
  key: K,
  segments: Record<string, string>,
  query: Partial<inferParserType<Rows[K]["query"]>>,
): string =>
  createSerializer(rows[key].query)(
    rows[key].path.split("/").map((part) => (part.startsWith(":") ? segments[part.slice(1)] ?? "" : part)).join("/"),
    query,
  )
```

## [3]-[TRAVERSAL_OWNER]

[TRAVERSAL_OWNER]:
- Owner: `Router.make(spec)` — mints the app's `{ Tag, layer }`: the Tag typed at `Router.Shape<Rows>` under the app's identifier, and `layer(admission)` building the scoped traversal owner: the location cell seeded from `navigation.currentEntry`, the pending cell carrying the in-flight target, the one `navigate`-event ingress, cold-boot query restoration, and the commit law.
- Law: one commit path — the intercept is the only writer of the location cell: programmatic `go`, link clicks, form submits, and history gestures all raise `navigate` events, the listener parses the destination against the table (an unmatched URL folds to the `fallback` row, never a throw), runs the admission slot, and commits or diverts inside the intercepted handler; a commit stamps the row's `Option`-carried `title` onto the document so the tab caption follows the table, and the pending cell holds the target for the handler's duration so a guard-delayed transition renders progress instead of a torn location. Both cells publish `Subscribable` — the intercept stays the only writer structurally.
- Law: the admission slot speaks commit mechanics only — `Proceed`/`Divert`/`Refuse` is what an intercept mechanically honors; the policy semantics that produce them are `[5]`'s fold, handed in as the `admission` parameter at composition, so the router never imports the guard and the slot is the seam. The slot receives both endpoints — the departing location read from the cell and the arriving target — because departure policy is decided by where the user IS, not where they go.
- Law: the listener is the module's platform-forced statement seam — `event.intercept` runs synchronously inside the native dispatch, so the listener resolves the match synchronously and defers the admission/commit into the intercepted async handler through the captured runtime (`Runtime.runPromise` — the sanctioned callback-seam spelling); the implementer carries the `// BOUNDARY ADAPTER` mark on the navigate listener's first line. `window.navigation` is absent from the DOM lib; `_Navigation`/`_NavigateEvent` are the boundary refinements pinned here once.
- Law: last-good query continuity rides `persist#DOMAIN_ROWS`'s `route` domain — each commit persists the row's serialized search string under its key; at construction, an entry arriving with an EMPTY query whose key holds a last-good replays it through one `replace` navigation — an explicit URL always wins, restoration never overrides it.
- Law: hosts without the Navigation API fail the layer at construction with the class-carried `unsupported` fault — routing degrades loudly at the wiring proof, never silently to a dead SPA.
- Law: the correlated location construction is the page's marked kernel — a dynamic table's key/row pairing is evidence the checker cannot carry across the distributed union, so `_located` and `_fallback` assert the proven pairing to its element and the implementer carries the `// BOUNDARY ADAPTER` mark on each; with the pinned boundary refinements and the listener's event pin these are the module's only cast sites, and the cast algebra stays one-directional.
- Receipt: the Tag's `Shape` annotation is the whole consumer contract — cells, `href`, `go`, `back`, `forward` — readable without the body; the ui wave binds the cells through its atom bridge at app composition.
- Boundary: scroll restoration rides the intercept's own `scroll` option; view transitions are the ui wave's composition over the commit, never authored here.
- Packages: `effect` (`Context`, `Data`, `Effect`, `Layer`, `Option`, `Runtime`, `Stream`, `Subscribable`, `SubscriptionRef`); `./persist.ts` (`Kv`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/core"

type _NavigateEvent = Event & {
  readonly canIntercept: boolean
  readonly hashChange: boolean
  readonly downloadRequest: string | null
  readonly destination: { readonly url: string }
  readonly intercept: (options: {
    readonly handler: () => Promise<void>
    readonly scroll?: "after-transition" | "manual"
  }) => void
}

type _Navigation = EventTarget & {
  readonly currentEntry: { readonly url: string | null } | null
  readonly navigate: (url: string, options?: { readonly history?: "push" | "replace" }) => void
  readonly back: () => void
  readonly forward: () => void
}

class RouteFault extends Data.TaggedError("RouteFault")<{ readonly reason: "unsupported"; readonly detail: string }> {
  get class(): FaultClass.Kind {
    return "absent"
  }
}

const _navigation = (): Option.Option<_Navigation> =>
  Option.fromNullable((globalThis as typeof globalThis & { readonly navigation?: _Navigation }).navigation)

const _table = <P, const Rows extends Router.Rows<P>>(rows: Rows): Rows => rows

const _make = <const Rows extends Router.Rows>(spec: Router.Spec<Rows>) => {
  class Tag extends Context.Tag(spec.identifier)<Tag, Router.Shape<Rows>>() {}
  const _fallback = (url: URL): Router.Location<Rows> =>
    Option.getOrElse(_located(spec.rows, url), () => ({
      key: spec.fallback,
      segments: {} as Router.Params<Rows[typeof spec.fallback]["path"]>,
      query: createLoader(spec.rows[spec.fallback].query)(url),
    }) as Router.Location<Rows>)
  const layer = (
    admission: (departing: Router.Location<Rows>, arriving: Router.Location<Rows>) => Effect.Effect<Router.Admission>,
  ): Layer.Layer<Tag, KvFault | RouteFault, Kv> =>
    Layer.scoped(
      Tag,
      Effect.gen(function* () {
        const kv = yield* Kv
        const navigation = yield* Option.match(_navigation(), {
          onNone: () => new RouteFault({ reason: "unsupported", detail: "<no-navigation-api>" }),
          onSome: Effect.succeed,
        })
        const runtime = yield* Effect.runtime<never>()
        const origin = new URL(navigation.currentEntry?.url ?? globalThis.location.href)
        const seeded = _fallback(origin)
        const location = yield* SubscriptionRef.make(seeded)
        const pending = yield* SubscriptionRef.make(Option.none<Router.Location<Rows>>())
        const _commit = (target: Router.Location<Rows>, url: URL): Effect.Effect<void, KvFault> =>
          SubscriptionRef.set(location, target).pipe(
            Effect.zipRight(SubscriptionRef.set(pending, Option.none())),
            Effect.zipRight(Option.match(spec.rows[target.key].title, {
              onNone: () => Effect.void,
              onSome: (title) => Effect.sync(() => void (globalThis.document.title = title)),
            })),
            Effect.zipRight(kv.write("route", target.key, url.search)),
          )
        const _admitted = (target: Router.Location<Rows>, url: URL): Effect.Effect<void> =>
          SubscriptionRef.set(pending, Option.some(target)).pipe(
            Effect.zipRight(Effect.flatMap(SubscriptionRef.get(location), (current) => admission(current, target))),
            Effect.flatMap(
              Admission.$match({
                Proceed: () => Effect.ignore(_commit(target, url)),
                Divert: ({ to }) =>
                  SubscriptionRef.set(pending, Option.none()).pipe(
                    Effect.zipRight(Effect.sync(() => navigation.navigate(to, { history: "replace" }))),
                  ),
                Refuse: () => SubscriptionRef.set(pending, Option.none()),
              }),
            ),
          )
        yield* Stream.fromEventListener(navigation, "navigate").pipe(
          Stream.runForEach((raw) =>
            Effect.sync(() => {
              const event = raw as _NavigateEvent
              const url = new URL(event.destination.url)
              const external = !event.canIntercept || event.hashChange || event.downloadRequest !== null ||
                url.origin !== globalThis.location.origin
              if (!external) {
                event.intercept({
                  handler: () => Runtime.runPromise(runtime)(_admitted(_fallback(url), url)),
                  scroll: "after-transition",
                })
              }
            }),
          ),
          Effect.forkScoped,
        )
        const held = yield* kv.read("route", seeded.key)
        yield* Effect.when(
          Effect.sync(() => navigation.navigate(`${origin.pathname}${Option.getOrElse(held, () => "")}`, { history: "replace" })),
          () => origin.search === "" && Option.getOrElse(held, () => "") !== "",
        )
        return {
          location,
          pending,
          href: (key, segments, query) => _href(spec.rows, key, segments, query),
          go: (target, history) => Effect.sync(() => navigation.navigate(target, { history: history ?? "push" })),
          back: Effect.sync(() => navigation.back()),
          forward: Effect.sync(() => navigation.forward()),
        }
      }),
    )
  return { Tag, layer } as const
}

const Router: {
  readonly Admission: Data.TaggedEnum.Constructor<Router.Admission>
  readonly param: typeof _param
  readonly standard: <P extends ParserMap>(map: P) => ReturnType<typeof createStandardSchemaV1<P, true>>
  readonly table: typeof _table
  readonly make: typeof _make
} = {
  Admission,
  param: _param,
  standard: (map) => createStandardSchemaV1(map, { partialOutput: true }),
  table: _table,
  make: _make,
}
```

## [4]-[SESSION_PLANE]

[SESSION_PLANE]:
- Owner: `SessionStatus`, one process-local `Data.taggedEnum` — `Anonymous`, `Authenticating`, `Authenticated { subject, expiresAt }`, `Expired` — constructed only through its generated constructors so every guard and affordance dispatch rides `$match`/`$is`; and `Vault`, one scoped `Effect.Service` built through `Vault.Default(spec)` — `status` (the one cell, published `Subscribable`), the transitions (`established`/`authenticating`/`cleared` — local transitions publish to the cross-tab channel, foreign folds never re-publish, so the channel cannot echo), `csrf` (the double-submit header pair read from the readable cookie under `security/authn/session`'s `CookieSpec.csrf` name, `Option`-carried), `posture` (`"include"` — the credentials row `fetch#DIAL_SURFACE` stamps on every dial), and the redirect continuity — `depart(plan)` persists the pending flow into `persist#DOMAIN_ROWS`'s `flow` domain then commits the full-page navigation, `land(url, exchange)` re-reads the flow single-use, guards replay, lapse, and the state echo, extracts the callback code, hands `{ code, state }` to the app-supplied exchange leg, and folds the fresh session into the cell.
- Law: `Authenticated` carries evidence, not secrets — `subject` is a display-grade identity string and `expiresAt` the refresh watermark; the credential itself never exists in this vocabulary because the cookie residency law keeps it out of script reach entirely, and a token string in Web Storage or a readable session cookie is the named residency defect.
- Law: the refresh arm is supersede-keyed — one `FiberHandle` holds at most one sleeper; each `Authenticated` transition re-arms it to wake at `expiresAt` minus the lead, run `spec.refresh` (the edge round-trip that re-establishes the cookie session), and fold the outcome (`some` re-establishes, `none` expires); any other phase replaces the sleeper with `Effect.void`, so a signed-out tab holds no timer and two sleepers cannot race; a refresh success publishes the fresh expiry to the channel, so sibling tabs fold forward and re-arm to the later watermark — the first refresher wins and the others move their timers instead of re-dialing.
- Law: cross-tab truth is one `BroadcastChannel` held `Effect.acquireRelease` — a decoded `Established` folds the foreign fact into the local cell, `Cleared` signs every tab out at once, and an undecodable message is dropped because a foreign tab's garbage is not this rail's fault; `Expired` and `Authenticating` are local phases that never cross the channel, because a foreign tab observing them acts on another tab's transient.
- Law: the CSRF read is the one sanctioned `document.cookie` touch in the branch — the cookie scan is expression-shaped over the split rows, the value decodes through `Encoding.decodeUriComponent` (an `Either`, never a thrown `URIError`), and absence is `Option.none` the caller folds; the cookie NAME composes from `security`'s `CookieSpec` table, never a local respelling, and the server refuses a mutation without the echo so no browser-side guard re-checks it.
- Law: the pending flow is single-use and time-bounded — `land` drops the record before acting on it, so a replayed callback finds nothing and folds to `replay`; a record older than `spec.grace` folds to `lapsed`; the state echo, when the record carries one, equals the callback's or folds to `replay` — defense in depth beside the server-side single-use stash `security/authn/oauth` owns. The departure commit is the module's platform-forced statement seam — `location.assign` unloads the document, nothing sequences after it, and the implementer carries the `// BOUNDARY ADAPTER` mark on `_departed`'s first line.
- Law: passkey ceremonies stay out of this plane — `security/authn/webauthn`'s `Passkeys` owns the `navigator.credentials` invocation and the ui wave owns the option/response POST-back legs; this plane owns only the phase cell those legs drive through the transitions.
- Receipt: `land` yields the flow's `returnTo` beside the established session so the traversal owner restores the interrupted destination; `csrf` yields the ready `[name, value]` header pair.
- Growth: a new phase is one case on the enum plus its `$match` arms breaking loudly; a new cross-tab fact is one `_Signal` member; a new continuity guard is one `FlowFault` reason row.
- Boundary: `security/authn/session` owns the server-side `Session`/`TokenPair` truth and the cookie attribute table; the refresh and exchange endpoints are app data the composition root supplies; this owner never dials.
- Packages: `@rasm/ts/security` (`CookieSpec`); `effect` (`Data`, `DateTime`, `Duration`, `Effect`, `Encoding`, `FiberHandle`, `Option`, `Order`, `Schema`, `Stream`, `Subscribable`, `SubscriptionRef`); `./persist.ts` (`Kv`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { CookieSpec } from "@rasm/ts/security"
import { DateTime, Duration, Encoding, FiberHandle, Order } from "effect"

const _CHANNEL = "rasm-session"
const _FLOW_KEY = "pending"

const _Signal = Schema.parseJson(Schema.Union(
  Schema.TaggedStruct("Established", { subject: Schema.NonEmptyString, expiresAt: Schema.DateTimeUtc }),
  Schema.TaggedStruct("Cleared", {}),
))

type SessionStatus = Data.TaggedEnum<{
  Anonymous: {}
  Authenticating: {}
  Authenticated: { readonly subject: string; readonly expiresAt: DateTime.Utc }
  Expired: {}
}>
const SessionStatus: Data.TaggedEnum.Constructor<SessionStatus> = Data.taggedEnum<SessionStatus>()

const _flowFaults = {
  replay: { class: "conflicted" },
  lapsed: { class: "expired" },
  malformed: { class: "malformed" },
} as const

declare namespace FlowFault {
  type Reason = keyof typeof _flowFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _flowFaults> = T
}

class FlowFault extends Data.TaggedError("FlowFault")<{
  readonly reason: FlowFault.Reason
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _flowFaults[this.reason].class
  }
}

declare namespace Vault {
  type Fresh = { readonly subject: string; readonly expiresAt: DateTime.Utc }
  type Plan = { readonly authorize: URL; readonly returnTo: string; readonly state: Option.Option<string> }
  type Landing = { readonly returnTo: string }
  type Exchange<E> = (payload: {
    readonly code: string
    readonly state: Option.Option<string>
  }) => Effect.Effect<Fresh, E>
  type Spec = {
    readonly lead: Duration.DurationInput
    readonly grace: Duration.DurationInput
    readonly refresh: Effect.Effect<Option.Option<Fresh>>
  }
}

const _cookie = (name: string): Option.Option<string> =>
  Array.findFirst(globalThis.document.cookie.split("; "), (row) => row.startsWith(`${name}=`)).pipe(
    Option.flatMap((row) => Option.getRight(Encoding.decodeUriComponent(row.slice(name.length + 1)))),
  )

class Vault extends Effect.Service<Vault>()("runtime/browser/Vault", {
  scoped: (spec: Vault.Spec) =>
    Effect.gen(function* () {
      const kv = yield* Kv
      const _status = yield* SubscriptionRef.make<SessionStatus>(SessionStatus.Anonymous())
      const sleeper = yield* FiberHandle.make()
      const channel = yield* Effect.acquireRelease(
        Effect.sync(() => new globalThis.BroadcastChannel(_CHANNEL)),
        (held) => Effect.sync(() => held.close()),
      )
      const _publish = (signal: typeof _Signal.Type): Effect.Effect<void> =>
        Effect.sync(() => channel.postMessage(Schema.encodeSync(_Signal)(signal)))
      const _armed = (held: SessionStatus): Effect.Effect<void> =>
        SessionStatus.$match(held, {
          Anonymous: () => FiberHandle.run(sleeper, Effect.void),
          Authenticating: () => FiberHandle.run(sleeper, Effect.void),
          Expired: () => FiberHandle.run(sleeper, Effect.void),
          Authenticated: ({ expiresAt }) =>
            FiberHandle.run(
              sleeper,
              Effect.gen(function* () {
                const now = yield* DateTime.now
                const wait = Order.max(Duration.Order)(
                  Duration.subtract(DateTime.distanceDuration(now, expiresAt), Duration.decode(spec.lead)),
                  Duration.zero,
                )
                yield* Effect.sleep(wait)
                const fresh = yield* spec.refresh
                yield* Option.match(fresh, {
                  onNone: () => SubscriptionRef.set(_status, SessionStatus.Expired()),
                  onSome: ({ expiresAt, subject }) =>
                    SubscriptionRef.set(_status, SessionStatus.Authenticated({ subject, expiresAt })).pipe(
                      Effect.zipRight(_publish({ _tag: "Established", subject, expiresAt })),
                    ),
                })
              }),
            ),
        })
      yield* Stream.fromEventListener<MessageEvent>(channel, "message").pipe(
        Stream.runForEach((event) =>
          Schema.decodeUnknown(_Signal)(event.data).pipe(
            Effect.flatMap((signal) =>
              SubscriptionRef.set(
                _status,
                signal._tag === "Established"
                  ? SessionStatus.Authenticated({ subject: signal.subject, expiresAt: signal.expiresAt })
                  : SessionStatus.Anonymous(),
              ),
            ),
            Effect.ignore,
          ),
        ),
        Effect.forkScoped,
      )
      yield* _status.changes.pipe(Stream.runForEach(_armed), Effect.forkScoped)
      const _departed = (target: URL): Effect.Effect<never> =>
        Effect.zipRight(Effect.sync(() => globalThis.location.assign(target.toString())), Effect.never)
      const depart = (plan: Vault.Plan): Effect.Effect<never, KvFault> =>
        SubscriptionRef.set(_status, SessionStatus.Authenticating()).pipe(
          Effect.zipRight(
            Effect.flatMap(DateTime.now, (minted) =>
              kv.write("flow", _FLOW_KEY, { state: plan.state, returnTo: plan.returnTo, minted }),
            ),
          ),
          Effect.zipRight(_departed(plan.authorize)),
        )
      const land = <E>(
        callback: URL,
        exchange: Vault.Exchange<E>,
      ): Effect.Effect<Vault.Landing, FlowFault | KvFault | E> =>
        Effect.gen(function* () {
          const held = yield* kv.read("flow", _FLOW_KEY)
          yield* kv.drop("flow", _FLOW_KEY)
          const flow = yield* Option.match(held, {
            onNone: () => new FlowFault({ reason: "replay", detail: "<no-pending-flow>" }),
            onSome: Effect.succeed,
          })
          const now = yield* DateTime.now
          yield* Effect.when(
            new FlowFault({ reason: "lapsed", detail: "<flow-expired>" }),
            () => Duration.greaterThan(DateTime.distanceDuration(flow.minted, now), Duration.decode(spec.grace)),
          )
          const state = Option.fromNullable(callback.searchParams.get("state"))
          yield* Effect.when(
            new FlowFault({ reason: "replay", detail: "<state-mismatch>" }),
            () =>
              Option.match(flow.state, {
                onNone: () => false,
                onSome: (expected) => !Option.contains(state, expected),
              }),
          )
          const code = yield* Option.match(Option.fromNullable(callback.searchParams.get("code")), {
            onNone: () => new FlowFault({ reason: "malformed", detail: "<no-code>" }),
            onSome: Effect.succeed,
          })
          const fresh = yield* exchange({ code, state })
          yield* SubscriptionRef.set(_status, SessionStatus.Authenticated(fresh)).pipe(
            Effect.zipRight(_publish({ _tag: "Established", subject: fresh.subject, expiresAt: fresh.expiresAt })),
          )
          return { returnTo: flow.returnTo }
        })
      const status: Subscribable.Subscribable<SessionStatus> = _status
      return {
        status,
        posture: "include" as const,
        established: (fresh: Vault.Fresh) =>
          SubscriptionRef.set(_status, SessionStatus.Authenticated(fresh)).pipe(
            Effect.zipRight(_publish({ _tag: "Established", subject: fresh.subject, expiresAt: fresh.expiresAt })),
          ),
        authenticating: SubscriptionRef.set(_status, SessionStatus.Authenticating()),
        cleared: SubscriptionRef.set(_status, SessionStatus.Anonymous()).pipe(
          Effect.zipRight(_publish({ _tag: "Cleared" })),
        ),
        csrf: Effect.sync(() =>
          Option.map(_cookie(CookieSpec.csrf.name), (token) => [CookieSpec.csrf.name, token] as const),
        ),
        depart,
        land,
      }
    }),
  accessors: true,
}) {}
```

## [5]-[ADMISSION_FOLD]

[ADMISSION_FOLD]:
- Owner: `Guard`, one scoped `Effect.Service` built through `Guard.Default(spec)` — `spec` carries the app-composed `availability` snapshot read (`Option`-carried: no evidence feed composed means no command gating), the `flag` verdict leg (`Option`-carried: the root satisfies it from whatever flag surface the deployment serves — `proc/flag`'s edge-served snapshot or `security`'s `FlagGate` over the subject's claims — so this definition names no engine), and the `settle` budget an in-flight authentication spends before the fold treats it as expired. Members: `resolve(departing, arriving)` — the one admission fold; `hold(token)` — the scoped dirty marker; `dirty` — the registry read, published `Subscribable` so only `hold`'s bracket writes it. `Guard.Policy` is the per-route policy value the table's rows carry — `session`, `flag`, `command`, `leave`, each `Option`-carried so a route states only the gates it earns — with `Guard.open` the all-`none` row and `Guard.policy(overrides)` the spread constructor. `Confirm` is the port Tag the ui wave satisfies at composition: one prompt-to-boolean ceremony.
- Law: the chain is ordered and first-refusal-wins — departure confirm (only when the departing row carries `leave` AND dirty work is held), then session, then flag, then command; the chain is one `Effect.reduce` over the four arm effects — an arm runs only while the held verdict is `Proceed`, so the first refusal short-circuits, the fold is total, and a new gate is one arm value in the list, never a branch.
- Law: `Authenticating` is waited out, never guessed — the session arm suspends on `Vault.status`'s change feed until a settled phase arrives or the `settle` budget lapses, and a lapse folds as `Expired` (divert), so a slow ceremony renders the router's pending affordance instead of a wrong verdict.
- Law: absence admits — a missing `Confirm` port proceeds (the native `beforeunload` arm still covers tab close), an absent flag leg admits, absent availability evidence admits, and only a `Withheld` verdict refuses (`Gated` proceeds — page-level affordances own gated rendering); the guard degrades open on missing evidence and shut on explicit refusal.
- Law: the command arm folds the core lattice — `Availability.admits(snapshot, command)` answers total over the level fallbacks, so a command absent from the snapshot's map still lands a verdict and the guard never re-derives posture the lattice already decides.
- Law: the `beforeunload` arm and its synchronous registry read are the module's platform-forced statement seam — the native handler decides within its dispatch, so it reads the dirty cell through the captured runtime's sync run (the sanctioned callback-seam spelling) and prevents default only while work is held; the implementer carries the `// BOUNDARY ADAPTER` mark on the `beforeunload` bracket's first line.
- Law: policy is data on the route row — `[2]`'s `Row.policy` carries this value, the app's admission wiring reads the departing and arriving rows, and no parallel policy registry exists; gate targets are hrefs minted by the router, so a divert target is typed at its mint site and the guard never assembles URLs.
- Receipt: `resolve`'s annotation states the whole read surface — `Router.Admission` out, no fault channel, requirement-free by construction.
- Entry: the app's composition maps the router's endpoint pair onto its rows — `(from, to) => guard.resolve(Option.some(rows[from.key].policy), rows[to.key].policy)` — one line in `main.ts`, zero lib coupling between the two clusters.
- Growth: a tenant gate, a capability gate, or a quota gate is one `Option` field plus one chain arm.
- Boundary: what a flag verdict means is its serving surface's law; what availability means is `core/state/evidence`'s lattice; the confirm ceremony renders behind the ui-satisfied `Confirm` Tag; this cluster owns only the fold order.
- Packages: `effect` (`Context`, `Duration`, `Effect`, `HashSet`, `Option`, `Runtime`, `Stream`, `Subscribable`, `SubscriptionRef`); `@rasm/ts/core` (`Availability`).

```typescript
import { Availability } from "@rasm/ts/core"
import { HashSet, type Scope } from "effect"

declare namespace Guard {
  type Prompt = { readonly title: string; readonly detail: string }
  type Policy = {
    readonly session: Option.Option<{ readonly to: string }>
    readonly flag: Option.Option<{ readonly name: string; readonly to: string }>
    readonly command: Option.Option<Availability.Command>
    readonly leave: Option.Option<Prompt>
  }
  type Spec = {
    readonly availability: Effect.Effect<Option.Option<Availability>>
    readonly flag: Option.Option<(name: string) => Effect.Effect<boolean>>
    readonly settle: Duration.DurationInput
  }
}

const _PROCEED: Router.Admission = Router.Admission.Proceed()

const _OPEN: Guard.Policy = {
  session: Option.none(),
  flag: Option.none(),
  command: Option.none(),
  leave: Option.none(),
}

class Confirm extends Context.Tag("runtime/browser/Confirm")<Confirm, (prompt: Guard.Prompt) => Effect.Effect<boolean>>() {}

class Guard extends Effect.Service<Guard>()("runtime/browser/Guard", {
  scoped: (spec: Guard.Spec) =>
    Effect.gen(function* () {
      const vault = yield* Vault
      const confirm = yield* Effect.serviceOption(Confirm)
      const runtime = yield* Effect.runtime<never>()
      const _dirty = yield* SubscriptionRef.make(HashSet.empty<string>())
      yield* Effect.acquireRelease(
        Effect.sync(() => {
          const handler = (event: BeforeUnloadEvent) => {
            const held = Runtime.runSync(runtime)(SubscriptionRef.get(_dirty))
            if (HashSet.size(held) > 0) event.preventDefault()
          }
          globalThis.addEventListener("beforeunload", handler)
          return handler
        }),
        (handler) => Effect.sync(() => globalThis.removeEventListener("beforeunload", handler)),
      )
      const _settled: Effect.Effect<SessionStatus> = Effect.flatMap(vault.status.get, (held) =>
        SessionStatus.$is("Authenticating")(held)
          ? vault.status.changes.pipe(
              Stream.filter((status) => !SessionStatus.$is("Authenticating")(status)),
              Stream.runHead,
              Effect.timeoutOption(spec.settle),
              Effect.map((outcome) => Option.getOrElse(Option.flatten(outcome), () => SessionStatus.Expired())),
            )
          : Effect.succeed(held),
      )
      const _asked = (prompt: Guard.Prompt): Effect.Effect<boolean> =>
        Option.match(confirm, { onNone: () => Effect.succeed(true), onSome: (ask) => ask(prompt) })
      const _leaveArm = (departing: Option.Option<Guard.Policy>): Effect.Effect<Router.Admission> =>
        Effect.flatMap(SubscriptionRef.get(_dirty), (held) =>
          Option.match(Option.flatMap(departing, (policy) => policy.leave), {
            onNone: () => Effect.succeed(Router.Admission.Proceed()),
            onSome: (prompt) =>
              HashSet.size(held) === 0
                ? Effect.succeed(Router.Admission.Proceed())
                : Effect.map(_asked(prompt), (leave) => (leave ? Router.Admission.Proceed() : Router.Admission.Refuse())),
          }),
        )
      const _sessionArm = (policy: Guard.Policy): Effect.Effect<Router.Admission> =>
        Option.match(policy.session, {
          onNone: () => Effect.succeed(Router.Admission.Proceed()),
          onSome: ({ to }) =>
            Effect.map(_settled, (status) =>
              SessionStatus.$is("Authenticated")(status) ? Router.Admission.Proceed() : Router.Admission.Divert({ to }),
            ),
        })
      const _flagArm = (policy: Guard.Policy): Effect.Effect<Router.Admission> =>
        Option.match(Option.zipWith(policy.flag, spec.flag, (gate, read) => [gate, read] as const), {
          onNone: () => Effect.succeed(Router.Admission.Proceed()),
          onSome: ([gate, read]) =>
            Effect.map(read(gate.name), (held) =>
              held ? Router.Admission.Proceed() : Router.Admission.Divert({ to: gate.to }),
            ),
        })
      const _commandArm = (policy: Guard.Policy): Effect.Effect<Router.Admission> =>
        Option.match(policy.command, {
          onNone: () => Effect.succeed(Router.Admission.Proceed()),
          onSome: (command) =>
            Effect.map(spec.availability, (evidence) =>
              Option.match(evidence, {
                onNone: () => Router.Admission.Proceed(),
                onSome: (snapshot) =>
                  Availability.admits(snapshot, command)._tag === "Withheld"
                    ? Router.Admission.Refuse()
                    : Router.Admission.Proceed(),
              }),
            ),
        })
      const resolve = (departing: Option.Option<Guard.Policy>, arriving: Guard.Policy): Effect.Effect<Router.Admission> =>
        Effect.reduce(
          [_leaveArm(departing), _sessionArm(arriving), _flagArm(arriving), _commandArm(arriving)],
          _PROCEED,
          (held, arm) => (Router.Admission.$is("Proceed")(held) ? arm : Effect.succeed(held)),
        )
      const hold = (token: string): Effect.Effect<void, never, Scope.Scope> =>
        Effect.acquireRelease(
          SubscriptionRef.update(_dirty, HashSet.add(token)),
          () => SubscriptionRef.update(_dirty, HashSet.remove(token)),
        )
      const dirty: Subscribable.Subscribable<HashSet.HashSet<string>> = _dirty
      return { resolve, hold, dirty }
    }),
  accessors: true,
}) {
  static readonly open: Guard.Policy = _OPEN
  static readonly policy = (overrides: Partial<Guard.Policy>): Guard.Policy => ({ ..._OPEN, ...overrides })
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Confirm, FlowFault, Guard, RouteFault, Router, SessionStatus, Vault }
```
