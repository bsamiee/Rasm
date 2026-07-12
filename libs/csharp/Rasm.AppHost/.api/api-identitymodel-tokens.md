# [RASM_APPHOST_API_IDENTITYMODEL_TOKENS]

`Microsoft.IdentityModel.Tokens` is the cryptographic and validation substrate under the IdentityModel stack: it owns the security-key hierarchy (symmetric, RSA, ECDSA, X.509, JWK, and post-quantum ML-DSA), signing/encrypting/key-wrap credentials, the crypto-provider factory and cache, and the two parallel token-validation contracts — the established `TokenValidationParameters` + `TokenValidationResult` with throwing-or-result delegate validators, and the result-based `ValidationParameters` + `ValidationResult<TResult, ValidationError>` ROP rail with typed `Validated*` outcomes. It is the contract `Microsoft.IdentityModel.JsonWebTokens` reads against and the slot (`BaseConfigurationManager`) into which `Microsoft.IdentityModel.Protocols.OpenIdConnect` discovery feeds rotating signing keys. No IdentityModel package validates a token without this assembly's parameters and keys.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.Tokens`

- package: `Microsoft.IdentityModel.Tokens`
- assembly: `Microsoft.IdentityModel.Tokens`
- namespace: `Microsoft.IdentityModel.Tokens`
- asset: runtime library
- license: MIT
- abi: native `lib/net10.0` asset (consumer-bound); ML-DSA support binds the `net10.0` `System.Security.Cryptography.MLDsa` type and is absent on the netstandard/legacy facades
- depends: `Microsoft.IdentityModel.Logging`, `Microsoft.IdentityModel.Abstractions`
- rail: tokens-core

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation contracts

- rail: tokens-core

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]     | [RAIL]                 |
| :-----: | :---------------------------------- | :---------------- | :--------------------- |
|  [01]   | `TokenValidationParameters`         | validation policy | throwing delegates     |
|  [02]   | `TokenValidationResult`             | validation result | established projection |
|  [03]   | `ValidationParameters`              | validation policy | result delegates       |
|  [04]   | `ValidationResult<TResult, TError>` | result struct     | ROP outcome            |
|  [05]   | `ValidationError`                   | error base        | non-throwing failure   |
|  [06]   | `ValidatedToken`                    | outcome aggregate | validated facets       |
|  [07]   | `ValidatedIssuer`                   | result struct     | issuer facet           |
|  [08]   | `ValidatedLifetime`                 | result struct     | lifetime facet         |
|  [09]   | `ValidatedSignatureKey`             | result struct     | signature-key facet    |
|  [10]   | `ValidatedTokenType`                | result struct     | token-type facet       |
|  [11]   | `Validators`                        | static validators | both contracts         |
|  [12]   | `ValidationFailureType`             | failure taxonomy  | facet classifier       |
|  [13]   | `TokenHandler`                      | handler base      | token operations       |
|  [14]   | `SecurityTokenHandler`              | XML handler base  | WS-Fed and SAML        |
|  [15]   | `SecurityToken`                     | token base        | parsed-token root      |
|  [16]   | `SecurityTokenDescriptor`           | creation input    | emission descriptor    |

[VALIDATION_TYPE_DETAIL]:

- `TokenValidationResult`: projects `IsValid`, `ClaimsIdentity`, `Exception`, `SecurityToken`, `Issuer`, and `Claims`.
- `ValidationResult<TResult, TError>`: a `readonly record struct` outcome.
- `ValidationError`: carries a stack trace.
- `ValidatedIssuer`, `ValidatedLifetime`, `ValidatedSignatureKey`, `ValidatedTokenType`: per-facet `readonly struct` outcomes.
- `TokenHandler`: roots `ValidateTokenAsync` and `ReadToken`.
- `SecurityTokenHandler`: roots `XmlReader` handlers.

[PUBLIC_TYPE_SCOPE]: keys, credentials, and crypto providers

