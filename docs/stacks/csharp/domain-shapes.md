# [DOMAIN_SHAPES]

Generated domain shapes own value admission, bounded vocabularies, closed variants, generated factories, generated validation, equality, comparison, and dispatch. Thinktecture provides the generated surface; repository analyzer policy may add local source-generation rules around that surface.

LanguageExt carries generated validation results after admission. Package graph and global using truth route to [build and packages](platform/build-and-packages.md).

## [1][SHAPE_SELECTION]

Choose the shape from modeling pressure, not from package inventory.

| [INDEX] | [PRESSURE]                    | [SHAPE]                     |
| :-----: | :---------------------------- | :-------------------------- |
|   [1]   | one scalar with admission     | `[ValueObject<T>]`          |
|   [2]   | composite identity            | `[ComplexValueObject]`      |
|   [3]   | bounded vocabulary            | `[SmartEnum<TKey>]`         |
|   [4]   | closed variants               | `[Union]`                   |
|   [5]   | plain structural carrier      | `record` or `record struct` |
|   [6]   | generic or stack-only variant | manual owner                |
|   [7]   | external wire shape           | boundary DTO                |

Use generated factories, generated equality, generated case access, and generated `Switch` or `Map` directly. Do not place dictionaries, helper factories, wrapper dispatch, generated case aliases, or duplicate DTOs beside generated owners.

## [2][VALUE_ADMISSION]

Scalar value object:
    Shape: `[ValueObject<T>]`.
    Use: one primitive value with admission, equality, comparison, parsing, or operator policy.
    Gate: use generated `Create`, `TryCreate`, `Validate`, and validation hooks directly.
    Comparer: put key comparison policy on `[KeyMemberEqualityComparer]` or `[KeyMemberComparer]`.

Composite value object:
    Shape: `[ComplexValueObject]`.
    Use: multi-field identity with generated construction and equality.
    Gate: use `partial class` or `partial struct`.
    Reject: records, record structs, positional record parameters, and `{ get; init; }` properties for generated identity.
    Constraint: expose get-only properties and let the generator create private construction.

Validation hook:
    Shape: generated validation partials with `ValidationError` or `ValidationError<TCustom>`.
    Signature: `ValidateFactoryArguments(ref ValidationError?, ref T1, ref T2, ...)` with `ref` parameters named as camelCase property names.
    Bridge: convert generated success or failure once into `Fin<T>` or `Validation<Error,T>`.
    Reject: helper factories that only rename generated factories.

Boundary admission:
    Owner: `IObjectFactory<TVO, TValue, ValidationError>` and source-owned bridge patterns when the boundary requires them.
    Route-away: host validity, numeric diagnostics, and rail transport stay in their owners.

Default structs:
    Rule: default struct value objects are rejected unless zero-init is a true domain value.
    Gate: `AllowDefaultStructs = true` only when the invariant proves zero-init validity.
    Struct layout: multi-field struct value objects carry explicit layout only when analyzer or runtime semantics require it.

Complex ordering:
    Rule: generated ordering operators belong to `[ValueObject<T>]`, not `[ComplexValueObject]`.
    Gate: write an owner-local comparison only when multi-field ordering is a domain operation.

## [3][VOCABULARIES_AND_VARIANTS]

Smart enum:
    Shape: `[SmartEnum<TKey>]`.
    Use: bounded cases with behavior, generated item lookup, stable keys, parsing, and delegate-backed members.
    Surface: use `Items`, `Get`, `TryGet`, and `Parse` directly.
    Replace: native enum plus parallel dictionaries.

Delegate-backed behavior:
    Owner: `[UseDelegateFromConstructor]` partial members on the smart enum or generated owner.
    Use: case-local behavior where the generated entry carries the operation.
    Allow: private static item factories only when they compress repeated case initializers inside the same owner.
    Reject: switch tables or dictionaries beside the owner.

Equality and comparison:
    Owner: generated key/member comparers such as `[KeyMemberEqualityComparer]`, `[KeyMemberComparer]`, and member comparer attributes.
    Reject: hand-written comparer classes beside smart-enum or value-object owners.

Union:
    Shape: `[Union]`.
    Use: closed variants where generated `Switch` or `Map` removes repeated dispatch arms.
    Gate: keep case payloads named and domain-owned.
    Default: generated dispatch is exhaustive.

Union overloads:
    Owner: `SwitchMethods`, `MapMethods`, and `[UnionSwitchMapOverload]`.
    Use: partial overloads only when the owner deliberately stops dispatch at a named case boundary.
    Shared context: use `SwitchMapStateParameterName` instead of duplicating context in every case payload.

Ad-hoc union:
    Use: narrow local alternatives only.
    Reject: public domain variants hidden inside type-list or ad-hoc unions.

Manual variant owner:
    Use: generic state, stack-only values, or open-ended state that generated union cases cannot express.
    Gate: generic `[Union]` can propagate `allows ref struct` constraints into consumers; keep those owners manual with total dispatch.

## [4][PROJECT_GENERATION_POLICY]

`GenerateUnionOps` and `SkipUnionOps` are repository-local policy attributes for generated union operation shape. They are not Thinktecture package options.

`GenerateUnionOps` emits a per-case `SelfOp` for generated operation routing. `SkipUnionOps` is the explicit opt-out when the operation belongs elsewhere or generated dispatch would obscure the owner.

Analyzer rules around generated dispatch, manual closed unions, closed-union plan fusion, generated case aliases, and decorative generated shapes are architecture feedback. Fix the shape first; refine the analyzer only when current source proves a false positive.

## [5][BOUNDARIES]

Rail bridge:
    Rule: generated validation admits or rejects raw values; LanguageExt carries the failure.
    Reject: exporting generated validation exceptions through domain code.

External protocol:
    Rule: create DTOs only when the external protocol demands a different wire shape.
    Gate: keep boundary translation at the boundary.

Serialization:
    Rule: keep serialization policy explicit at the boundary.
Gate: integration packages are inactive until package graph and an accepted owner route prove adoption.

Factory hiding:
    Option: `SkipFactoryMethods`.
    Use: only when a stronger domain owner deliberately hides the generated factory surface.
    Reject: helper-only replacement factories.

Generated storage:
    Option: `UseSingleBackingField`.
    Use: only when memory shape or generated semantics require one backing field.
    Gate: keep the choice with the union owner.

Conversion operators:
    Use: public boundary ergonomics only.
    Gate: keep domain admission explicit.

Partial generated splits:
    Use: generator-owned declarations and implementations.
    Reject: arbitrary file fragmentation.
