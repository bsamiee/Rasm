# [PLATFORM_ROUTER]

One page owns the browser client routing infrastructure — `AppRouter`, the Effect-native router over one `Schema.Literal` route-key axis with a per-route behavior `Record`, the active `Location` and pending transition held in one `SubscriptionRef`, and the typed transitions, and `RouteParamCodec`, the per-route `Schema` round-trip composing the path segments with the `nuqs` query-state codec. The ingress is the Baseline Navigation API: one `navigate`-event interception over `window.navigation` lifted into an Effect `Stream` through a scoped `addEventListener`/`removeEventListener` resource owns link clicks, form submits, back/forward, and programmatic navigation in one ingress, and `event.intercept` with its scroll option owns scroll restoration natively. `viewTransition` is the one transition combinator wrapping the guard-admitted location commit as a scoped `document.startViewTransition` Effect resource with a reduced-motion gate degrading to an instant swap. The page admits ZERO routing package — `Schema`, `nuqs`, the Navigation API, the native View Transitions API, and the history `SubscriptionRef` own the concern; react-router/tanstack-router and an animation dependency are a deliberate no-admission. The page crosses no wire contract and authors no decode.

## [01]-[INDEX]

- [01]-[APP_ROUTER]: the route-key axis, the param codec, and the navigation ingress.
- [02]-[VIEW_TRANSITION]: the `document.startViewTransition` scoped fold around the location commit with a reduced-motion gate.

## [02]-[APP_ROUTER]

- Owner: `AppRouter`, the single client router — one `Schema.Literal` route-key axis with a behavior `Record` carrying each route's segment template, param codec, guard policy, and default params (the data-table source the redirect resolution reads, never an inline `{}` cast), the active `Location` and pending transition held in one `SubscriptionRef`, and `push`/`replace`/`back`/`forward` as typed `Effect` transitions; and `RouteParamCodec`, the per-route `Schema` round-trip composing the path-segment decode with the `nuqs` query-state codec into one `Location`. The route-key literal is the one routing vocabulary and a parallel route-string constant is the named const-spam defect.
- Cases: the route-key axis is one `Schema.Literal` union (`evidence-timeline`/`benchmark`/`collector`/`geo-series`/`viewport`/`login`/`not-found`) with a companion `RouteBehavior` `Record` keyed by the literal, so a new route is one literal and one `Record` row, never a parallel route table; `AppRouter` binds the Navigation API `navigate` event on `window.navigation` through a scoped `Stream.asyncScoped` `addEventListener`/`removeEventListener` resource — `fromEventListenerWindow` is the rejected ingress here because the `navigate` event fires on the `Navigation` interface, not on `window`, and is absent from `WindowEventMap` — so link clicks, form submits, the back/forward gesture, and the programmatic transition resolve through one interceptable ingress rather than a split between `popstate` and manual `pushState`; route resolution parses the `NavigateEvent` destination URL against the active route's segment template and `RouteParamCodec` decodes the path segments and the `nuqs` query state into the typed `RouteParams` for that key in one `Schema.decodeUnknown` pass, an undecodable URL folding to the `not-found` resolution rather than throwing; each admitted navigation calls `event.intercept` with its handler so the URL commit and the active-location set stay one fact, and the intercept scroll option owns scroll restoration natively so the back/forward gesture restores its offset without a hand-maintained scroll-offset map; the pending-navigation transition cell holds the in-flight target so a guard awaiting an `AvailabilityStore` read renders a pending affordance rather than a torn location, and the `NavigateEvent` `signal` owns the pending/abort transition state natively.
- Auto: `RouteParamCodec` is the per-route `Schema` round-trip — `decode` reads the path segments against the route's segment template and the `nuqs` query-state codec into the typed `RouteParams`, `encode` produces the pathname and search string back, so the active location and the URL stay one fact through one codec, never a hand-written URL string concatenation.
- Packages: `effect` for the `Schema.Literal` route axis, the `SubscriptionRef` location/pending cell, the `Schema` param round-trip, and the `Stream.asyncScoped`/`Effect.acquireRelease` scoped ingress; `nuqs` for the query-string state codec composed inside `RouteParamCodec`; the native Navigation API (`window.navigation`) for the `navigate` ingress, the interception, the scroll restoration, and the transition signal. ZERO router package — `react-router`/`tanstack-router`/`history` are a deliberate no-admission recorded at the folder README registry.
- Growth: a new route lands as one literal on the route-key axis and one `RouteBehavior` `Record` row carrying its segment template, param codec, guard policy, and default params; a new query-state key lands as one `nuqs`-composed field on that route's `RouteParamCodec`; the View Transitions API wrapper is the `[3]-[VIEW_TRANSITION]` `viewTransition` combinator folded into the same navigation pipeline, never an animation dependency.
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
          handler: () =>
            Runtime.runPromise(runtime)(
              viewTransition(commit(behavior, guard, location, pending, new URL(event.destination.url))),
            ),
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

