# [RASM_APPHOST_API_IDENTITYMODEL_TOKENS]

`Microsoft.IdentityModel.Tokens` is the cryptographic and validation substrate under the IdentityModel stack: it owns the security-key hierarchy (symmetric, RSA, ECDSA, X.509, JWK, and post-quantum ML-DSA), signing/encrypting/key-wrap credentials, the crypto-provider factory and cache, and the two parallel token-validation contracts — the established `TokenValidationParameters` + `TokenValidationResult` with throwing-or-result delegate validators, and the modern `ValidationParameters` + `ValidationResult<TResult, ValidationError>` ROP rail with typed `Validated*` outcomes. It is the contract `Microsoft.IdentityModel.JsonWebTokens` reads against and the slot (`BaseConfigurationManager`) into which `Microsoft.IdentityModel.Protocols.OpenIdConnect` discovery feeds rotating signing keys. No IdentityModel package validates a token without this assembly's parameters and keys.

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

[PUBLIC_TYPE_SCOPE]: validation contract — legacy and modern
- rail: tokens-core

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]        | [RAIL]                                       |
| :-----: | :-------------------------------- | :------------------- | :------------------------------------------- |
|  [01]   | `TokenValidationParameters`       | validation policy    | legacy validate-knob + delegate-slot bag     |
|  [02]   | `TokenValidationResult`           | validation result    | legacy `IsValid`/`ClaimsIdentity`/`Exception`|
|  [03]   | `ValidationParameters`            | validation policy    | modern delegate-driven validate policy       |
|  [04]   | `ValidationResult<TResult,TError>`| result struct        | `readonly record struct` ROP outcome         |
|  [05]   | `ValidationError`                 | error base           | non-throwing failure with stack trace        |
|  [06]   | `ValidatedToken`                  | validated outcome    | aggregate of every validated facet           |
|  [07]   | `ValidatedIssuer` / `ValidatedLifetime` / `ValidatedSignatureKey` / `ValidatedTokenType` | result structs | per-facet `readonly struct` outcomes |
|  [08]   | `Validators`                      | static validators    | per-facet validate functions (both shapes)   |
|  [09]   | `ValidationFailureType`           | failure taxonomy     | abstract per-facet failure classifier        |
|  [10]   | `TokenHandler`                    | handler base         | `ValidateTokenAsync`/`ReadToken` base        |
|  [11]   | `SecurityTokenHandler`            | handler base (XML)   | WS-Fed/SAML-style `XmlReader` handler base   |
|  [12]   | `SecurityToken`                   | token base           | abstract parsed-token root                    |
|  [13]   | `SecurityTokenDescriptor`         | creation input       | declarative token-emission descriptor        |

[PUBLIC_TYPE_SCOPE]: keys, credentials, and crypto providers
- rail: tokens-core

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                        |
| :-----: | :----------------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `SecurityKey`                  | key base           | `KeyId`, `ComputeJwkThumbprint`, factory      |
|  [02]   | `SymmetricSecurityKey`         | symmetric key      | HMAC/AES key from bytes                        |
|  [03]   | `RsaSecurityKey`               | asymmetric key     | RSA signing/encryption key                     |
|  [04]   | `ECDsaSecurityKey`             | asymmetric key     | EC signing key                                 |
|  [05]   | `X509SecurityKey`              | certificate key    | X.509-backed key                               |
|  [06]   | `MlDsaSecurityKey`             | post-quantum key   | wraps BCL `MLDsa` (FIPS 204) for PQ signatures |
|  [07]   | `JsonWebKey` / `JsonWebKeySet` | JWK / JWKS         | RFC 7517 key + key-set, JWKS discovery target  |
|  [08]   | `JsonWebKeyConverter`          | JWK conversion     | `SecurityKey` <-> `JsonWebKey`                 |
|  [09]   | `SigningCredentials`           | signing credential | key + algorithm (+ digest)                     |
|  [10]   | `EncryptingCredentials`        | encrypt credential | key + enc/alg for JWE                           |
|  [11]   | `X509SigningCredentials` / `X509EncryptingCredentials` | certificate credentials | cert-backed sign/encrypt material |
|  [12]   | `CryptoProviderFactory`        | provider factory   | signature/keywrap/AEAD provider + cache        |
|  [13]   | `CryptoProviderCache` / `InMemoryCryptoProviderCache` | provider cache | provider memoization across validations |
|  [14]   | `EcdhKeyExchangeProvider`      | key agreement      | ECDH-ES KDF for JWE key agreement              |
|  [15]   | `AuthenticatedEncryptionProvider` / `AuthenticatedEncryptionResult` | AEAD | AES-GCM / AES-CBC-HMAC content encryption |
|  [16]   | `BaseConfiguration` / `BaseConfigurationManager` | discovery slot | issuer/JWKS config refresh contract |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: modern result-based validation — `Validators` + `ValidationParameters`
- rail: tokens-core