- rail: tokens-core

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [RAIL]                 |
| :-----: | :-------------------------------- | :------------------ | :--------------------- |
|  [01]   | `SecurityKey`                     | key base            | identity and factory   |
|  [02]   | `SymmetricSecurityKey`            | symmetric key       | HMAC and AES           |
|  [03]   | `RsaSecurityKey`                  | asymmetric key      | signing and encryption |
|  [04]   | `ECDsaSecurityKey`                | asymmetric key      | EC signing             |
|  [05]   | `X509SecurityKey`                 | certificate key     | X.509 material         |
|  [06]   | `MlDsaSecurityKey`                | post-quantum key    | FIPS 204 signatures    |
|  [07]   | `JsonWebKey`                      | JWK                 | RFC 7517 key           |
|  [08]   | `JsonWebKeySet`                   | JWKS                | discovery key set      |
|  [09]   | `JsonWebKeyConverter`             | JWK conversion      | bidirectional bridge   |
|  [10]   | `SigningCredentials`              | signing credential  | algorithm and digest   |
|  [11]   | `EncryptingCredentials`           | encrypt credential  | JWE algorithm pair     |
|  [12]   | `X509SigningCredentials`          | certificate sign    | X.509 material         |
|  [13]   | `X509EncryptingCredentials`       | certificate encrypt | X.509 material         |
|  [14]   | `CryptoProviderFactory`           | provider factory    | provider creation      |
|  [15]   | `CryptoProviderCache`             | provider cache      | validation memoization |
|  [16]   | `InMemoryCryptoProviderCache`     | provider cache      | in-memory memoization  |
|  [17]   | `EcdhKeyExchangeProvider`         | key agreement       | ECDH-ES KDF            |
|  [18]   | `AuthenticatedEncryptionProvider` | AEAD provider       | content encryption     |
|  [19]   | `AuthenticatedEncryptionResult`   | AEAD result         | encryption result      |
|  [20]   | `BaseConfiguration`               | discovery model     | issuer and JWKS        |
|  [21]   | `BaseConfigurationManager`        | discovery slot      | configuration refresh  |

`SecurityKey` carries `KeyId`, `ComputeJwkThumbprint`, and `CryptoProviderFactory`; `JsonWebKeyConverter` bridges `SecurityKey` and `JsonWebKey`. `MlDsaSecurityKey` wraps the BCL `MLDsa` type, and `AuthenticatedEncryptionProvider` owns AES-GCM and AES-CBC-HMAC content encryption.

[PUBLIC_TYPE_SCOPE]: legacy validation exceptions (throwing rail)

- rail: tokens-core

The legacy throwing `Validators.Validate*` overloads and `JsonWebTokenHandler.ValidateTokenAsync` surface these on `TokenValidationResult.Exception`; a consumer pattern-matches the concrete subtype, never a bare `Exception`.

`SecurityTokenException` inherits `Exception`; `SecurityTokenValidationException` inherits `SecurityTokenException`, and each concrete validation exception inherits `SecurityTokenValidationException`.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]  | [FAILURE]          |
| :-----: | :--------------------------------------- | :------------- | :----------------- |
|  [01]   | `SecurityTokenException`                 | exception base | throwing rail      |
|  [02]   | `SecurityTokenValidationException`       | exception base | validation         |
|  [03]   | `SecurityTokenExpiredException`          | exception      | `nbf` or `exp`     |
|  [04]   | `SecurityTokenInvalidSignatureException` | exception      | signature rejected |
|  [05]   | `SecurityTokenInvalidIssuerException`    | exception      | untrusted issuer   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: result-based validation — `Validators` + `ValidationParameters`

- rail: tokens-core

The result rail is pure ROP: every validator returns `ValidationResult<T, ValidationError>` (or its async `Task<…>` mirror) and never throws; `ValidationParameters` carries one `*Delegate` slot per facet that defaults to the matching `Validators` static. The signatures below share the `Validators` owner.

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [RESULT]                |
| :-----: | :--------------------- | :------------- | :---------------------- |
|  [01]   | `ValidateAudience`     | audience       | `string`                |
|  [02]   | `ValidateIssuerAsync`  | issuer         | `ValidatedIssuer`       |
|  [03]   | `ValidateLifetime`     | lifetime       | `ValidatedLifetime`     |
|  [04]   | `ValidateSignatureKey` | signature key  | `ValidatedSignatureKey` |
|  [05]   | `ValidateTokenType`    | token type     | `ValidatedTokenType`    |
|  [06]   | `ValidateAlgorithm`    | algorithm      | `string`                |
|  [07]   | `ValidateTokenReplay`  | replay         | `DateTime?`             |

