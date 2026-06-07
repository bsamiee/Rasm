# [DOMAIN_SHAPES]

Generated domain shapes own admission, identity, bounded vocabulary, closed variants, generated factories, validation partials, equality, comparison, and dispatch. Thinktecture is the normal implementation surface for those concerns; LanguageExt carries admitted values and failures after the generated owner has accepted or rejected raw input.

## [1][SHAPE_CHOOSER]

Choose the owner from modeling pressure. The package is already admitted; the question is which shape owns the invariant.

| [INDEX] | [PRESSURE]               | [OWNER]                    | [REJECT]                 |
| :-----: | :----------------------- | :------------------------- | :----------------------- |
|   [1]   | scalar admission         | `[ValueObject<T>]`         | raw primitive            |
|   [2]   | composite identity       | `[ComplexValueObject]`     | positional record        |
|   [3]   | bounded behavior         | `[SmartEnum<TKey>]`        | enum plus dictionary     |
|   [4]   | closed payload variants  | `[Union]`                  | nullable payload bag     |
|   [5]   | stack-confined workspace | manual owner               | heap wrapper             |
|   [6]   | invariant collection     | validated owner            | naked `Seq<T>` contract  |
|   [7]   | lifecycle aggregate      | record aggregate           | mutable property bag     |
|   [8]   | external wire contract   | boundary DTO               | duplicate domain carrier |
|   [9]   | plain data carrier       | `record` or `record struct` | generated decoration     |

Generated owners are the default when the generator can express the invariant. Manual owners are reserved for generator-proven unsupported shape, stack-only or open-ended state, or total dispatch that the generated surface cannot express. Generic unions are allowed when the generator supports the shape.

## [2][ADMISSION_STACK]

Admission is a stack, not a wrapper family. Each layer owns one decision and hands the next layer a stronger value.

Raw candidate:
    Owner: boundary adapter.
    Job: trim, parse, or project foreign shape only when the protocol requires it.
    Reject: carrying raw strings, numbers, nulls, or sentinel values into domain signatures.

Generated factory:
    Owner: `[ValueObject<T>]`, `[ComplexValueObject]`, or `[SmartEnum<TKey>]`.
    Job: expose generated `Create`, `TryCreate`, `Validate`, item lookup, and parsing directly.
    Reject: helper factories that only rename generated members.

Validation partial:
    Owner: generated partial hook.
    Signature: `ValidateFactoryArguments(ref ValidationError?, ref T1, ref T2, ...)` with camelCase `ref` parameters matching generated property names.
    Custom fault: use `[ValidationError<TFault>]` when the generated failure type must already be the domain fault.
    Reject: post-construction validation that lets invalid values exist.

Identity policy:
    Owner: generated key or member comparer attributes.
    Use: `[KeyMemberEqualityComparer]`, `[KeyMemberComparer]`, member comparers, and declared string comparison policy when identity has non-default comparison.
    Reject: hand-written comparer classes beside generated owners.

Rail bridge:
    Owner: boundary adapter or one declared bridge when several boundary value types share the same projection.
    Shape: generated success or failure converts once into `Fin<T>` or `Validation<Error,T>`.
    Reject: both a generated factory wrapper and a second `Validate` wrapper for the same value.

## [3][GENERATED_OWNERS]

Scalar value object:
    Shape: `[ValueObject<T>]`.
    Use: one primitive representation with admission, normalization, equality, comparison, parsing, or operator policy.
    Gate: keep `Create` for trusted construction and `TryCreate` or `Validate` for untrusted ingress.
    Default structs: allow zero-init only when zero is a real domain value.
    Reject: public raw primitive signatures after the value object exists.

Complex value object:
    Shape: `[ComplexValueObject]`.
    Use: multi-field identity with generated construction and equality.
    Gate: use a partial class or partial struct with get-only properties.
    Ordering: write owner-local comparison only when multi-field ordering is a domain operation.
    Reject: records, record structs, positional record parameters, and `{ get; init; }` identity fields.

Smart enum:
    Shape: `[SmartEnum<TKey>]`.
    Use: bounded cases with stable keys, lookup, parsing, generated item sets, and case-local behavior.
    Behavior: put `[UseDelegateFromConstructor]` members on the smart enum when each case owns the operation.
    Allow: private static item factories only when they compress repeated case initializers inside the same owner.
    Reject: native enum plus parallel dictionaries, switch tables, or behavior services.

Union:
    Shape: `[Union]`.
    Use: closed variants with named payloads and generated `Switch` or `Map` dispatch.
    Result rule: dispatch arms must unify to one exact result type, including generic parameters, before composing into rails.
    Context rule: use `SwitchMapStateParameterName` or generated state-threaded overloads when 3 or more arms share context.
    Reject: optional payload bags, repeated switch arms, generated case aliases, and helper dispatch.

