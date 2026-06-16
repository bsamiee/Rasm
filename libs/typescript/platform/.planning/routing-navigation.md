# [PLATFORM_ROUTING_NAVIGATION]

One page owns the browser client routing and navigation infrastructure — `AppRouter`, the Effect-native router over one `Schema.Literal` route-key axis with the active location and pending transition held in a history `SubscriptionRef`; `RouteParamCodec`, the per-route `Schema` round-trip that composes the `nuqs` query-state with the path segments; and `NavigationGuard`, the route-admission fold gating a route on the `platform` `AuthSession` status and the `projection` `AvailabilityStore`. The page admits ZERO routing package — `Schema`, `nuqs`, and a hand-held history `SubscriptionRef` own the concern, react-router/tanstack-router are an uncatalogued deliberate no-admission. Route-resident state lives in the router cell and the query string and reaches a component only through the `ui` `AtomBinding`, never duplicated into component state. The page crosses no wire contract and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                            |
| :-----: | :----------------- | :--------------------------------------------------------------- |
|   [1]   | ROUTING_NAVIGATION | the route-key axis, the param codec, navigation, and the guard   |

## [2]-[ROUTING_NAVIGATION]

- Owner: `AppRouter`, the single client router — one `Schema.Literal` route-key axis with a behavior `Record` carrying each route's segment template, param codec, guard policy, and default params (the data-table source the redirect resolution reads, never an inline `{}` cast), the active `Location` and the pending transition held in one history `SubscriptionRef`, and `push`/`replace`/`back`/`forward` as typed `Effect` transitions; `RouteParamCodec`, the per-route `Schema` round-trip composing the path-segment decode with the `nuqs` query-state codec into one `Location`; and `NavigationGuard`, the admission fold over `AuthSession.status` and the `projection` `AvailabilityStore`. The route-key literal is the one routing vocabulary and a parallel route-string constant is the named const-spam defect.
- Cases: the route-key axis is one `Schema.Literal` union (`evidence-timeline`/`benchmark`/`collector`/`geo-series`/`viewport`/`login`) with a companion `RouteBehavior` `Record` keyed by the literal, so a new route is one literal and one `Record` row, never a parallel route table; `AppRouter` binds the `popstate` ingress through `BrowserStream.fromEventListenerWindow("popstate")` folded into the history `SubscriptionRef`, so the back/forward gesture and the programmatic transition resolve through one cell; route resolution parses `window.location.pathname` against the active route's segment template and `RouteParamCodec` decodes the path segments and the `nuqs` query state into the typed `RouteParams` for that key in one `Schema.decodeUnknown` pass, an undecodable URL folding to the `not-found` resolution rather than throwing; navigation is a typed `Effect` transition that resolves the guard, encodes the target `Location` back to a URL string through `RouteParamCodec`, calls `history.pushState`/`history.replaceState`, and sets the history `SubscriptionRef` so the active location and the URL stay one fact; the scroll-restoration cell records the scroll offset per history entry and restores it on a `back`/`forward` resolution; the pending-navigation transition cell holds the in-flight target so a guard awaiting an `AvailabilityStore` read renders a pending affordance rather than a torn location.
- Auto: `NavigationGuard` resolves as a total fold — it reads `AuthSession.status` and, for an availability-gated route, the matching `projection` `AvailabilityStore` row, and returns a `GuardOutcome` (`admit`/`redirect`/`pending`); an `Anonymous` or `Expired` session on a protected route folds to `redirect("login")` and a route whose command is unavailable folds to `pending` against the live availability cell, so the guard is one fold over two read cells, never an imperative `if (!user) navigate(...)` scattered across components; the guard read never mutates the session or the availability fold.
- Packages: `effect` for the `Schema.Literal` route axis, the `SubscriptionRef` history cell, `Match` for the guard fold, and the `Schema` param round-trip; `nuqs` for the query-string state codec composed inside `RouteParamCodec`; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the `popstate` ingress. ZERO router package — `react-router`/`tanstack-router`/`history` are uncatalogued and `catalogMode: strict` rejects them, recorded in the charter ADMISSIONS_RECORD as a deliberate no-admission.
- Growth: a new route lands as one literal on the route-key axis and one `RouteBehavior` `Record` row carrying its segment template, param codec, guard policy, and default params; a new query-state key lands as one `nuqs`-composed field on that route's `RouteParamCodec`; a new guard condition lands as one arm on the `NavigationGuard` fold, never a parallel guard.
- Boundary: `AppRouter` is the single routing owner and a parallel route-string constant or a second navigation surface is the named defect; route-resident state lives in the history `SubscriptionRef` and the query string, surfaced to a component only through the `ui` `AtomBinding`, and a route value copied into local component state is the named duplication defect; the guard reads the `platform` `AuthSession` and the `projection` `AvailabilityStore` and emits no command — navigation triggers an intent only through the `interchange` `CommandGateway` when a route transition carries one; the router never dials a transport and authors no decode; `platform` composes the router into the SPA entry and `ui` never imports it.