const AppRouter = Effect.Tag("@rasm/ts/platform/AppRouter")<AppRouter, AppRouter>();
const AppRouterLive: Layer.Layer<AppRouter, never, AuthSession | AvailabilityStore> = Layer.scoped(AppRouter, makeAppRouter);

export { type Location, type RouteKey, type RouteParams, AppRouter, AppRouterLive };
```

## [03]-[VIEW_TRANSITION]

- Owner: `viewTransition`, the one route-transition combinator — an `<E>`-polymorphic arrow over the guard-admitted location-commit `Effect<void, E>`, wrapping it as a scoped `document.startViewTransition` resource with the start/ready/finished lifecycle owned by one `Effect.acquireRelease` and the reduced-motion gate degrading to an instant commit; the carrier is the parameter so the same body wraps every commit, never a copy per call site. The combinator is the single transition owner and a `document.startViewTransition` call hand-wired at the intercept handler is the named scattered-transition defect.
- Cases: `viewTransition` reads the `prefers-reduced-motion: reduce` media query once through `window.matchMedia`, so a reduced-motion user runs the commit with no transition while a default user wraps it — the commit `Effect` is the `ViewTransitionUpdateCallback` body `startViewTransition` invokes, the DOM swap and the active-location set staying one fact inside the callback; the `ViewTransition` `updateCallbackDone`/`ready`/`finished` promises convert at the boundary through `Effect.tryPromise`, the `ready` await is the GPU-composited animation start the navigation-guard pending affordance covers, and the `finished` await is the scoped resource's release so a `NavigateEvent` `signal` abort interrupting the fiber calls `ViewTransition.skipTransition()` exactly once on scope exit; the combinator stays generic in the carrier so the same body wraps the `Admit` commit, a `Redirect` re-commit, and any later guard-admitted swap with zero per-call duplication.
- Auto: the transition is one `Effect.acquireRelease` — acquire runs `document.startViewTransition(() => Runtime.runPromise(runtime)(commit))` capturing the `ViewTransition` handle (the `BOUNDARY ADAPTER` callback is the platform-forced statement seam wiring the foreign update callback into the Effect runtime), the use phase awaits `ready` through `Effect.tryPromise` so the pending cell clears when the pseudo-element tree is built, and release calls `skipTransition()` then awaits `finished` so an interrupted transition tears down cleanly; the reduced-motion branch skips `startViewTransition` entirely and runs the commit `Effect` directly, so the gate is one `matchMedia` row rather than scattered media-query checks in `ui` components.
- Packages: native `document.startViewTransition` (the `ViewTransition` handle carrying `ready`/`finished`/`updateCallbackDone`/`skipTransition`) and `window.matchMedia` for the reduced-motion gate; `effect` `Effect.acquireRelease` for the scoped transition lifetime, `Effect.tryPromise` for the `ready`/`finished` promise conversion, and `Runtime.runPromise` for the callback-boundary commit dispatch. ZERO animation dependency.
- Growth: a transition `types` set lands as one `StartViewTransitionOptions.types` row on the `viewTransition` call, never a parallel transition family; a per-route transition policy lands as one column on the `RouteBehavior` `Record` read inside the gate, never a second combinator; a cross-document transition leg lands as one branch over the same `startViewTransition` seam, the commit body unchanged.
- Boundary: `viewTransition` wraps only the guard-admitted commit inside the `navigate`-event intercept handler and composes with the `navigation-guard.md` `Pending` outcome through the one navigation pipeline, so a `document.startViewTransition` call authored outside this combinator is the named defect; the reduced-motion gate is the one accessibility row and a media-query check in a `ui` component is the named scattered-gate defect; `viewTransition` mutates no cell beyond the commit it wraps, emits no command, and dials no transport.

```ts contract
// --- [OPERATIONS] ----------------------------------------------------------------------
const prefersReducedMotion = (): boolean => window.matchMedia("(prefers-reduced-motion: reduce)").matches;

const viewTransition = <E>(commitEffect: Effect.Effect<void, E>): Effect.Effect<void, E> =>
  Effect.runtime<never>().pipe(
    Effect.flatMap((runtime) =>
      prefersReducedMotion()
        ? commitEffect
        : Effect.acquireRelease(
            Effect.sync(() =>
              document.startViewTransition(() => Runtime.runPromise(runtime)(commitEffect)), // BOUNDARY ADAPTER: native update callback into the runtime
            ),
            (transition) =>
              Effect.sync(() => transition.skipTransition()).pipe(
                Effect.zipRight(Effect.promise(() => transition.finished)),
              ),
          ).pipe(
            Effect.tap((transition) => Effect.promise(() => transition.updateCallbackDone)),
            Effect.tap((transition) => Effect.promise(() => transition.ready)),
            Effect.asVoid,
          ),
    ),
  );
```
