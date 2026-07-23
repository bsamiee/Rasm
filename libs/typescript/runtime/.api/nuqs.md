# [TS_RUNTIME_API_NUQS]

`nuqs` is a type-safe URL search-param state manager. `browser/route.md` consumes ONLY its framework-agnostic codec core from the `nuqs/server` entry — the `parseAs*` builder atoms, `createParser`, `createSerializer`, `createLoader`, `createStandardSchemaV1` — and composes it against the Navigation API: nuqs owns typed parse/serialize of the query string, the Navigation API owns traversal. That split IS the zero-routing-package law. The React `useQueryState`/`useQueryStates` hooks require a `nuqs/adapters/*` provider plus React and are outside the browser rail; the `nuqs/server` parsers, serializer, loader, and standard-schema are React-free in behavior, so a browser build tree-shakes to the codec alone — only `createSearchParamsCache` (the RSC cache) statically touches React, dropped when unused.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nuqs`
- package: `nuqs` (MIT)
- module: pure ESM (`"type": "module"`); entries `nuqs` (React hooks + re-exported core) and `nuqs/server` (the framework-agnostic codec entry — the browser rail; its parsers/serializer/loader are React-free, only the RSC `createSearchParamsCache` imports React, tree-shaken when unused); types `dist/index.d.ts` / `dist/server.d.ts`
- marker: `sideEffects` limited to `./dist/debug.js`; peer `@standard-schema/spec` (Standard Schema catalog-bound interop); the `Options` type references React's `TransitionStartFunction` type-only, never a runtime pull
- bound asset: TSDECL `node_modules/nuqs/dist/{index,server,parsers,defs}.d.ts` (`assay api resolve nuqs` → catalog-bound, `restore: restored`)
- admission: folder-local `# browser` catalog group; version centralized in `pnpm-workspace.yaml`, never pinned here
- role: `browser/route.md` (the R17 composition site — URL query-state typing over the Navigation-API router); `browser/route.md` folds the decoded params
- rail: `browser/route`

## [02]-[PARSER_ALGEBRA]

The parser is the atom: a `parse`/`serialize`/`eq` triple over one URL key. `createParser`/`createMultiParser` is the one parameterized mechanism that owns any state type `T` beyond the built-in atoms — the fixed `parseAs*` roster is convenience over this, never the surface itself.

[SURFACES]: `SingleParser` `MultiParser` `GenericParser` `SingleParserBuilder` `createParser` `createMultiParser` `ParserMap` `inferParserType` `withDefault` `parseServerSide`

Consumer note: `browser/route.md` declares one `ParserMap` per route; `.withDefault(v)` makes a key non-nullable and (with `clearOnDefault`) omits it from the URL at the default; `createParser` owns any bespoke encoding rather than string-mashing `URLSearchParams`.

## [03]-[BUILTIN_PARSERS]

The atom roster — primitive scalars, closed-vocabulary refinements, and the two composite constructors that own everything richer. `parseAsJson` and `parseAsArrayOf` are the parameterized escape hatches; a custom shape is one of those under a Schema, never a new hand-rolled parser.

| [INDEX] | [SYMBOL]                                                     | [STATE_TYPE]      | [KIND]                             |
| :-----: | :----------------------------------------------------------- | :---------------- | :--------------------------------- |
|  [01]   | `parseAsString` / `parseAsInteger` / `parseAsFloat`          | `string`/`number` | scalar atoms                       |
|  [02]   | `parseAsIndex` / `parseAsHex`                                | `number`          | offset/radix scalar atoms          |
|  [03]   | `parseAsBoolean`                                             | `boolean`         | scalar atom                        |
|  [04]   | `parseAsTimestamp` / `parseAsIsoDateTime` / `parseAsIsoDate` | `Date`            | epoch-ms / ISO-8601 / date-only    |
|  [05]   | `parseAsStringEnum<E>(values)`                               | `E`               | closed enum vocabulary             |
|  [06]   | `parseAsStringLiteral<L>(readonly values)`                   | `L`               | closed string-literal vocabulary   |
|  [07]   | `parseAsNumberLiteral<L>(readonly values)`                   | `L`               | closed number-literal vocabulary   |
|  [08]   | `parseAsJson<T>(validator \| StandardSchemaV1<T>)`           | `T`               | validated JSON — the Schema seam   |
|  [09]   | `parseAsArrayOf<I>(itemParser, separator?)`                  | `I[]`             | delimited list over an item parser |
|  [10]   | `parseAsNativeArrayOf<I>(itemParser)`                        | `I[]`             | repeated-key (`?k=a&k=b`) multi    |

Consumer note: `parseAsStringLiteral`/`parseAsNumberLiteral` type a URL param against a closed set exactly as a `Schema.Literal`; `parseAsJson(validator)` accepts any predicate or Standard Schema catalog-bound, so a kernel `Schema` (via `Schema.standardSchemaV1`) validates a structured query value in place.

## [04]-[SERIALIZER_LOADER]

