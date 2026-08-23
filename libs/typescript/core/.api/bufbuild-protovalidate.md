# [TS_CORE_API_BUFBUILD_PROTOVALIDATE]

`@bufbuild/protovalidate` evaluates the corpus's own `buf.validate` rules over `@bufbuild/protobuf` descriptors at runtime: one `Validator` minted over the one registry, one `validate(schema, message)` per admission, a three-kind result electing on shape. `interchange/format` runs it behind the `$typeName` guard on every decode and every encode, so no branch page carries a field rule the corpus already states.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protovalidate`
- package: `@bufbuild/protovalidate` (Apache-2.0)
- peer: `@bufbuild/protobuf` (`DescMessage`/`MessageShape`/`MessageValidType`/`Registry`; `../../.api/bufbuild-protobuf.md`)
- effect-peer: none direct — a verdict folds onto the `ParseError` rail inside `interchange/format`'s one `Schema.filter`
- runtime: universal; CEL evaluated in-process, RE2 matched by the ECMAScript engine unless `regexMatch` supplies one
- module: single `.` export, dual ESM+CJS

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the validator, its options, the verdict, and the violation — rail interchange/format

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                                                      |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `Validator`              | validator     | `validate<Desc>(schema, message): ValidationResult<MessageValidType, Shape>`             |
|  [02]   | `ValidatorOptions`       | mint options  | `registry` (predefined rules + `Any` in CEL), `failFast`, `regexMatch`, `legacyRequired` |
|  [03]   | `ValidationResult<V, I>` | verdict       | `kind: "valid"` (message narrowed) \| `"invalid"` (+ `violations`) \| `"error"`          |
|  [04]   | `Violation`              | violation     | `message`, `ruleId`, `field: Path`, `rule: Path`, `forKey`; `toString()`                 |
|  [05]   | `ValidationError`        | invalid error | `name: "ValidationError"`, `violations: Violation[]` — the `invalid` arm's `error`       |
|  [06]   | `CompilationError`       | rule defect   | a rule that fails to compile — the corpus's defect, never the document's                 |
|  [07]   | `RuntimeError`           | eval defect   | a rule that fails to evaluate — the same class as a compile defect                       |
|  [08]   | `RegexMatcher`           | engine seat   | RE2-compliant matcher the `regexMatch` option admits                                     |

- [VERDICT_SHAPE]: `valid` narrows the message to `MessageValidType<Desc>`. This branch generates with `valid_types=protovalidate_required`, so required message fields become nonoptional on admitted values. Generator v2.14.0 still includes the unset face of a required oneof. Total consumers close that impossible admitted arm explicitly instead of weakening every required field back to `MessageShape<Desc>`.
- [ERROR_ARM]: `error` is a defect of the RULE SET (compile or evaluate), a different fact from `invalid`; `interchange/format` lands it as `ParseResult.Forbidden` so a census never counts a broken rule as a stream of bad documents.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mint once, validate per message, render a violation — rail interchange/format

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                               |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `createValidator(options?): Validator`             | mint           | ONE per registry at `interchange/format` module init              |
|  [02]   | `validator.validate(schema, message)`              | verdict        | behind `isMessage(value, schema)`; decode AND encode every family |
|  [03]   | `violationsToProto(violations)`                    | wire           | `[Violations, ViolationsSchema]` — the violations message pair    |
|  [04]   | `violationToProto(violation)`                      | wire           | one `Violation`; unmined — violations cross as `FieldViolation`   |
|  [05]   | `pathFromViolationProto(schema, proto, registry?)` | path           | reverse of the wire form; unmined                                 |
|  [06]   | `createStandardSchema(schema, validator?)`         | bridge         | Standard Schema adapter; unmined — `effect` owns admission        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one validator, one registry: `createValidator({ registry })` takes `interchange/format`'s `Registry`, so predefined rules and `Any`-typed CEL resolve against the same suite every decode does, and a second validator would re-compile every rule set.
- the verdict is DATA electing on `kind`; `failFast` stays off so a document's every violation reaches the refusal, which is what a caller re-authoring a form needs.
- rules are the corpus's: `buf.yaml` declares the `protovalidate` dep, `buf lint` grades the declarations, and this runtime evaluates them — no branch page restates a range, a length, a pattern, or a oneof requirement.

[STACKING]:
- `@bufbuild/protobuf`(`../../.api/bufbuild-protobuf.md`): generated descriptors drive validation and `pathToString` renders coordinates.
- `effect`(`libs/typescript/.api/effect.md`): the verdict folds inside ONE `Schema.filter` — `true`, an array of `FilterIssue`, or a `ParseResult.Forbidden` — so a violation is a `ParseError` on the decode rail and no second refusal family exists.
- `interchange/codec`: remote `FaultDetail.violations` are the PEER's `google.rpc.BadRequest.FieldViolation` rows, not this package's `Violation`; the two never convert into each other.

[LOCAL_ADMISSION]:
- mint once at `interchange/format`; run behind the `$typeName` guard; keep `failFast` off; land `error` as `Forbidden` and `invalid` as issues; never call `validate` from a consumer page.

[RAIL_LAW]:
- Package: `@bufbuild/protovalidate`
- Owns: rule compilation and evaluation over generated descriptors, the three-kind verdict, the `Violation` coordinate pair, and the `Violations` wire form
- Accept: one `createValidator({ registry })` at the format owner, `validate` on every admission and egress, the verdict folded onto `ParseError`
- Reject: a second validator, a hand field rule beside a generated message, `failFast`, a consumer-page `validate`, `createStandardSchema` where `effect` `Schema` owns admission
