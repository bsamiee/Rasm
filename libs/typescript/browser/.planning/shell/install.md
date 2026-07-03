# [BROWSER_INSTALL]

`shell/install.ts` owns the PWA installability plane: the web-app manifest as a typed VALUE the app constructs and the build encodes — never a hand-authored JSON asset — and the install/update affordance state as one owned cell. `Manifest` is a `Schema.Class` whose encoded side carries the manifest dialect's snake-case spellings through field renames, so the interior speaks canonical names and `Manifest.json` emits the exact `.webmanifest` byte contract; `Install` captures the nonstandard `beforeinstallprompt` at the one sanctioned listener (the deferral call is synchronous or the prompt is lost), folds installability into an `InstallStance` cell — browser tab, prompt-ready, installed, running standalone — and surfaces the update affordance by deriving `shell/worker`'s `Waiting` phase into a boolean feed beside the one `refresh` verb. A second prompt listener, an untyped manifest asset, or an update affordance reading `navigator.serviceWorker` directly is the named defect.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                            | [PUBLIC]                  |
| :-----: | :--------------- | :------------------------------------------------------------------ | :------------------------ |
|  [01]   | `MANIFEST_VALUE` | the typed manifest owner and its emitted wire twin                  | `Manifest`                |
|  [02]   | `INSTALL_OWNER`  | the prompt capture, the stance cell, the ask/refresh affordances    | `Install`, `InstallFault` |

## [2]-[MANIFEST_VALUE]

[MANIFEST_VALUE]:
- Owner: `Manifest`, one `Schema.Class` — `name`, `shortName`, `description`, `startUrl`, `scope`, `display` (the closed display-mode literal), `themeColor`/`backgroundColor` (`Option`-carried), and `icons` as a `Schema.NonEmptyArray` of icon rows — with every dialect spelling divergence handled as a field-level `Schema.fromKey` rename, so `shortName` encodes as `short_name`, `startUrl` as `start_url`, and no parallel wire interface exists.
- Law: the manifest is app DATA constructed at build — the app builds a `Manifest` value from its own identity and `Manifest.json` (the fused `Schema.parseJson` twin riding the owner as a static) encodes it to the `.webmanifest` string the build emits beside the precache; hydration and serving are the app build's, the shape law is this owner's.
- Law: non-emptiness is a type fact — an installable manifest without icons is unconstructible, so the PWA install criteria fail at compile time, never at a lighthouse audit.
- Growth: a new manifest member (screenshots, shortcuts, share_target) is one field row with its rename; a new display mode is one literal on the axis.
- Boundary: cache identity and precache emission are `shell/worker`'s build rows; this owner carries only the manifest contract.
- Packages: `effect` (`Schema`, `Option`).

```typescript
import { Data, Effect, Option, Schema, Stream, SubscriptionRef } from "effect"
import { Sw, SwLifecycle } from "./worker.ts"

const _Icon = Schema.Struct({
  src: Schema.NonEmptyString,
  sizes: Schema.NonEmptyString,
  type: Schema.NonEmptyString,
  purpose: Schema.optionalWith(Schema.Literal("any", "maskable", "monochrome"), { as: "Option" }),
})

class Manifest extends Schema.Class<Manifest>("Manifest")({
  name: Schema.NonEmptyString,
  shortName: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("short_name")),
  description: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  startUrl: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("start_url")),
  scope: Schema.NonEmptyString,
  display: Schema.Literal("standalone", "browser", "minimal-ui", "fullscreen"),
  themeColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }).pipe(Schema.fromKey("theme_color")),
  backgroundColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }).pipe(Schema.fromKey("background_color")),
  icons: Schema.NonEmptyArray(_Icon),
}) {
  static readonly json: Schema.SchemaClass<Manifest, string> = Schema.parseJson(Manifest)
}
```

## [3]-[INSTALL_OWNER]

