# [TS_RUNTIME_API_NUQS]

`nuqs` types the URL query string: the `parseAs*` builder atoms, `createParser`/`createMultiParser`, and the pure `createSerializer`/`createLoader`/`createStandardSchemaV1` codec own parse and serialize while the Navigation API owns traversal — that split is the zero-routing-package law.

`browser/route` composes only the React-free `nuqs/server` codec; the `useQueryState*` hooks and their adapters are a `ui`-tier surface, and the RSC `createSearchParamsCache` is the streaming-SSR non-goal.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the parser algebra and the URL-write option vocabulary
- `.withDefault(v)` makes a key non-nullable and, with `clearOnDefault`, drops it from the URL at the default; `.withOptions` overrides write behavior per key.
- `createLoader` decodes the widened `LoaderInput = URL \| Request \| URLSearchParams \| Record<string, string \| string[] \| undefined> \| string`.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `SingleParser` / `MultiParser`               | object type   | one-key vs repeated-key `parse`/`serialize`/`eq` triple         |
|  [02]   | `GenericParser`                              | union         | `SingleParser \| MultiParser`                                   |
|  [03]   | `SingleParserBuilder` / `MultiParserBuilder` | builder       | atom carrying `withDefault`/`withOptions` folded over `Options` |
|  [04]   | `ParserMap`                                  | record        | per-route key→parser map every codec function folds             |
|  [05]   | `inferParserType`                            | type-fn       | `ParserMap` → decoded typed state record                        |
|  [06]   | `Options`                                    | record        | URL-write knob set; the pure codec reads only `clearOnDefault`  |
|  [07]   | `HistoryOptions`                             | union         | `"replace" \| "push"`                                           |
|  [08]   | `LimitUrlUpdates`                            | record        | `{ method: "debounce" \| "throttle"; timeMs }`                  |
|  [09]   | `UrlKeys`                                    | mapped type   | `Partial<Record<keyof P, string>>` URL-key rename map           |
|  [10]   | `LoaderInput`                                | union         | the `createLoader` input union, rostered below                  |
|  [11]   | `SerializeFunction`                          | call type     | `(patch) -> R`; overload `(base, patch \| null) -> R`           |

- Rows [08] and [11] declare inside the entry module and reach no export specifier, so an import of the bare name resolves nowhere — each reads off the surface returning it: `ReturnType<typeof throttle>`, `ReturnType<typeof createSerializer<Parsers>>`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parser atoms, the parameterized constructors, the pure codec, and the rate-limit builders

[SCALAR_ATOMS]: `parseAsString` `parseAsInteger` `parseAsFloat` `parseAsIndex` `parseAsHex` `parseAsBoolean` — `SingleParserBuilder` scalar constants over `string`/`number`/`boolean`, `parseAsIndex` offset, `parseAsHex` radix.
[DATE_ATOMS]: `parseAsTimestamp` `parseAsIsoDateTime` `parseAsIsoDate` — `SingleParserBuilder<Date>` over epoch-ms, ISO-8601, date-only.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `parseAsStringEnum(values) -> SingleParserBuilder<E>`         | factory  | closed enum vocabulary                  |
|  [02]   | `parseAsStringLiteral(readonly values)`                       | factory  | closed string-literal set               |
|  [03]   | `parseAsNumberLiteral(readonly values)`                       | factory  | closed number-literal set               |
|  [04]   | `parseAsJson(validator \| StandardSchemaV1<T>)`               | factory  | validated JSON — the Schema seam        |
|  [05]   | `parseAsArrayOf(itemParser, separator?)`                      | factory  | delimited list over an item parser      |
|  [06]   | `parseAsNativeArrayOf(itemParser)`                            | factory  | repeated-key (`?k=a&k=b`) multi         |
|  [07]   | `createParser(SingleParser) -> SingleParserBuilder`           | factory  | own any state `T` beyond the atoms      |
|  [08]   | `createMultiParser(MultiParser) -> MultiParserBuilder`        | factory  | own any repeated-key state `T`          |
|  [09]   | `createSerializer(ParserMap, Options?) -> SerializeFunction`  | factory  | state → next query string               |
|  [10]   | `createLoader(ParserMap, {urlKeys}?) -> LoaderFunction`       | factory  | `LoaderInput` → typed state record      |
|  [11]   | `createStandardSchemaV1(ParserMap, {urlKeys,partialOutput}?)` | factory  | route `ParserMap` → Standard Schema     |
|  [12]   | `throttle(number)` / `debounce(number)`                       | factory  | `-> LimitUrlUpdates`                    |
|  [13]   | `defaultRateLimit`                                            | property | the default `LimitUrlUpdates`           |
|  [14]   | `isParserBijective(parser, serialized, input)`                | proof    | round-trip identity in both directions  |
|  [15]   | `testParseThenSerialize(parser, input)`                       | proof    | parse then serialize returns the string |
|  [16]   | `testSerializeThenParse(parser, input)`                       | proof    | serialize then parse returns the value  |

- `nuqs/testing` exports the three round-trip proofs; a bespoke `createParser` row exercises them in the tests tier, certifying the bijectivity every `Router.Row` codec claims.

- `createSerializer`: honors only `clearOnDefault`, `urlKeys`, `processUrlSearchParams`; `history`/`scroll`/`shallow`/`startTransition`/`limitUrlUpdates` are hook-only.
- `createLoader`: returns a loader re-decoding any `LoaderInput` per `navigate` event.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `createParser`/`createMultiParser` own any state `T` beyond the fixed `parseAs*` atoms — each a `parse`/`serialize`/`eq` triple over one key, the whole surface — so a bespoke shape is one of those under a Schema.
- `createSerializer`/`createLoader`/`createStandardSchemaV1` are pure functions over a `ParserMap`, no hook and no DOM; `urlKeys` wires identically across loader and serializer or the round-trip drifts.

[STACKING]:
- `effect`(`.api/effect.md`): the pure `createSerializer`/`createLoader` wrap in `Effect.sync`; the decoded param record drives `Match.value` route dispatch, and a `SubscriptionRef` holds the current typed query state the `browser/route` admission folds read.
- kernel `Schema`: `parseAsJson(Schema.standardSchemaV1(RouteParamSchema))` decodes a URL param straight into a kernel-branded value; `createStandardSchemaV1(parsers)` hands the whole route's `ParserMap` to any Standard-Schema consumer.
- `browser/route` Navigation API: nuqs is the codec, `navigation.navigate(url)` the traversal; a `navigate`-event listener re-runs `createLoader(parsers)(new URL(navigation.currentEntry.url))` to derive the current typed query state.
- `idb-keyval`(`.api/idb-keyval.md`) (`browser/persist`): `set`/`get` the last-good serialized query string per route key so a cold boot `createLoader`-decodes the restored string before the Navigation API resolves the entry.
- `data read/live`: `createLoader` output IS the live-query key — the decoded record feeds a `Subscribable` window rather than a bespoke param bag.

[LOCAL_ADMISSION]:
- One `ParserMap` per route; `createParser` owns any bespoke encoding rather than string-mashing `URLSearchParams`.
