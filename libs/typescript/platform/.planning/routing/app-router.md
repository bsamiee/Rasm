# [PLATFORM_APP_ROUTER]

One page owns the browser client routing infrastructure — `AppRouter`, the Effect-native router over one `Schema.Literal` route-key axis with a per-route behavior `Record`, the active `Location` and pending transition held in one `SubscriptionRef`, and the typed transitions, and `RouteParamCodec`, the per-route `Schema` round-trip composing the path segments with the `nuqs` query-state codec. The ingress is the Baseline Navigation API: one `navigate`-event interception over `window.navigation` lifted into an Effect `Stream` through a scoped `addEventListener`/`removeEventListener` resource owns link clicks, form submits, back/forward, and programmatic navigation in one ingress, and `event.intercept` with its scroll option owns scroll restoration natively. The page admits ZERO routing package — `Schema`, `nuqs`, the Navigation API, and the history `SubscriptionRef` own the concern; react-router/tanstack-router are a deliberate no-admission. The page crosses no wire contract and authors no decode.

## [1]-[INDEX]

[APP_ROUTER]: the route-key axis, the param codec, and the navigation ingress.

## [2]-[APP_ROUTER]

- Owner: `AppRouter`, the single client router — one `Schema.Literal` route-key axis with a behavior `Record` carrying each route's segment template, param codec, guard policy, and default params (the data-table source the redirect resolution reads, never an inline `{}` cast), the active `Location` and pending transition held in one `SubscriptionRef`, and `push`/`replace`/`back`/`forward` as typed `Effect` transitions; and `RouteParamCodec`, the per-route `Schema` round-trip composing the path-segment decode with the `nuqs` query-state codec into one `Location`. The route-key literal is the one routing vocabulary and a parallel route-string constant is the named const-spam defect.
- Cases: the route-key axis is one `Schema.Literal` union (`evidence-timeline`/`benchmark`/`collector`/`geo-series`/`viewport`/`login`/`not-found`) with a companion `RouteBehavior` `Record` keyed by the literal, so a new route is one literal and one `Record` row, never a parallel route table; `AppRouter` binds the Navigation API `navigate` event on `window.navigation` through a scoped `Stream.asyncScoped` `addEventListener`/`removeEventListener` resource — `fromEventListenerWindow` is the rejected ingress here because the `navigate` event fires on the `Navigation` interface, not on `window`, and is absent from `WindowEventMap` — so link clicks, form submits, the back/forward gesture, and the programmatic transition resolve through one interceptable ingress rather than a split between `popstate` and manual `pushState`; route resolution parses the `NavigateEvent` destination URL against the active route's segment template and `RouteParamCodec` decodes the path segments and the `nuqs` query state into the typed `RouteParams` for that key in one `Schema.decodeUnknown` pass, an undecodable URL folding to the `not-found` resolution rather than throwing; each admitted navigation calls `event.intercept` with its handler so the URL commit and the active-location set stay one fact, and the intercept scroll option owns scroll restoration natively so the back/forward gesture restores its offset without a hand-maintained scroll-offset map; the pending-navigation transition cell holds the in-flight target so a guard awaiting an `AvailabilityStore` read renders a pending affordance rather than a torn location, and the `NavigateEvent` `signal` owns the pending/abort transition state natively.
- Auto: `RouteParamCodec` is the per-route `Schema` round-trip — `decode` reads the path segments against the route's segment template and the `nuqs` query-state codec into the typed `RouteParams`, `encode` produces the pathname and search string back, so the active location and the URL stay one fact through one codec, never a hand-written URL string concatenation.
- Packages: `effect` for the `Schema.Literal` route axis, the `SubscriptionRef` location/pending cell, the `Schema` param round-trip, and the `Stream.asyncScoped`/`Effect.acquireRelease` scoped ingress; `nuqs` for the query-string state codec composed inside `RouteParamCodec`; the native Navigation API (`window.navigation`) for the `navigate` ingress, the interception, the scroll restoration, and the transition signal. ZERO router package — `react-router`/`tanstack-router`/`history` are a deliberate no-admission recorded at the folder README registry.
- Growth: a new route lands as one literal on the route-key axis and one `RouteBehavior` `Record` row carrying its segment template, param codec, guard policy, and default params; a new query-state key lands as one `nuqs`-composed field on that route's `RouteParamCodec`; the View Transitions API wrapper (`document.startViewTransition` around the admitted location commit, a scoped Effect resource with a reduced-motion gate) lands as one transition combinator folded into the same navigation pipeline, never an animation dependency.
- Boundary: `AppRouter` is the single routing owner and a parallel route-string constant or a second navigation surface is the named defect; route-resident state lives in the location `SubscriptionRef` and the query string, surfaced to a component only through the `ui` `AtomBinding`, and a route value copied into local component state is the named duplication defect; the router never dials a transport and authors no decode — a route transition triggers an intent only through the `interchange` `CommandGateway` when it carries one; `platform` composes the router into the SPA entry and `ui` never imports it.

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

