# [RASM_APPHOST_API_VALIDATION]

`FluentValidation` supplies rule graphs, validation results, validators, and boundary input contracts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation`
- package: `FluentValidation`
- assembly: `FluentValidation`
- namespace: `FluentValidation`
- asset: runtime library
- rail: validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation family
- rail: validation

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]    | [CAPABILITY]                |
| :-----: | :--------------------------------- | :---------------- | :-------------------------- |
|   [1]   | `AbstractValidator<T>`             | rule graph        | anchors validation contract |
|   [2]   | `IValidator<T>`                    | contract surface  | defines boundary contract   |
|   [3]   | `IRuleBuilder<T,TProperty>`        | builder surface   | constructs configured root  |
|   [4]   | `IRuleBuilderOptions<T,TProperty>` | policy object     | carries policy input        |
|   [5]   | `ValidationResult`                 | result value      | projects receipt state      |
|   [6]   | `ValidationFailure`                | result value      | projects receipt state      |
|   [7]   | `ValidationContext<T>`             | operation context | carries operation state     |
|   [8]   | `Severity`                         | failure severity  | anchors validation contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: validation operations
- rail: validation

| [INDEX] | [SURFACE]       | [CALL_SHAPE]            | [CAPABILITY]              |
| :-----: | :-------------- | :---------------------- | :------------------------ |
|   [1]   | `RuleFor`       | rule builder            | defines field rule        |
|   [2]   | `RuleForEach`   | collection rule builder | defines item rule         |
|   [3]   | `Must`          | predicate rule          | adds predicate rule       |
|   [4]   | `Custom`        | custom rule             | adds custom failure       |
|   [5]   | `When`          | conditional rule        | gates rule branch         |
|   [6]   | `Unless`        | negative condition      | excludes rule branch      |
|   [7]   | `Validate`      | validation method       | returns validation result |
|   [8]   | `ValidateAsync` | async validator         | validates asynchronously  |

## [4]-[IMPLEMENTATION_LAW]

[VALIDATION_TOPOLOGY]:
- namespaces: `FluentValidation`, `FluentValidation.Results`, `FluentValidation.Validators`
- rule families: property rule, collection rule, child validator, include rule, ruleset
- condition rails: `When`, `Unless`, async conditions, rule-level condition scope
- failure shape: property name, error code, attempted value, severity, custom state
- cascade policy: class-level and rule-level cascade modes

[LOCAL_ADMISSION]:
- Boundary validators accumulate input failures before runtime state changes.
- Async validators stay outside hot runtime paths unless the boundary explicitly owns I/O.
- Validation failures fold into typed rail errors; exception-driven validation stays rejected.

[RAIL_LAW]:
- Package: `FluentValidation`
- Owns: input and options validation
- Accept: failures fold into typed rails
- Reject: inline imperative guards

