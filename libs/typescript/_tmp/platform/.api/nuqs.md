# [API_CATALOGUE] nuqs

`nuqs` is type-safe URL search-param state for React: `useQueryState`/`useQueryStates` bind React state to the URL; a `parseAs*` roster (all instances of ONE `createParser`/`createMultiParser` mechanism + the `.withDefault`/`.withOptions` builder chain) types each key; the React-free `createLoader`/`createSearchParamsCache`/`createSerializer` surface parses and renders URLs outside React; `createStandardSchemaV1` and `parseAsJson(StandardSchemaV1)` are the two-directional Standard-Schema bridge to Effect `Schema`; and the `nuqs/adapters/*` family is the REQUIRED router-binding seam the hooks resolve through. The `Options` type governs history/scroll/shallow/rate-limit/transition across every operation. In `platform`, `Session/router.md` `RouteParamCodec` composes the React-FREE parser+serializer surface inside an Effect codec (no adapter needed); the React hooks (consumed by `ui`) require a mounted adapter, and because `platform` runs the native Navigation API with ZERO router package the only consistent adapter is `nuqs/adapters/custom` bridging `updateUrl` to `window.navigation`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nuqs`
- package: `nuqs`
- version: `2.9.0`
- license: `MIT`
- runtime: `dual` — client hooks + adapters need React + DOM; `nuqs/server` (loader/cache/serializer/parsers) is runtime-neutral (React-free); `parseAs*` builders are pure and importable from either entry
- module: `nuqs` (hooks + parsers + `createParser`); `nuqs/server` (SSR/RSC + loader/cache/serializer/parsers); `nuqs/adapters/<host>` (REQUIRED router binding); `nuqs/testing` (parser round-trip helpers); `nuqs/debug` (localStorage debug logging)
- peer: `react >=18.2.0 || ^19.0.0-0` (only hard peer for hooks); framework peers (`next`, `react-router`, `@tanstack/react-router`, `@remix-run/react`) are OPTIONAL, selected by the adapter subpath; dep `@standard-schema/spec@1.0.0` (the `parseAsJson`/`createStandardSchemaV1` bridge type)
- side-effects: `["./dist/debug.js"]` only — the core is tree-shakeable; `nuqs/debug` is the one side-effecting import
- catalog-verdict: KEEP; the URL-search-param codec, admitted at `Session/router.md`; router/history packages (`react-router`/`tanstack-router`/`history`) stay NO-ADMISSION — the native Navigation API + `nuqs` custom adapter own routing

[SUBPATH_AND_ADAPTER_FAMILY]: the adapter is the parameterized host-binding axis — one `createAdapterProvider(useAdapter)` mechanism, one subpath per host; `platform` selects `custom`.

| [INDEX] | [SUBPATH]                          | [HOST]                          | [PLATFORM DISPOSITION]                                  |
| :-----: | :--------------------------------- | :------------------------------ | :----------------------------------------------------- |
|  [01]   | `nuqs/adapters/custom`             | bespoke router / native APIs    | ADMIT — bridge `updateUrl` to `window.navigation`      |
|  [02]   | `nuqs/adapters/react-router/v6\|v7\|v8` | React Router                | reject (router pkg NO-ADMISSION)                       |
|  [03]   | `nuqs/adapters/tanstack-router`    | TanStack Router                 | reject (router pkg NO-ADMISSION)                       |
|  [04]   | `nuqs/adapters/next\|next/app\|next/pages` | Next.js App/Pages       | reject (no Next runtime)                               |
|  [05]   | `nuqs/adapters/remix`              | Remix / React Router data       | reject (no Remix runtime)                              |
|  [06]   | `nuqs/adapters/react`              | generic React (history-less)    | fallback only; `custom` is the native-Navigation choice |
|  [07]   | `nuqs/adapters/testing`            | test harness (`NuqsTestingAdapter`) | test-only                                          |

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser types
- rail: state

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                  |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `SingleParser<T>`            | interface     | `parse: (string) => T \| null`, optional `serialize`/`eq` for one key |
|  [02]   | `MultiParser<T>`             | interface     | `parse: (readonly string[]) => T \| null` over repeated keys |
|  [03]   | `GenericParser<T>`           | union         | `SingleParser<T> \| MultiParser<T>`                   |
|  [04]   | `SingleParserBuilder<T>`     | type          | `Required<SingleParser<T>> & Options` + `.withOptions`/`.withDefault` |
|  [05]   | `MultiParserBuilder<T>`      | type          | multi-key builder variant                             |
|  [06]   | `GenericParserBuilder<T>`    | union         | `SingleParserBuilder<T> \| MultiParserBuilder<T>`     |
|  [07]   | `ParserMap`                  | type alias    | `Record<string, ParserWithOptionalDefault<any>>` — the loader/serializer/cache input |
|  [08]   | `inferParserType<Input>`     | utility type  | infers state type from a parser or a `ParserMap`      |
|  [09]   | `Parser<T>` / `ParserBuilder<T>` | type      | DEPRECATED aliases of `SingleParser`/`SingleParserBuilder` — never cite |

[PUBLIC_TYPE_SCOPE]: options and URL types
- rail: state
- `Options` is the one policy carrier every op reads; `.withOptions` bakes it at parser declaration, the setter's trailing `options?` overrides per-call.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [FIELDS / BOUNDARY]                                                        |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Options`          | interface     | `history`, `scroll`, `shallow`, `limitUrlUpdates`, `startTransition`, `clearOnDefault` (+ DEPRECATED `throttleMs`) |
|  [02]   | `HistoryOptions`   | string union  | `"replace"` (default) \| `"push"`                                          |
|  [03]   | `LimitUrlUpdates`  | union         | `{ method: "debounce"; timeMs }` \| `{ method: "throttle"; timeMs }`       |
|  [04]   | `SearchParams`     | type alias    | `Record<string, string \| string[] \| undefined>`                          |
|  [05]   | `UrlKeys<Parsers>` | utility type  | `Partial<Record<keyof Parsers, string>>` — URL key aliasing               |
|  [06]   | `Nullable<T>`      | utility type  | `{ [K in keyof T]: T[K] \| null }`                                         |
|  [07]   | `LoaderInput`      | union         | `URL \| Request \| URLSearchParams \| Record<...> \| string`               |