[VALIDATOR_SIGNATURES]:

- `ValidateAudience(IList<string>, SecurityToken?, ValidationParameters, CallContext)`.
- `ValidateIssuerAsync(string?, SecurityToken?, ValidationParameters, CallContext?, CT)` returns the asynchronous mirror.
- `ValidateLifetime(DateTime?, DateTime?, SecurityToken?, ValidationParameters, CallContext)`.
- `ValidateSignatureKey(SecurityKey, SecurityToken, ValidationParameters, CallContext)`.
- `ValidateTokenType(string?, SecurityToken?, ValidationParameters, CallContext)`.
- `ValidateAlgorithm(string?, SecurityToken, ValidationParameters, CallContext)`.
- `ValidateTokenReplay(DateTime?, string, ValidationParameters, CallContext)`.

[VALIDATION_RESULT]:

- Projection: `ValidationResult<T, E>.Succeeded`, `.Result`, and `.Error` expose the discriminated outcome.
- Lift: implicit `ValidationResult<T, E>(T)` and `(TError)` conversions lift success and error values.

[ENTRYPOINT_SCOPE]: legacy validation contract — `TokenValidationParameters` + `Validators`

- rail: tokens-core

The throwing signatures share the `Validators` owner and abbreviate `TokenValidationParameters` as `TVP`.

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]          |
| :-----: | :-------------------------- | :------------- | :-------------- |
|  [01]   | `ValidateIssuer`            | issuer         | validated value |
|  [02]   | `ValidateAudience`          | audience       | mismatch throw  |
|  [03]   | `ValidateLifetime`          | lifetime       | lifetime throw  |
|  [04]   | `ValidateIssuerSecurityKey` | signing key    | key throw       |
|  [05]   | `Clone`                     | policy clone   | request copy    |

[LEGACY_SIGNATURES]:

- `ValidateIssuer(string, SecurityToken, TVP)` returns the validated issuer or throws.
- `ValidateAudience(IEnumerable<string>, SecurityToken, TVP)` throws on mismatch.
- `ValidateLifetime(DateTime?, DateTime?, SecurityToken, TVP)` throws on an `nbf` or `exp` violation.
- `ValidateIssuerSecurityKey(SecurityKey, SecurityToken, TVP)` throws on a disallowed key.
- `TokenValidationParameters.Clone()` returns a per-request policy copy.

[LEGACY_DELEGATES]: `TokenValidationParameters` carries `IssuerValidator`, `AudienceValidator`, `LifetimeValidator`, `SignatureValidator`, and the remaining per-facet delegate slots.

[LEGACY_RESULT]: `TokenValidationResult` projects `SecurityToken` for a `JsonWebToken` cast, `IsValid`, `ClaimsIdentity`, and `Exception`; pattern matching `TokenValidationResult.Exception` against concrete `SecurityToken*Exception` subtypes classifies failures.

[ENTRYPOINT_SCOPE]: keys, credentials, crypto, and encoding

- rail: tokens-core

`CryptoProviderFactory` owns the provider-creation surfaces in this grid.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]   | [RAIL]            |
| :-----: | :-------------------------------------- | :--------------- | :---------------- |
|  [01]   | `SymmetricSecurityKey`                  | key constructor  | HMAC key          |
|  [02]   | `SigningCredentials`                    | credential ctor  | signing material  |
|  [03]   | `ComputeJwkThumbprint`                  | key identity     | RFC 7638          |
|  [04]   | `KeyId`                                 | key identity     | `kid`             |
|  [05]   | `JsonWebKeySet.Create`                  | JWKS parse       | document parse    |
|  [06]   | `GetSigningKeys`                        | JWKS projection  | signing keys      |
|  [07]   | `JsonWebKeyConverter.ConvertFrom…`      | key conversion   | JWK bridge        |
|  [08]   | `Default`                               | provider factory | shared instance   |
|  [09]   | `CreateForSigning`                      | provider create  | cached signature  |
|  [10]   | `CreateForVerifying`                    | provider create  | verification      |
|  [11]   | `CreateKeyWrapProvider`                 | provider create  | key wrap          |
|  [12]   | `CreateAuthenticatedEncryptionProvider` | provider create  | AEAD              |
|  [13]   | `IsSupportedAlgorithm`                  | capability probe | algorithm and key |
|  [14]   | `GenerateKdf`                           | key agreement    | ECDH-ES           |
|  [15]   | `Base64UrlEncoder.Encode`               | base64url        | span encoding     |
|  [16]   | `Base64UrlEncoder.Decode`               | base64url        | string decoding   |

