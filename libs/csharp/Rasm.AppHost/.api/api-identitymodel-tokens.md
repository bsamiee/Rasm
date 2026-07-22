# [RASM_APPHOST_API_IDENTITYMODEL_TOKENS]

`Microsoft.IdentityModel.Tokens` owns the cryptographic and validation substrate under the IdentityModel stack: the security-key hierarchy, signing/encrypting/key-wrap credentials, the crypto-provider factory and cache, and two parallel token-validation contracts — a throwing delegate rail and a result-based ROP rail with typed `Validated*` outcomes. `Microsoft.IdentityModel.JsonWebTokens` validates against these parameters and keys, and the `BaseConfigurationManager` slot binds the rotating signing keys `Microsoft.IdentityModel.Protocols.OpenIdConnect` discovery feeds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.Tokens`
- package: `Microsoft.IdentityModel.Tokens` (MIT)
- assembly: `Microsoft.IdentityModel.Tokens`
- namespace: `Microsoft.IdentityModel.Tokens`
- asset: runtime library
- abi: native `lib/net10.0` asset (consumer-bound); ML-DSA binds the `net10.0` `System.Security.Cryptography.MLDsa` type
- depends: `Microsoft.IdentityModel.Logging`, `Microsoft.IdentityModel.Abstractions`
- rail: tokens-core

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation contracts

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]     | [CAPABILITY]         |
| :-----: | :---------------------------------- | :---------------- | :------------------- |
|  [01]   | `TokenValidationParameters`         | validation policy | throwing delegates   |
|  [02]   | `TokenValidationResult`             | validation result | throwing projection  |
|  [03]   | `ValidationParameters`              | validation policy | result delegates     |
|  [04]   | `ValidationResult<TResult, TError>` | result struct     | ROP outcome          |
|  [05]   | `ValidationError`                   | error base        | non-throwing failure |
|  [06]   | `ValidatedToken`                    | outcome aggregate | validated facets     |
|  [07]   | `ValidatedIssuer`                   | result struct     | issuer facet         |
|  [08]   | `ValidatedLifetime`                 | result struct     | lifetime facet       |
|  [09]   | `ValidatedSignatureKey`             | result struct     | signature-key facet  |
|  [10]   | `ValidatedTokenType`                | result struct     | token-type facet     |
|  [11]   | `Validators`                        | static validators | both contracts       |
|  [12]   | `ValidationFailureType`             | failure taxonomy  | facet classifier     |
|  [13]   | `TokenHandler`                      | handler base      | token operations     |
|  [14]   | `SecurityTokenHandler`              | XML handler base  | WS-Fed and SAML      |
|  [15]   | `SecurityToken`                     | token base        | parsed-token root    |
|  [16]   | `SecurityTokenDescriptor`           | creation input    | emission descriptor  |

[PUBLIC_TYPE_SCOPE]: keys, credentials, and crypto providers

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [CAPABILITY]           |
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

[PUBLIC_TYPE_SCOPE]: validation exceptions (throwing rail)

Throwing `Validators.Validate*` overloads and `JsonWebTokenHandler.ValidateTokenAsync` surface these on `TokenValidationResult.Exception`; a consumer pattern-matches the concrete subtype, never a bare `Exception`. `SecurityTokenValidationException` roots every concrete validation exception under `SecurityTokenException`.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]  | [CAPABILITY]       |
| :-----: | :--------------------------------------- | :------------- | :----------------- |
|  [01]   | `SecurityTokenException`                 | exception base | throwing rail      |
|  [02]   | `SecurityTokenValidationException`       | exception base | validation         |
|  [03]   | `SecurityTokenExpiredException`          | exception      | `nbf` or `exp`     |
|  [04]   | `SecurityTokenInvalidSignatureException` | exception      | signature rejected |
|  [05]   | `SecurityTokenInvalidIssuerException`    | exception      | untrusted issuer   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: result-based validation — `Validators` + `ValidationParameters`

Every `Validators` result overload is `static`, trails `ValidationParameters, CallContext`, and returns `ValidationResult<T, ValidationError>` (`Succeeded`, `Result`, `Error`); async mirrors add `CT` and return the `Task<…>` form. `ValidationResult<T, E>` is a `readonly record struct` lifting implicitly from both `TResult` and `TError`. `ValidationParameters` defaults each per-facet `*Delegate` slot to the matching `Validators` static.

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `ValidateAudience(IList<string>, SecurityToken?)`        | audience match -> `string`                |
|  [02]   | `ValidateIssuerAsync(string?, SecurityToken?)`           | issuer trust -> `ValidatedIssuer` (async) |
|  [03]   | `ValidateLifetime(DateTime?, DateTime?, SecurityToken?)` | `nbf`/`exp` window -> `ValidatedLifetime` |
|  [04]   | `ValidateSignatureKey(SecurityKey, SecurityToken)`       | key trust -> `ValidatedSignatureKey`      |
|  [05]   | `ValidateTokenType(string?, SecurityToken?)`             | `typ` check -> `ValidatedTokenType`       |
|  [06]   | `ValidateAlgorithm(string?, SecurityToken)`              | algorithm allow -> `string`               |
|  [07]   | `ValidateTokenReplay(DateTime?, string)`                 | replay guard -> `DateTime?`               |

[ENTRYPOINT_SCOPE]: throwing validation — `TokenValidationParameters` + `Validators`

Throwing `Validators.Validate*` overloads trail `TokenValidationParameters` and return the validated value or raise a `SecurityToken*Exception`. `TokenValidationParameters` carries `IssuerValidator`, `AudienceValidator`, `LifetimeValidator`, `SignatureValidator`, and the remaining per-facet delegate slots.

| [INDEX] | [SURFACE]                                               | [CAPABILITY]               |
| :-----: | :------------------------------------------------------ | :------------------------- |
|  [01]   | `ValidateIssuer(string, SecurityToken)`                 | validated issuer, or throw |
|  [02]   | `ValidateAudience(IEnumerable<string>, SecurityToken)`  | throw on mismatch          |
|  [03]   | `ValidateLifetime(DateTime?, DateTime?, SecurityToken)` | throw on `nbf`/`exp`       |
|  [04]   | `ValidateIssuerSecurityKey(SecurityKey, SecurityToken)` | throw on disallowed key    |
|  [05]   | `TokenValidationParameters.Clone() -> TVP`              | per-request policy copy    |

[ENTRYPOINT_SCOPE]: keys, credentials, crypto, and encoding

`CryptoProviderFactory` owns the provider-creation rows, shown without its prefix; `CacheSignatureProviders` and `CryptoProviderCache` memoize the minted providers.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]         |
| :-----: | :----------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `new SymmetricSecurityKey(byte[])`                           | ctor     | HMAC/AES key         |
|  [02]   | `new SigningCredentials(SecurityKey, string)`                | ctor     | signing material     |
|  [03]   | `SecurityKey.ComputeJwkThumbprint()`                         | instance | RFC 7638 identity    |
|  [04]   | `SecurityKey.KeyId`                                          | property | `kid`                |
|  [05]   | `JsonWebKeySet.Create(string)`                               | static   | parse JWKS document  |
|  [06]   | `JsonWebKeySet.GetSigningKeys()`                             | instance | signing keys         |
|  [07]   | `JsonWebKeyConverter.ConvertFromSecurityKey(SecurityKey)`    | static   | `SecurityKey` -> JWK |
|  [08]   | `Default`                                                    | property | shared factory       |
|  [09]   | `CreateForSigning(SecurityKey, string)`                      | factory  | cached signer        |
|  [10]   | `CreateForVerifying(SecurityKey, string)`                    | factory  | verifier             |
|  [11]   | `CreateKeyWrapProvider(SecurityKey, string)`                 | factory  | key wrap             |
|  [12]   | `CreateAuthenticatedEncryptionProvider(SecurityKey, string)` | factory  | AEAD                 |
|  [13]   | `IsSupportedAlgorithm(string, SecurityKey)`                  | instance | algorithm+key probe  |
|  [14]   | `EcdhKeyExchangeProvider.GenerateKdf(string, string)`        | instance | ECDH-ES derivation   |
|  [15]   | `Base64UrlEncoder.Encode(ReadOnlySpan<byte>, Span<char>)`    | static   | zero-alloc encode    |
|  [16]   | `Base64UrlEncoder.Decode(string)`                            | static   | decode               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- two rails: `TokenValidationParameters` (wide knob bag with per-facet delegate slots) folds into `TokenValidationResult`; `ValidationParameters` (per-facet `*Delegate` slots, lazy `SigningKeys`/`DecryptionKeys`, `CryptoProviderFactory`, `ConfigurationManager`) folds into `ValidationResult<T, ValidationError>` and the typed `ValidatedToken`
- result versus throw: result-based `Validators.Validate*` return `ValidationResult<T, ValidationError>` and never throw — the canonical ROP shape; the same-named throwing overloads raise `SecurityToken*Exception` and bind `TokenValidationParameters` consumers
- error rail: `ValidationError` is the non-throwing failure carrying `FailureType`, `Message`, `InnerException`, `StackFrames`, and `GetException()`; `ValidationFailureType` is the abstract discriminator, and one `*ValidationError` facet classifies each validator
- validated outcome: `ValidatedToken` aggregates the per-facet `Validated*` structs, `ValidatedAudience`/`ValidatedAlgorithm`, `Claims`, and `ClaimsIdentity` as the result rail's success payload
- key hierarchy: `SecurityKey` (`KeyId`, `KeySize`, `CryptoProviderFactory`, `ComputeJwkThumbprint`) roots `SymmetricSecurityKey`, the `AsymmetricSecurityKey` subtree (`RsaSecurityKey`, `ECDsaSecurityKey`, `X509SecurityKey`, `MlDsaSecurityKey`), and `JsonWebKey`; `MlDsaSecurityKey` wraps the net10 BCL `MLDsa` for FIPS-204 post-quantum signing and is `IDisposable`
- crypto cache: `CryptoProviderFactory.Default` and `CryptoProviderCache` memoize signature providers across the validate loop — the throughput seam under high-volume validation
- discovery slot: `BaseConfigurationManager` is the abstract refresh contract assigned to both `ConfigurationManager` slots, owning `MetadataAddress`, `AutomaticRefreshInterval`, `RefreshInterval`, `LastKnownGoodConfiguration`, `LastKnownGoodLifetime`, and `UseLastKnownGoodConfiguration`; `ValidateWithLKG` and `RefreshBeforeValidation` drive last-known-good fallback and forced refresh on signature-key-not-found

[STACKING]:
- `Microsoft.IdentityModel.Protocols`(`.api/api-identitymodel-protocols.md`): its concrete `ConfigurationManager<OpenIdConnectConfiguration>` is the `BaseConfigurationManager` assigned to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`, and its refreshed `JsonWebKeySet` supplies the rotating `IssuerSigningKeys` these validators pull when `IssuerSigningKeys` is left unset
- `Microsoft.IdentityModel.JsonWebTokens`(`.api/api-identitymodel-jwt.md`): `JsonWebTokenHandler.ValidateTokenAsync` reads against `TokenValidationParameters`/`ValidationParameters` and this assembly's keys, landing failures on `TokenValidationResult.Exception`; `DecryptTokenWithConfigurationAsync` resolves decryption keys through the `BaseConfigurationManager` slot
- `Microsoft.IdentityModel.Protocols.OpenIdConnect`(`.api/api-identitymodel-oidc.md`): `OpenIdConnectConfiguration.JsonWebKeySet` is the discovery-fed signing-key source consumed through the manager slot after `OpenIdConnectConfigurationValidator` asserts JWKS sufficiency
- within-lib: the AppHost auth composition constructs one `ValidationParameters` per security context, branches every `Validators.Validate*` on `ValidationResult.Succeeded`, projects `ValidationError.GetException()` onto the host failure rail only at the boundary, and shares one cached `CryptoProviderFactory` across the validate loop