[PUBLIC_TYPE_SCOPE]: adapter binding (`nuqs/adapters/custom`, `unstable_`-prefixed)
- rail: state
- nuqs 2.x resolves every hook through a React-context adapter mounted at the app root; NO adapter -> the hooks throw a `NuqsError`. `createAdapterProvider(useAdapter)` builds the provider from one `UseAdapterHook`; `platform` implements that hook over the native Navigation API.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [SIGNATURE / BOUNDARY]                                                     |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `unstable_UseAdapterHook`         | type          | `(watchKeys: string[]) => unstable_AdapterInterface`                       |
|  [02]   | `unstable_AdapterInterface`       | type          | `{ searchParams: URLSearchParams; pathname?; updateUrl: UpdateUrlFunction; getSearchParamsSnapshot?; rateLimitFactor?; autoResetQueueOnUpdate? }` |
|  [03]   | `unstable_UpdateUrlFunction`      | type          | `(search: URLSearchParams, options: Required<AdapterOptions>) => void` — the `window.navigation` bridge point |
|  [04]   | `unstable_AdapterOptions`         | type          | `Pick<Options, "history" \| "scroll" \| "shallow">`                        |
|  [05]   | `unstable_AdapterContext`         | type          | the React context the provider fills                                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: React hooks (require a mounted adapter)
- rail: state

```ts
// nuqs — the setter returns Promise<URLSearchParams> and batches URL writes
declare function useQueryState<T>(
  key: string,
  options: UseQueryStateOptions<T> & { defaultValue: T },
): UseQueryStateReturn<NonNullable<ReturnType<typeof options.parse>>, typeof options.defaultValue>
declare function useQueryState<T>(key: string, options: UseQueryStateOptions<T>): UseQueryStateReturn<NonNullable<ReturnType<typeof options.parse>>, undefined>
declare function useQueryState(key: string, options?: Pick<UseQueryStateOptions<string>, keyof Options>): UseQueryStateReturn<string, undefined>
declare function useQueryStates<KeyMap extends UseQueryStatesKeysMap>(
  keyMap: KeyMap,
  options?: Partial<UseQueryStatesOptions<KeyMap>>, // { urlKeys?: UrlKeys<KeyMap> } & Options
): UseQueryStatesReturn<KeyMap> // [Values<KeyMap>, SetValues<KeyMap>]; SetValues returns Promise<URLSearchParams>
```

[ENTRYPOINT_SCOPE]: parser roster — seed data over the `createParser` mechanism
- rail: state
- The 15 builders below are NOT the mechanism; they are instances of `createParser`/`createMultiParser` pre-baked with a `parse`/`serialize`/`eq`. A new type is `createParser({ parse, serialize })`, never a 16th special case. Each yields a `SingleParserBuilder<T>` (except `parseAsNativeArrayOf` -> pre-defaulted multi) carrying `.withDefault(v)` (non-nullable state) and `.withOptions(opts)`.

