# [RASM_APPUI_API_REACTIVEUI_VALIDATION]

`ReactiveUI.Validation` supplies validation contexts and observable validation state for AppUi screen inputs.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Validation`
- package: `ReactiveUI.Validation`
- assembly: `ReactiveUI.Validation`
- namespace: `ReactiveUI.Validation`
- asset: runtime library
- rail: validation-ui

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation UI family
- rail: validation-ui

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]                   |
| :-----: | :---------------------- | :---------------- | :----------------------------- |
|   [1]   | `IValidatableViewModel` | contract surface  | defines boundary contract      |
|   [2]   | `ValidationContext`     | operation context | carries operation state        |
|   [3]   | `ValidationState`       | rail contract     | anchors validation-ui contract |
|   [4]   | `ValidationText`        | rail contract     | anchors validation-ui contract |
|   [5]   | `IValidationComponent`  | contract surface  | defines boundary contract      |
|   [6]   | `ValidationHelper`      | rail contract     | anchors validation-ui contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: validation operations
- rail: validation-ui

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]   | [CAPABILITY]                  |
| :-----: | :------------------------ | :------------- | :---------------------------- |
|   [1]   | `ValidationRule`          | member surface | drives validation-ui behavior |
|   [2]   | `ValidationRuleFor`       | member surface | drives validation-ui behavior |
|   [3]   | `BindValidation`          | member surface | drives validation-ui behavior |
|   [4]   | `IsValid`                 | member surface | drives validation-ui behavior |
|   [5]   | `GetValidationObservable` | lookup call    | resolves typed value          |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: observable input validation
- Accept: screen validation joins command availability
- Reject: detached validation bags

