# [RASM_APPUI_API_REACTIVEUI_VALIDATION]

`ReactiveUI.Validation` is the reactive input-validation rail over ReactiveUI: one `ValidationContext` aggregates `IValidationComponent` rules and exposes a live `IObservable<bool> Valid` stream; `ValidationRule` is a single polymorphic registrar (10 overloads discriminating on property-expression vs. observable vs. `IValidationState` input); `IValidationText` carries validation messages as a typed `IReadOnlyList<string>` rather than freeform strings; and `BindValidation` projects component state onto view properties. It owns its validation surface through `IValidatableViewModel`/`ReactiveValidationObject` (whose `INotifyDataErrorInfo` is fed by the `ValidationContext`) — it is the view-model-scoped validation aggregator and does not extend the core `ReactiveUI.ReactiveProperty<T>`, whose own `INotifyDataErrorInfo` (`AddValidationError`/`HasErrors`/`ObserveHasErrors`) is a self-contained core-ReactiveUI per-property mechanism the two rails sit beside. Namespaces are precise and do NOT flatten: the contracts live in `.Abstractions`, `.Components.Abstractions`, `.Formatters.Abstractions`, and `.ValidationBindings.Abstractions`; the text collections in `.Collections`; the state comparer in `.Comparators`. The AppUi forms rail (`Editing/forms`) lifts these into the suite-wide `Validation<Error,T>` applicative.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Validation`
- package: `ReactiveUI.Validation`
- version: `7.1.0`
- license: `MIT`
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

| [INDEX] | [SYMBOL]                  | [NAMESPACE]    | [KIND]                                   |
| :-----: | :------------------------ | :------------- | :--------------------------------------- |
|  [01]   | `IValidatableViewModel`   | `.Abstractions` | view-model contract (`ValidationContext`) |
|  [02]   | `IValidationContext`      | `.Contexts`    | context contract (`IValidationComponent`, `ICancelable`) |
|  [03]   | `ValidationContext`       | `.Contexts`    | context runtime (`ctor(IScheduler?)`)    |
|  [04]   | `IValidationState`        | `.States`      | state contract (`IsValid`/`Text`)        |
|  [05]   | `ValidationState`         | `.States`      | sealed record state value + static `Valid` |
|  [06]   | `ValidationStateComparer` | `.Comparators` | `IEqualityComparer<IValidationState>`    |

[COMPONENT_TYPES]: validation components — rail: validation-ui

| [INDEX] | [SYMBOL]                                   | [NAMESPACE]              | [KIND]             |
| :-----: | :----------------------------------------- | :----------------------- | :----------------- |
|  [01]   | `IValidationComponent`                     | `.Components.Abstractions` | `ValidationStatusChange` stream owner |
|  [02]   | `IPropertyValidationComponent`             | `.Components.Abstractions` | property component (`: IValidatesProperties`) |
|  [03]   | `IValidatesProperties`                     | `.Components.Abstractions` | property-set contract |
|  [04]   | `BasePropertyValidation<T>`                | `.Components`            | property base       |
|  [05]   | `BasePropertyValidation<T,TValue>`         | `.Components`            | typed property rule |
|  [06]   | `ObservableValidationBase<T,TValue>`       | `.Components`            | observable-rule base |
|  [07]   | `ObservableValidation<T,TValue>`           | `.Components`            | observable rule     |
|  [08]   | `ObservableValidation<T,TValue,TProperty>` | `.Components`            | property-bound observable rule |

[TEXT_AND_BINDING_TYPES]: validation text, helpers, and bindings — rail: validation-ui

| [INDEX] | [SYMBOL]                      | [NAMESPACE]                    | [KIND]             |
| :-----: | :---------------------------- | :----------------------------- | :----------------- |
|  [01]   | `IValidationText`             | `.Collections`                 | `IReadOnlyList<string>` text contract |
|  [02]   | `ValidationText`              | `.Collections`                 | static text factory + `None`/`Empty` |
|  [03]   | `SingleValidationText`        | `.Collections`                 | single message     |
|  [04]   | `ArrayValidationText`         | `.Collections`                 | message collection |
|  [05]   | `IValidationTextFormatter<T>` | `.Formatters.Abstractions`     | formatter contract |
|  [06]   | `SingleLineFormatter`         | `.Formatters`                  | `IValidationTextFormatter<string>` + static `Default` |
|  [07]   | `ValidationHelper`            | `.Helpers`                     | rule-binding handle (`IsValid`/`Message`/`ValidationChanged`) |
|  [08]   | `ReactiveValidationObject`    | `.Helpers`                     | `ReactiveObject` + `INotifyDataErrorInfo` base |
|  [09]   | `IValidationBinding`          | `.ValidationBindings.Abstractions` | binding contract |
|  [10]   | `ValidationBinding`           | `.ValidationBindings`          | binding runtime (static factories) |

## [03]-[ENTRYPOINTS]

[RULE_ENTRYPOINTS]: validation rule operations
- rail: validation-ui

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]                   | [RAIL]          |
| :-----: | :----------------------------------------------------- | :------------------------------- | :-------------- |
|  [01]   | `ValidationRule<…>` (10 public overloads: property-expr / `IObservable<T>` / `IObservable<IValidationState>` / `IObservable<bool>`; each routes through the `internal RegisterValidation<TComponent>` helper that constructs a `BasePropertyValidation`/`ObservableValidation` component) | `ValidatableViewModelExtensions` | polymorphic rule creation |
|  [02]   | `ClearValidationRules` / `ClearValidationRules<…>(prop)` (2 overloads) | `ValidatableViewModelExtensions` | rule removal (all / per-property) |
|  [03]   | `IsValid<TViewModel>()` -> `IObservable<bool>`         | `ValidatableViewModelExtensions` | aggregate valid stream |
|  [04]   | `ContainsProperty<TVM,TProp>(expr, bool exclusively)` | `ValidatesPropertiesExtensions`  | property lookup |
|  [05]   | `ObserveFor<TVM,TProp>(expr, bool strict = true)` -> `IObservable<IList<IValidationState>>` | `ValidationContextExtensions` | per-property state stream |

[CONTEXT_ENTRYPOINTS]: context and component operations
- rail: validation-ui

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]         | [RAIL]                |
| :-----: | :----------------------------------------------------- | :--------------------- | :-------------------- |
|  [01]   | `Valid` (`IObservable<bool>`) / `IsValid` / `GetIsValid()` | `IValidationContext` | aggregate valid stream + snapshot |
|  [02]   | `Validations` (`IObservableList<IValidationComponent>`) | `IValidationContext`  | live component list    |
|  [03]   | `Add` / `Remove` / `RemoveMany`                        | `IValidationContext`   | component mutation     |
|  [04]   | `Text` (`IValidationText`) / `ValidationStatusChange`  | `ValidationContext`    | aggregate text + state stream |
|  [05]   | `ValidationStatusChange` (`IObservable<IValidationState>`) | `IValidationComponent` | per-component change stream |

[BINDING_ENTRYPOINTS]: view and helper binding operations
- rail: validation-ui

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]             | [RAIL]        |
| :-----: | :----------------------------------------------------- | :------------------------- | :------------ |
|  [01]   | `BindValidation<…>` (3 overloads: property / single-target / `ValidationHelper`-property) | `ViewForExtensions` | view binding |
|  [02]   | `ForProperty` / `ForViewModel` / `ForValidationHelperProperty` (2 overloads each, `strict`/`formatter`) -> `IValidationBinding` | `ValidationBinding` | binding factories (`BindToView` is internal) |
|  [03]   | `IsValid` / `Message` (`IValidationText`) / `ValidationChanged` | `ValidationHelper`  | rule-handle projection |
|  [04]   | `GetErrors` / `ErrorsChanged` / `HasErrors`            | `ReactiveValidationObject` | `INotifyDataErrorInfo` surface |

[TEXT_ENTRYPOINTS]: text and formatter operations
- rail: validation-ui

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]                | [RAIL]          |
| :-----: | :----------------------------------------------------- | :---------------------------- | :-------------- |
|  [01]   | `Create(string?)` / `Create(IEnumerable<string?>)` / `Create(params string?[])` / `Create(IEnumerable<IValidationText>)` | `ValidationText` | typed-text factory |
|  [02]   | `None` / `Empty` (static `IValidationText`)            | `ValidationText`              | empty sentinels |
|  [03]   | `ToSingleLine(string? separator = ",")`                | `IValidationText`             | line projection |
|  [04]   | `Format(IValidationText)`                              | `IValidationTextFormatter<T>` | format text     |
|  [05]   | `Default` (static `SingleLineFormatter`)               | `SingleLineFormatter`         | default formatter |

## [04]-[IMPLEMENTATION_LAW]

[VALIDATION_LAW]:
- Package: `ReactiveUI.Validation`
- Owns: observable input validation — the `ValidationContext` component aggregator, the polymorphic `ValidationRule` registrar, `IValidationText` typed messages, `IValidationState` change streams, and `BindValidation` view projection
- Stacks: the AppUi forms rail (`Editing/forms#FORM_SCHEMA`) lifts each `FormField`'s rule into the suite-wide `Validation<Error,T>` applicative so independent field errors accumulate rather than short-circuit, then feeds the all-valid fold into the same `Gate` context-validity stream the command table reads (`Shell/screens#VALIDATION_UX`); the `ValidationContext.Valid` stream and `IsValid<TViewModel>()` join command availability (a `CommandIntent` gates on the valid fold), and `IValidationText` is the one validation vocabulary panels, companion windows, sidecars, and diagnostics share
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
