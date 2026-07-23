# [RASM_APPHOST_API_VALIDATION]

`FluentValidation` owns boundary input and options validation: `AbstractValidator<T>` folds one rule graph over a policy or request record and accumulates every field failure into a `ValidationResult` before any runtime state changes. Its boundary is the composition edge — a policy record validates once at bootstrap, a request record at ingress — and the accumulated result folds onto the typed rail, never an exception on the domain path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation`
- package: `FluentValidation` (Apache-2.0)
- assembly: `FluentValidation`
- namespace: `FluentValidation`, `FluentValidation.Results`, `FluentValidation.Validators`
- asset: runtime library
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validator and rule family

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]       | [CAPABILITY]           |
| :-----: | :------------------------------------------ | :------------------ | :--------------------- |
|  [01]   | `AbstractValidator<T>`                      | validator base      | rule graph owner       |
|  [02]   | `InlineValidator<T>`                        | inline validator    | inline rule graph      |
|  [03]   | `IValidator<T>`                             | typed contract      | typed validation       |
|  [04]   | `IValidator`                                | untyped contract    | context validation     |
|  [05]   | `IValidatorDescriptor`                      | descriptor contract | rule inspection        |
|  [06]   | `ValidatorDescriptor<T>`                    | descriptor value    | rule metadata          |
|  [07]   | `IRuleBuilder<T,TProperty>`                 | rule builder        | property rule setup    |
|  [08]   | `IRuleBuilderOptions<T,TProperty>`          | rule option         | rule continuation      |
|  [09]   | `IRuleBuilderInitialCollection<T,TElement>` | collection builder  | collection rules       |
|  [10]   | `IValidationRule`                           | rule contract       | rule metadata          |
|  [11]   | `IValidationContext`                        | context contract    | validation input       |
|  [12]   | `ValidationContext<T>`                      | context value       | validation input state |
|  [13]   | `ValidationStrategy<T>`                     | selector strategy   | partial validation     |
|  [14]   | `IConditionBuilder`                         | condition builder   | otherwise branch       |

[PUBLIC_TYPE_SCOPE]: result, option, and scanner family

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]       | [CAPABILITY]               |
| :-----: | :-------------------------------------------- | :------------------ | :------------------------- |
|  [01]   | `ValidationResult`                            | result value        | validation receipt         |
|  [02]   | `ValidationFailure`                           | failure value       | field failure              |
|  [03]   | `ValidationException`                         | exception value     | throw-on-failure path      |
|  [04]   | `Severity`                                    | severity enum       | failure severity           |
|  [05]   | `CascadeMode`                                 | cascade enum        | stop/continue policy       |
|  [06]   | `ApplyConditionTo`                            | condition enum      | condition scope            |
|  [07]   | `ValidatorOptions`                            | global options root | global validation policy   |
|  [08]   | `ValidatorConfiguration`                      | global config       | selector/message policy    |
|  [09]   | `ValidatorSelectorOptions`                    | selector options    | rule selector factories    |
|  [10]   | `AssemblyScanner`                             | validator scanner   | validator discovery        |
|  [11]   | `AssemblyScanner.AssemblyScanResult`          | scan result         | discovered validator       |
|  [12]   | `LanguageManager`                             | localization policy | error-message localization |
|  [13]   | `AsyncValidatorInvokedSynchronouslyException` | async exception     | async rule guard           |

[PUBLIC_TYPE_SCOPE]: validator implementation family

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]         | [CAPABILITY]              |
| :-----: | :------------------------------------- | :-------------------- | :------------------------ |
|  [01]   | `DefaultValidatorExtensions`           | rule extensions       | built-in validators       |
|  [02]   | `DefaultValidatorOptions`              | rule options          | message/severity/options  |
|  [03]   | `IPropertyValidator<T,TProperty>`      | property validator    | sync property validation  |
|  [04]   | `IAsyncPropertyValidator<T,TProperty>` | property validator    | async property validation |
|  [05]   | `PropertyValidator<T,TProperty>`       | validator base        | custom sync validator     |
|  [06]   | `AsyncPropertyValidator<T,TProperty>`  | validator base        | custom async validator    |
|  [07]   | `PolymorphicValidator<T,TProperty>`    | inheritance validator | subtype validation        |
|  [08]   | `IChildValidatorAdaptor`               | child adapter         | nested validation         |
|  [09]   | `EmailValidationMode`                  | validator option      | email rule policy         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: rule graph operations

| [INDEX] | [SURFACE]                 | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------ | :------- | :------------------------ |
|  [01]   | `RuleFor`                 | instance | property rule graph       |
|  [02]   | `RuleForEach`             | instance | collection item rules     |
|  [03]   | `RuleSet`                 | instance | named rule grouping       |
|  [04]   | `Include`                 | instance | validator composition     |
|  [05]   | `SetValidator`            | static   | child validator attach    |
|  [06]   | `SetAsyncValidator`       | static   | async child validator     |
|  [07]   | `ChildRules`              | static   | inline nested rules       |
|  [08]   | `SetInheritanceValidator` | static   | polymorphic subtype rules |
|  [09]   | `DependentRules`          | instance | ordered dependent branch  |
|  [10]   | `When`                    | instance | positive condition scope  |
|  [11]   | `Unless`                  | instance | negative condition scope  |
|  [12]   | `WhenAsync`               | instance | async positive condition  |
|  [13]   | `UnlessAsync`             | instance | async negative condition  |

[ENTRYPOINT_SCOPE]: built-in validator operations

| [INDEX] | [SURFACE]          | [SHAPE] | [CAPABILITY]            |
| :-----: | :----------------- | :------ | :---------------------- |
|  [01]   | `NotNull`          | static  | required value          |
|  [02]   | `NotEmpty`         | static  | required content        |
|  [03]   | `Length`           | static  | length range            |
|  [04]   | `Matches`          | static  | regex pattern match     |
|  [05]   | `EmailAddress`     | static  | email shape             |
|  [06]   | `Equal`            | static  | equality comparison     |
|  [07]   | `NotEqual`         | static  | inequality comparison   |
|  [08]   | `LessThan`         | static  | upper bound             |
|  [09]   | `GreaterThan`      | static  | lower bound             |
|  [10]   | `InclusiveBetween` | static  | inclusive range         |
|  [11]   | `ExclusiveBetween` | static  | exclusive range         |
|  [12]   | `IsInEnum`         | static  | enum value              |
|  [13]   | `IsEnumName`       | static  | enum name               |
|  [14]   | `PrecisionScale`   | static  | decimal precision       |
|  [15]   | `Must`             | static  | custom sync predicate   |
|  [16]   | `MustAsync`        | static  | custom async predicate  |
|  [17]   | `Custom`           | static  | manual failure emission |
|  [18]   | `CustomAsync`      | static  | async manual failure    |

[ENTRYPOINT_SCOPE]: execution and result operations

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------ | :------- | :------------------------- |
|  [01]   | `Validate`                            | instance | sync validation result     |
|  [02]   | `ValidateAsync`                       | instance | async validation result    |
|  [03]   | `ValidateAndThrow`                    | static   | throw on failure           |
|  [04]   | `CreateDescriptor`                    | instance | rule descriptor extraction |
|  [05]   | `ValidationContext.CreateWithOptions` | factory  | selector strategy context  |
|  [06]   | `IncludeProperties`                   | instance | property subset selector   |
|  [07]   | `IncludeRuleSets`                     | instance | ruleset subset selector    |
|  [08]   | `IncludeAllRuleSets`                  | instance | all rulesets selector      |
|  [09]   | `ThrowOnFailures`                     | instance | exception mode selector    |
|  [10]   | `ValidationContext.AddFailure`        | instance | custom failure emission    |
|  [11]   | `ValidationResult.ToDictionary`       | instance | grouped failure projection |

[ENTRYPOINT_SCOPE]: rule option operations

| [INDEX] | [SURFACE]              | [SHAPE] | [CAPABILITY]            |
| :-----: | :--------------------- | :------ | :---------------------- |
|  [01]   | `Cascade`              | static  | stop/continue policy    |
|  [02]   | `WithMessage`          | static  | failure message         |
|  [03]   | `WithErrorCode`        | static  | failure code            |
|  [04]   | `WithName`             | static  | display name            |
|  [05]   | `OverridePropertyName` | static  | failure property name   |
|  [06]   | `WithState`            | static  | custom failure state    |
|  [07]   | `WithSeverity`         | static  | failure severity        |
|  [08]   | `Configure`            | static  | low-level rule mutation |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AbstractValidator<T>` folds one rule graph per type; each property chain short-circuits or continues on its `CascadeMode`.
- One `ValidationResult` accumulates every `ValidationFailure` — property name, error code, attempted value, severity, custom state — before any runtime state changes.
- Rule sets and `When`/`Unless` conditions are explicit boundary variants a `ValidationStrategy` selects, never hidden control flow.

[STACKING]:
- `api-languageext.md`(`libs/csharp/.api/api-languageext.md`): each `ValidationResult.Errors` failure maps to a LanguageExt `Error` accumulated as `Validation<Error, T>`, egressed through `.ToFin()` onto the `Fin<T>` rail every AppHost boundary op returns.
- `api-validation-di.md`(`.api/api-validation-di.md`): `AbstractValidator<T>` implementations register as `IValidator<T>` through the DI scanner, resolved at the composition root the boundary op reads.
- within-lib: AppHost composes one `AbstractValidator<T>` per policy or request record, and `ValidationContext.CreateWithOptions` under a `ValidationStrategy` drives partial validation over a property or ruleset subset at ingress.

[LOCAL_ADMISSION]:
- Boundary validators accumulate every input failure before runtime state changes.
- Async validators stay off hot runtime paths unless the boundary owns I/O.
- Rule sets stay explicit boundary variants, never hidden conditional branches.
- Custom state and severity map onto typed rail `Error` codes.

[RAIL_LAW]:
- Package: `FluentValidation`
- Owns: input and options validation at the composition boundary
- Accept: accumulated failures fold onto the typed rail
- Reject: inline imperative guard branches
