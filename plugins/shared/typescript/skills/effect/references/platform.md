# [PLATFORM]

## [01]-[FILES]

`FileSystem` and `Path` are services a platform layer provides, and file content enters as a byte `Stream` that a codec of `effect/encoding` decodes in the same pipe:

```ts
const documented = (file: string) => Stream.unwrap(FileSystem.FileSystem.useSync((fs) => fs.stream(file))).pipe(Stream.pipeThroughChannel(Ndjson.decodeSchema(DocumentationRow)()));
```

`DocumentationRow` is the schema of one line. Each member reads or writes through one handle the scope owns:
- `fs.stream(file, { offset, bytesToRead })` reads a range, and `Stream.run(bytes, fs.sink(file))` closes its file when the stream ends
- `fs.readDirectoryEntries(folder)` returns each name with its type, where `readDirectory` returns names alone
- `fs.makeTempFileScoped({ suffix })` and `fs.makeTempDirectoryScoped()` delete their path when the owning scope closes
- `fs.watch(path, { recursive, onReady })` streams `Create`, `Update`, and `Remove`, and `onReady` runs once the watcher buffers events
- `NodeStream.fromReadable({ evaluate, onError })` enters a Node readable as a `Stream`

Paths and patterns come from the package, never from text tests:
- `path.resolveWithin(root, input)` returns `Option.none()` for input leaving `root` or holding its own root, and follows no symlink
- `fs.glob(pattern, { root, exclude, dot })` walks depth first with each folder sorted by name, and reads no excluded folder
- `Glob.make(pattern, { dot })` compiles once for `Glob.match(glob, path)` with no disk read, in place of `node:path` `matchesGlob`
- `Glob.escape(text)` matches a name literally, and leading segments equal to their escaped form are a pattern's literal base

Engines with a file API and no Node runtime build `FileSystem` and `Path` from their primitives:
- `FileSystem.make(FileSystem.makeNoop(primitives))` derives `exists`, `glob`, `readFileString`, `stream`, `sink`, and `writeFileString`
- Derived members read primitives: `glob` walks `readDirectoryEntries`, `exists` reads `access`, `stream` and `sink` call `open`
- `makeNoop` fails each omitted member with `NotFound`, dies on temporary paths and `makeDirectory`, and succeeds `remove` with no effect
- Options an engine refuses fail with `PlatformError.badArgument` naming the method, as an omitted member reads as a missing file
- `Path.make({ ...base, resolve })` derives `resolveWithin` from the given `parse`, `resolve`, `relative`, `isAbsolute`, and `sep`
- `FileSystem.layerNoop(members)` provides the `makeNoop` form as a layer, with no member derived

## [02]-[ENCODING]

`effect/encoding` holds one codec per format, and the outcome a parser returns selects how it enters an effect:

| [INDEX] | [FORMAT]                | [MEMBER]                                          | [OUTCOME]                       |
| :-----: | :---------------------- | :------------------------------------------------ | :------------------------------ |
|  [01]   | Hex, Base64, Base64 URL | `Hex.decode`, `Base64.decode`, `Base64Url.decode` | `Result` for `fromResult`       |
|  [02]   | TOML, JSONC, JSON5      | `Toml.parse`, `Jsonc.parse`, `Json5.parse`        | `Result` for `fromResult`       |
|  [03]   | YAML                    | `Yaml.parse`                                      | `SyntaxError` for `Effect.try`  |
|  [04]   | INI                     | `Ini.parse`                                       | Total                           |
|  [05]   | Lines, CSV, JSON array  | `Ndjson`, `Csv`, `JsonArray` `decodeSchema(S)`    | Channel of decoded records      |
|  [06]   | Server-sent events      | `Sse.decodeSchema`, `Sse.decodeDataSchema`        | Channel of decoded events       |
|  [07]   | XML                     | `Xml.decodeString` through `Xml.elements(path)`   | Channel of element trees        |
|  [08]   | Tar, Zip                | `Tar.decode`, `Zip.decode`                        | Channel of entries with streams |

Channels enter a stream through `Stream.pipeThroughChannel`, `decodeSchema` reading bytes and `decodeSchemaString` text. CSV headers a source names map to fields through `Schema.encodeKeys` on the row:

