# [RASM_APPHOST_API_VALIDATION]

`FluentValidation` supplies validators, rule graphs, built-in validators, conditions,
rule sets, child and polymorphic validation, validation contexts, descriptors, results,
failures, exceptions, global options, and scanner facts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation`
- package: `FluentValidation`
- assembly: `FluentValidation`
- namespace: `FluentValidation`
- asset: runtime library
- rail: validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validator and rule family
- rail: validation

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]       | [RAIL]                 |
| :-----: | :------------------------------------------ | :------------------ | :--------------------- |
|   [1]   | `AbstractValidator<T>`                      | validator base      | rule graph owner       |
|   [2]   | `InlineValidator<T>`                        | inline validator    | inline rule graph      |
|   [3]   | `IValidator<T>`                             | typed contract      | typed validation       |
|   [4]   | `IValidator`                                | untyped contract    | context validation     |
|   [5]   | `IValidatorDescriptor`                      | descriptor contract | rule inspection        |
|   [6]   | `ValidatorDescriptor<T>`                    | descriptor value    | rule metadata          |
|   [7]   | `IRuleBuilder<T,TProperty>`                 | rule builder        | property rule setup    |
|   [8]   | `IRuleBuilderOptions<T,TProperty>`          | rule option         | rule continuation      |
|   [9]   | `IRuleBuilderInitialCollection<T,TElement>` | collection builder  | collection rules       |
|  [10]   | `IValidationRule`                           | rule contract       | rule metadata          |
|  [11]   | `IValidationContext`                        | context contract    | validation input       |
|  [12]   | `ValidationContext<T>`                      | context value       | validation input state |
|  [13]   | `ValidationStrategy<T>`                     | selector strategy   | partial validation     |
|  [14]   | `IConditionBuilder`                         | condition builder   | otherwise branch       |

[PUBLIC_TYPE_SCOPE]: result option and scanner family
- rail: validation

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]       | [RAIL]                     |
| :-----: | :-------------------------------------------- | :------------------ | :------------------------- |
|   [1]   | `ValidationResult`                            | result value        | validation receipt         |
|   [2]   | `ValidationFailure`                           | failure value       | field failure              |
|   [3]   | `ValidationException`                         | exception value     | throw-on-failure path      |
|   [4]   | `Severity`                                    | severity enum       | failure severity           |
|   [5]   | `CascadeMode`                                 | cascade enum        | stop/continue policy       |
|   [6]   | `ApplyConditionTo`                            | condition enum      | condition scope            |
|   [7]   | `ValidatorOptions`                            | global options root | global validation policy   |
|   [8]   | `ValidatorConfiguration`                      | global config       | selector/message policy    |
|   [9]   | `ValidatorSelectorOptions`                    | selector options    | rule selector factories    |
|  [10]   | `AssemblyScanner`                             | validator scanner   | validator discovery        |
|  [11]   | `AssemblyScanner.AssemblyScanResult`          | scan result         | discovered validator       |
|  [12]   | `LanguageManager`                             | localization policy | error-message localization |
|  [13]   | `AsyncValidatorInvokedSynchronouslyException` | async exception     | async rule guard           |

[PUBLIC_TYPE_SCOPE]: validator implementation family
- rail: validation

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]         | [RAIL]                    |
| :-----: | :------------------------------------- | :-------------------- | :------------------------ |
|   [1]   | `DefaultValidatorExtensions`           | rule extensions       | built-in validators       |
|   [2]   | `DefaultValidatorOptions`              | rule options          | message/severity/options  |
|   [3]   | `IPropertyValidator<T,TProperty>`      | property validator    | sync property validation  |
|   [4]   | `IAsyncPropertyValidator<T,TProperty>` | property validator    | async property validation |
|   [5]   | `PropertyValidator<T,TProperty>`       | validator base        | custom sync validator     |
|   [6]   | `AsyncPropertyValidator<T,TProperty>`  | validator base        | custom async validator    |
|   [7]   | `PolymorphicValidator<T,TProperty>`    | inheritance validator | subtype validation        |
|   [8]   | `IChildValidatorAdaptor`               | child adapter         | nested validation         |
|   [9]   | `EmailValidationMode`                  | validator option      | email rule policy         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: rule graph operations
- rail: validation

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY]     | [RAIL]                   |
| :-----: | :------------------------ | :----------------- | :----------------------- |
|   [1]   | `RuleFor`                 | property rule      | property validation      |
|   [2]   | `RuleForEach`             | collection rule    | item validation          |
|   [3]   | `RuleSet`                 | rule grouping      | named validation set     |
|   [4]   | `Include`                 | rule composition   | validator inclusion      |
|   [5]   | `SetValidator`            | child validator    | nested validation        |
|   [6]   | `SetAsyncValidator`       | child validator    | async nested validation  |
|   [7]   | `ChildRules`              | inline child rules | nested inline rules      |
|   [8]   | `SetInheritanceValidator` | polymorphic rules  | subtype validation       |
|   [9]   | `DependentRules`          | dependency rules   | ordered rule branch      |
|  [10]   | `When`                    | condition branch   | positive condition       |
|  [11]   | `Unless`                  | condition branch   | negative condition       |
|  [12]   | `WhenAsync`               | async condition    | async positive condition |
|  [13]   | `UnlessAsync`             | async condition    | async negative condition |

[ENTRYPOINT_SCOPE]: built-in validator operations
- rail: validation

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY]       | [RAIL]                  |
| :-----: | :----------------- | :------------------- | :---------------------- |
|   [1]   | `NotNull`          | null validator       | required value          |
|   [2]   | `NotEmpty`         | empty validator      | required content        |
|   [3]   | `Length`           | string validator     | length range            |
|   [4]   | `Matches`          | regex validator      | pattern match           |
|   [5]   | `EmailAddress`     | string validator     | email shape             |
|   [6]   | `Equal`            | comparison validator | equality comparison     |
|   [7]   | `NotEqual`         | comparison validator | inequality comparison   |
|   [8]   | `LessThan`         | comparison validator | upper bound             |
|   [9]   | `GreaterThan`      | comparison validator | lower bound             |
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
|   [1]   | `Validate`                            | sync execution      | validation result        |
|   [2]   | `ValidateAsync`                       | async execution     | async validation result  |
|   [3]   | `ValidateAndThrow`                    | exception execution | throws validation errors |
|   [4]   | `CreateDescriptor`                    | metadata extraction | rule descriptor          |
|   [5]   | `ValidationContext.CreateWithOptions` | context factory     | selector strategy        |
|   [6]   | `IncludeProperties`                   | selector strategy   | property subset          |
|   [7]   | `IncludeRuleSets`                     | selector strategy   | ruleset subset           |
|   [8]   | `IncludeAllRuleSets`                  | selector strategy   | all rulesets             |
|   [9]   | `ThrowOnFailures`                     | selector strategy   | exception mode           |
|  [10]   | `ValidationContext.AddFailure`        | failure emission    | custom failure           |
|  [11]   | `ValidationResult.ToDictionary`       | result projection   | grouped failures         |

[ENTRYPOINT_SCOPE]: rule option operations
- rail: validation

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]       | [RAIL]                  |
| :-----: | :--------------------- | :------------------- | :---------------------- |
|   [1]   | `Cascade`              | cascade option       | stop/continue policy    |
|   [2]   | `WithMessage`          | message option       | failure message         |
|   [3]   | `WithErrorCode`        | error-code option    | failure code            |
|   [4]   | `WithName`             | display-name option  | display name            |
|   [5]   | `OverridePropertyName` | property-name option | failure property name   |
|   [6]   | `WithState`            | state option         | custom failure state    |
|   [7]   | `WithSeverity`         | severity option      | failure severity        |
|   [8]   | `Configure`            | rule mutation        | low-level rule mutation |

## [4]-[IMPLEMENTATION_LAW]

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