| [INDEX] | [SYMBOL]                                 | [STATE_TYPE]  | [NOTE]                                        |
| :-----: | :--------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `parseAsString`                          | `string`      | identity                                      |
|  [02]   | `parseAsInteger` / `parseAsIndex` / `parseAsHex` | `number` | int / 0-based↔1-based index / hex             |
|  [03]   | `parseAsFloat`                           | `number`      | float                                         |
|  [04]   | `parseAsBoolean`                         | `boolean`     |                                               |
|  [05]   | `parseAsTimestamp` / `parseAsIsoDateTime` / `parseAsIsoDate` | `Date` | epoch ms / ISO-8601 / ISO date-only |
|  [06]   | `parseAsStringEnum<Enum>(values)`        | `Enum`        | string enum membership                        |
|  [07]   | `parseAsStringLiteral<const L>(values)`  | `L`           | `readonly L[]` literal union                  |
|  [08]   | `parseAsNumberLiteral<const L>(values)`  | `L`           | `readonly L[]` numeric literal union          |
|  [09]   | `parseAsJson<T>(validator)`              | `T`           | validator is `((unknown) => T \| null) \| StandardSchemaV1<T>` — the Effect `Schema` bridge |
|  [10]   | `parseAsArrayOf<Item>(itemParser, sep?)` | `Item[]`      | one URL key, `sep`-joined (default `,`)       |
|  [11]   | `parseAsNativeArrayOf<Item>(itemParser)` | `Item[]`      | repeated URL key (`?a=1&a=2`); returns a pre-defaulted (non-nullable) multi builder |

[ENTRYPOINT_SCOPE]: custom parser + adapter factories
- rail: state

```ts
declare function createParser<T>(parser: Require<SingleParser<T>, "parse" | "serialize">): SingleParserBuilder<T>
declare function createMultiParser<T>(parser: Omit<Require<MultiParser<T>, "parse" | "serialize">, "type">): MultiParserBuilder<T>

// nuqs/adapters/custom — the platform SPA-root binding over the native Navigation API
declare function unstable_createAdapterProvider(useAdapter: unstable_UseAdapterHook): AdapterProvider
declare function renderQueryString(search: URLSearchParams): string // nuqs's canonical URL encoding
```

[ENTRYPOINT_SCOPE]: React-free codec surface (`nuqs` root + `nuqs/server`)
- rail: state
- The surface `Session/router.md` `RouteParamCodec` composes — pure parse/serialize with NO React, NO adapter.

```ts
declare function createLoader<P extends ParserMap>(parsers: P, options?: { urlKeys?: UrlKeys<P> }): LoaderFunction<P>
// LoaderFunction: (input: LoaderInput, { strict? }?) => inferParserType<P>  — strict:true throws on parse failure

declare function createSearchParamsCache<P extends ParserMap>(parsers: P, options?: { urlKeys?: UrlKeys<P> }): CacheInterface<P>
// CacheInterface<P>: parse(searchParams, { strict? }?) -> parsed; all() -> parsed; get(key) -> value  (RSC per-request cache)

declare function createSerializer<P extends ParserMap, Base = string, Return = string>(
  parsers: P,
  options?: { clearOnDefault?: boolean; urlKeys?: UrlKeys<P>; processUrlSearchParams?: (URLSearchParams) => URLSearchParams },
): SerializeFunction<P, Base, Return> // (values) => string  OR  (base, values | null) => string
```

[ENTRYPOINT_SCOPE]: Standard-Schema bridge, rate-limiting, testing
- rail: state

```ts
// Standard Schema v1 — the Effect Schema interop seam (both directions)
declare function createStandardSchemaV1<P extends ParserMap, Partial extends boolean = false>(
  parsers: P, options?: { urlKeys?: UrlKeys<P>; partialOutput?: Partial },
): StandardSchemaV1<MaybePartial<Partial, inferParserType<P>>> // nuqs parsers -> Standard Schema (TanStack Router / Effect consume)
// reverse: parseAsJson(Schema.standardSchemaV1(effectSchema)) — an Effect Schema validates a JSON URL param

// nuqs (root) + nuqs/server
declare function throttle(timeMs: number): LimitUrlUpdates
declare function debounce(timeMs: number): LimitUrlUpdates
declare const defaultRateLimit: LimitUrlUpdates // 50ms (120ms Safari) floor

// nuqs/testing — parser round-trip (bijectivity) laws for RouteParamCodec property tests
declare function isParserBijective<T>(parser: SingleParserBuilder<T> | MultiParserBuilder<T>, serialized: string | string[], input: T): boolean
declare function testSerializeThenParse<T>(parser, input): boolean
declare function testParseThenSerialize<T>(parser, input): boolean
```

## [04]-[IMPLEMENTATION_LAW]

