# [HOST_PROVIDER]

The provider chain is one composed value answering every `Config` read in the process: an ordered, non-empty tuple of source stages — process env (which is also where `doppler run` injection lands), dotenv file, file tree, remote document, literal table — folded left through `ConfigProvider.orElse` into one `ConfigProvider` and installed once as `Layer.setConfigProvider` beneath the whole graph. Precedence is the tuple order: the head answers first, each later stage answers only what every earlier stage missed. A source is a case of one closed `Stage` family, so a new source is one case plus one build arm, and the chain itself is app data — the host owns the fold, never a hardcoded source roster. `schema.md`'s `Setting` and every folder contract resolve against whatever chain the root installed; no reader knows or names its source.

## [1]-[INDEX]

- [01]-[STAGE_FAMILY]: the closed source vocabulary and the doppler-injection law.
- [02]-[CHAIN_FOLD]: the orElse fold, the skip-versus-fail construction law, and the one `Layer.setConfigProvider` install.

## [2]-[STAGE_FAMILY]

- Owner: `Provider.Stage` — one `Data.taggedEnum` family: `Env` (process environment), `DotEnv` carrying its file path, `Tree` carrying the mount root (the K8s secret-mount form — one file per key), `Remote` carrying the document origin, `Table` carrying a literal row map (the harness and inline-override form). Constructors ride the exported `Provider` owner, so declaring a chain is one import.
- Law: doppler is env — `doppler run --` injects leased secrets as process environment before the runtime boots, so the doppler stage IS the `Env` stage's content and holds env precedence; the chain never dials Doppler at runtime. The runtime Doppler axis — TTL-leased rotation, `Redacted` end to end — is `security/secret`'s owner; this chain's doppler posture is injection-only.
- Law: the remote stage is a boot-time document — fetched once at chain construction through the branch `batch` lane (`../net/client.md`), so the chain inherits the same egress posture as every other outbound call, decoded as one JSON document, and served through `ConfigProvider.fromJson` under `ConfigProvider.constantCase` so a camelCase remote document answers the CONSTANT_CASE reads the rows declare; live re-evaluation is not a config concern — the one live remote surface is the flag feed (`../flag/verdict.md`), never a mutating config chain.
- Law: a stage carries data, not behavior — path, root, origin, rows are case payloads; the build arm owns the mechanics, so the chain declaration reads as policy.
- Boundary: proof pins ride the same family — `Table` under `ConfigProvider.fromMap` is the harness stage, composed at the head of a spec chain; a second override mechanism beside the family is the rejected form.
- Packages: `effect` (`ConfigProvider`, `Data`), `@effect/platform` (`PlatformConfigProvider`, `HttpClientRequest`, `FileSystem`, `Path`), `../net/client.ts` (`Client`).

## [3]-[CHAIN_FOLD]

- Owner: `Provider.chain` — the one entry: a `NonEmptyReadonlyArray<Stage>` folds to `Layer.Layer<never, …>` installing the composed provider. Each stage builds effectfully; the fold is `Array.reduce` over `ConfigProvider.orElse` with the head as seed, so precedence is structural and the empty chain is unspellable.
- Law: construction failure splits by stage nature — the file stages (`DotEnv`, `Tree`) fold construction absence to a skipped stage through `Effect.option`, because a dev-only file legitimately does not exist in prod; the `Remote` stage was declared to be answered, so its fetch or decode failure rides the layer's error channel and fails the boot at the root proof. `Env` and `Table` are total. A chain whose every declared stage skipped folds to the empty provider — every read fails as missing data at the boot proof, never a forged env fallback the chain never declared.
- Law: one install site — the returned `Layer<never>` merges once beneath the root; a second `setConfigProvider` at a deeper altitude shadows the root's chain and is the named defect. Requirements (`FileSystem`, `Path` for file stages; `HttpClient` for the remote dial) are satisfied by the runtime row's context and the shared client, so the chain layer composes after the platform layer and before every config-reading service.
- Entry: `Provider.chain(stages)` at the app root; nothing else in the branch touches `ConfigProvider`.
- Receipt: the layer's stated annotation is the chain's contract — `Layer.Layer<never, HttpClientError | Lapse | ParseError, HttpClient | FileSystem | Path>` — readable at the root without opening the fold.
- Packages: `effect` (`Array`, `Effect`, `Layer`, `Option`, `Schema`), `@effect/platform` and `../net/client.ts` as above.

```typescript
import { type FileSystem, type HttpClient, type HttpClientError, HttpClientRequest, type Path, PlatformConfigProvider } from "@effect/platform"
import { Array, ConfigProvider, Data, Effect, Layer, Option, type ParseResult, Schema, pipe } from "effect"
import { Client, type Lapse } from "../net/client.ts"

declare namespace Provider {
  type Stage = Data.TaggedEnum<{
    Env: {}
    DotEnv: { readonly path: string }
    Tree: { readonly root: string }
    Remote: { readonly origin: URL }
    Table: { readonly rows: ReadonlyMap<string, string> }
  }>
  type Faults = HttpClientError.HttpClientError | Lapse | ParseResult.ParseError
  type Needs = FileSystem.FileSystem | HttpClient.HttpClient | Path.Path
}

const _Stage = Data.taggedEnum<Provider.Stage>()

const _fetched = (origin: URL): Effect.Effect<ConfigProvider.ConfigProvider, Provider.Faults, HttpClient.HttpClient> =>
  Effect.map(
    Client.dial("batch", HttpClientRequest.get(origin.href), Schema.Unknown),
    (document) => ConfigProvider.constantCase(ConfigProvider.fromJson(document)),
  )

const _built = (stage: Provider.Stage): Effect.Effect<Option.Option<ConfigProvider.ConfigProvider>, Provider.Faults, Provider.Needs> =>
  _Stage.$match(stage, {
    Env: () => Effect.succeed(Option.some(ConfigProvider.fromEnv())),
    DotEnv: ({ path }) => Effect.option(PlatformConfigProvider.fromDotEnv(path)),
    Tree: ({ root }) => Effect.option(PlatformConfigProvider.fromFileTree({ rootDirectory: root })),
    Remote: ({ origin }) => Effect.map(_fetched(origin), Option.some),
    Table: ({ rows }) => Effect.succeed(Option.some(ConfigProvider.fromMap(rows))),
  })

const _folded = (
  stages: Array.NonEmptyReadonlyArray<Provider.Stage>,
): Effect.Effect<ConfigProvider.ConfigProvider, Provider.Faults, Provider.Needs> =>
  Effect.map(
    Effect.forEach(stages, _built),
    (built) =>
      pipe(
        Array.getSomes(built),
        (chain) =>
          Array.isNonEmptyReadonlyArray(chain)
            ? Array.reduce(Array.tailNonEmpty(chain), Array.headNonEmpty(chain), (acc, next) =>
                ConfigProvider.orElse(acc, () => next))
            : ConfigProvider.fromMap(new Map()),
      ),
  )

const Provider: Data.TaggedEnum.Constructor<Provider.Stage> & {
  readonly chain: (stages: Array.NonEmptyReadonlyArray<Provider.Stage>) => Layer.Layer<never, Provider.Faults, Provider.Needs>
} = {
  ..._Stage,
  chain: (stages) => Layer.unwrapEffect(Effect.map(_folded(stages), Layer.setConfigProvider)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Provider }
```
