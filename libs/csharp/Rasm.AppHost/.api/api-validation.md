# [RASM_APPHOST_API_VALIDATION]

`FluentValidation` supplies validators, rule graphs, built-in validators, conditions, rule sets, child and polymorphic validation, validation contexts, descriptors, results, failures, exceptions, global options, and scanner facts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation`

- package: `FluentValidation`
- assembly: `FluentValidation`
- namespace: `FluentValidation`
- asset: runtime library
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validator and rule family

- rail: validation

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]       | [RAIL]                 |
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

[PUBLIC_TYPE_SCOPE]: result option and scanner family

- rail: validation

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]       | [RAIL]                     |
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

- rail: validation

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]         | [RAIL]                    |
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

- rail: validation

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY]     | [RAIL]                   |
| :-----: | :------------------------ | :----------------- | :----------------------- |
|  [01]   | `RuleFor`                 | property rule      | property validation      |
|  [02]   | `RuleForEach`             | collection rule    | item validation          |
|  [03]   | `RuleSet`                 | rule grouping      | named validation set     |
|  [04]   | `Include`                 | rule composition   | validator inclusion      |
|  [05]   | `SetValidator`            | child validator    | nested validation        |
|  [06]   | `SetAsyncValidator`       | child validator    | async nested validation  |
|  [07]   | `ChildRules`              | inline child rules | nested inline rules      |
|  [08]   | `SetInheritanceValidator` | polymorphic rules  | subtype validation       |
|  [09]   | `DependentRules`          | dependency rules   | ordered rule branch      |
|  [10]   | `When`                    | condition branch   | positive condition       |
|  [11]   | `Unless`                  | condition branch   | negative condition       |
|  [12]   | `WhenAsync`               | async condition    | async positive condition |
|  [13]   | `UnlessAsync`             | async condition    | async negative condition |

[ENTRYPOINT_SCOPE]: built-in validator operations

- rail: validation

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY]       | [RAIL]                  |
| :-----: | :----------------- | :------------------- | :---------------------- |
|  [01]   | `NotNull`          | null validator       | required value          |
|  [02]   | `NotEmpty`         | empty validator      | required content        |
|  [03]   | `Length`           | string validator     | length range            |
|  [04]   | `Matches`          | regex validator      | pattern match           |
|  [05]   | `EmailAddress`     | string validator     | email shape             |
|  [06]   | `Equal`            | comparison validator | equality comparison     |
|  [07]   | `NotEqual`         | comparison validator | inequality comparison   |
|  [08]   | `LessThan`         | comparison validator | upper bound             |
|  [09]   | `GreaterThan`      | comparison validator | lower bound             |
|  [10]   | `InclusiveBetween` | range validator      | inclusive range         |
|  [11]   | `ExclusiveBetween` | range validator      | exclusive range         |
|  [12]   | `IsInEnum`         | enum validator       | enum value              |
|  [13]   | `IsEnumName`       | enum validator       | enum name               |
|  [14]   | `PrecisionScale`   | decimal validator    | decimal precision       |
|  [15]   | `Must`             | predicate validator  | custom sync predicate   |
|  [16]   | `MustAsync`        | predicate validator  | custom async predicate  |
|  [17]   | `Custom`           | custom validator     | manual failure emission |
|  [18]   | `CustomAsync`      | custom validator     | async manual failure    |

[ENTRYPOINT_SCOPE]: execution and result operations

- rail: validation

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]      | [RAIL]                   |
| :-----: | :------------------------------------ | :------------------ | :----------------------- |
|  [01]   | `Validate`                            | sync execution      | validation result        |
|  [02]   | `ValidateAsync`                       | async execution     | async validation result  |
|  [03]   | `ValidateAndThrow`                    | exception execution | throws validation errors |
|  [04]   | `CreateDescriptor`                    | metadata extraction | rule descriptor          |
|  [05]   | `ValidationContext.CreateWithOptions` | context factory     | selector strategy        |
|  [06]   | `IncludeProperties`                   | selector strategy   | property subset          |
|  [07]   | `IncludeRuleSets`                     | selector strategy   | ruleset subset           |
|  [08]   | `IncludeAllRuleSets`                  | selector strategy   | all rulesets             |
|  [09]   | `ThrowOnFailures`                     | selector strategy   | exception mode           |
|  [10]   | `ValidationContext.AddFailure`        | failure emission    | custom failure           |
|  [11]   | `ValidationResult.ToDictionary`       | result projection   | grouped failures         |

[ENTRYPOINT_SCOPE]: rule option operations

- rail: validation

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]       | [RAIL]                  |
| :-----: | :--------------------- | :------------------- | :---------------------- |
|  [01]   | `Cascade`              | cascade option       | stop/continue policy    |
|  [02]   | `WithMessage`          | message option       | failure message         |
|  [03]   | `WithErrorCode`        | error-code option    | failure code            |
|  [04]   | `WithName`             | display-name option  | display name            |
|  [05]   | `OverridePropertyName` | property-name option | failure property name   |
|  [06]   | `WithState`            | state option         | custom failure state    |
|  [07]   | `WithSeverity`         | severity option      | failure severity        |
|  [08]   | `Configure`            | rule mutation        | low-level rule mutation |

## [04]-[IMPLEMENTATION_LAW]

[VALIDATION_TOPOLOGY]:

- namespaces: `FluentValidation`, `FluentValidation.Results`, `FluentValidation.Validators`
- rule families: property rule, collection rule, child validator, include rule, ruleset
- validator families: null/empty, string, regex, comparison, range, enum, decimal, predicate, custom, async
- condition rails: `When`, `Unless`, async conditions, rule-level condition scope
- selector rails: properties, rulesets, all rulesets, custom selector, throw-on-failures
- failure shape: property name, error code, attempted value, severity, custom state
- descriptor shape: rules, member validators, member rules, ruleset metadata
- cascade policy: class-level and rule-level cascade modes
- global policy: cascade defaults, language manager, selector factories, resolver delegates

[LOCAL_ADMISSION]:

- Boundary validators accumulate input failures before runtime state changes.
- Async validators stay outside hot runtime paths unless the boundary explicitly owns I/O.
- Rule sets are explicit boundary variants and do not become hidden conditional branches.
- Custom state and severity must map to typed rail errors.
- Validation failures fold into typed rail errors; exception-driven validation stays rejected.

[RAIL_LAW]:

- Package: `FluentValidation`
- Owns: input and options validation
- Accept: failures fold into typed rails
- Reject: inline imperative guards
