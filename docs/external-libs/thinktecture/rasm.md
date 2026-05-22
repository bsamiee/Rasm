# [H1][THINKTECTURE_RASM]
>**Dictum:** *Rasm uses generated shape to collapse primitive, enum, and variant sprawl.*

<br>

[IMPORTANT] Thinktecture owns domain shape. LanguageExt owns error and execution rails.

---
## [1][OWNERSHIP]
>**Dictum:** *One generated declaration replaces each repeated concept family.*

<br>

| [INDEX] | [PRESSURE] | [RESPONSE] |
| :-----: | ---------- | ---------- |
| [1] | Repeated primitive validation. | `[ValueObject<T>]` or `[ComplexValueObject]`. |
| [2] | Strings or native enum mirrors. | `[SmartEnum<TKey>]` with item-owned behavior. |
| [3] | Repeated switch/visitor cases. | `[Union]` with generated `Switch`/`Map`. |
| [4] | Boundary conversion. | Generated factory plus LanguageExt rail bridge. |

---
## [2][RHINO_GH2]
>**Dictum:** *Generated shape describes intent; native APIs keep semantics.*

<br>

- Use value objects for tolerances, names, formula text, and scalar admission.
- Use smart enums for operation modes, metric families, port policy, and bounded UI/native vocabularies.
- Use unions for command intent, document mutation, output shape, and operation result variants.
- Keep Rhino validity, GH2 tree/path semantics, and MathNet diagnostics in their owning layers.

---
## [3][REJECTION]
>**Dictum:** *Generated code is not a wrapper excuse.*

<br>

- Do not create convenience wrappers around generated factories or dispatch.
- Do not keep parallel dictionaries beside smart enums.
- Do not make current implementation symbols the ceiling for generated shape.
- Do not use generated types decoratively around unchanged imperative code.