The modern rail is pure ROP: every validator returns `ValidationResult<T, ValidationError>` (or its async `Task<…>` mirror) and never throws; `ValidationParameters` carries one `*Delegate` slot per facet that defaults to the matching `Validators` static.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY]    | [RAIL]                                |
| :-----: | :---------------------------------------------------------------------------------------------- | :---------------- | :------------------------------------ |
|  [01]   | `Validators.ValidateAudience(IList<string>, SecurityToken?, ValidationParameters, CallContext)` | audience          | `ValidationResult<string, …>`         |
|  [02]   | `Validators.ValidateIssuerAsync(string?, SecurityToken?, ValidationParameters, CallContext?, CT)`| issuer (async)    | `ValidationResult<ValidatedIssuer, …>`|
|  [03]   | `Validators.ValidateLifetime(DateTime?, DateTime?, SecurityToken?, ValidationParameters, CallContext)` | lifetime    | `ValidationResult<ValidatedLifetime, …>` |
|  [04]   | `Validators.ValidateSignatureKey(SecurityKey, SecurityToken, ValidationParameters, CallContext)`| signature key     | `ValidationResult<ValidatedSignatureKey, …>` |
|  [05]   | `Validators.ValidateTokenType(string?, SecurityToken?, ValidationParameters, CallContext)`      | token type        | `ValidationResult<ValidatedTokenType, …>` |
|  [06]   | `Validators.ValidateAlgorithm(string?, SecurityToken, ValidationParameters, CallContext)`       | algorithm         | `ValidationResult<string, …>`         |
|  [07]   | `Validators.ValidateTokenReplay(DateTime?, string, ValidationParameters, CallContext)`          | replay            | `ValidationResult<DateTime?, …>`      |
|  [08]   | `ValidationResult<T,E>.Succeeded` / `.Result` / `.Error`                                        | result projection | discriminated outcome access          |
|  [09]   | implicit `ValidationResult<T,E>(T)` / `(TError)`                                                | result lift       | success/error implicit conversions    |

[ENTRYPOINT_SCOPE]: legacy validation contract — `TokenValidationParameters` + `Validators`
- rail: tokens-core

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]    | [RAIL]                                 |
| :-----: | :-------------------------------------------------------------------- | :---------------- | :------------------------------------- |
|  [01]   | `Validators.ValidateIssuer(string, SecurityToken, TVP)`              | issuer (throwing) | returns validated issuer or throws     |
|  [02]   | `Validators.ValidateAudience(IEnumerable<string>, SecurityToken, TVP)`| audience          | throws on mismatch                     |
|  [03]   | `Validators.ValidateLifetime(DateTime?, DateTime?, SecurityToken, TVP)`| lifetime         | throws on nbf/exp violation            |
|  [04]   | `Validators.ValidateIssuerSecurityKey(SecurityKey, SecurityToken, TVP)`| signing key      | throws on disallowed key               |
|  [05]   | `TokenValidationParameters.Clone()`                                  | policy clone      | per-request TVP copy                   |
|  [06]   | `TokenValidationParameters.{IssuerValidator,AudienceValidator,LifetimeValidator,SignatureValidator,…}` | delegate override | per-facet legacy delegate slot |