[STATE_TOPOLOGY]:
- `useQueryState` binds one URL key to `[value, setter]`; the setter returns `Promise<URLSearchParams>` and batches URL writes. `useQueryStates` binds a key map atomically — all keys update in one URL push/replace.
- `.withDefault(v)` makes state non-nullable when the key is absent (the value is NOT written to the URL unless set); `.withOptions(opts)` bakes `Options` at declaration; the setter's trailing `options?` overrides per-call.
- `limitUrlUpdates: throttle(N)`/`debounce(N)` caps Web History API writes; the floor is 50ms (120ms Safari) — lower values no-op. `clearOnDefault: true` (default) omits a key equal to its default from the URL.
- the parser roster is `createParser`-derived seed data; a new URL type is one `createParser({ parse, serialize, eq? })` call, never a parallel `parseAsXxx` special case outside the mechanism.

[ADAPTER_LAW]:
- nuqs 2.x is adapter-based: EVERY hook resolves its `searchParams`/`updateUrl` through a React-context adapter mounted at the app root; an unmounted adapter throws. The adapter is the ONE host-binding seam — `unstable_createAdapterProvider(useAdapter)` wraps a `UseAdapterHook: (watchKeys) => AdapterInterface`.
- `platform` runs the native Navigation API with ZERO router package, so it implements the `custom` adapter: `useAdapter` reads `searchParams` from `new URLSearchParams(window.location.search)` (re-subscribing on the `navigate` event `Session/router.md` already owns) and routes `updateUrl(search, { history }) => window.navigation.navigate(\`?${search}\`, { history })`. This is the single seam where nuqs writes flow into the same Navigation-API ingress `AppRouter` intercepts — never a second `history.pushState` path.
- the adapter is mounted only when `ui` consumes the hooks directly; `Session/router.md` `RouteParamCodec` uses the React-FREE parser+serializer surface below and needs NO adapter.

[INTEGRATION_LAW]:
- Effect `Schema` bridge (both directions): `parseAsJson(Schema.standardSchemaV1(ParamsSchema))` (`libs/typescript/.api/effect.md` `Schema.standardSchemaV1`) validates a JSON URL param through an Effect Schema — decode failures become the parser's `null`; `createStandardSchemaV1(parserMap)` produces a Standard Schema FROM nuqs parsers for a Standard-Schema consumer. `@standard-schema/spec@1.0.0` is the shared contract both sides implement.
- `Session/router.md` codec split: `RouteParamCodec.encode` composes `createSerializer(parsers)` (params -> search string, `processUrlSearchParams` for key ordering); `RouteParamCodec.decode` composes `createLoader(parsers)` or a direct parser `.parse` over the `URLSearchParams` (search -> typed params), each wrapped in Effect so a parse `null` folds to the `not-found` resolution rather than throwing. This surface is React-free — no adapter, no hook.
- `ui` state binding: a URL-resident value surfaces to a component only through the `ui` `AtomBinding` over the router's location `SubscriptionRef` + query string, OR through a `useQueryState`/`useQueryStates` hook under the mounted custom adapter — never a route value copied into local component state.
- rate-limit + transition: `startTransition` (from `React.useTransition`) wraps non-shallow updates; `limitUrlUpdates` shares the same throttle/debounce vocabulary the router's other ingress rate-limits use.

[LOCAL_ADMISSION]:
- import parsers/loader/serializer/cache from `nuqs/server` (or the root) for React-free codec work; import hooks + `unstable_createAdapterProvider` from `nuqs` + `nuqs/adapters/custom` only where React runs.
- the custom adapter is `platform`'s SPA-root obligation ONLY if `ui` uses the hooks; the router codec never needs it.
- never cite the deprecated surface: `Parser`/`ParserBuilder` (use `SingleParser`/`SingleParserBuilder`), `Options.throttleMs` (use `limitUrlUpdates: throttle(N)`), `parseServerSide` (use `createLoader`).

[RAIL_LAW]:
- Package: `nuqs`
- Owns: URL search-param state — typed `parseAs*` parsing (over `createParser`), React hooks, the React-free loader/serializer/cache codec, the Standard-Schema↔Effect bridge, and the adapter host-binding seam
- Accept: `parseAs*` (+ `createParser`) for typing; `createSerializer`/`createLoader` for the React-free `RouteParamCodec`; `parseAsJson(Schema.standardSchemaV1(...))` for Effect-validated JSON params; `nuqs/adapters/custom` `unstable_createAdapterProvider` bridging `updateUrl` to `window.navigation`; `nuqs/testing` bijectivity helpers for codec property tests
- Reject: manual `URLSearchParams` manipulation a `useQueryState`/serializer can own; a router package (`react-router`/`tanstack-router`/`history`) or a non-`custom` adapter; a second `history.pushState` path beside the Navigation-API bridge; the deprecated `Parser`/`throttleMs`/`parseServerSide` surface; a route value copied into local component state