[CRYPTO_SIGNATURES]:

- Credentials: `new SymmetricSecurityKey(byte[])` and `new SigningCredentials(key, algorithm)`.
- Identity: `SecurityKey.ComputeJwkThumbprint()` and `KeyId`.
- JWKS: `JsonWebKeySet.Create(string json)` and `GetSigningKeys()`.
- Provider: `CreateForSigning(key, alg)`, `CreateForVerifying`, `CreateKeyWrapProvider`, `CreateAuthenticatedEncryptionProvider`, and `IsSupportedAlgorithm(alg, key)`.
- Agreement: `EcdhKeyExchangeProvider.GenerateKdf(apu, apv)`.
- Encoding: `Base64UrlEncoder.Encode(ReadOnlySpan<byte>, Span<char>)` and `Decode(string)` form the zero-allocation JOSE path.

## [04]-[IMPLEMENTATION_LAW]

[TOKENS_TOPOLOGY]:

- two validation rails: the legacy rail is `TokenValidationParameters` (a wide knob bag — `ValidateIssuer`/`ValidateAudience`/`ValidateLifetime`/`ValidateIssuerSigningKey`/`RequireExpirationTime`/`RequireSignedTokens`/`ClockSkew`/`ValidIssuer(s)`/`ValidAudience(s)`/`IssuerSigningKey(s)` plus per-facet delegate slots) folded into `TokenValidationResult`; the result rail is `ValidationParameters` (the same facets as `*Delegate` slots, `SigningKeys`/`DecryptionKeys` as lazy `IList`, `CryptoProviderFactory`, `ConfigurationManager`) folded into `ValidationResult<T, ValidationError>` and the typed `ValidatedToken`
- result-vs-throw: the result-based `Validators.Validate*` overloads return `ValidationResult<…>` (`Succeeded`, `Result`, `Error`) and the `readonly record struct` carries implicit lifts from both `TResult` and `TError` — the canonical ROP shape; the same-named legacy overloads throw `SecurityToken*Exception` subtypes, retained only for `TokenValidationParameters` consumers
- error rail: `ValidationError` is the non-throwing failure and carries `FailureType`, `Message`, `InnerException`, `StackFrames`, `GetException()`, and `AddStackFrame`; `ValidationFailureType` is the abstract discriminator
- error facets: `IssuerValidationError`, `AudienceValidationError`, `LifetimeValidationError`, `SignatureValidationError`, `SignatureKeyValidationError`, `AlgorithmValidationError`, `TokenTypeValidationError`, and `TokenReplayValidationError` classify failures per facet
- validated outcome: `ValidatedToken` aggregates `SecurityToken`, `TokenHandler`, `ValidationParameters`, `ValidatedIssuer?`, `ValidatedLifetime?`, `ValidatedSignatureKey?`, `ValidatedTokenType?`, `ValidatedAudience`/`ValidatedAlgorithm`, `Claims`, and `ClaimsIdentity` — the success payload of the result rail
- key hierarchy: `SecurityKey` (abstract: `KeyId`, `KeySize`, `CryptoProviderFactory`, `ComputeJwkThumbprint`) roots `SymmetricSecurityKey`, `AsymmetricSecurityKey` (-> `RsaSecurityKey`, `ECDsaSecurityKey`, `X509SecurityKey`, `MlDsaSecurityKey`), and `JsonWebKey`; `MlDsaSecurityKey` wraps the net10 BCL `MLDsa` for FIPS-204 post-quantum signatures and is `IDisposable`
- credentials: `SigningCredentials(SecurityKey, algorithm[, digest])` binds key+alg for issuance; `EncryptingCredentials` binds the JWE alg/enc pair; both carry a `CryptoProviderFactory` override; `X509SigningCredentials`/`X509EncryptingCredentials` are the certificate-backed forms
- crypto factory: `CryptoProviderFactory.Default` is the shared instance; `CreateForSigning`/`CreateForVerifying`/`CreateKeyWrapProvider`/`CreateAuthenticatedEncryptionProvider` mint providers, and `CacheSignatureProviders` + `CryptoProviderCache` memoize them across validations — the perf seam under high-throughput validate loops
- discovery contract: `BaseConfigurationManager` in this assembly is the abstract refresh contract assigned to `TokenValidationParameters.ConfigurationManager` and `ValidationParameters.ConfigurationManager`; it owns `MetadataAddress`, `AutomaticRefreshInterval`, `RefreshInterval`, `LastKnownGoodConfiguration`, `LastKnownGoodLifetime`, and `UseLastKnownGoodConfiguration`
- discovery behavior: `ValidateWithLKG` and `RefreshBeforeValidation` drive last-known-good fallback and forced refresh on signature-key-not-found
- discovery implementation: `Microsoft.IdentityModel.Protocols` owns the concrete `ConfigurationManager<T>` specialized to `T = OpenIdConnectConfiguration` by the OIDC discovery rail
- encoding: `Base64UrlEncoder` owns string and `Span<byte>`/`Span<char>` overloads plus generic `Decode<T>(…, Func<…>)` projections, the zero-allocation JOSE codec the JWT handler builds on

