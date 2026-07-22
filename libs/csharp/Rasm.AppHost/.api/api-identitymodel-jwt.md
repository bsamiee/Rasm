# [RASM_APPHOST_API_IDENTITYMODEL_JWT]

`Microsoft.IdentityModel.JsonWebTokens` owns the allocation-conscious JWT engine: `JsonWebTokenHandler` creates, signs, encrypts, and validates JWS/JWE compact tokens, and `JsonWebToken` parses one into typed, span-friendly header and payload accessors. That handler validates inbound tokens against the `Microsoft.IdentityModel.Tokens` validation contract and mints host-issued tokens from a `SecurityTokenDescriptor`, serving the AppHost auth rail its token-reading leg.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.JsonWebTokens`
- package: `Microsoft.IdentityModel.JsonWebTokens` (MIT)
- assembly: `Microsoft.IdentityModel.JsonWebTokens`
- namespace: `Microsoft.IdentityModel.JsonWebTokens`
- asset: pure-managed runtime library; net10.0 assembly, no native/RID asset
- depends: `Microsoft.IdentityModel.Tokens` (validation contract, keys, credentials, `TokenValidationResult`)
- rail: jwt

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handler, parsed token, and constants

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                |
| :-----: | :------------------------ | :-------------- | :------------------------------------------ |
|  [01]   | `JsonWebTokenHandler`     | handler class   | create / sign / encrypt / read / validate   |
|  [02]   | `JsonWebToken`            | parsed token    | `SecurityToken` with typed header/payload   |
|  [03]   | `JwtTokenUtilities`       | utility class   | signature, key bytes, decryption-key gather |
|  [04]   | `JwtRegisteredClaimNames` | constant struct | RFC 7519 / OIDC claim-name literals         |
|  [05]   | `JwtHeaderParameterNames` | constant struct | JOSE header-parameter literals              |
|  [06]   | `JwtConstants`            | constants class | JWS/JWE token-type and delimiter constants  |
|  [07]   | `JsonClaimValueTypes`     | constants class | JSON claim value-type URIs                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token creation — `JsonWebTokenHandler`

`CreateToken(SecurityTokenDescriptor)` is the canonical entry; the `payload`-string overloads are the low-level escape hatch composing signing, encryption, and compression into JWS, JWE, or nested tokens.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `CreateToken(SecurityTokenDescriptor)`                                   | instance | descriptor creation     |
|  [02]   | `CreateToken(string, SigningCredentials)`                                | instance | raw-payload JWS         |
|  [03]   | `CreateToken(string, EncryptingCredentials)`                             | instance | raw-payload JWE         |
|  [04]   | `CreateToken(string, SigningCredentials, EncryptingCredentials)`         | instance | nested token            |
|  [05]   | `CreateToken(string, SigningCredentials, EncryptingCredentials, string)` | instance | compressed nested token |
|  [06]   | `CreateToken(string, …, IDictionary<string, object>)`                    | instance | custom JOSE headers     |
|  [07]   | `EncryptToken(string, EncryptingCredentials)`                            | instance | existing-JWT encryption |
|  [08]   | `EncryptToken(string, EncryptingCredentials, string)`                    | instance | explicit-alg encryption |

[ENTRYPOINT_SCOPE]: reading and validation — `JsonWebTokenHandler`

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]             |
| :-----: | :-------------------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `CanReadToken(string) -> bool`                                              | instance | structural probe         |
|  [02]   | `ReadJsonWebToken(string) -> JsonWebToken`                                  | instance | unvalidated parse        |
|  [03]   | `ReadJsonWebToken(ReadOnlyMemory<char>)`                                    | instance | zero-copy parse          |
|  [04]   | `ReadToken(string) -> SecurityToken`                                        | instance | base-type parse          |
|  [05]   | `ValidateTokenAsync(string, TVP) -> Task<TokenValidationResult>`            | instance | async validation         |
|  [06]   | `ValidateTokenAsync(SecurityToken, TVP)`                                    | instance | parsed-token validation  |
|  [07]   | `ValidateToken(string, TVP) -> TokenValidationResult`                       | instance | synchronous validation   |
|  [08]   | `DecryptToken(JsonWebToken, TVP) -> string`                                 | instance | inner-JWS decryption     |
|  [09]   | `DecryptTokenWithConfigurationAsync(JsonWebToken, TVP, CT) -> Task<string>` | instance | discovery-key decryption |

[ENTRYPOINT_SCOPE]: typed claim and header access — `JsonWebToken`

`Get*Value<T>` deserializes directly from the parsed payload; the `Try*` mirror returns `false` for an absent or type-mismatched claim rather than throwing.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `GetPayloadValue<T>(string) -> T`              | instance | typed claim read, throws on absent        |
|  [02]   | `TryGetPayloadValue<T>(string, out T) -> bool` | instance | non-throwing typed claim read             |
|  [03]   | `GetHeaderValue<T>(string) -> T`               | instance | typed JOSE header read, throws            |
|  [04]   | `TryGetHeaderValue<T>(string, out T) -> bool`  | instance | non-throwing typed header read            |
|  [05]   | `GetClaim(string) -> Claim`                    | instance | `System.Security.Claims.Claim` projection |
|  [06]   | `TryGetClaim(string, out Claim) -> bool`       | instance | non-throwing claim projection             |
|  [07]   | `Claims -> IEnumerable<Claim>`                 | property | all payload claims as `Claim`             |

- `[REGISTERED]`: `Subject` `Audiences` `Issuer` `Azp` `Actor` — parsed RFC 7519 claim properties
- `[TEMPORAL]`: `ValidFrom` `ValidTo` `IssuedAt` `Id` — parsed `nbf`/`exp`/`iat`/`jti`
- `[JOSE]`: `Alg` `Enc` `Kid` `Typ` `Cty` `Zip` `X5t` — parsed header parameters
- `[STRUCTURAL]`: `IsEncrypted` `IsSigned` `InnerToken` — JWE/JWS discrimination and nested token
- `[SEGMENTS]`: `EncodedHeader` `EncodedPayload` `EncodedSignature` — base64url segments for re-emission

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- handler contract: `JsonWebTokenHandler : TokenHandler, IResultBasedValidation`; `ValidateTokenAsync` is the async-first path returning `Task<TokenValidationResult>` (`IsValid`, `ClaimsIdentity`, `SecurityToken`, `Issuer`, `TokenType`, `Exception`, `Claims`) — a validation failure lands on `Exception`, never thrown from the validate path
- claim mapping: `MapInboundClaims` gates whether inbound claim types remap through `InboundClaimTypeMap` to the long ClaimTypes URIs; left unmapped, the raw short JWT names (`sub`, `azp`) survive as claim types
- creation algebra: `SecurityTokenDescriptor` (owned by `Microsoft.IdentityModel.Tokens`) is the declarative creation input carrying `Subject`/`Claims`, `Issuer`, `Audience(s)`, `Expires`/`NotBefore`/`IssuedAt`, `SigningCredentials`, `EncryptingCredentials`, and `CompressionAlgorithm`; `IncludeKeyIdInHeader` and `SetDefaultTimesOnTokenCreation` drive `kid` emission and automatic lifetime stamping
- JWS versus JWE: `SigningCredentials` yields a signed token, `EncryptingCredentials` an encrypted token, both a nested signed-then-encrypted token; `JsonWebToken.IsSigned`/`IsEncrypted` and `InnerToken` reflect the structure on read
- span path: `ReadJsonWebToken(ReadOnlyMemory<char>)` and the `JsonWebToken(ReadOnlyMemory<char>, …)` constructors parse without copying the token into a new string; the `TryReadJwtClaim` delegate (from the Tokens package) customizes per-claim deserialization

[STACKING]:
- `Microsoft.IdentityModel.Tokens`(`.api/api-identitymodel-tokens.md`): `ValidateTokenAsync` reads against `TokenValidationParameters`/`ValidationParameters` and that package's keys, landing every failure on `TokenValidationResult.Exception`; `CreateToken` consumes its `SecurityTokenDescriptor`, `SigningCredentials`, and `EncryptingCredentials`
- `Microsoft.IdentityModel.Protocols`(`.api/api-identitymodel-protocols.md`): the `ConfigurationManager<OpenIdConnectConfiguration>` on `TokenValidationParameters.ConfigurationManager` feeds `IssuerSigningKeys` and the `DecryptTokenWithConfigurationAsync` decryption keys, so both track JWKS rotation
- `OpenIddict.Client`(`.api/api-openiddict-client.md`): the access and identity tokens `OpenIddictClientService` acquires cross to `ValidateTokenAsync` — that catalog obtains, this handler reads and validates
- within-lib: the AppHost auth composition resolves one thread-safe `JsonWebTokenHandler`, branches on `TokenValidationResult.IsValid`, and projects `ClaimsIdentity` into the `Microsoft.AspNetCore.Authorization` core under `MapInboundClaims = false`

[LOCAL_ADMISSION]:
- Resolve one `JsonWebTokenHandler` per host; it is thread-safe and reusable across concurrent validations.
- Validate through `ValidateTokenAsync`, branch on `TokenValidationResult.IsValid`, and only then project `Exception` onto the host failure rail and `ClaimsIdentity` into the `Microsoft.AspNetCore.Authorization` core.
- Set `MapInboundClaims = false` so authorization requirements match the raw JWT claim types the OIDC provider emits.
- Read claims through `TryGetPayloadValue<T>` and the typed registered properties (`Subject`, `Audiences`, `ValidTo`).
- Feed `TokenValidationParameters.ConfigurationManager` from the OIDC discovery rail so `IssuerSigningKeys` and decryption keys track JWKS rotation.
- Mint host-issued tokens through `CreateToken(SecurityTokenDescriptor)` with `SigningCredentials` (and `EncryptingCredentials` for confidential payloads), supplying `Claims`/`Subject` and lifetime on the descriptor rather than hand-assembling a payload string.

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.JsonWebTokens`
- Owns: JWS/JWE creation, signing, encryption, compression, parsing, and validation of compact JWTs
- Accept: `JsonWebTokenHandler` over `SecurityTokenDescriptor` (create) and `TokenValidationParameters`/`ValidationParameters` (validate); typed claim and header access through `JsonWebToken`
- Reject: the legacy `JwtSecurityTokenHandler`, hand-rolled base64url JWT splitting or JSON parsing, reading claims before `IsValid`, string-keyed claim parsing where a typed `Get*Value<T>` exists