interface AppRouter {
  readonly location: SubscriptionRef.SubscriptionRef<Location>;
  readonly pending: SubscriptionRef.SubscriptionRef<Option.Option<Location>>;
  readonly resolve: (url: string) => Effect.Effect<Location>;
  readonly push: (target: Location) => Effect.Effect<void, ParseResult.ParseError, AuthSession | AvailabilityStore>;
  readonly replace: (target: Location) => Effect.Effect<void, ParseResult.ParseError, AuthSession | AvailabilityStore>;
  readonly back: Effect.Effect<void>;
  readonly forward: Effect.Effect<void>;
  readonly start: Effect.Effect<void, never, Scope.Scope | AuthSession | AvailabilityStore>;
}

const navigateEvents: Stream.Stream<NavigateEvent, never, Scope.Scope> = Stream.asyncScoped<NavigateEvent>((emit) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const fn = (event: NavigateEvent): void => emit.single(event);
      window.navigation.addEventListener("navigate", fn);
      return fn;
    }),
    (fn) => Effect.sync(() => window.navigation.removeEventListener("navigate", fn)),
  ));

const makeAppRouter: Effect.Effect<AppRouter, never, Scope.Scope | AuthSession | AvailabilityStore> = Effect.gen(function* () {
  const behavior = routeBehaviorTable;
  const guard = makeNavigationGuard(behavior);
  const runtime = yield* Effect.runtime<AuthSession | AvailabilityStore>();
  const initial = yield* resolveUrl(behavior, window.location.pathname + window.location.search);
  const location = yield* SubscriptionRef.make<Location>(initial);
  const pending = yield* SubscriptionRef.make<Option.Option<Location>>(Option.none());
  const start = navigateEvents.pipe(
    Stream.filter((event: NavigateEvent) => event.canIntercept && !event.hashChange),
    Stream.mapEffect((event: NavigateEvent) =>
      Effect.sync(() =>
        event.intercept({
          scroll: "after-transition",
          handler: () => Runtime.runPromise(runtime)(commit(behavior, guard, location, pending, new URL(event.destination.url))),
        }),
      ),
    ),
    Stream.runDrain,
    Effect.forkScoped,
    Effect.asVoid,
  );
  const transition = (mode: "push" | "replace") =>
    (target: Location): Effect.Effect<void, ParseResult.ParseError, AuthSession | AvailabilityStore> =>
      guard.resolve(target.key).pipe(
        Effect.flatMap((outcome) =>
          Match.value(outcome).pipe(
            Match.tagsExhaustive({
              Admit: () =>
                behavior[target.key].codec.encode(target.params).pipe(
                  Effect.tap(({ pathname, search }) =>
                    Effect.sync(() => window.navigation.navigate(pathname + search, { history: mode }))),
                  Effect.asVoid,
                ),
              Redirect: (redirect) =>
                transition("replace")({ key: redirect.to, params: behavior[redirect.to].defaultParams } as Location),
              Pending: () => SubscriptionRef.set(pending, Option.some(target)),
            }),
          )));
  return {
    location,
    pending,
    resolve: (url) => resolveUrl(behavior, url),
    push: transition("push"),
    replace: transition("replace"),
    back: Effect.sync(() => window.navigation.back()),
    forward: Effect.sync(() => window.navigation.forward()),
    start,
  } satisfies AppRouter;
});
```