[LOCAL_ADMISSION]:
- Construct one `ValidationParameters` (result rail) per security context, override a `*Delegate` slot only to replace the default `Validators` function, and branch on `ValidationResult.Succeeded`; project `ValidationError` through `GetException()` at the boundary, never as control flow
- Use `TokenValidationParameters` only where a `JsonWebTokenHandler.ValidateTokenAsync(token, TVP)` consumer requires it; populate `ValidIssuer(s)`/`ValidAudience(s)`, set `ClockSkew` deliberately, and `Clone()` for per-request mutation over a shared mutated instance
- Assign `ConfigurationManager` from the OIDC discovery rail, leave `IssuerSigningKeys` unset for rotating providers, and set `ValidateWithLKG = true` so a transient JWKS-fetch failure falls back to last-known-good
- Build keys from typed constructors (`SymmetricSecurityKey`, `RsaSecurityKey`, `X509SecurityKey`, `MlDsaSecurityKey`) and credentials from `SigningCredentials(key, algorithm)`; identify keys by `ComputeJwkThumbprint`/`KeyId`, never by index
- Reuse `CryptoProviderFactory.Default` (or one instance with `CacheSignatureProviders = true`) across the validate loop; never mint a provider per token
- Take `MlDsaSecurityKey` as the post-quantum signing path on net10, dispose it, and gate its use on `CryptoProviderFactory.IsSupportedAlgorithm(alg, key)`

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.Tokens`
- Owns: security keys (post-quantum ML-DSA and JWK included), signing/encrypting credentials, the crypto-provider factory and cache, ECDH-ES key agreement, base64url encoding, and both the throwing and result-based token-validation contracts
- Accept: result validation through `ValidationParameters` + `Validators` returning `ValidationResult<T, ValidationError>`; throwing validation through `TokenValidationParameters` + `TokenValidationResult`; discovery through a `BaseConfigurationManager`
- Reject: throwing-validator control flow where the result-based `Validators` overload exists, hand-rolled base64url/JWK parsing, per-token crypto-provider construction, static signing keys for rotating issuers, string-keyed key identity
