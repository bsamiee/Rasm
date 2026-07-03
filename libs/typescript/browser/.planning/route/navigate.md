# [BROWSER_NAVIGATE]

`route/navigate.ts` is the Navigation-API typed router under the zero-routing-package law: the route table is app DATA — one row per route carrying its segment template, its `nuqs` query codec, and its guard policy value — and the lib owns the algebra over it: segment-param types derived from the template literal itself, the query plane decoded and serialized through the `nuqs/server` codec core (`[R17]`), one interceptable ingress over `window.navigation` owning link clicks, form submits, back/forward, and programmatic traversal alike, and one commit path — every navigation resolves through the intercept, so the location cell and the URL cannot tear. `nuqs` is the codec, the Navigation API is the traversal, `Schema`-grade typing rides the derivation — no router package, no `popstate`/`pushState` split, no hand-mashed `URLSearchParams`. The `Router.make` factory mints the app's own Tag and layer over its table (the `{ tag, layer }` precedent), so hundreds of apps carry fully-typed locations with zero lib edits.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              | [PUBLIC]  |
| :-----: | :---------------- | :--------------------------------------------------------------------- | :-------- |
|  [01]   | `TABLE_ALGEBRA`   | the row shape, segment-type derivation, match/href, the query codec    | `Router`  |
|  [02]   | `TRAVERSAL_OWNER` | the minted Tag, the intercept ingress, cells, restore, commit law      | `Router`  |

## [2]-[TABLE_ALGEBRA]

[TABLE_ALGEBRA]:
- Owner: the `Router` type plane and pure algebra — `Router.Row<P>` (`path` template, `query` `ParserMap`, `policy` the guard-interpreted value, `title` `Option`), `Router.Params<Path>` deriving the segment record from the template literal (`"/doc/:key/:rev"` yields `{ key: string; rev: string }` — the correspondence computes, never restates), `Router.Location<Rows>` (the key-discriminated union of `{ key, segments, query }`), `_matched` (template against pathname, one fold, `Option` of captures), and the per-row `createLoader`/`createSerializer` pair — decode and write of the query plane through one `ParserMap`, so loader and serializer cannot drift.
- Law: the query codec is `nuqs/server` and only that — the `parseAs*` atoms, `createParser` for bespoke encodings, `parseAsJson` over `Schema.standardSchemaV1` where a structured value rides one param; the React hook/adapter surface is `ui`-tier and never imported here.
- Law: a route is one row — path, codec, policy, title travel together, so `route/guard` reads its policy from the matched row and no parallel policy registry exists; a new route is one row plus its compile-time ripple through every exhaustive location consumer.
- Law: `href` is the one URL mint — segments substitute into the template, the serializer writes the query with `clearOnDefault` semantics, and a hand-concatenated URL anywhere in an app is the string-mash this owner deletes.
- Growth: a new query key is one `ParserMap` field on its row; a new route axis (a transition hint, a preload row) is one `Row` field every table inherits.
- Boundary: what a policy MEANS is `route/guard`'s fold; this page carries it as an opaque row value.
- Packages: `nuqs/server` (`createLoader`, `createSerializer`, `parseAs*`, `createParser`, `type ParserMap`, `type inferParserType`); `effect` (`Option`, `Array`, `Record`).

```typescript
import { Array, Context, Data, Effect, Layer, Option, Record, Runtime, Stream, Subscribable, SubscriptionRef } from "effect"
import { createLoader, createSerializer, type inferParserType, type ParserMap } from "nuqs/server"
import { Kv, type KvFault } from "../persist/kv.ts"

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
- Law: the admission slot speaks commit mechanics only — `Proceed`/`Divert`/`Refuse` is what an intercept can mechanically honor; the policy semantics that produce them (session, entitlement, confirm ceremonies) are `route/guard`'s fold, handed in as the `admission` parameter at composition, so navigate never imports guard and the slot is the seam. The slot receives both endpoints — the departing location read from the cell and the arriving target — because departure policy (a confirm-on-exit row) is decided by where the user IS, not where they go.
- Law: the listener is the module's platform-forced statement seam — `event.intercept` must be called synchronously inside the native dispatch, so the listener resolves the match synchronously and defers the admission/commit into the intercepted async handler through the captured runtime (`Runtime.runPromise` — the sanctioned callback-seam spelling); the implementer carries the `// BOUNDARY ADAPTER` mark on the navigate listener's first line. `window.navigation` is absent from the DOM lib; `_Navigation`/`_NavigateEvent` are the boundary refinements pinned here once.
- Law: last-good query continuity rides `persist/kv`'s `route` domain — each commit persists the row's serialized search string under its key; at construction, an entry arriving with an EMPTY query whose key holds a last-good replays it through one `replace` navigation — an explicit URL always wins, restoration never overrides it.
- Law: hosts without the Navigation API fail the layer at construction with the typed `unsupported` fault — routing degrades loudly at the wiring proof, never silently to a dead SPA.
- Law: the correlated location construction is the page's marked kernel — a dynamic table's key/row pairing is evidence the checker cannot carry across the distributed union, so `_located` and `_fallback` assert the proven pairing to its element and the implementer carries the `// BOUNDARY ADAPTER` mark on each; with the pinned boundary refinements and the listener's event pin these are the module's only cast sites, and the cast algebra stays one-directional.
- Receipt: the Tag's `Shape` annotation is the whole consumer contract — cells, `href`, `go`, `back`, `forward` — readable without the body; `ui` binds the cells through its atom bridge at app composition.
- Boundary: scroll restoration rides the intercept's own `scroll` option; view transitions are `ui/act`'s composition over the commit, never authored here.
- Packages: `effect` (`Context`, `Layer`, `Runtime`, `Stream`, `SubscriptionRef`, `Subscribable`, `Effect`, `Option`, `Data`); `../persist/kv.ts` (`Kv`).

```typescript
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

class RouteFault extends Data.TaggedError("RouteFault")<{ readonly reason: "unsupported"; readonly detail: string }> {}

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
              const external = !event.canIntercept || event.hashChange || event.downloadRequest !== null || url.origin !== globalThis.location.origin
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
  readonly table: typeof _table
  readonly make: typeof _make
} = { Admission, table: _table, make: _make }

// --- [EXPORTS] --------------------------------------------------------------------------

export { RouteFault, Router }
```
