# [PY_BRANCH_API_PROTOVALIDATE]

`protovalidate` evaluates `buf.validate` standard and CEL constraints directly from protobuf descriptors. Generated `protobuf-py` messages enter once, and each refusal retains typed violation evidence as a protobuf detail.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation engine, refusal, and violation evidence

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `Validator`        | class         | compiles and caches descriptor rules for repeated validation                |
|  [02]   | `Violation`        | value class   | retains field, rule, rule id, message, key posture, and protobuf projection |
|  [03]   | `ValidationError`  | exception     | carries every constraint violation and projects them as `Violations`        |
|  [04]   | `CompilationError` | exception     | reports a rule set that cannot compile                                      |
|  [05]   | `EvaluationError`  | exception     | reports a rule that cannot evaluate                                         |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shared validation and structured violation projection

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `validate(message, *, fail_fast=False)`                     | static   | validate through the shared cached engine; return on admission |
|  [02]   | `collect_violations(message, *, fail_fast=False)`           | static   | return every `Violation` without raising a constraint refusal  |
|  [03]   | `Validator(registry=None)`                                  | ctor     | construct isolated engine over optional extension registry     |
|  [04]   | `Validator.validate(message, *, fail_fast=False)`           | instance | validate through the owned engine                              |
|  [05]   | `Validator.collect_violations(message, *, fail_fast=False)` | instance | return violations through the owned engine                     |
|  [06]   | `ValidationError.to_proto() -> Violations`                  | instance | return `buf.validate.Violations`                               |
|  [07]   | `ValidationError.violations`                                | property | read the typed violation sequence                              |
|  [08]   | `Violation.proto`                                           | property | read one `buf.validate.Violation` protobuf                     |

- `validate`: admits both `protobuf-py` and `google.protobuf` messages; this branch passes generated `protobuf-py` classes alone.
- `ValidationError.to_proto`: returns a `protobuf-py` `buf.validate.Violations` message that `ConnectError.details` packs without translation.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Generated descriptors remain the constraint authority; validation reads their embedded rules and keeps field-law logic out of producers and transports.

[STACKING]:
- `protobuf-py`(`protobuf-py.md`): generated descriptors feed `validate`; refusal details return on the same runtime.
- `connectrpc`(`connectrpc.md`): body interception maps request refusals to `INVALID_ARGUMENT` and response refusals to `INTERNAL`. `ValidationError.to_proto()` passes through `ConnectError.details` without translation.
- `runtime/transport/serve`: one body interceptor validates unary and streamed elements; metadata admission retains its own interceptor.
- `artifacts/graphic/texture/set`: producers validate the completed generated appearance document before publication; byte measurement reads that admitted binary.

[ADMISSION]:
- Runtime serving evaluates the generated rule set at every request and response body crossing; compilation and evaluation defects map to `INTERNAL`.
- Artifact producers validate completed generated documents before publication; contract violations retain rule ids instead of becoming field-validator prose.
