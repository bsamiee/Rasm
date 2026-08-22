# [RASM_API_PROTOVALIDATE]

`ProtoValidate` owns runtime evaluation of the `buf.validate` rules the corpus declares on every generated message: one validator reads the rules off a message's descriptor, evaluates them through the embedded CEL engine, and returns a typed violation list a boundary admission projects onto the fault rail. Rule authorship stays with the `.proto` corpus and the generated descriptors stay with `Rasm.Contracts`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ProtoValidate`
- package: `ProtoValidate` (Apache-2.0, TELUS)
- assembly: `ProtoValidate` (binds `lib/net8.0`; `lib/netstandard2.0` beside it)
- namespace: `ProtoValidate`, `Buf.Validate`
- depends: `Cel` (the expression engine), `Google.Protobuf`, `Microsoft.Extensions.Options`
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: evaluator owners

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Validator`                  | class         | rule evaluation over any `IMessage` by descriptor   |
|  [02]   | `IValidator`                 | interface     | the evaluation contract a host registers            |
|  [03]   | `ValidatorOptions`           | class         | descriptor preload and lazy-build posture           |
|  [04]   | `ValidationResult`           | class         | the violation list with its success projection      |
|  [05]   | `ServiceCollectionExtensions`| static class  | `AddProtoValidate` host registration                |
|  [06]   | `ViolationExtensions`        | static class  | `CreateViolation` for a descriptor-addressed rule   |
|  [07]   | `FieldPathExtensions`        | static class  | `GetPath` rendering of a violation's field path     |

[PUBLIC_TYPE_SCOPE]: `Buf.Validate` generated messages (the `validate.proto` descriptor the emission references)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `ValidateReflection` | static class  | the `buf/validate/validate.proto` `FileDescriptor`     |
|  [02]   | `ValidateExtensions` | static class  | `Field`, `Message`, `Oneof`, and `Predefined` handles  |
|  [03]   | `Violation`          | message       | `RuleId`, `Message`, `Field`, `Rule`, `ForKey`, `Value` |
|  [04]   | `Violations`         | message       | the wire-shaped violation list                         |
|  [05]   | `FieldPath`          | message       | the addressed field chain of one violation             |
|  [06]   | `FieldPathElement`   | message       | one chain step with its number, name, type, and key    |
|  [07]   | `FieldRules`         | message       | the per-field rule union every scalar rule rides       |
|  [08]   | `MessageRules`       | message       | message-level CEL rules and oneof requirements         |

[RULE_MESSAGE]: `FloatRules` `DoubleRules` `Int32Rules` `Int64Rules` `UInt32Rules` `UInt64Rules` `SInt32Rules` `SInt64Rules` `Fixed32Rules` `Fixed64Rules` `SFixed32Rules` `SFixed64Rules` `BoolRules` `StringRules` `BytesRules` `EnumRules` `RepeatedRules` `MapRules` `AnyRules` `DurationRules` `TimestampRules` `OneofRules` `MessageOneofRule` `PredefinedRules` `Rule`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: evaluation (`Validator`)

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `Validator()`                                        | ctor     | lazy evaluator build per message type on first validate     |
|  [02]   | `Validator(ValidatorOptions)`                        | ctor     | preloaded descriptors, lazy build disabled where declared   |
|  [03]   | `Validator(IOptions<ValidatorOptions>)`              | ctor     | host-options seat for the `AddProtoValidate` registration   |
|  [04]   | `Validate(IMessage, bool failFast) -> ValidationResult` | instance | every rule, or the first refusal under `failFast`        |
|  [05]   | `GetEvaluatorDebugString(IMessage) -> string`        | instance | the compiled evaluator tree for one message type            |

[ENTRYPOINT_SCOPE]: options and result

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `ValidatorOptions.FileDescriptors`                  | property | the descriptor set whose rules compile ahead of use     |
|  [02]   | `ValidatorOptions.PreLoadDescriptors`               | property | compile every listed descriptor at construction         |
|  [03]   | `ValidatorOptions.DisableLazy`                      | property | refuse a message type outside the preloaded set         |
|  [04]   | `ValidationResult.Violations -> List<Violation>`    | property | the refused rules in evaluation order                   |
|  [05]   | `ValidationResult.IsSuccess -> bool`                | property | zero violations                                         |
|  [06]   | `ValidationResult.Empty`                            | static   | the shared success value                                |
|  [07]   | `AddProtoValidate(IServiceCollection[, Action<ValidatorOptions>])` | static | registers `IValidator` with its options seat |
|  [08]   | `GetPath(FieldPath) -> string`                      | static   | dotted field-path text for a violation                  |
|  [09]   | `CreateViolation(FieldDescriptor, string ruleId, string message) -> Violation` | static | a descriptor-addressed violation a custom rule mints |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Validator.Validate` evaluates the rules carried by the message's own descriptor, so the generated `Rasm.Contracts` assembly referencing `ValidateReflection.Descriptor` is the whole rule source; no rule is restated in C#.
- `failFast` stops at the first refusal; a boundary that renders every violation onto `google.rpc.BadRequest.FieldViolation` passes `false`.
- `ValidationResult` never throws on a refusal; a malformed rule expression throws at evaluator build, which `PreLoadDescriptors` moves to construction.
- `Violation.Field` carries a `FieldPath`; `GetPath` renders it as the dotted path a `FieldViolation.Field` column carries.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): the evaluator reads `IMessage` through reflection descriptors and never through a parse, so admission validates after `MessageParser<T>` and before the interior sees the value.
- `Google.Api.CommonProtos`(`.api/api-commonprotos.md`): `Violation` rows project onto `BadRequest.Types.FieldViolation{Field = GetPath(Field), Reason = RuleId, Description = Message}` and ride `fault.v1.FaultDetail.violations` inside the `google.rpc.Status` detail the transport carries.
- `Cel`: the expression engine every `(buf.validate.message).cel` rule compiles through; it ships as a dependency and is never composed directly.
- remote-contracts seam: one validator per process, registered once, admits every generated message family under one rule set.

[LOCAL_ADMISSION]:
- Validation runs exactly once at the wire boundary on the admitted message; interior owners hold validated values and re-check nothing.
- A `ValidationResult` with violations projects onto the typed fault rail as the contract-violation band, never as an exception.

[RAIL_LAW]:
- Package: `ProtoValidate`
- Owns: runtime evaluation of corpus-declared `buf.validate` rules on generated messages
- Accept: one registered `IValidator` invoked at admission, `failFast: false` where violations render to a peer
- Reject: hand-written per-field checks that restate a rule the descriptor already carries
- `Rasm.Contracts` (`Rasm.Contracts.csproj`): references the package for the `Buf.Validate.ValidateReflection` descriptor every emitted file lists as a dependency.
- `Rasm.Compute` (`Runtime/wire#PROTO_VOCABULARY`): `ParseGuard.Read` runs the ONE process `Validator` (`PreLoadDescriptors` over every `<F>Reflection.Descriptor`, `DisableLazy`) after the parse and before the interior, `failFast: false`, the violations projecting onto `WireFault.InvalidRequest` and the producer's `FaultContext.Violations`.