```ts
const ayahs = (csv: string, headed: { readonly [K in keyof typeof AyahRow.fields]: string }) => Stream.make(csv).pipe(Stream.pipeThroughChannel(Csv.decodeSchemaString(AyahRow.pipe(Schema.encodeKeys(headed)))({ headers: true, ignoreEmptyLines: true })), Stream.runCollect);
```

`AyahRow` is a struct of `surah`, `number`, and `text`. `headers: true` keys each record by the first row, and an empty cell for an optional field decodes as a missing key:
- Each format holds an encoder beside its decoder (`Ndjson.encodeSchema(S)`, `Csv.encodeSchemaString(S)`, `Xml.encodeString`)
- `Xml.toEvents(element)` turns an `Xml.Element` tree into the events `Xml.encodeString` writes, unindented
- `Schema.Uint8ArrayFromHex` and `Schema.Uint8ArrayFromBase64` decode bytes inside a record, where the table's members decode a lone value
- `SchemaBinary.toCodec(S)` encodes a schema's values as bytes
- `Toml.parse` reads TOML, and `patch` of `@decimalturn/toml-patch` writes an edit keeping a file's layout and comments
- Decoders bound one unit: `Csv` `maxRecordSize` 1 MiB, `Sse` `maxEventSize` 10 MiB, `JsonArray` and `Xml.elements` 16 MiB
- `Ndjson` byte decoders hold any line length until `maxLineBytes` bounds it, a longer line failing with `LineTooLarge`

Archives stream their entries, and no temporary tree holds them:
- `Zip.fileStream(name, fs.stream(file), { mode })` through `Zip.encode()` into `fs.sink(archive)` writes an archive keeping permission bits
- Entries default to mtime 1980-01-01 and mode `0o644` (`0o755` for folders), so equal input writes an equal archive
- `Zip.fileStream` deflates by default, a `Store` entry takes `size`, and `Zip.file` writes in-memory content's sizes in its local header
- `Zip.decode()` returns paths as written (absolute, `..`, backslashes), and `path.resolveWithin(root, header.path)` places one before a write
- `Tar.file`, `Tar.fileStream`, `Tar.encode`, and `Tar.decode` take the same forms

## [03]-[CRYPTO]

`effect/crypto` holds one service per primitive over WebCrypto, each taking an algorithm value and an algorithm-typed key:

| [INDEX] | [PRIMITIVE]            | [SERVICE]                          | [KEY]                                  |
| :-----: | :--------------------- | :--------------------------------- | :------------------------------------- |
|  [01]   | Hash                   | `Digest` (`Digest.sha256`)         | None                                   |
|  [02]   | Message tag            | `Mac` (`Mac.hmacSha256`)           | `Mac.key(algorithm, bytes)`            |
|  [03]   | Authenticated cipher   | `Aead` (`Aead.aes256Gcm`)          | `Aead.key(algorithm, bytes)`           |
|  [04]   | Unauthenticated cipher | `Cipher` (`Cipher.aes256Ctr`)      | `Cipher.key(algorithm, bytes)`         |
|  [05]   | Key wrapping           | `KeyWrap` (`aes256Kw`, `rsaOaep`)  | `KeyWrap.key`, RSA key pair            |
|  [06]   | Signature              | `Signature` (`ed25519`)            | `Signature.privateKey`, `publicKey`    |
|  [07]   | Shared secret          | `KeyAgreement` (`x25519`)          | `KeyAgreement.privateKey`, `publicKey` |
|  [08]   | Key derivation         | `Kdf` (`hkdfSha256`, `argon2id()`) | Input bytes                            |
|  [09]   | Password storage       | `Password.layer({ algorithm })`    | PHC string                             |
|  [10]   | Random bytes and ids   | `SecureRandom` (`uuidV7`, `ulid`)  | None                                   |

`NodeCrypto.layer` provides every service on Node and requires nothing, and `NodeCrypto.layer<Service>` provides one. Every `layerWebCrypto` except `Digest` and `Kdf` requires `SecureRandom.layerWebCrypto`. A webhook body verifies against its shared secret:

```ts
const verified = Effect.fn('verified')(function* (secret: Uint8Array, signature: string, body: Uint8Array) {
    const [mac, key, tag] = yield* Effect.all([Mac.Mac, Mac.key(Mac.hmacSha256, secret), Effect.fromResult(Hex.decode(signature))]);
    return yield* mac.verify(key, tag, body);
});
```

