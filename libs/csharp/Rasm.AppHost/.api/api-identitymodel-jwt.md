# [RASM_APPHOST_API_IDENTITYMODEL_JWT]

`Microsoft.IdentityModel.JsonWebTokens` supplies the allocation-conscious JWT engine: `JsonWebTokenHandler` creates, signs, encrypts (JWE), and validates JWS/JWE compact tokens, and `JsonWebToken` is the parsed token with typed, span-friendly header/payload accessors. It is the token leg that peers with the `OpenIddict.Client` acquisition rail: where the client obtains tokens, this handler reads and validates them against the `Microsoft.IdentityModel.Tokens` validation contract (`TokenValidationParameters`/`TokenValidationResult` and `ValidationParameters`/`ValidationResult<,>`), keyed by the signing material that `Microsoft.IdentityModel.Protocols.OpenIdConnect` discovery feeds into `TokenValidationParameters.ConfigurationManager`. It succeeds the legacy `System.IdentityModel.Tokens.Jwt` `JwtSecurityTokenHandler`; this package is the only JWT handler the host admits.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.JsonWebTokens`

- package: `Microsoft.IdentityModel.JsonWebTokens`
- assembly: `Microsoft.IdentityModel.JsonWebTokens`
- namespace: `Microsoft.IdentityModel.JsonWebTokens`
- asset: runtime library
- license: MIT
- abi: native `lib/net10.0` asset (consumer-bound); also `net9.0`/`net8.0`/`net6.0`/`netstandard2.0`/`net462`/`net472`
- depends: `Microsoft.IdentityModel.Tokens` (validation contract, keys, credentials, `TokenValidationResult`)
- rail: jwt

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handler, parsed token, and constants — `Microsoft.IdentityModel.JsonWebTokens`

- rail: jwt

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [RAIL]                                      |
| :-----: | :------------------------ | :--------------- | :------------------------------------------ |
|  [01]   | `JsonWebTokenHandler`     | handler class    | create / sign / encrypt / read / validate   |
|  [02]   | `JsonWebToken`            | parsed token     | `SecurityToken` with typed header/payload   |
|  [03]   | `JwtTokenUtilities`       | static utility   | signature, key bytes, decryption-key gather |
|  [04]   | `JwtRegisteredClaimNames` | constant struct  | RFC 7519 / OIDC claim-name literals         |
|  [05]   | `JwtHeaderParameterNames` | constant struct  | JOSE header-parameter literals              |
|  [06]   | `JwtConstants`            | static constants | JWS/JWE token-type and delimiter constants  |
|  [07]   | `JsonClaimValueTypes`     | static constants | JSON claim value-type URIs                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token creation — `JsonWebTokenHandler`

- rail: jwt

The descriptor overload is the canonical creation entry; the `payload` string overloads are the low-level escape hatch. Signing, encryption, and compression compose into signed-only (JWS), encrypted-only (JWE), or nested signed-then-encrypted tokens, with optional `additionalHeaderClaims`/`additionalInnerHeaderClaims`.

| [INDEX] | [SURFACE]                                                                                        | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `CreateToken(SecurityTokenDescriptor tokenDescriptor)`                                           | descriptor creation     |
|  [02]   | `CreateToken(string payload, SigningCredentials signingCredentials)`                             | raw-payload JWS         |
|  [03]   | `CreateToken(string payload, EncryptingCredentials encryptingCredentials)`                       | raw-payload JWE         |
|  [04]   | `CreateToken(string payload, SigningCredentials, EncryptingCredentials)`                         | nested token            |
|  [05]   | `CreateToken(string payload, SigningCredentials, EncryptingCredentials, string compressionAlgo)` | compressed nested token |
|  [06]   | `CreateToken(string payload, …, IDictionary<string,object> additionalHeaderClaims)`              | custom JOSE headers     |
|  [07]   | `EncryptToken(string innerJwt, EncryptingCredentials encryptingCredentials)`                     | existing-JWT encryption |
|  [08]   | `EncryptToken(string innerJwt, EncryptingCredentials, string algorithm)`                         | explicit-alg encryption |

[ENTRYPOINT_SCOPE]: reading and validation — `JsonWebTokenHandler`

- rail: jwt

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]             |
| :-----: | :------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `bool CanReadToken(string token)`                                          | structural probe         |
|  [02]   | `JsonWebToken ReadJsonWebToken(string token)`                              | unvalidated parse        |
|  [03]   | `JsonWebToken ReadJsonWebToken(ReadOnlyMemory<char> token)`                | zero-copy parse          |
|  [04]   | `SecurityToken ReadToken(string token)`                                    | base-type parse          |
|  [05]   | `Task<TokenValidationResult> ValidateTokenAsync(string token, TVP)`        | async validation         |
|  [06]   | `Task<TokenValidationResult> ValidateTokenAsync(SecurityToken token, TVP)` | parsed-token validation  |
|  [07]   | `TokenValidationResult ValidateToken(string token, TVP)`                   | synchronous validation   |
|  [08]   | `string DecryptToken(JsonWebToken jwtToken, TVP)`                          | inner-JWS decryption     |
|  [09]   | `Task<string> DecryptTokenWithConfigurationAsync(JsonWebToken, TVP, CT)`   | discovery-key decryption |

[ENTRYPOINT_SCOPE]: typed claim/header access — `JsonWebToken`

- rail: jwt

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]    | [RAIL]                                    |
| :-----: | :------------------------------------------------------ | :---------------- | :---------------------------------------- |
|  [01]   | `T GetPayloadValue<T>(string key)`                      | typed payload     | strongly-typed claim read (throws)        |
|  [02]   | `bool TryGetPayloadValue<T>(string key, out T value)`   | typed payload try | non-throwing typed claim read             |
|  [03]   | `T GetHeaderValue<T>(string key)`                       | typed header      | strongly-typed JOSE header read           |
|  [04]   | `bool TryGetHeaderValue<T>(string key, out T value)`    | typed header try  | non-throwing typed header read            |
|  [05]   | `Claim GetClaim(string key)` / `bool TryGetClaim`       | claim access      | `System.Security.Claims.Claim` projection |
|  [06]   | `IEnumerable<Claim> Claims`                             | claim enumeration | all payload claims as `Claim`             |
|  [07]   | `Subject` / `Audiences` / `Issuer` / `Azp` / `Actor`    | typed registered  | parsed RFC 7519 claim properties          |
|  [08]   | `ValidFrom` / `ValidTo` / `IssuedAt` / `Id`             | typed temporal    | parsed `nbf`/`exp`/`iat`/`jti`            |
|  [09]   | `Alg` / `Enc` / `Kid` / `Typ` / `Cty` / `Zip` / `X5t`   | typed JOSE        | parsed header parameters                  |
|  [10]   | `IsEncrypted` / `IsSigned` / `InnerToken`               | structural state  | JWE/JWS discrimination + nested token     |
|  [11]   | `EncodedHeader` / `EncodedPayload` / `EncodedSignature` | raw segments      | base64url segments for re-emission        |

## [04]-[IMPLEMENTATION_LAW]

[JWT_TOPOLOGY]:

- handler contract: `JsonWebTokenHandler : TokenHandler, IResultBasedValidation`; `ValidateTokenAsync` is the async-first path and `Task<TokenValidationResult>` is the result carrier (`IsValid`, `ClaimsIdentity`, `SecurityToken`, `Issuer`, `TokenType`, `Exception`, `Claims`) — failures land on `Exception`, never thrown from the validate path
- claim mapping: `MapInboundClaims` defaults to the static `DefaultMapInboundClaims`; set `false` to keep short JWT claim types (`sub`, `azp`) instead of the long ClaimTypes URIs — `InboundClaimTypeMap` is the per-handler remap table
- creation algebra: `SecurityTokenDescriptor` (from the Tokens package) is the declarative creation input carrying `Subject`/`Claims`, `Issuer`, `Audience(s)`, `Expires`/`NotBefore`/`IssuedAt`, `SigningCredentials`, `EncryptingCredentials`, and `CompressionAlgorithm`; `IncludeKeyIdInHeader` and `SetDefaultTimesOnTokenCreation` control header `kid` emission and automatic lifetime stamping
- JWS vs JWE: presence of `SigningCredentials` yields a signed token, `EncryptingCredentials` yields an encrypted token, both yield a nested signed-then-encrypted token; `JsonWebToken.IsSigned`/`IsEncrypted` and `InnerToken` reflect the structure on read
- typed access: `GetPayloadValue<T>`/`GetHeaderValue<T>` deserialize directly from the parsed `Utf8JsonReader`-backed payload; the `Try*` mirrors return `false` instead of throwing for absent or type-mismatched claims, the form the host's typed-result rail consumes
- span path: `ReadJsonWebToken(ReadOnlyMemory<char>)` and the `JsonWebToken(ReadOnlyMemory<char>, …)` constructors parse without copying the token into a new string; the `TryReadJwtClaim` delegate (from the Tokens package) customizes per-claim deserialization
- decryption + discovery: `DecryptTokenWithConfigurationAsync` resolves decryption keys from the `BaseConfigurationManager` on `TokenValidationParameters` — concretely the `ConfigurationManager<OpenIdConnectConfiguration>` from `Microsoft.IdentityModel.Protocols` (cataloged at `api-identitymodel-protocols.md`) that the OIDC discovery rail refreshes — so encrypted-token keys follow JWKS rotation

[LOCAL_ADMISSION]:

- Resolve or construct one `JsonWebTokenHandler` (it is thread-safe and reusable); never instantiate the legacy `JwtSecurityTokenHandler`.
- Validate inbound tokens through `ValidateTokenAsync(token, validationParameters)` and branch on `TokenValidationResult.IsValid`, projecting `Exception` into the host failure rail and `ClaimsIdentity` into the `Microsoft.AspNetCore.Authorization` evaluation core — never read claims before `IsValid` is confirmed.
- Set `MapInboundClaims = false` so authorization requirements match the raw JWT claim types the OIDC provider emits.
- Read claims through `TryGetPayloadValue<T>`/typed registered properties (`Subject`, `Audiences`, `ValidTo`), not by enumerating `Claims` and string-parsing.
- Feed `TokenValidationParameters.ConfigurationManager` from the OIDC discovery rail so `IssuerSigningKeys` and decryption keys track JWKS rotation; do not pin a static `IssuerSigningKey` for a rotating provider.
- Create host-issued tokens through `CreateToken(SecurityTokenDescriptor)` with `SigningCredentials` (and `EncryptingCredentials` for confidential payloads); supply `Claims`/`Subject` and lifetime on the descriptor rather than hand-assembling a payload string.

[RAIL_LAW]:

- Package: `Microsoft.IdentityModel.JsonWebTokens`
- Owns: JWS/JWE creation, signing, encryption, compression, parsing, and validation of compact JWTs
- Accept: `JsonWebTokenHandler` over `SecurityTokenDescriptor` (create) and `TokenValidationParameters`/`ValidationParameters` (validate); typed claim/header access through `JsonWebToken`
- Reject: the legacy `JwtSecurityTokenHandler`, hand-rolled base64url JWT splitting/JSON parsing, reading claims before `IsValid`, string-keyed claim parsing where a typed `Get*Value<T>` exists
