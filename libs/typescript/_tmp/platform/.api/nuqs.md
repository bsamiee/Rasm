# [API_CATALOGUE] nuqs

`nuqs` provides URL query-string state synchronization for React applications: `useQueryState` and `useQueryStates` bind React state to URL search params, `createLoader` and `createSearchParamsCache` serve server-side parsing in RSC/SSR frameworks, `createSerializer` generates typed URL strings, and a catalog of `parseAs*` parser builders covers all primitive types. The `Options` type governs history mode, scroll, shallow routing, rate limiting, and transition integration across all operations.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nuqs`
- package: `nuqs`
- module: `nuqs` (client hooks + parsers); `nuqs/server` (SSR/RSC utilities)
- asset: runtime library
- rail: state

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser types
- rail: state

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `SingleParser<T>`         | interface     | parse/serialize/eq for one URL key                |
|  [02]   | `MultiParser<T>`          | interface     | parse/serialize/eq over array of keys             |
|  [03]   | `GenericParser<T>`        | union type    | `SingleParser<T> \| MultiParser<T>`               |
|  [04]   | `SingleParserBuilder<T>`  | type          | builder with `withOptions`/`withDefault`          |
|  [05]   | `MultiParserBuilder<T>`   | type          | multi-key variant of builder                      |
|  [06]   | `GenericParserBuilder<T>` | union type    | `SingleParserBuilder<T> \| MultiParserBuilder<T>` |
|  [07]   | `ParserMap`               | type alias    | `Record<string, ParserWithOptionalDefault<any>>`  |
|  [08]   | `inferParserType<Input>`  | utility type  | infers state type from parser or map              |

[PUBLIC_TYPE_SCOPE]: options and URL types
- rail: state

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                                     |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Options`          | interface     | history, scroll, shallow, limitUrlUpdates, startTransition, clearOnDefault |
|  [02]   | `HistoryOptions`   | string union  | `"replace" \| "push"`                                                      |
|  [03]   | `LimitUrlUpdates`  | union type    | `{ method: "debounce"; timeMs }` or throttle                               |
|  [04]   | `SearchParams`     | type alias    | `Record<string, string \| string[] \| undefined>`                          |
|  [05]   | `UrlKeys<Parsers>` | utility type  | `Partial<Record<keyof Parsers, string>>` for key aliasing                  |
|  [06]   | `Nullable<T>`      | utility type  | `{ [K in keyof T]: T[K] \| null }`                                         |
|  [07]   | `LoaderInput`      | union type    | `URL \| Request \| URLSearchParams \| Record<...> \| string`               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: React hooks
- rail: state

```ts
// nuqs (dist/index.d.ts)
declare function useQueryState<T>(
  key: string,
  options: UseQueryStateOptions<T> & { defaultValue: T },
): UseQueryStateReturn<NonNullable<ReturnType<typeof options.parse>>, typeof options.defaultValue>

declare function useQueryState<T>(
  key: string,
  options: UseQueryStateOptions<T>,
): UseQueryStateReturn<NonNullable<ReturnType<typeof options.parse>>, undefined>

declare function useQueryState(
  key: string,
  options?: Pick<UseQueryStateOptions<string>, keyof Options>,
): UseQueryStateReturn<string, undefined>

// return type: [value, setter] where setter returns Promise<URLSearchParams>

declare function useQueryStates<KeyMap extends UseQueryStatesKeysMap>(
  keyMap: KeyMap,
  options?: Partial<UseQueryStatesOptions<KeyMap>>,
): UseQueryStatesReturn<KeyMap>
```

[ENTRYPOINT_SCOPE]: built-in parser builders
- rail: state

| [INDEX] | [SYMBOL]                                 | [STATE_TYPE] |
| :-----: | :--------------------------------------- | :----------- |
|  [01]   | `parseAsString`                          | `string`     |
|  [02]   | `parseAsInteger`                         | `number`     |
|  [03]   | `parseAsFloat`                           | `number`     |
|  [04]   | `parseAsBoolean`                         | `boolean`    |
|  [05]   | `parseAsTimestamp`                       | `Date`       |
|  [06]   | `parseAsIsoDateTime`                     | `Date`       |
|  [07]   | `parseAsIsoDate`                         | `Date`       |
|  [08]   | `parseAsIndex`                           | `number`     |
|  [09]   | `parseAsHex`                             | `number`     |
|  [10]   | `parseAsStringEnum<Enum>(values)`        | `Enum`       |
|  [11]   | `parseAsStringLiteral<Literal>(values)`  | `Literal`    |
|  [12]   | `parseAsNumberLiteral<Literal>(values)`  | `Literal`    |
|  [13]   | `parseAsJson<T>(validator)`              | `T`          |
|  [14]   | `parseAsArrayOf<ItemType>(parser)`       | `ItemType[]` |
|  [15]   | `parseAsNativeArrayOf<ItemType>(parser)` | `ItemType[]` |