[LOCAL_ADMISSION]:

- Construct one `ValidationParameters` (result rail) per security context, set the per-facet `*Delegate` slots only to override a default `Validators` function, and consume `Validators.Validate*` results by branching on `ValidationResult.Succeeded` — project `ValidationError` into the host failure rail through `GetException()` only at the boundary, never as control flow.
- Use `TokenValidationParameters` only where a `JsonWebTokenHandler.ValidateTokenAsync(token, TVP)` consumer requires it; populate `ValidIssuer(s)`/`ValidAudience(s)` explicitly, set `ClockSkew` deliberately, and prefer `Clone()` for per-request mutation over sharing a mutated instance.
- Assign `ConfigurationManager` from the OIDC discovery rail and leave `IssuerSigningKeys` unset for rotating providers; set `ValidateWithLKG = true` so a transient JWKS fetch failure falls back to last-known-good rather than failing validation.
- Build keys from typed constructors (`SymmetricSecurityKey`, `RsaSecurityKey`, `X509SecurityKey`, `MlDsaSecurityKey`) and credentials from `SigningCredentials(key, algorithm)`; identify keys by `ComputeJwkThumbprint`/`KeyId`, never by index.
- Reuse `CryptoProviderFactory.Default` (or one configured instance with `CacheSignatureProviders = true`) across the validation loop so signature providers are cached; do not mint a provider per token.
- Treat `MlDsaSecurityKey` as the post-quantum signing path on net10; dispose it, and gate its use on `CryptoProviderFactory.IsSupportedAlgorithm(alg, key)`.

[RAIL_LAW]:

- Package: `Microsoft.IdentityModel.Tokens`
- Owns: security keys (incl. post-quantum ML-DSA and JWK), signing/encrypting credentials, crypto-provider factory/cache, ECDH-ES key agreement, base64url encoding, and the throwing + result-based token-validation contracts
- Accept: result-based validation through `ValidationParameters` + `Validators` returning `ValidationResult<T, ValidationError>`; legacy validation through `TokenValidationParameters` + `TokenValidationResult`; discovery through a `BaseConfigurationManager`
- Reject: throwing-validator control flow in domain code where the result-based `Validators` overload exists, hand-rolled base64url/JWK parsing, per-token crypto-provider construction, static signing keys for rotating issuers, weak/string key identity
