# [RASM_API_JETBRAINS_ANNOTATIONS]

`JetBrains.Annotations` (MIT, `netstandard2.0`) is injected workspace-wide by `Directory.Build.props` when `UsesThinktecture` is true. The package supplies the static-analysis attribute surface consumed by generated Thinktecture code and local API contracts, while `ThinktectureRuntimeExtensions_SourceGenerator_GenerateJetBrainsAnnotations=false` keeps one CLR identity for the annotations.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `JetBrains.Annotations`

- package: `JetBrains.Annotations`
- license: MIT
- assembly: `JetBrains.Annotations`
- namespace: `JetBrains.Annotations`
- asset: `netstandard2.0`
- rail: source-analysis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: nullability and collection contracts

- rail: source-analysis

| [INDEX] | [SYMBOL]                 | [CAPABILITY]                                |
| :-----: | :----------------------- | :------------------------------------------ |
|  [01]   | `CanBeNullAttribute`     | marks nullable result, parameter, or member |
|  [02]   | `NotNullAttribute`       | marks non-null result, parameter, or member |
|  [03]   | `ItemCanBeNullAttribute` | marks nullable collection elements          |
|  [04]   | `ItemNotNullAttribute`   | marks non-null collection elements          |

[PUBLIC_TYPE_SCOPE]: API reachability and generated-code contracts

- rail: source-analysis

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                        |
| :-----: | :---------------------------- | :-------------------------------------------------- |
|  [01]   | `PublicAPIAttribute`          | preserves public API members from unused warnings   |
|  [02]   | `UsedImplicitlyAttribute`     | marks reflection, generator, or host-called usage   |
|  [03]   | `MeansImplicitUseAttribute`   | lifts implicit-use semantics onto custom attributes |
|  [04]   | `InstantHandleAttribute`      | marks delegates consumed during the call            |
|  [05]   | `MustUseReturnValueAttribute` | marks results that callers must consume             |

[PUBLIC_TYPE_SCOPE]: expression, assertion, and string contracts

- rail: source-analysis

| [INDEX] | [SYMBOL]                             | [CAPABILITY]                                      |
| :-----: | :----------------------------------- | :------------------------------------------------ |
|  [01]   | `PureAttribute`                      | declares a side-effect-free member                |
|  [02]   | `ContractAnnotationAttribute`        | encodes input/output nullability and termination  |
|  [03]   | `AssertionMethodAttribute`           | marks assertion helpers                           |
|  [04]   | `AssertionConditionAttribute`        | marks the asserted boolean parameter              |
|  [05]   | `StringFormatMethodAttribute`        | binds a format-string parameter                   |
|  [06]   | `StructuredMessageTemplateAttribute` | binds structured logging message templates        |
|  [07]   | `InvokerParameterNameAttribute`      | restricts arguments to caller parameter names     |
|  [08]   | `ValueProviderAttribute`             | binds an argument to named value-provider members |
|  [09]   | `RegexPatternAttribute`              | marks regex pattern strings                       |
|  [10]   | `NoEnumerationAttribute`             | forbids eager enumeration by analyzers            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: attribute construction

- rail: source-analysis

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE] | [CAPABILITY]               |
| :-----: | :------------------------------------------------------ | :----------- | :------------------------- |
|  [01]   | `new PublicAPIAttribute()` / `(string comment)`         | attribute    | public API preservation    |
|  [02]   | `new UsedImplicitlyAttribute()`                         | attribute    | implicit usage declaration |
|  [03]   | `new MeansImplicitUseAttribute()`                       | attribute    | custom implicit-use marker |
|  [04]   | `new InstantHandleAttribute()`                          | attribute    | delegate lifetime marker   |
|  [05]   | `new MustUseReturnValueAttribute()`                     | attribute    | result-consumption marker  |
|  [06]   | `new ContractAnnotationAttribute(string contract)`      | attribute    | contract grammar marker    |
|  [07]   | `new StringFormatMethodAttribute(string parameterName)` | attribute    | format parameter marker    |
|  [08]   | `new StructuredMessageTemplateAttribute()`              | attribute    | structured template marker |

## [04]-[IMPLEMENTATION_LAW]

[ONE_IDENTITY]:

- `Directory.Build.props` injects `JetBrains.Annotations` under `UsesThinktecture`.
- `ThinktectureRuntimeExtensions_SourceGenerator_GenerateJetBrainsAnnotations=false` suppresses generated duplicate definitions.
- Local code consumes the package identity only; generated code never mints a second annotations namespace.

[LOCAL_ADMISSION]:

- Public or reflection-bound members use `PublicAPI` / `UsedImplicitly` rather than analyzer suppression comments.
- Generator and host seams use reachability attributes at the member or custom-attribute owner, never wrapper shims.
- Return-value, assertion, and format contracts ride the verified JetBrains attributes instead of local marker types.

[RAIL_LAW]:

- Package: `JetBrains.Annotations`
- Owns: annotation attributes for nullability, API reachability, expression contracts, assertions, and structured string analysis
- Accept: generated-code and analyzer contracts over the single package identity
- Reject: generated duplicate JetBrains annotations, local attribute mirrors, or suppression comments where a package attribute states the contract
