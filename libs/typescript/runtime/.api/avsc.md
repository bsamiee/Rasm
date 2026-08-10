# [TS_RUNTIME_API_AVSC]

`avsc` owns the Avro binary codec behind the `application/cloudevents+avro` event format and its batch sibling — the one format in the roster no admitted SDK ships and no descriptor compiler generates, because Avro carries its schema as a value rather than as generated code. One `Type` mints once from the frozen `io.cloudevents.AvroCloudEvent` schema and every encode and decode runs through it, so the union-wrapping posture and the logical-type registry arm before the first untrusted byte.

Bundled typings declare themselves incomplete: they cover the node entry alone, spell most payload positions `any`, and omit every member the browser build substitutes. Composition therefore binds the narrow proven core — `Type.forSchema`, `toBuffer`, `fromBuffer`, `isValid`, `fingerprint` — and lands each result through a `Schema` owner rather than reading an `any` forward.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `avsc`
- package: `avsc` (MIT)
- module: CJS `main` (`./lib`) with NO `exports` map, so a deep path physically resolves and the branch refuses it
- types: bundled at `./types` — one hand-written `index.d.ts` whose header declares itself incomplete and browser-blind
- runtime: node lane — a `browser` map redirects `./lib` to `etc/browser/avsc.js` under a bundler-supplied `buffer` shim, which is exactly the host binding core forbids, so the arm seats here
- boundary: every byte member takes and returns node `Buffer` and the reader indexes `Buffer`-only slice methods, so a bare `Uint8Array` is not interchangeable at this seam
- rail: the Avro row of `core/interchange/format`'s event-format roster, whose `arm` column stands empty for exactly this reason

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the schema value, the compiled type, and the option records that decide union and logical-type posture

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `Type`                                          | compiled type  | the codec — `toBuffer`/`fromBuffer`/`isValid`/`fingerprint`       |
|  [02]   | `schema.AvroSchema`                             | schema value   | `DefinedType \| DefinedType[]` over the JSON schema grammar       |
|  [03]   | `schema.RecordType` / `EnumType` / `FixedType`  | named types    | `name`/`namespace`/`aliases` and the `fields` roster              |
|  [04]   | `schema.ArrayType` / `MapType`                  | container      | `items` and `values` recurring into `Schema`                      |
|  [05]   | `schema.LogicalTypeExtension`                   | logical type   | `logicalType` beside an open parameter slot                       |
|  [06]   | `ForSchemaOptions`                              | mint posture   | `wrapUnions`, `logicalTypes`, `registry`, `typeHook`, `namespace` |
|  [07]   | `IsValidOptions`                                | check posture  | `noUndeclaredFields` and an `errorHook` yielding path and value   |
|  [08]   | `CreateResolverOptions` / `Resolver`            | evolution      | reader-vs-writer schema resolution; `Resolver` has no members     |
|  [09]   | `types.RecordType` / `types.Field`              | reflection     | `fields`, `field(name)`, `defaultValue()`, `order`                |
|  [10]   | `types.LogicalType`                             | extension base | `_fromValue`/`_toValue`/`_resolve`/`_export` protected hooks      |
|  [11]   | `types.UnwrappedUnionType` / `WrappedUnionType` | union arms     | the two shapes `wrapUnions` selects between                       |
|  [12]   | `streams.BlockDecoder` / `BlockEncoder`         | node stream    | object-container framing on `stream.Duplex`, node lane only       |

