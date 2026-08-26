# [RASM_API_CELLY_PROTOVALIDATE]

`Celly.Protovalidate` evaluates corpus-authored `buf.validate` rules against generated `IMessage` values and returns structured violations.

Admission owners project each violation onto their typed fault carrier.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: evaluator owners

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `Validator`                  | class         | concurrent rule evaluation over `IMessage` values |
|  [02]   | `ValidationException`        | exception     | rule evaluation failure                           |
|  [03]   | `ValidationCompileException` | exception     | malformed rule or descriptor failure              |

[PUBLIC_TYPE_SCOPE]: `Buf.Validate` generated messages

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------------ | :------------ | :----------------------------------------------------- |
|  [01]   | `ValidateReflection`                  | static class  | `buf/validate/validate.proto` descriptor               |
|  [02]   | `ValidateExtensions`                  | static class  | message, oneof, field, and predefined option handles   |
|  [03]   | `Violation`                           | message       | rule identity, message, field path, rule path, and key |
|  [04]   | `FieldPath`                           | message       | ordered field-address elements                         |
|  [05]   | `FieldPathElement`                    | message       | field identity plus an optional collection subscript   |
|  [06]   | `FieldPathElement.SubscriptOneofCase` | enum          | index and bool, int, uint, or string map-key cases     |
|  [07]   | `FieldRules`                          | message       | per-field rule union                                   |
|  [08]   | `MessageRules`                        | message       | message CEL rules and oneof requirements               |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: validation lifecycle

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Validator(params FileDescriptor[])`                       | ctor     | build one descriptor-aware evaluator |
|  [02]   | `Validator.Validate(IMessage) -> IReadOnlyList<Violation>` | instance | accumulate every rule refusal        |
|  [03]   | `Violation.Field`                                          | property | structured refused field coordinate  |
|  [04]   | `Violation.RuleId`                                         | property | authored rule identity               |
|  [05]   | `Violation.Message`                                        | property | authored refusal description         |
|  [06]   | `FieldPath.Elements`                                       | property | ordered address traversal            |
|  [07]   | `FieldPathElement.FieldName`                               | property | canonical proto field spelling       |
|  [08]   | `FieldPathElement.FieldNumber`                             | property | numeric fallback field identity      |
|  [09]   | `FieldPathElement.SubscriptCase`                           | property | collection-coordinate discriminant   |
|  [10]   | `FieldPathElement.Index`                                   | property | repeated-field index                 |
|  [11]   | `FieldPathElement.BoolKey`                                 | property | boolean map key                      |
|  [12]   | `FieldPathElement.IntKey`                                  | property | signed map key                       |
|  [13]   | `FieldPathElement.UintKey`                                 | property | unsigned map key                     |
|  [14]   | `FieldPathElement.StringKey`                               | property | string map key                       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Validator` caches compiled programs and admits concurrent calls; one process owner constructs it from the closed descriptor-root set.
- `Validate` returns an empty list for success and every `Violation` for refusal; malformed rules raise during compilation or evaluation.
- Bootstrap validates one default instance per admitted non-map message so readiness proves every reachable rule compiles.
- `Violation.Field` retains typed collection coordinates; the boundary owner alone renders its field path exhaustively.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): `Validator` reads generated messages through descriptors after bounded parsing.
- `Google.Api.CommonProtos`(`.api/api-commonprotos.md`): the admission owner projects `Violation` onto `BadRequest.Types.FieldViolation`.
- `Celly.Protobuf`: the package adapts protobuf values and descriptors into the CEL evaluator internally.

[LOCAL_ADMISSION]:
- Validation runs once at each wire admission before the interior receives the message.
- The allowed descriptor-name set rejects messages outside the process contract before validation.
