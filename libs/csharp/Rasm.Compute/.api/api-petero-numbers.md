# [RASM_COMPUTE_API_PETERO_NUMBERS]

`PeterO.Numbers` is the exact-rational arithmetic substrate behind the ℚ⁷ dimensional proof: `ERational` carries every SI base-dimension exponent as an exact numerator/denominator pair, so a `Powf(arg, 1/2)` half-power root stays exactly representable and a float-rounded exponent never decides dimensional consistency. It is also the CAS numeric bridge — AngouriMath `Entity.Number.Rational` leaves carry `ERational` and `Real` leaves carry `EDecimal`, so the symbolic fold consumes the engine's own number carriers with zero conversion loss.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PeterO.Numbers`
- package: `PeterO.Numbers`
- license: CC0-1.0
- assembly: `Numbers`
- namespace: `PeterO.Numbers` (`ERational`/`EDecimal`/`EInteger`/`EFloat`/`EContext`/`ERounding`/`ETrapException`)
- asset: single managed AnyCPU IL assembly; the workspace `net10.0` consumer binds the `netstandard1.0` build
- rail: exact-rational arithmetic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: arbitrary-precision number carriers
- namespace: `PeterO.Numbers`
- rail: exact-rational arithmetic

| [INDEX] | [SYMBOL]    | [CAPABILITY]                                                                                  |
| :-----: | :---------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `ERational` | exact rational — arbitrary-precision numerator/denominator, the ℚ⁷ dimension-exponent carrier |
|  [02]   | `EDecimal`  | arbitrary-precision decimal — the AngouriMath `Entity.Number.Real` leaf carrier               |
|  [03]   | `EInteger`  | arbitrary-precision integer — `ERational.Numerator`/`Denominator` component type              |
|  [04]   | `EFloat`    | arbitrary-precision binary float — the binary-radix peer `CompareToBinary` reads              |
|  [05]   | `EContext`  | precision/rounding context the `ToEDecimal(EContext)` egress consumes                         |
|  [06]   | `ERounding` | rounding-mode vocabulary an `EContext` row carries                                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mint, fold, compare, and egress exact rationals
- namespace: `PeterO.Numbers`
- rail: exact-rational arithmetic

| [INDEX] | [SURFACE]                                        | [CALL]                                                                        |
| :-----: | :----------------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `ERational.Zero` / `One` / `Ten`                 | canonical constants — the zero exponent vector seeds from `Zero`              |
|  [02]   | `ERational.FromInt32(int)`                       | integer lift — every UnitsNet `BaseDimensions` axis enters through it         |
|  [03]   | `ERational.FromEDecimal(EDecimal)`               | CAS bridge — a `Real` literal exponent lifts exactly                          |
|  [04]   | `ERational.Create(EInteger, EInteger)`           | numerator/denominator mint; `(int, int)` and `(long, long)` overloads         |
|  [05]   | `+` / `-` / `*` / `/` and `Add`/`Multiply`/…     | exact arithmetic — operator and instance-method spellings of one fold         |
|  [06]   | `Negate()` / `Abs()` / `ToLowestTerms()`         | sign and canonical-form projections                                           |
|  [07]   | `Equals(ERational)` / `CompareTo(ERational)`     | the only equality and ordering verbs — the type ships no comparison operators |
|  [08]   | `IsZero` / `IsNegative` / `Sign` / `IsInteger()` | classification probes the render and seam-projection folds read               |
|  [09]   | `Numerator` / `Denominator`                      | exact component egress as `EInteger`                                          |
|  [10]   | `ToEDecimal(EContext)` / `ToDouble()`            | lossy egress under an explicit context — never inside the exponent algebra    |
|  [11]   | `IsNaN()` / `IsInfinity()` / `IsFinite`          | special-value probes — specials reject at admission, never enter a monomial   |

## [04]-[IMPLEMENTATION_LAW]

[EXACTNESS_LAW]:
- `ERational` is the one dimension-exponent carrier — dimensional consistency is decided by exact equality, and a rounded `double`/`decimal` exponent silently admits an inconsistent formula.
- Lossy egress (`ToDouble`, `ToEDecimal(EContext)`) runs only at a render or diagnostic edge, never inside the exponent group algebra.
- `NaN`/infinity specials never enter a `DimensionMonomial`: admission lifts through `FromInt32`/`FromEDecimal` from finite sources.

[EQUALITY_LAW]:
- Every equality read spells `Equals` and every ordering read `CompareTo`; the type ships no `==`/`<` operators, so a phantom operator spelling fails at compile and generated structural equality composes `Equals`/`GetHashCode` directly.

[STACKING]:
- `AngouriMath`(`.api/api-angourimath.md`): `Entity.Number.Rational` carries `ERational` and `Real` carries `EDecimal`; `FromEDecimal` is the one lift from a CAS literal into the exponent algebra.
- `UnitsNet`(`.api/api-unitsnet.md`): the seven `BaseDimensions` `int` axes lift through `ERational.FromInt32` into the ℚ⁷ vector.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `[ValueObject<Seq<ERational>>]` structural equality rides `ERational.Equals`, making the monomial a comparer-free dictionary key.

[LOCAL_ADMISSION]:
- Folder-tier domain seam: the `Symbolic` dimensional-proof lane is the owning consumer; the `Rasm` kernel admits the package for its own folder-local seam and carries its own catalog.

[RAIL_LAW]:
- Package: `PeterO.Numbers`
- Owns: exact-rational and arbitrary-precision number carriers.
- Accept: dimension-exponent algebra, CAS numeric-leaf bridging, exact component egress.
- Reject: general numeric computation — dense/sparse algebra rides MathNet/CSparse and tensor kernels ride `System.Numerics.Tensors`.