Ad-hoc union:
    Shape: type-list union.
    Use: narrow adapter alternatives where foreign input arrives in several raw representations.
    Gate: project into the domain owner before domain logic.
    Reject: public domain variants hidden inside ad-hoc unions.

Manual variant owner:
    Shape: sealed manual owner with total dispatch.
    Use: generator-proven unsupported shape, stack-only state, open-ended state, or constraints the generated union cannot express.
    Gate: document the semantic reason in the owner, then keep dispatch total.
    Reject: manual unions that only reproduce generated `Switch` or `Map`.

Boundary DTO:
    Shape: protocol-owned carrier.
    Use: external wire shape, host API shape, or serialization contract that differs from the domain owner.
    Gate: translate at the boundary and keep the domain owner canonical.
    Reject: duplicate DTOs beside generated owners when no external protocol demands them.

## [4][DISPATCH_POLICY]

Generated dispatch is the behavior owner when a closed vocabulary or variant controls execution. Add behavior to the generated owner before adding another service, helper, dictionary, or wrapper.

State-threaded dispatch:
    Use: pass shared context through generated state overloads when repeated arms capture the same value.
    Result: branch lambdas stay static and return the same rail or value type.
    Reject: capturing the same context in every branch.

Smart-enum row behavior:
    Use: constructor delegates or generated members when each case has behavior.
    Result: adding a case forces its behavior row to be supplied with the case.
    Reject: external switch tables, lookup dictionaries, and case-name branching.

Union operation routing:
    Use: generated `Switch`, `Map`, and selected overloads for total case dispatch.
    Boundary stop: partial overloads are allowed only when the owner deliberately stops dispatch at a named case boundary.
    Reject: case aliases that only forward to generated construction.

Algebraic operators:
    Use: owner-local operators only when the type has explicit algebraic laws.
    Gate: generated unions do not imply domain `+` or `|`.
    Reject: operator sugar that only hides dispatch.

## [5][LOCAL_GENERATION_POLICY]

`GenerateUnionOps` and `SkipUnionOps` are repository-local policy attributes for generated union operation shape. They are not Thinktecture package options.

GenerateUnionOps:
    Use: the union owns generated per-case operation routing.
    Effect: emit the owner-local operation surface for each case.
    Reject: separate operation tables that drift from generated cases.

SkipUnionOps:
    Use: generated operation routing belongs elsewhere, or generated dispatch would obscure the owner.
    Effect: opt out explicitly so the absence is intentional.
    Reject: unqualified generated unions in functional surfaces.

Analyzer feedback:
    Rule: analyzer findings around generated dispatch, generated case aliases, manual closed unions, closed-union plan fusion, passive sibling surfaces, and decorative generated shapes are architecture pressure.
    Action: fix the domain shape first.
    Exception: refine the analyzer only when current source proves a false positive.

## [6][BOUNDARIES]

Rail boundary:
    Rule: generated validation admits or rejects raw values; LanguageExt transports the admitted value or typed failure.
    Reject: generated validation exceptions or raw generated failures leaking through domain code.

Serialization:
    Rule: keep serialization policy explicit at the boundary.
    Gate: integration packages stay inactive until package graph and an accepted owner route prove adoption.
    Reject: serializer-shaped domain owners.

Factory hiding:
    Option: `SkipFactoryMethods`.
    Use: only when a stronger owner deliberately hides generated factory surface behind an algebraic or rail-returning constructor.
    Reject: helper-only replacement factories.

Generated storage:
    Option: `UseSingleBackingField`.
    Use: only when memory shape or generated semantics require one backing field.
    Gate: keep the choice with the generated owner.

Conversion operators:
    Use: public boundary ergonomics only.
    Gate: domain admission remains explicit.
    Reject: implicit conversion that bypasses validation.

Partial generated splits:
    Use: generator-owned declarations, implementations, validation hooks, and behavior members.
    Reject: arbitrary file fragmentation to make generated owners look smaller.

## [7][VALIDATION]

- [ ] The shape follows modeling pressure, not package inventory.
- [ ] Generated factories, validation partials, equality, comparison, and dispatch are used directly.
- [ ] Manual owners are limited to generator-proven unsupported shape, stack-only state, open-ended state, or total dispatch the generator cannot express.
- [ ] Generic unions are not rejected merely because they are generic.
- [ ] Rail conversion happens once after generated admission.
- [ ] Smart-enum behavior and union dispatch stay on the generated owner.
- [ ] Package versions, graph state, globals, and integration-package adoption stay out of generated-shape policy.