```ts contract
type RouteKey = "evidence-timeline" | "benchmark" | "collector" | "geo-series" | "viewport" | "login" | "not-found";

type RouteParams = {
  readonly "evidence-timeline": { readonly correlation: Option.Option<string>; readonly since: Option.Option<number> };
  readonly benchmark: { readonly suite: Option.Option<string> };
  readonly collector: Record<string, never>;
  readonly "geo-series": { readonly layer: string; readonly bbox: Option.Option<readonly [number, number, number, number]> };
  readonly viewport: { readonly artifactId: string };
  readonly login: { readonly returnTo: Option.Option<string> };
  readonly "not-found": Record<string, never>;
};

type Location = { readonly [K in RouteKey]: { readonly key: K; readonly params: RouteParams[K] } }[RouteKey];

type GuardOutcome =
  | { readonly _tag: "Admit" }
  | { readonly _tag: "Redirect"; readonly to: RouteKey }
  | { readonly _tag: "Pending"; readonly reason: string };

interface RouteParamCodec<K extends RouteKey> {
  readonly decode: (pathname: string, search: URLSearchParams) => Effect.Effect<RouteParams[K], ParseResult.ParseError>;
  readonly encode: (params: RouteParams[K]) => Effect.Effect<{ readonly pathname: string; readonly search: string }, ParseResult.ParseError>;
}

interface RouteBehavior<K extends RouteKey> {
  readonly segmentTemplate: string;
  readonly codec: RouteParamCodec<K>;
  readonly protectedRoute: boolean;
  readonly availabilityKey: Option.Option<string>;
  readonly defaultParams: RouteParams[K];
}

interface NavigationGuard {
  readonly resolve: (key: RouteKey) => Effect.Effect<GuardOutcome, never, AuthSession | AvailabilityStore>;
}

interface AppRouter {
  readonly location: SubscriptionRef.SubscriptionRef<Location>;
  readonly pending: SubscriptionRef.SubscriptionRef<Option.Option<Location>>;
  readonly resolve: (url: string) => Effect.Effect<Location>;
  readonly push: (target: Location) => Effect.Effect<void, ParseResult.ParseError, AuthSession | AvailabilityStore>;
  readonly replace: (target: Location) => Effect.Effect<void, ParseResult.ParseError, AuthSession | AvailabilityStore>;
  readonly back: Effect.Effect<void>;
  readonly forward: Effect.Effect<void>;
}

const makeAppRouter: Effect.Effect<AppRouter, never, Scope.Scope | AuthSession | AvailabilityStore> = Effect.gen(function* () {
  const behavior = yield* Effect.succeed(routeBehaviorTable);
  const guard = makeNavigationGuard(behavior);
  const initial = yield* resolveUrl(behavior, window.location.pathname + window.location.search);
  const location = yield* SubscriptionRef.make<Location>(initial);
  const pending = yield* SubscriptionRef.make<Option.Option<Location>>(Option.none());
  const scroll = yield* Ref.make<ReadonlyMap<string, number>>(new Map());
  yield* BrowserStream.fromEventListenerWindow("popstate").pipe(
    Stream.mapEffect(() => resolveUrl(behavior, window.location.pathname + window.location.search)),
    Stream.tap((next) => SubscriptionRef.set(location, next)),
    Stream.tap(() =>
      Ref.get(scroll).pipe(
        Effect.map((m) => m.get(window.location.pathname)),
        Effect.tap((y) => (y === undefined ? Effect.void : Effect.sync(() => window.scrollTo(0, y)))),
      )),
    Stream.runDrain,
    Effect.forkScoped,
  );
  const transition = (mode: "push" | "replace") => (target: Location) =>
    guard.resolve(target.key).pipe(
      Effect.flatMap((outcome) =>
        outcome._tag === "Admit"
          ? behavior[target.key].codec.encode(target.params).pipe(
              Effect.tap(({ pathname, search }) =>
                Effect.sync(() => history[mode === "push" ? "pushState" : "replaceState"]({}, "", pathname + search))),
              Effect.zipRight(SubscriptionRef.set(pending, Option.none())),
              Effect.zipRight(SubscriptionRef.set(location, target)),
            )
          : outcome._tag === "Redirect"
            ? transition("replace")({ key: outcome.to, params: behavior[outcome.to].defaultParams } as Location)
            : SubscriptionRef.set(pending, Option.some(target)))) as Effect.Effect<void, ParseResult.ParseError, AuthSession | AvailabilityStore>;
  return {
    location,
    pending,
    resolve: (url) => resolveUrl(behavior, url),
    push: transition("push"),
    replace: transition("replace"),
    back: Effect.sync(() => history.back()),
    forward: Effect.sync(() => history.forward()),
  } satisfies AppRouter;
});
```
