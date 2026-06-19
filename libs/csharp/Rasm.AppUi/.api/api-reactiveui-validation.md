# [RASM_APPUI_API_REACTIVEUI_VALIDATION]

`ReactiveUI.Validation` supplies validation contexts, observable validation components, validation text, view bindings, and helper objects for AppUi screen inputs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Validation`
- package: `ReactiveUI.Validation`
- assembly: `ReactiveUI.Validation`
- namespace: `ReactiveUI.Validation`
- namespace: `ReactiveUI.Validation.Contexts`
- namespace: `ReactiveUI.Validation.Components`
- namespace: `ReactiveUI.Validation.Extensions`
- namespace: `ReactiveUI.Validation.Helpers`
- namespace: `ReactiveUI.Validation.States`
- asset: runtime library
- rail: validation-ui

## [02]-[PUBLIC_TYPES]

[CONTEXT_AND_STATE_TYPES]: validation context and state — rail: validation-ui

| [INDEX] | [SYMBOL]                  | [KIND]              |
| :-----: | :------------------------ | :------------------ |
|  [01]   | `IValidatableViewModel`   | view-model contract |
|  [02]   | `IValidationContext`      | context contract    |
|  [03]   | `ValidationContext`       | context runtime     |
|  [04]   | `IValidationState`        | state contract      |
|  [05]   | `ValidationState`         | state value         |
|  [06]   | `ValidationStateComparer` | state comparer      |

[COMPONENT_TYPES]: validation components — rail: validation-ui

| [INDEX] | [SYMBOL]                                   | [KIND]             |
| :-----: | :----------------------------------------- | :----------------- |
|  [01]   | `IValidationComponent`                     | component contract |
|  [02]   | `IPropertyValidationComponent`             | property component |
|  [03]   | `IValidatesProperties`                     | property set       |
|  [04]   | `BasePropertyValidation<T>`                | property base      |
|  [05]   | `BasePropertyValidation<T,TValue>`         | typed property     |
|  [06]   | `ObservableValidation<T,TValue>`           | observable rule    |
|  [07]   | `ObservableValidation<T,TValue,TProperty>` | property rule      |

[TEXT_AND_BINDING_TYPES]: validation text, helpers, and bindings — rail: validation-ui

| [INDEX] | [SYMBOL]                      | [KIND]             |
| :-----: | :---------------------------- | :----------------- |
|  [01]   | `IValidationText`             | text contract      |
|  [02]   | `ValidationText`              | text factory       |
|  [03]   | `SingleValidationText`        | single message     |
|  [04]   | `ArrayValidationText`         | message collection |
|  [05]   | `IValidationTextFormatter<T>` | formatter contract |
|  [06]   | `SingleLineFormatter`         | line formatter     |
|  [07]   | `ValidationHelper`            | helper binding     |
|  [08]   | `ReactiveValidationObject`    | error object       |
|  [09]   | `IValidationBinding`          | binding contract   |
|  [10]   | `ValidationBinding`           | binding runtime    |

## [03]-[ENTRYPOINTS]

[RULE_ENTRYPOINTS]: validation rule operations
- rail: validation-ui

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                   | [RAIL]          |
| :-----: | :--------------------- | :------------------------------- | :-------------- |
|  [01]   | `ValidationRule`       | `ValidatableViewModelExtensions` | rule creation   |
|  [02]   | `ClearValidationRules` | `ValidatableViewModelExtensions` | rule removal    |
|  [03]   | `RegisterValidation`   | `ValidatableViewModelExtensions` | rule admission  |
|  [04]   | `IsValid`              | `ValidatableViewModelExtensions` | valid flag      |
|  [05]   | `ContainsProperty`     | `ValidatesPropertiesExtensions`  | property lookup |
|  [06]   | `ObserveFor`           | `ValidationContextExtensions`    | property state  |

[CONTEXT_ENTRYPOINTS]: context and component operations
- rail: validation-ui

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]         | [RAIL]           |
| :-----: | :----------------------- | :--------------------- | :--------------- |
|  [01]   | `Add`                    | `IValidationContext`   | component add    |
|  [02]   | `Remove`                 | `IValidationContext`   | component remove |
|  [03]   | `RemoveMany`             | `IValidationContext`   | component remove |
|  [04]   | `GetIsValid`             | `IValidationContext`   | valid stream     |
|  [05]   | `BuildText`              | `ValidationContext`    | text projection  |
|  [06]   | `Activate`               | `ValidationContext`    | context start    |
|  [07]   | `ValidationStatusChange` | `IValidationComponent` | change stream    |

[BINDING_ENTRYPOINTS]: view and helper binding operations
- rail: validation-ui

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT]             | [RAIL]        |
| :-----: | :---------------------------- | :------------------------- | :------------ |
|  [01]   | `BindValidation`              | `ViewForExtensions`        | view binding  |
|  [02]   | `ForProperty`                 | `ValidationBinding`        | property bind |
|  [03]   | `ForViewModel`                | `ValidationBinding`        | model bind    |
|  [04]   | `ForValidationHelperProperty` | `ValidationBinding`        | helper bind   |
|  [05]   | `BindToView`                  | `ValidationBinding`        | target bind   |
|  [06]   | `GetErrors`                   | `ReactiveValidationObject` | error lookup  |
|  [07]   | `ErrorsChanged`               | `ReactiveValidationObject` | error event   |

[TEXT_ENTRYPOINTS]: text and formatter operations
- rail: validation-ui

| [INDEX] | [SURFACE]      | [SURFACE_ROOT]                | [RAIL]          |
| :-----: | :------------- | :---------------------------- | :-------------- |
|  [01]   | `Create`       | `ValidationText`              | text factory    |
|  [02]   | `ToSingleLine` | `IValidationText`             | line projection |
|  [03]   | `Format`       | `IValidationTextFormatter<T>` | format text     |
|  [04]   | `Default`      | `SingleLineFormatter`         | default format  |

## [04]-[IMPLEMENTATION_LAW]

[VALIDATION_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: observable input validation, rule components, context state, text projection, and view binding
- Accept: screen validation joins command availability, view state, and receipt projection
- Reject: detached validation bags

[TEXT_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: validation text as typed data, not freeform string side channels
- Accept: panels, companion windows, sidecars, diagnostics, and support views share one validation vocabulary
- Reject: per-view validation message formats
