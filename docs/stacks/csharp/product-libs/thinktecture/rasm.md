# [THINKTECTURE_RASM]

Thinktecture owns domain shape. LanguageExt owns error and execution rails.

## [1][OWNERSHIP]

| [INDEX] | [PRESSURE]                            | [RESPONSE]                                              |
| :-----: | ------------------------------------- | ------------------------------------------------------- |
|   [1]   | Repeated primitive validation.        | `[ValueObject<T>]` or `[ComplexValueObject]`.           |
|   [2]   | Strings or native enum mirrors.       | `[SmartEnum<TKey>]` with item-owned behavior.           |
|   [3]   | Repeated switch/visitor cases.        | `[Union]` with generated `Switch`/`Map`.                |
|   [4]   | Boundary conversion.                  | Generated factory plus LanguageExt rail bridge.         |
|   [5]   | Union SelfOp emission policy.         | `[SkipUnionOps]` / `[GenerateUnionOps]` — see §2 below. |
|   [6]   | Dispatch union with threaded runtime. | `[Union(SwitchMapStateParameterName = …)]`.             |

## [2][UNION_OPS_POLICY]

Defined in `libs/csharp/Rasm/Domain/Validation.cs`. Enforced by CSP0802 in `Rasm.Domain` and `Rasm.Analysis` namespaces only.

| [INDEX] | [ATTRIBUTE]          | [EFFECT]                                                                         |
| :-----: | -------------------- | -------------------------------------------------------------------------------- |
|   [1]   | `[GenerateUnionOps]` | Emits `internal static readonly Op SelfOp = Op.Of(nameof(Case))` per sealed case |
|   [2]   | `[SkipUnionOps]`     | Opt out of CSP0802 SelfOp requirement                                            |

**Not generated:** `operator +` or `operator |` on union types. Hand operators live on separate structs/records or plain types outside CSP0802 scope.

Universal Thinktecture union attributes: `union-attributes.md`.

## [3][RHINO_GH2]

- Use value objects for tolerances, names, formula text, and scalar admission.
- Use smart enums for operation modes, metric families, port policy, and bounded UI/native vocabularies.
- Use unions for command intent, document mutation, output shape, and operation result variants.
- Keep Rhino validity, GH2 tree/path semantics, and MathNet diagnostics in their owning layers.

## [4][REJECTION]

- Do not create convenience wrappers around generated factories or dispatch.
- Do not keep parallel dictionaries beside smart enums.
- Do not make current implementation symbols the ceiling for generated shape.
- Do not use generated types decoratively around unchanged imperative code.
