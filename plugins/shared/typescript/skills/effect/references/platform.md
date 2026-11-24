# [PLATFORM]

## [01]-[FILES]

`FileSystem` and `Path` are services a platform layer provides, and file content enters as a byte `Stream` that a codec of `effect/encoding` decodes in the same pipe:

```ts
const entries = (path: string) => Stream.unwrap(FileSystem.FileSystem.useSync((fs) => fs.stream(path))).pipe(Stream.pipeThroughChannel(Ndjson.decodeSchema(Entry)()));
```

`Entry` is a schema class of one line:
- `fs.sink(path)` writes a stream, and `Stream.run(stream, fs.sink(path))` closes its file when the stream ends
- `fs.makeTempFileScoped({ suffix })` and `fs.makeTempDirectoryScoped()` delete their path when the owning scope closes
- `fs.glob(pattern, { root, exclude, dot })` returns sorted paths, and `Glob.make(pattern)` with `Glob.match` tests a path without the disk
- `fs.watch(path)` streams changes, and `NodeStream.fromReadable({ evaluate, onError })` enters a Node readable as a `Stream`
- `FileSystem.layerNoop({ ...overrides })` stubs the service for a test

## [02]-[ENCODING]

`effect/encoding` holds one codec per format, and the outcome a parser returns selects how it enters an effect:

| [INDEX] | [FORMAT]                | [MEMBER]                                          | [OUTCOME]                  |
| :-----: | :---------------------- | :------------------------------------------------ | :------------------------- |
|  [01]   | Hex, Base64, Base64 URL | `Hex.decode`, `Base64.decode`, `Base64Url.decode` | `Result` for `fromResult`  |
|  [02]   | TOML, JSONC, JSON5      | `Toml.parse`, `Jsonc.parse`, `Json5.parse`        | `Result` for `fromResult`  |
|  [03]   | YAML                    | `Yaml.parse`                                      | Throws `SyntaxError`       |
|  [04]   | INI                     | `Ini.parse`                                       | Total                      |
|  [05]   | Lines, CSV, JSON array  | `Ndjson`, `Csv`, `JsonArray` `decodeSchema(S)`    | Channel of decoded records |
|  [06]   | Server-sent events      | `Sse.decodeDataSchema`                            | Channel of decoded events  |
|  [07]   | XML                     | `Xml.decodeString` through `Xml.elements(path)`   | Channel of element trees   |
|  [08]   | Tar, Zip                | `Tar.decode`, `Zip.decode`                        | Channel of archive entries |

Channels enter a stream through `Stream.pipeThroughChannel`, `decodeSchema` reading bytes and `decodeSchemaString` text. CSV headers a caller names map to fields through `Schema.encodeKeys` on the row:

```ts
const rows = (text: string) => Stream.make(text).pipe(Stream.pipeThroughChannel(Csv.decodeSchemaString(Row.pipe(Schema.encodeKeys({ sku: 'SKU', quantity: 'Qty' })))({ headers: true })), Stream.runCollect);
```

`Row` is a schema class. `headers: true` keys each record by the first row, and an empty cell for an optional field decodes as a missing key:
- Each format holds an encoder beside its decoder (`Ndjson.encodeSchema(S)`, `Csv.encodeSchemaString(S)`, `Xml.encodeString`)
- `Xml.toEvents(element)` turns an `Xml.Element` tree into the events `Xml.encodeString` writes, unindented
- `Schema.Uint8ArrayFromHex` and `Schema.Uint8ArrayFromBase64` decode bytes inside a record, where the table's members decode a lone value
- `SchemaBinary.toCodec(S)` encodes a schema's values as bytes

## [03]-[CRYPTO]

`effect/crypto` holds one service per primitive over WebCrypto, each taking an algorithm value and an algorithm-typed key:

| [INDEX] | [PRIMITIVE]          | [SERVICE]                        | [KEY]                                  |
| :-----: | :------------------- | :------------------------------- | :------------------------------------- |
|  [01]   | Hash                 | `Digest` (`Digest.sha256`)       | None                                   |
|  [02]   | Message tag          | `Mac` (`Mac.hmacSha256`)         | `Mac.key(algorithm, bytes)`            |
|  [03]   | Authenticated cipher | `Aead` (`Aead.aes256Gcm`)        | `Aead.key(algorithm, bytes)`           |
|  [04]   | Signature            | `Signature` (`ed25519`)          | `Signature.privateKey`, `publicKey`    |
|  [05]   | Shared secret        | `KeyAgreement` (`x25519`)        | `KeyAgreement.privateKey`, `publicKey` |
|  [06]   | Key derivation       | `Kdf` (`hkdfSha256`, `argon2id`) | Input bytes                            |
|  [07]   | Password storage     | `Password.layer({ algorithm })`  | PHC string                             |
|  [08]   | Random bytes and ids | `SecureRandom`                   | None                                   |

Services have no default layer, `NodeCrypto.layer<Service>` provides one service on Node, and every `layerWebCrypto` except `Digest` and `Kdf` requires `SecureRandom.layerWebCrypto`. A signed request body verifies against a shared secret:

```ts
const signed = Effect.fn('signed')(function* (secret: Uint8Array, signature: string, body: Uint8Array) {
    const [mac, key, tag] = yield* Effect.all([Mac.Mac, Mac.key(Mac.hmacSha256, secret), Effect.fromResult(Hex.decode(signature))]);
    return yield* mac.verify(key, tag, body);
});
```

`mac.verify` compares in constant time, and `Mac.timingSafeEqual` compares bytes a caller holds:
- `Kdf.layerWebCrypto` fails Argon2id with an `Unsupported` reason, `NodeCrypto.layerKdf` supplying it for `Password`
- `KeyFormat.decodePublic(format, algorithm, data)` reads `spki`, `jwk`, or `pem(spki)`, `decodePrivate` reads `pkcs8`, `pkcs1`, `sec1`, or `jwk`
- `signer.publicKeyOf(privateKey)` derives the verifying key, and `Signature.privateKey(Signature.ed25519, seed)` takes a raw seed
- `WebCrypto.layer(crypto)` replaces the `Crypto` object every `layerWebCrypto` reads

## [04]-[TOKENS]

`effect/jose` signs, verifies, and encrypts JSON Web Tokens over the `Signature` service, and a key set a remote publishes verifies tokens it issued:

```ts
const verified = (token: string, issuer: string, audience: string) => Jwt.verifyWithKeySet(Claims, token, { issuer, audience, algorithms: ['RS256'] });

const Verifier = Layer.mergeAll(Jwks.layerRemote({ url: keys }).pipe(Layer.provide(FetchHttpClient.layer)), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto)));
```

`Claims` is a claims schema and `keys` a key set URL. `algorithms` lists every accepted algorithm and takes at least one:
- `Jwt.sign` and `Jwt.verify` work over one key, and `Jwks.layerLocal(keySet)` holds a fixed set
- `Jwt.encrypt` and `Jwt.decrypt` handle encrypted tokens, and `Jwt.decodeUnverified` reads claims with no check

## [05]-[NETWORK]

`effect/net` holds address values and name resolution apart from any socket:
- `NetAddress` parses and formats IP, socket, and Unix path addresses, `IpNetwork` CIDR prefixes, and `Host` an unresolved host and port
- `AddressResolver` resolves a `Host.HostPort` to every socket address through the `Dns` service
- `NodeAddressResolver.layer` and `NodeDns.layer` supply both services
