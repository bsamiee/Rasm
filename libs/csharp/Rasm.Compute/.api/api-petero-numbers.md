# [RASM_COMPUTE_API_PETERO_NUMBERS]

`PeterO.Numbers` is the exact-rational arithmetic substrate behind the ℚ⁷ dimensional proof: `ERational` carries every SI base-dimension exponent as an exact numerator/denominator pair, so a half-power root stays exactly representable and a float-rounded exponent never decides dimensional consistency. `PeterO.Numbers` also bridges the CAS numeric tower — AngouriMath `Entity.Number.Rational` leaves carry `ERational` and `Real` leaves carry `EDecimal` — so the symbolic fold consumes the engine's own carriers with zero conversion loss.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PeterO.Numbers`
- package: `PeterO.Numbers` (CC0-1.0)
- assembly: `Numbers`
- namespace: `PeterO.Numbers`
- asset: single managed AnyCPU IL assembly; the `netstandard1.0` build binds on `net10.0`
- rail: exact-rational arithmetic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: arbitrary-precision number carriers

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                                  |
| :-----: | :---------- | :------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `ERational` | class         | exact rational — arbitrary-precision numerator/denominator, the ℚ⁷ dimension-exponent carrier |
|  [02]   | `EDecimal`  | class         | arbitrary-precision decimal — the AngouriMath `Entity.Number.Real` leaf carrier               |
|  [03]   | `EInteger`  | class         | arbitrary-precision integer — `ERational.Numerator`/`Denominator` component type              |
|  [04]   | `EFloat`    | class         | arbitrary-precision binary float — the `CompareToBinary` binary-radix peer                    |
|  [05]   | `EContext`  | class         | precision/rounding context the `ToEDecimal(EContext)` egress consumes                         |
|  [06]   | `ERounding` | enum          | rounding-mode vocabulary an `EContext` carries                                                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mint, fold, compare, and egress exact rationals

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                                          |
| :-----: | :----------------------------------------------- | :------- | :-------------------------------------------------------------------- |
|  [01]   | `ERational.Zero` / `One` / `Ten`                 | static   | canonical constants — the zero exponent vector seeds from `Zero`      |
|  [02]   | `ERational.FromInt32(int)`                       | factory  | integer lift — every UnitsNet `BaseDimensions` axis enters here       |
|  [03]   | `ERational.FromEDecimal(EDecimal)`               | factory  | CAS bridge — a `Real` literal exponent lifts exactly                  |
|  [04]   | `ERational.Create(EInteger, EInteger)`           | factory  | numerator/denominator mint; `(int,int)` and `(long,long)` overloads   |
|  [05]   | `+` / `-` / `*` / `/` and `Add`/`Multiply`/…     | fold     | exact arithmetic — operator and instance-method spellings of one fold |
|  [06]   | `Negate()` / `Abs()` / `ToLowestTerms()`         | instance | sign and canonical-form projections                                   |
|  [07]   | `Equals(ERational)` / `CompareTo(ERational)`     | instance | the sole equality and ordering verbs — no comparison operators ship   |
|  [08]   | `IsZero` / `IsNegative` / `Sign` / `IsInteger()` | property | classification probes the render and seam-projection folds read       |
|  [09]   | `Numerator` / `Denominator`                      | property | exact component egress as `EInteger`                                  |
|  [10]   | `ToEDecimal(EContext)` / `ToDouble()`            | instance | lossy egress under an explicit context                                |
|  [11]   | `IsNaN()` / `IsInfinity()` / `IsFinite`          | instance | special-value probes — specials reject at admission                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ERational` is the sole dimension-exponent carrier; dimensional consistency decides by exact equality, and a rounded `double`/`decimal` exponent silently admits an inconsistent formula.
- Lossy egress (`ToDouble`, `ToEDecimal(EContext)`) runs only at a render or diagnostic edge, never inside the exponent group algebra.
- `NaN`/infinity specials never enter a `DimensionMonomial`; admission lifts finite sources through `FromInt32`/`FromEDecimal`.
- Every equality read spells `Equals` and every ordering read `CompareTo`; the type ships no `==`/`<` operators, so a phantom operator spelling fails at compile and generated structural equality composes `Equals`/`GetHashCode`.

[STACKING]:
- `AngouriMath`(`.api/api-angourimath.md`): `Entity.Number.Rational` carries `ERational` and `Real` carries `EDecimal`; `FromEDecimal` is the one lift from a CAS literal into the exponent algebra.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): the seven `BaseDimensions` `int` axes lift through `ERational.FromInt32` into the ℚ⁷ vector.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `[ValueObject<Seq<ERational>>]` structural equality rides `ERational.Equals`, making the monomial a comparer-free dictionary key.

[LOCAL_ADMISSION]:
- `Symbolic` owns consumption through its dimensional-proof lane; `Rasm` admits the package for its own folder-local seam and carries its own catalog.

[RAIL_LAW]:
- Package: `PeterO.Numbers`
- Owns: exact-rational and arbitrary-precision number carriers.
- Accept: dimension-exponent algebra, CAS numeric-leaf bridging, exact component egress.
- Reject: general numeric computation — dense/sparse algebra rides MathNet/CSparse and tensor kernels ride `System.Numerics.Tensors`.
