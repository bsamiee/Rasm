# [BROWSER_RUNTIME]

`boot/runtime.ts` is the single-boot law and the budget vocabulary: one `BrowserRuntime.runMain` call boots the document — a second boot in the same document is the named defect, and a worker entry is its own thread's boot under the same law — and `AppSpec` is the budget VALUE every app constructs before anything runs: the kernel `AppIdentity` it boots as, the feed rows it will drive, and the numeric ceilings the folder's layer factories consume at composition. The closed five-feed Rasm budget (`wire.WireClients`, `wire.CommandGateway`, `store.SnapshotFeed`, `store.RuntimeFeed`, `store.EvidenceFeed`) is product guidance an app encodes as five feed rows — never lib law. The render posture is law here: client-rendered PWA plus build-time prerender rows own the SEO surface — per-route static HTML emitted at app build, detected and handed to the app's mount by `Boot.hydrated` — and a streaming-SSR react server runtime is the named non-goal. An app is a ~30-line `main.ts`: construct the spec, merge the selected Layer families, call `Boot.main` once.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                | [PUBLIC]  |
| :-----: | :------------ | :---------------------------------------------------------------------- | :-------- |
|  [01]   | `BUDGET_VALUE`| the `AppSpec` shape — identity, feed rows, ceilings                     | `AppSpec` |
|  [02]   | `SINGLE_BOOT` | the one `runMain` seam, the spec Tag, the hydration detect              | `Boot`    |

## [2]-[BUDGET_VALUE]

[BUDGET_VALUE]:
- Owner: `AppSpec`, one `Schema.Class` — `identity` (the kernel `AppIdentity` composed as a field at full depth, so app key, tenancy, build, and host fingerprint arrive proven), `feeds` (the app's feed rows: a name plus the `host` client lane it dials on — the budget is countable, named, and lane-typed), and `ceilings` (the numeric budget the composition root threads into factories: `workers` into `Pool.layer`, `settle` into `Guard.Default`, `outbox` as the soft depth the app's relay policy honors).
- Law: the lane axis is `host`'s, governed both ways — the interior tuple `satisfies` `Client.Lane` and the `_Spans` guard demands full coverage, so a host lane rename or addition breaks this anchor at compile time, never as a stale budget row.
- Law: the spec is constructed once from validated material — `host/config`'s provider chain resolves the identity at boot and the app assembles the value; nothing downstream re-reads an environment, and the spec's decode is the one admission of the budget.
- Law: consumers read rows, never re-declare knobs — `Pool.layer(spawn, spec.ceilings.workers)`, `Guard.Default({ settle: spec.ceilings.settle, ... })`: the composition root threads spec rows into factories, so retuning an app is editing its spec value with zero lib edits, the acceptance gate's mechanical form.
- Law: `telemetry`'s OTel `Resource` and this boot stamp derive from the SAME `AppIdentity` value — one identity spine, so hundreds of apps emit and boot through one vocabulary and a per-app identity fork is structurally impossible.
- Growth: a new budget axis is one `ceilings` field; a new feed fact is one field on the feed row.
- Boundary: what each feed DOES is the owning folder's law (`wire` gateways, `store` feeds); this owner counts and types them.
- Packages: `@rasm/ts/kernel` (`AppIdentity`); `@rasm/ts/host` (type `Client`); `effect` (`Schema`).

```typescript
import { BrowserRuntime } from "@effect/platform-browser"
import type { Client } from "@rasm/ts/host"
import { AppIdentity } from "@rasm/ts/kernel"
import { Context, Effect, Layer, Option, Schema } from "effect"

const _LANES = ["live", "batch", "feed"] as const satisfies ReadonlyArray<Client.Lane>

const _Feed = Schema.Struct({
  name: Schema.NonEmptyString,
  lane: Schema.Literal(..._LANES),
})

class AppSpec extends Schema.Class<AppSpec>("AppSpec")({
  identity: AppIdentity,
  feeds: Schema.Array(_Feed),
  ceilings: Schema.Struct({
    workers: Schema.Int.pipe(Schema.between(1, 16)),
    outbox: Schema.Int.pipe(Schema.positive()),
    settle: Schema.Duration,
  }),
}) {
  get label(): string {
    return `${this.identity.label}#${this.feeds.length}`
  }
}

declare namespace AppSpec {
  type Feed = Schema.Schema.Type<typeof _Feed>
  type Lane = (typeof _LANES)[number]
  type _Spans<K extends Lane = Client.Lane> = K
}
```

## [3]-[SINGLE_BOOT]

[SINGLE_BOOT]:
- Owner: `Boot`, one `Context.Tag` class — the Tag itself is the spec slot, so a service that earns a budget read writes `yield* Boot` with zero second hop; `Boot.main(spec, root, app)`, the one `BrowserRuntime.runMain` seam riding it as a static: the app effect annotated with the identity stamp, the spec provided beneath the app-selected root, and the requirement channel pinned to `never` at this line — an unwired Tag fails here at compile time, the wiring proof; `Boot.hydrated`, the prerender handoff read.
- Law: `main` is called exactly once per document, from the app's `main.ts`, and that module exports nothing — the empty surface is the structural proof it is terminal; every other module in the branch is barred from any `run*` call, and the pool's worker entry is the one sibling boot, its own thread's.
- Law: the boot line is the only imperative seam — `runMain` installs error reporting and teardown wiring; `disableErrorReporting`/`disablePrettyLogger` stay default because crash visibility is a telemetry concern composed as Layers, never a boot flag.
- Law: hydration is boot's law — the build emits per-route static HTML stamped with the `data-rasm-prerender` marker; `Boot.hydrated` reads the marker (`Option`-carried) so the app's mount takes over a prerendered document instead of re-rendering it, and a document without the marker is a cold client render; the marker read is this module's one DOM touch.
- Receipt: `main` returns `void` — everything observable thereafter flows through the composed graph; the annotation on `main`'s signature is the whole boot contract.
- Boundary: which Layer families merge into `root` is the app's selection across the thirteen folders; the runMain mechanics are `@effect/platform-browser`'s; view mounting is `ui`'s behind its atom bridge.
- Packages: `@effect/platform-browser` (`BrowserRuntime`); `effect` (`Layer`, `Effect`, `Context`, `Option`).

```typescript
class Boot extends Context.Tag("browser/boot/AppSpec")<Boot, AppSpec>() {
  static readonly hydrated: Effect.Effect<Option.Option<string>> = Effect.sync(() =>
    Option.fromNullable(globalThis.document.documentElement.getAttribute("data-rasm-prerender")),
  )
  static readonly main = <A, E, R, E2>(spec: AppSpec, root: Layer.Layer<R, E2>, app: Effect.Effect<A, E, R | Boot>): void =>
    BrowserRuntime.runMain(
      app.pipe(
        Effect.annotateLogs({ app: spec.label }),
        Effect.provide(Layer.mergeAll(root, Layer.succeed(Boot, spec))),
      ),
    )
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AppSpec, Boot }
```