[ENTRYPOINT_SCOPE]: keys, credentials, crypto, and encoding
- rail: tokens-core

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]    | [RAIL]                                 |
| :-----: | :----------------------------------------------------------------- | :---------------- | :------------------------------------- |
|  [01]   | `new SymmetricSecurityKey(byte[])` / `new SigningCredentials(key, algorithm)` | key/cred ctor | HMAC key + signing credential          |
|  [02]   | `SecurityKey.ComputeJwkThumbprint()` / `KeyId`                     | key identity      | RFC 7638 thumbprint + `kid`            |
|  [03]   | `JsonWebKeySet.Create(string json)` / `GetSigningKeys()`           | JWKS parse        | parse a JWKS document to keys          |
|  [04]   | `JsonWebKeyConverter.ConvertFrom…` family                         | key conversion    | bridge `SecurityKey` and JWK           |
|  [05]   | `CryptoProviderFactory.Default` / `CreateForSigning(key, alg)`     | provider create   | cached signature provider              |
|  [06]   | `CryptoProviderFactory.CreateForVerifying` / `CreateKeyWrapProvider` / `CreateAuthenticatedEncryptionProvider` | provider create | verify/keywrap/AEAD providers |
|  [07]   | `CryptoProviderFactory.IsSupportedAlgorithm(alg, key)`            | capability probe  | algorithm/key support check            |
|  [08]   | `EcdhKeyExchangeProvider.GenerateKdf(apu, apv)`                    | key agreement     | ECDH-ES derived key                    |
|  [09]   | `Base64UrlEncoder.Encode(ReadOnlySpan<byte>, Span<char>)` / `Decode(string)` | base64url | span-based zero-alloc JOSE encoding    |

## [04]-[IMPLEMENTATION_LAW]