The write side (state → query string) and the read side (any input → typed state) are pure functions over a `ParserMap` — no hook, no DOM, React-free in behavior. These three are the whole browser codec.

[SERIALIZE_FUNCTION]: `SerializeFunction.call(Partial<Nullable<inferParserType<P>>>) -> R` `SerializeFunction.call(Base,Partial<Nullable<inferParserType<P>>>|null) -> R`
[LOADER_INPUT]: `LoaderInput = URL|Request|URLSearchParams|Record<string,string|string[]|undefined>|string`
[SURFACES]: `createSerializer(P,{clearOnDefault?:boolean;urlKeys?:UrlKeys<P>;processUrlSearchParams?:(s:URLSearchParams)=>URLSearchParams}?) -> SerializeFunction<P>` `createLoader(P,{urlKeys?:UrlKeys<P>}?) -> {…}` `createStandardSchemaV1(P,{urlKeys?:UrlKeys<P>;partialOutput?:Partial}?) -> StandardSchemaV1<MaybePartial<Partial,inferParserType<P>>>`

Consumer note: `createSerializer(parsers)(currentUrl, patch)` yields the next query string the Navigation API navigates to; `createLoader(parsers)(new URL(entry.url))` re-decodes on each `navigate` event. `createSearchParamsCache` (in `nuqs/server`) is React `cache()`-backed RSC-only and is NOT the browser rail — streaming SSR is the named non-goal.

## [05]-[OPTIONS_RATE_LIMIT]

`Options` governs URL-write behavior across every operation, but the pure serializer reads only a slice of it.

[HISTORY_OPTIONS]: `HistoryOptions = "replace"|"push"`
[LIMIT_URL_UPDATES]: `LimitUrlUpdates.method: "debounce"|"throttle"` `LimitUrlUpdates.timeMs: number`
[OPTIONS]: `Options.history: HistoryOptions` `Options.scroll: boolean` `Options.shallow: boolean` `Options.limitUrlUpdates: LimitUrlUpdates` `Options.startTransition: TransitionStartFunction` `Options.clearOnDefault: boolean`
[URL_KEYS]: `UrlKeys = Partial<Record<keyof P,string>>`
[SURFACES]: `throttle(number) -> LimitUrlUpdates` `debounce(number) -> LimitUrlUpdates` `defaultRateLimit: LimitUrlUpdates`

Consumer note: `createSerializer` honors ONLY `clearOnDefault` (plus `urlKeys`, `processUrlSearchParams`); `history`/`scroll`/`shallow`/`startTransition`/`limitUrlUpdates` are the hook/router surface consumed by `useQueryState*`, so the browser codec never touches them. `urlKeys` must be wired identically across loader and serializer or the round-trip drifts.

## [06]-[REACT_SURFACE_BOUNDARY]

`useQueryState<T>(key, options)` and `useQueryStates<KeyMap>(keyMap, options?)` bind a `[value, setter]` pair (setter returns `Promise<URLSearchParams>`, batching writes) — but only inside a React tree wrapped by a `nuqs/adapters/{react,next,react-router,tanstack-router}` provider. They are catalogued as the boundary a future `browser` rebuild must NOT reach for: the browser folder is non-React, so any query-state hook is a `ui`-tier concern, and `browser/route.md` composes the `nuqs/server` codec instead.

## [07]-[STACKING]

- kernel `Schema`: `parseAsJson(Schema.standardSchemaV1(RouteParamSchema))` feeds a kernel `Schema` (projected to Standard Schema catalog-bound) straight into the JSON parser — the URL param decodes once into a kernel-branded value, never a raw string. Conversely `createStandardSchemaV1(parsers)` hands the whole route's `ParserMap` to any Standard-Schema consumer.
- `browser/route` Navigation API: nuqs is the codec, `navigation.navigate(url)` the traversal; a `navigate`-event listener re-runs `createLoader(parsers)(new URL(navigation.currentEntry.url))` to derive the current typed query state — zero routing package.
- `effect` rails: the pure `createSerializer`/`createLoader` wrap in `Effect.sync`; the decoded param record drives `Match.value` route dispatch; a `SubscriptionRef` holds the current typed query state the `browser/route` admission folds read.
- sibling `idb-keyval` (`browser/persist`): `set`/`get` the last-good serialized query string per route key so a cold boot `createLoader`-decodes the restored string to typed state before the Navigation API resolves the entry.
- `data read/live`: a decoded query record is a live-query key — `createLoader` output feeds a `Subscribable` window rather than a bespoke param bag.

## [08]-[RAIL_LAW]

- Owns: typed URL query-state parse, serialize, and load — the codec half of `browser/route`.
- Accept: the `nuqs/server` core (`parseAs*` atoms, `createParser`, `createSerializer`, `createLoader`, `createStandardSchemaV1`) composed against the Navigation API and kernel `Schema`.
- Reject: the React hooks/adapters inside `browser` (a `ui`-tier surface); `createSearchParamsCache` (RSC non-goal); manual `URLSearchParams` manipulation for state a parser can own; treating nuqs as the router (the Navigation API is).
