# [RASM_APPUI_API_REACTIVEUI_VALIDATION]

`ReactiveUI.Validation` is the reactive input-validation rail over ReactiveUI: one `ValidationContext` aggregates `IValidationComponent` rules and exposes a live `IObservable<bool> Valid` stream; `ValidationRule` is a polymorphic registrar over property-expression, observable, or `IValidationState` input; `IValidationText` carries typed message lists; and `BindValidation` projects state onto view properties. `IValidatableViewModel` and `ReactiveValidationObject` own view-model aggregation beside the self-contained `ReactiveUI.ReactiveProperty<T>` per-property mechanism. Precise namespaces keep contracts in their `*.Abstractions` owners, text collections in `.Collections`, and state comparison in `.Comparators`; the AppUi forms rail lifts them into `Validation<Error,T>`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Validation`
- package: `ReactiveUI.Validation` (MIT)
- assembly: `ReactiveUI.Validation`
- build-floor: `net10.0` (consumer-bound; multi-targets net8.0/net9.0/net462/net472/net481 + mobile/desktop RIDs — none bound here)
- namespace: `ReactiveUI.Validation.Abstractions` (`IValidatableViewModel`)
- namespace: `ReactiveUI.Validation.Contexts` (`IValidationContext`, `ValidationContext`)
- namespace: `ReactiveUI.Validation.Components` (`BasePropertyValidation<…>`, `ObservableValidation<…>`, `ObservableValidationBase<…>`)
- namespace: `ReactiveUI.Validation.Components.Abstractions` (`IValidationComponent`, `IPropertyValidationComponent`, `IValidatesProperties`)
- namespace: `ReactiveUI.Validation.States` (`IValidationState`, `ValidationState`)
- namespace: `ReactiveUI.Validation.Collections` (`IValidationText`, `ValidationText`, `SingleValidationText`, `ArrayValidationText`)
- namespace: `ReactiveUI.Validation.Comparators` (`ValidationStateComparer`)
- namespace: `ReactiveUI.Validation.Formatters` (`SingleLineFormatter`)
- namespace: `ReactiveUI.Validation.Formatters.Abstractions` (`IValidationTextFormatter<T>`)
- namespace: `ReactiveUI.Validation.Helpers` (`ValidationHelper`, `ReactiveValidationObject`)
- namespace: `ReactiveUI.Validation.ValidationBindings` (`ValidationBinding`)
- namespace: `ReactiveUI.Validation.ValidationBindings.Abstractions` (`IValidationBinding`)
- namespace: `ReactiveUI.Validation.Extensions` (`ValidatableViewModelExtensions`, `ValidatesPropertiesExtensions`, `ValidationContextExtensions`, `ViewForExtensions`)
- asset: runtime library
- rail: validation-ui

## [02]-[PUBLIC_TYPES]

[CONTEXT_AND_STATE_TYPES]: validation context and state — rail: validation-ui

| [INDEX] | [SYMBOL]                  | [NAMESPACE]     | [KIND]                                                   |
| :-----: | :------------------------ | :-------------- | :------------------------------------------------------- |
|  [01]   | `IValidatableViewModel`   | `.Abstractions` | view-model contract (`ValidationContext`)                |
|  [02]   | `IValidationContext`      | `.Contexts`     | context contract (`IValidationComponent`, `ICancelable`) |
|  [03]   | `ValidationContext`       | `.Contexts`     | context runtime (`ctor(IScheduler?)`)                    |
|  [04]   | `IValidationState`        | `.States`       | state contract (`IsValid`/`Text`)                        |
|  [05]   | `ValidationState`         | `.States`       | sealed record state value + static `Valid`               |
|  [06]   | `ValidationStateComparer` | `.Comparators`  | `IEqualityComparer<IValidationState>`                    |

[COMPONENT_TYPES]: validation components — rail: validation-ui

| [INDEX] | [SYMBOL]                                   | [NAMESPACE]                | [KIND]                                        |
| :-----: | :----------------------------------------- | :------------------------- | :-------------------------------------------- |
|  [01]   | `IValidationComponent`                     | `.Components.Abstractions` | `ValidationStatusChange` stream owner         |
|  [02]   | `IPropertyValidationComponent`             | `.Components.Abstractions` | property component (`: IValidatesProperties`) |
|  [03]   | `IValidatesProperties`                     | `.Components.Abstractions` | property-set contract                         |
|  [04]   | `BasePropertyValidation<T>`                | `.Components`              | property base                                 |
|  [05]   | `BasePropertyValidation<T,TValue>`         | `.Components`              | typed property rule                           |
|  [06]   | `ObservableValidationBase<T,TValue>`       | `.Components`              | observable-rule base                          |
|  [07]   | `ObservableValidation<T,TValue>`           | `.Components`              | observable rule                               |
|  [08]   | `ObservableValidation<T,TValue,TProperty>` | `.Components`              | property-bound observable rule                |

[TEXT_AND_BINDING_TYPES]: validation text, helpers, and bindings — rail: validation-ui

| [INDEX] | [SYMBOL]                      | [NAMESPACE]                        | [KIND]                                                        |
| :-----: | :---------------------------- | :--------------------------------- | :------------------------------------------------------------ |
|  [01]   | `IValidationText`             | `.Collections`                     | `IReadOnlyList<string>` text contract                         |
|  [02]   | `ValidationText`              | `.Collections`                     | static text factory + `None`/`Empty`                          |
|  [03]   | `SingleValidationText`        | `.Collections`                     | single message                                                |
|  [04]   | `ArrayValidationText`         | `.Collections`                     | message collection                                            |
|  [05]   | `IValidationTextFormatter<T>` | `.Formatters.Abstractions`         | formatter contract                                            |
|  [06]   | `SingleLineFormatter`         | `.Formatters`                      | `IValidationTextFormatter<string>` + static `Default`         |
|  [07]   | `ValidationHelper`            | `.Helpers`                         | rule-binding handle (`IsValid`/`Message`/`ValidationChanged`) |
|  [08]   | `ReactiveValidationObject`    | `.Helpers`                         | `ReactiveObject` + `INotifyDataErrorInfo` base                |
|  [09]   | `IValidationBinding`          | `.ValidationBindings.Abstractions` | binding contract                                              |
|  [10]   | `ValidationBinding`           | `.ValidationBindings`              | binding runtime (static factories)                            |

## [03]-[ENTRYPOINTS]

[RULE_ENTRYPOINTS]: validation rule operations
- rail: validation-ui

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                   | [RAIL]                    |
| :-----: | :--------------------- | :------------------------------- | :------------------------ |
|  [01]   | `ValidationRule<…>`    | `ValidatableViewModelExtensions` | polymorphic rule creation |
|  [02]   | `ClearValidationRules` | `ValidatableViewModelExtensions` | rule removal              |
|  [03]   | `IsValid<TViewModel>`  | `ValidatableViewModelExtensions` | aggregate valid stream    |
|  [04]   | `ContainsProperty<…>`  | `ValidatesPropertiesExtensions`  | property lookup           |
|  [05]   | `ObserveFor<…>`        | `ValidationContextExtensions`    | per-property state stream |

[VALIDATION_RULE]: Ten public overloads discriminate among property-expression, `IObservable<T>`, `IObservable<IValidationState>`, and `IObservable<bool>` inputs. Each routes through the internal `RegisterValidation<TComponent>` helper, which constructs a `BasePropertyValidation` or `ObservableValidation` component.

[CLEAR_VALIDATION_RULES]: Two overloads remove all rules or the rules associated with a property.

[IS_VALID]: `IsValid<TViewModel>()` returns `IObservable<bool>`.

[CONTAINS_PROPERTY]: `ContainsProperty<TVM,TProp>(expr, bool exclusively)` performs the lookup.

[OBSERVE_FOR]: `ObserveFor<TVM,TProp>(expr, bool strict = true)` returns `IObservable<IList<IValidationState>>`.

[CONTEXT_ENTRYPOINTS]: context and component operations
- rail: validation-ui

| [INDEX] | [SURFACE]                                                  | [SURFACE_ROOT]         | [RAIL]                            |
| :-----: | :--------------------------------------------------------- | :--------------------- | :-------------------------------- |
|  [01]   | `Valid` (`IObservable<bool>`) / `IsValid` / `GetIsValid()` | `IValidationContext`   | aggregate valid stream + snapshot |
|  [02]   | `Validations` (`IObservableList<IValidationComponent>`)    | `IValidationContext`   | live component list               |
|  [03]   | `Add` / `Remove` / `RemoveMany`                            | `IValidationContext`   | component mutation                |
|  [04]   | `Text` (`IValidationText`) / `ValidationStatusChange`      | `ValidationContext`    | aggregate text + state stream     |
|  [05]   | `ValidationStatusChange` (`IObservable<IValidationState>`) | `IValidationComponent` | per-component change stream       |

[BINDING_ENTRYPOINTS]: view and helper binding operations
- rail: validation-ui

| [INDEX] | [SURFACE]                                                      | [SURFACE_ROOT]             | [RAIL]                         |
| :-----: | :------------------------------------------------------------- | :------------------------- | :----------------------------- |
|  [01]   | `BindValidation<…>`                                            | `ViewForExtensions`        | view binding                   |
|  [02]   | `ForProperty` / `ForViewModel` / `ForValidationHelperProperty` | `ValidationBinding`        | binding factories              |
|  [03]   | `IsValid` / `Message` / `ValidationChanged`                    | `ValidationHelper`         | rule-handle projection         |
|  [04]   | `GetErrors` / `ErrorsChanged` / `HasErrors`                    | `ReactiveValidationObject` | `INotifyDataErrorInfo` surface |

[BIND_VALIDATION]: Three overloads bind a property, a single target, or a `ValidationHelper` property.

[VALIDATION_BINDING_FACTORIES]: Two `strict` and `formatter` overloads per factory return `IValidationBinding`; internal `BindToView` applies the binding.

[VALIDATION_HELPER]: `Message` carries `IValidationText`.

[TEXT_ENTRYPOINTS]: text and formatter operations
- rail: validation-ui

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]                | [RAIL]             |
| :-----: | :--------------- | :---------------------------- | :----------------- |
|  [01]   | `Create`         | `ValidationText`              | typed-text factory |
|  [02]   | `None` / `Empty` | `ValidationText`              | empty sentinels    |
|  [03]   | `ToSingleLine`   | `IValidationText`             | line projection    |
|  [04]   | `Format`         | `IValidationTextFormatter<T>` | text formatting    |
|  [05]   | `Default`        | `SingleLineFormatter`         | default formatter  |

[CREATE]: Factory overloads accept `string?`, `IEnumerable<string?>`, `params string?[]`, or `IEnumerable<IValidationText>`.

[EMPTY_TEXT]: `None` and `Empty` are static `IValidationText` values.

[TO_SINGLE_LINE]: `ToSingleLine(string? separator = ",")` projects one line.

[FORMAT]: `Format(IValidationText)` returns the formatter's `T` result.

[DEFAULT_FORMATTER]: `Default` is a static `SingleLineFormatter`.

## [04]-[IMPLEMENTATION_LAW]

[VALIDATION_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: observable input validation — the `ValidationContext` component aggregator, the polymorphic `ValidationRule` registrar, `IValidationText` typed messages, `IValidationState` change streams, and `BindValidation` view projection
- Stacks: the AppUi forms rail lifts each `FormField` rule into the suite-wide `Validation<Error,T>` applicative so independent field errors accumulate rather than short-circuit, then feeds the all-valid fold into the `Gate` context-validity stream the command table reads; the `ValidationContext.Valid` stream and `IsValid<TViewModel>()` join command availability through a `CommandIntent` gated on the valid fold, and `IValidationText` is the one validation vocabulary panels, companion windows, sidecars, and diagnostics share
- Accept: screen validation joins command availability, view state, and receipt projection through the `Valid`/`ValidationStatusChange` observable streams; rules register through the single polymorphic `ValidationRule`
- Reject: detached validation bags; a second per-view validation scheme beside the one `Validation<Error,T>` lift

[AOT_TRIM_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: the expression-tree rule path — `ValidationRule`/`ClearValidationRules`/`BindValidation` carry `[RequiresDynamicCode]` + `[RequiresUnreferencedCode]` because they route through ReactiveUI `WhenAnyValue` expression trees
- Accept: under the AppUi `net10.0` desktop posture (JIT) the expression-tree rules resolve normally; a future trimmed/AOT publish must preserve the validated members or shift those rules to the observable (`IObservable<TValue>`) `ValidationRule` overloads that avoid expression trees
- Reject: assuming the property-expression `ValidationRule` overloads are trim-safe; silently trimming validated view-model members

[TEXT_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: validation text as typed `IValidationText` data (`SingleValidationText`/`ArrayValidationText` under `.Collections`), not freeform string side channels; `ValidationText.None`/`Empty` are the canonical empty sentinels and `SingleLineFormatter.Default` is the default line projection
- Accept: panels, companion windows, sidecars, diagnostics, and support views share one validation vocabulary projected through `IValidationTextFormatter<T>`
- Reject: per-view validation message formats; constructing freeform error strings outside the `ValidationText` factory