[INSTALL_OWNER]:
- Owner: `Install`, one scoped `Effect.Service` over `Sw` — `stance`, the `InstallStance` cell (`Browser`/`Ready`/`Installed`/`Standalone`); `ask`, the install affordance firing the captured prompt and folding the user's choice; `fresh`, the update-available feed derived from the worker's `Waiting` phase; `refresh`, the one update verb delegating to `Sw.apply`.
- Law: the prompt capture is the module's platform-forced statement seam — `beforeinstallprompt` is nonstandard (absent from `WindowEventMap`, spelled here once as the `_PromptEvent` boundary refinement) and its `preventDefault` must run synchronously inside the native handler or the browser consumes the prompt; the capture bridge therefore attaches its own listener under `Stream.asyncScoped`, deferring and emitting in the same synchronous frame, and the implementer carries the `// BOUNDARY ADAPTER` mark on `_prompts`' first line.
- Law: the prompt is single-use — `ask` consumes the held prompt, folds `accepted` to `Installed` and `dismissed` back to `Browser`, and clears the slot either way; a second `ask` with no captured prompt is the typed `unavailable` fault, never a silent no-op.
- Law: stance is evidence-ordered — the standalone display-mode probe (seeded from `matchMedia`, advanced by its `change` events) and the `appinstalled` event both fold to their stance directly; running standalone is terminal for the session, so no later fold demotes it.
- Law: the update affordance is a derivation, not a state — `fresh` maps the worker phase feed through the `Waiting` refinement, so install and update read one truth and `ui` binds both through its atom bridge at app composition; this module exposes no second phase cell.
- Receipt: `ask` yields the fold's stance so the caller renders the outcome without re-reading the cell.
- Boundary: the worker phase and the apply handshake are `shell/worker`'s; the affordance rendering is `ui`'s through the app-composed port.
- Packages: `effect` (`Effect`, `Stream`, `SubscriptionRef`, `Data`, `Option`); `./worker.ts` (`Sw`, `SwLifecycle`).

```typescript
type InstallStance = Data.TaggedEnum<{
  Browser: {}
  Ready: {}
  Installed: {}
  Standalone: {}
}>
const InstallStance: Data.TaggedEnum.Constructor<InstallStance> = Data.taggedEnum<InstallStance>()

const InstallFaultPolicy = {
  unavailable: { rank: 2, retry: false },
  ceremony: { rank: 3, retry: false },
} as const

declare namespace InstallFault {
  type Reason = keyof typeof InstallFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof InstallFaultPolicy> = T
}

class InstallFault extends Data.TaggedError("InstallFault")<{
  readonly reason: InstallFault.Reason
  readonly detail: string
}> {
  get policy(): InstallFault.Row {
    return InstallFaultPolicy[this.reason]
  }
}

type _PromptEvent = Event & {
  readonly prompt: () => Promise<void>
  readonly userChoice: Promise<{ readonly outcome: "accepted" | "dismissed" }>
}

const _prompts: Stream.Stream<_PromptEvent> = Stream.asyncScoped((emit) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const handler = (event: Event) => {
        event.preventDefault()
        void emit.single(event as _PromptEvent)
      }
      globalThis.addEventListener("beforeinstallprompt", handler)
      return handler
    }),
    (handler) => Effect.sync(() => globalThis.removeEventListener("beforeinstallprompt", handler)),
  ),
)

class Install extends Effect.Service<Install>()("browser/shell/Install", {
  scoped: Effect.gen(function* () {
    const sw = yield* Sw
    const standalone = globalThis.matchMedia("(display-mode: standalone)")
    const stance = yield* SubscriptionRef.make<InstallStance>(
      standalone.matches ? InstallStance.Standalone() : InstallStance.Browser(),
    )
    const held = yield* SubscriptionRef.make(Option.none<_PromptEvent>())
    yield* _prompts.pipe(
      Stream.runForEach((prompt) =>
        SubscriptionRef.set(held, Option.some(prompt)).pipe(
          Effect.zipRight(SubscriptionRef.set(stance, InstallStance.Ready())),
        ),
      ),
      Effect.forkScoped,
    )
    yield* Stream.fromEventListener(globalThis, "appinstalled").pipe(
      Stream.runForEach(() => SubscriptionRef.set(stance, InstallStance.Installed())),
      Effect.forkScoped,
    )
    yield* Stream.fromEventListener(standalone, "change").pipe(
      Stream.runForEach(() =>
        standalone.matches ? SubscriptionRef.set(stance, InstallStance.Standalone()) : Effect.void,
      ),
      Effect.forkScoped,
    )
    const ask: Effect.Effect<InstallStance, InstallFault> = Effect.gen(function* () {
      const slot = yield* SubscriptionRef.get(held)
      const prompt = yield* Option.match(slot, {
        onNone: () => new InstallFault({ reason: "unavailable", detail: "<no-captured-prompt>" }),
        onSome: Effect.succeed,
      })
      yield* SubscriptionRef.set(held, Option.none())
      const choice = yield* Effect.tryPromise({
        try: () => prompt.prompt().then(() => prompt.userChoice),
        catch: (cause) => new InstallFault({ reason: "ceremony", detail: String(cause) }),
      })
      const landed = choice.outcome === "accepted" ? InstallStance.Installed() : InstallStance.Browser()
      yield* SubscriptionRef.set(stance, landed)
      return landed
    })
    return {
      stance,
      ask,
      fresh: Stream.map(sw.phase.changes, SwLifecycle.$is("Waiting")),
      refresh: sw.apply,
    }
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Install, InstallFault, InstallStance, Manifest }
```
