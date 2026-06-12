# [RASM_APPUI_API_REACTIVEUI_VALIDATION]

`ReactiveUI.Validation` supplies validation contexts, observable validation components, validation text, view bindings, and helper objects for AppUi screen inputs.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[CONTEXT_AND_STATE_TYPES]: validation context and state
- rail: validation-ui

| [INDEX] | [SYMBOL]                  | [RAIL]              |
| :-----: | :------------------------ | :------------------ |
|   [1]   | `IValidatableViewModel`   | view-model contract |
|   [2]   | `IValidationContext`      | context contract    |
|   [3]   | `ValidationContext`       | context runtime     |
|   [4]   | `IValidationState`        | state contract      |
|   [5]   | `ValidationState`         | state value         |
|   [6]   | `ValidationStateComparer` | state comparer      |

[COMPONENT_TYPES]: validation components
- rail: validation-ui

| [INDEX] | [SYMBOL]                                   | [RAIL]             |
| :-----: | :----------------------------------------- | :----------------- |
|   [1]   | `IValidationComponent`                     | component contract |
|   [2]   | `IPropertyValidationComponent`             | property component |
|   [3]   | `IValidatesProperties`                     | property set       |
|   [4]   | `BasePropertyValidation<T>`                | property base      |
|   [5]   | `BasePropertyValidation<T,TValue>`         | typed property     |
|   [6]   | `ObservableValidation<T,TValue>`           | observable rule    |
|   [7]   | `ObservableValidation<T,TValue,TProperty>` | property rule      |

[TEXT_AND_BINDING_TYPES]: validation text, helpers, and bindings
- rail: validation-ui

| [INDEX] | [SYMBOL]                      | [RAIL]             |
| :-----: | :---------------------------- | :----------------- |
|   [1]   | `IValidationText`             | text contract      |
|   [2]   | `ValidationText`              | text factory       |
|   [3]   | `SingleValidationText`        | single message     |
|   [4]   | `ArrayValidationText`         | message collection |
|   [5]   | `IValidationTextFormatter<T>` | formatter contract |
|   [6]   | `SingleLineFormatter`         | line formatter     |
|   [7]   | `ValidationHelper`            | helper binding     |
|   [8]   | `ReactiveValidationObject`    | error object       |
|   [9]   | `IValidationBinding`          | binding contract   |
|  [10]   | `ValidationBinding`           | binding runtime    |

## [3]-[ENTRYPOINTS]

[RULE_ENTRYPOINTS]: validation rule operations
- rail: validation-ui

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                   | [RAIL]          |
| :-----: | :--------------------- | :------------------------------- | :-------------- |
|   [1]   | `ValidationRule`       | `ValidatableViewModelExtensions` | rule creation   |
|   [2]   | `ClearValidationRules` | `ValidatableViewModelExtensions` | rule removal    |
|   [3]   | `RegisterValidation`   | `ValidatableViewModelExtensions` | rule admission  |
|   [4]   | `IsValid`              | `ValidatableViewModelExtensions` | valid flag      |
|   [5]   | `ContainsProperty`     | `ValidatesPropertiesExtensions`  | property lookup |
|   [6]   | `ObserveFor`           | `ValidationContextExtensions`    | property state  |

[CONTEXT_ENTRYPOINTS]: context and component operations
- rail: validation-ui

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]         | [RAIL]           |
| :-----: | :----------------------- | :--------------------- | :--------------- |
|   [1]   | `Add`                    | `IValidationContext`   | component add    |
|   [2]   | `Remove`                 | `IValidationContext`   | component remove |
|   [3]   | `RemoveMany`             | `IValidationContext`   | component remove |
|   [4]   | `GetIsValid`             | `IValidationContext`   | valid stream     |
|   [5]   | `BuildText`              | `ValidationContext`    | text projection  |
|   [6]   | `Activate`               | `ValidationContext`    | context start    |
|   [7]   | `ValidationStatusChange` | `IValidationComponent` | change stream    |

[BINDING_ENTRYPOINTS]: view and helper binding operations
- rail: validation-ui

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT]             | [RAIL]        |
| :-----: | :---------------------------- | :------------------------- | :------------ |
|   [1]   | `BindValidation`              | `ViewForExtensions`        | view binding  |
|   [2]   | `ForProperty`                 | `ValidationBinding`        | property bind |
|   [3]   | `ForViewModel`                | `ValidationBinding`        | model bind    |
|   [4]   | `ForValidationHelperProperty` | `ValidationBinding`        | helper bind   |
|   [5]   | `BindToView`                  | `ValidationBinding`        | target bind   |
|   [6]   | `GetErrors`                   | `ReactiveValidationObject` | error lookup  |
|   [7]   | `ErrorsChanged`               | `ReactiveValidationObject` | error event   |

[TEXT_ENTRYPOINTS]: text and formatter operations
- rail: validation-ui

| [INDEX] | [SURFACE]      | [SURFACE_ROOT]                | [RAIL]          |
| :-----: | :------------- | :---------------------------- | :-------------- |
|   [1]   | `Create`       | `ValidationText`              | text factory    |
|   [2]   | `ToSingleLine` | `IValidationText`             | line projection |
|   [3]   | `Format`       | `IValidationTextFormatter<T>` | format text     |
|   [4]   | `Default`      | `SingleLineFormatter`         | default format  |

## [4]-[IMPLEMENTATION_LAW]

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