- `Callback<V, Err = any>` is the package's node-style callback shape; every member the format arm composes is synchronous and reaches none of it.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: type minting, the binary codec pair, validation, and schema identity

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------------------------------- |
|  [01]   | `Type.forSchema(schema, opts?)`                | static   | compiles a schema value into the reusable codec                   |
|  [02]   | `Type.forValue(value, opts?)`                  | static   | infers a type from a sample; never on a frozen contract           |
|  [03]   | `Type.forTypes(types, opts?)`                  | static   | unions already-compiled types                                     |
|  [04]   | `Type.isType(arg, ...prefix)`                  | static   | narrows an unknown to a compiled type by name prefix              |
|  [05]   | `type.toBuffer(value) -> Buffer`               | instance | binary encode; throws on a value the type refuses                 |
|  [06]   | `type.fromBuffer(buffer, resolver?, noCheck?)` | instance | binary decode, optionally through a writer resolver               |
|  [07]   | `type.isValid(value, opts?) -> boolean`        | instance | total check; `errorHook` collects path and value per failure      |
|  [08]   | `type.createResolver(writerType, opts?)`       | instance | reader-vs-writer resolution for schema evolution                  |
|  [09]   | `type.fingerprint(algorithm?) -> Buffer`       | instance | canonical schema digest; default MD5, `"sha256"` admitted         |
|  [10]   | `type.schema(opts?)`                           | instance | the canonical schema value back out of a compiled type            |
|  [11]   | `type.decode(buf, pos?, resolver?)`            | instance | `{ value, offset }` for a framed multi-record buffer              |
|  [12]   | `type.encode(value, buf, pos?) -> number`      | instance | writes into a caller buffer; negative return means overflow       |
|  [13]   | `type.equals(other)` / `type.compare(a, b)`    | instance | schema identity and value ordering                                |
|  [14]   | `parse(schemaOrProtocolIdl, options?)`         | function | declared `any`-returning, so the branch mints through `forSchema` |
|  [15]   | `readSchema(schemaIdl, options?)`              | function | `.avdl` IDL to a schema value; the corpus ships `.avsc` alone     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `wrapUnions` decides the whole payload shape and defaults to `"auto"`, which picks wrapped or unwrapped PER UNION by whether two branches share a JavaScript type bucket; both unions the contract declares are bucket-disjoint and land unwrapped, so the posture is stated at the mint rather than left to that inference.
- Wrapped and unwrapped spellings never interchange: an unwrapped branch is the bare value while a wrapped branch is a one-key object named for the branch type, so a posture change respells every encoded payload.
- `io.cloudevents.AvroCloudEvent` declares `attribute` as a map over `null | boolean | int | string | bytes`, so an extension value outside those four Avro types has no Avro representation and refuses at admission rather than at the wire.
- `AvroCloudEventData` recurses through map and array branches, so a decoded `data` is a tree the branch lands through one `Schema` owner instead of reading its `any` forward.
- `toBuffer` and `fromBuffer` speak node `Buffer`: egress passes straight because a `Buffer` IS a `Uint8Array` view, while ingress must wrap, since the reader calls `Buffer`-only slice methods a bare `Uint8Array` does not carry.
- `fingerprint` yields the canonical schema digest, so schema identity derives from the type rather than from a hand-spelled registry string.
- `isValid` is total and reports through `errorHook`, so a pre-encode check accumulates every path rather than surfacing the first throw.
- `createResolver` binds a writer schema to this reader, which is how a peer's older `AvroCloudEvent` generation decodes without the reader re-minting a second type.
- Compilation runs once at module initialization; `Type.forValue` and `parse` infer a schema instead of binding the frozen one and never enter a contract path.

[STACKING]:
- `core/interchange/format`(`core/.planning/interchange/format.md`): the Avro row declares the media type, its batch sibling, and the empty `arm` whose `degrade` names this lane; the codec that fills it mints once here and no second `Type` is constructed anywhere.
- `core/interchange/carrier`(`core/.planning/interchange/carrier.md`): supplies the attribute record the `attribute` map carries and the extension roster whose value types the map's union admits.
- `effect` `Schema`(`.api/effect.md`): `fromBuffer` yields an untyped tree; `Schema.decodeUnknown` lands it once into owned vocabulary and lifts a `ParseError` onto the rail, and `Either.try` converts the encode and decode throws at that one seam.
- `@confluentinc/schemaregistry`(`runtime/.api/confluentinc-schemaregistry.md`): carries its own transitive `avsc` for Kafka PAYLOAD serdes under the registry framing; this catalogue's use encodes the message envelope and the two never share a `Type`.
- `core/value/schema`(`core/.planning/value/schema.md`): `Shape.Ingress` bounds the admitted octets before the decoder sees them, exactly as the core binary arms bound theirs.

[LOCAL_ADMISSION]:
- Mint one module-scope `Type` through `Type.forSchema` with `wrapUnions` stated explicitly, and never re-mint per call.
- Bind the frozen `io.cloudevents.AvroCloudEvent` schema value; `Type.forValue`, `parse`, and `readSchema` infer a shape the contract already fixes.
- Cross `Buffer` and `Uint8Array` at this seam alone, and let no `Buffer` type reach a domain surface or a core signature.
- Land every `fromBuffer` result through a `Schema` owner; the declared return is `any` and the interior never reads one.
- Convert the throw at the arm through `Either.try`, since `toBuffer` and `fromBuffer` signal by throwing and carry no result channel.
- Read schema identity from `fingerprint`, never from a hand-spelled subject-and-version literal beside it.
- Keep `streams.BlockDecoder`/`BlockEncoder`, `createFileDecoder`, `createFileEncoder`, `Service`, and `assembleProtocol` out of the branch: object-container files and the Avro RPC protocol are neither the event format nor a branch concern.

[RAIL_LAW]:
- Package: `avsc`
- Owns: Avro schema compilation, the binary codec pair, validation with per-path evidence, writer-to-reader resolution, canonical schema fingerprinting, and the node object-container streams
- Accept: one contract-bound module-scope `Type`, explicit `wrapUnions`, bounded octets, `Either.try` at the arm, `Schema` landing, `fingerprint` identity, `createResolver` evolution
- Reject: inferred schemas, per-call type minting, `Buffer` in domain shapes, an `any` decode read, `avsc/lib/*` deep paths, object-container framing, the Avro RPC service surface