[ENTRYPOINT_SCOPE]: custom parser factories
- rail: state

```ts
declare function createParser<T>(
  parser: Require<SingleParser<T>, "parse" | "serialize">,
): SingleParserBuilder<T>

declare function createMultiParser<T>(
  parser: Omit<Require<MultiParser<T>, "parse" | "serialize">, "type">,
): MultiParserBuilder<T>
```

[ENTRYPOINT_SCOPE]: server-side utilities (nuqs/server)
- rail: state

```ts
// nuqs/server (dist/server.d.ts)
declare function createLoader<Parsers extends ParserMap>(
  parsers: Parsers,
  options?: CreateLoaderOptions<Parsers>,
): LoaderFunction<Parsers>

// RSC cache — Next.js App Router integration
declare function createSearchParamsCache<Parsers extends ParserMap>(
  parsers: Parsers,
  options?: { urlKeys?: UrlKeys<Parsers> },
): CacheInterface<Parsers>
// CacheInterface<P> exposes: parse(searchParams), all(), get(key)
```

[ENTRYPOINT_SCOPE]: serializer and Standard Schema integration
- rail: state

```ts
declare function createSerializer<Parsers extends ParserMap, BaseType extends Base = Base, Return = string>(
  parsers: Parsers,
  options?: CreateSerializerOptions<Parsers>,
): SerializeFunction<Parsers, BaseType, Return>
// SerializeFunction: (values) => string  or  (base, values) => string

declare function createStandardSchemaV1<Parsers extends ParserMap, PartialOutput extends boolean = false>(
  parsers: Parsers,
  options?: CreateStandardSchemaV1Options<Parsers, PartialOutput>,
): StandardSchemaV1<MaybePartial<PartialOutput, inferParserType<Parsers>>>
```

[ENTRYPOINT_SCOPE]: rate-limiting helpers
- rail: state

```ts
declare function throttle(timeMs: number): LimitUrlUpdates
declare function debounce(timeMs: number): LimitUrlUpdates
declare const defaultRateLimit: LimitUrlUpdates
```

## [04]-[IMPLEMENTATION_LAW]

[STATE_TOPOLOGY]:
- `useQueryState` binds one URL key to a `[value, setter]` pair; the setter returns `Promise<URLSearchParams>` and batches URL writes
- `useQueryStates` binds a key map atomically; all keys update in a single URL push/replace
- parser builders expose `.withDefault(value)` to make state non-nullable when the key is absent
- `.withOptions(options)` bakes `Options` into the parser at declaration time; the setter's `options` argument overrides per-call
- `limitUrlUpdates: throttle(N)` or `debounce(N)` caps Web History API calls; minimum effective value is 50 ms (120 ms for Safari)
- `clearOnDefault: true` (default) omits the key from the URL when the state equals the default value

[LOCAL_ADMISSION]:
- Server components use `createLoader` or `createSearchParamsCache`; neither depends on React state or DOM APIs.
- `createSearchParamsCache` integrates with Next.js App Router `searchParams` page prop; call `.parse(searchParams)` in the page, then `.get(key)` inside server components.
- `urlKeys` option renames the URL key without changing the state object key; wire `urlKeys` consistently across loader, serializer, and hooks.
- `createStandardSchemaV1` produces a Standard Schema v1 validator for TanStack Router search param integration.

[RAIL_LAW]:
- Package: `nuqs`
- Owns: URL search-param state synchronization, typed parsing, and server-side search-param access
- Accept: `parseAs*` builders for state typing; `createLoader`/`createSearchParamsCache` for SSR
- Reject: manual `URLSearchParams` manipulation for state that a `useQueryState` hook can own
