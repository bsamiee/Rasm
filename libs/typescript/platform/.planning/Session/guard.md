# [PLATFORM_GUARD]

One page owns the route-admission fold — `NavigationGuard`, the total fold gating a route on the `identity-session` `AuthSession` status and the `projection` `AvailabilityStore`, returning a `GuardOutcome` `Data.TaggedEnum` of `Admit`/`Redirect`/`Pending`. The guard is one fold over two read cells, never an imperative `if (!user) navigate(...)` scattered across components; the guard read never mutates the session or the availability fold.

## [1]-[INDEX]

- [1]-[NAVIGATION_GUARD]: the route-admission fold over auth and availability.

## [2]-[NAVIGATION_GUARD]

- Owner: `NavigationGuard`, the admission fold over `AuthSession.status` and the `projection` `AvailabilityStore`.
- Cases: `NavigationGuard` resolves as a total fold — it reads `AuthSession.status` and, for an availability-gated route, the matching `projection` `AvailabilityStore` row, and returns a `GuardOutcome` `Data.TaggedEnum` (`Admit`/`Redirect`/`Pending`); an `Anonymous` or `Expired` session on a protected route folds to `GuardOutcome.Redirect({ to: "login" })` and a route whose command is unavailable folds to `GuardOutcome.Pending` against the live availability cell, so the guard is one fold over two read cells, never an imperative `if (!user) navigate(...)` scattered across components and never a hand-listed `_tag` literal union the `AppRouter` `$match` cannot prove total.
- Auto: the guard read is pure — it never mutates the session or the availability fold; the `AppRouter` transition consumes the `GuardOutcome` and the Navigation API `event.intercept` handler awaits a `pending` outcome as the in-flight transition the `signal` can abort.
- Packages: `effect` `Match` for the guard fold and the `Effect` read of the two cells.
- Growth: a new guard condition lands as one arm on the `NavigationGuard` fold, never a parallel guard.
- Boundary: the guard reads the `identity-session` `AuthSession` and the `projection` `AvailabilityStore` and emits no command; navigation triggers an intent only through the `interchange` `CommandGateway` when a route transition carries one; the guard authors no decode and dials no transport; `ui` never imports it.

```ts contract
type GuardOutcome = Data.TaggedEnum<{
  readonly Admit: object;
  readonly Redirect: { readonly to: RouteKey };
  readonly Pending: { readonly reason: string };
}>;
const GuardOutcome = Data.taggedEnum<GuardOutcome>();

interface NavigationGuard {
  readonly resolve: (key: RouteKey) => Effect.Effect<GuardOutcome, never, AuthSession | AvailabilityStore>;
}
```
