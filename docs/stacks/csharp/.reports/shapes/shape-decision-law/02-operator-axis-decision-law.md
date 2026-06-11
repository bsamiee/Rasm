# Operator Axis Decision Law

[AXES_AS_GROUP_STRUCTURE]:
- The six per-owner operator axes are a per-operation grant set, never an "is it a number" toggle, and the enabled set is a statement of the quantity's group structure: the axes that are on are exactly the binary operations under which the quantity is closed. A like-quantity owner grants addition and subtraction and holds multiply and divide at none, because a like-quantity product changes dimension — and no axis can express a dimension-changing operation, since every generated operator is homogeneous owner-op-owner or key-typed owner-op-key returning the owner. Cross-dimension operators are hand-written against the foreign result type; they are the part of the quantity's algebra that leaves the type.

```csharp
[ValueObject<decimal>(
    AdditionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    SubtractionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    MultiplyOperators = OperatorsGeneration.None,
    DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Meters;

[ValueObject<decimal>(MultiplyOperators = OperatorsGeneration.None)]
public readonly partial struct SquareMeters;

static SquareMeters Area(Meters width, Meters height) =>
    SquareMeters.Create((decimal)width * (decimal)height);

static TQuantity Total<TQuantity>(IEnumerable<TQuantity> parts, TQuantity zero)
    where TQuantity : IAdditionOperators<TQuantity, TQuantity, TQuantity> =>
    parts.Aggregate(zero, static (acc, next) => acc + next);
```

[CHECKED_DUALITY_AS_SUBSTITUTABILITY]:
- Every granted arithmetic axis emits both the unchecked operator and its checked counterpart when the key exposes both, so the owner inherits the platform's two-mode arithmetic contract whole: overflow-trapping is a property of the call site's checked context, never a second owner type and never a flag on the value. The duality is what makes a quantity owner a sound substitute for its key under checked-disciplined accumulation — a generic accumulator run inside a checked region traps overflow at the quantity boundary, so wrapping a raw key in evidence does not weaken the overflow guarantees the key had. An evidence wrapper that lost the checked mode would silently downgrade every checked algorithm it flows through; the generated pairing forecloses that downgrade.

[MONOTONE_AXIS_LATTICE]:
- The comparison and equality-comparison axes form one monotone chain, not two free knobs: comparison strength can only pull equality strength up to match, the read value of the equality axis is the join of the two settings, and the only route to suppressing equality operators is suppressing the owner's whole equality stance — which forces both axes to none as a single move. The lattice point "ordered but not equatable" is inexpressible by construction, mirroring the interface inheritance rather than re-checking it; identity and order are revoked together or not at all.
- The whole operator surface is downstream of admission: factory suppression forces every arithmetic axis to none regardless of declared values, because the operator bodies re-admit their results through the factory. An axis declared on a factory-suppressed owner is inert in the same silent way as an axis declared over a key without the operation — both are latent lies about the owner's algebra, and both read as capability the type does not have.

[PARTIAL_NUMBER_CITIZENSHIP]:
- The grantable axes are exactly the binary additive and multiplicative families plus the two relational interfaces; there is no axis for unary negation, modulus, increment, or the aggregate number interface bundling parsing, radix, and identity constants. The partiality is the correctness property, not a generator gap: a dimensioned quantity that advertised full number-hood would invite unit constants, absolute values, and radix operations that are nonsense on it, and the absent identity constants are the type-level enforcement that admitted values have no unadmitted siblings — not even algebraic units.
- A generic algorithm names the exact algebra it requires — the addition interface for a sum, the comparison interface for a clamp, both for a running min-with-accumulation — and the constraint solver checks the intersection against what the owner grants: a quantity used outside its group is a compile error at the call, never a representation bug at runtime. An algorithm written against the aggregate number constraint excludes every owner by construction; relaxing it to the exact binary families it uses is the owner-compatible spelling, with every seed and bound entering as an admitted parameter.
- Reading an owner's generated axis set against its hand-written operators recovers the full algebraic signature of the quantity: the generated axes carry the operations that are closed and homogeneous, the manual operators carry the operations that cross dimension or arity, and the axes held at none declare the operations the quantity forbids. The declaration is the algebra, recoverable without reading a single method body.