[TOKENS_TOPOLOGY]:
- two validation rails: the legacy rail is `TokenValidationParameters` (a wide knob bag — `ValidateIssuer`/`ValidateAudience`/`ValidateLifetime`/`ValidateIssuerSigningKey`/`RequireExpirationTime`/`RequireSignedTokens`/`ClockSkew`/`ValidIssuer(s)`/`ValidAudience(s)`/`IssuerSigningKey(s)` plus per-facet delegate slots) folded into `TokenValidationResult`; the modern rail is `ValidationParameters` (the same facets as `*Delegate` slots, `SigningKeys`/`DecryptionKeys` as lazy `IList`, `CryptoProviderFactory`, `ConfigurationManager`) folded into `ValidationResult<T, ValidationError>` and the typed `ValidatedToken`
- result-vs-throw: the modern `Validators.Validate*` overloads return `ValidationResult<…>` (`Succeeded`, `Result`, `Error`) and the `readonly record struct` carries implicit lifts from both `TResult` and `TError` — the canonical ROP shape; the same-named legacy overloads throw `SecurityToken*Exception` subtypes, retained only for `TokenValidationParameters` consumers
- error taxonomy: `ValidationError` is the non-throwing failure (`FailureType`, `Message`, `InnerException`, `StackFrames`, `GetException()`, `AddStackFrame`); concrete subtypes (`IssuerValidationError`, `AudienceValidationError`, `LifetimeValidationError`, `SignatureValidationError`, `SignatureKeyValidationError`, `AlgorithmValidationError`, `TokenTypeValidationError`, `TokenReplayValidationError`) classify per facet, and `ValidationFailureType` is the abstract discriminator
- validated outcome: `ValidatedToken` aggregates `SecurityToken`, `TokenHandler`, `ValidationParameters`, `ValidatedIssuer?`, `ValidatedLifetime?`, `ValidatedSignatureKey?`, `ValidatedTokenType?`, `ValidatedAudience`/`ValidatedAlgorithm`, `Claims`, and `ClaimsIdentity` — the success payload of the modern rail
- key hierarchy: `SecurityKey` (abstract: `KeyId`, `KeySize`, `CryptoProviderFactory`, `ComputeJwkThumbprint`) roots `SymmetricSecurityKey`, `AsymmetricSecurityKey` (-> `RsaSecurityKey`, `ECDsaSecurityKey`, `X509SecurityKey`, `MlDsaSecurityKey`), and `JsonWebKey`; `MlDsaSecurityKey` wraps the net10 BCL `MLDsa` for FIPS-204 post-quantum signatures and is `IDisposable`
- credentials: `SigningCredentials(SecurityKey, algorithm[, digest])` binds key+alg for issuance; `EncryptingCredentials` binds the JWE alg/enc pair; both carry a `CryptoProviderFactory` override; `X509SigningCredentials`/`X509EncryptingCredentials` are the certificate-backed forms
- crypto factory: `CryptoProviderFactory.Default` is the shared instance; `CreateForSigning`/`CreateForVerifying`/`CreateKeyWrapProvider`/`CreateAuthenticatedEncryptionProvider` mint providers, and `CacheSignatureProviders` + `CryptoProviderCache` memoize them across validations — the perf seam under high-throughput validate loops
- discovery slot: `BaseConfigurationManager` (this assembly) is the abstract refresh contract assigned to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager` — it owns `MetadataAddress`, `AutomaticRefreshInterval`, `RefreshInterval`, `LastKnownGoodConfiguration`/`LastKnownGoodLifetime`, and `UseLastKnownGoodConfiguration`; `ValidateWithLKG` + `RefreshBeforeValidation` drive last-known-good fallback and forced refresh on signature-key-not-found. The concrete `ConfigurationManager<T>` that fills this slot ships in `Microsoft.IdentityModel.Protocols` (cataloged at `api-identitymodel-protocols.md`), specialized to `T = OpenIdConnectConfiguration` by the OIDC discovery rail
- encoding: `Base64UrlEncoder` provides string and `Span<byte>`/`Span<char>` overloads plus generic `Decode<T>(…, Func<…>)` projections, the zero-allocation JOSE codec the JWT handler builds on

[LOCAL_ADMISSION]:
- Construct one `ValidationParameters` (modern rail) per security context, set the per-facet `*Delegate` slots only to override a default `Validators` function, and consume `Validators.Validate*` results by branching on `ValidationResult.Succeeded` — project `ValidationError` into the host failure rail through `GetException()` only at the boundary, never as control flow.
- Use `TokenValidationParameters` only where a `JsonWebTokenHandler.ValidateTokenAsync(token, TVP)` consumer requires it; populate `ValidIssuer(s)`/`ValidAudience(s)` explicitly, set `ClockSkew` deliberately, and prefer `Clone()` for per-request mutation over sharing a mutated instance.
- Assign `ConfigurationManager` from the OIDC discovery rail and leave `IssuerSigningKeys` unset for rotating providers; set `ValidateWithLKG = true` so a transient JWKS fetch failure falls back to last-known-good rather than failing validation.
- Build keys from typed constructors (`SymmetricSecurityKey`, `RsaSecurityKey`, `X509SecurityKey`, `MlDsaSecurityKey`) and credentials from `SigningCredentials(key, algorithm)`; identify keys by `ComputeJwkThumbprint`/`KeyId`, never by index.
- Reuse `CryptoProviderFactory.Default` (or one configured instance with `CacheSignatureProviders = true`) across the validation loop so signature providers are cached; do not mint a provider per token.
- Treat `MlDsaSecurityKey` as the post-quantum signing path on net10; dispose it, and gate its use on `CryptoProviderFactory.IsSupportedAlgorithm(alg, key)`.

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.Tokens`
- Owns: security keys (incl. post-quantum ML-DSA and JWK), signing/encrypting credentials, crypto-provider factory/cache, ECDH-ES key agreement, base64url encoding, and the legacy + modern token-validation contracts
- Accept: modern validation through `ValidationParameters` + `Validators` returning `ValidationResult<T, ValidationError>`; legacy validation through `TokenValidationParameters` + `TokenValidationResult`; discovery through a `BaseConfigurationManager`
- Reject: throwing-validator control flow in domain code where the result-based `Validators` overload exists, hand-rolled base64url/JWK parsing, per-token crypto-provider construction, static signing keys for rotating issuers, weak/string key identity
