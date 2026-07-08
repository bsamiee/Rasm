# [RASM_COMPUTE_API_UNITSNET]

`UnitsNet` supplies source-generated quantity structs, unit enums, a value
carrier that unifies `double` and `decimal`, culture-scoped parse/format,
generic-math operator quantities, the SI dimension vector with its own algebra,
and a single mutable setup root for conversion/abbreviation policy. It is the
measured-execution boundary owner: every unit-bearing Compute input
canonicalizes through it exactly once, and only raw SI scalars cross interior
signatures or the wire. The substrate
canonical member catalog is `libs/csharp/.api/api-unitsnet.md`; this overlay carries
only the Compute delta — the admission rail, dimensional bridge, and conversion
policy the `Symbolic/units` and `Symbolic/dimensional` pages compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-unitsnet.md`
- the quantity-contract/metadata/statics type rosters, the admitted family and unit-enum tables, and the ingress/operator/policy call-shape tables live on the substrate catalog — this overlay never re-states them
- rail: units

## [02]-[COMPUTE_BINDINGS]

[COMPUTE_BINDINGS]:
- the Compute `Admit` rail consumes the ERASED `IQuantity` face (`QuantityInfo`/`Dimensions`/`Unit`/`As`) so ONE polymorphic entrypoint admits every family without a per-family signature; text, value+enum, and value+abbreviation ingress converge on the same `As(canonical)` projection, and conversion runs exactly once at admission — interior numerics are raw `double` owned by Rasm core.
- `QuantityFamily` rows store the erased `QuantityInfo` and read `Info.BaseUnitInfo.Value` once at static construction — canonical/display units are metadata reads, never hardcoded enum literals; `IQuantity.Equals(other, tolerance)` feeds the per-row tolerance column.
- `UnitMath` (`Sum`/`Min`/`Max`/`Average`/`Clamp`/`Abs` at a chosen unit) is the ONLY generic-math aggregation owner — `GenericMathExtensions`/`DecimalGenericMathExtensions` do not exist in 5.75; for per-element `+`/`-`/`*`/`/`/`Zero` use the `IArithmeticQuantity` operators directly. `UnitFormatter` is `internal` (the public formatter is `QuantityFormatter`) and `IDecimalQuantity` is `[Obsolete]` (use `IValueQuantity<decimal>`).
- abbreviation resolution runs through `UnitParser.Default.TryParse(string, Type, IFormatProvider, out Enum)`, never a `"1 {unit}"` probe-string parse; `UnitsNetSetup.Default` is the single setup root composed once.

## [03]-[IMPLEMENTATION_LAW]


[STACK_THINKTECTURE]:
- `QuantityFamily` is a `[SmartEnum<string>]` (Thinktecture.Runtime.Extensions) whose rows store the erased `QuantityInfo` and read `BaseUnitInfo.Value` once at static construction; the smart-enum key policy `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]` rides `StringComparer.OrdinalIgnoreCase` so the family key is the canonical name.
- `DimensionMonomial` is a Thinktecture `[ValueObject]` over the Q⁷ `Seq<ERational>` exponent vector (PeterO.Numbers exact rationals; `ERational` compares with `.Equals`, never `==`) lifting the `BaseDimensions` seven `int` exponents so the symbolic `Powf` arm carries a non-integer exponent UnitsNet cannot; structural value equality is generated.
- Serialization stacks onto `Thinktecture.Runtime.Extensions.Json`: NO UnitsNet type crosses a JSON or proto wire — `UnitEvidence` projects to plain `string`/`double` fields, and the Thinktecture JSON context serializes the `[SmartEnum]`/`[ValueObject]` wrappers. Conversion-at-admission is what enforces the UnitsNet-serialization SKIP.

[STACK_LANGUAGEEXT]:
- Every admission returns `Fin<UnitEvidence>` (LanguageExt.Core): `Quantity.TryParse`/`TryFrom`/`UnitParser.TryParse` `bool`+`out` boundary calls lift into the `Fin` rail, and a parse/family/dimension failure mints `ComputeFault` on the 2200 band rather than throwing.
- The dimensional proof stacks onto `Validation<Error, DimensionMonomial>`: the applicative `Apply`/`Traverse` accumulates EVERY compound mismatch across a symbolic tree in one pass — `Error` is the monoidal failure carrier (`ComputeFault` is not its own monoid, so each typed `DimensionMismatch` lifts onto `Error` through its `Expected` base), the UnitsNet `BaseDimensions.Equals` is the leaf predicate, and the LanguageExt applicative is the accumulation algebra.
- Sequence aggregation uses `Seq`/`Arr` carriers; `UnitMetadata.ConvertTargets` maps `info.UnitInfos.ToSeq()` to its `.Value` projection.

[STACK_ANGOURIMATH]:
- The `Symbolic/dimensional` arm folds the AngouriMath `Entity` tree (positional-pattern `Descend` over `Sumf`/`Mulf`/`Divf`/`Powf`/… node records) onto the Q⁷ `Seq<ERational>` monomial, then the `UNITS_BRIDGE` matches the proven monomial against `QuantityFamily.Items` lifted through the seven `BaseDimensions` `int` exponents (`ERational.FromInt32`) — UnitsNet supplies the named-family target vocabulary, AngouriMath supplies the symbolic tree, and the bridge is the single seam. `sqrt` lowers to `Powf(arg, 1/2)`, which the exact-rational monomial scales — the reason the monomial is `ERational` and not the `int`-exponent `BaseDimensions`.

[CONVERSION_POLICY]:
- Conversion runs EXACTLY ONCE at admission (`As(canonical)`); a quantity type in an interior signature or on a wire is the seam violation the boundary deletes.
- Numeric-only conversion that crosses no quantity type rides `UnitConverter.TryConvert` (non-throwing, constructs no `IQuantity`).
- Canonical and display units source from `QuantityInfo.BaseUnitInfo.Value` and an explicit per-row display `Enum`; the generated structs emit no `DefaultUnitAttribute`/`DisplayAsUnitAttribute`, so attribute reflection over them resolves nothing.
- Abbreviation resolution runs through `UnitParser.Default.TryParse(string, Type, IFormatProvider, out Enum)`, never a `"1 {unit}"` probe-string parse.
- `UnitsNetSetup.Default` is the single setup root composed once; a second ambient setup is rejected.
- Affine families (`Temperature`) carry an offset — sum/aggregate at the canonical absolute scale, never across display offsets.

[RAIL_LAW]:
- Package: `UnitsNet` (MIT-0)
- Owns: source-generated quantity/unit algebra, the SI dimension vector, culture-scoped parse/format, and conversion/abbreviation policy
- Accept: measured unit-aware execution input at the boundary, canonicalized once to an SI raw double
- Reject: raw numeric unit comments; a UnitsNet type on an interior signature or a wire; `GenericMathExtensions`/`DecimalGenericMathExtensions`/`UnitFormatter`/`IDecimalQuantity` (the first two do not exist, the third is internal, the fourth is `[Obsolete]`)
