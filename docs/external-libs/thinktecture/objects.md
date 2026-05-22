# [H1][THINKTECTURE_OBJECTS]
>**Dictum:** *Primitive values enter Rasm through one generated gate.*

<br>

[IMPORTANT] Value objects define admission and equality. They do not replace LanguageExt rails.

---
## [1][VALUE_OBJECTS]
>**Dictum:** *Generated factories are the validation boundary.*

<br>

| [INDEX] | [CAPABILITY] | [RULE] |
| :-----: | ------------ | ------ |
| [1] | Factory generation | Use generated `Create`, `TryCreate`, `Validate`, or configured factory names directly. |
| [2] | Custom validation errors | Convert generator errors once into Rasm `Error` or `Fault`. |
| [3] | Comparers | Put string comparison policy on the generated declaration. |
| [4] | Conversion operators | Enable only when interop removes boundary noise. |
| [5] | Arithmetic operators | Use only where failure cannot be hidden. |

---
## [2][COMPLEX_OBJECTS]
>**Dictum:** *Composite identity belongs in the generated type when fields form one concept.*

<br>

Use complex value objects for normalized ranges, sample windows, tolerance bundles, formula variable sets, and boundary descriptors. Do not create parallel records beside a value object for the same concept.

---
## [3][DANGEROUS_OPTIONS]
>**Dictum:** *Convenience switches change domain admission semantics.*

<br>

| [INDEX] | [OPTION] | [RASM_POLICY] |
| :-----: | -------- | ------------- |
| [1] | `SkipFactoryMethods` | Rare custom-rail choice; document lost generated surface. |
| [2] | `AllowDefaultStructs` | Boundary-only; default values often hide invalid state. |
| [3] | `NullInFactoryMethodsYieldsNull` | External binding only; domain absence uses `Option<T>`. |
| [4] | `EmptyStringInFactoryMethodsYieldsNull` | Text boundary only with explicit protocol semantics. |

---
## [4][RAIL_BRIDGE]
>**Dictum:** *Generated validation and LanguageExt rails meet once.*

<br>

- Generated validation admits or rejects raw values.
- LanguageExt carries failure through `Fin<T>` or `Validation<Error,T>`.
- Rhino/GH/MathNet projections preserve native validity and tolerance in error detail.
- Do not wrap generated factories in single-call helpers.