`mac.verify` compares in constant time and returns `false` for a tag of the wrong length, and `Mac.timingSafeEqual` compares bytes a caller holds:
- `Mac.key` copies its bytes and fails an empty key with an `InvalidKey` reason
- `Signature.privateKey(Signature.ed25519, seed)` takes a 32-byte seed, and `signer.publicKeyOf(privateKey)` derives the verifying key
- `KeyFormat.decodePublic(format, algorithm, data)` reads `spki`, `jwk`, or `pem(spki)`, `decodePrivate` reads `pkcs8`, `pkcs1`, `sec1`, or `jwk`
- `KeyFormat.encode(format, key)` writes a key, and `KeyFormat.decodeSecret(KeyFormat.jwk, algorithm, jwk)` reads a secret key
- `Kdf.layerWebCrypto` fails Argon2id with an `Unsupported` reason, and `NodeCrypto.layerKdf` supplies it where `node:crypto` holds `argon2`
- `Password.layer` requires `Kdf` and `SecureRandom`, and `needsRehash` after a passing `verify` marks a hash of another algorithm or cost
- `WebCrypto.layer(crypto)` replaces the `Crypto` object every `layerWebCrypto` reads

## [04]-[TOKENS]

`effect/jose` signs, verifies, and encrypts JSON Web Tokens, decoding claims through a schema after the registered claims pass the policy. A key set an issuer publishes verifies its tokens:

```ts
const verified = (token: string, issuer: string, audience: string) => Jwt.verifyWithKeySet(Claims, token, { issuer, audience, algorithms: ['RS256'] });

const KeySet = (url: string) =>
    Layer.mergeAll(
        Jwks.layerRemote({ url }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))),
        Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto)),
    );
```

`Claims` is the claims schema and `url` the issuer's key set address from configuration. `algorithms` lists every accepted algorithm and takes at least one:
- `Jwks.layerRemote` accepts a 200 response from `url` alone, and `redirect: 'manual'` stops `fetch` sending the redirected request
- Remote sets refetch after `timeToLive` (10 minutes) and for an unknown `kid` at most once per `cooldown` (30 seconds)
- Policies take `issuer`, `audience`, `subject`, `typ`, `clockTolerance`, `maxTokenAge`, and `requiredClaims`
- Claims failing a policy fail with a `ClaimValidationFailed` reason naming the claim
- `Jwt.sign` and `Jwt.verify` take one key, `Jwk.importSecret(jwk, 'HS256')` reading `Mac` and a private key reading `Signature`
- `Jwks.layerLocal(keySet)` holds a fixed set, and `Jwt.decodeUnverified` reads claims for choosing a key, never for a decision

State a server hands out and reads back seals as a compact `dir` JWE bound to its consumer, in place of hand AEAD framing (nonce, `aad`, concatenation, base64url):
- `Jwk.generateSecret('dir')` then `Jwk.importSecret(secret, 'dir')` build a key once at layer build, reading `SecureRandom`
- `Jwt.encrypt(Schema, claims, key)` returns `Redacted<string>` and requires `Aead` and `SecureRandom`, captured once by `Effect.context`
- `Jwt.decrypt(Schema, sealed, key, { audience })` fails a token sealed for another `aud`, and takes a key resolver for rotation

## [05]-[NETWORK]

`effect/net` holds address values and name resolution apart from any socket:
- `NetAddress` parses and formats IP, socket, and Unix path addresses, `IpNetwork` CIDR prefixes with `contains`, and `Host` host and port
- `AddressResolver.resolve(hostPort)` returns every socket address, and a server binds each (`localhost` as `127.0.0.1` and `::1`)
- Resolvers look a domain name up through `Dns`, read a numeric host with no lookup, and skip IPv6 link-local results
- `NodeAddressResolver.layer` requires `Dns`, which `NodeDns.layer` or `NodeDns.layerConfig` supplies, and `Dns.layerStatic(zone)` holds fixed names
- `NetAddress.isLoopback` over `Host.hostPortFromString` and `Host.parseHostsFile` decides locality, in place of a hand loopback set
- `NetAddress.loopbackAddresses` through `NetAddress.formatUrlHost` lists loopback hosts for a `Host` allow-list, IPv6 bracketed
